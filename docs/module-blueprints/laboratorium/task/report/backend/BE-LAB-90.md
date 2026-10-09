# Laporan Perubahan Backend — `BE-LAB-90`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-90` |
| Judul | Konfirmasi pada pesanan yang wadahnya layak, Proses wajib terkonfirmasi, dan izin Konfirmasi tersendiri |
| Slice | Gelombang `MVP-12a` — `EPIC-LAB-18` (BR-138); **melebur `BE-LAB-89`** |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian **6ar.1** |
| Trace | `LAB-DEC-193`..`LAB-DEC-197`, `LAB-DEC-199`; `FR-18.1`..`FR-18.5`; `AC-283`..`AC-286`, `AC-288` |
| Contract version | `LAB-API-v1` **`r40`** bagian 35; `LAB-STATE-v1` **`r8`** bagian 10; `LAB-VAL-v1` **`r17`** bagian 19 (`VAL-71`, `VAL-151`); `LAB-PERM-v1` **revision 13** bagian 15 — keempatnya **`approved` 2026-10-07** lewat `LAB-REQ-016` |
| Dependency | Nol |
| Klasifikasi | `MEDIUM` — skor 6: repository 0, berkas diperiksa 1 (±14), berkas diubah 0 (2), logika bisnis 1, kontrak API 2 (perilaku dan hak akses berubah), database 0 (nol schema; perilaku persistence tetap), keamanan/auth 2 (pemindahan izin), UI/workflow 0 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/LaboratoryManagement/Controllers/LabOrderController.cs`, `Services/LabOrderService.cs`; laporan ini beserta status roadmap dan traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `f17cb984` (branch `yoga`, upstream `origin/yoga`) |
| Tanggal | 2026-10-07 |
| Status | ✅ **`SELESAI`** — naik 2026-10-07 sesudah verifikasi `FE-LAB-50`. Semula ⚠: harness 30/30 dan tiga penolakan HTTP asli. **Susulan:** jalur `200` HTTP (`confirm` pada `Accepted` dan `start-process` pada pesanan terkonfirmasi, `LAB-RSMMC-000003`, akun analis asli, izin tulis pemilik modul) dan `403` per izin (analis: `cancel`, `pathology-context`) terbukti — bagian 5a |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `LaboratoryManagement` |
| Pemilik / prefix registry | `Lab` — `ACTIVE` (`MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris LaboratoryManagement) |
| Keberlakuan | `TOUCHED LEGACY` — `LabOrderController.Confirm`, `.StartProcess`; `LabOrderService.ConfirmAsync`, `.StartProcessAsync`. Nol `NEW CODE` persisted, nol `LEGACY MIGRATION` |
| QBE yang berlaku | `QBE-SVC-001` (aturan di service, controller hanya memetakan), `QBE-API-001` (`ApiResponse`, `409` lewat `LabOrderConflictException` yang sudah ada), `QBE-PERM-001` (`[AccessAction]` berpasangan dengan `[AccessPermission]`), `QBE-VAL-001` (`VAL-71`, `VAL-151`), `QBE-LOG-001` (riwayat `Order.Confirm`/`Order.StartProcess` beserta aktor), `QBE-TXN-001` (satu `SaveChanges` per tindakan, tidak berubah) |
| Governance yang dibaca | `AGENTS.md` backend; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; `rules/backend/TASK_RULES.md`, `TASK_CLASSIFICATION.md`, `API_RULES.md`, `REVIEW_RULES.md`, `REPORT_TEMPLATE.md` |
| Pengecualian | Nol |

---

## 1. Masalah yang diperbaiki

| Sebelum | Akibat |
| --- | --- |
| Konfirmasi hanya sah pada `Requested`; pada `Accepted` → `409` `VAL-71` | Urutan v1 — sampel diterima dulu, baru dikonfirmasi — tidak dapat dijalankan |
| `start-process` tidak memeriksa konfirmasi | Pesanan dapat dikerjakan tanpa dokter pemeriksa tercatat |
| `start-process` pada status salah → `400` dari `MoveOrderStatusAsync` | Berbeda dari `LAB-STATE-v1` (`409`) — `CAP-P22-03`, `BE-LAB-89` |
| `confirm` dijaga `LabOrder : Update` (dengan `DisplayName` keliru *"Cancel Lab Order"*) | Siapa pun yang boleh Konfirmasi otomatis boleh Batalkan dan menulis konteks klinis; jabatan Analis tidak memegangnya sama sekali |

**Contoh:** pesanan Hemoglobin pasien rawat jalan, wadah sudah layak → *Diterima*. Kemarin analis menekan
Konfirmasi → `409` *"sudah melewati tahap konfirmasi"*, lalu tetap dapat menekan Proses tanpa konfirmasi.
Sekarang Konfirmasi diterima dan status tetap *Diterima*; Proses ditolak sampai konfirmasi ada.

