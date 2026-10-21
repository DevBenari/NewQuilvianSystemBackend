# PLAN-REPAIR-028 — Pemisahan Jalur Khusus Admisi Pasien Bayi dan Penataan Arsitektur Session Storage

```yaml
plan_id: PLAN-REPAIR-EPS-028
issue: ../issue/issue-028-pemisahan-alur-admisi-pasien-bayi.md
status_rencana: SELESAI_DIIMPLEMENTASIKAN
tanggal_rencana: "2026-10-17"
diputuskan_oleh: "Pemilik Sistem (User)"
tanggal_keputusan: "2026-10-17"
basis_source_backend: "d420c2fbe094a2d70776c44955233bce9c87bda8 (MHamzah)"
basis_source_frontend: "0accf387138a3fcc1ff40fbb5bdeb960076b4d59 (HamzahV2)"
```

---

## 1. Register Status Pengerjaan

| ID Perbaikan | Menutup | Ringkasan | Area | Prioritas | Task ID | Status | Bukti |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :--- |
| `FIX-EPS-028-01` | `ISS-EPS-028-03` | Tambahkan kartu entri ke-5 "Admisi Bayi Baru Lahir" (`entry=newborn`) pada layar utama Pilih Tipe Pendaftaran | Frontend | 1 | `FIX-EPS-028` | ✅ SELESAI | `INPATIENT_ADMISSION_ENTRY_OPTIONS`, `PatientEntryChoiceStep.jsx` (Ilustrasi 3D & .gridFive) |
| `FIX-EPS-028-02` | `ISS-EPS-028-01` | Tetapkan 7 langkah statis deterministik untuk alur `entry=newborn` dan hilangkan mutasi stepper dinamis pada alur umum | Frontend | 1 | `FIX-EPS-028` | ✅ SELESAI | `getInpatientAdmissionSteps`, `inpatient-newborn-admission.test.mjs` (9/9 lulus) |
| `FIX-EPS-028-03` | `ISS-EPS-028-02` | Standarisasi namespace kunci `sessionStorage` (`quilvian:inpatient-admission:*`) dan perbarui fungsi pembersihan sesi | Frontend | 1 | `FIX-EPS-028` | ✅ SELESAI | `useInpatientAdmissionFlow.jsx`, `clearInpatientAdmissionSession` (sweep legacy & current) |
| `FIX-EPS-028-04` | `ISS-EPS-028-T1` | Hapus pemilih episode ibu redundan dari form umum dan konsolidasikan pemilihan persalinan pada Langkah 1 Bayi | Frontend | 2 | `FIX-EPS-028` | ✅ SELESAI | `inpatient-admission-view.jsx` (hapus MotherEpisodeSelector), `INPATIENT_PATIENT_TYPE_OPTIONS` |

**Ringkasan: 4 dari 4 perbaikan selesai (100% selesai).**

---

## 2. Solusi Terpilih Per Temuan

### ISS-EPS-028-01 & ISS-EPS-028-03 — Pemisahan Pintu Masuk Admisi Bayi ke Kartu Mandiri

| Opsi | Pendekatan | Kelebihan | Kekurangan |
| :---: | :--- | :--- | :--- |
| A | Mempertahankan alur bayi di dalam "Pasien Baru", lalu menambah proteksi *fallback* dan perbaikan *routing* internal | Perubahan UI awal minimal | Arsitektur tetap rapuh; *stepper morphing* tetap ada; petugas tetap bingung mencari bayi di dalam form pasien baru umum |
| B | Menghapus alur bayi dari rawat inap dan memindahkannya ke modul rawat jalan/VK | Memisahkan form secara fisik | Mengubah proses pendaftaran ranap bayi yang memang harus membentuk episode ranap; memerlukan integrasi baru |
| **C — TERPILIH** | **Menambahkan kartu ke-5 "Admisi Bayi Baru Lahir" (`entry=newborn`) langsung pada layar utama `Pilih Tipe Pendaftaran`** | **100% deterministik; alur memiliki 7 langkah statis sejak awal; tidak ada mutasi stepper; UX admisi sangat jelas; isolasi state sempurna** | Menambah 1 kartu pilihan pada layar awal admisi (kini menjadi 5 kartu) |

