using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Events;

[System.Serializable]
public class IntStateDictionary : SerializableDictionary<int, PCAAnimState> { }

[System.Serializable]
public class StringGameObjectDictionary : SerializableDictionary<string, GameObject> { }

[System.Serializable]
public class TransformDataDictionary : SerializableDictionary<Transform, PCAKeyframeTransformData> { }

[System.Serializable]
public class StateMachineStatePCAAnimStateDictionary : SerializableDictionary<AnimatorState, PCAAnimState> { }

[System.Serializable]
public class PCAAnimStateEvent : UnityEvent<PCAAnimState, PCAAnimFrameData> {
}

public class PCAAnimManager : MonoBehaviour {
    public PCAAnimStateEvent onKeyframeApplied = new PCAAnimStateEvent();
    public StringGameObjectDictionary additionalConstraintsByName = new StringGameObjectDictionary();
    public List<PCAAnimState> stateList = null;
    public StateMachineStatePCAAnimStateDictionary animStateByStateMachineState = new StateMachineStatePCAAnimStateDictionary();

    [SerializeField]
    private AnimatorController stateMachineController = null;
    [SerializeField]
    // In order: "Idle", "Run"
    private AnimatorState[] defaultStateMachineStates = null;
    [SerializeField]
    private StringGameObjectDictionary constraintByName = null;
    [SerializeField]
    private PCAKeyframe defaultKeyframe = null;
    [SerializeField]
    private bool defaultKeyframeSet = false;

    [SerializeField]
    private bool inPreviewMode = false;
    [SerializeField]
    private EditorApplication.CallbackFunction compilationDelegate;

    [SerializeField]
    private TransformDataDictionary allDefaultBoneDataByTransform = new TransformDataDictionary();
    [SerializeField]
    private Transform hipBone;

    //private IntStateDictionary stateDict;
    private PCAAnimState currentState;
    private float interpRatio = 0;
    private float animTime = 0;
    private PCAAnimFrameData previousFrame;
    private PCAAnimFrameData currentFrame;
    private PCAAnimFrameData nextFrame;
    private int indexFrame = 0;

    private Animator mainAnim = null;
    private RigBuilder rigBuilder = null;

    private void Start() {
        currentFrame = new PCAAnimFrameData($"{gameObject.name}_now", CaptureKeyframe("burnt", false));

        if (mainAnim == null) {
            mainAnim = GetComponent<Animator>();
        }
        if (rigBuilder == null) {
            rigBuilder = GetComponent<RigBuilder>();
        }

        InitStateMachineStateToAnimStateDict();
    }

    private void Update() {
        if (previousFrame != null && currentFrame != null && nextFrame != null && currentState != null) {
            HandleAnim();
        }
    }

    private void HandleAnim() {
        animTime += nextFrame.speed*Time.deltaTime;
        animTime = Mathf.Clamp(animTime, 0f, 1f);
        interpRatio = nextFrame.curve.Evaluate(animTime);
        //use previous and next to interp current
        currentFrame.frame = InterpKeyframe(previousFrame.frame, nextFrame.frame, interpRatio);
        //Apply current
        ApplyKeyframe(currentFrame.frame);

        onKeyframeApplied.Invoke(currentState, currentFrame);

        if (animTime >= 1f) {
            previousFrame = nextFrame;
            indexFrame++;
            indexFrame = indexFrame % currentState.frames.Count;
            nextFrame = currentState.frames[indexFrame];
            animTime = 0f;
        }
    }

    public void PlayAnim(AnimatorState stateMachineState) {
        if (animStateByStateMachineState == null || animStateByStateMachineState.Count <= 0) {
            InitStateMachineStateToAnimStateDict();
		}

        if (!animStateByStateMachineState.ContainsKey(stateMachineState)) {
            Debug.LogError($"{name}: PCAAnimManager did not have associated anim state with state machine state: '{stateMachineState.name}'");
        }
        else {
            previousFrame = currentFrame;
            animTime = 0;
            currentState = animStateByStateMachineState[stateMachineState];
            indexFrame = 0;
            nextFrame = currentState.frames[indexFrame];
        }
    }

