using System;
using System.Collections.Generic;
using CallbackDefs;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using StageLib;
using Tangerine.Utils;
using UnityEngine;

namespace MonHunCollabRestored.Character
{
    public class CH106_Controller : CharacterControlBase
    {
        public override void Start()
        {
            this.CallBase<CharacterControlBase>("Start");
            this.InitLinkSkill();
            this.InitExtraMeshData();
        }

        private void InitExtraMeshData()
        {
            Il2CppReferenceArray<Transform> componentsInChildren = this._refEntity._transform.GetComponentsInChildren<Transform>(true).Cast<Il2CppReferenceArray<Transform>>();
            this._refEntity.ExtraTransforms = new Transform[3];
            this._refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(componentsInChildren, "L WeaponPoint", true);
            this._refEntity.ExtraTransforms[1] = OrangeBattleUtility.FindChildRecursive(componentsInChildren, "R WeaponPoint", true);
            this._refEntity.ExtraTransforms[2] = OrangeBattleUtility.FindChildRecursive(componentsInChildren, "Bip R UpperArm", true);
            Transform[] array = OrangeBattleUtility.FindMultiChildRecursive(componentsInChildren, "fxdemo_valstrax_burst", true);
            this._liFxWindBursts.Clear();
            Transform[] array2 = array;
            for (int i = 0; i < array2.Length; i++)
            {
                ParticleSystem component = array2[i].GetComponent<ParticleSystem>();
                if (component)
                {
                    this._liFxWindBursts.Add(component);
                }
            }
            this.fxuse_dragonbreath_main = OrangeBattleUtility.FindChildRecursive(componentsInChildren, "fxuse_dragonbreath_main", true).GetComponent<ParticleSystem>();
            Transform[] array3 = OrangeBattleUtility.FindMultiChildRecursive(componentsInChildren, "fxuse_valstraxlaser_001", true);
            this._liFxRedBalls.Clear();
            array2 = array3;
            for (int i = 0; i < array2.Length; i++)
            {
                ParticleSystem component2 = array2[i].GetComponent<ParticleSystem>();
                if (component2)
                {
                    this._liFxRedBalls.Add(component2);
                }
            }
            this.EnableRedBalls(false);
            Transform[] array4 = OrangeBattleUtility.FindMultiChildRecursive(componentsInChildren, "fxuse_dragonbreath_000_(work)", true);
            this._liFxWindFires.Clear();
            array2 = array4;
            for (int i = 0; i < array2.Length; i++)
            {
                ParticleSystem component3 = array2[i].GetComponent<ParticleSystem>();
                if (component3)
                {
                    this._liFxWindFires.Add(component3);
                }
            }
            
            MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBase<BeamBullet>("prefab/bullet/p_valstraxlaser_000_01", "p_valstraxlaser_000_01", 4, null);
            MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_valstraxlaser_000", 4, null);
        }

        private void InitLinkSkill()
        {
            this._refPlayer = this._refEntity.Cast<OrangeConsoleCharacter>();
        }

        public override void OverrideDelegateEvent()
        {
            this.CallBase<CharacterControlBase>("OverrideDelegateEvent");
            this._refEntity.AnimationEndCharacterDependEvt = new Action<OrangeCharacter.MainStatus, OrangeCharacter.SubStatus>(this.AnimationEndCharacterDepend);
            this._refEntity.SetStatusCharacterDependEvt = new Action<OrangeCharacter.MainStatus, OrangeCharacter.SubStatus>(this.SetStatusCharacterDepend);
            this._refEntity.CheckSkillLockDirectionEvt = (Callback)new System.Action(this.CheckSkillLockDirection);
            this._refEntity.StageTeleportOutCharacterDependEvt = (Callback)new System.Action(this.StageTeleportOutCharacterDepend);
            this._refEntity.StageTeleportInCharacterDependEvt = (Callback)new System.Action(this.StageTeleportInCharacterDepend);
        }

        public override void SetRushBullet(RushCollideBullet rushCollideBullet)
        {
            this._pRushCollideBullet = rushCollideBullet;
            if (this._refEntity.UsingVehicle)
            {
                this._pRushCollideBullet.BackToPool();
                this._pRushCollideBullet = null;
            }
        }

        public override void SyncSkillDirection(Vector3 dir, IAimTarget target)
        {
            if (this._refEntity.UsingVehicle)
            {
                if (this._pRushCollideBullet)
                {
                    this._pRushCollideBullet.BackToPool();
                    this._pRushCollideBullet = null;
                }
                this._pRushTarget = null;
                return;
            }
            this._bSyncRushSkillCompleted = true;
            this._vSyncDirection = dir;
            this._pRushTarget = target;
            if (this._refEntity.CurMainStatus == OrangeCharacter.MainStatus.SKILL && this._refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL1)
            {
                this._vSkillVelocity = this._vSyncDirection.normalized * (float)(OrangeCharacter.DashSpeed * 4);
                this.UpdateSkill1Direction(this._vSkillVelocity.x);
            }
        }

