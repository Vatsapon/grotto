using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Setting")]
    [Tooltip("Determine to use audio across all scenes or not")]
    [SerializeField] private bool useAcrossScene = true;
    [Tooltip("Speed to fade music")]
    [SerializeField] private float fadeMusicSpeed = 5f;

    [Space(5f)]
    [Tooltip("List of all audios")]
    [SerializeField] private List<Audio> audioSFXList;
    [SerializeField] private List<Audio> audioBGMList;

    [Header("Components")]
    [Tooltip("Audio Source for SFX sounds")]
    [SerializeField] private AudioSource audioSFX;
    [Tooltip("Audio Source for BGM musics")]
    [SerializeField] private AudioSource audioBGM;
    [Tooltip("Slider for SFX sounds volume")]
    [SerializeField] private Slider SFXSlider;
    [Tooltip("Slider for BGM musics volume")]
    [SerializeField] private Slider BGMSlider;
    [Space(5f)]
    [Tooltip("Amount text for SFX sounds volume")]
    [SerializeField] private TextMeshProUGUI SFXAmountText;
    [Tooltip("Amount text for BGM music volume")]
    [SerializeField] private TextMeshProUGUI BGMAmountText;

    private bool fading = false; // Determine if audio is fading or not.

    private void Awake()
    {
        // If Audio Manager already exist, destroy this one.
        if (instance)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        // If it's use across the scene, don't destroy this object when loaded new scene.
        if (useAcrossScene)
        {
            if (transform.root.Equals(transform))
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        // If there's no audio sources for either SFX or BGM or both, then stop the script.
        if (!audioSFX || !audioBGM)
        {
            Debug.LogWarning("There's no audio sources for either SFX or BGM or both. Try references them then try again.");

            this.enabled = false;
            gameObject.SetActive(false);
            return;
        }

        // If there's no audio slider for either SFX or BGM or both, then stop the script.
        if (!SFXSlider || !BGMSlider)
        {
            Debug.LogWarning("There's no audio sliders for either SFX or BGM or both. Using audio volume instead.");
        }

        Setup();
        LoadVolume();

        // Play audio that has playOnStart variable.
        for (int i = 0; i < audioSFXList.Count; i++)
        {
            Audio audio = audioSFXList[i];

            if (audio.playOnStart)
            {
                PlaySound(audio.name);
            }
        }

        for (int i = 0; i < audioBGMList.Count; i++)
        {
            Audio audio = audioBGMList[i];

            if (audio.playOnStart)
            {
                PlayMusic(audio.name, false);
                break;
            }
        }
    }

    private void Update()
    {
        GameManager gameM = GameManager.instance;

        // If there's no audio slider for either SFX or BGM or both, then try find one and assign them.
        if (!SFXSlider && gameM)
        {
            SFXSlider = gameM.component.audioSFXSlider;
        }

        if (!BGMSlider && gameM)
        {
            BGMSlider = gameM.component.audioBGMSlider;
        }

        if (SFXAmountText)
        {
            SFXAmountText.text = ((int)SFXSlider.value).ToString();
        }
        else
        {
            if (gameM)
            {
                SFXAmountText = gameM.component.audioSFXAmount;
            }
        }

        if (BGMAmountText)
        {
            BGMAmountText.text = ((int)BGMSlider.value).ToString();
        }
        else
        {
            if (gameM)
            {
                BGMAmountText = gameM.component.audioBGMAmount;
            }
        }

        for (int i = 0; i < audioSFXList.Count; i++)
        {
            Audio audio = audioSFXList[i];

            if (SFXSlider)
            {
                audio.GetSource().volume = (SFXSlider.value / 100f) * audio.volume;
            }
            else
            {
                audio.GetSource().volume = audio.volume;
            }
            
            audio.GetSource().pitch = audio.pitch;
        }

        if (audioBGM.isPlaying && !fading)
        {
            Audio audio = GetAudio(audioBGM.clip);

            if (BGMSlider)
            {
                audioBGM.volume = (BGMSlider.value / 100f) * audio.volume;
            }
            else
            {
                audioBGM.volume = audio.volume;
            }
            
            audioBGM.pitch = audio.pitch;
        }
    }

    // Function to setup the audios.
    private void Setup()
    {
        for (int i = 0; i < audioSFXList.Count; i++)
        {
            Audio audio = audioSFXList[i];
            GameObject audioObject = new GameObject(audio.name);
            audioObject.transform.SetParent(transform);

            AudioSource audioSource = audioObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.clip = audio.clip;
            audioSource.volume = audio.volume;
            audioSource.pitch = audio.pitch;
            audioSource.loop = audio.loop;

            audio.SetSource(audioSource);
        }
    }

    // Function to load volume from game data.
    public void LoadVolume()
    {
        GameData gameData = GameManager.instance.GetGameData();

        float SFXValue = gameData != null ? gameData.soundVolume : 100f;
        float BGMValue = gameData != null ? gameData.musicVolume : 100f;

        if (SFXSlider)
        {
            SFXSlider.value = SFXValue;
        }
        
        if (BGMSlider)
        {
            BGMSlider.value = BGMValue;
        }

        if (SFXAmountText)
        {
            SFXAmountText.text = ((int)SFXSlider.value).ToString();
        }

        if (BGMAmountText)
        {
            BGMAmountText.text = ((int)BGMSlider.value).ToString();
        }
    }

    // Function to update volume to game data.
    public void VolumeUpdater()
    {
        GameData gameData = GameManager.instance.GetGameData();

        gameData.musicVolume = BGMSlider.value;
        gameData.soundVolume = SFXSlider.value;

        LoadVolume();
    }

    // Function to random play sound by name.
    public void RandomPlaySound(string name)
    {
        Audio audio = GetRandomSound(name);

        if (audio == null)
        {
            Debug.LogWarning("There's no audio with name '" + name + "'");
        }

        PlaySound(audio.name);
    }

    // Function to play sound by name.
    public void PlaySound(string name)
    {
        Audio audio = GetAudio(name);

        if (audio == null)
        {
            Debug.LogWarning("There's no audio with name '" + name + "'");
            return;
        }

        if (!audio.clip)
        {
            Debug.LogWarning("That audio doesn't have any audio clip yet!");
            return;
        }

        if (audio.loop)
        {
            audio.GetSource().Play();
        }
        else
        {
            audio.GetSource().PlayOneShot(audio.clip);
        }
    }

    // Function to stop all current SFX sounds.
    public void StopSound()
    {
        for (int i = 0; i < audioSFXList.Count; i++)
        {
            Audio audio = audioSFXList[i];
            audio.GetSource().Stop();
        }
    }

    // Function to play music by fadeout and fadein music.
    public async void PlayMusic(string name)
    {
        Audio audio = GetAudio(name);

        if (audio == null)
        {
            Debug.LogWarning("There's no audio with name '" + name + "'");
            return;
        }

        fading = true;

        if (audioBGM.isPlaying)
        {
            await FadeOutMusic();
        }

        await FadeInMusic(audio);
        fading = false;
    }

    // Function to play music with option to fadein/out music or not.
    public async void PlayMusic(string name, bool fade)
    {
        Audio audio = GetAudio(name);

        if (audio == null)
        {
            Debug.LogWarning("There's no audio with name '" + name + "'");
            return;
        }

        if (audioBGM.isPlaying)
        {
            fading = true;
            await FadeOutMusic();
        }

        if (fade)
        {
            fading = true;
            await FadeInMusic(audio);
        }
        else
        {
            audioBGM.clip = audio.clip;
            audioBGM.volume = audio.volume;

            if (BGMSlider)
            {
                audioBGM.volume = (BGMSlider.value / 100f) * audio.volume;
            }
            
            audioBGM.pitch = audio.pitch;
            audioBGM.loop = audio.loop;
            audioBGM.Play();
        }

        fading = false;
    }

    // Function to stop current music with fade-out.
    public async void StopMusic()
    {
        await FadeOutMusic();
    }

    // Function to stop current music with option to fade-out or not.
    public async void StopMusic(bool fadeOut)
    {
        if (fadeOut)
        {
            await FadeOutMusic();
        }
        else
        {
            audioBGM.Stop();
        }
    }

    // Function to fade out old music.
    private async Task FadeOutMusic()
    {
        float volume = audioBGM.volume;

        while (volume > 0)
        {
            if (!Application.isPlaying || !audioBGM)
            {
                return;
            }

            volume -= fadeMusicSpeed * Time.deltaTime;
            audioBGM.volume = volume;

            await Task.Yield();
        }

        audioBGM.volume = 0f;
        audioBGM.Stop();
    }

    // Function to fade in music with name.
    private async Task FadeInMusic(Audio audio)
    {
        if (audio == null)
        {
            return;
        }

        audioBGM.volume = 0f;
        audioBGM.clip = audio.clip;
        audioBGM.pitch = audio.pitch;
        audioBGM.loop = audio.loop;

        audioBGM.Play();

        float volume = 0f;
        float maxVolume = audio.volume;

        if (BGMSlider)
        {
            maxVolume = (BGMSlider.value / 100f) * audio.volume;
        }

        while (volume < maxVolume)
        {
            if (!Application.isPlaying || !audioBGM)
            {
                return;
            }

            volume += fadeMusicSpeed * Time.deltaTime;
            audioBGM.volume = volume;

            await Task.Yield();
        }

        audioBGM.volume = maxVolume;
    }

    // Function to set volume for SFX to custom audio source.
    public void SetVolumeSFX(ref AudioSource audioSource, Audio audio)
    {
        if (SFXSlider)
        {
            audioSource.volume = (SFXSlider.value / 100f) * audio.volume;
        }
        else
        {
            audioSource.volume = audio.volume;
        }
    }

    // Function to fetch sound by name.
    public Audio GetAudio(string name)
    {
        foreach (Audio audio in audioSFXList)
        {
            if (audio.name.Equals(name))
            {
                return audio;
            }
        }

        foreach (Audio audio in audioBGMList)
        {
            if (audio.name.Equals(name))
            {
                return audio;
            }
        }

        return null;
    }

    // Function to fetch sound by audio clip.
    private Audio GetAudio(AudioClip clip)
    {
        foreach (Audio audio in audioSFXList)
        {
            if (audio.clip && audio.clip.Equals(clip))
            {
                return audio;
            }
        }

        foreach (Audio audio in audioBGMList)
        {
            if (audio.clip && audio.clip.Equals(clip))
            {
                return audio;
            }
        }

        return null;
    }

    // Function to fetch random sound by name with beginning string.
    private Audio GetRandomSound(string name)
    {
        List<Audio> audioList = new List<Audio>();

        foreach (Audio audio in audioSFXList)
        {
            if (!audio.clip)
            {
                continue;
            }

            string[] audioString = audio.name.Split(' ');

            if (audioString[0].Equals(name))
            {
                audioList.Add(audio);
            }
        }

        if (audioList.Count > 0)
        {
            Audio selectedAudio = audioList[Random.Range(0, audioList.Count)];
            return selectedAudio;
        }

        return null;
    }
}

[System.Serializable]
public class Audio
{
    [Tooltip("Name of this audio")]
    public string name = "Audio Name";
    [Tooltip("Audio file")]
    public AudioClip clip;
    [Range(0f, 1f)]
    [Tooltip("Volume of this audio (Default is 1)")]
    public float volume = 1f;
    [Range(0.1f, 3f)]
    [Tooltip("Pitch of this audio (Default is 1)")]
    public float pitch = 1f;
    [Tooltip("Determine to make this audio become loopable or not.")]
    public bool loop = false;
    [Tooltip("Use this audio to play on start or not")]
    public bool playOnStart = false;

    private AudioSource audioSource; // Audio Source of this audio.

    // Function to fetch this audio source.
    public AudioSource GetSource()
    {
        return audioSource;
    }

    // Function to set audio source.
    public void SetSource(AudioSource source)
    {
        audioSource = source;
    }
}