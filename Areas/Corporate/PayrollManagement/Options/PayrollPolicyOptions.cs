namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Options;

/// <summary>
/// Parameter kebijakan Payroll yang BELUM disahkan pemilik proses, dibaca dari konfigurasi (<c>Payroll:Policy</c>) dan memiliki
/// versi. Nilai bawaan sengaja mempertahankan perilaku yang sudah berjalan sebelum modul ini dipisah dari HRD, sehingga tidak ada
/// perubahan perilaku uang yang terjadi diam-diam. Setiap butir bertanda NEEDS CONFIRMATION menunggu keputusan pemilik kebijakan.
/// Pola registrasi sama dengan <c>FinanceReceivablePayrollSyncOptions</c>.
/// </summary>
public sealed class PayrollPolicyOptions
{
    public const string SectionName = "Payroll:Policy";

    /// <summary>Versi kebijakan; disalin ke snapshot setiap payroll run yang dihitung agar hasilnya dapat ditelusuri.</summary>
    public string PolicyVersion { get; set; } = "PAYROLL-POLICY-2026.11-DRAFT";

    /// <summary>
    /// Bila true, gaji pokok boleh memakai kolom kompatibilitas pada profil payroll (PrlPayroll.BaseSalary) saat pegawai belum
    /// memiliki penetapan gaji yang disetujui. Bawaan false: gaji hanya dari penetapan gaji yang disetujui dan efektif.
    /// </summary>
    public bool AllowProfileSalaryFallback { get; set; } = false;

    /// <summary>NEEDS CONFIRMATION. Bila true, tunjangan tetap ikut diprorata dengan faktor yang sama seperti gaji pokok.</summary>
    public bool ProrateFixedAllowances { get; set; } = true;

    /// <summary>
    /// NEEDS CONFIRMATION. Bila true dan HRD tidak mengirim nominal potongan kehadiran, potongan cuti tanpa gaji/mangkir dihitung
    /// dari tarif harian = gaji pokok dibayarkan / jumlah hari dibayarkan pada periode (hari kalender).
    /// </summary>
    public bool UnpaidLeaveDeductionEnabled { get; set; } = true;

    /// <summary>NEEDS CONFIRMATION. Bila true, pengesah run tidak boleh orang yang sama dengan pengaju (pemisahan kewenangan).</summary>
    public bool RequireDifferentApprover { get; set; } = false;

    /// <summary>
    /// Saklar jalur perhitungan lama (modul "Modern": <c>PayrollRunModernController</c>). Payroll Run resmi adalah satu-satunya pemilik hasil
    /// gaji. Konsumen jalur lama di Frontend (layar HR payroll lama: siklus, wizard, persetujuan, handoff, variansi, pengaturan) tidak lagi
    /// memiliki route maupun menu yang dapat dijangkau (diperiksa statis pada kode Frontend), sehingga bawaannya false: seluruh aksi tulis
    /// jalur lama ditolak (HTTP 422), sedangkan pembacaan riwayat tetap berjalan dan tidak ada data historis yang dihapus. Setel true
    /// HANYA bila ada run lama yang masih berjalan di alur persetujuan lama dan perlu diselesaikan; riwayat database wajib diperiksa
    /// pemilik sebelum saklar dibiarkan mati. Terlepas dari saklar ini: kalkulasi PPh dan BPJS jalur lama SELALU ditolak (tarifnya tertanam
    /// atau berasal dari nilai bawaan yang belum diverifikasi), dan run yang sudah dihitung mesin resmi tidak pernah dapat diubah jalur
    /// lama (mencegah komponen ganda).
    /// </summary>
    public bool LegacyModernMutationsEnabled { get; set; } = false;

    /// <summary>
    /// NEEDS CONFIRMATION. Persentase maksimum potongan cicilan terhadap gaji bersih yang tersedia, dipakai HANYA bila master jenis
    /// potongan tidak menentukan persentase. Nilai bawaan 50 sama dengan perilaku sebelumnya; ubah setelah kebijakan disahkan.
    /// </summary>
    public decimal InstallmentDefaultMaxNetSalaryPercentage { get; set; } = 50m;

    /// <summary>Bila true, handoff pembayaran boleh mengirim sebagian penerima (yang rekeningnya valid) dan melewati sisanya.</summary>
    public bool AllowPartialFinanceHandoff { get; set; } = false;

    /// <summary>
    /// NEEDS CONFIRMATION (jalur honor dokter). Bila false, jasa medis HANYA dibayar lewat Finance AP (perilaku Medical Fee saat ini)
    /// dan intake Payroll menolak klaim sumber. Aktifkan hanya setelah pemilik kebijakan menetapkan jalur PAYROLL.
    /// </summary>
    public bool MedicalFeePayrollPathEnabled { get; set; } = false;

    /// <summary>
    /// Kode komponen penggajian (Data Induk &gt; Komponen Penggajian, tipe Earning) yang dipakai untuk jasa medis yang dibayar lewat
    /// slip. Wajib ada dan aktif sebelum jalur PAYROLL dipakai.
    /// </summary>
    public string MedicalFeeComponentCode { get; set; } = "MEDICAL_SERVICE_FEE";

    /// <summary>
    /// NEEDS CONFIRMATION (aturan akses slip gaji). Kapan slip boleh diterbitkan ke penerima:
    /// <c>PaymentPaid</c> = setelah pembayaran terkonfirmasi lunas; <c>RunApproved</c> = sejak run disahkan.
    /// </summary>
    public string PayslipPublishTrigger { get; set; } = PayrollPayslipPublishTriggers.PaymentPaid;

    /// <summary>
    /// Masa peralihan tunjangan tetap (PRL-DEC-04, PGJ-C1). Bila true dan Struktur Gaji penetapan belum memuat baris tunjangan,
    /// mesin hitung memakai PrlPayroll.TotalAllowance beserta peringatan. Dimatikan setelah data struktur lengkap (DEC-PRL-015).
    /// </summary>
    public bool AllowProfileAllowanceFallback { get; set; } = true;

    /// <summary>Batas jumlah pegawai per permintaan simulasi penggajian (PRL-DEC-32, validasi SIM-02).</summary>
    public int SimulationMaxEmployees { get; set; } = 50;

    /// <summary>
    /// Saklar tunggal fitur BPJS Kesehatan dan BPJS Ketenagakerjaan (BE-06), dibaca lewat <c>IBpjsFeatureGate</c>: Payroll (iuran,
    /// kebijakan, kepesertaan), HRD (klaim COB/SEP, nomor kepesertaan benefit), dan K3 (klaim dan worker JKK) mengikuti nilai yang sama,
    /// begitu pula Frontend lewat <c>GET api/v1/system/features</c>.
    ///
    /// Bawaan <c>false</c> (BPJS dinonaktifkan sementara, keputusan pemilik 2026-11-18): BPJS tidak dihitung, tidak ada kebijakan,
    /// kepesertaan, nomor klaim, atau formulir BPJS baru, dan konfigurasinya tidak tampil. Data dan riwayat tidak dihapus. Setel
    /// <c>true</c> secara eksplisit untuk mengaktifkan kembali. Run yang dihitung saat <c>false</c> ditandai mode pengujian dan
    /// ditahan dari Finance Handoff produksi sampai ada keputusan bisnis yang sah (pengaman tidak dilemahkan).
    /// </summary>
    public bool EnableBpjsInPayroll { get; set; } = false;
}

public static class PayrollPayslipPublishTriggers
{
    public const string PaymentPaid = "PaymentPaid";
    public const string RunApproved = "RunApproved";
}
