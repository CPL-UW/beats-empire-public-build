using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UpgradeType : MonoBehaviour
{
	public enum BandUpgradeType
    {
        Ambition,
        Reliability,
        Speed,
        Persistence,
        Mood1,
        Mood2,
        Mood3,
        Mood4,
        Mood5,
        Mood6,
        Topic1,
        Topic2,
        Topic3,
        Topic4,
        Topic5,
        Topic6
    };

	public ArtistSigningPanel mainPanel;
	public Toggle m_toggle;
	public TextMeshProUGUI numberText;
	public Color incColor = new Color(1f, .78f, 0f);
	public Color origColor = new Color(1f, .78f, 0f);
	public BandUpgradeType upgradeType;

	private bool beenActivated = false;
	private GameObject plus;

	void Awake()
	{
		plus = transform.Find("Plus").gameObject;
	}

	void OnDisable()
	{
		if(numberText != null && beenActivated)
		{
			modifyText(false);
		}

		m_toggle.isOn = false;
	}

	void Start()
	{
		if (m_toggle != null)
		{
			m_toggle.onValueChanged.AddListener(delegate {
				ToggleValueChanged(m_toggle);
			});
		}
	}

	public void modifyText(bool increment)
	{
		if (numberText != null)
		{
			int origNum = int.Parse(numberText.text);
			if (increment)
			{
				numberText.text = string.Format("{0}", origNum + 1);
				numberText.color = incColor;
				beenActivated = true;
			}
			else
			{
				numberText.text = string.Format("{0}", origNum - 1);
				numberText.color = origColor;
				beenActivated = false;
			}
		}
	}

	void ToggleValueChanged(bool toggle)
	{
		plus.SetActive(!m_toggle.isOn);

		if (numberText != null && gameObject.activeSelf)
			modifyText(!beenActivated);
		mainPanel.CheckToggleStates();
	}


	public void ConfirmUpgrade()
	{
		numberText.color = origColor; 
		m_toggle.isOn = false;
	}

}
