using Unity.EditorCoroutines.Editor;
using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using UnityEngine.UIElements;
using System.Collections;
using UnityEditor.UIElements;
using System.IO;
using System.Collections.Generic;

public class ImagesDownloaderWindow : EditorWindow
{
	#region Variables

	private static VisualTreeAsset _uxml = null;

	private static EditorCoroutine _downloadCoroutine = null;

	private Button _downloadButton = null;
	private Button _saveButton = null;
	private TextField _urlField = null;
	private ObjectField _textureField = null;

	private ProgressBar _progressBar = null;
	private Label _progressBarLabel = null;
	private VisualElement _progressBarProgression = null;

	private Texture2D _downloadedTexture = null;

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

			window.Initialize();
		}
		else
		{
			Debug.LogError(nameof(ImagesDownloaderWindow) + ": Init() failed to load uxml, maybe the path changed.");
		}

		window.Show();
	}

	#endregion

	#region Initialize / OnDestroy

	public void Initialize()
	{
		// Note (rémi): Retrieve UIElements
		_urlField = rootVisualElement.Q<TextField>("Url");
		_downloadButton = rootVisualElement.Q<Button>("Download");
		_progressBar = rootVisualElement.Q<ProgressBar>("DownloadProgression");
		_textureField = rootVisualElement.Q<ObjectField>("Result");
		_saveButton = rootVisualElement.Q<Button>("Save");

		// Note (rémi): Retrieve UIElements children
		_progressBarLabel = _progressBar.Q<Label>();
		// Remarks (rémi): remove the dot at beginning when search using class name
		_progressBarProgression = _progressBar.Q<VisualElement>(className: "unity-progress-bar__progress");

		// Note (rémi): Link UIElements
		_downloadButton.clicked += OnDownloadButtonClicked;
		_saveButton.clicked += OnSaveButtonClicked;
	}

	private void OnDestroy()
	{
		 /// ToDo (remi): Dispose request and coroutine
	}

	private void OnDownloadButtonClicked()
	{
		_downloadCoroutine = EditorCoroutineUtility.StartCoroutine(DownloadTexture(_urlField.text), this);
	}

	private IEnumerator	DownloadTexture(string url)
	{
		_downloadButton.SetEnabled(false);

		_progressBar.style.display = DisplayStyle.Flex;
		_progressBarLabel.style.color = Color.black;
		_progressBarProgression.style.unityBackgroundImageTintColor = Color.blue;

		_textureField.style.display = DisplayStyle.None;
		_saveButton.style.display = DisplayStyle.None;

		using (UnityWebRequest textureRequest = UnityWebRequestTexture.GetTexture(url))
		{
			UnityWebRequestAsyncOperation operation = textureRequest.SendWebRequest();

			while (!textureRequest.isDone)
			{
				yield return null;

				_progressBar.SetValueWithoutNotify(operation.progress);
				_progressBar.title = _progressBar.value.ToString() + "%";
			}

			if (textureRequest.isHttpError == false && textureRequest.isNetworkError == false)
			{
				_downloadedTexture = DownloadHandlerTexture.GetContent(textureRequest);

				_textureField.style.display = DisplayStyle.Flex;
				_saveButton.style.display = DisplayStyle.Flex;

				_progressBar.title = "Done";
			}
			else
			{
				_downloadedTexture = null;

				_progressBarLabel.style.color = Color.white;
				_progressBarProgression.style.unityBackgroundImageTintColor = Color.red;


				_progressBar.SetValueWithoutNotify(100f);
				_progressBar.title = "Error";
			}
		}

		_downloadButton.SetEnabled(true);
	}

	private void OnSaveButtonClicked()
	{
		string path = EditorUtility.SaveFilePanelInProject("Save Texture", "Image", "png", "Please enter a file name to save the texture to");

		if (string.IsNullOrEmpty(path) == false)
		{
			byte[] pngData = _downloadedTexture.EncodeToPNG();
			if (pngData != null)
			{
				File.WriteAllBytes(path, pngData);

				// As we are saving to the asset folder, tell Unity to scan for modified or new assets
				AssetDatabase.Refresh();
			}
		}
	}

	#endregion
}
