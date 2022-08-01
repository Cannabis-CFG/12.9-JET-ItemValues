using System.Collections.Generic;
using System;
using EFT.InventoryLogic;
using HarmonyLib;
using MelonLoader;
using Comfort;

using Ammo = GClass1709; // Found
using Grenade = GClass1171; // Found
using GrenadeTemplate = GClass1611; // Errors
using SecureContainer = GClass1675; // Found
using SecureContainerTemplate = GClass1578; // Found
using Container = GClass1625; // Found
using ContainerTemplate = GClass1528;
using Magazine = GClass1663; // Found
using ItemAttribute = GClass1721; // Found

namespace ItemValue
{
    public class ItemValue : MelonMod
    {
        //public override Dictionary<string, string> DefaultOptions => null;

        public override void OnApplicationStart()
        {
            base.OnApplicationStart();
            var harmony = new HarmonyLib.Harmony("com.can.ItemValuePatch");
            harmony.PatchAll();
        }

        public static void AddItemValue<T>(ref T __instance, string id, ItemTemplate template) where T : Item
        {
            // Remove item if it has no value
            // if (Math.Round(__instance.Value()) == 0) return;

            // Make a copy of the existing attributes list, this is needed for inherited types of Item that use a global attributes list (ammo)
            var atts = new List<ItemAttribute>();
            atts.AddRange(__instance.Attributes);
            __instance.Attributes = atts;

            ItemAttribute attr = new ItemAttribute(EItemAttributeId.MoneySum)
            {
                Name = "RUB ₽",
                StringValue = new Func<string>(__instance.ValueStr),
                DisplayType = new Func<EItemAttributeDisplayType>(() => EItemAttributeDisplayType.Compact)
            };
            __instance.Attributes.Add(attr);
        }
    }
    public static class ValueExtension
    {
        public static double Value(this Item item)
        {
            double price = item.Template.CreditsPrice;

            // Container
            if (item is Container container)
            {
                foreach (var slot in container.AllSlots)
                {
                    foreach (var i in slot.Items)
                    {
                        price += i.Value();
                    }
                }
            }

            if (item is Magazine mag)
            {
                foreach (var i in mag.Cartridges.Items)
                {
                    price += i.Value();
                }

            }

            if (item is Weapon wep)
            {
                foreach (Slot s in wep.Chambers)
                {
                    foreach (Item i in s.Items)
                    {
                        price += i.Value();
                    }
                }
            }

            var medKit = item.GetItemComponent<MedKitComponent>();
            if (medKit != null)
            {
                price *= medKit.HpResource / medKit.MaxHpResource;
            }

            var repair = item.GetItemComponent<RepairableComponent>();
            if (repair != null)
            {
                price *= repair.Durability / repair.MaxDurability;
            }

            var dogtag = item.GetItemComponent<DogtagComponent>();
            if (dogtag != null)
            {
                price *= dogtag.Level;
            }

            if (!item.SpawnedInSession)
            {
                price /= 2;
            }

            price *= item.StackObjectsCount;

            return price;
        }
        public static string ValueStr(this Item item)
        {
            return Math.Round(item.Value()).ToString();
        }
    }

    [HarmonyPatch]
    class ValuePatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Item), MethodType.Constructor, new Type[] { typeof(string), typeof(ItemTemplate) })]
        static void PostfixItem(ref Item __instance, string id, ItemTemplate template) => ItemValue.AddItemValue(ref __instance, id, template);
        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(Container), MethodType.Constructor, new Type[] {typeof(string), typeof(ContainerTemplate) })]
        //static void PostfixContainer(ref Container __instance, string id, ContainerTemplate template) => ItemValue.AddItemValue(ref __instance, id, template);

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Ammo), MethodType.Constructor, new Type[] { typeof(string), typeof(AmmoTemplate) })]
        static void PostfixAmmo(ref Ammo __instance, string id, AmmoTemplate template) => ItemValue.AddItemValue(ref __instance, id, template);

        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(Grenade), MethodType.Constructor, new Type[] { typeof(string), typeof(GrenadeTemplate) })]
        //static void PostfixGrenade(ref Grenade __instance, string id, GrenadeTemplate template) => ItemValue.AddItemValue(ref __instance, id, template);

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SecureContainer), MethodType.Constructor, new Type[] { typeof(string), typeof(SecureContainerTemplate) })]
        static void PostfixConainer(ref SecureContainer __instance, string id, SecureContainerTemplate template) => ItemValue.AddItemValue(ref __instance, id, template);
    }
}
