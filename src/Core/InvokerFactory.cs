using System;
using System.Linq;

namespace MessageQueuer.Core
{
    internal class InvokerFactory
    {
        private readonly Func<Type, object> _resolver;

        public InvokerFactory(Func<Type, object> resolver)
        {
            _resolver = resolver;
        }

        public object Build(Type type)
        {
            var constructedType = type; //.MakeGenericType(type);

            var constructors = constructedType.GetConstructors();
            var constructor = constructors[0];
            var constructorParameters = constructor.GetParameters();

            if (constructorParameters.Length > 0 && _resolver == null)
                throw new ArgumentException("A resolver was not configured");

            return Activator.CreateInstance(constructedType,
                constructorParameters.Select(parameter => _resolver(parameter.ParameterType)).ToArray());
        }
    }
}