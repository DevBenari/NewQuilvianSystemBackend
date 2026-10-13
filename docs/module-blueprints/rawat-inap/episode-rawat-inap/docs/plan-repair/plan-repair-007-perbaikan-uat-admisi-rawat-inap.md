# PLAN-REPAIR-007 — Perbaikan UAT Admisi Rawat Inap: Waktu Rujukan dan Integrasi Kebijakan Deposit

```yaml
plan_id: PLAN-REPAIR-EPS-007
issue: ../issue/issue-007-perbaikan-uat-admisi-rawat-inap.md
status_rencana: DISETUJUI
tanggal_rencana: "2026-10-12"
diputuskan_oleh: "Pemilik Sistem (User)"
tanggal_keputusan: "2026-10-12"
basis_source_backend: "9a108c6422e12e06080ca7dfccee0063a57dfa04 (MHamzah)"
basis_source_frontend: "3f8cb2facce438969ffa95a0cd172b5ebde7a333 (HamzahV2)"
```

---

## 1. Register Status Pengerjaan

| ID Perbaikan | Menutup | Ringkasan Perbaikan | Area | Prioritas | Task ID | Status | Bukti |
| :--- | :--- | :--- | :---: | :---: | :---: | :---: | :--- |
| `FIX-EPS-007-01` | `ISS-EPS-007-01` | Tambahkan tombol aksi "Sekarang" pada pemilih tanggal & jam rujukan | FE | 2 | — | BELUM DIKERJAKAN | — |
| `FIX-EPS-007-02` | `ISS-EPS-007-02` | Integrasikan API kebijakan deposit dan tampilkan batas minimum pada langkah Deposit | FE | 1 | — | BELUM DIKERJAKAN | — |
| `FIX-EPS-007-03` | `ISS-EPS-007-03` | Konfigurasi seeder idempotent / SQL kebijakan deposit Rp10.000.000 untuk pasien umum UAT | BE | 1 | — | BELUM DIKERJAKAN | — |
| `FIX-EPS-007-04` | `ISS-EPS-007-T1` | Verifikasi alur pelunasan kekurangan deposit di Workspace PPRI | FE + BE | 2 | — | BELUM DIKERJAKAN | — |

**Ringkasan: 0 dari 4 perbaikan selesai (Menunggu persetujuan pemilik).**

---

## 2. Solusi Terpilih Per Temuan

### ISS-EPS-007-01 — Tombol Pintas "Sekarang" pada Tanggal & Jam Rujukan

| Opsi | Pendekatan | Kelebihan | Kekurangan |
| :---: | :--- | :--- | :--- |
| **A** | Modifikasi langsung pada berkas form rujukan (`inpatient-admission-visit-type-step.jsx` dan `inpatient-admission-visit-category-step.jsx`) dengan menambahkan tombol di header/samping field | Perubahan terlokalisasi hanya pada modul Rawat Inap, tidak berisiko terhadap modul lain | Tidak otomatis tersedia bila modul lain membutuhkan fitur serupa |
| **B** | Modifikasi base component `BaseDateTimeField` pada `base-form-control.jsx` dengan menambahkan prop opsional `showNowButton` dan `nowButtonLabel="Sekarang"` | Seluruh form di sistem Quilvian dapat memakai ulang fitur tombol waktu sekarang secara konsisten | Radius dampak menyentuh base component bersama |

**Solusi Terpilih: Opsi B (dengan fallback Opsi A jika otorisasi base component dibatasi).**  
1. Menambahkan prop `showNowButton={true}` pada `BaseDateTimeField` sehingga kode pemanggil di langkah rujukan cukup menambahkan satu prop tanpa merombak layout JSX.
2. Ketika tombol diklik, sistem mengisi string waktu lokal format ISO (`YYYY-MM-DDTHH:mm`). Input kalender dan pemilih waktu jam:menit tetap dapat diedit secara manual oleh petugas.

