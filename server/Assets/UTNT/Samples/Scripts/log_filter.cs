using UnityEngine;
using Wing.Tools.WebServer;
using Wing.Tools.Terminal;
using Wing.Tools;
using System;

public class log_filter : MonoBehaviour
{
    WebServer server = new WebServer();
    float timer = 0;
    void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        //when you run in editor,plz add this
        Application.runInBackground = true;

        //for debug
        server.EnableCrossDomain = true;
        //HttpServer.Logging.LogFactory.Assign(new LogFactory());

        //add log worker to termial
        UTerminal.Instance.Regist(new LogWorker());
        server.Start();

        //termial default is close
        server.EnableTerminal(8087,"log -o -f timer", 1000);
    }

    // Update is called once per frame
    void Update()
    {
        if(timer > 5f)
        {
            Debug.Log("timer " + Time.realtimeSinceStartup);
            Debug.LogWarning("warning, run time " + Time.realtimeSinceStartup);
            Debug.LogAssertion("assertion");
            Debug.LogError("run time " + Time.realtimeSinceStartup);
            Debug.LogException(new Exception("excetion"));

            timer = 0;
        }
        timer += Time.deltaTime;
    }

    void OnApplicationQuit()
    {
        if (server != null)
        {
            server.Stop();
        }
    }
}
