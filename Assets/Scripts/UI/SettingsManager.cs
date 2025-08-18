using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

//We need just a few settings (mostly Sound and Music)
public class SettingsManager : MonoBehaviour
{
	public Slider soundSlider, musicSlider;
	
	public AudioMixer soundMixer, musicMixer;
	
	private float music, sound;
	
	private void Start()
	{
		Load();
	}
	
	public void SetSound(float value)
	{
		sound = value;
		soundMixer.SetFloat("Vol", sound);
	}
	
	public void SetMusic(float value)
	{
		music = value;
		musicMixer.SetFloat("Vol", music);
	}
	
	public void Save()
	{
		PlayerPrefs.SetFloat("music_vol", music);
		PlayerPrefs.SetFloat("sound_vol", sound);
	}
	
	public void Load()
	{
		music = PlayerPrefs.GetFloat("music_vol", 0f);
		sound = PlayerPrefs.GetFloat("music_vol", 0f);
		
		soundSlider.value = sound;
		musicSlider.value = music;
	}
}
