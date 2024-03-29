using System;
using System.Collections;
using HarmonyLib;
using UnityEngine;
using TMPro;
using System.Text;

namespace SignalTranslatorAligner;

[HarmonyPatch(typeof(HUDManager), "DisplaySignalTranslatorMessage")]
class DisplaySignalTranslatorMessage
{
    static void Prefix(
        ref bool __runOriginal,
        ref IEnumerator __result,
        Animator ___signalTranslatorAnimator, AudioSource ___UIAudio, TextMeshProUGUI ___signalTranslatorText,
        string signalMessage, int seed, SignalTranslator signalTranslator
    )
    {
        // Skip original
        __runOriginal = false;
        Plugin.Instance.LogInfo("DisplaySignalTranslatorMessage hooked");

        IEnumerator ReplacedRoutine()
        {
            System.Random signalMessageRandom = new System.Random(seed + StartOfRound.Instance.randomMapSeed);
            ___signalTranslatorAnimator.SetBool("transmitting", value: true);
            signalTranslator.localAudio.Play();
            ___UIAudio.PlayOneShot(signalTranslator.startTransmissionSFX, 1f);

            string trimedMessage = signalMessage.Trim();

            ___signalTranslatorText.text = $"<#00000000>{trimedMessage}</color>";
            yield return new WaitForSeconds(1.21f);
            for (int i = 0; i <= trimedMessage.Length; i++)
            {
                if (signalTranslator == null)
                {
                    break;
                }
                if (!signalTranslator.gameObject.activeSelf)
                {
                    break;
                }
                ___UIAudio.PlayOneShot(signalTranslator.typeTextClips[UnityEngine.Random.Range(0, signalTranslator.typeTextClips.Length)]);
                string text = $"{trimedMessage.Substring(0, i)}<#00000000>{trimedMessage.Substring(i, trimedMessage.Length - i)}</color>";
                ___signalTranslatorText.text = text;
                float num = Mathf.Min((float)signalMessageRandom.Next(-1, 4) * 0.5f, 0f);
                yield return new WaitForSeconds(0.7f + num);
            }
            if (signalTranslator != null)
            {
                ___UIAudio.PlayOneShot(signalTranslator.finishTypingSFX);
                signalTranslator.localAudio.Stop();
            }
            yield return new WaitForSeconds(0.5f);
            ___signalTranslatorAnimator.SetBool("transmitting", value: false);
        }

        __result = ReplacedRoutine();
    }
}

[HarmonyPatch(typeof(HUDManager), "Awake")]
class AwakePatch
{
    static void Postfix(
        TextMeshProUGUI ___signalTranslatorText
    )
    {
        // Align "RECEIVING SIGNAL"
        Transform container = ___signalTranslatorText.transform.GetParent();
        RectTransform text1rt = (RectTransform)container.GetChild(1);
        RectTransform text2rt = (RectTransform)container.GetChild(2);

        text1rt.anchorMin = new Vector2(0f, 0.5f);
        text1rt.anchorMax = new Vector2(1f, 0.5f);
        text1rt.sizeDelta = new Vector2(0f, text1rt.sizeDelta.y);
        var tmpu1 = text1rt.GetComponent<TextMeshProUGUI>();
        tmpu1.margin = new Vector4(0, 0, text1rt.anchoredPosition.x * 2, 0);
        tmpu1.alignment = TextAlignmentOptions.Center;

        text2rt.anchorMin = new Vector2(0f, 0.5f);
        text2rt.anchorMax = new Vector2(1f, 0.5f);
        text2rt.sizeDelta = new Vector2(0f, text2rt.sizeDelta.y);
        var tmpu2 = text2rt.GetComponent<TextMeshProUGUI>();
        tmpu2.margin = new Vector4(0, 0, text2rt.anchoredPosition.x * 2, 0);
        tmpu2.alignment = TextAlignmentOptions.Center;

        // Align signal message
        ___signalTranslatorText.alignment = TextAlignmentOptions.Center;
        ___signalTranslatorText.rectTransform.anchorMin = new Vector2(0, 0.5f);
        ___signalTranslatorText.rectTransform.anchorMax = new Vector2(1f, 0.5f);
        ___signalTranslatorText.rectTransform.anchoredPosition = new Vector2(0, ___signalTranslatorText.rectTransform.anchoredPosition.y);
        ___signalTranslatorText.rectTransform.sizeDelta = new Vector2(0, ___signalTranslatorText.rectTransform.sizeDelta.y);
    }
}