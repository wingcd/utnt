using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Wing.Tools.ArgsParser;
using System.Text.RegularExpressions;
using Wing.Tools.Terminal;
using Wing.Tools.Utils;

namespace Wing.Tools
{
    public class LogWorker : IWorker
    {
        class Listenser
        {
            public Connect Conn;
            public MessageModel Request;
            public string[] Args;
            public string Filter;
            public Regex Regex;
            public bool Track;
        }

		class Args
		{
            public Argument On = new Argument("o", "on", "log on", true, true, false);
			public Argument Off = new Argument("c", "off", "log off", true, true, false);
			public Argument Filter = new Argument(null, "f", "filter", "the filter string", true);
            public Argument Track = new Argument("t", "track", "get tracking info", true, true, false);
		}

        public Color32 LogColor = Color.white;
        public Color32 ErrorColor = Color.red;
        public Color32 WarningColor = Color.yellow;
        public Color32 ExceptionColor = Color.magenta;
        public Color32 AssetColor = Color.cyan;

        Dictionary<Connect, Listenser> mListeners = new Dictionary<Connect, Listenser>();
        List<Listenser> mList = new List<Listenser>();
        ArgumentParser mParser = new ArgumentParser();
        bool mLogOpend = false;
        public LogWorker()
        {
            var args = new Args();
            mParser.Arguments.Add(args.On);
            mParser.Arguments.Add(args.Off);
            mParser.Arguments.Add(args.Filter);
            mParser.Arguments.Add(args.Track);
        }

		public string Description()
		{
			return "log: is a tool to listen unity log";
		}

        public string Usage()
        {
            return mParser.Usage();
        }

        public string Do(Connect conn, MessageModel request, string[] args)
        {
            string reason = "";
            if(_doOn(conn,request, args, ref reason))
            {
                if(mListeners.Count > 0 && !mLogOpend)
                {
                    _openlog();
                }
            }
            else if(_doOff(conn, args, ref reason))
            {
            }
            else 
            {
                reason = Usage();
            }
            return reason;
        }

        public string GetName()
        {
            return "log";
        }
        public void OnClose(Connect conn)
        {
            _remove(conn);
        }

        bool _doOn(Connect conn, MessageModel request, string[] args, ref string reason)
        {
			ArgumentParser parser = new ArgumentParser();
			Args pargs = new Args();
            pargs.On.Optional = false;
            pargs.Filter.Optional = true;
            pargs.Track.Optional = true;

            parser.Arguments.Add(pargs.On);
            parser.Arguments.Add(pargs.Filter);
            parser.Arguments.Add(pargs.Track);

            if(!parser.Parse(args, ref reason))
            {
                return pargs.On.Parsed;
            }

			mListeners[conn] = new Listenser()
			{
				Args = args,
                Filter = pargs.Filter.Parsed ? pargs.Filter.Value.ToString() : "",
				Conn = conn,
				Request = request,
                Track = pargs.Track.Parsed,
			};
            if(!string.IsNullOrEmpty(mListeners[conn].Filter))
			{
				var lis = mListeners[conn];
                try
                {
                    lis.Regex = new Regex(lis.Filter);
                }
                catch(Exception ex)
                {
					lis.Regex = null;
                }
            }

			mList.RemoveAll(item => item.Conn == conn);
			mList.Add(mListeners[conn]);

            reason = "log on";
            if(pargs.Filter.Value != null)
            {
                reason += ",filter by " + pargs.Filter.Value; 
            }
            return true;
        }

		bool _doOff(Connect conn, string[] args, ref string reason)
		{
			ArgumentParser parser = new ArgumentParser();
			Args pargs = new Args();
            pargs.Off.Optional = false;

			parser.Arguments.Add(pargs.Off);
			if (!parser.Parse(args, ref reason))
			{
				return pargs.Off.Parsed;
			}

			_remove(conn);

			reason = "log off";

            return true;
		}
        void _remove(Connect conn)
        {
            mListeners.Remove(conn);
            mList.RemoveAll(item => item.Conn == conn);

            if(mListeners.Count <= 0)
            {
                _closelog();
            }
        }
        void _openlog()
        {
            if(mLogOpend)
            {
                return;
            }
            mLogOpend = true;
#if UNITY_5
			Application.logMessageReceived += HandleLog;
#else
            Application.RegisterLogCallback(HandleLog);  
#endif
		}
        void _closelog()
        {
			if (!mLogOpend)
			{
				return;
			}
            mLogOpend = false;
#if UNITY_5 || UNITY_5_0_OR_NEWER
            Application.logMessageReceived -= HandleLog;
#else
            Application.RegisterLogCallback(null);  
#endif
		}

		void HandleLog(string message, string stackTrace, LogType type)
		{
            for (var i = 0; i < mList.Count; i++)
            {
                var item = mList[i];
                var color = LogColor;
                switch(type)
                {
                    case LogType.Log:
                        color = LogColor;
                        break;
                    case LogType.Error:
                        color = ErrorColor;
                        break;
                    case LogType.Warning:
                        color = WarningColor;
                        break;
                    case LogType.Assert:
                        color = AssetColor;
                        break;
                    case LogType.Exception:
                        color = ExceptionColor;
                        break;
                }
                var inline = new Inline
                {
                    color = color.Format(),                     
                };
                var msg = string.Format("{0} [{1}] {2}",
                    DateTime.Now.ToString("yyyy/MM/dd mm:HH:ss"),
                    type,
                    message + (item.Track ? "\n\t\t\t\t" + stackTrace.Replace("\n", "\n\t\t\t\t") : ""));
                bool send = false;
                if (item.Regex != null)
                {
                    if (item.Regex.IsMatch(message))
                    {
                        send = true;
                    }
                    else if (message.Contains(item.Filter))
                    {
                        send = true;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(item.Filter))
                    {
                        send = true;
                    }
                    else if (message.Contains(item.Filter))
                    {
                        send = true;
                    }
                }

                if (send)
                {
                    item.Conn.SendText(item.Request, msg, inline);
                }
            }
		}
	}
}
