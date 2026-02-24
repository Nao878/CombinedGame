using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZombieSurvival
{
    /// <summary>
    /// クラフト（合成）システム
    /// Cキーでクラフトを試行し、レシピに合致すれば合成を実行
    /// </summary>
    public class CraftingManager : MonoBehaviour
    {
        public static CraftingManager Instance { get; private set; }

        /// <summary>
        /// クラフト成功時に発火（生成されたアイテム名を通知）
        /// </summary>
        public event Action<string> OnCraftSuccess;

        /// <summary>
        /// クラフト失敗時に発火（メッセージを通知）
        /// </summary>
        public event Action<string> OnCraftFailed;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Update()
        {
            // Cキーでクラフト試行
            if (Input.GetKeyDown(KeyCode.C))
            {
                TryCraft();
            }
        }

        /// <summary>
        /// 全レシピをチェックし、作成可能な最初のレシピで合成を実行
        /// </summary>
        public void TryCraft()
        {
            if (InventoryManager.Instance == null)
            {
                OnCraftFailed?.Invoke("インベントリが見つかりません");
                return;
            }

            var inventory = InventoryManager.Instance;

            foreach (var recipe in ItemDatabase.Recipes)
            {
                if (inventory.HasItem(recipe.material1, recipe.amount1) &&
                    inventory.HasItem(recipe.material2, recipe.amount2))
                {
                    // 素材を消費
                    inventory.RemoveItem(recipe.material1, recipe.amount1);
                    inventory.RemoveItem(recipe.material2, recipe.amount2);

                    // 成果物を追加
                    inventory.AddItem(recipe.result);

                    string msg = $"{recipe.material1}x{recipe.amount1} + {recipe.material2}x{recipe.amount2} → {recipe.result} を合成！";
                    Debug.Log($"[Crafting] {msg}");
                    OnCraftSuccess?.Invoke(msg);
                    return;
                }
            }

            // 合成可能なレシピなし
            string failMsg = "合成できるレシピがありません。素材を集めましょう。";
            Debug.Log($"[Crafting] {failMsg}");
            OnCraftFailed?.Invoke(failMsg);
        }

        /// <summary>
        /// 利用可能なレシピ一覧を取得
        /// </summary>
        public List<string> GetAvailableRecipes()
        {
            var result = new List<string>();
            foreach (var recipe in ItemDatabase.Recipes)
            {
                result.Add($"{recipe.material1}x{recipe.amount1} + {recipe.material2}x{recipe.amount2} → {recipe.result}");
            }
            return result;
        }
    }
}
