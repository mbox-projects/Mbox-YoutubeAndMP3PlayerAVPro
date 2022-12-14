using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using UnityEngine;
using UnityEngine.Audio;

public class IRAnalyzer : MonoBehaviour
{
    [Header("리버브 세팅")]
    //public AudioMixer AudioMixer = null;
    public GameObject Listener = null;
    public GameObject Speaker = null;
    public AudioClip IR = null;
    public int TargetChannel = 0;

    [Tooltip("리버브 온오프\n음장효과를 On/Off 스위치입니다.")]
    public bool ReverbOn = true;

    [Tooltip("리버브 레벨\n음장효과의 크기를조정합니다. \n 이 크기를 높히면 음장효과가 부각되지만, 지나치게 높히면 비현실적으로 들립니다.")]
    [Range(-80, 40)]
    public float ReverbLevel;// = 0;

    [Tooltip("초기반사음 레벨\n뮤지션이 소리를 내고 나서, 벽에 반사된 후 0.1초 이내로 청취자에게 도달하는 반사음의 크기를 조정합니다.\n초기반사음이 크면 뮤지션의 위치가 불분명해집니다.")]
    [Range(-80, 0)]
    public float EarlyReflectionLevel;// = -40;

    [Tooltip("음장데이터의 분석 주파수\n음장데이터를 분석 시 주파수 분석 해상도를 조정합니다.\n높히면 리버브의 음색이 정밀해지지만 시간이 오래걸립니다.")]
    [Range(1, 3)]
    public int FreqResolution;// = 1;

    [Tooltip("음장데이터의 분석 시정수\n음장데이터를 분석 시 시정수 분석 해상도를 조정합니다.\n높히면 리버브 지속시간이 정밀해지지만 시간이 오래걸립니다.")]
    [Range(1, 2)]
    public float TimeResolution;// = 1;

    [Tooltip("음선 레이트레이싱의 해상도를 조정합니다.\n뮤지션과 청취자간의 소리 경로를 추적할 때 음선 방사각의 밀도를 조정합니다.\n이 밀도를 높히면 뮤지션의 소리와 리버브 음의 도달 시간 차이가 정밀해지지만\n많은 연산량이 요구됩니다.")]
    [Range(1, 6)]
    public float RayTracingResolution;// = 16;

    [Tooltip("음선의 경로를 표시합니다.\n음선 경로를 모니터링하기 위한 시스템으로 디버깅 시에만 사용됩니다.")]
    [Range(1, 6)]
    public bool DrawRayTracingLine = true;

    [Tooltip("벽 반사 범위\n청취자가 벽에 가까이 있을 때 뮤지션의 소리가 벽에 반사되어 나오는 소리가 들리도록하는 거리를 정의합니다.\n너무 높히면 벽 반사 방향성이 흐릿해집니다.")]
    [Range(1, 6)]
    public float WallReflectionRange;

    [Tooltip("배경 초기반사음과 벽 반사음의 비율\n청취자는 근접한 벽의 큰 반사음과 멀리 있는 벽의 작은 반사음을 더해 듣게됩니다. \n근접한 벽반사와 배경 초기반사음의 소리 크기의 비율을 조정합니다.")]
    [Range(1, 6)]
    public float WallReflectionRatio;

    [Tooltip("초기반사음으로 취급할 음선의 정의를 조정합니다.\n초기반사음은 뮤지션의 소리가 전달 된 후 특정 시간 이내의 반사음을 뜻합니다.\n그 특정 시간을 몇 초로 할건지 정의합니다.")]
    [Range(1, 6)]
    public float ERTimeRangeDefine;

