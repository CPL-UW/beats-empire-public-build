using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Band
{
    public enum Instrument
    {
        Guitar,
        Bass,
        Keyboard,
        Synth,
        Drums,
        Vocals,
    }

    public enum PoseContext
    {
		Jam,    // all instruments plus animation
		Lineup, // no instruments
		Feature // mics, guitar, and bass only
    }

	[System.Serializable]
	public class SaveData
	{
		public string name;
		public int genreID;
		public int[] knownTraits;
		public int ambition;
		public int reliability;
		public int speed;
		public int persistence;
		public int points;
		public bool isNew;
		public BandMember.MemberSave[] members;
	}

	public string Name;

    public int StartingFollowers;

    public int AmbitionScore;
    public int ReliabilityScore;
    public int SpeedScore;
    public int PersistenceScore;

	public StatSubType preReleaseLocation = StatSubType.NONE;
	public StatSubType preReleaseMood = StatSubType.NONE;
	public StatSubType preReleaseTopic = StatSubType.NONE;
	public string preReleaseName;

	public List<BandMember> members;

	private List<StatSubType> knownTraits;
    private StatSubType genre;

	// Save the template used to create for loading game data
	public BandGenerator templateCreated;

	[System.NonSerialized]
    private Song recordingSong;
	private SaveData savedData;
    public int TurnsLeft;
    public bool IsSigned;
    public bool IsNew;


	// Used when loading a band that no longer exists and we just need it for stats
	public Band(string name, StatSubType newGenre)
	{
		this.Name = name;
		genre = newGenre;
	}

	public Band(SaveData saveData, bool isSigned)
	{
		Name = saveData.name;
		genre = StatSubType.List[saveData.genreID];
		knownTraits = new List<StatSubType>();
		for (int i = 0; i < saveData.knownTraits.Length; i++)
			knownTraits.Add(StatSubType.List[saveData.knownTraits[i]]);
		members = new List<BandMember>();
		for (int i = 0; i < saveData.members.Length; i++)
			members.Add(new BandMember(saveData.members[i]));
		AmbitionScore = saveData.ambition;
		ReliabilityScore = saveData.reliability;
		SpeedScore = saveData.speed;
		PersistenceScore = saveData.persistence;
		IsNew = saveData.isNew;
		BaseCost = GameRefs.I.m_gameController.BandTierList.BaseBandCost;
		CostPerPoint = GameRefs.I.m_gameController.BandTierList.AdditionalBandCostPerPoint;
		CostMultiplierPerPoint = GameRefs.I.m_gameController.BandTierList.AdditionalBandCostMultiplierPerPoint;

		IsSigned = isSigned;
		savedData = saveData;
	}

	public Band(BandGenerator template, int points, BandGenerators bandGenerators)
    {
		templateCreated = template;
		this.Name = template.name;

		if (template.RealGenre.ID == StatSubType.RANDOM_ID)
        {
            this.genre = StatSubType.GetRandomSubType(StatType.GENRE);
        }
        else
        {
            this.genre = template.RealGenre;
        }

        this.knownTraits = new List<StatSubType>();

        List<StatSubType> possibleTopics = StatSubType.GetFilteredList(StatType.TOPIC, false);
        List<StatSubType> possibleMoods = StatSubType.GetFilteredList(StatType.MOOD, false);
		
		StatSubType newTopic;
		StatSubType newMood;
		
		for (int i = 0; i < 2; i++) //Give each artist two topics and two moods for free
		{
			newTopic = possibleTopics[UnityEngine.Random.Range(0, possibleTopics.Count)];
			possibleTopics.Remove(newTopic);
			this.knownTraits.Add(newTopic);
			
			newMood = possibleMoods[UnityEngine.Random.Range(0, possibleMoods.Count)];
			possibleMoods.Remove(newMood);
			this.knownTraits.Add(newMood);
		}

		AvailableAttributes availables = new AvailableAttributes(bandGenerators);

		members = new List<BandMember>();
		if (template.members.Count == 0)
		{
			int nMembers = Random.Range(1, 6);
			for (int i = 0; i < nMembers; ++i)
			{
				members.Add(new BandMember(new BandMemberGenerator(), bandGenerators, availables));
			}
		}
		else
		{
			foreach (BandMemberGenerator memberGenerator in template.members)
			{
				members.Add(new BandMember(memberGenerator, bandGenerators, availables));
			}
		}
		
        this.AmbitionScore = 1;
        this.ReliabilityScore = 1;
        this.SpeedScore = 1;
        this.PersistenceScore = 1;

        for (int i = 0; i < points; i++)
        {
			bool pointChosen = false;

			if (this.AmbitionScore >= 5 &&
				this.SpeedScore >= 5 &&
				this.ReliabilityScore >= 5 &&
				this.PersistenceScore >= 5 &&
				possibleMoods.Count == 0 &&
				possibleTopics.Count == 0)
				break;

			while (!pointChosen)
			{
				switch (UnityEngine.Random.Range(0, 5))
				{
					case 0:
						if (this.AmbitionScore < 5)
						{
							this.AmbitionScore += 1;
							pointChosen = true;
						}
						break;

					case 1:
						if (this.SpeedScore < 5)
						{
							this.SpeedScore += 1;
							pointChosen = true;
						}
						break;

					case 2:
						if (this.ReliabilityScore < 5)
						{
							this.ReliabilityScore += 1;
							pointChosen = true;
						}
						break;

					case 3:
						if (this.PersistenceScore < 5)
						{
							this.PersistenceScore += 1;
							pointChosen = true;
						}
						break;

					case 4:
						if (UnityEngine.Random.value >= 0.5)
						{
							if (possibleTopics.Count > 0)
							{
								newTopic = possibleTopics[UnityEngine.Random.Range(0, possibleTopics.Count)];
								possibleTopics.Remove(newTopic);
								this.knownTraits.Add(newTopic);
								pointChosen = true;

							}
						}
						else
						{
							if (possibleMoods.Count > 0)
							{
								newMood = possibleMoods[UnityEngine.Random.Range(0, possibleMoods.Count)];
								possibleMoods.Remove(newMood);
								this.knownTraits.Add(newMood);
								pointChosen = true;
							}
						}
						break;
				}
			}

        }

        this.recordingSong = null;

		savedData = new SaveData();
		savedData.isNew = IsNew;
		savedData.ambition = AmbitionScore;
		savedData.persistence = PersistenceScore;
		savedData.speed = SpeedScore;
		savedData.reliability = ReliabilityScore;
		savedData.knownTraits = new int[knownTraits.Count];
		for (int i = 0; i < savedData.knownTraits.Length; i++)
			savedData.knownTraits[i] = knownTraits[i].ID;

		savedData.name = Name;
		savedData.genreID = genre.ID;
		savedData.members = new BandMember.MemberSave[members.Count];
		for(int i = 0; i < members.Count; i++)
		{
			savedData.members[i] = members[i].saveData;
		}

    }

	public SaveData GetSavedData()
	{
		savedData.isNew = IsNew;
		savedData.ambition = AmbitionScore;
		savedData.persistence = PersistenceScore;
		savedData.speed = SpeedScore;
		savedData.reliability = ReliabilityScore;
		savedData.knownTraits = new int[knownTraits.Count];
		for (int i = 0; i < savedData.knownTraits.Length; i++)
			savedData.knownTraits[i] = knownTraits[i].ID;

		return this.savedData;
	}

	public void Perfect()
	{
		AmbitionScore = 5;
		SpeedScore = 5;
		ReliabilityScore = 5;
		PersistenceScore = 5;

		foreach (StatSubType mood in StatSubType.GetFilteredList(StatType.MOOD, false))
		{
			if (!knownTraits.Contains(mood))
			{
				knownTraits.Add(mood);
			}
		}

		foreach (StatSubType topic in StatSubType.GetFilteredList(StatType.TOPIC, false))
		{
			if (!knownTraits.Contains(topic))
			{
				knownTraits.Add(topic);
			}
		}
	}

    public List<StatSubType> GetKnownTraits()
    {
        return this.knownTraits;
    }

    public StatSubType GetGenre()
    {
        return this.genre;
    }

	public void SetGenre(StatSubType setGenre)
	{
		this.genre = setGenre;
	}

    public void AssignRecordingSong(Song song)
    {
        this.recordingSong = song;
    }

    public Song GetRecordingSong()
    {
        return this.recordingSong;
    }

    public bool IsRecordingSong()
    {
        return this.recordingSong != null;
    }

    public void OnSongReleased()
    {
        this.recordingSong = null;
    }

	// A band's points are effectively the number of upgrades that have been applied.
	public int Points
	{
		get
		{
			return (PersistenceScore + AmbitionScore + ReliabilityScore + SpeedScore - 4) + (knownTraits.Count - 4);
		}
	}
	public int BaseCost { get; set; }
	public int CostPerPoint { get; set; }
	public float CostMultiplierPerPoint { get; set; }
	public int UpkeepCost
	{
		get
		{
			return CalculateUpkeepCost(Points);
		}
	}

	public int NextUpkeepCost
	{
		get
		{
			return CalculateUpkeepCost(Points + 1);
		}
	}

	private int CalculateUpkeepCost(int points)
	{
		return BaseCost + Mathf.CeilToInt((Mathf.Pow(points, CostMultiplierPerPoint) * CostPerPoint) / 500) * 500 ;
	}

	public void Incarnate(GameObject[] models, Band.PoseContext poseContext = Band.PoseContext.Feature, Renderer plane = null, Color shadowColor = default(Color))
	{
		for (int i = 0; i < members.Count; i++)
		{
			members[i].Incarnate(models[i], poseContext, plane, shadowColor);
		}

		// Disable unused members.
		for (int i = members.Count; i < models.Length; ++i)
		{
			models[i].SetActive(false);
		}
	}
}

