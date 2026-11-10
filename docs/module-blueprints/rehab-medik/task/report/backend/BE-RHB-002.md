# Laporan Perubahan Backend — `BE-RHB-002`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RHB-002` |
| Judul | Migration skema Rehab |
| Slice | S-A — Fondasi (`MVP-0`) |
| Roadmap | `docs/module-blueprints/rehab-medik/roadmap/backend-roadmap.md` r2 — kartu `BE-RHB-002` |
| Trace | RHB-DEC-015, RHB-DEC-022 (amandemen: agent boleh `migrations add`), `02-backend-architecture.md` §7, `data/data-dictionary.md` §5 |
| Contract version | Set kontrak `v1`, `approved` 2026-11-05 |
| Dependency | `BE-RHB-001` ✅ |
| Klasifikasi | `MEDIUM` — satu migration, hanya `CREATE TABLE` |
| Task mode | `BACKEND` |
| Target tulis | `Migrations/` + DB dev `QuilvianNewDevSukma` (izin owner 2026-11-05) |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `6bb3fba7` + working tree `BE-RHB-001`/`003` |
| Tanggal | 2026-11-05 |
| Status | **Selesai** |

## 1. Masalah yang diselesaikan

Model `Rhb*` dari `BE-RHB-001` belum punya tabel di basis data, sehingga belum ada satu pun
endpoint Rehab yang bisa menyimpan data. Task ini membuat migration-nya dan menerapkannya ke
DB dev pribadi owner.

## 2. Proses yang dijalankan

1. `ef migrations add AddRehabilitationManagement` (lewat `ef.dll`, tanpa koneksi DB) terhadap
   assembly Debug hasil `BE-RHB-001`. Terbentuk `20261105045613_AddRehabilitationManagement.cs`,
   Designer-nya, dan pembaruan `ApplicationDbContextModelSnapshot.cs`.
2. Isi migration diperiksa terhadap kamus data (§5).
3. Build ulang dengan perintah owner, lalu `migrations has-pending-model-changes`.
4. **Pengaman DB.** Nama database dibaca dari `appsettings.Development.json` tanpa mencetak
   connection string, dengan hasil `QuilvianNewDevSukma`. `migrations list` menunjukkan **8
   migration modul lain** juga pending (Rawat Inap, Purchasing/AP, Medical Fee, Billing, Nurse
   View, Optik). Karena itu `database update` **tidak** dipakai, dan owner memilih skrip khusus
   Rehab (keputusan 2026-11-05).
5. `ef migrations script 20261103050001_AddOpticalManagement 20261105045613_AddRehabilitationManagement --idempotent`
   menghasilkan 1.211 baris: 20 `CREATE TABLE` `Rhb*`, satu `INSERT` riwayat, dibungkus satu
   transaksi, dan setiap blok dijaga `IF NOT EXISTS` riwayat.
6. Skrip dijalankan lewat eksekutor sementara Npgsql di scratchpad sesi. Eksekutor menolak
   berjalan bila database ≠ `QuilvianNewDevSukma` dan tidak mencetak connection string.
7. Verifikasi read-only langsung ke katalog Postgres dan ulang `migrations list`.

## 3. Perubahan yang dikerjakan

