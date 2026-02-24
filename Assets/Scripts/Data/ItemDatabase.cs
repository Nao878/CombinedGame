using System.Collections.Generic;
using UnityEngine;

namespace ZombieSurvival
{
    /// <summary>
    /// 全アイテムとクラフトレシピの静的データベース
    /// プロトタイプ用にコード内で定義
    /// </summary>
    public static class ItemDatabase
    {
        // ===== アイテム定義 =====
        public static readonly ItemData ScrapMetal = new ItemData(
            "鉄くず", ItemType.Material, "錆びた金属片。何かに使えそうだ。",
            new Color(0.6f, 0.6f, 0.6f)
        );

        public static readonly ItemData Gunpowder = new ItemData(
            "火薬", ItemType.Material, "黒い粉末。取り扱い注意。",
            new Color(0.3f, 0.3f, 0.3f)
        );

        public static readonly ItemData Handgun = new ItemData(
            "ハンドガン", ItemType.Weapon, "基本的な拳銃。ゾンビに有効。",
            new Color(0.8f, 0.5f, 0.1f)
        );

        public static readonly ItemData Wood = new ItemData(
            "木材", ItemType.Material, "丈夫な木の板。",
            new Color(0.6f, 0.4f, 0.2f)
        );

        public static readonly ItemData Nail = new ItemData(
            "釘", ItemType.Material, "鋭い鉄の釘。",
            new Color(0.7f, 0.7f, 0.7f)
        );

        public static readonly ItemData NailBat = new ItemData(
            "釘バット", ItemType.Weapon, "釘を打ち付けたバット。近接武器。",
            new Color(0.5f, 0.3f, 0.1f)
        );

        /// <summary>
        /// 名前からアイテムデータを取得
        /// </summary>
        public static ItemData GetByName(string name)
        {
            foreach (var item in AllItems)
            {
                if (item.itemName == name) return item;
            }
            return null;
        }

        /// <summary>
        /// 全アイテム一覧
        /// </summary>
        public static readonly List<ItemData> AllItems = new List<ItemData>
        {
            ScrapMetal, Gunpowder, Handgun, Wood, Nail, NailBat
        };

        // ===== クラフトレシピ =====
        public static readonly List<CraftRecipe> Recipes = new List<CraftRecipe>
        {
            new CraftRecipe("鉄くず", 1, "火薬", 1, "ハンドガン"),
            new CraftRecipe("木材", 1, "釘", 1, "釘バット"),
        };
    }

    /// <summary>
    /// クラフトレシピ定義
    /// </summary>
    [System.Serializable]
    public class CraftRecipe
    {
        public string material1;
        public int amount1;
        public string material2;
        public int amount2;
        public string result;

        public CraftRecipe(string mat1, int amt1, string mat2, int amt2, string res)
        {
            material1 = mat1;
            amount1 = amt1;
            material2 = mat2;
            amount2 = amt2;
            result = res;
        }
    }
}
