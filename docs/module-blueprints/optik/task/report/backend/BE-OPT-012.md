# Laporan Perubahan Backend — `BE-OPT-012`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-OPT-012` |
| Judul | Resep |
| Slice | `MVP-1` — `EPIC OPT-03` |
| Roadmap | `roadmap/backend-roadmap.md` (`OPT-RM-BE-001` r2) — Rincian `BE-OPT-012` |
| Trace | `FR-OPT-030`..`032`; `OPT-DEC-007`, `011`, `017`, `022`, `023`, `039`, `045`, `046`, `056`, `058`; `OPT-VAL-001`..`005`, `010`..`023`; `OPT-AC-001`, `003`, `004`, `012`, `028`; `UAT-05`, `UAT-06` |
| Contract version | `OPT-API-v1` grup Opt Prescription, `OPT-STATE-v1` bagian 2, `OPT-VAL-v1` bagian 1–2, `OPT-PERM-v1` — `approved` sisi Optik |
| Dependency | `BE-OPT-011` ✅ (pemeriksaan final, `OptPractitionerGuard`) |
| Klasifikasi | `MEDIUM` — satu controller (7 endpoint), satu service + kelas aturan statis, DTO |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/HealthServices/OpticalManagement/{Controllers,Services,DTOs}/`, `Program.cs`, `Tests/QuilvianSystemBackend.OpticalTests/` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `2a5094ca` (branch `yoga`) |
| Tanggal | 2026-11-03 |
| Status | Selesai dan tervalidasi. Belum di-commit |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `OpticalManagement` |
| Registry | `Opt` / `ACTIVE` |
| Keberlakuan | `NEW CODE`; `TOUCHED LEGACY` hanya `Program.cs` (DI) |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001`, `QBE-DTO-001`, `QBE-PAGE-001`, `QBE-CODE-001`, `QBE-AUD-001` |
| Wewenang terpisah | Source ✅ · migration tidak diperlukan (kolom sudah ada) · eksekusi database ✘ · deployment ✘ |

---

## 1. Masalah yang diperbaiki

Pemeriksaan refraksi sudah bisa difinalkan, tetapi resepnya belum bisa dicatat. Pesanan kacamata dan penjualan
lensa kontak (`BE-OPT-033`/`034`) bergantung pada resep yang sah dan masih berlaku.

---

## 2. Proses bisnis

1. **Resep internal.** Sesudah pemeriksaan Ibu Sari final, Rina (RO) menyimpan resep OD −1,50 / −0,50 × 180, OS −1,75, PD 62. Hasilnya `OPT-RX-RSMMC-000001`, terbit 03-11-2026 dan berlaku sampai 03-11-2027 (12 bulan dari pengaturan). dr. Bima (Sp.M) memakai form yang sama (`OPT-DEC-039`).
2. **Revisi oleh RO lain (`OPT-AC-003`).** Budi merevisi CYL −0,50 → −0,75 dengan alasan "verifikasi ulang dengan trial frame". Versi 1 atas nama Rina dan versi 2 atas nama Budi sama-sama terbaca.
3. **Salin resep luar (`UAT-05`).** Pak Andi membawa resep dari "Optik Sinar" tertanggal 20-10-2026: OD +1,00 / +0,50 × 90, OS +0,75. Hasil salinan:
   - tersimpan OD +1,50 / −0,50 × 180 dan OS +0,75;
   - ditandai "ditransposisi";
   - teks asli "OD +1,00 / +0,50 × 90; OS +0,75" ikut tersimpan.

   Bila disalin dua kali, ditolak "Resep luar ini sudah disalin (OPT-RX-…)".
4. **Ukuran ekstrem.** RO yang mengisi SPH −22,00 ditolak. dr. Bima, yang memegang hak `OverrideRange`, boleh menyimpannya dengan alasan "miopia tinggi, dikonfirmasi retinoskopi".
5. **Lensa kontak.** OD −2,00 BC 8,6 DIA 14,2, OS −2,25 BC 8,6 DIA 14,2, merk B bulanan. Lensa kosmetik ditulis power 0.
6. **Dipakai pesanan.** Resep bertanggal 03-11-2026 masih diterima pada 01-05-2027 dan ditolak pada 10-12-2027 (`OPT-DEC-023`). Pilihan resep untuk pesanan hanya menampilkan resep yang masih berlaku.

