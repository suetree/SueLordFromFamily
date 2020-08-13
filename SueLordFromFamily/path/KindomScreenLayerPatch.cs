using HarmonyLib;
using SandBox.GauntletUI;
using SueLordFromFamily.view;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Engine.Screens;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.GauntletUI;

namespace SueLordFromFamily.path
{
	[HarmonyPatch(typeof(ScreenBase))]
	class KindomScreenLayerPatch
	{

		internal static GauntletLayer screenLayer;
		internal static KindomScreenVM kindomScreenVM;

		[HarmonyPatch("AddLayer")]
		public static void Postfix(ref ScreenBase __instance)
		{
			GauntletKingdomScreen gauntletClanScreen = __instance as GauntletKingdomScreen;
			bool flag = gauntletClanScreen != null && KindomScreenLayerPatch.screenLayer == null;
			if (flag)
			{
				KindomScreenLayerPatch.screenLayer = new GauntletLayer(100, "GauntletLayer");
				KindomScreenLayerPatch.kindomScreenVM = new KindomScreenVM(gauntletClanScreen);
				KindomScreenLayerPatch.screenLayer.LoadMovie("KindomScreen", KindomScreenLayerPatch.kindomScreenVM);
				KindomScreenLayerPatch.screenLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
				gauntletClanScreen.AddLayer(KindomScreenLayerPatch.screenLayer);
			}
		}

		[HarmonyPatch("RemoveLayer")]
		public static void Prefix(ref ScreenBase __instance, ref ScreenLayer layer)
		{
			bool flag = __instance is GauntletKingdomScreen && KindomScreenLayerPatch.screenLayer != null && layer.Input.IsCategoryRegistered(HotKeyManager.GetCategory("GenericCampaignPanelsGameKeyCategory"));
			if (flag)
			{
				__instance.RemoveLayer(KindomScreenLayerPatch.screenLayer);
				KindomScreenLayerPatch.kindomScreenVM.OnFinalize();
				KindomScreenLayerPatch.kindomScreenVM = null;
				KindomScreenLayerPatch.screenLayer = null;
			}
		}
	}


	}
