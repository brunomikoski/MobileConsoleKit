#if DebugLog
using System;
using System.IO;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;

namespace MobileConsole
{

	static class LogInitializeOnLoad
	{
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		static void OnInitializeOnLoad()
		{
			// Init share log
			EventBridge.OnShareLog += ShareLog;
			EventBridge.OnShareAllLog += ShareAllLogs;
			EventBridge.OnShareFiles += ShareFiles;
			EventBridge.AppVersionCode = NativeHelper.GetAppVersionCode();
			EventBridge.AppBundleIdentifier = BundleIdentifier.GetAppIdentifier();
			EventBridge.OnRequestAllocatedMemory += AllocatedMemory;
			EventBridge.OnRequestReservedMemory += ReservedMemory;
			EventBridge.OnRequestMonoUsedMemory += MonoUsedMemory;

			// Init log receiver
			SceneManager.sceneLoaded += OnSceneLoaded;
			LogReceiver.Init();
		}

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		static void LoadConsoleScene()
		{
            if (LogConsoleSettings.Instance.autoStartup)
            {
                SceneManager.LoadSceneAsync("LogConsole", LoadSceneMode.Additive);
            }
		}

		static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
		{
			if (scene.name == "LogConsole")
			{
				SceneManager.sceneLoaded -= OnSceneLoaded;
				SceneManager.UnloadSceneAsync(scene);
			}
		}

		static void ShareLog(string log)
		{
			ShareBridge.ShareText(log);
		}

		static void ShareAllLogs(string logs)
		{
			ShareBridge.ShareAllLogs(logs);
		}

        static void ShareFiles(string[] filePaths)
        {
	        ShareBridge.ShareFiles(filePaths);
        }

		static float AllocatedMemory()
		{
#if UNITY_5_6_OR_NEWER
			return Profiler.GetTotalAllocatedMemoryLong() / 1048576f;
#else
			return Profiler.GetTotalAllocatedMemory() / 1048576f;
#endif
		}

		static float ReservedMemory()
		{
#if UNITY_5_6_OR_NEWER
			return Profiler.GetTotalReservedMemoryLong() / 1048576f;
#else
			return Profiler.GetTotalReservedMemory() / 1048576f;
#endif
		}

		static float MonoUsedMemory()
		{
#if UNITY_5_6_OR_NEWER
			return Profiler.GetMonoUsedSizeLong() / 1048576f;
#else
			return Profiler.GetMonoUsedSize() / 1048576f;
#endif
		}
	}
}
#endif