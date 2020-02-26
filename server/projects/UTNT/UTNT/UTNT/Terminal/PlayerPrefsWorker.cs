#if !DISABLE_TERMINAL
using UnityEngine;
using System.Collections;
using System;
using System.Linq;
using System.IO;
using Wing.Tools.ArgsParser;
using System.Collections.Generic;

namespace Wing.Tools.Terminal
{
    public class PlayerPrefsWorker : IWorker
    {
        class Args
        {
            public Argument Get = new Argument("g", "get", "get value by key", true, true, false);
            public Argument Set = new Argument("s", "set", "set value by key", true, true, false);
            public Argument Delete = new Argument("d", "delete", "delete value by key", true, true, false);
            public Argument Has = new Argument("h", "has", "query has the key", true, true, false);

            public Argument All = new Argument("a", "all", "for all key, just verify when delete", true, true, false);

            public Argument Key = new Argument(null, "k", "key", "the key", true);
            public Argument Value = new Argument(null, "v", "value", "the value, type can be int/float/string", true);
            public Argument Type = new Argument(null, "t", "type","the type must be in [int,float,string]", true);

            public Args()
            {
                Type.Regex = "^(int|float|string){1}$";
            }
        }

		ArgumentParser mParser = new ArgumentParser();
        public PlayerPrefsWorker()
        {
            Args args = new Args();
            mParser.Arguments.Add(args.Get);
            mParser.Arguments.Add(args.Set);
            mParser.Arguments.Add(args.Delete);
            mParser.Arguments.Add(args.Has);
            mParser.Arguments.Add(args.All);
            mParser.Arguments.Add(args.Key);
            mParser.Arguments.Add(args.Value);
            mParser.Arguments.Add(args.Type);
        }

        public string Usage()
        {
            return mParser.Usage();
        }

        public string Description()
        {
            return "prefs: is a tool to manager PlayerPrefs";
        }

        //prefs -g int -k key
        //prefs -s int -k key -v value
        //prefs -d [-all]
        public string Do(Connect conn, MessageModel request, string[] args)
        {
            try
            {
                string result = null;
                if (_doGet(args, ref result))
                {

                }
                else if (_doAdd(args, ref result))
                {

                }
                else if(_doDelete(args, ref result))
                {

                }
                else if(_doQuery(args, ref result))
                {

                }
                else
                {
                    result = Usage();
                }

                return result;
            }
            catch(Exception ex)
            {
                return ex.Message;
            }
        }

        public string GetName()
        {
            return "prefs";
        }

        bool _doGet(string[] args, ref string data)
        {
            ArgumentParser parser = new ArgumentParser();
			Args pargs = new Args();
            pargs.Get.Optional = false;
            pargs.Type.Optional = false;
            pargs.Key.Optional = false;

			parser.Arguments.Add(pargs.Get);
			parser.Arguments.Add(pargs.Key);
            parser.Arguments.Add(pargs.Type);
                        
            if(!parser.Parse(args, ref data))
            {
                return pargs.Get.Parsed;
            }

            var key = pargs.Key.Value.ToString();
            if (PlayerPrefs.HasKey(key))
            {
                var t = pargs.Type.Value.ToString();
                if(t == "int"){
                    data = PlayerPrefs.GetInt(key).ToString();
                }else if(t == "float"){
                    data = PlayerPrefs.GetFloat(key).ToString();
                }else if(t == "string"){
                    data = PlayerPrefs.GetString(key);
                }else{
                    data = pargs.Type.Description + " you set is " + t;
                }
            }
            else
            {
				data = "this key not exists:" + pargs.Key.Value;
            }

            return true;
        }
		bool _doAdd(string[] args, ref string data)
		{
            ArgumentParser parser = new ArgumentParser();
            Args pargs = new Args();
			pargs.Set.Optional = false;
			pargs.Type.Optional = false;
			pargs.Key.Optional = false;
            pargs.Value.Optional = false;

			parser.Arguments.Add(pargs.Set);
			parser.Arguments.Add(pargs.Key);
			parser.Arguments.Add(pargs.Type);
            parser.Arguments.Add(pargs.Value);
            
            if (!parser.Parse(args, ref data))
            {
                return pargs.Set.Parsed;
            }

            var t = pargs.Type.Value.ToString();
            var key = pargs.Key.Value.ToString();
            data = key+"=" + pargs.Value.Value;
			if (t == "int")
			{
				var val = 0;
                if (int.TryParse(pargs.Value.Value.ToString(), out val))
				{
					PlayerPrefs.SetInt(key, val);
				}
				else
				{
                    data = pargs.Value.Description;
                    return true;
				}
			}
			else if (t == "float")
			{
				var val = 0f;
                if (float.TryParse(pargs.Value.Value.ToString(), out val))
				{
					PlayerPrefs.SetFloat(key, val);
				}
				else
				{
                    data = pargs.Value.Description;
                    return true;
				}
			}
			else if (t == "string")
			{
                PlayerPrefs.SetString(pargs.Key.Value.ToString(), pargs.Value.Value.ToString());
			}
			else
			{
                data = pargs.Type.Description + " you set is " + t;
            }

			return true;
		}
        bool _doDelete(string[] args, ref string data)
        {
            ArgumentParser parser = new ArgumentParser();
            Args pargs = new Args();
            pargs.Delete.Optional = false;
            pargs.Key.Multiply = true;

            parser.Arguments.Add(pargs.Delete);
            parser.Arguments.Add(pargs.Key);
            parser.Arguments.Add(pargs.All);

            if (!parser.Parse(args, ref data))
            {
                return pargs.Delete.Parsed;
            }

            if (!pargs.Key.Parsed && !pargs.All.Parsed)
            {
                data = parser.Usage();
                return true;
            }

            if (pargs.Key.Parsed)
            {
                data = "";
                for (var i = 0; i < pargs.Key.Values.Count; i++)
                {
                    var key = pargs.Key.Values[i].ToString();
                    PlayerPrefs.DeleteKey(key);
                    data += "delete key:" + key + "\n";
                }
            }
            else if(pargs.All.Parsed)
            {
                PlayerPrefs.DeleteAll();
                data = "delete all keys";
            }
            return true;
        }
        bool _doQuery(string[] args, ref string data)
        {
            ArgumentParser parser = new ArgumentParser();
            Args pargs = new Args();
            pargs.Has.Optional = false;
            pargs.Key.Optional = false;

            parser.Arguments.Add(pargs.Has);
            parser.Arguments.Add(pargs.Key);

            if (!parser.Parse(args, ref data))
            {
                return pargs.Has.Parsed;
            }

            data = PlayerPrefs.HasKey(pargs.Key.Value.ToString()).ToString();

            return true;
        }

        public void OnClose(Connect conn)
        {

        }
    }

}

#endif
