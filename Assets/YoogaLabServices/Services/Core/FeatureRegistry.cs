using System;
using System.Collections.Generic;

namespace ServicesPackage
{
    public class FeatureRegistry
    {
        private readonly Dictionary<Type, object> _services = new();

        public void Register<T>(T service)
        {
            _services[typeof(T)] = service;
        }

        public T Resolve<T>()
        {
            return (T)_services[typeof(T)];
        }

        public bool TryResolve<T>(out T instance)
        {
            if (_services.TryGetValue(typeof(T), out var obj))
            {
                instance = (T)obj;
                return true;
            }
            instance = default;
            return false;
        }
    }

}
