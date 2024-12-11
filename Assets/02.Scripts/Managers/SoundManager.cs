using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Random = UnityEngine.Random;

public class SoundManager : MonoBehaviour
{
    [Header ("Volume")]
    public float masterVolumeSfx = 1f;
    public float masterVolumeBgm = 1f;
    private float basicPitchVolume = 0.2f;
    
    [Header("Others")]
    [SerializeField] AudioClip bgmClip; // Specific bgmSource
    [SerializeField] AudioClip[] audioClip;

    [Header("Pitch")]
    [SerializeField] private GameObject[] pitches;
    
    Dictionary<string, AudioClip> audioClipsDic;

    private AudioSource sfxPlayer;
    private AudioSource bgmPlayer;
    
    // Coroutine for faded out bgm
    private Coroutine fadeOutCoroutine;
    
    // Make audio source for each pitch
    private void Awake()
    {
        sfxPlayer = GetComponent<AudioSource>();
        SetupBGM();
        
        audioClipsDic = new Dictionary<string, AudioClip>();
        foreach (AudioClip a in audioClip)
        {
            audioClipsDic.Add(a.name, a);
        }
        
        UpdatePitchVolumes();
    }
    
    private void SetupBGM()
    {
        if (bgmClip == null) return;
        
        // BGM을 재생하기 위한 AudioSource 설정
        GameObject child = new GameObject("BGMPlayer");
        child.transform.SetParent(transform);
        bgmPlayer = child.AddComponent<AudioSource>();
        bgmPlayer.clip = bgmClip;
        bgmPlayer.loop = true;
        bgmPlayer.volume = masterVolumeBgm;
    }
    

    private void Start()
    {
        if (bgmPlayer != null) bgmPlayer.Play();
    }

    
    private void UpdatePitchVolumes()
    {
        foreach (GameObject pitch in pitches)
        {
            AudioSource audioSource = pitch.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.volume = basicPitchVolume * masterVolumeSfx;
            }
        }
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            pitches[0].SetActive(false);
            pitches[0].SetActive(true);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            pitches[1].SetActive(false);
            pitches[1].SetActive(true);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            pitches[2].SetActive(false);
            pitches[2].SetActive(true);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            pitches[3].SetActive(false);
            pitches[3].SetActive(true);
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            pitches[4].SetActive(false);
            pitches[4].SetActive(true);
        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            pitches[5].SetActive(false);
            pitches[5].SetActive(true);
        }
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            pitches[6].SetActive(false);
            pitches[6].SetActive(true);
        }
        
        if (Input.GetKeyUp(KeyCode.Alpha1))
        {
            pitches[0].SetActive(false);
        }
        if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            pitches[1].SetActive(false);
        }
        if (Input.GetKeyUp(KeyCode.Alpha3))
        {
            pitches[2].SetActive(false);
        }
        if (Input.GetKeyUp(KeyCode.Alpha4))
        {
            pitches[3].SetActive(false);
        }
        if (Input.GetKeyUp(KeyCode.Alpha5))
        {
            pitches[4].SetActive(false);
        }
        if (Input.GetKeyUp(KeyCode.Alpha6))
        {
            pitches[5].SetActive(false);
        }
        if (Input.GetKeyUp(KeyCode.Alpha7))
        {
            pitches[6].SetActive(false);
        }
        
    }

    public void PlayBGM(string a_name)
    {
        if (audioClipsDic.ContainsKey(a_name) == false)
        {
            Debug.LogWarning("Invalid BGM index.");
            return;
        }

        AudioClip newClip = audioClipsDic[a_name];

        // 현재 BGM이 이미 재생 중이라면 페이드 아웃 후 전환
        if (bgmPlayer.isPlaying && bgmPlayer.clip != newClip)
        {
            if (fadeOutCoroutine != null)
                StopCoroutine(fadeOutCoroutine);
            fadeOutCoroutine = StartCoroutine(FadeOutAndChangeBGM(newClip));
        }
        else
        {
            // 바로 새로운 BGM 재생
            bgmPlayer.clip = newClip;
            bgmPlayer.Play();
        }
    }
    
    // Play sfx once
    public void PlaySound(string a_name, float a_volume = 1f)
    {
        if (audioClipsDic.ContainsKey(a_name) == false)
        {
            Debug.Log(a_name + " is not Contained audioClipsDic");
            return;
        }
        sfxPlayer.PlayOneShot(audioClipsDic[a_name], a_volume * masterVolumeSfx);
    }
    
    // Play random sfx once in given list 
    public void PlayRandomSound(string[] a_nameArray, float a_volume = 1f)
    {
        string l_playClipName;

        l_playClipName = a_nameArray[Random.Range(0, a_nameArray.Length)];

        if (audioClipsDic.ContainsKey(l_playClipName) == false)
        {
            Debug.Log(l_playClipName + " is not Contained audioClipsDic");
            return;
        }
        sfxPlayer.PlayOneShot(audioClipsDic[l_playClipName], a_volume * masterVolumeSfx);
    }
    
    // Play loop sound: The caller keep the gameobject which generates audio, and delete it properly
    public GameObject PlayLoopSound(string a_name)
    {
        if (audioClipsDic.ContainsKey(a_name) == false)
        {
            Debug.Log(a_name + " is not Contained audioClipsDic");
            return null;
        }

        GameObject l_obj = new GameObject("LoopSound");
        AudioSource source = l_obj.AddComponent<AudioSource>();
        source.clip = audioClipsDic[a_name];
        source.volume = masterVolumeSfx;
        source.loop = true;
        source.Play();
        return l_obj;
    }
    
    // Bgm Stop (Faded)
    public void StopBGM()
    {
        if (bgmPlayer.isPlaying)
        {
            if (fadeOutCoroutine != null)
                StopCoroutine(fadeOutCoroutine);
            fadeOutCoroutine = StartCoroutine(FadeOutAndStop());
        }
    }
    
    // 페이드 아웃 후 새로운 BGM으로 전환하는 코루틴
    private IEnumerator FadeOutAndChangeBGM(AudioClip newClip)
    {
        float fadeDuration = 1.5f; // 페이드 아웃 시간
        float startVolume = bgmPlayer.volume;

        while (bgmPlayer.volume > 0)
        {
            bgmPlayer.volume -= startVolume * Time.deltaTime / fadeDuration;
            yield return null;
        }

        // 페이드 아웃 완료 후 새로운 BGM으로 전환
        bgmPlayer.Stop();
        bgmPlayer.clip = newClip;
        bgmPlayer.volume = masterVolumeBgm;
        bgmPlayer.Play();

        fadeOutCoroutine = null;
    }
    // 페이드 아웃 Bgm 코루틴
    private IEnumerator FadeOutAndStop()
    {
        float fadeDuration = 1.5f;
        float startVolume = bgmPlayer.volume;

        while (bgmPlayer.volume > 0)
        {
            bgmPlayer.volume -= startVolume * Time.deltaTime / fadeDuration;
            yield return null;
        }

        bgmPlayer.Stop();
        bgmPlayer.volume = masterVolumeBgm;
    }
    
    #region Volume adjustment
    public void SetVolumeSFX(float a_volume)
    {
        masterVolumeSfx = a_volume;
        UpdatePitchVolumes();
    }

    public void SetVolumeBGM(float a_volume)
    {
        masterVolumeBgm = a_volume;
        bgmPlayer.volume = masterVolumeBgm;
    }
    #endregion

}
