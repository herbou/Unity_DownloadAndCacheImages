using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class Test : MonoBehaviour
{
	public Image imageUi;
	public string imageUrl;

	string davinciCacheDirectory;

	static bool isFirstTimeRun = true;

	void Start ()
	{
		davinciCacheDirectory = Application.persistentDataPath + "/davinci/";

		if (isFirstTimeRun) {
			ClearCacheURL (imageUrl);
			isFirstTimeRun = false;
		}

		Davinci
		.get ()
		.load (imageUrl)
		.into (imageUi)
		.setCached (true)
		.setFadeTime (0)
		.withErrorAction (error => Debug.Log (error))
		.start ();
	}

	void ClearCacheURL (string url)
	{
		string hashedUrl = Davinci.CreateMD5 (url);

		if (File.Exists (davinciCacheDirectory + hashedUrl))
			File.Delete (davinciCacheDirectory + hashedUrl);
	}

	void ClearCacheAll ()
	{
		if (Directory.Exists (davinciCacheDirectory))
			Directory.Delete (davinciCacheDirectory, true);
	}

	bool HasInternetConnection ()
	{
		return Application.internetReachability != NetworkReachability.NotReachable;
	}
}
