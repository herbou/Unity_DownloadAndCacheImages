using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class SceneController : MonoBehaviour
{
	Button sceneButton;

	void Start ()
	{
		sceneButton = GetComponent<Button> ();

		sceneButton.onClick.RemoveAllListeners ();
		sceneButton.onClick.AddListener (OnButtonClick);
	}

	void OnButtonClick ()
	{
		int scene = SceneManager.GetActiveScene ().buildIndex;
		SceneManager.LoadScene ((scene == 0) ? 1 : 0);
	}
}


