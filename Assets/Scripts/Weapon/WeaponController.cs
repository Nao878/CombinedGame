using UnityEngine;

namespace ZombieSurvival
{
    /// <summary>
    /// 武器の使用制御（グリッドベース・ターン制版）
    /// 漢字武器所持時に攻撃キーで弾を発射
    /// </summary>
    public class WeaponController : MonoBehaviour
    {
        [Header("発射設定")]
        [SerializeField] private float bulletSpeed = 12f;
        [SerializeField] private float bulletLifetime = 2f;
        [SerializeField] private float bulletScale = 0.2f;

        /// <summary>
        /// 攻撃方向（最後にプレイヤーが向いた方向）
        /// </summary>
        private Vector2Int lastDirection = Vector2Int.up;

        private GridMovement gridMovement;

        private void Start()
        {
            gridMovement = GetComponent<GridMovement>();
            if (gridMovement != null)
            {
                gridMovement.OnMoved += OnPlayerMoved;
            }
        }

        private void OnDestroy()
        {
            if (gridMovement != null)
            {
                gridMovement.OnMoved -= OnPlayerMoved;
            }
        }

        /// <summary>
        /// 移動時に方向を記録
        /// </summary>
        private void OnPlayerMoved()
        {
            // GridMovementの最後の移動方向を記録
            // （将来的にGridMovementに方向プロパティを追加できる）
        }

        private void Update()
        {
            // 方向キーから攻撃方向を更新
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) lastDirection = Vector2Int.up;
            else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) lastDirection = Vector2Int.down;
            else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) lastDirection = Vector2Int.left;
            else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) lastDirection = Vector2Int.right;

            // スペースキーで攻撃
            if (Input.GetKeyDown(KeyCode.Space))
            {
                TryFire();
            }
        }

        /// <summary>
        /// 発射可能か判定して弾を生成
        /// </summary>
        private void TryFire()
        {
            if (InventoryManager.Instance == null) return;

            bool hasWeapon = InventoryManager.Instance.HasItem("銃") ||
                             InventoryManager.Instance.HasItem("剣") ||
                             InventoryManager.Instance.HasItem("板") ||
                             InventoryManager.Instance.HasItem("爆");

            if (!hasWeapon) return;

            Vector3 dir = new Vector3(lastDirection.x, lastDirection.y, 0f);
            Vector3 spawnPos = transform.position + dir * 0.5f;

            // 弾の生成
            GameObject bullet = new GameObject("Bullet");
            bullet.transform.position = spawnPos;
            bullet.transform.localScale = Vector3.one * bulletScale;

            // TextMeshで「弾」を表示
            TextMesh tm = bullet.AddComponent<TextMesh>();
            tm.text = "・";
            tm.fontSize = 48;
            tm.characterSize = 0.15f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = new Color(1f, 0.9f, 0.2f);

            MeshRenderer mr = bullet.GetComponent<MeshRenderer>();
            if (mr != null) mr.sortingOrder = 8;

            // 弾の挙動（Rigidbody不使用、Transform移動）
            Bullet bulletScript = bullet.AddComponent<Bullet>();
            bulletScript.Initialize(dir * bulletSpeed, bulletLifetime);
        }
    }
}
