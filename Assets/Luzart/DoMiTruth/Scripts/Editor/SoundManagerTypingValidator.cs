namespace Luzart
{
    using System;
    using System.IO;
    using System.Linq;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public static class SoundManagerTypingValidator
    {
        private const string MainScenePath = "Assets/Scenes/MainScene.unity";
        private const string BriefingPath = "Assets/Luzart/DoMiTruth/Scripts/UI/Screens/UIBriefing.cs";
        private const string DialoguePath = "Assets/Luzart/DoMiTruth/Scripts/UI/Screens/UINPCDialogue.cs";
        private const string TypingExtensionPath = "Assets/Luzart/DoMiTruth/Scripts/UI/TypingExtensions.cs";

        [MenuItem("DoMiTruth/Diagnostics/Validate Typing Sound")]
        public static void ValidateTypingSound()
        {
            EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);

            var soundManager = UnityEngine.Object.FindFirstObjectByType<SoundManager>();
            if (soundManager == null)
                throw new Exception("SoundManager not found in MainScene.");

            var serializedObject = new SerializedObject(soundManager);
            var soundConfigProp = serializedObject.FindProperty("soundConfig");
            var soundConfig = soundConfigProp != null ? soundConfigProp.objectReferenceValue as SoundConfigSO : null;
            if (soundConfig == null)
                throw new Exception("SoundManager.soundConfig is missing.");

            var typingEntry = soundConfig.entries != null
                ? soundConfig.entries.FirstOrDefault(entry => entry != null && entry.id == SoundId.SFX_Typing)
                : null;

            var typingClip = typingEntry != null
                ? typingEntry.clips?.FirstOrDefault(clip => clip != null)
                : soundConfig.GetLegacyClip(SoundId.SFX_Typing);

            if (typingClip == null)
                throw new Exception("No AudioClip configured for SFX_Typing.");

            AssertSourceDoesNotContain(BriefingPath, "SoundManager.Instance?.PlayTypingSFX();");
            AssertSourceDoesNotContain(DialoguePath, "SoundManager.Instance?.PlayTypingSFX();");
            AssertSourceDoesNotContain(TypingExtensionPath, "textInfo.characterCount");

            Debug.Log(
                $"[SoundManagerTypingValidator] OK | Scene={SceneManager.GetActiveScene().name} | " +
                $"Config={soundConfig.name} | TypingClip={typingClip.name} | " +
                $"TypingMinInterval={(typingEntry != null ? typingEntry.minIntervalBetweenPlays.ToString("0.###") : "legacy")}");
        }

        private static void AssertSourceDoesNotContain(string assetPath, string forbiddenText)
        {
            if (!File.Exists(assetPath))
                throw new Exception($"Missing source file: {assetPath}");

            var content = File.ReadAllText(assetPath);
            if (content.Contains(forbiddenText))
                throw new Exception($"Unexpected legacy typing-sound logic still found in {assetPath}");
        }
    }
}
