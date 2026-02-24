using UnityEngine;

namespace ZombieSurvival
{
    /// <summary>
    /// 弾の挙動
    /// 前方に直進し、一定時間後に自動消滅
    /// </summary>
    public class Bullet : MonoBehaviour
    {
        private float lifetime = 3f;

        /// <summary>
        /// 弾の寿命を設定
        /// </summary>
        public void SetLifetime(float time)
        {
            lifetime = time;
        }

        private void Start()
        {
            Destroy(gameObject, lifetime);
        }

        private void OnCollisionEnter(Collision collision)
        {
            // 将来的にダメージ処理をここに追加
            // 例: IDamageable インターフェースを持つオブジェクトにダメージを与える
            Debug.Log($"[Bullet] {collision.gameObject.name} にヒット！");
            Destroy(gameObject);
        }
    }
}
