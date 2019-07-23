﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace PortAPI.Shared
{
    public class Info
    {
        public bool Build { get; set; }

        public List<string> Configuration { get; set; }

        public List<string> Platform { get; set; }

        public string TargetPath { get; set; }

        public List<string> Assembly { get; set; }

        public string Location { get; set; }

        public bool Package { get; set; }

        public Info(bool build, List<string> configuration, List<string> platform, string targetPath, List<string> assembly, string location, bool package)
        {
            Build = build;
            Configuration = configuration;
            Platform = platform;
            TargetPath = targetPath;
            Assembly = assembly;
            Location = location;
            Package = package;
        }
    }
}
