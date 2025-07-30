using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AmbienceManager : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Determine to enable ambience or not")]
    [SerializeField] private bool enable = true;

    [Space(5f)]

    [Tooltip("Determine to random delay between 5-10 or not")]
    [SerializeField] private bool randomDelay = false;
    [ShowIf("randomDelay", false, DisableType.Hide)]
    [Tooltip("Delay before start playing another SFX")]
    [SerializeField] private float delay = 5f;

    [Space(10f)]

    [SerializeField] private List<Ambience> ambiences = new List<Ambience>(); // List of all ambiences.

    private float currentDelay;

    private AudioSource audioSource;

    private void Start()
    {
        // Check if there's no audio source in this object, disable it.
        if (!TryGetComponent(out audioSource))
        {
            Debug.LogWarning("AmbienceManager in '" + name + "' doesn't have Audio Source! Try implement it first, then try again.");

            enable = false;
            return;
        }
    }

    private void Update()
    {
        // If it's not enable or has no ambience, return.
        if (!enable || ambiences.Count == 0)
        {
            return;
        }

        currentDelay -= Time.deltaTime;
        currentDelay = Mathf.Clamp(currentDelay, 0f, 10f);

        if (currentDelay <= 0f)
        {
            PlayAmbience();
            currentDelay = randomDelay ? Random.Range(5, 10) : delay;
        }
    }

    // Function to play ambience sound.
    private void PlayAmbience()
    {
        AudioManager audioManager = AudioManager.instance;
        Ambience selectedAmbience = GetRandomAmbience();

        Audio audio = audioManager.GetAudio(selectedAmbience.name);

        if (audio != null)
        {
            AudioManager.instance.SetVolumeSFX(ref audioSource, audio);
            audioSource.clip = audio.clip;
            audioSource.PlayOneShot(audio.clip);
        }
        else
        {
            Debug.LogWarning("There's no audio with name '" + selectedAmbience.name + "'! Unable to play ambience sound for '" + name + "'");
        }
    }

    // Function to get random ambience by frequency (weight).
    private Ambience GetRandomAmbience()
    {
        float[] weights = new float[ambiences.Count];

        for (int i = 0; i < weights.Length; i++)
        {
            weights[i] = ambiences[i].frequency;
        }

        float randomWeight = Random.Range(0f, weights.Sum());

        for (int i = 0; i < ambiences.Count; i++)
        {
            randomWeight -= weights[i];

            if (randomWeight < 0f)
            {
                return ambiences[i];
            }
        }

        return ambiences[0];
    }
}

[System.Serializable]
public struct Ambience
{
    [Tooltip("Name of SFX to play ambience")]
    public string name;
    [Tooltip("Weight to use this SFX")]
    [Range(0.1f, 1f)]
    public float frequency;
}