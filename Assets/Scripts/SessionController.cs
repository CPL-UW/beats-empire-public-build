using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class SessionController : MonoBehaviour {
	public static SessionController S;

	public bool isVisible;
	public bool isConnected;

	private List<UnityAction<bool>> connectionListeners;

	void Awake()
	{
		isConnected = true;
		DontDestroyOnLoad(this);
		S = this;
		connectionListeners = new List<UnityAction<bool>>();
	}

	void Start()
	{
#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS
		OnVisible();
#endif

		// We want this scene to always get loaded, no matter what scene we
		// start from. When some other scene loads this one, we don't need to
		// auto-advance to pregame. But when this is the only scene, we do need
		// to auto-advance.
		if (SceneManager.sceneCount == 1)
		{
			SceneManager.LoadSceneAsync("Pregame", LoadSceneMode.Additive);
		}
	}

	void OnVisible()
	{
		isVisible = true;
	}

	void OnConnectionChange(int status)
	{
		isConnected = status == 1;
		foreach (UnityAction<bool> listener in connectionListeners)
		{
			listener(isConnected);
		}
	}

	public void AddListener(UnityAction<bool> listener)
	{
		connectionListeners.Add(listener);
	}

	public void ClearListeners()
	{
		connectionListeners.Clear();
	}
}
