# PLAN-REPAIR-003 — Membereskan Langkah 2 Pendaftaran Pasien Baru pada admisi rawat inap

```yaml
plan_id: PLAN-REPAIR-EPS-003
issue: ../issue/issue-003-pendaftaran-pasien-baru.md
status_rencana: SELESAI   # seluruh 10 perbaikan ✅ pada 2026-10-06
tanggal_rencana: "2026-10-06"
diputuskan_oleh: "Pemilik modul — perintah implementasi 2026-10-06"
tanggal_keputusan: "2026-10-06"
basis_source_backend: "00fc141f23a6cf04862235dfd3876be22e581b8a (MHamzah)"
basis_source_frontend: "b010ffb9722f236160477c95c931c22467550200 (HamzahV2)"
perubahan_backend: "Tidak ada — seluruh perbaikan source berada di frontend"
ditulis_dengan: "skill diagnose-module-issue (penggunaan pertama)"
```

Rencana ini menutup enam butir laporan dan tiga temuan tambahan pada
[ISSUE-003](../issue/issue-003-pendaftaran-pasien-baru.md). Seluruh perbaikan source berada di
**frontend**; backend sudah menyediakan semua yang dibutuhkan. Satu perbaikan berupa revisi dokumen
desain, karena salah satu butir ternyata sesuai skema yang disetujui.

---

## 1. Register status pengerjaan

| ID perbaikan | Menutup | Ringkasan | Area | Prioritas | Task ID | Status | Bukti |
| --- | --- | --- | --- | :---: | --- | --- | --- |
| `FIX-EPS-003-01` | `ISS-EPS-003-01` | Sembunyikan tombol "Input Manual" yang tidak berfungsi; tampilkan kalimat "isi formulir secara manual" | FE | 2 | `FE-RWI-203` | ✅ SELESAI 2026-10-06 | [FE-RWI-203](../../task/report/frontend/FE-RWI-203.md) |
| `FIX-EPS-003-02` | `ISS-EPS-003-02` | Label daftar kota memuat jenisnya: "Kabupaten Bekasi" / "Kota Bekasi" | FE | 1 | `FE-RWI-204` | ✅ SELESAI 2026-10-06 | [FE-RWI-204](../../task/report/frontend/FE-RWI-204.md) |
| `FIX-EPS-003-03` | `ISS-EPS-003-T1` | Pencocokan hasil scan KTP membedakan Kota dan Kabupaten; tidak menebak saat seri | FE | 1 | `FE-RWI-204` | ✅ SELESAI 2026-10-06 | [FE-RWI-204](../../task/report/frontend/FE-RWI-204.md) |
| `FIX-EPS-003-04` | `ISS-EPS-003-04` | Hapus seluruh isian UUID; Tier Membership dan Pasien Ibu menjadi daftar pilihan | FE | 1 | `FE-RWI-205` | ✅ SELESAI 2026-10-06 | [FE-RWI-205](../../task/report/frontend/FE-RWI-205.md) |
| `FIX-EPS-003-05` | `ISS-EPS-003-05` | Jam Lahir memakai `FilterTimePicker` | FE | 3 | `FE-RWI-205` | ✅ SELESAI 2026-10-06 | [FE-RWI-205](../../task/report/frontend/FE-RWI-205.md) |
| `FIX-EPS-003-06` | `ISS-EPS-003-06` | Susun ulang Data tambahan pasien menurut makna | FE | 2 | `FE-RWI-206` | ✅ SELESAI 2026-10-06 — `K-02` diambil sebagai `RWI-DEC-224` | [FE-RWI-206](../../task/report/frontend/FE-RWI-206.md) |
| `FIX-EPS-003-07` | `ISS-EPS-003-07` | Tombol Simpan selalu dapat ditekan; ringkasan dan fokus ke isian yang kurang | FE | 1 | `FE-RWI-207` | ✅ SELESAI 2026-10-06 | [FE-RWI-207](../../task/report/frontend/FE-RWI-207.md) |
| `FIX-EPS-003-08` | `ISS-EPS-003-T2` | Pesan galat scanner berbahasa Indonesia, bukan "Failed to fetch" | FE | 3 | `FE-RWI-203` | ✅ SELESAI 2026-10-06 | [FE-RWI-203](../../task/report/frontend/FE-RWI-203.md) |
| `FIX-EPS-003-09` | `ISS-EPS-003-T3` | "Metode Persalinan" pindah ke data kelahiran bayi | FE | 3 | `FE-RWI-206` | ✅ SELESAI 2026-10-06 | [FE-RWI-206](../../task/report/frontend/FE-RWI-206.md) |
| `FIX-EPS-003-10` | `ISS-EPS-003-07`, `ISS-EPS-003-06` | Revisi skema tampilan 3.3 dan 4.2 agar tidak membantah source | Dokumen | 1 | — (dokumen) | ✅ SELESAI 2026-10-06 | `../../05-skema-tampilan.md` revision `0.6`; `RWI-DEC-223`, `RWI-DEC-224` pada `../../../00-interview-decisions.md` revision `34` |

**Ringkasan: 10 dari 10 perbaikan selesai** (6 Oktober 2026). Validasi bersama: `npx eslint src --quiet`
exit `0`; `npm run build` PASS; unit test `inpatient-admission-registration-issue-003.test.mjs` 9/9; suite unit
penuh 2502/2510 dengan 8 kegagalan di modul lain. Verifikasi peramban `NOT RUN`, dikecualikan atas keputusan
pemilik 1 dan 10 September 2026.

Kolom **Task ID** diisi saat task didaftarkan ke `roadmap/frontend-roadmap-v2.md`. Ruang nomor
`FE-RWI-###` dipakai bersama seluruh sub-modul rawat inap, jadi nomornya diambil pada saat itu dengan
mencari angka tertinggi di seluruh repository, bukan dari `task_id_next_free` yang sering basi.

### 1.1 Di luar rencana ini

| Temuan | Kenapa tidak masuk | Diteruskan ke |
| --- | --- | --- |
| `ISS-EPS-003-03` — butir 3 kosong | Menunggu jawaban pelapor (`P-01`) | Pelapor |
| `ISS-EPS-003-T4` — rute Tier Membership keliru di registry select frontend dan metadata backend | Milik modul Patient Management; `FIX-EPS-003-04` cukup memakai rute yang benar tanpa memperbaiki registry | Pemilik modul Patient Management |
| `ISS-EPS-003-T5` — UUID "Membership Aktif" di master data pasien | Milik modul Patient Management | Pemilik modul Patient Management |
| Penyusunan ulang tiga kartu pada **pendaftaran IGD** | Milik modul pendaftaran IGD. Rekomendasinya ada di bagian 3.3.4 | Pemilik pendaftaran IGD |

---

## 2. Solusi terpilih per temuan

### 2.1 `ISS-EPS-003-01` — Tombol "Input Manual"

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Tombol hanya dirender bila pemakai panel memberi `onUseManual`, sama seperti "Cek Scanner" dan "Hapus Hasil"; tambahkan kalimat petunjuk skema 3.3 | Konsisten dengan dua tombol lain; perubahan kecil; tidak ada pemakai lain yang terdampak | — |
| B | Beri tombol tugas baru: menggulir dan memfokuskan "Nama Lengkap" | Tombol menjadi berguna | Pelapor meminta tombol dihilangkan; formulir sudah terlihat tepat di bawahnya; tombol ini tidak ada di skema |
| C | Hapus kode tombol dari panel seluruhnya | Paling bersih | Menutup jalan bagi pemakai panel yang memang menyembunyikan formulirnya |

**Solusi terpilih: Opsi A.**

