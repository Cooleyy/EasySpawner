using UnityEngine;
using UnityEngine.EventSystems;

namespace EasySpawner.UI
{
	public class UIElementDragger : MonoBehaviour, IDragHandler, IBeginDragHandler
	{
		private RectTransform window;
		private Vector2 delta;

		private void Awake()
		{
			window = (RectTransform)transform;
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
			delta = Input.mousePosition - window.position;
		}

		public void OnDrag(PointerEventData eventData)
		{
			Vector2 pos = eventData.position - delta;
			Rect rect = window.rect;
			Vector2 lossyScale = window.lossyScale;

			float minX = rect.width / 2f * lossyScale.x;
			float maxX = Screen.width - minX;
			float minY = rect.height / 2f * lossyScale.y;
			float maxY = Screen.height - minY;

			pos.x = Mathf.Clamp(pos.x, minX, maxX);
			pos.y = Mathf.Clamp(pos.y, minY, maxY);

			transform.position = pos;
		}
	}
}
