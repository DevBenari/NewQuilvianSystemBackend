# PLAN-REPAIR-011 — Rencana Perbaikan Tampilan Tagihan Pasien Bangsal: Eliminasi Nilai Palsu "Rp 0", Harmonisasi Pesan Privasi Kasir, dan Pengetatan Otorisasi Nominal

```yaml
plan_id: PLAN-REPAIR-KEP-011
issue: ../issue/issue-011-tampilan-tagihan-pasien-bangsal.md
status_rencana: SELESAI
tanggal_rencana: "2026-10-06"
diputuskan_oleh: "Pengguna (Pemilik Sistem — 'rekomendasi di setujui')"
tanggal_keputusan: "2026-10-07"
basis_source_backend: "671191eb1aff3f618456cb6c909863bb0eca82f4 (MHamzah)"
basis_source_frontend: "1f889d67cbaaa5c3df5f8d73cfe9cd0b1d434d83 (HamzahV2)"
ditulis_dengan: "skill diagnose-module-issue"
```

---

## 1. Register Status Pengerjaan

| ID perbaikan | Menutup | Ringkasan | Area | Prioritas | Task ID | Status | Bukti |
| --- | --- | --- | :---: | :---: | :---: | --- | --- |
| `FIX-KEP-011-01` | `ISS-KEP-011-01` | Hapus fallback `"Rp 0"` palsu; tampilkan status *"Belum dikalkulasi"* saat `subtotalAmount`/`runningTotalAmount` bernilai `null` | FE | 1 | `FE-KEP-23` | ✅ SELESAI | `nursing-billing-section.jsx:236`, `:311`, unit test 11/11 PASS |
| `FIX-KEP-011-02` | `ISS-KEP-011-02` | Selaraskan pesan privasi kasir agar kontekstual terhadap kepemilikan izin `ViewAmount` | FE | 1 | `FE-KEP-23` | ✅ SELESAI | `billing-summary-card.jsx:38`, `:157`, `nursing-billing-section.jsx:158` |
| `FIX-KEP-011-03` | `ISS-KEP-011-03` | Perjelas teks keterangan ketiadaan waktu evaluasi kasir dengan status operasional alur kepulangan | FE | 2 | `FE-KEP-23` | ✅ SELESAI | `billing-summary-card.jsx:140` |
| `FIX-KEP-011-04` | `ISS-KEP-011-04` | Integrasikan tombol `[Muat Ulang]` untuk menyegarkan status kasir dan rincian tagihan secara serentak | FE | 2 | `FE-KEP-23` | ✅ SELESAI | Terkoordinasi via `userCanViewAmount` dan `fetchBillingBreakdown` |
| `FIX-KEP-011-05` | `ISS-KEP-011-05` | Terapkan *strict permission check* pada `ViewAmount` dengan memvalidasi flag `viewAmountLoaded` | FE | 1 | `FE-KEP-23` | ✅ SELESAI | `nursing-billing-section.jsx:61`, `:82`, `:111`, `:158`, `:234`, `:305` |

**Ringkasan: 5 dari 5 perbaikan selesai (seluruh implementasi kode tuntas dan terverifikasi).**

---

## 2. Solusi Terpilih per Temuan

### ISS-KEP-011-01 — Pemalsuan Nilai "Rp 0" Palsu pada Subtotal Kelompok dan Total Tagihan

| Opsi | Cara | Kelebihan | Kekurangan |
| :---: | --- | --- | --- |
| A | Ganti fallback `"Rp 0"` dengan lencana status netral *"Belum dikalkulasi kasir"* jika bernilai `null`, dan hanya tampilkan format rupiah jika nilainya bertipe numerik valid | Sangat jujur, transparan secara klinis/keuangan, menghilangkan kesalahpahaman pasien | Butuh penyesuaian styling badge kecil di samping header grup |
| B | Ganti `"Rp 0"` dengan tanda strip `"—"` | Sangat sederhana diimplementasikan | Kurang informatif; staf tidak tahu mengapa strip muncul |
| C | Sembunyikan baris subtotal sepenuhnya jika nilainya `null` | Tampilan sangat ringkas | Staf berwenang tidak tahu apakah kalkulasi sedang berjalan atau sistem tidak memiliki data |

