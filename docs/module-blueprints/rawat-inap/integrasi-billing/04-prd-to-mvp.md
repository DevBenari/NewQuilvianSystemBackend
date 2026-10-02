# PRD ke MVP — Integrasi Rawat Inap ↔ Kasir / Billing (`INP-S22`)

| Field | Nilai |
|---|---|
| Blueprint ID | `RWI-BP-001-INT-BIL` |
| Sub-modul | `integrasi-billing` (Slice `INP-S22`) |
| Versi Dokumen | `1.0.0` (Draft) — 17 September 2026 |
| Target Rilis | MVP Integrasi Rawat Inap & Billing V2 |
| Penanggung Jawab | Muhammad Hamzah (Rawat Inap) & Yasmina (Billing) |
| Dokumen Sumber | `PRD Integrasi-Rawat-Inap-dengan-Billing.md`, `00-interview-decisions.md` revision 25, `01-existing-capability-map.md` Bagian 18, `02-requirement-completeness-gate.md` Bagian 16 |

---

## 1. Batas Rilis Pertama (MVP Boundary)

Batas rilis pertama untuk Integrasi Rawat Inap ↔ Billing didefinisikan secara tegas:

- **Titik Mulai (Start Boundary):** Pasien Rawat Inap telah dikonfirmasi status admisi menjadi `Admitted` di bangsal rawat inap, memicu pembuatan/penautan folio tagihan terbuka di kasir dan inisiasi perhitungan sewa kamar harian saat tempat tidur berstatus `Bed Occupied` secara fisik.
- **Titik Akhir (End Boundary):** Pasien telah menyelesaikan pelunasan/clearance di kasir, perawat mengonfirmasi pasien keluar bangsal secara fisik (`PhysicallyLeftAt`), durasi sewa kamar final dihitung presisi, dan status invoice kasir dikunci menjadi `CLOSED`. Termasuk di dalamnya penanganan kondisi darurat klinis melalui mekanisme *Supervisor Override* yang terdokumentasi rapi.

---

## 2. Kemampuan Wajib Ada (MUST HAVE)

Seluruh 6 kemampuan integrasi kanonik masuk ke dalam cakupan MVP:

