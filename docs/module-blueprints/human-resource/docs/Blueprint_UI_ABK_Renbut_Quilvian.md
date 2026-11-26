# BLUEPRINT MODUL ANALISA BEBAN KERJA (ABK) RUMAH SAKIT

**Versi:** 1.0  
**Tanggal:** 10 November 2026  
**Konteks:** Konsep UI dan proses bisnis mandiri untuk rumah sakit di Indonesia. Tidak menggunakan, membaca, atau memodifikasi GitHub, repository, backend, ataupun database Quilvian.  
**Sifat:** Blueprint target untuk pembahasan, bukan hasil audit implementasi maupun sertifikasi kepatuhan Renbut.

## 1. Tujuan dan keputusan desain

Modul **Analisa Beban Kerja (ABK)** membantu SDM memperkirakan kebutuhan tenaga berdasarkan aktivitas kerja, volume, norma waktu, Waktu Kerja Tersedia (WKT), dan tugas penunjang; kemudian membandingkannya dengan tenaga tersedia dan memantau hasil lintas departemen.

**Keputusan navigasi yang telah disepakati:**

1. Sidebar hanya mempunyai **satu menu: Analisa Beban Kerja**; bukan *Monitoring ABK*.
2. Halaman awal berbentuk **dashboard monitoring** (global dan per departemen), bukan tabel CRUD master.
3. Dua tombol pada header dashboard, **Riwayat Analisa** dan **Buat Analisa**, ditampilkan sejajar.
4. **Buat Analisa** membuka halaman/form tersendiri dengan empat bagian dalam satu halaman: **Identitas Analisa, Tugas Pokok, Tugas Penunjang, Hasil Kalkulasi**. Keempatnya bukan menu sidebar.
5. **Riwayat Analisa** membuka halaman tersendiri untuk mencari, melihat detail dan menelusuri perubahan status analisis; bukan menu sidebar.
6. Monitoring dapat difilter berdasarkan **Tahun → Departemen → Unit Kerja → Jabatan/Jenjang**. Jika Departemen = *Seluruh Departemen*, data semua departemen yang terverifikasi ditampilkan.
7. Seluruh angka dalam demo bersifat **simulasi**. Kalkulasi formal mengikuti pedoman dan kebijakan yang telah disahkan rumah sakit.

## 2. Ruang lingkup dan batas fungsi

| Di dalam ABK | Di luar ABK |
| --- | --- |
| Monitoring kebutuhan per posisi, unit dan departemen | Membuat/mengubah Master Departemen, Unit dan Jabatan |
| Input manual dan opsional penarikan volume operasional | Mengelola Master Pegawai atau Payroll |
| Perhitungan SBK, JKT, FTP, STP dan kebutuhan | Persetujuan anggaran dan pembukaan lowongan |
| Pencatatan sumber, verifikasi dan versi analisis | Rekrutmen, seleksi dan pengangkatan pegawai |
| Riwayat hasil dan referensi ke kebutuhan formasi | Memastikan cakupan shift 24 jam hanya dari volume tahunan |

### Landasan dan klasifikasi

- **PROVEN (inti perencanaan):** Permenkes 13/2025 Pasal 9, 10, 12, 16, 277 mendukung perencanaan kebutuhan SDMK, faktor kompetensi/kewenangan, norma waktu, volume kerja, waktu efektif, penetapan dan pelaporan. **Tidak mewajibkan layout UI tertentu.**
- **STANDARD_PRACTICE (referensi aplikasi):** Manual Renbut Rumah Sakit versi 5.0 (2024) menunjukkan tugas pokok, tugas penunjang, norma/capaian, tenaga eksisting, perhitungan, pembulatan, dan pengiriman usulan ke verifikator. Manual ini lebih tua daripada Permenkes 13/2025 dan harus dicek kesesuaiannya sebelum pemakaian formal.
- **RECOMMENDED (rancangan UI):** Dashboard dua level, filter departemen/unit, tombol sejajar, stepper satu halaman, versi immutable, pemisahan draft/terverifikasi, input volume otomatis atau manual.
- **Pengecualian:** PerBKN 9/2022 berlaku pada ASN; kuesioner, wawancara, observasi di sana menjadi referensi metode bagi RS swasta, bukan kewajiban langsung bagi seluruh RS swasta.

