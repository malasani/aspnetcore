# Build ASP.NET Core from Source

This document outlines how to build the source in the aspnetcore repo locally for development purposes.

For more info on issues related to build infrastructure and ongoing work, see <https://github.com/dotnet/aspnetcore/labels/area-infrastructure>.

## Step 0: Getting started

This tutorial assumes that you are familiar with:

- Git
- Command line fundamentals in your operating system of choice

## Step 1: Clone the source code

ASP.NET Core uses git submodules to include the source from a few other projects. In order to pull the sources of the these submodules when cloning the repo, be sure to pass the `--recursive` flag to the `git clone` command.

```ps1
git clone --recursive https://github.com/dotnet/aspnetcore
```

If you've already cloned the aspnetcore repo without fetching subdmoule sources, you can fetch them after cloning by running the following command.

```ps1
git submodule update --init --recursive
```

> :bulb: Some ISPs have been know to use web filtering software that has caused issues with git repository cloning, if you experience issues cloning this repo please review <https://help.github.com/en/github/authenticating-to-github/using-ssh-over-the-https-port>.

## Step 2: Install pre-requisites

Developing in the aspnetcore repo requires some additional tools to build the source code and run integration tests.

### On Windows

Building ASP.NET Core on Windows (10, version 1803 or newer) requires that you have the following tooling installed.

> :bulb: Be sure you have least 10 GB of disk space and a good Internet connection. The build scripts will download several tools and dependencies onto your machine.

#### [Visual Studio 2019](https://visualstudio.com)

Visual Studio 2019 (16.8) is required to build the repo locally. If you don't have visual studio installed you can run [eng/scripts/InstallVisualStudio.ps1](/eng/scripts/InstallVisualStudio.ps1) to install the exact required dependencies.

> :bulb: By default, the script will install Visual Studio Enterprise Edition, however you can use a different edition by passing the `-Edition` flag.
> :bulb: To install Visual Studio from the preview channel, you can use the `-Channel` flag to set the channel (`-Channel Preview`).
> :bulb: Even if you have installed Visual Studio, we still recommend using this script to install again to avoid errors due to missing components.

```ps1
PS> ./eng/scripts/InstallVisualStudio.ps1  [-Edition {Enterprise|Community|Professional}] [-Channel {Release|Preview}]
```

