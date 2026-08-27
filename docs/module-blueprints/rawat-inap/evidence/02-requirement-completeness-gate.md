# Rawat Inap — Requirement Completeness Gate

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| Assessment revision | `1.0` |
| Assessment date | 21 Agustus 2026 (`Asia/Jakarta`) |
| Assessment status | `CURRENT` |
| **Overall readiness** | **`PARTIALLY_READY`** |
| Ready destination | `hospital-domain-architect`, hanya untuk slice yang ditandai siap pada bagian 7 |
| Business evidence | [`00-interview-decisions.md`](../00-interview-decisions.md) revision `2`, SHA-256 `e210cd40ee9a0207a0e2df7e00ac055e9029ae42aface5dedb74e4e9ae2c7b6a` |
| Capability evidence | [`01-existing-capability-map.md`](../01-existing-capability-map.md) revision `1.2`, SHA-256 `567d7f7ea57537f419efca28d551e965524d27ea1889a00cc7707d17ec74c3b6` |
| Primary business source | `docs/Modul-RS/PRD-Modul-Rawat-Inap.md`, status `TARGET PROPOSAL`, baseline commit `5103e68` |
| Baseline rujukan | `indonesia-hospital-domain-reference`, berkas `references/inpatient.md`, `Reference coverage: PARTIAL`, seluruh observasi berstatus `REFERENCE_ONLY` |
| Backend snapshot | `5afb54bd75281648010e50ef14f43ca1f80d8efd` (branch `MHamzah`) |
| Frontend snapshot | `dec4fdeff07c3c96ad9f07f41f184c54cf771371` (branch `HamzahV2`) |
| Write boundary | Hanya dokumen ini. Tidak ada source aplikasi, migration, entity, endpoint, UI, atau ClickUp yang diubah |

> **Apa gunanya dokumen ini.** Dokumen ini tidak merancang apa pun. Tugasnya satu: memeriksa
> apakah kebutuhan bisnis Rawat Inap sudah cukup lengkap dan cukup berbukti untuk mulai dirancang
> arsitektur domainnya. Hasilnya berupa daftar bagian mana yang boleh maju dan bagian mana yang
> harus berhenti dulu, beserta alasannya.
>
> Dokumen ini juga **tidak menjawab** keputusan bisnis yang belum diputuskan pemiliknya. Butir
> semacam itu dicatat sebagai Decision ID lalu dikembalikan ke `/grill-me`.

---

## 1. Scope penilaian

### 1.1 Modul dan menu

| Hal | Nilai |
| --- | --- |
| Area | `HEALTH_SERVICES` |
| Modul | `InPatientManagement` / Rawat Inap, prefix `Inp`, lifecycle registry `PLANNED` |
| Batas scope bisnis | Satu episode perawatan pasien menginap, dari pasien diterima masuk sampai episode ditutup dan tempat tidur kembali kosong, sesuai `RWI-DEC-004` |

### 1.2 Lima belas slice yang dinilai

Penilaian dilakukan **per slice**, bukan per modul. Ini penting: satu slice yang terhambat tidak
boleh menghentikan slice lain yang sebenarnya sudah siap.

| Slice ID | Nama slice | Kemampuan PRD yang dicakup | Aturan bisnis utama |
| --- | --- | --- | --- |
| `INP-S01` | Admisi dan pemesanan tempat tidur | CAP-002, CAP-003, CAP-004, CAP-005, CAP-006 | `RWI-RULE-001` s.d. `005`, `013`, `015`, `022` |
| `INP-S02` | Penempatan tempat tidur, census, dan lama dirawat | CAP-006, CAP-008 | `RWI-RULE-019`, `RWI-RULE-027` |
| `INP-S03` | Perpindahan pasien dan pindah kelas | CAP-017 | `RWI-RULE-006`, `007`, `008`, `016`, `030` |
| `INP-S04` | Penugasan perawat penanggung jawab | CAP-011 | `RWI-RULE-033` |
| `INP-S05` | Dokumentasi klinis rawat inap dan visite | CAP-012, CAP-014, CAP-020, CAP-021, CAP-022, CAP-024, CAP-025 | `RWI-RULE-017`, `021`, `026` |
| `INP-S06` | Resep rawat inap dan obat pulang | CAP-023 | `RWI-RULE-024`, `RWI-RULE-026` |
| `INP-S07` | Keputusan pulang, cara pulang, dan resume pulang | CAP-026 | `RWI-RULE-011`, `RWI-RULE-032` |
| `INP-S08` | Daftar periksa administrasi, kelayakan keuangan, dan penutupan episode | CAP-028 | `RWI-RULE-009`, `010`, `018`, `020`, `028` |
| `INP-S09` | Serah terima IGD ke rawat inap | Titik sentuh IGD | `RWI-RULE-029` |
| `INP-S10` | Persetujuan umum rawat inap | Bagian CAP-009 yang tidak ditunda | `RWI-RULE-025` |
| `INP-S11` | Penempatan menurut jenis kelamin dan isolasi | Bagian CAP-005 dan CAP-006 | `RWI-RULE-012` |
| `INP-S12` | Bayi baru lahir dan boks bayi | Bagian CAP-002 dan CAP-006 | `RWI-RULE-014` |
| `INP-S13` | Riwayat status, audit, dan daftar pantau kepatuhan | NFR-003 | `RWI-RULE-023`, `RWI-RULE-031` |
| `INP-S14` | Pengaturan yang dapat diubah admin | Pendukung | `RWI-RULE-034` |
| `INP-S15` | Interoperabilitas SATUSEHAT dan pelaporan | **Belum ada di daftar kemampuan** | **Belum ada aturannya** |

`INP-S15` **tidak** berasal dari dokumen keputusan. Slice ini muncul dari pembandingan dengan
baseline rumah sakit Indonesia, dan penjelasannya ada di bagian 4.11.

### 1.3 Yang sengaja tidak dinilai

Kemampuan berikut sudah dinyatakan di luar scope oleh `RWI-DEC-004`, sehingga tidak dinilai
kelengkapannya di sini: daftar tunggu masuk, cetak kartu dan gelang, paket dokumen serah terima
lengkap, deposit dan estimasi biaya, rencana asuhan keperawatan SDKI, pemeriksaan penunjang
ujung-ke-ujung, pencatatan pemakaian alat, booking operasi, tagihan berjalan, asuhan gizi,
mesin farmasi, buku besar keuangan, pemberian obat di samping tempat tidur, dan pasien titipan.

---

## 2. Bukti yang dipakai dan wewenangnya

### 2.1 Urutan wewenang yang dipakai

| Urutan | Jenis bukti | Tersedia untuk modul ini | Keterangan |
| ---: | --- | --- | --- |
| 1 | Requirement eksplisit terkini dari rumah sakit/user | **Sebagian** | Berupa jawaban wawancara pada `00-interview-decisions.md`, diberikan pemegang sementara, bukan oleh rumah sakit secara kelembagaan |
| 2 | SOP atau kebijakan rumah sakit yang disahkan | **Tidak ada** | Tidak ada satu pun SOP yang dilampirkan atau dirujuk |
| 3 | Keputusan rapat yang dikonfirmasi | **Tidak ada** | Tidak ada notulen yang dirujuk |
| 4 | Bukti bisnis analis atau ClickUp yang disetujui | **Sebagian** | PRD Modul Rawat Inap berstatus `TARGET PROPOSAL`, artinya usulan, bukan kebijakan yang disahkan |
| 5 | Baseline rumah sakit Indonesia | **Ada** | `references/inpatient.md`, `Reference coverage: PARTIAL`, seluruhnya `REFERENCE_ONLY` |
| 6 | Bukti implementasi Quilvian V2 | **Ada dan kuat** | `01-existing-capability-map.md` revision `1.2` |
| 7 | Bukti legacy Quilvian V1 | **Tidak dipakai** | Tidak ada lampiran legacy untuk modul ini |

