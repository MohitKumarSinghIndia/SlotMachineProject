using UnityEngine;
using UnityEngine.UI;

namespace SlotMachine.Reels.Runtime
{
    public class BannerManager : MonoBehaviour
    {
        [SerializeField] private GameObject introBanner;

        public bool isIntroBannerActive = true;

        public void OnIntroBannerClick()
        {
            introBanner.SetActive(false);
            isIntroBannerActive = false;
            SoundController.Instance.PlayBaseGameMusic();
        }

    }
}