*Kenapa bukan Opsi A murni:* Mengarang tombol ad-hoc di luar base component melanggar panduan konsistensi antarmuka Quilvian (`rules/frontend/base-component-catalog.md`).

---

### ISS-EPS-007-02 — Integrasi API Kebijakan Deposit pada Langkah Deposit

| Opsi | Pendekatan | Kelebihan | Kekurangan |
| :---: | :--- | :--- | :--- |
| **A** | Hardcode angka Rp 10.000.000 langsung pada konstanta frontend `inpatient-admission-flow-constants.jsx` | Sangat cepat diimplementasikan | Melanggar aturan tata kelola (dilarang mengarang angka bisnis); kebijakan rumah sakit tidak dinamis |
| **B** | Panggil API backend `GET /api/v1/health-services/billing-management/billing/patient-funds/deposit-policies` dengan `guarantorId` dan `patientClassId` | Sesuai arsitektur dan kontrak resmi; dinamis menurut penjamin dan kelas rawat | Membutuhkan sinkronisasi data antar langkah |

**Solusi Terpilih: Opsi B.**  
1. Menambahkan fungsi `getDepositPolicy(guarantorId, patientClassId)` pada service frontend.
2. Hook `useInpatientAdmissionDeposit` memanggil API tersebut saat langkah Deposit aktif.
3. Menampilkan status kebijakan deposit secara jelas:
   - Jika ada kebijakan: Menampilkan alert info dengan nominal minimum kebijakan (contoh: `Rp 10.000.000`).
   - Jika keluarga membayar di bawah minimum (contoh: `Rp 9.000.000`): Menampilkan rincian bahwa terdapat kekurangan `Rp 1.000.000` yang dapat dilunasi di Workspace PPRI.
   - Tombol **Lanjut** tetap aktif (tidak pernah `disabled`), mematuhi invariant `RWI-DEC-095`.

---

### ISS-EPS-007-03 — Konfigurasi Minimum Deposit Rp10 Juta untuk Pasien Umum UAT

| Opsi | Pendekatan | Kelebihan | Kekurangan |
| :---: | :--- | :--- | :--- |
| **A** | Eksekusi manual SQL ad-hoc di database tanpa disimpan di repository | Cepat untuk kebutuhan tes sesaat | Tidak dapat direproduksi ulang (*not reproducible*); hilang jika database di-reset |
| **B** | Buat script SQL migrasi/seeder idempotent dan seeder C# idempotent `InpatientMasterDataSeeder.cs` khusus lingkungan non-produksi | Idempotent, tercatat di repository, aman dijalankan berulang kali | Membutuhkan pembuatan kode seeder |

**Solusi Terpilih: Opsi B.**  
Menyiapkan data master kebijakan default pada tabel `MstDepositPolicy`:
- `Code`: `DEP-POL-UMUM`
- `Name`: `Kebijakan Deposit Pasien Tunai / Umum`
- `GuarantorId`: `NULL`
- `PatientClassId`: `NULL` (berlaku universal untuk seluruh kelas umum sebagai baseline)
- `IsRequired`: `true`
- `MinimumAmount`: `10000000.00` (Rp 10.000.000)
- `FollowUpIntervalDays`: `3`
- `Description`: `Kebijakan deposit minimum Rp10.000.000 untuk pasien umum/tunai rawat inap (UAT).`
- `EffectiveFrom`: `2026-01-01 00:00:00+00`
- `IsActive`: `true`

---

## 3. Skema Tampilan Sebelum → Sesudah

### 3.1 Langkah 1: Formulir Rujukan Eksternal (Langkah 1)