## 3. Hierarki navigasi dan rute konseptual

```text
HRD / Kepegawaian
└── Analisa Beban Kerja [satu-satunya menu sidebar]
    ├── Dashboard Monitoring (halaman awal)
    │   ├── Filter Tahun / Departemen / Unit / Jabatan
    │   ├── Ringkasan dan Grafik Kebutuhan vs Eksisting
    │   ├── [Riwayat Analisa] [Buat Analisa]
    │   └── Rincian Per Departemen / Unit / Jabatan
    ├── Buat Analisa (dibuka melalui tombol; bukan sidebar)
    │   ├── 1. Identitas Analisa
    │   ├── 2. Tugas Pokok
    │   ├── 3. Tugas Penunjang
    │   └── 4. Hasil Kalkulasi
    └── Riwayat Analisa (dibuka melalui tombol; bukan sidebar)
        ├── Daftar dan Filter Riwayat
        ├── Detail Snapshot Perhitungan
        └── Aksi Verifikasi / Revisi sesuai hak akses
```

Jika aplikasi kelak menggunakan URL, contoh rute logis: `/hrd/abk`, `/hrd/abk/buat`, `/hrd/abk/riwayat`, `/hrd/abk/riwayat/{id}`. **Ini hanya ilustrasi, bukan rute proyek asli.**

## 4. Blueprint layar A — Dashboard Monitoring

```text
┌────────────────────────────────────────────────────────────────────────────┐
│ HRD › Analisa Beban Kerja                                                  │
│ Monitoring kebutuhan SDM berdasarkan hasil analisa terverifikasi           │
│                                       [Riwayat Analisa] [+ Buat Analisa]   │
├────────────────────────────────────────────────────────────────────────────┤
│ Tahun [2026 ▼] Departemen [Semua ▼] Unit [Semua ▼] Jabatan [Semua ▼]       │
│ Cakupan: 18/25 posisi dianalisis                 Diperbarui: [tanggal]     │
├──────────────────┬─────────────────┬────────────────┬─────────────────────┤
│ Kebutuhan        │ Tersedia        │ Kekurangan     │ Kelebihan           │
│ 180 orang        │ 165 orang       │ 23 orang       │ 8 orang             │
├──────────────────────────────────────┬─────────────────────────────────────┤
│ Grafik kebutuhan vs tersedia per unit│ Persentase cakupan analisa          │
├──────────────────────────────────────┴─────────────────────────────────────┤
│ Rincian posisi: Departemen > Unit > Jabatan > Jenjang > Perlu > Ada > Gap  │
│ Buka Detail • Status • Versi • Tanggal Verifikasi                          │
└────────────────────────────────────────────────────────────────────────────┘
```

**Angka 180/165/23/8 hanya skenario UI**; total kekurangan dan kelebihan **tidak dinetokan antarkompetensi**.

### Komponen dashboard

