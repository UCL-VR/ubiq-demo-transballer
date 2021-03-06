using System;
using System.Collections.Generic;
using System.Text;

namespace Ubik.Networking
{
    namespace JmBucknall.Threading
    {
        public class Task<T>
        {
            private T priority;

            public Task(T priority)
            {
                this.priority = priority;
            }

            public T Priority
            {
                get { return priority; }
            }
        }
    }
}