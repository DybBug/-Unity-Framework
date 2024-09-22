using System;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildingTable", menuName = "Table/Building")]
public class BuildingTable : Table<BuildingTable.Item>
{
    [Serializable]
    public class Item : TableItem
    {
        public string Key;
        public Sprite Sprite;
        public GameObject Prefab;
        public string Name;
        public string Description;
        public long TakeTimeSec;

        public override string GetKey() => Key;
    }

}
