using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioClip _btnClip;
    [SerializeField] private AudioClip _btnClickClip;
    [SerializeField] private AudioClip _rotClip;
    [SerializeField] private AudioClip _blockClip;
    [SerializeField] private AudioClip _clearClip;
    [SerializeField] private AudioClip _levelUpClip;

    private AudioSource _audioSource;

    private static AudioManager instance;

    public static AudioManager Instance //싱글톤
    {
        get
        {
            if (!instance)
            {
                //씬에서 AudioManager를 찾음.
                instance = FindObjectOfType<AudioManager>();
                
                if (!instance)
                {
                    Debug.Log("Not Found AudioManager, creating a new one.");
                    //씬에 없으면 새로 생성
                    GameObject audioPrefab = Resources.Load("AudioPrefab") as GameObject;
                    GameObject audioInstance = Instantiate(audioPrefab);
                    if (audioInstance != null)
                        instance = audioInstance.GetComponent<AudioManager>();
                }
            }

            return instance;
        }
    }


    void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        if (!_audioSource)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            Debug.Log("New AudioSource Added");
        }
    }
    
    // 이하 클립재생
    public void PlayButtonSound()
    {
        _audioSource.PlayOneShot(_btnClip);
    }
    public void PlayClickSound()
    {
        _audioSource.PlayOneShot(_btnClickClip);
    }
    public void PlayRotationSound()
    {
        _audioSource.PlayOneShot(_rotClip);
    }
    public void PlayBlockSound()
    {
        _audioSource.PlayOneShot(_blockClip);
    }
    public void PlayClearSound()
    {
        _audioSource.PlayOneShot(_clearClip);
    }
    public void PlayLevelupSound()
    {
        _audioSource.PlayOneShot(_levelUpClip);
    }

}
