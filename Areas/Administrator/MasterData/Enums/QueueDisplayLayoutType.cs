namespace QuilvianSystemBackend.Areas.Administrator.MasterData.Enums
{
    public enum QueueDisplayLayoutType
    {
        Unknown = 0,
        ClusterBoard = 1,
        ClinicBoard = 2,
        DoctorBoard = 3,
        CallingBoard = 4,
        SummaryBoard = 5,

        /// <summary>
        /// Display Antrean Admisi Rawat Jalan (rawat-jalan Cetak Antrean Admisi v1.1). Tidak terikat
        /// Nurse Station Cluster; login perangkat diarahkan ke /queue-display/admission.
        /// </summary>
        AdmissionBoard = 6
    }

}
