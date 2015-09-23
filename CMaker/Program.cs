// **********************************************
// YHGenomics Inc. Production
// Date       : 2015-09-23
// Author     : Shubo Yang
// Description: Help Building CMake File
// **********************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CMaker
{
    class Program
    {
        public static StringBuilder OutputData = new StringBuilder();
        public static string ProjectName = "";
        public static string GenerateType = "";
        public static string[] FileType;
        static void Main(string[] args)
        {
            if(args.Length == 1 && args[0] == "-h")
            {
                Console.WriteLine("[Project Name] [File Type(*.h,*.cpp)] [lib(exe)]");
                return;
            }
            else if(args.Length<3)
            {
                Console.WriteLine("Input arrangements error");
                Console.WriteLine("ProjectName FileType(*.h,*.cpp) lib(lib,exe) [link libs(a.o,b.o,c.o]");
                return;
            }

            ProjectName = args[0];
            FileType = args[1].Split(',');
            GenerateType = args[2];

            OutputData.AppendLine(string.Format("project({0})", ProjectName));
            var files = ScanFiles(System.IO.Directory.GetCurrentDirectory());

            if(files.Count>0)
                OutputData.AppendLine(string.Format("set(SRC_LIST {0})", string.Join(" ",files.ToArray())));

            if (GenerateType == "exe")
            {
                OutputData.AppendLine(string.Format("add_executable({0} {1})", ProjectName, "${SRC_LIST}")); 
            }
            else if(GenerateType =="lib")
            {
                OutputData.AppendLine(string.Format("add_library({0} {1})", ProjectName, "${SRC_LIST}"));
            }

            for (int i = 3; i < args.Length; i++)
            {
                var linkLibs = args[i].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                OutputData.AppendLine(string.Format("target_link_libraries({0} {1})", ProjectName, string.Join(" ",linkLibs)));
            }

            System.IO.File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "CMakeLists.txt"), OutputData.ToString());
        }

        static List<string> ScanFiles(string directory)
        {
            List<string> ret = new List<string>();
            Console.WriteLine("Scaning Directory:"+ directory);
            foreach (var item in FileType)
            {
                var files = System.IO.Directory.GetFiles(directory, item);
                foreach (var f in files)
                {
                    Console.WriteLine("Add File:" + f);
                    ret.Add(f);
                }
            }

            var dirs = System.IO.Directory.GetDirectories(directory);
            foreach (var item in dirs)
            {
                ret.AddRange(ScanFiles(item));
            }

            return ret;
        }
    }
}
