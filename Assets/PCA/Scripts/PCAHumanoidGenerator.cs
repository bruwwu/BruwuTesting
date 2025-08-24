using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEditor.Animations;

public class PCAHumanoidGenerator : ScriptableObject {
    public Transform parent;
    public Transform hip;
    public Transform chest;
    public Transform head;
    public Transform leftUpperLeg;
    public Transform leftLowerLeg;
    public Transform leftFoot;
    public Transform rightUpperLeg;
    public Transform rightLowerLeg;
    public Transform rightFoot;
    public Transform leftUpperArm;
    public Transform leftLowerArm;
    public Transform leftHand;
    public Transform rightUpperArm;
    public Transform rightLowerArm;
    public Transform rightHand;

    private Transform[] leftLegTransforms;
    private Transform[] rightLegTransforms;

    private Transform[] leftArmTransforms;
    private Transform[] rightArmTransforms;

    private Mesh effectorSphereMesh = null;
    private Mesh effectorBoxMesh = null;
    private Mesh effectorCircleMesh = null;
    private Mesh effectorLocatorMesh = null;
    public void OnGenerate() {
        leftLegTransforms = new Transform[] {
            leftUpperLeg,
            leftLowerLeg,
            leftFoot
        };

        rightLegTransforms = new Transform[] {
            rightUpperLeg,
            rightLowerLeg,
            rightFoot
        };

        leftArmTransforms = new Transform[] {
            leftUpperArm,
            leftLowerArm,
            leftHand
        };

        rightArmTransforms = new Transform[] {
            rightUpperArm,
            rightLowerArm,
            rightHand
        };

        if (parent != null) {
            RigSetup(parent);
		}
        if (hip != null) {
            BoneRendererSetup(hip);
        }
    }

    public void AddConstraintEffector(RigBuilder rigBuilder, Transform constraintTransform, Color effectorColor, string meshType, float effectorSize = 0.1f) {
        if (effectorSphereMesh == null) {
            effectorSphereMesh = Resources.Load<Mesh>("EffectorShapes/BallEffector");
        }
        if (effectorBoxMesh == null) {
            effectorBoxMesh = Resources.Load<Mesh>("EffectorShapes/BoxEffector");
        }
        if (effectorCircleMesh == null) {
            effectorCircleMesh = Resources.Load<Mesh>("EffectorShapes/CircleEffector");
        }
        if (effectorLocatorMesh == null) {
            effectorLocatorMesh = Resources.Load<Mesh>("EffectorShapes/LocatorEffector");
        }

        Mesh selectedMesh;
        switch (meshType) {
            case "box":
                selectedMesh = effectorBoxMesh;
                break;
            case "circle":
                selectedMesh = effectorCircleMesh;
                break;
            case "locator":
                selectedMesh = effectorLocatorMesh;
                break;
            default:
                selectedMesh = effectorSphereMesh;
                break;
        }

        RigEffectorData.Style newStyle = new RigEffectorData.Style();
        newStyle.size = effectorSize;
        newStyle.color = effectorColor;
        newStyle.shape = selectedMesh;
        newStyle.position = Vector3.zero;
        newStyle.rotation = Vector3.zero;

#if !UNITY_2020_1_OR_NEWER
        rigBuilder.AddEffector(constraintTransform);
#else
        rigBuilder.AddEffector(constraintTransform, newStyle);
#endif

        RigEffectorData newEffectorData = null;
        foreach (RigEffectorData rigEffectorData in rigBuilder.effectors) {
            if (rigEffectorData.transform == constraintTransform) {
                newEffectorData = rigEffectorData;
            }
        }
        if (newEffectorData == null) {
            Debug.LogWarning($"WARNING - Unable to find newly created effector on transform '{constraintTransform}'");
            return;
        }

        newEffectorData.Initialize(constraintTransform, newStyle);
    }