1. Memenuhi permintaan pelapor: tombol hilang dari admisi rawat inap.
2. Mengembalikan panel ke skema 3.3, termasuk kalimat penjelas yang diwajibkan bagian Keadaan.
3. `plustek-scan-panel.jsx` hanya dipakai admisi rawat inap, sehingga tidak ada layar lain yang berubah.

**Kenapa bukan opsi lain.** B menambah tombol yang tidak diminta siapa pun. C membuang kemampuan yang
mungkin dibutuhkan pemakai lain, padahal A sudah mencapai hasil yang sama.

### 2.2 `ISS-EPS-003-02` — Dua pilihan "Bekasi"

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Frontend membaca `additionalInfo` sebagai jenis kota | Murni frontend; kontrak tidak berubah; sejalan dengan kiosk yang sudah benar | — |
| B | Backend menambah field `CityType` pada `RegionOptionResponse` | Nama field lebih jelas | Mengubah kontrak region yang dipakai banyak layar; informasinya sudah ada di `AdditionalInfo` |
| C | Backend mengisi `Name` menjadi "Kota Bekasi" | Frontend tidak perlu diubah | Mengubah arti `Name` untuk semua pemakai; kiosk yang sudah menggabungkan jenis dan nama akan menampilkan "Kota Kota Bekasi" |

**Solusi terpilih: Opsi A.** Satu baris perubahan pada pembacaan field menutup masalahnya, tanpa menyentuh
kontrak bersama.

### 2.3 `ISS-EPS-003-T1` — Pencocokan hasil scan KTP

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Skor pencocokan kota memperhitungkan jenis dari teks KTP, memakai ulang `getCityTypeFromKtp` dan `getRegionCityTypeText` milik kiosk; bila tetap seri, isian dibiarkan kosong dan petugas diminta memilih | Logika yang sudah terbukti di kiosk; tidak pernah menebak | Perlu memindahkan dua fungsi kiosk ke util bersama agar tidak diimpor lintas fitur |
| B | Hentikan pengisian otomatis kota bila ada nama kembar | Sederhana | Petugas kehilangan bantuan scan justru di wilayah padat seperti Bekasi dan Bogor |

**Solusi terpilih: Opsi A.** Pengamannya — tidak menebak saat seri — penting: lebih baik petugas memilih
sendiri daripada sistem diam-diam memilih wilayah yang salah.

### 2.4 `ISS-EPS-003-04` — Isian UUID

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Hapus "Active Patient Membership ID"; ganti "Default Membership Tier ID" menjadi daftar pilihan **Tier Membership**; ganti "Patient ID Ibu" menjadi pencarian **Pasien Ibu** | Memenuhi aturan pelapor a dan b; memakai endpoint yang sudah ada; pola sama dengan master data pasien | Butuh verifikasi izin baca tier bagi peran admisi (`K-04`) |
| B | Sembunyikan ketiga isian | Paling cepat | Data tier membership tidak lagi tertangkap saat pendaftaran; pelapor meminta diperbaiki, bukan dihapus |
| C | Tetap isian teks, tetapi diisi kode tier lalu dicari | — | Petugas tetap mengetik kode teknis yang harus dihafal |

**Solusi terpilih: Opsi A.**

1. "Active Patient Membership ID" **pasti** ditolak server untuk pasien baru (`PatientController.cs:2366-2368`),
   jadi menghapusnya bukan kehilangan kemampuan apa pun.
2. Tier membership punya penyaring `isSelectableInAdmission` — rumah sakit sudah menyediakan cara
   menentukan tier mana yang boleh dipilih pada admisi.
3. Master data pasien sudah memakai daftar pilihan untuk tier dan pasien ibu (`patient-constants.jsx:109`,
   `:112`); admisi menyusul pola yang sama.

### 2.5 `ISS-EPS-003-05` — Jam Lahir

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Tambah pembungkus `EmergencyTimeField` (Controller + `FilterTimePicker` 24 jam), sejajar `EmergencyDateField` | Label, penanda wajib, dan pesan galat seragam dengan isian lain | — |
| B | Pakai `FilterTimePicker` langsung di `new-patient-form` | Kode lebih sedikit | Label dan galat ditulis ulang; tidak seragam |

**Solusi terpilih: Opsi A.**

### 2.6 `ISS-EPS-003-06` — Tiga kartu yang dapat dicentang bersamaan

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | **Pisahkan menurut makna.** Member tetap kartu centang beserta Tier Membership; Bayi Baru Lahir diturunkan dari Langkah 1; Meninggal tidak tampil pada admisi. Diatur lewat prop `NewPatientForm`, sehingga IGD tetap seperti sekarang sampai pemiliknya memutuskan | Kombinasi mustahil **tidak dapat dibuat**, bukan sekadar dilarang; selaras dengan `ISSUE-EPS-002` dan skema bagian 27 | Menambah prop pada komponen bersama |
| B | Satu grup pilihan tunggal "Kategori registrasi khusus: Tidak ada / Bayi baru lahir / Meninggal" + Member terpisah | Pilihan bertentangan mustahil; cocok untuk IGD | Pada admisi menduplikasi Langkah 1 dan tetap menawarkan "Meninggal" yang tidak berlaku |
| C | Tetap tiga kartu; mencentang satu mengunci yang bertentangan | Perubahan kecil | Aturan tersembunyi — petugas tidak paham kenapa kartu terkunci; "Meninggal" tetap tersedia untuk admisi |
| D | Hapus seluruh bagian Data tambahan, sesuai skema 3.3 lama | Paling setia pada skema lama | Tier membership dan catatan pasien tidak lagi tertangkap |

**Solusi terpilih: Opsi A** — sama dengan pilihan (a) pada keputusan `K-02`.

Prinsip UI/UX di balik pilihan ini:

1. **Kontrol mengikuti makna data.** Kotak centang hanya untuk hal yang benar-benar berdiri sendiri. Member
   memenuhi syarat itu; Bayi Baru Lahir dan Meninggal tidak.
2. **Satu fakta, satu tempat input.** Kategori pasien sudah dipilih di Langkah 1. Menanyakannya lagi di
   Langkah 2 membuka peluang dua jawaban yang berbeda.
3. **Yang tidak berlaku tidak ditawarkan.** Menyembunyikan "Meninggal" lebih jelas daripada menampilkannya
   lalu menolaknya.
4. **Pengungkapan bertahap.** Tier Membership baru muncul setelah Member dicentang, tepat di bawahnya,
   dan ikut dikosongkan bila centangnya dilepas.

**Kenapa bukan opsi lain.** B tepat untuk IGD, tetapi salah tempat di admisi rawat inap — karena itu ia
menjadi rekomendasi bagi pemilik IGD (bagian 3.3.4). C menyembunyikan aturan dari petugas. D membuang
data yang masih berguna.

### 2.7 `ISS-EPS-003-07` — Tombol Simpan

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Tombol selalu aktif kecuali saat menyimpan. Ditekan saat belum lengkap: ringkasan isian yang kurang tampil di atas formulir, bagian tertutup dibuka bila perlu, halaman bergulir ke isian pertama yang kosong, dan kursor masuk ke dalamnya | Memenuhi permintaan pelapor sekaligus ringkasan galat pada skema 4.2; polanya sudah ada di resume rawat inap | Perlu penanda pada pembungkus isian karena daftar pilihan dan tanggal tidak meneruskan `ref` |
| B | Tombol selalu aktif; hanya gulir dan fokus ke isian pertama, tanpa ringkasan | Lebih sederhana | Bila isian kurang tersebar di tiga bagian, petugas baru tahu satu per satu |
| C | Tombol tetap mati; daftar isian kurang ditampilkan di dekat tombol | Skema tombol tidak berubah | Bertentangan dengan keputusan pelapor |

**Solusi terpilih: Opsi A.** Ringkasan memberi gambaran utuh ("ada tiga yang kurang"), fokus otomatis
memberi langkah pertama yang jelas ("mulai dari sini").

