#!/usr/bin/env bash

set -e -o pipefail

dotnet tool restore
dotnet fake $@