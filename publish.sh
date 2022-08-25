#!/bin/bash

cd "$(dirname "$0")" || exit

function dotnet_publish()
{
	dotnet publish -c Release  -nologo -clp:NoSummary -verbosity:quiet "${@}" TcfBackup/TcfBackup.csproj
}

function dotnet_publish_portable() {
	DIR="${1}"
	RUNTIME="${2}"
	dotnet_publish -r $RUNTIME -o "$DIR/portable/Release" -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:SelfContained=true -p:EnableCompressionInSingleFile=true -p:PublishTrimmed=true --self-contained true -nowarn:IL2104,IL2087,IL2091,IL2026,IL2090
}

function dotnet_publish_standart() {
	DIR="${1}"
	RUNTIME="${2}"
	dotnet_publish -r $RUNTIME -o "$DIR/standart/Release" --no-self-contained
}

function make_portable() {
	DIR="${1}"

	mkdir -p "$DIR"/portable/{Release,pdb,dist/{bin,}}

	mv "$DIR"/portable/Release/*.pdb "$DIR/portable/pdb"
	mv "$DIR/portable/Release/tcf-backup" "$DIR/portable/dist/bin"
	cp Makefile.portable "$DIR/portable/dist/Makefile"

	printf '\033[0;31m'
	find "$DIR/portable/Release/*" -print 2>/dev/null
	printf '\033[0m'
}

function make_standart() {
	DIR="${1}"

	mkdir -p "$DIR"/standart/{Release,pdb,dist/{bin,}}

	mv "$DIR"/standart/Release/*.pdb "$DIR/standart/pdb"
	mv "$DIR"/standart/Release/*.dll "$DIR/standart/dist/bin"
	mv "$DIR"/standart/Release/*.so "$DIR/standart/dist/bin"
	mv "$DIR/standart/Release/tcf-backup.deps.json" "$DIR/standart/dist/bin"
	mv "$DIR/standart/Release/tcf-backup.runtimeconfig.json" "$DIR/standart/dist/bin"
	mv "$DIR/standart/Release/tcf-backup" "$DIR/standart/dist/bin"
	cp Makefile.standart "$DIR/standart/dist/Makefile"

	printf '\033[0;31m'
	find "$DIR/standart/Release/*" -print 2>/dev/null
	printf '\033[0m'
}

function make_for_runtime() {
	RUNTIME="${1}"

	mkdir -p "publish/$RUNTIME"
	dotnet_publish_portable "publish/$RUNTIME" $RUNTIME
	make_portable "publish/$RUNTIME"

	dotnet_publish_standart "publish/$RUNTIME" $RUNTIME
	make_standart "publish/$RUNTIME"
}

rm -rf publish

make_for_runtime linux-x64
make_for_runtime linux-arm64
