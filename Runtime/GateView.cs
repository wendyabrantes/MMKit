using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MightyMinds.Kit
{
    /// <summary>
    /// Parental gate: a two-digit addition a young child cannot solve. Three wrong answers cancel.
    /// Use <see cref="MMKit.RequireGate"/> rather than calling this directly.
    /// </summary>
    internal sealed class GateView : KitView
    {
        const int MaxFails = 3;

        string title;
        Action onPass, onCancel;
        TextMeshProUGUI problem;
        int a, b, answer, fails;
        string input = "";
        bool busy;

        public static void Show(string title, Action onPass, Action onCancel)
        {
            var v = Create<GateView>("MMKit_ParentalGate");
            v.title = title;
            v.onPass = onPass;
            v.onCancel = onCancel;
            v.Open();
        }

        // Layout and colours follow the puzzle game's parental gate prefab (850 x 850 card).
        static readonly Color ProblemColor = new Color32(0x5F, 0x37, 0x85, 255);

        protected override void Build()
        {
            MakeBackdrop(0.65f);

            var card = Sprite("Card", transform, "MMKit/gate_background", Image.Type.Sliced);
            Card = card.rectTransform;
            Card.anchorMin = Card.anchorMax = Card.pivot = new Vector2(0.5f, 0.5f);
            Card.sizeDelta = new Vector2(850f, 850f);

            // Instruction, top-left
            var instruction = KitUI.Label(Card, title ?? MMKit.T("kit.gate.hint", "Solve the problem to continue\u2026"),
                50f, Color.white, FontStyles.Bold, TextAlignmentOptions.TopLeft);
            var ir = instruction.rectTransform;
            ir.anchorMin = ir.anchorMax = ir.pivot = new Vector2(0f, 1f);
            ir.anchoredPosition = new Vector2(55f, -55f);
            ir.sizeDelta = new Vector2(565f, 136f);

            // Close button, top-right
            var close = Sprite("Close", Card, "MMKit/gate_close", Image.Type.Simple);
            var cr = close.rectTransform;
            cr.anchorMin = cr.anchorMax = cr.pivot = new Vector2(1f, 1f);
            cr.anchoredPosition = new Vector2(-55f, -55f);
            cr.sizeDelta = new Vector2(110f, 110f);
            var closeBtn = close.gameObject.AddComponent<Button>();
            closeBtn.targetGraphic = close;
            closeBtn.onClick.AddListener(Cancel);

            // The sum
            var box = Sprite("Problem", Card, "MMKit/gate_problem", Image.Type.Sliced);
            var br = box.rectTransform;
            br.anchorMin = br.anchorMax = br.pivot = new Vector2(0.5f, 0.5f);
            br.anchoredPosition = new Vector2(0f, 129f);
            br.sizeDelta = new Vector2(730f, 163f);
            problem = KitUI.Label(box.transform, "", 85f, ProblemColor, FontStyles.Bold);
            KitUI.Stretch(problem.rectTransform);

            // Digits 1-9 then 0, five to a row
            var grid = KitUI.Grid(Card, 5, new Vector2(130f, 130f), 14f);
            var gr = (RectTransform)grid.transform;
            gr.anchorMin = new Vector2(0f, 0f);
            gr.anchorMax = new Vector2(1f, 0f);
            gr.pivot = new Vector2(0.5f, 0.5f);
            gr.anchoredPosition = new Vector2(0f, 189.8f);
            gr.sizeDelta = new Vector2(0f, 379.7f);
            for (int d = 1; d <= 10; d++)
            {
                int digit = d % 10;
                var key = Sprite("Digit" + digit, grid.transform, "MMKit/gate_digit", Image.Type.Simple);
                var btn = key.gameObject.AddComponent<Button>();
                btn.targetGraphic = key;
                btn.onClick.AddListener(() => OnDigit(digit));
                var label = KitUI.Label(key.transform, digit.ToString(), 80f, Color.white, FontStyles.Bold);
                KitUI.Stretch(label.rectTransform);
            }

            NewProblem();
        }

        static Image Sprite(string name, Transform parent, string resource, Image.Type type)
        {
            var img = KitUI.Box(name, parent, Color.white, false);
            img.sprite = KitUI.LoadSprite(resource);
            img.type = type;
            return img;
        }

        void NewProblem()
        {
            a = UnityEngine.Random.Range(11, 40);
            b = UnityEngine.Random.Range(3, 10);
            answer = a + b;
            input = "";
            Refresh();
        }

        void Refresh(Color? tint = null)
        {
            string d1 = input.Length > 0 ? input.Substring(0, 1) : "_";
            string d2 = input.Length > 1 ? input.Substring(1, 1) : "_";
            problem.text = $"{a} + {b} = {d1} {d2}";
            problem.color = tint ?? ProblemColor;
        }

        void OnDigit(int d)
        {
            if (busy || input.Length >= 2) return;
            input += d;
            Refresh();
            if (input.Length == 2) StartCoroutine(Validate());
        }

        void Cancel()
        {
            if (busy) return;
            busy = true;
            Close(onCancel);
        }

        IEnumerator Validate()
        {
            busy = true;
            bool ok = int.TryParse(input, out int n) && n == answer;
            Refresh(ok ? MMKit.Config.Positive : new Color(0.85f, 0.2f, 0.2f));
            yield return new WaitForSecondsRealtime(0.4f);

            if (ok) { Close(onPass); yield break; }

            fails++;
            yield return Shake();
            if (fails >= MaxFails) { Close(onCancel); yield break; }

            NewProblem();
            busy = false;
        }

        IEnumerator Shake()
        {
            var rt = problem.rectTransform;
            Vector2 origin = rt.anchoredPosition;
            for (float t = 0f; t < 0.35f; t += Time.unscaledDeltaTime)
            {
                rt.anchoredPosition = origin + new Vector2(Mathf.Sin(t * 60f) * 14f * (1f - t / 0.35f), 0f);
                yield return null;
            }
            rt.anchoredPosition = origin;
        }
    }
}