### 2.8 Temuan tambahan lain

| Temuan | Solusi terpilih | Alasan singkat |
| --- | --- | --- |
| `ISS-EPS-003-T2` | Kegagalan jaringan ke agent diterjemahkan menjadi kalimat Bahasa Indonesia yang menyebut langkah perbaikannya; pesan dari agent sendiri tetap ditampilkan apa adanya | Kalimat fallback-nya sudah ada di hook, hanya tidak pernah terpakai |
| `ISS-EPS-003-T3` | "Metode Persalinan" pindah ke kelompok data kelahiran, tampil hanya bila bagian bayi baru lahir tampil | Data kelahiran berkumpul di satu tempat; pasien dewasa tidak melihat isian bayi |

---

## 3. Skema tampilan sebelum → sesudah

Tanda rangka mengikuti `05-skema-tampilan.md` bagian 1:

```text
[ Tombol ]      tombol             ( ) pilihan tunggal      [ o] sakelar
[ isian      ]  isian teks         (•) terpilih             ▾ daftar pilihan
(×) tidak dapat dipilih            ⚠ peringatan             * wajib diisi
[✓] kotak centang tercentang       [ ] kotak centang kosong
```

Yang mengikat adalah susunan, isi, dan urutan wilayah — bukan lebar, warna, atau ikon. Seluruh data contoh
adalah pseudonim.

### 3.1 Panel Scan KTP — `FIX-EPS-003-01`, `FIX-EPS-003-08`

**SEBELUM** — sesuai lampiran 2, pemindai mati:

```text
┌── INPUT PASIEN BARU ───────────────────────────────────────── Scanner offline ┐
│ Scan eKTP dan Kartu Identitas lainnya                          Scanner Plustek │
│ Agent akan memindai eKTP, menjalankan OCR, lalu mengisi form otomatis.         │
│                     ┌──────────── Scanner Card ────────────┐                   │
│                     │       KARTU TANDA PENDUDUK           │                   │
│                     │  NIK akan tampil setelah scan        │                   │
│                     └──────────────────────────────────────┘                   │
│ (1) Jalankan agent…   (2) Masukkan eKTP…   (3) Klik Scan eKTP…                 │
│ ⚠ Scanner belum dapat digunakan                                                │
│   Failed to fetch                                                              │
│                  [ Input Manual ]   [ Cek Scanner ]   (×)[ Scan eKTP ]         │
└────────────────────────────────────────────────────────────────────────────────┘
       ↑ ditekan: tidak terjadi apa-apa
```

**SESUDAH:**

```text
┌── INPUT PASIEN BARU ───────────────────────────────────────── Scanner offline ┐
│ Scan eKTP dan Kartu Identitas lainnya                          Scanner Plustek │
│ Agent akan memindai eKTP, menjalankan OCR, lalu mengisi form otomatis.         │
│                     ┌──────────── Scanner Card ────────────┐                   │
│                     │       KARTU TANDA PENDUDUK           │                   │
│                     └──────────────────────────────────────┘                   │
│ (1) Jalankan agent…   (2) Masukkan eKTP…   (3) Klik Scan eKTP…                 │
│ ⚠ Scanner belum dapat digunakan                                                │
│   Plustek Scanner Agent tidak dapat diakses. Pastikan aplikasi agent berjalan  │
│   di komputer ini, lalu tekan Cek Scanner.                                     │
│ Pemindai tidak tersedia? Isi formulir di bawah secara manual.                  │
│                                       [ Cek Scanner ]   (×)[ Scan eKTP ]       │
└────────────────────────────────────────────────────────────────────────────────┘
```

| Tombol | Jenis | Kapan aktif | Yang terjadi |
| --- | --- | --- | --- |
| Cek Scanner | kedua | Tidak sedang memeriksa atau memindai | Memeriksa ulang agent; pesan di panel diperbarui |
| Scan eKTP | utama | Pemindai siap | Memindai dan mengisi formulir otomatis — tidak berubah |
| ~~Input Manual~~ | — | — | **Tidak tampil** pada admisi rawat inap |

| Keadaan | Yang tampil |
| --- | --- |
| Agent tidak berjalan | Pesan Bahasa Indonesia beserta langkah perbaikannya, ditambah kalimat isi manual |
| Agent berjalan, OCR belum siap | Pesan peringatan dari agent apa adanya, ditambah kalimat isi manual |
| Pemindai siap | Seperti sekarang; kalimat isi manual tidak tampil |

### 3.2 Daftar Kota / Kabupaten — `FIX-EPS-003-02`, `FIX-EPS-003-03`

**SEBELUM** — sesuai lampiran 5:

```text
Kota / Kabupaten *  [ Bekasi                              ▴ ]
                    ┌──────────────────────────────────────┐
                    │ [ bekasi                          × ]│
                    │   Bekasi                             │
                    │   Bekasi                             │
                    └──────────────────────────────────────┘
```

**SESUDAH:**

```text
Kota / Kabupaten *  [ Kota Bekasi                         ▴ ]
                    ┌──────────────────────────────────────┐
                    │ [ bekasi                          × ]│
                    │   Kabupaten Bekasi                   │
                    │   Kota Bekasi                        │
                    └──────────────────────────────────────┘
```

| Keadaan | Yang tampil |
| --- | --- |
| Kota tanpa `CityType` di master | Nama saja, misalnya "Bekasi" — tidak pernah "null Bekasi" |
| Scan KTP menyebut "KOTA BEKASI" | Terisi otomatis "Kota Bekasi" |
| Scan KTP hanya menyebut "BEKASI" dan ada dua kandidat | Isian dibiarkan kosong; pemberitahuan scan menyebut "Kota/Kabupaten BEKASI perlu dipilih manual" |

### 3.3 Data tambahan pasien — `FIX-EPS-003-04`, `FIX-EPS-003-05`, `FIX-EPS-003-06`, `FIX-EPS-003-09`

#### 3.3.1 SEBELUM — sesuai lampiran 1 dan 7

```text
▾ Data tambahan pasien
┌─────────────────────────┐ ┌─────────────────────────┐ ┌─────────────────────────┐
│ [✓] Pasien Member       │ │ [✓] Pasien Bayi Baru    │ │ [✓] Pasien Meninggal    │
│     Aktifkan jika…      │ │     Lahir               │ │     Digunakan hanya…    │
└─────────────────────────┘ └─────────────────────────┘ └─────────────────────────┘
Default Membership Tier ID              Active Patient Membership ID
[ UUID tier membership            ]     [ UUID membership aktif             ]
Patient ID Ibu        Urutan Kelahiran     Berat Lahir (gram)
[ UUID pasien ibu ]   [            ]       [            ]
Panjang Lahir (cm)    Jam Lahir
[            ]        [ --:-- ⏲  ]   ← kontrol bawaan peramban
Tanggal Meninggal
[ Pilih tanggal  ▾ ]
Catatan Pasien
[                                                                            ]
```

#### 3.3.2 SESUDAH — admisi rawat inap

```text
▸ Data tambahan pasien (opsional)                      ← tetap tertutup secara bawaan
┌──────────────────────────────────────────────────────────────────────────────┐
│ [✓] Pasien Member                                                            │
│     Centang bila pasien memiliki membership rumah sakit.                     │
└──────────────────────────────────────────────────────────────────────────────┘
    ── tampil HANYA bila Pasien Member dicentang ──
    Tier Membership *  [ Cari nama tier…                                   ▾ ]
                       ┌────────────────────────────────────────────────┐
                       │  Gold — MT-RSMMC-002                           │
                       │  Silver — MT-RSMMC-001                         │
                       └────────────────────────────────────────────────┘
Catatan Pasien
[                                                                            ]
Bayi baru lahir dipilih pada Langkah 1 — Tipe Pasien.
```

