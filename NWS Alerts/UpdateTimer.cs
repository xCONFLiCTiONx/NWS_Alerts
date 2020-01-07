using System;

namespace NWS_Alerts
{
    public class UpdateTimer
    {
        public static bool ReturnDateTime(DateTime date1, DateTime date2)
        {
            int result = DateTime.Compare(date1, date2);

            if (result < 0)
                return false;
            else
                return true;
        }
    }
}
