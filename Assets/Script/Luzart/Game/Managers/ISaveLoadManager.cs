using System;
using System.Collections.Generic;
using Luzart.Core;

namespace Luzart.Game.Managers
{
    /// <summary>
    /// Interface cho Save/Load system (chưa implement - dự phòng cho tương lai)
    /// </summary>
    public interface ISaveLoadManager
    {
        /// <summary>
        /// Lưu game
        /// </summary>
        void SaveGame(SaveData data);

        /// <summary>
        /// Load game
        /// </summary>
        SaveData LoadGame();

        /// <summary>
        /// Kiểm tra có save data không
        /// </summary>
        bool HasSaveData();

        /// <summary>
        /// Xóa save data
        /// </summary>
        void DeleteSaveData();
    }

    /// <summary>
    /// Cấu trúc dữ liệu save game
    /// </summary>
    [Serializable]
    public class SaveData
    {
        /// <summary>
        /// Danh sách ID của các clue đã thu thập
        /// </summary>
        public List<string> collectedClueIds = new List<string>();

        /// <summary>
        /// Danh sách ID của các map đã unlock
        /// </summary>
        public List<string> unlockedMapIds = new List<string>();

        /// <summary>
        /// Trạng thái hoàn thành của các room (RoomId -> IsCompleted)
        /// </summary>
        public Dictionary<string, bool> roomCompletionState = new Dictionary<string, bool>();

        /// <summary>
        /// Settings của game
        /// </summary>
        public GameSettings settings = new GameSettings();

        /// <summary>
        /// Game state hiện tại
        /// </summary>
        public GameState currentState = GameState.MainMenu;

        /// <summary>
        /// Timestamp khi save
        /// </summary>
        public long saveTimestamp;

        /// <summary>
        /// Version của save data
        /// </summary>
        public string saveVersion = "1.0";
    }

    /// <summary>
    /// Cấu trúc settings game
    /// </summary>
    [Serializable]
    public class GameSettings
    {
        public float musicVolume = 0.7f;
        public float sfxVolume = 0.8f;
        public UnityEngine.SystemLanguage language = UnityEngine.SystemLanguage.English;
        public bool enableHints = true;
    }
}
