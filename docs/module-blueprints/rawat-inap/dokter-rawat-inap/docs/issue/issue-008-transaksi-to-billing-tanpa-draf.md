# ISSUE-008 — Rekonstruksi Transaksi To Billing Rawat Inap: Penghapusan Siklus Draf, Pengiriman Instan ke Modul Terkait, dan Integrasi Estimasi Biaya Berjalan

```yaml
issue_id: ISSUE-DOK-008
module_id: rawat-inap
submodule: dokter-rawat-inap
layar: "Dokter & Keperawatan Rawat Inap — Tab Resep, Tindakan, Order Penunjang (Lab & Rad), dan Estimasi Biaya"
sumber_laporan: "Arahan dan laporan pengguna 20-10-2026: Di rawat inap tidak ada draf dan tidak ada selesaikan & kunci. Seluruh item yang di-checkout/ditambahkan langsung masuk ke modul masing-masing (misal resep langsung terlihat di farmasi, penunjang langsung terlihat di lab/rad) dan langsung masuk estimasi biaya beserta harga. Penanganan salah menggunakan tombol batal atau hapus."
tanggal_issue: "2026-10-20"
status: SELESAI
keparahan_tertinggi: Blocker
basis_source_backend: "6e0f2b1b58a2192a0f54f0fdff718bc77670e3bd (MHamzah)"
basis_source_frontend: "a491b05afcb6b3ed70218ccaa557007719d45ac3 (HamzahV2)"
rencana_perbaikan: ../plan-repair/plan-repair-008-transaksi-to-billing-tanpa-draf.md
ditulis_dengan: diagnose-module-issue
```

---

## 1. Ringkasan

Berdasarkan arahan bisnis dan operasional rumah sakit dari pengguna, sistem transaksi pelayanan pada **Dokter dan Keperawatan Rawat Inap (*Inpatient*)** memiliki aturan operasional yang berbeda secara fundamental dari Rawat Jalan (*Poliklinik*):

1. **Penghapusan Konsep Draf & Penguncian**: Tidak boleh ada sistem "Draf" yang menahan pesanan, dan tidak boleh ada tombol "Selesaikan & Kunci" yang mematikan episode perawatan rawat inap.
2. **Pengiriman Instan ke Modul Tujuan (CPOE Real-Time)**: Setiap item yang ditambahkan atau di-checkout oleh dokter atau perawat (contoh: resep obat, permintaan alkes, order laboratorium, atau order radiologi) harus **detik itu juga terkirim dan terlihat di modul masing-masing** (contoh: resep langsung muncul di antrean Instalasi Farmasi agar segera diracik/disiapkan; pesanan lab langsung masuk di antrean analis lab; pesanan radiologi langsung masuk jadwal modalitas).
3. **Pencatatan Instan ke Estimasi Biaya (Real-Time Running Invoice & Live Preview)**: Setiap item yang masuk ke riwayat transaksi harus langsung memunculkan rincian biaya (nama item, kuantitas, harga per unit, dan total subtotal) pada layar **Estimasi Biaya Pasien** (kelompok `ROOM`, `PHARMACY`, `PROCEDURE`, dan `SUPPORT`), untuk keperluan pengendalian plafon jaminan (BPJS / asuransi swasta) dan transparansi biaya pasien.
4. **Penanganan Kesalahan Menggunakan Batal / Hapus (Clinical Void & Charge Reversal)**: Jika dokter atau perawat salah menginput item, transaksi ditangani dengan menekan tombol **Batal / Hapus**, disertai pengisian alasan pembatalan klinis. Sistem secara otomatis membatalkan pesanan di modul tujuan dan membalikkan (*reverse*) saldo tagihan di billing invoice kasir / estimasi biaya secara otomatis.

---

## 2. Ringkasan Temuan

