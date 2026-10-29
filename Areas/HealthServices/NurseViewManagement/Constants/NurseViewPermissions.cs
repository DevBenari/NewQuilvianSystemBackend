namespace QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Constants
{
    public static class NurseViewPermissions
    {
        public const string ModuleCode = "HEALTH_SERVICE_NURSE_VIEW";
        public const string ModuleName = "Health Service Nurse View";
        public const string AreaName = "HealthServices";

        public const string Read = "Read";
        public const string Create = "Create";
        public const string Update = "Update";

        public static class DisplayDevice
        {
            public const string Resource = "NurseViewDisplayDevice";
        }

        public static class VitalSignInterval
        {
            public const string Resource = "NurseViewVitalSignInterval";
        }
    }
}
