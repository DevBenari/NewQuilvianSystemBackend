# Permintaan Penunjukan dan Tanda Tangan Tata Kelola Klinis — Modul Radiologi

| Field | Value |
|---|---|
| `request_id` | `RAD-REQ-002` |
| `tanggal` | 2026-09-09 |
| `pengaju` | Yoga Aji Pratama — penyusun blueprint Radiologi |
| `rujukan` | `00-interview-decisions.md` revision `8`; `02-requirement-completeness-assessment.md` `RAD-RCG-001-r2` |
| `status` | `terbuka` |
| `sifat` | Operasional. **Bukan** artefak desain — tidak masuk daftar hash manifest |
| Ditujukan kepada | Manajemen rumah sakit, untuk diteruskan kepada dokter penanggung jawab radiologi atau Komite Medis |

---

## 0. Ringkasan

Modul Radiologi menyentuh keselamatan pasien pada empat titik: siapa yang boleh menyatakan
sebuah pemeriksaan aman dilakukan, siapa yang boleh menyatakan sebuah bacaan benar, apa yang
terjadi dalam keadaan darurat, dan apa yang dihitung sebagai temuan yang mengancam nyawa.

Keempatnya sudah dirumuskan. Yang belum ada adalah **pihak klinis yang berwenang menandatangani
rumusannya**.

| No | Yang diminta | Menahan |
|---:|---|---|
| 1 | Menunjuk penanggung jawab tata kelola klinis untuk modul Radiologi | Butir 2, 3, dan 4 di bawah |
| 2 | Tanda tangan atas `RJ-BIL-GATE-DEC-004` | Pemakaian production seluruh gerbang keselamatan |
| 3 | Tanda tangan atas `RAD-DEC-008` — pelewatan gerbang darurat | Slice `S5` |
| 4 | Daftar apa saja yang dihitung temuan kritis | Slice `S11` |
| 5 | Sumber nilai awal aturan keselamatan | Kesiapan pakai modul |

Butir 1 adalah akar dari seluruhnya. Selama peran itu kosong, empat butir sisanya tidak dapat
bergerak.

---

## 1. Mengapa Ini Bukan Formalitas

Keempat butir di bawah menentukan hal yang berakibat langsung pada tubuh pasien.

| Keputusan | Yang ditentukannya |
|---|---|
| Gerbang keselamatan | Apakah seorang pasien yang sedang hamil boleh disinari; apakah pasien berimplan logam boleh masuk MRI; apakah pasien dengan riwayat alergi boleh diberi kontras |
| Pelewatan darurat | Siapa yang boleh memutuskan bahwa manfaat pemeriksaan melebihi risikonya, ketika pertanyaan keselamatan tidak dapat dijawab |
| Pengesahan hasil bacaan | Siapa yang boleh menyatakan sebuah bacaan benar sehingga dokter lain boleh mengambil keputusan pengobatan di atasnya |
| Daftar temuan kritis | Apa yang harus dikabarkan segera, dan apa yang boleh menunggu |

Bila kelak terjadi insiden, rumah sakit perlu menunjukkan bahwa pihak klinis ikut memutuskan
hal-hal ini. Persetujuan dari pemilik repository atau pemilik modul **tidak menggantikan**
kewenangan itu.

---

## 2. Butir 1 — Penunjukan Penanggung Jawab Tata Kelola Klinis

### Yang diminta

Menunjuk satu orang atau satu badan yang memegang wewenang klinis atas modul Radiologi. Lazimnya
dokter penanggung jawab instalasi radiologi, atau Komite Medis.

### Mengapa mendesak

Empat pertanyaan terbuka menunggu peran ini, dan dua di antaranya menahan pekerjaan desain
sepenuhnya:

| ID | Pertanyaan | Menahan |
|---|---|---|
| `DEC-RAD-001` | Apakah pelewatan gerbang darurat disetujui? | Slice `S5` |
| `DEC-RAD-002` | Apa saja yang dihitung temuan kritis? | Slice `S11` |
| `DEC-RAD-005` | Nilai awal aturan keselamatan dari mana? | Kesiapan pakai modul |
| `RAD-OPEN-009` | Perlukah pemantauan keterlambatan pesanan cito? | Perluasan setelah rilis pertama |

Peran ini juga akan menjadi **pengesah aturan keselamatan sehari-hari** menurut `RAD-DEC-005`.
Jadi penunjukannya bukan hanya untuk menandatangani sekali, melainkan untuk peran yang terus
berjalan.

