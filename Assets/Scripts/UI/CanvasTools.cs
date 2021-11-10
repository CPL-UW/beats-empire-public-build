using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class CanvasTools : MonoBehaviour
{
	CanvasGroup cg;
    public List<GameObject> objToTurnOn = new List<GameObject>();
	public List<GameObject> objToTurnOff = new List<GameObject>();
	public List<CanvasGroup> cgTurnOffTurnOn = new List<CanvasGroup>();
	public List<System.Action> OnOpenMethods = new List<System.Action>();
	public List<System.Action> OnCloseMethods = new List<System.Action>();

	void Awake ()
	{
		if (cg == null)
			cg = GetComponent<CanvasGroup> ();
	}

	public bool isOpen = false;
	public void ToggleMe (bool b)
	{
		if (b == isOpen)
			return;
		if (b) {
			cg.alpha = 1;
			cg.interactable = true;
			cg.blocksRaycasts = true;

            //toggle worldspace objects
			if (objToTurnOn.Count > 0)
            {
				for (int inc = 0; inc < objToTurnOn.Count; inc++)
                {
                    objToTurnOn[inc].SetActive(true);
                }
            }

			if (objToTurnOff.Count > 0)
			{
				for (int inc = 0; inc < objToTurnOff.Count; inc++)
				{
                    objToTurnOff[inc].SetActive(false);
				}
			}


			if (cgTurnOffTurnOn.Count > 0)
			{
				for (int inc = 0; inc < cgTurnOffTurnOn.Count; inc++)
				{
					cgTurnOffTurnOn [inc].alpha = 0;
				}
			}

			//run open actions
			ProcessOpenActions();

		} else {
			cg.alpha = 0;
			cg.interactable = false;
			cg.blocksRaycasts = false;

            //toggle worldspace objects
			if (objToTurnOn.Count > 0)
            {
				for (int inc = 0; inc < objToTurnOn.Count; inc++)
                {
                    objToTurnOn[inc].SetActive(false);
                }
            }

			if (objToTurnOff.Count > 0)
			{
				for (int inc = 0; inc < objToTurnOff.Count; inc++)
				{
                    objToTurnOff[inc].SetActive(true);
				}
			}

			if (cgTurnOffTurnOn.Count > 0)
			{
				for (int inc = 0; inc < cgTurnOffTurnOn.Count; inc++)
				{
					cgTurnOffTurnOn [inc].alpha = 1;
				}
			}
			//run close actions
			ProcessCloseActions();

        }

		isOpen = b;
	}

    //Non-Canvas elements to toggle
	public void AddObjTurnOn(GameObject obj)
    {
        objToTurnOn.Add(obj);
    }

	public void AddObjTurnOff(GameObject obj)
	{
		objToTurnOff.Add(obj);
	}

	public void AddOpenAction(System.Action sa){
		OnOpenMethods.Add (sa);
	}

	void ProcessOpenActions(){
		foreach (System.Action sa in OnOpenMethods) {
			sa ();
		}
	}

	public void AddCloseAction(System.Action sa){
		OnCloseMethods.Add (sa);
	}

	void ProcessCloseActions(){
		foreach (System.Action sa in OnCloseMethods) {
			sa ();
		}
	}

	public void CallFadeCanvas (float _desiredAlpha)
	{

		StartCoroutine (FadeCanvasGroup (_desiredAlpha));
	}

	IEnumerator FadeCanvasGroup (float _newAlpha)
	{

		float _currentAlpha = cg.alpha;

		//Fading out
		if (_newAlpha < _currentAlpha) {
			for (float alpha = _currentAlpha; alpha >= _newAlpha; alpha -= .1f) {
				yield return new WaitForSeconds (0);
				cg.alpha = alpha;
			}
		}

		//Fading in
		if (_newAlpha > _currentAlpha) {
			for (float alpha = _currentAlpha; alpha <= _newAlpha; alpha += .1f) {
				yield return new WaitForSeconds (0);
				cg.alpha = alpha;
			}
		}

		cg.alpha = _newAlpha;

		if (cg.alpha == 1) {
			cg.interactable = true;
			cg.blocksRaycasts = true;
			isOpen = true;
		} else if (cg.alpha == 0) {
			cg.interactable = false;
			cg.blocksRaycasts = false;
			isOpen = false;
		}
	}



}
