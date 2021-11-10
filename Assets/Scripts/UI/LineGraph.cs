using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using Utility;

public class LineGraph : MonoBehaviour
{
    public LineGraphEntry LineGraphEntryPrefab;
    public LineGraphEntry LineGraphEntryPrefabGlow;

    public List<LineGraphEntry> Entries;

	public List<StatSubType> currTypes = new List<StatSubType>();
    public List<TextMeshProUGUI> LegendEntries;
    public List<Image> LegendSwatches;
	public List<GameObject> NormalLegendEntries;
	public GameObject LegendHeatmap;

	public RectTransform BYSLine;
	public GameObject BYSText;

    public List<Text> YNumberLabels;
    public List<Text> YSuffixLabels;
    public List<Text> TurnLabels;

	private int selectedEntry = -1;
	private int altSelectedEntry = -1;
	public float[] thickness;
    public float minDistTest;
    private float highestValue;
    private float lowestValue;
    private bool allowFractions;

	void Start()
	{
		thickness = new float[12];
		for (int i = 0; i < thickness.Length; i++)
			thickness[i] = GameRefs.I.trendsLineThickness;
	}

    public void SetupNewGraphView(List<GraphManager.GraphEntryData> data, int minKey, int maxKey, bool allowFractions, Band bandFilter=null, float axisMultiplier=1f)
    {
		for(int i = 0; i < NormalLegendEntries.Count; i++)
		{
			NormalLegendEntries[i].SetActive(true);
		}
		LegendHeatmap.SetActive(false);
		this.allowFractions = allowFractions;
        foreach (LineGraphEntry entry in this.Entries)
        {
            GameObject.Destroy(entry.gameObject);
        }

        this.Entries.Clear();

        for (int i = 0; i < data.Count; i++)
        {
            LineGraphEntry entry;
            if(data[i].Glow)
                entry = LineGraphEntry.Instantiate(this.LineGraphEntryPrefabGlow);
            else
                entry = LineGraphEntry.Instantiate(this.LineGraphEntryPrefab);

            entry.transform.SetParent(this.transform);
            entry.transform.localPosition = Vector3.zero;
            entry.Thickness = thickness[i] >= GameRefs.I.glowThickness ? thickness[i] : data[i].Thickness;
            entry.Init(data[i]);
			if (i == altSelectedEntry && i != selectedEntry)
				entry.IsAltGlow = true;
			else
				entry.IsAltGlow = false;
            this.Entries.Add(entry);
        }

        HashSet<KeyValuePair<string, Color>> legendEntries = new HashSet<KeyValuePair<string, Color>>();
		currTypes.Clear();

        if (data.Any(x => x.Subsets.Count > 1))
        {
            for (int i = 0; i < data.Count; i++)
            {
                for (int j = 0; j < data[i].Subsets.Count; j++)
                {
                    if(!data[i].Glow)
					{
						currTypes.Add(data[i].Subsets[j].SubType);
						legendEntries.Add(new KeyValuePair<string, Color>(data[i].Subsets[j].SubType.Name, data[i].Subsets[j].SubType.GraphColor));
					}
                        
                }
            }
        }
        else
        {
            for (int i = 0; i < data.Count; i++)
            {
                if (data[i].Subsets.Count > 0)
                {
                    if (!data[i].Glow)
					{
						currTypes.Add(data[i].Subsets[0].SubType);
						legendEntries.Add(new KeyValuePair<string, Color>(data[i].Name, data[i].Subsets[0].SubType.GraphColor));
					} 
                }
            }
        }

        int entriesAdded = 0;

        foreach (KeyValuePair<string, Color> kvp in legendEntries)
        {
            this.LegendEntries[entriesAdded].gameObject.SetActive(true);
            this.LegendEntries[entriesAdded].SetIText(kvp.Key);
            this.LegendSwatches[entriesAdded].gameObject.SetActive(true);
            this.LegendSwatches[entriesAdded].color = kvp.Value;
            entriesAdded++;
        }

        for (int i = data.Count; i < this.LegendEntries.Count; i++)
        {
            this.LegendEntries[i].gameObject.SetActive(false);
            this.LegendSwatches[i].gameObject.SetActive(false);
        }

        this.UpdateGraph(minKey, maxKey, axisMultiplier);
    }

