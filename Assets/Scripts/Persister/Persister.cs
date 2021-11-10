using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class Persister {
  public static void Persist(string json)
  {
#if UNITY_WEBGL && !UNITY_EDITOR
    PersistFirebase(json);
#else
		JSONObject debugTest = new JSONObject(json);
		Debug.LogFormat("{0}, {1}, {2} {3}", debugTest["isLogVerbose"], debugTest["triggerAction"], debugTest["actionValue"], debugTest["currentScreen"]);
	//Debug.Log(json );
#endif
  }

  public static string UserID()
  {
#if UNITY_WEBGL && !UNITY_EDITOR
	  return FirebaseUserID();
#else
	  return null;
#endif
  }

  public static int GetPendingSavesCount()
  {
#if UNITY_WEBGL && !UNITY_EDITOR
	  return PendingSavesCount();
#else
	  return 0;
#endif
  }

  public static string UserEmail()
  {
#if UNITY_WEBGL && !UNITY_EDITOR
	  return FirebaseUserEmail();
#else
	  return null;
#endif
  }

  public static void SaveCMSData(string json)
  {
#if UNITY_WEBGL && !UNITY_EDITOR
    SaveData(json);
#endif
  }

  public static string LoadCMSData()
  {
#if UNITY_WEBGL && !UNITY_EDITOR
	return LoadData();
#else
	return null;
#endif
  }

#if UNITY_WEBGL && !UNITY_EDITOR
  [DllImport("__Internal")]
  private static extern void PersistFirebase(string json);

  [DllImport("__Internal")]
  private static extern string SaveData(string json);

  [DllImport("__Internal")]
  private static extern string LoadData();

  [DllImport("__Internal")]
  private static extern string FirebaseUserID();

  [DllImport("__Internal")]
  private static extern string FirebaseUserEmail();

  [DllImport("__Internal")]
  private static extern int PendingSavesCount();
#endif
}
