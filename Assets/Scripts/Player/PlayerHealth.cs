using System;
using UnityEngine;

namespace ZombieSurvival
{
    /// <summary>
    /// プレイヤーのHP管理
    /// ダメージ・回復・ゲームオーバー判定
    /// </summary>
    public class PlayerHealth : MonoBehaviour
    {
        [Header("HP設定")]
        [SerializeField] private int maxHP = 3;
        private int currentHP;

        /// <summary>
        /// HP変化時に発火（current, max）
        /// </summary>
        public event Action<int, int> OnHPChanged;

        /// <summary>
        /// ゲームオーバー時に発火
        /// </summary>
        public event Action OnGameOver;

        public int CurrentHP => currentHP;
        public int MaxHP => maxHP;
        public bool IsAlive => currentHP > 0;

        public static PlayerHealth Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            currentHP = maxHP;
        }

        /// <summary>
        /// ダメージを受ける
        /// </summary>
        public void TakeDamage(int amount)
        {
            if (!IsAlive) return;

            currentHP = Mathf.Max(0, currentHP - amount);
            Debug.Log($"[PlayerHealth] ダメージ {amount}！ HP: {currentHP}/{maxHP}");
            OnHPChanged?.Invoke(currentHP, maxHP);

            if (currentHP <= 0)
            {
                Debug.Log("[PlayerHealth] ゲームオーバー");
                OnGameOver?.Invoke();
            }
        }

        /// <summary>
        /// HP回復
        /// </summary>
        public void Heal(int amount)
        {
            if (!IsAlive) return;
            currentHP = Mathf.Min(maxHP, currentHP + amount);
            OnHPChanged?.Invoke(currentHP, maxHP);
        }
    }
}
