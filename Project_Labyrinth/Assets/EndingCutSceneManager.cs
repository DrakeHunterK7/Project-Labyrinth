using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingCutSceneManager : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Image fadeImage;
    [SerializeField] private UnityEngine.UI.Image endingSlide1;
    [SerializeField] private UnityEngine.UI.Image endingSlide2;
    [SerializeField] private UnityEngine.UI.Image endingSlide3;


    private void Start()
    {
        StartCoroutine(SlideshowStart());
    }


    IEnumerator SlideshowStart()
    {
        endingSlide1.gameObject.SetActive(true);
        StartCoroutine(FadeImage(fadeImage, true));
        
        yield return new WaitForSeconds(40f);
        StartCoroutine(FadeImage(fadeImage, false));
        
        yield return new WaitForSeconds(3f);
        endingSlide2.gameObject.SetActive(true);
        StartCoroutine(FadeImage(fadeImage, true));
        
        yield return new WaitForSeconds(40f);
        StartCoroutine(FadeImage(fadeImage, false));
        
        yield return new WaitForSeconds(3f);
        endingSlide3.gameObject.SetActive(true);
        StartCoroutine(FadeImage(fadeImage, true));
        
        yield return new WaitForSeconds(40f);
        StartCoroutine(FadeImage(fadeImage, false));
        
        
        yield return new WaitForSeconds(5f);
        SceneManager.LoadScene("Main_Menu");

    }
    
    
    
    IEnumerator FadeImage(UnityEngine.UI.Image img, bool fadeAway)
    {
        // fade from opaque to transparent
        if (fadeAway)
        {
            // loop over 1 second backwards
            for (float i = 1; i >= 0; i -= Time.deltaTime)
            {
                // set color with i as alpha
                img.color = new Color(1, 1, 1, i);
                yield return null;
            }
        }
        // fade from transparent to opaque
        else
        {
            // loop over 1 second
            for (float i = 0; i <= 1; i += Time.deltaTime)
            {
                // set color with i as alpha
                img.color = new Color(1, 1, 1, i);
                yield return null;
            }
        }
    }
}
