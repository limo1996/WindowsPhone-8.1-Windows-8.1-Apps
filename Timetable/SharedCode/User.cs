/*********************************************************
 * Copyright 2015, All rights reserved                   *
 * Author: Jakub Lichman                                 *
 * Sharing of code for purpose of learnig permissed      *
 *********************************************************/

using System;
using System.Collections.Generic;
using System.Text;

namespace Timetable
{
    /// <summary>
    /// represents User and his personal information used in app
    /// </summary>
    public class User : UserData
    {
        private static bool IsLogged = false;

        //currently logged user
        public static User Current
        {
            get
            {
                return current;
            }
            set 
            {
                if (value != null)
                {
                    current = value;
                    App.Current.Resources["user"] = value;
                    if (current != null)
                        IsLogged = true;
                    else
                        IsLogged = false;
                }
            }
        }
        private static User current = null;
    }
}
