using UnityEngine;

namespace ZombieSurvival
{
    /// <summary>
    /// 弾の挙動（グリッドベース版）
    /// Rigidbody不使用、Transformで直進し一定時間後に消滅
    /// </summary>
    public class Bullet : MonoBehaviour
    {
        private Vector3 velocity;
        private float lifetime;
        private bool initialized = false;

        /// <summary>
        /// 弾の初期化（速度と寿命）
        /// </summary>
        public void Initialize(Vector3 vel, float life)
        {
            velocity = vel;
            lifetime = life;
            initialized = true;
        }

        /// <summary>
        /// 後方互換用
        /// </summary>
        public void SetLifetime(float time)
        {
            lifetime = time;
        }

        private void Update()
        {
            if (!initialized) return;

            // Transform ベースの移動
            transform.position += velocity * Time.deltaTime;

            // 寿命管理
            lifetime -= Time.deltaTime;
            if (lifetime <= 0f)
            {
                Destroy(gameObject);
            }
        }
    }
}
