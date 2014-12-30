using System;
using System.Collections.Generic;
using System.Linq;

namespace MessageQueuer.Core
{
    internal class TypeLocator
    {
        private readonly Dictionary<Type, object> _cache = new Dictionary<Type, object>();

        public IEnumerable<TypeResult> Locate<T>() where T : new()
        {
            if (_cache.ContainsKey(typeof (T)))
                return (List<TypeResult>) _cache[typeof (T)];

            var results = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                from type in assembly.GetTypes()
                let attributes = type.GetCustomAttributes(false)
                where attributes != null && attributes.Length > 0 && attributes.Any(x=> x.GetType() == typeof(T))
                select new TypeResult
                {
                    Type = type,
                    Attributes = attributes
                }).ToList();

            _cache.Add(typeof (T), results);

            return results;
        }
    }
}