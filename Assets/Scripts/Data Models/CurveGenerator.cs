using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurveGenerator : MonoBehaviour
{
    private static CurveGenerator _instance;
    public static CurveGenerator Instance
    {
        get
        {
            //If _instance is null then we find it from the scene 
            if (_instance == null)
                _instance = GameObject.FindObjectOfType<CurveGenerator>();
            return _instance;
        }
    }

    public List<SurgeData> m_PredefinedSurges = new List<SurgeData>();

    /// <summary>
    /// Generate a list of floats, which represent the point in time along the curve.
    /// This is for a surge of interest modifier
    /// Currently just supports 1 list. Not able to generate multiple surges yet
    /// </summary>
    public List<float> GenerateSurgeModifierList(SurgeData surgeData)
    {
        float numValues = surgeData.surgeLength;
        //get correct increment, so we can generate keys along the curve (depending how many we require)
        float increment = 1 / numValues;
        float timePosition = 0;

        List<float> dataPoints = new List<float>();

        //loop through and calculate points
        for (int i = 0; i < numValues; i++)
        {
            //What is our position + value?
            //Debug.Log("time: " + timePosition + " value:" + m_Curve.Evaluate(timePosition));
            dataPoints.Add(surgeData.curve.Evaluate(timePosition));
            //Update our timePosition
            timePosition += increment;
        }
        return dataPoints;
    }
}
