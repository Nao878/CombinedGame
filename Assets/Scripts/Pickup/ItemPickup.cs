using UnityEngine;

namespace ZombieSurvival
{
    /// <summary>
    /// フィールド上に配置されるアイテムオブジェクト
    /// プレイヤーが近づくと自動取得される
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class ItemPickup : MonoBehaviour
    {
        [Header("アイテム設定")]
        [SerializeField] private string itemName = "鉄くず";

        [Header("ビジュアル")]
        [SerializeField] private float bobSpeed = 2f;
        [SerializeField] private float bobHeight = 0.3f;
        [SerializeField] private float rotateSpeed = 90f;

        private Vector3 startPos;

        private void Start()
        {
            startPos = transform.position;

            // アイテムの色を設定
            ItemData data = ItemDatabase.GetByName(itemName);
            if (data != null)
            {
                Renderer rend = GetComponent<Renderer>();
                if (rend != null)
                {
                    rend.material.color = data.displayColor;
                }
            }
        }

        private void Update()
        {
            // 浮遊アニメーション
            float newY = startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(startPos.x, newY, startPos.z);

            // 回転アニメーション
            transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
        }

        private void OnTriggerEnter(Collider other)
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
