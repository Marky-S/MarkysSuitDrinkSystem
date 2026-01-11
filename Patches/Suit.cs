using Assets.Scripts;
using Assets.Scripts.Atmospherics;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Clothing;
using Assets.Scripts.Objects.Items;
using Assets.Scripts.Sound;
using Assets.Scripts.Util;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Assets.Scripts.Objects.Thing;

namespace MarkysSuitDrinkSystem.Patches
{
    public static class PatchData
    {
        public const string SlotName = "WaterTank";

        public static int SlotNameHash => Animator.StringToHash(SlotName);
    }

    [HarmonyPatch(typeof(Suit))]
    public class Suit_Patch
    {
        [HarmonyPatch("GetContextualName")]
        [HarmonyPrefix]
        public static bool GetContextualName_Prefix_Patch(ref string __result, Interactable interactable)
        {
            if (interactable.Action == InteractableType.Button5)
            {
                __result = "Drink";
                return false;
            }

            return true;
        }

        [HarmonyPatch("InteractWith")]
        [HarmonyPrefix]
        public static bool InteractWith_Prefix_Patch(ref DelayedActionInstance __result, Suit __instance, Interactable interactable, Interaction interaction, bool doAction)
        {
            var delayedActionInstance = new DelayedActionInstance
            {
                Duration = 0f,
                ActionMessage = interactable.ContextualName,
            };

            if (interactable.Action == InteractableType.Button5)
            {
                var waterTank = __instance.Slots.FirstOrDefault(v => v.StringHash == PatchData.SlotNameHash).Get<GasCanister>();
                if (waterTank is null)
                {
                    __result = delayedActionInstance.Fail();
                    return false;
                }

                var availableWater = waterTank.InternalAtmosphere.GasMixture.Water.Volume.ToFloat();
                if (availableWater <= 0.01f)
                {
                    __result = delayedActionInstance.Fail();
                    return false;
                }

                if (!doAction)
                {
                    __result = delayedActionInstance.Succeed();
                    return false;
                }

                if (__instance.ParentEntity != null && __instance.ParentEntity.IsLocalPlayer)
                {
                    Singleton<AudioManager>.Instance.PlayAudioClipsData(__instance.ParentEntity.ReferenceId, Item.DrinkingFinishedHash, Vector3.zero, null, 1, 1);
                }

                var desiredWater = (__instance.ParentEntity.GetHydrationStorage() - __instance.ParentEntity.Hydration) / 5f;
                var toDrink = Mathf.Min(desiredWater, availableWater);
                
                Plugin.Log.LogDebug($"{desiredWater}/{availableWater} | {toDrink}");
                
                __instance.ParentEntity.Hydrate(toDrink * 5f);
                waterTank.InternalAtmosphere.Remove(new MoleQuantity(55.55555555555556 * toDrink), Chemistry.GasType.Water);

                __result = delayedActionInstance.Succeed();
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Localization.Language))]
    public class Language_Patch
    {
        [HarmonyPatch("Load")]
        [HarmonyPostfix]
        public static void Language_Load_Postfix(Localization.Language __instance)
        {
            var slotsNameField = AccessTools.Field(typeof(Localization), "SlotsName");
            var slotsName = slotsNameField.GetValue(null) as Dictionary<int, string>;
            slotsName[PatchData.SlotNameHash] = "Water Tank";
        }
    }

    [HarmonyPatch(typeof(Thing))]
    public class Thing_Patch
    {
        [HarmonyPatch("Awake")]
        [HarmonyPrefix]
        public static void Awake_Prefix(Thing __instance)
        {
            var isEvaSuit = __instance.PrefabName == "ItemEvaSuit";
            var isHardSuit = __instance.PrefabName == "ItemHardSuit";
            var isHARMSuit = __instance.PrefabName == "ItemSuitHARM";

            if (!isEvaSuit && !isHardSuit && !isHARMSuit)
            {
                return;
            }

            if (__instance is not Suit suit)
            {
                return;
            }

            var slots = suit.Slots;

            slots.Add(new Slot
            {
                StringKey = PatchData.SlotName,
                StringHash = PatchData.SlotNameHash,
                Type = Slot.Class.LiquidCanister
            });

            var interactables = suit.Interactables;

            interactables.Add(new Interactable
            {
                Parent = suit,
                Action = InteractableType.Button5,
                CanKeyInteract = true
            });
        }
    }
}