### 2.2 Catatan penting tentang wewenang bukti

Ini yang paling menentukan hasil penilaian, dan harus dibaca sebelum tabel mana pun:

**Seluruh 44 keputusan berstatus `approved` pada dokumen keputusan disetujui oleh "pemegang
sementara" yang namanya belum diisi.** Ini tercatat sendiri di dalam dokumen itu sebagai
`RWI-DEC-006`, `RWI-DEC-037`, dan `RWI-OQ-023`. Ada juga dua pendelegasian menyeluruh,
`RWI-DEC-036` dan `RWI-DEC-044`, ketika pemilik kebutuhan meminta agent mengambil opsi yang
direkomendasikan tanpa menimbang satu per satu.

Akibatnya bagi gerbang ini: keputusan-keputusan itu **cukup untuk merancang arsitektur domain**,
karena arahnya jelas dan konsisten. Tetapi keputusan itu **tidak cukup untuk dipakai melayani
pasien sungguhan**, karena wewenang kelembagaannya belum ada. Perbedaan ini dipertahankan di
seluruh dokumen: gerbang ini menilai kesiapan untuk **domain design**, bukan kesiapan untuk
**produksi**.

Ketiadaan SOP yang disahkan tidak dipakai sebagai alasan memblokir, karena bukti tingkat 1 dan 4
sudah menjawab sebagian besar pertanyaan. Tetapi ketiadaan itu dicatat sebagai keterbatasan pada
bagian 9.

---

## 3. Ringkasan hasil

| Hal | Jumlah |
| --- | ---: |
| Slice yang dinilai | 15 |
| Slice `READY_FOR_DOMAIN_DESIGN` | 8 |
| Slice `PARTIALLY_READY` | 2 |
| Slice `BUSINESS_DECISION_REQUIRED` | 5 |
| Dimensi kelengkapan yang dinilai | 18 |
| Dimensi berstatus cukup lengkap | 13 |
| Dimensi dengan gap material | 5 |
| Decision ID pemblokir | 7 |
| Butir `PROPOSED` atau `MISSING` yang tidak memblokir | 11 |
| Butir `CONFLICT` | 1 |

**Kalimat pendeknya:** tulang punggung Rawat Inap — admisi, tempat tidur, perpindahan, census,
penutupan, audit — sudah cukup lengkap untuk dirancang. Yang menahan adalah lima slice yang
bergantung pada persetujuan pihak lain atau pada keputusan klinis, privasi, dan interoperabilitas
yang belum pernah dibahas.

---

## 4. Temuan kelengkapan pada 18 dimensi

### 4.1 Dimensi 01 — Tujuan

**Status: `CONFIRMED`.**

Hasil bisnis yang dituju dinyatakan satu kalimat pada batas scope, dan diperinci menjadi 18
kemampuan MUST. Kalimat batasnya: mengelola satu episode perawatan pasien menginap, dari pasien
diterima masuk sampai episode ditutup dan tempat tidur kembali kosong.

Bukti: `00-interview-decisions.md` bagian Scope dan Outcome; `RWI-DEC-004`.

### 4.2 Dimensi 02 — Aktor

**Status: `CONFIRMED`.**

| Aktor | Perannya | Bukti |
| --- | --- | --- |
| Petugas admisi | Membuka admisi, memesan bed, menempatkan pasien, menandai daftar periksa, menutup episode | `RWI-RULE-004`, `010`, `018` |
| DPJP | Memutuskan pasien boleh pulang, meminta perpindahan, menandatangani resume | `RWI-RULE-010`, `016`, `030`, `032` |
| Kepala ruangan | Menugaskan perawat, memindahkan pasien, menindaklanjuti daftar pantau kepatuhan | `RWI-RULE-006`, `023`, `033` |
| Perawat pelaksana | Menulis pengkajian dan catatan, memindahkan pasien | `RWI-RULE-006`, `021` |
| Supervisor | Membatalkan admisi setelah `Admitted`, menutup episode, menembus gerbang keuangan, membuka kembali episode | `RWI-RULE-004`, `009`, `010`, `020` |
| Petugas kasir atau billing | Menandai kelayakan keuangan | `RWI-RULE-028` |
| Admin master data | Mengatur parameter, mengisi master, menyetel keadaan bed non-pasien | `RWI-RULE-027`, `034` |

Baseline `ID-INP-CAP-006` mengingatkan agar wewenang profesional tidak disimpulkan dari nama
jabatan. Di sini wewenang memang ditulis eksplisit per tindakan, bukan disimpulkan.

### 4.3 Dimensi 03 — Pemicu dan prasyarat

**Status: `CONFIRMED` untuk dua dari tiga jalur masuk.**

| Jalur masuk | Pemicu | Status |
| --- | --- | --- |
| Pasien datang langsung | Petugas admisi membuka admisi | `CONFIRMED` — `RWI-DEC-011` |
| Pasien dari poliklinik | Kunjungan poliklinik yang sudah ada dipakai | `CONFIRMED` — `RWI-DEC-011` |
| Pasien dari IGD | Disposisi `RANAP` dijalankan | `CONFIRMED` arahnya lewat `RWI-DEC-041`, tetapi **terblokir** pada `DEC-INP-002` |

Baseline `ID-INP-CAP-001` menanyakan "keputusan merawat" yang mendahului admission, termasuk
tingkat kegawatan dan prasyarat payer. Tingkat kegawatan tidak dipakai sebagai prasyarat mana pun
pada modul ini, dan prasyarat payer sengaja ditunda bersama CAP-010. Keduanya deferral yang
disadari, bukan lubang.

### 4.4 Dimensi 04 — Alur utama

**Status: `CONFIRMED`.**

Alur utama tertulis urut dan lengkap:

`Admisi → pemesanan bed → penempatan bed → episode Admitted → census → penugasan perawat →
pengkajian awal → dokumentasi harian → resep → perpindahan bila perlu → keputusan pulang →
DischargePending → resume pulang → daftar periksa administrasi → kelayakan keuangan → Closed →
bed kembali Available.`

Setiap langkah punya pelaku, syarat, dan hasil akhir yang tertulis. Contoh berangka juga tersedia
pada hampir setiap aturan.

### 4.5 Dimensi 05 — Alur alternatif dan exception

**Status: `CONFIRMED` untuk sebagian besar, dengan tiga gap.**

Yang sudah tertutup:

| Exception | Aturan |
| --- | --- |
| Pembatalan admisi | `RWI-RULE-004` |
| Pemesanan bed gugur, lalu bed diambil pasien lain | `RWI-RULE-002`, `RWI-RULE-015` |
| Episode `Draft` telantar | `RWI-RULE-022` |
| Perpindahan gagal di tengah jalan | `RWI-RULE-008` |
| Lima cara pulang | `RWI-RULE-011` |
| Penutupan tanpa kelayakan keuangan | `RWI-RULE-009` |
| Pembukaan kembali episode | `RWI-RULE-020` |
| Serah terima IGD gagal | `RWI-RULE-029` aturan 5 |

Yang belum tertutup, ketiganya diangkat baseline pasal 10:

