using System;
using System.Collections.Generic;
using System.Linq;

namespace MessageQueuer.Core
{
    internal class TypeLocator
    {
        private readonly Dictionary<Type, object> _cache = new Dictionary<Type, object>();

        public List<TypeResult<T>> Locate<T>() where T : new()
        {
            if (_cache.ContainsKey(typeof (T)))
                return (List<TypeResult<T>>) _cache[typeof (T)];

            var results = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                from type in assembly.GetTypes()
                let attributes = type.GetCustomAttributes(typeof (T), true)
                where attributes != null && attributes.Length > 0
                select new TypeResult<T>
                {
                    Type = type,
                    Attributes = attributes.Cast<T>()
                }).ToList();

            _cache.Add(typeof (T), results);

            return results;
        }
    }
}