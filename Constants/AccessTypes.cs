namespace QuilvianSystemBackend.Constants
{
    public static class AccessTypes
    {
        public const string Read = "Read";
        public const string Create = "Create";
        public const string Update = "Update";
        public const string Delete = "Delete";

        public static readonly string[] AllowedForRoleAccess =
        {
            Read,
            Create,
            Update,
            Delete
        };
    }
}