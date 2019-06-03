using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
namespace hcp {
    public class Payload : MonoBehaviourPun ,IPunObservable{

        System.Action payLoadArrive;

        [System.Serializable]
        enum team
        {
            None,
            TeamA,
            TeamB,
            MAX
        }
        [System.Serializable]
        struct WPRange
        {
            public int Aside;
            public int Bside;
            public WPRange(int a, int b )
            {
                Aside = a;
                Bside = b;
            }

            public void MoveSide(team team)
            {
                switch (team)
                {
                    case team.TeamA:
                        Aside--;
                        Bside--;
                        break;

                    case team.TeamB:
                        Aside++;
                        Bside++;
                        break;
                }
            }
        }

        [System.Serializable]
        struct TransitionDistance
        {
            int startWPNum;
            int endWPNum;
            float distance;
            public float Distance { get { return distance; } }
            public TransitionDistance(int sn, int en, float dis)
            {
                startWPNum = sn;
                endWPNum = en;
                distance = dis;
            }
            bool IsThisTransition(int sn, int en)
            {
                if (startWPNum == sn && endWPNum == en)
                    return true;
                return false;
            }
        }

        [SerializeField]
        Canvas PayloadShowCanvas;
        [SerializeField]
        Image TeamBProgress;
        [SerializeField]
        Image TeamAProgress;
        [SerializeField]
        Image PayloadIcon;
        [SerializeField]
        Text FarFromAText;
        [SerializeField]
        Text FarFromBText;
        [SerializeField]
        float HowFarFromA;
        [SerializeField]
        float HowFarFromB;
        [SerializeField]
        float fillAmountForProgress;
        [SerializeField]
        float payLoadIconX;
        [SerializeField]
        Transform progressStart;
        [SerializeField]
        Transform progressEnd;
        [SerializeField]
        float progressPosStart;
        [SerializeField]
        float progressPosEnd;


        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    HowFarFromA = GetHowFarFromTeamA();
                    FarFromAText.text = HowFarFromA.ToString("000");

                    fillAmountForProgress = HowFarFromA * wholeWPLengthDiv;
                    TeamAProgress.fillAmount = fillAmountForProgress;

                    payLoadIconX = progressPosStart + ((progressPosEnd - progressPosStart) * fillAmountForProgress);
                    
                    Vector2 iconPos= PayloadIcon.rectTransform.position;
                    iconPos.x = payLoadIconX;
                    PayloadIcon.rectTransform.position = iconPos;

                    HowFarFromB = GetHowFarFromTeamB();
                    FarFromBText.text = HowFarFromB.ToString("000");

