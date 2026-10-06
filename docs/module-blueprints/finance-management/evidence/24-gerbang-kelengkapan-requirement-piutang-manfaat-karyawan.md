# Gerbang Kelengkapan Requirement — Piutang Manfaat Karyawan (`EPIC FIN-04`)

| Field | Nilai |
|---|---|
| Blueprint | `FIN-BP-001` Finance Management, revisi 16, bentuk `SINGLE` |
| Yang dinilai | `EPIC FIN-04` — piutang manfaat karyawan, dipecah menjadi delapan slice |
| Tanggal | 5 Oktober 2026 |
| Dijalankan karena | `design-business-module` menolak menggambar tanpa bukti gerbang ini. `FIN-CQ-03` menandai requirement rumpun ini sebagai satu-satunya titik modul yang belum pernah dinilai pemilik domainnya |
| SHA bukti | Backend `46fa2a91` (branch `Yasmina`), frontend `0ed37b5c4` (branch `yasmina`). Kedua working tree **tidak bersih** — lihat bagian 2 |
| Kesiapan keseluruhan | **`PARTIALLY_READY`** — tiga slice siap saat gerbang dijalankan; menjadi **lima** sesudah `FIN-DEC-178` dan `FIN-DEC-179` ditutup hari yang sama, lihat **bagian 11 (Addendum)** |
| Arsitektur domain | `DOMAIN_ARCHITECTURE_NOT_RUN`, meneruskan deviasi tercatat pemilik 20 September 2026. Untuk slice yang dinyatakan siap di bawah, arsitektur domain **tidak dibutuhkan** karena ketiganya murni di dalam batas Finance. Untuk slice lintas konteks yang masih terblokir, `hospital-domain-architect` ditawarkan sesudah blocker-nya tertutup |
| Sifat berkas | Penilaian, bukan kontrak dan bukan desain. **Tidak** membuat entity, ERD, endpoint, migration, UI, atau task |

---

## 1. Scope penilaian

Yang dinilai adalah kemampuan **mencatat, menagih, dan menyelesaikan tagihan layanan rumah sakit
atas pegawai dan keluarganya**. Supaya satu bagian yang belum selesai tidak menahan seluruh rumpun,
kemampuan itu dipecah menjadi delapan slice berikut.

| Slice | Kemampuan |
|---|---|
| `S1` | Mencatat piutang **porsi pegawai** dari serah terima Billing |
| `S2a` | Perjanjian dan jadwal cicilan beserta persetujuannya |
| `S2b` | Aturan penumpukan tunggakan dan batas potongan per periode |
| `S3` | Pelunasan lewat potongan gaji, dua arah dengan HR |
| `S4a` | Menghitung status "bebas tanggungan" per pegawai |
| `S4b` | Gerbang berhenti kerja di HR yang membaca status itu |
| `S5` | Koreksi ketika pemilik manfaat ternyata salah orang |
| `S6` | Memasukkan piutang karyawan lama lewat batch migrasi |
| `S7` | Mencatat dan menutup **porsi yang ditanggung benefit** atas penjamin "RS Benefit" |
| `S8` | Penghapusan buku dan penyesuaian piutang karyawan |

**Di luar penilaian ini:** piutang internal pegawai atau dokter yang tidak lahir dari kunjungan pasien
(kasbon dan sejenisnya). Itu kemampuan lain, dilacak `FIN-OQ-085`, dan belum pernah digali.

---

## 2. Bukti yang dipakai

| Jenis bukti | Isi | Wewenangnya |
|---|---|---|
| Keputusan pemilik Finance | `FIN-DEC-162`..`177`, diputuskan interaktif 5 Oktober 2026, `approved` sisi Finance | **Otoritatif** untuk apa yang seharusnya dibangun di sisi Finance |
| Keputusan pemilik lama | `FIN-DEC-006` (`approved` sisi Finance, 20 September 2026, konfirmasi Billing + HR **belum turun**), `FIN-DEC-016`, `FIN-DEC-012`, `FIN-DEC-010` | Otoritatif sebatas sisi Finance |
| Bukti implementasi terkini | `01-existing-capability-map.md` bagian 21: `FIN-CAP-067`..`085`, dibaca langsung dari source 5 Oktober 2026 | **Otoritatif** untuk apa yang saat ini ada |
| Bukti sistem lama | Praktik V1 menandai tab Karyawan lewat penjamin bernama "RS Benefit" (`FIN-CAP-085`); menu "Piutang Karyawan" pada `Keuangan.md` bagian 7.3 | Bukti legacy, **bukan** kebijakan yang disahkan |
| Permintaan lintas modul | `evidence/02` bagian 3 (20 September 2026, belum dijawab) dan `evidence/23` (**draf, belum dikirim**) | Menunjukkan keputusan yang diminta, bukan keputusan yang sudah ada |
| Pekerjaan yang baru selesai | `BE-FIN-FIX-002` (identitas debitur penjamin perusahaan) dan `FE-FIN-FIX-002` (nilai jenis debitur) | Bukti implementasi, sudah ter-build |

