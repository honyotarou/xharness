# helix — Helix・統合試験の壁打ち（LINE の orthopedics 枝スロット）

メイン番号表のキーワード **`helix`**。LINE スキルでは **orthopedics** がドメイン深掘り用だったスロットで、**このリポでは Helix / 統合試験**向け。

## いつ開くか

- `tests/integration-tests/` を触る、または **Helix ジョブが赤い**とき。
- 「単体では足りないが E2E を回すほどでもない」境界の相談。

## 壁打ちの出力イメージ

1. **検証したい振る舞い**（どのプラットフォーム・どのアプリ種別か）。
2. **既存の `.proj` / スクリプト**（`tools/run-e2e-test.sh`）で足りるか、新規ジョブが要るか。
3. **ローカル再現の可否**（Mac 必須か、Linux のみか）。
4. **失敗時のログの見方**（Helix アーティファクト、xharness ログ）。

## 正本

- [README.md](../../../README.md) の「Running E2E tests」
- `tests/integration-tests/`、`tools/run-e2e-test.sh`

## 次のステップ

- 手順の実行: [steps-4-8-gates.md](steps-4-8-gates.md) Step **9**。
- テスト階層の定義: [steps-harness.md](steps-harness.md) の「E2E の層」。
