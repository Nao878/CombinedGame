using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ZombieSurvival
{
    /// <summary>
    /// ドラッグ可能な漢字カード
    /// 手札エリアからドラッグして合成スロットにドロップする
    /// </summary>
    public class DraggableCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        /// <summary>
        /// このカードが表す漢字名
        /// </summary>
        public string KanjiName { get; private set; }

        private Canvas canvas;
        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private Transform originalParent;
        private Vector2 originalPosition;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        /// <summary>
        /// カードを初期化
        /// </summary>
        public void Setup(string kanji, Canvas parentCanvas)
        {
            KanjiName = kanji;
            canvas = parentCanvas;

            // テキスト設定
            Text text = GetComponentInChildren<Text>();
            if (text != null)
            {
                text.text = kanji;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            // 元の親と位置を記録
            originalParent = transform.parent;
            originalPosition = rectTransform.anchoredPosition;

            // ドラッグ中はCanvas直下に移動（最前面に表示）
            transform.SetParent(canvas.transform);
            transform.SetAsLastSibling();

            // レイキャストを無効化（下のスロットにドロップイベントが届くように）
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0.8f;
        }

        public void OnDrag(PointerEventData eventData)
        {
            // マウスに追従
            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1f;

            // スロットにドロップされなかった場合は手札に戻す
            if (transform.parent == canvas.transform)
            {
                ReturnToHand();
            }
        }

        /// <summary>
        /// 手札エリアに戻す
        /// </summary>
        public void ReturnToHand()
        {
            if (CraftingUI.Instance != null)
            {
                transform.SetParent(CraftingUI.Instance.HandArea);
                canvasGroup.blocksRaycasts = true;
                canvasGroup.alpha = 1f;
            }
        }
    }
}