**Solusi Terpilih: Opsi C (Win Solution Pengguna).**

1. **Alasan Operasional Rumah Sakit**: Pendaftaran bayi baru lahir (neonatus) adalah alur khusus yang berbeda secara fundamental dari pasien umum (tanpa scan KTP, identitas dari ibu, penjaminan terhubung kasus persalinan ibu, kamar rawat gabung/neonatal). Menampilkannya secara lugas di menu utama memudahkan staf pendaftaran rawat inap/bidan VK.
2. **Alasan Arsitektural**: Alur `entry=newborn` memiliki 7 langkah pasti sejak langkah pertama hingga akhir. Tidak ada pergantian *stepper* di tengah jalan, tidak ada lompatan URL liar, dan tidak ada ketergantungan pada logika *fallback slug* bersama.

---

### ISS-EPS-028-02 — Standarisasi Manajemen Session Storage

| Opsi | Pendekatan | Kelebihan | Kekurangan |
| :---: | :--- | :--- | :--- |
| A | Menghapus seluruh penggunaan `sessionStorage` dan hanya mengandalkan URL query parameter | Bersih dari residu peramban | Jika tab tertutup atau disegarkan saat input panjang di langkah identitas, seluruh data hilang seketika |
| **B — TERPILIH** | **Standarisasi namespace tunggal `quilvian:inpatient-admission:*`, gunakan kunci terisolasi per alur (`flow:newborn`), dan perbaiki pembersihan otomatis** | **Tahan terhadap refresh peramban tanpa risiko polusi silang; pembersihan sesi tuntas saat alur selesai/batal/beralih** | Memerlukan sinkronisasi kode pada hook alur dan utilitas penyimpanan |

**Solusi Terpilih: Opsi B.**

1. Seluruh kunci diseragamkan dengan format kanonikal: `quilvian:inpatient-admission:flow:<entryMode>:<sessionId>`.
2. Fungsi `clearInpatientAdmissionSession` diperbarui untuk membersihkan seluruh draf sesi yang diawali prefix kanonikal tersebut, sehingga tidak ada lagi draf usang (*zombie draft*) yang merusak pendaftaran berikutnya.

---

### ISS-EPS-028-T1 — Eliminasi Pemilih Ibu Redundan

**Solusi Terpilih:** Hapus kontrol `MotherEpisodeSelector` dari `PatientTypeStep` alur umum. Pemilihan kasus persalinan ibu dipusatkan 100% pada Langkah 1 alur bayi (`NewbornContextStep`) yang memang dirancang khusus untuk memvalidasi kasus persalinan aktif, kamar ibu, dan status penjaminan.

---

## 3. Skema Tampilan Sebelum → Sesudah

### 3.1 Layar Utama: Pilih Tipe Pendaftaran (FE-INP-01)

**SEBELUM**

