using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp_NetCore
{
    public class WinError
    {
        public string Message;
        public string ErrorCode;
        public int HResult;
        public WinError() { }
        public WinError(string message, string errorCode, int hResult)
        {
            Message = message;
            ErrorCode = errorCode;
            HResult = hResult;
        }
    }
}