---

## 2. Proses bisnis

**Tujuan:** setiap pesanan yang dikerjakan laboratorium punya dokter pemeriksa tercatat, dengan urutan v1.
**Pelaku:** analis laboratorium (pemegang `LabOrder : Confirm` dan `LabOrder : Process` sesudah rilis).
**Pemicu:** pesanan tampil di daftar pasien lab.

1. Wadah pesanan dinyatakan layak → `Accepted` (tidak berubah).
2. Analis mengonfirmasi dan memilih dokter pemeriksa → status **tetap** `Accepted`; konfirmator, waktu, dan dokter
   tercatat; satu riwayat `Order.Confirm` `Accepted`→`Accepted`.
3. Analis menekan Proses Pemeriksaan → `InProcess`.
4. Hasil dirilis, analis menekan Selesaikan → `Completed` (tidak memeriksa konfirmasi).

**Perubahan status:**

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat |
| --- | --- | --- | --- | --- |
| `Requested` | Konfirmasi | `Confirmed` | `LabOrder : Confirm` | Belum pernah dikonfirmasi; dokter aktif |
| `Accepted` | Konfirmasi | `Accepted` (tetap) | `LabOrder : Confirm` | Belum pernah dikonfirmasi; dokter aktif |
| `Accepted` | Proses | `InProcess` | `LabOrder : Process` | `ConfirmedAt` terisi |

**Jalur tidak normal:** konfirmasi kedua → `409` `VAL-70`; konfirmasi pada `InProcess`/`Completed`/`Cancelled`/
`OnHold`/`Draft` → `409` `VAL-71`; Proses sebelum konfirmasi → `409` `VAL-151`; Proses pada status selain
`Accepted` → `409` (bunyi lama). Semua penolakan tidak menulis apa pun.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`LabOrderController.cs`; `LabOrderService.cs` (`ConfirmAsync`, `StartProcessAsync`, `MoveOrderStatusAsync`,
`CompleteAsync`, `HoldAsync`, `GetDetailAsync`, `ExecuteAsync`); `LabPathologyReportController.cs`;
`Attributes/AccessActionAttribute.cs`, `AccessPermissionAttribute.cs`; `Services/Security/PermissionRegistryValidator.cs`,
`PermissionRegistryDescriptor.cs`; `Seeders/AccessMenuSeeder.cs`; `LabOrderDtos.cs`; `LabOrder.cs`; kontrak `r40`/`r8`/`r17`/rev 13;
`02-backend-architecture.md` bagian 24.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/LaboratoryManagement/Controllers/LabOrderController.cs` | `Confirm`: `[AccessAction("Confirm", "Confirm Lab Order", …, AccessType = AccessTypes.Update, SortOrder = 3)]` + `[AccessPermission("LabOrder", "Confirm")]` menggantikan pasangan `Update`. `StartProcess`: `ProducesResponseType(400)` dicabut — jalan `400` tidak ada lagi. +13 −6 |
| `Areas/HealthServices/LaboratoryManagement/Services/LabOrderService.cs` | `ConfirmAsync`: `VAL-71` menerima `Requested` atau `Accepted`; status berpindah ke `Confirmed` hanya dari `Requested`; riwayat mencatat status tujuan sebenarnya. `StartProcessAsync`: muat pesanan → status bukan `Accepted` → `LabOrderConflictException` (bunyi lama) → `ConfirmedAt` kosong → `LabOrderConflictException` `VAL-151` → `MoveOrderStatusAsync` tanpa perubahan. +58 −11 |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `POST /{id}/confirm` sah juga pada `Accepted` (status tetap); `PUT /{id}/start-process` status salah `400` → `409`, dan `409` `VAL-151` baru. Nol ruas, nol endpoint baru |
| Database | Nol schema, nol migration. **Satu baris data platform**: `SysActionAccess` `LabOrder : Confirm` ditambahkan otomatis oleh `AccessMenuSeeder` saat backend baru menyala — **sudah terjadi di `QuilvianNewDevYoga`** pada uji HTTP 2026-10-07 (bagian 5); tanpa pemegang kebijakan |
| Keamanan/Auth | **Breaking:** pemegang `LabOrder : Update` saja tidak lagi dapat Konfirmasi (hari ini: jabatan Dokter Umum). Analis perlu `LabOrder : Confirm` lewat langkah rilis `MVP-12c`. Batalkan dan konteks klinis PA tetap `LabOrder : Update` |

---

## 4. Dokumentasi endpoint

#### Health Services / Laboratory Management / Lab Order

Base URL: `api/v1/health-services/laboratory-management/lab-orders`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/{id}/confirm` | Mengonfirmasi pesanan beserta dokter pemeriksa — pada *Diminta* atau *Diterima* | `LabOrder : Confirm` |
| `PUT` | `/{id}/start-process` | Memulai pemeriksaan — hanya pesanan *Diterima* yang sudah dikonfirmasi | `LabOrder : Process` |

