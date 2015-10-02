﻿using System;
using System.Text.RegularExpressions;

namespace RoxorBot
{
    /// <summary>
    /// Contains logic to exctract duration from YouTube Duration Strings
    /// </summary>
    public static class TimeParser
    {
        /// <summary>
        /// The duration regex expression
        /// </summary>
        private static readonly string durationRegexExpression1 = @"(\d+)-(\d+)-(\d+)T(\d+):(\d+):(\d+)\+(\d+):(\d+)";
        private static readonly string durationRegexExpression2 = @"(\d+)-(\d+)-(\d+)T(\d+):(\d+):(\d+)Z";
        /// <summary>
        /// Gets the duration.
        /// </summary>
        /// <param name="durationStr">The duration string.</param>
        /// <returns>return ticks of the song</returns>
        public static DateTime GetDuration(string durationStr)
        {
            var m = Regex.Match(durationStr, durationRegexExpression1);
            if (!m.Success)
                m = Regex.Match(durationStr, durationRegexExpression2);
            if (!m.Success)
                return new DateTime(999, 12, 30);

            int year = int.Parse(m.Groups[1].Value);
            int month = int.Parse(m.Groups[2].Value);
            int day = int.Parse(m.Groups[3].Value);
            int hour = int.Parse(m.Groups[4].Value);
            int minute = int.Parse(m.Groups[5].Value);
            int second = int.Parse(m.Groups[6].Value);

            return new DateTime(year, month, day, hour, minute, second);
        }
    }
}
