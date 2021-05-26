using System;
using UnityEngine;
using UnityEngine.UI;

namespace EasySpawner.UI
{
    public class PrefabItem : MonoBehaviour
    {
        public RectTransform rectTransform;
        public Toggle toggle;
        public Text label;
        public Toggle favourite;
        public float originalHeight;
        public bool isSearched;
        public int posIndex = -1;

        private Color starOnColor = new Color(1f, 200/255f, 41 / 255f);
        private Color starOffColor = new Color(154/255f, 154/255f,154/255f);

        private void Update()
        {
            favourite.targetGraphic.color = favourite.isOn ? starOnColor : starOffColor;
        }
    }
}
