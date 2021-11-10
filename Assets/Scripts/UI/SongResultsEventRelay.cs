using System.Collections;
using UnityEngine;

public class SongResultsEventRelay : MonoBehaviour {
	public SongResultsView resultsView;

	public void OnOff()
	{
		resultsView.ReallyContinueToTopCharts();
	}
}
