using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Gameplay
{
    public class AudioManager : Singleton<AudioManager>
    {
        #region Audio

        private GameObject _audioSourcePool;
        public Transform AudioSourcePool
        {
            get
            {
                if (_audioSourcePool == null)
                {
                    _audioSourcePool = new GameObject("AudioSourcePool");
                }
                return _audioSourcePool.transform;
            }
        }

        private Dictionary<string, AudioClip> _audioClips = new Dictionary<string, AudioClip>();

        /// <summary>
        /// 播放音效
        /// </summary>
        public void PlaySound(string soundName, Vector3 position)
        {
            if (string.IsNullOrEmpty(soundName)) return;

            AudioClip clip = null;

            // 从缓存中获取
            if (_audioClips.ContainsKey(soundName))
            {
                clip = _audioClips[soundName];
            }
            else
            {
                // 尝试从已有方法获取
                clip = GetSound(soundName);

                if (clip == null)
                {
                    // 如果 GetSound 返回 null，尝试从资源加载
                    const string audioPath = "Assets/Res/Audio/";
                    string fullPath = audioPath + soundName + ".wav";

#if UNITY_EDITOR
                    clip = AssetDatabase.LoadAssetAtPath<AudioClip>(fullPath);
#else
                    clip = ResourceManager.Instance.GetAsset<AudioClip>(fullPath);
#endif
                }

                if (clip != null)
                {
                    _audioClips[soundName] = clip;
                }
                else
                {
                    Debug.LogWarning($"未找到音效: {soundName}");
                    return;
                }
            }

            // 创建临时的 AudioSource 播放音效
            GameObject audioObj = new GameObject("OneShotAudio");
            audioObj.transform.position = position;
            audioObj.transform.SetParent(AudioSourcePool);

            AudioSource source = audioObj.AddComponent<AudioSource>();
            source.clip = clip;
            source.playOnAwake = false;

            // 根据名称判断是否为3D音效
            if (soundName.Contains("Fire") || soundName.Contains("Hit"))
            {
                source.spatialBlend = 0.7f; // 半3D音效
                source.minDistance = 1f;
                source.maxDistance = 50f;
            }
            else
            {
                source.spatialBlend = 0f; // 2D音效
            }

            source.Play();
            Object.Destroy(audioObj, clip.length + 0.1f);
        }

        public AudioClip GetSound(string soundName)
        {
            return null;
        }
        
        #endregion
    }
}