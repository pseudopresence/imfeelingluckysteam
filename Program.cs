using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace SteamImFeelingLucky {
    class Program {
        static void Main(string[] args) {
            string[] filePaths = Directory.GetDirectories(SteamInstallDir() + @"\userdata\");

            string path = filePaths[0] + @"\config\localconfig.vdf";

            FileStream steam = new FileStream(path, FileMode.Open);

            StreamReader reader = new StreamReader(steam);

            bool softwareSeen = false;
            bool appsSeen = false;

            List<String> ids = new List<String>();

            while (!reader.EndOfStream) {
                String line = reader.ReadLine().Trim();

                switch (line) {
                    case "\"Software\"":
                        softwareSeen = true;
                        break;
                    case "\"apps\"":
                        if (softwareSeen) {
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
                                ids.Add(num);
                            }
                        } else if (line.StartsWith("\"UpdateKBtoDL\"")) {
                            updateKBtoDLExists = true;
                        }
                        line = reader.ReadLine().Trim(); ;
                    }
                }

            }

            if (ids.Count > 0) {
                Console.Write(ids.Count + " games found");
                Random gen = new Random();
                int chosen = gen.Next(ids.Count);

                Console.Write("steam://run/" + ids[chosen]);
                System.Diagnostics.Process.Start("steam://run/" + ids[chosen]);
            }
            else
            {
                Console.Write("No games found!");
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

                return dumbAssumption;
            }

            ProcessModule steamProcessModule = steamProcess.MainModule;
            string exeDir = steamProcessModule.FileName;
            string installDir = exeDir.Substring(0, exeDir.LastIndexOf("\\")); // Steam install directory
            return installDir;
        }
    }
}
