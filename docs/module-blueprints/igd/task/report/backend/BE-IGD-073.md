# Laporan Perubahan Backend — `BE-IGD-073`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-IGD-073` |
| Judul | Permintaan admisi Rawat Inap menerima sumber IGD |
| Slice | `EPIC IGD-15` · `MVP-11` · R3.18 gelombang 2 — **perubahan milik Rawat Inap** |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian R3.18 |
| Requirement | `FR-IGD-113`, `115` (batal oleh petugas admisi); `AT-IGD-229`…`231` |
| Keputusan | `IGD-DEC-275`, `276`, `279`, `286`, `288`, `292` (`ARC-RI-02`), `299` (persetujuan lisan pemilik Rawat Inap Muhammad Hamzah dan owner Billing), `300` (`POST`), `301` (pengisian unit di layar admisi ditahan — tidak memengaruhi backend) |
| Contract version | API **`0.17.1`** §12.1 nomor 3–5, §12.4; validation **`0.16.0`** §14 aturan 20–22; state **`0.11.0`** §11.3; permission **`0.10.0`** §11; integration **`0.8.0`** §9.2; kamus data §10 (`InpAdmissionReferral`); `02-backend-architecture.md` §17.5–17.7 |
| Dependency | `BE-IGD-072` 🟡 — source dan migration 1 sudah ada (commit `f6b07432`) |
| Klasifikasi | `MEDIUM` — 7 berkas Rawat Inap disentuh, 1 endpoint baru, 1 migration (6 kolom + 1 kolom dilonggarkan), dipakai alur kamar pulih yang sudah berjalan |
| Task mode | `BACKEND` — izin pemilik 13 Oktober 2026: *"kita kerjakan dulu BE-IGD-073 … saya akan jalankan semua build dan migration add"* |
| Target tulis | `NewQuilvianSystemBackend` (branch `rizkiG`): berkas pada 3.2; laporan ini, roadmap, traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `9e0e517f` (`rizkiG`) + working tree |
| Tanggal | 13 Oktober 2026 |
| Status | ⛔ **DIBATALKAN — 14 Oktober 2026, `IGD-DEC-304`.** Seluruh perubahan source task ini dikembalikan ke `HEAD` (7 berkas Rawat Inap + 1 berkas Billing); migration 2 sudah di-rollback dari basis data dev dan berkasnya dihapus (14 Oktober 2026 17.11, dijalankan agent Antigravity atas perintah Rizki; bukti di `QuilvianSystemFrontendDev/test-with-agy/igd/rollback-migration-2-20261014/` (01–05)). Saringan antrean Transfer yang semula delta 10 task ini kini dicatat pada `BE-IGD-074` |

### Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `InPatientManagement` |
| Pemilik dan prefix registry | Rawat Inap, prefix **`Inp`**, `ACTIVE`; pemilik perubahan Muhammad Hamzah, persetujuan lisan tercatat `IGD-DEC-299` |
| Keberlakuan | `TOUCHED LEGACY` — seluruh berkas yang diubah sudah ada. Nol berkas baru |
| QBE ID yang berlaku | `QBE-ENT-001`, `QBE-NAM-001`, `QBE-CFG-001`, `QBE-MOD-001`, `QBE-MOD-002`, `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001`, `QBE-TXN-001`, `QBE-DTO-001` |
| `Program.cs` | Nol baris. `InpAdmissionReferralService` sudah terdaftar; pembacaan salinan beku IGD memakai method `static` `EmergencyDispositionService.BacaRingkasanPermintaanRawatInapAsync` — tanpa dependency DI baru, tanpa lingkaran |

---

## 1. Masalah yang diselesaikan

Antrean permintaan admisi Rawat Inap (`InpAdmissionReferral`) selama ini hanya menerima pasien dari **kamar pulih** Kamar
Operasi: setiap baris wajib menunjuk kasus OK (`OprCaseId`). Permintaan dari IGD tidak punya kasus OK, sehingga tidak bisa
masuk antrean yang sama, dan petugas admisi tidak punya cara membatalkan permintaan yang tidak dapat diterimanya.

