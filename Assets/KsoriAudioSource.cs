using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class KsoriAudioSource : MonoBehaviour
{
    public AudioSource AudioSource = null;
    public static AudioMixer ReverbEffectMixer = null;
    public static AudioMixerGroup DryChannel = null;
    public static AudioMixerGroup WetChannel = null;
    
    AudioSource DrySource = null;
    AudioSource WetSource = null;
    // Start is called before the first frame update
    GameObject GameObject = null;
    public KsoriAudioSource(GameObject GO, AudioSource AS, float sound3D)
    {
        GameObject = GO;
        AudioSource = AS;
        PlaySetting();
        Sound3D = sound3D;
    }
    public AudioClip clip
    {
        get
        {
            return AudioSource.clip;
        }
        set
        {
            AudioSource.clip = value;
            PlaySetting();
        }
    }
    public float Sound3D
    {
        get
        {
            return AudioSource.spatialBlend;
        }
        set 
        { 
            AudioSource.spatialBlend = value; 
        }
    }
    public void Play()
    {
        DrySource.Play();
        WetSource.Play();
    }
    public void Stop()
    {
        DrySource.Stop();
        WetSource.Stop();
    }
    public void Pause()
    {
        DrySource.Pause();
        WetSource.Pause();
    }
    public void UnPause()
    {
        DrySource.UnPause();
        WetSource.UnPause();
    }
    public void PlaySetting()
    {
        if (AudioSource == null)
        {
            AudioSource = GetComponent<AudioSource>();
            if (AudioSource == null)
            {
                Debug.Log("해당 오브젝트에 오디오 소스가 없습니다.");
                return;
            }
        }
        if (AudioSource != null) AudioSource.Stop();
        if (DryChannel == null || WetChannel == null)
        {
            DryChannel = ReverbEffectMixer.FindMatchingGroups("Dry")[0];
            WetChannel = ReverbEffectMixer.FindMatchingGroups("Wet")[0];
        }

        if (DrySource == null) DrySource = (GameObject == null ? gameObject.AddComponent<AudioSource>() : GameObject.AddComponent<AudioSource>());
        if (WetSource == null) WetSource = (GameObject == null ? gameObject.AddComponent<AudioSource>() : GameObject.AddComponent<AudioSource>());


        DrySource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, AudioSource.GetCustomCurve(AudioSourceCurveType.CustomRolloff));

        DrySource.clip = AudioSource.clip;
        WetSource.clip = AudioSource.clip;

        DrySource.loop = AudioSource.loop;
        WetSource.loop = AudioSource.loop;

        DrySource.volume = AudioSource.volume;
        WetSource.volume = AudioSource.volume;

        DrySource.outputAudioMixerGroup = DryChannel;
        WetSource.outputAudioMixerGroup = WetChannel;

        DrySource.spatialBlend = Sound3D;
        WetSource.spatialBlend = 0;

        DrySource.playOnAwake = AudioSource.playOnAwake;
        WetSource.playOnAwake = AudioSource.playOnAwake;

        DrySource.dopplerLevel = 0;
        WetSource.dopplerLevel = 0;

        DrySource.panStereo = 0f;
        WetSource.panStereo = 0f;
    }
    public void PlayInstant()
    {
        PlaySetting();
        Play();
    }
    void Start()
    {
        PlayInstant();
    }

    // Update is called once per frame
    void Update()
    {
        //if (Mathf.Abs(DrySource.time-WetSource.time) > 0.1)
        {
            //WetSource.time = DrySource.time;
            //Debug.Log(WetSource == null && DrySource == null);
        }
    }
}
