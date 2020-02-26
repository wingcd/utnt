using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ICSharpCode.SharpZipLib.Zip;
using System;

namespace Wing.Tools.Utils
{
    public class Zipper
    {
        public delegate void ZipEventHandler(bool result);
        public delegate void ZipProgressEventHandler(float progress);
        public delegate void PostEventHandler(ZipEntry entry);
        public delegate bool PostFilterEventHandler(ZipEntry entry);

        public event ZipEventHandler Finished;
        public event ZipProgressEventHandler Progress;
        public event PostEventHandler PostEvent;
        public event PostFilterEventHandler PostFilterEvent;

        private bool mProcessing = false;
        public object Tag;

        private class ZipItem
        {
            public string Filename;
        }

        public bool Processing
        {
            get
            {
                return mProcessing;
            }
        }

        private void OnPost(ZipEntry entry)
        {
            if (PostEvent != null)
            {
                PostEvent(entry);
            }
        }

        private void OnProgress(float value)
        {
            if (Progress != null)
            {
                Progress(value);
            }
        }

        public void OnFinished(bool result)
        {
            if (Finished != null)
            {
                Finished(result);
            }
        }

        private bool OnPostFilter(ZipEntry entry)
        {
            if (PostFilterEvent != null)
            {
                return PostFilterEvent(entry);
            }
            return true;
        }

        private void SearchFiles(string[] dirOrFiles, List<ZipItem> items)
        {
            for (var i = 0; i < dirOrFiles.Length; i++)
            {
                SearchFiles(dirOrFiles[i], items);
            }
        }

        private void SearchFiles(string dirOrFile, List<ZipItem> items)
        {
            if (File.Exists(dirOrFile))
            {
                var item = new ZipItem
                {
                    Filename = dirOrFile
                };
                items.Add(item);
            }
            else if (Directory.Exists(dirOrFile))
            {
                var files = Directory.GetFiles(dirOrFile);
                for (var i = 0; i < files.Length; i++)
                {
                    var file = files[i];
                    var item = new ZipItem
                    {
                        Filename = file
                    };
                    items.Add(item);
                }

                var dirs = Directory.GetDirectories(dirOrFile);
                SearchFiles(dirs, items);
            }
        }

        public void Zip(string[] dirOrFiles, string baseDir, string _outputPathName, string _password = null)
		{
			if (!mProcessing)
			{
				mProcessing = true;

				var counter = 0f;
				var items = new List<ZipItem>();

				ZipOutputStream zipOutputStream = new ZipOutputStream(File.Create(_outputPathName));
				zipOutputStream.SetLevel(6);    // 压缩质量和压缩速度的平衡点
				if (!string.IsNullOrEmpty(_password))
					zipOutputStream.Password = _password;

				SearchFiles(dirOrFiles, items);

				for (var i = 0; i < items.Count; i++)
				{
					var item = items[i];
					var filename = item.Filename;
					var relPath = item.Filename.Replace(baseDir, "");
					relPath = Path.GetDirectoryName(relPath);

					var ret = ZipFile(filename, relPath, zipOutputStream);
					if (!ret)
					{
						Debug.Log("filename is " + filename + " zip filed!");
						OnFinished(false);
						break;
					}

					counter++;
					OnProgress(counter / items.Count * 100);
				}

                zipOutputStream.Flush();
                zipOutputStream.Close();

				OnFinished(true);
				mProcessing = false;
			}
		}

        public void AsyncZip(string[] dirOrFiles, string baseDir, string _outputPathName, string _password = null)
        {
			CoroutineProvider.Instance.StartCoroutine(AsyncZipFiles(dirOrFiles, baseDir, _outputPathName, _password));
		}

        private IEnumerator AsyncZipFiles(string[] dirOrFiles, string baseDir, string _outputPathName, string _password = null)
        {
            if (!mProcessing)
            {
                mProcessing = true;

                var counter = 0f;
                var items = new List<ZipItem>();

                ZipOutputStream zipOutputStream = new ZipOutputStream(File.Create(_outputPathName));
                zipOutputStream.SetLevel(6);    // 压缩质量和压缩速度的平衡点
                if (!string.IsNullOrEmpty(_password))
                    zipOutputStream.Password = _password;

                SearchFiles(dirOrFiles, items);

                for (var i = 0; i < items.Count; i++)
                {
                    var item = items[i];
                    var filename = item.Filename;
                    var relPath = item.Filename.Replace(baseDir, "");
                    relPath = Path.GetDirectoryName(relPath);

                    var ret = ZipFile(filename, relPath, zipOutputStream);
                    if (!ret)
                    {
                        Debug.Log("filename is " + filename + " zip filed!");
                        OnFinished(false);
                        break;
                    }

                    counter++;
                    OnProgress(counter / items.Count * 100);

                    yield return null;
                }

				zipOutputStream.Flush();
				zipOutputStream.Close();

                OnFinished(true);
                mProcessing = false;
            }
        }

