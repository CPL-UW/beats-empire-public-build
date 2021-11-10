using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BoroughMapController : MonoBehaviour {
	public TextMeshProUGUI nameLabel;
	public Image lockImage;
	public Image outlineImage;
	public GameObject notificationRoot;
	public TextMeshProUGUI salesBonusLabel;
	public TextMeshProUGUI salesDeltaLabel;
	public TextMeshProUGUI salesUnitLabel;
	public TextMeshProUGUI fansBonusLabel;
	public TextMeshProUGUI fansDeltaLabel;
	public TextMeshProUGUI fansUnitLabel;

	public void Synchronize()
	{
		if (isSelected)
		{
			transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
			nameLabel.color = GameRefs.I.colors.heatmapSelectedBoroughColor;
			lockImage.gameObject.SetActive(false);
			if (hasPrediction)
			{
				outlineImage.sprite = GameRefs.I.songReleaseParameters.predictedOutlineSprite;
			}
			else
			{
				outlineImage.sprite = GameRefs.I.songReleaseParameters.unpredictedOutlineSprite;
			}
		}
		else
		{
			transform.localScale = new Vector3(0.8f, 0.8f, 1.0f);
			if (isLocked)
			{
				nameLabel.color = GameRefs.I.colors.heatmapLockedBoroughColor;
				lockImage.color = GameRefs.I.colors.heatmapLockedBoroughColor;
				lockImage.gameObject.SetActive(true);
			}
			else
			{
				nameLabel.color = GameRefs.I.colors.heatmapUnlockedBoroughColor;
				lockImage.gameObject.SetActive(false);
			}
		}

		notificationRoot.SetActive(!isLocked);
		if (!isLocked)
		{
			salesDeltaLabel.text = string.Format("${0:n0}", salesDelta * GameRefs.I.m_dataSimulationManager.DataSimulationVariables.CashPerSale);
			fansDeltaLabel.text = string.Format("{0}{1:n0}", fansDelta < 0 ? "" : "+", fansDelta);
		}
	}

	public void SetPrediction(MarketingInsights.InsightType predictionType, float bonus)
	{
		if (predictionType == MarketingInsights.InsightType.IsTrending)
		{
			fansBonusLabel.text = string.Format("+{0}% fans", bonus);
		}
		else
		{
			salesBonusLabel.text = string.Format("+{0}% cash", bonus);
		}
	}

	public bool hasPrediction { get; set; }
	public bool isSelected { get; set; }
	public bool isLocked { get; set; }
	public int salesDelta { get; set; }
	public int fansDelta { get; set; }
}
