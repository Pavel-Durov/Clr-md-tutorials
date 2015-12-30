﻿using System.Runtime.InteropServices;

namespace Assignments.Core.msos
{

    //Helpers
    class Util
    {
        public static void VerifyHr(int hr)
        {
            if (hr != 0)
                Marshal.ThrowExceptionForHR(hr);
        }
    }


}
