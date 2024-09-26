using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using Il2CppSystem.Collections.Generic;
using System;
using System.Linq;
using Tangerine.Patchers.LogicUpdate;

namespace MonHunCollabRestored.Beambullet
{
    public class BeamBullet_Hooks
    {
        public static Dictionary<string, Il2CppSystem.Object> lstBullet = new Dictionary<string, Il2CppSystem.Object>();
        private static bool bNeedClean = false;

        internal static void InitializeHarmony(Harmony harmony)
        {
            harmony.PatchAll(typeof(BeamBullet_Hooks));
        }

        [HarmonyPostfix, HarmonyPatch(typeof(OrangeSceneManager), nameof(OrangeSceneManager.ChangeSceneComplete))]
        static void fw_OrangeSceneManager_ChangeScene(OrangeSceneManager __instance)
        {
            lstBullet.Clear();
        }


        #region CH106_BeamBullet Simulation
        [HarmonyPrefix, HarmonyPatch(typeof(BeamBullet), nameof(BeamBullet.OnStartMove))]
        private static void fw_BeamBullet_OnStartMove(BeamBullet __instance, ref Il2CppSystem.Collections.IEnumerator __result)
        {
            //Only switch if this 2 bullet
            if (__instance.BulletData.s_MODEL == "p_valstraxlaser_000" || __instance.BulletData.s_MODEL == "p_valstraxlaser_000_01")
            {
                if (!lstBullet.ContainsKey(__instance.BulletData.s_MODEL))
                {
                    var bullet2 = new ValstraLaser_BeamBullet();
                    bullet2.Setup(__instance);
                    lstBullet.Add(__instance.BulletData.s_MODEL, bullet2);
                }
                __result = null;
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(BeamBullet), nameof(BeamBullet.Update_Effect))]
        public static void BeamBullet_UpdateEffect(BeamBullet __instance)
        {
            if (__instance.BulletData.s_MODEL == "p_valstraxlaser_000" || __instance.BulletData.s_MODEL == "p_valstraxlaser_000_01")
            {
                ValstraLaser_BeamBullet bullet;
                Il2CppSystem.Object temp;
                if (!lstBullet.TryGetValue(__instance.BulletData.s_MODEL, out temp))
                {
                    var bullet2 = new ValstraLaser_BeamBullet();
                    bullet2.Setup(__instance);
                    lstBullet.Add(__instance.BulletData.s_MODEL, bullet2);
                    bullet = bullet2;
                }
                else
                {
                    bullet = temp.Cast<ValstraLaser_BeamBullet>();
                }

                bullet.UpdateEffect();
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(BeamBullet), nameof(BeamBullet.BackToPool))]
        private static void fw_BeamBullet_BackToPool(BeamBullet __instance)
        {
            Il2CppSystem.Object temp;
            if (lstBullet.TryGetValue(__instance.BulletData.s_MODEL, out temp))
            {
                lstBullet.Remove(__instance.BulletData.s_MODEL);
            }
        }
        #endregion

        #region Register Class
        private static void RegisterClass(Type controllerType, Type[] interfaces = null)
        {
            try
            {
                if (!ClassInjector.IsTypeRegisteredInIl2Cpp(controllerType))
                {
                    interfaces ??= Array.Empty<Type>();
                    if (typeof(ITangerineLogicUpdate).IsAssignableFrom(controllerType) && !interfaces.Contains(typeof(ILogicUpdate)))
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
            catch (Exception ex)
            {
                Plugin.Log.LogError(ex);
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(OrangeConst), nameof(OrangeConst.ConstInit))]
        private static void OrangeConstInitPostfix()
        {
            RegisterClass(typeof(ValstraLaser_BeamBullet));
        }
        #endregion
    }
}
