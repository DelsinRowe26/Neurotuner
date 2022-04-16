using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CSCore;
using CSCore.SoundIn;//Вход звука
using CSCore.SoundOut;//Выход звука
using CSCore.CoreAudioAPI;
using CSCore.Streams;
using CSCore.Codecs;
using CSCore.Streams.Effects;
using CSCore.DSP;

namespace Neurotuner
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MMDeviceCollection mInputDevices;
        private MMDeviceCollection mOutputDevices;
        private WasapiCapture mSoundIn;
        private WasapiOut mSoundOut;
        private SampleDSP mDsp, mDspR, mDsp1, mDspR1;
        private SimpleMixer mMixer;
        private ISampleSource mMp3;
        private IWaveSource mSource;
        private int SampleRate;
        private int SampleRate1 = 48000;

        int[] Pitch = new int[10];
        int plusclick = 0, plus = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Находит устройства для захвата звука и заполнияет комбобокс
            MMDeviceEnumerator deviceEnum = new MMDeviceEnumerator();
            mInputDevices = deviceEnum.EnumAudioEndpoints(DataFlow.Capture, DeviceState.Active);
            MMDevice activeDevice = deviceEnum.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Multimedia);
            foreach (MMDevice device in mInputDevices)
            {
                cmbInput.Items.Add(device.FriendlyName);
                if (device.DeviceID == activeDevice.DeviceID) cmbInput.SelectedIndex = cmbInput.Items.Count - 1;
            }

            //Находит устройства для вывода звука и заполняет комбобокс
            activeDevice = deviceEnum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            mOutputDevices = deviceEnum.EnumAudioEndpoints(DataFlow.Render, DeviceState.Active);
            foreach (MMDevice device in mOutputDevices)
            {
                cmbOutput.Items.Add(device.FriendlyName);
                if (device.DeviceID == activeDevice.DeviceID) cmbOutput.SelectedIndex = cmbOutput.Items.Count - 1;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StopFullDuplex();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            StopFullDuplex();
        }

        private void trackGain_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mDsp.GainDB = (float)trackGain.Value;
            lbVolValue.Content = (float)trackGain.Value;
        }

        private void trackPitch_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (cmbSelEff.SelectedIndex == 1 || cmbSelEff.SelectedIndex == 2)
            {
                SetPitchShiftValue();
            }
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            trackGain.Value = 0;
            trackPitch.Value = 0;
            lbVolValue.Content = "0";
            lbPitchValue.Content = "0";
        }

        private void tbDiapPlus()
        {
            switch (plusclick)
            {
                case 0:
                    btnMinus.IsEnabled = true;
                    tbFrom1.Visibility = Visibility.Visible;
                    tbTo1.Visibility = Visibility.Visible;
                    lbFrom1.Visibility = Visibility.Visible;
                    lbTo1.Visibility = Visibility.Visible;
                    lbPitch1.Visibility = Visibility.Visible;
                    tbPitch1.Visibility = Visibility.Visible;
                    lbVol1.Visibility = Visibility.Visible;
                    tbVolume1.Visibility = Visibility.Visible;
                    btnFix.IsEnabled = true;
                    lbZnachPitch.Visibility = Visibility.Visible;
                    tbReverb1.Visibility = Visibility.Visible;
                    tbReverbHFRTR1.Visibility = Visibility.Visible;
                    lbReverb1.Visibility= Visibility.Visible;
                    tbFromReverb1.Visibility = Visibility.Visible;
                    lbFromReverb1.Visibility = Visibility.Visible;
                    lbToReverb1.Visibility = Visibility.Visible;
                    tbToReverb1.Visibility = Visibility.Visible;
                    plusclick++;
                    break;
                case 1:
                    lbFrom2.Visibility = Visibility.Visible;
                    tbFrom2.Visibility = Visibility.Visible;
                    lbTo2.Visibility = Visibility.Visible;
                    tbTo2.Visibility = Visibility.Visible;
                    lbPitch2.Visibility = Visibility.Visible;
                    tbPitch2.Visibility = Visibility.Visible;
                    lbVol2.Visibility = Visibility.Visible;
                    tbVolume2.Visibility = Visibility.Visible;
                    lbReverb2.Visibility = Visibility.Visible;
                    tbReverb2.Visibility= Visibility.Visible;
                    tbReverbHFRTR2.Visibility = Visibility.Visible;
                    tbFromReverb2.Visibility = Visibility.Visible;
                    tbToReverb2.Visibility = Visibility.Visible;
                    lbFromReverb2.Visibility = Visibility.Visible;
                    lbToReverb2.Visibility = Visibility.Visible;
                    plusclick++;
                    break;
                case 2:
                    lbFrom3.Visibility = Visibility.Visible;
                    tbFrom3.Visibility = Visibility.Visible;
                    lbTo3.Visibility = Visibility.Visible;
                    tbTo3.Visibility = Visibility.Visible;
                    lbPitch3.Visibility = Visibility.Visible;
                    tbPitch3.Visibility = Visibility.Visible;
                    lbVol3.Visibility = Visibility.Visible;
                    tbVolume3.Visibility = Visibility.Visible;
                    lbReverb3.Visibility = Visibility.Visible;
                    tbReverb3.Visibility = Visibility.Visible;
                    tbReverbHFRTR3.Visibility = Visibility.Visible;
                    tbFromReverb3.Visibility = Visibility.Visible;
                    tbToReverb3.Visibility = Visibility.Visible;
                    lbFromReverb3.Visibility = Visibility.Visible;
                    lbToReverb3.Visibility = Visibility.Visible;
                    plusclick++;
                    break;
                case 3:
                    lbFrom4.Visibility = Visibility.Visible;
                    tbFrom4.Visibility = Visibility.Visible;
                    lbTo4.Visibility = Visibility.Visible;
                    tbTo4.Visibility = Visibility.Visible;
                    lbPitch4.Visibility = Visibility.Visible;
                    tbPitch4.Visibility = Visibility.Visible;
                    lbVol4.Visibility = Visibility.Visible;
                    tbVolume4.Visibility = Visibility.Visible;
                    lbReverb4.Visibility = Visibility.Visible;
                    tbReverb4.Visibility = Visibility.Visible;
                    tbReverbHFRTR4.Visibility = Visibility.Visible;
                    tbFromReverb4.Visibility = Visibility.Visible;
                    tbToReverb4.Visibility = Visibility.Visible;
                    lbFromReverb4.Visibility = Visibility.Visible;
                    lbToReverb4.Visibility = Visibility.Visible;
                    plusclick++;
                    break;
                case 4:
                    lbFrom5.Visibility = Visibility.Visible;
                    tbFrom5.Visibility = Visibility.Visible;
                    lbTo5.Visibility = Visibility.Visible;
                    tbTo5.Visibility = Visibility.Visible;
                    lbPitch5.Visibility = Visibility.Visible;
                    tbPitch5.Visibility = Visibility.Visible;
                    lbVol5.Visibility = Visibility.Visible;
                    tbVolume5.Visibility = Visibility.Visible;
                    lbReverb5.Visibility = Visibility.Visible;
                    tbReverb5.Visibility = Visibility.Visible;
                    tbReverbHFRTR5.Visibility = Visibility.Visible;
                    tbFromReverb5.Visibility = Visibility.Visible;
                    tbToReverb5.Visibility = Visibility.Visible;
                    lbFromReverb5.Visibility = Visibility.Visible;
                    lbToReverb5.Visibility = Visibility.Visible;
                    plusclick++;
                    break;
                case 5:
                    lbFrom6.Visibility = Visibility.Visible;
                    tbFrom6.Visibility = Visibility.Visible;
                    lbTo6.Visibility = Visibility.Visible;
                    tbTo6.Visibility = Visibility.Visible;
                    lbPitch6.Visibility = Visibility.Visible;
                    tbPitch6.Visibility = Visibility.Visible;
                    lbVol6.Visibility = Visibility.Visible;
                    tbVolume6.Visibility = Visibility.Visible;
                    lbReverb6.Visibility = Visibility.Visible;
                    tbReverb6.Visibility = Visibility.Visible;
                    tbReverbHFRTR6.Visibility = Visibility.Visible;
                    tbFromReverb6.Visibility = Visibility.Visible;
                    tbToReverb6.Visibility = Visibility.Visible;
                    lbFromReverb6.Visibility = Visibility.Visible;
                    lbToReverb6.Visibility = Visibility.Visible;
                    plusclick++;
                    break;
                case 6:
                    lbFrom7.Visibility = Visibility.Visible;
                    tbFrom7.Visibility = Visibility.Visible;
                    lbTo7.Visibility = Visibility.Visible;
                    tbTo7.Visibility = Visibility.Visible;
                    lbPitch7.Visibility = Visibility.Visible;
                    tbPitch7.Visibility = Visibility.Visible;
                    lbVol7.Visibility = Visibility.Visible;
                    tbVolume7.Visibility = Visibility.Visible;
                    lbReverb7.Visibility = Visibility.Visible;
                    tbReverb7.Visibility = Visibility.Visible;
                    tbReverbHFRTR7.Visibility = Visibility.Visible;
                    tbFromReverb7.Visibility = Visibility.Visible;
                    tbToReverb7.Visibility = Visibility.Visible;
                    lbFromReverb7.Visibility = Visibility.Visible;
                    lbToReverb7.Visibility = Visibility.Visible;
                    plusclick++;
                    break;
                case 7:
                    lbFrom8.Visibility = Visibility.Visible;
                    tbFrom8.Visibility = Visibility.Visible;
                    lbTo8.Visibility = Visibility.Visible;
                    tbTo8.Visibility = Visibility.Visible;
                    lbPitch8.Visibility = Visibility.Visible;
                    tbPitch8.Visibility = Visibility.Visible;
                    lbVol8.Visibility = Visibility.Visible;
                    tbVolume8.Visibility = Visibility.Visible;
                    lbReverb8.Visibility = Visibility.Visible;
                    tbReverb8.Visibility = Visibility.Visible;
                    tbReverbHFRTR8.Visibility = Visibility.Visible;
                    tbFromReverb8.Visibility = Visibility.Visible;
                    tbToReverb8.Visibility = Visibility.Visible;
                    lbFromReverb8.Visibility = Visibility.Visible;
                    lbToReverb8.Visibility = Visibility.Visible;
                    plusclick++;
                    break;
                case 8:
                    lbFrom9.Visibility = Visibility.Visible;
                    tbFrom9.Visibility = Visibility.Visible;
                    lbTo9.Visibility = Visibility.Visible;
                    tbTo9.Visibility = Visibility.Visible;
                    lbPitch9.Visibility = Visibility.Visible;
                    tbPitch9.Visibility = Visibility.Visible;
                    lbVol9.Visibility = Visibility.Visible;
                    tbVolume9.Visibility = Visibility.Visible;
                    lbReverb9.Visibility = Visibility.Visible;
                    tbReverb9.Visibility = Visibility.Visible;
                    tbReverbHFRTR9.Visibility = Visibility.Visible;
                    tbFromReverb9.Visibility = Visibility.Visible;
                    tbToReverb9.Visibility = Visibility.Visible;
                    lbFromReverb9.Visibility = Visibility.Visible;
                    lbToReverb9.Visibility = Visibility.Visible;
                    plusclick++;
                    break;
                case 9:
                    lbFrom10.Visibility = Visibility.Visible;
                    tbFrom10.Visibility = Visibility.Visible;
                    lbTo10.Visibility = Visibility.Visible;
                    tbTo10.Visibility = Visibility.Visible;
                    lbPitch10.Visibility = Visibility.Visible;
                    tbPitch10.Visibility = Visibility.Visible;
                    lbVol10.Visibility = Visibility.Visible;
                    tbVolume10.Visibility = Visibility.Visible;
                    lbReverb10.Visibility = Visibility.Visible;
                    tbReverb10.Visibility = Visibility.Visible;
                    tbReverbHFRTR10.Visibility = Visibility.Visible;
                    tbFromReverb10.Visibility = Visibility.Visible;
                    tbToReverb10.Visibility = Visibility.Visible;
                    lbFromReverb10.Visibility = Visibility.Visible;
                    lbToReverb10.Visibility = Visibility.Visible;
                    btnPlus.IsEnabled = false;
                    break;
            }

        }

        private void tbDiapMinus()
        {
            switch (plusclick)
            {
                case 9:
                    lbFrom10.Visibility = Visibility.Hidden;
                    tbFrom10.Visibility = Visibility.Hidden;
                    lbTo10.Visibility = Visibility.Hidden;
                    tbTo10.Visibility = Visibility.Hidden;
                    lbPitch10.Visibility = Visibility.Hidden;
                    tbPitch10.Visibility = Visibility.Hidden;
                    lbVol10.Visibility = Visibility.Hidden;
                    tbVolume10.Visibility = Visibility.Hidden;
                    lbReverb10.Visibility = Visibility.Hidden;
                    tbReverb10.Visibility = Visibility.Hidden;
                    tbReverbHFRTR10.Visibility = Visibility.Hidden;
                    tbFromReverb10.Visibility = Visibility.Hidden;
                    tbToReverb10.Visibility = Visibility.Hidden;
                    lbFromReverb10.Visibility = Visibility.Hidden;
                    lbToReverb10.Visibility = Visibility.Hidden;
                    btnPlus.IsEnabled = true;
                    plusclick--;
                    break;
                case 8:
                    lbFrom9.Visibility = Visibility.Hidden;
                    tbFrom9.Visibility = Visibility.Hidden;
                    lbTo9.Visibility = Visibility.Hidden;
                    tbTo9.Visibility = Visibility.Hidden;
                    lbPitch9.Visibility = Visibility.Hidden;
                    tbPitch9.Visibility = Visibility.Hidden;
                    lbVol9.Visibility = Visibility.Hidden;
                    tbVolume9.Visibility = Visibility.Hidden;
                    lbReverb9.Visibility = Visibility.Hidden;
                    tbReverb9.Visibility = Visibility.Hidden;
                    tbReverbHFRTR9.Visibility = Visibility.Hidden;
                    tbFromReverb9.Visibility = Visibility.Hidden;
                    tbToReverb9.Visibility = Visibility.Hidden;
                    lbFromReverb9.Visibility = Visibility.Hidden;
                    lbToReverb9.Visibility = Visibility.Hidden;
                    plusclick--;
                    break;
                case 7:
                    lbFrom8.Visibility = Visibility.Hidden;
                    tbFrom8.Visibility = Visibility.Hidden;
                    lbTo8.Visibility = Visibility.Hidden;
                    tbTo8.Visibility = Visibility.Hidden;
                    lbPitch8.Visibility = Visibility.Hidden;
                    tbPitch8.Visibility = Visibility.Hidden;
                    lbVol8.Visibility = Visibility.Hidden;
                    tbVolume8.Visibility = Visibility.Hidden;
                    lbReverb8.Visibility = Visibility.Hidden;
                    tbReverb8.Visibility = Visibility.Hidden;
                    tbReverbHFRTR8.Visibility = Visibility.Hidden;
                    tbFromReverb8.Visibility = Visibility.Hidden;
                    tbToReverb8.Visibility = Visibility.Hidden;
                    lbFromReverb8.Visibility = Visibility.Hidden;
                    lbToReverb8.Visibility = Visibility.Hidden;
                    plusclick--;
                    break;
                case 6:
                    lbFrom7.Visibility = Visibility.Hidden;
                    tbFrom7.Visibility = Visibility.Hidden;
                    lbTo7.Visibility = Visibility.Hidden;
                    tbTo7.Visibility = Visibility.Hidden;
                    lbPitch7.Visibility = Visibility.Hidden;
                    tbPitch7.Visibility = Visibility.Hidden;
                    lbVol7.Visibility = Visibility.Hidden;
                    tbVolume7.Visibility = Visibility.Hidden;
                    lbReverb7.Visibility = Visibility.Hidden;
                    tbReverb7.Visibility = Visibility.Hidden;
                    tbReverbHFRTR7.Visibility = Visibility.Hidden;
                    tbFromReverb7.Visibility = Visibility.Hidden;
                    tbToReverb7.Visibility = Visibility.Hidden;
                    lbFromReverb7.Visibility = Visibility.Hidden;
                    lbToReverb7.Visibility = Visibility.Hidden;
                    plusclick--;
                    break;
                case 5:
                    lbFrom6.Visibility = Visibility.Hidden;
                    tbFrom6.Visibility = Visibility.Hidden;
                    lbTo6.Visibility = Visibility.Hidden;
                    tbTo6.Visibility = Visibility.Hidden;
                    lbPitch6.Visibility = Visibility.Hidden;
                    tbPitch6.Visibility = Visibility.Hidden;
                    lbVol6.Visibility = Visibility.Hidden;
                    tbVolume6.Visibility = Visibility.Hidden;
                    lbReverb6.Visibility = Visibility.Hidden;
                    tbReverb6.Visibility = Visibility.Hidden;
                    tbReverbHFRTR6.Visibility = Visibility.Hidden;
                    tbFromReverb6.Visibility = Visibility.Hidden;
                    tbToReverb6.Visibility = Visibility.Hidden;
                    lbFromReverb6.Visibility = Visibility.Hidden;
                    lbToReverb6.Visibility = Visibility.Hidden;
                    plusclick--;
                    break;
                case 4:
                    lbFrom5.Visibility = Visibility.Hidden;
                    tbFrom5.Visibility = Visibility.Hidden;
                    lbTo5.Visibility = Visibility.Hidden;
                    tbTo5.Visibility = Visibility.Hidden;
                    lbPitch5.Visibility = Visibility.Hidden;
                    tbPitch5.Visibility = Visibility.Hidden;
                    lbVol5.Visibility = Visibility.Hidden;
                    tbVolume5.Visibility = Visibility.Hidden;
                    lbReverb5.Visibility = Visibility.Hidden;
                    tbReverb5.Visibility = Visibility.Hidden;
                    tbReverbHFRTR5.Visibility = Visibility.Hidden;
                    tbFromReverb5.Visibility = Visibility.Hidden;
                    tbToReverb5.Visibility = Visibility.Hidden;
                    lbFromReverb5.Visibility = Visibility.Hidden;
                    lbToReverb5.Visibility = Visibility.Hidden;
                    plusclick--;
                    break;
                case 3:
                    lbFrom4.Visibility = Visibility.Hidden;
                    tbFrom4.Visibility = Visibility.Hidden;
                    lbTo4.Visibility = Visibility.Hidden;
                    tbTo4.Visibility = Visibility.Hidden;
                    lbPitch4.Visibility = Visibility.Hidden;
                    tbPitch4.Visibility = Visibility.Hidden;
                    lbVol4.Visibility = Visibility.Hidden;
                    tbVolume4.Visibility = Visibility.Hidden;
                    lbReverb4.Visibility = Visibility.Hidden;
                    tbReverb4.Visibility = Visibility.Hidden;
                    tbReverbHFRTR4.Visibility = Visibility.Hidden;
                    tbFromReverb4.Visibility = Visibility.Hidden;
                    tbToReverb4.Visibility = Visibility.Hidden;
                    lbFromReverb4.Visibility = Visibility.Hidden;
                    lbToReverb4.Visibility = Visibility.Hidden;
                    plusclick--;
                    break;
                case 2:
                    lbFrom3.Visibility = Visibility.Hidden;
                    tbFrom3.Visibility = Visibility.Hidden;
                    lbTo3.Visibility = Visibility.Hidden;
                    tbTo3.Visibility = Visibility.Hidden;
                    lbPitch3.Visibility = Visibility.Hidden;
                    tbPitch3.Visibility = Visibility.Hidden;
                    lbVol3.Visibility = Visibility.Hidden;
                    tbVolume3.Visibility = Visibility.Hidden;
                    lbReverb3.Visibility = Visibility.Hidden;
                    tbReverb3.Visibility = Visibility.Hidden;
                    tbReverbHFRTR3.Visibility = Visibility.Hidden;
                    tbFromReverb3.Visibility = Visibility.Hidden;
                    tbToReverb3.Visibility = Visibility.Hidden;
                    lbFromReverb3.Visibility = Visibility.Hidden;
                    lbToReverb3.Visibility = Visibility.Hidden;
                    plusclick--;
                    break;
                case 1:
                    lbFrom2.Visibility = Visibility.Hidden;
                    tbFrom2.Visibility = Visibility.Hidden;
                    lbTo2.Visibility = Visibility.Hidden;
                    tbTo2.Visibility = Visibility.Hidden;
                    lbPitch2.Visibility = Visibility.Hidden;
                    tbPitch2.Visibility = Visibility.Hidden;
                    lbVol2.Visibility = Visibility.Hidden;
                    tbVolume2.Visibility = Visibility.Hidden;
                    lbReverb2.Visibility = Visibility.Hidden;
                    tbReverb2.Visibility = Visibility.Hidden;
                    tbReverbHFRTR2.Visibility = Visibility.Hidden;
                    tbFromReverb2.Visibility = Visibility.Hidden;
                    tbToReverb2.Visibility = Visibility.Hidden;
                    lbFromReverb2.Visibility = Visibility.Hidden;
                    lbToReverb2.Visibility = Visibility.Hidden;
                    plusclick--;
                    break;
                case 0:
                    lbFrom1.Visibility = Visibility.Hidden;
                    tbFrom1.Visibility = Visibility.Hidden;
                    lbTo1.Visibility = Visibility.Hidden;
                    tbTo1.Visibility = Visibility.Hidden;
                    lbPitch1.Visibility = Visibility.Hidden;
                    tbPitch1.Visibility = Visibility.Hidden;
                    lbVol1.Visibility = Visibility.Hidden;
                    tbVolume1.Visibility = Visibility.Hidden;
                    lbReverb1.Visibility = Visibility.Hidden;
                    tbReverb1.Visibility = Visibility.Hidden;
                    tbReverbHFRTR1.Visibility = Visibility.Hidden;
                    tbFromReverb1.Visibility = Visibility.Hidden;
                    tbToReverb1.Visibility = Visibility.Hidden;
                    lbFromReverb1.Visibility = Visibility.Hidden;
                    lbToReverb1.Visibility = Visibility.Hidden;
                    lbZnachPitch.Visibility = Visibility.Hidden;
                    btnFix.IsEnabled = false;
                    btnMinus.IsEnabled = false;
                    break;
            }
        }

        private void SoundOut()
        {
            if (cmbOutput.SelectedIndex == 0)
            {
                mSoundOut = new WasapiOut(/*false, AudioClientShareMode.Exclusive, 1*/);
                mSoundOut.Device = mOutputDevices[cmbOutput.SelectedIndex];
                mSoundOut.Initialize(mMixer.ToWaveSource(16));

                mSoundOut.Play();
            }
            else if (cmbSelEff.SelectedIndex == 1)
            {
                mSoundOut = new WasapiOut(false, AudioClientShareMode.Exclusive, 1);
                mSoundOut.Device = mOutputDevices[cmbOutput.SelectedIndex];
                mSoundOut.Initialize(mMixer.ToWaveSource(16));

                //Start rolling!
                mSoundOut.Play();
            }
            else if (cmbSelEff.SelectedIndex == 2)
            {
                mSoundOut = new WasapiOut(false, AudioClientShareMode.Exclusive, 1);
                mSoundOut.Device = mOutputDevices[cmbOutput.SelectedIndex];
                mSoundOut.Initialize(mMixer.ToWaveSource(16));

                //Start rolling!
                mSoundOut.Play();
            }
        }

        private void Mixer()
        {
            mMixer = new SimpleMixer(2, SampleRate) //стерео, 44,1 КГц
            {
                FillWithZeros = true,
                DivideResult = true, //Для этого установлено значение true, чтобы избежать звуков тиков из-за превышения -1 и 1.
            };
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            StopFullDuplex();
            trackGain.IsEnabled = false;
            trackPitch.IsEnabled = false;
            btnReset.IsEnabled = false;
        }

        private void StopFullDuplex()
        {
            if (mSoundOut != null) mSoundOut.Dispose();
            if (mSoundIn != null) mSoundIn.Dispose();
        }

        private void SetPitchShiftValue()
        {
            for (int i = 0; i < plusclick; i++)
            {
                mDsp.PitchShift = (float)Math.Pow(2.0F, Pitch[i] / 13.0F);
                //mDspR.PitchShift = (float)Math.Pow(2.0F, trackPitch.Value / 13.0F);
            }
        }

        private void btnPlus_Click(object sender, RoutedEventArgs e)
        {
            tbDiapPlus();
        }

        private void btnMinus_Click(object sender, RoutedEventArgs e)
        {
            tbDiapMinus();
        }

        private void PitchValue()
        {
            lbPitchValue.Content = trackPitch.Value.ToString();
        }

        private void VolValue()
        {
            lbVolValue.Content = trackGain.Value.ToString();
        }

        public ISampleSource BandPassFilter(WasapiCapture mSoundIn, int sampleRate, int bottomFreq, int topFreq)
        {
            var sampleSource = new SoundInSource(mSoundIn) { FillWithZeros = true }
                    .ChangeSampleRate(sampleRate).ToStereo().ToSampleSource();
            var tempFilter = sampleSource.AppendSource(x => new BiQuadFilterSource(x));
            tempFilter.Filter = new HighpassFilter(sampleRate, bottomFreq);
            var filteredSource = tempFilter.AppendSource(x => new BiQuadFilterSource(x));
            filteredSource.Filter = new LowpassFilter(sampleRate, topFreq);

            return filteredSource;
        }

        public bool isDataValid(int[] botFreq, int[] topFreq, int[] reverbTime, int[] reverbHFRTR, int size)
        {
            for (int i = 0; i < size; i++)
            {
                if (botFreq[i] > SampleRate / 2 || topFreq[i] > SampleRate / 2)
                {
                    MessageBox.Show("Частоты полосового фильтра не могут быть больше половины частоты дискретизации ("
                        + Convert.ToString(SampleRate / 2) + ")");
                    return false;
                }
                if (reverbTime[i] > 3000)
                {
                    MessageBox.Show("Время реверберации не может быть выше 3000мс");
                    return false;
                }
                if (botFreq[i] >= topFreq[i])
                {
                    MessageBox.Show("Нижняя частота полосового фильтра должна быть меньше верхней");
                    return false;
                }
                if (reverbHFRTR[i] > 999 || reverbHFRTR[i] < 1)
                {
                    MessageBox.Show("Не может быть выше 999 и ниже 1");
                    return false;
                }
            }
            return true;
        }


    }
}
