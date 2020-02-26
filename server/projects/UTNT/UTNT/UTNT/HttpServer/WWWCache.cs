using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using Wing.Tools.Utils;

namespace Wing.Tools.WebServer
{
    public class WWWCache
    {
        static WWWCache sInstance;
        public static WWWCache Instance
        {
            get
            {
                if (sInstance == null)
                {
                    sInstance = new WWWCache();
                }
                return sInstance;
            }
        }

        class Item
        {
            public string key;
            public object data;
        }

        public string CustomPath = "__custom__";

        Dictionary<string, byte[]> mItems = null;
        Dictionary<string, Item> mCustomItems = new Dictionary<string, Item>();
        private WWWCache()
        {
            
        }

        public void Load(string zip, Action done, Action error, string psw = null)
        {
            mItems = new Dictionary<string, byte[]>();
            Zipper zipper = new Zipper();
            var file = UtilsHelper.WWWStreamAssetsPath + zip;

            UtilsHelper.WWWLoad(file, (www) => 
            {
                if(!string.IsNullOrEmpty(www.error))
                {
                    if(error!= null)
                    {
                        error();
                    }
                    return;
                }
                
                zipper.AsyncUnzipToMemary(www.bytes, mItems, psw);
				zipper.Finished += (result) =>
				{
					if (done != null)
					{
						done();
					}
				};
            });
        }

        string getCustomKey(string key)
        {
            if(!key.StartsWith("/"))
            {
                key = "/" + key;
            }
            return CustomPath + key;
        }

        public string AddCustomData(string key, string filename)
        {
            key = getCustomKey(key);

            mCustomItems[key] = new Item
            {
                key = key,
                data = filename,
            };

            return key;
        }

        public string AddCustomData(string key, byte[] data)
        {
            key = getCustomKey(key);

            mCustomItems[key] = new Item
            {
                key = key,
                data = data,
            };

            return key;
        }

        public void RemoveCustomData(string key)
        {
            mCustomItems.Remove(key);
        }

        public byte[] GetCustomItem(string key)
        {
			if (mCustomItems.ContainsKey(key))
			{
				var item = mCustomItems[key];
				if (item.data != null)
				{
					if (item.data is byte[])
					{
						return (byte[])item.data;
					}
					else if (item.data is string)
					{
						var fn = (string)item.data;
						if (File.Exists(fn))
						{
							return File.ReadAllBytes(fn);
						}
					}
				}
			}
			return null;
        }

        public byte[] GetItem(string key)
        {
            if(mItems == null || !mItems.ContainsKey(key))
            {
                return null;
            }
            else
            {
                return mItems[key];
            }
        }
    }
}
