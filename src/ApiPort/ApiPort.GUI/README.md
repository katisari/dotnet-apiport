## .NET Portability Analyzer GUI


The .NET Portability Analyzer GUI is an open source tool that simplifies your porting experience to .NET Core by identifying your project's compatibility with .NET Core. In addition, the GUI tool also provides a list of recommended changes of what to do with the incompatible APIs for each assembly, as well as warning the user about the format of their NuGet Packages ([see Troubleshooting](#Troubleshooting)). The GUI tool builds upon the .NET Portabiltiy Analyzer standalone tool, but it focuses on just .NET Core 3.0 as a porting framework, and it provides additional add-ons to improve user experience. 



Running the tool can do two things: 


1.	Generate a spreadsheet within the UI that will report the level of compatibility that your project has with .NET Core 3.0, including the specific APIs that are currently unsupported.
2.	If the **Export** button is chosen, it will generate an export result in html or Excel formats that can then be saved onto your computer. 



### Using the Portability Analyzer

Use the following instructions to run the Portability Analyzer.
1.	Go to the GitHub repo for the portability analyzer.
2.	Download the .zip file and unzip it, then run the .exe file.
3.	In the *Path to application folder* text box enter the directory path to your .csproj file (either by inserting a path string or clicking Browse button and navigating to the folder).
4.	Choose desired *Build configuration* and *Build platform* for MSBuild of desired project.
5.	Click the **Analyze** button.
6.	After the analysis is complete, a report of how portable your app is right now to .NET Core 3.0 will be presented in the UI.
7.	By pressing the **Export** button save the analysis in your desired format to your computer.


### Troubleshooting

If your project is not built, an error warning will pop up to remind you to build your project so that the .exe file can be found by the analyzer. In addition, if your project is not using PackageReference for your NuGet packages, a warning will appear. Having your NuGet packages in ProjectReference format is not neccesary in order to use the analyzer, however if you wish to convert your project to .NET Core you will need to migrate your NuGet packages to PackageReference ([How to migrate packages for porting](https://docs.microsoft.com/en-us/nuget/reference/migrate-packages-config-to-package-reference))



### Summary

Please download the .zip file and use the Portability Analyzer .exe on your desktop applications. It will help you determine how compatible your apps are with .NET Core 3.0.
