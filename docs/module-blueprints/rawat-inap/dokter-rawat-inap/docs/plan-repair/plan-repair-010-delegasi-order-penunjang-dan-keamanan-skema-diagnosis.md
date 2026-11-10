# PLAN-REPAIR-010 — Uji langsung pesanan perawat (BE-RWI-104) lalu buka tombol FE-RWI-172; perubahan endpoint dan migration versi 1 dibatalkan

```yaml
plan_id: PLAN-REPAIR-DOK-010
issue: ../issue/issue-010-delegasi-order-penunjang-dan-keamanan-skema-diagnosis.md
status_rencana: MENUNGGU_PERSETUJUAN
tanggal_rencana: "2026-11-02"
diputuskan_oleh: null
tanggal_keputusan: null
basis_source_backend: "396c8363 (MHamzah)"
basis_source_frontend: "9c4000724 (HamzahV2)"
```

> **Catatan revisi 2 (2026-11-02).** Versi 1 berisi dua perbaikan. `FIX-DOK-010-01` akan membuat endpoint
> dan permission kedua untuk pesanan yang jalurnya sudah ada. `FIX-DOK-010-02` akan mengubah `Down()`
> migration menjadi tidak mengembalikan skema, dan memakai nama kolom yang salah. Keduanya
> **dibatalkan**. Pekerjaan yang benar-benar tersisa adalah uji langsung `BE-RWI-104` lalu menuntaskan
> task yang sudah ada, `FE-RWI-172`. Pekerjaan itu dicatat sebagai `FIX-DOK-010-03`.

---

## 1. Register status pengerjaan

| ID perbaikan | Menutup | Ringkasan | Area | Prioritas | Task ID | Status | Bukti |
| --- | --- | --- | --- | :---: | --- | --- | --- |
| `FIX-DOK-010-01` | `ISS-DOK-010-01` | **DIBATALKAN.** Endpoint `POST .../delegated-supporting-orders` dan permission `CreateDelegated` tidak dibuat, karena jalur pesanan perawat sudah ada pada `POST .../lab-orders` dan `POST .../rad-orders` (`BE-RWI-104`) | — | — | — | ⛔ TERBLOKIR — dibatalkan, jangan dikerjakan; menunggu pemilik mengesahkan pembatalan (K-02) | `LabOrderDtos.cs:127-131`, `LabOrderController.cs:420`, `:454` |
| `FIX-DOK-010-02` | `ISS-DOK-010-02` | **DIBATALKAN.** `Down()` migration `BE-RWI-068` tidak diubah. Urutan mundur aman sudah tertulis pada `02-backend-architecture.md` bagian 7.3 | — | — | — | ⛔ TERBLOKIR — dibatalkan, jangan dikerjakan; menunggu pemilik mengesahkan pembatalan (K-02) | `20260909065125_...cs:16-22` |
| `FIX-DOK-010-03` | `ISS-DOK-010-01` | Uji langsung `BE-RWI-104`: perawat memesan Lab/Rad atas instruksi dokter, dokter memverifikasi, petugas Lab menerima. Setelah lulus, lanjutkan `FE-RWI-172` untuk membuka tombol dan memasang form | Uji langsung + FE | 1 | `FE-RWI-172` | ⛔ TERBLOKIR — menunggu K-01 (akun dan lingkungan uji dari pemilik); prasyarat `FE-RWI-173` masih 🟡 menurut `FE-RWI-172.md:10` | — |

**Ringkasan: 0 dari 1 perbaikan aktif selesai. Dua perbaikan dibatalkan dan tidak dihitung.**

---

## 2. Solusi terpilih per temuan

### ISS-DOK-010-01 — Tombol pesan Lab/Rad perawat terkunci

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Endpoint dan permission delegasi baru (usulan versi 1) | — | Menduplikasi `BE-RWI-104`. Dua jalur pesanan untuk satu kebutuhan membuat aturan "dokter wajib bertugas" dan antrean verifikasi harus dijaga di dua tempat |
| B | Buka tombol sekarang tanpa uji langsung | Cepat | Melanggar gerbang bukti runtime pada kartu `FE-RWI-172`. Bila jalur lintas modul gagal, pesanan cito bisa tidak sampai ke Lab tanpa ada yang tahu |
| **C** | **Uji langsung `BE-RWI-104` dengan akun nyata, lalu tuntaskan `FE-RWI-172`** | Memakai jalur yang sudah disetujui `RWI-DEC-153`; tidak ada kode backend baru | Menunggu akun dan lingkungan uji |

