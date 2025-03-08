using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

// NOTE: SAC stands for Set As Current

namespace ModHandler
{
    class Program
    {
        static void Main(string[] args)
        {
            string Commands = "mh new <name>: create a new project in the SACked folder (the folder set as current)\nmh sac <id>: set an id created as the id that projects will be created in\nmh add-id <id>: add the current directory as a mod folder\nmh link: automatically link the exe folder to the PATH variable\nmh get-sacked: get the id set as current\nmh get-ids: get all the available ids.\nmh clear-all: clear all ids and the sacked id.\nmh remove <id>: remove the specified id from memory. CANNOT BE UNDONE!\nmh delete <name>: delete the specified mod folder from the current id.";

            if (args.Length >= 1)
            {
                if (args[0] == "new" & args.Length == 2)
                {
                    NewProj(args[1]);
                    return;
                }

                if (args[0] == "sac" & args.Length == 2)
                {
                    SAC(args[1]);
                    return;
                }

                if (args[0] == "add-id" & args.Length == 2)
                {
                    AddModDir(args[1]);
                    return;
                }

                if (args[0] == "link")
                {
                    Link();
                    return;
                }

                if (args[0] == "get-sacked")
                {
                    if (GetSACked() != null)
                    {
                        Console.WriteLine(GetSACked());
                        return;
                    } else
                    {
                        Console.WriteLine("Please sac an id");
                        return;
                    }
                } 

                if (args[0] == "get-ids")
                {
                    string[] IDs = GetIDs();

                    if (IDs != null)
                    {
                        foreach (var ID in IDs)
                        {
                            Console.WriteLine(ID);
                        }

                        return;
                    } else
                    {
                        Console.WriteLine("No IDs have been created.");
                        return;
                    }
                }

                if (args[0] == "clear-all")
                {
                    ClearAll();
                    return;
                }

                if (args[0] == "remove" & args.Length == 2)
                {
                    RemoveID(args[1]);
                    return;
                }

                if (args[0] == "delete" & args.Length == 2)
                {
                    Delete(args[1]);
                    return;
                }

                Console.WriteLine("Please enter a valid command:");
                Console.Write(Commands);
            } else
            {
                Console.Write(Commands);
            }
        }

        static string ExeDir()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        static void Link()
        {
            var Name = "PATH";
            var Scope = EnvironmentVariableTarget.User; // or User
            var OldValue = Environment.GetEnvironmentVariable(Name, Scope);
            string ApplicationDir = ExeDir();
            var NewValue = OldValue + ApplicationDir;

            Environment.SetEnvironmentVariable(Name, NewValue, Scope);
        }

        static void AddModDir(string Id)
        {
            string Dir = Directory.GetCurrentDirectory();
            File.AppendAllLines("./dirs.txt", [Id + ": " + Dir]);

            Console.WriteLine("Path added successfully.");
        }

        static string GetPath(string Id)
        {
            string[] Lines;

            if (Id == null)
            {
                return null;
            }

            if (File.Exists("./dirs.txt") && File.ReadAllText("./dirs.txt") != "")
            {
                Lines = File.ReadAllLines("./dirs.txt");
            } else
            {
                Console.WriteLine("ERROR: No IDs have been created.");
                return null;
            }

            string CurrentId = "", Path = "";
            string[] Split;

            for (int i = 0; i < Lines.Length; i++) 
            {
                Split = Lines[i].Split(": ");

                CurrentId = Split[0];
                Path = Split[1];

                if (CurrentId == Id)
                {
                    return Path;
                }
            }

            return null;
        }

        static string[] GetIDs()
        {
            if (File.Exists("./dirs.txt"))
            {
                if (File.ReadAllText("./dirs.txt") != "") 
                {
                    return File.ReadAllLines("./dirs.txt");
                } else
                {
                    return null;
                }
            } else
            {
                return null;
            }
        }

