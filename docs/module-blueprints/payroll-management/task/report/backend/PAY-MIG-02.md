# Laporan Perubahan Backend — `PAY-MIG-02`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-MIG-02` |
| Judul | Migration `RenameTable` 19 tabel Payroll ke awalan `Prl*`, Designer, dan snapshot manual |
| Slice | Payroll mandiri — schema |
| Roadmap | Tidak ada roadmap Payroll terdaftar; register status di `PAYROLL_V2_04_KONTRAK_DAN_TASK_IMPLEMENTASI.md` bagian 2 |
| Trace | `PAYROLL_V2_01` bagian 7.3; rencana `PAYROLL_V2_06` bagian 6 dan 7; permintaan eksplisit pemilik 11 November 2026 ("kerjakan fase 2 backend") |
| Contract version | Tidak ada perubahan kontrak API |
| Dependency | `PAY-BE-12` ([laporan](PAY-BE-12.md)) — source sudah memakai nama `Prl*`; `PAY-MIG-01` (tiga indeks unik yang ikut diganti namanya) |
| Klasifikasi | `HEAVY` — mengganti nama 19 tabel produksi beserta ratusan constraint dan indeks; tanpa kemampuan menjalankan database |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` branch `Yasmina`; laporan ini di `docs/module-blueprints/payroll-management/task/report/backend/` |
| Model | Claude Sonnet 5.5 |
| Commit backend saat dikerjakan | `f2dd20a6` ditambah perubahan lokal yang belum di-commit |
| Tanggal | 11 November 2026 |
| Status | 🟡 **Tiga berkas ditulis dan lolos simulasi statis; belum dijalankan terhadap database.** Eksekusi, pra-cek, dan pasca-cek oleh pemilik pada database uji. **Setelah merge integrasi (bagian 9):** snapshot dan Designer dibangun ulang dari snapshot integrasi, simulasi diulang pada 10.786 nama |

### Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `Corporate` / `PayrollManagement` |
| Pemilik/prefix registry | `Prl`, `ACTIVE` (baris ditambahkan oleh `PAY-BE-12`) |
| Keberlakuan | `LEGACY MIGRATION` |
| QBE ID yang berlaku | `QBE-NAM-003` (normalisasi `Trx*`/`Wfp*` harus mencakup tabel fisik), `QBE-DB-001`, `QBE-DB-002` (tanpa `DROP` + `CREATE`), `QBE-MOD-002` |
| Wewenang | Pembuatan berkas migration, Designer, dan snapshot **diberikan** oleh instruksi eksplisit pemilik 11 November 2026 (`CLAUDE.md` backend mensyaratkan instruksi eksplisit). Eksekusi migration, `dotnet ef`, SQL, dan deployment **tidak diberikan** |
| Migration / database | **Tidak dijalankan.** Tidak ada koneksi database, tidak ada `dotnet ef`, tidak ada SQL yang dieksekusi agen |

## 1. Masalah yang diperbaiki

Source sudah memakai nama `Prl*` (`PAY-BE-12`), tetapi database masih memuat tabel `Trx*` dan `Wfp*`. Tanpa pasangan fisik ini, aplikasi baru dan database lama saling meleset: setiap kueri ke `PrlPayrollRun` dan kawan-kawannya akan gagal.

## 2. Proses bisnis

Migration mengganti nama 19 tabel, seluruh constraint, dan seluruh indeks yang namanya memuat nama tabel itu. **Isi tabel tidak disentuh:** run, slip, pembayaran, bukti, dan profil pajak/iuran/tunjangan pegawai tetap utuh. Tidak ada kolom yang berubah dan tidak ada `DROP` maupun `CREATE`.

**Contoh.** Tabel `TrxPayrollRunEmployee` menjadi `PrlPayrollRunEmployee`. Constraint kunci asing `FK_TrxPayrollAdjustment_TrxPayrollRunEmployee_PayrollRunEmployeeId` pada tabel penyesuaian menjadi `FK_PrlPayrollAdjustment_PrlPayrollRunEmployee_PayrollRunEmployeeId` (dua nama tabel dalam satu nama constraint diganti sekaligus). Indeks `UX_TrxPayrollRun_RegularPeriod` menjadi `UX_PrlPayrollRun_RegularPeriod`.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas

| Berkas | Perubahan |
| --- | --- |
| `Migrations/20261111100000_RenamePayrollTrxAndWfpTablesToPrlPrefix.cs` (baru, 185 baris, BOM, LF) | `Up` dan `Down` memanggil satu blok `DO $qbe$ ... $qbe$` dengan peta 19 pasangan (maju dan balik) |
| `Migrations/20261111100000_RenamePayrollTrxAndWfpTablesToPrlPrefix.Designer.cs` (baru, 145.283 baris) | Model penuh, diturunkan dari snapshot yang sudah diperbarui; hanya header yang berbeda (bagian 3.3) |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | 100 penggantian nama dan namespace (`PAY-BE-12`); BOM dan 145.280 baris utuh |

Timestamp `20261111100000` lebih baru dari migration terakhir (`20261110090000_AddFinPurchasingBlmStaging`). Berkas migration lama tidak ditulis ulang. `MigrationMetadata.g.cs` dan `Migrations/History/` dibangkitkan tool dan tidak disentuh.

### 3.2 Rancangan blok SQL

Mengikuti pola `20260831000000_RenameMedicalRecordTrxTablesToMrcPrefix` (nama dicari dari katalog Postgres, bukan diketik satu per satu), dengan empat perbedaan yang disengaja:

| Langkah | Isi |
| --- | --- |
| 0. Pemeriksaan tabrakan | Bila tabel lama **dan** tabel baru sama-sama ada untuk satu pasangan, migration berhenti dengan pesan jelas (`RAISE EXCEPTION`) sebelum mengubah apa pun. Ini menandakan migration setengah jalan atau tabel dibuat manual |
| 1. Nama tabel | `ALTER TABLE public.<lama> RENAME TO <baru>` hanya bila tabel lama ada (aman diulang) |
| 2. Constraint | Setiap constraint di skema `public` yang namanya memuat token lama diganti lewat `ALTER TABLE ... RENAME CONSTRAINT`. Mencakup PK, unique, FK (termasuk FK milik tabel lain yang menunjuk ke sini), dan CHECK. Mengganti nama PK atau unique sekaligus mengganti nama indeks penopangnya |
| 3. Indeks | Indeks yang tidak ditopang constraint (indeks biasa dan `UX_*`) diganti lewat `ALTER INDEX ... RENAME` |

**Perbedaan dari pola Rekam Medis:**

1. **Pencocokan memakai batas token, bukan substring (`LIKE`).** Token dikenali bila didahului awal nama atau `_`, dan diikuti `_`, `~`, atau akhir nama: `(^|_)<token>(?=_|~|$)`. Alasannya arah `Down`: `PrlPayroll` (dulu `WfpPayroll`) adalah awalan `PrlPayrollRun` (dulu `TrxPayrollRun`). Dengan substring bebas, `Down` akan salah mengubah `PrlPayrollRun` menjadi `WfpPayrollRun`. Pencocokan memakai *lookahead* supaya FK ke tabel yang sama (satu token dua kali pada satu nama) terganti seluruhnya.
2. **Tidak ada langkah "rapikan nama terpotong".** Awalan lama dan baru sama-sama tiga huruf, jadi panjang nama tidak berubah dan titik potong `~` pada nama yang sudah terpotong tetap jatuh di tempat yang sama. Migration Rekam Medis memerlukannya karena salah satu penggantinya memendek 13 huruf.
3. **Ada pemeriksaan tabrakan** (langkah 0) yang tidak ada pada pola lama.
4. **Rencana `PAYROLL_V2_06` bagian 6 menyebut pra-cek `LIKE`.** Pencocokan token membuat pra-cek itu tidak lagi diperlukan untuk menjaga milik tabel lain; sebagai gantinya ada pra-cek tabrakan (bagian 6 di bawah).

**Yang sengaja tidak berubah:** nama kolom, tipe, urutan kolom, isi data, nama properti navigasi, dan nama constraint atau indeks yang tidak memuat nama tabel Payroll. Tabel milik modul lain tidak berganti nama; hanya constraint FK mereka yang menunjuk tabel Payroll ikut diganti.

**Tidak ada objek tersembunyi yang bergantung pada nama tabel.** Telaah seluruh migration lama (kedua blok SQL mentah pada migration awal dibaca sampai penutupnya): tidak ada `CREATE FUNCTION`, `TRIGGER`, `VIEW`, atau indeks parsial berbasis SQL mentah yang menyebut 19 tabel ini. SQL mentah yang menyebut nama tabel Payroll hanya dua, dan keduanya riwayat yang berjalan **sebelum** migration ini pada urutan timestamp, saat tabelnya masih bernama lama, sehingga tetap valid bila riwayat dimainkan ulang dari awal pada database baru: (a) satu `UPDATE public."WfpTransportAllowanceTransaction" SET "TransactionNumber" = ...` (pengisian data satu kali) pada `20260726161839_initializeBigModulHRD2`, dan (b) beberapa `ALTER ... RENAME CONSTRAINT` pada `20260822154039_RenameAttendancePersistenceToHrd`. Source aplikasi tidak memuat string SQL yang menyebut tabel ini (pemeriksaan 28 string literal yang menyebut nama entity: tidak ada yang berupa SQL).

### 3.3 Designer dan snapshot

Designer EF pada dasarnya adalah salinan model setelah migration. Designer baru dibuat dari snapshot yang sudah diperbarui, dengan empat perubahan header (ditegaskan oleh `diff`): satu `using Microsoft.EntityFrameworkCore.Migrations;`, atribut `[Migration("20261111100000_RenamePayrollTrxAndWfpTablesToPrlPrefix")]`, nama kelas `RenamePayrollTrxAndWfpTablesToPrlPrefix` tanpa kelas dasar, dan `/// <inheritdoc />` dengan `BuildTargetModel` menggantikan `BuildModel`. Isi model identik dengan snapshot.

