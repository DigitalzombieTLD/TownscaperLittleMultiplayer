using MelonLoader;
using HarmonyLib;
using Il2CppSystem;
using Placemaker;
using UnhollowerRuntimeLib;
using Unity.Mathematics;
using UnityEngine;
using System.Reflection;
using Placemaker.Ui;
using DigitalRuby.Threading;
using System.Collections;

namespace LittleMultiplayer
{
	public class Harmony_Main
	{
		[HarmonyLib.HarmonyPatch(typeof(HoverData), "SetHover")]
		public class SetHoverGetCoords
		{
			public static void Postfix(ref HoverData __instance)
			{
				if (!GlobalVars.currentHoverData)
				{
					GlobalVars.currentHoverData = __instance;
				}
			}
		}

		/*
		[HarmonyLib.HarmonyPatch(typeof(Maker), "Undo")]
		public class DisableUndoButton
		{
			public static bool Prefix(ref Maker __instance)
			{
				if (!GlobalVars.isServer || GlobalVars.isClient)
				{
					MelonLogger.Msg("Undo command disabled while multiplayer is active!");
					OnScreenMessageManager.DisplayMessage("You can't use [Undo] while multiplayer is active", 6, OnScreenMessageManager.MessageType.Info);
					return false;
				}
				return true;
			}
		}

		[HarmonyLib.HarmonyPatch(typeof(Maker), "Redo")]
		public class DisableRedoButton
		{
			public static bool Prefix(ref Maker __instance)
			{
				if (!GlobalVars.isServer || GlobalVars.isClient)
				{
					
					MelonLogger.Msg("Redo command disabled while multiplayer is active!");
					OnScreenMessageManager.DisplayMessage("You can't use [Redo] while multiplayer is active", 6, OnScreenMessageManager.MessageType.Info);
					return false;
				}
				return true;
			}
		}
		*/

		[HarmonyLib.HarmonyPatch(typeof(MasterClicker), "AddClick")]
		public class AddVoxelPatcher
		{			
			public static bool Prefix(ref MasterClicker __instance)
			{	
				if (GlobalVars.isServer || GlobalVars.isClient)
				{
					bool ValidAdd = BootMaster.instance.worldMaster.hoverData.validAdd;
					int DestinationHeight = BootMaster.instance.worldMaster.hoverData.dstHeight;
					float VertAngle = BootMaster.instance.worldMaster.hoverData.dstVert.angle;
					int2 HexPos = BootMaster.instance.worldMaster.hoverData.dstVert.hexPos;
					float2 PlanePos = BootMaster.instance.worldMaster.hoverData.dstVert.planePos;
					VoxelType VoxelType = __instance.currentVoxelType;

					if (ValidAdd)
					{
						if (GlobalVars.isServer)
						{
							GameSender.AddVoxel(6666, true, ValidAdd, DestinationHeight, VertAngle, HexPos, VoxelType, PlanePos);
							return true;							
						}

						if (GlobalVars.isClient)
						{
							if (GlobalVars.isSynchronizing)
							{
								MelonLogger.Msg("Game is still synchronizing, interactions disabled");
								return false;
							}

							if (GlobalVars.remoteServerAllowsBuilding)
							{
								GameSender.AddVoxel(GlobalVars.myPeerID, true, ValidAdd, DestinationHeight, VertAngle, HexPos, VoxelType, PlanePos);								
							}

							if (!GlobalVars.remoteServerAllowsBuilding)
							{
								MelonLogger.Msg("Server doesn't allow guest interactions!");
								return false;
							}							
						}
					}
					return true;
				}
				return true;
			}		
		}

		[HarmonyLib.HarmonyPatch(typeof(MasterClicker), "RemoveClick")]
		public class RemoveVoxelPatcher
		{
			public static bool Prefix(ref MasterClicker __instance)
			{
				if (GlobalVars.isServer || GlobalVars.isClient)
				{					
					bool validRemove = BootMaster.instance.worldMaster.hoverData.validRemove;
					
					if (validRemove)
					{
						int DestinationHeight = BootMaster.instance.worldMaster.hoverData.srcHeight;
						float VertAngle = BootMaster.instance.worldMaster.hoverData.srcVert.angle;
						int2 HexPos = BootMaster.instance.worldMaster.hoverData.srcHexPos;
						float2 PlanePos = BootMaster.instance.worldMaster.hoverData.srcVert.planePos;
						VoxelType voxelType = __instance.currentVoxelType;
						Vector3 voxelPosition = BootMaster.instance.worldMaster.hoverData.voxel.transform.position;

						if (GlobalVars.isServer)
						{
							GameSender.RemoveVoxel(0, true, validRemove, DestinationHeight, VertAngle, PlanePos, HexPos, voxelPosition, voxelType);
							return true;
						}
						
						if (GlobalVars.isClient)
						{
							if (GlobalVars.isSynchronizing)
							{
								MelonLogger.Msg("Game is still synchronizing, interactions disabled");
								return false;
							}

							if (GlobalVars.remoteServerAllowsBuilding)
							{
								GameSender.RemoveVoxel(0, true, validRemove, DestinationHeight, VertAngle, PlanePos, HexPos, voxelPosition, voxelType);
							}

							if (!GlobalVars.remoteServerAllowsBuilding)
							{
								MelonLogger.Msg("Server doesn't allow guest interactions!");
								return false;
							}
						}
					}
				}
				return true;
			}

		
		}

		[HarmonyLib.HarmonyPatch(typeof(MasterClicker), "PaintClick")]
		public class PaintVoxelPatcher
		{
			public static void Postfix(ref MasterClicker __instance)
			{
			/*
				if (GlobalVars.isServer || GlobalVars.isClient)
				{
					bool validRemove = BootMaster.instance.worldMaster.hoverData.validRemove;

					if (validRemove)
					{
						int DestinationHeight = BootMaster.instance.worldMaster.hoverData.srcHeight;
						float VertAngle = BootMaster.instance.worldMaster.hoverData.srcVert.angle;
						int2 HexPos = BootMaster.instance.worldMaster.hoverData.srcHexPos;
						float2 PlanePos = BootMaster.instance.worldMaster.hoverData.srcVert.planePos;
						VoxelType voxelType = __instance.currentVoxelType;
						Vector3 voxelPosition = BootMaster.instance.worldMaster.hoverData.voxel.transform.position;

						if (GlobalVars.isServer)
						{
							GameSender.PaintVoxel(0, true, validRemove, DestinationHeight, VertAngle, PlanePos, HexPos, voxelPosition, voxelType);
						}

						if (GlobalVars.isClient && GlobalVars.remoteServerAllowsBuilding)
						{
							GameSender.PaintVoxel(0, true, validRemove, DestinationHeight, VertAngle, PlanePos, HexPos, voxelPosition, voxelType);
						}
					}
				}
				*/
			}
		}
	
		[HarmonyLib.HarmonyPatch(typeof(WorldMaster), "Load", new System.Type[] { typeof(SaveData)})]
		public class NewTownLoaded
		{
			public static void Postfix(ref WorldMaster __instance, SaveData saveData)
			{
				if (GlobalVars.isServer)
				{
					MelonLogger.Msg("Synchronizing town after load with clients ...");
					string newTownString = saveData.saveString;
					
					MelonCoroutines.Start(SendTownAfterTime(newTownString));					
				}				
			}
		}

		public static IEnumerator SendTownAfterTime(string newTownString)
		{
			yield return new WaitForSeconds(3);
			GameSender.SendTownString(99, true, newTownString);			
		}		
	}
}
