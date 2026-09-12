namespace QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services
{
    /// <summary>
    /// Permintaan satu nomor bisnis kepada alokator bersama.
    /// </summary>
    /// <remarks>
    /// <b>Empat dari enam parameter datang dari modul pemanggil</b>, dan itu bukan kebetulan:
    /// inilah `DEC-PLT-005` dalam bentuk kode. Platform memiliki <b>mesin alokasinya</b>; awalan,
    /// jumlah digit, kebijakan pengulangan, dan penanda deret tetap milik modul masing-masing.
    /// Memindahkan salah satunya ke platform akan memaksa setiap deret baru melewati approval
    /// platform.
    /// </remarks>
    /// <param name="SequenceKey">
    /// Penanda deret milik modul pemanggil, contoh <c>BBK_BLOOD_ORDER</c>. Satu penanda dimiliki
    /// tepat satu modul; dua modul yang memakainya bersama akan berbagi satu pencacah.
    /// </param>
    /// <param name="Prefix">Awalan nomor, maksimal 15 karakter.</param>
    /// <param name="ResetPolicy">
    /// Kebijakan pengulangan. Deret baru memakai <c>NEVER</c> (<c>DEC-PLT-004</c>); nilai lain ada
    /// semata untuk menampung deret lama saat <c>PLT-SLICE-02</c> memindahkannya.
    /// </param>
    /// <param name="SequenceDigits">Jumlah digit bagian urut, antara 4 dan 12.</param>
    /// <param name="ActorUserId">Pelaku, disimpan sebagai audit pada baris deret.</param>
    /// <param name="Instant">Waktu acuan penghitungan periode.</param>
    public sealed record NumberAllocationRequest(
        string SequenceKey,
        string Prefix,
        string ResetPolicy,
        int SequenceDigits,
        Guid ActorUserId,
        DateTimeOffset Instant);
}
