# Laporan Perubahan Backend — `BE-OPT-052`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-OPT-052` |
| Judul | Empat laporan Optik |
| Slice | `MVP-4` — `EPIC OPT-10` |
| Roadmap | `roadmap/backend-roadmap.md` (`OPT-RM-BE-001` r2) — Rincian `BE-OPT-052` |
| Trace | `FR-OPT-100`; `OPT-DEC-025`; `UAT-22`, `23` |
| Contract version | `OPT-API-v1` grup Opt Report — `approved` sisi Optik |
| Penyetuju modul lain | Tidak ada perubahan kode modul lain; membaca baris tagihan Billing |
| Dependency | `BE-OPT-041` ✅, `BE-OPT-050` ✅ |
| Klasifikasi | `MEDIUM` — 4 endpoint baca-saja, service, DTO |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/HealthServices/OpticalManagement/{Controllers,Services,DTOs}/`, `Program.cs`, `Tests/QuilvianSystemBackend.OpticalTests/` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `d8fa6e40` (branch `yoga`), di atas perubahan `BE-OPT-003`, `030`..`051` yang belum di-commit |
| Tanggal | 2026-11-04 |
| Status | Selesai dan tervalidasi di harness. Belum di-commit |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `OpticalManagement` |
| Registry | `Opt` / `ACTIVE` |
| Keberlakuan | `NEW CODE`; `TOUCHED LEGACY` hanya `Program.cs` (DI) |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-DTO-001`, `QBE-ENT-003` (penjualan dari Billing, bukan harga Optik) |
| Wewenang terpisah | Source ✅ · migration tidak diperlukan · eksekusi database ✘ · deployment ✘ |

---

## 1. Masalah yang diperbaiki

Kepala Optik belum punya laporan untuk memantau kerja pemeriksa, penjualan, pesanan yang tertunda, dan garansi
(`OPT-DEC-025`).

---

## 2. Proses bisnis

Semua laporan baca-saja dan hanya terbuka bagi pemegang `OptReport : Read`; petugas penjualan ditolak `403` (`UAT-23`).
Periode memakai tanggal WIB, termasuk kedua ujung, paling panjang 366 hari; bawaan awal bulan berjalan s.d. hari ini.

1. **Kunjungan dan pemeriksaan per pemeriksa.** Jumlah kunjungan Optik pada periode (tidak termasuk yang dibatalkan), lalu per pemeriksa: kunjungan berbeda, pemeriksaan final, dan pemeriksaan yang pernah direvisi. **Contoh:** Rina (RO) 2 pemeriksaan di 2 kunjungan, 1 di antaranya direvisi; dr. Bima 1 pemeriksaan. Draf tidak dihitung.
2. **Penjualan.** Dibaca dari baris tagihan Billing sumber `OPTICAL` yang aktif dan pertama kali ditagih pada periode, per jenis barang (frame, lensa, lensa kontak, aksesoris, kacamata jadi, **Pemeriksaan Refraksi**) dan per cara bayar kunjungan (Tunai, Asuransi, Penjamin Perusahaan). **Contoh:** frame Rp 450.000 + lensa Rp 600.000 = Rp 1.050.000 Tunai; pesanan yang dibatalkan (barisnya di-void) tidak dihitung.
3. **Pesanan terbuka (`UAT-22`).** Semua pesanan yang belum diserahkan, dibatalkan, atau tidak diambil, per tanggal acuan. Ditandai **terlambat** bila masih dikerjakan melewati perkiraan jadi, dan **perlu dihubungi** bila siap diambil melewati batas pengingat (30 hari). **Contoh** per 9 November 2026: pesanan dengan perkiraan jadi 5 November terlambat 4 hari; pesanan siap sejak 5 Oktober (35 hari) perlu dihubungi; pesanan siap 5 hari belum perlu dihubungi.
4. **Keluhan, garansi, dan remake.** Jumlah keluhan pada periode: dalam/di luar garansi, disetujui/ditolak/menunggu, remake dibuat dan sudah diserahkan, serta rincian per jenis keluhan.

**Jalur tidak normal:**

