# Bukti Verifikasi Desain — Modul Accounting

| Field | Value |
|---|---|
| Blueprint ID | `ACC-BP-001` · Revision `3` |
| Tanggal verifikasi | 1 September 2026 |
| Backend SHA | `aa837d784ff51cb2b889cf975ada3a204018f1f5` (branch `rizkiG`) |
| Frontend SHA | `fc49cc7714baa9a2c37ed6519fbaba5dffcbda99` (branch `RizkiV2`) — baseline **saat verifikasi ini**; lihat catatan di bawah |
| Sifat | Read-only. Tidak ada source, migration, maupun database yang disentuh |

Berkas ini menyimpan **bukti** di balik klaim yang dipakai blueprint. Tujuannya supaya orang
berikutnya tidak perlu mempercayai begitu saja, dan dapat mengulang pemeriksaannya sendiri.

Setiap bagian memuat perintah yang dipakai, hasilnya, dan kesimpulannya.

> **Catatan baseline (1 September 2026).** Baseline frontend blueprint sudah di-rebase ke
> `31a82c8` (`QuilvianIntegrationFrontend`). Bukti di berkas ini dikumpulkan pada `fc49cc7` dan
> **tetap berlaku** — impact scan menunjukkan drift-nya tidak menyentuh wilayah Accounting.
> Rinciannya di [02-frontend-rebaseline-impact-scan.md](02-frontend-rebaseline-impact-scan.md).

---

## `EV-ACC-001` — Accounting benar-benar dimulai dari nol

**Klaim yang diuji:** tidak ada buku besar tandingan, sehingga `ACC-DEC-002` aman.

**Cara memeriksa:**

```bash
grep -rn --include=*.cs -E "class +(ChartOfAccount|Journal|JournalLine|GeneralLedger|AccountingPeriod|PostingRule)" .
grep -rn --include=*.cs -i "DbSet<" . | grep -iE "akun|account|journal|ledger|coa|finance"
```

**Hasil:** pencarian kelas tidak menemukan satu pun. Pencarian `DbSet` hanya menemukan dua yang
namanya mirip:

| Ditemukan | Milik modul | Kesimpulan |
|---|---|---|
| `BilDepositAccounts` | Billing | Titipan uang muka pasien, bukan buku besar |
| `WfpBankAccounts` | Workforce | Rekening bank pegawai, bukan buku besar |

**Kesimpulan:** `ACC-CAP-001` sampai `ACC-CAP-005` berstatus `MISSING`. Tidak ada risiko
membangun sesuatu yang sudah ada.

---

## `EV-ACC-002` — `MstCostCenter` sudah ada dan wajib dipakai ulang

**Klaim yang diuji:** Accounting **tidak** perlu membuat master unit biaya sendiri.

**Cara memeriksa:**

```bash
grep -oE 'DbSet<Mst[A-Za-z]*(Unit|Department|CostCenter|Organization)[A-Za-z]*>' \
  Repositories/ApplicationDbContext.cs | sort -u
grep -rl "class MstCostCenter" --include=*.cs Areas
```

**Hasil:** ditemukan `MstCostCenter`, `MstDepartment`, `MstOrganizationUnit`, `MstServiceUnit`,
dan `MstLegalEntity`. Berkas modelnya:

```
Areas/Corporate/HumanResource/MasterData/Organization/Models/MstCostCenter.cs
```

Isinya memuat `LegalEntityId` (wajib), `CostCenterCode`, `CostCenterName`, `IsActive`, dan —
yang menarik — kolom **`AccountingCode`** sepanjang 100 karakter yang boleh kosong.

**Kesimpulan:** `ACC-CAP-007` berstatus `READY TO REUSE`. Membuat tabel Cost Center milik
Accounting akan menjadi duplikasi, dan karena itu masuk daftar "Yang sengaja tidak dibuat".

**Pertanyaan lanjutan yang ditimbulkan temuan ini:** makna `MstCostCenter.AccountingCode` menjadi
kabur setelah Accounting resmi memiliki COA. Tidak memblokir MVP karena Accounting tidak
membacanya. Perlu diperjelas pemilik Human Resource.

---

## `EV-ACC-003` — Pemisahan per badan hukum adalah konvensi domain Corporate

**Klaim yang diuji:** dasar `ACC-DEC-037`.

**Cara memeriksa:**

```bash
grep -rlE 'public Guid\??\s+LegalEntityId' --include=*.cs Areas | wc -l
grep -rlE 'public Guid\??\s+LegalEntityId' --include=*.cs Areas | grep -i billing | wc -l
```

**Hasil:**

| Cakupan | Jumlah berkas |
|---|---:|
| Seluruh `Areas/` | 83 |
| Yang mengandung `billing` pada path-nya | 0 |

Hampir seluruh 83 berkas berada di bawah `Areas/Corporate/`, yaitu domain yang sama dengan
Accounting. Modul Billing tidak memakai pola ini sama sekali.

