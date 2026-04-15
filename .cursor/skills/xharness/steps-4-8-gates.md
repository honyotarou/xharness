# TDD Step 7〜11 + check — ゲート・統合・仕上げ

**スキル上の位置づけ**: Step **3〜6** は [steps-0-3-red-green-refactor.md](steps-0-3-red-green-refactor.md)。**pentest**は [steps-pentest-tdd-loop.md](steps-pentest-tdd-loop.md) がレールだが、**各ラウンドの完了ゲートはこのファイルの Step 7 と同じ harness**。

---

## Step 7 — 層別ゲート（ローカル harness）

**目的**: ローカルで決定論的に「マージ相当」を証明する（**pentest の各ラウンド末もここ**）。

### マージ相当の一本化（推奨）

```bash
pnpm harness
```

中身は **`node scripts/check-encapsulation.mjs`**（= `pnpm check:encapsulation`）→ **`dotnet build XHarness.slnx`** → **`dotnet test XHarness.slnx --no-build`**（[package.json](../../../package.json)）。**clone 直後や SDK 更新後**は **`pnpm harness:ci`**（`restore` 込み）。**構成・条件付きコンパイルを広く触った**ときは **`pnpm harness:full`**（`Debug` / `Release` の両方）を追加。**Step 4〜5 で既にカプセル化を回していても**、Step 7 は **マージ相当の再検証**として必ず通す（LINE の `pnpm harness` と同じ）。

手動で同じにする場合:

```bash
pnpm check:encapsulation
dotnet build XHarness.slnx
dotnet test XHarness.slnx
```

### 狭い反復（開発中）

1. **`pnpm check:encapsulation`** — `ProjectReference` を触ったとき必須
2. `dotnet build`（変更した csproj のみでも可）
3. `dotnet test` を **変更した Tests プロジェクト**に限定
4. マージ前または広い変更: **`pnpm harness`**（必要なら **`pnpm harness:full`**）またはソリューション全体の build + test

**失敗時**: [steps-harness.md](steps-harness.md) の「マージゲートが赤いとき」。テストを意味なく緩めない。

---

## Step 8 — 追加のプロジェクト横断テスト

**目的**: 複数プロジェクトにまたがる変更の回帰。

- 例: `Common` の公開 API を変えたら **CLI.Tests** も実行。
- `dotnet test` を **2 つ以上の Tests プロジェクト**で順に（またはフィルタで）確認。

---

## Step 9 — 統合（Helix / integration-tests）

**目的**: 単体では足りない **実アプリパス** を検証する。

README に従い、必要なら:

```bash
./tools/run-e2e-test.sh Apple/Simulator.Tests.proj
```

（実際の `.proj` 名は `tools/` と `tests/integration-tests/` で確認。）

- **ローカルですべて再現できない**ことがある → Helix ログを正本に。

---

## Step 10 — ベンチマーク・負荷（任意）

**目的**: ホットスポットの性能回帰に注意する。

```bash
dotnet run --project benchmarks/Microsoft.DotNet.XHarness.Common.Benchmarks/ -c Release
```

（TDD の必須ステップではない。変更がパフォーマンスクリティカルなとき。）

---

## Step 11 — 仕上げ

**目的**: コード以外の「真実」も更新する。

1. **README.md** — 新オプション・環境変数・例。
2. **ExitCode.cs** — 終了コードの追加・変更とドキュメント整合。
3. **eng/** — CI 変数（例: `eng/common-variables.yml`）を変えたらコメントと整合。
4. **セキュリティ境界** — [steps-pentest-tdd-loop.md](steps-pentest-tdd-loop.md) で攻撃面を再確認。

---

## check — 品質ゲート（リリース相当）

次を **すべて緑**にしてから報告する:

```bash
pnpm harness:ci
```

（日常の **check** で restore まで含めない場合は **`pnpm harness`** でも可。公開 CI の **Debug/Release 両行列**に寄せるなら **`pnpm harness:full`** を追加。）

または同順の手動実行:

```bash
pnpm check:encapsulation
dotnet restore XHarness.slnx
dotnet build XHarness.slnx
dotnet test XHarness.slnx
```

統合を変えた場合は **Step 9** を追加。公開 CI（`azure-pipelines-public.yml`）の **Windows / macOS ユニットビルド**は **`eng/check-encapsulation.yml`** により **dotnet build 前**にカプセル化を実行する。

---

## よくある分岐

| 変更の種類 | 最低限のゲート |
|------------|----------------|
| **`ProjectReference` / 新規 `.csproj`** | **`pnpm check:encapsulation`** + `allowedProjectReferences` 更新 |
| `Common` のみ | `pnpm check:encapsulation` + `dotnet test tests/Microsoft.DotNet.XHarness.Common.Tests/...` + build slnx |
| `CLI` + 引数・ログ | + `CLI.Tests`、README / ExitCode |
| `Android` / `Apple` | 対応 `*.Tests` + **OS が合う環境**で確認 |
| 統合テスト or Helix 定義 | Step **9** + パイプライン結果 |
| セキュリティ・入力検証 | **pentest 正本** + 該当 `*Security*` テスト全通し |
