using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class InterceptedText : TMPro.TextMeshProUGUI
{
    public static Dictionary<string, string> TextKVPs;

    private new void Start()
    {
        this.SetText(this.text);
    }

    public new void SetText(string text)
    {
        if (Application.isPlaying)
        {
            bool recheck = true;
            while (recheck)
            {
                string[] splitText = text.Split(new char[] { '[', ']' });

                recheck = false;

                for (int i = 0; i < splitText.Length; i++)
                {
                    if (InterceptedText.TextKVPs.ContainsKey(splitText[i]))
                    {
                        splitText[i] = InterceptedText.TextKVPs[splitText[i]];
                        recheck = true;
                    }
                }

                text = string.Concat(splitText);
            }
        }

        base.SetText(text);
    }
}
