using System.Collections;
using UnityEngine;

public class ShowIfOverride : PropertyAttribute {
	public readonly string dependsOnProperty;

	public ShowIfOverride(string dependsOnProperty)
	{
		this.dependsOnProperty = dependsOnProperty;
	}
}
