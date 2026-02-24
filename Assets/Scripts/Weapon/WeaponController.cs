using UnityEngine;

namespace ZombieSurvival
{
    /// <summary>
    /// 武器の発射制御（2D版）
    /// インベントリに武器がある時のみ左クリックで弾を発射
    /// </summary>
    public class WeaponController : MonoBehaviour
    {
        [Header("発射設定")]
        [SerializeField] private float bulletSpeed = 15f;
        [SerializeField] private float fireRate = 0.3f;
        [SerializeField] private float bulletLifetime = 3f;
        [SerializeField] private float bulletScale = 0.15f;

        [Header("発射位置")]
        [SerializeField] private Transform firePoint;

        private float nextFireTime = 0f;

        private void Update()
        {
            if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
            {
                TryFire();
            }
        }

        /// <summary>
        /// 発射可能か判定して弾を生成（2D）
        /// </summary>
        private void TryFire()
        {
            if (InventoryManager.Instance == null) return;

            // いずれかの武器を持っているか確認
            bool hasWeapon = InventoryManager.Instance.HasItem("ハンドガン") ||
                             InventoryManager.Instance.HasItem("釘バット");

            if (!hasWeapon) return;

            nextFireTime = Time.time + fireRate;

            // 発射位置の決定
            Vector3 spawnPos = firePoint != null
                ? firePoint.position
                : transform.position + transform.up * 0.6f;

            // 弾の生成（2Dスプライト）
            GameObject bullet = new GameObject("Bullet");
            bullet.transform.position = spawnPos;
            bullet.transform.localScale = Vector3.one * bulletScale;

            // SpriteRenderer（黄色い丸）
            SpriteRenderer sr = bullet.AddComponent<SpriteRenderer>();
            sr.sprite = GetCircleSprite();
            sr.color = new Color(1f, 0.9f, 0.2f);
            sr.sortingOrder = 5;

            // 2D物理
            Rigidbody2D rb = bullet.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            // 発射方向 = プレイヤーのtransform.up（Z軸回転に対応）
            rb.linearVelocity = transform.up * bulletSpeed;

            // CircleCollider2D
            CircleCollider2D col = bullet.AddComponent<CircleCollider2D>();
            col.radius = 0.5f;

            // Bulletスクリプト付与
            Bullet bulletScript = bullet.AddComponent<Bullet>();
            bulletScript.SetLifetime(bulletLifetime);

            // 発射者との衝突を無視
            Collider2D playerCol = GetComponent<Collider2D>();
            if (playerCol != null && col != null)
            {
                Physics2D.IgnoreCollision(playerCol, col);
            }
        }

        /// <summary>
        /// Unity標準の Circle スプライトを取得
        /// </summary>
        private static Sprite GetCircleSprite()
        {
            // Knobスプライトを丸として利用
            return AssetDatabase2DHelper.GetDefaultSprite("Circle");
        }
    }

    /// <summary>
    /// Unity標準2Dスプライト取得ヘルパー
    /// </summary>
    public static class AssetDatabase2DHelper
    {
        private static Sprite cachedSquare;
        private static Sprite cachedCircle;

        /// <summary>
        /// Unity内蔵のデフォルトスプライトを取得
        /// "Square" または "Circle" を指定
        /// </summary>
        public static Sprite GetDefaultSprite(string shapeName)
        {
            if (shapeName == "Square" && cachedSquare != null) return cachedSquare;
            if (shapeName == "Circle" && cachedCircle != null) return cachedCircle;

            // Unity のビルトインスプライトを検索
            Sprite[] allSprites = Resources.FindObjectsOfTypeAll<Sprite>();
            foreach (var s in allSprites)
            {
                if (s.name == "Knob" && shapeName == "Circle")
                {
                    cachedCircle = s;
                    return s;
                }
                if (s.name == "UISprite" && shapeName == "Square")
                {
                    cachedSquare = s;
                    return s;
                }
            }

            // フォールバック: 任意のスプライトを返す
            foreach (var s in allSprites)
            {
                if (s.name == shapeName)
                {
                    if (shapeName == "Square") cachedSquare = s;
                    if (shapeName == "Circle") cachedCircle = s;
                    return s;
                }
            }

            return null;
        }
    }
}
