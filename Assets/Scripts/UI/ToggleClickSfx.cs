using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ToggleClickSfx : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
{
	public SfxAudio.SfxType clipToPlay;
	private Button possibleButton;
	private Toggle possibleToggle;

	public void Start()
	{
		possibleButton = GetComponent<Button>();
		possibleToggle = GetComponent<Toggle>();
	}
	public void OnPointerClick(PointerEventData eventData)
	{
		// Don't play sfx if we are attached to an actual button or toggle and it's disabled
		if (possibleButton != null && possibleButton.interactable == false)
			return;

		if (possibleToggle != null && possibleToggle.interactable == false)
			return;

		GameRefs.I.m_sfxAudio.PlaySfxClip(clipToPlay);
	}

	public void OnPointerEnter(PointerEventData data)
	{
		if (possibleButton != null && possibleButton.interactable == false)
			return;

		if (possibleToggle != null && possibleToggle.interactable == false)
			return;

		GameRefs.I.m_sfxAudio.PlaySfxClip(SfxAudio.SfxType.Hover);
	}
}
