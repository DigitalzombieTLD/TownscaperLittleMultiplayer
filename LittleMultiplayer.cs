using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using MelonLoader;
using Mono.Cecil;
using TMPro;
using UnityEngine;
using System.Threading;
using System.Runtime.InteropServices;
using System.ComponentModel;
using DigitalRuby.Threading;
using ENet;
using HarmonyLib;
using Placemaker;
using UnhollowerRuntimeLib;
using Placemaker.Ui;

namespace LittleMultiplayer
{
    public class LittleMultiplayerMain : MelonMod
    {
		public static SunButton currentSunButton;

	    public override void OnApplicationStart()
		{
			ClassInjector.RegisterTypeInIl2Cpp<EZThread>();
			
			Application.runInBackground = true;
			MyInput.thisMod = this;

			OnScreenMessageManager.multiplayerBundle = Il2CppAssetBundleManager.LoadFromFile("Mods\\LittleMultiplayer.unity3d");

		}

		public override void OnSceneWasLoaded(int buildIndex, string sceneName)
		{
			if(sceneName == "Placemaker")
			{
				// Initializing ModUI
				GUI.Initialize(this);
				OnScreenMessageManager.InitOnscreenMessages();

				//Placemaker.Ui.SunButton.ImportSunValues()
				
			}
		}
		
		public override void OnUpdate()
		{
			MyInput.GetInput();			
		}

		public override void OnApplicationQuit()
		{
			IP.DeleteNATPortMapping();
		}


	}
}
