using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HelpGuyController : MonoBehaviour
{
	public TextMeshProUGUI helpText;
	public Animator anim;
	public Animator characterAnimator;

	public void Speak()
	{
		characterAnimator.SetTrigger("Speak");
	}

	public void Confirm()
	{
		characterAnimator.SetTrigger("Confirm");
	}
}