    private void InitStateMachineStateToAnimStateDict() {
        animStateByStateMachineState = new StateMachineStatePCAAnimStateDictionary();
        foreach (PCAAnimState animState in stateList) {
            if (animState.stateMachineState == null) {
                Debug.LogError($"{name}: PCAAnimManager did not have associated state machine state with anim state named: '{animState.id}'");
			}
            else {
                animStateByStateMachineState.Add(animState.stateMachineState, animState);
            }
        }
    }

    private void LoadInAdditionalConstraintsIfNeeded() {
        // Add in valid additional constraints by name
        foreach (string additionalConstraintName in additionalConstraintsByName.Keys) {
            if (additionalConstraintName.Length <= 0) {
                Debug.LogWarning($"WARNING - Unable to add a additional constraint game object without a name string associated with it.");
                continue;
            }
            if (constraintByName != null && constraintByName.ContainsKey(additionalConstraintName)) {
                continue;
            }

            constraintByName.Add(additionalConstraintName, additionalConstraintsByName[additionalConstraintName]);
        }
    }

    public void OnNewFrameButton(int stateIndex) {
        PCAAnimState animState = stateList[stateIndex];
        string newFrameName = $"{animState.id}_frame_{animState.GetFrameCount()}";
        string newFrameKeyframeName = $"{newFrameName}_keyframe";
        PCAKeyframe newFrameKeyframe = CaptureKeyframe(newFrameKeyframeName);

        animState.AddFrame(newFrameName, newFrameKeyframe);
    }

    private void RecursiveAddDefaultBoneData(Transform boneTransform) {
        PCAKeyframeTransformData boneData = new PCAKeyframeTransformData(boneTransform.name, boneTransform.localPosition, boneTransform.localEulerAngles);
        AddDefaultBoneDataByTransform(boneTransform, boneData);

        foreach (Transform childTransform in boneTransform) {
            RecursiveAddDefaultBoneData(childTransform);
        }
    }

    public void OnCaptureDefaultPoseButton() {
        // Make sure all additional constraints are added at this point
        LoadInAdditionalConstraintsIfNeeded();

        RecursiveAddDefaultBoneData(hipBone);

        defaultKeyframeSet = true;
        defaultKeyframe = CaptureKeyframe("default_pose", true);

        if (stateList == null) {
            stateList = new List<PCAAnimState>();
        }
        LoadDefaultStatesPreset();
    }

    public void OnLoadFrameButton(int stateIndex, int frameIndex) {
        PCAAnimState animState = stateList[stateIndex];

        PCAAnimFrameData selectedFrame = animState.frames[frameIndex];

        ApplyKeyframe(selectedFrame.frame);
    }

    public void OnUpdateFrameButton(int stateIndex, int frameIndex) {
        PCAAnimState animState = stateList[stateIndex];

        PCAAnimFrameData selectedFrame = animState.frames[frameIndex];

        UpdateKeyframe(selectedFrame.frame);
    }

    public void AddConstraintWithName(string constraintName, GameObject constraintObject) {
        if (constraintByName == null) {
            constraintByName = new StringGameObjectDictionary();
        }

        constraintByName[constraintName] = constraintObject;
    }

    public bool CheckIfDefaultKeyframeSet() {
        return defaultKeyframeSet;
    }

    public PCAKeyframe CaptureKeyframe(string keyframeName, bool isDefaultPose = false) {
        // Cannot capture any keypose (aside from default) until default is set
        if (!defaultKeyframeSet && !isDefaultPose) {
            Debug.LogError("ERROR - Can't capture any keyframe (aside from default pose) until default pose is set");
            return null;
        }

        List<PCAKeyframeTransformData> allNewTransformData = CaptureTransformData(keyframeName);

        return new PCAKeyframe(keyframeName, allNewTransformData);
    }

    public void UpdateKeyframe(PCAKeyframe inKeyframe) {
        List<PCAKeyframeTransformData> allNewTransformData = CaptureTransformData(inKeyframe.GetKeyframeName());
        inKeyframe.SetTransformData(allNewTransformData);
    }