**Solusi terpilih: Opsi A.**
1. Memenuhi standar integritas data keuangan rumah sakit (`patient-billing-summary-utils.js` dan `FE-RWI-185` AC-3): data yang belum dihitung tidak boleh dipalsukan menjadi nol rupiah.
2. Memberikan kejelasan status operasional kepada staf administrasi bangsal bahwa tindakan telah tercatat namun antrean hitung kasir belum dieksekusi.

**Kenapa bukan opsi lain:**
Opsi B terlalu minim informasi bagi staf medis; Opsi C membingungkan karena pemegang izin `ViewAmount` mengira hak aksesnya tidak berfungsi ketika subtotal hilang tanpa jejak.

---

### ISS-KEP-011-02 — Kontradiksi Antara Banner Privasi Kasir dengan Tampilan Nominal

| Opsi | Cara | Kelebihan | Kekurangan |
| :---: | --- | --- | --- |
| A | Berikan prop `userCanViewAmount` ke `BillingSummaryCard` untuk mengondisikan catatan privasi di footer | Mempertahankan arsitektur kartu bersama, pesan privasi akurat sesuai peran pengguna | Sedikit menyentuh `billing-summary-card.jsx` |
| B | Hapus seluruh catatan privasi dari `BillingSummaryCard` | Tidak ada teks kontradiktif | Kehilangan penegasan privasi penting untuk perawat pelaksana biasa |
| C | Sembunyikan `BillingSummaryCard` dari seksi Tagihan Pasien | Seksi tagihan menjadi sangat ringkas | Perawat kehilangan pantauan izin pulang kasir (*clearance status*) |

**Solusi terpilih: Opsi A.**
1. Menjaga fungsi utama kartu status kasir (`FE-INT-06`) untuk memantau izin pulang pasien rawat inap.
2. Menghilangkan kontradiksi logika: untuk perawat biasa (`!canViewAmount`), pesan tetap menegaskan bahwa rupiah hanya di kasir; untuk pemegang izin `ViewAmount`, pesan diubah menjadi keterangan informatif bahwa subtotal/total hanya untuk pemantauan administratif dan rincian harga per item tetap steril di kasir.

**Kenapa bukan opsi lain:**
Opsi B melemahkan kepatuhan etika perawat di samping tempat tidur; Opsi C melanggar arsitektur `FE-RWI-166` yang mewajibkan status kasir terpantau di bangsal.

---

### ISS-KEP-011-03 — Ambiguasi Status "Waktu penilaian kasir belum tersedia"

| Opsi | Cara | Kelebihan | Kekurangan |
| :---: | --- | --- | --- |
| A | Perjelas teks menjadi: *"Belum dievaluasi kasir (evaluasi dilakukan saat alur kepulangan dimulai)"* | Sangat jelas bagi perawat, tidak menimbulkan kepanikan data hilang | Menambah panjang teks footer beberapa karakter |
| B | Sembunyikan baris waktu evaluasi jika nilainya `null` | Tampilan lebih bersih | Perawat tidak mengetahui apakah kasir sudah pernah menilai atau belum |

**Solusi terpilih: Opsi A.**
Memberikan pemahaman alur kerja rumah sakit yang tepat bahwa evaluasi kasir baru diterbitkan saat dokter menginstruksikan kepulangan pasien.

**Kenapa bukan opsi lain:**
Opsi B menyembunyikan status alur administratif yang seharusnya dapat dipantau oleh kepala ruangan.

---

### ISS-KEP-011-04 — Duplikasi Tombol Pemuatan Ulang

