using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlashcardBattle
{
    public class Flashcard
    {
        public string Question { get; set; }
        public string Answer { get; set; }
        public int TimesCorrect { get; set; }
        public int TimesWrong { get; set; }

        public Flashcard(string question, string answer)
        {
            Question = question;
            Answer = answer;
        }

        public Flashcard() { }


    }
}
