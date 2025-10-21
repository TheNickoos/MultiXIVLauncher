using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MultiXIVLauncher
{
    public partial class SelectCharacterWindow : Window
    {
        public string SelectedCharacterPath { get; private set; }

        public SelectCharacterWindow()
        {
            InitializeComponent();
            LoadCharacters();

            OkButton.Click += (s, e) =>
            {
                var item = CharacterListBox.SelectedItem as ListBoxItem;
                if (item != null)
                {
                    SelectedCharacterPath = item.Tag.ToString();
                    DialogResult = true;
                }
            };

            CancelButton.Click += (s, e) => DialogResult = false;
        }

        private void LoadCharacters()
        {
            try
            {
                string baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Characters");
                string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");

                if (!File.Exists(configPath) || !Directory.Exists(baseDir))
                    return;

                var config = Config.Load(configPath);
                if (config == null || config.Characters == null || config.Characters.Count == 0)
                    return;

                
                foreach (var character in config.Characters)
                {
                    string charDir = Path.Combine(baseDir, "Character_" + character.Id);
                    if (Directory.Exists(charDir))
                    {
                        CharacterListBox.Items.Add(new ListBoxItem
                        {
                            Content = character.Name,
                            Tag = charDir
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{Properties.Resources.Unabletoloadcharacters}\n{ex.Message}",
                    Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
