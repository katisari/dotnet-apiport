﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using BuildcsprojtoMem;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Locator;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PortAPI.Shared;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Versioning;
using System.Text;

namespace MSBuildAnalyzer
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            MSBuildLocator.RegisterDefaults();
            string csProjPath = args[0];
            string jsonPath = args[1];
            if (args.Length == 2)
            {
                Temp.BuildIt(csProjPath, jsonPath);
            }

            if (args.Length > 2)
            {
                string chosenConfig = args[2];
                string chosenPlat = args[3];
                string json1Path = args[4];
                Chosen.Configure(csProjPath, chosenConfig, chosenPlat, json1Path);
            }
        }
    }

    internal class Temp
    {
        private static List<string> configurations;

        private static List<string> platforms;

        public static List<string> Configurations { get => configurations; set => configurations = value; }

        public static List<string> Platforms { get => platforms; set => platforms = value; }

        public static StringBuilder Output;

        public static void BuildIt(string csProjPath, string jsonPath)
        {
            ProjectCollection pc = new ProjectCollection(null, null, ToolsetDefinitionLocations.Default);
            var project = pc.LoadProject(csProjPath);
            bool correct = false;
            var projectItems = project.Items;
            List<string> packInfo = new List<string>();
            List<FrameworkName> packInfo1 = new List<FrameworkName>();
            foreach (var count in projectItems)
            {
                if (count.ItemType.Equals("PackageReference"))
                {
                    correct = true;
                }
            }

            System.IO.File.WriteAllText(jsonPath, string.Empty);
            Configurations = project.ConditionedProperties["Configuration"];
            Platforms = project.ConditionedProperties["Platform"];
            JsonSerializer serializer = new JsonSerializer();
            StreamWriter sw = new StreamWriter(jsonPath, false);
            serializer.Converters.Add(new JavaScriptDateTimeConverter());
            serializer.NullValueHandling = NullValueHandling.Ignore;
            using (sw)
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                Info info = new Info(null, Configurations, Platforms, null, null, null, correct);
                serializer.Serialize(writer, info);
                sw.Close();
                writer.Close();
            }
        }
    }
}
