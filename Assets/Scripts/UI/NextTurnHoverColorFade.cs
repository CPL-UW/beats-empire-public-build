using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using BeauRoutine;
using UnityEngine.UI;

public class NextTurnHoverColorFade : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	private Routine routine;
	public Image element;

	public void OnPointerEnter(PointerEventData eventData)
	{
		routine.Replace(this, ColorFade(true));
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		routine.Replace(this, ColorFade(false));
	}

	private IEnumerator ColorFade(bool toWhite)
	{
		yield return element.ColorTo(toWhite ? Color.white : GameRefs.I.m_gameController.GetNextTurnColor(), 0.2f);
	}
}
