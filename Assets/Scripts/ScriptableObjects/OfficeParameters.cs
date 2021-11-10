using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName="OfficeParameters", menuName="Parameters/OfficeParameters", order = 1)]
public class OfficeParameters : ScriptableObject
{
	[System.Serializable]
	public class StaffMemberConfigurator {
		[HideInInspector]
		public string name;
		public float minTimeBetweenFidgets;
		public float maxTimeBetweenFidgets;
		public Texture skin;
	}
	public StaffMemberConfigurator[] staffMemberConfigurators;
}
