using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HeatGraph : MonoBehaviour
{
	public Image[] locationImage;
	public Button[] locationButton;
	public TextMeshProUGUI[] locationTexts;
	public Image locationBgImage;
	public TextMeshProUGUI[] doesntCareTexts;

	public LocationButtonHelper miniLocButtons;
	public GameObject[] selectedImages;
	public TextMeshProUGUI[] populationText;
	public GameObject[] percentListeningObject;
	public TextMeshProUGUI[] percentListeningNumber;

	public Image[] lockedImages;
	public Color disabledColor;
	public List<GameObject> NormalLegendEntries;
	public GameObject LegendHeatmap;

	public TextMeshProUGUI LowestHeatmapNum;
	public TextMeshProUGUI HighestHeatmapNum;
	public Image heatmapLowImage;
	public Image heatmapHighImage;
	public GraphManager graphManager;
	public StatList genreList;
	public StatList moodList;
	public StatList topicList;

	public enum ColorType
	{
		Normal=0,
		Light,
		Dark
	}

	public void SetupNewGraphView(DataSimulationManager allData, StatSubType subTypeToGraph, StatSubType selectedLocation, int pannedWeek)
	{
		// Same order as LocationButtonHelper
		List<StatSubType> locations = new List<StatSubType>{ StatSubType.TURTLE_HILL, StatSubType.BOOKLINE, StatSubType.THE_BRONZ, StatSubType.IRONWOOD, StatSubType.KINGS_ISLE, StatSubType.MADHATTER };

		Dictionary<StatSubType, float> locationValues = new Dictionary<StatSubType, float>();
		Color graphColor = Color.white;
		foreach (StatSubType loc in locations)
		{
			if (GameRefs.I.m_marketingView.IsBoroughUnlocked(loc))
			{
				percentListeningObject[LocationIndex(loc)].SetActive(true);

				lockedImages[LocationIndex(loc)].gameObject.SetActive(false);
				locationButton[LocationIndex(loc)].interactable = GameRefs.I.m_dataSimulationManager.DataSimulationVariables.getBoroughInterests(loc)[subTypeToGraph.SuperType == StatType.GENRE ? 0 : subTypeToGraph.SuperType == StatType.MOOD ? 1 : 2]; ;
			}
			else
			{
				lockedImages[LocationIndex(loc)].gameObject.SetActive(true);
				locationButton[LocationIndex(loc)].interactable = false;
			}

			if (loc == selectedLocation)
				selectedImages[LocationIndex(loc)].SetActive(true);
			else
				selectedImages[LocationIndex(loc)].SetActive(false);

			Dictionary<StatSubType, List<float>> data = allData.GetCachedIndustryLocationData(loc);

			foreach (KeyValuePair<StatSubType, List<float>> kvp in data)
			{
				if (kvp.Key == subTypeToGraph)
				{
					locationValues.Add(loc, kvp.Value[pannedWeek]);
					continue;
				}
			}

			foreach(KeyValuePair<StatSubType, float> kvp in locationValues)
			{
				graphColor = subTypeToGraph.GraphColor;
				float percentFull = kvp.Value / 1250000f;

				//Debug.LogFormat("Loc: {2} Raw: {1} Val: {0}", percentFull, kvp.Value, kvp.Key.Name);

				bool doesntCare = !GameRefs.I.m_dataSimulationManager.DataSimulationVariables.getBoroughInterests(kvp.Key)[subTypeToGraph.SuperType == StatType.GENRE ? 0 : subTypeToGraph.SuperType == StatType.MOOD ? 1 : 2];
				locationImage[LocationIndex(kvp.Key)].color = doesntCare ? disabledColor : (GameRefs.I.m_marketingView.IsBoroughUnlocked(kvp.Key) ? 
					(1 - Mathf.Clamp01(percentFull)) * SubTypeToColor(subTypeToGraph, ColorType.Light) + Mathf.Clamp01(percentFull) * SubTypeToColor(subTypeToGraph, ColorType.Dark):
					disabledColor);

				locationTexts[LocationIndex(kvp.Key)].color = doesntCare ? SubTypeToColor(subTypeToGraph, ColorType.Dark) : (GameRefs.I.m_marketingView.IsBoroughUnlocked(kvp.Key) ?
					Color.white : SubTypeToColor(subTypeToGraph, ColorType.Dark));

				percentListeningNumber[LocationIndex(loc)].gameObject.SetActive(GameRefs.I.m_marketingView.IsBoroughUnlocked(loc) && !doesntCare);
				populationText[LocationIndex(loc)].color = (!GameRefs.I.m_marketingView.IsBoroughUnlocked(kvp.Key) || doesntCare) ? SubTypeToColor(subTypeToGraph, ColorType.Dark) : Color.white;
				percentListeningNumber[LocationIndex(loc)].color = (doesntCare || !GameRefs.I.m_marketingView.IsBoroughUnlocked(loc)) ? SubTypeToColor(subTypeToGraph, ColorType.Dark) : Color.white;
				percentListeningNumber[LocationIndex(loc)].text = string.Format("{0:0}%", 100.0f * Mathf.Clamp01(percentFull));
				lockedImages[LocationIndex(kvp.Key)].color = SubTypeToColor(subTypeToGraph, ColorType.Dark);
				doesntCareTexts[LocationIndex(kvp.Key)].text = doesntCare ? string.Format("{0} doesn't care about {1}.", kvp.Key.Name, subTypeToGraph.SuperType.Name) : "";
				doesntCareTexts[LocationIndex(kvp.Key)].color = SubTypeToColor(subTypeToGraph, ColorType.Dark);
			}

			locationBgImage.color = SubTypeToColor(subTypeToGraph, ColorType.Dark);
		}

		for(int i = 0; i < NormalLegendEntries.Count; i++)
		{
			NormalLegendEntries[i].SetActive(false);
		}
		LegendHeatmap.SetActive(true);
		LowestHeatmapNum.text = "0%";
		HighestHeatmapNum.text = "100%";
		heatmapLowImage.color = SubTypeToColor(subTypeToGraph, ColorType.Light);
		heatmapHighImage.color = SubTypeToColor(subTypeToGraph, ColorType.Dark);
	}

	private Color SubTypeToColor(StatSubType type, ColorType color)
	{
		if (type.SuperType == StatType.GENRE)
		{
			for(int i = 0; i < genreList.Contents.Count; i++)
			{
				if(genreList.Contents[i].statSubType == type)
				{
					return color == ColorType.Normal ? genreList.Contents[i].Color : (color == ColorType.Dark ? genreList.Contents[i].DarkColor : genreList.Contents[i].LightColor);
				}
			}
		}
		else if (type.SuperType == StatType.MOOD)
		{
			for (int i = 0; i < moodList.Contents.Count; i++)
			{
				if (moodList.Contents[i].statSubType == type)
				{
					return color == ColorType.Normal ? moodList.Contents[i].Color : (color == ColorType.Dark ? moodList.Contents[i].DarkColor : moodList.Contents[i].LightColor);
				}
			}
		}
		else if(type.SuperType == StatType.TOPIC)
		{
			for (int i = 0; i < topicList.Contents.Count; i++)
			{
				if (topicList.Contents[i].statSubType == type)
				{
					return color == ColorType.Normal ? topicList.Contents[i].Color : (color == ColorType.Dark ? topicList.Contents[i].DarkColor : topicList.Contents[i].LightColor);
				}
			}
		}

		return Color.magenta;
	}

	private int LocationIndex(StatSubType loc)
	{
		if (loc == StatSubType.TURTLE_HILL)
			return 0;
		else if (loc == StatSubType.BOOKLINE)
			return 1;
		else if (loc == StatSubType.THE_BRONZ)
			return 2;
		else if (loc == StatSubType.IRONWOOD)
			return 3;
		else if (loc == StatSubType.KINGS_ISLE)
			return 4;
		else if (loc == StatSubType.MADHATTER)
			return 5;

		return -1;
	}

	public void OnGraphButtonClicked(int loc)
	{
		for(int i = 0; i < locationButton.Length; i++)
		{
			if (i == loc)
				selectedImages[i].SetActive(true);
			else
				selectedImages[i].SetActive(false);
		}


		miniLocButtons.OnBouroughSelected(loc);
	}

}