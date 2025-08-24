using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

public class PCAStateBehavior : StateMachineBehaviour
{
    [SerializeField]
    private AnimatorState attatchedStateMachineState;

    public void SetAttatchedStateMachineState(AnimatorState attatchedStateMachineState) {
        this.attatchedStateMachineState = attatchedStateMachineState;
    }

    //given animation state data to animate somehow
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //base.OnStateEnter(animator, stateInfo, layerIndex);
        PCAAnimManager animManager = animator.GetComponentInParent<PCAAnimManager>();
        if (animManager != null) {
            animManager.PlayAnim(attatchedStateMachineState);
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //base.OnStateExit(animator, stateInfo, layerIndex);
        //Debug.Log("Exit");
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //base.OnStateUpdate(animator, stateInfo, layerIndex);
        //Debug.Log("Enter");
    }

    public override void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //base.OnStateMove(animator, stateInfo, layerIndex);
        //Debug.Log("Move");
    }
}
