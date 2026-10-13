# PLAN-REPAIR-001 — Menutup celah checkout klinis sampai invoice kasir

```yaml
plan_id: PLAN-REPAIR-BKC-001
issue: ../issue/issue-001-integrasi-checkout-ke-billing-kasir.md
status_rencana: DIKERJAKAN_10_DARI_11
tanggal_rencana: "2026-10-12"
diputuskan_oleh: "Pemilik pekerjaan (pilihan interaktif K-01..K-04); K-05..K-07 diturunkan dari keputusan/rekomendasi yang ada"
tanggal_keputusan: "2026-10-12"
basis_source_backend: "05634d1f498a57c990789367521c8b430e08c32e (MHamzah)"
basis_source_frontend: "7d44526ff11822e04d2ed558994f602986b4e2ef (HamzahV2)"
basis_implementasi_backend: "d9089882 (MHamzah) + working tree, belum di-commit"
basis_implementasi_frontend: "7d44526ff (HamzahV2) + working tree, belum di-commit"
```

Rencana ini disetujui dan dikerjakan pada 12 Oktober 2026 atas permintaan pemilik ("kerjakan sampai tuntas"). Keputusan K-01..K-07 tercatat sebagai `BKC-DEC-135`..`141` di [`00-interview-decisions.md`](../../00-interview-decisions.md). Nomor `FIX` tetap nomor perbaikan dokumen ini; task builder yang mengerjakannya adalah `BE-BKC-FIX-015`..`021` dan `FE-BKC-FIX-012`..`013`, dengan laporan di `task/report/`. Seluruh perubahan masih berada di working tree — commit dan push dilakukan pemilik.

## 1. Register status pengerjaan

| ID perbaikan | Menutup | Ringkasan | Area / pemilik | Prioritas | Task ID | Status | Bukti |
| --- | --- | --- | --- | :---: | --- | --- | --- |
| FIX-BKC-001-01 | ISS-BKC-001-01 | Hubungkan intervensi keperawatan billable | Clinical + Billing | 1 | — | ⏸️ DITUNDA PEMILIK | `BKC-DEC-135` (K-01: tunda) |
| FIX-BKC-001-02 | ISS-BKC-001-02 | Selaraskan checkout Alkes dengan API pemakaian | FE Keperawatan + kontrak Farmasi | 1 | `FE-BKC-FIX-012` (+ sisi server di `BE-BKC-FIX-017`) | ✅ SELESAI | [laporan](../../task/report/frontend/FE-BKC-FIX-012.md) |
| FIX-BKC-001-03 | ISS-BKC-001-02 | Kirim pemakaian nonresep/BMHP yang tercatat ke invoice | Farmasi + Billing | 1 | `BE-BKC-FIX-017` | ✅ SELESAI | [laporan](../../task/report/backend/BE-BKC-FIX-017.md) |
| FIX-BKC-001-04 | ISS-BKC-001-03 | Teruskan fakta Bank Darah ke invoice | Billing + Bank Darah | 1 | `BE-BKC-FIX-018` | ✅ SELESAI | [laporan](../../task/report/backend/BE-BKC-FIX-018.md) |
| FIX-BKC-001-05 | ISS-BKC-001-04 | Satukan pengiriman resep dan aturan care setting/racikan | Farmasi + Billing | 1 | `BE-BKC-FIX-015` | ✅ SELESAI | [laporan](../../task/report/backend/BE-BKC-FIX-015.md) |
| FIX-BKC-001-06 | ISS-BKC-001-05 | Tetapkan dan implementasikan titik tagih Gizi | Gizi + Billing | 2 | — | ✅ SELESAI (keputusan, tanpa kode) | `BKC-DEC-137` (K-03: tidak ditagih terpisah) |
| FIX-BKC-001-07 | ISS-BKC-001-06 | Samakan penerbitan fakta seluruh jalur Completed | Clinical + IGD | 1 | `BE-BKC-FIX-019` | ✅ SELESAI | [laporan](../../task/report/backend/BE-BKC-FIX-019.md) |
| FIX-BKC-001-08 | ISS-BKC-001-06 | Tutup cakupan invoice IGD dan transfer | IGD + Billing | 1 | `BE-BKC-FIX-016` | ✅ SELESAI | [laporan](../../task/report/backend/BE-BKC-FIX-016.md) |
| FIX-BKC-001-09 | ISS-BKC-001-07 | Cegah angka daftar kasir yang tertinggal/tampak nol | Billing | 1 | `BE-BKC-FIX-020` | ✅ SELESAI | [laporan](../../task/report/backend/BE-BKC-FIX-020.md) |
| FIX-BKC-001-10 | ISS-BKC-001-08 | Perjelas total tagihan, pembayaran, dan kekinian di kasir | FE Billing | 2 | `FE-BKC-FIX-013` | ✅ SELESAI | [laporan](../../task/report/frontend/FE-BKC-FIX-013.md) |
| FIX-BKC-001-11 | ISS-BKC-001-T1 | Periksa charge eligible tertunda sebelum finalisasi | Billing | 1 | `BE-BKC-FIX-021` | ✅ SELESAI | [laporan](../../task/report/backend/BE-BKC-FIX-021.md) |

**Ringkasan 12 Oktober 2026: 10 dari 11 usulan perbaikan selesai** — 9 lewat source dan 1 lewat keputusan (FIX-06); **1 ditunda pemilik** (FIX-01). Semuanya masih di working tree, belum di-commit.

