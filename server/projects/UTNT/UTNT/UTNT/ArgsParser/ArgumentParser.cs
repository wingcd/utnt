using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;

namespace Wing.Tools.ArgsParser
{
	public class ArgumentParser
	{
        List<Argument> mArguments = new List<Argument>();
        public List<Argument> Arguments
        {
            get
            {
                return mArguments;
            }
        }

        public bool Parse(string[] args, ref string reason)
        {
            if (args == null)
            {
                reason = "empty arguments";
                return false;
            }

            var temp = new List<Argument>(Arguments);
            Argument argument = null;
            bool optionanl = false;

            var name = "";
            var scope = false;
            for (var index = 0; index < args.Length; index++)
            {
				var val = args[index];
				if (val.StartsWith("\""))
				{
					name = val.Trim(new[] { '"' });
					scope = true;
					continue;
				}
				else if (val.EndsWith("\"") && scope)
				{
					name += " " + val.Trim(new[] { '"' });
					scope = false;
				}
				else if (scope)
				{
					name += " " + val;
					continue;
				}
				else
				{
					name = val;
				}
                 
                if (name.StartsWith("--"))
                {
                    if(argument != null && !argument.Switched && argument.Value == null)
                    {
                        reason = "not set value to argument " + argument.GetGroupName();
                        return false;
                    }

                    name = name.Substring(2);
                    var list = temp.Where(arg => arg.LongName == name).ToList();
                    if (list.Count > 0)
                    {
                        argument = list[0];
                        optionanl = true;
                    }
                    else
                    {
                        reason = "unknow parameter:" + name;
                        return false;
                    }
                }
                else if (name.StartsWith("-"))
                {
					if (argument != null && !argument.Switched && argument.Value == null)
					{
						reason = "not set value to argument " + argument.GetGroupName();
						return false;
					}

                    name = name.Substring(1);
                    var list = temp.Where(arg => arg.ShotName == name).ToList();
                    if (list.Count > 0)
                    {
                        argument = list[0];
                        optionanl = true;
                    }
					else
					{
						reason = "unknow parameter:" + name;
						return false;
					}
                }
                else
                {
                    if (argument != null)
                    {
                        if(argument.SetValue(name, ref reason))
                        {
                            argument.Parsed = true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }

                if (argument == null)
                {
                    reason = "unkonw argument " + args[index];
                    return false;
                }

                if (optionanl && argument != null && argument.Switched)
                {
                    argument.Parsed = true;
                    argument.Value = !argument.SwitchedDefault;
                    argument = null;
                }
            }

            var requireds = mArguments
                .Where(arg => !arg.Optional && !arg.Parsed)
                .Select(arg=> arg.GetGroupName())
                .ToArray();
            if(requireds.Length > 0)
            {
                reason = "not set parameters' value or missing parameters " + string.Join(", ", requireds);
                return false;
            }

            return true;
        }

        public string Usage()
        {
            var requireds = mArguments.Where(arg => !arg.Optional && !arg.Parsed).ToList();
            var optionals = mArguments.Where(arg => arg.Optional && !arg.Parsed).ToList();

            var result = new StringBuilder();
            result.Append("Usage:\n");
            var temp = new List<Argument>();
            temp.AddRange(requireds);
            temp.AddRange(optionals);
            for (var i = 0; i < temp.Count;i++)
            {
                var arg = temp[i];
                result.Append(string.Format("\t-{0}\t--{1} [{2}]...{3}\n", 
                                            arg.ShotName,
                                            arg.LongName,
                                            arg.Optional ? "Optional" : "Required",
                                            arg.Description));
            }

            if(temp.Count == 0)
            {
                result.Append("No arguments");
            }

            return result.ToString();
        }
	}
}
