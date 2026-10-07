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

---

## 8. Amandemen kontrak `1.1.0` — PRD → MVP Finishing ★ 1 Oktober 2026

Bagian ini menurunkan isi dari `02-backend-architecture.md` 9, `contracts/` bagian `1.1.0`, `data/data-dictionary.md` 6, dan `flowcharts/04` s.d. `06`. Ia tidak menciptakan entity, status, permission, atau endpoint baru. Bagian 1 s.d. 7 di atas tetap sebagai jejak; butir yang dicabut `02-backend-architecture.md` 9.1 tidak berlaku.

### 8.1 Identitas dokumen

| Field | Nilai |
|---|---|
| Produk | Quilvian — Rawat Inap, sub-modul `integrasi-billing` |
| Status | **`approved`** — Muhammad Hamzah, 2026-10-02 (`RWI-DEC-221`) |
| Repository | `NewQuilvianSystemBackend` (`MHamzah`), `QuilvianSystemFrontendDev` (`HamzahV2`) |
| Baseline | Backend `c8e99ce5` (HEAD `425cfeae` hanya dokumen); frontend `22ad67330` (HEAD `ee75e055b`) |
| Masukan | `PRD-RWI-FINISHING-001` v`0.4`; decision log revision `30`; gate `1.9` |
| Cakupan | Tagihan rawat inap masuk kasir secara otomatis dan jujur; izin kasir satu sumber yang menjaga penutupan episode; rincian Tagihan Pasien tanpa rupiah bagi perawat |

### 8.2 Ringkasan eksekutif

Setiap layanan pasien rawat inap muncul di invoice kasir tanpa diketik ulang, tarif kamar dihitung dari jam masuk dan keluar yang sebenarnya, dan episode baru dapat ditutup setelah kasir memberi izin. Pasien tetap boleh meninggalkan ruangan setelah dokter memutuskan pulang; bila kasir belum memberi izin, perawat diperingatkan dan jejaknya tersimpan.

### 8.3 Masalah produk

Lihat `FIN-CAP-01` s.d. `FIN-CAP-15`. Ringkasnya: pengirim event hanya menulis log tetapi menandai "Published"; invoice rawat inap tidak pernah terbentuk otomatis; jembatan klinis menolak kunjungan rawat inap; status kasir tersimpan di tiga tempat; webhook dapat dipanggil tanpa login; PIN supervisor tidak pernah diperiksa; ada hitungan tarif kamar kedua dengan angka tertanam; dan Rawat Inap membaca tabel Billing langsung.

### 8.4 Visi produk

1. Admisi disahkan → ketukan pintu → invoice `RANAP` terbuka.
2. Bed ditempati, dipindah, dikoreksi, dilepas → ketukan pintu → tarif kamar dihitung ulang dari linimasa.
3. Tindakan, lab, radiologi, obat, alat, operasi → jembatan folio → baris invoice.
4. Perawat mencatat keluar ruangan → bed kosong → tarif kamar final.
5. Kasir memfinalkan dan memberi izin → petugas admisi menutup episode.

### 8.5 Batas MVP

**Titik mulai.**

1. Episode berstatus `Admitted`.
2. Bed pertama ditempati.

**Titik akhir.**

1. Pasien sudah keluar ruangan dan bed kosong.
2. Invoice `RANAP` memuat seluruh layanan, tarif kamar final, dan biaya administrasi.
3. Izin kasir `CLEARED`, atau supervisor menutup dengan alasan.
4. Episode `Closed`.

### 8.6 Pelaku sasaran

| Pelaku | Tanggung jawab di dalam MVP |
|---|---|
| Perawat pelaksana, kepala ruangan | Mencatat keluar ruangan; kepala ruangan mengoreksi salah catat penempatan |
| Petugas admisi | Menutup episode; mengoreksi penempatan |
| Supervisor rawat inap | Menutup tanpa izin kasir dengan alasan |
| Kasir | Memfinalkan invoice, memberi atau mencabut izin, memeriksa invoice "perlu diperiksa" |
| Tim TI | Memantau outbox dan menjalankan putar ulang dengan wewenang tertulis |

