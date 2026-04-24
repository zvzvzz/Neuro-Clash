using System;
using System.Collections.Generic;
using System.Linq;

namespace FlashcardBattle
{
    public class Battle
    {
        public Deck Deck { get; private set; }
        public Mode Mode { get; private set; }
        public int PlayerScore { get; private set; }
        public int BotTargetScore { get; private set; }
        public int CurrentIndex { get; private set; }
        public int TotalCards { get; private set; }
        public int CorrectCount { get; private set; }
        public int WrongCount { get; private set; }
        public int Lives { get; private set; }
        public bool IsGameOver => Lives <= 0 || PlayerWins();

        private List<Flashcard> shuffledCards;

        public Battle(Deck deck, Mode mode)
        {
            Deck = deck;
            Mode = mode;
            PlayerScore = 0;
            CurrentIndex = 0;
            CorrectCount = 0;
            WrongCount = 0;
            Lives = 3;

            shuffledCards = deck.Cards.OrderBy(c => Guid.NewGuid()).ToList();
            TotalCards = shuffledCards.Count;

            BotTargetScore = mode switch
            {
                Mode.Easy => (int)(TotalCards * 10 * 1.5),
                Mode.Medium => (int)(TotalCards * 10 * 2.0),
                Mode.Hard => (int)(TotalCards * 10 * 2.5),
                Mode.Infinite => 0,
                _ => 0
            };
        }

        public Flashcard GetCurrentCard()
        {
            if (IsGameOver) return null;
            return shuffledCards[CurrentIndex];
        }

        public bool SubmitAnswer(string playerAnswer)
        {
            if (IsGameOver) return false;

            bool correct = playerAnswer.Trim().Equals(
                shuffledCards[CurrentIndex].Answer.Trim(),
                StringComparison.OrdinalIgnoreCase);

            if (correct)
            {
                PlayerScore += 10;
                CorrectCount++;
            }
            else
            {
                Lives--;
                WrongCount++;
            }

            CurrentIndex++;

            if (CurrentIndex >= TotalCards)
            {
                shuffledCards = shuffledCards.OrderBy(c => Guid.NewGuid()).ToList();
                CurrentIndex = 0;
            }

            return correct;
        }

        public bool PlayerWins()
        {
            return PlayerScore >= BotTargetScore;
        }

        public double GetAccuracy()
        {
            int total = CorrectCount + WrongCount;
            if (total == 0) return 0;
            return Math.Round((double)CorrectCount / total * 100, 1);
        }
    }
}