Contoh. Form Tn. Budi ditandatangani dr. Andi pukul 10.15 (`BE-IGD-074`). Tanpa task ini, permintaan itu tidak punya
tempat di daftar petugas admisi Sari; Sari juga tidak bisa menolaknya ketika Kelas 2 Bangsal Melati penuh.

## 2. Proses bisnis

| Langkah | Pelaku | Yang terjadi | Keadaan permintaan |
| ---: | --- | --- | --- |
| 1 | Sistem (dalam transaksi tanda tangan IGD, `BE-IGD-074`) | `BuatDariIgdAsync` membuat permintaan bersumber IGD: unit, kelas, DPJP usulan, CITO, tingkat perawatan (`Icu` bila *Perawatan intensif*, selain itu `Inpatient`) | `Pending` |
| 2 | Petugas admisi | Daftar memuat permintaan kamar pulih dan IGD; **CITO paling atas**, lalu terlama menunggu; saringan `sourceKind` | `Pending` |
| 3 | Petugas admisi | Membuka rincian; untuk sumber IGD tampil blok `emergencyRequest` dari salinan beku form: diagnosis, alergi, indikasi, rencana, kebutuhan khusus, lama rawat, penanda tangan | `Pending` |
| 4a | Petugas admisi | Membuka admisi dari permintaan (alur yang sudah ada) | `Completed` |
| 4b | Petugas admisi | `POST /{id}/cancel` beralasan, misalnya *"Kelas 2 penuh"* | `Cancelled`, `cancellationSource = AdmissionOfficer` |
| — | Sistem (`BE-IGD-074`) | Perawat membatalkan form terkirim atau tindak lanjut `RANAP` dibatalkan → `BatalkanDariIgdAsync` | `Cancelled`, `EmergencyNurse` / `EmergencyDispositionCancelled` |

**Jalur tidak normal.**

