using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs
{
    public class CreateLabOrderRequest
    {
        [Required]
        public Guid EncounterId { get; set; }

        [Required]
        public Guid ProcedureId { get; set; }

        /// <summary>
        /// Disiplin yang menaungi pesanan — Patologi Klinik, Patologi Anatomi, atau
        /// Mikrobiologi (<c>LAB-DEC-025</c>).
        ///
        /// Sengaja tidak wajib. `LAB-API-v1` r3 mengunci `POST /lab-orders` tetap berlaku apa
        /// adanya, sehingga pemanggil lama yang belum mengirim ruas ini tidak boleh mendadak
        /// ditolak. Mewajibkannya adalah perubahan kontrak tersendiri.
        /// </summary>
        public LabDiscipline? Discipline { get; set; }

        /// <summary>
        /// Perawatan rawat inap yang menaungi pesanan. Boleh kosong; bila terisi tetapi tidak
        /// cocok dengan perawatan milik kunjungannya, permintaan ditolak <c>400</c>.
        /// </summary>
        /// <remarks>
        /// <c>BE-RWI-052</c>, <c>VAL-DOK-22</c>, <c>INV-DOK-12</c>. Inilah yang membuat pesanan
        /// perawatan A tidak dapat diproses sebagai milik perawatan B. Tanpa penanda ini, satu
        /// pasien yang dirawat dua kali dalam sebulan memiliki dua rangkaian pesanan yang
        /// bercampur pada layar dokter.
        /// </remarks>
        public Guid? InpEpisodeId { get; set; }
    }

    public class LabOrderListResponse
    {
        public Guid Id { get; set; }

        public Guid EncounterId { get; set; }

        /// <summary>Perawatan rawat inap yang menaungi pesanan, bila ada.</summary>
        public Guid? InpEpisodeId { get; set; }

        /// <summary>
        /// Benar ketika hasil pemeriksaan sudah final dan sah dipakai sebagai dasar keputusan
        /// klinis.
        /// </summary>
        /// <remarks>
        /// <c>BE-RWI-052</c>, <c>VAL-DOK-30</c>. Hasil yang belum final <b>wajib</b> ditandai
        /// dan tidak boleh disajikan sebagai hasil sah. Hasil basi di layar dokter adalah risiko
        /// keselamatan, bukan masalah tampilan.
        /// </remarks>
        public bool IsResultFinal { get; set; }

        /// <summary>Keterangan singkat ketersediaan hasil, siap ditampilkan apa adanya.</summary>
        public string ResultAvailabilityNote { get; set; } = string.Empty;

        public Guid ProcedureId { get; set; }

        public string ProcedureCode { get; set; } = string.Empty;

        public string ProcedureName { get; set; } = string.Empty;

        /// <summary>
        /// Status operasional pesanan. Bukan status pembayaran — Laboratorium tidak memiliki
        /// status finansial apa pun.
        /// </summary>
        public string OrderStatus { get; set; } = string.Empty;

        public int SpecimenCount { get; set; }

        /// <summary>
        /// Jumlah sampel yang sudah dinyatakan layak, yaitu jumlah komponen pemeriksaan yang
        /// sudah memenuhi milestone kelayakan tagih.
        /// </summary>
        public int AcceptedSpecimenCount { get; set; }

        public bool IsCancel { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class LabOrderDetailResponse : LabOrderListResponse
    {
        /// <summary>
        /// Disiplin pesanan (<c>LAB-API-v1</c> r3, <c>LAB-DEC-025</c>). Kosong hanya untuk
        /// pesanan yang dibuat sebelum kolom disiplin ada.
        /// </summary>
        public string? Discipline { get; set; }

        public DateTime? RequestedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public string? StatusBeforeHold { get; set; }

        public int Version { get; set; }

        public DateTime? CancelDateTime { get; set; }

        public Guid? CancelBy { get; set; }
    }
}
