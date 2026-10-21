# Laporan Perubahan Backend — `BE-LAB-100`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-100` |
| Judul | Laporan Pembatalan: unduhan `.xlsx` dan pencatat cetak |
| Slice | `S16b-1` Laporan Pembatalan Order & Pemeriksaan; gelombang `MVP-15d` |
| Roadmap | `docs/module-blueprints/laboratorium/roadmap/backend-roadmap.md` bagian 6au.5 |
| Trace | `FR-20.3`, `FR-20.8`, `FR-20.9`; `LAB-DEC-233`, `LAB-DEC-248`; `AC-332`, `AC-337` (sisi Pembatalan) |
| Contract version | `LAB-API-v1` `r49` 44.2 (`GET /export`, `POST /print-log`), 44.4; `LAB-PERM-v1` revision 17 19.3, 19.5 — **`approved` 2026-10-20** |
| Dependency | `BE-LAB-99`, `BE-LAB-98` — selesai |
| Klasifikasi | `MEDIUM` — skor 7: repository 0, berkas diperiksa 1 (±12), berkas diubah 1 (5), logika bisnis 1, kontrak API 2 (dua endpoint baru), database 1 (kueri baca yang sama), keamanan/auth 1 (aksi `Export`, data pasien di berkas), UI 0 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/HealthServices/LaboratoryManagement/{Controllers,Services,DTOs}`, laporan ini, baris status roadmap/traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `4d3e4f98` (branch `yoga`) + working tree `BE-LAB-96`..`99` |
| Tanggal | 2026-10-20 |
| Status | ✅ **Selesai** — `403` bagi pemegang `Read` tanpa `Export` terbukti 2026-10-20 dengan akun dr. Bima (bagian 8). Semula *batas verifikasi* |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `LaboratoryManagement` |
| Pemilik / prefix registry | `Lab` — Laboratory, **`ACTIVE`** |
| Keberlakuan | `TOUCHED LEGACY` atas kode baru gelombang ini (`LabCancellationReportController`, `LabCancellationReportService`, `LabCancellationReportDtos`, `LabReportXlsxWriter`) dan `LabFilterMetadataFactory` |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-DTO-001`, `QBE-VAL-001`, `QBE-LOG-001`, `QBE-AUD-001`, `QBE-MOD-001` |
| QBE yang tidak berlaku | `QBE-ENT-*`, `QBE-CFG-*`, `QBE-DB-*`, `QBE-CODE-*` — nol entity, configuration, migration, nomor bisnis |
| Branch | `yoga` |

---

## 1. Masalah yang diperbaiki

Laporan Pembatalan (`BE-LAB-99`) baru dapat dibaca di layar. Kepala instalasi belum dapat **mengunduh** atau **mencetak**
daftar pembatalan untuk rapat mutu, dan artifact meminta Excel serta cetak (`LAB-EVD-014` §12, `LAB-DEC-233`).

## 2. Proses bisnis

1. Pemegang `LabCancellationReport : Export` menekan **Unduh Excel** → `GET /export` dengan penyaring aktif.
2. Backend membentuk **seluruh** baris hasil penyaring — penyaring, urutan, dan pelengkap baris **sama** dengan daftar;
   halaman diabaikan — lalu menulis `.xlsx`: dua belas kolom BR-151, waktu **WIB**, label Bahasa Indonesia (*Pesanan*/
   *Pemeriksaan*, *Diminta*, *Layak tagih*, …), ***Alasan tidak dicatat*** bagi alasan kosong, dan baris total per jenis.
3. Unduhan dicatat — laporan, periode, penyaring, jumlah baris, pelaku — **tanpa** No. RM, nama pasien, maupun alasan.
   Berkas sendiri **memuat** identitas pasien (isi laporan), sehingga keluarnya dijaga izin `Export` yang terpisah.
4. Untuk **Cetak**, layar lebih dulu memanggil `POST /print-log` dengan penyaring yang sama; backend memvalidasi periode,
   mencatat, dan menjawab `200`. Layar baru membuka dialog cetak sesudah itu (fail-closed — sisi layar `FE-LAB-55`).

