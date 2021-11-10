using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropdownToggle : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
{
	public Text labelText;
	public Toggle toggle;

	void Start()
	{
		if (toggle.isOn)
			labelText.color = Color.black;
	}
	public void OnPointerClick(PointerEventData eventData)
	{

	}

	public void OnPointerEnter(PointerEventData data)
	{

	}
}
