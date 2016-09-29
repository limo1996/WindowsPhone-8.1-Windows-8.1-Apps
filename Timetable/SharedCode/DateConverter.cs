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
using Windows.Globalization.DateTimeFormatting;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Timetable
{

    /// <summary>
    /// converts date time to be nicely displayed in the icons of the split page
    /// </summary>
    public class DateConverter : IValueConverter
    {
        //converts given type so it can be nicely displayed in dateTime template
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
            {
                throw new ArgumentNullException("Value can not be null");
            }
            if (!(value is string))
            {
                throw new ArgumentException("Value must be of type: DateTime", "value");
            }
            DateTime dateTime = DateTime.Parse((string)value);

            if (parameter == null)
            {
                try
                {
                    // Date "7/27/2011 9:30:59 AM" returns "7/27/2011"
                    return DateTimeFormatter.ShortDate.Format(dateTime);
                }
                catch
                {
                    return DateTimeFormatter.ShortDate.Format(DateTime.Now);
                }
            }
            else if ((string)parameter == "day")
            {
                // Date "7/27/2011 9:30:59 AM" returns "27"
                DateTimeFormatter dateFormater = new DateTimeFormatter("{day.integer(2)}");
                return dateFormater.Format(dateTime);
            }
            else if ((string)parameter == "month")
            {
                // Date "7/27/2011 9:30:59 AM" returns "JUL"
                DateTimeFormatter dateFormater = new DateTimeFormatter("{month.abbreviated(3)}");
                return dateFormater.Format(dateTime).ToUpper();
            }
            else if ((string)parameter == "hour")
            {
                // Date "7/27/2011 9:30:59 AM" returns "2011"
                return string.Format("{0:00}",dateTime.Hour);
            }
            else if ((string)parameter == "minute")
            {
                return string.Format("{0:00}", dateTime.Minute);
            }
            else
            {
                return dateTime.ToString();
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            string strValue = value as string;
            DateTime resultDateTime;
            if (DateTime.TryParse(strValue, out resultDateTime))
            {
                return resultDateTime;
            }
            else
            {
                return DependencyProperty.UnsetValue;
            }

        }
    }
}
