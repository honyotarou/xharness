#!/usr/bin/env bash
set -euo pipefail
root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$root"
node scripts/check-encapsulation.mjs
dotnet build XHarness.slnx
dotnet test XHarness.slnx --no-build
