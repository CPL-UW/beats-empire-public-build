using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoverTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
	public UnityAction<PointerEventData> onEnter { private get; set; }
	public UnityAction<PointerEventData> onExit { private get; set; }

	public void OnPointerEnter(PointerEventData data)
	{
		if (onEnter != null)
		{
			onEnter(data);
		}
	}

	public void OnPointerExit(PointerEventData data)
	{
		if (onExit != null)
		{
			onExit(data);
		}
	}
}
