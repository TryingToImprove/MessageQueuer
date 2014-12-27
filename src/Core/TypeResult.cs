using System;
using System.Collections.Generic;

namespace MessageQueuer.Core
{
    internal class TypeResult<T>
    {
        public Type Type { get; set; }

        public IEnumerable<T> Attributes { get; set; }
    }
}