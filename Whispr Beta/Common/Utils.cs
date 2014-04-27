using System;

namespace WhisprBeta.Common
{
    class Utils
    {
        private static DateTime _epochDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);

        public static long DateTimeToUnixTimestamp(DateTime dateTime)
        {
            return (long)(dateTime - new DateTime(1970, 1, 1).ToLocalTime()).TotalSeconds;
        }
        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            DateTime resultDateTime = _epochDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return resultDateTime;
        }
        public static long UnixTimeNow()
        {
            return DateTimeToUnixTimestamp(DateTime.Now);
        }
        public static double ToRadians(double degrees)
        {
            return degrees * (Math.PI / 180);
        }
        public static string ToTimeSinceString(DateTime pubDate)
        {
            string timeSinceString;
            if (pubDate > DateTime.Now) {
                timeSinceString = "Right now";
            } else {
                TimeSpan timeSince = DateTime.Now - pubDate;
                if ((int)timeSince.TotalSeconds < 5) {
                    timeSinceString = "Right now";
                } else if ((int)timeSince.TotalMinutes < 1) {
                    timeSinceString = (int)timeSince.TotalSeconds + " seconds ago";
                } else if ((int)timeSince.TotalHours < 1) {
                    if ((int)timeSince.TotalMinutes > 1) {
                        timeSinceString = (int)timeSince.TotalMinutes + " minutes ago";
                    } else {
                        timeSinceString = (int)timeSince.TotalMinutes + " minute ago";
                    }
                } else if ((int)timeSince.TotalDays < 1) {
                    if ((int)timeSince.TotalHours > 1) {
                        timeSinceString = (int)timeSince.TotalHours + " hours ago";
                    } else {
                        timeSinceString = (int)timeSince.TotalHours + " hour ago";
                    }
                } else if ((int)timeSince.TotalDays < 30) {
                    if ((int)timeSince.TotalDays > 1) {
                        timeSinceString = (int)timeSince.TotalDays + " days ago";
                    } else {
                        timeSinceString = (int)timeSince.TotalDays + " day ago";
                    }
                } else if ((int)timeSince.TotalDays < 2 * 30) {
                    timeSinceString = "a month ago";
                } else if ((int)timeSince.TotalDays < 3 * 30) {
                    timeSinceString = "2 months ago";
                } else if ((int)timeSince.TotalDays < 4 * 30) {
                    timeSinceString = "3 months ago";
                } else if ((int)timeSince.TotalDays < 5 * 30) {
                    timeSinceString = "4 months ago";
                } else if ((int)timeSince.TotalDays < 6 * 30) {
                    timeSinceString = "5 months ago";
                } else if ((int)timeSince.TotalDays < 365) {
                    timeSinceString = "half a year ago";
                } else if ((int)timeSince.TotalDays < 2 * 365) {
                    timeSinceString = "a year ago";
                } else if ((int)timeSince.TotalDays < 3 * 365) {
                    timeSinceString = "2 years ago";
                } else if ((int)timeSince.TotalDays < 4 * 365) {
                    timeSinceString = "3 years ago";
                } else if ((int)timeSince.TotalDays < 5 * 365) {
                    timeSinceString = "4 years ago";
                } else {
                    timeSinceString = "over 5 years ago";
                }
            }
            return timeSinceString;
        }
    }
}
