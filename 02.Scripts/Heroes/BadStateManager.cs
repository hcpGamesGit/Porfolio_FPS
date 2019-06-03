using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace hcp
{
    public class BadStateManager
    {
        const int notMove = 0;
        const int notActive = 1;
        static readonly bool[,] BadStateEffectTable = new bool[(int)E_BadState.MAX,2]
        {   
            { false,true},  //무브   스테이트 서술
            { false,true}   //액티브 스테이트 서술
        };
        class BadStateUnit
        {
            E_BadState state;
            float clearTime;
            public float ClearTime
            {
                get { return clearTime; }
            }

            public bool IsIt(E_BadState state)
            {
                if (this.state == state) return true;
                return false;
            }
            public BadStateUnit(E_BadState state)
            {
                this.state = state;
                clearTime = 0f;
            }
            public void SetBadState(float clearTime)
            {
                this.clearTime = clearTime;
            }
            public bool CanNotStateNow(int badSort)
            {
                if (!BadStateEffectTable[(int)state, badSort]) return false;
                if (clearTime >= Time.time)
                {
                    return true;
                }
                else return false;
            }
        }

        List<BadStateUnit> bslist = new List<BadStateUnit>();

        public BadStateManager()
        {
            for (int i = 0; i < (int)E_BadState.MAX; i++)
            {
                bslist.Add(new BadStateUnit((E_BadState)i));
            }
        }
        public void GetBadState(E_BadState state, float howMuch)
        {
            BadStateUnit unit = null;
            for (int i = 0; i < bslist.Count; i++)
            {
                if (bslist[i].IsIt(state)) unit = bslist[i];
            }
            if (unit == null) MyDebug.Log("배드 스테이트를 찾지 못함" + state);
            else
            {
                unit.SetBadState(Time.time + howMuch);
            }
        }
        
        public bool CanNotMoveState()
        {
            for (int i = 0; i < bslist.Count; i++)
            {
                if (bslist[i].CanNotStateNow(notMove)) return true;
            }
            return false;
        }
        public bool CanNotActiveState()
        {
            for (int i = 0; i < bslist.Count; i++)
            {
                if (bslist[i].CanNotStateNow(notActive)) return true;
            }
            return false;
        }
        public bool IsSetBadState(E_BadState state)
        {
            BadStateUnit unit = null;
            for (int i = 0; i < bslist.Count; i++)
            {
                if (bslist[i].IsIt(state)) unit = bslist[i];
            }
            if (unit != null&& Time.time < unit.ClearTime)
            {
                return true;
            }
            return false;
        }
    }
  
}