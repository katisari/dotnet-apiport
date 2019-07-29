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
    private string _selectedPath;

    private List<string> _assemblies;

    private List<string> _assembliesPath;

    private ObservableCollection<string> _chooseAssemblies;

    private string _message;

    private bool _isAnalyzeEnabled;

    private Visibility _isMessageVisible = Visibility.Hidden;

    private Visibility _isWarningVisible = Visibility.Collapsed;

    private Visibility _isErrorVisible = Visibility.Collapsed;

    private Visibility _isCheckVisible = Visibility.Collapsed;

    private bool _isEnabled = false;

    public ObservableCollection<ApiViewModel> AssemblyCollection1 { get; set; }

    private string format = "Please build your project first. To build your project, open a Developer Command Prompt and input:"
                    + "\n" + "msbuild {0}  /p:Configuration=\"{1}\" /p:Platform=\"{2}\"";

    public List<string> config;

    public RelayCommand Browse { get; set; }

    public RelayCommand Export { get; set; }

    public RelayCommand Analyze { get; set; }

    public IApiPortService Service { get; set; }

    private List<string> _platform;

    public string ExeFile;

    public static string selectedConfig;

    public static string selectedPlatform;

    public string _selectedAssembly;

    private IList<MemberInfo> _members;

    public bool IsAnalyzeEnabled
    {
        get
        {
            return _isAnalyzeEnabled;
        }

        set
        {
            _isAnalyzeEnabled = value;
            RaisePropertyChanged(nameof(IsAnalyzeEnabled));
        }
    }

    public bool IsEnabled
    {
        get => _isEnabled;

        set
        {
            if (value != _isEnabled)
            {
                _isEnabled = value;
                RaisePropertyChanged(nameof(IsEnabled));
            }
        }
    }

    public Visibility IsWarningVisible
    {
        get => _isWarningVisible;

        set
        {
            _isWarningVisible = value;
            RaisePropertyChanged(nameof(IsWarningVisible));
        }
    }

    public Visibility IsErrorVisible
    {
        get => _isErrorVisible;

        set
        {
            _isErrorVisible = value;
            RaisePropertyChanged(nameof(IsErrorVisible));
        }
    }

    public Visibility IsCheckVisible
    {
        get => _isCheckVisible;

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
            return AssemblyCollection1;
        }

        set
        {
            AssemblyCollection1 = value;
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
            return config;
        }

        set
        {
            config = value;
            RaisePropertyChanged(nameof(Config));
        }
    }

    public List<string> Platform
    {
        get
        {
            return _platform;
        }

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
        get => _assembliesPath;

        set
        {
            _assembliesPath = value;
            RaisePropertyChanged(nameof(AssembliesPath));
        }
    }

    public ObservableCollection<string> ChooseAssemblies
    {
        get
        {
            return _chooseAssemblies;
        }

        set
        {
            _chooseAssemblies = value;
            RaisePropertyChanged(nameof(ChooseAssemblies));
        }
    }

    public static string GetSelectedConfig()
    {
        return selectedConfig;
    }

    public void SetSelectedConfig(string value)
    {
        selectedConfig = value;
        RaisePropertyChanged("SelectedConfig");
    }

    public static string GetSelectedPlatform()
    {
        return selectedPlatform;
    }

    public void SetSelectedPlatform(string value)
    {
        selectedPlatform = value;
        RaisePropertyChanged("SelectedPlatfrom");
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
        config = new List<string>();
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
        IsAnalyzeEnabled = false;
        Message = "Analyzing...";
        CollapseIcons();

        _ = Task.Run(async () =>
          {
              Info info = AnalyzeSelected.ChosenBuild(SelectedPath);

              if (info.Build == false)
              {
                  IsErrorVisible = Visibility.Visible;
                  Message = string.Format(format, SelectedPath, GetSelectedConfig(), GetSelectedPlatform());
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
                          IsAnalyzeEnabled = true;
                          ChooseAssemblies.Add("All Assemblies");
                          IsEnabled = true;
                          Message = "Analyzing...Done!";
                      }
                      else
                      {
                          IsCheckVisible = Visibility.Visible;
                          Message = "All APIs are compatibile!";
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
        var dialog = new Microsoft.Win32.OpenFileDialog();
        dialog.Filter = "Project File (*.csproj)|*.csproj|All files (*.*)|*.*";
        dialog.InitialDirectory = @"C:\";
        bool? result = dialog.ShowDialog();
        if (result == true)
        {
            IsEnabled = false;
            ResetAnalyzer();
            SelectedPath = dialog.FileName;
            MSAnalyzer();
        }

        IsAnalyzeEnabled = string.IsNullOrWhiteSpace(SelectedPath) ? false : true;
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
                Config = output.Configuration;
                Platform = output.Platform;

                if (output.Package == false)
                {
                    IsWarningVisible = Visibility.Visible;
                    Message = "In order to port to .NET Core, NuGet References need to be in PackageReference format. For more information go to the Portability Analyzer documentation.";
                }

                if (Config.Count > 0)
                {
                    SetSelectedConfig(Config[0]);
                }

                if (Platform.Count > 0)
                {
                    SetSelectedPlatform(Platform[0]);
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
        savedialog.FileName = "PortabilityAnalyzerReport";
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
