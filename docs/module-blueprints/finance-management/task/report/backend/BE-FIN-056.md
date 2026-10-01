# Laporan Perubahan Backend — `BE-FIN-056`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-056` |
| Judul | Skema piutang sewa non-pasien (Parkir dan Tenant) |
| Slice | `REV-13D` — `EPIC FIN-19` |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` bagian "AMENDMENT ROADMAP REVISI 13" |
| Trace | `FIN-DEC-099`..`104`; `FIN-DES-074` |
| Contract version | `erd/data-dictionary.md` AMENDMENT REVISI 13 (lanjutan) — `approved` 1 Oktober 2026 (lihat `blueprint-manifest.md` `status_note_revision_13`) |
| Dependency | Tidak ada — berdiri sendiri pada grafik dependency `REV-13`. **Menjadi prasyarat** `BE-FIN-057` |
| Klasifikasi | `MEDIUM` — satu repository (skor 0); 8 berkas (6 baru, 2 diubah) (skor 1); 2 entity baru + 1 migration yang **belum dibuat sebelumnya** (skor 2); nol endpoint pada task ini (skor 0); database — 2 tabel `Baru`, 1 migration (skor 2); keamanan/auth — nol resource/action pada task ini (skor 0); UI/workflow — nol, murni skema (skor 0). Total 5 → `MEDIUM` |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/FinanceManagement/Receivable/**`, `Repositories/**`, `Migrations/**`, dan laporan ini |
| Model | Claude Sonnet 5 |
| Commit backend saat dikerjakan | Belum di-commit — branch `Yasmina` |
| Tanggal | 1 Oktober 2026 |
| Status | ✅ **Selesai** 1 Oktober 2026 — source dan migration lengkap sesuai kontrak. `dotnet build` **PASS** dan migration **berhasil dieksekusi ke database**, keduanya dikonfirmasi pengguna |

---

## 1. Masalah yang diperbaiki

Tagihan sewa parkir dan sewa unit tenant belum punya tempat tersimpan sama sekali di sistem
governed. Dua kapabilitas ini sengaja **tidak** diperluas ke `FinReceivable` (piutang pasien) —
`FIN-DEC-101` menetapkan entity tersendiri karena `FinReceivable` mewajibkan setiap barisnya
berasal dari serah terima tagihan Billing (`SourceHandoffKey`/`InvoiceId` → `BilInvoice`), sesuatu
yang tidak pernah berlaku untuk sewa parkir/tenant yang dicatat manual oleh staf AR.

---

## 2. Proses bisnis

Task ini **murni skema** — tidak ada proses bisnis yang dijalankan pengguna di sini. Proses
pencatatan tagihan, pelunasan, penghapusan, dan pembatalan adalah cakupan `BE-FIN-057`
(dependency langsung pada task ini) yang akan memakai tabel-tabel yang dibangun di sini.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `docs/module-blueprints/finance-management/02-backend-architecture.md` AMENDMENT REVISI 13
  bagian K.1-K.8 (rancangan lengkap: class diagram, migration plan, data master)
- `docs/module-blueprints/finance-management/erd/data-dictionary.md` AMENDMENT REVISI 13
  (lanjutan) — bentuk DDL persis per kolom
- `Areas/Corporate/FinanceManagement/Receivable/Models/FinReceivableInvoiceBatch.cs`,
  `Models/FinReceiptAllocation.cs` — pola `IdentityModel`, `[Table(...)]`, static class konstanta
  status dipakai ulang persis
- `Repositories/Configurations/Corporate/FinanceManagement/Receivable/FinReceivableInvoiceBatchConfiguration.cs`
  — pola `IEntityTypeConfiguration<T>`, check constraint, index, kolom audit `IdentityModel`
  dipakai ulang persis
- `Repositories/ApplicationDbContext.cs` — dikonfirmasi `ApplyConfigurationsFromAssembly` dipakai
  (baris 1139-ish), sehingga dua configuration baru **otomatis teregistrasi** tanpa pendaftaran
  manual tambahan; namespace `Receivable.Models` sudah ter-`using` (baris 27)
- `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` — dikonfirmasi `FinanceManagement /
  Receivable / Piutang` sudah terdaftar prefix `Fin`, `ACTIVE`. Nol pendaftaran baru dibutuhkan
  (submodul yang sama dengan `FinReceivable`, `FinReceivableInvoiceBatch`, dkk.)
