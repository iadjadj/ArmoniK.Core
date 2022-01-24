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

using System.Threading;
using System.Threading.Tasks;

namespace ArmoniK.Core.Common.Storage;

public static class TableStorageExtensions
{
  public static async Task< DispatchHandler> AcquireDispatchHandler(this ITableStorage tableStorage,
                                                                    string             dispatchId,
                                                                    string             taskId,
                                                                    string             podId             = "",
                                                                    string             nodeId            = "",
                                                                    CancellationToken  cancellationToken = default)
  {
    var isAcquired = await tableStorage.TryAcquireDispatchAsync(dispatchId,
                                                                taskId,
                                                                podId,
                                                                nodeId,
                                                                cancellationToken);

    var dispatch = await tableStorage.GetDispatchAsync(dispatchId,
                                                       cancellationToken);
    if (isAcquired)
      return new DispatchHandler(dispatch,
                                 tableStorage,
                                 cancellationToken);

    return null;
  }
}
