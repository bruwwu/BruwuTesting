using UnityEngine;
using System;

[Serializable]
/* Simple data holder class for position and rotation for a bone within a keyframe */
public class PCAKeyframeTransformData {
	[SerializeField]
	private string transformName;
	[SerializeField]
	private Vector3 pos;
	[SerializeField]
	private Vector3 rot;

	public PCAKeyframeTransformData(string transformName, Vector3 pos, Vector3 rot) {
		this.transformName = transformName;
		this.pos = pos;
		this.rot = rot;
	}

	public string GetTransformName() {
		return transformName;
	}

	public Vector3 GetPos() {
		return pos;
	}

	public Vector3 GetRot() {
		return rot;
	}

	public PCAKeyframeTransformData GetDeepCopy() {
		return new PCAKeyframeTransformData(transformName, pos, rot);
	}
}
