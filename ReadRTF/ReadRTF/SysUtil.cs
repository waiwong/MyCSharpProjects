using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace Sys.Common
{
    public class SysUtil
    {
        private static Stopwatch _sw = new Stopwatch();

        [Conditional("DEBUG"), Conditional("UAT")]
        public static void StartLog()
        {
            _sw.Reset();
            _sw = Stopwatch.StartNew();
        }

        [Conditional("DEBUG"), Conditional("UAT")]
        public static void StopLog(string logMsg)
        {
            _sw.Stop();
            long elapsedMS = _sw.ElapsedMilliseconds;
            if (elapsedMS > 500)
                Log.LogDebug(logMsg + string.Format(",Time elapsed(s):{0}", elapsedMS / 1000.0));
        }

        [Conditional("DEBUG"), Conditional("UAT")]
        public static void PauseLog(string logMsg)
        {
            StopLog(logMsg);
            StartLog();
        }

        /// <summary>
        /// Get Current assembly Directory
        /// </summary>
        /// <returns></returns>
        public static string GetAssemblyDirectory()
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            return System.IO.Path.GetDirectoryName(path);
        }

        public static string GetUriPath(string uriString)
        {
            System.Uri uri = new Uri(uriString);
            string result = uriString.Remove(uriString.Length - uri.Segments[uri.Segments.Length - 1].Length);
            if (!result.EndsWith("/"))
            {
                result = result + "/";
            }
            return result;
        }

        /// <summary>
        /// Get Current assembly Version
        /// </summary>
        /// <returns></returns>
        public static string GetAssemblyVersion()
        {
            AssemblyName currAssembley = Assembly.GetExecutingAssembly().GetName();
            return currAssembley.Version.ToString();
        }

        /// <summary>
        /// Get Current assembly Version
        /// </summary>
        /// <param name="asseblyName">assebly Name</param>
        /// <returns></returns>
        public static string GetAssemblyVersion(string asseblyName)
        {
            AssemblyName currAssembley = AssemblyName.GetAssemblyName(asseblyName);
            return currAssembley.Version.ToString();
        }

        /// <summary>
        /// Check a string is Contains Unicode char
        /// </summary>
        /// <param name="input">input string</param>
        /// <returns></returns>
        public static bool ContainsUnicodeChar(string input)
        {
            bool result = false;
            const int MaxAnsiCode = 255;
            char[] inputChar = input.ToCharArray();
            foreach (char item in inputChar)
            {
                if (item > MaxAnsiCode)
                {
                    result = true;
                    break;
                }
            }

            return result;
        }

        public static bool CheckFileIsComplete(string fileFullName)
        {
            bool result = false;

            if (File.Exists(fileFullName))
            {
                int count = 0;
                while (count <= 5)
                {
                    FileInfo fi = new FileInfo(fileFullName);
                    FileStream stream = null;
                    try
                    {
                        stream = fi.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                        result = true;
                    }
                    catch (IOException ex)
                    {
                        result = false;
                        Log.LogDebug(string.Format("Name:{0};{1}", fi.Name, ex.Message));
                    }
                    catch (Exception ex)
                    {
                        result = false;
                        Log.LogDebug(string.Format("Name:{0};{1}", fi.Name, ex.Message));
                    }
                    finally
                    {
                        if (stream != null)
                            stream.Close();
                    }

                    if (result)
                        break;

                    System.Threading.Thread.Sleep(5000);
                }
            }

            return result;
        }

        public static string BackupSourceFiles(string sourceFileName, bool firstTime)
        {
            return BackupSourceFiles(sourceFileName, string.Empty, firstTime);
        }

        public static string BackupSourceFiles(string sourceFileName, string folderName, bool firstTime)
        {
            string strResult = string.Empty;
            if (File.Exists(sourceFileName))
            {
                string bkpFolderFullName = string.Empty;
                FileInfo fi = new FileInfo(sourceFileName);
                if (string.IsNullOrEmpty(folderName))
                    folderName = "Backup";

                if (firstTime)
                    bkpFolderFullName = Path.Combine(fi.DirectoryName, Path.Combine(folderName, DateTime.Now.ToString("yyyyMMddHHmm")));
                else
                    bkpFolderFullName = Path.Combine(fi.DirectoryName, Path.Combine(folderName, DateTime.Now.ToString("yyyyMM")));

                if (!Directory.Exists(bkpFolderFullName))
                {
                    Directory.CreateDirectory(bkpFolderFullName);
                }

                if (!firstTime)
                {
                    strResult = Path.Combine(bkpFolderFullName, string.Format("{0}_{1}{2}",
                        Path.GetFileNameWithoutExtension(sourceFileName), System.DateTime.Now.ToString("yyyyMMddHHmmss"), fi.Extension));
                }
                else
                {
                    strResult = Path.Combine(bkpFolderFullName, fi.Name);
                }

                File.Copy(sourceFileName, strResult, true);

                if (!firstTime)
                {
                    if (File.Exists(strResult))
                        File.Delete(sourceFileName);
                }

                if (firstTime)
                {
                    try
                    {
                        List<string> listDelDir = new List<string>();
                        DirectoryInfo di = new DirectoryInfo(bkpFolderFullName);
                        DirectoryInfo[] diArr = di.GetDirectories("*", SearchOption.TopDirectoryOnly);
                        string compareDate = DateTime.Now.AddDays(-10).ToString("yyyyMMdd");

                        foreach (DirectoryInfo dri in diArr)
                        {
                            if (dri.Name.Length == 12)
                            {
                                if (dri.Name.CompareTo(compareDate) < 0)
                                {
                                    listDelDir.Add(dri.FullName);
                                }
                            }
                        }

                        foreach (string item in listDelDir)
                        {
                            if (Directory.Exists(item))
                            {
                                Directory.Delete(item, true);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.LogErr(ex);
                    }
                }
            }

            return strResult;
        }

        /// <summary>
        /// Function to save byte array to a file
        /// </summary>
        /// <param name="strFileName">File name to save byte array</param>
        /// <param name="byteArray">Byte array to save to external file</param>
        public static void ByteArrayToFile(string strFileName, byte[] byteArray)
        {
            if (File.Exists(strFileName))
            {
                File.Delete(strFileName);
            }
            // Open file for reading
            using (System.IO.FileStream fs = new System.IO.FileStream(strFileName, System.IO.FileMode.Create, System.IO.FileAccess.Write))
            {
                // Writes a block of bytes to this stream using data from a byte array.
                fs.Write(byteArray, 0, byteArray.Length);
            }
        }

        public static byte[] FileToByteArray(string filename)
        {
            FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            // Create a byte array of file stream length
            byte[] fileByte = new byte[Convert.ToInt32(fs.Length)];
            //Read block of bytes from stream into the byte array
            fs.Read(fileByte, 0, Convert.ToInt32(fs.Length));

            //Close the File Stream
            fs.Close();
            return fileByte;
        }
    }
}
