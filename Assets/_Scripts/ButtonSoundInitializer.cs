using UnityEngine;
using UnityEngine.UI;

public class ButtonSoundInitializer : MonoBehaviour
{
    public AudioClip clickSound;

    void Start()
    {
        Button[] allButtons = Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (Button btn in allButtons)
        {
            if (btn.CompareTag("NoSound")) continue;

            btn.onClick.AddListener(() =>
            {
                if (SoundManager.Instance != null && clickSound != null)
                {
                    SoundManager.Instance.PlaySFX(clickSound);
                }
            });
        }
    }

}
