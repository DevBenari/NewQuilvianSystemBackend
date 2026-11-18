# Laporan Perubahan Backend — `BE-AST-010`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-AST-010` |
| Judul | Laporan dan penetapan kondisi |
| Slice | `REG-S2` — Register aset: pencatatan, label, penempatan, kondisi, dokumen, pemanfaatan, impor, dashboard |
| Roadmap | `register/roadmap/backend-roadmap.md` — kartu `BE-AST-010` |
| Trace | `CAP-06.*`; `AST-DEC-017`, `AST-DEC-046`, `AST-RULE-010..012`, `AST-DES-020`; API 2.11; `state-transition-matrix` §2–3; `validation-matrix` (`AST-VAL-REG-030..035`, `070..072`); `permission-audit-matrix` (`AssetCondition`); `integration-contract` `INT-REG-22`; `AT-REG-06`, `07`, `39`, `40` |
| Contract version | `AST-REG-1.0.0` `approved` 2026-11-12 |
| Dependency | `BE-AST-007` ✅; memakai `BE-AST-005` (lampiran, penomoran, linimasa, aktor) dan `BE-AST-009` (pola transaksi, PDF) |
| Klasifikasi | `HEAVY` — 1 aggregate ber-lifecycle (3 transisi), 1 controller (9 endpoint, 2 multipart), PDF berita acara, titik panggil lintas sub-modul; tanpa entity atau migration baru |
| Task mode | `BACKEND`, diminta Rizki 2026-11-18 ("lanjutkan") |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/AssetManagement/**`; laporan ini dan baris status roadmap/traceability |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `f993e3ec` (branch `rizkiG`) |
| Tanggal | 18 November 2026 |
| Status | ✅ SELESAI 18 November 2026 — kelima kriteria terpetakan ke source; QBE Strict `PASS`; build Rizki berhasil; uji runtime 34/34 + 2 pemeriksaan tambahan |

**Backend Governance Preflight**

| Butir | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `AssetManagement` / `Register` |
| Pemilik / prefix registry | `AssetManagement / Register Aset`, prefix `Ast`, `ACTIVE` |
| Keberlakuan | `NEW CODE` |
| QBE yang berlaku | `QBE-MOD-001`, `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-CODE-001`/`002`/`003` (nomor `AST-KND` lewat deret bersama), `QBE-VAL-001`, `QBE-TXN-001`, `QBE-DTO-001`, `QBE-DEL-001` (tolak, bukan hapus) |
| Tidak berlaku | `QBE-ENT-*`, `QBE-CFG-*`, `QBE-NAM-*`, `QBE-DB-*` — model `AstConditionReport` dan `AstAttachment` beserta indeks unik "satu laporan terbuka" sudah ada sejak `BE-AST-003`/`004`; tidak ada migration |
| Arketipe transaksi | Aggregate ber-lifecycle (`Dilaporkan` → `Ditetapkan`/`Ditolak`) |

---

## 1. Masalah yang diperbaiki

Kondisi aset (`Aktif`, `Rusak Ringan`, `Rusak Berat`, `Tidak Digunakan`, `Hilang`) belum dapat diubah sama sekali
sesudah aset terdaftar. Akibatnya:

- Unit tidak dapat melaporkan kerusakan, dan Bagian Aset tidak punya antrean penetapan.
- Aset hilang tidak dapat dicatat dengan berita acara, sehingga penghapusan beralasan Hilang (`AST-DEC-045`) dan
  larangan mutasi/pinjam aset Hilang/Rusak Berat (`AST-RULE-012`) tidak punya data sumber.
- Sub-modul pergerakan (pengembalian pinjaman) dan inventarisasi (tindak lanjut opname) belum punya titik panggil
  untuk membuka laporan kondisi.

---

## 2. Proses bisnis

**Pelaku:** penanggung jawab aset, Kepala Unit, Petugas Aset (melapor); Petugas Aset (menetapkan/menolak);
Kepala Bagian Aset (menyetujui Hilang dan ditemukan kembali).

| Langkah | Pelaku | Yang terjadi |
|---|---|---|
| 1 | Penanggung jawab / Kepala Unit | Melaporkan kondisi baru **beserta foto** (sumber Laporan Unit). Nomor `AST-KND-######` terbit; status **Dilaporkan**. Kondisi aset **belum** berubah |
| 1b | Petugas Aset | Mencatat hasil pemeriksaannya sendiri (sumber Pemeriksaan Bagian Aset); foto tidak wajib |
| 2a | Petugas Aset | **Tetapkan** → kondisi aset berubah ke kondisi yang ditetapkan (boleh berbeda dari yang dilaporkan sesudah diperiksa); linimasa `Kondisi Berubah` bernomor laporan |
| 2b | Petugas Aset | **Tolak** beralasan → kondisi aset tidak berubah |
| 3 | Kepala Bagian Aset | Untuk laporan **Hilang**: **Setujui Hilang** dengan berita acara kehilangan (dilampirkan pelapor atau penyetuju); penyetuju bukan pelapor. Kondisi aset menjadi Hilang |
| 4 | Siapa pun berhak baca | **Berita Acara Kehilangan** (PDF) untuk laporan Hilang yang sudah disetujui, dengan nama, jabatan, dan waktu pelapor dan penyetuju |
| 5 | Unit | Aset Hilang ternyata ada → laporan baru bertanda **ditemukan kembali** dengan kondisi selain Hilang → disetujui Kepala Bagian Aset → kondisi aset kembali |
| — | Sistem | Pengembalian pinjaman (`pergerakan`) dan tindak lanjut opname (`inventarisasi`) membuka laporan lewat `OpenReportAsync`; laporan itu tetap ditetapkan manusia |

Contoh dari uji: Dimas Kurniawan (penanggung jawab) melaporkan kursi `AST-MBL-000033` Hilang beserta foto, tanpa
berita acara (`AST-KND-000004`). Superadmin mencoba menyetujui tanpa berita acara → 400 `032`; mencoba lewat
"tetapkan" biasa → 409; lalu menyetujui dengan berkas `ba-kehilangan-unit.pdf` → kursi Hilang, linimasa
"Aktif → Hilang". Esoknya kursi ditemukan: laporan `AST-KND-000005` "ditemukan kembali" disetujui → Aktif.

**Jalur tidak normal**

| Kondisi | Hasil | Kode |
|---|---|---|
| Aset atau kondisi tidak diisi; catatan > 1000 karakter; alasan > 500 karakter | 400 | `AST-REG-FIELD` / `010` |
| Sumber Pengembalian Pinjaman / Tindak Lanjut Opname lewat API | 400 — dibuat otomatis oleh prosesnya | `AST-REG-FIELD` |
| Sumber Pemeriksaan Bagian Aset oleh pengguna tanpa hak `AssetCondition : Decide` | 403 | `AST-CND-SOURCE` |
| Laporan unit tanpa foto | 400 | `AST-VAL-REG-030` |
| Berkas bukan PDF/JPG/PNG/XLSX; terlalu besar; pengaturan lampiran kosong | 400 / 413 / 503 | `070` / `071` / `072` |
| Aset tidak ada / milik badan hukum lain | 404 | `AST-REG-NOT-FOUND` |
| Aset belum `Terdaftar` | 409 | `AST-REG-STATUS` |
| Aset di luar unit pelapor dan bukan tanggungannya | 403 | `AST-VAL-REG-016` |
| Aset Hilang dilaporkan biasa | 400 | `AST-VAL-REG-034` |
| "Ditemukan kembali" untuk aset tidak hilang, atau dengan kondisi Hilang | 400 | `AST-REG-FIELD` |
| Masih ada laporan terbuka untuk aset itu | 409 | `AST-VAL-REG-035` |
| Tetapkan/tolak/setujui laporan yang sudah diputuskan | 409 | `AST-VAL-REG-031` |
| `rowVersion` basi | 409 | `AST-VAL-REG-015` |
| Tetapkan kondisi Hilang lewat `decide`; laporan Hilang/ditemukan kembali lewat `decide`; laporan biasa lewat `approve-loss` | 400 / 409 / 409 | `AST-REG-FIELD` / `AST-CND-ROUTE` |
| Setujui Hilang tanpa berita acara | 400 | `AST-VAL-REG-032` |
| Menyetujui laporan Hilang yang dibuat sendiri | 403 | `AST-VAL-REG-033` |
| Berita acara kehilangan sebelum Hilang disetujui | 409 | `AST-CND-STATUS` |

Berkas yang ditolak tidak menghabiskan nomor `AST-KND`: lampiran disimpan lebih dulu, nomor dialokasikan sesudahnya.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md`; `rules/backend/*`, `transaction-endpoint-standard.md`, `role-access-rules.md`; kontrak 2.11,
`state-transition-matrix` §2, §3, §7, `validation-matrix`, `permission-audit-matrix`, `integration-contract`
(`INT-REG-22`, `INT-MOV-03`, `INT-INV-07`), `acceptance-test-matrix`, `00-interview-decisions` (`AST-DEC-017`,
`045`, `046`, `AST-RULE-010..012`), `02-backend-architecture` (`AST-DES-020`), `data-dictionary` §3.4; model dan
configuration `AstConditionReport` (indeks unik `IX_AstConditionReport_Open`), `AstAttachment`;
`AssetAttachmentService`/`Options`, `AssetNumberingService` (`AST_CONDITION`), `AssetHistoryService`,
`AssetActorResolver`, `AccessPermissionService.HasAccessAsync`, `AssetPlacementService` (pola transaksi,
`FOR UPDATE`), `AssetLabelDocumentService.Handover.cs`; pola multipart `FinanceTransactionProofsController`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Register/DTOs/AssetConditionDtos.cs` | Baru — request lapor (multipart), tetapkan, setujui hilang (multipart), query, `AssetConditionOpenRequest`, response, ringkasan |
| `Register/Services/AssetConditionService.cs` | Baru — lapor, tetapkan/tolak, setujui hilang, `OpenReportAsync`, lampiran, berita acara; kunci `FOR UPDATE` aset dan laporan |
| `Register/Services/AssetConditionService.Query.cs` | Baru — metadata, ringkasan, daftar, rincian dengan cakupan unit, nama/jabatan pelaku, `availableActions` |
| `Register/Services/AssetLabelDocumentService.Loss.cs` | Baru — PDF Berita Acara Kehilangan (QuestPDF) |
| `Register/Services/AssetLabelDocumentService.Handover.cs` | Diubah — kotak tanda tangan dipisah menjadi `ComposeSignature` agar dipakai kedua berita acara; isi PDF penyerahan tidak berubah |
| `Register/Controllers/AssetConditionReportsController.cs` | Baru — 9 endpoint |
| `Register/Services/AssetManagementException.cs` | Diubah — kode `030..035`, `AST-CND-*` dan pesannya (pesan `030..035` persis `validation-matrix`) |
| `Register/Services/AssetRecordService.cs` | Diubah — label `ConditionReportStatus`, `ConditionReportSource` |
| `Register/Services/AssetPlacementService.Query.cs` | Diubah — `EnumOptions` dari `private` menjadi `internal` untuk dipakai ulang |
| `AssetManagementServiceCollectionExtensions.cs` | Diubah — registrasi `AssetConditionService` |

Nol baris komentar.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | 6 endpoint 2.11 tersedia. Delta: (1) `GET /filters/metadata` dan `GET /summary` sesuai standar aggregate ber-lifecycle; (2) `GET /{id}/attachments/{attachmentId}` untuk menampilkan foto dan berita acara unit di layar `FE-AST-007`; (3) `approve-loss` menerima multipart (`AssetConditionApproveLossRequest`, berkas `lossReport` opsional) supaya Kepala Bagian Aset dapat melampirkan berita acara saat menyetujui; kontrak menyebut `AssetConditionDecideRequest`; (4) laporan **ditemukan kembali** diputuskan lewat `approve-loss`, bukan `decide`, karena matriks §2 menetapkan Kepala Bagian Aset sebagai pemutusnya dan hak yang dipegang peran itu adalah `ApproveLoss`; (5) sumber **Pemeriksaan Bagian Aset** (tanpa foto wajib) mensyaratkan hak `Decide`, agar pelapor unit tidak dapat melewati kewajiban foto; (6) `decide` memakai `accept` (true = tetapkan, false = tolak) dan `decidedCondition` opsional; (7) `/status-history` dan `PUT /{id}` tidak dibuat — laporan tidak dapat disunting dan hanya punya satu transisi, yang tercatat di kolom keputusan dan linimasa aset; (8) kode galat tambahan `AST-CND-NOT-FOUND`, `AST-CND-ROUTE`, `AST-CND-SOURCE`, `AST-CND-STATUS`; (9) body JSON memakai **angka** untuk enum (`decidedCondition: 5`), sedangkan form multipart menerima angka maupun nama |
| Database | `NOT APPLICABLE` untuk schema. Data baru ditulis ke tabel yang sudah ada. Indeks unik `IX_AstConditionReport_Open` menjadi lapis kedua aturan satu laporan terbuka: pelanggaran unik diterjemahkan ke `409 035` |
| Keamanan/Auth | Resource `AssetCondition` (`Read`, `Report` → `Create`, `Decide` → `Update`, `ApproveLoss` → `Update`), keempatnya muncul di Akses Role. Cakupan data: tanpa `ReadAllUnits`, pengguna hanya melihat/melapor aset di unitnya, aset tanggungannya, atau laporan yang ia buat. Pemeriksaan sumber dan `availableActions` memakai `AccessPermissionService`, bukan nama peran |
| Konfigurasi | Lampiran tetap *fail-closed* (`AST-DES-020`). Untuk uji, `AssetManagement:Attachment` diisi di user-secrets PC kantor (`.pdf .jpg .jpeg .png .xlsx`, 5 MB); `appsettings*.json` tidak diubah |

---

## 4. Dokumentasi endpoint

#### Corporate / Asset Management / Register / Asset Condition Report

Base URL: `api/v1/corporate/asset-management/register/condition-reports`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Saringan status, kondisi, sumber; parameter kueri | `AssetCondition : Read` |
| `GET` | `/summary` | Jumlah per status, menunggu penetapan, menunggu persetujuan kehilangan | `AssetCondition : Read` |
| `GET` | `/` | Daftar laporan (`status`, `reportedCondition`, `source`, `isFoundAgain`, `awaitingLossApproval`, `assetId`, `organizationUnitId`, `from`, `to`, `search`) | `AssetCondition : Read` |
| `GET` | `/{id}` | Rincian beserta aset, foto, berita acara, pelaku, `availableActions` | `AssetCondition : Read` |
| `POST` | `/` | Melaporkan kondisi (multipart: `assetId`, `reportedCondition`, `source`, `isFoundAgain`, `notes`, `photo`, `lossReport`) | `AssetCondition : Report` |
| `POST` | `/{id}/decide` | Menetapkan kondisi selain Hilang (`accept: true`, `decidedCondition`), atau menolak (`accept: false`, `reason`) | `AssetCondition : Decide` |
| `POST` | `/{id}/approve-loss` | Menyetujui Hilang (berita acara wajib) atau ditemukan kembali (multipart: `reason`, `decidedCondition`, `rowVersion`, `lossReport`) | `AssetCondition : ApproveLoss` |
| `GET` | `/{id}/attachments/{attachmentId}` | Mengunduh foto/berita acara unit milik laporan itu | `AssetCondition : Read` |
| `GET` | `/{id}/loss-document` | PDF Berita Acara Kehilangan | `AssetCondition : Read` |

`availableActions`: `decide`, `reject`, `approveLoss`, `lossDocument`. `approveLoss` tidak muncul untuk pelapor
laporan Hilang itu sendiri.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| QBE Strict `WorkingTree` | 10 berkas, `VIOLATION: 0`, `REVIEW: 0` (dua kali: sebelum dan sesudah pemindahan alokasi nomor) | `PASS` | Keluaran perintah |
| Grep komentar | 0 baris | `PASS` | — |
| `dotnet build` | Dijalankan Rizki dan berhasil; DLL 2026-11-18 08:48:58, sesudah suntingan terakhir 08:41:03; DLL memuat penanda baru | `PASS` | `bin/Debug/net9.0` |
| Uji runtime (`ast010-runtime.mjs`) | **34/34** | `PASS` | `test-with-agy/asset-management/be-ast-010-20261118/ast010-runtime.json` |
| Pemeriksaan tambahan `decide` + Hilang dengan angka | 400 `AST-REG-FIELD` "Kondisi hilang hanya dapat ditetapkan lewat persetujuan kehilangan." (dengan teks `"Hilang"` di JSON: 400 model binding) | `PASS` | `cek-tambahan.mjs` |
| Pemeriksaan sumber Pemeriksaan Bagian Aset | Akun penyetuju diberi `Read`+`Report` sementara (tanpa `Decide`) → 403 `AST-CND-SOURCE`; daftar 200; policy dipulihkan dan identik (6 hak) | `PASS` | `cek-sumber.json` |
| Berita acara kehilangan dibuka di Edge | Kop, nomor, data aset, kondisi sebelumnya, catatan, berkas unit, blok pelapor/penyetuju dengan nama, jabatan, waktu WIB | `PASS` | `berita-acara-kehilangan-pratinjau.png`, `berita-acara-kehilangan-AST-KND-000004.pdf` |
| Registrasi hak | `GET /role-access/resources`: `AssetCondition` dengan `Read`, `Report`, `Decide`, `ApproveLoss` | `PASS` | Keluaran perintah |

Aktor uji: Dimas Kurniawan / akun PERAWAT (penanggung jawab aset), superadmin uji (petugas dan Kepala Bagian Aset,
**bukan** pelapor kecuali pada uji `033`), akun penyetuju aset, LOKET.

| Kriteria | Langkah uji | Hasil |
|---|---|---|
| AC1 matriks §2–3 | Lapor → tetapkan; lapor → tolak; aksi ulang; jalur Hilang dan ditemukan kembali | Dilaporkan → Ditetapkan / Ditolak; aksi ulang 409 `031`; Hilang lewat `decide` 409 `AST-CND-ROUTE`; ditemukan kembali → Aktif |
| AC2 foto wajib dari unit | Laporan unit tanpa foto; dengan berkas `.txt`; dengan PNG; pemeriksaan Bagian Aset tanpa foto | 400 `030`; 400 `070`; 200 dan foto dapat diunduh (`image/png`, ukuran sama); 200 untuk pemegang `Decide`, 403 `AST-CND-SOURCE` tanpa `Decide` |
| AC3 Hilang butuh berita acara, penyetuju bukan pelapor | `AT-REG-39`: setujui tanpa berita acara; superadmin menyetujui laporan Hilang buatannya; `AT-REG-07`: setujui dengan berita acara | 400 `032`; 403 `033`; 200, kondisi Hilang, PDF 43.909 byte |
| AC4 satu laporan terbuka per aset | `AT-REG-40`: laporan kedua untuk `AST-MBL-000032` saat laporan pertama terbuka | 409 `035` "Aset ini masih punya laporan kondisi yang belum diputuskan." |
| AC5 linimasa `KondisiBerubah` | `AT-REG-06`: lapor Rusak Ringan → kondisi tetap Aktif → tetapkan | Kondisi Rusak Ringan; linimasa terbaru "Aktif → Rusak Ringan (AST-KND-000001)"; untuk Hilang "Aktif → Hilang" |

Lainnya: versi basi 409 `015`; tolak tanpa alasan 400 `010`; aset Hilang dilaporkan biasa 400 `034`; ditemukan
kembali untuk aset tidak hilang 400; lampiran dengan id asing 404; berita acara sebelum Hilang 409
`AST-CND-STATUS`; daftar, saringan, ringkasan 200; Dimas melihat laporan aset tanggungannya; LOKET dan penyetuju
tanpa hak 403.

`OpenReportAsync` belum teruji runtime karena pemanggilnya (`BE-AST-018`, `inventarisasi`) belum ada; jalurnya memakai
pemeriksaan yang sama dengan laporan lewat API (`EnsureReportable`, laporan terbuka, indeks unik).

`AUTOMATED TEST: SKIPPED (opsional) — kebijakan tanpa unit test`.

**Data yang ditulis ke `QuilvianNewDevRizki`** (lewat API, melanjutkan izin uji modul Aset): laporan `AST-KND-000001`
sampai `000006` (3 ditetapkan, 3 ditolak); kursi `AST-MBL-000031` kini **Rusak Ringan**; `AST-MBL-000033` sempat
Hilang lalu kembali **Aktif**; `000032`, `000034`, `000035` tidak berubah. Enam lampiran uji (4 foto, 2 berita acara) di
`Storage/uploads/asset-management/attachments/2026/11/`. Policy Logistik × Staff Logistik diubah sementara lalu dipulihkan.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Matriks state §2–3 | Terpenuhi | `ReportAsync`, `DecideAsync`, `ApproveLossAsync`, `EnsureOpen`, `RequiresLossApproval`; uji AC1 |
| 2. Foto wajib dari unit | Terpenuhi | `ReportAsync` (`030` untuk Laporan Unit; sumber Pemeriksaan Bagian Aset hanya untuk pemegang `Decide`); uji AC2 |
| 3. Hilang butuh berita acara dan Kepala Bagian Aset bukan pelapor | Terpenuhi | `ApproveLossAsync` (`032`, `033`); PDF `RenderLossReport`; uji AC3 |
| 4. Satu laporan terbuka per aset | Terpenuhi | `EnsureNoOpenReportAsync` + `SaveReportAsync` (indeks unik → `035`); uji AC4 |
| 5. Linimasa `KondisiBerubah` | Terpenuhi | `ApplyDecision` → `AssetHistoryService.Append`; uji AC5 |
| Cakupan: `OpenReport` untuk sub-modul lain | Tersedia, belum teruji runtime | `OpenReportAsync`; pemanggil menyusul `BE-AST-018` dan `inventarisasi` |
| `DoD-BE` | Terpenuhi | QBE, build, runtime, laporan, roadmap, traceability |

Rekap: **5 dari 5 kriteria terpenuhi** → `✅`.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Masalah yang diketahui | (1) Superadmin uji tidak punya jabatan utama, jadi tercetak "Jabatan tidak tercatat" di berita acara. (2) Bila transaksi lapor gagal sesudah berkas tersimpan (misalnya bentrok laporan terbuka pada detik yang sama), baris lampiran ikut dibatalkan tetapi berkasnya tertinggal di disk. (3) Penyerahan yang sudah diajukan untuk aset yang kemudian disetujui Hilang tetap dapat dikonfirmasi; pemeriksaan Hilang di `BE-AST-009` hanya terjadi saat penyerahan dibuat. (4) Di DB dev, akun Perawat memegang `Decide`, sehingga ia juga melihat aksi tetapkan/tolak |
| Risiko tersisa | Penerapan di lingkungan lain butuh nilai `AssetManagement:Attachment`; tanpa itu laporan berfoto ditolak 503 `072` |
| Perubahan sampingan | `ComposeSignature` dipisah dari `ComposeApproval`; PDF penyerahan tidak berubah isi. `EnumOptions` menjadi `internal` |
| Status Git | 5 berkas diubah, 5 berkas baru, + dokumen; belum di-commit. Working tree juga memuat laporan dan baris roadmap `FE-AST-006` yang belum di-commit |
| Langkah berikutnya | `FE-AST-007` (kondisi dan status), berpasangan dengan task ini |
