using Oxide.Core;
using Oxide.Core.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Drawing.Imaging;

namespace Oxide.Ext.LocalFiles
{
    public sealed class LocalFilesExt : Extension
    {
        public override string Name => "LocalFiles";
        public override string Author => "RFC1920";
        public override VersionNumber Version => new VersionNumber(1, 0, 2);

        public static string filesDirectory;
        public static bool isconsole = false;

        public static Dictionary<int, FileMeta> localFiles;
        public static Dictionary<string, int> fileList;

        private static bool debug = true;

        public class FileMeta
        {
            public string Dir = "";
            public string Url = "";
            public string FileName;
            public string Description;
            public string Category;
            public float FileSize;
            public DateTime CreationTime;
            public string FileType;
            public int Width;
            public int Height;
        }

        public LocalFilesExt(ExtensionManager manager) : base(manager)
        {
            try
            {
                LogDebug("LocalFiles Extension loaded.");
            }
            catch
            {
                isconsole = true;
            }

            localFiles = new Dictionary<int, FileMeta>();
            fileList = new Dictionary<string, int>();

            filesDirectory = Interface.Oxide.DataDirectory + "/localfiles/Content/";

            LoadData();
            ScanDir();
        }

        private static void LoadData()
        {
            localFiles = Interface.Oxide.DataFileSystem.ReadObject<Dictionary<int, FileMeta>>("localfiles/FileMeta");
            fileList = Interface.Oxide.DataFileSystem.ReadObject<Dictionary<string, int>>("localfiles/FileToKey");
        }
        private static void SaveData()
        {
            Interface.Oxide.DataFileSystem.WriteObject("localfiles/FileMeta", localFiles);
            Interface.Oxide.DataFileSystem.WriteObject("localfiles/FileToKey", fileList);
        }

        public override void Load()
        {
            Interface.Oxide.OnFrame(new Action<float>(OnFrame));
        }
        private void OnFrame(float delta)
        {
            object[] objArray = new object[] { delta };
        }
        public override void OnModLoad()
        {
        }

        public override void OnShutdown()
        {
        }

        #region processing
        public static void ScanDir(string subdir = "", bool force = false)
        {
            LogDebug($"Scanning for files in {filesDirectory}");
            var files = Directory.GetFiles(filesDirectory);
            if (!string.IsNullOrEmpty(subdir))
            {
                files = Directory.GetFiles(filesDirectory + Path.DirectorySeparatorChar + subdir);
            }

            foreach (var path in files)
            {
                LogDebug($"Checking path {path}");
                if (File.Exists(path))
                {
                    ProcessFile(path, force);
                }
                else if (Directory.Exists(path))
                {
                    ProcessDirectory(path, force);
                }
            }
            SaveData();
        }

        public static int ProcessFile(string path, bool force = false)
        {
            var fname = Path.GetFileName(path);
            if (fileList.ContainsKey(fname) && !force) return 0;
            int w = 0;
            int h = 0;
            string mimeType = "unknown/unknown";
            try
            {
                Bitmap img = new Bitmap(path);
                w = img.Width;
                h = img.Height;
                mimeType = GetMimeType(img);
                img.Dispose();
            }
            catch { }
            var sysfinfo = new FileInfo(path);

            var finfo = new FileMeta()
            {
                //Dir = Path.GetDirectoryName(path),
                Dir = sysfinfo.DirectoryName,
                FileName = fname,
                FileSize = sysfinfo.Length,
                CreationTime = sysfinfo.CreationTime,
                FileType = mimeType,
                Width = w,
                Height = h
            };

            int mykey = 0;
            if (!fileList.ContainsKey(path))
            {
                var exists = localFiles.FirstOrDefault(x => x.Value.FileName == fname).Key;
                if (exists > 0)
                {
                    localFiles.Remove(exists);
                    fileList.Remove(fname);
                }

                mykey = RandomNumber();
                localFiles.Add(mykey, finfo);
                fileList.Add(fname, mykey);
            }
            return mykey;
        }

        public static void ProcessDirectory(string targetDirectory, bool force = false)
        {
            // Process the list of files found in the directory.
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string fileName in fileEntries)
            {
                ProcessFile(fileName, force);
            }

            // Recurse into subdirectories of this directory.
            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
            {
                ProcessDirectory(subdirectory);
            }
        }

