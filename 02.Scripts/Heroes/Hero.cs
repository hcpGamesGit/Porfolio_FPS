using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
namespace hcp
{
    public abstract class Hero : MonoBehaviourPun
    {
        protected enum E_MoveDir
        {
            NONE,
            Forward,
            Backward,
            Left,
            Right,
            MAX
        }

        protected BadStateManager badStateMgr =new BadStateManager();

        protected Animator anim;

        [Header("Hero's Property")]
        [Space(10)]
        [SerializeField]
        protected E_HeroType heroType;
        public E_HeroType HeroType
        {
            get { return heroType; }
        }
        [SerializeField]
        string playerName;
        public string PlayerName
        {
            set {
                playerName = value;
            }
            get {
                return playerName;
            }
        }
        [SerializeField]
        Renderer[] heroRenderers;
        protected HeroRenderer heroRendererMgr;

        [SerializeField]
        float respawnTime = 5f;

        [SerializeField]
        protected float rotateYUpLimit ;
        protected float rotateYUpLimitBy360toQuarternion;

        [SerializeField]
        protected float rotateYDownLimit;
       

        [SerializeField]
        protected float moveSpeed;
        [SerializeField]
        protected float rotateSpeed;

        [SerializeField]
        protected float maxHP;
        public float MaxHP {
            get {
                return maxHP;
            }
        }

        [SerializeField]
        protected float currHP;
        public float CurrHP
        {
            get
            {
                return currHP;
            }
        }
        [SerializeField]
        protected bool IsDie = false;
        public bool Die
        {
            get { return IsDie; }
        }

        protected Dictionary<E_ControlParam, ActiveCtrl> activeCtrlDic = new Dictionary<E_ControlParam, ActiveCtrl>();

        [Tooltip("needed amount for Ult Activate")]
        [SerializeField]
        protected float neededUltAmount;
        protected float neededUltAmountDiv;
        [Tooltip("now Amount for Ult Activate")]
        [SerializeField]
        protected float nowUltAmount;
        [Tooltip("ult Amount up value per sec")]
        [SerializeField]
        protected float ultPlusPerSec;


       
        [SerializeField]
        protected Vector3 screenCenterPoint
        {
            get { return new Vector3(Screen.width *0.5f, Screen.height *0.5f, 0); }
        }

        [Tooltip("Max Shot Length (For infinite Range)")]
        [SerializeField]
        protected float maxShotLength = 5000;

        protected float maxShotLengthDiv;

        [SerializeField]
        public Sprite[] crossHairs;

        [Tooltip("local ! height headShot")]
        [SerializeField]
        float headShotOffset;
        public float HeadShotOffset
        {
            get { return headShotOffset; }
        }
        [SerializeField]
        Transform camPos;
        [Tooltip("gameobject has fpscamperhero component")]
        [SerializeField]
        GameObject FPSCamPerHeroGO;

        [Tooltip("attached FPS Cam, only take a handle for animation thing for fps cam")]
        [SerializeField]
        protected FPSCameraPerHero FPSCamPerHero;


        [SerializeField]
        public HeroHpBar hpBar;

        [Tooltip("local base . to apply center Position Offset")]
        [SerializeField]
        protected float centerOffset;

        System.Action dieAction;

        /*
         center position for apply this hero's center
             */
        public Vector3 CenterPos
        {
            get
            {
                Vector3 v = transform.position;
                v.y += centerOffset;
                return v;
            }
        }
        
        Rigidbody rb;
        public Rigidbody GetRigidBody
        {
            get
            {
                return rb;
            }
        }
        Collider coll;
        public Collider GetCollider
        {
            get {
                return coll;
            }
        }
        protected virtual void Awake()
        {
            heroRendererMgr = new HeroRenderer(heroRenderers);
            heroRendererMgr.SetOcclude(false);
          
            dieAction = new System.Action(DieCallBack);

            rb = this.gameObject.GetComponent<Rigidbody>();
            coll = GetComponent<Collider>();
            maxShotLengthDiv = 1 / maxShotLength;
            neededUltAmountDiv = 1 / neededUltAmount;
            rotateYUpLimitBy360toQuarternion = 360 - rotateYUpLimit;
            anim = this.gameObject.GetComponent<Animator>();
            SetActiveCtrls();
            
            playerName = NetworkManager.instance.Names[photonView.ViewID/1000];

            if (photonView.IsMine)
            {
                heroRendererMgr.Show(false);

                InGameUIManager.Instance.SetTargetHero(this);
                Camera mainCam = Camera.main;
                mainCam.transform.SetParent(transform);
                mainCam.transform.SetPositionAndRotation(camPos.position, camPos.rotation);
                GameObject fpsCam = Resources.Load("hcp/FPSCamera") as GameObject;
             
                GameObject fpsCamIns = GameObject.Instantiate<GameObject>(fpsCam,mainCam.transform);
                if (fpsCam == null)
                {
                    Debug.LogError("fpsCam 자체가 리소스에서 읽어오기 불가능.");
                }

                FPSCamPerHeroGO = GameObject.Instantiate<GameObject>(FPSCamPerHeroGO);
                if (FPSCamPerHeroGO == null)
                {
                    Debug.LogError("fpsCam 히어로가 생성 불가능.");
                }
                FPSCamPerHeroGO.transform.SetParent(fpsCamIns.transform);
                FPSCamPerHeroGO.transform.SetPositionAndRotation(FPSCamPerHeroGO.transform.parent.position, FPSCamPerHeroGO.transform.parent.rotation);

                FPSCamPerHero = FPSCamPerHeroGO.GetComponent<FPSCameraPerHero>();
                if (FPSCamPerHero == null)
                {
                    Debug.LogError("영웅별 fps cam 이 어태치 되지 않았음.");
                }
            }
            else
            {
                rb.isKinematic = true;
            }
        }

