# Laporan Perubahan Backend — `BE-OPT-060`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-OPT-060` |
| Judul | Kunci simpan ganda pada empat `POST` pembuat |
| Slice | Amendment kesiapan — `EPIC OPT-03`, `OPT-04`, `OPT-07` |
| Roadmap | `roadmap/backend-roadmap.md` (`OPT-RM-BE-001` r4) — Rincian `BE-OPT-060` |
| Trace | `FR-OPT-033`, `044`, `072`; `NFR-003`; `OPT-DEC-077`; `OPT-AC-035`; `UAT-33`; risiko `R1` |
| Contract version | `OPT-API-v1.2`, `OPT-VAL-v1.1` bagian 7, `erd/data-dictionary.md` r2, `02-backend-architecture.md` r2 — `approved` sisi Optik, Yoga Aji Pratama 2026-11-06, terkunci hash di manifest r2 |
| Penyetuju modul lain | Tidak ada kode modul lain yang berubah |
| Approval task | Yoga Aji Pratama, 2026-11-06 ("lanjutkan langkah berikutnya", menjawab tawaran mengerjakan `BE-OPT-060`) |
| Dependency | `BE-OPT-012` ✅, `BE-OPT-032` ✅, `BE-OPT-050` ✅ |
| Klasifikasi | `HEAVY`, skor 9: berkas diperiksa 2, berkas diubah 2, logika bisnis 1, kontrak API 2, database 2 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/HealthServices/OpticalManagement/{Models,Services,Controllers,DTOs}/`, `Repositories/Configurations/HealthServices/OpticalManagement/`, `Migrations/`, `Tests/QuilvianSystemBackend.OpticalTests/` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `06e40fb7` (branch `yoga`) |
| Tanggal | 2026-11-06 |
| Status | Selesai dan tervalidasi di harness. Migration diterapkan ke devYoga 2026-11-06 atas persetujuan pemilik; server dev bersama dan produksi belum. Belum di-commit. **Belum boleh terbit sebelum `FE-OPT-040`** (kode `FE-OPT-040` sudah selesai, belum terbit) |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `OpticalManagement` |
| Registry | `Opt` / `ACTIVE` |
| Keberlakuan | `NEW CODE` di berkas Optik yang sudah ada |
| QBE yang berlaku | `QBE-ENT-001`, `QBE-CFG-001`, `QBE-SVC-001`, `QBE-API-001`, `QBE-VAL-001`, `QBE-DTO-001`, `QBE-LOG-001`, `QBE-CODE-004` |
| Wewenang terpisah | Source ✅ · pembuatan migration ✅ · eksekusi database ✅ hanya devYoga, persetujuan Yoga Aji Pratama 2026-11-06 · deployment ✘ |

---

## 1. Masalah yang diperbaiki

Sebelum task ini, hanya `POST visits` yang membaca header `Idempotency-Key`. Resep, pesanan, dan keluhan tidak punya pengaman serupa.

Akibatnya, bila jaringan putus sesudah server menyimpan, lalu petugas menekan Simpan lagi, data bisa tercatat dua kali. Yang paling berisiko adalah keluhan.

**Contoh:** server sudah menyimpan keluhan KLM-0001, tetapi jawabannya tidak sampai ke layar. Dedi menekan Simpan lagi, dan muncul keluhan kedua. Dua keluhan itu bisa berujung dua remake gratis.

---

## 2. Proses bisnis

| Unsur | Isi |
| --- | --- |
| Tujuan | Satu niat menyimpan menghasilkan satu data, walaupun terkirim berkali-kali |
| Pelaku | RO/Sp.M (resep), petugas penjualan (pesanan), petugas atau RO/Sp.M (keluhan) |
| Pemicu | Tombol Simpan ditekan lagi, atau layar mengirim ulang sesudah jaringan putus |
| Prasyarat | Layar mengirim header `Idempotency-Key`. Bagian layar dikerjakan `FE-OPT-040` |

**Langkah utama:**

1. Layar mengirim permintaan beserta kunci.
2. Server memeriksa kunci lebih dulu, sebelum aturan status dan isian.
3. Bila kunci kosong atau Guid nol, server menolak `400` dan tidak menyimpan apa pun.
4. Bila kunci sudah pernah dipakai, server menjawab `200` berisi data pertama dengan `IsReplay = true`. Log mencatat kiriman ulang, bukan pembuatan baru.
5. Bila kunci baru, aturan biasa berjalan, lalu data tersimpan beserta kuncinya.

**Aturan yang berlaku:**

| Aturan | Contoh |
| --- | --- |
| Kunci diperiksa paling awal | Resep dari pemeriksaan EXM-0001 sudah tersimpan. Kiriman ulang berkunci sama menerima resep itu, bukan `409` "sudah ada". Kiriman berkunci **baru** tetap kena `409` itu |
| Isi tidak dibandingkan | Kunci sama dengan isi berbeda tetap mengembalikan data pertama. Layar mencegahnya dengan membuat kunci baru saat isi diubah |
| Pesanan remake tanpa kunci | Remake dibuat sistem, sehingga kolomnya kosong. Banyak baris kosong tidak saling bentrok |

**Jalur tidak normal:**

| Keadaan | Hasil |
| --- | --- |
| Dua kiriman serentak berkunci sama | Keduanya lolos pemeriksaan awal, lalu indeks unik menolak yang kalah. Server membaca data pemenang dan mengembalikannya, sehingga pengguna tidak melihat galat |
| Penyimpanan gagal karena sebab lain | Tidak ada data berkunci itu, sehingga galat aslinya diteruskan dan tidak ditutupi sebagai kiriman ulang |
| Kunjungan sudah ditutup sesudah data pertama tersimpan | Kiriman ulang tetap menerima data pertama. Kunci baru ditolak `OPT-VAL-030` |

**Hasil akhir:** satu baris data per kunci. Tidak ada status baru, dan tidak ada perubahan pada transisi status.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md`, `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`, `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`, `rules/backend/` (`TASK_RULES`, `DATABASE_RULES`, `TASK_CLASSIFICATION`, `REPORT_TEMPLATE`), kontrak dan arsitektur r2, `FinPatientRefundRequest` beserta service dan configuration-nya, `OptVisitController`, `OptVisitService`, `OptHttp`, `EncounterIntakeService`, serta harness `Tests/QuilvianSystemBackend.OpticalTests/Infrastructure/`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Models/OptPrescription.cs`, `OptOrder.cs`, `OptWarrantyClaim.cs` | Tambah `IdempotencyKey` (`Guid?`) |
| `OptPrescriptionConfiguration.cs`, `OptOrderConfiguration.cs`, `OptWarrantyClaimConfiguration.cs` | Indeks unik terfilter `UX_<Tabel>_IdempotencyKey` |
| `DTOs/OptPrescriptionDtos.cs`, `OptOrderDtos.cs`, `OptWarrantyClaimDtos.cs` | Tambah `IsReplay` pada response; tidak disimpan |
| `Services/OptPrescriptionService.cs` | `CreateInternalAsync`, `CopyExternalAsync` menerima kunci; gerbang kunci di awal; pemulihan kiriman serentak |
| `Services/OptOrderService.cs`, `OptWarrantyClaimService.cs` | `CreateAsync` menerima kunci; gerbang kunci di awal; pemulihan kiriman serentak. Jalur remake tidak berubah |
| `Controllers/OptPrescriptionController.cs`, `OptOrderController.cs`, `OptWarrantyClaimController.cs` | Membaca header `Idempotency-Key`; teks log kiriman ulang |
| `Migrations/20261106025752_AddOpticalCreateIdempotencyKey.cs` + `.Designer.cs` | **Baru.** Tiga kolom + tiga indeks; `Down` membalik keenamnya |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | +24 / −0 baris, hanya tiga tabel Optik |
| `Tests/…/Infrastructure/KunciSimpanUji.cs` | **Baru.** Helper dua argumen untuk uji lama, peniru kiriman serentak, dan pemasang indeks unik sungguhan |
| `Tests/…/Infrastructure/PesananUji.cs` | Kait `SebelumSimpan` pada interceptor uji |
| `Tests/…/Be012PrescriptionTests.cs`, `Be032OrderTests.cs`, `Be050WarrantyClaimTests.cs`, `Be053EndToEndTests.cs` | 20 uji baru; pemanggilan controller lama diberi kunci |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Mengikuti `OPT-API-v1.2`. **Perilaku berubah:** empat `POST` menolak kiriman tanpa header. Response hanya bertambah `IsReplay`. Registri izin tetap 45 endpoint |
| Database | Migration `20261106025752_AddOpticalCreateIdempotencyKey`: kolom boleh kosong tanpa nilai bawaan, sehingga tanpa pengisian data lama. **Diterapkan ke devYoga 2026-11-06**; lingkungan lain belum |
| Keamanan/Auth | Izin tidak berubah. Kiriman ulang tetap melewati `[AccessPermission]`, tetapi tidak melewati pemeriksaan izin praktik karena kunci diperiksa lebih dulu; data yang dikembalikan sudah dibuat sah sebelumnya. Kunci berupa Guid acak dan tidak ditulis ke log |

---

## 4. Dokumentasi endpoint

Base URL: `api/v1/health-services/optical-management`. Keempatnya kini wajib membawa header `Idempotency-Key` (Guid).

#### Health Services / Optical Management / Opt Prescription

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/prescriptions` | Buat resep internal dari pemeriksaan | `OptPrescription : Create` |
| `POST` | `/prescriptions/external` | Salin resep luar | `OptPrescription : Create` |

