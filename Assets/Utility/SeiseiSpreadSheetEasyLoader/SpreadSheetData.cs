using System.Collections.Generic;
using System;
using UnityEngine;

namespace SeiseiUtilyty
{
    /// <summary>
    /// �l�̌^�̎��
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
    /// �X�v���b�h�V�[�g�̃f�[�^�S�̂��i�[����  
    /// </summary>  
    public class SpreadSheetData : ScriptableObject
    {
        public List<RowData> rows;

        /// <summary>
        /// �w��̃L�[�ƒl�Ɉ�v����s�����X�g�ŕԂ�
        /// </summary>
        /// <param name="key">�L�[</param>
        /// <param name="value">���S��v�̒l</param>
        /// <returns>�s�f�[�^�̃��X�g</returns>
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
    /// �X�v���b�h�V�[�g��1�s�ɑ�������f�[�^  
    /// </summary>  
    [Serializable]
    public struct RowData
    {
        public List<MultiValuePair> pairs;

        /// <summary>  
        /// �w�肳�ꂽ�L�[�Ɉ�v����MultiValuePair���擾����  
        /// </summary>  
        public readonly MultiValuePair? GetPair(string key)
        {
            var found = pairs.Find(pair => pair.key == key);
            return string.IsNullOrEmpty(found.key) ? null : found;
        }

        /// <summary>  
        /// �w�肳�ꂽ�L�[�Ɉ�v����l�� object �Ƃ��Ď擾����  
        /// </summary>  
        public readonly T GetValue<T>(string key)
        {
            var pair = pairs.Find(p => p.key == key);
            if (string.IsNullOrEmpty(pair.key))
                return default;

            object value = pair.GetValue();

            // Enum�^�̏ꍇ�͓��ʈ���
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
                    Debug.LogError($"Enum�ɕϊ��ł��܂���ł���: {typeof(T)} {value}");
                    return default;
                }
            }

            // �ʏ�̌^�͕��ʂɕϊ�
            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                Debug.LogError($"�l�^�ɕϊ��ł��܂���ł���: {typeof(T)} {value}");
                return default;
            }
        }

    }

    /// <summary>  
    /// �L�[�A�^�A�^�ʂ̒l������  
    /// </summary>  
    [Serializable]
    public struct MultiValuePair
    {
        /// <summary>  
        /// �L�[  
        /// </summary>  
        public string key;
        /// <summary>  
        /// �l�̌^  
        /// </summary>  
        public MultiValueType type;
        /// <summary>
        /// ���̒l
        /// </summary>
        public string rawValue;

        /// <summary>  
        /// ������^�̒l  
        /// </summary>  
        public string stringValue;
        /// <summary>  
        /// �����^�̃f�[�^  
        /// </summary>  
        public int intValue;
        /// <summary>  
        /// �����^�̃f�[�^  
        /// </summary>  
        public float floatValue;
        /// <summary>  
        /// �_���^�̃f�[�^  
        /// </summary>  
        public bool boolValue;
        /// <summary>
        /// Enum�^�̃f�[�^
        /// </summary>
        public string enumValue;
        public string enumTypeName;

        /// <summary>  
        /// ���݂̒l�̌^�̒l��Ԃ�  
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
        /// Enum�^��Type�擾
        /// </summary>
        public Type GetEnumType()
        {
            if (string.IsNullOrEmpty(enumTypeName))
                return null;

            var type = Type.GetType(enumTypeName);
            if (type != null) return type;

            // �t���l�[�������̏ꍇ�̓A�Z���u������T���iUnity�p�̒ǉ��ی��j
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = asm.GetType(enumTypeName);
                if (type != null) return type;
            }

            return null;
        }

        /// <summary>
        /// Enum�l���^�ϊ����ĕԂ�
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
                Debug.LogError($"Enum�^�ɕϊ��ł��܂���ł���: {enumType} {enumValue}");
                return enumValue;
            }
        }
    }
}
