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
    /// ����`�����W�����ׂď����āA��ʏ�̐�������
    /// </summary>
    void Reset()
    {
        _lineRenderer.positionCount = 0;
    }

    /// <summary>
    /// �}�E�X�̍��W�ɐ���`��
    /// </summary>
    void Draw()
    {
        var mousePos = Input.mousePosition;
        mousePos.z = 5;
        // Line Renderer �� positions �ɐV���ɒǉ�������W���v�Z����
        Vector3 pos = _mainCamera.ScreenToWorldPoint(mousePos);
        // �Ō�ɒǉ����� Line Renderer �� positions ��肠����x����Ă�����A���̍��W�� Line Renderer �ɒǉ�����
        if (_lineRenderer.positionCount == 0 || (Vector3.Distance(pos, _lineRenderer.GetPosition(_lineRenderer.positionCount - 1)) > 0.1f))
        {
            _lineRenderer.positionCount++;
            _lineRenderer.SetPosition(_lineRenderer.positionCount - 1, pos);
        }
    }
}