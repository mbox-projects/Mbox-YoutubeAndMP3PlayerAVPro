using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Demo_MediaPlayer : MonoBehaviour
{
    public void GotoMainScene()
    {
        SceneManager.LoadScene("MainScene");
    }
}