    public List<PCAKeyframeTransformData> CaptureTransformData(string keyframeName) {
        List<PCAKeyframeTransformData> allNewTransformData = new List<PCAKeyframeTransformData>();

        foreach (string constraintName in constraintByName.Keys) {
            Transform constraintTransform = constraintByName[constraintName].transform;
            if (keyframeName.Equals("default_pose")) {
                allNewTransformData.Add(
                    new PCAKeyframeTransformData(
                        constraintName,
                        constraintTransform.localPosition,
                        constraintTransform.localEulerAngles
                    )
                );
            }
            else {
                PCAKeyframeTransformData defaultKeyframeData = defaultKeyframe.GetDataForTransformName(constraintName);
                if (defaultKeyframeData == null) {
                    Debug.LogError($"ERROR - Couldn't capture constraint '{constraintName}' for keyframe '{keyframeName}'"
                        + "as it was not found under the default pose (try setting an updated default pose)");
                    return null;
                }
                allNewTransformData.Add(
                    new PCAKeyframeTransformData(
                        constraintName,
                        constraintTransform.localPosition - defaultKeyframeData.GetPos(),
                        constraintTransform.localEulerAngles - defaultKeyframeData.GetRot()
                    )
                );
            }
        }

        return allNewTransformData;
    }

    public void ApplyDefaultKeyframe() {
        if (defaultKeyframeSet && defaultKeyframe != null) {
            ApplyKeyframe(defaultKeyframe);
        }
	}

    public void ApplyKeyframe(PCAKeyframe inKeyframe) {
        if (constraintByName == null) {
            constraintByName = new StringGameObjectDictionary();
            Debug.LogWarning("WARNING - Tried to apply a keyframe with no known constraints attatched");
            return;
        }

        foreach (string constraintName in constraintByName.Keys) {
            PCAKeyframeTransformData transformData = inKeyframe.GetDataForTransformName(constraintName);

            // Return a warning if the rig could not be found
            if (transformData == null) {
                Debug.LogWarning($"WARNING - PCAHumanoidController on '{gameObject.name}' can not find constraint '{constraintName}' in keyframe '{inKeyframe.GetKeyframeName()}'");
                continue;
            }
            Transform constraintTransform = constraintByName[constraintName].transform;

            if (inKeyframe.GetKeyframeName().Equals("default_pose")) {
                constraintTransform.localPosition = transformData.GetPos();
                constraintTransform.localEulerAngles = transformData.GetRot();
            }
            else {
                if (defaultKeyframe == null) {
                    Debug.LogError("ERROR - Can't apply any keyframe before default keyframe is set");
                    return;
                }
                PCAKeyframeTransformData defaultKeyframeData = defaultKeyframe.GetDataForTransformName(constraintName);
                if (defaultKeyframeData == null) {
                    Debug.LogError($"ERROR - Couldn't apply constraint '{constraintName}' for keyframe '{inKeyframe.GetKeyframeName()}'"
                        + "as it was not found under the default pose (try setting an updated default pose)");
                    return;
                }

                constraintTransform.localPosition = transformData.GetPos() + defaultKeyframeData.GetPos();
                constraintTransform.localEulerAngles = transformData.GetRot() + defaultKeyframeData.GetRot();
            }
        }
    }

    public PCAKeyframe InterpKeyframe(PCAKeyframe keyframe1, PCAKeyframe keyframe2, float t) {

        List<PCAKeyframeTransformData> newData = new List<PCAKeyframeTransformData>();
        List<PCAKeyframeTransformData> info1 = keyframe1.GetAllTransformData();
        List<PCAKeyframeTransformData> info2 = keyframe2.GetAllTransformData();
        for (int i = 0; i < info1.Count; i++) {
            Vector3 pos1 = info1[i].GetPos();
            Vector3 pos2 = info2[i].GetPos();
            Vector3 rot1 = info1[i].GetRot();
            Vector3 rot2 = info2[i].GetRot();

            Vector3 newpos = Vector3.LerpUnclamped(pos1, pos2, t);

            float newXRot = Mathf.LerpAngle(rot1.x, rot2.x, t);
            float newYRot = Mathf.LerpAngle(rot1.y, rot2.y, t);
            float newZRot = Mathf.LerpAngle(rot1.z, rot2.z, t);
            Vector3 newrot = new Vector3(newXRot, newYRot, newZRot);

            PCAKeyframeTransformData thisData = new PCAKeyframeTransformData(info1[i].GetTransformName(), newpos, newrot);
            newData.Add(thisData);
        }
        return new PCAKeyframe($"interp_{keyframe1.GetKeyframeName()}_{keyframe2.GetKeyframeName()}_t={t}", newData);
    }

