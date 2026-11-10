# Laporan Perubahan Backend — `BE-OPT-034`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-OPT-034` |
| Judul | Kirim ke pengerjaan dan tandai siap |
| Slice | `MVP-2` — `EPIC OPT-04` (menutup `MVP-2` backend) |
| Roadmap | `roadmap/backend-roadmap.md` (`OPT-RM-BE-001` r2) — Rincian `BE-OPT-034` |
| Trace | `FR-OPT-032`, `040`, `042`, `043`; `OPT-DEC-007`, `013`, `019`, `043`; `OPT-AC-004`, `005`, `008`, `019`; `UAT-07`..`09` |
| Contract version | `OPT-API-v1` grup Opt Order, `OPT-STATE-v1` bagian 3, `OPT-VAL-v1` `OPT-VAL-022`, `046`..`049`, `057`, `058`, `OPT-INT-v1` `INT-OPT-02`/`04`/`06`/`07` — `approved` sisi Optik |
| Penyetuju modul lain | Tidak ada perubahan kode modul lain. Memakai jalur Farmasi (`BE-OPT-033`) dan Billing (`BE-OPT-030`) |
| Dependency | `BE-OPT-032` ✅, `BE-OPT-033` ✅ |
| Klasifikasi | `MEDIUM` — 2 endpoint dan dua tindakan service dengan transaksi |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/HealthServices/OpticalManagement/{Controllers,Services,DTOs}/`, `Tests/QuilvianSystemBackend.OpticalTests/` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `d8fa6e40` (branch `yoga`), di atas perubahan `BE-OPT-003`, `030`..`033` yang belum di-commit |
| Tanggal | 2026-11-04 |
| Status | Selesai dan tervalidasi di harness. Belum di-commit |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `OpticalManagement` |
| Registry | `Opt` / `ACTIVE` |
| Keberlakuan | `NEW CODE` |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-VAL-001`, `QBE-DTO-001`, `QBE-LOG-001`, `QBE-ENT-003` |
| Wewenang terpisah | Source ✅ · migration tidak diperlukan · eksekusi database ✘ · deployment ✘ |

---

## 1. Masalah yang diperbaiki

Pesanan yang sudah ditagihkan belum bisa dikirim ke bengkel atau lab, sehingga stok tidak terpotong, resep
tidak terkunci, dan pasien yang melunasi di awal bisa membuat kunjungan tertutup sebelum kacamatanya jadi.

---

## 2. Proses bisnis

**Contoh (`UAT-07`):** pesanan Ibu Sari (samaran) Rp 1.050.000, sudah dibayar Rp 525.000.

1. **Kirim ke pengerjaan.** Petugas memilih bengkel sendiri dan perkiraan jadi 5 November 2026. Sistem memeriksa uang muka (minimum Rp 525.000 terpenuhi) dan tidak ada pesanan berpengerjaan lain yang belum lunas. Dalam **satu transaksi**: stok 1 frame dan 1 lensa stok dipotong dari depo Optik, resep OPT-RX-1 dikunci, status menjadi `Dalam Pengerjaan`. Sesudah itu baris tagihannya berubah ke `IN_WORK`, sehingga Billing tidak memfinalisasi tagihan walaupun pasien sudah lunas (`OPT-DEC-043`).
2. **Lensa custom ke lab luar.** Contoh Pak Andi (samaran): lensa progresif dipesan ke Lab Lensa X. Hanya frame yang dipotong dari stok.
3. **Tekan dua kali.** Permintaan kedua dengan isian sama dijawab berhasil tanpa memotong stok lagi dan tanpa riwayat kedua (`OPT-AC-008`).
4. **Resep terkunci.** Rina mencoba merevisi resep OPT-RX-1 → ditolak `OPT-VAL-022` (`OPT-AC-004`).
5. **Tandai siap.** Status `Siap Diambil`, baris tagihan berubah ke `READY` (masih menahan finalisasi). Stok tidak dipotong lagi.

**Jalur tidak normal:**

