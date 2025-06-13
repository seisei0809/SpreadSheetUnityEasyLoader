using MiniJSON;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using SeiseiUtilyty;
using System.Linq;

/// <summary>
/// スプレッドシート読み込み用のエディタウィンドウ
/// </summary>
public class SpreadSheetLoaderWindow : EditorWindow
{
    /// <summary>
    /// スプレッドシートのID
    /// </summary>
    private string spreadsheetId = "";
    /// <summary>
    /// 読み込むシートの名前
    /// </summary>
    private string sheetName = "";
    /// <summary>
    /// タイムアウトの秒数
    /// </summary>
    private const int timeOut = 5;
    /// <summary>
    /// セーブするパス
    /// </summary>
    private string savePath = "";
    /// <summary>
    /// データのプレビュー
    /// </summary>
    private SpreadSheetData previewData = null;
    /// <summary>
    /// スクロールの現在座標
    /// </summary>
    private Vector2 scrollPosition = Vector2.zero;
    /// <summary>
    /// プレビューを出しているか
    /// </summary>
    private bool isPreviewed = false;
    /// <summary>
    /// 行ごとの型設定辞書
    /// </summary>
    private Dictionary<string, MultiValueType> rowTypeDict = new();
    /// <summary>
    /// Enum型用の型名辞書
    /// </summary>
    private Dictionary<string, string> enumTypeNameDict = new();
    /// <summary>
    /// 範囲の開始セル
    /// </summary>
    private string startCell = "";
    /// <summary>
    /// 範囲の終了セル
    /// </summary>
    private string endCell = "";

    /// <summary>
    /// ウィンドウを表示
    /// </summary>
    [MenuItem("Tools/SpreadSheet Loader")]
    public static void ShowWindow()
    {
        // ウィンドウを作成
        var window = GetWindow<SpreadSheetLoaderWindow>("SpreadSheet Loader");
        // 最小サイズ
        window.minSize = new Vector2(400, 500);  
    }

    private void OnGUI()
    {
        // 入力フィールドの更新
        DrawInputFields();

        EditorGUILayout.Space();

        // プレビューを出すか
        OpenPreview();

        // プレビューがまだないならリターン
        if (previewData == null) return;

        DrawRowTypeUI();

        DrawEnumTypeUI();

        // プレビューの表示
        DrawPreview();

        // セーブするかリセットするか
        SaveOrReset();
    }

    /// <summary>
    /// ここのフィールドにIDと名前を入力
    /// </summary>
    private void DrawInputFields()
    {
        // プレビュー中は入力できないようにする
        GUI.enabled = !isPreviewed;

        // 各種入力フィールドを表示
        spreadsheetId = EditorGUILayout.TextField("Spreadsheet ID", spreadsheetId);
        sheetName = EditorGUILayout.TextField("Sheet Name", sheetName);
        startCell = EditorGUILayout.TextField("Start Cell (例:A3)", startCell);
        endCell = EditorGUILayout.TextField("End Cell (例:D10)", endCell);
        savePath = EditorGUILayout.TextField("Save Path", savePath);

        // GUIの状態を戻す
        GUI.enabled = true;
    }

    /// <summary>
    /// プレビューの表示を行う
    /// </summary>
    private void OpenPreview()
    {
        // プレビューボタンが押されたときの処理
        if (GUILayout.Button("Preview"))
        {
            // 入力内容の検証
            if (!ValidateInput()) return;

            // スプレッドシートのURLを生成
            string url = GetSpreadSheetUrl();
            if(url == null) return;

            // スプレッドシートの内容をJSON形式で取得
            string jsonText = GetJsonData(url);
            if (jsonText == null) return;

            // JSONデータをScriptableObject用のデータ形式に変換
            previewData = ParseSpreadSheetDataToJson(jsonText);

            // プレビュー初期化（各カラムの型推測など）
            InitRowType();

            // プレビュー中フラグを立てる
            isPreviewed = true;
        }
    }

    /// <summary>
    /// 必須事項に入力があるかどうか
    /// </summary>
    /// <returns>入力あるならtrue</returns>
    private bool ValidateInput()
    {
        // 無しならエラー
        if (string.IsNullOrEmpty(spreadsheetId) || string.IsNullOrEmpty(sheetName) ||
            string.IsNullOrEmpty(savePath))
        {
            Debug.LogError("SpreadsheetIDまたはSheetNameまたはSavePathが空です");
            return false;
        }
        return true;
    }