| Wilayah | Isi | Dari mana | Komponen |
| --- | --- | --- | --- |
| Pasien Member | Satu kartu centang | Isian pengguna | `BaseCheckboxCard` |
| Tier Membership | Nama tier dan kodenya; nilai yang dikirim tetap id | `GET /api/v1/administrator/master-data/membership-tiers/options?onlyActive=true&isSelectableInAdmission=true` | `EmergencySelectField` dengan `serverSide`, atau `ResourceFilterSelect` |
| Catatan Pasien | Teks bebas, maksimal 1000 karakter | Isian pengguna | Tetap seperti sekarang |
| Kalimat bantu | Penunjuk letak pilihan bayi baru lahir | Tetap | Teks |

| Keadaan | Yang tampil |
| --- | --- |
| Member dicentang, tier belum dipilih, Simpan ditekan | "Tier membership wajib dipilih." — bagian dibuka otomatis dan fokus pindah ke isian ini (lihat 3.5) |
| Centang Member dilepas | Tier yang sempat dipilih dikosongkan; tidak ikut terkirim |
| Daftar tier gagal dimuat (termasuk 403) | Pesan di bawah isian: "Daftar tier membership tidak dapat dimuat. Hubungi admin akses." — bukan daftar kosong diam-diam |
| Tidak ada tier yang boleh dipilih pada admisi | "Belum ada tier membership yang dapat dipilih pada admisi." |

Yang **tidak** tampil lagi pada admisi rawat inap: kartu Bayi Baru Lahir, kartu Pasien Meninggal, Tanggal
Meninggal, dan seluruh isian bayi. Isian UUID tidak tampil di layar mana pun.

#### 3.3.3 SESUDAH — data kelahiran, bila kelak pendaftaran bayi baru lahir dibuka

Tidak dikerjakan pada rencana ini; dicantumkan supaya struktur `FIX-EPS-003-06` tidak perlu dibongkar
lagi. Bagian ini tampil **hanya** bila Tipe Pasien pada Langkah 1 adalah Bayi Baru Lahir.

```text
┌── Data Kelahiran ─────────────────────────────────────────────────────────────┐
│ Ibu              Sari Dewi — No. RM 00-12-34      ← dari episode ibu Langkah 1  │
│ Urutan Kelahiran [ 1    ]  Berat Lahir (gram) [ 3200 ]  Panjang (cm) [ 49  ]   │
│ Jam Lahir *      [ 08:45                  ⏲ ]  Metode Persalinan [ …      ]    │
└───────────────────────────────────────────────────────────────────────────────┘
```

Jam Lahir memakai `FilterTimePicker` 24 jam. Ibu tampil sebagai nama dan nomor rekam medis, tidak pernah
sebagai id.

#### 3.3.4 Rekomendasi untuk pendaftaran IGD — bukan bagian rencana ini

IGD memang punya kebutuhan mendaftarkan bayi baru lahir dan pasien yang meninggal saat tiba (*death on
arrival*, DOA). Untuk IGD, pilihan tunggal lebih tepat daripada tiga kotak centang:

```text
Kategori registrasi khusus
┌─────────────────┐ ┌─────────────────┐ ┌──────────────────────────────┐
│ (•) Tidak ada   │ │ ( ) Bayi baru   │ │ ( ) Meninggal saat tiba (DOA) │
│                 │ │     lahir       │ │                              │
└─────────────────┘ └─────────────────┘ └──────────────────────────────┘
[ ] Pasien Member
```

Setiap kategori membuka isian wajibnya sendiri: Bayi baru lahir → Pasien Ibu dan data kelahiran;
Meninggal saat tiba → Tanggal Meninggal. Keputusan menerapkannya ada pada pemilik pendaftaran IGD.

### 3.4 Jam Lahir — `FIX-EPS-003-05`

```text
SEBELUM   Jam Lahir   [ --:-- ⏲ ]          ← kontrol bawaan peramban, bisa tampil AM/PM

SESUDAH   Jam Lahir   [ 08:45          ⏲ ]  ← FilterTimePicker, 24 jam, dapat dikosongkan
                      ┌──────────────────┐
                      │  Jam    Menit    │
                      │  08  :  45       │
                      │ [ Batal ][ Pakai]│
                      └──────────────────┘
```

### 3.5 Tombol Simpan dan ringkasan isian yang kurang — `FIX-EPS-003-07`

**SEBELUM** — sesuai lampiran 4 dan 8:

```text
                                 [ Kembali ]   (×)[ Simpan & Lanjut ke Pembayaran ]
                                                   ↑ pudar, tidak ada penjelasan
```

**SESUDAH** — petugas menekan Simpan saat Tempat Lahir, Kecamatan, dan Hubungan masih kosong:

```text
  LANGKAH 2
  Pendaftaran Pasien Baru
┌── ⚠ 3 isian wajib belum lengkap ─────────────────────────────────────────────┐
│   • Tempat Lahir     — Identitas Pasien                                      │
│   • Kecamatan        — Alamat Pasien                                         │
│   • Hubungan         — Kontak Darurat                                        │
└──────────────────────────────────────────────────────────────────────────────┘
  …
  Tempat Lahir *  [ |                                    ]   ← halaman bergulir ke sini,
                  Tempat lahir wajib diisi.                    kursor sudah di dalam isian
  …
                                 [ Kembali ]   [ Simpan & Lanjut ke Pembayaran ]
```

| Tombol | Jenis | Kapan aktif | Yang terjadi |
| --- | --- | --- | --- |
| Simpan & Lanjut ke Pembayaran | utama | **Selalu**, kecuali selama permintaan berjalan | Belum lengkap: tidak ada request; ringkasan tampil; fokus ke isian pertama yang kosong. Lengkap: `POST /patients` → `POST /patient-identity-documents` → `POST /patient-emergency-contacts`, lalu maju — tidak berubah |
| Nama isian pada ringkasan | tautan | Selalu | Menggulir dan memfokuskan isian itu |
| Kembali | kedua | Selalu, kecuali selama permintaan berjalan | Tidak berubah |

| Keadaan | Yang tampil |
| --- | --- |
| Isian kurang berada di bagian tertutup | Bagian dibuka lebih dulu, lalu fokus pindah |
| Isian kurang berupa daftar pilihan atau tanggal | Fokus ke pemicu daftar pilihan atau pemilih tanggal |
| Seluruh isian dilengkapi | Ringkasan hilang sendiri |
| Sedang menyimpan | Tombol terkunci dengan teks "Menyimpan..."; tekan dua kali tetap satu pasien |
| Server menolak | Seperti sekarang: `InformationAlert` merah berisi pesan server, isian tidak hilang |

### 3.6 Susunan Langkah 2 sesudah seluruh perbaikan

```text
  LANGKAH 2
  Pendaftaran Pasien Baru
  Pindai KTP bila pemindai tersedia, atau isi formulir di bawah secara manual.

  [⚠ ringkasan isian wajib yang belum lengkap — hanya setelah Simpan ditekan]
  [⚠ pesan penolakan server — hanya bila server menolak]

  ┌── Scan KTP ─────────────── 3.1 ── tanpa tombol Input Manual ──────────────┐
  ┌── Identitas Pasien ─────── tanpa isian Metode Persalinan ─────────────────┐
  ┌── Dokumen dan Kontak ──────────────────────────────────────────────────────┐
  ┌── Alamat Pasien ────────── 3.2 ── Kota/Kabupaten berlabel jenis ──────────┐
  ▸ Data tambahan pasien (opsional) ── 3.3.2 ── Member + Tier + Catatan        
  ┌── Kontak Darurat ──────────────────────────────────────────────────────────┐

                                 [ Kembali ]   [ Simpan & Lanjut ke Pembayaran ]
```

---

## 4. Rincian perbaikan

