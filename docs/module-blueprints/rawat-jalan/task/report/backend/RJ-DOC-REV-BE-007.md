# Laporan Perubahan Backend — `RJ-DOC-REV-BE-007`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `RJ-DOC-REV-BE-007` |
| Judul | Satu kunjungan aktif per pasien |
| Roadmap | `rawat-jalan/roadmap/doctor-consultation-roadmap.md` bagian `10` |
| Trace | Catatan pemilik 1 Okt 2026, Prioritas Screening butir 3; `RJ-DOC-DEC-010` (c) |
| Kontrak | Tanpa perubahan bentuk request/response. Perilaku baru: `400` pada `POST /patient-encounters`, `/kiosk`, `/admin` saat ada kunjungan aktif |
| Task mode | `BACKEND` |
| Baseline | BE `27fd8fb4` (`sukmagp`), bersih sebelum task |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Tanggal | 1 Oktober 2026 |
| Status | ✅ SELESAI — AC 1–3 terbukti runtime |

## 1. Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `RegistrationManagement` (controller) dan `MedicalRecordManagement` (definisi kunjungan aktif) |
| Keberlakuan | `TOUCHED LEGACY` — controller existing; tanpa entity, tabel, atau prefix baru |
| Registry | Tidak ada entity baru, sehingga `QBE-MOD-002` tidak berlaku |
| QBE yang berlaku | Conformance Strict pada dua berkas yang disentuh |
| Hak akses | Endpoint tidak berubah; `[AccessAction]`/`[AccessPermission]` existing tetap. Tanpa hardcode role |
| Migration / DB | Tidak ada |

## 2. Perubahan

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/MedicalRecordManagement/Services/MedicalRecordAccessAuditService.cs` | `KunjunganMasihBerjalan` dari `private` menjadi `internal`, supaya pendaftaran memakai **satu** definisi "kunjungan masih berjalan" (belum dihapus, belum batal, `CompletedAt` kosong, status bukan Selesai/Batal/Tidak Hadir). Komentar diperbarui |
| `Areas/HealthServices/RegistrationManagement/Controllers/PatientEncounterController.cs` | (1) `ValidateCreateRequestAsync` menolak bila `FindActiveEncounterAsync` menemukan kunjungan aktif. (2) Di dalam transaction create: `pg_advisory_xact_lock(hashtext("REG_ENCOUNTER_ACTIVE_{patientId}"))`, lalu cek ulang, supaya dua permintaan bersamaan tidak sama-sama lolos (pola `AccJournalService`). Pesan: "Pasien masih memiliki kunjungan aktif bernomor {nomor} tanggal {dd MMM yyyy}. Selesaikan atau batalkan kunjungan tersebut sebelum mendaftar ke poliklinik atau layanan lain." |

Diff: 2 berkas, +54 / −1.

## 3. Validasi

| Perintah / bukti | Hasil | Status |
| --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -c Release -nodeReuse:false -p:UseSharedCompilation=false` | Exit 0, `0 Error(s)`, 244 warning, 1 m 57 s. Satu-satunya warning di berkas task adalah `CS8619` existing (`PatientEncounterController.cs:2422`, bergeser karena baris baru) | `PASS` |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` × 2 berkas | `VIOLATION 0`, `REVIEW 0`, `Final result: PASS` | `PASS` |
| `AUTOMATED TEST` | `NOT APPLICABLE` — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`) | — |

### Uji runtime

Lingkungan: app `bin/Release/net9.0`, `Development`, `Runtime__Role=Web`, `http://localhost:5199`, DB `QuilvianNewDevSukma`. Akun seed SuperAdmin (lolos `KioskRead`), autentikasi lewat cookie `quilvian_access_token`; kredensial tidak dicetak. Pasien uji `KSKTEST-RM-07` (P7), service unit `fddbe4ae-…`, Tunai.