**Solusi terpilih: Opsi C.**

1. Backend sudah lengkap dan disetujui pemilik Lab/Rad (`RWI-DEC-153`).
2. Kunci tombol hanya boleh dilepas setelah ada bukti runtime. Aturan ini tertulis pada kartu task
   `FE-RWI-172` dan dikutip laporannya (`FE-RWI-172.md:116`).

**Kenapa bukan opsi lain.** Opsi A membangun ulang yang sudah ada. Opsi B melompati gerbang keselamatan
yang disengaja.

### ISS-DOK-010-02 — Langkah mundur migration

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| **A** | **Tidak mengubah `Down()`. Pakai urutan mundur pada bagian 7.3 dan periksa database dengan query baca sebelum rollback** | Nol perubahan source; rollback tetap mengembalikan skema yang benar | Rollback tetap butuh langkah manual bila sudah ada diagnosis rawat inap |
| B | Tambah pemeriksaan di awal `Down()` yang menghentikan rollback dengan pesan jelas bila ada baris tanpa nomor konsultasi | Pesan galat lebih mudah dibaca | Hasil akhirnya sama dengan A, karena rollback tetap berhenti. Menyentuh berkas migration tanpa manfaat baru |
| C | `Down()` versi 1: biarkan kolom boleh kosong dan perbarui baris | — | Salah nama kolom (`EpisodeId`), membuang `DropColumn`, dan tidak mengembalikan skema |

**Solusi terpilih: Opsi A.** Kegagalan rollback dibatalkan utuh oleh transaksi, sehingga tidak ada
kerusakan yang perlu dicegah dengan kode. Yang dibutuhkan hanya disiplin prosedur, dan prosedurnya
sudah tertulis.

---

## 3. Skema tampilan sebelum → sesudah

### Tab Laboratorium dan Radiologi pada Penunjang Medis keperawatan — `FIX-DOK-010-03`

**SEBELUM (sekarang)**

```text
Penunjang Medis › Laboratorium
┌ Pesanan Laboratorium ─────────────────────────────────────────────────┐
│ Riwayat pesanan dan hasil final tampil seperti biasa                   │
│                                                    (×) [ Pesan ]       │ ← disabled={true}
└────────────────────────────────────────────────────────────────────────┘
```

**SESUDAH.** Bentuk form pesanan perawat **tidak digambar ulang di sini**. Form itu adalah lingkup kartu
task `FE-RWI-172` pada `dokter-rawat-inap/roadmap/frontend-roadmap-finishing.md`, beserta skema tampilan
yang sudah disetujui untuk kartu itu. Rangka "SESUDAH" pada versi 1 dibatalkan karena memuat isian yang
tidak ada pada kontrak, misalnya `Metode Instruksi` dan `PriorityLevel`. Yang dijamin rencana ini hanya
dua hal:

1. tombol **Pesan** baru dapat ditekan setelah `FIX-DOK-010-03` langkah 1-4 lulus;
2. form memakai kolom kontrak yang sudah ada: dokter pemberi instruksi dan daftar pemeriksaan cito
   (`CitoExaminations`).

### Migration diagnosis — `FIX-DOK-010-02`

**Tidak ada perubahan tampilan.** Perbaikannya dibatalkan, dan migration tidak punya layar.

---

## 4. Rincian perbaikan

### FIX-DOK-010-01 — DIBATALKAN

| | |
| --- | --- |
| **Menutup** | `ISS-DOK-010-01` |
| **Alasan pembatalan** | Jalur pesanan perawat sudah ada. Bukti: `LabOrderDtos.cs:127-131`, `LabOrderController.cs:372-378`, `:395`, `:420`, `:454`, `RadOrderController.cs:356`, `:370`, `LabOrderService.cs:751`; keputusan `RWI-DEC-153` |

Tidak ada langkah. `InpatientSupportingOrderController`, `CreateDelegatedSupportingOrderRequest`, dan
permission `InpatientSupportingOrder:CreateDelegated` **tidak boleh** dibuat.

---

### FIX-DOK-010-02 — DIBATALKAN

