using System;
using System.Collections.Generic;
using System.Text;
using TimeZoneConverter;

public static class DateTimeBr
{

    private const string TIME_ZONE_NAME = "E. South America Standard Time";

    public static DateTime NowBr()
    {
        var dateTime = DateTime.UtcNow;
        var horaBrasilia = TZConvert.GetTimeZoneInfo(TIME_ZONE_NAME);
        return TimeZoneInfo.ConvertTimeFromUtc(dateTime, horaBrasilia);
    }
}

