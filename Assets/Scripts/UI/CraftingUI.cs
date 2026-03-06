using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ZombieSurvival
{
    /// <summary>
    /// 合成UIの管理
    /// 手札カード生成、合成判定、インベントリ連携
    /// </summary>
    public class CraftingUI : MonoBehaviour
    {
        public static CraftingUI Instance { get; private set; }

        [Header("UI参照")]
        [SerializeField] private Transform handArea;
        [SerializeField] private CraftSlot slot1;
        [SerializeField] private CraftSlot slot2;
        [SerializeField] private Text resultText;

        [Header("カード設定")]
        [SerializeField] private Canvas parentCanvas;

        /// <summary>
        /// 手札エリア（DraggableCardがReturnToHandで使用）
        /// </summary>
        public Transform HandArea => handArea;

        // カードプレハブは動的生成
        private List<DraggableCard> handCards = new List<DraggableCard>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            // インベントリ変更時にカードを再生成
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.OnInventoryChanged += RefreshHandCards;
                RefreshHandCards();
            }
        }

        private void OnDestroy()
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.OnInventoryChanged -= RefreshHandCards;
            }
        }

        /// <summary>
        /// 手札カードをインベントリから再生成
        /// </summary>
        public void RefreshHandCards()
        {
            // 既存カード削除（スロットにセットされているカードは除外）
            for (int i = handCards.Count - 1; i >= 0; i--)
            {
                if (handCards[i] != null && handCards[i].transform.parent == handArea)
                {
                    Destroy(handCards[i].gameObject);
                }
                handCards.RemoveAt(i);
            }

            if (InventoryManager.Instance == null) return;

            var items = InventoryManager.Instance.GetAllItems();

            foreach (var kvp in items)
            {
                // 武器はカードとして表示しない（装備中のため）
                ItemData data = ItemDatabase.GetByName(kvp.Key);
                if (data != null && data.itemType == ItemType.Weapon) continue;

                for (int i = 0; i < kvp.Value; i++)
                {
                    CreateCard(kvp.Key);
                }
            }
        }

        /// <summary>
        /// 漢字カードを生成
        /// </summary>
        private void CreateCard(string kanji)
        {
            // カードオブジェクト
            GameObject cardObj = new GameObject("Card_" + kanji);
            cardObj.transform.SetParent(handArea, false);

            // RectTransform
            RectTransform rt = cardObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(70f, 90f);

            // 背景
            Image bgImage = cardObj.AddComponent<Image>();
            bgImage.color = new Color(0.15f, 0.15f, 0.2f, 0.9f);

            // CanvasGroup（ドラッグ用）
            cardObj.AddComponent<CanvasGroup>();

            // テキスト
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(cardObj.transform, false);

            RectTransform textRt = textObj.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

            Text text = textObj.AddComponent<Text>();
            text.text = kanji;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 36;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            // アイテムの色を取得
            ItemData data = ItemDatabase.GetByName(kanji);
            text.color = data != null ? data.displayColor : Color.white;

            // DraggableCard
            DraggableCard card = cardObj.AddComponent<DraggableCard>();
            card.Setup(kanji, parentCanvas);

            handCards.Add(card);
        }

        /// <summary>
        /// 両スロットが埋まったら合成判定
        /// </summary>
        public void CheckCraft()
        {
            if (slot1 == null || slot2 == null) return;
            if (slot1.IsEmpty || slot2.IsEmpty) return;

            string mat1 = slot1.KanjiName;
            string mat2 = slot2.KanjiName;

            // レシピ検索
            string result = FindRecipeResult(mat1, mat2);

            if (!string.IsNullOrEmpty(result))
            {
                // 合成成功！
                Debug.Log($"[CraftingUI] {mat1} + {mat2} → {result} 合成成功！");

                // スロットのカードを破棄
                if (slot1.CurrentCard != null) Destroy(slot1.CurrentCard.gameObject);
                if (slot2.CurrentCard != null) Destroy(slot2.CurrentCard.gameObject);
                slot1.ClearSlot();
                slot2.ClearSlot();

                // インベントリから素材を消費
                if (InventoryManager.Instance != null)
                {
                    InventoryManager.Instance.RemoveItem(mat1);
                    InventoryManager.Instance.RemoveItem(mat2);
                }

                // 武器を装備
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    WeaponController wc = player.GetComponent<WeaponController>();
                    if (wc != null)
                    {
                        // 一旦インベントリに追加してから装備（Equipが内部でRemoveする）
                        InventoryManager.Instance?.AddItem(result);
                        wc.Equip(result);
                    }
                }

                // 結果表示
                if (resultText != null)
                {
                    resultText.text = result;
                    resultText.color = Color.yellow;
                }

                // 手札リフレッシュはインベントリ変更イベント経由で自動
            }
            else
            {
                // 合成失敗：カードを手札に戻す
                Debug.Log($"[CraftingUI] {mat1} + {mat2} → 合成失敗！手札に戻します");

                if (slot1.CurrentCard != null) slot1.CurrentCard.ReturnToHand();
                if (slot2.CurrentCard != null) slot2.CurrentCard.ReturnToHand();
                slot1.ClearSlot();
                slot2.ClearSlot();

                if (resultText != null)
                {
                    resultText.text = "？";
                    resultText.color = Color.gray;
                }
            }
        }

        /// <summary>
        /// レシピ検索（順番不問）
        /// </summary>
        private string FindRecipeResult(string mat1, string mat2)
        {
            foreach (var recipe in ItemDatabase.Recipes)
            {
                if ((recipe.material1 == mat1 && recipe.material2 == mat2) ||
                    (recipe.material1 == mat2 && recipe.material2 == mat1))
                {
                    return recipe.result;
                }
            }
            return null;
        }

        /// <summary>
        /// 参照を設定（SceneSetupToolから使用）
        /// </summary>
        public void SetReferences(Transform hand, CraftSlot s1, CraftSlot s2, Text result, Canvas canvas)
        {
            handArea = hand;
            slot1 = s1;
            slot2 = s2;
            resultText = result;
            parentCanvas = canvas;
        }
    }
}
