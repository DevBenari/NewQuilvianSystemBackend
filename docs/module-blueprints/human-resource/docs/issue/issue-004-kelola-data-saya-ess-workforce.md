# ISSUE-004 — Perbaikan Alur Usulan Mandiri, Validasi Backend 400, Tab Informasi Pribadi, dan Kerapihan Tampilan ESS Kelola Data Saya

```yaml
issue_id: ISSUE-HRD-004
module_id: human-resource
submodule: S-A5 (Employee Self-Service) & S-A1 (Administrasi Kepegawaian)
layar: "Layanan Mandiri Pegawai — Kelola Data Saya (/self-services/human-resource/employee/profile-changes)"
sumber_laporan: "Laporan pengguna 11-11-2026: 3 butir keluhan baru, 2 keluhan tampilan awal, 4 tangkapan layar"
tanggal_issue: "2026-11-11"
status: TERBUKA
keparahan_tertinggi: Blocker
source_sha_backend: "f751f8ca (MHamzah)"
source_sha_frontend: "d09dc74da (HamzahV2)"
rencana_perbaikan: ../plan-repair/plan-repair-004-kelola-data-saya-ess-workforce.md
```

---

## 1. Ringkasan

Pemeriksaan ini menindaklanjuti keluhan pengguna pada layar **Layanan Mandiri Pegawai (ESS) — Kelola Data Saya** (`/self-services/human-resource/employee/profile-changes`) yang terhubung dengan modul backend Administrasi Kepegawaian (`WorkforceCore`).

Terdapat **5 butir permasalahan**:
1. **HTTP 400 Bad Request saat Pengajuan Alamat & Rekening Bank**: Pengiriman usulan data baru ditolak oleh validasi ASP.NET Core karena payload frontend salah memetakan nama properti (`targetEntity` alih-alih `TargetEntityName`) serta mengosongkan `FieldGroup`. Selain itu, modal tinjauan menampilkan string mentah JSON (`addressType: Identity, addressLine: test...`) yang tidak semestinya dipajang kepada pengguna.
2. **Kebutuhan Menu/Tab Mandiri "Informasi Pribadi"**: Tombol "Mode Ubah Data Pribadi" pada hero saat ini membuka modal pop-up raksasa yang tidak praktis. Pengguna meminta modal pop-up tersebut ditiadakan dan digantikan oleh menu/tab kanonikal baru **"Informasi Pribadi"** di panel sisi kiri, yang menampilkan data profil pribadi dalam mode baca serta menyediakan aksi ubah data yang wajib melalui alur approval HRD.
3. **Ketiadaan Aksi Tambah pada Sub-Menu Pendukung (seperti Kontak Darurat) & Ketegasan Batas Hak Akses**: Beberapa menu mandiri seperti Kontak Darurat tidak memunculkan tombol tambah data karena ketiadaan inisialisasi state resource mandiri. Diperlukan audit klasifikasi tegas antara data yang boleh diubah mandiri via approval HRD (Alamat, Rekening Bank, Kontak Darurat, Keluarga, Pendidikan) versus data yang mutlak berstatus *read-only* (Penempatan Organisasi, Jabatan, Gaji, Kontrak, Kredensial Medis).
4. **Teks Informasi Buram / Tertutup Footer Fixed**: Bilah footer melayang dengan efek frosted glass (`backdrop-filter: blur(18px); z-index: 99`) menimpa baris bawah kartu Ringkasan Profil dan kartu Kepatuhan Dokumen karena kontainer halaman belum memiliki jarak aman (*bottom safe space*), diperparah oleh ukuran label teks mikro (`9px`).
5. **Data Kepegawaian Hilang di ESS Dibandingkan Master Data**: Informasi penting seperti Departemen Utama, Posisi Utama, Grade Level, Tanggal Bergabung, dan Kontrak Berakhir belum dikirim oleh endpoint ESS backend (`EmployeeCurrentProfileResponse`) dan terbuang oleh logika dashboard karena nama field yang tidak sinkron.

Masalah paling kritis dan berbahaya berstatus **Blocker** adalah kegagalan transaksi HTTP 400 (Butir 1) karena membuat alur pengajuan perubahan data mandiri karyawan gagal total di server.

---

## 2. Laporan Asli Pelapor

