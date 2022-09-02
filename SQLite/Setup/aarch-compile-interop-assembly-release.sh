#!/bin/bash

SCRIPT_DIR=`dirname "$BASH_SOURCE"`

export GCC="aarch64-unknown-linux-gnu-gcc"
export ARCH="arm64"
"$SCRIPT_DIR/compile-interop-assembly-release.sh"