| Keadaan | Hasil |
| --- | --- |
| Admisi biasa untuk pasien yang punya permintaan `Pending` dari sumber mana pun | `409` `INP-ADM-REF-001` *"Pasien punya permintaan admisi yang menunggu. Buka admisi dari permintaan itu."* (aturan 21 — pesan dinetralkan, kode tetap) |
| Pasien sudah punya permintaan `Pending` saat form IGD ditandatangani | `BuatDariIgdAsync` menolak dengan `INP-ADM-REF-001`; IGD memetakannya ke pesan aturan 4 (`BE-IGD-074`) |
| Form IGD dibatalkan sesudah admisi dibuka | `BatalkanDariIgdAsync` menolak dengan `INP-ADM-REF-002`; IGD memetakannya ke pesan aturan 19 |
| Petugas admisi membatalkan permintaan yang sudah selesai/batal | `409` *"Permintaan admisi ini sudah selesai atau dibatalkan."* |
| Alasan kosong atau > 500 karakter | `400` *"Alasan pembatalan wajib diisi, paling panjang 500 karakter."* |

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kartu `BE-IGD-073`; `02-backend-architecture.md` §17.4–17.7; API §12.1, §12.4; validation §14.5; state §11.3; integration
§9.2; kamus data §10; source: `InpAdmissionReferral.cs`, `InpAdmissionReferralStatus.cs`,
`InpAdmissionReferralConfiguration.cs`, `InpAdmissionReferralService.cs`, `InpatientAdmissionReferralController.cs`,
`InpatientAdmissionReferralDtos.cs`, `InpEpisodeService.cs` (`:172`, `:363`), `BillingInpatientEventReceiver.cs` (`:252`),
`InpIntegrationReplayService.cs` (`:148`), `InpatientMonitoringController.cs` (`:300`), `InpatientActorClaims.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/InPatientManagement/Enums/InpAdmissionReferralStatus.cs` | Dua enum baru: `InpAdmissionReferralSourceKind` (`RecoveryRoom = 1`, `Emergency = 2`), `InpAdmissionReferralCancellationSource` (`RecoveryDecisionChanged = 1`, `EmergencyNurse = 2`, `EmergencyDispositionCancelled = 3`, `AdmissionOfficer = 4`) |
| `…/Models/InpAdmissionReferral.cs` | `SourceKind` (bawaan `RecoveryRoom`); `OprCaseId` → `Guid?`; `EmergencyAdmissionRequestId`, `RequestedServiceUnitId`, `RequestedPatientClassId`, `IsCito`, `CancellationSource`; tiga navigasi |
| `Repositories/Configurations/HealthServices/InPatientManagement/InpAdmissionReferralConfiguration.cs` | Check `CK_InpAdmissionReferral_Source`; `SourceKind` bawaan 1; tiga FK `Restrict`; unique bersaring `UX_InpAdmissionReferral_EmergencyRequest`; index `IX_InpAdmissionReferral_Status_Cito_RequestedAt`. Index dan constraint lama tidak berubah — satu `Pending` per pasien kini berlaku untuk kedua sumber |
| `…/DTOs/InpatientAdmissionReferralDtos.cs` | Saringan `SourceKind`; `OprCaseId` boleh kosong; ruas respons baru; `AdmissionReferralEmergencyRequestResponse`; `CancelAdmissionReferralRequest`; record pertukaran IGD `EmergencyAdmissionReferralCreate`, `EmergencyAdmissionReferralState` |
| `…/Services/InpAdmissionReferralService.cs` | `BuatDariIgdAsync`, `BatalkanDariIgdAsync` (menyimpan di dalam transaksi pemanggil, tanpa membuka transaksi sendiri), `BatalkanOlehPetugasAdmisiAsync`, `AmbilKeadaanUntukIgdAsync`; daftar CITO dulu; saringan sumber; blok `emergencyRequest`; pesan `409` netral; jalur kamar pulih menandai `SourceKind = RecoveryRoom` dan `CancellationSource = RecoveryDecisionChanged` |
| `…/Services/InpEpisodeService.cs` | Pesan sukses admisi membedakan sumber: *"…dari permintaan admisi IGD."* / tetap *"…dari permintaan admisi kamar pulih."* |
| `…/Controllers/InpatientAdmissionReferralController.cs` | `POST /{id}/cancel` — `InpatientAdmissionReferral : Cancel` (`AccessType = Update`) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Aditif: `POST /admission-referrals/{id}/cancel`, query `sourceKind`, ruas respons baru. Memutus kecil sesuai kontrak §12.1: `oprCaseId` kini boleh `null`; pesan `INP-ADM-REF-001` berubah teks (kode tetap); urutan daftar CITO dulu — juga berlaku pada Daftar Pantau (`InpatientMonitoringController`) karena memakai `GetPagedAsync` yang sama |
| Database | Migration 2 `20261013100736_ExtendInpatientAdmissionReferralForEmergency` sudah dibuat Rizki (bagian 5); penerapannya ke basis data dev belum dilaporkan. Selama belum diterapkan, model EF lebih maju daripada skema: daftar/rincian permintaan admisi, pembukaan admisi dari permintaan, dan **simpan keputusan kamar pulih di Kamar Operasi** akan `500` `42703 column … does not exist` |
| Keamanan/Auth | Butir hak akses baru `InpatientAdmissionReferral : Cancel` — diberikan pengurus Akses Role kepada petugas admisi, bukan agent. Log audit pembatalan tanpa alasan (sensitif) |

### 3.4 Delta terhadap kontrak yang disetujui

