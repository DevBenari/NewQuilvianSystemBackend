# Rekomendasi: Scan Sistem Wajib Sebelum Wawancara Bisnis

|  |  |
| --- | --- |
| Tanggal | 2026-08-14 |
| Status | **Terpasang**, menunggu tiga keputusan owner pada bagian 9 |
| Dokumen canonical | `docs/agency/update-skills/04-prascan-registry-sistem.md` |
| Dokumen induk | [README.md](README.md) |
| Pemicu | Wawancara modul selama ini dimulai tanpa peta sistem, sehingga modul baru berisiko membangun ulang yang sudah ada dan bertabrakan dengan modul lain |
| Usulan | **1 skill baru + 1 folder rule baru + 1 slash command**, dari 7 skill menjadi 8 |
| Cakupan perubahan | Konfigurasi AI agent dan dokumentasi. Source aplikasi tidak diubah |
| Database/runtime migration | Tidak ada |
| Breaking change | Ya, terbatas: `/grill-me` berhenti bila registry belum ada |

---

## 1. Ringkasan untuk pembaca yang terburu-buru

Backend Quilvian saat ini memuat **445 `DbSet`**, **452 file EF configuration**, **246
controller**, dan **81 migration**. Tidak ada satu orang pun yang dapat mengingat isi sistem
sebesar itu.

Meskipun begitu, workflow yang berlaku sekarang memulai modul baru dengan wawancara bisnis
(`/grill-me`) tanpa seorang pun melihat apa yang sudah ada. Audit kode (`/trace-existing-capabilities`)
baru berjalan **setelah** wawancara selesai, dan audit itu pun hanya menyisir satu modul.

Akibatnya wawancara berjalan buta pada hal-hal yang sebenarnya sudah tersedia. Keputusan bisnis
diambil lebih dulu, baru kemudian ketahuan bahwa fondasinya sudah ada, namanya sudah dipakai, atau
datanya milik modul lain.

Usulannya satu langkah baru di paling depan:

```text
/qv-scan  →  /qv-grill  →  /qv-trace  →  /qv-design  →  /qv-plan  →  /qv-build-*  →  /qv-verify
  baru        wajib         mendalam
```

`/qv-scan` dijalankan **sekali** dan hasilnya dipakai **semua** modul. Biayanya ditanggung satu
kali, manfaatnya berulang.

---

## 2. Masalah yang sedang diselesaikan

### 2.1 Urutan sekarang membuat wawancara berjalan buta

| Tahap sekarang | Yang diketahui agent saat itu |
| --- | --- |
| 1. `/grill-me` Scope Pass | **Tidak ada.** Agent belum membaca kode sama sekali |
| 2. `/trace-existing-capabilities` | Isi kode, tetapi hanya sebatas modul yang sedang dibahas |
| 3. `/grill-me` Closure Pass | Isi kode modul tersebut |

Tahap 1 adalah tahap yang paling menentukan, karena di situlah scope dan aturan bisnis dikunci.
Justru di tahap itu agent paling sedikit tahu.

### 2.2 Audit per modul tidak melihat tetangga

`/trace-existing-capabilities` dirancang untuk satu modul. Ia menjawab "kebutuhan modul ini sudah
tersedia atau belum". Ia tidak menjawab "modul lain sedang memakai nama ini atau tidak".

Konflik antar modul justru lahir di ruang yang tidak dilihat siapa pun: nama entity, kepemilikan
data, dan alamat endpoint.

### 2.3 Pengetahuan tidak menumpuk

Setiap modul baru mengulang penyisiran dari nol. Modul kesepuluh tidak lebih mudah dikerjakan
daripada modul pertama, padahal sembilan modul sebelumnya sudah menyisir kode yang sama.

### 2.4 Contoh konflik yang dicegah

| Pola | Akibat nyata |
| --- | --- |
| Duplikasi konsep | Modul laboratorium membuat tabel pasien sendiri. Alamat pasien diperbarui di satu tempat, tetapi surat hasil laboratorium tetap memakai alamat lama |
| Nama kembar | Dua entity bernama mirip di area berbeda. Developer berikutnya menambahkan kolom ke tabel yang salah, dan datanya tidak pernah muncul di layar |
| Rebutan kepemilikan | Dua modul menulis ke tabel penjamin dengan aturan berbeda. Tagihan pasien yang sama menghasilkan angka berbeda tergantung layar mana yang dipakai |
| Endpoint bentrok | Dua controller memakai grup Swagger yang sama. Frontend memanggil endpoint yang salah dan baru ketahuan di lingkungan uji |

