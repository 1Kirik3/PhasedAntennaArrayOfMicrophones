// ISettingsService.cs
using PAAOM_Common.Models;
using PAAOM_Common.Models.Interfaces;
using PAAOM_Server.ViewModels;
using System;

namespace PAAOM_Server.Services
{
    public interface ISettingsService
    {
        Task<AppSettings> LoadSettingsAsync();
        Task SaveSettingsAsync(AppSettings settings);

        Task ApplySettingsAsync(AppSettings settings, IEnvironment envSettings,
            IAudioSource audioSource, IMicrophoneArray microphoneArray,
            Action onSettingsApplied = null);

        AppSettings CreateSettingsFromModels(IEnvironment envSettings,
            IAudioSource audioSource, IMicrophoneArray microphoneArray);

        AppSettings CreateSettingsFromModels(IEnvironment envSettings,
            IAudioSource audioSource, IMicrophoneArray microphoneArray,
            NetworkSettingsViewModel networkSettings);
    }
}