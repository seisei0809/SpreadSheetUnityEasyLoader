using System.Collections.Generic;
using System;
using UnityEngine;

namespace SeiseiUtilyty
{
    /// <summary>
    /// 値の型の種類
    /// </summary>
    public enum MultiValueType
    {
        String,
        Int,
        Float,
        Bool,
        Enum
    }

    /// <summary>  
    /// スプレッドシートのデータ全体を格納する  
    /// </summary>  
    public class SpreadSheetData : ScriptableObject
    {
        public List<RowData> rows;

        /// <summary>
        /// 指定のキーと値に一致する行をリストで返す
        /// </summary>
        /// <param name="key">キー</param>
        /// <param name="value">完全一致の値</param>
        /// <returns>行データのリスト</returns>
        public List<RowData> FindRowsByKeyValue(string key, object value)
        {
            var result = new List<RowData>();

            foreach (var row in rows)
            {
                var pair = row.pairs.Find(p => p.key == key);
                if (!string.IsNullOrEmpty(pair.key) && pair.GetValue()?.ToString() == value.ToString())
                {
                    result.Add(row);
                }
            }

            return result;
        }
        public List<RowData> FindRowsByKeyValue<TEnum>(string key, TEnum enumValue) where TEnum : Enum
        {
            return FindRowsByKeyValue(key, enumValue.ToString());
        }
    }

    /// <summary>  
    /// スプレッドシートの1行に相当するデータ  
    /// </summary>  
    [Serializable]
    public struct RowData
    {
        public List<MultiValuePair> pairs;

        /// <summary>  
        /// 指定されたキーに一致するMultiValuePairを取得する  
        /// </summary>  
        public readonly MultiValuePair? GetPair(string key)
        {
            var found = pairs.Find(pair => pair.key == key);
            return string.IsNullOrEmpty(found.key) ? null : found;
        }

        /// <summary>  
        /// 指定されたキーに一致する値を object として取得する  
        /// </summary>  
        public readonly T GetValue<T>(string key)
        {
            var pair = pairs.Find(p => p.key == key);
            if (string.IsNullOrEmpty(pair.key))
                return default;

            object value = pair.GetValue();

            // Enum型の場合は特別扱い
            if (typeof(T).IsEnum)
            {
                if (value is T enumValue)
                    return enumValue;

                try
                {
                    return (T)Enum.Parse(typeof(T), value.ToString());
                }
                catch
                {
                    Debug.LogError($"Enumに変換できませんでした: {typeof(T)} {value}");
                    return default;
                }
            }

            // 通常の型は普通に変換
            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                Debug.LogError($"値型に変換できませんでした: {typeof(T)} {value}");
                return default;
            }
        }

    }

    /// <summary>  
    /// キー、型、型別の値を持つ  
    /// </summary>  
    [Serializable]
    public struct MultiValuePair
    {
        /// <summary>  
        /// キー  
        /// </summary>  
        public string key;
        /// <summary>  
        /// 値の型  
        /// </summary>  
        public MultiValueType type;
        /// <summary>
        /// 生の値
        /// </summary>
        public string rawValue;

        /// <summary>  
        /// 文字列型の値  
        /// </summary>  
        public string stringValue;
        /// <summary>  
        /// 整数型のデータ  
        /// </summary>  
        public int intValue;
        /// <summary>  
        /// 実数型のデータ  
        /// </summary>  
        public float floatValue;
        /// <summary>  
        /// 論理型のデータ  
        /// </summary>  
        public bool boolValue;
        /// <summary>
        /// Enum型のデータ
        /// </summary>
        public string enumValue;
        public string enumTypeName;

        /// <summary>  
        /// 現在の値の型の値を返す  
        /// </summary>  
        /// <returns></returns>  
        public object GetValue()
        {
            return type switch
            {
                MultiValueType.String => stringValue,
                MultiValueType.Int => intValue,
                MultiValueType.Float => floatValue,
                MultiValueType.Bool => boolValue,
                MultiValueType.Enum => GetEnumValue(),
                _ => null
            };
        }

        /// <summary>
        /// Enum型のType取得
        /// </summary>
        public Type GetEnumType()
        {
            if (string.IsNullOrEmpty(enumTypeName))
                return null;

            var type = Type.GetType(enumTypeName);
            if (type != null) return type;

            // フルネームだけの場合はアセンブリから探す（Unity用の追加保険）
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = asm.GetType(enumTypeName);
                if (type != null) return type;
            }

            return null;
        }

        /// <summary>
        /// Enum値を型変換して返す
        /// </summary>
        public object GetEnumValue()
        {
            var enumType = GetEnumType();
            if (enumType == null) return enumValue;

            try
            {
                return Enum.Parse(enumType, enumValue);
            }
            catch
            {
                Debug.LogError($"Enum型に変換できませんでした: {enumType} {enumValue}");
                return enumValue;
            }
        }
    }
}