        static void RemoveID(string ID)
        {
            if (GetPath(ID) != null)
            {
                List<string> Lines = new List<string>(File.ReadAllLines("./dirs.txt"));

                foreach (var Line in Lines.ToArray())
                {
                    if (Line.StartsWith(ID))
                    {
                        Lines.Remove(Line);
                    }
                }

                File.WriteAllLines("./dirs.txt", Lines);
                Console.WriteLine("ID removed successfully");
            } else
            {
                Console.WriteLine("ERROR: ID " + ID + " IS Nonexistent.");
            }
        }

        static void ClearAll()
        {
            if (File.Exists("./dirs.txt") & File.Exists("./curdir.txt"))
            {
                File.Delete("./dirs.txt");
                File.Delete("./curdir.txt");

                Console.WriteLine("Everything was successfully cleared.");
            }
        }

        static void SAC(string Id)
        {            
            if (GetPath(Id) != null)
            {
                File.WriteAllText("./curdir.txt", Id);
                Console.WriteLine("ID set as current");
            } else
            {
                Console.WriteLine("ERROR: ID " + Id + " Nonexistent");
            }

        }

        static string GetSACked()
        {
            if (File.Exists("./curdir.txt") && File.ReadAllText("./curdir.txt") != "")
            {
                return File.ReadAllText("./curdir.txt");
            } else
            {
                return null;
            }
        }

        static void NewProj(string Name)
        {
            string currentId = GetSACked();
            if (string.IsNullOrEmpty(currentId))
            {
                Console.WriteLine("ERROR: No current ID set. Please set one.");
                return;
            }

            string Path = GetPath(currentId);

            if (string.IsNullOrEmpty(Path))
            {
                Console.WriteLine("ERROR: Path is null. Either you need to set a current ID or add an ID.");
                return;
            }

            string RegexStr, VarName;
            string[] Lines = File.ReadAllLines("./mod.conf.template");
            string[] ModData = new string[Lines.Length];
            int i = 0;

            foreach (var Line in Lines)
            {
                if (Lines[i].Contains(", "))
                {
                    VarName = Line.Split(", ")[0].Trim();
                    RegexStr = Line.Split(", ")[1].Trim();

                    Console.Write(VarName + ": ");

                    ModData[i] = Console.ReadLine();

                    if (ModData[i] == null)
                    {
                        Console.WriteLine("ERROR: No input provided.");
                        return;
                    }

                    if (!Regex.IsMatch(ModData[i], RegexStr))
                    {
                        Console.WriteLine("Input for " + VarName + " does not follow conventions listed in mod.conf.template: " + RegexStr);
                        return;
                    }
                } else
                {
                    VarName = Line;

                    Console.Write(VarName + ": ");

                    ModData[i] = Console.ReadLine();

                    if (ModData[i] == null)
                    {
                        Console.WriteLine("ERROR: No input provided.");
                        return;
                    }
                }

                i++;
            }

            if (Path != null)
            {
                CopyFiles(Path + "\\" + Name + "\\");

                for (int j = 0; j < ModData.Length; j++)
                {
                    if (Lines[j].Contains(", "))
                    {
                        VarName = Lines[j].Split(", ")[0].Trim();
                    } else
                    {
                        VarName = Lines[j].Trim();
                    }

                    File.AppendAllText(Path + "\\" + Name + "\\mod.conf", VarName + " = " + ModData[j] + "\n");
                }
                
            } else
            {
                Console.WriteLine("ERROR: PLEASE SAC AN ID: mh sac <id>");
            }
        }

        static void Delete(string Mod)
        {
            string Path = GetPath(GetSACked());

            if (Directory.Exists(Path + "\\" + Mod))
            {
                Directory.Delete(Path + "\\" + Mod, true);
            } else
            {
                Console.WriteLine("ERROR: Directory does not exist. Are you using the wrong ID?");
            }
        }

        private static void CopyFiles(string TargetPath)
        {
            string SourcePath = "./mod_eg/";

            foreach (string DirPath in Directory.GetDirectories(SourcePath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(DirPath.Replace(SourcePath, TargetPath));
            }

            
            foreach (string NewPath in Directory.GetFiles(SourcePath, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(NewPath, NewPath.Replace(SourcePath, TargetPath), true);
            }
        }
    }
}