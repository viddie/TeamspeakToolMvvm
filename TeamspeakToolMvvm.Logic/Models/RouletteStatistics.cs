using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamspeakToolMvvm.Logic.Models {
    public class RouletteStatistics {

        public int GamesWon { get; set; } = 0;
        public int GamesLost { get; set; } = 0;
        public int PointsWon { get; set; } = 0;
        public int PointsLost { get; set; } = 0;
        public int Jackpots { get; set; } = 0;
        public int JackpotsPointsWon { get; set; } = 0;
        public double Rolls { get; set; } = 0;

    }
}