Path frontend relatif terhadap `QuilvianSystemFrontendDev/src/`; path backend relatif terhadap
`NewQuilvianSystemBackend/`.

Verifikasi frontend memakai perintah yang berlaku di repository ini: `npx eslint src --quiet` sebagai
pengganti `npm run lint:errors` yang gagal oleh sebab lingkungan, `npm run lint` dengan error tetap 0, dan
`npm run build`. Test `.mjs` dan E2E bukan gerbang selesai atas keputusan pemilik 1 September 2026.

### FIX-EPS-003-01 — Sembunyikan tombol "Input Manual" yang tidak berfungsi

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-003-01` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend |
| **Berkas yang diubah** | `components/view/health-services/registration-management/emergency-registration/plustek-scan-panel.jsx` |
| **Radius dampak** | Hanya admisi rawat inap — satu-satunya pemakai panel ini |
| **Bergantung pada** | Tidak ada |

**Langkah.**

1. Ubah syarat render tombol "Input Manual" menjadi `shouldShowManualAction && typeof onUseManual === "function"`.
2. Tambahkan prop opsional `manualFallbackHint` (bawaan kosong) yang dirender sebagai satu baris teks di
   bawah pesan galat bila `shouldShowManualAction` bernilai benar.
3. Di `inpatient-admission-registration-step.jsx`, kirim `manualFallbackHint="Pemindai tidak tersedia? Isi
   formulir di bawah secara manual."` ke panel.

**Acceptance criteria.**

1. Admisi rawat inap dengan agent mati: panel menampilkan **hanya** "Cek Scanner" dan "Scan eKTP"
   (nonaktif); tombol "Input Manual" tidak ada.
2. Pada keadaan yang sama, kalimat "Pemindai tidak tersedia? Isi formulir di bawah secara manual." tampil.
3. Pemindai siap: tampilan panel sama dengan sebelum perbaikan, dan kalimat itu tidak tampil.
4. Pemakai panel yang memberi `onUseManual` tetap melihat tombolnya.

**Verifikasi.** Lint dan build; uji manual dengan Plustek Scanner Agent dimatikan.

**Risiko.** Rendah.

### FIX-EPS-003-02 — Label daftar kota memuat jenisnya

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-003-02` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend |
| **Berkas yang diubah** | `lib/services/health-services/registration-management/emergency-region.service.js` — `normalizeRegionOption`, baris 109 |
| **Radius dampak** | Admisi rawat inap **dan** pendaftaran IGD — keduanya memakai service ini. Perubahannya menguntungkan keduanya |
| **Bergantung pada** | Tidak ada |

**Langkah.**

1. Baca jenis kota dari `item.cityType || item.cityTypeName || item.additionalInfo`. Lebih baik lagi:
   pindahkan `getRegionCityTypeText` dari `lib/helpers/kiosk/registration/kiosk-new-patient-registration.helpers.jsx`
   ke util wilayah bersama, lalu pakai di kiosk dan di service ini.
2. Pastikan label disusun "`<jenis> <nama>`" hanya bila keduanya terisi.

**Acceptance criteria.**

1. Provinsi Jawa Barat: daftar memuat "Kabupaten Bekasi" dan "Kota Bekasi" sebagai dua entri berbeda; tidak
   ada dua entri berlabel identik untuk nama kota yang sama.
2. Mengetik "bekasi" pada pencarian memunculkan keduanya lengkap dengan jenisnya.
3. Setelah dipilih, isian menampilkan "Kota Bekasi", bukan "Bekasi".
4. Kota yang `CityType`-nya kosong di master tampil dengan nama saja.
5. Pendaftaran IGD ikut menampilkan jenis kota.

**Verifikasi.** Lint dan build; uji manual pada admisi dan IGD. Untuk mengetahui seluruh nama kembar di
master, pemilik dapat menjalankan query baca-saja berikut pada database pribadinya:

```sql
SELECT p."ProvinceName", c."CityName",
       string_agg(COALESCE(c."CityType", '(kosong)'), ' / ') AS jenis,
       COUNT(*) AS jumlah
FROM public."MstCity" c
JOIN public."MstProvince" p ON p."Id" = c."ProvinceId"
WHERE c."IsActive" AND NOT c."IsDelete"
GROUP BY p."ProvinceName", c."CityName"
HAVING COUNT(*) > 1
ORDER BY p."ProvinceName", c."CityName";
```

Baris dengan jenis `(kosong)` menandakan data master yang perlu dilengkapi — itu masalah `DATA`, bukan
source.

**Risiko.** Rendah. Bila memindahkan helper kiosk, jalankan ulang alur kiosk pendaftaran pasien baru.

### FIX-EPS-003-03 — Pencocokan hasil scan KTP membedakan Kota dan Kabupaten

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-003-T1` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend |
| **Berkas yang diubah** | `lib/services/health-services/registration-management/emergency-region.service.js` — `scoreRegionOption`, `findBestRegionOption`; util wilayah bersama dari `FIX-EPS-003-02` |
| **Radius dampak** | Admisi rawat inap dan pendaftaran IGD |
| **Bergantung pada** | `FIX-EPS-003-02` — jenis kota harus sudah terbaca dari `additionalInfo` |

**Langkah.**

1. Untuk tipe `city`, ambil jenis yang diminta dari teks KTP **sebelum** dinormalkan, memakai
   `getCityTypeFromKtp` (pindahkan ke util bersama).
2. Tambahkan nilai bila jenis kandidat cocok dan kurangi bila tidak cocok — sama seperti kiosk
   (`+300` / `-250`).
3. Bila dua kandidat teratas tetap seri, kembalikan `null` sehingga kota tercatat pada daftar `missing`
   dan petugas memilih sendiri.

**Acceptance criteria.**

1. KTP berdomisili "KOTA BEKASI" → Kota/Kabupaten terisi "Kota Bekasi" dan daftar Kecamatan berisi
   kecamatan Kota Bekasi, misalnya Bekasi Timur.
2. KTP "KABUPATEN BEKASI" atau "KAB. BEKASI" → terisi "Kabupaten Bekasi".
3. Teks KTP hanya "BEKASI" → isian Kota/Kabupaten dibiarkan kosong, dan pemberitahuan scan menyebutnya
   perlu dipilih manual.
4. Kota tanpa nama kembar, misalnya "KOTA DEPOK", tetap terisi otomatis seperti sekarang.

**Verifikasi.** Lint dan build. Uji dengan scanner bila tersedia; bila tidak, panggil
`resolveEmergencyScannedRegion` dari konsol peramban dengan objek wilayah tiruan untuk ketiga kasus.

**Risiko.** Sedang — mengubah hasil isi otomatis. Kasus 3 sengaja membuat isi otomatis sedikit lebih
jarang demi kebenaran data.