        public override void ClearSkill()
        {
            if (this._refEntity.CurrentActiveSkill == 0)
            {
                this.CancelSkill0();
            }
            else if (this._refEntity.CurrentActiveSkill == 1)
            {
                this.CancelSkill1();
            }
            this._refEntity.CurrentActiveSkill = -1;
        }

        public override void CheckSkill()
        {
            if (this._refEntity.IsAnimateIDChanged())
            {
                return;
            }
            this.UpdateVirtualButtonAnalog();
            this.UpdateSkill();
        }

        private void UpdateVirtualButtonAnalog()
        {
            if (!this._refEntity.IsLocalPlayer)
            {
                return;
            }
            if (this._refPlayer != null)
            {
                this._refPlayer.SetVirtualButtonAnalog(VirtualButtonId.SKILL1, true);
            }
        }

        public override void PlayerPressSkillCharacterCall(int id)
        {
        }

        public override void PlayerReleaseSkillCharacterCall(int id)
        {
            if (this._refEntity.CurrentActiveSkill != -1)
            {
                return;
            }
            if (id != 0)
            {
                if (id != 1)
                {
                    return;
                }
                if (!this._refEntity.CheckUseSkillKeyTrigger(id, true))
                {
                    return;
                }
                this.UseSkill1(id);
                return;
            }
            else
            {
                if (!this._refEntity.CheckUseSkillKeyTrigger(id, true))
                {
                    return;
                }
                this.UseSkill0(id);
                return;
            }
        }

        public void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
        {
            switch (mainStatus)
            {
                case OrangeCharacter.MainStatus.TELEPORT_IN:
                        return;
                case OrangeCharacter.MainStatus.TELEPORT_OUT:
                    if (subStatus == OrangeCharacter.SubStatus.TELEPORT_POSE)
                    {
                        this.EnableWindBrustFx(true, true);
                        this.ToggleWeapon(-3);
                        return;
                    }
                    if (subStatus == OrangeCharacter.SubStatus.WIN_POSE)
                    {
                        this.ToggleWeapon(-2);
                        return;
                    }
                    break;
                case OrangeCharacter.MainStatus.SLASH:
                    break;
                case OrangeCharacter.MainStatus.SKILL:
                    switch (subStatus)
                    {
                        case OrangeCharacter.SubStatus.SKILL0:
                            this._refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
                            this._refEntity.Animator._animator.speed = 2f;
                            return;
                        case OrangeCharacter.SubStatus.SKILL0_1:
                            this._refEntity.IgnoreGravity = true;
                            this._refEntity.SetAnimateId((HumanBase.AnimateId)68U);
                            this._refEntity.Animator._animator.speed = 2f;
                            return;
                        case OrangeCharacter.SubStatus.SKILL0_2:
                            this._refEntity.SetAnimateId((HumanBase.AnimateId)71U);
                            this._refEntity.Animator._animator.speed = 2f;
                            return;
                        case OrangeCharacter.SubStatus.SKILL0_3:
                            this._refEntity.SetAnimateId((HumanBase.AnimateId)66U);
                            this._refEntity.Animator._animator.speed = 1f;
                            return;
                        case OrangeCharacter.SubStatus.SKILL0_4:
                            this._refEntity.IgnoreGravity = true;
                            this._refEntity.SetAnimateId((HumanBase.AnimateId)69U);
                            this._refEntity.Animator._animator.speed = 1f;
                            return;
                        case OrangeCharacter.SubStatus.SKILL0_5:
                            this._refEntity.SetAnimateId((HumanBase.AnimateId)72U);
                            this._refEntity.Animator._animator.speed = 1f;
                            return;
                        case OrangeCharacter.SubStatus.SKILL0_6:
                            this._refEntity.SetAnimateId((HumanBase.AnimateId)67U);
                            this._refEntity.Animator._animator.speed = 2.25f;
                            return;
                        case OrangeCharacter.SubStatus.SKILL0_7:
                            this._refEntity.IgnoreGravity = true;
                            this._refEntity.SetAnimateId((HumanBase.AnimateId)70U);
                            this._refEntity.Animator._animator.speed = 2.25f;
                            return;
                        case OrangeCharacter.SubStatus.SKILL0_8:
                            this._refEntity.SetAnimateId((HumanBase.AnimateId)73U);
                            this._refEntity.Animator._animator.speed = 2.25f;
                            return;
                        default:
                            switch (subStatus)
                            {
                                case OrangeCharacter.SubStatus.SKILL1:
                                    this._refEntity.IgnoreGravity = true;
                                    this._refEntity.SetAnimateId((HumanBase.AnimateId)74U);
                                    this._refEntity.Animator._animator.speed = 2f;
                                    return;
                                case OrangeCharacter.SubStatus.SKILL1_1:
                                    this._refEntity.IgnoreGravity = true;
                                    this._refEntity.SetAnimateId((HumanBase.AnimateId)75U);
                                    this._refEntity.Animator._animator.speed = 1f;
                                    this._refEntity.SetSpeed((int)this._vSkillVelocity.x, (int)this._vSkillVelocity.y);
                                    this.UpdateSkill1Rotation(this._vSkillVelocity);
                                    return;
                                case OrangeCharacter.SubStatus.SKILL1_2:
                                    this._refEntity.IgnoreGravity = false;
                                    this.UpdateSkill1Rotation(Vector3.right * (float)this._refEntity.direction);
                                    if (this._refEntity.Controller.Collisions.below || this._refEntity.Controller.Collisions.JSB_below)
                                    {
                                        this._refEntity.SetSpeed(0, 0);
                                        this._refEntity.SetAnimateId((HumanBase.AnimateId)76U);
                                        this._refEntity.Animator._animator.speed = 1f;
                                        return;
                                    }
                                    this._refEntity.SetAnimateId((HumanBase.AnimateId)77U);
                                    this._refEntity.Animator._animator.speed = 1f;
                                    break;
                                default:
                                    return;
                            }
                            break;
                    }
                    break;
                default:
                    return;
            }
        }

