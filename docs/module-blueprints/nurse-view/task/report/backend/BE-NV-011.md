# Laporan Perubahan Backend — `BE-NV-011`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-NV-011` |
| Judul | Panggilan dari tablet: tekan, Darurat, batal, perawat hadir |
| Slice | `MVP-7` — `EPIC NV-06` (gelombang backend 4, revision 2) |
| Roadmap | `docs/module-blueprints/nurse-view/roadmap/backend-roadmap.md` — bagian Revision 2, kartu `BE-NV-011` |
| Trace | `FR-NV-037`, `038`, `040`, `042`, `044` (kejadian dari tablet), `045` (sisi server); `NV-DEC-037`, `039`, `041`, `046`, `050`, `054`; `G-12`, `G-13`; `VAL-NV-28`–`32`, `37`, `38`; state §5; `contracts/api-contract.md` R2 §6 `POST`; `02-backend-architecture.md` R2.1, R2.4.2, R2.4.3, R2.11; `NFR-009`, `010`, `011`, `014` |
| Contract version | `0.2.0` — `approved` (Rizki Gunawan, 2026-11-05, `NV-DEC-055`) |
| Dependency | `BE-NV-010` ✅ (8 Nov 2026) |
| Klasifikasi | `MEDIUM` — skor 8: satu repository (0), berkas diperiksa > 20 (2), berkas diubah 6 (1), logika kompleks — balapan index unik, idempotensi, token versi (2), endpoint baru sesuai kontrak (1), persistence yang sudah ada (1), keamanan berkaitan tetapi bukan inti — memakai policy `BE-NV-010` (1), UI tidak ada (0) |
| Task mode | `BACKEND` — perintah Rizki 8 Nov 2026 ("mulai sekarang", memilih `BE-NV-011`), target tulis `NewQuilvianSystemBackend` branch `rizkiG` |
| Target tulis | `Areas/HealthServices/NurseViewManagement/**`, `Program.cs` (satu `AddScoped`, restu `NV-DEC-056`), laporan ini, baris status roadmap dan traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `589fb513` (branch `rizkiG`); perubahan belum di-commit (sudah di-stage Rizki) |
| Build saat diuji | DLL `bin/Debug/net9.0/QuilvianSystemBackend.dll` 8 Nov 2026 11.10, lebih baru dari source terakhir (09.51); memuat `NvwPatientCallService`, `NvwTabletCallVersionRequest`, `ConfirmNurseArrivedAsync`, nama index idempotensi, dan rute `calls/current/nurse-arrived` |
| Tanggal | 2026-11-08 (implementasi); 2026-11-09 (uji HTTP) |
| Status | ✅ **SELESAI 9 November 2026** — AC 8/8 terpetakan ke source. QBE Strict `PASS` (6 berkas). Build `PASS` menurut pernyataan Rizki. Uji HTTP **21/21 `PASS`** (no. 1–17 beserta sub-cek). Log bersih dari kata sandi, token, dan identitas pasien. Bukti runtime bagian kedua AC-4 dan AC-6 **dikecualikan atas keputusan Rizki 9 November 2026** (bukti source saja), karena tidak dapat dibuat lewat API Rawat Inap tanpa merusak data dev (§5, temuan Rawat Inap). Riwayat: 🟡 SEBAGIAN 8 Nov 2026 sebelum build dan uji |

---

## 1. Masalah yang diperbaiki

Sesudah `BE-NV-010`, tablet di samping bed sudah bisa login dan membaca keadaan bed-nya, tetapi
belum bisa memanggil perawat. Tombol "Panggil perawat", "Darurat", "Batal", dan "Perawat hadir"
belum punya endpoint.

**Contoh.** Pasien 305-A merasa sesak pukul 10.00. Tanpa task ini, tablet tidak bisa mengirim
apa pun ke perawat, dan pasien tetap harus menekan bel di dinding.

---

## 2. Proses bisnis

**Tujuan:** satu bed paling banyak punya satu panggilan aktif. Tekanan berulang, kenaikan ke
Darurat, dan permintaan yang terkirim ulang karena Wi-Fi tersendat tidak pernah melahirkan
panggilan kedua. Setiap perubahan meninggalkan tepat satu jejak.

**Langkah normal:**

1. Tablet mengirim `POST /calls` dengan header `Idempotency-Key` (UUID baru untuk setiap
   tekanan) dan prioritas `1` (Panggilan) atau `2` (Darurat).
2. Sistem memastikan tablet masih aktif dan sesinya terbaru (`BE-NV-010`). Bed selalu diambil
   dari tablet, tidak pernah dari request.
3. Bila kunci itu sudah tercatat, sistem mengembalikan keadaan panggilan saat ini tanpa jejak
   baru.
4. Bed harus punya penempatan aktif (`InpBedPlacement`, `EndDateTime` kosong).
5. Bila belum ada panggilan aktif, sistem membuat panggilan `Waiting` dan jejak `Pressed`. Bila
   sudah ada, sistem menambah hitungan tekan (`PressedAgain`), atau menaikkan Normal menjadi
   Darurat (`EscalatedToEmergency`). Jam pertama ditekan tidak pernah berubah.
