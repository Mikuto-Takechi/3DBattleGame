using System.Collections.Generic;
using UnityEngine;

namespace Takechi.BT
{
    public abstract class Composite : BehaviorBase
    {
        protected int activeChild;
        protected List<BehaviorBase> children = new List<BehaviorBase>();
        public virtual Composite OpenBranch(params BehaviorBase[] children)
        {
            for (var i = 0; i < children.Length; i++)
                this.children.Add(children[i]);
            return this;
        }

        public List<BehaviorBase> Children()
        {
            return children;
        }

        public int ActiveChild()
        {
            return activeChild;
        }

        public virtual void ResetChildren()
        {
            activeChild = 0;
            for (var i = 0; i < children.Count; i++)
            {
                Composite b = children[i] as Composite;
                if (b != null)
                {
                    b.ResetChildren();
                }
            }
        }
    }
    public class Sequence : Composite
    {
        public override BTState Tick()
        {
            var childState = children[activeChild].Tick();
            switch (childState)
            {
                case BTState.Success:
                    activeChild++;
                    if (activeChild == children.Count)
                    {
                        activeChild = 0;
                        return BTState.Success;
                    }
                    else
                        return BTState.Running;
                case BTState.Failure:
                    activeChild = 0;
                    return BTState.Failure;
                case BTState.Running:
                    return BTState.Running;
                case BTState.Abort:
                    activeChild = 0;
                    return BTState.Abort;
            }
            throw new System.Exception("This should never happen, but clearly it has.");
        }
    }
    /// <summary>
    /// 子が成功するまで各子を実行し、成功を返す。
    /// 成功した子プロセスがない場合は失敗を返す。
    /// </summary>
    public class Selector : Composite
    {
        public Selector(bool shuffle)
        {
            if (shuffle)
            {
                var n = children.Count;
                while (n > 1)
                {
                    n--;
                    var k = Mathf.FloorToInt(Random.value * (n + 1));
                    var value = children[k];
                    children[k] = children[n];
                    children[n] = value;
                }
            }
        }

        public override BTState Tick()
        {
            var childState = children[activeChild].Tick();
            switch (childState)
            {
                case BTState.Success:
                    activeChild = 0;
                    return BTState.Success;
                case BTState.Failure:
                    activeChild++;
                    if (activeChild == children.Count)
                    {
                        activeChild = 0;
                        return BTState.Failure;
                    }
                    else
                        return BTState.Running;
                case BTState.Running:
                    return BTState.Running;
                case BTState.Abort:
                    activeChild = 0;
                    return BTState.Abort;
            }
            throw new System.Exception("This should never happen, but clearly it has.");
        }
    }
}