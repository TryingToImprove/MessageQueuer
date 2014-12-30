using System;
using System.Collections.Generic;

namespace MessageQueuer.Core
{
    internal class TypeResult
    {
        public Type Type { get; set; }

        public IEnumerable<Object> Attributes { get; set; }
    }
}