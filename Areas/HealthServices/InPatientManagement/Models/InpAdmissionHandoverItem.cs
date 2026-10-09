using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models
{
    /// <summary>
    /// Satu baris Ceklist Serah Terima Pasien Baru dengan kode dan nama butir yang dibekukan —
    /// kamus data 20.5 (<c>BE-RWI-192</c>, <c>RWI-DEC-241</c> butir 4).
    /// </summary>
    /// <remarks>
    /// Butir dibentuk saat simpan pertama dari master jenis serah terima. Contoh: admin mengganti
    /// nama butir 14 menjadi "Input Kartu Parkir" pada 1 November; serah terima Tn. Budi tanggal
    /// 7 Oktober tetap tercetak "INPUT PARKIR". Saran sistem tidak disimpan; ia dihitung saat dibaca.
    /// </remarks>
    public class InpAdmissionHandoverItem : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid DocumentId { get; set; }

        /// <summary>Butir master asal, jenis serah terima.</summary>
        public Guid ClearanceItemId { get; set; }

        /// <summary>
        /// Urutan baris pada lembar (1..n), dibekukan saat dibuat — urutan bisnis cetakan, bukan
        /// urutan tampilan generik.
        /// </summary>
        public int LineNo { get; set; }

        /// <summary>Nomor butir induk tercetak 1..15; kosong untuk sub-butir.</summary>
        public int? ItemNumberSnapshot { get; set; }

        /// <summary>Nomor induk sub-butir, misalnya <c>2</c>.</summary>
        public int? ParentItemNumberSnapshot { get; set; }

        /// <summary>Kode butir saat dibuat, misalnya <c>STPB-13</c>.</summary>
        public string ItemCodeSnapshot { get; set; } = string.Empty;

        /// <summary>Nama butir saat dibuat, misalnya "PASANG GELANG".</summary>
        public string ItemNameSnapshot { get; set; } = string.Empty;

        /// <summary>Kosong = belum dipilih; wajib terisi saat kunci.</summary>
        public InpHandoverItemChoice? Choice { get; set; }

        /// <summary>Keterangan; wajib bila Belum saat kunci.</summary>
        public string? Note { get; set; }

        public InpAdmissionDocument? Document { get; set; }

        public MstInpatientClearanceItem? ClearanceItem { get; set; }
    }
}
