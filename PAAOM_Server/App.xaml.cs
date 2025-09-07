using Microsoft.Extensions.Logging;
using PAAOM_Common.Models;
using PAAOM_Common.Network.Interfaces;
using PAAOM_Common.Network.Services;
using PAAOM_Server.ViewModels;
using System.Windows;

namespace PAAOM_Server
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Инициализация моделей
            var envSettings = new EnvironmentSettings
            {
                TemperatureCelsius = 20.0f,
                NoiseLevel = 0.05f
            };

            var audioSource = new AudioSource
            {
                Frequency = 500f,
                Amplitude = 1.0f,
                Position = new System.Windows.Media.Media3D.Point3D(5, 5, 0)
            };

            var micArray = new MicrophoneArray(envSettings, audioSource)
            {
                Radius = 0.5f,
                ArrayCenter = new System.Windows.Media.Media3D.Point3D(0, 0, 0)
            };

            // Инициализация сетевого сервиса
            var crcCalculator = new Crc16Calculator();
            var packetBuilder = new PacketBuilder(crcCalculator);
            var networkService = new UdpNetworkService(packetBuilder);

            // Создание ViewModel
            var settingsViewModel = new SettingsViewModel(
                envSettings,
                audioSource,
                micArray,
                networkService);

            // Создание и отображение главного окна
            var mainWindow = new MainWindow();
            mainWindow.DataContext = settingsViewModel;
            mainWindow.Show();
        }
    }
}