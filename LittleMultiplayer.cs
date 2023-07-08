using System.IO;
using MelonLoader;
using UnityEngine;
using DigitalRuby.Threading;
using UnhollowerRuntimeLib;
using Placemaker.Ui;
using System.Reflection;

namespace LittleMultiplayer
{
    public class LittleMultiplayerMain : MelonMod
    {
		public static SunButton currentSunButton;

	    public override void OnInitializeMelon()
		{
            LoadEmbeddedAssetBundle();
			
			Application.runInBackground = true;
			MyInput.thisMod = this;
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

        public static void LoadEmbeddedAssetBundle()
        {
            MemoryStream memoryStream;
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("LittleMultiplayer.Resources.LittleMultiplayerBundle");
            memoryStream = new MemoryStream((int)stream.Length);
            stream.CopyTo(memoryStream);

            OnScreenMessageManager.multiplayerBundle = Il2CppAssetBundleManager.LoadFromMemory(memoryStream.ToArray());
        }
    }
}
