﻿using System;
using System.IO;
using System.Linq;
using Nuke.Common;
using Nuke.Common.BuildServers;
using Nuke.Common.Git;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.MSBuild;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.MSBuild.MSBuildTasks;

class Build : NukeBuild
{
    // Console application entry point. Also defines the default target.
    public static int Main () => Execute<Build>(x => x.Compile);

    // Auto-injection fields:

    // [GitVersion] readonly GitVersion GitVersion;
    // Semantic versioning. Must have 'GitVersion.CommandLine' referenced.

    // [GitRepository] readonly GitRepository GitRepository;
    // Parses origin, branch name and head from git config.

    // [Parameter] readonly string MyGetApiKey;
    // Returns command-line arguments and environment variables.
    
    // [Solution] readonly Solution Solution;
    // Provides access to the structure of the solution.

    Target Clean => _ => _
            .OnlyWhen(() => false) // Disabled for safety.
            .Executes(() =>
            {
                DeleteDirectories(GlobDirectories(SourceDirectory, "**/bin", "**/obj"));
                EnsureCleanDirectory(OutputDirectory);
            });

    Target Restore => _ => _
            .DependsOn(Clean)
            .Executes(() =>
            {
                MSBuild(s => DefaultMSBuildRestore);
            });

    Target Compile => _ => _
            .DependsOn(Restore)
            .Executes(() =>
            {
                MSBuild(s => DefaultMSBuildCompile
                    .SetAssemblyVersion(AppVeyor.Instance.BuildVersion)
                    .SetFileVersion(AppVeyor.Instance.BuildVersion)
                    .SetInformationalVersion(AppVeyor.Instance.BuildVersion));

                // Stamp build version into PowerShell manifest
                var path = @"bin\ViGEm.Management\ViGEm.Management.psd1";
                var psd1 = File.ReadAllText(path);
                psd1 = psd1.Replace("MODULE_VERSION", AppVeyor.Instance.BuildVersion);
                File.WriteAllText(path, psd1);
            });
}