    /// <summary>
    /// URLを作成して取得する
    /// </summary>
    /// <returns>URL</returns>
    private string GetSpreadSheetUrl()
    {
        string baseUrl = $"https://opensheet-seisei-custom.vercel.app/api/{spreadsheetId}/{sheetName}";

        // 両方未入力ならそのまま
        if (string.IsNullOrEmpty(startCell) && string.IsNullOrEmpty(endCell))
        {
            return baseUrl;
        }

        // どちらかだけ入力ならエラー
        if (string.IsNullOrEmpty(startCell) || string.IsNullOrEmpty(endCell))
        {
            Debug.LogError("開始セルと終了セルは両方入力する必要があります");
            return null;
        }

        // 両方あるならrangeクエリを付与
        return $"{baseUrl}?range={startCell}:{endCell}";
    }

    /// <summary>
    /// jsonデータをテキストベースで取得する
    /// </summary>
    /// <param name="url">URL</param>
    /// <returns>テキストベースのjsonデータ</returns>
    private string GetJsonData(string url)
    {
        // 指定URLにGETリクエストを送信
        UnityWebRequest www = UnityWebRequest.Get(url);

        // タイムアウトを設定
        www.timeout = timeOut;
        www.SendWebRequest();

        // 完了まで待機
        while (!www.isDone) { }

        // 失敗時はエラーログを出してnullを返す
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("データ取得失敗:" + www.error);
            return null;
        }