**Yang TIDAK tersedia, dan ini penting dinyatakan:**

- **Tidak ada SOP atau kebijakan rumah sakit yang disahkan** tentang manfaat kesehatan pegawai yang
  dapat dirujuk. Seluruh aturan di bawah berasal dari jawaban pemilik Finance pada wawancara, bukan
  dari dokumen kebijakan.
- **Tidak ada notulen rapat** khusus rumpun ini.
- **Tidak ada jawaban** dari pemilik Billing, Registrasi, maupun HR sampai hari ini.
- Kedua working tree tidak bersih (pekerjaan `BE-FIN-FIX-001`/`FE-FIN-FIX-001` belum di-commit),
  sehingga sebagian bukti implementasi bertambat pada working tree, bukan pada SHA.
- `indonesia-hospital-domain-reference` **tidak** dipakai pada penilaian ini, karena pertanyaan yang
  tersisa bukan soal kelaziman praktik rumah sakit Indonesia, melainkan soal kontrak antarmodul di
  dalam sistem ini sendiri. Karena itu tidak ada `REFERENCE_ONLY` observation yang dikutip.

---

## 3. Temuan kelengkapan pada 18 dimensi

Dinilai untuk kemampuan utuh. Kolom terakhir menyebut slice yang terdampak bila dimensinya belum lengkap.

| # | Dimensi | Status | Temuan | Slice terdampak |
|---:|---|---|---|---|
| 1 | Tujuan | `CONFIRMED` | Tagihan layanan RS atas pegawai atau keluarganya yang menjadi tanggungan pegawai (`FIN-DEC-162`) | — |
| 2 | Aktor | `CONFIRMED` | Registrasi menetapkan pemilik manfaat; Billing menghitung porsi; Finance mencatat, menagih, dan menyetujui; HR memotong gaji (`FIN-DEC-171`, `164`, `166`, `167`) | — |
| 3 | Pemicu / prasyarat | `CONFIRMED` | Finalisasi tagihan sebuah kunjungan yang pemilik manfaatnya sudah ditetapkan saat pendaftaran | — |
| 4 | Alur utama | `CONFIRMED` | Sembilan langkah, tercatat beserta angka contohnya pada `evidence/23` bagian 2 | — |
| 5 | Alur alternatif / exception | **`MISSING` sebagian** | Potongan gagal **sudah** diputuskan (`FIN-DEC-168`). Yang belum: koreksi pemilik manfaat di sisi Registrasi/Billing (`FIN-OQ-097`), pegawai meninggal (`FIN-OQ-094`), dan tanggungan tanpa nomor identitas (`FIN-OQ-096`) | `S5`, dan jalur tepi `S1` |
| 6 | Data minimum | **`MISSING` sebagian** | Kolom di Finance **sudah ada** dan ber-constraint (`FIN-CAP-067`). Yang belum: siapa mengisi pemilik manfaat dan dari data apa (`FIN-OQ-096`), serta kunci pegawai pada templat migrasi (`FIN-OQ-098`) | `S1`, `S6` |
| 7 | Aturan bisnis / validation | **`MISSING` sebagian** | Nominal piutang pegawai **sudah** diputuskan (`FIN-DEC-164`). Yang belum: dari mana sisa plafon dibaca dan plafon mana yang berlaku untuk tanggungan (`FIN-OQ-087`), serta batas potongan per periode (`FIN-OQ-092`) | `S1`, `S2b` |
| 8 | Status / perubahan status | `CONFIRMED` | Memakai status `FinReceivable` yang sudah berjalan; cicilan gagal menjadi tertunggak tanpa mengubah perjanjian (`FIN-DEC-168`) | — |
| 9 | Peran / authorization | `CONFIRMED` | Siapa pun yang berhak AR dapat melihat, tanpa penyamaran (`FIN-DEC-172`); perjanjian dan penghapusan memakai pengaju–penyetuju lewat konfigurasi hak akses (`FIN-DEC-166`, `176`) | — |
| 10 | Dependency antarmodul | **`PROPOSED`** | Billing menerbitkan **dua** serah terima per tagihan (`FIN-DEC-177`) — ini keputusan Finance yang **belum dikonfirmasi pemilik Billing** (`evidence/23` butir `B1`/`B2`) | `S1`, `S7` |
| 11 | Integrasi internal / eksternal | **`MISSING`** | Arah HR → Finance untuk hasil potongan tidak ada pengirim dan tidak ada penerima (`FIN-CAP-074`); bentuk pesannya belum disepakati (`FIN-OQ-091`) | `S3` |
| 12 | Hasil akhir | `CONFIRMED` | Piutang pegawai lunas, dan status bebas tanggungan dihitung dari saldo piutang aktif (`FIN-DEC-170`) | — |
| 13 | Pembatalan / koreksi | **`MISSING` sebagian** | Prinsipnya **sudah**: pembalikan lalu serah terima baru, Finance tidak mengganti nama debitur (`FIN-DEC-173`). Yang belum: apakah Billing memang dapat membatalkan penuh lalu menerbitkan ulang atas orang lain (`FIN-CAP-078` `Unknown`), dan siapa mengembalikan potongan yang sudah terjadi (`FIN-OQ-097`) | `S5` |
| 14 | Audit / histori | `CONFIRMED` | `IdentityModel` plus buku mutasi piutang yang sudah berjalan (`FinReceivableMovement`) | — |
| 15 | Notifikasi | `NON_BLOCKING_STANDARD` | Belum dibahas. Pemberitahuan ke pegawai tentang jadwal potongan wajar ada, tetapi tidak mengubah model maupun lifecycle | — |
| 16 | Dampak billing / charge | **`MISSING` sebagian** | Pembagian porsi **sudah** diputuskan (`FIN-DEC-164`, `177`). Yang belum: cara menutup piutang atas "RS Benefit", yang bila dibiarkan akan menetap di umur piutang dan menggelembungkan saldo (`FIN-OQ-101`) | `S7` |
| 17 | Dampak keselamatan klinis | **Tidak relevan** | Rumpun ini finansial. Tidak ada keputusan klinis, tidak ada data diagnosis yang dipakai, dan `FIN-DEC-172` secara sadar tidak menampilkan rincian medis | — |
| 18 | Pelaporan / traceability | `CONFIRMED` | Umur piutang empat kelompok (`FIN-DEC-010`) dan segmen subledger "Manfaat Karyawan" yang sudah ada (`FIN-CAP-082`) | — |

