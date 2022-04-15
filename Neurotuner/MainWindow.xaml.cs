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
