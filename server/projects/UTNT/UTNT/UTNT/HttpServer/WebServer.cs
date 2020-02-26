using UnityEngine;
using System.Collections;
using UTNT.HttpServer;
using UTNT.HttpServer.Modules;
using UTNT.HttpServer.BodyDecoders;
using UTNT.HttpServer.Routing;
using System.Text;
using UTNT.HttpServer.Headers;
using System.Collections.Generic;
using System.IO;
using System;
using Wing.Tools.Utils;

namespace Wing.Tools.WebServer
{
    public class WebServer
    {
        static string psw = "wwwing123";
        private bool mInitialed = false;

        private bool mRunning = false;
        Server mServer = new Server();
        string mTerminalDefaultInput = "";
        List<string> mEdiableExt = new List<string>();
        bool mEnableCrossDomain = false;
        int mMaxTerimalRow = 1000;

        void init(int port)
        {
            if (mInitialed)
            {
                return;
            }

            Loom.Current.Initial();
            UtilsHelper.Init();

            UTNT.HttpServer.Utils.Instance.InternetCacheDir = UtilsHelper.TemporaryCachePath;
            //HttpServer.Utils.Instance.ApplicationDataDir = UtilsHelper.TemporaryCachePath;
            //HttpServer.Utils.Instance.LocalApplicationDataDir = UtilsHelper.TemporaryCachePath;

            AddEditableFileExtion(".txt");
            AddEditableFileExtion(".lua");
            AddEditableFileExtion(".xml");

            //HttpServer.Logging.LogFactory.Assign(new LogFactory());

            mServer.MaxContentSize = 1024 * 1024 * 1024;
            mServer.ContentLengthLimit = 1024 * 1024 * 1024;

            var module = new FileModule();
            module.ContentTypes.Add("svg", new ContentTypeHeader("image/svg+xml"));
            //复杂的url必须先注册
            var loader = new UintyStreamAssetsLoader();
            loader.Add("/" + WWWCache.Instance.CustomPath, "/" + WWWCache.Instance.CustomPath, true);
            loader.Add("/", "/www/");

            module.Resources.Add(loader);

            mServer.Add(module);
            mServer.Add(new MultiPartDecoder());

            // use one http listener.
            mServer.Add(HttpListener.Create(System.Net.IPAddress.Any, port));
            mServer.Add(new SimpleRouter("/", "/index.html"));

            mServer.Add(new HandleRouter("/api/getrootdir", getRootDirHandler));
            mServer.Add(new HandleRouter("/api/getdir", getDirHandler));
            mServer.Add(new HandleRouter("/api/rename", renameHandler));
            mServer.Add(new HandleRouter("/api/addfold", addFoldHandler));
            mServer.Add(new HandleRouter("/api/addfile", addFileHandler));
            mServer.Add(new HandleRouter("/api/delete", deleteHandler));
            mServer.Add(new HandleRouter("/api/replacefile", repaceFileHandler));
            mServer.Add(new HandleRouter("/api/setcontent", setContentHandler));
            mServer.Add(new HandleRouter("/api/unzip", unzipHandler));
            mServer.Add(new HandleRouter("/api/getfile", getFileHandler));

            mServer.Add(new HandleRouter("/api/terminalinfo", getTerminalInfoHandler));

            mInitialed = true;
        }

        private string getTypeResDir(EDirType dirType)
        {
            return string.Format("/res/type_{0}", (int)dirType);
        }

        public void AddEditableFileExtion(string ext)
        {
            if (ext == null)
            {
                return;
            }

            if (!ext.StartsWith("."))
            {
                ext = "." + ext.ToLower();
            }
            mEdiableExt.Add(ext);
        }

        public bool EnableCrossDomain
        {
            get
            {
                return mEnableCrossDomain;
            }
            set
            {
                mEnableCrossDomain = value;
            }
        }

