using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class KilledPiecesDisplay : MonoBehaviour
{
    private float CanvasWidth;
    private float CanvasHeight;
    private RectTransform rectTransform;

    private void Awake()
    {
        RectTransform canvasRectTransform = GameObject.Find("Body").GetComponent<RectTransform>();
        float CanvasScale = GameObject.Find("Parent Canvas").GetComponent<RectTransform>().rect.height / 1334;
        CanvasWidth = canvasRectTransform.rect.width / CanvasScale;
        CanvasHeight = 1086;
        rectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        float padding = (CanvasHeight - CanvasWidth) / 2;
        padding -= rectTransform.rect.height;
        if (gameObject.name == "Killed White Pieces") padding *= -1;
        rectTransform.anchoredPosition = new Vector2(0, padding);
    }
}