    private Component AddComponentWithUndo(Type componentType, Transform inTransform) {
        var outComponent = inTransform.GetComponent(componentType);

        if (outComponent == null) {
            outComponent = Undo.AddComponent(inTransform.gameObject, componentType);
        }
        else {
            Undo.RecordObject(outComponent, $"{componentType.FullName} Component Added.");
        }

        return outComponent;
    }

    public void RigSetup(Transform transform) {
        RigBuilder rigBuilder = (RigBuilder)AddComponentWithUndo(typeof(RigBuilder), transform);

        PCAAnimManager animManager = (PCAAnimManager)
            AddComponentWithUndo(typeof(PCAAnimManager), transform);

        RuntimeAnimatorController newStateController = CreateStateMachineController(animManager);
        Animator stateAnimator = parent.GetComponent<Animator>();
        if (stateAnimator == null) {
            stateAnimator = parent.gameObject.AddComponent<Animator>();
        }
        stateAnimator.runtimeAnimatorController = newStateController;

        animManager.SetHipBone(hip);

        //Set up main parent object containg 'Rig' component
        GameObject rigGameObject = new GameObject("rig");
        Undo.RegisterCreatedObjectUndo(rigGameObject, "rig");
        rigGameObject.transform.SetParent(rigBuilder.transform);
        rigGameObject.transform.localPosition = Vector3.zero;
        rigGameObject.transform.localRotation = Quaternion.identity;
        // Set up 'Rig' component
        Rig rig = Undo.AddComponent<Rig>(rigGameObject);

#if UNITY_2020_1_OR_NEWER
        if (rigBuilder.layers.Count > 0) {
            rigBuilder.layers[0] = new RigLayer(rig);
        }
        else {
            rigBuilder.layers.Add(new RigLayer(rig));
        }
#else
        rigBuilder.layers.Add(new RigBuilder.RigLayer(rig));
#endif

        Vector3 characterFaceDir = (rightUpperArm.position - leftUpperArm.position).normalized;
        characterFaceDir = (Quaternion.AngleAxis(-90.0f, Vector3.up) * characterFaceDir).normalized;

        // Add hip constraint
        GameObject hipConstraintObject = new GameObject("hips_constraint");
        Undo.RegisterCreatedObjectUndo(hipConstraintObject, "hips_constraint");
        hipConstraintObject.transform.SetParent(rigGameObject.transform);
        hipConstraintObject.transform.localPosition = Vector3.zero;
        hipConstraintObject.transform.localRotation = Quaternion.identity;

        GameObject hipConstraintController = new GameObject("hips_controller");
        Undo.RegisterCreatedObjectUndo(hipConstraintController, "hips_controller");
        hipConstraintController.transform.SetParent(hipConstraintObject.transform);
        hipConstraintController.transform.position = hip.position;
        hipConstraintController.transform.rotation = hip.rotation;
        AddConstraintEffector(rigBuilder, hipConstraintController.transform, new Color(0.1854615f, 1.0f, 0.0f, 0.5f), "circle", 0.3f);

        MultiParentConstraint hipMultiParentConstraint = hipConstraintObject.AddComponent<MultiParentConstraint>();
        hipMultiParentConstraint.data.constrainedObject = hip;

        WeightedTransformArray hipWeightedTransformArray = new WeightedTransformArray();
        hipWeightedTransformArray.Add(new WeightedTransform(hipConstraintController.transform, 1.0f));
        hipMultiParentConstraint.data.sourceObjects = hipWeightedTransformArray;

        animManager.AddConstraintWithName("hips", hipConstraintController);

        // Add chest aim constraint
        GameObject chestAimObject = new GameObject("chest_aim");
        Undo.RegisterCreatedObjectUndo(chestAimObject, "chest_aim");
        chestAimObject.transform.SetParent(rigGameObject.transform);
        chestAimObject.transform.localPosition = Vector3.zero;
        chestAimObject.transform.localRotation = Quaternion.identity;

        GameObject chestTargetObject = new GameObject("chest_target");
        Undo.RegisterCreatedObjectUndo(chestTargetObject, "chest_target");
        chestTargetObject.transform.SetParent(chestAimObject.transform);
        chestTargetObject.transform.position = chest.position;
        // Move the target in the direction of the character face direction
        chestTargetObject.transform.position += characterFaceDir * 0.5f;

        chestTargetObject.transform.rotation = chest.rotation;
        AddConstraintEffector(rigBuilder, chestTargetObject.transform, new Color(0.1854615f, 1.0f, 0.0f, 0.5f), "box");

        MultiAimConstraint chestMultiAimConstraint = chestAimObject.AddComponent<MultiAimConstraint>();
        chestMultiAimConstraint.data.constrainedObject = chest;
        chestMultiAimConstraint.data.aimAxis = MultiAimConstraintData.Axis.Z;

        WeightedTransformArray chestWeightedTransformArray = new WeightedTransformArray();
        chestWeightedTransformArray.Add(new WeightedTransform(chestTargetObject.transform, 1.0f));
        chestMultiAimConstraint.data.sourceObjects = chestWeightedTransformArray;

        chestMultiAimConstraint.data.constrainedZAxis = false;

        animManager.AddConstraintWithName("chest", chestTargetObject);

        // Set up parent object holding ik constraints
        GameObject feetIKParentGameObject = new GameObject("feet_ik");
        Undo.RegisterCreatedObjectUndo(feetIKParentGameObject, "feet_ik");
        feetIKParentGameObject.transform.SetParent(rigGameObject.transform);
        feetIKParentGameObject.transform.localPosition = Vector3.zero;
        feetIKParentGameObject.transform.localRotation = Quaternion.identity;

        GameObject leftFootConstraintObject = SetUpLimbConstraint("left_foot", feetIKParentGameObject, leftLegTransforms);
        // Move the hint in the direction of the character face direction
        leftFootConstraintObject.transform.GetChild(0).position += characterFaceDir * 0.25f;

        GameObject rightFootConstraintObject = SetUpLimbConstraint("right_foot", feetIKParentGameObject, rightLegTransforms);
        // Move the hint in the direction of the character face direction
        rightFootConstraintObject.transform.GetChild(0).position += characterFaceDir * 0.25f;

        AddConstraintEffector(rigBuilder, leftFootConstraintObject.transform, new Color(1.0f, 0.0f, 0.0f, 0.5f), "sphere");
        AddConstraintEffector(rigBuilder, leftFootConstraintObject.transform.GetChild(0), new Color(1.0f, 0.0f, 0.0f, 0.5f), "locator");

        AddConstraintEffector(rigBuilder, rightFootConstraintObject.transform, new Color(0.0f, 1.0f, 1.0f, 0.5f), "sphere");
        AddConstraintEffector(rigBuilder, rightFootConstraintObject.transform.GetChild(0), new Color(0.0f, 1.0f, 1.0f, 0.5f), "locator");

        animManager.AddConstraintWithName("left_foot", leftFootConstraintObject);
        animManager.AddConstraintWithName("right_foot", rightFootConstraintObject);

        // Set up parent object holding ik constraints
        GameObject handsIKParentGameObject = new GameObject("hands_ik");
        Undo.RegisterCreatedObjectUndo(handsIKParentGameObject, "hands_ik");
        handsIKParentGameObject.transform.SetParent(rigGameObject.transform);
        handsIKParentGameObject.transform.localPosition = Vector3.zero;
        handsIKParentGameObject.transform.localRotation = Quaternion.identity;

        GameObject leftHandConstraintObject = SetUpLimbConstraint("left_hand", handsIKParentGameObject, leftArmTransforms);
        // Move the hint in the direction of the character face direction
        leftHandConstraintObject.transform.GetChild(0).position -= characterFaceDir * 0.25f;
        GameObject rightHandConstraintObject = SetUpLimbConstraint("right_hand", handsIKParentGameObject, rightArmTransforms);
        // Move the hint in the direction of the character face direction
        rightHandConstraintObject.transform.GetChild(0).position -= characterFaceDir * 0.25f;

        AddConstraintEffector(rigBuilder, leftHandConstraintObject.transform, new Color(1.0f, 0.0f, 0.0f, 0.5f), "sphere");
        AddConstraintEffector(rigBuilder, leftHandConstraintObject.transform.GetChild(0), new Color(1.0f, 0.0f, 0.0f, 0.5f), "locator");


        AddConstraintEffector(rigBuilder, rightHandConstraintObject.transform, new Color(0.0f, 1.0f, 1.0f, 0.5f), "sphere");
        AddConstraintEffector(rigBuilder, rightHandConstraintObject.transform.GetChild(0), new Color(0.0f, 1.0f, 1.0f, 0.5f), "locator");

        animManager.AddConstraintWithName("left_hand", leftHandConstraintObject);
        animManager.AddConstraintWithName("right_hand", rightHandConstraintObject);

        // Add head aim constraint
        GameObject headAimObject = new GameObject("head_aim");
        Undo.RegisterCreatedObjectUndo(headAimObject, "head_aim");
        headAimObject.transform.SetParent(rigGameObject.transform);
        headAimObject.transform.localPosition = Vector3.zero;
        headAimObject.transform.localRotation = Quaternion.identity;

        GameObject headTargetObject = new GameObject("head_target");
        Undo.RegisterCreatedObjectUndo(headTargetObject, "head_target");
        headTargetObject.transform.SetParent(headAimObject.transform);
        headTargetObject.transform.position = head.position;
        // Move the target in the direction of the character face direction
        headTargetObject.transform.position += characterFaceDir * 0.5f;

        headTargetObject.transform.rotation = head.rotation;
        AddConstraintEffector(rigBuilder, headTargetObject.transform, new Color(1.0f, 0.0f, 0.8835707f, 0.5f), "box");

        MultiAimConstraint headMultiAimConstraint = headAimObject.AddComponent<MultiAimConstraint>();
        headMultiAimConstraint.data.constrainedObject = head;
        headMultiAimConstraint.data.aimAxis = MultiAimConstraintData.Axis.Z;

        WeightedTransformArray headWeightedTransformArray = new WeightedTransformArray();
        headWeightedTransformArray.Add(new WeightedTransform(headTargetObject.transform, 1.0f));
        headMultiAimConstraint.data.sourceObjects = headWeightedTransformArray;

        headMultiAimConstraint.data.constrainedZAxis = false;

        animManager.AddConstraintWithName("head", headTargetObject);

        if (PrefabUtility.IsPartOfPrefabInstance(rigBuilder)) {
            EditorUtility.SetDirty(rigBuilder);
        }
        if (PrefabUtility.IsPartOfPrefabInstance(animManager)) {
            EditorUtility.SetDirty(animManager);
        }
    }

