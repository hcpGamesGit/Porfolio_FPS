using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using Photon.Pun;

namespace hcp
{
    public class HeroSoldier : Hero
    {
        [Space(20)]
        [Header("Hero - Soldier's Property")]
        [Space(10)]

        [SerializeField]
        E_MoveDir lastDir = E_MoveDir.NONE;
        [SerializeField]
        Transform firePos;

        [SerializeField]
        HSHealDrone healDrone;

        [Space(10)]
        [Header("   Hero-Soldier-NormalAttack")]
        [Space(10)]

        [SerializeField]
        ParticleSystem normalMuzzleFlash;
        [SerializeField]
        GameObject normalAttackParticleParent;
        [Tooltip("Hero- Soldier normal Attack particle, Object Pooling.")]
        [SerializeField]
        ParticleSystem[] normalAttackParticles;

        WaitForSeconds normalParticleDuration;

        [SerializeField]
        float normalFireDamage;
        [Tooltip("fireRate for normal attack")]
        [SerializeField]
        float fireRate;
        [SerializeField]
        int currBullet;
        [SerializeField]
        int maxBullet;
        [SerializeField]
        GameObject HSMagazineUIPrefab;
        [SerializeField]
        GameObject HSBulletImage;
        [SerializeField]
        GameObject HSBulletImageBack;
        [SerializeField]
        GameObject HSMagazineUI;
        [SerializeField]
        Transform HSMagazineUICurrBulletImageParent;
        [SerializeField]
        UnityEngine.UI.Text currBulletUIText;

        [Space(10)]
        [Header("   Hero-Soldier-Reload")]
        [Space(10)]

        [SerializeField]
        bool reloading = false;

        [SerializeField]
        AnimationClip reloadClip;

        [Space(10)]
        [Header("   Hero-Soldier-First Skill Heal Drone")]
        [Space(10)]
        [SerializeField]
        float healDroneCoolTime;

        [Space(10)]
        [Header("   Hero-Soldier-Second Skill PenetrateVision")]
        [Space(10)]
        [SerializeField]
        HSPenetrateVision penetrateVision;
        [SerializeField]
        float penetrateCoolTime;


        [Space(10)]
        [Header("   Hero-Soldier-Ultimate")]
        [Space(10)]

        [SerializeField]
        GameObject ultMissileParent;
        [Tooltip("Hero- Soldier Ultimate, Object Pooling.")]
        [SerializeField]
        HSUltMissile[] ultMissiles;

        [Tooltip("Hero- Soldier Ultimate, max Missile Counts.")]
        [SerializeField]
        int ultMissilesMaxCount ;

        [SerializeField]
        int ultShootCount = 0;

        [Tooltip("Hero- Soldier Ultimate, max Missile Shot Time.")]
        [SerializeField]
        float ultMaxTime ;
        float ultMaxTimeDiv;

        [Tooltip("Hero- Soldier Ultimate, fire rate.")]
        [SerializeField]
        float ultFireRate;
        
        [SerializeField]
        bool isUltOn = false;

        [SerializeField]
        float ultActivateTime = 0f;

        [SerializeField]
        GameObject HSUltCrossHairPrefab;
        [SerializeField]
        GameObject HSUltCrossHair;
        [SerializeField]
        UnityEngine.UI.Image HSUltHalfCircleImage;


        private void Start()
        {
            penetrateVision.transform.SetParent(null);
            ultMissiles = new HSUltMissile[ultMissileParent.transform.childCount];
            for (int i = 0; i < ultMissiles.Length; i++)
            {
                ultMissiles[i] = ultMissileParent.transform.GetChild(i).GetComponent<HSUltMissile>();
                ultMissiles[i].DeActivate();
                ultMissiles[i].attachedNumber = i;
            }
            
            ultMissileParent.transform.SetParent(null);
            ultMissileParent.transform.position = Vector3.zero + Vector3.down * 5f; 

            if (photonView.IsMine)
            {
                HSMagazineUI = GameObject.Instantiate(HSMagazineUIPrefab, 
                    Vector3.zero,
                    Quaternion.identity, InGameUIManager.Instance.transform);
                HSMagazineUICurrBulletImageParent = HSMagazineUI.transform.GetChild(1);
                for (int i = 0; i < maxBullet; i++)
                {
                    GameObject.Instantiate(HSBulletImage, HSMagazineUICurrBulletImageParent);
                    GameObject.Instantiate(HSBulletImageBack, HSMagazineUI.transform.GetChild(0));
                }
                
                HSMagazineUI.transform.GetChild(2).GetComponent<UnityEngine.UI.Text>().text = "/"+maxBullet.ToString();
                currBulletUIText = HSMagazineUI.transform.GetChild(3).GetComponent<UnityEngine.UI.Text>() ;
                currBulletUIText.text = currBullet.ToString();
            }
        }

