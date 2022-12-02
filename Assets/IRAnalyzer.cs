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
    public AudioMixer AudioMixer = null;
    public AudioClip IR = null;
    public int TargetChannel = 0;
    public bool ReverbOn = true;
    [Range(-80, 40)]
    public float ReverbLevel = 0;
    


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
    int FFTLength = 8192;
    bool OnOffsave;
    float ReverbLevelMemory = 0;
    // Start is called before the first frame update
    void Start()
    {
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
        AudioMixer.SetFloat("Decay", Decay * 3);

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
            AudioMixer.SetFloat(Oct0.ToString() + "Hz", band[i]);
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
        float Resolution = 0.01f;
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
                FreqDecay[Key][j] /= NumberOfBlock;// (Oct1ToIndex-Oct0ToIndex);
            }
            float EnergySum = 0;
            float Sum = 0;
            for (int j = NumberOfBlock - 1; j >= 0; j--)
            {
                EnergySum += Math.Abs(FreqDecay[Key][j] * FreqDecay[Key][j]);
                Sum += Math.Abs(FreqDecay[Key][j]);
                FreqDecaySch[Key][j] = 10 * Mathf.Log10(EnergySum);
                FreqDecay20dB[Key][j] = 20 * Mathf.Log10(Sum);
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
                    AudioMixer.SetFloat(Key + "Hz", FreqDecayRT20[Key]);
                    break;
                }
            }
            FreqDecayGaindB[Key] = FreqDecayRT20[Key] <= 0 ? -80 : FreqDecay20dB[Key][0];
            AudioMixer.SetFloat(Key + "Hz Gain", FreqDecayGaindB[Key]);
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
    // Update is called once per frame
    void Update()
    {
        if (IR.name != IRName || IR.length != IRLength || IR.channels != IRChannels || IR.frequency != IRSamplerate || IR.samples != IRSamples)
        {
            AnalyzeStart();
        }
        if (OnOffsave != ReverbOn || ReverbLevelMemory != ReverbLevel)
        {
            foreach (string hz in FreqDecayGaindB.Keys)
            {
                AudioMixer.SetFloat(hz + "Hz Gain", ReverbOn ? FreqDecayGaindB[hz]+ReverbLevel : -80);
            }
            OnOffsave = ReverbOn;
            ReverbLevelMemory = ReverbLevel;
        }
    }
}