---

## 3. Usulan: satu skill baru, bukan empat

Godaan yang sengaja ditolak: menambahkan beberapa skill sekaligus, misalnya skill khusus
pengecekan nama, skill khusus pemetaan kepemilikan, dan skill khusus deteksi konflik.

Alasan menolaknya:

1. Ketiganya membaca sumber data yang sama, yaitu daftar entity. Memisahkannya berarti menyisir
   kode tiga kali untuk hasil yang saling bergantung.
2. Suite ini sudah punya tujuh skill. Menambah empat sekaligus membuat pengguna harus mengingat
   sebelas nama, dan skill yang jarang dipanggil akan cepat basi.
3. Pelajaran dari dokumen 01 dan 02: yang mahal bukan menulis skill, melainkan memeliharanya.

Karena itu usulannya satu skill dengan tiga mode.

| Yang ditambahkan | Nama | Fungsi |
| --- | --- | --- |
| Skill | `scan-system-registry` | Memindai seluruh sistem menjadi registry keadaan nyata |
| Slash command | `/qv-scan` | Pintasan dengan tiga mode: `full`, `refresh`, `focus <area>` |
| Folder rule | `.claude/rules/rule-prascan/` | Aturan wajib dan format baku keluaran |

Pengecekan nama, kepemilikan, dan konflik menjadi **berkas keluaran** skill ini, bukan skill
tersendiri.

### 3.1 Beda dengan skill yang sudah ada

| | `/qv-scan` (baru) | `/qv-trace` (sudah ada) |
| --- | --- | --- |
| Pertanyaan | Sistem ini isinya apa, siapa pemilik tiap bagian | Kebutuhan modul ini sudah tersedia atau belum |
| Cakupan | Seluruh sistem | Satu modul |
| Prasyarat | Tidak ada | Butuh decision log modul |
| Waktu jalan | Sebelum `/grill-me` | Sesudah `/grill-me` Scope Pass |
| Keluaran | `docs/system-registry/`, dipakai semua modul | `01-existing-capability-map.md`, satu modul |
| Frekuensi | Sekali, lalu diperbarui saat SHA berubah | Setiap modul |

Keduanya tidak saling menggantikan. `/qv-trace` justru menjadi lebih murah setelah registry ada,
karena ia tidak lagi menyisir dari nol.

---

## 4. Format keluaran: perbaikan dari format audit yang sudah dipakai

Format audit arsitektur yang selama ini dipakai di lingkungan Quilvian sudah benar arahnya. Ia
memisahkan yang sudah ada dari yang belum, memberi legenda status, dan menyusun isi sistem per
area. Format baru mempertahankan seluruh niat itu, dengan tujuh perbaikan.

| No | Yang diperbaiki | Sebelum | Sesudah |
| ---: | --- | --- | --- |
| 1 | Status berulang dalam banyak bentuk | `[SUDAH]`, `[SUDAH-IDENTITY]`, `[SUDAH/SESUAIKAN]`, `[SUDAH-MENDUKUNG]` | Satu sumbu, lima tingkat `L0`–`L4` |
| 2 | Fakta bercampur usulan | `[BARU-WAJIB]` sejajar dengan `[SUDAH]` dalam satu pohon | Registry hanya fakta. Usulan pindah ke blueprint modul |
| 3 | "Sudah" tidak berarti siap | `[SUDAH]` hanya berarti terdaftar di `ApplicationDbContext` | Kesiapan berlapis: model, configuration, migration, API, consumer |
| 4 | Klaim tanpa bukti | Tidak ada path maupun SHA | Setiap baris membawa path dan commit SHA |
| 5 | Pemilik data tidak tercatat | Tidak ada kolom pemilik | Kolom pemilik wajib; yang kabur menjadi zona konflik |
| 6 | Pohon ASCII untuk ratusan entity | Sulit dibaca, tidak bisa disaring atau dibandingkan | Tabel per area ditambah indeks abjad |
| 7 | Tidak jelas kapan berlaku | Hanya tanggal | Manifest memuat SHA dan status kesegaran |

