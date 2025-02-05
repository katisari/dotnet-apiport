﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Newtonsoft.Json;
using PortAPI.Shared;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace PortAPIUI
{
    public static class AnalyzeSelected
    {
        private static StringBuilder outputConsole = null;

        public static Info ChosenBuild(string path)
        {
            var ourPath = System.Reflection.Assembly.GetEntryAssembly().Location;
            var ourDirectory = System.IO.Path.GetDirectoryName(ourPath);
            var analyzerPath = System.IO.Path.Combine(ourDirectory, "MSBuildAnalyzer\\BuildProj.exe");
            var json1Path = System.IO.Path.Combine(ourDirectory, "MSBuildAnalyzer\\json1.txt");
            Process process = new Process();
            process.StartInfo.FileName = analyzerPath;
            process.StartInfo.Arguments = $"{path} {"blank"} {MainViewModel.GetSelectedConfig()} {MainViewModel.GetSelectedPlatform()} {json1Path}";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.Close();
            Info output;
            using (StreamReader r = new StreamReader(json1Path))
            {
                string json = r.ReadToEnd();
                output = JsonConvert.DeserializeObject<Info>(json);
            }

            return output;
        }

        private static void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (!string.IsNullOrEmpty(outLine.Data))
            {
                outputConsole.Append(outLine.Data);
            }
        }
    }
}
