using UnityEngine;

public class TutorialOn : StateMachineBehaviour {
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		HelpGuyController helper = animator.transform.parent.gameObject.GetComponent<HelpGuyController>();
		if (animator.GetInteger("Action") == 1)
		{
			helper.Speak();
		}
		else if (animator.GetInteger("Action") == 2)
		{
			helper.Confirm();
		}
	}	
}
