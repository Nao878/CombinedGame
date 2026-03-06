using UnityEngine;

namespace ZombieSurvival
{
    /// <summary>
    /// 武器の装備・表示・攻撃制御（グリッドベース版）
    /// 武器は1つだけ装備可能。向きの隣にEquippedWeaponDisplayを表示。
    /// Spaceキーで武器種別に応じた攻撃を実行。
    /// </summary>
    public class WeaponController : MonoBehaviour
    {
        /// <summary>
        /// 現在装備中の武器名（空 = 未装備）
        /// </summary>
        public string EquippedWeapon { get; private set; } = "";

        private GridMovement gridMovement;
        private TextMesh weaponDisplay;
        private MeshRenderer weaponRenderer;

        private void Start()
        {
            gridMovement = GetComponent<GridMovement>();

            // EquippedWeaponDisplay 子オブジェクトを検索
            Transform displayTf = transform.Find("EquippedWeaponDisplay");
            if (displayTf != null)
            {
                weaponDisplay = displayTf.GetComponent<TextMesh>();
                weaponRenderer = displayTf.GetComponent<MeshRenderer>();
            }

            // 移動時に武器表示位置を更新
            if (gridMovement != null)
            {
                gridMovement.OnMoved += UpdateWeaponDisplayPosition;
            }

            UpdateWeaponDisplay();
        }

        private void OnDestroy()
        {
            if (gridMovement != null)
            {
                gridMovement.OnMoved -= UpdateWeaponDisplayPosition;
            }
        }

        private void Update()
        {
            // Spaceキーで攻撃
            if (Input.GetKeyDown(KeyCode.Space))
            {
                TryAttack();
            }
        }

        /// <summary>
        /// 武器を装備する（インベントリから呼ばれる）
        /// </summary>
        public void Equip(string weaponName)
        {
            // 既に装備中の武器があればインベントリに戻す
            if (!string.IsNullOrEmpty(EquippedWeapon) && InventoryManager.Instance != null)
            {
                InventoryManager.Instance.AddItem(EquippedWeapon);
            }

            EquippedWeapon = weaponName;

            // インベントリから消費
            if (!string.IsNullOrEmpty(weaponName) && InventoryManager.Instance != null)
            {
                InventoryManager.Instance.RemoveItem(weaponName);
            }

            UpdateWeaponDisplay();
            UpdateWeaponDisplayPosition();
            Debug.Log($"[Weapon] 「{weaponName}」を装備！");
        }

        /// <summary>
        /// 装備を解除する
        /// </summary>
        public void Unequip()
        {
            if (!string.IsNullOrEmpty(EquippedWeapon) && InventoryManager.Instance != null)
            {
                InventoryManager.Instance.AddItem(EquippedWeapon);
            }
            EquippedWeapon = "";
            UpdateWeaponDisplay();
        }

        /// <summary>
        /// 攻撃を試行（武器種別で分岐）
        /// </summary>
        private void TryAttack()
        {
            if (string.IsNullOrEmpty(EquippedWeapon)) return;
            if (gridMovement == null) return;

            Vector2Int facing = gridMovement.FacingDirection;

            switch (EquippedWeapon)
            {
                case "剣":
                    AttackMelee(facing);
                    break;
                case "銃":
                    AttackRanged(facing);
                    break;
                case "板":
                    // 板（盾）は攻撃なし
                    Debug.Log("[Weapon] 板は防御用です");
                    break;
                case "爆":
                    AttackExplosion();
                    break;
                default:
                    AttackMelee(facing); // デフォルトは近接
                    break;
            }

            // 攻撃はターン消費
            if (TurnManager.Instance != null)
            {
                // OnMovedイベント経由ではないのでここで直接通知はしない
                // （将来的にTurnManagerに攻撃アクション追加）
            }
        }

        /// <summary>
        /// 近接攻撃（剣）: 目の前1マスの敵を撃破
        /// </summary>
        private void AttackMelee(Vector2Int facing)
        {
            Vector2Int targetPos = gridMovement.GridPosition + facing;
            EnemyAI enemy = FindEnemyAtPosition(targetPos);

            if (enemy != null)
            {
                Debug.Log($"[Weapon] 「剣」で ({targetPos.x},{targetPos.y}) の敵を斬撃！");
                Destroy(enemy.gameObject);
            }
            else
            {
                Debug.Log($"[Weapon] 「剣」空振り ({targetPos.x},{targetPos.y})");
            }
        }

        /// <summary>
        /// 遠距離攻撃（銃）: 向き方向の直線上で最初の敵を撃破
        /// </summary>
        private void AttackRanged(Vector2Int facing)
        {
            Vector2Int checkPos = gridMovement.GridPosition + facing;

            // 最大20マス先まで走査
            for (int i = 0; i < 20; i++)
            {
                EnemyAI enemy = FindEnemyAtPosition(checkPos);
                if (enemy != null)
                {
                    Debug.Log($"[Weapon] 「銃」で ({checkPos.x},{checkPos.y}) の敵を撃破！");
                    Destroy(enemy.gameObject);
                    return;
                }
                checkPos += facing;
            }

            Debug.Log("[Weapon] 「銃」命中なし");
        }

        /// <summary>
        /// 範囲攻撃（爆）: 周囲8マスの敵を全て撃破
        /// </summary>
        private void AttackExplosion()
        {
            Vector2Int center = gridMovement.GridPosition;
            int killCount = 0;

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;
                    Vector2Int pos = center + new Vector2Int(dx, dy);
                    EnemyAI enemy = FindEnemyAtPosition(pos);
                    if (enemy != null)
                    {
                        Destroy(enemy.gameObject);
                        killCount++;
                    }
                }
            }

            Debug.Log($"[Weapon] 「爆」で周囲の敵 {killCount} 体を撃破！");
        }

        /// <summary>
        /// 指定グリッド座標の敵を検索
        /// </summary>
        private EnemyAI FindEnemyAtPosition(Vector2Int pos)
        {
            var enemies = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
            foreach (var enemy in enemies)
            {
                if (enemy.GridPosition == pos) return enemy;
            }
            return null;
        }

        /// <summary>
        /// 武器表示テキストを更新
        /// </summary>
        private void UpdateWeaponDisplay()
        {
            if (weaponDisplay == null) return;

            if (string.IsNullOrEmpty(EquippedWeapon))
            {
                weaponDisplay.text = "";
                if (weaponRenderer != null) weaponRenderer.enabled = false;
            }
            else
            {
                weaponDisplay.text = EquippedWeapon;
                if (weaponRenderer != null) weaponRenderer.enabled = true;
            }
        }

        /// <summary>
        /// 武器表示位置を向き方向に合わせて更新
        /// </summary>
        private void UpdateWeaponDisplayPosition()
        {
            if (weaponDisplay == null || gridMovement == null) return;

            Vector2Int facing = gridMovement.FacingDirection;
            weaponDisplay.transform.localPosition = new Vector3(facing.x, facing.y, 0f);
        }
    }
}