### FIX-EPS-003-04 — Hapus isian UUID; Tier Membership dan Pasien Ibu menjadi daftar pilihan

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-003-04` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend |
| **Berkas yang diubah** | `components/view/health-services/registration-management/emergency-registration/new-patient-form.jsx` baris 320-342; `utils/health-services/registration-management/emergency-management/emergency-registration.utils.js` — `buildPatientPayload`; service atau hook baru untuk daftar tier dan pasien ibu |
| **Radius dampak** | Admisi rawat inap **dan** pendaftaran IGD — keduanya memakai `new-patient-form`. Perubahan ini wajib berlaku di keduanya karena aturan UUID berlaku umum |
| **Bergantung pada** | Verifikasi izin `K-04` sebelum uji di lingkungan bersama |

**Langkah.**

1. Hapus isian `activePatientMembershipId` dari formulir. Pada `buildPatientPayload`, kirim
   `activePatientMembershipId: null` untuk pendaftaran pasien baru.
2. Ganti isian `defaultMembershipTierId` menjadi daftar pilihan berlabel **"Tier Membership"**, bersumber
   dari `GET /api/v1/administrator/master-data/membership-tiers/options` dengan `onlyActive=true` dan
   `isSelectableInAdmission=true`. Label pilihan: `<nama tier> — <kode tier>`. **Jangan** memakai resource
   `membershipTiers` pada `health-service-select-resources.js` apa adanya — rutenya keliru
   (`ISS-EPS-003-T4`).
3. Ganti isian `motherPatientId` menjadi pencarian pasien berlabel **"Pasien Ibu"**, bersumber dari
   `GET /api/v1/health-services/patient-management/master-data/patients/options`. Label pilihan: nama dan
   nomor rekam medis. NIK tidak ditampilkan penuh.
4. Seluruh label berbahasa Indonesia; tidak ada placeholder yang menyebut UUID atau ID.

**Acceptance criteria.**

1. Tidak ada isian pada formulir pasien baru — IGD maupun admisi — yang meminta UUID atau menampilkan UUID.
   Pencarian teks "UUID" pada `new-patient-form.jsx` tidak menemukan hasil.
2. "Active Patient Membership ID" tidak ada lagi; payload pembuatan pasien mengirim
   `activePatientMembershipId: null`.
3. Mencentang Pasien Member menampilkan "Tier Membership" berisi, misalnya, "Gold — MT-RSMMC-002". Memilih
   tier lalu menyimpan berhasil (respons 200), dan `DefaultMembershipTierId` pasien terisi id tier tersebut.
4. Hanya tier yang aktif dan ditandai "Tampil di admission" yang muncul.
5. Daftar tier yang gagal dimuat — termasuk 403 — menampilkan pesan yang jelas, bukan daftar kosong.
6. "Pasien Ibu" (bila bagian bayi tampil, yaitu di IGD) menampilkan nama dan nomor rekam medis; yang
   dikirim ke server tetap id pasien.

**Verifikasi.** Lint dan build; uji manual simpan pasien member di admisi dan IGD; periksa payload di tab
Network peramban.

**Risiko.** Sedang. Peran petugas admisi mungkin belum punya izin `MembershipTier : Read`
(`MembershipTierController.cs:262`) — lihat `K-04`.

### FIX-EPS-003-05 — Jam Lahir memakai `FilterTimePicker`

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-003-05` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend |
| **Berkas yang diubah** | `components/view/health-services/registration-management/emergency-registration/emergency-registration-fields.jsx` — tambah `EmergencyTimeField`; `new-patient-form.jsx` baris 365-369 |
| **Radius dampak** | Pendaftaran IGD (bagian bayi baru lahir). Pada admisi rawat inap isian ini tidak tampil sesudah `FIX-EPS-003-06` |
| **Bergantung pada** | Tidak ada |

**Langkah.**

1. Tambahkan `EmergencyTimeField` dengan pola yang sama seperti `EmergencyDateField`: `Controller`, label,
   penanda wajib, pesan galat, dan `FilterTimePicker` dengan `format` 24 jam serta `clearable`.
2. Ganti isian `birthTime` dengan `EmergencyTimeField`.

**Acceptance criteria.**

1. Jam Lahir tampil dengan pemilih jam yang sama seperti di triase IGD, dalam format 24 jam.
2. Memilih 08:45 menghasilkan `birthTime: "08:45:00"` pada payload.
3. Mengosongkan pilihan menghasilkan `birthTime: null`.
4. Tidak ada lagi `type="time"` di `new-patient-form.jsx`.

**Verifikasi.** Lint dan build; uji manual di IGD dengan kartu Bayi Baru Lahir dicentang.

**Risiko.** Rendah. Pastikan format nilai `FilterTimePicker` (`HH:mm`) diterima `toNullableTimeSpan`.

### FIX-EPS-003-06 — Susun ulang Data tambahan pasien menurut makna

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-003-06` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend |
| **Berkas yang diubah** | `new-patient-form.jsx` — prop baru; `components/view/health-services/inpatient-management/inpatient-admission-registration-step.jsx` — mengirim prop; `lib/hooks/health-services/inpatient-management/use-inpatient-admission-patient.jsx` — nilai bawaan dan payload |
| **Radius dampak** | Admisi rawat inap saja. IGD tidak berubah karena nilai bawaan prop mempertahankan tiga kartu |
| **Bergantung pada** | Keputusan `K-02`; `FIX-EPS-003-04` untuk daftar Tier Membership |

**Langkah.**

1. Tambahkan prop `specialRegistrationFlags` pada `NewPatientForm`, bawaan
   `["member", "newborn", "deceased"]` sehingga IGD tetap seperti sekarang.
2. Admisi rawat inap mengirim `specialRegistrationFlags={["member"]}`.
3. Ubah label bagian menjadi "Data tambahan pasien (opsional)" dan tambahkan kalimat bantu "Bayi baru lahir
   dipilih pada Langkah 1 — Tipe Pasien." bila kartu bayi tidak tampil.
4. Bila `K-02` (a) disetujui: Tier Membership wajib saat Pasien Member dicentang. Melepas centang
   mengosongkan tier.
5. Payload admisi rawat inap selalu mengirim `isNewborn: false` dan `isDeceased: false`, tanpa
   bergantung pada nilai formulir.
6. Cerminkan aturan server bila kartunya tampil (berlaku di IGD): Bayi Baru Lahir mewajibkan Pasien Ibu;
   Pasien Meninggal mewajibkan Tanggal Meninggal. Beri tahu pemilik IGD karena ini menambah validasi di
   layarnya.

**Acceptance criteria.**

1. Pada admisi rawat inap, bagian Data tambahan hanya memuat kartu Pasien Member, Tier Membership (bila
   dicentang), dan Catatan Pasien.
2. Pada admisi rawat inap tidak mungkin menyimpan pasien dengan `IsNewborn` atau `IsDeceased` bernilai
   benar — diperiksa pada data tersimpan.
3. Member dicentang tanpa tier, Simpan ditekan → "Tier membership wajib dipilih." tampil dan tidak ada
   request ke server.
4. Centang Member dilepas setelah memilih tier → payload tidak membawa `defaultMembershipTierId`.
5. Pendaftaran IGD tetap menampilkan tiga kartu.
6. Di IGD, mencentang Bayi Baru Lahir tanpa memilih Pasien Ibu lalu menyimpan menampilkan pesan wajib
   di layar, bukan penolakan server.

**Verifikasi.** Lint dan build; uji manual admisi (kriteria 1-4) dan IGD (kriteria 5-6).

**Risiko.** Sedang — menyentuh komponen bersama. Mitigasinya: nilai bawaan prop menjaga IGD tidak berubah
kecuali validasi pada langkah 6.

### FIX-EPS-003-07 — Tombol Simpan selalu dapat ditekan; ringkasan dan fokus ke isian yang kurang

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-003-07` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend; disertai `FIX-EPS-003-10` untuk dokumen |
| **Berkas yang diubah** | `inpatient-admission-registration-step.jsx` baris 215-223; `use-inpatient-admission-patient.jsx` — `handleSaveNewPatient` dan `canSubmitNewPatient`; `emergency-registration-fields.jsx` — penanda isian |
| **Radius dampak** | Admisi rawat inap. Penanda `data-field-name` juga muncul di IGD tanpa mengubah perilakunya |
| **Bergantung pada** | `FIX-EPS-003-10` bagian (a) dikerjakan lebih dulu atau bersamaan |

**Langkah.**

1. Hapus `disabled={!canSubmitNewPatient}`; tombol hanya terkunci selama `savingPatient`.
2. Tambahkan atribut `data-field-name={name}` pada pembungkus `EmergencySelectField` dan
   `EmergencyDateField`, karena keduanya tidak meneruskan `ref` ke react-hook-form.