1. **Kepergian fisik pasien terpisah dari penutupan administratif.** Baseline pasal 9 secara tegas
   memisahkan "kepergian pasien", "pembebasan tempat tidur", dan "penyelesaian encounter" sebagai
   tiga kejadian yang belum tentu bersamaan. Dokumen keputusan menggabungkan pembebasan tempat
   tidur ke dalam penutupan episode, sehingga tempat tidur tetap terbaca terisi selama pasien
   sudah pulang tetapi episodenya belum ditutup. Baris daftar pantau "penutupan tertunda" dengan
   ambang 4 jam justru membuktikan jeda itu memang diperkirakan terjadi. Klasifikasi:
   `PROPOSED` / `NON_BLOCKING_STANDARD`, lihat bagian 5.
2. **Episode rawat inap aktif ganda untuk satu pasien.** Tidak ada aturan yang melarang satu
   pasien punya dua episode aktif sekaligus. Klasifikasi: `MISSING` / `NON_BLOCKING_STANDARD`.
3. **Perpindahan yang dicatat sebelum pasien benar-benar berpindah.** Sistem hanya mengenal satu
   waktu perpindahan. Klasifikasi: `MISSING` / `NON_BLOCKING_STANDARD`.

### 4.6 Dimensi 06 — Data minimum

**Status: `CONFIRMED` untuk slice yang siap; `MISSING` untuk `INP-S15`.**

Data minimum untuk admisi, pemesanan, penempatan, perpindahan, penugasan, resume, dan penutupan
sudah disebut satu per satu di dalam aturannya masing-masing, termasuk kolom wajib dan alasan
wajib. Contoh: `RWI-RULE-030` menyebut catatan DPJP wajib memuat dokter, masa berlaku, pengalih,
dan alasan.

Yang belum: data minimum untuk pengiriman interoperabilitas, karena topik itu memang belum pernah
dibahas. Lihat bagian 4.11.

### 4.7 Dimensi 07 — Aturan bisnis dan validation

**Status: `CONFIRMED`.**

34 aturan bisnis tertulis, seluruhnya disertai contoh berangka, dan diturunkan menjadi 115
acceptance criteria yang dapat diuji. Ini jauh di atas kelengkapan minimum yang dituntut gerbang
ini.

### 4.8 Dimensi 08 — Status dan perubahan status

**Status: `CONFIRMED`.**

Model status episode dikunci lima nilai: `Draft`, `Admitted`, `DischargePending`, `Closed`,
`Cancelled`, dengan tabel perpindahan yang menyebut siapa boleh memicu dan syaratnya
(`RWI-RULE-003`). Status tempat tidur memakai enum yang sudah ada di source. Perpindahan status
wajib lewat satu pintu dan meninggalkan riwayat (`RWI-RULE-031`).

Baseline pasal 5 mengingatkan bahwa status klinis, okupansi tempat tidur, administratif,
finansial, dan interoperabilitas dapat berjalan mandiri dan perlu direkonsiliasi. Dokumen
keputusan memang memisahkan status episode dari status tempat tidur dan dari status kelayakan
keuangan. Yang belum dipisahkan adalah status interoperabilitas, karena `INP-S15` belum ada.

### 4.9 Dimensi 09 — Peran dan authorization

**Status: `CONFIRMED`.**

Setiap tindakan material punya peran yang berwenang, dan yang paling penting: kewenangan per
pasien sudah dipikirkan, bukan hanya kewenangan per peran. `RWI-RULE-030` menetapkan hanya DPJP
aktif episode itu yang boleh meminta perpindahan, dan menyadari bahwa mesin hak akses yang ada
tidak dapat menegakkannya sehingga penjaganya ditulis di dalam service.

### 4.10 Dimensi 10 — Dependency antarmodul

**Status: `CONFIRMED` isinya, tetapi persetujuannya belum ada.**

| Modul tetangga | Yang dibutuhkan | Status persetujuan |
| --- | --- | --- |
| `RegistrationManagement` | Kunjungan sebagai jangkar episode | Belum diminta secara eksplisit |
| `ClinicalManagement` | Pelonggaran keharusan antrean dan konsultasi | **Belum ada** — `DEC-INP-001` |
| `PharmacyManagement` | Pelonggaran resep dan penanda obat pulang | **Belum ada** — `DEC-INP-001` |
| `MasterData` | Pembatasan endpoint ketersediaan tempat tidur | **Belum ada** — tidak memblokir, lihat bagian 5 |
| `EmergencyInstallationManagement` | Serah terima disposisi `RANAP` | **Belum ada** — `DEC-INP-002` |
| `BillingManagement` | Status kelayakan keuangan | Tidak dibutuhkan pada MVP, diganti penandaan manual `RWI-RULE-028` |

Baseline pasal 11 juga menyebut Notification, Medical Record, Credentialing, Nutrition, dan
Rehabilitation. Empat yang terakhir sudah dinyatakan di luar scope. Notification belum pernah
dibahas, lihat dimensi 15.

### 4.11 Dimensi 11 — Integrasi internal dan eksternal

**Status: `MISSING`. Ini gap terbesar yang ditemukan gerbang ini.**

Integrasi internal sudah jelas: Farmasi menerima resep dengan konteks encounter dan status
penyerahannya dibaca balik; Billing diganti penandaan manual sementara; IGD lewat disposisi.

Integrasi eksternal **tidak dibahas sama sekali**. Kata "SATUSEHAT" tidak muncul satu kali pun di
dalam 2.163 baris dokumen keputusan. Padahal PRD sendiri menyebutnya:

> "Playbook SATUSEHAT Rawat Inap mendefinisikan satu rangkaian rawat inap sebagai `Encounter`,
> termasuk timeline lokasi, diagnosis, observation, procedure dan discharge-related data.
> Dokumentasi juga menunjukkan perubahan lokasi/bed perlu direpresentasikan sebagai histori
> location dalam encounter."
>
> — `docs/Modul-RS/PRD-Modul-Rawat-Inap.md` baris 814

Baseline juga menandai topik ini dengan lima observasi terpisah — `ID-INP-INT-001` sampai
`ID-INP-INT-005` — seluruhnya dengan `integration_relevance: HIGH`, `audit_relevance: HIGH`, dan
`billing_relevance: HIGH`.

**Kenapa ini penting dan bukan sekadar pekerjaan susulan.** Baris PRD di atas menyebut riwayat
lokasi harus terwakili **di dalam encounter**. Sementara `RWI-DEC-039` menempatkan riwayat lokasi
pada catatan penempatan milik Rawat Inap, dan capability map membuktikan kunjungan hari ini hanya
punya satu kolom `RoomId` tanpa riwayat. Keduanya bisa saja tetap sejalan bila catatan penempatan
dipakai sebagai sumber yang dibaca saat pengiriman. Tetapi itu **belum diputuskan**, dan bila
jawabannya ternyata "riwayat lokasi harus tersimpan pada kunjungan", maka pemilik datanya berpindah
dari Rawat Inap ke Registrasi. Perpindahan pemilik data adalah perubahan yang mahal bila baru
ketahuan setelah desain jadi.

Klasifikasi: `MISSING` / `BLOCKING`, dicatat sebagai `DEC-INP-005`. Memblokir `INP-S15`, dan
**tidak** memblokir `INP-S01` maupun `INP-S02` dengan syarat catatan penempatan dirancang sebagai
sumber yang dapat dibaca ulang, bukan sekadar penanda keadaan terakhir.

