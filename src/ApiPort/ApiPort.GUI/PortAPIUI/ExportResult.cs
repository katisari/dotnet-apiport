﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using CsvHelper;
using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.Fx.Portability.Reporting;
using Microsoft.Fx.Portability.Resources;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace PortAPIUI
{
    internal class ExportResult
    {
        private static string inputPath;
        private const string Json = "json";
        private readonly IProgressReporter _progressReport;
        private readonly IFileWriter _writer;

        public static string GetInputPath()
        {
            return inputPath;
        }

        public static void SetInputPath(string value)
        {
            inputPath = value;
        }

        public ExportResult()
        {
            _progressReport = App.Resolve<IProgressReporter>();
            _writer = App.Resolve<IFileWriter>();
        }

        // returns location of the portabitlity analyzer result
        public async void ExportApiResult(string selectedPathToExport, IApiPortService service, string exportPath)
        {
            string fileExtension = Path.GetExtension(exportPath);
            bool isCSV = false;

            if (fileExtension == ".csv")
            {
                isCSV = true;
                fileExtension = ".json";
            }

            string simpleFileExtension = GetFileFormat(fileExtension);

            ApiAnalyzer apiAnalyzerClass = new ApiAnalyzer();
            AnalyzeRequest request = apiAnalyzerClass.GenerateRequestFromDepedencyInfo(selectedPathToExport, service);
            bool jsonAdded = false;
            AnalyzeResponse response = null;
            List<string> exportFormat = new List<string>();

            exportFormat.Add(simpleFileExtension);

            var results = await service.SendAnalysisAsync(request, exportFormat);
            var myResult = results.Response;
            string outputPath = string.Empty;

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

                if (isCSV)
                {

                    using (var writer = new StreamWriter(exportPath))
                    using (var csv = new CsvWriter(writer))
                    {
                        csv.WriteRecords(response.MissingDependencies);
                    }
                }
                else
                {
                    outputPath = await CreateReport(result.Data, exportPath, fileExtension, true);
                }
            }

            return;
        }

        private string GetFileFormat(string format)
        {
            switch (format)
            {
                case ".json":
                    return format.Substring(1);

                case ".html":
                    return format.Substring(1);

                case ".xlsx":
                    return "excel";

                default:
                    return "json";
            }
        }

        private static string GenerateReportPath(string fileExtension)
        {
            var outputDirectory = System.IO.Path.GetTempPath();
            var outputName = "PortabilityReport";
            var outputExtension = fileExtension;
            var counter = 1;
            var outputPath = System.IO.Path.Combine(outputDirectory, outputName + outputExtension);

            while (File.Exists(outputPath))
            {
                outputPath = System.IO.Path.Combine(outputDirectory, $"{outputName}({counter}){outputExtension}");

                counter++;
            }

            return outputPath;
        }

        /// <summary>
        /// Writes a report given the output format and filename.
        /// </summary>
        /// <returns>null if unable to write the report otherwise, will return the full path to the report.</returns>
        private async Task<string> CreateReport(byte[] result, string suppliedOutputFileName, string outputFormat, bool overwriteFile)
        {
            string filePath = null;

            // string has format has writing html report
            using (var progressTask = _progressReport.StartTask(string.Format(CultureInfo.CurrentCulture, LocalizedStrings.WritingReport, outputFormat)))
            {
                try
                {
                    filePath = Path.GetFullPath(suppliedOutputFileName);
                }
                catch (Exception ex)
                {
                    _progressReport.ReportIssue(string.Format(CultureInfo.InvariantCulture, ex.Message));
                    progressTask.Abort();

                    return null;
                }

                var outputDirectory = Path.GetDirectoryName(filePath);
                var outputFileName = Path.GetFileNameWithoutExtension(filePath);
                try
                {
                    var filename = await _writer.WriteReportAsync(result, outputFormat, outputDirectory, outputFileName, overwriteFile);

                    if (string.IsNullOrEmpty(filename))
                    {
                        _progressReport.ReportIssue(string.Format(CultureInfo.CurrentCulture, LocalizedStrings.CouldNotWriteReport, outputDirectory, outputFileName, outputFormat));
                        progressTask.Abort();

                        return null;
                    }
                    else
                    {
                        return filename;
                    }
                }
                catch (Exception)
                {
                    progressTask.Abort();
                    throw;
                }
            }
        }
    }
}