# Spesifikasi Serah Terima Integrasi Rawat Inap (Amandemen IGD-DEC-308 & IGD-DEC-309)

| Field | Nilai |
| :--- | :--- |
| **Dokumen** | Spesifikasi Serah Terima Integrasi Transfer IGD → Rawat Inap |
| **Tanggal** | 15 Oktober 2026 |
| **Penulis** | Agent Pengembang Modul IGD (mewakili Rizki Gunawan, Product/Domain Owner IGD) |
| **Penerima** | Muhammad Hamzah (Product/Domain Owner Rawat Inap) |
| **Status** | Diserahkan untuk Implementasi Sisi Rawat Inap |
| **Dasar Keputusan** | `IGD-DEC-308` (Penutupan Kunjungan IGD Dua Arah), `IGD-DEC-309` (Penolakan Form Admisi & Ringkasan Antrean) |

---

## 1. Latar Belakang & Ringkasan Perubahan

Berdasarkan kesepakatan domain transfer pasien antara IGD dan Rawat Inap (`IGD-DEC-304`…`307`), pasien transfer IGD masuk ke antrean transfer yang dikelola oleh modul Rawat Inap (**Antrean Transfer IGD/Poli**).

Untuk melengkapi fungsionalitas end-to-end, disepakati dua amandemen tambahan:
1. **`IGD-DEC-309`**: Petugas admisi Rawat Inap dapat menolak pasien transfer dari antrean jika kondisi ruangan penuh atau pasien tidak memenuhi indikasi rawat inap di unit tujuan. Penolakan ini mewajibkan pengisian alasan penolakan dan memanggil fungsi pembatalan form IGD tanpa membatalkan disposisi DPJP IGD.
2. **`IGD-DEC-308`**: Penutupan kunjungan IGD harus berjalan otomatis dua arah. Jika pasien diantar fisik lebih dahulu ke bangsal rawat inap (dokumen kepergian SBAR diterima dan kedatangan dicatat) sebelum petugas admisi menyelesaikan proses admisi, maka saat transaksi *admit* dijalankan di Rawat Inap, fungsi evaluasi penutupan susulan IGD dipanggil agar status kunjungan IGD otomatis menjadi `Completed`.

Dokumen ini mendefinisikan secara presisi fungsi-fungsi backend IGD yang dipanggil oleh Rawat Inap dan spesifikasi layar/antarmuka yang perlu dikembangkan oleh tim Rawat Inap.

---

## 2. Kontrak Pemanggilan Fungsi Backend IGD oleh Rawat Inap

Sisi modul IGD telah menyediakan method publik di namespace `NewQuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Services`:

### 2.1 Fungsi Penolakan Form oleh Petugas Admisi

```csharp
namespace NewQuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Services;

public partial class EmergencyDispositionService
{
    public async Task<HasilBatalFormAdmisi> BatalkanFormPermintaanOlehPetugasAdmisiAsync(
        Guid dispositionId,
        string reason,
        Guid officerUserId,
        CancellationToken ct = default);
}
```

#### Spesifikasi Perilaku:
1. **Wewenang:** Dipanggil oleh service Rawat Inap (`InpAdmissionTransferService`) saat menangani endpoint `POST /admission-transfers/{id}/reject`.
2. **Transaksi & Konkurensi:**
   - Membuka transaksi dan mengambil advisory lock kunjungan `EMG_ADMISSION_REQUEST_{visitId}`.
   - Mengambil row lock baris `EmgDisposition` menggunakan `FOR UPDATE`.
3. **Validasi Status:**
   - Jika `EmgDisposition.Status == Executed` (sudah diproses admisi), fungsi mengembalikan error penolakan `409 Conflict`: *"Permintaan rawat inap tidak dapat ditolak karena proses admisi sudah dijalankan."*
   - Jika form IGD berstatus aktif (`Signed`), status form diubah menjadi `Cancelled` dengan `displayStatus = CancelledByAdmission`.
   - `CancellationReason` diisi dengan parameter `reason` yang diinput petugas admisi.
   - Baris riwayat audit `EmgAdmissionRequestEvent` dicatat dengan `EventType = AdmissionRejected (8)`.
