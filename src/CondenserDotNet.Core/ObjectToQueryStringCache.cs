using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Net.Http;

namespace CondenserDotNet.Core
{
    public class ObjectToQueryStringCache
    {
        private Dictionary<Type, PropertyInfo[]> _properties = new Dictionary<Type, PropertyInfo[]>();

        public string GetQueryString(string uri, object obj)
        {
            var objType = obj.GetType();
            PropertyInfo[] props;
            lock(_properties)
            {
                if(!_properties.TryGetValue(objType, out props))
                {
                    props = objType.GetProperties();
                    _properties.Add(objType, props);
                }
            }
            var dictionary = new Dictionary<string,string>(props.Length);
            foreach(var p in props)
            {
                var propValue = p.GetValue(obj, null);
                if (propValue != null)
                {
                    dictionary.Add(p.Name, propValue.ToString());
                }
            }
            return Microsoft.AspNetCore.WebUtilities.QueryHelpers.AddQueryString(uri, dictionary);
        }
    }
}
