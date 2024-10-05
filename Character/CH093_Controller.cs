using BepInEx.Unity.IL2CPP.Utils.Collections;
using CallbackDefs;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System.Collections;
using Tangerine.Patchers.LogicUpdate;
using Tangerine.Utils;
using UnityEngine;

namespace MonHunCollabRestored.Character
{
    public class CH093_Controller : CharacterControlBase, ITangerineLogicUpdate
    {
        #region Basic Setup (Logic)
        public System.IntPtr LogicPointer => this.Pointer;

        public void LogicUpdate()
        {
        }

        private void OnEnable()
        {
            TangerineLogicUpdateManager.AddUpdate(this);
        }

        private void OnDisable()
        {
            TangerineLogicUpdateManager.RemoveUpdate(this);
        }
        #endregion

        public override void Start()
        {
            this.CallBase<CharacterControlBase>("Start");
            this.InitializeSkillMesh();
            this.InitializeLinkSkillData();
            this._refEntity.teleportInVoicePlayed = true;
            UpdateSkillData(true);
        }

        private void InitializeSkillMesh()
        {
            var componentsInChildren = this._refEntity._transform.GetComponentsInChildren<Transform>(true).Cast<Il2CppReferenceArray<Transform>>();
            this._refEntity.ExtraTransforms = new Transform[3];
            this._refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(componentsInChildren, "L WeaponPoint", true);
            this._refEntity.ExtraTransforms[1] = OrangeBattleUtility.FindChildRecursive(componentsInChildren, "R WeaponPoint", true);
            this._refEntity.ExtraTransforms[2] = OrangeBattleUtility.FindChildRecursive(componentsInChildren, "Bip R UpperArm", true);
            UpdateUniqueMotion(this._refEntity, szSource, szTarget);

            Transform transform = OrangeBattleUtility.FindChildRecursive(componentsInChildren, "BusterMesh_m", true);
            Transform transform1 = OrangeBattleUtility.FindChildRecursive(componentsInChildren, "SaberMesh_m", true);
            Transform transform2 = OrangeBattleUtility.FindChildRecursive(componentsInChildren, "BackSaberMesh_m", true);

            this._busterMesh = transform.GetComponent<SkinnedMeshRenderer>();
            this._saberMesh = transform1.GetComponent<SkinnedMeshRenderer>();
            this._backSaberMesh = transform2.GetComponent<SkinnedMeshRenderer>();

            CollideBullet bulletCollider = this._refEntity.BulletCollider;
            bulletCollider.HitCallback = Il2CppSystem.Delegate.Combine(bulletCollider.HitCallback, (CallbackObj)new System.Action<Il2CppSystem.Object>(this.OnBulletColliderHit)).Cast<CallbackObj>();


            foreach (string fxName in System.Enum.GetNames(typeof(FxName)))
                MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(fxName, 2);

            ToggleWeapon(WeaponState.TELEPORT_IN);
        }

        private void InitializeLinkSkillData()
        {
            int n_LINK_SKILL = this._refEntity.PlayerSkills[0].BulletData.n_LINK_SKILL;
            if (n_LINK_SKILL == 0)
                return;
            
            SKILL_TABLE skill_TABLE;
            if (ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetNewValue(n_LINK_SKILL, out skill_TABLE))
            {
                this._refEntity.tRefPassiveskill.ReCalcuSkill(ref skill_TABLE);
                this._SKL0_LINK_DATA = skill_TABLE;
            }
        }

        public override void OverrideDelegateEvent()
        {
            this.CallBase<CharacterControlBase>("OverrideDelegateEvent");
            this._refEntity.SetStatusCharacterDependEvt = new System.Action<OrangeCharacter.MainStatus, OrangeCharacter.SubStatus>(this.SetStatusCharacterDepend);
            this._refEntity.AnimationEndCharacterDependEvt = new System.Action<OrangeCharacter.MainStatus, OrangeCharacter.SubStatus>(this.AnimationEndCharacterDepend);
            this._refEntity.ChangeComboSkillEventEvt = (CallbackObjs)new System.Action<Il2CppReferenceArray<Il2CppSystem.Object>>(this.ChangeComboSkillEvent);
            this._refEntity.StageTeleportOutCharacterDependEvt = (Callback)new System.Action(this.StageTeleportOutCharacterDepend);
            this._refEntity.StageTeleportInCharacterDependEvt = (Callback)new System.Action(this.StageTeleportInCharacterDepend);
            this._refEntity.PlayTeleportOutEffectEvt = (Callback)new System.Action(this.PlayTeleportOutEffect);
        }

        public void AnimationEndCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
        {
            if (mainStatus != OrangeCharacter.MainStatus.TELEPORT_IN)
            {

            }
            else if (subStatus == OrangeCharacter.SubStatus.TELEPORT_POSE)
            {
                ToggleWeapon(WeaponState.NORMAL);
                return;
            }
        }

        public void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
        {
            if (mainStatus == OrangeCharacter.MainStatus.TELEPORT_OUT && subStatus == OrangeCharacter.SubStatus.WIN_POSE)
            {
                ToggleWeapon(WeaponState.TELEPORT_IN);
            }
        }

