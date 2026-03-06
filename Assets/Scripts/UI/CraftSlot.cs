using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ZombieSurvival
{
    /// <summary>
    /// 合成スロット（素材枠1/素材枠2）
    /// ドラッグされた漢字カードを受け取る
    /// </summary>
    public class CraftSlot : MonoBehaviour, IDropHandler
    {
        /// <summary>
        /// このスロットにセットされているカード
        /// </summary>
        public DraggableCard CurrentCard { get; private set; }

        /// <summary>
        /// スロットが空かどうか
        /// </summary>
        public bool IsEmpty => CurrentCard == null;

        /// <summary>
        /// セットされている漢字名（空なら""）
        /// </summary>
        public string KanjiName => CurrentCard != null ? CurrentCard.KanjiName : "";

        [Header("スロット表示")]
        [SerializeField] private Text labelText;

        public void OnDrop(PointerEventData eventData)
        {
            DraggableCard card = eventData.pointerDrag?.GetComponent<DraggableCard>();
            if (card == null) return;

            // 既にカードがセットされている場合は元のカードを手札に戻す
            if (CurrentCard != null)
            {
                CurrentCard.ReturnToHand();
            }

            // カードをスロットにセット
            SetCard(card);

            // 合成チェック
            if (CraftingUI.Instance != null)
            {
                CraftingUI.Instance.CheckCraft();
            }
        }

        /// <summary>
        /// カードをスロットにセット
        /// </summary>
        public void SetCard(DraggableCard card)
        {
            CurrentCard = card;
            card.transform.SetParent(transform);

            RectTransform rt = card.GetComponent<RectTransform>();
            rt.anchoredPosition = Vector2.zero;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);

            CanvasGroup cg = card.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.blocksRaycasts = true;
                cg.alpha = 1f;
            }
        }

        /// <summary>
        /// スロットをクリア
        /// </summary>
        public void ClearSlot()
        {
            CurrentCard = null;
        }

        /// <summary>
        /// ラベルテキストを設定
        /// </summary>
        public void SetLabel(Text text)
        {
            labelText = text;
        }
    }
}
