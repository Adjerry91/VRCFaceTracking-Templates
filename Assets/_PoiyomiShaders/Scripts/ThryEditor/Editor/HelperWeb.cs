﻿// Material/Shader Inspector for Unity 2017/2018
// Copyright (C) 2019 Thryrallo

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Thry
{
    public class WebHelper
    {
        public static string FixUrl(string url)
        {
            if (!url.StartsWith("http"))
                url = "http://" + url;
            url = url.Replace("\\","/");
            if (System.Text.RegularExpressions.Regex.IsMatch(url, @"^https?:\/[^\/].*"))
                url = url.Replace(":/", "://");
            return url;
        }

        public static string GetFinalRedirect(string url)
        {
            if (string.IsNullOrEmpty(url))
                return url;
            try
            {
                UnityWebRequest request = new UnityWebRequest(url);
                request.method = UnityWebRequest.kHttpVerbHEAD;
                DownloadHandlerBuffer response = new DownloadHandlerBuffer();
                request.downloadHandler = response;
                request.SendWebRequest();
                bool fetching = true;
                while (fetching)
                {
                    if (request.isHttpError || request.isNetworkError)
                    {
                        fetching = false;
                        Debug.Log(request.error);
                    }
                    if (request.isDone)
                    {
                        fetching = false;
                    }
                }
                return request.url;
            }
            catch (Exception ex)
            {
                ex.ToString();
                return null;
            }
        }

        private static Dictionary<string, string> fileCache = new Dictionary<string, string>();
        public static string GetCachedString(string url)
        {
            if (fileCache.ContainsKey(url) == false) fileCache[url] = DownloadString(url);
            return fileCache[url];
        }

        //-------------------Downloaders-----------------------------

        [InitializeOnLoad]
        public class MainThreader
        {
            private struct CallData
            {
                public Action<string> action;
                public object[] arguments;
            }
            static List<CallData> queue;

            static MainThreader()
            {
                queue = new List<CallData>();
                EditorApplication.update += Update;
            }

            public static void Call(Action<string> action, params object[] args)
            {
                if (action == null)
                    return;
                CallData data = new CallData();
                data.action = action;
                data.arguments = args;
                if (args == null || args.Length == 0 || args[0] == null)
                    data.arguments = new object[] { "" };
                else
                    data.arguments = args;
                queue.Add(data);
            }

            public static void Update()
            {
                if (queue.Count > 0)
                {
                    try
                    {
                        queue[0].action.DynamicInvoke(queue[0].arguments);
                    }
                    catch { }
                    queue.RemoveAt(0);
                }
            }
        }

        public static void DownloadFile(string url, string path)
        {
            DownloadAsFile(url, path);
        }

        public static void DownloadFileASync(string url, string path, Action<string> callback)
        {
            DownloadAsBytesASync(url, delegate (object o, DownloadDataCompletedEventArgs a)
            {
                if (a.Cancelled || a.Error != null)
                    MainThreader.Call(callback, null);
                else
                {
                    FileHelper.writeBytesToFile(a.Result, path);
                    MainThreader.Call(callback, path);
                }
            });
        }

        public static string DownloadString(string url)
        {
            return DownloadAsString(url);
        }

        public static void DownloadStringASync(string url, Action<string> callback)
        {
            DownloadAsStringASync(url, delegate (object o, DownloadStringCompletedEventArgs e)
            {
                if (e.Cancelled || e.Error != null)
                {
                    Debug.LogWarning(e.Error);
                    MainThreader.Call(callback, null);
                }
                else
                    MainThreader.Call(callback, e.Result);
            });
        }

        private static void SetCertificate()
        {
            ServicePointManager.ServerCertificateValidationCallback =
        delegate (object s, X509Certificate certificate,
                 X509Chain chain, SslPolicyErrors sslPolicyErrors)
        { return true; };
        }

        private static string DownloadAsString(string url)
        {
            SetCertificate();
            string contents = null;
            try
            {
                using (var wc = new System.Net.WebClient())
                    contents = wc.DownloadString(url);
            }catch(WebException e)
            {
                Debug.LogError(e);
            }
            return contents;
        }

        private static void DownloadAsStringASync(string url, Action<object, DownloadStringCompletedEventArgs> callback)
        {
            SetCertificate();
            using (var wc = new System.Net.WebClient())
            {
                wc.Headers["User-Agent"] = "Mozilla/4.0 (Compatible; Windows NT 5.1; MSIE 6.0)";
                wc.DownloadStringCompleted += new DownloadStringCompletedEventHandler(callback);
                wc.DownloadStringAsync(new Uri(url));
            }
        }

        private static void DownloadAsFileASync(string url, string path, Action<object, AsyncCompletedEventArgs> callback)
        {
            SetCertificate();
            using (var wc = new System.Net.WebClient())
            {
                wc.DownloadFileCompleted += new AsyncCompletedEventHandler(callback);
                wc.DownloadFileAsync(new Uri(url), path);
            }
        }

        private static void DownloadAsFile(string url, string path)
        {
            SetCertificate();
            using (var wc = new System.Net.WebClient())
                wc.DownloadFile(url, path);
        }

        private static byte[] DownloadAsBytes(string url)
        {
            SetCertificate();
            byte[] contents = null;
            using (var wc = new System.Net.WebClient())
                contents = wc.DownloadData(url);
            return contents;
        }

        private static void DownloadAsBytesASync(string url, Action<object, DownloadDataCompletedEventArgs> callback)
        {
            SetCertificate();
            using (var wc = new System.Net.WebClient())
            {
                wc.DownloadDataCompleted += new DownloadDataCompletedEventHandler(callback);
                url = FixUrl(url);
                wc.DownloadDataAsync(new Uri(url));
            }
        }
    }
}