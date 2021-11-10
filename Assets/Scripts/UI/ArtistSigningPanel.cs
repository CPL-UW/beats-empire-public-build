using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using TMPro;
using Utility;
using System.Globalization;
using BeauRoutine;

public class ArtistSigningPanel : ArtistViewPanel
{
    public ArtistSigningView ArtistSigningView;
	public Colors colors;

    public Color KnownTraitColor;
    public Color UnknownTraitColor;
    public List<Image> Moods, Topics;
    public TextMeshProUGUI SigningCostLabel;
	public TextMeshProUGUI ArtistCostText;
	public TextMeshProUGUI pendingSalaryText;

    public Image SignedBorder;
    public GameObject NewArtistNotification;
    public GameObject PotentialHire;
    public GameObject ExistingArtist;

	[Header("Upgrade Objects")]
	public Button UpgradeButton;
    public GameObject UpgradeArtist;
	public GameObject UpgradeCost;
	public RectTransform UpgradeCostTransform;
    public ToggleGroup upgradeToggleGroup;
    public TextMeshProUGUI upgradeCostText;
	public TextMeshProUGUI upgradeButtonText;
	public TextMeshProUGUI cancelButtonText;
	public Color disabledUpgradeButtonTextColor;
	public Color activeUpgradeButtonTextColor;

    public GameObject[] moodUpgrades;
    public GameObject[] topicUpgrades;
    public TextMeshProUGUI[] moodTexts;
    public TextMeshProUGUI[] topicTexts;

    public UpgradeType ambitionUpgrade;
    public UpgradeType reliabilityUpgrade;
    public UpgradeType speedUpgrade;
    public UpgradeType persistenceUpgrade;

	private bool purchasedUpgrade = false;
    private const int MAX_STAT_RANK = 5;
	private string lastState = "existing";
	private Routine shakeRoutine;
	private Vector2 originalCostPos;

	public override void AssignBand(Band band)
    {
        base.AssignBand(band);

        this.SigningCostLabel.SetIText(string.Format("${0:n0} / WEEK", band.UpkeepCost));
		this.ArtistCostText.SetIText(string.Format("${0:n0} / WEEK", band.UpkeepCost));

		List<StatSubType> bandKnownTraits = this.currentBand.GetKnownTraits();

		Color knownTraitColor = currentBand.IsSigned ? colors.hiredTraitColor : colors.unhiredTraitColor;

        foreach (KeyValuePair<int, StatSubType> kvp in this.moodLookup)
        {
            this.Moods[kvp.Key].color = bandKnownTraits.Contains(kvp.Value) ? knownTraitColor : this.UnknownTraitColor;
        }

        foreach (KeyValuePair<int, StatSubType> kvp in this.topicLookup)
        {
            this.Topics[kvp.Key].color = bandKnownTraits.Contains(kvp.Value) ? knownTraitColor : this.UnknownTraitColor;
        }

        this.SignedBorder.gameObject.SetActive(band.IsSigned);

        SetSigningPanelsate(base.currentBand.IsSigned ? lastState : "potential");

        this.NewArtistNotification.SetActive(band.IsNew);

        band.IsNew = false;
		originalCostPos = UpgradeCostTransform.anchoredPosition;
	}

	public void CheckToggleStates()
	{
		if (lastState == "existing")
			return;

		else if(lastState == "upgrade" || lastState == "upgradeReady")
		{
			if (upgradeToggleGroup.AnyTogglesOn())
				SetSigningPanelsate("upgradeReady");
			else
				SetSigningPanelsate("upgrade");
		}
	}

    public void SetSigningPanelsate(string state)
    {
        switch(state)
        {
            case "existing":
                PotentialHire.SetActive(false);
                UpgradeArtist.SetActive(false);
                ExistingArtist.SetActive(true);
				lastState = state;
				ArtistSigningView.animator.SetBool("UpgradesSalaryAlert", false);
				SetUpgradePanel(false);
				break;

            case "potential":
                PotentialHire.SetActive(true);
                ExistingArtist.SetActive(false);
                UpgradeArtist.SetActive(false);
				lastState = state;
				ArtistSigningView.animator.SetBool("UpgradesSalaryAlert", false);
				SetUpgradePanel(false);
				break;

            case "upgrade":
                PotentialHire.SetActive(false);
                UpgradeArtist.SetActive(true);
				UpgradeCost.SetActive(false);
				UpgradeButton.interactable = false;
				upgradeButtonText.color = disabledUpgradeButtonTextColor;
				upgradeButtonText.text = "SELECT AN UPGRADE";
				cancelButtonText.text = "DONE";
				ExistingArtist.SetActive(false);
				lastState = state;
				ArtistSigningView.animator.SetBool("UpgradesSalaryAlert", false);
				SetUpgradePanel(true);
				break;

			case "upgradeReady":
				PotentialHire.SetActive(false);
				UpgradeArtist.SetActive(true);
				UpgradeCost.SetActive(true);
				UpgradeButton.interactable = true;
				upgradeButtonText.text = GetCurrentUpgradeText();
				cancelButtonText.text = "CANCEL";
				pendingSalaryText.text = string.Format("${0:n0} / WEEK", currentBand.NextUpkeepCost);
				upgradeButtonText.color = activeUpgradeButtonTextColor;
				ExistingArtist.SetActive(false);
				lastState = state;
				ArtistSigningView.animator.SetBool("UpgradesSalaryAlert", true);
				SetUpgradePanel(true);
				break;
			default: break;
        }
	}
	
