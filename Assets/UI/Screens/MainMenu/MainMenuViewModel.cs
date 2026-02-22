namespace Luzart.UIFramework.Examples
{
    public class MainMenuViewModel : UIViewModel
    {
        private string playerName;
        private int playerLevel;

        public string PlayerName
        {
            get => playerName;
            set
            {
                if (playerName != value)
                {
                    playerName = value;
                    NotifyDataChanged();
                }
            }
        }

        public int PlayerLevel
        {
            get => playerLevel;
            set
            {
                if (playerLevel != value)
                {
                    playerLevel = value;
                    NotifyDataChanged();
                }
            }
        }

        public override void Reset()
        {
            base.Reset();
            playerName = string.Empty;
            playerLevel = 0;
        }
    }
}
