# PLAN-REPAIR-008 — Rencana Rekonstruksi Transaksi To Billing Rawat Inap Tanpa Draf dan Tanpa Kunci

```yaml
plan_id: PLAN-REPAIR-DOK-008
issue: ../issue/issue-008-transaksi-to-billing-tanpa-draf.md
status_rencana: SELESAI
tanggal_rencana: "2026-10-20"
diputuskan_oleh: "User Directive & Antigravity Engineering"
tanggal_keputusan: "2026-10-20"
basis_source_backend: "6e0f2b1b58a2192a0f54f0fdff718bc77670e3bd (MHamzah)"
basis_source_frontend: "a491b05afcb6b3ed70218ccaa557007719d45ac3 (HamzahV2)"
```

---

## 1. Register Status Pengerjaan

| ID Perbaikan | Menutup | Ringkasan Tindakan | Area | Prioritas | Task ID | Status | Bukti |
| :--- | :--- | :--- | :--- | :---: | :---: | :--- | :--- |
| `FIX-DOK-008-01` | `ISS-DOK-008-01` | Inisialisasi resep rawat inap langsung `Submitted` / `QueuedAtPharmacy` dan buka `SubmitAsync` mandiri | Backend | 1 | — | SELESAI | `PrescriptionWorkflowService.cs:39-48, 81-105` |
| `FIX-DOK-008-02` | `ISS-DOK-008-02` | Emisi milestone `BilChargeLine` secara instan ke `BillingFolio` saat order resep rawat inap dibuat | Backend | 1 | — | SELESAI | `BillingClinicalChargeBridgeService.cs:263` & `PrescriptionController.cs:412-450` |
| `FIX-DOK-008-03` | `ISS-DOK-008-03` | Sediakan tombol aksi Batal / Hapus (*Void*) dengan modal alasan dan integrasi `CancelPrescription` | Frontend | 1 | — | SELESAI | `prescription.service.js:84-93`, `cancel-prescription-modal.jsx`, `prescription-history-panel.jsx:76-118` |
| `FIX-DOK-008-04` | `ISS-DOK-008-04` | Hapus tombol "Selesaikan & Kunci" dan badge "Draf" pada antarmuka dokter/keperawatan rawat inap | Frontend | 2 | — | SELESAI | `soap-editor.jsx:541-576`, `prescription-selected-items-card.jsx:220-252` |
| `FIX-DOK-008-05` | `ISS-DOK-008-05` | Resolusi tarif penunjang medis langsung dari pesanan bangsal (tanpa menunggu spesimen/study diterima) | Backend | 1 | — | SELESAI | `BillingSourceTariffResolver.cs:153-245` fallback ke `LabOrderedProcedure`/`RadOrder` |
| `FIX-DOK-008-06` | `ISS-DOK-008-06` | Emisi instan milestone penagihan penunjang medis (Laboratorium & Radiologi) saat dibuat dari rawat inap | Backend | 1 | — | SELESAI | `LabOrderService.cs` & `RadOrderService.cs` memancarkan `EmitChargeEligibilityAsync` |
| `FIX-DOK-008-07` | `ISS-DOK-008-07` | Penarikan penagihan otomatis (*reversal charge*) saat order laboratorium & radiologi rawat inap dibatalkan | Backend | 1 | — | SELESAI | `LabOrderService.cs:1764-1825` & `RadOrderService.cs:2380-2444` `EmitClinicalCancellationAsync` |
| `FIX-DOK-008-08` | `ISS-DOK-008-08` | Tombol "Batal" dan modal alasan pembatalan klinis pada riwayat penunjang medis bangsal | Frontend | 1 | — | SELESAI | `supporting-order-section.jsx:189-285`, `supporting-service-tab.jsx`, `use-inpatient-supporting-service.jsx` |

**Ringkasan: 8 dari 8 perbaikan selesai.**

---

## 2. Solusi Terpilih per Temuan

### ISS-DOK-008-01 — Resep Rawat Inap Tertahan di Status Draf dan Menolak Pengajuan Mandiri

| Opsi | Cara | Kelebihan | Kekurangan |
| :---: | :--- | :--- | :--- |
| **A (Terpilih)** | Modifikasi `PrescriptionWorkflowService.ApplyInitialClinicalState`: jika `entity.InpEpisodeId != null`, berikan status `PrescriptionStatus = Submitted` dan `FulfillmentStatus = QueuedAtPharmacy`. Izinkan `SubmitAsync` untuk konteks rawat inap. | Resep rawat inap langsung muncul di farmasi saat disimpan, tanpa perlu menunggu finalisasi atau penguncian konsultasi. Alur rawat jalan tetap terjaga. | Perlu memastikan farmasi siap menerima order rawat inap berstatus `QueuedAtPharmacy`. |
| **B** | Mengubah seluruh resep rumah sakit (termasuk rawat jalan) menjadi langsung `Submitted` | Seragam | Merusak alur poliklinik di mana pasien harus membayar di kasir sebelum obat disiapkan. |

**Solusi Terpilih: Opsi A.**

---

### ISS-DOK-008-02 — Resep Rawat Inap Tidak Muncul di Estimasi Biaya (Ditahan Status Dispensed)

