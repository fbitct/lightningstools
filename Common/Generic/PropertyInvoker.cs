using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
namespace Common.Generic
{
    public class PropertyInvoker<T>
    {
        private PropertyInfo propInfo;
        private object obj;

        public PropertyInvoker(string PropertyName, object o)
        {
            propInfo = o.GetType().GetProperty(PropertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.IgnoreCase);
            obj = o;
        }

        public T Property
        {
            get
            {
                return propInfo != null ? (T)propInfo.GetValue(obj, null) : default(T);
            }

            set
            {
                if (propInfo != null) propInfo.SetValue(obj, value,null);
            }
        }

        public T GetProperty()
        {
            return Property;
        }

        public void SetProperty(T value)
        {
            Property = value;
        }
    }
}