> **Catatan ketidakselarasan yang sudah ada sebelumnya (bukan akibat pekerjaan ini).** Designer `20261110090000_AddFinPurchasingBlmStaging` **bukan** salinan snapshot dikurangi header: selisihnya sekitar 2.248 baris, terutama entity `Rhb*` (Rehabilitasi) yang ada di snapshot dan Designer `20261105045613_AddRehabilitationManagement` tetapi tidak ada di Designer `20261106025752` maupun `20261110090000`. Karena itu Designer baru **tidak** disalin dari Designer BLM, melainkan dari snapshot, dan memuat `Rhb*`. Bila pemilik melihat perubahan model tertunda terkait `Rhb*`, sumbernya ketidakselarasan lama itu.

Entity pada snapshot tetap berurutan menurut urutan lama (alfabet nama lengkap lama). Pembangkitan snapshot oleh EF berikutnya akan mengurutkan ulang blok `Prl*` ke posisi alfabetnya, sehingga migration berikutnya akan membawa diff pemindahan blok pada snapshot. Itu kosmetik dan tidak mengubah model.

## 4. Dokumentasi endpoint

Tidak menambah atau mengubah endpoint (`NOT APPLICABLE`).

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Header Designer baru dibandingkan snapshot (`diff`) | 3 bagian beda yang memuat empat perubahan (`using`, atribut `[Migration]` dan nama kelas, `<inheritdoc />` dan `BuildTargetModel`); seluruhnya di 19 baris pertama, tidak ada selisih di isi model | `PASS` | Keluaran `diff` |
| Daftar nama yang dihasilkan snapshot lama dan snapshot baru dihitung ulang (nama default EF: `PK_`, `IX_`, `FK_`, serta nama bernama seperti `UX_`, `CK_`), dipotong seperti Npgsql (62 karakter ditambah `~`) | 10.499 nama pada masing-masing snapshot | informasi | Skrip statis |
| **Simulasi algoritma migration** pada 10.499 nama yang tampak di database sebelum migration: terapkan penggantian token berurutan seperti pada blok SQL, bandingkan dengan nama yang diharapkan model baru | **10.499 dari 10.499 sama.** Nama terdampak 283 (67 di antaranya terpotong `~`) | `PASS` (simulasi; **bukan eksekusi PostgreSQL**) | Skrip statis |
| Simulasi `Up` lalu `Down` | 10.499 dari 10.499 kembali ke nama semula | `PASS` (simulasi) | Skrip statis |
| Nama ganda setelah `Up`; sisa token lama pada hasil `Up` | 0; 0 | `PASS` (simulasi) | Skrip statis |
| Token mirip yang bukan milik 19 tabel (substring berisiko) di seluruh nama pada snapshot | 0 | `PASS` | Skrip statis |
| Token terpotong di tengah oleh batas 63 karakter | 0. Satu nama yang tampak terpotong (`FK_TrxPayrollPayslip_TrxPayrollRunEmployee_...`) ternyata artefak pembaca pada FK satu-ke-satu; nama aslinya 62 karakter dan tidak terpotong | `PASS` | Telaah |
| Tabel lain yang berawalan sama dengan 19 tabel ini | Tidak ada | `PASS` | Skrip statis |
| SQL mentah pada migration lama atau source yang bergantung pada nama tabel | Tidak ada fungsi, trigger, view, atau indeks parsial yang menyebutnya. Dua SQL riwayat yang menyebut nama lama (`UPDATE` pada migration awal, `ALTER ... RENAME CONSTRAINT` pada `20260822154039`) berjalan sebelum migration ini dan tidak terpengaruh | `PASS` (telaah) | `grep`, pembacaan blok SQL |
| Bentuk C# berkas migration (raw string `$$"""`, tanda kutip, indentasi penutup) mengikuti migration Rekam Medis; `'\1'` ter-render utuh | Sesuai | `PASS` (telaah; **bukan kompilasi**) | Pembacaan berkas |
| Sintaks SQL pada PostgreSQL | Tidak dijalankan | `NOT RUN` | — |
| `dotnet build`, `dotnet ef migrations list`, `dotnet ef migrations script`, `has-pending-model-changes` | Tidak dijalankan (aturan pemilik) | `NOT RUN` | — |
| Penerapan pada database uji, uji `Down` dan `Up` ulang | Belum | `NOT RUN` | — |
| Tes otomatis backend | Tidak ada project tes otomatis di backend; tidak dibuat | `NOT APPLICABLE` | `rules/backend/TEST_POLICY.md` |

AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)

**Batas bukti.** Simulasi memakai mesin ekspresi reguler Python dengan konstruksi yang sama (awal atau `_`, token, *lookahead* `_`, `~`, atau akhir). Ia membuktikan **logika penggantian nama** terhadap nama-nama yang dihasilkan model; ia tidak membuktikan bahwa PostgreSQL menerima setiap pernyataan, bahwa nama di database nyata sama dengan nama yang dihitung dari model (objek buatan tangan atau migration lama yang berbeda), maupun bahwa C# terkompilasi. Karena itu pra-cek dan pasca-cek di bagian 6 wajib dijalankan.

## 6. Skrip untuk pemilik (belum dijalankan oleh agen)

Seluruh pernyataan di bawah **belum diuji pada PostgreSQL oleh agen**. Jalankan pada **database uji**, dalam satu sesi (tabel sementara `peta` hilang saat sesi berakhir). Skrip 1 dan 2 hanya membaca, kecuali tabel dan tampilan sementara.

### 6.1 Sebelum apa pun

1. Cadangan penuh database uji.
2. Jalankan `dotnet ef migrations list` (atau padanannya) dan catat daftar migration yang **tertunda**. `database update` menerapkan **semua** migration tertunda secara berurutan, termasuk `20261104071700_AddPayrollIntegrityConstraints` (`PAY-MIG-01`) bila belum diterapkan, dan `20261110090000_AddFinPurchasingBlmStaging`. Pastikan tidak ada yang tidak Anda sangka.
3. `PAY-MIG-01` menambah tiga indeks unik parsial. Bila belum diterapkan, jalankan SQL pra-cek duplikasi pada `PAYROLL_V2_05` bagian 5 lebih dulu. Migration ini sendiri tidak peduli urutannya (ia mencari nama dari katalog), tetapi `database update` akan membuat tiga indeks itu dengan nama lama lalu langsung menggantinya.