**Contoh:** daftar 2026 di devYoga dibuka dengan `pageSize=1` (satu baris tampil); unduhan dengan penyaring yang sama
tetap berisi **2** baris — sama dengan total — dan log menulis *"… — seluruh petugas — 2 baris"*.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`LabCancellationReportController.cs`, `LabCancellationReportService.cs`, `LabCancellationReportDtos.cs` (`BE-LAB-99`);
`LabReportXlsxWriter.cs`, `LabReportOutputLogger.cs`, `LabOrderStatisticsReportController.cs` (pola `BE-LAB-98`);
`LabFilterMetadataFactory.cs` (label status); `Helpers/AppDateTimeHelper.cs` (zona `Asia/Jakarta`); kontrak `r49` 44.2/44.4,
`LAB-PERM-v1` 19.3/19.5.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Controllers/LabCancellationReportController.cs` | `GET /export` dan `POST /print-log` — `[AccessAction("Export", …, AccessType = Read)]` + `[AccessPermission("LabCancellationReport", "Export")]`; konstruktor menerima `LabReportXlsxWriter` |
| `Services/LabCancellationReportService.cs` | `GetAllForExportAsync` — `Peristiwa` + `Saring` + urutan + `LengkapiAsync` yang sama dengan `GetListAsync`, tanpa halaman; total dihitung dari baris yang sama |
| `DTOs/LabCancellationReportDtos.cs` | `LabCancellationReportPrintRequest` (mewarisi query; halaman diabaikan), `LabCancellationReportExport` (bahan berkas, bukan kontrak HTTP) |
| `Services/LabReportXlsxWriter.cs` | `Write(LabCancellationReportExport)` — dua belas kolom, waktu WIB bertipe tanggal (`dd/MM/yyyy HH:mm`), label status, *Alasan tidak dicatat* miring, baris total |
| `Services/LabFilterMetadataFactory.cs` | `LabelOrderStatus` menjadi `internal`; `LabelStatusPemeriksaan` baru — dipakai berkas saja; respons API tetap nilai enum mentah |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Aditif** — dua endpoint persis `r49` 44.2/44.4 |
| Database | Nol perubahan. Kueri baca yang sama dengan daftar, tanpa `Skip`/`Take` (dibatasi `VAL-149`) |
| Keamanan/Auth | Aksi baru `LabCancellationReport : Export` (registri: 1757 kemampuan valid). Berkas `.xlsx` memuat identitas pasien — hanya bagi pemegang `Export`; log unduhan dan cetak tanpa pasien dan tanpa alasan |

## 4. Dokumentasi endpoint

#### Health Services / Laboratory Management / Lab Cancellation Report

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/laboratory-management/lab-cancellation-reports/export` | Seluruh baris hasil penyaring sebagai `.xlsx` (`laporan-pembatalan_<awal>_<akhir>.xlsx`) | `LabCancellationReport : Export` |
| `POST` | `/api/v1/health-services/laboratory-management/lab-cancellation-reports/print-log` | Mencatat cetak sebelum dialog cetak dibuka | `LabCancellationReport : Export` |

