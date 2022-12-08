using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class GlobalKsoriSoundSetting : MonoBehaviour
{
    public AudioMixer ReverbMixer = null;
    // Start is called before the first frame update
    private void Awake()
    {
        KsoriAudioSource.ReverbEffectMixer = ReverbMixer;
    }
}