        public void ChangeComboSkillEvent(Il2CppReferenceArray<Il2CppSystem.Object> parameters)
        {
            if (parameters.Length != 2)
            {
                return;
            }
            int num = parameters[0].Unbox<int>();
            int num2 = parameters[1].Unbox<int>();
            if (this._refEntity.CurMainStatus == OrangeCharacter.MainStatus.TELEPORT_IN || this._refEntity.CurMainStatus == OrangeCharacter.MainStatus.TELEPORT_OUT || this._refEntity.Hp <= 0)
            {
                return;
            }
            if (num == 0 && this._refEntity.PlayerSkills[0].Reload_index != num2)
            {
                this._refEntity.PlayerSkills[0].Reload_index = num2;
            }
        }

        public override void PlayerPressSkillCharacterCall(int id)
        {
            if (this._refEntity.CurrentActiveSkill != -1)
                return;

            if (id == 0)
            {
                if (!this._refEntity.CheckUseSkillKeyTrigger(id, true))
                    return;

                UseSkill0();
                return;
            }
        }

        protected void StageTeleportInCharacterDepend()
        {
            base.StartCoroutine(this.OnDelayToggleWeapon(WeaponState.NORMAL, 0.6f).WrapToIl2Cpp());
        }

        protected void StageTeleportOutCharacterDepend()
        {
            base.StartCoroutine(this.OnDelayToggleWeapon(WeaponState.TELEPORT_OUT, 0.6f).WrapToIl2Cpp());
        }

        private void PlayTeleportOutEffect()
        {
            base.StartCoroutine(this.OnDelayToggleWeapon(WeaponState.TELEPORT_OUT, 0.2f).WrapToIl2Cpp());
        }

        public override void PlayerReleaseSkillCharacterCall(int id)
        {
            if (this._refEntity.CurrentActiveSkill != -1)
                return;

            if (id == 1)
            { 
                if (!this._refEntity.CheckUseSkillKeyTrigger(id, true))
                    return;

                UseSkill1();
                return;
            }
        }

