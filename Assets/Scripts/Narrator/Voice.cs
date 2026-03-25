using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SSPot;
using UnityEngine.Localization.Settings;
using System.Threading.Tasks;

public class Voice : MonoBehaviour
{
	[SerializeField] private AudioSource source;
	private RectTransform[] audioIndicatorBars;

	[SerializeField] private GameObject subtitleBox;
	[SerializeField] private GameObject audioIndicator;
	[SerializeField] private Subtitles subtitles;

	public static Voice instance { get; private set; }

	private Queue<(AudioObject[] clips, TaskCompletionSource<bool> tcs)> narrationQueue = 
    new Queue<(AudioObject[] clips, TaskCompletionSource<bool> tcs)>();
	private bool isPlaying = false;

	public delegate void NarrationRequestHandler(AudioObject[] clips, bool interrupt, TaskCompletionSource<bool> tcs);
	public static event NarrationRequestHandler OnNarrationRequested;

	private bool enableNarrator = true;
	private AudioObject[] lastRequest;
	private (AudioObject[] clips, TaskCompletionSource<bool> tcs) currentActiveJob;


	public void EnableNarrator()
	{
		Speak(lastRequest);
		enableNarrator = true;
	}

	public bool IsSpeakig()
	{
		return narrationQueue.Count == 0;
	}

	public void DisableNarrator()
	{
		StopSpeaking();
		enableNarrator = false;
	}

	private void Awake()
	{
		if (instance != null && instance != this)
		{
			Destroy(gameObject);
			return;
		}
		instance = this;
	}

	private void Start()
	{
		if (audioIndicator.GetComponentInChildren<RectTransform>() == null)
		{
			Debug.LogError("No RectTransform found in Audio Indicator");
			return;
		}
		else
		{
			audioIndicatorBars = audioIndicator.GetComponentsInChildren<RectTransform>();
			audioIndicatorBars = audioIndicatorBars[1..];

			RectTransform[] audioIndicatorBarsTemp = new RectTransform[audioIndicatorBars.Length];

			int left = 0;
			int right = 0;

			for(int i = audioIndicatorBars.Length - 1; i >= 0; i--) 
			{ 
				if (left <= right)
				{
					audioIndicatorBarsTemp[left] = audioIndicatorBars[i];
					left++;
				}
				else
				{
					audioIndicatorBarsTemp[audioIndicatorBars.Length - right - 1] = audioIndicatorBars[i];
					right++;
				}
			}

			for (int j = 0; j < audioIndicatorBars.Length; j++)
			{
				audioIndicatorBars[j] = audioIndicatorBarsTemp[j];
				print(audioIndicatorBars[j].name);
			}
		}
	}

	private void Update()
	{
		int sampleSize = 64;
		for (int j = 0; j < audioIndicatorBars.Length; j++)
		{
			float[] spectrumData = new float[sampleSize * (int) Mathf.Pow(2, j/2)];
			source.GetSpectrumData(spectrumData, 0, FFTWindow.BlackmanHarris);

			float maxAmplitude = 0;
			for (int i = 0; i < sampleSize; i++)
			{
				if (spectrumData[i] > maxAmplitude)
				{
					maxAmplitude = spectrumData[i];
				}
			}

			audioIndicatorBars[j].sizeDelta = new Vector2(audioIndicatorBars[j].sizeDelta.x, Mathf.Lerp(audioIndicatorBars[j].sizeDelta.y, maxAmplitude * 500, Time.deltaTime * 10));
		}
	}

	private void OnEnable()
	{
		OnNarrationRequested += HandleNarrationRequest;
	}

	private void OnDisable()
	{
		OnNarrationRequested -= HandleNarrationRequest;
	}

	private void StopSpeaking()
	{
		narrationQueue.Clear();
		source.Stop();
		StopAllCoroutines();
		subtitles.ClearSubtitle();
		subtitleBox.SetActive(false);
		isPlaying = false;
	}

	private void HandleNarrationRequest(AudioObject[] clips, bool interrupt, TaskCompletionSource<bool> tcs)
	{
		if (interrupt)
		{
			currentActiveJob.tcs?.TrySetResult(false);

			while (narrationQueue.Count > 0)
			{
				var pending = narrationQueue.Dequeue();
				pending.tcs.TrySetResult(false); // Avisa que foi cancelado
			}

			if (isPlaying)
			{
				source.Stop();
				StopAllCoroutines();
				subtitles.ClearSubtitle();
				isPlaying = false;
			}
		}

		narrationQueue.Enqueue((clips, tcs));

		if (!isPlaying)
		{
			StartCoroutine(ProcessNarrationQueue());
		}
	}

	private IEnumerator ProcessNarrationQueue()
	{
		isPlaying = true;
		while (narrationQueue.Count > 0)
		{
			subtitleBox.SetActive(true);
			currentActiveJob = narrationQueue.Dequeue();

			foreach (AudioObject clip in currentActiveJob.clips)
			{			
				AudioClip clipToPlay = (LocalizationSettings.SelectedLocale.Identifier.Code == "pt-BR") 
					? clip.clipPTBR : clip.clipENUS;
				string subToDisplay = (LocalizationSettings.SelectedLocale.Identifier.Code == "pt-BR") 
					? clip.subtitlePTBR : clip.subtitleENUS;

				source.clip = clipToPlay;
				source.PlayOneShot(clipToPlay);
				subtitles.DisplaySubtitle(subToDisplay);
				
				yield return new WaitForSeconds(clipToPlay.length);
				subtitles.ClearSubtitle();
			}

			currentActiveJob.tcs.TrySetResult(true); 
        	currentActiveJob = (null, null);
		}
		isPlaying = false;
		subtitleBox.SetActive(false);
	}

	public async Task Speak(AudioObject[] clips)
	{
		lastRequest = clips;
		if (!enableNarrator) return;

		var tcs = new TaskCompletionSource<bool>();
		
		OnNarrationRequested?.Invoke(clips, true, tcs);

		await tcs.Task;
	}
}