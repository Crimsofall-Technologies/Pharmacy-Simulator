using UnityEngine;

public static class StringSimplifier
{
    public static string GetSortedTimeFromSeconds(int time)
	{
		int totalSec = Mathf.FloorToInt((float)time);
		int hours = totalSec / 3600;
		int minutes = (totalSec % 3600) / 60;
		int seconds = totalSec % 60;

		string result = "";

		if (hours > 0) result += $"{hours}h";
		if (minutes > 0 || hours > 0) result += $" {minutes}m";
		result += $" {seconds}s";

		//returns in format: 1h 2m 5s
		return result.Trim();
	}
}
