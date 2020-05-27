using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Media;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Windows.Threading;

namespace ProcessSimulateImportConditioner
{
    public static class Utils
    {
        public static Dispatcher GUIDispatcher { get; set; }

        public static Color WarningColour { get { return Color.FromArgb(75, 255, 69, 0); } }
        public static Color OrangeRed { get { return Colors.OrangeRed; } }
        public static Color LimeGreen { get { return Colors.LimeGreen; } }

        public static Color GreyColour { get { return Color.FromArgb(35, 220, 220, 220); } }

        private static Random random = new Random();
        public static bool RandomBool { get { return random.Next(100) < 50; } }
        public static double RandomDouble { get { return random.NextDouble(); } }

        public static int RandomInt() { return random.Next(); }
        public static int RandomInt(int max) { return random.Next(max); }
        public static int RandomInt(int min, int max) { return random.Next(min, max); }

        public static string PartClassString { get { return "PmCompoundPart;PmPartPrototype;PmPartInstance"; } }
        public static string ResourceClassString { get { return "PmCompoundResource;PmResourcePrototype;PmResourceInstance"; } }

        public static bool PathIsValid(string path)
        {
            if (!Path.IsPathRooted(path)) return false;

            try
            {
                Path.GetFullPath(path);
                return true;
            }

            catch (Exception)
            {
                return false;
            }
        }

        public static string NewTempDirectory
        {
            get
            {
                return System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName());
            }
        }

