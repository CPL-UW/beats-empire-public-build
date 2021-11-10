using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName="SharedParameters", menuName="Parameters/SharedParameters", order = 1)]
public class SharedParameters : ScriptableObject {
	public bool skipTutorials;
	public bool isMuted;
	public Sprite[] genreSpritesSmall; 

	public Sprite SpriteSmallForGenre(StatSubType genre)
	{
		return genreSpritesSmall[genre.ID - StatSubType.ROCK_ID];
	}
}
