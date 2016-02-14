﻿using Assignments.Core.Handlers;
using Microsoft.Diagnostics.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Consumer.Global
{
    class DumpFileTest
    {
        const string SOME_86_DUMP = @"C:\temp\dumps\tha-dump4.dmp";
        const int PID_NOT_FOUND = -1;
        const int ATTACH_TO_PPROCESS_TIMEOUT = 999999;

        public static void Run()
        {
            using (DataTarget target = DataTarget.LoadCrashDump(SOME_86_DUMP))
            {
                DoAnaytics(target, SOME_86_DUMP);
            }

            Console.ReadKey();
        }

        private static void DoAnaytics(DataTarget target, string pathToDump)
        {
            var runtime = target.ClrVersions[0].CreateRuntime();

            //Dump process handler
            ThreadStackHandler handler = new ThreadStackHandler(target.DebuggerInterface, runtime, pathToDump);

            var result = handler.Handle();

            PrintHandler.Print(result, true);
            Console.ReadKey();
        }

        private static int GetPidFromDumpProcessTextFile()
        {
            int pid = PID_NOT_FOUND;

            var fileContent = File.ReadAllText(@"./../../../dump_pid.txt");
            if (!String.IsNullOrEmpty(fileContent))
            {
                var success = int.TryParse(fileContent, out pid);
            }

            return pid;
        }

     

     
       
    }
}
