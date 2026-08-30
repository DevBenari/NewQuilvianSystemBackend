# Human Resource — PRD ke MVP

## 1. Identitas dokumen

| Field | Value |
| --- | --- |
| Produk | Quilvian Hospital Information System |
| Modul | Human Resource (`human-resource`, prefix `HRD`) |
| Blueprint ID | `HRD-BP-001` |
| Dokumen | `04-prd-to-mvp.md` |
| `contract_version` | `v2` |
| `last_changed_in` | `v2` |
| Status | `draft` — **belum** `approved`. Approval adalah tindakan manusia, bukan keluaran skill |
| Owner | Pemilik produk HR bersama technical owner (`HRD-DEC-015`) |
| `approved_by` / `approved_at` | **Belum ada** |
| Repository backend | `NewQuilvianSystemBackend`, branch kerja `AndryZain` |
| Repository frontend | `QuilvianSystemFrontendDev`, branch `AgentCodexFrontend` |
| Backend SHA baseline | `e0ee42c752a5f92c5b1663ff88bef07a5859f79f` |
| Backend baseline canonical | `origin/QuilvianIntegrationBackend` (`HRD-DEC-021`), diverifikasi `16b8b71` |
| Frontend SHA baseline | `fff76a1b394d4b247c70a04f106c8ec098c9696e` |
| `input_revision` | `02-backend-architecture.md` rev `1`; `03-frontend-architecture.md` rev `1`; seluruh `contracts/` `v1`; `data/data-dictionary.md` `v1`; `flowcharts/**` |
| `input_hash` — decision log | `0f4bb66d96d5fcd10a388e7b98efa08510f9edf50e3033dddf84951ad09854a3` |
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
| Perencanaan tenaga kerja | `HRD-CAP-19` | **`BLOCKED`** oleh `HRD-Q-05` | Berjalan di luar sistem |
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
| Apa isi tabel 67 entity yang belum punya API? (`HRD-Q-05`) | Pemilik basis data | Keputusan skema yang merusak data tidak boleh diambil. Ikut menahan pemasangan unique constraint pada penempatan shift | **Ya** untuk seluruh `POST-MVP` domain tanpa API |
| Siapa wakil Komite Medik? (`HRD-Q-08`) | Manajemen | Kredensial dan kewenangan klinis tetap `BLOCKED` | **Ya** untuk `S-C1` |
| Apakah `HRD-DEC-010` disahkan K3RS? | K3RS | Kesehatan kerja staf tetap `BLOCKED` | **Ya** untuk `S-C6` |
| Apakah pemisahan peran pengusul dan penyetuju diperlukan pada tindakan disiplin? (`HRD-Q-51`) | Pemilik proses HR | Seseorang dapat mengusulkan sekaligus menyetujui sanksi | **Ya** untuk `S-C5` |
| Tingkatan izin apa yang berlaku bagi data paling terbatas? (`HRD-Q-52`) | Pemilik keamanan bersama pemilik proses | Tidak ada yang menetapkan siapa boleh membaca kasus kedisiplinan | **Ya** untuk `S-C5` |
| Siapa pemilik kebijakan bisnis HR? (`HRD-Q-01`) | Manajemen | Blueprint tidak dapat disetujui secara keseluruhan | **Ya** untuk approval blueprint |
| ~~`HRD-Q-54`~~ — satu definisi bersama atau empat terpisah | ~~Pemilik produk~~ | **Ditutup `HRD-DEC-036`:** empat definisi terpisah dengan pola awal yang sama | Tidak |

**Dokumen ini memuat pertanyaan memblokir yang belum terjawab.** Sesuai kontrak, ia tetap boleh
berstatus `draft`, tetapi **MUST NOT** diteruskan ke `/plan-module-delivery` sebelum pertanyaan
yang memblokir gelombang pengiriman dijawab pemiliknya.
