using UnityEngine;

namespace ZombieSurvival
{
    /// <summary>
    /// プレイヤーを追従するカメラ制御（2D版）
    /// Orthographicカメラで見下ろし視点を維持
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [Header("追従設定")]
        [SerializeField] private Transform target;
        [SerializeField] private float smoothSpeed = 8f;
        [SerializeField] private float cameraZ = -10f;

        private void LateUpdate()
        {
            // ターゲットが未設定なら自動検索
            if (target == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    target = player.transform;
                }
                else
                {
                    return;
                }
            }

            Vector3 desiredPos = new Vector3(target.position.x, target.position.y, cameraZ);
            Vector3 smoothedPos = Vector3.Lerp(transform.position, desiredPos, smoothSpeed * Time.deltaTime);
            transform.position = smoothedPos;
        }
    }
}
