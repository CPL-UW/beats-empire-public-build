using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using Utility;

public class BarGraph : MonoBehaviour
{
    public BarGraphEntry BarGraphEntryPrefab;

    public List<BarGraphEntry> Entries;
	public List<BarGraphEntry> GlowEntries;

    public List<TextMeshProUGUI> LegendEntries;
    public List<Image> LegendSwatches;
	public List<GameObject> NormalLegendEntries;
	public GameObject LegendHeatmap;
	public List<StatSubType> currTypes = new List<StatSubType>();

	public List<Text> YNumberLabels;
    public List<Text> YSuffixLabels;
    public List<Text> TurnLabels;

	public Transform barsParent;
	public Transform glowsParent;

    private float highestValue;
	private int selectedEntry = -1;
	private int altSelectedEntry = -1;
    private bool allowFractions;
	public List<bool> entriesUnlocked;

	void Start()
	{
		entriesUnlocked = new List<bool>();
	}

    public void SetupNewGraphView(List<GraphManager.GraphEntryData> data, int key, bool allowFractions, float axisMultiplier)
    {
		for (int i = 0; i < NormalLegendEntries.Count; i++)
		{
			NormalLegendEntries[i].SetActive(true);
		}
		this.allowFractions = allowFractions;
		LegendHeatmap.SetActive(false);
		foreach (BarGraphEntry entry in this.Entries)
            GameObject.Destroy(entry.gameObject);
		foreach (BarGraphEntry entry in this.GlowEntries)
			GameObject.Destroy(entry.gameObject);

		currTypes.Clear();
        this.Entries.Clear();
		this.GlowEntries.Clear();

        for (int i = 0; i < data.Count; i++)
        {
			BarGraphEntry entry = BarGraphEntry.Instantiate(this.BarGraphEntryPrefab);
			entry.transform.SetParent(glowsParent);
			entry.transform.localPosition = Vector3.zero;
			entry.Init(data[i], true);
			if (selectedEntry == i)
			{
				entry.gameObject.SetActive(true);
			}
			else if (altSelectedEntry == i)
			{
				entry.altGlow = true;
				entry.gameObject.SetActive(true);
			}
			else
			{
				entry.gameObject.SetActive(false);
			}
				
			GlowEntries.Add(entry);

			entry = BarGraphEntry.Instantiate(this.BarGraphEntryPrefab);
			entry.transform.SetParent(barsParent);
			entry.transform.localPosition = Vector3.zero;
			entry.Init(data[i]);
			this.Entries.Add(entry);
		}

        HashSet<KeyValuePair<string, Color>> legendEntries = new HashSet<KeyValuePair<string, Color>>();

        for (int i = 0; i < data.Count; i++)
        {
            for (int j = 0; j < data[i].Subsets.Count; j++)
            {
                legendEntries.Add(new KeyValuePair<string, Color>(data[i].Name, data[i].Subsets[j].SubType.GraphColor));
				currTypes.Add(data[i].Subsets[j].SubType);
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

        this.UpdateGraph(key, axisMultiplier);
    }

	public Vector3 GetPredictionMarkerPosition(int entry)
	{
		if((entry < 0) || (entry > Entries.Count - 1) || !Entries[entry].GetPointInCanvas().HasValue)
		{
			Debug.LogFormat("Entry: {0}, Entries.Count: {1}", entry, Entries.Count);
			return Vector3.zero;
		}
		return Entries[entry].GetPointInCanvas().Value;
	}

	public void SetBarGlow(int entry, bool altGlow)
	{
		if (altGlow)
			altSelectedEntry = entry;
		else
			selectedEntry = entry;
	}

	public int GetClosestBar(Vector2 clickPoint)
	{
		int closestKey = -1; 

		for (int i = 0; i < Entries.Count; i++)
		{
			if (Entries[i].isUnlocked && Entries[i].CheckBounds(clickPoint))
			{
				closestKey = i;
			}
		}

		return closestKey;
	}

	public void UpdateGraph(int key, float axisMultiplier)
    {
        for (int i = 0; i < this.TurnLabels.Count; i++)
        {
            if (i < this.Entries.Count)
            {
                this.TurnLabels[i].gameObject.SetActive(true);
                this.TurnLabels[i].text = this.Entries[i].Name;
				Utilities.SetIText(this.TurnLabels[i], this.Entries[i].Name);
            }
            else
            {
                this.TurnLabels[i].gameObject.SetActive(false);
            }
        }

        if (this.Entries.Count == 0)
        {
            return;
        }

        for (int i = key; i >= 0; i--)
        {
            foreach (BarGraphEntry entry in this.Entries)
            {
                entry.ChangeKey(i);
            }

            if (this.Entries.Any(x => x.IsValidKey()))
            {
                break;
            }
        }

        float? currentHighestValue = null;

        foreach (BarGraphEntry entry in this.Entries)
        {
            currentHighestValue = Utility.Utilities.NullableMax(currentHighestValue, entry.GetValue());
        }

        if (!currentHighestValue.HasValue)
        {
            currentHighestValue = 1;
        }

        this.highestValue = currentHighestValue.Value;

        if (this.highestValue <= (this.allowFractions ? 1 : 6))
        {
            this.highestValue = (this.allowFractions ? 1 : 6);
        }
        else
        {
            float interval = this.highestValue / 6f;

            float orderOfMagnitude = Mathf.Ceil(Mathf.Log10(interval));

            float adjustedInterval = interval / Mathf.Pow(10, orderOfMagnitude);

            float[] possibleIntervals = new float[] { 0.1f, 0.2f, 0.25f, 0.4f, 0.5f, 0.6f, 0.75f, 0.8f, 1f};

            for (int i = 0; i < possibleIntervals.Length; i++)
            {
                if (adjustedInterval < possibleIntervals[i])
                {
                    adjustedInterval = possibleIntervals[i];
                    break;
                }
            }

            this.highestValue = adjustedInterval * Mathf.Pow(10, orderOfMagnitude) * 6f;
        }

        for (int i = 0; i < this.Entries.Count; i++)
        {
            this.Entries[i].SyncTransform(this.highestValue, ((float)i + 1) / (this.Entries.Count + 1));
			this.GlowEntries[i].SyncTransform(this.highestValue, ((float)i + 1) / (this.GlowEntries.Count + 1));
		}

        for (int i = 0; i < this.YNumberLabels.Count; i++)
        {
            float valueAtThisTick = axisMultiplier * this.highestValue * (((float)i) / (this.YNumberLabels.Count - 1));

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
}
