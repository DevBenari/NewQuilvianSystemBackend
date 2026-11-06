# Laporan Perubahan Backend — `BE-OPT-050`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-OPT-050` |
| Judul | Keluhan garansi dan remake |
| Slice | `MVP-4` — `EPIC OPT-07` |
| Roadmap | `roadmap/backend-roadmap.md` (`OPT-RM-BE-001` r2) — Rincian `BE-OPT-050` |
| Trace | `FR-OPT-070`, `071`; `OPT-DEC-015`, `041`, `043`; `OPT-AC-013`, `022`, `026`; `UAT-16`, `17` |
| Contract version | `OPT-API-v1` grup Opt Warranty Claim, `OPT-STATE-v1` bagian 4, `OPT-VAL-v1` bagian 5 — `approved` sisi Optik |
| Penyetuju modul lain | Tidak ada perubahan kode modul lain |
| Dependency | `BE-OPT-041` ✅, `BE-OPT-010` ✅ |
| Klasifikasi | `MEDIUM` — 5 endpoint, service, DTO |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/HealthServices/OpticalManagement/{Controllers,Services,DTOs}/`, `Program.cs`, `Tests/QuilvianSystemBackend.OpticalTests/` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `d8fa6e40` (branch `yoga`), di atas perubahan `BE-OPT-003`, `030`..`042` yang belum di-commit |
| Tanggal | 2026-11-04 |
| Status | Selesai dan tervalidasi di harness. Belum di-commit |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `OpticalManagement` |
| Registry | `Opt` / `ACTIVE` |
| Keberlakuan | `NEW CODE`; `TOUCHED LEGACY` hanya `Program.cs` (DI) |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-VAL-001`, `QBE-DTO-001`, `QBE-PAGE-001`, `QBE-LOG-001` (isi keluhan dan temuan tidak dicatat di log), `QBE-CODE-006` (nomor `OPT-CLM` lewat `OptNumberService`) |
| Wewenang terpisah | Source ✅ · migration tidak diperlukan · eksekusi database ✘ · deployment ✘ |

---

## 1. Masalah yang diperbaiki

Keluhan atas kacamata yang sudah diserahkan belum bisa dicatat. Remake tanpa biaya belum bisa dibuat, dan masa
garansi yang dibekukan saat penyerahan belum dipakai.

---

## 2. Proses bisnis

**Contoh (`UAT-16`, `OPT-AC-013`):** kacamata Ibu Sari (samaran) diserahkan 3 November 2026 dengan garansi ukuran 30 hari.

1. **Catat keluhan.** Pada 13 November (hari ke-10) Ibu Sari mengeluh pusing. Petugas lebih dulu mendaftarkan **kunjungan Optik baru**, lalu mencatat keluhan atas pesanan asal. Sistem menghitung batas garansi 3 Desember 2026 dan menandai keluhan "dalam garansi".
2. **Periksa ulang.** Rina (RO, SIP dan STR berlaku) mencatat temuan "AXIS salah ukur".
3. **Kepala Optik menyetujui remake.** Pesanan turunan `IsRemake` dibuat di kunjungan keluhan, tertaut ke pesanan asal, berstatus `Menunggu Uang Muka` dengan minimum Rp 0. Tidak ada fakta tagihan.
4. **Remake dikerjakan dan diserahkan** lewat jalur pesanan yang sama: dikirim tanpa uang muka, stok frame dan lensa dipotong saat dikirim, lalu siap dan diserahkan. Tagihan kunjungan asal tidak bertambah.

**Jalur tidak normal:**

