namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums
{
    /// <summary>
    /// Layanan yang dituju pasien ketika ia memakai kiosk.
    ///
    /// Sebelum ini, sesi kiosk hanya memindai identitas dan mencocokkannya ke pasien — ia tidak
    /// tahu pasien itu hendak ke mana. Petugas di setiap unit karena itu tidak dapat melihat
    /// siapa yang sedang menunggunya.
    ///
    /// Ditambahkan atas persetujuan lintas modul <c>LAB-REQ-006</c>, 2026-09-15.
    ///
    /// <b>Sengaja pendek.</b> Hanya layanan yang benar-benar sudah punya alur di kiosk yang
    /// dicantumkan. Menambahkan nilai untuk unit yang belum punya layarnya akan menawarkan
    /// pilihan yang tidak menuju ke mana-mana.
    /// </summary>
    public enum KioskServiceTarget
    {
        /// <summary>Belum dinyatakan — termasuk seluruh sesi yang dibuat sebelum ruas ini ada.</summary>
        Unknown = 0,

        /// <summary>Poliklinik rawat jalan; alur yang sudah berjalan lewat jadwal dokter.</summary>
        OutpatientClinic = 1,

        /// <summary>Laboratorium.</summary>
        Laboratory = 2,

        /// <summary>Layanan lain yang belum punya alur tersendiri di kiosk.</summary>
        Other = 99
    }
}
