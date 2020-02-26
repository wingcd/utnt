#if !DISABLE_TERMINAL

using UnityEngine;
using System.Collections;
using System;
using Wing.Tools.ArgsParser;
using System.Linq;

namespace Wing.Tools.Terminal
{
    public class ManWorker : IWorker
    {
		public string Description()
		{
			return "man: is a tool to query worker's usage";
		}

        public string Usage()
        {
            return @"Usage:
search cmd's usage;
use like:
    man cmdName";
        }

        public string Do(Connect conn, MessageModel request, string[] args)
        {
            if(args.Length > 0)
            {
                var result = "";
                for (var i = 0; i < args.Length; i++)
                {
                    var cmd = args[i];
                    var worker = UTerminal.Instance.Get(cmd);
                    if (worker != null)
                    {
                        result += "\n" + cmd + ":" + worker.Description();
                        result += "\n" + worker.Usage();
                    }
                    else
                    {
                        result += "\nno worker named:" + cmd;
                    }
                }
                return result;
            }
            else
            {
                return "need set worker' name!";
            }
        }

        public string GetName()
        {
            return "man";
        }
        public void OnClose(Connect conn)
        {
        }
    }
}

#endif