    float[] IRdata = null;
    float[][] IRArray = null;
    float[] SelectedIR = null;
    float[] SelectedIRdB = null;
    float Decay = 0;
    float Delay = 0;
    float Wet = 0;
    float ER = 0;
    float ERDelay = 0;
    float Predelay = 0;
    float Diffusion = 0;
    int PeakIndex = 0;
    int ERPointIndex = 0;
    float Average = 0;
    float Sum = 0;
    float ERSum = 0;
    float Max = 0;
    int RT20 = 0;
    float[] SchCurve = null;
    float[] SchCurve10LogdB = null;
    string IRName = "";
    float IRLength = 0;
    int IRChannels = 0;
    int IRSamplerate = 0;
    int IRSamples = 0;
    List<float> band = new List<float>();
    DITRadix2FFT FFT = null;
    List<float[]> STFT = null;
    Dictionary<string, float[]> FreqDecay = new Dictionary<string, float[]>();
    Dictionary<string, float[]> FreqDecaySch = new Dictionary<string, float[]>();
    Dictionary<string, float[]> FreqDecay20dB = new Dictionary<string, float[]>();
    Dictionary<string, float> FreqDecayRT20 = new Dictionary<string, float>();
    Dictionary<string, float> FreqDecayGaindB = new Dictionary<string, float>();
    float PeakdB = 0;
    int FFTLength = 2048;
    float OverlapLength = 0.1f;
    bool OnOffsave;
    float ReverbLevelMemory = 0;
    float ERLevelMemory = 0;
    // Start is called before the first frame update
    void Start()
    {
        FFTLength = (int)Mathf.Pow(2,10+FreqResolution);
        OverlapLength = 1f / (Mathf.Pow(10, TimeResolution));
        AnalyzeStart();
    }
    void AnalyzeStart()
    {
        if (IR.length > 20)
        {
            Debug.Log("임펄스가 20초 이상으로 너무 깁니다.");
            return;
        }
        FFT = new DITRadix2FFT(FFTLength);
        IRName = IR.name;
        IRLength = IR.length;
        IRChannels = IR.channels;
        IRSamplerate = IR.frequency;
        IRSamples = IR.samples;
        InitAndImpulseSetting();
        //GetPeakAndAverage();
        //GetLongFFT();
        //GetSchroederCurve();
        GetSTFT();
        KsoriAudioSource.ReverbEffectMixer.SetFloat("Decay", Decay * 3);

    }
    void InitAndImpulseSetting()
    {
        if (IR == null) return;

        ERPointIndex = (int)(0.02f * IR.frequency);
        IRdata = new float[IR.channels * IRSamples];
        IR.GetData(IRdata, 0);
        IRArray = new float[IR.channels][];

        for (int i = 0; i < IR.channels; i++)
        {
            IRArray[i] = new float[IRSamples];
            for (int j = 0; j < IRSamples; j++)
            {
                IRArray[i][j] = IRdata[(IR.channels * j) + i];
            }
        }
        SelectedIR = IRArray[TargetChannel];
        SelectedIRdB = new float[IRSamples];

    }

    void GetPeakAndAverage()
    {

        for (int i = 0; i < IRSamples; i++)
        {
            float Sample = Mathf.Abs(SelectedIR[i]);
            Sum += Sample;
            if (i < ERPointIndex)
            {
                ERSum += Sample;
            }

            if (Sample > Max)
            {
                PeakIndex = i;
                Max = Sample;
            }
            PeakdB = 20 * Mathf.Log10(Max);
            SelectedIRdB[i] = 20 * Mathf.Log10(Sample);
        }
        Average = Sum / IRSamples;
        Wet = Average / Max;


    }