```text
┌────────────────────────────────────────────────────────────────────────────────────────┐
│ Health Services / Rawat Inap                                                           │
│ Admisi Rawat Inap                                                                      │
│ Daftarkan pasien, tentukan penjamin dan DPJP, lalu pesan tempat tidurnya.              │
└────────────────────────────────────────────────────────────────────────────────────────┘

                                  ADMISI RAWAT INAP
                                Pilih Tipe Pendaftaran
  Tentukan apakah pasien didaftarkan baru, dicari dari rekam medis lama, atau berasal
             dari rujukan pasca operasi Kamar Pulih (IBS).

┌─────────────────────┐ ┌─────────────────────┐ ┌─────────────────────┐ ┌─────────────────────┐
│ PASIEN BELUM DAFTAR │ │ PASIEN TERDAFTAR    │ │ PASIEN PASCA OPERASI│ │ TRANSFER IGD/POLI   │
│ Pendaftaran Pasien  │ │ Pendaftaran Pasien  │ │ Admisi dari Kamar   │ │ Admisi Transfer IGD │
│ Baru                │ │ Lama                │ │ Pulih               │ │ & Poliklinik        │
│                     │ │                     │ │                     │ │                     │
│ • Scan KTP bila ada │ │ • Cari No RM / NIK  │ │ • Rujukan anestesi  │ │ • Data terisi auto  │
│ • 12 langkah urut   │ │ • Tinjau identitas  │ │ • Riwayat IBS terisi│ │ • DPJP terhubung    │
│ • Kartu di akhir    │ │ • Langkah bersama   │ │ • Tagihan tertaut   │ │ • Notifikasi kamar  │
│                     │ │                     │ │                     │ │                     │
│                 [>] │ │                 [>] │ │                 [>] │ │                 [>] │
└─────────────────────┘ └─────────────────────┘ └─────────────────────┘ └─────────────────────┘
  (Bayi terselip di                                                                            
   dalam Pasien Baru)
```

**SESUDAH**

```text
┌────────────────────────────────────────────────────────────────────────────────────────┐
│ Health Services / Rawat Inap                                                           │
│ Admisi Rawat Inap                                                                      │
│ Daftarkan pasien, tentukan penjamin dan DPJP, lalu pesan tempat tidurnya.              │
└────────────────────────────────────────────────────────────────────────────────────────┘

                                  ADMISI RAWAT INAP
                                Pilih Tipe Pendaftaran
 Tentukan apakah pasien didaftarkan baru, dicari dari rekam medis lama, bayi baru lahir,
                atau berasal dari rujukan Kamar Pulih / Transfer IGD-Poli.

┌──────────────────┐ ┌──────────────────┐ ┌──────────────────┐ ┌──────────────────┐ ┌──────────────────┐
│ PASIEN UMUM BARU │ │ PASIEN LAMA      │ │ BAYI BARU LAHIR  │ │ PASCA OPERASI    │ │ TRANSFER RAWAT   │
│ Pendaftaran      │ │ Pendaftaran      │ │ Admisi Bayi Baru │ │ Admisi Kamar     │ │ Admisi Transfer  │
│ Pasien Baru      │ │ Pasien Lama      │ │ Lahir            │ │ Pulih (IBS)      │ │ IGD & Poliklinik │
│                  │ │                  │ │                  │ │                  │ │                  │
│ • Scan KTP / NIK │ │ • Cari No RM/NIK │ │ • Taut kasus ibu │ │ • Rujukan bedah  │ │ • Antrean rawat  │
│ • Biodata wali   │ │ • Tinjau riwayat │ │ • Rawat gabung / │ │ • Anestesi & IBS │ │ • Data otomatis  │
│ • 12 langkah     │ │ • 10 langkah     │ │   neonatal NICU  │ │ • Tagihan IBS    │ │ • 10 langkah     │
│   lengkap        │ │   berurutan      │ │ • 7 langkah bayi │ │ • 11 langkah     │ │                  │
│              [>] │ │              [>] │ │              [>] │ │              [>] │ │              [>] │
└──────────────────┘ └──────────────────┘ └──────────────────┘ └──────────────────┘ └──────────────────┘
```

#### Tabel Wilayah Layar Awal

| Wilayah | Isi & Deskripsi | Sumber Data / Props | Komponen |
| :--- | :--- | :--- | :--- |
| **Header Halaman** | Judul menu, breadcrumb, dan ringkasan fungsi admisi | Statis / i18n | Layout standar rawat inap |
| **Grid Pilihan Tipe** | 5 kartu pilihan entri admisi mandiri responsif | `INPATIENT_ADMISSION_ENTRY_OPTIONS` | `PatientEntryChoiceStep` + `BaseCheckboxCard` |
| **Kartu Bayi Baru Lahir** | Kartu ke-3 dengan badge toska `BAYI BARU LAHIR`, judul jelas, poin alur bayi | `INPATIENT_ADMISSION_ENTRY_MODE.NEWBORN` | `BaseCheckboxCard` |

