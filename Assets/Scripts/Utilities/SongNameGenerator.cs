using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SongNameGenerator : MonoBehaviour
{
    private static SongNameGenerator _instance;
    public static SongNameGenerator Instance
    {
        get
        {
            //If _instance is null then we find it from the scene 
            if (_instance == null)
                _instance = GameObject.FindObjectOfType<SongNameGenerator>();
            return _instance;
        }
    }

    [SerializeField]
    private List<string> m_Determiners = new List<string>(); //Determiners Introduce the Noun
    [SerializeField]
    private List<string> m_Adjectives = new List<string>();
    [SerializeField]
    private List<string> m_Nouns = new List<string>();
    [SerializeField]
    private List<string> m_Subjects = new List<string>();
    [SerializeField]
    private List<string> m_PossessivePronouns = new List<string>();
    [SerializeField]
    private List<string> m_Prepositions = new List<string>();
    [SerializeField]
    private List<string> m_SubordinatingConjunctions = new List<string>();
    [SerializeField]
    private List<string> m_TransitiveVerbs = new List<string>(); //can attach directly to a noun
    [SerializeField]
    private List<string> m_TPPIntransitiveVerbs = new List<string>(); //cannot attach directly to a noun - conjugated for third person present
    [SerializeField]
    private List<string> m_InfIntransitiveVerbs = new List<string>(); //cannot attach directly to a noun - infinitive for following a helping verb
    [SerializeField]
    private List<string> m_HelpingVerbs = new List<string>();
    [SerializeField]
    private List<string> m_OneWordTitle = new List<string>();



    //List for all the song titles
    [SerializeField]
    private List<string> m_SongTitlesGenerated = new List<string>();

    private void Start()
    {
        //This export is just a temporary tool which lets us see batches of names and correct problems
        //GenerateSongTitles();
        //ExportTitles();
    }

    void GenerateSongTitles()
    {
        for (int i = 0; i < 1000; i++)
        {
            string nextSongTitle = GetRandomSongTitle();
            m_SongTitlesGenerated.Add(nextSongTitle);
        }
    }

    public string GetRandomSongTitle()
    {
        string songTitleGenerated = "";
        int songTitleType = Random.Range(0, 9);
        switch (songTitleType)
        {
            case 0:
                songTitleGenerated = Determiner_Adjective_Noun();
                break;
            case 1:
                songTitleGenerated = OneWord();
                break;
            case 2:
                songTitleGenerated = PossessivePronoun_Adjective_Noun();
                break;
            case 3:
                songTitleGenerated = Adjective_Noun();
                break;
            case 4:
                songTitleGenerated = TransitiveVerb_Determiner_Noun();
                break;
            case 5:
                songTitleGenerated = Preposition_PossessivePronoun_Noun();
                break;
            case 6:
                songTitleGenerated = Determiner_Noun();
                break;
            case 7:
                songTitleGenerated = SubordinatingConjunction_Determiner_Noun_TPPIntransitiveVerb();
                break;
            case 8:
                songTitleGenerated = HelpingVerb_Subject_InfIntransitiveVerb();
                break;

        }
        return songTitleGenerated;



    }


    string Determiner_Noun()
    {
        string determiner = m_Determiners[Random.Range(0, m_Determiners.Count)];
        string noun = m_Nouns[Random.Range(0, m_Nouns.Count)];
        return DeterminerAdjustment(determiner,noun) + " " + noun;
    }

    string Determiner_Adjective_Noun()
    {
        string determiner = m_Determiners[Random.Range(0, m_Determiners.Count)];
        string adjective = m_Adjectives[Random.Range(0, m_Adjectives.Count)];
        string noun = m_Nouns[Random.Range(0, m_Nouns.Count)];
        return DeterminerAdjustment(determiner,adjective) + " " + adjective + " " + noun;
    }

    string PossessivePronoun_Adjective_Noun()
    {
        string personalPronoun = m_PossessivePronouns[Random.Range(0, m_PossessivePronouns.Count)];
        string adjective = m_Adjectives[Random.Range(0, m_Adjectives.Count)];
        string noun = m_Nouns[Random.Range(0, m_Nouns.Count)];
        return personalPronoun + " " + adjective + " " + noun;
    }

    string Adjective_Noun()
    {
        string adjective = m_Adjectives[Random.Range(0, m_Adjectives.Count)];
        string noun = m_Nouns[Random.Range(0, m_Nouns.Count)];
        return adjective + " " + noun;
    }

    string Preposition_PossessivePronoun_Noun()
    {
        string preposition = m_Prepositions[Random.Range(0, m_Prepositions.Count)];

        string personalPronoun = m_PossessivePronouns[Random.Range(0, m_PossessivePronouns.Count)];
        string noun = m_Nouns[Random.Range(0, m_Nouns.Count)];
        return preposition + " " + personalPronoun + " " + noun;
    }

    string TransitiveVerb_Determiner_Noun()
    {
        string transitiveVerb = m_TransitiveVerbs[Random.Range(0, m_TransitiveVerbs.Count)];
		string determiner = m_Determiners[Random.Range(0, m_Determiners.Count)];
        string noun = m_Nouns[Random.Range(0, m_Nouns.Count)];
        return transitiveVerb + " " + DeterminerAdjustment(determiner,noun) + " " + noun;
    }

    string SubordinatingConjunction_Determiner_Noun_TPPIntransitiveVerb()
    {
        string subordinatingConjunction = m_SubordinatingConjunctions[Random.Range(0, m_SubordinatingConjunctions.Count)];
        string determiner = m_Determiners[Random.Range(0, m_Determiners.Count)];
        string noun = m_Nouns[Random.Range(0, m_Nouns.Count)];
        string tppIntransitiveVerb = m_TPPIntransitiveVerbs[Random.Range(0, m_TPPIntransitiveVerbs.Count)];
		return subordinatingConjunction + " " + DeterminerAdjustment(determiner,noun) + " " + noun + " " + tppIntransitiveVerb;
    }

    string HelpingVerb_Subject_InfIntransitiveVerb()
    {
        string helpingVerb = m_HelpingVerbs[Random.Range(0, m_HelpingVerbs.Count)];
        string subject = m_Subjects[Random.Range(0, m_Subjects.Count)];
        string infIntransitiveVerb = m_InfIntransitiveVerbs[Random.Range(0, m_InfIntransitiveVerbs.Count)];
		return helpingVerb + " " + subject + " " + infIntransitiveVerb + "?";
    }

    string OneWord()
    {
        string singleWord = m_OneWordTitle[Random.Range(0, m_OneWordTitle.Count)];
        return singleWord;
    }

    /// <summary>
    /// This is used to swap the determiner based upon the noun that follows it, Eg: An Cat,  should be A Cat
    /// </summary>
    string DeterminerAdjustment(string determiner, string followingWord)
    {
        char first = followingWord[0];
        bool isVowel = IsVowel(first);


        if (determiner == "A")
        {
            if (isVowel)
            {
                return "An";
            }
        }
        else if (determiner == "An")
        {
            if (!isVowel)
            {
                return "A";
            }
        }

        //if it doesnt match any above rules, return origina;l
        return determiner;

    }

    bool IsVowel(char c)
    {
        bool isVowel = "aeiouAEIOU".IndexOf(c) >= 0;
        return isVowel;
    }

    public void ExportTitles()
    {

        string filePath = Application.dataPath + "/Exports/" + "GeneratedSongTitle.csv";

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        //This is the writer, it writes to the filepath
        StreamWriter writer = new StreamWriter(filePath);

        //This is writing the line of the type, name, damage... etc... (I set these)
        writer.WriteLine("SongTitle");
        //This loops through everything in the inventory and sets the file to these.
        for (int i = 0; i < m_SongTitlesGenerated.Count; ++i)
        {
            writer.WriteLine(m_SongTitlesGenerated[i]);
        }
        writer.Flush();
        //This closes the file
        writer.Close();

        Debug.Log("File Generated Successfully: " + filePath);
    }


}