### 8.7 Pemilihan kemampuan MVP

| Kemampuan | ID kemampuan asal | Keputusan MVP |
|---|---|---|
| Satu tindakan keluar ruangan dengan peringatan dan jejak | `CAP-RWF-01`, `FIN-CAP-09`, `FIN-CAP-10` | Wajib; tanpa ini bed tidak dapat dilepas dengan aturan yang benar |
| Penutupan episode yang membaca Billing langsung; override dengan permission | `CAP-RWF-01`, `CAP-RWF-03`, `FIN-CAP-07`, `FIN-CAP-11` | Wajib; gerbang keuangan episode |
| Ketukan pintu jujur dan penerima di Billing | `CAP-RWF-04`, `FIN-CAP-01` | Wajib; tanpa ini invoice tidak terbentuk |
| Invoice `RANAP` otomatis, biaya admin, jembatan klinis `RANAP`, label seragam | `CAP-RWF-02`, `FIN-CAP-02`, `04`, `05`, `08` | Wajib; tanpa ini kasir mengetik ulang |
| Pensiun hitungan tarif kamar kedua | `CAP-RWF-04`, `FIN-CAP-06` | Wajib; mencegah tagihan karangan |
| Koreksi salah catat penempatan | `CAP-RWF-04`, `FIN-CAP-15` | Wajib; tanpa ini periode salah kelas tetap tertagih |
| Putar ulang saat rilis | `CAP-RWF-04`, `FIN-CAP-14` | Wajib; pasien yang sedang dirawat saat rilis harus punya invoice |
| Rincian Tagihan Pasien tanpa rupiah dan hak lihat rupiah berbasis permission | `CAP-RWF-05` (sisi backend), `CAP-RWF-15`, `FIN-CAP-13` | Wajib; layar bangsal bergantung padanya |
| Retur obat membatalkan tagihan | `CAP-RWF-02`, `RWI-DEC-195` | Wajib; disetujui Ikbal Yulianto (pemilik Farmasi) lewat `RWI-DEC-210` |

### 8.8 Kemampuan yang ditunda

| Kemampuan | ID | Alasan ditunda | Pengganti selama MVP |
|---|---|---|---|
| Notifikasi seketika status kasir | PRD 5.4 | Tidak menahan alur; butuh infrastruktur WebSocket yang belum dipakai modul ini | Penyegaran 10 detik |
| Penghapusan kolom `InpEpisode` yang dipensiunkan | `02-backend-architecture.md` 9.10 | Menghapus kolom berisi data lama berisiko saat rollback | Kolom tetap ada, tidak dibaca |
| Perlakuan gerbang pasien meninggal dan kabur | `RWI-RULE-037`, gate G-01 | Aturan klinisnya belum final | Penutupan oleh supervisor dengan alasan |

### 8.9 Alur bisnis target

`FLOW-RWF-MVP-01` mengikuti `flowcharts/00-alur-utama.md` bagian 3: admisi → invoice terbuka → layanan → keputusan pulang → keluar ruangan → kasir memberi izin → penutupan.

### 8.10 Epic dan functional requirement

| Epic | FR | Disposisi backend |
|---|---|---|
| `EPIC-RWF-01` Gerbang penutupan dan izin kasir | `FR-RWF-001` s.d. `008` | `EXTEND` (`record-departure`, `close`, `close-with-override`), `MISSING / NEW` (adapter, daftar pulang sebelum izin), penghapusan endpoint lama |
| `EPIC-RWF-02` Tagihan rawat inap masuk kasir | `FR-RWF-010` s.d. `019` | `MISSING / NEW` (penerima, tanda terima, koreksi, putar ulang), `EXTEND` (jembatan, biaya admin, finalisasi, outbox), `REPAIR` (label `RANAP`) |
| `EPIC-RWF-03` Tagihan Pasien (sisi backend) | `FR-RWF-020` s.d. `024` | `EXTEND` (`patient-billing-summaries`), `MISSING / NEW` (`/breakdown`, `/amounts`) |

