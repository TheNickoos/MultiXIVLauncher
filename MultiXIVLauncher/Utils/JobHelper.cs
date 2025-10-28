using MultiXIVLauncher.Services;
using System;
using System.Collections.Generic;
using System.IO;

namespace MultiXIVLauncher.Utils
{
    /// <summary>
    /// Provides helper methods related to character jobs and class icon mapping from the Lodestone API.
    /// </summary>
    public static class JobHelper
    {
        /// <summary>
        /// Contains a mapping between Lodestone job icon filenames and their corresponding internal job names.
        /// </summary>
        public static readonly Dictionary<string, string> JobIconMap = new(StringComparer.OrdinalIgnoreCase)
        {
            // --- Base Classes ---
            { "ZpqEJWYHj9SvHGuV9cIyRNnIkk.png", "Archer" },
            { "VYP1LKTDpt8uJVvUT7OKrXNL9E.png", "Arcanist" },
            { "gl62VOTBJrm7D_BmAZITngUEM8.png", "Conjurer" },
            { "tYTpoSwFLuGYGDJMff8GEFuDQs.png", "Lancer" },
            { "St9rjDJB3xNKGYg-vwooZ4j6CM.png", "Marauder" },
            { "F5JzG9RPIKFSogtaKNBk455aYA.png", "Gladiator" },
            { "5CZEvDOMYMyVn2td9LZigsgw9s.png", "Pugilist" },
            { "IM3PoP6p06GqEyReygdhZNh7fU.png", "Thaumaturge" },

            // --- Jobs (Combatants) ---
            { "KWI-9P3RX_Ojjn_mwCS2N0-3TI.png", "Bard" },
            { "WdFey0jyHn9Nnt1Qnm-J3yTg5s.png", "Scholar" },
            { "4ghjpyyuNelzw1Bl0sM_PBA_FE.png", "Summoner" },
            { "i20QvSPcSQTybykLZDbQCgPwMw.png", "WhiteMage" },
            { "gX4OgBIHw68UcMU79P7LYCpldA.png", "Dragoon" },
            { "A3UhbjZvDeN3tf_6nJ85VP0RY0.png", "Warrior" },
            { "d0Tx-vhnsMYfYpGe9MvslemEfg.png", "Paladin" },
            { "HW6tKOg4SOJbL8Z20GnsAWNjjM.png", "Monk" },
            { "V01m8YRBYcIs5vgbRtpDiqltSE.png", "BlackMage" },
            { "s3MlLUKmRAHy0pH57PnFStHmIw.png", "RedMage" },
            { "KndG72XtCFwaq1I1iqwcmO_0zc.png", "Samurai" },
            { "erCgjnMSiab4LiHpWxVc-tXAqk.png", "Astrologian" },
            { "vmtbIlf6Uv8rVp2YFCWA25X0dc.png", "Machinist" },
            { "Fso5hanZVEEAaZ7OGWJsXpf3jw.png", "Ninja" },
            { "wdwVVcptybfgSruoh8R344y_GA.png", "Rogue" },

            // --- Disciples of the Hand (Crafters) ---
            { "bBVQ9IFeXqjEdpuIxmKvSkqalE.png", "Alchemist" },
            { "Rq5wcK3IPEaAB8N-T9l6tBPxCY.png", "Armorer" },
            { "EEHVV5cIPkOZ6v5ALaoN5XSVRU.png", "Blacksmith" },
            { "YCN6F-xiXf03Ts3pXoBihh2OBk.png", "Carpenter" },
            { "1kMI2v_KEVgo30RFvdFCyySkFo.png", "Culinarian" },
            { "LbEjgw0cwO_2gQSmhta9z03pjM.png", "Goldsmith" },
            { "ACAcQe3hWFxbWRVPqxKj_MzDiY.png", "Leatherworker" },
            { "E69jrsOMGFvFpCX87F5wqgT_Vo.png", "Weaver" },

            // --- Disciples of the Land (Gatherers) ---
            { "jGRnjIlwWridqM-mIPNew6bhHM.png", "Botanist" },
            { "B4Azydbn7Prubxt7OL9p1LZXZ0.png", "Fisher" },
            { "aM2Dd6Vo4HW_UGasK7tLuZ6fu4.png", "Miner" }
        };

        /// <summary>
        /// Retrieves the localized job name based on the Lodestone job icon URL.
        /// If the icon is unknown but not null, the event is logged for diagnostic purposes.
        /// </summary>
        /// <param name="iconUrl">The full URL of the job icon retrieved from the Lodestone API.</param>
        /// <returns>
        /// The localized job name if recognized, or a translated "Unknown" string if not found.
        /// </returns>
        public static string GetJobFromIconUrl(string? iconUrl)
        {
            // If no icon is provided, return a localized "Unknown" string without logging.
            if (string.IsNullOrWhiteSpace(iconUrl))
                return LanguageManager.T("Job_Unknown");

            string fileName = Path.GetFileName(iconUrl);

            // Try to match the icon filename to a known job.
            if (!JobIconMap.TryGetValue(fileName, out var jobKey))
            {
                // Log when a new or unexpected job icon is encountered.
                Logger.Warn($"[JobHelper] Unknown job icon detected: {fileName} (URL: {iconUrl})");
                return LanguageManager.T("Job_Unknown");
            }

            // Try to get the localized job name using LanguageManager.
            string localized = LanguageManager.T($"Job_{jobKey}");

            // Return the localized name if available, otherwise fall back to the English job key.
            return string.IsNullOrWhiteSpace(localized)
                ? jobKey
                : localized;
        }

        /// <summary>
        /// Determines whether the provided icon URL corresponds to a recognized job.
        /// </summary>
        /// <param name="iconUrl">The Lodestone job icon URL.</param>
        /// <returns><c>true</c> if the job is recognized, otherwise <c>false</c>.</returns>
        public static bool IsKnownJob(string? iconUrl)
        {
            if (string.IsNullOrWhiteSpace(iconUrl))
                return false;

            string fileName = Path.GetFileName(iconUrl);
            return JobIconMap.ContainsKey(fileName);
        }

        /// <summary>
        /// Retrieves the internal (non-localized) job key from an icon URL.
        /// </summary>
        /// <param name="iconUrl">The Lodestone job icon URL.</param>
        /// <returns>The English job key if found, or "Unknown" if not recognized.</returns>
        public static string GetJobKey(string? iconUrl)
        {
            if (string.IsNullOrWhiteSpace(iconUrl))
                return "Unknown";

            string fileName = Path.GetFileName(iconUrl);
            return JobIconMap.TryGetValue(fileName, out var jobKey) ? jobKey : "Unknown";
        }
    }
}