**Verifikasi yang benar-benar dijalankan:** build backend `QuilvianSystemBackend.csproj` 0 error (warning baru dari berkas yang diubah: nol); 15/15 test baru `CheckoutBillingIntegrationTests` lulus (dijalankan lewat proyek uji sementara karena proyek `PharmacyTests` di repo sudah tidak terkompilasi sebelum task ini — galat di `BillingInvoiceEligibilityTests.cs` dan `BillingZeroPatientObligationTests.cs` yang tidak disentuh); unit frontend 48/49 (satu-satunya gagal `FE-RWI-094`, mismatch lama bagian 8); `npx eslint src --quiet` 0 error; `npm run build` frontend lulus. **`NOT RUN`:** uji transaksi PostgreSQL ujung-ke-ujung, aplikasi berjalan, dan skenario bagian 8 — lihat tiap laporan.

## 2. Solusi terpilih per temuan

| Temuan | Opsi A | Opsi B | Solusi terpilih dan alasan |
| --- | --- | --- | --- |
| ISS-01 — intervensi | Tambah producer/resolver lewat jalur fakta existing | Kasir mengetik ulang tindakan | A setelah K-01; jejak sumber dan anti-duplikasi dapat dipertahankan |
| ISS-02 — Alkes/nonresep | Gunakan kontrak Drug Usage yang sah lalu integrasi charge aktual | Fallback otomatis menjadi resep ketika request gagal | A; pemakaian dan resep mempunyai prasyarat/lifecycle berbeda. Fallback saat gagal dapat menyembunyikan kegagalan nyata |
| ISS-03 — Bank Darah | Tambah consumer invoice atas fakta existing | Tambah charge manual tanpa tautan fakta | A setelah K-06; tidak mengulang pencatatan klinis dan tetap satu fakta per tindakan |
| ISS-04 — resep | Satu pengiriman lewat fakta/jembatan, dengan aturan tahap dan racikan yang dikunci | Pertahankan dua pengirim lalu abaikan konflik | A; menghapus dua otoritas harga/waktu tagih. B membiarkan keberhasilan parsial dan konflik sumber |
| ISS-05 — Gizi | Tetapkan milestone billable/paket sebelum integrasi | Otomatis menagih setiap order | A setelah K-03; order tidak membuktikan kunjungan/layanan sudah terjadi |
| ISS-06 — Completed/IGD | Satukan emisi fakta; kunci cakupan invoice IGD secara terpisah | Minta petugas execute ulang dan cabut filter Emergency | A; execute ulang saat ini short circuit, dan cabut filter berisiko salah invoice pada transfer |
| ISS-07 — angka daftar | Deteksi kekinian dan sediakan angka/keadaan yang sah dari backend | Selalu pakai kalkulasi terakhir atau nol | A; B dapat menampilkan kewajiban yang salah. Tidak perlu menulis kalkulasi transaksi hanya karena halaman daftar dibuka |
| ISS-08 — label | Pisahkan total tagihan, pembayaran, dan sisa pasien | Ganti angka pada kolom pembayaran menjadi nilai invoice | A; B mengubah makna kolom tanpa menjelaskan tanggal bayar/status |
| ISS-T1 — finalisasi | Cek sumber eligible tertunda serta pengecualian sah | Cek hanya item invoice yang sudah terbentuk | A; B tidak dapat mendeteksi charge yang belum menjadi item |

Untuk kemampuan baru, “terpilih” berarti rekomendasi teknis setelah kebijakan pemilik dikunci. Dokumen ini tidak menetapkan tarif, kebijakan klinis, atau kewenangan perawat baru.

## 3. Skema tampilan sebelum → sesudah

Seluruh **SESUDAH adalah rencana, belum tersedia**. Data contoh memakai pasien samaran.

### 3.1 Daftar Invoice & Billing Kasir

**SEBELUM — sesuai source dan bentuk screenshot:**

```text
Invoice & Billing Kasir
[ Cari nama/RM/penjamin ] [ Semua Periode ▾ ] [Tanggal Mulai] [Tanggal Akhir] [ Cari ]

Pasien       Biaya & Tgl Bayar   Sisa Pembayaran    Info Deposit       Aksi
Pasien A     Rp0 / -             Rp0               Belum Ada Deposit  [ ⋮ ]
                                Belum Bayar

Tidak ada total invoice tersendiri atau penanda kalkulasi belum terkini pada row.
```

**SESUDAH — rancangan untuk FIX-09/FIX-10:**

```text
Invoice & Billing Kasir
[ Cari nama/RM/penjamin ] [ Semua Periode ▾ ] [Tanggal Mulai] [Tanggal Akhir] [ Cari ] [ Muat Ulang ]

Pasien     Total Tagihan   Sudah Dibayar & Tgl   Sisa Pasien       Status Tagihan       Aksi
Pasien A   Rp150.000       Rp0 / -              Rp150.000        Terkini               [ ⋮ ]
Pasien B   Belum dihitung  Rp0 / -              Belum dihitung   Perlu hitung ulang    [ ⋮ ]

⚠ Ada pelayanan yang belum masuk invoice. [ Lihat status integrasi ]
```

| Wilayah | Isi | Sumber data | Komponen |
| --- | --- | --- | --- |
| Filter | Nama/RM/penjamin dan periode existing | Query daftar | Reuse filter/base component existing |
| Total Tagihan | Nilai seluruh invoice sesuai kalkulasi sah; tidak disamakan dengan kewajiban pasien | Backend, basis yang disetujui FIX-09 | Kolom pada tabel existing |
| Sudah Dibayar | Pembayaran sah dan tanggal terakhir | TotalPaidAmount / LastSuccessfulPaymentAt existing | Kolom existing dengan label diperjelas |
| Sisa Pasien | Kewajiban pasien setelah penjamin/pembayaran yang sah | Backend, tidak menghitung coverage di browser | Nilai + status existing |
| Kekinian/sinkron | Belum dihitung, perlu hitung ulang, layanan tertunda, atau terkini | Kontrak respons hasil FIX-09/11, masih rencana | Badge/status state existing |
| Deposit | Saldo dan status yang sudah tersedia | Respons deposit existing | Kolom existing |

