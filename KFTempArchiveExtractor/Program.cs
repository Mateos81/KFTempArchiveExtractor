using System;
using System.IO;
using System.Linq;

namespace TempArchiveExtractor
{
    /// <summary>
    /// Main class.
    /// </summary>
    public class Program
    {
        #region Fields

        /// <summary>
        /// Base path where to create a folder per Archive file.
        /// </summary>
        private static string _baseExtractionPath = string.Empty;

        /// <summary>
        /// Indicates if either the original behaviour is used or not.
        /// </summary>
        private static bool _originalBehaviour = true;

        #endregion Fields

        #region Main

        /// <summary>
        /// Main function.
        /// </summary>
        /// <param name="args">Application arguments.</param>
        public static void Main(string[] args)
        {
            if (args.Length != 0)
            {
                OriginalBehaviour(args);

                ProcessOverPausing();

                return;
            }

            // New behaviour
            _originalBehaviour = false;

            // List the KF Archive files to process
            DirectoryInfo kf = new DirectoryInfo(GetKFInstallDir());
            FileInfo[] archiveFiles = kf.GetFiles().Where(x => x.Name.StartsWith("TempArchive")).ToArray();

            if (archiveFiles.Length == 0)
            {
                Console.WriteLine("No file to process...");
                return;
            }

            // If needed, create the sub-folder in application's path
            _baseExtractionPath =
                Path.Combine(
                    Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                    "KF Archive Files");

            if (!Directory.Exists(_baseExtractionPath))
            {
                Directory.CreateDirectory(_baseExtractionPath);
            }

            // Process the files
            foreach (FileInfo archiveFile in archiveFiles)
            {
                Process(archiveFile);
            }

            ProcessOverPausing();
        }

        #endregion Main

        #region Methods

        /// <summary>
        /// Retrieves KF installation directory from the system registry.
        /// </summary>
        /// <returns>The path to the local KF installation directory.</returns>
        private static string GetKFInstallDir()
        {
            return Microsoft.Win32.Registry.GetValue(
                @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 1250",
                "InstallLocation",
                null).ToString();
        }

        /// <summary>
        /// Original behaviour: drag and drop one file onto the executable.
        /// One difference though: the <see cref="Process(FileInfo)"/> function will create a sub-folder per Archive file.
        /// </summary>
        /// <param name="args">Application arguments.</param>
        private static void OriginalBehaviour(string[] args)
        {
            if (0 == args.Length || string.IsNullOrWhiteSpace(args[0]))
            {
                Console.Out.WriteLine("Usage: TempArchiveExtractor [TempArchiveFile]");
                return;
            }

            FileInfo archiveFile = new FileInfo(args[0]);

            if (!Process(archiveFile))
            {
                Console.WriteLine($"File {archiveFile.Name} incorrectly processed.");
            }
        }

        /// <summary>
        /// Processes a KF Archive file.
        /// </summary>
        /// <param name="archiveFile">KF Archive file.</param>
        /// <returns>true if the file has been correctly processed, false otherwise.</returns>
        private static bool Process(FileInfo archiveFile)
        {
            Console.WriteLine();

            if (!archiveFile.Exists)
            {
                Console.Out.WriteLine("No such file.");

                ProcessOverPausing();

                return false;
            }

            Console.WriteLine($"Processing file {archiveFile.Name}...");

            FileStream archiveStream = null;
            try
            {
                archiveStream = archiveFile.OpenRead();
                TempArchiveReader reader = new TempArchiveReader(archiveStream);

                int fileCount = reader.ReadInt32();
                Console.Out.WriteLine($"Archive contains {fileCount} files.");

                // Create own Archive file folder if needed
                DirectoryInfo archiveOwnFolder = new DirectoryInfo(
                    Path.Combine(
                        _baseExtractionPath,
                        archiveFile.Name));

                if (!archiveOwnFolder.Exists)
                {
                    Console.WriteLine($"Creating {archiveOwnFolder.Name} folder.");
                    archiveOwnFolder.Create();
                }

                // Process each inner file from the Archive file
                for (int i = 0; i < fileCount; i++)
                {
                    string filePath = reader.ReadFString();
                    int fileLen = reader.ReadFCompactIndex();

                    FileInfo fi =
                        new FileInfo(
                            Path.Combine(
                                archiveOwnFolder.FullName,
                                filePath));

                    // Either confirm each file or extract everything without prompt
                    if (_originalBehaviour)
                    {
                        string msg =
                            string.Format(
                                "{0}  {1}  Size: 0x{2:X} {2:N0}  [Y/N] : ",
                                fi.Exists ? "Overwrite" : "Extract",
                                filePath,
                                fileLen);
                        Console.Out.Write(msg);
                        string resp = Console.In.ReadLine();
                        if ("Y".Equals(resp.ToUpper()))
                        {
                            DirectoryInfo di = fi.Directory;
                            if (!di.Exists)
                            {
                                di.Create();
                            }

                            using (FileStream fs = fi.OpenWrite())
                            {
                                reader.ReadIntoStream(fileLen, fs);
                            }
                        }
                        else
                        {
                            reader.Skip(fileLen);
                        }
                    }
                    else
                    {
                        Console.Out.WriteLine($"Extracting {filePath}.");

                        DirectoryInfo di = fi.Directory;
                        if (!di.Exists)
                        {
                            di.Create();
                        }

                        using (FileStream fs = fi.OpenWrite())
                        {
                            reader.ReadIntoStream(fileLen, fs);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine("Error reading archive.");
                Console.Out.WriteLine(ex);

                return false;
            }
            finally
            {
                archiveStream?.Close();
            }

            Console.WriteLine($"{archiveFile.Name} processed successfully.");

            return true;
        }

        /// <summary>
        /// Pausing the console to leave it open, so the user can read the output.
        /// </summary>
        private static void ProcessOverPausing()
        {
            Console.WriteLine();
            Console.Write("Process finished. Pausing...");
            Console.Read();
        }

        #endregion Methods
    }
}