Rumusan dan contoh berangka setiap FR ada di `PRD-RWI-FINISHING-001` v`0.4` bagian 6; disposisi teknisnya pada `02-backend-architecture.md` 9.6.

### 8.11 Model status yang diusulkan

Mengikuti `contracts/state-transition-matrix.md` bagian 5: pesan outbox (`Pending` → `Processing` → `Published`/`Failed` → `DeadLetter`), jejak pengamatan status kasir, gerbang keluar ruangan dan penutupan, koreksi penempatan, dan tanda "perlu diperiksa". Invariant `INV-RWF-01` s.d. `09`.

### 8.12 Sasaran arsitektur

| Dipakai ulang | Diperluas | Baru |
|---|---|---|
| `BilInpatientClearanceHandoff`, `BillingCalculationService` tarif kamar, `MstRoomChargePolicy`, jembatan folio, `InpBillingDepositAdapter` sebagai pola | `InpEpisode`, `InpBedPlacement`, `InpIntegrationOutboxes`, `BilInvoice`, worker, layanan penutupan dan finalisasi | `InpBillingClearanceAdapter`, `InpPlacementCorrectionService`, `InpIntegrationReplayService`, `BillingInpatientEventReceiver`, `BilInpatientEventReceipt` |

### 8.13 Sasaran kemampuan API

Seluruhnya bagian dari `contracts/api-contract.md` bagian 3, tidak melebihinya.

| Tag | Method dan path | Hak akses | Epic | Status |
|---|---|---|---|---|
| `Health Services / Inpatient Management / Inpatient Discharge` | `POST discharges/{episodeId}/record-departure` | `InpatientDischarge : RecordDeparture` | `EPIC-RWF-01` | Diubah |
| Sama | `POST discharges/{episodeId}/close`, `/close-with-override`, `GET /closure-readiness` | `InpatientDischarge : Close`, `: CloseOverride`, `: Read` | `EPIC-RWF-01` | Diubah |
| `Inpatient Billing Operational` | `GET episodes/{episodeId}/billing-status` | `InpatientBillingOperational : Read` | `EPIC-RWF-01` | Diubah |
| `Health Services / Inpatient Management / Inpatient Monitoring` | `GET monitoring/departures-before-clearance` | `InpatientMonitoring : Read` | `EPIC-RWF-01` | **Rencana (belum tersedia)** |
| `Health Services / Inpatient Management / Bed Occupancy` | `POST bed-occupancies/placements/{placementId}/corrections` | `InpatientBedOccupancy : Correct` | `EPIC-RWF-02` | **Rencana (belum tersedia)** |
| `Health Services / Inpatient Management / Integration Outbox` | `GET integration-outbox`, `POST integration-outbox/replay` | `InpatientIntegrationOutbox : Read`, `: Replay` | `EPIC-RWF-02` | **Rencana (belum tersedia)** |
| `Health Services / Billing Management / Patient Billing Summary` | `GET …/episodes/{episodeId}/breakdown`, `…/breakdown/amounts`, `…/amounts` | `PatientBillingSummary : Read`, `: ViewAmount` | `EPIC-RWF-03` | **Rencana (belum tersedia)** |
| `Health Services / Billing Management / Billing / Invoices` | `GET billing/invoices/review-queue`, `POST …/{id}/review-resolution` | `BillingInvoice : Read`, `: Update` | `EPIC-RWF-02` | **Rencana (belum tersedia)** |

### 8.14 Matriks kewenangan

Mengikuti `contracts/permission-audit-matrix.md` 5.2 dan 5.3. String permission persis sama.

### 8.15 Batas integrasi dan billing

| Yang **MUST NOT** dibuat sendiri oleh Rawat Inap | Pemiliknya |
|---|---|
| Status izin kasir, salinannya, atau cache-nya | Billing |
| Hitungan tarif kamar, biaya admin, atau rupiah apa pun | Billing |
| Pembacaan tabel `Bil*` secara langsung | Billing; dibaca lewat service |
| Endpoint publik penerima event | Tidak ada; penerima di dalam aplikasi |

