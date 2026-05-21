using UnityEngine;
using UnityEngine.UI;

public class BannerManager : MonoBehaviour
{
    [SerializeField] private GameObject introBanner;
    [SerializeField] private Button introBannerButton;
    public bool isIntroBannerClick;

    private void OnEnable()
    {
        if (introBannerButton != null)
        {
            introBannerButton.onClick.AddListener(OnIntroBannerClick);
        }
    }

    private void OnDisable()
    {
        if (introBannerButton != null)
        {
            introBannerButton.onClick.RemoveListener(OnIntroBannerClick);
        }
    }

    public void OnIntroBannerClick()
    {
        if (isIntroBannerClick)
        {
            return;
        }

        isIntroBannerClick = true;             

        if (introBanner != null)
        {
            introBanner.SetActive(false);
        }
    }

}