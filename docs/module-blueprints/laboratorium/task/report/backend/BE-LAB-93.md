# Laporan Perubahan Backend — `BE-LAB-93`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-93` |
| Judul | Ringkasan tahunan Beranda dan `VAL-154` |
| Slice | `MVP-13a` — `EPIC-LAB-19` Beranda Lab mengikuti susunan v1 (BR-140) |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 6as.2 |
| Trace | `LAB-DEC-206`, `LAB-DEC-207`, `LAB-DEC-211`; `LAB-REQ-021` butir 2, 7, 8, 10; `FR-19.3` (*Jenis laboratorium*), `FR-19.4`, `FR-19.5`, `FR-19.8`; `02-backend-architecture.md` bagian 28 |
| Contract version | `LAB-API-v1` `r44` (39.2, 39.4, 39.8), `LAB-VAL-v1` `r20` (22.1, `VAL-154`), `LAB-PERM-v1` revision 16 (18.2) — **`approved`** 2026-10-08 oleh Yoga Aji Pratama lewat `LAB-REQ-021` |
| Dependency | `BE-LAB-92` ⚠ (working tree, 2026-10-08) |
| Klasifikasi | `MEDIUM` — satu method service baru, satu endpoint baca, satu aturan validasi; nol skema, nol izin baru |
| Task mode | `BACKEND` — wewenang tulis dari pemilik modul (*"lanjut"* 2026-10-08 atas tawaran `BE-LAB-93`) |
| Target tulis | `NewQuilvianSystemBackend` — source Laboratorium dan dokumen blueprint `laboratorium`. Frontend read-only |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `38c8a4f4` (branch `yoga`, upstream `origin/yoga`) + working tree `BE-LAB-92` |
| Tanggal | 2026-10-08 |
| Status | ⚠ **`SELESAI DENGAN BATAS VERIFIKASI`** — harness 56/56, HTTP baca-saja 16/16, build 0 error, validator lolos. **Batas:** `403` belum diamati di runtime |

---

## 1. Masalah yang diperbaiki

Beranda belum punya data untuk tiga grafik v1 — pemeriksaan per disiplin, sebarannya, dan tren pesanan bulanan — maupun
kartu *Jenis laboratorium*. Rekap lama hanya menghitung **pesanan** per disiplin untuk rentang bebas, sedangkan putaran
25 memutuskan grafik menghitung **pemeriksaan** pada **tahun terpilih**, dan tren membandingkan tahun itu dengan tahun
sebelumnya.

**Contoh jebakan yang kini tertangani.** Pesanan PA diminta 31 Desember 2026 pukul 23.30 WIB tersimpan
`2026-12-31T16:30Z`; pesanan pukul 00.00 WIB 1 Januari 2027 tersimpan `2026-12-31T17:00Z`. Keduanya bertanggal UTC
sama, tetapi yang pertama milik 2026 dan yang kedua milik 2027.

---

## 2. Proses bisnis

