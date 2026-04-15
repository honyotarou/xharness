---
name: xharness
description: >-
  dotnet/xharness 向け統合ワークフロー（/xharness）。番号メニューでデザイン0・1→ハーネス2→TDD3〜7→ゲート8〜11→リリース12。
  harness / harness:ci / harness:full、Helix、TDD、pentest（steps-pentest-tdd-loop）、NuGet/CI。
  Use when: /xharness, /x, XHarness, xharness, harness, pentest, ペネトレ, dotnet xharness, Arcade, Helix, TestRunner.
---

> **リポジトリローカルスキル**: `AGENTS.md` / `docs/adr/` は **このリポをルートに開いたとき**に解決。無い場合の正本は [README.md](../../../README.md)。

# XHarness（`/xharness`）

**やること**: 下のメニューから **番号** または **キーワード**（`pentest` / `helix`）を選ぶ → **対応する `steps-*.md` を Read**（このファイルは索引。長文は置かない）。

---

## 1. メニュー（入口だけ）

| 種類 | 選び方 |
|------|--------|
| **番号** | `0`〜`12`、または `check` |
| **名前** | `helix` / `pentest`（番号と同列の別入口） |

```
【デザイン・要件】親は 0 と 1 のみ
  0   CLI 契約・ログ UX・環境変数 → steps-design-0-1.md（Step 0）
      サブコマンド/help の壁打ち → steps-rich-menu-wallball.md
  1   要件・受け入れ・破壊的変更チェック → steps-design-0-1.md（Step 1）+ steps-brand-1-5.md
      （パッケージメタ・TFM・バージョン方針）

【ドメイン枝】（LINE の orthopedics に相当する任意の深掘りスロット）
  helix   Helix・統合試験の壁打ち → steps-orthopedics-wallball.md

【セキュリティ】（TDD 本線の Step 番号は pentest 正本の対応表に従う）
  pentest       攻撃者視点・自走ループ → steps-pentest-tdd-loop.md または /pentest-tdd-loop

【ハーネス】0・1 の直後や実装前に推奨
  2   harness / harness:ci / harness:full・dotnet・CI 近傍 → steps-harness.md
      ※ harness が赤いときの分岐の正本もこのファイル「マージゲートが赤いとき」

【TDD・機能追加】
  3 観点 / 4 Red / 5 Green / 6 Refactor → steps-0-3-red-green-refactor.md
  7〜11・check → steps-4-8-gates.md（7 = ローカル harness 相当まで含む完了条件）

【リリース・公開】
  12  NuGet/CI・寄稿の心がけ（手順内の段は deploy 専用）→ steps-deploy.md
  check  harness:ci（または harness:full）→ 統合（必要時）→ steps-4-8-gates.md
```

---

## 2. `steps-*` 早見表

| ファイル | 主な Step / 入口 |
|----------|------------------|
| [steps-design-0-1.md](steps-design-0-1.md) | **0** / **1** |
| [steps-rich-menu-wallball.md](steps-rich-menu-wallball.md) | **0**・CLI 表層の壁打ち |
| [steps-brand-1-5.md](steps-brand-1-5.md) | **1**・パッケージ / TFM |
| [steps-orthopedics-wallball.md](steps-orthopedics-wallball.md) | **helix**（統合・Helix の壁打ち） |
| [steps-pentest-tdd-loop.md](steps-pentest-tdd-loop.md) | **pentest 正本**。分岐表は [steps-harness.md](steps-harness.md) へ |
| [steps-harness.md](steps-harness.md) | Step **2**・`harness` / `harness:ci` / `harness:full`・**マージゲートが赤いとき**・テスト階層 |
| [steps-0-3-red-green-refactor.md](steps-0-3-red-green-refactor.md) | **3〜6** |
| [steps-4-8-gates.md](steps-4-8-gates.md) | **7〜11**・**check** |
| [steps-deploy.md](steps-deploy.md) | **12** |

**その他の正本**: [README.md](../../../README.md)、[CODE_OF_CONDUCT.md](../../../CODE_OF_CONDUCT.md)、[ExitCode.cs](../../../src/Microsoft.DotNet.XHarness.Common/CLI/ExitCode.cs)。

---

## 3. リポジトリ地図（ルール優先）

| 場所 | 役割 |
|------|------|
| `src/Microsoft.DotNet.XHarness.CLI/` | エントリ・サブコマンド（**厚みは Common / 各プラットフォームへ**） |
| `src/Microsoft.DotNet.XHarness.Common/` | 共有基盤・ネットワーク・セキュリティユーティリティ |
| `src/Microsoft.DotNet.XHarness.Android/` | Android・ADB・APK |
| `src/Microsoft.DotNet.XHarness.Apple/` | Apple オーケストレーション |
| `src/Microsoft.DotNet.XHarness.iOS.Shared/` | リスナー・ログ・結果 XML・パーサ |
| `src/Microsoft.DotNet.XHarness.TestRunners.*` | デバイス内ランナー |
| `tests/Microsoft.DotNet.XHarness.*.Tests/` | **xUnit** 単体・コンポーネントテスト |
| `tests/integration-tests/` | **Helix** 向け E2E |
| `eng/` | Arcade・パイプライン |
| `scripts/check-encapsulation.mjs` | カプセル化ゲート（`allowedProjectReferences` 等） |