        // 成功時はテキストデータを返す
        return www.downloadHandler.text;
    }

    /// <summary>
    /// jsonをデシリアライズして使えるデータ形式にパース
    /// </summary>
    /// <param name="jsonText">テキストベースのjson</param>
    /// <returns>読み込んだ全体のデータ</returns>
    private SpreadSheetData ParseSpreadSheetDataToJson(string jsonText)
    {
        // JSON文字列をパースしてオブジェクトのリストに変換
        var rowsList = Json.Deserialize(jsonText) as List<object>;
        // パース結果が期待形式でなければエラー出力し終了
        if (rowsList == null)
        {
            Debug.LogError("JSONが配列形式ではありませんでした");
            return null;
        }

        // ScriptableObject用のデータオブジェクトを作成
        var dataAsset = ScriptableObject.CreateInstance<SpreadSheetData>();
        dataAsset.rows = new List<RowData>(); 

        // 各JSON行オブジェクトについて処理
        foreach (var rowObj in rowsList)
        {
            // 行オブジェクトがDictionary形式でなければスキップ
            if (rowObj is not Dictionary<string, object> rowDict) continue;

            // 新しい行データオブジェクトを生成
            var rowData = new RowData { pairs = new List<MultiValuePair>() };

            // 各カラムペアごとに処理
            foreach (var pair in rowDict)
            {
                // MultiValuePairオブジェクトを作成し、キーをセット
                var mv = new MultiValuePair { key = pair.Key };

                // 値を文字列化。nullのときは空文字を入れる
                string valueStr = pair.Value?.ToString() ?? "";
                // 生の文字列値を保持
                mv.rawValue = valueStr;  

                // 文字列内容から型を推測し、対応するフィールドに値を格納
                if (int.TryParse(valueStr, out int i))
                {
                    mv.type = MultiValueType.Int;
                    mv.intValue = i;
                }
                else if (float.TryParse(valueStr, out float f))
                {
                    mv.type = MultiValueType.Float;
                    mv.floatValue = f;
                }
                else if (bool.TryParse(valueStr, out bool b))
                {
                    mv.type = MultiValueType.Bool;
                    mv.boolValue = b;
                }
                else
                {
                    mv.type = MultiValueType.String;
                    mv.stringValue = valueStr;
                }

                // このペアを行データに追加
                rowData.pairs.Add(mv);
            }

            // 完了した行データをScriptableObject用に追加
            dataAsset.rows.Add(rowData);
        }

        // すべての行処理が終わったら返却
        return dataAsset;
    }

    /// <summary>
    /// 読み込み状態を初期化してプレビューをリセットする
    /// </summary>
    private void ResetData()
    {
        previewData = null;
        isPreviewed = false;
    }

    /// <summary>
    /// 指定パスにフォルダがなければ再帰的に作成する
    /// </summary>
    /// <param name="folderPath">作成対象のパス</param>
    private void CreateFolder(string folderPath)
    {
        if (AssetDatabase.IsValidFolder(folderPath)) return;

        // どのディレクトリにあるかをみてそれを親のパスとする
        string parent = System.IO.Path.GetDirectoryName(folderPath);
        // パスから名前だけ抽出
        string newFolderName = System.IO.Path.GetFileName(folderPath);

        if (!AssetDatabase.IsValidFolder(parent))
        {
            // 親フォルダも作成
            CreateFolder(parent);
        }

        AssetDatabase.CreateFolder(parent, newFolderName);
    }

    /// <summary>
    /// 保存と取り消しの操作を描画
    /// </summary>
    private void SaveOrReset()
    {
        GUILayout.BeginHorizontal();

        bool canSave = true;
        foreach (var type in rowTypeDict.Values)
        {
            if (type == MultiValueType.Enum)
            {
                // 正しくないEnumがあれば保存できないようにする
                canSave = ValidateEnumTypes();
            }
        }

        GUI.enabled = canSave;

        if (GUILayout.Button("Save"))
        {
            SaveAsAsset();
        }

        GUI.enabled = true;

        if (GUILayout.Button("Reset"))
        {
            ResetData();
        }

        GUILayout.EndHorizontal();

        if (!canSave)
        {
            EditorGUILayout.HelpBox("Invalid enum type name.Please enter a valid type name.", MessageType.Error);
        }
    }

    /// <summary>
    /// Enum型の型名が正しいかどうかを検証する
    /// </summary>
    /// <returns>すべて有効ならtrue</returns>
    private bool ValidateEnumTypes()
    {
        foreach (var kvp in enumTypeNameDict)
        {
            string typeName = kvp.Value;
            if (string.IsNullOrEmpty(typeName))
                return false;

            var type = Type.GetType(typeName);
            if (type == null)
            {
                // Unityアセンブリからも探す
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    type = asm.GetType(typeName);
                    if (type != null) break;
                }
            }

            if (type == null || !type.IsEnum)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// ScriptableObjectとしてデータを保存する
    /// </summary>
    private void SaveAsAsset()
    {
        // 保存先フォルダのパスを作る
        string folderPath = $"Assets/{savePath}";

        // フォルダがなければ作成
        CreateFolder(folderPath);

        // 保存パス（ファイル名含む）
        string assetPath = $"{folderPath}/{sheetName}DataBase.asset";

        AssetDatabase.CreateAsset(previewData, assetPath);
        AssetDatabase.SaveAssets();

        ResetData();

        Debug.Log($"ScriptableObjectを作成しました: {assetPath}");
    }

    /// <summary>
    /// 行ごとの型設定を初期化する
    /// </summary>
    private void InitRowType()
    {
        rowTypeDict.Clear();
        foreach (var row in previewData.rows)
        {
            foreach (var pair in row.pairs)
            {
                if (!rowTypeDict.ContainsKey(pair.key))
                {
                    rowTypeDict.Add(pair.key, pair.type);
                }
            }
        }
    }

    /// <summary>
    /// Enum型の型名入力UIを描画
    /// </summary>
    private void DrawEnumTypeUI()
    {
        bool enumEnabled = false;
        foreach (var key in rowTypeDict.Keys)
        {
            // Enumにしているのがあれば有効にする
            if (rowTypeDict[key] == MultiValueType.Enum)
            {
                enumEnabled = true;
            }
        }
        if (!enumEnabled) return;

        EditorGUILayout.LabelField("Input Enum Type", EditorStyles.boldLabel);

        foreach (var key in rowTypeDict.Keys)
        {
            // 型がEnumのフィールド用にだけ表示
            if (rowTypeDict[key] == MultiValueType.Enum)
            {
                string current = enumTypeNameDict.ContainsKey(key) ? enumTypeNameDict[key] : "";
                string newType = EditorGUILayout.TextField($"{key}", current);
                enumTypeNameDict[key] = newType;
            }
        }
    }

    /// <summary>
    /// 行ごとの型設定UIを描画
    /// </summary>
    private void DrawRowTypeUI()
    {
        EditorGUILayout.LabelField("Type Settings per Field", EditorStyles.boldLabel);

        // 辞書のキー一覧を取得してループ
        List<string> keys = new List<string>(rowTypeDict.Keys);
        bool changed = false;
        foreach (var key in keys)
        {
            MultiValueType selectedType = (MultiValueType)EditorGUILayout.EnumPopup(key, rowTypeDict[key]);
            if (selectedType != rowTypeDict[key])
            {
                rowTypeDict[key] = selectedType;
                changed = true;
            }
        }
        // 型指定に変更があった時だけ起動
        if (changed)
        {
            ApplyRowType();
        }
    }

    /// <summary>
    /// 型の内容を反映し、各値を再キャストする
    /// </summary>
    private void ApplyRowType()
    {
        // 各行データに対してループ
        for (int rowIndex = 0; rowIndex < previewData.rows.Count; rowIndex++)
        {
            // 現在処理している行を取得
            var row = previewData.rows[rowIndex];  

            // 各行内の全ての行データに対してループ
            for (int pairIndex = 0; pairIndex < row.pairs.Count; pairIndex++)
            {
                // 対象のキーと値のペアを取得
                var pair = row.pairs[pairIndex];  

                // 該当の行に型の設定が存在するか確認
                if (rowTypeDict.TryGetValue(pair.key, out MultiValueType newType))
                {
                    // ペアの型情報を変更された型に変更
                    pair.type = newType;

                    // 値の文字列を取得。nullの場合は空文字とする
                    string raw = pair.rawValue ?? "";

                    // 選択された型に応じて、対応するフィールドへ型変換して代入
                    switch (newType)
                    {
                        // Int型に変換
                        case MultiValueType.Int:
                            pair.intValue = int.TryParse(raw, out int i) ? i : 0;
                            break;

                        // Float型に変換
                        case MultiValueType.Float:
                            pair.floatValue = float.TryParse(raw, out float f) ? f : 0f;
                            break;

                        // Bool型に変換
                        case MultiValueType.Bool:
                            pair.boolValue = bool.TryParse(raw, out bool b) && b;
                            break;

                        // String型としてそのまま代入
                        case MultiValueType.String:
                            pair.stringValue = raw;
                            break;

                        // Enum型として文字列を設定し、対応する型名も保存
                        case MultiValueType.Enum:
                            pair.enumValue = raw;

                            // enumTypeNameDictから型名を取得できる場合は設定
                            if (enumTypeNameDict.TryGetValue(pair.key, out string typeName))
                            {
                                pair.enumTypeName = typeName;
                            }
                            break;
                    }

                    // 型変換、代入が終わったペアを元のリストに再設定
                    row.pairs[pairIndex] = pair;
                }
            }
        }
        // Enum型でなくなったキーをenumTypeNameDictから除去
        var keysToRemove = enumTypeNameDict.Keys.Where(k => !rowTypeDict.ContainsKey(k) || rowTypeDict[k] != MultiValueType.Enum).ToList();
        foreach (var key in keysToRemove)
        {
            enumTypeNameDict.Remove(key);
        }
    }

    /// <summary>
    /// スプレッドシートのプレビューを表示する
    /// </summary>
    private void DrawPreview()
    {
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);

        // スクロールビュー開始
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        for (int i = 0; i < previewData.rows.Count; i++)
        {
            var row = previewData.rows[i];
            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.LabelField($"row {i + 1}");

            foreach (var pair in row.pairs)
            {
                EditorGUILayout.LabelField($"{pair.key} ({pair.type})", pair.GetValue()?.ToString() ?? "null");
            }

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndScrollView();
    }
}

