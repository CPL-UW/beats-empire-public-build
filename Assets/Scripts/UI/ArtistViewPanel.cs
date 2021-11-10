using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using TMPro;
using Utility;

public class ArtistViewPanel : MonoBehaviour
{
    public GameController Controller;

    public List<Color> BandGenreColors;

    public TextMeshProUGUI AmbitionLabel, ReliabilityLabel, SpeedLabel, PersistenceLabel;
    public Image BandGenreBacker;

	public Sprite[] bandGenres;
    public TextMeshProUGUI BandNameLabel, BandGenreLabel;

    public Transform PanelRoot;
	public GameObject displayPanel;
	public GameObject container;

	[Header("Band")]
	public GameObject[] memberModels;
    protected Band currentBand;

    protected Dictionary<int, StatSubType> moodLookup = new Dictionary<int, StatSubType>
    {
        {0, StatSubType.MOOD1},
        {1, StatSubType.MOOD2},
        {2, StatSubType.MOOD3},
        {3, StatSubType.MOOD4},
        {4, StatSubType.MOOD5},
        {5, StatSubType.MOOD6},
    };

    protected Dictionary<StatSubType, int> moodLookupReverse = new Dictionary<StatSubType, int>
    {
        {StatSubType.MOOD5, 0},
        {StatSubType.MOOD2, 1},
        {StatSubType.MOOD3, 2},
        {StatSubType.MOOD4, 3},
        {StatSubType.MOOD1, 4},
        {StatSubType.MOOD6, 5},
    };

    protected Dictionary<int, StatSubType> topicLookup = new Dictionary<int, StatSubType>
    {
        {0, StatSubType.TOPIC1},
        {1, StatSubType.TOPIC2},
        {2, StatSubType.TOPIC3},
        {3, StatSubType.TOPIC4},
        {4, StatSubType.TOPIC5},
        {5, StatSubType.TOPIC6},
    };

    protected Dictionary<StatSubType, int> topicLookupReverse = new Dictionary<StatSubType, int>
    {
        {StatSubType.TOPIC1, 0},
        {StatSubType.TOPIC2, 1},
        {StatSubType.TOPIC3, 2},
        {StatSubType.TOPIC4, 3},
        {StatSubType.TOPIC5, 4},
        {StatSubType.TOPIC6, 5},
    };

    private void UpdateSongQualityLabels()
    {
        this.ReliabilityLabel.SetIText(string.Format("{0}", this.currentBand.ReliabilityScore));
        this.SpeedLabel.SetIText(string.Format("{0}", this.currentBand.SpeedScore));
        this.AmbitionLabel.SetIText(string.Format("{0}", this.currentBand.AmbitionScore));
        this.PersistenceLabel.SetIText(string.Format("{0}", this.currentBand.PersistenceScore));
    }

    public virtual void AssignBand(Band band)
    {
        this.currentBand = band;

        this.BandNameLabel.SetIText(this.currentBand.Name);

		if (bandGenres.Length > 0)
		{
			this.BandGenreBacker.color = Color.white;
			this.BandGenreBacker.sprite = bandGenres[this.currentBand.GetGenre().ID - StatSubType.ROCK_ID];
			this.BandGenreLabel.SetIText(string.Format("{0}", this.currentBand.GetGenre().Name));
		}
		else
		{
			this.BandGenreBacker.color = this.BandGenreColors[this.currentBand.GetGenre().ID - StatSubType.ROCK_ID];
			this.BandGenreLabel.SetIText(string.Format("{0}", this.currentBand.GetGenre().Name));
		}
        

		GameRefs.I.m_globalLastBand = band;
        this.UpdateSongQualityLabels();

		// Hack alert. Incarnate's animators only work properly in active
		// hierarchies, and the animator owning this view might have made
		// ArtistDisplayPanel inactive.
		bool isDisplayPanelActive = displayPanel.activeSelf;
		displayPanel.SetActive(true);
		bool isContainerActive = container.activeSelf;
		container.SetActive(true);

		band.Incarnate(memberModels, Band.PoseContext.Feature);

		displayPanel.SetActive(isDisplayPanelActive);
		container.SetActive(isContainerActive);
    }
}
