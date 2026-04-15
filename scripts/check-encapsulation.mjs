#!/usr/bin/env node
/**
 * カプセル化ゲート（LINE harness の check:encapsulation に相当）
 * 1) ProjectReference レイヤー
 * 2) 他プロジェクトのソースを Compile Include で取り込むパス（許可リスト外なら exit 1）
 */
import { readdirSync, readFileSync, statSync } from "node:fs";
import { basename, dirname, join, relative, resolve, sep } from "node:path";
import { fileURLToPath } from "node:url";

const __dirname = dirname(fileURLToPath(import.meta.url));
const repoRoot = join(__dirname, "..");

/** @type {Record<string, string[]>} プロジェクト名 → 許可する参照先 csproj ベース名 */
const allowedProjectReferences = {
  "Microsoft.DotNet.XHarness.Common": [],
  "Microsoft.DotNet.XHarness.Android": ["Microsoft.DotNet.XHarness.Common"],
  "Microsoft.DotNet.XHarness.iOS.Shared": ["Microsoft.DotNet.XHarness.Common"],
  "Microsoft.DotNet.XHarness.Apple": [
    "Microsoft.DotNet.XHarness.Common",
    "Microsoft.DotNet.XHarness.iOS.Shared",
  ],
  "Microsoft.DotNet.XHarness.CLI": [
    "Microsoft.DotNet.XHarness.Android",
    "Microsoft.DotNet.XHarness.Apple",
    "Microsoft.DotNet.XHarness.Common",
    "Microsoft.DotNet.XHarness.iOS.Shared",
  ],
  "Microsoft.DotNet.XHarness.TestRunners.Common": [],
  "Microsoft.DotNet.XHarness.TestRunners.Xunit": [
    "Microsoft.DotNet.XHarness.TestRunners.Common",
  ],
  "Microsoft.DotNet.XHarness.TestRunners.NUnit": [
    "Microsoft.DotNet.XHarness.TestRunners.Common",
  ],
  "Microsoft.DotNet.XHarness.DefaultAndroidEntryPoint.Xunit": [
    "Microsoft.DotNet.XHarness.TestRunners.Xunit",
  ],
  "Microsoft.DotNet.XHarness.Common.Benchmarks": [
    "Microsoft.DotNet.XHarness.Common",
  ],
  "Microsoft.DotNet.XHarness.Common.Tests": [
    "Microsoft.DotNet.XHarness.Common",
  ],
  "Microsoft.DotNet.XHarness.Android.Tests": [
    "Microsoft.DotNet.XHarness.Android",
  ],
  "Microsoft.DotNet.XHarness.Apple.Tests": [
    "Microsoft.DotNet.XHarness.Apple",
    "Microsoft.DotNet.XHarness.iOS.Shared",
  ],
  "Microsoft.DotNet.XHarness.CLI.Tests": [
    "Microsoft.DotNet.XHarness.CLI",
    "Microsoft.DotNet.XHarness.Common",
  ],
  "Microsoft.DotNet.XHarness.iOS.Shared.Tests": [
    "Microsoft.DotNet.XHarness.iOS.Shared",
  ],
  "Microsoft.DotNet.XHarness.TestRunners.Tests": [
    "Microsoft.DotNet.XHarness.TestRunners.Common",
    "Microsoft.DotNet.XHarness.TestRunners.NUnit",
    "Microsoft.DotNet.XHarness.TestRunners.Xunit",
  ],
};

/**
 * プロジェクト名 → リポジトリ相対パスの許可ルール。
 * - 末尾 `/` … そのディレクトリ配下のファイルのみ許可（前方一致）
 * - それ以外 … そのファイルパスと完全一致
 * 未指定のプロジェクトは「プロジェクトフォルダ外への Compile Include 禁止」
 */
const allowedCompileIncludeOverrides = {
  "Microsoft.DotNet.XHarness.TestRunners.Common": [
    "src/Microsoft.DotNet.XHarness.Common/",
    "src/Microsoft.DotNet.XHarness.iOS.Shared/Execution/EnviromentVariables.cs",
  ],
  "Microsoft.DotNet.XHarness.TestRunners.Xunit": [
    "src/Microsoft.DotNet.XHarness.Common/Xml/SecureXmlReaderSettings.cs",
    "src/Microsoft.DotNet.XHarness.Common/Utilities/WebServerStatefulSession.cs",
  ],
};

function getAllowedCompileRules(projectName) {
  return allowedCompileIncludeOverrides[projectName] ?? [];
}

