using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeauRoutine;

public class NotificationButton : MonoBehaviour {

	public RectTransform buttonTrans;
	public float popInTime;
	public float popInScale;

	private Routine hoverRoutine;
	private Routine popInRoutine;

	private void Start()
	{
		Hover();
	}

	public void PopIn()
	{
		popInRoutine.Replace(this, PopInRoutine());
	}

	public void Hover()
	{
		hoverRoutine.Replace(this, HoverRoutine());
	}

	private IEnumerator HoverRoutine()
	{
		Vector3 originalPos = buttonTrans.localPosition;
		yield return buttonTrans.MoveTo(originalPos + new Vector3(0f, 6f, 0f), 0.75f, Axis.Y, Space.Self).YoyoLoop().Ease(Curve.SineInOut);
	}

	private IEnumerator PopInRoutine()
	{
		Vector3 originalScale = buttonTrans.localScale;
		buttonTrans.SetScale(popInScale);
		yield return buttonTrans.ScaleTo(1f, popInTime).Ease(Curve.BackInOut);
	}
}