| Tombol | Jenis | Kapan aktif | Yang terjadi |
| --- | --- | --- | --- |
| Cari | Baca | Filter sah | Memuat ulang daftar |
| Muat Ulang | Baca | Tidak sedang memuat | Membaca status terbaru; tidak membuat item baru |
| Detail / Menu Pembayaran | Navigasi | Ada invoice dan hak akses | Membuka invoice kunjungan yang dipilih |
| Lihat status integrasi | Navigasi/baca, rencana | Ada sumber tertunda dan hak baca yang sah | Membuka status/rekonsiliasi sesuai kontrak; tidak otomatis mengirim ulang |

| Keadaan | Perilaku |
| --- | --- |
| Memuat | Skeleton/loading existing; angka tidak diganti nol |
| Kosong | Pesan tidak ada invoice sesuai filter |
| Gagal | Pesan gagal membaca + coba lagi; tidak mengganti data dengan nol |
| Tidak berhak | Ikuti permission existing; tidak membuka detail finansial |
| Belum dihitung / stale | Nilai yang belum sah tidak disajikan sebagai kewajiban pasti; beri sebab |
| Charge tertunda | Tampilkan perhatian meskipun sebagian item telah masuk |
| Seluruh kewajiban pasien ditanggung penjamin | Sisa pasien sah Rp0, dengan total invoice dan konteks penjamin tetap dapat dibedakan |

### 3.2 Checkout Alkes/BMHP

**SEBELUM — berdasarkan source, bukan screenshot pelapor:**

```text
Katalog Alkes → Keranjang 3 spuit
[ Simpan Order ]
  → createDrugUsage(petugas/lokasi GUID kosong)
  → bila gagal: coba createPrescription tanpa consultationId
  → pesan sukses tidak membedakan draft, pemakaian tercatat, dan masuk invoice
```

**SESUDAH — rancangan untuk FIX-02/FIX-03:**

```text
Katalog Alkes → Keranjang 3 spuit
Lokasi stok: [ pilih lokasi yang berwenang ▾ ]
Petugas: Petugas aktif yang sah sesuai kontrak
[ Simpan Draft / Order ]

Status: Draft tersimpan — belum dicatat sebagai pemakaian
[ Catat Pemakaian ]  hanya untuk actor yang mempunyai DrugUsage : Record
Status: Pemakaian tercatat → Menunggu Billing / Masuk invoice / Perlu ditinjau
```

Jika actor perawat tidak diberi kewenangan `Record`, ia hanya membuat order/draft dan menyerahkan langkah tersebut kepada unit yang berwenang. Usulan ini tidak menambah permission secara otomatis.

| Wilayah | Isi | Sumber data | Komponen |
| --- | --- | --- | --- |
| Katalog/keranjang | Item, jumlah, satuan | Katalog Farmasi existing | Panel existing |
| Lokasi/petugas | Master sah atau resolusi actor server yang sudah dikontrak | Farmasi/identitas | Base select dan label identitas existing; tanpa input UUID |
| Hasil | Status draft, record stok, dan hasil pengiriman invoice terpisah | Respons command + status integrasi | Alert/status existing |

| Tombol | Jenis | Kapan aktif | Yang terjadi |
| --- | --- | --- | --- |
| Simpan Draft / Order | Tulis draft | Berwenang; tidak sedang submit | Validasi isian, kirim kontrak lengkap; pertahankan keranjang saat gagal |
| Catat Pemakaian | Tulis pemakaian | Draft sah dan actor mempunyai Record | Versi/kunci ulang stabil, stok berubah dan charge mengikuti milestone yang disepakati |
| Coba Lagi | Mengulang operasi yang gagal | Kegagalan yang dapat diulang | Dokumen/kunci yang sama; tidak membuat resep pengganti |

| Keadaan | Perilaku |
| --- | --- |
| Memuat | Katalog/status ditandai memuat |
| Kosong | Keranjang kosong; arahkan memilih item |
| Gagal | Tampilkan sebab bisnis; item pilihan tetap ada |
| Tidak berhak | Jelaskan langkah yang dibutuhkan dari petugas berwenang |
| Draft saja | Jangan menyebut pemakaian/penagihan selesai |
| Stok kurang / versi berubah | Penolakan sah; muat data terbaru, tidak menagih atau mengurangi sebagian secara tersembunyi |

### 3.3 Integrasi backend lainnya

FIX-01, 03, 04, 05, 07, 08, dan 11 pada dasarnya tidak membutuhkan tata letak baru. Gunakan status existing dengan makna yang benar: catatan klinis selesai, fakta diterima, invoice tersinkron, atau perlu ditinjau. FIX-06 belum boleh dirancang tampilan tagihnya sebelum kebijakan Gizi dipilih. Bahan Radiologi belum diputuskan untuk ditagih tersendiri.

## 4. Rincian perbaikan

Semua bukti awal berada pada katalog E01–E28 di dokumen issue. Path BE berawalan `Areas/HealthServices/`; path FE berawalan `src/`.

### FIX-BKC-001-01 — Intervensi keperawatan

**Menutup:** ISS-01. **Area/jenis:** backend Clinical + Billing dan kontrak. **Berkas sasaran:** `ClinicalManagement/Services/NursingInterventionService.cs`, `BillingManagement/Operational/Constants/BillingSourceContract.cs`, jembatan/resolver Billing. **Radius dampak:** tindakan harian tunggal/batch, riwayat perawat, ringkasan Billing, kasir. **Dependency:** K-01 dan kontrak lintas pemilik.

