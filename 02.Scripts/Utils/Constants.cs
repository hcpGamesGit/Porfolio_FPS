using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace hcp
{
    public static class Constants
    {
        public const string fpsCamLayerName = "FPSCam";
        public static readonly int fpsCamLayerMask = LayerMask.NameToLayer(fpsCamLayerName);
        public const string mapLayerName = "MAP";
        public static readonly int mapLayerMask = LayerMask.NameToLayer(mapLayerName);
        public const string teamA_LayerName = "TEAM A";
        public static readonly int teamA_LayerMask = LayerMask.NameToLayer(teamA_LayerName);
        public const string teamB_LayerName = "TEAM B";
        public static readonly int teamB_LayerMask = LayerMask.NameToLayer(teamB_LayerName);
        public const string teamC_LayerName = "TEAM C";
        public static readonly int teamC_LayerMask = LayerMask.NameToLayer(teamC_LayerName);
        public const string teamD_LayerName = "TEAM D";
        public static readonly int teamD_LayerMask = LayerMask.NameToLayer(teamD_LayerName);

        public static int GetLayerByE_Team(E_Team team)
        {
            int layer = 0;
            switch (team)
            {
                case E_Team.Team_A:
                    layer = teamA_LayerMask;
                    break;
                case E_Team.Team_B:
                    layer = teamB_LayerMask;
                    break;
                case E_Team.Team_C:
                    layer = teamC_LayerMask;
                    break;
                case E_Team.Team_D:
                    layer = teamD_LayerMask;
                    break;
                default:
                    Debug.LogError("팀이 잘못 전달됨.");
                    break;
            }
            return layer;
        }
        public static E_Team GetE_TeamByLayer(int layer)
        {
            if (layer == teamA_LayerMask)
            {
                return E_Team.Team_A;
            }
            if (layer == teamB_LayerMask)
            {
                return E_Team.Team_B;
            }
            if (layer == teamC_LayerMask)
            {
                return E_Team.Team_C;
            }
            if (layer == teamD_LayerMask)
            {
                return E_Team.Team_D;
            }
            return E_Team.Team_A;
        }


        public static readonly string outLineTag = "OutLine";

        public static readonly string hero_SoldierPath =    "hcp/HeroSoldier";
        public static readonly string hero_HookPath =       "hcp/HeroHook";

        public static string GetHeroPhotonNetworkInstanciatePath(E_HeroType type)
        {
            string str = "";
            switch (type)
            {
                case E_HeroType.Soldier:
                    str = hero_SoldierPath;
                    break;
                case E_HeroType.Hook:
                    str = hero_HookPath;
                    break;
                default:
                    Debug.LogError("GetHeroPhotonNetworkInstanciatePath: 넘겨받은 히어로 타입이 적절치 못함.");
                    break;
            }
            return str;
        }
        public static void DebugLayerMask(int layer)
        {
            int mask = 1;
            int showMask = 0 | mask;
            string str="충돌 체크 되는 레이어 목록[";
            for (int i = 0; i < 32; i++)
            {
                int temp = layer >> i;
                temp = temp & showMask;
                if (temp == 1)
                {
                    str += (LayerMask.LayerToName(i)+",");
                }
            }
            str += "]";
            Debug.Log(str);
        }
    }
}