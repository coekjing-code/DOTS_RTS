using System;
using UnityEngine;

public class UnitSelectionManagerUI : MonoBehaviour
{
    [SerializeField] private RectTransform selectionAreaRectTransform;
    [SerializeField] private Canvas canvas;

    void Start()
    {
        UnitSelectionManager.Instance.onSelectionAreaStart += UnitSelectionManager_onSelectionAreaStart;
        UnitSelectionManager.Instance.onSelectionAreaEnd += UnitSelectionManager_onSelectionAreaEnd;
    
        selectionAreaRectTransform.gameObject.SetActive(false);
    }

    void Update()
    {
        if (selectionAreaRectTransform.gameObject.activeSelf)
        {
            UpdateVisual();
        }
    }

    private void UnitSelectionManager_onSelectionAreaStart(object sender, EventArgs e)
    {
        selectionAreaRectTransform.gameObject.SetActive(true);

        UpdateVisual();
    }

    private void UnitSelectionManager_onSelectionAreaEnd(object sender, EventArgs e)
    {
        selectionAreaRectTransform.gameObject.SetActive(false);
    }
    private void UpdateVisual()
    {
        Rect selectAreaRect = UnitSelectionManager.Instance.GetSelectionAreaRect();

        float canvasScale = canvas.transform.localScale.x;
        selectionAreaRectTransform.anchoredPosition = new Vector2(selectAreaRect.x, selectAreaRect.y) / canvasScale;
        selectionAreaRectTransform.sizeDelta = new Vector2(selectAreaRect.width, selectAreaRect.height) / canvasScale;
    }
}
