#!/bin/bash

cd "$(dirname "$0")" || exit 1

export CREDENTIALS_BASE64

envsubst < GDriveClientSecrets.template > GDriveClientSecrets.cs