| No | Kontrak | Implementasi | Alasan |
| ---: | --- | --- | --- |
| 1 | Aturan 20: petugas admisi membatalkan permintaan `Pending` | **Hanya permintaan bersumber IGD**; sumber kamar pulih → `409` *"Permintaan admisi dari kamar pulih dibatalkan lewat keputusan kamar pulih di Kamar Operasi."* | `IGD-DEC-279` hanya memutuskan permintaan IGD; membatalkan permintaan kamar pulih dari admisi belum pernah diputuskan pemilik Rawat Inap/Kamar Operasi, dan acceptance 6 menuntut alur kamar pulih tidak berubah. **Perlu konfirmasi Muhammad Hamzah** |
| 2 | Tanpa pesan untuk `rowVersion` basi pada pembatalan | `409` *"Permintaan admisi sudah diubah orang lain. Muat ulang lalu coba lagi."* | Permukaan teknis, pola aturan 26 |
| 3 | Pesan sukses admisi tidak dibahas | Sumber IGD: *"Admisi berhasil dibuka dari permintaan admisi IGD."* | Pesan lama menyebut kamar pulih untuk semua admisi dari permintaan |
| 4 | Ruas respons | Tambahan `cancelledAt`; blok `emergencyRequest` berisi `capturedAt`, nomor kunjungan IGD dan encounter, `diagnoses[]`, `allergyRecorded` + `allergies[]`, indikasi, rencana, empat kebutuhan khusus, lama rawat, penanda tangan | Rincian bentuk blok yang di kontrak disebut ringkas |
| 5 | Desain §17.5 menyebut `AddColumn ×5` | **6 kolom** (`SourceKind`, `EmergencyAdmissionRequestId`, `RequestedServiceUnitId`, `RequestedPatientClassId`, `IsCito`, `CancellationSource`) | Salah hitung di desain; daftar kolomnya sendiri benar |
| 6 | Penolakan dari `BuatDariIgdAsync`/`BatalkanDariIgdAsync` | Dikembalikan sebagai kode `INP-ADM-REF-001`/`002` dalam `EmergencyAdmissionReferralChange`; IGD yang memetakan ke pesan validation §14 | Pesan IGD milik IGD; kode Rawat Inap dipakai ulang |
| 7 | — | `BuatDariIgdAsync` **tidak** memeriksa episode rawat inap yang sedang berjalan (jalur kamar pulih memeriksanya) | Tidak ada keputusan untuk sumber IGD; satu `Pending` per pasien tetap dijaga index |
| 8 | — | Pembuatan/pembatalan dari IGD tidak menulis log audit Rawat Inap | Keduanya berjalan di dalam transaksi IGD; log ditulis IGD sesudah commit (`BE-IGD-074`), supaya tidak ada log untuk transaksi yang batal |
| 10 | — (ditambahkan 14 Oktober 2026, `IGD-DEC-302`, menutup `IGD-CONF-21`) | `InpAdmissionTransferService.Queries.cs` `TransferQuery`: saringan `!x.DispositionType.RequiresAdmissionRequest` (+1 baris). Antrean, rincian, dan *admit* Admisi Transfer IGD & Poliklinik tidak lagi memuat tindak lanjut IGD yang butuh form; *admit* Transfer untuk tindak lanjut itu menjadi `404`. Jalur Poliklinik tidak berubah. QBE `-Mode Strict` `PASS`, CRLF, nol komentar | Jalur Transfer milik Rawat Inap (`EPS-006`) membuka admisi pasien IGD tanpa form dan TTD DPJP lalu men-set tindak lanjut *Executed*, melewati `IGD-DEC-275`/`277`. **Konfirmasi Muhammad Hamzah menunggu** sebelum rilis; butuh build ulang backend |
| 9 | — | Komentar XML lama di controller, model, dan enum masih menyebut "hanya kamar pulih" | Aturan nol penyuntingan komentar lama; dicatat sebagai utang dokumentasi untuk pemilik Rawat Inap |

## 4. Dokumentasi endpoint

#### Health Services / Inpatient Management / Inpatient Admission Referral

