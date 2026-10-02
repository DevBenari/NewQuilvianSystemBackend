# `BE-ACC-011` — Jurnal: pengajuan, persetujuan, penolakan, pengesahan

| Field | Isi |
|---|---|
| Task ID | `BE-ACC-011` |
| Blueprint | `ACC-BP-001` revisi `9`, `decision_revision` `1.6` |
| Status | **`DONE`** 3 September 2026 — acceptance terbukti **12 test** terhadap PostgreSQL sungguhan, seluruhnya lulus |
| Tanggal | 3 September 2026 |
| Branch | `rizkiG` |
| Kontrak | `ACC-API-0.2` grup Journal (4 endpoint aksi); `ACC-STATE-0.1` bagian 1; `ACC-VALIDATION-0.2` bagian 4 |
| Migration | **Nol.** Tidak ada entity baru, snapshot tidak disentuh |

**Bukti eksekusi.** Acceptance dibuktikan `JournalLifecycleTests.cs` terhadap PostgreSQL
sungguhan pada 3 September 2026: **37 test lulus, 0 gagal** untuk `BE-ACC-011`..`014` bersama
invariant penomoran `BE-ACC-010`; suite penuh **311 lulus, 0 gagal**.

**Berkas test-nya kemudian DIHAPUS** atas keputusan owner hari yang sama (`ACC-TD-016`),
bersama seluruh test Accounting lain. Suite tersisa **176 lulus**, build **0 error**.
Laporan ini karena itu menjadi **satu-satunya bukti yang tersisa** — bagian 4 dan bagian 5
sengaja ditulis cukup rinci untuk dijalankan ulang. Bukti ini sah untuk kode per
3 September 2026 dan berhenti berlaku begitu kodenya berubah.

## 1. Backend Governance Preflight