---

### 3.2 Stepper Alur Admisi Bayi Baru Lahir (`entry=newborn`)

**Tampilan Stepper 7 Langkah Deterministik:**

```text
LANGKAH 1 DARI 7 — Pilih Ibu Persalinan
(•) 1. Pilih Ibu  ─── ( ) 2. Identitas ─── ( ) 3. Perawatan ─── ( ) 4. Tempat ─── ( ) 5. Persetujuan ─── ( ) 6. Tinjau ─── ( ) 7. Dokumen
    Persalinan            Bayi                 & DPJP               Tidur              Wali                  & Aktifkan        Bayi
```

| No | Slug Langkah | Judul Langkah | Tugas Petugas di Layar |
| :---: | :--- | :--- | :--- |
| 1 | `newborn-context` | Pilih Ibu Persalinan | Memilih ibu dari kasus operasi persalinan yang sedang berjalan/selesai. |
| 2 | `newborn-identity` | Identitas Bayi | Memasukkan tanggal, jam lahir, jenis kelamin, dan nama otomatis bayi. |
| 3 | `doctor` | Perawatan & DPJP Bayi | Memilih rawat gabung / neonatal terpisah, sumber keputusan, dan dokter penanggung jawab bayi. |
| 4 | `bed-selection` | Penempatan & Pemesanan | Memilih tempat tidur / inkubator bayi dan mengunci reservasi. |
| 5 | `consent-form` | Persetujuan Wali | Meminta tanda tangan persetujuan rawat inap dari orang tua/wali bayi. |
| 6 | `confirmation` | Tinjau & Aktifkan | Memeriksa seluruh ringkasan data bayi–ibu dan mengaktifkan admisi resmi. |
| 7 | `newborn-documents` | Dokumen Bayi | Mencetak gelang bayi, label bayi, dan bukti admisi bayi. |

---

## 4. Rincian Perbaikan

### FIX-EPS-028-01 — Penambahan Kartu Entri ke-5 "Admisi Bayi Baru Lahir" pada Pilihan Tipe Pendaftaran

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-028-03` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend & konstanta |
| **Berkas yang diubah** | `src/lib/constants/health-services/inpatient-management/inpatient-admission-flow-constants.jsx`<br>`src/components/view/health-services/inpatient-management/inpatient-admission-view.jsx` |
| **Radius dampak** | Layar awal Admisi Rawat Inap (`/health-services/inpatient-management/admissions`) |
| **Bergantung pada** | Keputusan Pemilik `K-01` |

**Langkah Pengerjaan:**
1. Tambahkan nilai enum baru pada `INPATIENT_ADMISSION_ENTRY_MODE`:
   ```javascript
   export const INPATIENT_ADMISSION_ENTRY_MODE = Object.freeze({
     ...PATIENT_ENTRY_MODE,
     NEWBORN: "newborn", // Entri khusus bayi baru lahir
     RECOVERY: "recovery",
     TRANSFER: "transfer",
   });
   ```
2. Tambahkan entri kartu baru pada `INPATIENT_ADMISSION_ENTRY_OPTIONS`:
   ```javascript
   Object.freeze({
     value: INPATIENT_ADMISSION_ENTRY_MODE.NEWBORN,
     badge: "BAYI BARU LAHIR",
     title: "Admisi Bayi Baru Lahir",
     description: "Gunakan untuk pendaftaran bayi baru lahir yang lahir di RS (VK/Kamar Operasi IBS).",
     bullets: Object.freeze([
       "Tautkan kasus persalinan ibu",
       "Rawat gabung atau neonatal terpisah",
       "Cetak gelang dan label identitas bayi",
     ]),
   }),
   ```
3. Perbarui `isKnownEntryMode` di `use-inpatient-admission-flow.jsx` agar mengenali `newborn`.

**Acceptance Criteria:**
1. Layar awal admisi rawat inap menampilkan 5 kartu pilihan yang rapi, sejajar, dan berpenampilan konsisten.
2. Kartu "Admisi Bayi Baru Lahir" dapat diklik dan langsung mengarahkan peramban ke `?entry=newborn&step=newborn-context`.
3. Tidak ada galat *console* atau pergeseran layout pada layar resolusi standar maupun laptop (responsif).

---

### FIX-EPS-028-02 — Penetapan 7 Langkah Statis dan Eliminasi Mutasi Stepper Dinamis

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-028-01` |
| **Area** | Frontend |
| **Jenis perubahan** | Hook alur navigasi & logika stepper |
| **Berkas yang diubah** | `src/lib/constants/health-services/inpatient-management/inpatient-admission-flow-constants.jsx`<br>`src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-flow.jsx`<br>`src/components/view/health-services/inpatient-management/inpatient-admission-view.jsx` |
| **Radius dampak** | Alur pendaftaran pasien baru, alur pasien lama, dan alur bayi |
| **Bergantung pada** | `FIX-EPS-028-01` |