| Keadaan | Hasil |
| --- | --- |
| Keluhan ukuran hari ke-31 (`UAT-17`, `OPT-AC-022`) | Tercatat "di luar garansi"; persetujuan remake `409 OPT-VAL-062` "Keluhan di luar masa garansi. Proses sebagai pesanan berbayar." Kepala Optik menolak dengan alasan |
| Lapisan lensa mengelupas di bulan ke-5 | Dalam garansi (cacat lensa 6 bulan) |
| Kerusakan karena pemakaian | Selalu di luar garansi |
| Keluhan dicatat di kunjungan pesanan asal atau kunjungan pasien lain (`OPT-AC-026`) | `400 OPT-VAL-061` "Daftarkan kunjungan keluhan baru untuk pasien ini." |
| Pesanan asal belum diserahkan | `409 OPT-VAL-060` |
| Diputuskan sebelum diperiksa ulang | `409 OPT-VAL-063` |
| Pemeriksaan ulang oleh bukan RO/Sp.M | `403` dari penjaga izin praktik |
| Remake kedua untuk keluhan yang sama | `409 OPT-VAL-064` |
| Tolak tanpa alasan, temuan kosong | `400` |
| Pesanan remake ditagihkan | `409 OPT-VAL-057` |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kontrak grup Opt Warranty Claim, `OPT-STATE-v1` bagian 4, `OPT-VAL-v1` bagian 5, `OPT-DEC-015`/`041`/`043`; model dan configuration `OptWarrantyClaim` (`Version` token konkurensi); `OptOrderService` (jalur remake: gerbang bayar, fakta, serah), `OptPractitionerGuard`, `OptPrescriptionService.CheckUsableForOrderAsync`, `OptNumberService`, `OptLabels`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/OpticalManagement/Controllers/OptWarrantyClaimController.cs` | **Baru.** 5 endpoint |
| `Areas/HealthServices/OpticalManagement/Services/OptWarrantyClaimService.cs` | **Baru.** Daftar, detail, catat (hitung garansi), periksa ulang, putuskan (buat remake); `WarrantyDeadline` statis |
| `Areas/HealthServices/OpticalManagement/DTOs/OptWarrantyClaimDtos.cs` | **Baru.** |
| `Areas/HealthServices/OpticalManagement/Services/OptOperationResult.cs` | **Diperbarui.** `OPT-VAL-060`..`064` |
| `Program.cs` | **Diperbarui.** `AddScoped<OptWarrantyClaimService>()` |
| `Tests/QuilvianSystemBackend.OpticalTests/Be050WarrantyClaimTests.cs` | **Baru** (diabaikan Git; perlu `git add -f`). 9 uji |
| `Tests/QuilvianSystemBackend.OpticalTests/OpticalPermissionRegistryTests.cs` | **Diperbarui.** Hitungan endpoint Optik 35 → 40 |
| `docs/module-blueprints/optik/00-interview-decisions.md` | **Diperbarui.** Satu baris penyegaran kontrak |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Lima endpoint `OPT-API-v1` tersedia. **Tambahan:** `PrescriptionVersionId?` pada keputusan (resep baru hasil pemeriksaan ulang untuk remake); `RemakeItems[]` ditafsirkan sebagai id baris pesanan asal, kosong = semua baris |
| Database | Tanpa migration. Menulis `OptWarrantyClaim`, pesanan remake (`OptOrder`, `OptOrderItem`, `OptOrderStatusHistory`) |
| Keamanan/Auth | `OptWarrantyClaim : Read/Create/Examine/Decide`; pemeriksaan ulang juga dijaga profesi RO/Sp.M dengan SIP dan STR; lolos `PermissionRegistryValidator` (40 endpoint Optik) |

---

## 4. Dokumentasi endpoint

Base URL: `api/v1/health-services/optical-management/warranty-claims`

| Method | Path | Isi | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/` | `Status`, `OrderId`, `PatientId`, `Search`, paging | `OptWarrantyClaim : Read` |
| `GET` | `/{id}` | — | `OptWarrantyClaim : Read` |
| `POST` | `/` | `OrderId`, `EncounterId`, `ComplaintType`, `ComplaintNote` | `OptWarrantyClaim : Create` |
| `POST` | `/{id}/examination` | `ExaminationFinding`, `ReexamRefractionExamId?`, `ExpectedVersion` | `OptWarrantyClaim : Examine` |
| `POST` | `/{id}/decision` | `Approve`, `DecisionReason?`, `RemakeItems[]?`, `PrescriptionVersionId?`, `ExpectedVersion` | `OptWarrantyClaim : Decide` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Cek server lokal sebelum build | Tidak ada proses `dotnet` berjalan | `PASS` | `tasklist` |
| `dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=False` | 0 error | `PASS` | Keluaran build |
| `dotnet test Tests/QuilvianSystemBackend.OpticalTests -p:RunAnalyzers=False` | 225 lulus, 0 gagal (9 baru) | `PASS` | Keluaran `dotnet test` |
| Batas garansi (uji unit) | Serah 5 Nov 2026 → ukuran 5 Des 2026, lensa/frame 5 Mei 2027; kerusakan tanpa garansi | `PASS` | Uji teori |
| `UAT-16` / `OPT-AC-013` | Dalam garansi, batas 3 Des; diperiksa RO; remake `IsRemake`/`Remake`, `AwaitingDownPayment`, 0%, tertaut `ParentOrderId`; tagih ditolak; dikirim (stok frame 4→3), siap, diserahkan; baris tagihan tetap 2; nol fakta untuk remake | `PASS` | Uji rantai |
| `UAT-17` / `OPT-AC-022` | Hari ke-30 dalam garansi; hari ke-31 di luar, setuju `409 OPT-VAL-062`, tolak tanpa alasan `400`, tolak dengan alasan `Rejected`; lensa bulan ke-5 dalam; kerusakan di luar | `PASS` | Uji |
| `OPT-AC-026` | Kunjungan asal dan kunjungan pasien lain `400 OPT-VAL-061`; pesanan belum diserahkan `409 OPT-VAL-060` | `PASS` | Uji |
| `OPT-VAL-063`, `064`, bukan RO/Sp.M, temuan kosong | Lulus; tepat satu remake | `PASS` | Uji |
| Remake sebagian barang + daftar keluhan | Hanya lensa; pencarian nomor RM menemukan keluhan | `PASS` | Uji |
| Uji mutasi: batas garansi ukuran digeser + syarat dalam garansi dibuang | 3 uji gagal; berkas dipulihkan | `PASS` | Keluaran `dotnet test` |
| QBE checker `-Mode Strict -Path` atas 5 berkas | `VIOLATION: 0`, `REVIEW: 0`, `PASS` | `PASS` | Keluaran checker |
| Kontrol negatif `ZzzRegistryProbe` (pemanggilan terpisah) | Terkena 3 aturan; probe dihapus | `PASS` | Keluaran checker |