        public override void CheckSkill()
        {
            if (this._refEntity.CurMainStatus != OrangeCharacter.MainStatus.SKILL)
            {
                if (this._refEntity.CurrentActiveSkill == 1 && this._refEntity.CheckSkillEndByShootTimer())
                    this.ToggleWeapon(WeaponState.NORMAL);

                return;
            }

            if (this._refEntity.IsAnimateIDChanged() || this._refEntity.CurrentActiveSkill == -1)
            {
                return;
            }

            this.nowFrame = GameLogicUpdateManager.GameFrame;
            OrangeCharacter.MainStatus curMainStatus = this._refEntity.CurMainStatus;
            
            if (curMainStatus == OrangeCharacter.MainStatus.SKILL)
            {
                OrangeCharacter.SubStatus curSubStatus = this._refEntity.CurSubStatus;
                UpdateSkillData();
                //Plugin.Log.LogInfo($"Online Style: {_IsOnlineMovement} - Speed: {SKL_0_DASHSPEED}");
                if (_IsOnlineMovement)
                {
                    switch (curSubStatus)
                    {
                        case OrangeCharacter.SubStatus.SKILL0:
                            {
                                if (this.nowFrame >= this.endFrame)
                                {
                                    this.endBreakFrame = GameLogicUpdateManager.GameFrame + this.SKL_TTS_FIRST_SWING;
                                    ManagedSingleton<CharacterControlHelper>.Instance.ChangeToNextStatus(this._refEntity, 0, this.SKL_TTS_FIRST_SWING, OrangeCharacter.SubStatus.SKILL0_1, out this.skillEventFrame, out this.endFrame);
                                    return;
                                }
                                if (!this.isSkillEventEnd && this.nowFrame >= this.skillEventFrame)
                                {
                                    this.isSkillEventEnd = true;
                                    return;
                                }
                                if (this.isSkillEventEnd && this.nowFrame >= this.endBreakFrame)
                                {
                                    ManagedSingleton<CharacterControlHelper>.Instance.CheckBreakFrame(this._refEntity.UserID, ref this.endFrame);
                                    return;
                                }
                                break;
                            }
                        case OrangeCharacter.SubStatus.SKILL0_1:
                            {
                                if (this.nowFrame >= this.endFrame)
                                {
                                    this.endBreakFrame = GameLogicUpdateManager.GameFrame + this.SKL_TTS_FIRST_SWING_HIT;
                                    ManagedSingleton<CharacterControlHelper>.Instance.ChangeToNextStatus(this._refEntity, 0, this.SKL_TTS_FIRST_SWING_HIT, OrangeCharacter.SubStatus.SKILL0_2, out this.skillEventFrame, out this.endFrame);
                                    Tangerine.Utils.Il2CppHelpers.FxManagerPlay(FxName.fxuse_chargecut_001.ToString(), this._refEntity._transform.position + Vector3.right * this.SKILL_TTS_HIT_FX_POS_SHIFT * (float)this._refEntity.direction, (this._refEntity.ShootDirection.x > 0f) ? OrangeCharacter.NormalQuaternion : OrangeCharacter.ReversedQuaternion);
                                    int x = (int)(Mathf.RoundToInt((float)this._refEntity._characterDirection * (float)OrangeCharacter.WalkSpeed * this.SKL_0_DASHSPEED));
                                    this._refEntity.SetSpeed(x, 0);

                                    ToggleCollideBullet(true);
                                    return;
                                }
                                if (!this.isSkillEventEnd && this.nowFrame >= this.skillEventFrame)
                                {
                                    this.isSkillEventEnd = true;
                                    return;
                                }
                                if (this.isSkillEventEnd && this.nowFrame >= this.endBreakFrame)
                                {
                                    ManagedSingleton<CharacterControlHelper>.Instance.CheckBreakFrame(this._refEntity.UserID, ref this.endFrame);
                                    return;
                                }
                                break;
                            }
                        case OrangeCharacter.SubStatus.SKILL0_2:
                            {
                                if (this.nowFrame >= this.endFrame)
                                {
                                    this.endBreakFrame = GameLogicUpdateManager.GameFrame + this.SKL_TTS_SECOND_SWING;
                                    ManagedSingleton<CharacterControlHelper>.Instance.ChangeToNextStatus(this._refEntity, 0, this.SKL_TTS_SECOND_SWING, OrangeCharacter.SubStatus.SKILL0_3, out this.skillEventFrame, out this.endFrame);
                                    ToggleCollideBullet(false);
                                    return;
                                }
                                if (!this.isSkillEventEnd && this.nowFrame >= this.skillEventFrame)
                                {
                                    this.isSkillEventEnd = true;
                                    return;
                                }
                                if (this.isSkillEventEnd && this.nowFrame >= this.endBreakFrame)
                                {
                                    ManagedSingleton<CharacterControlHelper>.Instance.CheckBreakFrame(this._refEntity.UserID, ref this.endFrame);
                                    return;
                                }
                                break;
                            }
                        case OrangeCharacter.SubStatus.SKILL0_3:
                            {
                                if (this.nowFrame >= this.endFrame)
                                {
                                    this.endBreakFrame = GameLogicUpdateManager.GameFrame + this.SKL_TTS_SECOND_SWING_HIT;
                                    ManagedSingleton<CharacterControlHelper>.Instance.ChangeToNextStatus(this._refEntity, 0, this.SKL_TTS_SECOND_SWING_HIT, OrangeCharacter.SubStatus.SKILL0_4, out this.skillEventFrame, out this.endFrame);
                                    Tangerine.Utils.Il2CppHelpers.FxManagerPlay(FxName.fxuse_chargecut_002.ToString(), this._refEntity._transform.position + Vector3.right * this.SKILL_TTS_HIT_FX_POS_SHIFT * (float)this._refEntity.direction, (this._refEntity.ShootDirection.x > 0f) ? OrangeCharacter.NormalQuaternion : OrangeCharacter.ReversedQuaternion);
                                    this._refEntity.SetSpeed(0, 0);
                                    ToggleCollideBullet(true, true);
                                    return;
                                }
                                if (!this.isSkillEventEnd && this.nowFrame >= this.skillEventFrame)
                                {
                                    this.isSkillEventEnd = true;
                                    return;
                                }
                                if (this.isSkillEventEnd && this.nowFrame >= this.endBreakFrame)
                                {
                                    ManagedSingleton<CharacterControlHelper>.Instance.CheckBreakFrame(this._refEntity.UserID, ref this.endFrame);
                                    return;
                                }
                                break;
                            }
                        case OrangeCharacter.SubStatus.SKILL0_4:
                            {
                                if (this.nowFrame >= this.endFrame)
                                {
                                    this.endBreakFrame = GameLogicUpdateManager.GameFrame + this.SKL_TTS_END_BREAK;
                                    ManagedSingleton<CharacterControlHelper>.Instance.ChangeToNextStatus(this._refEntity, 0, this.SKL_TTS_FINISH, OrangeCharacter.SubStatus.SKILL0_5, out this.skillEventFrame, out this.endFrame);
                                    ToggleCollideBullet(false);
                                    return;
                                }
                                if (!this.isSkillEventEnd && this.nowFrame >= this.skillEventFrame)
                                {
                                    this.isSkillEventEnd = true;
                                    return;
                                }
                                if (this.isSkillEventEnd && this.nowFrame >= this.endBreakFrame)
                                {
                                    ManagedSingleton<CharacterControlHelper>.Instance.CheckBreakFrame(this._refEntity.UserID, ref this.endFrame);
                                    return;
                                }
                                break;
                            }
                        case OrangeCharacter.SubStatus.SKILL0_5:
                            {
                                if (this.nowFrame >= this.endFrame || (!this._refEntity.IsInGround & this.nowFrame >= this.endBreakFrame))
                                {
                                    this.OnSkillEnd();
                                    return;
                                }
                                if (!this.isSkillEventEnd && this.nowFrame >= this.skillEventFrame)
                                {
                                    this.isSkillEventEnd = true;
                                    return;
                                }
                                if (this.isSkillEventEnd && this.nowFrame >= this.endBreakFrame)
                                {
                                    ManagedSingleton<CharacterControlHelper>.Instance.CheckBreakFrame(this._refEntity.UserID, ref this.endFrame);
                                    return;
                                }
                                break;
                            }
                        case OrangeCharacter.SubStatus.SKILL1:
                            {
                                if (this.nowFrame >= this.endFrame)
                                {
                                    this.OnSkillEnd();
                                    return;
                                }
                                if (!this.isSkillEventEnd && this.nowFrame >= this.skillEventFrame)
                                {
                                    this.SummonFireRing();
                                    this.isSkillEventEnd = true;
                                    return;
                                }
                                if (this.isSkillEventEnd && this.nowFrame >= this.endBreakFrame)
                                {
                                    ManagedSingleton<CharacterControlHelper>.Instance.CheckBreakFrame(this._refEntity.UserID, ref this.endFrame);
                                    return;
                                }
                                break;
                            }
                    }
                }
                else
                {
                    switch (curSubStatus)
                    {
                        case OrangeCharacter.SubStatus.SKILL0:
                            {
                                if (this.nowFrame >= this.endFrame)
                                {
                                    this.endBreakFrame = GameLogicUpdateManager.GameFrame + this.SKL_TTS_FIRST_SWING;
                                    ManagedSingleton<CharacterControlHelper>.Instance.ChangeToNextStatus(this._refEntity, 0, this.SKL_TTS_FIRST_SWING, OrangeCharacter.SubStatus.SKILL0_1, out this.skillEventFrame, out this.endFrame);
                                    int x = (int)(Mathf.RoundToInt((float)this._refEntity._characterDirection * (float)OrangeCharacter.WalkSpeed * this.SKL_0_DASHSPEED));
                                    this._refEntity.SetSpeed(x, 0);
                                    return;
                                }
                                if (!this.isSkillEventEnd && this.nowFrame >= this.skillEventFrame)
                                {
                                    this.isSkillEventEnd = true;
                                    return;
                                }
                                if (this.isSkillEventEnd && this.nowFrame >= this.endBreakFrame)
                                {
                                    ManagedSingleton<CharacterControlHelper>.Instance.CheckBreakFrame(this._refEntity.UserID, ref this.endFrame);
                                    return;
                                }
                                break;
                            }
                        case OrangeCharacter.SubStatus.SKILL0_1:
                            {
                                if (this.nowFrame >= this.endFrame)
                                {
                                    this.endBreakFrame = GameLogicUpdateManager.GameFrame + this.SKL_TTS_FIRST_SWING_HIT;
                                    ManagedSingleton<CharacterControlHelper>.Instance.ChangeToNextStatus(this._refEntity, 0, this.SKL_TTS_FIRST_SWING_HIT, OrangeCharacter.SubStatus.SKILL0_2, out this.skillEventFrame, out this.endFrame);
                                    Tangerine.Utils.Il2CppHelpers.FxManagerPlay(FxName.fxuse_chargecut_001.ToString(), this._refEntity._transform.position + Vector3.right * this.SKILL_TTS_HIT_FX_POS_SHIFT * (float)this._refEntity.direction, (this._refEntity.ShootDirection.x > 0f) ? OrangeCharacter.NormalQuaternion : OrangeCharacter.ReversedQuaternion);
                                    ToggleCollideBullet(true);
                                    return;
                                }
                                if (!this.isSkillEventEnd && this.nowFrame >= this.skillEventFrame)
                                {
                                    this.isSkillEventEnd = true;
                                    return;
                                }
                                if (this.isSkillEventEnd && this.nowFrame >= this.endBreakFrame)
                                {
                                    ManagedSingleton<CharacterControlHelper>.Instance.CheckBreakFrame(this._refEntity.UserID, ref this.endFrame);
                                    return;
                                }
                                break;
                            }
                        case OrangeCharacter.SubStatus.SKILL0_2:
                            {
                                if (this.nowFrame >= this.endFrame)
                                {
                                    this.endBreakFrame = GameLogicUpdateManager.GameFrame + this.SKL_TTS_SECOND_SWING;
                                    ManagedSingleton<CharacterControlHelper>.Instance.ChangeToNextStatus(this._refEntity, 0, this.SKL_TTS_SECOND_SWING, OrangeCharacter.SubStatus.SKILL0_3, out this.skillEventFrame, out this.endFrame);
                                    ToggleCollideBullet(false);
                                    return;
                                }
                                if (!this.isSkillEventEnd && this.nowFrame >= this.skillEventFrame)
                                {
                                    this.isSkillEventEnd = true;
                                    return;
                                }
                                if (this.isSkillEventEnd && this.nowFrame >= this.endBreakFrame)
                                {
                                    ManagedSingleton<CharacterControlHelper>.Instance.CheckBreakFrame(this._refEntity.UserID, ref this.endFrame);
                                    return;
                                }
                                break;
                            }
                        case OrangeCharacter.SubStatus.SKILL0_3:
                            {
                                if (this.nowFrame >= this.endFrame)
                                {
                                    this.endBreakFrame = GameLogicUpdateManager.GameFrame + this.SKL_TTS_SECOND_SWING_HIT;
                                    ManagedSingleton<CharacterControlHelper>.Instance.ChangeToNextStatus(this._refEntity, 0, this.SKL_TTS_SECOND_SWING_HIT, OrangeCharacter.SubStatus.SKILL0_4, out this.skillEventFrame, out this.endFrame);
                                    Tangerine.Utils.Il2CppHelpers.FxManagerPlay(FxName.fxuse_chargecut_002.ToString(), this._refEntity._transform.position + Vector3.right * this.SKILL_TTS_HIT_FX_POS_SHIFT * (float)this._refEntity.direction, (this._refEntity.ShootDirection.x > 0f) ? OrangeCharacter.NormalQuaternion : OrangeCharacter.ReversedQuaternion);
                                    this._refEntity.SetSpeed(0, 0);
                                    ToggleCollideBullet(true, true);
                                    return;
                                }
                                if (!this.isSkillEventEnd && this.nowFrame >= this.skillEventFrame)
                                {
                                    this.isSkillEventEnd = true;
                                    return;
                                }
                                if (this.isSkillEventEnd && this.nowFrame >= this.endBreakFrame)
                                {
                                    ManagedSingleton<CharacterControlHelper>.Instance.CheckBreakFrame(this._refEntity.UserID, ref this.endFrame);
                                    return;
                                }
                                break;
                            }
                        case OrangeCharacter.SubStatus.SKILL0_4:
                            {
                                if (this.nowFrame >= this.endFrame)
                                {
                                    ManagedSingleton<CharacterControlHelper>.Instance.ChangeToNextStatus(this._refEntity, 0, this.SKL_TTS_FINISH, OrangeCharacter.SubStatus.SKILL0_5, out this.skillEventFrame, out this.endFrame);
                                    ToggleCollideBullet(false);
                                    return;
                                }
                                if (!this.isSkillEventEnd && this.nowFrame >= this.skillEventFrame)
                                {
                                    this.isSkillEventEnd = true;
                                    return;
                                }
                                if (this.isSkillEventEnd && this.nowFrame >= this.endBreakFrame)
                                {
                                    ManagedSingleton<CharacterControlHelper>.Instance.CheckBreakFrame(this._refEntity.UserID, ref this.endFrame);
                                    return;
                                }
                                break;
                            }
                        case OrangeCharacter.SubStatus.SKILL0_5:
                            {
                                if (this.nowFrame >= this.endFrame || !this._refEntity.IsInGround)
                                {
                                    this.OnSkillEnd();
                                    return;
                                }
                                if (!this.isSkillEventEnd && this.nowFrame >= this.skillEventFrame)
                                {
                                    this.isSkillEventEnd = true;
                                    return;
                                }
                                if (this.isSkillEventEnd && this.nowFrame >= this.endBreakFrame)
                                {
                                    ManagedSingleton<CharacterControlHelper>.Instance.CheckBreakFrame(this._refEntity.UserID, ref this.endFrame);
                                    return;
                                }
                                break;
                            }
                        case OrangeCharacter.SubStatus.SKILL1:
                            {
                                if (this.nowFrame >= this.endFrame)
                                {
                                    this.OnSkillEnd();
                                    return;
                                }
                                if (!this.isSkillEventEnd && this.nowFrame >= this.skillEventFrame)
                                {
                                    this.SummonFireRing();
                                    this.isSkillEventEnd = true;
                                    return;
                                }
                                if (this.isSkillEventEnd && this.nowFrame >= this.endBreakFrame)
                                {
                                    ManagedSingleton<CharacterControlHelper>.Instance.CheckBreakFrame(this._refEntity.UserID, ref this.endFrame);
                                    return;
                                }
                                break;
                            }
                    }
                }
            }
        }

