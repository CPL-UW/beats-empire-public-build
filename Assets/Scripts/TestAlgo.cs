using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAlgo : MonoBehaviour
{
    //float wordOfMouth = 100000;
    //float wordOfMouthPrevTurn = 118000;
    //float thisTurnAppeal = .2f;
    //float numTurnsSinceRelease = 2;
    //float latencyWordOfMouth = 1;
    //float marketImpenetrability = 1;
    //float population = 1000000;

    void Start()
    {
        //Word of Mouth = Listens from Previous Turn *(1 + (Appeal as of This Turn ^ (Number of Turns Since Release *Latency of Word of Mouth )) *(1 - (Listens from Previous Turn / Population)) ^ Market Impenetrability)
        //float wordOfMouth =wordOfMouthPrevTurn * (1+ Mathf.Pow(thisTurnAppeal, (numTurnsSinceRelease * latencyWordOfMouth)) * (Mathf.Pow((1 - (wordOfMouthPrevTurn / population)), marketImpenetrability)));
    //    Debug.Log("Word Of Mouth:"+wordOfMouth);
    }

    //float wordOfMouth = 100000;
    //float wordOfMouthPrevTurn = 118000;
    //float thisTurnAppeal = .2f;
    //float numTurnsSinceRelease = 2;
    //float latencyWordOfMouth = 1;
    //float marketImpenetrability = 1;
    //float population = 1000000;
    private float GetWordOfMouth(float wordOfMouthPrevTurn,float thisTurnAppeal, float numTurnsSinceRelease, float latencyWordOfMouth, float population, float marketImpenetrability)
    {
        float wordOfMouth = wordOfMouthPrevTurn * (1 + Mathf.Pow(thisTurnAppeal, (numTurnsSinceRelease * latencyWordOfMouth)) * (Mathf.Pow((1 - (wordOfMouthPrevTurn / population)), marketImpenetrability)));
        Debug.Log("Word Of Mouth:" + wordOfMouth);
        return wordOfMouth;
    }
}
