using UnityEngine;
using Wing.Tools;
using Wing.Tools.Terminal;
using Wing.Tools.WebServer;

public class transform_worker : MonoBehaviour {

    public GameObject Target;
	WebServer server = new WebServer();
	void Start()
	{
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
		//when you run in editor,plz add this
		Application.runInBackground = true;

        var trans = new TransfomWorker();
        trans.SetTarget(Target);
        
        UTerminal.Instance.Regist(trans);
		server.Start();
		server.EnableTerminal(8087, "trans --move -v (1,0,0)");
	}

	void Update()
	{
		
	}

	void OnApplicationQuit()
	{
		server.Stop();
	}
}
