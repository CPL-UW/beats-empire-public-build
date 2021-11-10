using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class BandGenerator
{
	public string name;
	[GenrePicker]
	public int genre;
	public List<BandMemberGenerator> members;

	public BandGenerator Clone()
	{
		return new BandGenerator {
			name = name,
			genre = genre,
			members = members.Select(member => member.Clone()).ToList(),
		};
	}

	public StatSubType RealGenre
	{
		get
		{
			return StatSubType.List[genre + StatSubType.ROCK_ID];
		}
	}
}

[System.Serializable]
public class BandMemberGenerator
{
	public InstrumentChoice instrument;
	public GenderChoice gender;
	public SkinToneChoice skinTone;
	[ShowIfOverride("skinTone")]
	public Texture skinToneOverride;
	public PoseChoice pose;
	[ShowIfOverride("pose")]
	public GameObject poseOverride;
	[ShowIfSmallInstrument]
	public FeaturePoseChoice featurePose;
	public bool isBackup;

	public BandMemberGenerator()
	{
		instrument = InstrumentChoice.Any;
		gender = GenderChoice.Any;
		skinTone = SkinToneChoice.Any;
		pose = PoseChoice.Universal;
		featurePose = FeaturePoseChoice.WithSmallInstrument;
	}

	public BandMemberGenerator Clone()
	{
		return new BandMemberGenerator {
			instrument = instrument,
			gender = gender,
			skinTone = skinTone,
			skinToneOverride = skinToneOverride,
			pose = pose,
			poseOverride = poseOverride,
			featurePose = featurePose,
			isBackup = isBackup,
		};
	}
}

public enum GenderChoice
{
	Any,
	Male,
	Female,
}

public enum SkinToneChoice
{
	Any,
	Black,
	Brown,
	Medium,
	Khaki,
	Cream,
	Override,
}

public enum PoseChoice
{
	Gendered,
	Universal,
	Override,
}

public enum FeaturePoseChoice
{
	WithoutInstrument,
	WithSmallInstrument,
}

public enum InstrumentChoice
{
	Any,
	Guitar,
	Bass,
	Keyboard,
	Synth,
	Drums,
	Vocals,	
}
