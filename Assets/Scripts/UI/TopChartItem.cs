using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using BeauRoutine;
using UnityEngine.UI;
using Utility;

public class TopChartItem : MonoBehaviour {
	public TextMeshProUGUI spotNumber;
	public TextMeshProUGUI songTitle;
	public TextMeshProUGUI artistName;
	public TextMeshProUGUI listensNumber;
	public TextMeshProUGUI salesNumberLabel;
	public TextMeshProUGUI weekLabel;
	public Image background;
	public GameObject trophyRoot;
	public Image trophyImage;
	public Image trophyGenreIcon;

	public Image backgroundDisable;
	public Color backgroundDisableColor;

	public void ShowSong(SaveGameController.PreviousSong song, Color color, bool isThisWeek = true)
	{
		artistName.gameObject.SetActive(true);
		weekLabel.gameObject.SetActive(!isThisWeek);
		salesNumberLabel.gameObject.SetActive(true);
		if (trophyRoot != null)
		{
			trophyRoot.SetActive(false);
		}

		songTitle.text = song.name;
		background.color = color;
		artistName.text = song.artist;
		listensNumber.text = string.Format("{0:n0}", song.sales);
		weekLabel.text = string.Format("Week {0}", song.turnOfRelease + 2 - GameRefs.I.m_gameInitVars.StartingWeeks); // I don't get the numbering system, but I can abide by it.
		spotNumber.text = song.chartPosition.ToString();
	}

	public void ShowTrophy(int rank, StatSubType genre)
	{
		if (trophyRoot == null)
		{
			return;
		}

		trophyRoot.SetActive(true);
		if (rank == 1)
		{
			trophyGenreIcon.sprite = GameRefs.I.topChartsParameters.PlatinumIconForGenre(genre);
			trophyImage.sprite = GameRefs.I.topChartsParameters.platinumSprite;
		}
		else
		{
			trophyGenreIcon.sprite = GameRefs.I.topChartsParameters.GoldIconForGenre(genre);
			trophyImage.sprite = GameRefs.I.topChartsParameters.goldSprite;
		}
	}

	public void ShowPlaceholder(StatSubType genre)
	{
		artistName.gameObject.SetActive(false);
		weekLabel.gameObject.SetActive(false);
		trophyRoot.SetActive(false);
		salesNumberLabel.gameObject.SetActive(false);

		backgroundDisable.color = backgroundDisableColor;
		songTitle.text = "A Future Hit Song?";
		spotNumber.text = "?";
		listensNumber.SetIText(string.Format("Make more {0} songs!", genre.Name));
	}
}
