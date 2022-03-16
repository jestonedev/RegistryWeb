using System;
using System.Collections.Generic;
using System.Text;

namespace RegistryPaymentsLoader.TffFileCreators
{
    public abstract class TffFileCreator
    {
        public abstract string Version { get; }
    }
}
