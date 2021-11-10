using System.Collections;
using UnityEngine;
using TMPro;

public class SongReleaseTraitController : MonoBehaviour {
	public GameObject predictionRoot;
	public GameObject popularImage;
	public GameObject trendingImage;
	public TextMeshProUGUI label;

	public void ShowPrediction(MarketingInsights.InsightType insightType)
	{
		predictionRoot.SetActive(true);
		popularImage.SetActive(insightType == MarketingInsights.InsightType.MostPopular);
		trendingImage.SetActive(insightType == MarketingInsights.InsightType.IsTrending);
		label.color = Color.black;
	}

	public void HidePrediction()
	{
		label.color = Color.white;
		predictionRoot.SetActive(false);
	}
}
