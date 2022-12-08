using System;
using System.Linq;
public class FFTBase
{
    protected int[] ReverseBinaryIndex = null;
    protected int[,] CurrentIndex = null;
    protected int[,] JumpIndex = null;
    protected float[,] Sinbox = null;
    protected float[,] Cosbox = null;

    protected int FFTSIZE = 0;
    protected int HalfFFTSIZE = 0;
    public enum FFTType
    {
        Real,
        Complex
    }
    public enum FFTMode
    {
        FFT,
        IFFT
    }
}
public class DITRadix2FFT : FFTBase
{
    protected bool disposedValue;
    public DITRadix2FFT(int fftsize)
    {
        FFTSIZE = fftsize;
        HalfFFTSIZE = fftsize / 2;
        int NumberOfStage = (int)Math.Log(FFTSIZE, 2);
        int AllStage = NumberOfStage + 1;
        Sinbox = new float[AllStage, FFTSIZE];
        Cosbox = new float[AllStage, FFTSIZE];
        CurrentIndex = new int[AllStage, FFTSIZE];
        JumpIndex = new int[AllStage, FFTSIZE];
        ReverseBinaryIndex = new int[FFTSIZE];
        for (int i = 0; i < FFTSIZE; i++)
        {
            string BinaryString = new string(Convert.ToString(i, 2).PadLeft(NumberOfStage, '0').ToCharArray().Reverse().ToArray());
            ReverseBinaryIndex[i] = Convert.ToInt32(BinaryString, 2);
        }

        for (int i = 0; i <= NumberOfStage; i++)
        {
            int Power2OfStage = (int)Math.Pow(2, i);
            int HalfPower2OfStage = Power2OfStage / 2;
            int Index = 0;
            for (int j = 0; j < FFTSIZE; j++)
            {
                if ((j % Power2OfStage) < HalfPower2OfStage)
                {
                    float Angle = 2 * (float)Math.PI * (j % Power2OfStage) / Power2OfStage;
                    CurrentIndex[i, Index] = j;
                    JumpIndex[i, Index] = j + HalfPower2OfStage;
                    Sinbox[i, Index] = (float)Math.Sin(Angle);
                    Cosbox[i, Index] = (float)Math.Cos(Angle);
                    Index++;
                }
            }
        }
    }
    public void FFT(float[] RealTimeData, float[] ImgTimeData, FFTMode Mode, FFTType NumberType, float[,] ResultOutput)
    {
        float[] BeforeStageReal = new float[FFTSIZE];
        float[] BeforeStageImg = new float[FFTSIZE];
        float[] CurrentStageReal = null;
        float[] CurrentStageImg = null;
        int Minus = (Mode == FFTMode.FFT ? -1 : 1);
        int NumberOfStage = (int)Math.Log(FFTSIZE, 2);
        for (int i = 0; i <= NumberOfStage; i++)
        {
            CurrentStageReal = new float[FFTSIZE];
            CurrentStageImg = new float[FFTSIZE];
            switch (i)
            {

                case 0:
                    if (NumberType == FFTType.Real)
                    {
                        for (int j = 0; j < FFTSIZE; j++)
                        {
                            int RBI = ReverseBinaryIndex[j];
                            CurrentStageReal[j] = RealTimeData[RBI];

                        }
                        BeforeStageReal = CurrentStageReal;
                    }
                    else
                    {
                        for (int j = 0; j < FFTSIZE; j++)
                        {
                            int RBI = ReverseBinaryIndex[j];
                            CurrentStageReal[j] = RealTimeData[RBI];
                            CurrentStageImg[j] = ImgTimeData[RBI];
                        }
                        BeforeStageReal = CurrentStageReal;
                        BeforeStageImg = CurrentStageImg;
                    }
                    break;

                case 1:
                    if (NumberType == FFTType.Real)
                    {

                        for (int j = 0; j < FFTSIZE; j += 2)
                        {
                            int jumper = j + 1;

                            float RealIndex = BeforeStageReal[j];
                            float RealJump = BeforeStageReal[jumper];

                            CurrentStageReal[j] = RealIndex + RealJump;
                            CurrentStageReal[jumper] = -RealJump + RealIndex;

                        }
                        BeforeStageReal = CurrentStageReal;
                    }
                    else
                    {

                        for (int j = 0; j < FFTSIZE; j += 2)
                        {
                            int jumper = j + 1;

                            float RealIndex = BeforeStageReal[j];
                            float ImgIndex = BeforeStageImg[j];
                            float RealJump = BeforeStageReal[jumper];
                            float ImgJump = BeforeStageImg[jumper];

                            CurrentStageReal[j] = RealIndex + RealJump;
                            CurrentStageImg[j] = ImgIndex + ImgJump;
                            CurrentStageReal[jumper] = -RealJump + RealIndex;
                            CurrentStageImg[jumper] = -ImgJump + ImgIndex;
                        }

                        BeforeStageReal = CurrentStageReal;
                        BeforeStageImg = CurrentStageImg;
                    }
                    break;
                default:
                    for (int j = 0; j < HalfFFTSIZE; j++)
                    {
                        int Index = CurrentIndex[i, j];
                        int Jump = JumpIndex[i, j];
                        float WImg = Minus * Sinbox[i, j];
                        float WReal = Cosbox[i, j];

                        float RealJump = BeforeStageReal[Jump];
                        float ImgJump = BeforeStageImg[Jump];

                        float RealIndex = BeforeStageReal[Index];
                        float ImgIndex = BeforeStageImg[Index];


                        float WMultiplyReal = (RealJump * WReal) - (ImgJump * WImg);
                        float WMultiplyImg = (RealJump * WImg) + (ImgJump * WReal);
                        CurrentStageReal[Index] = RealIndex + WMultiplyReal;
                        CurrentStageImg[Index] = ImgIndex + WMultiplyImg;
                        CurrentStageReal[Jump] = RealIndex + -WMultiplyReal;
                        CurrentStageImg[Jump] = ImgIndex + -WMultiplyImg;
                    }
                    BeforeStageReal = CurrentStageReal;
                    BeforeStageImg = CurrentStageImg;
                    if (i == NumberOfStage)
                    {

                        for (int j = 0; j < FFTSIZE; j++)
                        {
                            ResultOutput[j, 0] = BeforeStageReal[j];
                            ResultOutput[j, 1] = BeforeStageImg[j];
                        }

                    }
                    break;
            }
        }
    }
}