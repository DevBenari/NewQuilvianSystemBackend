# ISSUE-021 — Kegagalan Admisi Transfer Akibat Validasi Metode Penerimaan Deposit yang Tidak Diperlukan di Loket Admisi

```yaml
issue_id: ISSUE-EPS-021
module_id: rawat-inap
submodule: episode-rawat-inap
blueprint_id: RWI-BP-001
layar: "Admisi Rawat Inap — Langkah 7 Booking Bed pada Admisi Transfer Pasien (FE-INP-20 / FE-RWI-228 / FE-RWI-229)"
sumber_laporan: "Laporan pengguna 15 Oktober 2026 disertai tangkapan layar kegagalan API admit (status 422: DEPOSIT_METHOD_REQUIRED)"
tanggal_issue: "2026-10-15"
status: SELESAI
keparahan_tertinggi: High
source_sha_backend: "bcc663df321aabbc07888385605aa196c74cc6f3 (MHamzah)"
source_sha_frontend: "60f7764a08c032e2ead66c9ea744e44e2497f6b6 (HamzahV2)"
rencana_perbaikan: ../plan-repair/plan-repair-021-peniadaan-input-metode-deposit-admisi-transfer.md
```

---

## 1. Ringkasan

Ketika petugas admisi menyelesaikan proses transfer pasien IGD ke rawat inap pada **Langkah 7 (Booking Bed)**, pengiriman data gagal dengan pesan galat **HTTP 422: "Pilih metode penerimaan deposit" (`DEPOSIT_METHOD_REQUIRED`)**. Berdasarkan penelusuran arsitektur dan proses bisnis rumah sakit, pendaftaran rawat inap di loket admisi sejatinya **hanya mencatat rencana uang muka (*Planned Deposit Amount*)** dan tidak melakukan penerimaan uang fisik maupun pemilihan instrumen pembayaran. Penerimaan uang kas, gesek EDC, atau transfer bank merupakan fungsi dari unit **Kasir/Billing**, bukan loket admisi. Di sisi kode sumber terbaru, frontend (`commit d2f5538e1`) dan backend (`commit bcc663df`) sebenarnya telah menyepakati penghapusan kewajiban metode deposit ini, namun instance runtime server backend lokal yang aktif masih menjalankan berkas *build* lama (tertanggal 14 Oktober 2026 pukul 16:59) sehingga penolakan server tetap terjadi.

---

## 2. Laporan Asli Pelapor

| No. Laporan | Ringkasan Keluhan Pelapor | Lampiran Terkait |
| :---: | :--- | :--- |
| **1** | *"Tolong atasi masalah ini , pada transfer pasien untuk input deposit method tidak diperlukan di admisi karena nanti itu adalah kebutuhan billing bukan admisi"* | Tangkapan layar Langkah 7 Booking Bed dengan Network DevTools menampilkan respon HTTP 422: `{"code": "DEPOSIT_METHOD_REQUIRED", "message": "Pilih metode penerimaan deposit."}` |

Data muatan request yang dikirimkan pada tangkapan layar:
```json
{
  "bedId": "67ce6820-3506-4301-8826-df65e5dc1969",
  "depositAmount": 1000000,
  "depositPaymentMethodId": null,
  "doctorId": "624ac24b-2720-4acb-8e40-f3eee3875f0b",
  "expectedBedVersion": 3,
  "isolationNote": null,
  "notes": "Observasi ketat trauma tumpul abdomen suspek ruptur lien grade 1-2 pasca KLL, memerlukan rawat inap intensif bedah.",
  "patientClassId": "3e6f18fc-950c-4205-a05f-5f4be3eda9ea",
  "patientCompanyGuarantorId": null,
  "patientId": "11111111-aaaa-4111-8111-111111111111",
  "patientInsuranceId": null,
  "paymentMethodId": null,
  "paymentType": 1,
  "requiresIsolation": false,
  "serviceUnitId": "fddbe4ae-832b-484c-aba7-d6280e07c311",
  "sourceType": "Emergency",
  "transferDispositionId": "11111111-dddd-4111-8111-111111111111"
}
```

---

## 3. Ringkasan Temuan

