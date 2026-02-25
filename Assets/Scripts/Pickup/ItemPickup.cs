using UnityEngine;

namespace ZombieSurvival
{
    /// <summary>
    /// フィールド上に配置される漢字パーツオブジェクト（2D版）
    /// TextMesh で漢字1文字を表示し、プレイヤー接近で自動取得
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class ItemPickup : MonoBehaviour
    {
        [Header("アイテム設定")]
        [SerializeField] private string itemName = "金";

        [Header("ビジュアル")]
        [SerializeField] private float bobSpeed = 2f;
        [SerializeField] private float bobHeight = 0.15f;

        private Vector3 startPos;

        private void Start()
        {
            startPos = transform.position;
        }

        private void Update()
        {
            // 浮遊アニメーション（Y軸で上下）
            float newY = startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(startPos.x, newY, startPos.z);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                if (InventoryManager.Instance != null)
                {
                    InventoryManager.Instance.AddItem(itemName);
                }
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// アイテム名を設定（Editorツールから使用）
        /// </summary>
        public void SetItemName(string name)
        {
            itemName = name;
        }
    }
}
