#!/bin/bash

SCRIPT_DIR=`dirname "$BASH_SOURCE"`

export GCC="gcc"
export ARCH="x64"
"$SCRIPT_DIR/compile-interop-assembly-release.sh"