| Komponen | Aturan |
| --- | --- |
| Filter Tahun | Menentukan tahun hasil analisis dan tanggal snapshot tenaga tersedia |
| Filter Departemen | *Seluruh Departemen* menampilkan semua departemen yang mempunyai hasil relevan; satu departemen membatasi unit dan posisi |
| Filter Unit | Hanya unit pada departemen terpilih; pada mode global dapat menampilkan semua unit |
| Filter Jabatan/Jenjang | Hanya posisi yang tercatat pada unit/struktur terpilih; satu jabatan dapat muncul di lebih dari satu unit |
| KPI Kebutuhan | Jumlah kebutuhan final hasil ABK terverifikasi yang termasuk cakupan filter |
| KPI Tenaga Tersedia | Snapshot tenaga tersedia untuk posisi yang sama dengan hasil valid; jangan bandingkan dengan total seluruh pegawai tanpa pembatas |
| KPI Kekurangan | Penjumlahan `max(kebutuhan - tersedia, 0)` **per posisi**, bukan selisih agregat umum |
| KPI Kelebihan | Penjumlahan `max(tersedia - kebutuhan, 0)` **per posisi** |
| KPI Cakupan | Posisi yang mempunyai analisis terverifikasi / posisi sasaran analisis; status belum dianalisis ditampilkan jelas |
| Grafik | Kebutuhan dan tenaga tersedia per departemen, unit atau jabatan setelah filter aktif |
| Empty state | Jika belum ada analisa valid, tampilkan **“Belum ada hasil ABK terverifikasi”**, bukan **0 kebutuhan** |

### Aturan agregasi penting

1. Identitas satu baris hasil: `(tahun/periode, departemen, unit, jabatan, jenjang, metode, versi berlaku)`.
2. Pilih hanya **satu versi** yang berstatus **terverifikasi dan berlaku** untuk tiap kombinasi; versi draft atau lama tidak ikut angka resmi.
3. Penugasan rangkap, paruh waktu, mutasi dan pegawai kontrak harus mempunyai kebijakan hitung **orang vs FTE** agar tidak terhitung ganda.
4. Hasil global yang belum mencakup seluruh posisi wajib diberi label **parsial**; kebutuhan belum dianalisis tidak boleh disimpulkan **0**.

## 5. Blueprint layar B — Buat Analisa

**Pola UI:** satu halaman wizard/stepper horizontal **Identitas → Tugas Pokok → Tugas Penunjang → Hasil Kalkulasi**. Sidebar tidak bertambah. Aksi tetap: **Simpan Draft, Kembali, Lanjut, Hitung, Ajukan Verifikasi** sesuai tahap.

### 5.1 Identitas Analisa

| Field | Tipe | Sumber | Aturan |
| --- | --- | --- | --- |
| ID Analisa | Otomatis | Sistem | ID unik, tidak diedit |
| Tahun / periode volume | Pilih | Pengguna | Tahun dan rentang waktu dinyatakan |
| Departemen | Dropdown | Master Organisasi | Read-only; tidak tersedia tombol tambah master |
| Unit Kerja | Dropdown bertingkat | Master Unit | Hanya unit di departemen terpilih |
| Jabatan / profesi | Dropdown | Master Jabatan | Sesuai unit/relasi unit-jabatan |
| Jenis / jenjang jabatan | Dropdown bersyarat | Master Jabatan | Ditampilkan bila relevan untuk metode |
| Metode | Pilih | Konfigurasi SDM | ABK-Kes; standar minimal bukan hasil ABK yang sama |
| WKT | Numerik menit/tahun | Konfigurasi WKT | Harus > 0 dengan sumber dan periode |
| Tenaga eksisting | Read-only + verifikasi | Kepegawaian/penempatan | Snapshot tanggal dan satuan orang/FTE |
| Alasan/tujuan analisa | Teks | Pengguna | Perencanaan tahunan, pengembangan, evaluasi perubahan volume |

### 5.2 Tugas Pokok

