using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ZombieSurvival
{
    /// <summary>
    /// ゲームのHUD表示（グリッドベース・ターン制版）
    /// 所持漢字、操作説明、合成レシピ、メッセージを表示
    /// </summary>
    public class GameHUD : MonoBehaviour
    {
        [Header("UI参照")]
        [SerializeField] private Text inventoryText;
        [SerializeField] private Text controlsText;
        [SerializeField] private Text messageText;
        [SerializeField] private Text recipeText;
        [SerializeField] private Text turnText;
        [SerializeField] private Text hpText;
        [SerializeField] private Text weaponText;

        private float messageTimer = 0f;
        private float messageDuration = 3f;

        private void Start()
        {
            if (controlsText != null)
            {
                controlsText.text =
                    "--- 操作 ---\n" +
                    "WASD : 1マス移動\n" +
                    "Space : 攻撃\n" +
                    "C : 漢字合成";
            }

            if (recipeText != null && CraftingManager.Instance != null)
            {
                var recipes = CraftingManager.Instance.GetAvailableRecipes();
                recipeText.text = "--- 合成レシピ ---\n" + string.Join("\n", recipes);
            }

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

            if (TurnManager.Instance != null)
            {
                TurnManager.Instance.OnTurnEnd += UpdateTurnDisplay;
                UpdateTurnDisplay(0);
            }

            if (PlayerHealth.Instance != null)
            {
                PlayerHealth.Instance.OnHPChanged += UpdateHPDisplay;
                UpdateHPDisplay(PlayerHealth.Instance.CurrentHP, PlayerHealth.Instance.MaxHP);
            }

            UpdateWeaponDisplay();
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

            if (TurnManager.Instance != null)
            {
                TurnManager.Instance.OnTurnEnd -= UpdateTurnDisplay;
            }

            if (PlayerHealth.Instance != null)
            {
                PlayerHealth.Instance.OnHPChanged -= UpdateHPDisplay;
            }
        }

        private void Update()
        {
            if (messageTimer > 0f)
            {
                messageTimer -= Time.deltaTime;
                if (messageTimer <= 0f && messageText != null)
                {
                    messageText.text = "";
                }
            }

            // レシピ遅延更新
            if (recipeText != null && string.IsNullOrEmpty(recipeText.text) && CraftingManager.Instance != null)
            {
                var recipes = CraftingManager.Instance.GetAvailableRecipes();
                recipeText.text = "--- 合成レシピ ---\n" + string.Join("\n", recipes);
            }
        }

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

        private void OnItemPickedUp(string itemName)
        {
            ShowMessage($"「{itemName}」を拾った！");
        }

        private void ShowMessage(string msg)
        {
            if (messageText != null)
            {
                messageText.text = msg;
                messageTimer = messageDuration;
            }
        }

        private void UpdateTurnDisplay(int turn)
        {
            if (turnText != null)
            {
                turnText.text = $"ターン: {turn}";
            }
        }

        private void UpdateHPDisplay(int current, int max)
        {
            if (hpText != null)
            {
                hpText.text = $"HP: {current}/{max}";
                hpText.color = current <= 1 ? Color.red : Color.white;
            }
        }

        private void UpdateWeaponDisplay()
        {
            if (weaponText == null) return;
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) { weaponText.text = "装備: なし"; return; }
            var wc = player.GetComponent<WeaponController>();
            if (wc == null || string.IsNullOrEmpty(wc.EquippedWeapon))
                weaponText.text = "装備: なし";
            else
                weaponText.text = $"装備: 「{wc.EquippedWeapon}」";
        }
    }
}