    public void UpdateGraph(int minKey, int maxKey, float axisMultiplier)
    {
        int timeRange = maxKey - minKey;

        for (int i = 0; i < this.TurnLabels.Count; i++)
        {
            this.TurnLabels[i].gameObject.SetActive(i < timeRange);

            if (timeRange > this.TurnLabels.Count)
            {
				int labelNum = minKey + 1 + i * 2 - 31 + GameRefs.I.m_gameController.currentTurn - GameRefs.I.m_gameInitVars.StartingWeeks + 1;
				this.TurnLabels[i].text = labelNum <= 0 ? "" : labelNum.ToString();
            }
            else
            {
				int labelNum = minKey + 1 + i - 31 + GameRefs.I.m_gameController.currentTurn - GameRefs.I.m_gameInitVars.StartingWeeks + 1;
				this.TurnLabels[i].text = labelNum <= 0 ? "" : labelNum.ToString();
            }

			if(GameRefs.I.m_gameController.currentTurn - GameRefs.I.m_gameInitVars.StartingWeeks < timeRange - 1)
			{
				BYSText.SetActive(true);
				BYSLine.gameObject.SetActive(true);
				float stepSize = timeRange == 6 ? 250f : (timeRange == 16 ? 80f : 40f);
				BYSLine.sizeDelta = new Vector2(1170f - stepSize * (GameRefs.I.m_gameController.currentTurn - 31f), 12.5f);
			}
			else
			{
				BYSText.SetActive(false);
				BYSLine.gameObject.SetActive(false);
			}
        }

        if (this.Entries.Count == 0)
        {
            return;
        }

        foreach (LineGraphEntry entry in this.Entries)
        {
            entry.ChangeBounds(minKey, maxKey);
        }

        float? currentLowestValue = null;
        float? currentHighestValue = null;

        foreach (LineGraphEntry entry in this.Entries)
        {
            currentLowestValue = Utility.Utilities.NullableMin(currentLowestValue, entry.GetLowestValue());
            currentHighestValue = Utility.Utilities.NullableMax(currentHighestValue, entry.GetHighestValue());
        }

        if (!currentLowestValue.HasValue || !currentHighestValue.HasValue)
        {
            currentLowestValue = 0;
            currentHighestValue = 1;
        }

        this.lowestValue = currentLowestValue.Value;
        this.highestValue = currentHighestValue.Value;
        if (this.lowestValue >= 0 && this.highestValue <= (this.allowFractions ? 1 : 6))
        {
            if (this.allowFractions)
            {
                this.lowestValue = 0;
                this.highestValue = 1;
            }
            else
            {
                this.lowestValue = 0;
                this.highestValue = 6;
            }
        }
        else
        {
            if (this.lowestValue == this.highestValue && this.lowestValue != 0)
            {
                if (this.highestValue >= 1000)
                {
                    this.lowestValue -= 500;
                    this.highestValue += 500;
                }
                else
                {
                    this.lowestValue -= 0.5f;
                    this.highestValue += 0.5f;
                }
            }

            float interval = this.highestValue / 6f;

            if (!this.allowFractions)
            {
                interval = Mathf.Ceil(interval);
            }

            float orderOfMagnitude = Mathf.Ceil(Mathf.Log10(interval));

            float adjustedInterval = interval / Mathf.Pow(10, orderOfMagnitude);

            float[] possibleIntervals = new float[] { 0.1f, 0.2f, 0.25f, 0.4f, 0.5f, 0.6f, 0.75f, 0.8f, 1f };

            for (int i = 0; i < possibleIntervals.Length; i++)
            {
                if (adjustedInterval < possibleIntervals[i])
                {
                    adjustedInterval = possibleIntervals[i];
                    break;
                }
            }

            this.highestValue = adjustedInterval * Mathf.Pow(10, orderOfMagnitude) * 6f;

            interval = (this.highestValue - this.lowestValue) / 6f;

            orderOfMagnitude = Mathf.Ceil(Mathf.Log10(interval));

            adjustedInterval = interval / Mathf.Pow(10, orderOfMagnitude);

            for (int i = 0; i < possibleIntervals.Length; i++)
            {
                if (adjustedInterval < possibleIntervals[i])
                {
                    adjustedInterval = possibleIntervals[i];
                    break;
                }
            }

            this.lowestValue = this.highestValue - adjustedInterval * Mathf.Pow(10, orderOfMagnitude) * 6f;

            if (this.lowestValue < 0)
            {
                this.lowestValue = 0;
            }
        }

        foreach (LineGraphEntry entry in this.Entries)
        {
            entry.SyncScale(this.lowestValue, this.highestValue);
        }

        for (int i = 0; i < this.YNumberLabels.Count; i++)
        {
            float valueAtThisTick = axisMultiplier * (this.lowestValue + (this.highestValue - this.lowestValue) * (((float)i) / (this.YNumberLabels.Count - 1)));

            string text = string.Format("{0}", Utilities.FormatNumberForDisplay(valueAtThisTick));

            if (text[text.Count() - 1] == 'k' || text[text.Count() - 1] == 'm')
            {
                this.YSuffixLabels[i].text = text[text.Count() - 1].ToString();
                text = text.Remove(text.Count() - 1);
            }
            else
            {
                this.YSuffixLabels[i].text = "";
            }

            this.YNumberLabels[i].SetIText(text);
            this.YSuffixLabels[i].gameObject.SetActive(true);
        }
    }