| Field per baris aktivitas | Perilaku |
| --- | --- |
| Uraian aktivitas | Pilih dari uraian jabatan resmi; aktivitas baru menjadi usulan hingga validasi |
| Masukkan ke perhitungan? | Pilih hanya aktivitas relevan dan tidak ganda |
| Satuan output | Mis. dokumen, resep, pemeriksaan; harus sesuai volume |
| Sumber volume | Otomatis HIS / laporan unit / wawancara / kuesioner / observasi / estimasi tervalidasi |
| Volume dan periode | Input manual atau rekap; simpan periode asli dan volume setara tahunan |
| Norma waktu (menit/aktivitas) | Pakai standar terkunci untuk jabatan tertentu; manual hanya bila diizinkan dan terbukti |
| Dasar norma | SOP, standar resmi, pengukuran atau justifikasi yang diverifikasi |
| Bukti | Referensi laporan/dokumen, tanggal pengamatan, penanggung jawab |
| Nilai kalkulatif | Menit aktivitas, SBK dan kontribusi kebutuhan; read-only hasil sistem |

Sumber volume **otomatis tidak menjadikan norma waktu otomatis**. Untuk pekerjaan administratif/nonklinis, data manual melalui observasi/kuesioner/wawancara tetap dapat digunakan dengan validasi.

### 5.3 Tugas Penunjang

Input daftar tugas penunjang, frekuensi per tahun, durasi per kejadian, total menit per orang per tahun, dan bukti. Contoh: pelatihan, rapat internal, supervisi atau kepanitiaan **jika benar-benar bagian dari tugas penunjang metode yang berlaku**.

**Anti double-count:** jika hari pelatihan sudah dikurangkan saat menetapkan WKT, jangan kembali membebankan waktu yang sama melalui tugas penunjang tanpa definisi metode yang sah.

### 5.4 Hasil Kalkulasi

Sistem menampilkan tabel: **Aktivitas | Volume/tahun | Norma menit | WKT | SBK | Kontribusi Tenaga**, lalu kartu hasil **JKT, FTP, STP, Kebutuhan Desimal, Kebutuhan Bulat, Tenaga Eksisting, Gap** beserta pesan validasi.

**Rumus kalkulasi dalam kondisi data konsisten:**

- `SBK_i = WKT / NormaWaktu_i`
- `KontribusiTenaga_i = VolumeTahunan_i / SBK_i`
- `JKT = Σ KontribusiTenaga_i`
- `MenitPenunjang = Σ (Frekuensi_i × Durasi_i)` per orang per tahun
- `FTP(%) = MenitPenunjang / WKT × 100`
- `STP = 1 / (1 - FTP/100)` untuk `0 <= FTP < 100`
- `KebutuhanSetaraTenagaPenuh = JKT × STP`
- `KebutuhanOrang = dibulatkan mengikuti kebijakan/pedoman yang telah ditetapkan`
- `Kekurangan = max(KebutuhanOrang - Eksisting, 0)` per unit/jabatan/jenjang
- `Kelebihan = max(Eksisting - KebutuhanOrang, 0)` per unit/jabatan/jenjang

**Ilustrasi semata:** WKT 75.000 menit/tahun; tiga tugas pokok dengan kontribusi 1,20 + 3,84 + 1,60 = JKT 6,64. Menit penunjang 2.250 → FTP 3% → STP 1,0309278 → kebutuhan 6,84536 setara tenaga penuh. Dengan contoh pembulatan ke atas, menjadi 7 orang; jika eksisting 5 maka kekurangan indikatif 2 orang. **Pembulatan dan kecukupan shift harus divalidasi sebelum dipakai resmi.**

## 6. Blueprint layar C — Riwayat Analisa

**Akses:** tombol **Riwayat Analisa** sejajar **Buat Analisa** pada dashboard. **Halaman terpisah, bukan sidebar.**

