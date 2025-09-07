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
            var envSettings = new EnvironmentSettings();
            var audioSource = new AudioSource();
            var micArray = new MicrophoneArray(envSettings, audioSource);

            var crcCalculator = new Crc16Calculator();
            var packetBuilder = new PacketBuilder(crcCalculator);

            var networkService = new UdpNetworkService(packetBuilder);

            var settingsViewModel = new SettingsViewModel(
                envSettings,
                audioSource,
                micArray,
                networkService);

            var mainWindow = new MainWindow();
            mainWindow.DataContext = settingsViewModel;
            mainWindow.Show();
        }
    }
}