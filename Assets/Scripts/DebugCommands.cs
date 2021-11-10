using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugCommands : MonoBehaviour
{
    public GameController GameController;
	public GameObject Console;

    private bool isShowing;

    public void GetLotsOfMoney()
    {
        this.GameController.Debug_GetMoney(100000000f);
    }
	public void RemoveLotsOfMoney()
	{
		this.GameController.Debug_GetMoney(-100000000f);
	}
	public void SetMoneyToZero()
	{
		this.GameController.RemoveCash(GameController.GetCash());
	}

	public void RefreshAvailableArtists()
    {
        this.GameController.Debug_RefreshAvailableArtists();
    }

    public void DoubleFans(int x)
    {
        switch (x)
        {
            case 0:
                this.GameController.Debug_DoubleFans(StatSubType.NONE);
                break;
            case 1:
                this.GameController.Debug_DoubleFans(StatSubType.TURTLE_HILL);
                break;
            case 2:
                this.GameController.Debug_DoubleFans(StatSubType.MADHATTER);
                break;
            case 3:
                this.GameController.Debug_DoubleFans(StatSubType.IRONWOOD);
                break;
            case 4:
                this.GameController.Debug_DoubleFans(StatSubType.THE_BRONZ);
                break;
            case 5:
                this.GameController.Debug_DoubleFans(StatSubType.KINGS_ISLE);
                break;
            case 6:
                this.GameController.Debug_DoubleFans(StatSubType.BOOKLINE);
                break;
        }
    }

	public void SpamSongs() {
		GameController.PrimeSongs();
	}

	public void PerfectSignedBands() {
		GameController.PerfectSignedBands();
	}
}