---

## 4. Butir `CONFIRMED`, `PROPOSED`, `MISSING`, dan `CONFLICT`

### `CONFIRMED` — 14 butir

Seluruhnya dari keputusan pemilik Finance 5 Oktober 2026, dan berlaku **hanya** sejauh batas Finance.

| Butir | Sumber |
|---|---|
| Arti "AR karyawan" | `FIN-DEC-162` |
| Pelunasan lewat potong gaji | `FIN-DEC-163` |
| Piutang pegawai = kelebihan di atas plafon | `FIN-DEC-164` |
| Boleh lunas sekaligus atau dicicil, cicilan selalu berdasarkan perjanjian | `FIN-DEC-165` |
| Perjanjian dicatat di Finance, pengaju ≠ penyetuju, isi minimalnya empat hal | `FIN-DEC-166` |
| Arah dan sifat otomatis hasil potongan, dan kiriman ganda harus aman | `FIN-DEC-167` |
| Cicilan gagal tetap terhutang dan ikut periode berikutnya | `FIN-DEC-168` |
| Pegawai berhenti: potong hak akhir, sisa wajib dilunasi, tanpa hapus otomatis | `FIN-DEC-169` |
| Status bebas tanggungan dihitung, tidak diisi tangan | `FIN-DEC-170` |
| Registrasi yang menetapkan pemilik manfaat, Finance tidak menentukan ulang | `FIN-DEC-171`, `FIN-DEC-016` |
| Keterlihatan setara piutang penjamin, tanpa penyamaran, tanpa rincian medis | `FIN-DEC-172` |
| Koreksi lewat pembalikan dan serah terima baru | `FIN-DEC-173` |
| Pembagian dua porsi: pegawai lewat `EMPLOYEE_BENEFIT`, benefit lewat "RS Benefit" | `FIN-DEC-177` |
| Penghapusan buku memakai pengaju–penyetuju `FIN-DEC-012` | `FIN-DEC-176` |

### `PROPOSED` — 3 butir

| Butir | Mengapa belum `CONFIRMED` |
|---|---|
| Billing menerbitkan **dua** serah terima per tagihan manfaat karyawan | Keputusan Finance (`FIN-DEC-177`); pemilik Billing belum menyatakan sanggup |
| Perluasan `BilArHandoff` dengan `EMPLOYEE_BENEFIT` dan dua kolom pemilik manfaat | `FIN-DEC-006` `approved` sisi Finance sejak 20 September 2026; konfirmasi Billing belum turun |
| Memakai kartu penjamin perusahaan yang sudah ada sebagai jalan menandai pegawai | Usulan Finance pada `evidence/23` butir `R1`; pemilik Registrasi belum menjawab |

### `MISSING` — 10 butir