**Kode status:** `200` berhasil; `403` tanpa izin; `404` pesanan tidak ada; `409` sudah dikonfirmasi (`VAL-70`),
sudah lewat tahap konfirmasi (`VAL-71`), status bukan *Diterima*, atau belum dikonfirmasi (`VAL-151`); `422`
dokter pemeriksa kosong/tidak aktif (`VAL-72`/`VAL-73`).

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=False` | 0 error, 242 warning, 1 menit 56 detik; **nol** warning dari kedua berkas yang diubah | `PASS` | Log build |
| Harness InMemory (`ApplicationDbContext` asli, nol tulis ke DB bersama) | **30/30** | `PASS` | Rincian di bawah |
| — `AC-283` | Konfirmasi pada `Accepted` → `200`, status tetap `Accepted` (respons dan data), `ConfirmedAt`/konfirmator = aktor/dokter terisi, `Version` 3→4, tepat satu riwayat `Order.Confirm:Accepted->Accepted` | `PASS` | |
| — `AC-284` | Konfirmasi kedua → `409` `VAL-70`; `InProcess`/`Completed`/`Cancelled`/`OnHold`/`Draft` → `409` `VAL-71`, nol perubahan; `Requested` → `200` `Confirmed`, riwayat `Requested->Confirmed`; tanpa dokter → `422` `VAL-72`; dokter nonaktif → `422` `VAL-73` | `PASS` | |
| — `AC-285` | `Accepted` belum dikonfirmasi → `409` `VAL-151` kata per kata, nol perubahan; dikonfirmasi sesudah wadah layak → `200` `InProcess`; dikonfirmasi **sebelum** wadah layak (`Accepted` + `ConfirmedAt`) → `200` | `PASS` | |
| — `AC-285` urutan dan `BE-LAB-89` | `InProcess` belum dikonfirmasi → `409` **status** (bukan `VAL-151`); `Requested`/`Confirmed`/`Completed`/`Cancelled`/`OnHold` → `409` (dahulu `400`), bunyi lama, nol perubahan; pesanan tidak ada → `404` | `PASS` | |
| — `AC-286` | `InProcess` tanpa konfirmasi, hasil dirilis → `complete` `200` `Completed` | `PASS` | |
| — Non-regresi | `hold` pada `Cancelled` dan `OnHold` tetap `400` | `PASS` | |
| — `AC-288` metadata | `Confirm` = `LabOrder:Confirm` / `Confirm Lab Order` / `Update`; `Cancel` tetap `Update`; `StartProcess`/`Complete` tetap `Process`; konteks klinis PA: baca `Read`, simpan `Update` | `PASS` | Refleksi atribut |
| — Registrasi | `PermissionRegistryValidator` lolos (1625 aksi); snapshot `LabOrder`: Confirm, Create, Hold, Process, Read, Update, Verify | `PASS` | |
| HTTP asli, backend lokal Development + DB dev, superadmin | `confirm` `LAB-RSMMC-000002` (`InProcess`) → `409` *"Pesanan ini sudah melewati tahap konfirmasi."*; `start-process` `000002` → `409` *"Pesanan berstatus InProcess tidak dapat dipindahkan ke InProcess."*; `start-process` `000003` (`Accepted`, belum dikonfirmasi) → `409` `VAL-151` | `PASS` | Ketiganya ditolak **sebelum menulis**: `Version` dan jumlah riwayat kedua pesanan sama sebelum/sesudah (v4/11, v1/7), dibaca baca-saja |
| Registrasi saat menyala | `SysActionAccess` `LabOrder` kini memuat `Confirm \| Confirm Lab Order \| Update \| aktif` | `PASS` | Kueri baca-saja sesudah startup |
| Jalur `200` lewat HTTP | Susulan 2026-10-07 — lihat bagian 5a | `PASS` | |
| `403` per izin lewat HTTP | Susulan 2026-10-07 — lihat bagian 5a | `PASS` | |

Uji manual: `NOT APPLICABLE` — task backend; layar dikerjakan `FE-LAB-50`.

**Tidak dijalankan:** akun pemegang `LabOrder : Update` tanpa `Confirm` (dev nol akun seperti itu; dibuktikan metadata atribut); uji otomatis repository — `Tests/`
dikecualikan `.gitignore` (`LAB-RDY-C04`), harness tinggal di scratchpad sesi.

### 5a. Verifikasi susulan 2026-10-07 (bersama `FE-LAB-50`)

Backend lokal Development dari biner task ini, DB dev bersama, akun analis asli (*Penunjang Medis / Analis
Laboratorium*) sesudah langkah rilis `MVP-12c` langkah 2 dijalankan di dev (lihat
[`FE-LAB-50.md`](../frontend/FE-LAB-50.md) bagian 6.2). Tulis sungguhan atas izin pemilik modul hanya pada
`LAB-RSMMC-000003`.

| Panggilan | Hasil | Klasifikasi |
| --- | --- | --- |
| `POST /lab-orders/{000003}/confirm` (analis, pesanan `Accepted` belum dikonfirmasi) | `200`; `orderStatus` tetap `Accepted`; DB: `ConfirmedAt` terisi, riwayat `Order.Confirm Accepted->Accepted`, `Version` 1→2 | `PASS` (`AC-283`, `AC-288`) |
| `PUT /lab-orders/{000003}/start-process` (analis, sesudah dikonfirmasi) | `200` `InProcess`; riwayat `Order.StartProcess Accepted->InProcess`, `Version` 2→3 | `PASS` (`AC-285`) |
| `PUT /lab-orders/{000003}/start-process` sebelum dikonfirmasi | `409` `VAL-151`, nol tulis | `PASS` |
| `PUT /lab-orders/{000002}/cancel` (analis) | `403` | `PASS` (`AC-288`) |
| `PUT /lab-orders/{000002}/pathology-context` (analis) | `403` | `PASS` (`AC-288`) |

**Temuan di luar task ini (T1 `FE-LAB-50`).** jawaban detail pesanan (`GetDetailAsync`) tidak membawa `confirmedAt`/`confirmedByName`/`examinerDoctorName`/`confirmedByUserId`/`examinerDoctorId` yang dijanjikan `r13`. Jawaban `confirm` dan `GET /lab-orders/{id}` membalas kelimanya `null` walau DB terisi. Bukan regresi `BE-LAB-90` (`r40` menyatakan jawaban *tidak berubah*), tetapi kini terlihat di layar karena status tidak lagi pindah ke `Confirmed`. Diusulkan sebagai task backend tersendiri.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-283` | Terpenuhi (harness + HTTP `200` asli) | Bagian 5, 5a |
| `AC-284` | Terpenuhi (harness + HTTP `409` `VAL-71`) | Bagian 5 |
| `AC-285` termasuk urutan | Terpenuhi (harness + HTTP `409` status, `VAL-151`, dan `200` sesudah dikonfirmasi) | Bagian 5, 5a |
| `AC-286` | Terpenuhi (harness) | Bagian 5 |
| `AC-288` sisi backend | Terpenuhi — analis pemegang `Confirm` tanpa `Update`: `confirm` `200`, `cancel` dan `pathology-context` `403`. Butir *pemegang `Update` saja* dari metadata atribut | Bagian 5, 5a |
| Non-regresi `hold`/`resume`, `complete` | Terpenuhi | Bagian 5 |
| Registrasi: validator lolos, `Confirm` di `SysActionAccess` | Terpenuhi | Bagian 5 |
| DoD — build hijau; laporan; kolom *Status* `r40` 35.1 | Terpenuhi | Status 35.1 diperbarui bersama laporan ini |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Nol baru; 242 warning build adalah warning repository yang sudah ada |
| Masalah yang diketahui | T1 (bagian 5a) — jawaban detail tanpa ruas konfirmasi `r13`. `ConfirmAsync` mengirim payload log berisi ruas `Id` — menurut catatan proyek, `LoggerService` dapat menimpa pelaku dengan ruas itu. Sudah ada sebelum task ini; tidak diubah |
| Risiko tersisa | **Jeda rilis:** sejak backend ini dideploy, Konfirmasi butuh `LabOrder : Confirm` dan Proses butuh konfirmasi. Beri `Confirm` kepada Analis **segera** (6ar.2), lalu deploy `FE-LAB-50`. Layar lama sudah membaca `409` sebagai *status berubah*, tetapi belum menawarkan Konfirmasi pada *Diterima* |
| Perubahan sampingan | Baris `SysActionAccess` `LabOrder : Confirm` di DB dev bersama — dibuat seeder saat backend baru dinyalakan untuk uji HTTP. Tanpa kebijakan jabatan; tidak dipakai biner lama |
| Interupsi | `NONE` |
| Status Git | Backend source: ` M …/Controllers/LabOrderController.cs`, ` M …/Services/LabOrderService.cs`; ditambah dokumen blueprint putaran 23 dan perencanaan yang belum di-commit |
| Langkah berikutnya | `FE-LAB-50` ⚠ selesai; langkah rilis `MVP-12c` langkah 2 sudah di dev. Task backend untuk T1; keputusan jalur dokter pemeriksa (T2 `FE-LAB-50`) sebelum langkah 3 |
