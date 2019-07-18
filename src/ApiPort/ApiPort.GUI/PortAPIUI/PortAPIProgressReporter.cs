// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability;
using System;
using System.Collections.Generic;

namespace PortAPIUI
{
    internal class PortAPIProgressReporter : IProgressReporter
    {
        public IReadOnlyCollection<string> Issues => throw new NotImplementedException();

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void ReportIssue(string issue)
        {
            throw new NotImplementedException();
        }

        public void Resume()
        {
            throw new NotImplementedException();
        }

        public IProgressTask StartTask(string taskName, int totalUnits)
        {
            throw new NotImplementedException();
        }

        public IProgressTask StartTask(string taskName)
        {
            throw new NotImplementedException();
        }

        public void Suspend()
        {
            throw new NotImplementedException();
        }
    }
}