| ID Kemampuan | Nama Kemampuan | Disposisi & Status Map | Peran Modul Rawat Inap |
|---|---|:---:|---|
| **`INT-CAP-01`** | **Sinkronisasi Admisi `Admitted` & Pembuatan Folio** | `EXTEND` | Menerbitkan event outbox `ADMISSION_CONFIRMED` saat admisi disahkan; modul kasir otomatis membuat `BillingFolio` berstatus `OPEN`. |
| **`INT-CAP-02`** | **Standardisasi Occupancy & Room Charge Fisik** | `EXTEND` | Menjamin room charge hanya aktif saat `Bed Occupied` fisik; mencatat `OccupancyStartAt` dan `OccupancyEndAt = PhysicallyLeftAt` presisi. |
| **`INT-CAP-03`** | **Mutasi & Koreksi Occupancy saat Billing `OPEN`** | `NEW / MISSING` | Memungkinkan Supervisor mengoreksi kelas/kamar saat status tagihan kasir masih `OPEN`; menerbitkan event `OCCUPANCY_CORRECTED` tanpa hard delete; menolak jika status `CLOSED`. |
| **`INT-CAP-04`** | **Ringkasan Tagihan Bangsal Tanpa Rupiah** | `EXTEND` | Menyajikan status operasional penagihan dan kendala blocker di layar perawat tanpa menampilkan nominal rupiah ([`RWI-DEC-160`](file:///C:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/NewQuilvianSystemBackend/docs/module-blueprints/rawat-inap/00-interview-decisions.md)). |
| **`INT-CAP-05`** | **Gerbang Pelepasan, Clearance, & Auto-Reblock** | `REPAIR & EXTEND` | Mengunci pemulangan fisik sebelum ada clearance kasir; mengunci kembali secara otomatis (*Auto-Reblock*) bila clearance dicabut; menyediakan *Supervisor Override* darurat klinis. |
| **`INT-CAP-06`** | **Transactional Outbox & Idempotensi Pesan** | `REUSE WITH ADAPTER & NEW` | Tabel `InpIntegrationOutbox`, format compound key idempoten unik, worker pengirim berulang (*exponential backoff*). |

---

## 3. Kemampuan yang Ditunda (DEFERRED to POST-MVP)

| Fitur yang Ditunda | Alasan Penundaan | Solusi Sementara Selama MVP |
|---|---|---|
| **Push Notifikasi Real-Time via WebSocket / SignalR** | Menghindari kompleksitas infrastruktur server socket terpusat pada rilis awal. | Menerapkan *smart polling* berkala (interval 30 detik pada detail episode, 10 detik pada modal discharge) yang ringan dan andal. |
| **Integrasi Klaim BPJS / Asuransi Lanjutan Otomatis** | Domain modul eksternal penjamin; aturan klaim VClaim dipegang modul Billing/Asuransi. | Status kelayakan asuransi dimasukkan sebagai hasil akhir clearance oleh staf kasir di loket billing. |
| **Kompensasi Otomatis Transfer Kamar > 2 Kali Sehari** | Kasus multiple transfer (A → B → C) sangat jarang terjadi (< 0.5% admisi) dan kebijakan tarifnya belum disahkan manajemen (`DEC-INT-004`). | Ditangani via penyesuaian (*adjustment*) manual oleh staf kasir di modul Billing. |

---

## 4. Daftar Epic dan Persyaratan Fungsional (FR)

### EPIC 1: Sinkronisasi Admisi & Inisiasi Folio Kasir (`EPIC-INT-01`)
- **`FR-INT-001`**: Sistem rawat inap wajib mencatat status `Admitted` saat pasien resmi diterima di ruangan rawat inap.
- **`FR-INT-002`**: Perubahan ke status `Admitted` wajib menyimpan event outbox `ADMISSION_CONFIRMED` ke tabel `InpIntegrationOutbox` dalam transaksi database yang sama.
- **`FR-INT-003`**: Sistem dilarang menerbitkan event tagihan aktif untuk pasien yang masih berstatus `Booking`, `Draft`, atau `Cancelled`.

### EPIC 2: Standardisasi Hunian Bed & Room Charge Fisik (`EPIC-INT-02`)
- **`FR-INT-004`**: Konfirmasi penempatan tempat tidur fisik oleh perawat wajib mencatat `OccupancyStartAt` dan menerbitkan event outbox `BED_OCCUPIED`.
- **`FR-INT-005`**: Durasi akhir sewa kamar (`OccupancyEndAt`) wajib dipatok dari waktu pelepasan bed / kepergian fisik (`PhysicallyLeftAt`), bukan dari saat dokter DPJP menerbitkan instruksi pulang (`DischargeRequested`).
- **`FR-INT-006`**: Pelepasan fisik pasien wajib menerbitkan event outbox `BED_RELEASED` dengan memuat `PhysicallyLeftAt` presisi detik.

### EPIC 3: Mutasi Kamar & Koreksi Occupancy saat Billing `OPEN` (`EPIC-INT-03`)
- **`FR-INT-007`**: Koreksi tempat tidur, kelas perawatan, atau durasi hunian hanya diperbolehkan apabila status folio kasir pasien terverifikasi masih `OPEN`.
- **`FR-INT-008`**: Sistem wajib menolak mutasi kamar langsung di rawat inap jika tagihan kasir telah berstatus `CLOSED` atau `LOCKED`.
- **`FR-INT-009`**: Setiap koreksi kamar wajib dilakukan oleh peran Supervisor, wajib menginput alasan tertulis, dan dilarang menghapus data lama (*soft-supersede* berversi baru).
- **`FR-INT-010`**: Koreksi kamar yang sah wajib menerbitkan event outbox `OCCUPANCY_CORRECTED` ke modul Billing untuk kalkulasi penyesuaian tarif (*repricing*).

### EPIC 4: Tampilan Status Operasional Kasir Steril Nominal Rupiah (`EPIC-INT-04`)
- **`FR-INT-011`**: Antarmuka bangsal perawat (sensus, detail episode, modal discharge) dilarang menampilkan nominal saldo rupiah, tagihan berjalan, atau kekurangan bayar.
- **`FR-INT-012`**: Layar bangsal hanya menampilkan lencana status operasional kasir (`Tagihan Berjalan`, `Menunggu Kasir`, `Clearance Disetujui`, `Clearance Dicabut`) beserta string alasan kendala blocker.
- **`FR-INT-013`**: Panel rincian finansial lengkap dengan nominal rupiah hanya dapat dibuka oleh pengguna yang memiliki permission eksplisit `InpatientBilling:View`.

### EPIC 5: Gerbang Pelepasan Pasien, Auto-Reblock, & Supervisor Override (`EPIC-INT-05`)
- **`FR-INT-014`**: Tombol `Konfirmasi Pasien Pulang Fisik` di layar perawat wajib dinonaktifkan (*disabled*) selama status clearance kasir belum `Cleared`.
- **`FR-INT-015`**: Bila status clearance kasir dicabut (`ClearanceRevoked`), sistem wajib melakukan *Auto-Reblock* seketika dan menampilkan banner peringatan merah di layar bangsal.
- **`FR-INT-016`**: Supervisor Bangsal dapat melakukan *Supervisor Override* untuk kondisi kedaruratan klinis / evakuasi ambulans rujukan dengan menginput alasan klinis wajib dan PIN otorisasi.

### EPIC 6: Transactional Outbox & Ketahanan Idempotensi (`EPIC-INT-06`)
- **`FR-INT-017`**: Seluruh pesan integrasi wajib disimpan di tabel `InpIntegrationOutbox` dengan `IdempotencyKey` gabungan `SourceDomain:SourceType:SourceDetailId:Version`.
- **`FR-INT-018`**: Background worker wajib melakukan retry otomatis dengan interval exponential backoff jika terjadi kegagalan jaringan atau downtime pada modul Billing.

---

## 5. Skenario Pengujian Penerimaan Pengguna (UAT)

### UAT-01: Alur Admisi Masuk dan Inisiasi Tagihan (Happy Path)
- **Kondisi Awal:** Pasien baru terdaftar di admisi rawat inap.
- **Aksi:** Staf Admisi konfirmasi admisi pasien ke status `Admitted`.
- **Hasil yang Diharapkan:**
  1. Record `InpAdmission` berstatus `Admitted`.
  2. Record `InpIntegrationOutbox` terbentuk dengan `EventType = ADMISSION_CONFIRMED`.
  3. Worker mengirim event ke Billing; Modul Billing membuat `BillingFolio` berstatus `OPEN`.
  4. Tidak ada tagihan sewa kamar yang dihitung sebelum pasien menempati tempat tidur.

### UAT-02: Mutasi Kamar saat Billing `CLOSED` (Negative / Rejection Path)
- **Kondisi Awal:** Tagihan pasien di kasir telah berstatus `CLOSED` menjelang pemulangan.
- **Aksi:** Perawat mencoba melakukan mutasi kamar pasien ke ruangan lain.
- **Hasil yang Diharapkan:**
  1. Sistem rawat inap menolak aksi mutasi.
  2. Muncul pesan error validasi: *"Mutasi kamar ditolak: Tagihan kasir pasien sudah berstatus CLOSED. Hubungi bagian Kasir untuk pembukaan tagihan."*
  3. Data penempatan tempat tidur tidak berubah.

### UAT-03: Pencabutan Clearance Kasir (*Auto-Reblock*)
- **Kondisi Awal:** Pasien berstatus `Clearance Disetujui` (Hijau); tombol pulang fisik aktif.
- **Aksi:** Kasir mencabut clearance karena ada resep obat susulan dari Farmasi.
- **Hasil yang Diharapkan:**
  1. Layar perawat mendeteksi sinyal `ClearanceRevoked`.
  2. Lencana berubah merah `Clearance Dicabut` dan banner peringatan muncul.
  3. Tombol `Konfirmasi Pasien Pulang Fisik` seketika terkunci (*disabled*).
  4. Perawat tidak dapat memulangkan pasien.

### UAT-04: Eksekusi *Supervisor Override* untuk Rujukan Gawat Darurat
- **Kondisi Awal:** Pasien terblokir oleh *Auto-Reblock* pada UAT-03, namun dokter menginstruksikan rujukan darurat segera dengan ambulans.
- **Aksi:** Supervisor Bangsal menekan tombol `Supervisor Override`, mengisi alasan kedaruratan, dan memasukkan PIN otorisasi.
- **Hasil yang Diharapkan:**
  1. Sistem menerima override dan mengaktifkan tombol pelepasan darurat.
  2. Pasien berhasil dipulangkan fisik (`PhysicallyLeftAt = Sekarang`).
  3. Tempat tidur berubah menjadi `Perlu Pembersihan`.
  4. Jejak audit mencatat: ID Supervisor, timestamp, alasan darurat, dan status `Discharged via Supervisor Override`.
  5. Event outbox `BED_RELEASED` dikirim ke Billing untuk memfinalisasi sewa kamar fisik.

---

## 6. Definition of Done (DoD)

Sub-modul Integrasi Rawat Inap ↔ Billing (`INP-S22`) dinyatakan selesai jika seluruh butir berikut terpenuhi:

- [ ] **Data Model:** Tabel `InpIntegrationOutbox` terbuat via migrasi EF Core dengan index unik `IdempotencyKey` dan index status pending.
- [ ] **Standardisasi Occupancy:** Entitas hunian tempat tidur mencatat `OccupancyStartAt`, `OccupancyEndAt`, `PhysicallyLeftAt`, dan `Version` tanpa hard delete.
- [ ] **Transactional Consistency:** Penulisan event outbox dilakukan dalam transaksi database yang sama dengan mutasi bisnis rawat inap.
- [ ] **Background Resilience:** Worker outbox beroperasi di background dengan exponential backoff dan deteksi kegagalan jaringan.
- [ ] **UI Steril Rupiah:** Layar bangsal perawat terbukti tidak menampilkan angka saldo rupiah pada pengetesan pengguna perawat.
- [ ] **Clearance Gate & Auto-Reblock:** Tombol kepulangan terkunci saat belum lunas, aktif saat clearance disetujui, dan terkunci kembali seketika saat clearance dicabut.
- [ ] **Supervisor Override:** Alur darurat medis berfungsi penuh dengan verifikasi alasan wajib dan audit log immutable.
- [ ] **Kontrak Swagger:** Seluruh endpoint baru memiliki anotasi tag `[Tags(...)]`, deskripsi Swagger, dan DTO terstruktur.
- [ ] **Automated Tests:** Seluruh unit test dan integration test untuk outbox, clearance gate, dan override lolos 100%.

---

## 7. Urutan Gelombang Pengiriman (Delivery Waves)

1. **Gelombang MVP-0 (Fondasi Data & Outbox Engine):**
   - Migrasi tabel `InpIntegrationOutbox` dan field tracking `InpBedPlacement`.
   - Implementasi `InpIntegrationOutboxService` dan `InpatientIntegrationOutboxWorker`.
   - Unit test ketahanan outbox dan idempotency key.
2. **Gelombang MVP-1 (Integrasi Admisi, Hunian Bed, & Mutasi):**
   - Penerbitan event `ADMISSION_CONFIRMED` saat status admisi `Admitted`.
   - Penerbitan event `BED_OCCUPIED` saat penempatan fisik.
   - Fitur mutasi & koreksi kamar beralasan dengan validasi status kasir `OPEN`.
3. **Gelombang MVP-2 (Antarmuka Bangsal & Status Kasir Bebas Rupiah):**
   - Implementasi `InpatientBillingOperationalController` (status kasir tanpa rupiah vs finansial berizin).
   - Pembuatan komponen UI: `BillingStatusBadge.jsx` dan `BillingSummaryCard.jsx`.
   - Polling status kasir di halaman sensus dan detail episode rawat inap.
4. **Gelombang MVP-3 (Gerbang Pemulangan, Auto-Reblock, & Supervisor Override):**
   - Implementasi webhook sinyal clearance kasir di `InpatientDischargeClearanceController`.
   - Logika penguncian tombol pemulangan fisik (*Discharge Gate*) dan *Auto-Reblock*.
   - Komponen modal `SupervisorOverrideModal.jsx` dan pencatatan audit darurat.
   - Penerbitan event `BED_RELEASED` saat pasien meninggalkan ruangan fisik (`PhysicallyLeftAt`).
5. **Gelombang MVP-4 (Verifikasi End-to-End & UAT Bersama Kasir):**
   - Eksekusi pengujian bersama tim Billing (`UAT-INT-001` s.d. `013`).
   - Penandatanganan berita acara kesiapan integrasi oleh kedua Product Owner.