| | |
| --- | --- |
| **Menutup** | `ISS-DOK-010-02` |
| **Alasan pembatalan** | `Down()` yang sekarang sudah benar secara skema. Risikonya terdokumentasi, dan kegagalannya dibatalkan utuh oleh transaksi. Kode usulan versi 1 salah nama kolom dan tidak mengembalikan skema |

Tidak ada langkah. Berkas `Migrations/20260909065125_AddInpatientEpisodeContextToPatientDiagnosis.cs`
**tidak boleh** diubah atas dasar issue ini. Penerapan migration ke database tetap menuntut wewenang
terpisah, sesuai `AGENTS.md` bagian Keselamatan Database.

---

### FIX-DOK-010-03 — Uji langsung BE-RWI-104 lalu tuntaskan FE-RWI-172

| | |
| --- | --- |
| **Menutup** | `ISS-DOK-010-01` |
| **Area** | Uji langsung lintas modul (Rawat Inap, Laboratorium, Radiologi), lalu frontend |
| **Jenis perubahan** | Langkah 1-4 tanpa perubahan source. Langkah 5 source frontend menurut kartu `FE-RWI-172` |
| **Berkas yang diubah** | Langkah 1-4: tidak ada. Langkah 5: sesuai kartu `FE-RWI-172`, berpusat pada `src/components/view/health-services/inpatient-management/nursing-workspace/sections/ancillary/nursing-ancillary-section.jsx` |
| **Radius dampak** | Tab Laboratorium dan Radiologi pada ruang kerja keperawatan; antrean verifikasi dokter; antrean kerja Laboratorium dan Radiologi |
| **Bergantung pada** | K-01; prasyarat `FE-RWI-173` |

**Langkah.**

1. Siapkan satu pasien uji rawat inap. DPJP-nya, dr. H (pseudonim), bertugas aktif atas pasien itu.
   Siapkan juga satu akun perawat bangsal berhak `LabOrder : Create` dan satu petugas Laboratorium.
2. Dengan akun perawat, kirim pesanan Darah Lengkap dan Kalium (cito) atas nama dr. H. Kirim lewat
   endpoint pesanan Lab yang ada atau alat uji API, karena tombol layar masih terkunci.
3. Dengan akun dr. H, buka antrean verifikasi instruksi lalu verifikasi pesanan itu.
4. Ulangi langkah 2-3 untuk satu pesanan Radiologi. Lalu ulangi langkah 2 dengan dokter yang **tidak**
   bertugas atas pasien.
5. Bila langkah 1-4 lulus, kerjakan kartu `FE-RWI-172` dengan `build-module-frontend`, lalu perbarui
   register ini dengan tautan laporannya.

**Acceptance criteria.**

1. Pesanan perawat pada langkah 2 tersimpan dengan dokter pemberi instruksi dr. H dan status verifikasi
   `Pending`. Pemeriksaan Kalium tercatat cito.
2. Pesanan itu muncul pada `GET .../lab-orders/instruction-verification-worklist` saat dr. H login, dan
   **tidak** muncul pada antrean dokter lain.
3. Setelah dr. H memverifikasi, status verifikasinya berubah dan pesanan hilang dari antrean verifikasi.
4. Pesanan atas nama dokter yang tidak bertugas ditolak dengan kode 403. Pesannya menjelaskan bahwa dokter
   pemberi instruksi tidak bertugas atas pasien.
5. Petugas Laboratorium melihat pesanan cito pada antrean kerjanya. Catat apakah sampel dapat diproses
   sebelum verifikasi dokter, sebagai jawaban faktual untuk K-03.
6. Kriteria radiologi sama dengan kriteria 1-4, memakai endpoint `rad-orders`.

**Verifikasi.** Uji langsung di peramban mengikuti `rules/testing/`. Laporannya ditulis pada laporan
task `FE-RWI-172`. Untuk langkah 5: `npm run lint:errors`, `npm run test:unit`, `npm run build`.

**Risiko.** Uji langsung membuat pesanan sungguhan di database bersama. Pakai pasien uji, lalu batalkan
pesanannya setelah uji lewat alur pembatalan yang berlaku.

---

## 5. Urutan pengerjaan

