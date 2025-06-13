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
        // 対応するキーのMultiValuePair構造体そのものを受け取る
        foreach(var row in datas.rows)
        {
            var pair = row.GetPair("EnemyType");

            if (pair.HasValue)
            {
                Debug.Log($"{pair.Value.key}:{pair.Value.GetValue()}");
            }
        }

        // 条件に合致する行を探す方法1
        foreach (var row in datas.rows)
        {
            var type = row.GetValue<EnemyType>("EnemyType");

            if (type == EnemyType.Goblin)
            {
                var name = row.GetValue<string>("Name");
                Debug.Log($"ゴブリンの名前は {name}");
            }
        }

        // 条件に合致する行を探す方法2
        var rows = datas.FindRowsByKeyValue("EnemyType",EnemyType.Dragon);

        foreach (var row in rows)
        {
            var power = row.GetValue<int>("Power");
            Debug.Log($"DragonのPowerは {power}");
        }
    }
}
