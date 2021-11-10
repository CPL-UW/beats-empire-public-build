using System.Collections;
using UnityEngine;

public class TopChartsEventRelay : MonoBehaviour {
	public TopChartsView topCharts;

	public void OnRightFinished() {
		topCharts.OnRightFinished();
	}
	
	public void OnLeftFinished() {
		topCharts.OnLeftFinished();
	}
}