Base URL: `/api/v1/health-services/inpatient-management/admission-referrals`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/?status=&sourceKind=&overdueOnly=&search=` | Daftar; **CITO dulu**, lalu terlama; ruas baru | `InpatientAdmissionReferral : Read` (sudah ada) |
| `GET` | `/{id}` | Rincian; sumber IGD memuat `emergencyRequest` dari salinan beku | `InpatientAdmissionReferral : Read` (sudah ada) |
| `POST` | `/{id}/cancel` | **Baru** — petugas admisi membatalkan permintaan IGD; body `{ "reason": "Kelas 2 penuh", "rowVersion": "…" }` | `InpatientAdmissionReferral : Cancel` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| QBE checker `-Mode Strict` atas 7 berkas | `VIOLATION 0`, `REVIEW 0`, `INFO 0`, *Final result: PASS* | `PASS` | 13 Oktober 2026 |
| Akhiran baris (hitungan byte Node) | 7 berkas CRLF, tanpa BOM, nol LF tunggal | `PASS` | Skrip hitung byte |
| Komentar baru | Nol `//`, `///`, `/* */` pada baris tambahan `git diff -U0` | `PASS` | `grep` |
| `Program.cs` | Tidak berubah | `PASS` | `git status --short` |
| Pemakai lain `InpAdmissionReferral` | Billing (`SourceEncounterId`, `CompletedEpisodeId`), replay integrasi (`CompletedEpisodeId`), Kamar Operasi (lewat service) — tidak memakai `OprCaseId` sebagai `Guid` wajib; tabrakan nama tipe dari `using` IGD: nihil | `PASS` | `grep` |
| `dotnet build` (Rizki) | `QuilvianSystemBackend.dll` 13 Oktober 2026 17.09.36 WIB, sesudah berkas migration 2 (17.07.36 WIB) dan sesudah seluruh berkas source task; jumlah warning belum dilaporkan | `PASS` (0 error), warning `NOT REPORTED` | Waktu berkas `bin/` dan `obj/` |
| Isi migration 2 dibandingkan bagian 7 | `AlterColumn` 1 (`OprCaseId` nullable), `AddColumn` 6, `CreateIndex` 4, `AddCheckConstraint` 1, `AddForeignKey` 3 (semua `Restrict`); nol `UPDATE`; nol operasi tabel modul lain | `PASS` | Pembacaan `20261013100736_ExtendInpatientAdmissionReferralForEmergency.cs` |
| Snapshot EF | `+55/−3`, seluruhnya di blok `InpAdmissionReferral` (properti, index, check, tiga relasi; `OprCase` tidak lagi `IsRequired`) | `PASS` | `git diff --stat` dan isi diff |
| Akhiran baris migration 2 | `.cs` dan `.Designer.cs` ber-BOM, CRLF penuh, nol LF tunggal | `PASS` | Hitungan byte Node |
| Penjaga `Down()` migration 2 | Disisipkan agent 14 Oktober 2026 atas permintaan pemilik; `migrationBuilder.Sql` = **1**; BOM dan CRLF utuh. Belum diuji `Up→Down→Up` | `PASS` (isi berkas), uji `NOT RUN` | Hitungan byte Node + `git diff` |
| `database update` migration 2 (Rizki, 14 Oktober 2026) | *"No migrations were applied. The database is already up to date."* — migration 2 sudah tercatat diterapkan di basis data dev | `PASS` | Tangkapan layar pemilik; agent tidak menjalankan kueri apa pun |
| `dotnet build -p:RunAnalyzers=false` (Rizki, 14 Oktober 2026) | `Build succeeded`, 0 error; 10 warning = 5 peringatan NuGet `NU1902`/`NU1903` (kerentanan paket `SixLabors.ImageSharp` 3.1.11) yang tercetak dua kali (restore dan build); tangkapan layar build inkremental 1,5 detik sehingga warning kompilator (`CS`) dari kompilasi DLL 09.00.11 tidak tampil. DLL `bin/` dan `obj/` 14 Oktober 2026 09.00.11 WIB — sesudah seluruh berkas source dan kedua penjaga `Down()` (08.55.20); literal baru task ditemukan di DLL | `PASS` (0 error), warning dilaporkan | Tangkapan layar pemilik + cap waktu DLL + pencarian literal UTF-16 di DLL |
| Uji API acceptance 1–6 | Belum | `NOT RUN` | Regresi kamar pulih (5, 6) dapat diuji sesudah migration; sumber IGD (2–4) pada putaran B sesudah `BE-IGD-074` |