**Langkah:** kunci pemetaan intervensi ke layanan/tarif; tambahkan penerbitan fakta pada milestone yang disetujui; teruskan melalui folio dan invoice; catat status pengiriman dan retry tanpa mengubah catatan klinis.

**Acceptance criteria:**

1. Satu tindakan eligible menghasilkan tepat satu charge terhubung ke sumber; kirim ulang tiga kali tidak menambah charge.
2. Dua intervensi berbeda masing-masing dapat ditelusuri ke item invoice, tanpa menagih ulang order prosedur yang mewakili layanan sama.
3. Gagal Billing meninggalkan catatan klinis dan status pending/failed yang dapat diproses ulang; nonbillable tidak masuk charge.

**Verifikasi:** pengujian integrasi producer → invoice setelah implementasi dan validasi backend yang diwajibkan task. **Risiko:** duplikasi antara intervensi, tindakan prosedur, dan tarif paket; gunakan K-01 untuk menutupnya.

### FIX-BKC-001-02 — Kontrak checkout Alkes

**Menutup:** ISS-02. **Area/jenis:** source FE Keperawatan. **Berkas sasaran:** `components/view/health-services/inpatient-management/nursing-workspace/sections/medication/panels/order-alat-kesehatan-panel.jsx` dan service Drug Usage bila pemetaan respons diperlukan. **Radius dampak:** tab Obat & Alkes → Order Alkes rawat inap/pulang; service Drug Usage juga dipakai menu Farmasi. **Dependency:** kontrak actor/lokasi Farmasi; tidak memerlukan FIX-03 untuk memperbaiki draft.

**Langkah:** kirim lokasi/petugas/satuan sah; pertahankan lifecycle draft dan record; hilangkan fallback menjadi resep pada sembarang error; simpan identitas/kunci draft untuk retry; tampilkan hasil sesuai langkah yang berhasil.

**Acceptance criteria:**

1. Keranjang tiga spuit dapat membuat draft dengan master sah, tanpa GUID kosong atau `consultationId` palsu.
2. Respons gagal tidak membuat resep alternatif; retry operasi yang sama tidak menggandakan draft.
3. Draft sukses tidak diklaim sebagai pemakaian atau tagihan selesai. Record hanya oleh actor yang memiliki permission existing.

**Verifikasi:** lint/unit/build sesuai aturan FE, lalu uji interaksi dengan API yang disetujui. **Risiko:** actor tidak memiliki Record; tangani handoff, jangan melemahkan permission.

### FIX-BKC-001-03 — Pemakaian tanpa resep/BMHP ke invoice

**Menutup:** ISS-02. **Area/jenis:** backend Farmasi + Billing, kontrak charge. **Berkas sasaran:** `PharmacyManagement/Services/DrugUsageService.cs`, kontrak sumber Billing, producer/resolver/jembatan existing. **Radius dampak:** menu Pemakaian Obat Farmasi, order BMHP perawat, stok, retur, invoice kasir. **Dependency:** K-04 dan kontrak tarif nonresep.

**Langkah:** tetapkan sumber yang merepresentasikan actual usage; emit setelah record stok sah; resolver harga per item; hubungkan retur/koreksi; gunakan retry durable. Batasi ke pemakaian nonresep agar dispense resep dan bahan operasi tidak ditagih lagi.

**Acceptance criteria:**

1. Pemakaian aktual tiga spuit × Rp5.000 menghasilkan Rp15.000 sebelum komponen lain, dengan tautan item sumber.
2. Draft belum menagih; replay record/dispatch tidak mengurangi stok atau menambah invoice kedua.
3. Dispense resep atau bahan operasi yang sudah mempunyai jalur charge tidak mendapat charge nonresep tambahan; koreksi/retur mengikuti kontrak yang dikunci.

**Verifikasi:** uji integrasi stok–fakta–invoice dan regresi dispense resep/operasi. **Risiko:** charge ganda dan retur tanpa sumber; periksa jenis usage dan identitas per item.

### FIX-BKC-001-04 — Consumer Bank Darah

**Menutup:** ISS-03. **Area/jenis:** backend Billing dan kontrak integrasi. **Berkas sasaran:** `BillingClinicalChargeBridgeService.cs`, `BillingSourceTariffResolver.cs`, `BillingChargeSourceAdapter.cs`, `BillingSourceContract.cs`; producer Bank Darah hanya jika kontrak membutuhkan field tambahan. **Radius dampak:** tindakan Bank Darah, status handoff, folio, ringkasan dan invoice kasir. **Dependency:** K-06.

**Langkah:** daftarkan sumber invoice dan lifecycle; gunakan fakta existing per tindakan; resolver membaca rujukan tarif sah; pastikan status pending/reconciliation, bukan NotApplicable untuk fakta eligible; bedakan status folio/invoice.

**Acceptance criteria:**

1. Tindakan selesai dengan dua kantong tetap mengikuti satu fakta per tindakan; nilai sesuai tarif yang disetujui.
2. Replay fakta tidak menghasilkan item kedua; kegagalan tarif tampil sebagai pengecualian yang dapat ditangani.
3. Klaim “masuk invoice” hanya diberikan setelah `InvoiceId`/item berhasil terbentuk.

**Verifikasi:** pengujian fakta Bank Darah → invoice dan regresi consumer/producer existing. **Risiko:** salah granularity per kantong; kontrak existing tetap otoritatif.

### FIX-BKC-001-05 — Satu jalur resep

**Menutup:** ISS-04. **Area/jenis:** backend Farmasi + Billing; kontrak racikan. **Berkas sasaran:** `ConsultationFinalizationService.cs`, `PrescriptionBillingChargeProducer.cs`, `BillingClinicalChargeBridgeService.cs`, `BillingSourceTariffResolver.cs`, `PrescriptionDispensingService.cs`. **Radius dampak:** finalisasi dokter rawat jalan/ranap/IGD, resep reguler/racikan/pulang, clearance Farmasi, dispense parsial, retur, rekonsiliasi kasir. **Dependency:** aturan rawat jalan/ranap existing; K-02 untuk racikan, K-05 untuk IGD.

