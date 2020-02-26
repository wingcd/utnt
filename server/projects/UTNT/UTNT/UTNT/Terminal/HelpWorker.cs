#if !DISABLE_TERMINAL

using UnityEngine;
using System.Collections;
using System;
using Wing.Tools.ArgsParser;
using System.Linq;

namespace Wing.Tools.Terminal
{
    public class HelpWorker : IWorker
    {
		public string Description()
		{
			return "help: for query all worker";
		}

        public string Usage()
        {
            return @"Usage:
help";
        }

        public string Do(Connect conn, MessageModel request, string[] args)
        {
			var works = UTerminal.Instance.Get();
			var result = new System.Text.StringBuilder();
			foreach (var pair in works)
			{
				result.Append(pair.Value.Description() + "\n");
			}

            return result.ToString();
        }

        public string GetName()
        {
            return "help";
        }
        public void OnClose(Connect conn)
        {
        }
    }
}

#endif
