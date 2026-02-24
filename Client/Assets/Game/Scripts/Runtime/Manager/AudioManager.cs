using System.Collections.Generic;
using ResKit;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace Gameplay
{
    public class AudioManager : SingletonMono<AudioManager>
    {
        #region Audio
        
        private AudioSource _audioSource;
        
        /// <summary>
        /// 播放音效
        /// </summary>
        public void PlaySound(string soundName)
        {
            if (string.IsNullOrEmpty(soundName)) return;
            AudioClip clip = GetSound(soundName);
            if (_audioSource == null) _audioSource = transform.AddComponent<AudioSource>();
            _audioSource.clip = clip;
            _audioSource.playOnAwake = false;
            // 2D音效
            _audioSource.spatialBlend = 0f; 
            _audioSource.Play();
        }

        public void Play3DSound(string soundName)
        {
            if (string.IsNullOrEmpty(soundName)) return;
            AudioClip clip = GetSound(soundName);
            if (_audioSource == null) _audioSource = transform.AddComponent<AudioSource>();
            _audioSource.clip = clip;
            _audioSource.playOnAwake = false;
            // 半3D音效
            _audioSource.spatialBlend = 0.7f; 
            _audioSource.minDistance = 1f;
            _audioSource.maxDistance = 50f;
            _audioSource.Play();
        }
        
        
        private static readonly string _AudioPath = $"{PathDefine.PATH_RES_PRODUCT_DIR}/Audio/";
        private Dictionary<string, AudioClip> _audioClips = new Dictionary<string, AudioClip>();
        
        public AudioClip GetSound(string soundName)
        {
            // 从缓存中获取
            if (_audioClips.TryGetValue(soundName, out var clip))
            {
                return clip;
            }
            else
            {
                // 如果 GetSound 返回 null，尝试从资源加载
                string fullPath = $"{_AudioPath}{soundName}.wav";
                clip = ResourceManager.Instance.Load<AudioClip>(fullPath);
                if (clip != null)
                {
                    _audioClips.Add(soundName, clip);
                }
                else
                {
                    Debug.LogWarning($"未找到音效: {soundName}");
                }
            }

            return clip;
        }
        
        #endregion
    }
}