---

## 4. 実装前提（1 行ずつ）

| 領域 | メモ |
|------|------|
| CLI | 引数パースとユーザー向けメッセージ。秘密はログに出さない（`XHARNESS_LOG_ISSUED_COMMAND` 等）。 |
| Common | 入力検証・パス・XML/JSON・Web サーバポリシーは **テストで回帰**（`*Security*` / `AttackerPerspective*`）。 |
| Android / Apple | 外部プロセス（adb / mlaunch）。**OS 前提**が違うテストは条件付きスキップに合わせる。 |
| TestRunners | デバイス内。ホスト側 CLI との契約（環境変数・TCP）は README / 既存テストを正本に。 |
| Arcade | `eng/common/CIBuild.cmd`・`azure-pipelines*.yml`。**本番 NuGet** の運用は [steps-deploy.md](steps-deploy.md)。 |
| カプセル化 | `pnpm check:encapsulation`（`scripts/check-encapsulation.mjs`）。新規 `.csproj` / 参照変更は **`allowedProjectReferences`** を忘れない。 |
| **harness が赤い** | 分岐の正本 → [steps-harness.md](steps-harness.md) **「マージゲートが赤いとき」** |

---

## 5. 共通ルール（短く）

### 5.1 よく使う経路

- **挙動・UX から**: `0 → 1 → 2 → 3`。**ライブラリだけ**: `2`（必要なら）→ `3`。
- **TDD**: `3→4→5→6` のあと **`7` で `pnpm harness` 緑**。Step **4〜5** で `.csproj` / `ProjectReference` を触るなら **`pnpm check:encapsulation` を Step 7 待ちにせず同時進行**（[steps-0-3-red-green-refactor.md](steps-0-3-red-green-refactor.md)）。必要なら 8〜9。**リリース相当**: `11` または `check`。
- **pentest**: [steps-pentest-tdd-loop.md](steps-pentest-tdd-loop.md)。Red／Green／harness の Step 対応は同ファイル先頭の表。

### 5.2 デザイン完了の意味（このリポ）

- **0**: `xharness help` 系・**ログ・環境変数の挙動**が README / 実装と矛盾しない状態（必要なら help 出力の差分をファイルに残す）。
- **1**: **受け入れ条件・触るパス・破壊的変更の有無**を箇条書き。承認前に本番相当の実装を広げすぎない。

### 5.3 秘密・本番

- 秘密をチャットに貼らない。CI のログ抑制は `eng/common-variables.yml` 等。
- **マージゲートの赤**と **本番フィード認証**を混同しない（前者は [steps-harness.md](steps-harness.md)「マージゲートが赤いとき」）。

### 5.4 TDD とカプセル化（同時進行）

- **Red なし Green 禁止**。
- Step 4〜5 のループの中で **`pnpm check:encapsulation`**（「Step 7 で harness を回すから今はいい」でスキップしない。公開 CI は build 前に走る）。
- **新規 `.csproj` または許可外参照を増やす**ときは `scripts/check-encapsulation.mjs` の **`allowedProjectReferences`** を更新（忘れると即落ちる）。**他プロジェクトの `.cs` を `Compile Include`** する場合は **`allowedCompileIncludeOverrides`** も更新。
- セキュリティ論点は **pentest 正本**の表。**回帰は xUnit に残す**（`dotnet test` が毎回実行）。

### 5.5 テスト階層

- **`tests/*.Tests`** ＝ 高速・決定論が主。**`tests/integration-tests` + Helix** ＝ 実アプリ・クラウド。混同しない（[steps-harness.md](steps-harness.md)）。

### 5.6 完了・その他

- 変更後は **`dotnet build` + 関連 `dotnet test`**（大きい変更はソリューション全体）。マージ相当の一本化は **`pnpm harness`** / **`npm run harness`**。**restore からの CI 近傍**は **`pnpm harness:ci`**、**Debug/Release 両方**は **`pnpm harness:full`**（定義は [package.json](../../../package.json)）。
- **Azure Pipelines**: PR の決定論ゲートの正本。詳細は [steps-harness.md](steps-harness.md) のみ。
- **Step 12**: パッケージ公開・バージョン・寄稿ルート。CLI のユーザー向け挙動を変えたら **0 / 1** も更新検討。

---

## 6. 参照リンク

- [Harness Engineering（2026）](https://nyosegawa.com/posts/harness-engineering-best-practices-2026/) — 背景（このリポでの対応は [steps-harness.md](steps-harness.md) 冒頭）
- [dotnet/xharness](https://github.com/dotnet/xharness)
- [Arcade xharness-runner Readme](https://github.com/dotnet/arcade/blob/main/src/Microsoft.DotNet.Helix/Sdk/tools/xharness-runner/Readme.md)
