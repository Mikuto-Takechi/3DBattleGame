using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Takechi.BT;

public class BTEnemy : MonoBehaviour
{
    Root _root;
    void Awake()
    {
        _root = BT.Root();

        _root.OpenBranch(
                BT.Sequence().OpenBranch(
                    BT.Repeat(3,
                        BT.Sequence().OpenBranch(
                            BT.Log("�^�X�N1"),
                            BT.Log("�^�X�N2"),
                            BT.Log("�^�X�N3")
                            )
                        ),
                    BT.Log("�^�X�N4")
               )
        );
        StartCoroutine(Loop());
    }
    WaitForSeconds wait = new WaitForSeconds(1);
    IEnumerator Loop()
    {
        while (true)
        {
            //_root.Tick();
            Debug.Log(_root.Tick());
            yield return wait;
        }
    }
}