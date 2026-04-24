using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;

namespace FlashcardBattle
{
    public class DataManager
    {
        private string filePath = "playerdata.json";
        private string deckFilePath = "decks.json";
            
        public void SaveProfile(PlayerProfile profile)
        {
            string json = JsonSerializer.Serialize(profile);
            File.WriteAllText(filePath, json);
        }
        public PlayerProfile LoadProfile()
        {
            if (!File.Exists(filePath))
            {
                return null;
            }
            
            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<PlayerProfile>(json);
        }

        public void SaveDecks(List<Deck> decks)
        {
            string json = JsonSerializer.Serialize(decks);
            File.WriteAllText(deckFilePath, json);
        }

        public List<Deck> LoadDecks()
        {
            if (!File.Exists(deckFilePath))
                return new List<Deck>();

            string json = File.ReadAllText(deckFilePath);
            return JsonSerializer.Deserialize<List<Deck>>(json);
        }

    }


}
