using UnityEngine;
using System.Collections;

[System.Serializable]
public class Sale
{
    public string songName;
    public string location;
    [Header("A sale can count toward sales and/or acclaim.")]
    //we may want to change how this data is stored For now this is the first go-around
    public float purchases;
    public float listens;
    public float acclaim;
}