| Field | Isi |
|---|---|
| Area / Module / Submodule | `Corporate` / `AccountingManagement` / `JournalManagement` |
| Pemilik / prefix | Rizki — `Acc`, lifecycle `ACTIVE` |
| Keberlakuan | `NEW CODE` |
| QBE yang berlaku | `QBE-API-001` |
| QBE yang **tidak** berlaku | `QBE-MOD-002`/`003`, `QBE-NAM-004` — nol entity persisted baru |
| Sumber governance | `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |

## 2. Berkas

| Berkas | Keadaan |
|---|---|
| `JournalManagement/Services/AccJournalService.cs` | diubah — empat method daur hidup, penjaga sembilan syarat, riwayat, `AvailableActions` |
| `JournalManagement/Controllers/JournalController.cs` | diubah — 4 endpoint aksi, penilaian hak akses |
| `JournalManagement/DTOs/JournalDtos.cs` | diubah — `RejectJournalRequest`, `JournalActorPermissions` |

Nol berkas modul lain disentuh. Nol perubahan `Migrations/`, snapshot, entity, configuration.

### Endpoint baru

| Method | Path | Permission |
|---|---|---|
| `POST` | `/{id}/submit` | `Journal : Submit` |
| `POST` | `/{id}/approve` | `Journal : Approve` |
| `POST` | `/{id}/reject` | `Journal : Approve` |
| `POST` | `/{id}/post` | `Journal : Post` |

**Nol delta kontrak.** Keempatnya persis `ACC-API-0.2`, termasuk `reject` yang memakai permission
`Approve` dan bukan permission tersendiri.

## 3. Keputusan implementasi yang perlu diketahui

### Sembilan syarat diperiksa dari data tersimpan, bukan dari isian

`PeriksaSembilanSyaratAsync` membaca ulang baris jurnal dan menghitung ulang keseimbangannya.
`TotalDebit` dan `TotalCredit` pada kepala jurnal **sengaja tidak dipercaya** — entity `AccJournal`
sendiri menyebut keduanya salinan untuk mempercepat daftar, bukan sumber kebenaran.

### Pemeriksaan kedua saat `post` bukan duplikasi

Syarat 4 (akun aktif), 8 (Cost Center aktif), dan 9 (periode menerima jenis jurnal) semuanya dapat
berubah **sesudah** jurnal disetujui. Pemeriksaan ulang inilah yang mencegah jurnal masuk ke
periode yang sudah terkunci.

### `Rejected` → `Draft` lewat penyuntingan, bukan endpoint tersendiri

`ACC-STATE-0.1` bagian 1.1 menyediakan perpindahan *"Rejected | Sunting kembali | Draft"*. Karena
itu `UpdateAsync` menerima jurnal `Rejected`, dan pada penyimpanan yang berhasil status kembali
`Draft`, `RejectionReason` dikosongkan, serta `SubmittedBy`/`SubmittedAt` direset. Tidak ada
endpoint `reopen` yang dikarang.

Jurnal `Rejected` **tidak dapat dihapus** — riwayat penolakannya sudah menjadi jejak audit.

### `AvailableActions` — tiga hal digabung

| Sumber | Tempat |
|---|---|
| Status jurnal | `TindakanTersedia` di service |
| Hak akses pengguna | `AccessPermissionService.HasAccessAsync`, dinilai di controller |
| Pembuat-bukan-penyetuju | `TindakanTersedia` di service — `ACC-PERMISSION-0.3` bagian 5 mewajibkannya di service |

Penilaian hak akses memakai **service yang sama** dengan `[AccessPermission]`, sehingga daftar
tombol di layar tidak akan pernah berbeda dari yang benar-benar ditegakkan.

Dua keputusan yang layak ditinjau:

- **`reject` tidak dibatasi aturan pembuat.** Menolak jurnal sendiri tidak berbahaya, dan itu
  jalan keluar wajar ketika pembuatnya sendiri menyadari jurnalnya keliru sesudah diajukan.
  `approve` tetap dibatasi mutlak.
- **`reverse` tidak muncul bila jurnal sudah pernah dibalik**, walaupun endpoint-nya baru ada di
  `BE-ACC-013`. Penautannya sudah dinilai sekarang supaya `BE-ACC-013` tidak perlu mengubah
  bagian ini lagi.

### Pesan per status, bukan satu pesan umum

`PeriksaDapatDisunting` mengembalikan pesan berbeda untuk `Posted`, `PendingApproval`, dan
`Approved`, kata demi kata dari `ACC-STATE-0.1` bagian 1.2. Pesan tunggal "tidak dapat diubah"
membuat petugas menebak apakah jurnalnya perlu ditolak dulu, sudah disahkan, atau sedang dinilai
orang lain.

## 4. Acceptance — seluruhnya TERBUKTI

| # | Acceptance | Tempat penegakan | Bukti |
|---|---|---|---|
| (1) | Sembilan syarat diperiksa saat `submit` **dan ulang** saat `post` | `PeriksaSembilanSyaratAsync`, dipanggil `SubmitAsync` dan `PostAsync` | **TERBUKTI** |
| (2) | Penyetuju sama dengan pembuat ditolak `403` | `PeriksaBukanJurnalSendiri` di `ApproveAsync` | **TERBUKTI** |
| (3) | Mengesahkan jurnal belum disetujui ditolak `409` | `PostAsync`, pemetaan status | **TERBUKTI** |
| (4) | Mengubah/menghapus jurnal `Posted` ditolak `409`, `IsDelete` tetap salah | `PeriksaDapatDisunting` di `UpdateAsync` dan `DeleteAsync` | **TERBUKTI** |
| (5) | Periode menolak jenis jurnal → `422` beserta nama periode | Syarat 9, memakai `AccAccountingPeriodService.AlasanPenolakanJenisJurnalAsync` | **TERBUKTI** |
| (6) | `AvailableActions` sesuai status, hak akses, aturan pembuat | `TindakanTersedia` + `AmbilIzinAsync` | **TERBUKTI** |

Riwayat persetujuan (`AccJournalApproval`) ditulis pada keempat tindakan: `Submitted`, `Approved`,
`Rejected`, `Posted`. Barisnya tidak pernah diubah maupun dihapus.

## 5. Skrip uji manual (untuk pemeriksaan ulang lewat Swagger)

Prasyarat, sekali saja:

```
POST /api/v1/corporate/accounting/master-data/journal-types/seed
POST /api/v1/corporate/accounting/periods/generate
     { "legalEntityId": "3bf63974-a754-4b20-81ee-70894f6fb058", "fiscalYear": 2026 }