**Langkah:** hapus duplikasi otoritas pengiriman dengan jalur canonical; jaga tahap prescribed rawat jalan dan dispensed rawat inap; jaga identitas/versi resep; tampilkan outcome sinkron yang sebenarnya. Dukungan racikan menjadi slice tersendiri setelah kontraknya disetujui; jangan mengganti ketiadaan kebijakan dengan total agregat langsung.

**Acceptance criteria:**

1. Finalisasi resep reguler rawat jalan menghasilkan satu item, tanpa konflik versi-sama-isi-berbeda dari pengirim kedua.
2. Finalisasi resep rawat inap yang belum diserahkan tidak membentuk charge obat; serah dua dari tiga vial menagih jumlah aktual sesuai kontrak.
3. Resep campuran reguler/racikan mempunyai satu perilaku yang disetujui: seluruh komponen dihitung benar atau ditahan secara jelas; tidak ada bypass lewat producer kedua.
4. Clearance, revisi jumlah, invoice final/adjustment, dan retur tetap dapat ditelusuri dan tidak membuat tagihan ganda.

**Verifikasi:** integrasi dua care setting, racikan, dispense parsial, retur, dan regresi clearance. **Risiko:** menghapus direct producer tanpa memastikan worker/fakta/jembatan berjalan menyebabkan hilang charge; implementasi wajib membuktikan jalur penggantinya sebelum selesai.

### FIX-BKC-001-06 — Kebijakan dan integrasi Gizi

**Menutup:** ISS-05. **Area/jenis:** kontrak bisnis + backend Gizi/Billing setelah disetujui. **Berkas sasaran:** kontrak Gizi/Billing, `NutritionManagement/Services/NutritionOrderService.cs` dan source/resolver Billing sesuai keputusan. **Radius dampak:** order konsultasi, kunjungan/asuhan, tagihan paket/kamar, kasir. **Dependency:** K-03.

**Langkah:** tetapkan apakah ada charge terpisah; jika ada, kunci milestone, granularity, tarif, gratis/paket dan pembatalan, lalu tambah producer/consumer. Jika sudah termasuk paket, dokumentasikan hasil itu tanpa membuat charge tambahan.

**Acceptance criteria:**

1. Dua kunjungan dalam satu episode mengikuti tepat kebijakan per-kunjungan/per-episode/paket yang dipilih.
2. Order dibatalkan sebelum pelayanan tidak menagih; layanan paket tidak menambah biaya kedua.
3. Charge terpisah, jika disetujui, sampai invoice dengan identitas sumber dan retry yang aman.

**Verifikasi:** contoh bisnis yang disetujui pemilik dan integrasi Gizi–invoice. **Risiko:** keputusan tarif yang belum ada; implementasi tidak boleh mendahului K-03.

### FIX-BKC-001-07 — Producer seluruh jalur tindakan Completed

**Menutup:** bagian producer ISS-06. **Area/jenis:** backend Clinical/IGD. **Berkas sasaran:** `ClinicalManagement/Controllers/PatientProcedureController.cs`, `Services/PatientProcedureOrderService.cs`, `Services/PatientProcedureExecutionService.cs`. **Radius dampak:** pembuatan tindakan langsung, execute medis, tindakan keperawatan IGD, penyelesaian dari OK. **Dependency:** kontrak milestone tindakan existing; cakupan invoice IGD mengikuti FIX-08.

**Langkah:** gunakan mekanisme penerbitan fakta yang sama untuk seluruh transisi eligible ke Completed; pastikan recovery untuk tindakan yang sudah selesai tetapi faktanya belum terbit. Recovery tidak melakukan eksekusi klinis kedua. Data lama hanya dipetakan; eksekusi backfill merupakan tindakan terpisah.

**Acceptance criteria:**

1. `ExecuteImmediately=true` menghasilkan satu fakta setelah commit; execute biasa tetap satu fakta.
2. Tindakan already executed tidak dijalankan ulang, tetapi fakta yang hilang dapat dipulihkan secara idempoten.
3. Order belum dikerjakan/FoC tidak menghasilkan charge pasien; kegagalan pengiriman tidak menghapus catatan klinis.

**Verifikasi:** seluruh entry point Completed, replay, dan simulasi pengiriman gagal. **Risiko:** perubahan short circuit dapat menggandakan fakta; pertahankan identitas sumber deterministik.

### FIX-BKC-001-08 — Cakupan invoice IGD

**Menutup:** bagian consumer IGD ISS-06. **Area/jenis:** kontrak lintas modul + backend Billing. **Berkas sasaran:** aturan encounter dalam jembatan, pemetaan layanan/transfer IGD–ranap serta kontrak yang dipilih owner. **Radius dampak:** tindakan, obat, Lab/Rad dan konsultasi IGD, invoice IGD, konsolidasi ranap. **Dependency:** K-05; FIX-07 untuk producer yang hilang.

**Langkah:** audit dan kunci pemilik invoice pada IGD/transfer; tambahkan cakupan sumber yang disetujui; selaraskan resep agar tidak memakai jalur berbeda tanpa aturan; pertahankan tautan encounter asli saat konsolidasi.

**Acceptance criteria:**

1. Pelayanan eligible pada encounter Emergency masuk ke invoice tujuan yang disetujui.
2. Transfer ke ranap tidak menagih pelayanan IGD dua kali dan tidak memindahkan kepemilikan sumber sembarangan.
3. Replay/late charge mengikuti invoice yang tepat, dengan adjustment bila sudah final.