### 4.1 Perbaikan nomor 3 adalah yang paling penting

Contoh dari sistem yang berjalan sekarang:

> `MstPatient` dan `MstBillingItemCategory` sama-sama akan ditulis `[SUDAH]` pada format lama.
>
> Kenyataannya jauh berbeda. `MstPatient` sudah ada tabelnya, ada endpoint-nya, dan ada layar
> yang memakainya. `MstBillingItemCategory` baru berupa kelas yang didaftarkan; belum tentu ada
> tabelnya di database dan belum ada endpoint-nya.
>
> Modul yang merencanakan pekerjaan berdasarkan `[SUDAH]` akan salah memperkirakan sisa
> pekerjaan pada kasus kedua.

Format baru menyatakan keduanya berbeda: `L4 Terpakai` dan `L1 Terdaftar`.

### 4.2 Tingkat kesiapan

| Tingkat | Nama | Syarat | Arti praktis |
| --- | --- | --- | --- |
| `L0` | Tidak ada | Tidak ditemukan | Belum ada apa pun |
| `L1` | Terdaftar | Model dan `DbSet` ada | Baru berupa kelas |
| `L2` | Berskema | `L1` + configuration + migration | Tabelnya nyata |
| `L3` | Berlayanan | `L2` + controller atau service | Bisa dipakai lewat API |
| `L4` | Terpakai | `L3` + ada pemakai nyata | Terbukti dipakai |
| `⚠` | Bermasalah | Ada lapisan yang melompat | Contoh: ada API tanpa migration |

### 4.3 Tujuh berkas keluaran

```text
docs/system-registry/
├── registry-manifest.md            # SHA, kesegaran, ringkasan angka
├── 01-peta-area-dan-modul.md       # area → modul → pemilik data
├── 02-entity-terdaftar.md          # daftar entity + tingkat kesiapan + bukti
├── 03-kepemilikan-data-bersama.md  # siapa boleh menulis, apa yang dilarang diduplikasi
├── 04-kavling-nama-dan-endpoint.md # prefix, nama terpakai, grup Swagger
├── 05-zona-konflik.md              # tujuh jenis konflik beserta risiko nyatanya
└── 06-indeks-entity.md             # indeks abjad untuk pencarian cepat
```

Format lengkapnya ada di
[`.claude/rules/rule-prascan/format-registry-sistem.md`](../../../.claude/rules/rule-prascan/format-registry-sistem.md).

---

## 5. Yang membuatnya wajib

Aturan tidak dijalankan oleh niat baik. Ia dijalankan oleh gerbang.

| Letak gerbang | Perilaku |
| --- | --- |
| `grill-me/SKILL.md` bagian "Gerbang wajib" | Berhenti sebelum pertanyaan pertama bila registry belum ada atau SHA-nya sudah berbeda |
| Kartu Konteks Pra-Wawancara | Wajib ditampilkan sebelum pertanyaan pertama, berisi lima bagian |
| Larangan bertanya | Hal yang sudah terjawab registry tidak boleh ditanyakan kepada pengguna |
| `trace-existing-capabilities/SKILL.md` | Wajib memeriksa kesegaran registry dan memulai dari registry |

### 5.1 Kartu Konteks Pra-Wawancara

Sebelum pertanyaan pertama, agent menampilkan satu layar berisi:

1. modul yang bersinggungan beserta pemilik datanya;
2. entity yang sudah siap dipakai ulang;
3. entity yang ada tetapi belum lengkap;
4. zona konflik yang menyentuh modul ini;
5. **daftar pertanyaan yang tidak akan diajukan** karena sudah terjawab registry.

Bagian kelima bukan hiasan. Ia memaksa agent membuktikan registry benar-benar dibaca, sekaligus
melindungi waktu pengguna dari pertanyaan yang jawabannya sudah ada di dalam kode.

### 5.2 Menjaga registry tetap murah

Registry mencakup seluruh sistem, sehingga cepat basi bila harus dipindai ulang seluruhnya.
Karena itu kesegaran diperiksa lewat SHA:

