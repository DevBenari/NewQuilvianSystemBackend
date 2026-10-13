# Laporan Perubahan Backend — `BE-CRM-002`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-CRM-002` |
| Judul | Data lama dan index unik |
| Slice | `MVP-0`, `EPIC CRM-01` — Fondasi data |
| Roadmap | `docs/module-blueprints/crm-membership/roadmap/backend-roadmap.md` § `BE-CRM-002` |
| Trace | `FR-CRM-003`, `FR-CRM-004`; `CRM-DEC-021`; `INV-01` s.d. `INV-03`; `02-backend-architecture.md` §7 langkah 2–3; `erd/patient-membership.md` |
| Contract version | Kamus data r1, state `v1`, `approved` (Yoga Aji Pratama, 2026-10-13) |
| Dependency | `BE-CRM-001` — selesai (source), migration belum diterapkan |
| Klasifikasi | `MEDIUM`: satu migration berisi SQL data idempoten + empat index unik bersyarat |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `Repositories/Configurations/`, `Migrations/`, dan laporan ini |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `761bfe9d` (branch `yoga`), di atas perubahan `BE-CRM-001` yang belum di-commit |
| Tanggal | 2026-10-13 |
| Status | Selesai. Migration diterapkan ke `QuilvianNewDevYoga` pada 2026-10-13 setelah dua uji kering (lihat §8) |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `PatientManagement` (`MstPatientMembership`, `MstPatient`, `PatMembershipStatusHistory`) |
| Registry | `PatientManagement / Membership / Patient Membership` — `Pat` — `ACTIVE`; `Master / Reference / MasterData` — `Mst` — `ACTIVE` |
| Keberlakuan | `TOUCHED LEGACY`: konfigurasi `MstPatientMembership`, data `MstPatientMembership` dan `MstPatient` |
| QBE yang berlaku | `QBE-CODE-004` (keunikan dijaga index database), `QBE-DEL-001` (tidak ada baris dihapus; data lama ditandai), `QBE-AUD-001` (jejak di riwayat, bukan log) |
| Tidak berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-CODE-001`/`002` (tidak ada service, endpoint, atau nomor bisnis) |

## 1. Masalah yang diperbaiki

Membership yang sudah tersimpan dibuat dengan aturan lama: status dan tanggal diketik bebas,
tanpa bukti iuran lunas. Bila dibiarkan Aktif, pasien itu tetap mendapat perlakuan member
tanpa dasar. Selain itu, sistem belum mencegah satu pasien memiliki dua membership Aktif
sekaligus, karena index lama hanya menjaga penanda `IsPrimary`.

Contoh: pasien X (samaran) punya Gold "Aktif" yang dibuat langsung tanpa pembayaran. Setelah
migration ini, Gold itu berstatus **Perlu Verifikasi**, pasien X terbaca Reguler, dan petugas
CRM nanti memutuskan mengakui atau menolaknya (`BE-CRM-013`).

## 2. Proses bisnis

1. **Pemicu:** migration `AddPatientMembershipStatusUniqueIndexes` diterapkan (dengan izin terpisah).
2. Semua membership yang belum dihapus dan belum bertanda `LegacyImport` diproses:
   1. `RowVersion` yang masih nol diisi nilai acak baru;
   2. proyeksi di pasien pemiliknya dikosongkan (`IsMember = false`, tingkat dan penunjuk membership kosong);
   3. satu baris riwayat `LEGACY_FLAGGED` sumber `System` dicatat, dengan status lama dan nilai `IsActive`/`IsPrimary` lama di kolom alasan;
   4. status menjadi Perlu Verifikasi (9), jenis `LegacyImport` (4), `IsActive`/`IsPrimary` = false.
3. Index unik `IsPrimary` lama dihapus, lalu empat index unik baru dibuat.
4. Semua langkah berjalan dalam **satu transaksi**. Bila satu langkah gagal, tidak ada yang tersimpan.

| Dari status | Tindakan | Ke status | Siapa | Syarat |
| --- | --- | --- | --- | --- |
| Status lama apa pun (Active, Expired, Cancelled, …) | Migration diterapkan | `NeedsVerification` (9) | Sistem | Belum dihapus dan belum `LegacyImport` |

**Jalur tidak normal:**

- Migration dijalankan ulang: tidak ada baris yang cocok lagi, sehingga riwayat tidak bertambah.
- Migration dibatalkan (`Down`): hanya baris yang **masih** Perlu Verifikasi yang dipulihkan
  ke status dan penanda lamanya. Baris yang sudah diakui atau ditolak CRM tidak dibalik,
  karena itu keputusan manusia. Riwayat `LEGACY_FLAGGED` milik baris yang dipulihkan dihapus,
  proyeksi pasien dihitung ulang dengan aturan lama, lalu index lama dibuat kembali.

**Hasil akhir:** tidak ada pasien yang terbaca member dari data lama, dan basis data menolak
membership Aktif, Menunggu Pembayaran, atau Terjadwal kedua untuk pasien yang sama.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Konfigurasi `MstPatientMembershipConfiguration.cs` (index lama), migration `BE-CRM-001`, model
`PatMembershipStatusHistory`, `IdentityModel` (kolom audit wajib), logika lama
`PatientMembershipController.RecalculatePatientMembershipReferenceAsync` (untuk `Down`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Repositories/Configurations/HealthServices/MstPatientMembershipConfiguration.cs` | Unik `(PatientId, IsPrimary)` lama diganti tiga unik bersyarat per status dan satu unik `FeeChargeReferenceId` |
| `Migrations/20261013052442_AddPatientMembershipStatusUniqueIndexes.cs` + `.Designer.cs` | Baru. Dibuat `dotnet ef migrations add`, lalu disisipi empat SQL data di `Up` dan tiga SQL pemulihan di `Down` |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Diperbarui otomatis (definisi index) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` |
| Database | Mengubah data `MstPatientMembership` (semua baris lama yang belum dihapus), `MstPatient` (hanya pemilik membership lama), menambah baris `PatMembershipStatusHistory`. Mengganti satu index unik dengan empat. **Belum diterapkan** |
| Keamanan/Auth | `NOT APPLICABLE`. Alasan riwayat hanya memuat teks sistem, tanpa data pribadi pasien |

## 4. Dokumentasi endpoint

`NOT APPLICABLE`: task ini tidak menyentuh endpoint.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -p:RunAnalyzers=False` (dua kali: sebelum dan sesudah SQL disisipkan) | 0 error, ±47 detik | `PASS` | Log build scratchpad |
| `dotnet ef migrations add AddPatientMembershipStatusUniqueIndexes` | Hanya operasi index yang diharapkan | `PASS` | Isi migration sebelum disisipi SQL |
| Skrip SQL offline `Up` (`ExtendPatientMembershipLifecycle` → migration ini) | Urutan: RowVersion → proyeksi → riwayat → tandai → drop index lama → empat index unik, satu transaksi | `PASS` | `be-crm-002-up.sql` |
| Skrip SQL offline `Down` | Urutan: drop empat index → pulihkan baris → hapus riwayat → hitung ulang proyeksi → index lama | `PASS` | `be-crm-002-down.sql` |
| QBE checker `ReportOnly`, 4 berkas | 0 VIOLATION, 0 REVIEW | `PASS` | Keluaran checker |
| `AT-27` / `UAT-01` (migration atas data lama) | — | `NOT RUN` | Butuh menerapkan migration ke DB lokal. Tidak ada izin eksekusi DB |
| `UAT-02` (sisipan Menunggu kedua ditolak) | — | `NOT RUN` | Idem |
| Idempotensi (jalankan dua kali) | Dibuktikan lewat pembacaan SQL: semua langkah memfilter `RegistrationType <> 4`, yang sudah diubah langkah 4 | `NOT RUN` di DB | Butuh DB |