        private void OnSkillEnd()
        {
            if (this._refEntity.IgnoreGravity)
            {
                this._refEntity.IgnoreGravity = false;
            }
            this.isSkillEventEnd = false;
            this._refEntity.SkillEnd = true;
            this._refEntity.CurrentActiveSkill = -1;
            this._refEntity.Animator._animator.speed = 1f;
            this.ToggleWeapon(WeaponState.NORMAL);
            HumanBase.AnimateId animateID = this._refEntity.AnimateID;
            if (animateID != HumanBase.AnimateId.ANI_SKILL_START && animateID != HumanBase.AnimateId.ANI_BTSKILL_START)
            {
                if (this._refEntity.IsInGround)
                {
                    this._refEntity.Dashing = false;
                    this._refEntity.SetSpeed(0, 0);
                    this._refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
                    return;
                }
                this._refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
                return;
            }
            else
            {
                this._refEntity.Dashing = false;
                if (ManagedSingleton<InputStorage>.Instance.IsHeld(this._refEntity.UserID, ButtonId.DOWN))
                {
                    this._refEntity.SetStatus(OrangeCharacter.MainStatus.CROUCH, OrangeCharacter.SubStatus.WIN_POSE);
                    return;
                }
                this._refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.CROUCH_UP);
                return;
            }
        }