| ID Temuan | No. Laporan | Judul Temuan | Jenis | Area | Keparahan | Status Bukti | ID Perbaikan |
| :--- | :---: | :--- | :--- | :--- | :---: | :--- | :--- |
| `ISS-EPS-021-01` | 1 | Instance Runtime Backend Menjalankan Binary Kedaluwarsa yang Masih Memvalidasi `DEPOSIT_METHOD_REQUIRED` | `BUG` / `RUNTIME_DRIFT` | Backend Runtime | High | `SUDAH-VERIFIKASI` di Git commit `bcc663df` vs timestamp file DLL backend | `FIX-EPS-021-01` |
| `ISS-EPS-021-02` | 1 | Kontrak DTO dan Payload Transfer Masih Membawa Properti `depositPaymentMethodId` yang Sudah Usang | `CONTRACT_MISMATCH` / `DESIGN_CHANGE` | Frontend & Backend | Medium | `SUDAH-VERIFIKASI` di `InpatientAdmissionTransferDtos.cs:133` & `inpatient-admission-transfer-utils.js:41` | `FIX-EPS-021-02` |
| `ISS-EPS-021-03` | 1 | Fungsi API Service Frontend `getDepositMethods` Masih Tersisa Tanpa Pemanggil UI | `CODE_SMELL` / `CLEANUP` | Frontend | Low | `SUDAH-VERIFIKASI` di `inpatient-admission-transfer.service.js:64-72` | `FIX-EPS-021-03` |

---

## 4. Rincian Per Temuan

### ISS-EPS-021-01 — Instance Runtime Backend Menjalankan Binary Kedaluwarsa yang Masih Memvalidasi `DEPOSIT_METHOD_REQUIRED`

| Parameter | Keterangan |
| :--- | :--- |
| **No. Laporan** | 1 |
| **Jenis** | `BUG` / `RUNTIME_DRIFT` |
| **Area** | Backend Runtime |
| **Keparahan** | High |
| **Status Bukti** | `SUDAH-VERIFIKASI` di Git commit `bcc663df` vs timestamp file DLL backend |
| **Perbaikan** | `FIX-EPS-021-01` |

**Apa yang terjadi.**  
Petugas admisi rawat inap mengisi nominal uang muka rencana sebesar Rp 1.000.000 pada Langkah 1 (Deposit). Pada antarmuka pengguna tidak ada pilihan metode pembayaran deposit karena uang muka disetorkan di kasir. Namun ketika petugas sampai pada Langkah 7 (Booking Bed) dan menekan tombol pemesanan kamar, sistem memunculkan kotak galat merah: *"Gagal Memuat Permintaan Rawat Inap — Pilih metode penerimaan deposit"*.

**Kenapa terjadi.**  
Secara historis pada implementasi awal (`commit d7df71e6`), berkas backend `InpAdmissionTransferService.cs` baris 146 memuat blok validasi:
```csharp
// Versi lama (d7df71e6):
if (request.DepositAmount > 0)
{
    if (!request.DepositPaymentMethodId.HasValue)
        throw new InpBedOperationException(422, "DEPOSIT_METHOD_REQUIRED", "Pilih metode penerimaan deposit.");
    try
    {
        await deposits.TopUpAsync(episode.EncounterId, new DepositTopUpRequest ...);
    }
}
```
Pada perbaikan tanggal 14 Oktober 2026 pukul 22:22 (`commit bcc663df`), baris tersebut telah **dihapus** dari kode sumber dan digantikan dengan penyimpanan rencana uang muka murni:
```csharp
// Versi terkini (bcc663df):
if (request.DepositAmount > 0)
{
    var trackedEpisode = await _dbContext.Set<InpEpisode>().FirstOrDefaultAsync(x => x.Id == episode.Id, cancellationToken);
    if (trackedEpisode != null)
    {
        trackedEpisode.PlannedDepositAmount = request.DepositAmount.Value;
        episode.PlannedDepositAmount = request.DepositAmount.Value;
    }
}
```
Akan tetapi, berkas terkompilasi `QuilvianSystemBackend.dll` pada direktori `bin/Debug/net9.0/` memiliki *timestamp* **14 Oktober 2026 pukul 16:59**. Artinya, server API backend yang sedang berjalan di komputer lokal saat pengujian dilakukan adalah binary hasil kompilasi **sebelum** perbaikan `bcc663df` diterapkan. Akibatnya, server tetap menjalankan aturan lama yang mewajibkan `depositPaymentMethodId`.

