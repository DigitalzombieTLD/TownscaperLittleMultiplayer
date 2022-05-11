using ENet;
using MelonLoader;
using System.Collections;
using TMPro;
using UnityEngine;

namespace LittleMultiplayer
{
	public static class OnScreenMessageManager
	{
		public static Il2CppAssetBundle multiplayerBundle;
		public static GameObject messageUI;
		public static GameObject bottomPanel;
		public static TextMeshProUGUI onScreenMessage;
		public static object runningRoutine;
		public static GameObject mySparkPrefab;

		public static void InitOnscreenMessages()
		{
			messageUI = UnityEngine.Object.Instantiate(multiplayerBundle.LoadAsset<GameObject>("OnscreenMessage"));
			UnityEngine.Object.DontDestroyOnLoad(messageUI);
			bottomPanel = messageUI.transform.Find("Bottompanel").gameObject;
			onScreenMessage = bottomPanel.GetComponentInChildren<TextMeshProUGUI>();
			bottomPanel.SetActive(false);

			// init spark
			mySparkPrefab = multiplayerBundle.LoadAsset<GameObject>("VfxBrightStars");			
		}

		public static void SetupPlayerSpark(uint playerID, Color sparkColor)
		{
			if(GlobalVars.isServer)
			{
				GlobalVars.activeServer.PlayerList.TryGetValue(playerID, out Server.Player player);

				player.playerSpark = GameObject.Instantiate(mySparkPrefab);
				player.playerSpark.SetActive(false);
				player.sparkRenderer = player.playerSpark.GetComponent<ParticleSystemRenderer>();			
				player.sparkMaterial = new Material(player.sparkRenderer.material);
				player.sparkMaterial.color = sparkColor;
				player.sparkRenderer.material = new Material(player.sparkMaterial);
			}
			
			if(GlobalVars.isClient)
			{				
				GlobalVars.activeClient.playerList.TryGetValue(playerID, out Client.Player player);

				player.playerSpark = GameObject.Instantiate(mySparkPrefab);
				player.playerSpark.SetActive(false);
				player.sparkRenderer = player.playerSpark.GetComponent<ParticleSystemRenderer>();
				player.sparkMaterial = new Material(player.sparkRenderer.material);
				player.sparkMaterial.color = sparkColor;
				player.sparkRenderer.material = new Material(player.sparkMaterial);
			}
		}

		public static void SparkParticle(uint playerID, Vector3 sparkPosition)
		{
			if(GlobalVars.isServer && GlobalVars.displayInteractionIndicator)
			{
				GameObject newSpark = UnityEngine.GameObject.Instantiate(GlobalVars.activeServer.GetPlayer(playerID).playerSpark);
				newSpark.transform.position = sparkPosition;
				newSpark.SetActive(true);
			}

			if (GlobalVars.isClient && GlobalVars.displayInteractionIndicator)
			{
				GameObject newSpark = UnityEngine.GameObject.Instantiate(GlobalVars.activeClient.GetPlayer(playerID).playerSpark);
				newSpark.transform.position = sparkPosition;
				newSpark.SetActive(true);
			}
		}

		public static void DisplayMessage(string message, int time, MessageType messageType)
		{
			string colorString;

			if (messageType == MessageType.Info)
			{
				colorString = "<color=#00A600>"; // Green
			}
			else if (messageType == MessageType.Chat)
			{
				//colorString = "<color=#0000A6>"; // Blue
				colorString = "<color=#FFFFFF>"; // White
			}
			else if (messageType == MessageType.Error)
			{
				colorString = "<color=#A60000>";// Red
			}
			else // MessageType.System
			{
				colorString = "<color=#FFFFFF>"; // White
			}

			string messageReformat = colorString + message;

			

			if (runningRoutine != null)
			{
				MelonCoroutines.Stop(runningRoutine);
			}

			if (time > 0)
			{
				runningRoutine = MelonCoroutines.Start(MessageTime(time));
			}

			Activate();
			onScreenMessage.SetText(messageReformat);
		}

		public static void Deactivate()
		{
			bottomPanel.SetActive(false);
		}

		public static void Activate()
		{
			bottomPanel.SetActive(true);
		}
		public static IEnumerator MessageTime(int timer)
		{
			yield return new WaitForSeconds(timer);
			Deactivate();
		}

		public enum MessageType : byte
		{
			Info = 0,
			Error = 1,
			Chat = 2,
			System = 3
		}
	}
}
