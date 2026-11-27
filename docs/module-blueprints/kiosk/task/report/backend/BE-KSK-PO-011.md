# Laporan Perubahan Backend — `BE-KSK-PO-011`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-KSK-PO-011` |
| Judul | Faskes perujuk mitra di halaman publik |
| Slice | `EPIC KSK-PO-02` — halaman publik backend (`FR-KSK-PO-015`), roadmap r1.1, rilis `MVP-1` (bersama `FE-KSK-PO-009`) |
| Roadmap | [online-registration-backend-roadmap.md](../../../roadmap/online-registration-backend-roadmap.md) — kartu `BE-KSK-PO-011` |
| Trace | `KSK-DEC-057`, `058`; `KSK-DSN-PO-018`; `DEC-FRJ-001`, `RJ-VAL-PM-17` |
| Contract version | `KSK-CONTRACT-v2.1` — `approved` (Sukma Giri Pratama, 2026-11-25, `KSK-DEC-058`): *Perubahan v2.1* pada `api-contract.md`; `KSK-VAL-PO-073`, `074` |
| Dependency | `BE-KSK-PO-005` ✅ |
| Klasifikasi | `MEDIUM` — perubahan perilaku dua endpoint publik tanpa perubahan skema |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/RegistrationManagement/{Controllers,Services}` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `356bf463` (`sukmagp`) |
| Tanggal | 2026-11-25 |
| Status | ✅ Selesai — kedua aturan terbukti di runtime; satu kasus data (faskes `IsPartner = false`) tidak dapat dibuat lewat aplikasi dan dicatat sebagai pengecualian |

**Backend Governance Preflight**

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `RegistrationManagement` (Registration, `Reg`, `ACTIVE / LEGACY`) |
| Keberlakuan | `NEW CODE` (`OnlineRegistrationPublicService`, `OnlineRegistrationPublicController` milik Amendment PO). Membaca `ReferralPartnerEligibility` milik `HealthServices/MasterData` tanpa mengubahnya |
| QBE yang berlaku | `QBE-SVC-001` (aturan di service; controller hanya meneruskan query), `QBE-LOG-001` (log tanpa data pasien) |
| Hardcode role | Tidak ada. Endpoint publik tetap tanpa login dengan batas laju yang sama |
| Skema / migration | Tidak ada |

## 1. Masalah yang diperbaiki

Sebelum task ini, halaman publik menawarkan semua faskes aktif. Padahal jalur kunjungan RJ hanya menerima faskes mitra yang perjanjiannya berlaku pada tanggal kunjungan (`RJ-VAL-PM-17`). Akibatnya pasien bisa mendaftar dengan faskes yang pasti ditolak saat check-in di kiosk. Pemilik memutuskan halaman publik hanya menampilkan faskes mitra (`KSK-DEC-057`).

## 2. Proses bisnis

1. Pasien Rujukan memilih tanggal kunjungan di langkah Data Kunjungan, lalu mencari faskes di langkah Data Rujukan.
2. Pencarian faskes membawa tanggal kunjungan. Hanya faskes yang aktif, berstatus mitra, dan perjanjiannya berlaku pada tanggal itu yang muncul. Contoh nyata dari uji: "TEST PO011 Klinik Mitra Terbatas" dengan perjanjian sampai 5 Des muncul untuk kunjungan 5 Des, tetapi tidak muncul untuk 6 Des.
3. Saat kirim, backend memeriksa ulang faskes yang dikirim terhadap tanggal kunjungan. Faskes yang tidak memenuhi syarat ditolak dengan pesan: "Faskes perujuk ini belum bekerja sama dengan rumah sakit untuk tanggal kunjungan Anda. Pilih faskes lain, atau daftar di loket pendaftaran."
4. Check-in di kiosk tidak berubah: `RJ-VAL-PM-17` tetap memeriksa faskes pada hari H.

## 3. Perubahan yang dikerjakan

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/RegistrationManagement/Services/OnlineRegistrationPublicService.cs` | `GetReferralInstitutionOptionsAsync(search, visitDate, …)`: `visitDate` wajib dan H+1 s.d. H+30 (`400` `KSK-VAL-PO-074`, konstanta `ReferralVisitDateRequiredMessage`); saringan `ReferralPartnerEligibility.IsEligibleOn(visitDate)` menggantikan saringan aktif saja |
| `Areas/HealthServices/RegistrationManagement/Services/OnlineRegistrationPublicService.Submit.cs` | Validasi referensi: sesudah cek faskes aktif (`027`), faskes diperiksa dengan `IsEligibleOn(tanggal kunjungan)`; gagal → `400` `KSK-VAL-PO-073` (konstanta `ReferralInstitutionNotPartnerMessage`) |
| `Areas/HealthServices/RegistrationManagement/Controllers/OnlineRegistrationPublicController.cs` | Query `[FromQuery] DateTime? visitDate` pada `GET options/referral-institutions` |

Tidak ada perubahan tabel, DTO respons, hak akses, maupun batas laju.

## 4. Dokumentasi endpoint

### [Tags("Online Registration Public")]

Base URL: `api/v1/health-services/registration-management/online-registrations/public`

