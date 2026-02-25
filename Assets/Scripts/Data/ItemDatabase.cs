using System.Collections.Generic;
using UnityEngine;

namespace ZombieSurvival
{
    /// <summary>
    /// 全アイテムとクラフトレシピの静的データベース
    /// 漢字パーツを組み合わせて漢字武器を生成する
    /// </summary>
    public static class ItemDatabase
    {
        // ===== 漢字パーツ（素材）=====
        public static readonly ItemData Kin = new ItemData(
            "金", ItemType.Material, "金属を表す偏。武器の素材になる。",
            new Color(1f, 0.84f, 0f), "金"
        );

        public static readonly ItemData Juu = new ItemData(
            "充", ItemType.Material, "満ちるの意。エネルギーを秘めている。",
            new Color(0.6f, 0.8f, 1f), "充"
        );

        public static readonly ItemData Ki = new ItemData(
            "木", ItemType.Material, "木偏。自然の力が宿る。",
            new Color(0.5f, 0.8f, 0.3f), "木"
        );

        public static readonly ItemData Han = new ItemData(
            "反", ItemType.Material, "反転の意。力を跳ね返す。",
            new Color(0.9f, 0.5f, 0.3f), "反"
        );

        public static readonly ItemData Hi = new ItemData(
            "火", ItemType.Material, "炎を表す偏。破壊力を秘める。",
            new Color(1f, 0.3f, 0.2f), "火"
        );

        public static readonly ItemData Yaku = new ItemData(
            "薬", ItemType.Material, "薬の字そのもの。回復の力。",
            new Color(0.4f, 1f, 0.6f), "薬"
        );

        public static readonly ItemData Tou = new ItemData(
            "刀", ItemType.Material, "刃を表す偏。斬撃の源。",
            new Color(0.8f, 0.8f, 0.9f), "刀"
        );

        public static readonly ItemData Katana = new ItemData(
            "刃", ItemType.Material, "鋭い刃。切れ味を増す。",
            new Color(0.7f, 0.75f, 0.85f), "刃"
        );

        // ===== 漢字武器（成果物）=====
        public static readonly ItemData Juu_Weapon = new ItemData(
            "銃", ItemType.Weapon, "金＋充 — 遠距離射撃武器。",
            new Color(1f, 0.9f, 0.2f), "銃"
        );

        public static readonly ItemData Ita = new ItemData(
            "板", ItemType.Weapon, "木＋反 — 防壁。身を守る盾。",
            new Color(0.6f, 0.45f, 0.25f), "板"
        );

        public static readonly ItemData Katana_Weapon = new ItemData(
            "剣", ItemType.Weapon, "金＋刃 — 近接斬撃武器。",
            new Color(0.75f, 0.85f, 1f), "剣"
        );

        public static readonly ItemData Bakudan = new ItemData(
            "爆", ItemType.Weapon, "火＋薬 — 範囲攻撃。",
            new Color(1f, 0.5f, 0.1f), "爆"
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
            Kin, Juu, Ki, Han, Hi, Yaku, Tou, Katana,
            Juu_Weapon, Ita, Katana_Weapon, Bakudan
        };

        // ===== 漢字合成レシピ =====
        public static readonly List<CraftRecipe> Recipes = new List<CraftRecipe>
        {
            new CraftRecipe("金", 1, "充", 1, "銃"),
            new CraftRecipe("木", 1, "反", 1, "板"),
            new CraftRecipe("金", 1, "刃", 1, "剣"),
            new CraftRecipe("火", 1, "薬", 1, "爆"),
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