**Apakah ini menyimpang dari desain?**  
Menyimpang (`BUG`). Berdasarkan prinsip batas konteks (*bounded context*) rumah sakit:
- **Loket Admisi Rawat Inap**: Bertugas mendaftarkan pasien, menentukan kelas dan penjamin, menetapkan DPJP, memesan tempat tidur (*bed reservation*), dan mencatat estimasi/rencana deposit keluarga (`PlannedDepositAmount`).
- **Loket Kasir / Billing**: Bertugas menerima uang fisik, memvalidasi bukti transfer bank / kartu kredit / debit, dan menerbitkan kwitansi resmi (*receipt*).
Memaksa petugas admisi menginput metode penerimaan deposit di awal melanggar alur operasional RS dan membingungkan petugas karena loket admisi bukanlah loket kasir penerimaan uang.

**Dampak nyata.**  
Alur admisi transfer terhenti total (*blocked*) pada langkah ke-7 setiap kali nominal uang muka diisi di atas Rp 0, sehingga pasien darurat tidak dapat didaftarkan ke tempat tidur rawat inap sebelum server disinkronkan.

**Rekomendasi.**  
1. Lakukan kompilasi ulang (*clean build*) project `QuilvianSystemBackend.csproj`.
2. Matikan proses backend yang berjalan dan nyalakan ulang (*restart*) agar memuat binary terbaru yang bebas dari validasi `DEPOSIT_METHOD_REQUIRED`.

---

### ISS-EPS-021-02 — Kontrak DTO dan Payload Transfer Masih Membawa Properti `depositPaymentMethodId` yang Sudah Usang

| Parameter | Keterangan |
| :--- | :--- |
| **No. Laporan** | 1 |
| **Jenis** | `CONTRACT_MISMATCH` / `DESIGN_CHANGE` |
| **Area** | Frontend & Backend |
| **Keparahan** | Medium |
| **Status Bukti** | `SUDAH-VERIFIKASI` di `InpatientAdmissionTransferDtos.cs:133` & `inpatient-admission-transfer-utils.js:41` |
| **Perbaikan** | `FIX-EPS-021-02` |

**Apa yang terjadi.**  
Meskipun metode deposit sudah tidak digunakan, payload yang dikirimkan oleh frontend Next.js masih menyertakan `"depositPaymentMethodId": null`. Di sisi backend, model DTO `OpenAdmissionFromTransferRequest` juga masih mempertahankan properti `public Guid? DepositPaymentMethodId { get; set; }`.

**Kenapa terjadi.**  
Ketika antarmuka dropdown metode deposit dieliminasi pada `commit d2f5538e1`, parameter pada helper `buildTransferAdmissionPayload` hanya diubah nilainya menjadi `null`, bukan dibersihkan seutuhnya dari kontrak pendaftaran transfer.

**Dampak nyata.**  
Menyisakan *technical debt*, membingungkan pengembang lain atau integrasi API publik yang mengira properti tersebut masih memiliki dampak terhadap pembentukan deposit.

**Rekomendasi.**  
Hapus atau tandai `[Obsolete]` pada DTO backend, dan hilangkan pengiriman properti `depositPaymentMethodId` dari helper payload frontend.

---

### ISS-EPS-021-03 — Fungsi API Service Frontend `getDepositMethods` Masih Tersisa Tanpa Pemanggil UI

| Parameter | Keterangan |
| :--- | :--- |
| **No. Laporan** | 1 |
| **Jenis** | `CODE_SMELL` / `CLEANUP` |
| **Area** | Frontend |
| **Keparahan** | Low |
| **Status Bukti** | `SUDAH-VERIFIKASI` di `inpatient-admission-transfer.service.js:64-72` |
| **Perbaikan** | `FIX-EPS-021-03` |

**Apa yang terjadi.**  
Pada `inpatient-admission-transfer.service.js`, terdapat fungsi `getDepositMethods()` yang memanggil endpoint `/v1/health-services/billing-management/master-data/payment-methods/options`. Fungsi ini sudah tidak lagi dipanggil oleh hook mana pun di modul rawat inap.

