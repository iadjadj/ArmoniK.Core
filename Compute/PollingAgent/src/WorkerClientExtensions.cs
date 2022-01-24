﻿// This file is part of the ArmoniK project
// 
// Copyright (C) ANEO, 2021-2022. All rights reserved.
//   W. Kirschenmann   <wkirschenmann@aneo.fr>
//   J. Gurhem         <jgurhem@aneo.fr>
//   D. Dubuc          <ddubuc@aneo.fr>
//   L. Ziane Khodja   <lzianekhodja@aneo.fr>
//   F. Lemaitre       <flemaitre@aneo.fr>
//   S. Djebbar        <sdjebbar@aneo.fr>
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using ArmoniK.Api.gRPC.V1;

using Google.Protobuf;

using Grpc.Core;

namespace ArmoniK.Core.Compute.PollingAgent;

public static class WorkerClientExtensions
{
  public static async IAsyncEnumerable<(ProcessReply First, IAsyncEnumerable<ProcessReply> Stream)> Separate(
    this                     IAsyncStreamReader<ProcessReply> stream,
    [EnumeratorCancellation] CancellationToken                cancellationToken)
  {
    var outputChannel      = Channel.CreateUnbounded<ProcessReply>();
    var replyType          = ProcessReply.TypeOneofCase.None;
    var requestId          = string.Empty;
    var isLastLargeRequest = false;


    await foreach (var reply in stream.ReadAllAsync(cancellationToken)
                                      .WithCancellation(cancellationToken))
    {
      if (replyType != ProcessReply.TypeOneofCase.None && reply.TypeCase != replyType)
      {
        throw new InvalidOperationException("Stream messages unexpectedly changed their types. Current implementation does not support stream multiplexing.");
      }

      if (string.IsNullOrEmpty(requestId))
      {
        outputChannel = Channel.CreateUnbounded<ProcessReply>();
        replyType     = reply.TypeCase;
        yield return (reply, outputChannel.Reader.ReadAllAsync(cancellationToken));
      }

      switch (replyType)
      {
        case ProcessReply.TypeOneofCase.Output:
          switch (reply.Output.TypeCase)
          {
            case ProcessReply.Types.Output.TypeOneofCase.Init when reply.Output.Init.ResultTypeCase == ProcessReply.Types.Output.Types.Init.ResultTypeOneofCase.Error:
            case ProcessReply.Types.Output.TypeOneofCase.Init
              when reply.Output.Init.ResultTypeCase == ProcessReply.Types.Output.Types.Init.ResultTypeOneofCase.OutputChunk &&
                   reply.Output.Init.OutputChunk.DataComplete:
            case ProcessReply.Types.Output.TypeOneofCase.Data when reply.Output.Data.DataComplete:
              CloseChannel();
              break;
            case ProcessReply.Types.Output.TypeOneofCase.None:
            case ProcessReply.Types.Output.TypeOneofCase.Init when reply.Output.Init.ResultTypeCase == ProcessReply.Types.Output.Types.Init.ResultTypeOneofCase.None:
            default:
              ThrowInChannel();
              break;
          }

          break;
        case ProcessReply.TypeOneofCase.Result:
          switch (reply.Result.TypeCase)
          {
            case ProcessReply.Types.Result.TypeOneofCase.Init when reply.Result.Init.ResultChunk.DataComplete:
            case ProcessReply.Types.Result.TypeOneofCase.Data when reply.Result.Data.DataComplete:
              CloseChannel();
              break;
            case ProcessReply.Types.Result.TypeOneofCase.None:
            default:
              ThrowInChannel();
              break;
          }

          break;
        case ProcessReply.TypeOneofCase.CreateLargeTask:
          switch (reply.CreateLargeTask.TypeCase)
          {
            case CreateLargeTaskRequest.TypeOneofCase.InitRequest:
              break;
            case CreateLargeTaskRequest.TypeOneofCase.InitTask when reply.CreateLargeTask.InitTask.PayloadComplete && reply.CreateLargeTask.InitTask.LastTask:
              CloseChannel();
              break;
            case CreateLargeTaskRequest.TypeOneofCase.InitTask when !reply.CreateLargeTask.InitTask.PayloadComplete && reply.CreateLargeTask.InitTask.LastTask:
              isLastLargeRequest = true;
              break;
            case CreateLargeTaskRequest.TypeOneofCase.TaskPayload when reply.CreateLargeTask.TaskPayload.PayloadComplete && isLastLargeRequest:
              CloseChannel();
              isLastLargeRequest = false;
              break;
            case CreateLargeTaskRequest.TypeOneofCase.None:
            default:
              ThrowInChannel();
              break;
          }

          break;
        case ProcessReply.TypeOneofCase.Resource:
        case ProcessReply.TypeOneofCase.CommonData:
        case ProcessReply.TypeOneofCase.DirectData:
        case ProcessReply.TypeOneofCase.CreateSmallTask:
          CloseChannel();
          break;
        case ProcessReply.TypeOneofCase.None:
        default:
          ThrowInChannel();
          break;
      }
    }

    void CloseChannel()
    {
      outputChannel.Writer.Complete();
      replyType = ProcessReply.TypeOneofCase.None;
      requestId = string.Empty;
    }

    void ThrowInChannel()
    {
      var error = new ArgumentOutOfRangeException(nameof(ProcessReply),
                                                  $"received either a \"None\" or an unknown reply type in the stream.");
      outputChannel.Writer.Complete(error);
      throw error;
    }
  }

  public static async IAsyncEnumerable<ProcessRequest.Types.DataReply> ToDataReply(this IAsyncEnumerable<byte[]> bytes,
                                                                                   string                        replyId,
                                                                                   string                        key,
                                                                                   [EnumeratorCancellation] CancellationToken            cancellationToken)
  {
    var enumerator = bytes.GetAsyncEnumerator(cancellationToken);
    if (!await enumerator.MoveNextAsync(cancellationToken))
    {
      throw new InvalidOperationException("No data were retrieved.");
    }

    var current = enumerator.Current;

    if (!await enumerator.MoveNextAsync())
    {
      yield return new()
                   {
                     ReplyId = replyId,
                     Init = new()
                            {
                              Key = key,
                              Data = new()
                                     {
                                       Data         = UnsafeByteOperations.UnsafeWrap(current),
                                       DataComplete = true,
                                     },
                            },
                   };
    }
    else
    {
      yield return new()
                   {
                     ReplyId = replyId,
                     Init = new()
                            {
                              Key = key,
                              Data = new()
                                     {
                                       Data         = UnsafeByteOperations.UnsafeWrap(current),
                                       DataComplete = false,
                                     },
                            },
                   };

      current = enumerator.Current;

      while (await enumerator.MoveNextAsync())
      {

        yield return new()
                     {
                       ReplyId = replyId,
                       Data = new()
                              {
                                Data         = UnsafeByteOperations.UnsafeWrap(current),
                                DataComplete = false,
                              },
                     };

        current = enumerator.Current;
      }

      yield return new()
                   {
                     ReplyId = replyId,
                     Data = new()
                            {
                              Data         = UnsafeByteOperations.UnsafeWrap(current),
                              DataComplete = true,
                            },
                   };
    }
  }

}
