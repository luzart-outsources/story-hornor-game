using UIFramework.Communication;

namespace Luzart.Game.Events
{
    /// <summary>
    /// Event khi cutscene hoàn thành
    /// </summary>
    public class CutsceneCompletedEvent : IUIEvent
    {
        public string CutsceneId { get; set; }
    }

    /// <summary>
    /// Event khi briefing (hội thoại giao nhiệm vụ) hoàn thành
    /// </summary>
    public class BriefingCompletedEvent : IUIEvent
    {
        public string BriefingId { get; set; }
    }

    /// <summary>
    /// Event khi người chơi chọn một map
    /// </summary>
    public class MapSelectedEvent : IUIEvent
    {
        public string MapId { get; set; }
    }

    /// <summary>
    /// Event khi hoàn thành điều tra một phòng
    /// </summary>
    public class RoomCompletedEvent : IUIEvent
    {
        public string RoomId { get; set; }
        public int CluesCollected { get; set; }
        public int TotalClues { get; set; }
    }

    /// <summary>
    /// Event khi thu thập được một clue
    /// </summary>
    public class ClueCollectedEvent : IUIEvent
    {
        public string ClueId { get; set; }
        public string ClueName { get; set; }
    }

    /// <summary>
    /// Event khi unlock được một room
    /// </summary>
    public class RoomUnlockedEvent : IUIEvent
    {
        public string RoomId { get; set; }
        public string MapId { get; set; }
    }

    /// <summary>
    /// Event khi game state thay đổi
    /// </summary>
    public class GameStateChangedEvent : IUIEvent
    {
        public Core.GameState PreviousState { get; set; }
        public Core.GameState NewState { get; set; }
    }

    /// <summary>
    /// Event khi dialogue sequence bắt đầu
    /// </summary>
    public class DialogueStartedEvent : IUIEvent
    {
        public string SequenceId { get; set; }
    }

    /// <summary>
    /// Event khi dialogue sequence kết thúc
    /// </summary>
    public class DialogueCompletedEvent : IUIEvent
    {
        public string SequenceId { get; set; }
    }

    /// <summary>
    /// Event khi gặp NPC
    /// </summary>
    public class NPCEncounteredEvent : IUIEvent
    {
        public string NPCId { get; set; }
        public string NPCName { get; set; }
    }

    /// <summary>
    /// Event khi player click vào một object
    /// </summary>
    public class ObjectClickedEvent : IUIEvent
    {
        public string ObjectId { get; set; }
    }

    /// <summary>
    /// Event khi game settings thay đổi
    /// </summary>
    public class SettingsChangedEvent : IUIEvent
    {
        public float MusicVolume { get; set; }
        public float SFXVolume { get; set; }
        public UnityEngine.SystemLanguage Language { get; set; }
    }
}