    void GetLongFFT()
    {
        int FFTSize = (int)Mathf.Pow(2, (int)Mathf.Log(IRSamples - PeakIndex, 2));
        //Debug.Log(FFTSize);
        float[] FFTTimeDomain = new float[FFTSize];
        float[,] Result = new float[FFTSize, 2];
        float[] Magnitude = new float[FFTSize];
        FFT = new DITRadix2FFT(FFTSize);
        for (int i = PeakIndex; i < PeakIndex + FFTSize; i++)
        {
            FFTTimeDomain[i - PeakIndex] = SelectedIR[i];
        }
        FFT.FFT(FFTTimeDomain, null, FFTBase.FFTMode.FFT, FFTBase.FFTType.Real, Result);

        for (int i = 0; i < FFTSize; i++)
        {
            float a = Result[i, 0];
            float b = Result[i, 1];
            Magnitude[i] = Mathf.Sqrt((a * a) + (b * b));
            //Debug.Log(Magnitude[i]);
        }
        string Info = "";
        for (int i = 0; i <= 9; i++)
        {
            float Oct0 = 31.25f * Mathf.Pow(2, i / 1f);
            float Oct1 = 31.25f * Mathf.Pow(2, (i + 1) / 1f);
            int Oct0ToIndex = (int)((Oct0 / IRSamplerate) * FFTSize);
            int Oct1ToIndex = (int)((Oct1 / IRSamplerate) * FFTSize);
            band.Add(0);
            for (int j = Oct0ToIndex; j < Oct1ToIndex; j++)
            {
                band[i] += Magnitude[j];
            }
            band[i] /= Oct1ToIndex - Oct0ToIndex;
            Info += string.Format("{0}Hz : {1}\n", Oct0, 20 * Mathf.Log10(band[i]));
            KsoriAudioSource.ReverbEffectMixer.SetFloat(Oct0.ToString() + "Hz", band[i]);
        }
    }
    float[] STFTProcess(int FFTSize, int Offset)
    {
        float[] FFTTimeDomain = new float[FFTSize];
        float[,] FFTResult = new float[FFTSize, 2];
        float[] Magnitude = new float[FFTSize];
        
        for (int i = Offset; i < Offset + FFTSize; i++)
        {
            FFTTimeDomain[i - Offset] = SelectedIR[i];
        }
        FFT.FFT(FFTTimeDomain, null, FFTBase.FFTMode.FFT, FFTBase.FFTType.Real, FFTResult);
        for (int i = 0; i < FFTSize; i++)
        {
            float a = FFTResult[i, 0];
            float b = FFTResult[i, 1];
            Magnitude[i] = Mathf.Sqrt((a * a) + (b * b)) / FFTSize;
        }
        return Magnitude;
    }
    void GetSTFT()
    {
        int STFTSize = FFTLength;
        float Resolution = OverlapLength;
        int OverlapBlockSize = (int)(IRSamplerate * Resolution);
        int NumberOfBlock = (IRSamples - STFTSize) / OverlapBlockSize;
        STFT = new List<float[]>();
        STFT.Clear();
        for (int i = 0; i < NumberOfBlock; i++)
        {
            STFT.Add(STFTProcess(STFTSize, OverlapBlockSize * i));
        }
        FreqDecay.Clear();
        FreqDecaySch.Clear();
        FreqDecay20dB.Clear();
        //string Info = "";
        for (int i = 0; i <= 9; i++)
        {
            float Oct0 = 31.25f * Mathf.Pow(2, i / 1f);
            float Oct1 = Mathf.Min(31.25f * Mathf.Pow(2, (i + 1) / 1f), 20000);
            int Oct0ToIndex = (int)((Oct0 / IRSamplerate) * STFTSize);
            int Oct1ToIndex = (int)((Oct1 / IRSamplerate) * STFTSize);
            string Key = Oct0.ToString();
            FreqDecay.Add(Key, new float[NumberOfBlock]);
            FreqDecaySch.Add(Key, new float[NumberOfBlock]);
            FreqDecay20dB.Add(Key, new float[NumberOfBlock]);
            for (int j = 0; j < NumberOfBlock; j++)
            {
                for (int k = Oct0ToIndex; k < Oct1ToIndex; k++)
                {
                    FreqDecay[Key][j] += STFT[j][k];
                }
                FreqDecay[Key][j] /= NumberOfBlock;// * (Oct1ToIndex-Oct0ToIndex);
            }
            float EnergySum = 0;
            float Sum = 0;
            for (int j = NumberOfBlock - 1; j >= 0; j--)
            {
                EnergySum += Math.Abs(FreqDecay[Key][j] * FreqDecay[Key][j]);
                Sum += Math.Abs(FreqDecay[Key][j]);
                FreqDecaySch[Key][j] = 10 * Mathf.Log10(EnergySum);
                FreqDecay20dB[Key][j] = 10 * Mathf.Log10(Sum);
            }
            //StringBuilder stringBuilder = new StringBuilder();
            //for (int j = 0; j < FreqDecaySch[Key].Length; j++)
            //{
            //    stringBuilder.AppendLine(FreqDecaySch[Key][j].ToString());
            //}
            //System.IO.File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + String.Format("\\Sch{0}.txt", Key), stringBuilder.ToString());
            for (int j = 0; j < FreqDecaySch[Key].Length; j++)
            {
                if (FreqDecaySch[Key][j] <= FreqDecaySch[Key][0] - 20)
                {
                    FreqDecayRT20[Key] = j * Resolution * 3;
                    //Debug.Log(string.Format("{0}Hz : {1}",Key,FreqDecayRT20[Key]));
                    if (FreqDecayRT20[Key] > 10) FreqDecayRT20[Key] = 0;
                    KsoriAudioSource.ReverbEffectMixer.SetFloat(Key + "Hz", FreqDecayRT20[Key]);
                    break;
                }
            }
            FreqDecayGaindB[Key] = FreqDecayRT20[Key] <= 0 ? -80 : FreqDecay20dB[Key][0];
            KsoriAudioSource.ReverbEffectMixer.SetFloat(Key + "Hz Gain", FreqDecayGaindB[Key]);
            //string Report = string.Format("{0}Hz : {1}초 {2}dB", Key, FreqDecayRT20[Key], FreqDecayGaindB[Key]);
            //Debug.Log(Report);
        }
    }
    void GetSchroederCurve()
    {
        bool RT60Detected = false;
        float SchSum = 0;
        SchCurve = new float[IRSamples];
        SchCurve10LogdB = new float[IRSamples];
        int SchMaxIndex = 0;
        float SchMax = 0;
        for (int i = SelectedIR.Length - 1; i >= 0; i--)
        {
            SchSum += Math.Abs(SelectedIR[i] * SelectedIR[i]);
            SchCurve[i] = SchSum;
            SchCurve10LogdB[i] = 10 * Mathf.Log10(SchCurve[i]);
            if (SchMax <= SchCurve10LogdB[i])
            {
                SchMaxIndex = i;
                SchMax = SchCurve10LogdB[i];
            }
        }

        for (int i = 0; i < SchCurve10LogdB.Length; i++)
        {
            if (SchCurve10LogdB[i] <= SchMax - 20 && RT60Detected == false)
            {
                RT20 = i;
                RT60Detected = true;
                break;
            }
        }
        Decay = (RT20 - SchMaxIndex) / (float)IR.frequency;
    }

