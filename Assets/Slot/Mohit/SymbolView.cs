using TMPro;
using UnityEngine;

public class SymbolView : MonoBehaviour
{
    [SerializeField]
    private SymbolType symbolType;

    public SymbolType SymbolType => symbolType;

    private Transform cachedTransform;

    public Transform CachedTransform
    {
        get
        {
            if (cachedTransform == null)
            {
                cachedTransform = transform;
            }

            return cachedTransform;
        }
    }

    public void ResetState()
    {
        CachedTransform.localScale = Vector3.one;

        CachedTransform.localRotation = Quaternion.identity;

        CachedTransform.localPosition = Vector3.zero;

        gameObject.SetActive(false);
    }
}