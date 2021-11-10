using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameInitializer : MonoBehaviour
{
    public InputField SeedField;
    public string GameSceneName;
	public Animator animator;
	public GameObject logo;
	public GameObject sfx;
	public GameObject music;

    public void StartGame()
    {
        int seed;

        if (int.TryParse(this.SeedField.text, out seed))
        {
			SeedData.seed = seed;
			Random.InitState(seed);
        }

		animator.SetBool("On", false);
    }

	public void OnVisible()
	{
		logo.SetActive(true);
		sfx.SetActive(true);
		music.SetActive(true);
		animator.SetBool("On", true);
	}

	public void LoadNextScene()
	{
		StartCoroutine(CoLoadNextScene());
	}

	public IEnumerator CoLoadNextScene()
	{
		yield return new WaitForSeconds(3.0f);
		SceneManager.LoadScene(this.GameSceneName);
	}

	public void Start()
	{
		logo.SetActive(false);
		StartCoroutine(WaitUntilVisible());
	}

	private IEnumerator WaitUntilVisible()
	{
		yield return Footilities.CoLoadSessionManager();
		yield return new WaitUntil(() => SessionController.S.isVisible);
		OnVisible();
	}
}
