#if UNITY_EDITOR

using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.Collections.Generic;
using Spine.Unity;
using DG.Tweening;

[CustomEditor(typeof(EventSequencePlayer))]
public class EventSequencePlayerEditor : Editor
{
    private SerializedProperty sequencesProp;
    private readonly List<ReorderableList> eventLists = new List<ReorderableList>();

    private void OnEnable()
    {
        sequencesProp = serializedObject.FindProperty("sequences");
        SetupLists();
    }

    private void SetupLists()
    {
        eventLists.Clear();

        for (int i = 0; i < sequencesProp.arraySize; i++)
        {
            SerializedProperty sequence = sequencesProp.GetArrayElementAtIndex(i);
            SerializedProperty eventsProp = sequence.FindPropertyRelative("events");

            ReorderableList list = new ReorderableList(serializedObject, eventsProp, true, true, true, true);

            list.drawHeaderCallback = (Rect rect) =>
            {
                SerializedProperty nameProp = sequence.FindPropertyRelative("sequenceName");
                EditorGUI.LabelField(rect, $"Events : {nameProp.stringValue}", EditorStyles.boldLabel);
            };

            list.elementHeightCallback = (int index) =>
            {
                SerializedProperty element = eventsProp.GetArrayElementAtIndex(index);
                SerializedProperty isExpanded = element.FindPropertyRelative("isExpanded");

                float line = EditorGUIUtility.singleLineHeight;
                float space = EditorGUIUtility.standardVerticalSpacing;

                if (!isExpanded.boolValue)
                    return line + (space * 2);

                SerializedProperty type = element.FindPropertyRelative("eventType");
                SerializedProperty conditionType = element.FindPropertyRelative("conditionType");

                EventType eventType = (EventType)type.enumValueIndex;

                float height = line + space;
                height += (line + space) * 4;

                switch (eventType)
                {
                    case EventType.UnityEvent:
                        SerializedProperty unityEvent = element.FindPropertyRelative("unityEvent");
                        height += EditorGUI.GetPropertyHeight(unityEvent, true) + space;
                        height -= line + space;
                        break;

                    case EventType.PlayAnimation:
                    case EventType.PlaySpineAnimation:
                        height += (line + space) * 2;
                        break;

                    case EventType.ToggleObjects:
                        height += EditorGUI.GetPropertyHeight(element.FindPropertyRelative("objectsToEnable"), true) + space;
                        height += EditorGUI.GetPropertyHeight(element.FindPropertyRelative("objectsToDisable"), true) + space;
                        height -= line + space;
                        break;

                    case EventType.PlayAudio:
                        height += (line + space) * 3;
                        break;

                    case EventType.PlaySequence:
                        height += (line + space) * 3;
                        height -= line + space;
                        break;

                    case EventType.ModifyTransform:
                        height += (line + space) * 2;

                        height += line + space;
                        if (element.FindPropertyRelative("modifyPosition").boolValue)
                            height += line + space;

                        height += line + space;
                        if (element.FindPropertyRelative("modifyRotation").boolValue)
                            height += line + space;

                        height += line + space;
                        if (element.FindPropertyRelative("modifyScale").boolValue)
                            height += line + space;

                        height += line + space;
                        break;
                }

                height += line + space;

                if ((ConditionType)conditionType.enumValueIndex == ConditionType.Custom)
                    height += line + space;

                return height + 10;
            };

            list.drawElementCallback = (Rect rect, int index, bool active, bool focused) =>
            {
                SerializedProperty element = eventsProp.GetArrayElementAtIndex(index);
                DrawEvent(rect, element);
            };

            eventLists.Add(list);
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EventSequencePlayer player = (EventSequencePlayer)target;

        GUILayout.Space(5);

        EditorGUILayout.BeginHorizontal();

        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("▶ Play", GUILayout.Height(30)))
            player.Play();

        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("⏹ Stop", GUILayout.Height(30)))
            player.Stop();

        GUI.backgroundColor = Color.white;

        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        DrawSequences();

        serializedObject.ApplyModifiedProperties();

