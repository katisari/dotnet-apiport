﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.Analyzer;
using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.Fx.Portability.Reporting;
using Microsoft.Fx.Portability.Resources;
using NuGet.Runtime;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace PortAPIUI
{
    internal class ApiAnalyzer
    {
        private readonly IApiPortService _apiPortService;
        private readonly IProgressReporter _progressReport;
        private readonly ITargetMapper _targetMapper;
        private readonly IDependencyFinder _dependencyFinder;
        private readonly IReportGenerator _reportGenerator;
        private readonly IEnumerable<IgnoreAssemblyInfo> _assembliesToIgnore;
        private readonly IFileWriter _writer;
        private const string Json = "json";

        private ImmutableDictionary<IAssemblyFile, bool> InputAssemblies { get; }

        private struct MultipleFormatAnalysis
        {
            public AnalyzeRequest Request;
            public IDependencyInfo Info;
            public IEnumerable<ReportingResultWithFormat> Results;
        }

        public ApiAnalyzer()
        {
            _apiPortService = App.Resolve<IApiPortService>();
            _targetMapper = App.Resolve<ITargetMapper>();
            _dependencyFinder = App.Resolve<IDependencyFinder>();
            _progressReport = App.Resolve<IProgressReporter>();
        }

        public ApiAnalyzer(IApiPortService apiPortService, IProgressReporter progressReport, ITargetMapper targetMapper, IDependencyFinder dependencyFinder, IReportGenerator reportGenerator, IEnumerable<IgnoreAssemblyInfo> assembliesToIgnore, IFileWriter writer)
        {
            _apiPortService = apiPortService;
            _progressReport = progressReport;
            _targetMapper = targetMapper;
            _dependencyFinder = dependencyFinder;
            _reportGenerator = reportGenerator;
            _assembliesToIgnore = assembliesToIgnore;
            _writer = writer;
        }

        public async Task<IList<MemberInfo>> AnalyzeAssemblies(string selectedPath, IApiPortService service)
        {
            AnalyzeRequest request = GenerateRequestFromDepedencyInfo(selectedPath, service);
            bool jsonAdded = false;
            AnalyzeResponse response = null;
            List<string> exportFormat = new List<string>();
            exportFormat.Add("json");
            var results = await service.SendAnalysisAsync(request, exportFormat);
            var myResult = results.Response;

            foreach (var result in myResult)
            {
                if (string.Equals(Json, result.Format, StringComparison.OrdinalIgnoreCase))
                {
                    response = result.Data?.Deserialize<AnalyzeResponse>();
                    if (jsonAdded)
                    {
                        continue;
                    }
                }
            }

            return response?.MissingDependencies ?? new List<MemberInfo>();
        }

        public AnalyzeRequest GenerateRequestFromDepedencyInfo(string selectedPath, IApiPortService service)
        {
            var parentDirectory = System.IO.Directory.GetParent(selectedPath).FullName;

            // var parentDirectory = @"C:\Users\t-jaele\Downloads\Paint\Paint";
#pragma warning disable CS0436 // Type conflicts with imported type
            var name = new FilePathAssemblyFile(selectedPath);
#pragma warning restore CS0436 // Type conflicts with imported type
            List<string> browserfile = new List<string>();
            browserfile.Add(parentDirectory);

            // inputfiles has all the assembly location
            var (inputFiles, invalidFiles) = ProcessInputAssemblies(browserfile);
            var assemblies = inputFiles?.Keys ?? Array.Empty<IAssemblyFile>();
            var dependencyInfo = _dependencyFinder.FindDependencies(assemblies, _progressReport);

            // run it and get error b/c dependencyInfo is null - we think b/c progressReport is null
            return GenerateRequest(dependencyInfo);
        }

        private AnalyzeRequest GenerateRequest(IDependencyInfo dependencyInfo)
        {
            return new AnalyzeRequest
            {
                Targets = new List<string> { ".NET Core, Version=3.0" },
                Dependencies = dependencyInfo.Dependencies,

                // We pass along assemblies to ignore instead of filtering them from Dependencies at this point
                // because breaking change analysis and portability analysis will likely want to filter dependencies
                // in different ways for ignored assemblies.
                // For breaking changes, we should show breaking changes for
                // an assembly if it is un-ignored on any of the user-specified targets and we should hide breaking changes
                // for an assembly if it ignored on all user-specified targets.
                // For portability analysis, on the other hand, we will want to show portability for precisely those targets
                // that a user specifies that are not on the ignore list. In this case, some of the assembly's dependency
                // information will be needed.
                AssembliesToIgnore = _assembliesToIgnore,
                UnresolvedAssemblies = dependencyInfo.UnresolvedAssemblies.Keys.ToList(),
                UnresolvedAssembliesDictionary = dependencyInfo.UnresolvedAssemblies,
                UserAssemblies = dependencyInfo.UserAssemblies.ToList(),
                AssembliesWithErrors = dependencyInfo.AssembliesWithErrors.ToList(),
                ApplicationName = string.Empty,
                Version = AnalyzeRequest.CurrentVersion,
                RequestFlags = AnalyzeRequestFlags.ShowNonPortableApis,
                BreakingChangesToSuppress = new List<string>(),

                // change later
                ReferencedNuGetPackages = new List<string>()
            };
        }

        private static (ImmutableDictionary<IAssemblyFile, bool>, IReadOnlyCollection<string>) ProcessInputAssemblies(IEnumerable<string> files)
        {
            var s_ValidExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".dll",
                ".exe",
                ".winmd",
                ".ilexe",
                ".ildll"
            };

            var inputAssemblies = new SortedDictionary<IAssemblyFile, bool>(AssemblyFileComparer.Instance);
            var invalidInputFiles = new List<string>();

            void ProcessInputAssemblies(string path, bool isExplicitlySpecified)
            {
                bool HasValidPEExtension(string assemblyLocation)
                {
                    return s_ValidExtensions.Contains(Path.GetExtension(assemblyLocation));
                }

                if (Directory.Exists(path))
                {
                    foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
                    {
                        // If the user passes in a whole directory, any assembly we find in there
                        // was not explicitly passed in.
                        ProcessInputAssemblies(file, isExplicitlySpecified: false);
                    }
                }
                else if (File.Exists(path))
                {
                    // Only add files with valid PE extensions to the list of
                    // assemblies to analyze since others are not valid assemblies
                    if (HasValidPEExtension(path))
                    {
#pragma warning disable CS0436 // Type conflicts with imported type
                        var filePath = new FilePathAssemblyFile(path);
#pragma warning restore CS0436 // Type conflicts with imported type
                        if (inputAssemblies.TryGetValue(filePath, out var isAssemblySpecified))
                        {
                            // If the assembly already exists, and it was not
                            // specified explicitly, in the the case where one
                            // value does not match the other, we default to
                            // saying that the assembly is specified.
                            inputAssemblies[filePath] = isExplicitlySpecified || isAssemblySpecified;
                        }
                        else
                        {
                            inputAssemblies.Add(filePath, isExplicitlySpecified);
                        }
                    }
                }
                else
                {
                    invalidInputFiles.Add(path);
                }
            }

            foreach (var file in files)
            {
                ProcessInputAssemblies(file, isExplicitlySpecified: true);
            }

            return (inputAssemblies.ToImmutableDictionary(), invalidInputFiles);
        }
    }
}