**Kenapa terjadi.**  
Sisa kode dari perbaikan `ISSUE-018` sebelum diputuskan bahwa admisi tidak mengurusi metode penerimaan pembayaran.

**Rekomendasi.**  
Hapus atau rapikan fungsi tersebut agar tidak membebani pemeliharaan kode sumber.

---

## 5. Tanya-Jawab Pelapor

> **T:** *"Tolong atasi masalah ini , pada transfer pasien untuk input deposit method tidak diperlukan di admisi karena nanti itu adalah kebutuhan billing bukan admisi"*
>
> **J:** Pernyataan pelapor **100% benar dan tepat secara arsitektur domain rumah sakit**. Loket admisi rawat inap hanya mencatat kesanggupan atau rencana uang muka (*Planned Deposit*). Eksekusi penerimaan dana, pemilihan metode (Tunai, EDC, Transfer, QRIS), dan pencetakan bukti setor (*deposit receipt*) adalah domain operasional **Billing / Kasir**. Di kode sumber backend `InpAdmissionTransferService.cs`, validasi ini sebenarnya sudah dicabut pada commit terakhir (`bcc663df`). Galat 422 `DEPOSIT_METHOD_REQUIRED` yang muncul saat pengujian di layar disebabkan oleh server lokal yang masih mengeksekusi *assembly dll* lama (sebelum `bcc663df`). Langkah penanganannya adalah melakukan *rebuild* backend dan me-restart server, serta membersihkan field usang `depositPaymentMethodId` dari kontrak DTO transfer.

---

## 6. Temuan Tambahan

1. **Pemisahan Alur Deposit Admisi vs Billing Kasir**:
   - Di Admisi Rawat Inap: Petugas mencatat nominal rencana deposit (`depositAmount`). Nilai ini disimpan ke kolom `PlannedDepositAmount` pada tabel `InpEpisode`.
   - Di Kasir / Billing: Ketika keluarga pasien menuju loket pembayaran kasir, petugas kasir membuka antrean deposit / billing intake pasien tersebut, memilih metode pembayaran (`MstPaymentMethod`), menerima uang fisik/non-tunai, dan memanggil `BillingDepositService.TopUpAsync()`.
   - Hal ini memastikan bahwa admisi tidak pernah gagal hanya karena keluarga belum menentukan metode pembayaran uang muka di loket pendaftaran ranap.

---

## 7. Pertanyaan dan Keputusan yang Dibutuhkan

**Untuk Pemilik / Pelapor:**

| No | Keputusan yang Dibutuhkan | Rekomendasi Solusi | Status |
| :---: | :--- | :--- | :---: |
| **K-01** | Apakah properti `depositPaymentMethodId` pada DTO transfer `OpenAdmissionFromTransferRequest` backend dihapus sepenuhnya atau dipertahankan sebagai opsional bernilai `null` untuk kompatibilitas ke belakang (*backward compatibility*)? | **Rekomendasi:** Dipertahankan sebagai opsional nullable `Guid?` pada DTO namun tidak digunakan lagi oleh logika backend, sedangkan frontend berhenti mengirimkannya. Hal ini mencegah *breaking change* bila ada klien lain yang masih memuat properti tersebut. | Menunggu Persetujuan |

---

## 8. Catatan Pola

- **Runtime Drift Pasca Refactoring Domain**: Setiap kali terdapat perubahan logika validasi di backend (terutama yang menghilangkan penolakan *HTTP 422*), developer/tester wajib mematikan proses lama dan melakukan *clean build* sebelum melakukan pengetesan ulang antarmuka web. Kegagalan me-restart server membuat perbaikan tampak belum berhasil meskipun kode sumber sudah benar.

---

## 9. Riwayat Dokumen

| Tanggal | Perubahan | Oleh |
| :--- | :--- | :--- |
| **2026-10-15** | Dokumen issue dibuat berdasarkan laporan pengguna mengenai penolakan `DEPOSIT_METHOD_REQUIRED` dan penegasan bahwa metode pembayaran deposit adalah wewenang billing, bukan admisi. | Antigravity AI |