**SEBELUM (Keadaan saat UAT):**
```text
+---------------------------------------------------------------------------------+
| DATA RUJUKAN FASKES LUAR                                                        |
| Form Isian Ringkas Rujukan Pasien                                               |
+---------------------------------------------------------------------------------+
| Nomor Surat Rujukan *                                                           |
| [ 0123/RUJ-RSUD/X/2026                                                        ] |
+---------------------------------------------------------------------------------+
| Tanggal & Jam Rujukan *                                                         |
| [ 12/10/2026       ]  [ 14:30 ▾ ]                                               |
| (Waktu surat rujukan diterbitkan oleh fasilitas kesehatan perujuk)              |
+---------------------------------------------------------------------------------+
| Fasilitas Kesehatan Perujuk *                                                   |
| [ Puskesmas Tebet Barat                                                       ] |
+---------------------------------------------------------------------------------+
```

**SESUDAH (Setelah Perbaikan FIX-EPS-007-01):**
```text
+---------------------------------------------------------------------------------+
| DATA RUJUKAN FASKES LUAR                                                        |
| Form Isian Ringkas Rujukan Pasien                                               |
+---------------------------------------------------------------------------------+
| Nomor Surat Rujukan *                                                           |
| [ 0123/RUJ-RSUD/X/2026                                                        ] |
+---------------------------------------------------------------------------------+
| Tanggal & Jam Rujukan *                                                         |
| [ 12/10/2026       ]  [ 14:30 ▾ ]  [ ⚡ Sekarang ]                              |
| (Waktu surat rujukan diterbitkan. Klik 'Sekarang' untuk waktu aktual saat ini)  |
+---------------------------------------------------------------------------------+
| Fasilitas Kesehatan Perujuk *                                                   |
| [ Puskesmas Tebet Barat                                                       ] |
+---------------------------------------------------------------------------------+
```

---

### 3.2 Langkah 5: Deposit / Uang Muka Pasien

**SEBELUM (Keadaan saat UAT):**
```text
+---------------------------------------------------------------------------------+
| Langkah Deposit                                                                 |
| Deposit / Uang Muka                                                             |
| Catat uang muka yang diterima dari keluarga pasien.                             |
+---------------------------------------------------------------------------------+
| ℹ Minimum kebijakan belum dapat ditampilkan                                    |
| Minimum deposit menurut penjamin dan kelas perawatan belum dapat dibaca.       |
| Nominal berapa pun tetap dapat dicatat, dan admisi tidak tertahan.              |
+---------------------------------------------------------------------------------+
| Nominal Diterima                                                                |
| Rp [ 9.000.000                                                                ] |
| Kosongkan bila keluarga belum membayar uang muka. Admisi tetap dapat dilanjutkan|
+---------------------------------------------------------------------------------+
| [ < Kembali ]                                                       [ Lanjut > ]|
+---------------------------------------------------------------------------------+
```

**SESUDAH (Setelah Perbaikan FIX-EPS-007-02):**
```text
+---------------------------------------------------------------------------------+
| Langkah Deposit                                                                 |
| Deposit / Uang Muka                                                             |
| Catat uang muka yang diterima dari keluarga pasien.                             |
+---------------------------------------------------------------------------------+
| ℹ Kebijakan Deposit Rawat Inap (Pasien Umum / Tunai)                            |
| Ketentuan Minimum Deposit: Rp 10.000.000                                        |
| Jatuh Tempo Pelunasan: 3 Hari Kerja Pasca Admisi                                |
+---------------------------------------------------------------------------------+
| Nominal Diterima                                                                |
| Rp [ 9.000.000                                                                ] |
|                                                                                 |
| ⚠ Status Pembayaran: Terbayar Sebagian                                          |
| Kekurangan deposit sebesar Rp 1.000.000 akan ditagihkan dan dapat dibuatkan     |
| Surat Pernyataan Kesediaan Melunasi pada Workspace PPRI.                        |
|                                                                                 |
| Catatan: Pembayaran deposit di bawah batas minimum tidak membatalkan admisi.   |
+---------------------------------------------------------------------------------+
| [ < Kembali ]                                                       [ Lanjut > ]|
+---------------------------------------------------------------------------------+
```

#### Tabel Komponen dan Keadaan Antarmuka Langkah Deposit

