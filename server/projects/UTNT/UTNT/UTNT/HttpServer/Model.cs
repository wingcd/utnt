using UnityEngine;
using System.Collections.Generic;
using Wing.Tools.Utils;

namespace Wing.Tools.WebServer
{
    [System.Serializable]
    public class Info
    {
        [SerializeField]
        public int error = 0;
        [SerializeField]
        public object data;
    }
    
    [System.Serializable]
    public class DirInfo
    {
        [SerializeField]
        public string name;
        [SerializeField]
        public EDirType dirtype;
        [SerializeField]
        public bool isdir;
        [SerializeField]
        public long size;
        [SerializeField]
        public string modifytime;
        [SerializeField]
        public bool editable;
        [SerializeField]
        public string filetype;
        [SerializeField]
		public bool renameable;
        [SerializeField]
        public string path;
        [SerializeField]
        public int childnum;
        [SerializeField]
        public List<DirInfo> children;
    }

    [System.Serializable]
    public class DirInfoList
    {
        [SerializeField]
        public List<DirInfo> children;
    }

    [System.Serializable]
    public class RootInfo
    {
        [SerializeField]
        public string platform;
        [SerializeField]
        public List<DirInfo> children;
    }

    [System.Serializable]
    public class TerminalInfo
	{
		[SerializeField]
		public bool enable;
		[SerializeField]
		public int port;
        [SerializeField]
        public string defaultInput;
		[SerializeField]
		public int maxRow;
	}
}