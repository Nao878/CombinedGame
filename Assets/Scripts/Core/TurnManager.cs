using System;
using UnityEngine;

namespace ZombieSurvival
{
    /// <summary>
    /// ターン制の進行管理
    /// プレイヤー行動 → 敵ターン → 次ターン のサイクルを制御
    /// </summary>
    public class TurnManager : MonoBehaviour
    {
        public static TurnManager Instance { get; private set; }

        /// <summary>
        /// 現在のターン数
        /// </summary>
        public int CurrentTurn { get; private set; } = 0;

        /// <summary>
        /// ターン終了時に発火（UIの更新等に使用）
        /// </summary>
        public event Action<int> OnTurnEnd;

        /// <summary>
        /// 敵のターン処理時に発火
        /// </summary>
        public event Action OnEnemyTurn;

        private GridMovement playerMovement;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            // プレイヤーのGridMovementを検索して登録
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerMovement = player.GetComponent<GridMovement>();
                if (playerMovement != null)
                {
                    playerMovement.OnMoved += OnPlayerAction;
                }
            }

            // CraftingManagerのクラフト成功も1ターン消費
            if (CraftingManager.Instance != null)
            {
                CraftingManager.Instance.OnCraftSuccess += (_) => OnPlayerAction();
            }

            Debug.Log("[TurnManager] ターン制システム開始");
        }

        private void OnDestroy()
        {
            if (playerMovement != null)
            {
                playerMovement.OnMoved -= OnPlayerAction;
            }
        }

        /// <summary>
        /// プレイヤーが行動した時に呼ばれる
        /// </summary>
        private void OnPlayerAction()
        {
            CurrentTurn++;

            // 敵のターン処理
            ProcessEnemyTurn();

            // ターン終了通知
            OnTurnEnd?.Invoke(CurrentTurn);

            Debug.Log($"[TurnManager] ターン {CurrentTurn} 完了");
        }

        /// <summary>
        /// 敵のターン処理（将来的にゾンビAIが登録される）
        /// </summary>
        private void ProcessEnemyTurn()
        {
            OnEnemyTurn?.Invoke();
            // 現在は敵未実装のため何もしない
        }
    }
}