Uji manual: `REQUIRED` — putaran B, bersama `BE-IGD-074` dan `FE-IGD-062`.

## 6. Acceptance criteria dan Definition of Done

| # | Kriteria | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | Kolom dan constraint sesuai kamus data §10; baris lama tetap sah sebagai sumber kamar pulih | Source terpenuhi; migration dan uji belum | Konfigurasi; `SourceKind` bawaan 1 membuat baris lama lolos `CK_InpAdmissionReferral_Source` |
| 2 | Daftar memuat kedua sumber, CITO paling atas lalu terlama; ruas respons baru terisi; sumber IGD tanpa kasus OK tidak membuat galat | Source terpenuhi; uji belum | `GetPagedAsync` urutan dan saringan; `Project` null-safe untuk `OprCase` |
| 3 | Rincian sumber IGD memuat blok `emergencyRequest` dari salinan beku | Source terpenuhi; uji belum | `GetByIdAsync` → `BacaRingkasanPermintaanRawatInapAsync` → `MapEmergencySnapshot` |
| 4 | Petugas admisi membatalkan beralasan (aturan 20); sumber pembatalan tercatat | Source terpenuhi (delta 1); uji belum | `BatalkanOlehPetugasAdmisiAsync`, `CancellationSource = AdmissionOfficer` |
| 5 | Admisi tanpa permintaan untuk pasien ber-permintaan `Pending` → `409` `INP-ADM-REF-001` dengan pesan netral | Source terpenuhi; uji belum | `CheckForAdmissionAsync` |
| 6 | Regresi kamar pulih: buat, batal, dan admisi dari permintaan kamar pulih tidak berubah | Source terpenuhi; uji belum | Jalur kamar pulih hanya menambah `SourceKind` dan `CancellationSource`; pesan sukses tetap |
| 7 | Migration 2 diterapkan pemilik sesudah migration 1; `Down()` berpenjaga | **Sebagian** — berkas dibuat dan isinya cocok; diterapkan di dev (14 Oktober); penjaga `Down()` disisipkan, belum diuji | Bagian 5 |
| 8 | Diff hanya berkas Cakupan; nol `Program.cs`; nol komentar baru; akhiran baris benar | Terpenuhi | Bagian 5 |
| 9 | Build 0 error; jumlah warning dilaporkan | Terpenuhi — build 14 Oktober 2026: 0 error, 10 warning (5 peringatan paket NuGet tercetak dua kali) | Bagian 5 |

DoD: laporan ini ✅; persetujuan Rawat Inap tercatat (`IGD-DEC-299`); acceptance 1–6 terpetakan ke source; 7 dan 9 sebagian
(uji penjaga `Down()`); 9 terpenuhi; uji API pada putaran B.

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | **Jangan menjalankan build ini ke basis data dev sebelum migration 2 diterapkan** — selain layar admisi, simpan keputusan kamar pulih di Kamar Operasi ikut gagal |
| Masalah yang diketahui | Delta 1 menunggu konfirmasi pemilik Rawat Inap. Penautan invoice untuk sumber IGD belum ditahan — itu `BE-IGD-075`; sebelum `BE-IGD-074` tidak ada permintaan IGD yang dapat lahir, jadi tidak berdampak |
| Risiko tersisa | Tabel dipakai Billing dan Kamar Operasi; regresi kamar pulih wajib diuji sesudah migration |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | 7 `M` di `Areas/HealthServices/InPatientManagement/**` dan `Repositories/Configurations/HealthServices/InPatientManagement/**`; snapshot EF `M` dan dua berkas migration 2 `??` dari Rizki; nol stage, nol commit |
| Langkah berikutnya | Tidak ada — task dibatalkan dan pembongkarannya tuntas |

