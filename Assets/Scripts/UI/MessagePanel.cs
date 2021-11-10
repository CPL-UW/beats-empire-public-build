using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessagePanel : MonoBehaviour {
    private static MessagePanel _instance;
    public static MessagePanel Instance
    {
        get
        {
            //If _instance is null then we find it from the scene 
            if (_instance == null)
                _instance = GameObject.FindObjectOfType<MessagePanel>();
            return _instance;
        }
    }

    [SerializeField]
    private Text messageText;
    
        CanvasTools ct;
	// Use this for initialization
	void Awake () {
        ct=GetComponent<CanvasTools>();
	}

    //this is always on top of everything - its like a generic error pop up - or message pop up
    public void CallOpen(string text)
    {
        ct.CallFadeCanvas(1);
        messageText.text = text;
        ct.isOpen = true;
    }

    public void Close() {
        ct.ToggleMe(false);
        messageText.text = "";
    }
}