        public void AnimationEndCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
        {
            if (mainStatus != OrangeCharacter.MainStatus.TELEPORT_IN)
            {
                if (mainStatus != OrangeCharacter.MainStatus.SKILL)
                {
                    return;
                }
                if (subStatus - OrangeCharacter.SubStatus.SKILL0_6 <= 1)
                {
                    //Plugin.Log.LogInfo($"0_6 <= 1 {(subStatus - OrangeCharacter.SubStatus.SKILL0_6)}");
                    //this.SkillEndChnageToIdle(false);
                    return;
                }
                if (subStatus == OrangeCharacter.SubStatus.SKILL0_8)
                {
                    this.SkillEndChnageToIdle(true);
                    return;
                }
                switch (subStatus)
                {
                    case OrangeCharacter.SubStatus.SKILL1:
                        if (this._refEntity.IsLocalPlayer)
                        {
                            this.CreateSkillBullet(this._refEntity.PlayerSkills[1]);
                            this._vSkillVelocity = this._vSkillVelocity.normalized * ((float)OrangeCharacter.DashSpeed * 4f);
                            this.UpdateSkill1Direction(this._vSkillVelocity.x);
                            this._refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_1);
                            return;
                        }
                        break;
                    case OrangeCharacter.SubStatus.SKILL1_1:
                        break;
                    case OrangeCharacter.SubStatus.SKILL1_2:
                        this._refEntity.SetSpeed(0, 0);
                        this.SkillEndChnageToIdle(false);
                        break;
                    default:
                        return;
                }
            }
            else if (subStatus == OrangeCharacter.SubStatus.TELEPORT_POSE)
            {
                this.EnableWindBrustFx(false, false);
                this.ToggleWeapon(0);
                return;
            }
        }

        public override void CreateSkillBullet(WeaponStruct wsSkill)
        {
            this._refEntity.FreshBullet = true;
            OrangeCharacter.MainStatus curMainStatus = this._refEntity.CurMainStatus;
            if (curMainStatus == OrangeCharacter.MainStatus.SKILL)
            {
                //Plugin.Log.LogInfo("Creating Skill Bullet");
                OrangeCharacter.SubStatus curSubStatus = this._refEntity.CurSubStatus;
                if (curSubStatus - OrangeCharacter.SubStatus.SKILL0 <= 2)
                {
                    //Plugin.Log.LogInfo("[Skill 0] - Push Bullet Detail Skill");
                    this._refEntity.PushBulletDetail(wsSkill.BulletData, wsSkill.weaponStatus, this._refEntity.ExtraTransforms[1], wsSkill.SkillLV, new Il2CppSystem.Nullable_Unboxed<UnityEngine.Vector3>(this._vSkill0ShootDirection), true);
                    //Plugin.Log.LogInfo("[Skill 0] - Check Passive Detail Skill");
                    this._refEntity.CheckUsePassiveSkill(0, wsSkill.weaponStatus, wsSkill.ShootTransform[0]);
                    //Plugin.Log.LogInfo("[Skill 0] - Update CD");
                    OrangeBattleUtility.UpdateSkillCD(wsSkill);
                    return;
                }
                if (curSubStatus == OrangeCharacter.SubStatus.SKILL1)
                {
                    this._refEntity.PushBulletDetail(wsSkill.BulletData, wsSkill.weaponStatus, this._refEntity.AimTransform, wsSkill.SkillLV, new Il2CppSystem.Nullable_Unboxed<UnityEngine.Vector3>(this._vSkillVelocity), true);
                    this._refEntity.CheckUsePassiveSkill(1, wsSkill.BulletData, wsSkill.weaponStatus, wsSkill.ShootTransform[0]);
                    OrangeBattleUtility.UpdateSkillCD(wsSkill);
                    return;
                }
                else
                {
                    //Plugin.Log.LogInfo($"[Skill 0] {curSubStatus.ToString()}");
                }
                
                
            }
        }

