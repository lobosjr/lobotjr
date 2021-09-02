# Contributing

This project was originally written by LobosJR - a main condition of any work done is he needs to understand the changes:
1. It's crucial that any changes be done in a way that LobosJR can continue development, should a developer leave the project.
2. You will need to create change documentation explaining your updates, and to coordinate with the lead developer (currently EmpyrealHell) into coordinated change notes for LobosJR.  Currently a google doc is used.
3. Final review of new changes and documentation is by LobosJR: project development follows his schedule and might go dormant for a while until he has time to review them.

## Project Setup

1. The project can be opened with the community version of Visual Studio.  You will need a windows machine or VM - Mono won't cut it.
2. Don't forget to NuGet your packages prior to building
3. If you are using the official git distribution, you will want line ends configured to minimize churn every time a file is saved:
	1. use the git command: `git config core.autocrlf true`
	2. If you notice every time you are planning to commit there are a bunch of "^M" characters suddenly added to lines you didn't edit, this is what is happening