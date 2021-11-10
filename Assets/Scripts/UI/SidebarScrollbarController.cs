using System.Collections;
using UnityEngine;

public class SidebarScrollbarController : MonoBehaviour
{
	public GameObject upButton;
	public GameObject downButton;
  
	void OnEnable() {
		upButton.SetActive(true);
		downButton.SetActive(true);
	}

	void OnDisable() {
		upButton.SetActive(false);
		downButton.SetActive(false);
	}
}
