using System;

namespace Demo365.Contracts
{
    public class Game
    {
        /// <summary>
        /// Unique ID (func of):
        ///     - sport
        ///     - competition
        ///     - teams[]
        ///     - time (2h range)
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// completion time
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// sport name (= sport ID)
        /// </summary>
        public string Sport { get; set; }

        /// <summary>
        /// competition name ( = competition ID)
        /// </summary>
        public string Competition { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public GameTeam[] Teams { get; set; }
    }
}
