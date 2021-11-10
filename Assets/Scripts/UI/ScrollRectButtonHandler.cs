using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollRectButtonHandler : MonoBehaviour
{
    public ScrollRect ScrollRect;

    private bool playerIsPressingUp;
    private bool playerIsPressingDown;

    private void Update()
    {
        if (this.playerIsPressingDown)
        {
            this.ScrollRect.verticalNormalizedPosition -= Time.deltaTime * this.ScrollRect.scrollSensitivity / 10f;
        }

        if (this.playerIsPressingUp)
        {
            this.ScrollRect.verticalNormalizedPosition += Time.deltaTime * this.ScrollRect.scrollSensitivity / 10f;
        }
    }

    public void UpButtonMouseEvent(bool flag)
    {
        this.playerIsPressingUp = flag;
    }

    public void DownButtonMouseEvent(bool flag)
    {
        this.playerIsPressingDown = flag;
    }
}