| Butir | Decision ID |
|---|---|
| Sumber sisa plafon, dan plafon mana yang berlaku untuk tanggungan | `FIN-OQ-087` |
| Bentuk pesan dan kesanggupan HR menerima jadwal serta mengirim hasil potongan | `FIN-OQ-091` |
| Batas potongan per periode saat tunggakan menumpuk, dan siapa menetapkannya | `FIN-OQ-092` |
| Perlakuan bila pegawai meninggal dengan sisa piutang | `FIN-OQ-094` |
| Mesin gerbang berhenti di HR, termasuk jalur pengecualiannya | `FIN-OQ-095` |
| Tautan kunjungan ke profil pegawai dan hubungan keluarganya di Registrasi | `FIN-OQ-096` |
| Alur koreksi dan pengembalian potongan pada pegawai yang salah | `FIN-OQ-097` |
| Kunci pegawai pada templat batch migrasi | `FIN-OQ-098` |
| Cara menutup piutang atas penjamin "RS Benefit" dan perlakuan akuntansinya | `FIN-OQ-101` |
| Keberadaan dan jenis master "RS Benefit" pada data V2 | `FIN-OQ-102` |

### `CONFLICT` — 1 butir, sudah diselesaikan

| Butir | Penyelesaian |
|---|---|
| `FIN-DEC-164` menyatakan porsi benefit "**tidak** menjadi piutang" tanpa kualifikasi, sedangkan `FIN-DEC-177` menjadikannya piutang atas "RS Benefit" | **Sudah diselesaikan 5 Oktober 2026**: klausa itu diperhalus menjadi "tidak menjadi piutang **pegawai**", dan anotasinya ditulis pada kedua baris `FIN-DEC-164` di decision log. Tidak ada konflik yang masih terbuka |

---

## 5. Klasifikasi dampak gap

| Decision ID | Dampak | Mengapa |
|---|---|---|
| `FIN-OQ-087` | **`BLOCKING`** | Menentukan nominal piutang, jadi menyentuh konsekuensi billing dan integritas data. Salah nominal berarti pegawai dipotong gaji dengan jumlah yang salah |
| `FIN-OQ-091` | **`BLOCKING`** | Kontrak integrasi antarmodul dan lifecycle pelunasan. Tanpa ini piutang tidak punya cara berpindah ke "terbayar" |
| `FIN-OQ-092` | **`BLOCKING`** untuk `S2b` | Menentukan siapa memegang batas potongan, sehingga menentukan apakah model Finance perlu field pembatas — yaitu ownership dan relasi entity |
| `FIN-OQ-094` | `NON_BLOCKING_STANDARD` | Kasus tepi. Jalur amannya sudah ada: sisa tetap piutang dan penghapusan hanya lewat persetujuan (`FIN-DEC-169`) |
| `FIN-OQ-095` | **`BLOCKING`** untuk `S4b` | Alur kerja yang sulit dibatalkan: pegawai yang sudah selesai administrasi berhentinya praktis tidak dapat ditagih lagi |
| `FIN-OQ-096` | **`BLOCKING`** | Menentukan sumber pemilik manfaat, sedangkan constraint database mewajibkan kolom itu terisi. Ini ownership dan integritas data |
| `FIN-OQ-097` | **`BLOCKING`** untuk `S5` | Menyentuh pembalikan yang melibatkan uang yang sudah dipotong dari orang yang salah |
| `FIN-OQ-098` | **`BLOCKING`** untuk `S6` | Tanpa kunci pegawai, baris migrasi tidak dapat memenuhi constraint pemilik manfaat |
| `FIN-OQ-101` | **`BLOCKING`** untuk `S7` | Konsekuensi billing dan integritas laporan: piutang yang tidak pernah ditutup akan menggelembungkan saldo dan umur piutang |
| `FIN-OQ-102` | **`BLOCKING`** untuk `S7` | Menentukan master mana yang dirujuk identitas debitur, yaitu relasi entity |
| `FIN-OQ-099` | `NON_BLOCKING_STANDARD` | Bentuk layar. Tidak mengubah model, lifecycle, maupun authorization; `FIN-DEC-175` sudah menetapkan wewenangnya milik pemilik |
| `FIN-OQ-085` | `NON_BLOCKING_STANDARD` | Kemampuan lain yang sengaja dikeluarkan dari scope |
| `FIN-OQ-088` | `NON_BLOCKING_STANDARD` | Sudah terjawab arahnya oleh `FIN-DEC-177`; sisanya dilacak `FIN-OQ-101` |
| Notifikasi ke pegawai | `NON_BLOCKING_STANDARD` | Dapat dinyatakan eksplisit nanti tanpa mengubah makna domain |

**Tidak ada butir `CONFIGURABLE_DEFAULT`.** Pilihan yang tersisa bukan hal yang wajar berbeda antar unit;
semuanya keputusan tunggal milik pemilik modul tertentu.

---

## 6. Decision Log

Penilaian ini **tidak** mencetak ruang ID baru seperti `DEC-EMP-001`. Blueprint ini sudah memakai
`FIN-OQ-###` untuk pertanyaan terbuka berpemilik, dan kontrak gerbang mengizinkan **mempertahankan**
ID yang sudah stabil. Menambah ruang ID kedua justru akan membuat satu pertanyaan punya dua nomor.

