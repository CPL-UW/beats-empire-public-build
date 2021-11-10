using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class PopulationData  {
    public StatSubType Location;
    public List<GenrePersonBucket> GenreBuckets;
    private List<float> followers;

    private float pendingFollowers;

    public PopulationData(StatSubType location, List<GenrePersonBucket> genreBuckets, float initialFollowersPercent)
    {
        this.Location = location;
        this.GenreBuckets = genreBuckets;
        this.followers = new List<float>();
        this.pendingFollowers = GetPopulation() * initialFollowersPercent;
    }

    public float GetPopulation()
    {
        float sum = 0;

        foreach (GenrePersonBucket bucket in this.GenreBuckets)
        {
            sum += bucket.Population;
        }

        return sum;
    }

    public float GetFollowers()
    {
        return this.followers[this.followers.Count - 1];
    }

    public float GetPendingFollowers()
    {
		if (GameRefs.I.m_marketingView.IsBoroughUnlocked(Location))
			return this.pendingFollowers;
		else
			return 0;
    }

    public List<float> GetFollowerData()
    {
        return this.followers;
    }

    public void AddPendingFollowers(float followers)
    {
        this.pendingFollowers += followers;
    }

	public void ReplacePendingFollowers(float followers)
	{
		this.pendingFollowers = followers;
	}

    public void CommitPendingFollowers()
    {
        this.followers.Add(this.pendingFollowers);
    }
}