        public static TaskCompletionSource<Tuple<XElement, string>> ConvertInput(Input input)
        {
            var promise = new TaskCompletionSource<Tuple<XElement, string>>();

            var tempDirectory = NewTempDirectory;
            
            var outputFileName = input.PartName + ".xml";

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = ApplicationData.ConverterExeName,
                    Arguments = String.Format("\"{0}\" -TxJT2cojt -extIdMod 3 {1} -dest \"{2}\" -output \"{3}\"", new object[] { input.JTPath, input.PartClass ? "" : "-emsClass defaultResource", tempDirectory, outputFileName }),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                },
                EnableRaisingEvents = true
            };

            try
            {
                if (process.Start())
                {
                    DataReceivedEventHandler processOutputDataReceived = (object sender, DataReceivedEventArgs dataReceivedEventArgs) => Console.WriteLine(dataReceivedEventArgs.Data);

                    DataReceivedEventHandler processErrorDataReceived = (object processObject, DataReceivedEventArgs dataReceivedEventArgs) => Console.Error.WriteLine(dataReceivedEventArgs.Data);

                    EventHandler processExited = null;
                    processExited = (object processObject, EventArgs exitedEventArguments) =>
                    {
                        process.OutputDataReceived -= processOutputDataReceived;
                        process.ErrorDataReceived -= processErrorDataReceived;
                        process.Exited -= processExited;

                        var pathToXMLDocument = Path.Combine(tempDirectory, outputFileName);

                        if (File.Exists(pathToXMLDocument))
                        {
                            var xmlDocument = XElement.Load(pathToXMLDocument);
                            var fileNameElements = xmlDocument.Descendants("fileName").ToArray();

                            for (int i = 0, c = fileNameElements.Length; i < c; ++i)
                            {
                                var fileNameElement = fileNameElements[i];

                                var existingPath = fileNameElement.Value.TrimStart(new char[] { '#' });

                                var existingFileName = Path.GetFileNameWithoutExtension(existingPath);
                                var newFileName = i.ToString() + ".cojt";

                                var newPath = Path.Combine(GetPathRelativeTo(input.OutputDirectory, ApplicationData.Service.SysRootPath).TrimStart(new char[] { Path.DirectorySeparatorChar }), newFileName);

                                fileNameElement.SetValue("#" + newPath);

                                var outputCOJTDirectory = Path.Combine(input.OutputDirectory, newFileName);

                                Directory.CreateDirectory(outputCOJTDirectory);

                                var existingJTFilePath = Path.Combine(existingPath, existingFileName + ".jt");

                                if (!File.Exists(existingJTFilePath))
                                {
                                    var existingJTFilePathTmp = Path.Combine(existingPath + ".tmp", existingFileName + ".jt");

                                    if (!File.Exists(existingJTFilePathTmp))
                                    {
                                        Utils.GUIDispatcher.Invoke(() =>
                                        {
                                            ApplicationData.Service.Errors.Add(new TranslationError()
                                            {
                                                Timestamp = DateTime.Now,
                                                JTPath = input.JTPath,
                                                Description = String.Format("Conversion error: '{0}' not found.", existingJTFilePath)
                                            });
                                        });

                                        continue;
                                    }

                                    else
                                    {
                                        existingJTFilePath = existingJTFilePathTmp;
                                    }
                                }

                                var newJTFilePath = Path.Combine(outputCOJTDirectory, i.ToString() + ".jt");

                                File.Copy(existingJTFilePath, newJTFilePath, true);
                            }

                            promise.TrySetResult(new Tuple<XElement, string>(xmlDocument, outputFileName));
                        }

                        else
                        {
                            Utils.GUIDispatcher.Invoke(() =>
                             {
                                 ApplicationData.Service.Errors.Add(new TranslationError()
                                 {
                                     Timestamp = DateTime.Now,
                                     JTPath = input.JTPath,
                                     Description = "Conversion failed: XML file was not created."
                                 });
                             });

                            promise.TrySetResult(null);
                        }

                        Directory.Delete(tempDirectory, true);
                    };

                    process.OutputDataReceived += processOutputDataReceived;
                    process.ErrorDataReceived += processErrorDataReceived;
                    process.Exited += processExited;

                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                }
            }

            catch (Exception exception)
            {
                Console.Error.WriteLine(exception.Data);
            }

            return promise;
        }

        public static bool DirectoryIsEmpty(string path)
        {
            if (!Directory.Exists(path)) return true;

            using (var enumerator = Directory.EnumerateFileSystemEntries(path).GetEnumerator())
                return !enumerator.MoveNext();
        }

        public static bool PathsAreSame(string path1, string path2)
        {
            if(!PathIsValid(path1) || !PathIsValid(path2)) return false;

            var dirInfo1 = new DirectoryInfo(path1);
            var dirInfo2 = new DirectoryInfo(path2);

            if (dirInfo1.Exists != dirInfo2.Exists) return false;

            if ((!dirInfo1.Exists && !new FileInfo(path1).Exists) || (!dirInfo2.Exists && !new FileInfo(path2).Exists)) return false;

            if (dirInfo1.CreationTime != dirInfo2.CreationTime) return false;

            var dir1Children = dirInfo1.EnumerateFileSystemInfos().ToDictionary(entry => entry.Name);
            var dir2Children = dirInfo2.EnumerateFileSystemInfos();

            if (dir1Children.Count != dir2Children.Count()) return false;

            foreach (var dir2Child in dir2Children)
            {
                if (!dir1Children.ContainsKey(dir2Child.Name)) return false;

                if (dir1Children[dir2Child.Name].CreationTime != dir2Child.CreationTime) return false;
            }

            return true;
        }

        public static bool PathIsSubpathOf(string subPath, string parentPath)
        {
            string matchingDirectoryPath;
            return PathIsSubpathOf(subPath, parentPath, out matchingDirectoryPath);
        }

        public static bool PathIsSubpathOf(string subPath, string parentPath, out string matchingDirectoryPath)
        {
            if (!PathIsValid(subPath) || !PathIsValid(parentPath))
            {
                matchingDirectoryPath = null;
                return false;
            }

            var subPathInfo = new DirectoryInfo(subPath);

            var subPathParts = subPathInfo.FullName.Split(Path.DirectorySeparatorChar);

            for (int i = subPathParts.Length - 1; i > -1; --i)
            {
                var path = String.Join(Path.DirectorySeparatorChar.ToString(), subPathParts.Take(i + 1));

                if (PathsAreSame(parentPath, path))
                {
                    matchingDirectoryPath = path;
                    return true;
                }
            }
            
            matchingDirectoryPath = null;
            return false;
        }

        public static string GetPathRelativeTo(string subPath, string parentPath)
        {
            string matchingDirectoryPath;

            if(!PathIsSubpathOf(subPath, parentPath, out matchingDirectoryPath)) return null;

            return subPath.Substring(matchingDirectoryPath.Length);
        }
    }
}