**Langkah Pengerjaan:**
1. Daftarkan `NEWBORN_ADMISSION_STEPS` pada fungsi kanonikal `getInpatientAdmissionSteps`:
   ```javascript
   if (entryMode === INPATIENT_ADMISSION_ENTRY_MODE.NEWBORN) {
     return NEWBORN_ADMISSION_STEPS;
   }
   ```
2. Bersihkan `resolveInpatientAdmissionSteps` dari parameter `isNewborn`. Pemilihan langkah sepenuhnya ditentukan oleh `entryMode`.
3. Pada alur umum (`entry=new`), daftar langkah selalu 12 langkah statis. Hapus logika pengubahan daftar langkah di tengah jalan saat memilih tipe pasien.
4. Pada `useInpatientAdmissionFlow`, pastikan saat `entry=newborn`, nilai awal `patientType` langsung terinisialisasi sebagai `"newborn"`.

**Acceptance Criteria:**
1. Membuka `?entry=newborn` selalu menampilkan *stepper* 7 langkah: `Pilih Ibu Persalinan` (Langkah 1) hingga `Dokumen Bayi` (Langkah 7).
2. Pengguna tidak akan pernah melompat ke Langkah 3 atau Langkah 4 saat memulai pendaftaran bayi.
3. Tombol "Kembali" pada Langkah 1 Bayi mengembalikan pengguna secara aman ke layar awal `Pilih Tipe Pendaftaran`.

---

