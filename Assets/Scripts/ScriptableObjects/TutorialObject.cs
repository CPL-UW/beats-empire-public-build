using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "TutorialObject", menuName = "TutorialObject", order = 1)]
public class TutorialObject : ScriptableObject
{
	[Multiline]
	public string chracterText;
}