| Wilayah | Konten / Data | Sumber Data | Komponen UI |
| :--- | :--- | :--- | :--- |
| **Panel Informasi Kebijakan** | Ketentuan minimum Rp 10.000.000 & jatuh tempo 3 hari | API Backend `GET /deposit-policies` | `<InformationAlert variant="info">` |
| **Input Nominal Uang Muka** | Angka rupiah yang dibayar keluarga (misal Rp 9.000.000) | State lokal `deposit.amount` | `input[type="text"]` (Numeric sanitasi) |
| **Indikator Kekurangan** | Selisih Rp 1.000.000 jika `amount < minimumAmount` | Komputasi Klien (`minimum - amount`) | `<InformationAlert variant="warning">` |
| **Tombol Navigasi** | Tombol Kembali & Tombol Lanjut | Aksi Flow Admisi | `<BaseButton variant="secondary">` & `<BaseButton variant="primary">` |

---

## 4. Rincian Perbaikan

### FIX-EPS-007-01 — Tambahkan Tombol Waktu "Sekarang" pada Formulir Rujukan

| Parameter | Keterangan |
| :--- | :--- |
| **Menutup** | `ISS-EPS-007-01` |
| **Area** | Frontend |
| **Jenis Perubahan** | Source Frontend |
| **Berkas yang Diubah** | 1. `src/components/features/base-features/base-form-control.jsx` — dukung prop `showNowButton`<br>2. `src/components/view/health-services/inpatient-management/inpatient-admission-visit-type-step.jsx`<br>3. `src/components/view/health-services/inpatient-management/inpatient-admission-visit-category-step.jsx` |
| **Radius Dampak** | Formulir rujukan admisi pasien baru dan pasien lama rawat inap |
| **Bergantung Pada** | Keputusan K-01 |

**Langkah Implementasi:**
1. Tambahkan fungsi helper waktu lokal sekarang format ISO (`getNowLocalDateTimeIso()`) yang menghasilkan string `YYYY-MM-DDTHH:mm`.
2. Pasang tombol `Sekarang` bertipe `type="button"` di samping kontrol jam pada `BaseDateTimeField` ketika `showNowButton={true}`.
3. Pastikan event klik mengisi nilai state `referralDateTime` tanpa memicu submit form.
4. Pastikan input manual kalender dan waktu tetap berfungsi normal jika petugas ingin mengubah kembali.

**Acceptance Criteria:**
1. Pada langkah 1 Jenis Kunjungan (opsi Rujukan), field Tanggal & Jam Rujukan menampilkan tombol bertuliskan **"Sekarang"**.
2. Mengklik tombol "Sekarang" seketika mengisi kolom tanggal dengan tanggal hari ini dan jam dengan jam lokal saat ini (misal `2026-10-12T14:45`).
3. Petugas dapat mengedit tanggal secara manual setelah menekan tombol tanpa terjadi galat validasi.
4. Menekan tombol "Sekarang" tidak memicu submit formulir secara otomatis.

**Verifikasi:**
- Jalankan pemeriksaan lint frontend: `npm run lint`.
- Verifikasi visual browser: klik tombol "Sekarang", periksa nilai input terisi waktu aktual.

---

### FIX-EPS-007-02 — Integrasikan API Kebijakan Deposit pada Langkah Deposit

| Parameter | Keterangan |
| :--- | :--- |
| **Menutup** | `ISS-EPS-007-02` |
| **Area** | Frontend |
| **Jenis Perubahan** | Source Frontend |
| **Berkas yang Diubah** | 1. `src/lib/services/health-services/inpatient-management/inpatient-admission-payment.service.js`<br>2. `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-deposit.jsx`<br>3. `src/components/view/health-services/inpatient-management/inpatient-admission-deposit-step.jsx`<br>4. `src/components/view/health-services/inpatient-management/inpatient-admission-view.jsx` |
| **Radius Dampak** | Layar Deposit Admisi Rawat Inap |
| **Bergantung Pada** | `FIX-EPS-007-03` (backend endpoint & data kebijakan tersedia) |