### 4.12 Dimensi 12 — Hasil akhir

**Status: `CONFIRMED`.**

Hasil akhir yang dapat diamati: episode berstatus `Closed`, tempat tidur kembali `Available`,
resume pulang tertandatangani, daftar periksa administrasi tertutup, dan riwayat status lengkap.
Seluruhnya punya acceptance criteria.

### 4.13 Dimensi 13 — Pembatalan dan koreksi

**Status: `CONFIRMED`.**

Pembatalan admisi, pembatalan pemesanan, pembatalan perpindahan, dan pembukaan kembali episode
semuanya punya aturan beserta wewenangnya. `RWI-RULE-020` bahkan tegas bahwa reopen hanya untuk
membetulkan catatan, tidak mengembalikan tempat tidur, dan tidak menambah lama dirawat.

Satu hal yang belum ada: koreksi resume pulang setelah episode ditutup hanya bisa lewat reopen,
dan tidak ada versi resume yang tersimpan. Baseline `ID-INP-CAP-019` menanyakan riwayat versi
resume. Klasifikasi: `MISSING` / `NON_BLOCKING_STANDARD`.

### 4.14 Dimensi 14 — Audit dan histori

**Status: `CONFIRMED`.**

`RWI-RULE-031` menetapkan tabel riwayat status tersendiri yang ditulis dalam transaksi yang sama,
lewat satu pintu, tidak dapat diubah, dan mencatat pelaku, waktu, alasan, serta perubahan yang
dilakukan sistem secara terpisah dari yang dilakukan orang. `RWI-RULE-030` dan `RWI-RULE-033`
menambahkan riwayat DPJP dan riwayat perawat.

Yang belum: masa simpan riwayat sebelum boleh diarsipkan (`RWI-OQ-035`). Klasifikasi: `MISSING` /
`NON_BLOCKING_STANDARD`, karena bentuk tabelnya tidak berubah oleh keputusan itu.

### 4.15 Dimensi 15 — Notifikasi

**Status: `MISSING`, tidak material untuk MVP.**

Modul ini memakai pendekatan tarik, bukan dorong: tiga daftar pantau pada `RWI-RULE-023` yang
dibuka sendiri oleh penanggung jawabnya. Tidak ada notifikasi yang dikirim ke siapa pun.

Ini masuk akal untuk MVP dan tidak menimbulkan risiko keselamatan langsung, karena tidak ada
aturan yang menuntut seseorang bertindak dalam hitungan menit. Tetapi keputusan "tidak ada
notifikasi" itu **tidak pernah dinyatakan**; ia hanya tidak dibahas. Klasifikasi: `PROPOSED` /
`NON_BLOCKING_STANDARD` — usulkan menyatakannya eksplisit supaya pembaca berikutnya tahu itu
pilihan sadar, bukan kelupaan.

### 4.16 Dimensi 16 — Dampak billing dan charge

**Status: `CONFIRMED` sebagai deferral yang disadari, dengan satu catatan.**

Yang sudah diputuskan: kelas yang ditagihkan selalu mengikuti kamar yang ditempati
(`RWI-RULE-007`); perubahan kelas disimpan sebagai riwayat; pasien titipan dikeluarkan dari MVP
sehingga tidak ada kelas hak yang terpisah; kelayakan keuangan memblokir penutupan dan ditandai
manual sementara (`RWI-RULE-028`).

Yang ditunda dengan alasan yang jelas: tagihan berjalan, deposit, estimasi biaya, cek manfaat
penjamin, dan klaim, semuanya menunggu `BillingManagement` operasional.

Catatan yang perlu diketahui pemilik: baseline pasal 12 menyebut charge kamar per hari sebagai
kepedulian utama rawat inap, dan `MstPatientClass` di source sudah punya kolom
`DefaultDailyRoomRate`. Karena tagihan berjalan ditunda, **tidak ada satu pun charge kamar yang
tercatat selama MVP**. Konsekuensinya: data lama dirawat dan riwayat kelas yang dihasilkan MVP
harus cukup untuk merekonstruksi charge kamar di kemudian hari. `RWI-RULE-007` dan `RWI-RULE-019`
sudah menyediakan keduanya, jadi rekonstruksi itu mungkin dilakukan. Klasifikasi: `CONFIRMED`
dengan catatan, tidak memblokir.

### 4.17 Dimensi 17 — Dampak keselamatan klinis

**Status: `CONFLICT` pada satu butir, `MISSING` pada satu butir lain.**

**Butir `CONFLICT` — isolasi dan pemisahan jenis kelamin.** `RWI-DEC-018` memilih keduanya tetap
berupa penyaring pencarian, bukan aturan yang menolak penempatan. Artinya sistem mengizinkan
pasien yang butuh isolasi ditempatkan di kamar biasa berisi pasien lain, dan mengizinkan pasien
laki-laki dan perempuan sekamar. Dokumen keputusan sendiri menandai ini sebagai gerbang keras dan
menolak menaikkannya ke `approved`.

Pertentangannya: `RWI-FACT-009` menunjukkan PRD memang menulisnya sebagai penyaring opsional,
sementara baseline `ID-INP-CAP-003` dan pasal 13 memperlakukan kebutuhan isolasi sebagai kendala
penempatan, bukan preferensi pencarian. Keduanya sumber yang berbeda wewenangnya, dan pertentangan
ini tidak dapat diselesaikan tanpa pemilik klinis. Klasifikasi: `CONFLICT` / `BLOCKING`, dicatat
sebagai `DEC-INP-004`.

**Butir `MISSING` — serah terima klinis antar shift.** Baseline `ID-INP-CAP-016` menandai serah
terima tim perawatan sebagai `SAFETY_CHECK`: siapa menyerahkan, siapa menerima, apa isinya, apakah
penerimaan dikonfirmasi, dan tugas apa yang belum tuntas. Dokumen keputusan hanya mengenal serah
terima IGD ke rawat inap, dan sama sekali tidak membahas pergantian jaga perawat, padahal pasien
menginap berhari-hari dan berganti perawat berkali-kali. `RWI-RULE-033` mencatat siapa perawat
penanggung jawab, tetapi tidak mencatat apa yang diserahkan saat berganti. Klasifikasi: `MISSING`
/ `BLOCKING` untuk slice serah terima klinis, dicatat sebagai `DEC-INP-006`. Tidak memblokir
`INP-S04`, karena penugasan perawat tetap dapat dirancang tanpa isi serah terima.

Butir keselamatan lain yang **sudah** tertutup: kepastian identitas pasien lewat kunjungan,
informasi alergi tersedia dan bebas antrean di source, tanggung jawab klinis lewat `RWI-RULE-030`,
dan keselamatan saat perpindahan lewat `RWI-RULE-008` yang mensyaratkan pasien tidak pernah
tercatat tanpa tempat tidur.

### 4.18 Dimensi 18 — Pelaporan dan traceability

**Status: `MISSING` sebagian.**

Yang sudah ada: laporan penutupan tanpa kelayakan keuangan, tiga daftar pantau kepatuhan, laporan
selisih tempat tidur, dan riwayat status yang dapat ditelusuri.

Yang belum: pelaporan wajib ke luar rumah sakit. Baseline `ID-INP-REG-001` menyebut informasi
klinis rawat inap menjadi bagian rekam medis elektronik yang diatur regulasi, dan `ID-INP-INT-005`
menyebut pengiriman resume medis. Keduanya belum pernah dibahas. Ini bagian dari `DEC-INP-005`.

