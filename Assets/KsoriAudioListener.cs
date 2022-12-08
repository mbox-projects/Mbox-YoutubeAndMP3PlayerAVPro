using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class KsoriAudioListener : MonoBehaviour
{
    // Start is called before the first frame update

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        GameObject Speaker = GameObject.Find("Sphere");
        int VerticalVectorN = 360;
        int HorizontalVectorN = 360;
        float SumDistance = 0;

        float StartTheta = 0;
        float EndTheta = 360;
        float StartPhi = 0;
        float EndPhi = 360;

        GameObject Listener = GameObject.Find("Sphere1");
        Vector3 pos = Speaker.transform.position;
        Vector3 rot = Speaker.transform.eulerAngles;
        float VerticalDivision = 16;
        float HorizontalDivision = 16;
        for (float i = StartPhi; i < EndPhi; i += HorizontalDivision)
        {
            for (float j = StartTheta; j < EndTheta; j += VerticalDivision)
            {
                float Theta = j;
                float Phi = i;
                if (Theta < EndTheta && Theta > StartTheta && Phi < EndPhi && Phi > StartPhi)
                {
                    const float Pi2 = 2 * Mathf.PI;
                    float Theta360 = (Theta / 360f);
                    float Phi360 = (Phi / 360f);
                    const int R = 100;
                    float ThetaAngle = Pi2 * (rot.x / 360f);
                    float PhiAngle = Pi2 * (rot.y / 360f);
                    float Pi2Theta360 = Pi2 * Theta360;
                    float Pi2Phi360 = Pi2 * Phi360;
                    float X = R * Mathf.Sin(ThetaAngle + Pi2Theta360) * Mathf.Cos(PhiAngle + Pi2Phi360);
                    float Z = R * Mathf.Sin(ThetaAngle + Pi2Theta360) * Mathf.Sin(PhiAngle + Pi2Phi360);
                    float Y = R * Mathf.Cos(ThetaAngle + Pi2Theta360);
                    Physics.Raycast(pos, new Vector3(X, Y, Z), out RaycastHit hit);
                    SumDistance += Mathf.Abs(Vector3.Magnitude(pos - hit.point) + Vector3.Magnitude((hit.point - Listener.transform.position)) - Vector3.Magnitude(Speaker.transform.position - Listener.transform.position));
                    Debug.DrawRay(pos, hit.point - pos, Color.red);
                    Debug.DrawRay(hit.point, Listener.transform.position - hit.point, Color.magenta);
                }
            }
        }
        SumDistance /= (VerticalVectorN * HorizontalVectorN);
        Debug.Log(string.Format("{0}ms : {1}m", ((SumDistance / 340) * 1000).ToString(), SumDistance));
        
        AudioMixer audioMixer = KsoriAudioSource.ReverbEffectMixer;
        for (int i = 0; i < 9; i++)
        {
            float freq = 31.25f * Mathf.Pow(2, i);
            audioMixer.SetFloat(freq.ToString() + "Hz Predelay", (SumDistance / 340f));
        }
    }
}
