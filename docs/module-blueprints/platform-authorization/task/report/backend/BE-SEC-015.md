# Laporan Perubahan Backend — `BE-SEC-015`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-SEC-015` |
| Judul | Pengerasan skrip policy `BE-SEC-003B` dan varian eksekusi DBeaver |
| Slice | Task terpisah di luar rantai — pemeliharaan skrip penerapan, bukan implementasi modul |
| Roadmap | [`../../../roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) — bagian *Task terpisah di luar rantai* |
| Trace | [`evidence/14`](../../../evidence/14-owner-policy-matrix-and-deployment-preparation.md), [`BE-SEC-014.md`](BE-SEC-014.md) |
| Contract version | `NOT APPLICABLE` — tidak ada kontrak API yang disentuh |
| Dependency | `BE-SEC-014` (`f209a376`) — selesai dan sudah di-commit |
| Klasifikasi | `MEDIUM` — nol perubahan source aplikasi; dua skrip database dan dua dokumen |
| Task mode | `BACKEND` |
| Target tulis | `Migrations/scripts/`, `docs/module-blueprints/platform-authorization/` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `f209a3760c9b6c25c5c1330333b8061eecfee655` |
| Tanggal | 17 September 2026 |
| Status | **Selesai.** Seluruh artefak siap; tidak satu pun dijalankan |

---

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | Shared Platform (otorisasi) |
| Module | `PLATFORM_AUTHORIZATION` |
| Submodule | `SysAccessPolicy` — skrip penerapan `BE-SEC-003B` |
| Pemilik/prefix pada registry | `Sys` — tidak ada perubahan registry |
| Keberlakuan | `TOUCHED LEGACY` — **nol berkas source aplikasi berubah pada task ini** |
| Status registry | Tidak berubah. Verifier tetap `1.300 / 340 / 48`, `BE-SEC-003` 24/24 |
| QBE ID yang berlaku | Tidak ada. Tidak ada entity, rename, migration, maupun eksekusi database |
| Wewenang database | **TIDAK DIBERIKAN dan TIDAK DIPAKAI.** `BE-SEC-003B` Tahap 1 dan Tahap 2 tetap `PAUSED` |
| Pemilihan ID task | `BE-SEC-004`–`BE-SEC-011` sudah dipesan sebagai dekomposisi `BE-SEC-002` berstatus `NOT STARTED`; `BE-SEC-012`–`BE-SEC-014` terpakai. `BE-SEC-015` diverifikasi bebas: nol rujukan pada working tree, riwayat commit, dan roadmap |

---

## 1. Masalah yang diperbaiki

Tiga cacat ditemukan pada skrip penerapan `BE-SEC-003B` saat validasi `BE-SEC-014`. Ketiganya
membuat skrip itu tidak dapat dijalankan sebagaimana dimaksud, dan dua di antaranya baru akan
terlihat pada saat eksekusi — persis mode kegagalan yang dilarang berkas itu sendiri.

### 1.1 Kolom audit `NOT NULL` tidak disebut — PostgreSQL `23502`

Kedua `INSERT INTO public."SysAccessPolicy"` pada Tahap 1 hanya menyebut sebelas kolom dan
menghilangkan `UpdateBy`, `DeleteBy`, serta `CancelBy`.

Ketiganya `uuid NOT NULL` **tanpa default database**. Berbeda dari tabel lain di repository ini
— [`initializeNurseStationCluster.cs`](../../../../../../Migrations/20260625033243_initializeNurseStationCluster.cs)
memberi `defaultValue: new Guid("00000000-…")` — `SysAccessPolicy` dibuat lebih awal dan tidak
pernah memperoleh default itu.

Akibatnya Tahap 1 akan gagal pada `INSERT` pertama dengan:

```
ERROR: null value in column "UpdateBy" of relation "SysAccessPolicy"
       violates not-null constraint  (SQLSTATE 23502)
```

Kegagalannya aman — seluruh transaksi dibatalkan dan tidak ada baris yang berubah — tetapi Tahap 1
tidak akan pernah menghasilkan satu baris pun.

### 1.2 Tahap 2 tidak memiliki gerbang kardinalitas

Tahap 2 hanya berupa `UPDATE` telanjang. Berapa pun baris yang cocok akan dinonaktifkan, tanpa satu
pun pemeriksaan. Bila keadaan database berbeda dari bukti yang mendasari keputusan pemilik —
misalnya tujuh baris menggantung, bukan empat — `UPDATE` itu tetap berjalan dan mencabut hak yang
tidak pernah ditinjau siapa pun.

Asimetri ini mencolok justru karena Tahap 1 sudah dijaga dua gerbang (4.0 prasyarat dan 4.2b
verifikasi sesudah tulis), sementara Tahap 2 — satu-satunya tahap yang **mencabut** hak — tidak
dijaga sama sekali.

### 1.3 Operator memakai DBeaver, skrip menuntut psql

Skrip `BE-SEC-003B` memakai meta-command psql pada bagian yang dieksekusi dan interpolasi variabel
bergaya psql pada seluruh bagian tulis. Tidak satu pun dikenal DBeaver. Operator yang tidak memiliki
psql karena itu tidak dapat menjalankan skrip itu sama sekali.

Repository sudah memiliki kebiasaan yang jelas untuk hal ini — akhiran `-dbeaver` menandai varian
DBeaver, seperti pada pasangan `diagnose-be-sec-003-permission-state.sql` — tetapi `BE-SEC-003B`
belum pernah memperoleh pasangannya.

---

## 2. Proses bisnis

Tidak ada proses bisnis baru. Kontrak `BE-SEC-003B` yang sudah disetujui pemilik **tidak berubah**:

| Kontrak | Nilai | Status pada task ini |
| --- | --- | --- |
| Peta pemecahan lama → baru | 23 identitas | Tidak disentuh |
| Identitas target `BE-SEC-003` | 24 wajib terdaftar dan aktif | Tidak disentuh |
| Aturan `PatientAssessment.Amend` | Diberikan kepada pemegang efektif `Complete`, dihitung **sesudah** 4.1 | Tidak disentuh |
| Tahap 1 | 35 pelestarian + 1 `Amend` = **36** baris | Tidak disentuh |
| Tahap 2 | **4** policy dinonaktifkan: `PatientProcedure.Update` = 1, `DoctorQueue.Update` = 3 | Angka tidak berubah; kini **ditegakkan**, sebelumnya hanya diasumsikan |
| Pemecahan `DoctorQueue` | 6 identitas | Tidak disentuh |
| Pemecahan `PatientProcedure` | 5 dari `Update` + 1 dari `Create` | Tidak disentuh |
| Urutan Tahap 1 → JEDA → Tahap 2 | Dua transaksi terpisah | Tidak disentuh |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas | Alasan diperiksa | Hasil |
| --- | --- | --- |
| `Migrations/20260516060830_initializeSetup.cs` | Sumber otoritatif DDL `SysAccessPolicy` | `UpdateBy`/`DeleteBy`/`CancelBy` = `uuid NOT NULL`, tanpa `defaultValue` |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Model EF yang berlaku sekarang | Tidak ada `HasDefaultValue` pada ketiganya |
| `Repositories/Configurations/Global/SysAccessPolicyConfiguration.cs` | Konfigurasi fluent | Default hanya pada `IsAllowed` dan `IsActive`; indeks unik kunci alami dikonfirmasi |
| `Areas/Administrator/Setting/Controllers/RoleAccessController.cs` | Satu-satunya tempat aplikasi membuat `SysAccessPolicy` | Konvensi: `CreateBy` = operator; `UpdateBy` = `DeleteBy` = `CancelBy` = `Guid.Empty` |
| `Models/IdentityModel.cs` | Default model | `Guid.Empty` untuk ketiganya |
| `tools/authorization-verifier/required-identities.txt` | Sumber kebenaran 24 identitas | Cocok persis dengan daftar pada kedua varian skrip |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Migrations/scripts/be-sec-003b-policy-expansion.sql` | Kolom audit pada dua `INSERT`; gerbang kardinalitas Tahap 2; catatan baseline source |
| `Migrations/scripts/be-sec-003b-policy-expansion-dbeaver.sql` | **Baru.** Varian DBeaver resmi |
| `docs/.../task/report/backend/BE-SEC-015.md` | **Baru.** Laporan ini |
| `docs/.../roadmap/backend-roadmap.md` | Entri `BE-SEC-015` di luar rantai |

### 3.3 Kontrak `INSERT` yang diperbaiki

```sql
INSERT INTO public."SysAccessPolicy" (
    "Id", "DepartmentId", "PositionId", "ControllerAccessId", "ActionAccessId",
    "IsAllowed", "IsActive", "IsDelete", "IsCancel", "CreateDateTime", "CreateBy",
    "UpdateBy", "DeleteBy", "CancelBy")          -- <- tiga kolom yang hilang
SELECT
    …,
    true, true, false, false, now(), <operator>::uuid,
    '00000000-0000-0000-0000-000000000000'::uuid,   -- UpdateBy
    '00000000-0000-0000-0000-000000000000'::uuid,   -- DeleteBy
    '00000000-0000-0000-0000-000000000000'::uuid    -- CancelBy
```

Empat belas kolom `NOT NULL` disebut seluruhnya. Tiga kolom timestamp yang *nullable*
(`UpdateDateTime`, `DeleteDateTime`, `CancelDateTime`) sengaja dibiarkan kosong. `CreateBy` tetap
memuat operator terverifikasi — hanya kolom itu yang menunjuk pelaku, sesuai konvensi aplikasi.

### 3.4 Gerbang kardinalitas Tahap 2

Empat lapis, menggantikan satu `UPDATE` telanjang:

| Langkah | Fungsi |
| --- | --- |
| 4.3a | Membekukan sasaran ke tabel sementara, diturunkan dari kunci bisnis `(resource, action)` lewat registry. Tidak ada GUID policy yang diketik |
| 4.3b | **Sebelum menulis**: menegaskan total = 4, `PatientProcedure.Update` = 1, `DoctorQueue.Update` = 3. Selisih apa pun membatalkan transaksi |
| 4.3c | `UPDATE` **dibatasi** pada himpunan beku, dengan `RETURNING` direkam supaya jumlah baris yang benar-benar berubah dapat ditegaskan |
| 4.3d | **Sesudah menulis**: tepat 4 baris berubah, keempatnya nonaktif, dan tidak ada policy lain yang ikut tersentuh (dideteksi lewat `UpdateDateTime = now()`, yaitu `transaction_timestamp()`) |

`UpdateBy` tetap operator terverifikasi, `UpdateDateTime` tetap `now()`, dan tahap ini tetap
berakhir `ROLLBACK`.

### 3.5 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NONE` |
| Source aplikasi | `NONE` — nol berkas `.cs` berubah |
| Skema database | `NONE` — tidak ada EF migration |
| Eksekusi database | `NONE` — tidak satu pun skrip dijalankan |
| Keamanan | **Positif.** Tahap 2 tidak lagi dapat mencabut hak dalam jumlah yang tidak ditinjau; kedua Tahap 1 `INSERT` tidak lagi gagal saat dijalankan |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE`. Tidak ada endpoint yang dibuat, diubah, atau dihapus.

---

## 5. Verifikasi

### 5.1 Build dan verifier

| Perintah | Hasil |
| --- | --- |
| `dotnet build -c Release --no-incremental` | **0 Error(s)**, 189 warning dokumentasi XML yang sudah ada sebelumnya |
| `tools/authorization-verifier/verify-authorization.sh` | **PASS** |
| `git diff --check` | Bersih, exit 0 |

| Metrik | Diharapkan | Terukur |
| --- | --- | --- |
| Actions | 1.300 | 1.300 ✅ |
| Resources | 340 | 340 ✅ |
| Modules | 48 | 48 ✅ |
| Metadata gaps | 0 | 0 ✅ |
| Fallback | 69 persis | 69 cocok persis ✅ |
| `BE-SEC-003` | 24/24 | 24/24 ✅ |
| Naked baru | 0 | 0 ✅ |
| Naked baseline | 0 | 0 ✅ |
| Identitas kanonik ganda | 0 | tidak ganda ✅ |

Nol perubahan source membuat hasil verifier identik dengan `f209a376` — itu memang buktinya:
task ini tidak menyentuh perilaku otorisasi aplikasi.

### 5.2 Audit statik seluruh `INSERT SysAccessPolicy`

Dijalankan terhadap seluruh `Migrations/scripts/*.sql`:

```
[OK] be-sec-003b-policy-expansion-dbeaver.sql:565  cols=14 vals=14 missing=none
[OK] be-sec-003b-policy-expansion-dbeaver.sql:586  cols=14 vals=14 missing=none
[OK] be-sec-003b-policy-expansion.sql:547          cols=14 vals=14 missing=none
[OK] be-sec-003b-policy-expansion.sql:568          cols=14 vals=14 missing=none
[OK] be-sec-014-post-seeder-hr-initial-grants.sql:301  cols=14 vals=14 missing=none

RESULT: ALL SysAccessPolicy INSERTS POPULATE EVERY NOT NULL COLUMN
```

Lima `INSERT`, seluruhnya 14 kolom dengan 14 nilai, tanpa satu pun kolom `NOT NULL` yang hilang.

### 5.3 Parity psql ↔ DBeaver

Dibuktikan secara mekanis: kedua berkas dinormalisasi (komentar, baris kosong, dan meta-command
dibuang; ekspresi operator disatukan menjadi satu token), lalu dibandingkan baris demi baris.

| Dimensi semantik | psql | DBeaver | Hasil |
| --- | --- | --- | --- |
| Peta pemecahan lama → baru | 23 baris | 23 baris | **Identik** |
| Identitas wajib | 24 baris | 24 baris | **Identik**, dan keduanya cocok persis dengan `required-identities.txt` |
| Penurunan pemegang historis (`be_sec_003b_pemegang`) | flag policy saja | flag policy saja | **Identik** |
| Resolusi identitas (`be_sec_003b_identitas`) | tanpa syarat action aktif | tanpa syarat action aktif | **Identik** |
| Sasaran Tahap 1 (`be_sec_003b_target`) | — | — | **Identik** |
| Aturan `PatientAssessment.Amend` | sumber = pemegang `Complete`, sesudah 4.1 | sama | **Identik** |
| Kolom `NOT NULL` pada `INSERT` | 14 | 14 | **Identik** |
| Perlindungan kunci alami | `NOT EXISTS` 4 kolom | `NOT EXISTS` 4 kolom | **Identik** |
| Gerbang prasyarat 24 identitas | ada | ada | **Identik** |
| Verifikasi Tahap 1 (4.2b) | `sisa_peta = 0`, `Amend = Complete` | sama | **Identik** |
| Sasaran Tahap 2 | `PatientProcedure.Update`, `DoctorQueue.Update` | sama | **Identik** |
| Gerbang kardinalitas Tahap 2 | 4 = 1 + 3 | 4 = 1 + 3 | **Identik** |
| Sasaran rollback 6.2 | 24 identitas | 24 identitas | **Identik** |
| Akhiran default tahap tulis | `ROLLBACK` | `ROLLBACK` | **Identik** |

Perbedaan yang tersisa **hanya** cangkang eksekusi, dan seluruhnya terklasifikasi:

| Perbedaan | Sifat |
| --- | --- |
| Urutan `DROP VIEW` dipindah ke atas dan dibalik menurut ketergantungan | Mekanis. Himpunan view yang di-*drop* identik (5 view). Membuat varian DBeaver dapat dijalankan ulang pada sesi yang sama tanpa galat dependency |
| Tiga blok `set_config` + gerbang operator (Tahap 1, Tahap 2, rollback) | Tambahan keamanan yang diminta pemilik: UUID operator wajib bukan nol-GUID dan wajib ada di `AspNetUsers` |

Tidak ada perbedaan semantik bisnis. Varian DBeaver bebas dari meta-command psql dan interpolasi
variabel psql — nol *backslash*, nol interpolasi bergaya `:'variabel'`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Skema `UpdateBy`/`DeleteBy`/`CancelBy` diverifikasi ulang dari source | Terpenuhi | Bagian 3.1 — tiga sumber independen |
| 2. Konvensi baris baru dikonfirmasi dari aplikasi, bukan dari laporan | Terpenuhi | `RoleAccessController.cs` |
| 3. Seluruh `INSERT SysAccessPolicy` diaudit, bukan hanya yang sudah diketahui | Terpenuhi | Bagian 5.2 — audit menyeluruh direktori skrip |
| 4. Kolom/nilai `INSERT` sama jumlahnya | Terpenuhi | 14 = 14 pada kelima `INSERT` |
| 5. `CreateBy` tetap operator terverifikasi | Terpenuhi | Bagian 3.3 |
| 6. Tahap 2 menegaskan tepat 4 (1 + 3) sebelum menulis | Terpenuhi | Bagian 3.4, langkah 4.3b |
| 7. Sasaran Tahap 2 dibekukan supaya yang ditegaskan = yang diubah | Terpenuhi | Langkah 4.3a |
| 8. Tahap 2 menegaskan hasil sesudah menulis | Terpenuhi | Langkah 4.3d |
| 9. Tidak ada policy di luar sasaran yang berubah | Terpenuhi | Langkah 4.3d, deteksi `UpdateDateTime = now()` |
| 10. Varian DBeaver resmi dibuat | Terpenuhi | `be-sec-003b-policy-expansion-dbeaver.sql` |
| 11. Varian DBeaver bebas meta-command dan interpolasi psql | Terpenuhi | Bagian 5.3 |
| 12. Validasi operator pada varian DBeaver | Terpenuhi | Nol-GUID ditolak; keberadaan di `AspNetUsers` ditegaskan; 3 gerbang |
| 13. Tidak ada GUID Departemen/Posisi/Controller/Action/policy yang di-*hardcode* | Terpenuhi | Satu-satunya GUID literal adalah nol-GUID: placeholder operator dan `Guid.Empty` |
| 14. Parity semantik dibuktikan, bukan diklaim | Terpenuhi | Bagian 5.3 — diff ternormalisasi |
| 15. Baseline source kanonik dicatat | Terpenuhi | `1.300 / 340 / 48` pada kepala kedua varian |
| 16. Build `Release` dan verifier dijalankan ulang | Terpenuhi | Bagian 5.1 |
| 17. Tidak ada eksekusi database, seeder, atau aplikasi | Terpenuhi | Bagian 5 |

**Butir Definition of Done yang belum terpenuhi — disebut apa adanya:**

| Butir | Keadaan |
| --- | --- |
| Kedua varian diuji terhadap database | **Belum.** Keduanya rancangan yang belum pernah dijalankan; sifat itu ditulis di kepala tiap berkas |
| Kardinalitas Tahap 2 = 4 diukur pada database | **Belum.** Angka 4 = 1 + 3 berasal dari kontrak pemilik. Gerbang 4.3b membatalkan transaksi bila database berkata lain — tetapi yang membuktikannya tetap bagian 1.5, dan bagian 1.5 belum pernah dijalankan |
| Penerapan `BE-SEC-003B` | **Belum.** Tahap 1 dan Tahap 2 tetap `PAUSED` |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Build memunculkan 189 warning dokumentasi XML yang sudah ada sebelumnya |
| Masalah yang diketahui | Varian psql mem-*drop* view sementara dalam urutan maju. Dijalankan dua kali pada sesi psql yang sama, `DROP VIEW be_sec_003b_identitas` akan gagal karena `be_sec_003b_pemegang` dan `be_sec_003b_target` bergantung padanya. Varian DBeaver sudah memakai urutan terbalik; varian psql sengaja tidak diubah pada task ini karena berada di luar scope yang disetujui |
| Risiko tersisa | **1.** Angka 4 pada gerbang Tahap 2 berasal dari kontrak pemilik dan belum diukur pada database; bila keadaan sebenarnya berbeda, Tahap 2 akan membatalkan diri — itu perilaku yang diinginkan, tetapi operator perlu tahu sebelum jendela pemeliharaan dibuka. **2.** View sementara bersifat per-sesi; memutus koneksi DBeaver di tengah urutan membuat Tahap 2 kehilangan sasarannya |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Langkah berikutnya | **1.** Operator menjalankan bagian 0–3 varian DBeaver dan menyerahkan keluaran 1.0b serta 1.5 kepada pemilik. **2.** Pemilik mengonfirmasi 24/24 dan tepat 4 baris menggantung. **3.** Baru Tahap 1, verifikasi bagian 5, lalu Tahap 2. **4.** Commit/push menunggu instruksi terpisah |
