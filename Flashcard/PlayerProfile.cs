using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlashcardBattle
{
    public class PlayerProfile
    {
        public string Username { get; set; }
        public int StreakDays { get; set; }
        public DateTime LastLoginDate { get; set; }
        public int TotalBattles { get; set; }
        public int TotalWins { get; set; }
        public int TotalCardsReviewed { get; set; }
        public List<string> Achievements { get; set; }

        public PlayerProfile()
        {
            Achievements = new List<string>();
            LastLoginDate = DateTime.MinValue;
        }

        public void UpdateStreak()
        {
            DateTime today = DateTime.Today;

            if (LastLoginDate.Date == today)
            {     
                return;
            }
            else if (LastLoginDate.Date == today.AddDays(-1))
            {
                StreakDays++;
            }
            else
            {
                StreakDays = 1;
            }

            LastLoginDate = today;
        }
    }
}
