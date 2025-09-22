using PAAOM_Common.Models;
using PAAOM_Common.Network.Services;
using PAAOM_Server.Services;
using PAAOM_Server.ViewModels;
using System.Windows;

namespace PAAOM_Server
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var envSettings = new PAAOM_Common.Models.Environment
            {
                TemperatureCelsius = 20.0f,
                NoiseLevel = 0.05f
            };

            var audioSource = new AudioSource
            {
                Frequency = 500f,
                Amplitude = 1.0f,
                Position = new Point3D(5, 5, 0)
            };

            var micArray = new MicrophoneArray(envSettings, audioSource)
            {
                Radius = 0.5f,
                ArrayCenter = new Point3D(0, 0, 0)
            };

            var crcCalculator = new Crc16Calculator();
            var packetBuilder = new PacketBuilder(crcCalculator);
            var networkService = new UdpNetworkService(packetBuilder);
            var settingsService = new SettingsService();

            var settingsViewModel = new MainSettingsViewModel(
                envSettings,
                audioSource, 
                micArray,    
                networkService,
                settingsService);

            var mainViewModel = new MainViewModel(
                envSettings,
                audioSource, 
                micArray,  
                settingsService);

            var mainWindow = new MainWindow();
            mainWindow.DataContext = settingsViewModel;
            mainWindow.Show();
        }
    }
}