4. **Keputusan Klinis IGD:**
   - Baris tindak lanjut `EmgDisposition.Status` **tetap `Confirmed`** (tidak dibatalkan, sesuai `IGD-DEC-309`).
   - Tombol *Buat Form Permintaan Rawat Inap* di IGD akan aktif kembali bagi perawat IGD.

---

### 2.2 Fungsi Evaluasi Penutupan Kunjungan IGD Susulan Dua Arah

```csharp
namespace NewQuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Services;

public partial class EmergencyDispositionService
{
    public async Task EvaluasiPenutupanKunjunganSusulanAsync(
        Guid visitId,
        Guid actorUserId,
        DateTime now,
        CancellationToken ct = default);
}
```

#### Spesifikasi Perilaku:
1. **Titik Pemanggilan:** Dipanggil di dalam transaksi *admit* Rawat Inap (`InpAdmissionTransferService.AdmitAsync`) tepat **setelah** status tindak lanjut `EmgDisposition.Status` diubah menjadi `Executed`.
2. **Evaluasi Penjaga Penutupan:**
   - Mengevaluasi apakah kunjungan IGD sudah tidak memiliki penahan (seluruh pesanan selesai/bersikap, observasi selesai, dan dokumen serah terima kepergian SBAR telah diterima serta kedatangan fisik dicatat di bangsal).
   - Jika seluruh penjaga terpenuhi, status kunjungan `EmgVisit.VisitStatus` diubah menjadi `Completed`, `VisitCompletedAt` diisi waktu saat ini, dan `ClosedByDispositionId` diisi dengan `dispositionId`.
   - Menjamin bahwa urutan *"antar fisik dulu, admisi belakangan"* tidak meninggalkan kunjungan IGD berstatus `AwaitingClosure`.

---

### 2.3 Fungsi Pembacaan Ringkasan Form untuk Antrean Transfer Rawat Inap

```csharp
namespace NewQuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Services;

public partial class EmergencyDispositionService
{
    public static async Task<RingkasanPermintaanRawatInap?> BacaRingkasanPermintaanRawatInapAsync(
        ApplicationDbContext db,
        Guid dispositionId,
        CancellationToken ct = default);
}

public sealed record RingkasanPermintaanRawatInap(
    Guid FormId,
    bool IsCito,
    bool NeedsIntensiveCare,
    bool NeedsIsolation,
    bool NeedsOxygenSupport,
    bool NeedsSpecialMonitoring,
    Guid? RequestedServiceUnitId,
    string? RequestedServiceUnitName,
    Guid? RequestedPatientClassId,
    string? RequestedPatientClassName,
    Guid? ProposedInpatientDoctorId,
    string? ProposedInpatientDoctorName,
    string? ClinicalIndication,
    string? CarePlanInstruction);
```

#### Spesifikasi Perilaku:
- Kueri baca-saja (`AsNoTracking`) yang efisien untuk dibaca oleh `InpAdmissionTransferService.Queries.cs` pada saat menyusun daftar transfer maupun saat mempersiapkan data *prefill* Proses Admisi.

---

## 3. Spesifikasi Pekerjaan Sisi Rawat Inap (Untuk Tim Muhammad Hamzah)

Berikut adalah daftar pekerjaan yang perlu diimplementasikan pada repositori modul Rawat Inap:

### 3.1 Backend Rawat Inap (`NewQuilvianSystemBackend`)

