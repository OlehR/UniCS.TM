using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Utils
{
    public class FileAndDir
    {
        static public (long, long) DelDir(string pDir, IEnumerable<string> pNotDelDir)
        {
            FileLogger.WriteLogMessage($"DelDir Start =>{pDir}");
            if (pNotDelDir == null || !pNotDelDir.Any())
                return (0, 0);
            long SizeDel = 0, SizeUse = 0;
            var dirs = Directory.GetDirectories(pDir, "*", SearchOption.TopDirectoryOnly);
            foreach (var el in dirs)
            {
                var dir = new DirectoryInfo(el);
                var LD = pNotDelDir.Where(e => el.EndsWith(e));
                if (LD.Count() > 0)
                {
                    SizeUse += DirSize(dir);
                    FileLogger.WriteLogMessage($"DelDir Skip =>{dir.FullName}");
                }
                else
                {
                    SizeDel += DirSize(dir);
                    FileLogger.WriteLogMessage($"DelDir=>{dir.FullName}");
                    dir.Delete(true);
                }
            }
            FileLogger.WriteLogMessage($"DelDir End SizeDel=>{SizeDel}, SizeUse=>{SizeUse}");
            return (SizeDel, SizeUse);

        }
        static public long DirSize(DirectoryInfo d)
        {
            long size = 0;
            // Add file sizes.
            FileInfo[] fis = d.GetFiles();
            foreach (FileInfo fi in fis)
            {
                size += fi.Length;
            }
            // Add subdirectory sizes.
            DirectoryInfo[] dis = d.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                size += DirSize(di);
            }
            return size;
        }

        static public double GetFreeSpace(string pPath)
        {
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            var r = allDrives.Where(el => pPath.StartsWith(el.Name)).OrderByDescending(el => el.Name.Length);
            if (r != null && r.Count() > 0)
                return r.FirstOrDefault().AvailableFreeSpace;
            return 20d * 1024d * 1024d;
        }

    }
}
