using UnityEngine;
using System.Collections;
using Wing.Tools.ArgsParser;
using Wing.Tools.Terminal;
using System;

namespace Wing.Tools
{
    public class TransfomWorker : IWorker
    {
        class Args
        {
            public Argument Move = new Argument("m", "move", "move gameobject addtional", true, true, false);
            public Argument Rotate = new Argument("r", "rotate", "rotate gameobject addtional", true, true, false);
            public Argument Scale = new Argument("s", "scale", "scale gameobject addtional", true, true, false);
            public Argument Value = new Argument(null, "v", "value", "the direction of operation, format like this: (x,y,z)", false);
            public Argument To = new Argument("t", "to", "change move/rotate/rotate value to absolute", true, true, false);
        }

        ArgumentParser mParser = new ArgumentParser();
        GameObject mTarget = null;
        public TransfomWorker()
        {
            var args = new Args();
            mParser.Arguments.Add(args.Move);
            mParser.Arguments.Add(args.Rotate);
            mParser.Arguments.Add(args.Scale);
            mParser.Arguments.Add(args.Value);
            mParser.Arguments.Add(args.To);
        }

        public void SetTarget(GameObject go)
        {
            mTarget = go;
        }

        public string GetName()
        {
            return "trans";
        }

        public string Do(Connect conn, MessageModel request, string[] args)
        {
            if(mTarget == null)
            {
                return "empty target";
            }

            string reason = "";
            if(_doMove(args, ref reason))
            {
                
            }
            else if(_doRotate(args, ref reason))
            {
                
            }
            else if(_doScale(args, ref reason))
            {
                
            }

            return reason;
        }

        public void OnClose(Connect conn)
        {
            
        }

        public string Usage()
        {
            return mParser.Usage();
        }

        public string Description()
        {
            return "transform the target gameobject";
        }

        bool _parseValue(string val, ref Vector3 dir)
        {
            if(string.IsNullOrEmpty(val))
            {
                return false; 
            }

            var strs = val.Trim(new []{'(',')'}).Split(',');
            if(strs.Length != 3)
            {
                return false;
            }

            float x=0, y=0, z=0;
            if (!float.TryParse(strs[0], out x) ||
                !float.TryParse(strs[1], out y) ||
                !float.TryParse(strs[2], out z))
            {
                return false;
            }

            dir.x = x;
            dir.y = y;
            dir.z = z;
            return true;
        }

        string _toString(Vector3 value)
        {
            return string.Format("({0},{1},{2})", value.x, value.y, value.z);
        }

        bool _doMove(string[] args, ref string reason)
        {
            var pargs = new Args();
            pargs.Move.Optional = false;
            pargs.Value.Optional = false;
            pargs.To.Optional = true;

            var parser = new ArgumentParser();
            parser.Arguments.Add(pargs.Move);
            parser.Arguments.Add(pargs.Value);
            parser.Arguments.Add(pargs.To);

            if(!parser.Parse(args, ref reason))
            {
                return pargs.Move.Parsed;
            }

            Vector3 value = Vector3.zero;
            if(!_parseValue(pargs.Value.Value.ToString(), ref value))
            {
                reason = pargs.Value.GetGroupName() + " " + pargs.Value.Description;
                return true;
            }

            if(pargs.To.Parsed)
            {
                mTarget.transform.position = value;
            }
            else
            {
                mTarget.transform.position += value;
            }

            reason = "current position:" + _toString(mTarget.transform.position);

            return true;
        }

		bool _doRotate(string[] args, ref string reason)
		{
			var pargs = new Args();
            pargs.Rotate.Optional = false;
			pargs.Value.Optional = false;
			pargs.To.Optional = true;

			var parser = new ArgumentParser();
			parser.Arguments.Add(pargs.Rotate);
			parser.Arguments.Add(pargs.Value);
			parser.Arguments.Add(pargs.To);

			if (!parser.Parse(args, ref reason))
			{
				return pargs.Rotate.Parsed;
			}

			Vector3 value = Vector3.zero;
			if (!_parseValue(pargs.Value.ToString(), ref value))
			{
				reason = pargs.Value.GetGroupName() + " " + pargs.Value.Description;
				return true;
			}

			if (pargs.To.Parsed)
			{
                mTarget.transform.rotation = Quaternion.FromToRotation(Vector3.forward, value);
			}
			else
			{
                mTarget.transform.Rotate(value);
			}

            reason = "current rotate:" + _toString(mTarget.transform.rotation.eulerAngles);

			return true;
		}

		bool _doScale(string[] args, ref string reason)
		{
			var pargs = new Args();
            pargs.Scale.Optional = false;
			pargs.Value.Optional = false;
			pargs.To.Optional = true;

			var parser = new ArgumentParser();
			parser.Arguments.Add(pargs.Scale);
			parser.Arguments.Add(pargs.Value);
			parser.Arguments.Add(pargs.To);

			if (!parser.Parse(args, ref reason))
			{
				return pargs.Scale.Parsed;
			}

			Vector3 value = Vector3.zero;
			if (!_parseValue(pargs.Value.ToString(), ref value))
			{
                reason = pargs.Value.GetGroupName() + " " + pargs.Value.Description;
				return true;
			}

			if (pargs.To.Parsed)
			{
                mTarget.transform.localScale = value;
			}
			else
			{
                mTarget.transform.localScale += value;
			}

            reason = "current scale:" + _toString(mTarget.transform.localScale);

			return true;
		}
    }
}
