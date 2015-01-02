using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MessageQueuer.Core
{
    internal class TypeLocator
    {
        private readonly Dictionary<Type, IEnumerable<TypeResult>> _cache = new Dictionary<Type, IEnumerable<TypeResult>>();

        private IList<Assembly> _assemblies;

        public IEnumerable<TypeResult> Locate<T>() where T : new()
        {
            if (_cache.ContainsKey(typeof(T)))
                return _cache[typeof(T)];

            var results = (from assembly in GetAssemblies()
                           from type in assembly.GetTypes()
                           let attributes = type.GetCustomAttributes(false)
                           where attributes != null && attributes.Length > 0 && attributes.Any(x => x.GetType() == typeof(T))
                           select new TypeResult
                           {
                               Type = type,
                               Attributes = attributes
                           }).ToList();

            _cache.Add(typeof(T), results);

            return results;
        }

        private IEnumerable<Assembly> GetAssemblies()
        {
            if (_assemblies != null)
                return _assemblies;

            _assemblies = new List<Assembly>();
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (path == null)
                throw new FileNotFoundException("Could not locate assemblies");

            foreach (var dll in Directory.GetFiles(path, "*.dll"))
                _assemblies.Add(Assembly.LoadFile(dll));

            return _assemblies;
        }
    }
}