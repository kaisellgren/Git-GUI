# Git-GUI

Git-GUI is Windows only graphical user interface for Git source code management.

#####*Please note: Git-GUI is not ready yet -- i.e. it's an alpha version that does not support everything you need. I'm working on it to make it a stable product.*

#### FAQ

#####Do I need the official Git installed on my computer?
No. Git-GUI is standalone, it does not need Git installation.

#####Does Git-GUI have something to do with the official Git built-in tool called git-gui (```git gui``` on command line)?
No. Git-GUI is a software written by me (Kai Sellgren) as a hobby project.

#####How does it look like?
<a href="http://img502.imageshack.us/img502/1176/screenshoton5272012at85.png"><img src="http://img32.imageshack.us/img32/3193/70794004.png" alt="Git-GUI" /></a>

*Remember: heavily work in progress!*

#####What do I need to run it?
- Windows Vista, 7 or 8.
- You need to have .NET framework 4.5 runtime installed/updated. Windows Update will do that for you if it does not have already.

### Project goals
I aim to make a complete product that features the majority of Git functionality including from basic stuff to Stashes, Remotes and Bisects.

### Contributing

So you want to fix a bug or add a feature? Or perhaps you just want to try it out? Here's what you need to do:

- Install [.NET Framework 4.5](http://www.microsoft.com/download/en/details.aspx?displaylang=en&id=27541) or newer.
- Install [Visual Studio 2012 RC](http://www.microsoft.com/visualstudio/11/en-us/downloads) or newer.
- Install [Nuget](http://nuget.org/) for Visual Studio (it's a package manager).
- Restart Visual Studio.
- Install [LibGit2Sharp](http://nuget.org/packages/LibGit2Sharp) via NuGet: ```Install-Package LibGit2Sharp```
- Install [Microsoft.Windows.Shell](https://nuget.org/packages/Microsoft.Windows.Shell) via NuGet: ```Install-Package Microsoft.Windows.Shell```
- Restart Visual Studio or reload the project.
- Press F5.