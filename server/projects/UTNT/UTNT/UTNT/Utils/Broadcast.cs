using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Linq;

namespace Wing.Tools.Utils
{
    public class Broadcast
    {
		private static Broadcast sInstance;
		public static Broadcast Instance
		{
			get
			{
				if (sInstance == null)
				{
					sInstance = new Broadcast();
				}
				return sInstance;
			}
		}

		private Broadcast()
		{

		}

        Socket socket;
        IPEndPoint iep;
        bool mTick = false;
		public void Init(int port = 8088)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

			iep = new IPEndPoint(IPAddress.Broadcast, port);
			//设置broadcast值为1，允许套接字发送广播信息
			socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
        }

        public void Send(string msg)
        {
            if (socket != null)
            {
                //将发送内容转换为字节数组
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(msg);
                //向子网发送信息
                socket.SendTo(bytes, iep);
            }
        }

        public void Close()
		{
			StopTick();
            if (socket != null)
            {
                socket.Close();
                socket = null;
            }
		}

        public void Tick(string msg, float interval)
        {
            mTick = true;
            tick(msg, interval);
        }

        public void StopTick()
        {
            mTick = false;
        }

        public void tick(string msg, float interval)
        {
            if(!mTick)
            {
                return;
            }

            CoroutineProvider.Instance.Delay(interval,()=>{
                Send(msg);

                tick(msg, interval);
            });
        }

		/// <summary> 
		/// 获得广播地址 
		/// </summary> 
		/// <param name="ipAddress">IP地址</param> 
		/// <param name="subnetMask">子网掩码</param> 
		/// <returns>广播地址</returns> 
		public static string GetBroadcast(string ipAddress, string subnetMask)
		{

			byte[] ip = IPAddress.Parse(ipAddress).GetAddressBytes();
			byte[] sub = IPAddress.Parse(subnetMask).GetAddressBytes();

			// 广播地址=子网按位求反 再 或IP地址 
			for (int i = 0; i < ip.Length; i++)
			{
				ip[i] = (byte)((~sub[i]) | ip[i]);
			}
			return new IPAddress(ip).ToString();
		}
    }
}
