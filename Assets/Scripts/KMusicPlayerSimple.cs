using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KMusicPlayerSimple : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip[] audioClips;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// 음악 바꾸기
    /// </summary>
    /// <param name="num"> 음악 배열 번호 </param>
    public void ChageMusic(int num)
    {
        audioSource.Stop();
        audioSource.clip = audioClips[num];
        audioSource.Play();
    }

    /// <summary>
    /// 음악 재생
    /// </summary>
    public void PlayMusic()
    {
        audioSource.Pause();
        audioSource.Play();
    }

    /// <summary>
    /// 음악 완전 정지
    /// </summary>
    public void StopMusic()
    {
        audioSource.Stop();
    }

    /// <summary>
    /// 음악 일시 정지
    /// </summary>
    public void PauseMusic()
    {
        audioSource.Pause();
    }
    
    public void GotoMainScene()
    {
        
        SceneManager.LoadScene(0);

    }
}
