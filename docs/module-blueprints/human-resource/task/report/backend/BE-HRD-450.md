# Laporan Perubahan Backend — `BE-HRD-450`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-450` |
| Judul | Detail roster mengelompokkan shift per petugas yang bertugas sekarang |
| Slice | `A52-B` / `MVP-74` gelombang 1 · `EPIC HRD-125` (`MUST HAVE`) |
| Roadmap | `roadmap/backend-roadmap.md` bagian 30, kartu `BE-HRD-450` |
| Trace | `FR-PK-033`; `HRD-DEC-237`; fakta `HRD-TF-088` |
| Contract version | `v30` — API 36.2; desain `02` 38.9.1; Validation `VR-PK-37` — approved 24 November 2026 |
| Dependency | Kontrak `v30` ✅ |
| Klasifikasi | `STANDARD` — kueri baca service, perluasan DTO respons; tanpa perubahan skema |
| Task mode | `BACKEND` |
| Model | Claude Opus 5.5 |
| Source acuan | Backend `4d2c0162` (branch `yoga`). Working tree sebelum task memuat perubahan dokumen amandemen `A52-B` dan laporan frontend yang belum di-commit; tidak disentuh task ini |
| Tanggal | 24–25 November 2026 |
| Status | ✅ Selesai — 4/4 acceptance criteria terpenuhi di harness; bukti API data asli devYoga ada di `BE-HRD-455` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `SchedulingManagement` (registry: submodule legacy pemuat entity `Wfp*`/`Trx*` lama, tanpa baris pemilik sendiri — catatan registry baris 121; task tidak menambah entity sehingga tidak butuh keputusan prefix) |
| Keberlakuan | `TOUCHED LEGACY` — `RosterPeriodService` (`GetByIdAsync`, dua jalur respons tambah penugasan/shift), `RosterSchedulingDtos` (dua DTO respons) |
| QBE yang berlaku | `QBE-SVC-001` (pengelompokan dan penanda pengganti berada di service; controller tidak berubah), `QBE-API-001` (respons tetap `ApiResponse`, kode status tidak berubah), `QBE-PERM-001` (hak `RosterPeriod : Read` tidak berubah). Tanpa entity/tabel baru, sehingga `QBE-NAM-001`, `QBE-ENT-001`, `QBE-CFG-001`, `QBE-MOD-002` tidak tersentuh. `<summary>` hanya pada properti DTO (schema), tidak ada pada action controller |
| Sumber governance | `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; `CLAUDE.md` backend |
| Wewenang terpisah | Implementasi source ✅; migration ✖ (tidak diperlukan); eksekusi database ✖; deployment ✖ |

---

## 1. Cara kerja sesudah perubahan

Contoh desain: roster ICU Februari 2027 sudah terbit. Indra memegang Pagi 16 dan 17 Feb, Budi memegang Pagi 18 Feb. Raka tidak punya penugasan di roster ini.

| Langkah | Yang tampil di `GET roster-periods/{id}` |
| --- | --- |
| Kepala Unit menjalankan Ganti Petugas: Pagi 16 Feb Indra → Raka | Baris Indra tetap ada dan hanya memuat 17 Feb. Di akhir daftar muncul baris Raka dengan `isReplacementOnly = true`, `id = 00000000-…`, status penugasan sama dengan roster (`Published`), dan `plannedShiftCount = 1`. Shift 16 Feb ada di baris Raka dengan `isReplacement = true`, `originalWorkforceDisplayName = "Indra Setiawan"`, dan `shiftName = "Pagi"` |
| Kepala Unit membuka sel 16 Feb di baris Raka lalu mengganti ke Tono | Frontend mengirim `expectedWorkforceProfileId` = Raka, dan permintaan diterima. Baris Raka hilang, dan baris pengganti-saja Tono muncul. Petugas awal tetap Indra, bukan Raka |
| Klien yang masih mengirim Indra sebagai `expectedWorkforceProfileId` | `409` dengan pesan yang sudah ada (`VR-PK-37`); tidak ada perubahan |
| Pagi 18 Feb Budi → Indra (Indra punya penugasan di roster ini) | Shift masuk ke baris Indra sendiri (16, 17, 18 Feb) dengan tanda pengganti dari Budi. Baris Budi tetap ada tanpa shift. Tidak ada baris tambahan |
| Shift 16 Feb dikembalikan dari Raka ke Indra | Shift kembali ke baris Indra tanpa tanda pengganti, dan baris Raka hilang |
| Shift dihapus dari roster (`IsActive = false`, status `Cancelled`) | Tidak tampil, sama seperti sebelumnya |
| Tambah penugasan (roster draf) atau tambah shift (roster terbit) | Butir shift pada respons kini memuat `shiftName` dan identitas petugas. Sebelumnya `shiftName` kosong pada kedua jalur ini |

---

## 2. Perubahan

| Berkas | Isi |
| --- | --- |
| `Areas/Corporate/HumanResource/SchedulingManagement/Services/RosterPeriodService.cs` | `GetByIdAsync`: shift dibaca dengan kueri terpisah (`TrxShiftAssignments` aktif milik penugasan aktif periode itu, beserta `Shift` dan `WorkforceProfile`) dan tidak lagi lewat `Include` bersarang. Setiap shift ditempatkan di baris penugasannya sendiri bila petugasnya sama. Bila tidak, shift masuk ke baris penugasan pertama milik petugas sekarang. Bila petugas sekarang tidak punya penugasan di roster ini, shift masuk ke baris pengganti-saja (`Id = Guid.Empty`, `IsReplacementOnly = true`, urut nama, di akhir daftar). Nama petugas awal diambil sekali dari `MstWorkforceProfiles`. Helper baru: `IsReplacementShift` (`OriginalEmployeeId` terisi **dan** berbeda dari petugas sekarang) dan `ToShiftItem` (satu pemetaan butir shift). Jalur respons tambah penugasan dan tambah shift mengisi `ShiftName` dari master shift yang sudah dibaca, ditambah `WorkforceProfileId/Code/DisplayName` |
| `…/DTOs/RosterSchedulingDtos.cs` | `RosterAssignmentResponse.IsReplacementOnly`. `ShiftAssignmentItemResponse`: `WorkforceProfileId`, `WorkforceProfileCode`, `WorkforceDisplayName`, `IsReplacement`, `OriginalWorkforceProfileId`, `OriginalWorkforceDisplayName` |
| `Tests/QuilvianSystemBackend.HumanResourceTests/RosterReplacementGridTests.cs` (baru), `AcceptanceTestIndex.md` | 7 uji (5 fakta + 1 teori 2 kasus) lewat `RosterPeriodService` dan `ShiftReplacementService` nyata; baris `AT-HRD-PK-47` dan baris kelas di indeks |

`ShiftReplacementService` tidak diubah. Aturan `expectedWorkforceProfileId` ≠ petugas sekarang → `409` sudah ada; task ini membuktikannya untuk sel pengganti.

### Delta terhadap kartu roadmap dan kontrak

| Hal | Kartu/kontrak | Pelaksanaan | Alasan |
| --- | --- | --- | --- |
| Arti `isReplacement` | API 36.2: "`true` bila shift hasil Ganti Petugas" | `true` hanya bila `OriginalEmployeeId` terisi **dan** berbeda dari petugas sekarang | Shift yang dikembalikan ke petugas awal kembali ke baris awal tanpa tanda. AC4 "tanpa penggantian `isReplacement = false`" tetap berlaku. `OriginalEmployeeId` tidak dihapus (jejak `ShiftRevisionRecorder` utuh) |
| Pengganti yang punya penugasan di roster | Tidak disebut | Shift masuk ke baris penugasan pertama petugas itu; tanpa baris pengganti-saja | Mengikuti outcome "baris `WorkforceProfileId`"; satu petugas tidak tampil dua kali |
| Isi baris pengganti-saja | `IsReplacementOnly`, `Id = Guid.Empty` | Ditambah `AssignmentStatus` dari penugasan asal shift, `PlannedShiftCount` = jumlah shift, `PositionId` kosong, dan urutan di akhir daftar (urut nama) | Supaya matriks `FE-HRD-402` dapat menampilkan baris tanpa data tambahan |
| Status kontrak API 36.2 | Tabel masih "Sudah ada — perluasan **Rencana (belum tersedia)**" | Tidak disunting | Artefak kontrak terkunci hash. Penyegaran status menjadi tersedia dicatat untuk pass blueprint |

---

## 3. Verifikasi

| Pemeriksaan | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `RosterReplacementGridTests` | 7/7 lulus | `PASS` (AC1–AC4) | Keluaran `dotnet test` |
| Kontrol negatif: 6 mutasi — (1) pengelompokan kembali ke `RosterAssignmentId` tanpa memeriksa petugas, kontrol yang diminta kartu; (2) `isReplacement` tanpa membandingkan petugas awal dan sekarang; (3) baris pengganti-saja dibuang; (4) nama petugas awal tidak dicari; (5) `ShiftName` jalur tambah shift (terbit) dihapus; (6) `ShiftName` jalur tambah penugasan (draf) dihapus | 6/6 tertangkap: (1) 3 uji gagal, termasuk AC1; (2) 1; (3) 2; (4) 3; (5) 1; (6) 1. Tanpa galat kompilasi. Source dipulihkan dengan hash SHA-256 `1aa98214…` sama dengan salinan asli | `PASS` | `mut450.cjs`, `mut450-hasil.log` di scratchpad sesi |
| Build ulang dan uji akhir sesudah mutasi | Pemulihan dengan `copyFileSync` mempertahankan waktu ubah lama sehingga MSBuild tidak build ulang (DLL masih berisi mutasi 6). Source di-`touch`, lalu build ulang (0 galat) dan uji task + skema 14/14 lulus | `PASS` | `build450-final.log`, `test450-final.log` |
| Regresi penuh proyek uji HR | 422/423 dari salinan scratchpad (13 menit 17 detik). Satu uji yang gagal, `WorkflowEscalationAndBulkFlagSchemaTests.Migration_hanya_menambah_dua_kolom…`, gagal karena "Root repository tidak ditemukan" (salinan di luar repo); diulang dengan env `QUILVIAN_BACKEND_ROOT` dan lulus. Total 423/423 | `PASS` | `test450-full.log`, `test450-final.log` |
| Build proyek utama | 0 galat (`-p:RunAnalyzers=False`) | `PASS` | Keluaran `dotnet build` |
| QBE `Strict` atas 3 berkas | 0 violation, 0 review. Probe `TrxZzzQbeProbe450` (ber-`[Table]`) terdeteksi 4 violation (`QBE-NAM-001`, `QBE-ENT-001`, `QBE-CFG-001`, `QBE-MOD-002`) lalu dihapus | `PASS` | Keluaran checker |
| Bukti API data asli devYoga (ulang `UAT-PK-03`/`UAT-PK-16`) | Tidak dijalankan di task ini | `NOT RUN` | Kartu menempatkannya di `BE-HRD-455` |

### Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Pagi 16 Feb Indra → Raka: baris Raka memuat shift itu (`isReplacement = true`, `originalWorkforceDisplayName = "Indra Setiawan"`); baris Indra tanpa shift 16 Feb; baris Indra tetap ada | Terpenuhi | `Pengganti_tanpa_penugasan_tampil_di_baris_pengganti_saja_dan_baris_awal_tetap_ada`; pengganti berpenugasan: `Pengganti_yang_punya_penugasan_menerima_shift_di_barisnya_sendiri` |
| 2. Raka tanpa penugasan roster → baris `isReplacementOnly = true`, `id` kosong | Terpenuhi | `Pengganti_tanpa_penugasan_tampil_di_baris_pengganti_saja_dan_baris_awal_tetap_ada` |
| 3. Ganti Petugas berikutnya dengan `expectedWorkforceProfileId` = Raka diterima; dengan Indra → `409` (`VR-PK-37`) | Terpenuhi | `Ganti_berikutnya_dari_sel_pengganti_memakai_petugas_sekarang` |
| 4. Shift `Cancelled` tidak tampil; shift tanpa penggantian `isReplacement = false` | Terpenuhi | `Shift_nonaktif_tidak_tampil`; `isReplacement = false` diperiksa di tiga uji, termasuk `Kembali_ke_petugas_awal_tidak_ditandai_pengganti`. Jalur hapus shift roster mengisi `IsActive = false` bersama `Cancelled` (`RosterPeriodService` sekitar baris 1672) |

---

## 4. Temuan

| Temuan | Dampak | Tindakan |
| --- | --- | --- |
| As-is: detail roster mengelompokkan shift menurut `RosterAssignmentId`. Ganti Petugas hanya mengubah `WorkforceProfileId` shift (`HRD-TF-088`) | Shift pengganti tampil di baris petugas awal. Ganti berikutnya dari sel itu mengirim petugas yang salah dan ditolak `409` | Diperbaiki (inti task) |
| As-is: respons tambah penugasan dan tambah shift tidak mengisi `ShiftName` | Sel baru di layar tidak bernama sampai halaman dimuat ulang | Diperbaiki |
| Klien yang mengandalkan pengelompokan per penugasan | Baris pengganti-saja ber-`id` kosong tidak boleh dipakai untuk hapus/validasi penugasan | Risiko kartu; matriks roster diperbarui di `FE-HRD-402` |
| Status kontrak API 36.2 masih "Rencana (belum tersedia)" | Dokumen kontrak tertinggal dari source | Catatan untuk pass blueprint (artefak terkunci hash) |

---

## 5. Catatan penutup

| Hal | Isi |
| --- | --- |
| Migration / database | Tidak ada migration. Tidak ada eksekusi database |
| Git | Tidak ada stage, commit, push, pull, merge, rebase, atau deploy. Berkas baru: `RosterReplacementGridTests.cs` (cukup `git add`) |
| Dampak frontend | Respons detail roster diperluas tanpa menghapus ruas. Matriks dapat membaca petugas sekarang per sel, tanda pengganti, dan baris pengganti-saja (`FE-HRD-402`). Ganti Petugas dari sel memakai `workforceProfileId` sel sebagai `expectedWorkforceProfileId` |
| Bukti data asli | `UAT-PK-03`/`UAT-PK-16` di devYoga dikerjakan di `BE-HRD-455` |
| Task berikutnya | `BE-HRD-451` (pengecualian konversi tidak memblokir tutup periode), satu gelombang `MVP-74`; `FE-HRD-402` dapat mulai memakai respons ini |
