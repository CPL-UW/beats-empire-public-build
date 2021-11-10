using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using BeauRoutine;
using UnityEngine.UI;
using TMPro;

public class UpgradeCardControl : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	public int currUpgradeLevel = 1;
	public TextMeshProUGUI cardUpperText;
	public TextMeshProUGUI cardLowerText;
	public TextMeshProUGUI[] arrowText;
	public Image bgColor;
	public MarketingView marketingView;
	public Animator cardAnimator;

	private Routine m_routine;
	private Routine m_colorRoutine;

	public void OnPointerEnter(PointerEventData eventData)
	{
		OnHover(true);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		OnHover(false);
	}

	public void OnHover(bool on)
	{
		if(on)
		{
			cardAnimator.SetBool("Hover", true);
		}

		else
		{
			cardAnimator.SetBool("Hover", false);
		}
	}

	public void SetColor(Color c)
	{
		m_colorRoutine.Replace(this, SetColorRoutine(c));
	}

	private IEnumerator SetColorRoutine(Color c)
	{
		yield return bgColor.ColorTo(c, 0.2f);
	}

	public void SetUpperCardText(string text)
	{
		cardUpperText.text = text;
	}

	public void SetLowerCardText(string text)
	{
		cardLowerText.text = text;
	}

	public void SetArrowText(int index, string text)
	{
		arrowText[index].text = text;
	}

	public void UpgradeImmediately(int upgradeLevel)
	{
		switch (upgradeLevel)
		{
			case 0:
				cardAnimator.SetInteger("ArrowTilt", 1);
				cardAnimator.SetInteger("Arrow1Active", 0);
				cardAnimator.SetInteger("Arrow2Active", 0);
				cardAnimator.SetInteger("Arrow3Active", 0);
				break;
			case 1:
				cardAnimator.SetInteger("ArrowTilt", 2);
				cardAnimator.SetInteger("Arrow1Active", 1);
				cardAnimator.SetInteger("Arrow2Active", 0);
				cardAnimator.SetInteger("Arrow3Active", 0);
				break;
			case 2:
				cardAnimator.SetInteger("ArrowTilt", 3);
				cardAnimator.SetInteger("Arrow1Active", 1);
				cardAnimator.SetInteger("Arrow2Active", 1);
				cardAnimator.SetInteger("Arrow3Active", 0);
				break;
			case 3:
				cardAnimator.SetInteger("ArrowTilt", 0);
				cardAnimator.SetInteger("Arrow1Active", 1);
				cardAnimator.SetInteger("Arrow2Active", 1);
				cardAnimator.SetInteger("Arrow3Active", 1);
				break;
		}
		currUpgradeLevel = upgradeLevel;
	}

	public void UpgradeAnim(int cardID)
	{
		if(marketingView.AttemptUnlock(cardID))
			m_routine.Replace(this, UpgradeThenSetUpgraded());
		else
			cardAnimator.SetTrigger("LowMoney");
	}

	private IEnumerator UpgradeThenSetUpgraded()
	{
		switch (currUpgradeLevel)
		{
			case 0:
				cardAnimator.SetInteger("ArrowTilt", 1);
				cardAnimator.SetInteger("Arrow1Active", 2);
				cardAnimator.SetInteger("Arrow2Active", 0);
				cardAnimator.SetInteger("Arrow3Active", 0);
				break;
			case 1:
				cardAnimator.SetInteger("ArrowTilt", 2);
				cardAnimator.SetInteger("Arrow1Active", 1);
				cardAnimator.SetInteger("Arrow2Active", 2);
				cardAnimator.SetInteger("Arrow3Active", 0);
				break;
			case 2:
				cardAnimator.SetInteger("ArrowTilt", 3);
				cardAnimator.SetInteger("Arrow1Active", 1);
				cardAnimator.SetInteger("Arrow2Active", 1);
				cardAnimator.SetInteger("Arrow3Active", 2);
				break;
			case 3:
				cardAnimator.SetInteger("ArrowTilt", 0);
				cardAnimator.SetInteger("Arrow1Active", 1);
				cardAnimator.SetInteger("Arrow2Active", 1);
				cardAnimator.SetInteger("Arrow3Active", 1);
				break;
		}
		currUpgradeLevel++;

		yield return 0.33f;

		switch (currUpgradeLevel-1)
		{
			case 0:
				cardAnimator.SetInteger("ArrowTilt", 2);
				cardAnimator.SetInteger("Arrow1Active", 1);
				cardAnimator.SetInteger("Arrow2Active", 0);
				cardAnimator.SetInteger("Arrow3Active", 0);
				break;
			case 1:
				cardAnimator.SetInteger("ArrowTilt", 3);
				cardAnimator.SetInteger("Arrow1Active", 1);
				cardAnimator.SetInteger("Arrow2Active", 1);
				cardAnimator.SetInteger("Arrow3Active", 0);
				break;
			case 2:
				cardAnimator.SetInteger("ArrowTilt", 0);
				cardAnimator.SetInteger("Arrow1Active", 1);
				cardAnimator.SetInteger("Arrow2Active", 1);
				cardAnimator.SetInteger("Arrow3Active", 1);
				break;
			case 3:
				cardAnimator.SetInteger("ArrowTilt", 0);
				cardAnimator.SetInteger("Arrow1Active", 1);
				cardAnimator.SetInteger("Arrow2Active", 1);
				cardAnimator.SetInteger("Arrow3Active", 1);
				break;
		}
		
	}
}
