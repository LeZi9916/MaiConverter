using System;

namespace MaiConverter.Exception
{
    public class UnknowMeterValueException: System.Exception
    {
        public UnknowMeterValueException(string s):base(s)
        {
            
        }
    }
}