using System;

public static class TimeUtil
{
    public static long NowUtc(bool isLocalize = false)
    {
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    // ms -> ��,��,��,�� ���ڿ��� ����
    public static string ConvertToDHMS(long time)
    {
        var timeSpan = TimeSpan.FromMilliseconds(time);
        return $"{timeSpan.Days:00}d:{timeSpan.Hours:00}h:{timeSpan.Minutes:00}:m{timeSpan.Seconds:00}s";
    }
}