| Berkas | Perubahan |
| --- | --- |
| `Migrations/20261105045613_AddRehabilitationManagement.cs` | Baru. `Up`: 20 `CreateTable` + 77 `CreateIndex`. `Down`: 20 `DropTable`. |
| `Migrations/20261105045613_AddRehabilitationManagement.Designer.cs` | Baru (hasil EF) |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Diperbarui oleh EF: blok 20 entity `Rhb*`. 198 baris "terhapus" terbukti hanya pengurutan ulang (semuanya muncul kembali sebagai tambahan). |

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` |
| Database | 20 tabel `public."Rhb…"` **diterapkan ke `QuilvianNewDevSukma`** (2026-11-05). Tidak ke DB lain. Migration modul lain tidak tersentuh. |
| Keamanan/Auth | `NOT APPLICABLE` |

## 4. Dokumentasi endpoint

`NOT APPLICABLE`

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Isi `Up` migration | Hanya `CreateTable` (20, semuanya `Rhb*`) dan `CreateIndex` (77). Tanpa `AddColumn`/`AlterColumn`/`DropColumn`/`Sql`/`InsertData` atas tabel lain. | `PASS` | grep operasi |
| Isi `Down` migration | 20 `DropTable` saja | `PASS` | grep |
| Constraint kunci di migration | `CK_RhbRequest_Source`, `CK_RhbMeasurement_Owner`, `CK_RhbSessionIntervention_Quantity/Duration`, `UX_RhbAssessment_MedicalFinal`, `UX_RhbProgram_ActivePerDiscipline`, `UX_RhbProgram_Version`, `jsonb`, `NullsDistinct=false` | `PASS` | grep |
| Perilaku hapus | 53 FK `Restrict`, 0 `Cascade` | `PASS` | grep |
| Snapshot | 198 baris terhapus, 0 yang tidak muncul kembali | `PASS` | Perbandingan multiset baris diff |
| `dotnet build` (perintah owner) setelah migration ada | `Build succeeded.` 0 error, 426 warning legacy, 0 warning Rehab, 4m16s | `PASS` | Log build |
| `migrations has-pending-model-changes` | "No changes have been made to the model since the last migration." exit 0 | `PASS` | Log |
| Eksekusi skrip ke DB dev | "Terhubung ke database: QuilvianNewDevSukma", "Skrip selesai dieksekusi." exit 0 | `PASS` | Keluaran eksekutor |
| Katalog Postgres | 20 tabel `Rhb%`; 1 baris `__EFMigrationsHistory` Rehab; 3 unique index kunci; 4 check constraint; 53 FK dari tabel `Rhb*`; `RhbRequest` 0 baris | `PASS` | Query read-only |
| `migrations list` sesudah apply | `AddRehabilitationManagement` tidak pending; 8 migration modul lain tetap pending, sama seperti sebelum apply | `PASS` | Keluaran EF |

AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) `Up` hanya `CREATE TABLE`/index untuk 20 tabel `Rhb*` | Terpenuhi | §5 |
| (2) `Down` menghapus 20 tabel tanpa menyentuh tabel lain | Terpenuhi | §5 |
| (3) `has-pending-model-changes` tanpa perubahan tertunda | Terpenuhi | §5 |
| (4) Diterapkan ke DB dev pemilik (`QuilvianNewDevSukma`) | Terpenuhi, oleh agent atas izin owner lewat skrip khusus Rehab | §2, §5 |
| DoD: laporan | Terpenuhi | Berkas ini |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Riwayat migration di `QuilvianNewDevSukma` kini tidak berurutan: Rehab sudah diterapkan, 8 migration lebih lama masih pending. EF tetap menerapkan yang pending pada `database update` berikutnya. Bila ada migration lain yang dibangun di atas snapshot yang belum memuat Rehab, periksa ulang `has-pending-model-changes`. |
| Masalah yang diketahui | — |
| Risiko tersisa | Penerapan ke DB lain (shared/UAT/produksi) belum dan tetap wewenang terpisah |
| Perubahan sampingan | Eksekutor sementara `rhbdbtool` dan skrip `rhb-migration.sql` hanya ada di scratchpad sesi, tidak di repository |
| Interupsi | `NONE` |
| Status Git | `M Migrations/ApplicationDbContextModelSnapshot.cs`, `?? Migrations/20261105045613_AddRehabilitationManagement*.cs`, ditambah perubahan task sebelumnya |
| Langkah berikutnya | `BE-RHB-004`, `BE-RHB-005` (paralel), lalu `BE-RHB-006` |
