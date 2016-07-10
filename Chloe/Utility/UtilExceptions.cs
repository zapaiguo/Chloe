using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chloe.Utility
{
    static class UtilExceptions
    {
        public static NullReferenceException NullReferenceException(string message = null)
        {
            if (message == null)
                return new NullReferenceException();

            return new NullReferenceException(message);
        }
        public static NotSupportedException NotSupportedException(string message = null)
        {
            if (message == null)
                return new NotSupportedException();

            return new NotSupportedException(message);
        }
    }
}
