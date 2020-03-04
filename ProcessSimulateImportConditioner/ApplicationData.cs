using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace ProcessSimulateImportConditioner
{
    public class ApplicationData : INotifyPropertyChanged
    {
        private static ApplicationData service = null;
        public static ApplicationData Service
        {
            get
            {
                if (service == null)
                    service = new ApplicationData();

                return service;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static string ConverterExeName { get { return "TxJT2co.exe"; } }

        public static string PathUnderSysRootMessage { get { return "Path must be under Sys Root"; } }

        private string sysRootPath = null;
        public string SysRootPath
        {
            get
            {
                if (sysRootPath == null)
                {
                    var tempDir = Utils.NewTempDirectory;
                    Directory.CreateDirectory(tempDir);

                    var dummyFilePath = Path.Combine(tempDir, "dummy.jt");

                    using(var dummyFileStream = File.Create(dummyFilePath))
                    {
                        dummyFileStream.Write(Properties.Resources.dummy, 0, Properties.Resources.dummy.Length);
                    }

                    try
                    {
                        var process = new Process()
                        {
                            StartInfo = new ProcessStartInfo()
                            {
                                FileName = ConverterExeName,
                                Arguments = String.Format("\"{0}\" -TxJT2cojt -dest \"{1}\"", dummyFilePath, tempDir),
                                UseShellExecute = false,
                                RedirectStandardOutput = true,
                                RedirectStandardError = true,
                                CreateNoWindow = true
                            }
                        };

                        process.Start();

                        var sysRootRegexp = new Regex(@"FATAL.*is\snot\sunder\sSystem\sRoot\s(.*)\.\sXML\sis\sincorrect");

                        while (!process.StandardOutput.EndOfStream)
                        {
                            var match = sysRootRegexp.Match(process.StandardError.ReadLine());

                            if (match.Success)
                            {
                                sysRootPath = match.Groups[1].Value;

                                process.Kill();

                                break;
                            }
                        }

                        process.WaitForExit();
                    }

                    catch (Exception) { }

                    finally
                    {
                        if (sysRootPath == null)
                            sysRootPath = String.Empty;

                        NotifyPropertyChanged();

                        Directory.Delete(tempDir, true);
                    }
                }

                return sysRootPath;
            }
        }

        private ObservableCollection<Input> inputs = new ObservableCollection<Input>();
        public ObservableCollection<Input> Inputs { get { return inputs; } }

        private ObservableCollection<TranslationError> errors = new ObservableCollection<TranslationError>();
        public ObservableCollection<TranslationError> Errors { get { return errors; } }

        private int autoOutputCount = 0;
        public int AutoOutputCount
        {
            get { return autoOutputCount; }

            private set
            {
                if (autoOutputCount != value)
                {
                    autoOutputCount = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private int inputsCount = 0;
        public int InputsCount
        {
            get { return inputsCount; }

            set
            {
                if (inputsCount != value)
                {
                    inputsCount = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private int errorsCount = 0;
        public int ErrorsCount
        {
            get { return errorsCount; }

            set
            {
                if (errorsCount != value)
                {
                    errorsCount = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool InputsContainMoreThanOneOutputPath(string path, Input currentInput)
        {
            using (var enumerator = Inputs.Where(input => input != currentInput && input.OutputDirectory == path).GetEnumerator())
            {
                return enumerator.MoveNext();
            }
        }

        private int invalidOutputDirectoryCount = 0;
        public int InvalidOutputDirectoryCount
        {
            get { return invalidOutputDirectoryCount; }

            set
            {
                if (invalidOutputDirectoryCount != value)
                {
                    invalidOutputDirectoryCount = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private bool mergeOutput = true;
        public bool MergeOutput
        {
            get { return mergeOutput; }

            set
            {
                if (mergeOutput != value)
                {
                    mergeOutput = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private bool usePartName = false;
        public bool UsePartName
        {
            get { return usePartName; }

            set
            {
                if (usePartName != value)
                {
                    usePartName = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private string baseOutputDirectory = "";
        public string BaseOutputDirectory
        {
            get { return baseOutputDirectory; }

            set
            {
                if (baseOutputDirectory != value)
                {
                    baseOutputDirectory = value;
                    NotifyPropertyChanged();

                    baseOutputDirectoryIsValid = null;
                    NotifyPropertyChanged("BaseOutputDirectoryIsValid");
                }
            }
        }

        private Nullable<bool> baseOutputDirectoryIsValid = null;
        public bool BaseOutputDirectoryIsValid
        {
            get
            {
                if (baseOutputDirectoryIsValid == null)
                {
                    baseOutputDirectoryIsValid = Utils.PathIsSubpathOf(BaseOutputDirectory, SysRootPath);
                }

                return baseOutputDirectoryIsValid.Value;
            }
        }

        private double maxValue = 0;
        public double MaxValue
        {
            get { return maxValue; }
            set
            {
                if (maxValue != value)
                {
                    maxValue = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private double progressValue = 0;
        public double ProgressValue
        {
            get { return progressValue; }
            set
            {
                if (progressValue != value)
                {
                    progressValue = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private Duration progressAnimationDuration = new Duration(new TimeSpan(0, 0, 0, 1, 500));
        public Duration ProgressAnimationDuration { get { return progressAnimationDuration; } }

        public Dispatcher GUIDispatcher { get; set; }

        private ApplicationData()
        {
            Inputs.CollectionChanged += Inputs_CollectionChanged;
            Errors.CollectionChanged += Errors_CollectionChanged;
        }

        void Errors_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ErrorsCount = Errors.Count;
        }

        void Inputs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var input in e.NewItems.Cast<Input>())
                {
                    input.PropertyChanged += input_PropertyChanged;

                    AutoOutputCount++;
                }
            }

            else
            {
                AutoOutputCount = Inputs.Where(input => input.AutoOutputDirectory).Count();
                InvalidOutputDirectoryCount = Inputs.Where(input => !input.OutputDirectoryIsValid).Count();
            }

            InputsCount = Inputs.Count;

            for (int i = 0, c = Inputs.Count; i < c; ++i)
                Inputs[i].Index = i;
        }

        void input_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "AutoOutputDirectory")
                AutoOutputCount += ((Input)sender).AutoOutputDirectory ? 1 : -1;

            if (e.PropertyName == "OutputDirectoryIsValid")
                InvalidOutputDirectoryCount = Inputs.Where(input => !input.OutputDirectoryIsValid).Count();
        }
    }
}