**Tidak dijalankan:** PostgreSQL dan backend yang menyala.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `OPT-AC-013` | Terpenuhi di harness | Uji `UAT_16_…` |
| `OPT-AC-022` | Terpenuhi di harness | Uji `UAT_17_…` + uji unit |
| `OPT-AC-026` | Terpenuhi di harness | Uji |
| `UAT-16`, `17` | Terpenuhi di harness | Uji |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Keputusan pelaksanaan yang perlu dikonfirmasi | (a) Batas garansi **termasuk** hari terakhir (hari ke-30 masih dalam garansi ukuran) dan dihitung dari tanggal WIB penyerahan. (b) Remake memakai resep pesanan asal kecuali Kepala Optik memilih versi resep baru (`PrescriptionVersionId`, tambahan atas kontrak) — keluhan ukuran biasanya berakhir dengan resep baru. (c) Nomor pesanan remake memakai seri `OPT-ORD` biasa (contoh PRD "ORD-…-R1" tidak diterapkan); tautan ke pesanan asal ada di `ParentOrderId` dan riwayat status |
| Risiko tersisa | Resep yang sudah terkunci oleh pesanan asal tetap terkunci; remake dengan resep baru tidak membuka kunci resep lama. Belum dicoba di PostgreSQL |
| Perubahan sampingan | `NONE`. Probe QBE sementara dibuat dan dihapus |
| Status Git | Tambahan task ini: `??` `OptWarrantyClaimController.cs`, `OptWarrantyClaimService.cs`, `OptWarrantyClaimDtos.cs`, laporan ini; `M` `OptOperationResult.cs`, `Program.cs`, roadmap, traceability, `00-interview-decisions.md`. Berkas uji diabaikan Git |
| Langkah berikutnya | `BE-OPT-051` (adapter Medical Fee untuk pemeriksaan refraksi), `BE-OPT-052` (empat laporan), lalu `BE-OPT-053` (uji menyeluruh) |
