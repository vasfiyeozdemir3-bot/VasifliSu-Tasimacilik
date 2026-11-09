using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace VasifliSu
{
    /// <summary>
    /// Arayüz elemanlarını koddan, marka stiline uygun üreten yardımcı sınıf.
    /// Editör sahne oluşturucu tarafından kullanılır; üretilen UI sahneye kaydedilir.
    /// </summary>
    public static class UIFactory
    {
        static GameAssets A => GameAssets.I;

        public static Canvas NewCanvas(string name, int order = 0)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var c = go.GetComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = order;
            var s = go.GetComponent<CanvasScaler>();
            s.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            s.referenceResolution = new Vector2(1920, 1080);
            s.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            return c;
        }

        public static void NewEventSystem()
        {
            var go = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(InputSystemUIInputModule));
        }

        public static RectTransform Rect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = 5;
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            return rt;
        }

        /// <summary>anchor: (0,0)=sol-alt (1,1)=sağ-üst.</summary>
        public static RectTransform Place(RectTransform rt, Vector2 anchor, Vector2 pos, Vector2 size)
        {
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = anchor;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return rt;
        }

        public static RectTransform Stretch(RectTransform rt, float padding = 0f)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(padding, padding);
            rt.offsetMax = new Vector2(-padding, -padding);
            return rt;
        }

        public static Image NewImage(string name, Transform parent, Sprite sprite, Color color, bool sliced = false)
        {
            var rt = Rect(name, parent);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            if (sliced && sprite != null) img.type = UnityEngine.UI.Image.Type.Sliced;
            img.preserveAspect = !sliced && sprite != null;
            return img;
        }

        public static Text NewText(string name, Transform parent, string text, int size, Color color,
            TextAnchor align = TextAnchor.MiddleCenter, bool bold = true)
        {
            var rt = Rect(name, parent);
            var t = rt.gameObject.AddComponent<Text>();
            t.text = text;
            t.font = bold ? A.fontBold : A.fontRegular;
            t.fontSize = size;
            t.color = color;
            t.alignment = align;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            t.supportRichText = true;
            return t;
        }

        public static Shadow AddShadow(Graphic g, float dist = 3f, float alpha = 0.35f)
        {
            var s = g.gameObject.AddComponent<Shadow>();
            s.effectColor = new Color(0.02f, 0.12f, 0.25f, alpha);
            s.effectDistance = new Vector2(dist, -dist);
            return s;
        }

        /// <summary>Marka stilinde buton. primary=mavi, değilse beyaz.</summary>
        public static Button NewButton(string name, Transform parent, string label, Vector2 size, bool primary = true, Sprite icon = null, int fontSize = 40)
        {
            var img = NewImage(name, parent, primary ? A.button : A.buttonAlt, Color.white, true);
            img.rectTransform.sizeDelta = size;
            img.raycastTarget = true;
            var b = img.gameObject.AddComponent<Button>();
            var cb = b.colors;
            cb.highlightedColor = new Color(0.92f, 0.97f, 1f);
            cb.pressedColor = new Color(0.78f, 0.88f, 0.96f);
            cb.disabledColor = new Color(0.6f, 0.6f, 0.6f, 0.7f);
            b.colors = cb;
            img.gameObject.AddComponent<ButtonFx>();

            Color tc = primary ? Color.white : Brand.TextDark;
            float textLeft = 0f;
            if (icon != null)
            {
                var ic = NewImage("Icon", img.transform, icon, tc);
                Place(ic.rectTransform, new Vector2(0f, 0.5f), new Vector2(28f, 0f), new Vector2(size.y * 0.5f, size.y * 0.5f));
                ic.rectTransform.pivot = new Vector2(0f, 0.5f);
                textLeft = size.y * 0.5f + 20f;
            }
            if (!string.IsNullOrEmpty(label))
            {
                var t = NewText("Label", img.transform, label, fontSize, tc);
                t.gameObject.AddComponent<LocalizedText>().key = label;
                Stretch(t.rectTransform);
                t.rectTransform.offsetMin = new Vector2(textLeft, 4f);
                if (primary) AddShadow(t, 2f, 0.4f);
            }
            return b;
        }

        /// <summary>Yuvarlak ikon butonu.</summary>
        public static Button IconButton(string name, Transform parent, Sprite icon, float size, Color bg)
        {
            var img = NewImage(name, parent, A.buttonRound, bg);
            img.preserveAspect = true;
            img.rectTransform.sizeDelta = new Vector2(size, size);
            img.raycastTarget = true;
            var b = img.gameObject.AddComponent<Button>();
            img.gameObject.AddComponent<ButtonFx>();
            var ic = NewImage("Icon", img.transform, icon, Color.white);
            Stretch(ic.rectTransform, size * 0.24f);
            ic.raycastTarget = false;
            return b;
        }

        public static Image Panel(string name, Transform parent, Vector2 size, bool dark = false)
        {
            var img = NewImage(name, parent, dark ? A.panelDark : A.panel, Color.white, true);
            img.rectTransform.sizeDelta = size;
            img.raycastTarget = true;
            return img;
        }

        /// <summary>Tam ekran yarı saydam karartma katmanı.</summary>
        public static Image Dim(string name, Transform parent, float alpha = 0.6f)
        {
            var img = NewImage(name, parent, null, new Color(0.02f, 0.1f, 0.22f, alpha));
            Stretch(img.rectTransform);
            img.raycastTarget = true;
            return img;
        }

        /// <summary>Yatay dolum çubuğu. Dolumu Image.fillAmount ile kontrol edilir.</summary>
        public static Image Bar(string name, Transform parent, Vector2 size, Color fill, out Image bg)
        {
            bg = NewImage(name, parent, A.barBg, Color.white, true);
            bg.rectTransform.sizeDelta = size;
            var f = NewImage("Fill", bg.transform, A.barFill, fill);
            Stretch(f.rectTransform, 6f);
            f.type = UnityEngine.UI.Image.Type.Filled;
            f.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
            f.fillOrigin = 0;
            f.fillAmount = 1f;
            f.preserveAspect = false;
            return f;
        }

        public static Slider NewSlider(string name, Transform parent, Vector2 size)
        {
            var root = Rect(name, parent);
            root.sizeDelta = size;
            var bg = NewImage("Background", root, A.barBg, Color.white, true);
            Stretch(bg.rectTransform);
            var fillArea = Rect("Fill Area", root);
            Stretch(fillArea, 6f);
            var fill = NewImage("Fill", fillArea, A.barFill, Brand.Aqua, true);
            fill.rectTransform.anchorMin = Vector2.zero; fill.rectTransform.anchorMax = new Vector2(0f, 1f);
            fill.rectTransform.sizeDelta = Vector2.zero;
            var handleArea = Rect("Handle Area", root);
            Stretch(handleArea);
            handleArea.offsetMin = new Vector2(size.y * 0.5f, 0); handleArea.offsetMax = new Vector2(-size.y * 0.5f, 0);
            var handle = NewImage("Handle", handleArea, A.buttonRound, Brand.Blue);
            handle.rectTransform.sizeDelta = new Vector2(size.y * 1.4f, 0f);
            handle.rectTransform.anchorMin = new Vector2(0f, 0f); handle.rectTransform.anchorMax = new Vector2(0f, 1f);
            handle.rectTransform.offsetMin = new Vector2(handle.rectTransform.offsetMin.x, -size.y * 0.2f);
            handle.rectTransform.offsetMax = new Vector2(handle.rectTransform.offsetMax.x, size.y * 0.2f);
            handle.raycastTarget = true;
            var s = root.gameObject.AddComponent<Slider>();
            s.fillRect = fill.rectTransform;
            s.handleRect = handle.rectTransform;
            s.targetGraphic = handle;
            s.direction = UnityEngine.UI.Slider.Direction.LeftToRight;
            s.minValue = 0f; s.maxValue = 1f; s.value = 0.5f;
            return s;
        }

        public static Toggle NewToggle(string name, Transform parent, float size)
        {
            var root = Rect(name, parent);
            root.sizeDelta = new Vector2(size, size);
            var bg = NewImage("Background", root, A.buttonRound, Color.white);
            Stretch(bg.rectTransform);
            bg.raycastTarget = true;
            var check = NewImage("Checkmark", bg.transform, A.buttonRound, Brand.Blue);
            Stretch(check.rectTransform, size * 0.18f);
            var t = root.gameObject.AddComponent<Toggle>();
            t.targetGraphic = bg;
            t.graphic = check;
            t.isOn = true;
            return t;
        }

        /// <summary>Kaydırılabilir uzun metin alanı.</summary>
        public static Text ScrollText(string name, Transform parent, Vector2 size, string content, int fontSize)
        {
            var root = Rect(name, parent);
            root.sizeDelta = size;
            var viewport = Rect("Viewport", root);
            Stretch(viewport);
            viewport.gameObject.AddComponent<RectMask2D>();
            var vpImg = viewport.gameObject.AddComponent<Image>();
            vpImg.color = new Color(1, 1, 1, 0.01f);
            var contentRt = Rect("Content", viewport);
            contentRt.anchorMin = new Vector2(0f, 1f); contentRt.anchorMax = new Vector2(1f, 1f);
            contentRt.pivot = new Vector2(0.5f, 1f);
            contentRt.offsetMin = new Vector2(10f, 0f); contentRt.offsetMax = new Vector2(-30f, 0f);
            var fitter = contentRt.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var text = contentRt.gameObject.AddComponent<Text>();
            text.text = content;
            text.font = A.fontRegular;
            text.fontSize = fontSize;
            text.color = Brand.TextDark;
            text.alignment = TextAnchor.UpperLeft;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.supportRichText = true;
            text.lineSpacing = 1.1f;
            var sr = root.gameObject.AddComponent<ScrollRect>();
            sr.viewport = viewport;
            sr.content = contentRt;
            sr.horizontal = false;
            sr.vertical = true;
            sr.movementType = ScrollRect.MovementType.Clamped;
            sr.scrollSensitivity = 40f;
            return text;
        }
    }
}