        protected override void Awake()
        {
            ultMaxTimeDiv = 1 / ultMaxTime;
            heroType = E_HeroType.Soldier;
            base.Awake();
            
            normalAttackParticles = new ParticleSystem[normalAttackParticleParent.transform.childCount];

            for (int i = 0; i < normalAttackParticles.Length; i++)
            {
                normalAttackParticles[i] = normalAttackParticleParent.transform.GetChild(i).GetComponent<ParticleSystem>();
                normalAttackParticles[i].gameObject.SetActive(false);
            }
            normalParticleDuration = new WaitForSeconds(normalAttackParticles[0].main.duration);
            normalAttackParticleParent.transform.SetParent(null);
            normalAttackParticleParent.transform.position = Vector3.zero + Vector3.down * 5f;
            
            currHP = maxHP;
            currBullet = maxBullet;
            nowUltAmount = 0f;
        }
        private void OnDestroy()
        {
            if (ultMissileParent != null)
                Destroy(ultMissileParent.gameObject);

            if (normalAttackParticleParent != null)
                Destroy(normalAttackParticleParent.gameObject);
            if (penetrateVision != null)
                Destroy(penetrateVision.gameObject);
        }

        protected override void SetActiveCtrls()
        {
            base.SetActiveCtrls();
            activeCtrlDic.Add(E_ControlParam.NormalAttack, new DelegateCtrl(E_ControlParam.NormalAttack, fireRate, NormalAttack,
               NormalAttackMeetCondition));
            activeCtrlDic.Add(E_ControlParam.FirstSkill, new DelegateCtrl(E_ControlParam.FirstSkill, healDroneCoolTime, FirstSkill_HealDrone, () => { return true; }));
            activeCtrlDic.Add(E_ControlParam.SecondSkill, new DelegateCtrl(E_ControlParam.SecondSkill, penetrateCoolTime, PenetrateVision, () => { return true; }));
            activeCtrlDic.Add(E_ControlParam.Reload, new DelegateCtrl(E_ControlParam.Reload, 1f, Reloading, ReloadingMeetCondition));
            activeCtrlDic.Add(E_ControlParam.Ultimate, new DelegateCtrl(E_ControlParam.Ultimate, ultFireRate, Ult_ShotMissile, () => { return true; }));
           
        }

        #region basic control 
        public override void MoveHero(Vector3 moveV)
        {
            if (!photonView.IsMine || IsCannotMoveState() )
            {
                return;
            }

            base.MoveHero(moveV);
            
            E_MoveDir dir = GetMostMoveDir(moveV);

            if (lastDir.Equals(dir)) return;
            lastDir = dir;
            anim.SetBool("forward", false);
            anim.SetBool("backward", false);
            anim.SetBool("left", false);
            anim.SetBool("right", false);
            anim.SetBool("idle", false);

            switch (dir)
            {
                case E_MoveDir.Forward:
                    anim.SetBool("forward", true);
                    break;
                case E_MoveDir.Backward:
                    anim.SetBool("backward", true);
                    break;
                case E_MoveDir.Left:
                    anim.SetBool("left", true);
                    break;
                case E_MoveDir.Right:
                    anim.SetBool("right", true);
                    break;
                case E_MoveDir.NONE:
                    anim.SetBool("idle", true);
                    break;
            }
        }

        public override void RotateHero(Vector3 rotateV)
        {
            if (!photonView.IsMine || IsCannotMoveState() )
            {
                return;
            }
            base.RotateHero(rotateV);
        }

