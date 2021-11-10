using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

[Serializable]
[CreateAssetMenu(fileName = "FeedbackTexts", menuName = "FeedbackTexts", order = 1)]
public class FeedbackTexts : ScriptableObject
{
	public FeedbackObject[] goodLongGenre;
	public FeedbackObject[] goodLongMood;
	public FeedbackObject[] goodLongTopic;
	public FeedbackObject[] goodLongHitMeter;

	public FeedbackObject[] goodShortGenre;
	public FeedbackObject[] goodShortMood;
	public FeedbackObject[] goodShortTopic;
	public FeedbackObject[] goodShortHitMeter;

	public FeedbackObject[] mediocreGenre;
	public FeedbackObject[] mediocreMood;
	public FeedbackObject[] mediocreTopic;
	public FeedbackObject[] mediocreHitMeter;

	public FeedbackObject[] badShortGenre;
	public FeedbackObject[] badShortMood;
	public FeedbackObject[] badShortTopic;
	public FeedbackObject[] badShortHitMeter;

	public FeedbackObject[] badLongGenre;
	public FeedbackObject[] badLongMood;
	public FeedbackObject[] badLongTopic;
	public FeedbackObject[] badLongHitMeter;

	public FeedbackObject[] notTrendingGenre;
	public FeedbackObject[] notTrendingMood;
	public FeedbackObject[] notTrendingTopic;

	public FeedbackObject[] actualMostPopGenre;
	public FeedbackObject[] actualMostPopMood;
	public FeedbackObject[] actualMostPopTopic;

	public string[] conjunctions;
}

[Serializable]
public class FeedbackObject
{
	public string string1;
	public string string2;
}