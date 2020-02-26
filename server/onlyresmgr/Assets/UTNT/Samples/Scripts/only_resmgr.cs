using UnityEngine;
using Wing.Tools.WebServer;

public class only_resmgr : MonoBehaviour {

	WebServer server = new WebServer();
	void Start()
	{
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
		//when you run in editor,plz add this
		Application.runInBackground = true;

        server.AddEditableFileExtion(".cfg");

		server.Start();
	}

	void Update()
	{
		
	}

	void OnApplicationQuit()
	{
		server.Stop();
	}
}