public class AvailableAttributes
{
	public List<Band.Instrument> instruments;

	public GenderedParts heads;
	public GenderedParts torsos;
	public GenderedParts legs;

	public List<GameObject> malePoses;
	public List<GameObject> femalePoses;
	public List<GameObject> universalPoses;
	public List<GameObject> guitarBassPoses;
	public List<GameObject> microphonePoses;

	public AvailableAttributes(BandGenerators generators)
	{
		instruments = System.Enum.GetValues(typeof(Band.Instrument)).Cast<Band.Instrument>().ToList();

		heads = new GenderedParts();
		heads.male = new List<GameObject>(generators.heads.male);
		heads.female = new List<GameObject>(generators.heads.female);

		torsos = new GenderedParts();
		torsos.male = new List<GameObject>(generators.torsos.male);
		torsos.female = new List<GameObject>(generators.torsos.female);

		legs = new GenderedParts();
		legs.male = new List<GameObject>(generators.legs.male);
		legs.female = new List<GameObject>(generators.legs.female);

		malePoses = new List<GameObject>(generators.malePoses);
		femalePoses = new List<GameObject>(generators.femalePoses);
		universalPoses = new List<GameObject>(generators.universalPoses);
		guitarBassPoses = new List<GameObject>(generators.guitarBassPoses);
		microphonePoses = new List<GameObject>(generators.microphonePoses);
	}
}