| Keadaan | Hasil |
| --- | --- |
| Uang muka Rp 500.000 (`UAT-08`) | `409 OPT-VAL-046` "Uang muka belum memenuhi minimum Rp 525.000." |
| Pesanan pertama di kunjungan yang sama sudah dikirim dan belum lunas (`UAT-09`) | `409 OPT-VAL-047` "Lunasi pesanan OPT-ORD-RSMMC-000001 dulu atau daftarkan kunjungan baru." |
| Stok lensa kosong | `409 OPT-VAL-049` "Stok lensa kacamata LNS-156 di depo Optik tidak cukup."; frame yang sudah dipotong lebih dulu ikut batal; status, kunci resep, dan tagihan tidak berubah |
| Tujuan kosong, lab tanpa pemasok aktif, atau perkiraan jadi sebelum hari ini | `400 OPT-VAL-048` "Lengkapi tujuan pengerjaan dan perkiraan jadi." |
| Billing tidak terbaca saat cek uang muka | `503`, tidak ada perubahan (*fail-closed*) |
| Pesanan masih `Draf`, atau kirim pada pesanan yang sudah dikirim dengan isian lain | `409 OPT-VAL-057` |
| `ExpectedVersion` basi | `409 OPT-VAL-058` |
| Tandai siap pada pesanan yang belum dikirim | `409 OPT-VAL-057` |
| Batal sesudah siap | `409` "Pembatalan sesudah pengerjaan perlu persetujuan Kepala Optik." (`BE-OPT-041`) |
| Tagihan belum mengikuti tahap baru (Billing tidak menjawab sesudah commit) | Status tetap tersimpan; jawaban membawa `BillingMessage`; fakta yang tercatat dikirim ulang producer |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`OPT-STATE-v1` bagian 3, `OPT-VAL-v1` bagian 4, `OPT-DEC-007`/`013`; `OptPrescriptionService.ReviseAsync` (penolakan `LockedAt`), `OptOrder`/`OptPrescription` (token `Version`), `MstSupplier`, `OptStockGateway`, `OptOrderPaymentGate`, `OptBillingGateway.EmitItemChargesAsync`, `ClinicalMilestoneFactProducer` (larangan di dalam transaksi).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/OpticalManagement/Services/OptOrderService.cs` | **Diperbarui.** `SendToWorkAsync`, `MarkReadyAsync`, deteksi tekan dua kali, validasi tujuan, revisi fakta sesudah commit; konstruktor menerima `OptStockGateway`; proyeksi memuat tujuan, pemasok, waktu kirim/siap, waktu potong stok per baris |
| `Areas/HealthServices/OpticalManagement/Controllers/OptOrderController.cs` | **Diperbarui.** `POST /{id}/send-to-work`, `POST /{id}/mark-ready` |
| `Areas/HealthServices/OpticalManagement/DTOs/OptOrderDtos.cs` | **Diperbarui.** `OptOrderSendToWorkRequest`, `OptOrderMarkReadyRequest`, ruas jawaban baru |
| `Areas/HealthServices/OpticalManagement/Services/OptOperationResult.cs` | **Diperbarui.** `OPT-VAL-046`..`048` |
| `Tests/QuilvianSystemBackend.OpticalTests/Be034SendToWorkTests.cs` | **Baru** (diabaikan Git; perlu `git add -f`). 10 uji |
| `Tests/QuilvianSystemBackend.OpticalTests/Infrastructure/PesananUji.cs` | **Baru** (diabaikan Git). Harness pesanan dipindah dari `Be032OrderTests` agar dipakai bersama |
| `Tests/QuilvianSystemBackend.OpticalTests/Be032OrderTests.cs` | **Diperbarui.** Mewarisi `PesananUji`; isi uji tidak berubah |
| `Tests/QuilvianSystemBackend.OpticalTests/OpticalPermissionRegistryTests.cs` | **Diperbarui.** Hitungan endpoint Optik 29 → 31 |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Dua endpoint `OPT-API-v1` tersedia. Tambahan pada jawaban: `WorkshopType(Label)`, `ExternalLabSupplierId/Name`, `SentToWorkAt`, `ReadyAt`, `Items[].StockIssuedAt` |
| Kontrak integrasi | `INT-OPT-07` potong di dalam transaksi Optik; `INT-OPT-04` revisi `IN_WORK`/`READY` sesudah commit; `INT-OPT-02` baris belum selesai menahan finalisasi; `INT-OPT-06` gate *fail-closed* |
| Database | Tanpa migration. Menulis `OptOrder`, `OptOrderItem.StockIssuedAt`, `OptOrderStatusHistory`, `OptPrescription.LockedAt/LockedByOrderId`; stok lewat `DrugStockService` |
| Keamanan/Auth | `OptOrder : SendToWork`, `OptOrder : MarkReady`, lolos `PermissionRegistryValidator` (31 endpoint Optik) |

---

## 4. Dokumentasi endpoint

#### Health Services / Optical Management / Opt Order

Base URL: `api/v1/health-services/optical-management/orders`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/{id}/send-to-work` | `WorkshopType`, `ExternalLabSupplierId?`, `EstimatedReadyDate`, `ExpectedVersion` | `OptOrder : SendToWork` |
| `POST` | `/{id}/mark-ready` | `ExpectedVersion` | `OptOrder : MarkReady` |