---

## 5. Klasifikasi bukti dan dampak gap

### 5.1 Butir `CONFIRMED` yang menopang kesiapan

| Butir | Bukti |
| --- | --- |
| Batas scope dan 18 kemampuan MUST | `RWI-DEC-004`, `RWI-DEC-005` |
| Model status episode dan tabel perpindahannya | `RWI-RULE-003` |
| Pemesanan tempat tidur, batas waktu, dan kedaluwarsa saat dibaca | `RWI-RULE-001`, `RWI-RULE-002` |
| Sumber kebenaran penghunian tempat tidur | `RWI-RULE-027` |
| Perpindahan utuh dan kewenangannya | `RWI-RULE-006`, `008`, `016`, `030` |
| Lima cara pulang | `RWI-RULE-011` |
| Gerbang keuangan dan sumber sementaranya | `RWI-RULE-009`, `RWI-RULE-028` |
| Daftar periksa administrasi yang diatur admin | `RWI-RULE-018` |
| Riwayat status yang tidak dapat diubah | `RWI-RULE-031` |
| Pemakaian ulang tanpa duplikasi entity | `01-existing-capability-map.md` bagian 14.4 |

### 5.2 Butir `PROPOSED`, `MISSING`, dan `CONFLICT` beserta dampaknya

| No | Butir | Status | Dampak | Slice terdampak | Decision ID |
| ---: | --- | --- | --- | --- | --- |
| 1 | Persetujuan pemilik `ClinicalManagement` dan `PharmacyManagement` atas pelonggaran antrean dan konsultasi | `MISSING` | `BLOCKING` | `INP-S05`, `INP-S06` | `DEC-INP-001` |
| 2 | Persetujuan pemilik `EmergencyInstallationManagement` atas serah terima disposisi `RANAP` | `MISSING` | `BLOCKING` | `INP-S09` | `DEC-INP-002` |
| 3 | Persetujuan pemilik privasi dan hukum atas persetujuan umum yang tidak menahan admisi | `PROPOSED` | `BLOCKING` | `INP-S10` | `DEC-INP-003` |
| 4 | Isolasi dan pemisahan jenis kelamin sebagai penyaring, bukan penolak penempatan | `CONFLICT` | `BLOCKING` | `INP-S11` | `DEC-INP-004` |
| 5 | Kepemilikan, isi, dan pemicu pengiriman SATUSEHAT rawat inap | `MISSING` | `BLOCKING` | `INP-S15` | `DEC-INP-005` |
| 6 | Isi dan konfirmasi serah terima klinis antar shift keperawatan | `MISSING` | `BLOCKING` | Slice serah terima klinis, belum masuk daftar kemampuan | `DEC-INP-006` |
| 7 | Aturan klinis pasien meninggal dan pasien kabur | `PROPOSED` | `BLOCKING` | `INP-S07` sebagian | `DEC-INP-007` |
| 8 | Persetujuan pemilik `MasterData` atas pembatasan endpoint ketersediaan bed | `MISSING` | `NON_BLOCKING_STANDARD` | `INP-S02` | — |
| 9 | Kepergian fisik pasien sebagai kejadian tersendiri, terpisah dari penutupan | `PROPOSED` | `NON_BLOCKING_STANDARD` | `INP-S02`, `INP-S08` | — |
| 10 | Larangan dua episode rawat inap aktif untuk satu pasien | `MISSING` | `NON_BLOCKING_STANDARD` | `INP-S01` | — |
| 11 | Perpindahan yang dicatat sebelum pasien benar-benar berpindah | `MISSING` | `NON_BLOCKING_STANDARD` | `INP-S03` | — |
| 12 | Riwayat versi resume pulang | `MISSING` | `NON_BLOCKING_STANDARD` | `INP-S07` | — |
| 13 | Masa simpan riwayat status | `MISSING` | `NON_BLOCKING_STANDARD` | `INP-S13` | `RWI-OQ-035` |
| 14 | Pernyataan eksplisit bahwa modul ini tidak mengirim notifikasi | `PROPOSED` | `NON_BLOCKING_STANDARD` | Seluruh modul | — |
| 15 | Frekuensi observasi keperawatan dan ambang eskalasi | `MISSING` | `CONFIGURABLE_DEFAULT` | `INP-S05` | — |
| 16 | Hasil penunjang yang masih tertunda saat pasien pulang | `MISSING` | `CONFIGURABLE_DEFAULT` | `INP-S08` | — |
| 17 | Instruksi medis dan keperawatan sebagai order yang ditelusuri | `MISSING` | `NON_BLOCKING_STANDARD` | `INP-S05` | — |
| 18 | Obat dan instruksi yang masih aktif setelah pasien pulang | `MISSING` | `NON_BLOCKING_STANDARD` | `INP-S07` | — |
| 19 | Penanggung jawab pengisian data master | `MISSING` | `NON_BLOCKING_STANDARD` | Implementasi | `RWI-OQ-036` |

### 5.3 Kenapa butir nomor 8 sampai 19 tidak memblokir

Alasan singkat masing-masing, supaya keputusan ini dapat diperiksa dan tidak sekadar diterima:

- **Nomor 8.** Yang belum ada hanyalah persetujuan atas pembatasan endpoint milik modul lain.
  Bentuk data Rawat Inap sendiri tidak berubah: catatan penempatan tetap menjadi sumber kebenaran.
  Bila persetujuan tidak didapat, jalan mundurnya jelas — status tempat tidur dihitung dari catatan
  penempatan setiap kali dibaca, bukan disalin. Ini pilihan B yang sudah pernah ditimbang pada
  Closure Pass pertanyaan 2.
- **Nomor 9, 11, 12, 17, 18.** Semuanya menambah kolom atau kejadian baru pada aggregate yang
  pemiliknya sudah jelas. Penambahan semacam itu tidak memindahkan ownership dan tidak mengubah
  makna klinis.
- **Nomor 10.** Berupa invariant yang wajar dan dapat dinyatakan eksplisit tanpa mengubah struktur.
- **Nomor 13.** Masa simpan mengubah kebijakan pengarsipan, bukan bentuk tabelnya.
- **Nomor 14.** Menyatakan ketiadaan notifikasi tidak mengubah apa pun; ia hanya membuat pilihan
  itu terbaca.
- **Nomor 15 dan 16.** Keduanya memang wajar berbeda antar rumah sakit dan antar unit, dan modul
  ini sudah punya dua tempat untuk menampungnya: tabel pengaturan `RWI-RULE-034` dan daftar periksa
  administrasi yang butirnya diatur admin `RWI-RULE-018`.
- **Nomor 19.** Tindakan organisasi pada tahap implementasi, tidak menyentuh desain.

---

## 6. Decision Log

Tujuh Decision ID berikut adalah ambiguitas pemblokir yang bergantung pemilik. Gerbang ini
**tidak menjawabnya**. Penutupannya diarahkan ke `/grill-me`.

### `DEC-INP-001`

