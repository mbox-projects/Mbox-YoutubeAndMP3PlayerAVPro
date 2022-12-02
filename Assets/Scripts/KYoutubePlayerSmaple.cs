using System.Collections;
using System.Collections.Generic;
using LightShaft.Scripts;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

// public enum GENRE
// {
//     Begin = 0,
//     
//     Dance,
//     Balad,
//     Jazz,
//     Traditional,
//     
//     End
// }

public class KYoutubePlayerSmaple : MonoBehaviour
{
    //유튜브 플레이어 컴포넌트
    public YoutubePlayer player;
   
    //비디오 플레이어 컴포넌트
    public VideoPlayer videoPlayer;
    
    //유튜브 RUL
    string url;
    
    // //장르 모음
    // private GENRE genre
    // {
    //     set
    //     {
    //         if(genre == value)
    //             return;
    //         else
    //         {
    //             genre = value;
    //             OnGenreChaged();
    //         }
    //     }
    //     get
    //     {
    //         return genre;
    //     }
    // }


    private void Awake()
    {
        //변수 초기화
        videoPlayer = GetComponentInChildren<VideoPlayer>();
        player = GetComponentInChildren<YoutubePlayer>();
        player.videoPlayer = videoPlayer;
    }

    private void Start()
    {
        
    }

    
    /// <summary>
    /// url 을 바꾸고 영상을 제생 합니다
    /// </summary>
    public void PlayUrl(string url_)
    {
        url = url_;
        player.Stop();
        
        // switch (genre)
        // {
        //     case GENRE.Dance : url = "https://www.youtube.com/watch?v=fgSXAKsq-Vo"; break;
        //     case GENRE.Balad : url = "https://www.youtube.com/watch?v=JY-gJkMuJ94"; break;
        //     case GENRE.Jazz : url = "https://www.youtube.com/watch?v=Y2rDb4Ur2dw"; break;
        //     case GENRE.Traditional : url = "https://www.youtube.com/watch?v=pUTOEoLUB50"; break;
        //     default: url = "https://www.youtube.com/watch?v=fgSXAKsq-Vo"; break;
        // }
        
        player.Play(url_);
    }

    /// <summary>
    /// 장르가 바뀌었을때 실행되는 함수
    /// </summary>
    void OnGenreChaged()
    {
        //PlayUrl(url);
    }

    /// <summary>
    /// 플레이어 재셍
    /// </summary>
    public void PlayPlayer()
    {
        if (!string.IsNullOrEmpty(url))
        {
            player.Pause();
            player.Play();
        }
    }

    /// <summary>
    /// 플레이어 일시정지
    /// </summary>
    public void PlayerPause()
    {
        if (!string.IsNullOrEmpty(url))
        {
            player.Pause();
        }
    }

    /// <summary>
    /// 플레이어 정지
    /// </summary>
    public void PlayerStop()
    {
        player.Stop();
    }

    // public void ChageGenre(int gr)
    // {
    //     genre = (GENRE)gr;
    // }

    public void GotoMainScene()
    {
        player.Stop();
        SceneManager.LoadScene(0);
    }
}
