using MultiXIVLauncher.Models;
using NetStone;
using NetStone.Model;
using NetStone.Search;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace MultiXIVLauncher.Utils
{
    public static class LodestoneFetcher
    {
        

        public static async Task<bool> UpdateCharacterFromLodestoneAsync(Character character)
        {
            if (character == null || character.LodestoneId <= 0)
                return false;

            try
            {
                var lodestoneClient = await LodestoneClient.GetClientAsync();

                var lodestoneCharacter = await lodestoneClient.GetCharacter(character.LodestoneId.ToString());
                if (lodestoneCharacter == null)
                {
                    return false;
                }


                character.Name = lodestoneCharacter.Name;
                character.Server = lodestoneCharacter.Server;
                character.Level = lodestoneCharacter.ActiveClassJobLevel;
                string classIcon = lodestoneCharacter.ActiveClassJobIcon;
                character.Class = JobHelper.GetJobFromIconUrl(classIcon);

#pragma warning disable CS8602
                string portraitUrl = lodestoneCharacter.Avatar.ToString();
#pragma warning restore CS8602
                if (!string.IsNullOrEmpty(portraitUrl))
                {
                    string cacheDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cache", character.Id.ToString());
                    Directory.CreateDirectory(cacheDir);
                    string portraitPath = Path.Combine(cacheDir, "portrait.jpg");

                    using (var http = new System.Net.Http.HttpClient())
                    {
                        var bytes = await http.GetByteArrayAsync(portraitUrl);
                        File.WriteAllBytes(portraitPath, bytes);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LodestoneFetcher] Error: {ex.Message}");
                return false;
            }
        }
    }
}