Kode status: `200`; `400` tujuan tidak lengkap; `404`; `409` uang muka, pesanan lain belum lunas, stok kurang, status, atau versi; `503` Billing tidak terbaca.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Cek server lokal sebelum build | Tidak ada proses `dotnet` berjalan | `PASS` | `tasklist` |
| `dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=False` | 0 error | `PASS` | Keluaran build |
| `dotnet test Tests/QuilvianSystemBackend.OpticalTests -p:RunAnalyzers=False` | 198 lulus, 0 gagal (10 baru) | `PASS` | Keluaran `dotnet test` |
| `UAT-07` | `InFabrication`; stok frame 5→4, lensa 5→4; resep terkunci oleh pesanan ini; 2 baris tagihan `IN_WORK` dan dianggap belum selesai | `PASS` | Uji rantai DrugStockService + producer + jembatan Billing |
| `UAT-08` / `OPT-AC-005` | Rp 500.000 → `409` kalimat persis; Rp 525.000 → berhasil | `PASS` | Uji |
| `UAT-09` / `OPT-AC-019` | `409` dengan nomor pesanan pertama | `PASS` | Uji |
| `OPT-AC-008` tekan dua kali | Stok terpotong sekali; satu riwayat `InFabrication`; versi sama | `PASS` | Uji |
| Stok kurang | `409 OPT-VAL-049`; frame kembali 5; status, kunci resep, mutasi stok, tagihan tidak berubah | `PASS` | Uji |
| Lensa custom ke lab | Hanya frame terpotong; nama lab tampil | `PASS` | Uji |
| `OPT-VAL-048`, status, versi | Lulus | `PASS` | Uji |
| `OPT-AC-004` | Revisi resep `409 OPT-VAL-022` | `PASS` | Uji dengan `OptPrescriptionService` asli |
| Tandai siap | `ReadyForPickup`; tagihan `READY`; stok tidak dipotong lagi | `PASS` | Uji |
| Uji mutasi: rollback diganti commit pada stok gagal + deteksi tekan dua kali dimatikan | 2 uji gagal; berkas dipulihkan | `PASS` | Keluaran `dotnet test` |
| QBE checker `-Mode Strict -Path` atas 4 berkas | `VIOLATION: 0`, `REVIEW: 0`, `PASS` | `PASS` | Keluaran checker |
| Kontrol negatif `ZzzRegistryProbe` (pemanggilan terpisah) | Terkena 3 aturan; probe dihapus | `PASS` | Keluaran checker |

**Tidak dijalankan:** dua petugas mengirim **serentak** (`OPT-AC-008` baris kedua). Perlindungannya adalah token konkurensi `Version` pada `OptOrder` (`DbUpdateConcurrencyException` → `409 OPT-VAL-058`, transaksi di-rollback) dan unique index kartu stok, tetapi belum diuji dengan dua permintaan paralel. Uji yang semula bernama "bersamaan" dihapus karena sebenarnya berurutan. PostgreSQL dan backend yang menyala belum dicoba.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) `OPT-AC-004` | Terpenuhi di harness | Uji |
| (1) `OPT-AC-005` | Terpenuhi di harness | Uji `UAT_08_…` |
| (1) `OPT-AC-008` | Tekan dua kali terpenuhi; serentak belum diuji | Uji |
| (1) `OPT-AC-019` | Terpenuhi di harness | Uji `UAT_09_…` |
| (2) Stok kurang → status, kunci, dan stok tidak berubah | Terpenuhi di harness | Uji + uji mutasi |
| (3) `UAT-07`..`09` | Terpenuhi di harness | Uji |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Konsekuensi aturan sementara | Terbayar dibaca per tagihan kunjungan. Pembayaran dianggap melunasi baris lain lebih dulu saat menilai penahan `OPT-VAL-047`, sehingga sistem tidak bisa membedakan "pesanan pertama lunas" dari "pembayaran untuk pesanan kedua" bila jumlahnya sama. Contoh: dua pesanan @ Rp 1.050.000 dengan total terbayar Rp 1.050.000 dianggap pesanan pertama lunas. Ini hilang saat alokasi per baris Billing (`OPT-APR-003`) tersedia |
| Keputusan pelaksanaan | Tekan dua kali dikenali dari keadaan (status sudah `InFabrication`, versi = versi kirim − 1, isian sama), karena `OptOrder` tidak punya kolom kunci idempoten. Lab luar wajib pemasok aktif di `MstSupplier` |
| Risiko tersisa | Belum diuji serentak dan belum di PostgreSQL. Depo Optik dan stok di devYoga belum dipastikan ada |
| Perubahan sampingan | `NONE` pada source aplikasi. Harness uji pesanan dipindah ke kelas dasar. Probe QBE sementara dibuat dan dihapus |
| Status Git | Tambahan task ini: `M` `OptOrderService.cs`, `OptOrderController.cs`, `OptOrderDtos.cs`, `OptOperationResult.cs`, roadmap, traceability; `??` laporan ini. Berkas uji diabaikan Git |
| Langkah berikutnya | `MVP-2` backend selesai. Lanjut `MVP-3`: `BE-OPT-040` (penyerahan: lunas, identitas, snapshot garansi, pemicu finalisasi tagihan). `BE-OPT-051` (Medical Fee) juga terbuka |
