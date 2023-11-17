using System;
using UnityEngine;

public class TrailUI : MonoBehaviour
{
    Camera _mainCamera;
    LineRenderer _lineRenderer;
    void Start()
    {
        _mainCamera = Camera.main;
        _lineRenderer = GetComponent<LineRenderer>();
    }
    void Update()
    {
        if (Input.GetButton("Fire1"))
            Draw();
        else if (Input.GetButtonUp("Fire1"))
            Reset();
    }
    /// <summary>
    /// 線を描く座標をすべて消して、画面上の線を消す
    /// </summary>
    void Reset()
    {
        _lineRenderer.positionCount = 0;
    }

    /// <summary>
    /// マウスの座標に線を描く
    /// </summary>
    void Draw()
    {
        var mousePos = Input.mousePosition;
        mousePos.z = 5;
        // Line Renderer の positions に新たに追加する座標を計算する
        Vector3 pos = _mainCamera.ScreenToWorldPoint(mousePos);
        // 最後に追加した Line Renderer の positions よりある程度離れていたら、その座標を Line Renderer に追加する
        if (_lineRenderer.positionCount == 0 || (Vector3.Distance(pos, _lineRenderer.GetPosition(_lineRenderer.positionCount - 1)) > 0.1f))
        {
            _lineRenderer.positionCount++;
            _lineRenderer.SetPosition(_lineRenderer.positionCount - 1, pos);
        }
    }
}