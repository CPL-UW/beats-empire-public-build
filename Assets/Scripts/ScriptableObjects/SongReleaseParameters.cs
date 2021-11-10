using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName="SongReleaseParameters", menuName="Parameters/SongReleaseParameters", order = 1)]
public class SongReleaseParameters : ScriptableObject
{
	public Sprite predictedOutlineSprite;
	public Sprite unpredictedOutlineSprite;

	public Color activeHitMeterBoxColor;
	public Color inactiveHitMeterBoxColor;

	public int hitnessLabelFontSize;
	public Color hitnessLabelColor;
	public Color hitnessLabelBackgroundColor;

	public int maxHitnessLabelFontSize;
	public Color maxHitnessLabelColor;
	public Color maxHitnessLabelBackgroundColor;

	[System.Serializable]
	public struct HitMeterThreshold {
		public string label;
		public int barCount;
	}
	public HitMeterThreshold[] hitMeterThresholds;
}
