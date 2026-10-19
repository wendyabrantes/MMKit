using System;
using System.Collections;
using TMPro;
using UnityEngine;

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

        protected override void Build()
        {
            MakeCard();
            var cfg = MMKit.Config;

            KitUI.Label(Card, title ?? MMKit.T("kit.gate.title", "Grown-ups only"), 52, cfg.TextColor,
                FontStyles.Bold, preferredHeight: 130f);
            KitUI.Label(Card, MMKit.T("kit.gate.hint", "Solve this to continue"), 34, cfg.Muted,
                preferredHeight: 50f);

            problem = KitUI.Label(Card, "", 78, cfg.TextColor, FontStyles.Bold, preferredHeight: 130f);

            var pad = KitUI.Grid(Card, 3, new Vector2(230, 120), 18f);
            for (int d = 1; d <= 9; d++)
            {
                int digit = d;
                AddKey(pad.transform, digit.ToString(), cfg.Secondary, () => OnDigit(digit));
            }
            AddKey(pad.transform, "<", cfg.Muted, OnDelete);
            AddKey(pad.transform, "0", cfg.Secondary, () => OnDigit(0));
            AddKey(pad.transform, MMKit.T("kit.common.cancel", "Cancel"), cfg.Muted, Cancel, 34f);

            NewProblem();
        }

        void AddKey(Transform parent, string text, Color color, Action click, float size = 56f)
            => KitUI.MakeButton(parent, text, color, click, size);

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
            problem.color = tint ?? MMKit.Config.TextColor;
        }

        void OnDigit(int d)
        {
            if (busy || input.Length >= 2) return;
            input += d;
            Refresh();
            if (input.Length == 2) StartCoroutine(Validate());
        }

        void OnDelete()
        {
            if (busy || input.Length == 0) return;
            input = input.Substring(0, input.Length - 1);
            Refresh();
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