| Status | Syarat | Tindakan |
| --- | --- | --- |
| `SEGAR` | SHA manifest sama dengan `HEAD` kedua repository | Lanjut |
| `PERLU_REFRESH` | SHA berbeda, area dan modul tidak bertambah | `/qv-scan refresh`, hanya menyisir berkas pada `git diff --name-only <sha>..HEAD` |
| `KADALUARSA` | Belum pernah scan penuh, lewat 30 hari, atau ada area/modul baru | `/qv-scan full` |

Dengan begitu pemindaian penuh hanya terjadi sesekali, sedangkan pembaruan harian bersifat delta
dan murah.

---

## 6. Batas kewenangan skill baru

Skill ini hanya **melaporkan fakta**. Larangan yang mengikat:

1. Dilarang mengisi kata `wajib`, `prioritas`, atau `sprint`. Itu keputusan owner.
2. Dilarang mengusulkan entity baru.
3. Dilarang mengubah source aplikasi. Perintah git terbatas pada `status`, `log`, `diff`,
   `show`, `blame`, dan `rev-parse`.
4. Dilarang menebak pemilik modul. Yang tidak jelas ditulis `Belum ditentukan` dan otomatis
   menjadi zona konflik.
5. Dilarang menyatakan entity siap tanpa memeriksa configuration, migration, dan controller-nya.

Pemisahan ini penting dan sengaja tegas. Dokumen pemindaian yang sudah memuat kata "wajib"
membuat pembaca berikutnya mengira keputusan sudah diambil, padahal owner bisnis belum pernah
dilibatkan. Setelah itu, wawancara berubah menjadi formalitas untuk membenarkan dokumen yang
sudah terlanjur ditulis.

---

## 7. Berkas yang berubah

### 7.1 Berkas baru

| Berkas | Isi |
| --- | --- |
| `.claude/skills/scan-system-registry/SKILL.md` | Prosedur pemindaian, tiga mode, batas kewenangan, cara menentukan tingkat kesiapan |
| `.claude/rules/rule-prascan/README.md` | Ringkasan aturan dan alur setelah perubahan |
| `.claude/rules/rule-prascan/aturan-prascan-modul.md` | Aturan wajib, gerbang, Kartu Konteks, kewajiban skill lain |
| `.claude/rules/rule-prascan/format-registry-sistem.md` | Format baku tujuh berkas registry dan legenda `L0`–`L4` |
| `.claude/commands/qv-scan.md` | Slash command beserta pengamannya |
| `docs/agency/update-skills/04-prascan-registry-sistem.md` | Dokumen ini |

### 7.2 Berkas yang disunting

| Berkas | Perubahan |
| --- | --- |
| `.claude/skills/grill-me/SKILL.md` | Tambah bagian "Gerbang wajib: registry sistem harus segar"; daftar scope kini diperiksa terhadap Kartu Konteks |
| `.claude/skills/trace-existing-capabilities/SKILL.md` | Tambah bagian "Batas dengan `/scan-system-registry`"; wajib memeriksa kesegaran registry dan memulai dari registry |
| `.claude/PANDUAN-PENGGUNAAN-SKILLS.md` | Skill backend 6 menjadi 7, alur ditambah tahap 0, tabel command dan tabel effort diperbarui |
| `docs/agency/update-skills/README.md` | Indeks dokumen 04 |

### 7.3 Yang sengaja tidak diubah

| Berkas | Alasan |
| --- | --- |
| `.claude/rules/rule-output/` | Registry tetap tunduk pada lima aturan output. Tidak ada yang perlu diubah di sana |
| Source aplikasi, migration, `appsettings*` | Perubahan ini murni konfigurasi agent dan dokumentasi |
| Skill frontend `build-module-frontend` | Registry dibaca lewat lokasi canonical backend. Tidak ada adapter dan tidak ada salinan, sesuai keputusan DEC-USK-001 |

---

## 8. Risiko dan penanganannya

