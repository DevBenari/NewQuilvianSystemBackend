using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.JournalManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.DTOs
{
    /// <summary>
    /// Pratinjau tutup tahun: saldo tiap akun pendapatan dan beban sepanjang satu tahun buku,
    /// beserta selisih yang akan masuk akun laba ditahan (<c>ACC-DEC-053</c>,
    /// <c>ACC-DEC-054</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Dihitung saat diminta dan tidak pernah disimpan.</b> Menyimpannya akan membuat jurnal
    /// penutup disusun dari angka yang sudah berubah — misalnya jurnal Desember baru saja
    /// disahkan setelah pratinjau dibuka. <see cref="EvaluatedAt"/> ada supaya layar dapat
    /// menunjukkan kapan angka ini diambil.
    /// </para>
    /// <para>
    /// Isinya adalah <b>persis</b> baris yang akan dibuat <c>POST /generate</c>, bukan ringkasan
    /// yang mirip. Pratinjau yang menampilkan angka berbeda dari jurnal yang akhirnya tersusun
    /// tidak akan pernah tertangkap siapa pun: keduanya sama-sama seimbang.
    /// </para>
    /// </remarks>
    public class YearEndClosingPreviewResponse
    {
        public Guid LegalEntityId { get; set; }

        public int FiscalYear { get; set; }

        /// <summary>
        /// Waktu perhitungan ini dijalankan. Selalu berubah setiap kali endpoint dipanggil.
        /// </summary>
        public DateTime EvaluatedAt { get; set; }

        /// <summary>
        /// Tanggal akuntansi jurnal penutup: tanggal terakhir periode terakhir tahun buku itu.
        /// Diambil dari periode, bukan ditulis <c>31 Desember</c> di kode, supaya tetap benar
        /// bila tahun bukunya belum lengkap dua belas periode.
        /// </summary>
        public DateTime ClosingDate { get; set; }

        /// <summary>
        /// Jumlah periode tahun buku itu yang ditemukan. Normalnya dua belas
        /// (<c>ACC-DEC-013</c>).
        /// </summary>
        public int PeriodCount { get; set; }

        public Guid RetainedEarningsAccountId { get; set; }

        public string RetainedEarningsAccountCode { get; set; } = string.Empty;

        public string RetainedEarningsAccountName { get; set; } = string.Empty;

        /// <summary>Baris akun pendapatan, terurut menurut kode akun.</summary>
        public List<YearEndClosingPreviewLineResponse> RevenueLines { get; set; } = new();

        /// <summary>Baris akun beban, terurut menurut kode akun lalu unit biaya.</summary>
        public List<YearEndClosingPreviewLineResponse> ExpenseLines { get; set; } = new();

        /// <summary>
        /// Total pendapatan tahun itu, disajikan positif sebagaimana lazimnya dibaca
        /// (saldo normal pendapatan adalah kredit).
        /// </summary>
        public decimal TotalRevenue { get; set; }

        /// <summary>Total beban tahun itu, disajikan positif.</summary>
        public decimal TotalExpense { get; set; }

        /// <summary>
        /// <see cref="TotalRevenue"/> dikurangi <see cref="TotalExpense"/>. Positif berarti laba,
        /// negatif berarti rugi, dan keduanya sah.
        /// </summary>
        public decimal NetIncome { get; set; }

        /// <summary>
        /// Nilai yang akan mendarat di akun laba ditahan, beserta sisinya. Laba masuk sisi
        /// kredit, rugi masuk sisi debit.
        /// </summary>
        public decimal RetainedEarningsDebit { get; set; }

        public decimal RetainedEarningsCredit { get; set; }

        /// <summary>
        /// Jumlah baris yang akan dibuat jurnal penutup, termasuk baris laba ditahan.
        /// </summary>
        public int LineCount { get; set; }

        /// <summary>Total debit jurnal penutup yang akan disusun.</summary>
        public decimal TotalDebit { get; set; }

        /// <summary>Total kredit jurnal penutup yang akan disusun.</summary>
        public decimal TotalCredit { get; set; }

        /// <summary>
        /// Selalu benar bila pratinjau berhasil. Disertakan supaya layar tidak perlu
        /// membandingkan dua angka sendiri.
        /// </summary>
        public bool IsBalanced { get; set; }
    }

    /// <summary>
    /// Satu baris pratinjau — sekaligus satu baris jurnal penutup yang akan dibuat.
    /// </summary>
    /// <remarks>
    /// <b>Baris akun beban dipecah per unit biaya, bukan digabung per akun.</b> Ini bukan
    /// hiasan: <c>ACC-DEC-019</c> mewajibkan setiap baris jurnal berakun <c>Expense</c>
    /// menyebutkan unit biaya, dan syarat 7 <c>ACC-STATE-0.1</c> bagian 1.3 memeriksanya ulang
    /// saat pengajuan dan pengesahan. Satu baris gabungan per akun beban akan tertolak di sana
    /// dengan pesan yang membingungkan, atau memaksa penutup tahun menebak satu unit biaya —
    /// dan tebakan itu merusak laporan beban per unit biaya tanpa membuat jurnalnya timpang.
    /// Jumlah seluruh barisnya per akun tetap sama persis dengan saldo akun itu.
    /// </remarks>
    public class YearEndClosingPreviewLineResponse
    {
        public Guid AccountId { get; set; }

        public string AccountCode { get; set; } = string.Empty;

        public string AccountName { get; set; } = string.Empty;

        public AccountType AccountType { get; set; }

        public Guid? CostCenterId { get; set; }

        public string? CostCenterName { get; set; }

        /// <summary>
        /// Saldo baris ini sepanjang tahun buku, condong debit positif — perjanjian tanda yang
        /// sama dengan <c>AccChartOfAccountService.HitungSaldoAsync</c>.
        /// </summary>
        public decimal Balance { get; set; }

        /// <summary>Nilai debit baris jurnal penutup. Nol bila baris ini kredit.</summary>
        public decimal DebitAmount { get; set; }

        /// <summary>Nilai kredit baris jurnal penutup. Nol bila baris ini debit.</summary>
        public decimal CreditAmount { get; set; }
    }

    /// <summary>
    /// Menyusun jurnal penutup tahun. Jurnalnya lahir <c>Draft</c> berjenis <c>JT</c>, lalu
    /// menempuh jalur pengajuan, persetujuan, dan pengesahan jurnal yang sudah ada
    /// (<c>ACC-STATE-0.2</c> bagian 4).
    /// </summary>
    public class GenerateYearEndClosingRequest
    {
        [Required]
        public Guid LegalEntityId { get; set; }

        /// <summary>
        /// Tahun buku yang ditutup. Sama dengan tahun kalender (<c>ACC-DEC-013</c>).
        /// </summary>
        [Required]
        [Range(2000, 2100)]
        public int FiscalYear { get; set; }

        /// <summary>
        /// Keterangan jurnal penutup. Dikosongkan berarti memakai kalimat bawaan yang menyebut
        /// tahun bukunya.
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }
    }

    /// <summary>
    /// Jurnal penutup tahun yang sudah pernah disusun untuk sebuah tahun buku. Dipakai pesan
    /// penolakan <c>409</c> supaya petugas dapat langsung membukanya, bukan mencarinya sendiri.
    /// </summary>
    public class YearEndClosingExistingJournal
    {
        public Guid Id { get; set; }

        public string JournalNumber { get; set; } = string.Empty;

        public JournalStatus JournalStatus { get; set; }
    }
}
