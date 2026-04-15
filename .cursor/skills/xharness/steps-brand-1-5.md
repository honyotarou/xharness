# 親 1 の続き — パッケージ・TFM・互換（LINE の brand スロット）

メインの **親 1** とセットで読む。**NuGet パッケージとしての顔**と **サポート TFM** の方針を揃える。

## 確認すること

1. **`Directory.Build.props`** — `XHarnessNetTFMs` 等、複数 TFM を触る変更の影響。
2. **各 `.csproj`** — `PackageId`、説明文、ライセンス（MIT）、依存パッケージの範囲。
3. **破壊的変更** — メジャー相当か、README のインストール手順（プレリリース版ピン）に影響するか。
4. **下流** — dotnet/runtime や他リポが取り込む前提で、公開 API を増やす／変える理由を一言。

## リリースとの関係

NuGet 公開フロー・バージョン・寄稿は [steps-deploy.md](steps-deploy.md)（**12**）。

## 次のステップ

- 要件の親: [steps-design-0-1.md](steps-design-0-1.md) Step 1。
- ビルド検証: [steps-harness.md](steps-harness.md)（**2**）。
