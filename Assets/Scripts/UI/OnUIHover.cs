using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class OnUIHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	public UnityEvent onHover;
	public UnityEvent onHoverExit;

	public void OnPointerEnter(PointerEventData eventData)
	{
		onHover.Invoke();
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		onHoverExit.Invoke();
	}
}
