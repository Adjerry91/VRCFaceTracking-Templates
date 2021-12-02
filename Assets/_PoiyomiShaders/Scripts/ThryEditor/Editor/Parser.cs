﻿// Material/Shader Inspector for Unity 2017/2018
// Copyright (C) 2019 Thryrallo

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Thry
{
    public class Parser
    {

        public static string Serialize(object o)
        {
            return Parser.ObjectToString(o);
        }

        public static T Deserialize<T>(string s)
        {
            return ParseToObject<T>(s);
        }

        public static string ObjectToString(object obj)
        {
            if (obj == null) return "null";
            if (Helper.IsPrimitive(obj.GetType())) return SerializePrimitive(obj);
            if (obj is IList) return SerializeList(obj);
            if (obj.GetType().IsGenericType && obj.GetType().GetGenericTypeDefinition() == typeof(Dictionary<,>)) return SerializeDictionary(obj);
            if (obj.GetType().IsArray) return SerializeList(obj);
            if (obj.GetType().IsEnum) return obj.ToString();
            if (obj.GetType().IsClass) return SerializeClass(obj);
            if (obj.GetType().IsValueType && !obj.GetType().IsEnum) return SerializeClass(obj);
            return "";
        }

        public static T ParseToObject<T>(string s)
        {
            object parsed = ParseJson(s);
            object ret = null;
            try
            {
                ret = (T)ParsedToObject(parsed, typeof(T));
            }
            catch (Exception e)
            {
                Debug.LogWarning(e.ToString());
                Debug.LogWarning(s + " cannot be parsed to object of type " + typeof(T).ToString());
                ret = Activator.CreateInstance(typeof(T));
            }
            return (T)ret;
        }

        //Parser methods

        public static object ParseJson(string input)
        {
            //input = input.Replace("\\n", "\n");
            return ParseJsonPart(input);
        }

        private static object ParseJsonPart(string input)
        {
            input = input.Trim();
            if (input.StartsWith("{"))
                return ParseObject(input);
            else if (input.StartsWith("["))
                return ParseArray(input);
            else
                return ParsePrimitive(input);
        }

        private static Dictionary<object, object> ParseObject(string input)
        {
            input = input.TrimStart(new char[] { '{' });
            int depth = 0;
            int variableStart = 0;
            bool isString = false;
            Dictionary<object, object> variables = new Dictionary<object, object>();
            for (int i = 0; i < input.Length; i++)
            {
                bool escaped = i != 0 && input[i - 1] == '\\';
                if (input[i] == '\"' && !escaped)
                    isString = !isString;
                if (!isString)
                {
                    if (i == input.Length - 1 || (depth == 0 && input[i] == ',' && !escaped) || (!escaped && depth == 0 && input[i] == '}'))
                    {
                        string[] parts = input.Substring(variableStart, i - variableStart).Split(new char[] { ':' }, 2);
                        if (parts.Length < 2)
                            break;
                        string key = "" + ParseJsonPart(parts[0].Trim());
                        object value = ParseJsonPart(parts[1]);
                        variables.Add(key, value);
                        variableStart = i + 1;
                    }
                    else if ((input[i] == '{' || input[i] == '[') && !escaped)
                        depth++;
                    else if ((input[i] == '}' || input[i] == ']') && !escaped)
                        depth--;
                }

            }
            return variables;
        }

        private static List<object> ParseArray(string input)
        {
            input = input.Trim(new char[] { ' ' });
            int depth = 0;
            int variableStart = 1;
            List<object> variables = new List<object>();
            for (int i = 1; i < input.Length; i++)
            {
                if (i == input.Length - 1 || (depth == 0 && input[i] == ',' && (i == 0 || input[i - 1] != '\\')))
                {
                    variables.Add(ParseJsonPart(input.Substring(variableStart, i - variableStart)));
                    variableStart = i + 1;
                }
                else if (input[i] == '{' || input[i] == '[')
                    depth++;
                else if (input[i] == '}' || input[i] == ']')
                    depth--;
            }
            return variables;
        }

        private static object ParsePrimitive(string input)
        {
            if (input.StartsWith("\""))
                return input.Trim(new char[] { '"' });
            else if (input.ToLower() == "true")
                return true;
            else if (input.ToLower() == "false")
                return false;
            else if (input == "null" || input == "NULL" || input == "Null")
                return null;
            else
            {
                string floatInput = input.Replace(",", ".");
                if (System.Globalization.CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator == ",")
                    floatInput = input.Replace(".", ",");
                float floatValue;
                if (float.TryParse(floatInput,  out floatValue))
                {
                    if ((int)floatValue == floatValue)
                        return (int)floatValue;
                    return floatValue;
                }
            }
            return input;
        }

        //converter methods

        public static string GlobalizationFloat(string s)
        {
            s = s.Replace(",", ".");
            if (System.Globalization.CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator == ",")
                s = s.Replace(".", ",");
            return s;
        }

        public static float ParseFloat(string s, float defaultF = 0)
        {
            s = GlobalizationFloat(s);
            float f = defaultF;
            float.TryParse(s, out f);
            return f;
        }

        public static type ConvertParsedToObject<type>(object parsed)
        {
            return (type)ParsedToObject(parsed, typeof(type));
        }

        private static object ParsedToObject(object parsed,Type objtype)
        {
            if (parsed == null) return null;
            if (Helper.IsPrimitive(objtype)) return ConvertToPrimitive(parsed, objtype);
            if (objtype.IsGenericType && objtype.GetInterfaces().Contains(typeof(IList))) return ConvertToList(parsed, objtype);
            if (objtype.IsGenericType && objtype.GetGenericTypeDefinition() == typeof(Dictionary<,>)) return ConvertToDictionary(parsed,objtype);
            if (objtype.IsArray) return ConvertToArray(parsed, objtype);
            if (objtype.IsEnum) return ConvertToEnum(parsed, objtype);
            if (objtype.IsClass) return ConvertToObject(parsed, objtype);
            if (objtype.IsValueType && !objtype.IsEnum) return ConvertToObject(parsed, objtype);
            return null;
        }

        private static object ConvertToDictionary(object parsed, Type objtype)
        {
            var returnObject = (dynamic)Activator.CreateInstance(objtype);
            Dictionary<object, object> dict = (Dictionary<object, object>)parsed;
            foreach (KeyValuePair<object, object> keyvalue in dict)
            {
                dynamic key = ParsedToObject(keyvalue.Key, objtype.GetGenericArguments()[0]);
                dynamic value = ParsedToObject(keyvalue.Value, objtype.GetGenericArguments()[1]);
                returnObject.Add(key , value );
            }
            return returnObject;
        }

        private static object ConvertToObject(object parsed, Type objtype)
        {
            if (parsed.GetType() == typeof(string) && objtype.GetMethod("ParseForThryParser", BindingFlags.Static | BindingFlags.NonPublic) != null)
                return objtype.GetMethod("ParseForThryParser", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { parsed });
            if (parsed.GetType() != typeof(Dictionary<object, object>)) return null;
            object returnObject = Activator.CreateInstance(objtype);
            Dictionary<object, object> dict = (Dictionary<object, object>)parsed;
            foreach (FieldInfo field in objtype.GetFields())
            {
                if (dict.ContainsKey(field.Name))
                {
                    field.SetValue(returnObject, ParsedToObject(dict[field.Name], field.FieldType));
                }
            }
            foreach (PropertyInfo property in objtype.GetProperties())
            {
                if (property.CanWrite && property.CanRead && property.GetIndexParameters().Length == 0 && dict.ContainsKey(property.Name))
                {
                    property.SetValue(returnObject, ParsedToObject(dict[property.Name], property.PropertyType), null);
                }
            }
            return returnObject;
        }

        private static object ConvertToList(object parsed, Type objtype)
        {
            Type list_obj_type = objtype.GetGenericArguments()[0];
            List<object> list_strings = (List<object>)parsed;
            IList return_list = (IList)Activator.CreateInstance(objtype);
            foreach (object s in list_strings)
                return_list.Add(ParsedToObject(s, list_obj_type));
            return return_list;
        }

        private static object ConvertToArray(object parsed, Type objtype)
        {
            if (parsed.GetType() == typeof(string) && objtype.GetMethod("ParseToArrayForThryParser", BindingFlags.Static | BindingFlags.NonPublic) != null)
                return objtype.GetMethod("ParseToArrayForThryParser", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { parsed });
            if (parsed == null || (parsed is string && (string)parsed == ""))
                return null;
            Type array_obj_type = objtype.GetElementType();
            List<object> list_strings = (List<object>)parsed;
            IList return_list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(array_obj_type));
            foreach (object s in list_strings)
            {
                object o = ParsedToObject(s, array_obj_type);
                if(o!=null)
                    return_list.Add(o);
            }
            object return_array = Activator.CreateInstance(objtype, return_list.Count);
            return_list.CopyTo(return_array as Array, 0);
            return return_array;
        }

        private static object ConvertToEnum(object parsed, Type objtype)
        {
            if (Enum.IsDefined(objtype, (string)parsed))
                return Enum.Parse(objtype, (string)parsed);
            Debug.LogWarning("The specified enum for " + objtype.Name + " does not exist. Existing Values are: " + Converter.ArrayToString(Enum.GetValues(objtype)));
            return Enum.GetValues(objtype).GetValue(0);
        }

        private static object ConvertToPrimitive(object parsed, Type objtype)
        {
            if (typeof(String) == objtype)
                return parsed!=null?parsed.ToString():null;
            if (typeof(char) == objtype)
                return ((string)parsed)[0];
            return parsed;
        }

        //Serilizer

        private static string SerializeDictionary(object obj)
        {
            string ret = "{";
            foreach (var item in (dynamic)obj)
            {
                object key = item.Key;
                object val = item.Value;
                ret += Serialize(key) + ":" + Serialize(val)+",";
            }
            ret = ret.TrimEnd(new char[] { ',' });
            ret += "}";
            return ret;
        }

        private static string SerializeClass(object obj)
        {
            string ret = "{";
            foreach(FieldInfo field in obj.GetType().GetFields())
            {
                if(field.IsPublic)
                    ret += "\""+field.Name + "\"" + ":" + ObjectToString(field.GetValue(obj)) + ",";
            }
            foreach (PropertyInfo property in obj.GetType().GetProperties())
            {
                if(property.CanWrite && property.CanRead && property.GetIndexParameters().Length==0)
                    ret += "\""+ property.Name + "\"" + ":" + ObjectToString(property.GetValue(obj,null)) + ",";
            }
            ret = ret.TrimEnd(new char[] { ',' });
            ret += "}";
            return ret;
        }

        private static string SerializeList(object obj)
        {
            string ret = "[";
            foreach (object o in obj as IEnumerable)
            {
                ret += ObjectToString(o) + ",";
            }
            ret = ret.TrimEnd(new char[] { ',' });
            ret += "]";
            return ret;
        }

        private static string SerializePrimitive(object obj)
        {
            if (obj.GetType() == typeof(string))
                return "\"" + obj + "\"";
            return obj.ToString().Replace(",", "."); ;
        }
    }

    public class AnimationParser
    {
        public class Animation
        {
            public PPtrCurve[] pPtrCurves;
        }

        public class PPtrCurve
        {
            public PPtrType curveType;
            public PPtrKeyframe[] keyframes;
        }

        public enum PPtrType
        {
            None,Material
        }

        public class PPtrKeyframe
        {
            public float time;
            public string guid;
            public int type;
        }

        public static Animation Parse(AnimationClip clip)
        {
            return Parse(AssetDatabase.GetAssetPath(clip));
        }

        public static Animation Parse(string path)
        {
            string data = FileHelper.ReadFileIntoString(path);

            List<PPtrCurve> pPtrCurves = new List<PPtrCurve>();
            int pptrIndex;
            int lastIndex = 0;
            while ((pptrIndex = data.IndexOf("m_PPtrCurves", lastIndex)) != -1)
            {
                lastIndex = pptrIndex + 1;
                int pptrEndIndex = data.IndexOf("  m_", pptrIndex);

                int curveIndex;
                int lastCurveIndex = pptrIndex;
                //find all curves
                while((curveIndex = data.IndexOf("  - curve:", lastCurveIndex, pptrEndIndex- lastCurveIndex)) != -1)
                {
                    lastCurveIndex = curveIndex + 1;
                    int curveEndIndex = data.IndexOf("    script: ", curveIndex);

                    PPtrCurve curve = new PPtrCurve();
                    List<PPtrKeyframe> keyframes = new List<PPtrKeyframe>();

                    int keyFrameIndex;
                    int lastKeyFrameIndex = curveIndex;
                    while((keyFrameIndex = data.IndexOf("    - time:", lastKeyFrameIndex, curveEndIndex - lastKeyFrameIndex)) != -1)
                    {
                        lastKeyFrameIndex = keyFrameIndex + 1;
                        int keyFrameEndIndex = data.IndexOf("}", keyFrameIndex);

                        PPtrKeyframe keyframe = new PPtrKeyframe();
                        keyframe.time = float.Parse(data.Substring(keyFrameIndex, data.IndexOf("\n", keyFrameIndex, keyFrameEndIndex)));
                        keyframes.Add(keyframe);
                    }

                    curve.curveType = data.IndexOf("    attribute: m_Materials", lastKeyFrameIndex, curveEndIndex - lastKeyFrameIndex) != -1 ? PPtrType.Material : PPtrType.None;
                    curve.keyframes = keyframes.ToArray();
                    pPtrCurves.Add(curve);
                }
            }
            Animation animation = new Animation();
            animation.pPtrCurves = pPtrCurves.ToArray();
            Debug.Log(Parser.Serialize(animation));
            return animation;
        }
    }
}