		/// <summary>
		/// 压缩文件
		/// </summary>
		/// <param name="_filePathName">文件路径名</param>
		/// <param name="_parentRelPath">要压缩的文件的父相对文件夹</param>
		/// <param name="_zipOutputStream">压缩输出流</param>
		/// <returns></returns>
		private bool ZipFile(string _filePathName, string _parentRelPath, ZipOutputStream _zipOutputStream)
		{
			//Crc32 crc32 = new Crc32();
			ZipEntry entry = null;
			FileStream fileStream = null;
			try
			{
				string entryName = _parentRelPath + '/' + Path.GetFileName(_filePathName);
				entry = new ZipEntry(entryName);
				entry.DateTime = System.DateTime.Now;

                if (!OnPostFilter(entry))
                {    
                    // 过滤
					return true;
                }

				fileStream = File.OpenRead(_filePathName);
				byte[] buffer = new byte[fileStream.Length];
				fileStream.Read(buffer, 0, buffer.Length);
				fileStream.Close();

				entry.Size = buffer.Length;

				//crc32.Reset();
				//crc32.Update(buffer);
				//entry.Crc = crc32.Value;

				_zipOutputStream.PutNextEntry(entry);
				_zipOutputStream.Write(buffer, 0, buffer.Length);
			}
			catch (System.Exception _e)
			{
				Debug.LogError("[ZipUtility.ZipFile]: " + _e.ToString());
				return false;
			}
			finally
			{
				if (null != fileStream)
				{
					fileStream.Close();
					fileStream.Dispose();
				}
			}

            OnPost(entry);
			return true;
		}

        public void AsyncUnzip(string _filePathName, string _outputPath, string _password = null)
		{
            if (!mProcessing)
            {
                if (string.IsNullOrEmpty(_filePathName) || string.IsNullOrEmpty(_outputPath))
                {
                    OnFinished(false);
                    return;
                }
				CoroutineProvider.Instance.StartCoroutine(AsyncUnzipFile(_filePathName, _outputPath, _password));
			}
		}

        public void AsyncUnzipToMemary(byte[] data, Dictionary<string, byte[]> _memory, string _password = null)
		{
			if (!mProcessing)
			{
				if (data == null)
				{
					OnFinished(false);
					return;
				}
				CoroutineProvider.Instance.StartCoroutine(AsyncUnzipFileToMemory(data, _memory, _password));
			}
		}

        private IEnumerator AsyncUnzipFileToMemory(byte[] data, Dictionary<string, byte[]> _memory, string _password = null)
        {
            var _inputStream = new MemoryStream(data);
            if (!mProcessing)
            {
                mProcessing = false;

                if ((null == _inputStream))
                {
                    OnFinished(false);
                }
                else
                {
                    _inputStream.Position = 0;

                    // 解压Zip包
                    ZipEntry entry = null;
                    using (ZipInputStream zipInputStream = new ZipInputStream(_inputStream))
                    {
                        if (!string.IsNullOrEmpty(_password))
                            zipInputStream.Password = _password;

                        var total = data.Length;
                        var readLen = 0;
                        
                        while ((entry = zipInputStream.GetNextEntry()) != null && UnityEngine.Application.isPlaying)
                        {
                            if (string.IsNullOrEmpty(entry.Name))
                                continue;

                            if (!OnPostFilter(entry))
                                continue;    // 过滤
                            
                            var countSize = 0;
                            using (var stream = new MemoryStream())
							{
								if (true)
								{
									byte[] bytes = new byte[1024];
									while (true)
									{
										int count = 0;
										try
										{
											count = zipInputStream.Read(bytes, 0, bytes.Length);
											readLen += count;
											OnProgress((readLen * 1f) / total);
										}
										catch (Exception e)
										{
											Debug.LogError(e);
										}

										countSize += count;
										if (countSize > 500000)
										{
											countSize = 0;
											yield return null;
										}

										if (count > 0)
										{
											stream.Write(bytes, 0, count);
										}
										else
										{
											OnPost(entry);
											break;
										}
									}

                                    stream.Flush();
                                    _memory[entry.Name.ToLower()] = stream.ToArray();

                                    //Debug.Log(entry.Name.ToLower());
								}
								else
								{
									OnPost(entry);
								}
							}

							yield return null;
                        }
                    }

                    OnFinished(true);
                }

                mProcessing = false;
            }
        }

