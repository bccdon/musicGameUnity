using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.IO;

namespace PulseHighway.Audio
{
    public static class AudioFileLoader
    {
        public const float MAX_DURATION = 600f; // 10 minutes
        public const long MAX_FILE_SIZE = 50 * 1024 * 1024; // 50MB

        public static readonly string[] SupportedExtensions = { ".mp3", ".wav", ".ogg", ".flac" };

        public static bool IsSupported(string path)
        {
            string ext = Path.GetExtension(path).ToLowerInvariant();
            return Array.Exists(SupportedExtensions, e => e == ext);
        }

        public static AudioType GetAudioType(string path)
        {
            string ext = Path.GetExtension(path).ToLowerInvariant();
            return ext switch
            {
                ".mp3" => AudioType.MPEG,
                ".wav" => AudioType.WAV,
                ".ogg" => AudioType.OGGVORBIS,
                ".flac" => AudioType.WAV, // Unity doesn't natively support FLAC; treat as WAV
                _ => AudioType.UNKNOWN
            };
        }

        /// <summary>
        /// Load an audio file from a local file path. Call via StartCoroutine.
        /// </summary>
        public static IEnumerator LoadFromFile(string filePath, Action<AudioClip, string> onComplete)
        {
            if (!File.Exists(filePath))
            {
                onComplete?.Invoke(null, $"File not found: {filePath}");
                yield break;
            }

            if (!IsSupported(filePath))
            {
                onComplete?.Invoke(null, $"Unsupported format: {Path.GetExtension(filePath)}");
                yield break;
            }

            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Length > MAX_FILE_SIZE)
            {
                onComplete?.Invoke(null, $"File too large: {fileInfo.Length / (1024 * 1024)}MB (max {MAX_FILE_SIZE / (1024 * 1024)}MB)");
                yield break;
            }

            string uri = "file:///" + filePath.Replace("\\", "/");
            var audioType = GetAudioType(filePath);

            using var request = UnityWebRequestMultimedia.GetAudioClip(uri, audioType);
            ((DownloadHandlerAudioClip)request.downloadHandler).streamAudio = false;

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                onComplete?.Invoke(null, $"Load failed: {request.error}");
                yield break;
            }

            var clip = DownloadHandlerAudioClip.GetContent(request);
            if (clip == null)
            {
                onComplete?.Invoke(null, "Failed to decode audio");
                yield break;
            }

            clip.name = Path.GetFileNameWithoutExtension(filePath);

            // Validate duration
            if (clip.length > MAX_DURATION)
            {
                onComplete?.Invoke(null, $"Audio too long: {clip.length:F0}s (max {MAX_DURATION:F0}s)");
                yield break;
            }

            onComplete?.Invoke(clip, null);
        }

        /// <summary>
        /// Load audio from a URL. Call via StartCoroutine.
        /// </summary>
        public static IEnumerator LoadFromUrl(string url, Action<AudioClip, string> onComplete,
            Action<float> onProgress = null)
        {
            if (string.IsNullOrEmpty(url))
            {
                onComplete?.Invoke(null, "URL is empty");
                yield break;
            }

            // Guess audio type from URL
            var audioType = GetAudioType(url);
            if (audioType == AudioType.UNKNOWN)
                audioType = AudioType.MPEG; // Default to MP3

            using var request = UnityWebRequestMultimedia.GetAudioClip(url, audioType);
            var operation = request.SendWebRequest();

            while (!operation.isDone)
            {
                onProgress?.Invoke(request.downloadProgress);
                yield return null;
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                onComplete?.Invoke(null, $"Download failed: {request.error}");
                yield break;
            }

            var clip = DownloadHandlerAudioClip.GetContent(request);
            if (clip == null)
            {
                onComplete?.Invoke(null, "Failed to decode audio from URL");
                yield break;
            }

            clip.name = "Online Track";

            if (clip.length > MAX_DURATION)
            {
                onComplete?.Invoke(null, $"Audio too long: {clip.length:F0}s (max {MAX_DURATION:F0}s)");
                yield break;
            }

            onComplete?.Invoke(clip, null);
        }

        /// <summary>
        /// Extract mono samples from an AudioClip.
        /// </summary>
        public static float[] GetMonoSamples(AudioClip clip)
        {
            float[] data = new float[clip.samples * clip.channels];
            clip.GetData(data, 0);

            if (clip.channels == 1) return data;

            // Convert to mono
            float[] mono = new float[clip.samples];
            for (int i = 0; i < clip.samples; i++)
            {
                float sum = 0f;
                for (int ch = 0; ch < clip.channels; ch++)
                    sum += data[i * clip.channels + ch];
                mono[i] = sum / clip.channels;
            }
            return mono;
        }
    }
}
