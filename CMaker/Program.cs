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
        public const string SRC = "src";
        public const string HEAD = "head";
        public const string OUT = "out";
        public const string COMPILER = "compiler";
        public const string FLAG = "flag";
        public const string DEBUG_FLAG = "debug";
        public const string AUTO = "auto";
        public const string LIB = "lib";

        const string CMakeFileDirectoryName = "cmakebuild";

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
            
            DefaultValue();
            ReadArrengment(args);

            if (!Settings.ContainsKey(PROJECTNAME) || string.IsNullOrEmpty(Settings[PROJECTNAME]))
            {
                ShowHelp();
                return;
            }

            OutputData.AppendLine(string.Format("project({0})", Settings[PROJECTNAME]));

            if (Settings.ContainsKey(HEAD) && !string.IsNullOrEmpty(Settings[HEAD]))
            {
                var headers = ScanFolder(System.IO.Directory.GetCurrentDirectory(), Settings[HEAD].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries));
                if (headers.Count > 0)
                {
                    foreach (var item in headers)
                    {
                        OutputData.AppendLine(string.Format("INCLUDE_DIRECTORIES({0})", item));
                    }
                }
            }

            var files = ScanFiles(System.IO.Directory.GetCurrentDirectory(), Settings[SRC].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries));
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

            if(Settings.ContainsKey(COMPILER) && !string.IsNullOrEmpty(Settings[COMPILER]))
                OutputData.AppendLine(string.Format("set (CMAKE_CXX_COMPILER \"{0}\")", Settings[COMPILER]));

            if (Settings.ContainsKey(FLAG) && !string.IsNullOrEmpty(Settings[FLAG]))
            {
                var flags = Settings[FLAG].Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                string dat = String.Join(" -", flags);
                OutputData.AppendLine(string.Format("set (CMAKE_CXX_FLAGS \"-{0}\")", dat));
            }

            if (Settings.ContainsKey(DEBUG_FLAG) && !string.IsNullOrEmpty(Settings[DEBUG_FLAG]))
            {
                OutputData.AppendLine(string.Format("set (CMAKE_BUILD_TYPE Debug)"));
                //OutputData.AppendLine(string.Format("set (CMAKE_CXX_FLAGS_DEBUG \"{0}\")", Settings[DEBUG_FLAG]));
            }

            if (Settings.ContainsKey(LIB) && !string.IsNullOrEmpty(Settings[LIB]))
            {
                var libsArray = Settings[LIB].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                List<string> list = new List<string>();

                foreach (var item in libsArray)
                {
                    OutputData.AppendLine(string.Format("target_link_libraries({0} {1})", Settings[PROJECTNAME], System.IO.Path.GetFullPath(item)));
                }
            }

            Console.WriteLine("Creating Directory:" + Directory.GetCurrentDirectory() + "/"+ CMakeFileDirectoryName);
            System.IO.Directory.CreateDirectory(Directory.GetCurrentDirectory()+ "/"+ CMakeFileDirectoryName);
            string make_directory = Directory.GetCurrentDirectory() + "/"+ CMakeFileDirectoryName+"/";
            Console.WriteLine("Creating CMakeFile:" + make_directory + "CMakeLists.txt");
            System.IO.File.WriteAllText(Path.Combine(make_directory, "CMakeLists.txt"), OutputData.ToString());

            if (Settings.ContainsKey(AUTO) && !string.IsNullOrEmpty(Settings[AUTO]) && Settings[AUTO]=="true")
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.WorkingDirectory = make_directory;
                psi.FileName = "cmake";
                psi.Arguments = "./";

                Process.Start(psi).WaitForExit();

                psi = new ProcessStartInfo();
                psi.WorkingDirectory = make_directory;
                psi.FileName = "make";
                Process.Start(psi);
            }
        }
        static void ShowHelp()
        {
            Console.WriteLine("CMaker");
            Console.WriteLine("YHGenomics Inc. Production");
            Console.WriteLine("Build 2016-08-05");
            Console.WriteLine("CMaker project:name [options]");
            Console.WriteLine("options:");
            Console.WriteLine("       head:[null](default)");
            Console.WriteLine("       src:*.cpp,*.hpp(default)");
            Console.WriteLine("       out:exe(default) - support exe,lib");
            Console.WriteLine("       compiler:/usr/bin/clang(default) - support gcc,g++");
            Console.WriteLine("       flag:-Wall-std=c++11-pthread(default)");
            Console.WriteLine("       debug:[null](default) - support -g");
            Console.WriteLine("       auto:false(default) - support -g : auto invoke cmake and make");
            Console.WriteLine("       lib:libc++.a - support libxxx.o,libyyy.o");
        }
        static void DefaultValue()
        {
            Settings[SRC] = "*.cpp,*.hpp";
            //Settings[HEAD] = "*.h";
            Settings[OUT] = "exe";
            Settings[COMPILER] = "g++";
            Settings[FLAG] = "-Wall-std=c++11-pthread";
            Settings[DEBUG_FLAG] = "";
            Settings[AUTO] = "false";
            Settings[LIB] = "";
        }
        static void ReadArrengment(string[] args)
        {
            foreach (var item in args)
            { 
                var kv = item.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
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
        static List<string> ScanFiles(string directory, string[] externNames)
        {
            List<string> ret = new List<string>();
            Console.WriteLine("Scaning Directory:"+ directory);
            var types = externNames; 
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
                ret.AddRange(ScanFiles(item, externNames));
            }

            return ret;
        }

        static List<string> ScanFolder(string directory, string[] externNames)
        {
            List<string> ret = new List<string>();
            Console.WriteLine("Scaning Directory:" + directory);
            var types = externNames;
            foreach (var item in types)
            {
                var files = System.IO.Directory.GetFiles(directory, item);
                foreach (var f in files)
                {
                    Console.WriteLine("Add Header Folder:" + Path.GetDirectoryName(f));
                    ret.Add(Path.GetDirectoryName(f));
                }
            }

            var dirs = System.IO.Directory.GetDirectories(directory);
            foreach (var item in dirs)
            {
                ret.AddRange(ScanFolder(item, externNames));
            }



            return ret.Distinct().ToList();
        }
    }
}