6. Perawat datang dan menahan "Perawat hadir" 3 detik (`POST /calls/current/nurse-arrived`).
   Panggilan menjadi `Handled`, `IsBedsideConfirmed = true`, dan jejaknya `NurseArrived` dengan
   `ActorKind = BedTablet`.
7. Pasien boleh membatalkan panggilan biasa yang masih menunggu
   (`POST /calls/current/cancel`). Panggilan menjadi `Cancelled` dengan jejak
   `CancelledByPatient`.

**Contoh lengkap (AC-1).** 305-A menekan "Panggil perawat" 10.00.00: panggilan baru, tekan 1×.
10.00.20 menekan lagi: tekan 2×, `PressedAgain`. 10.00.50 menekan "Darurat": prioritas naik,
`EscalatedAt = 10.00.50`, jam awal tetap 10.00.00. Jejaknya tiga: `Pressed`, `PressedAgain`,
`EscalatedToEmergency`.

**Jalur tidak normal:**

| Kejadian | Jawaban sistem | Kode |
| --- | --- | --- |
| Header `Idempotency-Key` kosong, bukan UUID, atau UUID nol | "Permintaan tidak lengkap. Coba tekan lagi." (`VAL-NV-29`) | `400` |
| Prioritas bukan 1 atau 2, termasuk tidak dikirim | "Jenis panggilan tidak dikenal." (`VAL-NV-30`) | `400` |
| Bed tanpa penempatan aktif, termasuk bila masih ada panggilan aktif (`NV-DEC-050`) | "Bed ini belum berpenghuni. Bila butuh bantuan, tekan bel di dinding." (`VAL-NV-28`) | `409` |
| Batal pada Darurat, atau pada panggilan yang sudah diklaim | "Panggilan ini tidak bisa dibatalkan. Perawat sedang menuju." (`VAL-NV-31`) | `409` |
| Batal/hadir dengan `Version` lama | "Panggilan sudah berubah. Daftar dimuat ulang." (`VAL-NV-38`) | `409` |
| Batal/hadir sesudah panggilan bed itu selesai | "Panggilan ini sudah selesai." (`VAL-NV-37`) | `409` |
| Batal/hadir di bed yang belum pernah punya panggilan | "Tidak ada panggilan aktif di bed ini." (`VAL-NV-32`) | `404` |
| Kunci yang sudah tercatat untuk bed lain | Pesan `VAL-NV-29` | `400` |
| Tablet nonaktif atau sesi lama | Pesan `VAL-NV-26` / `NV-SESSION-STALE` (dari `BE-NV-010`) | `403` |
| Akun manusia atau TV | Ditolak policy `NurseViewBedTabletAccess` | `403` |

