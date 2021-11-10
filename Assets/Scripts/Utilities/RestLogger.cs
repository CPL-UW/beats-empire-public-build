using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.IO;

public class RestLogger : MonoBehaviour
{
	// was used for rest logs
	public string baseURL = "http://54.149.126.189:14056/logs";

	private void Awake()
	{
		StartCoroutine(LoadFromFile());
	}

	private IEnumerator LoadFromFile()
	{
		WWW www = new WWW(Application.streamingAssetsPath + "/ipAddr.txt");
		yield return www;
		baseURL = www.text;
		GameRefs.I.debugText.text = "IP: " + www.text;
	}

    public void PostReq(JSONObject json)
    {
        Persister.Persist(json.ToString());
    }

	public string GetUserID()
	{
		return Persister.UserID();
	}

	public string GetUserEmail()
	{
		return Persister.UserEmail();
	}

}
