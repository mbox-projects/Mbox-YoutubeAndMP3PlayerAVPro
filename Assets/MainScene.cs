using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainScene : MonoBehaviour
{
    public void GotoMusicVideoScene()
    {
        SceneManager.LoadScene("YoutubeScene");
    }

    public void GotoMusicScene()
    {
        SceneManager.LoadScene("MP3Scene");
    }

    public void GotoMainScene()
    {
        SceneManager.LoadScene("MainScene");
    }

    public void GotoAVProScene()
    {
        SceneManager.LoadScene("Demo_MediaPlayer");
    }
}