Dua entri yang paling menentukan ditulis lengkap di bawah; sisanya sudah tercatat utuh pada
`00-interview-decisions.md` pass 5 Oktober 2026.

**Decision ID: `FIN-OQ-096`**

- **Pertanyaan:** Apakah pemilik Registrasi menyetujui dan sanggup menautkan kunjungan ke profil
  pegawai beserta hubungan keluarganya, sebagai sumber pemilik manfaat?
- **Kemampuan terdampak:** `S1` (pencatatan piutang pegawai), dan lewat itu seluruh slice yang
  membutuhkan identitas pegawai.
- **Bukti saat ini:** Registrasi **sudah** punya kartu hubungan pasien–perusahaan dengan nomor karyawan,
  plan, status layak, dan plafon (`FIN-CAP-070`). Yang tidak ada: hubungan keluarga, dan rujukan ke
  profil pegawai HR — nomor karyawan hanya teks.
- **Usulan baseline:** Menyambungkan mekanisme yang sudah ada, bukan membuat yang baru.
- **Dampak:** Ownership data, integritas data, dan relasi entity. Constraint database Finance
  mewajibkan pemilik manfaat terisi untuk jenis ini.
- **Pemilik:** Pemilik Registrasi, bersama pemilik HR untuk sumber datanya.
- **Status:** `OPEN`. Ditanyakan pada `evidence/23` butir `R1`..`R6`, **berkas belum dikirim**.
- **Dampak implementasi:** `S1` berhenti. `S2a`, `S4a`, dan `S8` boleh berjalan.

**Decision ID: `FIN-OQ-101`**

- **Pertanyaan:** Bagaimana piutang atas penjamin "RS Benefit" dilunasi atau ditutup, dan apa
  perlakuan akuntansinya?
- **Kemampuan terdampak:** `S7`.
- **Bukti saat ini:** `FIN-DEC-177` memutuskan porsi benefit dicatat sebagai piutang atas "RS Benefit",
  meneruskan praktik V1. Cara menutupnya belum pernah dibahas, baik di V1 maupun di blueprint ini.
- **Usulan baseline:** Tidak ada usulan yang aman diberikan. Penjamin ini adalah rumah sakit sendiri,
  sehingga pilihan antara penghapusan berkala, pelunasan internal, dan jurnal beban benefit adalah
  keputusan akuntansi, bukan keputusan teknis.
- **Dampak:** Konsekuensi billing, integritas pelaporan, dan umur piutang. Bila tidak pernah ditutup,
  saldo piutang rumah sakit akan menggelembung oleh angka yang tidak akan pernah tertagih.
- **Pemilik:** Pemilik Finance bersama pemilik Accounting.
- **Status:** `OPEN`, dibuka 5 Oktober 2026.
- **Dampak implementasi:** `S7` berhenti. Porsi pegawai (`S1`) tidak bergantung padanya.

---

## 7. Kesiapan per slice

| Slice | Kesiapan | Blocker |
|---|---|---|
| `S2a` — Perjanjian dan jadwal cicilan | **`READY_FOR_DOMAIN_DESIGN`** | — |
| `S4a` — Menghitung status bebas tanggungan | **`READY_FOR_DOMAIN_DESIGN`** | — |
| `S8` — Penghapusan buku dan penyesuaian | **`READY_FOR_DOMAIN_DESIGN`** (nol desain baru) | — |
| `S1` — Mencatat piutang porsi pegawai | `BUSINESS_DECISION_REQUIRED` | `FIN-OQ-096`, `FIN-OQ-087`, dan konfirmasi Billing atas `FIN-DEC-177`/`FIN-DEC-006` |
| `S2b` — Penumpukan tunggakan dan batas potongan | `BUSINESS_DECISION_REQUIRED` | `FIN-OQ-092` |
| `S3` — Pelunasan lewat potongan gaji | `BUSINESS_DECISION_REQUIRED` | `FIN-OQ-091` |
| `S4b` — Gerbang berhenti kerja di HR | `BUSINESS_DECISION_REQUIRED` | `FIN-OQ-095` |
| `S5` — Koreksi pemilik manfaat | `BUSINESS_DECISION_REQUIRED` | `FIN-OQ-097`, dan `FIN-CAP-078` yang masih `Unknown` |
| `S6` — Batch migrasi saldo lama | `BUSINESS_DECISION_REQUIRED` | `FIN-OQ-098` |
| `S7` — Porsi benefit atas "RS Benefit" | `BUSINESS_DECISION_REQUIRED` | `FIN-OQ-101`, `FIN-OQ-102` |

Kesiapan keseluruhan **`PARTIALLY_READY`**: tiga slice siap, tujuh terblokir.

### Mengapa `S2a`, `S4a`, dan `S8` dinyatakan siap

Ketiganya berada **seluruhnya di dalam batas Finance** dan tidak menunggu jawaban modul lain:

