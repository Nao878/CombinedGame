using UnityEngine;

namespace ZombieSurvival
{
    /// <summary>
    /// アイテムの種類
    /// </summary>
    public enum ItemType
    {
        Material,  // 漢字パーツ（素材）
        Weapon     // 漢字武器
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

        /// <summary>
        /// フィールド上で表示する文字（漢字1文字）
        /// </summary>
        public string displayCharacter;

        public ItemData(string name, ItemType type, string desc, Color color, string character = "")
        {
            itemName = name;
            itemType = type;
            description = desc;
            displayColor = color;
            displayCharacter = string.IsNullOrEmpty(character) ? name : character;
        }
    }
}
