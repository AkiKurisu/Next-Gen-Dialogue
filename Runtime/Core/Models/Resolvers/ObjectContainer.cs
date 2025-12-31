using System;
using System.Collections.Generic;

namespace NextGenDialogue
{
    public class ObjectContainer : IObjectResolver
    {
        private readonly Dictionary<Type, object> _instances = new();
        
        public void Register<T>(T instance)
        {
            var type = typeof(T);
            _instances[type] = instance;
        }
        
        public void UnRegister<T>(T instance)
        {
            var type = typeof(T);
            if (_instances.ContainsKey(type) && _instances[type].Equals(instance))
            {
                _instances.Remove(type);
            }
        }
        
        public T Resolve<T>()
        {
            var type = typeof(T);
            if (_instances.TryGetValue(type, out object obj))
            {
                return (T)obj;
            }
            return default;
        }
    }
}
