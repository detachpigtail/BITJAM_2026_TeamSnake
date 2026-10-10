using UnityEngine;

public class audioManager : MonoBehaviour
{



    [Header("Audio source here V")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource SFXSource;


    [Header("Audio clips here V")]
    public AudioClip background;
    public AudioClip Move;
    public AudioClip Death;
    public AudioClip Jump;
    public AudioClip Dash;

    private void Start()
    {
    /* Music code for when play starts.
    musicSource.clip = background;
    musicSource.Play();
    */
    }

    //below function allows us to play sounds. You can take from it since its public.

    public void PlaySFX(AudioClip clip)
    {
        SFXSource.PlayOneShot(clip);
    }

}