| ID | Judul Temuan | Jenis | Area | Keparahan | Status Bukti | ID Perbaikan |
| :--- | :--- | :--- | :--- | :---: | :--- | :--- |
| `ISS-DOK-008-01` | Resep rawat inap tertahan di status `Draft` dan menolak pengajuan mandiri (`SubmitAsync` diblokir) | `BUG` | Backend | Blocker | `SUDAH-VERIFIKASI` | `FIX-DOK-008-01` |
| `ISS-DOK-008-02` | Item resep obat rawat inap ditahan dari Estimasi Biaya sebelum dispensing fisik farmasi | `BUG` | Backend | High | `SUDAH-VERIFIKASI` | `FIX-DOK-008-02` |
| `ISS-DOK-008-03` | Penunjang medis (Laboratorium & Radiologi) tidak langsung masuk ke Estimasi Biaya (menunggu sampel/foto selesai) | `BUG` | Backend | High | `SUDAH-VERIFIKASI` | `FIX-DOK-008-05` & `FIX-DOK-008-06` |
| `ISS-DOK-008-04` | Ketiadaan tombol Batal / Hapus (*Void*) pada baris transaksi aktif penunjang medis di antarmuka dokter rawat inap | `BUG` | Frontend | Blocker | `SUDAH-VERIFIKASI` | `FIX-DOK-008-08` |
| `ISS-DOK-008-05` | Ketiadaan mekanisme pembalikan tagihan otomatis (*reversal charge*) saat order laboratorium & radiologi dibatalkan | `BUG` | Backend | High | `SUDAH-VERIFIKASI` | `FIX-DOK-008-07` |
| `ISS-DOK-008-06` | Antarmuka rawat inap menampilkan tombol "Selesaikan & Kunci" poliklinik dan badge "Draf" yang membingungkan | `DEFECT` | Frontend | High | `SUDAH-VERIFIKASI` | `FIX-DOK-008-04` |

---

## 3. Rincian Temuan dan Solusi

### 1. Resep Obat Rawat Inap (Prescription CPOE)
- **Kondisi Lama**: Resep tertahan di `Draft` (`WaitingForClinicalFinalization`) dan jembatan billing menahannya dengan aturan *"Resep rawat inap menunggu obat diserahkan sebelum ditagihkan."*
- **Kondisi Baru**: Resep rawat inap langsung terbit sebagai `Submitted` dan `QueuedAtPharmacy`. Jembatan billing (`BillingClinicalChargeBridgeService.cs`) langsung menerbitkan `BilInvoiceItem` berstatus `PHARMACY` dengan kuantitas dan harga satuan tertera di Estimasi Biaya.

### 2. Penunjang Medis (Laboratorium & Radiologi)
- **Kondisi Lama**: Tagihan ditahan hingga spesimen darah diterima analis (`SpecimenAccepted`) atau foto radiologi selesai (`StudyQualityAccepted`). Resolver tarif melempar `SourceNotFound` jika diperiksa saat masih tahap order.
- **Kondisi Baru**: `BillingSourceTariffResolver.cs` memiliki fallback membaca tarif langsung dari `LabOrderedProcedure` atau `RadOrder`. Constructor `LabOrderService` dan `RadOrderService` diinjeksi `ClinicalMilestoneFactProducer` sehingga pemesanan bangsal langsung memancarkan fakta penagihan ke kelompok `SUPPORT`.

### 3. Pembatalan Transaksi Medis (*Reversal Charge*)
- **Kondisi Lama**: Tidak ada tombol Batal pada pesanan lab dan radiologi bangsal. Pembatalan di backend tidak menarik tagihan rawat inap yang sudah terbit.
- **Kondisi Baru**:
  - Frontend: `supporting-order-section.jsx` dan `supporting-service-tab.jsx` menampilkan tombol outline merah "Batal" pada order berstatus aktif (`Requested`/`Confirmed`/`Draft`/`Scheduled`). Menampilkan dialog modal alasan pembatalan klinis.
  - Backend: Method `CancelAsync` pada `LabOrderService.cs` dan `RadOrderService.cs` memancarkan `EmitClinicalCancellationAsync` yang seketika membatalkan tagihan terkait di running invoice kasir dan mengurangi total estimasi biaya rawat inap.