        if (GUI.changed)
            SetupLists();
    }

    private void DrawSequences()
    {
        for (int i = 0; i < sequencesProp.arraySize; i++)
        {
            SerializedProperty sequence = sequencesProp.GetArrayElementAtIndex(i);
            SerializedProperty nameProp = sequence.FindPropertyRelative("sequenceName");
            SerializedProperty idProp = sequence.FindPropertyRelative("sequenceId");

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField($"Sequence {i}", EditorStyles.boldLabel);

            GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);

            if (GUILayout.Button("Remove", GUILayout.Width(80)))
            {
                sequencesProp.DeleteArrayElementAtIndex(i);
                serializedObject.ApplyModifiedProperties();
                SetupLists();
                GUI.backgroundColor = Color.white;
                break;
            }

            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);

            EditorGUILayout.PropertyField(nameProp, new GUIContent("Sequence Name"));
            EditorGUILayout.PropertyField(idProp, new GUIContent("Sequence ID"));

            GUILayout.Space(5);

            if (i < eventLists.Count)
                eventLists[i].DoLayoutList();

            EditorGUILayout.EndVertical();

            GUILayout.Space(10);
        }

        GUI.backgroundColor = Color.green;

        if (GUILayout.Button("+ Add Sequence", GUILayout.Height(30)))
        {
            sequencesProp.arraySize++;
            serializedObject.ApplyModifiedProperties();
            SetupLists();
        }

        GUI.backgroundColor = Color.white;
    }

    private void DrawEvent(Rect rect, SerializedProperty element)
    {
        SerializedProperty isExpanded = element.FindPropertyRelative("isExpanded");
        SerializedProperty name = element.FindPropertyRelative("eventName");
        SerializedProperty type = element.FindPropertyRelative("eventType");

        EventType eventType = (EventType)type.enumValueIndex;

        float line = EditorGUIUtility.singleLineHeight;
        float space = EditorGUIUtility.standardVerticalSpacing;
        float y = rect.y + 2;

        DrawBackground(rect, eventType);

        Rect foldoutRect = new Rect(rect.x + 10, y, 20, line);
        isExpanded.boolValue = EditorGUI.Foldout(foldoutRect, isExpanded.boolValue, GUIContent.none);

        Rect iconRect = new Rect(rect.x + 25, y, 20, line);
        GUIContent iconContent = GetIconForEvent(eventType);

        if (iconContent != null)
            EditorGUI.LabelField(iconRect, iconContent);

        Rect headerLabelRect = new Rect(rect.x + 45, y, rect.width - 45, line);
        GUIStyle boldLabel = new GUIStyle(EditorStyles.label)
        {
            fontStyle = FontStyle.Bold
        };

        EditorGUI.LabelField(headerLabelRect, $"{name.stringValue} ({eventType})", boldLabel);

        y += line + space;

        if (!isExpanded.boolValue)
            return;

        y += space * 2;

        EditorGUI.PropertyField(new Rect(rect.x + 8, y, rect.width - 16, line), name);
        y += line + space;

        EditorGUI.PropertyField(new Rect(rect.x + 8, y, rect.width - 16, line), type);
        y += line + space;

        SerializedProperty eventDelay = element.FindPropertyRelative("eventDelay");
        EditorGUI.PropertyField(new Rect(rect.x + 8, y, rect.width - 16, line), eventDelay);
        y += line + space;

        if (eventType != EventType.UnityEvent &&
            eventType != EventType.PlaySequence &&
            eventType != EventType.ToggleObjects)
        {
            SerializedProperty target = element.FindPropertyRelative("target");
            EditorGUI.PropertyField(new Rect(rect.x + 8, y, rect.width - 16, line), target);
            y += line + space;
        }

        SerializedProperty animationName = element.FindPropertyRelative("animationName");
        SerializedProperty loop = element.FindPropertyRelative("loop");
        SerializedProperty waitForCompletion = element.FindPropertyRelative("waitForCompletion");

        switch (eventType)
        {
            case EventType.UnityEvent:
                SerializedProperty unityEvent = element.FindPropertyRelative("unityEvent");
                float unityHeight = EditorGUI.GetPropertyHeight(unityEvent, true);

                EditorGUI.PropertyField(
                    new Rect(rect.x + 8, y, rect.width - 16, unityHeight),
                    unityEvent,
                    true
                );

                y += unityHeight + space;
                break;

            case EventType.PlayAnimation:
                EditorGUI.PropertyField(new Rect(rect.x + 8, y, rect.width - 16, line), animationName);
                y += line + space;

                EditorGUI.PropertyField(new Rect(rect.x + 8, y, rect.width - 16, line), waitForCompletion);
                y += line + space;

                EditorGUI.PropertyField(new Rect(rect.x + 8, y, rect.width - 16, line), loop);
                y += line + space;
                break;

            case EventType.PlaySpineAnimation:
                SerializedProperty target = element.FindPropertyRelative("target");

                DrawSpineDropdown(rect, ref y, line, target, animationName);

                EditorGUI.PropertyField(new Rect(rect.x + 8, y, rect.width - 16, line), waitForCompletion);
                y += line + space;

                EditorGUI.PropertyField(new Rect(rect.x + 8, y, rect.width - 16, line), loop);
                y += line + space;
                break;

            case EventType.ToggleObjects:
                SerializedProperty objectsToEnable = element.FindPropertyRelative("objectsToEnable");
                SerializedProperty objectsToDisable = element.FindPropertyRelative("objectsToDisable");

                float enableHeight = EditorGUI.GetPropertyHeight(objectsToEnable, true);

                EditorGUI.PropertyField(
                    new Rect(rect.x + 8, y, rect.width - 16, enableHeight),
                    objectsToEnable,
                    new GUIContent("Objects To Enable"),
                    true
                );

                y += enableHeight + space;

                float disableHeight = EditorGUI.GetPropertyHeight(objectsToDisable, true);

                EditorGUI.PropertyField(
                    new Rect(rect.x + 8, y, rect.width - 16, disableHeight),
                    objectsToDisable,
                    new GUIContent("Objects To Disable"),
                    true
                );

                y += disableHeight + space;
                break;

            case EventType.PlayAudio:
                SerializedProperty audioSource = element.FindPropertyRelative("audioSource");
                SerializedProperty audioClip = element.FindPropertyRelative("audioClip");

                EditorGUI.PropertyField(new Rect(rect.x + 8, y, rect.width - 16, line), audioSource);
                y += line + space;

                EditorGUI.PropertyField(new Rect(rect.x + 8, y, rect.width - 16, line), audioClip);
                y += line + space;

                EditorGUI.PropertyField(new Rect(rect.x + 8, y, rect.width - 16, line), waitForCompletion);
                y += line + space;

                EditorGUI.PropertyField(new Rect(rect.x + 8, y, rect.width - 16, line), loop);
                y += line + space;
                break;

            case EventType.PlaySequence:
                SerializedProperty sequencePlayerTarget = element.FindPropertyRelative("sequencePlayerTarget");
                SerializedProperty targetSequenceId = element.FindPropertyRelative("targetSequenceId");

                EditorGUI.PropertyField(
                    new Rect(rect.x + 8, y, rect.width - 16, line),
                    sequencePlayerTarget,
                    new GUIContent("Target Player")
                );

                y += line + space;

                EditorGUI.PropertyField(
                    new Rect(rect.x + 8, y, rect.width - 16, line),
                    targetSequenceId,
                    new GUIContent("Sequence ID")
                );

                y += line + space;

                EditorGUI.PropertyField(new Rect(rect.x + 8, y, rect.width - 16, line), waitForCompletion);
                y += line + space;
                break;

            case EventType.ModifyTransform:
                SerializedProperty modifyPosition = element.FindPropertyRelative("modifyPosition");
                SerializedProperty targetPosition = element.FindPropertyRelative("targetPosition");

                SerializedProperty modifyRotation = element.FindPropertyRelative("modifyRotation");
                SerializedProperty targetRotation = element.FindPropertyRelative("targetRotation");

                SerializedProperty modifyScale = element.FindPropertyRelative("modifyScale");
                SerializedProperty targetScale = element.FindPropertyRelative("targetScale");

                SerializedProperty tweenDuration = element.FindPropertyRelative("tweenDuration");
                SerializedProperty easeType = element.FindPropertyRelative("easeType");

                EditorGUI.PropertyField(new Rect(rect.x + 8, y, rect.width - 16, line), tweenDuration);
                y += line + space;

                EditorGUI.PropertyField(new Rect(rect.x + 8, y, rect.width - 16, line), easeType);
                y += line + space;

                EditorGUI.PropertyField(new Rect(rect.x + 8, y, rect.width - 16, line), modifyPosition);
                y += line + space;

                if (modifyPosition.boolValue)
                {
                    EditorGUI.PropertyField(new Rect(rect.x + 8, y, rect.width - 16, line), targetPosition);
                    y += line + space;
                }

                EditorGUI.PropertyField(new Rect(rect.x + 8, y, rect.width - 16, line), modifyRotation);
                y += line + space;

                if (modifyRotation.boolValue)
                {
                    EditorGUI.PropertyField(new Rect(rect.x + 8, y, rect.width - 16, line), targetRotation);
                    y += line + space;
                }

                EditorGUI.PropertyField(new Rect(rect.x + 8, y, rect.width - 16, line), modifyScale);
                y += line + space;

                if (modifyScale.boolValue)
                {
                    EditorGUI.PropertyField(new Rect(rect.x + 8, y, rect.width - 16, line), targetScale);
                    y += line + space;
                }

                EditorGUI.PropertyField(new Rect(rect.x + 8, y, rect.width - 16, line), waitForCompletion);
                y += line + space;
                break;
        }

        SerializedProperty conditionType = element.FindPropertyRelative("conditionType");

        EditorGUI.PropertyField(new Rect(rect.x + 8, y, rect.width - 16, line), conditionType);
        y += line + space;

        if ((ConditionType)conditionType.enumValueIndex == ConditionType.Custom)
        {
            SerializedProperty conditionValue = element.FindPropertyRelative("conditionValue");
            EditorGUI.PropertyField(new Rect(rect.x + 8, y, rect.width - 16, line), conditionValue);
        }
    }

    private void DrawSpineDropdown(
        Rect rect,
        ref float y,
        float line,
        SerializedProperty targetProp,
        SerializedProperty animProp
    )
    {
        GameObject target = targetProp.objectReferenceValue as GameObject;

        if (target == null)
        {
            EditorGUI.LabelField(new Rect(rect.x + 8, y, rect.width - 16, line), "Assign Spine Target");
            y += line + 6;
            return;
        }

        ISkeletonAnimation skeleton = target.GetComponent<SkeletonAnimation>();

        if (skeleton == null)
            skeleton = target.GetComponent<SkeletonGraphic>();

        if (skeleton == null || skeleton.Skeleton == null)
        {
            EditorGUI.LabelField(new Rect(rect.x + 8, y, rect.width - 16, line), "No Spine Data Found");
            y += line + 6;
            return;
        }

        List<string> names = new List<string>();

        foreach (var anim in skeleton.Skeleton.Data.Animations)
            names.Add(anim.Name);

        if (names.Count == 0)
            return;

        int currentIndex = names.IndexOf(animProp.stringValue);

        if (currentIndex < 0)
            currentIndex = 0;

        int newIndex = EditorGUI.Popup(
            new Rect(rect.x + 8, y, rect.width - 16, line),
            "Animation",
            currentIndex,
            names.ToArray()
        );

        animProp.stringValue = names[newIndex];

        y += line + 6;
    }

    private GUIContent GetIconForEvent(EventType type)
    {
        switch (type)
        {
            case EventType.UnityEvent:
                return EditorGUIUtility.IconContent("cs Script Icon");

            case EventType.PlayAnimation:
                return EditorGUIUtility.IconContent("AnimationClip Icon");

            case EventType.PlaySpineAnimation:
                return EditorGUIUtility.IconContent("Avatar Icon");

            case EventType.ToggleObjects:
                return EditorGUIUtility.IconContent("d_Toggle Icon");

            case EventType.PlayAudio:
                return EditorGUIUtility.IconContent("AudioSource Icon");

            case EventType.PlaySequence:
                return EditorGUIUtility.IconContent("d_PlayButton");

            case EventType.ModifyTransform:
                return EditorGUIUtility.IconContent("Transform Icon");

            default:
                return null;
        }
    }

    private void DrawBackground(Rect rect, EventType type)
    {
        Color previous = GUI.backgroundColor;

        switch (type)
        {
            case EventType.PlayAnimation:
                GUI.backgroundColor = new Color(0.7f, 0.9f, 1f);
                break;

            case EventType.PlaySpineAnimation:
                GUI.backgroundColor = new Color(0.8f, 0.7f, 1f);
                break;

            case EventType.ToggleObjects:
                GUI.backgroundColor = new Color(0.8f, 1f, 0.9f);
                break;

            case EventType.PlayAudio:
                GUI.backgroundColor = new Color(1f, 0.9f, 0.6f);
                break;

            case EventType.PlaySequence:
                GUI.backgroundColor = new Color(1f, 0.7f, 0.4f);
                break;

            case EventType.ModifyTransform:
                GUI.backgroundColor = new Color(0.6f, 0.9f, 0.9f);
                break;

            default:
                GUI.backgroundColor = new Color(0.9f, 0.9f, 0.9f);
                break;
        }

        GUI.Box(rect, GUIContent.none);

        GUI.backgroundColor = previous;
    }
}
#endif