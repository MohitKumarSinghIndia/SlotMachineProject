using UnityEngine;
using UnityEngine.UI;

public class PaytableUI : MonoBehaviour
{
    [Header("Paytable")]
    [SerializeField] private GameObject paytableUI;

    [Header("Pages")]
    [SerializeField] private GameObject page1;
    [SerializeField] private GameObject page2;

    [Header("Buttons")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button backButton;
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

    public void ShowPaytable()
    {
        paytableUI.SetActive(true);
        ShowPage1();
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
}