```text
K-01 (akun & lingkungan uji dari pemilik)
└── ⛔ FIX-DOK-010-03   langkah 1-4: uji langsung BE-RWI-104
    └── FE-RWI-172   langkah 5: buka tombol & pasang form (task roadmap yang sudah ada)
FE-RWI-173 (prasyarat, 🟡)
└── ⛔ FIX-DOK-010-03
K-02 (pengesahan pembatalan)
├── ⛔ FIX-DOK-010-01   dibatalkan
└── ⛔ FIX-DOK-010-02   dibatalkan
```

---

## 6. Keputusan pemilik yang dibutuhkan

| No | Keputusan | Rekomendasi | Menahan perbaikan |
| ---: | --- | --- | --- |
| K-01 | Menyediakan lingkungan dan akun uji langsung (perawat, dokter bertugas, petugas Lab) | Sediakan | `FIX-DOK-010-03` |
| K-02 | Mengesahkan pembatalan `FIX-DOK-010-01` dan `FIX-DOK-010-02` | Setuju | Penutupan issue |
| K-03 | Laboratorium memproses sampel sebelum atau sesudah verifikasi dokter | Putuskan setelah kriteria 5 `FIX-DOK-010-03` memberi fakta perilaku sekarang | Tidak menahan |

---

## 7. Dokumen hulu yang ikut direvisi

| Dokumen | Bagian | Sekarang | Menjadi | Untuk |
| --- | --- | --- | --- | --- |
| `dokter-rawat-inap/02-backend-architecture.md` | — | Versi 1 meminta alur delegasi baru | **Tidak direvisi** — alurnya sudah ada | — |
| `dokter-rawat-inap/contracts/api-contract.md` | — | Versi 1 meminta endpoint `delegated-supporting-orders` | **Tidak direvisi** — endpoint itu tidak dibuat | — |
| `dokter-rawat-inap/task/report/frontend/FE-RWI-172.md` | Bukti runtime | `BE-RWI-104` runtime `NOT RUN` | Diisi hasil `FIX-DOK-010-03` langkah 1-4 | `FIX-DOK-010-03` |

---

## 8. Verifikasi menyeluruh

1. **Pesanan perawat ujung ke ujung.** Setelah `FE-RWI-172` selesai: perawat membuka tab Laboratorium,
   memilih dr. H sebagai pemberi instruksi, menandai Kalium cito, lalu mengirim. Pesanan masuk antrean
   Lab dengan tanda cito. dr. H memverifikasinya dari antrean verifikasi.
2. **Pemeriksaan keadaan migration sebelum rollback apa pun.** Pemilik menjalankan dua query baca ini.
   Agent tidak menjalankan query ke database bersama.

   ```sql
   -- Apakah migration BE-RWI-068 sudah diterapkan?
   SELECT "MigrationId" FROM "__EFMigrationsHistory"
   WHERE "MigrationId" = '20260909065125_AddInpatientEpisodeContextToPatientDiagnosis';

   -- Berapa diagnosis yang akan menghalangi rollback?
   SELECT COUNT(*) FROM public."TrxPatientDiagnosis" WHERE "ConsultationId" IS NULL;
   ```

   Contoh membaca hasilnya: query pertama mengembalikan satu baris dan query kedua mengembalikan `12`.
   Artinya migration sudah diterapkan dan ada 12 diagnosis rawat inap tanpa nomor konsultasi. Rollback
   hanya boleh dilakukan setelah 12 baris itu ditangani bersama pemilik klinis, sesuai bagian 7.3.

---

## 9. Riwayat

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-11-02 | Versi 1: rencana diterbitkan, 2 perbaikan, menunggu persetujuan | AI Agent (`diagnose-module-issue`) |
| 2026-11-02 | Revisi 2: verifikasi ulang terhadap source SHA yang sama. `FIX-DOK-010-01` dibatalkan karena jalur pesanan perawat sudah ada (`BE-RWI-104`). `FIX-DOK-010-02` dibatalkan karena usulan `Down()` salah nama kolom dan tidak mengembalikan skema. `FIX-DOK-010-03` ditambahkan: uji langsung `BE-RWI-104` lalu lanjutkan `FE-RWI-172`. Rangka "SESUDAH" versi 1 dibatalkan karena memuat isian di luar kontrak | AI Agent (`diagnose-module-issue`) atas permintaan pemilik |