public class BandMember
{
	[System.Serializable]
	public class MemberSave
	{
		public bool female;
		public string head;
		public string torso;
		public string legs;
		public string textureName;
		public int instrument;
		public int lineupPoseType;
		public int lineupPoseNumber;
		public int featurePoseType;
		public int featurePoseNumber;
		public bool isBackup;
	}

	public Band.Instrument instrumentId;
	public bool isFemale;
	public bool isBackup;
	public GameObject headPrefab;
	public GameObject torsoPrefab;
	public GameObject legsPrefab;
	public GameObject instrumentPrefab;
	public int lineupPoseType;
	public int lineupPoseNumber;
	public int featurePoseType;
	public int featurePoseNumber;
	public Texture texture;
	public BandGenerators bandGenerators;
	public MemberSave saveData;

	// Band members will be incarnated in several contexts. The state above is
	// common to all those contexts. Animators for the recording studio,
	// however, are not shared. We persist them as state to support twitches.
	private List<Animator> twitchAnimators;

	public BandMember(MemberSave savedMember)
	{
		// Model
		bandGenerators = GameRefs.I.bandGenerators;
		AvailableAttributes availables = new AvailableAttributes(GameRefs.I.bandGenerators);
		isBackup = savedMember.isBackup;
		isFemale = savedMember.female;
		if (isFemale)
		{
			headPrefab = FindGOByName(availables.heads.female, savedMember.head);
			legsPrefab = FindGOByName(availables.legs.female, savedMember.legs);
			torsoPrefab = FindGOByName(availables.torsos.female, savedMember.torso);
		}
		else
		{
			headPrefab = FindGOByName(availables.heads.male, savedMember.head);
			legsPrefab = FindGOByName(availables.legs.male, savedMember.legs);
			torsoPrefab = FindGOByName(availables.torsos.male, savedMember.torso);
		}

		instrumentId = (Band.Instrument)savedMember.instrument;
		instrumentPrefab = GameRefs.I.bandGenerators.instrumentPrefabs[(int)instrumentId];

		lineupPoseType = savedMember.lineupPoseType;
		lineupPoseNumber = savedMember.lineupPoseNumber;
		featurePoseType = savedMember.featurePoseType;
		featurePoseNumber = savedMember.featurePoseNumber;

		for (int i = 0; i < GameRefs.I.bandGenerators.skins.Count; i++)
		{
			foreach(Texture t in GameRefs.I.bandGenerators.skins[i].options)
			{
				if(t.name == savedMember.textureName)
				{
					texture = t;
				}
			}
		}
	}

