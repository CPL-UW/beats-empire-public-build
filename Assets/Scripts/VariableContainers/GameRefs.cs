using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Utility;
using UnityEngine.UI;

public static class SeedData
{
	public static int seed;
}

	public class GameRefs : MonoBehaviour
{

    private static GameRefs _instance;
    public static GameRefs I { get { return _instance; } }

	public TopChartsParameters topChartsParameters;
	public SharedParameters sharedParameters;
	public HudController hudController;
	public SongReleaseParameters songReleaseParameters;
	public ArtistViewParameters artistViewParameters;
	public BandGenerators bandGenerators;
	public DataCollectionParameters dataCollectionParameters;
	public Colors colors;
	public Strings strings;
	public UpgradeVariables m_upgradeVariables;
	public TopChartsView m_topCharts;
    public ArtistSigningView m_artistSigningView;
	public MarketingView m_marketingView;
	public GameInitializationVariables m_gameInitVars;
	public DataSimulationManager m_dataSimulationManager;
	public SongReleaseVariables m_songReleaseVariables;
    public GameController m_gameController;
	public DataCollectionController m_dataCollect;
	public CurrentCashTooltipControl cashTooltipValues;
	public RecordingSlotsTooltipControl recordingTooltipValues;
	public StatSubType m_lastLocationGraphed;
	public RecordingAudio m_recordingAudio;
	public SfxAudio m_sfxAudio;
	public TutorialController m_tutorialController;
	public Band m_globalLastBand;
	public string m_globalLastScreen;
	public Camera m_mainCamera;
	public RestLogger m_restLogger;
	public bool forceTDDOn = false;
	public string PlayerUniqueID;
	public Text debugText;
	public int currentBPM = 100;
	public bool preserveLastSelected = false;

	public int forceNextSongToSales = -1;

	[Header("Insight Graph Variables")]
    public float trendsLineThickness = 3f;
    public float unselectableLineThickness = 1f;
	public float insightSelectableLineThickness = 3f;
    public float glowThickness = 12f;

	public Dictionary<StatType, List<bool?>> isSampledAt;

