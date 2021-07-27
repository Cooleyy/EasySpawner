using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EasySpawner.UI
{
    public class PrefabItem : MonoBehaviour
    {
        public RectTransform rectTransform;
        public Toggle toggle;
        public Text label;
        public int posIndex = -1;

        private Toggle favourite;

        private Color starOnColor = new Color(1f, 200f / 255f, 41f / 255f);
        private Color starOffColor = new Color(154f / 255f, 154f / 255f, 154f / 255f);

        public void SetupTemplate(Transform template)
        {
            rectTransform = template.GetComponent<RectTransform>();
            toggle = template.GetComponent<Toggle>();
            label = template.transform.Find("ItemLabel").GetComponent<Text>();
            favourite = template.transform.Find("Star").GetComponent<Toggle>();
            template.gameObject.SetActive(false);
        }

        public void Init(Action<PrefabItem> selectPrefabCall, Action<bool, PrefabItem> favouritePrefabCall)
        {
            toggle.isOn = false;
            toggle.onValueChanged.AddListener(delegate { selectPrefabCall(this); });
            SetFavouriteOn(false, false);
            favourite.onValueChanged.AddListener(delegate(bool on) { favouritePrefabCall(on, this); });
        }

        public void SetFavouriteOn(bool on, bool silent)
        {
            if (silent)
                favourite.SetIsOnWithoutNotify(on);
            else
                favourite.isOn = on;

            favourite.targetGraphic.color = on ? starOnColor : starOffColor;
        }

        public void Pool()
        {
            gameObject.SetActive(false);
            posIndex = -1;
            toggle.SetIsOnWithoutNotify(false);
            SetFavouriteOn(false, true);
        }
    }
}
