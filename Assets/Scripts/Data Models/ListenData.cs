using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ListenData  {
    public string locationString;//nice to have for the inspector, dont use for anything else
    public int turn;
    public Song song;
    public StatSubType location;
    public float appeal;
    public float listens;
    public float wordOfMouth;
    public float starRating;
    public float freshness;
	
}
