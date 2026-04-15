# XHarness — 開発ハーネス詳細

親メニューは [SKILL.md](SKILL.md) の **Step 2**（**0・1 の直後**）。このファイルは **決定論ゲート**・**テスト階層**の正本。

このリポジトリでは **モデルやプロンプトより、リポジトリ内の実行可能なハーネス**（ビルド・テスト・CI）が品質の主担当である。考え方の背景は [Harness Engineering ベストプラクティス（逆瀬川ちゃん, 2026）](https://nyosegawa.com/posts/harness-engineering-best-practices-2026/)（[「ハーネスがモデルより重要」](https://nyosegawa.com/posts/harness-engineering-best-practices-2026/#%E3%83%8F%E3%83%BC%E3%83%8D%E3%82%B9%E3%81%8C%E3%83%A2%E3%83%87%E3%83%AB%E3%82%88%E3%82%8A%E9%87%8D%E8%A6%81) ほか）に沿う。

## 記事の論点 → このリポの実装

出典: [Harness Engineering ベストプラクティス（逆瀬川ちゃん, 2026）](https://nyosegawa.com/posts/harness-engineering-best-practices-2026/)（目次の §1〜§7・MVH・アンチパターン）。

| 記事の原則（章のイメージ） | このリポジトリでの実装 |
|----------------------------|-------------------------|
| [ハーネスがモデルより重要](https://nyosegawa.com/posts/harness-engineering-best-practices-2026/#%E3%83%8F%E3%83%BC%E3%83%8D%E3%82%B9%E3%81%8C%E3%83%A2%E3%83%87%E3%83%AB%E3%82%88%E3%82%8A%E9%87%8D%E8%A6%81)（§7） | 完了条件は **`pnpm harness` 系**（カプセル化 + build + test）と **Azure Pipelines**；プロンプトだけに頼らない |
| **§1 リポジトリ衛生**（長文の「現状説明」だけを増やさない） | 期待動作の正本は **xUnit** と README / `ExitCode.cs`；`SKILL.md` は索引、手順は `steps-*.md` |
| **§1 テストはドキュメントより腐敗に強い** | 同じミスが二度出たら **テスト**（必要なら Helix 層）を足す |
| **§2 アーキをガードレールに** | **ソリューション分割**と **`scripts/check-encapsulation.mjs`**；公開 CI は **build 前**にカプセル化（[`eng/check-encapsulation.yml`](../../../eng/check-encapsulation.yml)） |
| フィードバックは速い層へ（§2） | **`pnpm check:encapsulation`** → **1 プロジェクトの `dotnet test`** → **`pnpm harness`** → **`pnpm harness:full`** → Azure |
| **§4 計画と実行の分離** | 下記「§4 ワークフロー」；TDD の観点は [steps-0-3-red-green-refactor.md](steps-0-3-red-green-refactor.md) Step 3 |
| E2E の層分け（§5） | **単体** vs **integration-tests + Helix** を混同しない（下記 §3） |

### 記事のアンチパターン → このリポでの回避

1. **プロンプトだけに頼る** → 変更後に **カプセル化 + build + test**（`pnpm harness` 系）を実行する。
2. **長文 README だけで仕様化** → 振る舞いは **テスト**で固定する。
3. **スキルを巨大化** → `SKILL.md` は索引、手順は `steps-*.md`。
4. **エージェント専用インフラだけを増やす** → **通常開発者と同じ** `dotnet` / `pnpm harness` を正本にする。
5. **ゲートが正当なフォークを壊す** → **テスト設計**を疑い、**フォークを先に責めない**。**セキュリティ不変は削らない**。
6. **`--no-verify`・ガード削除を促すスキル文** → **禁止級**（LINE スキルと同じ運用ルール）。

### MVH（記事 §10）とこのリポ

| 記事の段階 | このリポでの状態 |
|------------|------------------|
| **最短** | `dotnet build XHarness.slnx` |
| **日常** | 変更プロジェクトの **`dotnet test`** + 参照を触ったら **`pnpm check:encapsulation`** |
| **マージ相当（ローカル）** | **`pnpm harness`**（[package.json](../../../package.json)）= カプセル化 + build + test（restore は省略可） |
| **clone / CI 近傍** | **`pnpm harness:ci`** = カプセル化 + **`dotnet restore`** + build + test |
| **Debug/Release 両構成**（公開パイプラインのユニット行列に寄せる） | **`pnpm harness:full`** |
| **Arcade 本番相当** | `eng/common/CIBuild.cmd` / `eng/common/cibuild.sh`（`-prepareMachine` 等）— **重いので必要時のみ** |

## 1. 真実のソース（Single sources of truth）

| 種別 | 置く場所 | エージェントの扱い |
|------|-----------|-------------------|
| 振る舞いの仕様 | `tests/Microsoft.DotNet.XHarness.*.Tests` | 変更したら該当テストを通す |
| ユーザー向け契約 | README、`ExitCode.cs`、help テキスト | [steps-design-0-1.md](steps-design-0-1.md) |
| エージェント向け手順 | `.cursor/skills/xharness/SKILL.md` | 長文の独自設計書を増やさない |

## 2. 決定論的ゲート（LLM に任せない）

完了前に実行して結果を確認する（**順序は速い層から**）。**`pnpm harness` / `npm run harness`** は **カプセル化 → build → test**（restore 省略）。**`harness:ci`** は **restore 込み**、**`harness:full`** は **Debug/Release 両方**（LINE の `pnpm harness` / `harness:ci` / `harness:full` の役割分担と同型）。

0. **カプセル化（ProjectReference + Compile 外部取り込み）**: `pnpm check:encapsulation` または `node scripts/check-encapsulation.mjs`（LINE のレイヤーゲートと同型。**`pnpm harness` の先頭**と同一スクリプト）  
   - **ProjectReference**: [`scripts/check-encapsulation.mjs`](../../../scripts/check-encapsulation.mjs) の `allowedProjectReferences`。新規 `.csproj` はここに追加。  
   - **Compile Include**: プロジェクトフォルダ**外**の `.cs` を取り込む場合は **`allowedCompileIncludeOverrides`** にのみ明示許可（末尾 `/` でディレクトリ前方一致、なければファイル完全一致）。**TestRunners** のリンク共有はここでホワイトリスト化済み。新規の「他プロジェクトのソース直リンク」は **PR でルール追加**か **ProjectReference に寄せる**。  
   - **公開 CI**（`azure-pipelines-public.yml`）の Windows / macOS ユニットジョブは [`eng/check-encapsulation.yml`](../../../eng/check-encapsulation.yml) で **dotnet build より前**に同スクリプトを実行する。

1. **restore**（初回・SDK 変更・**`harness:ci` / `harness:full`**）: `dotnet restore XHarness.slnx` または `build.sh --restore`

2. **build**: `dotnet build XHarness.slnx`（構成を変えた検証は **`harness:full`**）

3. **unit / component tests**: `dotnet test`（全体または変更プロジェクトのみ）

4. **統合**（変更が及ぶとき）: [steps-4-8-gates.md](steps-4-8-gates.md) Step 9（Helix。**`harness:full` には含めない** — LINE の `test:api` / E2E と同じく別レイヤー）

SDK をリポの `.dotnet` に揃える場合は README の `build.sh` / `./.dotnet/dotnet` を使う。

### マージゲートが赤いとき（分岐・フォーク耐性の正本）

**先に** `pnpm harness`（または手順のバラし）なら **どの段か**を特定する。典型順序: **`check:encapsulation` → build → test**。

**先に** `dotnet test` のログで **どのプロジェクト・どのテスト**かを特定する。

| 落ちた段 | 典型原因 | 次の一手（セキュリティ不変は削らない） |
|----------|----------|----------------------------------------|
| **`check:encapsulation`** | 許可外の `ProjectReference` または **プロジェクト外の `Compile Include`** | **`allowedProjectReferences` / `allowedCompileIncludeOverrides`** を正当に更新するか、参照・配置を戻す。 |
| **build** | 参照・API 変更 | コンパイルエラーを解消。不要な public API 変更を避ける。 |
| **test（一般）** | 仕様変更 vs バグ | 意図した変更ならテスト更新。**バグなら Red で先に固定**（[steps-0-3-red-green-refactor.md](steps-0-3-red-green-refactor.md)）。 |
| **test（セキュリティ）** | 緩和で直した | **緩和しない**。[steps-pentest-tdd-loop.md](steps-pentest-tdd-loop.md) の期待に戻す。 |
| **OS 条件** | Apple / adb 未導入 | 既存テストの `Skip` 条件に合わせる。Linux だけの環境で Apple 実機テストを無理に通そうとしない。 |
| **Flaky** | タイミング | issue / 既存パターンを確認。決定的な待機へ。 |

**運用欠陥とゲート欠陥の切り分け**: **Helix キュー・デバイスプール・シークレット・フィード認証**だけ古い／ローカルに無いのは **README / `eng/` / パイプライン運用**で追う（ユニットだけでは検知されない）。**コードは妥当でマージゲートだけ赤い**なら **ゲート／テスト設計**を疑い、**フォークを先に責めない**。

**ペネトレ自走**は [steps-pentest-tdd-loop.md](steps-pentest-tdd-loop.md)。**「なぜ harness が赤いか」の一般分岐はこの表が正本**。

### クイックゲート（速度順）

| コマンド | 内容 |
|----------|------|
| `pnpm check:encapsulation` / `node scripts/check-encapsulation.mjs` | **ProjectReference レイヤーのみ**（単体でも可） |
| `dotnet build path/to/Single.csproj` | 1 プロジェクトだけ触ったとき |
| `dotnet test path/to/Single.Tests.csproj` | そのプロジェクトのテストのみ |
| `dotnet build XHarness.slnx && dotnet test XHarness.slnx` | build + test（**カプセル化は含まない**。マージ相当は `pnpm harness` を推奨） |
| `pnpm harness` / `npm run harness` | **カプセル化 + build + test**（restore なし・最速のマージ相当） |
| `pnpm harness:ci` / `npm run harness:ci` | 上記 + **`dotnet restore`**（clone 直後や SDK 更新後の **CI 近傍**） |
| `pnpm harness:full` / `npm run harness:full` | **`Debug` と `Release` の両方**で build + test（[`azure-pipelines-public.yml`](../../../azure-pipelines-public.yml) ユニット行列に寄せる） |
| `bash scripts/harness.sh` | `pnpm harness` と同順（リポルート基準） |

## 3. E2E の層（名前と期待値を一致させる）

| 層 | 何を証明するか | 実装 |
|----|----------------|------|
| **単体・コンポーネント** | 純粋ロジック・パース・ポリシー | `tests/Microsoft.DotNet.XHarness.*.Tests` |
| **統合（Helix）** | 実アプリ + クラウドデバイス | `tests/integration-tests/`、`tools/run-e2e-test.sh` |
| **手元の実機** | 特定 OS のみの挙動 | 人間 or 専用ジョブ（CI の OSX 等） |

**単体だけを「全部試した」と言わない**（Helix 層がある）。

## 4. ワークフロー（計画と実行の分離）

1. **計画**: 触るプロジェクト、失敗しうるテストを列挙。
2. **実行**: 最小差分で実装。
3. **検証**: 決定論的ゲートを実行。
4. **仕上げ**: 回帰テストを足す。

### 4.0 TDD（SKILL Step 3〜7）との接続

| タイミング | 推奨コマンド |
|------------|----------------|
| Step 4 Red を書いた直後 | 対象 `dotnet test` で **意図どおり赤**を確認 |
| Step 4 Red 確認後〜Step 5 の間 | **`.csproj` / `ProjectReference` を触るなら** **`pnpm check:encapsulation` を必須**（「早めに確認してもよい」ではなく **TDD ループと同時進行**。LINE のカプセル化と同型） |
| Step 5 Green のあと | **`ProjectReference` を変えたら** **再度 `pnpm check:encapsulation`**（Step 7 待ち禁止） |
| Step 6 Refactor 後 | フォーカス → 広く `dotnet test`；参照を動かしたら **再度 `pnpm check:encapsulation`** |
| **Step 7 完了条件** | **`pnpm harness`**（カプセル化 + build + 全 unit）。構成まわりを広く触ったら **`pnpm harness:full`** も検討。Step 4〜6 でカプセル化済みでも **マージ相当の再実行**として必ず通す。詳細は [steps-4-8-gates.md](steps-4-8-gates.md) |

## 4.1 定期観測（スケジュール CI）— Modifius 型の補助線

**日々のマージ可否**は **Azure Pipelines**（`azure-pipelines-public.yml` 等）とローカル **`pnpm harness` 系**が決める。週次・月次で LLM や静的集計を回す **Modifius 型**のワークフローは、このリポの**必須構成ではない**（入れるなら **PR ゲートとは別ジョブ**として **補助の定点観測**に留める）。

設計の参照例として [DMM Developers Blog（2026-04-01）: Modifius CI](https://developersblog.dmm.com/entry/2026/04/01/110000) を正本のひとつにできる（パイプライン分割・コスト・通知の語り口）。

### このリポとの役割分担（LINE スキル §4.1 と同型）

| 層 | 役割 | このリポの実装例 |
|----|------|------------------|
| **マージゲート（決定論）** | 壊れた変更を弾く | **`pnpm harness`** / **`harness:ci`** / **`harness:full`** + **Azure Pipelines** |
| **定点観測（補助）** | 負債の**見える化**と優先度議論 | 任意で別ワークフロー（**スキルや YAML に分析プロンプトを複製しない**。正本はリポ内の単一スクリプトかパイプライン定義に寄せる） |

静的な境界違反は **`check-encapsulation.mjs`** で機械的に検知する。AI 定点は **「関心事が混ざっているか」等の構造コメント**向きで、**決定論ゲートの代替にはしない**（記事の趣旨と整合）。

## 5. ソリューションの地図（要約）

- **CLI** — コマンド境界。厚みは **Common** へ。
- **Common** — 共有ロジック・セキュリティユーティリティ・ネットワーク。
- **Android / Apple / iOS.Shared** — 外部ツール・プラットフォーム固有。
- **TestRunners** — デバイス内。ホストとの契約は README とテストを正本に。

詳細は [SKILL.md](SKILL.md) セクション 3。

## 6. ハーネス強化（MVH）

同じミスを二度やったら **xUnit** か **統合テスト** のいずれかを足す。長い説明だけで代替しない。

## 7. 参照リンク

- [Harness Engineering（2026）](https://nyosegawa.com/posts/harness-engineering-best-practices-2026/)
- [dotnet/xharness](https://github.com/dotnet/xharness)
