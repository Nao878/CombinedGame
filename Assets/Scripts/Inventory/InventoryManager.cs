using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZombieSurvival
{
    /// <summary>
    /// インベントリ管理（シングルトン）
    /// アイテムの追加・削除・確認とイベント通知を担当
    /// </summary>
    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager Instance { get; private set; }

        /// <summary>
        /// インベントリ内容が変化した時に発火
        /// </summary>
        public event Action OnInventoryChanged;

        /// <summary>
        /// アイテム取得時に発火（アイテム名を通知）
        /// </summary>
        public event Action<string> OnItemPickedUp;

        // アイテム名 → 所持数
        private Dictionary<string, int> items = new Dictionary<string, int>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        /// <summary>
        /// アイテムを追加
        /// </summary>
        public void AddItem(string itemName, int amount = 1)
        {
            if (items.ContainsKey(itemName))
            {
                items[itemName] += amount;
            }
            else
            {
                items[itemName] = amount;
            }

            Debug.Log($"[Inventory] {itemName} x{amount} を取得！ (計: {items[itemName]})");
            OnItemPickedUp?.Invoke(itemName);
            OnInventoryChanged?.Invoke();
        }

        /// <summary>
        /// アイテムを消費
        /// </summary>
        public bool RemoveItem(string itemName, int amount = 1)
        {
            if (!HasItem(itemName, amount)) return false;

            items[itemName] -= amount;
            if (items[itemName] <= 0)
            {
                items.Remove(itemName);
            }

            OnInventoryChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// 指定数以上のアイテムを所持しているか
        /// </summary>
        public bool HasItem(string itemName, int amount = 1)
        {
            return items.ContainsKey(itemName) && items[itemName] >= amount;
        }

        /// <summary>
        /// アイテムの所持数を取得
        /// </summary>
        public int GetItemCount(string itemName)
        {
            return items.ContainsKey(itemName) ? items[itemName] : 0;
        }

        /// <summary>
        /// 全インベントリデータを取得（読み取り専用）
        /// </summary>
        public Dictionary<string, int> GetAllItems()
        {
            return new Dictionary<string, int>(items);
        }
    }
}