- `Migrations/ApplicationDbContextModelSnapshot.cs` — dibaca untuk menentukan posisi alfabetis
  penyisipan blok entity baru (`FinN...` sebelum `FinR...` pada namespace
  `Receivable.Models`), dan pola representasi tipe (`decimal?`/`DateOnly`/check constraint/index)
  dari entity tetangga pada file yang sama

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/.../Receivable/Models/FinNonPatientReceivable.cs` **(baru)** | Aggregate root — 14 properti (lihat §3.3), dua static class konstanta (`FinNonPatientReceivableCategories`, `FinNonPatientReceivableStatuses`) |
| `Areas/.../Receivable/Models/FinNonPatientReceivableSettlement.cs` **(baru)** | Entity anak — 7 properti, navigasi `NonPatientReceivable` |
| `Repositories/.../FinNonPatientReceivableConfiguration.cs` **(baru)** | Konfigurasi EF: dua check constraint (`Category`, `Status`), empat index, relasi `HasMany(Settlements)` dengan `DeleteBehavior.Restrict` |
| `Repositories/.../FinNonPatientReceivableSettlementConfiguration.cs` **(baru)** | Konfigurasi EF: satu index (`NonPatientReceivableId`) |
| `Repositories/ApplicationDbContext.cs` | Dua `DbSet` baru ditambahkan bersebelahan dengan `FinReceivableInvoiceBatch`/`Item` |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Dua blok entity baru disisipkan pada posisi alfabetis yang benar (sebelum `FinReceivable`), beserta blok relasi (`HasOne`/`Navigation`) pada kedua bagian lanjutan file yang sama |
| `Migrations/20261001100000_AddFinNonPatientReceivable.cs` **(baru)** | `Up()`: dua `CreateTable` (induk lalu anak, FK Restrict) + lima `CreateIndex`. `Down()`: `DropTable` urutan kebalikan (anak dulu) |
| `Migrations/20261001100000_AddFinNonPatientReceivable.Designer.cs` **(baru)** | Snapshot model lengkap pada titik migration ini — dibentuk dari `ApplicationDbContextModelSnapshot.cs` yang sudah diperbarui, diverifikasi `diff` byte-identik |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Nol.** Task ini tidak menyentuh controller/DTO/endpoint — itu cakupan `BE-FIN-057` |
| Database | **Dua tabel `Baru`**: `FinNonPatientReceivable` (14 kolom + 2 check constraint + 4 index) dan `FinNonPatientReceivableSettlement` (7 kolom + 1 FK Restrict + 1 index). Migration `20261001100000_AddFinNonPatientReceivable` dibuat; **belum dieksekusi** — wewenang terpisah |
| Keamanan/Auth | **Nol** pada task ini. Resource `FinanceNonPatientReceivable` beserta tiga action adalah cakupan `BE-FIN-057` |

**Metode pembuatan migration (atas permintaan eksplisit pengguna, tanpa `dotnet build`/`dotnet ef`),
mengikuti persis metode `BE-FIN-052`:**

1. Blok entity `FinNonPatientReceivable`/`FinNonPatientReceivableSettlement` disunting langsung ke
   `ApplicationDbContextModelSnapshot.cs` pada **tiga** lokasi berbeda dalam file yang sama (file
   ini punya tiga bagian: properti+index+table pada bagian pertama, relasi `HasOne`/`WithMany`
   pada bagian kedua untuk entity yang punya FK, dan `Navigation`-saja pada bagian ketiga untuk
   entity yang punya koleksi anak) — posisi alfabetis diverifikasi dengan `grep -n` sebelum dan
   sesudah penyisipan.
2. Berkas `.Designer.cs` migration dibentuk dengan menyalin isi `BuildModel`
   (`ApplicationDbContextModelSnapshot.cs` yang sudah diperbarui) ke `BuildTargetModel` migration
   baru, **diverifikasi `diff` baris-per-baris** — byte-identik, `exit code 0`, atas 125.146 baris
   yang dibandingkan.
3. Berkas migration utama (`Up`/`Down`) ditulis mengikuti pola `CreateTable`/`CreateIndex` yang
   sudah dipakai `AddArInvoiceBatchAndReceiptDeduction.cs` (migration terakhir yang membuat tabel
   baru beserta FK di repository ini) — **bukan** pola `AddColumn` seperti `BE-FIN-052`, karena
   task ini membuat tabel baru, bukan memperluas tabel lama.
4. UTF-8 BOM pada `.Designer.cs` disamakan dengan berkas Designer existing.

**Risiko metode ini yang MUST dicatat apa adanya:** karena **tidak ada satu pun** `dotnet build`
dijalankan untuk task ini (berbeda dari `BE-FIN-052` yang akhirnya dibuktikan build setelahnya),
**nol verifikasi compiler** atas model, configuration, `DbSet`, maupun migration. Verifikasinya
murni review manual dan `diff` tekstual. Risiko ini **ditanggung sadar** sesuai permintaan eksplisit
pengguna ("tanpa build automatis"), dan **MUST** ditutup dengan `dotnet build` sebelum migration
ini dieksekusi ke database atau sebelum `BE-FIN-057` mulai dikerjakan di atasnya.

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — task ini murni skema. Nol controller/endpoint disentuh.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | Berhasil tanpa error | `PASS` | Dikonfirmasi pengguna, 1 Oktober 2026 |
| Eksekusi migration ke database (`20261001100000_AddFinNonPatientReceivable`) | Berhasil | `PASS` | Dikonfirmasi pengguna, 1 Oktober 2026 |
| `diff` isi `BuildModel`/`BuildTargetModel` antara snapshot dan Designer.cs migration baru | Byte-identik | `PASS` | `diff` keluar kode 0 |
| Review diff/scope | 8 berkas, persis sesuai desain `02-backend-architecture.md` §K; nol berkas tidak terkait | `PASS` | `git status --short` Bagian 7 |
| Review skema terhadap kamus data | Setiap kolom, tipe, nullability, check constraint, dan index pada model+configuration+migration **dibandingkan satu per satu** dengan `erd/data-dictionary.md` AMENDMENT REVISI 13 (lanjutan) — cocok persis | `PASS` | Perbandingan manual baris demi baris |
| Review invariant — nol relasi ke FinReceivable/FinReceipt/BilInvoice | `FinNonPatientReceivable` dan `FinNonPatientReceivableSettlement` **tidak memiliki satu properti navigasi maupun FK pun** menuju entity piutang pasien, penerimaan, atau Billing | `PASS` | Baca penuh kedua model — nol referensi |
| QBE preflight | Area `Corporate/Finance`, Module `FinanceManagement`, Submodule `Receivable`, prefix `Fin` `ACTIVE` — sudah terdaftar, applicability `NEW CODE` (dua entity baru, submodul existing) | `PASS` | `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 15 |

