namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums
{
    public enum EmergencyVisitStatus
    {
        Arrived = 1,
        WaitingForTriage = 2,
        Triaged = 3,
        InTreatment = 4,
        UnderObservation = 5,
        AwaitingDisposition = 6,
        Disposed = 7,
        Cancelled = 8,

        /// <summary>
        /// Urusan pasien di IGD benar-benar tuntas, berbeda dari Disposed yang hanya
        /// berarti keputusan tindak lanjut sudah ditetapkan. Bernilai 9 supaya delapan
        /// nilai lama tidak bergeser dan data lama tetap terbaca dengan arti yang sama.
        /// </summary>
        Completed = 9
    }
}
