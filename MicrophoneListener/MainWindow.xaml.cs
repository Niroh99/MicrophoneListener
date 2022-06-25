using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MicrophoneListener
{
    public partial class MainWindow : CustomWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            var inputDevices = Enumerable.Range(-1, WaveIn.DeviceCount + 1).Select(n => WaveIn.GetCapabilities(n)).ToArray();

            for (int i = 0; i < inputDevices.Length; i++)
            {
                var inputDevice = inputDevices[i];

                InputDevices.Add(WaveCapabilitiesHelpers.GetNameFromGuid(inputDevice.NameGuid) ?? inputDevice.ProductName);
            }

            var outputDevices = Enumerable.Range(-1, WaveOut.DeviceCount + 1).Select(n => WaveOut.GetCapabilities(n)).ToArray();

            for (int i = 0; i < outputDevices.Length; i++)
            {
                var outputDevice = outputDevices[i];

                OutputDevices.Add(WaveCapabilitiesHelpers.GetNameFromGuid(outputDevice.NameGuid) ?? outputDevice.ProductName);
            }
        }

        private string settingsDirectory
        {
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MicrophoneListener"); }
        }

        private string settingsFilePath
        {
            get { return Path.Combine(settingsDirectory, "settings.txt"); }
        }

        bool _isRunning;
        public bool IsRunning
        {
            get { return _isRunning; }
            set { SetProperty(ref _isRunning, value); }
        }

        ObservableCollection<object> _inputDevices;
        public ObservableCollection<object> InputDevices
        {
            get
            {
                if (_inputDevices == null) _inputDevices = new ObservableCollection<object>();

                return _inputDevices;
            }
        }

        ObservableCollection<object> _outputDevices;
        public ObservableCollection<object> OutputDevices
        {
            get
            {
                if (_outputDevices == null) _outputDevices = new ObservableCollection<object>();

                return _outputDevices;
            }
        }

        private void CustomWindow_TemplateApplyed(object sender, EventArgs e)
        {
            if (File.Exists(settingsFilePath))
            {
                var settings = File.ReadAllLines(settingsFilePath);

                if (settings.Length == 3)
                {
                    VolumeSlider.Value = double.Parse(settings[0]);
                    InputDeviceComboBox.SelectedIndex = int.Parse(settings[1]) + 1;
                    OutputDeviceComboBox.SelectedIndex = int.Parse(settings[2]) + 1;
                }
            }

            VolumeSlider.ValueChanged += VolumeSlider_ValueChanged;
            InputDeviceComboBox.SelectionChanged += InputDeviceComboBox_SelectionChanged;
            OutputDeviceComboBox.SelectionChanged += OutputDeviceComboBox_SelectionChanged;
        }

        private WaveIn waveIn;
        private BufferedWaveProvider bufferedWaveProvider;
        private WaveOut waveOut;

        private void OnStartRecordingButton_Click(object sender, RoutedEventArgs e)
        {
            if (waveIn != null) waveIn.Dispose();

            var waveFormat = new WaveFormat(48000, 2);

            // set up the recorder
            waveIn = new WaveIn();
            waveIn.DeviceNumber = InputDeviceComboBox.SelectedIndex - 1;
            waveIn.BufferMilliseconds = 5;
            waveIn.NumberOfBuffers = 3;
            waveIn.WaveFormat = waveFormat;
            waveIn.DataAvailable += RecorderOnDataAvailable;

            // set up our signal chain
            bufferedWaveProvider = new BufferedWaveProvider(waveIn.WaveFormat);

            // set up playback
            waveOut = new WaveOut();
            waveOut.NumberOfBuffers = 3;
            waveOut.DesiredLatency = 50;
            waveOut.DeviceNumber = OutputDeviceComboBox.SelectedIndex - 1;
            waveOut.Volume = (float)VolumeSlider.Value;
            waveOut.Init(bufferedWaveProvider);

            // begin playback & record
            waveOut.Play();
            waveIn.StartRecording();

            IsRunning = true;
        }

        private void RecorderOnDataAvailable(object sender, WaveInEventArgs waveInEventArgs)
        {
            bufferedWaveProvider.AddSamples(waveInEventArgs.Buffer, 0, waveInEventArgs.BytesRecorded);
        }

        private void OnStopRecordingButton_Click(object sender, RoutedEventArgs e)
        {
            StopRecording();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StopRecording();
        }

        private void StopRecording()
        {
            // stop recording
            if (waveIn != null) waveIn.StopRecording();
            // stop playback
            if (waveOut != null) waveOut.Stop();

            IsRunning = false;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (waveOut != null) waveOut.Volume = (float)e.NewValue;

            SaveSettings(e.NewValue, InputDeviceComboBox.SelectedIndex - 1, OutputDeviceComboBox.SelectedIndex - 1);
        }

        private void InputDeviceComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            SaveSettings(VolumeSlider.Value, InputDeviceComboBox.SelectedIndex - 1, OutputDeviceComboBox.SelectedIndex - 1);
        }

        private void OutputDeviceComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            SaveSettings(VolumeSlider.Value, InputDeviceComboBox.SelectedIndex - 1, OutputDeviceComboBox.SelectedIndex - 1);
        }

        private void SaveSettings(double volume, int inputDeviceNumber, int outputDeviceNumber)
        {
            if (!Directory.Exists(settingsDirectory)) Directory.CreateDirectory(settingsDirectory);

            File.WriteAllText(settingsFilePath, string.Join("\n", volume.ToString(), inputDeviceNumber.ToString(), outputDeviceNumber.ToString()));
        }
    }
}