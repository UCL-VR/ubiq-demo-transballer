using System;
using System.Collections.Generic;
using System.Text;

namespace Ubik.Networking
{
    namespace JmBucknall.Structures
    {
        public interface IPriorityConverter<P>
        {
            int Convert(P priority);
            int PriorityCount { get; }
        }
    }
}