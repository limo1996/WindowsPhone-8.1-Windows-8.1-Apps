/*********************************************************
 * Copyright 2015, All rights reserved                   *
 * Author: Jakub Lichman                                 *
 * Sharing of code for purpose of learnig permissed      *
 *********************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Timetable
{
    /// <summary>
    /// used in Windows Phone app to remember settings about line searching that user made on the other page
    /// </summary>
    class SearchingSetttings
    {
        public bool OnlyDirectConnections { get; set; }
        public CRIT Criterium { get; set; }
        public int ResultsCount { get; set; }
        public DateTime StartingTime { get; set; }
    }
}
