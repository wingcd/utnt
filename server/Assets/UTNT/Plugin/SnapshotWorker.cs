using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Wing.Tools.ArgsParser;
using System.Text.RegularExpressions;
using Wing.Tools.Terminal;
using Wing.Tools.WebServer;
using Wing.Tools.Utils;

namespace Wing.Tools
{
    public class SnapshotWorker : IWorker
    {        
        ArgumentParser mParser = new ArgumentParser();
        public SnapshotWorker()
        {

        }

		public string Description()
		{
			return "snapshot: snapshot current screen";
		}

        public string Usage()
        {
            return mParser.Usage();
        }

        public string Do(Connect conn, MessageModel request, string[] args)
        {
            CoroutineProvider.Instance.StartCoroutine(CaptureScreen(conn, request));

            return null;
        }

        IEnumerator CaptureScreen(Connect conn, MessageModel request)
        {
            var rect = new Rect(Screen.width * 0f, Screen.height * 0f, Screen.width * 1f, Screen.height * 1f);
            Texture2D screenShot = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGB24, false);

            yield return new WaitForEndOfFrame();

            screenShot.ReadPixels(rect, 0, 0);
            screenShot.Apply();

            string dir = UtilsHelper.GetDataPath() + "temp/";
            if(!System.IO.Directory.Exists(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
            }
            var fn = dir + "shapshot.jpg";

			var data = screenShot.EncodeToJPG();
			System.IO.File.WriteAllBytes(fn, data);

            var url = WWWCache.Instance.AddCustomData("temp/shapshot.jpg", fn);
            conn.SendImage(request, url + "?gid=" + Time.realtimeSinceStartup);
        }

        public string GetName()
        {
            return "shapshot";
        }
        public void OnClose(Connect conn)
        {
           
        }        
	}
}
