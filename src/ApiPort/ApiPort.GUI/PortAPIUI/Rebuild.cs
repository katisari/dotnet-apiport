﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Newtonsoft.Json;
using PortAPI.Shared;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace PortAPIUI
{
    public static class Rebuild
    {
        private static StringBuilder outputConsole = null;

        public static bool MessageBox { get; set; }

        public static Info ChosenBuild(string path)
        {
            var ourPath = System.Reflection.Assembly.GetEntryAssembly().Location;
            var ourDirectory = System.IO.Path.GetDirectoryName(ourPath);
            var analyzerPath = System.IO.Path.Combine(ourDirectory, "MSBuildAnalyzer\\BuildProj.exe");
            var json1Path = System.IO.Path.Combine(ourDirectory, "MSBuildAnalyzer\\json1.txt");
            Process process = new Process();
            process.StartInfo.FileName = analyzerPath;
            process.StartInfo.Arguments = $"{path} {"blank"} {MainViewModel._selectedConfig} {MainViewModel._selectedPlatform} {json1Path}";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.CreateNoWindow = true;
            outputConsole = new StringBuilder();
            process.OutputDataReceived += OutputHandler;
            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit();
            process.Close();
            Info assemblies;
            using (StreamReader r = new StreamReader(json1Path))
            {
                string json = r.ReadToEnd();
                assemblies = JsonConvert.DeserializeObject<Info>(json);              
                r.Close();
            }

           Message(assemblies);
            return assemblies;
        }

        private static void OutputHandler(object sendingProcess, DataReceivedEventArgs line)
        {
            if (!string.IsNullOrEmpty(line.Data))
            {
                outputConsole.Append(line.Data);
            }
        }

        public static void Message(Info answer)
        {
            if (answer.Build.Equals("False"))
            {
                MessageBox = true;
            }
            else
            {
                MessageBox = false;
            }
        }
    }
}
