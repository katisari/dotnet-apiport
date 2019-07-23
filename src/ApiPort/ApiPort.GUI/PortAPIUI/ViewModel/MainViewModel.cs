// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.ObjectModel;
using PortAPI.Shared;
using PortAPIUI;
using PortAPIUI.ViewModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

internal class MainViewModel : ViewModelBase
{
    public RelayCommand Browse { get; set; }

    public RelayCommand Export { get; set; }

    public RelayCommand Analyze { get; set; }

    public IApiPortService Service { get; set; }

    private string _selectedPath;

    private List<string> _assemblies;

    private List<string> _assembliesPath;

    private ObservableCollection<string> _chooseAssemblies;

    public List<string> _config;

    public static List<string> _platform;

    public static string ExeFile;

    public static string _selectedConfig;

    public static string _selectedPlatform;

    public ObservableCollection<ApiViewModel> _assemblyCollection { get; set; }

    public string _selectedAssembly;

    public IList<MemberInfo> _members;

    private string _message;


    public Visibility _isMessageVisible = Visibility.Hidden;


    public Visibility _IsWarningVisible = Visibility.Hidden;
    public Visibility _IsErrorVisible = Visibility.Hidden;
    public Visibility _isCheckVisible = Visibility.Hidden;


    private bool _endDateEnabled = false;

    public bool EndDateEnabled
    {
        get { return _endDateEnabled; }
        set
        {
            if (value != _endDateEnabled)
            {
                _endDateEnabled = value;
                RaisePropertyChanged(nameof(EndDateEnabled));
            }
        }
    }

    public Visibility IsWarningVisible
    {

        get { return _IsWarningVisible; }

        set
        {
            _IsWarningVisible = value;
            RaisePropertyChanged(nameof(IsWarningVisible));
        }
    }

    public Visibility IsErrorVisible
    {

        get { return _IsErrorVisible; }

        set
        {
            _IsErrorVisible = value;
            RaisePropertyChanged(nameof(IsErrorVisible));
        }
    }
    public Visibility IsCheckVisible
    {

        get { return _isCheckVisible; }

        set
        {
            _isCheckVisible = value;
            RaisePropertyChanged(nameof(IsCheckVisible));
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

    public ObservableCollection<string> ChooseAssemblies
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



    public Visibility IsMessageVisible
    {
        get
        {
            return _isMessageVisible;
        }


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
        _chooseAssemblies = new ObservableCollection<string>();
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
        EndDateEnabled = false;
        Message = "Analyzing...";
        CollapseIcons();

        Task.Run(async () =>
        {
            Info info = Rebuild.ChosenBuild(SelectedPath);

            if (Rebuild.IsProjectBuilt == true)


            {
                IsErrorVisible = Visibility.Visible;
                Message = "Build your project first.";
            }

            else
            {
                AssembliesPath = info.Assembly;
                ExeFile = info.Location;
                ApiAnalyzer analyzer = new ApiAnalyzer();
                var result = await analyzer.AnalyzeAssemblies(ExeFile, Service);


                Application.Current.Dispatcher.Invoke(() =>
                {
                    Members = result;
                    if (Members.Count != 0)
                    { 
                    ChooseAssemblies.Add("All Assemblies");
                    EndDateEnabled = true;
                    Message = "Analyzing...Done!";
                    }
                    else
                    {
                        Message = "All APIs are compatibile!";
                        IsCheckVisible = Visibility.Visible;
                    }
                });
            }
        });
    }

    public void CollapseIcons()
    {
        IsWarningVisible = Visibility.Collapsed;
     IsErrorVisible = Visibility.Collapsed;
     IsCheckVisible = Visibility.Collapsed;
}

public void AssemblyCollectionUpdate(string assem)
{
  
        Message = string.Empty;
        AssemblyCollection.Clear();
        foreach (var r in Members)
        {  
            AssemblyCollection.Add(new ApiViewModel("All Assemblies", r.MemberDocId, false, r.RecommendedChanges));

        }
  }

private void ExecuteOpenFileDialog()
{
        AssemblyCollection.Clear();
        EndDateEnabled = false;
        var dialog = new Microsoft.Win32.OpenFileDialog();
        dialog.Filter = "Project File (*.csproj)|*.csproj|All files (*.*)|*.*";
        dialog.InitialDirectory = @"C:\";
        bool? result = dialog.ShowDialog();
        if (result == true)
        {
            ResetAnalyzer();
            SelectedPath = dialog.FileName;
        }
        else
        {
            SelectedPath = null;
          
        }

        MSAnalyzer();
    }

    private void MSAnalyzer()

    {
        CollapseIcons();
        MsBuildAnalyzer msBuild = new MsBuildAnalyzer();
        if (SelectedPath != null)
        {
            Info output = msBuild.GetAssemblies(SelectedPath);
            if (output != null)
            {
                if (MsBuildAnalyzer.MessageBox1 == true)

                {
                    IsWarningVisible = Visibility.Visible;
                    Message = "In order to port to .NET Core, NuGet References need to be in PackageReference format. For more information go to the Portability Analyzer documentation.";
                }

                Config = output.Configuration;
                if (Config.Count > 0)
                {
                    SelectedConfig = Config[0];
                }

                Platform = output.Platform;
                if (Platform.Count > 0)
                {
                    SelectedPlatform = Platform[0];
                }

                ExeFile = output.Location;
            }
        }
    }


    private void ResetAnalyzer()
    {
        ChooseAssemblies.Clear();
        SelectedPath = null;
        Platform.Clear();
        AssemblyCollection.Clear();
        Config.Clear();
        MSAnalyzer();
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

            ExportResult exportClass = new ExportResult();
            exportClass.ExportApiResult(_selectedPath, Service, savedialog.FileName);
        }
    }

}