### 6.2 Daftar pasangan dan tampilan bantu (dipakai semua skrip di bawah)

```sql
CREATE TEMP TABLE peta (lama text, baru text);
INSERT INTO peta VALUES
  ('TrxMedicalServiceFeeCalculation', 'PrlMedicalServiceFeeCalculation'),
  ('TrxMedicalServiceFeePayment', 'PrlMedicalServiceFeePayment'),
  ('TrxPayrollAdjustment', 'PrlPayrollAdjustment'),
  ('TrxPayrollApproval', 'PrlPayrollApproval'),
  ('TrxPayrollAttendanceInput', 'PrlPayrollAttendanceInput'),
  ('TrxPayrollEmployeeComponent', 'PrlPayrollEmployeeComponent'),
  ('TrxPayrollOvertimeInput', 'PrlPayrollOvertimeInput'),
  ('TrxPayrollPayment', 'PrlPayrollPayment'),
  ('TrxPayrollPayslip', 'PrlPayrollPayslip'),
  ('TrxPayrollReversal', 'PrlPayrollReversal'),
  ('TrxPayrollRun', 'PrlPayrollRun'),
  ('TrxPayrollRunEmployee', 'PrlPayrollRunEmployee'),
  ('TrxPayrollVariableInput', 'PrlPayrollVariableInput'),
  ('WfpInsurance', 'PrlInsurance'),
  ('WfpPayroll', 'PrlPayroll'),
  ('WfpTax', 'PrlTax'),
  ('WfpTransportAllowance', 'PrlTransportAllowance'),
  ('WfpTransportAllowancePolicy', 'PrlTransportAllowancePolicy'),
  ('WfpTransportAllowanceTransaction', 'PrlTransportAllowanceTransaction');

CREATE TEMP VIEW objek_bernama AS
SELECT 'constraint'::text AS jenis, t.relname::text AS tabel, c.conname::text AS nama
FROM pg_constraint c
JOIN pg_class t ON t.oid = c.conrelid
JOIN pg_namespace n ON n.oid = t.relnamespace
WHERE n.nspname = 'public'
UNION ALL
SELECT 'index', t.relname::text, i.relname::text
FROM pg_index x
JOIN pg_class i ON i.oid = x.indexrelid
JOIN pg_class t ON t.oid = x.indrelid
JOIN pg_namespace n ON n.oid = i.relnamespace
WHERE n.nspname = 'public';
```

