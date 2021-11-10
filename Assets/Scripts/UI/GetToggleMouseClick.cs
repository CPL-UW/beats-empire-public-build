using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class GetToggleMouseClick : MonoBehaviour, IPointerClickHandler
{
	public int num;
	public SongRecordingPanel srp;
	public bool mood;
	public UnityEvent onClick;

	private Button attachedButton;
	private Toggle attachedToggle;

	void Start()
	{
		attachedButton = GetComponent<Button>();
		attachedToggle = GetComponent<Toggle>();
	}
	public void OnPointerClick(PointerEventData eventData)
	{
		ForceClick();
	}

	public void ForceClick()
	{
		if (attachedButton != null && !attachedButton.interactable)
			return;

		if (attachedToggle != null && !attachedToggle.interactable)
			return;

		if (srp != null)
		{
			if (mood)
				srp.OnMoodSelected(num);
			else
				srp.OnTopicSelected(num);
		}

		if (onClick != null)
			onClick.Invoke();
	}

}
