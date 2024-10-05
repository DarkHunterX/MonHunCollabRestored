using BepInEx.Unity.IL2CPP.Utils;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System;
using System.Collections;
using UnityEngine;
using MonHunCollabRestored.Character;
using Tangerine.Patchers.LogicUpdate;

namespace MonHunCollabRestored.Beambullet
{
    public class ValstraLaser_BeamBullet : MonoBehaviour, ITangerineLogicUpdate
    {
        #region Basic Setup (Il2Cpp)
        public ValstraLaser_BeamBullet(IntPtr ptr) : base(ptr) { }

        public ValstraLaser_BeamBullet() : base(ClassInjector.DerivedConstructorPointer<ValstraLaser_BeamBullet>())
        {
            ClassInjector.DerivedConstructorBody(this);
        }
        #endregion

        #region Basic Setup (Logic Update)
        public System.IntPtr LogicPointer => this.Pointer;
        public void LogicUpdate()
        {
            if (this.bStartTurn)
            {
                if (this.secortCollider == null)
                    return;

                float z = base.transform.localEulerAngles.z;
                if (this.nShootDirection == 1)
                {
                    this.secortCollider.UpdateAngle(this.oldAngle, z);
                }
                else
                {
                    this.secortCollider.UpdateAngle(z, this.oldAngle);
                }
                this.oldAngle = z;
            }
        }
        #endregion

        public void Setup(BeamBullet _bullet)
        {
            _beamBullet = _bullet;
            if (_beamBullet.CoroutineMove != null)
            {
                _beamBullet.StopCoroutine(_beamBullet.CoroutineMove);
            }

            GetBulletInfo();
            _beamBullet.CoroutineMove = _beamBullet.StartCoroutine(CH106_BeamBullet_StartMove());
        }

        public IEnumerator CH106_BeamBullet_StartMove()
        {
            _beamBullet.IsActivate = true;
            _beamBullet._hitCollider.enabled = true;
            TangerineLogicUpdateManager.AddUpdate(this);
            _beamBullet._clearTimer.TimerReset();
            _beamBullet._clearTimer.TimerStart();
            _beamBullet._durationTimer.TimerReset();
            _beamBullet._durationTimer.TimerStart();

            if (_beamBullet.refPBMShoter.SOB != null)
            {
                _pOwner = _beamBullet.refPBMShoter.SOB.GetComponent<CH106_Controller>();
            }

            //Plugin.Log.LogInfo($"Is SubBeam: {_beamBullet.isSubBullet} - bStartTurn {bStartTurn} - Owner Exist? {_pOwner != null} - Destroyed? {_beamBullet.IsDestroy}");
            //if (_beamBullet.isSubBullet)
                //Plugin.Log.LogInfo("============================");
            while (!_beamBullet.IsDestroy)
            {
                if (_beamBullet._clearTimer.GetMillisecond() >= _beamBullet._hurtCycle)
                {
                    _beamBullet._clearTimer.TimerStart();
                    _beamBullet._ignoreList.Clear();
                    _beamBullet._rigidbody2D.WakeUp();
                }

                if (!bStartTurn && _beamBullet.isSubBullet)
                {
                    DirectonTurn();
                }
                    
                               
                if (_beamBullet._duration != -1L && _beamBullet._durationTimer.GetMillisecond() >= _beamBullet._duration && !bGamePause)
                {
                    _beamBullet.IsDestroy = true;
                    if (_beamBullet.isSubBullet == false)
                    {

                        bool flag = true;
                        bool turn = _pOwner.BeamStartTurn();
                        flag = _pOwner != null ? turn : false;
                        
                        if (flag == true)
                        {
                            CreateSubBeam();
                        }
                        else
                        {
                            OrangeCharacter.MainStatus mainStatus = _pOwner._refEntity.CurMainStatus;
                            OrangeCharacter.SubStatus curSubStatus = _pOwner._refEntity.CurSubStatus;

                        }

                    }
                    //if (!)
                    //{
                    //    bool flag = true;
                    //    if (_pOwner != null)
                    //    {
                    //        flag = _pOwner.BeamStartTurn();
                    //    }
                    //    if (flag)
                    //    {
                    //        Plugin.Log.LogInfo("Create Sub Beam");
                    //        CH106_01_BeamBullet ch106_BeamBullet = CreateSubBeam();
                    //        if (ch106_BeamBullet)
                    //        {
                    //            Plugin.Log.LogInfo("Sub Beam Rotated");
                    //            ch106_BeamBullet.DirectonTurn();
                    //        }
                    //    }
                    //}
                    //else
                    //{
                    //    Plugin.Log.LogInfo("Beam is SubBullet");
                    //}
                }

                if (_beamBullet.AlwaysFaceCamera)
                {
                    _beamBullet.transform.LookAt(_beamBullet._mainCamera.transform.position, -Vector3.up);
                }
                yield return CoroutineDefine._waitForEndOfFrame;
            }
            this.BackToPoolLaser();
            _beamBullet.BackToPool();
            yield return null;
            yield break;
        }