/// <summary>
/// ScriptableObjectのInspectorを読み取り専用にするカスタムエディタ
/// </summary>
[CustomEditor(typeof(SpreadSheetData))]
public class SpreadSheetDataEditor : Editor
{    
    /// <summary>
    /// GUIを無効化して読み取り専用にする
    /// </summary>
    public override void OnInspectorGUI()
    {
        GUI.enabled = false;
        base.OnInspectorGUI();
    }
}

/// <summary>
/// MultiValuePair のカスタム表示（型に応じた表示を行う）
/// </summary>
[CustomPropertyDrawer(typeof(MultiValuePair))]
public class TypedKeyValuePairDrawer : PropertyDrawer
{
    /// <summary>
    /// PropertyDrawerの描画処理
    /// </summary>
    /// <param name="position">描画領域</param>
    /// <param name="property">対象プロパティ</param>
    /// <param name="label">ラベル</param>
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var keyProp = property.FindPropertyRelative("key");
        var typeProp = property.FindPropertyRelative("type");

        var stringVal = property.FindPropertyRelative("stringValue");
        var intVal = property.FindPropertyRelative("intValue");
        var floatVal = property.FindPropertyRelative("floatValue");
        var boolVal = property.FindPropertyRelative("boolValue");

        float fieldWidth = position.width / 3f;

        Rect keyRect = new(position.x, position.y, fieldWidth, position.height);
        Rect typeRect = new(position.x + fieldWidth + 5, position.y, fieldWidth, position.height);
        Rect valueRect = new(position.x + 2 * fieldWidth + 10, position.y, fieldWidth, position.height);

        EditorGUI.PropertyField(keyRect, keyProp, GUIContent.none);
        EditorGUI.PropertyField(typeRect, typeProp, GUIContent.none);

        switch ((MultiValueType)typeProp.enumValueIndex)
        {
            case MultiValueType.String:
                EditorGUI.PropertyField(valueRect, stringVal, GUIContent.none);
                break;
            case MultiValueType.Int:
                EditorGUI.PropertyField(valueRect, intVal, GUIContent.none);
                break;
            case MultiValueType.Float:
                EditorGUI.PropertyField(valueRect, floatVal, GUIContent.none);
                break;
            case MultiValueType.Bool:
                EditorGUI.PropertyField(valueRect, boolVal, GUIContent.none);
                break;
            case MultiValueType.Enum:
                EditorGUI.PropertyField(valueRect, stringVal, GUIContent.none);
                break;
        }
    }
}