### Migration untuk Rizki — `ExtendInpatientAdmissionReferralForEmergency`

Wajib sesudah migration 1 (`20261012080102_AddEmergencyAdmissionRequest`), karena FK baru menunjuk `EmgAdmissionRequest`.

```bash
dotnet ef migrations add ExtendInpatientAdmissionReferralForEmergency
dotnet build -p:RunAnalyzers=false
```

**Isi yang diharapkan** — bila muncul operasi tabel modul lain, hentikan dan periksa snapshot lebih dulu:

| Operasi | Isi |
| --- | --- |
| `AlterColumn` | `InpAdmissionReferral.OprCaseId` `uuid` → **nullable** |
| `AddColumn` ×6 | `SourceKind` `integer NOT NULL DEFAULT 1`; `EmergencyAdmissionRequestId` `uuid NULL`; `RequestedServiceUnitId` `uuid NULL`; `RequestedPatientClassId` `uuid NULL`; `IsCito` `boolean NOT NULL DEFAULT false`; `CancellationSource` `integer NULL` |
| `AddForeignKey` ×3 | → `EmgAdmissionRequest`, `MstServiceUnit`, `MstPatientClass`; semua `Restrict` |
| `CreateIndex` | `UX_InpAdmissionReferral_EmergencyRequest` (unik, saring `"EmergencyAdmissionRequestId" IS NOT NULL`), `IX_InpAdmissionReferral_Status_Cito_RequestedAt`, index FK `RequestedServiceUnitId` dan `RequestedPatientClassId` yang dibangkitkan EF |
| `AddCheckConstraint` | `CK_InpAdmissionReferral_Source` |

Tidak ada `UPDATE` data: baris lama otomatis `SourceKind = 1` dari nilai bawaan, dan seluruhnya sudah punya `OprCaseId`.

**Tempel manual di awal `Down()`:**

```csharp
migrationBuilder.Sql("""
    DO $$
    BEGIN
        IF EXISTS (SELECT 1 FROM public."InpAdmissionReferral" WHERE "SourceKind" = 2 OR "OprCaseId" IS NULL) THEN
            RAISE EXCEPTION 'Down BE-IGD-073 dihentikan: InpAdmissionReferral sudah berisi permintaan admisi bersumber IGD. Mengembalikan OprCaseId menjadi wajib akan gagal dan jejak permintaan dari IGD hilang.';
        END IF;
    END $$;
    """);
```

Sebelum `database update`, pastikan berkas migration 2 berisi tepat **1** `migrationBuilder.Sql`.

## Pembatalan — 14 Oktober 2026 (`IGD-DEC-304`)

Owner menetapkan tugas IGD berakhir saat pasien masuk Antrean Transfer IGD/Poli di *Admisi Rawat Inap* milik Muhammad Hamzah. Jalur permintaan admisi bersumber IGD yang dibangun task ini tidak dipakai. Tujuh berkas Rawat Inap (`InpatientAdmissionReferralController`, DTO, enum status, model, service, `InpEpisodeService`, configuration) dikembalikan dengan `git restore --source=HEAD`. Migration 2 `20261013100736_ExtendInpatientAdmissionReferralForEmergency` beserta perubahan snapshot dihapus oleh Rizki: `dotnet ef database update AddEmergencyAdmissionRequest`, lalu `dotnet ef migrations remove`. Bila penjaga `Down()` menolak karena ada baris permintaan bersumber IGD dari uji putaran B, Rizki membersihkan baris itu sendiri di basis data miliknya; agent tidak menjalankan kueri.

