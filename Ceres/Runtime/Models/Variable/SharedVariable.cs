using UnityEngine;
using System;
namespace Ceres
{
    /// <summary>
    /// Variable can be shared between behaviors in behavior tree
    /// </summary>
    [Serializable]
    public abstract class SharedVariable : ICloneable
    {
        public SharedVariable()
        {

        }
        /// <summary>
        /// Whether variable is shared
        /// </summary>
        /// <value></value>
        public bool IsShared
        {
            get => isShared;
            set => isShared = value;
        }
        [SerializeField]
        private bool isShared;
        /// <summary>
        /// Whether variable is global
        /// </summary>
        /// <value></value>
        public bool IsGlobal
        {
            get => isGlobal;
            set => isGlobal = value;
        }
        [SerializeField]
        private bool isGlobal;
        /// <summary>
		/// Whether variable is exposed to editor
		/// </summary>
		/// <value></value>
		public bool IsExposed
        {
            get => isExposed;
            set => isExposed = value;
        }
        [SerializeField]
        private bool isExposed;
        public string Name
        {
            get
            {
                return mName;
            }
            set
            {
                mName = value;
            }
        }
        public abstract object GetValue();
        public abstract void SetValue(object value);
        /// <summary>
        /// Bind to other sharedVariable
        /// </summary>
        /// <param name="other"></param>
        public abstract void Bind(SharedVariable other);
        /// <summary>
        /// Unbind self
        /// </summary>
        public abstract void Unbind();
        /// <summary>
        /// Clone shared variable by deep copy, an option here is to override for preventing using reflection
        /// </summary>
        /// <returns></returns>
        public virtual SharedVariable Clone()
        {
            return ReflectionHelper.DeepCopy(this);
        }

        [SerializeField]
        private string mName;
        /// <summary>
        /// Create a observe proxy variable
        /// </summary>
        /// <returns></returns>
        public abstract ObserveProxyVariable Observe();

        object ICloneable.Clone()
        {
            return Clone();
        }
    }
    [Serializable]
    public abstract class SharedVariable<T> : SharedVariable, IBindableVariable<T>
    {
        public T Value
        {
            get
            {
                return (Getter == null) ? value : Getter();
            }
            set
            {
                if (Setter != null)
                {
                    Setter(value);
                }
                else
                {
                    this.value = value;
                }
            }
        }
        public sealed override object GetValue()
        {
            return Value;
        }
        public sealed override void SetValue(object value)
        {
            if (Setter != null)
            {
                Setter((T)value);
            }
            else if (value is IConvertible)
            {
                this.value = (T)Convert.ChangeType(value, typeof(T));
            }
            else
            {
                this.value = (T)value;
            }
        }
        protected Func<T> Getter;
        protected Action<T> Setter;
        public void Bind(IBindableVariable<T> other)
        {
            Getter = () => other.Value;
            Setter = (evt) => other.Value = evt;
        }
        public override void Bind(SharedVariable other)
        {
            if (other is IBindableVariable<T> variable)
            {
                Bind(variable);
            }
            else
            {
                Debug.LogError($"Variable named with {Name} bind failed!");
            }
        }
        public override void Unbind()
        {
            Getter = null;
            Setter = null;
        }
        [SerializeField]
        protected T value;
        public override ObserveProxyVariable Observe()
        {
            return ObserveT();
        }
        public ObserveProxyVariable<T> ObserveT()
        {
            Setter ??= (evt) => { value = evt; };
            var wrapper = new SetterWrapper<T>((w) => { Setter -= w.Invoke; });
            var proxy = new ObserveProxyVariable<T>(this, in wrapper);
            Setter += wrapper.Invoke;
            return proxy;
        }
        public sealed override SharedVariable Clone()
        {
            var variable = CloneT();
            variable.CopyProperty(this);
            return variable;
        }
        protected virtual SharedVariable<T> CloneT()
        {
            return ReflectionHelper.DeepCopy(this);
        }
        protected void CopyProperty(SharedVariable other)
        {
            IsGlobal = other.IsGlobal;
            IsExposed = other.IsExposed;
            IsShared = other.IsShared;
            Name = other.Name;
        }
    }
    public class SetterWrapper<T> : IDisposable
    {
        private readonly Action<SetterWrapper<T>> Unregister;
        public Action<T> Setter;
        public void Invoke(T value)
        {
            Setter(value);
        }
        public void Dispose()
        {
            Unregister(this);
        }
        public SetterWrapper(Action<SetterWrapper<T>> unRegister)
        {
            Unregister = unRegister;
        }
    }
    /// <summary>
    /// Proxy variable to observe value change
    /// </summary>
    public abstract class ObserveProxyVariable : IDisposable
    {
        public abstract void Dispose();
        /// <summary>
        /// Unbind self
        /// </summary>
        public abstract void Unbind();
        public abstract void Register(Action<object> observeCallBack);

    }
    public class ObserveProxyVariable<T> : ObserveProxyVariable, IBindableVariable<T>
    {
        public T Value
        {
            get
            {
                return Getter();
            }
            set
            {
                Setter(value);
            }
        }
        private Func<T> Getter;
        private Action<T> Setter;
        private readonly SetterWrapper<T> setterWrapper;
        public void Bind(IBindableVariable<T> other)
        {
            Getter = () => other.Value;
            Setter = (evt) => other.Value = evt;
        }
        public ObserveProxyVariable(SharedVariable<T> variable, in SetterWrapper<T> setterWrapper)
        {
            this.setterWrapper = setterWrapper;
            setterWrapper.Setter = Notify;
            Bind(variable);
        }
        public event Action<T> OnValueChange;
        private void Notify(T value)
        {
            OnValueChange?.Invoke(value);
        }
        public sealed override void Dispose()
        {
            setterWrapper.Dispose();
        }
        public override void Unbind()
        {
            Getter = null;
            Setter = null;
        }

        public override void Register(Action<object> observeCallBack)
        {
            OnValueChange += (x) => observeCallBack?.Invoke(x);
        }
    }
}