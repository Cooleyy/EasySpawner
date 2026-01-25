#!/usr/bin/env bash
set -euo pipefail

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ps1_path="$script_dir/build-and-run.ps1"
ps1_win_path="$(wslpath -w "$ps1_path")"

powershell.exe -NoProfile -ExecutionPolicy Bypass -File "$ps1_win_path" "$@"
