using System;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
//[CreateAssetMenu(fileName = "Data", menuName = "PCA/PCAAnimStatesPreset", order = 1)]
public class PCAAnimStatesPreset : ScriptableObject {
    [SerializeField]
    public List<PCAAnimState> presetStates = null;
}