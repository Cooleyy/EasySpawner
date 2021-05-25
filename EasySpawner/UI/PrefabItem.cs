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
    }
}
