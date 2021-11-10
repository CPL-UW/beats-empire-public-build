using UnityEngine;

public class AnyStateCanceler : StateMachineBehaviour {
	public string parameter;

	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.SetBool(parameter, false);
	}	
}