	private IEnumerator ShakeCost()
	{
		GameRefs.I.m_sfxAudio.PlaySfxClip(SfxAudio.SfxType.Error);
		UpgradeCostTransform.anchoredPosition = originalCostPos;
		yield return UpgradeCostTransform.AnchorPosTo(UpgradeCostTransform.anchoredPosition.x + 35f, 0.4f, Axis.X).Wave(Wave.Function.SinFade, 5);
	}

    public void ConfirmUpgrade()
    {
        if (upgradeToggleGroup.AnyTogglesOn())
        {
            Toggle t = upgradeToggleGroup.ActiveToggles().FirstOrDefault();
            if(t != null && t.gameObject.activeSelf)
            {
                float upgradeCost = CalculateRequiredCash(currentBand.GetKnownTraits().Count);
                if (GameRefs.I.m_gameController.GetCash() > upgradeCost)
				{
					GameRefs.I.m_gameController.RemoveCash(upgradeCost);
				}
				else
				{
					shakeRoutine.Replace(this, ShakeCost());
					return; // Not enough moneys!
				}
                   

				purchasedUpgrade = true;
				UpgradeType.BandUpgradeType type = t.gameObject.GetComponent<UpgradeType>().upgradeType;
				UpgradeButton.interactable = false;
                switch (type)
                {
                    case UpgradeType.BandUpgradeType.Reliability:
						UpgradeSkill(reliabilityUpgrade, () => {
							currentBand.ReliabilityScore++;
							reliabilityUpgrade.ConfirmUpgrade();
						});
						break;
                    case UpgradeType.BandUpgradeType.Speed:
						UpgradeSkill(speedUpgrade, () => {
							currentBand.SpeedScore++;
							speedUpgrade.ConfirmUpgrade();
						});
						break;
                    case UpgradeType.BandUpgradeType.Persistence:
						UpgradeSkill(persistenceUpgrade, () => {
							currentBand.PersistenceScore++;
							persistenceUpgrade.ConfirmUpgrade();
						});
						break;
                    case UpgradeType.BandUpgradeType.Ambition:
						UpgradeSkill(ambitionUpgrade, () => {
							currentBand.AmbitionScore++;
							ambitionUpgrade.ConfirmUpgrade();
						});
						break;
                    case UpgradeType.BandUpgradeType.Mood1:
						UpgradeMood(0);
                        break;
                    case UpgradeType.BandUpgradeType.Mood2:
						UpgradeMood(1);
                        break;
                    case UpgradeType.BandUpgradeType.Mood3:
						UpgradeMood(2);
                        break;
                    case UpgradeType.BandUpgradeType.Mood4:
						UpgradeMood(3);
                        break;
                    case UpgradeType.BandUpgradeType.Mood5:
						UpgradeMood(4);
                        break;
                    case UpgradeType.BandUpgradeType.Mood6:
						UpgradeMood(5);
                        break;
                    case UpgradeType.BandUpgradeType.Topic1:
						UpgradeTopic(0);
                        break;
                    case UpgradeType.BandUpgradeType.Topic2:
						UpgradeTopic(1);
                        break;
                    case UpgradeType.BandUpgradeType.Topic3:
						UpgradeTopic(2);
                        break;
                    case UpgradeType.BandUpgradeType.Topic4:
						UpgradeTopic(3);
                        break;
                    case UpgradeType.BandUpgradeType.Topic5:
						UpgradeTopic(4);
                        break;
                    case UpgradeType.BandUpgradeType.Topic6:
						UpgradeTopic(5);
                        break;
                    default:
                        break;
                }
				GameRefs.I.m_sfxAudio.PlaySfxClip(SfxAudio.SfxType.UpgradeArtist);
				GameRefs.I.PostGameState(true, "clickedButton", "ConfirmUpgradeButton");
			}
        }
    }

