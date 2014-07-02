using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

// TODO Make it possible to launch only games with a certain Category

namespace SteamImFeelingLucky {
    class Program {
        static void Main(string[] args)
        {
          // TODO HAX
          List<string> steamAppsPaths = new List<string>();
          // Add the best guess at the steam install dir
          string steamInstallDir = SteamInstallDir();
          steamAppsPaths.Add(steamInstallDir + @"\steamapps");
          // Now try to parse the config file for other folders
          // Open the config file 
          {
            string configPath = steamInstallDir + @"\config\config.vdf";
            // Console.WriteLine("Opening " + configPath);
            StreamReader reader = new StreamReader(new FileStream(configPath, FileMode.Open));
            parseConfig(reader, steamAppsPaths);
          }

          foreach (string path in steamAppsPaths)
          {
            Console.WriteLine("Found steamapps path: " + path);
          }

          List<String> ids = new List<String>();

          foreach (string path in steamAppsPaths)
          {
            string[] manifests = Directory.GetFiles(path, "appmanifest_*.acf");
            foreach (string filename in manifests)
            {
              // Console.WriteLine("Opening " + filename);
              StreamReader reader = new StreamReader(new FileStream(filename, FileMode.Open));
              parseManifest(reader, ids);
            }
          }

          Console.WriteLine(ids.Count + " games found");
          Random gen = new Random();
          int chosen = gen.Next(ids.Count);

          Console.WriteLine("running steam://run/" + ids[chosen]);
          System.Diagnostics.Process.Start("steam://run/" + ids[chosen]);

          Console.WriteLine("All done!");
        }
      
        static void parseManifest(StreamReader reader, List<string> o_ids)
        {
          char[] delim = { '\t', ' ', '\"' };
          bool isInstalled = false;
          string id = null;
          while (!reader.EndOfStream)
          {
            String line = reader.ReadLine().Trim();
            string[] tokens = line.Split(delim, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.GetLength(0) == 2)
            {
              if (tokens[0] == "StateFlags" && tokens[1] == "4")
              {
                isInstalled = true;
              }
              if (tokens[0] == "appID")
              {
                id = tokens[1];
              }
            }
          }
          if (isInstalled && id != null)
          {
            // Console.WriteLine("Found installed game id " + id);
            o_ids.Add(id);
          }
        }

        static void parseConfig(StreamReader reader, List<string> o_steamAppsPaths)
        {
          char[] delim = { '\t', ' ', '\"' };
          while (!reader.EndOfStream)
          {
            String line = reader.ReadLine().Trim();
            string[] tokens = line.Split(delim, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.GetLength(0) == 2)
            {
              if (tokens[0].StartsWith("BaseInstallFolder"))
              {
                o_steamAppsPaths.Add(tokens[1].Replace(@"\\", @"\") + @"\steamapps");
              }
            }
          }
        }

        static string ProgramFilesx86() {
            if (8 == IntPtr.Size
                || (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432")))) {
                return Environment.GetEnvironmentVariable("ProgramFiles(x86)");
            }

            return Environment.GetEnvironmentVariable("ProgramFiles");
        }

        //thanks onefifth; had to modify a bit though ;)
        static string SteamInstallDir() {
            Process[] steamProcessArray;
            Process steamProcess = null;
            steamProcessArray = Process.GetProcessesByName("steam");

            if (steamProcessArray.Length >= 1) {
                steamProcess = steamProcessArray[0];
            } else {
                //Steam is not running; try returning dir under program files....
                string dumbAssumption = ProgramFilesx86() + @"\Steam";
                Console.WriteLine("I have no idea so let's pretend steam install dir is: " + dumbAssumption);
                return dumbAssumption;
            }

            ProcessModule steamProcessModule = steamProcess.MainModule;
            string exeDir = steamProcessModule.FileName;
            string installDir = exeDir.Substring(0, exeDir.LastIndexOf("\\")); // Steam install directory
            Console.WriteLine("I'm pretty sure your steam install dir is: " + installDir);
            return installDir;
        }
    }
}
