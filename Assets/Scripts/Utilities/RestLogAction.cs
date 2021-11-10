using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RestLogAction : MonoBehaviour, IPointerClickHandler
{

	public bool includeSystem = false;

	private Button possibleButton;
	private Toggle possibleToggle;

	public void Start()
	{
		possibleButton = GetComponent<Button>();
		possibleToggle = GetComponent<Toggle>();
	}
	public void OnPointerClick(PointerEventData eventData)
	{
		if (possibleButton != null && possibleButton.interactable == false)
			return;

		if (possibleToggle != null && possibleToggle.interactable == false)
			return;

		string action = "clickedButton";
		GameRefs.I.PostGameState(includeSystem, action, string.Format("{0}", this.gameObject.name));
	}
}
