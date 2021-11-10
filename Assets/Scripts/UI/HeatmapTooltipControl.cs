using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BeauRoutine;
using TMPro;
using System.Globalization;

public class HeatmapTooltipControl : MonoBehaviour
{
	public CanvasGroup tooltipAlpha;
	public TextMeshProUGUI boroughName;
	public TextMeshProUGUI populationValue;
	public GameObject caresAboutGenre;
	public GameObject caresAboutMood;
	public GameObject caresAboutTopic;

	private Routine m_routine;

	public void ShowTooltip(StatSubType location)
	{
		if (location == StatSubType.NONE)
		{
			boroughName.text = "Choose a Borough";
			m_routine.Replace(this, FadeTooltip(false));
		}
		else
		{
			List<PopulationData> data = GameRefs.I.m_gameController.DataManager.GetPopulationData();

			// Get population
			for (int i = 0; i < data.Count; i++)
			{
				if (data[i].Location.ID == location.ID)
				{
					populationValue.text = data[i].GetPopulation().ToString("N0", CultureInfo.InvariantCulture);
					break;
				}
			}

			boroughName.text = location.Name;

			DataSimulationVariables sim = GameRefs.I.m_dataSimulationManager.DataSimulationVariables;
			// Cares about and trend speed will be handled manually
			switch (location.ID)
			{
				case StatSubType.BOOKLINE_ID:
					caresAboutGenre.SetActive(sim.BooklineCaresAbout[0]);
					caresAboutMood.SetActive(sim.BooklineCaresAbout[1]);
					caresAboutTopic.SetActive(sim.BooklineCaresAbout[2]);
					break;
				case StatSubType.IRONWOOD_ID:
					caresAboutGenre.SetActive(sim.IronwoodCaresAbout[0]);
					caresAboutMood.SetActive(sim.IronwoodCaresAbout[1]);
					caresAboutTopic.SetActive(sim.IronwoodCaresAbout[2]);
					break;
				case StatSubType.MADHATTER_ID:
					caresAboutGenre.SetActive(sim.MadhatterCaresAbout[0]);
					caresAboutMood.SetActive(sim.MadhatterCaresAbout[1]);
					caresAboutTopic.SetActive(sim.MadhatterCaresAbout[2]);
					break;
				case StatSubType.TURTLE_HILL_ID:
					caresAboutGenre.SetActive(sim.TurtleHillCaresAbout[0]);
					caresAboutMood.SetActive(sim.TurtleHillCaresAbout[1]);
					caresAboutTopic.SetActive(sim.TurtleHillCaresAbout[2]);
					break;
				case StatSubType.KINGS_ISLE_ID:
					caresAboutGenre.SetActive(sim.KingsIsleCaresAbout[0]);
					caresAboutMood.SetActive(sim.KingsIsleCaresAbout[1]);
					caresAboutTopic.SetActive(sim.KingsIsleCaresAbout[2]);
					break;
				case StatSubType.THE_BRONZ_ID:
					caresAboutGenre.SetActive(sim.BronzCaresAbout[0]);
					caresAboutMood.SetActive(sim.BronzCaresAbout[1]);
					caresAboutTopic.SetActive(sim.BronzCaresAbout[2]);
					break;
				default: break;
			}

			m_routine.Replace(this, FadeTooltip(true));
		}

	}

	private IEnumerator FadeTooltip(bool fadeIn)
	{
		tooltipAlpha.blocksRaycasts = fadeIn;
		yield return tooltipAlpha.FadeTo(fadeIn ? 1f : 0f, 0.2f);
	}
}
