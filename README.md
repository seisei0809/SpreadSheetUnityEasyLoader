# Unity SpreadSheet Data Tools

GoogleスプレッドシートのデータをScriptableObjectとして取り込み、型設定・エディタ上でのプレビュー・検索/ソート機能を提供するUnityエディタ拡張です。

---

## 同梱ツール

- **SpreadSheetLoaderWindow**  
  GoogleスプレッドシートのデータをScriptableObject化するエディタウィンドウ  

- **SpreadSheetDataViewerWindow**  
  作成したScriptableObjectをエディタ上で検索・ソート・プレビューできるデータビューワー  

- **SpreadSheetData**  
  データ構造用ScriptableObject定義＆データ型管理クラス  

---

## インストール手順

1. [Releasesページ](https://github.com/seisei0809/SpreadSheetUnityEasyLoader/releases/tag/1.0)から最新版の `.unitypackage` をダウンロード  
2. Unityエディタ上で `Assets` → `Import Package` → `Custom Package...` を選択し、ダウンロードした `.unitypackage` をインポート  

---

## 使用方法

### 事前準備

Googleスプレッドシートを「ウェブに公開」します。

1. Googleスプレッドシートを開く  
2. メニューから **「ファイル」→「ウェブに公開」** を選択  
3. 公開対象を「**ドキュメント全体**」または「**シート単体**」で公開  
4. URLから `spreadsheetId` を取得（URLの `/d/` と `/edit` の間の文字列）

例：  https://docs.google.com/spreadsheets/d/【ここがspreadsheetId】/edit#gid=0

### SpreadSheetLoaderWindowの使い方

1. Unityメニューから `Tools` → `SpreadSheet Loader` を開く  
2. 必要項目を入力  

| 項目 | 内容 |
|:--|:--|
| Spreadsheet ID | 上記で取得したspreadsheetId |
| Sheet Name | スプレッドシートのシート名（タブ名） |
| Save Path | `Assets/` からの保存先フォルダ名 |

3. `Preview` ボタンでデータ読み込み＆プレビュー確認  
4. 必要なら型設定・Enum型の型名を入力  
5. 問題なければ `Save` で `ScriptableObject` を作成  

---

### SpreadSheetDataViewerWindowの使い方

1. Unityメニューから `Tools` → `SpreadSheet Data Viewer` を開く  
2. `Data` 欄に `SpreadSheetLoaderWindow` で作成した ScriptableObject をセット  
3. 検索・ソート条件を設定してデータ確認可能  

---

## ScriptableObjectの利用例

パッケージには[**Example.cs**](https://github.com/seisei0809/SpreadSheetUnityEasyLoader/blob/main/Assets/Example.cs)も同梱しています。  
このサンプルでは、スプレッドシートデータの行・キー・型別取得や、条件検索の実装例を示しています。

---

## 注意事項

- スプレッドシートは「ウェブに公開」しないと読み込めません  
- API制限のない[opensheet.vercel.app](https://opensheet.vercel.app/)を利用しているため、公開範囲に注意  

---

## ライセンス

MIT License  
© 2025 SeiseiUtility

