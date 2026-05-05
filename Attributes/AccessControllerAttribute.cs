namespace QuilvianSystemBackend.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AccessControllerAttribute : Attribute
    {
        public AccessControllerAttribute(
            string moduleCode,
            string moduleName,
            string displayName)
        {
            ModuleCode = moduleCode;
            ModuleName = moduleName;
            DisplayName = displayName;
        }

        public string ModuleCode { get; }

        public string ModuleName { get; }

        public string DisplayName { get; }

        public string? AreaName { get; set; }

        public string? ControllerName { get; set; }

        public string? Description { get; set; }

        public int SortOrder { get; set; } = 0;
    }
}
