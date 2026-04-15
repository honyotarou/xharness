# TDD Step 3〜6 — 観点 → Red → Green → Refactor

**スキル上の位置づけ**: 通常の機能 TDD は **3→4→5→6→7**（Step 7 は [steps-4-8-gates.md](steps-4-8-gates.md)）。**セキュリティ（pentest 枝）**も Red / Green / ゲートの**作法は同じ**で、レールは [steps-pentest-tdd-loop.md](steps-pentest-tdd-loop.md)。

**スコープ「アプリ全体」**: このリポでは **アプリ＝[`XHarness.slnx`](../../../XHarness.slnx) に入る全部**（CLI / Common / Android / Apple / iOS.Shared / TestRunners と **`tests/Microsoft.DotNet.XHarness.*.Tests`**）。Step 4〜6 の途中は **反復の速さ**のため **1 つの `*Tests.csproj` + `--filter`** でもよいが、**完了の定義**は **Step 5 の `dotnet build XHarness.slnx`** と **Step 7 の `pnpm harness`**（= **ソリューションの全 unit**）で緑であること。**触っていないプロジェクトのテストが落ちたまま**「その機能だけできた」としない。

**カプセル化は Step 7 待ちにしない**: **ProjectReference レイヤー**と **`allowedProjectReferences`**（LINE の `ROUTE_LINE_CAPS` に相当）は **Step 4〜5 と同時進行**で守る。PR の **CI は dotnet build の直前**に `check-encapsulation` が走る（[steps-harness.md](steps-harness.md)）。**後からまとめて直させない**。

### エージェント（Cursor 等）— Step 4〜6 のあと **自走して Step 7 まで**

ユーザーが **Step 4〜6 だけ**を依頼した場合でも、**Step 6 が緑で終わった時点で止まらない**。**同じターン／セッションで Step 7 に進み**、[steps-4-8-gates.md](steps-4-8-gates.md) Step 7 に従い **`pnpm harness`**（または `npm run harness`）を**実行して結果を確認**する。「次は Step 7 ですか？」**と聞いて待たない**。

- **`pnpm harness` が赤い** → **どの Tests プロジェクトが落ちたかはソリューション全体のログで見る**（アプリ一部だけ直して見逃さない）。[steps-harness.md](steps-harness.md) の **「マージゲートが赤いとき」**に従い、**セキュリティ不変を削らず**に直すループを回す（緑になるまで自走）。
- **ユーザーが明示的に「ここで止めて」と言うまで**、4→5→6→7 は **一続きの作業**として扱う。

---

## Step 3 — 観点・受け入れ条件

**目的**: テストメソッド名とファイル・**csproj** まで決める。**この段階では本実装を書かない**（スケルトンのみ可）。

エージェントは次を出力する:

1. **スコープ**: 触る `src/` プロジェクトと対応する `tests/*.Tests`。
2. **Given / When / Then**（または表形式の受け入れ条件）。
3. **テストの置き場所**: 既存の `*Tests.cs` か新規ファイルか（`AttackerPerspective*` / `*Security*` の命名に合わせる）。
4. **モック方針**: プロセス・ファイル I/O・ネットワークをどう隔離するか。
5. **型・公開 API**: 変える型・メソッド名。

**チェック**:

- [steps-harness.md](steps-harness.md) のソリューション地図と矛盾しないか。
- **新規 `.csproj`**（`src/` / `tests/Microsoft.DotNet.XHarness.*` / `benchmarks/`）を追加するなら、**Step 5 のチェックリストに** [`scripts/check-encapsulation.mjs`](../../../scripts/check-encapsulation.mjs) の **`allowedProjectReferences` へプロジェクト名と許可参照を足すタスク**を含める（忘れると **Step 4〜5 中の `pnpm check:encapsulation`** および **CI** で即失敗）。
- **他プロジェクトの `.cs` を `Compile Include` で直リンクする**場合は、同スクリプトの **`allowedCompileIncludeOverrides`** にルールを足すタスクも含める（既存は TestRunners.Common / TestRunners.Xunit のみホワイトリスト）。
- **セキュリティ境界**なら [steps-pentest-tdd-loop.md](steps-pentest-tdd-loop.md) のチェックリストを先に見る。

