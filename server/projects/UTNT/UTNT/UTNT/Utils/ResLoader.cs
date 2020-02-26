using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Wing.Tools.Utils
{
    public class LoadAsset
    {
        private string m_sPath = string.Empty;//资源路径
        private WWW m_www = null;

        private int m_iCountRetry = 1;//重复下载次数

        public LoadAsset(string path)
        {
            m_sPath = path;
        }

        public WWW StarLoad()
        {
            WWW result = null;

            while (result == null || !result.isDone)
            {
                foreach (WWW obj in LoadWWW())
                {
                    result = obj;
                }
                if (m_iCountRetry > 0)
                {
                    if (result != null && result.isDone)
                    {
                        break;
                    }
                    m_iCountRetry--;
                }
                else
                {
                    break;
                }
            }
            //DeInit();
            return result;
        }


        public IEnumerable<WWW> LoadWWW()
        {
            m_www = new WWW(m_sPath);
            yield return m_www;
        }


        private void DeInit()
        {
            if (m_www != null)
            {
                m_www.Dispose();
            }
            m_www = null;
        }
    }


    public class ResLoad
    {
        private static ResLoad instance = null;
        public static ResLoad Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ResLoad();
                }
                return instance;
            }
        }
        public WWW StarLoad(string path)
        {
            WWW obj = null;
            LoadAsset asset = new LoadAsset(path);
            obj = asset.StarLoad();
            return obj;
        }
    }
}