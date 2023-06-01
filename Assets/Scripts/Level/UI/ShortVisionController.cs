using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class ShortVisionController : MonoBehaviour
{
    private bool shortVisionEnabled;
    private int lastEnabledBy;
    private const float radiusToLengthMultiplier = 91f;
    private float visibleAreaRadius;
    [SerializeField]
    private float minVisibleAreaRadius;
    private float maxVisibleAreaRadius;
    [SerializeField]
    private float visibleAreaRadiusDecreaseSpeed;
    [SerializeField]
    private float visibleAreaRadiusIncreaseSpeed;
    private bool reachedMinRadius;
    private bool reachedMaxRadius;
    private RectTransform visibleAreaRT;
    private Transform playerTransform;
    private LevelController levelController;

    private void Awake()
    {
        playerTransform = GameObject.Find("Player").GetComponent<Transform>();

        visibleAreaRT = GameObject.Find("VisibleArea").GetComponent<RectTransform>();

        levelController = GameObject.Find("LevelController").GetComponent<LevelController>();

        shortVisionEnabled = false;
    }

    private void Start()
    {
        Vector2 canvasSize = GameObject.Find("Canvas").GetComponent<RectTransform>().sizeDelta;

        RectTransform darkImageRT = GameObject.Find("DarkImage").GetComponent<RectTransform>();
        Debug.Log(canvasSize * 2);
        darkImageRT.sizeDelta = canvasSize * 2;
        Debug.Log(darkImageRT.sizeDelta);

        maxVisibleAreaRadius = Mathf.Sqrt(Vector2.SqrMagnitude(canvasSize)) * 2 / radiusToLengthMultiplier;
        visibleAreaRadius = maxVisibleAreaRadius;
    }

    private void Update()
    {
        if (levelController.IsGameActive)
        {
            visibleAreaRT.position = Camera.main.WorldToScreenPoint(playerTransform.position);
            if (shortVisionEnabled)
            {
                if (!reachedMinRadius)
                {
                    if (visibleAreaRadius > minVisibleAreaRadius + visibleAreaRadiusDecreaseSpeed)
                    {
                        visibleAreaRadius -= visibleAreaRadiusDecreaseSpeed;
                    }
                    else
                    {
                        visibleAreaRadius = minVisibleAreaRadius;
                        reachedMinRadius = true;
                    }
                }
                reachedMaxRadius = false;
            }
            else
            {
                if (!reachedMaxRadius)
                {
                    if (visibleAreaRadius + visibleAreaRadiusIncreaseSpeed < maxVisibleAreaRadius)
                    {
                        visibleAreaRadius += visibleAreaRadiusIncreaseSpeed;
                    }
                    else
                    {
                        visibleAreaRadius = maxVisibleAreaRadius;
                        reachedMaxRadius = true;
                    }
                }
                reachedMinRadius = false;
            }

            visibleAreaRT.sizeDelta = (new Vector2(visibleAreaRadius, visibleAreaRadius)) * radiusToLengthMultiplier;
        }
    }

    public void EnableShortVision(int ID)
    {
        lastEnabledBy = ID;
        shortVisionEnabled = true;
    }

    public void DisableShortVision(int ID)
    {
        if (ID == lastEnabledBy) shortVisionEnabled = false;
    }
}
