using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

[System.Serializable]
public class PCAAnimState {
    public string id;
    public List<PCAAnimFrameData> frames;
    public bool isLooping;
    public bool isMirrored;
    public AnimatorState stateMachineState;

    public PCAAnimState(string id) {
        this.id = id;
        frames = new List<PCAAnimFrameData>();
        isLooping = false;
        stateMachineState = null;
    }

    public void SetStateMachineState(AnimatorState stateMachineState) {
        this.stateMachineState = stateMachineState;
    }

    public void AddFrame(string frameName, PCAKeyframe frameKeyframe) {
        frames.Add(new PCAAnimFrameData(frameName, frameKeyframe));
    }

    public int GetFrameCount() {
        return frames.Count;
	}

    public PCAAnimState GetDeepCopy() {
        PCAAnimState newAnimState = new PCAAnimState(id);
        newAnimState.frames = new List<PCAAnimFrameData>();
        foreach (PCAAnimFrameData frame in frames) {
            newAnimState.frames.Add(frame.GetDeepCopy());
        }
        newAnimState.isLooping = isLooping;
        newAnimState.isMirrored = isMirrored;

        newAnimState.stateMachineState = null;

        return newAnimState;
    }
}
