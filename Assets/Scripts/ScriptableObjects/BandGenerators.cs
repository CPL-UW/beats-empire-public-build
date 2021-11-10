using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName="BandGenerators", menuName="Parameters/BandGenerators", order=1)]
public class BandGenerators : ScriptableObject
{
	[Header("Bands")]
	public List<BandGenerator> generators;

	[Header("Body Parts")]
	public GenderedParts heads;
	public GenderedParts torsos;
	public GenderedParts legs;

	[Header("Instrument Meshes")]
	public List<GameObject> instrumentPrefabs;
	public GameObject drumstickPrefab;

	[Header("Poses")]
	public List<GameObject> malePoses;
	public List<GameObject> femalePoses;
	public List<GameObject> universalPoses;
	public List<GameObject> guitarBassPoses;
	public List<GameObject> microphonePoses;

	[Header("Skin Textures (from dark to light)")]
	public List<SkinToneClass> skins;

	[Header("Materials")]
	public Material unshadowedMaterial;
	public Material shadowedMaterial;
	public Material unshadowedInstrumentMaterial;
	public Material shadowedInstrumentMaterial;

	[Header("Animation")]
	public RuntimeAnimatorController animatorController;
	public Avatar avatar;

	[Header("Timings")]
	public float minTimeBetweenTwitches;
	public float maxTimeBetweenTwitches;

	public List<BandGenerator> Clone()
	{
		return generators.Select(generator => generator.Clone()).ToList();
	}

	public void Reset()
	{
		generators.Clear();

		string[] defaultNames =
		{
			"OCDC",
			"Lead Zipline",
			"Manic! at the Bistro",
			"Four Cat Day",
			"Arcade Ice",
			"Driftwood Pac",
			"Juno Mercury",
			"Envision Gryphons",
			"Ariana Venti",
			"BPS",
			"Lady Zaza",
			"M!nk",
			"Pharrell Millions",
			"Alyssa Keys",
			"Beyonde",
			"Ushest",
			"Stevie Thunder",
			"Bilanna",
			"Butane Plan",
			"Brake",
			"Lauren Tyll",
			"DJ Kale",
			"The Candy Mill Gang",
			"Micki Ninaj",
			"Lil Biggie",
			"Zee Jay",
			"Reesus Peaces",
			"Halfa Dollar",
			"Kanye East",
			"6Pac",
			"Paft Dunk",
			"Apex Triplet",
			"livehau5",
			"The Switch Knob",
			"Grillex",
			"The Chemistry Sisters",
		};

		int[] defaultGenres = {
			24, 24, 24, 24, 24, 24, 25, 25, 25, 25, 25, 25, 26, 26, 26, 26, 26, 26, 27, 27, 27, 27, 27, 27, 28, 28, 28, 28, 28, 28, 29, 29, 29, 29, 29, 29,
		};

		for (int i = 0; i < defaultNames.Length; ++i)
		{
			BandGenerator generator = new BandGenerator {
				name = defaultNames[i],
				genre = defaultGenres[i] - StatSubType.ROCK_ID,
			};
			generators.Add(generator);
		}
	}

	public void ResetFeaturePose()
	{
		foreach (BandGenerator generator in generators)
		{
			foreach (BandMemberGenerator member in generator.members)
			{
				member.featurePose = FeaturePoseChoice.WithSmallInstrument;
			}
		}
	}
}

[System.Serializable]
public class GenderedParts
{
	public List<GameObject> female;
	public List<GameObject> male;

	public GenderedParts()
	{
		female = new List<GameObject>();
		male = new List<GameObject>();
	}
}

[System.Serializable]
public class SkinToneClass
{
	public Texture[] options;
}
