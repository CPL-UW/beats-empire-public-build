using UnityEngine;

[System.Serializable]
public class TraitSampling {
	[Range(0, 3)]
	public int slotCount;
	public int iteration;

	public TraitSampling Clone() {
		return new TraitSampling {
			slotCount = slotCount,
			iteration = iteration
		};
	}
}