#### Health Services / Optical Management / Opt Order

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/orders` | Buat pesanan `Draft` | `OptOrder : Create` |

#### Health Services / Optical Management / Opt Warranty Claim

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/warranty-claims` | Catat keluhan pada kunjungan baru | `OptWarrantyClaim : Create` |

**Kode status baru atau berubah:**

| Kode | Arti bagi pengguna |
| --- | --- |
| `200`, `IsReplay = false` | Data baru tersimpan |
| `200`, `IsReplay = true` | Data ini sudah tersimpan dari kiriman sebelumnya; tidak ada data baru |
| `400` `OPT-INVALID-REQUEST` | Layar tidak mengirim kunci, misalnya "Header Idempotency-Key wajib diisi agar Simpan dua kali tidak membuat pesanan ganda." |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Cek server lokal sebelum build | Tidak ada proses server `dotnet`/`node` berjalan | `PASS` | `tasklist` |
| `dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=False` | 0 error; 424 warning; ±1 menit 34 detik | `PASS` | Keluaran build |
| `dotnet ef migrations add AddOpticalCreateIdempotencyKey --no-build` | Berhasil | `PASS` | Berkas migration |
| `dotnet ef migrations script` naik dan turun (luring) | Naik: 3 `ALTER TABLE … ADD`, 3 `CREATE UNIQUE INDEX … WHERE "IdempotencyKey" IS NOT NULL`, sama dengan DDL kamus data. Turun: 3 `DROP INDEX`, 3 `DROP COLUMN`, tidak menyentuh tabel lain | `PASS` | Keluaran perintah |
| `dotnet test Tests/QuilvianSystemBackend.OpticalTests -p:RunAnalyzers=False` | 267 lulus, 0 gagal (247 lama + 20 baru); 4 menit 41 detik | `PASS` | Keluaran `dotnet test` |
| Uji mutasi 1: kunci resep yang sudah dipakai tidak dikembalikan di awal | 1 uji gagal (`…bukan_409`); berkas dipulihkan (`cmp` identik) | `PASS` | Keluaran `dotnet test` |
| Uji mutasi 2: pesanan tidak mengambil data pemenang saat bentrok | 1 uji gagal (`…kiriman_serentak…pesanan_pemenang`, `DbUpdateException`); berkas dipulihkan (`cmp` identik) | `PASS` | Keluaran `dotnet test` |
| QBE checker `-Mode Strict -Path` atas 15 berkas produksi, pemanggilan terpisah dari `dotnet test` | `Files evaluated: 15`, `VIOLATION: 0`, `REVIEW: 0`, `PASS` | `PASS` | Keluaran checker |
| Kontrol negatif `ZzzRegistryProbe` (pemanggilan yang sama) | Terkena `QBE-ENT-001`, `QBE-CFG-001`, `QBE-MOD-002`; probe dihapus | `PASS` | Keluaran checker |
| Penerapan ke devYoga (`QuilvianNewDevYoga`, PostgreSQL 15.15), 2026-11-06, atas persetujuan pemilik: `migrations list` | Hanya migration ini yang tertunda | `PASS` | Keluaran perintah |
| Preflight baca-saja | Kolom 0, indeks 0, riwayat 380, ketiga tabel 0 baris, tidak ada koneksi lain memegang kunci | `PASS` | Program Npgsql di scratchpad |
| Skrip naik `20261104020212` → `20261106025752`, dalam satu transaksi, `lock_timeout` 15 detik | Berhasil | `PASS` | Program Npgsql |
| Pemeriksaan sesudahnya | Kolom 3, indeks 3 sama dengan DDL kamus data, riwayat 381, ketiga tabel tetap 0 baris; `migrations list` 0 tertunda | `PASS` | Baca-saja dan `migrations list` |