### 6.3 Pra-cek (sebelum migration)

```sql
-- 1a. Harus: tabel_lama_ada = 19 dan tabel_baru_ada = 0.
SELECT
  count(*) FILTER (WHERE to_regclass(format('public.%I', lama)) IS NOT NULL) AS tabel_lama_ada,
  count(*) FILTER (WHERE to_regclass(format('public.%I', baru)) IS NOT NULL) AS tabel_baru_ada
FROM peta;

-- 1b. Jumlah baris tiap tabel. SIMPAN hasilnya untuk dibandingkan dengan 2b.
SELECT lama AS tabel_lama, baru AS tabel_baru,
       (xpath('/row/c/text()', query_to_xml(
          format('SELECT count(*) AS c FROM public.%I',
                 CASE WHEN to_regclass(format('public.%I', lama)) IS NOT NULL THEN lama ELSE baru END),
          false, true, '')))[1]::text::bigint AS baris
FROM peta ORDER BY lama;

-- 1c. Banyaknya nama constraint dan indeks yang akan diganti.
--     Perkiraan dari snapshot model: 283 nama unik. Selisih kecil wajar (objek buatan tangan);
--     selisih besar berarti berhenti dan periksa dulu.
SELECT count(DISTINCT o.nama) AS nama_akan_diganti
FROM objek_bernama o
WHERE EXISTS (SELECT 1 FROM peta p WHERE o.nama ~ ('(^|_)' || p.lama || '(_|~|$)'));

-- 1d. Tidak boleh ada objek yang sudah memakai nama baru. Harus 0 baris.
SELECT o.jenis, o.tabel, o.nama
FROM objek_bernama o
WHERE EXISTS (SELECT 1 FROM peta p WHERE o.nama ~ ('(^|_)' || p.baru || '(_|~|$)'));
```

### 6.4 Terapkan

Terapkan migration pada database uji dengan perintah yang biasa dipakai tim (`dotnet ef database update` atau skrip idempoten dari `dotnet ef migrations script --idempotent`; skrip di `Migrations/scripts/` dibuat pemilik). Bila gagal, seluruh migration dibatalkan karena Npgsql menjalankannya dalam transaksi; catat pesan galatnya apa adanya.

### 6.5 Pasca-cek (setelah migration, sesi yang sama atau ulangi 6.2)