| Opsi | Cara | Kelebihan | Kekurangan |
| :---: | --- | --- | --- |
| A | Sediakan callback `onRefresh` pada header utama yang memicu `refresh()` kartu kasir dan `fetchBillingBreakdown()` sekaligus, serta sembunyikan tombol refresh kecil pada kartu kasir bila dipasang di seksi tagihan | Pengalaman pengguna (*UX*) menjadi satu pintu, tampilan bersih dan rapi | Memerlukan prop opsional `showRefreshButton={false}` pada `BillingSummaryCard` |
| B | Biarkan dua tombol tetap ada namun ubah labelnya menjadi "Perbarui Izin Kasir" dan "Muat Ulang Tagihan" | Sangat minim perubahan kode | Tetap memboroskan ruang dan membingungkan pengguna |

**Solusi terpilih: Opsi A.**
Memberikan pengalaman antarmuka pengguna yang bersih, profesional, dan seragam sesuai kaidah *Single Action Trigger*.

**Kenapa bukan opsi lain:**
Opsi B tetap menyisakan redundansi kontrol pada jarak visual yang berdekatan.

---

### ISS-KEP-011-05 — Pengetatan Gate Hak Akses `ViewAmount`

| Opsi | Cara | Kelebihan | Kekurangan |
| :---: | --- | --- | --- |
| A | Evaluasi strict permission: `const isAuthorizedViewAmount = Boolean(viewAmountLoaded && canViewAmount);` | Mencegah flicker tampilan rupiah dan mencegah pemanggilan liar endpoint `/amounts` saat state belum siap | Tidak ada |
| B | Tetap menggunakan `canViewAmount` apa adanya | Tidak perlu ubah logika hook | Risiko kebocoran tampilan nominal sesaat dan pemborosan request HTTP 403 |

**Solusi terpilih: Opsi A.**
Sesuai kontrak otorisasi Quilvian (`permission-slice.jsx:30-38`): hak akses yang menampilkan data keuangan sensitif wajib dicek secara ketat (*strict*).

---

## 3. Skema Tampilan Sebelum → Sesudah

### Skenario 1: Tampilan bagi Petugas Berwenang (Pemegang Hak Akses `ViewAmount`)

#### SEBELUM (Kondisi Bermasalah pada Screenshot Pelapor):
```text
+- Status Kasir -------------------------------------------------------------+
| ⚠ Kasir belum menilai izin pulang.                                         |
| Waktu penilaian kasir belum tersedia                 [Perbarui Status]     |
| Layar bangsal tidak menampilkan rupiah. Rincian nominal hanya di kasir. ❌ |
+----------------------------------------------------------------------------+
+- Tagihan Pasien -----------------------------------------------------------+
| Dihitung per waktu terkini                            [Muat Ulang]         |
| ℹ Pedoman Keperawatan Bangsal: Perawat dilarang memperdebatkan harga...    |
|                                                                            |
| [Tindakan]                                               Subtotal: Rp 0 ❌ |
|   Tarif Pemasangan Infus (IV Line Dewasa)   qty 2   6 Okt 2026   [Aktif]   |
|   ALAT MONITORING EKG POLIKLINIK            qty 1   6 Okt 2026   [Aktif]   |
|                                                                            |
| [Penunjang Medis]                                        Subtotal: Rp 0 ❌ |
|   A1 GAMBARAN DARAH TEPI                    qty 1   6 Okt 2026   [Aktif]   |
|                                                                            |
| [Obat & Alkes]                                           Subtotal: Rp 0 ❌ |
|   PARACETAMOL 500 MG TABLET                 qty 10  6 Okt 2026   [Aktif]   |
|                                                                            |
| +========================================================================+ |
| | Total Tagihan Berjalan                                         Rp 0 ❌ | |
| | Hanya dapat dilihat oleh pemegang izin PatientBillingSummary:ViewAmount| |
| +========================================================================+ |
+----------------------------------------------------------------------------+
```

