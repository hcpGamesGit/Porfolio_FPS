using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace hcp
{
    public class ActiveCtrl //일반 공격이나 스킬 재장전등 모두 포함하는 개념 .
    {
        protected E_ControlParam controlParam;
        public E_ControlParam ControlParam
        {
            get { return controlParam; }
        }
        protected float coolTime;
        public float CoolTime
        {
            get { return coolTime; }
        }
        protected float coolTimeDiv;
        protected float lastActivatedTime = 0f;
        /*
         쿨타임 외의 실행 조건.
             */
        public virtual bool MeetCondition()
        {
            return false;
        }

        public float ReUseRemainingTime
        {
            get
            {
                if (IsCoolTimeOver())
                {
                    return -1f;
                }
                return lastActivatedTime + coolTime - Time.time;
            }
        }
        public float ReUseRemainingTimeInAZeroToOne
        {
            get {
                if (ReUseRemainingTime == -1f)
                {
                    return 0f;
                }
                else {
                    return ReUseRemainingTime * coolTimeDiv;
                }
            }
        }

        public ActiveCtrl(E_ControlParam contParam, float coolTime)
        {
            this.controlParam = contParam;
            this.coolTime = coolTime;
            coolTimeDiv = 1 / coolTime;
        }

        public virtual void Activate()
        {
            lastActivatedTime = Time.time;
        }
        public virtual bool IsCoolTimeOver()  //쿨타임 끝났는지 여부 반환.
        {
            if (lastActivatedTime + coolTime > Time.time)   //쿨타임 도중
            {
                return false;
            }
            return true;
        }

    }
}