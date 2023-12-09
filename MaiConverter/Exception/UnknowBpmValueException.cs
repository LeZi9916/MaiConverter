using System;

namespace MaiConverter.Exception
{
    public class UnknowBpmValueException: System.Exception
    {
        public UnknowBpmValueException(string s):base(s)
        {
            
        }
    }
}