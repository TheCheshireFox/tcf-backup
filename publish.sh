#!/bin/bash

cd "$(dirname "$0")"

rm -rf publish
mkdir -p publish

# x64
dotnet publish -c Release -r linux-x64 -p:PublishSingleFile=true -p:SelfContained=true -p:EnableCompressionInSingleFile=true -p:PublishTrimmed=true --self-contained true -o publish/ TcfBackup/TcfBackup.csproj
dotnet publish -c Release -r linux-x64 -p:PublishSingleFile=true -p:SelfContained=true -p:EnableCompressionInSingleFile=true -p:PublishTrimmed=true --self-contained true -o publish/ TcfGoogleDriveAuth/TcfGoogleDriveAuth.csproj

mkdir publish/dist
mkdir publish/pdb

mv -v publish/*.pdb publish/pdb/
mv -v publish/tcf-backup publish/dist/
mv -v publish/tcf-google-drive-auth publish/dist/
mv -v publish/credentials.json publish/dist/
cp -v Makefile publish/dist/

