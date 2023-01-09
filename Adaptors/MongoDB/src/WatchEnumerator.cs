// This file is part of the ArmoniK project
// 
// Copyright (C) ANEO, 2021-2022. All rights reserved.
//   W. Kirschenmann   <wkirschenmann@aneo.fr>
//   J. Gurhem         <jgurhem@aneo.fr>
//   D. Dubuc          <ddubuc@aneo.fr>
//   L. Ziane Khodja   <lzianekhodja@aneo.fr>
//   F. Lemaitre       <flemaitre@aneo.fr>
//   S. Djebbar        <sdjebbar@aneo.fr>
//   J. Fonseca        <jfonseca@aneo.fr>
//   D. Brasseur       <dbrasseur@aneo.fr>
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY, without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using MongoDB.Driver;

namespace ArmoniK.Core.Adapters.MongoDB;

/// <summary>
///   Class to convert a <see cref="IChangeStreamCursor{TInput}" /> into a <see cref="IAsyncEnumerator{TOutput}" />
/// </summary>
/// <typeparam name="TOutput">Output type</typeparam>
/// <typeparam name="TInput">Input type</typeparam>
public sealed class WatchEnumerator<TOutput, TInput> : IAsyncEnumerator<TOutput>
{
  private readonly CancellationToken           cancellationToken_;
  private readonly Func<TInput, TOutput>       converter_;
  private readonly IChangeStreamCursor<TInput> cursor_;
  private          IEnumerator<TInput>?        currentEnumerable_;

  /// <summary>
  ///   Initializes a <see cref="IAsyncEnumerator{TOutput}" /> from a <see cref="IChangeStreamCursor{TInput}" />
  ///   with a conversion function to transform <typeparamref name="TInput" /> into <typeparamref name="TOutput" />
  /// </summary>
  /// <param name="cursor">Input change stream cursor</param>
  /// <param name="converter">Func to convert the input into the output</param>
  /// <param name="cancellationToken">Token used to cancel the execution of the method</param>
  public WatchEnumerator(IChangeStreamCursor<TInput> cursor,
                         Func<TInput, TOutput>       converter,
                         CancellationToken           cancellationToken)
  {
    cursor_            = cursor;
    converter_         = converter;
    cancellationToken_ = cancellationToken;
  }

  /// <inheritdoc />
  public ValueTask<bool> MoveNextAsync()
  {
    if (cancellationToken_.IsCancellationRequested)
    {
      return new ValueTask<bool>(false);
    }

    if (currentEnumerable_ is not null)
    {
      if (currentEnumerable_.MoveNext())
      {
        return new ValueTask<bool>(true);
      }

      currentEnumerable_ = null;
    }

    while (currentEnumerable_ is null)
    {
      if (cancellationToken_.IsCancellationRequested)
      {
        return new ValueTask<bool>(false);
      }

      if (!cursor_.MoveNext(cancellationToken_))
      {
        return new ValueTask<bool>(false);
      }

      var enumerator = cursor_.Current.GetEnumerator();
      if (enumerator.MoveNext())
      {
        currentEnumerable_ = enumerator;
        return new ValueTask<bool>(true);
      }
    }

    return new ValueTask<bool>(false);
  }

  /// <inheritdoc />
  public TOutput Current
  {
    get
    {
      if (currentEnumerable_ is null)
      {
        throw new InvalidOperationException();
      }

      return converter_(currentEnumerable_!.Current);
    }
  }

  /// <inheritdoc />
  public ValueTask DisposeAsync()
  {
    currentEnumerable_?.Dispose();
    return ValueTask.CompletedTask;
  }
}