        public bool BeamStartTurn()
        {
            if (this._refEntity.CurMainStatus == OrangeCharacter.MainStatus.SKILL)
            {
                OrangeCharacter.SubStatus curSubStatus = this._refEntity.CurSubStatus;
                if (curSubStatus - OrangeCharacter.SubStatus.SKILL0_3 <= 2)
                {
                    this.EnableRedBalls(false);
                    this._refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, this._refEntity.CurSubStatus + 3);
                    return true;
                }
                
            }
            return false;
        }

        public float GetBeamAngle()
        {
            Vector3 v = this._refEntity.ExtraTransforms[1].position - this._refEntity.ExtraTransforms[2].position;
            if (this._refEntity._characterDirection == CharacterDirection.LEFT)
            {
                return Vector3.Angle(v.xy(), Vector3.left);
            }
            return Vector3.Angle(v.xy(), Vector3.right);
        }

        public void CheckSkillLockDirection()
        {
            OrangeCharacter.SubStatus curSubStatus = this._refEntity.CurSubStatus;
            if (curSubStatus - OrangeCharacter.SubStatus.SKILL0 > 8)
            {
                //this._refEntity._characterDirection = this._refEntity._characterDirection * CharacterDirection.LEFT;
                this._refEntity._characterDirection = (CharacterDirection)(-(int)this._refEntity._characterDirection);
                return;
            }
            if (this._nLockSkill1Direction != 0)
            {
                this._refEntity._characterDirection = (CharacterDirection)this._nLockSkill1Direction;
                return;
            }
            //this._refEntity._characterDirection = this._refEntity._characterDirection * CharacterDirection.LEFT;
            this._refEntity._characterDirection = (CharacterDirection)(-(int)this._refEntity._characterDirection);
        }

        public void StageTeleportOutCharacterDepend()
        {
            this.EnableWindBrustFx(false, false);
            this.EnableWindFire(false);
        }

        public void StageTeleportInCharacterDepend()
        {
            this.EnableWindFire(true);
        }

        /*private void UpdateSkill()
        {
            OrangeCharacter.MainStatus curMainStatus = this._refEntity.CurMainStatus;
            if (curMainStatus == OrangeCharacter.MainStatus.SKILL)
            {
                OrangeCharacter.SubStatus curSubStatus = this._refEntity.CurSubStatus;
                Plugin.Log.LogInfo($"[SKILL 0] - {curSubStatus.ToString()}");
                switch (curSubStatus)
                {
                    case OrangeCharacter.SubStatus.SKILL0:
                    case OrangeCharacter.SubStatus.SKILL0_1:
                    case OrangeCharacter.SubStatus.SKILL0_2:
                        //if (this.bPlayRedBalls && this._refEntity.CurrentFrame > 0.7f)
                        //{
                        //    this.bPlayRedBalls = false;
                        //    this.EnableRedBalls(true);
                        //    break;
                        //}

                        if (this.bInSkill)
                        {
                            Plugin.Log.LogInfo("[SKILL 0] - Shoot Bullet");
                            this.bInSkill = false;
                            this.CreateSkillBullet(this._refEntity.PlayerSkills[0]);
                            //this._refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, this._refEntity.CurSubStatus + 3);
                            return;
                        }
                        break;
                    case OrangeCharacter.SubStatus.SKILL0_3:
                    case OrangeCharacter.SubStatus.SKILL0_4:
                    case OrangeCharacter.SubStatus.SKILL0_5:
                        break;
                    case OrangeCharacter.SubStatus.SKILL0_6:
                    case OrangeCharacter.SubStatus.SKILL0_7:
                    case OrangeCharacter.SubStatus.SKILL0_8:
                        if (this._refEntity.CurrentFrame > 1f)
                        {
                            bool isCrouch = this._refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL0_8;
                            this.SkillEndChnageToIdle(isCrouch);
                            return;
                        }
                        if (this.CheckCancelAnimate(0) && this._refEntity.CurrentFrame > 0.65f)
                        {
                            this.SkipSkill0Animation();
                            return;
                        }
                        break;
                    default:
                        switch (curSubStatus)
                        {
                            case OrangeCharacter.SubStatus.SKILL1:
                                if (this._refEntity.CurrentFrame > 1f)
                                {
                                    bool flag = false;
                                    if (this._refEntity.IsLocalPlayer)
                                    {
                                        flag = true;
                                    }
                                    else if (this._bSyncRushSkillCompleted)
                                    {
                                        this._bSyncRushSkillCompleted = false;
                                        flag = true;
                                    }
                                    if (flag)
                                    {
                                        this.CreateSkillBullet(this._refEntity.PlayerSkills[1]);
                                        this._vSkillVelocity = this._vSkillVelocity.normalized * ((float)OrangeCharacter.DashSpeed * 4f);
                                        this.UpdateSkill1Direction(this._vSkillVelocity.x);
                                        this._refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_1);
                                        return;
                                    }
                                }
                                break;
                            case OrangeCharacter.SubStatus.SKILL1_1:
                                {
                                    bool flag2 = false;
                                    if (Vector2.Distance(this._vSkillStartPosition, this._refEntity.AimPosition) > this._refEntity.PlayerSkills[1].BulletData.f_DISTANCE || this._refEntity.PlayerSkills[1].LastUseTimer.GetMillisecond() > 500L)
                                    {
                                        flag2 = true;
                                    }
                                    else if (this._pRushTarget != null)
                                    {
                                        float num = Vector2.Distance(this._refEntity.AimPosition, this._pRushTarget.AimPosition);
                                        float num2 = (float)this._refEntity.Velocity.magnitude * 0.001f * GameLogicUpdateManager.m_fFrameLen;
                                        if (num < num2 * 1.5f)
                                        {
                                            flag2 = true;
                                        }
                                    }
                                    if (flag2)
                                    {
                                        if (!this._refEntity.Controller.Collisions.below)
                                        {
                                            this._refEntity.SetSpeed(this._refEntity.direction * OrangeCharacter.DashSpeed, 0);
                                        }
                                        else
                                        {
                                            this._refEntity.SetSpeed(0, 0);
                                        }
                                        this.EnableWindBrustFx(false, false);
                                        this._refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_2);
                                        return;
                                    }
                                    break;
                                }
                            case OrangeCharacter.SubStatus.SKILL1_2:
                                if (this._refEntity.CurrentFrame > 1f)
                                {
                                    if (this._pRushCollideBullet)
                                    {
                                        this._pRushCollideBullet.BackToPool();
                                        this._pRushCollideBullet = null;
                                    }
                                    this.SkillEndChnageToIdle(false);
                                    return;
                                }
                                if (this._refEntity.CurrentFrame > 0.45f && (this.CheckCancelAnimate(1) || this._refEntity.Controller.Collisions.below || this._refEntity.Controller.Collisions.JSB_below))
                                {
                                    if (this._pRushCollideBullet)
                                    {
                                        this._pRushCollideBullet.BackToPool();
                                        this._pRushCollideBullet = null;
                                    }
                                    this.SkipSkill1Animation();
                                    return;
                                }
                                break;
                            default:
                                return;
                        }
                        break;
                }
            }
            else if (this._pRushCollideBullet)
            {
                this._pRushCollideBullet.BackToPool();
                this._pRushCollideBullet = null;
            }
        }*/

        private void UpdateSkill()
        {
            OrangeCharacter.MainStatus curMainStatus = this._refEntity.CurMainStatus;
            if (curMainStatus == OrangeCharacter.MainStatus.SKILL)
            {
                OrangeCharacter.SubStatus curSubStatus = this._refEntity.CurSubStatus;
                switch (curSubStatus)
                {
                    case OrangeCharacter.SubStatus.SKILL0:
                    case OrangeCharacter.SubStatus.SKILL0_1:
                    case OrangeCharacter.SubStatus.SKILL0_2:
                        if (this._refEntity.CurrentFrame > 1f && this.bInSkill)
                        {
                            this.bInSkill = false;
                            this.CreateSkillBullet(this._refEntity.PlayerSkills[0]);
                            this._refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, this._refEntity.CurSubStatus + 3);
                            return;
                        }
                        if (this.bPlayRedBalls && this._refEntity.CurrentFrame > 0.7f)
                        {
                            this.bPlayRedBalls = false;
                            this.EnableRedBalls(true);
                            return;
                        }
                        break;
                    case OrangeCharacter.SubStatus.SKILL0_3:
                    case OrangeCharacter.SubStatus.SKILL0_4:
                    case OrangeCharacter.SubStatus.SKILL0_5:
                        break;
                    case OrangeCharacter.SubStatus.SKILL0_6:
                    case OrangeCharacter.SubStatus.SKILL0_7:
                    case OrangeCharacter.SubStatus.SKILL0_8:
                        if (this._refEntity.CurrentFrame > 1f)
                        {
                            bool flag = this._refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL0_8;
                            this.SkillEndChnageToIdle(flag);
                            return;
                        }
                        if (this.CheckCancelAnimate(0) && this._refEntity.CurrentFrame > 0.65f)
                        {
                            this.SkipSkill0Animation();
                            return;
                        }
                        break;
                    default:
                        switch (curSubStatus)
                        {
                            case OrangeCharacter.SubStatus.SKILL1:
                                if (this._refEntity.CurrentFrame > 1f)
                                {
                                    bool flag2 = false;
                                    if (this._refEntity.IsLocalPlayer)
                                    {
                                        flag2 = true;
                                    }
                                    else if (this._bSyncRushSkillCompleted)
                                    {
                                        this._bSyncRushSkillCompleted = false;
                                        flag2 = true;
                                    }
                                    if (flag2)
                                    {
                                        this.CreateSkillBullet(this._refEntity.PlayerSkills[1]);
                                        this._vSkillVelocity = this._vSkillVelocity.normalized * ((float)OrangeCharacter.DashSpeed * 4f);
                                        this.UpdateSkill1Direction(this._vSkillVelocity.x);
                                        this._refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_1);
                                        return;
                                    }
                                }
                                break;
                            case OrangeCharacter.SubStatus.SKILL1_1:
                                {
                                    bool flag3 = false;
                                    if (Vector2.Distance(this._vSkillStartPosition, this._refEntity.AimPosition) > this._refEntity.PlayerSkills[1].BulletData.f_DISTANCE || this._refEntity.PlayerSkills[1].LastUseTimer.GetMillisecond() > 500L)
                                    {
                                        flag3 = true;
                                    }
                                    else if (this._pRushTarget != null)
                                    {
                                        float num = Vector2.Distance(this._refEntity.AimPosition, this._pRushTarget.AimPosition);
                                        float num2 = (float)this._refEntity.Velocity.magnitude * 0.001f * GameLogicUpdateManager.m_fFrameLen;
                                        if (num < num2 * 1.5f)
                                        {
                                            flag3 = true;
                                        }
                                    }
                                    if (flag3)
                                    {
                                        if (!this._refEntity.Controller.Collisions.below)
                                        {
                                            this._refEntity.SetSpeed(this._refEntity.direction * OrangeCharacter.DashSpeed, 0);
                                        }
                                        else
                                        {
                                            this._refEntity.SetSpeed(0, 0);
                                        }
                                        this.EnableWindBrustFx(false, false);
                                        this._refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_2);
                                        return;
                                    }
                                    break;
                                }
                            case OrangeCharacter.SubStatus.SKILL1_2:
                                if (this._refEntity.CurrentFrame > 1f)
                                {
                                    if (this._pRushCollideBullet)
                                    {
                                        this._pRushCollideBullet.BackToPool();
                                        this._pRushCollideBullet = null;
                                    }
                                    this.SkillEndChnageToIdle(false);
                                    return;
                                }
                                if (this._refEntity.CurrentFrame > 0.45f && (this.CheckCancelAnimate(1) || this._refEntity.Controller.Collisions.below || this._refEntity.Controller.Collisions.JSB_below))
                                {
                                    if (this._pRushCollideBullet)
                                    {
                                        this._pRushCollideBullet.BackToPool();
                                        this._pRushCollideBullet = null;
                                    }
                                    this.SkipSkill1Animation();
                                    return;
                                }
                                break;
                            default:
                                return;
                        }
                        break;
                }
            }
            else if (this._pRushCollideBullet)
            {
                this._pRushCollideBullet.BackToPool();
                this._pRushCollideBullet = null;
            }
        }


        //Aoki's Note: Use CharacterControlHelper instead, it was used in all 3rd-year CharacterController
        //private void TurnToAimTarget()
        //{
        //    Il2CppSystem.Nullable_Unboxed<UnityEngine.Vector3> vector = this._refEntity.CalibrateAimDirection(this._refEntity.AimPosition, null);
        //    if (vector.has_value)
        //    {
        //        int num = Math.Sign(vector.Value.x);
        //        if (this._refEntity.direction != num && Mathf.Abs(vector.Value.x) > 0.05f)
        //        {
        //            //this._refEntity._characterDirection = this._refEntity._characterDirection * CharacterDirection.LEFT;
        //            this._refEntity._characterDirection = (CharacterDirection)(-(int)this._refEntity._characterDirection);
        //            this._refEntity.ShootDirection = vector.Value;
        //        }
        //    }
        //}

        private void TurnToShootDirection(Vector3 dir)
        {
            int num = Math.Sign(dir.x);
            if (this._refEntity.direction != num && Mathf.Abs(dir.x) > 0.05f)
            {
                //this._refEntity._characterDirection = this._refEntity._characterDirection * CharacterDirection.LEFT;
                this._refEntity._characterDirection = (CharacterDirection)(-(int)this._refEntity._characterDirection);
                this._refEntity.ShootDirection = dir;
            }
        }

        private void UseSkill0(int skillId)
        {
            base.PlayVoiceSE("v_zm_skill01");
            base.PlaySkillSE("zm_kakuyou");
            this.bInSkill = true;
            this.bPlayRedBalls = true;
            this._refEntity.CurrentActiveSkill = skillId;
            this._refEntity.SkillEnd = false;
            this._refEntity.PlayerStopDashing();
            this._refEntity.SetSpeed(0, 0);
            this._refEntity.IsShoot = 1;
            this.ToggleWeapon(1);
            if (this._refEntity.UseAutoAim)
            {
                ManagedSingleton<CharacterControlHelper>.Instance.TurnToAimTarget(this._refEntity);
            }
            this._vSkill0ShootDirection = this._refEntity.ShootDirection;
            this._nLockSkill1Direction = (int)this._refEntity._characterDirection;
            this.UpdateSkill1Direction(this._vSkill0ShootDirection.x);
            this._fxUseSkill0 = Tangerine.Utils.Il2CppHelpers.FxManagerPlayReturn<FxBase>("fxuse_valstraxlaser_000", this._refEntity.AimTransform.position + Vector3.right * 0.2f * (float)this._refEntity.direction, Quaternion.identity);

            if (this._refEntity.CurMainStatus == OrangeCharacter.MainStatus.CROUCH)
            {
                this._refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_2);
                return;
            }
            if (this._refEntity.Controller.Collisions.below)
            {
                this._refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0);
                return;
            }
            this._refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_1);
        }

        private void CancelSkill0()
        {
            this._refEntity.SkillEnd = true;
            this.EnableRedBalls(false);
            if (this._fxUseSkill0)
            {
                this._fxUseSkill0.BackToPool();
                this._fxUseSkill0 = null;
            }
            this.SkipSkill0Animation();
        }

        private void SkipSkill0Animation()
        {
            if (this._refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL0_2 || this._refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL0_5 || this._refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL0_8)
            {
                this.SkillEndChnageToIdle(true);
                return;
            }
            this.SkillEndChnageToIdle(false);
        }

        private void UseSkill1(int skillId)
        {
            base.PlayVoiceSE("v_zm_skill02");
            base.PlaySkillSE("zm_dash");
            this.bInSkill = true;
            this._refEntity.CurrentActiveSkill = skillId;
            this._refEntity.SkillEnd = false;
            this._refEntity.PlayerStopDashing();
            this._refEntity.SetSpeed(0, 0);
            this._refEntity.IsShoot = 1;
            this.ToggleWeapon(2);
            this._bSyncRushSkillCompleted = false;
            if (this._refEntity.IsLocalPlayer)
            {
                if (this._refEntity.UseAutoAim && this._refEntity.PlayerAutoAimSystem.AutoAimTarget != null)
                {
                    ManagedSingleton<CharacterControlHelper>.Instance.TurnToAimTarget(this._refEntity);
                    this._pRushTarget = this._refEntity.PlayerAutoAimSystem.AutoAimTarget;
                    this._vSkillVelocity = (this._pRushTarget.AimPosition - this._refEntity.AimPosition).normalized * ((float)OrangeCharacter.DashSpeed * 4f);
                    this.UpdateSkill1Direction(this._vSkillVelocity.x);
                }
                else
                {
                    this._pRushTarget = null;
                    this._vSkillVelocity = this._refEntity.ShootDirection.normalized * ((float)OrangeCharacter.DashSpeed * 4f);
                    this.UpdateSkill1Direction(this._vSkillVelocity.x);
                }
            }
            this.EnableWindBrustFx(true, false);
            this._vSkillStartPosition = this._refEntity.AimPosition;
            this._refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1);
        }

        private void UpdateSkill1Direction(float dirX)
        {
            if (Mathf.Abs(dirX) > 0.05f)
            {
                int num = Math.Sign(dirX);
                if (num != this._refEntity.direction)
                {
                    this._refEntity.direction = num;
                }
            }
        }

        protected void UpdateSkill1Rotation(Vector3 dir)
        {
            Vector3 to = (this._refEntity._characterDirection == CharacterDirection.LEFT) ? Vector3.left : Vector3.right;
            float num = Vector3.Angle(dir.xy(), to);
            if (dir.y < 0f)
            {
                num = -num;
            }
            this._refEntity.ModelTransform.localEulerAngles = new Vector3(this._refEntity.ModelTransform.localEulerAngles.x, this._refEntity.ModelTransform.localEulerAngles.y, num);
        }

        private void CancelSkill1()
        {
            this._refEntity.SkillEnd = true;
            if (this._pRushCollideBullet)
            {
                this._pRushCollideBullet.BackToPool();
                this._pRushCollideBullet = null;
            }
            this.EnableWindBrustFx(false, false);
            this.SkipSkill1Animation();
        }

        private void SkipSkill1Animation()
        {
            this._refEntity.SetSpeed(0, 0);
            this.EnableWindBrustFx(false, false);
            this.UpdateSkill1Rotation(Vector3.right * (float)this._refEntity.direction);
            this.SkillEndChnageToIdle(false);
        }

        private bool CheckCancelAnimate(int skilliD)
        {
            if (skilliD == 0)
            {
                if (this._refEntity.CurMainStatus == OrangeCharacter.MainStatus.SKILL && (this._refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL0_2 || this._refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL0_5 || this._refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL0_8))
                {
                    if (ManagedSingleton<InputStorage>.Instance.IsAnyHeld(this._refEntity.UserID) && !ManagedSingleton<InputStorage>.Instance.IsHeld(this._refEntity.UserID, ButtonId.SKILL0) && !ManagedSingleton<InputStorage>.Instance.IsHeld(this._refEntity.UserID, ButtonId.DOWN))
                    {
                        return true;
                    }
                }
                else if (ManagedSingleton<InputStorage>.Instance.IsAnyHeld(this._refEntity.UserID) && !ManagedSingleton<InputStorage>.Instance.IsHeld(this._refEntity.UserID, ButtonId.SKILL0))
                {
                    return true;
                }
            }
            else if (skilliD == 1 && ManagedSingleton<InputStorage>.Instance.IsAnyHeld(this._refEntity.UserID) && !ManagedSingleton<InputStorage>.Instance.IsHeld(this._refEntity.UserID, ButtonId.SKILL1))
            {
                return true;
            }
            return false;
        }

        private void SkillEndChnageToIdle(bool isCrouch = false)
        {
            this._refEntity.SkillEnd = true;
            this._refEntity.Dashing = false;
            this._refEntity.IgnoreGravity = false;
            this._refEntity.GravityMultiplier = new VInt(1f);
            this._refEntity.Animator._animator.speed = 1f;
            this.bInSkill = false;
            this._pRushTarget = null;
            if (this._pRushCollideBullet)
            {
                this._pRushCollideBullet.BackToPool();
                this._pRushCollideBullet = null;
            }
            this.ToggleWeapon(0);
            if (isCrouch)
            {
                if (ManagedSingleton<InputStorage>.Instance.IsHeld(this._refEntity.UserID, ButtonId.DOWN))
                {
                    this._refEntity.SetStatus(OrangeCharacter.MainStatus.CROUCH, OrangeCharacter.SubStatus.WIN_POSE);
                    return;
                }
                this._refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.CROUCH_UP);
                return;
            }
            else
            {
                if (this._refEntity.Controller.Collisions.below)
                {
                    this._refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
                    return;
                }
                this._refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
                return;
            }
        }

        private void ToggleWeapon(int style)
        {
            switch (style)
            {
                case -3:
                    if (this._refEntity.CheckCurrentWeaponIndex())
                    {
                        this._refEntity.DisableWeaponMesh(this._refEntity.GetCurrentWeaponObj(), 0.01f);
                        return;
                    }
                    return;
                case -2:
                    if (this._refEntity.CheckCurrentWeaponIndex())
                    {
                        this._refEntity.DisableWeaponMesh(this._refEntity.GetCurrentWeaponObj(), 0.01f);
                        return;
                    }
                    return;
                case -1:
                    if (this._refEntity.CheckCurrentWeaponIndex())
                    {
                        this._refEntity.DisableWeaponMesh(this._refEntity.GetCurrentWeaponObj(), 0.01f);
                        return;
                    }
                    return;
                case 1:
                    if (this._refEntity.CheckCurrentWeaponIndex())
                    {
                        this._refEntity.DisableWeaponMesh(this._refEntity.GetCurrentWeaponObj(), 0.01f);
                        return;
                    }
                    return;
                case 2:
                    if (this._refEntity.CheckCurrentWeaponIndex())
                    {
                        this._refEntity.DisableWeaponMesh(this._refEntity.GetCurrentWeaponObj(), 0.01f);
                        return;
                    }
                    return;
            }
            if (this._refEntity.CheckCurrentWeaponIndex())
            {
                this._refEntity.EnableCurrentWeapon();
            }
        }

        protected void EnableWindBrustFx(bool enable, bool isTELEPORT_POSE = false)
        {
            foreach (ParticleSystem particleSystem in this._liFxWindBursts)
            {
                if (enable)
                {
                    particleSystem.Play(true);
                    if (!isTELEPORT_POSE)
                    {
                        this.fxuse_dragonbreath_main.Play(true);
                    }
                }
                else
                {
                    particleSystem.Stop(true);
                    if (!isTELEPORT_POSE)
                    {
                        this.fxuse_dragonbreath_main.Stop(true);
                    }
                }
            }
        }

        protected void EnableWindFire(bool enable)
        {
            foreach (ParticleSystem particleSystem in this._liFxWindFires)
            {
                if (enable)
                {
                    particleSystem.Play(true);
                }
                else
                {
                    particleSystem.Stop(true);
                }
            }
        }

        protected void EnableRedBalls(bool enable)
        {
            foreach (ParticleSystem particleSystem in this._liFxRedBalls)
            {
                if (enable)
                {
                    particleSystem.Play(true);
                }
                else
                {
                    particleSystem.Stop(true);
                }
            }
        }

        private void OnApplicationPause(bool pause)
        {
            if (StageUpdate.gbIsNetGame && this._refPlayer != null)
            {
                this.ClearSkill();
            }
        }

        public override Il2CppStringArray GetCharacterDependAnimations()
        {
            return new Il2CppStringArray(new string[] 
            {
                "ch106_skill_01_stand_start",
                "ch106_skill_01_stand_loop",
                "ch106_skill_01_stand_end",
                "ch106_skill_01_jump_start",
                "ch106_skill_01_jump_loop",
                "ch106_skill_01_jump_end",
                "ch106_skill_01_crouch_start",
                "ch106_skill_01_crouch_loop",
                "ch106_skill_01_crouch_end",
                "ch106_skill_02_start",
                "ch106_skill_02_loop",
                "ch106_skill_02_stand_end",
                "ch106_skill_02_jump_end"
            });
        }

        protected bool bInSkill;
        protected bool bPlayRedBalls;
        protected List<ParticleSystem> _liFxWindBursts = new List<ParticleSystem>();
        protected List<ParticleSystem> _liFxRedBalls = new List<ParticleSystem>();
        protected List<ParticleSystem> _liFxWindFires = new List<ParticleSystem>();
        protected ParticleSystem fxuse_dragonbreath_main;
        //protected CH106_BeamBullet _pBeam;
        protected Vector3 _vSkill0ShootDirection;
        protected int _nLockSkill1Direction;
        protected FxBase _fxUseSkill0;
        protected IAimTarget _pRushTarget;
        protected Vector3 _vSkillStartPosition;
        protected Vector2 _vSkillVelocity;
        protected RushCollideBullet _pRushCollideBullet;
        protected bool _bSyncRushSkillCompleted;
        protected Vector3 _vSyncDirection;
        private OrangeConsoleCharacter _refPlayer;
    }
}
