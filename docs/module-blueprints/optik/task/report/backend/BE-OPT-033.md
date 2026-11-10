# Laporan Perubahan Backend — `BE-OPT-033`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-OPT-033` |
| Judul | Jenis dokumen stok Optik dan jalur stok ke Farmasi |
| Slice | `MVP-2` — `EPIC OPT-04` |
| Roadmap | `roadmap/backend-roadmap.md` (`OPT-RM-BE-001` r2) — Rincian `BE-OPT-033` |
| Trace | `FR-OPT-042`; `OPT-DEC-004`, `019`, `038`, `060`; `INT-OPT-07`; `OPT-AC-008`; `OPT-VAL-049` |
| Contract version | `OPT-INT-v1` `INT-OPT-07`, `OPT-VAL-v1` `OPT-VAL-049` — `approved` sisi Optik |
| Penyetuju modul lain | Farmasi `OPT-APR-005` ✅ — Ikbal (`OPT-DEC-054`, `OPT-EVD-002`). Cara stok kembali `OPT-DEC-060` dipilih Yoga Aji Pratama 2026-11-04; **konfirmasi Ikbal belum tercatat** |
| Dependency | `BE-OPT-002` ✅ (model `OptOrderItem`), `BE-OPT-005` ✅ (pencari Depo Optik) |
| Klasifikasi | `MEDIUM` — satu konstanta Farmasi, satu service Optik baru, kode galat, DI |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/HealthServices/PharmacyManagement/Enums/DrugStockEnums.cs`, `Areas/HealthServices/OpticalManagement/Services/`, `Program.cs`, `Tests/QuilvianSystemBackend.OpticalTests/` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `d8fa6e40` (branch `yoga`), di atas perubahan `BE-OPT-003` dan `BE-OPT-030` yang belum di-commit |
| Tanggal | 2026-11-04 |
| Status | Selesai dan tervalidasi di harness. Belum di-commit |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `OpticalManagement` (gateway); `HealthServices` / `PharmacyManagement` (satu konstanta) |
| Registry | `Opt` / `ACTIVE`; `Phm` / `ACTIVE` |
| Keberlakuan | `NEW CODE` untuk `OptStockGateway`; `TOUCHED LEGACY` untuk `DrugStockEnums.cs`, `OptOperationResult.cs`, `Program.cs` |
| QBE yang berlaku | `QBE-SVC-001` (gateway lewat service Farmasi, tidak menulis saldo sendiri), `QBE-VAL-001`, `QBE-LOG-001`, `QBE-AUD-001` |
| Wewenang terpisah | Source ✅ · migration tidak diperlukan · eksekusi database ✘ · deployment ✘ |

---

## 1. Masalah yang diperbaiki

Optik belum punya jalur untuk memotong dan mengembalikan stok barang pesanan. Farmasi belum mengenal
dokumen sumber "pesanan Optik", dan cara stok kembali saat pesanan batal belum diputuskan (`OPT-OQ-015`).
Tanpa jalur ini `BE-OPT-034` (kirim ke pengerjaan) dan `BE-OPT-041` (batal sesudah pengerjaan) tidak bisa
dikerjakan.

---

## 2. Proses bisnis

**Keputusan baru `OPT-DEC-060`** (menutup `OPT-OQ-015`): stok yang sudah dipotong kembali ke Depo Optik
sebagai **Stok Masuk batch yang sama**, bukan koreksi mutasi asal dan bukan dokumen retur Farmasi.

1. **Potong stok.** **Contoh:** pesanan Ibu Sari (samaran) berisi frame FRM-A-HTM-52 sebanyak 1. Depo Optik punya batch B1 (kedaluwarsa Januari 2027, 1 buah) dan B2 (Januari 2028, 5 buah). Gateway meminta Farmasi mengeluarkan 1 buah; Farmasi memilih B1 (FEFO). Kartu stok mencatat Stok Keluar bertanda `OpticalOrder` dan nomor pesanan.
2. **Dipanggil ulang.** Petugas menekan tombol dua kali atau jaringan mengulang permintaan. Kunci `OPT:{pesanan}:{baris}:issue` sama, sehingga stok tetap berkurang sekali (`OPT-AC-008`).
3. **Pesanan batal sesudah dipotong.** Gateway mengembalikan seluruh potongan baris itu ke batch dan lokasi asalnya sebagai Stok Masuk. Kalau potongan dulu memakai dua batch, pengembaliannya juga ke dua batch yang sama.
4. **Lensa custom.** Lensa custom dipesan ke lab dan tidak pernah ada di depo, jadi tidak ada pergerakan stok.

**Jalur tidak normal:**

| Keadaan | Hasil |
| --- | --- |
| Stok kurang | `409 OPT-VAL-049` "Stok frame FRM-A-HTM-52 di depo Optik tidak cukup."; tidak ada pergerakan tercatat |
| Farmasi gagal menyimpan karena saldo berubah bersamaan | `409 OPT-CONCURRENCY`; potongan setengah jadi dibuang dari konteks, sehingga penyimpanan berikutnya oleh pemanggil tidak membawanya |
| Depo Optik belum didaftarkan atau tidak aktif | `409 OPT-DEPOT-MISSING` "Depo Optik belum didaftarkan atau tidak aktif. Hubungi bagian Farmasi." |
| Pengguna tanpa identitas | `403 OPT-STOCK-FORBIDDEN` dengan kalimat Farmasi |
| Farmasi menolak dengan sebab lain | `409 OPT-STOCK-REJECTED` dengan kalimat Farmasi |
| Basis data Farmasi tidak terjangkau | `503 OPT-STOCK-UNAVAILABLE` |
| Kembali untuk baris yang belum pernah dipotong | Tidak ada pergerakan (`Skipped`) |
| Kembali dipanggil dua kali, atau dua permintaan bersamaan | Stok kembali sekali; permintaan kedua menerima hasil yang sama (`IsReplay`) |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`DrugStockService` (`IssueAsync`, `ReceiveBatchAsync`, `CorrectMutationAsync`, `ApplyMutationAsync`, `PlanFefo`, `FindMutationsByKeyPrefixAsync`, `SaveAsync`, kelas galat), konfigurasi `PhmDrugStockMutation` (unique index `SourceDocumentType` + `CorrelationId`), `DrugReturnService`, `StockTransferService`, `OperatingRoomInventoryDispatchService`, `OptReferenceDataService.FindOpticalDepotIdAsync`, `OptOrderItem`, `OptLabels`, `contracts/validation-matrix.md` `OPT-VAL-049`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/PharmacyManagement/Enums/DrugStockEnums.cs` | **Diperbarui** (+7). `DrugStockSourceDocumentTypes.OpticalOrder` |
| `Areas/HealthServices/OpticalManagement/Services/OptStockGateway.cs` | **Baru.** `IssueAsync`, `ReturnAsync`, `IssueKey`, `ReturnKey`, `RequiresStock`; pemetaan galat Farmasi; pembersihan pergerakan tertunda; audit pengembalian. Tidak membuka transaksi |
| `Areas/HealthServices/OpticalManagement/Services/OptOperationResult.cs` | **Diperbarui.** `OPT-VAL-049`, `OPT-DEPOT-MISSING`, `OPT-STOCK-REJECTED`, `OPT-STOCK-UNAVAILABLE`, `OPT-STOCK-FORBIDDEN` |
| `Program.cs` | **Diperbarui.** `AddScoped<OptStockGateway>()` |
| `Tests/QuilvianSystemBackend.OpticalTests/Be033StockGatewayTests.cs` | **Baru** (diabaikan Git; perlu `git add -f`). 11 uji dengan `DrugStockService` asli |
| `docs/module-blueprints/optik/00-interview-decisions.md` | **Diperbarui** ke r7. `OPT-DEC-060`; `OPT-OQ-015` ditutup; empat baris penyegaran artefak terkunci (termasuk temuan `BE-OPT-003` dan `BE-OPT-030`) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Tidak ada endpoint. Gateway dipakai `BE-OPT-034`, `040`, `041`, `042`, `050` |
| Kontrak integrasi | `INT-OPT-07` terpenuhi: jenis dokumen, kunci tetap, FEFO, gagal = tidak ada perubahan. Baris "Kembali" kini pasti: Stok Masuk batch yang sama (`OPT-DEC-060`) |
| Kode galat | Empat kode di luar `OPT-VAL-v1` (bagian 2); dicatat untuk penyegaran matriks validasi |
| Database | Tanpa migration. Nilai baru `SourceDocumentType = "OpticalOrder"` muat di kolom `varchar(50)` yang ada |
| Keamanan/Auth | Optik tidak menulis saldo atau kartu stok sendiri. Identitas pengguna ditegakkan Farmasi. Audit pengembalian memuat nomor pesanan dan jumlah di teks pesan, tanpa data pasien |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — task ini tidak membuat atau mengubah endpoint.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Cek server lokal sebelum build | Tidak ada proses `dotnet`/`node` berjalan | `PASS` | `tasklist` |
| `dotnet test Tests/QuilvianSystemBackend.OpticalTests -p:RunAnalyzers=False` (membangun backend) | 166 lulus, 0 gagal (11 baru) | `PASS` | Keluaran `dotnet test` |
| Kriteria (1) / `OPT-AC-008`: potong dua kali dengan kunci sama | Stok berkurang sekali; satu mutasi `StockOut` bertanda `OpticalOrder` + id pesanan; panggilan kedua `IsReplay` | `PASS` | Uji `Kriteria_1_…` |
| FEFO lintas batch lalu kembali | Potong 3 = B1 (1) + B2 (2); kembali 3 ke B1 dan B2; mutasi `StockIn`, `CorrectionOfMutationId` kosong, kunci `…:return` | `PASS` | Uji |
| Kriteria (2): kembali sesuai keputusan Farmasi, dua kali → sekali, belum dipotong → tidak bergerak | Lulus | `PASS` | Uji `Kriteria_2_…` |
| `OPT-VAL-049` stok kurang | `409`, kalimat persis kontrak; saldo dan kartu stok tidak berubah sesudah pemanggil menyimpan | `PASS` | Uji |
| Bentrok versi saat Farmasi menyimpan | `409 OPT-CONCURRENCY`; tidak ada mutasi tertinggal di konteks maupun di basis data | `PASS` | Uji dengan interceptor `DbUpdateConcurrencyException` |
| Uji mutasi: pembersihan pada jalur bentrok versi dibuang sementara | Uji di atas gagal; berkas dipulihkan | `PASS` | Keluaran `dotnet test` |
| Lensa custom / lensa stok tanpa saldo | Dilewati / ditolak `OPT-VAL-049` | `PASS` | Uji |
| Tanpa identitas pengguna | `403 OPT-STOCK-FORBIDDEN`; tanpa pergerakan | `PASS` | Uji |
| Depo Optik tidak aktif | `409 OPT-DEPOT-MISSING` | `PASS` | Uji |
| Transaksi pemanggil di-rollback | Potongan ikut batal | `PASS` | Uji |
| Dua pesanan berbeda dengan barang sama | Masing-masing memotong | `PASS` | Uji |
| QBE checker `-Mode Strict -Path` atas 4 berkas source | `VIOLATION: 0`, `REVIEW: 0`, `PASS` | `PASS` | Keluaran checker |
| Kontrol negatif `ZzzRegistryProbe` (multi-baris, pemanggilan terpisah) | Terkena QBE-ENT-001, QBE-CFG-001, QBE-MOD-002; probe dihapus | `PASS` | Keluaran checker |