| Kode | Kapan |
| --- | --- |
| `200` | Berkas terkirim / cetak tercatat |
| `400` | Periode kosong atau terbalik; enum tidak dikenal |
| `403` | Tanpa `Export` |
| `422` | Periode lebih dari 366 hari |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -p:RunAnalyzers=False` (server dimatikan) | 0 error | `PASS` | Keluaran build |
| Harness `be100harness` = 19 uji `BE-LAB-99` (regresi) + 12 uji baru | **31/31** | `PASS` | Keluaran harness |
| ↳ `AC-332`: baris berkas = total daftar dengan penyaring sama, `pageSize` diabaikan; 12 kolom berjudul Indonesia | 6 = 6 | `PASS` | Dibaca ulang dengan `ClosedXML` |
| ↳ `AC-328`: *Alasan tidak dicatat* di berkas; `AC-331`: 31 Agt 17.10 UTC → *01/09/2026 00:10*, bertipe tanggal | Lulus | `PASS` | Harness |
| ↳ Label: *Pemeriksaan*/*Pesanan*, *Dipesan*/*Diminta*, *6 pemeriksaan*, layak tagih *Ya/Tidak*/kosong; baris total | Lulus | `PASS` | Harness — *Dipesan* diganti *Terpesan* 2026-10-21 (`LAB-DEC-250`, bagian 9) |
| ↳ Log unduhan dan cetak: penyaring dan jumlah baris di teks pesan; **tanpa** nama, No. RM, alasan | Lulus | `PASS` | Pencatat penangkap |
| ↳ Print-log 367 hari `422` dan export tanpa periode `400` — nol log baru | Lulus | `PASS` | Harness |
| ↳ Atribut izin keenam endpoint cocok huruf demi huruf | Lulus | `PASS` | Refleksi |
| Regresi `be98harness` 29/29; Beranda `be96harness` identik | Lulus | `PASS` | Dua harness |
| Startup: *"Permission registry valid. 1757 …"* | Lulus | `PASS` | `scratchpad/be.log` |
| HTTP superadmin devYoga: daftar `pageSize=1` → 1 dari 2; `GET /export` → `200`, `.xlsx` sah, **2** baris (log *"2 baris"*); `POST /print-log` isi JSON `cancellationKind:"Order"` → `200`; tanpa periode `400` | Lulus | `PASS` | `scratchpad/be100-http.mjs`; berkas `scratchpad/laporan-pembatalan-http.xlsx` |
| Log aplikasi `Logs/quilvian-backend-20261020.json` | Satu `.Export`, satu `.Print`, pelaku tercatat, tanpa pasien | `PASS` | Berkas log |
| `403` bagi pemegang `Read` tanpa `Export` | Terbukti 2026-10-20 — unduh dan print-log `403`, nol log; sesudah `Export` `200` | `PASS` | Bagian 8 |

Uji manual: `NOT APPLICABLE` — layar adalah `FE-LAB-55`.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-332` — total sama di setiap halaman **dan** sama dengan jumlah baris berkas; satu log unduhan tanpa pasien | **Terpenuhi** | Harness + HTTP + log aplikasi |
| `AC-337` (sisi Pembatalan, backend) — print-log `200` dan satu log; gagal validasi nol log | **Terpenuhi** sisi backend — `403` bagi `Read` saja terbukti (bagian 8); fail-closed layar milik `FE-LAB-55` | Harness + HTTP |
| Dua endpoint berjalan; aksi `Export` terdaftar | **Terpenuhi** | Registri 1757 |
| Berkas dari fungsi yang sama dengan layar | **Terpenuhi** | Satu jalur `Peristiwa`/`Saring`/`LengkapiAsync` |
| Laporan `BE-LAB-100.md` | **Terpenuhi** | Berkas ini |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Server lokal dimatikan untuk build dan dinyalakan ulang; berjalan sebagai proses latar sesi ini |
| Masalah yang diketahui | Unduhan periode panjang memuat seluruh baris ke memori — dibatasi 366 hari; jumlah pembatalan rumah sakit wajar kecil |
| Risiko tersisa | Sedang — berkas `.xlsx` membawa identitas pasien keluar sistem; dijaga izin `Export` yang bawaannya tertutup dan dicatat |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |

**Status Git akhir** (`git status --short`, di luar `docs/`): task ini mengubah lima berkas di atas (seluruhnya sudah
berubah sejak `BE-LAB-98`/`99` dalam working tree yang sama). Nol berkas baru, nol migration. Nol stage, commit, atau push.

**Gelombang `MVP-15` sisi backend selesai pada kode.** Berikutnya: `FE-LAB-54` (layar Statistik Pesanan), langkah rilis
`MVP-15e` (migration index, pemberian izin, data uji), dan pembuktian `403` dengan akun non-superadmin — butir 1, 2 (Kepala
Instalasi), dan 4 (dr. Bima) diselesaikan pada hari yang sama, lihat bagian 8.

