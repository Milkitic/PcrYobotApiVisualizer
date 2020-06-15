using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace YobotChart
{
    internal static class SingletonPageHelper
    {

        private static Dictionary<Type, Page> _dic = new Dictionary<Type, Page>();

        public static T Get<T>() where T : Page, new()
        {
            return GetInstance<T>();
        }

        private static T GetInstance<T>() where T : Page, new()
        {
            var type = typeof(T);
            if (!_dic.ContainsKey(type))
            {
                _dic.Add(type, new T());
            }

            return (T)_dic[type];
        }
    }
}