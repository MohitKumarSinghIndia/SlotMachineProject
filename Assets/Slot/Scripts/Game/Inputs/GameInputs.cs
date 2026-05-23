using SlotMachine.Game.Runtime;
using SlotMachine.Reels.Runtime;
using UnityEngine;

public class GameInputs : MonoBehaviour
{

    [SerializeField] private ReelManager reelManager;
    [SerializeField] private FreeSpinsPresenter freeSpinsPresenter;
    [SerializeField] private FreeSpinManager freeSpinManager;
    [SerializeField] private BannerManager bannerManager;

    public bool isFreeSpinStarted;
    public bool isFreeSpinEnded;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (reelManager != null || freeSpinsPresenter != null || freeSpinManager != null)
            {
                if (bannerManager.isIntroBannerActive)
                {
                    bannerManager.OnIntroBannerClick();
                    return;
                }

                if (isFreeSpinStarted)
                {
                    freeSpinsPresenter?.OnClickStartBanner();
                    isFreeSpinStarted = false;
                    return;
                }

                if (isFreeSpinEnded)
                {
                    freeSpinsPresenter?.OnClickEndBanner();
                    isFreeSpinEnded = false;
                    return;
                }

                if (!reelManager.IsSpinInProgress || !freeSpinManager.IsFreeSpinActive)
                {
                    reelManager.StartSpin();
                    return;
                }
            }
        }
    }

    public void SetFreeSpinStarted(bool value)
    {
        isFreeSpinStarted = value;
    }

    public void SetFreeSpinEnded(bool value)
    {
        isFreeSpinEnded = value;
    }
}