	public bool HasPlayerPurchasedUpgrade()
	{
		return purchasedUpgrade;
	}

	private void UpgradeSkill(UpgradeType upgrade, System.Action action)
	{
		StartCoroutine(HighlightUpgrade(upgrade.transform.parent.gameObject, () => action()));
	}

	private void UpgradeTopic(int i)
	{
		StartCoroutine(HighlightUpgrade(Topics[i].gameObject, () => {
			List<StatSubType> subtraits = StatSubType.GetFilteredList(StatType.TOPIC, false);
			currentBand.GetKnownTraits().Add(subtraits[i]);
		}));
	}

	private void UpgradeMood(int i)
	{
		StartCoroutine(HighlightUpgrade(Moods[i].gameObject, () => {
			List<StatSubType> subtraits = StatSubType.GetFilteredList(StatType.MOOD, false);
			currentBand.GetKnownTraits().Add(subtraits[i]);
		}));
	}

	private IEnumerator HighlightUpgrade(GameObject parent, System.Action action)
	{
		Image flash = parent.transform.Find("AnimUpgradeImage").gameObject.GetComponent<Image>();

		RectTransform xform = flash.GetComponent<RectTransform>();
		flash.gameObject.SetActive(true);
		Color color = flash.color;
		float secondsPerFrame = 0.02f;

		StartCoroutine(Footilities.CoLerp(4 * secondsPerFrame, 0, 1, alpha => {
			color.a = alpha;
			flash.color = color;
		}, () => {
			StartCoroutine(Footilities.CoLerp(14 * secondsPerFrame, 1, 1, alpha => {
				color.a = alpha;
				flash.color = color;
			}, () => {
				StartCoroutine(Footilities.CoLerp(17 * secondsPerFrame, 1, 0, alpha => {
					color.a = alpha;
					flash.color = color;
				}));
			}));
		}));

		yield return Footilities.CoLerp(10 * secondsPerFrame, 4, 1, factor => xform.localScale = new Vector3(factor, factor, 1));

		action();
        SetSigningPanelsate("upgrade");
        AssignBand(currentBand);

		yield return Footilities.CoLerp(4 * secondsPerFrame, new Vector2(1, 1), new Vector2(0.8f, 0.7f), factor => xform.localScale = new Vector3(factor.x, factor.y, 1));
		yield return Footilities.CoLerp(4 * secondsPerFrame, new Vector2(0.8f, 0.7f), new Vector2(1, 1), factor => xform.localScale = new Vector3(factor.x, factor.y, 1));
		yield return Footilities.CoLerp(4 * secondsPerFrame, new Vector2(1, 1), new Vector2(1, 0.7f), factor => xform.localScale = new Vector3(factor.x, factor.y, 1));
		yield return Footilities.CoLerp(13 * secondsPerFrame, new Vector2(1, 0.7f), new Vector2(3, 3), factor => xform.localScale = new Vector3(factor.x, factor.y, 1));
		yield return new WaitForSeconds(0.02f);
		flash.gameObject.SetActive(false);

		ArtistSigningView.RefreshArtistList();
	}

    private void SetUpgradePanel(bool on)
    {
		if (currentBand == null && GameRefs.I.m_gameController.GetSignedBands().Count > 0)
			currentBand = GameRefs.I.m_gameController.GetSignedBands()[0];
        List<StatSubType> bandKnownTraits = this.currentBand.GetKnownTraits();

        if (on)
        {
            foreach (KeyValuePair<int, StatSubType> kvp in this.moodLookup)
            {
                moodUpgrades[kvp.Key].SetActive(!bandKnownTraits.Contains(kvp.Value));
                moodTexts[kvp.Key].color = bandKnownTraits.Contains(kvp.Value) ? Color.white : Color.black;
            }

            foreach (KeyValuePair<int, StatSubType> kvp in this.topicLookup)
            {
                topicUpgrades[kvp.Key].SetActive(!bandKnownTraits.Contains(kvp.Value));
                topicTexts[kvp.Key].color = bandKnownTraits.Contains(kvp.Value) ? Color.white : Color.black;
            }

            ambitionUpgrade.gameObject.SetActive(currentBand.AmbitionScore < MAX_STAT_RANK);
            reliabilityUpgrade.gameObject.SetActive(currentBand.ReliabilityScore < MAX_STAT_RANK);
            speedUpgrade.gameObject.SetActive(currentBand.SpeedScore < MAX_STAT_RANK);
            persistenceUpgrade.gameObject.SetActive(currentBand.PersistenceScore < MAX_STAT_RANK);

            float upgradeCost = CalculateRequiredCash(bandKnownTraits.Count);
            upgradeCostText.SetText(string.Format("-${0}", upgradeCost.ToString("N0", CultureInfo.InvariantCulture)) );
        }
        else // turn all off to default
        {
            foreach (KeyValuePair<int, StatSubType> kvp in this.moodLookup)
            {
                moodUpgrades[kvp.Key].SetActive(false);
                moodTexts[kvp.Key].color = Color.white;
            }

            foreach (KeyValuePair<int, StatSubType> kvp in this.topicLookup)
            {
                topicUpgrades[kvp.Key].SetActive(false);
                topicTexts[kvp.Key].color = Color.white;
            }
            ambitionUpgrade.gameObject.SetActive(false);
            reliabilityUpgrade.gameObject.SetActive(false);
            speedUpgrade.gameObject.SetActive(false);
            persistenceUpgrade.gameObject.SetActive(false);
        }

    }