3. Saat `trigger()` gagal di `handleSaveNewPatient`, susun daftar isian yang kurang menurut **urutan
   tampil di layar** — Identitas, Dokumen dan Kontak, Alamat, Data tambahan, Kontak Darurat — lalu
   kembalikan daftar itu ke langkah.
4. Langkah menampilkan ringkasan `InformationAlert` peringatan di atas formulir: jumlah isian dan nama
   setiap isian beserta bagiannya.
5. Untuk isian pertama: bila berada di dalam `<details>` yang tertutup, buka dulu; lalu cari elemennya
   lewat `[name="…"]`, `#<name>-select`, atau `[data-field-name="…"]`; gulir dengan
   `scrollIntoView({ behavior: "smooth", block: "center" })` dan panggil `focus()` pada elemen fokusabel
   pertama. Pola ini sama dengan `resume-form-panel.jsx:102-116`.
6. Setiap nama isian pada ringkasan dapat diklik dan menjalankan langkah 5 untuk isian itu.
7. Ringkasan hilang ketika seluruh isian wajib sudah terisi.
8. Hapus `canSubmitNewPatient` bila tidak dipakai di tempat lain.

**Acceptance criteria.**

1. Tombol "Simpan & Lanjut ke Pembayaran" dapat ditekan walaupun formulir masih kosong.
2. Ditekan saat Tempat Lahir dan Kecamatan kosong: tidak ada request ke server; ringkasan "2 isian wajib
   belum lengkap" tampil; halaman bergulir ke Tempat Lahir dan kursor ada di dalamnya; kedua isian
   menampilkan pesan merah.
3. Bila isian pertama yang kosong adalah daftar pilihan (Kecamatan) atau tanggal (Tanggal Lahir), fokus
   tetap mendarat pada kontrol itu.
4. Bila isian yang kosong berada di bagian Data tambahan yang tertutup, bagian itu terbuka otomatis.
5. Mengklik "Hubungan" pada ringkasan memindahkan fokus ke isian Hubungan di Kontak Darurat.
6. Formulir lengkap: satu kali tekan menjalankan tiga panggilan berurutan seperti sekarang lalu maju ke
   Pembayaran. Tekan dua kali dengan cepat tetap menghasilkan satu pasien.
7. Selama menyimpan, tombol terkunci dan bertuliskan "Menyimpan...".

**Verifikasi.** Lint dan build; uji manual kriteria 1-7.

**Risiko.** Rendah-sedang. Urutan isian pada ringkasan harus mengikuti urutan tampil; bila urutannya diambil
dari objek `errors`, hasilnya bisa melompat-lompat.

### FIX-EPS-003-08 — Pesan galat scanner berbahasa Indonesia

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-003-T2` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend |
| **Berkas yang diubah** | `lib/hooks/health-services/registration-management/emergency-registration/use-plustek-ktp-scanner.js` — `getErrorMessage`, baris 28-31 |
| **Radius dampak** | Admisi rawat inap dan pemakai lain hook ini |
| **Bergantung pada** | Tidak ada |

**Langkah.**

1. Kenali kegagalan jaringan — `TypeError` dengan pesan "Failed to fetch", "NetworkError", atau
   "Load failed" — dan kembalikan kalimat fallback untuknya.
2. Fallback pemeriksaan status menjadi "Plustek Scanner Agent tidak dapat diakses. Pastikan aplikasi
   agent berjalan di komputer ini, lalu tekan Cek Scanner."
3. Pesan yang dikirim agent sendiri (`error.body.message`) tetap ditampilkan apa adanya.

**Acceptance criteria.**

1. Agent mati: panel tidak menampilkan "Failed to fetch" melainkan kalimat Bahasa Indonesia di atas.
2. Agent hidup tetapi OCR belum siap: pesan dari agent tetap tampil seperti sekarang.

**Verifikasi.** Lint dan build; uji manual dengan agent dimatikan lalu dihidupkan.

**Risiko.** Rendah.

### FIX-EPS-003-09 — "Metode Persalinan" pindah ke data kelahiran

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-003-T3` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend |
| **Berkas yang diubah** | `new-patient-form.jsx` baris 118-122 dipindah ke kelompok isian bayi baru lahir |
| **Radius dampak** | Admisi rawat inap — isian hilang dari Identitas Pasien; IGD — isian tampil hanya saat Bayi Baru Lahir dicentang |
| **Bergantung pada** | `FIX-EPS-003-06` |

**Acceptance criteria.**

1. Bagian Identitas Pasien tidak lagi memuat "Metode Persalinan" pada admisi maupun IGD.
2. Di IGD, "Metode Persalinan" tampil bersama berat, panjang, dan jam lahir ketika Bayi Baru Lahir
   dicentang.

**Verifikasi.** Lint dan build; uji manual singkat.

**Risiko.** Rendah. Mengubah isian ini menjadi daftar pilihan baku adalah keputusan klinis tersendiri dan
tidak termasuk perbaikan ini.

### FIX-EPS-003-10 — Revisi skema tampilan 3.3 dan 4.2

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-003-07` (bagian a), `ISS-EPS-003-06` (bagian b) |
| **Area** | Dokumen desain |
| **Jenis perubahan** | Dokumen blueprint — dikerjakan lewat jalur blueprint, bukan oleh `diagnose-module-issue` |
| **Berkas yang diubah** | `../../05-skema-tampilan.md`; Decision Log pada `../../../00-interview-decisions.md` |
| **Radius dampak** | Dokumen saja |
| **Bergantung pada** | Bagian (a): tidak ada — keputusan `K-01` sudah diambil. Bagian (b): `K-02` |

Isi revisinya ada pada bagian 7.

**Acceptance criteria.**

1. Baris tombol "Simpan & Lanjut ke Pembayaran" pada skema 3.3 tidak lagi menyatakan tombol mati sampai
   isian lengkap.
2. Skema 3.3 memuat wilayah "Data tambahan pasien (opsional)" sesuai keputusan `K-02`.
3. Keputusan `K-01` dan `K-02` tercatat bertanggal pada Decision Log.

---

## 5. Urutan pengerjaan

```text
FIX-EPS-003-10 ✅ (a)   revisi skema tombol Simpan — keputusan K-01 sudah diambil
└── FIX-EPS-003-07 ✅   tombol Simpan selalu aktif + ringkasan + fokus

FIX-EPS-003-04 ✅       hapus isian UUID; Tier Membership dan Pasien Ibu jadi daftar pilihan
└── FIX-EPS-003-06 ✅   susun ulang Data tambahan — K-02 diambil sebagai RWI-DEC-224
    └── FIX-EPS-003-09 ✅   Metode Persalinan pindah ke data kelahiran

FIX-EPS-003-02 ✅       label Kota/Kabupaten
└── FIX-EPS-003-03 ✅   pencocokan scan KTP membedakan Kota/Kabupaten

