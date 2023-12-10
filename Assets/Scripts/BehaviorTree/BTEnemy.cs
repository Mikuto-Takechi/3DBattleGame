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

        _root.AddChildren(
            new Sequence().AddChildren(
                new ParallelSelector().AddChildren(
                    new Condition(()=>false),
                    new Sequence().AddChildren(
                        new Log("çıìG3ïb"),
                        new Wait(3)
                        )
                    )
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