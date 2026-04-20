using UnityEngine;
using Spine;
using Spine.Unity;

public class SpineAnimationController : MonoBehaviour
{
    public enum OnEnableAction
    {
        DoNothing,
        Reset,
        Play
    }

    [Header("Reference")]
    public SkeletonAnimation skeletonAnimation;

    [Header("Animation")]
    [SpineAnimation(dataField: "skeletonAnimation")]
    public string animationName;

    public bool loop = true;

    [Tooltip("-1 = original duration, otherwise force animation to fit given time")]
    public float customDuration = -1f;

    [Header("On Enable")]
    public OnEnableAction onEnableAction = OnEnableAction.Play;

    private void Reset()
    {
        skeletonAnimation = GetComponent<SkeletonAnimation>();
    }

    private void OnEnable()
    {
        switch (onEnableAction)
        {
            case OnEnableAction.DoNothing:
                break;

            case OnEnableAction.Reset:
                ResetAnimation();
                break;

            case OnEnableAction.Play:
                PlayAnimation();
                break;
        }
    }

    // 🔥 MAIN PLAY FUNCTION
    public void PlayAnimation()
    {
        if (skeletonAnimation == null || string.IsNullOrEmpty(animationName))
            return;

        var state = skeletonAnimation.AnimationState;
        var skeletonData = skeletonAnimation.Skeleton.Data;

        var anim = skeletonData.FindAnimation(animationName);
        if (anim == null)
        {
            Debug.LogError($"Animation {animationName} not found!");
            return;
        }

        TrackEntry entry = state.SetAnimation(0, animationName, loop);

        // 🔥 Apply custom timing
        if (customDuration > 0)
        {
            float originalDuration = anim.Duration;

            if (originalDuration > 0)
            {
                entry.TimeScale = originalDuration / customDuration;
            }
        }
        else
        {
            entry.TimeScale = 1f; // normal speed
        }
    }

    // 🔄 RESET
    public void ResetAnimation()
    {
        if (skeletonAnimation == null)
            return;

        skeletonAnimation.AnimationState.ClearTracks();
        skeletonAnimation.Skeleton.SetToSetupPose();
        skeletonAnimation.Update(0);
    }

    // 🎮 Manual Controls (for code use)
    public void Play(string anim, bool loopAnim = false, float duration = -1f)
    {
        animationName = anim;
        loop = loopAnim;
        customDuration = duration;

        PlayAnimation();
    }
}