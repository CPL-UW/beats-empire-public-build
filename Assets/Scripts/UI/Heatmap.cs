using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BeauRoutine;
using TMPro;

public class Heatmap : MonoBehaviour
{
	[Header("Unlocked color block")]
	public ColorBlock unlockedColorBlock;

	[Header("Locked color block")]
	public ColorBlock lockedColorBlock;

	[Header("Others")]
    public Color BaseColor;

	public Color DisabledColor;
	public Color DisabledTextColor;

	public bool locsCanBeDisabled = false;
	public List<TextMeshProUGUI> NameTexts;
	public List<Toggle> Toggles;
	public List<Image> Locks;
	private Routine m_routine;

    public void UpdateSaturation(int selectedBoroughId = StatSubType.NONE_ID)
    {
        for (int i = 0; i < this.NameTexts.Count; i++)
        {
			StatSubType boroughType = IntToBoroughType(i);

			if (locsCanBeDisabled)
			{
				if (GameRefs.I.m_marketingView.IsBoroughUnlocked(IntToBoroughType(i)))
				{
					this.Toggles[i].interactable = true;
					this.Toggles[i].colors = unlockedColorBlock;
					this.Locks[i].gameObject.SetActive(false);
					this.NameTexts[i].color = boroughType.ID == selectedBoroughId ? Color.white : Color.black;
				}
				else
				{
					this.Toggles[i].interactable = false;
					this.Toggles[i].colors = lockedColorBlock;
					this.Locks[i].gameObject.SetActive(true);
					this.NameTexts[i].color = DisabledTextColor;
				}
			}
		}
    }

	private StatSubType IntToBoroughType(int borough)
	{
		switch (borough)
		{
			case 0: return StatSubType.BOOKLINE;
			case 1: return StatSubType.THE_BRONZ;
			case 2: return StatSubType.IRONWOOD;
			case 3: return StatSubType.KINGS_ISLE;
			case 4: return StatSubType.MADHATTER;
			case 5: return StatSubType.TURTLE_HILL;
			default: return StatSubType.NONE;
		}
	}
}