    private GameObject SetUpLimbConstraint(string limbType, GameObject ikParentGameObject, Transform[] limbTransforms) {
        // Set up foot ik constraint
        GameObject ikGameObject = new GameObject($"{limbType}_ik");
        Undo.RegisterCreatedObjectUndo(ikGameObject, $"{limbType}_ik");
        ikGameObject.transform.SetParent(ikParentGameObject.transform);
        ikGameObject.transform.position = limbTransforms[2].position;
        ikGameObject.transform.rotation = limbTransforms[2].rotation;
        // Set up 'hint' ik marker
        GameObject limbIKHintGameObject = new GameObject($"{limbType}_ik_hint");
        Undo.RegisterCreatedObjectUndo(limbIKHintGameObject, $"{limbType}_ik_hint");
        limbIKHintGameObject.transform.SetParent(ikGameObject.transform);
        // Set initial 'hint' ik marker position somewhat abritrarily at knee
        limbIKHintGameObject.transform.position = limbTransforms[1].position;
        limbIKHintGameObject.transform.rotation = Quaternion.identity;
        // Set up IK Constraint component
        TwoBoneIKConstraint limbConstraint = Undo.AddComponent<TwoBoneIKConstraint>(ikGameObject);
        limbConstraint.data.root = limbTransforms[0];
        limbConstraint.data.mid = limbTransforms[1];
        limbConstraint.data.tip = limbTransforms[2];
        limbConstraint.data.target = ikGameObject.transform;
        limbConstraint.data.hint = limbIKHintGameObject.transform;
        limbConstraint.data.hintWeight = 0.5f;

        return ikGameObject;
    }

