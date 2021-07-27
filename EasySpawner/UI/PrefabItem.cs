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

        private Color starOnColor = new Color(1f, 200f / 255f, 41f / 255f);
        private Color starOffColor = new Color(154f / 255f, 154f / 255f, 154f / 255f);

        public void SetFavouriteOn(bool on, bool silent)
        {
            if (silent)
                favourite.SetIsOnWithoutNotify(on);
            else
                favourite.isOn = on;

            favourite.targetGraphic.color = on ? starOnColor : starOffColor;
        }
    }
}
