using UnityEngine;
using System;

namespace ZombieSurvival
{
    /// <summary>
    /// グリッドベースのプレイヤー移動コントローラー
    /// WASDで1マスずつスナップ移動。物理演算不使用。
    /// </summary>
    public class GridMovement : MonoBehaviour
    {
        [Header("グリッド設定")]
        [SerializeField] private float gridSize = 1f;
        [SerializeField] private float moveAnimDuration = 0.1f;

        [Header("移動範囲")]
        [SerializeField] private int mapMinX = -9;
        [SerializeField] private int mapMaxX = 9;
        [SerializeField] private int mapMinY = -6;
        [SerializeField] private int mapMaxY = 6;

        /// <summary>
        /// プレイヤーが1マス移動したときに発火（ターン進行用）
        /// </summary>
        public event Action OnMoved;

        /// <summary>
        /// プレイヤーの現在のグリッド座標
        /// </summary>
        public Vector2Int GridPosition { get; private set; }

        private bool isMoving = false;
        private Vector3 moveFrom;
        private Vector3 moveTo;
        private float moveTimer;

        private void Start()
        {
            // 現在位置をグリッドにスナップ
            SnapToGrid();
        }

        private void Update()
        {
            if (isMoving)
            {
                // 移動アニメーション中
                moveTimer += Time.deltaTime;
                float t = Mathf.Clamp01(moveTimer / moveAnimDuration);
                // スムーズステップで滑らかに
                t = t * t * (3f - 2f * t);
                transform.position = Vector3.Lerp(moveFrom, moveTo, t);

                if (t >= 1f)
                {
                    transform.position = moveTo;
                    isMoving = false;
                }
                return;
            }

            // 入力処理（移動中は受け付けない）
            Vector2Int direction = Vector2Int.zero;

            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
                direction = Vector2Int.up;
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
                direction = Vector2Int.down;
            else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
                direction = Vector2Int.left;
            else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
                direction = Vector2Int.right;

            if (direction != Vector2Int.zero)
            {
                TryMove(direction);
            }
        }

        /// <summary>
        /// 指定方向に1マス移動を試行
        /// </summary>
        public bool TryMove(Vector2Int direction)
        {
            Vector2Int targetPos = GridPosition + direction;

            // 範囲チェック
            if (targetPos.x < mapMinX || targetPos.x > mapMaxX ||
                targetPos.y < mapMinY || targetPos.y > mapMaxY)
            {
                return false;
            }

            // 移動実行
            GridPosition = targetPos;
            moveFrom = transform.position;
            moveTo = new Vector3(targetPos.x * gridSize, targetPos.y * gridSize, 0f);
            moveTimer = 0f;
            isMoving = true;

            // ターン進行通知
            OnMoved?.Invoke();

            // 移動先のアイテムチェック
            CheckPickupAtPosition(targetPos);

            return true;
        }

        /// <summary>
        /// 現在位置をグリッドにスナップ
        /// </summary>
        private void SnapToGrid()
        {
            int gx = Mathf.RoundToInt(transform.position.x / gridSize);
            int gy = Mathf.RoundToInt(transform.position.y / gridSize);
            GridPosition = new Vector2Int(gx, gy);
            transform.position = new Vector3(gx * gridSize, gy * gridSize, 0f);
        }

        /// <summary>
        /// グリッド座標のアイテムをチェックして取得
        /// </summary>
        private void CheckPickupAtPosition(Vector2Int pos)
        {
            // シーン内のすべてのItemPickupを検索
            var pickups = FindObjectsByType<ItemPickup>(FindObjectsSortMode.None);
            foreach (var pickup in pickups)
            {
                Vector2Int pickupGrid = new Vector2Int(
                    Mathf.RoundToInt(pickup.transform.position.x / gridSize),
                    Mathf.RoundToInt(pickup.transform.position.y / gridSize)
                );

                if (pickupGrid == pos)
                {
                    pickup.PickUp();
                }
            }
        }

        /// <summary>
        /// 移動範囲を設定（Editorツールから使用）
        /// </summary>
        public void SetBounds(int minX, int maxX, int minY, int maxY)
        {
            mapMinX = minX;
            mapMaxX = maxX;
            mapMinY = minY;
            mapMaxY = maxY;
        }
    }
}
