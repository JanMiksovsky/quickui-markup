This QuickUI folder contains the tools and associated files for using QuickUI under OS/X.

The \lib folder contains the QuickUI .NET binaries (qb.exe and qc.exe).
The \bin folder contains bash shell scripts for running the .NET binaries on OS/X via mono.

To install on OS/X:
1. Download Mono from http://www.mono-project.com. Mono allows .NET executables such as the QuickUI tools to run under OS/X and Linux.
2. Copy the contents of this QuickUI folder to a location such as /Developer/Applications/QuickUI.
3. Add the location of the bin folder from step #2, e.g., /Developer/Applications/QuickUI/bin, to your path.
4. If you installed in a location other than /Developer/Applications/QuickUI, then update the qb and qc scripts in the bin folder to point to the location you used.