| Opsi | Cara | Kelebihan | Kekurangan |
| :---: | :--- | :--- | :--- |
| **A (Terpilih)** | Hapus guard penahan rawat inap di `BillingClinicalChargeBridgeService.cs`: resep rawat inap pada tahap `PRESCRIBED` langsung diproses jembatan penagihan ke running invoice pasien (`BilInvoiceItem`) berstatus `PHARMACY`. | Seluruh obat yang diresepkan dokter bangsal langsung masuk ke live preview kasir / estimasi biaya pasien beserta kuantitas dan harga satuannya. | Jika resep dibatalkan sebelum dispensing, otomatis memicu pembalikan penagihan (`EmitClinicalCancellationAsync`). |
| **B** | Menunggu petugas farmasi menyerahkan obat fisik (*Dispensed*) baru menagihkan ke invoice | Penagihan hanya untuk obat fisik yang keluar | Pasien dan perawat tidak dapat melihat estimasi biaya obat berjalan secara real-time. |

**Solusi Terpilih: Opsi A.**

---

### ISS-DOK-008-03 — Penunjang Medis (Lab & Radiologi) Tidak Muncul di Estimasi Biaya

| Opsi | Cara | Kelebihan | Kekurangan |
| :---: | :--- | :--- | :--- |
| **A (Terpilih)** | 1. Injeksi `ClinicalMilestoneFactProducer` ke `LabOrderService` dan `RadOrderService` untuk memancarkan kelayakan tagih langsung saat order rawat inap dibuat.<br>2. Perluas `BillingSourceTariffResolver.cs` agar membaca ID prosedur dari `LabOrderedProcedure` atau `RadOrder` jika spesimen/study fisik belum diproses di laboratorium/radiologi. | Permintaan pemeriksaan darah dan foto rontgen langsung masuk kelompok `SUPPORT` pada Estimasi Biaya secara real-time saat dokter bangsal mengklik checkout order. | Membutuhkan penanganan pembatalan pesanan di tingkat order untuk membalikkan tagihan. |
| **B** | Menunggu analis lab menerima tabung darah (*SpecimenAccepted*) atau radiografer mengambil foto (*StudyQualityAccepted*) | Tagihan terbit setelah tindakan fisik | Melanggar instruksi CPOE to-billing real-time rawat inap; estimasi biaya bangsal menjadi kosong selama berjam-jam. |

**Solusi Terpilih: Opsi A.**

---

### ISS-DOK-008-04 — Ketiadaan Tombol Batal / Hapus pada Riwayat Transaksi

| Opsi | Cara | Kelebihan | Kekurangan |
| :---: | :--- | :--- | :--- |
| **A (Terpilih)** | Sediakan tombol aksi `Batal` pada tabel riwayat resep, tindakan, laboratorium, dan radiologi di antarmuka dokter/perawat rawat inap. Menampilkan modal alasan pembatalan klinis, lalu mengeksekusi endpoint pembatalan resmi yang memicu pembalikan penagihan (`EmitClinicalCancellationAsync`). | Tenaga medis memiliki kontrol penuh membatalkan salah input secara aman, diaudit, dan tagihan di kasir/estimasi biaya berkurang seketika (*Reversal Charge*). | Pesanan yang sudah diproses penuh atau selesai tidak dapat dibatalkan dari bangsal. |
| **B** | Menggunakan tombol hard delete (hapus permanen dari database) | Simpel | Dilarang keras oleh standar rekam medis dan integritas keuangan rumah sakit. Menghilangkan audit trail. |

**Solusi Terpilih: Opsi A.**

---

### ISS-DOK-008-05 — Pembersihan Tombol "Selesaikan & Kunci" dan Badge "Draf" di Rawat Inap

| Opsi | Cara | Kelebihan | Kekurangan |
| :---: | :--- | :--- | :--- |
| **A (Terpilih)** | Pada komponen form rawat inap (`QuilvianSystemFrontendDev`), lakukan kondisional: jika modul = rawat inap, sembunyikan tombol `Selesaikan & Kunci`. Hanya sediakan `Simpan Catatan Visite / CPPT`. Ganti badge `Draf` menjadi status operasional nyata (`Terkirim ke Farmasi`, `Diminta`). | Antarmuka bersih, ramah pengguna medis, dan menghilangkan risiko penguncian kunjungan pasien secara tidak sengaja. | Tidak ada. |

**Solusi Terpilih: Opsi A.**

---

## 3. Rencana Pengujian & Kriteria Keberterimaan (Acceptance Criteria)

1. **AC-DOK-008-01 (Pembuatan Resep Langsung Aktif)**:
   - Dokter membuat resep pada pasien rawat inap.
   - Resep tersimpan dengan status `Submitted` dan `QueuedAtPharmacy` (tidak berstatus `Draft` atau `WaitingForClinicalFinalization`).
2. **AC-DOK-008-02 (Tampil di Instalasi Farmasi)**:
   - Resep yang dibuat dokter langsung muncul di layar antrean farmasi rawat inap tanpa menunggu tindakan penguncian SOAP.
3. **AC-DOK-008-03 (Tampil di Estimasi Biaya)**:
   - Baris kamar, obat (`PHARMACY`), tindakan (`PROCEDURE`), dan penunjang medis (`SUPPORT`) langsung tampil di layar Estimasi Biaya rawat inap dengan rincian nama item, jumlah, tarif satuan, dan subtotal yang akurat.
4. **AC-DOK-008-04 (Pembatalan Transaksi & Reversal Finansial)**:
   - Tenaga medis menekan tombol "Batal" pada obat atau pemeriksaan penunjang (lab/radiologi) yang belum selesai dan mengisi alasan pembatalan.
   - Pesanan berubah menjadi `Cancelled`.
   - Saldo tagihan di Estimasi Biaya berkurang sesuai nilai item yang dibatalkan (`Reversal Charge`).
5. **AC-DOK-008-05 (Tanpa Tombol Kunci)**:
   - Form SOAP dan form perawat rawat inap tidak lagi menampilkan tombol "Selesaikan & Kunci". Kunjungan pasien tetap berstatus `Admitted`.