    // Extracted from the main Animation Rigging package
    public static void BoneRendererSetup(Transform transform) {
        var boneRenderer = transform.GetComponent<BoneRenderer>();
        if (boneRenderer == null)
            boneRenderer = Undo.AddComponent<BoneRenderer>(transform.gameObject);
        else
            Undo.RecordObject(boneRenderer, "Bone renderer setup.");

        var animator = transform.GetComponent<Animator>();
        var renderers = transform.GetComponentsInChildren<SkinnedMeshRenderer>();
        var bones = new List<Transform>();
        if (animator != null && renderers != null && renderers.Length > 0) {
            for (int i = 0; i < renderers.Length; ++i) {
                var renderer = renderers[i];
                for (int j = 0; j < renderer.bones.Length; ++j) {
                    var bone = renderer.bones[j];
                    if (!bones.Contains(bone)) {
                        bones.Add(bone);

                        for (int k = 0; k < bone.childCount; k++) {
                            if (!bones.Contains(bone.GetChild(k)))
                                bones.Add(bone.GetChild(k));
                        }
                    }
                }
            }
        }
        else {
            bones.AddRange(transform.GetComponentsInChildren<Transform>());
        }

        boneRenderer.transforms = bones.ToArray();

        if (PrefabUtility.IsPartOfPrefabInstance(boneRenderer))
            EditorUtility.SetDirty(boneRenderer);
    }