#### SESUDAH (Hasil Perbaikan):
```text
+- Status Kasir -------------------------------------------------------------+
| ⚠ Kasir belum menilai izin pulang.                                         |
| Belum dievaluasi kasir (evaluasi dilakukan saat alur kepulangan dimulai)   |
| ℹ Catatan Wewenang: Subtotal & total berjalan ditampilkan untuk pemantauan |
|   administratif. Rincian tarif per item tetap steril di loket kasir.       |
+----------------------------------------------------------------------------+
+- Tagihan Pasien -----------------------------------------------------------+
| Dihitung per waktu terkini                            [Muat Ulang]         |
| ℹ Pedoman Keperawatan Bangsal: Tampilan bersifat informatif untuk klinis.  |
|                                                                            |
| [Tindakan]                                  Subtotal: [Belum Dikalkulasi]  |
|   Tarif Pemasangan Infus (IV Line Dewasa)   qty 2   6 Okt 2026   [Aktif]   |
|   ALAT MONITORING EKG POLIKLINIK            qty 1   6 Okt 2026   [Aktif]   |
|                                                                            |
| [Penunjang Medis]                           Subtotal: [Belum Dikalkulasi]  |
|   A1 GAMBARAN DARAH TEPI                    qty 1   6 Okt 2026   [Aktif]   |
|                                                                            |
| [Obat & Alkes]                              Subtotal: [Belum Dikalkulasi]  |
|   PARACETAMOL 500 MG TABLET                 qty 10  6 Okt 2026   [Aktif]   |
|                                                                            |
| +========================================================================+ |
| | Total Tagihan Berjalan                             [Menunggu Kasir]    | |
| | Pemantauan biaya berjalan bagi pemegang izin ViewAmount                | |
| +========================================================================+ |
+----------------------------------------------------------------------------+
```

### Skenario 2: Tampilan bagi Perawat Pelaksana Biasa (Tanpa Hak Akses `ViewAmount`)

#### SESUDAH (Hasil Perbaikan):
```text
+- Status Kasir -------------------------------------------------------------+
| ⚠ Kasir belum menilai izin pulang.                                         |
| Belum dievaluasi kasir (evaluasi dilakukan saat alur kepulangan dimulai)   |
| 🔒 Layar bangsal tidak menampilkan rupiah. Rincian nominal hanya di kasir. |
+----------------------------------------------------------------------------+
+- Tagihan Pasien -----------------------------------------------------------+
| Rincian item layanan asuhan keperawatan               [Muat Ulang]         |
| ℹ Pedoman Keperawatan Bangsal: Tampilan bersifat informatif untuk klinis.  |
|   Perawat dilarang memperdebatkan harga layanan. Rincian tersedia di kasir.|
|                                                                            |
| [Tindakan]                                                                 |
|   Tarif Pemasangan Infus (IV Line Dewasa)   qty 2   6 Okt 2026   [Aktif]   |
|   ALAT MONITORING EKG POLIKLINIK            qty 1   6 Okt 2026   [Aktif]   |
|                                                                            |
| [Penunjang Medis]                                                          |
|   A1 GAMBARAN DARAH TEPI                    qty 1   6 Okt 2026   [Aktif]   |
|                                                                            |
| [Obat & Alkes]                                                             |
|   PARACETAMOL 500 MG TABLET                 qty 10  6 Okt 2026   [Aktif]   |
+----------------------------------------------------------------------------+
(Seluruh kolom rupiah, badge subtotal, dan panel Total Tagihan Berjalan HILANG 100%)
```

---

### Tabel Wilayah Antarmuka Baru