| Risiko | Penanganan |
| --- | --- |
| Registry cepat basi karena mencakup seluruh sistem | Mode `refresh` berbasis `git diff`. Pemindaian penuh hanya sesekali |
| Pemindaian penuh pertama memakan waktu | Dijalankan sekali. Modul kedua dan seterusnya menikmati hasilnya tanpa biaya ulang |
| Registry dianggap kebenaran mutlak lalu klaimnya tidak diperiksa lagi | Setiap baris membawa bukti path dan SHA. `/qv-trace` dan `/qv-verify` tetap wajib membuktikan sendiri |
| Gerbang menghambat pekerjaan mendesak | Ada jalan keluar eksplisit: pengguna menyatakan menerima risiko, dan pernyataan itu dicatat sebagai asumsi terbuka di decision log |
| Isi registry menyimpang menjadi dokumen usulan | Bagian 10 format registry memuat daftar larangan isi beserta tempat yang benar untuk tiap jenis isi |
| Registry dan capability map memuat isi yang sama | Batasnya ditulis tegas di kedua `SKILL.md`: registry seluas sistem tetapi dangkal, capability map sempit tetapi dalam |

---

## 9. Keputusan yang diminta

| No | Keputusan | Owner | Usulan | Status |
| ---: | --- | --- | --- | --- |
| 1 | Menyetujui `/qv-scan` sebagai gerbang wajib sebelum `/grill-me`, termasuk kemungkinan wawancara tertunda karena registry belum ada | Pemilik suite skill | Setujui. Penundaan beberapa menit jauh lebih murah daripada satu modul yang harus dirombak | Menunggu |
| 2 | Menetapkan siapa yang berwenang mengisi kolom **Pemilik data** pada berkas 03 | Pemilik arsitektur backend | Pemilik arsitektur backend menetapkan, agent hanya mengusulkan kandidat berdasarkan lokasi kode | Menunggu |
| 3 | Menetapkan masa berlaku pemindaian penuh | Pemilik suite skill | 30 hari, atau lebih cepat bila ada area/modul baru | Menunggu |

Selama ketiganya belum diputuskan, skill dan aturan sudah terpasang dan dapat dipakai, tetapi
kolom pemilik data akan banyak berisi `Belum ditentukan`.

---

## 10. Hubungan dengan dokumen audit arsitektur pasien rujukan

Dokumen audit arsitektur pasien rujukan yang beredar saat ini memuat dua jenis isi yang berbeda
sifatnya:

| Jenis isi | Contoh | Tempat yang benar setelah perubahan ini |
| --- | --- | --- |
| Keadaan sekarang | Daftar entity yang sudah terdaftar di `ApplicationDbContext` | Bahan `/qv-scan`, masuk `docs/system-registry/` |
| Usulan | `TrxPatientReferral`, `MstExternalHealthcareFacility`, prioritas sprint, rekomendasi index, urutan implementasi | **Masukan** untuk `/qv-grill pasien-rujukan`, lalu diputuskan owner dan ditulis `/qv-design` |

Cara memakainya:

1. Jalankan `/qv-scan full` untuk memastikan bagian "keadaan sekarang" benar-benar sesuai kode,
   lengkap dengan tingkat kesiapan dan bukti. Beberapa entri yang ditulis `[SUDAH]` kemungkinan
   ternyata baru `L1 Terdaftar`.
2. Jalankan `/qv-grill pasien-rujukan` dengan dokumen usulan itu sebagai bahan masukan, bukan
   sebagai keputusan. Owner bisnis yang menentukan mana yang benar-benar dibutuhkan.
3. Simpan dokumen aslinya sebagai arsip. Jangan dijadikan sumber kebenaran, karena isinya
   mencampur fakta dan usulan.

Dokumen aslinya sendiri sudah menyatakan batas ini dengan jujur: status "sudah" di sana hanya
berarti entity terdaftar sebagai `DbSet`, dan tidak menyatakan field, configuration, migration,
controller, service, atau aturan bisnisnya sudah lengkap. Justru itulah yang diperbaiki tingkat
kesiapan `L0`–`L4`.

---

## 11. Cara memakai setelah perubahan ini

```bash
cd NewQuilvianSystemBackend
claude --add-dir ../QuilvianSystemFrontendDev
```

Lalu, sekali di awal:

```text
/qv-scan full
```

Setiap kali memulai modul baru:

```text
/qv-grill pasien-rujukan scope Menerima pasien rujukan dari fasilitas luar melalui kiosk
```

Bila registry sudah tidak segar, agent akan berhenti dan meminta:

```text
/qv-scan refresh
```

Selebihnya, alur tetap sama seperti sebelumnya.
