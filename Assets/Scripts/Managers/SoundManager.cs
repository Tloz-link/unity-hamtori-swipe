using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    AudioSource[] _audioSources = new AudioSource[(int)Define.Sound.MaxCount];
    Dictionary<string, AudioClip> _audioClips = new Dictionary<string, AudioClip>();
    // Player -> AudioSource -> PlayOneShot = 다른 클립을 실행하더라도 이 클립은 한번은 실행시켜라
    // 음원 -> AudioClip
    // 귀 -> AudioListener -> 디폴트로 카메라에 넣어져있음 -> Scene에 1개만 있으면 됨

    static List<float> _playEffectList = new List<float>();

    public void Init()
    {
        GameObject root = GameObject.Find("@Sound");
        if (root == null)
        {
            root = new GameObject { name = "@Sound" };
            root.AddComponent<SoundManager>();
            Object.DontDestroyOnLoad(root);

            string[] soundNames = System.Enum.GetNames(typeof(Define.Sound));
            for (int i = 0; i < soundNames.Length - 1; ++i)
            {
                GameObject go = new GameObject { name = soundNames[i] };
                _audioSources[i] = go.AddComponent<AudioSource>();
                go.transform.parent = root.transform;
            }

            _audioSources[(int)Define.Sound.Bgm].loop = true;
        }
    }

    private void Update()
    {
        List<float> newList = new List<float>();
        foreach (float len in _playEffectList)
        {
            float newLen = len - Time.deltaTime;
            if (newLen > 0.0f)
                newList.Add(newLen);
        }
        _playEffectList = newList;
    }

    public void Clear()
    {
        foreach (AudioSource audioSource in _audioSources)
        {
            audioSource.Stop();
            audioSource.clip = null;
        }
        _audioClips.Clear();
    }

    public void AdjustVolume(float volume)
    {
        foreach (AudioSource audioSource in _audioSources)
            audioSource.volume = volume;
    }

    public bool Play(Define.Sound type, string path, float pitch = 1.0f)
    {
        if (string.IsNullOrEmpty(path))
            return false;

        AudioSource audioSource = _audioSources[(int)type];

        if (path.Contains("Sounds/") == false)
            path = string.Format("Sounds/{0}", path);

        audioSource.volume = Managers.Game.Volume;

        if (type == Define.Sound.Bgm)
        {
            AudioClip audioClip = Managers.Resource.Load<AudioClip>(path);
            if (audioClip == null)
                return false;

            if (audioSource.isPlaying)
                audioSource.Stop();

            audioSource.clip = audioClip;
            audioSource.pitch = pitch;
            audioSource.Play();
            return true;
        }
        else if (type == Define.Sound.Effect)
        {
            AudioClip audioClip = GetAudioClip(path);
            if (audioClip == null)
                return false;

            if (_playEffectList.Count < Define.MAX_SOUND_OVERLAPPED || Time.timeScale == 0)
            {
                _playEffectList.Add(0.04f);

                audioSource.pitch = pitch;
                audioSource.PlayOneShot(audioClip);
                return true;
            }
        }

        return false;
    }

    public void Stop(Define.Sound type)
    {
        AudioSource audioSource = _audioSources[(int)type];
        audioSource.Stop();
    }

    private AudioClip GetAudioClip(string path)
    {
        AudioClip audioClip = null;
        if (_audioClips.TryGetValue(path, out audioClip))
            return audioClip;

        audioClip = Managers.Resource.Load<AudioClip>(path);
        _audioClips.Add(path, audioClip);
        return audioClip;
    }
}