| No | Komponen | Deskripsi Pekerjaan |
| :---: | :--- | :--- |
| 1 | **Endpoint Tolak Admisi** | Buat endpoint: `POST /api/admission-transfers/{transferId}/reject`<br>- Body: `{"reason": "string (min 5, max 500 karakter)"}`<br>- Auth: Petugas admisi (`InpatientAdmissionTransfer : Update`)<br>- Memanggil `EmergencyDispositionService.BatalkanFormPermintaanOlehPetugasAdmisiAsync`<br>- Mengeluarkan pasien dari Antrean Transfer rawat inap aktif. |
| 2 | **Evaluasi Penutupan Susulan** | Di method transaksi `AdmitAsync` (saat menekan tombol simpan admisi):<br>- Tambahkan pemanggilan `await _emergencyDispositionService.EvaluasiPenutupanKunjunganSusulanAsync(visitId, actorId, DateTime.UtcNow, ct)` tepat sesudah disposisi menjadi `Executed`. |
| 3 | **Ekspos Indikator CITO & Kebutuhan Khusus** | Pada DTO daftar antrean transfer `AdmissionTransferQueueItemDto`:<br>- Sertakan `isCito` (boolean)<br>- Sertakan `needsIsolation` dan `needsIntensiveCare` (boolean)<br>- Sertakan usulan `requestedServiceUnitId`, `requestedPatientClassId`, dan `proposedDoctorId`. |

---

### 3.2 Frontend Rawat Inap (`QuilvianSystemFrontendDev`)

| No | Layar / Komponen | Deskripsi Tampilan & Interaksi |
| :---: | :--- | :--- |
| 1 | **Tombol Aksi Tolak di Antrean Transfer** | Pada baris antrean transfer pasien IGD:<br>- Tambahkan tombol/menu aksi bertuliskan **Tolak**.<br>- Menampilkan modal konfirmasi penolakan dengan input textarea: *"Alasan Penolakan Admisi (Wajib)"* (validasi min 5 karakter).<br>- Menembak endpoint `POST /api/admission-transfers/{transferId}/reject`.<br>- Setelah sukses, tabel antrean transfer me-refresh otomatis. |
| 2 | **Lencana CITO & Indikator Khusus** | Pada kolom prioritas/status di tabel antrean transfer:<br>- Tampilkan badge merah bertuliskan **CITO** jika `isCito == true`.<br>- Tampilkan ikon/penanda khusus jika pasien membutuhkan ICU atau Ruang Isolasi. |
| 3 | **Prefill Langkah Proses Admisi** | Saat petugas admisi menekan tombol **Proses Admisi**:<br>- Field Unit Layanan Rawat Inap terisi awal (*prefilled*) dengan unit usulan DPJP IGD.<br>- Field Kelas Perawatan terisi awal dengan kelas usulan form IGD.<br>- Field DPJP Rawat Inap terisi awal dengan dokter DPJP yang diusulkan IGD.<br>- Petugas admisi tetap memiliki kebebasan mengubah pilihan jika ketersediaan kamar tidak sesuai. |

---

## 4. Matriks Validasi & Respons Error

| Skenario | Endpoint | Respons HTTP | Keterangan Error |
| :--- | :--- | :---: | :--- |
| Petugas admisi menolak tanpa mengisi alasan | `POST …/reject` | `400 Bad Request` | *"Alasan penolakan admisi wajib diisi (minimal 5 karakter)."* |
| Petugas admisi menolak pasien yang sudah selesai di-admit | `POST …/reject` | `409 Conflict` | *"Permintaan rawat inap tidak dapat ditolak karena proses admisi sudah dijalankan."* |
| Petugas admisi menolak pasien yang formnya sudah dibatalkan perawat IGD | `POST …/reject` | `409 Conflict` | *"Form permintaan rawat inap sudah dibatalkan sebelumnya."* |
| Pasien transfer tidak ditemukan di antrean | `POST …/reject` | `404 Not Found` | *"Data transfer pasien tidak ditemukan."* |

---

## 5. Kontak & Koordinasi

- **Domain Owner IGD:** Rizki Gunawan
- **Domain Owner Rawat Inap:** Muhammad Hamzah
- **Dokumen Terkait:**
  - `NewQuilvianSystemBackend/docs/module-blueprints/igd/contracts/api-contract.md` (§12.8)
  - `NewQuilvianSystemBackend/docs/module-blueprints/igd/contracts/validation-matrix.md` (§14.9)
  - `NewQuilvianSystemBackend/docs/module-blueprints/igd/00-interview-decisions.md` (`IGD-DEC-308`, `IGD-DEC-309`)
