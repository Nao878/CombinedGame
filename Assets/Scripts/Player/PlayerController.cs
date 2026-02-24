using UnityEngine;

namespace ZombieSurvival
{
    /// <summary>
    /// プレイヤーの移動とマウス方向への回転を制御
    /// 見下ろし型（Top-Down）視点用
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("移動設定")]
        [SerializeField] private float moveSpeed = 7f;
        [SerializeField] private float gravity = -9.81f;

        private CharacterController controller;
        private Vector3 velocity;
        private Camera mainCamera;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
        }

        private void Start()
        {
            mainCamera = Camera.main;
        }

        private void Update()
        {
            HandleMovement();
            HandleRotation();
        }

        /// <summary>
        /// WASD/矢印キーでXZ平面上を移動
        /// </summary>
        private void HandleMovement()
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");

            Vector3 moveDir = new Vector3(h, 0f, v).normalized;
            controller.Move(moveDir * moveSpeed * Time.deltaTime);

            // 重力
            if (controller.isGrounded && velocity.y < 0f)
            {
                velocity.y = -2f;
            }
            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }

        /// <summary>
        /// マウスカーソルのワールド座標に向かって回転
        /// </summary>
        private void HandleRotation()
        {
            if (mainCamera == null) return;

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

            if (groundPlane.Raycast(ray, out float distance))
            {
                Vector3 hitPoint = ray.GetPoint(distance);
                Vector3 lookDir = hitPoint - transform.position;
                lookDir.y = 0f;

                if (lookDir.sqrMagnitude > 0.01f)
                {
                    transform.rotation = Quaternion.LookRotation(lookDir);
                }
            }
        }
    }
}
