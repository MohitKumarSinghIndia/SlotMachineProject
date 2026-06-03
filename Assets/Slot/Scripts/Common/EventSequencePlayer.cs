using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using DG.Tweening;

public enum EventType
{
    UnityEvent,
    PlayAnimation,
    PlaySpineAnimation,
    ToggleObjects,
    PlayAudio,
    PlaySequence,
    ModifyTransform
}

public enum ConditionType
{
    None,
    Custom
}

[Serializable]
public class SequenceEvent
{
    [HideInInspector] public bool isExpanded = false;

    public string eventName = "New Event";
    public EventType eventType;
    public float eventDelay;
    public bool waitForCompletion;

    public GameObject target;
    public string animationName;
    public bool loop;

    [Header("Toggle Objects")]
    public List<GameObject> objectsToEnable = new List<GameObject>();
    public List<GameObject> objectsToDisable = new List<GameObject>();

    public AudioClip audioClip;
    public AudioSource audioSource;

    public EventSequencePlayer sequencePlayerTarget;
    public int targetSequenceId;

    public bool modifyPosition;
    public Vector3 targetPosition;

    public bool modifyRotation;
    public Vector3 targetRotation;

    public bool modifyScale;
    public Vector3 targetScale = Vector3.one;

    public float tweenDuration;
    public Ease easeType = Ease.OutQuad;

    public UnityEvent unityEvent;

    public ConditionType conditionType;
    public bool conditionValue;
}

[Serializable]
public class EventSequence
{
    public string sequenceName;
    public int sequenceId;
    public List<SequenceEvent> events = new List<SequenceEvent>();
}

public class EventSequencePlayer : MonoBehaviour
{
    public List<EventSequence> sequences = new List<EventSequence>();

    private readonly List<Coroutine> runningCoroutines = new List<Coroutine>();

    public bool IsPlaying => runningCoroutines.Count > 0;

    public void Play()
    {
        if (sequences.Count == 0)
            return;

        PlaySequence(sequences[0]);
    }

    public void Stop()
    {
        for (int i = 0; i < runningCoroutines.Count; i++)
        {
            if (runningCoroutines[i] != null)
            {
                StopCoroutine(runningCoroutines[i]);
            }
        }

        runningCoroutines.Clear();
    }

    public void PlaySequence(string sequenceName)
    {
        EventSequence sequence = sequences.Find(x => x.sequenceName == sequenceName);

        if (sequence != null)
        {
            PlaySequence(sequence);
        }
    }

    public void PlaySequenceById(int id)
    {
        PlaySequenceByIdRoutine(id);
    }

    public Coroutine PlaySequenceByIdRoutine(int id)
    {
        EventSequence sequence = sequences.Find(x => x.sequenceId == id);

        if (sequence == null)
        {
            return null;
        }

        return PlaySequence(sequence);
    }

    private Coroutine PlaySequence(EventSequence sequence)
    {
        if (!gameObject.activeInHierarchy || sequence == null)
        {
            return null;
        }

        Coroutine coroutine = null;

        IEnumerator Wrapper()
        {
            yield return RunSequence(sequence);

            if (coroutine != null)
            {
                runningCoroutines.Remove(coroutine);
            }
        }

        coroutine = StartCoroutine(Wrapper());
        runningCoroutines.Add(coroutine);

        return coroutine;
    }

    private IEnumerator RunSequence(EventSequence sequence)
    {
        foreach (SequenceEvent e in sequence.events)
        {
            if (!CheckCondition(e))
            {
                continue;
            }

            yield return ExecuteEvent(e);
        }
    }

    private IEnumerator ExecuteEvent(SequenceEvent e)
    {
        if (e.eventDelay > 0f)
        {
            yield return new WaitForSeconds(e.eventDelay);
        }

        Coroutine waitCoroutine = null;

        switch (e.eventType)
        {
            case EventType.UnityEvent:
                e.unityEvent?.Invoke();
                break;

            case EventType.PlayAnimation:
                waitCoroutine = StartCoroutine(PlayAnimator(e));
                break;

            case EventType.PlaySpineAnimation:
                waitCoroutine = StartCoroutine(PlaySpine(e));
                break;

            case EventType.ToggleObjects:
                ToggleObjects(e);
                break;

            case EventType.PlayAudio:
                waitCoroutine = StartCoroutine(PlayAudio(e));
                break;

            case EventType.PlaySequence:
                waitCoroutine = StartCoroutine(PlayExternalSequence(e));
                break;

            case EventType.ModifyTransform:
                waitCoroutine = StartCoroutine(ModifyTransformCoroutine(e));
                break;
        }

        if (e.waitForCompletion && waitCoroutine != null)
        {
            yield return waitCoroutine;
        }
    }

