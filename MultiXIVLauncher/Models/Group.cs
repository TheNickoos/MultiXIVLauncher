namespace MultiXIVLauncher.Models
{
    public class Group
    {
        /// <summary>
        /// Internal unique identifier of the group in the configuration.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Display name of the group.
        /// </summary>
        public string Name { get; set; }

        public Group()
        {
            Name = "";
        }
    }
}
