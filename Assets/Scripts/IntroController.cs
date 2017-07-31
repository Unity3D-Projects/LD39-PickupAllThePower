using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IntroController : MonoBehaviour
{
    public Image PlayerImage;
    public Text PlayerCaption, Text1, Text2, Text3;
    public Text ButtonText;

    private bool _playing;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
            return;
        }
    }

    public void OnStartClick()
    {
        if (!_playing)
        {
            ButtonText.text = "Skip ->";

            _playing = true;
            StartCoroutine(Intro());
        }
        else
        {
            StopAllCoroutines();
            SceneManager.LoadScene(SceneNames.Game);
        }
    }
    
    IEnumerator Intro()
    {
        for (float a = 0.0f; a <= 1.0f; a += 0.1f)
        {
            var color = PlayerImage.color;
            color.a = a;
            PlayerImage.color = color;
            yield return new WaitForSeconds(0.05f);
        }

        yield return new WaitForSeconds(1);

        for (float a = 0.0f; a <= 1.0f; a += 0.1f)
        {
            var color = PlayerCaption.color;
            color.a = a;
            PlayerCaption.color = color;
            yield return new WaitForSeconds(0.05f);
        }

        yield return new WaitForSeconds(2);

        for (float a = 0.0f; a <= 1.0f; a += 0.1f)
        {
            var color = Text1.color;
            color.a = a;
            Text1.color = color;
            yield return new WaitForSeconds(0.05f);
        }

        yield return new WaitForSeconds(3);

        for (float a = 0.0f; a <= 1.0f; a += 0.1f)
        {
            var color = Text2.color;
            color.a = a;
            Text2.color = color;
            yield return new WaitForSeconds(0.05f);
        }

        yield return new WaitForSeconds(3);
        
        for (float a = 0.0f; a <= 1.0f; a += 0.1f)
        {
            var color = Text3.color;
            color.a = a;
            Text3.color = color;
            yield return new WaitForSeconds(0.05f);
        }

        yield return new WaitForSeconds(3);

        SceneManager.LoadScene(SceneNames.Game);
    }
}

public static class SceneNames
{
    public const string Intro = "Intro";
    public const string Game = "Game";
}