	private GameObject FindGOByName(List<GameObject> listOfObjs, string name)
	{
		for (int i = 0; i < listOfObjs.Count; i++)
		{
			if (listOfObjs[i].name == name)
			{
				return listOfObjs[i];
			}
		}
		return null;
	}

	public BandMember(BandMemberGenerator generator, BandGenerators bandGenerators, AvailableAttributes availables)
	{
		this.bandGenerators = bandGenerators;
		saveData = new MemberSave();

		isBackup = generator.isBackup;

		// Instrument
		if (generator.instrument == InstrumentChoice.Any)
		{
			instrumentId = availables.instruments.RandomElement();
		}
		else
		{
			instrumentId = (Band.Instrument)(((int)generator.instrument) - 1);
		}
		availables.instruments.Remove(instrumentId);

		instrumentPrefab = bandGenerators.instrumentPrefabs[(int)instrumentId];

		// Gender
		if (generator.gender == GenderChoice.Any)
		{
			isFemale = Random.Range(0, 2) == 0;
		}
		else
		{
			isFemale = generator.gender == GenderChoice.Female;
		}

		// Model
		if (isFemale)
		{
			if (availables.heads.female.Count == 0) throw new System.InvalidOperationException("Not enough unique female heads!");
			if (availables.torsos.female.Count == 0) throw new System.InvalidOperationException("Not enough unique female torsos!");
			if (availables.legs.female.Count == 0) throw new System.InvalidOperationException("Not enough unique female legs!");
			headPrefab = availables.heads.female.RemoveRandomElement();
			torsoPrefab = availables.torsos.female.RemoveRandomElement();
			legsPrefab = availables.legs.female.RemoveRandomElement();
		}
		else
		{
			if (availables.heads.male.Count == 0) throw new System.InvalidOperationException("Not enough unique male heads!");
			if (availables.torsos.male.Count == 0) throw new System.InvalidOperationException("Not enough unique male torsos!");
			if (availables.legs.male.Count == 0) throw new System.InvalidOperationException("Not enough unique male legs!");
			headPrefab = availables.heads.male.RemoveRandomElement();
			torsoPrefab = availables.torsos.male.RemoveRandomElement();
			legsPrefab = availables.legs.male.RemoveRandomElement();
		}

		// Pose
		GameObject lineupPose;
		if (generator.pose == PoseChoice.Override)
		{
			lineupPose = generator.poseOverride;
		}
		else if (generator.pose == PoseChoice.Gendered)
		{
			if (isFemale)
			{
				if (availables.femalePoses.Count == 0) throw new System.InvalidOperationException("Not enough unique female poses!");
				lineupPose = availables.femalePoses.RemoveRandomElement();
			}
			else
			{
				if (availables.malePoses.Count == 0) throw new System.InvalidOperationException("Not enough unique male poses!");
				lineupPose = availables.malePoses.RemoveRandomElement();
			}
		}
		else
		{
			lineupPose = availables.universalPoses.RemoveRandomElement();
		}

		// Skin
		if (generator.skinTone == SkinToneChoice.Override)
		{
			texture = generator.skinToneOverride;
		}
		else if (generator.skinTone == SkinToneChoice.Any)
		{
			texture = bandGenerators.skins.RandomElement().options.RandomElement();
		}
		else
		{
			texture = bandGenerators.skins[generator.skinTone - SkinToneChoice.Black].options.RandomElement();
		}

		int iType = lineupPose.name.LastIndexOf('_') + 1;
		int iNumber = lineupPose.name.Length - 2;
		string poseTypeName = lineupPose.name.Substring(iType, iNumber - iType);
		lineupPoseNumber = int.Parse(lineupPose.name.Substring(iNumber));

		if (poseTypeName == "Female")
		{
			lineupPoseType = 0;
		}
		else if (poseTypeName == "Male")
		{
			lineupPoseType = 1;
		}
		else
		{
			lineupPoseType = 2;
		}

		GameObject featurePose;
		if (generator.featurePose == FeaturePoseChoice.WithSmallInstrument &&
			(instrumentId == Band.Instrument.Guitar || instrumentId == Band.Instrument.Bass))
		{
			if (availables.guitarBassPoses.Count == 0) throw new System.InvalidOperationException("Not enough unique guitar/bass poses!");
			featurePose = availables.guitarBassPoses.RemoveRandomElement();
			featurePoseType = 4;
		}
		else if (generator.featurePose == FeaturePoseChoice.WithSmallInstrument && instrumentId == Band.Instrument.Vocals)
		{
			if (availables.microphonePoses.Count == 0) throw new System.InvalidOperationException("Not enough unique microphone poses!");
			featurePose = availables.microphonePoses.RandomElement();
			featurePoseType = 3;
		}
		else
		{
			featurePose = lineupPose;
			featurePoseType = lineupPoseType;
		}

		iType = featurePose.name.LastIndexOf('_') + 1;
		iNumber = featurePose.name.Length - 2;
		poseTypeName = featurePose.name.Substring(iType, iNumber - iType);
		featurePoseNumber = int.Parse(featurePose.name.Substring(iNumber));

		saveData.female = isFemale;
		saveData.isBackup = isBackup;
		saveData.head = headPrefab.name;
		saveData.torso = torsoPrefab.name;
		saveData.legs = legsPrefab.name;
		saveData.instrument = (int)instrumentId;
		saveData.textureName = texture.name;

		saveData.lineupPoseType = lineupPoseType;
		saveData.lineupPoseNumber = lineupPoseNumber;
		saveData.featurePoseType = featurePoseType;
		saveData.featurePoseNumber = featurePoseNumber;
}

