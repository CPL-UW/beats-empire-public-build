using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName="ArtistNameGenerator", menuName="Parameters/ArtistNameGenerator", order = 1)]
public class ArtistNameGenerator : ScriptableObject
{
    public List<string> lefts;
    public List<string> rights;

	public string GetRandom()
	{
		return string.Format("{0} {1}", lefts.RandomElement(), rights.RandomElement());
	}
}