| Slice | Yang membuatnya mandiri |
|---|---|
| `S2a` | Isi perjanjian, siapa mengajukan, siapa menyetujui, dan larangan menyetujui sendiri semuanya sudah diputuskan (`FIN-DEC-165`, `166`). Bentuk perjanjian tidak bergantung pada cara piutangnya lahir |
| `S4a` | Rumusnya sudah dikunci: dihitung dari saldo piutang aktif per pemilik manfaat, dan tidak boleh diisi tangan (`FIN-DEC-170`). Yang terblokir adalah **pembacanya** di HR (`S4b`), bukan perhitungannya |
| `S8` | Sudah `CONFIRMED` (`FIN-DEC-176`) dan kemampuannya sudah ada serta siap dipakai ulang (`FIN-CAP-077`). Tidak ada yang perlu digambar — cukup dinyatakan bahwa jalur yang ada berlaku juga untuk jenis debitur ini |

### Dependency yang MUST dinyatakan

Ketiga slice siap itu **tidak dapat dipakai di produksi** sebelum `S1` terbuka, karena tanpa `S1` tidak
ada piutang karyawan yang bisa dilekati perjanjian atau dihitung status bebas tanggungannya. Yang
dibolehkan gerbang ini adalah **merancangnya**, bukan menyatakannya siap pakai. Urutan nyatanya tetap
`S1` lebih dulu, lalu `S2a` dan `S4a`.

Karena itu `plan-module-delivery` **MUST NOT** menjadwalkan `S2a`, `S4a`, atau `S8` sebagai slice yang
dapat dirilis sendiri. Ketiganya bahan desain yang siap, bukan gelombang rilis yang siap.

---

## 8. Apa yang boleh berjalan, dan apa yang harus berhenti

**Boleh berjalan sekarang:**

1. `design-business-module` untuk `S2a`, `S4a`, dan `S8` saja — entity perjanjian beserta jadwal
   cicilannya, cara menghitung status bebas tanggungan, dan pernyataan bahwa jalur penghapusan buku yang
   ada berlaku untuk jenis debitur ini.
2. Mengirim `evidence/23` kepada pemilik Billing, Registrasi, dan HR.

**Harus berhenti:**

1. Seluruh desain `S1`, `S2b`, `S3`, `S4b`, `S5`, `S6`, dan `S7`.
2. Implementasi apa pun untuk seluruh slice, termasuk yang siap — gerbang ini menilai kesiapan
   **desain**, bukan kesiapan build.
3. `EPIC FIN-04` tetap `OPEN DECISION` pada `04-prd-to-mvp.md` dan kedua roadmap, dan **MUST NOT**
   masuk gelombang pengiriman mana pun. Penilaian ini tidak mengubah status itu.

---

## 9. Keputusan pemilik yang dibutuhkan

Diurutkan dari yang membuka paling banyak.

| Prioritas | Yang dibutuhkan | Pemilik | Membuka |
|---:|---|---|---|
| 1 | Konfirmasi dua serah terima per tagihan, dan perluasan `BilArHandoff` | Billing | `S1`, dan lewat itu `S7` |
| 2 | Tautan kunjungan ke pegawai beserta hubungan keluarganya (`FIN-OQ-096`) | Registrasi + HR | `S1` |
| 3 | Sumber sisa plafon dan plafon mana yang berlaku (`FIN-OQ-087`) | Billing + HR | `S1` |
| 4 | Kesanggupan HR menerima jadwal dan mengirim hasil potongan (`FIN-OQ-091`) | HR/Payroll | `S3` |
| 5 | Cara menutup piutang "RS Benefit" (`FIN-OQ-101`) | Finance + Accounting | `S7` |
| 6 | Batas potongan per periode (`FIN-OQ-092`) | Finance + HR | `S2b` |
| 7 | Gerbang berhenti kerja (`FIN-OQ-095`) | HR | `S4b` |
| 8 | Alur koreksi dan pengembalian potongan (`FIN-OQ-097`) | Registrasi + Billing + HR | `S5` |
| 9 | Kunci pegawai pada templat migrasi (`FIN-OQ-098`) | HR | `S6` |
| 10 | Keberadaan dan jenis master "RS Benefit" (`FIN-OQ-102`) | Administrator | `S7` |

Dua di antaranya milik pemilik Finance sendiri dan **tidak** perlu menunggu modul lain: `FIN-OQ-101`
(bersama Accounting) dan bagian Finance pada `FIN-OQ-092`.

---

## 10. Handoff