        public override void ClearSkill()
        {
            if (this._isSkillShooting)
            {
                this._refEntity.CancelBusterChargeAtk();
                this._isSkillShooting = false;
            }

            this._refEntity.SkillEnd = true;
            this._refEntity.CurrentActiveSkill = -1;
            this.ToggleWeapon(WeaponState.NORMAL);
            ToggleCollideBullet(false);

        }
        public override void SetStun(bool enable)
        {
        }

        public override int GetUniqueWeaponType()
        {
            return 1;
        }

        public override Il2CppStringArray GetCharacterDependAnimations()
        {
            return new Il2CppStringArray(new string[]
            {
                "ch093_skill_01",
                "ch093_skill_02_step2"
            });
        }

        private void UseSkill0()
        {
            ToggleWeapon(WeaponState.SKILL_TRUE_CHARGE_SLASH);
            DoTrueChargeSlash();
            PlayVoiceSE("v_xm_skill01");
            PlaySkillSE("xm_tame01");
            Tangerine.Utils.Il2CppHelpers.FxManagerPlay(FxName.fxuse_chargecut_000.ToString(), this._refEntity._transform.position, (this._refEntity.ShootDirection.x > 0f) ? OrangeCharacter.NormalQuaternion : OrangeCharacter.ReversedQuaternion);
        }

        private void DoTrueChargeSlash()
        {
            int id = 0;
            this._refEntity.IgnoreGravity = true;
            this._refEntity.Dashing = true;

            WeaponStruct weaponStruct = this._refEntity.PlayerSkills[id];
            this._refEntity.CheckUsePassiveSkill(id, weaponStruct.weaponStatus, this._refEntity.ExtraTransforms[0]);
            OrangeBattleUtility.UpdateSkillCD(weaponStruct);
            
            this.endBreakFrame = GameLogicUpdateManager.GameFrame + this.SKL_TTS_FIRST_MOVE;
            ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(this._refEntity, 0, 0, 1, OrangeCharacter.SubStatus.SKILL0, out this.skillEventFrame, out this.endFrame);
        
            this._refEntity.SetAnimateId(SKL_TRUE_CHARGE_SLASH_ANIM);
        }

