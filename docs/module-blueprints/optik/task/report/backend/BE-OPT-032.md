# Laporan Perubahan Backend — `BE-OPT-032`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-OPT-032` |
| Judul | Pesanan: draf, ubah, tagihkan per barang, batal sebelum pengerjaan, status bayar |
| Slice | `MVP-2` — `EPIC OPT-04` |
| Roadmap | `roadmap/backend-roadmap.md` (`OPT-RM-BE-001` r2) — Rincian `BE-OPT-032` |
| Trace | `FR-OPT-040`, `041`, `043`, `080`; `OPT-DEC-008`, `009`, `028`, `029`, `034`, `035`, `044`; `OPT-AC-005`, `006`, `018`, `019`, `027` |
| Contract version | `OPT-API-v1` grup Opt Order, `OPT-STATE-v1` bagian 3, `OPT-VAL-v1` bagian 4, `OPT-INT-v1` `INT-OPT-04`/`05`/`06` — `approved` sisi Optik |
| Penyetuju modul lain | Tidak ada perubahan kode modul lain. Membaca Billing (`OPT-APR-002`, Yasmina) |
| Dependency | `BE-OPT-012` ✅, `030` ✅, `031` ✅ |
| Klasifikasi | `LARGE` — 7 endpoint, service pesanan, pembaca status bayar, perluasan gateway tagihan |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/HealthServices/OpticalManagement/{Controllers,Services,DTOs}/`, `Program.cs`, `Tests/QuilvianSystemBackend.OpticalTests/` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `d8fa6e40` (branch `yoga`), di atas perubahan `BE-OPT-003`, `030`, `031`, `033` yang belum di-commit |
| Tanggal | 2026-11-04 |
| Status | Selesai dan tervalidasi di harness. Belum di-commit |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `OpticalManagement` |
| Registry | `Opt` / `ACTIVE` |
| Keberlakuan | `NEW CODE`; `TOUCHED LEGACY` hanya `Program.cs` (DI) |
| QBE yang berlaku | `QBE-SVC-001` (controller tanpa `DbContext`), `QBE-API-001`, `QBE-PERM-001`, `QBE-VAL-001`, `QBE-DTO-001`, `QBE-PAGE-001`, `QBE-LOG-001`, `QBE-ENT-003` (tanpa harga di Optik), `QBE-CODE-006` (nomor lewat `OptNumberService`) |
| Wewenang terpisah | Source ✅ · migration tidak diperlukan · eksekusi database ✘ · deployment ✘ |

---

## 1. Masalah yang diperbaiki

Petugas Optik belum bisa membuat pesanan kacamata, menagihkannya, atau membatalkannya, dan belum bisa
melihat berapa uang muka yang dibutuhkan. Tanpa ini alur `FLOW-OPT-MVP-001` berhenti sesudah resep.

---

## 2. Proses bisnis

**Contoh:** Ibu Sari (samaran) memesan frame FRM-A-HTM-52 Rp 450.000 dan lensa stok LNS-156 Rp 600.000
berdasarkan resep OPT-RX-1.

1. **Buat draf.** Petugas memilih kunjungan, resep, dan barang. Sistem memeriksa barangnya barang Optik, lensa kacamata punya pilihan stok/custom, jumlah > 0, dan resepnya milik pasien, berlaku, dan versi terkini. Minimum uang muka 50% dari pengaturan dibekukan pada pesanan (`OPT-AC-006`). Status `Draf`.
2. **Ubah draf.** Barang boleh diganti selama belum ditagihkan. Baris yang dibuang ditandai batal; baris baru mendapat nomor baris lanjutan.
3. **Simpan & tagihkan.** Sistem memastikan tarif tiap barang ada, lalu memasukkan satu baris tagihan per barang (Rp 450.000 dan Rp 600.000). Status menjadi `Menunggu Uang Muka`. Dua pesanan dengan frame yang sama menghasilkan dua baris terpisah, masing-masing tertaut ke barang pesanannya (`OPT-AC-018`).
4. **Status bayar.** Bagian pasien pesanan Rp 1.050.000, minimum uang muka Rp 525.000. Terbayar Rp 500.000 → belum memenuhi; Rp 525.000 → memenuhi (`OPT-AC-005`). Bila penjamin menanggung Rp 1.000.000 dari Rp 1.200.000, minimum Rp 100.000; ditanggung penuh → Rp 0 (`OPT-AC-027`).
5. **Batal sebelum pengerjaan.** Dengan alasan, pesanan `Draf` atau `Menunggu Uang Muka` dibatalkan; baris tagihannya di-void.

**Jalur tidak normal:**

| Keadaan | Hasil |
| --- | --- |
| Barang tanpa profil Optik aktif | `400 OPT-VAL-040` "Barang ini belum terdaftar sebagai barang Optik." |
| Pesanan tanpa resep | `400 OPT-VAL-041` "Pilih resep untuk pesanan kacamata." |
| Lensa kontak tanpa resep lensa kontak berlaku | `409 OPT-VAL-042` |
| Lensa kacamata tanpa stok/custom | `400 OPT-VAL-043` |
| Jumlah ≤ 0 | `400 OPT-VAL-044` |
| Resep kedaluwarsa | `409 OPT-VAL-023` (aturan resep `BE-OPT-012`) |
| Versi resep sudah direvisi | `409` "Resep ini sudah direvisi. Pilih versi terbaru." |
| Kunjungan tertutup | `409 OPT-VAL-030` |
| Tarif barang belum ada | `400 OPT-VAL-045` "Tarif barang FRM-B-CKL-50 belum ada. Hubungi bagian tarif."; **tidak ada** baris yang terlanjur ditagih; pesanan tetap `Draf` |
| Billing tidak menjawab saat menagihkan | `503`; pesanan tetap `Draf`. Sesudah fakta tertunda dikirim ulang producer, tombol Tagihkan berhasil tanpa baris ganda |
| Ubah/tagih ulang sesudah ditagih, batal pesanan yang sudah batal | `409 OPT-VAL-057` "Tindakan ini tidak berlaku untuk pesanan berstatus …" |
| Batal saat `Dalam Pengerjaan`/`Siap Diambil` | `409` "Pembatalan sesudah pengerjaan perlu persetujuan Kepala Optik." |
| Alasan batal kosong | `400 OPT-VAL-053` |
| `ExpectedVersion` basi | `409 OPT-VAL-058` |
| Pesanan berpengerjaan lain di kunjungan ini belum lunas | Status bayar menyebut nomor pesanan penahan (`OPT-VAL-047`); penolakannya diterapkan saat kirim ke pengerjaan (`BE-OPT-034`) |
| Tagihan tidak terbaca atau kalkulasi belum mengikuti pesanan | Status bayar `503` (*fail-closed*), tidak pernah dianggap lunas |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kontrak grup Opt Order, `OPT-STATE-v1` bagian 3, `OPT-VAL-v1` bagian 4, `INT-OPT-06`; model `OptOrder`, `OptOrderItem`, `OptOrderStatusHistory`, `OptItemProfile`, `OptPrescription(Version)`, `OptSetting` beserta configuration (indeks unik `(OrderId, LineNo)` tanpa saringan, `Version` token konkurensi); `OptPrescriptionService.CheckUsableForOrderAsync`; `OptVisitService`; `BillingInvoiceClosureService.CalculateOutstandingAsync`; `BillingCalculationService` (rincian per item, kalkulasi ulang otomatis sesudah charge); `BillingPayerEditService` (rumus porsi pasien per item); `BillingClinicalChargeBridgeService.GetChargeStateAsync`; `ClinicalMilestoneFactProducer`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/OpticalManagement/Controllers/OptOrderController.cs` | **Baru.** 7 endpoint (bagian 4). Tanpa `<summary>` XML pada action |
| `Areas/HealthServices/OpticalManagement/Services/OptOrderService.cs` | **Baru.** Daftar, detail, status bayar, buat, ubah, tagihkan, batal |
| `Areas/HealthServices/OpticalManagement/Services/OptOrderPaymentGate.cs` | **Baru.** Bagian pasien per pesanan, minimum uang muka, terbayar per tagihan, penahan aturan sementara; *fail-closed* |
| `Areas/HealthServices/OpticalManagement/Services/OptBillingGateway.cs` | **Diperbarui.** `CheckItemTariffsAsync`, `EmitItemChargesAsync` (fakta `ORDERED` + verifikasi masuk tagihan), `EmitItemCancellationsAsync`, `ItemRuleSnapshot`. Konstruktor bertambah tiga dependensi Billing |
| `Areas/HealthServices/OpticalManagement/DTOs/OptOrderDtos.cs` | **Baru.** |
| `Areas/HealthServices/OpticalManagement/Services/OptOperationResult.cs` | **Diperbarui.** `OPT-VAL-040`..`045`, `053`, `057`, `058`, `OPT-BIL-UNAVAILABLE` |
| `Program.cs` | **Diperbarui.** `OptOrderPaymentGate`, `OptOrderService` |
| `Tests/QuilvianSystemBackend.OpticalTests/Be032OrderTests.cs` | **Baru** (diabaikan Git; perlu `git add -f`). 15 uji |
| `Tests/QuilvianSystemBackend.OpticalTests/OpticalPermissionRegistryTests.cs` | **Diperbarui.** Hitungan endpoint Optik 22 → 29 |
| `docs/module-blueprints/optik/00-interview-decisions.md` | **Diperbarui.** Satu baris penyegaran kontrak (bagian 7) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | 7 endpoint `OPT-API-v1` grup Opt Order tersedia. Tambahan tanpa mengubah kontrak: `OptOrderUpdateRequest` = isi buat + `ExpectedVersion`; `BillingMessage` pada jawaban; status bayar memuat `PaidBasis`, `BlockingOrderNumber`, angka tagihan |
| Kontrak integrasi | `INT-OPT-04` barang (`ORDERED`, `RuleSnapshot` lengkap), `INT-OPT-05` (pembatalan baris), `INT-OPT-06` (baca bagian pasien dan terbayar) |
| Database | Tanpa migration. Optik menulis tabel Optik saja; Billing ditulis lewat producer klinis |
| Keamanan/Auth | `OptOrder : Read/Create/Update/Bill/Cancel`, lolos `PermissionRegistryValidator`. Kalimat log hanya nomor pesanan, jumlah barang, dan status |