        public override void ControlHero(E_ControlParam param)
        {
            if (!photonView.IsMine || IsCannotActiveState() )
            {
                return;
            }

            if (param == E_ControlParam.Ultimate)
            {
                if (UltAmountPercent < 1 && !isUltOn)
                    return;

                if (isUltOn)    //미사일 스킬 발사
                {
                    if (!activeCtrlDic[E_ControlParam.Ultimate].IsCoolTimeOver() || ultShootCount >= ultMissilesMaxCount)
                        return;
                    
                    activeCtrlDic[param].Activate();

                    if (HSUltCrossHair != null)
                    {
                        Transform ultMagazine = HSUltCrossHair.transform.GetChild(0);
                        for (int i = 0; i < ultShootCount; i++)
                        {
                            ultMagazine.GetChild(i).gameObject.SetActive(false);
                        }
                    }

                    return;
                }
                else
                {
                    //미사일 스킬 발동(초기화)
                    InGameUIManager.Instance.CrossHairChange(crossHairs[1]);
                    nowUltAmount = 0f;
                    isUltOn = true;
                    ultShootCount = 0;
                    ultActivateTime = 0f;
                    activeCtrlDic[param].Activate();

                    HSUltCrossHair = GameObject.Instantiate(HSUltCrossHairPrefab, new Vector3(Screen.width/2,Screen.height/2,0),Quaternion.identity, InGameUIManager.Instance.transform);
                    HSUltHalfCircleImage = HSUltCrossHair.GetComponent<UnityEngine.UI.Image>();
                    Transform ultMagazine = HSUltCrossHair.transform.GetChild(0);
                    for (int i = 0; i < ultShootCount; i++)
                    {
                        ultMagazine.GetChild(i).gameObject.SetActive(false);
                    }

                    return;
                }
            }


            if (!activeCtrlDic[param].IsCoolTimeOver())
                return;
            MyDebug.Log(param + "입력 - 쿨타임 검사통과");

            activeCtrlDic[param].Activate();
        }

        #endregion


        #region Normal Attack
        bool NormalAttackMeetCondition()
        {
            if (isUltOn) return false;
            if (currBullet <= 0)
            {
                Reloading();
                return false;
            }
            return true;
        }
        void NormalAttack()
        {
            currBullet--;
            for (int i = 0; i < maxBullet- currBullet; i++)
            {
                HSMagazineUICurrBulletImageParent.transform.GetChild(i).gameObject.SetActive(false);
            }
            currBulletUIText.text = currBullet.ToString();

            photonView.RPC("normalMuzzleFlashPlay", RpcTarget.Others);//다른 클라이언트의 발사 시각효과
            FPSCamPerHero.FPSCamAct (E_ControlParam.NormalAttack);// 나자신의 시각효과만 담당.
            anim.SetTrigger("shot");
            
            Ray screenCenterRay = Camera.main.ScreenPointToRay(screenCenterPoint);
            RaycastHit firstRayHit = ShotCtrl.GetFirstHitAsMapOrEnemy(this, screenCenterRay, maxShotLength);
            photonView.RPC("normalHitParticle", RpcTarget.All, firstRayHit.point);  //피격 파티클 효과.
            
            if (TeamInfo.GetInstance().IsThisLayerEnemy(firstRayHit.collider.gameObject.layer))//적 타격.
            {
                Hero hitEnemyHero = firstRayHit.collider.gameObject.GetComponent<Hero>();
                if (hitEnemyHero == null) return;

                MyDebug.Log("HS-NormalAttack 적 타격. 이름 = "+hitEnemyHero.name+"photon ID = "+hitEnemyHero.photonView.ViewID);
                float damage = normalFireDamage;

                if (hitEnemyHero.IsHeadShot(firstRayHit.point))
                {
                    MyDebug.Log("HS-NormalAttack 적 타격 헤드샷. 이름 = " + hitEnemyHero.name + "photon ID = " + hitEnemyHero.photonView.ViewID);
                    damage *= 2f;
                }
               
                hitEnemyHero.photonView.RPC("GetDamaged", RpcTarget.All, damage, photonView.ViewID);
            }
        }

        [PunRPC]
        void normalMuzzleFlashPlay()
        {
            normalMuzzleFlash.Play();
        }
        [PunRPC]
        void normalHitParticle(Vector3 hitPos)
        {
            StartCoroutine(NormalAttackParticlePlay(hitPos));
        }
        IEnumerator NormalAttackParticlePlay(Vector3 hitPos)
        {
            ParticleSystem temp = null;
            for (int i = 0; i < normalAttackParticles.Length; i++)
            {
                if (normalAttackParticles[i].gameObject.activeSelf == false)
                {
                    temp = normalAttackParticles[i];
                    break;
                }
            }
            if (temp != null)
            {
                temp.gameObject.SetActive(true);
                temp.transform.position = hitPos;
                temp.Play();
                yield return normalParticleDuration;
                temp.gameObject.SetActive(false);
            }
        }

        #endregion
        
        #region First Skill HealDrone

        void FirstSkill_HealDrone() 
        {
            if (!photonView.IsMine) return;
            photonView.RPC("DroneAppear", RpcTarget.All);
            healDrone.Activate();
        }

