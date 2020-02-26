using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Wing.Tools.ArgsParser
{
    public class Argument
    {
        public Type Type;
        public string ShotName;
        public string LongName;
        public string Description;
        public bool Optional;
        public object Value;
        public List<object> Values;
        public bool Parsed = false;
        public bool Switched = false;
        public bool SwitchedDefault = false;
        public bool Multiply = false;

        string mRegexStr = null;
        Regex mRegex;
        public string Regex
        {
            get
            {
                return mRegexStr;
            }
            set
            {
                if(mRegexStr != value)
                {
                    mRegexStr = value;

                    mRegex = new Regex(value);
                }
            }
        }
        public Argument()
        {
            
        }

        public void Reset()
        {
            Value = null;
            Values = null;
        }

        public Argument(Type type, string shotName,string longName, string description,bool optional)
        {
            Type = type;
            ShotName = shotName != null ? shotName : "";
            LongName = longName != null ? longName : "";
            Description = description != null ? description : "";
            Optional = optional;
        }

        public Argument(string shotName, string longName, string description, bool optional, bool switched, bool defaultValue)
        {
			ShotName = shotName != null ? shotName : "";
			LongName = longName != null ? longName : "";
			Description = description != null ? description : "";
			Optional = optional;

            Switched = switched;
            SwitchedDefault = defaultValue;
        }

        public string GetGroupName()
        {
            return string.Format("-{0}[--{1}]", ShotName, LongName);
        }

        public bool SetValue(string value, ref string reason)
        {
			if (Value == null)
			{
				if (value != null)
				{
					try
					{
                        if (!string.IsNullOrEmpty(Regex))
						{
                            if(!mRegex.IsMatch(value))
                            {
                                reason = GetGroupName() + "'s value " + value + " not compare to argument regex " + Regex;
                                return false;
                            }
						}

						if (Type != null)
						{
							Value = Convert.ChangeType(value, Type);
						}
						else
						{
							Value = value;
						}

						if (Multiply)
						{
							if (Values == null)
							{
								Values = new List<object>();
							}
							Values.Add(Value);
						}
					}
					catch
					{
						reason = "the value type of argument " + GetGroupName() + " is error!";
						return false;
					}
				}
			}
			else
			{
                if(!Multiply)
                {
                    reason = GetGroupName() + " is a not multiple value argument";
                    return false;
                }

				try
				{
					if (!string.IsNullOrEmpty(Regex))
					{
						if (!mRegex.IsMatch(value))
						{
							reason = GetGroupName() + "'s value " + value + " not compare to argument regex " + Regex;
							return false;
						}
					}

					if (Type != null)
					{
						var val = Convert.ChangeType(value, Type);
						Values.Add(val);
					}
					else
					{
						Values.Add(value);
					}
				}
				catch
				{
					reason = "the value type of argument " + GetGroupName() + " is error!";
					return false;
				}
			}

            return true;
        }
    }
}

