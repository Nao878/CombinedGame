using UnityEngine;

namespace ZombieSurvival
{
    /// <summary>
    /// プレイヤーの移動とマウス方向への回転を制御
    /// 2Dトップダウン視点（XY平面移動 + Z軸回転）
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("移動設定")]
        [SerializeField] private float moveSpeed = 7f;

        private Rigidbody2D rb;
        private Camera mainCamera;
        private Vector2 moveInput;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        private void Start()
        {
            mainCamera = Camera.main;
        }

        private void Update()
        {
            // 入力取得
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            moveInput = new Vector2(h, v).normalized;

            // マウス照準（Z軸回転）
            HandleRotation();
        }

        private void FixedUpdate()
        {
            // 物理移動
            rb.linearVelocity = moveInput * moveSpeed;
        }

        /// <summary>
        /// マウスカーソル方向にZ軸回転（2D）
        /// </summary>
        private void HandleRotation()
        {
            if (mainCamera == null) return;

            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direction = (Vector2)mouseWorldPos - (Vector2)transform.position;

            if (direction.sqrMagnitude > 0.01f)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
            }
        }
    }
}
