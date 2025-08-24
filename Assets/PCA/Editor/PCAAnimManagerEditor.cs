using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Malee.List;
using UnityEngine.Animations.Rigging;
using UnityEngine.Events;

[CustomPropertyDrawer(typeof(StringGameObjectDictionary))]
public class StringGameObjectDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }

[CustomPropertyDrawer(typeof(StringPCAKeyframeDataDictionary))]
public class StringPCAKeyframeDataDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }

[CustomPropertyDrawer(typeof(TransformDataDictionary))]
public class TransformDataDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }

[CustomPropertyDrawer(typeof(StateMachineStatePCAAnimStateDictionary))]
public class StateMachineStatePCAAnimStateDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }

[CustomEditor(typeof(PCAAnimManager))]
public class PCAAnimManagerEditor : Editor {
    private PCAAnimManager animManager = null;
    private Animator mainAnim;
    private RigBuilder rigBuilder;

    private List<bool> stateSelectedBools;

    private List<ReorderableList> frameListLists;

    private GUIStyle warningTextStyle;

    private SerializedProperty statesListProp;
    private SerializedProperty additionalConstraintsByNameProp;
    private SerializedProperty onKeyframeAppliedProp;
    private SerializedProperty inPreviewModeProp;

    private List<SerializedProperty> stateProps;
    private List<SerializedProperty> stateIDProps;

    private EditorApplication.CallbackFunction compilationDelegate;
    private System.Action<PlayModeStateChange> exitEditModeDelegate;
    private Undo.UndoRedoCallback undoRedoPerformedDelegate;

    private bool needsUpdateInternalStateData = false;

    private int lastKnownArraySize = -1;
    void OnEnable() {
        warningTextStyle = new GUIStyle();
        warningTextStyle.normal.textColor = Color.yellow;

        statesListProp = serializedObject.FindProperty("stateList");
        additionalConstraintsByNameProp = serializedObject.FindProperty("additionalConstraintsByName");
        onKeyframeAppliedProp = serializedObject.FindProperty("onKeyframeApplied");

        inPreviewModeProp = serializedObject.FindProperty("inPreviewMode");

        animManager = (PCAAnimManager)target;
        compilationDelegate = () => CheckForCompilation(this, animManager);
        exitEditModeDelegate = state => CheckForExitEditMode(state, this, animManager);
        undoRedoPerformedDelegate = () => CheckForUndoWithPreviewModeOn(this, animManager);

        rigBuilder = animManager.GetComponent<RigBuilder>();

        mainAnim = animManager.GetComponent<Animator>();

        UpdateInternalStateData();
    }

    private void UpdateInternalStateData() {
        stateSelectedBools = new List<bool>();
        stateProps = new List<SerializedProperty>();
        stateIDProps = new List<SerializedProperty>();
        frameListLists = new List<ReorderableList>();

        // Add data for all existing states in the list at the start
        for (int i = 0; i < statesListProp.arraySize; i++) {
            SerializedProperty stateProp = statesListProp.GetArrayElementAtIndex(i);

            stateProps.Add(stateProp);
            stateIDProps.Add(stateProp.FindPropertyRelative("id"));
            AddNewFrameReoderableList(stateProp.FindPropertyRelative("frames"));
            stateSelectedBools.Add(false);
        }

        lastKnownArraySize = statesListProp.arraySize;
    }

    private void AddNewFrameReoderableList(SerializedProperty framesListProp) {
        ReorderableList newFrameList = new ReorderableList(framesListProp);
        newFrameList.canAdd = false;

        frameListLists.Add(newFrameList);
    }

    private void HandleStatesListSizeChange() {
		// In case (like an undo) that ONLY internal structures need to be updated (not list prop)
		if (statesListProp.arraySize != lastKnownArraySize) {
			while (statesListProp.arraySize > lastKnownArraySize) {
                SerializedProperty newStateProp = statesListProp.GetArrayElementAtIndex(lastKnownArraySize);
                stateProps.Add(newStateProp);
                stateIDProps.Add(newStateProp.FindPropertyRelative("id"));
                AddNewFrameReoderableList(newStateProp.FindPropertyRelative("frames"));
                stateSelectedBools.Add(false);

                lastKnownArraySize++;
            }
			while (statesListProp.arraySize < lastKnownArraySize) {
				stateSelectedBools.RemoveAt(lastKnownArraySize - 1);
				stateProps.RemoveAt(lastKnownArraySize - 1);
				stateIDProps.RemoveAt(lastKnownArraySize - 1);
				frameListLists.RemoveAt(lastKnownArraySize - 1);

				lastKnownArraySize--;
			}
		}
		lastKnownArraySize = statesListProp.arraySize;
	}

