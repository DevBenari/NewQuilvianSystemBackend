# Human Resource — PRD ke MVP

## 1. Identitas dokumen

| Field | Value |
| --- | --- |
| Produk | Quilvian Hospital Information System |
| Modul | Human Resource (`human-resource`, prefix `HRD`) |
| Blueprint ID | `HRD-BP-001` |
| Dokumen | `04-prd-to-mvp.md` |
| `contract_version` | `v6` |
| `last_changed_in` | `v7` — bagian 21 (`S-D1`) ditambahkan 3 November 2026; bagian 1–20 tidak berubah kecuali dua baris rujukan |
| Status | `draft` — **belum** `approved`. Approval adalah tindakan manusia, bukan keluaran skill |
| Owner | Pemilik produk HR bersama technical owner (`HRD-DEC-015`) |
| `approved_by` / `approved_at` | **Hanya bagian 21** (`S-D1` Perencanaan Tenaga Kerja): `approved` oleh Pengguna — pemilik teknis dan produk (`HRD-DEC-015`) — pada 2026-11-03. Bagian lain dokumen ini **belum** disetujui, sehingga status dokumen tetap `draft` |
| Repository backend | `NewQuilvianSystemBackend`, branch kerja `AndryZain` |
| Repository frontend | `QuilvianSystemFrontendDev`, branch `AgentCodexFrontend` |
| Backend SHA baseline | `e0ee42c752a5f92c5b1663ff88bef07a5859f79f` |
| Backend baseline canonical | `origin/QuilvianIntegrationBackend` (`HRD-DEC-021`), diverifikasi `16b8b71` |
| Frontend SHA baseline | `fff76a1b394d4b247c70a04f106c8ec098c9696e` |
| `input_revision` | `02-backend-architecture.md` rev `2`; `03-frontend-architecture.md` rev `2`; seluruh `contracts/` `v6`; `data/data-dictionary.md` `v6`; `flowcharts/**` |
| `input_hash` — decision log | `26da64e129b0c34a17b67b0a41f47986b1aedae92a600077d936b0741eb574ee` |
| Kesiapan arsitektur domain | `DOMAIN_ARCHITECTURE_NOT_RUN` — seluruh kemampuan dalam cakupan bersifat administratif ketenagakerjaan |
| Ringkasan cakupan | Menutup rantai pekerjaan HR harian dari jadwal terbit sampai payroll diserahkan, dengan pengecualian kemampuan klinis dan enam domain tanpa API yang tetap `BLOCKED` |

**Aturan tunggal yang mengikat seluruh isi dokumen ini: dokumen ini menurunkan, tidak
menciptakan.** Setiap entity, status, hak akses, dan endpoint yang disebut di sini sudah ada di
`02-backend-architecture.md`, `data/data-dictionary.md`, `flowcharts/**`, atau `contracts/`.

---

## 2. Ringkasan eksekutif

Modul HR Quilvian punya masalah yang tidak biasa: **backend-nya jauh lebih matang daripada yang
dapat dipakai orang.** Ada 150 controller dan 1.343 endpoint HR di backend hari ini. Frontend
operasionalnya hampir tidak ada — hanya master data dan satu formulir absensi.

Akibatnya nyata dan terasa setiap hari: 93 endpoint cuti tanpa satu pun pemakai, 78 endpoint
lembur tanpa satu pun pemakai, dan tidak ada satu pun layar persetujuan bagi atasan. Pekerjaan
HR yang sudah dibangun perangkatnya tetap dikerjakan di luar sistem.

MVP ini tidak membangun modul HR dari nol. Ia **menyambungkan apa yang sudah ada** menjadi satu
rantai yang benar-benar dapat dijalankan satu bulan penuh: jadwal terbit, pegawai mencatat
kehadiran, pengajuan diputuskan atasan dari satu kotak masuk, kehadiran diolah, periode ditutup,
dan hasilnya diserahkan ke payroll.

Hasil bisnis yang dikejar: **satu unit rumah sakit dapat menjalankan satu periode kerja penuh di
dalam sistem, dengan jejak yang dapat diperiksa saat terjadi sengketa jam kerja atau saldo cuti.**

---

## 3. Masalah produk

### 3.1 Apa yang sudah ada

| Kemampuan | ID | Keadaan | Bukti |
| --- | --- | --- | --- |
| Master data HR | `HRD-CAP-01` | `READY TO REUSE` | 65 controller, 618 endpoint; frontend sudah memakai 64 kelompok route |
| Profil pegawai dan berkas kepegawaian | `HRD-CAP-02` | `READY TO REUSE` | 14 controller, 145 endpoint |
| Konteks pengguna layanan mandiri | `HRD-CAP-06` | `READY TO REUSE` | Kepemilikan data diturunkan dari pengguna terautentikasi |
| Hak akses dan jejak audit | `HRD-CAP-26` | `READY TO REUSE` | 150 dari 150 controller HR memakai penjagaan akses; tidak ada yang terbuka |
| Kehadiran dan koreksi | `HRD-CAP-04` | `EXTEND` | 71 endpoint termasuk tutup dan buka periode |
| Cuti, izin, dan saldo | `HRD-CAP-07` | `EXTEND` | 93 endpoint |
| Lembur | `HRD-CAP-08` | `EXTEND` | 78 endpoint |
| Workflow dan persetujuan bersama | `HRD-CAP-23` | `EXTEND` | 48 endpoint; mesinnya ada |

### 3.2 Apa yang belum ada, dan itulah masalahnya

| Masalah | ID | Akibat yang dirasakan hari ini |
| --- | --- | --- |
| **Nol konsumen frontend** untuk cuti, lembur, dan administrasi kehadiran | `HRD-CAP-04`, `HRD-CAP-07`, `HRD-CAP-08` | 242 endpoint yang tidak dapat dipakai siapa pun. Pekerjaannya tetap manual |
| **Tidak ada antarmuka atasan sama sekali** | `HRD-CAP-24` | Seluruh rantai persetujuan HR tidak punya layar. Atasan tidak dapat memutuskan apa pun di sistem |
| Menu Administrasi Kepegawaian menunjuk halaman yang tidak ada | `HRD-CAP-03` | **Cacat yang sudah terlihat pengguna.** Enam butir menu membuka halaman kosong |
| Route layanan mandiri kehadiran melanggar konvensi | `HRD-CAP-05` | Berfungsi, tetapi tidak seragam dengan modul lain |
| Penjadwalan operasional tidak berjalan | `HRD-CAP-09` | Rumah sakit 24 jam tanpa mesin roster. Ini celah terbesar di modul |
| Tidak ada satu pun test yang menyentuh HR | `HRD-CAP-27` | Tidak ada jaring pengaman untuk 1.343 endpoint |

### 3.3 Yang berada di luar jangkauan MVP ini

Enam domain punya skema lengkap tetapi **nol controller**: perencanaan tenaga kerja, rekrutmen,
benefit, layanan HR, perjalanan dinas, dan reimbursement. Isi tabelnya belum diketahui, sehingga
keputusan skema yang merusak data tidak boleh diambil — `HRD-Q-05`.

Kredensial, kewenangan klinis, OPPE, FPPE, dan kesehatan kerja staf menunggu pihak yang berwenang
menetapkan batasnya.

---

## 4. Visi produk

Rantai keterhubungan data yang ingin dicapai, ditulis sebagai urutan:

1. Seorang pegawai punya **satu** identitas workforce yang dipakai seluruh proses HR.
2. Identitas itu punya **penempatan organisasi** dan **penetapan gaji** yang berlaku sejak tanggal tertentu.
3. Penempatan itu masuk ke **roster** unitnya, lalu terbit sebagai **jadwal kerja** per hari.
4. Jadwal itu menjadi acuan saat **rekaman kehadiran** diolah menjadi **kehadiran harian**.
5. Penyimpangan terhadap jadwal menjadi **pengecualian**, yang diselesaikan lewat **koreksi** atau diabaikan dengan alasan tercatat.
6. **Cuti** dan **lembur** yang disetujui ikut mengubah kehadiran harian, dan masing-masing meninggalkan jejak di **buku besar saldo** atau **realisasi**.
7. Seluruh pengajuan pada langkah 5 dan 6 diputuskan dari **satu kotak masuk persetujuan**.
8. Ketika tidak ada lagi pengecualian pemblokir, **periode kehadiran ditutup**.
9. Periode yang tertutup menjadi masukan **putaran payroll**, yang dihitung, diperiksa, disetujui, lalu **diserahkan**.
10. Setelah serah terima, **tanggung jawab HR selesai**.

Rantai ini punya satu sifat yang menentukan seluruh desainnya: **setiap angka di ujung dapat
ditelusuri kembali ke bukti di pangkalnya.**

---

## 5. Batas MVP

### 5.1 Titik mulai

1. Master data HR sudah terisi sesuai rencana data master awal pada `02-backend-architecture.md` bagian 10.
2. Profil pegawai untuk unit percontohan sudah ada dan aktif.
3. Penempatan organisasi dan penetapan gaji untuk pegawai itu sudah berlaku.
4. Matriks persetujuan untuk unit percontohan sudah diisi.
5. Periode kehadiran untuk bulan berjalan sudah dibuka.

### 5.2 Titik akhir

1. Satu unit menjalankan **satu periode penuh** di dalam sistem, dari jadwal terbit sampai serah terima payroll dijalankan.
2. Seluruh pengajuan cuti, lembur, dan koreksi kehadiran pada periode itu diputuskan dari kotak masuk persetujuan, bukan di luar sistem.
3. Periode kehadiran ditutup tanpa menyisakan pengecualian pemblokir.
4. Angka yang diserahkan ke payroll dapat ditelusuri sampai ke rekaman kehadiran yang mendasarinya.
5. Setiap layar yang dibuat punya jalan masuk yang sah — butir menu, atau layar induk yang menuju ke sana.

**Yang bukan titik akhir MVP:** pembayaran gaji, jurnal akuntansi, dan pelaporan pajak. Ketiganya
milik Finance — `HRD-DEC-009`.

---

## 6. Pelaku sasaran

| Pelaku | Tanggung jawabnya di dalam MVP |
| --- | --- |
| **Pegawai** | Mencatat kehadiran; mengajukan cuti, lembur, koreksi kehadiran, ubah jadwal, tukar shift, dan izin pulang cepat; melihat saldo dan jadwalnya sendiri |
| **Atasan / manajer unit** | Menyusun dan menerbitkan roster unitnya; memutuskan seluruh pengajuan anak buahnya dari satu kotak masuk; mengklasifikasikan pekerjaan di luar jadwal |
| **HR Admin** | Mengelola master data dan profil pegawai; memantau pengecualian kehadiran; menerapkan koreksi yang sudah disetujui; menangani rekaman bermasalah |
| **Petugas payroll** | Menutup dan membuka kembali periode kehadiran; menjalankan putaran payroll; menyerahkan hasilnya |
| **Pejabat berwenang** | Menyetujui perubahan penempatan dan gaji; menyetujui putaran payroll |

Ketiga perspektif layar — HR Admin, layanan mandiri pegawai, dan atasan — dipisahkan sesuai
`03-frontend-architecture.md`. Memakai satu layar untuk ketiganya akan membuat hak akses bocor.

---

## 7. Pemilihan kemampuan MVP

Setiap kemampuan diuji dengan dua pertanyaan: tanpa ini, apakah satu periode kerja dapat selesai
dari awal sampai akhir? Kalau tidak, adakah jalan sementara yang aman dan tetap dapat diaudit?

Masuk `MUST HAVE` hanya bila kedua jawabannya "tidak".

| Kemampuan | ID kemampuan asal | Keputusan MVP |
| --- | --- | --- |
| Master data HR | `HRD-CAP-01` | Wajib; tanpa master terisi, tidak ada proses yang dapat berjalan |
| Profil pegawai | `HRD-CAP-02` | Wajib; seluruh transaksi HR menunjuk profil ini |
| Menu Administrasi Kepegawaian yang tidak menuju halaman kosong | `HRD-CAP-03` | Wajib; ini cacat yang sudah terlihat pengguna hari ini |
| Kehadiran dan koreksi kehadiran | `HRD-CAP-04` | Wajib; tanpa ini periode tidak dapat ditutup |
| Kehadiran layanan mandiri | `HRD-CAP-05` | Wajib; pegawai tidak punya cara lain mencatat kehadiran |
| Konteks pengguna layanan mandiri | `HRD-CAP-06` | Wajib; kepemilikan data ditentukan dari sini |
| Cuti, izin, dan saldo | `HRD-CAP-07` | Wajib; cuti mengubah kehadiran, dan kehadiran menentukan payroll |
| Lembur | `HRD-CAP-08` | Wajib; alasan yang sama dengan cuti |
| Penjadwalan dan tukar shift | `HRD-CAP-09` | Wajib; tanpa jadwal terbit, kehadiran tidak punya acuan pengolahan |
| Payroll sampai serah terima | `HRD-CAP-10` | Wajib; titik akhir MVP |
| Workflow dan persetujuan bersama | `HRD-CAP-23` | Wajib; seluruh pengajuan melewatinya |
| Antarmuka atasan dan kotak masuk persetujuan | `HRD-CAP-24` | Wajib; tanpa ini tidak ada satu pun pengajuan yang dapat diputuskan di sistem |
| Layanan mandiri selain kehadiran | `HRD-CAP-25` | Wajib; pegawai mengajukan cuti dan lembur dari sini |
| Hak akses dan jejak audit | `HRD-CAP-26` | Wajib; sudah ada dan dipakai apa adanya |
| Bukti pengujian | `HRD-CAP-27` | Wajib; tanpa test, tidak ada yang membuktikan MVP benar-benar berjalan |

---

## 8. Kemampuan yang ditunda

Setiap baris menyebut **sebab** dan **penggantinya selama MVP berjalan**. Menunda tanpa pengganti
membuat pengguna kehilangan pekerjaan yang selama ini bisa dilakukan.

| Kemampuan | ID kemampuan asal | Alasan ditunda | Pengganti selama MVP |
| --- | --- | --- | --- |
| Kompetensi dan pelatihan | `HRD-CAP-14` | Tidak berada di jalur kritis satu periode kerja. Pelatihan tidak menentukan apakah periode dapat ditutup | Pencatatan pelatihan tetap berjalan di luar sistem seperti sekarang. Tidak ada kemampuan yang hilang |
| Manajemen kinerja | `HRD-CAP-15` | Siklusnya tahunan atau semesteran, bukan bulanan. Tidak menghalangi penutupan periode | Sama; berjalan di luar sistem seperti sekarang |
| Lifecycle dan offboarding | `HRD-CAP-17` | Pengunduran diri terjadi sesekali, dan jalur manualnya sudah berjalan. Rasio controller terhadap model paling timpang, sehingga biaya penyelesaiannya besar | Proses manual yang sudah berjalan tetap dipakai. HR tetap dapat menonaktifkan profil pegawai lewat kemampuan yang sudah ada |
| Hubungan karyawan dan kedisiplinan | `HRD-CAP-18` | Dua keputusan pemiliknya belum turun: pemisahan peran pengusul dan penyetuju (`HRD-Q-51`), dan tingkatan izin data paling terbatas (`HRD-Q-52`). Merancangnya sekarang berarti menetapkan siapa boleh membaca kasus kedisiplinan tanpa wewenang | Proses manual yang sudah berjalan tetap dipakai, dengan berkas di luar sistem |
| Benefit | `HRD-CAP-11` | `MISSING`. Skema ada, perilaku tidak ada, dan isi tabelnya belum diketahui — `HRD-Q-05` | Berjalan di luar sistem seperti sekarang |
| Kredensial dan kewenangan klinis | `HRD-CAP-12` | **`BLOCKED`**, bukan ditunda karena prioritas. Batas keselamatan klinisnya belum ditetapkan Komite Medik — `HRD-Q-08` | Berjalan di luar sistem. **Tidak ada penggantinya di dalam sistem, dan tidak boleh diadakan** |
| OPPE dan FPPE | `HRD-CAP-13` | **`BLOCKED`**. Belum ada satu pun entity maupun endpoint, dan pemiliknya belum menetapkan apa pun | Sama |
| Kesehatan dan keselamatan kerja staf | `HRD-CAP-16` | **`BLOCKED`**. Aturan akses rekam kesehatan kerja belum disahkan K3RS — `HRD-DEC-010` masih `draft` | Sama |
| Perencanaan tenaga kerja | `HRD-CAP-19` | Tidak lagi `BLOCKED` sejak `HRD-DEC-059`. **Dirancang pada bagian 21** dengan gelombang tersendiri `MVP-7` s.d. `MVP-10` | Berjalan di luar sistem sampai gelombang itu selesai |
| Rekrutmen dan hiring | `HRD-CAP-20` | **`BLOCKED`** oleh `HRD-Q-05`. Domain terbesar yang sepenuhnya tanpa perilaku | Sama |
| Layanan HR dan tiket kepegawaian | `HRD-CAP-21` | **`BLOCKED`** oleh `HRD-Q-05` | Sama |
| Perjalanan dinas dan reimbursement | `HRD-CAP-22` | **`DEFERRED`** karena prioritas paling rendah, dan tetap terikat `HRD-Q-05` | Sama |

---

## 9. Alur bisnis target

`FLOW-HRD-MVP-001` — satu periode kerja penuh, jalur normal:

1. HR Admin memastikan master data dan profil pegawai unit percontohan sudah terisi.
2. Manajer unit membuka periode roster untuk bulan berikutnya.
3. Manajer menempatkan pegawai ke shift; sistem memeriksa bentrok.
4. Manajer menyelesaikan bentrok, lalu menerbitkan jadwal.
5. Pegawai melihat jadwalnya dan mencatat kehadiran masuk serta pulang setiap hari.
6. Pegawai mengajukan cuti, lembur, koreksi kehadiran, ubah jadwal, tukar shift, atau izin pulang cepat sesuai kebutuhannya.
7. Atasan membuka satu kotak masuk berisi seluruh jenis pengajuan itu, lalu memutuskan satu per satu.
8. Sistem mengolah rekaman kehadiran menjadi kehadiran harian, dengan memperhitungkan pengajuan yang sudah disetujui.
9. Penyimpangan terhadap jadwal menjadi pengecualian; HR dan atasan menyelesaikannya lewat koreksi atau pengabaian beralasan.
10. Petugas payroll memeriksa bahwa tidak ada lagi pengecualian pemblokir, lalu menutup periode kehadiran.
11. Petugas payroll membuka putaran payroll, mengumpulkan masukan, menghitung, dan memeriksa hasilnya.
12. Pejabat berwenang menyetujui putaran payroll.
13. Petugas payroll menjalankan serah terima. **Tanggung jawab HR selesai di sini.**

Percabangan dan jalur pengecualian tidak diulang di sini; seluruhnya ada di `flowcharts/**`.

---

## 10. Epic dan functional requirement

### `EPIC HRD-01` — Fondasi route dan registry

**Tujuan.** Menyeragamkan penamaan route tanpa memutus pemanggil yang sudah ada.
**Disposisi backend:** `EXTEND`.

> **`FR-HRD-001` — Route canonical bergaya kebab-case**
>
> Setiap endpoint HR yang mendapat alias dapat dipanggil lewat route bergaya kebab-case, dan
> mengembalikan jawaban yang sama dengan route lamanya.
>
> **Contoh:** memanggil route canonical sebuah endpoint master data HR mengembalikan isi yang
> identik dengan route lamanya, karena keduanya menunjuk satu implementasi yang sama.

> **`FR-HRD-002` — Route lama tetap hidup**
>
> Route lama tetap dapat dipanggil dan tidak menghasilkan kegagalan bagi pemanggil yang ada.
>
> **Contoh:** delapan route yang mendapat alias pada `contracts/api-contract.md` bagian 1.1 tetap
> menjawab `200` setelah alias ditambahkan.

> **`FR-HRD-003` — Satu action, satu implementasi**
>
> Alias ditambahkan sebagai route template pada action yang sama, bukan sebagai controller kedua.
>
> **Contoh:** setelah alias ditambahkan, jumlah action yang melayani kedua route tetap satu.

> **`FR-HRD-004` — Prefix `Wfp` terdaftar sebagai prefix yang sah**
>
> Registry kepemilikan memuat baris `Wfp` beserta pemiliknya, sehingga ia tidak lagi terbaca
> sebagai prefix tak dikenal.

### `EPIC HRD-02` — Administrasi kepegawaian yang dapat dicapai

**Tujuan.** Menutup cacat menu yang sudah terlihat pengguna, menyediakan layar perubahan data, dan
membangun persetujuan penempatan serta remunerasi.
**Disposisi backend:** `EXISTING / REUSE` untuk API perubahan data; **`MISSING / NEW` untuk seluruh
persetujuan penempatan dan remunerasi**; `MISSING / NEW` untuk layar.

> **Peringatan cakupan — jangan disembunyikan saat perencanaan.** `HRD-DEC-031` memperbesar epic
> ini secara material. Tiga entity penempatan **tidak punya** kolom persetujuan, endpoint
> persetujuan, maupun wiring workflow sama sekali; entity gaji punya kolom dan endpoint tetapi
> tanpa gerbang efektivitas dan tanpa pemisahan peran.
>
> `HRD-DEC-036` menegaskan pekerjaannya **tidak** dapat diselesaikan sebagai satu potong. Saat
> perencanaan delivery dijalankan kelak, epic ini **MUST** dipecah sekurang-kurangnya per
> transaksi bisnis:
>
> 1. Persetujuan Penetapan Gaji
> 2. Persetujuan Penempatan Organisasi
> 3. Persetujuan Penempatan Jabatan
> 4. Persetujuan Penetapan Atasan
>
> Infrastruktur workflow bersama boleh dipakai ulang **hanya bila memang generik**, bukan
> dipaksakan agar terlihat hemat. Dua penjaga — gerbang efektivitas dan pemisahan peran — **MUST**
> direncanakan bersama penambahan kolomnya, bukan sesudahnya.

> **`FR-HRD-010` — Enam butir menu Administrasi Kepegawaian menuju halaman yang ada**
>
> Setiap butir menu pada kelompok Administrasi Kepegawaian membuka halaman yang benar-benar ada.
>
> **Contoh:** membuka keenam butir menu berturut-turut menghasilkan enam halaman yang menampilkan
> isi, bukan halaman kosong.

> **`FR-HRD-011` — Permohonan perubahan data melewati verifikasi**
>
> Perubahan data pegawai yang material baru berlaku setelah diverifikasi HR.
>
> **Contoh:** permohonan Ani Lestari mengubah nomor telepon berstatus menunggu verifikasi;
> nomor lamanya masih yang berlaku sampai HR menyetujui.

> **`FR-HRD-012` — Perubahan penempatan dan remunerasi wajib disetujui pihak yang berbeda**
>
> Dasar: `HRD-DEC-031`. Berlaku pada penetapan gaji, penempatan organisasi, penempatan jabatan,
> dan penetapan atasan.
>
> Perubahan **tidak berlaku efektif** sebelum disetujui, dan penyetuju **harus berbeda** dari
> pembuat transaksi.
>
> **Contoh berhasil:** HR Admin Sari mengajukan perubahan gaji Budi Santoso. HR Manager Dewi
> menyetujuinya. Nilai gaji berubah sejak tanggal berlaku.
>
> **Contoh gagal:** HR Admin Sari mengajukan perubahan gaji Budi Santoso lalu mencoba
> menyetujuinya sendiri. Permintaan ditolak dengan `403`, dan nilai gaji **tidak** berubah —
> termasuk ketika unit itu hanya punya satu petugas HR.
>
> **Keadaan hari ini:** perilaku ini **belum ada**. Endpoint persetujuan gaji memakai butir hak
> akses yang sama dengan buat dan ubah, dan tidak ada pemeriksaan status persetujuan sebelum
> penempatan berlaku. Tiga entity penempatan lainnya belum punya kolom persetujuan sama sekali.

> **`FR-HRD-013` — Satu orang = satu profil workforce (`HRD-DEC-051`)**
>
> Setiap individu yang bekerja di rumah sakit (Pegawai, Dokter, Pengguna Eksternal) hanya memiliki tepat satu profil workforce berdasarkan nomor identitas resmi (NIK/paspor).
>
> **Contoh:** HR mendaftarkan dr. Hendra yang sudah ada di modul dokter menjadi pegawai tetap; sistem mendeteksi NIK yang sama dan menautkan peran Pegawai ke profil yang sudah ada tanpa menduplikasi profil (`AC-WFC-06`).

> **`FR-HRD-014` — Penempatan berlaku-efektif adalah satu-satunya sumber kebenaran (`HRD-DEC-047`)**
>
> Formulir edit Master Data Pegawai terkunci dan dilarang mengubah unit organisasi atau jabatan secara langsung. Perpindahan wajib lewat dokumen mutasi/promosi resmi.
>
> **Contoh:** HR membuka form edit Budi Santoso; dropdown unit dan jabatan dalam keadaan disable/terkunci (`AC-WFC-03`).

> **`FR-HRD-015` — Dokumen Peristiwa Kepegawaian (SK Mutasi/Promosi/Rotasi/Demosi) disetujui utuh (`HRD-DEC-048`, `HRD-DEC-056`, `HRD-DEC-057`)**
>
> Seluruh perubahan unit, jabatan, dan atasan akibat suatu peristiwa kepegawaian diikat dalam satu dokumen SK utuh. Wajib melampirkan berkas scan SK resmi bertandatangan sebelum diajukan. Penyetuju dilarang sama dengan pembuat SK.
>
> **Contoh:** SK Promosi Ns. Rina mengikat pemindahan ke unit ICU, jabatan Ketua Tim, dan atasan Ka. Ruangan ICU. Disetujui utuh oleh HR Manager Dodi. Pembatalan SK hanya diizinkan sebelum tanggal berlaku (`AC-WFC-11`, `AC-WFC-12`, `AC-WFC-13`).

> **`FR-HRD-016` — Tepat satu penempatan utama per jenis per tanggal (`HRD-DEC-049`)**
>
> Pada satu tanggal, seorang pegawai aktif wajib memiliki tepat satu penempatan unit utama. Dua penempatan utama sejenis yang bertabrakan tanggalnya ditolak sistem.
>
> **Contoh:** Pengajuan penempatan utama baru kedua ditolak jika tanggalnya beririsan dengan penempatan utama yang masih aktif (`AC-WFC-04`).

> **`FR-HRD-017` — Berlaku surut dibatasi pada periode kehadiran terbuka (`HRD-DEC-050`)**
>
> Tanggal efektif peristiwa kepegawaian yang berlaku surut hanya diterima jika jatuh pada periode kehadiran yang masih berstatus terbuka.
>
> **Contoh:** SK berlaku surut ke periode November yang sudah ditutup otomatis ditolak sistem (`AC-WFC-05`).

> **`FR-HRD-018` — Data sensitif hanya berubah lewat permohonan berbukti verifikasi orang lain (`HRD-DEC-052`)**
>
> Rekening bank, NIK, NPWP, dan tanggungan dilarang diedit langsung oleh HR di berkas profil. HR dapat mengajukan atas nama pegawai dengan melampirkan berkas bukti resmi. Petugas yang memverifikasi wajib berbeda dari pembuat pengajuan.
>
> **Contoh:** HR Sari mengajukan perubahan rekening atas nama Budi dengan foto buku tabungan. Sari dilarang memverifikasi permohonannya sendiri (`AC-WFC-07`).

> **`FR-HRD-019` — Employee 360 terpadu dengan kontrol otorisasi ketat (`HRD-DEC-053`)**
>
> Menampilkan profil 360 pegawai: HR melihat profil utuh (nominal gaji jika punya izin ViewAmount), sedangkan atasan langsung hanya melihat profil kerja bawahan aktif hari ini (nominal gaji, rekening bank, NIK, dan kasus disiplin disembunyikan total).
>
> **Contoh:** Ka. Ruangan membuka profil perawat ICU bawahannya; data karir dan sertifikasi tampil, gaji dan rekening tersembunyi. Membuka perawat unit lain ditolak 403 Forbidden (`AC-WFC-08`).

> **`FR-HRD-020` — Batch data awal penempatan kepegawaian disetujui sekali (`HRD-DEC-054`)**
>
> Pengisian data awal jabatan, atasan, dan gaji untuk 55 pegawai aktif existing dilakukan melalui satu batch data awal yang divalidasi sistem dan disetujui sekali oleh HR Manager. Setiap pegawai hanya boleh masuk batch sekali seumur hidup.
>
> **Contoh:** Mengunggah batch penempatan 55 pegawai; sistem memvalidasi kelengkapan relasi. Pegawai yang sudah masuk batch disetujui ditolak bila dimasukkan ke batch baru (`AC-WFC-09`).

> **`FR-HRD-019` — Kebijakan gaji memakai empat dimensi yang dapat dikonfigurasi**
>
> Dasar: `HRD-DEC-041`, `HRD-DEC-043`, `HRD-DEC-045`. Golongan, Level, Status kerja, dan Jenjang
> Pendidikan dievaluasi sebagai dimensi yang berdiri sendiri.
>
> **Masa kerja BUKAN faktor pada MVP saat ini** — `HRD-DEC-045` menggantikan `HRD-DEC-042`.
>
> **Contoh berhasil:** kebijakan diubah agar jenjang pendidikan tertentu menghasilkan penyesuaian.
> Perubahan itu dilakukan **sebagai konfigurasi**, tanpa mengubah kode.
>
> **Contoh gagal:** usulan menuliskan persentase kenaikan sebagai percabangan di source ditolak
> pada tinjauan. Begitu pula usulan menambahkan pita masa kerja, yang berada di luar cakupan MVP
> saat ini.

> **`FR-HRD-020` — Jenjang pendidikan hanya dipakai bila terverifikasi**
>
> Dasar: `HRD-DEC-041`.
>
> **Contoh berhasil:** Ani melaporkan ijazah `S2`, melampirkan buktinya, HR memverifikasi, lalu
> evaluasi kebijakan gaji berjalan memakai jenjang barunya.
>
> **Contoh gagal:** jenjang yang dilaporkan tetapi belum diverifikasi **tidak** dipakai evaluasi,
> dan tidak menghasilkan calon penyesuaian.

> **`FR-HRD-021` — Perubahan faktor tidak mengubah gaji secara langsung**
>
> Dasar: `HRD-DEC-043`.
>
> **Contoh berhasil:** pendidikan Ani terverifikasi naik. Sistem membentuk **calon penyesuaian**,
> bukan mengubah gajinya.
>
> **Contoh gagal:** tidak ada satu pun jalur yang mengubah gaji efektif tanpa melewati pengajuan
> dan persetujuan penetapan gaji.

> **`FR-HRD-016` — Slip gaji hanya dapat dilihat pemiliknya**
>
> Dasar: `HRD-DEC-038`. Backend menurunkan kepemilikan dari pengguna yang terautentikasi; pengenal
> dari layar tidak dipercaya.
>
> **Contoh gagal:** Budi Santoso meminta slip gaji Ani Lestari memakai pengenal yang benar.
> Permintaan ditolak, isinya tidak terkirim, dan jawabannya tidak membocorkan apakah slip itu ada.

> **`FR-HRD-017` — Data gaji menuntut konfirmasi kata sandi**
>
> Dasar: `HRD-DEC-038`. Sebelum data gaji ditampilkan, pegawai mengonfirmasi kata sandi akunnya.
> Sesi gaji sensitif berlaku **lima menit**.
>
> **Contoh berhasil:** Ani membuka Slip Gaji, memasukkan kata sandinya, lalu slip tampil.
>
> **Contoh gagal:** enam menit kemudian Ani membuka slip lain; sistem meminta kata sandi lagi.

> **`FR-HRD-018` — Pembacaan gaji meninggalkan jejak audit, tanpa nominalnya**
>
> Dasar: `HRD-DEC-039`. Pembacaan gaji dan slip gaji adalah pengecualian terhadap konvensi yang
> tidak mencatat `GET`.
>
> **Contoh berhasil:** membaca nominal gaji meninggalkan catatan berisi pelaku, sasaran, aksi,
> waktu, dan hasil — **tanpa** nominalnya.
>
> **Contoh gagal:** catatan audit tidak memuat kata sandi, nominal, nomor rekening, nomor pajak,
> isi response, maupun token.

> **`FR-HRD-015` — Empat transaksi penempatan dan remunerasi berdiri sendiri**
>
> Dasar: `HRD-DEC-036`. Perubahan penetapan gaji, penempatan organisasi, penempatan jabatan, dan
> penetapan atasan adalah **empat jenis transaksi terpisah**, masing-masing dengan definisi alur,
> siklus versi konfigurasi, dan jejak audit sendiri.
>
> **Contoh berhasil:** kebijakan persetujuan penetapan gaji diubah menjadi dua tingkat.
> Penempatan organisasi, jabatan, dan atasan **tetap** memakai rantai satu tingkat seperti semula.
>
> **Contoh gagal:** usulan membuat satu definisi alur bersama untuk keempatnya ditolak pada
> tinjauan, karena membuat pertanyaan "siapa menyetujui perubahan yang mana" tidak terjawab dari
> jejak audit.
>
> **Pola awal MVP untuk keempatnya sama:** HR Admin → HR Manager / `CorporateHr`, dengan
> penyetuju berbeda dari pemrakarsa. Pola yang sama **tidak** berarti definisi yang sama.

> **`FR-HRD-014` — Nominal gaji tidak tampil pada daftar lintas pegawai**
>
> Dasar: `HRD-DEC-033`. Response daftar penetapan gaji **tidak menyertakan** nominal.
>
> **Contoh berhasil:** HR Admin membuka daftar penetapan gaji satu unit dan melihat pegawai,
> kelas gaji, dan tanggal berlaku — tanpa satu pun angka rupiah.
>
> **Contoh gagal:** pengguna yang hanya memegang butir baca umum meminta nominal; nilainya tidak
> dikembalikan, bahkan tidak dalam bentuk tersamarkan di dalam payload.

> **`FR-HRD-013` — Nilai gaji tidak masuk log**
>
> Tidak ada nominal gaji yang tertulis di catatan log dalam bentuk apa pun.

### `EPIC HRD-03` — Layanan mandiri pegawai

**Tujuan.** Memberi pegawai satu tempat untuk mencatat kehadiran dan mengajukan apa pun.
**Disposisi backend:** `EXISTING / REUSE` untuk sebagian besar API; `REPAIR` untuk route kehadiran.

> **`FR-HRD-020` — Pegawai hanya melihat datanya sendiri**
>
> Permintaan yang menyentuh data pegawai lain ditolak, tanpa bergantung pada penyaringan di layar.
>
> **Contoh:** Budi Santoso meminta data cuti Ani Lestari lewat pemanggilan langsung; permintaan
> ditolak dengan `403`.

> **`FR-HRD-021` — Angka saldo berasal dari backend**
>
> Angka sisa cuti yang tampil di layar sama persis dengan yang dikembalikan backend, dan layar
> tidak menghitungnya sendiri.
>
> **Contoh:** backend mengembalikan sisa 8,5 hari; layar menampilkan 8,5 hari, bukan hasil
> pengurangan yang dihitung di sisi layar.

> **`FR-HRD-022` — Ambang waktu pencatatan berasal dari backend**
>
> Pegawai baru dapat mencatat kehadiran pulang setelah ambang waktu yang ditetapkan backend
> terlewati.
>
> **Contoh:** backend menetapkan ambang pukul 16.00; pencatatan pukul 15.45 ditolak, pukul 16.05
> berhasil.

> **`FR-HRD-023` — Route layanan mandiri kehadiran mengikuti konvensi**
>
> Route layanan mandiri kehadiran memakai bentuk yang sama dengan layanan mandiri HR lainnya,
> dengan route lamanya tetap hidup.

### `EPIC HRD-04` — Kotak masuk persetujuan terpadu

**Tujuan.** Memberi atasan satu tempat untuk memutuskan seluruh jenis pengajuan.
**Disposisi backend:** `EXTEND` untuk mesin SLA; `MISSING / NEW` untuk seluruh layar.

> **`FR-HRD-030` — Satu kotak masuk melayani seluruh jenis pengajuan**
>
> Satu daftar menampilkan cuti, lembur, koreksi kehadiran, ubah jadwal, tukar shift, dan
> perubahan profil yang ditugaskan kepada penyetuju itu.
>
> **Contoh:** atasan Ani Lestari membuka kotak masuk dan melihat tiga pengajuan berbeda jenis
> dalam satu daftar, tanpa berpindah layar.

> **`FR-HRD-031` — Aturan tiap jenis pengajuan tetap berbeda**
>
> Menolak cuti dan menolak lembur menghasilkan perpindahan status yang berbeda, sesuai jenis
> transaksinya masing-masing.
>
> **Contoh:** penolakan cuti menghasilkan status penolakan milik cuti; penolakan lembur
> menghasilkan status penolakan milik lembur. Keduanya tidak diseragamkan.

> **`FR-HRD-032` — Penyetuju hanya melihat yang ditugaskan kepadanya**
>
> Pengajuan yang tidak ditugaskan kepada seorang penyetuju tidak muncul di kotak masuknya.

> **`FR-HRD-033` — Pengingat terkirim saat batas waktu terlampaui**
>
> Tugas persetujuan yang melewati batas waktunya memicu pengingat, dan hitungan pengingatnya naik.
>
> **Contoh:** tugas dengan batas waktu pukul 17.00 hari Senin belum diputuskan; pada putaran
> pemeriksaan berikutnya pengingat terkirim dan hitungan pengingat menjadi 1.

> **`FR-HRD-034` — Eskalasi berjalan setelah pengingat tidak berhasil**
>
> Tugas yang tetap tidak diputuskan setelah diingatkan dieskalasi ke atasan berikutnya, dan
> jejaknya tercatat.
>
> **Contoh:** setelah dua kali pengingat, tugas muncul di kotak masuk atasan berikutnya, dan
> tugas asalnya menunjukkan bahwa ia pernah dieskalasi.

> **`FR-HRD-035` — Pengajuan tanpa penyetuju tidak hilang**
>
> Pengajuan dari unit yang matriks persetujuannya belum diisi tertahan dan muncul di daftar
> pengawasan HR, bukan gagal diam-diam.

### `EPIC HRD-05` — Administrasi kehadiran

**Tujuan.** Memberi HR dan petugas payroll layar untuk memantau, memperbaiki, dan menutup periode.
**Disposisi backend:** `EXTEND`.

> **`FR-HRD-040` — Rekaman mentah tidak pernah berubah**
>
> Isi rekaman kehadiran mentah tetap sama sebelum dan sesudah koreksi diterapkan.
>
> **Contoh:** rekaman pukul 08.17 tetap tercatat 08.17 setelah koreksi mengubah kehadiran
> hariannya menjadi tepat waktu.

> **`FR-HRD-041` — Status kehadiran harian tidak dapat disunting langsung**
>
> Tidak ada endpoint yang mengubah status kehadiran harian tanpa melalui koreksi dan pemrosesan
> ulang.

> **`FR-HRD-042` — Pengecualian pemblokir menahan penutupan periode**
>
> Penutupan periode ditolak selama masih ada pengecualian pemblokir yang belum selesai, dan
> petugas melihat daftar penghalangnya.
>
> **Contoh:** periode Agustus punya tiga pengecualian pemblokir terbuka; penutupan ditolak dengan
> `409` dan menampilkan ketiganya.

> **`FR-HRD-043` — Permohonan koreksi yang sudah diterapkan tidak dapat turun statusnya**
>
> Sinkronisasi terhadap permohonan yang sudah diterapkan tidak menurunkan statusnya, dan tidak
> memutasi ulang kehadiran hariannya.
>
> **Contoh:** koreksi Ani Lestari sudah diterapkan; menjalankan sinkronisasi sekali lagi tidak
> mengubah status permohonan dan tidak mengubah angka kehadiran hari itu.

> **`FR-HRD-044` — Pengajuan atas nama menyimpan siapa yang mengetiknya**
>
> Permohonan koreksi yang dibuat HR atas nama pegawai menyimpan akun HR sebagai pengetik, dan
> pegawai sebagai pemilik data, beserta alasan mengapa bukan pegawainya yang mengajukan.

> **`FR-HRD-045` — Bekerja di luar jadwal tidak otomatis menjadi lembur**
>
> Pengecualian berjenis bekerja di luar jadwal tidak membentuk permohonan maupun realisasi lembur
> sampai atasan mengklasifikasikannya.
>
> **Contoh:** dr. Rahmawati tercatat bekerja pukul 20.00 di luar jadwalnya; tidak ada permohonan
> lembur yang terbentuk, dan pengecualiannya menunggu klasifikasi atasan.

### `EPIC HRD-06` — Administrasi cuti dan saldo

**Tujuan.** Membuka 93 endpoint cuti yang hari ini tidak dapat dipakai siapa pun.
**Disposisi backend:** `EXTEND`.

> **`FR-HRD-050` — Setiap pergerakan saldo meninggalkan baris buku besar**
>
> Pemotongan, pengembalian, dan penyesuaian saldo masing-masing membentuk satu baris buku besar
> yang dapat ditelusuri.
>
> **Contoh:** cuti tiga hari Ani Lestari memotong saldo 3,0 hari, dan buku besarnya bertambah
> tepat satu baris bernilai −3,0.

> **`FR-HRD-051` — Saldo tidak dapat diubah tanpa buku besar**
>
> Tidak ada jalur yang mengubah angka saldo tanpa membentuk baris buku besar yang bersesuaian.

> **`FR-HRD-052` — Pembalikan pelaksanaan cuti wajib beralasan**
>
> Pembalikan pelaksanaan cuti ditolak bila alasannya tidak diisi, dan bila berhasil, ia menyimpan
> siapa yang membalik beserta waktunya.

> **`FR-HRD-053` — Pengakuan pegawai atas penarikan bukan penghalang**
>
> Penarikan pegawai dari cuti tetap dapat diterapkan meski pegawai belum mengakui pemberitahuan,
> selama alasan pelewatannya tercatat.
>
> **Contoh:** unit kekurangan tenaga; atasan menarik Budi Santoso dari cutinya sebelum Budi
> membaca pemberitahuan, dengan alasan pelewatan tercatat. Sisa dua hari kembali ke saldonya.

> **`FR-HRD-054` — Pembatalan cuti mengembalikan saldo**
>
> Pembatalan yang disetujui mengembalikan saldo penuh atau sebagian sesuai kebijakan, dan
> pengembaliannya tercatat di buku besar.

### `EPIC HRD-07` — Administrasi lembur

**Tujuan.** Membuka 78 endpoint lembur yang hari ini tidak dapat dipakai siapa pun.
**Disposisi backend:** `EXTEND`.

> **`FR-HRD-060` — Realisasi lembur dibuktikan data kehadiran**
>
> Realisasi tidak terbentuk untuk hari yang tidak punya kehadiran tercatat.
>
> **Contoh:** Budi Santoso mengajukan lembur Selasa tetapi tidak mencatat kehadiran hari itu;
> realisasinya tidak terbentuk sampai kehadirannya dikoreksi.

> **`FR-HRD-061` — Jam yang dibayar tidak melebihi yang tercatat**
>
> Bila permohonan menyebut empat jam sementara kehadiran mendukung tiga jam, realisasinya bernilai
> tiga jam.

> **`FR-HRD-062` — Cuti pengganti terbit hanya dari realisasi terverifikasi**
>
> Hak cuti pengganti tidak terbit dari realisasi yang belum diverifikasi.

### `EPIC HRD-08` — Penjadwalan kerja

**Tujuan.** Menghidupkan mesin penjadwalan bagi rumah sakit yang berjalan 24 jam.
**Disposisi backend:** `EXTEND` — skemanya sudah ada, perilakunya belum.

> **`FR-HRD-070` — Roster diperiksa terhadap bentrok sebelum terbit**
>
> Penerbitan roster ditolak selama masih ada bentrok yang menghalangi, dan manajer melihat daftar
> bentroknya.
>
> **Contoh:** roster September menempatkan Ani Lestari pada hari ia sedang cuti; penerbitan
> ditolak dan bentrok cuti ditampilkan.

> **`FR-HRD-071` — Penetapan manual wajib beralasan**
>
> Manajer dapat menetapkan penempatan meski ada bentrok yang tercatat, tetapi hanya dengan alasan
> yang diisi.

> **`FR-HRD-072` — Bentrok lisensi dan kewenangan klinis tidak dapat dilewati**
>
> Penetapan manual tidak tersedia untuk bentrok lisensi maupun kewenangan klinis.
>
> **Contoh:** penempatan yang menimbulkan bentrok kewenangan klinis tetap ditolak walaupun manajer
> mengisi alasan.

> **`FR-HRD-073` — Jadwal berlaku surut tidak dapat disunting langsung**
>
> Penyuntingan jadwal pada tanggal yang kehadirannya sudah diproses ditolak.

> **`FR-HRD-074` — Tukar shift memerlukan persetujuan rekan**
>
> Permohonan tukar shift tidak sampai ke atasan sebelum rekan yang diminta menyetujuinya.

> **`FR-HRD-075` — Pertukaran diterapkan utuh atau tidak sama sekali**
>
> Bila salah satu sisi bentrok, tidak ada jadwal yang berubah.
>
> **Contoh:** Ani dan Budi bertukar shift; sisi Budi ternyata bentrok. Jadwal keduanya tetap
> seperti semula, bukan hanya sisi Ani yang berubah.

> **`FR-HRD-076` — Jadwal terbit menjadi acuan pengolahan kehadiran**
>
> Kehadiran pada tanggal berjadwal diolah memakai shift yang terbit untuk pegawai itu.

### `EPIC HRD-09` — Kesiapan payroll sisi HR

**Tujuan.** Menghasilkan **masukan HR yang siap payroll**, dan berhenti di situ.
**Disposisi backend:** `REUSE WITH ADAPTER`.

**Batas epic ini setelah `HRD-DEC-035`.** Orkestrasi putaran payroll **keluar** dari MVP.

| Tetap di dalam `EPIC HRD-09` | Pindah ke `POST-MVP` |
| --- | --- |
| Kesiapan kehadiran untuk payroll | Pembuatan `TrxPayrollRun` |
| Masukan dan kesiapan cuti | Pemajuan status putaran payroll |
| Masukan dan kesiapan lembur | Perhitungan payroll |
| Rekonsiliasi sisi HR | Persetujuan putaran payroll |
| Validasi bahwa data HR siap diserahkan | Serah terima final ke Finance |

`Payroll Executed` **MUST NOT** dibaca sebagai `Employee Paid`.

> **`FR-HRD-080` — Payroll memakai kehadiran final**
>
> Putaran payroll ditolak berjalan selama periode kehadirannya belum ditutup.

> **`FR-HRD-081` — Angka payroll dapat ditelusuri ke kehadiran**
>
> Angka masukan payroll sama persis dengan kehadiran harian pada periode yang sudah ditutup.
>
> **Contoh:** total jam kerja Ani Lestari pada masukan payroll Agustus sama dengan jumlah jam
> pada kehadiran hariannya bulan itu.

> **`FR-HRD-082` — Serah terima yang diulang tidak menghasilkan pengiriman ganda**
>
> Menjalankan serah terima dua kali menghasilkan satu pengiriman.

> **`FR-HRD-083` — HR tidak menyimpan hasil pembayaran, jurnal, atau pajak**
>
> Tidak ada endpoint HR yang menyimpan ketiganya.

### `EPIC HRD-10` — Jaring pengaman pengujian

**Tujuan.** Memberi 1.343 endpoint HR bukti bahwa perubahan tidak merusak yang sudah berjalan.
**Disposisi backend:** `MISSING / NEW`.

> **`FR-HRD-090` — Setiap epic `MUST HAVE` punya test yang lulus**
>
> Setiap epic pada dokumen ini punya sekurang-kurangnya satu test otomatis yang lulus, sesuai
> baris pada `testing/acceptance-test-matrix.md`.

> **`FR-HRD-091` — Kolom sensitif tidak masuk log**
>
> Memicu jalur yang menyentuh kolom bertanda sensitif tidak meninggalkan isinya di catatan log.

### Epic yang TIDAK masuk gelombang pengiriman mana pun

| Epic | Disposisi | Sebabnya |
| --- | --- | --- |
| Izin pulang cepat — dampak saldo dan pembayaran | **`OPEN DECISION`** | `HRD-Q-47`. Bentuk alurnya sudah dirancang, tetapi nilai kebijakannya belum ditetapkan pemiliknya |
| Hubungan karyawan dan kedisiplinan | **`OPEN DECISION`** | `HRD-Q-51` dan `HRD-Q-52` |
| Kredensial, kewenangan klinis, OPPE, FPPE | **`OPEN DECISION`** | `HRD-Q-08`. Menunggu Komite Medik |
| Kesehatan dan keselamatan kerja staf | **`OPEN DECISION`** | `HRD-DEC-010` masih `draft`. Menunggu K3RS |
| Perencanaan tenaga kerja, rekrutmen, benefit, layanan HR, perjalanan dinas | **`OPEN DECISION`** | `HRD-Q-05`. Isi tabelnya belum diketahui |
| Bentuk serah terima ke Finance | **`OPEN DECISION`** | `HRD-Q-10` dan `HRD-Q-11` |

**Tidak satu pun dari keenam baris di atas boleh masuk gelombang pengiriman sebelum keputusannya
turun.**

---

## 11. Model status yang diusulkan

Daftar lengkapnya ada di `contracts/state-transition-matrix.md`. Yang disebut di sini hanya
invariant yang menentukan batas MVP.

| Kelompok status | Invariant utama |
| --- | --- |
| Periode kehadiran | `Closed` tidak dapat kembali ke `Open`. Satu-satunya jalan kembali adalah `Reopened`, yang meninggalkan jejak bahwa periode pernah ditutup |
| Kehadiran harian | Bukan state machine. Nilainya **hasil hitung**, dan tidak boleh disunting langsung |
| Rekaman mentah | Isinya tidak pernah berubah; hanya status pemrosesannya yang berpindah |
| Permohonan koreksi | `Applied` bersifat terminal terhadap sinkronisasi normal. Perbaikan setelahnya lewat permohonan baru |
| Pengecualian kehadiran | Pengecualian pemblokir yang masih terbuka menahan penutupan periode |
| Buku besar saldo cuti | Transaksi tidak dihapus. Pembalikan dilakukan dengan transaksi baru yang menunjuk transaksi asalnya |
| Tugas persetujuan | Lapisan routing dan keputusan; **bukan** pengganti status domain. Cuti tetap punya statusnya sendiri, lembur tetap punya statusnya sendiri |
| Putaran payroll | Tidak ada perpindahan yang boleh dirancang melewati serah terima |

---

## 12. Sasaran arsitektur

| Yang dipakai ulang apa adanya | Yang diperluas | Yang baru |
| --- | --- | --- |
| Seluruh 104 tabel master HR | Enam tabel yang mendapat kolom tambahan: pengecualian kehadiran, permohonan koreksi, pelaksanaan cuti, penarikan cuti, tugas persetujuan, penempatan shift | **Tidak ada tabel baru** |
| `MstWorkforceProfile` sebagai identitas tunggal | Tujuh tabel roster yang mendapat controller, service, dan DTO tanpa perubahan skema | Seluruh layar administrasi kehadiran, cuti, lembur, penjadwalan |
| Mesin persetujuan bersama | Mesin SLA, pengingat, dan eskalasi di atas mesin persetujuan yang sudah ada | Seluruh layar atasan dan kotak masuk persetujuan |
| Penjagaan hak akses dan jejak audit | — | Enam halaman Administrasi Kepegawaian yang menutup cacat menu |

**Tidak ada entity baru pada MVP ini.** Seluruh kemampuan target dipenuhi dengan penambahan kolom
dan penambahan perilaku terhadap 337 tabel yang sudah ada. Ini bukan kebetulan, melainkan hasil
tabel kepemilikan data pada `02-backend-architecture.md` bagian 3.

---

## 13. Sasaran kemampuan API

Seluruh endpoint di bawah adalah **bagian dari** `contracts/api-contract.md`, tidak melebihinya.
Endpoint yang belum ada di kode diberi label `Rencana (belum tersedia)`.

### Corporate / Human Resource / Attendance Management / Attendance Period

Base URL: `api/v1/corporate/human-resource/attendance/periods`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/{id:guid}/close-preview` | Melihat apa saja yang masih menghalangi penutupan | `AttendancePeriod : Read` | — | `ApiResponse<ClosePreviewResponse>` | `EPIC HRD-05` | Sudah ada |
| `POST` | `/{id:guid}/close` | Menutup periode | `AttendancePeriod : Close` | `CloseAttendancePeriodRequest` | `ApiResponse<PeriodDetailResponse>` | `EPIC HRD-05` | Sudah ada |
| `POST` | `/{id:guid}/reopen` | Membuka kembali periode yang sudah ditutup | `AttendancePeriod : Reopen` | `ReopenAttendancePeriodRequest` | `ApiResponse<PeriodDetailResponse>` | `EPIC HRD-05` | Sudah ada |

### Corporate / Human Resource / Attendance Management / Attendance Exception Classification

Base URL: `api/v1/corporate/human-resource/attendance/exception-classifications`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/pending` | Daftar pengecualian kerja di luar jadwal yang menunggu keputusan atasan | `AttendanceException : Read` | Query | `ApiResponse<PagedResult<ExceptionResponse>>` | `EPIC HRD-05` | **Rencana (belum tersedia)** |
| `POST` | `/{exceptionId:guid}/classify` | Menetapkan klasifikasi akhir | `AttendanceException : Classify` | `ClassifyExceptionRequest` | `ApiResponse<ExceptionResponse>` | `EPIC HRD-05` | **Rencana (belum tersedia)** |

### Corporate / Human Resource / Attendance Management / Attendance Payroll Handoff

Base URL: `api/v1/corporate/human-resource/attendance/payroll-handoff`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/{id:guid}/execute` | Menjalankan serah terima ke payroll | `AttendancePayrollHandoff : Execute` | `ExecuteHandoffRequest` | `ApiResponse<HandoffResponse>` | `EPIC HRD-09` | Sudah ada |
| `POST` | `/{id:guid}/rollback` | Membatalkan serah terima yang gagal | `AttendancePayrollHandoff : Rollback` | `RollbackHandoffRequest` | `ApiResponse<HandoffResponse>` | `EPIC HRD-09` | Sudah ada |

### Corporate / Human Resource / Workflow Management / Approval Inbox

Base URL: `api/v1/corporate/human-resource/approval-inbox`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar pengajuan yang ditugaskan kepada penyetuju ini, lintas jenis transaksi | `ApprovalInbox : Read` | Query | `ApiResponse<PagedResult<InboxItemResponse>>` | `EPIC HRD-04` | Sudah ada |

### Corporate / Human Resource / Scheduling Management / Roster Period

Base URL: `api/v1/corporate/human-resource/scheduling-management/roster-periods`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Seluruh grup | — | Mengelola periode roster, penempatan, dan penerbitan jadwal | Sesuai `contracts/api-contract.md` | — | — | `EPIC HRD-08` | **Rencana (belum tersedia)** |

### Grup lain

Grup cuti (`leave/balances`, `leave/adjustments`, `leave/executions`, `leave/recalls`), lembur
(`overtime-management/plans`, `overtime-management/realizations`), koreksi kehadiran
(`attendance/correction-requests`, `attendance/correction-monitoring`), dan penjadwalan
(`scheduling-management/schedule-change-requests`, `scheduling-management/shift-swap-requests`)
seluruhnya tercantum lengkap pada `contracts/api-contract.md`. Daftar di dokumen ini **tidak**
menambah satu pun endpoint di luar berkas itu.

---

## 14. Matriks kewenangan

String hak akses ditulis persis seperti pada `contracts/api-contract.md` kolom **Hak akses**.

| Peran | Boleh melakukan | Hak akses |
| --- | --- | --- |
| Pegawai | Mencatat dan melihat kehadirannya sendiri | Diturunkan dari konteks pengguna terautentikasi, bukan dari butir hak akses administratif |
| Pegawai | Mengajukan cuti, lembur, koreksi, ubah jadwal, tukar shift | Sama |
| Atasan | Memutuskan pengajuan yang ditugaskan kepadanya | `ApprovalInbox : Read` ditambah butir per jenis transaksi |
| Atasan | Mengklasifikasikan pengecualian bekerja di luar jadwal | `AttendanceException : Classify` |
| HR Admin | Membaca kehadiran harian seluruh pegawai | `AttendanceDaily : Read` |
| HR Admin | Membaca dan mengelola rekaman mentah | `AttendanceRawLog : Read`, `AttendanceRawLog : Update` |
| HR Admin | Menerapkan koreksi yang sudah disetujui | `AttendanceCorrection : Apply` |
| HR Admin | Menjalankan pemrosesan ulang | `AttendanceProcessing : Update` |
| Petugas payroll | Membuka periode kehadiran | `AttendancePeriod : Create` |
| Petugas payroll | Menutup periode | `AttendancePeriod : Close` |
| Petugas payroll | Membuka kembali periode | `AttendancePeriod : Reopen` — **pemetaan perannya masih `[OPEN]`**, `HRD-Q-23` |
| Petugas payroll | Membatalkan periode | `AttendancePeriod : Cancel` |
| Petugas payroll | Menjalankan serah terima | `AttendancePayrollHandoff : Execute` |
| Petugas payroll | Membatalkan serah terima yang gagal | `AttendancePayrollHandoff : Rollback` |

**Aturan yang mengikat matriks ini:** hak akses yang menjaga sebuah tombol di layar **MUST** sama
persis dengan hak akses yang menjaga endpoint yang dipanggilnya. Perbedaan di antara keduanya
adalah cacat keamanan, bukan ketidakrapian.

---

## 15. Batas integrasi dan billing

| Yang **MUST NOT** dibuat sendiri oleh modul HR | Pemiliknya | Alasan |
| --- | --- | --- |
| Tabel akun aplikasi, role, atau permission | Administrator / Identity | HR hanya mengirim permintaan buat dan cabut akses. Bentuk kontraknya `[OPEN]` — `HRD-DEP-003` |
| Tabel pembayaran, jurnal akuntansi, pajak, dan pelaporan | Finance | `HRD-DEC-009` menghentikan tanggung jawab HR setelah serah terima |
| Jadwal praktik dokter untuk pendaftaran pasien | Health Services | `HRD-DEC-006` memisahkan jadwal kerja dari jadwal praktik |
| Data klinis pasien dan volume layanan | Health Services | Sumber angka OPPE ada di sana, tetapi OPPE sendiri `BLOCKED` |
| Penyimpanan berkas fisik | Shared platform | HR hanya menyimpan metadata dan rujukan path. Kontraknya `[OPEN]` — `HRD-DEP-006` |
| Salinan data pasien dalam bentuk apa pun | Patient Management | HR tidak memerlukannya sama sekali |

**Dampak billing MVP ini: tidak ada.** Modul HR tidak menyentuh tagihan pasien, dan tidak
menghasilkan satu pun angka yang masuk ke billing.

---

## 16. Guardrail regulasi

| Kewajiban | Bagaimana MVP memenuhinya |
| --- | --- |
| Jejak jam kerja yang dapat diperiksa | Rekaman kehadiran mentah tidak pernah diubah; koreksi memutasi hasil olahannya dan meninggalkan jejak siapa serta kapan |
| Perlindungan data pribadi pegawai | Kolom bertanda sensitif pada `data/data-dictionary.md` tidak masuk log dan tidak dipakai sebagai contoh berisi data asli |
| Kerahasiaan data gaji | Nilai gaji hanya terbaca pemilik data dan HR yang berwenang |
| Kerahasiaan rekam kesehatan kerja | **Tidak dijamin MVP ini**, karena kemampuannya `BLOCKED` menunggu K3RS. Tidak ada layar maupun endpoint baru yang menyentuhnya |
| Batas kewenangan praktik klinis | **Tidak ditetapkan MVP ini.** `S-C1` `BLOCKED` menunggu Komite Medik |
| Jejak keputusan yang menyangkut nasib pegawai | Tindakan kedisiplinan berada di luar MVP karena pemisahan perannya belum diputuskan — `HRD-Q-51` |

**Ketiga baris "tidak dijamin" di atas bukan kelalaian.** Menjaminnya berarti menetapkan aturan
atas nama pihak yang belum memberikan wewenangnya.

---

## 17. Kebutuhan non-fungsional

| ID | Kebutuhan | Bagaimana dibuktikan |
| --- | --- | --- |
| `NFR-001` | **Keutuhan transaksi.** Penerapan koreksi, pelaksanaan cuti, dan pertukaran shift berhasil seluruhnya atau gagal seluruhnya | Test yang memutus proses di tengah lalu memeriksa bahwa tidak ada perubahan sebagian |
| `NFR-002` | **Ketahanan terhadap dua petugas yang bekerja bersamaan.** Dua petugas yang mengubah data yang sama tidak saling menimpa tanpa peringatan | Test dua permintaan hampir bersamaan; satu berhasil, satu ditolak dengan pesan yang terbaca |
| `NFR-003` | **Ketahanan terhadap pengiriman ganda.** Menjalankan serah terima payroll dua kali menghasilkan satu pengiriman | Test yang menghitung jumlah pengiriman |
| `NFR-004` | **Jejak audit.** Setiap permintaan yang mengubah data meninggalkan catatan siapa, kapan, dan apa yang diubah; pembacaan tidak dicatat | Isi catatan audit yang diperiksa |
| `NFR-005` | **Otorisasi konsisten.** Hak akses pada tombol layar sama persis dengan hak akses pada endpoint yang dipanggilnya | Perbandingan `contracts/api-contract.md` dengan skema fitur pada `03-frontend-architecture.md` |
| `NFR-006` | **Koreksi tidak merusak masa lalu.** Perubahan berlaku surut ke periode tertutup ditolak | Test penolakan |
| `NFR-007` | **Penanganan waktu.** Seluruh kolom waktu disimpan dengan zona waktu, dan ambang waktu pencatatan berasal dari backend | Test yang membandingkan nilai tersimpan dengan zona waktunya |
| `NFR-008` | **Penghapusan bersifat penandaan.** Tidak ada baris yang benar-benar dihapus dari basis data | Test yang membuktikan baris masih ada dengan penanda hapus |
| `NFR-009` | **Kerahasiaan.** Kolom bertanda sensitif tidak masuk custom logger dalam bentuk apa pun | Isi log yang diperiksa |

---

## 18. Skenario UAT

Seluruh nama pada skenario di bawah adalah **nama samaran**. Tidak ada data pegawai asli.

> **`UAT-01` — Satu periode kerja penuh berjalan di dalam sistem**
>
> **Kondisi awal:** unit percontohan punya master terisi, lima pegawai aktif, dan periode
> kehadiran bulan berjalan sudah dibuka.
>
> **Langkah:** manajer menerbitkan roster; kelima pegawai mencatat kehadiran selama satu bulan;
> dua di antaranya mengajukan cuti dan lembur; atasan memutuskan seluruh pengajuan dari kotak
> masuk; petugas payroll menutup periode lalu menjalankan serah terima.
>
> **Hasil yang diharapkan:** periode berstatus tertutup, serah terima terlaksana, dan total jam
> kerja setiap pegawai dapat ditelusuri sampai ke rekaman kehadirannya.

> **`UAT-02` — Penutupan periode ditolak karena masih ada penghalang**
>
> **Kondisi awal:** periode punya tiga pengecualian pemblokir yang masih terbuka.
>
> **Langkah:** petugas payroll mencoba menutup periode.
>
> **Hasil yang diharapkan:** penutupan ditolak, dan layar menampilkan ketiga pengecualian
> beserta pegawai dan tanggalnya. Periode tetap terbuka.

> **`UAT-03` — Koreksi kehadiran memperbaiki hari yang keliru**
>
> **Kondisi awal:** Ani Lestari lupa mencatat kehadiran pulang pada 12 Agustus; hari itu punya
> pengecualian terbuka.
>
> **Langkah:** Ani mengajukan koreksi beserta alasan; atasannya menyetujui dari kotak masuk; HR
> menerapkannya.
>
> **Hasil yang diharapkan:** kehadiran 12 Agustus berubah menjadi lengkap, pengecualiannya
> tertutup, dan **rekaman mentah pukul 08.17 tetap tercatat 08.17**.

> **`UAT-04` — Koreksi yang sudah diterapkan tidak dapat berjalan dua kali**
>
> **Kondisi awal:** koreksi `UAT-03` sudah diterapkan.
>
> **Langkah:** jalankan sinkronisasi persetujuan sekali lagi terhadap permohonan itu.
>
> **Hasil yang diharapkan:** status permohonan tetap sebagai sudah diterapkan, dan angka
> kehadiran 12 Agustus **tidak berubah**.

> **`UAT-05` — Permohonan cuti melebihi saldo ditolak**
>
> **Kondisi awal:** Budi Santoso punya sisa cuti 2 hari.
>
> **Langkah:** Budi mengajukan cuti 5 hari.
>
> **Hasil yang diharapkan:** permohonan ditolak sebelum sampai ke atasan, dengan pesan yang
> menyebut sisa saldonya. Tidak ada baris buku besar yang terbentuk.

> **`UAT-06` — Penarikan dari cuti tanpa menunggu pengakuan pegawai**
>
> **Kondisi awal:** Budi Santoso sedang menjalani cuti 5 hari, baru berjalan 3 hari; unitnya
> kekurangan tenaga.
>
> **Langkah:** atasan mengajukan penarikan, melewati pengakuan Budi dengan alasan tercatat, lalu
> menerapkannya.
>
> **Hasil yang diharapkan:** cuti Budi berstatus tertarik, sisa 2 hari kembali ke saldonya lewat
> satu baris buku besar, dan alasan pelewatan pengakuan tercatat beserta siapa yang melewatinya.

> **`UAT-07` — Lembur tanpa bukti kehadiran tidak menghasilkan realisasi**
>
> **Kondisi awal:** permohonan lembur Budi untuk 20 Agustus sudah disetujui, tetapi Budi tidak
> mencatat kehadiran hari itu.
>
> **Langkah:** jalankan pembentukan realisasi.
>
> **Hasil yang diharapkan:** realisasi **tidak** terbentuk. Setelah kehadiran 20 Agustus
> dikoreksi, pembentukan realisasi berhasil.

> **`UAT-08` — Dua petugas menyetujui pengajuan yang sama**
>
> **Kondisi awal:** satu permohonan cuti ditugaskan kepada satu penyetuju yang membukanya di dua
> perangkat.
>
> **Langkah:** kedua perangkat menekan setujui pada waktu hampir bersamaan.
>
> **Hasil yang diharapkan:** satu keputusan tersimpan, satu ditolak dengan pesan yang terbaca
> pengguna. Tidak ada keputusan ganda di riwayat pengajuan.

> **`UAT-09` — Tukar shift gagal pada satu sisi**
>
> **Kondisi awal:** Ani dan Budi mengajukan tukar shift; jadwal baru Budi bentrok dengan
> pelatihan wajibnya.
>
> **Langkah:** rekan menyetujui, atasan menyetujui, sistem menerapkan.
>
> **Hasil yang diharapkan:** penerapan ditolak, dan **jadwal keduanya tetap seperti semula** —
> bukan hanya jadwal Ani yang berubah.

> **`UAT-10` — Pegawai membuka data pegawai lain**
>
> **Kondisi awal:** Budi Santoso masuk sebagai pegawai biasa.
>
> **Langkah:** Budi memanggil langsung data cuti Ani Lestari.
>
> **Hasil yang diharapkan:** permintaan ditolak. Isi data Ani tidak terkirim sama sekali,
> termasuk dalam jawaban kesalahan.

> **`UAT-11` — Dokter bekerja di luar jadwal**
>
> **Kondisi awal:** dr. Rahmawati punya jadwal 08.00–14.00, tetapi tercatat bekerja pukul 20.00.
>
> **Langkah:** jalankan pengolahan kehadiran hari itu.
>
> **Hasil yang diharapkan:** terbentuk pengecualian berjenis bekerja di luar jadwal yang menunggu
> klasifikasi atasan. **Tidak ada permohonan maupun realisasi lembur yang terbentuk otomatis.**

> **`UAT-12` — Pengajuan dari unit tanpa matriks persetujuan**
>
> **Kondisi awal:** satu unit belum punya matriks persetujuan.
>
> **Langkah:** pegawai unit itu mengajukan cuti.
>
> **Hasil yang diharapkan:** pengajuan tertahan dan muncul di daftar pengawasan HR beserta
> sebabnya. Pengajuan **tidak** hilang diam-diam.

> **`UAT-13` — Enam butir menu Administrasi Kepegawaian**
>
> **Kondisi awal:** pengguna dengan hak akses administrasi kepegawaian.
>
> **Langkah:** buka keenam butir menu berturut-turut.
>
> **Hasil yang diharapkan:** keenamnya menampilkan halaman berisi, bukan halaman kosong.

> **`UAT-14` — Perubahan gaji tanpa persetujuan pejabat**
>
> **Kondisi awal:** permohonan perubahan gaji Ani Lestari sudah diverifikasi HR.
>
> **Langkah:** HR mencoba menerapkannya tanpa persetujuan pejabat berwenang.
>
> **Hasil yang diharapkan:** penerapan ditolak, dan nilai gaji Ani tidak berubah.

> **`UAT-15` — HR Admin mencoba menyetujui perubahan gaji yang ia ajukan sendiri**
>
> **Kondisi awal:** HR Admin Sari mengajukan perubahan gaji Budi Santoso. Unitnya hanya punya
> satu petugas HR.
>
> **Langkah:** Sari membuka pengajuan itu lalu menekan setujui.
>
> **Hasil yang diharapkan:** permintaan ditolak. Nilai gaji Budi **tidak** berubah. Pengajuan
> diteruskan ke otoritas di atasnya, **bukan** disetujui otomatis karena unit kekurangan personel.

> **`UAT-16` — Nominal gaji pada daftar lintas pegawai**
>
> **Kondisi awal:** HR Admin membuka daftar penetapan gaji satu unit berisi 20 pegawai.
>
> **Langkah:** buka layar daftar, lalu periksa isi jawaban jaringan yang diterima peramban.
>
> **Hasil yang diharapkan:** layar menampilkan pegawai, kelas gaji, dan tanggal berlaku tanpa
> satu pun angka rupiah — dan **isi jawaban jaringan juga tidak memuat nominal**, bukan sekadar
> menyembunyikannya di layar.

> **`UAT-17` — Pegawai mencoba membuka slip gaji rekannya**
>
> **Kondisi awal:** Budi Santoso masuk sebagai pegawai dan mengetahui pengenal slip gaji Ani.
>
> **Langkah:** Budi memanggil langsung slip gaji Ani.
>
> **Hasil yang diharapkan:** ditolak. Isi slip tidak terkirim sama sekali, dan jawabannya tidak
> memberi tahu apakah slip dengan pengenal itu ada. Percobaan ini tercatat di jejak audit.

> **`UAT-18` — Layar gaji ditinggalkan terbuka**
>
> **Kondisi awal:** Ani sudah mengonfirmasi kata sandi dan membuka slip gajinya.
>
> **Langkah:** layar dibiarkan terbuka lebih dari lima menit, lalu Ani membuka slip periode lain.
>
> **Hasil yang diharapkan:** sistem meminta konfirmasi kata sandi lagi sebelum menampilkannya.

> **`UAT-19` — Jejak audit tidak memuat nominal**
>
> **Kondisi awal:** seorang HR Manager membaca nominal gaji beberapa pegawai.
>
> **Langkah:** periksa catatan audit yang dihasilkan.
>
> **Hasil yang diharapkan:** setiap pembacaan tercatat, dan **tidak satu pun** catatan memuat
> angka gajinya.

> **`UAT-20` — Form Master Data mengunci unit dan jabatan (`AC-WFC-03`)**
>
> **Kondisi awal:** Pegawai aktif Budi Santoso sudah memiliki penempatan unit dan jabatan.
>
> **Langkah:** HR Admin membuka form edit master data pegawai Budi dan mencoba mengubah unit kerja.
>
> **Hasil yang diharapkan:** dropdown unit dan jabatan dalam keadaan terkunci (disabled) dan API menolak pengubahan langsung dengan kode 422.

> **`UAT-21` — Penolakan dua penempatan unit utama yang bertabrakan (`AC-WFC-04`)**
>
> **Kondisi awal:** Pegawai memiliki penempatan unit utama di Rawat Inap (1 Jan 2025 - selesai).
>
> **Langkah:** HR Admin mengajukan penempatan unit utama kedua di ICU mulai 1 Juni 2026 tanpa menutup penempatan Rawat Inap.
>
> **Hasil yang diharapkan:** sistem menolak pengajuan dengan pesan bahwa penempatan utama tidak boleh bertumpuk (kode 409).

> **`UAT-22` — Penolakan SK berlaku surut ke periode kehadiran tertutup (`AC-WFC-05`)**
>
> **Kondisi awal:** Periode kehadiran November sudah berstatus Closed; periode Desember Open.
>
> **Langkah:** HR Admin mengajukan SK mutasi berlaku efektif 15 November.
>
> **Hasil yang diharapkan:** sistem menolak pengajuan karena tanggal efektif jatuh pada periode yang sudah ditutup (kode 409).

> **`UAT-23` — Satu nomor NIK tidak membuat profil ganda (`AC-WFC-06`)**
>
> **Kondisi awal:** dr. Hendra terdaftar di modul dokter dengan NIK 3171xxx dan memiliki 1 profil workforce.
>
> **Langkah:** HR Admin mendaftarkan Pegawai baru dengan NIK yang sama.
>
> **Hasil yang diharapkan:** sistem tidak menambah baris profil workforce baru, melainkan menautkan peran Pegawai ke profil dr. Hendra yang sudah ada.

> **`UAT-24` — Rekening bank dilarang diedit langsung dan verifikator wajib berbeda (`AC-WFC-07`)**
>
> **Kondisi awal:** HR Admin Sari menerima permintaan ganti rekening dari pegawai.
>
> **Langkah:** Sari mencoba mengubah nomor rekening di profil pegawai (ditolak). Sari membuat permohonan atas nama pegawai melampirkan foto buku tabungan, lalu mencoba memverifikasi sendiri.
>
> **Hasil yang diharapkan:** verifikasi mandiri ditolak 403 Forbidden. Permohonan hanya dapat disetujui oleh petugas HR lain (Dodi).

> **`UAT-25` — Pembatasan tampilan Employee 360 bagi Atasan Langsung (`AC-WFC-08`)**
>
> **Kondisi awal:** Ka. Ruangan ICU login ke sistem. Ns. Rina adalah bawahan langsung aktif hari ini; Ns. Siti adalah perawat IGD (bukan bawahan).
>
> **Langkah:** Ka. Ruangan membuka Employee 360 Rina, lalu mencoba membuka Employee 360 Siti.
>
> **Hasil yang diharapkan:** Profil Rina menampilkan data kerja, kontak darurat, dan riwayat mutasi tanpa nominal gaji, rekening bank, NIK, dan kasus disiplin. Membuka profil Siti ditolak HTTP 403 Forbidden.

> **`UAT-26` — Batch data awal penempatan disetujui sekali (`AC-WFC-09`)**
>
> **Kondisi awal:** 55 pegawai aktif diunggah dalam Batch Data Awal #1 dan disetujui HR Manager.
>
> **Langkah:** HR Admin mencoba memasukkan kembali salah satu pegawai ke Batch Data Awal #2.
>
> **Hasil yang diharapkan:** validasi sistem menolak baris tersebut karena pegawai sudah pernah masuk batch baseline yang disetujui (aturan once-per-employee).

> **`UAT-27` — Penerapan SK utuh pada tanggal berlaku (`AC-WFC-11`)**
>
> **Kondisi awal:** SK Promosi dan Mutasi Rina ke ICU disetujui berstatus Scheduled dengan tanggal berlaku 1 Desember 2026.
>
> **Langkah:** sistem menjalankan job terjadwal pada 1 Desember 2026.
>
> **Hasil yang diharapkan:** penempatan unit lama di Rawat Inap ditutup per 30 November, penempatan ICU aktif per 1 Desember, jabatan Ketua Tim aktif, dan satu baris riwayat kepegawaian tercatat merujuk nomor SK tersebut.

> **`UAT-28` — Wajib scan SK dan larangan pembatalan SK berlaku (`AC-WFC-12`, `AC-WFC-13`)**
>
> **Kondisi awal:** HR Admin membuat draft SK promosi.
>
> **Langkah:** HR Admin mencoba mengajukan SK tanpa berkas scan SK resmi (ditolak). Setelah scan diunggah dan disetujui hingga berstatus Berlaku, HR mencoba membatalkan SK tersebut.
>
> **Hasil yang diharapkan:** pengajuan tanpa scan ditolak 400. Pembatalan SK yang sudah berstatus Berlaku ditolak 409; sistem mewajibkan penerbitan SK baru.

---

## 19. Definition of Done

Setiap butir dijawab "ya" atau "belum", dan setiap jawaban menyebut buktinya.

| Butir | Bukti |
| --- | --- |
| Satu unit dapat menjalankan satu periode kerja penuh dari jadwal terbit sampai serah terima | `UAT-01` |
| Periode tidak dapat ditutup selama masih ada pengecualian pemblokir | `UAT-02`, `AT-HRD-B1-05` |
| Rekaman kehadiran mentah tidak pernah berubah isinya | `UAT-03`, `AT-HRD-B1-02` |
| Koreksi yang sudah diterapkan tidak dapat berjalan dua kali | `UAT-04`, `AT-HRD-B1-09` |
| Setiap pergerakan saldo cuti meninggalkan baris buku besar | `UAT-05`, `UAT-06`, `AT-HRD-B2-02` |
| Lembur tidak dibayar tanpa bukti kehadiran | `UAT-07`, `AT-HRD-B3-02` |
| Dua petugas tidak dapat menghasilkan keputusan ganda | `UAT-08`, `NFR-002` |
| Tukar shift tidak dapat diterapkan sebagian | `UAT-09`, `AT-HRD-B4-05b` |
| Pegawai tidak dapat membaca data pegawai lain | `UAT-10`, `AT-HRD-A2-01` |
| Bekerja di luar jadwal tidak otomatis menjadi lembur | `UAT-11`, `AT-HRD-B1-07` |
| Pengajuan tanpa penyetuju tidak hilang diam-diam | `UAT-12`, `AT-HRD-A7-05` |
| Setiap layar punya jalan masuk yang sah — butir menu atau layar induk | `UAT-13`, peta butir menu pada `03-frontend-architecture.md` |
| Perubahan penempatan dan remunerasi tidak berlaku tanpa persetujuan pihak yang berbeda | `UAT-14`, `UAT-15`, `AT-HRD-A1-03`, `AT-HRD-A1-07` |
| Nominal gaji tidak tampil pada daftar lintas pegawai | `UAT-16`, `AT-HRD-A1-08` |
| Pegawai hanya dapat melihat slip gaji miliknya sendiri | `UAT-17`, `AT-HRD-SAL-01` |
| Data gaji hanya tampil setelah konfirmasi kata sandi | `UAT-18`, `AT-HRD-SAL-02` |
| Pembacaan gaji tercatat, tanpa nominalnya | `UAT-19`, `AT-HRD-SAL-08`, `AT-HRD-SAL-08b` |
| Keterlihatan nominal gaji secara massal tidak tersedia | `AT-HRD-SAL-11` |
| Seluruh tabel master MVP sudah terisi | Rencana data master awal pada `02-backend-architecture.md` bagian 10 |
| Hak akses pada tombol layar sama dengan hak akses pada endpoint yang dipanggilnya | `AT-HRD-SEC-04`, `NFR-005` |
| Kolom bertanda sensitif tidak masuk catatan log | `AT-HRD-SEC-01`, `NFR-009` |
| Setiap epic `MUST HAVE` punya sekurang-kurangnya satu test yang lulus | `testing/acceptance-test-matrix.md` |
| Tidak ada endpoint HR yang menyimpan hasil pembayaran, jurnal, atau pajak | `AT-HRD-B5-02` |
| Tidak ada tabel baru yang dibuat di luar tabel kepemilikan data | `02-backend-architecture.md` bagian 3 dan 7.4 |

---

## 20. Urutan pengiriman dan pertanyaan terbuka

### 20.1 Gelombang pengiriman

Urutan ditulis sebagai gelombang, bukan tanggal. Penjadwalan tetap wewenang manusia.

| Gelombang | Epic yang tercakup | Syarat mulai |
| --- | --- | --- |
| `MVP-0` | `EPIC HRD-01` fondasi route dan registry; pengisian master data awal, termasuk definisi dan langkah alur persetujuan `T1`–`T7` sesuai `HRD-DEC-034` | Blueprint dan kontrak disetujui |
| `MVP-1` | `EPIC HRD-08` penjadwalan kerja | `MVP-0` selesai. Tanpa jadwal terbit, kehadiran tidak punya acuan pengolahan |
| `MVP-2` | `EPIC HRD-03` layanan mandiri pegawai; `EPIC HRD-05` administrasi kehadiran | `MVP-1` selesai |
| `MVP-3` | `EPIC HRD-04` kotak masuk persetujuan terpadu | `MVP-2` selesai. Kotak masuk memerlukan pengajuan yang benar-benar ada untuk diputuskan |
| `MVP-4` | `EPIC HRD-06` administrasi cuti dan saldo; `EPIC HRD-07` administrasi lembur | `MVP-3` selesai |
| `MVP-5` | `EPIC HRD-02` administrasi kepegawaian, termasuk **empat** alur persetujuan penempatan dan remunerasi sesuai `HRD-DEC-031` dan `HRD-DEC-036`; `EPIC HRD-09` kesiapan payroll sisi HR | `MVP-4` selesai. Isi alur sudah disetujui; **MUST** dipecah per transaksi bisnis saat perencanaan |
| `MVP-6` | `EPIC HRD-10` jaring pengaman pengujian | Berjalan **bersama** setiap gelombang, bukan setelahnya. Ditulis terpisah agar tidak terlupa, bukan agar ditunda |
| `POST-MVP` | Seluruh kemampuan yang ditunda pada bagian 8; **ditambah orkestrasi putaran payroll** sesuai `HRD-DEC-035` | Di luar cakupan rilis pertama. Orkestrasi payroll menunggu batas Finance disepakati |

**Tidak ada gelombang yang memuat epic berstatus `OPEN DECISION`.**

Satu catatan tentang urutan `MVP-1`. Menempatkan penjadwalan lebih dulu terasa berlawanan dengan
naluri — kehadiran tampak lebih mendesak. Tetapi pengolahan kehadiran membutuhkan jadwal sebagai
acuan; tanpa itu, setiap hari kerja menghasilkan pengecualian jadwal yang tidak dapat
diselesaikan. Mendahulukan kehadiran justru menghasilkan tumpukan pengecualian palsu.

### 20.2 Pertanyaan terbuka sebelum development lock

| Pertanyaan | Siapa yang menjawab | Dampak bila belum dijawab | Memblokir |
| --- | --- | --- | :---: |
| Siapa pemegang hak membuka kembali periode kehadiran? (`HRD-Q-23`) | Pemilik proses HR bersama pemilik keamanan | Mekanismenya sudah ada, tetapi pemetaan perannya belum. `EPIC HRD-05` dapat berjalan; pemetaan peran diisi belakangan | Tidak |
| Berapa nilai bawaan menit kerja terencana, menggantikan angka tetap 480? (`HRD-Q-48`) | Pemilik proses HR | Angka tetap yang ada sekarang terus dipakai. Memindahkannya ke master tanpa jawaban hanya memindahkan angka karangan | Tidak |
| Apa dampak izin pulang cepat terhadap saldo dan pembayaran? (`HRD-Q-47`) | Pemilik proses HR | Layar izin pulang cepat tidak dapat diselesaikan | **Ya** untuk `EPIC HRD-03` bagian izin pulang cepat |
| Apa bentuk data yang diterima Finance? (`HRD-Q-10`) | Pemilik produk bersama Finance | Hanya memblokir batas serah terima itu sendiri. `HRD-DEC-035` memindahkan orkestrasi payroll ke `POST-MVP`, sehingga MVP administratif tidak tertahan | Tidak |
| Apa yang terjadi bila Finance menolak satu batch? (`HRD-Q-11`) | Pemilik produk bersama Finance | Sama seperti di atas. Di luar jalur kritis MVP sejak `HRD-DEC-035` | Tidak |
| Apa isi tabel 67 entity yang belum punya API? (`HRD-Q-05`) | Pemilik basis data | Keputusan skema yang merusak data tidak boleh diambil. Ikut menahan pemasangan unique constraint pada penempatan shift. **Sudah ditutup untuk `S-D1`** lewat `HRD-DEC-059` | **Ya** untuk `S-D2` s.d. `S-D5` |
| Siapa wakil Komite Medik? (`HRD-Q-08`) | Manajemen | Kredensial dan kewenangan klinis tetap `BLOCKED` | **Ya** untuk `S-C1` |
| Apakah `HRD-DEC-010` disahkan K3RS? | K3RS | Kesehatan kerja staf tetap `BLOCKED` | **Ya** untuk `S-C6` |
| Apakah pemisahan peran pengusul dan penyetuju diperlukan pada tindakan disiplin? (`HRD-Q-51`) | Pemilik proses HR | Seseorang dapat mengusulkan sekaligus menyetujui sanksi | **Ya** untuk `S-C5` |
| Tingkatan izin apa yang berlaku bagi data paling terbatas? (`HRD-Q-52`) | Pemilik keamanan bersama pemilik proses | Tidak ada yang menetapkan siapa boleh membaca kasus kedisiplinan | **Ya** untuk `S-C5` |
| Siapa pemilik kebijakan bisnis HR? (`HRD-Q-01`) | Manajemen | Blueprint tidak dapat disetujui secara keseluruhan | **Ya** untuk approval blueprint |
| ~~`HRD-Q-55`~~ — arti "masa studi" | ~~Pemilik produk~~ | **Ditutup** `HRD-DEC-041`: istilah ditarik menjadi **Jenjang Pendidikan** | Tidak |
| ~~`HRD-Q-56`~~ — tanggal mulai bekerja yang otoritatif | ~~Pemilik produk~~ | **`DEFERRED / NOT_APPLICABLE_TO_CURRENT_MVP`.** Masa kerja dikeluarkan dari faktor gaji oleh `HRD-DEC-045`, sehingga pertanyaannya kehilangan pemicunya | Tidak |
| ~~`HRD-Q-54`~~ — satu definisi bersama atau empat terpisah | ~~Pemilik produk~~ | **Ditutup `HRD-DEC-036`:** empat definisi terpisah dengan pola awal yang sama | Tidak |

**Dokumen ini memuat pertanyaan memblokir yang belum terjawab.** Sesuai kontrak, ia tetap boleh
berstatus `draft`, tetapi **MUST NOT** diteruskan ke `/plan-module-delivery` sebelum pertanyaan
yang memblokir gelombang pengiriman dijawab pemiliknya.

---

## 21. Bagian `S-D1` — Perencanaan Tenaga Kerja (`last_changed_in` `v7`, 3 November 2026)

Bagian ini menerapkan kedua puluh bagian wajib kontrak PRD → MVP **khusus untuk `S-D1`**. Bagian
1–20 di atas tidak berubah, kecuali baris `HRD-CAP-19` pada bagian 8 dan baris `HRD-Q-05` pada
bagian 20.2 yang kini merujuk ke sini. Aturan yang sama mengikat: bagian ini menurunkan, tidak
menciptakan. Seluruh entity, status, hak akses, dan endpoint berasal dari
`02-backend-architecture.md` bagian 16, `data/data-dictionary.md` bagian 8, `flowcharts/`, dan
`contracts/` bagian `S-D1`.

### 21.1 Identitas

| Field | Value |
| --- | --- |
| Slice | `S-D1` Perencanaan Tenaga Kerja |
| Status | `approved` — Pengguna (pemilik teknis dan produk, `HRD-DEC-015`), 2026-11-03. Jawaban pemilik: "Yang masih draft semua ya saya setujui", atas pertanyaan approval desain `S-D1` |
| Backend SHA baseline | `26066c76e69b021f68b63b31bde2258cd9d1bb74` (branch `MHamzah`) |
| Frontend SHA baseline | `66b7e5600e1707e4153ca07981ddef054856c8c9` (branch `HamzahV2`) |
| `input_revision` | `00-interview-decisions.md` rev `17` bagian 32; `01-existing-capability-map.md` rev `1.3` bagian 15; `02-backend-architecture.md` rev `3` bagian 16; `03-frontend-architecture.md` rev `3` bagian 12; kontrak `v7` |
| Kesiapan arsitektur domain | `DOMAIN_ARCHITECTURE_NOT_RUN` — administratif; nilai kebijakan klinis tidak diputuskan di sini |
| Ringkasan cakupan | Dari standar kebutuhan tenaga dan formasi resmi sampai setiap kursi tambahan terisi orang yang benar-benar bertugas, lengkap dengan persetujuan, biaya estimasi, dan potret yang dapat diaudit |

### 21.2 Ringkasan eksekutif

Hari ini rumah sakit menentukan kebutuhan tenaga di luar sistem. Akibatnya kepala unit meminta
tenaga lewat jalur informal, HR tidak dapat menjawab "kursi tambahan ICU tahun ini diisi siapa", dan
Direktur menyetujui anggaran tanpa angka gap yang tetap. `S-D1` menutup rantai itu: standar
disahkan, formasi resmi tercatat berversi, gap dihitung jujur ("data belum tersedia" bila datanya
belum ada), permintaan melewati persetujuan sesuai dampaknya, dan setiap kursi tercatat terisi oleh
siapa dan kapan.

### 21.3 Masalah produk

| Yang ada hari ini | Bukti | Yang belum ada |
| --- | --- | --- |
| 11 model perencanaan tenaga, 10 di antaranya tanpa controller dan tanpa isi | `HRD-TF-022`, `HRD-TF-024` | Perilaku apa pun |
| Master grade gaji dengan gaji tengah | `HRD-CAP-60` | Pemakaiannya untuk estimasi biaya |
| SK mutasi yang menolak swa-setuju | `HRD-CAP-58` | Tautannya ke kursi permintaan |
| Matriks persetujuan dengan sumber penyetuju jabatan | `HRD-DEC-072` | Isinya, dan penelusuran unit induk |
| Menu "Kebutuhan Tenaga Kerja" | `HRD-TF-036` | Ternyata membuka persyaratan berkas, bukan perencanaan |
| Layar perencanaan | `HRD-CAP-65` | Tidak ada satu pun |

### 21.4 Visi produk

1. Standar kebutuhan tenaga per unit disahkan dan berlaku pada tanggalnya.
2. Formasi resmi tercatat berversi, bersumber dokumen yang disetujui.
3. Gap dihitung dari formasi dan penempatan yang benar-benar berlaku.
4. Kebutuhan tahunan diusulkan dan disahkan bersama estimasi biayanya.
5. Kebutuhan mendadak diajukan sebagai kursi dan disetujui sesuai dampaknya.
6. Setiap kursi terisi oleh penempatan yang mulai berlaku, dari luar atau dari dalam.
7. Setiap angka yang dipakai untuk memutuskan tetap dapat dibaca kembali.

### 21.5 Batas rilis `S-D1`

**Titik mulai:**

1. Matriks persetujuan, jabatan direktur, dan definisi `T9` s.d. `T13` sudah terisi.
2. HR Admin mengunggah dokumen formasi resmi rumah sakit.

**Titik akhir:**

1. Satu permintaan di luar formasi untuk ICU disetujui sampai Direktur.
2. Kursinya terisi oleh satu pegawai baru dan satu pindahan internal.
3. Satu kursi sisanya dibatalkan dengan formasi ikut turun.
4. Potret bulanan bulan itu tersimpan.

### 21.6 Pelaku sasaran

| Pelaku | Tanggung jawab di `S-D1` |
| --- | --- |
| HR Admin | Menyusun standar, batch formasi awal, rencana, addendum; menautkan pemenuhan |
| HR Manager | Menyetujui langkah HR; mengubah rencana pemenuhan; menyetujui pembatalan; mengatur kebijakan perencanaan |
| Kepala Unit | Mengusulkan standar; mengisi angka beban; mengajukan permintaan headcount untuk unitnya |
| Finance / Anggaran | Menilai anggaran dan mengoreksi biaya estimasi |
| Direktur | Mengesahkan standar, rencana, dan kursi di luar formasi |
| Recruiter | Melihat antrean kursi luar dan menautkan pegawai baru |
| Auditor | Membaca seluruh jejak |

### 21.7 Pemilihan kemampuan

| Kemampuan | ID kemampuan asal | Keputusan |
| --- | --- | --- |
| Standar kebutuhan tenaga tiga metode | `HRD-CAP-48` | Wajib; tanpa standar tidak ada rekomendasi maupun aturan untuk Penjadwalan |
| Skill mix per shift sebagai aturan | `HRD-CAP-49` | Wajib; sumber tunggal aturan minimum (`HRD-DEC-058`) |
| Formasi berversi | `HRD-CAP-50` | Wajib; tanpa formasi setiap permintaan menjadi di luar formasi |
| Batch formasi awal | `HRD-CAP-61` | Wajib; formasi hari ini 0 baris |
| Rencana tahunan dan addendum | `HRD-CAP-51` | Wajib; satu-satunya jalan mengurangi atau memindah kursi |
| Gap langsung dan potret | `HRD-CAP-52`, `HRD-CAP-53` | Wajib; dasar setiap persetujuan |
| Permintaan headcount, kursi, pembatalan | `HRD-CAP-54` | Wajib; inti rantai |
| Rantai `T9` s.d. `T13` dan direktur dari matriks | `HRD-CAP-55`, `HRD-CAP-57` | Wajib |
| Estimasi biaya dari grade | `HRD-CAP-60` | Wajib; Finance tidak dapat menilai tanpa angka |
| Kursi internal lewat SK | `HRD-CAP-58` | Wajib; contoh ICU "4 rekrut + 2 pindahan" |
| Antrean Recruiter dengan tautan manual | `HRD-CAP-63` | Wajib; pengganti Rekrutmen yang belum ada |
| Pemisahan menu | `HRD-CAP-64` | Wajib; dua menu bermakna sama menyesatkan |

### 21.8 Kemampuan yang ditunda

| Kemampuan | ID kemampuan asal | Alasan ditunda | Pengganti selama rilis ini |
| --- | --- | --- | --- |
| Metode Analisis Beban Kerja | `HRD-CAP-48` | Butuh katalog kegiatan dan waktu baku per profesi yang belum ada (`HRD-DEC-060`) | Dihitung di luar sistem, hasilnya diisi sebagai jumlah tetap/minimum |
| Angka beban otomatis dari data pasien | `INT-HR-12` | `HRD-DEP-005` `MISSING`; tidak ada kontrak privasi data pasien | Isian manual beserta sumbernya |
| Requisition otomatis dari permintaan | `HRD-CAP-63` | `S-D2` `BLOCKED` | Antrean Recruiter dan tautan manual |
| Ringkasan kekurangan tenaga dari Penjadwalan | `HRD-CAP-59` | Penjadwalan belum memeriksa standar | Kolom potret kosong; perencana memakai catatan unit |
| Pembersihan sepuluh tabel kandidat | `HRD-CAP-19` | Lima di antaranya dirujuk tabel `S-D2` dan Penjadwalan | Tabel dibiarkan kosong dan tidak dipakai |
| Porsi kerja per penempatan | `HRD-CAP-53` | Milik Workforce Core | Dihitung 1 sampai kolomnya ada |
| Kategori alih daya pada jenis tenaga | `HRD-CAP-53` | Milik Master Data HR | Lapisan alih daya tampil "data belum tersedia" |

### 21.9 Alur bisnis target — `FLOW-HRD-MVP-002`

1. HR Admin mengisi formasi awal dari SK formasi resmi; disetujui `T10`.
2. Kepala ICU dan HR Admin menyusun standar ICU; disahkan `T9`.
3. Kepala ICU mengisi angka beban Januari–Maret.
4. HR Admin menyusun Rencana 2027, menghitung ulang, lalu mengajukan; potret gap terkunci.
5. Finance mengoreksi biaya; Direktur Utama mengesahkan; formasi 2027 lahir.
6. Maret: ICU membuka 2 bed; Kepala ICU mengajukan +2 di luar formasi beserta rencana pemenuhan.
7. Rantai `T12` sampai Direktur Pelayanan Medis; formasi naik.
8. Recruiter melihat kursi luar; HR menyiapkan SK untuk kursi internal.
9. Penempatan mulai berlaku; kursi terisi; gap turun.
10. Akhir bulan, potret bulanan tersimpan.

Gambar alurnya ada di `flowcharts/standar-kebutuhan-tenaga.md`, `formasi-dan-rencana-tahunan.md`,
dan `permintaan-headcount.md`.

### 21.10 Epic dan functional requirement

#### `EPIC HRD-11` — Standar kebutuhan tenaga dan angka beban

Disposisi backend: `MISSING / NEW`.

| FR | Perilaku yang dapat diuji |
| --- | --- |
| `FR-HRD-092` | Standar disimpan per unit dengan versi bertanggal berlaku; satu versi berlaku per tanggal. **Contoh:** versi 2 berlaku 1 Desember 2026, versi 1 otomatis berakhir 30 November 2026 |
| `FR-HRD-093` | Setiap baris memilih satu dari tiga metode; Analisis Beban Kerja ditolak. **Contoh:** memilih ABK menghasilkan pesan `VR-WFP-02` |
| `FR-HRD-094` | Rekomendasi dihitung beserta rinciannya. **Contoh:** ICU 7 × 3 × 1,5 = 31,5 → 32 |
| `FR-HRD-095` | Standar hanya dipakai setelah disahkan `T9` |
| `FR-HRD-096` | Angka beban diisi per unit × jabatan × periode beserta sumbernya; isian ganda ditolak |
| `FR-HRD-097` | Penjadwalan dapat membaca standar berlaku per unit per tanggal |

#### `EPIC HRD-12` — Formasi berversi dan batch formasi awal

Disposisi backend: `MISSING / NEW`.

| FR | Perilaku yang dapat diuji |
| --- | --- |
| `FR-HRD-098` | Formasi tidak dapat diubah lewat endpoint apa pun; method tulis menjawab `405` |
| `FR-HRD-099` | Setiap versi menyimpan dokumen sumber, penyetuju, dan waktunya |
| `FR-HRD-100` | Versi terakhir tetap berlaku sampai digantikan. **Contoh:** tanpa Rencana 2028, formasi 1 Januari 2028 = formasi akhir 2027 |
| `FR-HRD-101` | Batch formasi awal wajib scan dokumen, tanpa baris tidak valid, dan disetujui `T10` oleh orang lain; satu unit × jabatan hanya sekali |

#### `EPIC HRD-13` — Gap langsung dan potret

Disposisi backend: `MISSING / NEW`.

| FR | Perilaku yang dapat diuji |
| --- | --- |
| `FR-HRD-102` | Gap dihitung per tanggal acuan dalam porsi kerja, hanya dari penempatan utama. **Contoh:** Sari (Plt di ICU) tidak menambah terisi ICU |
| `FR-HRD-103` | Tiga gap: permanen, sementara, kompetensi. **Contoh:** formasi 32, terisi 29,5, akan kosong 1 → gap permanen 3,5 |
| `FR-HRD-104` | Data yang belum ada bernilai kosong dan tampil "data belum tersedia", bukan 0 |
| `FR-HRD-105` | Potret tersimpan saat dokumen diajukan dan setiap akhir bulan; tidak dapat diubah; bulan yang terlewat tidak diisi diam-diam |

#### `EPIC HRD-14` — Rencana tahunan, addendum, dan biaya

Disposisi backend: `MISSING / NEW`.

| FR | Perilaku yang dapat diuji |
| --- | --- |
| `FR-HRD-106` | Satu rencana tahunan per rumah sakit per tahun; rencana `Approved` terkunci |
| `FR-HRD-107` | Addendum hanya dari rencana `Approved`, memuat baris yang berubah, melewati `T10` penuh |
| `FR-HRD-108` | Usulan yang berbeda dari rekomendasi wajib beralasan |
| `FR-HRD-109` | Biaya = porsi × gaji tengah grade × bulan, berlabel estimasi. **Contoh:** 2 × Rp 6.500.000 × 9 = Rp 117.000.000 |
| `FR-HRD-110` | Finance boleh mengoreksi total pada langkahnya; angka awal tetap tersimpan |
| `FR-HRD-111` | Pemegang tanpa `WorkforcePlanningCost : Read` tidak menerima angka biaya |
| `FR-HRD-112` | Persetujuan rencana melahirkan versi formasi dan menandai permintaan yang kini melebihi formasi |

#### `EPIC HRD-15` — Permintaan headcount, pemenuhan, dan pembatalan

Disposisi backend: `MISSING / NEW`; tautan SK `EXTEND` pada pemakaian `HrdPersonnelActionOrder`.

| FR | Perilaku yang dapat diuji |
| --- | --- |
| `FR-HRD-113` | Permintaan dalam formasi tidak boleh melebihi sisa kursi, dengan pemeriksaan ulang pada persetujuan terakhir |
| `FR-HRD-114` | Dua permintaan yang diajukan bersamaan tidak dapat mengklaim kursi yang sama |
| `FR-HRD-115` | Jalur `T11` untuk dalam formasi dan sementara; `T12` untuk di luar formasi yang menaikkan formasi |
| `FR-HRD-116` | Direktur ditemukan dari matriks persetujuan lewat unit induk; kosong naik ke Direktur Utama; keduanya kosong → pengajuan ditolak |
| `FR-HRD-117` | Kursi terisi hanya saat penempatan cocok mulai berlaku; satu penempatan satu kursi |
| `FR-HRD-118` | Kursi luar muncul di antrean Recruiter; kursi internal dipenuhi lewat SK |
| `FR-HRD-119` | HR Manager bukan pembuat dapat mengubah sumber kursi yang belum terisi, dengan alasan |
| `FR-HRD-120` | Pembatalan hanya kursi yang belum terisi, setelah lowongannya ditutup, disetujui `T13`; formasi dari `T12` ikut turun |
| `FR-HRD-121` | Permintaan melewati batas umur ditandai "lewat batas", tanpa pembatalan otomatis |

### 21.11 Model status

| Entity | Status | Invariant utama |
| --- | --- | --- |
| Standar | `Draft`, `Submitted`, `Returned`, `Rejected`, `Scheduled`, `Active`, `Superseded`, `Cancelled` | Satu `Active` per unit per tanggal |
| Batch formasi awal | `Draft`, `Submitted`, `Returned`, `Rejected`, `Applied`, `Cancelled` | Tidak ada `Applied` tanpa `T10` |
| Rencana dan addendum | `Draft`, `Submitted`, `Returned`, `Rejected`, `Approved`, `Cancelled` | `Approved` terkunci |
| Permintaan | `Draft`, `Submitted`, `Returned`, `Rejected`, `Withdrawn`, `ReadyForRecruitment`, `PartiallyFulfilled`, `Fulfilled`, `Closed` | Sisa kursi tidak pernah negatif akibat permintaan |
| Kursi | `Open`, `Linked`, `Fulfilled`, `Frozen`, `Cancelled` | Kursi `Fulfilled` tidak pernah dibatalkan |
| Pembatalan | `Submitted`, `Approved`, `Rejected` | Pengaju ≠ penyetuju |

Rinciannya: `contracts/state-transition-matrix.md` bagian 11.

### 21.12 Sasaran arsitektur

| Dipakai ulang | Diperluas | Baru |
| --- | --- | --- |
| Mesin workflow dan kotak masuk; penolakan langkah tanpa penyetuju; grade gaji; SK; penempatan; proyeksi cakupan organisasi; number series; pola scheduler; pola pengaman migration | Pencocokan matriks persetujuan lewat unit induk; lima jenis rujukan workflow | 16 tabel `Hrd*`, 15 service, 9 controller, 13 layar |

### 21.13 Sasaran kemampuan API

Seluruhnya **Rencana (belum tersedia)** dan merupakan bagian dari `contracts/api-contract.md`
bagian 13.

| Grup `[Tags]` | Base URL | Epic |
| --- | --- | --- |
| `Corporate / Human Resource / Workforce Planning / Staffing Standard` | `.../workforce-planning/staffing-standards` | `EPIC HRD-11` |
| `Corporate / Human Resource / Workforce Planning / Staffing Workload` | `.../workforce-planning/staffing-workloads` | `EPIC HRD-11` |
| `Corporate / Human Resource / Workforce Planning / Position Formation` | `.../workforce-planning/position-formations` | `EPIC HRD-12` |
| `Corporate / Human Resource / Workforce Planning / Initial Formation Batch` | `.../workforce-planning/initial-formation-batches` | `EPIC HRD-12` |
| `Corporate / Human Resource / Workforce Planning / Staffing Gap` | `.../workforce-planning/staffing-gaps` | `EPIC HRD-13` |
| `Corporate / Human Resource / Workforce Planning / Manpower Plan` | `.../workforce-planning/manpower-plans` | `EPIC HRD-14` |
| `Corporate / Human Resource / Workforce Planning / Headcount Request` | `.../workforce-planning/headcount-requests` | `EPIC HRD-15` |
| `Corporate / Human Resource / Workforce Planning / Recruitment Queue` | `.../workforce-planning/recruitment-queue` | `EPIC HRD-15` |
| `Corporate / Human Resource / Workforce Planning / Setting` | `.../workforce-planning/settings` | `EPIC HRD-13`, `EPIC HRD-15` |

### 21.14 Matriks kewenangan

| Peran | String hak akses utama |
| --- | --- |
| HR Admin | `WorkforceStaffingStandard : Create`, `WorkforceInitialFormationBatch : Submit`, `WorkforceManpowerPlan : Submit`, `WorkforceHeadcountRequest : LinkFulfilment` |
| HR Manager | Seluruh butir HR Admin, ditambah `WorkforceHeadcountRequest : ChangeFulfilmentPlan`, `WorkforcePlanningSetting : Update` |
| Kepala Unit | `WorkforceHeadcountRequest : Submit`, `WorkforceStaffingWorkload : Create`, `WorkforceStaffingStandard : Submit` — unitnya sendiri |
| Finance / Anggaran | `WorkforcePlanningCost : Read`, `WorkforcePlanningCost : Adjust` |
| Recruiter | `WorkforceRecruitmentQueue : Read`, `WorkforceHeadcountRequest : LinkFulfilment` |
| Petugas payroll | Tidak ada |

Daftar lengkap: `contracts/permission-audit-matrix.md` bagian 11.

### 21.15 Batas integrasi dan billing

| `S-D1` **MUST NOT** | Sebabnya |
| --- | --- |
| Membaca data pasien untuk angka beban | `INT-HR-12` ditunda |
| Mencatat siapa bertugas pada shift tertentu | Milik Penjadwalan (`HRD-DEC-058`) |
| Membuat requisition atau lowongan | Milik Rekrutmen (`S-D2`) |
| Memindahkan pegawai | Lewat SK Workforce Core |
| Membaca gaji perorangan | `HRD-DEC-065` |
| Menyentuh billing pasien | Tidak ada hubungannya |

### 21.16 Guardrail regulasi

Buku Panduan bab 03 merujuk Permenkes 13 Tahun 2025 (Pengelolaan SDM Kesehatan) dan Standar
Akreditasi Rumah Sakit 2024. Yang mengikat rilis ini: kebutuhan tenaga harus dapat ditelusuri dari
standar sampai pengisian, dan nilai standar klinis — misalnya rasio perawat ICU — **MUST** ditetapkan
pemilik kebijakan rumah sakit, bukan sistem (`HRD-DEC-060` `CLINICAL_VALUES_NOT_DECIDED_HERE`). Isi
regulasi tidak diverifikasi pada desain ini dan tetap tanggung jawab pemilik kebijakan.

### 21.17 Kebutuhan non-fungsional

| ID | Kebutuhan |
| --- | --- |
| `NFR-010` | Pemeriksaan sisa kursi dan penulisan formasi diserialkan per unit × jabatan dengan advisory lock; dua pengajuan bersamaan tidak sama-sama lolos |
| `NFR-011` | Persetujuan yang melahirkan formasi bersifat atomik: seluruh baris berhasil, atau tidak satu pun |
| `NFR-012` | Draf memakai `RowVersion`; penyimpanan atas versi usang ditolak `409` |
| `NFR-013` | Keputusan workflow yang terkirim ulang dan scheduler yang berjalan dua kali tidak menghasilkan versi formasi atau potret kedua |
| `NFR-014` | Angka biaya dan teks alasan tidak masuk logger dan tidak disimpan di penyimpanan sisi klien |
| `NFR-015` | Seluruh tanggal acuan memakai tanggal operasional rumah sakit; waktu kejadian disimpan UTC |

### 21.18 Skenario UAT

| ID | Epic | Kondisi awal | Langkah | Hasil yang diharapkan |
| --- | --- | --- | --- | --- |
| `UAT-29` | `EPIC HRD-11` | ICU tanpa standar | Susun standar rasio 1:1, 3 shift, faktor 1,5, pembulatan ke atas; isi angka beban 7; ajukan; HR Manager dan Direktur menyetujui | Standar `Active`; rekomendasi 32 beserta rinciannya |
| `UAT-30` | `EPIC HRD-11` | Draf standar ICU | Ajukan tanpa memilih faktor cadangan | Ditolak dengan pesan bahwa faktor cadangan wajib dipilih |
| `UAT-31` | `EPIC HRD-12` | Formasi kosong | Isi batch dari SK Direktur 12/2026, unggah scan, ajukan; tiga penyetuju menyetujui | Formasi ICU versi 1 = 28, bersumber batch itu |
| `UAT-32` | `EPIC HRD-12` | Batch dengan satu baris tidak valid | Ajukan | Ditolak; tombol ajukan nonaktif sampai baris diperbaiki |
| `UAT-33` | `EPIC HRD-13` | ICU formasi 32; Budi paruh waktu; Sari Plt; pengaturan cuti panjang belum diisi | Buka Pemantauan Gap | Terisi memakai 0,5 untuk Budi; Sari tidak dihitung; kolom cuti panjang "data belum tersedia" |
| `UAT-34` | `EPIC HRD-13` | Permintaan diajukan saat gap 3,5 | Satu orang pindah masuk; Finance membuka permintaan | Tampil "saat diajukan 3,5 — sekarang 2,5"; potret tidak dapat diubah |
| `UAT-35` | `EPIC HRD-14` | Formasi 2026 berlaku | Susun Rencana 2027 +2 untuk ICU; Finance mengoreksi biaya; Direktur Utama menyetujui | Formasi ICU 30 mulai 1 Januari 2027; angka awal dan koreksi Finance sama-sama terlihat |
| `UAT-36` | `EPIC HRD-14` | Rencana 2027 `Approved` | Ubah salah satu barisnya | Ditolak; diarahkan membuat addendum |
| `UAT-37` | `EPIC HRD-15` | ICU formasi 30, terisi 28,5 | Ajukan pengganti Tono 1 kursi; HR site dan HR Manager menyetujui | `ReadyForRecruitment`; kursi muncul di antrean Recruiter |
| `UAT-38` | `EPIC HRD-15` | Sisa kursi 0,5 | Ajukan 1 kursi penuh dalam formasi | Ditolak dengan pesan menyebut sisa kursi 0,5 |
| `UAT-39` | `EPIC HRD-15` | Permintaan +2 di luar formasi `ReadyForRecruitment`, 1 kursi luar dan 1 internal | Tautkan penempatan Wati dan SK Andi; tunggu tanggal berlaku; batalkan kursi ketiga dari permintaan lain setelah lowongannya ditutup | Kursi terisi pada tanggal mulai; pembatalan menurunkan formasi |
| `UAT-40` | `EPIC HRD-15` | Kursi dengan lowongan masih terbuka | Ajukan pembatalan | Ditolak; diminta menutup lowongan lebih dulu |

### 21.19 Definition of Done

| Butir | Bukti |
| --- | --- |
| Formasi awal dapat diisi dan disetujui orang lain | `UAT-31`, `AT-HRD-D1-17` |
| Standar tidak berlaku sebelum disahkan | `AT-HRD-D1-03`, `AT-HRD-D1-04` |
| Gap tidak pernah menampilkan 0 untuk data yang belum ada | `UAT-33`, `AT-HRD-D1-05b` |
| Formasi tidak dapat diubah langsung | `AT-HRD-D1-06` |
| Dua permintaan tidak dapat mengklaim kursi yang sama | `AT-HRD-D1-09` |
| Direktur ditemukan dari matriks, dan langkah tanpa penyetuju tidak dilewati | `AT-HRD-D1-20` |
| Biaya tidak terbaca peran yang tidak berhak | `AT-HRD-D1-11` |
| Kursi terisi hanya saat penempatan berlaku | `UAT-39`, `AT-HRD-D1-12` |
| Pembatalan kursi `T12` menurunkan formasi | `AT-HRD-D1-18` |
| Potret tidak dapat diubah | `AT-HRD-D1-16` |
| Seluruh layar `S-D1` dapat dicapai lewat menu | `AT-HRD-D1-22` |
| Data master awal terisi | `02-backend-architecture.md` bagian 16.8 — seluruh baris terisi |

### 21.20 Urutan pengiriman dan pertanyaan terbuka

| Gelombang | Epic | Syarat mulai |
| --- | --- | --- |
| `MVP-7` | Fondasi `S-D1`: migration `AddWorkforcePlanningCore`, enum, number series, jenis rujukan workflow, penelusuran unit induk pada matriks, pengaturan; `EPIC HRD-12` batch formasi awal dan formasi | `S-A1` sudah menyediakan penempatan dan SK; blueprint `S-D1` disetujui |
| `MVP-8` | `EPIC HRD-11` standar dan angka beban; `EPIC HRD-13` gap langsung | `MVP-7` selesai — gap butuh formasi |
| `MVP-9` | `EPIC HRD-15` permintaan, pemenuhan, pembatalan; potret pengajuan | `MVP-8` selesai — permintaan butuh gap dan sisa kursi |
| `MVP-10` | `EPIC HRD-14` rencana tahunan, addendum, biaya; potret bulanan | `MVP-9` selesai — addendum menandai permintaan yang berjalan |
| `POST-MVP` | Seluruh baris bagian 21.8 | Di luar rilis `S-D1` |

Penempatan `MVP-7` s.d. `MVP-10` **sesudah** `MVP-6` adalah usulan: urutan di dalam `S-D1` mengikat,
tetapi apakah `S-D1` ikut rilis pertama modul atau dirilis tersendiri adalah keputusan pemilik produk.

**Tidak ada gelombang yang memuat epic berstatus `OPEN DECISION`.**

| Pertanyaan | Siapa yang menjawab | Dampak bila belum dijawab | Memblokir |
| --- | --- | --- | :---: |
| `S-D1` ikut rilis pertama modul, atau dirilis tersendiri? | Pemilik produk | Hanya penamaan rilis; urutan gelombang tetap | Tidak |
| Nama grup dan butir menu, termasuk pengganti "Kebutuhan Tenaga Kerja" (`HRD-FE-04`) | Pemilik produk | Usulan kerja dipakai sementara | Tidak |
| Pemetaan peran Finance / Anggaran, Direktur, Recruiter (`HRD-DEC-032`) | Administrator | `T10` dan `T12` tidak dapat diajukan | Tidak untuk pengembangan; **Ya untuk rilis** |
| Nilai standar per unit, misalnya rasio perawat ICU (`HRD-Q-01`) | Pemilik kebijakan rumah sakit | Standar tidak dapat diisi untuk dipakai produksi | Tidak untuk pengembangan; **Ya untuk rilis** |
| Cacat batch penempatan awal `HRD-TF-033`, `HRD-TF-034` | Pemilik `S-A1` | Angka terisi `S-D1` bersandar pada penempatan yang dibuat batch itu | Tidak |

**Untuk bagian 21 tidak ada pertanyaan yang memblokir development lock.** Setelah pemilik menyetujui
`02-backend-architecture.md` bagian 16, `03-frontend-architecture.md` bagian 12, kontrak `v7`, dan
bagian ini, `S-D1` boleh diteruskan ke `/plan-module-delivery`. Pertanyaan memblokir pada bagian 20.2
menyangkut epic lain dan tidak menahan `S-D1`.


---

## 22. Bagian `S-D2` — Rekrutmen dan Hiring (`last_changed_in` `v8`, 3 November 2026)

### 22.1 Identitas

| Field | Value |
| --- | --- |
| Slice | `S-D2` Rekrutmen dan Hiring |
| Status | `draft` — mengikuti keputusan `HRD-DEC-073` s.d. `HRD-DEC-080` pada `00-interview-decisions.md` revisi 18 |
| Backend SHA baseline | `e8d47c77` |
| Frontend SHA baseline | `cfa82fbb3` |
| `input_revision` | `00-interview-decisions.md` rev `18` bagian 33; `02-backend-architecture.md` rev `4` bagian 17; `03-frontend-architecture.md` rev `4` bagian 13 |
| Kesiapan arsitektur domain | `DOMAIN_ARCHITECTURE_NOT_RUN` — keputusan bisnis dan regulasi klinis (STR/Akreditasi) telah disahkan |
| Ringkasan cakupan | Pengadaan tenaga kerja rumah sakit dari requisition formasi hingga penyerahan kandidat siap orientasi ke Sub-modul 05 |

### 22.2 Ringkasan eksekutif

Rekrutmen rumah sakit bukan sekadar mengumpulkan CV atau mencatat data pelamar. Kesalahan rekrutmen dapat berakibat fatal
terhadap keselamatan pasien (*patient safety*), risiko hukum tenaga kesehatan tanpa izin sah (STR), pemborosan biaya tes kesehatan,
serta pembengkakan anggaran upah rumah sakit.

`S-D2` menutup seluruh risiko tersebut: lowongan wajib berbasis alokasi formasi yang disahkan Direktur (`HRD-DEC-073`),
pelamar klinis tanpa STR aktif otomatis gugur administrasi (`HRD-DEC-076`), asesor unit memiliki hak veto klinis mutlak (`HRD-DEC-077`),
MCU pra-kerja hanya dibiayai untuk kandidat yang menyepakati tawaran dengan masking kerahasiaan rekam medis (`HRD-DEC-078`),
paket upah divalidasi terhadap batas skala grade dengan eskalasi Direksi (`HRD-DEC-079`), dan kandidat cadangan berperingkat
disiagakan dalam Talent Pool untuk promosi cepat jika kandidat utama mundur (`HRD-DEC-080`).

### 22.3 Batas MVP S-D2

- **Titik Mulai**: Kursi luar (*external seat*) dari `HeadcountRequest` berstatus `APPROVED` di Sub-modul 03.
- **Titik Akhir**: Surat penawaran berstatus `FINAL_ACCEPTED` dengan kelaikan MCU `FIT`, diserahterimakan menjadi draf `WorkforceProfile` berstatus `Pending Onboarding` ke Sub-modul 05.

### 22.4 Kemampuan MUST HAVE & Epic

#### `EPIC HRD-16`: Permintaan Requisition dan Penayangan Lowongan
- `FR-REC-01`: Pembuatan draf `HrdJobRequisition` wajib menarik kursi yang tersedia dari `HeadcountRequestId` aktif (`HRD-DEC-073`).
- `FR-REC-02`: Pembuatan lowongan `HrdJobVacancy` dengan kuota kursi, batas waktu lamaran, dan penanda peran klinis `IsClinical`.

#### `EPIC HRD-17`: Registrasi Pelamar dan Penapisan STR Klinis Otomatis
- `FR-REC-03`: Pendaftaran pelamar dengan validasi unik NIK/KTP terhadap buku induk `WorkforceProfile` existing (`HRD-DEC-075`).
- `FR-REC-04`: Penapisan berkas administrasi: pelamar posisi klinis tanpa STR aktif atau tanggal kedaluwarsa STR < hari ini otomatis gugur (*Auto-Reject*) (`HRD-DEC-076`).

#### `EPIC HRD-18`: Evaluasi Wawancara HR dan Hak Veto Klinis Unit
- `FR-REC-05`: Penjadwalan wawancara independen dan pengisian formulir evaluasi HR (`HRD-DEC-077`).
- `FR-REC-06`: Formulir uji kompetensi klinis oleh Kepala Ruang/Asesor Profesi; rekomendasi `NOT_RECOMMENDED` mengunci kandidat dari tahapan penawaran (*Clinical Veto*).

#### `EPIC HRD-19`: Penawaran Bersyarat dan Kelaikan MCU Pra-Kerja
- `FR-REC-07`: Penerbitan Surat Penawaran Kerja Bersyarat (*Conditional Job Offer*) sebelum MCU pra-kerja (`HRD-DEC-078`).
- `FR-REC-08`: Integrasi kesimpulan kelaikan kerja MCU (`FIT`, `FIT_WITH_RESTRICTIONS`, `UNFIT`) dengan perlindungan kerahasiaan diagnosa medis.

#### `EPIC HRD-20`: Persetujuan Penawaran Bertingkat dan Serah Terima Onboarding
- `FR-REC-09`: Evaluasi penawaran upah terhadap `MstSalaryGrade`: jalur standar `T13` (Unit & HR Manager) vs jalur eskalasi `T14` (Finance & Direktur Utama) (`HRD-DEC-079`).
- `FR-REC-10`: Konversi kandidat `FINAL_ACCEPTED` menjadi draf profil tenaga kerja baru di Sub-modul 02 berstatus `Pending Onboarding` (`HRD-DEC-075`).

#### `EPIC HRD-21`: Bank Kandidat Cadangan (*Talent Pool*)
- `FR-REC-11`: Penampungan otomatis kandidat kompeten berstatus `TALENT_POOL_QUALIFIED` dengan masa retensi 6 bulan (`HRD-DEC-080`).
- `FR-REC-12`: Promosi cepat (*Fast-Track*) kandidat cadangan berperingkat tertinggi saat kandidat utama menolak tawaran (*Offer Rejected*) atau tidak hadir (*No-Show*).

### 22.5 Skenario UAT

| ID | Epic | Skenario Uji | Tindakan | Hasil yang Diharapkan |
| --- | --- | --- | --- | --- |
| `UAT-REC-01` | `EPIC HRD-16` | Buka lowongan tanpa requisition | Buat lowongan tanpa memilih nomor requisition yang sah | Sistem menolak dengan pesan validasi formasi |
| `UAT-REC-02` | `EPIC HRD-17` | Pelamar perawat STR kadaluwarsa | Unggah dokumen pelamar perawat dengan tanggal STR kemarin | Sistem otomatis menetapkan status *Screening Rejected* |
| `UAT-REC-03` | `EPIC HRD-17` | Mantan karyawan melamar kembali | Input NIK yang sudah ada di database `WorkforceProfile` | Sistem mendeteksi profil lama dan menautkan lamaran (*rehire flow*) |
| `UAT-REC-04` | `EPIC HRD-18` | Veto klinis kepala ruang ICU | Kepala ICU memberi nilai `NOT_RECOMMENDED` | Status kandidat menjadi *Assessment Failed*, tombol penawaran terkunci |
| `UAT-REC-05` | `EPIC HRD-19` | Privasi rekam medis MCU | Login akun HR Recruiter, buka data MCU kandidat | Hanya tampil kesimpulan kelaikan kerja; detail rekam medis tersembunyi |
| `UAT-REC-06` | `EPIC HRD-20` | Penawaran gaji di atas plafon grade | Input penawaran gaji pokok di atas `MaximumSalary` | Rantai persetujuan otomatis mewajibkan Direktur Keuangan dan Dirut |
| `UAT-REC-07` | `EPIC HRD-21` | Promosi cepat dari talent pool | Kandidat utama menolak offer; recruiter memilih promosi cadangan | Surat penawaran baru terbit untuk kandidat cadangan peringkat 1 |

### 22.6 Definition of Done (DoD) S-D2

1. [x] Keputusan bisnis `HRD-DEC-073` s.d. `HRD-DEC-080` disahkan pemilik teknis dan produk.
2. [ ] Seluruh 12 entitas model ber-prefix `Hrd` terkonfigurasi pada EF Core DbContext.
3. [ ] Migration membersihkan 20 tabel lama dilengkapi blok pengaman `EMPTY_TABLE_GUARD`.
4. [ ] Endpoint API `[Tags("RecruitmentManagement")]` lulus kompilasi `dotnet build` dan uji QBE.
5. [ ] Layar frontend `FE-HRD-110` s.d. `FE-HRD-122` terpasang dengan base-form-control Quilvian.
6. [ ] Validasi hak veto klinis dan perlindungan rekam medis MCU terverifikasi lulus uji.

### 22.7 Urutan Pengiriman Gelombang

| Gelombang | Epic | Keterangan |
| --- | --- | --- |
| `MVP-11` | `EPIC HRD-16`, `EPIC HRD-17` | Fondasi requisition, lowongan, registrasi pelamar, dan gerbang STR klinis |
| `MVP-12` | `EPIC HRD-18`, `EPIC HRD-19` | Evaluasi wawancara, uji klinis, conditional offer, dan integrasi MCU pra-kerja |
| `MVP-13` | `EPIC HRD-20`, `EPIC HRD-21` | Persetujuan bertingkat, serah terima onboarding, dan talent pool promosi cepat |


---

## 23. Bagian `S-C4-ONB` — Onboarding dan Kontrak Kerja, sub-modul 05 (`last_changed_in` `v9`, 4 November 2026)

Bagian ini menerapkan kedua puluh bagian wajib kontrak PRD → MVP **khusus untuk sub-modul 05**.
Bagian lain dokumen ini tidak berubah. Aturan yang sama mengikat: bagian ini menurunkan, tidak
menciptakan. Seluruh entity, status, hak akses, dan endpoint berasal dari `02-backend-architecture.md`
bagian 18, `03-frontend-architecture.md` bagian 14, `data/data-dictionary.md` bagian 10, `flowcharts/`,
dan `contracts/` bagian `S-C4-ONB`.

### 23.1 Identitas

| Field | Value |
| --- | --- |
| Slice | `S-C4-ONB` — Onboarding dan Kontrak Kerja (bagian sub-modul 05 dari `S-C4`; label usulan kerja) |
| Status | `approved` — Pengguna (pemilik teknis dan produk, `HRD-DEC-015`), 2026-11-04, beserta kelima pilihan desain (`HRD-DEC-096`). Jawaban pemilik: "setujui semua ya dan kerjakan" |
| Repository target | `NewQuilvianSystemBackend` (backend), `QuilvianSystemFrontendDev` (frontend) |
| Backend SHA baseline | `8b17b75a` (branch `MHamzah`) |
| Frontend SHA baseline | `44990b3d3` (branch `HamzahV2`) |
| Keputusan sumber | `HRD-DEC-081` s.d. `HRD-DEC-090`, `HRD-DEC-095` (decision log revision `19` bagian 34) |
| Ringkasan cakupan | Serah terima rekrutmen sampai siap kerja, kontrak pertama, masa percobaan PKWTT, pendampingan klinis, siklus kontrak PKWT, dan data awal kontrak pegawai lama |

### 23.2 Ringkasan eksekutif

Rumah sakit dapat menerima pegawai baru sampai siap dinas **tanpa formulir kertas yang tercecer**:
kontrak tersusun otomatis dari tawaran, checklist orientasi terbentuk sendiri, dan pegawai baru tidak
mungkin masuk jadwal sebelum kontraknya ditandatangani dan HRD menyatakan siap kerja. Setelah bekerja,
tidak ada kontrak PKWT yang lewat tanpa keputusan, tidak ada masa percobaan yang melanggar hukum, dan
setiap keputusan cukup satu langkah persetujuan HRD (`HRD-DEC-081`).

### 23.3 Masalah produk

| Kondisi hari ini | Bukti |
| --- | --- |
| Tidak ada satu pun endpoint atau layar onboarding, masa percobaan, maupun kontrak tidak diperpanjang | `HRD-TF-039` |
| Dua struktur onboarding kembar tanpa pemakai | `HRD-TF-040` |
| Masa percobaan dapat diisi pada kontrak PKWT, berlawanan dengan PP 35/2021 | `HRD-TF-043` |
| Riwayat kontrak dapat diketik bebas; 55 pegawai tanpa satu pun kontrak tercatat | `HRD-TF-044` |
| Status `Pending Onboarding` yang dijanjikan rekrutmen belum ada | `HRD-TF-046` |
| Tawaran kerja tidak memuat jenis maupun lama kontrak | `HRD-TF-049` |

Yang sudah ada dan dipakai ulang: riwayat kontrak `WfpContractHistory` (02), pembuatan akun di master
pegawai, mesin persetujuan bersama, penyimpanan berkas, dan exit clearance 20.

### 23.4 Visi produk

1. Kandidat menerima tawaran yang memuat jenis dan lama kontrak.
2. Serah terima membuka kasus onboarding, draf kontrak, dan permintaan akun sekaligus.
3. Pegawai hadir, kontrak ditandatangani, penempatan dan gaji berlaku sejak tanggal mulai kontrak.
4. Checklist tiga lapis selesai; HRD menyatakan siap kerja; pegawai masuk jadwal.
5. Tenaga klinis didampingi preceptor sampai Kepala Unit menyatakan siap mandiri.
6. Pegawai PKWTT sejak awal menjalani masa percobaan tiga bulan yang pasti berujung.
7. Menjelang akhir PKWT, atasan memutuskan perpanjang, pegawai tetap, atau tidak diperpanjang;
   keputusan terakhir diserahkan ke proses keluar 20.

### 23.5 Batas rilis

**Titik mulai:**

1. Tawaran `S-D2` berstatus final diterima dan kandidat dinyatakan layak MCU.
2. Pegawai yang sudah bekerja sebelum sub-modul ini dipakai (lewat batch data awal).

**Titik akhir:**

1. Pegawai baru berstatus Aktif dan boleh dijadwalkan, atau kasusnya batal bergabung.
2. Pendampingan klinis dinyatakan siap mandiri.
3. Masa percobaan berujung lulus, lulus otomatis, atau tidak lulus yang sudah diserahkan ke 20.
4. Setiap kontrak PKWT berujung kontrak lanjutan bertanda tangan, atau tidak diperpanjang yang sudah
   diserahkan ke 20.

### 23.6 Pelaku sasaran

| Pelaku | Tanggung jawab di dalam MVP |
| --- | --- |
| HR Manager | Serah terima dari rekrutmen (04); pengaturan; melihat eskalasi |
| Petugas HRD | Koordinator kasus, hadir hari pertama, kontrak, siap kerja, batal bergabung, koreksi, batch, penyetuju `T15` s.d. `T18` |
| Kepala Unit | Butir unit, atasan langsung, preceptor, keputusan kontrak, hasil masa percobaan, siap mandiri |
| Atasan langsung | Keputusan kontrak dan hasil masa percobaan bawahannya |
| Administrator / IT | Memberi peran kerja dan menandai permintaan akses selesai |
| Pegawai baru | Hadir, menandatangani kontrak, memakai layanan mandiri sejak hari pertama |

### 23.7 Pemilihan kemampuan MVP

| Kemampuan | ID kemampuan asal | Keputusan MVP |
| --- | --- | --- |
| Kasus onboarding otomatis dan checklist tiga lapis | `HRD-CAP-17` | Wajib; tanpa ini pegawai baru tidak punya jalur masuk |
| Kontrak pertama dari tawaran dan tanda tangan | `HRD-CAP-17`, `HRD-CAP-02` | Wajib; PKWT wajib tertulis |
| Penempatan dan gaji awal dari tawaran | `HRD-CAP-30`, `HRD-CAP-31` | Wajib; tanpa ini pegawai tidak digaji |
| Siap kerja dan akun bertahap | `HRD-CAP-17` | Wajib; penentu boleh dijadwalkan |
| Masa percobaan PKWTT | `HRD-CAP-17` | Wajib; tanpa ini PKWTT tidak punya batas evaluasi yang sah |
| Pendampingan klinis | `HRD-CAP-17` | Wajib; keselamatan pasien untuk tenaga klinis baru |
| Keputusan kontrak dan penahan jadwal | `HRD-CAP-17`, `HRD-CAP-36` | Wajib; tanpa ini PKWT lewat tanpa keputusan |
| Koreksi kontrak | `HRD-CAP-17`, `HRD-CAP-36` | Wajib; satu-satunya jalan memperbaiki kontrak setelah 02 baca saja |
| Batch data awal kontrak | `HRD-CAP-02` | Wajib; tanpa ini 55 pegawai lama tidak pernah mendapat pengingat |
| Daftar Perlu Tindakan | `HRD-CAP-17` | Wajib; pengganti notifikasi yang tidak ada (`HRD-TF-052`) |

### 23.8 Kemampuan yang ditunda

| Kemampuan | ID kemampuan asal | Alasan ditunda | Pengganti selama MVP |
| --- | --- | --- | --- |
| Notifikasi surel atau push | `HRD-CAP-17` | Repository tidak punya layanan notifikasi (`HRD-TF-052`); membangunnya adalah kemampuan platform tersendiri | Daftar Perlu Tindakan dengan penanda jumlah di menu |
| Blokir kerja karena kewenangan klinis | `HRD-CAP-17` | `HRD-DEC-005` menunggu Komite Medik/Keperawatan | Peringatan kredensial pada profil, kasus, dan jadwal |
| Template per jabatan, site, atau entitas hukum | `HRD-CAP-17` | Ditolak untuk rilis pertama (`HRD-DEC-081` butir 6, `HRD-DEC-086`) | Tiga lapis: Umum, Klinis, Unit |
| Layar "Kontrak Saya" bagi pegawai | `HRD-CAP-17` | Tidak diminta pada wawancara 05 | Pegawai menerima salinan kontrak bertanda tangan di luar sistem |
| Pemindahan master template ke folder `MasterData` | — | Utang teknis lokasi; butuh approval pemilik arsitektur backend | Master tetap di `LifecycleManagement/Models` |

### 23.9 Alur bisnis target — `FLOW-HRD-MVP-003`

1. Recruiter menyusun tawaran berisi jenis dan lama kontrak; tawaran disetujui dan diterima.
2. HR Manager menjalankan serah terima; kasus onboarding, draf kontrak, dan permintaan akun terbentuk.
3. Pada hari pertama, HRD mencatat hadir; akun layanan mandiri aktif.
4. Kontrak ditandatangani dan scan-nya diunggah; penempatan dan gaji awal berlaku.
5. HRD dan Kepala Unit menyelesaikan checklist; Kepala Unit memilih atasan langsung dan preceptor.
6. HRD menyatakan siap kerja; pegawai Aktif, didampingi, dan masuk jadwal.
7. Kepala Unit menyatakan siap mandiri.
8. Untuk PKWTT sejak awal: atasan menilai masa percobaan sebelum hari ke-90; HRD memutuskan.
9. Untuk PKWT: 60 hari sebelum berakhir, atasan memilih keputusan; HRD memutuskan; kontrak
   lanjutan ditandatangani atau kasus diserahkan ke 20.

Rincian dan jalur pengecualian: `flowcharts/onboarding-dan-siap-kerja.md`, `masa-percobaan.md`,
`pendampingan-klinis.md`, `siklus-kontrak.md`, `batch-data-awal-kontrak.md`.

### 23.10 Epic dan functional requirement

#### `EPIC HRD-22` — Kasus onboarding dan checklist

Disposisi backend: `MISSING / NEW` (kasus, butir); `EXTEND` (template).

| FR | Perilaku yang dapat diuji |
| --- | --- |
| `FR-ONB-001` | Serah terima membuat tepat satu kasus. **Contoh:** serah terima Rina dijalankan dua kali; hanya satu kasus `ONB-2027-000015` yang ada |
| `FR-ONB-002` | Kasus perawat ICU memuat butir Umum, Klinis, dan ICU; kasus petugas keuangan hanya Umum. **Contoh:** Rina 10 butir, Budi 6 butir |
| `FR-ONB-003` | Butir otomatis tidak dapat dicentang tangan; butir wajib bukti menolak tanpa berkas |
| `FR-ONB-004` | Kepala Unit hanya mencentang butir unit di unitnya; HRD hanya butir HRD |
| `FR-ONB-005` | Batal Bergabung hanya sebelum hadir hari pertama, dengan alasan dan catatan; kursi kembali terbuka di 04 |
| `FR-ONB-006` | Perubahan template tidak mengubah kasus yang sudah berjalan |

#### `EPIC HRD-23` — Kontrak pertama dari tawaran

Disposisi backend: `EXTEND` (`HrdJobOffer`, `WfpContractHistory`); `MISSING / NEW` (service penulisan).

| FR | Perilaku yang dapat diuji |
| --- | --- |
| `FR-ONB-010` | Tawaran tanpa jenis kontrak, atau PKWT tanpa lama 1–60 bulan, tidak dapat diajukan |
| `FR-ONB-011` | Draf kontrak pertama menyalin jenis, tanggal mulai, dan tanggal berakhir dari tawaran. **Contoh:** PKWT 12 bulan mulai 1 Feb 2027 → berakhir 31 Jan 2028 |
| `FR-ONB-012` | Kontrak tidak dapat ditandai sudah ditandatangani tanpa scan |
| `FR-ONB-013` | Penandatanganan membuat penempatan organisasi, jabatan, dan gaji awal sejak tanggal mulai kontrak, dalam satu transaksi |
| `FR-ONB-014` | Isi draf kontrak tidak dapat diubah langsung; perubahan lewat revisi tawaran |
| `FR-ONB-015` | Endpoint tulis riwayat kontrak 02 menjawab `409` dengan pesan terbaca |

#### `EPIC HRD-24` — Siap kerja dan akun bertahap

Disposisi backend: `MISSING / NEW`; `EXTEND` (`EmployeeController` diekstrak).

| FR | Perilaku yang dapat diuji |
| --- | --- |
| `FR-ONB-020` | Siap kerja ditolak selama masih ada butir wajib; ditolak bila penyata sama dengan pemilih atasan langsung |
| `FR-ONB-021` | Siap kerja mengubah status menjadi Aktif, membuat relasi atasan, dan meminta akses kerja |
| `FR-ONB-022` | Pegawai `PENDING_ONBOARDING` tidak dapat dijadwalkan |
| `FR-ONB-023` | Akun dibuat saat kasus terbentuk dan aktif sejak hadir hari pertama hanya untuk layanan mandiri |
| `FR-ONB-024` | Gaji periode pertama dihitung sejak tanggal mulai kontrak. **Contoh:** siap kerja 4 Feb, gaji Februari dari 1 Feb |

#### `EPIC HRD-25` — Masa percobaan PKWTT

Disposisi backend: `MISSING / NEW`.

| FR | Perilaku yang dapat diuji |
| --- | --- |
| `FR-ONB-030` | Masa percobaan hanya terbentuk untuk kontrak PKWTT pertama sejak bergabung; tidak untuk PKWT maupun pengangkatan PKWT ke PKWTT |
| `FR-ONB-031` | Masa percobaan berlaku tepat 3 bulan dan tidak dapat diperpanjang. **Contoh:** 1 Feb → 30 Apr 2027 |
| `FR-ONB-032` | Tidak Lulus wajib lampiran; Lulus tidak |
| `FR-ONB-033` | Tanpa hasil disetujui sampai tanggal akhir, pegawai lulus otomatis sehari sesudahnya |
| `FR-ONB-034` | Tidak Lulus yang disetujui diserahkan ke 20 |

#### `EPIC HRD-26` — Pendampingan klinis

Disposisi backend: `MISSING / NEW`.

| FR | Perilaku yang dapat diuji |
| --- | --- |
| `FR-ONB-040` | Tenaga klinis tidak dapat siap kerja sebelum preceptor dipilih |
| `FR-ONB-041` | Selama didampingi, pegawai tetap dapat dijadwalkan dan jadwal menampilkan penanda Didampingi |
| `FR-ONB-042` | Siap Mandiri wajib lampiran dan tidak masuk persetujuan HRD |
| `FR-ONB-043` | Peringatan kewenangan klinis tampil tetapi tidak menghalangi siap kerja maupun jadwal |

#### `EPIC HRD-27` — Siklus kontrak dan penahan jadwal

Disposisi backend: `MISSING / NEW`; `EXTEND` (`ResignationLifecycleHandoffService`).

| FR | Perilaku yang dapat diuji |
| --- | --- |
| `FR-ONB-050` | Kontrak PKWT muncul di Perlu Tindakan 60 hari sebelum berakhir, dan eskalasi tampil bagi HR Manager 14 hari sebelum berakhir |
| `FR-ONB-051` | Perpanjangan yang membuat total PKWT lebih dari 5 tahun ditolak. **Contoh:** PKWT pertama 1 Apr 2024, perpanjangan sampai 30 Sep 2029 ditolak |
| `FR-ONB-052` | Tidak Diperpanjang yang disetujui diserahkan ke 20 dengan tanggal terakhir bekerja sama dengan tanggal berakhir kontrak |
| `FR-ONB-053` | PKWT yang lewat tanpa lanjutan bertanda tangan berstatus Berakhir tanpa keputusan dan jadwal sesudahnya ditahan |
| `FR-ONB-054` | Koreksi kontrak disetujui petugas HRD lain dan versi lama tersimpan |

#### `EPIC HRD-28` — Batch data awal kontrak dan pembersihan tabel kandidat

Disposisi backend: `MISSING / NEW`; tabel kandidat dibuang.

| FR | Perilaku yang dapat diuji |
| --- | --- |
| `FR-ONB-060` | Batch dengan baris tidak sah tidak dapat diajukan; pegawai hanya sekali masuk batch |
| `FR-ONB-061` | Batch yang disetujui membuat kontrak data awal tanpa masa percobaan, dan pengingat kontrak langsung berlaku |
| `FR-ONB-062` | Scan kontrak lama boleh menyusul dan tampil di Perlu Tindakan |
| `FR-ONB-063` | Migration pembersihan enam tabel kandidat berhenti bila salah satunya berisi data |

### 23.11 Model status

| Objek | Status | Invariant utama |
| --- | --- | --- |
| Kasus onboarding | `InProgress`, `Completed`, `Cancelled` | Satu per serah terima; batal hanya sebelum hadir |
| Kontrak | `Draft`, `Active`, `Completed`, `Ended`, `LapsedWithoutDecision`, `Cancelled` | Tepat satu kontrak berjalan per pegawai; sah hanya dengan scan |
| Masa percobaan | `Running`, `AwaitingDecision`, `Passed`, `PassedAutomatically`, `Failed` | Hanya PKWTT sejak bergabung; selalu berujung |
| Pendampingan | `PreceptorAssigned`, `Accompanied`, `Independent`, `Cancelled` | Satu preceptor berjalan |
| Status kepegawaian | `PENDING_ONBOARDING`, `ACTIVE`, `CANCELLED_JOIN` | Hanya `ACTIVE` yang boleh dijadwalkan |

Daftar lengkap perpindahan: `contracts/state-transition-matrix.md` bagian 13.

### 23.12 Sasaran arsitektur

| Dipakai ulang | Diperluas | Baru |
| --- | --- | --- |
| Mesin persetujuan, number series, penyimpanan berkas, proyeksi cakupan unit, exit clearance 20 | `WfpContractHistory`, dua master template, `MstContractType`, `HrdJobOffer`, `ResignationLifecycleHandoffService`, `EmployeeController` (ekstraksi akun), `WfpContractHistoryController` (baca saja) | 12 tabel `Hrd*`, 17 service, 9 controller di `LifecycleManagement`; 4 service di 01 dan 02 |

### 23.13 Sasaran kemampuan API

Base URL bersama: `api/v1/corporate/human-resource/lifecycle-management`. Seluruh endpoint
**Rencana (belum tersedia)**; daftar lengkap pada `contracts/api-contract.md` bagian 15.

| Grup `[Tags]` | Endpoint pokok | Hak akses | Epic |
| --- | --- | --- | --- |
| `Corporate / Human Resource / Lifecycle Management / Onboarding Case` | `GET /onboarding-cases`, `GET /{id}`, `POST /{id}/first-day-attendance`, `POST /{id}/items/{itemId}/complete`, `POST /{id}/unit-items/{itemId}/complete`, `POST /{id}/declare-ready`, `POST /{id}/cancel-join` | `WorkforceOnboardingCase : Read`, `: CompleteItem`, `: CompleteUnitItem`, `: DeclareReady`, `: CancelJoin` | `EPIC HRD-22`, `HRD-24` |
| `Corporate / Human Resource / Lifecycle Management / Onboarding Template` | `GET`, `POST`, `PUT /onboarding-templates` | `WorkforceOnboardingTemplate : Read`, `: Create`, `: Update` | `EPIC HRD-22` |
| `Corporate / Human Resource / Lifecycle Management / Employment Contract` | `GET /employment-contracts`, `POST /{id}/mark-signed`, `POST /{id}/decisions`, `POST /{id}/corrections`, `POST /decisions/{decisionId}/notification` | `WorkforceEmploymentContract : Read`, `: MarkSigned`, `: RequestCorrection`; `WorkforceContractDecision : Submit`, `: RecordNotification` | `EPIC HRD-23`, `HRD-27` |
| `Corporate / Human Resource / Lifecycle Management / Probation` | `GET /probations`, `POST /{id}/assessments` | `WorkforceProbation : Read`, `: Submit` | `EPIC HRD-25` |
| `Corporate / Human Resource / Lifecycle Management / Clinical Preceptorship` | `PUT /{id}/preceptor`, `POST /{id}/declare-independent` | `WorkforcePreceptorship : Manage` | `EPIC HRD-26` |
| `Corporate / Human Resource / Lifecycle Management / Initial Contract Batch` | `GET /template`, `POST /validate`, `POST`, `POST /{id}/submit` | `WorkforceInitialContractBatch : Read`, `: Create`, `: Submit` | `EPIC HRD-28` |
| `Corporate / Human Resource / Lifecycle Management / Onboarding Access Request` | `GET`, `POST /{id}/complete`, `POST /{id}/retry` | `WorkforceOnboardingAccessRequest : Read`, `: Complete`, `: Retry` | `EPIC HRD-24` |
| `Corporate / Human Resource / Lifecycle Management / Lifecycle Worklist` | `GET /worklist`, `GET /worklist/counts` | `WorkforceLifecycleWorklist : Read` | `EPIC HRD-27` |
| `Corporate / Human Resource / Lifecycle Management / Lifecycle Setting` | `GET`, `PUT /lifecycle-settings` | `WorkforceLifecycleSetting : Read`, `: Update` | `EPIC HRD-27` |

### 23.14 Matriks kewenangan

| Peran | String hak akses |
| --- | --- |
| HR Admin | `WorkforceOnboardingCase : Read/CompleteItem/Reassign/DeclareReady/CancelJoin`, `WorkforceOnboardingTemplate : Read/Create/Update`, `WorkforceEmploymentContract : Read/MarkSigned/UploadDocument/RequestCorrection`, `WorkforceContractDecision : Read/RecordNotification`, `WorkforceProbation : Read`, `WorkforcePreceptorship : Read`, `WorkforceInitialContractBatch : Read/Create/Update/Submit/Cancel`, `WorkforceOnboardingAccessRequest : Read/Retry`, `WorkforceLifecycleWorklist : Read`, `WorkforceLifecycleSetting : Read` |
| HR Manager | HR Admin + `WorkforceLifecycleSetting : Update` |
| Kepala Unit | `WorkforceOnboardingCase : Read/CompleteUnitItem`, `WorkforceEmploymentContract : Read`, `WorkforceContractDecision : Read/Submit`, `WorkforceProbation : Read/Submit`, `WorkforcePreceptorship : Read/Manage`, `WorkforceLifecycleWorklist : Read` — unit dalam cakupan saja |
| Administrator / IT | `WorkforceOnboardingAccessRequest : Read/Complete` |
| Auditor | Seluruh `: Read` |

Sumber: `contracts/permission-audit-matrix.md` bagian 13.

### 23.15 Batas integrasi dan billing

| Yang **MUST NOT** dibuat sendiri oleh sub-modul 05 | Pemilik |
| --- | --- |
| Tabel kontrak kedua | 02 (`WfpContractHistory`) |
| Penempatan dan gaji di luar service 02 | 02 |
| Peran dan hak akses aplikasi | Administrator |
| Catatan pemisahan dan exit clearance di luar service 20 | 20 |
| Perhitungan gaji dan uang kompensasi PKWT | 10 Payroll |
| Aturan penjadwalan berpasangan | 06 Penjadwalan |
| Billing pasien | Tidak menyentuh billing sama sekali |

### 23.16 Guardrail regulasi

| Kewajiban | Penerapan |
| --- | --- |
| PKWT wajib tertulis (PP 35/2021) | Kontrak sah hanya dengan scan bertanda tangan; penahan jadwal sesudah kontrak berakhir |
| PKWT tidak boleh mensyaratkan masa percobaan (PP 35/2021) | Masa percobaan hanya dari jenis kontrak yang mengizinkan |
| Masa percobaan PKWTT paling lama 3 bulan (UU 13/2003 Pasal 60) | Tetap 3 bulan, tanpa perpanjangan, lulus otomatis bila tidak dinilai |
| Batas jangka waktu PKWT 5 tahun termasuk perpanjangan (PP 35/2021) | Perpanjangan melewati 5 tahun ditolak; batch memeriksa data lama |
| Uang kompensasi saat PKWT berakhir (PP 35/2021) | 05 menyediakan tanggal dan jenis pemisahan untuk 10 |
| Kerahasiaan kesehatan (`HRD-DEC-010`) | Hanya kesimpulan kelaikan dan catatan pembatasan tanpa diagnosis |
| Bukti orientasi untuk akreditasi (KMK 1596/2024) | Butir wajib bukti dan lampiran siap mandiri tersimpan per pegawai |

### 23.17 Kebutuhan non-fungsional

| ID | Kebutuhan |
| --- | --- |
| `NFR-ONB-001` | Serah terima, penandatanganan kontrak, siap kerja, batal bergabung, dan penerapan keputusan masing-masing satu transaksi; gagal di tengah tidak meninggalkan data setengah jadi |
| `NFR-ONB-002` | Perubahan kontrak satu pegawai diserialkan kunci advisory per pegawai |
| `NFR-ONB-003` | Setiap dokumen memakai `RowVersion`; versi usang ditolak `409` |
| `NFR-ONB-004` | Scheduler harian idempoten: dijalankan dua kali pada hari yang sama tidak mengubah hasil |
| `NFR-ONB-005` | Kolom sensitif tidak masuk logger; berkas bukti diunduh lewat endpoint terautentikasi |
| `NFR-ONB-006` | Tanggal memakai zona waktu rumah sakit; "hari sesudah tanggal akhir" dihitung pada pergantian hari setempat |

### 23.18 Skenario UAT

| ID | Epic | Kondisi awal | Langkah | Hasil yang diharapkan |
| --- | --- | --- | --- | --- |
| `UAT-ONB-01` | `HRD-22` | Tawaran Rina final diterima, MCU layak | HR Manager menjalankan serah terima | Kasus dengan 10 butir dan draf kontrak PKWT terbentuk |
| `UAT-ONB-02` | `HRD-22` | Kasus Rina berjalan, belum hadir | Rina tidak datang; HRD menekan Batal Bergabung | Kasus dibatalkan, profil nonaktif, kursi terbuka di rekrutmen |
| `UAT-ONB-03` | `HRD-22` | Rina sudah tercatat hadir | HRD mencoba Batal Bergabung | Ditolak dengan pesan pengunduran diri |
| `UAT-ONB-04` | `HRD-23` | Draf kontrak Rina | HRD menandai ditandatangani tanpa scan | Ditolak; kontrak tetap draf |
| `UAT-ONB-05` | `HRD-23` | Draf kontrak Rina | HRD mengunggah scan dan menandai | Kontrak berjalan; penempatan dan gaji awal sejak 1 Feb 2027 |
| `UAT-ONB-06` | `HRD-23` | Recruiter menyusun tawaran PKWT | Lama kontrak dikosongkan lalu diajukan | Ditolak |
| `UAT-ONB-07` | `HRD-24` | Satu butir wajib belum selesai | HRD menekan Nyatakan Siap Kerja | Ditolak dengan daftar butir |
| `UAT-ONB-08` | `HRD-24` | Seluruh butir wajib selesai | HRD menyatakan siap kerja | Rina Aktif dan dapat dijadwalkan; akses kerja diminta |
| `UAT-ONB-09` | `HRD-24` | Email Rina kosong | Kasus terbentuk | Permintaan akun gagal dengan pesan; ulang berhasil setelah email diisi |
| `UAT-ONB-10` | `HRD-25` | Andi PKWTT, masa percobaan berjalan | Kepala Instalasi mengajukan Tidak Lulus tanpa lampiran | Ditolak |
| `UAT-ONB-11` | `HRD-25` | Masa percobaan Andi tanpa penilaian | Tanggal akhir lewat | Andi lulus otomatis |
| `UAT-ONB-12` | `HRD-25` | Rina PKWT | Petugas mencoba mengisi tanggal masa percobaan | Ditolak |
| `UAT-ONB-13` | `HRD-26` | Rina didampingi Tono | Sari menekan Siap Mandiri tanpa logbook | Ditolak |
| `UAT-ONB-14` | `HRD-26` | Rina didampingi Tono | Sari mengunggah logbook dan menekan Siap Mandiri | Penanda Didampingi hilang tanpa persetujuan HRD |
| `UAT-ONB-15` | `HRD-27` | Budi PKWT sejak 1 Apr 2024 | Kepala Instalasi mengajukan perpanjangan 30 bulan | Ditolak karena melewati 5 tahun |
| `UAT-ONB-16` | `HRD-27` | Kontrak Budi berakhir tanpa keputusan | Penyusun jadwal menjadwalkan Budi sesudah tanggal berakhir | Ditolak; jadwal sampai tanggal berakhir tetap dapat dibuat |
| `UAT-ONB-17` | `HRD-27` | Kontrak Rina akan berakhir | Sari memilih Angkat jadi Pegawai Tetap; Dodi menyetujui; kontrak ditandatangani | Kontrak PKWTT tanpa masa percobaan |
| `UAT-ONB-18` | `HRD-27` | Koreksi kontrak diajukan Dodi | Dodi mencoba menyetujui sendiri | Ditolak; petugas HRD lain menyetujui |
| `UAT-ONB-19` | `HRD-28` | Batch berisi PKWT tanpa tanggal berakhir | HRD mengajukan | Ditolak dengan pesan per baris |
| `UAT-ONB-20` | `HRD-28` | Batch valid | Petugas HRD lain menyetujui | Kontrak data awal terbentuk; pengingat kontrak Budi muncul |

### 23.19 Definition of Done

| Butir | Bukti |
| --- | --- |
| Satu pegawai baru berjalan dari serah terima sampai siap kerja dan masuk jadwal | `UAT-ONB-01`, `UAT-ONB-05`, `UAT-ONB-08` |
| Pegawai yang belum siap kerja atau kontraknya berakhir tanpa keputusan tidak dapat dijadwalkan | `UAT-ONB-16`, `AT-HRD-ONB-24`, `AT-HRD-ONB-32` |
| Tidak ada masa percobaan pada PKWT dan tidak ada masa percobaan tanpa ujung | `UAT-ONB-11`, `UAT-ONB-12` |
| Total PKWT tidak dapat melewati 5 tahun | `UAT-ONB-15`, `AT-HRD-ONB-29` |
| Setiap keputusan sub-modul 05 cukup satu langkah persetujuan dan penyetujunya bukan pengaju | `AT-HRD-ONB-22`, `AT-HRD-ONB-33` |
| Seluruh tabel master dan definisi alur yang dibutuhkan terisi | Rencana data master awal `02-backend-architecture.md` 18.8 |
| Enam tabel kandidat dibuang dengan pengaman tabel kosong | `AT-HRD-ONB-37` |
| Endpoint tulis riwayat kontrak 02 menjawab `409` dan tabnya baca saja | `AT-HRD-ONB-39` |
| Grup menu "Onboarding & Kontrak" terdaftar dan setiap layar terjangkau | `03-frontend-architecture.md` 14.2 |

### 23.20 Urutan pengiriman dan pertanyaan terbuka

| Gelombang | Isi | Epic | Syarat mulai |
| --- | --- | --- | --- |
| `MVP-14` | Fondasi: migration 1–4, data master, number series, definisi `T15` s.d. `T18`, isian kontrak pada tawaran `S-D2`, service penulisan 01/02, riwayat kontrak 02 baca saja; kasus onboarding dan template | `EPIC HRD-22`, `EPIC HRD-23` | Desain `S-C4-ONB` disetujui; service serah terima `S-D2` (`BE-HRD-061`) tersedia |
| `MVP-15` | Siap kerja, akun bertahap, pendampingan klinis | `EPIC HRD-24`, `EPIC HRD-26` | `MVP-14` selesai |
| `MVP-16` | Siklus kontrak, penahan jadwal, koreksi, batch data awal, daftar Perlu Tindakan | `EPIC HRD-27`, `EPIC HRD-28` | `MVP-14` selesai; penahan jadwal aktif setelah 06 membaca `INT-HR-31` |
| `MVP-17` | Masa percobaan | `EPIC HRD-25` | `MVP-14` selesai |
| `POST-MVP` | Seluruh kemampuan pada 23.8 | — | Di luar rilis pertama |

Tidak ada epic `OPEN DECISION` pada gelombang mana pun.

| Pertanyaan | Siapa yang menjawab | Dampak bila belum dijawab | Memblokir |
| --- | --- | --- | :---: |
| Konfirmasi lima pilihan desain yang diturunkan dari keputusan: (1) butir sistem atasan langsung, (2) penempatan dan gaji awal dari tawaran tanpa SK, (3) pengingat sebagai daftar Perlu Tindakan, (4) sertifikat dibaca dari `WfpCertification`, (5) riwayat kontrak 02 menjadi baca saja | Pengguna, saat approval desain | Bila salah satu ditolak, bagian terkait dirancang ulang | Ya — approval desain |
| Perubahan tawaran `S-D2` (`HRD-DEC-095`) dimasukkan ke roadmap `S-D2` | `plan-module-delivery` | `EPIC HRD-23` tidak dapat dikerjakan | Ya — untuk `MVP-14` |
| Pemetaan peran HR Admin, HR Manager, Kepala Unit, Administrator/IT | Administrator (`HRD-DEC-032`) | Rilis tertahan; development tidak | Tidak |
| Aturan blokir kewenangan klinis | Komite Medik/Keperawatan (`HRD-DEC-005`) | Tetap peringatan | Tidak |
| Pengesahan tampilan catatan pembatasan kerja | K3RS (`HRD-DEC-010`) | Butir itu tertahan saat rilis | Tidak |
| Penjadwalan 06 membaca kesiapan (`INT-HR-31`) | Desain 06 (`S-B4`) | Penahan jadwal belum bekerja walau datanya sudah ada | Tidak untuk development 05 |


---

## 24. Bagian `S-B4` — Penjadwalan, Roster, dan On-Call, sub-modul 06 (`last_changed_in` `v10`, 4 November 2026)

| Field | Value |
| --- | --- |
| Status | `draft` — Siap diajukan untuk approval pengguna |
| `contract_version` | `v10` |
| `input_revision` — decision log | `00-interview-decisions.md` revision `20`, bagian 36 (`HRD-DEC-097` s.d. `HRD-DEC-103`) |
| Tanggal | 2026-11-04 |
| Target rilis | Gelombang `MVP-18` s.d. `MVP-20` |

### 24.1 Batas Slice `S-B4`

- **Titik Mulai:** Kepala Ruangan membuka modul Penjadwalan untuk unit yang bekerja bergiliran (misal ICU, IGD, Bangsal), sistem otomatis menarik daftar staf yang berstatus siap kerja (`HRD-DEC-088`), atau staf siaga on-call yang aktif.
- **Titik Selesai:** Roster terbit tanpa masalah pemblokir, siap dibaca oleh modul Kehadiran (07) dan modul klinis; penyesuaian shift pasca-terbit terekam dalam riwayat mutasi; siaga on-call terukur pemenuhan SLA-nya serta terhubung ke modul Payroll (10); dan periode roster terkunci mutlak (`Closed`) oleh HRD saat cut-off payroll.

### 24.2 Pemilihan kemampuan MVP

| ID Kemampuan | Deskripsi Kemampuan | Status di MVP | Dasar Keputusan |
| --- | --- | --- | --- |
| `HRD-CAP-09.1` | Penyusunan roster unit dengan template pola shift dan salin periode lalu | `MUST HAVE` | `HRD-DEC-098` |
| `HRD-CAP-09.2` | Penerbitan roster mandiri oleh Kepala Unit dengan validasi otomatis ulang | `MUST HAVE` | `HRD-DEC-098` |
| `HRD-CAP-09.3` | Mesin validasi keselamatan: blokir `B1`–`B7` & peringatan beralasan `W1`–`W7` | `MUST HAVE` | `HRD-DEC-099`, `HRD-DEC-100` |
| `HRD-CAP-09.4` | Hak pantau dan notifikasi terbit/mutasi roster bagi Kepala Bidang (*read-only*) | `MUST HAVE` | `HRD-DEC-098`, `HRD-DEC-101` |
| `HRD-CAP-09.5` | Penggantian petugas mendadak (*override*) langsung aktif tanpa menurunkan ke Draft | `MUST HAVE` | `HRD-DEC-101` |
| `HRD-CAP-09.6` | Koreksi tanggal mundur maksimal 3 hari ke belakang dengan pemicu hitung ulang absensi | `MUST HAVE` | `HRD-DEC-102` |
| `HRD-CAP-09.7` | Penutupan periode roster oleh HRD Payroll yang mengunci jadwal secara mutlak | `MUST HAVE` | `HRD-DEC-102` |
| `HRD-CAP-09.8` | Manajemen siaga dokter/staf, pemanggilan (*call-in*), SLA respon menit, integrasi Lab/Payroll | `MUST HAVE` | `HRD-DEC-103`, `LAB-DEC-111` |
| `HRD-CAP-09.9` | Permintaan staf darurat antar-unit (*Emergency Staffing Request*) | `DEFERRED` | `HRD-DEC-097` (Ditunda rilis pertama) |
| `HRD-CAP-09.10` | Persetujuan berjenjang sebelum roster terbit (*TrxRosterApproval*) | `DEFERRED` | `HRD-DEC-098` (Ditunda rilis pertama) |

### 24.3 Epic dan Functional Requirement

#### `EPIC HRD-29`: Penyusunan & Penerbitan Roster Bulanan Unit
- `FR-SCH-01`: Sistem otomatis mengisi daftar perawat/staf eligible unit pada draft roster bulan baru dari penempatan aktif di WorkforceCore. `[EXISTING + HARDEN]`
- `FR-SCH-02`: Sistem menyediakan aksi **Salin dari Periode Lalu** untuk menduplikasi seluruh shift bulan sebelumnya ke periode baru. `[MISSING / NEW]`
- `FR-SCH-03`: Sistem menyediakan aksi **Terapkan Pola Shift** (misal Pagi-Siang-Malam-Libur) per pegawai atau kelompok. `[MISSING / NEW]`
- `FR-SCH-04`: Kepala Unit dapat menerbitkan roster unitnya sendiri tanpa persetujuan sebelum terbit (`HRD-DEC-098`). `[EXTEND]`
- `FR-SCH-05`: Tombol **Terbitkan** secara wajib memicu validasi ulang menyeluruh sebelum mengubah status ke `Published`. `[MISSING / NEW]`
- `FR-SCH-06`: Kepala Bidang / Manajer Pelayanan otomatis menerima notifikasi terbit dan memiliki hak pantau *read-only* atas roster unit terkait. `[MISSING / NEW]`

#### `EPIC HRD-30`: Mesin Validasi Keselamatan Jadwal
- `FR-SCH-07`: Sistem memblokir penerbitan roster bila ditemukan pelanggaran `B1`–`B6` (bentrok jam, cuti penuh disetujui, beda unit, belum siap kerja `HRD-DEC-088`, kontrak berakhir tanpa keputusan `HRD-DEC-089`, status kesiapan gagal dibaca `INT-HR-31`). `[EXTEND]`
- `FR-SCH-08`: Sistem memblokir penerbitan (`B7`) bila staf bertanda **Didampingi** dijadwalkan pada shift tanpa perawat Mandiri (`HRD-DEC-100`). `[MISSING / NEW]`
- `FR-SCH-09`: Sistem memunculkan peringatan kuning untuk `W1`–`W3` (jeda < 11 jam, hari kerja > 6 hari, malam berurutan > 2) berdasarkan setelan korporat `HrdSchedulingSetting`. `[EXTEND]`
- `FR-SCH-10`: Sistem memunculkan peringatan kuning untuk `W4`–`W5` (cuti pending, kredensial kedaluwarsa `HRD-DEC-005`). `[EXTEND]`
- `FR-SCH-11`: Sistem memunculkan peringatan kuning untuk `W6` (kurang kuota tenaga) dan `W7` (komposisi kualifikasi shift belum terpenuhi), serta mewajibkan Kepala Unit memilih alasan sebelum tombol Terbitkan aktif. `[MISSING / NEW]`

#### `EPIC HRD-31`: Penggantian Shift Mendadak & Mutasi Pasca-Terbit
- `FR-SCH-12`: Kepala Unit dapat menekan **Ganti Petugas** pada shift terbit untuk menunjuk staf pengganti dengan alasan wajib (`HRD-DEC-101`). `[EXTEND]`
- `FR-SCH-13`: Penggantian shift langsung berstatus efektif di sistem tanpa approval Kepala Bidang sehingga staf pengganti langsung dapat absen tap-in. `[EXTEND]`
- `FR-SCH-14`: Penggantian staf wajib divalidasi terhadap `B1`–`B7` (pengganti yang cuti/bentrok ditolak). `[MISSING / NEW]`
- `FR-SCH-15`: Status roster periode tetap `Published` (tidak turun ke Draft), dengan jejak audit lengkap dan notifikasi otomatis ke pegawai asal, pengganti, dan atasan. `[MISSING / NEW]`

#### `EPIC HRD-32`: Koreksi Mundur & Penguncian Periode Roster
- `FR-SCH-16`: Kepala Unit dapat melakukan koreksi mundur tanggal yang sudah lewat maksimal 3 hari kalender ke belakang dengan alasan wajib (`HRD-DEC-102`). `[MISSING / NEW]`
- `FR-SCH-17`: Koreksi mundur otomatis memicu rekalkulasi status kehadiran harian pada modul Kehadiran (07). `[MISSING / NEW]`
- `FR-SCH-18`: Petugas HRD/Payroll dapat menutup periode roster (**`Closed`**) saat cut-off payroll, yang mengunci seluruh kotak jadwal secara mutlak (*read-only*). `[EXTEND]`

#### `EPIC HRD-33`: Tata Kelola Siaga (On-Call), SLA Respon, dan Integrasi Lintas Modul
- `FR-SCH-19`: Pemisahan wewenang: jadwal siaga staf disusun Kepala Unit; siaga dokter spesialis disusun Pelayanan Medis / KSM (`HRD-DEC-103`). `[MISSING / NEW]`
- `FR-SCH-20`: Status siaga aktif menggunakan kosakata `Active` (bukan `Activated`) agar kontrak resolver dokter konfirmator Lab (`LAB-DEC-111`) tidak rusak. `[EXTEND]`
- `FR-SCH-21`: Unit yang membutuhkan penanganan cito dapat mencatat pemanggilan (*call-in*) dokter/staf siaga beserta waktu panggil dan waktu kedatangan. `[MISSING / NEW]`
- `FR-SCH-22`: Sistem menghitung selisih waktu respon dan membandingkannya dengan target SLA menit (`ExpectedResponseMinutes`) untuk indikator mutu akreditasi RS. `[MISSING / NEW]`
- `FR-SCH-23`: Jam standby siaga otomatis terhubung ke modul Payroll (10), dan transaksi pemanggilan fisik otomatis diteruskan ke modul Lembur (09) dan Payroll (10). `[MISSING / NEW]`

### 24.4 Skenario UAT Utama

#### Skenario UAT-SCH-01: Penerbitan Roster Mandiri dengan Peringatan Beralasan (Jalur Berhasil)
- **Aktor:** Sari (Kepala Ruangan ICU).
- **Langkah:** Sari membuka draft Roster Februari 2027, menekan **Periksa Jadwal**. Sistem mendeteksi tidak ada error memblokir (`B1`–`B7`), namun mendeteksi `W1` (jeda Tono 10 jam) dan `W6` (Shift Malam 10 Feb kurang 1 perawat). Sari memilih alasan `Permintaan Pegawai` untuk Tono dan `Kekurangan Tenaga` untuk shift malam. Tombol **Terbitkan** menyala. Sari menekan **Terbitkan**.
- **Hasil yang Diharapkan:** Status periode berubah menjadi `Published`. Pak Dodi (Kepala Bidang) menerima notifikasi dan dapat membuka roster ICU untuk melihat tanda peringatan beserta alasan yang dipilih Sari. Roster siap dibaca modul Kehadiran.

#### Skenario UAT-SCH-02: Pencegahan Penugasan Pegawai Didampingi Sendirian (Jalur Gagal / Blokir)
- **Aktor:** Sari (Kepala Ruangan ICU).
- **Langkah:** Sari menjadwalkan Rina (perawat bertanda `Didampingi`) pada Shift Malam 18 Februari hanya bersama Susi (yang juga bertanda `Didampingi`), tanpa perawat Mandiri. Sari menekan **Periksa Jadwal**.
- **Hasil yang Diharapkan:** Sistem memblokir penerbitan (`B7`): *"Rina berstatus Didampingi dan tidak boleh berdinas pada shift tanpa perawat Mandiri."* Tombol Terbitkan terkunci (*disabled*). Roster tidak dapat terbit sampai Sari menukar jadwal perawat Mandiri ke shift tersebut.

#### Skenario UAT-SCH-03: Penggantian Petugas Darurat Pasca-Terbit (Jalur Berhasil)
- **Aktor:** Sari (Kepala Ruangan ICU).
- **Langkah:** Pada 14 Februari pukul 19.30 (sebelum shift malam dimulai), Budi sakit mendadak. Sari membuka roster ICU `Published`, menekan **Ganti Petugas** pada Budi, memilih Tono, dan memilih alasan `Sakit Mendadak`.
- **Hasil yang Diharapkan:** Sistem memvalidasi Tono lolos keselamatan. Perubahan tersimpan seketika. Status roster tetap `Published`. Notifikasi otomatis terkirim ke ponsel Budi, Tono, dan tembusan ke Pak Dodi. Tono tiba pukul 20.45 dan langsung dapat absen tap-in tanpa status anomali.

#### Skenario UAT-SCH-04: Pemanggilan Dokter Siaga & Integrasi Hasil Kritis Lab (Jalur Berhasil)
- **Aktor:** Analis Laboratorium & dr. Dodi, Sp.PK.
- **Langkah:** Pukul 02.00 dini hari ditemukan hasil tes kritis di Lab. Analis membuka resolver dokter konfirmator Lab. Sistem Lab mendeteksi dr. Dodi dengan status siaga `Active`. Analis menekan **Panggil Siaga**. dr. Dodi tiba di Lab pukul 02.25. Analis menginput kedatangan dr. Dodi.
- **Hasil yang Diharapkan:** Sistem mencatat waktu respon 25 menit (memenuhi target SLA 30 menit). Transaksi pemanggilan otomatis tercatat di modul Payroll untuk klaim tugas panggil cito.

#### Skenario UAT-SCH-05: Penguncian Mutlak Periode Roster (Jalur Gagal Edit)
- **Aktor:** Tim Payroll HRD & Sari.
- **Langkah:** Tanggal 28 Februari, tim Payroll menekan **Tutup Periode**. Status Roster Februari berubah menjadi `Closed`. Pada tanggal 2 Maret, Sari mencoba mengedit kotak shift Februari.
- **Hasil yang Diharapkan:** Sistem menolak tindakan (HTTP 403) dan antarmuka menampilkan pesan bahwa periode telah ditutup mutlak oleh Payroll. Penyesuaian gaji diarahkan melalui rapel periode berjalan.

### 24.5 Definition of Done (DoD) Slice `S-B4`

| No | Kriteria Selesai (DoD) | Bukti Pengujian |
| :---: | --- | --- |
| 1 | Roster unit bergiliran dapat dibuat otomatis dari penempatan aktif di WorkforceCore | Lulus pengujian query staf eligible |
| 2 | Fitur Salin Periode Lalu dan Terapkan Pola Shift berfungsi mengisi jadwal massal | Pengujian salin 500+ kotak jadwal < 2 detik |
| 3 | Mesin validasi menolak penerbitan saat ada pelanggaran `B1`–`B7` | Unit test & UAT skenario jalur blokir |
| 4 | Peringatan `W1`–`W7` wajib mengisi alasan sebelum tombol Terbitkan aktif | UAT validasi form alasan peringatan |
| 5 | Ganti petugas langsung efektif tanpa approval dan status roster tetap `Published` | UAT mutasi shift & verifikasi absensi tap-in |
| 6 | Koreksi mundur dibatasi maksimal 3 hari ke belakang dan memicu hitung ulang absensi | UAT backdated edit & audit modul 07 |
| 7 | Periode `Closed` mengunci seluruh mutasi shift secara mutlak | UAT pengujian edit pada periode Closed ditolak |
| 8 | Status siaga `Active` terbaca stabil oleh `LabConfirmingDoctorResolver` saat dipanggil | Integrasi test dengan sub-modul Lab |
| 9 | Pemanggilan siaga menghitung waktu respon dan kepatuhan target SLA menit | UAT perekaman call-in dan kalkulasi SLA |
| 10 | Tunjangan siaga dan pemanggilan cito terhubung ke modul Payroll dan Lembur | Verifikasi data handoff ke modul 09 & 10 |

### 24.6 Urutan pengiriman gelombang MVP

| Gelombang | Cakupan Kemampuan | Epic Terkait | Prasyarat |
| --- | --- | --- | --- |
| `MVP-18` | Mesin validasi keselamatan (`B1`–`B7` & `W1`–`W7`), pengaturan parameter kerja `HrdSchedulingSetting`, penyusunan dan penerbitan roster mandiri | `EPIC HRD-29`, `EPIC HRD-30` | Sub-modul 05 (`MVP-15` & `MVP-16`) selesai |
| `MVP-19` | Penggantian shift pasca-terbit (*override*), riwayat mutasi, koreksi mundur 3 hari, dan penguncian mutlak periode `Closed` | `EPIC HRD-31`, `EPIC HRD-32` | `MVP-18` selesai |
| `MVP-20` | Tata kelola penugasan siaga (on-call), pencatatan pemanggilan cito, evaluasi SLA respon menit, integrasi Lab (`LAB-DEC-111`), dan handoff Payroll | `EPIC HRD-33` | `MVP-18` selesai |

### 24.7 Traceability keputusan ke PRD

| Keputusan | Epic & Kriteria DoD |
| --- | --- |
| `HRD-DEC-097` | 24.2, `FR-SCH-01` (Roster khusus shift bergiliran; darurat via ganti petugas) |
| `HRD-DEC-098` | `EPIC HRD-29`, `FR-SCH-04`, `FR-SCH-05`, `FR-SCH-06`, UAT-SCH-01, DoD butir 2 & 4 |
| `HRD-DEC-099` | `EPIC HRD-30`, `FR-SCH-07`, `FR-SCH-09`, `FR-SCH-10`, DoD butir 3 |
| `HRD-DEC-100` | `EPIC HRD-30`, `FR-SCH-08`, `FR-SCH-11`, UAT-SCH-02, DoD butir 3 |
| `HRD-DEC-101` | `EPIC HRD-31`, `FR-SCH-12` s.d. `FR-SCH-15`, UAT-SCH-03, DoD butir 5 |
| `HRD-DEC-102` | `EPIC HRD-32`, `FR-SCH-16` s.d. `FR-SCH-18`, UAT-SCH-05, DoD butir 6 & 7 |
| `HRD-DEC-103` | `EPIC HRD-33`, `FR-SCH-19` s.d. `FR-SCH-23`, UAT-SCH-04, DoD butir 8, 9, 10 |


---

## 25. Bagian `S-B1` — Administrasi Kehadiran dan Presensi Staf Rumah Sakit (Sub-modul 07, `last_changed_in` `v11`, 4 November 2026)

| Field | Value |
| --- | --- |
| Status | `draft` — Siap diajukan untuk approval pengguna |
| `contract_version` | `v11` |
| `input_revision` — decision log | `00-interview-decisions.md` revision `21`, bagian 37 (`HRD-DEC-104` s.d. `HRD-DEC-109`) |
| Tanggal | 2026-11-04 |
| Target rilis | Gelombang `MVP-21` s.d. `MVP-23` |

### 25.1 Batas Slice `S-B1`

- **Titik Mulai:** Mesin biometrik (fingerprint/face recognition) atau mobile ESS mengirimkan rekaman mentah kehadiran (*raw attendance log*), sistem menyimpannya secara *immutable*, lalu mesin pencocok jadwal (*Schedule Resolver*) memetakan log tersebut ke penugasan shift roster aktif pada Sub-Modul 06 (`S-B4`).
- **Titik Selesai:** Seluruh kehadiran harian terklasifikasi statusnya (`Present`, `Late`, `EarlyLeave`, `IncompletePunch`, `Absent`, `OnDuty`); koreksi presensi mandiri maupun kuasa HR *on-behalf* terselesaikan; izin pulang cepat dan surat tugas dinas luar tervalidasi; dan periode presensi ditutup secara bersih (`Closed`) oleh HRD sebelum diserahterimakan ke modul Payroll (10).

### 25.2 Pemilihan Kemampuan MVP

| ID Kemampuan | Deskripsi Kemampuan | Status di MVP | Dasar Keputusan |
| --- | --- | --- | --- |
| `HRD-CAP-10.1` | Penyerapan log presensi biometrik mentah yang *immutable* | `MUST HAVE` | `flows/02-attendance.md`, `ATT-B1` |
| `HRD-CAP-10.2` | Pengaitan otomatis shift malam lintas hari (*overnight shift*) ke tanggal shift masuk | `MUST HAVE` | `HRD-DEC-104` |
| `HRD-CAP-10.3` | Konfigurasi toleransi keterlambatan (*grace period*, default 10 menit) dan hitung penuh bila lewat | `MUST HAVE` | `HRD-DEC-105` |
| `HRD-CAP-10.4` | Penanganan lupa tap 1 punch (*IncompletePunch*) dan konversi otomatis menjadi mangkir saat tutup periode | `MUST HAVE` | `HRD-DEC-106` |
| `HRD-CAP-10.5` | Koreksi presensi mandiri ESS dan kuasa HR (*on-behalf*) dengan bukti lampiran | `MUST HAVE` | `HRD-DEC-028` |
| `HRD-CAP-10.6` | Izin pulang cepat administratif resmi (*Early Leave Permission*) *non-deductible* | `MUST HAVE` | `HRD-DEC-029`, `HRD-DEC-107` |
| `HRD-CAP-10.7` | Presensi penugasan dinas luar rumah sakit (*Official Duty*) ambulans/home care/pelatihan | `MUST HAVE` | `HRD-DEC-109` |
| `HRD-CAP-10.8` | Penutupan periode bulanan dengan guard penolakan bila ada koreksi pending | `MUST HAVE` | `HRD-DEC-108` |
| `HRD-CAP-10.9` | Buka kembali darurat (*Controlled Reopen*) hanya oleh HR Manager & rollback serah terima payroll | `MUST HAVE` | `HRD-DEC-108` |
| `HRD-CAP-10.10` | Integrasi pengenalan wajah biometrik berbasis kecerdasan buatan langsung di kamera CCTV RS | `DEFERRED` | Bukan prioritas MVP (Cukup mesin biometrik standar) |

### 25.3 Epic dan Functional Requirement

#### `EPIC HRD-34`: Penyerapan Log Mentah, Schedule Resolver & Shift Malam Lintas Hari
- `FR-ATT-01`: Sistem menerima rekaman mentah log presensi dari berbagai sumber biometrik dan menyimpannya secara *immutable* di `HrdAttendanceRawLog`. `[EXISTING + HARDEN]`
- `FR-ATT-02`: Mesin pencocok jadwal (*Schedule Resolver*) secara otomatis memetakan tap masuk dan tap keluar ke penugasan shift pada roster kerja aktif (`S-B4`). `[EXTEND]`
- `FR-ATT-03`: Untuk shift malam yang melintasi tengah malam (misal 21.00 s.d. 07.00), sistem mengunci seluruh sesi kehadiran pada tanggal dimulainya shift (`HRD-DEC-104`). `[EXTEND]`
- `FR-ATT-04`: Sistem mendeteksi tap ganda berurutan (*duplicate punch*) dalam rentang 3 menit dan hanya mencatat satu log valid pertama. `[HARDEN]`
- `FR-ATT-05`: Sistem menyediakan drawer audit log mentah untuk menelusuri jam riil dan ID perangkat biometrik staf. `[MISSING / NEW]`

#### `EPIC HRD-35`: Deteksi Keterlambatan, Toleransi Menit & Penanganan Lupa Tap
- `FR-ATT-06`: Sistem menyediakan setelan toleransi keterlambatan (*grace period*, default 10 menit) yang dapat diubah HR Admin (`HRD-DEC-105`). `[MISSING / NEW]`
- `FR-ATT-07`: Kedatangan di dalam batas toleransi menghasilkan status `Present` dan 0 menit terlambat; kedatangan melewati batas toleransi menghasilkan status `Late` dan dihitung penuh dari jam mulai shift resmi. `[EXTEND]`
- `FR-ATT-08`: Catatan presensi yang hanya memiliki 1 tap diberi status sementara `IncompletePunch` dan jam kerja harian belum dihitung (`HRD-DEC-106`). `[MISSING / NEW]`
- `FR-ATT-09`: Sistem mengirimkan notifikasi pengingat ke ESS pegawai dan atasan langsung untuk mengajukan koreksi presensi atas catatan yang tidak lengkap. `[MISSING / NEW]`

#### `EPIC HRD-36`: Tata Kelola Koreksi Presensi Mandiri & Kuasa HR On-Behalf
- `FR-ATT-10`: Pegawai dapat mengajukan koreksi presensi (lupa tap masuk/pulang) via ESS dengan menyertakan alasan dan bukti pendukung. `[EXISTING]`
- `FR-ATT-11`: HR Admin dapat mengajukan koreksi presensi atas nama pegawai (*on-behalf* per `HRD-DEC-028`) bila pegawai terkendala akses ESS, dengan wewenang persetujuan tetap di atasan langsung pegawai. `[MISSING / NEW]`
- `FR-ATT-12`: Koreksi presensi yang disetujui otomatis memicu rekalkulasi status kehadiran harian menjadi `Present` dan menghitung jam kerja sah. `[EXTEND]`
- `FR-ATT-13`: Pengajuan koreksi dibatasi maksimal 3 hari mundur (`HRD-DEC-105`) dan ditolak bila menyentuh periode yang sudah ditutup (`ATT-B2`). `[HARDEN]`

#### `EPIC HRD-37`: Izin Pulang Cepat Non-Deductible & Dinas Luar Official Duty
- `FR-ATT-14`: Pegawai dapat mengajukan Izin Pulang Cepat administratif (*Early Leave Permission*) dengan alasan darurat keluarga atau dinas mendadak (`HRD-DEC-107`). `[MISSING / NEW]`
- `FR-ATT-15`: Izin pulang cepat yang disetujui atasan menandai pengecualian pulang cepat sebagai `Waived/Approved` tanpa sanksi disiplin dan **tidak memotong kuota saldo cuti tahunan**. `[MISSING / NEW]`
- `FR-ATT-16`: Jam kerja riil pada izin pulang cepat dihitung hingga waktu tap keluar nyata di mesin absensi. `[MISSING / NEW]`
- `FR-ATT-17`: Pegawai atau atasan dapat mengajukan Surat Tugas Dinas Luar (*Official Duty Request*) untuk ambulans rujukan, kurir lab, workshop akreditasi RS, atau kunjungan home care (`HRD-DEC-109`). `[MISSING / NEW]`
- `FR-ATT-18`: Permohonan dinas luar yang disetujui mencatat status kehadiran hari itu sebagai `OnDuty` dan jam kerja diakui penuh untuk payroll, dengan opsi perekaman GPS dan foto mobile. `[MISSING / NEW]`

#### `EPIC HRD-38`: Penutupan Periode Kehadiran, Guard Koreksi & Buka Kembali Darurat
- `FR-ATT-19`: HR Attendance Officer dapat menutup periode presensi bulanan (*Close Attendance Period*) untuk mengunci seluruh transaksi menjadi *read-only* (`HRD-DEC-108`). `[EXTEND]`
- `FR-ATT-20`: Sistem secara otomatis menolak aksi penutupan periode bila masih ada permohonan koreksi presensi yang berstatus `Pending Approval`. `[MISSING / NEW]`
- `FR-ATT-21`: Saat penutupan periode berhasil dieksekusi, sistem secara otomatis mengonversi sisa catatan `IncompletePunch` yang belum dikoreksi menjadi `Absent` (Mangkir) tanpa jam kerja (`HRD-DEC-106`). `[MISSING / NEW]`
- `FR-ATT-22`: Buka kembali periode tertutup (*Controlled Reopen*) hanya dapat dieksekusi oleh level HR Manager dengan mengisi alasan audit tertulis wajib. `[EXTEND]`
- `FR-ATT-23`: Aksi reopen periode secara otomatis memicu pembatalan (*rollback*) batch serah terima payroll (`AttendancePayrollHandoff.Rollback`) bila serah terima ke payroll sudah berstatus `Executed`. `[MISSING / NEW]`

### 25.4 Skenario UAT Slice `S-B1`

#### Skenario UAT-ATT-01: Pencatatan Shift Malam Lintas Hari (Jalur Berhasil)
- **Aktor:** Suster Ani (Perawat IGD).
- **Langkah:** Ani memiliki jadwal Shift Malam 10 November (21.00 s.d. 07.00 WIB). Ani tap masuk di mesin biometrik IGD tanggal 10 Nov pukul 20.50 WIB, dan tap keluar tanggal 11 Nov pukul 07.15 WIB.
- **Hasil yang Diharapkan:** Sistem mengaitkan kedua log ke satu record kehadiran tanggal 10 November. Status kehadiran berstatus `Present`, jam kerja efektif tercatat 10 jam, menit terlambat 0 menit, dan Ani tidak tercatat mangkir di tanggal 10 maupun dinas ganda di tanggal 11.

#### Skenario UAT-ATT-02: Evaluasi Toleransi Keterlambatan Grace Period (Jalur Berhasil)
- **Aktor:** Budi (Staf Administrasi) dan Dewi (Perawat ICU).
- **Langkah:** Jam masuk shift 07.00 WIB, toleransi disetel 10 menit. Budi tap masuk pukul 07.08 WIB. Dewi tap masuk pukul 07.14 WIB.
- **Hasil yang Diharapkan:** Budi tercatat berstatus `Present` dengan `LateMinutes = 0`. Dewi tercatat berstatus `Late` dengan `LateMinutes = 14 menit` (dihitung penuh dari 07.00 WIB).

#### Skenario UAT-ATT-03: Penanganan Lupa Tap dan Konversi Otomatis saat Cut-Off (Jalur Gagal / Disiplin)
- **Aktor:** Tono (Staf Rekam Medis).
- **Langkah:** Tono hanya tap masuk pukul 08.00 WIB tanggal 15 November dan lupa tap keluar saat pulang. Sistem menandai status hari itu sebagai `IncompletePunch` dan mengirimkan pengingat ke ESS Tono. Tono tidak mengajukan koreksi hingga tanggal 25 November saat penutupan periode bulanan dilakukan HRD.
- **Hasil yang Diharapkan:** Saat HR Officer mengeksekusi penutupan periode, record kehadiran Tono pada tanggal 15 November otomatis bergeser statusnya menjadi `Absent` (Mangkir) dan tidak ada jam kerja yang dibayarkan.

#### Skenario UAT-ATT-04: Izin Pulang Cepat Non-Deductible (Jalur Berhasil)
- **Aktor:** Suster Rina (Perawat Bangsal).
- **Langkah:** Rina berdinas shift pagi (07.00–14.00). Pada pukul 11.00, anak Rina mengalami demam tinggi dan harus dibawa ke dokter. Rina mengajukan Izin Pulang Cepat di ESS dengan rencana keluar pukul 11.30. Kepala Ruangan menyetujui izin. Rina tap keluar di mesin pukul 11.32.
- **Hasil yang Diharapkan:** Sistem mencatat jam kerja efektif Rina 4 jam 32 menit. Pengecualian pulang cepat berstatus `Waived/Approved`. Kuota saldo cuti tahunan Rina tetap utuh (tidak berkurang 1 menit pun).

#### Skenario UAT-ATT-05: Penutupan Periode Ditolak Bila Ada Koreksi Pending & Reopen Payroll Rollback (Jalur Gagal & Rollback)
- **Aktor:** Sari (HR Officer) dan Pak Dodi (HR Manager).
- **Langkah:** 
  1. Sari mencoba menutup periode presensi November, namun ada 2 permohonan koreksi staf ICU yang masih berstatus `Pending Approval` Kepala Ruangan.
  2. Sistem menolak penutupan periode dengan pesan: *"Penutupan periode ditolak: masih terdapat 2 permohonan koreksi berstatus pending."*
  3. Setelah Kepala Ruangan memproses kedua koreksi, Sari berhasil menutup periode, dan melakukan serah terima payroll (`Executed`).
  4. Keesokan harinya ditemukan kesalahan data darurat. Pak Dodi (HR Manager) membuka kembali periode dengan menekan tombol **Reopen Period** dan menginput alasan audit.
- **Hasil yang Diharapkan:** Sistem mengizinkan pembukaan kembali periode, status periode menjadi `Reopened`, dan sistem secara otomatis mengeksekusi *rollback* pada batch serah terima payroll terkait.

### 25.5 Definition of Done (DoD) Slice `S-B1`

| No | Kriteria Selesai (DoD) | Bukti Pengujian |
| :---: | --- | --- |
| 1 | Raw log presensi biometrik tersimpan secara *immutable* dan tidak dapat diubah/dihapus | UAT immutability & audit database |
| 2 | Sesi shift malam lintas hari dikaitkan penuh ke tanggal shift dimulai | UAT-ATT-01 & verifikasi query daily |
| 3 | Toleransi keterlambatan 10 menit berfungsi dan menghitung penuh bila lewat | UAT-ATT-02 & unit test kalkulasi |
| 4 | Catatan presensi 1 tap berstatus `IncompletePunch` dan memicu notifikasi pengingat | Verifikasi status daily & modul ESS |
| 5 | Sisa incomplete punch otomatis terkonversi menjadi `Absent` saat cut-off periode | UAT-ATT-03 & verifikasi post-close |
| 6 | Koreksi presensi on-behalf oleh HR tersimpan dengan jejak audit dan persetujuan atasan | UAT koreksi on-behalf & audit trail |
| 7 | Izin pulang cepat administratif tidak memotong kuota saldo cuti tahunan | UAT-ATT-04 & verifikasi saldo cuti 08 |
| 8 | Surat tugas dinas luar ambulans/home care tercatat berstatus `OnDuty` | UAT official duty & verifikasi payroll |
| 9 | Penutupan periode presensi menolak bila masih ada koreksi berstatus pending | UAT-ATT-05 & pengujian guard penutupan |
| 10 | Buka kembali periode (reopen) hanya untuk HR Manager dan memicu rollback serah terima payroll | UAT-ATT-05 & verifikasi rollback payroll |

### 25.6 Urutan Pengiriman Gelombang MVP

| Gelombang | Cakupan Kemampuan | Epic Terkait | Prasyarat |
| --- | --- | --- | --- |
| `MVP-21` | Penyerapan log biometrik mentah, schedule resolver shift malam lintas hari, toleransi keterlambatan grace period, dan penanganan lupa tap | `EPIC HRD-34`, `EPIC HRD-35` | Sub-Modul 06 (`MVP-18` & `MVP-19`) selesai |
| `MVP-22` | Tata kelola koreksi presensi mandiri & on-behalf, perizinan pulang cepat non-deductible, dan penugasan dinas luar official duty | `EPIC HRD-36`, `EPIC HRD-37` | `MVP-21` selesai |
| `MVP-23` | Penutupan periode bulanan, guard penolakan koreksi pending, buka kembali darurat (reopen), dan integrasi serah terima payroll | `EPIC HRD-38` | `MVP-22` selesai |

### 25.7 Traceability Keputusan ke PRD

| Keputusan | Epic & Kriteria DoD |
| --- | --- |
| `HRD-DEC-028` | `EPIC HRD-36`, `FR-ATT-11`, DoD butir 6 (Koreksi kehadiran on-behalf HR) |
| `HRD-DEC-029` | `EPIC HRD-37`, `FR-ATT-14`, `FR-ATT-16` (Tiga peran waktu izin pulang cepat) |
| `HRD-DEC-104` | `EPIC HRD-34`, `FR-ATT-03`, UAT-ATT-01, DoD butir 2 (Shift malam dikunci pada tanggal masuk) |
| `HRD-DEC-105` | `EPIC HRD-35`, `FR-ATT-06`, `FR-ATT-07`, UAT-ATT-02, DoD butir 3 (Toleransi keterlambatan grace period) |
| `HRD-DEC-106` | `EPIC HRD-35`, `FR-ATT-08`, `FR-ATT-09`, `FR-ATT-21`, UAT-ATT-03, DoD butir 4 & 5 (Penanganan 1 tap incomplete punch) |
| `HRD-DEC-107` | `EPIC HRD-37`, `FR-ATT-14`, `FR-ATT-15`, UAT-ATT-04, DoD butir 7 (Izin pulang cepat non-deductible) |
| `HRD-DEC-108` | `EPIC HRD-38`, `FR-ATT-19` s.d. `FR-ATT-23`, UAT-ATT-05, DoD butir 9 & 10 (Period close guard & reopen rollback) |
| `HRD-DEC-109` | `EPIC HRD-37`, `FR-ATT-17`, `FR-ATT-18`, DoD butir 8 (Presensi dinas luar ambulans/home care OnDuty) |


---

## 26. Bagian `S-B2` — Administrasi Cuti & Izin Pegawai Rumah Sakit (Sub-modul 08, `last_changed_in` `v12`, 4 November 2026)

Bagian ini menetapkan kontrak produk dan batasan rilis pertama (*MVP scope*) untuk **Slice `S-B2` — Administrasi Cuti & Izin Pegawai Rumah Sakit**, menyerap keputusan interview `HRD-DEC-110` s.d. `HRD-DEC-115` serta asumsi `HRD-ASM-19` s.d. `HRD-ASM-22`.

### 26.1 Batas Lingkup Rilis Pertama (MVP) Cuti & Izin

* **Titik Mulai (Starting Boundary):** Master data jenis cuti aktif (`MstLeaveType`), pembentukan saldo hak cuti tahunan berjalan sejumlah **12 hari kerja per tahun kalender** (atau prorata pegawai baru), dan transfer saldo sisa akhir tahun (*carry-forward*) **maksimal 6 hari kerja dengan batas kedaluwarsa 30 Juni** melalui pencatatan buku besar mutasi saldo (`TrxLeaveBalanceLedger`).
* **Titik Akhir (Ending Boundary):** Pengajuan cuti mandiri ESS disetujui atasan langsung dengan evaluasi kuota pelayanan (*Safe Staffing Coverage Warning*), integrasi pengosongan shift ke Roster 06 (`S-B4`), verifikasi surat sakit dokter ber-SIP berprivasi medis K3RS, mekanisme pemanggilan dinas darurat (*Leave Recall*) yang memulihkan saldo otomatis, rekonsiliasi retrospektif saat cut-off payroll, serta penguncian status *Completed* yang menutup celah audit `/reverse` (`HRD-DEC-115`).

### 26.2 Pemetaan Kemampuan As-Is ke To-Be

| Domain Kemampuan | Kondisi As-Is (Kode Lama) | Target To-Be (Slice `S-B2`) | Disposisi |
| --- | --- | --- | :---: |
| Buku Besar Mutasi Saldo | Saldo disimpan sebagai total akumulasi agregat tanpa rincian mutasi atomik | Pembukuan mutasi saldo kredit/debit via `TrxLeaveBalanceLedger` dengan audit rujukan | `MISSING / NEW` |
| Batasan Carry-Forward | Belum ada pemotongan otomatis batas 6 hari dan belum ada scheduler kedaluwarsa 30 Juni | Kuota sisa bawaan dibatasi max 6 hari dan otomatis hangus per 30 Juni (`HRD-DEC-110`) | `MISSING / NEW` |
| Validasi Kuota Layanan Unit | Pengajuan cuti tidak membaca jadwal dinas shift unit 24/7 di modul 06 | Notifikasi *Safe Staffing Coverage Warning* pada Approval Inbox Kepala Unit (`HRD-DEC-111`) | `MISSING / NEW` |
| Verifikasi Surat Sakit & Privasi | Surat sakit diunggah bebas dan diagnosa klinis dapat terbaca atasan | Sakit $\ge 2$ hari wajib surat dokter ber-SIP; diagnosa medis dimaskir demi privasi K3RS (`HRD-DEC-112`) | `EXTEND` |
| Alur Recall Darurat | Endpoint recall ada namun tanpa alur persetujuan HR Manager yang terkunci | Inisiasi Kepala Ruangan wajib disetujui HR Manager, saldo sisa pulih otomatis (`HRD-DEC-113`) | `EXTEND` |
| Rekonsiliasi Payroll Retrospektif | Cuti pending memotong gaji atau memicu inkonsistensi closing payroll | Cuti pending tidak memotong gaji; rekonsiliasi dilakukan retrospektif bulan depan (`HRD-DEC-114`) | `MISSING / NEW` |
| Penguncian Cuti Selesai | Endpoint `/reverse` terbuka tanpa guard status (`HRD-Q-35`) | Guard status HTTP 409 menolak pembalikan cuti `Completed`; koreksi via HR Adjustment (`HRD-DEC-115`) | `REPAIR` |

### 26.3 Rincian Epic & Kebutuhan Fungsional (Functional Requirements)

#### `EPIC HRD-39`: Manajemen Saldo Cuti Tahunan, Carry-Forward & Buku Besar Mutasi Saldo
- `FR-LEV-01`: Pegawai berhak atas kuota cuti tahunan sejumlah 12 hari kerja per tahun kalender yang dihitung prorata bagi pegawai baru (`HRD-DEC-110`). `[EXISTING / REUSE]`
- `FR-LEV-02`: Sistem mencatat setiap penambahan, pengurangan, dan pembatalan saldo dalam buku besar transaksi `TrxLeaveBalanceLedger` (`HRD-DEC-110`). `[MISSING / NEW]`
- `FR-LEV-03`: Scheduler akhir tahun (31 Desember) otomatis memindahkan sisa cuti berjalan ke saldo bawaan tahun berikutnya maksimal 6 hari kerja (`HRD-DEC-110`). `[MISSING / NEW]`
- `FR-LEV-04`: Sisa saldo cuti di atas 6 hari kerja pada akhir tahun otomatis dihanguskan dengan transaksi `Forfeiture` (`HRD-DEC-110`). `[MISSING / NEW]`
- `FR-LEV-05`: Scheduler tengah tahun (30 Juni pukul 23:59:59 WIB) otomatis menghanguskan sisa kuota saldo carry-forward yang belum digunakan (`HRD-DEC-110`). `[MISSING / NEW]`

#### `EPIC HRD-40`: Pengajuan Cuti ESS, Validasi Kuota Layanan (*Safe Staffing Coverage Warning*) & Persetujuan Atasan
- `FR-LEV-06`: Pegawai dapat mengajukan permohonan cuti tahunan secara mandiri melalui portal ESS dengan memilih rentang tanggal kerja efektif (`HRD-DEC-110`). `[EXISTING / REUSE]`
- `FR-LEV-07`: Pegawai dapat memilih opsi cuti setengah hari (*Half-Day Leave*) dengan pemotongan saldo tepat 0.5 hari kerja pada sesi Pagi atau Siang/Sore (`HRD-ASM-20`). `[MISSING / NEW]`
- `FR-LEV-08`: Sistem otomatis menolak submit permohonan cuti bila kuota saldo aktif pegawai tidak mencukupi durasi yang diajukan (`LEV-B1`). `[EXISTING / REUSE]`
- `FR-LEV-09`: Saat permohonan cuti diajukan, sistem mengevaluasi ketersediaan tenaga unit pada jadwal dinas `S-B4`; bila jumlah staf bertugas berada di bawah rasio minimal aman, sistem memunculkan banner *Safe Staffing Coverage Warning* pada Approval Inbox Kepala Ruangan (`HRD-DEC-111`). `[MISSING / NEW]`
- `FR-LEV-10`: Kepala Ruangan dapat menyetujui permohonan cuti berindikator kuota kritis dengan memilih aksi *Approve with Mitigation* serta mencatat mitigasi perawat pengganti (`HRD-DEC-111`). `[MISSING / NEW]`
- `FR-LEV-11`: Pegawai dapat membatalkan permohonan cuti yang telah disetujui sebelum tanggal mulai berjalan (*Pre-Leave Cancellation*), yang secara otomatis memulihkan saldo 100% ke buku besar (`HRD-ASM-21`). `[EXISTING / REUSE]`

#### `EPIC HRD-41`: Tata Kelola Cuti Sakit, Verifikasi Surat Keterangan Dokter Ber-SIP & Perlindungan Privasi Medis Staf
- `FR-LEV-12`: Pegawai yang mengalami sakit dapat mengajukan izin sakit mandiri 1 hari kerja melalui ESS dengan melampirkan surat pernyataan diri (`HRD-DEC-112`). `[EXISTING / REUSE]`
- `FR-LEV-13`: Pengajuan cuti sakit 2 hari kerja berturut-turut atau lebih wajib mengunggah berkas pindaian Surat Keterangan Sakit resmi dokter ber-SIP sah (`HRD-DEC-112`). `[MISSING / NEW]`
- `FR-LEV-14`: Sistem memvalidasi kelengkapan metadata surat dokter (nama dokter pemeriksa, nomor SIP, nama faskes penerbit, tanggal mulai dan selesai istirahat) sebelum form dapat disubmit (`HRD-DEC-112`). `[MISSING / NEW]`
- `FR-LEV-15`: Diagnosa klinis penyakit pegawai dimaskir dari atasan langsung; atasan hanya melihat status keabsahan surat dokter dan durasi istirahat demi kepatuhan privasi K3RS (`HRD-DEC-112`). `[MISSING / NEW]`

#### `EPIC HRD-42`: Penanganan Situasi Darurat Rumah Sakit (*Leave Recall*) & Pemulihan Hak Cuti Otomatis
- `FR-LEV-16`: Kepala Ruangan atau Kepala Instalasi dapat menginisiasi pemanggilan kembali pegawai yang sedang cuti aktif (*Leave Recall*) saat terjadi kondisi darurat RS (KLB, bencana, lonjakan pasien kritis) (`HRD-DEC-113`). `[EXTEND]`
- `FR-LEV-17`: Permohonan leave recall wajib memperoleh persetujuan administratif resmi dari HR Manager di sistem sebelum dapat dieksekusi (`HRD-DEC-113`, menutup `HRD-Q-36`). `[MISSING / NEW]`
- `FR-LEV-18`: Saat persetujuan recall diberikan oleh HR Manager, sistem secara otomatis menghitung sisa hari cuti yang belum dijalankan dan memulihkannya ke saldo pegawai dengan transaksi `RecallRestore` pada buku besar (`HRD-DEC-113`). `[MISSING / NEW]`
- `FR-LEV-19`: Eksekusi recall secara otomatis mengirimkan event pemulihan jadwal shift pegawai ke Roster 06 (`INT-HR-46`) agar siap dijadwalkan kembali tanpa penalti absensi (`HRD-DEC-113`). `[MISSING / NEW]`
- `FR-LEV-20`: Seluruh riwayat pemanggilan dinas darurat terdokumentasi dalam jejak audit kepatuhan ketenagakerjaan rumah sakit (`HRD-DEC-113`). `[MISSING / NEW]`

#### `EPIC HRD-43`: Rekonsiliasi Akhir Cuti Penggajian Retrospektif, Immutabilitas Cuti Selesai & Penutupan Celah `/reverse`
- `FR-LEV-21`: Permohonan cuti yang masih menggantung (*WaitingApproval*) pada tanggal cut-off payroll tidak memotong hak gaji pegawai pada bulan berjalan (`HRD-DEC-114`). `[MISSING / NEW]`
- `FR-LEV-22`: Permohonan cuti yang baru disetujui setelah tanggal cut-off payroll otomatis ditandai `IsPendingPayrollReconciliation = true` untuk diperhitungkan penyesuaiannya secara retrospektif pada payroll bulan berikutnya (`HRD-DEC-114`). `[MISSING / NEW]`
- `FR-LEV-23`: Seluruh permohonan cuti yang telah selesai dijalani (*status: Completed*) dan telah ditutup dalam periode payroll berstatus *Immutable* dan ditolak keras bila dipanggil via endpoint `/reverse` (HTTP 409 Conflict per `HRD-DEC-115` menutup `HRD-Q-35`). `[REPAIR]`
- `FR-LEV-24`: Koreksi kesalahan data cuti historis masa lampau hanya dapat dilakukan oleh HR Manager melalui transaksi penyesuaian saldo resmi `TrxLeaveAdjustment` dengan kewajiban mengisi nomor dokumen audit dan alasan tertulis (`HRD-DEC-115`). `[EXTEND]`
- `FR-LEV-25`: Penutupan periode rekonsiliasi cuti bulanan menghasilkan potret snapshot terkunci (*read-only*) untuk diserahkan ke Sub-Modul 10 Payroll (`HRD-DEC-114`). `[MISSING / NEW]`

### 26.4 Skenario UAT Slice `S-B2`

#### Skenario UAT-LEV-01: Alokasi Saldo 12 Hari, Carry-Forward Max 6 Hari & Kedaluwarsa 30 Juni (Jalur Berhasil)
- **Aktor:** Budi (Staf Administrasi) dan HR Leave Scheduler.
- **Langkah:**
  1. Pada 1 Januari 2026, Budi memiliki sisa cuti tahun 2025 sebanyak 8 hari.
  2. Scheduler akhir tahun berjalan: sistem membatasi saldo bawaan (*carry-forward*) maksimal 6 hari kerja, dan mencatat sisa 2 hari sebagai `Forfeiture` (Hangus) di `TrxLeaveBalanceLedger`. Budi juga menerima hak baru 2026 sebanyak 12 hari (Total saldo dapat dipakai = 18 hari).
  3. Hingga tanggal 30 Juni 2026, Budi hanya menggunakan 4 hari dari kuota carry-forward (tersisa 2 hari bawaan belum terpakai).
  4. Pada 30 Juni 2026 pukul 23:59:59 WIB, scheduler tengah tahun berjalan.
- **Hasil yang Diharapkan:** Sistem otomatis menghanguskan 2 hari saldo bawaan yang tersisa (`CarryForwardForfeiture`). Saldo Budi per 1 Juli 2026 kembali murni sebesar sisa hak tahun 2026 (12 hari).

#### Skenario UAT-LEV-02: Pengajuan Cuti ESS dengan Safe Staffing Coverage Warning (Jalur Peringatan & Mitigasi)
- **Aktor:** Suster Siti (Perawat Rawat Inap) dan Ibu Ratna (Kepala Ruangan).
- **Langkah:**
  1. Siti mengajukan cuti tahunan tanggal 15–16 November 2026 melalui ESS.
  2. Roster terbit unit menunjukkan bahwa pada shift pagi 15 November, ada 4 perawat bertugas (standar minimal aman: 4 perawat). Jika Siti cuti, staf bertugas tersisa 3 orang.
  3. Sistem memunculkan banner oranye pada Approval Inbox Ibu Ratna: *"Coverage Warning: Tenaga dinas tersisa 3 orang (di bawah kuota minimal 4 orang)"*.
  4. Ibu Ratna berkoordinasi dengan perawat cadangan (Suster Maya) untuk bertukar shift, lalu menekan tombol **Approve with Mitigation** dan mengisi catatan: *"Disetujui, digantikan Suster Maya pada shift pagi 15 Nov"*.
- **Hasil yang Diharapkan:** Permohonan cuti Siti berstatus `Approved`. Audit trail mencatat persetujuan dengan status mitigasi ketersediaan staf lengkap beserta nama perawat pengganti.

#### Skenario UAT-LEV-03: Pengajuan Cuti Sakit $\ge 2$ Hari & Perlindungan Privasi Medis K3RS (Jalur Privasi Medis)
- **Aktor:** Anton (Radiografer), Pak Heru (Koordinator Penunjang Medis), dan dr. Linda (Dokter K3RS).
- **Langkah:**
  1. Anton sakit demam berdarah dan mengajukan cuti sakit 3 hari (10–12 November) via ESS.
  2. Sistem mewajibkan Anton mengunggah berkas Surat Keterangan Sakit dokter resmi berstempel RS dan nomor SIP dokter.
  3. Pak Heru (atasan langsung) membuka kotak masuk approval: Pak Heru hanya dapat melihat durasi istirahat 3 hari, nama Klinik Sehat, dan SIP dokter penerbit, sementara diagnosa medis disamarkan (*Masked: Terlindungi Privasi Medis*). Pak Heru menyetujui izin istirahat Anton.
  4. dr. Linda (Tim K3RS / HR Admin Medis) membuka layar verifikasi kesehatan kerja dan dapat meninjau berkas medis asli untuk tindak lanjut K3RS.
- **Hasil yang Diharapkan:** Cuti sakit Anton sah disetujui, hak saldo cuti tahunan Anton tidak berkurang (karena jenis cuti sakit non-deductible), dan kerahasiaan diagnosa penyakit terlindungi dari atasan langsung.

#### Skenario UAT-LEV-04: Pemanggilan Cuti Darurat (Leave Recall) & Pemulihan Saldo Otomatis (Jalur Darurat RS)
- **Aktor:** Suster Dewi (Perawat ICU), Pak Joko (Kepala Ruangan ICU), dan Ibu Susan (HR Manager).
- **Langkah:**
  1. Dewi sedang menjalani cuti tahunan selama 5 hari kerja (10–14 November). Saldo cuti Dewi telah terpotong 5 hari.
  2. Pada tanggal 12 November terjadi bencana kecelakaan massal, ICU membutuhkan tambahan perawat mahir ventilator segera.
  3. Pak Joko menginisiasi **Leave Recall** di sistem untuk memanggil Dewi kembali bertugas mulai tanggal 13 November (sisa cuti 2 hari batal dinikmati).
  4. Permohonan recall masuk ke persetujuan Ibu Susan (HR Manager). Ibu Susan menyetujui recall darurat tersebut.
- **Hasil yang Diharapkan:** Sistem otomatis menerbitkan SK recall, memposting transaksi kredit `RecallRestore` sebesar +2 hari ke `TrxLeaveBalanceLedger` Dewi, dan jadwal dinas Dewi di Roster 06 tanggal 13–14 November berubah menjadi dinas aktif (*Scheduled*) tanpa penalti keterlambatan atau mangkir.

#### Skenario UAT-LEV-05: Penguncian Immutabilitas Cuti Selesai & Penutupan Celah `/reverse` (Jalur Integritas Audit)
- **Aktor:** Doni (HR Officer) dan Pak Tono (HR Manager).
- **Langkah:**
  1. Permohonan cuti Ahmad pada bulan Oktober telah berstatus `Completed` dan periode payroll Oktober telah berstatus `Closed`.
  2. Doni mencoba mengirimkan request HTTP `POST /api/v1/corporate/human-resource/leave/execution/ahm-01/reverse` untuk membatalkan cuti Ahmad yang sudah lampau.
  3. Sistem menolak pemanggilan tersebut dengan kode respon `409 Conflict`: *"Operasi ditolak: Permohonan cuti telah Completed dan terkunci dalam periode penutupan payroll. Pembatalan bebas dilarang (HRD-DEC-115)"*.
  4. Setelah diverifikasi ada audit rekam medis resmi, Pak Tono (HR Manager) membuka `LeaveAdjustmentController`, menginput nomor SK Audit `AUD-2026-HR-089`, dan menyesuaikan saldo Ahmad melalui jalur audit resmi.
- **Hasil yang Diharapkan:** Celah bypass pembatalan cuti lampau berhasil diblokir total, konsistensi penggajian terjaga, dan setiap penyesuaian memiliki bukti nomor dokumen audit yang valid.

### 26.5 Definition of Done (DoD) Slice `S-B2`

| No | Kriteria Selesai (DoD) | Bukti Pengujian |
| :---: | --- | --- |
| 1 | Seluruh transaksi penambahan, pemakaian, pembatalan, dan recall saldo cuti dicatat pada `TrxLeaveBalanceLedger` | UAT-LEV-01 & verifikasi tabel ledger database |
| 2 | Scheduler akhir tahun membatasi carry-forward maksimal 6 hari kerja dan menghanguskan sisanya | UAT-LEV-01 & unit test runner carry-forward |
| 3 | Scheduler tengah tahun (30 Juni) otomatis menghanguskan saldo bawaan carry-forward yang belum terpakai | UAT-LEV-01 & verifikasi saldo per 1 Juli |
| 4 | Pengajuan cuti staf klinis memicu evaluasi kuota ketersediaan staf (*Safe Staffing Coverage Warning*) ke Roster 06 | UAT-LEV-02 & verifikasi payload warning approval |
| 5 | Kepala Ruangan dapat menyetujui cuti kritis dengan aksi *Approve with Mitigation* dan catatan staf pengganti | UAT-LEV-02 & verifikasi audit trail persetujuan |
| 6 | Pengajuan cuti sakit $\ge 2$ hari menolak submit tanpa unggah surat keterangan dokter ber-SIP | UAT-LEV-03 & pengujian form validasi |
| 7 | Diagnosa medis surat sakit dimaskir dari atasan langsung dan hanya dapat diakses oleh K3RS / HR Medis | UAT-LEV-03 & verifikasi otorisasi API dokumen |
| 8 | Leave recall darurat wajib melalui persetujuan HR Manager dan otomatis memulihkan saldo sisa ke ledger | UAT-LEV-04 & verifikasi posting transaksi recall restore |
| 9 | Permohonan cuti pending saat cut-off payroll direkonsiliasi secara retrospektif pada payroll berikutnya | Verifikasi snapshot data rekonsiliasi payroll |
| 10 | Endpoint `/reverse` mengembalikan HTTP 409 Conflict bila cuti telah berstatus Completed / Payroll Closed | UAT-LEV-05 & integration test guard reverse |

### 26.6 Urutan Pengiriman Gelombang MVP

| Gelombang | Cakupan Kemampuan | Epic Terkait | Prasyarat |
| --- | --- | --- | --- |
| `MVP-24` | Buku besar saldo cuti `TrxLeaveBalanceLedger`, pembentukan hak 12 hari, aturan carry-forward 6 hari, dan scheduler kedaluwarsa 30 Juni | `EPIC HRD-39` | Sub-Modul 07 (`MVP-21`–`MVP-23`) selesai |
| `MVP-25` | Formulir pengajuan cuti ESS (half-day 0.5 hari), evaluasi kuota *Safe Staffing Coverage Warning*, approval atasan, dan alur cuti sakit privasi dokter K3RS | `EPIC HRD-40`, `EPIC HRD-41` | `MVP-24` selesai & Sub-Modul 06 terintegrasi |
| `MVP-26` | Prosedur *Leave Recall* darurat (Kepala Unit & HR Manager), pemulihan saldo otomatis, rekonsiliasi retrospektif payroll, dan penutupan celah `/reverse` | `EPIC HRD-42`, `EPIC HRD-43` | `MVP-25` selesai |

### 26.7 Traceability Keputusan ke PRD

| Keputusan / Asumsi | Epic & Kriteria DoD |
| --- | --- |
| `HRD-DEC-110` | `EPIC HRD-39`, `FR-LEV-01` s.d. `FR-LEV-05`, UAT-LEV-01, DoD butir 1, 2, 3 (Saldo 12 hari & carry-forward 6 hari s.d. 30 Juni) |
| `HRD-DEC-111` | `EPIC HRD-40`, `FR-LEV-09`, `FR-LEV-10`, UAT-LEV-02, DoD butir 4, 5 (Safe Staffing Coverage Warning) |
| `HRD-DEC-112` | `EPIC HRD-41`, `FR-LEV-12` s.d. `FR-LEV-15`, UAT-LEV-03, DoD butir 6, 7 (Cuti sakit $\ge 2$ hari surat dokter & privasi medis) |
| `HRD-DEC-113` | `EPIC HRD-42`, `FR-LEV-16` s.d. `FR-LEV-20`, UAT-LEV-04, DoD butir 8 (Leave recall darurat & pemulihan saldo otomatis) |
| `HRD-DEC-114` | `EPIC HRD-43`, `FR-LEV-21`, `FR-LEV-22`, `FR-LEV-25`, DoD butir 9 (Rekonsiliasi retrospektif payroll) |
| `HRD-DEC-115` (`HRD-Q-35`) | `EPIC HRD-43`, `FR-LEV-23`, `FR-LEV-24`, UAT-LEV-05, DoD butir 10 (Penguncian immutabilitas cuti selesai & blokir /reverse) |
| `HRD-ASM-19` | `FR-LEV-06` (Cuti bersama mengurangi hak tahunan) |
| `HRD-ASM-20` | `EPIC HRD-40`, `FR-LEV-07` (Cuti setengah hari half-day 0.5 hari kerja) |
| `HRD-ASM-21` | `EPIC HRD-40`, `FR-LEV-11` (Pre-leave cancellation pemulihan saldo 100%) |
| `HRD-ASM-22` | Batasan validasi durasi berurutan max 12 hari kerja |


---

## 27. Bagian `S-B3` — PRD to MVP Administrasi Lembur Pegawai Rumah Sakit (Sub-modul 09, `last_changed_in` `v13`, 4 November 2026)

Dokumen ini mendefinisikan batas rilis pertama (MVP), rincian kebutuhan fungsional (*functional requirements*), skenario uji penerimaan pengguna (*User Acceptance Testing*), *Definition of Done*, dan pentahapan pengiriman untuk **Slice `S-B3` — Administrasi Lembur Pegawai Rumah Sakit (Sub-modul 09)**.

### 27.1 Batas MVP Sub-Modul 09 (`S-B3`)

* **Titik Mulai (Start Boundary):** Pegawai atau Kepala Ruangan mengajukan permohonan SPKL terencana pra-dinas (minimal H-1) atau staf medis melaporkan klaim lembur darurat tindakan cito medis pasca-dinas (maksimal H+1 hari kerja) di portal ESS.
* **Titik Akhir (End Boundary):** Seluruh jam lembur diverifikasi secara otomatis terhadap bukti fisik mesin presensi biometrik Sub-modul 07, hak libur pengganti (CTO) diposting ke buku besar saldo cuti Sub-modul 08, dan berkas lembur bulanan terkunci resmi diserahkan ke Sub-modul 10 Payroll, dengan klaim pending direkonsiliasi secara retrospektif pada periode bulan berikutnya.

---

### 27.2 Kemampuan MUST HAVE (`HRD-CAP-08`)

1. **Dual-Mode SPKL Initiation (`HRD-DEC-116`):** Pengajuan lembur terencana pra-dinas H-1 dan pelaporan pasca-dinas lembur darurat medis cito maksimal H+1 kerja dengan nomor rekam medis wajib.
2. **Kepatuhan Batas Lembur PP 35/2021 (`HRD-DEC-117`):** Penegakan kuota baku maksimal 4 jam per hari dan 18 jam per minggu (*hard block* lembur biasa), serta alur eskalasi Direktur Pelayanan Medik untuk darurat penyelamatan nyawa pasien.
3. **Pilihan Kompensasi Fleksibel Finansial vs CTO (`HRD-DEC-118`):** Pembayaran uang lembur berjenjang (1.5x / 2x) atau konversi menjadi hak libur pengganti (*Compensatory Time Off*) berlaku 90 hari, dengan pengecualian lembur manajerial (*exempt*).
4. **Verifikasi Silang Biometrik & Min-Duration Rule (`HRD-DEC-119`):** Pengakuan durasi lembur berdasar nilai terkecil antara permohonan SPKL dan catatan mesin presensi, dilengkapi penanda *Flagged Discrepancy* dan investigasi manual HR.
5. **Pemisahan Peran & Anti-Self-Approval (`HRD-DEC-120`):** Alur persetujuan operasional ruangan dan verifikasi administratif HRD, dengan larangan mutlak menyetujui pengajuan lembur sendiri.
6. **Penutupan Periode & Rekonsiliasi Retrospektif (`HRD-DEC-121`):** Penguncian snapshot lembur pada cut-off bulanan (tgl 20) dan perhitungan penyesuaian retrospektif pada payroll berikutnya untuk klaim yang disetujui terlambat.

---

### 27.3 Kemampuan yang Ditunda (POST-MVP)

1. *Integrasi Mesin Katering Lembur Otomatis:* Pengeluaran voucher makanan lembur digital di kantin RS (ditunda ke rilis lanjutan pasca-payroll stabil).
2. *Prediksi Lembur Berbasis AI:* Prediksi kebutuhan lembur perawat rawat inap menggunakan machine learning berdasarkan tren sensus pasien (ditunda ke fase Enterprise Hospital Intelligence).
3. *Mobile Push Notification untuk Approval Lembur Darurat:* Notifikasi instan push mobile app (sementara menggunakan in-app notification dan portal inbox web).

---

### 27.4 Epic & Kebutuhan Fungsional (Functional Requirements)

#### `EPIC HRD-44` — Pengajuan & Penerbitan SPKL Dual-Mode
* **`FR-OT-01` [EXISTING / EXTEND]:** Sistem menyediakan formulir pengajuan SPKL terencana pra-dinas minimal H-1 sebelum waktu dinas lembur dimulai (`HRD-DEC-116`).
* **`FR-OT-02` [MISSING / NEW]:** Sistem menyediakan formulir pelaporan klaim darurat medis pasca-dinas cito (bedah OK, resusitasi IGD, ICU) maksimal H+1 hari kerja (`HRD-DEC-116`).
* **`FR-OT-03` [MISSING / NEW]:** Sistem mewajibkan pengisian Nomor Rekam Medis / Nomor Insiden Darurat dan ringkasan tindakan klinis pada setiap klaim cito darurat (`HRD-DEC-116`).
* **`FR-OT-04` [EXISTING / EXTEND]:** Sistem mendukung inisiasi SPKL kelompok (*team overtime*) oleh Kepala Ruangan dengan satu pengajuan induk untuk banyak nakes pelaksana (`HRD-ASM-26`).
* **`FR-OT-05` [MISSING / NEW]:** Sistem menolak secara otomatis klaim pasca-dinas yang diserahkan melebihi batas waktu H+1 hari kerja (`SubmissionDeadlinePassed`).

#### `EPIC HRD-45` — Penegakan Batas Maksimal Lembur PP 35/2021 & Eskalasi Darurat Klinis
* **`FR-OT-06` [MISSING / NEW]:** Sistem menghitung akumulasi jam lembur harian dan mingguan per staf aktif secara realtime (`HRD-DEC-117`).
* **`FR-OT-07` [MISSING / NEW]:** Sistem memblokir otomatis (*hard-block*) pengajuan lembur terencana biasa yang menyebabkan jam lembur harian > 4 jam atau mingguan > 18 jam (`HRD-DEC-117`).
* **`FR-OT-08` [MISSING / NEW]:** Sistem menyediakan alur eskalasi darurat klinis ke Direktur Pelayanan Medik jika klaim cito darurat melampaui batas kuota PP 35/2021 (`HRD-DEC-117`).
* **`FR-OT-09` [MISSING / NEW]:** Sistem menampilkan indikator risiko kelelahan nakes (*fatigue risk*) pada kotak masuk approval Direktur Pelayanan Medik (`HRD-DEC-117`).
* **`FR-OT-10` [MISSING / NEW]:** Sistem mencatat audit otorisasi khusus dispensasi darurat yang diterbitkan Direktur Pelayanan Medik.

#### `EPIC HRD-46` — Verifikasi Silang Kehadiran Biometrik & Penanganan Discrepancy
* **`FR-OT-11` [MISSING / NEW]:** Sistem secara otomatis mencocokkan jam klaim SPKL terhadap log mesin presensi biometrik aktual Sub-modul 07 (`INT-HR-50`).
* **`FR-OT-12` [MISSING / NEW]:** Sistem menerapkan aturan nilai terkecil (*Min-Duration Rule*): durasi dihitung dari `min(spkl_claimed_minutes, biometric_excess_minutes)` (`HRD-DEC-119`).
* **`FR-OT-13` [MISSING / NEW]:** Sistem menerapkan pembulatan kelipatan 30 menit ke bawah, dan durasi fisik < 30 menit bernilai 0 jam kompensasi (`HRD-ASM-23`).
* **`FR-OT-14` [MISSING / NEW]:** Sistem menandai status *Flagged Discrepancy* bila selisih jam klaim vs mesin > 30 menit atau pegawai lupa tap biometrik (`HRD-DEC-119`).
* **`FR-OT-15` [MISSING / NEW]:** Sistem menyediakan fasilitas investigasi manual HR Admin dengan kewajiban unggah berkas logbook dinas ruangan fisik sebelum klaim anomali disetujui.

#### `EPIC HRD-47` — Tata Kelola Kompensasi: Finansial vs Compensatory Time Off (CTO)
* **`FR-OT-16` [EXISTING / EXTEND]:** Sistem memungkinkan pegawai memilih bentuk kompensasi: Uang Lembur Finansial atau Cuti Pengganti (CTO) pada saat submit SPKL (`HRD-DEC-118`).
* **`FR-OT-17` [MISSING / NEW]:** Sistem menghitung pengali uang lembur reguler (1.5x jam pertama, 2x jam kedua dst.) dan tarif khusus hari libur resmi RS (`HRD-DEC-118`).
* **`FR-OT-18` [MISSING / NEW]:** Sistem mengonversi durasi lembur menjadi kuota CTO dan memposting transaksi mutasi kredit ke `TrxLeaveBalanceLedger` Sub-modul 08 (`INT-HR-49`).
* **`FR-OT-19` [MISSING / NEW]:** Sistem menetapkan masa kedaluwarsa kuota CTO selama tepat 90 hari kalender sejak tanggal terbit (`HRD-DEC-118`, `HRD-ASM-25`).
* **`FR-OT-20` [MISSING / NEW]:** Scheduler sistem otomatis menghanguskan sisa kuota CTO yang belum digunakan setelah melewati 90 hari kalender tanpa kompensasi uang tunai.

#### `EPIC HRD-48` — Penutupan Periode Cut-off Bulanan, Rekonsiliasi Retrospektif, & Serah Terima Payroll
* **`FR-OT-21` [EXISTING / EXTEND]:** Sistem mengunci periode transaksi lembur bulanan pada tanggal cut-off (tgl 20) menjadi *CutoffLocked* (`HRD-DEC-121`).
* **`FR-OT-22` [MISSING / NEW]:** Sistem menandai seluruh SPKL yang belum selesai diverifikasi saat cut-off sebagai `PendingNextReconciliation` (`HRD-DEC-121`).
* **`FR-OT-23` [MISSING / NEW]:** Sistem menyusun paket rekonsiliasi retrospektif atas lembur periode lampau yang disetujui terlambat untuk disertakan pada slip gaji periode berjalan (`HRD-DEC-121`).
* **`FR-OT-24` [EXISTING / EXTEND]:** Sistem membentuk berkas snapshot immutable serah terima data lembur terverifikasi ke Sub-modul 10 Payroll (`INT-HR-52`).
* **`FR-OT-25` [MISSING / NEW]:** Sistem memblokir mutlak penambahan, perubahan, atau pembatalan SPKL pada periode lembur yang telah terkunci atau ditutup (`Closed`).

---

### 27.5 Skenario UAT (User Acceptance Testing)

#### `UAT-OT-01`: Pengajuan SPKL Pra-Dinas Terencana & Validasi Kuota PP 35/2021
* **Aktor:** Bidan Siti (Staf Rawat Inap), Ibu Ani (Kepala Ruangan Kebidanan).
* **Jalur Sukses:**
  1. Pada tanggal 10 November pukul 10:00 WIB, Bidan Siti mengajukan SPKL pra-dinas untuk tanggal 11 November durasi 3 jam (16:00 - 19:00 WIB) karena ada persiapan akreditasi ruangan.
  2. Akumulasi jam lembur Siti minggu ini tercatat 6 jam; sistem mengevaluasi total menjadi 9 jam (di bawah batas maksimal 18 jam/minggu). Validasi kuota lulus.
  3. Pengajuan masuk ke kotak masuk Ibu Ani (Kepala Ruangan). Ibu Ani menyetujui. SPKL berstatus resmi `ApprovedForWork`.
* **Jalur Gagal (Pelanggaran Kuota PP 35/2021):**
  1. Pada tanggal 12 November, Siti mengajukan kembali SPKL terencana durasi 5 jam dalam satu hari.
  2. Sistem menolak dengan pesan kesalahan baku: *"Pengajuan lembur terencana harian melebihi batas maksimal 4 jam per hari sesuai regulasi PP 35/2021."* Form ditolak sistem sebelum tersubmit.

#### `UAT-OT-02`: Pelaporan Klaim Lembur Darurat Medis Cito & Penegakan Batas H+1
* **Aktor:** dr. Budi (Dokter IGD), dr. Hendra (Direktur Pelayanan Medik).
* **Jalur Sukses:**
  1. Pada shift malam 14 November, terjadi kecelakaan bus massal. dr. Budi harus memperpanjang dinas selama 5 jam untuk tindakan resusitasi kritis hingga pukul 03:00 WIB dini hari.
  2. Pada tanggal 15 November pagi (H+1), dr. Budi menginput klaim lembur pasca-dinas kategori `EmergencyCito` dengan nomor rekam medis bencana `RM-CITO-20261114-089` dan uraian tindakan resusitasi.
  3. Karena durasi 5 jam melampaui batas harian reguler 4 jam, sistem otomatis mengalihkan klaim ke kotak masuk eskalasi Direktur Pelayanan Medik.
  4. dr. Hendra meninjau indikator kelelahan, menyetujui klaim darurat medis, dan sistem merekam persetujuan dispensasi darurat.
* **Jalur Gagal (Klaim Terlambat > H+1 Kerja):**
  1. Perawat Doni mencoba melaporkan klaim lembur operasi cito tanggal 10 November pada tanggal 14 November (melebihi H+1 kerja).
  2. Sistem menolak dengan pesan kesalahan baku: *"Batas waktu pelaporan klaim lembur darurat medis pasca-dinas telah berakhir (maksimal H+1 hari kerja)."*

#### `UAT-OT-03`: Verifikasi Silang Biometrik Presensi & Min-Duration Rule
* **Aktor:** Staf Laboratorium Eko, HR Officer Rina.
* **Jalur Sukses:**
  1. Eko mengajukan SPKL 3 jam (17:00 - 20:00 WIB) dan disetujui atasan.
  2. Berdasarkan data mesin tap biometrik Sub-modul 07, Eko melakukan tap pulang aktual pada pukul 19:40 WIB (kelebihan fisik 2 jam 40 menit).
  3. Service verifikasi biometrik menerapkan aturan nilai terkecil: `Min(180 menit, 160 menit) = 160 menit`.
  4. Pembulatan 30 menit ke bawah diterapkan: 160 menit dibulatkan menjadi 150 menit (2 jam 30 menit).
  5. Sistem mencatat `VerifiedPayableMinutes = 150` dengan status `AutoVerified`.
* **Jalur Gagal (Anomali Lupa Tap Mesin / Discrepancy):**
  1. Staf Farmasi Dewi memiliki SPKL disetujui 3 jam, tetapi lupa melakukan tap presensi saat pulang dinas lembur.
  2. Engine verifikasi mendeteksi ketiadaan tap pulang fisik dan otomatis menandai klaim sebagai `FlaggedDiscrepancy: MissingBiometric`.
  3. Klaim ditahan dari pembayaran payroll hingga HR Officer Rina menerima bukti fisik logbook dinas farmasi bertanda tangan atasan dan melakukan *Manual Adjustment*.

#### `UAT-OT-04`: Kompensasi Hak Libur Pengganti (CTO) & Siklus Kedaluwarsa 90 Hari
* **Aktor:** Perawat ICU Maya.
* **Jalur Sukses:**
  1. Maya lembur cito ICU selama 8 jam terverifikasi pada 1 November dan memilih kompensasi `CompensatoryTimeOff`.
  2. Sistem otomatis mengonversi 8 jam lembur menjadi 1 hari libur pengganti (CTO) dan mengirimkan mutasi kredit ke `TrxLeaveBalanceLedger` Sub-modul 08 (`INT-HR-49`).
  3. Maya melihat tambahan 1 hari saldo CTO di portal ESS dengan tanggal kedaluwarsa tercatat 30 Januari 2027 (H+90 hari kalender).
* **Jalur Evaluasi Kedaluwarsa (Expired Runner):**
  1. Hingga tanggal 30 Januari 2027 pukul 23:59:59 WIB, Maya belum menggunakan hak libur CTO tersebut.
  2. Pada 31 Januari 2027 pukul 00:05 WIB, background scheduler mengeksekusi runner kedaluwarsa; saldo CTO Maya dicatat hangus (*Expired*) tanpa kompensasi uang tunai.

#### `UAT-OT-05`: Penutupan Cut-off Payroll & Rekonsiliasi Retrospektif
* **Aktor:** HR Payroll Officer Diana.
* **Jalur Sukses:**
  1. Pada tanggal 20 November pukul 23:59:59 WIB, Diana menutup periode lembur November 2026. Seluruh SPKL terverifikasi bersih dikunci menjadi snapshot immutable.
  2. SPKL milik Bidan Ratih yang masih dalam tahap investigasi anomali biometrik otomatis ditandai `PendingNextReconciliation` dan ditahan dari slip gaji November.
  3. Pada 23 November, bukti logbook Ratih selesai diverifikasi HR Admin dan SPKL disetujui.
  4. Saat pembentukan batch payroll Desember 2026, sistem secara otomatis menarik SPKL Ratih sebagai rapel rekonsiliasi retrospektif pada slip gaji Desember.

---

### 27.6 Definition of Done (DoD) Slice `S-B3`

| No | Kriteria Selesai (DoD) | Bukti Pengujian |
| :---: | --- | --- |
| 1 | SPKL terencana menolak jam mulai di masa lampau dan menolak kuota harian > 4 jam / mingguan > 18 jam | UAT-OT-01 & pengujian form validasi |
| 2 | Klaim cito pasca-dinas mewajibkan nomor RM klinis dan menolak pelaporan > H+1 hari kerja | UAT-OT-02 & integrasi backend validation |
| 3 | Klaim cito melebihi kuota PP 35 otomatis dialihkan ke eskalasi Direktur Pelayanan Medik | UAT-OT-02 & verifikasi workflow eskalasi |
| 4 | Durasi lembur dihitung otomatis berdasar nilai terkecil antara SPKL dan tap mesin presensi biometrik | UAT-OT-03 & pengujian biometric verification service |
| 5 | Anomali selisih waktu > 30 menit atau lupa tap biometrik ditandai status `FlaggedDiscrepancy` | UAT-OT-03 & verifikasi discrepancy queue |
| 6 | Penyesuaian manual durasi anomali oleh HR Admin mewajibkan unggah bukti logbook dinas ruangan | UAT-OT-03 & pengujian drawer investigasi HR |
| 7 | Atasan langsung dilarang keras menyetujui pengajuan lemburnya sendiri (*Anti-Self-Approval*) | Pengujian validasi `VR-OT-03` & hierarki eskalasi |
| 8 | Kompensasi opsi CTO otomatis memposting mutasi kredit ke buku besar cuti Sub-modul 08 berkedaluwarsa 90 hari | UAT-OT-04 & verifikasi `INT-HR-49` |
| 9 | Penutupan cut-off bulanan mengunci seluruh SPKL terverifikasi menjadi snapshot *read-only/immutable* | UAT-OT-05 & integration test lock period |
| 10 | SPKL pending saat cut-off otomatis diperhitungkan sebagai rapel retrospektif pada payroll berikutnya | UAT-OT-05 & verifikasi handoff payload `INT-HR-52` |

---

### 27.7 Urutan Pengiriman Gelombang MVP

| Gelombang | Cakupan Kemampuan | Epic Terkait | Prasyarat |
| --- | --- | --- | --- |
| `MVP-27` | Skema database lembur, SPKL pra-dinas terencana H-1, kuota PP 35/2021, dan anti-self-approval | `EPIC HRD-44`, `EPIC HRD-45` | Sub-Modul 08 (`MVP-24`–`MVP-26`) selesai |
| `MVP-28` | Klaim darurat cito H+1, eskalasi Direktur Medis, verifikasi silang mesin biometrik presensi, dan anomali discrepancy | `EPIC HRD-45`, `EPIC HRD-46` | `MVP-27` selesai & Sub-Modul 07 terintegrasi |
| `MVP-29` | Konversi saldo libur pengganti CTO (90 hari), penutupan cut-off bulanan, rekonsiliasi retrospektif, dan handoff payroll | `EPIC HRD-47`, `EPIC HRD-48` | `MVP-28` selesai |

---

### 27.8 Traceability Keputusan ke PRD

| Keputusan / Asumsi | Epic & Kriteria DoD Terkait |
| --- | --- |
| `HRD-DEC-116` | `EPIC HRD-44`, `FR-OT-01` s.d. `FR-OT-05`, UAT-OT-01, UAT-OT-02, DoD butir 1, 2 |
| `HRD-DEC-117` | `EPIC HRD-45`, `FR-OT-06` s.d. `FR-OT-10`, UAT-OT-01, UAT-OT-02, DoD butir 1, 3 |
| `HRD-DEC-118` | `EPIC HRD-47`, `FR-OT-16` s.d. `FR-OT-20`, UAT-OT-04, DoD butir 8 |
| `HRD-DEC-119` | `EPIC HRD-46`, `FR-OT-11` s.d. `FR-OT-15`, UAT-OT-03, DoD butir 4, 5, 6 |
| `HRD-DEC-120` | `EPIC HRD-44`, `EPIC HRD-45`, UAT-OT-01, DoD butir 7 |
| `HRD-DEC-121` | `EPIC HRD-48`, `FR-OT-21` s.d. `FR-OT-25`, UAT-OT-05, DoD butir 9, 10 |
| `HRD-ASM-23` | `FR-OT-13` (Pembulatan 30 menit ke bawah) |
| `HRD-ASM-24` | `EPIC HRD-44`, `FR-OT-02` (Aktivasi lembur nakes on-call medis) |
| `HRD-ASM-25` | `EPIC HRD-47`, `FR-OT-19`, `FR-OT-20` (Kedaluwarsa CTO 90 hari kalender) |
| `HRD-ASM-26` | `EPIC HRD-44`, `FR-OT-04` (Inisiasi lembur kelompok/tim medis ruangan) |


---

## 28. Bagian S-B5 — PRD ke MVP Penggajian Pegawai Rumah Sakit (Sub-modul 10, last_changed_in v14, 5 November 2026)

Bagian ini mendokumentasikan Dokumen Kebutuhan Produk (*Product Requirements Document* / PRD) yang diterjemahkan menjadi batas rilis minimum (*Minimum Viable Product* / MVP) untuk **Slice `S-B5` — Penggajian Pegawai Rumah Sakit (Payroll Management)**.

### 28.1 Batas Rilis Pertama (MVP Scope)
* **Titik Awal (Inisiasi):** Sistem membaca dan memvalidasi kelengkapan penguncian data presensi kehadiran (Sub-07), cuti/izin (Sub-08), dan klaim lembur (Sub-09) yang telah terkunci cut-off per tanggal 20 bulan berjalan.
* **Titik Akhir (Delivery):** Paket serah terima instruksi pembayaran penggajian berintegritas tinggi (SHA-256) diserahkan ke Modul Keuangan (*Finance Handoff* `INT-HR-53`), serta penerbitan slip gaji digital mandiri berpassword dan berproteksi PIN 6-digit pada portal Employee Self-Service (ESS) per tanggal 25.
* **Batas Kewenangan (Out of Scope MVP sesuai `HRD-DEC-009`):**
  * Pencairan transfer uang riil ke bank pegawai, antarmuka internet banking/host-to-host bank korporat, dan pengeluaran kas (kewenangan eksklusif modul Finance & Kasir).
  * Pembuatan jurnal umum akuntansi (General Ledger) dan pembukuan akun beban gaji (kewenangan modul Akuntansi).
  * Pelaporan formulir SPT Masa PPh 21 unifikasi ke portal e-Bupot DJP Online (kewenangan Tim Pajak/Finance).

---

### 28.2 Daftar Kebutuhan Fungsional (Functional Requirements)

| Kode FR | Nama Kebutuhan Fungsional | Deskripsi Kebutuhan Bisnis | Rujukan Aturan / Keputusan |
| --- | --- | --- | --- |
| `FR-PAY-01` | Inisiasi Siklus Penggajian Bulanan | Payroll Officer dapat menginisiasi siklus penggajian baru untuk periode aktif pasca cut-off tgl 20. | `HRD-DEC-122` |
| `FR-PAY-02` | Precondition Gate Check Periode Kerja | Sistem memvalidasi bahwa periode presensi (07), cuti (08), dan lembur (09) telah terkunci (*Locked/Closed*). | `PAY-B1`, `VR-PAY-02` |
| `FR-PAY-03` | Snapshot Master Parameter Penggajian | Sistem mengambil salinan beku (*freeze snapshot*) atas grade gaji, tunjangan, dan batas plafon BPJS. | `HRD-DEC-122` |
| `FR-PAY-04` | Kalkulasi Penghasilan Tetap Pegawai | Menghitung gaji pokok, tunjangan jabatan struktural, tunjangan fungsional profesi, dan tunjangan medis. | `HRD-DEC-123` |
| `FR-PAY-05` | Penyerapan Data Lembur Terverifikasi Sub-09 | Menarik jam lembur sah terverifikasi dari Sub-modul 09 dan menghitung uang lembur sesuai PP 35/2021. | `INT-HR-52`, `PAY-B4` |
| `FR-PAY-06` | Penyerapan Premi Shift Malam Presensi Sub-07 | Menarik frekuensi dinas malam dari Sub-modul 07 dan mengakumulasi premi dinas malam secara akurat. | `INT-HR-36`, `HRD-DEC-123` |
| `FR-PAY-07` | Kalkulasi Iuran BPJS Kesehatan | Menghitung 1% iuran pekerja dan 4% pemberi kerja berdasar upah dengan pembatasan plafon resmi. | `VR-PAY-08` |
| `FR-PAY-08` | Kalkulasi Iuran BPJS Ketenagakerjaan | Menghitung JHT (2% pekerja, 3.7% RS), JKK (0.89%), JKM (0.3%), dan JP (1% pekerja berplafon). | `VR-PAY-09` |
| `FR-PAY-09` | Pemotongan Pajak PPh 21 Tarif TER | Menerapkan tabel Tarif Efektif Rata-Rata (Kategori A/B/C) PMK 168/2023 secara otomatis sesuai PTKP. | `INT-HR-56`, `HRD-DEC-123` |
| `FR-PAY-10` | Penyerapan Potongan Pinjaman & Restitusi Sub-11| Menarik tagihan angsuran pinjaman koperasi karyawan dan pinjaman darurat RS dari Sub-modul 11. | `INT-HR-55` |
| `FR-PAY-11` | Perhitungan Rapel & Penyesuaian Retrospektif | Menghitung penyesuaian susulan atas persetujuan lembur/kehadiran terlambat dari periode lalu. | `HRD-DEC-123` |
| `FR-PAY-12` | Evaluasi Variansi Anomali Gaji (*Variance Guard*)| Sistem membandingkan take-home pay dengan bulan lalu; menandai deviasi $\ge 20\%$ atau nominal $\ge$ Rp 2.000.000. | `HRD-DEC-124`, `PAY-W1` |
| `FR-PAY-13` | Otomasi Pembuatan Tiket Variance Alert | Membangkitkan tiket `trx_payroll_variance_alerts` berstatus `PendingJustification` saat anomali terdeteksi. | `HRD-DEC-124` |
| `FR-PAY-14` | Pengisian Justifikasi Audit Variansi | Payroll Officer wajib mengisi catatan alasan logis (minimum 20 karakter) pada setiap anomali yang muncul. | `VR-PAY-15` |
| `FR-PAY-15` | Gerbang Pemblokiran Submit Batch Variansi | Sistem menolak pengajuan approval batch jika masih terdapat tiket alert variansi yang berstatus pending. | `PAY-B5`, `VR-PAY-14` |
| `FR-PAY-16` | Workflow Otorisasi Tier 1 (Payroll Officer) | Officer mengunci draft perhitungan dan mengajukan review resmi kepada HR Manager (`WaitingHrReview`). | `HRD-DEC-125` |
| `FR-PAY-17` | Proteksi *Anti-Self-Approval* HR Manager | Sistem memblokir HR Manager menyetujui perhitungan gajinya sendiri; merutekan ke Direktur SDM. | `HRD-DEC-125`, `VR-PAY-17`|
| `FR-PAY-18` | Workflow Otorisasi Tier 3 & Gembok Permanen | Direktur SDM/Keuangan menyetujui batch penggajian; sistem mengunci permanen seluruh data (*Immutable*). | `VR-PAY-20` |
| `FR-PAY-19` | Rekapitulasi Finansial Penggajian per Unit | Menghasilkan rekap biaya gaji per instalasi, unit kerja, kelompok profesi nakes, dan akun pembebanan. | `FE-HRD-164` |
| `FR-PAY-20` | Pembentukan Paket Serah Terima Finance SHA-256 | Men-generate paket berkas `trx_payroll_handoff_batches` dengan verifikasi integritas checksum SHA-256. | `INT-HR-53`, `HRD-DEC-126`|
| `FR-PAY-21` | Transmisi Asinkron Serah Terima ke Finance | Mengirimkan berkas instruksi pencairan ke Finance dengan kebijakan *exponential backoff retry*. | `INT-HR-53` |
| `FR-PAY-22` | Penanganan Callback Penolakan & Rollback Terkendali| Menangani sinyal `PaymentRejected` dari Finance; membuka gembok terkendali ke status `NeedCorrection`. | `INT-HR-54`, `HRD-DEC-126`|
| `FR-PAY-23` | Otomasi Pelepasan Slip Gaji Digital ESS | Merilis slip gaji digital `trx_payroll_payslips` berstatus `Released` per tgl 25 atau pasca bayar Finance. | `HRD-DEC-127` |
| `FR-PAY-24` | Proteksi Autentikasi PIN Sekunder 6-Digit ESS | Mewajibkan input PIN 6-digit untuk membuka slip gaji; mengunci akses 30 menit jika 3 kali salah. | `HRD-DEC-127`, `VR-PAY-22`|
| `FR-PAY-25` | Unduh Slip Gaji PDF Ber-password & Watermark | Pegawai dapat mengunduh berkas slip gaji PDF resmi ber-password kombinasi tanggal lahir dan ber-watermark RS. | `HRD-DEC-127`, `VR-PAY-24`|

---

### 28.3 Rincian Epic MVP

#### EPIC HRD-49: Inisiasi Siklus Penggajian & Precondition Gate Check
Mencakup pembuatan siklus batch bulanan, validasi gerbang penguncian waktu kerja tgl 20, pembekuan parameter tarif master gaji, dan penolakan duplikasi periode.
* **Kebutuhan Terkait:** `FR-PAY-01`, `FR-PAY-02`, `FR-PAY-03`.
* **Kriteria Keberhasilan:** Payroll Officer tidak dapat membuat siklus baru sebelum presensi, cuti, dan lembur terkunci.

#### EPIC HRD-50: Engine Perhitungan Gaji, Tunjangan, Lembur, Premi Shift & Pajak TER
Mencakup engine kalkulasi otomatis seluruh komponen penghasilan kotor, potongan wajib regulasi pemerintah (BPJS Kesehatan, BPJS Ketenagakerjaan 4 program, PPh 21 TER PMK 168/2023), penarikan angsuran pinjaman Sub-11, dan penyesuaian retrospektif.
* **Kebutuhan Terkait:** `FR-PAY-04`, `FR-PAY-05`, `FR-PAY-06`, `FR-PAY-07`, `FR-PAY-08`, `FR-PAY-09`, `FR-PAY-10`, `FR-PAY-11`.
* **Kriteria Keberhasilan:** Seluruh perhitungan matematis presisi tanpa selisih pembulatan, mematuhi batas plafon upah regulasi, dan menghasilkan gaji bersih yang valid.

#### EPIC HRD-51: Payroll Variance Guard & Penyelesaian Anomali Justifikasi
Mencakup pendeteksian otomatis anomali perubahan gaji bersih $\ge 20\%$ atau $\ge$ Rp 2.000.000 terhadap histori bulan lalu, pembuatan tiket alert variansi, dan gerbang penolakan submission sebelum justifikasi dilengkapi.
* **Kebutuhan Terkait:** `FR-PAY-12`, `FR-PAY-13`, `FR-PAY-14`, `FR-PAY-15`.
* **Kriteria Keberhasilan:** Tidak ada lonjakan gaji yang lolos ke meja manajer tanpa catatan investigasi audit yang sah.

#### EPIC HRD-52: Otorisasi 3-Jenjang, Penguncian Immutable & Paket Serah Terima Finance
Mencakup alur persetujuan berjenjang (Officer $\rightarrow$ HR Manager $\rightarrow$ Direktur), penegakan aturan anti-self-approval, penguncian permanen batch, pembentukan paket serah terima Finance berintegritas SHA-256, dan mekanisme rollback callback terkendali.
* **Kebutuhan Terkait:** `FR-PAY-16`, `FR-PAY-17`, `FR-PAY-18`, `FR-PAY-19`, `FR-PAY-20`, `FR-PAY-21`, `FR-PAY-22`.
* **Kriteria Keberhasilan:** Pemisahan tugas (*Segregation of Duties*) terjamin mutlak, paket ke Finance anti-tamper dengan verifikasi checksum, dan status sinkron dengan Finance.

#### EPIC HRD-53: Distribusi Slip Gaji Digital ESS & Keamanan PIN Sekunder
Mencakup pelepasan slip gaji digital kepada seluruh pegawai rumah sakit pada portal ESS per tanggal penggajian resmi (tgl 25), proteksi PIN 6-digit dengan penalti lockout, serta fasilitas unduh dokumen PDF resmi.
* **Kebutuhan Terkait:** `FR-PAY-23`, `FR-PAY-24`, `FR-PAY-25`.
* **Kriteria Keberhasilan:** Informasi remunerasi bersifat konfidensial aman, pegawai dapat mengakses rincian gajinya secara mandiri, dan slip gaji ber-password.

---

### 28.4 Skenario Pengujian Penerimaan Pengguna (User Acceptance Testing - UAT)

#### UAT-PAY-01: Inisiasi Siklus Penggajian & Validasi Gerbang Waktu Kerja
* **Aktor:** Payroll Officer
* **Langkah:**
  1. Masuk ke menu HR Payroll Processing pada tanggal 21 November 2026.
  2. Klik tombol "Buat Siklus Payroll Baru" untuk periode `2026-11`.
  3. Sistem memeriksa status periode Sub-07 (Presensi), Sub-08 (Cuti), dan Sub-09 (Lembur).
* **Hasil yang Diharapkan:** Bila salah satu modul belum berstatus locked/closed, sistem menolak inisiasi dengan pesan kesalahan informatif. Bila seluruhnya telah terkunci, siklus batch berstatus `Draft` terbentuk dan parameter master gaji tersalin ke snapshot.

#### UAT-PAY-02: Kalkulasi Komponen Gaji, Lembur, Premi Shift & Pajak PPh 21 TER
* **Aktor:** Payroll Officer & Auditor Finansial
* **Langkah:**
  1. Jalankan tombol "Hitung Seluruh Pegawai" pada batch aktif.
  2. Periksa detail rincian penghasilan pada pegawai Dokter Spesialis Organik dan Perawat Rawat Inap Pelaksana Shift Malam.
  3. Verifikasi komponen lembur PP 35/2021, premi shift malam, potongan BPJS berplafon, dan potongan PPh 21 TER Kategori A/B/C.
* **Hasil yang Diharapkan:** Komponen penghasilan dan potongan terhitung akurat sesuai rumus master tanpa galat pembulatan; take-home pay mencerminkan penjumlahan dan pengurangan yang benar.

#### UAT-PAY-03: Deteksi Anomali Variansi & Pengisian Justifikasi Audit
* **Aktor:** Payroll Officer
* **Langkah:**
  1. Buka tab "Variansi & Anomali Gaji" pada layar kalkulasi batch.
  2. Temukan pegawai perawat yang memiliki deviasi kenaikan take-home pay +25% akibat intensitas lembur ruang isolasi.
  3. Upayakan klik tombol "Ajukan Review ke HR Manager" sebelum mengisi justifikasi.
  4. Isi catatan justifikasi: "Kenaikan take-home pay diakibatkan penugasan darurat lembur ruang isolasi sebanyak 28 jam (SPKL-202611-042)."
  5. Klik simpan dan ulangi pengajuan submit approval.
* **Hasil yang Diharapkan:** Sistem memblokir submission pada langkah 3 (HTTP 422). Pada langkah 5, status tiket alert berubah menjadi `Resolved` dan batch berhasil diajukan ke HR Manager.

#### UAT-PAY-04: Alur Otorisasi 3-Jenjang & Penguncian Permanen (Immutable)
* **Aktor:** HR Manager & Direktur SDM/Keuangan
* **Langkah:**
  1. HR Manager membuka menu Persetujuan Payroll dan memeriksa berkas batch `2026-11`.
  2. Sistem mendeteksi bahwa baris gaji HR Manager ada di dalam batch tersebut; HR Manager menyetujui batch secara umum namun flag otorisasi baris pribadinya dialihkan ke Direktur.
  3. Direktur SDM/Keuangan melakukan final sign-off approval.
* **Hasil yang Diharapkan:** Status batch beralih menjadi `ApprovedFinal`. Seluruh data batch, item gaji pegawai, dan rincian komponen terkunci permanen (*IsImmutable = true*); tombol hitung ulang atau sunting dinonaktifkan.

#### UAT-PAY-05: Serah Terima Finance SHA-256 & Distribusi Slip Gaji ESS Ber-PIN
* **Aktor:** Direktur SDM, Petugas Finance, & Pegawai Rumah Sakit (ESS)
* **Langkah:**
  1. Sistem memicu pembentukan paket `trx_payroll_handoff_batches` dengan Checksum SHA-256 dan mengirimkannya ke Finance.
  2. Finance mengirimkan callback `PaymentConfirmed` setelah transfer bank selesai.
  3. Pegawai login ke modul ESS pada tgl 25 November dan mengakses menu "Slip Gaji Saya".
  4. Pegawai memasukkan PIN sekunder 6-digit.
  5. Pegawai mengunduh berkas PDF slip gaji resmi.
* **Hasil yang Diharapkan:** Data handoff Finance cocok dengan nilai HRD. Slip gaji digital berstatus `Released`. Input PIN 6-digit yang benar menampilkan rincian slip gaji, dan berkas PDF terunduh berproteksi kata sandi tanggal lahir.

---

### 28.5 Definisi Selesai (Definition of Done - DoD)

| No | Kriteria Selesai (DoD) | Bukti Pengujian |
| :---: | --- | --- |
| 1 | Inisiasi batch payroll menolak pembuatan jika periode presensi, cuti, atau lembur belum berstatus terkunci cut-off tgl 20 | UAT-PAY-01 & pengujian `VR-PAY-02` |
| 2 | Inisiasi batch payroll menolak duplikasi periode penggajian yang sama untuk unit kerja / entitas RS yang sama | Pengujian constraint unik & `VR-PAY-01` |
| 3 | Perhitungan seluruh komponen penghasilan kotor, lembur, dan premi shift malam presisi tanpa selisih pembulatan | UAT-PAY-02 & unit test engine kalkulasi |
| 4 | Pemotongan iuran BPJS Kesehatan dan BPJS Ketenagakerjaan menerapkan batas plafon upah tertinggi regulasi resmi | UAT-PAY-02 & validasi `VR-PAY-08`, `VR-PAY-09` |
| 5 | Pemotongan PPh 21 bulanan menggunakan Tarif Efektif Rata-Rata (TER Kategori A, B, C) PMK 168/2023 secara tepat | UAT-PAY-02 & integrasi tabel master pajak `INT-HR-56` |
| 6 | Anomali variansi kenaikan/penurunan net pay $\ge 20\%$ atau nominal $\ge$ Rp 2.000.000 wajib dilengkapi catatan justifikasi | UAT-PAY-03 & validasi `VR-PAY-14`, `PAY-B5` |
| 7 | Alur otorisasi 3-jenjang menegakkan aturan pemisahan tugas (*Segregation of Duties*) dan anti-self-approval manajerial | UAT-PAY-04 & verifikasi workflow authorization |
| 8 | Persetujuan final Direksi mengunci seluruh baris data penggajian secara permanen (*IsImmutable = true*) tanpa opsi mutasi | UAT-PAY-04 & pengujian penolakan edit `PAY-B6` |
| 9 | Paket serah terima ke Finance membentuk checksum SHA-256 dan mendukung mekanisme rollback terkendali pasca callback | UAT-PAY-05 & pengujian integrasi `INT-HR-53`, `INT-HR-54` |
| 10 | Slip gaji digital ESS berstatus `Released` tgl 25 dan dilindungi autentikasi PIN 6-digit sekunder dengan penguncian 30 menit | UAT-PAY-05 & pengujian keamanan PIN `VR-PAY-22` |

---

### 28.6 Urutan Pengiriman Gelombang MVP

| Gelombang | Cakupan Kemampuan | Epic Terkait | Prasyarat |
| --- | --- | --- | --- |
| `MVP-30` | Skema database penggajian, precondition gate cut-off tgl 20, freeze parameter snapshot, dan engine kalkulasi gaji pokok, tunjangan, lembur, premi malam, BPJS, & PPh 21 TER | `EPIC HRD-49`, `EPIC HRD-50` | Sub-Modul 09 (`MVP-27`–`MVP-29`) selesai |
| `MVP-31` | Payroll variance guard, deteksi anomali deviasi $\ge 20\%$, alur pengisian justifikasi investigasi, dan alur otorisasi 3-jenjang dengan anti-self-approval | `EPIC HRD-51`, `EPIC HRD-52` | `MVP-30` selesai & master role siap |
| `MVP-32` | Penguncian immutable batch, paket serah terima Finance SHA-256, callback reconciler, dan penerbitan slip gaji digital ESS berproteksi PIN 6-digit | `EPIC HRD-52`, `EPIC HRD-53` | `MVP-31` selesai |
| `Verifikasi E2E`| Audit kesiapan menyeluruh, eksekusi acceptance test suite `AT-HRD-PAY-01` s.d. `AT-HRD-PAY-20`, dan konformansi arsitektur QBE | Seluruh Epic | `MVP-30` s.d. `MVP-32` selesai |

---

### 28.7 Traceability Keputusan ke PRD

| Keputusan / Asumsi | Epic & Kriteria DoD Terkait |
| --- | --- |
| `HRD-DEC-122` | `EPIC HRD-49`, `FR-PAY-01`, `FR-PAY-02`, `FR-PAY-03`, UAT-PAY-01, DoD butir 1, 2 |
| `HRD-DEC-123` | `EPIC HRD-50`, `FR-PAY-04` s.d. `FR-PAY-11`, UAT-PAY-02, DoD butir 3, 4, 5 |
| `HRD-DEC-124` | `EPIC HRD-51`, `FR-PAY-12` s.d. `FR-PAY-15`, UAT-PAY-03, DoD butir 6 |
| `HRD-DEC-125` | `EPIC HRD-52`, `FR-PAY-16` s.d. `FR-PAY-19`, UAT-PAY-04, DoD butir 7 |
| `HRD-DEC-126` | `EPIC HRD-52`, `FR-PAY-20` s.d. `FR-PAY-22`, UAT-PAY-05, DoD butir 8, 9 |
| `HRD-DEC-127` | `EPIC HRD-53`, `FR-PAY-23` s.d. `FR-PAY-25`, UAT-PAY-05, DoD butir 10 |
| `HRD-ASM-27` | `FR-PAY-06` (Nilai nominal baku premi dinas malam nakes) |
| `HRD-ASM-28` | `FR-PAY-09` (Basis penyesuaian PPh 21 Pasal 17 pada masa pajak Desember) |
| `HRD-ASM-29` | `FR-PAY-12` (Threshold deviasi variansi Rp 2.000.000 atau $\ge 20\%$) |
| `HRD-ASM-30` | `FR-PAY-24` (Kebijakan penguncian 30 menit pasca 3 kegagalan PIN) |

## 29. Bagian S-B6 — PRD ke MVP Restitusi & Pemotongan Penggajian Pegawai Rumah Sakit (Sub-modul 11, last_changed_in v15, 5 November 2026)

Bagian ini mendefinisikan Dokumen Kebutuhan Produk (*Product Requirements Document* - PRD) hingga Batas Rilis Pertama (*Minimum Viable Product* - MVP) untuk **Sub-modul 11. Restitusi & Integrasi Pemotongan Penggajian (Slice `S-B6`)**, mengikat keputusan `HRD-DEC-128` s.d. `HRD-DEC-133`, asumsi `HRD-ASM-31` s.d. `HRD-ASM-34`, serta batasan regulasi pemotongan upah Pasal 65 PP 36/2021 jo PP 49/2025.

---

### 29.1 Batas Rilis Pertama (MVP Scope)

- **Titik Mulai MVP:** Penarikan snapshot komitmen angsuran pinjaman aktif dari modul Keuangan pada tanggal 17 (H-3 cut-off) dan dibukanya pengajuan klaim kacamata/faskes luar mandiri di portal ESS pegawai.
- **Titik Akhir MVP:** Penegakan batas maksimal potongan 50% *Gross Pay* pada slip gaji bulanan tanggal 25, pengiriman callback realisasi pemotongan ke Keuangan, pelunasan utang penutupan PHK/Resign pada Exit Clearance (Sub-20), dan kliring voucher pencairan kasir darurat medis cito 24/7.
- **Kemampuan MUST HAVE yang Disertakan:**
  1. Penegakan otomatis batas potongan 50% *Gross Pay* dengan protokol pemangkasan berjenjang prioritas 1 s.d. 4 (`HRD-DEC-128`).
  2. Penjadwalan sisa tagihan melampaui plafon 50% ke bulan berikutnya (*carry-over*) tanpa denda bunga (`HRD-DEC-128`).
  3. Sinkronisasi dua arah saldo piutang pegawai dengan Departemen Keuangan via `INT-HR-57` dan `INT-HR-58` (`HRD-DEC-129`).
  4. Portal ESS pengajuan klaim restitusi kacamata kerja & faskes darurat lengkap dengan kuota plafon tahunan dan multi-level approval (`HRD-DEC-130`).
  5. Mekanisme restitusi koreksi salah potong periode lampau (*Traceable Salary Adjustment*) tanpa mengedit slip gaji final (`HRD-DEC-131`).
  6. Penyelesaian pinjaman pada slip penutupan (*Final Clearance Payslip*) dan integrasi Exit Clearance Sub-Modul 20 (`HRD-DEC-132`).
  7. Jalur cepat pencairan tunai di Kasir RS 24/7 (*Urgent Disbursement Cito*) bagi nakes pajan kecelakaan okupasional ber-token SHA-256 (`HRD-DEC-133`).
- **Kemampuan yang Ditunda (POST-MVP):**
  1. Integrasi multi-funder dengan bank eksternal penyedia kredit tanpa agunan (KTA) komersial pegawai (dikelola manual via Finance).
  2. Pengajuan klaim kacamata otomatis terintegrasi mesin auto-refraktometer poli mata RS (staf mengunggah struk/resep manual).
  3. Pembayaran cicilan via potong saldo e-wallet pribadi karyawan (hanya melalui slip gaji bulanan dan kasir fisik).

---

### 29.2 Daftar Kebutuhan Fungsional (Functional Requirements)

| Kode FR | Nama Kebutuhan Fungsional | Disposisi | Rujukan Keputusan | Deskripsi Fungsional & Kriteria Penerimaan |
| :--- | :--- | :---: | :---: | :--- |
| `FR-DED-01` | Kalkulasi Ambang Batas 50% Gross Pay | `MISSING / NEW` | `HRD-DEC-128` | Sistem wajib menghitung plafon maksimal pemotongan upah non-regulasi tepat 50% dari total penghasilan kotor sebelum pajak PPh 21 dan BPJS. |
| `FR-DED-02` | Penegakan Hirarki Prioritas Pemotongan 1 s.d. 4 | `MISSING / NEW` | `HRD-DEC-128` | Sistem wajib mengeksekusi urutan pemotongan: (1) Ganti Rugi Kasir/Aset, (2) Kasbon RS, (3) Koperasi Karyawan, (4) Paguyuban/Sosial. |
| `FR-DED-03` | Pemangkasan Parsial & Pencatatan Deferred Amount | `MISSING / NEW` | `HRD-DEC-128` | Tagihan yang melebihi sisa plafon 50% wajib dipotong sebagian sesuai sisa kuota dan sisanya dicatat pada `deferred_amount`. |
| `FR-DED-04` | Otomasi Carry-Over Tagihan Tertunda ke Bulan Depan | `MISSING / NEW` | `HRD-DEC-128` | Sistem wajib otomatis menjadwalkan ulang tagihan berstatus `DeferredDueToSalaryCap` pada siklus penggajian bulan kalender berikutnya. |
| `FR-DED-05` | Tarik Snapshot Piutang Aktif Finance H-3 Cut-off | `MISSING / NEW` | `HRD-DEC-129` | Modul HRD wajib menarik data batch cicilan aktif dari Keuangan setiap tanggal 17 pukul 01:00 WIB via `INT-HR-57`. |
| `FR-DED-06` | Penguncian Jadwal Cicilan Bulan Berjalan per Tgl 20 | `MISSING / NEW` | `HRD-DEC-129` | Jadwal cicilan bulan berjalan terkunci mutlak per tanggal 20 pukul 23:59:59; komitmen baru masuk siklus bulan depan. |
| `FR-DED-07` | Pengiriman Webhook Realisasi Pemotongan ke Finance | `MISSING / NEW` | `HRD-DEC-129` | Segera setelah slip gaji disahkan tgl 25, sistem mengirimkan realisasi pemotongan aktual ke Keuangan via `INT-HR-58`. |
| `FR-DED-08` | Retry Eksponensial Sinkronisasi Finance | `MISSING / NEW` | `HRD-DEC-129` | Jika koneksi ke Finance timeout, sistem mencoba ulang otomatis pada menit ke-5, 15, dan 30 serta mengirim alert jika gagal. |
| `FR-DED-09` | Formulir Pengajuan Klaim Mandiri di ESS | `MISSING / NEW` | `HRD-DEC-130` | Pegawai dapat mengajukan klaim kacamata/faskes luar darurat di portal ESS dengan mengunggah foto struk asli dan resume medis. |
| `FR-DED-10` | Validasi Plafon Kuota Tahunan Otomatis di ESS | `MISSING / NEW` | `HRD-DEC-130` | Sistem menolak submit klaim jika nominal diajukan melebihi sisa kuota plafon tahunan pegawai untuk kategori tersebut. |
| `FR-DED-11` | Validasi Batas Usia Kuitansi Maksimal 60 Hari | `MISSING / NEW` | `HRD-DEC-130` | Sistem menolak kuitansi yang memiliki tanggal pengeluaran lebih lampau dari 60 hari kalender sejak tanggal submit. |
| `FR-DED-12` | Verifikasi Bertingkat 3-Tier (Atasan, HR, Manajer) | `MISSING / NEW` | `HRD-DEC-130` | Berkas klaim wajib disetujui berurutan oleh: (1) Atasan Langsung, (2) HR C&B Officer (verifikasi fisik), (3) Manajer SDM. |
| `FR-DED-13` | Penegakan Aturan Anti-Self-Approval Klaim | `MISSING / NEW` | `HRD-DEC-130` | Atasan yang mengajukan klaim dilarang menyetujui klaimnya sendiri; penugasan dialihkan otomatis ke pejabat 1 tingkat di atasnya. |
| `FR-DED-14` | Injeksi Otomatis Klaim Approved ke Penggajian Tgl 25 | `MISSING / NEW` | `HRD-DEC-130` | Klaim berstatus `Approved` non-cito per tanggal 20 otomatis masuk komponen tunjangan restitusi slip gaji bulan berjalan. |
| `FR-DED-15` | Formulir Investigasi Koreksi Salah Potong Masa Lalu | `MISSING / NEW` | `HRD-DEC-131` | HR Payroll Officer dapat mencari slip gaji masa lalu dan memilih baris potongan keliru untuk pengembalian dana. |
| `FR-DED-16` | Penguncian Tautan Audit Slip Gaji Lampau Terdampak | `MISSING / NEW` | `HRD-DEC-131` | Transaksi restitusi koreksi wajib mencatat foreign key `affected_payroll_run_id` dan `affected_payslip_id` secara permanen. |
| `FR-DED-17` | Klasifikasi Komponen Restitusi Non-Pajak (Bebas PPh 21)| `MISSING / NEW` | `HRD-DEC-131` | Kompensasi salah potong dikelompokkan ke komponen `RestitutionAdjustment` non-objek pajak untuk mencegah pajak berganda. |
| `FR-DED-18` | Konsolidasi Sisa Utang pada Final Clearance Payslip | `MISSING / NEW` | `HRD-DEC-132` | Saat PHK/Resign, seluruh sisa pokok pinjaman ditarik dan dipotongkan langsung dari akumulasi hak pesangon dan kompensasi cuti. |
| `FR-DED-19` | Pengecualian Batas 50% Khusus Slip Pemisahan PHK | `MISSING / NEW` | `HRD-DEC-132` | Khusus slip pemisahan kerja akhir, sistem mengizinkan pemotongan hingga 100% dari pesangon untuk melunasi utang RS. |
| `FR-DED-20` | Penerbitan Tagihan Sisa Piutang (Outstanding Statement)| `MISSING / NEW` | `HRD-DEC-132` | Jika hak pesangon tidak cukup, sistem menerbitkan surat tagihan sisa utang yang wajib disetor tunai ke Kasir RS. |
| `FR-DED-21` | Penahanan Persetujuan Exit Clearance Sub-20 | `MISSING / NEW` | `HRD-DEC-132` | Sistem memblokir pengesahan formulir Exit Clearance staf jika kasir belum menerbitkan bukti lunas sisa piutang. |
| `FR-DED-22` | Tagging & Flagging Darurat Medis Nakes Cito | `MISSING / NEW` | `HRD-DEC-133` | Staf medis IGD yang mengalami kecelakaan okupasional dapat menandai klaim dengan tag `UrgentDisbursementCito`. |
| `FR-DED-23` | Penerbitan Voucher Kasir Elektronik Ber-Token SHA-256 | `MISSING / NEW` | `HRD-DEC-133` | Direktur Medis/SDM dapat menerbitkan voucher kasir darurat dengan masa berlaku 48 jam dan token acak terenkripsi. |
| `FR-DED-24` | Kliring & Verifikasi Voucher di Loket Kasir RS 24/7 | `MISSING / NEW` | `HRD-DEC-133` | Kasir RS 24/7 memvalidasi voucher secara online, menyerahkan dana tunai, dan mencatat nomor kuitansi kasir. |
| `FR-DED-25` | Penguncian Flag DisbursedViaCashier Anti-Double Payment| `MISSING / NEW` | `HRD-DEC-133` | Sistem mengunci status `DisbursedViaCashier` agar klaim tidak ditransfer ganda pada slip gaji bulanan tanggal 25. |

---

### 29.3 Rincian Epic MVP (`EPIC HRD-54` s.d. `HRD-58`)

#### `EPIC HRD-54`: Penegakan Batas Potongan Upah Hard-Cap 50% PP 36/2021 & Prioritas Pemotongan Berjenjang
- **Cakupan FR:** `FR-DED-01`, `FR-DED-02`, `FR-DED-03`, `FR-DED-04`.
- **Tujuan Bisnis:** Memastikan kepatuhan hukum ketenagakerjaan Indonesia bahwa potongan gaji non-regulasi tidak melebihi 50% upah kotor staf, serta menjamin hak rumah sakit atas ganti rugi aset tertagih lebih dulu daripada pinjaman sukarela.
- **Artefak Terkait:** Service `PayrollDeductionEngineService`, tabel `trx_payroll_deduction_items`, aturan `DED-B1`..`B3`, layar `FE-HRD-170`.

#### `EPIC HRD-55`: Sinkronisasi Dua Arah Jadwal Cicilan Piutang Pegawai dengan Keuangan
- **Cakupan FR:** `FR-DED-05`, `FR-DED-06`, `FR-DED-07`, `FR-DED-08`.
- **Tujuan Bisnis:** Menjaga keselarasan data antara pembukuan piutang Departemen Keuangan dengan eksekusi pemotongan gaji di HRD tanpa intervensi re-entry manual yang berisiko salah catat.
- **Artefak Terkait:** Service `FinanceDeductionSyncService`, integrasi `INT-HR-57` & `INT-HR-58`, tabel `trx_payroll_deduction_schedules`, layar `FE-HRD-168`.

#### `EPIC HRD-56`: Portal Pengajuan & Persetujuan Klaim Restitusi Mandiri di ESS
- **Cakupan FR:** `FR-DED-09`, `FR-DED-10`, `FR-DED-11`, `FR-DED-12`, `FR-DED-13`, `FR-DED-14`.
- **Tujuan Bisnis:** Memberikan transparansi benefit kesehatan kacamata kerja dan faskes luar bagi staf rumah sakit dengan kontrol anggaran ketat per golongan dan tata kelola persetujuan tanpa celah self-approval.
- **Artefak Terkait:** Service `RestitutionClaimService`, tabel `trx_employee_restitution_claims`, layar `FE-HRD-169` & `FE-HRD-172`.

#### `EPIC HRD-57`: Koreksi Restitusi Salah Potong Periode Lampau Tanpa Merusak Payroll Final
- **Cakupan FR:** `FR-DED-15`, `FR-DED-16`, `FR-DED-17`.
- **Tujuan Bisnis:** Mengembalikan hak pegawai atas kesalahan pemotongan masa lalu secara ber-audit dan bebas pajak tanpa melanggar prinsip integritas pembukuan akuntansi penggajian yang telah disahkan.
- **Artefak Terkait:** Entitas `TrxPayrollRestitution`, aturan `PAY-B6` & `DED-B4`, drawer `FE-HRD-171`.

#### `EPIC HRD-58`: Penyelesaian Piutang Pegawai Resign/PHK & Jalur Cepat Pencairan Kasir Cito 24/7
- **Cakupan FR:** `FR-DED-18`, `FR-DED-19`, `FR-DED-20`, `FR-DED-21`, `FR-DED-22`, `FR-DED-23`, `FR-DED-24`, `FR-DED-25`.
- **Tujuan Bisnis:** Mencegah piutang macet pegawai keluar melalui integrasi Exit Clearance Sub-20, serta melindungi keselamatan nakes yang terkena pajanan infeksius cito melalui loket Kasir RS 24/7 tanpa risiko bayar ganda.
- **Artefak Terkait:** Service `ExitClearanceReceivableService`, `CashierVoucherDisbursementService`, integrasi `INT-HR-59` & `INT-HR-60`, tabel `trx_cashier_disbursement_vouchers`.

---

### 29.4 Skenario Pengujian Penerimaan Pengguna (User Acceptance Testing - UAT)

| ID UAT | Epic Terkait | Skenario Pengujian Alur Nyata Rumah Sakit | Kriteria Kelulusan Pengguna (Acceptance Criteria) |
| :--- | :--- | :--- | :--- |
| `UAT-DED-01` | `EPIC HRD-54` | **Skenario Perlindungan Gaji Pegawai dari Potongan Berlebih:** Seorang perawat bergaji kotor Rp 6.000.000 memiliki ganti rugi alat Rp 1.500.000, pinjaman RS Rp 1.000.000, cicilan koperasi Rp 800.000, dan paguyuban Rp 100.000 (total tagihan Rp 3.400.000 > batas 50% yaitu Rp 3.000.000). | Penggajian memotong penuh ganti rugi Rp 1.500.000 & kasbon Rp 1.000.000, memotong parsial koperasi Rp 500.000, dan menunda sisa koperasi Rp 300.000 & paguyuban Rp 100.000 ke bulan berikutnya. Total terpotong tepat Rp 3.000.000. |
| `UAT-DED-02` | `EPIC HRD-55` | **Skenario Siklus Sinkronisasi Dua Arah Finance:** Penarikan snapshot piutang Finance pada tgl 17, penguncian data tgl 20, dan pengiriman callback realisasi setelah transfer gaji tgl 25. | Finance menerima konfirmasi potongan riil per pegawai dan secara otomatis memperbarui saldo buku besar piutang tanpa selisih 1 Rupiah pun. |
| `UAT-DED-03` | `EPIC HRD-56` | **Skenario Pengajuan Klaim Kacamata Mandiri Staf di ESS:** Staf rekam medis mengajukan kuitansi optik Rp 1.200.000 di ESS. Kepala Instalasi menyetujui, HR C&B memvalidasi nota fisik, dan Manajer SDM mengesahkan. | Kuota staf berkurang Rp 1.200.000, status klaim `Approved`, dan nominal otomatis muncul sebagai penambah hak pada slip gaji tgl 25. |
| `UAT-DED-04` | `EPIC HRD-57` | **Skenario Restitusi Salah Potong Periode Lampau:** Pegawai melapor bahwa pada slip gaji 2 bulan lalu gajinya terpotong denda mangkir Rp 400.000 padahal ia hadir dinas luar kota. HR mengesahkan restitusi koreksi. | Slip lama tetap tidak berubah (immutable); slip gaji bulan berjalan memuat baris komponen `RestitutionAdjustment` Rp 400.000 tanpa potongan pajak PPh 21 tambahan. |
| `UAT-DED-05` | `EPIC HRD-58` | **Skenario Pencairan Cito Kasir Nakes Tertusuk Jarum:** Nakes IGD tertusuk jarum butuh obat profilaksis Rp 600.000 cito tengah malam. Direktur Medis terbitkan voucher QR cito. Nakes mencairkan tunai di loket Kasir RS IGD. | Kasir menyerahkan Rp 600.000 tunai; status voucher seketika `DisbursedViaCashier`. Saat siklus gajian tgl 25 diproses, sistem menjamin dana Rp 600.000 TIDAK ditransfer ulang. |

---

### 29.5 Definisi Selesai (Definition of Done - DoD)

Sebuah item pekerjaan pada Sub-Modul 11 hanya dinyatakan selesai jika memenuhi 10 kriteria terukur berikut:
1. **Bukti Batas 50% PP 36/2021:** Unit test membuktikan total potongan non-regulasi tidak pernah melebihi 50% *Gross Pay* pada skenario gaji normal maupun ekstrem.
2. **Bukti Hirarki Berjenjang:** Unit test membuktikan urutan pemotongan Prioritas 1 s.d. 4 dijalankan konsisten dan sisa tagihan tercatat pada `deferred_amount`.
3. **Bukti Sinkronisasi Dua Arah:** Integrasi `INT-HR-57` dan `INT-HR-58` teruji sukses dengan payload valid dan respons idempotent.
4. **Bukti ESS Anti-Self-Approval:** Pengujian RBAC membuktikan atasan tidak dapat menyetujui klaim restitusi atas dirinya sendiri.
5. **Bukti Validasi Plafon & Kuitansi:** Sistem menolak klaim yang melebihi plafon tahunan atau kuitansi berusia > 60 hari.
6. **Bukti Integritas Slip Masa Lalu (`PAY-B6`):** Pengujian koreksi salah potong membuktikan slip gaji lama tidak mengalami perubahan record database.
7. **Bukti Pembebasan Pajak Koreksi:** Pengujian payroll membuktikan komponen `RestitutionAdjustment` tidak menambah beban pajak PPh 21 pegawai.
8. **Bukti Nol Kerugian Bayar Ganda Kasir:** Pengujian membuktikan klaim berstatus `DisbursedViaCashier` terblokir mutlak dari transfer slip gaji bulanan.
9. **Bukti Integrasi Exit Clearance:** Sisa pinjaman pegawai resign terpotong hingga 100% dari pesangon dan memblokir exit form jika saldo utang masih minus.
10. **Laporan Audit & Traceability Lengkap:** Laporan perubahan tercatat dan matriks penelusuran requirement bernilai 0 Gap.

---

### 29.6 Urutan Pengiriman Gelombang MVP (`MVP-33` s.d. `MVP-35`)

- **Gelombang `MVP-33` — Fondasi Konfigurasi, Engine Guard 50% & Sinkronisasi Piutang Finance:**
  - Migrasi database 6 tabel baru (`hrd_restitution_settings`, `trx_payroll_deduction_schedules`, dll.).
  - Pembangunan `PayrollDeductionEngineService` dengan penegakan hard-cap 50% berjenjang (`DED-B1`..`B3`).
  - Implementasi sinkronisasi jadwal Finance `INT-HR-57` dan webhook callback `INT-HR-58`.
  - Pembangunan layar `FE-HRD-168` (Dasbor Restitusi & Pemotongan) dan `FE-HRD-170` (Lembar Kerja Angsuran).
- **Gelombang `MVP-34` — Portal ESS Klaim Restitusi Mandiri & Alur Approval 3-Tier:**
  - Pembangunan service dan controller `RestitutionClaimService`.
  - Implementasi validasi plafon tahunan otomatis per golongan dan filter kuitansi < 60 hari.
  - Pembangunan layar `FE-HRD-169` (Portal ESS Pengajuan Klaim Mandiri Pegawai) dan `FE-HRD-172` (Layar Persetujuan Klaim).
  - Integrasi komponen restitusi approved ke dalam mesin slip gaji bulanan tanggal 25.
- **Gelombang `MVP-35` — Jalur Cepat Kasir Cito 24/7, Koreksi Masa Lalu & Exit Clearance:**
  - Implementasi penerbitan voucher kasir cito ber-token SHA-256 dan kliring loket Kasir RS 24/7 (`INT-HR-59`).
  - Penguncian flag keselamatan `DisbursedViaCashier` pencegah bayar ganda.
  - Pembangunan drawer `FE-HRD-171` untuk restitusi koreksi salah potong masa lalu non-pajak.
  - Integrasi penyelesaian utang resign/PHK dengan formulir Exit Clearance Sub-Modul 20 (`INT-HR-60`).

---

### 29.7 Traceability Keputusan ke PRD

| ID Keputusan | Epic PRD Terkait | Functional Requirement Terkait | Skenario UAT Penguji |
| :--- | :--- | :--- | :--- |
| `HRD-DEC-128` | `EPIC HRD-54` | `FR-DED-01`, `FR-DED-02`, `FR-DED-03`, `FR-DED-04` | `UAT-DED-01` |
| `HRD-DEC-129` | `EPIC HRD-55` | `FR-DED-05`, `FR-DED-06`, `FR-DED-07`, `FR-DED-08` | `UAT-DED-02` |
| `HRD-DEC-130` | `EPIC HRD-56` | `FR-DED-09`, `FR-DED-10`, `FR-DED-11`, `FR-DED-12`, `FR-DED-13`, `FR-DED-14` | `UAT-DED-03` |
| `HRD-DEC-131` | `EPIC HRD-57` | `FR-DED-15`, `FR-DED-16`, `FR-DED-17` | `UAT-DED-04` |
| `HRD-DEC-132` | `EPIC HRD-58` | `FR-DED-18`, `FR-DED-19`, `FR-DED-20`, `FR-DED-21` | `UAT-DED-01`, `UAT-DED-04` |
| `HRD-DEC-133` | `EPIC HRD-58` | `FR-DED-22`, `FR-DED-23`, `FR-DED-24`, `FR-DED-25` | `UAT-DED-05` |


---

## 30. Dokumen Ruang Lingkup PRD ke MVP Sub-Modul 12 (Slice S-D3 Benefit Management — Manajemen Manfaat, Asuransi BPJS & Tanggungan Pegawai Rumah Sakit)

Dokumen ini mendefinisikan batasan rilis produk operasional (*Product Requirements Document to Minimum Viable Product* - PRD ke MVP) untuk **Sub-Modul 12 (Benefit Management — Slice `S-D3`)**, menjembatani kesepakatan bisnis wawancara ke tahap perencanaan delivery engineering.

---

### 30.1 Batas Ruang Lingkup MVP (MVP Boundaries)

- **Titik Mulai (Starting Boundary):**
  Tersedianya profil kepegawaian baru (H+0 Onboarding `S-C4-ONB`), katalog paket manfaat berjenjang 3-lapisan (`MstBenefitPlan`), matriks kelayakan per grade staf RS (`HrdBenefitGradeEligibility`), serta pembukaan hak pendaftaran mandiri tanggungan keluarga di portal ESS.
- **Titik Akhir (Ending Boundary):**
  Terverifikasinya berkas keluarga inti sah (maks 1 pasangan + 3 anak), pemotongan saldo buku besar plafon internal RS secara real-time di loket kasir SIMRS (`INT-HR-61`), verifikasi dan penyelesaian klaim koordinasi manfaat (COB) penjamin sekunder berlampiran SEP BPJS Kesehatan, aktivasi bertahap pasca-lulus probation 3 bulan, serta penonaktifan hak benefit otomatis pada gerbang Exit Clearance Sub-Modul 20.

---

### 30.2 Kemampuan `MUST HAVE` & Kemampuan yang Ditunda

#### Kemampuan `MUST HAVE` (Masuk Rilis MVP)
1. Struktur paket manfaat 3-lapisan: BPJS wajib, asuransi swasta tambahan per grade, dan subsidi berobat internal RS Quilvian (`HRD-DEC-134`).
2. Pembatasan tanggungan keluarga inti sah (maks 1 pasangan + 3 anak hingga usia 21 tahun atau 25 tahun kuliah aktif via verifikasi surat kampus) (`HRD-DEC-135`).
3. Alur berobat nakes di RS Quilvian sendiri terintegrasi real-time dengan loket admission dan kasir SIMRS (`INT-HR-61`) dengan pemotongan buku besar plafon tahunan non-negatif dan penanganan ekses tagihan (`HRD-DEC-136`).
4. Kepatuhan aturan Koordinasi Manfaat (COB) dengan BPJS Kesehatan sebagai Penjamin Utama (*Primary Payer*) ber-SEP dan asuransi swasta/RS sebagai Penjamin Sekunder (`HRD-DEC-137`).
5. Aktivasi bertahap: BPJS TK JKK/JKM aktif H+0 Onboarding, dan BPJS Kes/swasta/subsidi RS aktif pasca-lulus masa percobaan 3 bulan (`HRD-DEC-138`).
6. Penonaktifan benefit otomatis pada tanggal keluar kerja dan verifikasi bebas tagihan medis pada checklist Exit Clearance Sub-20 (`HRD-DEC-139`).

#### Kemampuan yang Ditunda (Out of Scope / Post-MVP)
1. **Rekam Medis Elektronik (RME) & Diagnosa ICD-10:** Diagnosa klinis nakes dilindungi kerahasiaan medis pasien di SIMRS Medik (`HRD-DEC-010`); modul HRD hanya memproses angka tagihan kuitansi.
2. **Kalkulator Matematis Formula Pemotongan Slip Gaji:** Pemotongan iuran bulanan BPJS 4% dan 1% pada slip gaji adalah domain Sub-10 Payroll Processing (`S-B5`).
3. **Pembukuan Piutang Klaim Eksternal ke Asuransi:** Rekonsiliasi klaim piutang asuransi pihak ketiga ke buku besar keuangan adalah domain Finance AR (`HRD-DEC-009`).
4. **Pendaftaran Famili di Luar Keluarga Inti:** Subsidi fasilitas kesehatan rumah sakit dibatasi murni pada keluarga inti sah; famili lain tidak difasilitasi dalam skema subsidi.

---

### 30.3 Rincian Epic & Functional Requirements (`FR-BEN-01` s.d. `FR-BEN-25`)

#### EPIC HRD-59: Katalog Paket Manfaat 3-Lapisan & Matriks Kelayakan Grade
- `FR-BEN-01`: Sistem mampu mengelola katalog paket manfaat berjenjang 3-lapisan (BPJS wajib, asuransi swasta tambahan, subsidi internal RS) lengkap dengan pagu anggaran tahunan. `[MISSING / NEW]`
- `FR-BEN-02`: Sistem mampu menetapkan persentase diskon tarif pelayanan internal RS bagi nakes dan keluarga (jasa medis dan farmasi). `[MISSING / NEW]`
- `FR-BEN-03`: Sistem mampu memetakan matriks kelayakan paket benefit berdasarkan level grade/golongan kepegawaian staf rumah sakit. `[MISSING / NEW]`
- `FR-BEN-04`: Sistem mampu mengonfigurasi hak kelas kamar rawat inap (Kelas 2, Kelas 1, VIP, VVIP) per golongan staf. `[MISSING / NEW]`
- `FR-BEN-05`: Sistem mampu mengunci parameter bahwa paket asuransi swasta tambahan wajib menerapkan COB BPJS Kesehatan sebagai *Primary Payer*. `[MISSING / NEW]`

#### EPIC HRD-60: Pendaftaran Kepesertaan Asuransi & Tanggungan Keluarga Sah
- `FR-BEN-06`: Sistem mampu mencatat nomor kepesertaan BPJS Kesehatan (13 digit), BPJS Ketenagakerjaan (11 digit), dan nomor polis asuransi komersial per staf. `[EXTEND]`
- `FR-BEN-07`: Sistem mampu memfasilitasi pegawai mendaftarkan data anggota keluarga di portal ESS berlampiran scan KK dan Akta Nikah/Lahir. `[MISSING / NEW]`
- `FR-BEN-08`: Sistem mampu menegakkan batas kuota maksimal 1 pasangan sah dan 3 anak kandung/adopsi sah per pegawai. `[MISSING / NEW]`
- `FR-BEN-09`: Sistem mampu membatasi usia anak maksimal 21 tahun untuk non-mahasiswa, dan hingga 25 tahun bagi mahasiswa aktif berlampiran surat dekanat kampus. `[MISSING / NEW]`
- `FR-BEN-10`: Sistem menyediakan antrean verifikasi bagi HR Benefit Officer untuk memeriksa dokumen kependudukan fisik dan menyetujui hak tanggungan. `[MISSING / NEW]`

#### EPIC HRD-61: Berobat Internal di RS Sendiri & Integrasi Billing SIMRS
- `FR-BEN-11`: Sistem menyediakan API inquiry eligibilitas online real-time untuk loket pendaftaran admission SIMRS (`INT-HR-61`). `[MISSING / NEW]`
- `FR-BEN-12`: Sistem mampu menghitung dan mengembalikan hak persentase diskon staf dan sisa saldo plafon berjalan secara akurat. `[MISSING / NEW]`
- `FR-BEN-13`: Sistem menyediakan API pemotongan saldo plafon real-time bagi kasir SIMRS saat menutup kuitansi tindakan medis. `[MISSING / NEW]`
- `FR-BEN-14`: Sistem mencatat mutasi pemotongan pada buku besar `HrdBenefitBalanceMovement` dengan validasi saldo akhir non-negatif (`balance_after >= 0.00`). `[MISSING / NEW]`
- `FR-BEN-15`: Sistem mampu memfasilitasi pendelegasian sisa ekses tagihan berobat yang melebihi plafon ke pemotongan slip gaji berjalan via Sub-11 (`INT-HR-63`). `[MISSING / NEW]`

#### EPIC HRD-62: Koordinasi Manfaat (COB) BPJS Kesehatan vs Penjamin Sekunder
- `FR-BEN-16`: Sistem menyediakan formulir pengajuan klaim biaya kesehatan COB di portal ESS berlampiran kuitansi asli dan resume medis. `[MISSING / NEW]`
- `FR-BEN-17`: Sistem menegakkan kewajiban pengisian nomor Surat Eligibilitas Peserta (SEP) BPJS Kesehatan sebagai syarat mutlak klaim COB sekunder. `[MISSING / NEW]`
- `FR-BEN-18`: Sistem memvalidasi keselarasan matematis antara total kuitansi rumah sakit, porsi klaim BPJS (INA-CBGs), santunan sekunder, dan ekses mandiri. `[MISSING / NEW]`
- `FR-BEN-19`: Sistem menyediakan panel verifikasi klaim bagi HR Benefit Officer untuk mengaudit keabsahan kuitansi selisih biaya naik kelas. `[MISSING / NEW]`
- `FR-BEN-20`: Sistem mampu mencatat settlement pelunasan klaim sekunder lengkap dengan nomor referensi transfer bank atau kasir. `[MISSING / NEW]`

#### EPIC HRD-63: Aktivasi Bertahap, Hangus Saldo & Penonaktifan Exit Clearance
- `FR-BEN-21`: Sistem mengaktifkan otomatis program BPJS TK (JKK & JKM) pada hari pertama kerja (H+0 Onboarding) guna perlindungan risiko kerja. `[MISSING / NEW]`
- `FR-BEN-22`: Sistem menahan aktivasi BPJS Kesehatan, asuransi swasta, dan subsidi berobat RS selama pegawai berada dalam masa percobaan 3 bulan. `[MISSING / NEW]`
- `FR-BEN-23`: Sistem mengaktifkan paket kesehatan lengkap seketika pegawai dinyatakan lulus masa percobaan 3 bulan (`ProbationPassed`). `[MISSING / NEW]`
- `FR-BEN-24`: Sistem menjalankan cron job otomatis per 1 Januari pukul 00:00 WIB untuk me-reset sisa saldo plafon tahun lalu menjadi hangus dan mengalokasikan plafon baru. `[MISSING / NEW]`
- `FR-BEN-25`: Sistem secara otomatis menonaktifkan seluruh fasilitas benefit pada `EffectiveTerminationDate`, menerbitkan surat portabilitas BPJS Mandiri, dan memblokir Exit Clearance Sub-20 jika terdapat tunggakan ekses medis. `[MISSING / NEW]`

---

### 30.4 Skenario Pengujian Penerimaan Pengguna (User Acceptance Testing - UAT)

#### UAT-HRD-BEN-01: Pendaftaran Tanggungan Keluarga Sah & Penegakan Kuota ESS
- **Jalur Berhasil:** Staf perawat mendaftarkan 1 suami dan 2 anak di portal ESS berlampiran scan KK dan akta kelahiran. HR Benefit Officer memverifikasi dokumen fisik dan mengklik "Setujui". Status tanggungan berubah menjadi `Verified` dan hak subsidi aktif.
- **Jalur Gagal:** Pegawai mencoba mendaftarkan anak ke-4 atau mendaftarkan anak usia 22 tahun tanpa melampirkan surat dekanat kampus. Sistem memunculkan dialog penolakan dan memblokir pengajuan (`VR-BEN-02`, `VR-BEN-07`).

#### UAT-HRD-BEN-02: Alur Berobat Nakes di RS Sendiri & Pemotongan Plafon Kasir SIMRS
- **Jalur Berhasil:** Dokter nakes berobat di poli spesialis RS Quilvian sendiri. Loket pendaftaran memvalidasi hak nakes melalui `INT-HR-61`. Kasir SIMRS menutup billing Rp 1.000.000 (diskon 30% menjadi Rp 700.000). Saldo plafon dipotong Rp 700.000 dan tercatat di buku besar `HrdBenefitBalanceMovement`.
- **Jalur Gagal:** Sisa saldo plafon nakes hanya tersisa Rp 200.000 sedangkan tagihan netto Rp 700.000. Kasir hanya memotong Rp 200.000, dan sisa ekses Rp 500.000 wajib dilunasi tunai atau ditandatangani surat kuasa potong gaji (`VR-BEN-14`).

#### UAT-HRD-BEN-03: Pengajuan Klaim Koordinasi Manfaat (COB) Selisih Naik Kelas Rawat
- **Jalur Berhasil:** Nakes menjalani rawat inap dengan penjamin primer BPJS Kesehatan dan naik kelas kamar ke VIP (hak asuransi swasta). Pasien mengunggah salinan SEP BPJS, billing rumah sakit, dan kuitansi selisih biaya ke ESS. HR menyetujui klaim sekunder Rp 4.000.000 dan status berubah menjadi `Settled`.
- **Jalur Gagal:** Nakes mengajukan klaim sekunder tetapi mengosongkan kolom nomor SEP BPJS. Sistem menolak permohonan dengan pesan error `VR-BEN-16` (Mandatory Nomor SEP BPJS).

#### UAT-HRD-BEN-04: Aktivasi Bertahap H+0 Onboarding vs Pasca-Lulus Probation
- **Jalur Berhasil:** Pegawai baru bergabung pada 1 November. Sistem langsung mendaftarkan BPJS Ketenagakerjaan (JKK & JKM aktif H+0). Paket BPJS Kesehatan dan asuransi swasta berstatus `PendingActivation`. Tepat setelah lulus probation 3 bulan pada 1 Februari, sistem otomatis mengaktifkan paket lengkap.
- **Jalur Gagal:** HR mencoba mengaktifkan paket asuransi komersial untuk staf baru yang masa kerjanya baru 1 bulan tanpa adanya SK pengangkatan resmi. Sistem memblokir aksi dengan pesan `VR-BEN-12`.

#### UAT-HRD-BEN-05: Penonaktifan Benefit Otomatis & Kliring Medis Exit Clearance
- **Jalur Berhasil:** Dokter yang resign menyelesaikan seluruh kewajiban medis di kasir RS. Checklist Exit Clearance Sub-Modul 20 mencatat tanda centang hijau. Pada tanggal efektif keluar, sistem menonaktifkan status polis menjadi `Terminated` dan menerbitkan surat pengantar BPJS Mandiri.
- **Jalur Gagal:** Staf resign masih memiliki tagihan ekses rawat inap tertunggak Rp 1.200.000 di kasir RS. Formulir Exit Clearance otomatis terblokir dari persetujuan penutupan (`VR-BEN-24`).

---

### 30.5 Kriteria Selesai (Definition of Done - DoD)

1. [ ] Seluruh 7 entitas database (`mst_benefit_plans`, `hrd_benefit_grade_eligibilities`, `trx_employee_insurance_enrollments`, `trx_employee_benefit_dependents`, `hrd_benefit_balance_movements`, `trx_benefit_claims`, `trx_benefit_claim_documents`) terdefinisi lengkap di EF Core dan lolos migrasi non-breaking.
2. [ ] Validasi batasan kuota keluarga inti sah (maks 1 pasangan + 3 anak) teruji lolos unit test dengan penolakan pendaftaran anggota keluarga berlebih.
3. [ ] Aturan batasan umur anak (maks 21 tahun non-mahasiswa dan 25 tahun ber-surat dekanat) teruji dengan otomatisasi penonaktifan per 31 Agustus.
4. [ ] API sinkronus inquiry eligibilitas dan pemotongan saldo kasir SIMRS (`INT-HR-61`) terintegrasi dengan proteksi saldo non-negatif dan locking baris data.
5. [ ] Mandatory validasi nomor SEP BPJS Kesehatan pada klaim COB sekunder teruji memblokir pengajuan klaim tanpa SEP.
6. [ ] Pendaftaran otomatis BPJS TK JKK/JKM pada H+0 Onboarding dan aktivasi bertahap paket kesehatan pasca-lulus probation 3 bulan teruji melalui event-driven test.
7. [ ] Cron job reset saldo hangus tahunan per 1 Januari pukul 00:00 WIB teruji mengeksekusi mutasi `YearEndExpiryReset`.
8. [ ] Integrasi gerbang Exit Clearance Sub-Modul 20 teruji memblokir approval penutupan jika masih terdapat tagihan ekses medis belum lunas.
9. [ ] Seluruh 6 layar antarmuka frontend (`FE-HRD-174` s.d. `FE-HRD-179`) terpasang lengkap dengan visualisasi indikator kuota, antrean verifikasi HR, dan drawer settlement.
10. [ ] 20 skenario acceptance tests (`AT-HRD-BEN-01` s.d. `AT-HRD-BEN-20`) lulus uji tanpa regresi fungsional.

---

### 30.6 Urutan Gelombang Pengiriman (Delivery Waves)

| Gelombang | Target Slice & Komponen | Cakupan Fungsional & Delivery Items |
| :---: | :--- | :--- |
| **`MVP-36`** | Slice `S-D3.1` (Plans, Eligibility & Dependents) | Master paket manfaat 3-lapisan, matriks kelayakan grade, pendaftaran BPJS pegawai, portal pendaftaran tanggungan ESS, dan antrean verifikasi berkas kependudukan HR (`FR-BEN-01` s.d. `10`). |
| **`MVP-37`** | Slice `S-D3.2` (In-House Healthcare & Balance Ledger) | API real-time SIMRS Admission & Kasir (`INT-HR-61`), buku besar mutasi saldo plafon tahunan non-negatif `HrdBenefitBalanceMovement`, diskon tarif nakes, dan penanganan ekses tagihan (`FR-BEN-11` s.d. `15`). |
| **`MVP-38`** | Slice `S-D3.3` (COB Claims, Activation & Exit Clearance) | Pengajuan klaim koordinasi manfaat (COB) ber-SEP BPJS, settlement santunan sekunder, aktivasi bertahap H+0 vs pasca-probation, reset tahunan 1 Januari, dan kliring tunggakan Exit Clearance Sub-20 (`FR-BEN-16` s.d. `25`). |


---

## 31. PRD to MVP Sub-Modul 13: HR Service Management & Request (Layanan Administrasi, Helpdesk SDM & Penerbitan Dokumen Kepegawaian RS, Slice S-D4)

Dokumen ini mendefinisikan batasan rilis pertama (*MVP Scope*), pemetaan kapabilitas asal, daftar kebutuhan fungsional (*Functional Requirements*), skenario pengujian penerimaan pengguna (*User Acceptance Testing*), kriteria selesai (*Definition of Done*), serta urutan gelombang pengiriman (*Delivery Waves*) untuk **Sub-Modul 13 (Slice `S-D4`)**.

---

### 31.1 Batasan MVP (MVP Boundaries)

- **Titik Mulai (Start Boundary):** Pengaturan katalog master layanan SDM, konfigurasi target jam SLA (Cito 4 jam, Standar 24 jam, Khusus 72 jam), perancangan template naskah dinas resmi berbasis token dinamis, serta formulir pengajuan tiket mandiri staf di portal ESS.
- **Titik Akhir (End Boundary):** Penugasan antrean analis HR, monitoring real-time SLA dengan peringatan 75% & eskalasi 100%, penarikan otomatis gaji final Sub-10 pada surat keterangan penghasilan, otorisasi penerbitan surat resmi ber-QR Code publik dan ber-PIN finansial 4-digit, pemutakhiran otomatis data master profil Sub-02 pasca-verifikasi dokumen sah, pengarsipan permanen ke *Employee E-Dossier*, serta portal publik verifikasi keabsahan dokumen `/verify/document/{hash}`.

---

### 31.2 Pemetaan Kemampuan Asal (Existing Capability Mapping)

| ID Kemampuan Asal | Nama Kemampuan | Disposisi Target | Catatan Eksekusi & Batas Kewenangan |
| :--- | :--- | :---: | :--- |
| `CAP-HRD-085` | Master Kategori & Tipe Layanan SDM | `MISSING / NEW` | Membuat `mst_hr_service_categories` dan `mst_hr_service_types`. |
| `CAP-HRD-086` | Mesin Penomoran Terpusat & Template Naskah Dinas | `MISSING / NEW` | Membuat `mst_employee_document_types` dengan generator token dinamis gapless. |
| `CAP-HRD-087` | Meja Bantuan Helpdesk SDM & Tracking SLA | `MISSING / NEW` | Membuat `trx_hr_service_requests` dengan background worker pemantau 3-tier SLA. |
| `CAP-HRD-088` | Portal ESS Pengajuan Tiket & Klarifikasi Berkas | `MISSING / NEW` | Membuat portal mandiri staf, lampiran berkas terenkripsi, dan utas klarifikasi 2-arah. |
| `CAP-HRD-089` | Penarikan Otomatis Gaji Payroll Final Sub-10 | `EXTEND` | Integrasi read-only dengan `trx_payroll_payslips` via `INT-HR-56`. |
| `CAP-HRD-090` | Otorisasi Surat Resmi & QR Code Dinamis | `MISSING / NEW` | Membuat `trx_employee_document_issuances` dengan generator QR Code hash SHA-256. |
| `CAP-HRD-091` | Proteksi 4-Digit PIN Finansial Portal Publik | `MISSING / NEW` | Menyediakan endpoint publik verifikasi dokumen dengan masking nominal gaji via PIN BCrypt. |
| `CAP-HRD-092` | Pemutakhiran Otomatis Data Profil Sub-02 | `EXTEND` | Integrasi transaksi atomik update profil `MstWorkforceProfile` via `INT-HR-65`. |
| `CAP-HRD-093` | Pengarsipan Digital E-Dossier & Protokol Revokasi | `EXTEND` | Integrasi penyimpanan permanen berkas ke Sub-02 dan pencatatan BAP pembatalan sah. |

---

### 31.3 Rincian Kebutuhan Fungsional (Functional Requirements)

#### EPIC HRD-64: Manajemen Katalog Layanan SDM & Template Naskah Dinas Terpusat
- **`FR-HRS-01`** (`NEW`): Sistem wajib menyediakan master kategori layanan SDM (`mst_hr_service_categories`) dan master tipe layanan (`mst_hr_service_types`) dengan konfigurasi batas waktu jam SLA kerja (Cito 4 jam, Standar 24 jam, Khusus 72 jam).
- **`FR-HRS-02`** (`NEW`): Sistem wajib mendukung master template naskah dinas resmi (`mst_employee_document_types`) berformat HTML/Liquid dengan token variabel dinamis (`{{NamaLengkap}}`, `{{NIP}}`, `{{Jabatan}}`, `{{UnitKerja}}`, `{{GajiNetto}}`, dll.).
- **`FR-HRS-03`** (`NEW`): Sistem wajib menyediakan mesin penomoran surat otomatis terpusat (*Centralized Auto-Numbering Engine*) dengan format standar rumah sakit (contoh: `[NO]/RSQ-SDM/[BULAN_ROMAWI]/[TAHUN]`) yang menjamin urutan nomor seri gapless tanpa loncatan nomor.
- **`FR-HRS-04`** (`NEW`): Sistem wajib memungkinkan penandaan jenis naskah dinas sensitif finansial (`is_financial_sensitive = TRUE`) yang mewajibkan approval Supervisor C&B dan proteksi 4-digit PIN rahasia.
- **`FR-HRS-05`** (`NEW`): Sistem wajib mengonfigurasi masa berlaku standar surat resmi kepegawaian (default 90 hari kalender) yang otomatis terhitung sejak tanggal terbit.

#### EPIC HRD-65: Meja Bantuan (Helpdesk) SDM, Penugasan Antrean & Penegakan SLA 3-Tier
- **`FR-HRS-06`** (`NEW`): Sistem wajib mencatat setiap permohonan staf ke dalam agregat tiket helpdesk (`trx_hr_service_requests`) dengan nomor unik terindeks `TKT-YYYYMM-XXXXX`.
- **`FR-HRS-07`** (`NEW`): Sistem wajib menghitung tenggat waktu penyelesaian (`sla_deadline_utc`) secara otomatis berbasis jam operasional resmi rumah sakit (mengecualikan akhir pekan/libur untuk layanan Standar dan Khusus).
- **`FR-HRS-08`** (`NEW`): Sistem wajib menyediakan fitur penugasan analis SDM (*claim to me* atau disposisi supervisor) dan mencatat rekam jejak waktu respon pertama (`first_response_at_utc`).
- **`FR-HRS-09`** (`NEW`): Sistem wajib menangguhkan (*pause*) timer SLA saat status tiket diubah menjadi `WaitingStaffResponse` untuk meminta kelengkapan berkas staf, dan otomatis melanjutkan timer (*resume*) saat staf membalas klarifikasi.
- **`FR-HRS-10`** (`NEW`): Background worker wajib mengecek timer SLA setiap 15 menit, memicu status `Warning75` dan alert kuning ke Supervisor HR pada 75% durasi, serta mengubah status menjadi `Overdue` dan auto-eskalasi ke Manajer SDM pada 100% durasi.

#### EPIC HRD-66: Layanan Mandiri Staf (ESS) Permohonan Tiket & Pemutakhiran Profil Otomatis
- **`FR-HRS-11`** (`NEW`): Sistem wajib menyediakan katalog layanan visual interaktif (*tiles*) di portal ESS yang menampilkan rincian persyaratan dan estimasi SLA.
- **`FR-HRS-12`** (`NEW`): Portal ESS wajib memfasilitasi staf mengunggah berkas persyaratan digital (PDF/JPG maksimal 5MB) dan memvalidasi kelengkapan berkas wajib sebelum submit diizinkan.
- **`FR-HRS-13`** (`NEW`): Sistem wajib mengunci bahwa permohonan prioritas Cito (SLA 4 jam) wajib melampirkan berkas disposisi darurat dari Kepala Ruangan atau Komite Medik.
- **`FR-HRS-14`** (`NEW`): Portal ESS wajib menyediakan utas percakapan dua arah terenkripsi antara staf pemohon dan analis HR untuk proses klarifikasi data dan revisi lampiran.
- **`FR-HRS-15`** (`EXTEND`): Tepat setelah tiket permohonan pemutakhiran data pribadi/rekening bank disetujui analis HR, sistem wajib memperbarui entitas `MstWorkforceProfile` dan `TrxEmployeeBankDetail` Sub-02 secara otomatis tanpa input manual (`INT-HR-65`), disertai pencatatan histori snapshot ke `HrdProfileChangeHistory`.

#### EPIC HRD-67: Pusat Penerbitan Surat Resmi Ber-QR Code & Proteksi 4-Digit PIN Finansial
- **`FR-HRS-16`** (`EXTEND`): Pada permohonan Surat Keterangan Penghasilan / KPR, sistem wajib menarik nominal gaji pokok, tunjangan, dan gaji bersih secara otomatis dari slip gaji final Sub-10 (`INT-HR-56`), dan melarang intervensi ketik manual.
- **`FR-HRS-17`** (`NEW`): Surat keterangan penghasilan wajib melalui gerbang verifikasi 2-tier: diverifikasi oleh Analis SDM -> disetujui resmi oleh Supervisor Kompensasi & Benefit / Manajer SDM.
- **`FR-HRS-18`** (`NEW`): Sistem wajib menerapkan aturan anti-self-approval di mana pejabat penandatangan dilarang mengesahkan surat resmi atas nama dirinya sendiri.
- **`FR-HRS-19`** (`NEW`): Saat surat disahkan pejabat, sistem wajib membangkitkan nomor seri surat gapless, menempelkan stempel QR Code unik ber-hash SHA-256, dan me-render berkas PDF naskah dinas resmi.
- **`FR-HRS-20`** (`NEW`): Khusus surat finansial, sistem wajib meng-generate 4-digit PIN rahasia acak, menyimpannya dalam bentuk hash BCrypt pada database, dan mengirimkan PIN tersebut secara privat ke akun ESS pegawai pemohon.

#### EPIC HRD-68: Portal Verifikasi Dokumen Publik Eksternal, Pengarsipan E-Dossier & Protokol Revokasi
- **`FR-HRS-21`** (`EXTEND`): Seluruh dokumen resmi yang diterbitkan wajib diarsipkan secara otomatis ke repositori berkas kepegawaian (*Employee E-Dossier*) Sub-Modul 02 (`INT-HR-66`) dan dapat diunduh staf di menu "Dokumen Saya".
- **`FR-HRS-22`** (`NEW`): Sistem wajib menyediakan portal publik `/verify/document/{hash}` tanpa sesi login yang dapat diakses pihak luar via scan QR Code untuk menampilkan metadata validitas naskah dinas (nomor surat, tanggal terbit, nama staf, nama pejabat).
- **`FR-HRS-23`** (`NEW`): Pada portal verifikasi publik surat finansial, angka nominal penghasilan wajib disamarkan (*masked*), dan hanya dapat dibuka jika verifikator menginputkan 4-digit PIN rahasia yang sah dari pegawai.
- **`FR-HRS-24`** (`NEW`): Sistem wajib membatasi percobaan input PIN salah di portal publik maksimal 3 kali per jam per IP address sebelum mengunci akses verifikasi dokumen selama 30 menit demi mencegah brute force.
- **`FR-HRS-25`** (`NEW`): Sistem wajib menyediakan fitur pencabutan keabsahan dokumen (*revocation*) ber-BAP oleh Manajer SDM/Legal yang mempertahankan berkas fisik untuk audit forensik dan menampilkan peringatan merah penolakan di portal publik.

---

### 31.4 Skenario Pengujian Penerimaan Pengguna (UAT Scenarios)

#### UAT-HRD-HRS-01: Alur Pengajuan Tiket Helpdesk Cito Medis Staf & Penegakan SLA 4 Jam
- **Jalur Berhasil:** Dokter spesialis yang mendapat penugasan kebencanaan mendadak mengajukan tiket permohonan surat tugas Cito di ESS dengan melampirkan disposisi Komite Medik. Sistem mengunci batas SLA 4 jam kerja. Analis HR merespons dalam 30 menit dan menerbitkan surat tugas resmi dalam waktu 2 jam (SLA status hijau).
- **Jalur Gagal:** Staf non-klinis memilih prioritas Cito untuk permohonan surat visa wisata tanpa menyertakan disposisi pimpinan medis. Sistem memblokir submit tiket dengan pesan validasi `VR-HRS-02`.

#### UAT-HRD-HRS-02: Alert Peringatan Dini 75% & Auto-Eskalasi Keterlambatan ke Manajer SDM
- **Jalur Berhasil:** Tiket permohonan administrasi standar (SLA 24 jam) belum ditindaklanjuti hingga jam ke-18. Background worker mengirimkan alert kuning ke dashboard Supervisor HR. Supervisor mere-assign tiket ke analis lain yang langsung menyelesaikan tiket sebelum jam ke-24 tiba.
- **Jalur Gagal:** Tiket dibiarkan hingga melewati batas 24 jam kerja (100% overdue). Sistem secara otomatis mengubah status tiket menjadi `Overdue`, mengirimkan notifikasi eskalasi merah ke Manajer SDM, dan mencatat penalti skor KPI layanan SDM.

#### UAT-HRD-HRS-03: Penerbitan Surat Keterangan Gaji KPR Ber-QR Code & Proteksi 4-Digit PIN
- **Jalur Berhasil:** Perawat mengajukan Surat Keterangan Penghasilan untuk KPR BTN di portal ESS. Sistem menarik rincian gaji final Sub-10 secara otomatis. Supervisor C&B menyetujui draf, dan Manajer SDM mengotorisasi penerbitan. File PDF bertanda tangan digital ber-QR Code terbit, tersimpan di e-Dossier, dan 4-digit PIN privat terkirim ke ESS perawat.
- **Jalur Gagal:** Analis HR mencoba mengubah angka nominal gaji pokok pada draf surat untuk menaikkan plafon kredit perawat. Sistem menolak manipulasi payload dengan pesan `VR-HRS-06`.

#### UAT-HRD-HRS-04: Verifikasi Keaslian Dokumen di Portal Publik & Pembukaan Nominal via PIN
- **Jalur Berhasil:** Pihak analis kredit Bank BTN memindai QR Code pada lembar fisik surat perawat. Portal `/verify/document/{hash}` terbuka menampilkan status surat sah, nomor surat valid, dan nominal gaji tersamar. Pihak bank memasukkan 4-digit PIN dari perawat, dan rincian gaji pokok serta tunjangan sah seketika terbuka transparan.
- **Jalur Gagal:** Pihak luar mencoba menebak PIN secara acak sebanyak 3 kali berturut-turut. Sistem memblokir percobaan ke-4 dan mengunci verifikasi PIN selama 30 menit (`VR-HRS-12`).

#### UAT-HRD-HRS-05: Pemutakhiran Rekening Payroll Otomatis & Protokol Revokasi Dokumen Ber-BAP
- **Jalur Berhasil (Pemutakhiran):** Staf nakes mengajukan tiket ubah rekening bank payroll melampirkan scan buku tabungan asli. Analis HR memverifikasi keabsahan berkas dan mengklik setujui. Rekening bank payroll pada `TrxEmployeeBankDetail` Sub-02 seketika ter-update otomatis tanpa pengetikan manual ulang.
- **Jalur Berhasil (Revokasi):** Ditemukan kekeliruan administrasi pada surat rekomendasi beasiswa staf. Manajer SDM menerbitkan perintah revokasi dengan memasukkan nomor BAP dan alasan pembatalan. Pihak universitas luar yang memindai QR code surat seketika melihat banner merah tegas: "DOKUMEN INI TELAH DICABUT DAN TIDAK BERLAKU LAGI SECARA HUKUM".

---

### 31.5 Kriteria Selesai (Definition of Done - DoD)

1. [ ] Seluruh 8 entitas database (`mst_hr_service_categories`, `mst_hr_service_types`, `mst_employee_document_types`, `trx_hr_service_requests`, `trx_hr_service_request_attachments`, `trx_hr_service_request_comments`, `trx_employee_document_requests`, `trx_employee_document_issuances`, `hrd_profile_change_histories`) terdefinisi lengkap di EF Core dan lolos migrasi non-breaking.
2. [ ] Mesin penomoran surat resmi terpusat teruji menghasilkan nomor urut unik anti-lompat (*gapless*) dalam pengujian konkurensi multi-user paralel.
3. [ ] Penarikan otomatis rincian penghasilan staf medis dari slip gaji final Sub-10 Payroll (`INT-HR-56`) terintegrasi tanpa intervensi form manual.
4. [ ] Generator QR Code ber-hash SHA-256 dan enkripsi 4-digit PIN finansial BCrypt teruji menghasilkan verifikasi portal publik yang aman.
5. [ ] Background worker pemantauan timer 3-tier SLA (Cito 4 jam, Standar 24 jam, Khusus 72 jam) teruji mengeksekusi alert peringatan 75% dan auto-eskalasi 100% ke Manajer SDM.
6. [ ] Fitur penangguhan (*pause*) dan pelanjutan (*resume*) timer SLA saat status klarifikasi berkas `WaitingStaffResponse` teruji akurat.
7. [ ] Pemutakhiran otomatis data master profil kepegawaian Sub-02 (`INT-HR-65`) pasca-verifikasi dokumen tiket teruji atomik dengan pencatatan log snapshot forensik.
8. [ ] Pengarsipan digital abadi berkas PDF ke repositori *Employee E-Dossier* Sub-02 (`INT-HR-66`) teruji dengan penataan watermark status revokasi.
9. [ ] Seluruh 7 layar antarmuka frontend (`FE-HRD-180` s.d. `FE-HRD-186`) terpasang lengkap dengan countdown timer SLA, drawer klarifikasi berkas, dan modal input PIN portal publik.
10. [ ] 20 skenario acceptance tests (`AT-HRD-HRS-01` s.d. `AT-HRD-HRS-20`) lulus uji tanpa regresi fungsional.

---

### 31.6 Urutan Gelombang Pengiriman (Delivery Waves)

| Gelombang | Target Slice & Komponen | Cakupan Fungsional & Delivery Items |
| :---: | :--- | :--- |
| **`MVP-39`** | Slice `S-D4.1` (Catalog, SLA Engine & Helpdesk Tickets) | Master katalog layanan, konfigurasi SLA 3-tier, editor template token dinamis, mesin penomoran surat gapless, agregat tiket helpdesk, dan portal ESS pengajuan tiket berlampiran bukti (`FR-HRS-01` s.d. `10`). |
| **`MVP-40`** | Slice `S-D4.2` (Profile Sync, Document Issuance & PIN) | Drawer investigasi analis HR, utas klarifikasi staf, pemutakhiran data profil Sub-02 otomatis (`INT-HR-65`), penarikan gaji final Sub-10 (`INT-HR-56`), approval C&B, dan generator nomor surat ber-QR & PIN 4-digit (`FR-HRS-11` s.d. `20`). |
| **`MVP-41`** | Slice `S-D4.3` (Public Verification, E-Dossier & Revocation) | Portal verifikasi publik `/verify/document/{hash}`, dialog pembukaan nominal gaji via PIN 4-digit dengan proteksi brute force, pengarsipan otomatis e-Dossier Sub-02, serta protokol pencabutan dokumen resmi ber-BAP (`FR-HRS-21` s.d. `25`). |


---

## 32. Dokumen PRD ke MVP Sub-Modul 14: Credentialing, License & Clinical Privilege (Slice `S-C1`, Kontrak `v18`)

Bagian ini mendefinisikan dokumen *Product Requirement Document (PRD) to Minimum Viable Product (MVP)* untuk **Sub-Modul 14 (Slice `S-C1`)** yang mengatur tata kelola kredensialing 3 komite mandiri (Medik, Keperawatan, Nakes Lain), verifikasi keabsahan primer (PSV) STR seumur hidup KKI/SatuSehat SDMK, radar pemantau kedaluwarsa SIP 5 tahun, penilaian panel Mitra Bestari (*Peer Review*), pengesahan SPK & RKK oleh Direktur Utama, resolusi final `HRD-DEC-005` (Hard-Stop penegakan wewenang SIMRS/EMR `INT-HR-69`), tata kelola kewenangan klinis darurat & sementara, serta siklus 3 tahunan rekredensialing berkala menuju Sub-15.

---

### 32.1 Batasan MVP (MVP Boundaries)

- **Titik Mulai (Start Boundary):** Konfigurasi master 3 komite mandiri (Medik, Keperawatan, Nakes Lain), katalog buku putih (*whitebook*) kompetensi, pendaftaran STR seumur hidup & SIP 5 tahun oleh nakes di portal ESS, serta eksekusi audit verifikasi sumber primer (PSV) oleh sekretariat komite.
- **Titik Akhir (End Boundary):** Penilaian mandiri oleh minimal 2 reviewer panel Mitra Bestari (*Peer Review*), sidang pleno komite, paraf Direktur Pelayanan, pengesahan tunggal e-Sign oleh Direktur Utama RS, penerbitan berkas PDF SPK-RKK ber-QR Code publik, auto-arsip ke e-Dossier Sub-02 (`INT-HR-72`), penegakan gerbang wewenang SIMRS (`INT-HR-69`) dengan mekanisme Hard-Stop tegas dan bypass cito penyelamatan nyawa ber-audit KMKP, penugasan cepat dokter tamu / relawan bencana, serta radar rekredensialing berkala siklus 3 tahun.
- **Di Luar Batas MVP (Out of MVP Scope):**
  - Formulasi skoring indikator mutu klinis OPPE detail (*dikelola di Sub-Modul 15*).
  - Penempatan unit kerja bangsal dan kontrak kerja administratif pegawai (*dikelola di Sub-Modul 05*).
  - Penghitungan dan pembagian jasa medis tarif tindakan (*dikelola di Sub-Modul 10*).
  - Integrasi pembacaan sertifikat kompetensi via OCR kecerdasan buatan (*diverifikasi manual staf kredensialing untuk keamanan medis*).

---

### 32.2 Pemetaan Kemampuan Asal (Existing Capability Mapping)

| Domain / Komponen Asal | Status Asal | Rekomendasi Target `S-C1` | Alasan & Aksi Arsitektural |
| :--- | :---: | :---: | :--- |
| `WfpCredentialLicense` | `Existing` | **Extend** | Tambahkan kolom `is_lifetime`, `is_psv_verified`, `psv_source_url`, `psv_verified_at`, `satusehat_practitioner_id` untuk kepatuhan UU 17/2023. |
| `WfpClinicalPrivilege` | `Existing` | **Extend** | Tambahkan kolom `supervision_level` (4 Mandiri, 3 Supervisi, 1-2 Ditolak), `privilege_nature`, `is_suspended`. |
| `TrxCredentialingApplication` | `Existing` | **Extend** | Kaitkan ke `committee_id` untuk pemisahan 3 komite mandiri sesuai Permenkes 6/2026. |
| `TrxClinicalPrivilegeAssessment` | `Existing` | **Extend** | Format ulang untuk menampung penilaian independen minimal 2 anggota panel Mitra Bestari (Peer Review). |
| `TrxSpkRkkIssuance` | `None` | **New** | Model baru dokumen penugasan klinis sah bertanda tangan tunggal Direktur Utama RS dan ber-QR Code publik. |
| `TrxClinicalPrivilegeEmergencyOverrideLog` | `None` | **New** | Model log audit bypass darurat cito SIMRS untuk evaluasi mutu keselamatan pasien KMKP. |
| `ClinicalPrivilegeGateController` | `None` | **New** | Endpoint real-time `validate-gate` ber-cache Redis untuk SIMRS/EMR (`INT-HR-69`), menyelesaikan `HRD-DEC-005`. |

---

### 32.3 Rincian Kebutuhan Fungsional (Functional Requirements)

#### `EPIC HRD-69`: Tata Kelola Lisensi Profesional & Verifikasi Sumber Primer (PSV) Kemenkes
- **`FR-CRD-01`**: Staf nakes dapat mendaftarkan dokumen STR seumur hidup dan SIP 5 tahunan faskes RS via portal ESS lengkap dengan pengunggahan pindaian berkas fisik.
- **`FR-CRD-02`**: Staf sekretariat komite dapat melakukan audit verifikasi sumber primer (PSV) ke portal KKI / SatuSehat SDMK Kemenkes dan mencatat URL bukti keabsahan serta stempel tanggal audit.
- **`FR-CRD-03`**: Sistem menolak pendaftaran lisensi SIP jika masa berlaku diatur melebihi 5 tahun sejak tanggal terbit izin oleh Dinas Kesehatan (`VR-CRD-03`).
- **`FR-CRD-04`**: Background worker harian memantau masa berlaku SIP dan mengirimkan radar peringatan dini bertahap pada H-90, H-60, H-30, dan H-7 ke nakes dan kepala bidang.
- **`FR-CRD-05`**: Sistem mengeksekusi tindakan *fail-closed* otomatis tepat pada H-0 pukul 00:00 tanggal kedaluwarsa SIP: menonaktifkan akun order SIMRS (`INT-HR-69`) dan memblokir penugasan jadwal di Sub-06 (`INT-HR-71`).

#### `EPIC HRD-70`: Pemisahan 3 Komite Profesi & Penilaian Panel Mitra Bestari (*Peer Review*)
- **`FR-CRD-06`**: Sistem memisahkan alur pengajuan kredensial ke dalam 3 komite mandiri: Komite Medik, Komite Keperawatan, dan Komite Nakes Lainnya (`HRD-DEC-147`).
- **`FR-CRD-07`**: Staf klinis dapat memilih butir tindakan klinis yang dimohonkan pada katalog buku putih (*whitebook*) kompetensi sesuai komite profesinya.
- **`FR-CRD-08`**: Ketua Subkomite Kredensial dapat menunjuk minimal 2 orang anggota panel Mitra Bestari sebidang untuk menelaah logbook kasus dan portofolio klinis pemohon.
- **`FR-CRD-09`**: Sistem mencegah anggota panel Mitra Bestari menilai berkas permohonan dirinya sendiri (*Anti-Self-Review* `VR-CRD-15`).
- **`FR-CRD-10`**: Panel Mitra Bestari dapat menginput rekomendasi derajat kewenangan per butir tindakan: Level 4 (Mandiri), Level 3 (Supervisi), atau Level 1-2 (Ditolak) disertai catatan pertimbangan klinis.

#### `EPIC HRD-71`: Sidang Pleno Komite & Pengesahan SPK-RKK oleh Direktur Utama
- **`FR-CRD-11`**: Pengurus komite dapat menggelar sidang pleno, menelaah konsolidasi penilaian reviewer, dan mengesahkan surat usulan rekomendasi Ketua Komite.
- **`FR-CRD-12`**: Sistem menyusun draf dokumen SPK dan lampiran RKK serta mewajibkan penetapan nama dokter supervisor untuk setiap butir tindakan Level 3 (`VR-CRD-05`).
- **`FR-CRD-13`**: Direktur Pelayanan Medik / Keperawatan dapat menelaah kesesuaian operasional layanan dan membubuhkan paraf persetujuan manajerial.
- **`FR-CRD-14`**: Direktur Utama Rumah Sakit mengesahkan secara tunggal penerbitan SPK dan lampiran RKK menggunakan tanda tangan elektronik sah (e-Sign) (`HRD-DEC-148`).
- **`FR-CRD-15`**: Sistem menerbitkan nomor register resmi SPK/RKK gapless, file PDF ber-QR Code publik, dan mengarsipkan otomatis ke e-Dossier pegawai Sub-02 (`INT-HR-72`).

#### `EPIC HRD-72`: Penegakan Batas Kewenangan SIMRS (`INT-HR-69`), Hard-Stop & Life-Saving Override
- **`FR-CRD-16`**: Layanan gate real-time menyediakan endpoint performa tinggi (`INT-HR-69`) ber-cache Redis dengan latensi < 50ms untuk divalidasi oleh SIMRS/EMR.
- **`FR-CRD-17`**: SIMRS secara otomatis memproses langsung order tindakan klinis jika nakes memiliki kewenangan Mandiri (Level 4) pada RKK aktifnya.
- **`FR-CRD-18`**: SIMRS menahan order tindakan berstatus Supervisi (Level 3) dan mewajibkan persetujuan digital (*co-signature*) dari dokter supervisor dalam waktu 24 jam (`VR-CRD-07`).
- **`FR-CRD-19`**: SIMRS menerapkan penolakan mutlak (*Hard-Stop*) jika dokter mencoba mengeksekusi tindakan di luar kewenangan (Level 1-2) atau lisensi kedaluwarsa, menyelesaikan `HRD-DEC-005`.
- **`FR-CRD-20`**: Dokter dapat mengaktifkan tombol darurat penyelamatan nyawa (*Life-Saving Emergency Override*) pada kondisi cito mengancam jiwa dengan input justifikasi klinis, memicu notifikasi audit seketika ke Ketua Komite Medik dan KMKP dalam 24 jam.

#### `EPIC HRD-73`: Penugasan Cepat (Fast-Track) & Siklus Rekredensialing 3 Tahunan
- **`FR-CRD-21`**: Direksi dapat menerbitkan kewenangan sementara (*Temporary Clinical Privilege*) bagi dokter konsultan tamu dari luar faskes dengan durasi maksimal 3 bulan (`HRD-DEC-150`).
- **`FR-CRD-22`**: Direktur Pelayanan Medik dapat mengotorisasi kewenangan darurat kebencanaan (*Emergency Clinical Privilege*) bagi nakes relawan dengan masa berlaku maksimal 72 jam.
- **`FR-CRD-23`**: Masa berlaku SPK/RKK standar dikunci selama 3 tahun sejak disahkan Direktur Utama RS (`HRD-DEC-151`).
- **`FR-CRD-24`**: Sistem mengirimkan notifikasi radar rekredensialing pada H-90 dan otomatis menginisiasi draf berkas rekredensialing pada H-60 terintegrasi dengan skor evaluasi OPPE Sub-15 (`INT-HR-70`).
- **`FR-CRD-25`**: Sistem secara otomatis menangguhkan (*Suspended*) kewenangan mandiri dokter tepat pada H-0 jika proses rekredensialing belum disahkan Direktur Utama.

---

### 32.4 Skenario Pengujian Penerimaan Pengguna (UAT Scenarios)

- **`UAT-CRD-01` (Alur Registrasi STR & Audit Verifikasi PSV):**  
  Dokter spesialis baru mendaftarkan STR seumur hidup dan SIP 5 tahun faskes RS. Petugas kredensialing memverifikasi keabsahan data ke portal SatuSehat SDMK Kemenkes, mengunggah bukti audit, dan memastikan berkas dinyatakan terverifikasi sah.
- **`UAT-CRD-02` (Sidang Kredensial 3 Komite & Asesmen Peer Review):**  
  Staf perawat mengajukan peningkatan jenjang ke Komite Keperawatan. Ketua subkomite menunjuk 2 perawat senior sebagai panel Mitra Bestari. Reviewer menilai portofolio klinis, merekomendasikan tindakan mandiri dan supervisi, serta mengesahkan berita acara pada sidang pleno komite.
- **`UAT-CRD-03` (Penyusunan SPK Ber-Supervisor & Pengesahan Dirut):**  
  Sekretariat menyusun draf SPK dengan menyematkan dokter supervisor pada tindakan Level 3. Direktur Pelayanan memberikan paraf, dan Direktur Utama mengesahkan SPK/RKK via e-Sign. Berkas PDF ber-QR Code terbit dan masuk ke e-Dossier dokter di Sub-02.
- **`UAT-CRD-04` (Penegakan Hard-Stop di SIMRS & Alur Co-Signature):**  
  Dokter umum mencoba meresepkan obat kemoterapi di SIMRS; sistem seketika memblokir keras dengan pesan wewenang ditolak (*Hard-Stop*). Dokter spesialis bedah junior mengorder tindakan laparoskopi; order berstatus *Pending Co-Signature* dan sukses tervalidasi setelah dokter supervisor menyetujui dalam kurun 24 jam.
- **`UAT-CRD-05` (Bypass Darurat Nyawa & Siklus Rekredensialing 3 Tahun):**  
  Pada kasus henti jantung cito, dokter mengaktifkan tombol *Life-Saving Override*; tindakan tereksekusi dan alert darurat seketika masuk ke dashboard KMKP. Pada skenario akhir masa berlaku 3 tahun, sistem memicu radar H-90, menarik skor OPPE Sub-15 pada H-60, dan menangguhkan status mandiri dokter jika pada H-0 belum terbit SPK baru.

---

### 32.5 Kriteria Selesai (Definition of Done - DoD)

1. **Model & Skema Terpasang:** 1 tabel baru (`mst_credentialing_committees`), 1 tabel dokumen sah (`trx_spk_rkk_issuances`), 1 tabel audit override (`trx_clinical_privilege_emergency_override_logs`), serta ekstensi kolom pada 6 tabel existing berhasil dimigrasikan ke PostgreSQL tanpa downtime.
2. **PSV & Kepatuhan STR/SIP Teruji:** Mekanisme audit verifikasi sumber primer STR seumur hidup dan pembatasan SIP maksimal 5 tahun tervalidasi 100% pada unit test backend.
3. **Pemisahan 3 Komite Mandiri Berfungsi:** Hak akses dan perutean berkas Komite Medik, Keperawatan, dan Nakes Lain terisolasi secara independen tanpa tumpang tindih wewenang.
4. **Peer Review & Anti-Self-Review Aktif:** Penunjukan minimal 2 anggota panel Mitra Bestari berjalan dan sistem memblokir mutlak penelaahan berkas oleh pemohon sendiri.
5. **Otorisasi Tunggal Direktur Utama Berjalan:** Penerbitan SPK/RKK sah wajib divalidasi tanda tangan digital Direktur Utama RS dan menolak otorisasi dari pihak lain.
6. **Resolusi Final `HRD-DEC-005` & Hard-Stop SIMRS Lulus Uji:** API `INT-HR-69` merespons dengan latensi < 50ms, memblokir tindakan tidak berwenang secara keras, dan mengizinkan tindakan supervisi via co-signature 24 jam.
7. **Life-Saving Emergency Override Terekam:** Eksekusi bypass darurat menyimpan rekam jejak identitas dokter, waktu, alasan cito, dan mengirimkan alert insiden ke KMKP dalam 24 jam.
8. **Jalur Cepat Fast-Track Terkelola:** Penerbitan temporary privilege (maks 3 bulan) dan emergency privilege (maks 72 jam) beroperasi dengan timer pembatasan otomatis.
9. **Siklus 3 Tahun & Radar Rekredensialing Otomatis:** Scheduler mendeteksi H-90 kedaluwarsa SIP/SPK, menginisiasi draf pada H-60 terintegrasi nilai OPPE Sub-15, dan mengeksekusi penangguhan pada H-0.
10. **Laporan & Traceability Lengkap:** Seluruh 25 Kebutuhan Fungsional (`FR-CRD-01` s.d. `FR-CRD-25`) terpetakan 100% dengan **0 Gap** pada matriks penelusuran kebutuhan (*Requirement Traceability Matrix*).

---

### 32.6 Urutan Gelombang Pengiriman (Delivery Waves)

| Gelombang Rilis | Fokus Fungsional & Modul Deliverables | Target Deliverable Backend & Frontend |
| :--- | :--- | :--- |
| **`MVP-42` (Gelombang Lisensi & PSV)** | Tata kelola lisensi profesional, verifikasi primer PSV KKI / SatuSehat SDMK, radar masa berlaku SIP 5 tahun, dan fail-closed scheduler H-0 (`INT-HR-71`). | Controllers & Services Lisensi, `FE-HRD-187` (Portal Kredensial ESS), `FE-HRD-188` (Dashboard PSV Kredensialing). |
| **`MVP-43` (Gelombang 3 Komite & SPK Dirut)** | Pemisahan 3 komite mandiri, katalog whitebook, panel Mitra Bestari (Peer Review), sidang pleno komite, dan pengesahan SPK-RKK oleh Direktur Utama. | Models & Services SPK-RKK, `FE-HRD-189` (Meja Peer Review), `FE-HRD-190` (Sidang Pleno Komite), `FE-HRD-191` (Konsol Dirut). |
| **`MVP-44` (Gelombang Penegakan SIMRS & Siklus)** | Penegakan wewenang klinis SIMRS (`INT-HR-69`), Hard-Stop bertingkat, co-signature 24 jam, life-saving override KMKP, fast-track, dan siklus rekredensialing 3 tahun. | `ClinicalPrivilegeGateController`, Redis Cache, `FE-HRD-192` (Registri RKK & Audit SIMRS), `FE-HRD-193` (Fast-Track), `FE-HRD-194` (Siklus Rekredensial). |

---

## 33. Dokumen PRD ke MVP Sub-Modul 15: OPPE, FPPE & Recredentialing (`S-C1-OPPE`)

### 33.1 Gambaran Umum & Batasan Produk Minimum (MVP Scope Boundaries)

Sub-Modul 15 (Slice `S-C1-OPPE`) berfokus pada standardisasi penjaminan mutu dan keselamatan asuhan klinis dokter spesialis dan tenaga medis di rumah sakit sesuai standar akreditasi KARS / STARKES 2024 (KPS 12 & KPS 13) dan Permenkes 6/2026:
1. **Evaluasi Praktik Profesional Berkelanjutan (OPPE):** Siklus evaluasi berkala (tahunan atau semesteran) dengan pembobotan baku 60% indikator kuantitatif terintegrasi SIMRS/KMKP dan 40% evaluasi kualitatif telaah sejawat (*Peer Review*) KSM berbasis 6 pilar ACGME/KARS.
2. **Kerahasiaan Telaah Sejawat (*Peer Review Privilege*):** Perlindungan hukum kerahasiaan lembar evaluasi sejawat dengan pembatasan hak akses ketat serta pencatatan log audit keamanan permanen (*tamper-proof read-audit logging*) setiap kali dokumen dibuka.
3. **Evaluasi Praktik Profesional Terfokus (FPPE):** Deteksi otomatis ambang pemicu insiden keselamatan pasien (1 kejadian sentinel, komplikasi bedah > 2 SD, keterlambatan EMR > 48 jam > 20%, rujukan komite etik) serta protokol proctoring terarah bagi dokter spesialis baru (3 bulan / 10 kasus) dan prosedur klinis baru (6 bulan / 5 kasus).
4. **Kalkulator Komposit 3 Tahun Rekredensialing:** Perhitungan deterministik rapor mutu 3 tahunan berbobot progresif (Tahun 1: 25%, Tahun 2: 35%, Tahun 3: 40%) beserta klasifikasi eligibilitas otomatis ke dalam 3 tier kelayakan: Tier Hijau (*Fast-Track*), Tier Kuning (*Clarification*), dan Tier Merah (*Plenary Investigation*).
5. **Otomasi 4 Putusan Akhir ke Modul SIMRS EMR:** Transmisi otomatis putusan sidang rekredensialing ke modul EMR SIMRS (`INT-HR-76`), mencakup perpanjangan wewenang mandiri Level 4, pembatasan wewenang Level 3 supervisi dengan batas *co-signature* 24 jam, serta pemblokiran mutlak (*Hard-Stop*) order peresepan dan pemesanan operasi untuk putusan pencabutan wewenang (*Revoked*).

*Batasan Luar Ruang Lingkup (Out of Scope):* Indikator kinerja umum HR non-klinis (Sub-17), audit keabsahan primer PSV STR seumur hidup & SIP 5 tahun (Sub-14), tata kelola sanksi disiplin umum ketenagakerjaan SP1-SP3 (Sub-19), dan perhitungan jasa medis/remunerasi dokter (Sub-10).

---

### 33.2 Rincian Fitur MVP & Epic Pemetaan

#### 33.2.1 `EPIC HRD-74`: Tata Kelola Indikator & Siklus Evaluasi OPPE Berkala
- `FR-OPP-01`: Sistem menyediakan master katalog indikator OPPE yang dikelompokkan ke dalam 6 pilar kompetensi standar ACGME/KARS.
- `FR-OPP-02`: Sistem memfasilitasi konfigurasi target ambang batas numerik dan pembobotan indikator spesifik kelompok KSM (Bedah, Non-Bedah, Penunjang).
- `FR-OPP-03`: Sistem memfasilitasi pembukaan periode siklus evaluasi OPPE tahunan atau semesteran per KSM maupun serentak seluruh RS.
- `FR-OPP-04`: Sistem memvalidasi bahwa total persentase pembobotan kuantitatif dan kualitatif pada periode OPPE wajib tepat 100% (`VR-OPP-01`).
- `FR-OPP-05`: Sistem secara otomatis menginisiasi draf berkas rapor evaluasi individu bagi seluruh tenaga medis aktif di unit sasaran.

#### 33.2.2 `EPIC HRD-75`: Integrasi Otomatis Data Mutu Klinis SIMRS/KMKP & Agregasi Bobot 60%
- `FR-OPP-06`: Sistem menyediakan adapter integrasi `INT-HR-73` untuk mengekstrak data indikator klinis objektif dari database SIMRS & KMKP.
- `FR-OPP-07`: Sistem menghitung rasio kepatuhan pengisian resume medis EMR dalam tempo 1x24 jam pasca-pasien pulang rawat inap.
- `FR-OPP-08`: Sistem menghitung persentase kepatuhan penerapan Clinical Pathway dan tingkat kepatuhan resep terhadap formularium rumah sakit.
- `FR-OPP-09`: Sistem mengagregasikan seluruh indikator kuantitatif SIMRS dengan kontribusi bobot baku sebesar 60% terhadap nilai komposit OPPE.
- `FR-OPP-10`: Sistem menampilkan sinyal peringatan dini (*early warning alert*) jika ada indikator klinis dokter yang jatuh di bawah target minimal KSM.

#### 33.2.3 `EPIC HRD-76`: Instrumen Telaah Sejawat KSM 40% & Perlindungan Hak Istimewa Peer Review
- `FR-OPP-11`: Sistem memfasilitasi pembentukan penugasan telaah sejawat (*peer review*) ke minimal 2 dokter sebidang dalam kelompok KSM yang sama.
- `FR-OPP-12`: Sistem memblokir mutlak penelaahan berkas evaluasi sejawat oleh dokter terhadap dirinya sendiri (*anti-self-review*, `VR-OPP-03`).
- `FR-OPP-13`: Sistem menyediakan instrumen penilaian skala Likert (1-5) untuk domain etika profesi, komunikasi efektif, dan partisipasi audit mortalitas/morbiditas.
- `FR-OPP-14`: Sistem mewajibkan pengisian catatan naratif deskriptif klinis apabila penilai memberikan nilai rendah (skor <= 2, `VR-OPP-04`).
- `FR-OPP-15`: Sistem mengamankan dokumen telaah sejawat di bawah hak istimewa *Peer Review Privilege* dan mencatat log audit permanen saat dokumen dibuka (`VR-OPP-23`).

#### 33.2.4 `EPIC HRD-77`: Deteksi Ambang Kritis, Pelacakan Kasus Proctoring FPPE & Sidang Pleno
- `FR-OPP-16`: Sistem menerima feed insiden keselamatan pasien `INT-HR-74` dan memicu penugasan FPPE seketika saat terjadi 1 insiden sentinel (`VR-OPP-05`).
- `FR-OPP-17`: Sistem mendeteksi anomali tren komplikasi bedah > 2 SD dan penundaan resume EMR > 48 jam > 20% sebagai pemicu evaluasi terarah FPPE.
- `FR-OPP-18`: Sistem memfasilitasi penetapan penugasan proctoring staf baru (target 3 bulan / 10 kasus) dan prosedur baru (target 6 bulan / 5 kasus).
- `FR-OPP-19`: Sistem menyediakan buku catatan digital (*logbook*) kasus klinis terproctoring dengan enkripsi/pseudonimisasi nomor rekam medis pasien (`VR-OPP-11`).
- `FR-OPP-20`: Sistem menyediakan antarmuka verifikasi dan pengesahan evaluasi kemandirian tindakan medis oleh Dokter Proctor senior pendamping.
- `FR-OPP-21`: Sistem memfasilitasi sidang pleno Subkomite Mutu Profesi untuk menerbitkan vonis kesimpulan FPPE (Lolos, Extended, Supervisi, atau Revoked).

#### 33.2.5 `EPIC HRD-78`: Kalkulator Komposit 3 Tahun Rekredensialing & Sinkronisasi Putusan SIMRS EMR
- `FR-OPP-22`: Sistem menyediakan kalkulator komposit 3 tahun rekredensialing dengan rasio pembobotan baku: Tahun 1 (25%), Tahun 2 (35%), Tahun 3 (40%).
- `FR-OPP-23`: Sistem mengklasifikasikan eligibilitas berkas rekredensialing secara otomatis ke dalam Tier Hijau (*Fast-Track*), Tier Kuning, atau Tier Merah.
- `FR-OPP-24`: Sistem memfasilitasi penetapan 4 putusan sidang rekredensialing (Perpanjang As-Is, Upgrade, Restricted, Revoked) oleh Komite Medik dan Direksi.
- `FR-OPP-25`: Sistem mentransmisikan instruksi pembaruan wewenang Level 1 s.d. Level 4 ke modul EMR SIMRS (`INT-HR-76`) dengan penegakan Hard-Stop mutlak.

---

### 33.3 Ringkasan Kebutuhan Fungsional (Functional Requirements Matrix)

| ID Kebutuhan | Nama Kebutuhan Fungsional | Epic Pemetaan | Aturan Validasi | Skenario Uji | Prioritas |
| :--- | :--- | :--- | :--- | :--- | :---: |
| `FR-OPP-01` | Master Katalog Indikator OPPE 6 Pilar ACGME/KARS | `EPIC HRD-74` | `VR-OPP-01` | `AT-HRD-OPP-01` | P0 |
| `FR-OPP-02` | Konfigurasi Target Ambang Batas & Bobot KSM | `EPIC HRD-74` | `VR-OPP-01` | `AT-HRD-OPP-02` | P0 |
| `FR-OPP-03` | Pembukaan Periode Siklus Evaluasi OPPE | `EPIC HRD-74` | `VR-OPP-24` | `AT-HRD-OPP-01` | P0 |
| `FR-OPP-04` | Validasi Komposisi Total Bobot Evaluasi Tepat 100% | `EPIC HRD-74` | `VR-OPP-01` | `AT-HRD-OPP-02` | P0 |
| `FR-OPP-05` | Inisiasi Otomatis Berkas Rapor Individu Dokter | `EPIC HRD-74` | `VR-OPP-01` | `AT-HRD-OPP-01` | P1 |
| `FR-OPP-06` | Adapter Ekstraksi Data Indikator SIMRS & KMKP | `EPIC HRD-75` | `VR-OPP-25` | `AT-HRD-OPP-03` | P0 |
| `FR-OPP-07` | Kalkulasi Kepatuhan Resume Medis EMR 1x24 Jam | `EPIC HRD-75` | `VR-OPP-07` | `AT-HRD-OPP-03` | P0 |
| `FR-OPP-08` | Evaluasi Kepatuhan Clinical Pathway & Formularium | `EPIC HRD-75` | `VR-OPP-01` | `AT-HRD-OPP-03` | P1 |
| `FR-OPP-09` | Agregasi Bobot Kuantitatif SIMRS 60% | `EPIC HRD-75` | `VR-OPP-01` | `AT-HRD-OPP-08` | P0 |
| `FR-OPP-10` | Early Warning Alert Indikator di Bawah Standar KSM | `EPIC HRD-75` | `VR-OPP-06` | `AT-HRD-OPP-11` | P1 |
| `FR-OPP-11` | Penugasan Telaah Sejawat Peer Review KSM | `EPIC HRD-76` | `VR-OPP-02` | `AT-HRD-OPP-04` | P0 |
| `FR-OPP-12` | Blokir Anti-Self-Review Penilaian Sejawat | `EPIC HRD-76` | `VR-OPP-03` | `AT-HRD-OPP-05` | P0 |
| `FR-OPP-13` | Instrumen Skala Likert Etika & Komunikasi Multidisiplin | `EPIC HRD-76` | `VR-OPP-04` | `AT-HRD-OPP-06` | P1 |
| `FR-OPP-14` | Kewajiban Narasi Deskriptif untuk Skor Likert <= 2 | `EPIC HRD-76` | `VR-OPP-04` | `AT-HRD-OPP-06` | P0 |
| `FR-OPP-15` | Penegakan Peer Review Privilege & Read-Audit Logging | `EPIC HRD-76` | `VR-OPP-23` | `AT-HRD-OPP-07` | P0 |
| `FR-OPP-16` | Deteksi Sentinel Keselamatan Pasien Pemicu FPPE | `EPIC HRD-77` | `VR-OPP-05` | `AT-HRD-OPP-10` | P0 |
| `FR-OPP-17` | Deteksi Komplikasi > 2 SD & Resume EMR Kronis | `EPIC HRD-77` | `VR-OPP-06` | `AT-HRD-OPP-11` | P0 |
| `FR-OPP-18` | Penugasan Proctoring Dokter Baru & Prosedur Baru | `EPIC HRD-77` | `VR-OPP-08` | `AT-HRD-OPP-13` | P0 |
| `FR-OPP-19` | Logbook Kasus Terproctoring Berpseudonim Pasien | `EPIC HRD-77` | `VR-OPP-11` | `AT-HRD-OPP-15` | P0 |
| `FR-OPP-20` | Verifikasi Pengesahan Derajat Kemandirian Proctor | `EPIC HRD-77` | `VR-OPP-12` | `AT-HRD-OPP-16` | P0 |
| `FR-OPP-21` | Sidang Pleno Penetapan Vonis Kesimpulan FPPE | `EPIC HRD-77` | `VR-OPP-14` | `AT-HRD-OPP-17` | P0 |
| `FR-OPP-22` | Kalkulator Komposit 3 Tahun Rekredensial (25-35-40) | `EPIC HRD-78` | `VR-OPP-15` | `AT-HRD-OPP-19` | P0 |
| `FR-OPP-23` | Klasifikasi Eligibilitas Tier Hijau/Kuning/Merah | `EPIC HRD-78` | `VR-OPP-16` | `AT-HRD-OPP-20` | P0 |
| `FR-OPP-24` | Penetapan 4 Putusan Akhir Sidang Rekredensialing | `EPIC HRD-78` | `VR-OPP-20` | `AT-HRD-OPP-21` | P0 |
| `FR-OPP-25` | Dispatch Wewenang ke SIMRS EMR & Hard-Stop Blokir | `EPIC HRD-78` | `VR-OPP-22` | `AT-HRD-OPP-22` | P0 |

---

### 33.4 Skenario Pengujian Penerimaan Pengguna (UAT Scenarios)

- **`UAT-OPP-01` (Alur Siklus Tahunan OPPE & Perhitungan Komposit 60:40):**  
  Subkomite Mutu Profesi membuka periode evaluasi OPPE tahun 2026. Sistem menarik data kuantitatif SIMRS (skor 82). Dua dokter sejawat KSM mengisi form peer review konfidensial (skor rata-rata 88). Sistem menghitung nilai komposit akhir ((0.60 * 82) + (0.40 * 88) = 84.40). Rapor disahkan dan dokter menandatangani konfirmasi baca via ESS.
- **`UAT-OPP-02` (Perlindungan Hak Istimewa Kerahasiaan Peer Review):**  
  Pengguna dengan wewenang sah membuka detail ulasan sejawat yang bersifat konfidensial. Sistem menampilkan dialog peringatan hukum *Peer Review Privilege* dan mencatat log audit permanen (nama pengakses, IP, waktu presisi). Staf HR non-medis yang mencoba mengakses dokumen ditolak dengan pesan `403 Forbidden`.
- **`UAT-OPP-03` (Pemicu Kritis Insiden Sentinel & Inisiasi FPPE):**  
  Modul keselamatan pasien SIMRS mencatat satu kejadian sentinel tertinggalnya kasa operasi. Sinyal `INT-HR-74` seketika memicu pembentukan kasus investigasi FPPE terarah pada dokter operator. Ketua Komite Medik menunjuk Dokter Proctor senior untuk memantau 10 kasus tindakan bedah berikutnya.
- **`UAT-OPP-04` (Proctoring Dokter Baru & Pengesahan Hasil FPPE):**  
  Dokter spesialis bedah baru menjalani proctoring 3 bulan. Seluruh 10 kasus operasi dientri ke logbook berformat pseudonim dan diverifikasi kemandiriannya oleh proctor. Sidang pleno Subkomite Mutu Profesi menetapkan vonis *FullPrivilegeConfirmed*, dan status RKK dokter diperbarui menjadi mandiri penuh di SIMRS.
- **`UAT-OPP-05` (Kalkulasi Rapor Komposit 3 Tahun & Penegakan Hard-Stop Putusan Revokasi):**  
  Pada evaluasi rekredensialing berkala, sistem mengagregasi skor OPPE 3 tahun (Th 1: 78, Th 2: 82, Th 3: 86) menghasilkan komposit 82.60 (Tier Hijau Fast-Track). Pada dokter lain dengan putusan sidang *Revoked* akibat pelanggaran mutu fatal, sistem mengirimkan sinyal `INT-HR-76` ke SIMRS yang seketika memblokir keras (*Hard-Stop*) akses pemesanan resep dan kamar operasi dokter tersebut.

---

### 33.5 Kriteria Selesai (Definition of Done - DoD)

1. **Model & Skema Database Terpasang:** 7 tabel operasional baru dan 1 tabel log audit permanen (`hrd_clinical_peer_review_audit_logs`) berhasil diterapkan pada database PostgreSQL tanpa galat.
2. **Katalog Indikator & Pembobotan Baku Teruji:** Konfigurasi master indikator berbasis 6 pilar ACGME/KARS dan validasi total bobot tepat 100% (standar 60:40) lolos unit test 100%.
3. **Kerahasiaan Peer Review Privilege Beroperasi:** Akses telaah sejawat terisolasi dari staf non-wewenang dan mekanisme pencatatan *read-audit log* terbukti *tamper-proof*.
4. **Adapter Integrasi SIMRS Mutu Berfungsi:** Layanan ekstraksi indikator kuantitatif SIMRS/KMKP (`INT-HR-73`) sukses menarik data volume, kepatuhan resume EMR 24 jam, dan Clinical Pathway.
5. **Feed Pemicu Kritis FPPE Real-Time Aktif:** Sinyal kejadian sentinel, komplikasi bedah > 2 SD, dan rujukan etik sukses memicu penugasan investigasi FPPE secara otomatis.
6. **Logbook Kasus Proctoring Terenkripsi:** Form pencatatan kasus klinis menerapkan pseudonimisasi ID rekam medis pasien demi kepatuhan regulasi privasi data medis.
7. **Sidang Pleno FPPE Menghasilkan Putusan Sah:** Berita acara sidang pleno menghasilkan 4 rekomendasi putusan yang secara otomatis mengupdate status RKK tenaga medis.
8. **Kalkulator Komposit 3 Tahun Deterministik:** Formula rasio baku 25%-35%-40% dan klasifikasi otomatis tier kelayakan (Hijau, Kuning, Merah) berjalan akurat.
9. **Penegakan Wewenang SIMRS Lulus Uji:** Transmisi putusan ke SIMRS EMR (`INT-HR-76`) terbukti mengaktifkan status wewenang Level 4, Level 3 supervisi co-sign 24 jam, atau Hard-Stop blokir total untuk status Revoked.
10. **Traceability Lengkap & Tanpa Gap:** Seluruh 25 Kebutuhan Fungsional (`FR-OPP-01` s.d. `FR-OPP-25`) terpetakan 100% pada matriks penelusuran kebutuhan (*Requirement Traceability Matrix*).

---

### 33.6 Urutan Gelombang Pengiriman (Delivery Waves)

| Gelombang Rilis | Fokus Fungsional & Modul Deliverables | Target Deliverable Backend & Frontend |
| :--- | :--- | :--- |
| **`MVP-45` (Gelombang OPPE & Kualitas SIMRS)** | Konfigurasi master indikator 6 pilar ACGME, pembukaan periode evaluasi, ekstraksi data kuantitatif SIMRS/KMKP 60%, dan instrumen telaah sejawat kualitatif 40% berstatus *Peer Review Privilege*. | Controller & Services OPPE, `FE-HRD-201` (Dashboard OPPE Radar), `FE-HRD-202` (Konfigurasi Periode), `FE-HRD-203` (Form Peer Review Konfidensial). |
| **`MVP-46` (Gelombang FPPE & Proctoring Terarah)** | Feed deteksi pemicu kritis (sentinel/komplikasi), inisiasi penugasan FPPE dokter baru & prosedur baru, logbook kasus berpseudonim, verifikasi kemandirian proctor, dan sidang pleno kesimpulan FPPE. | Services FPPE & Logbook Proctoring, `FE-HRD-204` (Feed Pemicu FPPE), `FE-HRD-205` (Pemantauan Proctoring), `FE-HRD-206` (Sidang Kesimpulan FPPE). |
| **`MVP-47` (Gelombang Rekredensialing & Integrasi SIMRS)** | Kalkulator komposit 3 tahun progresif (25%-35%-40%), klasifikasi tier eligibilitas (Hijau/Kuning/Merah), diskresi override, penetapan 4 putusan sidang rekredensialing, dan dispatch pembaruan wewenang Level 1-4 ke modul SIMRS EMR (`INT-HR-76`). | `RecredentialingCompositeCalculator`, SIMRS Privileges Dispatcher, `FE-HRD-207` (Kalkulator Rapor Komposit 3 Tahun), `FE-HRD-208` (Konsol Sinkronisasi Putusan SIMRS). |

## 34. Sub-Modul 16: Learning, Training & Competency (Slice S-C2)

Dokumen PRD ke MVP ini mendefinisikan batas rilis, target kapabilitas bisnis, kebutuhan fungsional, kriteria keberterimaan (UAT), Definition of Done (DoD), serta gelombang implementasi (*Delivery Waves*) untuk **Sub-Modul 16: Learning, Training & Competency** (Target Slice **`S-C2`**).

### 34.1 Scope Ringkasan & Boundaries

- **Tujuan Modul:** Menjamin 100% kepatuhan standar pelatihan wajib akreditasi rumah sakit (STARKES 2024: KPS 8 & 8.1), menegakkan kompetensi nakes melalui ujian simulasi klinis langsung (OSCE), mencegah risiko keselamatan pasien akibat kedaluwarsa kompetensi via radar bertingkat dan safeguard dinas jaga (*buddy system*), mengontrol efisiensi anggaran diklat via TNA dan ikatan dinas retensi, serta menyediakan pelacakan portofolio 50 SKP nakes terintegrasi Plataran Sehat Kemenkes RI.
- **Batasan Dalam Cakupan (In-Scope):**
  1. Standar 5 Pelatihan Wajib Akreditasi RS (BHD/BLS, PPI, SKP, K3RS & Code Red, Komunikasi Efektif SBAR) untuk seluruh pegawai (100%) dengan siklus masa berlaku 2 tahun (24 bulan).
  2. Pelatihan kompetensi klinis khusus unit intensif (ICU: Ventilator & CVC; OK: Perioperatif; IGD: ATLS & Triase; Dialisis: Pernefri).
  3. Pemisahan kehadiran presensi vs kelulusan kompetensi via ujian teori post-test tertulis (>= 75) dan simulasi keterampilan langsung OSCE (>= 80) bertanda tangan digital assessor.
  4. Radar kedaluwarsa bertingkat H-90, H-60, H-30 dan masa tenggang 30 hari (*grace period*).
  5. Pengamanan shift dinas (*buddy system*) ke Sub-06 Scheduling saat sertifikat darurat mati (H-0 Overdue) dan penurunan hak mandiri RKK Level 4 ke Level 3 supervisi ke Sub-14 (`INT-HR-77`).
  6. Rencana Program Pembelajaran dan Pelatihan Tahunan RS berbasis *Training Needs Analysis* (TNA) dan pembukaan pagu anggaran.
  7. Pengajuan pendaftaran diklat eksternal/internal mandiri via ESS, persetujuan berjenjang, dan validasi ketersediaan pagu departemen (`trx_training_budgets`).
  8. Penerbitan otomatis akta perjanjian ikatan dinas retensi kerja nakes untuk diklat > Rp 10 Juta atau > 1 bulan, pemantauan masa retensi (12 - 36 bulan), serta perhitungan denda penalti ganti rugi wanprestasi saat resign sepihak.
  9. Registri sertifikat elektronik (e-Sertifikat) berpenomoran baku dan pengarsipan otomatis ke e-Dossier Sub-02.
  10. Pencatatan portofolio SKP nakes terbagi 3 kategori Kemenkes (Pembelajaran, Pelayanan, Pengabdian), visual tracker 50 SKP 5 tahunan, dan verifikasi sertifikat Plataran Sehat Kemenkes (`INT-HR-80`).
- **Batasan Luar Cakupan (Out-of-Scope):**
  - Verifikasi keabsahan primer (PSV) ijazah, STR seumur hidup, dan SIP 5 tahunan (merupakan wewenang Sub-14).
  - Evaluasi mutu rekam medis, audit kepatuhan klinis, dan insiden sentinel dokter (wewenang Sub-15 OPPE/FPPE).
  - Penilaian indikator kinerja non-klinis tahunan umum (wewenang Sub-17 Performance Management).
  - Pencairan honorarium narasumber dan klaim reimburse perjalanan dinas (wewenang Sub-10 / Sub-11 Payroll).
  - Pemeriksaan berkala kesehatan kerja (MCU) dan vaksinasi hepatitis staf medis (wewenang Sub-18 OHS).

---

### 34.2 Daftar Epic Bisnis

| ID Epic | Nama Epic | Deskripsi Ringkas Cakupan Kapabilitas |
| :--- | :--- | :--- |
| **`EPIC HRD-79`** | Tata Kelola Pelatihan Wajib Akreditasi & Radar Kedaluwarsa STARKES | Konfigurasi 5 standar pelatihan wajib RS siklus 24 bulan, radar peringatan bertingkat H-90/60/30, grace period 30 hari, flag Overdue, penugasan dinas *buddy system* ke Sub-06, dan penurunan hak RKK Level 4 ke Level 3 di Sub-14. |
| **`EPIC HRD-80`** | Pelaksanaan Sesi Diklat, Ujian Teori & Simulasi OSCE Klinis | Penjadwalan batch kelas workshop laboratorium simulasi, presensi absensi harian, ujian post-test tertulis, ujian praktik keterampilan klinis OSCE berskor checklist & e-sign assessor, fasilitas remedial, dan penerbitan e-sertifikat ke e-Dossier. |
| **`EPIC HRD-81`** | TNA, Pengajuan Diklat ESS & Kontrol Anggaran Diklat RS | Rencana program tahunan diklat berbasis TNA, pengajuan mandiri staf via ESS, persetujuan dispensasi dinas berjenjang, dan validasi pagu anggaran tahunan unit pengusul agar tidak defisit. |
| **`EPIC HRD-82`** | Perjanjian Ikatan Dinas & Komitmen Retensi Kerja Nakes | Penerbitan otomatis akta ikatan dinas untuk diklat bernilai > Rp 10 Juta atau > 1 bulan, pengesahan meterai, monitoring masa retensi 12-36 bulan, dan perhitungan tagihan ganti rugi wanprestasi saat staf resign sepihak. |
| **`EPIC HRD-83`** | Pencatatan Portofolio SKP & Sinkronisasi Plataran Sehat Kemenkes | Pencatatan perolehan SKP nakes 3 kategori, visual tracker radial target 50 SKP dalam 5 tahun, verifikasi nomor sertifikat Plataran Sehat Kemenkes, dan penyediaan bukti CPD untuk evaluasi OPPE Sub-15. |

---

### 34.3 Kebutuhan Fungsional (Functional Requirements)

| ID FR | Epic Terkait | Kebutuhan Fungsional | Kriteria Keberterimaan (Acceptance Criteria) |
| :--- | :--- | :--- | :--- |
| `FR-TRN-01` | `EPIC HRD-79` | Konfigurasi Master 5 Pelatihan Wajib STARKES KPS 8 | Sistem menyediakan master aturan untuk BHD, PPI, SKP, K3RS/Code Red, dan Komunikasi SBAR yang wajib diikuti oleh 100% pegawai aktif (`VR-TRN-01`). |
| `FR-TRN-02` | `EPIC HRD-79` | Penegakan Siklus Masa Berlaku Maksimal 24 Bulan | Sistem menetapkan masa kedaluwarsa maksimal 24 bulan untuk pelatihan wajib akreditasi dan menolak konfigurasi melampaui batas tersebut (`VR-TRN-02`). |
| `FR-TRN-03` | `EPIC HRD-79` | Pemetaan Pelatihan Khusus Ruang Intensif RS | Sistem memetakan sertifikat spesialisasi klinis untuk unit kerja tertentu (ICU, OK, IGD, Dialisis, NICU) yang menjadi prasyarat penugasan unit. |
| `FR-TRN-04` | `EPIC HRD-79` | Radar Kedaluwarsa Bertingkat (H-90, H-60, H-30) | Worker harian mengevaluasi tanggal masa berlaku sertifikat dan menerbitkan notifikasi otomatis ke portal ESS, email, dan WhatsApp staf serta kepala ruangan. |
| `FR-TRN-05` | `EPIC HRD-79` | Pengaturan Masa Tenggang (*Grace Period*) 30 Hari | Sistem memberikan toleransi pendaftaran penyegaran selama 30 hari pasca habis masa berlaku sebelum memberlakukan pembatasan dinas penuh (`VR-TRN-10`). |
| `FR-TRN-06` | `EPIC HRD-79` | Flag Overdue & Safeguard Dinas Shift (*Buddy System*) | Sertifikat mati (H-0 Overdue) memicu flag Overdue dan mewajibkan penugasan didampingi perawat/dokter senior pada modul jadwal Sub-06 (`VR-TRN-11`). |
| `FR-TRN-07` | `EPIC HRD-79` | Pembekuan Otomatis RKK Level 4 ke Level 3 di Sub-14 | Sertifikat klinis kritis yang mati memicu penurunan status rincian kewenangan klinis mandiri menjadi supervisi ke Sub-14 Kredensialing (`INT-HR-77`). |
| `FR-TRN-08` | `EPIC HRD-81` | Penyusunan Rencana Program Diklat Tahunan (TNA) | Bagian Diklat dapat menyusun dokumen program pelatihan tahunan berbasis TNA, memuat judul, target departemen, kuota, dan estimasi biaya per program. |
| `FR-TRN-09` | `EPIC HRD-81` | Otorisasi Pengesahan Program Tahunan oleh Direksi | Direktur SDM dan Keuangan dapat mengesahkan dokumen program tahunan diklat dan secara otomatis mengaktifkan pagu anggaran tahun fiskal berjalan. |
| `FR-TRN-10` | `EPIC HRD-80` | Penjadwalan Batch Sesi Kelas & Workshop Simulasi | Diklat dapat membuat jadwal kelas internal/eksternal, menentukan instruktur/assessor, aula/lab simulasi, URL meeting online, dan kapasitas kuota peserta. |
| `FR-TRN-11` | `EPIC HRD-81` | Pengajuan Pendaftaran Diklat Mandiri via ESS | Pegawai dapat melihat katalog pelatihan yang dibuka dan mengajukan pendaftaran mandiri via ESS dengan mengunggah justifikasi pengembangan kompetensi. |
| `FR-TRN-12` | `EPIC HRD-81` | Alur Persetujuan Dispensasi Dinas Berjenjang | Pendaftaran diklat diproses melalui alur persetujuan Kepala Ruangan (dispensasi jadwal dinas) dan Tim Diklat (validasi kualifikasi dan anggaran). |
| `FR-TRN-13` | `EPIC HRD-81` | Pengendalian & Monitoring Pagu Anggaran Diklat Unit | Sistem mencatat alokasi pagu per unit kerja dan menolak persetujuan pengajuan jika biaya melebihi sisa pagu anggaran departemen (`VR-TRN-14`). |
| `FR-TRN-14` | `EPIC HRD-81` | Otomasi Pemindahan Saldo ke Anggaran Komitmen | Persetujuan pendaftaran oleh Diklat memotong sisa saldo pagu dan mencatatnya sebagai anggaran komitmen berjalan hingga realisasi pembayaran tuntas. |
| `FR-TRN-15` | `EPIC HRD-80` | Perekaman Presensi Harian & Ambang Kehadiran | Presensi peserta dicatat per hari materi; peserta wajib hadir minimal 80% (umum) atau 100% (klinis) untuk memenuhi syarat mengikuti ujian kelulusan (`VR-TRN-03`). |
| `FR-TRN-16` | `EPIC HRD-80` | Penilaian Ujian Teori Post-Test Tertulis | Sistem mencatat skor post-test peserta (skala 0 - 100) dan menentukan status kelulusan teori dengan ambang batas minimal 75.00 (`VR-TRN-04`). |
| `FR-TRN-17` | `EPIC HRD-80` | Penilaian Ujian Simulasi Keterampilan Klinis (OSCE) | Pelatihan klinis menyelenggarakan ujian simulasi langsung berpandukan lembar rubrik checklist dengan nilai kelulusan minimal 80.00 (`VR-TRN-05`). |
| `FR-TRN-18` | `EPIC HRD-80` | Pengesahan Asesmen dengan Tanda Tangan Assessor | Lembar rubrik OSCE wajib disahkan dengan tanda tangan digital atau token kriptografis dari instruktur klinis yang terverifikasi (`VR-TRN-06`). |
| `FR-TRN-19` | `EPIC HRD-80` | Tata Kelola Remedial Evaluasi Teori dan OSCE | Peserta yang belum memenuhi batas kelulusan berhak mengikuti remedial maksimal 2 kali percobaan dalam rentang waktu paling lambat 30 hari kalender (`VR-TRN-20`). |
| `FR-TRN-20` | `EPIC HRD-80` | Penerbitan Otomatis e-Sertifikat & Arsip e-Dossier | Peserta yang dinyatakan lulus menerima e-Sertifikat resmi berpenomoran unik yang secara otomatis tersimpan pada berkas e-Dossier pegawai di Sub-02. |
| `FR-TRN-21` | `EPIC HRD-82` | Deteksi Otomatis Penerbitan Draf Akta Ikatan Dinas | Pengajuan diklat dengan pembiayaan rumah sakit > Rp 10.000.000 atau durasi > 30 hari secara otomatis memicu pembuatan draf akta ikatan dinas (`VR-TRN-16`). |
| `FR-TRN-22` | `EPIC HRD-82` | Penegakan Komitmen Retensi Kerja & Penalti Resign | Menetapkan masa retensi 12 - 36 bulan dan secara otomatis menghitung tagihan denda penalti ganti rugi jika pegawai mengundurkan diri sepihak sebelum ikatan usai (`VR-TRN-17`). |
| `FR-TRN-23` | `EPIC HRD-83` | Registri Portofolio SKP Nakes 3 Kategori Kemenkes | Pegawai nakes dapat mencatat perolehan SKP yang dikelompokkan ke kategori Pembelajaran, Pelayanan, dan Pengabdian Masyarakat sesuai pedoman Kemenkes RI. |
| `FR-TRN-24` | `EPIC HRD-83` | Visual Tracker Akumulasi 50 SKP dalam 5 Tahun | Tampilan visual dashboard radial yang memantau progres perolehan target 50 SKP nakes selama siklus 5 tahun perpanjangan Surat Izin Praktik (SIP). |
| `FR-TRN-25` | `EPIC HRD-83` | Sinkronisasi Plataran Sehat & Umpan Pilar OPPE | Verifikasi ID kegiatan ke API Plataran Sehat Kemenkes (`INT-HR-80`) serta agregasi jam CPD sebagai umpan pilar *Medical Knowledge* evaluasi OPPE Sub-15 (`INT-HR-78`). |

---

### 34.4 Skenario Pengujian Penerimaan Pengguna (User Acceptance Testing)

1. **UAT-1 (Kepatuhan Pelatihan Wajib & Radar Kedaluwarsa Akreditasi):**
   Memastikan 5 pelatihan wajib akreditasi STARKES terkonfigurasi dengan siklus 24 bulan. Ketika staf mencapai H-60 dan H-30 sebelum kedaluwarsa, notifikasi otomatis terkirim. Pada H-0 Overdue, sistem memicu penugasan pendampingan dinas shift (*buddy system*) di Sub-06 dan pembekuan wewenang RKK mandiri Level 4 di Sub-14.
2. **UAT-2 (Pelaksanaan Sesi Diklat, Ujian Teori & Simulasi OSCE Klinis):**
   Membuat jadwal sesi pelatihan klinis ICU Ventilator, mendaftarkan peserta, mencatat presensi 100%, menginput skor ujian post-test teori (85) dan ujian simulasi praktik klinis OSCE (90) dengan tanda tangan digital assessor, hingga penerbitan e-sertifikat yang terarsip di e-Dossier Sub-02.
3. **UAT-3 (TNA, Pengajuan ESS & Kontrol Saldo Pagu Anggaran Diklat):**
   Staf mengajukan pendaftaran kursus eksternal via portal ESS. Sistem memverifikasi ketersediaan sisa pagu anggaran departemen pengusul. Jika saldo pagu mencukupi, persetujuan atasan dan Diklat berhasil memotong komitmen anggaran; jika defisit, pendaftaran ditolak seketika (*fail-closed*).
4. **UAT-4 (Penerbitan Kontrak Ikatan Dinas & Penagihan Penalti Wanprestasi):**
   Pendaftaran beasiswa subspesialisasi senilai Rp 25.000.000 memicu penerbitan draf akta ikatan dinas retensi kerja selama 24 bulan. Dilakukan simulasi pengunduran diri pegawai pada bulan ke-6; sistem berhasil menerbitkan tagihan penalti ganti rugi berdasarkan formula prorata sisa bulan retensi.
5. **UAT-5 (Portofolio 50 SKP Kemenkes & Verifikasi Plataran Sehat):**
   Nakes menginput berkas perolehan SKP dengan menyertakan ID Sertifikat Plataran Sehat Kemenkes. Sistem memanggil gateway API Kemenkes untuk verifikasi validitas, memperbarui capaian tracker visual target 50 SKP 5 tahunan, dan meneruskan bukti jam CPD ke evaluasi semesteran OPPE Sub-15.

---

### 34.5 Sepuluh Kriteria Selesai (Definition of Done - DoD)

1. **Skema Basis Data Lengkap & Termigrasi:** Seluruh 10 tabel (`mst_mandatory_training_rules`, `trx_training_plans`, `trx_training_sessions`, `trx_training_enrollment_requests`, `trx_training_attendances`, `trx_training_assessments`, `trx_training_certificates`, `trx_training_budgets`, `hrd_training_bond_contracts`, `trx_plataran_sehat_skp_records`) terdefinisi di EF Core dengan indeks unik dan audit columns lengkap.
2. **24 Kontrak Endpoint API Terkunci:** Seluruh endpoint berstatus `Rencana (belum tersedia)` telah terpetakan ke 4 grup tag Swagger baku.
3. **Pemisahan Presensi vs Kelulusan OSCE Berfungsi:** Validasi presensi 80%/100% dan kelulusan ganda (teori >= 75 dan OSCE >= 80) terbukti menegakkan integritas kelulusan.
4. **Radar Kedaluwarsa & Safeguard Shift Aktif:** Background worker evaluasi berkala (H-90, H-60, H-30) terbukti mengirim peringatan dan menginstruksikan pengamanan *buddy system* ke Sub-06 saat H-0 Overdue.
5. **Integrasi RKK & OPPE Terverifikasi:** Event integrasi ke Sub-14 (`INT-HR-77`) dan Sub-15 (`INT-HR-78`) terbukti memperbarui status kewenangan klinis dan pilar *Medical Knowledge*.
6. **Kontrol Anggaran Diklat Mencegah Defisit:** Alokasi pagu anggaran unit kerja terbukti menolak pengajuan pelatihan yang melampaui sisa saldo pagu.
7. **Penerbitan Otomatis Ikatan Dinas Teruji:** Pelatihan dengan biaya > Rp 10 Juta atau durasi > 1 bulan secara konsisten memicu draf akta ikatan dinas dan formula penalti ganti rugi.
8. **Tracker 50 SKP Plataran Sehat Akurat:** Visualisasi progres perolehan SKP 5 tahunan berfungsi dengan integrasi gateway API Kemenkes (`INT-HR-80`).
9. **UI/UX Mengikuti Standar Antarmuka Quilvian:** 8 layar frontend (`FE-HRD-216` s.d. `FE-HRD-223`) menggunakan base component standar, status badge, dan Redux state yang konsisten.
10. **Traceability Lengkap & Tanpa Celah:** Seluruh 25 Kebutuhan Fungsional (`FR-TRN-01` s.d. `FR-TRN-25`) terpetakan 100% pada matriks penelusuran kebutuhan (*Requirement Traceability Matrix*).

---

### 34.6 Urutan Gelombang Pengiriman (Delivery Waves)

| Gelombang Rilis | Fokus Fungsional & Modul Deliverables | Target Deliverable Backend & Frontend |
| :--- | :--- | :--- |
| **`MVP-48` (Gelombang Pelatihan Wajib & Radar Kedaluwarsa Akreditasi)** | Master 5 pelatihan wajib STARKES KPS 8, siklus 24 bulan, radar peringatan H-90/60/30, grace period 30 hari, flag Overdue, penugasan *buddy system* ke Sub-06 (`INT-HR-79`), dan pembekuan wewenang RKK ke Sub-14 (`INT-HR-77`). | Controller & Services Mandatory Rules & Radar, `FE-HRD-216` (Dashboard Radar Pelatihan Wajib RS), `FE-HRD-217` (Konfigurasi Aturan Wajib Akreditasi). |
| **`MVP-49` (Gelombang Sesi Diklat, Penilaian OSCE & e-Sertifikat)** | Penjadwalan batch kelas workshop simulasi, presensi harian, penilaian ujian teori post-test, rubrik simulasi klinis OSCE bertanda tangan digital assessor, alur remedial, dan penerbitan e-Sertifikat ke e-Dossier Sub-02. | Services Training Sessions, Assessment & Certification, `FE-HRD-218` (Manajemen Sesi Kelas & Presensi), `FE-HRD-219` (Lembar Penilaian Ujian Teori & OSCE), `FE-HRD-220` (Registri Portofolio e-Sertifikat Pegawai). |
| **`MVP-50` (Gelombang TNA, Budget, Ikatan Dinas & SKP Plataran Sehat)** | Program tahunan diklat TNA, pengajuan mandiri ESS, persetujuan berjenjang, kontrol pagu anggaran unit, penerbitan kontrak ikatan dinas retensi 12-36 bulan, tagihan penalti resign sepihak, dan visual tracker 50 SKP Plataran Sehat Kemenkes (`INT-HR-80`). | Controller Training Plans, Budgets, Bonds & SKP Tracker, `FE-HRD-221` (Rencana Tahunan TNA & Kontrol Anggaran), `FE-HRD-222` (Tata Kelola Kontrak Ikatan Dinas Retensi), `FE-HRD-223` (Portofolio 50 SKP & Plataran Sehat Kemenkes). |

## 35. Sub-Modul 17: Performance Management & Clinical Career (Slice S-C3)

Dokumen PRD ke MVP ini mendefinisikan batas rilis, target kapabilitas bisnis, kebutuhan fungsional, kriteria keberterimaan (UAT), Definition of Done (DoD), serta gelombang implementasi (*Delivery Waves*) untuk **Sub-Modul 17: Performance Management & Clinical Career** (Target Slice **`S-C3`**).

### 35.1 Scope Ringkasan & Boundaries

- **Tujuan Modul:** Menyelaraskan target strategis rumah sakit ke kinerja individu pegawai, memastikan evaluasi adil dan berimbang antara capaian hasil kerja kuantitatif (60% KPI) dengan etika perilaku (40% Core Values BerAKHLAK), mencegah bias kelonggaran nilai melalui sidang pleno kalibrasi manajemen berpedoman distribusi fleksibel, menyediakan mekanisme pembinaan formal terstruktur melalui PIP 60 hari bagi pegawai berkinerja rendah, memfasilitasi evaluasi jenjang karier profesional perawat klinis/nakes (*Clinical Ladder* PK I s.d. PK V) berbasis portofolio komite independen, serta menyediakan saluran sanggah banding nilai kinerja yang berkeadilan.
- **Batasan Dalam Cakupan (In-Scope):**
  1. Konfigurasi periode siklus evaluasi kerja tahunan/semesteran rumah sakit dan jadwal tenggat waktu bertingkat.
  2. Master katalog indikator kinerja kuantitatif dan rubrik deskriptor Core Values BerAKHLAK skala likert 1-5.
  3. Cascading sasaran strategis instalasi ke 3 s.d. 8 target individu pegawai dengan total bobot 100%.
  4. Penilaian berjenjang resmi tiga tingkat: Refleksi mandiri (Self-assessment), Penilaian atasan langsung (Karu), dan Penelaahan atasan kedua (Kabid).
  5. Sidang pleno kalibrasi manajemen rumah sakit dengan pedoman distribusi fleksibel (*Guiding Distribution* 10-15% A, 70-80% B, 5-10% C/D) dan kewajiban nota pertimbangan tertulis untuk setiap penyesuaian skor.
  6. Kontrak formal Program Peningkatan Kinerja (PIP) 60 hari kalender, monitoring logbook mingguan mentor, dan 3 putusan akhir (Lulus, Perpanjangan 30 Hari, atau Eskalasi HR Legal).
  7. Pembekuan sementara hak promosi/mutasi pegawai selama berstatus kontrak PIP aktif.
  8. Evaluasi kenaikan jenjang karier profesional perawat klinis (PK I s.d. PK V) dan nakes lainnya dengan prasyarat minimal skor kinerja tahunan >= 75.00, kompetensi Sub-16, bebas sentinel Sub-15, dan rekomendasi Komite Keperawatan.
  9. Saluran pengajuan sanggah banding nilai resmi oleh pegawai dalam rentang maksimal 7 hari kalender pasca rilis nilai, mediasi HR Performance, dan sidang putusan final majelis banding Direktur SDM dalam 14 hari kalender.
  10. Integrasi umpan skor kinerja terkalibrasi ke pengali insentif remunerasi Sub-10 (`INT-HR-81`) dan rekomendasi pelatihan IDP ke Sub-16 (`INT-HR-82`).
- **Batasan Luar Cakupan (Out-of-Scope):**
  - Evaluasi praktik profesional berkelanjutan dokter berbasis rekam medis dan 6 pilar ACGME (wewenang Sub-Modul 15 OPPE/FPPE).
  - Ujian simulasi klinis langsung (OSCE) dan sertifikasi kompetensi STARKES (wewenang Sub-Modul 16).
  - Kalkulasi nominal insentif remunerasi, bonus tahunan, dan pencairan gaji (wewenang Sub-Modul 10 Payroll).
  - Proses persidangan disiplin etik berat dan pemutusan hubungan kerja definitif (wewenang Sub-Modul 19 Hubungan Industrial / Legal SDM).

---

### 35.2 Daftar Epic Bisnis

| ID Epic | Nama Epic | Deskripsi Ringkas Cakupan Kapabilitas |
| :--- | :--- | :--- |
| **`EPIC HRD-84`** | Siklus Kinerja Rumah Sakit, Cascading Target KPI & Pemantauan Check-in | Pengaturan siklus kinerja tahunan/semesteran, katalog metrik KPI kuantitatif, cascading target unit ke individu (3-8 butir, 100% bobot), dan pencatatan check-in berkala. |
| **`EPIC HRD-85`** | Penilaian Berjenjang Tiga Tingkat (Self, Karu, Kabid) Komposisi 60:40 BerAKHLAK | Portal evaluasi mandiri staf di ESS, penilaian atasan langsung (Karu) dengan rasio baku 60% KPI + 40% Core Values BerAKHLAK, dan penelaahan konsistensi oleh atasan kedua (Kabid). |
| **`EPIC HRD-86`** | Sidang Pleno Kalibrasi Manajemen & Pedoman Distribusi Fleksibel Kurva Lonceng | Konsol sidang kalibrasi komite RS, penelaahan deviasi leniency bias antar unit, penyesuaian skor ber-nota justifikasi resmi, pengesahan nilai komposit RS, dan rilis massal ke ESS. |
| **`EPIC HRD-87`** | Tata Kelola Program Peningkatan Kinerja (PIP 60 Hari) & Logbook Monitoring | Pemicu otomatis PIP untuk skor < 65, penerbitan kontrak pembinaan 60 hari, pembekuan mutasi, logbook mingguan mentor (1-8 minggu), dan sidang 3 putusan akhir. |
| **`EPIC HRD-88`** | Integrasi Jenjang Karier Klinis Nakes (Clinical Ladder PK I-V) & Majelis Sanggah Banding | Validasi gate skor KPI >= 75 untuk jenjang klinis perawat/nakes, audit logbook komite, sinkronisasi SK jenjang baru ke Sub-02/14, serta saluran sanggah banding nilai SLA 7/14 hari. |

---

### 35.3 Kebutuhan Fungsional (Functional Requirements)

| ID FR | Epic Terkait | Kebutuhan Fungsional | Kriteria Keberterimaan (Acceptance Criteria) |
| :--- | :--- | :--- | :--- |
| `FR-PRF-01` | `EPIC HRD-84` | Konfigurasi Siklus Periode Evaluasi Kinerja RS | Admin dapat membuka siklus tahunan/semesteran baru dan menetapkan jadwal tenggat waktu; mencegah tumpang tindih siklus aktif (`VR-PRF-01`). |
| `FR-PRF-02` | `EPIC HRD-84` | Katalog Master Indikator KPI & Core Values BerAKHLAK | Sistem menyediakan katalog metrik kuantitatif terukur dan rubrik deskriptor Core Values BerAKHLAK skala likert 1 s.d. 5 (`VR-PRF-19`). |
| `FR-PRF-03` | `EPIC HRD-84` | Cascading Target Sasaran Mutu Unit ke Individu | Karu dapat menurunkan sasaran mutu unit ke 3-8 butir target individu staf dengan total akumulasi bobot tepat 100.00% (`VR-PRF-02`). |
| `FR-PRF-04` | `EPIC HRD-84` | Sesi Check-in Berkala & Monitoring Evidence | Pegawai dan atasan dapat mencatat progres capaian kerja berkala dan melampirkan tautan logbook untuk mencegah kejutan penilaian di akhir tahun. |
| `FR-PRF-05` | `EPIC HRD-85` | Pengisian Refleksi Diri (Self-Assessment) di ESS | Pegawai dapat mengisi evaluasi mandiri atas target KPI dan Core Values beserta unggahan bukti pendukung sebelum batas waktu siklus mandiri berakhir. |
| `FR-PRF-06` | `EPIC HRD-85` | Penilaian Komposit Resmi 60:40 oleh Karu | Atasan langsung menilai seluruh butir KPI dan perilaku dengan formula baku: `(KPI * 0.60) + (Perilaku * 0.40)` (`VR-PRF-03`). |
| `FR-PRF-07` | `EPIC HRD-85` | Pemberian Catatan Ulasan Pembinaan Atasan | Karu wajib mencantumkan narasi umpan balik pembinaan konkret pada lembar evaluasi sebelum menyerahkan berkas ke atasan kedua. |
| `FR-PRF-08` | `EPIC HRD-85` | Penelaahan Konsistensi Nilai oleh Kabid | Atasan kedua meninjau konsistensi skor antar ruangan di bawah kewenangannya dan mengesahkan berkas menuju antrean sidang kalibrasi (`VR-PRF-04`). |
| `FR-PRF-09` | `EPIC HRD-86` | Kompilasi Visual Matriks Kurva Sebaran Nilai RS | Sistem mengompilasi kurva sebaran nilai komposit RS aktual terhadap pedoman distribusi fleksibel (10-15% A, 70-80% B, 5-10% C/D). |
| `FR-PRF-10` | `EPIC HRD-86` | Sidang Pleno Kalibrasi Manajemen RS | Komite Kalibrasi dapat membuka sidang pleno digital, menelaah deviasi nilai ruangan, dan mencatat risalah berita acara sidang. |
| `FR-PRF-11` | `EPIC HRD-86` | Penyesuaian Skor (Override) Ber-Nota Justifikasi | Komite berwenang menyesuaikan skor pegawai dengan kewajiban mencatat nota pertimbangan tertulis resmi (*override notes*, `VR-PRF-05`). |
| `FR-PRF-12` | `EPIC HRD-86` | Pengesahan Nilai Final Komposit RS | Direktur SDM mengesahkan penutupan sidang pleno, mengunci skor komposit akhir, dan menetapkan predikat kinerja baku (A s.d. E, `VR-PRF-07`). |
| `FR-PRF-13` | `EPIC HRD-86` | Publikasi Serentak Skor Kinerja ke Portal ESS | Skor akhir dirilis secara serentak ke portal ESS pegawai dan mengaktifkan hitung mundur masa sanggah banding selama 7 hari kalender (`VR-PRF-12`). |
| `FR-PRF-14` | `EPIC HRD-87` | Pemicu Otomatis Program PIP untuk Skor < 65 | Staf dengan nilai akhir komposit < 65.00 secara otomatis diterbitkan draf kontrak pembinaan formal PIP 60 hari kalender (`VR-PRF-08`). |
| `FR-PRF-15` | `EPIC HRD-87` | Penerbitan Kontrak Komitmen Rencana Aksi PIP | Kontrak PIP memuat sasaran target perbaikan terukur, penunjukan mentor pendamping, dan ditandatangani oleh pegawai serta atasan. |
| `FR-PRF-16` | `EPIC HRD-87` | Pembekuan Hak Promosi & Mutasi Selama Masa PIP | Sistem membekukan sementara hak pengajuan promosi jabatan, rotasi, dan mutasi internal bagi pegawai yang sedang menjalani PIP aktif (`VR-PRF-11`). |
| `FR-PRF-17` | `EPIC HRD-87` | Pengisian Logbook Mingguan Monitoring PIP | Mentor dan atasan wajib mengisi catatan evaluasi progres perbaikan mingguan (Minggu 1 s.d. 8) dengan indikator ketercapaian target (`VR-PRF-09`). |
| `FR-PRF-18` | `EPIC HRD-87` | Sidang Evaluasi Hari Ke-60 & Tiga Putusan PIP | Sidang komite hari ke-60 menetapkan putusan: Lulus (normal kembali), Perpanjangan terbatas (maks 30 hari, `VR-PRF-10`), atau Gagal (eskalasi HR Legal). |
| `FR-PRF-19` | `EPIC HRD-88` | Gate Prasyarat Skor Kinerja Minimal 75 untuk Jenjang | Kenaikan jenjang karier klinis perawat/nakes (PK I s.d. PK V) memvalidasi syarat skor kinerja tahunan minimal 75.00 (`VR-PRF-14`). |
| `FR-PRF-20` | `EPIC HRD-88` | Validasi Prasyarat Masa Kerja Klinis & Sertifikat | Sistem memvalidasi masa kerja klinis minimum per jenjang (`VR-PRF-15`) dan kepemilikan sertifikat kompetensi aktif di Sub-16 (`VR-PRF-16`). |
| `FR-PRF-21` | `EPIC HRD-88` | Pengecekan Bebas Insiden Sentinel Klinis Sub-15 | Sistem memverifikasi ketiadaan riwayat pelanggaran mutu klinis atau investigasi sentinel aktif di Sub-15 OPPE/FPPE (`VR-PRF-17`). |
| `FR-PRF-22` | `EPIC HRD-88` | Audit Portofolio Logbook Klinis Komite Keperawatan | Asesor Komite Keperawatan mengaudit dokumen logbook tindakan mandiri nakes dan menetapkan rekomendasi kenaikan jenjang klinis (`VR-PRF-18`). |
| `FR-PRF-23` | `EPIC HRD-88` | Sinkronisasi SK Jenjang Klinis Baru ke Sub-02 & Sub-14 | Penerbitan SK kenaikan jenjang baru otomatis memutakhirkan level kepangkatan di Sub-02 dan membuka paket wewenang RKK di Sub-14 (`INT-HR-83`). |
| `FR-PRF-24` | `EPIC HRD-88` | Pengajuan Sanggah Banding Nilai Resmi (Batas H+7) | Pegawai dapat mengajukan sanggahan resmi dalam batas waktu 7 hari kalender pasca nilai dirilis dengan wajib melampirkan bukti pembanding (`VR-PRF-12`). |
| `FR-PRF-25` | `EPIC HRD-88` | Mediasi HRD & Putusan Majelis Banding (Batas H+14) | Tim HR memediasi klarifikasi tertutup dalam 7 hari kerja; Majelis Banding menetapkan putusan final mengikat maksimal 14 hari kalender (`VR-PRF-13`). |

---

### 35.4 Skenario Pengujian Penerimaan Pengguna (User Acceptance Testing)

1. **UAT-1 (Cascading Target KPI, Evaluasi Mandiri & Penilaian Berjenjang 60:40):**
   Memastikan pembukaan siklus kerja tahunan berhasil, cascading sasaran instalasi ke target individu staf dengan bobot 100%, pengisian refleksi mandiri di ESS, serta penilaian Karu menerapkan formula komposit baku 60% KPI + 40% Core Values BerAKHLAK yang disetujui Kabid.
2. **UAT-2 (Sidang Pleno Kalibrasi Manajemen & Penyesuaian Skor Ber-Nota):**
   Komite Kalibrasi meninjau sebaran kurva lonceng nilai RS terhadap pedoman distribusi fleksibel (10-15% A, 70-80% B, 5-10% C/D). Dilakukan penyesuaian skor pada unit dengan deviasi ekstrem yang mewajibkan pengisian nota pertimbangan tertulis hingga pengesahan dan rilis massal ke ESS.
3. **UAT-3 (Pemicu Otomatis PIP 60 Hari, Monitoring Mingguan & Sidang Kelulusan):**
   Staf dengan skor akhir 62.00 otomatis memicu kontrak pembinaan formal PIP 60 hari kalender, pembekuan hak mutasi, perekaman 8 logbook mingguan mentor on-track, dan sidang hari ke-60 menetapkan putusan kelulusan yang memulihkan status kepegawaian normal.
4. **UAT-4 (Permohonan Jenjang Karier Klinis Nakes PK I-V & Verifikasi Gate):**
   Perawat mengajukan kenaikan jenjang ke PK III. Sistem memvalidasi skor kinerja tahunan (86 >= 75), sertifikat kompetensi aktif Sub-16, dan bebas kasus sentinel Sub-15. Asesor komite mengaudit logbook tindakan, dan SK Direktur Utama memutakhirkan profil Sub-02 serta paket RKK Sub-14.
5. **UAT-5 (Mekanisme Sanggah Banding Nilai Kinerja & Putusan Majelis):**
   Pegawai mengajukan sanggahan nilai resmi pada hari ke-5 pasca rilis (memenuhi batas H+7) dengan melampirkan berkas bukti pembanding. HR Performance memfasilitasi mediasi klarifikasi tertutup, dan Majelis Banding menerbitkan putusan penyesuaian nilai final dalam batas SLA 14 hari.

---

### 35.5 Sepuluh Kriteria Selesai (Definition of Done - DoD)

1. **Skema Basis Data Lengkap & Termigrasi:** Seluruh 10 tabel (`mst_kpi_metric_catalogs`, `trx_performance_cycles`, `trx_performance_goals`, `trx_performance_appraisals`, `trx_performance_appraisal_details`, `trx_performance_calibration_sessions`, `trx_performance_improvement_plans`, `trx_performance_pip_action_logs`, `trx_clinical_career_ladder_reviews`, `trx_performance_appraisal_disputes`) terdefinisi di EF Core dengan indeks unik dan audit columns lengkap.
2. **24 Kontrak Endpoint API Terkunci:** Seluruh endpoint berstatus `Rencana (belum tersedia)` telah terpetakan ke 4 grup tag Swagger baku Kontrak `v21`.
3. **Komposisi Skor 60:40 Berfungsi Deterministik:** Formula baku 60% KPI hasil kerja dan 40% perilaku Core Values BerAKHLAK terbukti menghitung skor komposit secara akurat.
4. **Penilaian Tiga Tingkat Beroperasi Konsisten:** Tahapan evaluasi berjenjang (Self -> Karu -> Kabid) terbukti menegakkan garis komando dan mencegah bypass wewenang.
5. **Sidang Kalibrasi dengan Guiding Distribution Aktif:** Konsol kalibrasi terbukti memetakan sebaran nilai aktual terhadap pedoman fleksibel dan mewajibkan nota justifikasi untuk setiap override nilai.
6. **Program Pembinaan PIP 60 Hari Beroperasi Penuh:** Skor < 65 otomatis memicu kontrak PIP 60 hari, pembekuan mutasi, dan monitoring mingguan mentor.
7. **Clinical Ladder Gate Terintegrasi Sub-14/15/16:** Pengajuan jenjang klinis perawat/nakes terbukti memeriksa skor KPI >= 75, sertifikat kompetensi Sub-16, dan bebas sentinel Sub-15.
8. **Mekanisme Sanggah Banding SLA 7/14 Hari Teruji:** Penolakan otomatis sanggahan > H+7 dan pengawalan batas penyelesaian majelis banding 14 hari berjalan terbukti efektif.
9. **UI/UX Mengikuti Standar Antarmuka Quilvian:** 8 layar frontend (`FE-HRD-231` s.d. `FE-HRD-238`) menggunakan base component standar, status badge, dan Redux state yang konsisten.
10. **Traceability Lengkap & Tanpa Celah:** Seluruh 25 Kebutuhan Fungsional (`FR-PRF-01` s.d. `FR-PRF-25`) terpetakan 100% pada matriks penelusuran kebutuhan (*Requirement Traceability Matrix*).

---

### 35.6 Urutan Gelombang Pengiriman (Delivery Waves)

| Gelombang Rilis | Fokus Fungsional & Modul Deliverables | Target Deliverable Backend & Frontend |
| :--- | :--- | :--- |
| **`MVP-51` (Gelombang Siklus, Target & Penilaian Berjenjang 60:40)** | Siklus kinerja tahunan/semesteran, katalog KPI & Core Values BerAKHLAK, cascading target sasaran mutu unit, self-assessment di ESS, penilaian resmi Karu 60:40, dan penelaahan Kabid. | Controller & Services Cycles, Goals & Appraisals, `FE-HRD-231` (Dashboard Kinerja), `FE-HRD-232` (Katalog KPI), `FE-HRD-233` (Siklus & Target), `FE-HRD-234` (Portal Lembar Evaluasi). |
| **`MVP-52` (Gelombang Sidang Kalibrasi & Program Pembinaan PIP 60 Hari)** | Sidang pleno kalibrasi manajemen RS, pedoman distribusi fleksibel (10-15% A, 70-80% B, 5-10% C/D), penyesuaian skor ber-nota justifikasi, rilis nilai serentak ke ESS, dan kontrak PIP 60 hari beserta monitoring logbook mingguan mentor. | Controller Calibration & PIP Services, `FE-HRD-235` (Konsol Sidang Kalibrasi), `FE-HRD-236` (Tata Kelola PIP & Logbook). |
| **`MVP-53` (Gelombang Clinical Ladder Nakes & Majelis Sanggah Banding)** | Permohonan jenjang klinis nakes PK I-V, gate prasyarat KPI >= 75 & sertifikat Sub-16, audit logbook komite, sinkronisasi SK jenjang baru ke Sub-02/14, serta saluran sanggah banding nilai SLA 7/14 hari dan putusan majelis. | Controller Clinical Ladder & Disputes Services, `FE-HRD-237` (Portofolio Jenjang Klinis PK I-V), `FE-HRD-238` (Portal Sanggah Banding Nilai). |


---

## 36. Sub-Modul 18: Hubungan Industrial & Terminasi (Slice `S-C5`)

### 36.1 Scope Ringkasan & Boundaries

Sub-modul 18 (Slice `S-C5`) mengelola siklus penegakan tata tertib pegawai, Berita Acara Pemeriksaan (BAP), penerbitan Surat Peringatan (SP 1, SP 2, SP 3) dengan masa kedaluwarsa 6 bulan kalender sesuai **PP No. 35 Tahun 2021**, penerapan prinsip keselamatan pasien berkeadilan (*Just Culture*), tata kelola skorsing investigasi (*paid suspension* 100% upah) dengan pencabutan seketika akses login SIMRS/EMR, saluran pengaduan kerja ganda (keluhan terbuka ESS & whistleblower rahasia terenkripsi), dokumentasi perundingan forum Bipartit, pengakhiran ikatan kerja (PHK/resign/pensiun/kontrak), mesin hitung kompensasi pesangon deterministik berbasis alasan terminasi, serta pusat kendali *Exit Clearance* 5 departemen yang bertindak sebagai pemblokir (*hard-block*) rilis pencairan gaji akhir dan pesangon di Sub-10 Payroll.

---

### 36.2 Daftar Epic Bisnis

1. **`EPIC HRD-89` — Tata Kelola Laporan Insiden Disiplin, Sidang BAP & Filter Just Culture Rumah Sakit**
   Mencatat pelaporan pelanggaran tata tertib, menghubungkan insiden pelayanan medis dengan nomor audit keselamatan pasien KMKP Sub-15, membedakan Human Error (pembinaan) vs Reckless Misconduct (sanksi), dan menyelenggarakan sidang pemeriksaan Berita Acara Pemeriksaan (BAP) berkeadilan.
2. **`EPIC HRD-90` — Penerbitan & Monitoring Surat Peringatan (SP 1, SP 2, SP 3) Kedaluwarsa 6 Bulan PP 35/2021**
   Menerbitkan sanksi tertulis berjenjang (Teguran Lisan, SP 1, SP 2, SP 3) dengan tanggal berlaku 6 bulan kalender, menyediakan jalur langsung SP 3 untuk pelanggaran berat, otomatisasi status kedaluwarsa H+180 via worker `EMP-W1`, serta memfasilitasi proses sanggah banding staf.
3. **`EPIC HRD-91` — Pengelolaan Skorsing Investigasi (*Paid Suspension*) & Pengamanan Akses SIMRS**
   Menetapkan status skorsing bagi pegawai dalam pemeriksaan intensif menuju PHK dengan jaminan pembayaran upah pokok dan tunjangan tetap 100% sesuai Pasal 157A UU Ketenagakerjaan, serta memicu pencabutan otomatis hak login SIMRS, EMR, PACS, dan akses kartu fisik rumah sakit (`INT-HR-88`).
4. **`EPIC HRD-92` — Kanal Pengaduan Staf (Grievance ESS), Whistleblowing Rahasia Terenkripsi & Forum Bipartit**
   Menyediakan saluran pengaduan keluhan kerja sehari-hari berbatas waktu penanganan 14 hari kerja, portal whistleblower rahasia terenkripsi end-to-end dengan proteksi anti-retaliasi, serta mendokumentasikan notulen jalannya perundingan perselisihan forum LKS Bipartit RS.
5. **`EPIC HRD-93` — Pengakhiran Kerja (PHK), Kalkulator Pesangon Regulasi PP 35/2021 & Exit Clearance Hard-Block**
   Mengelola proses pengunduran diri sukarela (one month notice), habis kontrak PKWT, pensiun, dan pemutusan sanksi; mengoperasikan mesin hitung kompensasi pesangon (UP, UPMK, UPH, Uang Pisah); memverifikasi kliring 5 unit; serta memblokir rilis gaji akhir di Sub-10 sebelum clearance tuntas 100%.

---

### 36.3 Kebutuhan Fungsional (Functional Requirements)

* **`FR-EMP-01`**: Sistem wajib menyediakan formulir pelaporan insiden tata tertib pegawai yang mencakup tanggal, lokasi, kategori, deskripsi kronologi, dan jenis pelapor.
* **`FR-EMP-02`**: Sistem wajib mewajibkan input nomor referensi audit keselamatan pasien KMKP (`KmKpAuditReferenceNumber`) untuk setiap insiden kategori pelayanan klinis (`INT-HR-86`).
* **`FR-EMP-03`**: Sistem wajib menerapkan prinsip *Just Culture* dengan mengklasifikasikan insiden menjadi `HumanError`, `AtRiskBehavior`, atau `RecklessMisconduct`.
* **`FR-EMP-04`**: Sistem dilarang menerbitkan sanksi Surat Peringatan (SP) untuk insiden yang terklasifikasi sebagai *Human Error* murni dan wajib mengalihkannya ke modul pembinaan/diklat Sub-16.
* **`FR-EMP-05`**: Sistem wajib menyediakan lembar kerja digital Berita Acara Pemeriksaan (BAP) yang memuat nama tim pemeriksa, kesaksian saksi, barang bukti, dan pembelaan staf terperiksa.
* **`FR-EMP-06`**: Sistem wajib mencatat penerbitan sanksi Surat Peringatan dengan tingkatan: Teguran Lisan, SP 1, SP 2, dan SP 3.
* **`FR-EMP-07`**: Sistem wajib menetapkan tanggal kedaluwarsa sanksi SP 1, SP 2, dan SP 3 tepat 6 bulan kalender sejak tanggal diterbitkan (`ValidUntil = IssuedDate + 6 Months`) sesuai PP No. 35 Tahun 2021 Pasal 52.
* **`FR-EMP-08`**: Sistem wajib menetapkan masa berlaku sanksi Teguran Lisan maksimal 3 bulan kalender (90 hari).
* **`FR-EMP-09`**: Sistem wajib memvalidasi kenaikan jenjang sanksi secara bertahap (SP 2 membutuhkan SP 1 aktif), kecuali untuk kasus pelanggaran berat mendesak.
* **`FR-EMP-10`**: Sistem wajib mengizinkan penerbitan langsung SP 3 untuk pelanggaran berat/mendesak setelah divalidasi oleh HR Legal dan disetujui Direktur SDM.
* **`FR-EMP-11`**: Sistem wajib menyediakan background worker `DisciplinarySanctionExpiryWorker` (`EMP-W1`) yang otomatis mengubah status SP menjadi `Expired` pada H+180 hari kalender.
* **`FR-EMP-12`**: Sistem wajib mengirimkan notifikasi peringatan H-14 sebelum berakhirnya masa aktif sanksi SP kepada staf bersangkutan dan atasan langsung.
* **`FR-EMP-13`**: Sistem wajib menyediakan fitur pengajuan sanggah banding tertulis atas surat sanksi SP maksimal 7 hari kerja sejak tanggal penerbitan.
* **`FR-EMP-14`**: Sistem wajib memfasilitasi penetapan surat skorsing masa investigasi (*paid suspension*) dengan durasi maksimal 180 hari kalender (6 bulan).
* **`FR-EMP-15`**: Sistem wajib menjamin status pembayaran upah penuh 100% (`wage_payment_status = 'PaidFull'`) selama masa skorsing investigasi sesuai Pasal 157A UU Ketenagakerjaan.
* **`FR-EMP-16`**: Sistem wajib memicu event seketika `INT-HR-88` untuk menonaktifkan akun login SSO SIMRS/EMR/PACS dan kartu fisik akses gedung saat surat skorsing atau penetapan PHK aktif.
* **`FR-EMP-17`**: Sistem wajib menyediakan kanal pengaduan keluhan hubungan kerja (*grievance*) di portal ESS pegawai dengan batas SLA mediasi maksimal 14 hari kerja.
* **`FR-EMP-18`**: Sistem wajib menyediakan kanal pelaporan pelanggaran rahasia (*whistleblowing*) terenkripsi dengan opsi pelaporan `Anonim` dan proteksi anti-retaliasi.
* **`FR-EMP-19`**: Sistem wajib mengisolasi hak akses tiket whistleblower sehingga hanya dapat dibuka oleh Tim Kepatuhan dan Direktur SDM (`Hrd.Disciplinary.WhistleblowerReview`).
* **`FR-EMP-20`**: Sistem wajib mendokumentasikan agenda perundingan Bipartit, daftar perwakilan kedua belah pihak, notulen risalah perselisihan, dan naskah Perjanjian Bersama (PB) berformat PDF.
* **`FR-EMP-21`**: Sistem wajib memfasilitasi pengajuan pengakhiran ikatan kerja (PHK) dengan kategori: Resign Sukarela, Habis Kontrak PKWT, Pensiun Normal, Pelanggaran Disiplin SP 3, Pelanggaran Berat, dan Meninggal Dunia.
* **`FR-EMP-22`**: Sistem wajib menyediakan mesin hitung pesangon otomatis yang menghitung Uang Pesangon (UP), Uang Penghargaan Masa Kerja (UPMK), Uang Penggantian Hak (UPH), dan Uang Pisah berdasarkan formula regulasi PP 35/2021 dan masa kerja sah dari `HireDate` Sub-02.
* **`FR-EMP-23`**: Sistem wajib menghitung kompensasi pengakhiran kontrak PKWT secara proporsional: `(Masa Kerja Bulan / 12) x 1 Bulan Upah Terakhir`.
* **`FR-EMP-24`**: Sistem wajib mengelola pusat kendali *Exit Clearance* dengan checklist verifikasi digital dari 5 departemen penanggung jawab (Unit Kerja, Umum/Aset, Farmasi/Alkes, Keuangan, IT).
* **`FR-EMP-25`**: Sistem wajib memblokir secara otomatis (*hard-block*) tombol otorisasi pencairan gaji terakhir dan transfer pesangon di Sub-10 Payroll sebelum seluruh 5 departemen memberikan status persetujuan `Approved` (`INT-HR-87`).

---

### 36.4 Skenario Pengujian Penerimaan Pengguna (User Acceptance Testing)

Telah didokumentasikan lengkap pada berkas `testing/acceptance-test-matrix.md` bagian 30.2 yang mencakup 5 skenario utama:
1. `UAT-EMP-01`: Penerbitan Sanksi Disiplin Berjenjang Sesuai PP 35/2021 & Kedaluwarsa Otomatis 6 Bulan.
2. `UAT-EMP-02`: Penanganan Insiden Pelayanan Medis Pasien dengan Filter Prinsip Just Culture RS.
3. `UAT-EMP-03`: Pelaporan Kecurangan Farmasi Melalui Kanal Whistleblowing Terenkripsi & Anti-Retaliasi.
4. `UAT-EMP-04`: Penetapan Skorsing Investigasi Pelanggaran Berat dengan Upah Penuh & Pencabutan Akses SIMRS.
5. `UAT-EMP-05`: Pengakhiran Kerja Resign Sukarela, Kalkulasi Pesangon PP 35/2021 & Exit Clearance Hard-Block.

---

### 36.5 Sepuluh Kriteria Selesai (Definition of Done - DoD)

1. **Kepatuhan Regulasi Ketenagakerjaan (PP 35/2021 & UU Cipta Kerja):** Formula pesangon dan masa berlaku sanksi SP 6 bulan kalender tervalidasi 100% sesuai dasar hukum perburuhan nasional.
2. **Penegakan Prinsip Just Culture RS:** Pemisahan tegas antara Human Error (diklat) vs Reckless Misconduct (disiplin) terintegrasi dengan nomor audit keselamatan pasien KMKP Sub-15 (`INT-HR-86`).
3. **Kerahasiaan Whistleblower & Anti-Retaliasi:** Payload laporan pengaduan rahasia terenkripsi pada level aplikasi dan terisolasi dari atasan langsung pihak terlapor.
4. **Kepastian Perlindungan Upah Skorsing:** Sistem menjamin hak 100% upah pokok dan tunjangan tetap berjalan selama masa skorsing investigasi maksimal 6 bulan kalender.
5. **Pengamanan Akses SIMRS & Fasilitas Fisik:** Event `INT-HR-88` memutus sesi aktif SSO SIMRS/EMR/PACS dan kartu fisik secara instan saat skorsing/PHK aktif.
6. **Deterministik Kalkulator Pesangon:** Seluruh komponen UP, UPMK, UPH, dan Uang Pisah terhitung otomatis berdasarkan masa kerja riil dari `HireDate` Sub-02.
7. **Pusat Kendali Exit Clearance 5 Departemen:** Checklist serah terima terverifikasi digital secara berjenjang oleh Unit Kerja, Aset Umum, Farmasi, Keuangan, dan IT.
8. **Hard-Block Payroll Sub-10:** Rilis pencairan gaji terakhir dan transfer pesangon di Sub-10 Payroll terblokir otomatis hingga clearance `Completed` (`INT-HR-87`).
9. **Kesiapan Layanan API & Swagger:** 24 endpoint API pada `contracts/api-contract.md` bagian 28 terstandarisasi dengan 5 tag grup Swagger resmi.
10. **Kesiapan Otomasi Background Workers:** 6 worker (`EMP-W1` s.d. `EMP-W6`) siap memantau kedaluwarsa SP, durasi skorsing, SLA grievance, dan sinkronisasi penonaktifan akun IT.

---

### 36.6 Urutan Gelombang Pengiriman (Delivery Waves)

* **`MVP-54` — Wave 1: Tata Kelola Disiplin, Filter Just Culture & Sanksi SP PP 35/2021**
  - Implementasi entitas `trx_disciplinary_incident_reports`, `trx_disciplinary_investigation_records`, dan `trx_disciplinary_sanctions`.
  - Filter Just Culture dengan nomor audit KMKP Sub-15 (`INT-HR-86`).
  - Worker kedaluwarsa sanksi SP 6 bulan `EMP-W1` dan layar `FE-HRD-247`, `FE-HRD-248`.
* **`MVP-55` — Wave 2: Skorsing Upah Penuh, Kanal Pengaduan Staf, Whistleblowing & Bipartit**
  - Implementasi entitas `trx_disciplinary_suspensions`, `trx_employee_grievances`, dan `trx_bipartite_negotiation_forums`.
  - Integrasi pencabutan sesi SIMRS `INT-HR-88` dan enkripsi payload whistleblower.
  - Worker eskalasi SLA grievance `EMP-W3` dan layar `FE-HRD-249`, `FE-HRD-250`, `FE-HRD-251`.
* **`MVP-56` — Wave 3: Pemutusan Kerja, Kalkulator Pesangon PP 35/2021 & Exit Clearance Hard-Block**
  - Implementasi entitas `trx_employee_separations`, `trx_severance_calculations`, `trx_exit_clearance_processes`, dan `trx_exit_clearance_checklists`.
  - Mesin hitung pesangon PP 35/2021 dan pengesahan Direktur Keuangan.
  - Integrasi hard-block pencairan payroll akhir ke Sub-10 (`INT-HR-87`).
  - Worker pengingat kliring aset `EMP-W5` dan layar `FE-HRD-252`, `FE-HRD-253`.