| Field | Isi |
| --- | --- |
| Pertanyaan | Apakah pemilik `ClinicalManagement` dan `PharmacyManagement` menyetujui pelonggaran keharusan antrean dan konsultasi, serta pelonggaran batas satu konsultasi per kunjungan dan satu resep aktif per konsultasi, khusus untuk kunjungan bertipe rawat inap? |
| Kemampuan terdampak | `INP-S05` dokumentasi klinis dan visite; `INP-S06` resep dan obat pulang |
| Bukti saat ini | `RWI-DEC-038` dan `RWI-RULE-026` sudah memilih arah pelonggaran. `RWI-FACT-011` dan `RWI-FACT-012` membuktikan pembatasnya nyata di source. Pemilik kedua modul belum tercatat namanya di blueprint mana pun |
| Usulan baseline | Baseline `ID-INP-INT-004` dan `ID-INP-CAP-011` menyatakan ownership Farmasi dan domain klinis lain tidak boleh diduplikasi ke dalam Inpatient. Ini mendukung arah pelonggaran, bukan arah membuat entity tandingan |
| Dampak | Bila persetujuan tidak didapat, Rawat Inap harus membangun entity dokumentasi dan resep sendiri. Itu mengubah ownership, relasi entity, dan memecah rekam medis pasien menjadi dua tempat |
| Pemilik yang dibutuhkan | Pemilik modul `ClinicalManagement` dan `PharmacyManagement`; keduanya belum ditunjuk |
| Status | `OPEN` |
| Dampak implementasi atau domain | `INP-S05` dan `INP-S06` berhenti. Slice lain boleh berjalan |

### `DEC-INP-002`

| Field | Isi |
| --- | --- |
| Pertanyaan | Apakah pemilik `EmergencyInstallationManagement` menyetujui bahwa disposisi `RANAP` menutup kunjungan IGD dan membuat kunjungan rawat inap baru, serta menyetujui penanda `ClosesEmergencyVisit` mulai benar-benar dijalankan? |
| Kemampuan terdampak | `INP-S09` serah terima IGD ke rawat inap |
| Bukti saat ini | `RWI-DEC-041` dan `RWI-RULE-029` sudah memilih arahnya. `RWI-TF-017` membuktikan penanda `ClosesEmergencyVisit` selama ini tidak pernah dibaca satu pun alur kerja |
| Usulan baseline | Baseline pasal 8 menyatakan perpindahan internal tidak otomatis berarti encounter baru, dan kebijakan rumah sakit yang menentukan batas episodenya. Ini justru menegaskan keputusan itu memang milik rumah sakit, bukan milik agent |
| Dampak | Bila persetujuan tidak didapat, jangkar episode berpindah ke kunjungan IGD, dan syarat pelonggaran `RWI-RULE-026` harus diperluas menjadi majemuk |
| Pemilik yang dibutuhkan | Pemilik modul `EmergencyInstallationManagement`: **Rizki Gunawan**, ditetapkan `RWI-DEC-069` 2026-08-24. Persetujuan formalnya belum tercatat; jawabannya sudah tersedia pada `IGD-DEC-067` yang masih `draft` |
| Status | `OPEN` |
| Dampak implementasi atau domain | `INP-S09` berhenti. `INP-S01` tetap boleh berjalan untuk jalur pasien datang langsung dan poliklinik |

### `DEC-INP-003`

| Field | Isi |
| --- | --- |
| Pertanyaan | Apakah pemilik keamanan, privasi, dan hukum menerima bahwa persetujuan umum rawat inap wajib ada tetapi **tidak** menahan admisi, sehingga ada jeda ketika pasien sudah dirawat tanpa persetujuan tertulis? |
| Kemampuan terdampak | `INP-S10` persetujuan umum rawat inap |
| Bukti saat ini | `RWI-DEC-035` dan `RWI-RULE-025` sudah menuliskan pilihannya, dan dokumen keputusan sendiri menolak menaikkannya ke `approved` karena berada di area privasi dan hukum |
| Usulan baseline | Baseline `ID-INP-CAP-002` menempatkan persetujuan sebagai prasyarat pra-admission yang perlu diverifikasi, bukan sebagai syarat penutupan |
| Dampak | Menyentuh kewajiban hukum dan perlindungan data pasien. Bila jawabannya berubah menjadi "menahan admisi", alur admisi ikut berubah |
| Pemilik yang dibutuhkan | Pemilik keamanan dan privasi; belum ditunjuk |
| Status | `OPEN` |
| Dampak implementasi atau domain | `INP-S10` berhenti. Butir persetujuan pada daftar periksa administrasi dapat dinonaktifkan admin, sehingga `INP-S08` tetap boleh berjalan |

### `DEC-INP-004`

| Field | Isi |
| --- | --- |
| Pertanyaan | Apakah kebutuhan isolasi dan pemisahan jenis kelamin hanya menjadi penyaring pencarian tempat tidur, atau menjadi aturan yang menolak penempatan? |
| Kemampuan terdampak | `INP-S11` penempatan menurut jenis kelamin dan isolasi |
| Bukti saat ini | `RWI-DEC-018` memilih "penyaring saja" dan tidak dapat naik ke `approved`. `RWI-FACT-009` menunjukkan PRD memang menulisnya sebagai penyaring opsional |
| Usulan baseline | Baseline `ID-INP-CAP-003` menempatkan kebutuhan isolasi dan kendala jenis kelamin sebagai pembatas penempatan, dan pasal 13 memasukkan isolasi ke dalam daftar kepedulian keselamatan klinis. Observasi ini `REFERENCE_ONLY` dan bukan kebijakan rumah sakit |
| Dampak | Menyentuh pengendalian infeksi dan privasi pasien. Bila menjadi aturan keras, penempatan mendapat validasi baru yang dapat menolak |
| Pemilik yang dibutuhkan | Pemilik klinis untuk isolasi; pemilik privasi untuk jenis kelamin. Keduanya belum ditunjuk |
| Status | `OPEN` |
| Dampak implementasi atau domain | `INP-S11` berhenti. `INP-S01` dan `INP-S02` tetap boleh berjalan dengan syarat pemeriksaan kelayakan penempatan dirancang sebagai titik yang dapat diisi aturan tambahan |

### `DEC-INP-005`

| Field | Isi |
| --- | --- |
| Pertanyaan | Siapa pemilik pengiriman data rawat inap ke SATUSEHAT, data apa yang wajib dikirim, kapan pengiriman dipicu, dan **di mana riwayat lokasi pasien disimpan** — pada catatan penempatan milik Rawat Inap, atau pada kunjungan milik Registrasi? |
| Kemampuan terdampak | `INP-S15` interoperabilitas dan pelaporan. Berpotensi menyentuh `INP-S02` |
| Bukti saat ini | Kata SATUSEHAT tidak muncul sama sekali pada 2.163 baris dokumen keputusan. PRD baris 814 menyebutnya dan menyatakan perubahan lokasi perlu direpresentasikan sebagai histori location di dalam encounter. `RWI-DEC-039` menempatkan riwayat lokasi pada catatan penempatan milik Rawat Inap. Capability map membuktikan kunjungan hari ini hanya punya satu kolom `RoomId` tanpa riwayat |
| Usulan baseline | Baseline `ID-INP-INT-001`, `ID-INP-INT-002`, dan `ID-INP-INT-005` seluruhnya menandai topik ini `integration_relevance: HIGH` dan `audit_relevance: HIGH`. Baseline juga memperingatkan agar Encounter tidak dipetakan satu lawan satu ke satu tabel setempat hanya karena FHIR merepresentasikannya sebagai satu resource |
| Dampak | Menentukan pemilik data riwayat lokasi. Bila jawabannya "pada kunjungan", ownership berpindah dari Rawat Inap ke Registrasi, dan itu perubahan mahal bila baru ketahuan setelah desain jadi |
| Pemilik yang dibutuhkan | Pemilik produk bersama pemilik integrasi dan pemilik rekam medis |
| Status | `OPEN` |
| Dampak implementasi atau domain | `INP-S15` berhenti. `INP-S01` dan `INP-S02` boleh berjalan **dengan syarat** catatan penempatan dirancang sebagai riwayat yang dapat dibaca ulang, bukan sekadar penanda keadaan terakhir |

