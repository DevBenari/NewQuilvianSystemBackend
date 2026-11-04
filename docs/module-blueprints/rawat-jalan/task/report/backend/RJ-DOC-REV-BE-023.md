# Laporan Perubahan Backend — `RJ-DOC-REV-BE-023`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `RJ-DOC-REV-BE-023` |
| Judul | Seeder stok Farmasi RJ dan sinkron master (`RJ-DOC-DEC-091`, `092`) |
| Slice | Amendment SF — Katalog Resep dari stok Farmasi Rawat Jalan |
| Roadmap | `docs/module-blueprints/rawat-jalan/roadmap/doctor-consultation-roadmap.md` bagian 21.1 |
| Trace | `RJ-DOC-DEC-091`, `092`, `093` |
| Contract version | Tidak ada perubahan kontrak API |
| Dependency | `RJ-DOC-REV-BE-022` ✅ |
| Klasifikasi | `MEDIUM` — skor 6: satu repository (0), 9–20 berkas diperiksa (1), 4–8 berkas diubah (1), logika sedang (1), tanpa kontrak (0), persistence dan eksekusi DB (1), keamanan tidak berubah (0), workflow terbatas (1); dinaikkan karena eksekusi ke database bersama |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: seeder Farmasi, `Program.cs`, `appsettings.json`, csproj, dokumen blueprint `rawat-jalan`; eksekusi `QuilvianNewDevMaster` lewat harness scratchpad |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `f1b26bf7` (branch `sukmagp`; `RJ-DOC-REV-BE-022` sudah di-commit owner pada `77af1d24`) |
| Tanggal | 2026-10-29 (kode 28 Okt, eksekusi master 29 Okt) |
| Status | ✅ Selesai, belum di-commit |

**Backend Governance Preflight**

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `PharmacyManagement` (seeder baru); `MasterData` (`DrugStorageLocationSeeder`, dipanggil tanpa diubah) |
| Pemilik / prefix registry | `Phm`, `ACTIVE / LEGACY` |
| Keberlakuan | `NEW CODE` untuk `PharmacyOpeningStockSeeder`; `TOUCHED LEGACY` untuk `Program.cs`, `appsettings.json`, csproj |
| Entity baru | Tidak ada |
| QBE berlaku | QBE-VAL-001 (baris CSV divalidasi, kedaluwarsa bertentangan ditolak), QBE-TXN-001 (satu transaksi), QBE-LOG-001 (log ringkasan seeder dan aktor SuperAdmin pada setiap baris), QBE-AUD-001 (kartu stok terpisah dari log aplikasi) |

---

## 1. Masalah yang diperbaiki

Stok Farmasi Rawat Jalan hanya ada di `QuilvianNewDevSukma`. Akibatnya katalog Resep RJ berbasis stok (`BE-022`) tidak bisa dipakai di `QuilvianNewDevMaster`. Ada tiga masalah di master:

- Gudang `GDG-001`..`GDG-123` belum ada.
- Depo Rawat Jalan masih menunjuk lokasi "test".
- Lokasi "test" masih membawa data stok uji.

Bila `GDG-023` sekadar ditambahkan, lokasi "test" dan `GDG-023` sama-sama tertaut ke Rawat Jalan. Katalog lalu gagal dengan pesan "Konfigurasi Depo Farmasi ganda".

---

## 2. Proses bisnis

**Saat backend dijalankan (jalur rilis):**

1. `DrugStorageLocationSeeder` membuat 123 gudang bila belum ada. Seeder ini sudah ada dan flag-nya sudah aktif.
2. Bila `Seeders:RunPharmacyOpeningStockSeed=true`, `PharmacyOpeningStockSeeder` dijalankan dengan langkah berikut:
   1. Menolak berjalan di lingkungan `Production`.
   2. Menolak berjalan bila `GDG-023` belum ada.
   3. Menautkan `GDG-023` ke `SU-OPD-001` bila tautannya kosong. Tautan yang sudah diisi admin tidak ditimpa.
   4. Untuk setiap baris salinan (7.864 baris): mencari obat per kode, lalu membuat batch bila belum ada.
   5. Membuat saldo bila kombinasi batch, lokasi, dan status belum ada. Saldo yang jumlahnya lebih dari nol ditambah satu baris kartu stok `OpeningBalance` dengan jumlah yang sama.
   6. Saldo yang sudah ada dilewati, sehingga stok yang sudah bergerak tidak dikembalikan ke angka salinan.
   7. Semua perubahan disimpan dalam satu transaksi.