Uji manual: `NOT FEASIBLE` tanpa izin database.

**Tidak dijalankan:** seluruh uji berbasis data (`AT-27`, `UAT-01`, `UAT-02`, uji idempotensi,
uji `Down`). Semuanya membutuhkan penerapan migration ke database lokal dengan izin terpisah.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Menjalankan migration dua kali tidak menggandakan riwayat | Terpenuhi | Uji kering 2: SQL data diulang, riwayat tetap 2 (§8) |
| (2) Setelah migration, `INSERT` kedua berstatus 7 untuk pasien sama ditolak | Terpenuhi | Sesudah `COMMIT`: `23505 UX_MstPatientMembership_Patient_Pending` (juga Aktif dan Terjadwal) |
| (3) `Down` memulihkan status lama | Terpenuhi | Uji kering 2: md5 membership dan proyeksi sama dengan semai |
| DoD: kriteria 1–3 terbukti dengan bukti kueri | Terpenuhi | §8 |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | **Delta dari rencana:** roadmap menempatkan SQL data di migration `ExtendPatientMembershipLifecycle`. SQL itu ditaruh di migration ini (sebelum index) agar migration `BE-CRM-001` dan laporannya tidak berubah. Urutan efektifnya sama |
| Masalah yang diketahui | (a) Membership lama yang statusnya sudah Expired/Cancelled ikut menjadi Perlu Verifikasi, sesuai `CRM-DEC-021` ("semua tanpa bukti bayar"). CRM perlu menolaknya satu per satu. (b) Banyak pasien punya `IsMember = true` dari nilai bawaan lama walaupun tidak pernah punya membership. Migration ini hanya membersihkan pemilik membership lama (sesuai desain). Pasien lain dibersihkan oleh sinkronisasi proyeksi `BE-CRM-010`/`BE-CRM-020` |
| Risiko tersisa | Sampai `BE-CRM-011`/`BE-CRM-015` selesai, endpoint lama masih bisa membuat membership "Active". Membuat Active kedua untuk pasien yang sama kini ditolak database dan tampil sebagai galat 500, bukan 409 yang ramah |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `M` 7 berkas source (`BE-CRM-001` + konfigurasi), `??` empat berkas migration, folder baru, laporan. Belum di-commit |
| Langkah berikutnya | `BE-CRM-003`, `BE-CRM-023`, `BE-CRM-010` |

