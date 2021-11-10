using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PersonPanel : MonoBehaviour
{
    public InputField m_quantityInputField;

    public GameObject m_DataRowContainer;

    CanvasTools ct;
    void Awake()
    {
        ct = GetComponent<CanvasTools>();
    }

    /// <summary>
    /// Calculate Listens buton in the UI
    /// </summary>
    public void CalculateListensClicked()
    {
        DataSimulationManager dsm = DataSimulationManager.Instance; //dsm acronym - sorry name was just so long!
        dsm.EvaluateListensDataUpToTurn();

    }

    /// <summary>
    /// Export Button in the UI
    /// </summary>
    public void ExportClicked()
    {
        DataSimulationManager dsm = DataSimulationManager.Instance; //dsm acronym - sorry name was just so long!
        dsm.ExportPersonData(dsm.m_PersonsGenerated, "Generated_Persons_Raw");
        dsm.EvaluatePersonDataAtTurn(dsm.m_CurrentTurn);

        dsm.ExportPersonData(dsm.m_PersonsGeneratedCurrentTurn, "Generated_Persons_Turn_" + dsm.m_CurrentTurn);

        DataSimulationManager.Instance.EvaluateLocationAveragesAtTurn();
        dsm.ExportPersonData(dsm.m_PersonAvgsByLocationCurrentTurn, "Generated_PersonsAverages_Turn_" + dsm.m_CurrentTurn);
        MessagePanel.Instance.CallOpen("Export CSV function is complete. See Asset/Exports/*your files*  to see your raw data and turn-based data.");
    }

    /// <summary>
    /// Export Listens Button in the UI
    /// </summary>
    public void ExportListenDataClicked()
    {
        DataSimulationManager dsm = DataSimulationManager.Instance; //dsm acronym - sorry name was just so long!
        dsm.ExportListenData(dsm.m_ListensAllTurns,"Generated_ListenData");
        MessagePanel.Instance.CallOpen("Export CSV function is complete. See Asset/Exports/*your files*  to see your listen data.");

    }

    /// <summary>
    /// Advance 1 turn
    /// </summary>
    [SerializeField]
    private Text m_IncrementTurnButtonText;
    [SerializeField]
    private Text m_TempSurgeDisplayText;
    public void IncrementTurnClicked(int i)
    {
        m_IncrementTurnButtonText.text = "Turn: " + DataSimulationManager.Instance.IncrementTurn(i);
    }

    /// <summary>
    /// Generate Button in the UI
    /// </summary>
    public void GenerateClicked()
    {
        DataSimulationManager.Instance.GeneratePersonsData();
        DataSimulationManager.Instance.EvaluateLocationAveragesAtTurn();
    }

    /// <summary>
    /// Get the quantity from the input text field
    /// </summary>
    public int GetQuantity()
    {
        int qty = 0;
        int.TryParse(m_quantityInputField.text, out qty);
        return qty;
    }

    /// <summary>
    /// Toggle the CanvasGroup alpha - AKA - Show Hide Menu
    /// </summary>
    /// <param name="b"></param>
    public void ToggleMe(bool b)
    {
        ct.ToggleMe(b);
    }

}