function collectCsprojPaths() {
  const out = [];

  function walkSrc(dir) {
    for (const ent of readdirSync(dir, { withFileTypes: true })) {
      const p = join(dir, ent.name);
      if (ent.isDirectory()) walkSrc(p);
      else if (ent.name.endsWith(".csproj")) out.push(p);
    }
  }
  walkSrc(join(repoRoot, "src"));

  const benchDir = join(repoRoot, "benchmarks");
  if (statSync(benchDir, { throwIfNoEntry: false })?.isDirectory()) {
    for (const ent of readdirSync(benchDir, { withFileTypes: true })) {
      if (!ent.isDirectory()) continue;
      const sub = join(benchDir, ent.name);
      for (const f of readdirSync(sub)) {
        if (f.endsWith(".csproj")) out.push(join(sub, f));
      }
    }
  }

  const testsRoot = join(repoRoot, "tests");
  for (const ent of readdirSync(testsRoot, { withFileTypes: true })) {
    if (!ent.isDirectory() || !ent.name.startsWith("Microsoft.DotNet.XHarness")) continue;
    const testDir = join(testsRoot, ent.name);
    for (const f of readdirSync(testDir)) {
      if (f.endsWith(".csproj")) out.push(join(testDir, f));
    }
  }

  return out;
}

function extractProjectReferences(csprojContent) {
  const refs = [];
  const re = /<ProjectReference\s+Include="([^"]+\.csproj)"/gi;
  let m;
  while ((m = re.exec(csprojContent)) !== null) refs.push(m[1]);
  return refs;
}

/** Compile Include のパス（Update は対象外） */
function extractCompileIncludes(csprojContent) {
  const paths = [];
  const re = /<Compile\s+Include="([^"]+)"/gi;
  let m;
  while ((m = re.exec(csprojContent)) !== null) paths.push(m[1]);
  return paths;
}

function isPathInsideOrEqual(parentDir, childPath) {
  const p = resolve(parentDir);
  const c = resolve(childPath);
  if (p === c) return true;
  const prefix = p.endsWith(sep) ? p : p + sep;
  return c.startsWith(prefix);
}

function repoRelativePosix(absPath) {
  return relative(repoRoot, absPath).replace(/\\/g, "/");
}

function matchesAllowedCompile(relRepoPath, rules) {
  const n = relRepoPath.replace(/\\/g, "/");
  for (const rule of rules) {
    const r = rule.replace(/\\/g, "/");
    if (r.endsWith("/")) {
      if (n === r.slice(0, -1) || n.startsWith(r)) return true;
    } else if (n === r) return true;
  }
  return false;
}

function main() {
  const paths = collectCsprojPaths();
  const errors = [];

  for (const csprojPath of paths) {
    const projectName = basename(csprojPath, ".csproj");
    const allowed = allowedProjectReferences[projectName];
    if (allowed === undefined) {
      errors.push(
        `[encapsulation] 未登録のプロジェクト: ${relative(repoRoot, csprojPath)} — allowedProjectReferences に追加してください。`,
      );
      continue;
    }

    const content = readFileSync(csprojPath, "utf8");
    const projDir = dirname(csprojPath);

    const includes = extractProjectReferences(content);
    const allowedSet = new Set(allowed);

    for (const inc of includes) {
      const refName = basename(inc.replace(/\\/g, "/"), ".csproj");
      if (!allowedSet.has(refName)) {
        errors.push(
          `[encapsulation] 禁止 ProjectReference: ${projectName} → ${refName}（${relative(repoRoot, csprojPath)}）\n  許可: ${allowed.length ? allowed.join(", ") : "(なし)"}`,
        );
      }
    }

    const compileRules = getAllowedCompileRules(projectName);
    const compilePaths = extractCompileIncludes(content);

    for (const rawInc of compilePaths) {
      if (rawInc.includes("*") || rawInc.includes("?")) continue;
      const normalizedInc = rawInc.replace(/\\/g, "/");
      const resolvedAbs = resolve(projDir, normalizedInc);
      const relFromRepo = repoRelativePosix(resolvedAbs);
      if (relFromRepo.startsWith("..")) {
        errors.push(
          `[encapsulation] Compile Include がリポ外を指す: ${projectName} — "${rawInc}" → ${relFromRepo}`,
        );
        continue;
      }
      if (isPathInsideOrEqual(projDir, resolvedAbs)) continue;

      if (!matchesAllowedCompile(relFromRepo, compileRules)) {
        errors.push(
          `[encapsulation] 禁止 Compile Include（プロジェクト外ソース）: ${projectName}\n  パス: ${relFromRepo}\n  csproj: ${relative(repoRoot, csprojPath)}\n  許可ルール: ${compileRules.length ? compileRules.join(" | ") : "(なし — 外部取り込み不可)"}\n  → allowedCompileIncludeOverrides に追加するか、ProjectReference + 通常配置に変更してください。`,
        );
      }
    }
  }

  if (errors.length) {
    console.error("check-encapsulation: 失敗\n\n" + errors.join("\n\n"));
    process.exit(1);
  }
  console.log(
    "check-encapsulation: OK（ProjectReference + Compile Include 外部取り込み）",
  );
}

main();
