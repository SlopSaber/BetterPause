using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using Zenject;

namespace BetterPause.Patches
{
	internal sealed class PauseMetadataTrace : IInitializable, ILateTickable
	{
		private readonly GameplayCoreSceneSetupData scene;
		private readonly PauseMenuManager menu;
		private readonly IGamePause pause;
		private readonly SetDataPatch patch;
		private bool observedPause;
		private int sampleFrame = -1;

		public PauseMetadataTrace(GameplayCoreSceneSetupData scene, PauseMenuManager menu, IGamePause pause, SetDataPatch patch)
		{
			this.scene = scene;
			this.menu = menu;
			this.pause = pause;
			this.patch = patch;
		}

		public void Initialize()
		{
			Capture("scene initialized");
		}

		public void LateTick()
		{
			if (!observedPause && pause.isPaused)
			{
				observedPause = true;
				sampleFrame = Time.frameCount + 2;
			}
			if (sampleFrame < 0 || Time.frameCount < sampleFrame) return;
			sampleFrame = -1;
			Capture("first pause");
		}

		private void Capture(string phase)
		{
			try
			{
				Plugin.Log.Info($"Pause metadata trace [{phase}]: frame={Time.frameCount}, enabled={PluginConfig.Instance.Enabled}, styleCalls={patch.StyleCalls}, setupDataCalls={patch.SetupDataCalls}, nativeSetupTask={patch.NativeSetupTask?.Status}.");
				ReportLevel("gameplay", scene.beatmapLevel);
				if (menu == null) return;
				var data = menu._initData;
				ReportLevel("pause InitData", data?.beatmapLevel);
				var didStart = typeof(MonoBehaviour).GetProperty("didStart", BindingFlags.Public | BindingFlags.Instance)?.GetValue(menu, null);
				Plugin.Log.Info($"Pause manager: path={Path(menu.transform)}, active={menu.gameObject.activeInHierarchy}, enabled={menu.enabled}, didStart={didStart}, showLevelBar={data?.showLevelBar}.");
				foreach (PauseMenuManager other in Resources.FindObjectsOfTypeAll<PauseMenuManager>())
					if (other != menu && other.gameObject.scene.IsValid() && other.gameObject.scene.isLoaded)
						Plugin.Log.Info($"Other pause manager: path={Path(other.transform)}, active={other.gameObject.activeInHierarchy}, enabled={other.enabled}, level='{Safe(other._initData?.beatmapLevel?.levelID)}'.");
				var bar = menu._levelBar;
				if (bar != null)
				{
					ReportLevel("native LevelBar", bar._beatmapLevel);
					Plugin.Log.Info($"Pause LevelBar: path={Path(bar.transform)}, active={bar.gameObject.activeInHierarchy}, title='{Safe(bar._songNameText?.text)}', author='{Safe(bar._authorNameText?.text)}', difficulty='{Safe(bar._difficultyText?.text)}'.");
				}
				var canvas = menu._continueButton != null ? menu._continueButton.GetComponentInParent<Canvas>(true) : null;
				if (canvas != null)
					foreach (TMP_Text text in canvas.GetComponentsInChildren<TMP_Text>(true))
						if (text.gameObject.activeInHierarchy)
							Plugin.Log.Info($"Pause label: path={Path(text.transform)}, enabled={text.enabled}, text='{Safe(text.text)}'.");
				ReportPatches(typeof(PauseMenuManager).GetMethod("Start", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
				ReportPatches(typeof(LevelBar).GetMethod("Setup", new[] { typeof(BeatmapLevel), typeof(BeatmapDifficulty), typeof(BeatmapCharacteristic) }));
				ReportPatches(typeof(LevelBar).GetMethod("SetupData", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
				ReportPatches(typeof(TMP_Text).GetProperty("text")?.GetSetMethod());
				foreach (Assembly assembly in new[] { typeof(SetDataPatch).Assembly, typeof(LevelBar).Assembly, typeof(BeatmapLevel).Assembly, typeof(SiraUtil.Affinity.IAffinity).Assembly }.Distinct())
					Plugin.Log.Info($"Pause assembly: {assembly.GetName().Name}, mvid={assembly.ManifestModule.ModuleVersionId}, path={assembly.Location}.");
			}
			catch (Exception error) { Plugin.Log.Warn($"Pause metadata trace unavailable: {error}"); }
		}

		private static void ReportLevel(string source, BeatmapLevel level)
		{
			Plugin.Log.Info(level == null ? $"Pause metadata [{source}]: null."
				: $"Pause metadata [{source}]: id='{Safe(level.levelID)}', title='{Safe(level.songName)}', subtitle='{Safe(level.songSubName)}', author='{Safe(level.songAuthorName)}'.");
		}

		private static void ReportPatches(MethodBase method)
		{
			if (method == null) return;
			Type harmony = AppDomain.CurrentDomain.GetAssemblies().Select(assembly => assembly.GetType("HarmonyLib.Harmony", false)).FirstOrDefault(type => type != null);
			object info = harmony?.GetMethod("GetPatchInfo", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new object[] { method });
			var owners = info?.GetType().GetProperty("Owners")?.GetValue(info, null) as IEnumerable;
			string names = owners == null ? "none" : string.Join(", ", owners.Cast<object>().Select(value => value.ToString()));
			Plugin.Log.Info($"Pause patch owners: {method.DeclaringType.FullName}.{method.Name}: {names}.");
		}

		private static string Path(Transform transform)
			=> transform.parent == null ? transform.name : Path(transform.parent) + "/" + transform.name;

		private static string Safe(string value)
			=> value == null ? "<null>" : value.Replace("\r", "\\r").Replace("\n", "\\n");
	}
}