**Langkah Implementasi:**
1. Buat fungsi service `fetchInpatientDepositPolicy(guarantorId, patientClassId)` yang memanggil `GET /v1/health-services/billing-management/billing/patient-funds/deposit-policies`.
2. Pada `useInpatientAdmissionDeposit`, tambahkan state untuk menyimpan objek kebijakan: `policyData`, `policyLoading`, dan `policyError`.
3. Pada `InpatientAdmissionView`, teruskan `patientClassId` dan `guarantorId` yang dipilih dari controller pembayaran ke hook `deposit`.
4. Pada `InpatientAdmissionDepositStep`, ganti alert statis dengan komponen dinamis yang menampilkan nominal kebijakan minimum.
5. Hitung selisih jika nominal yang diketik lebih kecil dari nominal kebijakan minimum, lalu tampilkan informasi kekurangan secara ramah.
6. Pastikan tombol **Lanjut** tetap aktif (tidak `disabled`) pada kondisi apa pun (nominal 0, di bawah minimum, atau pas).

**Acceptance Criteria:**
1. Ketika layar deposit dimuat untuk pasien umum kelas 1, sistem berhasil membaca kebijakan dari API dan menampilkan: *"Ketentuan Minimum Deposit: Rp 10.000.000"*.
2. Jika petugas mengetik nominal `Rp 9.000.000`, sistem menampilkan peringatan informatif kuning: *"Kekurangan deposit sebesar Rp 1.000.000..."*.
3. Jika petugas mengosongkan nominal (`Rp 0`), sistem menampilkan informasi bahwa seluruh deposit Rp 10.000.000 menjadi kekurangan yang ditagihkan nanti.
4. Tombol **Lanjut** dapat diklik dan berhasil mengarahkan petugas ke langkah berikutnya (Dokter / Bed) tanpa hambatan.

**Verifikasi:**
- Jalankan lint frontend: `npm run lint`.
- Verifikasi network tab: request ke `/patient-funds/deposit-policies` menghasilkan HTTP 200 dengan data kebijakan.

---

### FIX-EPS-007-03 — Konfigurasi Seeder & SQL Idempotent Kebijakan Deposit Rp10 Juta UAT

| Parameter | Keterangan |
| :--- | :--- |
| **Menutup** | `ISS-EPS-007-03` |
| **Area** | Backend & Database |
| **Jenis Perubahan** | Script SQL & Seeder C# Backend |
| **Berkas yang Diubah** | 1. Script SQL `docs/seed-scripts/seed-mst-deposit-policy-uat.sql`<br>2. `Areas/HealthServices/MasterData/Seeders/InpatientMasterDataSeeder.cs` (atau seeder billing terkait) |
| **Radius Dampak** | Konfigurasi master data billing & rawat inap lingkungan Dev/UAT |
| **Bergantung Pada** | Keputusan K-02 |

**Langkah Implementasi:**
1. Buat script SQL idempotent untuk menyisipkan kebijakan deposit default umum pada tabel `public."MstDepositPolicy"`:

```sql
INSERT INTO public."MstDepositPolicy" (
    "Id", "Code", "Name", "GuarantorId", "PatientClassId",
    "IsRequired", "MinimumAmount", "FollowUpIntervalDays",
    "Description", "EffectiveFrom", "EffectiveTo", "IsActive",
    "CreatedUtc", "CreatedBy"
)
VALUES (
    'd1e43e52-19e3-4d7a-8d19-bb193a778ef2',
    'DEP-POL-UMUM',
    'Kebijakan Deposit Pasien Tunai / Umum',
    NULL,
    NULL,
    true,
    10000000.00,
    3,
    'Kebijakan deposit minimum Rp10.000.000 untuk pasien umum/tunai rawat inap (UAT).',
    '2026-01-01 00:00:00+00',
    NULL,
    true,
    NOW(),
    'SYSTEM_SEEDED'
)
ON CONFLICT ("Code") DO UPDATE SET
    "MinimumAmount" = EXCLUDED."MinimumAmount",
    "FollowUpIntervalDays" = EXCLUDED."FollowUpIntervalDays",
    "IsActive" = true;
```