Uji manual: `NOT FEASIBLE` — murni skema, nol endpoint untuk diuji pada task ini.

**AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis
(`rules/backend/TEST_POLICY.md`).**

**Tidak dijalankan:** uji manual/runtime (tidak relevan — nol endpoint pada task ini).

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Kedua tabel terbentuk beserta check constraint `Category` dan `Status` | Terpenuhi (source) | Model, configuration, migration ketiganya konsisten |
| **Nol** kolom rujukan ke `BilInvoice`, `FinReceivable`, atau `FinReceipt` | Terpenuhi | Lihat Bagian 5 |
| `FinReceivable` **tidak tersentuh sama sekali** | Terpenuhi | `git diff` atas `FinReceivable.cs` kosong |
| Build PASS | Terpenuhi | `dotnet build` PASS, dikonfirmasi pengguna 1 Oktober 2026 |
| Migration dibuat dan dieksekusi atas izin eksplisit | Terpenuhi | `Migrations/20261001100000_AddFinNonPatientReceivable.cs`/`.Designer.cs`; eksekusi dikonfirmasi pengguna 1 Oktober 2026 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Migration dibuat **manual** tanpa `dotnet ef`. `dotnet build` dan eksekusi migration ke database keduanya dikonfirmasi berhasil oleh pengguna 1 Oktober 2026, menutup risiko "nol verifikasi compiler" yang sempat tercatat |
| Masalah yang diketahui | Tidak ada temuan baru |
| Risiko tersisa | **Rendah.** Build dan eksekusi migration sudah dibuktikan berhasil; uji manual/runtime tidak relevan untuk task ini (nol endpoint) |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `git status --short` mencakup 8 berkas task ini (6 baru, 2 diubah — lihat Bagian 3.2) |
| Langkah berikutnya | Tidak ada — task ini selesai. `BE-FIN-057` (layanan dan 10 endpoint) sudah dikerjakan pada task berikutnya |
