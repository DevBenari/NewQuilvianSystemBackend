# Laporan Perubahan Backend — `BE-OPT-062`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-OPT-062` |
| Judul | Pekerja latar kirim ulang tarif pemeriksaan |
| Slice | Amendment kesiapan lanjutan — `EPIC OPT-02`, `OPT-08` |
| Roadmap | `roadmap/backend-roadmap.md` (`OPT-RM-BE-001` r5) — Rincian `BE-OPT-062` |
| Trace | `FR-OPT-022`; `NFR-008`; `OPT-DEC-082`, `083`; `OPT-AC-039`..`041`; `UAT-36`, `37`; risiko `R2` |
| Contract version | `OPT-INT-v1.2`, `OPT-PERM-v1.2`, `02-backend-architecture.md` r3 — `approved` sisi Optik, Yoga Aji Pratama 2026-11-06, terkunci hash di manifest r3 |
| Penyetuju modul lain | Tidak ada kode modul lain yang berubah. Rilis menunggu `OPT-APR-015` (Yasmina) |
| Approval task | Yoga Aji Pratama, 2026-11-06 ("lanjutkan sesuai saran kamu", menjawab tawaran mengerjakan `BE-OPT-062`) |
| Dependency | `BE-OPT-011` ✅, `BE-OPT-031` ✅ |
| Klasifikasi | `MEDIUM`, skor 4: berkas diperiksa 1, berkas diubah 1, logika bisnis 1, database 1 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/HealthServices/OpticalManagement/Services/`, `Program.cs`, `Tests/QuilvianSystemBackend.OpticalTests/` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `06e40fb7` (branch `yoga`) + `BE-OPT-060` di working tree |
| Tanggal | 2026-11-06 |
| Status | Selesai dan tervalidasi di harness. Hosted service **belum pernah dijalankan** terhadap basis data mana pun. Belum di-commit. Rilis tertahan `OPT-APR-015` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `OpticalManagement` |
| Registry | `Opt` / `ACTIVE` |
| Keberlakuan | `NEW CODE` |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-MOD-001`, `QBE-LOG-001`, `QBE-TXN-001` |
| Wewenang terpisah | Source ✅ · migration tidak diperlukan · eksekusi database ✘ · deployment ✘ |

---

## 1. Masalah yang diperbaiki

Pemeriksaan bisa final tanpa tarif masuk tagihan, misalnya saat tindakan Pemeriksaan Refraksi belum diatur atau basis data putus sebelum fakta tercatat. Sebelum task ini, tarif itu hanya dicoba lagi kalau pemeriksaannya direvisi. Bila tidak pernah direvisi, tarifnya tertinggal permanen, sehingga pemeriksaan selesai tanpa pendapatan dan tidak ada jalan mengulang. Inilah risiko `R2`.

**Contoh:** tindakan baru diatur sesudah 50 pemeriksaan final. Sebelumnya, ke-50 tarif itu tidak akan pernah masuk tagihan kecuali masing-masing direvisi.

---

## 2. Proses bisnis

| Unsur | Isi |
| --- | --- |
| Tujuan | Tarif pemeriksaan yang tertinggal masuk tagihan otomatis, tanpa tagihan ganda |
| Pelaku | Sistem. Pada fakta tagihan tercatat pemeriksa yang memfinalkan (`FinalizedBy`) |
| Pemicu | Setiap 5 menit, bisa diatur lewat `OpticalManagement:ExamChargeRetry:IntervalMinutes` |
| Prasyarat | Tindakan dan tarif sudah diatur |

**Langkah utama satu putaran:**

1. Sistem memilih pemeriksaan `Final` yang tarifnya belum tercatat dan sudah difinalkan lebih dari 5 menit lalu, paling banyak 50.
2. Bila tindakan belum diatur, putaran berhenti dengan satu peringatan.
3. Setiap pemeriksaan dikirim sendiri-sendiri lewat jalur yang sama dengan saat final dan revisi.
4. Bila fakta tercatat, penanda `BillingFactEmittedAt` terisi, sehingga pemeriksaan itu tidak dipilih lagi.
5. Bila halamannya penuh, putaran berikutnya melanjutkan dari posisi ini; bila tidak, kembali ke awal.

**Jalur tidak normal:**

| Keadaan | Hasil |
| --- | --- |
| Tagihan sudah final | Tagihan tidak berubah; muncul adjustment debit Rp 75.000 berstatus `SUBMITTED` yang menunggu keputusan Billing |
| Kunjungan sudah tertutup dan belum punya tagihan | **Temuan:** Billing membuat tagihan baru berstatus `OPEN` pada kunjungan yang sudah `Completed`, dan tarif masuk ke situ. Tarif tidak hilang, tetapi kasir perlu menagihnya |
| Tarif berlaku sesudah tanggal pemeriksaan | Fakta tercatat, tidak ada baris tagihan, dan tidak dikirim ulang lagi. Menurut kode Billing, kasus ini masuk rekonsiliasi |
| Pemeriksaan tanpa pelaku final | Dilewati dengan peringatan, dan tidak menghalangi yang lain |
| Galat pada satu pemeriksaan | Dicatat; pemeriksaan lain tetap jalan, dan yang gagal dicoba lagi |

