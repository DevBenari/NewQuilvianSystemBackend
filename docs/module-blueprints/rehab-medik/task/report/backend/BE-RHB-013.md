# Laporan Perubahan Backend — `BE-RHB-013`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RHB-013` |
| Judul | Handoff billing sesi terapi |
| Slice | S-D — Sesi dan handoff billing |
| Roadmap | `docs/module-blueprints/rehab-medik/roadmap/backend-roadmap.md` — kartu `BE-RHB-013` |
| Trace | FR-RHB-033..035; FEAT-032; RHB-DEC-021; INT-RHB-08, INT-RHB-09; `state-transition-matrix.md` §6; UAT-08, 10, 11 |
| Contract version | Set kontrak `v1`, `approved` 2026-11-05 — grup API *Rehab Billing Handoff*; integrasi INT-RHB-08/09 |
| Dependency | `BE-RHB-012` ✅ (2026-11-09); keputusan `RHB-OQ-015` ✅ (2026-11-09, [bukti](../../evidence/RHB-OQ-015-persetujuan-clinical-billing.md)) |
| Klasifikasi | `STANDARD` — service + controller baru di Rehab, perubahan tanda tangan sesi, satu nilai enum baru milik ClinicalManagement; tanpa schema baru |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/HealthServices/RehabilitationManagement/{Controllers,Services,DTOs}`, `Areas/HealthServices/ClinicalManagement/Enums/PatientProcedureSource.cs`, `Program.cs`, dokumen blueprint |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `6bb3fba7` (`sukmagp`), belum di-commit |
| Tanggal | 2026-11-09 |
| Status | **✅ Selesai** (2026-11-09). Build owner `PASS` (0 error; 426 warning bawaan, tidak ada di folder Rehab atau `PatientProcedureSource`). Uji API: run utama 11/13 (dua FAIL dari data uji, §5), uji ulang 3/3. Bukti DB READ ONLY: 5 tindakan rehab, masing-masing tepat 1 fakta dan 1 charge line. |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `RehabilitationManagement` (`Rhb`, `ACTIVE`) |
| Applicability | `NEW CODE` |
| Registry | `Rhb` `ACTIVE`. Tidak ada entity baru: kolom `RhbSession.BillingHandoff*` dan `RhbSessionIntervention.PatientProcedureId` sudah ada sejak `BE-RHB-001`. |
| Jenis capability | **Transaksi — sub-resource ter-scope induk.** `GET /{id}/billing-handoff` (baca) dan `POST /{id}/billing-handoff/retry` (aksi idempoten). Tidak ada `options`, `PATCH status`, maupun `DELETE`. |
| QBE yang berlaku | QBE-NAM, QBE-SVC-001, QBE-API-001, standar hak akses (`RehabilitationPermissions.BillingHandoff`), aturan transaksi (fakta charge di luar transaksi) |
| Lintas modul | **ClinicalManagement**: nilai enum baru `PatientProcedureSource.RehabilitationSession = 5`; tulis `TrxPatientProcedure` (INT-RHB-08). **Billing**: fakta charge lewat `ClinicalMilestoneFactProducer` (INT-RHB-09), bentuk sama dengan `PatientProcedureController` dan Hemodialisa. Persetujuan pemilik: `RHB-OQ-015` (pernyataan owner 2026-11-09; nama pemberi persetujuan belum dicatat). Tidak ada berkas Billing yang diubah. |

## 1. Masalah yang diperbaiki

Sejak `BE-RHB-012`, sesi terapi bisa ditandatangani, tetapi tindakannya tidak pernah sampai ke tagihan.
Tanpa task ini, rumah sakit tidak dapat menagih terapi rehab rawat jalan maupun rawat inap, karena
Billing menolak charge manual untuk pelayanan RJ/RWI (`RHB-DEC-021`).

**Contoh.** Terapis menandatangani sesi `RHB-SES-2026-000013` berisi dua tindakan, ALAT_1970 dan
ALAT_1971. Saat itu juga terbentuk dua tindakan pasien bertarif atas nama dokter penanggung jawab
episode, dan keduanya masuk invoice kunjungan pasien. Bila Billing sedang tidak bisa dihubungi, sesi
tetap final dan status penyerahan menjadi *Gagal*; kasir menekan *Ulang*, dan tagihan terbentuk satu
kali saja.

## 2. Proses bisnis

1. **Tanda tangan sesi.** Di dalam transaksi tanda tangan yang sama, setiap intervensi aktual
   menjadi satu `TrxPatientProcedure` (INT-RHB-08):
   - kunci idempoten `RHB-SESINT-{intervensi}` (indeks unik);
   - sumber `RehabilitationSession`;
   - status *Completed*;
   - `DoctorId` dan `InstructingDoctorId` = dokter penanggung jawab episode (keputusan `RHB-OQ-015`);
   - pelaksana = terapis sesi;
   - jumlah sesuai intervensi, unit "Kali".

   Status handoff menjadi *Menunggu*. Sesi *Tidak terlaksana* atau tanpa intervensi tetap *Tidak
   ditagihkan* (state §6).
2. **Sesudah commit**, satu fakta kelayakan charge dipancarkan per tindakan (INT-RHB-09), dengan
   sumber `Procedure`, efek `ProcedureCharge`, waktu = waktu tanda tangan, dan korelasi = id sesi.
   Hasil gabungannya:
   - semua `Emitted`/`Replayed` → *Terkirim*;
   - ada penolakan → *Gagal*, dengan pesan berisi kode penolakan dan jumlah tindakan saja;
   - hasil tidak pasti → *Menunggu*.

   Kegagalan Billing tidak membuka kembali sesi.
3. **Ulang (`retry`)** untuk sesi final:
   - status *Menunggu*/*Gagal* → fakta dipancarkan ulang (dijawab `Replayed` bila sudah pernah
     diterima, jadi tidak ada tagihan ganda);
   - status *Terkirim* → dijawab apa adanya;
   - status *Tidak ditagihkan* tetapi masih ada intervensi tanpa tindakan → **backfill** sesi lama
     yang ditandatangani sebelum task ini: tindakan dibentuk dalam satu transaksi berkunci, lalu
     diserahkan (AC 4).

   Retry pada sesi draft ditolak 409 `RHB-VAL-020`.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

| Berkas | Status | Isi |
| --- | --- | --- |
| `ClinicalManagement/Enums/PatientProcedureSource.cs` | Diperbarui | `RehabilitationSession = 5` (disimpan sebagai `int`, tanpa migration) |
| `RehabilitationManagement/Services/RhbBillingHandoffService.cs` | Baru | `CreateProceduresAsync` (di dalam transaksi pemanggil), `HandOffAsync` (di luar transaksi), `GetStatusAsync`, `RetryAsync` (termasuk backfill) |
| `RehabilitationManagement/Services/RhbSessionService.cs` | Diperbarui | `SignAsync` membentuk tindakan di transaksi tanda tangan dan menyerahkan fakta sesudah commit (menggantikan penanda sementara "BE-RHB-013 belum tersedia") |
| `RehabilitationManagement/Controllers/RehabSessionBillingController.cs` | Baru | 2 endpoint, Resource `RehabBillingHandoff`, aksi `Read` dan `Retry` |
| `RehabilitationManagement/DTOs/RehabBillingHandoffDtos.cs` | Baru | `RehabBillingHandoffResponse`, `RehabBillingHandoffItem` (per intervensi: tindakan, jumlah, `PatientProcedureId`, `IsBillable`, `IsBillingGenerated`) |
| `Program.cs` | Diperbarui | `AddScoped<RhbBillingHandoffService>()` |

### 3.2 Dampak kontrak, database, dan keamanan

| Hal | Isi |
| --- | --- |
| Schema / migration | Tidak ada. Kolom rehab sudah ada; enum disimpan `int`; indeks unik `IdempotencyKey` `TrxPatientProcedure` sudah ada. |
| Hak akses | Controller baru `RehabBillingHandoff` (`Read`, `Retry`), terpisah dari `RehabSession`, sehingga kasir bisa diberi hak mengulang tanpa hak mengubah catatan sesi. Tidak ada nama peran di kode. |
| Privasi | `ClinicalNote` tindakan hanya memuat nomor sesi; parameter aktual dan respons pasien tetap di rehab. `BillingHandoffError` hanya memuat kode penolakan, jumlah tindakan, atau nama tipe exception (AC 5). |
| Logger | `RehabBillingHandoff.Retry`: id sesi, nomor, status, jumlah item |
| Konsumen `ProcedureSource` | Pencarian seluruh kode: tidak ada `switch` atau label yang menolak nilai baru. Satu-satunya pemeriksaan nilai (`EmergencyVisitFee` di `PatientProcedureController`) tidak terdampak. |

### 3.3 Delta terhadap desain

| Delta | Rincian |
| --- | --- |
| **UAT-11 (tindakan belum bertarif)** | Desain mengharapkan handoff *Gagal*. Billing saat ini **menerima** fakta tindakan tanpa tarif (`Emitted`), lalu menahannya di sisi Billing dengan `InvoiceSyncErrorCode = TARIFF_NOT_FOUND` (status sinkronisasi 4). Akibatnya handoff rehab tercatat *Terkirim*, dan masalah tarif tampil di Billing ("tarif belum ada"), bukan di sesi. Item tersinkron ulang oleh Billing setelah tarif dibetulkan. Inti UAT-11 — ulang dua kali tanpa tagihan ganda — terbukti (§5). |
| Unit tindakan | `UnitNameSnapshot = "Kali"`, jumlah = jumlah intervensi |
| Sesi *Dihentikan dini* | Ditagihkan sesuai tindakan aktual yang dicatat (state §6: ≥ 1 intervensi). Ini berbeda dari Hemodialisa, yang tidak menagih sesi dihentikan. |
| Versi sesi | Perubahan status handoff tidak menaikkan `Version` sesi, karena bukan perubahan isi klinis |

## 4. Validasi

| Pemeriksaan | Hasil |
| --- | --- |
| Build owner (`dotnet build .\QuilvianSystemBackend.csproj --configuration Debug -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RunAnalyzers=false`) | `PASS` — 0 error, 426 warning bawaan, 3 menit 56 detik. Percobaan pertama terhenti saat sesi agent berakhir (sebelum kompilasi), lalu diulang. |
| QBE Strict | `PASS` — `[AccessController]` `ControllerName = RehabBillingHandoff` = argumen 1 `[AccessPermission]`; aksi `Read`/`Retry` = argumen 1 `[AccessAction]`; `AccessTypes.Read`/`Update`; tanpa hardcode peran; service tanpa `ApplicationDbContext` di controller |
| Review diff | 1 berkas ClinicalManagement (enum), 4 berkas Rehab, `Program.cs`. Tidak ada berkas Billing yang diubah. |

AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)

## 5. Uji API dan DB (manual)

**Lingkungan (2026-11-09):**
- backend Debug di `https://localhost:7184` dengan DB dev `QuilvianNewDevSukma`.
- Backend yang sebelumnya berjalan (PID 18516, mengunci DLL) dihentikan atas izin owner sebelum build.
- Backend uji dijalankan lalu dihentikan sesudah uji.

