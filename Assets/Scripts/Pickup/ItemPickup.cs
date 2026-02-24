using UnityEngine;

namespace ZombieSurvival
{
    /// <summary>
    /// フィールド上に配置されるアイテムオブジェクト（2D版）
    /// プレイヤーが近づくと自動取得される
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class ItemPickup : MonoBehaviour
    {
        [Header("アイテム設定")]
        [SerializeField] private string itemName = "鉄くず";

        [Header("ビジュアル")]
        [SerializeField] private float bobSpeed = 2f;
        [SerializeField] private float bobHeight = 0.15f;
        [SerializeField] private float rotateSpeed = 90f;

        private Vector3 startPos;

        private void Start()
        {
            startPos = transform.position;

            // アイテムの色を設定
            ItemData data = ItemDatabase.GetByName(itemName);
            if (data != null)
            {
                SpriteRenderer sr = GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.color = data.displayColor;
                }
            }
        }

        private void Update()
        {
            // 浮遊アニメーション（Y軸で上下）
            float newY = startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(startPos.x, newY, startPos.z);

            // Z軸回転アニメーション
            transform.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);
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
