﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assignments.Core.Model.Unified;
using Assignments.Core.Model.Unified.Thread;
using Assignments.Core.msos;
using Microsoft.Diagnostics.Runtime;
using Assignments.Core.Model.WCT;
using Assignments.Core.Exceptions;

namespace Assignments.Core.Handlers.StackAnalysis.Strategies
{
    public class BlockingObjectsFetcherLiveProcessStrategy : BlockingObjectsFetcherStrategy
    {
        public BlockingObjectsFetcherLiveProcessStrategy(int pid) 
        {
            _wctApi = new WctApiHandler();
        }

        WctApiHandler _wctApi;


        public override List<UnifiedBlockingObject> GetUnmanagedBlockingObjects(ThreadInfo thread, List<UnifiedStackFrame> unmanagedStack)
        {
            return GetWCTBlockingObject(thread.OSThreadId);
        }

        private List<UnifiedBlockingObject> GetWCTBlockingObject(uint threadId)
        {
            List<UnifiedBlockingObject> result = null;

            ThreadWCTInfo wct_threadInfo = null;
            if (_wctApi.GetBlockingObjects(threadId, out wct_threadInfo))
            {
                result = new List<UnifiedBlockingObject>();

                if (wct_threadInfo.WctBlockingObjects?.Count > 0)
                {
                    foreach (var blockingObj in wct_threadInfo.WctBlockingObjects)
                    {
                        result.Add(new UnifiedBlockingObject(blockingObj));
                    }
                }
            }

            return result;
        }
    }
}
