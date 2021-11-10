using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using BeauRoutine;

public class TopChartsHudControl : MonoBehaviour {
	public GameObject[] genreRoots;
	public GameObject[] genreLabels;
	public GameObject[] neutralIcons;
	public GameObject[] goldBackgrounds;
	public GameObject[] platinumBase;
	public GameObject[] platinumFirst;
	public GameObject[] platinumSecond;
	public GameObject[] platinumThird;
	//public Image[] circleFills;

	private Routine m_routine;

	public void Start()
	{
		for (int i = 0; i < genreRoots.Length; ++i)
		{
			RegisterEvents(i);
		}
	}

	private void RegisterEvents(int i)
	{
		HoverTrigger hoverTrigger = genreRoots[i].AddComponent<HoverTrigger>();
		hoverTrigger.onEnter = data => genreLabels[i].SetActive(true);
		hoverTrigger.onExit = data => genreLabels[i].SetActive(false);
	}

	public void UpdateCircles()
	{
		StatSubType[] genreList = new StatSubType[6] { StatSubType.ROCK, StatSubType.POP, StatSubType.RANDB, StatSubType.HIP_HOP, StatSubType.RAP, StatSubType.ELECTRONIC };

		for (int i = 0; i < genreList.Length; i++)
		{
			int status = GameRefs.I.m_topCharts.GetGenreCompletionStatus(genreList[i]);
			/*if(status > 0)
			{
				goldBackgrounds[i].SetActive(true);
			}
			if(status > 1)
			{
				//circleFills[i].fillAmount = 0.333f * (status - 1);
			}*/
			if(status == 4)
			{
				neutralIcons[i].SetActive(false);
				platinumThird[i].SetActive(true);
			}
			
			else if(status == 3)
			{
				neutralIcons[i].SetActive(false);
				platinumSecond[i].SetActive(true);
			}
			else if(status == 2)
			{
				goldBackgrounds[i].SetActive(false);
				platinumBase[i].SetActive(true);
				platinumFirst[i].SetActive(true);
				neutralIcons[i].SetActive(false);
			}
			else if(status == 1)
			{
				neutralIcons[i].SetActive(false);
				goldBackgrounds[i].SetActive(true);
			}//sorry rob
		}
	}

}