    float SimpleDFT(float Freq, int SampleRate, float[] Samples, float Start, float Length)
    {
        float Magnitude;
        int StartIndex = (int)(Start * SampleRate);
        int LengthIndex = (int)(Length * SampleRate);
        float SinValue = 0;
        float CosValue = 0;
        int FreqIndex = (int)((Freq / SampleRate) * LengthIndex);
        int t = 0;
        for (int i = StartIndex; i < StartIndex + LengthIndex; i++)
        {
            float w = (2 * Mathf.PI * FreqIndex * t) / LengthIndex;
            SinValue += Samples[i] * -Mathf.Sin(w);
            CosValue += Samples[i] * Mathf.Cos(w);
            t++;
        }
        Magnitude = Mathf.Sqrt((CosValue * CosValue) + (SinValue * SinValue));
        return Magnitude;
    }

    void SoundRayTracing()
    {
        GameObject Speaker = GameObject.Find("Sphere");
        int VerticalVectorN = 360;
        int HorizontalVectorN = 360;
        float SumDistance = 0;
        float StartTheta = 0;
        float EndTheta = 360;
        float StartPhi = 0;
        float EndPhi = 360;
        int NumberOfERVector = 0;
        GameObject Listener = GameObject.Find("Sphere1");
        Vector3 pos = Speaker.transform.position;
        Vector3 rot = Speaker.transform.eulerAngles;
        float RayTracingResolution2Power = Mathf.Pow(2, RayTracingResolution);

        float VerticalDivision = 360f / RayTracingResolution2Power;
        float HorizontalDivision = 360f / RayTracingResolution2Power;

        float LeftEarER = 0;
        float RightEarER = 0;

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
                    float Pi2Theta360 = Pi2 * Theta360;
                    float Pi2Phi360 = Pi2 * Phi360;
                    float Z = R * Mathf.Sin(Pi2Theta360) * Mathf.Cos(Pi2Phi360);
                    float X = R * Mathf.Sin(Pi2Theta360) * Mathf.Sin(Pi2Phi360);
                    float Y = R * Mathf.Cos(Pi2Theta360);
                    Physics.Raycast(pos, new Vector3(X, Y, Z), out RaycastHit hit);
                    float SpeakerToWallDistance = Vector3.Magnitude(hit.point - pos);
                    Vector3 ListenerPos = Listener.transform.position;
                    float WallToListenerDistance = Vector3.Magnitude(ListenerPos - hit.point);
                    float DirectSoundDistance = Vector3.Magnitude(ListenerPos - Speaker.transform.position);
                    float SWD = SpeakerToWallDistance + WallToListenerDistance - DirectSoundDistance;
                   
                    if (SWD < (340 * 0.05f) && WallToListenerDistance < 5)
                    {
                        Vector2 LeftEar = new Vector2((-Listener.transform.right).x,(-Listener.transform.right).z);
                        Vector2 RightEar = new Vector2(Listener.transform.right.x, Listener.transform.right.z);

                        Vector2 B = new Vector2(hit.point.x,hit.point.z);
                        float left = LeftEar.magnitude;
                        float right = RightEar.magnitude;
                        float wall = B.magnitude;
                        float LeftVol = Vector2.Dot(LeftEar, B) / (left * wall);
                        float RightVol = Vector2.Dot(RightEar, B) / (right * wall);
                        LeftEarER += Mathf.Max(LeftVol, 0);
                        RightEarER += Mathf.Max(RightVol, 0);
                        NumberOfERVector++;
                        Debug.DrawRay(pos, hit.point - pos, Color.red);
                        Debug.DrawRay(hit.point, ListenerPos - hit.point, Color.magenta);
                    }
                    else
                    {
                        //Debug.DrawRay(pos, hit.point - pos, Color.green);
                        //Debug.DrawRay(hit.point, ListenerPos - hit.point, Color.cyan);
                    }
                    SumDistance += Mathf.Abs(SWD);
                    if (DrawRayTracingLine)
                    {
                        
                    }
                }
            }
        }
        float VH = VerticalVectorN * HorizontalVectorN;
        SumDistance /= VH;
        AudioMixer audioMixer = KsoriAudioSource.ReverbEffectMixer;
        for (int i = 0; i < 9; i++)
        {
            float freq = 31.25f * Mathf.Pow(2, i);
            audioMixer.SetFloat(freq.ToString() + "Hz Predelay", (SumDistance / 340f));
        }
        NumberOfERVector = Mathf.Max(NumberOfERVector,1);
        LeftEarER /= NumberOfERVector;
        RightEarER /= NumberOfERVector;
        audioMixer.SetFloat("Left ER", 0.2f + (LeftEarER * 0.8f));
        audioMixer.SetFloat("Right ER", 0.2f + (RightEarER * 0.8f));
        Debug.Log(string.Format("{0} {1}",LeftEarER,RightEarER));
        Debug.DrawRay(Listener.transform.position, -Listener.transform.right, Color.green);
        Debug.DrawRay(Listener.transform.position, Listener.transform.right, Color.red);
        Debug.DrawRay(Listener.transform.position, Listener.transform.forward, Color.yellow);
    }
    // Update is called once per frame
    void Update()
    {
        SoundRayTracing();
        if (IR.name != IRName || IR.length != IRLength || IR.channels != IRChannels || IR.frequency != IRSamplerate || IR.samples != IRSamples)
        {
            AnalyzeStart();
        }
        if (OnOffsave != ReverbOn || ReverbLevelMemory != ReverbLevel || ERLevelMemory != EarlyReflectionLevel)
        {
            foreach (string hz in FreqDecayGaindB.Keys)
            {
                KsoriAudioSource.ReverbEffectMixer.SetFloat(hz + "Hz Gain", ReverbOn ? FreqDecayGaindB[hz]+ReverbLevel : -80);
            }
            KsoriAudioSource.ReverbEffectMixer.SetFloat("ER Gain", ReverbOn ? EarlyReflectionLevel : -80);
            OnOffsave = ReverbOn;
            ERLevelMemory = EarlyReflectionLevel;
            ReverbLevelMemory = ReverbLevel;
        }
    }
}