	public Vector3 GetPredictionMarkerPosition(int minKey, int maxKey, int entry)
	{
		Vector3 returnPoint = Vector3.zero;
		for (int i = minKey+1; i < maxKey; i++)
		{
			// Don't look for glow points
			if (!Entries[entry*2].GetPointInCanvas(i).HasValue)
				continue;

			returnPoint = Entries[entry * 2].GetPointInCanvas(i).Value;
		}

		return returnPoint;
	}

	public int GetClosestLine(int minKey, int maxKey, Vector2 clickPoint)
	{
		// We could ty checking every single point to see what's the closest. But let's try just searching the y values for a given x column
		float minDist = 99999f;
		int closestKey = -1;
		for (int i = minKey+1; i < maxKey; i++)
		{
			if (!Entries[0].GetPointInCanvas(i).HasValue)
				continue;

			if(Mathf.Abs(clickPoint.x - Entries[0].GetPointInCanvas(i).Value.x) < minDist)
			{
				minDist = Mathf.Abs(clickPoint.x - Entries[0].GetPointInCanvas(i).Value.x);
				closestKey = i;
			}
		}

		if (minDist > minDistTest)
			return -1;

		minDist = 99999f;
		int minEntry = -1;

		for (int i = 0; i < Entries.Count; i++)
		{
            // Don't look for glow points, points with no value
            if (Entries[i].IsGlow || Entries[i].Thickness < GameRefs.I.insightSelectableLineThickness || !Entries[i].GetPointInCanvas(closestKey).HasValue)
                continue;

			if (Mathf.Abs(clickPoint.y - Entries[i].GetPointInCanvas(closestKey).Value.y) < minDist)
			{
				minDist = Mathf.Abs(clickPoint.y - Entries[i].GetPointInCanvas(closestKey).Value.y);
				minEntry = i;
			}
		}

        if (minDist < minDistTest)
            return minEntry / 2;
        else
            return -1;
	}

    public void SetLineGlow(int line, bool enable, bool altGlowColor)
    {
		if (line * 2 + 1 == selectedEntry && !enable && altGlowColor)
			return;

        if (enable)
		{
			thickness[line * 2 + 1] = GameRefs.I.glowThickness;
		}
        else
		{
			thickness[line * 2 + 1] = 0f;
		}
   
    }

	public void SetLineGlowEntry(int entry, bool altGlow)
	{
		if (altGlow)
			altSelectedEntry = entry;
		else
			selectedEntry = entry;
	}
}
