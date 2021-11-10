using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Utility;
using UnityEngine.Events;

public class TextInterceptor : MonoBehaviour
{
    public Dropdown OptionalDropdown;

    private void Start()
    {
        this.InterceptText();

        if (this.OptionalDropdown)
        {
            this.OptionalDropdown.onValueChanged.AddListener(this.InterceptDropdown);
        }
    }

    private void InterceptText()
    {
        TextMeshProUGUI text = this.GetComponent<TextMeshProUGUI>();

        if (text)
        {
            text.SetIText(text.text);
        }

        Text text2 = this.GetComponent<Text>();

        if (text2)
        {
            text2.SetIText(text2.text);
        }
    }

    public void InterceptDropdown(int value)
    {
        this.InterceptText();
    }
}