2. Pada `InpatientMasterDataSeeder.cs`, tambahkan method `SeedDefaultDepositPolicyAsync` yang memeriksa keberadaan baris ber-code `DEP-POL-UMUM` dan menambahkannya jika belum ada, dengan pengaman lingkungan non-produksi.

**Acceptance Criteria:**
1. Script SQL berhasil dieksekusi tanpa error dan bersifat idempotent (dijalankan 2 kali tidak menghasilkan error duplikat).
2. Endpoint `GET /api/v1/health-services/billing-management/billing/patient-funds/deposit-policies` mengembalikan HTTP 200 dengan `minimumAmount: 10000000.0`.
3. Pasien jaminan asuransi/perusahaan tidak terdampak oleh penambahan kebijakan default umum ini.

**Verifikasi:**
- Eksekusi query select di database UAT: `SELECT "Code", "MinimumAmount" FROM public."MstDepositPolicy" WHERE "Code" = 'DEP-POL-UMUM'`.
- Request API via Postman/cURL menghasilkan objek JSON dengan `minimumAmount: 10000000.0`.

---

### FIX-EPS-007-04 — Verifikasi Keterhubungan Pelunasan Deposit di Workspace PPRI

| Parameter | Keterangan |
| :--- | :--- |
| **Menutup** | `ISS-EPS-007-T1` |
| **Area** | Frontend & Backend (Testing & Verifikasi Alur) |
| **Jenis Perubahan** | Verifikasi End-to-End |
| **Berkas yang Diperiksa** | `src/components/view/health-services/inpatient-management/admission-workspace/sections/documents/deposit/deposit-settlement-form.jsx` |
| **Radius Dampak** | Workspace PPRI — Tab Dokumen Admisi (Pelunasan Deposit) |
| **Bergantung Pada** | `FIX-EPS-007-02` dan `FIX-EPS-007-03` |

**Langkah Implementasi:**
1. Lakukan pendaftaran pasien baru rawat inap dengan input deposit `Rp 9.000.000`.
2. Selesaikan alur hingga episode rawat inap terbentuk (`InpEpisode` aktif).
3. Buka Workspace PPRI untuk episode pasien tersebut.
4. Buka tab **Dokumen Admisi** $\rightarrow$ **Pelunasan Deposit**.
5. Verifikasi bahwa formulir menampilkan ringkasan:
   - Kebijakan Minimum: `Rp 10.000.000`
   - Total Diterima: `Rp 9.000.000`
   - Kekurangan: `Rp 1.000.000`
   - Batas Jatuh Tempo: H+3 hari kerja pukul 11:00.
6. Simpan konsep dokumen dan pastikan Surat Pernyataan Kesediaan Melunasi Deposit dapat diterbitkan.

**Acceptance Criteria:**
1. Workspace PPRI berhasil menampilkan kekurangan deposit `Rp 1.000.000` secara otomatis tanpa perlu diketik ulang oleh petugas.
2. Surat Pernyataan Kesediaan Melunasi Deposit dapat dicetak dengan data penanggung jawab dan nominal kekurangan yang sesuai.

---

## 5. Urutan Pengerjaan

```text
FIX-EPS-007-03 (BE / Database: Seeder kebijakan Rp10 Juta)
│
├──► FIX-EPS-007-02 (FE: Integrasikan API kebijakan pada layar Deposit)
│    │
│    └──► FIX-EPS-007-04 (FE+BE: Verifikasi alur Workspace PPRI Pelunasan Deposit)
│
FIX-EPS-007-01 (FE: Tombol 'Sekarang' pada formulir rujukan)  ← Bebas dependency, dapat paralel
```

---

## 6. Keputusan Pemilik yang Dibutuhkan

