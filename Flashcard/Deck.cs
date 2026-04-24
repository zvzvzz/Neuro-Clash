using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlashcardBattle
{
    public class Deck
    {
        public string Name { get; set; }
        public string Subject { get; set; }
        public int TimesPlayed { get; set; }
        public string BestRank { get; set; }
        public List<Flashcard> Cards { get; set; }
        



        public Deck(string name, string subject)
        {
            Name = name;
            Subject = subject;
            TimesPlayed = 0;
            BestRank = "None";
            Cards = new List<Flashcard>();
        }
        public Deck() { Cards = new List<Flashcard>(); }

        public void AddCard(Flashcard card)
        {
            if (card != null)
            {
                Cards.Add(card);    
            }
        }

        public void DeleteCard(int index)
        {
            if (index >= 0 && index < Cards.Count)
            {
                Cards.RemoveAt(index);
            }
        }

        public void EditCard(int index, string newQuestion, string newAnswer)
        {
            if (index >= 0 && index < Cards.Count)
            {
                Cards[index].Question = newQuestion;
                Cards[index].Answer = newAnswer;
            }
        }






    }
}
