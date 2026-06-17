using UnityEngine;

namespace SlotMachine.Reels.Runtime
{
    public class BannerManager : MonoBehaviour
    {
        [SerializeField] private GameObject introBanner;
        [SerializeField] private GameObject gameScreen;
 
        public bool isIntroBannerActive = true;

        public void OnIntroBannerClick()
        {
                introBanner.SetActive(false);
                isIntroBannerActive = false;
                gameScreen.SetActive(true);
                SoundController.Instance.PlayBaseGameMusic();
        }

        public void CrossButtonClicked()
        {
            introBanner.SetActive(true);
            isIntroBannerActive = true;
            gameScreen.SetActive(false);
            SoundController.Instance.PlayIntroMusic();
        }

    }
}