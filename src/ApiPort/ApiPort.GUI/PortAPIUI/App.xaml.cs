// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPort;
using Autofac;
using Microsoft.Fx.Portability;
using System.Windows;

namespace PortAPIUI
{
    /// <summary>
    /// Interaction logic for App.xaml.
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Container = SetUp();
        }

        public static IContainer SetUp()
        {
            var productInformation = new ProductInformation("ApiPort_Console");
            var container = DependencyBuilder.Build(productInformation);
            return container;
        }

        private static IContainer Container { get; set; }

        public static T Resolve<T>()
        {
            return Container.Resolve<T>();
        }
    }
}
