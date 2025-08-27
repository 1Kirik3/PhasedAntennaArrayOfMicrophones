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
            // Создаем модели
            var envSettings = new EnvironmentSettings();
            var audioSource = new AudioSource();
            var micArray = new MicrophoneArray(envSettings, audioSource);

            // Создаем сетевые сервисы
            var crcCalculator = new Crc16Calculator();
            var packetBuilder = new PacketBuilder(crcCalculator);

            // Создаем UdpNetworkService с передачей packetBuilder
            var networkService = new UdpNetworkService(packetBuilder);

            // Создаем ViewModel
            var settingsViewModel = new SettingsViewModel(
                envSettings,
                audioSource,
                micArray,
                networkService);

            // Создаем главное окно
            var mainWindow = new MainWindow();
            mainWindow.DataContext = settingsViewModel;
            mainWindow.Show();
        }
    }
}