---

## 3. Butir 2 — Tanda Tangan atas `RJ-BIL-GATE-DEC-004`

### Keadaan sekarang

Keputusan induk yang mengatur seluruh siklus hidup Radiologi berstatus `locked-draft` dengan
catatan:

> `Formal governance status: OPEN` — Radiology, Clinical Governance, dan Billing/Finance
> sign-off belum dilampirkan.

Aturannya sudah mengikat pekerjaan teknis, dan seluruh blueprint dibangun di atasnya. Yang belum
ada adalah tanda tangan resminya.

### Yang perlu ditinjau

| Butir | Isi yang dikunci |
|---|---|
| Gerbang keselamatan | Pemeriksaan ditolak bila butir wajib belum tuntas |
| Sifat fail-closed | Pemeriksaan **juga** ditolak bila aturan keselamatannya belum ditetapkan sama sekali |
| Pemisahan pengambilan citra dan pembacaan | Selesai memeriksa tidak sama dengan hasil sudah sah |
| Koreksi berversi | Hasil yang sudah dirilis tidak pernah ditimpa |

### Satu hal yang perlu dipahami sebelum menandatangani

Sifat *fail-closed* berarti: **selama sebuah alat belum punya aturan keselamatan yang disahkan,
seluruh pemeriksaan dengan alat itu akan ditolak sistem.**

Ini disengaja. Tidak adanya aturan berarti belum ada yang menetapkan apa yang aman, bukan
berarti semuanya aman.

Konsekuensinya nyata: modul tidak dapat dipakai sebelum aturan keselamatan diisi dan disahkan.
Itu bukan kerusakan, melainkan pilihan yang perlu diketahui dan disetujui.

---

## 4. Butir 3 — Tanda Tangan atas `RAD-DEC-008`, Pelewatan Gerbang Darurat

### Rumusan yang diajukan

| Syarat | Ketentuan |
|---|---|
| Siapa yang boleh mengesahkan | Dokter penanggung jawab pasien atau dokter jaga senior. **Bukan** petugas radiologi |
| Alasan | Wajib ditulis, tidak boleh kosong, tidak boleh pilihan seragam |
| Penandaan | Study ditandai permanen "dilakukan dengan pelewatan gerbang keselamatan" |
| Butir yang dilewati | Dicatat satu per satu, bukan sebagai satu centang menyeluruh |
| Tinjauan | Seluruh pelewatan masuk daftar tinjauan berkala |

Pelewatan **tidak** mengubah jawaban butir keselamatan menjadi aman. Butir yang dilewati tetap
tercatat belum terjawab; yang berubah hanyalah bahwa pemeriksaan diizinkan berjalan.

### Mengapa pelewatan diusulkan ada, bukan dilarang

Alasannya bukan kemudahan, melainkan kejujuran data.

> **Contoh.** Pukul 02.15 seorang korban kecelakaan tiba di IGD dalam keadaan tidak sadar dan
> butuh CT-Scan kepala segera. Riwayat alergi kontras tidak dapat ditanyakan — pasien tidak
> sadar dan tidak ada keluarga.
>
> Bila pelewatan dilarang sama sekali, petugas hanya punya dua pilihan: menunda pemeriksaan yang
> menyelamatkan nyawa, atau mengisi butir itu sebagai "tidak berlaku" padahal sebenarnya
> berlaku.
>
> Pilihan kedua yang lebih sering diambil. Dan begitu diambil, **jejak auditnya hilang
> sepenuhnya** — rekam datanya terlihat seperti pemeriksaan normal yang lolos wajar.

Pelewatan yang tercatat jelas jauh lebih baik daripada pemalsuan yang tidak terlihat.

### Yang perlu diputuskan

1. Apakah konsep pelewatan darurat disetujui?
2. Bila ya, apakah syarat pengesahnya sudah tepat — DPJP atau dokter jaga senior?
3. Bila tidak, bagaimana kasus seperti contoh di atas ditangani?

### Catatan kewenangan

`RAD-DEC-008` saat ini berstatus `approved` **untuk keperluan desain saja**, disetujui pemilik
modul tanpa tanda tangan pendamping klinis. Statusnya sengaja dicatat terbuka pada decision log.

**Ia tidak boleh dipakai di lingkungan production sebelum butir ini ditandatangani.**

