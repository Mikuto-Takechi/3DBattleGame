using System.Collections;
using System.Collections.Generic;
using System.IO.Pipes;
using UnityEngine;
using static UnityEngine.EventSystems.StandaloneInputModule;

public class Player : MonoBehaviour
{
    Animator _animator;
    Vector3 _previousPos;
    Vector3 _currentPos;
    Direction _direction;
    enum Direction
    {
        Up = 1,
        Down = 1 << 1,
        Left = 1 << 2,
        Right = 1 << 3,
    }
    void Awake()
    {
        _animator = GetComponent<Animator>();
        //_rotation = Quaternion.Euler(90, 90, 90);
    }
    void OnAnimatorIK(int layerIndex)
    {
        //_animator.SetBoneLocalRotation(HumanBodyBones.RightHand, _rotation);
    }
    void Update()
    {
        // ƒXƒƒCƒv‚É‚æ‚éˆÚ“®ˆ—
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
                2 => "ã",
                3 => "¶ã",
                4 => "Left Slash",
                5 => "¶‰º",
                6 => "‰º",
                7 => "Lower Right Slash",
                _ => string.Empty,
            });
        }
    }
}