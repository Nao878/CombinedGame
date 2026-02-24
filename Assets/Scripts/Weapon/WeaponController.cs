using UnityEngine;

namespace ZombieSurvival
{
    /// <summary>
    /// 武器の発射制御
    /// インベントリにハンドガンがある時のみ左クリックで弾を発射
    /// </summary>
    public class WeaponController : MonoBehaviour
    {
        [Header("発射設定")]
        [SerializeField] private float bulletSpeed = 25f;
        [SerializeField] private float fireRate = 0.3f;
        [SerializeField] private float bulletLifetime = 3f;
        [SerializeField] private float bulletScale = 0.2f;

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
        /// 発射可能か判定して弾を生成
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
                : transform.position + transform.forward * 1.0f + Vector3.up * 0.5f;

            // 弾の生成
            GameObject bullet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bullet.name = "Bullet";
            bullet.transform.position = spawnPos;
            bullet.transform.localScale = Vector3.one * bulletScale;

            // 色設定（黄色）
            Renderer rend = bullet.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material.color = new Color(1f, 0.9f, 0.2f);
            }

            // 物理設定
            Rigidbody rb = bullet.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.linearVelocity = transform.forward * bulletSpeed;

            // 弾のスクリプト付与
            Bullet bulletScript = bullet.AddComponent<Bullet>();
            bulletScript.SetLifetime(bulletLifetime);

            // 発射者との衝突を無視
            Collider playerCol = GetComponent<Collider>();
            Collider bulletCol = bullet.GetComponent<Collider>();
            if (playerCol != null && bulletCol != null)
            {
                Physics.IgnoreCollision(playerCol, bulletCol);
            }
        }
    }
}
