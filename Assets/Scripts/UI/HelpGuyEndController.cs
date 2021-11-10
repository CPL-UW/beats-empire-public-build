using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using BeauRoutine;

public class HelpGuyEndController : MonoBehaviour
{
	public Animator characterAnimator;

	private bool m_playConfirm;
	private Routine m_playConfirmAnim;

	private void OnEnable()
	{
		characterAnimator.Play("Idle", 0);
		characterAnimator.Update(0);

		if (m_playConfirm)
		{
			m_playConfirmAnim.Replace(this, Routine.Delay(() => {
				characterAnimator.Play("Confirmation", 0);
			}, 0.6f));
		}
		else
		{
			m_playConfirmAnim.Stop();
		}
	}

	private void OnDisable()
	{
		m_playConfirmAnim.Stop();
	}

	public void Idle()
	{
		m_playConfirm = false;
	}

	public void Confirm()
	{
		m_playConfirm = true;
	}
}
