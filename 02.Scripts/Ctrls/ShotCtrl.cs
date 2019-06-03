using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace hcp
{
    public class ShotCtrl
    {
        /*
         장애물 판정
         layer - TEAM A(A팀 팀원), TEAM B(B팀 팀원) ... MAP(장애물(맵 지형 등등))  팀원은 팀의 레이어를 공유
         Tag - 같은 팀의 장애물(방벽) 이라면 Tag가 팀 레이어의 이름과 동일
             */
        public static bool ObstacleToShooter(RaycastHit hitObs, Hero shooter)
        {
            string shooterTeamName = LayerMask.LayerToName(shooter.gameObject.layer);

            return ! hitObs.collider.gameObject.CompareTag(shooterTeamName);
        }

        public static bool MapIntersectedCheck(Hero shooter, Vector3 shotPos,  Vector3 hitPos)  //중간에 장애물이 있는지 여부 검사
        {
            Vector3 targetV = hitPos - shotPos;
            float enemyDistance = targetV.magnitude;
            RaycastHit[] hits = Physics.RaycastAll(shotPos, targetV, enemyDistance, 1<<Constants.mapLayerMask);

            MyDebug.Log("ShotCtrl MapIntersectedCheck : 검사 진입 " + hits.Length + "만큼의 콜라이더 검출");
            
            for (int i = 0; i < hits.Length; i++)
            {
                MyDebug.Log("ShotCtrl MapIntersectedCheck : 검사중 " + i+"번째. 이름 = "+hits[i].collider.gameObject.name);
                if (ObstacleToShooter(hits[i], shooter))
                {
                    MyDebug.Log("ShotCtrl MapIntersectedCheck : 검사중 " + i+"번째. 장애물으로 판정됨.");
                    return true;
                }
            }
            return false;
        }

        public static RaycastHit GetFirstHitAsMapOrEnemy(Hero shooter, Ray ray, float shotLength)
        {
            RaycastHit[] hits = Physics.RaycastAll(ray, shotLength,  TeamInfo.GetInstance().MapAndEnemyMaskedLayer);
            
            MyDebug.Log("ShotCtrl GetFirstHitAsMapOrEnemy: "+hits.Length + "개가 검출됨");

            RaycastHit hit  = new RaycastHit();
            float minDis = Mathf.Infinity;

            for (int i = 0; i < hits.Length; i++)
            {
                MyDebug.Log("ShotCtrl GetFirstHitAsMapOrEnemy: " + hits[i].collider.name + "검사중");

                if (    
                    hits[i].distance < minDis
                    &&
                    (hits[i].collider.gameObject.layer != Constants.mapLayerMask  || ObstacleToShooter(hits[i], shooter))
                    )
                {
                    MyDebug.Log("ShotCtrl GetFirstHitAsMapOrEnemy: " + hits[i].collider.name + "검사 통과");
                    hit = hits[i];
                    minDis = hits[i].distance;
                }
            }
            MyDebug.Log("ShotCtrl GetFirstHitAsMapOrEnemy: 최종 선택된 가장 가까운 장애물 or 적 RaycastHit "+hit.collider.name);
            return hit;
        }
    }
}