        private void ToggleCollideBullet(bool isActive, bool isSecondHit = false)
        {
            if (isActive)
            {
                if (!this._isCollideBulletSetted)
                {
                    WeaponStruct currentSkillObj = this._refEntity.GetCurrentSkillObj();
                    if (isSecondHit && this._SKL0_LINK_DATA != null)
                    {
                        BulletBase bulletCollider = this._refEntity.BulletCollider;
                        SKILL_TABLE linkSkillData = this._SKL0_LINK_DATA;
                        string sPlayerName = this._refEntity.sPlayerName;
                        int nowRecordNO = this._refEntity.GetNowRecordNO();
                        OrangeCharacter refEntity = this._refEntity;
                        int num = refEntity.nBulletRecordID;
                        refEntity.nBulletRecordID = num + 1;
                        bulletCollider.UpdateBulletData(linkSkillData, sPlayerName, nowRecordNO, num, this._refEntity.direction);
                    }
                    else
                    {
                        BulletBase bulletCollider2 = this._refEntity.BulletCollider;
                        SKILL_TABLE bulletData = currentSkillObj.BulletData;
                        string sPlayerName2 = this._refEntity.sPlayerName;
                        int nowRecordNO2 = this._refEntity.GetNowRecordNO();
                        OrangeCharacter refEntity2 = this._refEntity;
                        int num = refEntity2.nBulletRecordID;
                        refEntity2.nBulletRecordID = num + 1;
                        bulletCollider2.UpdateBulletData(bulletData, sPlayerName2, nowRecordNO2, num, this._refEntity.direction);
                    }
                    this._refEntity.BulletCollider.SetBulletAtk(currentSkillObj.weaponStatus, this._refEntity.selfBuffManager.sBuffStatus, null);
                    this._refEntity.BulletCollider.BulletLevel = currentSkillObj.SkillLV;
                    this._refEntity.BulletCollider.Active(this._refEntity.TargetMask);
                    this._isCollideBulletSetted = true;
                    return;
                }
            }
            else
            {
                this._refEntity.BulletCollider.BackToPool();
                this._isCollideBulletSetted = false;
                this._IsHitPauseStarted = false;
            }
        }

        private void OnBulletColliderHit(object obj)
        {
            StartHitPause(0.15f);
        }

        private void UseSkill1()
        {
            WeaponStruct weaponStruct = this._refEntity.PlayerSkills[1];
            this._refEntity.CurrentActiveSkill = 1;

            ToggleWeapon(WeaponState.SKILL_FIRE_BREATH);

            if (!HasComboSkill(1)) DoBusterShot(weaponStruct);
            else DoFireRing(weaponStruct);
        }

        private void DoBusterShot(WeaponStruct weaponStruct)
        {
            this._refEntity.IsShoot = 3;
            this._refEntity.StartShootTimer();
            this._refEntity.Animator.SetAnimatorEquip(1);
            this._isSkillShooting = true;
            this._refEntity.PushBulletDetail(weaponStruct.BulletData, weaponStruct.weaponStatus, this._refEntity.ExtraTransforms[0], weaponStruct.SkillLV, new Il2CppSystem.Nullable_Unboxed<UnityEngine.Vector3>(this._refEntity.ShootDirection), true);
            int reload_index = this._refEntity.PlayerSkills[0].Reload_index;
            this._refEntity.RemoveComboSkillBuff(weaponStruct.FastBulletDatas[0].n_ID);
            this._refEntity.TriggerComboSkillBuff(this._refEntity.PlayerSkills[0].FastBulletDatas[0].n_ID);
            this._refEntity.CheckUsePassiveSkill(1, weaponStruct.weaponStatus, this._refEntity.ExtraTransforms[0]);
            base.PlayVoiceSE("v_xm_skill02_1");
        }

        private void DoFireRing(WeaponStruct weaponStruct)
        {
            //Reset IsShot (if still at 3), Ignore gravitiy, grab ComboSkill and Push it bullet
            this._refEntity.IgnoreGravity = true;
            ManagedSingleton<CharacterControlHelper>.Instance.TurnToAimTarget(this._refEntity);

            //Play FX, Voice and Animation
            Tangerine.Utils.Il2CppHelpers.FxManagerPlay(FxName.fxuse_dragonslash_000.ToString(), this._refEntity._transform, OrangeCharacter.NormalQuaternion);
            ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(this._refEntity, SKL_FIRE_RING_ANIM, SKL_FIRE_RING_ANIM, SKL_FIRE_RING_ANIM, true);
            PlayVoiceSE("v_xm_skill02_2");

            //Set Skill Time & Status
            this.endBreakFrame = GameLogicUpdateManager.GameFrame + this.SKL_FIRE_RING_BREAK;
            ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(this._refEntity, 1, this.SKL_FIRE_RING_TRIGGER, this.SKL_FIRE_RING_END, OrangeCharacter.SubStatus.SKILL1, out this.skillEventFrame, out this.endFrame);
        }

