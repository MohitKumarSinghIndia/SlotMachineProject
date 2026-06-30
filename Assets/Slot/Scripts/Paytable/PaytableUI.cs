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
    [SerializeField] private GameObject[] pages;

    private int currentPageIndex = 0;

    [Header("Buttons")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button previousButton;

    [Header("Paytable Texts")]
    [SerializeField] private List<TMP_Text> symbolsValueText;

    private float currentBet;

    private void OnEnable()
    {
        nextButton.onClick.AddListener(OpenNextPage);
        previousButton.onClick.AddListener(OpenPreviousPage);
    }

    private void OnDisable()
    {
        nextButton.onClick.RemoveListener(OpenNextPage);
        previousButton.onClick.RemoveListener(OpenPreviousPage);
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

        if (pages == null || pages.Length == 0)
            return;

        currentPageIndex = 0;
        ShowPage(currentPageIndex);
        SetSymbolsPay();
    }

    public void ClosePaytable()
    {
        paytableUI.SetActive(false);
    }

    private void OpenNextPage()
    {
        if (currentPageIndex < pages.Length - 1)
        {
            SoundController.Instance.PlaySound(SoundType.ButtonClick);
            currentPageIndex++;
            ShowPage(currentPageIndex);
        }
    }

    private void OpenPreviousPage()
    {
        if (currentPageIndex > 0)
        {
            SoundController.Instance.PlaySound(SoundType.ButtonClick);
            currentPageIndex--;
            ShowPage(currentPageIndex);
        }
    }

    private void ShowPage(int index)
    {
        if (pages == null || pages.Length == 0)
            return;

        for (int i = 0; i < pages.Length; i++)
        {
            if (pages[i] != null)
                pages[i].SetActive(i == index);
        }

        previousButton.interactable = index > 0;
        nextButton.interactable = index < pages.Length - 1;
    }

    #endregion
}