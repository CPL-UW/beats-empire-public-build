using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SurgeGenerator : MonoBehaviour
{
    public int Interval;
    public SurgeList SurgePool;

    private int turnsSinceLastSurge;

    private void Awake()
    {
        this.turnsSinceLastSurge = 6;
    }

	public SurgeData GetSurgeDataByUniqueID(int id)
	{
		for(int i = 0; i < SurgePool.Contents.Count; i++)
		{
			if (SurgePool.Contents[i].uniqueID == id)
				return SurgePool.Contents[i];
		}
		return null;
	}

	public SurgeData AdvanceTurnAndGetPendingSurge(List<Surge> activeSurges, out int chosenSurge)
    {
        this.turnsSinceLastSurge += 1;
		chosenSurge = -1;

        if (this.turnsSinceLastSurge >= this.Interval && Random.value > Mathf.Pow(0.5f, 1 + this.turnsSinceLastSurge - this.Interval))
        {
            this.turnsSinceLastSurge = 0;

            List<SurgeData> validSurges = this.SurgePool.Contents.Where(x => !activeSurges.Select(y => y.AffectedSubType).ToList().Contains(StatSubType.List[x.affectedSubStatType.SubType]) || x.affectedSubStatType.SubType == StatSubType.RANDOM_ID).ToList();

            if (validSurges.Count > 0)
            {
				SurgeData chosenData = validSurges[Random.Range(0, validSurges.Count)];
				chosenSurge = chosenData.uniqueID;
				return chosenData;
            }
        }

        return null;
    }
}
