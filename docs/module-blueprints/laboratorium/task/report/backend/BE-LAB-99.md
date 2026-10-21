# Laporan Perubahan Backend — `BE-LAB-99`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-99` |
| Judul | Laporan Pembatalan: daftar, detail, penyaring, index, dan jejak baca |
| Slice | `S16b-1` Laporan Pembatalan Order & Pemeriksaan; gelombang `MVP-15d` |
| Roadmap | `docs/module-blueprints/laboratorium/roadmap/backend-roadmap.md` bagian 6au.4 |
| Trace | `FR-20.1`..`FR-20.4`, `FR-20.7`, `FR-20.11`; `LAB-DEC-232`, `237`..`240`; 32.10 no. 1, 3, 4, 5; `INV-58`; `AC-328`, `AC-329`, `AC-330` (sisi Pembatalan), `AC-331`, `AC-332` (total) |
| Contract version | `LAB-API-v1` `r49` 44.2 (lima endpoint baca), 44.4; `LAB-PERM-v1` revision 17 bagian 19; `LAB-VAL-v1` `r21` 23.2 — **`approved` 2026-10-20** |
| Dependency | `BE-LAB-98` (pencatat bersama, factory) dan `BE-LAB-97` (dianjurkan) — keduanya selesai |
| Klasifikasi | `HEAVY` — skor 11: repository 0, berkas diperiksa 2 (>20), berkas diubah 2 (11), logika bisnis 2 (peristiwa vs status, identitas, paging, total), kontrak API 2, database 2 (index + migration), keamanan/auth 1 (resource izin, data pasien), UI 0 |
| Task mode | `BACKEND` — backend target tulis; berkas migration boleh **disiapkan** (preferensi pemilik 2026-10-05) **tanpa diterapkan** |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/HealthServices/LaboratoryManagement/{Controllers,Services,DTOs,Enums}`, `Repositories/Configurations/HealthServices/LaboratoryManagement/LabTransitionHistoryConfiguration.cs`, `Migrations/` (berkas baru + snapshot), `Program.cs`, laporan ini, baris status roadmap/traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `4d3e4f98` (branch `yoga`) + working tree `BE-LAB-96`..`98` |
| Tanggal | 2026-10-20 |
| Status | ✅ **Selesai** — migration diterapkan ke devYoga dan `403` non-superadmin terbukti, 2026-10-20 (bagian 8). Semula *batas verifikasi* |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `LaboratoryManagement` |
| Pemilik / prefix registry | `Lab` — Laboratory, **`ACTIVE`** |
| Keberlakuan | `NEW CODE` (controller, service, DTO, enum `LabCancellationKind`); `TOUCHED LEGACY` (`LabTransitionHistoryConfiguration`, `LabFilterMetadataFactory`, `LabReportOutputLogger`, `Program.cs`) |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-DTO-001`, `QBE-VAL-001`, `QBE-PAGE-001` (paging pola `LabOrderPagedQuery`), `QBE-LOG-001` (log baca beraktor), `QBE-CFG-001` (index di `IEntityTypeConfiguration`), `QBE-OPT-001`, `QBE-AUD-001` (log aplikasi terpisah dari riwayat basis data), `QBE-MOD-001`, `QBE-NAM-001` |
| QBE yang tidak berlaku | `QBE-ENT-*` (nol entity/kolom baru), `QBE-CODE-*`, `QBE-DB-001/002` (bukan `LEGACY MIGRATION`) |
| Branch | `yoga` |

---

## 1. Masalah yang diperbaiki

Tidak ada cara membaca **siapa membatalkan apa, kapan, dan mengapa** di Laboratorium; artifact *Audit Delete Order &
Parameter* (`LAB-EVD-014` §11) meminta jejak itu, dan `LAB-DEC-232` menetapkannya sebagai **Laporan Pembatalan** dari jejak
yang sudah ada — tanpa fitur hapus.

## 2. Proses bisnis

