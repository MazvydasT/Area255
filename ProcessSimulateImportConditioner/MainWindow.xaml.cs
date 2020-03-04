using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml.Linq;

using FolderSelect;
using System.Windows.Forms;

namespace ProcessSimulateImportConditioner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = ApplicationData.Service;
            ApplicationData.Service.GUIDispatcher = Dispatcher;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "JT (*.jt)|*.jt";
            openFileDialog.Title = "Select JT files";

            var openFileResult = openFileDialog.ShowDialog();
            if (openFileResult == System.Windows.Forms.DialogResult.Cancel) return;

            AddInput(openFileDialog.FileNames);
        }

        private void AddInput(string[] pathsToJT)
        {
            var inputs = ApplicationData.Service.Inputs;
            var inputDictionary = inputs.ToDictionary(input => input.JTPath);

            foreach (var pathToJT in pathsToJT)
            {
                if (!inputDictionary.ContainsKey(pathToJT))
                {
                    var input = new Input(pathToJT);
                    input.Delete = () => inputs.Remove(input);

                    inputs.Add(input);
                }
            }
        }

        private void autoOutputDirectoryBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var folderBrowserDialog = new FolderSelectDialog();
            folderBrowserDialog.Title = "Choose output directory";

            var openFolderResult = folderBrowserDialog.ShowDialog();
            if (!openFolderResult) return;

            AutoOutputBaseDirectoryTextBox.Text = folderBrowserDialog.FileName;
        }

        private void buttonGo_Click(object sender, RoutedEventArgs e)
        {
            var service = ApplicationData.Service;

            service.Errors.Clear();

            string mergedOutputFilePath = null;

            if (service.MergeOutput)
            {
                var saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "eM-Planner data (*.xml)|*.xml";

                var openFileResult = saveFileDialog.ShowDialog();
                if (openFileResult == System.Windows.Forms.DialogResult.Cancel) return;

                mergedOutputFilePath = saveFileDialog.FileName;
            }

            service.MaxValue = service.Inputs.Count;
            service.ProgressValue = 0;

            int elementId = 0;

            Task.WhenAll(ApplicationData.Service.Inputs.Select(input => Utils.ConvertInput(input).Task.ContinueWith(task =>
            {
                var data = task.Result;
                XElement xmlDocument = null;

                if (data != null)
                {
                    xmlDocument = data.Item1;
                    //string name = System.IO.Path.GetFileNameWithoutExtension(input.JTPath);

                    string compoundElementName = null;
                    string prototypeElementName = null;
                    string instanceElementName = null;

                    if (input.PartClass && xmlDocument.Descendants("PmCompoundPart").Count() == 0)
                    {
                        compoundElementName = "PmCompoundPart";
                        prototypeElementName = "PmPartPrototype";
                        instanceElementName = "PmPartInstance";
                    }

                    else if (input.ResourceClass && xmlDocument.Descendants("PmCompoundResource").Count() == 0)
                    {
                        compoundElementName = "PmCompoundResource";
                        prototypeElementName = "PmToolPrototype";
                        instanceElementName = "PmToolInstance";
                    }

                    if (compoundElementName != null)
                    {
                        var instanceId = elementId++;

                        var compoundElements = new object[]
                    {
                        new XElement(instanceElementName, new object[]
                        {
                            new XAttribute("ExternalId", "#" + instanceId.ToString()),
                            new XElement("name", input.PartName),
                            new XElement("prototype", xmlDocument.Descendants(prototypeElementName).First().Attribute("ExternalId").Value)
                        }),
                        new XElement(compoundElementName, new object[]
                        {
                            new XAttribute("ExternalId", "#" + elementId++.ToString()),
                            new XElement("name", input.PartName),
                            new XElement("children", new XElement("item", "#" + instanceId.ToString()))
                        })
                    };

                        xmlDocument.Descendants("Objects").First().AddFirst(compoundElements);
                    }

                    if (mergedOutputFilePath == null)
                    {
                        Directory.CreateDirectory(input.OutputDirectory);
                        xmlDocument.Save(System.IO.Path.Combine(input.OutputDirectory, data.Item2));
                    }
                }

                service.ProgressValue++;

                return data == null ? null : new Tuple<XElement, string>(xmlDocument, input.PartName);
            })))
            .ContinueWith(task =>
            {
                if (mergedOutputFilePath != null)
                {
                    var dataItems = task.Result;

                    XElement mergedDocument = null;
                    XElement objectsElement = null;

                    foreach (var data in dataItems)
                    {
                        if (data == null) continue;

                        var xmlDocument = data.Item1;
                        var name = data.Item2;

                        if (mergedDocument == null)
                        {
                            mergedDocument = new XElement(xmlDocument);

                            objectsElement = mergedDocument.Descendants("Objects").First();

                            objectsElement.RemoveAll();
                        }

                        var elements = xmlDocument.Descendants("Objects").First().Elements();
                        
                        var partPrototypes = elements.Where(element => element.Name == "PmPartPrototype");
                        var resourcePrototypes = elements.Where(element => element.Name == "PmToolPrototype");

                        string libraryElementName = null;
                        IEnumerable<XElement> prototypes = null;

                        if (partPrototypes.Count() > 0)
                        {
                            libraryElementName = "PmPartLibrary";
                            prototypes = partPrototypes;
                        }

                        else if (resourcePrototypes.Count() > 0)
                        {
                            libraryElementName = "PmResourceLibrary";
                            prototypes = resourcePrototypes;
                        }

                        if (libraryElementName != null)
                        {
                            objectsElement.Add(Enumerable.Concat(new object[]
                            {
                                new XElement(libraryElementName, new object[]
                                {
                                    new XAttribute("ExternalId", "#" + elementId++.ToString()),
                                    new XElement("name", name),
                                    new XElement("children", prototypes.Select(element => new XElement("item", element.Attribute("ExternalId").Value)))
                                })
                            }, elements).ToArray());
                        }
                    }

                    Directory.CreateDirectory(System.IO.Path.GetDirectoryName(mergedOutputFilePath));
                    mergedDocument.Save(mergedOutputFilePath);
                }

                Dispatcher.Invoke(() => service.Inputs.Clear());
                service.BaseOutputDirectory = "";

                Task.Delay(service.ProgressAnimationDuration.TimeSpan.Add(new TimeSpan(0, 0, 1))).ContinueWith(_ =>
                {
                    service.ProgressValue = 0;
                    service.MaxValue = 0;
                });
            });
        }
    }
}