| Field | Nilai |
|---|---|
| Modul dan kemampuan | `FIN-BP-001` Finance Management — `EPIC FIN-04`, slice `S2a`, `S4a`, `S8` |
| Snapshot bukti | Backend `46fa2a91`, frontend `0ed37b5c4`, 5 Oktober 2026; capability map bagian 21 |
| Klasifikasi bukti | 14 `CONFIRMED`, 3 `PROPOSED`, 10 `MISSING`, 1 `CONFLICT` (sudah diselesaikan) |
| Baseline rujukan | Tidak ada. `indonesia-hospital-domain-reference` tidak dipakai, alasannya pada bagian 2 |
| Decision ID yang masih terbuka | `FIN-OQ-085`, `087`, `091`, `092`, `094`, `095`, `096`, `097`, `098`, `099`, `101`, `102` |
| Dependency | `S2a` dan `S4a` bergantung pada `S1` untuk kegunaan di produksi, bukan untuk desainnya |
| Kesiapan | `PARTIALLY_READY` |
| Tahap berikutnya | **`design-business-module`** untuk `S2a`, `S4a`, `S8`. `hospital-domain-architect` **tidak dibutuhkan** untuk ketiganya karena seluruhnya di dalam batas Finance; skill itu ditawarkan nanti untuk `S1` dan `S7` yang melintasi Billing, Registrasi, HR, dan Accounting |
| Keluaran hilir yang diharapkan | Entity perjanjian dan jadwal cicilan, cara menghitung status bebas tanggungan, dan pernyataan pemakaian ulang jalur penghapusan buku — ketiganya berstatus `draft` sampai pemilik menyetujui |

---

## 11. Addendum — pembaruan kesiapan sesudah `FIN-DEC-178` dan `FIN-DEC-179`, 5 Oktober 2026

Bagian 1 sampai 10 di atas **dibiarkan apa adanya** sebagai penilaian pada saat gerbang dijalankan.
Addendum ini mencatat perubahannya sesudah pemilik Finance menutup dua butir pemblokir pada hari yang
sama, lewat `grill-me` lanjutan.

### 11.1 Dua butir yang ditutup

| Decision ID | Status baru | Keputusan yang menutupnya |
|---|---|---|
| `FIN-OQ-101` | **CLOSED** sisi Finance | `FIN-DEC-178` — piutang "RS Benefit" ditutup berkala lewat pelunasan internal non-kas, lawan jurnalnya beban manfaat karyawan. **Dilarang** ditutup lewat penghapusan buku, dan **dilarang** dibiarkan terbuka |
| `FIN-OQ-092` | **SEBAGIAN TERJAWAB** — bagian Finance CLOSED | `FIN-DEC-179` — HR yang menegakkan batas potongan; Finance mengirim jadwal dan menerima hasil apa adanya, dan **tidak** menyimpan batas |

### 11.2 Satu butir baru yang terbuka

| Decision ID | Pertanyaan | Pemilik | Dampak |
|---|---|---|---|
| `FIN-OQ-103` | Jenis kejadian dan jurnal beban manfaat karyawan untuk pelunasan internal piutang "RS Benefit" | Accounting, bersama Finance | **`BLOCKING`** untuk kontrak jurnalnya saja; pencatatan dan pelunasan internal di dalam Finance tetap dapat dirancang |

### 11.3 Dua keputusan lama yang diperhalus

`FIN-DEC-179` membawa akibat yang **MUST** dicatat: hasil potongan gaji kini punya **tiga** keadaan —
penuh, **sebagian**, dan gagal — sedangkan `FIN-DEC-167` dan `FIN-DEC-168` baru menyebut berhasil atau
gagal. Keduanya **dianotasi di decision log, tidak dicabut**.

> **Contoh.** Cicilan Rp 1.000.000 gagal dua bulan berturut-turut, sehingga bulan ketiga terutang
> Rp 3.000.000. HR memotong sebesar yang diizinkan, misalnya Rp 1.500.000, dan melaporkannya sebagai
> potongan **sebagian**. Piutang turun Rp 1.500.000, dan sisa Rp 1.500.000 tetap tertunggak lalu ikut
> periode berikutnya.

Akibatnya bagi desain: mekanisme penerima hasil potongan **MUST** menerima nominal yang benar-benar
terpotong, bukan hanya penanda berhasil atau gagal.

### 11.4 Kesiapan per slice — keadaan terbaru

Dua slice naik menjadi siap, dan `S7` dipecah dua karena bagian Finance-nya terpisah bersih dari
kontrak Accounting-nya.