```sql
-- 2a. Harus: tabel_lama_ada = 0 dan tabel_baru_ada = 19.
SELECT
  count(*) FILTER (WHERE to_regclass(format('public.%I', lama)) IS NOT NULL) AS tabel_lama_ada,
  count(*) FILTER (WHERE to_regclass(format('public.%I', baru)) IS NOT NULL) AS tabel_baru_ada
FROM peta;

-- 2b. Jumlah baris tiap tabel: harus SAMA dengan 1b untuk setiap baris.
--     (Pernyataan yang sama dengan 1b; tabel yang dibaca otomatis tabel baru.)
SELECT lama AS tabel_lama, baru AS tabel_baru,
       (xpath('/row/c/text()', query_to_xml(
          format('SELECT count(*) AS c FROM public.%I',
                 CASE WHEN to_regclass(format('public.%I', lama)) IS NOT NULL THEN lama ELSE baru END),
          false, true, '')))[1]::text::bigint AS baris
FROM peta ORDER BY lama;

-- 2c. Tidak ada objek yang masih bernama lama. Harus 0 baris.
SELECT o.jenis, o.tabel, o.nama
FROM objek_bernama o
WHERE EXISTS (SELECT 1 FROM peta p WHERE o.nama ~ ('(^|_)' || p.lama || '(_|~|$)'));

-- 2d. Banyaknya nama bernama baru. Harus SAMA dengan 1c.
SELECT count(DISTINCT o.nama) AS nama_bernama_baru
FROM objek_bernama o
WHERE EXISTS (SELECT 1 FROM peta p WHERE o.nama ~ ('(^|_)' || p.baru || '(_|~|$)'));

-- 2e. Kunci asing tetap ada dan valid. fk_tidak_valid harus sama dengan sebelum migration
--     (idealnya 0); fk_menunjuk_tabel_payroll harus sama dengan sebelum migration.
SELECT count(*) FILTER (WHERE NOT c.convalidated) AS fk_tidak_valid,
       count(*) FILTER (WHERE t.relname IN (SELECT lama FROM peta UNION SELECT baru FROM peta)) AS fk_menunjuk_tabel_payroll
FROM pg_constraint c
JOIN pg_class t ON t.oid = c.confrelid
WHERE c.contype = 'f';

-- 2f. Riwayat migration. Baris teratas harus 20261111100000_RenamePayrollTrxAndWfpTablesToPrlPrefix.
--     Sesuaikan skema bila riwayat migration tidak berada di public.
SELECT "MigrationId" FROM public."__EFMigrationsHistory" ORDER BY "MigrationId" DESC LIMIT 3;
```

Jalankan `2e` juga **sebelum** migration (pernyataan yang sama) dan catat angkanya, karena keduanya dibandingkan.

### 6.6 Uji `Down` lalu `Up` ulang

1. Kembalikan ke migration sebelumnya: `dotnet ef database update 20261110090000_AddFinPurchasingBlmStaging`.
2. Ulangi **6.3** (1a, 1b, 1c, 1d): hasilnya harus sama dengan sebelum migration (1a: 19 dan 0; 1b sama; 1c sama; 1d kosong).
3. Terapkan lagi dan ulangi **6.5**: seluruhnya harus cocok seperti pada percobaan pertama.

### 6.7 Uji aplikasi

Daftar uji manual ada pada [laporan `PAY-BE-12`](PAY-BE-12.md) bagian 7, butir 5. Uji **sesudah** pasca-cek lulus, dan hanya dengan aplikasi hasil `dotnet build` dari working tree yang memuat `PAY-BE-12`.

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Migration non-destruktif: seluruhnya `RENAME`, tanpa `DROP` dan `CREATE` (`QBE-DB-002`) | Terpenuhi (source) | Bagian 3.2 |
| 19 tabel, seluruh constraint (PK, unique, FK, CHECK), dan seluruh indeks bernama diganti | Terpenuhi (simulasi) | Simulasi 10.499 dari 10.499 |
| `Down` mengembalikan semuanya tanpa salah petakan `Prl*` ke `Trx*`/`Wfp*` | Terpenuhi (simulasi) | Simulasi `Up` lalu `Down` |
| Tidak ada nama yang bertambah panjang sampai terpotong | Terpenuhi | Awalan tiga huruf diganti tiga huruf |
| Berhenti bila ada tabrakan nama tabel | Terpenuhi (source) | Langkah 0 |
| Designer dan snapshot konsisten dengan model | Terpenuhi (statis) | `diff` Designer vs snapshot: hanya header |
| Migration lama tidak diubah | Terpenuhi | Tidak ada berkas lama yang disentuh |
| Tidak dijalankan terhadap database | Sesuai pembatasan | Bagian 5 |
| DoD: laporan tracked dan register | Terpenuhi | Berkas ini dan `PAYROLL_V2_04` |
| DoD: `dotnet build` | **Belum** | Menunggu pemilik |
| DoD: diterapkan, pasca-cek, dan `Down`/`Up` pada database uji | **Belum** | Menunggu pemilik (bagian 6) |

