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
        public int posIndex = -1;

        private Text label;
        private Toggle favourite;
        private string itemName;

        private Color starOnColor = new Color(1f, 200f / 255f, 41f / 255f);
        private Color starOffColor = new Color(154f / 255f, 154f / 255f, 154f / 255f);

        public void Init(Action<PrefabItem> selectPrefabCall, Action<bool, PrefabItem> favouritePrefabCall)
        {
            rectTransform = GetComponent<RectTransform>();
            toggle = GetComponent<Toggle>();
            label = transform.Find("ItemLabel").GetComponent<Text>();
            favourite = transform.Find("Star").GetComponent<Toggle>();

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

        public void SetName(string itemName)
        {
            this.itemName = itemName;
            string localizedName = EasySpawnerMenu.PrefabStates[itemName].localizedName;
            label.text = localizedName.Length > 0 ? $"{localizedName} ({itemName})" : itemName;
        }

        public string GetName()
        {
            return itemName;
        }
    }
}