**Tidak dijalankan:**
- Balapan dua pengembalian bersamaan di PostgreSQL. Penanganannya bergantung pada unique index kartu stok; jalur `DbUpdateException` belum dicoba nyata.
- `Tests/QuilvianSystemBackend.PharmacyTests` gagal dikompilasi sebelum task ini (lihat laporan `BE-OPT-030`). Perubahan Farmasi di task ini hanya satu konstanta.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Potong dua kali dengan kunci sama → stok berkurang sekali (`OPT-AC-008`) | Terpenuhi di harness | Uji `Kriteria_1_…` |
| (2) Jenis mutasi pengembalian sesuai keputusan Farmasi | Terpenuhi sesuai `OPT-DEC-060` | Uji FEFO-kembali dan `Kriteria_2_…`. Konfirmasi Ikbal atas `OPT-DEC-060` belum tercatat |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Perlu tindak lanjut | Mintakan konfirmasi Ikbal (Farmasi) atas `OPT-DEC-060`. Bila Farmasi memilih cara lain, yang berubah hanya `OptStockGateway.ReturnAsync` |
| Untuk `BE-OPT-034` | Bungkus potong stok, status pesanan, dan kunci resep dalam satu transaksi. `DrugStockService.IssueAsync` menyimpan sendiri di dalam transaksi itu. Isi `OptOrderItem.StockIssuedAt`/`StockReturnedAt` dari hasil gateway |
| Risiko tersisa | Belum dicoba di PostgreSQL. Depo Optik di devYoga belum dipastikan ada (`OPT-APR-005` memberi izin, data depo diisi Farmasi) |
| Perubahan sampingan | `NONE`. Probe QBE sementara dibuat dan dihapus |
| Status Git | Tambahan task ini: `M` `DrugStockEnums.cs`, `OptOperationResult.cs`, `Program.cs`, `00-interview-decisions.md`, roadmap dan traceability; `??` `OptStockGateway.cs` dan laporan ini. Berkas uji diabaikan Git. Perubahan `BE-OPT-003`/`030` masih ada di working tree |
| Langkah berikutnya | `BE-OPT-031` (tarif pemeriksaan sekali saat final), lalu `BE-OPT-032` (pesanan) dan `BE-OPT-034` (kirim ke pengerjaan memakai gateway ini) |
