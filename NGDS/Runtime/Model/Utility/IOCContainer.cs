using System;
using System.Collections.Generic;
namespace Kurisu.NGDS
{
    public class IOCContainer
    {
        private static readonly Dictionary<Type, object> Instances = new();
        public static void Register<T>(T instance)
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
        public static void UnRegister<T>(T instance)
        {
            var type = typeof(T);
            if (Instances.ContainsKey(type) && Instances[type].Equals(instance))
            {
                Instances.Remove(type);
            }
        }
        public static T Resolve<T>() where T : class
        {
            var type = typeof(T);
            if (Instances.TryGetValue(type, out object obj))
            {
                return obj as T;
            }
            return null;
        }
    }
}
