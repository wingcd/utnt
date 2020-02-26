using UnityEngine;
using UnityEditor;
using System.Net;
using System.Net.Sockets;
using Wing.Tools;
using System.Threading;
using System;
using System.Collections.Generic;
using System.Linq;

public class UTNTWindow : EditorWindow
{
    class UrlItem
    {
        public string Platform;
        public string Url;
        public float Time;
        public bool Delete = false;
    }

    [MenuItem("Tools/UTNT Window")]
    static void Init()
    {
        UTNTWindow window = (UTNTWindow)EditorWindow.GetWindow(typeof(UTNTWindow));
        window.Show();
    }
    
    static Socket socket;
    bool receiving = false;
    IPEndPoint iep;
    byte[] bytes = new byte[1024];
    string receiveData;
    List<UrlItem> mItems = new List<UrlItem>();

    public UTNTWindow()
    {
        receiving = false;
        iep = new IPEndPoint(IPAddress.Any, 8088);
    }

    void OnGUI()
    {
        GUILayout.BeginVertical();
        GUILayout.Label("UTNT Server List:");
        for (var i = 0; i < mItems.Count; i++)
        {
            var item = mItems[i];
            if (GUILayout.Button(item.Platform + "  " + item.Url))
            {
                Application.OpenURL("http://" + item.Url);
            }
        }
        GUILayout.EndVertical();
    }

    void Update()
    {
        if (socket == null)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Bind(iep);
        }
        listen();

        if (!string.IsNullOrEmpty(receiveData))
        {
            var strs = receiveData.Split(':');
            if (strs.Length == 3)
            {
                var url = strs[1] + ":" + strs[2];
                if (!mItems.Exists(it => it.Url == url))
                {
                    mItems.Add(new UrlItem
                    {
                        Platform = strs[0],
                        Url = url,
                        Time = Time.realtimeSinceStartup
                    });

                    Repaint();
                }
                else
                {
                    var item = mItems.First(it => it.Url == url);
                    item.Time = Time.realtimeSinceStartup;
                }
            }

            receiveData = null;
        }

        for (var i = 0; i < mItems.Count; i++)
        {
            var item = mItems[i];
            item.Delete = Time.realtimeSinceStartup - item.Time > 5;
        }

        var cnt = mItems.Count;
        mItems = mItems.Where(item => !item.Delete).ToList();
        if(cnt != mItems.Count)
        {
            Repaint();
        }
    }

    void listen()
    {
        if (!receiving)
        {
            receiving = true;
            EndPoint ep = (EndPoint)iep;
            socket.BeginReceiveFrom(bytes, 0, bytes.Length, SocketFlags.None, ref ep, onreceived, null);
        }
    }

    void onreceived(IAsyncResult ar)
    {
        if (ar.IsCompleted)
        {
            try
            {
                EndPoint ep = (EndPoint)iep;
                int recv_len = socket.EndReceiveFrom(ar, ref ep);

                if (recv_len > 0)
                {
                    var receivedBytes = new byte[bytes.Length];
                    Array.Copy(bytes, 0, receivedBytes, 0, recv_len);

                    receiveData = System.Text.Encoding.UTF8.GetString(receivedBytes);
                }
                else
                {
                    receiveData = null;
                }
            }
            catch(Exception ex)
            {
                receiveData = null;
            }
            receiving = false;
        }
    }
}