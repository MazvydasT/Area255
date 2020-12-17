using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace ProcessSimulateImportConditioner
{
    public class Input : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // This method is called by the Set accessor of each property.  
        // The CallerMemberName attribute that is applied to the optional propertyName  
        // parameter causes the property name of the caller to be substituted as an argument.  
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private int index = -1;
        public int Index
        {
            get
            {
                return index;
            }

            set
            {
                if (index != value)
                {
                    index = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private string jtPath = String.Empty;
        public string JTPath
        {
            get { return jtPath; }

            set
            {
                if (jtPath != value)
                {
                    jtPath = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private string partName = null;
        public string PartName
        {
            get
            {
                if (partName == null)
                    partName = Path.GetFileNameWithoutExtension(JTPath).Trim();

                return partName;
            }
        }

        private bool autoOutputDirectory = true;
        public bool AutoOutputDirectory
        {
            get { return autoOutputDirectory; }

            set
            {
                if (autoOutputDirectory != value)
                {
                    autoOutputDirectory = value;
                    NotifyPropertyChanged();

                    outputDirectoryIsValid = null;
                    NotifyPropertyChanged("OutputDirectoryIsValid");

                    outputDirectoryIsEmpty = null;
                    NotifyPropertyChanged("OutputDirectoryIsEmpty");
                }
            }
        }

        private string outputDirectory = "";
        public string OutputDirectory
        {
            get { return outputDirectory; }

            set
            {
                if (outputDirectory != value)
                {
                    outputDirectory = value;
                    NotifyPropertyChanged();

                    outputDirectoryIsValid = null;
                    NotifyPropertyChanged("OutputDirectoryIsValid");

                    outputDirectoryIsEmpty = null;
                    NotifyPropertyChanged("OutputDirectoryIsEmpty");
                }
            }
        }

        private bool partClass = true;
        public bool PartClass
        {
            get { return partClass; }
            set
            {
                if (partClass != value)
                {
                    partClass = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("ResourceClass");
                }
            }
        }
        public bool ResourceClass { get { return !partClass; } }

        private Nullable<bool> outputDirectoryIsValid = null;
        public bool OutputDirectoryIsValid
        {
            get
            {
                if (AutoOutputDirectory) return true;

                if (outputDirectoryIsValid == null)
                    outputDirectoryIsValid = Utils.PathIsSubpathOf(OutputDirectory, ApplicationData.Service.SysRootPath);

                return outputDirectoryIsValid.Value;
            }
        }

        private Nullable<bool> outputDirectoryIsEmpty = null;
        public bool OutputDirectoryIsEmpty
        {
            get
            {
                if (AutoOutputDirectory) return true;

                if (outputDirectoryIsEmpty == null)
                    outputDirectoryIsEmpty = Utils.DirectoryIsEmpty(OutputDirectory);

                return outputDirectoryIsEmpty.Value;
            }
        }

        public Action Delete { get; set; }

        public Input(string jtPath)
        {
            JTPath = jtPath;
        }
    }
}