| No | Keluhan Asli Pelapor | Lampiran Terkait |
| :---: | :--- | :--- |
| **1** | *"Ketika menambahkan alamat baru tersebut , terjadi eror seperti ini: Request URL: https://localhost:7184/api/v1/self-services/human-resource/profile-changes, POST 400 Bad Request: Details[0].FieldGroup is required, Details[0].TargetEntityName is required. Masalah dari segi tampilan tidak perlu menampilkan kolom nilai baru tersebut , dan pada nilai baru itu juga yang di kirim json backend tidak di perlukan, untuk eror terkait ini tidak hanya di alamat saja di rekening bank juga."* | Tangkapan layar modal tinjauan pengajuan bertanda galat HTTP 400 (`media_1794373252033_ebe6637c.jpg`) |
| **2** | *"pada saat klik mode ubah data pribadi tersebut , seharus ya bukan menampilkan modal , tetapi jadi tidak perlu itu di hilangkan saja, saya ingin anda menambahkan menu baru yaitu informasi pribadi , lalu pada informasi pribadi tersebut menampilkan informasi dia , lalu bisa di klik edit jika sudah ada , nantik Ketika melakukan edit maka dia wajib melewati approval tersebut."* | Tangkapan layar modal form Ubah Data Pribadi (`media_1794373257474_72c37458.jpg`) |
| **3** | *"Pada menu yang lain belum bisa di tambahkan tidak ada icon tambah tersebut seperti kontak darurat , jika memang berlaku data sensitive seperti penampatan organisasi , itu just read only , coba anda analisis Kembali mana yang readonly mana yang bisa di tambah dan edit melewati approval dari HRD tersebut."* | Tangkapan layar menu samping ESS (`media_1794372319160_82fc0d3d.jpg`) |
| **4** | *"terlihat pada informasi ya kok tidak jelas ya tidak terllihat ?"* | Tangkapan layar halaman ESS tertimpa footer (`media_1794372319160_82fc0d3d.jpg`) |
| **5** | *"pada capture yang saya berikan pada gambar ke 2 adalah dari menu master data workforce karyawan , ada sebagian yang bisa di tampilkan, lalu saya ingin anda merapihkan itu juga pada menu workforce tampilan ya masih banyak yang hilang pada informasi tersebut."* | Tangkapan layar Master Data Karyawan (`media_1794372438715_0a51a029.jpg`) |

---

## 3. Ringkasan Temuan

| ID Temuan | No. Lap | Judul Temuan | Jenis | Area | Keparahan | Status Bukti | ID Perbaikan |
| :--- | :---: | :--- | :--- | :---: | :---: | :---: | :--- |
| **ISS-HRD-004-01** | 1 | Penolakan HTTP 400 Bad Request saat Submit Alamat & Rekening Bank Mandiri | `BUG`, `CONTRACT_MISMATCH` | BE + FE | **Blocker** | SUDAH-VERIFIKASI di source & capture | `FIX-HRD-004-01` |
| **ISS-HRD-004-02** | 1 | Tampilan Modal Tinjauan Memajang Teks Teknis Mentah JSON & Kolom Tidak Diperlukan | `DESIGN_CHANGE` | FE | Medium | SUDAH-VERIFIKASI di source & capture | `FIX-HRD-004-02` |
| **ISS-HRD-004-03** | 2 | Redesain Alur: Eliminasi Modal Hero & Penambahan Tab Kanonikal "Informasi Pribadi" | `DESIGN_CHANGE` | FE | High | SUDAH-VERIFIKASI di source & capture | `FIX-HRD-004-03` |
| **ISS-HRD-004-04** | 3 | Ketiadaan Aksi Tambah pada Sub-Menu Mandiri & Penegasan Matriks Read-Only vs Approval | `BUG`, `RULE_VIOLATION` | FE + BE | High | SUDAH-VERIFIKASI di source | `FIX-HRD-004-04` |
| **ISS-HRD-004-05** | 4 | Teks Buram/Pudar Akibat Tertimpa Footer Fixed Glass & Tipografi Mikro | `BUG` | FE | High | SUDAH-VERIFIKASI di source & capture | `FIX-HRD-004-05` |
| **ISS-HRD-004-06** | 5 | Data Kepegawaian Utama Hilang di Header & Ringkasan ESS Dibandingkan Master Data | `CONTRACT_MISMATCH` | BE + FE | High | SUDAH-VERIFIKASI di source & capture | `FIX-HRD-004-06` |

---

## 4. Rincian Per Temuan

### ISS-HRD-004-01 — Penolakan HTTP 400 Bad Request saat Submit Alamat & Rekening Bank Mandiri

| Atribut | Nilai |
| :--- | :--- |
| **No. Laporan** | 1 |
| **Jenis** | `BUG`, `CONTRACT_MISMATCH` |
| **Area** | Backend & Frontend |
| **Keparahan** | **Blocker** |
| **Status Bukti** | `SUDAH-VERIFIKASI` di source kode dan log capture pelapor |
| **Perbaikan** | `FIX-HRD-004-01` |

**Apa yang terjadi.**
Saat karyawan mengisi form penambahan alamat baru atau rekening bank dan menekan tombol simpan/ajukan, permohonan langsung gagal dan menampilkan pesan kesalahan: *"Request failed with status code 400"*. Pada inspeksi konsol/network, server menolak karena atribut `FieldGroup` dan `TargetEntityName` dianggap kosong.