| ID | Skenario | Hasil | Status |
| --- | --- | --- | --- |
| R0 | Login | `200` | `PASS` |
| R1 | `POST /kiosk` kunjungan pertama | `200`, `ENC-RSMMC-00178` | `PASS` |
| R2 | `POST /kiosk` lagi saat 00178 aktif | `400` "Pasien masih memiliki kunjungan aktif bernomor ENC-RSMMC-00178 tanggal 01 Okt 2026. …" | `PASS` |
| R3 | `POST /admin` saat 00178 aktif | `400`, pesan sama | `PASS` |
| R4 | `PATCH /{id}/cancel` 00178, lalu `POST /kiosk` | cancel `200`, create `200` `ENC-RSMMC-00179` | `PASS` |
| R5 | Dua `POST /kiosk` bersamaan (setelah 00179 dibatalkan) | `200` / `400`; yang ditolak menyebut `ENC-RSMMC-00180` hasil permintaan pertama — kunci bekerja | `PASS` |
| R6 | (bukti `FE-KSK-010`) asuransi kiosk dengan `effectiveEndDate` | `200`, terbaca kembali `2027-09-12` | `PASS` |
| R7 | (bukti `FE-KSK-011`) lookup KTP samaran | `200`, `result 2` | `PASS` |
| R8 | `POST /kiosk` tanpa autentikasi | `401` | `PASS` |

Ringkasan: `9/9 PASS` (`runtime-run3.log` di scratchpad sesi). Run pertama gagal di autentikasi (`401`, tidak ada tulisan). Run kedua membuat `ENC-RSMMC-00177`, tetapi skrip salah membaca id respons (`encounterId`), sehingga R4/R5 gagal karena skrip. 00177 lalu dibatalkan lewat API, dan run ketiga dipakai sebagai bukti.

Keadaan data sesudah uji (query read-only): kunjungan P7 `00177`–`00180` seluruhnya `IsCancel = true`; P7 tanpa kunjungan aktif. Dua asuransi uji `UJI-FEKSK010` `IsDelete = true`. Pembersihan memakai endpoint aplikasi (cancel/delete), tanpa SQL tulis langsung. Salinan `appsettings.Development.json` di `bin/Release` dihapus.

## 4. Acceptance criteria

| AC | Status | Bukti |
| --- | --- | --- |
| 1. `/admin` dan `/kiosk` menolak `400` dengan nomor + tanggal | Terbukti | R2, R3 |
| 2. Setelah batal, pendaftaran berhasil | Terbukti | R4 |
| 3. Permintaan bersamaan → tepat satu kunjungan | Terbukti | R5 |

## 5. Risiko tersisa

1. **Kunjungan lama yang menggantung ikut memblokir** (keputusan pemilik: tanggal berapa pun). DB dev memuat ±165 kunjungan belum selesai sejak 24 Jun 2026 dan 14 pasien dengan lebih dari satu kunjungan aktif. Pasien tersebut tidak bisa didaftarkan sampai kunjungannya diselesaikan atau dibatalkan petugas. Sebelum rilis perlu pembersihan data atau prosedur penutupan kunjungan.
2. Status `Konsultasi Selesai` (7) dan `Proses Billing` (8) masih dihitung aktif, sesuai definisi tunggal. Pasien yang sudah selesai konsultasi tetapi billing-nya belum ditutup tidak bisa mendaftar ke poli lain.
3. Kunjungan yang dibooking untuk tanggal mendatang juga dihitung aktif.
4. Di Kiosk Pasien Lama, penolakan baru muncul di step Konfirmasi (step terakhir). Usulan task FE terpisah: cek kunjungan aktif sejak Identifikasi.
5. IGD punya penjaganya sendiri (`EmergencyVisitService`) dan tidak diubah.

## 6. Task berikutnya

`RJ-DOC-REV-FE-009` (frontend, sudah dikerjakan pada sesi yang sama). Usulan: pemeriksaan kunjungan aktif di awal alur Kiosk, dan IGD/Rawat Inap memakai tanggal berlaku penjamin seperti `FE-KSK-010`.
