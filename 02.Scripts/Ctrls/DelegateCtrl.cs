using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace hcp
{
    public class DelegateCtrl : ActiveCtrl
    {
        System.Action action;
        System.Func<bool> meetCondition;
        public DelegateCtrl(E_ControlParam contParam, float coolTime, System.Action action, System.Func<bool> meetCondition) : base(contParam, coolTime)
        {
            if (action == null)
            {
                Debug.LogError("DelegateCtrl : 델리게이트 액션 null");
            }
            this.action = action;
            this.meetCondition = meetCondition;
        }
        public override void Activate()
        {
            if (!MeetCondition() || !IsCoolTimeOver())
                return;

            base.Activate();
            action();
        }
        public override bool MeetCondition()
        {
            return meetCondition();
        }
    }
}