                    stream.SendNext(HowFarFromA);
                    stream.SendNext(fillAmountForProgress);
                    stream.SendNext(payLoadIconX);
                    stream.SendNext(HowFarFromB);
                    stream.SendNext(nowRange.Aside);
                    stream.SendNext(nowRange.Bside);
                    
                }
            }
            if (stream.IsReading && ! PhotonNetwork.IsMasterClient)
            {
                HowFarFromA = (float)stream.ReceiveNext() ;
                fillAmountForProgress = (float)stream.ReceiveNext();
                payLoadIconX = (float)stream.ReceiveNext();
                HowFarFromB = (float)stream.ReceiveNext();
                nowRange.Aside = (int)stream.ReceiveNext();
                nowRange.Bside = (int)stream.ReceiveNext();

                FarFromAText.text = HowFarFromA.ToString("000");
                TeamAProgress.fillAmount = fillAmountForProgress;

                Vector2 iconPos = PayloadIcon.rectTransform.position;
                iconPos.x = progressPosStart + ((progressPosEnd - progressPosStart) * fillAmountForProgress); ;
                PayloadIcon.rectTransform.position = iconPos;

                FarFromBText.text = HowFarFromB.ToString("000");

            }
        }


        [SerializeField]
        WPRange nowRange;
        [SerializeField]
        List<TransitionDistance> transitionDises;
        [SerializeField]
        float wholeWPLength=-1f;
        [SerializeField]
        float wholeWPLengthDiv;

        [SerializeField]
        float moveSpeed;
        [SerializeField]
        Transform startPoint;
        [SerializeField]
        List<Transform> AWayPoints;
        [SerializeField]
        List<Transform> BWayPoints;
        [SerializeField]
        List<Transform> wholeWayPoints;
        

        [SerializeField]
        Transform nextTarget;


        [SerializeField]
        float wayPointcloseEnough;
        float wayPointcloseEnoughSqr;

        [SerializeField]
        float distance;
        float distanceSqr;
        
        [SerializeField]
        bool arrive = false;

        [SerializeField]
        List<Hero> ATeamHeroes;
        [SerializeField]
        List<Hero> BTeamHeroes;

        [SerializeField]
        int nowTargetPoint;

        

        IEnumerator Start() {

            TeamInfo.GetInstance(). AddListenerOnCLCD(OnClientLefted);

            progressPosStart = progressStart.position.x;
            progressPosEnd = progressEnd.position.x;
            SetWholePath();
            
            wayPointcloseEnoughSqr = wayPointcloseEnough * wayPointcloseEnough;
            distanceSqr = distance * distance;
            
            payLoadArrive += PayloadArrive;
            
            while (!TeamInfo.GetInstance().isTeamSettingDone)
                yield return new WaitForSeconds(1f);
            
            GetABHeroes();

            if (TeamInfo.GetInstance().EnemyTeamLayers.Count != 1 )
            {
                MyDebug.Log("Payload - 이 게임은 2 팀 대결이 아님.");
            }
        }

        void OnClientLefted()
        {
            GetABHeroes();
        }

        void SetWholePath()
        {
            wholeWayPoints = new List<Transform>();
            for (int i =AWayPoints.Count-1;i >=0;i--)
            {
                wholeWayPoints.Add(AWayPoints[i]);
            }

            nowRange = new WPRange(wholeWayPoints.Count-1 , wholeWayPoints.Count);
            
            for (int i = 0; i < BWayPoints.Count; i++)
            {
                wholeWayPoints.Add(BWayPoints[i]);
            }
            transitionDises = new List<TransitionDistance>();
            wholeWPLength = 0f;

            for (int i = 0; i < wholeWayPoints.Count; i++)
            {
                int sn = i;
                int en = i + 1;
                if (en > wholeWayPoints.Count - 1)
                {
                    break;
                }
                float distance = Vector3.Distance(wholeWayPoints[sn].position, wholeWayPoints[en].position);
                transitionDises.Add(new TransitionDistance(sn, en, distance));

                wholeWPLength += distance;
            }
            wholeWPLengthDiv = 1 / wholeWPLength;
        }
        public float GetHowFarFromTeamA()
        {
            float basicDis = Vector3.Distance (wholeWayPoints[nowRange.Aside].position , transform.position);
            if (nowRange.Aside == 0)
                return basicDis;
            for (int i = 0; i < nowRange.Aside; i++)
            {
                basicDis += transitionDises[i].Distance;
            }
            return basicDis;
        }
        public float GetHowFarFromTeamB()
        {
            float basicDis = Vector3.Distance(wholeWayPoints[nowRange.Bside].position, transform.position);
            if (nowRange.Bside == wholeWayPoints.Count-1)
                return basicDis;
            for (int i = wholeWayPoints.Count-1; i >nowRange.Bside; i--)
            {
                basicDis += transitionDises[i-1].Distance;
            }
            return basicDis;
        }

        void GetABHeroes()
        {
            ATeamHeroes = new List<Hero>();
            BTeamHeroes = new List<Hero>();
            Hero[] heroes = GameObject.FindObjectsOfType<Hero>();

            for (int i = 0; i < heroes.Length; i++)
            {
                int photonKey = heroes[i].photonView.ViewID / 1000;
                string teamName =  NetworkManager.instance.Teams[photonKey];
                if (teamName == Constants.teamA_LayerName)
                {
                    ATeamHeroes.Add(heroes[i]);
                }
                else if (teamName == Constants.teamB_LayerName)
                {
                    BTeamHeroes.Add(heroes[i]);
                }
                else {
                    MyDebug.Log("Payload :: GetABHeroes - 양팀 대결이 아님.");
                }
            }
        }
        [SerializeField]
        bool heroClose = false;
        public bool HeroClose
        {
            get { return heroClose; }
        }

        void Update()
        {
            if (!TeamInfo.GetInstance().isTeamSettingDone) return;
            if (!PhotonNetwork.IsMasterClient) return;
            if (arrive) return;

            Vector3 dir;
            if (MoveSideCheck(out dir) == false)
            {
                if (ATeamCount > 0 || BTeamCount > 0)
                {
                    heroClose = true;
                }
                else if (ATeamCount == 0 && BTeamCount == 0)
                {
                    heroClose = false;
                }
                return;
            }
            if (ATeamCount > 0 || BTeamCount > 0)
            {
                heroClose = true;
            }
            else if (ATeamCount == 0 && BTeamCount == 0)
            {
                heroClose = false;
            }

            transform.Translate(dir * Time.deltaTime * moveSpeed, Space.World);
        }
        
        [SerializeField]
        int ATeamCount;
        public int GetATeamCount
        {
            get { return ATeamCount; }
        }
        [SerializeField]
        int BTeamCount;
        public int GetBTeamCount
        {
            get { return BTeamCount; }
        }
        [SerializeField]
        team judgedTeam;

        bool MoveSideCheck(out Vector3 dir)
        {
            dir = Vector3.zero;
                 ATeamCount = GetCountOfCloseHeroes(ATeamHeroes);
                 BTeamCount = GetCountOfCloseHeroes(BTeamHeroes);

            if (ATeamCount > 0 && BTeamCount == 0)  //팀 b로 밀음.
            {
                if (nowRange.Bside == wholeWayPoints.Count - 1 && WayPointClose(nowRange.Bside))
                {
                    payLoadArrive();
                    return false;
                }

                if (WayPointClose(nowRange.Bside) )
                {
                    nowRange.MoveSide(team.TeamB);
                }
                dir = (wholeWayPoints[nowRange.Bside].position - transform.position).normalized;
                
                judgedTeam = team.TeamB;
                    return true;
            }
                else if (ATeamCount == 0 && BTeamCount > 0)
            {
                if (nowRange.Aside == 0 && WayPointClose(nowRange.Aside))
                {
                    payLoadArrive();
                    return false;
                }

                if (WayPointClose(nowRange.Aside))
                {
                    nowRange.MoveSide(team.TeamA);
                }
                dir = (wholeWayPoints[nowRange.Aside].position - transform.position).normalized;
                judgedTeam = team.TeamA;
                return true;
            }
            judgedTeam = team.None;
            dir = Vector3.zero;
            return false;
        }
        
        bool WayPointClose(int num)
        {
            if ((wholeWayPoints[num].position - transform.position).sqrMagnitude < wayPointcloseEnoughSqr)
                return true;
            return false;
        }
        
        void PayloadArrive()
        {
            arrive = true;
            MyDebug.Log("화물이 종단점에 도착했습니다.");
        }
        
        bool IsHeroClose(Hero hero)
        {
            if (hero.Die||hero==null) return false;
            Vector3 heroPos = hero.transform.position - transform.position;
            if (heroPos.sqrMagnitude <= distanceSqr)
            {
                return true;
            }
            return false;
        }
        
        int GetCountOfCloseHeroes(List<Hero> heroes)
        {
            int result = 0;
            for (int i = 0; i < heroes.Count; i++)
            {
                if (IsHeroClose(heroes[i]))
                    result++;
            }
            return result;
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position, distance);
            Gizmos.DrawWireSphere(transform.position, wayPointcloseEnough);
        }

        public void AddListenerPayloadArrive(System.Action ac)
        {
            payLoadArrive += ac;
        }
        public void StopPayload()
        {
            arrive = true;
        }
    }
}