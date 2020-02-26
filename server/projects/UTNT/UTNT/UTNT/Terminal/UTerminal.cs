#if !DISABLE_TERMINAL
using UnityEngine;
using System.Collections;
using UTNT.WebSocketSharp;
using UTNT.WebSocketSharp.Server;
using System.Collections.Generic;
using System.Linq;
using System;
using Wing.Tools.Utils;
using UTNT.WebSocketSharp;

namespace Wing.Tools.Terminal
{
    [Serializable]
    public class Inline
    {
        public string background = "#000000";
        public bool bold = true;
        public string color = "#ffffff";
        //public string font = "";
        //Inline Code
        public bool code = false;
        public bool italic = false;
        public string link;
        //public int size = 80;
        public bool strike = false;
        public bool script = false;
        public bool underline = false;
    }

    [Serializable]
    public class MessageModel
    {
        [SerializeField]
        public long id;
        [SerializeField]
        public string cmd;
        [SerializeField]
        public string content;

        /// <summary>
        /// text/image
        /// </summary>
        [SerializeField]
        public string type = "text";
        [SerializeField]
        public Inline inline;
        [SerializeField]
        public object value;
    }

    /// <summary>
    /// working on the main thread
    /// </summary>
    public interface IWorker
    {
        string GetName();
        string Do(Connect conn, MessageModel request, string[] args);
        void OnClose(Connect conn);
        string Usage();
        string Description();
    }

    public class UTerminal
    {
        private static UTerminal sInstance;
        public static UTerminal Instance
        {
            get
            {
                if (sInstance == null)
                {
                    sInstance = new UTerminal();
                }
                return sInstance;
            }
        }

        private UTerminal()
        {

        }

        private bool mIgnoreExtensions = true;
        /// <summary>
        /// default is true
        /// when not in il2cpp,must to close websocket extension,mono not support except win&osx
        /// </summary>
        public bool IgnoreExtensions
        {
            get
            {
                return mIgnoreExtensions;
            }
            set
            {
                IgnoreExtensions = value;
            }
        }

        WebSocketServer mWSsv;
        int mPort = 0;
        Dictionary<string, IWorker> mWorks = new Dictionary<string, IWorker>();
        public void init(int port = 8087)
        {
            UTNT.WebSocketSharp.Utils.Instance.InternetCacheDir = UtilsHelper.TemporaryCachePath;
            //WebSocketSharp.Utils.Instance.ApplicationDataDir = UtilsHelper.TemporaryCachePath;
            //WebSocketSharp.Utils.Instance.LocalApplicationDataDir = UtilsHelper.TemporaryCachePath;

            mPort = port;
            if (mWSsv == null)
            {
                mWSsv = new WebSocketServer(port);

                mWSsv.AllowForwardedRequest = true;
                mWSsv.AddWebSocketService<Connect>("/terminal", behaviour =>
                {
                    behaviour.IgnoreExtensions = IgnoreExtensions;
                });

                RegistDefaultWork();
            }
        }

        public bool IsListening()
        {
            return mWSsv != null && mWSsv.IsListening;
        }

        public int Port
        {
            get
            {
                return mPort;
            }
        }

        public void Start(int port = 8086)
        {
            init(port);
            if (!mWSsv.IsListening)
            {
                mWSsv.Start();

                Debug.Log("terminal server running on port:" + port);
            }
        }

        public void Stop()
		{
            if (mWSsv != null)
            {
                mWSsv.Stop(CloseStatusCode.Normal, "__server_close__");
                mWSsv = null;
            }
        }

        public void Regist(IWorker worker)
        {
            var name = worker.GetName();
            if(worker == null)
            {
                throw new System.Exception("can not regist empty work");
            }
            if(Get(name) != null)
            {
                throw new System.Exception("had registed the same name " + name);
            }
            mWorks[name] = worker;
        }

        public void UnRegist(string name)
        {
            mWorks.Remove(name);
        }

        public IWorker Get(string name)
        {
            if(mWorks.ContainsKey(name))
            {
                return mWorks[name];
            }
            return null;
        }

        public Dictionary<string, IWorker> Get()
        {
            return mWorks;
        }

        private void RegistDefaultWork()
        {
            Regist(new HelpWorker());
            Regist(new ManWorker());
            Regist(new PlayerPrefsWorker());
            //Regist(new LogWorker());
        }
    }

}

#endif
