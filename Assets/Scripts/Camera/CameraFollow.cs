using UnityEngine;

namespace ZombieSurvival
{
    /// <summary>
    /// プレイヤーを追従するカメラ制御
    /// 見下ろし視点を維持しながらプレイヤーに追従
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [Header("追従設定")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 18f, -8f);
        [SerializeField] private float smoothSpeed = 8f;

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

            Vector3 desiredPos = target.position + offset;
            Vector3 smoothedPos = Vector3.Lerp(transform.position, desiredPos, smoothSpeed * Time.deltaTime);
            transform.position = smoothedPos;
        }
    }
}
