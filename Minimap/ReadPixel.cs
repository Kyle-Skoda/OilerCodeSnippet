using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ReadPixels : MonoBehaviour
{
    [SerializeField]
    private Image miniMapImage;
    [SerializeField]
    private GameObject loadingScreen = null;

    public static Action OnScreenshotTaken;

    private void Awake()
    {
        MatchMaker.Instance.HideUI();
        StartCoroutine(TakeSnapshot());
    }

    public IEnumerator TakeSnapshot()
    {
        //Hide all objects which can cover the UI
        MatchMaker.Instance.HideUI();
        loadingScreen.SetActive(false);
        //Wait for seconds and endofframe makes sure that nothing is shown in the minimap
        yield return new WaitForSeconds(0.1f);
        yield return new WaitForEndOfFrame();

        //Take the screenshot of the level 
        Texture2D texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0, false);
        texture.Apply();
        miniMapImage.material.mainTexture = texture;
        //Reloading image resets the draw to show the minimap, without its just a white square
        miniMapImage.gameObject.SetActive(false);
        miniMapImage.gameObject.SetActive(true);
        //UI starts disabled, enabling here ensures it is enabled only after the minimap screenshot has been taken,
        //Hidden behind the loading screen UI's draw order
        miniMapImage.transform.root.gameObject.SetActive(true);
        OnScreenshotTaken?.Invoke();
        //Renable loading screen, disabled from server once all players have loaded.
        loadingScreen.SetActive(true);
    }
