using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PCAAnimFrameData
{
    public string id;
    public PCAKeyframe frame;
    public AnimationCurve curve;
    public float speed;

    public PCAAnimFrameData(string id, PCAKeyframe frame)
    {
        this.id = id;
        this.frame = frame;
        curve = AnimationCurve.Linear(0,0,1,1);
        speed = 6;
    }

    public void SetKeyFrame(PCAKeyframe newFrame)
    {
        frame = newFrame;
    }

    public PCAAnimFrameData GetDeepCopy() {
        PCAAnimFrameData frameDataCopy = new PCAAnimFrameData(id, frame.GetDeepCopy());
        Keyframe[] newCurveKeyframes = new List<Keyframe>(curve.keys).ToArray();
        frameDataCopy.curve = new AnimationCurve(newCurveKeyframes);
        frameDataCopy.speed = speed;

        return frameDataCopy;
    }
}