| Keadaan | Hasil |
| --- | --- |
| Tanggal awal sesudah tanggal akhir | `400` "Tanggal awal periode tidak boleh sesudah tanggal akhir." |
| Periode lebih dari 366 hari | `400` "Periode laporan paling panjang 366 hari." |
| Pengguna tanpa `OptReport : Read` | `403` (filter izin) |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kontrak grup Opt Report, `OPT-DEC-025`; `BilInvoiceItem`/`BilInvoice` (status baris, cap waktu), `RegPatientEncounter.PaymentType` (`EncounterPaymentType`: Tunai, Asuransi, Penjamin Perusahaan), `OptOrder`, `OptOrderItem`, `OptRefractionExam`, `OptWarrantyClaim`, `OptSetting.PickupReminderDays`, `AccessPermissionAttribute`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/OpticalManagement/Controllers/OptReportController.cs` | **Baru.** 4 endpoint |
| `Areas/HealthServices/OpticalManagement/Services/OptReportService.cs` | **Baru.** |
| `Areas/HealthServices/OpticalManagement/DTOs/OptReportDtos.cs` | **Baru.** |
| `Program.cs` | **Diperbarui.** `AddScoped<OptReportService>()` |
| `Tests/QuilvianSystemBackend.OpticalTests/Be052ReportTests.cs` | **Baru** (diabaikan Git; perlu `git add -f`). 7 uji |
| `Tests/QuilvianSystemBackend.OpticalTests/OpticalPermissionRegistryTests.cs` | **Diperbarui.** Hitungan endpoint Optik 40 → 44 |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Empat endpoint `OPT-API-v1` tersedia. Parameter: `From`/`To` (periode) dan `AsOf` (pesanan terbuka) |
| Database | Tanpa migration; baca-saja |
| Keamanan/Auth | `OptReport : Read` pada keempat endpoint; lolos `PermissionRegistryValidator` (44 endpoint Optik). Laporan pesanan terbuka memuat nama dan nomor RM pasien, sehingga izin ini sebaiknya hanya diberikan ke Kepala Optik |

---

## 4. Dokumentasi endpoint

Base URL: `api/v1/health-services/optical-management/reports`

| Method | Path | Parameter | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/visits-by-examiner` | `From`, `To` | `OptReport : Read` |
| `GET` | `/sales` | `From`, `To` | `OptReport : Read` |
| `GET` | `/open-orders` | `AsOf` | `OptReport : Read` |
| `GET` | `/warranty` | `From`, `To` | `OptReport : Read` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Cek server lokal sebelum build | Tidak ada proses `dotnet` berjalan | `PASS` | `tasklist` |
| `dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=False` | 0 error | `PASS` | Keluaran build |
| `dotnet test Tests/QuilvianSystemBackend.OpticalTests -p:RunAnalyzers=False` | 237 lulus, 0 gagal (7 baru) | `PASS` | Keluaran `dotnet test` |
| Penjualan = baris tagihan `OPTICAL` aktif | Total laporan = jumlah baris aktif di Billing = Rp 1.050.000; pesanan batal (baris `VOIDED`) tidak dihitung; per kategori dan per cara bayar benar; periode lain kosong | `PASS` | Uji dengan tagihan dari alur pesanan sungguhan |
| Baris pemeriksaan | Masuk kategori "Pemeriksaan Refraksi" Rp 75.000 | `PASS` | Uji |
| `UAT-22` | Terlambat 4 hari; perlu dihubungi 35 hari; siap 5 hari tidak perlu dihubungi; draf ikut terbuka tanpa tanda | `PASS` | Uji |
| Kunjungan per pemeriksa | Total kunjungan dan pemeriksaan; rincian Rina dan dr. Bima; draf tidak dihitung | `PASS` | Uji |
| Garansi | Dalam/di luar, status, remake dan remake diserahkan, per jenis keluhan | `PASS` | Uji |
| Periode tidak sah | `400` kedua kasus | `PASS` | Uji |
| `UAT-23` | Keempat aksi berizin `OptReport : Read` | `PASS` | Uji atribut + registri izin |
| Uji mutasi: baris void ikut dihitung + batas pengingat diabaikan | Kedua mutasi gagal pada uji (yang kedua sesudah kasus "siap 5 hari" ditambahkan); berkas dipulihkan | `PASS` | Keluaran `dotnet test` |
| QBE checker `-Mode Strict -Path` atas 4 berkas | `VIOLATION: 0`, `REVIEW: 0`, `PASS` | `PASS` | Keluaran checker |
| Kontrol negatif `ZzzRegistryProbe` (pemanggilan terpisah) | Terkena 3 aturan; probe dihapus | `PASS` | Keluaran checker |

**Tidak dijalankan:** penolakan `403` lewat permintaan HTTP sungguhan (dibuktikan lewat atribut dan registri izin); PostgreSQL.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `UAT-22` | Terpenuhi di harness | Uji |
| `UAT-23` | Terpenuhi lewat izin endpoint | Uji atribut + registri |
| Angka penjualan sama dengan baris tagihan sumber `Optical` | Terpenuhi di harness | Uji penjualan |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Keputusan pelaksanaan yang perlu dikonfirmasi | (a) Penjualan dihitung **bruto** dari baris tagihan (jumlah × harga), sebelum diskon, adjustment, dan pembayaran yang berada di tingkat tagihan Billing. (b) `OPT-DEC-025` menyebut BPJS terpisah, tetapi kunjungan hanya membedakan Tunai/Asuransi/Penjamin Perusahaan; BPJS masuk Asuransi. (c) Periode penjualan memakai waktu baris **pertama kali ditagih**. (d) Laporan pesanan terbuka menyertakan draf |
| Risiko tersisa | Kredit atas baris yang dibatalkan sesudah pengerjaan (`BE-OPT-041`) tidak mengurangi angka penjualan, karena barisnya tetap aktif dan kreditnya ada di adjustment Billing. Belum dicoba di PostgreSQL |
| Perubahan sampingan | `NONE`. Probe QBE sementara dibuat dan dihapus |
| Status Git | Tambahan task ini: `??` `OptReportController.cs`, `OptReportService.cs`, `OptReportDtos.cs`, laporan ini; `M` `Program.cs`, roadmap, traceability. Berkas uji diabaikan Git |
| Langkah berikutnya | `BE-OPT-053` (uji menyeluruh: privasi log, registri izin, alur penuh di harness) — task terakhir backend Optik |
