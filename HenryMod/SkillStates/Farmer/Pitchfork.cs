﻿using EntityStates;
using RoR2;
using UnityEngine;

namespace FirstLightMod.SkillStates
{
    public class Pitchfork : BaseSkillState
    {
        public static float damageCoefficient = Modules.Config.forkDamageCoefficient.Value;
        public static float procCoefficient = 1f;
        public static float baseDuration = 0.5f;
        public static float force = 400f;
        public static float recoil = 3f;
        public static float range = 256f;
        public static GameObject tracerEffectPrefab = RoR2.LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/Tracers/TracerToolbotRebar");

        private float duration;
        private float fireTime;
        private bool hasFired;
        private string muzzleString;

        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = Pitchfork.baseDuration / this.attackSpeedStat;
            this.fireTime = 0.2f * this.duration;
            base.characterBody.SetAimTimer(2f);
            this.muzzleString = "Muzzle";

            base.PlayAnimation("LeftArm, Override", "ShootGun", "ShootGun.playbackRate", 1.8f);
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        private void Fire()
        {
            if (!this.hasFired)
            {
                this.hasFired = true;

                base.characterBody.AddSpreadBloom(1.5f);
                EffectManager.SimpleMuzzleFlash(EntityStates.Commando.CommandoWeapon.FirePistol2.muzzleEffectPrefab, base.gameObject, this.muzzleString, false);
                Util.PlaySound("HenryShootPistol", base.gameObject);

                if (base.isAuthority)
                {
                    Ray aimRay = base.GetAimRay();
                    base.AddRecoil(-1f * Pitchfork.recoil, -2f * Pitchfork.recoil, -0.5f * Pitchfork.recoil, 0.5f * Pitchfork.recoil);

                    new BulletAttack
                    {
                        bulletCount = 1,
                        aimVector = aimRay.direction,
                        origin = aimRay.origin,
                        damage = Pitchfork.damageCoefficient * this.damageStat,
                        damageColorIndex = DamageColorIndex.Default,
                        damageType = DamageType.BleedOnHit, //Should now slow and bleed targets hit
                        falloffModel = BulletAttack.FalloffModel.DefaultBullet,
                        maxDistance = Pitchfork.range,
                        force = Pitchfork.force,
                        hitMask = LayerIndex.CommonMasks.bullet,
                        minSpread = 0f,
                        maxSpread = 0f,
                        isCrit = base.RollCrit(),
                        owner = base.gameObject,
                        muzzleName = muzzleString,
                        smartCollision = false,
                        procChainMask = default(ProcChainMask),
                        procCoefficient = procCoefficient,
                        radius = 0.75f,
                        sniper = false,
                        stopperMask = LayerIndex.world.collisionMask, //This should cause piercing?
                        weapon = null,
                        tracerEffectPrefab = Pitchfork.tracerEffectPrefab,
                        spreadPitchScale = 0f,
                        spreadYawScale = 2.5f,
                        queryTriggerInteraction = QueryTriggerInteraction.UseGlobal,
                        hitEffectPrefab = EntityStates.Commando.CommandoWeapon.FireThermite.effectPrefab
                    }.Fire();
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= this.fireTime)
            {
                this.Fire();
            }

            if (base.fixedAge >= this.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}