**Verifikasi:** skenario IGD pulang dan IGD→ranap, termasuk obat/tindakan/penunjang. **Risiko:** salah tujuan atau double billing saat transfer; perubahan bukan sekadar mencabut filter Emergency.

### FIX-BKC-001-09 — Kekinian angka daftar kasir

**Menutup:** ISS-07. **Area/jenis:** backend Billing + kontrak respons bila dibutuhkan. **Berkas sasaran:** `BillingManagement/Billing/Services/BillingInvoiceService.cs`, DTO cashier, pembacaan calculation existing. **Radius dampak:** Invoice & Billing Kasir dan consumer lain DTO yang ditemukan saat implementasi; detail preview harus tetap konsisten. **Dependency:** K-07 untuk field/keadaan baru; tidak tergantung penyelesaian producer lain.

**Langkah:** deteksi kalkulasi yang lebih lama daripada item/kebijakan relevan; sediakan basis angka sah atau keadaan belum-terhitung; jangan pakai nol definitif untuk coverage yang belum diketahui; selaraskan total invoice, kewajiban pasien, dan pembayaran. Optimalkan pembacaan batch, jangan menulis kalkulasi per row pada GET.

**Acceptance criteria:**

1. Setelah kalkulasi Rp100.000 lalu charge Rp50.000, daftar menunjukkan nilai terbaru yang sah atau “Perlu hitung ulang”; tidak menampilkan Rp100.000 seolah terkini.
2. Penjamin tanpa kalkulasi tidak diberi sisa pasien nol sebagai hasil final; kasus coverage 100% yang benar-benar dihitung tetap boleh nol.
3. Setelah recalculate, angka daftar dan detail untuk kunjungan/versi sama konsisten; pagination tidak memicu penulisan transaksi.

**Verifikasi:** tambah charge setelah kalkulasi, ubah/void item, pasien tunai dan penjamin, bandingkan detail. **Risiko:** query mahal dan perbedaan formula coverage; backend tetap satu otoritas.

### FIX-BKC-001-10 — Penjelasan daftar kasir

**Menutup:** ISS-08. **Area/jenis:** FE Billing + desain. **Berkas sasaran:** `cashier-overview-view.jsx`, hook/service/constant cashier terkait kontrak baru. **Radius dampak:** daftar kasir dan uji/kolom tabelnya; tidak mengganti perhitungan backend. **Dependency:** K-07, FIX-09 untuk kekinian.

**Langkah:** gunakan label pembayaran yang tepat; sajikan total invoice terpisah dari sisa pasien; render null/stale sebagai keadaan yang dijelaskan; tampilkan sumber tertunda bila kontraknya tersedia. Gunakan base component existing.

**Acceptance criteria:**

1. Invoice Rp200.000 belum dibayar menampilkan Total Tagihan Rp200.000, Sudah Dibayar Rp0, dan sisa pasien sesuai coverage.
2. Invoice ditanggung penjamin penuh tetap membedakan total tagihan positif dari sisa pasien Rp0.
3. Memuat/gagal/belum dihitung tidak diubah menjadi Rp0; pengguna dapat membuka detail invoice yang sama.

**Verifikasi:** lint/unit/build FE dan pemeriksaan visual daftar pada keadaan di bagian 3.1. **Risiko:** perubahan makna status; update desain dan test existing bersama implementasi yang disetujui.

### FIX-BKC-001-11 — Penjaga sumber tertunda sebelum finalisasi

**Menutup:** ISS-T1. **Area/jenis:** backend Billing + kontrak readiness. **Berkas sasaran:** `BillingManagement/Billing/Services/BillingFinalizationService.cs` dan pembacaan status sumber/fakta/efek yang disetujui. **Radius dampak:** preview/finalisasi manual, finalisasi otomatis lewat pembayaran, clearance Farmasi/ranap, late charge dan adjustment. **Dependency:** definisi eligibility dan pengecualian tiap sumber; perlindungan sumber yang sudah didukung dapat dimulai sebelum seluruh perluasan selesai.

**Langkah:** bandingkan sumber eligible dengan penerimaan invoice, bukan hanya item yang sudah ada; tampilkan pending/failed/reconciliation yang belum diselesaikan; kecualikan nonbillable/tahap belum eligible secara eksplisit; sediakan alasan operasional yang terbaca.

**Acceptance criteria:**

1. Invoice dengan charge eligible berstatus Pending/Failed yang belum menjadi item tidak dapat dinyatakan lengkap pada preview/finalisasi normal.
2. Penyelesaian sinkron/rekonsiliasi sah menghapus penahan tanpa membuat charge kedua.
3. FoC/nonbillable dan tahap rawat inap yang belum eligible tidak mengunci invoice selamanya; kebijakan obat pulang/layanan susulan mengikuti keputusan existing yang diuji.
4. Finalisasi otomatis dan manual memakai pemeriksaan yang sama, tanpa membatalkan penyelesaian klinis.

**Verifikasi:** delay/failure pengiriman, retry, tarif hilang, unsupported yang eligible, gratis, dan regresi clearance. **Risiko:** memblokir permanen karena tidak membedakan “belum eligible” dengan “eligible belum masuk”; kontrak harus eksplisit.

## 5. Urutan pengerjaan

Pohon berikut menunjukkan dependency yang dipakai saat pengerjaan 12 Oktober 2026. Status terkini ada di register bagian 1: semua cabang selesai kecuali K-01 → FIX-01 (ditunda pemilik). FIX-05 dan FIX-08 dikerjakan pada gelombang yang sama karena penghapusan pengirim resep langsung hanya aman bila jembatan sudah mencakup IGD.

