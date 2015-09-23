// **********************************************
// YHGenomics Inc. Production
// Date       : 2015-09-23
// Author     : Shubo Yang
// Description: Help Building CMake File
// **********************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace CMaker
{
    class Program
    {
        public const string PROJECTNAME = "project";
        public const string FILETYPE = "ft";
        public const string OUT = "out";
        public const string COMPILER = "compiler";
        public const string FLAG = "flag";
        public const string DEBUG_FLAG = "debug";
        public const string AUTO = "auto";

        public static StringBuilder OutputData = new StringBuilder();
        public static Dictionary<string, string> Settings = new Dictionary<string, string>();

        static void Main(string[] args)
        {
            if (args.Length == 1 && args[0] == "-h")
            {
                ShowHelp();
                return;
            }
            else if(args.Length==0)
            {
                ShowHelp();
                return;
            }
            //Settings[PROJECTNAME]
            DefaultValue();
            ReadArrengment(args);

            if (!Settings.ContainsKey(PROJECTNAME) || string.IsNullOrEmpty(Settings[PROJECTNAME]))
            {
                ShowHelp();
                return;
            }

            //ProjectName = args[0];
            //FileType = args[1].Split(',');
            //GenerateType = args[2];

            OutputData.AppendLine(string.Format("project({0})", Settings[PROJECTNAME]));
            var files = ScanFiles(System.IO.Directory.GetCurrentDirectory());

            if(files.Count>0)
                OutputData.AppendLine(string.Format("set(SRC_LIST {0})", string.Join(" ",files.ToArray())));

            if (Settings[OUT] == "exe")
            {
                OutputData.AppendLine(string.Format("add_executable({0} {1})", Settings[PROJECTNAME], "${SRC_LIST}")); 
            }
            else if(Settings[OUT] == "lib")
            {
                OutputData.AppendLine(string.Format("add_library({0} {1})", Settings[PROJECTNAME], "${SRC_LIST}"));
            }

            for (int i = 3; i < args.Length; i++)
            {
                var linkLibs = args[i].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                OutputData.AppendLine(string.Format("target_link_libraries({0} {1})", Settings[PROJECTNAME], string.Join(" ",linkLibs)));
            }

            if(Settings.ContainsKey(COMPILER) && !string.IsNullOrEmpty(Settings[COMPILER]))
                OutputData.AppendLine(string.Format("SET (CMAKE_CXX_COMPILER \"{0}\")", Settings[COMPILER]));
            if (Settings.ContainsKey(FLAG) && !string.IsNullOrEmpty(Settings[FLAG]))
                OutputData.AppendLine(string.Format("SET (CMAKE_CXX_FLAGS \"{0}\")", Settings[FLAG]));
            if (Settings.ContainsKey(DEBUG_FLAG) && !string.IsNullOrEmpty(Settings[DEBUG_FLAG]))
                OutputData.AppendLine(string.Format("SET (CMAKE_CXX_FLAGS_DEBUG \"{0}\")", Settings[DEBUG_FLAG]));

            System.IO.File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "CMakeLists.txt"), OutputData.ToString());

            if (Settings.ContainsKey(AUTO) && !string.IsNullOrEmpty(Settings[AUTO]) && Settings[AUTO]=="true")
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "cmake";
                psi.Arguments = "./";
                Process.Start(psi).WaitForExit();

                psi = new ProcessStartInfo();
                psi.FileName = "make";
                Process.Start(psi);
            }
        }
        static void ShowHelp()
        {
            Console.WriteLine("CMaker");
            Console.WriteLine("YHGenomics Inc. Production");
            Console.WriteLine("CMaker project=name [options]");
            Console.WriteLine("options:");
            Console.WriteLine("       ft=*.h,*.cpp(default)");
            Console.WriteLine("       out=exe(default) - support exe,lib");
            Console.WriteLine("       compiler=/usr/bin/clang++(default) - support gcc,g++");
            Console.WriteLine("       flag=-Wall c++11(default)");
            Console.WriteLine("       debug=[null](default) - support -g");
            Console.WriteLine("       auto=false(default) - support -g : auto invoke cmake and make");
        }
        static void DefaultValue()
        {
            Settings[FILETYPE] = "*.cpp,*.h";
            Settings[OUT] = "exe";
            Settings[COMPILER] = "/usr/bin/clang++";
            Settings[FLAG] = "-Wall --std=c++11";
            Settings[DEBUG_FLAG] = "";
            Settings[AUTO] = "false";
        }
        static void ReadArrengment(string[] args)
        {
            foreach (var item in args)
            {
                string d = item.TrimStart().TrimEnd();
                
                var kv = item.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                if (kv.Length != 2)
                {
                    continue;
                }
                if (!Settings.ContainsKey(kv[0]))
                {
                    Settings.Add(kv[0], null);
                }
                Settings[kv[0]] = kv[1];
            }
        }
        static List<string> ScanFiles(string directory)
        {
            List<string> ret = new List<string>();
            Console.WriteLine("Scaning Directory:"+ directory);
            var types = Settings[FILETYPE].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in types)
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
