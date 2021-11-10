using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName="RecordingParameters", menuName="Parameters/RecordingParameters", order = 1)]
public class RecordingParameters : ScriptableObject
{
	public float recordingFullEaseInitialScale;
	public float recordingFullEaseTime;
}
