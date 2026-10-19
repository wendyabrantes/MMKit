using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MightyMinds.Kit
{
    /// <summary>
    /// Base for the kit's full-screen overlays: a dimmed backdrop plus a centred, auto-sized card.
    /// The canvas keeps its shorter side at 1080 units in both orientations, so layouts are
    /// designed once for a 1080-unit short edge.
    /// </summary>
    internal abstract class KitView : MonoBehaviour
    {
        protected const float CardWidth = 900f;

        /// <summary>Number of kit overlays currently on screen.</summary>
        internal static int OpenCount;

        CanvasScaler scaler;
        CanvasGroup group;
        bool closing;
        protected RectTransform Card { get; set; }

        protected static T Create<T>(string name) where T : KitView
        {
            KitUI.EnsureEventSystem();
            var go = new GameObject(name);
            DontDestroyOnLoad(go);

            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = MMKit.Config.SortingOrder;

            var v = go.AddComponent<T>();
            OpenCount++;
            v.scaler = go.AddComponent<CanvasScaler>();
            v.scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            v.scaler.referenceResolution = new Vector2(1080f, 1080f);
            v.scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            v.UpdateScaleMatch();
            go.AddComponent<GraphicRaycaster>();
            v.group = go.AddComponent<CanvasGroup>();
            return v;
        }

        /// <summary>Build the card contents. Call <see cref="MakeCard"/> first.</summary>
        protected abstract void Build();

        protected void Open()
        {
            Build();
            StartCoroutine(KitUI.Fade(group, 0f, 1f, 0.2f));
        }

        /// <summary>Canvas size in units (the shorter edge is always 1080).</summary>
        protected static Vector2 CanvasSize
        {
            get
            {
                float shortEdge = Mathf.Min(Screen.width, Screen.height);
                float longEdge = 1080f * Mathf.Max(Screen.width, Screen.height) / Mathf.Max(1f, shortEdge);
                return Screen.width > Screen.height ? new Vector2(longEdge, 1080f) : new Vector2(1080f, longEdge);
            }
        }

        protected Image MakeBackdrop(float alpha = 0.65f)
        {
            var backdrop = KitUI.Box("Backdrop", transform, new Color(0f, 0f, 0f, alpha), rounded: false);
            KitUI.Stretch(backdrop.rectTransform);
            return backdrop;
        }

        protected void MakeCard()
        {
            MakeBackdrop();

            var cardImg = KitUI.Box("Card", transform, MMKit.Config.CardColor);
            Card = cardImg.rectTransform;
            Card.anchorMin = Card.anchorMax = Card.pivot = new Vector2(0.5f, 0.5f);
            Card.sizeDelta = new Vector2(CardWidth, 0f);

            var layout = cardImg.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(40, 40, 40, 40);
            layout.spacing = 24f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var fitter = cardImg.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        protected void ClearCard()
        {
            for (int i = Card.childCount - 1; i >= 0; i--)
                Destroy(Card.GetChild(i).gameObject);
        }

        protected internal void Close(Action after = null)
        {
            if (closing) return;
            closing = true;
            StartCoroutine(CloseRoutine(after));
        }

        IEnumerator CloseRoutine(Action after)
        {
            yield return KitUI.Fade(group, group.alpha, 0f, 0.15f);
            Destroy(gameObject);
            after?.Invoke();
        }

        protected virtual void OnDestroy() => OpenCount--;

        protected virtual void Update() => UpdateScaleMatch();

        void UpdateScaleMatch()
        {
            // Keep the short edge at 1080 units: match height in landscape, width in portrait.
            scaler.matchWidthOrHeight = Screen.width > Screen.height ? 1f : 0f;
        }
    }
}
