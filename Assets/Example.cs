using UnityEngine;

public enum EnemyType
{
    Goblin,
    Orc,
    Dragon
}

public class Example : MonoBehaviour
{
    [SerializeField]
    private SeiseiUtilyty.SpreadSheetData datas;

    void Start()
    {
        // �Ή�����L�[��MultiValuePair�\���̂��̂��̂��󂯎��
        foreach(var row in datas.rows)
        {
            var pair = row.GetPair("EnemyType");

            if (pair.HasValue)
            {
                Debug.Log($"{pair.Value.key}:{pair.Value.GetValue()}");
            }
        }

        // �����ɍ��v����s��T�����@1
        foreach (var row in datas.rows)
        {
            var type = row.GetValue<EnemyType>("EnemyType");

            if (type == EnemyType.Goblin)
            {
                var name = row.GetValue<string>("Name");
                Debug.Log($"�S�u�����̖��O�� {name}");
            }
        }

        // �����ɍ��v����s��T�����@2
        var rows = datas.FindRowsByKeyValue("EnemyType",EnemyType.Dragon);

        foreach (var row in rows)
        {
            var power = row.GetValue<int>("Power");
            Debug.Log($"Dragon��Power�� {power}");
        }
    }
}