| No. | Keputusan | Pilihan | Rekomendasi | Menahan Perbaikan |
| :---: | :--- | :--- | :--- | :--- |
| **K-01** | Bentuk dan posisi tombol "Sekarang" pada formulir rujukan | A. Teks kecil di samping label<br>B. Tombol aksi di sebelah pemilih waktu | **Pilihan B**: Tombol pintas berukuran sedang bersisian dengan pemilih waktu | `FIX-EPS-007-01` |
| **K-02** | Nilai `FollowUpIntervalDays` kebijakan deposit | A. 3 hari kerja<br>B. 1 hari kerja | **Pilihan A**: 3 hari kerja pasca admisi | `FIX-EPS-007-03` |

---

## 7. Dokumen Hulu yang Ikut Direvisi

| Dokumen | Bagian / Seksyen | Sekarang | Menjadi |
| :--- | :--- | :--- | :--- |
| `05-skema-tampilan.md` | Bagian 1 — Langkah 1 Jenis Kunjungan (Rujukan) | Hanya input tanggal & jam rujukan standar | Dilengkapi tombol aksi pintas `[ ⚡ Sekarang ]` |
| `05-skema-tampilan.md` | Bagian 1 — Langkah 5 Deposit | Menampilkan alert "Minimum kebijakan belum dapat dibaca" | Menampilkan ringkasan kebijakan minimum resmi dari billing dan indikator kekurangan deposit |
| `inpatient-admission-flow-constants.jsx` | `INPATIENT_DEPOSIT_STEP_TEXT` | Catatan legasi bahwa endpoint belum ada | Disesuaikan dengan teks operasional integrasi kebijakan deposit aktif |

---

## 8. Verifikasi Menyeluruh (Skenario Uji End-to-End)

Setelah perbaikan diimplementasikan, uji alur lengkap berikut wajib dijalankan:

1. **Skenario 1 — Pasien Rujukan & Tombol Waktu Sekarang:**
   - Buka Admisi Rawat Inap $\rightarrow$ Pendaftaran Pasien Baru $\rightarrow$ Pilih Kunjungan Rujukan.
   - Klik tombol **Sekarang**.
   - **Hasil:** Tanggal terisi tanggal hari ini dan jam terisi jam saat ini secara instan.
2. **Skenario 2 — Deposit di Bawah Minimum (Rp9 Juta vs Rp10 Juta):**
   - Lengkapi identitas pasien (Tn. Budi Santoso, NIK 16 digit, alamat).
   - Pada Langkah 4 (Pembayaran), pilih cara bayar Tunai / Umum, kelas perawatan Kelas 1.
   - Masuk ke Langkah 5 (Deposit).
   - **Hasil:** Layar menampilkan kebijakan minimum `Rp 10.000.000`.
   - Ketik nominal `9000000`.
   - **Hasil:** Sistem menampilkan indikator kekurangan `Rp 1.000.000`.
   - Klik tombol **Lanjut**.
   - **Hasil:** Sistem mengizinkan lanjut ke pemilihan kamar/bed tanpa hambatan.
3. **Skenario 3 — Selesai Admisi & Pelunasan di Workspace PPRI:**
   - Selesaikan admisi hingga terbentuk episode rawat inap.
   - Buka Workspace PPRI untuk episode tersebut $\rightarrow$ Masuk ke tab Pelunasan Deposit.
   - **Hasil:** Angka kekurangan `Rp 1.000.000` tampil secara akurat dari billing, dan petugas dapat menerbitkan Surat Pernyataan Kesediaan Melunasi Deposit.

---

## 9. Riwayat Dokumen

| Tanggal | Perubahan | Oleh |
| :---: | :--- | :---: |
| 2026-10-12 | Dokumen rencana perbaikan PLAN-REPAIR-007 dibuat berdasarkan diagnosis ISSUE-007 | AI Agent (`diagnose-module-issue`) |
| 2026-10-12 | Rencana disetujui pemilik sistem untuk dikerjakan seluruhnya | Pemilik Sistem (User) |