**Data uji:**
- episode baru `RHB-EPS-2026-000008` (rujukan eksternal), dokter PJ dari lookup dokter;
- program layanan A dengan dua intervensi bertarif: ALAT_1970 dan ALAT_1971 (masing-masing 19 tarif);
- akun uji berprofesi Dokter Umum.

| # | Pemeriksaan | Hasil |
| --- | --- | --- |
| 1 | Controller akses `RehabBillingHandoff` terdaftar dengan `Read` + `Retry` | PASS |
| 2 | Sebelum tanda tangan: status *Tidak ditagihkan*, 2 item tanpa tindakan; retry pada draft → 409 `RHB-VAL-020` | PASS |
| 3 | **UAT-08 / AC1**: tanda tangan 200 → *Terkirim*; `GET` 2 item, masing-masing ber-`PatientProcedureId` dan bertarif | PASS ×2 |
| 4 | **UAT-10 / AC2**: sesi *Tidak terlaksana* ditandatangani → *Tidak ditagihkan*, 0 item; retry tetap *Tidak ditagihkan* | PASS |
| 5 | **AC4**: sesi final lama `RHB-SES-2026-000001` (ditandatangani sebelum task ini, 2 intervensi tanpa tindakan) → retry membentuk 2 tindakan + *Terkirim*; retry kedua tanpa tindakan baru | PASS |
| 6 | Tanpa butir `RehabBillingHandoff` (login ulang) → `GET` dan retry 403 | PASS |
| 7 | Sesi tidak ada → 404 | PASS |
| 8 | **UAT-11 (varian)**: tindakan tanpa tarif PR-HMD-001 di luar rencana (deviasi *Dimodifikasi*) → tanda tangan 200, *Terkirim* (lihat delta §3.3) | PASS (uji ulang) |
| 9 | **AC3**: retry dua kali pada sesi #8 dan retry sesi UAT-08 → 200, tindakan tetap sama, status *Terkirim* | PASS (uji ulang) |

