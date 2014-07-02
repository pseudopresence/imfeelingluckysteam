using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

// TODO needs fixing for current format? Compare diffs taken during and after downloads

// TODO Make it possible to launch only games with a certain Category

// TODO it seems a better option would be to look at all steamapps directories
// "G:\Steam\steamapps\appmanifest_*.acf"
// "D:\SteamLibrary\steamapps\appmanifest_*.acf"
// If a game is installed, it will have the line "StateFlags"		"4"
// App id is in line "appID"		"242920" (for example) and it will also be in the filename

namespace SteamImFeelingLucky {
    class Program {
        static void parseACF(StreamReader reader, List<String> ids) {
          char[] delim = { '\t', ' ', '\"' };
          bool is_installed = false;
          string id = null;
          while (!reader.EndOfStream) {
            String line = reader.ReadLine().Trim();
            string[] tokens = line.Split(delim, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.GetLength(0) == 2) {
              if (tokens[0] == "StateFlags" && tokens[1] == "4") {
                is_installed = true;
              }
              if (tokens[0] == "appID") {
                id = tokens[1];
              }
            }
          }
          if (is_installed && id != null) {
            Console.WriteLine("Found installed game id " + id);
            ids.Add(id);
          }
        }

        static void Main(string[] args) {
            // TODO HAX
            string[] steamAppsPaths = { @"G:\Steam\steamapps", @"D:\SteamLibrary\steamapps" };
            List<String> ids = new List<String>();

            foreach (string path in steamAppsPaths) {
              string[] manifests = Directory.GetFiles(path, "appmanifest_*.acf");
              foreach (string filename in manifests) {
                Console.WriteLine("Opening " + filename);
                StreamReader reader = new StreamReader(new FileStream(filename, FileMode.Open));
                parseACF(reader, ids);
              }
            }

            Console.Write(ids.Count + " games found");
            Random gen = new Random();
            int chosen = gen.Next(ids.Count);

            Console.WriteLine("steam://run/" + ids[chosen]);
            System.Diagnostics.Process.Start("steam://run/" + ids[chosen]);
            
            Console.WriteLine("Exiting!");
            return;

#if FALSE
            string[] filePaths = Directory.GetDirectories(SteamInstallDir() + @"\userdata\");

            string path = filePaths[0] + @"\config\localconfig.vdf";

            FileStream steam = new FileStream(path, FileMode.Open);

            StreamReader reader = new StreamReader(steam);

            bool softwareSeen = false;
            bool appsSeen = false;

            

            Console.WriteLine("----------------------------------------------");
            while (!reader.EndOfStream) {
                String line = reader.ReadLine().Trim();

                switch (line) {
                    case "\"Software\"":
                        Console.WriteLine("Found Software folder");
                        softwareSeen = true;
                        break;
                    case "\"apps\"":
                        if (softwareSeen) {
                            Console.WriteLine("Found apps folder");
                            appsSeen = true;
                            reader.ReadLine(); //skip "{" line
                            line = reader.ReadLine();
                        }
                        break;
                    default:
                        break;
                }

                if (softwareSeen && appsSeen) {
                    char[] lineDelims = { '\"', '\t', ' ' };
                    string num = line.Trim(lineDelims);

                    if (num == "7") {
                        continue;
                    }

                    reader.ReadLine();   //skip "{" line
                    line = reader.ReadLine();
                    if (line != null) {
                        line = line.Trim();
                    } else {
                        break;
                    }

                    bool updateKBtoDLExists = false;

                    while (line != "}") {
                        if (updateKBtoDLExists && line.StartsWith("\"HasAllLocalContent")) {
                            char[] delim = { '\t', ' ', '\"' };
                            string[] tokens = line.Split(delim, StringSplitOptions.RemoveEmptyEntries);

                            string check = tokens[1];

                            if (check == "1") {
                                //woohoo!
                                Console.WriteLine("Found game id " + num);
                                ids.Add(num);
                            }
                        } else if (line.StartsWith("\"UpdateKBtoDL\"")) {
                            Console.WriteLine("Found UpdateKBtoDL");
                            updateKBtoDLExists = true;
                        }
                        line = reader.ReadLine().Trim(); ;
                    }
                }
            }
            Console.WriteLine("----------------------------------------------");

            if (ids.Count > 0) {
                Console.Write(ids.Count + " games found");
                Random gen = new Random();
                int chosen = gen.Next(ids.Count);

                Console.WriteLine("steam://run/" + ids[chosen]);
                System.Diagnostics.Process.Start("steam://run/" + ids[chosen]);
            }
            else
            {
                Console.Write("No games found!");
            }
#endif
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