> :bulb: To execute the setup script or other PowerShell scripts in the repo, you may need to update the execution policy on your machine.
> You can do so by running the `Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser` command
> in PowerShell. For more information on execution policies, you can read the [execution policy docs](https://docs.microsoft.com/en-us/powershell/module/microsoft.powershell.security/set-executionpolicy).

The  [global.json](/global.json) file specifies the minimum requirements needed to build using `msbuild`. The [eng/scripts/vs.json](/eng/scripts/vs.json) file provides a description of the components needed to build within VS. If you plan on developing in Visual Studio, you will need to have these components installed.

> :bulb: The `IntallVisualStudio.ps1` script mentioned above reads from the `vs.json` file to determine what components to install.

#### [Git](https://git-scm.org)

If you're reading this, you probably already have Git installed to support cloning the repo as outlined in Step 1.

#### [NodeJS](https://nodejs.org)

Building the repo requires version 10.14.2 or newer of Node. You can find installation executables for Node at https://nodejs.org.

#### [Yarn](https://yarnpkg.com/)

NodeJS installes the Node package manager (npm) by default. This repo depends on yarn, an alternate package manager for the Node ecosystem. You can install Yarn from the command line using the following command.

```ps1
npm install -g yarn
```

#### Java Development Kit in Windows*

This repo contains some Java source code that depends on an install of the JDK v11 or newer. The JDK can be installed from either:
  * OpenJDK <https://jdk.java.net/>
  * Oracle's JDK <https://www.oracle.com/technetwork/java/javase/downloads/index.html>

Alternatively, you can run [eng/scripts/InstallJdk.ps1](/eng/scripts/InstallJdk.ps1) to install a version of the JDK that will only be used in this repo.

```ps1
PS> ./eng/scripts/InstallJdk.ps1
```

The build should find any JDK 11 or newer installation on the machine as long as the `JAVA_HOME` environment variable is set. Typically, your installation will do this automatically. However, if it is not set you can set the environment variable manually:
      * Set `JAVA_HOME` to `RepoRoot/.tools/jdk/win-x64/` if you used the `InstallJdk.ps1` script.
      * Set `JAVA_HOME` to `C:/Program Files/Java/jdk<version>/` if you installed the JDK globally.

#### Chrome

This repo contains a Selenium-based tests require a version of Chrome to be installed. Download and install it from <https://www.google.com/chrome>.

#### Wix (Optional)

If you plan on working with the Windows installers defined in [src/Installers/Windows](../src/Installers/Windows), you will need to install the Wix toolkit from https://wixtoolset.org/releases/.

### On macOS/Linux

You can also build ASP.NET Core on macOS or Linux. macOS Sierra or newer is required if you're building on macOS. If you're building on Linux, your machine will need to meet the [.NET Core Linux preequisities](https://docs.microsoft.com/en-us/dotnet/core/linux-prerequisites).

> :bulb: Be sure you have least 10 GB of disk space and a good Internet connection. The build scripts will download several tools and dependencies onto your machine.

#### curl/wget

`curl` and `wget` are command line tools that can be used to download files from an HTTP server. Either utility will need to be installed in order to complete the setup. Typically, these will be included on your machine by default.

If netier utility is installed, you can install curl (https://curl.haxx.se) or wget (https://www.gnu.org/software/wget).

##### Git

If you've made it this far, you've already got `Git` installed. Sit back, relax, and move on to the next requirement.

#### [NodeJS](https://nodejs.org)

Building the repo requires version 10.14.2 or newer of Node. You can find installation executables for Node at https://nodejs.org.

#### [Yarn](https://yarnpkg.com/)

NodeJS installs the Node package manager (npm) by default. This repo depends on yarn, an alternate package manager for the Node ecosystem. You can install Yarn from the command line using the following command.

```bash
$ npm install -g yarn
```

#### Java Development Kit

This repo contains some Java source code that depends on an install of the JDK v11 or newer. The JDK can be installed from either:
  * OpenJDK <https://jdk.java.net/>
  * Oracle's JDK <https://www.oracle.com/technetwork/java/javase/downloads/index.html>

Similar to [the instructions above for Windows](#java-development-kit-in-windows), be sure that the the `JAVA_HOME` environment variable is set to the location of your Java installation.

## Step 3: Build the repo

Before opening our .sln/.slnf files in Visual Studio or VS Code, you will need to build the repo locally.

### In Visual Studio

To set up your project for development on Visual Studio, you'll need to execute the following command.

```ps1
PS1> .\restore.cmd
```

> :bulb: If you happen to be working on macOS or Linux, you can use the `restore.sh` command.

This will download the required tools and restore all projects inside the repository. At that point, you should be able
to open the .sln file or one of the project specific .slnf files to work on the projects you care about.

   > :bulb: Pro tip: you will also want to run this command after pulling large sets of changes. On the master
   > branch, we regularly update the versions of .NET Core SDK required to build the repo.
   > You will need to restart Visual Studio every time we update the .NET Core SDK.

Typically, you want to focus on a single project within the monorepo. For example,
if you want to work on Blazor WebAssembly, you'll need to launch the solution file for that project by changing into the `src/Components`
directory and executing `startvs.cmd` in that directory like so:

```ps1
PS1> cd src\Components
PS1> .\startvs.cmd
```

After opening the solution in Visual Studio, you can build/rebuild using the controls in Visual Studio.

> :exclamation: VS for Mac does not currently support opening .slnf files so you must use VS Code when developing on macOS. 

#### A brief interlude on solution files

We have a single .sln file for all of ASP.NET Core, but most people don't work with it directly because Visual Studio
doesn't currently handle projects of this scale very well.

Instead, we have many Solution Filter (.slnf) files which include a sub-set of projects. See the Visual Studio
documentation [here](https://docs.microsoft.com/en-us/visualstudio/ide/filtered-solutions?view=vs-2019) for more
information about Solution Filters.

These principles guide how we create and manage .slnf files:

1. Solution files are not used by CI or command line build scripts. They are meant for use by developers only.
2. Solution files group together projects which are frequently edited at the same time.
3. Can't find a solution that has the projects you care about? Feel free to make a PR to add a new .slnf file.

### In Visual Studio Code

Before opening the project in Visual Studio Code, you will need to make sure that you have built the project.
You can find more info on this in the "Building on command-line" section below.

Using Visual Studio Code with this repo requires setting environment variables on command line first.
Use these command to launch VS Code with the right settings.

> :bulb: Note that you'll need to launch Visual Studio Code from the command line in order to ensure that it picks up the environment variables. To learn more about the Visual Studio Code CLI, you can check out [the docs page](https://code.visualstudio.com/docs/editor/command-line).

On Windows (requires PowerShell):

```ps1
# The extra dot at the beginning is required to 'dot source' this file into the right scope.

. .\activate.ps1
code .
```

On macOS/Linux:

```bash
source activate.sh
code .
```

> :bulb: Note that if you are using the "Remote-WSL" extension in VSCode, the environment is not supplied
> to the process in WSL.  You can workaround this by explicitly setting the environment variables
> in `~/.vscode-server/server-env-setup`.
> See https://code.visualstudio.com/docs/remote/wsl#_advanced-environment-setup-script for details.

## Building on command-line

When developing in VS Code, you'll need to use the `build.cmd` or `build.sh` scripts in order to build the project. You can learn more about the command line options available, check out [the section below](using-dotnet-on-command-line-in-this-repo).

On Windows:

```ps1
.\build.cmd
```

On macOS/Linux:

```bash
./build.sh
```

By default, all of the C# projects are built. Some C# projects require NodeJS to be installed to compile JavaScript assets which are then checked in as source. If NodeJS is detected on the path, the NodeJS projects will be compiled as part of building C# projects. If NodeJS is not detected on the path, the JavaScript assets checked in previously will be used instead. To disable building NodeJS projects, specify `-noBuildNodeJS` or `--no-build-nodejs` on the command line.

## Step 4: Make your code changes

At this point, you will have all the dependencies installed and a code editor to up and running to make changes in. Once you've made changes, you will need to rebuild the project locally to pick up your changes. You'll also need to run tests locally to verify that your changes worked.

The section below provides some helpful guides for using the `dotnet` CLI in the ASP.NET Core repo.

## Using `dotnet` on command line in this repo

Because we are using pre-release versions of .NET Core, you have to set a handful of environment variables
to make the .NET Core command line tool work well. You can set these environment variables like this:

On Windows (requires PowerShell):

```ps1
# The extra dot at the beginning is required to 'dot source' this file into the right scope.

. .\activate.ps1
```

On macOS/Linux:

```bash
source ./activate.sh
```

> :bulb: Be sure to set the environment variables using the "activate" script above before executing the `dotnet` command inside the repo.

### Running tests on command-line

Tests are not run by default. Use the `-test` option to run tests in addition to building.

On Windows:

```ps1
.\build.cmd -test
```

On macOS/Linux:

```bash
./build.sh --test
```

> :bulb: If you're working on changes for a particular subset of the project, you might not want to execute the entire test suite. Instead, only run the tests within the subdirectory where changes were made. This can be accomplished by passing the `projects` property like so: `.\build.cmd -test -projects .\src\Framework\test\Microsoft.AspNetCore.App.UnitTests.csproj`.

### Building a subset of the code

This repository is large. Look for `build.cmd`/`.sh` scripts in subfolders. These scripts can be used to invoke build and test on a smaller set of projects.

Furthermore, you can use flags on `build.cmd`/`.sh` to build subsets based on language type, like C++, TypeScript, or C#. Run `build.sh --help` or `build.cmd -help` for details.

### Build properties

Additional properties can be added as an argument in the form `/property:$name=$value`, or `/p:$name=$value` for short. For example:

```ps1
.\build.cmd -Configuration Release
```

Common properties include:

Property                 | Description
-------------------------|-------------------------------------------------------------------------------------------------------------
Configuration            | `Debug` or `Release`. Default = `Debug`.
TargetArchitecture       | The CPU architecture to build for (x64, x86, arm, arm64).
TargetOsName             | The base runtime identifier to build for (win, linux, osx, linux-musl).

### Use the result of your build

After building ASP.NET Core from source, you will need to install and use your local version of ASP.NET Core.
See ["Artifacts"](./Artifacts.md) for more explanation of the different folders produced by a build.

Building installers does not run as part of `build.cmd` run without parameters, so you should opt-in for building them:

```ps1
.\build.cmd -all -pack -arch x64
.\build.cmd -all -pack -arch x86 -noBuildJava
.\build.cmd -buildInstallers
```

*Note*: Additional build steps listed above aren't necessary on Linux or macOS.

* Run the installers produced in `artifacts/installers/{Debug, Release}/` for your platform.
* Add a NuGet.Config to your project directory with the following content:

  ```xml
  <?xml version="1.0" encoding="utf-8"?>
  <configuration>
      <packageSources>
          <clear />
          <add key="MyBuildOfAspNetCore" value="C:\src\aspnet\AspNetCore\artifacts\packages\Debug\Shipping\" />
          <add key="NuGet.org" value="https://api.nuget.org/v3/index.json" />
      </packageSources>
  </configuration>
  ```

  *NOTE: This NuGet.Config should be with your application unless you want nightly packages to potentially start being restored for other apps on the machine.*

* Update the versions on `PackageReference` items in your .csproj project file to point to the version from your local build.

  ```xml
  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.SpaServices" Version="3.0.0-dev" />
  </ItemGroup>
  ```

Some features, such as new target frameworks, may require prerelease tooling builds for Visual Studio.
These are available in the [Visual Studio Preview](https://www.visualstudio.com/vs/preview/).

## Resx files

If you need to make changes to a .resx file, run `dotnet msbuild t:/Resgen <path to csproj>`. This will update the generated C#.

## Troubleshooting

This section contains a troubleshooting guide for common issues you might run into while building the repo.

### Common error: CS0006

Opening solution filters and building may produce an error code CS0006 with a message such

> Error CS0006 Metadata file 'C:\src\aspnet\AspNetCore\artifacts\bin\Microsoft.AspNetCore.Metadata\Debug\netstandard2.0\Microsoft.AspNetCore.Metadata.dll' could not be found

The cause of this problem is that the solution filter you are using does not include the project that produces this .dll. This most often occurs after we have added new projects to the repo, but failed to update our .sln/slnf files to include the new project. In some cases, it is sometimes the intended behavior of the .slnf which has been crafted to only include a subset of projects.

#### You can fix this in one of three ways

1. Build the project on command line. In most cases, running `build.cmd` on command line solves this problem.
2. If the project is missing from the .sln file entirely, you can use `dotnet sln add` to add it, or else right click on the solution/folder in Visual Studio and choose Add->Existing Project, and adding it.
3. If it is present in the .sln, but not the .slnf, you can update the solution filter to include the missing project. You can either do this one by right-clicking on project in Visual Studio and choosing to load it's direct dependencies, and then saving.  Alternatively, you can hand edit the .slnf file - it's a fairly simple json format.

### Common error: Unable to locate the .NET Core SDK

Executing `.\restore.cmd` or `.\build.cmd` may produce these errors:

> error : Unable to locate the .NET Core SDK. Check that it is installed and that the version specified in global.json (if any) matches the installed version.
> error MSB4236: The SDK 'Microsoft.NET.Sdk' specified could not be found.

In most cases, this is because the option _Use previews of the .NET Core SDK_ in VS2019 is not checked. Start Visual Studio, go to _Tools > Options_ and check _Use previews of the .NET Core SDK_ under _Environment > Preview Features_.

### Common error: HTTP Error 500.33 - ANCM Request Handler Load Failure

The [ASP.NET Core Module](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/aspnet-core-module) (ANCM) for IIS is not supported when running projects in this repository.

After using `startvs.cmd` to open a solution in Visual Studio, the Kestrel web host option must be used (name of the project) and not IIS Express.

Example of running the `MvcSandbox` project:

`.\startvs.cmd .\src\Mvc\Mvc.sln`

![Web host options in Visual Studio](./vs-iis-express-aspnet-core-mvc-sandbox.jpg)

### Common error: error : Unable to load the service index for &hellip;

When attempting to restore servicing tags e.g. `v3.1.7`,  the NuGet.config file may contain internal feeds that are not accessible. This will result in errors such as

``` text
...\aspnetcore\.dotnet\sdk\3.1.103\NuGet.targets(123,5): error : Unable to load the service index for source https://pkgs.dev.azure.com/dnceng/_packaging/darc-int-dotnet-extensions-784b0ffa/nuget/v3/index.json. [...\Temp\1gsd3rdo.srb\restore.csproj] [...\.nuget\packages\microsoft.dotnet.arcade.sdk\1.0.0-beta.20213.4\tools\Tools.proj]
```

The `darc-int-...` feeds in NuGet.config are used only when building internally and are not needed after the tags are created. Delete all such entries in the file and retry.
