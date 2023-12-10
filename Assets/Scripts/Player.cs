using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    Animator _animator;
    Vector3 _previousPos;
    Vector3 _currentPos;
    void Awake()
    {
        _animator = GetComponent<Animator>();
    }
    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            _previousPos = Input.mousePosition;
        }
        if (Input.GetButtonUp("Fire1"))
        {
            _currentPos = Input.mousePosition;
            var diff = (Vector2)(_currentPos - _previousPos);
            int direction8 = (Mathf.RoundToInt(4.0f * Mathf.Atan2(diff.y, diff.x) / Mathf.PI) + 8) % 8;
            _animator.Play(direction8 switch
            {
                0 => "Right Slash",
                1 => "Upper Right Slash",
                2 => "Upper Slash",
                3 => "Upper Left Slash",
                4 => "Left Slash",
                5 => "Lower Left Slash",
                6 => "Lower Slash",
                7 => "Lower Right Slash",
                _ => string.Empty,
            });
        }
        if(Input.GetButtonDown("Jump"))
        {
            _animator.SetBool("IsBlocked", true);
        }
        if (Input.GetButtonUp("Jump"))
        {
            _animator.SetBool("IsBlocked", false);
        }
    }
    public void Swing() => AudioManager.Instance.PlaySE("ëfêUÇË");
    public void ShieldBlock() => AudioManager.Instance.PlaySE("ç\Ç¶ÇÈ");
}