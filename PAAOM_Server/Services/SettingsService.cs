// SettingsService.cs
using PAAOM_Common.Models;
using PAAOM_Common.Models.Interfaces;
using PAAOM_Server.ViewModels;
using System;
using System.IO;
using System.Text.Json;

namespace PAAOM_Server.Services
{
    public class SettingsService : ISettingsService
    {
        private const string SettingsFileName = "appsettings.json";
        private readonly JsonSerializerOptions _jsonOptions;

        public SettingsService()
        {
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public async Task<AppSettings> LoadSettingsAsync()
        {
            try
            {
                if (!File.Exists(SettingsFileName))
                {
                    Console.WriteLine("Файл настроек не существует, используются настройки по умолчанию");
                    return new AppSettings();
                }

                var json = await File.ReadAllTextAsync(SettingsFileName);
                var settings = JsonSerializer.Deserialize<AppSettings>(json, _jsonOptions) ?? new AppSettings();

                Console.WriteLine($"Загружены настройки AudioSource: " +
                                 $"Pos={settings.AudioSource?.Position}, " +
                                 $"Freq={settings.AudioSource?.Frequency}, " +
                                 $"Amp={settings.AudioSource?.Amplitude}, " +
                                 $"Phase={settings.AudioSource?.Phase}");

                return settings;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки настроек: {ex.Message}");
                return new AppSettings();
            }
        }

        public async Task SaveSettingsAsync(AppSettings settings)
        {
            try
            {
                // Добавляем отладочный вывод
                Console.WriteLine("=== SAVING SETTINGS ===");
                Console.WriteLine($"AudioSource: Pos={settings.AudioSource.Position}, " +
                                 $"Freq={settings.AudioSource.Frequency}, " +
                                 $"Amp={settings.AudioSource.Amplitude}, " +
                                 $"Phase={settings.AudioSource.Phase}");

                var json = JsonSerializer.Serialize(settings, _jsonOptions);
                await File.WriteAllTextAsync(SettingsFileName, json);

                Console.WriteLine("=== SETTINGS SAVED ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка сохранения настроек: {ex.Message}");
                throw;
            }
        }

        public async Task ApplySettingsAsync(AppSettings settings, IEnvironment envSettings,
            IAudioSource audioSource, IMicrophoneArray microphoneArray)
        {
            await ApplySettingsAsync(settings, envSettings, audioSource, microphoneArray, null);
        }

        public async Task ApplySettingsAsync(AppSettings settings, IEnvironment envSettings,
    IAudioSource audioSource, IMicrophoneArray microphoneArray,
    Action onSettingsApplied = null)
        {
            Console.WriteLine("=== APPLYING SETTINGS ===");
            Console.WriteLine($"AudioSource: Pos={settings.AudioSource.Position}, " +
                             $"Freq={settings.AudioSource.Frequency}, " +
                             $"Amp={settings.AudioSource.Amplitude}, " +
                             $"Phase={settings.AudioSource.Phase}");

            envSettings.TemperatureCelsius = settings.Environment.TemperatureCelsius;
            envSettings.NoiseLevel = settings.Environment.NoiseLevel;

            audioSource.Position = settings.AudioSource.Position;
            audioSource.Frequency = settings.AudioSource.Frequency;
            audioSource.Amplitude = settings.AudioSource.Amplitude;
            audioSource.Phase = settings.AudioSource.Phase;

            microphoneArray.ArrayCenter = settings.MicrophoneArray.ArrayCenter;
            microphoneArray.Radius = settings.MicrophoneArray.Radius;
            microphoneArray.MicrophonesCount = settings.MicrophoneArray.MicrophonesCount;

            Console.WriteLine("=== SETTINGS APPLIED ===");

            // Вызываем callback если он предоставлен
            onSettingsApplied?.Invoke();

            NotifyPropertiesChanged(envSettings, audioSource, microphoneArray);

            await Task.CompletedTask;
        }

        private void NotifyPropertiesChanged(IEnvironment env, IAudioSource audio, IMicrophoneArray array)
        {
            Console.WriteLine("Properties changed - UI should be updated");
        }

        public AppSettings CreateSettingsFromModels(IEnvironment envSettings,
    IAudioSource audioSource, IMicrophoneArray microphoneArray)
        {
            return new AppSettings
            {
                AudioSource = new AudioSourceSettings
                {
                    Position = audioSource.Position,
                    Frequency = audioSource.Frequency,
                    Amplitude = audioSource.Amplitude, 
                    Phase = audioSource.Phase          
                },
                Environment = new EnvironmentSettings
                {
                    TemperatureCelsius = envSettings.TemperatureCelsius,
                    NoiseLevel = envSettings.NoiseLevel
                },
                MicrophoneArray = new MicrophoneArraySettings
                {
                    ArrayCenter = microphoneArray.ArrayCenter,
                    Radius = microphoneArray.Radius,
                    MicrophonesCount = microphoneArray.MicrophonesCount
                },
                Network = new NetworkSettings()
            };
        }

        public AppSettings CreateSettingsFromModels(IEnvironment envSettings,
    IAudioSource audioSource, IMicrophoneArray microphoneArray,
    NetworkSettingsViewModel networkSettings)
        {
            return new AppSettings
            {
                AudioSource = new AudioSourceSettings
                {
                    Position = audioSource.Position,
                    Frequency = audioSource.Frequency,
                    Amplitude = audioSource.Amplitude, 
                    Phase = audioSource.Phase         
                },
                Environment = new EnvironmentSettings
                {
                    TemperatureCelsius = envSettings.TemperatureCelsius,
                    NoiseLevel = envSettings.NoiseLevel
                },
                MicrophoneArray = new MicrophoneArraySettings
                {
                    ArrayCenter = microphoneArray.ArrayCenter,
                    Radius = microphoneArray.Radius,
                    MicrophonesCount = microphoneArray.MicrophonesCount
                },
                Network = new NetworkSettings
                {
                    LocalIP = networkSettings.LocalIP,
                    LocalPort = networkSettings.LocalPort,
                    RemoteIP = networkSettings.RemoteIP,
                    RemotePort = networkSettings.RemotePort
                }
            };
        }
    }

}