    private float CalculateRequiredCash(int numKnownTraits)
    {
        float upgradeCost;
        int totalUpgrades = 0;
        totalUpgrades += currentBand.AmbitionScore - 1;
        totalUpgrades += currentBand.ReliabilityScore - 1;
        totalUpgrades += currentBand.SpeedScore - 1;
        totalUpgrades += currentBand.PersistenceScore - 1;
        totalUpgrades += numKnownTraits - 4; //Subtract the four traits given for free

        upgradeCost = Mathf.Round((GameRefs.I.m_gameController.BandTierList.UpgradeBaseCost * Mathf.Pow(GameRefs.I.m_gameController.BandTierList.MultiplierPerUpgrade, totalUpgrades))/500f) *500f;

        return upgradeCost;
    }

	private string GetCurrentUpgradeText()
	{
		if (upgradeToggleGroup.AnyTogglesOn())
		{
			Toggle t = upgradeToggleGroup.ActiveToggles().FirstOrDefault();
			if (t != null && t.gameObject.activeSelf)
			{
				UpgradeType.BandUpgradeType type = t.gameObject.GetComponent<UpgradeType>().upgradeType;

				switch (type)
				{
					case UpgradeType.BandUpgradeType.Reliability: return "Increase Reliability";
					case UpgradeType.BandUpgradeType.Speed: return "Increase Talent";
					case UpgradeType.BandUpgradeType.Persistence: return "Increase Persistence";
					case UpgradeType.BandUpgradeType.Ambition: return "Increase Ambition";
					case UpgradeType.BandUpgradeType.Mood1: return string.Format("ADD {0} MOOD", Utilities.InterceptText(StatSubType.MOOD1.Name));
					case UpgradeType.BandUpgradeType.Mood2: return string.Format("ADD {0} MOOD", Utilities.InterceptText(StatSubType.MOOD2.Name));
					case UpgradeType.BandUpgradeType.Mood3: return string.Format("ADD {0} MOOD", Utilities.InterceptText(StatSubType.MOOD3.Name));
					case UpgradeType.BandUpgradeType.Mood4: return string.Format("ADD {0} MOOD", Utilities.InterceptText(StatSubType.MOOD4.Name));
					case UpgradeType.BandUpgradeType.Mood5: return string.Format("ADD {0} MOOD", Utilities.InterceptText(StatSubType.MOOD5.Name));
					case UpgradeType.BandUpgradeType.Mood6: return string.Format("ADD {0} MOOD", Utilities.InterceptText(StatSubType.MOOD6.Name));
					case UpgradeType.BandUpgradeType.Topic1: return string.Format("ADD {0} TOPIC", Utilities.InterceptText(StatSubType.TOPIC1.Name));
					case UpgradeType.BandUpgradeType.Topic2: return string.Format("ADD {0} TOPIC", Utilities.InterceptText(StatSubType.TOPIC2.Name));
					case UpgradeType.BandUpgradeType.Topic3: return string.Format("ADD {0} TOPIC", Utilities.InterceptText(StatSubType.TOPIC3.Name));
					case UpgradeType.BandUpgradeType.Topic4: return string.Format("ADD {0} TOPIC", Utilities.InterceptText(StatSubType.TOPIC4.Name));
					case UpgradeType.BandUpgradeType.Topic5: return string.Format("ADD {0} TOPIC", Utilities.InterceptText(StatSubType.TOPIC5.Name));
					case UpgradeType.BandUpgradeType.Topic6: return string.Format("ADD {0} TOPIC", Utilities.InterceptText(StatSubType.TOPIC6.Name));
					default: break;
				}
			}
		}
		return "UPGRADE";
	}
}