    private void ForceExitPreviewAndAnimationMode(PCAAnimManager animManager, RigBuilder rigBuilder) {
        animManager.SetPreviewMode(false);
        rigBuilder.StopPreview();

        animManager.ApplyDefaultKeyframe();

        AnimationMode.StopAnimationMode();

        animManager.ApplyAllDefaultBoneData();
    }

    private void CheckForUndoWithPreviewModeOn(PCAAnimManagerEditor animManagerEditor, PCAAnimManager animManager) {
        if (!animManager.CheckIfInPreviewMode()) {
            RigBuilder rigBuilder = animManager.GetComponent<RigBuilder>();

            serializedObject.Update();

            ForceExitPreviewAndAnimationMode(animManager, rigBuilder);

            Undo.undoRedoPerformed -= animManagerEditor.undoRedoPerformedDelegate;

            serializedObject.ApplyModifiedProperties();
        }
    }

    private void CheckForExitEditMode(PlayModeStateChange state, PCAAnimManagerEditor animManagerEditor, PCAAnimManager animManager) {
        if (state == PlayModeStateChange.ExitingEditMode && animManager != null) {
            RigBuilder rigBuilder = animManager.GetComponent<RigBuilder>();

            if (animManager.CheckIfInPreviewMode()) {
                serializedObject.Update();

                ForceExitPreviewAndAnimationMode(animManager, rigBuilder);

                EditorApplication.playModeStateChanged -= animManagerEditor.exitEditModeDelegate;

                serializedObject.ApplyModifiedProperties();
            }
        }
    }