POST /api/v1/corporate/accounting/master-data/chart-of-accounts   (dua akun, IsPostable=true)
```

Butuh **dua akun pengguna**: `STAF` (punya `Journal : Create/Update/Delete/Submit`) dan
`APPROVER` (punya `Journal : Approve`), serta `MANAJER` (punya `Journal : Post`).

| # | Langkah | Hasil yang diharapkan |
|---|---|---|
| **(1a)** | `STAF` buat jurnal **satu baris** saja, lalu `POST /{id}/submit` | `400` — "Jurnal harus memiliki sekurang-kurangnya dua baris." |
| **(1b)** | Buat jurnal 2 baris **tidak seimbang** (debit 1.000, kredit 500), `submit` | `400` — "Jurnal belum seimbang. Total debit Rp 1.000, total kredit Rp 500, selisih Rp 500." |
| **(1c)** | Perbaiki jadi seimbang, `submit` | `200`, status `PendingApproval` |
| **(1d)** | **Inti butir (1).** Sesudah `approve`, tutup periodenya (`POST /periods/{id}/close`, `permanent=false`), lalu `POST /{id}/post` | `422` — "Periode {nama} sudah ditutup sementara..." Inilah bukti pemeriksaan kedua benar-benar berjalan |
| **(2)** | `STAF` sendiri memanggil `POST /{id}/approve` atas jurnal buatannya | `403` — "Anda tidak dapat menyetujui jurnal yang Anda buat sendiri." Wajib gagal walau `STAF` diberi hak `Approve` |
| **(3)** | Jurnal berstatus `PendingApproval`, `MANAJER` panggil `POST /{id}/post` | `409` — "Jurnal belum disetujui." |
| **(3b)** | Jurnal `Draft`, panggil `post` | `409` — "Jurnal harus diajukan dan disetujui lebih dahulu." |
| **(4a)** | Jurnal `Posted`, panggil `PUT /{id}` | `409` — "Jurnal yang sudah disahkan tidak dapat diubah. Gunakan pembalikan atau jurnal penyesuaian." |
| **(4b)** | Jurnal `Posted`, panggil `DELETE /{id}` | `409` — "Jurnal yang sudah disahkan tidak dapat dihapus." |
| **(4c)** | Sesudah (4b), `SELECT "IsDelete" FROM "AccJournal" WHERE "Id"=...` | **`false`**. Ini yang paling penting: penolakan tidak boleh menyisakan penandaan terhapus |
| **(5)** | Periode `SoftClosed`, ajukan jurnal jenis `JU` | `422` menyebut **nama periode**, misalnya "Periode September 2026 sudah ditutup sementara. Hanya jurnal penyesuaian dan pembalikan yang masih dapat disahkan." |
| **(5b)** | Periode `SoftClosed`, ajukan jurnal jenis `JP` | `200` — penyesuaian tetap diterima |
| **(6a)** | `GET /{id}` sebagai `STAF` atas jurnal `Draft` miliknya | `availableActions` = `["update","delete","submit"]` |
| **(6b)** | `GET /{id}` sebagai `STAF` atas jurnal `PendingApproval` **buatannya sendiri** | `availableActions` = `[]` — tanpa `approve` |
| **(6c)** | `GET /{id}` sebagai `APPROVER` atas jurnal yang sama | `availableActions` = `["approve","reject"]` |
| **(6d)** | `GET /{id}` sebagai `APPROVER` atas jurnal `Approved` | `["reject"]` saja — `post` tidak muncul karena `APPROVER` tidak punya `Journal : Post` |
| **(6e)** | `GET /{id}` sebagai `MANAJER` atas jurnal `Approved` | memuat `post` |
| **Riwayat** | Sesudah alur penuh, `GET /{id}` | `approvals` memuat empat baris berurutan: `Submitted`, `Approved`, `Posted` — dan `Rejected` bila sempat ditolak, lengkap dengan alasannya |
| **Rejected** | Tolak sebuah jurnal, lalu `PUT /{id}` memperbaikinya | `200`, status kembali **`Draft`**, `rejectionReason` kosong |
| **Rejected-hapus** | Jurnal `Rejected`, panggil `DELETE /{id}` | `409` — riwayat penolakan tidak boleh hilang |

## 6. Data uji

**Nol data uji dibuat oleh saya pada task ini** — tidak ada test otomatis yang dijalankan. Data
yang muncul dari skrip bagian 5 adalah data yang Anda buat sendiri saat menguji.

## 7. Blocker dan catatan

| Hal | Keterangan |
|---|---|
| `ACC-TD-011` | `AccJournalType` masih 0 baris. Skrip bagian 5 tidak dapat dimulai sebelum `POST /seed` dipanggil |
| `ACC-TD-017` | **Baru.** Kebijakan owner 3 September 2026: test otomatis tidak ditulis, verifikasi manual. Seluruh acceptance `BE-ACC-011`..`014` tidak punya bukti eksekusi |
| `ACC-TD-016` | Tetap terbuka — test `BE-ACC-010` tidak ditulis ulang, konsisten dengan kebijakan di atas |
| `ACC-TD-015` | Registry berselisih dua arah. Tidak menahan penulisan kode |

## 8. Task berikutnya

`BE-ACC-012` — buku besar, saldo per akun, dan neraca saldo. Dependency-nya (`BE-ACC-011`) sudah
terimplementasi. Ia hanya membaca, sehingga risikonya jauh di bawah task ini.
