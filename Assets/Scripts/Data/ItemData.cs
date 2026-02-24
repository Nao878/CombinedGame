using UnityEngine;

namespace ZombieSurvival
{
    /// <summary>
    /// アイテムの種類
    /// </summary>
    public enum ItemType
    {
        Material,  // 素材
        Weapon     // 武器
    }

    /// <summary>
    /// アイテムの基本データ定義
    /// </summary>
    [System.Serializable]
    public class ItemData
    {
        public string itemName;
        public ItemType itemType;
        public string description;
        public Color displayColor = Color.white;

        public ItemData(string name, ItemType type, string desc, Color color)
        {
            itemName = name;
            itemType = type;
            description = desc;
            displayColor = color;
        }
    }
}