	private Dictionary<StatSubType, int> recordsAchieved = new Dictionary<StatSubType, int>();

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
		}

		traitSamplings = new Dictionary<StatType, TraitSampling>();
		if (m_gameInitVars.isSampledMaximally)
		{
			traitSamplings[StatType.GENRE] = new TraitSampling { slotCount = 3, iteration = 0 };
			traitSamplings[StatType.MOOD] = new TraitSampling { slotCount = 3, iteration = 0 };
			traitSamplings[StatType.TOPIC] = new TraitSampling { slotCount = 3, iteration = 0 };
		}
		else
		{
			traitSamplings[StatType.GENRE] = dataCollectionParameters.initialGenreSampling.Clone();
			traitSamplings[StatType.MOOD] = dataCollectionParameters.initialMoodSampling.Clone();
			traitSamplings[StatType.TOPIC] = dataCollectionParameters.initialTopicSampling.Clone();
		}

		isSampledAt = new Dictionary<StatType, List<bool?>> {
			{ StatType.MOOD, new List<bool?>() },
			{ StatType.TOPIC, new List<bool?>() },
			{ StatType.GENRE, new List<bool?>() },
		};

		storageSlotCount = m_gameInitVars.initialStorageSlotCount;
    }

	private Texture2D _screenShot;

	private byte[] TakeScreenShot()
	{
		//yield return new WaitForEndOfFrame();

		int resWidth = 400;
		int resHeight = 300;

		RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
		m_mainCamera.targetTexture = rt;
		_screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
		m_mainCamera.Render();
		RenderTexture.active = rt;
		_screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
		m_mainCamera.targetTexture = null;
		RenderTexture.active = null;
		Destroy(rt);

		return _screenShot.EncodeToPNG();
	}

	private int GetEpochTime()
	{
		System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
		return (int)(System.DateTime.UtcNow - epochStart).TotalSeconds;
	}

	public void PostGameState(bool includeAllData = true, string customAction = "", string actionValue = "")
	{
		if(Application.platform == RuntimePlatform.WebGLPlayer || forceTDDOn)
		{
			JSONObject json = new JSONObject(JSONObject.Type.OBJECT);

			// JSON info
			json.AddField("GameVersion", PlayerInformation.versionNum);
			json.AddField("CMSLogVersion", "1.11");
			json.AddField("lastChangedDate", "1/3/2019");
			json.AddField("isLogVerbose", includeAllData);
			json.AddField("epochTime", GetEpochTime());
			json.AddField("realTimeUTC", System.DateTime.UtcNow.ToString());
			json.AddField("upTimeSeconds", Time.time);
			json.AddField("saveGameStartDate", GameRefs.I.m_gameController.Saves.dateCreated);
			json.AddField("gameSeed", SeedData.seed);
			json.AddField("playerUniqueID", GameRefs.I.m_restLogger.GetUserEmail() == null ? "" : StrToMD5(GameRefs.I.m_restLogger.GetUserEmail()));
			json.AddField("userEmail", GameRefs.I.m_restLogger.GetUserEmail() == null ? "" : GameRefs.I.m_restLogger.GetUserEmail());

			if(includeAllData)
			{
				// General system
				json.AddField("deviceModel", SystemInfo.deviceModel);
				json.AddField("deviceName", SystemInfo.deviceName);
				json.AddField("deviceType", SystemInfo.deviceType.ToString());
				json.AddField("deviceUniqueIdentifier", SystemInfo.deviceUniqueIdentifier);
				json.AddField("systemMemorySize", SystemInfo.systemMemorySize);
				json.AddField("operatingSystem", SystemInfo.operatingSystem);
				json.AddField("operatingSystemFamily", SystemInfo.operatingSystemFamily.ToString());
				json.AddField("supportsAudio", SystemInfo.supportsAudio);
				json.AddField("batteryLevel", SystemInfo.batteryLevel);
				json.AddField("batteryStatus", SystemInfo.batteryStatus.ToString());
				// Processor
				json.AddField("processorCount", SystemInfo.processorCount);
				json.AddField("processorFrequency", SystemInfo.processorFrequency);
				json.AddField("processorType", SystemInfo.processorType);
				// Graphics
				json.AddField("graphicsDeviceID", SystemInfo.graphicsDeviceID);
				json.AddField("graphicsDeviceName", SystemInfo.graphicsDeviceName);
				json.AddField("graphicsDeviceType", SystemInfo.graphicsDeviceType.ToString());
				json.AddField("graphicsMultiThreaded", SystemInfo.graphicsMultiThreaded);
				json.AddField("graphicsShaderLevel", SystemInfo.graphicsShaderLevel);
				json.AddField("graphicsDeviceVersion", SystemInfo.graphicsDeviceVersion); // Issues?
				json.AddField("graphicsDeviceVendorID", SystemInfo.graphicsDeviceVendorID);
			}

			/************* GAME INFO ***************/
			json.AddField("triggerAction", customAction);
			json.AddField("actionValue", actionValue);
			json.AddField("currentTurn", m_gameController.currentTurn);
			json.AddField("currentCash", m_gameController.GetCash());
			json.AddField("recordingsInProgress", m_gameController.GetNumRecordings());
			json.AddField("currentFans", m_dataSimulationManager.GetTotalFollowerCount());
			json.AddField("currentScreen", m_globalLastScreen);


			if (includeAllData)
			{

				// Previous week info
				json.AddField("previousStorageCosts", cashTooltipValues.saveStorageCosts);
				json.AddField("previousResidualCash", cashTooltipValues.saveCashFromListens);
				json.AddField("previousBandUpkeepCost", cashTooltipValues.saveBandUpkeep);
				json.AddField("previousSalesCash", cashTooltipValues.saveCashFromSongs);

				// // Signed Bands + Recording songs
				JSONObject bandsInfo = new JSONObject(JSONObject.Type.OBJECT);
				List<Band> signedBands = m_gameController.GetSignedBands();
				bandsInfo.AddField("numSignedBands", signedBands.Count);
				for (int i = 0; i < signedBands.Count; i++)
				{

					JSONObject bandInfo = new JSONObject(JSONObject.Type.OBJECT);
					bandInfo.AddField(string.Format("bandInfo", i), new JSONObject(JsonUtility.ToJson(signedBands[i].GetSavedData())));
					if (signedBands[i].IsRecordingSong())
					{
						bandInfo.AddField("recordingSong", new JSONObject(JsonUtility.ToJson(signedBands[i].GetRecordingSong().GetDataForLogs())));
					}
					
					else
						bandInfo.AddField("recordingSong", "");

					bandsInfo.AddField(string.Format("signedBand{0}", i), bandInfo);
				}
				json.AddField("signedBandInfo", bandsInfo);

				// // Unsigned bands
				JSONObject unsignedBandsInfo = new JSONObject(JSONObject.Type.OBJECT);
				unsignedBandsInfo.AddField("numUnsignedBands", m_gameController.GetAvailableBands().Count);
				for (int i = 0; i < m_gameController.GetAvailableBands().Count; i++)
				{
					unsignedBandsInfo.AddField(string.Format("unsignedBand{0}", i), new JSONObject(JsonUtility.ToJson(m_gameController.GetAvailableBands()[i].GetSavedData())));
				}
				json.AddField("unsignedBandInfo", unsignedBandsInfo);

				// // Insights
				JSONObject unconfirmedInsightsInfo = new JSONObject(JSONObject.Type.OBJECT);
				unconfirmedInsightsInfo.AddField("numUnconfirmedInsights", m_gameController.MarketingInsights.unconfirmedInsights.Count);
				for(int i = 0; i < m_gameController.MarketingInsights.unconfirmedInsights.Count; i++)
				{
					unconfirmedInsightsInfo.AddField(string.Format("unconfirmedInsight{0}", i), 
						new JSONObject(JsonUtility.ToJson(m_gameController.MarketingInsights.unconfirmedInsights[i].GetDataForLogs())));
				}
				json.AddField("unconfirmedInsightsInfo", unconfirmedInsightsInfo);

				JSONObject confirmedInsightsInfo = new JSONObject(JSONObject.Type.OBJECT);
				confirmedInsightsInfo.AddField("numUnconfirmedInsights", m_gameController.marketingInsightList.Count);
				for (int i = 0; i < m_gameController.marketingInsightList.Count; i++)
				{
					confirmedInsightsInfo.AddField(string.Format("confirmedInsight{0}", i), 
						new JSONObject(JsonUtility.ToJson(m_gameController.marketingInsightList[i].GetDataForLogs())));
				}
				json.AddField("confirmedInsightsInfo", confirmedInsightsInfo);

				JSONObject pastInsightsInfo = new JSONObject(JSONObject.Type.OBJECT);
				pastInsightsInfo.AddField("pastInsightsInfo", GameRefs.I.m_gameController.previousMarketingInsights.Count);
				for (int i = 0; i < GameRefs.I.m_gameController.previousMarketingInsights.Count; i++)
				{
					pastInsightsInfo.AddField(string.Format("confirmedInsight{0}", i), 
						new JSONObject(JsonUtility.ToJson(GameRefs.I.m_gameController.previousMarketingInsights[i].GetDataForLogs())));
				}
				json.AddField("pastInsightsInfo", pastInsightsInfo);

				// Marketing Unlocks
				foreach (KeyValuePair<StatSubType, MarketingView.CardUnlocks[]> key in GameRefs.I.m_marketingView.GetBoroughUnlocks())
				{
					for (int i = 0; i < 3; i++)
					{
						json.AddField(string.Format("marketingUnlocks_{0}_card{1}", key.Key.Name, i), key.Value[i].getNumUnlocks());
					}
				}
				foreach (KeyValuePair<StatSubType, MarketingView.CardUnlocks[]> key in GameRefs.I.m_marketingView.GetGenreUnlocks())
				{
					for (int i = 0; i < 3; i++)
					{
						json.AddField(string.Format("marketingUnlocks_{0}_card{1}", key.Key.Name, i), key.Value[i].getNumUnlocks());
					}
				}

				// Song chart positions
				for (int i = 0; i < GameRefs.I.m_topCharts.songSalesList.Count; i++)
					json.AddField(string.Format("releasedSong{0}", i), new JSONObject(JsonUtility.ToJson(GameRefs.I.m_topCharts.songSalesList[i])));

				recordsAchieved.Clear();

				for(int i = 0; i < 6; i++)
				{
					int completionStatus = GameRefs.I.m_topCharts.GetGenreCompletionStatus(StatSubType.List[StatSubType.ROCK_ID + i]);

					json.AddField(string.Format("{0}HasGold", StatSubType.List[StatSubType.ROCK_ID + i].Name), completionStatus > 0);
					json.AddField(string.Format("{0}PlatinumsAchieved", StatSubType.List[StatSubType.ROCK_ID + i].Name), Mathf.Max(0, completionStatus - 1));
				}

				// Graph Collection Rates
				json.AddField("dataCollectedGenre", GameRefs.I.traitSamplings[StatType.GENRE].slotCount);
				json.AddField("dataCollectedMood", GameRefs.I.traitSamplings[StatType.MOOD].slotCount);
				json.AddField("dataCollectedTopic", GameRefs.I.traitSamplings[StatType.TOPIC].slotCount);
				json.AddField("dataCollectStorageSlots", GameRefs.I.storageSlotCount);

				/******** END GAME INFO *************/

				// For all locations
				Dictionary<StatSubType, List<float>> data;
				List<StatSubType> locations = new List<StatSubType> { StatSubType.TURTLE_HILL, StatSubType.THE_BRONZ, StatSubType.KINGS_ISLE, StatSubType.MADHATTER, StatSubType.BOOKLINE, StatSubType.IRONWOOD };

				foreach(StatSubType location in locations)
				{
					data = DataSimulationManager.Instance.GetCachedIndustryLocationData(location);

					foreach (KeyValuePair<StatSubType, List<float>> kvp in data)
					{
						// Need to check all of the types of this super to see whose is the highest
						if (kvp.Key.SuperType == StatType.GENRE ||
							kvp.Key.SuperType == StatType.TOPIC ||
							kvp.Key.SuperType == StatType.MOOD)
						{
							List<float?> values = kvp.Value.Cast<float?>().ToList();
							json.AddField(string.Format("{0}_{1}_value", location.Name, Utilities.InterceptText(kvp.Key.Name)), values[values.Count - 1].Value);
						}
					}
				}


			} // End include all data

			m_restLogger.PostReq(json);
		}
	}

	public int storageSlotCount { get; set; }

	public int filledSlotCount
	{
		get
		{
			return traitSamplings[StatType.MOOD].slotCount + traitSamplings[StatType.GENRE].slotCount + traitSamplings[StatType.TOPIC].slotCount;
		}
	}

	public Dictionary<StatType, TraitSampling> traitSamplings { get; set; }

	public string StrToMD5(string strToEncrypt)
	{
		System.Text.UTF8Encoding ue = new System.Text.UTF8Encoding();
		byte[] bytes = ue.GetBytes(strToEncrypt);

		// encrypt bytes
		System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
		byte[] hashBytes = md5.ComputeHash(bytes);

		// Convert the encrypted bytes back to a string (base 16)
		string hashString = "";

		for (int i = 0; i < hashBytes.Length; i++)
		{
			hashString += System.Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
		}

		return hashString.PadLeft(32, '0');
	}
}


/* Screenshot stuff

// attach screenshot (very large, need to compress
 string ss = Convert.ToBase64String(TakeScreenShot());
 gameInfo.AddField("screenshot", ss);

*/

