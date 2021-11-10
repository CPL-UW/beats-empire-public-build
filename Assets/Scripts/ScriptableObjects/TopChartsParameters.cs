using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName="TopChartsParameters", menuName="Parameters/TopChartsParameters", order = 1)]
public class TopChartsParameters : ScriptableObject
{
	public Sprite platinumSprite;
	public Sprite goldSprite;
	public Sprite[] goldGenreIcons;
	public Sprite[] platinumGenreIcons;

	public Sprite GoldIconForGenre(StatSubType genre)
	{
		return goldGenreIcons[genre.ID - StatSubType.ROCK_ID];
	}

	public Sprite PlatinumIconForGenre(StatSubType genre)
	{
		return platinumGenreIcons[genre.ID - StatSubType.ROCK_ID];
	}
}