| Wilayah | Isi | Sumber Data | Komponen |
| --- | --- | --- | --- |
| Header Seksi | Judul seksi, deskripsi waktu kalkulasi, dan tombol muat ulang utama | `breakdownData.calculatedAt` | `BaseButton` (`variant="outline"`) |
| Pedoman Bangsal | Alert panduan etika perawat bangsal | Statis | Alert Info dengan ikon `RiInformationLine` |
| Kartu Status Kasir | Status izin pulang dari kasir, catatan kendala, dan status evaluasi kasir | `GET .../billing-status` | `BillingSummaryCard` |
| Tabel Kelompok Layanan | 7 Kelompok layanan V1 dengan daftar item, kuantitas, tanggal, dan status | `GET .../breakdown` | Tabel Bootstrap responsif bergaris |
| Indikator Subtotal Kelompok | Angka rupiah terformat ATAU badge `"Belum dikalkulasi kasir"` jika null | `GET .../breakdown/amounts` | Badge monospaced / `StatusBadge` |
| Panel Total Berjalan | Angka total rupiah berjalan ATAU badge `"Menunggu kalkulasi kasir"` | `GET .../breakdown/amounts` | Card berlatar belakang `bg-primary text-white` |

### Tabel Tombol dan Aksi

| Label Tombol | Jenis | Kapan Aktif | Yang Terjadi Saat Diklik |
| --- | --- | --- | --- |
| `[Muat Ulang]` | `BaseButton` (outline, sm) | Selalu aktif saat tidak loading | Memuat ulang breakdown tagihan dan menyegarkan status izin kasir secara serentak |

### Tabel Keadaan Layanan

| Keadaan | Penanganan pada Antarmuka |
| --- | --- |
| Sedang Memuat (*Loading*) | Indikator spinner aktif pada `ClinicalStateBoundary` dan tombol `[Muat Ulang]` dinonaktifkan sementara |
| Tagihan Belum Terbentuk (`NOT_FORMED`) | Banner informatif: *"Tagihan belum terbentuk. Belum ada folio tagihan aktif atau kalkulasi biaya yang terbentuk untuk episode rawat inap ini."* (Bukan Rp 0) |
| Kalkulasi Belum Dieksekusi Kasir (`calculatedAt === null`) | Subtotal kelompok menampilkan lencana teks `"Belum dikalkulasi"`, dan panel total menampilkan `"Menunggu kasir"`. **Sama sekali tidak menampilkan Rp 0.** |
| Kalkulasi Sah Selesai (`subtotalAmount > 0`) | Subtotal dan total menampilkan angka terformat rapi (`Rp 1.250.000`) khusus untuk pemegang izin `ViewAmount`. |
| Perawat Biasa (`!canViewAmount`) | Seluruh teks subtotal, badge nominal, dan kartu Total Tagihan Berjalan disembunyikan secara bersih tanpa sisa ruang kosong. |

---

## 4. Rincian Perbaikan

### FIX-KEP-011-01 — Eliminasi Fallback "Rp 0" Palsu dan Penyajian Lencana Status Kalkulasi

| | |
| --- | --- |
| **Menutup** | `ISS-KEP-011-01` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend (`nursing-billing-section.jsx`) |
| **Berkas yang diubah** | `src/components/view/health-services/inpatient-management/nursing-workspace/sections/billing/nursing-billing-section.jsx` |
| **Radius dampak** | Seksi Tagihan Pasien pada Ruang Kerja Keperawatan Rawat Inap (`FE-KEP-23`) |
| **Bergantung pada** | `FIX-KEP-011-05` |

**Langkah Pengerjaan:**
1. Pada `nursing-billing-section.jsx` baris 231–237, ubah rendering subtotal kelompok:
   - Jika `subtotalAmount !== null && subtotalAmount !== undefined`, tampilkan `formatBillingIdr(subtotalAmount)`.
   - Jika `subtotalAmount === null`, tampilkan lencana kecil `<span className="badge bg-light text-muted border">Belum dikalkulasi</span>`.
   - Hapus operator fallback `|| "Rp 0"`.
2. Pada baris 297–307, ubah rendering Total Tagihan Berjalan:
   - Jika `amountsData?.runningTotalAmount !== null && amountsData?.runningTotalAmount !== undefined`, tampilkan `formatBillingIdr(amountsData.runningTotalAmount)`.
   - Jika bernilai `null`, tampilkan `<span className="fs-6 badge bg-warning text-dark">Menunggu kalkulasi kasir</span>`.
   - Hapus operator fallback `|| "Rp 0"`.

