using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName="Colors", menuName="Parameters/Colors", order = 1)]
public class Colors : ScriptableObject
{
	[Header("Recording Audience")]
	public Color hoverBackground;
	public Color hoverPopulationBackground;
	public Color hoverTraitsBackground;
	public Color hoverTrendBackground;
	public Color selectedBackground;
	public Color selectedPopulationBackground;
	public Color selectedTraitsBackground;
	public Color selectedTrendBackground;
	public Color selectedBoroughTextColor;
	public Color unselectedBoroughTextColor;

	[Header("Recording Traits")]
	public Color recordingTraitMandatorySelectedColor;
	public Color recordingTraitMandatoryUnselectedColor;
	public Color recordingTraitOptionalSelectedColor;
	public Color recordingTraitOptionalUnselectedColor;
	public Color recordingTraitPressedColor;
	public Color recordingTraitDisabledColor;
	public Color recordingTraitLabelPredicted;
	public Color recordingTraitLabelUnpredicted;
	public Color recordingTraitLabelUnknown;
	public Color recordingTraitLabelSelectable;

	[Header("Heatmap")]
	public Color heatmapSelectedBoroughColor;
	public Color heatmapUnlockedBoroughColor;
	public Color heatmapLockedBoroughColor;

	[Header("Recording Phases")]
	public Color dotFinishedColor;
	public Color dotUnfinishedColor;
	public Color dotUnfilledColor;

	[Header("Recording Phases")]
	public Color preRecordingColor;
	public Color midRecordingColor;
	public Color postRecordingColor;

	[Header("Uncategorized Colors")]
	public Color selectedBackgroundColor;
	public Color hoverBackgroundColor;
	public Color unhiredTraitColor;
	public Color hiredTraitColor;

	[Header("HUD")]
	public Color hudArtistsBackgroundColor;
	public Color hudMarketingBackgroundColor;
	public Color hudRecordingBackgroundColor;
	public Color hudResultsBackgroundColor;
	public Color hudDataCollectionBackgroundColor;
	public Color hudDefaultMuteBackgroundColor;

	[Header("Shadow Colors")]
	public Color recordingShadowColor;
	public Color artistShadowColor;
	public Color marketingShadowColor;

	[System.Serializable]
	public class ColorSet {
		public Color dark;
		public Color midtone;
		public Color light;
	}

	[Header("Genre Colors")]
	public ColorSet rockColor;
	public ColorSet popColor;
	public ColorSet rbColor;
	public ColorSet hipHopColor;
	public ColorSet rapColor;
	public ColorSet electronicColor;

	private Dictionary<int, ColorSet> genreToColorSet;
	
	void OnEnable() {
		genreToColorSet = new Dictionary<int, ColorSet>();
		genreToColorSet[StatSubType.ROCK.ID] = rockColor;
		genreToColorSet[StatSubType.POP.ID] = popColor;
		genreToColorSet[StatSubType.RANDB.ID] = rbColor;
		genreToColorSet[StatSubType.HIP_HOP.ID] = hipHopColor;
		genreToColorSet[StatSubType.RAP.ID] = rapColor;
		genreToColorSet[StatSubType.ELECTRONIC.ID] = electronicColor;
	}

	public ColorSet GenreToColorSet(StatSubType genre) {
		return genreToColorSet[genre.ID];
	}
}