## 8. Addendum verifikasi — 2026-10-20 (langkah rilis `MVP-15e` butir 2, 4)

Atas instruksi pemilik modul — *"lakukan migrasi tersebut"* beserta sandi akun dr. Bima untuk bukti validasi — dan
pilihannya *"Bertahap: Read → uji → Export"*. Sandi tidak disimpan di berkas mana pun.

| Langkah | Hasil | Bukti |
| --- | --- | --- |
| Tahap 0 — akun dr. Bima (Kepala Instalasi: memegang `LabOperationalReport : Read`/`Export` dan `LabOrder : Read`, nol izin laporan baru) | Kesepuluh endpoint kedua laporan **`403`**; kontrol `LabOperationalReport` `200`; log hanya `SECURITY_ACCESSDENIED`, nol baris log laporan | `scratchpad/bima-403.mjs` |
| Tahap 1 — jabatan diberi `Read` kedua laporan (set 41 → 43; nol izin hilang; nol izin tersembunyi sebelum kirim ulang) | Metadata, data laporan, pilihan petugas `200`; detail id acak `404`; **unduh dan print-log `403`** pada kedua laporan; satu log baca, nol log unduh/cetak | idem + log aplikasi |
| Tahap 2 — ditambah `Export` (set 43 → 45) | Seluruh endpoint `200`; tepat satu baris log per peristiwa (`.Read`, `.Export`, `.Print`), tanpa data pasien | Log aplikasi |

Jabatan *Kepala Instalasi Laboratorium* kini memegang keempat izin laporan `S16b` — izin bawaan `LAB-DEC-239` untuk kepala
instalasi terpasang di devYoga. Jabatan manajemen **ditunda** atas keputusan pemilik modul 2026-10-21 — master jabatan belum
punya jabatan manajemen pelayanan (hanya *Manajer Finance* dan *Manajer HR*, yang tidak diberi laporan berisi data pasien);
diberikan admin begitu jabatannya dibuat. Akun analis (Gilang) diuji 2026-10-21:
kontrol positif `lab-orders/filters/metadata` `200`, kesepuluh endpoint `S16b` kedua laporan `403`.

---

## 9. Penyeragaman label status — `LAB-DEC-250` (2026-10-21)

Temuan `FE-LAB-55`: kolom *Status Sebelum* di layar memakai label status Laboratorium, berkas `.xlsx` memakai label backend
yang bunyinya sedikit berbeda. Pemilik modul menyetujui penyeragaman dengan arah yang direkomendasikan: **berkas mengikuti layar**.

| Hal | Isi |
| --- | --- |
| Berkas yang berubah | `Areas/HealthServices/LaboratoryManagement/Services/LabFilterMetadataFactory.cs` — `LabelOrderStatus`: *Sedang dikerjakan* → *Sedang Dikerjakan*, *Pembatalan diminta* → *Pembatalan Diminta*; `LabelStatusPemeriksaan`: *Dipesan* → *Terpesan*, *Layak tagih* → *Layak Tagih*, *Gugur* → *Digugurkan* |
| Dampak | Berkas `.xlsx` Laporan Pembatalan dan metadata penyaring status pesanan (`GET /lab-orders/filters/metadata`, dipakai penyaring Daftar Pesanan) kini berbunyi sama dengan lencana status di layar. Nol perubahan kontrak bentuk, nol perubahan kode enum |
| Build | `dotnet build -p:RunAnalyzers=False` lulus (lewat harness yang mereferensikan proyek backend) |
| Harness `BE-LAB-100` | **31/31** `PASS` — ekspektasi *Dipesan* diganti *Terpesan* |
| Bukti runtime | Akun asli dr. Bima mengunduh `.xlsx`: *Layak Tagih*, *Diminta*; nol *Layak tagih*/*Dipesan*. Metadata status pesanan: *Sedang Dikerjakan*, *Pembatalan Diminta* |
