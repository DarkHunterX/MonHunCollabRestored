using BepInEx.Unity.IL2CPP.Utils.Collections;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MonHunCollabRestored.Character;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tangerine.Patchers.LogicUpdate;
using Tangerine.Utils;
using UnityEngine;

namespace MonHunCollabRestored.BeambulletEx
{
    public class ValstraxLaser_BeamBullet : BeamBullet, ITangerineLogicUpdate
    {
        #region Basic Setup (Il2Cpp)
        public ValstraxLaser_BeamBullet(IntPtr ptr) : base(ptr) { }

        public ValstraxLaser_BeamBullet() : base(ClassInjector.DerivedConstructorPointer<ValstraxLaser_BeamBullet>())
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

        public override Il2CppSystem.Collections.IEnumerator OnStartMove()
        {
            return CH106_BeamBullet_StartMove().WrapToIl2Cpp();
        }

        public IEnumerator CH106_BeamBullet_StartMove()
        {
            this.IsActivate = true;
            this._hitCollider.enabled = true;
            TangerineLogicUpdateManager.AddUpdate(this);
            this._clearTimer.TimerReset();
            this._clearTimer.TimerStart();
            this._durationTimer.TimerReset();
            this._durationTimer.TimerStart();

            if (this.refPBMShoter.SOB != null)
            {
                _pOwner = this.refPBMShoter.SOB.GetComponent<CH106_Controller>();
            }

            while (!this.IsDestroy)
            {
                if (this._clearTimer.GetMillisecond() >= this._hurtCycle)
                {
                    this._clearTimer.TimerStart();
                    this._ignoreList.Clear();
                    this._rigidbody2D.WakeUp();
                }

                if (!bStartTurn && this.isSubBullet)
                    DirectonTurn();

                if (this._duration != -1L && this._durationTimer.GetMillisecond() >= this._duration && !bGamePause)
                {
                    this.IsDestroy = true;
                    if (this.isSubBullet == false)
                    {
                        bool flag = true;
                        flag = _pOwner != null ? _pOwner.BeamStartTurn() : false;

                        if (flag == true)
                            CreateSubBeam();
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
                    //            ch106this.DirectonTurn();
                    //        }
                    //    }
                    //}
                    //else
                    //{
                    //    Plugin.Log.LogInfo("Beam is SubBullet");
                    //}
                }

                if (this.AlwaysFaceCamera)
                {
                    this.transform.LookAt(this._mainCamera.transform.position, -Vector3.up);
                }
                yield return CoroutineDefine._waitForEndOfFrame;
            }
            this.BackToPool();
            yield return null;
            yield break;
        }

        public override void Update_Effect()
        {
            UpdateEffect();
        }

        public void UpdateEffect()
        {
            this.bIsEnd = false;
            if (!bInit)
            {
                bInit = true;
                float num = this._hitCollider.Cast<BoxCollider2D>().size.x - defLength;
                this.fxEndpoint.localPosition = this.fxEndpoint.localPosition + new Vector3(0f, 0f, num);
                fxLine01.SetPosition(0, new Vector3(fxLine01.GetPosition(0).x - num, 0f, fxLine01.GetPosition(0).z));
                fxLine01A.SetPosition(0, new Vector3(fxLine01A.GetPosition(0).x - num, 0f, fxLine01A.GetPosition(0).z));
                fxLine02.SetPosition(0, new Vector3(fxLine02.GetPosition(0).x - num, 0f, fxLine02.GetPosition(0).z));
                fxLine02A.SetPosition(0, new Vector3(fxLine02A.GetPosition(0).x - num, 0f, fxLine02A.GetPosition(0).z));
                fxLightning00.SetPosition(0, new Vector3(fxLightning00.GetPosition(0).x - num, 0f, fxLightning00.GetPosition(0).z));
                fxLightning00_Black.SetPosition(0, new Vector3(fxLightning00_Black.GetPosition(0).x - num, 0f, fxLightning00_Black.GetPosition(0).z));
                SetLightning(ref fxLightning, num);
                SetSS1(ref fxSs1, num);
                SetSS1(ref fxLL001, num);
                SetSS1(ref fxLL002, num);
                SetSS1(ref fxLL001A, num);
                SetSS1(ref fxLL002A, num);
            }
        }

        public void OnApplicationPause(bool pause)
        {
            BulletPause(pause);
        }

        private static void SetLightning(ref ParticleSystem ps, float difLength)
        {
            ParticleSystem.MainModule main = ps.main;
            ParticleSystem.MinMaxCurve startSizeY = main.startSizeY;
            startSizeY.constantMin = (main.startSizeY.constantMin + difLength) * 0.75f;
            startSizeY.constantMax = (main.startSizeY.constantMax + difLength) * 0.75f;
            main.startSizeY = startSizeY;
        }

        private static void SetSS1(ref ParticleSystem ps, float difLength)
        {
            ParticleSystem.MainModule main = ps.main;
            ParticleSystem.MinMaxCurve startSizeX = main.startSizeX;
            startSizeX.constantMin = main.startSizeX.constantMin + difLength * 0.5f;
            startSizeX.constantMax = main.startSizeX.constantMax + difLength * 0.5f;
            main.startSizeX = startSizeX;
        }

        public void DirectonTurn()
        {
            ActiveExtraCollider();
            _fStartAngle = this.transform.localEulerAngles.z;
            if (this.transform.localEulerAngles.z == 270f)
            {
                if (this.refPBMShoter.SOB != null && this.refPBMShoter.SOB.direction == 1)
                {
                    _fStartAngle = this.transform.localEulerAngles.z - 360f;
                }
            }
            else if (this.transform.localEulerAngles.z > 270f)
            {
                _fStartAngle = this.transform.localEulerAngles.z - 360f;
            }
            if (this.transform.localEulerAngles.z < 90f || this.transform.localEulerAngles.z > 270f)
            {
                nShootDirection = 1;
            }
            else
            {
                nShootDirection = -1;
            }
            bStartTurn = true;
            oldAngle = this.transform.localEulerAngles.z;
            LeanTween.value(this.transform.gameObject, _fStartAngle, 90f, (float)(this._duration - 50L) * 0.001f).setOnUpdate(new System.Action<float>((float val) =>
            {
                this.transform.localEulerAngles = new Vector3(0f, 0f, val);
            })).setEaseInQuart();
        }

        private void ActiveExtraCollider()
        {
            if (this.secortCollider == null)
                return;

            BoxCollider2D boxCollider2D = this._hitCollider.Cast<BoxCollider2D>();
            if (boxCollider2D == null)
                return;

            this.secortCollider.Active(this, boxCollider2D.size.x);
        }

        public void CreateSubBeam()
        {
            int n_LINK_SKILL = this.BulletData.n_LINK_SKILL;
            if (n_LINK_SKILL == 0)
                return;
            SKILL_TABLE skill_TABLE = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[n_LINK_SKILL].GetSkillTableByValue();
            if (this.refPBMShoter.SOB.Cast<OrangeCharacter>() != null)
            {
                (this.refPBMShoter.SOB.Cast<OrangeCharacter>()).tRefPassiveskill.ReCalcuSkill(ref skill_TABLE);
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
            weaponStatus.nHP = this.nHp;
            weaponStatus.nATK = this.nOriginalATK;
            weaponStatus.nCRI = this.nOriginalCRI;
            weaponStatus.nHIT = this.nHit - this.refPSShoter.GetAddStatus(8, this.nWeaponCheck);
            weaponStatus.nCriDmgPercent = this.nCriDmgPercent;
            weaponStatus.nReduceBlockPercent = this.nReduceBlockPercent;
            weaponStatus.nWeaponCheck = this.nWeaponCheck;
            weaponStatus.nWeaponType = this.nWeaponType;
            PerBuffManager.BuffStatus buffStatus = new PerBuffManager.BuffStatus();
            buffStatus.fAtkDmgPercent = this.fDmgFactor - 100f;
            buffStatus.fCriPercent = this.fCriFactor - 100f;
            buffStatus.fCriDmgPercent = this.fCriDmgFactor - 100f;
            buffStatus.fMissPercent = this.fMissFactor;
            buffStatus.refPBM = this.refPBMShoter;
            buffStatus.refPS = this.refPSShoter;
            ch106_BeamBullet.UpdateBulletData(skill_TABLE, this.Owner, 0, 0, 1);
            ch106_BeamBullet.SetBulletAtk(weaponStatus, buffStatus, null);
            ch106_BeamBullet.BulletLevel = this.BulletLevel;
            ch106_BeamBullet.isSubBullet = true;
            ch106_BeamBullet.transform.SetPositionAndRotation(this._transform.position, Quaternion.identity);
            ch106_BeamBullet.Active(this._transform.position, this.Direction, this.TargetMask, null);
        }

        public void GetBulletInfo()
        {
            Il2CppReferenceArray<Transform> componentsInChildren = this._transform.GetComponentsInChildren<Transform>(true).Cast<Il2CppReferenceArray<Transform>>();
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
        }

        public void BulletPause(bool pause)
        {
            bGamePause = pause;
            if (bGamePause)
            {
                this._clearTimer.TimerPause();
                this._durationTimer.TimerPause();
                return;
            }
            this._clearTimer.TimerResume();
            this._durationTimer.TimerResume();
        }

        public override void BackToPool()
        {
            if (!this.isSubBullet && _pOwner != null)
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
            this.CallBase<BeamBullet>("BackToPool");
        }

        //public BeamBullet _beamBullet;

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
        private bool bInit = false;
        private bool bGamePause = false;
        protected CH106_Controller _pOwner;
    }
}
