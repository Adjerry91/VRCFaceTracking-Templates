using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public static class VRCCachedWebRequest
{
    private const float DefaultCacheTimeHours = 24 * 7;

    public static void ClearOld(float cacheLimitHours = DefaultCacheTimeHours)
    {
        string cacheDir = CacheDir;
        if(!Directory.Exists(cacheDir))
        {
            return;
        }

        foreach(string fileName in Directory.GetFiles(cacheDir))
        {
            if(!(GetAge(fileName) > cacheLimitHours))
            {
                continue;
            }

            Debug.Log($"Deleting {fileName}");
            File.Delete(fileName);
        }
    }

    private static string CacheDir => Application.temporaryCachePath;

    public static IEnumerator Get(string url, Action<Texture2D> onDone, float cacheLimitHours = DefaultCacheTimeHours)
    {
        string cacheDir = CacheDir;
        if(!Directory.Exists(cacheDir))
        {
            Directory.CreateDirectory(cacheDir);
        }

        string hash = CreateHash(url);
        string cache = cacheDir + "/www_" + hash;

        if(File.Exists(cache))
        {
            // Use cached file if it exists
            if(GetAge(cache) > cacheLimitHours)
            {
                File.Delete(cache);
            }
            else
            {
                Texture2D texture = new Texture2D(2, 2);
                if(!texture.LoadImage(File.ReadAllBytes(cache)))
                {
                    yield break;
                }

                // load texture from disk and exit if we successfully read it
                texture.Apply();
                onDone(texture);
            }
        }

        else
        {
            // No cached file, load it from url
            using(UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
            {
                // Wait until request and download are complete
                yield return uwr.SendWebRequest();
                while(!uwr.isDone || !uwr.downloadHandler.isDone)
                {
                    yield return null;
                }

                var texture = DownloadHandlerTexture.GetContent(uwr);

                if(string.IsNullOrEmpty(uwr.error))
                {
                    File.WriteAllBytes(cache, uwr.downloadHandler.data);
                }

                onDone(texture);
            }
        }
    }

    private static string CreateHash(string input)
    {
        SHA256 hash = SHA256.Create();
        byte[] computedHash = hash.ComputeHash(Encoding.Default.GetBytes(input));
        return Uri.EscapeDataString(Convert.ToBase64String(computedHash));
    }

    private static double GetAge(string file)
    {
        if(!File.Exists(file))
        {
            return 0;
        }

        DateTime writeTime = File.GetLastWriteTimeUtc(file);
        return DateTime.UtcNow.Subtract(writeTime).TotalHours;
    }
}
