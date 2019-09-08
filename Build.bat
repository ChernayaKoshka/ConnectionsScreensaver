@echo off
dotnet build -c Release
:: this script needs https://www.nuget.org/packages/ilmerge

:: set your target executable name (typically [projectname].exe)
SET APP_NAME=Fancy.exe

:: Set build, used for directory. Typically Release or Debug
SET ILMERGE_BUILD=Release

:: Set platform, typically x64
SET ILMERGE_PLATFORM=x64

:: set your NuGet ILMerge Version, this is the number from the package manager install, for example:
:: PM> Install-Package ilmerge -Version 3.0.21
:: to confirm it is installed for a given project, see the packages.config file
SET ILMERGE_VERSION=3.0.29

:: the full ILMerge should be found here:
SET ILMERGE_PATH=%USERPROFILE%\.nuget\packages\ilmerge\%ILMERGE_VERSION%\tools\net452
:: dir "%ILMERGE_PATH%"\ILMerge.exe

echo Merging %APP_NAME% ...

cd bin\Release\net461\

:: ILMerge breaks when the PDB is there. Dunno why, I tried changing the PDB format to Full without success ¯\_(ツ)_/¯
del *.pdb

%ILMERGE_PATH%\ilmerge.exe /target:exe /out:FancyScreensaver.exe /wildcards %APP_NAME% *.dll

move FancyScreensaver.exe FancyScreensaver.scr

echo Cleaning up...

RD /S /Q ".\cs\"
RD /S /Q ".\da\"
RD /S /Q ".\de\"
RD /S /Q ".\en"
RD /S /Q ".\es\"
RD /S /Q ".\fa\"
RD /S /Q ".\fi\"
RD /S /Q ".\fr\"
RD /S /Q ".\it\"
RD /S /Q ".\ja\"
RD /S /Q ".\ko\"
RD /S /Q ".\mk\"
RD /S /Q ".\nl\"
RD /S /Q ".\pl\"
RD /S /Q ".\pt-BR\"
RD /S /Q ".\ru\"
RD /S /Q ".\sv\"
RD /S /Q ".\tr\"
RD /S /Q ".\zh-Hans\"
RD /S /Q ".\zh-Hant\"

del *.pdb
del *.dll
del *.exe*

echo Done!