| Method | Path | Kegunaan | Hak akses | Request | Response | Kode status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/options/referral-institutions` | Cari faskes perujuk mitra pada tanggal kunjungan | Tanpa login, `OnlineRegistrationPublicOptions` | Query `search` (3–100 karakter), `visitDate` (`yyyy-MM-dd`, wajib, H+1 s.d. H+30) | `ApiResponse<List<{ id, code, name }>>`, maks. 20 | `200`; `400` "Pilih tanggal kunjungan lebih dulu." (`074`) atau "Ketik minimal 3 huruf nama atau kode faskes."; `429` |
| `POST` | `` | Kirim pendaftaran online | Tanpa login, captcha | Tidak berubah | Tidak berubah | Tambahan: `400` `KSK-VAL-PO-073` |

Contoh:

```http
GET …/public/options/referral-institutions?search=setiabudi&visitDate=2026-11-26
→ 200 { "data": [ { "code": "MMC-FRJ-001", "name": "Puskesmas Kecamatan Setiabudi" } ] }

GET …/public/options/referral-institutions?search=puskesmas
→ 400 { "message": "Pilih tanggal kunjungan lebih dulu." }
```

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -c Release -o <scratchpad>/be-build` | Build succeeded, 0 error; 0 warning di tiga berkas yang diubah | `PASS` | `build-po011.log` |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` | 3 berkas, VIOLATION 0, REVIEW 0 | `PASS` | Keluaran QBE |
| `tools/authorization-verifier/verify-authorization.sh --configuration Release` | PASS: fallback 68 tetap, endpoint telanjang baru 0, identitas wajib 24/24 | `PASS` | Keluaran verifier |
| O1–O3 runtime 7185 | Tanpa `visitDate`, `visitDate` hari ini, dan H+36 → `400` "Pilih tanggal kunjungan lebih dulu." (`074`) | `PASS` | `po011_test.py`, `po011-run.log` |
| O4 | `search` kurang dari 3 huruf tetap `400` dengan pesan lama | `PASS` | Idem |
| O5 | Mitra aktif dengan perjanjian berlaku (Puskesmas Kecamatan Setiabudi) muncul | `PASS` | Idem |
| O6, O7 | Faskes mitra uji dengan perjanjian 1 Nov–5 Des: muncul pada 26 Nov dan 5 Des (hari terakhir), **tidak** muncul pada 6 Des | `PASS` | Idem |
| O8 | Mitra dengan perjanjian kedaluwarsa (`MMC-FRJ-009`, 2025) dan mitra nonaktif (`MMC-FRJ-010`) tidak muncul | `PASS` | Idem |
| S2 | Kirim dengan faskes mitra yang perjanjiannya berakhir 5 Des untuk kunjungan 7 Des → `400` `KSK-VAL-PO-073`; tidak ada pendaftaran tersimpan | `PASS` | Idem |
| S3 | Kirim dengan faskes yang sama untuk kunjungan 5 Des → `200` (`PO-20261125-0010`) | `PASS` | Idem |
| S1, O6b — faskes `IsPartner = false` | Tidak dapat diuji dengan data nyata: API master faskes selalu menyimpan `IsPartner = true` (`ReferralMasterDataService` baris 405 dan 503), dan DB dev tidak punya faskes non-mitra. Faskes uji yang dikirim `isPartner: false` tersimpan sebagai mitra aktif, sehingga muncul dan diterima — perilaku yang benar untuk data itu | `NOT RUN` (dikecualikan) | `po011_nonpartner.py`, `po011-run.log` |
| Log 7185 | 476 baris; satu `ERR` penjadwal absensi HR yang tidak terkait; tanpa NIK dan kontak uji | `PASS` | `be7185-po011.log` |
| `dotnet test` | — | `NOT RUN` | Tidak ada project test (pola Bank Darah) |

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Baris uji `KSK-DEC-057` (1): mitra muncul hanya pada tanggal perjanjian berlaku; tanpa `visitDate` → `400` `074` | Terpenuhi | O1–O3, O5–O8 |
| Baris uji `KSK-DEC-057` (1): non-mitra tidak pernah muncul | Terpenuhi lewat saringan yang sama (`IsEligibleOn` mensyaratkan `IsPartner`); data `IsPartner = false` tidak dapat dibuat lewat aplikasi | O8 (kedaluwarsa, nonaktif), O7 (perjanjian berakhir); kode |
| Baris uji `KSK-DEC-057` (2): kirim dengan faskes non-mitra → `400` `073`, tidak tersimpan | Terpenuhi | S2 |
| Tambahan: faskes mitra tetap `200`; `search` dan batas laju tidak berubah | Terpenuhi | S3, O4 |
| DoD: tanpa migration; log tanpa data pasien | Terpenuhi | Bagian 3, log |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Perubahan kompatibilitas | Query `visitDate` wajib. Halaman publik yang sudah ter-deploy (sebelum `FE-KSK-PO-009`) akan menerima `400` saat mencari faskes; rilis bersama `FE-KSK-PO-009` |
| Risiko tersisa | Petugas yang memindah tanggal kunjungan (`PUT {id}/schedule`) tidak memeriksa ulang faskes; bila perjanjian tidak berlaku pada tanggal baru, check-in ditolak `RJ-VAL-PM-17` dan pasien diarahkan ke loket (sesuai `KSK-DEC-057` butir 3) |
| Data uji | Faskes `TEST-PO011-MT` dan `TEST-PO011-NM` (keduanya sudah dinonaktifkan), jadwal uji `TEST-PO005 PO011-*`, pendaftaran `PO-20261125-0010` (5 Des) dan `PO-20261125-0011` (7 Des, sudah dibatalkan). Ledger `po011_ledger.json`. Dibersihkan bersama data uji modul |
| Perubahan sampingan | `NONE` di repository |
| Migration/DB | Tidak ada migration. Tidak ada eksekusi DB langsung; semua tulis lewat API |
| Status Git | Tiga berkas backend diubah, belum di-commit. Agent tidak membuat commit |
| Langkah berikutnya | `FE-KSK-PO-009` (Data Rujukan mengirim `visitDate`) |
