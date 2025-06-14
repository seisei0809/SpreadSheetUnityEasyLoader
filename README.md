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

1. [Releasesページ](https://github.com/seisei0809/SpreadSheetUnityEasyLoader/releases/tag/1.1)から最新版の `.unitypackage` をダウンロード  
2. Unityエディタ上で `Assets` → `Import Package` → `Custom Package...` を選択し、ダウンロードした `.unitypackage` をインポート  

---

## 使用方法

### 事前準備

1. Googleスプレッドシートを開く  
2. 一般的なアクセスを **「共有」→「リンクを知っている全員」** に変更  
3. URLから `spreadsheetId` を取得（URLの `/d/` と `/edit` の間の文字列）

例：  https://docs.google.com/spreadsheets/d/【ここがspreadsheetId】/edit#gid=0

### SpreadSheetLoaderWindowの使い方

1. Unityメニューから `Tools` → `SpreadSheet Loader` を開く  
2. 必要項目を入力  

| 項目 | 内容 |
|:--|:--|
| Spreadsheet ID | 上記で取得したspreadsheetId |
| Sheet Name | スプレッドシートのシート名（タブ名） |
| Start Cell | 範囲指定の開始セル（例：A3）※空なら全体 |
| End Cell | 範囲指定の終了セル（例：D10）※空なら全体 |
| Save Path | `Assets/` からの保存先フォルダ名 |

 **Start CellとEnd Cellは両方入力するか、両方空にする必要があります。片方だけ入力した場合はエラーとなります。**

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

リポジトリには[**Example.cs**](https://github.com/seisei0809/SpreadSheetUnityEasyLoader/blob/main/Assets/Example.cs)があります。  
このサンプルでは、スプレッドシートデータの行・キー・型別取得や、条件検索の実装例を示しています。

---

## 注意事項

- **スプレッドシートは「リンクを知ってる全員に公開」にしないと読み込めません**  
  必ず事前に共有から公開設定を行ってください  

- **opensheet-seisei-custom.vercel.app** を利用しているため、**公開範囲に注意**してください  
  公開されたスプレッドシートは**URLを知っている全員が閲覧可能**になります  

- **スプレッドシートの構成ルール**
  - 途中に**空データのセルや空行は不可**です
  - 画像・ファイル添付などセルに文字列以外の要素を含めることも不可。表には文字のみ。
  - 指定した開始行目を「ヘッダ行（キー名）」として使用します  
  - データ行は開始行目以降に記述してください  

例：

| ID | Name   | EnemyType | Power |
|:----|:--------|:------------|:--------|
| 1  | Goblin | Goblin     | 20     |
| 2  | Orc    | Orc        | 50     |
| 3  | Draco  | Dragon     | 100    |

---

## ライセンス

MIT License  
© 2025 SeiseiUtility