| Slice | Kemampuan | Kesiapan semula | **Kesiapan sekarang** | Blocker yang tersisa |
|---|---|---|---|---|
| `S2a` | Perjanjian dan jadwal cicilan | `READY` | **`READY_FOR_DOMAIN_DESIGN`** | — |
| `S2b` | Penumpukan tunggakan dan batas potongan | `BUSINESS_DECISION_REQUIRED` | **`READY_FOR_DOMAIN_DESIGN`** (sisi Finance) | Nilai batas di HR — milik HR, di luar desain Finance |
| `S4a` | Menghitung status bebas tanggungan | `READY` | **`READY_FOR_DOMAIN_DESIGN`** | — |
| `S7a` | Mencatat porsi benefit dan menutupnya berkala di Finance | bagian dari `S7` yang terblokir | **`READY_FOR_DOMAIN_DESIGN`** | `FIN-OQ-102` memblokir **implementasi**, bukan desain — lihat 11.5 |
| `S8` | Penghapusan buku dan penyesuaian | `READY` (nol desain baru) | **`READY_FOR_DOMAIN_DESIGN`** | — |
| `S1` | Mencatat piutang porsi pegawai | `BUSINESS_DECISION_REQUIRED` | `BUSINESS_DECISION_REQUIRED` | `FIN-OQ-096`, `FIN-OQ-087`, konfirmasi Billing |
| `S3` | Pelunasan lewat potongan gaji | `BUSINESS_DECISION_REQUIRED` | `BUSINESS_DECISION_REQUIRED` | `FIN-OQ-091` |
| `S4b` | Gerbang berhenti kerja di HR | `BUSINESS_DECISION_REQUIRED` | `BUSINESS_DECISION_REQUIRED` | `FIN-OQ-095` |
| `S5` | Koreksi pemilik manfaat | `BUSINESS_DECISION_REQUIRED` | `BUSINESS_DECISION_REQUIRED` | `FIN-OQ-097`, `FIN-CAP-078` `Unknown` |
| `S6` | Batch migrasi saldo lama | `BUSINESS_DECISION_REQUIRED` | `BUSINESS_DECISION_REQUIRED` | `FIN-OQ-098` |
| `S7b` | Kontrak kejadian dan jurnal ke Accounting | bagian dari `S7` yang terblokir | `BUSINESS_DECISION_REQUIRED` | `FIN-OQ-103` |

**Kesiapan keseluruhan tetap `PARTIALLY_READY`**, tetapi komposisinya berubah: **lima** slice siap
dirancang (semula tiga), **enam** berhenti (semula tujuh).

### 11.5 Mengapa `FIN-OQ-102` tidak memblokir desain

`FIN-OQ-102` menanyakan apakah master "RS Benefit" dicatat sebagai penjamin asuransi atau penjamin
perusahaan. Itu **tidak** mengubah desain sisi Finance, karena identitas debitur pada piutang berupa satu
kolom `Guid?` yang memang sudah boleh menunjuk **salah satu dari dua** master itu, dan Finance sudah
memetakan nilainya ke kedua master saat menampilkan nama penjamin (`FIN-CAP-069`, `FIN-CAP-085`, dan
pekerjaan `BE-FIN-FIX-002`). Jadi butir itu memblokir **implementasi** — barisnya harus benar-benar ada
dan Id-nya diketahui — bukan bentuk desainnya.

Penilaian semula menandainya `BLOCKING` tanpa membedakan desain dan implementasi. Addendum ini
**mengoreksi** klasifikasi itu.

### 11.6 Yang boleh berjalan dan yang harus berhenti — keadaan terbaru

**Boleh berjalan sekarang:** `design-business-module` untuk `S2a`, `S2b`, `S4a`, `S7a`, dan `S8`.

**Harus berhenti:** desain `S1`, `S3`, `S4b`, `S5`, `S6`, dan `S7b`; serta **implementasi apa pun**,
termasuk untuk kelima slice yang siap.

**Dependency yang tetap berlaku apa adanya:** kelima slice siap itu **hanya siap dirancang, bukan siap
dirilis**. Tanpa `S1` tidak ada piutang karyawan yang bisa dilekati perjanjian, dihitung status bebas
tanggungannya, maupun dipisahkan porsi benefitnya. `EPIC FIN-04` tetap `OPEN DECISION` dan tetap di luar
seluruh gelombang pengiriman.

### 11.7 Handoff yang diperbarui

| Field | Nilai |
|---|---|
| Kemampuan yang dikirim | `S2a`, `S2b`, `S4a`, `S7a`, `S8` |
| Decision ID yang masih terbuka | `FIN-OQ-085`, `087`, `091`, `092` (bagian HR), `094`, `095`, `096`, `097`, `098`, `099`, `102`, `103` |
| Kesiapan | `PARTIALLY_READY` — lima siap, enam berhenti |
| Tahap berikutnya | `design-business-module` untuk kelima slice itu. `hospital-domain-architect` tetap **tidak dibutuhkan**: kelimanya di dalam batas Finance. Satu-satunya titik sentuh pada `S7a` adalah jurnal ke Accounting, dan itu sengaja dipisah menjadi `S7b` |
| Keluaran hilir yang diharapkan | Entity perjanjian beserta jadwal cicilannya; aturan penumpukan tunggakan dan penerimaan potongan sebagian; cara menghitung status bebas tanggungan; pencatatan porsi benefit beserta mekanisme pelunasan internal non-kas; dan pernyataan pemakaian ulang jalur penghapusan buku — seluruhnya `draft` sampai pemilik menyetujui |
