using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class StringPCAKeyframeDataDictionary : SerializableDictionary<string, PCAKeyframeTransformData> { }

[Serializable]
public class PCAKeyframe  {
	[SerializeField]
	private string keyframeName = null;
	[SerializeField]
	private List<PCAKeyframeTransformData> allTransformData = null;
	[SerializeField]
	private StringPCAKeyframeDataDictionary transformDataByName = null;

	public PCAKeyframe(string keyframeName, List<PCAKeyframeTransformData> allTransformData) {
		this.keyframeName = keyframeName;
		this.allTransformData = allTransformData;

		InitTransformDataDict();
	}

	public PCAKeyframe(List<PCAKeyframeTransformData> allTransformData) {
		this.keyframeName = "none";
		this.allTransformData = allTransformData;

		InitTransformDataDict();
	}

	private void InitTransformDataDict() {
		transformDataByName = new StringPCAKeyframeDataDictionary();
		foreach (PCAKeyframeTransformData transformData in allTransformData) {
			transformDataByName.Add(transformData.GetTransformName(), transformData);
		}
	}

	public void SetTransformData(List<PCAKeyframeTransformData> newTransformData) {
		allTransformData = newTransformData;
		InitTransformDataDict();
	}

	public string GetKeyframeName() {
		return keyframeName;
	}

	public List<PCAKeyframeTransformData> GetAllTransformData() {
		return allTransformData;
    }

	public PCAKeyframeTransformData GetDataForTransformName(string transformName) {
		if (transformDataByName.ContainsKey(transformName)) {
			return transformDataByName[transformName];
		}
		return null;
	}

	public PCAKeyframe GetDeepCopy() {
		List<PCAKeyframeTransformData> newKeyframeTransformData = new List<PCAKeyframeTransformData>();
		foreach (PCAKeyframeTransformData transformData in allTransformData) {
			newKeyframeTransformData.Add(transformData.GetDeepCopy());
		}
		return new PCAKeyframe(keyframeName, newKeyframeTransformData);
	}
}