| Elemen | Cakupan |
| --- | --- |
| Filter | Tahun, departemen, unit, jabatan, status, pembuat |
| Kolom daftar | Nomor Analisa, Tahun, Unit, Jabatan/Jenjang, Kebutuhan, Eksisting, Selisih, Status, Versi, Tanggal |
| Detail analisa | Semua parameter dan aktivitas pada versi tersebut, sumber bukti, JKT/FTP/STP, catatan verifikasi |
| Buka Detail | Hak baca sesuai jabatan dan unit |
| Lanjutkan Draft | Hanya draft yang sah dan dapat diedit oleh pemilik/role |
| Ajukan Verifikasi | Validasi wajib lengkap sebelum status berubah |
| Kembalikan/Revisi | Alasan wajib dicatat dan riwayat tersimpan |
| Terverifikasi | Snapshot terkunci; revisi menghasilkan versi baru |
| Hapus | Hanya untuk draft belum diajukan sesuai kebijakan; hasil terverifikasi tidak boleh dihapus permanen melalui UI biasa |

### Status dan keputusan

**Diagram alur:** Analisa dimulai dari **Draft**, dapat **Diajukan** dan **Diperiksa**, lalu **Terverifikasi**; jika ada koreksi, dikembalikan ke **Draft**, dan revisi membuat versi baru.

**Terverifikasi bukan berarti rekrutmen disetujui.** Pengajuan kebutuhan pegawai dan persetujuan anggaran berada pada workflow terpisah.

## 7. Proses bisnis end-to-end

**Diagram alur:**

```text
Dashboard ABK
    → Buat Analisa
    → Identitas
    → Tugas Pokok
    → Tugas Penunjang
    → Hasil Kalkulasi
    → Validasi
    → Verifikasi
    → Dashboard/Riwayat
```

## 8. Matriks hak akses yang direkomendasikan

| Aksi | Staf SDM | Kepala Unit | Kepala SDM / Verifikator | Direksi |
| --- | --- | --- | --- | --- |
| Baca Dashboard global | Sesuai hak | Sesuai unit | Ya | Ya |
| Buat dan edit draft | Ya | Usul/isi bila diizinkan | Ya | Tidak perlu |
| Melihat master organisasi | Ya | Ya | Ya | Ya |
| Mengubah master organisasi dari ABK | Tidak | Tidak | Tidak | Tidak |
| Memvalidasi data aktivitas unit | Memeriksa | Ya | Memeriksa | Tidak perlu |
| Mengajukan hasil | Ya | Bergantung kebijakan | Ya | Tidak perlu |
| Verifikasi/penetapan hasil | Tidak secara mandiri | Validasi unit | Ya, sesuai kewenangan | Jika ditetapkan kebijakan |
| Buka Riwayat | Sesuai otorisasi | Sesuai unit | Ya | Ya |
| Menyetujui rekrutmen dari layar ABK | Tidak | Tidak | Tidak | Tidak |

## 9. Entitas data konseptual — hanya usulan, bukan struktur basis data proyek

| Entitas konseptual | Field minimum | Relasi / aturan |
| --- | --- | --- |
| `ABK_Analyses` | AnalysisID, Year, Period, DepartmentId, UnitId, PositionId, GradeId, Method, Status, Version, CreatedBy | Satu record per versi analisis |
| `ABK_WorkTime` | AnalysisID, WKTMinutes, Basis, Period, StaffSnapshotAt | Konteks WKT harus dapat diaudit |
| `ABK_CoreActivities` | ActivityId, AnalysisID, TaskRef, UnitOutput, Volume, Period, NormMinutes, SourceType, EvidenceId, LockedNorm | Baris tugas pokok dengan sumber volume/norma |
| `ABK_SupportActivities` | ActivityId, AnalysisID, TaskName, Frequency, MinutesPerEvent, MinutesPerYear | Jangan dihitung ganda sebagai tugas pokok/pengurang WKT |
| `ABK_CalculationSnapshots` | AnalysisID, JKT, FTP, STP, DecimalNeed, RoundedNeed, Existing, Deficit, Surplus, FormulaVersion | Hasil perhitungan pada versi tertentu |
| `ABK_Reviews` | ReviewId, AnalysisID, Actor, Decision, Reason, Timestamp | Jejak pemeriksaan/perubahan status |
| `ABK_Evidence` | EvidenceId, FileRef, Type, Source, Date, Owner | Bukti untuk input manual/standar |
| `ABK_AuditTrail` | Entity, Action, Before/AfterRef, Actor, Timestamp | Catatan perubahan dan revisi |
| Master Organisasi / Kepegawaian | DepartmentId, UnitId, PositionId, EmployeeAssignmentId | Dibaca dari sumber luar ABK, tidak dibuat di sini |

