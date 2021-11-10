using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadCheckContinue : MonoBehaviour {

	public GameObject continueObj;

	public void Start()
	{
		Debug.Log("Attempting load...");
		Persister.LoadCMSData();
	}

	public void LoadCallback(string status)
	{
		if(status == null || status == "null" || status == "")
		{
			continueObj.SetActive(false);
		}
		else
		{
			SaveGameController.MusicStudioSave loadedData = new SaveGameController.MusicStudioSave();
			loadedData = JsonUtility.FromJson<SaveGameController.MusicStudioSave>(status);
			if (loadedData.gameCompleted)
			{
				continueObj.SetActive(false);
			}
		}
	}
}
