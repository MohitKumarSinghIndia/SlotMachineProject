using System.Collections;
using UnityEngine;
using TMPro;

public class BigWinController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject content;
    [SerializeField] private TMP_Text valueText;
    [SerializeField] private float currentValue;

    [Header("Settings")]
    [SerializeField] private float countSpeed = 2000f;
    [SerializeField] private float hideDelay = 2.5f;

    private int targetValue;

    private Coroutine countRoutine;

    private bool isRunning = false;
    private bool isStopped = false;

    public void ShowBigWin(int value)
    {
        targetValue = value;
        currentValue = 0;

        content.SetActive(true);

        if (countRoutine != null)
            StopCoroutine(countRoutine);

        countRoutine = StartCoroutine(CountRoutine());
    }

    private IEnumerator CountRoutine()
    {
        isRunning = true;
        isStopped = false;

        while (!isStopped)
        {
            currentValue += countSpeed * Time.deltaTime;

            if (currentValue >= targetValue)
            {
                currentValue = targetValue;
            }

            // valueText.text = ((int)currentValue).ToString();

            yield return null;
        }

        isRunning = false;
    }
    private void OnMouseDown()
    {
        if (isRunning)
        {
            HideBigWin();
        }
    }

    public void HideBigWin()
    {
        isStopped = true;

        if (countRoutine != null)
            StopCoroutine(countRoutine);

        currentValue = targetValue;
        // valueText.text = targetValue.ToString();

        StartCoroutine(HideRoutine());
    }

    private IEnumerator HideRoutine()
    {
        yield return new WaitForSeconds(hideDelay);

        content.SetActive(false);

        ResetState();
    }

    private void ResetState()
    {
        // valueText.text = "0";
        currentValue = 0;
        isRunning = false;
        isStopped = false;
    }
}