**Relasi konseptual:** `Master Department → Units → UnitPositions → ABK_Analyses → (CoreActivities, SupportActivities, WorkTime) → CalculationSnapshots dan Reviews → Dashboard/Riwayat`. Semua hasil final memakai **snapshot historis**; jangan hanya merujuk nama master terkini yang bisa berubah.

## 10. Aturan validasi & keamanan perhitungan

1. **Input angka:** WKT > 0; norma aktivitas > 0; volume >= 0; tugas pokok yang dihitung mempunyai volume/sumber yang valid; menit penunjang >= 0.
2. **Satuan:** seluruh rumus memakai menit per orang per tahun dan output per aktivitas yang seragam. Contoh 10 menit/dokumen × 100 dokumen; jangan mencampur pasien dengan pemeriksaan.
3. **Sumber:** catat tahun, laporan/URL internal, pemilik data dan tanggal pengambilan. Data wawancara atau estimasi harus ditandai sesuai metodologi.
4. **Locked norm:** bila jabatan memiliki norma baku yang terkunci, form tidak mengizinkan operator mengubah tanpa otorisasi terpisah.
5. **Dedup:** satu kejadian tidak dihitung dua kali sebagai pekerjaan berbeda tanpa pemisahan aktivitas terukur.
6. **Tugas penunjang:** jika FTP >= 100%, blokir kalkulasi. Cegah duplikasi terhadap pengurang WKT.
7. **Perubahan input:** semua hasil kalkulasi harus diperbarui saat volume/norma/WKT berubah; nilai lama tidak tetap dianggap valid.
8. **Data belum lengkap:** gunakan **Belum Dihitung**, bukan otomatis **0**.
9. **Persetujuan:** hanya snapshot terverifikasi masuk monitoring resmi. Aturan jabatan yang memverifikasi harus dapat dikonfigurasi.
10. **Kerahasiaan:** gunakan data agregat pegawai untuk dashboard, bukan menampilkan informasi pribadi pegawai tanpa kebutuhan dan hak akses.
11. **Kebutuhan layanan khusus:** validasi kecukupan petugas per shift, kewenangan klinis, kesiagaan, dan standar minimal di luar rumus rata-rata tahunan.
12. **Penggantian formasi:** hasil ABK bersifat dasar evaluasi; keputusan rekrutmen memerlukan proses terpisah.

## 11. Kriteria penerimaan (UAT) minimum

| ID | Skenario | Hasil yang diharapkan |
| --- | --- | --- |
| UAT-01 | Buka menu sidebar | Hanya menu **Analisa Beban Kerja**; langsung ke dashboard |
| UAT-02 | Klik Buat Analisa | Halaman empat tahap terbuka, tanpa sidebar sub-menu |
| UAT-03 | Klik Riwayat Analisa | Halaman riwayat terbuka, tanpa menu sidebar baru |
| UAT-04 | Ubah Departemen | Unit dan jabatan berubah sesuai departemen; filter invalid ter-reset |
| UAT-05 | Pilih Seluruh Departemen | Dashboard kembali memuat seluruh hasil valid sesuai periode |
| UAT-06 | Simpan draft baru | Draft muncul di riwayat; tidak menambah angka dashboard resmi |
| UAT-07 | Volume 0, norma 0, WKT 0 | Kalkulasi terblokir atau menghasilkan pesan validasi yang tepat |
| UAT-08 | Input volume/norma manual berbukti | Sistem menyimpan metode, periode, satuan dan sumber data |
| UAT-09 | FTP >= 100% | Tidak ada pembagian nol; hasil STP tidak diterbitkan |
| UAT-10 | Verifikasi berhasil | Hasil baru muncul pada monitoring dan dapat ditelusuri |
| UAT-11 | Ubah analisis final | Sistem membuat revisi/versi; hasil final lama tidak ditimpa |
| UAT-12 | Jabatan A surplus, Jabatan B defisit | Kekurangan B tetap ditampilkan dan tidak otomatis dikurangi oleh surplus A |
| UAT-13 | Ada posisi belum dianalisis | Dashboard menampilkan cakupan parsial, bukan kebutuhan 0 |
| UAT-14 | Karyawan memiliki dua penugasan | Perhitungan eksisting sesuai aturan FTE/penempatan; tidak double count |
| UAT-15 | Form keterbatasan shift | Sistem memberi peringatan review kebutuhan shift untuk layanan 24 jam |