        public Server Server
        {
            get
            {
                return mServer;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="port"></param>
        /// <param name="penddingConnNum">Number of pending connections</param>
        public void Start(int port = 8086, int penddingConnNum = 5)
        {
            if (mRunning)
            {
                return;
            }
            mRunning = true;

            init(port);
            WWWCache.Instance.Load("www.zip", () =>
            {
                mServer.Start(penddingConnNum);
                Debug.Log("http server running on port:" + port);

                Broadcast.Instance.Init();
                Broadcast.Instance.Tick(UtilsHelper.GetPlatform() + ":" + Network.player.ipAddress + ":" + port.ToString(), 1f);
            },
            () =>
            {

            }, psw);
        }
        public void Stop()
        {
            if (!mRunning)
            {
                return;
            }

            if (mServer != null)
            {
                mServer.Stop(true);
            }

#if !DISABLE_TERMINAL
            DisableTerminal();
#endif

            Broadcast.Instance.Close();
        }

        public void EnableTerminal(int port = 8087, string defaultInput = "", int maxRow = 1000)
        {
#if !DISABLE_TERMINAL
            Terminal.UTerminal.Instance.Start(port);
            mTerminalDefaultInput = defaultInput;
            mMaxTerimalRow = maxRow;
#endif
        }

#if !DISABLE_TERMINAL
        public void DisableTerminal()
        {
            Terminal.UTerminal.Instance.Stop();
        }
#endif

        private void enableCrossDomain(IResponse response)
        {
            response.Add(new StringHeader("Access-Control-Allow-Origin", "*"));
        }

        private void sendString(IResponse response, string data)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(data);
            response.Body.Write(buffer, 0, buffer.Length);
        }

        private void sendData(IResponse response, object data)
        {
            if (data != null)
            {
                var json = JsonUtility.ToJson(data);
                sendString(response, json);
            }
            else
            {
                sendString(response, "{}");
            }
        }

        #region handlers

        private void getPlatformHandler(IRequest request, IResponse response)
        {
            if (EnableCrossDomain)
            {
                enableCrossDomain(response);
            }

            sendString(response, "{\"platform\":\"" + UtilsHelper.GetPlatform() + "\"}");
        }
        private void getRootDirHandler(IRequest request, IResponse response)
        {
            if (EnableCrossDomain)
            {
                enableCrossDomain(response);
            }

            var dirList = getRootInfo();
            sendData(response, dirList);
        }
        private void getDirHandler(IRequest request, IResponse response)
        {
            if (EnableCrossDomain)
            {
                enableCrossDomain(response);
            }

            if (request.QueryString == null)
            {
                response.Status = System.Net.HttpStatusCode.BadRequest;
                return;
            }

            var type = request.QueryString.Get("type");
            var path = request.QueryString.Get("path");
            if (type == null || path == null)
            {
                response.Status = System.Net.HttpStatusCode.BadRequest;
                return;
            }
            var dirType = 0;
            var pathStr = UnescapeDataString(path.Value);
            if (!int.TryParse(type.Value, out dirType))
            {
                response.Status = System.Net.HttpStatusCode.BadRequest;
                return;
            }

            var dirList = getDirInfo((EDirType)dirType, pathStr);
            sendData(response, dirList);
        }

        private void addFoldHandler(IRequest request, IResponse response)
        {
            if (EnableCrossDomain)
            {
                enableCrossDomain(response);
            }

            if (request.QueryString == null)
            {
                response.Status = System.Net.HttpStatusCode.BadRequest;
                return;
            }

            var type = request.QueryString.Get("type");
            var path = request.QueryString.Get("path");
            var name = request.QueryString.Get("name");
            if (type == null || path == null || name == null)
            {
                response.Status = System.Net.HttpStatusCode.BadRequest;
                return;
            }

            var dirType = 0;
            var pathStr = UnescapeDataString(path.Value);
            var nameStr = UnescapeDataString(name.Value);
            if (!int.TryParse(type.Value, out dirType))
            {
                response.Status = System.Net.HttpStatusCode.BadRequest;
                return;
            }
            if (!canEdit((EDirType)dirType))
            {
                response.Status = System.Net.HttpStatusCode.BadRequest;
                return;
            }

            var dir = getDir((EDirType)dirType);
            var basePath = Path.GetDirectoryName(pathStr);
            if (basePath == null || basePath == "/")
            {
                basePath = "";
            }
            var src = dir + pathStr;
            var dst = src + "/" + nameStr;

            try
            {
                if (File.Exists(src))
                {
                    response.Status = System.Net.HttpStatusCode.BadRequest;
                    return;
                }
                else if (Directory.Exists(src))
                {
                    Directory.CreateDirectory(dst);
                }
                else
                {
                    response.Status = System.Net.HttpStatusCode.BadRequest;
                    return;
                }
            }
            catch (Exception ex)
            {
                response.Status = System.Net.HttpStatusCode.BadRequest;
                return;
            }

            var info = getInfo((EDirType)dirType, pathStr);
            if (info != null)
            {
                sendData(response, info);
            }
            else
            {
                response.Status = System.Net.HttpStatusCode.BadRequest;
                return;
            }
        }

        private void renameHandler(IRequest request, IResponse response)
        {
            if (EnableCrossDomain)
            {
                enableCrossDomain(response);
            }

            if (request.QueryString == null)
            {
                response.Status = System.Net.HttpStatusCode.BadRequest;
                return;
            }

            var type = request.QueryString.Get("type");
            var path = request.QueryString.Get("path");
            var name = request.QueryString.Get("name");
            if (type == null || path == null || name == null)
            {
                response.Status = System.Net.HttpStatusCode.BadRequest;
                return;
            }

            var dirType = 0;
            var pathStr = UnescapeDataString(path.Value);
            var nameStr = UnescapeDataString(name.Value);
            if (!int.TryParse(type.Value, out dirType))
            {
                response.Status = System.Net.HttpStatusCode.BadRequest;
                return;
            }
            if (!canEdit((EDirType)dirType) && pathStr != "/")
            {
                response.Status = System.Net.HttpStatusCode.BadRequest;
                return;
            }

            var dir = getDir((EDirType)dirType);
            var basePath = Path.GetDirectoryName(pathStr);
            if (basePath == null || basePath == "/")
            {
                basePath = "";
            }
            var src = dir + pathStr;
            var dst = dir + basePath + "/" + nameStr;

            try
            {
                if (File.Exists(src))
                {
                    File.Move(src, dst);
                }
                else if (Directory.Exists(src))
                {
                    Directory.Move(src, dst);
                }
                else
                {
                    response.Status = System.Net.HttpStatusCode.BadRequest;
                    return;
                }
            }
            catch
            {
                response.Status = System.Net.HttpStatusCode.BadRequest;
                return;
            }

            var info = getInfo((EDirType)dirType, basePath + "/" + nameStr);
            if (info != null)
            {
                sendData(response, info);
            }
            else
            {
                response.Status = System.Net.HttpStatusCode.BadRequest;
                return;
            }
        }

        private void deleteHandler(IRequest request, IResponse response)
        {
            if (EnableCrossDomain)
            {
                enableCrossDomain(response);
            }

            if (request.QueryString == null)
            {
                response.Status = System.Net.HttpStatusCode.BadRequest;
                return;
            }

            var type = request.QueryString.Get("type");
            var path = request.QueryString.Get("path");
            if (type == null || path == null)
            {
                response.Status = System.Net.HttpStatusCode.BadRequest;
                return;
            }

            var dirType = 0;
            var pathStr = UnescapeDataString(path.Value);
            if (!int.TryParse(type.Value, out dirType))
            {
                response.Status = System.Net.HttpStatusCode.BadRequest;
                return;
            }

            var edit = canEdit((EDirType)dirType) && pathStr != "/";
            if (!edit)
            {
                response.Status = System.Net.HttpStatusCode.BadRequest;
                return;
            }

            var dir = getDir((EDirType)dirType);
            var fullname = dir + pathStr;
            try
            {
                if (Directory.Exists(fullname))
                {
                    Directory.Delete(fullname, true);
                }
                else if (File.Exists(fullname))
                {
                    File.Delete(fullname);
                }
            }
            catch
            {
                response.Status = System.Net.HttpStatusCode.BadRequest;
                return;
            }

            var baseDir = Path.GetDirectoryName(pathStr);
            if (baseDir == null)
            {
                baseDir = "/";
            }
            var info = getDirInfo((EDirType)dirType, baseDir);
            sendData(response, info);
        }

        private void getFileHandler(IRequest request, IResponse response)
        {
            if (EnableCrossDomain)
            {
                enableCrossDomain(response);
            }

            if (request.QueryString == null)
            {
                response.Status = System.Net.HttpStatusCode.BadRequest;
                return;
            }

            var type = request.QueryString.Get("type");
            var path = request.QueryString.Get("path");
            if (type == null || path == null)
            {
                response.Status = System.Net.HttpStatusCode.BadRequest;
                return;
            }

            var dirType = 0;
            var pathStr = UnescapeDataString(path.Value);
            if (!int.TryParse(type.Value, out dirType))
            {
                response.Status = System.Net.HttpStatusCode.BadRequest;
                return;
            }

            var dir = getDir((EDirType)dirType);
            var fullname = dir + pathStr;
            if (!File.Exists(fullname))
            {
                response.Status = System.Net.HttpStatusCode.NotFound;
                return;
            }

            FileInfo finfo = new FileInfo(fullname);
            response.Add(new StringHeader("Content-Disposition", "attachment; filename=" + finfo.Name));
            response.Add(new StringHeader("Content-Length", finfo.Length.ToString()));
            response.ContentType = new ContentTypeHeader(ContentTypeHelper.GetType(finfo.Extension.ToLower()));
            var data = File.ReadAllBytes(fullname);
            response.Body.Write(data, 0, data.Length);
        }

        private void repaceFileHandler(IRequest request, IResponse response)
        {
            if (EnableCrossDomain)
            {
                enableCrossDomain(response);
            }

            if (request.QueryString == null || request.Files == null)
            {
                response.Status = System.Net.HttpStatusCode.BadRequest;
                return;
            }

            var type = request.QueryString.Get("type");
            var path = request.QueryString.Get("path");
            var file = request.Files["file"];
            if (type == null || path == null || file == null)
            {
                response.Status = System.Net.HttpStatusCode.BadRequest;
                return;
            }

            var dirType = 0;
            var pathStr = UnescapeDataString(path.Value);
            if (!int.TryParse(type.Value, out dirType))
            {
                response.Status = System.Net.HttpStatusCode.BadRequest;
                return;
            }
            if (!canEdit((EDirType)dirType))
            {
                response.Status = System.Net.HttpStatusCode.BadRequest;
                return;
            }

            if (File.Exists(file.TempFileName))
            {
                var dir = getDir((EDirType)dirType);
                var tgtFn = dir + pathStr;

                try
                {
                    File.Delete(tgtFn);
                    File.Move(file.TempFileName, tgtFn);
                }
                catch (Exception ex)
                {
                    response.Status = System.Net.HttpStatusCode.BadRequest;
                    return;
                }
            }

            sendData(response, null);

        }

        private void addFileHandler(IRequest request, IResponse response)
        {
            if (EnableCrossDomain)
            {
                enableCrossDomain(response);
            }

            if (request.QueryString == null || request.Files == null)
            {
                response.Status = System.Net.HttpStatusCode.BadRequest;
                return;
            }

            var type = request.QueryString.Get("type");
            var path = request.QueryString.Get("path");
            var file = request.Files["file"];
            if (type == null || path == null || file == null)
            {
                response.Status = System.Net.HttpStatusCode.BadRequest;
                return;
            }

            var dirType = 0;
            var pathStr = UnescapeDataString(path.Value);
            if (!int.TryParse(type.Value, out dirType))
            {
                response.Status = System.Net.HttpStatusCode.BadRequest;
                return;
            }
            if (!canEdit((EDirType)dirType))
            {
                response.Status = System.Net.HttpStatusCode.BadRequest;
                return;
            }

            if (File.Exists(file.TempFileName))
            {
                var dir = getDir((EDirType)dirType);
                var tgtFn = dir + pathStr;

                try
                {
                    if (Directory.Exists(tgtFn))
                    {
                        var fn = tgtFn + "/" + file.OriginalFileName;
                        File.Delete(fn);
                        File.Move(file.TempFileName, fn);
                    }
                    else
                    {
                        response.Status = System.Net.HttpStatusCode.BadRequest;
                        return;
                    }
                }
                catch (Exception ex)
                {
                    response.Status = System.Net.HttpStatusCode.BadRequest;
                    return;
                }
            }

            var info = getDirInfo((EDirType)dirType, pathStr);
            sendData(response, info);

        }

        private void setContentHandler(IRequest request, IResponse response)
        {
            if (EnableCrossDomain)
            {
                enableCrossDomain(response);
            }

            if (request.QueryString == null)
            {
                response.Status = System.Net.HttpStatusCode.BadRequest;
                return;
            }

            var type = request.QueryString.Get("type");
            var path = request.QueryString.Get("path");
            var content = request.Form.Get("content");
            if (type == null || path == null || content == null)
            {
                response.Status = System.Net.HttpStatusCode.BadRequest;
                return;
            }

            var dirType = 0;
            var pathStr = UnescapeDataString(path.Value);
            if (!int.TryParse(type.Value, out dirType))
            {
                response.Status = System.Net.HttpStatusCode.BadRequest;
                return;
            }
            if (!canEdit((EDirType)dirType))
            {
                response.Status = System.Net.HttpStatusCode.BadRequest;
                return;
            }

            var dir = getDir((EDirType)dirType);
            var fullname = dir + pathStr;
            if (!File.Exists(fullname))
            {
                response.Status = System.Net.HttpStatusCode.NotFound;
                return;
            }

            try
            {
                File.WriteAllText(fullname, content.Value);
            }
            catch
            {
                response.Status = System.Net.HttpStatusCode.NotFound;
                return;
            }
        }

        private void unzipHandler(IRequest request, IResponse response)
        {
            if (EnableCrossDomain)
            {
                enableCrossDomain(response);
            }

            if (request.QueryString == null)
            {
                response.Status = System.Net.HttpStatusCode.BadRequest;
                return;
            }

            var type = request.QueryString.Get("type");
            var path = request.QueryString.Get("path");
            if (type == null || path == null)
            {
                response.Status = System.Net.HttpStatusCode.BadRequest;
                return;
            }


            var dirType = 0;
            var pathStr = UnescapeDataString(path.Value);
            if (!int.TryParse(type.Value, out dirType))
            {
                response.Status = System.Net.HttpStatusCode.BadRequest;
                return;
            }

            if (!canEdit((EDirType)dirType))
            {
                response.Status = System.Net.HttpStatusCode.BadRequest;
                return;
            }

            var dir = getDir((EDirType)dirType);
            var fullname = dir + pathStr;
            if (!File.Exists(fullname))
            {
                response.Status = System.Net.HttpStatusCode.BadRequest;
                return;
            }
            var baseDir = Path.GetDirectoryName(pathStr);
            if (baseDir == null)
            {
                baseDir = "/";
            }

            var zip = new Zipper();
            zip.UnzipFile(fullname, dir + baseDir);

            var info = getInfo((EDirType)dirType, baseDir);
            sendData(response, info);
        }

        private void getTerminalInfoHandler(IRequest request, IResponse response)
        {
            if (EnableCrossDomain)
            {
                enableCrossDomain(response);
            }

            var info = new TerminalInfo();
#if !DISABLE_TERMINAL
            if(Terminal.UTerminal.Instance.IsListening())
            {
                info.enable = true;
                info.port = Terminal.UTerminal.Instance.Port;
                info.defaultInput = mTerminalDefaultInput;
                info.maxRow = mMaxTerimalRow;
                sendData(response, info);
            }
            else
            {
                info.enable = false;
                sendData(response, info);
            }
#else
            info.enable = false;
            sendData(response, info);
#endif
        }

#endregion

#region inner functions

		private string UnescapeDataString(string val)
        {
            return val;

            var ret = Uri.UnescapeDataString(val);
            ret = ret.Replace("%20", " ");
            return ret;
        }

		private string getDir(EDirType dirtype)
        {
            switch(dirtype)
            {
                case EDirType.DataPath:
                    return UtilsHelper.DataPath;
                case EDirType.PersistentDataPath:
                    return UtilsHelper.PersistentDataPath;
                case EDirType.StreamingAssetsPath:
                    return UtilsHelper.StreamingAssetsPath;
                case EDirType.TemporaryCachePath:
                    return UtilsHelper.TemporaryCachePath;
            }
            return "";
        }

        private RootInfo getRootInfo()
        {
            var list = new RootInfo();
            list.platform = UtilsHelper.GetPlatform().ToString();
            list.children = new List<DirInfo>();
            var info = getDirInfo(EDirType.PersistentDataPath, null);
            if(info != null)
            {
                list.children.Add(info);
            }
            info = getDirInfo(EDirType.StreamingAssetsPath, null);
            if (info != null)
            {
                list.children.Add(info);
            }
            info = getDirInfo(EDirType.TemporaryCachePath, null);
            if (info != null)
            {
                list.children.Add(info);
            }
            return list;
        }

        private string getFileType(string ext)
        {
            if(ext == null)
            {
                return "dir";
			}
            ext = ext.ToLower();
            if(mEdiableExt.Contains(ext))
            {
                return "txt";
            }
            else if(ext == ".png" || ext == ".jpg" || ext == ".bmp" || ext == ".gif")
            {
                return "img";
            }
            else if(ext == ".zip" || ext == ".rar")
            {
                return "zip";
            }

            return "unkonw";
        }

        private bool canEdit(EDirType dirtype)
        {
			bool editable = true;
			if (dirtype == EDirType.StreamingAssetsPath ||
				dirtype == EDirType.DataPath)
			{
                var platform = UtilsHelper.GetPlatform();
                if (platform == EPlatform.Mac || platform == EPlatform.Windows)
                {
                    editable = true;
                }
                else
                {
                    editable = false;
                }
			}
            return editable;
		}

        private DirInfo getInfo(EDirType dirtype, string path)
        {
            DirInfo dirInfo = null;
            var fn = getDir(dirtype) + path;
            if(File.Exists(fn))
            {
                dirInfo = getFileInfo(dirtype, path);
            }
            else if(Directory.Exists(fn))
            {
                dirInfo = getDirInfo(dirtype, path);
            }
            return dirInfo;
        }

        private DirInfo getFileInfo(EDirType dirtype, string path)
        {
            bool editable = this.canEdit(dirtype);
            if (UtilsHelper.GetPlatform() == EPlatform.Android)
            {
                if (dirtype == EDirType.StreamingAssetsPath)
                {
                    return null;
                }
            }
            var dir = getDir(dirtype);
            var fn = dir + path;
			var fileinfo = new FileInfo(path);

			var ifo = new DirInfo();
			ifo.isdir = true;
			ifo.dirtype = dirtype;
			ifo.name = fileinfo.Name;
			ifo.path = path;
			ifo.modifytime = fileinfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss");
			ifo.editable = editable;
            ifo.filetype = getFileType(fileinfo.Extension);
			ifo.size = 0;
            return ifo;
        }

        private DirInfo getDirInfo(EDirType dirtype, string path)
        {
            bool editable = this.canEdit(dirtype);
            if (UtilsHelper.GetPlatform() == EPlatform.Android)
            {
                if (dirtype == EDirType.StreamingAssetsPath)
                {
                    return null;
                }
            }

            var dir = getDir(dirtype) + path;
            if(!Directory.Exists(dir))
            {
                return null;
            }
            var info = new DirInfo();
            info.dirtype = dirtype;
            info.isdir = true;
            if (string.IsNullOrEmpty(path) || path == "/")
            {
                info.name = dirtype.ToString();
                info.path = "/";
                info.renameable = false;
                path = "";
            }
            else
            {
                info.name = Path.GetFileName(path);
                info.path = path;
                info.renameable = editable;
            }
            info.editable = editable;
            info.filetype = getFileType(null);
            info.children = new List<DirInfo>();
            
            var dirinfo = new DirectoryInfo(dir);
            info.modifytime = dirinfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss");

            var dirinfos = dirinfo.GetDirectories();
            foreach(var di in dirinfos)
            {
                var ifo = new DirInfo();
                ifo.isdir = true;
                ifo.dirtype = dirtype;
                ifo.name = di.Name;
                ifo.path = path + "/" + di.Name;
                ifo.modifytime = di.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss");
                ifo.editable = editable;
                ifo.filetype = getFileType(null);
                ifo.renameable = editable;
                ifo.size = 0;
                info.children.Add(ifo);
            }
            var filesinfos = dirinfo.GetFiles();
            foreach(var fi in filesinfos)
			{
                var filename = fi.Name.ToLower();
                if (dirtype == EDirType.StreamingAssetsPath && 
                    (filename == "www.zip" || filename == "www.zip.meta"))
				{
					continue;
				}

                var ifo = new DirInfo();
                ifo.isdir = false;
                ifo.dirtype = dirtype;
                ifo.name = fi.Name;
                ifo.path = path + "/" + fi.Name;
                ifo.modifytime = fi.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss");
                ifo.editable = editable;
                ifo.filetype = getFileType(fi.Extension);
				ifo.renameable = editable;
                ifo.size = fi.Length;
                info.children.Add(ifo);
            }
            info.childnum = info.children.Count;

            return info;
        }

#endregion
	}
}
