using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace EasySpawner.UI {
    public static class Style {
        private static Font averiaSerif;
        private static Font averiaSerifBold;

        public static void ApplyAll(GameObject root, EasySpawnerMenu menu) {
            FindFonds();
            ApplyPanel(root.GetComponent<Image>());

            foreach (Text text in root.GetComponentsInChildren<Text>(true))
                ApplyText(text, new Color(219f / 255f, 219f / 255f, 219f / 255f));

            foreach (InputField inputField in root.GetComponentsInChildren<InputField>(true))
                ApplyInputField(inputField);

            foreach (Button button in root.GetComponentsInChildren<Button>(true))
                ApplyButton(button);

            foreach (Toggle toggle in root.GetComponentsInChildren<Toggle>(true))
                ApplyToogle(toggle);

            foreach (ScrollRect scrollRect in root.GetComponentsInChildren<ScrollRect>(true))
                ApplyScrollRect(scrollRect);

            foreach (Dropdown dropdown in root.GetComponentsInChildren<Dropdown>(true))
                ApplyDropdown(dropdown);
        }

        public static void ApplyPanel(Image image) {
            image.sprite = GetSprite("woodpanel_settings");
        }

        public static void ApplyText(Text text, Color color) {
            if (text.fontStyle == FontStyle.Bold)
            {
                text.font = averiaSerifBold;
                text.fontStyle = FontStyle.Normal;
            }
            else
            {
                text.font = averiaSerif;
            }

            text.color = color;
        }

        public static void ApplyInputField(InputField inputField) {
            inputField.GetComponent<Image>().sprite = GetSprite("text_field");
        }

        public static void ApplyButton(Button button) {
            button.GetComponent<Image>().sprite = GetSprite("button");
        }

        public static void ApplyToogle(Toggle toggle) {
            switch (toggle.gameObject.name)
            {
                case "_Template":
                case "_Template(Clone)":
                    ((Image)toggle.targetGraphic).sprite = GetSprite("button");
                    ((Image)toggle.targetGraphic).type = Image.Type.Sliced;
                    ((Image)toggle.targetGraphic).pixelsPerUnitMultiplier = 2f;
                    break;
                case "Star":
                case "Star(Clone)":
                    break;
                default:
                    ((Image)toggle.targetGraphic).sprite = GetSprite("checkbox");
                    ((Image)toggle.targetGraphic).type = Image.Type.Sliced;
                    ((Image)toggle.targetGraphic).pixelsPerUnitMultiplier = 2f;
                    break;
            }

            if (toggle.graphic != null)
            {
                toggle.graphic.GetComponent<Image>().color = new Color(1f, 0.678f, 0.103f, 1f);
                toggle.graphic.GetComponent<Image>().sprite = GetSprite("checkbox_marker");
                toggle.graphic.GetComponent<Image>().maskable = true;
            }
        }

        public static void ApplyScrollRect(ScrollRect scrollRect) {
            scrollRect.GetComponent<Image>().sprite = GetSprite("item_background_sunken");

            if ((bool)scrollRect.horizontalScrollbar)
            {
                ((Image)scrollRect.horizontalScrollbar.targetGraphic).sprite = GetSprite("text_field");
                ((Image)scrollRect.horizontalScrollbar.targetGraphic).color = Color.grey;
                ((Image)scrollRect.horizontalScrollbar.targetGraphic).type = Image.Type.Sliced;
                ((Image)scrollRect.horizontalScrollbar.targetGraphic).pixelsPerUnitMultiplier = 2f;
                scrollRect.horizontalScrollbar.GetComponent<Image>().sprite = GetSprite("text_field");
                scrollRect.horizontalScrollbar.GetComponent<Image>().pixelsPerUnitMultiplier = 3f;
            }

            if ((bool)scrollRect.verticalScrollbar)
            {
                ((Image)scrollRect.verticalScrollbar.targetGraphic).sprite = GetSprite("text_field");
                ((Image)scrollRect.verticalScrollbar.targetGraphic).color = Color.grey;
                ((Image)scrollRect.verticalScrollbar.targetGraphic).type = Image.Type.Sliced;
                ((Image)scrollRect.verticalScrollbar.targetGraphic).pixelsPerUnitMultiplier = 2f;
                scrollRect.verticalScrollbar.GetComponent<Image>().sprite = GetSprite("text_field");
                scrollRect.verticalScrollbar.GetComponent<Image>().pixelsPerUnitMultiplier = 3f;
            }
        }

        public static void ApplyDropdown(Dropdown dropdown) {
            ((Image)dropdown.targetGraphic).sprite = GetSprite("button");
            ApplyScrollRect(dropdown.template.GetComponent<ScrollRect>());
        }

        public static Sprite GetSprite(string name) {
            return Resources.FindObjectsOfTypeAll<Sprite>().FirstOrDefault(x => x.name == name);
        }

        private static void FindFonds() {
            Font[] fonts = Resources.FindObjectsOfTypeAll<Font>();
            averiaSerif = fonts.FirstOrDefault(x => x.name == "AveriaSerifLibre-Regular");
            averiaSerifBold = fonts.FirstOrDefault(x => x.name == "AveriaSerifLibre-Bold");
        }
    }
}
