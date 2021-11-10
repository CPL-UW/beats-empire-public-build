using System.Collections;
using UnityEngine;

public class RecordingSlotBandController : MonoBehaviour {
	private GameObject[] memberModels;

	void Awake()
	{
		memberModels = new GameObject[5];
		for (int i = 0; i < 5; ++i)
		{
			memberModels[i] = transform.GetChild(i).gameObject;
		}
	}

	public Band band {
		set {
			value.Incarnate(memberModels);
		}
	}
}
