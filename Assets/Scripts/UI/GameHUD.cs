using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ZombieSurvival
{
    /// <summary>
    /// ゲームのHUD表示
    /// インベントリ内容、操作説明、クラフト結果のフィードバックを表示
    /// </summary>
    public class GameHUD : MonoBehaviour
    {
        [Header("UI参照")]
        [SerializeField] private Text inventoryText;
        [SerializeField] private Text controlsText;
        [SerializeField] private Text messageText;
        [SerializeField] private Text recipeText;

        private float messageTimer = 0f;
        private float messageDuration = 3f;

        private void Start()
        {
            // 操作説明の設定
            if (controlsText != null)
            {
                controlsText.text =
                    "--- 操作方法 ---\n" +
                    "WASD : 移動\n" +
                    "マウス : 照準\n" +
                    "左クリック : 射撃\n" +
                    "C : 漢字合成";
            }

            // レシピ表示
            if (recipeText != null && CraftingManager.Instance != null)
            {
                var recipes = CraftingManager.Instance.GetAvailableRecipes();
                recipeText.text = "--- 合成レシピ ---\n" + string.Join("\n", recipes);
            }

            // イベント購読
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.OnInventoryChanged += UpdateInventoryDisplay;
                InventoryManager.Instance.OnItemPickedUp += OnItemPickedUp;
                UpdateInventoryDisplay();
            }

            if (CraftingManager.Instance != null)
            {
                CraftingManager.Instance.OnCraftSuccess += ShowMessage;
                CraftingManager.Instance.OnCraftFailed += ShowMessage;
            }
        }

        private void OnDestroy()
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.OnInventoryChanged -= UpdateInventoryDisplay;
                InventoryManager.Instance.OnItemPickedUp -= OnItemPickedUp;
            }

            if (CraftingManager.Instance != null)
            {
                CraftingManager.Instance.OnCraftSuccess -= ShowMessage;
                CraftingManager.Instance.OnCraftFailed -= ShowMessage;
            }
        }

        private void Update()
        {
            // メッセージのフェードアウト
            if (messageTimer > 0f)
            {
                messageTimer -= Time.deltaTime;
                if (messageTimer <= 0f && messageText != null)
                {
                    messageText.text = "";
                }
            }

            // レシピ表示を遅延更新（Startでシングルトンが間に合わない場合）
            if (recipeText != null && string.IsNullOrEmpty(recipeText.text) && CraftingManager.Instance != null)
            {
                var recipes = CraftingManager.Instance.GetAvailableRecipes();
                recipeText.text = "--- 合成レシピ ---\n" + string.Join("\n", recipes);
            }
        }

        /// <summary>
        /// インベントリ表示を更新
        /// </summary>
        private void UpdateInventoryDisplay()
        {
            if (inventoryText == null || InventoryManager.Instance == null) return;

            var items = InventoryManager.Instance.GetAllItems();
            string display = "--- 所持漢字 ---\n";

            if (items.Count == 0)
            {
                display += "(空)";
            }
            else
            {
                foreach (var kvp in items)
                {
                    display += $"{kvp.Key} x{kvp.Value}\n";
                }
            }

            inventoryText.text = display;
        }

        /// <summary>
        /// アイテム取得時のフィードバック
        /// </summary>
        private void OnItemPickedUp(string itemName)
        {
            ShowMessage($"「{itemName}」を拾った！");
        }

        /// <summary>
        /// メッセージ表示
        /// </summary>
        private void ShowMessage(string msg)
        {
            if (messageText != null)
            {
                messageText.text = msg;
                messageTimer = messageDuration;
            }
        }
    }
}
