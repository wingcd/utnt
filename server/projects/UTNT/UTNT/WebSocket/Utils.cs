using System;

namespace UTNT.WebSocketSharp
{
    public class Utils
    {
        static Utils mInstance = null;
        public static Utils Instance
        {
            get
            {
                if(mInstance == null)
                {
                    mInstance = new Utils();
                }
                return mInstance;
            }
        }

        string mInternetCacheDir = "";
        string mApplicationDataDir = "";
        string mLocalApplicationDataDir = "";
        public Utils()
        {
            mInternetCacheDir = Environment.GetFolderPath(Environment.SpecialFolder.InternetCache);
            mApplicationDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            mLocalApplicationDataDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        }

        public string InternetCacheDir
        {
            get
            {
                return mInternetCacheDir;
            }
            set
            {
                mInternetCacheDir = value;
            }
        }

        public string ApplicationDataDir
        {
            get
            {
                return mApplicationDataDir;
            }
            set
            {
                mApplicationDataDir = value;
            }
        }

        public string LocalApplicationDataDir
        {
            get
            {
                return mLocalApplicationDataDir;
            }
            set
            {
                mLocalApplicationDataDir = value;
            }
        }
    }
}