        private IEnumerator AsyncUnzipFile(string _filePathName, string _outputPath, string _password = null)
        {
            var _inputStream = File.OpenRead(_filePathName);
            if (!mProcessing)
            {
				mProcessing = false;

                if ((null == _inputStream) || string.IsNullOrEmpty(_outputPath))
                {
                    OnFinished(false);
                }
                else
                {
                    // 创建文件目录
                    if (!Directory.Exists(_outputPath))
                        Directory.CreateDirectory(_outputPath);
                    _inputStream.Position = 0;

                    // 解压Zip包
                    ZipEntry entry = null;
                    using (ZipInputStream zipInputStream = new ZipInputStream(_inputStream))
                    {
                        if (!string.IsNullOrEmpty(_password))
                            zipInputStream.Password = _password;

                        FileInfo fileInfo = new FileInfo(_filePathName);
                        var total = fileInfo.Length;
                        var readLen = 0;

                        while ((entry = zipInputStream.GetNextEntry()) != null && UnityEngine.Application.isPlaying)
                        {
                            if (string.IsNullOrEmpty(entry.Name))
                                continue;

                            if (!OnPostFilter(entry))
                                continue;    // 过滤

                            string filePathName = _outputPath.TrimEnd('/') + "/" + entry.Name.TrimStart('/');
                            var dir = Path.GetDirectoryName(filePathName);
                            if(!Directory.Exists(dir))
                            {
                                Directory.CreateDirectory(dir);
                            }

                            // 创建文件目录
                            if (entry.IsDirectory)
                            {
                                Directory.CreateDirectory(filePathName);
                                continue;
                            }

							// 写入文件
							//try
							//{

							//}
							//catch (System.Exception _e)
							//{
							//    Debug.LogError("[ZipUtility.UnzipFile]: " + _e.ToString() + " file:" + filePathName);

							//    OnFinished(false);

							//    break;
							//}

							var countSize = 0;
							using (FileStream fileStream = File.Create(filePathName))
							{
								if (true)
								{
									byte[] bytes = new byte[1024];
									while (true)
									{
										int count = 0;
										try
										{
											count = zipInputStream.Read(bytes, 0, bytes.Length);
											readLen += count;
											OnProgress((readLen * 1f) / total);
										}
										catch (Exception e)
										{
											Debug.LogError(e);
										}

										countSize += count;
										if (countSize > 500000)
										{
											countSize = 0;
											yield return null;
										}

										if (count > 0)
										{
											fileStream.Write(bytes, 0, count);
										}
										else
										{
											OnPost(entry);
											break;
										}
									}
								}
								else
								{
									OnPost(entry);
								}
							}

							yield return null;
                        }
                    }

                    OnFinished(true);
                }

				mProcessing = false;
            }
        }

        public void UnzipFile(string _filePathName, string _outputPath, string _password = null)
        {
            var _inputStream = File.OpenRead(_filePathName);
            if (!mProcessing)
            {
                mProcessing = false;

                if ((null == _inputStream) || string.IsNullOrEmpty(_outputPath))
                {
                    OnFinished(false);
                }
                else
                {
                    // 创建文件目录
                    if (!Directory.Exists(_outputPath))
                        Directory.CreateDirectory(_outputPath);
                    _inputStream.Position = 0;

                    // 解压Zip包
                    ZipEntry entry = null;
                    using (ZipInputStream zipInputStream = new ZipInputStream(_inputStream))
                    {
                        if (!string.IsNullOrEmpty(_password))
                            zipInputStream.Password = _password;

                        FileInfo fileInfo = new FileInfo(_filePathName);
                        var total = fileInfo.Length;
                        var readLen = 0;

                        while ((entry = zipInputStream.GetNextEntry()) != null)
                        {
                            if (string.IsNullOrEmpty(entry.Name))
                                continue;

                            if (!OnPostFilter(entry))
                                continue;    // 过滤

                            string filePathName = _outputPath.TrimEnd('/') + "/" + entry.Name.TrimStart('/');
                            var dir = Path.GetDirectoryName(filePathName);
                            if (!Directory.Exists(dir))
                            {
                                Directory.CreateDirectory(dir);
                            }

                            // 创建文件目录
                            if (entry.IsDirectory)
                            {
                                Directory.CreateDirectory(filePathName);
                                continue;
                            }

                            using (FileStream fileStream = File.Create(filePathName))
                            {
                                if (true)
                                {
                                    byte[] bytes = new byte[1024];
                                    while (true)
                                    {
                                        int count = 0;
                                        try
                                        {
                                            count = zipInputStream.Read(bytes, 0, bytes.Length);
                                            readLen += count;
                                            OnProgress((readLen * 1f) / total);
                                        }
                                        catch (Exception e)
                                        {
                                            Debug.LogError(e);
                                        }

                                        if (count > 0)
                                        {
                                            fileStream.Write(bytes, 0, count);
                                        }
                                        else
                                        {
                                            OnPost(entry);
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    OnPost(entry);
                                }
                            }
                        }
                    }

                    OnFinished(true);
                }

                mProcessing = false;
            }
        }
    }
}
