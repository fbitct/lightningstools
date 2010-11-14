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
            obj = o;
            propInfo = o.GetType().GetProperty(PropertyName);
        }

        public T Property
        {
            get
            {
                return (T)propInfo.GetValue(obj, null);
            }

            set
            {
                propInfo.SetValue(obj, value, null);
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