        [PunRPC]
        void DroneAppear()
        {
            healDrone.Appear();
        }
        [PunRPC]
        public void DroneDisAppear()
        {
            healDrone.DisAppear();
        }
       
        public void DroneHeal(Hero healedHero, float healAmount)
        {
            healedHero.photonView.RPC("GetHealed",RpcTarget.All,  healAmount);
        }
        
        #endregion

        #region Second Skill - Penetrate Vision
        void PenetrateVision()
        {
            Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
            photonView.RPC("PenetrateVisionActivate", RpcTarget.All, ray.origin + ray.direction*1f,ray.direction);
        }
        [PunRPC]
        public void PenetrateVisionActivate(Vector3 origin, Vector3 direction)
        {
            penetrateVision.Activate( origin,  direction);
        }
        [PunRPC]
        public void PenetrateVisionSettle(Vector3 position)
        {
            penetrateVision.Settle(position);
        }
        [PunRPC]
        public void PenetrateVisionDeActivate()
        {
            penetrateVision.DeActivate();
        }
        
        #endregion

        #region Reloading

        bool ReloadingMeetCondition()
        {
            if (isUltOn) return false;
            if (currBullet == maxBullet) return false;
            return true;
        }

        void Reloading()
        {
            reloading = true; 
            FPSCamPerHero.FPSCamAct(E_ControlParam.Reload);

            StartCoroutine(ReloadingCheck());
            currBullet = maxBullet;
            
            for (int i = 0; i < maxBullet; i++)
            {
                HSMagazineUICurrBulletImageParent.transform.GetChild(i).gameObject.SetActive(true);
            }
            currBulletUIText.text = currBullet.ToString();
        }

        IEnumerator ReloadingCheck()
        {
            anim.SetTrigger("reload");
            yield return new WaitForSeconds(reloadClip.length);
            reloading = false;
        }
        #endregion

        #region Ultimate

        void Ult_ShotMissile()
        {
            Vector3 startPos = firePos.position;
            Ray screenCenterRay = Camera.main.ScreenPointToRay(screenCenterPoint);
            Vector3 targetVector = screenCenterRay.direction * maxShotLength;
            Quaternion dir = Quaternion.LookRotation((targetVector - startPos)); 
            
#if UNITY_EDITOR
            Debug.DrawLine(startPos, targetVector, Color.blue, 5f);
#endif
            photonView.RPC("ShootUltimate", RpcTarget.All, ultShootCount, firePos.position, dir);
            ultShootCount++;
        }
        
        [PunRPC]
        void ShootUltimate(int num, Vector3 shootStartPos, Quaternion shootStartRot)
        {
            ultMissiles[num].Activate(shootStartPos, shootStartRot);
        }
        [PunRPC]
        void BoomUltMissile(int num, Vector3 boomedPos)
        {
            ultMissiles[num].Boom(boomedPos);
        }

        #endregion

        public override float GetReUseRemainTimeByZeroToOne(E_ControlParam param)
        {
            if (param == E_ControlParam.Ultimate)
            {
                if (isUltOn)
                {
                    return activeCtrlDic[param].ReUseRemainingTimeInAZeroToOne;
                }
                else {
                    return 1- UltAmountPercent;
                }
            }
            if (isUltOn&& param!=E_ControlParam.FirstSkill)
            {
                return 1;
            }
            return activeCtrlDic[param].ReUseRemainingTimeInAZeroToOne;
            ;
        }
        public override bool IsCannotActiveState()
        {
            if (reloading)
            {
                return true;
            }
            return base.IsCannotActiveState();
        }

        private void Update()
        {
            if (!photonView.IsMine) return;

            if (!isUltOn)
            {
                PlusUltAmount(ultPlusPerSec * Time.deltaTime);
            }
            else
            {
                ultActivateTime += Time.deltaTime;

                if (HSUltCrossHair != null && HSUltHalfCircleImage != null)
                {
                    HSUltHalfCircleImage.fillAmount=(1 - (ultActivateTime * ultMaxTimeDiv))*0.5f;  //0.5f는 이미지가 하프 서클이라서 붙인 값
                }

                if (ultActivateTime > ultMaxTime || ultShootCount >= ultMissilesMaxCount)
                {
                    isUltOn = false;    //미사일 스킬 종료 시점.

                    InGameUIManager.Instance.CrossHairChange(crossHairs[0]);
                    if (HSUltCrossHair != null)
                    {
                        Destroy(HSUltCrossHair);
                    }
                }
            }
        }
    }
}