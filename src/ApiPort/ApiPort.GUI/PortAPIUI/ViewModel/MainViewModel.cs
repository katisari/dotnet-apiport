// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.ObjectModel;
using PortAPI.Shared;
using PortAPIUI;
using PortAPIUI.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

internal class MainViewModel : ViewModelBase
{
    public RelayCommand Browse { get; set; }

    public RelayCommand Export { get; set; }

    public RelayCommand Analyze { get; set; }

    public IApiPortService Service { get; set; }

    private string _selectedPath;

    private List<string> _assemblies;

    private List<string> _assembliesPath;

    private HashSet<string> _chooseAssemblies;

    public  List<string> _config;

    public static List<string> _platform;

    public static string ExeFile;

    public static string _selectedConfig;

    public static string _selectedPlatform;

    public ObservableCollection<ApiViewModel> _assemblyCollection { get; set; }

    public string _selectedAssembly;

    public IList<MemberInfo> _members;

    private  string _message;

    public Visibility _isMessageVisible = Visibility.Hidden;

    private bool _isEnabled = false;

   
   // private string _image;

   /* public string Image
    {
        get
        {
            return _image;
        }
        set
        {
            _image = value;
            RaisePropertyChanged(nameof(Image));
        }
    }
    public Image IconImage
    {
        get
        {
            return _iconImage;
        }
        set
        {
            _iconImage = value;
            RaisePropertyChanged(nameof(IconImage));
        }
    }*/

    public bool IsEnabled
    {
        get
        {
            return _isEnabled;
        }

        set
        {
            _isEnabled = value;
            RaisePropertyChanged(nameof(IsEnabled));
        }
    }

    public string Message
    {
        get
        {
            return _message;
        }

        set
        {
            _message = value;
            if (string.IsNullOrEmpty(value))
            {
                IsMessageVisible = Visibility.Hidden;
            }
            else
            {
                IsMessageVisible = Visibility.Visible;
            }
            RaisePropertyChanged(nameof(Message));
        }
    }

    public IList<MemberInfo> Members
    {
        get
        {
            return _members;
        }

        set
        {
            _members = value;
            RaisePropertyChanged(nameof(Members));
        }
    }

    public ObservableCollection<ApiViewModel> AssemblyCollection
    {
        get
        {
            return _assemblyCollection;
        }

        set
        {
            _assemblyCollection = value;
            RaisePropertyChanged(nameof(AssemblyCollection));
        }
    }

    public string SelectedPath
    {
        get => _selectedPath;
        set
        {
            _selectedPath = value;
            RaisePropertyChanged(nameof(SelectedPath));
        }
    }

    public List<string> Config
    {
        get
        {
            return _config;
        }

        set
        {
            _config = value;
            RaisePropertyChanged(nameof(Config));
        }
    }

    public List<string> Platform
    {
        get { return _platform; }

        set
        {
            _platform = value;
            RaisePropertyChanged(nameof(Platform));
        }
    }

    public List<string> Assemblies
    {
        get
        {
            return _assemblies;
        }

        set
        {
            _assemblies = value;
            RaisePropertyChanged(nameof(Assemblies));
        }
    }

    public List<string> AssembliesPath
    {
        get { return _assembliesPath; }

        set
        {
            _assembliesPath = value;
            RaisePropertyChanged(nameof(AssembliesPath));
        }
    }

    public HashSet<string> ChooseAssemblies
    {
        get { return _chooseAssemblies; }

        set
        {
            _chooseAssemblies = value;
            RaisePropertyChanged(nameof(ChooseAssemblies));
        }
    }

    public string SelectedConfig
    {
        get => _selectedConfig;

        set
        {
            _selectedConfig = value;
            RaisePropertyChanged(nameof(SelectedConfig));
        }
    }

    public string SelectedPlatform
    {
        get
        {
            return _selectedPlatform;
        }

        set
        {
            _selectedPlatform = value;
            RaisePropertyChanged("SelectedPlatfrom");
        }
    }