**Hasil akhir:** tepat satu baris tarif per pemeriksaan. Status pemeriksaan dan `Version`-nya tidak berubah, sehingga layar yang sedang terbuka tidak kena bentrok versi.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md`, kontrak engineering, registry, `OPT-INT-v1.2`, arsitektur r3, `McuNoShowHostedService`, `ClinicalFactDispatchWorker`, `OptRefractionExamService`, `OptBillingGateway`, `BillingClinicalChargeBridgeService`, `ClinicalMilestoneFactProducer`, `LoggerService`, dan `Program.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/OpticalManagement/Services/OptExamChargeRetryHostedService.cs` | **Baru.** `OptExamChargeRetryHostedService`, `OptExamChargeRetryOptions`, dan tipe hasil. `ProcessRoundAsync` terbuka agar satu putaran bisa diuji |
| `Areas/HealthServices/OpticalManagement/Services/OptRefractionExamService.cs` | `GetExamsAwaitingChargeAsync`, `RetryExamChargeAsync`. Logika penyerahan dan penandaan dipindah ke `EmitAndMarkAsync`, yang dipakai bersama `BillAfterCommitAsync` |
| `Areas/HealthServices/OpticalManagement/Services/OptBillingGateway.cs` | `IsExamProcedureConfiguredAsync`; pencarian tindakan dipakai bersama |
| `Program.cs` | Opsi dan hosted service di blok `runBackgroundJobs` |
| `Tests/QuilvianSystemBackend.OpticalTests/Be031ExamBillingTests.cs` | 10 uji baru dengan rantai Billing sungguhan |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE`: tidak ada endpoint baru; registri izin tetap 45 endpoint |
| Database | Tanpa migration; `has-pending-model-changes` menyatakan model tidak berubah. Hosted service menulis ke tabel yang sudah ada begitu backend menyala |
| Keamanan/Auth | Tidak ada izin baru. Pekerja latar berjalan tanpa pengguna HTTP; pelaku fakta adalah `FinalizedBy`. Log hanya memuat nomor pemeriksaan |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE`: pekerja latar tidak punya endpoint. Konfigurasinya `OpticalManagement:ExamChargeRetry`:

| Kunci | Bawaan | Batas | Kegunaan |
| --- | --- | --- | --- |
| `Enabled` | `true` | — | Saklar |
| `IntervalMinutes` | `5` | Paling kecil 1 | Jeda antarputaran |
| `BatchSize` | `50` | 1–500 | Batas per putaran |
| `GraceMinutes` | `5` | Paling kecil 1 | Jeda sesudah final |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Cek server lokal sebelum build | Tidak ada proses `dotnet`/`node` berjalan | `PASS` | `tasklist` |
| `dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=False` | 0 error; 424 warning, sama dengan `BE-OPT-060`; 1 menit 30 detik | `PASS` | Keluaran build |
| `dotnet test Tests/QuilvianSystemBackend.OpticalTests -p:RunAnalyzers=False` | 277 lulus, 0 gagal (267 lama + 10 baru); 4 menit 51 detik | `PASS` | Keluaran `dotnet test` |
| Uji mutasi 1: syarat penanda kosong dilepas | 4 uji gagal; berkas dipulihkan (`cmp` identik) | `PASS` | Keluaran `dotnet test` |
| Uji mutasi 2: pelaku fakta bukan `FinalizedBy` | 1 uji gagal; berkas dipulihkan (`cmp` identik) | `PASS` | Keluaran `dotnet test` |
| `dotnet ef migrations has-pending-model-changes --no-build` | Model tidak berubah | `PASS` | Keluaran perintah |
| QBE checker `-Mode Strict -Path` atas 4 berkas, pemanggilan terpisah dari `dotnet test` | `Files evaluated: 4`, `VIOLATION: 0`, `REVIEW: 0`, `PASS` | `PASS` | Keluaran checker |
| Kontrol negatif `ZzzRegistryProbe` (pemanggilan yang sama) | Terkena `QBE-ENT-001`, `QBE-CFG-001`, `QBE-MOD-002`; probe dihapus | `PASS` | Keluaran checker |

Uji manual: `NOT FEASIBLE` — hosted service menulis ke basis data bersama dan tidak boleh dijalankan terhadap devYoga tanpa izin. devYoga juga belum punya pemeriksaan, karena `B1` masih kosong.

**Tidak dijalankan:** hosted service sungguhan di proses backend. Uji memanggil `ProcessRoundAsync` langsung, dan `ExecuteAsync` hanya untuk jalur `Enabled = false`. Jeda antarputaran dan uji di PostgreSQL belum dibuktikan; yang kedua menjadi bagian `B5`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) `OPT-AC-039`: 3 tertinggal menjadi 3 baris; putaran kedua tidak menambah; pelaku `FinalizedBy`, waktu `FinalizedAt` | Terpenuhi | `BE_OPT_062_OPT_AC_039_*`; mutasi 1 dan 2 |
| (2) Dalam jeda, `Draft`, `Cancelled`, dan yang sudah tercatat tidak dipilih | Terpenuhi | `BE_OPT_062_yang_tidak_memenuhi_syarat_tidak_dipilih` |
| (3) Pemeriksaan yang terus gagal tidak menghalangi | Terpenuhi, dengan angka lebih kecil dari roadmap: 4 pemeriksaan dan `BatchSize` 3, bukan 51 dan 50. Logikanya sama | `BE_OPT_062_pemeriksaan_yang_terus_gagal_*` |
| (4) `OPT-AC-040`: tindakan belum diatur menunggu; sesudah diatur terkirim | Terpenuhi | `BE_OPT_062_OPT_AC_040_*`, `BE_OPT_062_kode_tindakan_kosong_*` |
| (5) Galat satu pemeriksaan tidak menghentikan putaran | Terpenuhi | `BE_OPT_062_galat_satu_pemeriksaan_*` |
| (6) `Enabled = false`: tidak ada putaran | Terpenuhi | `BE_OPT_062_Enabled_false_*` |
| (7) `OPT-AC-041`: tagihan final menjadi adjustment, tagihan tidak berubah | Terpenuhi | `BE_OPT_062_OPT_AC_041_*` |
| (8) Kunjungan tertutup tanpa tagihan: perilaku dicatat | Terpenuhi; perilakunya ada di bagian 2 | `BE_OPT_062_kunjungan_tertutup_*` |
| (9) Tarif berlaku sesudah tanggal pemeriksaan | **Sebagian.** Terbukti fakta tercatat, tanpa baris, dan tidak diulang. Masuknya ke antrean rekonsiliasi dibaca dari kode (`BillingClinicalChargeBridgeService`), belum diperiksa isi antreannya | `BE_OPT_062_tarif_berlaku_sesudah_*` |
| (10) Log tanpa kolom Sensitif | Terpenuhi | Uji `OPT_AC_039` memeriksa isi log |
| (11) Final dan revisi berperilaku sama | Terpenuhi | Uji `Be031` lama lulus tanpa diubah isinya |
| DoD: `OPT-APR-015` tercatat sebelum rilis | **Belum** | Menunggu Yasmina |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | **Hosted service menyala secara bawaan dan menulis ke basis data begitu backend berjalan** dengan peran `All` atau `Worker`. Saat ini devYoga belum punya pemeriksaan, sehingga belum ada yang ditulis. Saat menguji modul lain terhadap basis data bersama, matikan lewat `OpticalManagement:ExamChargeRetry:Enabled = false` |
| Masalah yang diketahui | **Tagihan baru pada kunjungan tertutup** (bagian 2) adalah perilaku Billing yang ada dan tidak diubah task ini, tetapi perlu disampaikan ke Yasmina bersama `OPT-APR-015`. **Selisih kecil dengan arsitektur r3:** penunjuk posisi hanya memakai `FinalizedAt`, bukan `(FinalizedAt, Id)`, karena perbandingan Guid tidak seragam antarpenyedia basis data. Pemeriksaan dengan waktu final persis sama di batas halaman terlewat satu giliran, lalu terambil saat penunjuk kembali ke awal. Dokumen terkunci hash, sehingga penyegarannya dicatat untuk pass blueprint berikutnya |
| Risiko tersisa | Pemeriksaan yang terus gagal sebelum fakta tercatat menulis peringatan berulang setiap putaran; tidak teramati pada kasus tarif terlambat, karena faktanya tercatat. Tagihan terlambat tidak diberi tahu ke layar mana pun selain peringatan yang hilang saat tercatat |
| Perubahan sampingan | `NONE`. Probe QBE dihapus, dan berkas mutasi dipulihkan |
| Interupsi | `NONE` |
| Status Git | `M` `Program.cs`, `OptBillingGateway.cs`, `OptRefractionExamService.cs`; `??` `OptExamChargeRetryHostedService.cs`, laporan ini; ditambah perubahan `BE-OPT-060` dan blueprint yang belum di-commit. `Tests/` diabaikan `.gitignore`, sehingga uji baru perlu `git add -f` |
| Langkah berikutnya | Menyampaikan `OPT-APR-015` ke Yasmina beserta temuan tagihan pada kunjungan tertutup. Audit ulang kesiapan sesudah data `B1`..`B4` terisi |