---

## 4. Dokumentasi endpoint

#### Health Services / Optical Management / Opt Order

Base URL: `api/v1/health-services/optical-management/orders`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/` | Daftar: `Status`, `EncounterId`, `PatientId`, `NeedsContact`, `Overdue`, `Search`, `PageNumber`, `PageSize` | `OptOrder : Read` |
| `GET` | `/{id}` | Detail, barang, riwayat status | `OptOrder : Read` |
| `GET` | `/{id}/payment-status` | Bagian pasien, minimum uang muka, terbayar, sisa, penahan | `OptOrder : Read` |
| `POST` | `/` | Buat draf berpengerjaan | `OptOrder : Create` |
| `PUT` | `/{id}` | Ubah draf (`ExpectedVersion`) | `OptOrder : Update` |
| `POST` | `/{id}/bill` | Simpan & tagihkan (`ExpectedVersion`) | `OptOrder : Bill` |
| `POST` | `/{id}/cancel` | Batal sebelum pengerjaan (`Reason`, `ExpectedVersion`) | `OptOrder : Cancel` |

Kode status: `200`; `400` isian tidak sah atau tarif belum ada; `404` pesanan/kunjungan/resep tidak ada; `409` status, versi, resep, atau kunjungan tidak mengizinkan; `503` Billing tidak menjawab.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Cek server lokal sebelum build | Tidak ada proses `dotnet` berjalan | `PASS` | `tasklist` |
| `dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=False` | 0 error | `PASS` | Keluaran build |
| `dotnet test Tests/QuilvianSystemBackend.OpticalTests -p:RunAnalyzers=False` | 188 lulus, 0 gagal (15 baru) | `PASS` | Keluaran `dotnet test` |
| Buat draf, validasi `OPT-VAL-040`..`044`, resep kedaluwarsa/versi lama, kunjungan tertutup, penjualan langsung ditolak | Lulus, kalimat persis kontrak | `PASS` | Uji |
| `OPT-AC-006` | Pesanan A tetap 50%, pesanan B 30% | `PASS` | Uji |
| Ubah draf + versi basi `OPT-VAL-058` | Lulus; baris dibuang = batal, baris baru nomor 3 | `PASS` | Uji |
| Tagihkan | `Menunggu Uang Muka`; 2 baris `OPTICAL`/`ORDERED` total Rp 1.050.000 tertaut ke id barang; tagih ulang `OPT-VAL-057` | `PASS` | Uji rantai producer → folio → jembatan → `BillingInvoiceService` |
| `OPT-AC-018` | Dua pesanan berframe sama → 4 baris, satu per barang | `PASS` | Uji |
| `OPT-VAL-045` | `400`, nol fakta, nol baris, tetap `Draf` | `PASS` | Uji |
| Billing tidak menjawab | `503`, tetap `Draf`; sesudah `RedispatchAsync` dan Tagihkan ulang → 2 baris saja | `PASS` | Uji dengan interceptor |
| Batal | Alasan wajib (`OPT-VAL-053`); baris tagihan `VOIDED`; batal ulang `OPT-VAL-057`; draf belum ditagih tanpa fakta; batal sesudah pengerjaan `409` | `PASS` | Uji |
| `OPT-AC-005` | Bagian pasien Rp 1.050.000, minimum Rp 525.000; Rp 500.000 belum memenuhi, Rp 525.000 memenuhi | `PASS` | Uji dengan alokasi pembayaran nyata pada tagihan |
| `OPT-AC-019` | Pesanan pertama `Dalam Pengerjaan` belum lunas → nomornya tampil sebagai penahan; sesudah lunas, penahan hilang | `PASS` | Uji |
| `OPT-AC-027` | Rumus bagian pasien dan minimum (Rp 100.000; ditanggung penuh Rp 0) | `PASS` | Uji unit atas rumus yang sama dengan Billing |
| Status bayar *fail-closed* | Belum ditagih `409`; tagihan tidak ada `503` | `PASS` | Uji |
| Uji mutasi: cek tarif dibuang + penahan dipaksa aktif | Uji `OPT-VAL-045` dan `OPT-AC-019` gagal; berkas dipulihkan | `PASS` | Keluaran `dotnet test` |
| `PermissionRegistryValidator` atas 29 endpoint Optik | Nol temuan | `PASS` | Uji registri |
| QBE checker `-Mode Strict -Path` atas 7 berkas | `VIOLATION: 0`, `REVIEW: 0`, `PASS` | `PASS` | Keluaran checker |
| Kontrol negatif `ZzzRegistryProbe` (pemanggilan terpisah) | Terkena 3 aturan; probe dihapus | `PASS` | Keluaran checker |

**Tidak dijalankan:** `OPT-AC-027` dengan penjamin sungguhan di harness (butuh data aturan tanggungan; rumusnya diuji langsung); PostgreSQL dan backend yang menyala.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) `OPT-AC-005` | Angka gate terbukti; penolakan kirim ke pengerjaan di `BE-OPT-034` | Uji `OPT_AC_005_…` |
| (1) `OPT-AC-006` | Terpenuhi | Uji |
| (1) `OPT-AC-018` | Terpenuhi di harness | Uji |
| (1) `OPT-AC-019` | Penahan terbukti; penolakan di `BE-OPT-034` | Uji |
| (1) `OPT-AC-027` | Rumus terbukti | Uji unit |
| (2) Billing tidak menjawab → pesanan tetap `Draft` (`503`) | Terpenuhi di harness | Uji |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Keputusan pelaksanaan yang perlu dikonfirmasi | (a) Hanya pesanan berpengerjaan; penjualan langsung menunggu `BE-OPT-042`. (b) Lensa kontak dan lensa kacamata tidak boleh dalam satu pesanan, karena pesanan hanya menunjuk satu versi resep. (c) Versi resep wajib versi terkini. (d) **Terbayar dihitung per tagihan kunjungan**, termasuk pembayaran untuk tarif pemeriksaan; ini mengikuti contoh PRD (tagihan Rp 1.125.000, uang muka Rp 525.000 → dikirim). (e) Penahan `OPT-VAL-047` aktif bila ada pesanan berpengerjaan lain dan sisa tagihan di luar baris pesanan ini belum nol; artinya tarif pemeriksaan yang belum dibayar juga ikut menahan. Semua tercatat di tabel penyegaran `00-interview-decisions.md` |
| Masalah yang diketahui | Tabel `OptOrder` tidak punya kolom kunci idempoten, sehingga Simpan dua kali pada **Buat** menghasilkan dua draf. Tindakan lain terlindung `ExpectedVersion` dan status. Satuan fakta barang memakai "unit" karena `MstDrug` hanya menyimpan id satuan |
| Risiko tersisa | Bila pembatalan baris gagal diterima Billing sesudah pesanan batal, jawaban membawa `BillingMessage` dan log peringatan; tidak ada pengiriman ulang otomatis dari sisi Optik untuk kasus fakta yang belum tercatat sama sekali. Belum dicoba di PostgreSQL |
| Perubahan sampingan | `NONE`. Probe QBE sementara dibuat dan dihapus |
| Status Git | Tambahan task ini: `??` `OptOrderController.cs`, `OptOrderService.cs`, `OptOrderPaymentGate.cs`, `OptOrderDtos.cs`, laporan ini; `M` `OptBillingGateway.cs` (belum di-commit sejak `031`), `OptOperationResult.cs`, `Program.cs`, roadmap, traceability, `00-interview-decisions.md`. Berkas uji diabaikan Git |
| Langkah berikutnya | `BE-OPT-034` (kirim ke pengerjaan: gerbang uang muka dan penahan dari gate ini, kunci resep, potong stok lewat `OptStockGateway`, satu transaksi; tandai siap). `BE-OPT-051` (Medical Fee) juga sudah bisa dikerjakan |