        public void UpdateEffect()
        {
            _beamBullet.bIsEnd = false;
            if (!bInit)
            {
                bInit = true;
                float num = _beamBullet._hitCollider.Cast<BoxCollider2D>().size.x - defLength;
                _beamBullet.fxEndpoint.localPosition = _beamBullet.fxEndpoint.localPosition + new Vector3(0f, 0f, num);
                fxLine01.SetPosition(0, new Vector3(fxLine01.GetPosition(0).x - num, 0f, fxLine01.GetPosition(0).z));
                fxLine01A.SetPosition(0, new Vector3(fxLine01A.GetPosition(0).x - num, 0f, fxLine01A.GetPosition(0).z));
                fxLine02.SetPosition(0, new Vector3(fxLine02.GetPosition(0).x - num, 0f, fxLine02.GetPosition(0).z));
                fxLine02A.SetPosition(0, new Vector3(fxLine02A.GetPosition(0).x - num, 0f, fxLine02A.GetPosition(0).z));
                fxLightning00.SetPosition(0, new Vector3(fxLightning00.GetPosition(0).x - num, 0f, fxLightning00.GetPosition(0).z));
                fxLightning00_Black.SetPosition(0, new Vector3(fxLightning00_Black.GetPosition(0).x - num, 0f, fxLightning00_Black.GetPosition(0).z));
                SetLightning(fxLightning, num);
                SetSS1(fxSs1, 5.0f, 5.0f,num);
                SetSS1(fxLL001, 5.0f, 4.0f, num);
                SetSS1(fxLL002, 5.0f, 4.0f, num);
                SetSS1(fxLL001A, 5.0f, 4.0f, num);
                SetSS1(fxLL002A, 5.0f, 4.0f, num);
            }
        }

        public void OnApplicationPause(bool pause)
        {
            BulletPause(pause);
        }

        private void SetLightning(ParticleSystem ps, float difLength)
        {
            ParticleSystem.MainModule main = ps.main;
            ParticleSystem.MinMaxCurve startSizeY = main.startSizeY;
            startSizeY.constantMin = (0.6f + difLength) * 0.75f;
            startSizeY.constantMax = (0.6f + difLength) * 0.75f;
            main.startSizeY = startSizeY;
            
        }

        private void SetSS1(ParticleSystem ps, float min, float max, float difLength)
        {
            ParticleSystem.MainModule main = ps.main;
            ParticleSystem.MinMaxCurve startSizeX = main.startSizeX;
            startSizeX.constantMin = min + (difLength * 0.5f);
            startSizeX.constantMax = max + (difLength * 0.5f);
            main.startSizeX = startSizeX;
        }

        public void DirectonTurn()
        {
            ActiveExtraCollider();
            _fStartAngle = _beamBullet.transform.localEulerAngles.z;
            if (_beamBullet.transform.localEulerAngles.z == 270f)
            {
                if (_beamBullet.refPBMShoter.SOB != null && _beamBullet.refPBMShoter.SOB.direction == 1)
                {
                    _fStartAngle = _beamBullet.transform.localEulerAngles.z - 360f;
                }
            }
            else if (_beamBullet.transform.localEulerAngles.z > 270f)
            {
                _fStartAngle = _beamBullet.transform.localEulerAngles.z - 360f;
            }
            if (_beamBullet.transform.localEulerAngles.z < 90f || _beamBullet.transform.localEulerAngles.z > 270f)
            {
                nShootDirection = 1;
            }
            else
            {
                nShootDirection = -1;
            }
            bStartTurn = true;
            oldAngle = _beamBullet.transform.localEulerAngles.z;
            LeanTween.value(_beamBullet.transform.gameObject, _fStartAngle, 90f, (float)(_beamBullet._duration - 50L) * 0.001f).setOnUpdate(new System.Action<float>((float val) =>
            {
                _beamBullet.transform.localEulerAngles = new Vector3(0f, 0f, val);
            })).setEaseInQuart();
        }

