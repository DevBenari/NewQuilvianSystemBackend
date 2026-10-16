# ISSUE-017 — Galat Server 500 dan Kerusakan Tampilan Saat Melanjutkan Admisi Pasien Transfer Poliklinik

```yaml
issue_id: ISSUE-EPS-017
module_id: rawat-inap
submodule: episode-rawat-inap
blueprint_id: RWI-BP-001
layar: "Admisi Rawat Inap (FE-INP-03 / Skema Tampilan 3.1) — Tab Transfer Pasien IGD/Poli menuju Langkah 2 Verifikasi Pasien"
sumber_laporan: "Laporan pengguna: tolong perbaiki tampilan admisi rawat inap, pada tranfer pasien, tolong di perbaiki dan di analisis kesalahan eror ya dimana ya. masalah ya terjadi ketika saya melanjutkan ke step berikut ya terjadi eror seperti itu"
tanggal_issue: "2026-10-14"
status: SELESAI
keparahan_tertinggi: Fatal
source_sha_backend: "9f8d82666e8d67dde203d64ffab640305efc90ab"
source_sha_frontend: "88cb19f0c8edd998fb7a19701f0f52ac9aab839b"
rencana_perbaikan: ../plan-repair/plan-repair-017-admisi-transfer-galat-server.md
```

---

## 1. Ringkasan Masalah

Pada modul **Admisi Rawat Inap**, petugas admisi membuka antrean rujukan transfer pasien dari Instalasi Gawat Darurat (IGD) dan Poliklinik Rawat Jalan (*Outpatient*). Pada daftar antrean (Langkah 1), data pasien poliklinik yang memerlukan rawat inap berhasil tampil dengan status antrean normal.

Namun, ketika petugas menekan tombol **"Proses Admisi ->"** pada salah satu pasien transfer Poliklinik (misalnya pasien *Ratna Dewi Kusuma*, Nomor Kunjungan `ENC-OPD-202610-002`, ID Transfer `476acefd-5f52-4140-9bcd-3e6d6238c3f3`) untuk melanjutkan ke **Langkah 2: Verifikasi Pasien**, sistem mengalami kegagalan fatal dua lapis:

1. **Lapis Backend (HTTP 500 Internal Server Error):**  
   Endpoint `GET /api/v1/health-services/inpatient-management/admission-transfers/{id}?sourceType=Outpatient` mengalami kegagalan eksekusi query PostgreSQL akibat `InvalidCastException`. Kolom `EmergencyVisitId` yang bernilai konstanta `Guid.Empty` diproyeksikan oleh Entity Framework Core sebagai literal teks string `'00000000-0000-0000-0000-000000000000'` dengan tipe data PostgreSQL `text`. Npgsql menolak mengonversi tipe `text` tersebut menjadi `System.Guid`, sehingga backend melempar unhandled exception yang ditangkap oleh middleware global dengan respons error status 500.

2. **Lapis Frontend (Kerusakan Tampilan / Layout Degradation):**  
   Saat hook `useInpatientAdmissionTransfer` menerima pesan galat `"Terjadi kesalahan pada server."`, komponen utama `InpatientAdmissionView` masuk ke cabang render error kondisional. Cabang ini menggantikan seluruh `InpatientAdmissionStepper` dan wadah kartu alur (`flowPanel`), sehingga header tahapan langkah 1 sampai 10 hilang seketika dari layar. Selain itu, tombol aksi *"Periksa Ulang"* dan *"Kembali ke Antrean"* dirender tanpa pembungkus kartu (container card) dan tanpa penataan tombol flex yang proporsional, menyebabkan tampilan menjadi kosong, polos, dan tombol membentang penuh 100% secara vertikal (rusak estetika).

---

## 2. Laporan Asli Pengguna

| No. | Kalimat Pelapor | Konteks Layar |
| :---: | --- | --- |
| 1 | *"tolong perbaiki tampilan admisi rawat inap , pada tranfer pasien , tolong di perbaiki dan di analisis kesalahan eror ya dimana ya. anda analisis terlebih dahulu dan berikan planning ya"* | Layar Admisi Rawat Inap — Tab Antrean Pasien Transfer IGD/Poli |
| 2 | *"masalah ya terjadi ketika saya melanjutkan ke step berikut ya terjadi eror seperti itu"* | Saat menekan tombol aksi "Proses Admisi ->" pada baris pasien poli untuk berpindah dari Langkah 1 ke Langkah 2 |

---

## 3. Ringkasan Temuan Masalah

| ID Temuan | Masalah | Jenis | Area | Keparahan | Status Bukti | Solusi Rencana |
| --- | --- | :---: | :---: | :---: | :---: | --- |
| `ISS-EPS-017-01` | `InvalidCastException` pada pembacaan `EmergencyVisitId` dari query transfer pasien Poliklinik | `BUG` | Backend | Fatal | TERBUKTI LENGKAP (Log Backend baris 847–851) | `FIX-EPS-017-01` |
| `ISS-EPS-017-02` | Nilai semantik `EmergencyVisitId` non-nullable untuk pasien Poliklinik yang tidak memiliki kunjungan IGD | `DESIGN_DEFECT` | Backend DTO | High | TERBUKTI LENGKAP (`InpatientAdmissionTransferDtos.cs:45`) | `FIX-EPS-017-02` |
| `ISS-EPS-017-03` | Percabangan error state transfer menghilangkan Stepper dan Flow Panel admisi | `BUG_UI` | Frontend | High | TERBUKTI LENGKAP (`inpatient-admission-view.jsx:1108-1114`) | `FIX-EPS-017-03` |
| `ISS-EPS-017-04` | Tombol aksi galat transfer meregang 100% tanpa kontainer kartu dan layout grup tombol | `UI_DEGRADATION` | Frontend | Medium | TERBUKTI LENGKAP (Screenshot 2 Pengguna) | `FIX-EPS-017-04` |

