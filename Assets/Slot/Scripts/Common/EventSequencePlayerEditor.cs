using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(EventSequencePlayer))]
public class EventSequencePlayerEditor : Editor
{
    private ReorderableList list;
    private SerializedProperty eventsProp;

    private void OnEnable()
    {
        eventsProp = serializedObject.FindProperty("events");

        list = new ReorderableList(serializedObject, eventsProp, true, true, true, true);

        list.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "Event Timeline");
        };

        list.elementHeightCallback = (int index) =>
        {
            var element = eventsProp.GetArrayElementAtIndex(index);
            var type = element.FindPropertyRelative("eventType");

            EventType eventType = (EventType)type.enumValueIndex;

            float line = EditorGUIUtility.singleLineHeight;
            float space = 4;
            float height = (line + space) * 2;

            switch (eventType)
            {
                case EventType.UnityEvent:
                    height += EditorGUI.GetPropertyHeight(element.FindPropertyRelative("unityEvent"), true);
                    break;

                case EventType.PlayAnimation:
                case EventType.PlaySpineAnimation:
                    height += (line + space) * 2;
                    break;

                case EventType.EnableObject:
                case EventType.DisableObject:
                case EventType.Delay:
                    height += (line + space);
                    break;
            }

            height += (line + space);

            var condition = element.FindPropertyRelative("conditionType");
            if ((ConditionType)condition.enumValueIndex == ConditionType.Custom)
                height += (line + space);

            return height + 10;
        };

        list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            var element = eventsProp.GetArrayElementAtIndex(index);

            var name = element.FindPropertyRelative("eventName");
            var type = element.FindPropertyRelative("eventType");
            var condition = element.FindPropertyRelative("conditionType");

            float line = EditorGUIUtility.singleLineHeight;
            float space = 4;
            float y = rect.y + 5;

            EventType eventType = (EventType)type.enumValueIndex;

            DrawBackground(rect, eventType);

            EditorGUI.PropertyField(new Rect(rect.x + 5, y, rect.width - 10, line), name);
            y += line + space;

            EditorGUI.PropertyField(new Rect(rect.x + 5, y, rect.width - 10, line), type);
            y += line + space;

            switch (eventType)
            {
                case EventType.UnityEvent:
                    var unityEventProp = element.FindPropertyRelative("unityEvent");
                    float h = EditorGUI.GetPropertyHeight(unityEventProp, true);

                    EditorGUI.PropertyField(
                        new Rect(rect.x + 5, y, rect.width - 10, h),
                        unityEventProp,
                        true
                    );
                    y += h + space;
                    break;

                case EventType.PlayAnimation:
                    EditorGUI.PropertyField(
                        new Rect(rect.x + 5, y, rect.width - 10, line),
                        element.FindPropertyRelative("target")
                    );
                    y += line + space;

                    EditorGUI.PropertyField(
                        new Rect(rect.x + 5, y, rect.width - 10, line),
                        element.FindPropertyRelative("animationName")
                    );
                    y += line + space;
                    break;

                case EventType.PlaySpineAnimation:
                    {
                        var targetProp = element.FindPropertyRelative("target");
                        var animProp = element.FindPropertyRelative("animationName");

                        EditorGUI.PropertyField(
                            new Rect(rect.x + 5, y, rect.width - 10, line),
                            targetProp
                        );
                        y += line + space;

                        GameObject target = targetProp.objectReferenceValue as GameObject;
                        var animations = GetSpineAnimations(target);

                        if (animations.Length > 0)
                        {
                            int indexAnim = System.Array.IndexOf(animations, animProp.stringValue);
                            if (indexAnim < 0) indexAnim = 0;

                            int newIndex = EditorGUI.Popup(
                                new Rect(rect.x + 5, y, rect.width - 10, line),
                                "Animation",
                                indexAnim,
                                animations
                            );

                            animProp.stringValue = animations[newIndex];
                        }
                        else
                        {
                            EditorGUI.LabelField(
                                new Rect(rect.x + 5, y, rect.width - 10, line),
                                "No Spine Animations Found"
                            );
                        }

                        y += line + space;
                    }
                    break;

                case EventType.EnableObject:
                case EventType.DisableObject:
                    EditorGUI.PropertyField(
                        new Rect(rect.x + 5, y, rect.width - 10, line),
                        element.FindPropertyRelative("target")
                    );
                    y += line + space;
                    break;

                case EventType.Delay:
                    EditorGUI.PropertyField(
                        new Rect(rect.x + 5, y, rect.width - 10, line),
                        element.FindPropertyRelative("delay")
                    );
                    y += line + space;
                    break;
            }

            EditorGUI.PropertyField(
                new Rect(rect.x + 5, y, rect.width - 10, line),
                condition
            );
            y += line + space;

            if ((ConditionType)condition.enumValueIndex == ConditionType.Custom)
            {
                EditorGUI.PropertyField(
                    new Rect(rect.x + 5, y, rect.width - 10, line),
                    element.FindPropertyRelative("conditionValue"),
                    new GUIContent("Value")
                );
            }
        };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawControls();

        EditorGUILayout.Space();
        list.DoLayoutList();

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("playOnStart"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("loop"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("onComplete"));

        serializedObject.ApplyModifiedProperties();
    }

    void DrawControls()
    {
        EventSequencePlayer player = (EventSequencePlayer)target;

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("▶ Play"))
            player.Play();

        if (GUILayout.Button("⏹ Stop"))
            player.Stop();

        EditorGUILayout.EndHorizontal();
    }

    string[] GetSpineAnimations(GameObject target)
    {
        if (target == null) return new string[0];

        var skeleton = target.GetComponent<Spine.Unity.SkeletonAnimation>();
        if (skeleton == null || skeleton.SkeletonDataAsset == null)
            return new string[0];

        var data = skeleton.SkeletonDataAsset.GetSkeletonData(true);

        List<string> names = new List<string>();
        foreach (var anim in data.Animations)
        {
            names.Add(anim.Name);
        }

        return names.ToArray();
    }

    void DrawBackground(Rect rect, EventType type)
    {
        Color prev = GUI.backgroundColor;
        GUI.backgroundColor = GetColor(type);
        GUI.Box(rect, GUIContent.none);
        GUI.backgroundColor = prev;
    }

    Color GetColor(EventType type)
    {
        switch (type)
        {
            case EventType.Delay: return new Color(1f, 0.9f, 0.4f);
            case EventType.PlayAnimation: return new Color(0.4f, 0.8f, 1f);
            case EventType.PlaySpineAnimation: return new Color(0.6f, 0.4f, 1f);
            case EventType.EnableObject: return new Color(0.4f, 1f, 0.4f);
            case EventType.DisableObject: return new Color(1f, 0.4f, 0.4f);
            default: return Color.white;
        }
    }
}