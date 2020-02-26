
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Wing.Tools.Utils
{
	public enum EDirType
	{
		DataPath,
		StreamingAssetsPath,
		PersistentDataPath,
		TemporaryCachePath
	}

	public enum EPlatform
	{
		iOS,
		Android,
		Windows,
		Mac,
	}

    public static class Ext
    {
        public static string Format(this Color32 color)
        {
            return string.Format("#{0:X2}{1:X2}{2:X2}", color.r, color.g, color.b);
        }
    }

    public class UtilsHelper
    {
        public static string DataPath = "";
        public static string PersistentDataPath;
        public static string StreamingAssetsPath;
        public static string TemporaryCachePath;
        public static RuntimePlatform Platform;

        public static void Init()
        {
            DataPath = Application.dataPath;
            PersistentDataPath = Application.persistentDataPath;
            StreamingAssetsPath = Application.streamingAssetsPath;
            TemporaryCachePath = Application.temporaryCachePath;
            Platform = Application.platform;
        }

		public static EPlatform GetPlatform()
		{
            if (Platform == RuntimePlatform.IPhonePlayer)
            {
                return EPlatform.iOS;
            }
            else if (Platform == RuntimePlatform.Android)
            {
                return EPlatform.Android;
            }
            else if (Platform == RuntimePlatform.WindowsPlayer || Platform == RuntimePlatform.WindowsEditor)
            {
                return EPlatform.Windows;
            }
            else if (Platform == RuntimePlatform.OSXPlayer || Platform == RuntimePlatform.OSXEditor)
            {
                return EPlatform.Mac;
            }
            else
            {
                return EPlatform.Mac;
            }
		}

        //不同平台下StreamingAssets的路径是不同的，这里需要注意一下。  
        public static string WWWStreamAssetsPath
        {
            get
            {
				string path = "";
                if (Application.platform == RuntimePlatform.Android)
                {
					path = "jar:file:///" + DataPath + "!/assets/";
				}
                else if(Application.platform == RuntimePlatform.IPhonePlayer)
				{
					path = "file://" + DataPath + "/Raw/";
				}
                //else if(Application.platform == RuntimePlatform.OSXEditor ||  Application.platform == RuntimePlatform.OSXPlayer)
                //{
                //    path = "file:/" + Application.dataPath + "/StreamingAssets/";
                //}
				else
				{
					path = "file://" + DataPath + "/StreamingAssets/";
				}

				return path;   
            }
        }

		public static string StreamAssetsPath
		{
			get
			{
				string path = "";
				if (Application.platform == RuntimePlatform.Android)
				{
					path = Application.dataPath + "!/assets/";
				}
				else if (Application.platform == RuntimePlatform.IPhonePlayer)
				{
					path = Application.dataPath + "/Raw/";
				}
				//else if(Application.platform == RuntimePlatform.OSXEditor ||  Application.platform == RuntimePlatform.OSXPlayer)
				//{
				//    path = "file:/" + Application.dataPath + "/StreamingAssets/";
				//}
				else
				{
					path = Application.dataPath + "/StreamingAssets/";
				}

				return path;
			}
		}

        public static string GetDataPath()
        {
            string path = "";
            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                path = Application.persistentDataPath + "/";
            }
            else if (Application.platform == RuntimePlatform.WindowsPlayer)
            {
                path = Application.dataPath + "/";
            }
            else if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                path = Application.dataPath + "/../";
            }
            else
            {
                path = Application.persistentDataPath + "/";
            }

            return path;
        }

        public static string GetResourcePath()
        {
            return GetDataPath() + "ResData/";
        }

        public static string GetExtentAssetsPath()
        {
            return GetDataPath() + "AppAssets/";
        }

        public static void CopyFileTo(string src, string dst, Action<bool> cb)
        {
            CoroutineProvider.Instance.StartCoroutine(_copyTo(src,dst, cb));
        }

        private static IEnumerator _copyTo(string src, string dst, Action<bool> cb)
        {
            var www = new WWW(src);
            yield return www;

			var done = string.IsNullOrEmpty(www.error);
			if (done)
			{
				try
				{
					File.WriteAllBytes(dst, www.bytes);
				}
				catch
				{
					done = false;
				}
			}
			if (cb != null)
			{
				cb(done);
			}
            www.Dispose();
        }

        public static IEnumerator LoadTexture(string url, Action<Texture2D> cb)
        {
            //这里的url可以是web路径也可以是本地路径file://  
            WWW www = new WWW(url);
            //挂起程序段，等资源下载完成后，继续执行下去  
            yield return www;

            //判断是否有错误产生  
            if (string.IsNullOrEmpty(www.error))
            {
                //把下载好的图片回调给调用者  
                cb.Invoke(www.texture);
			}
			//释放资源  
			www.Dispose();
        }

        public static IEnumerator Load(string url, Action<WWW> cb)
        {
            //这里的url可以是web路径也可以是本地路径file://  
            WWW www = new WWW(url);
            //挂起程序段，等资源下载完成后，继续执行下去  
            yield return www;

            //判断是否有错误产生  
            if (string.IsNullOrEmpty(www.error))
            {
                //把下载好的图片回调给调用者  
                cb.Invoke(www);
			}
			//释放资源  
			www.Dispose();
        }

        public static IEnumerator Load(string url, Action<WWW,object> cb, object data)
        {
            //这里的url可以是web路径也可以是本地路径file://  
            WWW www = new WWW(url);
            //挂起程序段，等资源下载完成后，继续执行下去  
            yield return www;

            //判断是否有错误产生  
            if (string.IsNullOrEmpty(www.error))
            {
                //把下载好的图片回调给调用者  
                cb.Invoke(www, data);
			}
			//释放资源  
			www.Dispose();
        }

		public static void LoadAssetBundle(string url, Action<AssetBundle> cb)
		{
            CoroutineProvider.Instance.StartCoroutine(_loadAssetBundle(url, cb));
		}

		public static void WWWLoad(string url, Action<WWW> cb)
		{
			CoroutineProvider.Instance.StartCoroutine(_loadAssetBundle(url, cb));
		}

		private static IEnumerator _loadAssetBundle(string url, Action<WWW> cb)
		{
			var www = new WWW(url);
			yield return www;
			while (!www.isDone)
			{
				yield return null;
			}
			cb(www);
			//www.Dispose();
		}

		private static IEnumerator _loadAssetBundle(string url, Action<AssetBundle> cb)
        {
            var www = new WWW(url);
            yield return www;
            while(!www.isDone)
            {
                yield return null;
            }
            cb(www.assetBundle);
            //www.Dispose();
        }

        public static void SaveTextureFile(Texture2D incomingTexture, string filename, bool png = true)
        {
            byte[] bytes = png ? incomingTexture.EncodeToPNG() : incomingTexture.EncodeToJPG();
            string dir = Path.GetDirectoryName(filename);
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            File.WriteAllBytes(filename, bytes);
        }

        public static bool SaveRenderTextureToPNG(Texture inputTex, Material mat, string filename)
        {
            RenderTexture temp = RenderTexture.GetTemporary(inputTex.width, inputTex.height, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(inputTex, temp, mat);
            bool ret = SaveRenderTextureToPNG(temp, filename);
            RenderTexture.ReleaseTemporary(temp);
            return ret;
        }
        
        public static bool SaveRenderTextureToPNG(RenderTexture rt, string filename)
        {
            Texture2D png = CreateTexture2DFromRT(rt);

            SaveTextureFile(png, filename);

            Texture2D.DestroyImmediate(png);
            png = null;
            return true;
        }

        public static Texture2D CreateTexture2DFromRT(RenderTexture rt, Rect? rect = null)
        {
            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = rt;
            if(rect == null)
            {
                rect = new Rect(0, 0, rt.width, rt.height);
            }
            else
            {
                float width = rect.Value.width;
                if(rect.Value.width + rect.Value.x > rt.width)
                {
                    width = rt.width - rect.Value.x;
                }
                float height = rect.Value.height;
                if(rect.Value.height + rect.Value.y > rt.height)
                {
                    height = rt.height - rect.Value.y;
                }
                rect = new Rect(rect.Value.x, rect.Value.y, width, height);
            }
            Texture2D texture = new Texture2D((int)rect.Value.width, (int)rect.Value.height, TextureFormat.ARGB32, false);
            texture.ReadPixels(rect.Value, 0 , 0);
            texture.Apply();

            RenderTexture.active = prev;
            return texture;

        }

        public static Texture2D LoadPNGFromDisk(string url)
        {
            if (!File.Exists(url))
            {
                return null;
            }
            FileStream fileStream = new FileStream(url, FileMode.Open, System.IO.FileAccess.Read);

            fileStream.Seek(0, SeekOrigin.Begin);

            byte[] binary = new byte[fileStream.Length]; //创建文件长度的buffer
            fileStream.Read(binary, 0, (int)fileStream.Length);

            fileStream.Close();

            fileStream.Dispose();

            fileStream = null;

            var tex = new Texture2D(1, 1);
            tex.LoadImage(binary);
            return tex;
        }

		public static string StringMD5(string data)
		{
			byte[] result = Encoding.Default.GetBytes(data.Trim());
			MD5 md5 = new MD5CryptoServiceProvider();
			byte[] output = md5.ComputeHash(result);
			return BitConverter.ToString(output).Replace("-", "");
		}

	    public static string GetFileHash(string filePath)
	    {
	        try
	        {
	            FileStream fs = new FileStream(filePath, FileMode.Open);
	            int len = (int)fs.Length;
	            byte[] data = new byte[len];
	            fs.Read(data, 0, len);
	            fs.Close();
	            System.Security.Cryptography.MD5 md5 = new MD5CryptoServiceProvider();
	            byte[] result = md5.ComputeHash(data);
	            string fileMD5 = "";
	            foreach (byte b in result)
	            {
	                fileMD5 += Convert.ToString(b, 16);
	            }
	            return fileMD5;
	        }
	        catch (FileNotFoundException e)
	        {
	            Console.WriteLine(e.Message);
	            return "";
	        }
	    }

        public static double GetUTCMilliseconds()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return ts.TotalMilliseconds;
        }
    }
}