### `DEC-INP-006`

| Field | Isi |
| --- | --- |
| Pertanyaan | Apakah serah terima klinis antar shift keperawatan wajib direkam sistem? Bila ya: siapa menyerahkan, siapa menerima, apa isi minimalnya, apakah penerimaan harus dikonfirmasi, dan bagaimana tugas yang belum tuntas diteruskan? |
| Kemampuan terdampak | Slice serah terima klinis yang belum masuk daftar 18 kemampuan MUST |
| Bukti saat ini | Dokumen keputusan hanya mengenal serah terima IGD ke rawat inap. Pergantian jaga perawat tidak dibahas sama sekali, padahal `RWI-RULE-033` mengakui perawat penanggung jawab bisa berganti di tengah episode |
| Usulan baseline | Baseline `ID-INP-CAP-016` menandainya `SAFETY_CHECK` dan meminta verifikasi pihak penyerah, pihak penerima, isi, konfirmasi penerimaan, tugas yang belum tuntas, informasi kritis, dan waktu berlakunya |
| Dampak | Menyentuh keselamatan pasien. Informasi kritis yang tidak diserahkan adalah penyebab insiden yang lazim di bangsal |
| Pemilik yang dibutuhkan | Pemilik klinis dan pemilik keperawatan; belum ditunjuk |
| Status | `OPEN` |
| Dampak implementasi atau domain | Slice serah terima klinis berhenti. `INP-S04` penugasan perawat tetap boleh berjalan, karena mencatat siapa perawatnya tidak bergantung pada isi serah terima |

### `DEC-INP-007`

| Field | Isi |
| --- | --- |
| Pertanyaan | Apa aturan klinis untuk pasien meninggal dan pasien kabur: siapa yang mencatat, dokumen apa yang wajib, apakah resume pulang tetap wajib, kapan tempat tidur dilepas, dan pelaporan apa yang mengikutinya? |
| Kemampuan terdampak | `INP-S07` untuk dua dari lima cara pulang |
| Bukti saat ini | `RWI-DEC-017` mengakui lima cara pulang dan `approved` untuk keputusan produknya, tetapi baris meninggal dan kabur secara tegas dinyatakan **tetap terbuka secara klinis** |
| Usulan baseline | Baseline `ID-INP-CAP-018` menyebut pulang atas permintaan sendiri, transfer keluar, dan meninggal sebagai jalur yang wewenangnya harus eksplisit, dan memperingatkan agar wewenang pemulangan tidak disimpulkan dari praktik umum |
| Dampak | Menyentuh rekam medis, pelaporan wajib, dan dokumen hukum. Pasien meninggal juga memicu surat keterangan kematian yang bentuknya berbeda |
| Pemilik yang dibutuhkan | Pemilik klinis; belum ditunjuk |
| Status | `OPEN` |
| Dampak implementasi atau domain | `INP-S07` berjalan hanya untuk tiga cara pulang: atas izin DPJP, atas permintaan sendiri, dan dirujuk. Dua sisanya berhenti |

---

## 7. Kesiapan per slice

| Slice | Kesiapan | Decision ID pemblokir | Catatan |
| --- | --- | --- | --- |
| `INP-S01` Admisi dan pemesanan bed | `READY_FOR_DOMAIN_DESIGN` | — | Hanya untuk jalur pasien datang langsung dan poliklinik. Jalur IGD menunggu `DEC-INP-002` |
| `INP-S02` Penempatan, census, lama dirawat | `READY_FOR_DOMAIN_DESIGN` | — | Dengan syarat catatan penempatan dirancang sebagai riwayat yang dapat dibaca ulang, lihat `DEC-INP-005` |
| `INP-S03` Perpindahan dan pindah kelas | `READY_FOR_DOMAIN_DESIGN` | — | Lengkap termasuk kewenangan per pasien |
| `INP-S04` Penugasan perawat | `READY_FOR_DOMAIN_DESIGN` | — | Isi serah terima antar shift terpisah, lihat `DEC-INP-006` |
| `INP-S05` Dokumentasi klinis dan visite | `BUSINESS_DECISION_REQUIRED` | `DEC-INP-001` | Dua alternatifnya menghasilkan model domain yang sama sekali berbeda |
| `INP-S06` Resep dan obat pulang | `BUSINESS_DECISION_REQUIRED` | `DEC-INP-001` | Bergantung pada keputusan yang sama dengan `INP-S05` |
| `INP-S07` Keputusan pulang dan resume | `PARTIALLY_READY` | `DEC-INP-007` | Tiga cara pulang siap; meninggal dan kabur berhenti |
| `INP-S08` Clearance dan penutupan | `PARTIALLY_READY` | `DEC-INP-007` lewat `INP-S07` | Mesin penutupan siap. Yang menunggu hanya syarat penutupan untuk dua cara pulang yang terblokir |
| `INP-S09` Serah terima IGD | `BUSINESS_DECISION_REQUIRED` | `DEC-INP-002` | — |
| `INP-S10` Persetujuan umum | `BUSINESS_DECISION_REQUIRED` | `DEC-INP-003` | — |
| `INP-S11` Jenis kelamin dan isolasi | `BUSINESS_DECISION_REQUIRED` | `DEC-INP-004` | Satu-satunya butir berstatus `CONFLICT` |
| `INP-S12` Bayi baru lahir dan boks bayi | `READY_FOR_DOMAIN_DESIGN` | — | Master sudah punya seluruh penanda yang dibutuhkan |
| `INP-S13` Riwayat status, audit, daftar pantau | `READY_FOR_DOMAIN_DESIGN` | — | Dua dari tiga daftar pantau siap; daftar pantau kepatuhan pengkajian dan CPPT menunggu `INP-S05` |
| `INP-S14` Pengaturan admin | `READY_FOR_DOMAIN_DESIGN` | — | — |
| `INP-S15` Interoperabilitas dan pelaporan | `BUSINESS_DECISION_REQUIRED` | `DEC-INP-005` | Slice ini belum pernah masuk daftar kemampuan mana pun |

### 7.1 Dependency antar slice

| Slice | Bergantung pada | Sifat ketergantungan |
| --- | --- | --- |
| `INP-S02` | `INP-S01` | Penempatan hanya terjadi setelah admisi diaktifkan |
| `INP-S03` | `INP-S02` | Perpindahan menutup satu penempatan dan membuka penempatan lain |
| `INP-S07` | `INP-S05` | Isi resume merujuk diagnosis dan tindakan. Struktur resume tetap dapat dirancang lebih dulu karena resume menyimpan salinannya sendiri |
| `INP-S08` | `INP-S07` | Penutupan menuntut resume tertandatangani |
| `INP-S08` | `INP-S06` | Butir "obat pulang sudah diserahkan" dapat dinonaktifkan admin, sehingga ketergantungan ini **tidak** memblokir |
| `INP-S12` | `INP-S01`, `INP-S02` | Bayi mendapat episode dan penempatan sendiri |
| `INP-S13` | `INP-S05` | Hanya untuk satu dari tiga daftar pantau |
| `INP-S15` | `INP-S02` | Riwayat lokasi menjadi bahan pengiriman |

---

## 8. Apa yang boleh berjalan dan apa yang harus berhenti