1. Pemegang `LabCancellationReport : Read` membuka laporan dan memilih periode (wajib, paling panjang 366 hari) serta bila
   perlu disiplin, jenis (*Pesanan*/*Pemeriksaan*), petugas pembatal, dan unit layanan.
2. Backend membaca **peristiwa** `Order.Cancel` dan `Examination.Cancel` pada riwayat perpindahan menurut **waktu
   pembatalan** WIB — satu baris per peristiwa (`INV-58`). Pembatalan wadah **bukan** baris; status pemeriksaan hari ini
   **tidak** dibaca (membatalkan pesanan tidak mengubah status pemeriksaannya — F30-18, terlihat juga di devYoga:
   pesanan `000018` batal dengan satu permintaan masih *belum masuk wadah*).
3. Setiap baris memuat dua belas kolom BR-151, termasuk **No. RM dan nama pasien** (dibaca per halaman lewat kunjungan),
   *Sudah layak tagih*, dan alasan — atau ***Alasan tidak dicatat*** (`isReasonRecorded = false`) bagi pembatalan lama.
4. **Total per jenis** dihitung atas seluruh hasil penyaring dan sama di setiap halaman.
5. Membuka daftar **dicatat** di log — periode, penyaring, halaman, pelaku — **tanpa** identitas pasien maupun alasan
   (32.10 no. 1).
6. Klik baris → **detail** di bawah izin laporan yang sama: pemeriksaan pesanan (termasuk permintaan yang belum masuk
   wadah, `AwaitingSpecimen`), wadahnya, dan riwayat transisi pesanan lengkap.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md`; `docs/engineering/*`; `rules/backend/DATABASE_RULES.md`, `API_RULES.md`, `role-access-rules.md`;
`Models/LabTransitionHistory.cs`; `LabTransitionHistoryConfiguration.cs`; `LabOrderService.cs` (jalur pasien `:502-520`,
paging `:150`, batal `:1678`); `LabExaminationService.cs` (nama pelaku `:1054-1061`); `LabSpecimenService.cs`;
`Models/ApplicationUser.cs`; `Migrations/` (struktur `History/`, `MigrationMetadata.g.cs`, 45 Designer modul lain yang
masih dikompilasi); `tooling/migrations/Update-MigrationHistory.ps1`; kontrak `r49` 44.2/44.4, `LAB-PERM-v1` 19.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Controllers/LabCancellationReportController.cs` | **Baru.** Route `.../lab-cancellation-reports`, resource `LabCancellationReport` (`SortOrder` 26). `GET /filters/metadata`, `GET /cancelled-by-options`, `GET /`, `GET /{historyId}` — keempatnya `Read`. Log baca pada `GET /` |
| `Services/LabCancellationReportService.cs` | **Baru.** `GetListAsync` (peristiwa, penyaring, total per jenis, paging dijepit), `GetDetailAsync` (`404` bila bukan peristiwa batal), `GetCancelledByOptionsAsync`, `GetFilterMetadataAsync`, `DescribeFiltersAsync`. Pasien, kunjungan, unit, nama pelaku, dan jumlah pemeriksaan pesanan dimuat **satu kueri per sumber per halaman** |
| `DTOs/LabCancellationReportDtos.cs` | **Baru.** Query (mewarisi `LabOperationalReportQuery`; konverter enum teks pada `CancellationKind`), respons, total, baris, detail, opsi pembatal, metadata |
| `Enums/LaboratoryEnums.cs` | `LabCancellationKind` (`Order` = 1, `Examination` = 2), tanpa bawaan |
| `Services/LabFilterMetadataFactory.cs` | `LabCancellationReport()` — bagian statis metadata |
| `Services/LabReportOutputLogger.cs` | `LogReadAsync` — aksi `<Resource>.Read` |
| `Repositories/Configurations/HealthServices/LaboratoryManagement/LabTransitionHistoryConfiguration.cs` | `HasIndex(x => new { x.Action, x.OccurredAt })` (32.10 no. 3) |
| `Migrations/20261020084310_AddLabTransitionHistoryActionOccurredAtIndex.cs` + `.Designer.cs` | **Baru** — dibangkitkan `dotnet ef migrations add --no-build` di atas snapshot terkini. `Up`: `CreateIndex IX_LabTransitionHistory_Action_OccurredAt`; `Down`: `DropIndex` |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | +1 baris `b.HasIndex("Action", "OccurredAt")` — **hanya** itu |
| `Program.cs` | `AddScoped<LabCancellationReportService>()` |

**Skrip `tooling/migrations/Update-MigrationHistory.ps1` sengaja tidak dijalankan:** 45 Designer migration modul lain masih
ikut dikompilasi di `Migrations/`; menjalankan skrip itu memindahkan berkas milik tim lain ke `History/` — di luar wewenang
task ini. Designer migration baru mengikuti pola berkas-berkas tersebut.

**Selisih kecil dengan kontrak `r49` 44.4 (aditif, dicatat):** detail memakai nilai `examinationStatus = "AwaitingSpecimen"`
bagi permintaan yang belum masuk wadah — kontrak tidak menyebut kasus ini, dan menghilangkannya membuat detail pesanan batal
(seperti `000018` di devYoga) kosong.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Aditif** — empat endpoint baca baru persis `r49` 44.2. `GET /export` dan `POST /print-log` milik `BE-LAB-100` |
| Database | Nol tabel/kolom. **Satu migration index — disiapkan, BELUM diterapkan** (devYoga diperiksa baca-saja: index tidak ada; migration terakhir DB = migration terakhir repo sebelum ini, sehingga hanya migration ini yang tertunda). Aplikasi tidak menjalankan `Migrate()` saat startup — diperiksa |
| Keamanan/Auth | Resource baru `LabCancellationReport : Read` (registri: 1756 kemampuan valid). Laporan **memuat identitas pasien** — izin bawaan tertutup; log baca/keluaran tanpa pasien dan tanpa alasan |

## 4. Dokumentasi endpoint

#### Health Services / Laboratory Management / Lab Cancellation Report

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/laboratory-management/lab-cancellation-reports/filters/metadata` | Pilihan penyaring: disiplin, jenis, unit aktif, batas periode dan halaman | `LabCancellationReport : Read` |
| `GET` | `/api/v1/health-services/laboratory-management/lab-cancellation-reports/cancelled-by-options` | Petugas pembatal pada periode terpilih | `LabCancellationReport : Read` |
| `GET` | `/api/v1/health-services/laboratory-management/lab-cancellation-reports` | Daftar peristiwa pembatalan, berhalaman, total atas seluruh penyaring; **dicatat** | `LabCancellationReport : Read` |
| `GET` | `/api/v1/health-services/laboratory-management/lab-cancellation-reports/{historyId}` | Detail satu peristiwa | `LabCancellationReport : Read` |

| Kode | Kapan |
| --- | --- |
| `200` | Daftar/detail/opsi terbentuk — termasuk periode tanpa pembatalan |
| `400` | Periode kosong (`VAL-147`), terbalik (`VAL-148`), enum tidak dikenal |
| `403` | Tanpa `LabCancellationReport : Read` |
| `404` | `historyId` tidak ada, atau bukan peristiwa `Order.Cancel`/`Examination.Cancel` — *"Peristiwa pembatalan tidak ditemukan."* |
| `422` | Periode lebih dari 366 hari (`VAL-149`) |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -p:RunAnalyzers=False` (server dimatikan) | 0 error | `PASS` | Keluaran build |
| `dotnet ef migrations add AddLabTransitionHistoryActionOccurredAtIndex --no-build` | Migration berisi **satu** `CreateIndex`/`DropIndex`; snapshot +1 baris | `PASS` | Isi berkas dan `git diff` |
| Harness InMemory `be99harness` | **19/19** | `PASS` | Keluaran harness |
| ↳ `AC-329`: pesanan 5 pemeriksaan (+1 permintaan belum wadah) dibatalkan lewat `LabOrderService` + satu pemeriksaan lewat `LabExaminationService` → **2** baris; wadah bukan baris; detail memuat kelima pemeriksaan, permintaan, wadah `Cancelled`, riwayat | Lulus | `PASS` | Harness |
| ↳ Kolom: jumlah pemeriksaan 6, alasan, pembatal, unit, pasien, disiplin dari katalog, *Sudah layak tagih* `true` | Lulus | `PASS` | Harness |
| ↳ `AC-331`: 1 Okt 06.30 WIB → Oktober; 1 Sep 00.10 WIB → September | Lulus | `PASS` | Harness |
| ↳ `AC-328`: alasan kosong/spasi → `isReasonRecorded = false`, tetap terhitung; `Specimen.Cancel` bukan baris | Lulus | `PASS` | Harness |
| ↳ `AC-332` (total): total 6 sama di halaman 1–3 (`pageSize` 2); urutan terbaru di atas; `pageSize` 500 → 100, 0 → 1 | Lulus | `PASS` | Harness |
| ↳ Penyaring jenis, pembatal, unit, disiplin; opsi pembatal bernama; detail `404`; `VAL-147`/`148` `400`, `VAL-149` `422` | Lulus | `PASS` | Harness |
| ↳ Log baca di teks pesan — **tanpa** nama, No. RM, alasan pasien (string rahasia dicari); periode kosong → nol log | Lulus | `PASS` | Pencatat penangkap |
| ↳ Atribut izin keempat endpoint cocok huruf demi huruf; JSON enum nama diterima | Lulus | `PASS` | Refleksi |
| Regresi `be98harness` (29/29) dan Beranda `be96harness` (identik) | Lulus | `PASS` | Dua harness |
| Startup Development: *"Permission registry valid. 1756 …"*; nol `Migrate()` otomatis | Lulus | `PASS` | Log startup; pencarian source |
| devYoga baca-saja: index `IX_LabTransitionHistory_Action_OccurredAt` **tidak ada**; migration terakhir DB `20261017085614_…` | Belum diterapkan, sesuai wewenang | `PASS` | Konsol Npgsql `default_transaction_read_only=on` |
| HTTP superadmin devYoga: metadata `200`; daftar 2026 `200` — Order `000018` (n=1, alasan tercatat) dan Pemeriksaan Trombosit `000001` (*ChargeEligible*, layak tagih `true`); No. RM, nama, unit, pembatal terisi | Lulus | `PASS` | `scratchpad/be99-http.mjs` |
| HTTP: opsi pembatal 2; detail kedua baris `200` (pesanan batal: `AwaitingSpecimen`); id tak dikenal `404`; periode kosong `400`; 367 hari `422`; `cancellationKind=Bogus` `400`; `kind=Order` 1 baris; `pageSize=500` → 100 | Lulus | `PASS` | Skrip yang sama |
| Log aplikasi `Logs/quilvian-backend-20261020.json` | Baris `LabCancellationReport.Read` dengan periode dan penyaring, pelaku tercatat, tanpa pasien | `PASS` | Berkas log |
| `403` bagi akun non-superadmin | Terbukti 2026-10-20 dengan akun dr. Bima — keempat endpoint `403` tanpa izin, `200` sesudah `Read` | `PASS` | Bagian 8 |
| Menerapkan migration ke devYoga | Diterapkan 2026-10-20 atas instruksi; riwayat 342, index `valid` | `PASS` | Bagian 8 |

Uji manual: `NOT APPLICABLE` — layar adalah `FE-LAB-55`/`56`.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-328`, `AC-329`, `AC-331`, `AC-332` (total) | **Terpenuhi** | Harness + HTTP |
| `T-S16B-15` — log `.Read` per permintaan tanpa identitas pasien | **Terpenuhi** | Harness + log aplikasi |
| `404` bagi `historyId` yang bukan peristiwa batal | **Terpenuhi** | Harness + HTTP |
| `AC-330` (sisi Pembatalan) — `403` bagi pemegang hak baca daftar saja | **Terpenuhi** — pemegang `LabOrder : Read` dan izin laporan lain → `403` (bagian 8) | HTTP |
| Index naik-turun | **Disiapkan** — `Up`/`Down` sesuai; penerapan nyata menunggu instruksi (`MVP-15e`) | Isi migration |
| Nol baris berubah oleh migration | **Terpenuhi pada rancangan** — hanya `CreateIndex`/`DropIndex` | Isi migration |
| Laporan `BE-LAB-99.md` | **Terpenuhi** | Berkas ini |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Server lokal dimatikan untuk build dan dinyalakan ulang (pengaturan sama). Migration baru **tertunda** di devYoga sampai pemilik menjalankannya — aplikasi tetap berjalan tanpa index (kueri memindai tabel kecil) |
| Masalah yang diketahui | `AC-330` runtime menunggu akun non-superadmin. Nama pembatal memakai `DisplayName` — sama dengan jalur `r48`; akun dengan `DisplayName` kosong tampil kosong |
| Risiko tersisa | Sedang — satu-satunya layar Lab yang membuka identitas banyak pasien; dijaga izin tertutup dan log baca. Kinerja pada riwayat besar bergantung index — diterapkan 2026-10-20 |
| Perubahan sampingan | `NONE`. Skrip arsip migration tidak dijalankan (alasan di 3.2) |
| Interupsi | `NONE` |

**Cara menerapkan migration (atas keputusan pemilik):** cek dulu `dotnet ef migrations list` — hanya
`20261020084310_AddLabTransitionHistoryActionOccurredAtIndex` yang tertunda per pemeriksaan 2026-10-20 — lalu terapkan
lewat skrip per migration seperti biasa. Membuat index mengunci tulis `LabTransitionHistory` sesaat.

**Status Git akhir** (`git status --short`, di luar `docs/`): task ini menambah `LabCancellationReportController.cs`,
`LabCancellationReportService.cs`, `LabCancellationReportDtos.cs`, dua berkas migration, dan mengubah `LaboratoryEnums.cs`,
`LabFilterMetadataFactory.cs`, `LabReportOutputLogger.cs`, `LabTransitionHistoryConfiguration.cs`,
`ApplicationDbContextModelSnapshot.cs`, `Program.cs`. Berkas lain milik `BE-LAB-96`..`98` atau pekerjaan Radiologi. Nol
stage, commit, atau push.

**Task berikutnya:** `BE-LAB-100` — unduhan `.xlsx` dan pencatat cetak Laporan Pembatalan.

## 8. Addendum verifikasi — 2026-10-20 (langkah rilis `MVP-15e` butir 1, 2, 4)

Atas instruksi pemilik modul — *"lakukan migrasi tersebut"* beserta sandi akun dr. Bima untuk bukti validasi — dan
pilihannya *"Bertahap: Read → uji → Export"*. Sandi tidak disimpan di berkas mana pun.

| Langkah | Hasil | Bukti |
| --- | --- | --- |
| Migration `20261020084310_AddLabTransitionHistoryActionOccurredAtIndex` ke devYoga | `migrations list`: hanya migration ini yang tertunda. Uji kering naik → turun dalam satu transaksi lalu `ROLLBACK` (riwayat 341 → 342 → 341; `LabTransitionHistory` 118 baris, sidik md5 identik), kemudian `COMMIT`: riwayat **342, nol tertunda**, index `valid` | `scratchpad/mig/migrate.js` |
| Tahap 0 — akun dr. Bima (Kepala Instalasi: memegang `LabOperationalReport : Read`/`Export` dan `LabOrder : Read`, nol izin laporan baru) | Kesepuluh endpoint kedua laporan **`403`**; kontrol `LabOperationalReport` `200`; log hanya `SECURITY_ACCESSDENIED`, nol baris log laporan | `scratchpad/bima-403.mjs` |
| Tahap 1 — jabatan diberi `Read` kedua laporan (set 41 → 43; nol izin hilang; nol izin tersembunyi sebelum kirim ulang) | Metadata, data laporan, pilihan petugas `200`; detail id acak `404`; **unduh dan print-log `403`** pada kedua laporan; satu log baca, nol log unduh/cetak | idem + log aplikasi |
| Tahap 2 — ditambah `Export` (set 43 → 45) | Seluruh endpoint `200`; tepat satu baris log per peristiwa (`.Read`, `.Export`, `.Print`), tanpa data pasien | Log aplikasi |

Jabatan *Kepala Instalasi Laboratorium* kini memegang keempat izin laporan `S16b` — izin bawaan `LAB-DEC-239` untuk kepala
instalasi terpasang di devYoga. Jabatan manajemen **ditunda** atas keputusan pemilik modul 2026-10-21 — master jabatan belum
punya jabatan manajemen pelayanan (hanya *Manajer Finance* dan *Manajer HR*, yang tidak diberi laporan berisi data pasien);
diberikan admin begitu jabatannya dibuat. Akun analis (Gilang) diuji 2026-10-21:
kontrol positif `lab-orders/filters/metadata` `200`, kesepuluh endpoint `S16b` kedua laporan `403`.