## 8. Penerapan ke database (2026-10-13)

Atas izin owner ("langkah nomor 1"), kedua migration CRM diterapkan ke `QuilvianNewDevYoga` lewat skrip per migration dan klien `pg`, tanpa `database update`.

| Langkah | Hasil |
| --- | --- |
| `migrations list` sebelum | Tertunda hanya `20261013051652_ExtendPatientMembershipLifecycle` dan `20261013052442_AddPatientMembershipStatusUniqueIndexes`. Tidak ada migration modul lain |
| Keadaan data | `MstPatientMembership` **kosong (0 baris)**. 14 pasien bertanda `IsMember = true` tanpa membership apa pun |
| Uji kering 1 (data nyata, `ROLLBACK`) | Naik dua migration → empat index unik ada, index lama hilang. Uji duplikat Menunggu/Aktif/Terjadwal ditolak `23505`. Turun dua migration → sidik md5 sama dengan awal |
| Uji kering 2 (tiga membership samaran disemai di dalam transaksi, `ROLLBACK`) | Aktif dan Expired menjadi Perlu Verifikasi (`LegacyImport`, `IsActive`/`IsPrimary` false). Yang terhapus tidak disentuh. Dua riwayat `LEGACY_FLAGGED` sumber `System` dengan nilai lama di alasan. Proyeksi pasien A kosong. Data SQL diulang: riwayat tetap 2. `Down` memulihkan membership dan proyeksi persis seperti semai (md5 sama) |
| Penerapan (`COMMIT`) | Berhasil |
| Verifikasi sesudah | Tabel riwayat dan kolom `AnnualFeeAmount` ada, dua baris `__EFMigrationsHistory`, empat index `UX_*`, `IX_..._IsPrimary` hilang. Uji duplikat (transaksi + `ROLLBACK`) ditolak `23505`. `migrations list`: **0 tertunda** |
| Startup backend / `/health` | `NOT RUN`: tidak ada perubahan perilaku runtime pada task ini |