**Kesimpulan:** memisahkan pembukuan per badan hukum mengikuti konvensi yang sudah berlaku di
domain tempat Accounting berada, bukan pilihan yang dikarang. Diperkuat kenyataan bahwa
`MstCostCenter` — yang wajib dirujuk `ACC-DEC-019` — mensyaratkan `LegalEntityId`.

---

## `EV-ACC-004` — Arah kontrak Billing sudah terkunci ke AR/AP

**Klaim yang diuji:** dasar `ACC-DEP-003` dan alasan `ACC-OQ-005` opsi A praktis tertutup.

**Sumber:** `docs/module-blueprints/billing-kasir/contracts/integration-contract.md@aa837d7`,
`contract_version: BIL-INTEGRATION-0.4`, status **approved** 20 Agustus 2026.

**Isi yang relevan:**

| ID | Arah |
|---|---|
| `BIL-INT-007` | Billing → **AR** |
| `BIL-INT-008` | Billing → **AP** |
| `BIL-INT-009` | Billing → **AR/AP** (penyesuaian) |

**Kesimpulan:** akibat keuangan dari tagihan sudah diarahkan ke Piutang dan Utang, yaitu wilayah
Finance — bukan ke Accounting. `ACC-PRD-001` §36 aturan 13 melarang mengubah kontrak Billing yang
sudah disetujui. Karena itu `ACC-XM-001` harus diputuskan bersama owner Billing, dan tidak boleh
diputuskan sepihak oleh Accounting.

---

## `EV-ACC-005` — Prefix `Acc` belum terdaftar, dan belum dipakai siapa pun

**Klaim yang diuji:** dasar `ACC-DEP-002`.

**Cara memeriksa:**

```bash
git ls-tree -r --name-only origin/QuilvianIntegrationBackend | grep -i PREFIX_REGISTRY
git show origin/QuilvianIntegrationBackend:docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md
grep -rlE 'class Acc[A-Z]' --include=*.cs Areas Models
grep -oE 'ToTable\("Acc[A-Za-z]*"' Migrations/ApplicationDbContextModelSnapshot.cs
```

**Hasil:**

- Registry ditemukan di `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`, tetapi **hanya
  pada branch `origin/QuilvianIntegrationBackend`** — tidak ada di `rizkiG`. Inilah sebabnya
  dokumen normatif itu tidak terlihat saat bekerja di branch modul.
- Delapan belas prefix terdaftar: `Hrd`, `Wfp`, `Fin`, `Mst`, `Cli`, `Reg`, `Pat`, `Phm`, `Emg`,
  `Bil`, `Lab`, `Rad`, `Inp`, `Out`, `Ins`, `Wfl`, `Opr`, `Mrc`.
- **Tidak ada baris Accounting**, dan **`Acc` belum dipakai** prefix mana pun.
- Pencarian kelas `Acc*` dan tabel `Acc*` sama-sama kosong.

**Aturan yang mengikat:** QBE-MOD-002 — *"Modul yang memiliki entity operasional persisted MUST
punya entri registry berstatus APPROVED sebelum entity pertamanya dibuat. Bila entri itu tidak
ada, pembuatan entity operasional berstatus `BLOCKED`."*

**Kesimpulan:** `ACC-DEP-002` berstatus `MISSING` dan memblokir seluruh task yang menulis berkas
di `Models/`. Nama `Acc*` sepanjang blueprint bersifat sementara sampai barisnya disetujui.

**Catatan penting:** registry sudah memuat `Fin` = Finance berstatus `ACTIVE` dengan **nol
entity** dibuat. Accounting adalah bounded context terpisah menurut `ACC-DEC-001`, `ACC-DEC-002`,
dan `ACC-DEC-003`, sehingga memakai `Fin` akan mencampur dua kepemilikan yang berbeda owner-nya.
Preseden pemisahan serupa ada di registry itu sendiri: `Wfp` dipisahkan dari `Hrd` pada
28 Agustus 2026.

---

## `EV-ACC-006` — Snapshot model EF sudah pulih

**Klaim yang diuji:** apakah `ACC-DEP-001` masih memblokir.

**Latar:** pada 28 Agustus 2026 tercatat snapshot kehilangan puluhan blok definisi entitas milik
modul lain, sehingga pembuatan migration akan menerbitkan ratusan operasi milik Billing.

**Cara memeriksa:**

```bash
S=Migrations/ApplicationDbContextModelSnapshot.cs
for ref in HEAD origin/QuilvianIntegrationBackend origin/master; do
  git show $ref:$S | grep -c 'b\.ToTable('
  git show $ref:$S | grep -oE 'b\.ToTable\("Bil[A-Za-z]*"' | wc -l
done
```

**Hasil:**

| Ref | Blok `b.ToTable(` | Blok `Bil` |
|---|---:|---:|
| `HEAD` = `aa837d7` (branch `rizkiG`) | **530** | **28** |
| `origin/QuilvianIntegrationBackend` = `c081939` | **530** | **28** |
| `origin/master` = `a507a64` | 523 | 28 |

Snapshot pada branch kerja **identik** dengan integration. Perbaikannya masuk lewat dua
migration:

```
Migrations/20260828063909_RepairCanonicalEfModelBaseline.cs
Migrations/20260830151340_RepairPostCanonicalIntegration.cs
```

**Kesimpulan:** `ACC-DEP-001` berstatus **`RESOLVED`**. Ia **tidak lagi** memblokir apa pun.

**Yang tetap dijalankan meskipun begitu:** pemeriksaan hitung-operasi pada migration Accounting
pertama, sebagaimana tertulis di `02-backend-architecture.md` bagian 8. Kesamaan jumlah blok
adalah bukti kuat, tetapi bukti pasti hanya didapat saat migration benar-benar dihasilkan. Biaya
pemeriksaannya murah dan dampaknya besar bila terlewat.

---

## `EV-ACC-007` — Pola folder dan penempatan berkas

**Klaim yang diuji:** dasar bagian "Arsitektur folder" pada arsitektur backend.

**Cara memeriksa:**

```bash
ls Areas/Corporate/HumanResource/MasterData/Organization/
ls Areas/Corporate/HumanResource/LeaveManagement/
ls Repositories/Configurations/
find Repositories/Configurations -iname "*CostCenter*"
```

**Hasil:**

| Yang diperiksa | Hasil |
|---|---|
| Submodul master | Berisi `Controllers/`, `DTOs/`, `Models/` |
| Submodul transaksional | Berisi `Constants/`, `Controllers/`, `DTOs/`, `Models/`, `Services/` |
| Folder configuration | `Repositories/Configurations/{Corporate, Global, HealthServices}` |
| Contoh configuration nyata | `Repositories/Configurations/Corporate/HumanResource/MasterData/Organization/MstCostCenterConfiguration.cs` |

**Kesimpulan:** berkas configuration memang **tidak** berada di dalam `Areas/`, dan folder domain
Corporate memakai bentuk jamak `Corporate` yang konsisten di kedua tempat. Pola inilah yang
disalin Accounting.

---

## `EV-ACC-008` — Layanan platform yang dipakai ulang

**Cara memeriksa:**

```bash
find . -name "LoggerService.cs" -not -path "*/obj/*"
grep -c "AddScoped<" Program.cs
grep -c "public DbSet<" Repositories/ApplicationDbContext.cs
```

**Hasil:**

| Yang diperiksa | Hasil |
|---|---|
| Layanan pencatatan | `Services/Logging/LoggerService.cs` — ada |
| Jumlah registrasi service di `Program.cs` | 164 |
| Jumlah `DbSet` di `ApplicationDbContext` | 523 |

**Kesimpulan:** menambah empat baris `AddScoped` untuk service Accounting adalah hal biasa di
repository ini, bukan penyimpangan. Yang tetap dilarang adalah menambahkan seeder atau logika
startup ke `Program.cs`.

Accounting juga **tidak** membuat tabel jejak audit sendiri, karena `LoggerService` sudah ada dan
dipakai seluruh modul.

---

## `EV-ACC-009` — Komponen frontend yang dipakai ulang

**Cara memeriksa:**

```bash
ls src/components/features/base-features/
ls src/lib/state/slice/master-data-resource-slice-factory.jsx
find src -type d -iname "*account*" -o -iname "*finance*" -o -iname "*keuangan*"
```

**Hasil:**

| Yang diperiksa | Hasil |
|---|---|
| Komponen dasar | 26 berkas tersedia, termasuk `data-table.jsx`, `data-filter.jsx`, `confirm-modal.jsx`, `status-badge.jsx`, `summary-grid.jsx`, `access-denied-gate.jsx` |
| Factory slice master data | Ada |
| Folder akuntansi/keuangan | **Tidak ada** |

**Kesimpulan:** `ACC-CAP-005` berstatus `MISSING`, sedangkan `ACC-CAP-013` dan `ACC-CAP-014`
berstatus `READY TO REUSE`. Modul Accounting tidak perlu — dan tidak boleh — membuat komponen
tabel maupun penyaring sendiri.

---

## Ringkasan status prasyarat setelah verifikasi

| ID | Status sebelum | Status sesudah | Memblokir MVP? |
|---|---|---|:---:|
| `ACC-DEP-001` snapshot EF | `REPAIR` | **`RESOLVED`** | Tidak |
| `ACC-DEP-002` prefix entity | `MISSING` | `MISSING` | **Ya** |
| `ACC-DEP-003` kontrak Billing | `CONFLICT` | `CONFLICT` | Tidak, Phase 2 saja |
| `ACC-DEP-004` modul Finance | `MISSING` | `MISSING` | Tidak, Phase 2 saja |

## Cara mengulang seluruh pemeriksaan

Seluruh perintah di atas bersifat **read-only**. Tidak satu pun menulis berkas, menyentuh
database, atau menjalankan build. Aman dijalankan kapan saja.

Bila SHA salah satu repository berubah, ulangi `EV-ACC-001`, `EV-ACC-005`, dan `EV-ACC-006` lebih
dahulu — ketiganya yang paling menentukan apakah rencana masih berlaku.
