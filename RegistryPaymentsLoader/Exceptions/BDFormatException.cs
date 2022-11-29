using System;
using System.Collections.Generic;
using System.Text;

namespace RegistryPaymentsLoader
{
    public class BDFormatException: ApplicationException
    {
        public BDFormatException(string message): base(message)
        {

        }
    }
}
