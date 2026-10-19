using System;
using UnityEngine;

namespace MightyMinds.Kit
{
    /// <summary>Small white icons drawn in code (tint them with Image.color), so the kit needs no extra art.</summary>
    internal static class KitIcons
    {
        const int Size = 128;

        static Sprite star, mail, back, chevron, gear;

        public static Sprite Star    => star    ?? (star    = Make(InStar));
        public static Sprite Mail    => mail    ?? (mail    = Make(InMail));
        public static Sprite Back    => back    ?? (back    = Make(InBack));
        public static Sprite Chevron => chevron ?? (chevron = Make((x, y) => InPolygon(x, y, ChevronPoly)));
        public static Sprite Gear    => gear    ?? (gear    = MakeGear());

        // ── Shapes (unit square, y up) ────────────────────────────────────────

        static readonly Vector2[] ChevronPoly =
            { new Vector2(0.08f, 0.72f), new Vector2(0.92f, 0.72f), new Vector2(0.5f, 0.24f) };

        static readonly Vector2[] BackPoly =
        {
            new Vector2(0.06f, 0.50f), new Vector2(0.46f, 0.88f), new Vector2(0.46f, 0.64f), new Vector2(0.94f, 0.64f),
            new Vector2(0.94f, 0.36f), new Vector2(0.46f, 0.36f), new Vector2(0.46f, 0.12f)
        };

        static bool InBack(float x, float y) => InPolygon(x, y, BackPoly);

        static bool InStar(float x, float y)
        {
            var pts = new Vector2[10];
            for (int i = 0; i < 10; i++)
            {
                float r = i % 2 == 0 ? 0.48f : 0.20f;
                float a = Mathf.PI / 2f + i * Mathf.PI / 5f;
                pts[i] = new Vector2(0.5f + Mathf.Cos(a) * r, 0.53f + Mathf.Sin(a) * r);
            }
            return InPolygon(x, y, pts);
        }

        static bool InMail(float x, float y)
        {
            bool body = x > 0.05f && x < 0.95f && y > 0.20f && y < 0.80f;
            if (!body) return false;
            // Flap: a V cut out of the body
            var a = new Vector2(0.05f, 0.80f);
            var m = new Vector2(0.50f, 0.46f);
            var b = new Vector2(0.95f, 0.80f);
            var p = new Vector2(x, y);
            return DistToSegment(p, a, m) > 0.035f && DistToSegment(p, m, b) > 0.035f;
        }

        static float DistToSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            var ab = b - a;
            float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude);
            return Vector2.Distance(p, a + ab * t);
        }

        static bool InPolygon(float x, float y, Vector2[] poly)
        {
            bool inside = false;
            for (int i = 0, j = poly.Length - 1; i < poly.Length; j = i++)
            {
                if ((poly[i].y > y) != (poly[j].y > y) &&
                    x < (poly[j].x - poly[i].x) * (y - poly[i].y) / (poly[j].y - poly[i].y) + poly[i].x)
                    inside = !inside;
            }
            return inside;
        }

        // ── Rasterising ───────────────────────────────────────────────────────

        static Sprite Make(Func<float, float, bool> inside)
        {
            var px = new Color32[Size * Size];
            const int ss = 3;   // 3x3 supersampling for smooth edges
            for (int py = 0; py < Size; py++)
            for (int pxl = 0; pxl < Size; pxl++)
            {
                int hits = 0;
                for (int sy = 0; sy < ss; sy++)
                for (int sx = 0; sx < ss; sx++)
                {
                    float x = (pxl + (sx + 0.5f) / ss) / Size;
                    float y = (py + (sy + 0.5f) / ss) / Size;
                    if (inside(x, y)) hits++;
                }
                px[py * Size + pxl] = new Color(1f, 1f, 1f, hits / (float)(ss * ss));
            }
            return ToSprite(px);
        }

        static Sprite MakeGear()
        {
            const int teeth = 8;
            var px = new Color32[Size * Size];
            float c = Size / 2f;
            for (int y = 0; y < Size; y++)
            for (int x = 0; x < Size; x++)
            {
                float dx = x + 0.5f - c, dy = y + 0.5f - c;
                float r = Mathf.Sqrt(dx * dx + dy * dy);
                float angle = Mathf.Atan2(dy, dx);
                float tooth = Mathf.Clamp01((Mathf.Cos(angle * teeth) + 0.35f) * 3f);
                float outer = 38f + 22f * tooth;
                float a = Mathf.Clamp01(outer - r + 0.5f) * Mathf.Clamp01(r - 17f + 0.5f);
                px[y * Size + x] = new Color(1f, 1f, 1f, a);
            }
            return ToSprite(px);
        }

        static Sprite ToSprite(Color32[] px)
        {
            var tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                hideFlags = HideFlags.HideAndDontSave
            };
            tex.SetPixels32(px);
            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), 100f);
            sprite.hideFlags = HideFlags.HideAndDontSave;
            return sprite;
        }
    }
}