| Langkah | Isi |
| --- | --- |
| Pelaku | Pemegang `LabOrder : Read` |
| Pemicu | Membuka Beranda, mengganti tahun pada pemilih, atau menekan *Perbarui Data* |
| 1 | Layar meminta `GET /lab-orders/dashboard/yearly?year=2026`; tanpa `year` berarti tahun berjalan WIB |
| 2 | Server memeriksa `VAL-154`: tahun 2000 sampai tahun berjalan WIB. Di luar itu → `422` |
| 3 | Rentang tahun dibaca WIB menurut **waktu diminta** pesanan |
| 4 | Per disiplin dihitung: pemeriksaan yang tidak gugur/batal **ditambah** permintaan yang belum masuk wadah, **hanya** pada pesanan yang tidak dibatalkan. Disiplin = disiplin pesanan, jatuh ke katalog; yang tetap kosong dihitung sebagai *belum tergolong* |
| 5 | *Jenis laboratorium* = banyaknya dari tiga disiplin yang angkanya > 0 |
| 6 | Tren: pesanan bukan *Dibatalkan*/*Draft* per bulan WIB, tahun terpilih dan tahun sebelumnya, selalu 12 bulan |
| Jalur tidak normal | `year` bukan angka → `400`; di luar batas → `422` *"Tahun tidak sah. Pilih tahun 2000 sampai tahun berjalan."*; belum masuk → `401`; tanpa izin → `403`; tahun tanpa data → semua 0 dengan `200` |
| Hasil | Angka grafik; tidak ada data yang berubah |

**Contoh angka** (harness, tahun 2026): pesanan PK berisi 2 pemeriksaan, 1 pemeriksaan gugur, 1 permintaan belum masuk
wadah, 1 permintaan sudah masuk wadah, dan 1 permintaan batal → PK menyumbang **3**. Pesanan PK lain yang
**dibatalkan** berisi 3 pemeriksaan tidak dibatalkan → **0**. Ditambah satu pesanan tanpa disiplin yang katalognya PK
→ PK **4**.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Governance yang sama dengan `BE-LAB-92` (dibaca pada sesi yang sama, tidak berubah); `LabOperationalReportController.cs`
(pola `422`); `LabWorklistController.cs` (pola tangkap exception); `LabDashboardService.cs` dan `LabDashboardDtos.cs`
hasil `BE-LAB-92`; `LabOrderedProcedure.cs`; `LaboratoryEnums.cs`.

### 3.2 Berkas yang berubah

| Berkas | Status | Perubahan |
| --- | --- | --- |
| `Areas/HealthServices/LaboratoryManagement/DTOs/LabDashboardDtos.cs` | Diperbarui (baru di `BE-LAB-92`) | `LabDashboardYearlyQuery`, `LabDashboardYearlyResponse`, `LabDashboardDisciplineCountResponse`, `LabDashboardMonthlyOrderResponse` — persis `r44` 39.4 |
| `Areas/HealthServices/LaboratoryManagement/Services/LabDashboardService.cs` | Diperbarui (baru di `BE-LAB-92`) | `GetYearlyAsync(year?, asOf?)`; konstanta `MinYear = 2000`; `LabDashboardValidationException` (`VAL-154`). Hitungan disiplin dikelompokkan di SQL; tren menarik **satu kolom waktu** lalu dikelompokkan per bulan WIB di memori |
| `Areas/HealthServices/LaboratoryManagement/Controllers/LabOrderController.cs` | Diperbarui | Action `GET dashboard/yearly`; `LabDashboardValidationException` → `422` |
| Dokumen blueprint | Diperbarui | Laporan ini; kolom *Status* `r44` 39.2; roadmap; traceability; manifest |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Satu endpoint baca baru sesuai `r44` 39.4 dan `VAL-154`; **aditif** |
| Database | `NOT APPLICABLE` — nol tabel, kolom, index, migration; tidak ada `database update`. Lama kueri diukur (bagian 5): **tidak** perlu index (`LAB-REQ-021` butir 10) |
| Keamanan/Auth | `LabOrder : Read`, kunci aksi `Read` / `Read Lab Order` yang sudah ada — nol aksi baru. Jawaban hanya angka, tanpa identitas pasien |

---

## 4. Dokumentasi endpoint

#### Health Services / Laboratory Management / Lab Order

Base URL `api/v1/health-services/laboratory-management/lab-orders`.

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/dashboard/yearly` | Pemeriksaan per disiplin, sebaran, jenis laboratorium, tren bulanan untuk satu tahun | `LabOrder : Read` |

| Parameter | Tipe | Wajib | Bawaan | Validasi |
| --- | --- | :---: | --- | --- |
| `year` | `int` | Tidak | Tahun berjalan WIB | `VAL-154`: 2000 sampai tahun berjalan WIB → selain itu `422`; bukan angka → `400` |

Jawaban asli DB dev 2026-10-08 (tahun 2026): Patologi Klinik 8, Patologi Anatomi 5, Mikrobiologi 5, belum tergolong 0,
*Jenis laboratorium* 3; tren 2026 September 17, Oktober 5, bulan lain 0; 2025 seluruhnya 0.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=False` (server lokal BE/FE dimatikan dulu) | Berhasil, 0 error, nol warning dari berkas yang disentuh, 1 menit 52 detik | `PASS` | Log build sesi |
| Harness InMemory | **56/56** — 22 uji baru + 34 uji `BE-LAB-92` sebagai regresi | `PASS` | Rincian di bawah |
| Startup Development + `PermissionRegistryValidator` | `/health` `200`; 1659 kunci dari 1659 kemampuan — **sama** dengan `BE-LAB-92`, nol aksi baru; nol `fail:`/`crit:` | `PASS` | Log server |
| HTTP baca-saja — backend lokal + DB dev, superadmin | **16/16** | `PASS` | Rincian di bawah |
| Lama kueri tahunan | 132–315 ms dalam 5 kali panggil, **termasuk** perjalanan HTTP, pada 23 pesanan | `PASS` | Di bawah ambang 1 detik `LAB-REQ-021` butir 10 |
| `403` endpoint baru di runtime | — | `NOT RUN` | Sama dengan `BE-LAB-92` — tidak ada sandi akun tanpa `LabOrder : Read` |

**Harness — 22 uji baru:**

- `AC-297`: tiga butir disiplin urutan tetap; PK 4 (pemeriksaan gugur, permintaan *Fulfilled*, permintaan batal,
  pesanan batal, pesanan terhapus, pemeriksaan terhapus, dan pesanan 00.00 WIB 1 Januari 2027 **tidak** terhitung);
  Mikro 2 dari permintaan belum masuk wadah; PA 1 dari pesanan 23.30 WIB 31 Desember; belum tergolong 1; *Jenis
  laboratorium* 3; tahun 2025 berbeda (PK 1, jenis 1).
- `AC-298`: 12 bulan berurutan; Januari 1 (23.30 WIB 31 Januari); Mei 3 (batal dan terhapus keluar); Juni 1 (*Draft*
  keluar); Desember 1; bulan lain 0; pembanding Mei dan Desember 2025 (23.59 WIB) masing-masing 1; tahun 2025 dengan
  pembanding 2024 nol.
- `VAL-154`: kosong → 2026; 2000 sah dan nol; 1999 ditolak dengan pesan kontrak persis; 2027 ditolak pada 2026; 2027
  sah pada `2026-12-31T17:30Z` dan ditolak pada `2026-12-31T16:59Z`.

**HTTP baca-saja 16/16:** tanpa masuk → `401`; tanpa `year` → `200`, tahun = tahun berjalan WIB; tepat tujuh ruas;
tiga butir disiplin urutan tetap; 12 bulan; `activeDisciplineCount` konsisten; `year` eksplisit = tanpa `year`;
`year=2000` → `200` nol; `year=1999` dan tahun depan → `422` dengan pesan kontrak; `year=duaribu` → `400`; regresi
`today` dan `recent-orders` `200`; total tren dua tahun (22) ≤ total pesanan tercatat (23, selisihnya satu pesanan
batal). Kueri `GroupBy` dengan jatuhan disiplin berjalan di Npgsql tanpa `500`. Nol permintaan tulis.

Uji manual: `NOT APPLICABLE` — task ini tanpa layar.

**Tidak dijalankan:** analyzer (`-p:RunAnalyzers=False`); `403` runtime; uji otomatis di repository (`LAB-RDY-C04`) —
harness tinggal di scratchpad sesi.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-297` — pesanan batal tidak terhitung; permintaan belum masuk wadah terhitung; *Fulfilled* tidak ganda; gugur tidak; jatuh ke katalog; tak tergolong terpisah; tahun berbeda → angka berbeda | Terpenuhi (harness + HTTP) | Bagian 5 |
| `AC-298` — 12 bulan dua seri; batal/draft tidak; batas bulan dan tahun WIB | Terpenuhi (harness + HTTP) | Bagian 5 |
| `VAL-154` — 1999 dan tahun depan `422`; kosong = tahun berjalan WIB; pergantian tahun WIB; bukan angka `400` | Terpenuhi (harness + HTTP) | Bagian 5 |
| `AC-301` — `403` tanpa izin | **Sebagian** — atribut terpasang, validator lolos, `401` teramati; `403` runtime belum | Bagian 5 |
| Lama kueri tahunan dicatat | Terpenuhi — 132–315 ms | Bagian 5 |
| Build hijau, laporan, kolom *Status* `r44` 39.2 | Terpenuhi | Berkas ini; `contracts/api-contract.md` |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Angka grafik **sengaja berbeda** dari laporan operasional `r37`, yang menghitung pemeriksaan **dirilis** menurut tanggal rilis (`r44` 39.8) |
| Masalah yang diketahui | Tidak ada pada scope task. Cacat CITO 28.8 tidak menyentuh endpoint ini |
| Risiko tersisa | Kueri memindai `LabOrder` tanpa index waktu; aman pada volume dev (23 pesanan). Ukur ulang bila volume produksi besar |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE`. Server lokal BE dan FE dimatikan sebelum build lalu dinyalakan lagi. Notifikasi *exit code 127* pada kedua proses latar adalah akibat proses dihentikan, bukan galat |
| Status Git | Source: `M LabOrderController.cs`, `M LabWorklistService.cs`, `M Program.cs`, `?? LabDashboardDtos.cs`, `?? LabDashboardService.cs` (gabungan `BE-LAB-92` dan `BE-LAB-93`); ditambah dokumen blueprint sesi ini. Tidak di-stage, tidak di-commit |
| Langkah berikutnya | `MVP-13a` selesai pada kode. Lanjut `FE-LAB-52` lalu `FE-LAB-53` (`MVP-13b`) |
