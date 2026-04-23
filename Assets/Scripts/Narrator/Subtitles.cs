using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace SSPot
{
    public class Subtitles : MonoBehaviour
    {
        [SerializeField] TMPro.TextMeshProUGUI subtitleText = default;
        private Coroutine typingCoroutine;


        public void DisplaySubtitle(string text, float audioDuration)
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            
            string processedText = FormatText(text);
            float calculatedSpeed = (audioDuration > 0.9f) ? (audioDuration - 0.8f) / processedText.Length : 0.03f;

            typingCoroutine = StartCoroutine(TypeText(processedText, calculatedSpeed));
        }

        private string FormatText(string text)
        {
			string formatted = text.Replace(". ", ".\n")
                           .Replace("! ", "!\n")
                           .Replace("? ", "?\n");
            string[] rawSentences = formatted.Split('\n');
            
            List<string> groupedLines = new List<string>();
            string currentBuffer = "";

            foreach (string sentence in rawSentences)
            {
                string s = sentence.Trim();
                if (string.IsNullOrEmpty(s)) continue;

                if (s.Length > 120)
                {
                    if (!string.IsNullOrEmpty(currentBuffer))
                    {
                        groupedLines.Add(currentBuffer);
                        currentBuffer = "";
                    }

                    string longSentence = s;
                    while (longSentence.Length > 120)
                    {
                        int splitIndex = longSentence.LastIndexOf(' ', 120);
                        if (splitIndex == -1) splitIndex = 120;

                        groupedLines.Add(longSentence.Substring(0, splitIndex).Trim());
                        longSentence = longSentence.Substring(splitIndex).Trim();
                    }
                    currentBuffer = longSentence;
                }
                else
                {
                    string separator = string.IsNullOrEmpty(currentBuffer) ? "" : " ";
                    if (currentBuffer.Length + separator.Length + s.Length <= 120)
                    {
                        currentBuffer += separator + s;
                    }
                    else
                    {
                        groupedLines.Add(currentBuffer);
                        currentBuffer = s;
                    }
                }
            }

            if (!string.IsNullOrEmpty(currentBuffer))
                groupedLines.Add(currentBuffer);

            return string.Join("\n", groupedLines);
        }

        private IEnumerator TypeText(string text, float speed)
        {
            subtitleText.text = "";
            foreach (char letter in text.ToCharArray())
            {
                subtitleText.text += letter;
				if(letter == '\n'){
					yield return new WaitForSeconds(0.5f);
					subtitleText.text = "";
				}
				else
				{
					yield return new WaitForSeconds(speed);
				}
            }
        }

        public void ClearSubtitle()
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            subtitleText.text = "";
        }
	}
}