---

## Step 4 — Red（失敗するテスト）

**目的**: **意図した理由で**失敗するテストを 1 本以上追加する（コンパイルエラーだけの Red は最小限に）。

手順:

1. Step 3 で決めたファイルに **最小の `[Fact]` / `[Theory]`**（既存スタイルに合わせる）を書く。
2. アサーションは **これから実装する振る舞い**を表す（現状のバグを「緑で固定」しない）。
3. 反復を速くするため **対象の `*Tests.csproj` だけ**（例）:

```bash
dotnet test tests/Microsoft.DotNet.XHarness.Common.Tests/Microsoft.DotNet.XHarness.Common.Tests.csproj --filter "FullyQualifiedName~ClassName"
```

   **アプリ全体の緑は Step 5〜7 で必ず確認**（上記「スコープ『アプリ全体』」）。

4. **`dotnet test` で赤を確認**してから次へ。
5. **`.csproj` の `ProjectReference` を増やす・変える Red**、または **CLI / プラットフォーム境界をまたぐ変更**を書いているなら、**この直後**に **`pnpm check:encapsulation`** を実行する。落ちたら **Red の置き場所**（許可外参照を誘発していないか）か **Step 3 のスコープ**を直してから Step 5 へ（**「あとでカプセル化」で進めない**）。

**カプセル化のコツ（Red 段階から意識）**:

- **CLI に厚みが乗る Red** を書くより、**Common の純関数・検証ロジック**を Red で表現し、CLI は薄い配線として検証する方が、**ProjectReference レイヤー**と相性がよい。
- **新規プロジェクト参照**が必要な Red なら、**先に** `allowedProjectReferences` の更新を Step 5 のタスクに書き、**Red 確認直後**に `pnpm check:encapsulation` で整合を見る。

**禁止**: このステップで本実装を入れて緑にすること（それは Step 5）。

---

## Step 5 — Green（最小実装）

**目的**: Step 4 のテストを **満たす最小差分**で通す。

手順:

1. 実装を追加・変更する（ルール: 無関係なリファクタ禁止）。
2. 同じ `dotnet test` で **緑**を確認。
3. **ソリューション全体が build できる**ことを確認（`dotnet build XHarness.slnx`）。
4. **新規 `.csproj` または `ProjectReference` を追加・変更したら** [`scripts/check-encapsulation.mjs`](../../../scripts/check-encapsulation.mjs) の **`allowedProjectReferences`** にエントリを足す（忘れると **CI / Step 7** で即失敗）。**外部 `Compile Include` を増やしたら** **`allowedCompileIncludeOverrides`** も更新。
5. **`ProjectReference` や csproj を触ったら** **Green のあと再度** **`pnpm check:encapsulation`**（**Step 7 まで待たない**）。

**禁止**: カプセル化を緩めて通す（正当な境界拡張なら `allowedProjectReferences` を **PR で明示更新**）。

---

## Step 6 — Refactor

**目的**: テストを **緑のまま**内部構造を良くする。

- 重複排除、命名、ファイル分割、早期 return。
- **新しい振る舞いは追加しない**（追加するなら Step 3 に戻る）。
- 再度フォーカス `dotnet test` → 必要なら **`dotnet test XHarness.slnx`** または Step 7 前提で **`pnpm harness`** に任せる（**アプリ全体**）。
- **リファクタで `ProjectReference` を動かしたら** この直後も **`pnpm check:encapsulation`**。

---

## ステップ完了後（Step 7 へ — 人間もエージェントも **省略しない**）

必ず **Step 7** に進み、**`pnpm harness`**（**カプセル化 + build + 全 unit**）で緑を確認する。Step 4〜5 で既にカプセル化を回していても、**Step 7 はマージ相当の一本化**として再度通す（LINE の `pnpm harness` と同じ）。**pentest 自走**でもラウンドごとに同じ（[steps-pentest-tdd-loop.md](steps-pentest-tdd-loop.md)）。

**エージェント**: 上記「Step 4〜6 のあと **自走して Step 7 まで**」に従い、**実際にコマンドを実行**し、ログを見て報告する（手順の列挙だけで終わらない）。
