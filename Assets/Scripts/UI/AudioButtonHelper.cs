using UnityEngine;

public class AudioButtonHelper : MonoBehaviour
{
   
    public void PlaySfx(string sfxName)
    {
        AudioManager.PlaySFXGlobal(sfxName);
    }
}