Validasi pembongkaran: `git status` menunjukkan berkas task ini kembali sama dengan `HEAD`. Dokumen kontrak yang memuat jalur lama dicatat perlu amandemen pada `IGD-DEC-304`.

## Penyelesaian pembongkaran — 14 Oktober 2026 sore

Dijalankan agent Antigravity atas perintah Rizki memakai prompt dari agent pengembang; bukti di `QuilvianSystemFrontendDev/test-with-agy/igd/rollback-migration-2-20261014/` (01–05). Agent pengembang memeriksa berkas bukti dan keadaan repository sendiri; keadaan basis data dibaca dari berkas bukti, tidak dikueri ulang.

| Langkah | Hasil | Bukti |
| --- | --- | --- |
| Baca baris penahan `Down()` | 4 baris `SourceKind = 2`; 0 tautan `BilInvoiceEncounterLink`; migration 2 tercatat di riwayat | `01-cek-penahan.txt` |
| Hapus baris penahan (blok `DO` yang membandingkan jumlah terhapus dengan 4) | Berhasil, tanpa exception | `02-hapus-penahan.txt` |
| `dotnet ef database update AddEmergencyAdmissionRequest --no-build` | *Reverting migration '20261013100736_ExtendInpatientAdmissionReferralForEmergency'*, *Done* | `03-rollback-ef.txt` |
| Verifikasi basis data | Riwayat berakhir di `20261012080102_AddEmergencyAdmissionRequest`; dari tujuh kolom yang dicek tinggal `OprCaseId` dengan `is_nullable = NO` | `04-verifikasi-db.txt` |
| Hapus dua berkas migration 2 dan `git restore` snapshot | `git status -- Migrations/` tinggal migration 1 (+9 baris penjaga `Down()`); snapshot 819 blok `ToTable`, 0 `SourceKind` — dicek ulang agent pengembang | `05-verifikasi-snapshot.txt`; `git status` |
| Build Rizki `dotnet build -p:RunAnalyzers=false` | DLL 17.17.01, lebih baru dari suntingan source terakhir (16.27); literal migration 2, `BuatDariIgdAsync`, `EmergencyAdmissionReferralState`, dan `UX_InpAdmissionReferral_EmergencyRequest` tidak ada lagi di DLL | Pemeriksaan DLL oleh agent pengembang |

Baris `InpAdmissionReferral` yang dihapus (data uji putaran B):

| Id | Status | PatientId | EmergencyAdmissionRequestId | CompletedEpisodeId |
| --- | --- | --- | --- | --- |
| `71f7fb8f-9ad6-48d7-80bb-d5be9607e0d2` | 3 dibatalkan | `4411cf93-fcca-49b5-8e63-1b08c15a9e32` | `98dd1691-2eeb-4849-9326-b130116b8095` | — |
| `7f0679ec-4b2b-428b-b972-7894dd9c21a8` | 3 dibatalkan | `4411cf93-fcca-49b5-8e63-1b08c15a9e32` | `0220d62c-bf65-4b44-a8b1-51ad198ea1d2` | — |
| `63581d8e-9d19-413b-a1b3-90b6bdc5952d` | 2 selesai | `4411cf93-fcca-49b5-8e63-1b08c15a9e32` | `82773d03-2941-489d-b060-2db1e133316e` | `8e57a959-ea38-4691-9bab-a993cbd47856` |
| `c25ebc9d-87f3-40f6-b146-a03fed7af3c4` | 3 dibatalkan | `673247d6-7674-417b-b204-f9bc0cf311df` | `d2ab6885-97ba-4b55-bddc-6dc9be212881` | — |

Sisa data uji: pasien `4411cf93-…` masih punya episode rawat inap `8e57a959-…` yang dibuka lewat jalur lama. Hindari pasien ini pada uji alur baru, atau tutup episodenya dari aplikasi. Bukti Antigravity tidak memuat daftar seluruh perintah yang dijalankan seperti yang diminta prompt; keluaran yang tersimpan konsisten dengan keadaan repository.
