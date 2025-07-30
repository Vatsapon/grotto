using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RadialLayout : LayoutGroup
{
    [Tooltip("Offset away from the center")]
    [SerializeField] private float _offset;
    [Range(0f, 360f)]
    [Tooltip("Angle to start as first order")]
    [SerializeField] private float _startAngle;
    [Range(0f, 360f)]
    [Tooltip("Minimum angle of this radial")]
    [SerializeField] private float _minAngle;
    [Range(0f, 360f)]
    [Tooltip("Maximum angle of this radial")]
    [SerializeField] private float _maxAngle;

    public float StartAngle
    {
        get { return _startAngle; }
        set { _startAngle = value; }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        
        CalculateRadial();
    }

    private void Update()
    {
        CalculateRadial();
    }

    public override void SetLayoutHorizontal() { }
    public override void SetLayoutVertical() { }

    public override void CalculateLayoutInputVertical()
    {
        CalculateRadial();
    }

    public override void CalculateLayoutInputHorizontal()
    {
        CalculateRadial();
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();

        UnityEditor.EditorApplication.update += OnValidateDelay;
    }

    private void OnValidateDelay()
    {
        UnityEditor.EditorApplication.update -= OnValidateDelay;

        if (!this || !UnityEditor.EditorUtility.IsDirty(this))
        {
            return;
        }

        CalculateRadial();

        UnityEditor.EditorUtility.SetDirty(rectTransform);
    }
#endif

    private void CalculateRadial()
    {
        m_Tracker.Clear();

        if (transform.childCount == 0)
        {
            return;
        }

        float offsetAngle = ((_maxAngle - _minAngle)) / (transform.childCount - 1);
        float angle = _startAngle;

        for (int i = 0; i < transform.childCount; i++)
        {
            RectTransform child = (RectTransform)transform.GetChild(i);

            if (child)
            {
                m_Tracker.Add(this, child, DrivenTransformProperties.Anchors | DrivenTransformProperties.AnchoredPosition | DrivenTransformProperties.Pivot);

                Vector3 pos = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0f);
                child.localPosition = pos * _offset;
                child.anchorMin = new Vector2(0.5f, 0.5f);
                child.anchorMax = new Vector2(0.5f, 0.5f);
                child.pivot = new Vector2(0.5f, 0.5f);
                angle += offsetAngle;
            }
        }

    }
}
