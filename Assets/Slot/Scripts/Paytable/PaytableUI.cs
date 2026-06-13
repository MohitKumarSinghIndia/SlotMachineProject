using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SlotMachine.Reels.Data;
using SlotMachine.Reels.Runtime;

public class PaytableUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BetManager betManager;
    [SerializeField] private SymbolPoolManager symbolPoolManager;

    [Header("Paytable")]
    [SerializeField] private GameObject paytableUI;

    [Header("Pages")]
    [SerializeField] private GameObject page1;
    [SerializeField] private GameObject page2;

    [Header("Buttons")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button backButton;

    [Header("Paytable Texts")]
    [SerializeField] private List<TMP_Text> symbolsValueText;

    private float currentBet;

    private void OnEnable()
    {
        nextButton.onClick.AddListener(OpenNextPage);
        backButton.onClick.AddListener(OpenPreviousPage);
    }

    private void OnDisable()
    {
        nextButton.onClick.RemoveListener(OpenNextPage);
        backButton.onClick.RemoveListener(OpenPreviousPage);
    }

    private void GetCurrentBet()
    {
        currentBet = betManager.BetPerLine;
    }

    private void SetSymbolsPay()
    {
        GetCurrentBet();

        int textIndex = 0;

        // Symbol IDs 2-12 = HV1-LV5
        for (int symbolId = 2; symbolId <= 12; symbolId++)
        {
            SymbolDefinition definition = symbolPoolManager.GetDefinition(symbolId);

            if (definition == null)
                continue;

            UpdatePaytableText(symbolsValueText[textIndex], definition);

            textIndex ++;
        }
    }

    private void UpdatePaytableText(TMP_Text text, SymbolDefinition definition)
    {
        float win1 = definition.Paytable[0].Multiplier * currentBet;
        float win2 = definition.Paytable[1].Multiplier * currentBet;
        float win3 = definition.Paytable[2].Multiplier * currentBet;

        text.text = $"5 - {win3:0.00}\n4 - {win2:0.00}\n3 - {win1:0.00}";
    }

    #region UI

    public void ShowPaytable()
    {
        paytableUI.SetActive(true);
        ShowPage1();
        SetSymbolsPay();
    }

    public void ClosePaytable()
    {
        paytableUI.SetActive(false);
    }

    private void OpenNextPage()
    {
        ShowPage2();
    }

    private void OpenPreviousPage()
    {
        ShowPage1();
    }

    private void ShowPage1()
    {
        page1.SetActive(true);
        page2.SetActive(false);

        nextButton.gameObject.SetActive(true);
        backButton.gameObject.SetActive(false);
    }

    private void ShowPage2()
    {
        page1.SetActive(false);
        page2.SetActive(true);

        nextButton.gameObject.SetActive(false);
        backButton.gameObject.SetActive(true);
    }

    #endregion
}