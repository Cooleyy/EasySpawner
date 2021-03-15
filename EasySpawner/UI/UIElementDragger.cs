using UnityEngine;
using UnityEngine.EventSystems;

namespace EasySpawner.UI
{
	public class UIElementDragger : MonoBehaviour, IDragHandler, IBeginDragHandler
	{
		private Vector2 lastMousePosition;

		public void OnBeginDrag(PointerEventData eventData)
		{
			lastMousePosition = eventData.position;
		}

		public void OnDrag(PointerEventData eventData)
		{
			Vector2 currentMousePosition = eventData.position;
			Vector2 diff = currentMousePosition - lastMousePosition;
			RectTransform rect = GetComponent<RectTransform>();

			Vector3 newPosition = rect.position + new Vector3(diff.x, diff.y, transform.position.z);
			Vector3 oldPos = rect.position;
			rect.position = newPosition;
			if (!IsRectTransformInsideSreen(rect))
			{
				rect.position = oldPos;
			}
			lastMousePosition = currentMousePosition;
		}

		private bool IsRectTransformInsideSreen(RectTransform rectTransform)
		{
			bool isInside = false;
			Vector3[] corners = new Vector3[4];
			rectTransform.GetWorldCorners(corners);
			int visibleCorners = 0;
			Rect rect = new Rect(0, 0, Screen.width, Screen.height);
			foreach (Vector3 corner in corners)
			{
				if (rect.Contains(corner))
				{
					visibleCorners++;
				}
			}
			if (visibleCorners == 4)
			{
				isInside = true;
			}
			return isInside;
		}
	}
}
