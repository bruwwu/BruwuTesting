using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class PCAGeneratorEditor : EditorWindow {

    public static PCAHumanoidGenerator generator;
    public Transform parent;

    public Transform hip;
    public Transform chest;
    public Transform head;

    public Transform leftUpperArm;
    public Transform leftLowerArm;
    public Transform leftHand;
    public Transform rightUpperArm;
    public Transform rightLowerArm;
    public Transform rightHand;

    public Transform leftUpperLeg;
    public Transform leftLowerLeg;
    public Transform leftFoot;
    public Transform rightUpperLeg;
    public Transform rightLowerLeg;
    public Transform rightFoot;


    [MenuItem("Window/PCA/HumanoidGenerator")]
    static void Init() {
        // Get existing open window or if none, make a new one:
        PCAGeneratorEditor window = (PCAGeneratorEditor)GetWindow(typeof(PCAGeneratorEditor));
        window.Show();
    }
    private void OnGUI() {
        parent = (Transform)EditorGUILayout.ObjectField("Parent", parent, typeof(Transform), true);

        hip = (Transform)EditorGUILayout.ObjectField("Hips", hip, typeof(Transform), true);
        chest = (Transform)EditorGUILayout.ObjectField("Chest", chest, typeof(Transform), true);
        head = (Transform)EditorGUILayout.ObjectField("Head", head, typeof(Transform), true);

        leftUpperArm = (Transform)EditorGUILayout.ObjectField("Left Upper Arm", leftUpperArm, typeof(Transform), true);
        leftLowerArm = (Transform)EditorGUILayout.ObjectField("Left Lower Arm", leftLowerArm, typeof(Transform), true);
        leftHand = (Transform)EditorGUILayout.ObjectField("Left Hand", leftHand, typeof(Transform), true);

        rightUpperArm = (Transform)EditorGUILayout.ObjectField("Right Upper Arm", rightUpperArm, typeof(Transform), true);
        rightLowerArm = (Transform)EditorGUILayout.ObjectField("Right Lower Arm", rightLowerArm, typeof(Transform), true);
        rightHand = (Transform)EditorGUILayout.ObjectField("Right Hand", rightHand, typeof(Transform), true);

        leftUpperLeg = (Transform)EditorGUILayout.ObjectField("Left Upper Leg", leftUpperLeg, typeof(Transform), true);
        leftLowerLeg = (Transform)EditorGUILayout.ObjectField("Left Lower Leg", leftLowerLeg, typeof(Transform), true);
        leftFoot = (Transform)EditorGUILayout.ObjectField("Left Foot", leftFoot, typeof(Transform), true);

        rightUpperLeg = (Transform)EditorGUILayout.ObjectField("Right Upper Leg", rightUpperLeg, typeof(Transform), true);
        rightLowerLeg = (Transform)EditorGUILayout.ObjectField("Right Lower Leg", rightLowerLeg, typeof(Transform), true);
        rightFoot = (Transform)EditorGUILayout.ObjectField("Right Foot", rightFoot, typeof(Transform), true);

        if (GUILayout.Button("Generate")) {
            generator = CreateInstance<PCAHumanoidGenerator>();
            generator.parent = parent;

            generator.hip = hip;
            generator.chest = chest;
            generator.head = head;

            generator.leftUpperArm = leftUpperArm;
            generator.leftLowerArm = leftLowerArm;
            generator.leftHand = leftHand;

            generator.rightUpperArm = rightUpperArm;
            generator.rightLowerArm = rightLowerArm;
            generator.rightHand = rightHand;

            generator.leftFoot = leftFoot;
            generator.leftLowerLeg = leftLowerLeg;
            generator.leftUpperLeg = leftUpperLeg;

            generator.rightFoot = rightFoot;
            generator.rightLowerLeg = rightLowerLeg;
            generator.rightUpperLeg = rightUpperLeg;

            generator.OnGenerate();
        }
    }
}