```text
Kontrak yang sudah berlaku
├── FIX-05  Satukan resep reguler dan aturan rawat jalan/rawat inap
│   └── K-02 → slice dukungan racikan pada FIX-05
├── FIX-07  Producer seluruh jalur Completed
└── FIX-11  Penjaga sumber yang sudah didukung (perluas saat kontrak baru tersedia)

K-07 (field/makna daftar kasir)
└── FIX-09  Kekinian proyeksi kasir
    └── FIX-10  Tampilan dan label kasir

Kontrak actor/lokasi Farmasi
└── FIX-02  Checkout Alkes yang sah

K-04 (BMHP terpisah vs paket)
└── FIX-03  Pemakaian nonresep → invoice
    └── Sambungkan status hasil ke FIX-02

K-01 → FIX-01  Intervensi keperawatan
K-06 → FIX-04  Bank Darah
K-03 → FIX-06  Gizi
K-05 + FIX-07 → FIX-08  Consumer invoice IGD/transfer

Seluruh producer/consumer selesai
└── Uji menyeluruh per kategori + finalisasi + anti-duplikasi
```

FIX-05, 07, 09, dan 11 merupakan prioritas awal karena memperbaiki jalur yang sudah tersedia dan mencegah informasi/penutupan tagihan yang keliru. Perluasan baru tidak menggantikan perbaikan itu.

## 6. Keputusan pemilik yang dibutuhkan

| ID | Keputusan | Rekomendasi | Perbaikan yang bergantung | Keputusan 12 Oktober 2026 |
| --- | --- | --- | --- | --- |
| K-01 | Titik tagih/tarif intervensi dan hubungannya dengan prosedur/paket | Tindakan aktual eligible, satu charge layanan | FIX-01 | **Ditunda** — `BKC-DEC-135`, pilihan pemilik |
| K-02 | Tarif, kuantitas serah, jasa racik, dan retur racikan | Kunci seluruh komponen sebelum membuka otomatisasi | Slice racikan FIX-05 | **Bahan × tarif obat, tanpa jasa racik; retur manual** — `BKC-DEC-136`, pilihan pemilik |
| K-03 | Gizi per kunjungan/episode/paket atau tidak terpisah | Pemilik menetapkan berdasarkan kebijakan rumah sakit | FIX-06 | **Tidak ditagih terpisah** — `BKC-DEC-137`, pilihan pemilik |
| K-04 | BMHP dan bahan Radiologi terpisah atau termasuk tarif | Hindari charge kedua atas bahan paket | FIX-03; perluasan Radiologi belum dialokasikan | **BMHP nonresep per butir saat dicatat**; bahan Radiologi tetap belum ditagih tersendiri — `BKC-DEC-138`, pilihan pemilik |
| K-05 | Invoice IGD, transfer, dan konsolidasi ranap | Pertahankan encounter asal dan tujuan invoice resmi | FIX-08; aturan resep IGD FIX-05 | **Invoice kunjungan IGD sendiri + tautan konsolidasi** — `BKC-DEC-139`, turunan `BKC-DEC-079`/`118`, `IGD-DEC-246`/`247` |
| K-06 | Perluasan fakta Bank Darah menjadi item invoice | Consumer otomatis per tindakan, sesuai fakta existing | FIX-04 | **Consumer otomatis per tindakan** — `BKC-DEC-140` |
| K-07 | Total invoice, pembayaran, dan keadaan kalkulasi pada daftar | Tiga makna terpisah, status kekinian jelas | Kontrak FIX-09 dan FIX-10 | **Empat kolom terpisah + status kekinian** — `BKC-DEC-141` |

Pertanyaan implementasi yang jawabannya sudah ada pada source/kontrak tidak perlu ditanyakan ulang. Task ID dan target branch ditetapkan saat task implementasi disetujui/didaftarkan, bukan pada audit ini.

## 7. Dokumen hulu yang ikut direvisi

Status 12 Oktober 2026: dokumen di tabel ini diberi catatan delta bertanggal yang menunjuk keputusan `BKC-DEC-135`..`141` dan laporan task. Isi lama tidak dihapus.

| Dokumen | Bagian | Sekarang | Usulan menjadi |
| --- | --- | --- | --- |
| `billing-kasir/03-frontend-architecture.md` | Daftar kasir | Outstanding/pembayaran tanpa penjelasan kekinian row | Total invoice, pembayaran, sisa pasien, serta belum-terhitung/stale sesuai K-07 |
| `billing-kasir/contracts/api-contract.md` | Cashier overview | Nilai angka, belum ada kontrak kekinian yang cukup pada source | Field/keadaan kekinian yang disepakati; backward compatibility dinilai |
| `billing-kasir/contracts/integration-contract.md` | Producer klinis | Belum lengkap untuk intervensi, nonresep, Bank Darah, Gizi, IGD | Milestone, granularity, source identity, tarif, retry, cancellation per sumber yang disetujui |
| `rawat-inap/keperawatan/contracts/integration-contract.md` | INT-KEP-05 | Menunggu kemampuan transaksi Billing; tetap Pending | Producer/consumer aktif dengan bukti invoice setelah K-01 |
| `rawat-inap/keperawatan/05-skema-tampilan.md` | Obat/Alkes, bila berkas ini menjadi desain canonical saat implementasi | Implementasi checkout saat ini memakai fallback request berbeda | Draft/order dan pemakaian aktual sesuai permission/kontrak, bukan fallback resep saat gagal |
| `rawat-jalan/contracts/integration-contract.md` | Jalur canonical resep | Satu jalur resmi fakta | Tegaskan larangan pengiriman paralel yang menghitung harga/waktu sendiri; racikan mengikuti K-02 |
| `rawat-inap/integrasi-billing/contracts/integration-contract.md` | INT-RWF-04 / obat ranap | Menagih saat diserahkan | Dipertahankan; implementasi direct producer diselaraskan, bukan aturan diubah agar cocok bug |
| `bank-darah/contracts/integration-contract.md` | Batas Billing | Sampai folio | Tambahkan consumer invoice dan status hasilnya setelah K-06 |
| `gizi/00-interview-decisions.md` dan kontraknya | Penagihan di luar scope | Billing belum mempunyai trigger | Keputusan K-03 dan ownership producer/consumer, tanpa mengarang tarif |
| Kontrak IGD/transfer yang ditunjuk owner | Tujuan invoice/konsolidasi | Jembatan umum mengecualikan Emergency | Kebijakan sumber/tujuan invoice yang dikunci K-05 |
| `billing-kasir/contracts/validation-matrix.md`, `state-transition-matrix.md` | Readiness/finalisasi | Tarif hilang dan kelengkapan item existing | Charge eligible tertunda menahan finalisasi dengan pengecualian yang sah |

