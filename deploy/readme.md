This folder contains tools to deploy the QuickUI runtime and tools
to http://quickui.org.

The deployment process is moving from a couple of bash scripts to Python.
During this transition, the process is a bit clunky.

1. Use Visual Studio to build tools/Setup/Setup.sln and create the .msi
installer.
2. Run mkzip to create the QuickUI.zip file for OS/X and Linux.
