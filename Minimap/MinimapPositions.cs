using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MinimapPositions : MonoBehaviour
{
    [SerializeField]
    private Transform playerTarget;
    [SerializeField]
    private MinimapMarker marker;
    [SerializeField]
    private GameObject physicalMarker;
    [SerializeField]
    private QuestPointer pointer;
    [SerializeField]
    private float visibleSoundLength = 5f;
    [SerializeField]
    private float soundIconFadeSpeed = 1f;

    [Header("Image Objects")]
    [SerializeField]
    private Image playerImage;
    [SerializeField]
    private Image markerImage;
    [SerializeField]
    private Image questionMarkImage;
    [SerializeField]
    private Image ringImage;

    private InputMaster controls;
    private Vector2 mousePos;
    private RectTransform rectTrans;
    private GameObject spawnedMarker;
    private float width;
    private float height;

    public static Action<Vector2> OnHeardSound;

    private void Awake()
    {
        controls = new InputMaster();
        controls.Player.Aim.performed += ctx => SetMousePos(ctx.ReadValue<Vector2>());

        rectTrans = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        controls.Player.Enable();
        OnHeardSound += ApplySoundHeardMinimap;
        GameManager.Instance.OnRingSizeChange += ChangeRingSize;
    }

    private void OnDisable()
    {
        //Prevent errors by removing delegate calls and stopping coroutines
        StopAllCoroutines();
        questionMarkImage.color = Color.clear;
        controls.Player.Disable();
        OnHeardSound -= ApplySoundHeardMinimap;
        if (GameManager.Instance == null) return;
        GameManager.Instance.OnRingSizeChange -= ChangeRingSize;
    }

    //Change the size of the playable area ring on the minimap
    public void ChangeRingSize(float sizeDif, Vector2 pos)
    {
        Vector2 actualPos = NormalizedPositionInWorld(pos);
        actualPos.x *= (width / 2);
        actualPos.y *= (height / 2);
        ringImage.rectTransform.localPosition = actualPos;
        ringImage.transform.localScale = Vector2.one / sizeDif;
    }

    //Set the players position on the minimap
    public void SetMiniMapPosition(Vector2 pos)
    {
        pos.x *= (width / 2);
        pos.y *= (height / 2);
        playerImage.rectTransform.localPosition = pos;
    }

    private void Update()
    {
        //TODO: Move width and height set to change only when application size changes, in update to ensure UI fits all screen size for now
        width = rectTrans.rect.width - rectTrans.sizeDelta.x;
        height = rectTrans.rect.height - rectTrans.sizeDelta.y;
        //Set player position on the minimap
        SetMiniMapPosition(NormalizedPositionInWorld(playerTarget.position));
    }

    private void SetMousePos(Vector2 pos) => mousePos = pos;

    public void AdjustMarker(Vector2 imagePos)
    {
        markerImage.transform.position = imagePos;
        //If player sets the marker to a point of interest enable and set position
        if (imagePos != Vector2.one * 99999)
        {
            pointer.gameObject.SetActive(true);

            Vector2 normalizedValue = markerImage.transform.localPosition;
            normalizedValue.x /= (width / 2);
            normalizedValue.y /= (height / 2);
            pointer.SetTarget(WorldPositionFromMinimapPosition(normalizedValue));
        }
        //Disable the marker
        else
            pointer.gameObject.SetActive(false);
    }

    private void ApplySoundHeardMinimap(Vector2 pos)
    {
        Debug.Log(pos);
        StopAllCoroutines();
        Vector2 actualPos = NormalizedPositionInWorld(pos);
        actualPos.x *= (width / 2);
        actualPos.y *= (height / 2);
        questionMarkImage.rectTransform.localPosition = actualPos;
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        questionMarkImage.color = Color.white;
        yield return new WaitForSeconds(visibleSoundLength);
        while (questionMarkImage.color != Color.clear)
        {
            questionMarkImage.color = Color.Lerp(questionMarkImage.color, Color.clear, Time.deltaTime * soundIconFadeSpeed);
            yield return null;
        }
    }

    /** By using normalized values on the minimap and worldsize its possible to get the exact
    * position on the minimap or within the world by converting to a normalized value even if the transform
    * positions are different, for example 0.5f, 0.5f could be 100, 100 on the UI but 0, 0 in the world this allows for
    * easy conversion from world to minimap positions. */

    //Convert world position to normalized position within mapsize
    private Vector2 NormalizedPositionInWorld(Vector2 position)
    {
        if (GameManager.Instance == null) return Vector2.zero;
        Vector2 mapSize = GameManager.Instance.GetMapSize();

        Vector2 normalizePosition = (position / (mapSize / 2));
        return normalizePosition;
    }

    //Convert from minimap position to world position
    private Vector2 MinimapToWorldPositionFromNormalizedValue(Vector2 normalizedValue)
    {
        Vector2 mapSize = GameManager.Instance.GetMapSize();

        Vector2 pos = (normalizedValue * (mapSize / 2));
        return pos;
    }

    //Convert from world position to minimap positon
    private Vector2 WorldPositionFromMinimapPosition(Vector2 normalizedValue)
    {
        Vector2 mapSize = GameManager.Instance.GetMapSize();
        Vector2 pos = (normalizedValue * (mapSize / 2));
        return pos;
    }

    public Vector2 GetMousePositionOnMinimap() => mousePos;
    public Vector2 GetMarkerPosition() => markerImage.transform.localPosition;
    public Vector2 GetQuestionMarkPosition() => questionMarkImage.transform.localPosition;
}
