# 親 Step 0 / 1 — CLI 契約・要件（LINE スキル型）

メイン番号表では **0 と 1 の 2 段**。

- **このファイルの「Step 0」** → メインの **親 0**（ユーザー向け CLI・ログ・環境変数）。続けて **サブ** → [steps-rich-menu-wallball.md](steps-rich-menu-wallball.md)（help 文言の壁打ち）。
- **このファイルの「Step 1」** → メインの **親 1**（受け入れ・破壊的変更）。続けて **パッケージ方針** → [steps-brand-1-5.md](steps-brand-1-5.md)。

正本: [README.md](../../../README.md)、[ExitCode.cs](../../../src/Microsoft.DotNet.XHarness.Common/CLI/ExitCode.cs)。

---

## Step 0 — CLI 契約・ログ UX・環境変数

**目的**: ユーザーと CI が依存する**外部契約**を固定する（チャットだけにしない）。

エージェントは次を確認・必要ならドキュメントやテストで追う:

1. **`xharness help` / サブコマンド help** — オプション名・説明が実装と一致しているか。
2. **終了コード** — `ExitCode.cs` とドキュメントの対応。
3. **環境変数** — README「Other settings」の一覧（`XHARNESS_LOG_ISSUED_COMMAND`、`XHARNESS_TCP_BIND_LOOPBACK_ONLY` 等）。
4. **ログ** — 色・タイムスタンプ・**発行コマンド行の赤入れ**の挙動。

**完了の目安**: 変更がある場合は README かコメント付きテストで **再発検知**できる状態。

---

## Step 1 — 要件・受け入れ・破壊的変更

**入力**: 「何をしたいか」  
**出力**: **受け入れ条件**（OK まで本番相当の実装を広げすぎない）

エージェントは次を出力する:

1. **スコープ**: 触る `src/` プロジェクトと `tests/*.Tests`。
2. **Given / When / Then**（または表）。
3. **破壊的変更**の有無（CLI 引数・終了コード・環境変数の意味変更）。
4. **プラットフォーム**（Apple のみ / Android のみ / 全 OS）と **手元で検証できる範囲**。

**チェック**:

- [SKILL.md](SKILL.md) のリポ地図と矛盾しないか。
- **新規プロジェクトや参照の追加が要件に含まれるなら**、実装フェーズ（Step 4〜5）で **`pnpm check:encapsulation` と TDD を同時進行**する前提で、`allowedProjectReferences` 更新をタスクに書く（[steps-0-3-red-green-refactor.md](steps-0-3-red-green-refactor.md)）。
- セキュリティ境界を変えるなら [steps-pentest-tdd-loop.md](steps-pentest-tdd-loop.md) の観点を先に読む。

---

## 次のステップ

- **ハーネス**: [steps-harness.md](steps-harness.md)（**2**）。
- **TDD**: [steps-0-3-red-green-refactor.md](steps-0-3-red-green-refactor.md)（**3〜6**）。