### 8.16 Guardrail regulasi

| Kewajiban | Penerapan |
|---|---|
| Privasi data keuangan pasien di bangsal | Rupiah hanya untuk pemegang `PatientBillingSummary : ViewAmount` (`RWI-DEC-160`, `170`) |
| Keterlusuran tindakan administratif | Keluar ruangan, penutupan, override, koreksi, dan putar ulang tercatat dengan pelaku dan waktu |
| Rekam medis | Tidak ada data klinis baru pada sub-modul ini |

### 8.17 Kebutuhan non-fungsional

| ID | Kebutuhan |
|---|---|
| `NFR-RWF-01` | Kegagalan Billing tidak menggagalkan admisi, penempatan, koreksi, maupun keluar ruangan (`RWI-DEC-161`) |
| `NFR-RWF-02` | Pengiriman event *at-least-once* dengan efek *exactly-once* di Billing (kunci idempotensi) |
| `NFR-RWF-03` | Status kasir di layar paling basi 10 detik; keputusan server selalu memakai bacaan terbaru |
| `NFR-RWF-04` | Koreksi penempatan memakai pemeriksaan versi; dua koreksi bersamaan tidak sama-sama berhasil |
| `NFR-RWF-05` | Waktu disimpan UTC; ditampilkan `Asia/Jakarta` |

### 8.18 Skenario UAT

| ID | Jalur | Kondisi awal | Langkah | Hasil yang diharapkan |
|---|---|---|---|---|
| `UAT-RWF-01` | Berhasil | "Tn. Contoh A" kelas 2 masuk 1 Okt 10.00 | Layanan tiga hari, keluar 4 Okt 09.00, kasir memberi izin, admisi menutup | Satu invoice `RANAP` lengkap tanpa input manual; tarif kamar berhenti 09.00; `Closed` |
| `UAT-RWF-02` | Gagal | — | Panggil webhook lama dan `close-with-override` tanpa token | 404 dan 401; status tidak berubah |
| `UAT-RWF-03` | Gagal lalu berhasil | Izin `PENDING` | Catat keluar dengan peringatan; kasir menyetujui lalu mencabut karena resep susulan | Bed langsung kosong; tombol Tutup aktif lalu terkunci dengan banner merah |
| `UAT-RWF-11` | Gagal lalu berhasil | Billing dimatikan | Sahkan admisi; hidupkan Billing | Admisi tersimpan; pesan gagal lalu terkirim; tepat satu invoice |
| `UAT-RWF-12` | Gagal dan berhasil | — | Override tanpa permission, alasan "...", lalu alasan jelas | 403, ditolak, lalu `Closed` dan masuk laporan |
| `UAT-RWF-15` | Berhasil | Dua episode aktif, satu dengan biaya kamar manual | Putar ulang dua kali | Satu invoice masing-masing; hanya yang berbiaya manual "perlu diperiksa"; putar ulang kedua tanpa perubahan |
| `UAT-RWF-23` | Berhasil dan gagal | — | Serah 3 vial; retur 1 layak; retur 1 rusak | 3 → 2 vial dengan rujukan retur; retur rusak tidak mengubah. Disaksikan petugas Farmasi |
| `UAT-RWF-25` | Gagal | Invoice `FINAL` | Kepala ruangan mengoreksi kelas | Ditolak "tagihan sudah difinalkan" |

### 8.19 Definition of Done

