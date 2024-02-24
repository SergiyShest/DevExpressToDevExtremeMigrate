using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DAL.Core
{
    public static class CoreHelper
    {
        /// <summary>
        /// Convert ID string to List(int)
        /// </summary>
        /// <param name="idsStr"></param>
        /// <returns></returns>
        public static List<int> ConvertIdsToArr(string idsStr)
        {
            List<int> ids = new List<int>();
            if (!string.IsNullOrWhiteSpace(idsStr))
            {
                var idsArr = idsStr.Split(',');
                foreach (var id in idsArr)
                {
                    if (int.TryParse(id, out int i))
                    {
                        if (ids.Contains(i))
                        {
                            continue;
                        }
                        ids.Add(i);
                    }
                }
            }
            return ids;
        }

        /// <summary>
        /// Convert ID string to List(int)
        /// </summary>
        /// <param name="idsNumber"></param>
        /// <returns></returns>
        public static List<decimal> ConvertToNumberArr(string idsNumber)
        {
            var numbers = new List<decimal>();
            if (!string.IsNullOrWhiteSpace(idsNumber))
            {
                var idsArr = idsNumber.Split(',');
                foreach (var id in idsArr)
                {
                    if (decimal.TryParse(id, out decimal i))
                    {

                        numbers.Add(i);
                    }
                }
            }
            return numbers;
        }

        public static DateTime? ParseRequestDate(string dt)
        {
            if (string.IsNullOrWhiteSpace(dt)) return null;
            dt = dt.Trim();
            CultureInfo enUS = new CultureInfo("en-US");
            DateTime dateValue;


            var formatStrings = new string[] {
                "MMM.dd.yyyy h:mm",
                "MMM.dd.yyyy hh:mm",
                "MM.dd.yyyy hh:mm",
                "MM.dd.yy hh:mm",
                "MM.dd.yy h:mm",
                "MM/dd/yyyy hh:mm:ss tt",
                "yyyy-MM-dd hh:mm:ss",
                "yyyy-MM-dd hh:mm",
                "yy-MM-dd hh:mm",
                 "yy-MM-dd",
                 "yyyy-MM-dd",
                 "yy/MM/dd",
                 "yyyy/MM/dd"
            };
            if (DateTime.TryParseExact(dt, formatStrings, enUS, DateTimeStyles.None, out dateValue))
                return dateValue;

            var strFormat = string.Join(Environment.NewLine, formatStrings);
            throw new Exception($"Don't know how to parse...\"{dt}\" to DateTime. {Environment.NewLine}Available format is:{Environment.NewLine}{strFormat}");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static string CamelStyleToName(string fieldName)
        {
            var value = Regex.Replace(fieldName, @"([a-z])([A-Z])", "$1 $2").ToLower();// вставляет пробел между словами и делает все маленьким
            value = value.ToString().Substring(0, 1).ToUpper() + value.ToString().Substring(1);// первая буква большая
            value = value.Replace("_", " ");
            return value;
        }

        public static string GetAllMessages(this Exception ex, string separator = "=>\r\n")
        {
            if (ex == null) throw new ArgumentNullException("ex");
            StringBuilder sb = new StringBuilder();
            while (ex != null)
            {
                //if (ex is DbEntityValidationException)
                //{
                //    foreach (var error in (ex as DbEntityValidationException).EntityValidationErrors)
                //    {
                //        foreach (var vError in error.ValidationErrors)
                //        {
                //            sb.AppendLine(vError.PropertyName + ":  " + vError.ErrorMessage);
                //        }
                //    }
                //}
                //else
                if (!string.IsNullOrEmpty(ex.Message))
                {
                    if (sb.Length > 0)
                        sb.Append(separator);
                    sb.Append(ex.Message);
                }
                ex = ex.InnerException;
            }

            return sb.ToString();
        }


        public static void MapRequestProperties(object requestDraft, object draft, NameObjectCollectionBase.KeysCollection keys)
        {
            var props = draft.GetType().GetProperties().Select(x => x.Name).ToList();
            foreach (string parName in keys)
            {
                if (props.Contains(parName))
                {
                    var propInf = draft.GetType().GetProperty(parName);
                    if (!propInf.CanRead || !propInf.CanWrite) continue;
                    var prValue = propInf.GetValue(requestDraft);
                    Debug.WriteLine($"{parName}:{prValue}");
                    propInf.SetValue(draft, prValue);
                }
            }
        }




        public static bool IsNumericType(this Type type)
        {
            HashSet<Type> NumericTypes = new HashSet<Type>
            {
                typeof(int),
                typeof(uint),
                typeof(long),
                typeof(ulong),
                typeof(double),
                typeof(float),
                typeof(decimal),

            };


            return NumericTypes.Contains(type) ||
                   NumericTypes.Contains(Nullable.GetUnderlyingType(type));
        }



        public static UserFieldDataType GetFieldType(PropertyInfo property)
        {
            var dataType = UserFieldDataType.Number;
            var type = property.PropertyType;
            if (Nullable.GetUnderlyingType(type) != null)
            { type = Nullable.GetUnderlyingType(type); }
            switch (type.Name)
            {
                case "String": dataType = UserFieldDataType.String; break;
                case "DateTime": dataType = UserFieldDataType.Date; break;
                case "Boolean": dataType = UserFieldDataType.Boolean; break;
            }

            return dataType;
        }


        public static bool ExistsProperty(object d, string propName)
        {
            Type type = d.GetType();
            if (type.Name == typeof(JObject).Name)
            {
                dynamic jobj = d;
                var x = jobj[propName];
                return x != null;
            }
            return type.GetProperties().Any(p => p.Name == propName);
        }


        public static bool IsNullable(this Type type)
        {
            Type underlyingType;
            return type.IsNullable(out underlyingType);
        }
        public static bool IsNullable(this Type type, out Type underlyingType)
        {
            underlyingType = Nullable.GetUnderlyingType(type);
            return underlyingType != null;
        }
        public static Type GetNullable(Type type)
        {
            Type underlyingType;
            return type.IsNullable(out underlyingType) ? type : NullableTypesCache.Get(type);
        }
        public static bool IsExactOrNullable(this Type type, Func<Type, bool> predicate)
        {
            Type underlyingType;
            if (type.IsNullable(out underlyingType))
                return underlyingType.IsExactOrNullable(predicate);
            return predicate(type);
        }
        public static bool IsExactOrNullable<T>(this Type type)
            where T : struct
        {
            return type.IsExactOrNullable(t => Equals(t, typeof(T)));
        }

        static class NullableTypesCache
        {
            readonly static ConcurrentDictionary<Type, Type> cache = new ConcurrentDictionary<Type, Type>();
            static NullableTypesCache()
            {
                cache.TryAdd(typeof(byte), typeof(byte?));
                cache.TryAdd(typeof(short), typeof(short?));
                cache.TryAdd(typeof(int), typeof(int?));
                cache.TryAdd(typeof(long), typeof(long?));
                cache.TryAdd(typeof(float), typeof(float?));
                cache.TryAdd(typeof(double), typeof(double?));
                cache.TryAdd(typeof(decimal), typeof(decimal?));
                cache.TryAdd(typeof(sbyte), typeof(sbyte?));
                cache.TryAdd(typeof(ushort), typeof(ushort?));
                cache.TryAdd(typeof(uint), typeof(uint?));
                cache.TryAdd(typeof(ulong), typeof(ulong?));
                //... 
            }
            readonly static Type NullableBase = typeof(Nullable<>);
            internal static Type Get(Type type)
            {
                // Try to avoid the expensive MakeGenericType method call
                return cache.GetOrAdd(type, t => NullableBase.MakeGenericType(t));
            }
        }
    }

    public enum UserFieldDataType
    {
        Number,
        String,
        Date,
        Boolean
    }


}