### FIX-EPS-028-03 — Standarisasi Namespace Session Storage dan Pembersihan Otomatis

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-028-02` |
| **Area** | Frontend |
| **Jenis perubahan** | Utilitas state dan persistensi sesi |
| **Berkas yang diubah** | `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-flow.jsx`<br>`src/utils/health-services/inpatient-management/inpatient-admission-session-utils.js`<br>`src/utils/health-services/inpatient-management/inpatient-admission-payment-storage.jsx`<br>`src/utils/health-services/inpatient-management/inpatient-admission-deposit-storage.jsx` |
| **Radius dampak** | Seluruh persistensi tab admisi rawat inap |
| **Bergantung pada** | `FIX-EPS-028-01` |

**Langkah Pengerjaan:**
1. Seragamkan konstanta prefix di `use-inpatient-admission-flow.jsx`:
   ```javascript
   const FLOW_STORAGE_PREFIX = "quilvian:inpatient-admission:flow:";
   ```
2. Buat kunci penyimpanan sesi terisolasi per mode:
   ```javascript
   const storageKey = useMemo(() => {
     const id = resumeEpisodeId || selectedPatientId || rawEntryMode || "draft";
     return `${FLOW_STORAGE_PREFIX}${rawEntryMode}:${id}`;
   }, [rawEntryMode, resumeEpisodeId, selectedPatientId]);
   ```
3. Perbarui `clearInpatientAdmissionSession`:
   - Iterasi `sessionStorage` dan hapus semua kunci yang diawali `"quilvian:inpatient-admission:"` atau `"quilvian_admission_flow_"`.
   - Tambahkan pembersihan draf alur (`clearFlowDraft`) saat `resetEntryMode()` atau saat memilih tipe pendaftaran baru pada `selectEntryMode()`.

**Acceptance Criteria:**
1. Menyelesaikan atau membatalkan admisi bayi membersihkan seluruh kunci `sessionStorage` terkait bayi.
2. Membuka "Pendaftaran Pasien Baru" umum tidak lagi merehidrasi state bayi dari sesi sebelumnya.
3. Tidak ada kebocoran data antar-tab atau antar-pasien pada sesi peramban yang sama.

---

### FIX-EPS-028-04 — Pembersihan Kontrol Ibu Redundan pada Alur Umum

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-028-T1`, `ISS-EPS-028-03` |
| **Area** | Frontend |
| **Jenis perubahan** | Komponen UI |
| **Berkas yang diubah** | `src/components/view/health-services/inpatient-management/inpatient-admission-view.jsx`<br>`src/lib/constants/health-services/inpatient-management/inpatient-admission-flow-constants.jsx` |
| **Radius dampak** | Langkah 2 Pasien Baru (`patient-type`) dan Langkah 2 Pasien Lama (`visit-category`) |
| **Bergantung pada** | `FIX-EPS-028-01`, `FIX-EPS-028-02` |

**Langkah Pengerjaan:**
1. Hapus kartu "Bayi Baru Lahir" dari `INPATIENT_PATIENT_TYPE_OPTIONS` pada alur pasien baru umum, atau beri keterangan pengalihan bahwa admisi bayi dilakukan melalui menu utama.
2. Hapus baris `{isNewborn && <MotherEpisodeSelector flow={flow} />}` dari `PatientTypeStep` di `inpatient-admission-view.jsx`.
3. Pastikan `NewbornContextStep` tetap menjadi satu-satunya tempat pemilihan ibu persalinan yang sah.

**Acceptance Criteria:**
1. Pada alur pasien baru reguler, tidak ada lagi kartu pemilihan episode ibu yang membingungkan.
2. Form pasien baru reguler murni mencakup Pasien Umum, Pegawai RS, dan One Day Care (ODC).

---

## 5. Urutan Pengerjaan

```text
K-01, K-02, K-03 (Persetujuan Pemilik Sistem)
│
├── FIX-EPS-028-01 (Kartu ke-5 "Admisi Bayi Baru Lahir" pada Tipe Pendaftaran)
│   │
│   ├── FIX-EPS-028-02 (Penetapan 7 Langkah Statis & Hapus Stepper Morphing)
│   │   │
│   │   └── FIX-EPS-028-04 (Pembersihan Kontrol Ibu Redundan di Form Pasien Baru)
│   │
│   └── FIX-EPS-028-03 (Standarisasi Namespace Session Storage & Pembersihan Otomatis)
```

---

## 6. Keputusan Pemilik yang Dibutuhkan

