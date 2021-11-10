using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BeauRoutine;

public class LocationButtonHelper : MonoBehaviour {

	public Color selectedColor;
	public Color deselectedColor;
	public Color lockedColor;
	public Color normalTextColor;
	public Color lockedTextColor;

	public GraphManager graphManager;

	public enum BouroughName
	{
		Turtle,
		Madhatter,
		Bronze,
		Kings,
		Bookline,
		Ironwood,
		All
	}

	public Image[] locationImages;
	public GameObject[] locationLocks;
	public GameObject[] locationTrends;
	public CanvasGroup[] locationTrendsCG;

	public Image[] trendImage;
	public Image allLocations;
	public TextMeshProUGUI[] locationTexts;
	public TextMeshProUGUI subheaderText;
	public Sprite mostPopSprite;
	public Sprite trendingSprite;

	private Vector3[] initPos;
	private Routine shakeRoutine;

	void Start()
	{
		initPos = new Vector3[6];
		for(int i = 0; i < locationImages.Length; i++)
		{
			bool isUnlocked = GameRefs.I.m_marketingView.IsBoroughUnlocked(LocationHelperToStat(i));
			locationImages[i].GetComponent<Button>().interactable = isUnlocked;
			initPos[i] = locationTrends[i].GetComponent<RectTransform>().localPosition;
			locationLocks[i].SetActive(!isUnlocked);
		}
	}

	public void SelectRandomUnlockedBorough()
	{
		OnBouroughSelected(0); // Random enough for the folks we roll with
	}

	public void DisableAllTrendIcons(MarketingInsightObject currInsight)
	{
		for(int i = 0; i < locationTrends.Length; i++)
		{
			if (locationTrends[i].activeSelf)
			{
				if (currInsight != null && LocationHelperToStat(i) == currInsight.location)
					continue;
				else
					shakeRoutine.Replace(this, ShakeAndDisappear(i));
			}
		}
	}

	private IEnumerator ShakeAndDisappear(int imageIndex)
	{
		RectTransform rt = locationTrends[imageIndex].GetComponent<RectTransform>();
		yield return rt.AnchorPosTo(rt.anchoredPosition.x + 5f, 0.4f, Axis.X).Wave(Wave.Function.SinFade, 5);
		yield return Routine.Combine(rt.AnchorPosTo(rt.anchoredPosition.y + 20f, 0.4f, Axis.Y),
			locationTrendsCG[imageIndex].FadeTo(0f, 0.4f));
		locationTrends[imageIndex].SetActive(false);
	}

	public void EnableTrendIcon(StatSubType location, bool isMostPopular)
	{
		switch(location.ID)
		{
			case StatSubType.BOOKLINE_ID: locationTrends[1].SetActive(true);
				locationTrendsCG[1].alpha = 1f;
				locationTrends[1].GetComponent<RectTransform>().localPosition = initPos[1];
				trendImage[1].sprite = isMostPopular ? mostPopSprite : trendingSprite;
				break;
			case StatSubType.MADHATTER_ID: locationTrends[5].SetActive(true);
				locationTrendsCG[5].alpha = 1f;
				locationTrends[5].GetComponent<RectTransform>().localPosition = initPos[5];
				trendImage[5].GetComponent<Image>().sprite = isMostPopular ? mostPopSprite : trendingSprite;
				break;
			case StatSubType.TURTLE_HILL_ID: locationTrends[0].SetActive(true);
				locationTrendsCG[0].alpha = 1f;
				locationTrends[0].GetComponent<RectTransform>().localPosition = initPos[0];
				trendImage[0].GetComponent<Image>().sprite = isMostPopular ? mostPopSprite : trendingSprite;
				break;
			case StatSubType.KINGS_ISLE_ID: locationTrends[4].SetActive(true);
				locationTrendsCG[4].alpha = 1f;
				locationTrends[4].GetComponent<RectTransform>().localPosition = initPos[4];
				trendImage[4].GetComponent<Image>().sprite = isMostPopular ? mostPopSprite : trendingSprite;
				break;
			case StatSubType.THE_BRONZ_ID: locationTrends[2].SetActive(true);
				locationTrendsCG[2].alpha = 1f;
				locationTrends[2].GetComponent<RectTransform>().localPosition = initPos[2];
				trendImage[2].GetComponent<Image>().sprite = isMostPopular ? mostPopSprite : trendingSprite;
				break;
			case StatSubType.IRONWOOD_ID: locationTrends[3].SetActive(true);
				locationTrendsCG[3].alpha = 1f;
				locationTrends[3].GetComponent<RectTransform>().localPosition = initPos[3];
				trendImage[3].GetComponent<Image>().sprite = isMostPopular ? mostPopSprite : trendingSprite;
				break;
		}
	}

	public void OnBouroughSelected(int bourough)
	{
		if(bourough < 6)
		{
			for(int i = 0; i < locationImages.Length; i++)
			{
				if(i == bourough)
				{
					locationImages[i].color = selectedColor;
					switch(bourough)
					{
						case 0: graphManager.OnLocationChanged(BouroughName.Turtle); subheaderText.text = "TURTLE HILL";  break;
						case 1: graphManager.OnLocationChanged(BouroughName.Bookline); subheaderText.text = "BROWER"; break;
						case 2: graphManager.OnLocationChanged(BouroughName.Bronze); subheaderText.text = "GORMAN"; break;
						case 3: graphManager.OnLocationChanged(BouroughName.Ironwood); subheaderText.text = "IRONWOOD"; break;
						case 4: graphManager.OnLocationChanged(BouroughName.Kings); subheaderText.text = "MORRIS"; break;
						case 5: graphManager.OnLocationChanged(BouroughName.Madhatter); subheaderText.text = "UPTOWN"; break;
						default: break;
					}
				}
				else
				{
					if (!GameRefs.I.m_marketingView.IsBoroughUnlocked(LocationHelperToStat(i)))
					{
						locationImages[i].color = lockedColor;
						locationTexts[i].color = lockedTextColor;
						locationLocks[i].SetActive(true);
						locationImages[i].GetComponent<Button>().interactable = false;
					}
						
					else
					{
						locationImages[i].color = deselectedColor;
						locationTexts[i].color = normalTextColor;
						locationLocks[i].SetActive(false);
						locationImages[i].GetComponent<Button>().interactable = true;
					}
						
				}
			}
			allLocations.color = deselectedColor;
		}
		else
		{
			for(int i = 0; i < locationImages.Length; i++)
			{
				if (!GameRefs.I.m_marketingView.IsBoroughUnlocked(LocationHelperToStat(i)))
				{
					locationImages[i].color = lockedColor;
					locationTexts[i].color = lockedTextColor;
					locationLocks[i].SetActive(true);
				}

				else
				{
					locationImages[i].color = selectedColor;
					locationTexts[i].color = normalTextColor;
					locationLocks[i].SetActive(false);
				}
			}
			allLocations.color = selectedColor;
			graphManager.OnLocationChanged(BouroughName.All);
			subheaderText.text = "EVERYWHERE";
		}
	}

	private StatSubType LocationHelperToStat(int borough)
	{
		switch (borough)
		{
			case 0: return StatSubType.TURTLE_HILL;
			case 1: return StatSubType.BOOKLINE;
			case 2: return StatSubType.THE_BRONZ;
			case 3: return StatSubType.IRONWOOD;
			case 4: return StatSubType.KINGS_ISLE;
			case 5: return StatSubType.MADHATTER;
			default: return StatSubType.NONE;
		}
	}
}
