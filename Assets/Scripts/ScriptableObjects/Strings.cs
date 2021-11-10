using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName="Strings", menuName="Parameters/Strings", order = 1)]
public class Strings : ScriptableObject
{
	// TAKES RISKS TO CREATE GREAT (OR TERRIBLE) SONGS
	// RARELY MAKES TERRIBLE SONGS, MAINTAINING HIGH STANDARDS
	// WORKS FAST, IMPROVING SONG QUALITY MORE QUICKLY
	// CAN RECORD LONGER FOR MORE CHANCES TO IMPROVE SONGS
	public string[] skillTooltipLabels;

	public string signArtistHeader;
	public string manageArtistHeader;
	public string upgradeArtistHeader;
	public string swapArtistHeader;
}