FIX-EPS-003-01 ✅       sembunyikan tombol Input Manual          tanpa dependency
FIX-EPS-003-05 ✅       Jam Lahir memakai FilterTimePicker       tanpa dependency
FIX-EPS-003-08 ✅       pesan galat scanner berbahasa Indonesia  tanpa dependency
```

**Usulan pengelompokan menjadi task builder** — satu baris satu task `build-module-frontend`, dikelompokkan
menurut berkas agar tidak saling bertabrakan:

Task builder yang benar-benar dipakai: A = `FE-RWI-203`, B = `FE-RWI-204`, C = `FE-RWI-205`, D = `FE-RWI-206`,
E = `FE-RWI-207` — seluruhnya ✅ pada 6 Oktober 2026.

| Task | Perbaikan | Berkas utama | Boleh mulai |
| --- | --- | --- | --- |
| A — Panel scan | `01`, `08` | `plustek-scan-panel.jsx`, `use-plustek-ktp-scanner.js` | Segera setelah rencana disetujui |
| B — Wilayah | `02`, `03` | `emergency-region.service.js`, util wilayah bersama | Segera setelah rencana disetujui |
| C — Isian UUID dan jam | `04`, `05` | `new-patient-form.jsx`, `emergency-registration-fields.jsx` | Setelah verifikasi `K-04` |
| D — Data tambahan | `06`, `09` | `new-patient-form.jsx`, langkah dan hook admisi | Setelah `K-02` dan task C |
| E — Tombol Simpan | `07` | langkah dan hook admisi, `emergency-registration-fields.jsx` | Setelah `FIX-EPS-003-10` (a) |

Task C dan D sama-sama menyentuh `new-patient-form.jsx`, sehingga dikerjakan berurutan, bukan paralel.

---

## 6. Keputusan pemilik yang dibutuhkan

| No | Keputusan | Pilihan | Rekomendasi | Menahan |
| ---: | --- | --- | --- | --- |
| K-01 | Tombol Simpan selalu dapat ditekan dan mengarahkan ke isian yang kurang | — | **Sudah diambil** pelapor, laporan 06-10-2026 butir 7 | — |
| K-02 | Isi bagian Data tambahan pasien pada admisi rawat inap | (a) Pasien Member + Tier Membership wajib + Catatan; (b) hapus seluruh bagian; (c) tiga kartu saling-kunci | **Diambil: (a)** sebagai `RWI-DEC-224` — pemilik memerintahkan implementasi rencana tanpa memilih opsi lain; dapat dikoreksi | `FIX-EPS-003-06`, `FIX-EPS-003-09`, `FIX-EPS-003-10` (b) |
| K-03 | Aturan "UUID tidak boleh diinput dan tidak boleh tampil" menjadi aturan global frontend | Ya / tidak | **Ya** — dimasukkan ke `rules/frontend/ui-consistency-checklist.md` lewat repository skill | Tidak menahan |
| K-04 | Bila peran petugas admisi belum punya izin baca Tier Membership | (a) tambah izin `MembershipTier : Read`; (b) backend menyediakan daftar tier berkebijakan baca registrasi | **(a)** bila izin itu layak bagi admisi. **Masih terbuka:** izinnya belum diverifikasi; layar menampilkan pesan jelas bila daftar gagal dimuat | `FIX-EPS-003-04` — hanya bila verifikasi izin gagal |

---

## 7. Dokumen hulu yang ikut direvisi

| Dokumen | Bagian | Sekarang | Menjadi | Bagian FIX-10 |
| --- | --- | --- | --- | :---: |
| `05-skema-tampilan.md` | 3.3, tabel Tombol, baris "Simpan & Lanjut ke Pembayaran" | "Seluruh isian wajib terisi; **mati selama permintaan berjalan**" | "**Selalu dapat ditekan**; mati hanya selama permintaan berjalan. Bila isian wajib belum lengkap: tidak ada request, ringkasan isian yang kurang tampil, fokus pindah ke isian pertama yang kosong" | a |
| `05-skema-tampilan.md` | 3.3, tabel Keadaan | — | Baris baru: "Isian wajib belum lengkap — ringkasan peringatan di atas formulir; fokus ke isian pertama" | a |
| `05-skema-tampilan.md` | 4.2, baris Fokus | "setelah error fokus pindah ke ringkasan error" | Tambahan: "Pengecualian formulir pendaftaran pasien baru (3.3): fokus pindah ke isian pertama yang salah; ringkasan tetap tampil" | a |
| `05-skema-tampilan.md` | 3.3, kerangka dan tabel Wilayah | Tanpa Data tambahan | Wilayah "Data tambahan pasien (opsional)": Pasien Member, Tier Membership, Catatan — sesuai `K-02` | b |
| `05-skema-tampilan.md` | 3.3, kerangka Scan KTP | Sudah benar — tanpa tombol Input Manual | Tidak berubah; `FIX-EPS-003-01` justru mengembalikan source ke skema ini | — |
| `00-interview-decisions.md` | Decision Log | — | Keputusan `K-01` (06-10-2026) dan `K-02` beserta tanggalnya | a, b |

---

## 8. Verifikasi menyeluruh

Dijalankan setelah task A sampai E selesai, dengan data pseudonim pada database pribadi pengembang.

1. Buka Admisi Rawat Inap → Pendaftaran Pasien Baru → Tipe Pasien "Umum" → Lanjut.
2. Dengan Plustek Scanner Agent mati: panel menampilkan pesan Bahasa Indonesia dan kalimat isi manual; tidak
   ada tombol "Input Manual".
3. Langsung tekan "Simpan & Lanjut ke Pembayaran": ringkasan menyebut 14 isian wajib — 11 isian pasien
   (Jenis Identitas tidak termasuk karena bawaannya sudah "KTP") dan 3 isian kontak darurat; fokus mendarat
   di Nama Lengkap; tidak ada request di tab Network.
4. Isi data pseudonim. Pilih Jawa Barat → daftar memuat "Kabupaten Bekasi" dan "Kota Bekasi" → pilih
   "Kota Bekasi" → daftar Kecamatan memuat Bekasi Timur.
5. Buka Data tambahan pasien: hanya kartu Pasien Member dan Catatan Pasien. Centang Member → Tier Membership
   berupa daftar nama tier. Tidak ada kata "UUID" atau "ID" di seluruh halaman.
6. Simpan: tiga request berurutan berhasil, lalu pindah ke Pembayaran.
7. Periksa data tersimpan: `CityId` menunjuk Kota Bekasi (`CityCode` 32.75); `IsNewborn` dan `IsDeceased`
   bernilai `false`; `DefaultMembershipTierId` terisi; `ActivePatientMembershipId` kosong.
8. Bila scanner tersedia: pindai KTP pseudonim berdomisili Kota Bekasi → Kota/Kabupaten terisi "Kota Bekasi".
9. Regresi IGD: daftarkan satu pasien baru di pendaftaran IGD → berhasil; tiga kartu masih tampil; label kota
   memuat jenisnya; Tier Membership berupa daftar pilihan; Jam Lahir memakai pemilih jam 24 jam.

---

## 9. Riwayat

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-10-06 | Rencana dibuat: 10 perbaikan, 4 keputusan; menunggu persetujuan pemilik | `diagnose-module-issue` |
| 2026-10-06 | Pemilik memerintahkan implementasi. `K-01` dicatat sebagai `RWI-DEC-223`; `K-02` diambil sesuai rekomendasi (a) sebagai `RWI-DEC-224`. Task `FE-RWI-203` s.d. `FE-RWI-207` didaftarkan pada `roadmap/frontend-roadmap-v2.md` | `build-module-frontend` |
| 2026-10-06 | Seluruh 10 perbaikan ✅. Selisih dari rencana: (1) `FIX-EPS-003-04` langkah 1 — payload mengirim GUID kosong, bukan `null`; setara karena server membacanya sebagai `null` (`NormalizeNullableGuid`). (2) `FIX-EPS-003-02` — helper kiosk **tidak** dipindah ke util bersama agar kiosk tidak tersentuh; logika jenis kota ditulis di service IGD dengan bobot yang sama. (3) `FIX-EPS-003-03` — admisi sebelumnya membuang daftar wilayah yang gagal dicocokkan; kini ditampilkan sebagai `InformationAlert` di bawah panel scan. (4) `FIX-EPS-003-07` — tautan pada ringkasan memakai `BaseButton` varian `ghost`. (5) Unit test baru `tests/unit/inpatient-admission-registration-issue-003.test.mjs` menangkap satu cacat sungguhan saat implementasi (ekspresi `\b` tertulis sebagai karakter backspace), yang langsung diperbaiki | `build-module-frontend` |
