# Laporan Perubahan Backend — `BE-RWI-171`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-171` |
| Judul | Pemberitahuan reaksi transfusi ke Bank Darah (`K11`) |
| Slice | `MVP-4` |
| Roadmap | [`../../../roadmap/backend-roadmap-finishing.md`](../../../roadmap/backend-roadmap-finishing.md) — kartu `BE-RWI-171` |
| Trace | `FR-RWF-085` (reaksi); `RWI-DEC-203`, `RWI-DEC-209`; `INT-RWF-12`; `UAT-RWF-19` (bagian Bank Darah); `RWI-AC-303` |
| Contract version | `keperawatan` `0.6.0` **`approved`** (`RWI-DEC-221`): API 8.7; integrasi 9.4; backend 12.5, 12.7 |
| Dependency | `BE-RWI-170` ✅ |
| Klasifikasi | `HEAVY` — skor 10: repository 0, berkas diperiksa 1, berkas diubah 1, logika 2, kontrak API 2, database 2, keamanan 1, workflow 1 |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/BloodBankManagement/**`, `Areas/HealthServices/ClinicalManagement/**`, `Repositories/**`, `Program.cs`, `Migrations/**` |
| Model | Codex (implementasi awal); Claude Opus 5.5 (review, perbaikan, build akhir, migration, database update, laporan) |
| Commit backend saat dikerjakan | `f32b2308` (branch `MHamzah`), working tree |
| Tanggal | 5 Oktober 2026 |
| Status | ✅ Selesai. Kelima acceptance criteria terpetakan ke source; build akhir `PASS`; tabel diterapkan ke database development |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `BloodBankManagement` (kotak masuk, milik Sukma Giri Pratama, persetujuan `RWI-DEC-209`); `ClinicalManagement` (pengirim dan worker) |
| Prefix registry | `Bbk` — `ACTIVE`; `Cli` — `ACTIVE / LEGACY` |
| Keberlakuan | `NEW CODE` |
| QBE yang berlaku | `QBE-ENT-001`, `QBE-NAM-001`/`002`, `QBE-CFG-001`, `QBE-MOD-001`, `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001`, `QBE-TXN-001`, `QBE-DTO-001`, `QBE-CODE-004` (unique `ClinicalReactionId`) |
| Wewenang | Source: ya. Migration: ya (`AGENTS.md` 5 Oktober 2026). Database development: ya, instruksi pengguna 5 Oktober 2026. Deployment: tidak |
| Branch | `MHamzah`; tidak ada operasi Git tulis |

## 1. Masalah yang diperbaiki

Reaksi transfusi yang dicatat bangsal tidak sampai ke Bank Darah lewat sistem. Bank Darah baru tahu bila ditelepon, padahal Bank Darah perlu menahan kantong lain dari donor yang sama dan meminta sampel pemeriksaan.

## 2. Proses bisnis

**Tujuan.** Reaksi transfusi sampai di kotak masuk Bank Darah; pengiriman yang gagal dicoba ulang tiap menit tanpa menghilangkan catatan klinis; petugas Bank Darah menyatakan sudah menindaklanjuti.

**Pelaku.**

| Pelaku | Peran |
| --- | --- |
| Perawat bangsal | Mencatat reaksi pada monitoring transfusi (`BE-RWI-170`) |
| Sistem Clinical | Mengirim pemberitahuan sesudah reaksi tersimpan, dan worker pengiriman ulang tiap 1 menit |
| Petugas Bank Darah | Membaca kotak masuk dan menindaklanjuti (`TransfusionReactionNotice : Read`, `: Acknowledge`) |

**Langkah utama.**

1. Reaksi klinis di-commit lebih dulu dengan status pemberitahuan `Pending`.
2. Sesudah commit, Clinical memanggil `BbkTransfusionReactionNoticeService.ReceiveAsync` di scope tersendiri.
3. Bank Darah mengunci kunci per reaksi, memeriksa reaksi belum pernah diterima, memeriksa kantong memang diserahkan kepada pasien itu, lalu menyimpan pemberitahuan `New` beserta pasien, kantong, ringkasan reaksi, waktu kejadian, dan unit pelayanan.
4. Clinical menandai reaksi `Delivered` beserta id pemberitahuan; bila gagal, `Failed` dan jumlah percobaan bertambah.
5. Worker tiap menit mengirim ulang reaksi yang belum `Delivered`.
6. Petugas Bank Darah membuka kotak masuk, membaca detail, lalu menindaklanjuti dengan catatan; pelaku dan waktunya tersimpan.

**Contoh.** Pukul 10.40 Ns. Siti mencatat reaksi "menggigil" pada kantong PMI 2026-000123 milik pasien Budi di Bangsal Melati. Pemberitahuan muncul di kotak masuk Bank Darah dengan nama pasien, nomor kantong, reaksi, waktu 10.40, dan unit Melati. Petugas Bank Darah menindaklanjuti dengan catatan "sisa kantong dan sampel pasien diminta ke laboratorium". Tindak lanjut kedua ditolak `409`.

**Aturan bisnis.**

| Aturan | Akibat |
| --- | --- |
| Pemberitahuan per reaksi hanya satu | Unique `ClinicalReactionId` + kunci advisory; kiriman ulang mengembalikan id yang sama |
| Kantong bukan milik pasien pada pemberitahuan | Penerimaan ditolak, reaksi tetap tersimpan dan berstatus `Failed` |
| Sudah ditindaklanjuti | `409` |

**Perubahan status.**

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat |
| --- | --- | --- | --- | --- |
| Reaksi `Pending`/`Failed` | Kirim | `Delivered` atau `Failed` | Sistem | Sesudah reaksi commit |
| Pemberitahuan `New` | Tindak lanjut | `Acknowledged` | Petugas Bank Darah ber-`Acknowledge` | Belum ditindaklanjuti |

**Jalur tidak normal.** Bila penerimaan Bank Darah melempar kesalahan, reaksi klinis tidak hilang; statusnya `Failed` dan worker mengulang. Komunikasi darurat di luar sistem tetap prosedur klinis (G-23); notifikasi seketika `DEFERRED`.

**Hasil akhir.** Bank Darah melihat reaksi pada penyegaran kotak masuknya, dengan jejak tindak lanjut.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

API 8.7, integrasi 9.4, backend 12.5, persetujuan `RWI-DEC-209`, `BbkBloodOrderController` (pola atribut modul Bank Darah), `BbkBloodUnit`, dan registry hak akses.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/BloodBankManagement/Models/BbkTransfusionReactionNotice.cs` | Baru. Kotak masuk reaksi |
| `Areas/HealthServices/BloodBankManagement/Enums/BbkReactionNoticeStatus.cs` | Baru. `New`, `Acknowledged` |
| `Repositories/Configurations/HealthServices/BloodBankManagement/BbkTransfusionReactionNoticeConfigurations.cs` | Baru. Unique `ClinicalReactionId`, index `(Status, ReceivedAt)`, FK `Restrict` |
| `Areas/HealthServices/BloodBankManagement/DTOs/TransfusionReactionNoticeDtos.cs` | Baru |
| `Areas/HealthServices/BloodBankManagement/Services/BbkTransfusionReactionNoticeService.cs` | Baru. `ReceiveAsync` idempoten, daftar, detail, `AcknowledgeAsync` |
| `Areas/HealthServices/BloodBankManagement/Controllers/BbkTransfusionReactionNoticeController.cs` | Baru. Tiga endpoint. Review Claude: `moduleCode` dikoreksi dari `HEALTH_SERVICE_BLOOD_BANK` menjadi `HEALTH_SERVICE_BLOOD_BANK_MANAGEMENT` |
| `Areas/HealthServices/ClinicalManagement/Services/CliTransfusionReactionDeliveryService.cs` | Baru. Pengiriman sesudah commit dengan status `Delivered`/`Failed` |
| `Areas/HealthServices/ClinicalManagement/Services/CliTransfusionReactionNoticeWorker.cs` | Baru. Pengiriman ulang tiap 1 menit |
| `Repositories/ApplicationDbContext.cs`, `Program.cs` | DbSet `BbkTransfusionReactionNotices`; registrasi service dan worker |
| Migration gabungan `20261005071042_AddRawatInapKeperawatanFinishing` | Tabel `BbkTransfusionReactionNotice` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Tiga endpoint API 8.7; penerimaan dari Clinical lewat service di dalam aplikasi, bukan endpoint |
| Database | Tabel baru `BbkTransfusionReactionNotice`. Tidak ada perubahan tabel Bank Darah lain. **Diterapkan** ke database development 5 Oktober 2026 |
| Keamanan/Auth | Resource baru `TransfusionReactionNotice` (`Read`, `Acknowledge`) di modul Bank Darah yang sudah ada. Tanpa koreksi `moduleCode`, resource ini akan muncul sebagai modul terpisah "Health Service Blood Bank" di layar Akses Role |

## 4. Dokumentasi endpoint

#### Health Services / Blood Bank Management / Transfusion Reaction Notice

Base URL: `api/v1/health-services/blood-bank-management/transfusion-reaction-notices`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Kotak masuk reaksi transfusi | `TransfusionReactionNotice : Read` | `ReactionNoticeQuery` (`status`, `from`, `to`, paging) | `PagedResult<ReactionNoticeDetail>` |
| `GET` | `/{id}` | Detail: pasien, kantong, reaksi, waktu, unit | `TransfusionReactionNotice : Read` | — | `ReactionNoticeDetail` |
| `POST` | `/{id}/acknowledge` | Bank Darah menyatakan sudah menindaklanjuti | `TransfusionReactionNotice : Acknowledge` | `AcknowledgeReactionNoticeRequest` | `ReactionNoticeDetail` |

Kode status: `403` tidak berhak; `404` pemberitahuan tidak ditemukan; `409` sudah ditindaklanjuti.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -v minimal` — build akhir | `Build succeeded`, `0 Error(s)`, `233 Warning(s)` | `PASS` | Log build akhir; nol warning di berkas baru |
| `dotnet ef migrations add` dan `dotnet ef database update --no-build` | Tabel dan unique `IX_BbkTransfusionReactionNotice_ClinicalReactionId` di migration `20261005071042`; diterapkan `Done.`; nol `Pending` | `PASS` | Keluaran perintah |
| Pemeriksaan statis atribut hak akses dan kode modul | Tiga endpoint cocok `TransfusionReactionNotice` ↔ action; satu-satunya `moduleCode` Bank Darah kini `HEALTH_SERVICE_BLOOD_BANK_MANAGEMENT` | `PASS` | Skrip pemeriksa atribut dan pemeriksa kode modul |
| Verifikasi proses bisnis — kirim ganda | `ReceiveAsync` mengunci per reaksi dan mengembalikan pemberitahuan yang ada | `PASS` | `BbkTransfusionReactionNoticeService.ReceiveAsync` |
| Verifikasi proses bisnis — Bank Darah gagal menerima | Pengecualian ditangkap; reaksi `Failed`, `NoticeAttemptCount` naik; worker mengulang | `PASS` | `CliTransfusionReactionDeliveryService.DeliverAsync`; `CliTransfusionReactionNoticeWorker` |
| Uji bersama petugas Bank Darah dan UAT `UAT-RWF-19` | Tidak dijalankan | `NOT RUN` | Dikecualikan atas instruksi pengguna 5 Oktober 2026 |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual: `NOT FEASIBLE` pada sesi ini (aplikasi dan worker tidak dijalankan).

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Reaksi "menggigil" tersimpan → pemberitahuan muncul di kotak masuk Bank Darah dengan pasien, kantong, reaksi, waktu, dan unit | Terpenuhi | `RecordReactionAsync` → `DeliverAsync` → `ReceiveAsync`; `ReactionNoticeDetail` |
| 2. Bank Darah tidak terjangkau → `Failed`, worker mengirim ulang, reaksi klinis tetap tersimpan | Terpenuhi | `DeliverAsync` (reaksi sudah commit sebelumnya); worker tiap 1 menit |
| 3. Pengiriman ganda tidak membuat pemberitahuan dua kali | Terpenuhi | Unique `ClinicalReactionId` + kunci advisory |
| 4. Tindak lanjut menyimpan catatan dan pelaku | Terpenuhi | `AcknowledgeAsync` (`AcknowledgedByUserId`, `AcknowledgedAt`, `AcknowledgeNote`) |
| 5. Alur Bank Darah lain tidak berubah (`RWI-AC-303`) | Terpenuhi | Hanya tabel, service, dan controller baru; tidak ada perubahan berkas Bank Darah lain oleh task ini |
| DoD: build tanpa error; laporan; roadmap dan traceability | Terpenuhi | Bagian 5 |
| DoD: verifikasi disaksikan petugas Bank Darah | **Dikecualikan atas instruksi pengguna 5 Oktober 2026** | `NOT RUN` |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Admin perlu memberikan `TransfusionReactionNotice : Read`/`Acknowledge` kepada petugas Bank Darah lewat layar Akses Role |
| Masalah yang diketahui | `NONE` |
| Risiko tersisa | Belum disaksikan petugas Bank Darah; worker belum terlihat berjalan di runtime |
| Perubahan sampingan | Koreksi `moduleCode` controller (lihat 3.2) |
| Interupsi | Sesi Codex terhenti karena batas penggunaan penyedia; dilanjutkan Claude |
| Status Git | 151 entri `git status --short` (63 `M`, 3 `D`, 85 `??`); berkas task ini pada 3.2; tidak ada stage/commit/push |
| Langkah berikutnya | `FE-RWI-191` (kotak masuk Bank Darah); UAT `UAT-RWF-19` |
