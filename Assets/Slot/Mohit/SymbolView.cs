using TMPro;
using UnityEngine;

public class SymbolView : MonoBehaviour
{
    [SerializeField]
    private SymbolType symbolType;

    public SymbolType SymbolType => symbolType;

    private RectTransform rectTransform;
    private TMP_Text symbolName;

    private void Start()
    {
        symbolName = GetComponentInChildren<TMP_Text>();

        symbolName.text = symbolType.ToString();
    }
    public RectTransform RectTransform
    {
        get
        {
            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }

            return rectTransform;
        }
    }

    public void ResetState()
    {
        RectTransform.localScale = Vector3.one;

        RectTransform.localRotation = Quaternion.identity;

        RectTransform.anchoredPosition = Vector2.zero;

        gameObject.SetActive(false);
    }
}