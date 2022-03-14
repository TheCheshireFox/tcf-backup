#!/bin/bash

cd "$(dirname "$0")"

rm -rf publish
mkdir -p publish/{portable,standart}/{Release,pdb,dist/{bin,}}

function dotnet_publish()
{
  dotnet publish -c Release -r linux-x64 -nologo -clp:NoSummary -verbosity:quiet "$@" TcfBackup/TcfBackup.csproj
}

dotnet_publish -o publish/portable/Release -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:SelfContained=true -p:EnableCompressionInSingleFile=true -p:PublishTrimmed=true --self-contained true -nowarn:IL2104,IL2087,IL2091,IL2026,IL2090
dotnet_publish -o publish/standart/Release --no-self-contained

mv publish/portable/Release/*.pdb publish/portable/pdb
mv publish/portable/Release/tcf-backup publish/portable/dist/bin
cp Makefile.portable publish/portable/dist/Makefile

printf '\033[0;31m'
find publish/portable/Release/* -print 2>/dev/null
printf '\033[0m'

mv publish/standart/Release/*.pdb publish/standart/pdb
mv publish/standart/Release/*.dll publish/standart/dist/bin
mv publish/standart/Release/*.so publish/standart/dist/bin
mv publish/standart/Release/tcf-backup.deps.json publish/standart/dist/bin
mv publish/standart/Release/tcf-backup.runtimeconfig.json publish/standart/dist/bin
mv publish/standart/Release/tcf-backup publish/standart/dist/bin
cp Makefile.standart publish/standart/dist/Makefile

printf '\033[0;31m'
find publish/standart/Release/* -print 2>/dev/null
printf '\033[0m'