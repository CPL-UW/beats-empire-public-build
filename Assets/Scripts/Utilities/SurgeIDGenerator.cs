using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurgeIDGenerator : MonoBehaviour {

	public List<SurgeList> surges;

	int uniqueID = 0;
	// Use this for initialization
	void Start () {
		foreach(SurgeList surgeList in surges)
		{
			foreach(SurgeData surgeData in surgeList.Contents)
			{
				surgeData.uniqueID = uniqueID++;
			}
		}
	}

}