    private void ToggleObjects(SequenceEvent e)
    {
        foreach (GameObject obj in e.objectsToEnable)
        {
            if (obj != null)
            {
                obj.SetActive(true);
            }
        }

        foreach (GameObject obj in e.objectsToDisable)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }
    }

    private IEnumerator ModifyTransformCoroutine(SequenceEvent e)
    {
        if (e.target == null)
        {
            yield break;
        }

        Transform t = e.target.transform;

        if (e.tweenDuration > 0f)
        {
            Sequence seq = DOTween.Sequence();

            if (e.modifyPosition)
            {
                seq.Join(t.DOLocalMove(e.targetPosition, e.tweenDuration).SetEase(e.easeType));
            }

            if (e.modifyRotation)
            {
                seq.Join(t.DOLocalRotate(e.targetRotation, e.tweenDuration).SetEase(e.easeType));
            }

            if (e.modifyScale)
            {
                seq.Join(t.DOScale(e.targetScale, e.tweenDuration).SetEase(e.easeType));
            }

            yield return seq.WaitForCompletion();
        }
        else
        {
            if (e.modifyPosition)
            {
                t.localPosition = e.targetPosition;
            }

            if (e.modifyRotation)
            {
                t.localEulerAngles = e.targetRotation;
            }

            if (e.modifyScale)
            {
                t.localScale = e.targetScale;
            }
        }
    }

    private IEnumerator PlayExternalSequence(SequenceEvent e)
    {
        if (e.sequencePlayerTarget == null)
        {
            yield break;
        }

        Coroutine coroutine = e.sequencePlayerTarget.PlaySequenceByIdRoutine(e.targetSequenceId);

        if (e.waitForCompletion && coroutine != null)
        {
            yield return coroutine;
        }
    }

    private IEnumerator PlayAnimator(SequenceEvent e)
    {
        if (e.target == null)
        {
            yield break;
        }

        Animator anim = e.target.GetComponent<Animator>();

        if (anim == null || string.IsNullOrEmpty(e.animationName))
        {
            yield break;
        }

        anim.Play(e.animationName);

        yield return null;

        if (e.waitForCompletion && !e.loop)
        {
            while (anim.GetCurrentAnimatorStateInfo(0).IsName(e.animationName) &&
                   anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
            {
                yield return null;
            }
        }
    }

    private IEnumerator PlaySpine(SequenceEvent e)
    {
        if (e.target == null)
        {
            yield break;
        }

        Spine.AnimationState animState = null;

        SkeletonAnimation meshSpine = e.target.GetComponent<SkeletonAnimation>();

        if (meshSpine != null)
        {
            animState = meshSpine.AnimationState;
        }
        else
        {
            SkeletonGraphic uiSpine = e.target.GetComponent<SkeletonGraphic>();

            if (uiSpine != null)
            {
                animState = uiSpine.AnimationState;
            }
        }

        if (animState == null || string.IsNullOrEmpty(e.animationName))
        {
            yield break;
        }

        Spine.TrackEntry track = animState.SetAnimation(0, e.animationName, e.loop);

        if (e.waitForCompletion && !e.loop && track != null)
        {
            yield return new WaitForSeconds(track.Animation.Duration);
        }
    }

    private IEnumerator PlayAudio(SequenceEvent e)
    {
        if (e.audioSource == null || e.audioClip == null)
        {
            yield break;
        }

        e.audioSource.clip = e.audioClip;
        e.audioSource.loop = e.loop;
        e.audioSource.Play();

        if (e.waitForCompletion && !e.loop)
        {
            yield return new WaitForSeconds(e.audioClip.length);
        }
    }

    private bool CheckCondition(SequenceEvent e)
    {
        if (e.conditionType == ConditionType.Custom)
        {
            return e.conditionValue;
        }

        return true;
    }
}