    public string SelectedAssembly
    {
        get
        {
            return _selectedAssembly;
        }

        set
        {
            _selectedAssembly = value;
            RaisePropertyChanged(nameof(SelectedAssembly));
        }
    }


    public Visibility IsMessageVisible {

        get { return _isMessageVisible; }
        set
        {
            _isMessageVisible = value;
            RaisePropertyChanged(nameof(IsMessageVisible));
        }
    }

    
   


    public MainViewModel()
    {
        RegisterCommands();
        _assemblies = new List<string>();
        _config = new List<string>();
        _platform = new List<string>();
        _chooseAssemblies = new HashSet<string>();
        _assembliesPath = new List<string>();
        AssemblyCollection = new ObservableCollection<ApiViewModel>();

    }

    private void RegisterCommands()
    {
        Browse = new RelayCommand(ExecuteOpenFileDialog);
        Export = new RelayCommand(ExecuteSaveFileDialog);
        Analyze = new RelayCommand(AnalyzeAPI);
    }

    private void AnalyzeAPI()

    {

        Message = "Analyzing...";
        Rebuild.ChosenBuild(SelectedPath);
        if (Rebuild.MessageBox == true)
        {
           // Image = "warning_icon.png";
            Message = "Build your project first.";
        }
        else
        {
           
            Info info = Rebuild.ChosenBuild(SelectedPath);
            AssembliesPath = info.Assembly;
            ExeFile = info.Location; 
            ApiAnalyzer analyzer = new ApiAnalyzer();
            var analyzeAssembliesTask = Task.Run<IList<MemberInfo>>(async () => { return await analyzer.AnalyzeAssemblies(ExeFile, Service); });
            analyzeAssembliesTask.Wait();
            Members = analyzeAssembliesTask.Result;
            
            ChooseAssemblies.Add("All Assemblies");

            
        }
    }

    public void AssemblyCollectionUpdate(string assem)
    {
        Message = "";
        AssemblyCollection.Clear();
        foreach (var r in Members)
        {
              
             AssemblyCollection.Add(new ApiViewModel("All Assemblies", r.MemberDocId, false, r.RecommendedChanges));
                
            
        }
    }

    private void ExecuteOpenFileDialog()
    {

        ChooseAssemblies = new HashSet<string>();
        AssemblyCollection.Clear();
        SelectedPath = null;
       // SelectedPlatform = null;
        Platform = new List<string>();
        Config = new List<string>();
        IsEnabled = false;
        var dialog = new Microsoft.Win32.OpenFileDialog();
        dialog.Filter = "Project File (*.csproj)|*.csproj|All files (*.*)|*.*";
        dialog.InitialDirectory = @"C:\";
        bool? result = dialog.ShowDialog();
        if (result == true)
        {
            SelectedPath = dialog.FileName;
        }
        else
        {
            SelectedPath = null;
            IsEnabled = false;
        }

        MsBuildAnalyzer msBuild = new MsBuildAnalyzer();
        if (SelectedPath != null)
        {
            Info output = msBuild.GetAssemblies(SelectedPath);
            if (output != null)
            {
                if (MsBuildAnalyzer.MessageBox1 == true)
                {
                    Message = "In order to port to .NET Core, NuGet References need to be in PackageReference format. For more information go to the Portability Analyzer documentation.";
              
                        

                }

                Config = output.Configuration;
                Platform = output.Platform;
                ExeFile = output.Location;
            }
        }
        Message = "Analyzing...";
    }

    private void ExecuteSaveFileDialog()
    {
        var savedialog = new Microsoft.Win32.SaveFileDialog();
        savedialog.FileName = "PortablityAnalysisReport";
        savedialog.DefaultExt = ".text";
        savedialog.Filter = "HTML file (*.html)|*.html|Json (*.json)|*.json|Csv (*.csv)|*.csv";
        bool? result = savedialog.ShowDialog();
        if (result == true)
        {
            
            ExportResult exportClass= new ExportResult();
            exportClass.ExportApiResult(_selectedPath, Service, savedialog.FileName);
        }
    }
}