    private void CheckForCompilation(PCAAnimManagerEditor animManagerEditor, PCAAnimManager animManager) {
        if (EditorApplication.isCompiling) {
            RigBuilder rigBuilder = animManager.GetComponent<RigBuilder>();

            if (animManager.CheckIfInPreviewMode()) {
                serializedObject.Update();

                ForceExitPreviewAndAnimationMode(animManager, rigBuilder);

                EditorApplication.update -= animManagerEditor.compilationDelegate;

                serializedObject.ApplyModifiedProperties();
            }
        }
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();

        if (animManager.CheckIfDefaultKeyframeSet()) {
            if (!EditorApplication.isPlaying && AnimationMode.InAnimationMode()) {
                rigBuilder.UpdatePreviewGraph(mainAnim.playableGraph);
            }

            DrawUILine(Color.clear);

            if (AnimationMode.InAnimationMode() && !inPreviewModeProp.boolValue) {
                GUILayout.Label("Can't enter preview. Animation mode enabled elsewhere.", warningTextStyle);
                GUI.backgroundColor = Color.red;
                EditorGUI.BeginDisabledGroup(true);
            }
            else {
                if (inPreviewModeProp.boolValue) {
                    GUI.backgroundColor = Color.green * 0.5f;
                }
                else {
                    GUI.backgroundColor = Color.green;
                }
                EditorGUI.BeginDisabledGroup(false);
            }
            if (GUILayout.Button("Preview", GUILayout.Width(100), GUILayout.Height(20))) {
                if (inPreviewModeProp.boolValue) {
                    inPreviewModeProp.boolValue = false;
                    rigBuilder.StopPreview();

                    animManager.ApplyDefaultKeyframe();

                    AnimationMode.StopAnimationMode();

                    animManager.ApplyAllDefaultBoneData();

                    EditorApplication.update -= compilationDelegate;
                    EditorApplication.playModeStateChanged -= exitEditModeDelegate;
                    Undo.undoRedoPerformed -= undoRedoPerformedDelegate;
                }
                else {
                    inPreviewModeProp.boolValue = true;
                    AnimationMode.StartAnimationMode();

                    rigBuilder.Build();

                    rigBuilder.StartPreview();

                    EditorApplication.update += compilationDelegate;
                    EditorApplication.playModeStateChanged += exitEditModeDelegate;
                    Undo.undoRedoPerformed += undoRedoPerformedDelegate;
                }
            }
            GUI.backgroundColor = Color.white;
            EditorGUI.EndDisabledGroup();

            DrawUILine(Color.clear);

            if (inPreviewModeProp.boolValue) {
                DrawUILine(Color.red);

                GUILayout.Label($"List Size: {statesListProp.arraySize}");

                HandleStatesListSizeChange();

                int removedStateIndex = -1;

                for (int i = 0; i < stateProps.Count; i++) {
                    SerializedProperty idProp = stateIDProps[i];

                    stateSelectedBools[i] = EditorGUILayout.Foldout(stateSelectedBools[i], idProp.stringValue);
                    if (stateSelectedBools[i]) {
                        EditorGUILayout.PropertyField(idProp, new GUIContent("ID"));

                        EditorGUILayout.PropertyField(stateProps[i].FindPropertyRelative("stateMachineState"), new GUIContent("State Machine State"));

                        frameListLists[i].DoLayoutList();

                        int[] selectedFrames = frameListLists[i].Selected;
                        if (selectedFrames.Length == 1) {
                            int selectedFrameData = selectedFrames[0];
                            if (GUILayout.Button("Load Selected Frame")) {
                                animManager.OnLoadFrameButton(i, selectedFrameData);
                                EditorUtility.SetDirty(animManager);
                            }

                            if (GUILayout.Button("Update Selected Frame")) {
                                animManager.OnUpdateFrameButton(i, selectedFrameData);
                                EditorUtility.SetDirty(animManager);
                            }
                        }

                        DrawUILine(Color.green);

                        if (animManager != null) {
                            if (GUILayout.Button("Record New Frame")) {
                                animManager.OnNewFrameButton(i);
                                EditorUtility.SetDirty(animManager);
                            }
                        }

                        DrawUILine(Color.green);
                    }

                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Add", GUILayout.Width(100), GUILayout.Height(20))) {
                        statesListProp.InsertArrayElementAtIndex(i + 1);

                        SerializedProperty stateProp = statesListProp.GetArrayElementAtIndex(i + 1);
                        stateProp.FindPropertyRelative("id").stringValue = $"New State ({statesListProp.arraySize})";

                        serializedObject.ApplyModifiedProperties();

                        animManager.SetUpNewStateMachineStateForAnimState(i + 1);

                        needsUpdateInternalStateData = true;
                    }

                    if (GUILayout.Button("Remove", GUILayout.Width(100), GUILayout.Height(20))) {
                        removedStateIndex = i;

                        needsUpdateInternalStateData = true;
                    }
                    EditorGUILayout.EndHorizontal();
                }

                if (removedStateIndex >= 0) {
                    animManager.RemoveStateMachineStateForAnimState(removedStateIndex);

                    statesListProp.DeleteArrayElementAtIndex(removedStateIndex);
                }

                if (needsUpdateInternalStateData) {
                    UpdateInternalStateData();
                }
                needsUpdateInternalStateData = false;

                DrawUILine(Color.red);
            }
        }

        if (animManager.CheckIfDefaultKeyframeSet()) {
            EditorGUILayout.PropertyField(onKeyframeAppliedProp, new GUIContent("On Keyframe Applied Callback"));
        }

        if (!animManager.CheckIfDefaultKeyframeSet()) {
            EditorGUILayout.PropertyField(additionalConstraintsByNameProp, new GUIContent("Additional Constraints"));
        }

        if (!animManager.CheckIfDefaultKeyframeSet()) {
            if (GUILayout.Button("Set Default Pose")) {
                animManager.OnCaptureDefaultPoseButton();
                EditorUtility.SetDirty(animManager);
            }
        }

		/*if (animManager.CheckIfDefaultKeyframeSet() && GUILayout.Button("Save as Default States")) {
			PCAAnimStatesPreset defaultStatesPreset = animManager.SaveDefaultStatesPreset();
			if (defaultStatesPreset != null) {
				EditorUtility.SetDirty(defaultStatesPreset);
			}
		}*/

		serializedObject.ApplyModifiedProperties();
    }

    // Credit: alexanderameye (https://forum.unity.com/threads/horizontal-line-in-editor-window.520812/)
    public static void DrawUILine(Color color, int thickness = 2, int padding = 10) {
        Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
        r.height = thickness;
        r.y += padding / 2;
        r.x -= 2;
        r.width += 6;
        EditorGUI.DrawRect(r, color);
    }
}