Keberadaan/versi desain layar keperawatan dan kontrak IGD harus dikonfirmasi dari manifest aktif saat revisi; nama file usulan tidak menjadi bukti bahwa dokumen baru sudah tersedia.

## 8. Verifikasi menyeluruh

Prasyarat: environment uji yang disetujui; pengguna dokter/perawat/Farmasi/kasir dengan hak sah; master tarif/satuan/stok valid; kebijakan worker sinkron aktif; versi aplikasi dicatat. Pembuatan data uji/transaksi serta pelaksanaan query/migration mengikuti wewenang task terpisah. Data pasien memakai samaran.

| No. | Skenario | Bukti akhir wajib |
| --- | --- | --- |
| 1 | Dokter/perawat checkout dua tindakan; satu selesai, satu belum | Order belum selesai tidak ditagih; tindakan eligible mempunyai rantai sumber→fakta→invoice |
| 2 | Dua tindakan harian perawat billable | Setiap sumber muncul sekali di invoice sesuai tarif/paket yang dipilih |
| 3 | Resep reguler rawat jalan final | Satu charge tahap awal; tidak ada konflik pengirim kedua; clearance tetap berfungsi |
| 4 | Resep rawat inap harian/pulang | Belum dispense tidak menagih; jumlah aktual dispense menjadi nilai yang benar |
| 5 | Resep reguler + racikan | Semua komponen mengikuti K-02; tidak ada tagihan sebagian atau bypass tersembunyi |
| 6 | BMHP tiga spuit, lalu retry | Kontrak sah; draft berbeda dari record; stok dan invoice tidak ganda |
| 7 | Lab dan Radiologi | Order saja tidak menagih; milestone sah menagih; pengulangan kesalahan internal mengikuti pengecualian |
| 8 | Bank Darah dua kantong pada satu tindakan | Folio dan invoice dapat ditelusuri; tidak berubah menjadi dua fakta tanpa keputusan |
| 9 | Gizi dua kunjungan | Hasil sesuai K-03, termasuk kemungkinan tidak ada charge terpisah |
| 10 | Alat per penggunaan/jam/hari; kasus operasi dan bahan | Durasi, pembulatan, jumlah bahan terpakai, serta tariff missing benar |
| 11 | Hemodialisa final/stopped | Billable lewat tindakan; stopped mengikuti aturan yang disetujui |
| 12 | IGD pulang dan IGD→ranap | Semua eligible source memakai invoice tujuan benar; tidak ada tagihan transfer ganda |
| 13 | Tarif hilang, worker gagal, retry tiga kali | Klinis tetap sah; kasir melihat pending/pengecualian; finalisasi tertahan sesuai kontrak |
| 14 | Charge Rp50.000 setelah kalkulasi Rp100.000 | Daftar/detail memperlihatkan Rp150.000 yang sah atau status perlu hitung ulang; tidak seolah Rp100.000 terkini |
| 15 | Penjamin 100%, tunai, dan excess | Total invoice berbeda jelas dari pembayaran dan sisa pasien; nol hanya ketika memang sah |
| 16 | Retur satu dari tiga vial, invoice open/final | Revisi/adjustment benar, tidak membatalkan transaksi klinis atau menduplikasi sumber |

Untuk setiap skenario rekam identitas samaran kunjungan, ID sumber yang tidak dipublikasikan ke UI, ID/versi fakta, status efek, invoice/item/adjustment, versi kalkulasi, dan hasil kasir. Bukti “handoff diterima” saja tidak cukup sebagai bukti “muncul kasir”.

Uji unit terarah pada audit awal menghasilkan 38/39 lulus; satu test penghubung section tagihan perawat gagal. Sebelum sign-off, tentukan apakah desain menu sekarang memang memindahkan section tersebut atau source kehilangan menu. Jangan memperbaiki test hanya untuk meluluskan tanpa dasar desain. Unit test tidak menggantikan pembuktian transaksi di tabel ini.

## 9. Riwayat

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-10-12 | Sebelas usulan perbaikan dibuat; status seluruhnya belum dikerjakan; keputusan lintas domain dan verifikasi per kategori dicatat | diagnose-module-issue |
| 2026-10-12 | Rencana dikerjakan atas permintaan pemilik. K-01..K-04 diputuskan pemilik lewat pilihan interaktif, K-05..K-07 diturunkan dari keputusan/rekomendasi yang ada (`BKC-DEC-135`..`141`). FIX-02..05 dan 07..11 selesai lewat source (`BE-BKC-FIX-015`..`021`, `FE-BKC-FIX-012`..`013`), FIX-06 selesai lewat keputusan, FIX-01 ditunda. Dokumen hulu bagian 7 diberi catatan delta | Claude (build-module-backend/frontend) |
