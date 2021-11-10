using System.Collections;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameController))]
public class GameControllerEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		GameController gc = (GameController) target;
		if (GUILayout.Button("Debug Recording"))
		{
			gc.DebugRecordingBand();
		}

		if (GUILayout.Button("Generate 100 Song Names"))
		{
			for (int i = 0; i < 100; ++i) {
				Debug.Log("- " + Utility.Utilities.InterceptText(SongNameGenerator.Instance.GetRandomSongTitle()));
			}
		}

		if (GUILayout.Button("Auto Next"))
		{
			gc.AutoNext();
		}

		if (GUILayout.Button("Record and Release"))
		{
			gc.RecordAndRelease();
		}

		if (GUILayout.Button("Spam Songs"))
		{
			gc.PrimeSongs();
		}

		if (GUILayout.Button("Perfect Artists"))
		{
			gc.PerfectSignedBands();
		}

		if (GUILayout.Button("Release Random"))
		{
			gc.ReleaseRandom();
		}
	}		
}
