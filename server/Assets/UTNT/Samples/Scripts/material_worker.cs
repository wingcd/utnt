using UnityEngine;
using Wing.Tools.ArgsParser;
using Wing.Tools.Terminal;
using Wing.Tools.WebServer;

public class material_worker : MonoBehaviour {
    class MaterialWorker : IWorker
    {
        class Args
        {
            public Argument Color = new Argument(null, "c", "color", "set material's color,format is byte array like (r,g,b,a)", true);
        }
        ArgumentParser mParser = new ArgumentParser();
        Material mTarget;
        public MaterialWorker()
        {
            var args = new Args();
            mParser.Arguments.Add(args.Color);
        }

        public void SetTarget(Material target)
        {
            mTarget = target;
        }

        public string Description()
        {
            return "change target material's color";
        }

        public string Do(Connect conn, MessageModel request, string[] args)
        {
            if (mTarget == null)
            {
                return "empty target";
            }

            string reason = "";
            mParser.Arguments[0].Reset();
            if (mParser.Parse(args, ref reason))
            {
                var color = mParser.Arguments[0].Value.ToString();
                var strs = color.Trim(new[] { '(',')'}).Split(',');
                if(strs.Length == 4)
                {
                    byte r=0, g=0, b=0, a=0;
                    if(!byte.TryParse(strs[0], out r) ||
                        !byte.TryParse(strs[1], out g) ||
                         !byte.TryParse(strs[2], out b) ||
                          !byte.TryParse(strs[3], out a))
                    {
                        reason = "value type error";
                    }
                    else
                    {
                        mTarget.color = new Color32(r, g, b, a);
                        reason = "done";
                    }
                }
                else
                {
                    reason = "value format error";
                }
            }

            return reason;
        }

        public string GetName()
        {
            return "mat";
        }

        public void OnClose(Connect conn)
        {
            
        }

        public string Usage()
        {
            return mParser.Usage();
        }
    }


    public Renderer Target;
	WebServer server = new WebServer();
	void Start()
	{
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
		//when you run in editor,plz add this
		Application.runInBackground = true;

        var trans = new MaterialWorker();
        trans.SetTarget(Target.sharedMaterial);
        
        UTerminal.Instance.Regist(trans);
		server.Start();
		server.EnableTerminal(8087, "mat --color (255,0,0,255)");
	}

	void Update()
	{
		
	}

	void OnApplicationQuit()
	{
		server.Stop();
	}
}