Status akhir **🟡**.

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Kecocokan dengan keluaran `dotnet ef migrations add` sungguhan belum dibuktikan (Designer dan snapshot ditulis manual, dengan cara yang sama seperti migration lain di repository ini). Jalankan `has-pending-model-changes` |
| Risiko tersisa | (1) Pernyataan SQL belum dijalankan pada PostgreSQL. (2) Aplikasi lama gagal total terhadap database baru dan sebaliknya: rilis satu paket, database dulu. (3) Objek database buatan tangan yang namanya memuat token lama akan ikut diganti namanya; 1c memperlihatkan jumlahnya sebelum migration |
| Masalah yang diketahui | Ketidakselarasan `Rhb*` pada Designer sebelumnya (bagian 3.3) |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Dua berkas migration baru berstatus `??`; snapshot berstatus `M`. Tidak ada stage atau commit |
| Langkah berikutnya | Pemilik: `dotnet build`, `has-pending-model-changes`, lalu 6.1 sampai 6.6 pada database uji |

## 9. Pembaruan setelah merge integrasi (11 November 2026)

Konteks: kegagalan CI QBE strict memperlihatkan bahwa `HEAD` belum memuat 39 commit `QuilvianIntegrationBackend`; penyebab dan perbaikannya ada di [`PAY-BE-12`](PAY-BE-12.md) bagian 8. Bagian ini mencatat dampaknya pada migration.

| Hal | Isi |
| --- | --- |
| Migration baru dari integrasi sejak titik cabang `f2dd20a6` | Termasuk `20261105150000_AddPayrollV2CoreEntitiesAndIndices` dan `20261106153000_AddRestitutionAndPayrollDeductionSliceB6Core` (pengembang lain). Semuanya bertimestamp **lebih lama** dari `20261111100000`, sehingga `database update` menerapkannya lebih dulu, lalu migration rename ini |
| Keterkaitan dengan 19 tabel | Dua migration itu membuat tabel baru yang memiliki **kunci asing ke `TrxPayrollRun` dan `TrxPayrollPayslip`** (mis. `FK_TrxPayrollVarianceAlert_TrxPayrollRun_PayrollRunId`, `FK_TrxPayrollRestitution_TrxPayrollPayslip_AffectedPayslipId`). Constraint itu ikut diganti namanya oleh blok SQL karena pencarian dilakukan dari katalog PostgreSQL dengan pencocokan token. Tidak ada perubahan pada migration |
| Berkas migration `20261111100000_...cs` | **Tidak berubah** |
| Snapshot | Basis diganti ke snapshot integrasi (perubahan Yasmina pada snapshot terbukti murni rename), lalu diganti nama: **nama lengkap 19 entity saja**, bukan seluruh namespace HR, karena entity Payroll Modern milik integrasi tetap bernama lengkap di namespace `...HumanResource.PayrollManagement.Models`. 0 nama lama tersisa; 106 kemunculan `Prl*`; 152.815 baris |
| Designer | Dibangun ulang dari snapshot hasil: 7 baris berbeda (header saja). Memuat `Rhb*` seperti sebelumnya |
| Simulasi nama migration, diulang | **10.786 nama** pada masing-masing snapshot (sebelumnya 10.499, tambahan dari entity integrasi). Setelah `Up`: **10.786 dari 10.786 sama** dengan nama yang diharapkan model baru. `Up` lalu `Down`: 10.786 dari 10.786 kembali. Nama terdampak **289** (sebelumnya 283; +6 FK entity integrasi), 67 di antaranya terpotong `~`. Tidak ada nama ganda, tidak ada sisa token lama |
| Pra-cek 1c | Perkiraan nama yang akan diganti kini **289** (bukan 283). Selisih kecil terhadap database wajar |
| Urutan penerapan di database uji | Pastikan `dotnet ef migrations list` menunjukkan seluruh migration integrasi tertunda; semuanya diterapkan berurutan. Karena migration integrasi menambah banyak tabel dan kolom, jalankan 6.1 sampai 6.6 pada salinan database yang sudah dicadangkan |
| Status | 🟡 sama: belum dijalankan terhadap database |