        private void ActiveExtraCollider()
        {
            if (this.secortCollider == null)
                return;
            
                BoxCollider2D boxCollider2D = _beamBullet._hitCollider.Cast<BoxCollider2D>();
                if (boxCollider2D == null)
                    return;
                
            this.secortCollider.Active(_beamBullet, boxCollider2D.size.x);
        }

        public void CreateSubBeam()
        {
            int n_LINK_SKILL = _beamBullet.BulletData.n_LINK_SKILL;
            //Plugin.Log.LogInfo($"Bullet ID: {_beamBullet.BulletData.n_ID}");
            if (n_LINK_SKILL == 0)
                return;
            //Plugin.Log.LogInfo("Create SubBeam");
           SKILL_TABLE skill_TABLE = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[n_LINK_SKILL].GetSkillTableByValue();
            if (_beamBullet.refPBMShoter.SOB.Cast<OrangeCharacter>() != null)
            {
                (_beamBullet.refPBMShoter.SOB.Cast<OrangeCharacter>()).tRefPassiveskill.ReCalcuSkill(ref skill_TABLE);
            }

            BeamBullet ch106_BeamBullet = null;
            if (MonoBehaviourSingleton<PoolManager>.Instance.IsPreload("p_valstraxlaser_000_01"))
            {
                ch106_BeamBullet = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<BeamBullet>("p_valstraxlaser_000_01");
            }
            if (ch106_BeamBullet == null)
            {
                Plugin.Log.LogError("Can't find p_valstraxlaser_000_01 in the pool");
                return;
            }
            WeaponStatus weaponStatus = new WeaponStatus();
            weaponStatus.nHP = _beamBullet.nHp;
            weaponStatus.nATK = _beamBullet.nOriginalATK;
            weaponStatus.nCRI = _beamBullet.nOriginalCRI;
            weaponStatus.nHIT = _beamBullet.nHit - _beamBullet.refPSShoter.GetAddStatus(8, _beamBullet.nWeaponCheck);
            weaponStatus.nCriDmgPercent = _beamBullet.nCriDmgPercent;
            weaponStatus.nReduceBlockPercent = _beamBullet.nReduceBlockPercent;
            weaponStatus.nWeaponCheck = _beamBullet.nWeaponCheck;
            weaponStatus.nWeaponType = _beamBullet.nWeaponType;
            PerBuffManager.BuffStatus buffStatus = new PerBuffManager.BuffStatus();
            buffStatus.fAtkDmgPercent = _beamBullet.fDmgFactor - 100f;
            buffStatus.fCriPercent = _beamBullet.fCriFactor - 100f;
            buffStatus.fCriDmgPercent = _beamBullet.fCriDmgFactor - 100f;
            buffStatus.fMissPercent = _beamBullet.fMissFactor;
            buffStatus.refPBM = _beamBullet.refPBMShoter;
            buffStatus.refPS = _beamBullet.refPSShoter;
            ch106_BeamBullet.UpdateBulletData(skill_TABLE, _beamBullet.Owner, 0, 0, 1);
            ch106_BeamBullet.SetBulletAtk(weaponStatus, buffStatus, null);
            ch106_BeamBullet.BulletLevel = _beamBullet.BulletLevel;
            ch106_BeamBullet.isSubBullet = true;
            ch106_BeamBullet.transform.SetPositionAndRotation(_beamBullet._transform.position, Quaternion.identity);
            ch106_BeamBullet.Active(_beamBullet._transform.position, _beamBullet.Direction, _beamBullet.TargetMask, null);
        }