**Dua FAIL run utama** (#8–9 versi awal) berasal dari data uji. Pemetaan PR-HMD-001 ke layanan A
berstatus nonaktif, sehingga `PUT` sesi ditolak 422 `RHB-VAL-042` dan sesi tidak pernah selesai. Uji
ulang mengaktifkan pemetaan itu sementara lewat `PUT /rehab-disciplines/{id}/procedures`, lalu
mengembalikannya nonaktif.

**Bukti DB (kueri READ ONLY):**

| Bukti | Hasil |
| --- | --- |
| `TrxPatientProcedure` berkunci `RHB-SESINT-*` | 5 baris, semuanya `ProcedureSource = 5`, 5 kunci unik, `IsBillable`, `DoctorId = InstructingDoctorId` |
| Fakta klinis dan charge line per tindakan | Masing-masing **tepat 1** `CliClinicalMilestoneFact` dan **tepat 1** `BilChargeLine`, sesudah retry berulang (tanpa ganda, AC 3) |
| Sinkronisasi invoice | ALAT_1970/1971 (4 item): `InvoiceSyncStatus = Synced`, artinya masuk invoice kunjungan (UAT-08). PR-HMD-001: status 4, `TARIFF_NOT_FOUND`. |
| Dokter tindakan | 3 tindakan episode uji = dokter PJ episode (`RHB-OQ-015`) |
| Status sesi | `000013` dan `000016` *Terkirim*; `000014` (*Tidak terlaksana*) *Tidak ditagihkan*; `000001` (backfill) *Terkirim* |

**Pembersihan:**
- 0 dari 40 akun `rhb-uji-*` yang masih aktif;
- kebijakan pasangan uji: 0 baris;
- layanan uji aktif: 0;
- pemetaan PR-HMD-001 dikembalikan nonaktif.

**Residu data dev:**
- di `RHB-EPS-2026-000008`: program aktif, sesi final `000013` dan `000016`, sesi *Tidak terlaksana* `000014`;
- sesi `000015` *Berlangsung* dari run utama yang gagal. Pelaksananya akun uji yang sudah nonaktif, jadi sesi ini hanya dapat dibatalkan pelaksananya.
- Sesi lama `000001` kini tertagih lewat backfill.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) UAT-08 (2 item tagihan) | Terpenuhi | §5 #3 + DB (2 charge line, *Synced*) |
| (2) UAT-10 (`NotDone` tanpa tagihan) | Terpenuhi | §5 #4 |
| (3) UAT-11 (`Failed` → retry → tanpa ganda) | Terpenuhi untuk "tanpa ganda"; keadaan *Gagal* tidak terjadi untuk tindakan tanpa tarif karena Billing menerimanya (delta §3.3) | §5 #8–9 + DB (1 fakta, 1 charge line per tindakan) |
| (4) Sesi final lama dapat diserahkan lewat `retry` | Terpenuhi | §5 #5 |
| (5) `BillingHandoffError` tanpa data klinis | Terpenuhi (kode) | §3.2. Jalur *Gagal* tidak terpicu di dev; pesan disusun hanya dari kode penolakan, jumlah, atau tipe exception. |
| DoD: persetujuan pemilik di laporan | Terpenuhi | `RHB-OQ-015` (bukti di `evidence/`) |
| DoD: build owner | Terpenuhi | §4 |

## 7. Risiko dan tindak lanjut

| Butir | Rincian |
| --- | --- |
| Tindakan tanpa tarif | Masalahnya tampil di Billing (*tarif belum ada*), bukan di sesi rehab. Kasir/admin tarif perlu memantau daftar Billing. Bila owner menghendaki handoff *Gagal* untuk kasus ini, perlu keputusan bersama pemilik Billing (perubahan di Billing, di luar Rehab). |
| Nama pemberi persetujuan `RHB-OQ-015` | Belum dicatat; lengkapi artefaknya di `evidence/` bila ada |
| Sesi *Berlangsung* residu | `000015` (EP8), ditambah 2 sesi di EP6 dari `FE-RHB-008` |
| Task berikutnya | `FE-RHB-009` (status dan ulang handoff billing di layar) kini 🟡 |