    public PCAAnimStatesPreset SaveDefaultStatesPreset() {
        if (defaultKeyframe == null) {
            Debug.LogWarning("Cannot save states presets before default pose keyframe has been set");
            return null;
        }

        PCAAnimStatesPreset statesDefaultAsset = Resources.Load<PCAAnimStatesPreset>("default_states");
        if (statesDefaultAsset == null) {
            Debug.LogWarning("Could not find default states asset (default_states)");
            return null;
        }

        statesDefaultAsset.presetStates = new List<PCAAnimState>();
        foreach (PCAAnimState state in stateList) {
            statesDefaultAsset.presetStates.Add(state.GetDeepCopy());
        }

        Debug.Log("<color=cyan>Saved new data to default states asset</color>");

        return statesDefaultAsset;
    }

    public void LoadDefaultStatesPreset() {
        if (defaultKeyframe == null) {
            Debug.LogWarning("Cannot load states presets before default pose keyframe has been set");
            return;
        }

        PCAAnimStatesPreset statesDefaultAsset = Resources.Load<PCAAnimStatesPreset>("default_states");
        if (statesDefaultAsset == null) {
            Debug.LogWarning("Could not find default states asset (default_states)");
            return;
        }

        if (statesDefaultAsset.presetStates != null) {
            foreach (PCAAnimState animState in statesDefaultAsset.presetStates) {
                PCAAnimState animStateCopy = animState.GetDeepCopy();
                stateList.Add(animStateCopy);
                if (defaultStateMachineStates != null) {
                    if (animStateCopy.id.Equals("Idle")) {
                        animStateCopy.SetStateMachineState(defaultStateMachineStates[0]);
                    }
                    else if (animStateCopy.id.Equals("Run")) {
                        animStateCopy.SetStateMachineState(defaultStateMachineStates[1]);
                    }
                }
            }
        }
    }

    public void SetHipBone(Transform hipBone) {
        this.hipBone = hipBone;
	}

    public void ApplyAllDefaultBoneData() {
        foreach (Transform boneTransform in allDefaultBoneDataByTransform.Keys) {
            PCAKeyframeTransformData boneTransformData = allDefaultBoneDataByTransform[boneTransform];

            boneTransform.localPosition = boneTransformData.GetPos();
            boneTransform.localEulerAngles = boneTransformData.GetRot();
        }
	}

    public void AddDefaultBoneDataByTransform(Transform boneTransform, PCAKeyframeTransformData transformData) {
        if (!allDefaultBoneDataByTransform.ContainsKey(boneTransform)) {
            allDefaultBoneDataByTransform.Add(boneTransform, transformData);
        }
	}

    public bool CheckIfInPreviewMode() {
        return inPreviewMode;
    }

    public void SetPreviewMode(bool inPreviewMode) {
        this.inPreviewMode = inPreviewMode;
    }

    public void SetStateMachineController(AnimatorController stateMachineController) {
        this.stateMachineController = stateMachineController;
    }

    public void SetDefaultStateMachineStates(AnimatorState[] defaultStateMachineStates) {
        this.defaultStateMachineStates = defaultStateMachineStates;
    }

    public void SetUpNewStateMachineStateForAnimState(int stateIndex) {
        PCAAnimState animState = stateList[stateIndex];

        AnimatorStateMachine rootStateMachine = stateMachineController.layers[0].stateMachine;

        AnimatorState newState = rootStateMachine.AddState(animState.id);
        PCAStateBehavior newStateBehavior = (PCAStateBehavior)newState.AddStateMachineBehaviour(typeof(PCAStateBehavior));

        newStateBehavior.SetAttatchedStateMachineState(newState);

        animState.SetStateMachineState(newState);
    }

    public void RemoveStateMachineStateForAnimState(int stateIndex) {
		PCAAnimState animState = stateList[stateIndex];

        AnimatorStateMachine rootStateMachine = stateMachineController.layers[0].stateMachine;

        if (animState.stateMachineState == null) {
            Debug.LogError($"{name}: PCAAnimManager did not have associated state machine state with anim state named: '{animState.id}'");
            return;
        }

        rootStateMachine.RemoveState(animState.stateMachineState);

        animState.SetStateMachineState(null);
    }
}