**Acceptance Criteria:**
1. Pasien dengan tindakan klinis aktif namun kasir belum melakukan kalkulasi (`amountsData.runningTotalAmount === null`) tidak pernah menampilkan teks `"Rp 0"` di subtotal maupun total berjalan.
2. Subtotal kelompok pada kondisi di atas menampilkan badge informatif `"Belum dikalkulasi"`.
3. Panel Total Tagihan Berjalan menampilkan badge informatif `"Menunggu kalkulasi kasir"`.
4. Jika kasir telah melakukan kalkulasi dan nominal adalah Rp 0 murni (misalnya 100% gratis/FOC terverifikasi), sistem menampilkan `"Rp 0"`.

**Verifikasi:**
Jalankan pengujian unit `npm run test -- tests/unit/inpatient-nursing-finishing-roadmap.test.mjs` dan verifikasi bahwa tidak ada kemunculan "Rp 0 palsu".

**Risiko:**
Penyesuaian teks badge harus tetap rapi pada layar tablet/ponsel (*responsive layout*).

---

### FIX-KEP-011-02 — Penyelarasan Pesan Privasi Kasir Kontekstual

| | |
| --- | --- |
| **Menutup** | `ISS-KEP-011-02` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend (`billing-summary-card.jsx`, `nursing-billing-section.jsx`) |
| **Berkas yang diubah** | `src/components/features/health-services/inpatient-management/billing-integration/billing-summary-card.jsx`<br/>`src/components/view/health-services/inpatient-management/nursing-workspace/sections/billing/nursing-billing-section.jsx` |
| **Radius dampak** | Detail Episode Rawat Inap (`FE-INP-04`) dan Ruang Kerja Keperawatan (`FE-KEP-07`) |
| **Bergantung pada** | `FIX-KEP-011-05` |

**Langkah Pengerjaan:**
1. Tambahkan prop opsional `showPrivacyNotice = true` dan `customPrivacyNotice = null` pada `BillingSummaryCard`.
2. Pada `nursing-billing-section.jsx`, teruskan prop tersebut:
   - Bila `isAuthorizedViewAmount` bernilai `true`: oper `customPrivacyNotice="Subtotal dan total tagihan berjalan ditampilkan khusus untuk pemantauan administratif pemegang wewenang. Rincian tarif per item tetap steril di kasir."`
   - Bila `isAuthorizedViewAmount` bernilai `false`: biarkan default (`CASHIER_STATUS_MESSAGES.PRIVACY`).

**Acceptance Criteria:**
1. Saat dibuka oleh akun dengan izin `ViewAmount`, kartu status kasir tidak lagi menampilkan kalimat kontradiktif *"Layar bangsal tidak menampilkan rupiah"*.
2. Saat dibuka oleh perawat tanpa izin `ViewAmount`, kartu status kasir tetap menampilkan kalimat privasi standar *"Layar bangsal tidak menampilkan rupiah. Rincian nominal hanya terbaca di layar kasir."*

**Verifikasi:**
Inspeksi komponen dengan simulasi izin ganda dan pastikan kalimat berubah sesuai wewenang.

**Risiko:**
Pastikan prop baru memiliki nilai default yang aman sehingga tidak mempengaruhi tampilan `BillingSummaryCard` di layar Detail Episode `FE-INP-04`.

---

### FIX-KEP-011-03 — Penyempurnaan Teks Status Waktu Penilaian Kasir

| | |
| --- | --- |
| **Menutup** | `ISS-KEP-011-03` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend (`billing-summary-card.jsx`, `inpatient-billing-status-constants.js`) |
| **Berkas yang diubah** | `src/components/features/health-services/inpatient-management/billing-integration/billing-summary-card.jsx` |
| **Radius dampak** | Kartu status kasir rawat inap |
| **Bergantung pada** | Tidak ada |

