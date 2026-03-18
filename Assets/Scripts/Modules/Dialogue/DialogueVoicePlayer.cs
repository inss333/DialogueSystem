using System.Collections.Generic;
using UnityEngine;

namespace D1
{
    public class DialogueVoicePlayer : MonoBehaviour
    {
        private readonly Dictionary<string, AudioClip> _clipCache = new();
        private AudioSource _audioSource;

        public void Init()
        {
            _audioSource = GetComponent<AudioSource>();
            if (!_audioSource)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }

            _audioSource.playOnAwake = false;
            _audioSource.loop = false;
        }

        public void Play(string voicePath)
        {
            if (string.IsNullOrWhiteSpace(voicePath))
            {
                Stop();
                return;
            }

            if (!_clipCache.TryGetValue(voicePath, out var clip))
            {
                clip = Resources.Load<AudioClip>(voicePath);
                _clipCache[voicePath] = clip;
            }

            _audioSource.clip = clip;
            if (clip)
            {
                _audioSource.Play();
                return;
            }

            _audioSource.Stop();
        }

        public void Stop()
        {
            _audioSource.Stop();
            _audioSource.clip = null;
        }
    }
}