**Catatan cara uji kiriman serentak.** Harness SQLite sengaja mencabut sifat unik pada indeks tersaring (`TestDatabase.ModelUjiSqlite`), sehingga percobaan pertama tidak bentrok dan tiga uji gagal. Uji kiriman serentak kini memasang indeks unik yang sama dengan migration lewat `TegakkanKunciUnikAsync`, lalu kiriman "pemenang" disisipkan tepat sebelum penyimpanan. Bentrok dan pemulihannya sungguh terjadi, tetapi ini **peniruan satu koneksi**, bukan dua kiriman yang benar-benar bersamaan.

Uji manual: `NOT FEASIBLE` — data prasyarat `B1`, `B2` di devYoga masih kosong, sehingga resep, pesanan, dan keluhan tidak bisa dibuat dengan data asli.

**Tidak dijalankan:**

- Penerapan ke server dev bersama dan produksi (wewenang hanya untuk devYoga).
- `dotnet ef database update`: penerapan memakai skrip khusus, agar hanya migration ini yang berjalan.
- Skrip turun terhadap devYoga. Isinya sudah dibaca (bagian kriteria 6) dan tersedia lewat `migrations script`, tetapi tidak dijalankan.
- Dua kiriman yang benar-benar serentak di PostgreSQL. Menulis data uji ke devYoga di luar wewenang; dicatat sebagai sisa bukti untuk `B5`.
- Build dengan analyzer (build penuh terlalu lama; `-p:RunAnalyzers=False` sesuai roadmap).

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) `OPT-AC-035`: keempat `POST` dikirim ulang berkunci sama menjawab `200`, data pertama, `IsReplay = true`, jumlah baris tetap satu | Terpenuhi | Uji `BE_OPT_060_OPT_AC_035_*` di `Be012`, `Be032`, `Be050`; `Be053` lewat controller |
| (2) Header kosong atau Guid nol ditolak `400` `OPT-INVALID-REQUEST` tanpa baris tersimpan | Terpenuhi | Uji `*_tanpa_kunci_ditolak_400_*`; lewat controller `BadRequestObjectResult` |
| (3) Kunci diperiksa paling awal; kunci **baru** tetap kena `409` | Terpenuhi | `…bukan_409`, `…kirim_ulang_resep_luar…`, `*_diperiksa_sebelum_status_kunjungan`; uji mutasi 1 |
| (4) Pesanan remake berkunci kosong tidak bentrok | Terpenuhi | Dua baris kosong pada indeks unik sungguhan; alur remake `Be050` menyimpan kunci kosong |
| (5) Log kiriman ulang "diulang dengan kunci yang sama", tanpa kolom Sensitif | Terpenuhi | Uji controller `Be053`; teks log hanya memuat nomor dokumen |
| (6) `Down` hanya menghapus tiga indeks dan tiga kolom | Terpenuhi | Skrip turun luring |
| (7) Kiriman serentak: satu baris, kedua jawaban menunjuk data yang sama | **Sebagian** | Peniruan pada indeks unik sungguhan untuk resep, pesanan, dan keluhan; uji mutasi 2. Dua koneksi serentak di PostgreSQL belum dijalankan; sisa bukti untuk `B5` |
| DoD: migration belum diterapkan tanpa izin | Terpenuhi | Diterapkan ke devYoga hanya sesudah persetujuan pemilik 2026-11-06 |
| DoD: laporan dan traceability diperbarui | Terpenuhi | Laporan ini; roadmap r4; traceability r3 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | **Backend ini tidak boleh terbit sebelum `FE-OPT-040`.** Layar yang ada belum mengirim kunci, sehingga Simpan resep, pesanan, dan keluhan akan ditolak `400`. Frontend terbit lebih dulu atau bersamaan |
| Masalah yang diketahui | **Selisih implementasi dengan arsitektur r2 bagian 10 dan 11:** dokumen menyebut menangkap pelanggaran unik `23505` lewat `IsUniqueViolation`. Implementasinya menangkap `DbUpdateException`, lalu mencari data berkunci sama; bila tidak ada, galat aslinya diteruskan. Hasilnya sama, tidak bergantung penyedia basis data, dan bisa diuji di harness. Dokumen terkunci hash, sehingga penyegarannya dicatat untuk pass blueprint berikutnya, bersama label **Rencana** pada kontrak dan kamus data yang kini sudah tersedia |
| Risiko tersisa | Kiriman serentak sungguhan belum terbukti di PostgreSQL. Nomor dokumen milik kiriman yang kalah bisa berlubang (diterima, `02-backend-architecture.md` bagian 10). Kunci sama dengan isi berbeda mengembalikan data pertama, sehingga pencegahnya ada di layar |
| Perubahan sampingan | `NONE`. Probe QBE sementara dan salinan mutasi dibuat lalu dihapus atau dipulihkan |
| Interupsi | `NONE` |
| Status Git | `M` 12 berkas `Areas/HealthServices/OpticalManagement/`, `M` 3 configuration, `M` `Migrations/ApplicationDbContextModelSnapshot.cs`, `??` dua berkas migration, `??` laporan ini, `M` dokumen blueprint Optik. `Tests/` diabaikan `.gitignore`, sehingga uji baru dan `Infrastructure/KunciSimpanUji.cs` perlu `git add -f`. Belum di-commit |
| Langkah berikutnya | `FE-OPT-040` ✅ selesai. Penerapan ke lingkungan lain butuh izin terpisah; periksa `migrations list` dulu. Uji alur asli sesudah data `B1`, `B2` terisi |
