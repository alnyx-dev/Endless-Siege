using UnityEngine;

namespace Game.Utils
{
    [RequireComponent(typeof(AudioSource))]
    public class MusicPlayer : MonoBehaviour
    {
        [SerializeField] private AudioClip musicClip;
        [SerializeField, Range(0f, 1f)] private float volume = 0.5f;

        private AudioSource _source;

        private void Awake()
        {
            _source = GetComponent<AudioSource>();
            _source.clip = musicClip;
            _source.loop = true;
            _source.playOnAwake = false;
            _source.volume = volume;
            _source.Play();
        }
    }
}