        protected virtual void SetActiveCtrls()//스킬 세팅
        {
            activeCtrlDic.Clear();
        }
        public virtual void MoveHero(Vector3 moveV)
        {
            transform.Translate(moveV * moveSpeed * Time.deltaTime, Space.Self);
        }
        public virtual void RotateHero(Vector3 rotateV)
        {
            float nowCamRotX = Camera.main.transform.localRotation.eulerAngles.x;
            float nextCamRotX = nowCamRotX + rotateV.y * rotateSpeed * Time.deltaTime;

            if (rotateYDownLimit < nextCamRotX && nextCamRotX < rotateYUpLimitBy360toQuarternion) //리밋 오버 구간
            {
                if (nowCamRotX < nextCamRotX)
                {
                    Camera.main.transform.localRotation = Quaternion.Euler(rotateYDownLimit, 0, 0);
                }
                else if (nowCamRotX > nextCamRotX)
                {
                    Camera.main.transform.localRotation = Quaternion.Euler(rotateYUpLimitBy360toQuarternion, 0, 0);
                }
            }
            else
            {
                Camera.main.transform.Rotate(new Vector3(rotateV.y, 0, 0)*Time.deltaTime * rotateSpeed, Space.Self);
            }
            //실제 몸통 이동.
            transform.Rotate(new Vector3(0, rotateV.x, 0) * Time.deltaTime * rotateSpeed, Space.Self);
        }
        public virtual void ControlHero(E_ControlParam param)
        {
        }
        [PunRPC]
        public virtual void GetDamaged(float damage , int attackerPhotonViewID)
        {
            if (IsDie) return;
            currHP -= damage;
            MyDebug.Log("Hero::GetDamaged - " + photonView.ViewID+ "GetDamaged RPC Called . "+damage);
            if (currHP <= 0)
            {
                MyDebug.Log("Hero::GetDamaged - " + photonView.ViewID + " 사망.");
                Hero attacker = TeamInfo.GetInstance().HeroPhotonIDDic[attackerPhotonViewID];
                if (attacker == null)
                {
                    MyDebug.Log("Hero::GetDamaged - " + photonView.ViewID + "RPC Called dmg = " + damage + " , but 공격자가 존재하지 않음." + attackerPhotonViewID);
                }
                else
                {
                    if (attackerPhotonViewID == photonView.ViewID)  //내가 나를 공격자로 두고 겟 데미지를 부름 - 낙사한 경우임.
                    {
                        ShowKillLog(null, HeroType);
                    }
                    else
                    ShowKillLog(attacker.PlayerName,attacker.HeroType);
                }
                dieAction();
            }
            else if ( TeamInfo.GetInstance().HeroPhotonIDDic[attackerPhotonViewID].gameObject.layer == TeamInfo.GetInstance().MyTeamLayer
                //어태커의 팀이 "이" 클라이언트와 같은 팀이라면 피격당한 이 히어로의 피격 효과를 줌.  
               )
            {
                SetBeAttackedEffect(3f);
            }
        }

        public void ShowKillLog(string kn, E_HeroType kt)
        {
            InGameUIManager.Instance.ShowKillLog(kn,kt,playerName,heroType);
        }


        [PunRPC]
        public virtual void GetHealed(float heal)
        {
            if (IsDie) return;
            currHP += heal;
            if (currHP > maxHP)
            {
                currHP = maxHP;
            }
            MyDebug.Log("Hero::GetHealed - "+photonView.ViewID + " RPC Called . " +heal);
        }
        [PunRPC]
        public virtual void GetBadState(E_BadState state, float howMuch)
        {
            if (IsDie || !photonView.IsMine) return;
            badStateMgr.GetBadState(state, howMuch);
        }
        
