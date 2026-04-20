using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using Spine.Unity;

public enum EventType
{
    UnityEvent,
    PlayAnimation,
    PlaySpineAnimation,
    EnableObject,
    DisableObject,
    Delay
}

public enum ConditionType
{
    None,
    Custom
}

[Serializable]
public class SequenceEvent
{
    public string eventName;
    public EventType eventType;

    public GameObject target;

    public UnityEvent unityEvent;

    public string animationName;
    public float delay;

    // Clean condition
    public ConditionType conditionType;
    public bool conditionValue;
}

public class EventSequencePlayer : MonoBehaviour
{
    public List<SequenceEvent> events = new List<SequenceEvent>();

    public bool playOnStart = false;
    public bool loop = false;

    public UnityEvent onComplete;

    private Coroutine runningCoroutine;

    private void Start()
    {
        if (playOnStart)
            Play();
    }

    public void Play()
    {
        if (runningCoroutine != null)
            StopCoroutine(runningCoroutine);

        runningCoroutine = StartCoroutine(RunSequence());
    }

    public void Stop()
    {
        if (runningCoroutine != null)
            StopCoroutine(runningCoroutine);
    }

    public void PlayEventByName(string eventName)
    {
        var e = events.Find(x => x.eventName == eventName);

        if (e == null)
        {
            Debug.LogWarning($"Event not found: {eventName}");
            return;
        }

        if (!CheckCondition(e))
            return;

        if (runningCoroutine != null)
            StopCoroutine(runningCoroutine);

        runningCoroutine = StartCoroutine(ExecuteEvent(e));
    }

    private IEnumerator RunSequence()
    {
        do
        {
            foreach (var e in events)
            {
                if (!CheckCondition(e))
                    continue;

                yield return ExecuteEvent(e);
            }

            onComplete?.Invoke();

        } while (loop);
    }

    private IEnumerator ExecuteEvent(SequenceEvent e)
    {
        switch (e.eventType)
        {
            case EventType.UnityEvent:
                e.unityEvent?.Invoke();
                break;

            case EventType.PlayAnimation:
                if (e.target != null)
                {
                    Animator anim = e.target.GetComponent<Animator>();
                    if (anim != null)
                        anim.Play(e.animationName);
                }
                break;

            case EventType.PlaySpineAnimation:
                if (e.target != null)
                {
                    SkeletonAnimation spine = e.target.GetComponent<SkeletonAnimation>();
                    if (spine != null)
                        spine.AnimationState.SetAnimation(0, e.animationName, false);
                }
                break;

            case EventType.EnableObject:
                if (e.target != null)
                    e.target.SetActive(true);
                break;

            case EventType.DisableObject:
                if (e.target != null)
                    e.target.SetActive(false);
                break;

            case EventType.Delay:
                yield return new WaitForSeconds(e.delay);
                break;
        }

        yield return null;
    }

    private bool CheckCondition(SequenceEvent e)
    {
        switch (e.conditionType)
        {
            case ConditionType.None:
                return true;

            case ConditionType.Custom:
                return e.conditionValue;

            default:
                return true;
        }
    }
}