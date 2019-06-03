using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
namespace hcp
{
    public class TeamInfo : MonoBehaviourPun
    {
        [SerializeField]
        int myPhotonViewIDKey;
        public int MyPhotonViewIDKey
        {
            get { return myPhotonViewIDKey; }
        }

        [SerializeField]
        int myTeamLayer;
        public int MyTeamLayer
        {
            get
            {
                return myTeamLayer;
            }
        }

        [SerializeField]
        List<int> enemyTeamLayers = new List<int>();
        public List<int> EnemyTeamLayers
        {
            get { return enemyTeamLayers; }
        }


        [SerializeField]
        List<Hero> enemyHeroes = new List<Hero>();
        public List<Hero> EnemyHeroes
        {
            get
            {
                return enemyHeroes;
            }
        }
        [SerializeField]
        List<Hero> myTeamHeroes = new List<Hero>();
        public List<Hero> MyTeamHeroes
        {
            get
            {
                return myTeamHeroes;
            }
        }
        static TeamInfo _instance = null;
        public static TeamInfo GetInstance()
        {
            return _instance;
        }

        Dictionary<int, Hero> heroPhotonIDDic = new Dictionary<int, Hero>();
        public Dictionary<int, Hero> HeroPhotonIDDic
        {
            get {
                return heroPhotonIDDic;
            }
        }

        public bool isTeamSettingDone=false;

        [SerializeField]
        Color enemyTeamColor;
        [SerializeField]
        Color myTeamColor;
        [SerializeField]
        [Range(0, 0.1f)]
        float outLineWidth;


        private void Awake()
        {
            if (_instance == null)
                _instance = this;
        }

        IEnumerator Start()
        {
            yield return new WaitForSeconds(2f);
            StartCoroutine(WaitForAllHeroBorn());

            NetworkManager.instance.AddListenerOnClientLeft(OnClientLefted);
        }

        void OnClientLefted()
        {
            StartCoroutine(clientLeftCheck());
        }
        System.Action clientLeftAndCheckDone;
        public void AddListenerOnCLCD(System.Action ac)
        {
            clientLeftAndCheckDone += ac;
        }
        IEnumerator clientLeftCheck()
        {
            yield return new WaitForEndOfFrame();

            for (int i = 0; i < myTeamHeroes.Count; i++)
            {
                if (myTeamHeroes[i] == null)
                {
                    myTeamHeroes.RemoveAt(i);
                }
            }
            for (int i = 0; i < enemyHeroes.Count; i++)
            {
                if (enemyHeroes[i] == null)
                {
                    enemyHeroes.RemoveAt(i);
                }
            }
            if (clientLeftAndCheckDone != null)
                clientLeftAndCheckDone();
        }


        IEnumerator WaitForAllHeroBorn()
        {
            int heroCounts = 0;
            while (heroCounts != PhotonNetwork.CurrentRoom.PlayerCount)
            {
                heroCounts = GameObject.FindObjectsOfType<Hero>().Length;
                yield return new WaitForSeconds(1f);
            }
            //Hero 전부 생성

            if (NetworkManager.instance == null)
            {
                MyDebug.Log("TeamInfo:::GetTeamInfoFromNetworkManager - NetworkManager DO NOT exist");
                //Game Abort.
                UnityEngine.SceneManagement.SceneManager. LoadScene(UnityEngine.SceneManagement.SceneManager.GetSceneByBuildIndex(0).name);
                yield break;
            }

            Dictionary<int,string> teamInfoDic = NetworkManager.instance.Teams;
            
            myPhotonViewIDKey = 0;
            Hero[] heroes = GameObject.FindObjectsOfType<Hero>();
            for (int i = 0; i < heroes.Length; i++)
            {
                if (heroes[i].photonView.IsMine)
                {
                    myPhotonViewIDKey = heroes[i].photonView.ViewID / 1000;
                }
            }

            myTeamLayer = LayerMask.NameToLayer(teamInfoDic[myPhotonViewIDKey]);
            enemyTeamLayers.Clear();
            
            Dictionary<int, string>.Enumerator enu = teamInfoDic.GetEnumerator();
            while (enu.MoveNext())
            {
                int photonViewIDKey = enu.Current.Key;
                string layerName = enu.Current.Value;
                int layerMask = LayerMask.NameToLayer(layerName);
                
                if (false == enemyTeamLayers.Contains(layerMask) && layerMask != myTeamLayer) 
                {
                    enemyTeamLayers.Add(layerMask);
                }
            }
            
            //레이어 세팅 끝.

            //이제 영웅의 레이어 세팅과 영웅 분류를 저장.
            
            myTeamHeroes.Clear();
            enemyHeroes.Clear();
            heroPhotonIDDic.Clear();
            for (int i = 0; i < heroes.Length; i++)
            {
                heroPhotonIDDic.Add(heroes[i].photonView.ViewID, heroes[i]);
                int heroPhotonID = heroes[i].photonView.ViewID / 1000;  //이 영웅의 포톤뷰 키
             
                int setLayerByNM = LayerMask.NameToLayer( teamInfoDic[heroPhotonID]);   //네트워크 매니저에서 저장되어 넘어온 이 포톤뷰의 팀 설정 (레이어)
                if (setLayerByNM ==  myTeamLayer)
                {
                    myTeamHeroes.Add(heroes[i]);
                    heroes[i].gameObject.layer = myTeamLayer;
                    heroes[i].SetOutLineAndRim(outLineWidth, myTeamColor);
                }
                else
                {
                    enemyHeroes.Add(heroes[i]);
                    heroes[i].gameObject.layer = setLayerByNM;  //저장되어 넘어온 레이어를 Hero에 넣어줌.
                    heroes[i].SetOutLineAndRim(outLineWidth, enemyTeamColor);
                }
            }

            //Team Setting is Done

            for (int i = 0; i < heroes.Length; i++)
            {
                if (heroes[i].hpBar != null)
                {
                    heroes[i].hpBar.SetAsTeamSetting();
                }
                if (heroes[i].photonView.IsMine)
                {
                    Destroy(heroes[i].hpBar.gameObject);
                }
            }

            isTeamSettingDone = true;

            MyDebug.Log(" TeamInfo : 팀세팅과 관련된 처리 모두 끝.");
        }
        
        public int EnemyMaskedLayer//에너미가 한 개 이상이면 그에 맞게 마스킹해서 줌.
        {
            get
            {
                int layer = -1;
                for (int i = 0; i < enemyTeamLayers.Count; i++)
                {
                    if (layer == -1)    
                    {
                        layer = 1 << enemyTeamLayers[i];
                    }
                    else
                    {
                        layer = layer | 1 << enemyTeamLayers[i];
                    }
                }
                return layer;
            }
        }
        public int MapAndEnemyMaskedLayer//MAP(장애물 , 지형) 과 적 레이어 마스크
        {
            get
            {
                int layer = EnemyMaskedLayer;
                return layer | 1 << Constants.mapLayerMask;
            }
        }
        public bool IsThisLayerEnemy(int layer)
        {
            return enemyTeamLayers.Contains(layer);
        }
        
        public int GetTeamLayerByPhotonViewID(int photonViewID)
        {
            return LayerMask.NameToLayer(NetworkManager.instance.Teams[photonViewID / 1000]);
        }
    }
}