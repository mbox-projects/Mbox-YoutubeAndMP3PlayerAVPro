using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KMusicPlayerSimple : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip[] audioClips;
    public KsoriAudioSource KsoriAudioSource;
    private void Awake()
    {
        
    }
    private void Start()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
        KsoriAudioSource = new KsoriAudioSource(gameObject,audioSource,0);
    }
    /// <summary>
    /// 음악 바꾸기
    /// </summary>
    /// <param name="num"> 음악 배열 번호 </param>
    public void ChageMusic(int num)
    {
        KsoriAudioSource.Stop();
        KsoriAudioSource.clip = audioClips[num];
        KsoriAudioSource.Play();
    }

    /// <summary>
    /// 음악 재생
    /// </summary>
    public void PlayMusic()
    {
        KsoriAudioSource.Pause();
        KsoriAudioSource.Play();
    }

    /// <summary>
    /// 음악 완전 정지
    /// </summary>
    public void StopMusic()
    {
        KsoriAudioSource.Stop();
    }

    /// <summary>
    /// 음악 일시 정지
    /// </summary>
    public void PauseMusic()
    {
        KsoriAudioSource.Pause();
    }
    
    public void GotoMainScene()
    {
        
        SceneManager.LoadScene(0);

    }
}
