using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlashcardBattle
{
    public class Powerup
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public int Cost { get; set; }

        public Powerup(string name, string description, int cost)
        {
            Name = name;
            Description = description;
            Cost = cost;
            Quantity = 0;
        }
    }
}
