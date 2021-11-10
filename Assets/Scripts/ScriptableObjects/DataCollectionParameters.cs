using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName="DataCollectionParameters", menuName="Parameters/DataCollectionParameters", order = 1)]
public class DataCollectionParameters : ScriptableObject
{
	public TraitSampling initialMoodSampling;
	public TraitSampling initialTopicSampling;
	public TraitSampling initialGenreSampling;
	public int[] slotCosts;
	public string noMoreSlotsText;
	public string costPerWeekFormat;
	public Frequency[] frequencies;

	[System.Serializable]
	public class Frequency
	{
		public int period;
		public string label;
		public string header;
		public int costPerWeek;
		public Color color;
	}

	public void OnValidate()
	{
		if (initialMoodSampling.slotCount == 0)
		{
			initialMoodSampling.iteration = -1;
		}
		else if (initialMoodSampling.iteration < 0)
		{
			initialMoodSampling.iteration = 0;
		}

		if (initialTopicSampling.slotCount == 0)
		{
			initialTopicSampling.iteration = -1;
		}
		else if (initialTopicSampling.iteration < 0)
		{
			initialTopicSampling.iteration = 0;
		}

		if (initialGenreSampling.slotCount == 0)
		{
			initialGenreSampling.iteration = -1;
		}
		else if (initialGenreSampling.iteration < 0)
		{
			initialGenreSampling.iteration = 0;
		}
	}
}
