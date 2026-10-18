# FileFlow

**Version 1.0** · .NET 8 · Windows Forms

FileFlow is a Windows desktop app for organizing files with user-defined rules. It can move files by extension, analyze folder contents, find files that have not been accessed for a chosen number of days, detect exact duplicates with SHA-256, and keep activity statistics in a local SQLite database.

## Features

- Create, edit, enable, and delete file-moving rules.
- Run background rule-based moves on a 10-second timer.
- Analyze a folder and report file counts, total size, and extension distribution.
- Find older files by last-access time.
- Detect duplicate files by matching file sizes and SHA-256 hashes.
- Review activity counts and recent cleanup sessions in the dashboard.

## Requirements

- Windows
- .NET 8 SDK
- Visual Studio 2022 or another .NET 8 compatible build environment

## Download and run

1. On GitHub, select **Code → Download ZIP**, then extract the ZIP file fully.
2. Open `FileFlow.sln` from the extracted folder in Visual Studio 2022.
3. Let Visual Studio restore the NuGet packages, then build and run the project.

Or use PowerShell from the extracted folder:

```powershell
dotnet build .\FileFlow.sln
dotnet run --project .\FileFlow\FileFlow.csproj
```

The app uses SQLite for rules and activity records. Its database is created locally when the application starts; runtime database files are not included in this repository.

## Important behavior

Review your rules and target folders before enabling the app. When a destination already contains a file with the same name, the current implementation deletes that destination file before moving the source file. The full automation flow also deletes duplicate files it finds. On startup, the app attempts to register itself to run when the current Windows user signs in.

## Versioning

This repository represents **v1.0**. Further versions are planned.