    private RuntimeAnimatorController CreateStateMachineController(PCAAnimManager animManager)
    {

        // Creates the controller
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath($"Assets/PCA/Animators/PCA_{parent.name}_StateMachine.controller");

        // Add parameters
        controller.AddParameter("Move", AnimatorControllerParameterType.Bool);

        // Add StateMachines
        AnimatorStateMachine rootStateMachine = controller.layers[0].stateMachine;

        AnimatorState[] defaultStateMachineStates = new AnimatorState[2];

        // Add States
        AnimatorState idleState = rootStateMachine.AddState("Idle");
        AnimatorState runState = rootStateMachine.AddState("Run");

        PCAStateBehavior idleStateBehavior = (PCAStateBehavior)idleState.AddStateMachineBehaviour(typeof(PCAStateBehavior));
        idleStateBehavior.SetAttatchedStateMachineState(idleState);

        PCAStateBehavior runStateBehavior = (PCAStateBehavior)runState.AddStateMachineBehaviour(typeof(PCAStateBehavior));
        runStateBehavior.SetAttatchedStateMachineState(runState);

        AnimatorStateTransition toRunTransition = idleState.AddTransition(runState);
        toRunTransition.AddCondition(AnimatorConditionMode.If, 1, "Move");
        toRunTransition.duration = 0;

        AnimatorStateTransition stopRunTransition = runState.AddTransition(idleState);
        stopRunTransition.AddCondition(AnimatorConditionMode.IfNot, 1, "Move");
        stopRunTransition.duration = 0;

        animManager.SetStateMachineController(controller);
        defaultStateMachineStates[0] = idleState;
        defaultStateMachineStates[1] = runState;
        animManager.SetDefaultStateMachineStates(defaultStateMachineStates);

        return controller;
    }
}
