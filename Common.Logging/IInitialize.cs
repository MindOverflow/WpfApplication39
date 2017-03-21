using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asteros.AsterosContact.Common.Logging
{
    public interface IInitialize<TInit> where TInit : InitializationBase
    {
        TInit Initialization { get; }

        void Initialize(TInit init);
        void ApplyInitialization();
    }
}