        private void SummonFireRing()
        {
            this._refEntity.IsShoot = 0;
            
            WeaponStruct weaponStruct = this._refEntity.PlayerSkills[1];
            SKILL_TABLE skill_TABLE = weaponStruct.FastBulletDatas[weaponStruct.Reload_index];
            this._refEntity.PushBulletDetail(skill_TABLE, weaponStruct.weaponStatus, this._refEntity.ExtraTransforms[0], weaponStruct.SkillLV, new Il2CppSystem.Nullable_Unboxed<UnityEngine.Vector3>(Vector2.right * (float)this._refEntity.direction), true);
            
            OrangeBattleUtility.UpdateSkillCD(weaponStruct);
            this._refEntity.CheckUsePassiveSkill(1, weaponStruct.weaponStatus, this._refEntity.ExtraTransforms[0]);

            //Remove ComboSkill
            this._refEntity.RemoveComboSkillBuff(skill_TABLE.n_ID);
        }

        private void UpdateSkillData(bool init = false)
        {
            if (Plugin.CH093_OnlineTTSMovement.Value == _IsOnlineMovement && init)
                return;

            _IsOnlineMovement = Plugin.CH093_OnlineTTSMovement.Value;

            if (_IsOnlineMovement)
            {
                SKL_0_DASHSPEED = 2.0f;

                SKL_TTS_FIRST_MOVE = (int)(0.35f / GameLogicUpdateManager.m_fFrameLen);
                SKL_TTS_FIRST_SWING = (int)(0.3f / GameLogicUpdateManager.m_fFrameLen);
                SKL_TTS_FIRST_SWING_HIT = (int)(0.1 / GameLogicUpdateManager.m_fFrameLen);

                SKL_TTS_SECOND_SWING = (int)(0.28f / GameLogicUpdateManager.m_fFrameLen);
                SKL_TTS_SECOND_SWING_HIT = (int)(0.1f / GameLogicUpdateManager.m_fFrameLen);

                SKL_TTS_END_BREAK = (int)(0.2f / GameLogicUpdateManager.m_fFrameLen);
                SKL_TTS_FINISH = (int)(0.65f / GameLogicUpdateManager.m_fFrameLen);

                SKL_TTS_HIT_PAUSE = (int)(0.3f / GameLogicUpdateManager.m_fFrameLen);
            }
            else
            {
                SKL_0_DASHSPEED = 1.5f;
                SKL_TTS_FIRST_MOVE = (int)(0.3f / GameLogicUpdateManager.m_fFrameLen);
                SKL_TTS_FIRST_SWING = (int)(0.35f / GameLogicUpdateManager.m_fFrameLen);
                SKL_TTS_FIRST_SWING_HIT = (int)(0.1 / GameLogicUpdateManager.m_fFrameLen);

                SKL_TTS_SECOND_SWING = (int)(0.55f / GameLogicUpdateManager.m_fFrameLen);
                SKL_TTS_SECOND_SWING_HIT = (int)(0.1f / GameLogicUpdateManager.m_fFrameLen);

                SKL_TTS_FINISH = (int)(0.65f / GameLogicUpdateManager.m_fFrameLen);

                SKL_TTS_HIT_PAUSE = (int)(0.15f / GameLogicUpdateManager.m_fFrameLen);
            }  
        }

        private IEnumerator OnDelayToggleWeapon(WeaponState weaponState, float delay)
        {
            yield return new WaitForSeconds(delay);
            this.ToggleWeapon(weaponState);
            yield break;
        }

        private void ToggleWeapon(WeaponState weaponState)
        {
            switch (weaponState)
            {
                case WeaponState.TELEPORT_IN:
                    this._refEntity.DisableWeaponMesh(CurrentWeaponObj());
                    this._busterMesh.enabled = true;
                    this._backSaberMesh.enabled = true;
                    this._saberMesh.enabled = false;
                    this._refEntity.EnableHandMesh(false);
                    break;
                case WeaponState.TELEPORT_OUT:
                    this._refEntity.DisableWeaponMesh(CurrentWeaponObj());
                    this._busterMesh.enabled = false;
                    this._backSaberMesh.enabled = false;
                    this._saberMesh.enabled = false;
                    this._refEntity.EnableHandMesh(false);
                    break;
                case WeaponState.SKILL_TRUE_CHARGE_SLASH:
                    this._refEntity.DisableWeaponMesh(CurrentWeaponObj());
                    this._busterMesh.enabled = false;
                    this._backSaberMesh.enabled = false;
                    this._saberMesh.enabled = true;
                    break;
                case WeaponState.SKILL_FIRE_BREATH:
                    {
                        this._refEntity.DisableWeaponMesh(CurrentWeaponObj());
                        
                        if (!HasComboSkill(1))
                        {
                            this._busterMesh.enabled = true;
                            this._backSaberMesh.enabled = true;
                            this._saberMesh.enabled = false;
                            this._refEntity.EnableHandMesh(false);
                            break;
                        }
                        else
                        {
                            this._busterMesh.enabled = false;
                            this._backSaberMesh.enabled = false;
                            this._saberMesh.enabled = true;
                            break;
                        }
                    }
                case WeaponState.NORMAL:
                    {

                        this._refEntity.EnableWeaponMesh(CurrentWeaponObj());
                        this._busterMesh.enabled = false;
                        this._backSaberMesh.enabled = true;
                        this._saberMesh.enabled = false;
                        break;
                    }
            }
        }

        private bool HasComboSkill(int SkillIdx)
        {
            return this._refEntity.PlayerSkills[SkillIdx].ComboCheckDatas.Length != 0 && this._refEntity.PlayerSkills[SkillIdx].ComboCheckDatas[0].CheckHasAllBuff(this._refEntity.selfBuffManager);
        }