        public virtual void PlusUltAmount(float value)
        {
            if (nowUltAmount < neededUltAmount)
            {
                nowUltAmount += value;
            }
            if (nowUltAmount > neededUltAmount)
            {
                nowUltAmount = neededUltAmount;
            }
        }
        public float UltAmountPercent
        {
            get
            {
                return nowUltAmount * neededUltAmountDiv;
            }
        }

        public virtual float GetReUseRemainTime(E_ControlParam param)
        {
            return activeCtrlDic[param].ReUseRemainingTime;
        }
        public virtual float GetReUseRemainTimeByZeroToOne(E_ControlParam param)
        {
            return activeCtrlDic[param].ReUseRemainingTimeInAZeroToOne;
;        }
        
        public virtual bool IsCannotMoveState()
        {
            if (badStateMgr.CanNotMoveState()  || IsDie)
                return true;
            return false;
        }
        public virtual bool IsCannotActiveState()
        {
            if (badStateMgr.CanNotActiveState() || IsDie)
                return true;
            return false;
        }
        
        protected E_MoveDir GetMostMoveDir(Vector3 moveDir)
        {
            E_MoveDir dir = E_MoveDir.NONE;
            if (moveDir.sqrMagnitude < Mathf.Epsilon)
                return dir;

            float x = moveDir.x;
            float z = moveDir.z;
            if (Mathf.Abs(x) > Mathf.Abs(z))
            {
                if (x > Mathf.Epsilon)
                    dir = E_MoveDir.Right;
                else dir = E_MoveDir.Left;
            }
            else
            {
                if (z > Mathf.Epsilon)
                    dir = E_MoveDir.Forward;
                else dir = E_MoveDir.Backward;
            }
            return dir;
        }

        public bool IsHeadShot(Vector3 worldHitPos)
        {
            if (transform.worldToLocalMatrix.MultiplyPoint3x4(worldHitPos).y > headShotOffset)
                return true;
            return false;
        }

        /*
         넉백 등이 일어날 때 호출.
             */
        [PunRPC]
        public void Knock(Vector3 worldForceVector)
        {
            if (photonView.IsMine)  //위치 동기화는 photon transform view
            {
                MyDebug.Log("Hero::Knock - "+photonView.ViewID+" RPC Called . "+ worldForceVector);
                rb.AddForce(worldForceVector, ForceMode.Force); 
            }
        }

        //갈고리에 걸린 위치, 땡겨올 위치. 몇 초 동안 의 정보
        [PunRPC]
        public void Hooked(Vector3 hookedStartWorldPos, Vector3 hookedDestWorldPos, float duration)
        {
            if (!photonView.IsMine)
                return;
            transform.position = hookedStartWorldPos;
            StartCoroutine(HookedMove(hookedStartWorldPos, hookedDestWorldPos, duration));
        }

        IEnumerator HookedMove(Vector3 hookedStartWorldPos, Vector3 hookedDestWorldPos, float duration)
        {
            Quaternion startRot = transform.rotation;
            Quaternion destRot = Quaternion.LookRotation(hookedDestWorldPos -  hookedStartWorldPos );
            destRot = new Quaternion(0,destRot.y,0,destRot.w);

            float startTime = 0f;
            float durationDiv = 1 / duration;

            while (startTime < duration)
            {
                startTime += Time.deltaTime;
                float progress = startTime * durationDiv;
                transform.position = Vector3.Lerp(hookedStartWorldPos, hookedDestWorldPos, progress);
                transform.rotation = Quaternion.Lerp(startRot, destRot, progress);
                yield return null;
            }
            transform.SetPositionAndRotation(hookedDestWorldPos, destRot);
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (!photonView.IsMine) return;
            if (other.CompareTag(Constants.outLineTag))
            {
                MyDebug.Log("Hero :: OnTriggerEnter - "+photonView.ViewID+ "아웃 라인 접촉. 낙사.");
                photonView.RPC("GetDamaged", RpcTarget.All, 99999f, photonView.ViewID);
            }
        }

        protected virtual void DieCallBack()
        {
            if (IsDie || !photonView.IsMine)
                return;
            MyDebug.Log("Hero::DieCallBack - "+photonView.ViewID);

            IsDie = true;
            photonView.RPC("RPCIsDie", RpcTarget.Others, IsDie);

            anim.SetTrigger("die");
            Camera.main.transform.Translate(Vector3.up * 2.0f, Space.World);
            Camera.main.transform.LookAt(transform.position);

            coll.enabled = false;
            photonView.RPC("ColliderOnOff", RpcTarget.Others, false);

            GetRigidBody.isKinematic = true;

            Invoke("Respawn", respawnTime);
        }