**Jalur tidak normal:**

| Keadaan | Hasil |
| --- | --- |
| Petugas penjualan membuat atau menyalin resep (`OPT-AC-001`) | `403 OPT-VAL-001`; tidak ada baris |
| RO mengisi SPH −22,00 (`OPT-AC-028`) | `403 OPT-VAL-004` "SPH di luar rentang −20,00..+20,00; hanya Sp.M yang dapat mengonfirmasi." |
| Sp.M tanpa alasan, atau Sp.M tanpa hak `OverrideRange` | `400 OPT-VAL-005` / `403 OPT-VAL-004` |
| CYL −0,75 tanpa AXIS (`OPT-AC-028`) | `400 OPT-VAL-012` |
| SPH −1,30, CYL plus pada resep internal/revisi, ADD 1,10, PD 45, AXIS tanpa silinder | `400 OPT-VAL-010` / `011` / `014` / `015` / `013` |
| Lensa kontak tanpa BC atau merk; BC 10 | `400 OPT-VAL-017` / `018` "BC harus antara 7,50 dan 9,50." |
| Resep luar tanpa asal fasilitas; tanggal 04-11-2026 | `400 OPT-VAL-019` / `020` |
| Revisi tanpa alasan | `400 OPT-VAL-021` |
| Revisi resep yang lensanya sudah dipesan (`UAT-06`, `OPT-AC-004`) | `409 OPT-VAL-022` "Resep terkunci karena lensa sudah dipesan." |
| Resep berusia 13 bulan dipakai pesanan (`OPT-AC-012`) | `409 OPT-VAL-023` "Resep berusia lebih dari 12 bulan, lakukan pemeriksaan ulang." |
| Lensa kontak kedaluwarsa | `409` "Resep lensa kontak kedaluwarsa, lakukan pemeriksaan ulang." (`OPT-DEC-046`) |
| Resep internal dari pemeriksaan draf; resep kedua berjenis sama dari pemeriksaan yang sama | `409` |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- Model dan konfigurasi `OptPrescription`/`OptPrescriptionVersion`, `OptSetting`.
- `optical-prescription-rules.js` (`FE-OPT-001`), supaya kalimat, pembagian galat keras vs di luar rentang, dan rumus transposisi sama persis.
- `AccessPermissionService.HasAccessAsync`, serta `PermissionRegistryDescriptor` (action hanya terdaftar dari atribut endpoint).
- `EncounterIntakeService` (pola seam kewenangan `protected virtual`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/OpticalManagement/Controllers/OptPrescriptionController.cs` | **Baru.** Tujuh endpoint (bagian 4) |
| `Areas/HealthServices/OpticalManagement/Services/OptPrescriptionService.cs` | **Baru.** `OptPrescriptionRules` (rentang, kelipatan, AXIS, lensa kontak, transposisi, ringkasan) dan `OptPrescriptionService` (buat, salin, revisi, daftar, pilihan, kelayakan pesanan, hak konfirmasi rentang) |
| `Areas/HealthServices/OpticalManagement/DTOs/OptPrescriptionDtos.cs` | **Baru.** `OptLensPowerDto`, `OptContactLensEyeDto`, permintaan, dan respons |
| `Areas/HealthServices/OpticalManagement/Services/OptOperationResult.cs` | **Diperbarui.** Kode `OPT-VAL-004`, `005`, `010`..`023`, `OPT-SETTING-MISSING` |
| `Program.cs` | **Diperbarui.** `AddScoped<OptPrescriptionService>()` |
| `Tests/QuilvianSystemBackend.OpticalTests/Be012PrescriptionTests.cs`, `Infrastructure/OptikUji.cs` | **Baru** (diabaikan Git). 38 uji, termasuk teori, dan penolong data uji bersama |
| `Tests/QuilvianSystemBackend.OpticalTests/OpticalPermissionRegistryTests.cs` | **Diperbarui.** Hitungan endpoint Optik 15 → 22 |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Enam endpoint kontrak, ditambah `GET range-override` (delta 1) |
| Database | Tanpa migration. Menulis `OptPrescription` dan `OptPrescriptionVersion`; versi tidak pernah diubah |
| Keamanan/Auth | `[AccessPermission("OptPrescription", Read/Create/Revise/OverrideRange)]`, lolos `PermissionRegistryValidator`. Service menambah syarat profesi, SIP, STR, dan untuk lewat rentang: Sp.M + `OverrideRange`. Log hanya memuat nomor resep, nomor versi, dan nama pelaku, tanpa nilai ukur |

**Delta terhadap kontrak:**

1. **`GET prescriptions/range-override` ditambahkan.** Kontrak menulis `OverrideRange` "dicek service, bukan atribut endpoint". Tetapi `PermissionRegistryDescriptor` hanya mendaftarkan action dari `[AccessAction]` pada endpoint, sehingga tanpa endpoint hak itu tidak pernah muncul di layar Akses Role dan tidak bisa diberikan. Endpoint ini membawa action tersebut dan memberi tahu layar apakah isian alasan konfirmasi perlu ditampilkan. Hak itu tetap dicek ulang di dalam buat/salin/revisi.
2. **Resep internal wajib dari pemeriksaan `Final`** (kontrak: "dari pemeriksaan"), dengan satu resep per jenis per pemeriksaan; koreksi lewat revisi. Resep luar ganda dicegah dengan pembanding pasien, jenis, penulis, fasilitas, dan tanggal. Tabel resep tidak punya kolom kunci idempoten, sehingga `Idempotency-Key` tidak dipakai.
3. **Aturan yang dirinci dari kamus data dan `FE-OPT-001`:**
   - mata yang diresepkan wajib ber-SPH (0 untuk plano);
   - power lensa kontak memakai aturan SPH (±20,00, kelipatan 0,25, boleh dikonfirmasi Sp.M);
   - transposisi hanya berlaku pada salinan resep luar kacamata, sedangkan resep internal dan revisi wajib sudah bernotasi minus (`OPT-VAL-011`).
4. **Kunci resep diisi task pesanan.** Di sini hanya ditegakkan penolakan revisi bila `LockedAt` terisi (`OPT-VAL-022`). `CheckUsableForOrderAsync` disiapkan untuk `OPT-VAL-023` pada task pesanan.

---

## 4. Dokumentasi endpoint

#### Health Services / Optical Management / Opt Prescription

Base URL: `api/v1/health-services/optical-management/prescriptions`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/range-override` | Apakah pengguna dapat mengonfirmasi ukuran di luar rentang | `OptPrescription : OverrideRange` |
| `GET` | `/` | Daftar resep: `PatientId`, `EncounterId`, `PrescriptionType`, `ValidOnly`, halaman | `OptPrescription : Read` |
| `GET` | `/options` | Resep berlaku untuk pesanan: `patientId`, `prescriptionType` | `OptPrescription : Read` |
| `GET` | `/{id}` | Detail beserta seluruh versi | `OptPrescription : Read` |
| `POST` | `/` | Resep internal (`RefractionExamId`, `PrescriptionType`, isi) | `OptPrescription : Create` |
| `POST` | `/external` | Salin resep luar (`EncounterId`, jenis, penulis, fasilitas, `IssuedDate`, isi) | `OptPrescription : Create` |
| `POST` | `/{id}/revisions` | Revisi (`ExpectedVersion`, isi lengkap, `RevisionReason`) | `OptPrescription : Revise` |

Kode status: `200` berhasil; `400` isian tidak sah; `403` bukan RO/Sp.M, izin tidak berlaku, atau lewat rentang tanpa wewenang; `404` tidak ditemukan; `409` terkunci, versi tertinggal, ganda, atau pemeriksaan belum final; `503` data HR tidak terbaca.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet test Tests/QuilvianSystemBackend.OpticalTests -p:RunAnalyzers=False` | 114 lulus, 0 gagal (38 baru) | `PASS` | Keluaran `dotnet test` |
| `OPT-AC-003`: resep internal lalu revisi oleh RO lain | Nomor `OPT-RX-RSMMC-000001`, berlaku 03-11-2027; versi 1 Rina, versi 2 Budi | `PASS` | Uji |
| `UAT-05`/`OPT-AC-028` transposisi + teks asli; salin ganda | Lulus; `409` | `PASS` | Uji |
| Transposisi sama dengan `FE-OPT-001` (3 kasus, termasuk AXIS 180 → 90) | Lulus | `PASS` | Uji teori |
| `OPT-AC-028` RO −22,00 → `403`; Sp.M + alasan → tersimpan; Sp.M tanpa alasan → `400`; Sp.M tanpa hak → `403` | Lulus | `PASS` | Uji |
| Aturan kacamata (11 kasus) dan lensa kontak (6 kasus); di luar rentang dikumpulkan, bukan galat keras | Lulus | `PASS` | Uji teori |
| `OPT-VAL-019`, `020`, `021`; `UAT-06`/`OPT-AC-004` terkunci; versi tertinggal | Lulus | `PASS` | Uji |
| `OPT-AC-012` resep 13 bulan; contoh `OPT-DEC-023` (01-05-2027 diterima, 10-12-2027 ditolak); lensa kontak kedaluwarsa; pasien/jenis tidak cocok | Lulus | `PASS` | Uji dengan jam tetap |
| `OPT-AC-001` petugas penjualan; hak konfirmasi hanya Sp.M | Lulus | `PASS` | Uji |
| Controller memetakan `200/400/403/404/409` | Lulus | `PASS` | Uji controller |
| `PermissionRegistryValidator` atas 22 endpoint Optik | Nol temuan | `PASS` | Uji registri |
| QBE checker atas 5 berkas + kontrol multi-baris | Kontrol terkena 5 temuan; 5 berkas bersih | `PASS` | Keluaran checker; probe dihapus |

**Tidak dijalankan:** panggilan HTTP ke backend yang menyala. Tabel Optik belum ada di devYoga karena migration belum diterapkan, dan wewenang database tidak diberikan.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) `OPT-AC-003`, `004`, `012`, `028` | Terpenuhi (`004` dengan kunci disimulasikan; pengisian kunci oleh task pesanan) | Uji |
| (2) `OPT-VAL-010`..`023` | Terpenuhi | Uji teori dan uji layanan |
| (3) `UAT-05`, `06` | Terpenuhi di harness | Uji |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Beri hak `OptPrescription : OverrideRange` hanya ke jabatan Sp.M (`OPT-PERM-v1` bagian 2). Walau diberikan ke RO, service tetap menolak karena profesinya bukan Sp.M |
| Masalah yang diketahui | Rentang BC/DIA bawaan (7,50–9,50 / 13,00–15,00) masih `PROPOSED` di traceability. Nilainya bisa diubah Admin Optik lewat pengaturan (`OPT-VAL-073`), jadi tidak memblokir. Konfirmasi RO/Sp.M tetap disarankan |
| Risiko tersisa | **Untuk task pesanan:** saat mengunci resep, `Version` resep wajib ikut dinaikkan, agar revisi yang berjalan bersamaan ditolak oleh pemeriksaan versi. Keselamatan klinis (salah tanda silinder) dijaga aturan CYL minus dan transposisi yang teruji, tetapi belum diuji di layar (`FE-OPT-012`) |
| Perubahan sampingan | `NONE`. Probe QBE sementara dibuat dan dihapus |
| Interupsi | `NONE` |
| Status Git | `M Program.cs`; `?? Areas/HealthServices/OpticalManagement/` bertambah `OptPrescriptionController.cs`, `OptPrescriptionService.cs`, `OptPrescriptionDtos.cs` |
| Langkah berikutnya | `BE-OPT-013` (pembaca Rekam Medis: dokumen per versi pemeriksaan dan resep). Sejajar dengan itu `BE-OPT-003` dan `BE-OPT-030` |
