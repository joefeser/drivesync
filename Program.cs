using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace DriveSync
{
    class Program
    {
        static bool ignoreNode = false;
        static bool ignoreTFS = false;

        static void Main(string[] args)
        {
            var source = default(string);
            var destination = default(string);
            try
            {
                if (args == null || args.Length < 2)
                {
                    Console.WriteLine("You must pass 2 drive paths");
                    return;
                }
                source = args[0];
                destination = args[1];

                ignoreNode = args.Any(item => item.Equals("--ignorenode", StringComparison.OrdinalIgnoreCase));
                ignoreTFS = args.Any(item => item.Equals("--ignoretfs", StringComparison.OrdinalIgnoreCase));

                DirectoryInfo sourceDir = new DirectoryInfo(source);

                if (sourceDir.Exists)
                {
                    DirectoryInfo destDir = new DirectoryInfo(destination);
                    if (!destDir.Exists)
                    {
                        destDir.Create();
                    }
                    ProcessDirectory(sourceDir, destDir);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            Console.WriteLine("Done...");
            //Console.ReadKey();
        }

        private static void ProcessDirectory(DirectoryInfo source, DirectoryInfo dest)
        {
            try
            {
                if (ignoreNode && source.Name == "node_modules")
                {
                    return;
                }

                if (ignoreTFS && source.Name == "$tf")
                {
                    return;
                }

                List<string> destFolders = new List<string>();
                List<string> destFiles = new List<string>();

                foreach (DirectoryInfo info in dest.GetDirectories())
                {
                    destFolders.Add(info.Name.ToUpper());
                }

                foreach (FileInfo info in dest.GetFiles())
                {
                    destFiles.Add(info.Name.ToUpper());
                }

                foreach (FileInfo info in source.GetFiles())
                {
                    if (!destFiles.Contains(info.Name.ToUpper()))
                    {
                        try
                        {
                            Console.WriteLine("Copy File:" + info.FullName);
                            info.CopyTo(dest.FullName + @"\" + info.Name);
                        }
                        catch (Exception fileException)
                        {
                            Console.WriteLine(fileException.ToString());
                        }
                    }
                    else
                    {
                        FileInfo destFile = dest.GetFiles(info.Name)[0];

                        DateTime start = DateTime.Parse(info.LastWriteTime.ToString());
                        DateTime end = DateTime.Parse(destFile.LastWriteTime.ToString());
                        if (start > end)
                        {
                            try
                            {
                                Console.WriteLine("Copy existing File:" + info.FullName);
                                var file = dest.FullName + @"\" + info.Name;
                                File.Delete(file);
                                info.CopyTo(file, true);
                            }
                            catch (Exception fileException)
                            {
                                Console.WriteLine(fileException.ToString());
                            }
                        }
                    }
                }

                foreach (DirectoryInfo info in source.GetDirectories())
                {
                    if (ignoreNode && info.Name == "node_modules")
                    {
                        continue;
                    }

                    if (ignoreTFS && info.Name == "$tf")
                    {
                        continue;
                    }

                    DirectoryInfo di = null;
                    if (!destFolders.Contains(info.Name.ToUpper()))
                    {
                        di = dest.CreateSubdirectory(info.Name);
                        Console.WriteLine("Create Directory:" + di.FullName);
                    }
                    else
                    {
                        di = dest.GetDirectories(info.Name)[0];
                    }

                    ProcessDirectory(info, di);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
