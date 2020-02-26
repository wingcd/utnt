#if !DISABLE_TERMINAL

using System;
using UnityEngine;
using UTNT.WebSocketSharp;
using UTNT.WebSocketSharp.Server;
using Wing.Tools.Utils;

namespace Wing.Tools.Terminal
{
	public class Connect : WebSocketBehavior
	{
		protected override void OnMessage(MessageEventArgs e)
		{
			var requst = JsonUtility.FromJson<MessageModel>(e.Data);
			if (requst != null)
			{
				var id = requst.id;
				var name = requst.cmd;
				var worker = UTerminal.Instance.Get(name);
				if (worker != null)
				{
					var args = requst.content != null ? requst.content.Trim().Split(' ') : new string[] { };
					var msg = "";
					Loom.QueueOnMainThread(() =>
					{
						msg = worker.Do(this, requst, args);
                        if (msg != null)
                        {
                            _send(id, name, msg);
                        }
					});
				}
				else
				{
					if (name == "ping")
					{
						_send(id, name, "peng");
					}
					else if (name == "heartbeat")
					{

					}
					else
					{
						_send(id, name, "can not find command " + name);
					}
				}
			}
			else
			{
				Send("");
			}
		}

		void _send(long id, string name, string msg)
		{
			var data = new MessageModel
			{
				id = id,
				cmd = name,
				content = msg,
			};
			var sendmsg = JsonUtility.ToJson(data);
			Send(sendmsg);
		}

		protected override void OnClose(CloseEventArgs e)
		{
			base.OnClose(e);

			foreach (var pair in UTerminal.Instance.Get())
			{
				pair.Value.OnClose(this);
			}

			Debug.Log("client " + Context.RequestUri.AbsoluteUri + " disconnected,reason is" + e.Reason);
		}

		protected override void OnOpen()
		{
			base.OnOpen();

			Debug.Log("client " + Context.RequestUri.AbsoluteUri + " connected");
		}

		public void SendData(string data)
		{
			Send(data);
		}

        public void CallClient(MessageModel request, string info, string type, Inline inline = null, object value = null)
        {
			var data = new MessageModel
			{
                id = request.id,
                cmd = request.cmd,
                content = info,
                type = type,
                inline = inline,
                value = value,
            };
			var sendmsg = JsonUtility.ToJson(data);
			Send(sendmsg);
        }

        public void SendText(MessageModel request, string text, Inline inline = null)
        {
            CallClient(request, text, "text", inline);
        }

        public void SendImage(MessageModel request, string url)
        {
            CallClient(request, url, "image");
        }

        //public void DeleteLines(MessageModel request, int lineNum)
        //{
        //    CallClient(request, lineNum.ToString(), "delete", null, lineNum);
        //}
    }
}

#endif