---

## 4. Rincian Per Temuan

### ISS-EPS-017-01 & ISS-EPS-017-02 — Kegagalan Fatal `InvalidCastException` pada Detail Transfer Poliklinik

* **Komponen Backend**:  
  `NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Services/InpAdmissionTransferService.Queries.cs` baris 50–77  
  `NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/DTOs/InpatientAdmissionTransferDtos.cs` baris 45
* **Bukti Jejak Log Server (`quilvian-backend-20261014.json:line 850`)**:
  ```json
  {
    "Path": "/api/v1/health-services/inpatient-management/admission-transfers/476acefd-5f52-4140-9bcd-3e6d6238c3f3",
    "Method": "GET",
    "StatusCode": 500,
    "ExceptionType": "InvalidCastException",
    "ExceptionMessage": "Reading as 'System.Guid' is not supported for fields having DataTypeName 'text'",
    "StackTrace": "at Npgsql.NpgsqlDataReader.GetFieldValueCore[T](Int32 ordinal)\r\n   at lambda_method8256(Closure, QueryContext, DbDataReader, ResultContext, SingleQueryResultCoordinator)\r\n   at Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable`1.AsyncEnumerator.MoveNextAsync()\r\n   at QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services.InpAdmissionTransferService.GetTransferByIdAsync(Guid transferId, CancellationToken cancellationToken, String sourceType) in InpAdmissionTransferService.cs:line 60"
  }
  ```
* **Akar Masalah**:
  1. Pada [InpAdmissionTransferService.Queries.cs](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Services/InpAdmissionTransferService.Queries.cs#L60), query `outpatient` yang mengambil data dari `CliDoctorCertificate` (surat pengantar rawat inap poli) menginisialisasi properti:
     ```csharp
     EmergencyVisitId = Guid.Empty,
     ```
  2. Dalam EF Core dengan provider PostgreSQL (Npgsql), konstanta C# `Guid.Empty` diproyeksikan langsung sebagai string literal `'00000000-0000-0000-0000-000000000000'`. Di PostgreSQL, literal tersebut tidak memiliki typecast `::uuid` dan diberi tipe `text`.
  3. Ketika pemanggilan `GetTransferByIdAsync` dilakukan dengan filter `sourceType = "Outpatient"`, query `outpatient` dieksekusi secara mandiri. Npgsql menolak memetakan kolom bertipe PostgreSQL `text` ke properti C# `System.Guid`.
  4. Secara domain model rumah sakit, pasien yang berasal dari rujukan Poliklinik memang **tidak memiliki kunjungan IGD**. Pemaksaan tipe `Guid` non-nullable dengan nilai `Guid.Empty` adalah cacat desain; tipe yang benar secara semantik adalah `Guid?` (nullable) dengan nilai `null`.

---

### ISS-EPS-017-03 & ISS-EPS-017-04 — Stepper Hilang dan Tampilan Tombol Rusak Saat Error

* **Komponen Frontend**:  
  `QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/inpatient-admission-view.jsx` baris 1108–1114
* **Kondisi Kode Saat Masalah Terjadi**:
  ```jsx
  {isTransfer && flow.currentStep?.slug !== "transfer-referral" && transfer.loading ? (
    <InformationAlert variant="info" message="Memuat dan memeriksa permintaan rawat inap..." />
  ) : isTransfer && transfer.error ? (
    <>
      <InformationAlert variant="danger" message={transfer.error} />
      <BaseButton variant="secondary" onClick={transfer.retry}>Periksa Ulang</BaseButton>
      <BaseButton variant="secondary" onClick={() => flow.writeFlowUrl({...})}>Kembali ke Antrean</BaseButton>
    </>
  ) : (
    <>
      <InpatientAdmissionStepper ... />
      <section className={admissionStyles.flowPanel}>...</section>
    </>
  )}
  ```
* **Akar Masalah**:
  1. Kondisi `isTransfer && transfer.error` diletakkan sebagai `else-if` sebelum komponen `<InpatientAdmissionStepper />`. Akibatnya, ketika terjadi error, seluruh wizard (stepper 10 langkah dan panel utama) tidak ikut dirender.
  2. Elemen `<InformationAlert>` dan dua tombol `<BaseButton>` dirender langsung di dalam fragment tanpa wadah kartu (card container) berstandar token Quilvian.
  3. Kedua tombol tidak dibungkus dalam container flex horizontal, sehingga tombol mengambil `width: 100%` default dan menumpuk ke bawah dengan spasi renggang, menciptakan visual kosong dan rusak seperti terlihat pada Screenshot 2 pengguna.

---

## 5. Dampak Terhadap Pengguna & Alur Kerja Rumah Sakit

1. **Staf Admisi Rawat Inap Terblokir Total (Blocker P0):**  
   Petugas admisi rumah sakit tidak dapat memproses pasien rawat jalan/poliklinik yang telah dinyatakan harus mondok/rawat inap oleh dokter spesialis poliklinik.
2. **Kepanikan Pengguna:**  
   Hilangnya navigasi tahapan stepper dan tampilan polos dengan alert kesalahan server menimbulkan kebingungan bagi petugas admisi dan mengesankan sistem mengalami kerusakan fatal.
3. **Risiko Keterlambatan Pasien Mendapatkan Kamar:**  
   Pasien poli yang sudah menunggu berpotensi melebihi batas waktu tunggu 60 menit (*overdue transfer*) karena admisi tidak dapat membuat episode rawat inap.