	private GameObject IncarnateInstrument(GameObject parent, GameObject prefab, Material material)
	{
		GameObject instrument = GameObject.Instantiate(prefab, parent.transform);

		SkinnedMeshRenderer skinnedRenderer = instrument.GetComponentInChildren<SkinnedMeshRenderer>();
		if (skinnedRenderer != null) 
		{
			skinnedRenderer.material = material;
		}
		else
		{
			MeshRenderer unskinnedRenderer = instrument.GetComponentInChildren<MeshRenderer>();
			if (unskinnedRenderer != null)
			{
				unskinnedRenderer.material = material;
			}
		}

		return instrument;
	}

	public void Incarnate(GameObject parent, Band.PoseContext poseContext = Band.PoseContext.Feature, Renderer plane = null, Color shadowColor = default(Color))
	{
		// Backup members only appear in a jam.
		if (isBackup && poseContext != Band.PoseContext.Jam)
		{
			parent.SetActive(false);
			return;
		}

		parent.SetActive(true);
		List<Animator> animators = new List<Animator>();

		// The parent comes with some placeholder models already attached. We
		// don't want those.
		Footilities.DestroyChildren(parent);

		bool isCastingShadow = plane != null;

		List<Material> materials = new List<Material>();
		List<GameObject> meshes = new List<GameObject>();

		// Add body parts.
		meshes.Add(GameObject.Instantiate(headPrefab, parent.transform));
		meshes.Add(GameObject.Instantiate(legsPrefab, parent.transform));
		meshes.Add(GameObject.Instantiate(torsoPrefab, parent.transform));

		Material skinMaterial = isCastingShadow ? bandGenerators.shadowedMaterial : bandGenerators.unshadowedMaterial;
		
		// Only the body parts get the skin texture.
		foreach (GameObject mesh in meshes)
		{
			SkinnedMeshRenderer renderer = mesh.GetComponentInChildren<SkinnedMeshRenderer>();
			renderer.material = skinMaterial;
			renderer.material.SetTexture("_MainTex", texture);
			materials.Add(renderer.material);
		}

		if (poseContext == Band.PoseContext.Jam ||
			(poseContext == Band.PoseContext.Feature && featurePoseType != lineupPoseType && (instrumentId == Band.Instrument.Guitar || instrumentId == Band.Instrument.Bass || instrumentId == Band.Instrument.Vocals)))
		{
			Material instrumentMaterial = isCastingShadow ? bandGenerators.shadowedInstrumentMaterial : bandGenerators.unshadowedInstrumentMaterial;
			materials.Add(instrumentMaterial);

			// Instantiate core instrument. Drums additionally warrant sticks.
			meshes.Add(IncarnateInstrument(parent, instrumentPrefab, instrumentMaterial));
			if (instrumentId == Band.Instrument.Drums)
			{
				meshes.Add(IncarnateInstrument(parent, bandGenerators.drumstickPrefab, instrumentMaterial));
			}
		}

		if (isCastingShadow)
		{
			foreach (Material material in materials)
			{
				material.SetMatrix("_World2Receiver", plane.worldToLocalMatrix);
				material.SetColor("_ShadowColor", shadowColor);
			}
		}

		int id = 0;
		switch (instrumentId)
		{
			case Band.Instrument.Guitar:
				id = 1;
				break;
			case Band.Instrument.Bass:
				id = 2;
				break;
			case Band.Instrument.Keyboard:
				id = 3;
				break;
			case Band.Instrument.Synth:
				id = 4;
				break;
			case Band.Instrument.Drums:
				id = 5;
				break;
			case Band.Instrument.Vocals:
				id = 6;
				break;
		}

		foreach (GameObject mesh in meshes)
		{
			Animator animator = mesh.AddComponent<Animator>();
			animator.runtimeAnimatorController = bandGenerators.animatorController;
			animator.avatar = bandGenerators.avatar;
			animator.keepAnimatorControllerStateOnDisable = true;
			animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

			if (!animator.gameObject.activeInHierarchy)
			{
				Debug.LogFormat("{0}: {1}", "path", Footilities.GetAbsolutePath(animator.gameObject.transform));
				Debug.LogFormat("{0}: {1}", "animator.gameObject.activeInHierarchy", animator.gameObject.activeInHierarchy);
				Debug.LogFormat("{0}: {1}", "Footilities.FindInactiveParent(animator.gameObject).gameObject.name", Footilities.FindInactiveParent(animator.gameObject.transform).gameObject.name);
			}

			if (poseContext == Band.PoseContext.Jam && id > 0)
			{
				animator.SetInteger("Instrument", id);
				animator.SetBool("IsJamming", true);
			}
			else
			{
				int poseType = poseContext == Band.PoseContext.Lineup ? lineupPoseType : featurePoseType;
				int poseNumber = poseContext == Band.PoseContext.Lineup ? lineupPoseNumber : featurePoseNumber;

				animator.SetInteger("PoseType", poseType);
				animator.SetInteger("PoseNumber", poseNumber);
				animator.SetBool("IsJamming", false);
				animator.Update(0f);
			}

			animators.Add(animator);
		}

		if (poseContext == Band.PoseContext.Jam)
		{
			twitchAnimators = animators;
		}
	}

	public void Twitch()
	{
		foreach (Animator animator in twitchAnimators)
		{
			animator.SetBool("IsTwitching", true);
		}
	}
}
