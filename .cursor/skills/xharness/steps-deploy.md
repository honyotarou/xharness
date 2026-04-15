# XHarness — リリース・寄稿（Step 12）

親メニューは [SKILL.md](SKILL.md) の **Step 12**。**dotnet エコシステム向けの公開**と **上流への寄稿**の心がけ。

## pre / post（lineskill 互換）

### pre（入口条件）

- `pnpm harness:ci`（または同等）が緑（[steps-4-8-gates.md](steps-4-8-gates.md) の `check`）
- 破壊的変更の有無・バージョン方針が決まっている

### post（出口条件）

- CI の成果物/公開フローが成功し、README の install 手順と矛盾しない
  - 公開 CI の正本: [`azure-pipelines-public.yml`](../../../azure-pipelines-public.yml)
  - 内部/公式の正本: [`azure-pipelines.yml`](../../../azure-pipelines.yml)

## Cursor / エージェントで進めるとき

- **Step 12** を選ばれたら、下記を **順に**進める（秘密をチャットに貼らせない）。
- 実際の **NuGet API キー・AzDO トークン**は人間の環境のみ。エージェントは手順とチェックリストを出すに留める。

## 0 — 前提

- **.NET SDK**（`global.json` に合わせる）
- リポは **Arcade** ベース（`eng/common`）
- パッケージは **dotnet-eng** 等のフィードに乗る運用（README の install 例を正本に）

## 1 — ローカルで品質

```bash
dotnet build XHarness.slnx
dotnet test XHarness.slnx
```

[steps-4-8-gates.md](steps-4-8-gates.md) の **check** と同等以上。

## 2 — バージョン・メタデータ

- プレリリース版のピン留め（README の `--version`）との整合。
- [steps-brand-1-5.md](steps-brand-1-5.md) のパッケージ方針。

## 3 — パック（必要なら）

公式パイプラインに任せるのが通常。ローカル検証のみなら Arcade / `build.sh` の **pack** 引数はリポの慣例に従う。

## 4 — 上流への PR（OSS 寄稿）

- **dotnet/xharness** のガイドラインに従う（[CODE_OF_CONDUCT.md](../../../CODE_OF_CONDUCT.md)）。
- 変更説明は **何を・なぜ・どう試したか**（英語が既定のことが多い）。
- **破壊的変更**は README・`ExitCode`・下流への影響を明示。

## 5 — リリース後の確認

- フィードにパッケージが現れるか、README の `dotnet tool install` が通るか（バージョン文字列）。

## 6 — 品質ゲート（リリース前の再掲）

- [steps-4-8-gates.md](steps-4-8-gates.md) の **check**
- 統合を触ったら [steps-4-8-gates.md](steps-4-8-gates.md) Step **9**

## 参照

- [README.md](../../../README.md)（Installation、Development instructions）
- [dotnet/xharness](https://github.com/dotnet/xharness)（Issues の振り分け）