| Butir | Bukti |
|---|---|
| Tidak ada endpoint pengubah status kepulangan tanpa login | `UAT-RWF-02`; test `AC-RWF-001` |
| Status kasir hanya satu sumber | Test statis `AC-RWF-005` |
| Invoice `RANAP` terbentuk otomatis dan tepat satu | `UAT-RWF-01`, `UAT-RWF-11`; test `INV-RWF-06` |
| Pesan outbox `Published` hanya dengan tanda terima | Test `AC-RWF-016` |
| Tidak ada angka tarif atau jam potong tertanam | Test statis `AC-RWF-014` |
| Koreksi penempatan tidak menggandakan tarif kamar | Kedua uji wajib `IsSuperseded` lulus |
| Rupiah tidak sampai ke perawat, dan hak rupiah tidak bergantung pada nama peran | Test `AC-RWF-020`, `021`, `022` |
| Episode aktif saat rilis punya invoice | `UAT-RWF-15` |
| Regresi rawat jalan nol | Test regresi pada `testing/acceptance-test-matrix.md` bagian 4 |
| Master kebijakan tarif kamar dan tarif kelas terisi di lingkungan uji | `02-backend-architecture.md` 9.11 |

### 8.20 Urutan pengiriman dan pertanyaan terbuka

| Gelombang | Isi | Syarat mulai |
|---|---|---|
| `MVP-0` (`RWF-W0`) | Hapus webhook, PIN, pemeriksaan nama peran, hitungan tarif kedua; seragamkan `RANAP`; migration `I1`, `I2` | Kontrak `1.1.0` disetujui |
| `MVP-1` (`RWF-W1`) | Penerima event, worker jujur, adapter, keluar ruangan, penutupan, jembatan `RANAP`, biaya admin, finalisasi, koreksi, rincian Tagihan Pasien | `MVP-0` |
| `MVP-2` | Putar ulang (`I5`) dengan wewenang tertulis | `MVP-1` terbukti di lingkungan uji |
| `MVP-3` | Retur obat membatalkan tagihan | `MVP-1` (persetujuan Farmasi sudah ada, `RWI-DEC-210`) |
| `POST-MVP` | Penghapusan kolom dipensiunkan; notifikasi seketika | — |

| Pertanyaan | Siapa yang menjawab | Dampak bila belum dijawab | Memblokir |
|---|---|---|:---:|
| ~~Pemilik `PharmacyManagement` dan persetujuannya (`RWI-OQ-108`)~~ | Ikbal Yulianto | **Disetujui `RWI-DEC-210`, 2 Oktober 2026** | Tidak lagi |
| Pengesahan pemetaan tujuh kelompok Tagihan Pasien (gate G-04) | Yasmina | Konfigurasi bawaan dipakai | Tidak |
| Pemetaan role ke permission di lingkungan target (`FIN-UNK-05`) | Admin Akses Role | UAT gagal walau kode benar | Tidak untuk desain |

**Penyelarasan decision log revision `31` ★ 2 Oktober 2026.** Tidak ada pertanyaan memblokir yang tersisa. `RWI-DEC-207` menambah satu kemampuan Billing pada gelombang `MVP-4` (`RWF-W7`): tautan kunjungan asal ke invoice `RANAP` dan baris operasinya di Tagihan Pasien (`02-backend-architecture.md` 9.14). Penyelesaian satu kwitansi mengikuti `BKC-DEC-118` milik `billing-kasir`.

| Gelombang | Isi | Syarat mulai |
|---|---|---|
| `MVP-4` (`RWF-W7`) | `I6`; tautan pada `ADMISSION_CONFIRMED`; baris operasi kunjungan tertaut di `breakdown` dan `amounts` | `MVP-1`; `episode-rawat-inap` `E6` (`InpAdmissionReferral`) |

| ID | Jalur | Langkah | Hasil |
|---|---|---|---|
| `UAT-RWF-39` | Berhasil | Pasien Poli Bedah dioperasi, kamar pulih memutuskan rawat inap, admisi dari permintaan | Invoice Poli Bedah tertaut ke invoice `RANAP` tanpa baris berpindah; Tagihan Pasien menampilkan baris operasi berlabel kunjungan asal (`RWI-AC-330`) |
| `UAT-RWF-40` | Gagal | Pesan `ADMISSION_CONFIRMED` yang sama dikirim ulang dan putar ulang dijalankan | Tetap satu tautan; tidak ada baris ganda |
