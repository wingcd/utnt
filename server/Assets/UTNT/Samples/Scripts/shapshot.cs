using UnityEngine;
using Wing.Tools.WebServer;
using Wing.Tools.Terminal;
using Wing.Tools;
using System;

public class shapshot : MonoBehaviour
{
    WebServer server = new WebServer();
    void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        //when you run in editor,plz add this
        Application.runInBackground = true;

        server.EnableCrossDomain = true;

        UTerminal.Instance.Regist(new SnapshotWorker());
        UTerminal.Instance.Regist(new LogWorker());
        server.Start();
        server.EnableTerminal(8087,"shapshot");
    }

    void OnApplicationQuit()
    {
        server.Stop();
    }
}
