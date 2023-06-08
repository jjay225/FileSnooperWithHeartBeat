using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FileSnooper.Helpers
{
    public static class PathComparer
    {        

        public static bool DirectoryEquals(DirectoryInfo dir1, DirectoryInfo dir2)
        {
            if (dir1.Name.ToLower() == dir2.Name.ToLower())
            {
                if ((dir1.Parent != null) && (dir2.Parent != null))
                {
                    return DirectoryEquals(dir1.Parent, dir2.Parent);
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        public static bool HasSameRootFolder(string filePath, string rootFolderPath)
        {
            string fullPathRootFolder = Path.GetFullPath(rootFolderPath);
            string relativePath = Path.GetRelativePath(fullPathRootFolder, filePath);

           
            return !relativePath.StartsWith("..");
        }

        public static bool DirectoryEqualsV2(string d1, string d2)
        {
            DirectoryInfo dirInfo1 = new DirectoryInfo(d1);
            DirectoryInfo dirInfo2 = new DirectoryInfo(d2);
            bool isParent = false;
            while (dirInfo2.Parent != null)
            {
                if (dirInfo2.Parent.FullName == dirInfo1.FullName)
                {
                    isParent = true;
                    break;
                }
                else dirInfo2 = dirInfo2.Parent;
            }

            return isParent;
        }
    }
}