        public void GetBulletInfo()
        {
            Il2CppReferenceArray<Transform> componentsInChildren = _beamBullet._transform.GetComponentsInChildren<Transform>(true).Cast<Il2CppReferenceArray<Transform>>();
            foreach (Transform component in componentsInChildren)
            {
                switch (component.name)
                {
                    case "p_sfbeam_001":
                        {
                            fxLine01 = component.gameObject.GetComponent<LineRenderer>();
                            Transform[] temp = component.GetComponentsInChildren<Transform>(true);
                            foreach (Transform child in temp)
                            {
                                if (child.name.Equals("loop1_6023_A"))
                                {
                                    fxLine01A = component.gameObject.GetComponent<LineRenderer>();
                                    component.gameObject.SetActive(false);
                                    break;
                                }
                            }
                            break;
                        }
                    case "p_sfbeam_002":
                        {
                            fxLine02 = component.gameObject.GetComponent<LineRenderer>();
                            Transform[] temp = component.GetComponentsInChildren<Transform>(true);
                            foreach (Transform child in temp)
                            {
                                if (child.name.Equals("loop1_6023_A"))
                                {
                                    fxLine02A = component.gameObject.GetComponent<LineRenderer>();
                                    component.gameObject.SetActive(false);
                                    break;
                                }
                            }
                            break;
                        }
                    case "lightning_6": fxLightning = component.gameObject.GetComponent<ParticleSystem>(); break;
                    case "ss1": fxSs1 = component.gameObject.GetComponent<ParticleSystem>(); break;
                    case "ll_001": fxLL001 = component.gameObject.GetComponent<ParticleSystem>(); break;
                    case "ll_002": fxLL002 = component.gameObject.GetComponent<ParticleSystem>(); break;
                    case "ll_001 (1)": fxLL001A = component.gameObject.GetComponent<ParticleSystem>(); break;
                    case "ll_002 (1)": fxLL002A = component.gameObject.GetComponent<ParticleSystem>(); break;
                    case "lightning00": fxLightning00 = component.gameObject.GetComponent<LineRenderer>(); break;
                    case "lightning00_black": fxLightning00_Black = component.gameObject.GetComponent<LineRenderer>(); break;
                }
            }

            ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetNewValue(_beamBullet.BulletData.n_ID, out BulletData);
        }

        public void BulletPause(bool pause)
        {
            bGamePause = pause;
            if (bGamePause)
            {
                _beamBullet._clearTimer.TimerPause();
                _beamBullet._durationTimer.TimerPause();
                return;
            }
            _beamBullet._clearTimer.TimerResume();
            _beamBullet._durationTimer.TimerResume();
        }

        public void BackToPoolLaser()
        {
            if (!_beamBullet.isSubBullet && _pOwner != null)
            {
                _pOwner.BeamStartTurn();
            }

            _pOwner = null;
            bStartTurn = false;
            bInit = false;
            TangerineLogicUpdateManager.RemoveUpdate(this);


            fxLine01 = null;
            fxLine01A = null;
            fxLine02 = null;
            fxLine02A = null;
            fxLightning = null;
            fxSs1 = null;
            fxLL001 = null;
            fxLL002 = null;
            fxLL001A = null;
            fxLL002A = null;
            fxLightning00 = null;
            fxLightning00_Black = null;
            //IsActived = false;
        }

        public BeamBullet _beamBullet;
        public SKILL_TABLE BulletData;

        protected float defLength = 8f;
        private LineRenderer fxLine01;
        private LineRenderer fxLine01A;
        private LineRenderer fxLine02;
        private LineRenderer fxLine02A;
        private ParticleSystem fxLightning;
        private ParticleSystem fxSs1;
        private ParticleSystem fxLL001;
        private ParticleSystem fxLL002;
        private ParticleSystem fxLL001A;
        private ParticleSystem fxLL002A;
        private LineRenderer fxLightning00;
        private LineRenderer fxLightning00_Black;
        private CH048_BeamSectorCollider secortCollider;

        private float _fStartAngle = 0f;
        private bool bStartTurn = false;
        private float oldAngle = 0f;
        protected int nShootDirection = 1;
        public bool bInit = false;
        private bool bGamePause = false;
        protected CH106_Controller _pOwner;
    }
}
