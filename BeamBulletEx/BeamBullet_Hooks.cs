using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using Il2CppSystem.Collections.Generic;
using System;
using System.Linq;
using Tangerine.Patchers.LogicUpdate;

namespace MonHunCollabRestored.BeambulletEx
{
    public class BeamBullet_Hooks
    {
        public static List<Il2CppSystem.Object> lstBullet = new List<Il2CppSystem.Object>();
        private static bool bNeedClean = false;

        internal static void InitializeHarmony(Harmony harmony)
        {
            harmony.PatchAll(typeof(BeamBullet_Hooks));
        }

        [HarmonyPatch(typeof(OrangeSceneManager), nameof(OrangeSceneManager.ChangeSceneComplete))]
        [HarmonyPostfix]
        static void fw_OrangeSceneManager_ChangeScene(OrangeSceneManager __instance)
        {
            lstBullet.Clear();
        }


        #region CH106_BeamBullet Simulation
        [HarmonyPatch(typeof(BeamBullet), nameof(BeamBullet.OnStartMove))]
        [HarmonyPrefix]
        private static void fw_BeamBullet_OnStartMove(BeamBullet __instance, ref Il2CppSystem.Collections.IEnumerator __result)
        {
            //Only switch if this 2 bullet
            if (__instance.BulletData.s_MODEL == "p_valstraxlaser_000" || __instance.BulletData.s_MODEL == "p_valstraxlaser_000_01")
            {
                CH106_01_BeamBullet bullet = GetBulletFromList(__instance);
                if (!bullet.IsActivate)
                {
                    var bullet2 = new CH106_01_BeamBullet();
                    bullet2.Setup(__instance);
                    bullet2.BulletData = __instance.BulletData;
                    lstBullet.Add(bullet2);
                }
                __result = null;
            }
        }

        [HarmonyPatch(typeof(BeamBullet), nameof(BeamBullet.Update_Effect))]
        [HarmonyPrefix]
        public static void BeamBullet_UpdateEffect(BeamBullet __instance)
        {
            if (__instance.BulletData.s_MODEL == "p_valstraxlaser_000" || __instance.BulletData.s_MODEL == "p_valstraxlaser_000_01")
            {
                CH106_01_BeamBullet bullet = GetBulletFromList(__instance);
                if (!bullet.IsActivate)
                {
                    var bullet2 = new CH106_01_BeamBullet();
                    bullet2.Setup(__instance);
                    bullet2.BulletData = __instance.BulletData;
                    lstBullet.Add(bullet2);
                    bullet = bullet2;
                }

                bullet.UpdateEffect();
            }
        }

        [HarmonyPatch(typeof(BeamBullet), nameof(BeamBullet.BackToPool))]
        [HarmonyPrefix]
        private static void fw_BeamBullet_BackToPool(BeamBullet __instance)
        {
            CH106_01_BeamBullet bullet = GetBulletFromList(__instance);
            if (bullet.BulletData == null)
            {
                return;
            }

            if (bullet.BulletData.s_MODEL.Equals(__instance.BulletData.s_MODEL))
            {
                lstBullet.Remove(bullet);
            }

        }

        public static CH106_01_BeamBullet GetBulletFromList(BeamBullet bullet)
        {
            CH106_01_BeamBullet result = new CH106_01_BeamBullet();
            foreach (CH106_01_BeamBullet item in lstBullet)
            {

                if (bullet.BulletData.s_MODEL.Equals(item.BulletData.s_MODEL))
                {
                    result = item;
                    break;
                }
            }
            return result;
        }
        #endregion

        #region Register Class
        private static void RegisterClass(Type controllerType, Type[] interfaces = null)
        {
            if (!ClassInjector.IsTypeRegisteredInIl2Cpp(controllerType))
            {
                Plugin.Log.LogWarning($"Registering Beam Class: {controllerType.FullName}");

                interfaces ??= Array.Empty<Type>();
                if (typeof(ITangerineLogicUpdate).IsAssignableFrom(controllerType)
                    && !interfaces.Contains(typeof(ILogicUpdate)))
                {
                    // Add ILogicUpdate to list of interfaces
                    interfaces = interfaces.AddToArray(typeof(ILogicUpdate));
                }

                var options = new RegisterTypeOptions()
                {
                    Interfaces = new Il2CppInterfaceCollection(interfaces),
                };

                ClassInjector.RegisterTypeInIl2Cpp(controllerType, options);
            }
        }

        [HarmonyPatch(typeof(OrangeConst), nameof(OrangeConst.ConstInit))]
        [HarmonyPostfix]
        private static void OrangeConstInitPostfix()
        {
            RegisterClass(typeof(CH106_01_BeamBullet));
            //RegisterClass(typeof(ValstraxLaser_BeamBullet)); //Just Testing
        }
        #endregion
    }
}
