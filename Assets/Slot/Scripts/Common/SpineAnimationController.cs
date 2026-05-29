using Spine.Unity;
using Spine;
using UnityEngine;

public class SpineAnimationController : MonoBehaviour
{
    public enum EnableBehaviour
    {
        None,
        Reset,
        Play
    }

    [Header("Reference")]
    [SerializeField] private SkeletonAnimation skeletonGraphic;

    [Header("Animation")]
    [SpineAnimation(dataField: nameof(skeletonGraphic))]
    [SerializeField] private string animationName;

    [SerializeField] private bool loop = true;
    [SerializeField, Min(0f)] private float playbackSpeed = 1f;

    [SerializeField, Tooltip("-1 keeps the source duration. Positive values rescale the animation to fit this duration.")]
    private float customDuration = -1f;

    [Header("On Enable")]
    [SerializeField] private EnableBehaviour onEnableBehaviour = EnableBehaviour.Play;

    public string AnimationName => animationName;

    private void Reset()
    {
        skeletonGraphic = GetComponent<SkeletonAnimation>();
    }

    private void OnEnable()
    {
        switch (onEnableBehaviour)
        {
            case EnableBehaviour.Reset:
                ResetAnimation();
                break;

            case EnableBehaviour.Play:
                PlayAnimation();
                break;
        }
    }

    public void PlayAnimation()
    {
        if (skeletonGraphic == null || string.IsNullOrWhiteSpace(animationName))
            return;

        var state = skeletonGraphic.AnimationState;
        var skeletonData = skeletonGraphic.Skeleton.Data;
        var animation = skeletonData.FindAnimation(animationName);

        if (animation == null)
        {
            Debug.LogWarning($"Spine animation '{animationName}' was not found on {name}.", this);
            return;
        }

        TrackEntry entry = state.SetAnimation(0, animationName, loop);
        entry.TimeScale = ResolveTimeScale(animation);

    }

    public void Play(string targetAnimation, bool shouldLoop = false, float speed = 1f, float targetDuration = -1f)
    {
        animationName = targetAnimation;
        loop = shouldLoop;
        playbackSpeed = Mathf.Max(0f, speed);
        customDuration = targetDuration;

        PlayAnimation();
    }

    public void ResetAnimation()
    {
        if (skeletonGraphic == null)
            return;

        skeletonGraphic.AnimationState.ClearTracks();
        skeletonGraphic.Skeleton.SetToSetupPose();
        skeletonGraphic.Update(0f);
    }

    private float ResolveTimeScale(Spine.Animation animation)
    {
        if (customDuration > 0f && animation.Duration > 0f)
            return animation.Duration / customDuration;

        return Mathf.Max(0.01f, playbackSpeed);
    }
}