        [PunRPC]
        public void ColliderOnOff(bool on)
        {
            coll.enabled = on;
        }
        [PunRPC]
        public void RPCIsDie(bool die)
        {
            IsDie = die;
        }

        protected virtual void Respawn()
        {
            if (!photonView.IsMine|| IsDie==false) return;
            anim.SetTrigger("alive");
            coll.enabled = true;
            photonView.RPC("ColliderOnOff", RpcTarget.Others, true);
            
            GetRigidBody.isKinematic = false;

            Camera.main.transform.SetPositionAndRotation(camPos.position, camPos.rotation);
            IsDie = false;
            photonView.RPC("RPCIsDie", RpcTarget.Others, IsDie);

            currHP = maxHP;
            photonView.RPC("GetHealed", RpcTarget.Others, 999999f);
            

            int photonKey = photonView.ViewID / 1000;
            string teamName = NetworkManager.instance.Teams[photonKey];
            Transform spawnPoint = null;

            if (teamName == Constants.teamA_LayerName)
            {
                spawnPoint = MapInfo.instance.ASpawnPoint;
            }
            else if (teamName == Constants.teamB_LayerName)
            {
                spawnPoint = MapInfo.instance.BSpawnPoint;
            }
            else if (teamName == Constants.teamC_LayerName)
            {
                spawnPoint = MapInfo.instance.CSpawnPoint;
            }
            else if (teamName == Constants.teamD_LayerName)
            {
                spawnPoint = MapInfo.instance.DSpawnPoint;
            }
            if (spawnPoint == null)
            {
                MyDebug.Log("Hero :: Respawn - Cannot find Spawn Point.");
                spawnPoint = MapInfo.instance.ASpawnPoint;
            }
            transform.position = spawnPoint.position + Vector3.up * 2f;
                transform.rotation = spawnPoint.rotation;
        }

        public virtual void SetOutLineAndRim(float outLineWidth,Color outLineColor)
        {
            heroRendererMgr.SetOutLine(outLineWidth, outLineColor);
            heroRendererMgr.SetRimColor(outLineColor);
        }

        [SerializeField]
        float beAttackedReservationTime;
        Coroutine beAttackedEffectCoroutine;

        float bePenetratedReservationTime=0f;
        Coroutine bePenetratedCoroutine;
        
        public virtual void SetPenetrateVision(float time)
        {
            float newPenetratedTime = Time.time + time;
            if (newPenetratedTime > bePenetratedReservationTime)
            {
                if (bePenetratedCoroutine != null) StopCoroutine(bePenetratedCoroutine);
                bePenetratedReservationTime = newPenetratedTime;
                bePenetratedCoroutine = StartCoroutine(BePenetrated(time)); 
            }
        }
        IEnumerator BePenetrated(float time)
        {
            heroRendererMgr.SetOcclude(true);
            yield return new WaitForSeconds(time);
            heroRendererMgr.SetOcclude(false);
        }

        public virtual void SetBeAttackedEffect(float occludeTime)
        {
            if (photonView.IsMine) return;  //다른 클라이언트의 캐릭터 일때 피격 효과 발생.
            
            float newOccludeReserveTime = Time.time + occludeTime;
            
            if (beAttackedReservationTime < newOccludeReserveTime) //새롭게 들어온 오클루드가 더 길때
            {
                beAttackedReservationTime = newOccludeReserveTime;

                if(beAttackedEffectCoroutine !=null)
                StopCoroutine(beAttackedEffectCoroutine);
                
                beAttackedEffectCoroutine = StartCoroutine(BeAttackedEffect(occludeTime));
            }
        }

        IEnumerator BeAttackedEffect( float occludeTime)
        {
            if (occludeTime == 0f)
            {
                hpBar.Show(false);
                heroRendererMgr.PlusRimWidth(0f);
                yield break;
            }

            hpBar.Show(true);
            heroRendererMgr.PlusRimWidth(0f);

            float nowTime = 0f;
            float wholeTimeDiv = 1 / occludeTime;
            float maxPlusRim = 0.8f;   //최대 림 증가하는 값.

            while (nowTime <= occludeTime)
            {
                nowTime += Time.deltaTime;

                float progress = nowTime * wholeTimeDiv;//0~1 값.
                if (progress < 0.2f)
                {
                    heroRendererMgr.PlusRimWidth(maxPlusRim * (progress*  5 ));
                }
                else if(progress >0.8f)
                {
                    heroRendererMgr.PlusRimWidth(maxPlusRim - (maxPlusRim * ((progress -0.8f) * 5))  );
                }
                yield return null;
            }
            heroRendererMgr.PlusRimWidth(0f);
            
            hpBar.Show(false);
        }
        
        public bool IsSetBadState(E_BadState state)
        {
            return badStateMgr.IsSetBadState(state);
        }
    }
}