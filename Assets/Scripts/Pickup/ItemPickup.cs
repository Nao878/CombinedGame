using UnityEngine;

namespace ZombieSurvival
{
    /// <summary>
    /// フィールド上に配置される漢字パーツオブジェクト（グリッドベース版）
    /// プレイヤーが同一グリッドに到達すると取得される
    /// </summary>
    public class ItemPickup : MonoBehaviour
    {
        [Header("アイテム設定")]
        [SerializeField] private string itemName = "金";

        [Header("ビジュアル")]
        [SerializeField] private float bobSpeed = 2f;
        [SerializeField] private float bobHeight = 0.1f;

        private Vector3 basePos;

        private void Start()
        {
            basePos = transform.position;
        }

        private void Update()
        {
            // 浮遊アニメーション（Y軸で上下）
            float offset = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = basePos + new Vector3(0f, offset, 0f);
        }

        /// <summary>
        /// アイテムを取得する（GridMovement から呼ばれる）
        /// </summary>
        public void PickUp()
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.AddItem(itemName);
            }
            Destroy(gameObject);
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