Contoh baris CSV `OBT00846,B290896949,2029-08-31,1,0` (ETOPUL) menjadi batch dan saldo `0` tanpa kartu stok. Contoh lain, `OBT01638,…,2,1.29` menjadi saldo Karantina `1,29` dengan satu kartu stok `+1,29`.

**Jalur tidak normal:**

| Keadaan | Akibat |
| --- | --- |
| Kode obat tidak ada di database tujuan | Baris dilewati dan dicatat sebagai kode tak dikenal |
| Nomor batch sama, kedaluwarsa berbeda | Baris dilewati; aturannya sama dengan `PHM032` |
| Lingkungan Production | Ditolak tanpa menulis apa pun |

**Eksekusi ke `QuilvianNewDevMaster` (`DEC-092`, `093`):** dijalankan lewat harness di scratchpad dalam satu transaksi.

1. Menghapus permanen seluruh data yang merujuk lokasi uji.
2. Menjalankan `DrugStorageLocationSeeder`.
3. Menjalankan `PharmacyOpeningStockSeeder`.
4. Commit, hanya bila hasil seeder bersih.

Aplikasi dan 23 worker latar tidak dijalankan, sehingga tidak ada proses lain yang menyentuh master.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`DrugStorageLocationSeeder.cs`, `LabSpecimenDetailTypeSeeder.cs` (pola CSV tertanam), `OperatingRoomDemoSeeder.cs` (pola penolakan Production), `Program.cs` (runner seeder, hosted service, tidak ada `Migrate` otomatis), `appsettings.json` (bagian `Seeders`), `QuilvianSystemBackend.csproj`, `DrugStockConfigurations.cs` (index unik batch, saldo, kartu stok), `PhmDrugStock.cs`, `PhmDrugBatch.cs`, `IdentityModel.cs`, `ApplicationDbContext.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/PharmacyManagement/Seeders/PharmacyOpeningStockSeeder.cs` | Seeder baru (bagian 2) beserta `PharmacyOpeningStockSeedResult` |
| `Areas/HealthServices/PharmacyManagement/Seeders/Data/pharmacy-opening-stock-frj.csv` | Salinan 7.864 saldo `GDG-023` Sukma per 28 Okt 2026 (kolom `DrugCode,BatchNumber,ExpiryDate,Status,QuantityOnHand`, 293 KB) |
| `QuilvianSystemBackend.csproj` | `EmbeddedResource` untuk CSV di atas |
| `Program.cs` | `SeedPharmacyOpeningStockAsync` dan flag `Seeders:RunPharmacyOpeningStockSeed`, dijalankan sesudah `DrugStorageLocationSeeder` |
| `appsettings.json` | `RunPharmacyOpeningStockSeed: false` beserta keterangan `_RunPharmacyOpeningStockSeed` |

Harness eksekusi (`SeedHarness`) berada di scratchpad sesi dan tidak masuk repository.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` |
| Database | Tidak ada perubahan schema atau migration. EF: "No changes have been made to the model since the last migration." Data `QuilvianNewDevMaster` berubah sesuai `DEC-092` (bagian 5) |
| Keamanan/Auth | `NOT APPLICABLE`. Credential master tidak dicetak: harness membaca `DefaultConnection` dan hanya mengganti nama database |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — task ini tidak menyentuh endpoint.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -c Release -p:UseSharedCompilation=false -o <scratchpad>/be-build` | `Build succeeded`, 0 Error, 417 warning lama; 0 warning compiler pada berkas baru | `PASS` | Keluaran perintah |
| `dotnet ef migrations has-pending-model-changes --configuration Release` | "No changes have been made to the model since the last migration." | `PASS` | Keluaran perintah |
| `Invoke-QbeConformanceCheck.ps1 -Mode Strict` (2 berkas `.cs`) | VIOLATION 0, REVIEW 0 | `PASS` | Keluaran perintah |
| `dotnet test` | Tidak ada project test (pola Bank Darah) | `NOT RUN` | — |
| Harness ke Sukma dan master | 7/7 skenario `PASS` (tabel di bawah) | `PASS` | Log harness di scratchpad |

