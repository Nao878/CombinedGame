using UnityEngine;

namespace ZombieSurvival
{
    /// <summary>
    /// 敵のグリッドベースAI
    /// TurnManagerの敵ターンで1マス移動し、プレイヤーを追跡する
    /// 「腐」の漢字で表示
    /// </summary>
    public class EnemyAI : MonoBehaviour
    {
        [Header("グリッド設定")]
        [SerializeField] private float gridSize = 1f;
        [SerializeField] private float moveAnimDuration = 0.15f;

        [Header("AI設定")]
        [SerializeField] private int attackDamage = 1;

        /// <summary>
        /// 現在のグリッド座標
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

            // TurnManager に敵ターンイベントを登録
            if (TurnManager.Instance != null)
            {
                TurnManager.Instance.OnEnemyTurn += OnEnemyTurn;
            }
        }

        private void OnDestroy()
        {
            if (TurnManager.Instance != null)
            {
                TurnManager.Instance.OnEnemyTurn -= OnEnemyTurn;
            }
        }

        private void Update()
        {
            // 移動アニメーション
            if (isMoving)
            {
                moveTimer += Time.deltaTime;
                float t = Mathf.Clamp01(moveTimer / moveAnimDuration);
                t = t * t * (3f - 2f * t);
                transform.position = Vector3.Lerp(moveFrom, moveTo, t);

                if (t >= 1f)
                {
                    transform.position = moveTo;
                    isMoving = false;
                }
            }
        }

        /// <summary>
        /// 敵ターンの処理: プレイヤーに1マス近づく
        /// </summary>
        private void OnEnemyTurn()
        {
            // プレイヤーの位置を取得
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;

            GridMovement playerGrid = player.GetComponent<GridMovement>();
            if (playerGrid == null) return;

            Vector2Int playerPos = playerGrid.GridPosition;
            Vector2Int myPos = GridPosition;

            // プレイヤーと同じマスなら攻撃
            if (myPos == playerPos)
            {
                Attack();
                return;
            }

            // X軸とY軸の距離を比較し、大きい方を1マス近づく
            int dx = playerPos.x - myPos.x;
            int dy = playerPos.y - myPos.y;

            Vector2Int moveDir;

            if (Mathf.Abs(dx) >= Mathf.Abs(dy))
            {
                // X軸方向に移動
                moveDir = new Vector2Int(dx > 0 ? 1 : -1, 0);
            }
            else
            {
                // Y軸方向に移動
                moveDir = new Vector2Int(0, dy > 0 ? 1 : -1);
            }

            // 移動先に他の敵がいないかチェック
            Vector2Int targetPos = myPos + moveDir;
            if (!IsOccupiedByEnemy(targetPos))
            {
                MoveTo(targetPos);
            }

            // 移動後にプレイヤーと重なったら攻撃
            if (GridPosition == playerPos)
            {
                Attack();
            }
        }

        /// <summary>
        /// 指定グリッド座標に移動
        /// </summary>
        private void MoveTo(Vector2Int targetPos)
        {
            GridPosition = targetPos;
            moveFrom = transform.position;
            moveTo = new Vector3(targetPos.x * gridSize, targetPos.y * gridSize, 0f);
            moveTimer = 0f;
            isMoving = true;
        }

        /// <summary>
        /// プレイヤーへの攻撃
        /// </summary>
        private void Attack()
        {
            Debug.Log($"[EnemyAI] 「腐」がプレイヤーにダメージ！ (攻撃力: {attackDamage})");
            // 将来的にHPシステムと連携
        }

        /// <summary>
        /// 指定グリッド座標に他の敵がいるかチェック
        /// </summary>
        private bool IsOccupiedByEnemy(Vector2Int pos)
        {
            var enemies = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
            foreach (var enemy in enemies)
            {
                if (enemy != this && enemy.GridPosition == pos)
                    return true;
            }
            return false;
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
    }
}
