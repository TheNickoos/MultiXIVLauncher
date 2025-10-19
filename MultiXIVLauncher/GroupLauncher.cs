using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MultiXIVLauncher
{
    public static class GroupLauncher
    {
        public static async Task LaunchGroupAsync(int groupId, Config config, MainWindow mainWindow)
        {
            try
            {
                string groupName;
                var characters = new List<Character>();

                if (groupId == -1)
                {
                    groupName = "All";
                    characters = (config.Characters ?? new List<Character>()).ToList();
                }
                else
                {
                    var group = config.Groups.FirstOrDefault(g => g.Id == groupId);
                    if (group == null)
                    {
                        MessageBox.Show("The selected group could not be found.",
                                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    groupName = group.Name;
                    characters = (config.Characters ?? new List<Character>())
                                 .Where(c => c.GroupIds != null && c.GroupIds.Contains(groupId))
                                 .ToList();
                }

                if (characters.Count == 0)
                {
                    MessageBox.Show(groupId == -1 ? "No characters to launch."
                                                  : $"No characters are assigned to group '{groupName}'.",
                                    "Nothing to launch", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                mainWindow.SetLauncherInteractivity(false);

                var loadingWindow = new GroupLoadingWindow { Owner = mainWindow };
                loadingWindow.Show();

                int index = 1;
                foreach (var character in characters)
                {
                    loadingWindow.UpdateProgress(groupName, character.Name, index, characters.Count);

                    CharacterLauncher.LaunchCharacter(character, config, mainWindow);

                    await WaitForFFXIVToStartAsync();
                    await Task.Delay(4000);
                    index++;
                }

                loadingWindow.Close();

                MessageBox.Show(groupId == -1
                    ? "All characters launched!"
                    : $"Group '{groupName}' launched successfully!",
                    "Group Launch", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while launching group: {ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                mainWindow.SetLauncherInteractivity(true);
            }
        }


        private static async Task WaitForFFXIVToStartAsync()
        {
            var existing = Process.GetProcessesByName("ffxiv_dx11")
                                  .Concat(Process.GetProcessesByName("ffxiv"))
                                  .Select(p => p.Id)
                                  .ToList();

            DateTime start = DateTime.Now;
            int timeout = 90;

            while (true)
            {
                var current = Process.GetProcessesByName("ffxiv_dx11")
                                     .Concat(Process.GetProcessesByName("ffxiv"))
                                     .Where(p => !existing.Contains(p.Id))
                                     .ToList();

                if (current.Any()) break;
                if ((DateTime.Now - start).TotalSeconds > timeout) break;

                await Task.Delay(2000);
            }
        }
    }
}
