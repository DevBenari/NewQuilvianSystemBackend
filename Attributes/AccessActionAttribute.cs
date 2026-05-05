namespace QuilvianSystemBackend.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class AccessActionAttribute : Attribute
    {
        public AccessActionAttribute(
            string actionName,
            string displayName)
        {
            ActionName = actionName;
            DisplayName = displayName;
        }

        public string ActionName { get; }

        public string DisplayName { get; }

        public string? Description { get; set; }

        public int SortOrder { get; set; } = 0;
    }
}