**Contoh balapan (AC-3).** Pasien menekan dua kali dalam 50 ms dengan dua kunci berbeda. Kedua
permintaan mencoba membuat panggilan. Index unik `IX_NvwPatientCall_BedId_Active` menolak
sisipan yang kalah; service membuang sisipan itu, membaca panggilan yang menang, lalu
memperlakukannya sebagai tekan ulang. Hasilnya satu panggilan, tekan 2×.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- Governance: `AGENTS.md` (konvensi API, EF, otorisasi, Git, pelaporan), suite
  `rules/backend/TASK_RULES.md`, `TASK_CLASSIFICATION.md`, `API_RULES.md`, `REVIEW_RULES.md`,
  `REPORT_TEMPLATE.md`, `engineering/BACKEND_ENGINEERING_CONTRACT.md`; registry
  `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 45
- Blueprint: kartu `BE-NV-011` dan tabel berkas bersama; `contracts/api-contract.md` R2 §6–§7;
  `contracts/validation-matrix.md` `VAL-NV-26`–`41`; `contracts/state-transition-matrix.md` §5;
  `02-backend-architecture.md` R2.1, R2.4.2–R2.4.6, R2.10, R2.11; `data/data-dictionary.md`
  `NvwPatientCall` dan `NvwPatientCallEvent`
- Source: model `NvwPatientCall`, `NvwPatientCallEvent`; enum status, prioritas, jenis kejadian,
  pelaku, alasan tutup; `NvwPatientCallConfiguration`, `NvwPatientCallEventConfiguration`;
  `NvwBedTabletContextService`, `NvwPatientCallQueryService`, `NvwBedTabletRuntimeController`,
  `NvwBedTabletRuntimeDtos`, `NvwHttp`, `NvwResult`; `NvwBedTabletService` (pola `Version`,
  penanganan pelanggaran index unik lewat `PostgresException.ConstraintName`, urutan cek
  `UpdateAsync`); `NvwBedTabletController` (pola log); `FinancePatientRefundsController.cs`
  baris 106 (header `Idempotency-Key`); `Program.cs` (opsi JSON, registrasi);
  `PermissionRegistryDescriptor.HasApprovedAlternativeAuthorization`
- DB dev (SELECT read-only): kedua index unik ada dengan nama dan filter yang sama dengan
  configuration; `NvwPatientCall` dan `NvwPatientCallEvent` masih kosong

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/NurseViewManagement/Services/NvwPatientCallService.cs` | **Baru.** `PressAsync`, `CancelByPatientAsync`, `ConfirmNurseArrivedAsync`. Satu perintah = satu `SaveChanges` berisi perubahan panggilan + tepat satu jejak. Idempotensi lewat `NvwPatientCallEvent.IdempotencyKey`. Sisipan yang kalah oleh `IX_NvwPatientCall_BedId_Active`, atau tekan ulang yang kalah oleh token `Version`, dicoba ulang paling banyak 3×. Kunci ganda yang lolos bersamaan dikenali lewat `IX_NvwPatientCallEvent_IdempotencyKey`, lalu dijawab sebagai permintaan ulang. Seluruh jam dari `DateTime.UtcNow` |
| `…/Services/NvwPatientCallQueryService.cs` | +5 baris: pemetaan panggilan untuk tablet beserta nama pengklaim dipisah menjadi `MapTabletCallAsync`, dipakai bersama `GET /current` dan ketiga perintah. Perilaku `GET /current` tidak berubah |
| `…/Controllers/NvwBedTabletRuntimeController.cs` | Tiga endpoint `POST`: `/calls`, `/calls/current/cancel`, `/calls/current/nurse-arrived`, dengan policy `NurseViewBedTabletAccess`. Header dibaca sebagai teks supaya kunci yang bukan UUID mendapat pesan `VAL-NV-29` dalam `ApiResponse`. Log `LoggerService` ditulis di teks pesan: Id panggilan, kode bed, kode tablet, jenis kejadian, aksi, status, prioritas, hitungan tekan. Tanpa identitas pasien |
| `…/DTOs/NvwBedTabletRuntimeDtos.cs` | `NvwTabletCallRequest` (`Priority`), `NvwTabletCallVersionRequest` (`Version`), dan `NvwTabletCallOutcome` (hasil internal service untuk log; tidak dikirim ke klien) |
| `…/Services/NvwResult.cs` | Kode dan pesan `VAL-NV-28`–`32`, `37`, `38`; `NotFound(code, message)` supaya `404` membawa kode `VAL-NV-32` |
| `Program.cs` | Berkas bersama: satu baris `AddScoped<NvwPatientCallService>()` |

Nol baris komentar baru. Enam berkas CRLF tanpa BOM; `Program.cs` tetap tanpa baris baru di akhir.

**Diff ringkas berkas bersama (syarat `NV-DEC-056`).**

| Berkas bersama | Yang ditambahkan | Yang tidak disentuh |
| --- | --- | --- |
| `Program.cs` (+1/−0) | `builder.Services.AddScoped<NvwPatientCallService>();` sesudah `NvwBedTabletContextService` | Seluruh registrasi dan policy lain |

**Keputusan developer di luar teks kartu.** Semuanya mengikuti kontrak; butir 1 dan 2 menengahi
dua baris kontrak yang bertabrakan dan perlu diketahui Rizki.

| No | Keputusan | Alasan |
| ---: | --- | --- |
| 1 | `/calls/current/cancel` dan `/nurse-arrived` tanpa panggilan aktif: **`409` `VAL-NV-37`** bila bed itu pernah punya panggilan (semuanya sudah selesai), **`404` `VAL-NV-32`** bila belum pernah | Kontrak menulis keduanya: AC-5 "tanpa panggilan aktif → `404`" dan AC-7/state §5 "batal sesudah `Handled` → `VAL-NV-37`". Endpoint tidak membawa Id panggilan, sehingga "panggilan yang sudah selesai" hanya bisa dikenali dari riwayat bed. **Contoh:** perawat menahan "Perawat hadir", lalu keluarga menekan "Batal" → `409` "Panggilan ini sudah selesai." |
| 2 | Pada batal, `VAL-NV-31` diperiksa **sebelum** `VAL-NV-38` | Contoh `VAL-NV-31` di validation matrix: keluarga membatalkan sesudah Dewi mengklaim. Versi di tablet pasti sudah usang, tetapi pesan yang benar tetap "Perawat sedang menuju" |
| 3 | Naik Darurat tidak menambah `PressCount`, tetapi `LastPressedAt` diperbarui | Tabel aturan `POST /calls` hanya menulis "+1" untuk tekan ulang; kamus data mendefinisikan `LastPressedAt` sebagai "tekan terakhir" |
| 4 | Panggilan baru `Version = 0`; `EscalatedAt` kosong bila langsung Darurat | Kamus data: bawaan `0`; contoh response api §6 yang menulis `1` hanya contoh. Tidak ada kenaikan prioritas pada Darurat langsung |
| 5 | `ActorUserId` jejak = akun login tablet; `ClosedByUserId` = pengklaim saat "Perawat hadir" (kosong bila belum diklaim) dan kosong saat pasien membatalkan | Kamus data `NvwPatientCallEvent.ActorUserId` dan `NvwPatientCall.ClosedByUserId` |
| 6 | Kunci yang tercatat pada panggilan bed lain → `400` `VAL-NV-29` | Kunci tidak boleh membuka keadaan bed lain |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Tiga endpoint `POST` sesuai api §6. Response `NvwTabletCallResponse` dan `GET /current` tidak berubah. Body tanpa field yang dikenali, misalnya `{}`, mendapat `VAL-NV-30` (tekan) atau dianggap versi `0` (batal/hadir), mengikuti pola `[Required] int Version` modul ini |
| Database | Tanpa migration, tanpa perubahan skema. Menulis `NvwPatientCall` dan `NvwPatientCallEvent`; membaca `NvwBedTablet`, `MstBed`, `MstRoom`, `InpBedPlacement`, `AspNetUsers` |
| Keamanan/Auth | Tanpa policy baru; memakai `NurseViewBedTabletAccess` (`BE-NV-010`), plus pemeriksaan status dan stempel sesi tablet di setiap permintaan. Response, log, dan jejak tanpa nama, inisial, No. RM, maupun umur pasien (`G-09`) |

