namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums
{
    /// <summary>
    /// Jenis kejadian pada <c>TrxEmergencyDepartureEvent</c> yang bersifat tambah-saja.
    /// </summary>
    /// <remarks>
    /// <c>BE-IGD-033</c>, keputusan <c>IGD-DEC-090</c>. Kolom status pada
    /// <c>TrxEmergencyDeparture</c> adalah <b>turunan</b> dari kejadian terakhir yang
    /// berlaku, bukan sumber kebenaran tandingan.
    /// </remarks>
    public enum EmergencyDepartureEventType
    {
        Prepared = 1,
        Departed = 2,
        Arrived = 3,
        HandoverSubmitted = 4,
        HandoverAccepted = 5,
        HandoverRejected = 6,
        Cancelled = 9,

        /// <summary>
        /// Koreksi atas kejadian sebelumnya — <c>IGD-DEC-065</c>. Ditulis sebagai baris baru
        /// yang menunjuk baris lama lewat <c>SupersedesEventId</c>.
        /// </summary>
        Amended = 10,

        /// <summary>
        /// Pembalikan kejadian, menuntut persetujuan orang kedua — <c>IGD-DEC-066</c>.
        /// </summary>
        Reversed = 11
    }
}
