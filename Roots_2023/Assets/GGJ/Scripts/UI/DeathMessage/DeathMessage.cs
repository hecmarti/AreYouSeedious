using DG.Tweening;
using GGJ.Levels;
using MoreMountains.CorgiEngine;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DeathMessage : MonoBehaviour, MMEventListener<CorgiEngineEvent>
{
    [SerializeField]
    private AudioSource audioSource;

    [SerializeField]
    private List<AudioClip> deathAudioClips;

    [SerializeField]
    private TextMeshProUGUI deathText;

    [SerializeField]
    private List<string> phrases;

    [SerializeField]
    private float fadeDuration = 0.3f;

    [SerializeField]
    private float textTime = 2f;

    [SerializeField]
    private int minRandomDeaths = 5;
    [SerializeField]
    private int maxRandomDeaths = 10;

    [SerializeField]
    [Range(0f, 1f)]
    private float audioProbability;

    private int deathCountTarget;
    private int deathCount = 0;

    private void Awake()
    {
        this.MMEventStartListening<CorgiEngineEvent>();
        SetNewDeathTarget();
    }

    private void Start()
    {
        deathText.DOFade(0f, 0f);
    }

    private void SetNewDeathTarget()
    {
        deathCount = 0;
        deathCountTarget = Random.Range(minRandomDeaths, maxRandomDeaths);
    }

    public void OnMMEvent(CorgiEngineEvent eventType)
    {
        if (eventType.EventType != CorgiEngineEventTypes.PlayerDeath) return;

        deathCount++;

        if (deathCount < deathCountTarget)
        {
            return;
        }

        SetNewDeathTarget();

        if (Random.Range(0f, 1f) <= audioProbability)
        {
            AudioMessage();
        }
        else
        {
            TextMessage();
        }
    }

    private void TextMessage()
    {
        int randomIndex = Random.Range(0, phrases.Count);
        deathText.text = phrases[randomIndex];

        var sequence = DG.Tweening.DOTween.Sequence();

        sequence.Append(deathText.DOFade(1f, fadeDuration));
        sequence.Append(deathText.DOFade(0f, fadeDuration).SetDelay(textTime));
    }

    private void AudioMessage()
    {
        int randomIndex = Random.Range(0, deathAudioClips.Count);
        audioSource.clip = deathAudioClips[randomIndex];
        audioSource.Play();
    }

    IEnumerable ShowMessage()
    {
        

        deathText.enabled = true;
        

        yield return new WaitForSeconds(0.5f);
        deathText.enabled = false;
    }
}
