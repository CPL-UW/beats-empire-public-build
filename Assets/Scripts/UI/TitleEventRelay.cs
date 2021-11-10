using System.Collections;
using UnityEngine;

public class TitleEventRelay : MonoBehaviour {
	public GameInitializer initializer;

	public void OnOff()
	{
		initializer.LoadNextScene();
	}
}
