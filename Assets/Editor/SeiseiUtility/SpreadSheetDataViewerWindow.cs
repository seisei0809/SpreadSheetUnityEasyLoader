using SeiseiUtilyty;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// ScriptableObjectのデータ確認用ウィンドウ
/// </summary>
public class SpreadSheetDataViewerWindow : EditorWindow
{
    /// <summary>
    /// 表示するデータ
    /// </summary>
    private SpreadSheetData targetData;

    /// <summary>
    /// スクロール位置
    /// </summary>
    private Vector2 scrollPosition;

    /// <summary>
    /// ソート対象のキー
    /// </summary>
    private string sortKey = "None";

    /// <summary>
    /// ソートの昇順・降順
    /// </summary>
    private bool sortAscending = true;

    /// <summary>
    /// 検索対象のキー
    /// </summary>
    private string searchKey = "None";

    /// <summary>
    /// 文字列検索クエリ
    /// </summary>
    private string searchQuery = "";

    /// <summary>
    /// 数値検索クエリ
    /// </summary>
    private string numericQuery = "";

    /// <summary>
    /// 数値検索時の比較条件
    /// </summary>
    private NumericComparison numericComparison = NumericComparison.Equal;

    /// <summary>
    /// 比較演算子の列挙
    /// </summary>
    private enum NumericComparison
    {
        Equal,        // ==
        Greater,      // >
        GreaterEqual, // >=
        Less,         // <
        LessEqual     // <=
    }

    [MenuItem("Tools/SpreadSheet Data Viewer")]
    public static void ShowWindow()
    {
        var window = GetWindow<SpreadSheetDataViewerWindow>("SpreadSheet Data Viewer");
        window.minSize = new Vector2(400, 500);
    }

    private void OnGUI()
    {
        DrawTargetDataField();

        if (targetData == null)
        {
            EditorGUILayout.HelpBox("Please assign a data asset.", MessageType.Info);
            return;
        }

        DrawSearchControls();
        DrawSortControls();
        DrawDataList();
    }

    private void DrawTargetDataField()
    {
        targetData = EditorGUILayout.ObjectField("Data", targetData, typeof(SpreadSheetData), false) as SpreadSheetData;
        EditorGUILayout.Space();
    }

    private void DrawSearchControls()
    {
        var allKeys = GetAllKeys().ToList();
        allKeys.Insert(0, "None");

        EditorGUILayout.LabelField("Search", EditorStyles.boldLabel);

        int selectedIndex = allKeys.IndexOf(searchKey);
        int newSelectedIndex = EditorGUILayout.Popup("Key", selectedIndex, allKeys.ToArray());

        if (newSelectedIndex != selectedIndex)
        {
            searchKey = allKeys[newSelectedIndex];
        }

        if (searchKey != "None")
        {
            var samplePair = targetData.rows
                .SelectMany(row => row.pairs)
                .FirstOrDefault(p => p.key == searchKey);

            var typeStr = samplePair.type;

            if (typeStr == MultiValueType.Int || typeStr == MultiValueType.Float)
            {
                numericComparison = (NumericComparison)EditorGUILayout.EnumPopup("Condition", numericComparison);
                numericQuery = EditorGUILayout.TextField("Value", numericQuery);
            }
            else
            {
                searchQuery = EditorGUILayout.TextField("SearchWord", searchQuery);
            }
        }

        EditorGUILayout.Space();
    }

    private void DrawSortControls()
    {
        var allKeys = GetAllKeys().ToList();
        allKeys.Insert(0, "None");

        EditorGUILayout.LabelField("Sort", EditorStyles.boldLabel);

        int selectedIndex = allKeys.IndexOf(sortKey);
        int newSelectedIndex = EditorGUILayout.Popup("Key", selectedIndex, allKeys.ToArray());

        if (newSelectedIndex != selectedIndex)
        {
            sortKey = allKeys[newSelectedIndex];
        }

        if (sortKey != "None")
        {
            if (GUILayout.Button(sortAscending ? "Ascending" : "Descending"))
            {
                sortAscending = !sortAscending;
            }
        }

        EditorGUILayout.Space();
    }

    private void DrawDataList()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        var filteredRows = GetFilteredRows();

        if (sortKey != "None")
        {
            filteredRows = SortRows(filteredRows);
        }

        for (int i = 0; i < filteredRows.Count; i++)
        {
            var (index, row) = filteredRows[i];

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField($"row {index + 1}");

            foreach (var pair in row.pairs)
            {
                EditorGUILayout.LabelField($"{pair.key} ({pair.type})", pair.GetValue()?.ToString() ?? "null");
            }

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndScrollView();
    }

    private HashSet<string> GetAllKeys()
    {
        var allKeys = new HashSet<string>();
        foreach (var row in targetData.rows)
        {
            foreach (var pair in row.pairs)
            {
                allKeys.Add(pair.key);
            }
        }
        return allKeys;
    }

    private List<(int index, RowData row)> GetFilteredRows()
    {
        var result = new List<(int, RowData)>();

        for (int i = 0; i < targetData.rows.Count; i++)
        {
            var row = targetData.rows[i];

            if (searchKey != "None")
            {
                var pair = row.pairs.FirstOrDefault(p => p.key == searchKey);
                var value = pair.GetValue();

                if (pair.type == MultiValueType.Int || pair.type == MultiValueType.Float)
                {
                    if (double.TryParse(numericQuery, out double queryValue) &&
                        double.TryParse(value?.ToString(), out double fieldValue))
                    {
                        bool match = numericComparison switch
                        {
                            NumericComparison.Equal => fieldValue == queryValue,
                            NumericComparison.Greater => fieldValue > queryValue,
                            NumericComparison.GreaterEqual => fieldValue >= queryValue,
                            NumericComparison.Less => fieldValue < queryValue,
                            NumericComparison.LessEqual => fieldValue <= queryValue,
                            _ => false
                        };

                        if (!match) continue;
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    var valueStr = value?.ToString() ?? "";
                    if (!valueStr.Contains(searchQuery))
                        continue;
                }
            }

            result.Add((i, row));
        }

        return result;
    }

    private List<(int index, RowData row)> SortRows(List<(int index, RowData row)> rows)
    {
        var sorted = rows.OrderBy<(int index, RowData row), object>(row =>
        {
            var pair = row.row.pairs.FirstOrDefault(p => p.key == sortKey);

            var value = pair.GetValue();
            if (value == null)
                return null;

            if (value is IConvertible)
            {
                double result;
                if (double.TryParse(value.ToString(), out result))
                    return result;
            }

            return value.ToString();

        }, new ObjectNaturalComparer()).ToList();

        if (!sortAscending)
            sorted.Reverse();

        return sorted;
    }

    public class ObjectNaturalComparer : IComparer<object>
    {
        public int Compare(object x, object y)
        {
            return EditorUtility.NaturalCompare(x?.ToString(), y?.ToString());
        }
    }
}
