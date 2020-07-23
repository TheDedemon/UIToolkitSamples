using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

public class ImagesDownloaderWindow : EditorWindow
{
	#region Variables

	private static VisualTreeAsset _uxml = null;

	#endregion

	#region Init

	[MenuItem("Window/Samples/ImagesDownloader")]
	private static void Init()
	{
		ImagesDownloaderWindow window = EditorWindow.GetWindow<ImagesDownloaderWindow>("Images Downloader");

		// Note (remi): Check if null to avoid loading it every times
		if (_uxml == null)
		{
			_uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Art/UI/ImagesDownloader/ImagesDownloaderUI.uxml");
		}
		if (_uxml != null)
		{
			_uxml.CloneTree(window.rootVisualElement);

			LinkToUIElements(window.rootVisualElement);
		}
		else
		{
			Debug.LogError(nameof(ImagesDownloaderWindow) + ": Init() failed to load uxml, maybe the path changed.");
		}

		window.Show();
	}

	private static void LinkToUIElements(VisualElement root)
	{
		TextField urlField = root.Q<TextField>("Url");
		Button downloadButton = root.Q<Button>("Download");

		downloadButton.clicked += () => { Debug.Log("Download button clicked"); };
	}

	#endregion
}
