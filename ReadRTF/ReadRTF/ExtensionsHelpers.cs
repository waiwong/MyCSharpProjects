using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Sys.Common
{
    public static class ExtensionsHelpers
    {
        #region DateTime
        /// <summary>
        /// Get the first Day of week
        /// </summary>
        /// <param name="dt">datetime</param>
        /// <param name="startOfWeek">DayOfWeek.Monday or DayOfWeek.Sunday</param>
        /// <returns></returns>
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = dt.DayOfWeek - startOfWeek;
            if (diff < 0)
            {
                diff += 7;
            }

            return dt.AddDays(-1 * diff).Date;
        }

        /// <summary>
        /// Get the first Day of week, start of day is Sunday
        /// </summary>
        /// <param name="dt">datetime</param>
        /// <returns></returns>
        public static DateTime StartOfWeek(this DateTime dt)
        {
            int diff = dt.DayOfWeek - DayOfWeek.Sunday;
            if (diff < 0)
            {
                diff += 7;
            }

            return dt.AddDays(-1 * diff).Date;
        }

        /// <summary>
        /// get string of src with yyyy/MM/dd
        /// </summary>
        /// <param name="src">source date</param>
        /// <returns></returns>
        public static string ToYMD(this DateTime src)
        {
            return src.ToString("yyyy/MM/dd").Replace("-", "/");
        }

        /// <summary>
        /// get string of src.AddDays(days) with yyyy/MM/dd
        /// </summary>
        /// <param name="src">source date</param>
        /// <param name="days">adjust day</param>
        /// <returns></returns>
        public static string ToYMD(this DateTime src, int days)
        {
            return src.AddDays(days).ToString("yyyy/MM/dd").Replace("-", "/");
        }

        /// <summary>
        /// get string of src with yyyyMMdd
        /// </summary>
        /// <param name="src">source date</param>
        /// <returns></returns>
        public static string ToYMDNoSlash(this DateTime src)
        {
            return src.ToString("yyyyMMdd");
        }

        #endregion

        #region List and DataTable
        /// <summary>
        /// Converts List To DataTable
        /// </summary>
        /// <typeparam name="TSource">Class Name</typeparam>
        /// <param name="data">ListData</param>
        /// <returns></returns>
        public static DataTable ToDataTable<TSource>(this IList<TSource> data)
        {
            DataTable dataTable = new DataTable(typeof(TSource).Name);
            PropertyInfo[] props = typeof(TSource).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in props)
            {
                dataTable.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }

            foreach (TSource item in data)
            {
                var values = new object[props.Length];
                for (int i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }

            return dataTable;
        }

        /// <summary>
        /// Converts DataTable To List
        /// </summary>
        /// <typeparam name="TSource">Class Name</typeparam>
        /// <param name="dataTable">DataTable</param>
        /// <returns></returns>
        public static List<TSource> ToList<TSource>(this DataTable dataTable) where TSource : new()
        {
            var dataList = new List<TSource>();

            const BindingFlags Flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;
            var objFieldNames = (from PropertyInfo aProp in typeof(TSource).GetProperties(Flags)
                                 select new { Name = aProp.Name, Type = Nullable.GetUnderlyingType(aProp.PropertyType) ?? aProp.PropertyType }).ToList();
            var dataTblFieldNames = (from DataColumn aHeader in dataTable.Columns
                                     select new { Name = aHeader.ColumnName, Type = aHeader.DataType }).ToList();
            var commonFields = objFieldNames.Intersect(dataTblFieldNames).ToList();

            foreach (DataRow dataRow in dataTable.AsEnumerable().ToList())
            {
                var aTSource = new TSource();
                foreach (var aField in commonFields)
                {
                    PropertyInfo propertyInfos = aTSource.GetType().GetProperty(aField.Name);
                    propertyInfos.SetValue(aTSource, dataRow[aField.Name], null);
                }
                dataList.Add(aTSource);
            }
            return dataList;
        }
        #endregion

        #region Enum
        public static int Value(this Enum enumIns)
        {
            return Convert.ToInt32(enumIns);
        }

        public static string ValueStr(this Enum enumIns)
        {
            return Convert.ToInt32(enumIns).ToString();
        }

        public static T ToEnum<T>(this int iValue)
        {
            if (Enum.IsDefined(typeof(T), iValue))
                return (T)Enum.ToObject(typeof(T), iValue);
            else
                return GetEnumDefault<T>();
        }

        public static T ToEnum<T>(this int iValue, T defaultValue)
        {
            if (Enum.IsDefined(typeof(T), iValue))
                return (T)Enum.ToObject(typeof(T), iValue);
            else
                return defaultValue;
        }

        public static T ToEnum<T>(this string strValue)
        {
            if (Enum.IsDefined(typeof(T), strValue))
                return (T)Enum.Parse(typeof(T), strValue, true);
            else
            {
                int iValue;
                if (int.TryParse(strValue, out iValue))
                    return iValue.ToEnum<T>();
                else
                    return GetEnumDefault<T>();
            }
        }

        public static T ToEnum<T>(this string strValue, T defaultValue)
        {
            if (Enum.IsDefined(typeof(T), strValue))
                return (T)Enum.Parse(typeof(T), strValue, true);
            else
            {
                int iValue;
                if (int.TryParse(strValue, out iValue))
                    return iValue.ToEnum<T>();
                else
                    return defaultValue;
            }
        }

        private static T GetEnumDefault<T>()
        {
            Type t = typeof(T);
            DefaultValueAttribute[] attributes = (DefaultValueAttribute[])t.GetCustomAttributes(typeof(DefaultValueAttribute), false);
            if (attributes != null && attributes.Length > 0)
                return (T)attributes[0].Value;
            else
                return (T)Enum.GetValues(typeof(T)).GetValue(0);
        }
        #endregion
    }
}