| No | Kode | Keputusan | Pilihan Rekomendasi | Perbaikan yang Ditahan |
| :---: | :---: | :--- | :--- | :--- |
| 1 | **K-01** | Pemisahan alur pendaftaran bayi menjadi kartu entri ke-5 mandiri pada layar awal admisi. | **Setuju (Opsi C)** — Pemisahan mandiri adalah solusi terbaik operasional dan arsitektur. | Menahan `FIX-EPS-028-01` dan `FIX-EPS-028-02`. |
| 2 | **K-02** | Penghapusan kartu bayi dari form pasien baru umum agar pintu masuk pendaftaran bayi tunggal dan tegas. | **Setuju** — Menghindari duplikasi pintu masuk yang membingungkan. | Menahan `FIX-EPS-028-04`. |
| 3 | **K-03** | Standarisasi kunci penyimpanan peramban menjadi `quilvian:inpatient-admission:*`. | **Setuju** — Menjamin kebersihan data sesi peramban. | Menahan `FIX-EPS-028-03`. |

---

## 7. Dokumen Hulu yang Ikut Direvisi

| Dokumen | Bagian | Keadaan Sekarang | Menjadi |
| :--- | :--- | :--- | :--- |
| `05-skema-tampilan.md` | Bagian 3.1 (Layar Utama Pilihan Tipe Admisi) | Menampilkan 4 kartu (Pasien Baru, Pasien Lama, Kamar Pulih, Transfer). | Menampilkan 5 kartu dengan penambahan "Admisi Bayi Baru Lahir". |
| `03-frontend-architecture.md` | Bagian Alur Navigasi & Entry Mode | `entryMode` hanya `new`, `existing`, `recovery`, `transfer`. Alur bayi adalah cabang dari `new`. | Menambahkan `newborn` sebagai `entryMode` resmi dengan 7 langkah statis. |

---

## 8. Verifikasi Menyeluruh

Setelah seluruh perbaikan diimplementasikan oleh builder, verifikasi ujung-ke-ujung berikut wajib dipenuhi:

1. **Uji Jalur Bayi Murni**:
   - Dari layar awal admisi, klik kartu **"Admisi Bayi Baru Lahir"**.
   - Sistem mendarat mulus di `?entry=newborn&step=newborn-context` (Langkah 1 dari 7).
   - Petugas memilih ibu dari daftar persalinan -> Klik Lanjut -> Berpindah ke Langkah 2 (`newborn-identity`).
   - Petugas mengisi data lahir bayi -> Klik Simpan & Lanjut -> Berpindah ke Langkah 3 (`doctor` - DPJP Bayi).
   - Petugas memilih mode rawat (rawat gabung) dan DPJP -> Klik Lanjut -> Berpindah ke Langkah 4 (`bed-selection`).
   - Petugas memilih tempat tidur -> Mengisi persetujuan wali -> Tinjau & Aktifkan -> Mencetak gelang/label di Langkah 7.
   - **Hasil**: Tidak ada lompatan langkah, data bayi tersimpan valid, penjamin mewarisi ibu.

2. **Uji Isolasi Pasien Baru Umum**:
   - Setelah menyelesaikan atau membatalkan pendaftaran bayi, buka kartu **"Pendaftaran Pasien Baru"**.
   - Sistem mendarat di `?entry=new&step=visit-type` (Langkah 1 dari 12).
   - **Hasil**: Layar bersih, tidak ada data ibu/bayi yang tertinggal, *stepper* menampilkan 12 langkah umum lengkap tanpa gangguan.

3. **Uji Validasi Teknis**:
   - Eksekusi targeted ESLint dan unit test frontend.
   - Eksekusi `npm run build` untuk memastikan tidak ada kesalahan kompilasi.

---

## 9. Riwayat

| Tanggal | Perubahan | Oleh |
| :--- | :--- | :--- |
| 2026-10-17 | Rencana perbaikan dibuat dengan 4 butir tindakan terstruktur, skema tampilan 5 kartu, urutan dependency, dan pengujian menyeluruh. Menunggu persetujuan pemilik sistem. | Antigravity AI Engine (diagnose-module-issue) |
| 2026-10-17 | Pemilik sistem menyetujui seluruh rencana perbaikan PLAN-REPAIR-028. Status diubah menjadi `DISETUJUI`. Pelaksanaan implementasi dimulai. | Pemilik Sistem (User) |
