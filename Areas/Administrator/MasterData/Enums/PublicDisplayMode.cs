namespace QuilvianSystemBackend.Areas.Administrator.MasterData.Enums
{
    /// <summary>
    /// Identitas pasien yang boleh tampil/terucap di layar antrean publik (RJ-DOC-DEC-057).
    /// Tidak berlaku untuk ruang kerja perawat/dokter yang login.
    /// </summary>
    public enum PublicDisplayMode
    {
        /// <summary>Mengikuti saklar perangkat display (perilaku lama).</summary>
        Default = 1,

        /// <summary>Nama lengkap boleh tampil bila perangkat mengizinkan.</summary>
        FullName = 2,

        /// <summary>Hanya nama samaran; suara tidak menyebut nama.</summary>
        MaskedName = 3,

        /// <summary>Hanya nomor antrean dan tujuan; tanpa identitas pasien.</summary>
        QueueNumberOnly = 4
    }
}