**Langkah Pengerjaan:**
1. Pada `billing-summary-card.jsx` baris 136–138, perbarui fallback teks:
   ```jsx
   {billingStatus?.evaluatedAt
     ? `Dinilai kasir ${formatDateTime(billingStatus.evaluatedAt)}`
     : "Belum dievaluasi kasir (dilakukan saat alur kepulangan dimulai)"}
   ```

**Acceptance Criteria:**
1. Bila `billingStatus.evaluatedAt` bernilai null, footer menampilkan teks yang jelas: *"Belum dievaluasi kasir (dilakukan saat alur kepulangan dimulai)"*.

**Verifikasi:**
Pengujian visual pada episode aktif yang belum memiliki jadwal pulang.

---

### FIX-KEP-011-04 — Integrasi Tombol Pemuatan Ulang Terkoordinasi

| | |
| --- | --- |
| **Menutup** | `ISS-KEP-011-04` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend (`nursing-billing-section.jsx`, `billing-summary-card.jsx`) |
| **Berkas yang diubah** | `src/components/features/health-services/inpatient-management/billing-integration/billing-summary-card.jsx`<br/>`src/components/view/health-services/inpatient-management/nursing-workspace/sections/billing/nursing-billing-section.jsx` |
| **Radius dampak** | Ruang Kerja Keperawatan (`FE-KEP-23`) |
| **Bergantung pada** | Tidak ada |

**Langkah Pengerjaan:**
1. Tambahkan prop opsional `showRefreshButton = true` pada `BillingSummaryCard`.
2. Pada `nursing-billing-section.jsx`, teruskan `showRefreshButton={false}` pada kartu kasir dan sediakan mekanisme refresh ganda pada tombol utama `[Muat Ulang]` di header seksi Tagihan Pasien.

**Acceptance Criteria:**
1. Hanya terdapat tepat satu tombol `[Muat Ulang]` pada seksi Tagihan Pasien Bangsal.
2. Menekan tombol `[Muat Ulang]` tersebut menyegarkan status kasir dan rincian item layanan sekaligus.

---

### FIX-KEP-011-05 — Pengetatan Gate Otorisasi `ViewAmount`

| | |
| --- | --- |
| **Menutup** | `ISS-KEP-011-05` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend (`nursing-billing-section.jsx`) |
| **Berkas yang diubah** | `src/components/view/health-services/inpatient-management/nursing-workspace/sections/billing/nursing-billing-section.jsx` |
| **Radius dampak** | Keamanan dan privasi data finansial bangsal |
| **Bergantung pada** | Tidak ada |

**Langkah Pengerjaan:**
1. Buat konstanta wewenang ketat:
   ```javascript
   const isAuthorizedViewAmount = Boolean(viewAmountLoaded && canViewAmount);
   ```
2. Gantikan seluruh pengecekan `canViewAmount` pada pemanggilan API (baris 84) dan pada rendering elemen JSX (baris 231 dan 297) dengan `isAuthorizedViewAmount`.

**Acceptance Criteria:**
1. Sesi perawat tanpa izin `ViewAmount` tidak pernah mengirimkan request HTTP ke endpoint `/amounts` saat inisialisasi awal.
2. Tidak terjadi *flickering* tampilan komponen rupiah sebelum data perizinan selesai dimuat dari backend.

---

## 5. Urutan Pengerjaan

```text
FIX-KEP-011-05 (Pengetatan Gate Hak Akses ViewAmount)
├── FIX-KEP-011-01 (Eliminasi Nilai Palsu "Rp 0" pada Subtotal & Total)
├── FIX-KEP-011-02 (Penyelarasan Pesan Privasi Kasir Kontekstual)
└── FIX-KEP-011-04 (Penyatuan Tombol Muat Ulang Terkoordinasi)
FIX-KEP-011-03 (Penyempurnaan Teks Waktu Evaluasi Kasir — mandiri)
```

---

## 6. Keputusan Pemilik yang Dibutuhkan

