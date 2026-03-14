#!/usr/bin/env bash
set -euo pipefail

# Sync repo from WSL2 to a Windows directory and optionally build/run.
# Usage: ./scripts/sync-to-windows.sh [--build] [--run <project>]

WIN_DIR="${PF_WIN_DIR:-/mnt/c/dev/pixel-factory}"
BUILD=false
RUN_PROJECT=""

while [[ $# -gt 0 ]]; do
  case "$1" in
    --build) BUILD=true; shift ;;
    --run) RUN_PROJECT="$2"; shift 2 ;;
    --dir) WIN_DIR="$2"; shift 2 ;;
    *) echo "Unknown option: $1"; exit 1 ;;
  esac
done

echo "Syncing to $WIN_DIR ..."
mkdir -p "$WIN_DIR"
rsync -a --delete \
  --exclude='bin/' \
  --exclude='obj/' \
  --exclude='.git/' \
  "$(dirname "$(dirname "$(readlink -f "$0")")")/" \
  "$WIN_DIR/"
echo "Sync complete."

if $BUILD; then
  echo "Building on Windows..."
  dotnet.exe build "$WIN_DIR/PixelFactory.slnx"
fi

if [[ -n "$RUN_PROJECT" ]]; then
  echo "Running $RUN_PROJECT on Windows..."
  dotnet.exe run --project "$WIN_DIR/src/$RUN_PROJECT/$RUN_PROJECT.csproj"
fi
