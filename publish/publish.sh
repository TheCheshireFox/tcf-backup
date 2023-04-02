#!/bin/bash

cd "$(dirname "$0")" || exit

SOLUTION_ROOT="$(realpath "$PWD"/..)"
PUBLISH_ROOT="$PWD"

function dotnet_publish()
{
	dotnet publish -c Release -nologo -clp:NoSummary -verbosity:quiet /p:SolutionDir="$SOLUTION_ROOT" "${@}" "$SOLUTION_ROOT/TcfBackup/TcfBackup.csproj"
}

function dotnet_publish_portable() {
	DIR="${1}"
	RUNTIME="${2}"
	PLATFORM="${3}"
	dotnet_publish -r "$RUNTIME" -p:Platform="$PLATFORM" -p:PlatformTarget="$PLATFORM" -o "$DIR/portable/Release" -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:SelfContained=true -p:EnableCompressionInSingleFile=true -p:PublishTrimmed=true --self-contained true
}

function dotnet_publish_standard() {
	DIR="${1}"
	RUNTIME="${2}"
	PLATFORM="${3}"
	dotnet_publish -r "$RUNTIME" -p:Platform="$PLATFORM" -p:PlatformTarget="$PLATFORM" -o "$DIR/standard/Release" --no-self-contained
}

function setup_systemd_files() {
  ROOT="${1}"
  
  mkdir -p "$ROOT"/lib/systemd/system
  cp files/tcf-backup@.service "$ROOT"/lib/systemd/system
}

function make_portable() {
	DIR="${1}"

  mkdir -p "$DIR"/portable/{pdb,dist/usr/bin}
  mv "$DIR"/portable/Release/*.pdb "$DIR/portable/pdb"
  mv "$DIR/portable/Release/tcf-backup" "$DIR/portable/dist/usr/bin"
  setup_systemd_files "$DIR/portable/dist"
}

function make_standard() {
	DIR="${1}"

  mkdir -p "$DIR"/standard/{pdb,dist/opt/tcf-backup}
	mv "$DIR"/standard/Release/*.pdb "$DIR/standard/pdb"
	mv "$DIR"/standard/Release/*.dll "$DIR/standard/dist/opt/tcf-backup"
	mv "$DIR"/standard/Release/*.so "$DIR/standard/dist/opt/tcf-backup"
	mv "$DIR/standard/Release/tcf-backup.deps.json" "$DIR/standard/dist/opt/tcf-backup"
	mv "$DIR/standard/Release/tcf-backup.runtimeconfig.json" "$DIR/standard/dist/opt/tcf-backup"
	mv "$DIR/standard/Release/tcf-backup" "$DIR/standard/dist/opt/tcf-backup"
  setup_systemd_files "$DIR/standard/dist"
}

function make_for_runtime() {
	RUNTIME="${1}"
	PLATFORM="${2}"
	
	PUBLISH_DIR="$PUBLISH_ROOT/publish/$RUNTIME"

	mkdir -p "$PUBLISH_DIR"
	dotnet_publish_portable "$PUBLISH_DIR" "$RUNTIME" "$PLATFORM"
	make_portable "$PUBLISH_DIR"

	dotnet_publish_standard "$PUBLISH_DIR" "$RUNTIME" "$PLATFORM"
	make_standard "$PUBLISH_DIR"
}

rm -rf publish

make_for_runtime linux-x64 x64
make_for_runtime linux-arm64 arm64
