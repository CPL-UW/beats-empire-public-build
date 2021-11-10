using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MarketingInsightSelector : MonoBehaviour {

	public TextMeshProUGUI statementText;
	public RectTransform container;
	
	public void SetStatementText(string borough, string trait)
	{
		statementText.text = string.Format("In {0}, {1} songs are...", borough, trait);
	}
}
