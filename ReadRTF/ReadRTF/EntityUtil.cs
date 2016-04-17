using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Sys.Common
{
    public class EntityUtil
    {
        /// <summary>
        /// Create a new Entity from datarow
        /// </summary>
        /// <typeparam name="T">Entity</typeparam>
        /// <param name="drInit">DataRow from DataTable</param>
        /// <returns></returns>
        public static T Create<T>(DataRow drInit)
        {
            T pINs = (T)Activator.CreateInstance(typeof(T));
            PropertyInfo[] piIns = pINs.GetType().GetProperties();
            for (int i = 0; i < piIns.Length; i++)
            {
                string piName = piIns[i].Name;
                if (piIns[i].CanWrite && drInit.Table.Columns.Contains(piName) && !drInit.IsNull(piName))
                {
                    Type propType = piIns[i].PropertyType;
                    if (propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        propType = propType.GetGenericArguments()[0];
                    }

                    if (propType == typeof(string))
                        piIns[i].SetValue(pINs, drInit[piName].ToString().Trim(), null);
                    else if (propType == typeof(decimal))
                        piIns[i].SetValue(pINs, Convert.ToDecimal(drInit[piName]), null);
                    else
                        piIns[i].SetValue(pINs, drInit[piName], null);
                }
            }

            return pINs;
        }

        /// <summary>
        /// Create a List of Entity from DataTable
        /// </summary>
        /// <typeparam name="T">Entity</typeparam>
        /// <param name="dtInit">DataTable</param>
        /// <returns></returns>
        public static List<T> Create<T>(DataTable dtInit)
        {
            List<T> result = new List<T>();

            foreach (DataRow drInit in dtInit.Rows)
            {
                result.Add(Create<T>(drInit));
            }

            return result;
        }

        public static T Clone<T>(T insSrc)
        {
            T insDest = (T)Activator.CreateInstance(typeof(T));
            PropertyInfo[] piSrc = insSrc.GetType().GetProperties();
            PropertyInfo[] piDesc = insDest.GetType().GetProperties();

            for (int i = 0; i < piSrc.Length; i++)
            {
                string srcName = piSrc[i].Name;
                if (!srcName.StartsWith("C_"))
                    continue;
                if (piDesc[i].CanWrite)
                    piDesc[i].SetValue(insDest, piSrc[i].GetValue(insSrc, null), null);
            }

            return insDest;
        }

        public static Dictionary<string, KeyValuePair<string, string>> GetDisplayAttr<T>(T pINS)
        {
            Dictionary<string, KeyValuePair<string, string>> result = new Dictionary<string, KeyValuePair<string, string>>();
            PropertyInfo[] piSrc = pINS.GetType().GetProperties();

            for (int i = 0; i < piSrc.Length; i++)
            {
                object objValue = piSrc[i].GetValue(pINS, null);

                foreach (Attribute attr in piSrc[i].GetCustomAttributes(true))
                {
                    DisplayAttr getAttr = attr as DisplayAttr;
                    if (null != getAttr)
                    {
                        string strValue = string.Empty;
                        if (objValue != null)
                            strValue = objValue.ToString();
                        result.Add(piSrc[i].Name, new KeyValuePair<string, string>(getAttr.Description, strValue));
                    }
                }
            }
            return result;
        }

        public static Dictionary<string, string> GetDisplayAttr<T>()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            Type type = typeof(T);
            foreach (PropertyInfo field in type.GetProperties())
            {
                foreach (Attribute attr in field.GetCustomAttributes(true))
                {
                    DisplayAttr getAttr = attr as DisplayAttr;
                    if (null != getAttr)
                    {
                        result.Add(field.Name, getAttr.Description);
                    }
                }
            }

            return result;
        }
    }

    internal class DisplayAttr : Attribute
    {
        public DisplayAttr(string descrition_in)
        {
            this.description = descrition_in;
        }

        private string description;
        public string Description
        {
            get
            {
                return this.description;
            }
        }
    }

    internal class BoolAttr : Attribute
    {
        public BoolAttr(bool pValue)
        {
            this.bValue = pValue;
        }

        private bool bValue;
        public bool B_Value
        {
            get
            {
                return this.bValue;
            }
        }
    }
}