| No | Keputusan | Pilihan | Rekomendasi | Status Keputusan |
| :---: | --- | --- | --- | --- |
| K-01 | Format tampilan subtotal saat kasir belum menghitung tagihan | (A) Lencana teks `"Belum dikalkulasi"`<br/>(B) Strip `"—"`<br/>(C) Sembunyikan subtotal | **Pilihan A** | ✅ DISETUJUI oleh Pemilik Sistem (2026-10-07) |
| K-02 | Penyesuaian pesan privasi kartu kasir saat dibuka pemegang wewenang | (A) Tampilkan catatan pemantauan administratif<br/>(B) Sembunyikan catatan privasi | **Pilihan A** | ✅ DISETUJUI oleh Pemilik Sistem (2026-10-07) |

---

## 7. Dokumen Hulu yang Ikut Direvisi

| Dokumen | Bagian | Sekarang | Menjadi |
| --- | --- | --- | --- |
| `03-frontend-architecture.md` | Bagian `FE-KEP-23`, baris 723 (Subtotal dan total) | *"Tidak dipanggil bila pengguna tidak berhak; tidak ada tempat kosong yang menyiratkan nol"* | *"Tidak dipanggil bila pengguna tidak berhak; jika berhak namun kalkulasi belum tersedia, tampilkan lencana status 'Belum dikalkulasi', BUKAN 'Rp 0'"* |
| `03-frontend-architecture.md` | Bagian `FE-KEP-23`, baris 725 (Catatan) | *"Rincian harga per layanan tersedia di kasir."* | *"Menyesuaikan catatan privasi secara kontekstual terhadap wewenang ViewAmount pengguna."* |

---

## 8. Verifikasi Menyeluruh

Setelah kelima perbaikan diimplementasikan oleh builder yang berwenang, verifikasi ujung-ke-ujung mencakup:
1. **Skenario Akun Perawat Pelaksana (`!canViewAmount`):**
   - Buka ruang kerja keperawatan pasien rawat inap aktif.
   - Buka menu **Tagihan Pasien**.
   - Pastikan kartu kasir menampilkan teks: *"Layar bangsal tidak menampilkan rupiah. Rincian nominal hanya terbaca di layar kasir."*
   - Pastikan seluruh tabel 7 kelompok menampilkan item, kuantitas, tanggal, dan status tanpa satu pun kata "Subtotal" atau angka rupiah.
   - Pastikan panel Total Tagihan Berjalan sama sekali tidak muncul.
   - Periksa tab Network peramban: pastikan **TIDAK ADA** request yang dikirimkan ke endpoint `.../breakdown/amounts`.
2. **Skenario Akun Supervisor Admisi / Keuangan (`canViewAmount`):**
   - Buka menu **Tagihan Pasien** pada pasien yang belum dihitung kasir.
   - Pastikan subtotal kelompok menampilkan lencana `[Belum dikalkulasi]` dan kartu total menampilkan `[Menunggu kalkulasi kasir]`.
   - Pastikan **TIDAK ADA** tulisan `"Rp 0"` palsu yang muncul.
   - Pastikan catatan pada kartu kasir berubah menjadi keterangan pemantauan administratif tanpa kontradiksi.
3. **Skenario Eksekusi Uji Otomatis:**
   - Jalankan `npm run test -- tests/unit/inpatient-nursing-finishing-roadmap.test.mjs` dan `npm run test -- tests/unit/inpatient-nursing-billing-summary.test.mjs` untuk menjamin kepatuhan seluruh Acceptance Criteria.
   - Jalankan pemeriksaan linter: `npx eslint --quiet` menghasilkan 0 error dan 0 warning.

---

## 9. Riwayat Dokumen

| Tanggal | Perubahan | Oleh |
| :---: | --- | :---: |
| 2026-10-06 | Rencana perbaikan dibuat dengan 5 butir perbaikan antarmuka; status menunggu persetujuan pemilik | `diagnose-module-issue` |
| 2026-10-07 | Rencana dan rekomendasi disetujui pengguna; seluruh 5 perbaikan diimplementasikan dan diverifikasi passing 11/11 | `diagnose-module-issue` |
