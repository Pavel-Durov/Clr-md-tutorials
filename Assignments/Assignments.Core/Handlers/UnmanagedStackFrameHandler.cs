﻿using System;
using System.Collections.Generic;
using Microsoft.Diagnostics.Runtime;
using Assignments.Core.Exceptions;
using Assignments.Core.Model.Unified;

namespace Assignments.Core.Handlers
{
    public class UnmanagedStackFrameWalker
    {
        public const string WAIT_FOR_SINGLE_OBJECTS_FUNCTION_NAME = "WaitForSingleObject";
        public const string WAIT_FOR_MULTIPLE_OBJECTS_FUNCTION_NAME = "WaitForMultipleObjects";
        public const string ENTER_CRITICAL_SECTION_FUNCTION_NAME = "EnterCriticalSection";


        const int WAIT_FOR_SINGLE_OBJECT_PARAM_COUNT = 2;
        const int WAIT_FOR_MULTIPLE_OBJECTS_PARAM_COUNT = 4;

        public static void SetParams(UnifiedStackFrame frame, ClrRuntime runtime)
        {
            List<byte[]> result = new List<byte[]>();

            if (CheckForWinApiCalls(frame, WAIT_FOR_SINGLE_OBJECTS_FUNCTION_NAME))
            {
                DealWithSingle(frame, runtime, result);
            }
            else if (CheckForWinApiCalls(frame, WAIT_FOR_MULTIPLE_OBJECTS_FUNCTION_NAME))
            {
                DealWithMultiple(frame, runtime, result);
            }
            else if (CheckForWinApiCalls(frame, ENTER_CRITICAL_SECTION_FUNCTION_NAME))
            {

            }
            frame.NativeParams = result;
        }

        private static bool CheckForCriticalSection(UnifiedStackFrame frame, ClrRuntime runtime, List<byte[]> result)
        {
            throw new NotImplementedException();
        }

        private static void DealWithSingle(UnifiedStackFrame frame, ClrRuntime runtime, List<byte[]> result)
        {
            result = GetNativeParams(frame, runtime, WAIT_FOR_SINGLE_OBJECT_PARAM_COUNT);
            frame.Handles = new List<UnifiedHandle>();
            frame.Handles.Add(new UnifiedHandle(Convert(result[0])));
        }

        private static void DealWithMultiple(UnifiedStackFrame frame, ClrRuntime runtime, List<byte[]> result)
        {
            result = GetNativeParams(frame, runtime, WAIT_FOR_MULTIPLE_OBJECTS_PARAM_COUNT);
            frame.Handles = new List<UnifiedHandle>();

            var HandlesCunt = BitConverter.ToUInt32(result[0], 0);
            var HandleAddress = BitConverter.ToUInt32(result[1], 0);

            var handles = ReadFromMemmory(HandleAddress, HandlesCunt, runtime);
            foreach (var handle in handles)
            {
                uint handleUint = Convert(handle);
                var typeName = NtQueryHandler.GetHandleType((IntPtr)handleUint);
                var handleName = NtQueryHandler.GetHandleObjectName((IntPtr)handleUint);

                UnifiedHandle unifiedHandle = new UnifiedHandle(handleUint, typeName, handleName);
                frame.Handles.Add(unifiedHandle);
            }
        }

        private static uint Convert(byte[] bits)
        {
            return BitConverter.ToUInt32(bits, 0);
        }

        public static bool CheckForWinApiCalls(UnifiedStackFrame c, string key)
        {
            bool result = c != null
                && !String.IsNullOrEmpty(c.Method)
                && c.Method != null && c.Method.Contains(key);

            return result;
        }

        public static List<byte[]> GetNativeParams(UnifiedStackFrame stackFrame, ClrRuntime runtime, int paramCount)
        {
            List<byte[]> result = new List<byte[]>();

            var offset = stackFrame.FrameOffset; //Base Pointer - % EBP
            byte[] paramBuffer;
            int bytesRead = 0;
            offset += 4;

            for (int i = 0; i < paramCount; i++)
            {
                paramBuffer = new byte[4];
                offset += (uint)IntPtr.Size;
                if (runtime.ReadMemory(offset, paramBuffer, 4, out bytesRead))
                {
                    result.Add(paramBuffer);
                }
            }

            return result;
        }

        public static List<byte[]> ReadFromMemmory(uint startAddress, uint count, ClrRuntime runtime)
        {
            List<byte[]> result = new List<byte[]>();
            int sum = 0;
            //TODO: Check if dfor can be inserted into the REadMemmory result (seems to be..)
            for (int i = 0; i < count; i++)
            {
                byte[] readedBytes = new byte[4];
                if (runtime.ReadMemory(startAddress, readedBytes, 4, out sum))
                {
                    result.Add(readedBytes);
                }
                else
                {
                    throw new AccessingNonReadableMemmory(string.Format("Accessing Unreadable memorry at {0}", startAddress));
                }
                //Advancing the pointer by 4 (32-bit system)
                startAddress += (uint)IntPtr.Size;
            }
            return result;
        }

    }
}