---

## 4. Dokumentasi endpoint

#### Health Services / Nurse View Management / Bed Tablet Runtime

Base URL: `api/v1/health-services/nurse-view-management/bed-tablet-runtime`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/calls` | Tekan "Panggil perawat" atau "Darurat": membuat panggilan, menambah hitungan tekan, atau menaikkan menjadi Darurat | policy `NurseViewBedTabletAccess` |
| `POST` | `/calls/current/cancel` | Pasien membatalkan panggilan biasa yang masih menunggu | policy `NurseViewBedTabletAccess` |
| `POST` | `/calls/current/nurse-arrived` | Perawat menahan "Perawat hadir"; panggilan selesai di bed | policy `NurseViewBedTabletAccess` |

Ketiganya wajib header `Idempotency-Key` (UUID). Body `/calls`: `{ "priority": 1 }` atau
`{ "priority": 2 }`. Body batal dan hadir: `{ "version": n }`.

**Contoh response `POST /calls` pertama.**
`{ "callId": "…", "priority": 1, "priorityLabel": "Panggilan", "callStatus": 1, "statusLabel": "Menunggu", "firstPressedAt": "2026-11-08T03:00:00Z", "waitingSeconds": 0, "pressCount": 1, "claimedByName": null, "canCancel": true, "canEscalate": true, "canConfirmNurseArrived": true, "isActive": true, "version": 0 }`

Kode status: `200` berhasil, termasuk permintaan ulang dengan kunci yang sama · `400`
`VAL-NV-29`, `VAL-NV-30` · `401` belum login · `403` bukan akun tablet aktif atau sesi lama ·
`404` `VAL-NV-32` · `409` `VAL-NV-28`, `31`, `37`, `38`.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path <6 berkas>` (array path) | Files evaluated **6** (= 6 berkas) · VIOLATION 0 · REVIEW 0 · INFO 0 · `Final result: PASS` | `PASS` | Keluaran checker 2026-11-08 |
| Review diff/scope | Hanya berkas modul Nurse View + satu baris `Program.cs`; tanpa migration; `GET /current` tetap memakai pemetaan yang sama | `PASS` | `git diff --stat`: 5 berkas, +133/−1; 1 berkas baru |
| Nol komentar baru; akhiran baris | Nol `//`/`/*`; 6 berkas CRLF tanpa BOM | `PASS` | `grep`, hitungan byte Node |
| Pemeriksaan index di DB dev (SELECT read-only) | `IX_NvwPatientCall_BedId_Active` unik dengan filter `CallStatus IN (1,2) AND IsDelete = false`; `IX_NvwPatientCallEvent_IdempotencyKey` unik dengan filter `IS NOT NULL` — nama sama dengan konstanta di service | `PASS` | `pg_indexes` 2026-11-08 |
| `dotnet build` | Dijalankan Rizki 8 Nov 2026 sesudah implementasi, berhasil (pernyataan Rizki 9 Nov). Backend dijalankan Rizki 9 Nov; ketiga rute baru tanpa login → `401` | `PASS` menurut pernyataan Rizki; jumlah warning tidak dinyatakan | DLL memuat kelima simbol task ini (pemeriksaan byte agent) |
| Uji HTTP run v1 — 9 Nov 08.17 WIB (`uji-panggilan.mjs`) | 18/21; tiga `FAIL` (4b, 4c, 4d) adalah **cacat skrip**, bukan perilaku: (1) psql di Windows mengakhiri baris dengan CRLF, sehingga kolom terakhir hasil multi-baris terbaca `"t\r"`, padahal DB menunjukkan kelima jejak berkunci; (2) skrip membandingkan `firstPressedAt` secara persis sampai 100 ns, sedangkan respons tekan pertama berpresisi 100 ns (`…26.4374367Z`) dan DB menyimpan mikrodetik (`…26.437436Z`). Nilai tersimpan tidak berubah sepanjang uji | Cacat skrip | `hasil-2026-11-09T01-17-20-865Z/` |
| Uji HTTP run v2 — 9 Nov 08.19 WIB (`uji-panggilan-v2.mjs`: dua perbaikan skrip itu saja, ditambah satu kolom bukti) | **21/21 `PASS`** — rincian di bawah | `PASS` | `hasil-2026-11-09T01-19-23-046Z/hasil.json`, `percobaan.json` (52 permintaan) |
| Balapan index unik benar-benar terjadi (no. 12) | Di kedua run, EF mencatat `23505` `IX_NvwPatientCall_BedId_Active` (01.17.28 dan 01.19.28 UTC) tepat pada dua tekanan paralel; service menangkapnya, kedua permintaan `200`, satu panggilan, tekan 2× | `PASS` | Log `SaveChangesFailed` + `hasil.json` no. 12 |
| Pemindaian log backend (`quilvian-backend-20261109.json`) | 56 permintaan HTTP di jendela run v2: 52 = rekaman skrip, 4 sisanya aset Swagger UI dari Chrome Rizki (anonim). 13 log perintah panggilan: 10 perubahan + 3 permintaan ulang, sama dengan jumlah perubahan dan kirim ulang dalam uji; setiap pesan memuat Id panggilan, kode bed, kode tablet, kejadian, status. Sandi env, nama pasien, No. RM, pola sandi tablet, ruas `password` berisi nilai, dan token: **semuanya 0** sepanjang hari. Galat hanya dua baris EF dari balapan no. 12 | `PASS` | `hasil-2026-11-09T01-19-23-046Z/pindai-log-agent.json` (skrip `pindai-log.mjs`) |
| Keadaan DB sesudah kedua run (SELECT read-only) | Tablet `NVT-000013`–`000016` status 3 dengan alasan; 0 panggilan aktif dan 0 tablet status 1/2 di kedua bed uji; 6 panggilan (4 `Handled`, 2 `Cancelled`), 20 jejak, semuanya berkunci; policy jabatan uji 20 butir, 0 butir tablet | `PASS` | Kueri agent 9 Nov 08.21 WIB |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`.

Uji manual: `REQUIRED`. **Tidak dijalankan:** `dotnet build`, `dotnet run`, `dotnet ef` — milik
Rizki. Tanpa migration.

### Perintah untuk Rizki

```bash
# PC kantor
dotnet build ./QuilvianSystemBackend.sln -p:RunAnalyzers=false -p:SkipMigrationMetadata=true
# laptop
dotnet build-server shutdown
dotnet build ./QuilvianSystemBackend.sln -p:RunAnalyzers=false -p:SkipMigrationMetadata=true -m:1 -nodeReuse:false
# jalankan ulang backend
dotnet run -p:RunAnalyzers=false --no-build
```

### Uji HTTP sesudah build

`node uji-panggilan.mjs <bedTerisi> <bedKosong>` dengan bed `BD-RSMMC-00028` (terisi) dan
`BD-RSMMC-00001` (kosong). Skrip membuat dua tablet uji lewat aktor Perawat Ruangan berizin
sementara (salin → tambah → pulihkan). Jejak dihitung lewat SELECT read-only.

| No | Langkah | Hasil yang diharapkan | AC |
| ---: | --- | --- | --- |
| 1 | Tanpa kunci, kunci bukan UUID, UUID nol; prioritas 3 dan kosong | `400` `VAL-NV-29` ×3; `400` `VAL-NV-30` ×2; tidak ada panggilan | AC-2 |
| 2 | Tablet bed kosong menekan Panggilan dan Darurat | `409` `VAL-NV-28` ×2 | AC-4 |
| 3 | Batal dan hadir di bed yang belum pernah punya panggilan | `404` `VAL-NV-32` ×2 | AC-5 |
| 4 | Tekan, tekan lagi, Darurat | Satu panggilan; tekan 2×; Darurat; `EscalatedAt` terisi; jam awal tetap; jejak `Pressed`, `PressedAgain`, `EscalatedToEmergency`, pelaku tablet | AC-1, AC-8 |
| 5 | Kirim ulang kunci yang sama (dua kunci) | `200` keadaan saat ini; jejak tetap 3 | AC-2 |
| 6 | Tekan Panggilan pada Darurat | Tetap Darurat; tekan 3×; `PressedAgain` | AC-7 (`G-13`) |
| 7 | Batal panggilan Darurat | `409` `VAL-NV-31` | AC-5 |
| 8 | Perawat hadir dengan versi lama | `409` `VAL-NV-38`; tetap aktif | — |
| 9 | Perawat hadir | `200` `Handled`; `IsBedsideConfirmed` true; `CloseReason` `NurseArrived`; jejak `NurseArrived` pelaku tablet | AC-6 |
| 10 | Kirim ulang perawat hadir dengan kunci sama | `200` `Handled`; tanpa jejak baru | AC-2 |
| 11 | Batal dan hadir sesudah `Handled`; `GET /current` | `409` `VAL-NV-37` ×2; panggilan tidak dibuka ulang; `activeCall` kosong | AC-7 (`G-12`) |
| 12 | Dua tekanan paralel, kunci berbeda | Satu panggilan baru (bukan yang lama); tekan 2×; jejak `Pressed` + `PressedAgain` | AC-3 |
| 13 | Batal panggilan biasa menunggu; batal lagi | `200` `Cancelled`, jejak `CancelledByPatient`; lalu `409` `VAL-NV-37` | AC-5, AC-7 |
| 14 | Darurat langsung; pindahkan tablet ke bed lain selama panggilan aktif | `EscalatedAt` kosong; pindah ditolak `409` `VAL-NV-20` (bukti runtime milik `BE-NV-009`) | — |
| 15 | Akun perawat dan SuperAdmin memanggil ketiga `POST` | `403` ×3 | — |
| 16 | Perawat hadir pada panggilan Darurat | `200` `Handled`; bed tanpa panggilan aktif | AC-6 |
| 17 | Tablet dinonaktifkan lalu menekan | `403` `VAL-NV-26` | — |

Pemindaian log memeriksa: nol sandi, nol token, nol nama dan No. RM pasien; jumlah log perintah
panggilan dan log permintaan ulang; nol `Error`.

### Hasil uji HTTP — 9 November 2026 (run v2)

Verifikasi developer oleh agent, bukan UAT. Putusan diambil dari JSON mentah, log backend, dan
SELECT read-only. Tablet uji `NVT-000015` (bed `BD-RSMMC-00028` 326-C, berpenghuni) dan
`NVT-000016` (bed `BD-RSMMC-00001` BAYI-1-A, kosong), dibuat lewat aktor Perawat Ruangan berizin
sementara.

| No | Terukur |
| ---: | --- |
| 1 | Kunci kosong, `bukan-uuid`, UUID nol → `400` `VAL-NV-29` ×3; prioritas `3` dan body `{}` → `400` `VAL-NV-30` ×2; 0 panggilan |
| 2 | Bed kosong, Panggilan dan Darurat → `409` `VAL-NV-28` ×2 |
| 3 | Batal dan hadir di bed yang belum pernah punya panggilan → `404` `VAL-NV-32` ×2 |
| 4a | `200` Menunggu, Panggilan, tekan 1×, versi 0, `canCancel`/`canEscalate`/`canConfirmNurseArrived` true, `firstPressedAt` berakhiran `Z`, nol kunci identitas, nama dan No. RM pasien tidak muncul |
| 4b | Panggilan sama, tekan 2×, versi 1, jam awal sama (presisi mikrodetik) |
| 4c | Panggilan sama, Darurat, `canEscalate`/`canCancel` false, `EscalatedAt` terisi di DB, tekan tetap 2×, jam awal sama |
| 4d | Jejak `1,2,3` (`Pressed`, `PressedAgain`, `EscalatedToEmergency`), `ActorKind` 1 (BedTablet), `ActorUserId` = akun tablet, semuanya berkunci |
| 5 | Dua kunci lama dikirim ulang → `200`, versi tetap 2, jejak tetap 3 |
| 6 | Panggilan pada Darurat → tetap Darurat, tekan 3×, jejak `PressedAgain` |
| 7 | Batal Darurat → `409` `VAL-NV-31`; tetap aktif |
| 8 | Hadir dengan versi lama → `409` `VAL-NV-38`; tetap aktif |
| 9 | Hadir → `200` Ditangani; DB: `IsBedsideConfirmed` true, `CloseReason` 1, `ClosedByUserId` kosong (tidak pernah diklaim), `ClosedAt` terisi; jejak ke-5 `NurseArrived`, pelaku tablet, `CloseReason` 1 |
| 10 | Kunci hadir dikirim ulang → `200` Ditangani; jejak tetap 5 |
| 11 | Batal dan hadir sesudah Ditangani → `409` `VAL-NV-37` ×2; status tetap 3; `GET /current` `activeCall` null |
| 12 | Dua tekanan paralel (`Promise.all`) → `200` ×2, Id panggilan sama, panggilan **baru** (bukan yang sudah selesai), 1 panggilan aktif, tekan 2×, jejak `Pressed` + `PressedAgain` |
| 13 | Batal → `200` Dibatalkan; DB `CloseReason` 5, `ClosedByUserId` kosong; jejak `CancelledByPatient`; 13b batal lagi → `409` `VAL-NV-37` |
| 14 | Darurat langsung → `EscalatedAt` kosong; pindah tablet ke bed lain → `409` `VAL-NV-20` (bukti runtime milik `BE-NV-009`) |
| 15 | Perawat (tekan, hadir) dan SuperAdmin (batal) → `403` ×3 |
| 16 | Hadir pada Darurat → `200` Ditangani; 0 panggilan aktif di bed |
| 17 | Tablet dinonaktifkan lalu menekan → `403` `VAL-NV-26` |

Pembersihan di `finally`: kedua tablet nonaktif; policy jabatan uji dipulihkan identik (20 → 20).

**Bagian kedua AC-4 dan AC-6 — dikecualikan atas keputusan Rizki 9 November 2026.** Keduanya
butuh bed yang kehilangan penempatan aktif selagi panggilannya masih aktif. Rizki meminta agent
membuatnya lewat SuperAdmin dengan koreksi salah catat penempatan
(`POST inpatient-management/bed-occupancies/placements/{id}/corrections`) dari 326-C ke 326-B,
lalu memulihkannya. Sebelum ada penulisan apa pun, penelusuran source menunjukkan pemulihan tidak
mungkin lewat API, sehingga tidak ada data Rawat Inap yang diubah. Kedua bagian tetap terpetakan
ke source: `PressAsync` memeriksa penempatan aktif **sebelum** mencari panggilan aktif, dan
`CloseFromTabletAsync` tidak memeriksa penempatan sama sekali (`NV-DEC-050`).

**Temuan Rawat Inap (milik tim Muhammad Hamzah; dicatat, tidak diperbaiki).** Pada koreksi bed
penempatan aktif, `InpBedOccupancyService.ApplyPlacementCorrectionCoreAsync` mengisi
`target.SupersededByCorrectionId` sebelum memanggil `ReleaseBedStatusCopyAsync`, padahal fungsi
itu hanya mencari penempatan dengan `SupersededByCorrectionId == null`. Akibatnya
`InvalidateAsync` tidak dipanggil, dan bed asal tidak masuk siklus `WaitingCleaning`. Statusnya
tetap `Unverified` dengan `BedStatus` `Cleaning`. Dari keadaan itu:
- mulai pembersihan ditolak, karena butuh `WaitingCleaning`;
- verifikasi awal ditolak dengan "Ada bukti pemakaian lama; lakukan backfill used-dirty dan
  pembersihan", padahal endpoint backfill tidak ada;
- bed tidak `Available`, sehingga tidak bisa dipesan dan koreksi balik pun ditolak.

**Contoh.** Pasien 326-C dikoreksi ke 326-B. Bed 326-C tertahan "Cleaning" selamanya dan hanya
bisa dibebaskan lewat perubahan DB oleh tim Rawat Inap.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC-1 tekan, tekan lagi, Darurat → satu panggilan `Emergency`, `FirstPressedAt` tetap, `EscalatedAt` terisi, jejak `Pressed`/`PressedAgain`/`EscalatedToEmergency` | ✅ Terbukti 9 Nov 2026 | `PressAsync`; uji no. 4a–4d |
| AC-2 kunci sama dua kali → satu jejak; tanpa kunci → `400` `VAL-NV-29`; prioritas `3` → `400` `VAL-NV-30` | ✅ Terbukti 9 Nov 2026 | `ReplayAsync`, `TryParseKey`, `Enum.IsDefined`; uji no. 1, 5, 10 |
| AC-3 dua tekanan ±50 ms, kunci berbeda → satu panggilan, `PressCount = 2` | ✅ Terbukti 9 Nov 2026 | Index unik + `SaveOutcome.ActiveCallTaken`; uji no. 12 dikirim bersamaan, dan log EF membuktikan sisipan kedua benar-benar ditolak index (`23505`) lalu ditangani |
| AC-4 bed tanpa penempatan aktif → `409` `VAL-NV-28`, termasuk bila masih punya panggilan aktif | ✅ Bagian pertama terbukti (uji no. 2). Bagian kedua: bukti runtime **dikecualikan atas keputusan Rizki 9 November 2026**; terpetakan ke source | `PressAsync` memeriksa penempatan aktif sebelum mencari panggilan aktif; alasan pengecualian di §5 |
| AC-5 batal biasa `Waiting` → `Cancelled` + `CancelledByPatient`; Darurat → `409` `VAL-NV-31`; tanpa panggilan aktif → `404` `VAL-NV-32` | ✅ Terbukti 9 Nov 2026 | `CloseFromTabletAsync` (`isCancel`); uji no. 3, 7, 13. Batal pada `Claimed` ditelusuri di source; bukti runtime-nya terambil 9 Nov 2026 lewat uji `BE-NV-012` no. 12 (`409` `VAL-NV-31`, [`BE-NV-012.md`](BE-NV-012.md)) |
| AC-6 perawat hadir → `Handled`, `IsBedsideConfirmed = true`, jejak `NurseArrived` `ActorKind = BedTablet`; tetap berhasil sesudah koreksi penempatan | ✅ Bagian pertama terbukti (uji no. 9, 16). Bagian kedua (`NV-AC-042`): bukti runtime **dikecualikan atas keputusan Rizki 9 November 2026**; terpetakan ke source | `CloseFromTabletAsync` tanpa cek penempatan (`NV-DEC-050`); alasan pengecualian di §5 |
| AC-7 tindakan apa pun sesudah `Handled` → `409` `VAL-NV-37`; tidak dibuka ulang (`G-12`); Darurat tidak turun (`G-13`) | ✅ Terbukti 9 Nov 2026 | Uji no. 6, 11, 12 (tekan sesudahnya melahirkan panggilan baru), 13b |
| AC-8 seluruh jam dari jam server, disimpan UTC | ✅ Terbukti 9 Nov 2026 | Uji no. 4a: `firstPressedAt` berakhiran `Z`, selisih < 2 menit dari jam penguji; DB `timestamptz` `+00` |
| DoD — AC-1–8 terpetakan ke source | Terpenuhi | Tabel ini |
| DoD — build Rizki lulus | Terpenuhi | Pernyataan Rizki 9 Nov; DLL 8 Nov 11.10 memuat kode task ini |
| DoD — uji HTTP tercatat | Terpenuhi | Run v2 21/21; run v1 beserta cacat skripnya juga disimpan |
| DoD — laporan | Terpenuhi | Berkas ini |

**QBE yang berlaku dan dipatuhi:** `QBE-SVC-001` (akses data di `NvwPatientCallService`;
controller hanya memanggil service), `QBE-API-001` (`ApiResponse` lewat `NvwHttp.Map`, kode
status sesuai kontrak), `QBE-PERM-001` (policy perangkat yang disetujui, tanpa hardcode peran),
`QBE-DTO-001`, `QBE-VAL-001`, `QBE-LOG-001` (log perubahan state memuat aktor dan teks pesan),
`QBE-TXN-001` (satu `SaveChanges` untuk panggilan + jejak), `QBE-ENUM-001`. Tidak berlaku:
`QBE-ENT-*`, `QBE-CFG-*`, `QBE-CODE-*`, `QBE-DB-*`, `QBE-PAGE-001`, `QBE-OPT-001`, `QBE-DEL-001`.

**Backend Governance Preflight:** Area `HealthServices` · Module `NurseViewManagement` · prefix
`Nvw` `ACTIVE` (registry baris 45) · applicability `NEW CODE`; `Program.cs` = `TOUCHED LEGACY`
(satu baris, restu `NV-DEC-056`) · branch `rizkiG`.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Sinyal realtime **belum** dikirim; itu bagian `BE-NV-014`. TV dan konsol belum tahu ada panggilan sampai `BE-NV-012`/`015`/`014` |
| Masalah yang diketahui | (1) Selama `BE-NV-012` belum ada, panggilan tidak bisa diklaim. Batal pada `Claimed` (`VAL-NV-31`) dan `ClosedByUserId` = pengklaim saat hadir baru terbukti lewat source; bukti runtime-nya di `BE-NV-012`. (2) Selama `BE-NV-013` belum ada, panggilan pada penempatan yang berakhir tetap aktif. (3) Respons tekan **pertama** membawa `firstPressedAt` berpresisi 100 ns, sedangkan respons berikutnya membawa nilai tersimpan berpresisi mikrodetik (selisih < 1 µs). Tampilan detik dan lama menunggu tidak terdampak; frontend sebaiknya tidak membandingkan string jam secara persis. (4) Setiap balapan index (uji no. 12) menghasilkan dua log `Error` dari EF Core, meski service menanganinya. Itu log bawaan EF, bukan galat aplikasi |
| Risiko tersisa | Panggilan ganda bila penanganan index unik keliru; uji no. 12 di dua run membuktikan balapan nyata ditangani benar |
| Perubahan sampingan | Kode: `NONE`. Data di DB dev bersama akibat uji 9 Nov: tablet uji `NVT-000013`–`000016` dibuat lalu dinonaktifkan dengan alasan; 6 panggilan selesai beserta 20 jejaknya tetap tersimpan sebagai riwayat (tidak ada penghapusan, `G-10`); policy jabatan uji diubah sementara lalu dipulihkan identik di kedua run. Tidak ada data Rawat Inap yang diubah |
| Interupsi | 8 Nov: backend belum hidup sampai Rizki membangun dan menjalankannya. 9 Nov: rencana koreksi penempatan lewat SuperAdmin dihentikan sebelum ada penulisan, karena pemulihannya tidak mungkin lewat API (§5); keputusan Rizki: dikecualikan |
| Status Git | Source dan laporan task ini sudah di-stage Rizki: `M` `NvwBedTabletRuntimeController.cs`, `NvwBedTabletRuntimeDtos.cs`, `NvwPatientCallQueryService.cs`, `NvwResult.cs`, `Program.cs`; `A` `NvwPatientCallService.cs`, laporan ini. Pembaruan 9 Nov pada laporan, roadmap, dan traceability belum di-stage. Bukti uji ada di `QuilvianSystemFrontendDev/test-with-agy/nurse-view/uji-be-nv-011-20261108/` (di-ignore git) |
| Langkah berikutnya | `BE-NV-012` (konsol: daftar dan aksi perawat), `BE-NV-013` (tutup otomatis), dan `BE-NV-015` (strip TV) sekarang boleh dimulai. Temuan Rawat Inap di §5 perlu disampaikan ke Muhammad Hamzah |
