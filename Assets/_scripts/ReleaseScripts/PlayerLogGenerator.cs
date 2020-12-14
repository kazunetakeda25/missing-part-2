using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using Newtonsoft.Json;

namespace MissingComplete
{
    public static class PlayerLogGenerator
    {
        private const string LOG_PATH = "Logs";
        private const string PLAYER_LOG_EXTENSTION = ".log";

        public static void SavePlayerLog(SaveGameManager.SaveGame save)
        {
            string newLine = Environment.NewLine;

            string outputString = "Player Log: " + save.profileName + newLine;
            outputString += "Progress Complete: " + save.GetPercentageComplete() + newLine;

            if (save.playTime > 5.0f)
            {
                TimeSpan time = TimeSpan.FromSeconds(save.playTime);
                string timePlayed = string.Format("{0:D2}h:{1:D2}m:{2:D2}s", time.Hours, time.Minutes, time.Seconds);
                outputString += "Time Played: " + timePlayed + newLine;
            }

            if (save.gameCompleted == true)
            {
                Debug.Log(save.dateCompleted);
                outputString += "Date Completed: " + save.dateCompleted.Month + " / " + save.dateCompleted.Day + " / " + save.dateCompleted.Year + newLine;
            }

            outputString += "=================================" + newLine;

            SessionManager.SerializedGameData gameData = JsonConvert.DeserializeObject<SessionManager.SerializedGameData>(save.sessionDataJSON);

            outputString += "Player Score: " + gameData.playerScore + newLine;

            List<BadgeTracker.BadgeScore> badgeScores = JsonConvert.DeserializeObject<List<BadgeTracker.BadgeScore>>(save.badgeTrackerJSON);
            for (int i = 0; i < badgeScores.Count; i++)
            {
                if (i == 0)
                {
                    continue;
                }

                if (badgeScores[i].totalAnswers == 0)
                {
                    continue;
                }

                outputString += "Badge " + badgeScores[i].badge.ToString() + newLine;
                outputString += "Player has earned " + BadgeTracker.GetMedalString(badgeScores[i]).ToString() + " medal." + newLine;
                outputString += "Correct Answers for this badge: " + badgeScores[i].correctAnswers + newLine;
            }

            if (Directory.Exists(Application.persistentDataPath + "/" + LOG_PATH) == false)
            {
                Directory.CreateDirectory(Application.persistentDataPath + "/" + LOG_PATH);
            }

            string filePath = Application.persistentDataPath + "/" + LOG_PATH + "/" + save.profileName + PLAYER_LOG_EXTENSTION;

            File.WriteAllText(filePath, outputString);
        }
    }
}
