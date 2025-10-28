namespace MultiXIVLauncher.Models
{
    public class PresetItem
    {
        /// <summary>
        /// Display name of the preset item.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Returns the name of the preset item as its string representation.
        /// </summary>
        public override string ToString() => Name;
    }
}
