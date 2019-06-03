using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace hcp
{
    [System.Serializable]
    public enum E_Team
    {
        Team_A=0,
        Team_B,
        Team_C,
        Team_D,
        MAX
    }
    [System.Serializable]
    public enum E_ControlParam
    {
        NormalAttack,
        Reload,

        FirstSkill,
        SecondSkill, 
        Ultimate,

        MAX
    }
    [System.Serializable]
    public enum E_BadState  //상태이상
    {
        None,

        Stun,

        MAX
    }
    [System.Serializable]
    public enum E_HeroType
    {
        Soldier=0,
        Hook,
        MAX
    }
    
    public class DataST
    {

    }
}