        public static string GetMimeType(Image img)
        {
            foreach (ImageCodecInfo codec in ImageCodecInfo.GetImageDecoders())
            {
                if (codec.FormatID == img.RawFormat.Guid)
                {
                    return codec.MimeType;
                }
            }

            return "image/unknown";
        }

        public static int FetchUrl(string url, string targetPath = "")
        {
            if(string.IsNullOrEmpty(targetPath))
            {
                targetPath = filesDirectory;
            }
            using (var client = new WebClient())
            {
                Uri myUri = new Uri(url);
                string path = targetPath + Path.DirectorySeparatorChar + myUri.Segments.Last();
                LogDebug($"Downloading {url} to {path}");
                client.DownloadFile(url, path);
                int mykey = ProcessFile(path);
                localFiles[mykey].Url = url;
                SaveData();
                return mykey;
            }
        }
        #endregion

        #region category
        public static List<FileMeta> GetFilesInCategory(string category)
        {
            List<FileMeta> fl = new List<FileMeta>();
            foreach(var file in localFiles)
            {
                if(file.Value.Category == category)
                {
                    fl.Add(file.Value);
                }
            }
            return fl;
        }
        public static bool SetCategory(string path, string cat)
        {
            if (fileList.ContainsKey(path))
            {
                localFiles[fileList[path]].Category = cat;
                SaveData();
                return true;
            }
            return false;
        }
        public static bool SetCategory(int filekey, string cat)
        {
            if (localFiles.ContainsKey(filekey))
            {
                localFiles[filekey].Category = cat;
                SaveData();
            }
            return false;
        }
        #endregion

        #region rename 
        public static bool RenameFile(string path, string newname)
        {
            if (fileList.ContainsKey(path))
            {
                var file = localFiles[fileList[path]];
                File.Move(file.FileName, newname);
                file.FileName = newname;
                SaveData();
                return true;
            }
            return false;
        }
        public static bool RenameFile(int filekey, string newname)
        {
            if (localFiles.ContainsKey(filekey))
            {
                var file = localFiles[filekey];
                File.Move(file.FileName, newname);
                file.FileName = newname;
                SaveData();
                return true;
            }
            return false;
        }
        #endregion

        #region delete
        public static bool DeleteFile(string path)
        {
            if(fileList.ContainsKey(path))
            {
                File.Delete(filesDirectory + Path.DirectorySeparatorChar + path);
                var fname = Path.GetFileName(path);

                var exists = localFiles.FirstOrDefault(x => x.Value.FileName == fname).Key;
                if (exists > 0)
                {
                    localFiles.Remove(exists);
                    fileList.Remove(path);
                    SaveData();
                    return true;
                }
            }
            return false;
        }
        public static bool DeleteFile(int filekey)
        {
            if (localFiles.ContainsKey(filekey))
            {
                File.Delete(localFiles[filekey].Dir + Path.DirectorySeparatorChar + localFiles[filekey].FileName);

                localFiles.Remove(filekey);
                fileList.Remove(localFiles[filekey].FileName);
                SaveData();
                return true;
            }
            return false;
        }
        #endregion

        #region filecontent
        public static byte[] GetFileContent(int index)
        {
            if (localFiles.ContainsKey(index))
            {
                string filePath = localFiles[index].Dir + Path.DirectorySeparatorChar + localFiles[index].FileName;
                return File.ReadAllBytes(filePath);
            }
            return null;
        }
        public static byte[] GetFileContent(string path)
        {
            if (fileList.ContainsKey(path))
            {
                int index = fileList[path];
                if (localFiles.ContainsKey(index))
                {
                    string filePath = localFiles[index].Dir + Path.DirectorySeparatorChar + localFiles[index].FileName;
                    return File.ReadAllBytes(filePath);
                }
            }
            return null;
        }
        #endregion

        #region util
        private static void LogDebug(string debugTxt)
        {
            if(debug) Interface.Oxide.LogDebug(debugTxt);
        }

        public static int RandomNumber(int length = 8)
        {
            const string chars = "0123456789";
            List<char> charList = chars.ToList();

            string random = "";

            for (int i = 0; i <= length; i++)
            {
                random = random + charList[UnityEngine.Random.Range(0, charList.Count - 1)];
            }
            return int.Parse(random);
        }

        public static string RandomString(int length = 16)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            List<char> charList = chars.ToList();

            string random = "";

            for (int i = 0; i <= length; i++)
            {
                random = random + charList[UnityEngine.Random.Range(0, charList.Count - 1)];
            }
            return random;
        }
        #endregion
    }
}
