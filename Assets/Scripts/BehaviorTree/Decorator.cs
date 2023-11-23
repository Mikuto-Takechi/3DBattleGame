namespace Takechi.BT
{
    /// <summary>
    /// デコレータの基本クラス、直下の子を1つしか持てない
    /// </summary>
    public abstract class Decorator : BehaviorBase
    {
        protected BehaviorBase child;
        public Decorator(BehaviorBase child)
        {
            this.child = child;
        }
    }

    /// <summary>
    /// 子ノードを指定した回数繰り返す。
    /// </summary>
    public class Repeat : Decorator
    {
        public int limit = 1;
        int count = 0;
        /// <summary>ノードと繰り返し回数を渡す</summary>
        public Repeat(int count, BehaviorBase child) : base(child)
        {
            this.limit = count;
        }
        public override BTState Tick()
        {
            if (limit > 0 && count < limit)
            {
                switch (child.Tick())
                {
                    case BTState.Running:
                        return BTState.Running;
                    case BTState.Failure:
                        count = 0;
                        return BTState.Failure;
                    default:
                        count++;
                        if (count == limit)
                        {
                            count = 0;
                            return BTState.Success;
                        }
                        return BTState.Running;
                }
            }
            count = 0;
            return BTState.Failure;
        }

        public override string ToString()
        {
            return "Repeat Until : " + count + " / " + limit;
        }
    }
}