### 8.1 Boleh diteruskan ke `hospital-domain-architect`

Delapan slice berikut dinyatakan **independen** dari penilaian `PARTIALLY_READY` dan boleh
dirancang arsitektur domainnya sekarang:

`INP-S01`, `INP-S02`, `INP-S03`, `INP-S04`, `INP-S12`, `INP-S13`, `INP-S14`

ditambah bagian `INP-S07` dan `INP-S08` yang menyangkut tiga cara pulang: atas izin DPJP, atas
permintaan sendiri, dan dirujuk.

Bersama-sama kedelapan slice itu sudah membentuk satu perjalanan yang utuh dan bermakna:

`admisi → pesan bed → tempatkan → census → tugaskan perawat → pindah bila perlu → putuskan pulang
→ resume → daftar periksa → kelayakan keuangan → tutup episode → bed kembali kosong`

Dengan kata lain, tulang punggung modul ini sudah dapat dirancang tanpa menunggu satu pun
keputusan yang terbuka.

### 8.2 Harus berhenti

| Yang berhenti | Alasan |
| --- | --- |
| `INP-S05` dan `INP-S06` | Dua alternatif `DEC-INP-001` menghasilkan model domain yang berbeda total: memakai ulang tabel modul lain, atau membangun lima aggregate baru |
| `INP-S09` | `DEC-INP-002` menentukan kunjungan mana yang menjadi jangkar episode |
| `INP-S10` | `DEC-INP-003` adalah keputusan hukum dan privasi |
| `INP-S11` | `DEC-INP-004` adalah satu-satunya `CONFLICT`, dan menyentuh pengendalian infeksi |
| `INP-S15` | `DEC-INP-005` menentukan pemilik data riwayat lokasi |
| Serah terima klinis antar shift | `DEC-INP-006`, kemampuan ini bahkan belum masuk daftar 18 MUST |
| Cara pulang meninggal dan kabur | `DEC-INP-007` |

### 8.3 Syarat yang harus dibawa ke arsitektur domain

Dua syarat berikut wajib dipatuhi arsitek domain supaya slice yang berjalan tidak perlu dibongkar
ketika keputusan yang terbuka akhirnya turun:

1. **Catatan penempatan tempat tidur dirancang sebagai riwayat yang dapat dibaca ulang**, lengkap
   dengan waktu mulai dan waktu berakhir setiap penempatan, bukan sekadar penanda keadaan
   terakhir. Ini menjaga agar `DEC-INP-005` dapat dijawab ke arah mana pun tanpa membongkar
   `INP-S02`.
2. **Pemeriksaan kelayakan penempatan dirancang sebagai satu titik yang dapat diisi aturan
   tambahan**, bukan sebagai daftar syarat yang ditanam mati. Ini menjaga agar `DEC-INP-004` dapat
   berubah dari penyaring menjadi penolak tanpa membongkar `INP-S01` dan `INP-S02`.

---

## 9. Keterbatasan penilaian ini

1. **Tidak ada SOP rumah sakit yang disahkan.** Seluruh aturan bisnis berasal dari wawancara dan
   dari PRD berstatus usulan. Gerbang ini menilai kelengkapannya, bukan keabsahan
   kelembagaannya.
2. **Pemilik kebutuhan masih pemegang sementara tanpa nama.** Karena itu tidak ada satu pun butir
   pada dokumen ini yang boleh dibaca sebagai persetujuan rumah sakit.
3. **Baseline rumah sakit Indonesia berstatus `Reference coverage: PARTIAL`.** Ketiadaan sebuah
   topik di dalam baseline **tidak** boleh dibaca sebagai bukti bahwa topik itu tidak dibutuhkan.
4. **Gerbang ini tidak membuka database dan tidak menjalankan aplikasi.** Fakta source seluruhnya
   diambil dari capability map revision `1.2`.
5. **Regulasi tidak diverifikasi ulang.** Observasi `ID-INP-REG-001` berstatus
   `VERIFY_CURRENT_REGULATION`, dan gerbang ini tidak memeriksa apakah regulasi yang dirujuk masih
   berlaku.

---

## 10. Handoff

### 10.1 Yang dikirim ke `hospital-domain-architect`

| Field | Nilai |
| --- | --- |
| Modul | `InPatientManagement` / Rawat Inap, prefix `Inp` |
| Slice yang dikirim | `INP-S01`, `INP-S02`, `INP-S03`, `INP-S04`, `INP-S12`, `INP-S13`, `INP-S14`, ditambah bagian `INP-S07` dan `INP-S08` untuk tiga cara pulang |
| Klasifikasi kesiapan | `PARTIALLY_READY` dengan slice siap yang dinyatakan eksplisit independen |
| Revision bukti | Decision log revision `2`; capability map revision `1.2` |
| Source SHA | Backend `5afb54b`; frontend `dec4fdeff` |
| Decision ID yang belum selesai | `DEC-INP-001` s.d. `DEC-INP-007` |
| Baseline observation ID yang dipakai | `ID-INP-INT-001` s.d. `005`, `ID-INP-REG-001`, `ID-INP-CAP-001` s.d. `020`, seluruhnya `REFERENCE_ONLY` |
| Syarat yang wajib dipatuhi | Dua butir pada bagian 8.3 |
| Keluaran hilir yang diharapkan | Bounded context, batas aggregate, ownership konsep, relasi, lifecycle, authorization, audit, integrasi, dampak billing, dan batas keselamatan klinis untuk slice yang dikirim saja |

### 10.2 Yang dikembalikan ke `/grill-me`

Tujuh Decision ID pada bagian 6. Empat di antaranya adalah **persetujuan pihak lain**
(`DEC-INP-001`, `DEC-INP-002`, `DEC-INP-003`, `DEC-INP-004`) dan tidak dapat ditutup lewat
wawancara dengan pemegang sementara — butuh orang yang benar-benar memegang modul atau memegang
kewenangan klinis.

Tiga sisanya adalah **keputusan bisnis baru** yang belum pernah ditanyakan sama sekali:

- `DEC-INP-005` interoperabilitas SATUSEHAT dan pemilik riwayat lokasi;
- `DEC-INP-006` serah terima klinis antar shift keperawatan;
- `DEC-INP-007` aturan klinis pasien meninggal dan pasien kabur.

`DEC-INP-005` dan `DEC-INP-006` adalah temuan baru gerbang ini. Keduanya tidak pernah muncul pada
Scope Pass maupun Closure Pass, dan keduanya ditemukan lewat pembandingan dengan baseline rumah
sakit Indonesia.

### 10.3 Skill berikutnya

| Urutan | Skill | Untuk apa |
| ---: | --- | --- |
| 1 | `/hospital-domain-architect` | Merancang arsitektur domain untuk delapan slice yang sudah siap. Boleh dijalankan sekarang |
| 2 | `/grill-me` Amendment Pass | Menutup `DEC-INP-005`, `DEC-INP-006`, dan `DEC-INP-007` yang memang dapat dijawab pemilik kebutuhan |
| 3 | Tindakan organisasi | Menunjuk pemilik modul tetangga, pemilik klinis, dan pemilik privasi untuk `DEC-INP-001` s.d. `DEC-INP-004` |
| 4 | `/design-business-module` | Setelah arsitektur domain berdiri untuk slice yang siap |

Urutan 1 dan 2 dapat berjalan bersamaan, karena slice yang dikirim ke arsitektur domain tidak
bergantung pada ketiga keputusan yang dikembalikan ke wawancara.