---

## 5. Butir 4 — Daftar Temuan Kritis

### Yang diminta

Daftar apa saja yang dihitung sebagai temuan kritis radiologi, yaitu temuan yang wajib
dikabarkan segera kepada dokter pengirim.

### Mengapa ini mengubah rancangan, bukan sekadar mengisi data

| Bila jawabannya | Rancangannya |
|---|---|
| Ada daftar resmi | Dibutuhkan data induk temuan kritis, dan hasil bacaan menunjuk ke butir daftar itu |
| Tidak ada daftar, diserahkan penilaian radiolog | Cukup satu penanda pada hasil bacaan |

Keduanya rancangan yang berbeda, dan menggantinya belakangan berarti mengubah struktur data yang
sudah terisi. Karena itu slice temuan kritis sengaja **belum dirancang** sampai butir ini
dijawab.

### Butir yang lazim, sebagai bahan pembanding saja

Daftar berikut **bukan usulan kebijakan** dan wajib diverifikasi terhadap keadaan rumah sakit
yang sebenarnya:

- perdarahan intrakranial;
- pneumotoraks luas;
- udara bebas intraabdomen;
- diseksi aorta.

### Mekanismenya sudah diputuskan

Yang ditanyakan hanya **isinya**. Cara kerjanya sudah dikunci `RAD-DEC-004` dan `RAD-DEC-010`:
temuan kritis ditandai, dikirim sebagai pemberitahuan berstatus, dan wajib diakui dokter
pengirim. Rilis hasil **tidak** ditahan menunggu pengakuan. Radiolog tetap wajib menghubungi
langsung, dan sistem merekam bahwa kontak itu terjadi.

---

## 6. Butir 5 — Sumber Nilai Awal Aturan Keselamatan

### Yang diminta

Apakah isi awal aturan keselamatan disiapkan tim sebagai data bawaan, atau seluruhnya diketik
admin rumah sakit sendiri?

### Latar belakangnya

`RJ-BIL-DEC-014` menyatakan bahwa baseline standardisasi Indonesia bersifat **tidak otoritatif**
dan wajib diverifikasi terhadap SOP rumah sakit yang sebenarnya sebelum dipakai.

Blueprint menyusun usulan pemetaan awal — misalnya skrining kehamilan wajib untuk CT-Scan,
pemeriksaan implan logam wajib untuk MRI — tetapi menandainya jelas sebagai **usulan**, bukan
kebijakan.

### Usulan cara penyelesaian

Sediakan data bawaan berstatus `Draf`, sehingga tetap harus disahkan penanggung jawab klinis
sebelum berlaku. Dengan begitu tidak ada satu pun aturan yang aktif tanpa persetujuan, tetapi
admin juga tidak mulai dari halaman kosong.

Pengaju tidak memaksakan cara ini. Yang penting diketahui: **tanpa aturan yang disahkan, modul
tidak dapat menjalankan satu pun pemeriksaan.**

---

## 7. Bila Penerima Merasa Bukan Wewenangnya

Bila dokumen ini sampai kepada pihak yang merasa bukan pemegang wewenang klinis, mohon
diteruskan, dan sebutkan kepada siapa.

Sebaliknya, bila salah satu penerima memang merangkap dokter penanggung jawab radiologi, cukup
nyatakan hal itu — dan butir 1 tertutup dengan nama yang bersangkutan.

---

## 8. Cara Menjawab

| Butir | Yang dibutuhkan |
|---:|---|
| 1 | Nama orang atau badan yang ditunjuk |
| 2 | Persetujuan atau keberatan atas `RJ-BIL-GATE-DEC-004` |
| 3 | Persetujuan, keberatan, atau perbaikan atas rumusan `RAD-DEC-008` |
| 4 | Daftar temuan kritis, atau pernyataan bahwa penilaiannya diserahkan pada radiolog |
| 5 | Pilihan cara pengisian nilai awal aturan keselamatan |

Butir 2 sampai 5 dapat dijawab bertahap. Butir 1 sebaiknya dijawab lebih dulu, karena
menentukan siapa yang menjawab sisanya.

---

## 9. Riwayat Dokumen

| Revision | Tanggal | Perubahan | Status |
|---:|---|---|---|
| 1 | 2026-09-09 | Permintaan pertama. Lima butir, seluruhnya berakar pada penunjukan yang belum ada. | `terbuka` |