| ID | Skenario | Hasil |
| --- | --- | --- |
| T1 | Sukma, transaksi dibatalkan: 7.864 baris CSV, 0 saldo baru, 7.864 dilewati, tautan tidak berubah, 0 kode tak dikenal | PASS |
| T2 | Sukma, transaksi dibatalkan: dua baris uji membuat 1 batch, 2 saldo (`12,5` Available dan `0` Karantina), 1 kartu stok `OpeningBalance` `0 → 12,5`. Kode `XXX-TIDAK-ADA` dicatat tak dikenal; batch sama dengan kedaluwarsa lain dicatat konflik. Ulang → 0 baru, 2 dilewati. Sesudah rollback batch uji tersisa 0 | PASS |
| T3 | Lingkungan `Production` → ditolak, 0 baris | PASS |
| M1 | Percobaan pertama ke master: `SaveChanges` timeout (> 30 detik) di `DrugStorageLocationSeeder`. Proses berhenti sebelum commit; query baca-saja memastikan master utuh dan tidak ada sesi tertinggal | `EXISTING / ENVIRONMENT ISSUE` (server DB lambat; koneksi pagi itu juga sempat terputus) |
| M2 | Percobaan kedua (batas perintah 600 detik), 4 menit 6 detik. Hapus: 1 alokasi, 1 item, 1 pemakaian, 3 reservasi, 8 riwayat, 4 item, dan 4 permintaan stok, 9 mutasi, 5 saldo, 5 batch, 1 lokasi uji. Lalu tambah 123 gudang, tautan `GDG-023`, 7.864 saldo, 7.862 batch, 7.862 kartu stok. `COMMIT` | PASS |
| M3 | Verifikasi baca-saja master terhadap Sukma: 123 lokasi identik di 23 kolom bisnis, 0 lokasi "test" | PASS |
| M4 | Saldo `GDG-023` master = Sukma per kode, batch, kedaluwarsa, status, dan jumlah (7.864 baris; total `2.488.007`). Reserved 0. 0 saldo yang tidak cocok dengan kartu stok. Satu-satunya lokasi tertaut unit layanan: `GDG-023 → SU-OPD-001` | PASS |

Uji manual: `NOT APPLICABLE` — tidak ada layar yang berubah.

**Tidak dijalankan:** seeder lewat startup aplikasi sungguhan dengan flag `true`. Runner startup memanggil method yang sama dengan harness, tetapi menyalakan backend ke master akan ikut menjalankan worker latar.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Dijalankan ke Sukma: 0 baris baru (idempoten), tautan tidak berubah | Terpenuhi | T1 |
| 2. Uji sisip dalam transaksi yang dibatalkan: batch, saldo, dan satu mutasi `OpeningBalance` yang jumlahnya sama dengan saldo | Terpenuhi | T2 |
| 3. Production → ditolak tanpa menulis | Terpenuhi | T3 |
| 4. Master sesudah eksekusi: tidak ada lokasi uji, 123 gudang dengan id sama dengan Sukma, saldo `GDG-023` sama dengan Sukma, total kartu stok = total saldo | Terpenuhi | M2, M3, M4 |
| 5. Build, EF tanpa perubahan model, QBE Strict | Terpenuhi | Tabel verifikasi |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Analyzer IDE melaporkan kompleksitas kognitif `SeedAsync` (25 > 15); dibiarkan karena alurnya satu lintasan dan setara seeder lain |
| Masalah yang diketahui | Di Sukma, saldo ETOPUL (`OBT00846`) 0 sementara kartu stoknya 70, akibat perubahan langsung di DB oleh owner. Di master, saldo itu dibuat sebagai 0 tanpa kartu stok, jadi master konsisten. Sukma belum dirapikan dan menunggu keputusan owner |
| Risiko tersisa | Satu reservasi aktif di master (`RX-20261028-00003`) ikut terhapus; resepnya tetap ada tanpa penahanan stok. CSV adalah snapshot 28 Okt; perubahan uji Sukma sesudahnya tidak ikut |
| Perubahan sampingan | `NONE` |
| Interupsi | Timeout percobaan pertama (M1) dibatalkan otomatis oleh server; percobaan kedua berhasil |
| Status Git | `M Program.cs`, `M QuilvianSystemBackend.csproj`, `M appsettings.json`, `?? Areas/HealthServices/PharmacyManagement/Seeders/PharmacyOpeningStockSeeder.cs`, `?? Areas/HealthServices/PharmacyManagement/Seeders/Data/`, `M`/`??` dokumen blueprint `rawat-jalan` (keputusan, roadmap, traceability, laporan ini) |
| Langkah berikutnya | Restart backend yang memakai master agar katalog Resep RJ berbasis stok aktif (butuh build `BE-022`); commit oleh owner |
