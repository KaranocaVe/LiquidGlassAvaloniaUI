#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PROJECT_NAME="${1:-${CF_PAGES_PROJECT_NAME:-}}"

if [[ -z "${PROJECT_NAME}" ]]; then
  echo "Usage: CF_PAGES_PROJECT_NAME=<project-name> $0" >&2
  echo "   or: $0 <project-name>" >&2
  exit 1
fi

if [[ -x "${HOME}/.dotnet-local/dotnet" ]]; then
  export DOTNET_ROOT="${HOME}/.dotnet-local"
  export PATH="${HOME}/.dotnet-local:${PATH}"
  DOTNET_BIN="${HOME}/.dotnet-local/dotnet"
else
  DOTNET_BIN="dotnet"
fi

if ! command -v npx >/dev/null 2>&1; then
  echo "npx is required to run Wrangler." >&2
  exit 1
fi

if [[ -z "${CLOUDFLARE_API_TOKEN:-}" ]]; then
  echo "CLOUDFLARE_API_TOKEN is not set." >&2
  echo "Set it before running this script in a non-interactive shell." >&2
  exit 1
fi

PUBLISH_DIR="${ROOT_DIR}/LiquidGlassAvaloniaUI.Demo.Browser/bin/Release/net10.0-browser/publish/wwwroot"

"${DOTNET_BIN}" publish "${ROOT_DIR}/LiquidGlassAvaloniaUI.Demo.Browser/LiquidGlassAvaloniaUI.Demo.Browser.csproj" -c Release
npx -y wrangler pages deploy "${PUBLISH_DIR}" --project-name "${PROJECT_NAME}"