**Kenapa terjadi.**
Rantai penyebab teknis:
1. Pada frontend file [`employee-profile-changes-self-service-view.jsx:814-819`](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/QuilvianSystemFrontendDev/src/components/view/self-services/human-resource/employee/employee-profile-changes-self-service-view.jsx#L814-L819):
   ```javascript
   const detailsList = Object.values(proposalChanges).map((item) => ({
     targetEntity: item.targetEntity,
     fieldName: item.fieldName,
     oldValue: String(item.oldValue ?? ""),
     newValue: String(item.newValue ?? ""),
   }));
   ```
   Frontend mengirim objek dengan properti berhuruf kecil `targetEntity` dan sama sekali **tidak mengirimkan** properti `fieldGroup` (atau `FieldGroup`) dan `targetEntityName` (atau `TargetEntityName`).
2. Pada backend file [`EmployeeProfileChangeDtos.cs:162-185`](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/Corporate/HumanResource/WorkforceCore/DTOs/EmployeeProfileChangeDtos.cs#L162-L185), DTO `CreateEmployeeProfileChangeDetailRequest` mewajibkan validasi ketat:
   ```csharp
   [Required, MaxLength(100)]
   public string FieldGroup { get; set; } = string.Empty;

   [Required, MaxLength(150)]
   public string TargetEntityName { get; set; } = string.Empty;
   ```
   Akibatnya, model state validator ASP.NET Core langsung memblokir permintaan sebelum controller dieksekusi.
3. Selain itu, pada service backend [`EmployeeProfileChangeService.cs:87-145`](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/Corporate/HumanResource/WorkforceCore/Services/EmployeeProfileChangeService.cs#L87-L145), kamus `AllowedFields` saat ini hanya mendaftarkan entitas `MstWorkforceProfile` dan `MstEmployee`. Entitas relasi penambahan seperti `WfpAddress` dan `WfpBankAccount` belum didaftarkan sebagai field yang valid untuk pengajuan perubahan, atau membutuhkan penanganan usulan entitas baru (*proposed new entity payload*).

**Apakah ini menyimpang dari desain?**
`BUG`. Desain `HRD-DEC-058..061` dan `PLAN-REPAIR-HRD-003` mengamanatkan karyawan dapat mengajukan perubahan alamat dan rekening bank mandiri secara sah ke HRD.

**Dampak nyata.**
Karyawan rumah sakit sama sekali tidak bisa memperbarui data rekening bank untuk kebutuhan transfer gaji maupun alamat domisili untuk keperluan administrasi RSMMC.

**Rekomendasi.**
1. Pada frontend: Kirimkan `TargetEntityName`, `FieldGroup`, `FieldName`, `ValueType` secara lengkap dan valid sesuai kontrak DTO backend.
2. Pada backend: Daftarkan entitas dan grup field `WfpAddress` dan `WfpBankAccount` ke dalam `AllowedFields` dan perbarui logika `BuildDetailEntitiesAsync` serta `ApplyAsync` agar mengenali mutasi entitas relasi WFP.

---

### ISS-HRD-004-02 — Tampilan Modal Tinjauan Memajang Teks Teknis Mentah JSON & Kolom Tidak Diperlukan

| Atribut | Nilai |
| :--- | :--- |
| **No. Laporan** | 1 |
| **Jenis** | `DESIGN_CHANGE` |
| **Area** | Frontend |
| **Keparahan** | Medium |
| **Status Bukti** | `SUDAH-VERIFIKASI` di source dan tangkapan layar `media_1794373252033_ebe6637c.jpg` |
| **Perbaikan** | `FIX-HRD-004-02` |

**Apa yang terjadi.**
Pada modal konfirmasi pengajuan ("Tinjauan Pengajuan Perubahan Data Mandiri"), tabel komparasi menampilkan kolom "Nilai Baru (Diusulkan)" yang berisi teks gabungan mentah seperti `addressType: Identity, addressLine: test , countryId: e19e1af2-1cd7-4fa1-a117-9e7d4ea4a2ff`. Pengguna menyatakan kolom nilai baru dalam bentuk tersebut tidak perlu ditampilkan dan teks teknis itu tidak diperlukan di JSON backend.

**Kenapa terjadi.**
Pada file [`employee-profile-changes-self-service-view.jsx:701-704`](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/QuilvianSystemFrontendDev/src/components/view/self-services/human-resource/employee/employee-profile-changes-self-service-view.jsx#L701-L704), handler form menangkap payload objek lalu mengubahnya secara paksa menjadi teks gabungan (`summaryText = payloadEntries.map(([k, v]) => `${k}: ${v}`).join(", ")`) yang mencetak identifier GUID teknis (`countryId: e19e1af2...`). Teks ini kemudian dipajang mentah-mentah ke dalam tabel modal review.

**Apakah ini menyimpang dari desain?**
`DESIGN_CHANGE`. Sebelumnya diasumsikan ringkasan string cukup, namun pelapor menginginkan antarmuka yang bersih, manusiawi, dan tidak memajang kolom nilai baru mentah atau data teknis tak berguna.

**Dampak nyata.**
Staf medis/karyawan awam bingung melihat string kode teknis dan GUID database saat hendak mengajukan perubahan data.

**Rekomendasi.**
1. Hapus kolom "Nilai Baru (Diusulkan)" yang menampilkan dump string teknis pada tabel tinjauan.
2. Gantikan struktur tabel tinjauan dengan format ringkas ramah pengguna: **Menu Profil**, **Jenis Usulan**, **Rincian Data** (misal: *"Alamat Domisili Baru: Jl. Rasuna Said No. 10"* tanpa GUID mentah), dan **Sifat Data** (Umum / Sensitif).

---

### ISS-HRD-004-03 — Redesain Alur: Eliminasi Modal Hero & Penambahan Tab Kanonikal "Informasi Pribadi"

| Atribut | Nilai |
| :--- | :--- |
| **No. Laporan** | 2 |
| **Jenis** | `DESIGN_CHANGE` |
| **Area** | Frontend |
| **Keparahan** | High |
| **Status Bukti** | `SUDAH-VERIFIKASI` di source dan tangkapan layar `media_1794373257474_72c37458.jpg` |
| **Perbaikan** | `FIX-HRD-004-03` |

**Apa yang terjadi.**
Pada bagian atas (hero banner), terdapat tombol *"Mode Ubah Data Pribadi"* yang saat ditekan memunculkan modal pop-up raksasa berisi 15+ isian form identitas kependudukan dan kontak pribadi. Pelapor meminta modal ini dihilangkan saja, dan digantikan menjadi **Menu/Tab baru "Informasi Pribadi"** di sisi kiri. Menu tersebut menampilkan data identitasnya dalam mode baca, dan menyediakan tombol edit jika data sudah ada, yang perubahannya wajib melalui alur persetujuan HRD.

**Kenapa terjadi.**
Implementasi sebelumnya memisahkan pengubahan data identitas pokok ke modal terpisah di tombol hero, sementara data workforce lainnya (alamat, bank, keluarga) berada di dalam tab samping. Hal ini menciptakan inkonsistensi pengalaman pengguna (*user experience fragmentation*).

**Apakah ini menyimpang dari desain?**
`DESIGN_CHANGE`. Permintaan pengguna menyatukan seluruh tata kelola data ke dalam sistem tab kanonikal yang seragam.

**Dampak nyata.**
Form modal pop-up terasa canggung, berukuran terlalu panjang di layar kecil, dan terpisah dari tata letak tab utama.

**Rekomendasi.**
1. Hapus tombol *"Mode Ubah Data Pribadi"* dari Hero banner dan hapus komponen modal `Modal Ubah Data Pribadi (Ringkasan Profil)`.
2. Tambahkan tab kanonikal baru berurutan pada posisi pertama/kedua di daftar menu samping: **"Informasi Pribadi"** (`personalInfo`).
3. Pada panel tab "Informasi Pribadi", sajikan kartu informasi lengkap data kependudukan dan kontak pribadi (Nama Lengkap, NIK, Tempat/Tgl Lahir, Jenis Kelamin, Agama, Status Pernikahan, Golongan Darah, Telepon, WhatsApp, Email).
4. Sediakan tombol *"Ubah Informasi Pribadi"* pada panel tersebut yang membuka laci editor (*drawer editor*) terstandar, di mana hasil perubahannya masuk ke daftar usulan perubahan untuk diajukan ke HRD.

---

### ISS-HRD-004-04 — Ketiadaan Aksi Tambah pada Sub-Menu Mandiri & Penegasan Matriks Read-Only vs Approval

| Atribut | Nilai |
| :--- | :--- |
| **No. Laporan** | 3 |
| **Jenis** | `BUG`, `RULE_VIOLATION` |
| **Area** | Frontend & Backend |
| **Keparahan** | High |
| **Status Bukti** | `SUDAH-VERIFIKASI` di source kode |
| **Perbaikan** | `FIX-HRD-004-04` |

**Apa yang terjadi.**
Ketika karyawan memilih sub-menu lain seperti Kontak Darurat atau Pendidikan, tidak ditemukan tombol atau ikon untuk menambah data baru. Pelapor meminta analisis menyeluruh mengenai batasan hak akses: mana menu yang wajib *read-only* (seperti penempatan organisasi dan jabatan) dan mana yang boleh ditambah/diedit melalui approval HRD.

**Kenapa terjadi.**
1. Pada file [`employee-profile-changes-self-service-view.jsx:261`](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/QuilvianSystemFrontendDev/src/components/view/self-services/human-resource/employee/employee-profile-changes-self-service-view.jsx#L261):
   `const [workforceResources] = useState(EMPTY_OBJECT);`
   Layar ESS tidak pernah menginisialisasi atau memuat state data resource sub-menu dari Redux/API (`workforceResources` selalu kosong melompong).
2. Akibatnya, komponen `ResourcePanel` (`workforce-profile-dashboard.jsx`) tidak memiliki data item untuk dirender, dan konfigurasi editor untuk beberapa resource belum dihubungkan ke alur penangkapan proposal.

**Apakah ini menyimpang dari desain?**
`BUG` pada ketiadaan tombol/data kontak darurat, dan `RULE_VIOLATION` terhadap prinsip tata kelola RS jika wewenang manajerial tercampur dengan hak mandiri karyawan.

**Analisis Matriks Hak Akses Karyawan vs HRD:**

| Menu / Tab Workforce | Status Wewenang Karyawan | Alur Kerja yang Sah | Dasar Regulasi / Tata Kelola RS |
| :--- | :---: | :--- | :--- |
| **Informasi Pribadi** | **BISA UBAH** | Usulan Mandiri → Wajib Lampiran Bukti KTP/KK (jika sensitif) → Verifikasi HRD → Workflow Engine → Master Terupdate | Kepatuhan Data Kependudukan (UU PDP & Kebijakan HR) |
| **Alamat** | **BISA TAMBAH / UBAH** | Usulan Mandiri → Verifikasi HRD → Master Terupdate | Pembaruan Domisili Karyawan |
| **Rekening Bank** | **BISA TAMBAH / UBAH** | Usulan Mandiri → Verifikasi HRD (Buku Tabungan) → Master Terupdate | Validasi Rekening Payroll Resmi |
| **Kontak Darurat** | **BISA TAMBAH / UBAH** | Usulan Mandiri → Verifikasi HRD → Master Terupdate | Keselamatan Kerja & Tanggap Darurat RS (K3RS) |
| **Pendidikan Formal** | **BISA TAMBAH / UBAH** | Usulan Mandiri → Wajib Unggah Ijazah/Transkrip → Verifikasi HRD | Verifikasi Kualifikasi SDM |
| **Dokumen Pribadi** | **BISA UNGGAH** | Unggah Dokumen Mandiri (KTP, NPWP, KK) → Verifikasi HRD | Kelengkapan Berkas Tenaga Kerja |
| **Anggota Keluarga & Tanggungan** | **BISA TAMBAH / UBAH** | Usulan Mandiri → Wajib Unggah Kartu Keluarga / Akta Lahir → Verifikasi HRD | Hak Tunjangan & Asuransi Kesehatan/BPJS |
| **Pelatihan & Sertifikasi Eksternal** | **BISA TAMBAH** | Usulan Mandiri → Wajib Unggah Sertifikat Pelatihan/Seminar → Verifikasi HRD/Diklat | Pengembangan Kompetensi Karyawan |
| **Penempatan Organisasi** | 🔒 **MUTLAK READ-ONLY** | Hanya HRD / Direksi via SK Direksi & Surat Mutasi Resmi | **Wewenang Korporat RS**: Karyawan dilarang memindahkan unit kerjanya sendiri. |
| **Penempatan Jabatan** | 🔒 **MUTLAK READ-ONLY** | Hanya HRD / Direksi via SK Promosi / Demosi / Pengangkatan | **Wewenang Korporat RS**: Jabatan struktural & fungsional ditetapkan manajemen. |
| **Relasi Atasan Langsung** | 🔒 **MUTLAK READ-ONLY** | Ditetapkan oleh Bagan Organisasi Resmi RSMMC | **Wewenang Manajerial**: Hubungan komando hirarkis rumah sakit. |
| **Penetapan Gaji & Benefit** | 🔒 **MUTLAK READ-ONLY** | Wewenang Komite Remunerasi & Payroll HRD | **Kerahasiaan Finansial**: Komponen gaji dan grade tarif master. |
| **Riwayat Kontrak (PKWT/PKWTT)** | 🔒 **MUTLAK READ-ONLY** | Diterbitkan Legal HRD & Ditandatangani Direksi | **Hukum Ketenagakerjaan**: Dokumen legalitas masa kerja resmi. |
| **Riwayat Kepegawaian (SK)** | 🔒 **MUTLAK READ-ONLY** | Catatan arsip SK kepegawaian resmi rumah sakit | **Arsip Statis**: Catatan historis mutasi / pengangkatan. |
| **Kredensial & Lisensi (STR/SIP)** | 🔒 **READ-ONLY di ESS** | Dikelola melalui Sub-Komite Kredensial Medis/Keperawatan | **Regulasi Kemenkes RI**: Wajib verifikasi keabsahan STR di KTKI/KKI. |
| **Rincian Kewenangan Klinis (RKK)** | 🔒 **MUTLAK READ-ONLY** | Ditetapkan oleh Direktur Utama atas Rekomendasi Komite Medis | **Keselamatan Pasien (Patient Safety)**: Batas wewenang tindakan medis nakes. |
| **Asesmen Kompetensi** | 🔒 **MUTLAK READ-ONLY** | Dikelola Bagian Diklat / Asesor Resmi RS | Hasil uji kompetensi internal RS. |
| **Rekam Kesehatan Kerja (MCU K3RS)**| 🔒 **MUTLAK READ-ONLY** | Dikelola oleh Instalasi K3RS & Dokter Okupasi | **Kerahasiaan Medis (Permenkes 24/2022)**: Catatan status fit to work. |

**Rekomendasi.**
1. Kunci tab Penempatan Organisasi, Penempatan Jabatan, Relasi Atasan, Penetapan Gaji, Riwayat Kontrak, Riwayat Kepegawaian, Kredensial, Hak Klinis, dan Rekam Kesehatan sebagai `readOnly: true` permanen di portal ESS.
2. Aktifkan tombol tambah dan ubah data pada tab Kontak Darurat, Pendidikan, Dokumen, Keluarga, dan Pelatihan dengan menangkap submit ke dalam daftar usulan perubahan (*Proposal Changes List*) menuju approval HRD.
3. Hubungkan pembacaan state resource data aktif karyawan melalui hook terstandar sehingga data kontak darurat dan lainnya yang sudah ada dapat tampil di tabel masing-masing tab.

---

### ISS-HRD-004-05 — Teks Buram/Pudar Akibat Tertimpa Footer Fixed Glass & Tipografi Mikro

| Atribut | Nilai |
| :--- | :--- |
| **No. Laporan** | 4 |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | High |
| **Status Bukti** | `SUDAH-VERIFIKASI` di source dan tangkapan layar `media_1794372319160_82fc0d3d.jpg` |
| **Perbaikan** | `FIX-HRD-004-05` |

**Apa yang terjadi.**
Pada tangkapan layar gambar ke-1, baris kedua dan ketiga kartu "Ringkasan Profil Workforce" (email, lokasi kerja, akun login, status) serta kartu "Kepatuhan Dokumen & Lisensi" terlihat sangat buram, samar, dan tampak seperti tidak terlihat / terpotong.

**Kenapa terjadi.**
1. File [`src/style/components/features/footer.css:44-76`](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/QuilvianSystemFrontendDev/src/style/components/features/footer.css#L44-L76) mengatur bilah footer secara melayang permanen (*fixed position*) di bagian bawah layar dengan lapisan kaca kabur (`backdrop-filter: blur(18px); z-index: 99 !important;`).
2. Kontainer halaman ESS (`employee-workforce-page`) tidak memiliki padding bawah (*bottom safe space*) yang cukup, sehingga saat viewport tidak cukup tinggi, bilah footer fixed langsung mendarat di atas kartu informasi. Efek blur kaca menutupi teks di bawahnya sehingga teks tampak pudar dan tidak jelas terbaca.
3. Ditambah lagi, ukuran font label kartu pada [`wfp-workforce-detail.css:290-309`](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/QuilvianSystemFrontendDev/src/style/hr/workforce/wfp-workforce-detail.css#L290-L309) diatur sangat mikro (`font-size: 9px;` warna abu-abu terang `#64748b`), yang memperparah rendahnya kontras keterbacaan.

**Apakah ini menyimpang dari desain?**
`BUG`. Antarmuka aplikasi bisnis wajib memiliki keterbacaan tinggi dan tidak boleh tertutup oleh elemen navigasi fixed.

**Dampak nyata.**
Karyawan tidak dapat membaca alamat email terdaftar, status akun login, maupun kepatuhan dokumen karena tertutup dan buram.

**Rekomendasi.**
1. Berikan safe bottom scroll space minimal `130px - 160px` pada kontainer `.employee-workforce-page` dan `.employee-workforce-shell` sehingga seluruh kartu terbawah dapat di-scroll naik bebas ke atas bilah footer.
2. Tingkatkan ukuran font label dari `9px` ke `11px` (`font-weight: 600`, warna `#475569`) dan font nilai dari `11px` ke `13px` (`font-weight: 600`, warna `#0f172a`).

---

### ISS-HRD-004-06 — Data Kepegawaian Utama Hilang di Header & Ringkasan ESS Dibandingkan Master Data

| Atribut | Nilai |
| :--- | :--- |
| **No. Laporan** | 5 |
| **Jenis** | `CONTRACT_MISMATCH` |
| **Area** | Backend & Frontend |
| **Keparahan** | High |
| **Status Bukti** | `SUDAH-VERIFIKASI` di source dan perbandingan Gambar 1 vs Gambar 2 |
| **Perbaikan** | `FIX-HRD-004-06` |

**Apa yang terjadi.**
Pada Master Data Karyawan (Gambar 2), seksi "Identitas Karyawan" menampilkan Departemen ("Keperawatan") dan Posisi ("Perawat Rawat Inap"), serta seksi "Kepegawaian & Penempatan" menampilkan Grade Level ("STAFF"), Lokasi Kerja ("Ruang Poliklinik Rawat Jalan"), Tanggal Bergabung ("04 Apr 2026"), dan Kontrak Berakhir ("04 Apr 2027"). Namun pada ESS Kelola Data Saya (Gambar 1), hampir seluruh informasi tersebut HILANG dan hanya menyisakan garis pemisah kosong yang patah.

**Kenapa terjadi.**
1. Di backend [`EmployeeProfileChangeDtos.cs:350-410`](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/Corporate/HumanResource/WorkforceCore/DTOs/EmployeeProfileChangeDtos.cs#L350-L410), DTO `EmployeeCurrentProfileResponse` belum menyertakan field `GradeLevel`, `JoinDate`, `ContractEndDate`, `EmploymentType`, dan `ProfessionType`.
2. Di frontend [`employee-profile-changes-self-service-view.jsx:336-353`](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/QuilvianSystemFrontendDev/src/components/view/self-services/human-resource/employee/employee-profile-changes-self-service-view.jsx#L336-L353), data dipetakan ke nama properti yang keliru (`departmentName` alih-alih `primaryDepartmentName`, `positionName` alih-alih `primaryPositionName`).
3. Di [`workforce-profile-dashboard.jsx:4342`](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/QuilvianSystemFrontendDev/src/components/features/workforce/workforce-profile-dashboard.jsx#L4342), fungsi `maybeRow` mengeliminasi (membuang) seluruh baris yang nilainya kosong/null karena field-field tersebut tidak ditandai `alwaysVisible: true`.

**Apakah ini menyimpang dari desain?**
`CONTRACT_MISMATCH`. Data resmi yang tampil pada master data seharusnya konsisten terlihat oleh karyawan yang bersangkutan pada portal mandirinya.

**Dampak nyata.**
Karyawan merasa datanya belum tercatat di sistem rumah sakit karena status departemen, grade, dan tanggal kontraknya tidak muncul.

**Rekomendasi.**
1. Perkaya DTO backend `EmployeeCurrentProfileResponse` dengan seluruh field kepegawaian resmi.
2. Sinkronkan mapping frontend ke `primaryDepartmentName`, `primaryPositionName`, `gradeLevel`, `joinDate`, dan `contractEndDate`.
3. Set `alwaysVisible: true` pada field profil utama di dashboard workforce agar baris tidak terbuang dan slot informasi tetap simetris menampilkan `"-"` jika belum terisi.

---

## 5. Tanya-Jawab Pelapor

> **T1:** *Ketika menambahkan alamat baru tersebut, terjadi error 400 Bad Request `FieldGroup is required` dan `TargetEntityName is required`. Kenapa bisa error seperti itu dan kenapa di alamat serta rekening bank sama-sama error?*
>
> **J1:** Galat ini terjadi karena frontend mengirimkan nama properti lama `targetEntity` (huruf kecil dan tanpa akhiran `Name`) dan lupa menyertakan properti wajib `FieldGroup`. Backend ASP.NET Core memiliki validasi ketat `[Required]` untuk kedua field tersebut pada setiap butir rincian pengajuan. Karena alamat dan rekening bank sama-sama diproses oleh alur pengajuan yang sama, keduanya terbentur aturan validasi yang identik di server.

> **T2:** *Apakah kolom 'Nilai Baru (Diusulkan)' yang berisi teks mentah seperti `addressType: Identity...` bisa dihilangkan dari tampilan modal dan pengiriman JSON backend?*
>
> **J2:** Ya, kolom teks mentah itu dapat dan harus dihilangkan. Menampilkan dump teks berformat kode teknis dan GUID database di layar tinjauan adalah kekeliruan tampilan. Format tinjauan akan disederhanakan menjadi ringkasan yang ramah pengguna (misalnya hanya nama jalan atau nomor rekening yang diusulkan), dan JSON backend akan diselaraskan dengan kontrak DTO yang bersih tanpa teks mentah tersebut.

> **T3:** *Saat klik mode ubah data pribadi di hero banner, kenapa muncul modal panjang? Bisakah dihilangkan dan diganti menjadi menu/tab baru 'Informasi Pribadi'?*
>
> **J3:** Ya, modal pop-up pada tombol hero tersebut akan dihilangkan sepenuhnya. Menu samping akan ditambahkan tab kanonikal baru bernama **"Informasi Pribadi"**. Tab ini akan menampilkan ringkasan data kependudukan dan kontak pribadi karyawan secara rapi, dilengkapi tombol edit yang akan memasukkan perubahan ke daftar usulan untuk diajukan ke HRD.

> **T4:** *Kenapa menu lain seperti Kontak Darurat tidak ada ikon tambah datanya? Mana menu yang boleh diedit dan mana yang wajib read-only?*
>
> **J4:** Tombol tambah tidak muncul karena state data sub-menu mandiri belum dimuat pada layar ESS. Berdasarkan tata kelola rumah sakit, menu dibagi menjadi dua kategori:
> - **Bisa Ditambah/Diedit (via Approval HRD):** Informasi Pribadi, Alamat, Rekening Bank, Kontak Darurat, Pendidikan Formal, Dokumen Pribadi, Keluarga & Tanggungan, serta Pelatihan Mandiri.
> - **Mutlak Read-Only (Wewenang Manajemen/HRD):** Penempatan Organisasi, Penempatan Jabatan, Relasi Atasan, Penetapan Gaji, Riwayat Kontrak, Riwayat Kepegawaian, Kredensial Medis/Keperawatan, Hak Klinis, Asesmen Kompetensi, dan Rekam Kesehatan K3RS.

---

## 6. Temuan Tambahan

### ISS-HRD-004-T1 — Penanganan State Sumber Daya Workforce Mandiri (`workforceResources`) Selalu Kosong

| Atribut | Nilai |
| :--- | :--- |
| **Area** | Frontend |
| **Keparahan** | High |
| **Kenapa Dicantumkan** | Ditemukan saat menelusuri ketiadaan tombol tambah pada kontak darurat (Butir 3) |
| **Perbaikan** | `FIX-HRD-004-04` |

**Uraian Temuan:**
Pada [`employee-profile-changes-self-service-view.jsx:261`](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/QuilvianSystemFrontendDev/src/components/view/self-services/human-resource/employee/employee-profile-changes-self-service-view.jsx#L261), dideklarasikan:
```javascript
const [workforceResources] = useState(EMPTY_OBJECT);
```
Tidak ada pemanggilan service atau selektor Redux yang mengisi objek ini untuk karyawan login. Akibatnya, seluruh tab sub-menu (Alamat, Bank, Kontak Darurat, dsb.) menganggap tidak ada data existing di server (`items = []`), sehingga data yang sebenarnya sudah tersimpan di master tidak pernah tampil di tabel tab masing-masing.

---

## 7. Pertanyaan dan Keputusan yang Dibutuhkan

### Untuk Pelapor:
*Seluruh pertanyaan telah terjawab secara jelas melalui masukan terperinci pada instruksi pelapor (Point 1, Point 2, dan Point 3).*

### Untuk Pemilik Sistem / Manajemen HRD:

| No | Keputusan yang Dibutuhkan | Rekomendasi Solusi | Dampak Bila Ditunda |
| :---: | :--- | :--- | :--- |
| **K-01** | Pengesahan Matriks Pembagian Wewenang: Menetapkan secara permanen bahwa Penempatan Organisasi, Jabatan, Gaji, Kontrak, Kredensial, dan Hak Klinis berstatus **Mutlak Read-Only** bagi karyawan di portal ESS. | **Setujui Rekomendasi.** Menjaga integritas tata kelola RS, kepatuhan Kemenkes RI, dan prinsip keselamatan pasien (*patient safety*). | Karyawan berpotensi menuntut pengubahan unit kerja atau grade gaji secara sepihak. |
| **K-02** | Pengesahan Alur Usulan Mandiri: Menghilangkan modal pop-up hero dan mengalihkan seluruh pengelolaan data identitas pribadi ke Tab Kanonikal **"Informasi Pribadi"**. | **Setujui Rekomendasi.** Memberikan pengalaman pengguna yang konsisten, bersih, dan sesuai standar arsitektur dashboard Quilvian. | Antarmuka terfragmentasi dan membingungkan pengguna. |

---

## 8. Catatan Pola

- **Pola Pelanggaran Kontrak DTO (Case-Sensitivity & Required Fields):**
  Frontend kerap mengirim objek JavaScript dengan camelCase yang tidak cocok dengan DTO C# yang membutuhkan PascalCase atau properti yang dinamai berbeda (`targetEntity` vs `TargetEntityName`). Seluruh model request di frontend wajib memiliki transformer / serializer yang memvalidasi keberadaan field required sebelum payload dikirimkan.
- **Pola Tumpang-Tindih Elemen Fixed (Z-Index War & Safe Space):**
  Penggunaan bilah navigasi atau footer melayang dengan `position: fixed` wajib selalu disertai pengaturan `padding-bottom` (safe scroll space) pada tingkat kontainer halaman agar tidak pernah menutupi konten data di belakangnya.

---

## 9. Riwayat Dokumen

| Tanggal | Penulis | Perubahan |
| :---: | :---: | :--- |
| **2026-11-11** | Antigravity AI Agent | Penyusunan dokumen issue resmi `ISSUE-HRD-004` mencakup diagnosis mendalam kegagalan HTTP 400 Alamat/Bank, usulan Tab Informasi Pribadi, matriks wewenang read-only vs approval, penataan footer glass, dan kelengkapan data kepegawaian. |