## 12. Tahapan implementasi yang disarankan

| Tahap | Cakupan | Keterangan |
| --- | --- | --- |
| Tahap 1 — Prototype Standalone | Dashboard, filter contoh, Buat Analisa 4 bagian, input manual, perhitungan dan riwayat lokal | Cocok demo dan validasi kebutuhan; data simulasi |
| Tahap 2 — Operasional Tervalidasi | Hak akses, pengajuan verifikasi, snapshot immutable, sumber norma/volume, monitoring resmi | Membutuhkan keputusan SDM/organisasi tentang metode dan otorisasi |
| Tahap 3 — Integrasi (opsional) | Baca master organisasi/pegawai dan tarik volume dari layanan yang tercatat | Tidak boleh dianggap telah terimplementasi; perlu mapping aktivitas dan mutu data |

### Keputusan rumah sakit yang masih terbuka

1. Standar WKT dan kebijakan pembulatan untuk masing-masing kelompok jabatan.
2. Jabatan yang norma waktunya terkunci dan sumber rujukan resmi yang digunakan.
3. Susunan verifikator hasil dan kemungkinan delegasi persetujuan.
4. Kebijakan hitung tenaga rangkap tugas, part-time, dan jenjang kompetensi.
5. Persyaratan cakupan seluruh unit agar ringkasan rumah sakit dianggap lengkap.
6. Metode penanganan unit 24 jam dan standar minimal yang wajib diprioritaskan.

## 13. Sumber resmi

1. **Permenkes 13/2025 (berlaku)** — Kementerian Kesehatan: https://jdih.kemkes.go.id/documents/peraturan-menteri-kesehatan-nomor-13-tahun-2025
2. **Manual Renbut Rumah Sakit versi 5.0 (2024; referensi operasional historis)** — Kemenkes: https://renbut.kemkes.go.id/unduh/manual_renbut_rumahsakit_2024.pdf
3. **Peraturan BKN 9/2022 (berlaku; konteks ASN)** — JDIH BKN: https://jdih.bkn.go.id/Home/cari_data?keyword=kebutuh
4. **Sosialisasi Permenkes 13/2025 (2025)** — Ditjen SDM Kesehatan Kemenkes: https://ditjen-sdmk.kemkes.go.id/pos/berita/sosialisasi-permenkes-nomor-13-tahun-2025-substansi-perencanaan-sdm-kesehatan-b6b76248

---

**Kesimpulan:** ABK adalah satu menu dashboard berbasis kalkulasi dan monitoring, dengan halaman **Buat Analisa** dan **Riwayat Analisa** yang dibuka lewat tombol; penginputan aktivitas dapat manual atau mengambil data sumber, norma waktu mengikuti otorisasi, dan hasil final baru masuk monitoring setelah verifikasi. Tidak diperlukan GitHub maupun perubahan proyek asli untuk menggunakan blueprint ini sebagai acuan diskusi dan pengembangan.

*Rancangan mandiri • 10 November 2026*