        public WeaponStruct CurrentWeaponObj()
        {
            return this._refEntity.PlayerWeapons[this._refEntity.WeaponCurrent];
        }

        private void UpdateUniqueMotion(OrangeCharacter pEntity, string[] originalAnims, string[] newAnims)
        {
            AnimatorOverrideController animtorOverrideController = pEntity.Animator._animator.runtimeAnimatorController.Cast<AnimatorOverrideController>();
            string bundle = "model/animation/character/" + pEntity.CharacterData.s_MODEL;

            if (originalAnims.Length != 0 && originalAnims[0] == "null")
                return;

            for (int i = 0; i < originalAnims.Length; i++)
            {
                animtorOverrideController.Internal_SetClipByName(originalAnims[i], MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<AnimationClip>(bundle, newAnims[i]));
            }

            pEntity.Animator._animator.runtimeAnimatorController = animtorOverrideController;
        }

        protected void StartHitPause(float pauseTime = 1f)
        {
            if (this._IsHitPauseStarted)
            {
                return;
            }

            this.endFrame += SKL_TTS_HIT_PAUSE;
            this.endBreakFrame += SKL_TTS_HIT_PAUSE;
            this._IsHitPauseStarted = true;
            this._lastVelocity = this._refEntity.Velocity;
            this._refEntity.SetSpeed(0, 0);
            StartCoroutine(this.HitPauseCoroutine(pauseTime).WrapToIl2Cpp());
        }

        private IEnumerator HitPauseCoroutine(float pauseTime)
        {
            this._refEntity.Animator._animator.speed = 0f;
            yield return new WaitForSeconds(pauseTime);
            this._refEntity.Animator._animator.speed = 1f;
            this._refEntity.SetSpeed(this._lastVelocity.x, this._lastVelocity.y);
            yield break;
        }

        private enum FxName
        {
            fxuse_chargecut_000,
            fxuse_chargecut_001,
            fxuse_chargecut_002,
            fxuse_dragonslash_000
        }

        enum WeaponState
        {
            TELEPORT_IN = 0,
            TELEPORT_OUT,
            NORMAL,
            SKILL_TRUE_CHARGE_SLASH,
            SKILL_FIRE_BREATH            
        }

        protected readonly HumanBase.AnimateId SKL_TRUE_CHARGE_SLASH_ANIM = (HumanBase.AnimateId)65U;
        protected readonly HumanBase.AnimateId SKL_FIRE_RING_ANIM = (HumanBase.AnimateId)66U;

        private int nowFrame;
        private int skillEventFrame;
        private int endFrame;
        private int endBreakFrame;
        private bool isSkillEventEnd;

        private bool _isSkillShooting = false;
        private bool _isCollideBulletSetted = false;
        private bool _IsHitPauseStarted = false;
        private bool _IsOnlineMovement = false;
        private VInt3 _lastVelocity;

        private SKILL_TABLE _SKL0_LINK_DATA;
        private SkinnedMeshRenderer _busterMesh;
        private SkinnedMeshRenderer _saberMesh;
        private SkinnedMeshRenderer _backSaberMesh;

        public float SKILL_TTS_HIT_FX_POS_SHIFT = 1.5f;
        private float SKL_0_DASHSPEED = 2.0f;

        private int SKL_FIRE_RING_TRIGGER = (int)(0.4f / GameLogicUpdateManager.m_fFrameLen);
        private int SKL_FIRE_RING_END = (int)(0.75f / GameLogicUpdateManager.m_fFrameLen);
        private int SKL_FIRE_RING_BREAK = (int)(0.5f / GameLogicUpdateManager.m_fFrameLen);

        private int SKL_TTS_FIRST_MOVE = (int)(0.35f / GameLogicUpdateManager.m_fFrameLen);
        private int SKL_TTS_FIRST_SWING = (int)(0.3f / GameLogicUpdateManager.m_fFrameLen);
        private int SKL_TTS_FIRST_SWING_HIT = (int)(0.1 / GameLogicUpdateManager.m_fFrameLen);

        private int SKL_TTS_SECOND_SWING = (int)(0.28f / GameLogicUpdateManager.m_fFrameLen);
        private int SKL_TTS_SECOND_SWING_HIT = (int)(0.1f / GameLogicUpdateManager.m_fFrameLen);

        private int SKL_TTS_END_BREAK = (int)(0.2f / GameLogicUpdateManager.m_fFrameLen);
        private int SKL_TTS_FINISH = (int)(0.65f / GameLogicUpdateManager.m_fFrameLen);

        private int SKL_TTS_HIT_PAUSE = (int)(0.3f / GameLogicUpdateManager.m_fFrameLen);



        protected readonly string[] szSource =
        {
            "login",
            "logout",
            "win",
            "buster_stand_charge_atk",
            "buster_crouch_charge_atk",
            "buster_jump_charge_atk",
            "buster_fall_charge_atk",
            "buster_wallgrab_charge_atk"
        };

        protected readonly string[] szTarget =
        {
            "ch093_login",
            "ch093_logout",
            "ch093_win",
            "ch093_skill_02_step1_stand",
            "ch093_skill_02_step1_Crouch",
            "ch093_skill_02_step1_Jump",
            "ch093_skill_02_step1_Fall",
            "ch093_skill_02_step1_Wallgrab"
        };

        
    }
}
