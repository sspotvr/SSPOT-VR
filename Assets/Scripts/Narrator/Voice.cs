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

	// Configurações das tabelas do Unity Localization
    private const string SUBTITLE_TABLE = "SubtitlesTable";
    private const string AUDIO_TABLE = "AudioTable";

	// Processamento de pedidos de narração
	public static Voice instance { get; private set; }
	private Queue<(string[] keys, TaskCompletionSource<bool> tcs)> narrationQueue = 
    new Queue<(string[] keys, TaskCompletionSource<bool> tcs)>();
	private bool isPlaying = false;
	public delegate void NarrationRequestHandler(string[] keys, bool interrupt, TaskCompletionSource<bool> tcs);
    public static event NarrationRequestHandler OnNarrationRequested;
	private bool enableNarrator = true;
	private string[] lastRequest;
    private (string[] keys, TaskCompletionSource<bool> tcs) currentActiveJob;


	public void EnableNarrator()
	{
		if (lastRequest != null && lastRequest.Length > 0)
        {
            _ = Speak(lastRequest);
        }
		enableNarrator = true;
	}

	public bool IsSpeakig()
	{
		return isPlaying || narrationQueue.Count > 0;
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
				// print(audioIndicatorBars[j].name);
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

	private void OnEnable() => OnNarrationRequested += HandleNarrationRequest;
    private void OnDisable() => OnNarrationRequested -= HandleNarrationRequest;

	public void StopSpeaking()
	{
		narrationQueue.Clear();
		source.Stop();
		StopAllCoroutines();
		subtitles.ClearSubtitle();
		subtitleBox.SetActive(false);
		isPlaying = false;
	}

	private void HandleNarrationRequest(string[] keys, bool interrupt, TaskCompletionSource<bool> tcs)
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
				StopSpeaking();
			}
		}

		narrationQueue.Enqueue((keys, tcs));

		if (!isPlaying)
		{
			StartCoroutine(ProcessNarrationQueue());
		}
	}

	private IEnumerator ProcessNarrationQueue()
    {
        isPlaying = true;
        subtitleBox.SetActive(true);

        while (narrationQueue.Count > 0)
        {
            currentActiveJob = narrationQueue.Dequeue();

            foreach (string key in currentActiveJob.keys)
            {
                // Carregamento Assíncrono do Unity Localization
                var stringOperation = LocalizationSettings.StringDatabase.GetLocalizedStringAsync(SUBTITLE_TABLE, key);
                var audioOperation = LocalizationSettings.AssetDatabase.GetLocalizedAssetAsync<AudioClip>(AUDIO_TABLE, key);

                // Espera as duas operações terminarem
                yield return stringOperation;
                yield return audioOperation;

                string subToDisplay = stringOperation.Result;
                AudioClip clipToPlay = audioOperation.Result;

                if (clipToPlay != null && !string.IsNullOrEmpty(subToDisplay))
                {
                    source.clip = clipToPlay;
                    source.PlayOneShot(clipToPlay);
                    subtitles.DisplaySubtitle(subToDisplay, clipToPlay.length);
                    
                    yield return new WaitForSeconds(clipToPlay.length + 0.5f);
                    subtitles.ClearSubtitle();
                }
                else
                {
                    Debug.LogWarning($"Narração faltando para a chave: {key}");
                }
            }

            currentActiveJob.tcs.TrySetResult(true); 
            currentActiveJob = (null, null);
        }
        
        isPlaying = false;
        subtitleBox.SetActive(false);
    }

	public async Task Speak(string[] keys)
    {
        lastRequest = keys;
        if (!enableNarrator) return;

        Debug.Log($"Foram recebidos {keys.Length} clipes para narração!");

        var tcs = new TaskCompletionSource<bool>();
        OnNarrationRequested?.Invoke(keys, true, tcs);
        await tcs.Task;
    }
}