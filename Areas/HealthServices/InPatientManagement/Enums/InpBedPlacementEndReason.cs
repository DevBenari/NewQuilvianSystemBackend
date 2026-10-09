namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums
{
    public enum InpBedPlacementEndReason
    {
        Transfer = 1,
        EpisodeClosed = 2,
        AdmissionCancelled = 3,
        PatientDeparted = 4,

        /// <summary>
        /// Baris ini digantikan koreksi salah catat (<c>BE-RWI-154</c>, <c>RWI-DEC-157</c>).
        /// Barisnya tetap tersimpan sebagai jejak, tetapi tidak ikut tarif kamar
        /// (<c>SupersededByCorrectionId</c> terisi, <c>INV-RWF-07</c>).
        /// </summary>
        CorrectedEntry = 5
    }
}
