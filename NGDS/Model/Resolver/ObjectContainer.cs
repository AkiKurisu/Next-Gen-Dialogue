using System;
using System.Collections.Generic;
namespace Kurisu.NGDS
{
    public class ObjectContainer : IObjectResolver
    {
        private readonly Dictionary<Type, object> Instances = new();
        public void Register<T>(T instance)
        {
            var type = typeof(T);
            if (Instances.ContainsKey(type))
            {
                Instances[type] = instance;
            }
            else
            {
                Instances.Add(type, instance);
            }
        }
        public void UnRegister<T>(T instance)
        {
            var type = typeof(T);
            if (Instances.ContainsKey(type) && Instances[type].Equals(instance))
            {
                Instances.Remove(type);
            }
        }
        public T Resolve<T>()
        {
            var type = typeof(T);
            if (Instances.TryGetValue(type, out object obj))
            {
                return (T)obj;
            }
            return default;
        }
    }
}
