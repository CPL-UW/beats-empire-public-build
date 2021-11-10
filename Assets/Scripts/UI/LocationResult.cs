using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class LocationResult : MonoBehaviour {

    public Text m_LocationNameText;
    public Text m_SalesValText;
    public Text m_AcclaimValText;

    public void SetResults(string location, float sales, float acclaim)
    {
        m_LocationNameText.text = location;
        m_SalesValText.text = sales.ToString();
        m_AcclaimValText.text = acclaim.ToString();
    }
}
