using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Takechi.BT
{
    public enum BTState
    {
        Failure,
        Success,
        Running,
        Abort
    }

    public static class BT
    {
        //public static T Block<T>() where T : Block, new() => new T();
        //public static T Branch<T>() where T : Composite, new() => new T();
        //public static T Node<T>() where T : BehaviorBase, new() => new T();

        public static Root Root() => new Root();    //  Block
        public static Sequence Sequence() => new Sequence();    //  Branch
        public static Selector Selector(bool shuffle = false) => new Selector(shuffle); //  Branch
        public static Action RunCoroutine(System.Func<IEnumerator<BTState>> coroutine) => new Action(coroutine);    //  Node
        public static Action Call(System.Action fn) => new Action(fn);  //  Node
        public static ConditionalBranch If(System.Func<bool> fn) => new ConditionalBranch(fn);  //  Block
        public static While While(System.Func<bool> fn) => new While(fn);   //  Block
        /// <summary>ノードの一種。条件を満たしていればtrueを出力する。Tick()では成功を出力する</summary>
        public static Condition Condition(System.Func<bool> fn) => new Condition(fn);   //  Node
        public static Repeat Repeat(int count, BehaviorBase child) => new Repeat(count, child);    //  Decorator
        /// <summary>ノードの一種。指定した秒数待ってから成功を出力する。待ち時間中のTick()ではContinueを出力し続ける</summary>
        public static Wait Wait(float seconds) => new Wait(seconds);    //  Node
        public static Trigger Trigger(Animator animator, string name, bool set = true) => new Trigger(animator, name, set); //  Node
        public static WaitForAnimatorState WaitForAnimatorState(Animator animator, string name, int layer = 0) => new WaitForAnimatorState(animator, name, layer);  //  Node
        public static SetBool SetBool(Animator animator, string name, bool value) => new SetBool(animator, name, value);    //  Node
        public static SetActive SetActive(GameObject gameObject, bool active) => new SetActive(gameObject, active); //  Node
        //public static WaitForAnimatorSignal WaitForAnimatorSignal(Animator animator, string name, string state, int layer = 0) => new WaitForAnimatorSignal(animator, name, state, layer);
        public static Terminate Terminate() => new Terminate(); //  Node
        /// <summary>ノードの一種。ログを出力する。失敗することが無いのでTick()では必ず成功を出力する</summary>
        public static Log Log(string msg) => new Log(msg);  //  Node
        public static RandomSequence RandomSequence(int[] weights = null) => new RandomSequence(weights);   //  Block

    }

    public abstract class BehaviorBase
    {
        public abstract BTState Tick();
    }
    /// <summary>
    /// メソッドを呼び出すか、コルーチンを実行する。
    /// </summary>
    public class Action : BehaviorBase
    {
        System.Action fn;
        System.Func<IEnumerator<BTState>> coroutineFactory;
        IEnumerator<BTState> coroutine;
        public Action(System.Action fn)
        {
            this.fn = fn;
        }
        public Action(System.Func<IEnumerator<BTState>> coroutineFactory)
        {
            this.coroutineFactory = coroutineFactory;
        }
        public override BTState Tick()
        {
            if (fn != null)
            {
                fn();
                return BTState.Success;
            }
            else
            {
                if (coroutine == null)
                    coroutine = coroutineFactory();
                if (!coroutine.MoveNext())
                {
                    coroutine = null;
                    return BTState.Success;
                }
                var result = coroutine.Current;
                if (result == BTState.Running)
                    return BTState.Running;
                else
                {
                    coroutine = null;
                    return result;
                }
            }
        }

        public override string ToString()
        {
            return "Action : " + fn.Method.ToString();
        }
    }

    /// <summary>
    /// メソッドを呼び出し、メソッドが真を返せば成功を返し、そうでなければ失敗を返す。
    /// </summary>
    public class Condition : BehaviorBase
    {
        public System.Func<bool> fn;

        public Condition(System.Func<bool> fn)
        {
            this.fn = fn;
        }
        public override BTState Tick()
        {
            return fn() ? BTState.Success : BTState.Failure;
        }

        public override string ToString()
        {
            return "Condition : " + fn.Method.ToString();
        }
    }

    public class ConditionalBranch : Block
    {
        public System.Func<bool> fn;
        bool tested = false;
        public ConditionalBranch(System.Func<bool> fn)
        {
            this.fn = fn;
        }
        public override BTState Tick()
        {
            if (!tested)
            {
                tested = fn();
            }
            if (tested)
            {
                var result = base.Tick();
                if (result == BTState.Running)
                    return BTState.Running;
                else
                {
                    tested = false;
                    return result;
                }
            }
            else
            {
                return BTState.Failure;
            }
        }

        public override string ToString()
        {
            return "ConditionalBranch : " + fn.Method.ToString();
        }
    }

    /// <summary>
    /// メソッドがtrueを返す間、すべての子を実行する。
    /// </summary>
    public class While : Block
    {
        public System.Func<bool> fn;

        public While(System.Func<bool> fn)
        {
            this.fn = fn;
        }

        public override BTState Tick()
        {
            if (fn())
                base.Tick();
            else
            {
                //if we exit the loop
                ResetChildren();
                return BTState.Failure;
            }

            return BTState.Running;
        }

        public override string ToString()
        {
            return "While : " + fn.Method.ToString();
        }
    }

    public abstract class Block : Composite
    {
        public override BTState Tick()
        {
            switch (children[activeChild].Tick())
            {
                case BTState.Running:
                    return BTState.Running;
                default:
                    activeChild++;
                    if (activeChild == children.Count)
                    {
                        activeChild = 0;
                        return BTState.Success;
                    }
                    return BTState.Running;
            }
        }
    }

    public class Root : Block
    {
        public bool isTerminated = false;

        public override BTState Tick()
        {
            if (isTerminated) return BTState.Abort;
            while (true)
            {
                switch (children[activeChild].Tick())
                {
                    case BTState.Running:
                        return BTState.Running;
                    case BTState.Abort:
                        isTerminated = true;
                        return BTState.Abort;
                    default:
                        activeChild++;
                        if (activeChild == children.Count)
                        {
                            activeChild = 0;
                            return BTState.Success;
                        }
                        continue;
                }
            }
        }
    }

    /// <summary>
    /// 子どもたちのブロックを何度も走らせる。
    /// </summary>
    //public class Repeat : Block
    //{
    //    public int count = 1;
    //    int currentCount = 0;
    //    public Repeat(int count)
    //    {
    //        this.count = count;
    //    }
    //    public override BTState Tick()
    //    {
    //        if (count > 0 && currentCount < count)
    //        {
    //            var result = base.Tick();
    //            switch (result)
    //            {
    //                case BTState.Running:
    //                    return BTState.Running;
    //                default:
    //                    currentCount++;
    //                    if (currentCount == count)
    //                    {
    //                        currentCount = 0;
    //                        return BTState.Success;
    //                    }
    //                    return BTState.Running;
    //            }
    //        }
    //        return BTState.Success;
    //    }

    //    public override string ToString()
    //    {
    //        return "Repeat Until : " + currentCount + " / " + count;
    //    }
    //}

    public class RandomSequence : Block
    {
        int[] m_Weight = null;
        int[] m_AddedWeight = null;

        /// <summary>
        /// 再びトリガーされるたびに、ランダムな子供を1人選ぶ。
        /// </summary>
        /// <param name="weight">すべての子ノードが同じウェイトを持つように、nullのままにする。 
        /// 子ノードよりウェイトが少ない場合、後続の子ノードはすべてウェイト = 1になります。</param>
        public RandomSequence(int[] weight = null)
        {
            activeChild = -1;

            m_Weight = weight;
        }

        public override Composite OpenBranch(params BehaviorBase[] children)
        {
            m_AddedWeight = new int[children.Length];

            for (int i = 0; i < children.Length; ++i)
            {
                int weight = 0;
                int previousWeight = 0;

                if (m_Weight == null || m_Weight.Length <= i)
                {//そのウェイトがなければ、ウェイトを1に設定する。
                    weight = 1;
                }
                else
                    weight = m_Weight[i];

                if (i > 0)
                    previousWeight = m_AddedWeight[i - 1];

                m_AddedWeight[i] = weight + previousWeight;
            }

            return base.OpenBranch(children);
        }

        public override BTState Tick()
        {
            if (activeChild == -1)
                PickNewChild();

            var result = children[activeChild].Tick();

            switch (result)
            {
                case BTState.Running:
                    return BTState.Running;
                default:
                    PickNewChild();
                    return result;
            }
        }

        void PickNewChild()
        {
            int choice = Random.Range(0, m_AddedWeight[m_AddedWeight.Length - 1]);

            for (int i = 0; i < m_AddedWeight.Length; ++i)
            {
                if (choice - m_AddedWeight[i] <= 0)
                {
                    activeChild = i;
                    break;
                }
            }
        }

        public override string ToString()
        {
            return "Random Sequence : " + activeChild + "/" + children.Count;
        }
    }


    /// <summary>
    /// 数秒間実行を停止する。
    /// </summary>
    public class Wait : BehaviorBase
    {
        public float seconds = 0;
        float future = -1;
        public Wait(float seconds)
        {
            this.seconds = seconds;
        }

        public override BTState Tick()
        {
            if (future < 0)
                future = Time.time + seconds;

            if (Time.time >= future)
            {
                future = -1;
                return BTState.Success;
            }
            else
                return BTState.Running;
        }

        public override string ToString()
        {
            return "Wait : " + (future - Time.time) + " / " + seconds;
        }
    }

    /// <summary>
    /// アニメーターのトリガーをアクティブにする。
    /// </summary>
    public class Trigger : BehaviorBase
    {
        Animator animator;
        int id;
        string triggerName;
        bool set = true;

        //もし set == false なら、トリガーをセットする代わりにリセットする。
        public Trigger(Animator animator, string name, bool set = true)
        {
            this.id = Animator.StringToHash(name);
            this.animator = animator;
            this.triggerName = name;
            this.set = set;
        }

        public override BTState Tick()
        {
            if (set)
                animator.SetTrigger(id);
            else
                animator.ResetTrigger(id);

            return BTState.Success;
        }

        public override string ToString()
        {
            return "Trigger : " + triggerName;
        }
    }

    /// <summary>
    /// アニメーターにブール値を設定する。
    /// </summary>
    public class SetBool : BehaviorBase
    {
        Animator animator;
        int id;
        bool value;
        string triggerName;

        public SetBool(Animator animator, string name, bool value)
        {
            this.id = Animator.StringToHash(name);
            this.animator = animator;
            this.value = value;
            this.triggerName = name;
        }

        public override BTState Tick()
        {
            animator.SetBool(id, value);
            return BTState.Success;
        }

        public override string ToString()
        {
            return "SetBool : " + triggerName + " = " + value.ToString();
        }
    }

    /// <summary>
    /// アニメーターがある状態に達するのを待つ。
    /// </summary>
    public class WaitForAnimatorState : BehaviorBase
    {
        Animator animator;
        int id;
        int layer;
        string stateName;

        public WaitForAnimatorState(Animator animator, string name, int layer = 0)
        {
            this.id = Animator.StringToHash(name);
            if (!animator.HasState(layer, this.id))
            {
                Debug.LogError("The animator does not have state: " + name);
            }
            this.animator = animator;
            this.layer = layer;
            this.stateName = name;
        }

        public override BTState Tick()
        {
            var state = animator.GetCurrentAnimatorStateInfo(layer);
            if (state.fullPathHash == this.id || state.shortNameHash == this.id)
                return BTState.Success;
            return BTState.Running;
        }

        public override string ToString()
        {
            return "Wait For State : " + stateName;
        }
    }

    /// <summary>
    /// ゲームオブジェクトのアクティブフラグを設定する。
    /// </summary>
    public class SetActive : BehaviorBase
    {

        GameObject gameObject;
        bool active;

        public SetActive(GameObject gameObject, bool active)
        {
            this.gameObject = gameObject;
            this.active = active;
        }

        public override BTState Tick()
        {
            gameObject.SetActive(this.active);
            return BTState.Success;
        }

        public override string ToString()
        {
            return "Set Active : " + gameObject.name + " = " + active;
        }
    }

    /// <summary>
    /// アニメーターの SendSignal ステートマシン動作からシグナルを受信するのを待つ。
    /// </summary>
    //public class WaitForAnimatorSignal : BTNode
    //{
    //    internal bool isSet = false;
    //    string name;
    //    int id;

    //    public WaitForAnimatorSignal(Animator animator, string name, string state, int layer = 0)
    //    {
    //        this.name = name;
    //        this.id = Animator.StringToHash(name);
    //        if (!animator.HasState(layer, this.id))
    //        {
    //            Debug.LogError("The animator does not have state: " + name);
    //        }
    //        else
    //        {
    //            SendSignal.Register(animator, name, this);
    //        }
    //    }

    //    public override BTState Tick()
    //    {
    //        if (!isSet)
    //            return BTState.Continue;
    //        else
    //        {
    //            isSet = false;
    //            return BTState.Success;
    //        }

    //    }

    //    public override string ToString()
    //    {
    //        return "Wait For Animator Signal : " + name;
    //    }
    //}

    public class Terminate : BehaviorBase
    {

        public override BTState Tick()
        {
            return BTState.Abort;
        }
    }

    public class Log : BehaviorBase
    {
        string msg;

        public Log(string msg)
        {
            this.msg = msg;
        }

        public override BTState Tick()
        {
            Debug.Log(msg);
            return BTState.Success;
        }
    }

}

#if UNITY_EDITOR
namespace Takechi.BT
{
    public interface IBTDebugable
    {
        Root GetAIRoot();
    }
}
#endif