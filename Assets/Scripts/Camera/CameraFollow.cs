using UnityEngine;

namespace ZombieSurvival
{
    /// <summary>
    /// プレイヤーを追従するカメラ制御（グリッドベース版）
    /// Orthographicカメラ、物理なし
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [Header("追従設定")]
        [SerializeField] private Transform target;
        [SerializeField] private float smoothSpeed = 8f;
        [SerializeField] private float cameraZ = -10f;

        private void LateUpdate()
        {
            if (target == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                    target = player.transform;
                else
                    return;
            }

            Vector3 desiredPos = new Vector3(target.position.x, target.position.y, cameraZ);
            transform.position = Vector3.Lerp(transform.position, desiredPos, smoothSpeed * Time.deltaTime);
        }
    }
}
