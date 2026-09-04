# Rawat Inap — Requirement Completeness Gate

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| Assessment revision | `1.4` |
| Assessment date | 21 Agustus 2026 (`Asia/Jakarta`); **focused reassessment Dokter Rawat Inap dan Keperawatan, 2 September 2026** |
| Assessment status | `CURRENT` |
| Koreksi `1.1` | Tiga keterangan yang menyatakan `DEC-INP-001` masih terbuka diperbaiki; kesiapan belum dinilai ulang pada revision itu |
| Focused reassessment `1.2` | Menilai ulang `INP-S05` bagian dokter, `INP-S06`, serta `CAP-015` berdasarkan decision log revision `7`, PRD final, dan capability map revision `1.3`. Hasil kanonisnya ada pada bagian 11 |
| Decision closure `1.3` | Menyerap hasil Amendment Pass `CAP-025` pada decision log revision `8`. `DEC-INP-008` ditutup oleh `RWI-DEC-084` dan `RWI-DEC-085`; hasil kanonis terbaru untuk Dokter Rawat Inap ada pada bagian 12 |
| Focused reassessment `1.4` | Menilai **lima kemampuan Keperawatan** yang tidak pernah punya slice sendiri: `CAP-012`, `CAP-013`, `CAP-014`, `CAP-016`, dan `CAP-027`. Slice baru `INP-S16`. Hasilnya pada bagian 13 |
| **Overall readiness** | **`PARTIALLY_READY`** |
| Ready destination | `hospital-domain-architect` atau langsung `design-business-module`. Ketujuh capability Dokter Rawat Inap siap sesuai bagian 12; empat kemampuan Keperawatan aktif siap sesuai bagian 13 |
| Business evidence | [`00-interview-decisions.md`](../00-interview-decisions.md) revision `11`, SHA-256 `f34b7aef1352d4c5a817ffeaf988c6eed514d668d3d92051b78806bfc09e635c`. Revision `8` SHA-256 `065b5cd5…` dipakai pada penilaian `1.0` s.d. `1.3` |
| Capability evidence | [`01-existing-capability-map.md`](../01-existing-capability-map.md) revision `1.3`, SHA-256 `0155b345abea61f1b69e6adaf48ee91056b5efaf7fa672ea6300e0546bf4db03` |
| Primary business source | `docs/Modul-RS/Rawat-Inap/PRD_Final_Rawat_Inap_100_Persen.md`, `PRD-RWI-FINAL-001` v1.0.0, SHA-256 `fb5e75d7a1ffffdaddf084a90ec417b00b893b2be23aac0a98ddef5d7bbddc55` |
| Baseline rujukan | `indonesia-hospital-domain-reference`, berkas `references/inpatient.md`, `Reference coverage: PARTIAL`, seluruh observasi berstatus `REFERENCE_ONLY` |
| Backend snapshot | `93b3227c431401d8f586dec4e1fb25fbf41766e3` (branch `MHamzah`) |
| Frontend snapshot | `863f24b0d1617069310c04e5770b47fd1b518b5b` (branch `HamzahV2`) |
| Write boundary | Dokumen evidence ini dan sinkronisasi metadata/hash blueprint. Tidak ada source aplikasi, migration, entity, endpoint, UI, task, database, atau ClickUp yang diubah |

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
| `INP-S05` | Dokumentasi klinis rawat inap, visite, dan penunjang dari workspace dokter | CAP-012, CAP-014, **CAP-015**, CAP-020, CAP-021, CAP-022, CAP-024, CAP-025 | `RWI-RULE-017`, `021`, `026`; `RWI-DEC-080`, `081`, `083` |
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
| **`INP-S16`** | **Keperawatan rawat inap** — ditambahkan revision `1.4` | CAP-012, CAP-013, CAP-014, CAP-016, CAP-027 | `RWI-RULE-021`, `RWI-RULE-026`, `RWI-RULE-033` |

`INP-S15` **tidak** berasal dari dokumen keputusan. Slice ini muncul dari pembandingan dengan
baseline rumah sakit Indonesia, dan penjelasannya ada di bagian 4.11.

### 1.3 Yang sengaja tidak dinilai

Daftar di luar scope pada assessment awal tetap historis. Sejak `RWI-DEC-080`, `CAP-015` dan
`CAP-023` masuk scope Rawat Inap sebagai **workspace dan kontrak integrasi**, bukan sebagai mesin
Laboratorium, Radiologi, atau Farmasi tandingan. Mesin pemrosesan internal, stok/dispensing,
validasi hasil, dan buku besar tetap berada di modul pemiliknya.

---

## 2. Bukti yang dipakai dan wewenangnya

### 2.1 Urutan wewenang yang dipakai

| Urutan | Jenis bukti | Tersedia untuk modul ini | Keterangan |
| ---: | --- | --- | --- |
| 1 | Requirement eksplisit terkini dari rumah sakit/user | **Ada** | Decision log revision `7`; Muhammad Hamzah dinyatakan sebagai owner lewat `RWI-DEC-061`, dan `PRD-RWI-FINAL-001` diterima sebagai baseline lewat `RWI-DEC-080` |
| 2 | SOP atau kebijakan rumah sakit yang disahkan | **Tidak ada** | Tidak ada satu pun SOP yang dilampirkan atau dirujuk |
| 3 | Keputusan rapat yang dikonfirmasi | **Tidak ada** | Tidak ada notulen yang dirujuk |
| 4 | Bukti bisnis analis atau ClickUp yang disetujui | **Ada untuk target produk** | `PRD-RWI-FINAL-001` v1.0.0 menjadi baseline requirement; nilai kebijakan klinis/legal tertentu tetap menunggu owner terkait sebelum produksi |
| 5 | Baseline rumah sakit Indonesia | **Ada** | `references/inpatient.md`, `Reference coverage: PARTIAL`, seluruhnya `REFERENCE_ONLY` |
| 6 | Bukti implementasi Quilvian V2 | **Ada dan kuat** | Focused impact scan pada `01-existing-capability-map.md` revision `1.3`, backend `93b3227`, frontend `863f24b` |
| 7 | Bukti legacy Quilvian V1 | **Tidak dipakai** | Tidak ada lampiran legacy untuk modul ini |

### 2.2 Catatan penting tentang wewenang bukti

Ini yang paling menentukan hasil penilaian, dan harus dibaca sebelum tabel mana pun:

Pada assessment awal, owner masih dicatat sebagai “pemegang sementara”. Keadaan itu sudah
`superseded`: `RWI-DEC-061` menetapkan Muhammad Hamzah sebagai owner Rawat Inap, dan
`RWI-DEC-062` memberi persetujuan lintas `ClinicalManagement`, `PharmacyManagement`, serta
`MasterData`. Karena itu `DEC-INP-001` tidak lagi menjadi blocker bisnis.

Clinical Governance, Security/Privacy, Pharmacy, Laboratory, dan Radiology tetap membutuhkan
sign-off kebijakan atau kontrak final sebelum produksi. Ketiadaan sign-off produksi tersebut
tidak otomatis memblokir domain design selama bentuk targetnya sudah dikunci dan nilai policy
yang belum final tetap configurable serta tidak dipalsukan.

Ketiadaan SOP yang disahkan tidak dipakai sebagai alasan memblokir, karena bukti tingkat 1 dan 4
sudah menjawab sebagian besar pertanyaan. Tetapi ketiadaan itu dicatat sebagai keterbatasan pada
bagian 9.

---

## 3. Ringkasan hasil

| Hal | Jumlah |
| --- | ---: |
| Slice yang dinilai | 15 |
| Slice yang dinilai ulang pada revision `1.2` | 2 — `INP-S05` bagian dokter dan `INP-S06` |
| Slice `READY_FOR_DOMAIN_DESIGN` | 8 |
| Slice `PARTIALLY_READY` | 3 |
| Slice `BUSINESS_DECISION_REQUIRED` | 4 |
| Capability focused scope `READY_FOR_DOMAIN_DESIGN` | 6 |
| Capability focused scope `BUSINESS_DECISION_REQUIRED` | 1 — `CAP-025` |
| Dimensi kelengkapan focused scope yang dinilai | 18 |
| Butir focused `PROPOSED`/`MISSING` nonblocking | 4 |
| Butir focused `CONFLICT` / Decision ID pemblokir | 1 / 1 — `DEC-INP-008` |

Jumlah blocker global di luar focused scope tidak dihitung ulang pada revision `1.2`; statusnya
tetap historis sampai slice terkait dinilai ulang dengan decision log terbaru.

**Kalimat pendeknya:** `INP-S06` dan enam capability Dokter Rawat Inap cukup lengkap untuk domain
design. Hanya `CAP-025 Physician Visit` yang berhenti karena definisi visite lama dan PRD final
bertentangan secara material; blocker lain pada source adalah pekerjaan teknis, bukan keputusan
bisnis.

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
| `ClinicalManagement` | Pelonggaran keharusan antrean dan konsultasi | **SUDAH ADA** — diberikan Muhammad Hamzah 2026-08-21 lewat `RWI-DEC-062`, yang menutup `RWI-OQ-032` sekaligus `DEC-INP-001`. Baris ini semula berbunyi "Belum ada"; **dikoreksi 2026-09-02** |
| `PharmacyManagement` | Pelonggaran resep dan penanda obat pulang | **SUDAH ADA** — sumber, pemberi, dan tanggalnya sama dengan baris di atas. **Dikoreksi 2026-09-02** |
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
| 1 | ~~Persetujuan pemilik `ClinicalManagement` dan `PharmacyManagement` atas pelonggaran antrean dan konsultasi~~ | ~~`MISSING`~~ → **`CONFIRMED`** | ~~`BLOCKING`~~ → **tidak memblokir** | `INP-S05`, `INP-S06` | `DEC-INP-001` **TERTUTUP** 2026-08-21 lewat `RWI-DEC-062`; dicatat di sini 2026-09-02 |
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

Tujuh Decision ID berikut berasal dari assessment awal. Status current dibaca per entri:
`DEC-INP-001` sudah tertutup; Decision ID lain di luar focused scope revision `1.2` tidak dinilai
ulang. Konflik baru focused scope dicatat sebagai `DEC-INP-008` pada bagian 11.8.

### `DEC-INP-001`

> **TERTUTUP 2026-08-21 lewat `RWI-DEC-062`. Koreksi dokumen ini dicatat 2026-09-02.**
>
> Pemilik `ClinicalManagement` dan `PharmacyManagement` adalah Muhammad Hamzah — pemilik yang sama
> dengan `RWI-DEC-061` — dan persetujuannya **sudah diberikan**. `RWI-OQ-032` ikut tertutup pada
> tanggal yang sama. Tabel di bawah sudah dinormalisasi ke keadaan current; jejak pertanyaan
> aslinya tetap dipertahankan pada baris Pertanyaan.
>
> Akibat hilir yang perlu diketahui pembaca: sejak `RWI-DEC-080` (2026-09-02) dokumentasi klinis
> rawat inap **masuk scope modul**, dan `RWI-DEC-083` memetakannya ke sub-modul `keperawatan/`
> serta `dokter-rawat-inap/`. Yang menahannya sekarang bersifat teknis — *shared inpatient
> clinical context resolver* pada `PRD-RWI-FINAL-001` bagian 30.3 — **bukan** keputusan bisnis.

| Field | Isi |
| --- | --- |
| Pertanyaan | Apakah pemilik `ClinicalManagement` dan `PharmacyManagement` menyetujui pelonggaran keharusan antrean dan konsultasi, serta pelonggaran batas satu konsultasi per kunjungan dan satu resep aktif per konsultasi, khusus untuk kunjungan bertipe rawat inap? |
| Kemampuan terdampak | `INP-S05` dokumentasi klinis dan visite; `INP-S06` resep dan obat pulang |
| Bukti saat ini | `RWI-DEC-038` dan `RWI-RULE-026` memilih arah pelonggaran; `RWI-DEC-062` memberi persetujuan owner; `RWI-DEC-080` memasukkan capability ke scope; capability map revision `1.3` membuktikan pembatas source masih ada |
| Usulan baseline | Baseline `ID-INP-INT-004` dan `ID-INP-CAP-011` menyatakan ownership Farmasi dan domain klinis lain tidak boleh diduplikasi ke dalam Inpatient. Ini mendukung arah pelonggaran, bukan arah membuat entity tandingan |
| Dampak | Risiko duplikasi ownership sudah ditutup: Rawat Inap dilarang membangun entity dokumentasi atau mesin resep tandingan |
| Pemilik yang dibutuhkan | Muhammad Hamzah, melalui `RWI-DEC-061` dan `RWI-DEC-062` |
| Status | **`CLOSED`** — 2026-08-21 |
| Dampak implementasi atau domain | Tidak lagi menahan `INP-S05/S06`; gap resolver, multiplicity, dan test adalah pekerjaan teknis |

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
| `INP-S05` Dokumentasi klinis, penunjang, dan visite | `PARTIALLY_READY` | `DEC-INP-008`, hanya `CAP-025` | `CAP-015`, `CAP-020`, `CAP-021`, `CAP-022`, dan `CAP-024` siap. Definisi visite masih konflik; lihat bagian 11 |
| `INP-S06` Resep dan obat pulang | `READY_FOR_DOMAIN_DESIGN` | — | Ownership dan kontrak target sudah dikunci; gap source `Extend` tidak menjadi keputusan bisnis |
| `INP-S07` Keputusan pulang dan resume | `PARTIALLY_READY` | `DEC-INP-007` | Tiga cara pulang siap; meninggal dan kabur berhenti |
| `INP-S08` Clearance dan penutupan | `PARTIALLY_READY` | `DEC-INP-007` lewat `INP-S07` | Mesin penutupan siap. Yang menunggu hanya syarat penutupan untuk dua cara pulang yang terblokir |
| `INP-S09` Serah terima IGD | `BUSINESS_DECISION_REQUIRED` | `DEC-INP-002` | — |
| `INP-S10` Persetujuan umum | `BUSINESS_DECISION_REQUIRED` | `DEC-INP-003` | — |
| `INP-S11` Jenis kelamin dan isolasi | `BUSINESS_DECISION_REQUIRED` | `DEC-INP-004` | Satu-satunya butir berstatus `CONFLICT` |
| `INP-S12` Bayi baru lahir dan boks bayi | `READY_FOR_DOMAIN_DESIGN` | — | Master sudah punya seluruh penanda yang dibutuhkan |
| `INP-S13` Riwayat status, audit, daftar pantau | `READY_FOR_DOMAIN_DESIGN` | — | Dua dari tiga daftar pantau siap; daftar pantau kepatuhan pengkajian dan CPPT menunggu `INP-S05` |
| `INP-S14` Pengaturan admin | `READY_FOR_DOMAIN_DESIGN` | — | — |
| `INP-S15` Interoperabilitas dan pelaporan | `BUSINESS_DECISION_REQUIRED` | `DEC-INP-005` | Slice ini belum pernah masuk daftar kemampuan mana pun |
| **`INP-S16` Keperawatan rawat inap** | **`PARTIALLY_READY`** | — | **Dinilai revision `1.4`, lihat bagian 13.** `CAP-012`, `CAP-013`, dan `CAP-014` siap; `CAP-027` siap hanya pada bagian skrining/rujukan; `CAP-016` `DEFERRED` oleh `RWI-DEC-089`. Nol blocker keputusan bisnis |

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

Delapan slice penuh berikut dinyatakan **independen** dari penilaian `PARTIALLY_READY` dan boleh
dirancang arsitektur domainnya sekarang:

`INP-S01`, `INP-S02`, `INP-S03`, `INP-S04`, `INP-S06`, `INP-S12`, `INP-S13`, `INP-S14`

ditambah bagian siap `INP-S05` (`CAP-015`, `CAP-020`, `CAP-021`, `CAP-022`, `CAP-024`) serta
bagian `INP-S07` dan `INP-S08` yang menyangkut tiga cara pulang: atas izin DPJP, atas permintaan
sendiri, dan dirujuk.

Slice operasional lama tetap membentuk perjalanan episode yang utuh:

`admisi → pesan bed → tempatkan → census → tugaskan perawat → pindah bila perlu → putuskan pulang
→ resume → daftar periksa → kelayakan keuangan → tutup episode → bed kembali kosong`

Focused reassessment menambahkan perjalanan klinis independen yang juga siap dirancang:

`census/episode dokter → kajian medis/SOAP/CPPT → resep/tindakan/penunjang → timeline dan status`

`Physician Visit` sengaja tidak dimasukkan ke rantai kedua sampai `DEC-INP-008` selesai.

### 8.2 Harus berhenti

Baris di luar `CAP-025` dipertahankan dari assessment awal dan tidak dinilai ulang oleh revision
`1.2`; status current-nya wajib diperiksa terhadap decision log terbaru sebelum dipakai.

| Yang berhenti | Alasan |
| --- | --- |
| `INP-S05`, hanya `CAP-025 Physician Visit` | `DEC-INP-008`: keputusan lama menurunkan visite dari SOAP/CPPT, sedangkan PRD final menuntut event mandiri; keduanya menghasilkan persistence, lifecycle, dan hitungan berbeda |
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

1. **Tidak ada SOP rumah sakit yang disahkan.** Aturan bisnis berasal dari keputusan owner dan
   `PRD-RWI-FINAL-001`. Gerbang ini menilai kelengkapan untuk desain, bukan sign-off kelembagaan
   atau klinis untuk produksi.
2. **Owner produk sudah bernama, owner governance belum lengkap.** Muhammad Hamzah memegang
   Product/Domain dan tiga modul tetangga yang disebut `RWI-DEC-062`; Clinical Governance,
   Security/Privacy, serta owner kontrak penunjang masih menjadi gerbang produksi.
3. **Baseline rumah sakit Indonesia berstatus `Reference coverage: PARTIAL`.** Ketiadaan sebuah
   topik di dalam baseline **tidak** boleh dibaca sebagai bukti bahwa topik itu tidak dibutuhkan.
4. **Gerbang ini tidak membuka database dan tidak menjalankan aplikasi.** Focused facts memakai
   capability map revision `1.3`; fakta slice lain yang tidak dipindai ulang tetap historis.
5. **Regulasi tidak diverifikasi ulang.** Observasi `ID-INP-REG-001` berstatus
   `VERIFY_CURRENT_REGULATION`, dan gerbang ini tidak memeriksa apakah regulasi yang dirujuk masih
   berlaku.

---

## 10. Handoff

### 10.1 Yang dikirim ke `hospital-domain-architect`

| Field | Nilai |
| --- | --- |
| Modul | `InPatientManagement` / Rawat Inap, prefix `Inp` |
| Slice yang dikirim | Slice lama yang sudah siap, ditambah `INP-S06` seluruhnya dan bagian `INP-S05` untuk `CAP-015`, `CAP-020`, `CAP-021`, `CAP-022`, `CAP-024` |
| Klasifikasi kesiapan | `PARTIALLY_READY` dengan slice siap yang dinyatakan eksplisit independen |
| Revision bukti | Decision log revision `7`; capability map revision `1.3`; focused reassessment revision `1.2` |
| Source SHA | Backend `93b3227c431401d8f586dec4e1fb25fbf41766e3`; frontend `863f24b0d1617069310c04e5770b47fd1b518b5b` |
| Decision ID scope dokter | `DEC-INP-001` **CLOSED**; `DEC-INP-008` **OPEN**, hanya `CAP-025` |
| Baseline observation ID yang dipakai | `ID-INP-INT-001` s.d. `005`, `ID-INP-REG-001`, `ID-INP-CAP-001` s.d. `020`, seluruhnya `REFERENCE_ONLY` |
| Syarat yang wajib dipatuhi | Ownership `ClinicalManagement`/`PharmacyManagement`/modul penunjang; konteks episode tanpa antrean; tidak membuat engine atau tabel tandingan; policy klinis yang belum final tetap configurable |
| Keluaran hilir yang diharapkan | Amendment arsitektur domain untuk capability siap: ownership konsep, relasi episode, lifecycle, authorization, audit, integrasi, billing, dan keselamatan klinis; `CAP-025` dikecualikan sampai `DEC-INP-008` selesai |

### 10.2 Yang dikembalikan ke `/grill-me`

Untuk focused scope Dokter Rawat Inap, hanya `DEC-INP-008` yang perlu ditutup: apakah visite tetap
diturunkan dari catatan perkembangan dan dihitung satu per dokter per tanggal, atau menjadi event
mandiri yang dapat dicatat tanpa SOAP sebagaimana `PRD-RWI-FINAL-001`.

`DEC-INP-002` s.d. `DEC-INP-007` berada di luar focused reassessment ini. Status historisnya tidak
diubah oleh revision `1.2`; masing-masing harus dibaca bersama keputusan yang lebih baru sebelum
slice terkait dilanjutkan.

### 10.3 Skill berikutnya

| Urutan | Skill | Untuk apa |
| ---: | --- | --- |
| 1 | `/hospital-domain-architect` amendment | Merancang batas domain untuk enam capability dokter yang siap, tanpa `CAP-025` |
| 2 | `/grill-me` Amendment Pass | Menutup `DEC-INP-008` definisi dan perhitungan visite |
| 3 | `/hospital-domain-architect` amendment lanjutan | Menyerap `CAP-025` setelah keputusan visite turun |
| 4 | `/design-business-module` | Setelah arsitektur domain slice target berstatus siap |

Urutan 1 dan 2 dapat berjalan bersamaan. Enam capability siap tidak bergantung pada bentuk
`Physician Visit`; `design-business-module` tetap menunggu arsitektur domain amendment agar tidak
mengarang batas aggregate dan ownership yang sebelumnya sengaja tidak dirancang.

---

## 11. Focused reassessment Dokter Rawat Inap — revision `1.2`

Bagian ini adalah hasil kanonis terbaru **hanya untuk** `INP-S05` bagian dokter, `INP-S06`,
`CAP-015`, `CAP-020` s.d. `CAP-025`, serta dependency langsungnya. Bagian 1–10 tetap menjadi
jejak assessment awal; bila ada perbedaan pada scope ini, bagian 11 yang berlaku.

### 11.1 Scope dan pertanyaan gerbang

| Field | Nilai |
|---|---|
| Modul / sub-modul | `InPatientManagement` / `dokter-rawat-inap` |
| Slice | `INP-S05` bagian dokter dan `INP-S06` |
| Capability | `CAP-015`, `CAP-020`, `CAP-021`, `CAP-022`, `CAP-023`, `CAP-024`, `CAP-025` |
| Pertanyaan | Apakah requirement bisnisnya cukup lengkap untuk amendment arsitektur domain tanpa mengarang ownership, lifecycle, authorization, integrasi, billing, atau keselamatan klinis? |
| Tidak dinilai ulang | Slice keperawatan pada `INP-S05`, slice Rawat Inap lain, kesiapan implementasi/runtime, dan kesiapan produksi |

### 11.2 Bukti yang dipakai

| Bukti | Wewenang | Pemakaian |
|---|---|---|
| `00-interview-decisions.md` revision `7`, hash `e9f2c957…` | Keputusan owner | `RWI-DEC-038`, `062`, `070`, `080`–`083`, serta keputusan visite lama `025` dan `031` |
| `PRD-RWI-FINAL-001` v1.0.0, hash `fb5e75d7…` | Baseline requirement terkini yang diterima lewat `RWI-DEC-080` | Shared Physician Workspace, ketujuh capability, source of truth, RBAC, audit, integrasi, dan production gates |
| Capability map revision `1.3`, hash `0155b345…` | Fakta implementasi saat ini | Status `Ready to reuse`, `Reuse with adapter`, `Extend`, `Repair`, `Missing`, dan `Conflict` pada source |
| Requirement gate revision `1.1` | Jejak assessment lama | Membuktikan `DEC-INP-001` semula menahan `INP-S05/S06` dan kemudian sudah ditutup |
| Hospital domain architecture revision `0.1`, hash `721268f1…` | Arsitektur domain existing | Membuktikan `INP-S05/S06` sengaja belum dirancang; bukan sumber untuk mengarang batas baru |
| Baseline Indonesia pada assessment awal | `REFERENCE_ONLY` | Tidak ada observasi baru yang dinaikkan menjadi requirement pada focused reassessment ini |

### 11.3 Hasil penilaian 18 dimensi

| ID | Dimensi | Status bukti | Hasil focused assessment | Dampak gap |
|---:|---|---|---|---|
| 01 | Tujuan | `CONFIRMED` | Dokter bekerja dari satu konteks episode untuk dokumentasi, resep, tindakan, visite, dan penunjang tanpa antrean semu | — |
| 02 | Aktor | `CONFIRMED` | Physician dan DPJP memiliki kewenangan klinis; profession lain hanya pada CPPT sesuai scope. Administrative attestation visite tidak berlaku sampai ada policy eksplisit | Sign-off peran rinci tetap gerbang produksi |
| 03 | Pemicu/prasyarat | `CONFIRMED` | Episode aktif, patient/encounter cocok, lokasi dan DPJP/assignment authority tersedia | Resolver source masih `Missing`, tetapi ini gap teknis |
| 04 | Alur utama | `CONFIRMED` | Buka census/episode → lihat konteks → tulis/finalkan dokumen atau order → baca status/hasil dari owner → tampilkan timeline | — |
| 05 | Alternatif/exception | `CONFLICT` hanya `CAP-025` | Mismatch episode, episode tertutup, duplicate submit, downstream gagal, dan unauthorized sudah dijelaskan. Bentuk visite sendiri bertentangan antar bukti | `BLOCKING` untuk `CAP-025`; capability lain tidak tertahan |
| 06 | Data minimum | `CONFIRMED` | Patient, encounter, episode, author/role, waktu klinis, isi capability, status, dan correlation/idempotency tersedia per requirement | — |
| 07 | Aturan/validation | `CONFIRMED` kecuali `CAP-025` | Tanpa queue/active IGD; banyak SOAP/konsultasi/resep; isolasi episode; hasil final read-only; clinical commit tidak hilang karena billing failure | Konflik visite `BLOCKING` |
| 08 | Status/lifecycle | `CONFIRMED` + `PROPOSED`, kecuali `CAP-025` | Draft/final/amendment dokumen, verifikasi CPPT, fulfillment Farmasi, dan lifecycle order penunjang cukup. Planned/performed tindakan dipertahankan sebagai capability opsional existing, bukan policy wajib | Lifecycle visite menunggu `DEC-INP-008` |
| 09 | Peran/authorization | `CONFIRMED` | RBAC final PRD membedakan Physician, DPJP, profession CPPT, read-only lintas peran, dan controlled override; backend tetap security boundary | Clinical/Security sign-off sebelum produksi |
| 10 | Dependency antarmodul | `CONFIRMED` | Episode milik Rawat Inap; dokumentasi/tindakan milik Clinical; fulfillment milik Pharmacy; hasil milik Lab/Radiology; charge milik Billing | Tidak boleh membuat tabel/engine tandingan |
| 11 | Integrasi | `CONFIRMED` untuk bentuk minimum | Context episode, correlation id, idempotent retry, status failure, dan source of truth downstream dijelaskan | Kontrak API/status final pemilik modul adalah gerbang produksi, bukan domain design |
| 12 | Hasil akhir | `CONFIRMED` | Catatan final/timeline, prescription identifier dan fulfillment, performed procedure/charge reference, serta order/result reference dapat diamati | — |
| 13 | Pembatalan/koreksi | `CONFIRMED` kecuali `CAP-025` | Dokumen final dikoreksi melalui amendment; cancel/reject order tetap mengikuti owner; silent overwrite/hard delete dilarang | Koreksi visite mengikuti keputusan bentuk visite |
| 14 | Audit/histori | `CONFIRMED` | Author, profession, authored/clinical time, finalize/verify/amend, submit/cancel, correlation, dan actor/reason wajib ditelusuri | Payload medis tidak boleh masuk custom logger |
| 15 | Notifikasi | `MISSING` | Requirement hanya mewajibkan daftar pantau CPPT overdue dan integration failure; push notification tidak ditetapkan | `NON_BLOCKING_STANDARD`; jangan mengarang kanal notifikasi |
| 16 | Billing/charge | `CONFIRMED` | Tindakan billable mengirim trigger idempotent; failure billing tidak menghapus record klinis; Rawat Inap hanya membaca status finansial yang diizinkan | Rekonsiliasi menjadi tanggung jawab owner Billing |
| 17 | Keselamatan klinis | `CONFIRMED` untuk desain | Identitas episode, mismatch A/B, allergy/context header, author, finalization, verified result, dan authority merupakan guard wajib | Nilai SLA serta sign-off governance menahan produksi, bukan desain |
| 18 | Pelaporan/traceability | `CONFIRMED` | Timeline, history visite, monitoring CPPT, failure integration, dan audit actor/time/correlation ditetapkan | Bentuk laporan visite menunggu `DEC-INP-008` |

### 11.4 Butir `CONFIRMED`

1. `RWI-DEC-080` memasukkan tujuh capability dokter ke scope; `RWI-DEC-083` memetakannya tanpa
   capability yatim.
2. `RWI-DEC-081` mengunci ownership lintas modul dan melarang tabel dokumentasi `Inp*` tandingan.
3. `DEC-INP-001` tertutup oleh `RWI-DEC-062`; pemilik `ClinicalManagement` dan
   `PharmacyManagement` sudah menyetujui perubahan yang dibutuhkan.
4. Konteks target adalah `PatientId + EncounterId + InpatientEpisodeId + Physician/Assignment
   Authority`; `QueueId` dan active IGD visit bukan prasyarat rawat inap.
5. Konflik frontend antrean rawat jalan adalah ketidaksesuaian implementasi terhadap target yang
   sudah jelas, bukan pilihan bisnis yang masih terbuka.

### 11.5 Butir `PROPOSED`, `MISSING`, dan `CONFLICT`

| ID | Butir | Status | Dampak | Scope |
|---|---|---|---|---|
| `RWI-DOK-RQG-001` | Tidak ada push notification; daftar pantau dan retry list menjadi mekanisme operasional MVP | `PROPOSED` | `NON_BLOCKING_STANDARD` | `CAP-021`, integrasi |
| `RWI-DOK-RQG-002` | Nilai SLA verifikasi CPPT dan policy lintas profesi belum mendapat sign-off Clinical Governance | `MISSING` | `CONFIGURABLE_DEFAULT`; menahan produksi, tidak menahan domain design | `CAP-021` |
| `RWI-DOK-RQG-003` | Kontrak API/status final Pharmacy, Laboratory, dan Radiology belum disetujui owner masing-masing | `MISSING` | `NON_BLOCKING_STANDARD`; target minimum sudah ada, sign-off menahan produksi | `CAP-015`, `CAP-023` |
| `RWI-DOK-RQG-004` | Keputusan lama: visite berasal dari SOAP/CPPT, tanpa event/form tersendiri, satu per dokter per tanggal. PRD final: visite adalah event eksplisit yang dapat ada tanpa SOAP dan memakai idempotency | `CONFLICT` | **`BLOCKING`**; mengubah persistence, lifecycle, audit, hitungan, UI, billing potensial, dan acceptance | `CAP-025` |
| `RWI-DOK-RQG-005` | Apakah tindakan dokter selalu direncanakan lebih dulu tidak dinyatakan; source sudah mendukung planned lalu performed maupun pencatatan langsung performed | `PROPOSED` | `NON_BLOCKING_STANDARD`; pertahankan kedua jalur sebagai kemampuan opsional, jangan hard-code kewajiban planning | `CAP-024` |

### 11.6 Gap teknis bukan keputusan bisnis

| Evidence | Status capability source | Makna bagi requirement gate |
|---|---|---|
| `DOK-TRC-CTX-01` | `Ready to reuse` | Fondasi episode/census/DPJP tersedia |
| `DOK-TRC-INT-01`, `DOK-TRC-VER-01` | `Missing` | Resolver dan bukti otomatis harus dibangun; requirement targetnya tidak ambigu |
| `DOK-TRC-DEF-01`, `DOK-TRC-CAP020` | `Repair` | Defect no-queue perlu repair dan regression test, bukan keputusan owner |
| `DOK-TRC-INT-02`, `CAP015`, `CAP021`, `CAP023`, `CAP024`, `AUTH-01` | `Extend` | Scope extension sudah dibatasi target dan keputusan |
| `DOK-TRC-CAP022`, `DOK-TRC-FE-BASE` | `Reuse with adapter` | Semantik target jelas; bentuk adapter menjadi wewenang desain hilir |
| `DOK-TRC-FE-01` | `Conflict` | Consumer existing harus dirework ke episode/census; tidak boleh mengubah requirement agar cocok dengan antrean rawat jalan |

### 11.7 Penutupan tiga pertanyaan audit sebelumnya

| Pertanyaan | Keputusan gate |
|---|---|
| `RWI-DOK-TRQ-001` — reuse `TrxPatientAssessment` atau tabel lain | **Bukan blocker requirement.** Requirement mengunci Medical Assessment sebagai record/lifecycle berbeda dari SOAP; bentuk persistence adalah keputusan arsitektur hilir dengan ownership tetap `ClinicalManagement` |
| `RWI-DOK-TRQ-002` — owner Clinical Governance | **Gerbang produksi.** Domain design boleh memakai controlled configurable policy tanpa mengarang angka atau approval |
| `RWI-DOK-TRQ-003` — rework atau karantina frontend | **Bukan keputusan requirement.** Target mewajibkan rework ke episode/census; consumer sekarang tidak boleh di-sign-off atau dirilis sebelum sesuai |

### 11.8 Decision Log baru

#### `DEC-INP-008` — Definisi dan perhitungan Physician Visit

| Field | Isi |
|---|---|
| Pertanyaan | Apakah Physician Visit tetap diturunkan dari SOAP/CPPT dan dihitung maksimal satu per dokter per tanggal, atau menjadi event mandiri yang dapat dicatat tanpa SOAP serta memakai idempotency per submission? |
| Kemampuan terdampak | `CAP-025 Physician Visit`; history/timeline dokter dan kemungkinan charge/report visite |
| Bukti saat ini | `RWI-DEC-025`, `RWI-RULE-017`, dan `RWI-DEC-031` memilih turunan catatan; `PRD-RWI-FINAL-001` CAP-025 serta keputusan final nomor 12 memilih event eksplisit |
| Usulan baseline | Gunakan event eksplisit seperti PRD final agar visite tanpa SOAP tetap auditable; keputusan ini **PROPOSED**, bukan jawaban owner |
| Dampak | Menentukan apakah persistence/event baru diperlukan, hubungan dengan SOAP/CPPT, uniqueness/idempotency, lifecycle koreksi, hitungan laporan, dan bentuk UI |
| Pemilik yang dibutuhkan | Muhammad Hamzah sebagai Product/Domain owner; Clinical Governance dan Billing perlu meninjau bila hitungan visite dipakai klinis atau finansial |
| Status | **`OPEN`** |
| Dampak implementasi/domain | `CAP-025` berhenti. Enam capability dokter lain boleh berjalan secara independen |

### 11.9 Kesiapan per capability

| Capability | Slice | Kesiapan | Blocker bisnis | Catatan |
|---|---|---|---|---|
| `CAP-015` Supporting Services | `INP-S05` bagian dokter | `READY_FOR_DOMAIN_DESIGN` | — | Owner processing dan source of truth hasil jelas; source perlu extension |
| `CAP-020` SOAP | `INP-S05` | `READY_FOR_DOMAIN_DESIGN` | — | Multiple entry, finalization, amendment, dan episode context jelas |
| `CAP-021` CPPT | `INP-S05` | `READY_FOR_DOMAIN_DESIGN` | — | Policy value configurable; sign-off menahan produksi |
| `CAP-022` Medical Assessment | `INP-S05` | `READY_FOR_DOMAIN_DESIGN` | — | Semantik/lifecycle terpisah dari SOAP jelas; persistence diputuskan di arsitektur |
| `CAP-023` Medication Management | `INP-S06` | `READY_FOR_DOMAIN_DESIGN` | — | Ownership Pharmacy dan discharge medication type jelas |
| `CAP-024` Physician Procedures | `INP-S05` | `READY_FOR_DOMAIN_DESIGN` | — | Konteks, performer, planned/performed, billing trigger, dan idempotency cukup |
| `CAP-025` Physician Visit | `INP-S05` | **`BUSINESS_DECISION_REQUIRED`** | `DEC-INP-008` | Jangan merancang persistence, lifecycle, endpoint, atau UI visite sebelum keputusan turun |

Hasil turunan: `INP-S06` **`READY_FOR_DOMAIN_DESIGN`**; `INP-S05`
**`PARTIALLY_READY`**; sub-modul `dokter-rawat-inap` secara keseluruhan **`PARTIALLY_READY`**.

### 11.10 Apa yang boleh berjalan dan harus berhenti

**Boleh berjalan:** amendment `hospital-domain-architect` untuk `CAP-015`, `CAP-020`, `CAP-021`,
`CAP-022`, `CAP-023`, dan `CAP-024`, dengan dependency dan production gate tetap terbuka secara
eksplisit.

**Harus berhenti:** domain design dan seluruh artefak hilir khusus `CAP-025`, sampai
`DEC-INP-008` ditutup. Tidak boleh memakai blueprint lama sebagai jawaban karena blueprint itu
sendiri memilih salah satu sisi konflik tanpa Decision ID yang menyelesaikannya.

### 11.11 Handoff terfokus

| Field | Nilai |
|---|---|
| `next_owner_ready_slice` | `hospital-domain-architect` amendment |
| `next_owner_blocked_slice` | `grill-me` Amendment Pass untuk `DEC-INP-008` |
| `requirement_readiness` | `PARTIALLY_READY` |
| `ready_capabilities` | `CAP-015`, `CAP-020`, `CAP-021`, `CAP-022`, `CAP-023`, `CAP-024` |
| `blocked_capability` | `CAP-025` |
| `decision_ids` | `DEC-INP-001 CLOSED`; `DEC-INP-008 OPEN` |
| `source_sha` | Backend `93b3227c431401d8f586dec4e1fb25fbf41766e3`; frontend `863f24b0d1617069310c04e5770b47fd1b518b5b` |
| `expected_output` | Amendment domain architecture yang menambah slice physician siap tanpa mengubah konsep episode existing; CAP-025 tetap ditandai belum dirancang |

---

## 12. Penutupan keputusan Physician Visit — revision `1.3`

Bagian ini adalah hasil kanonis terbaru untuk `CAP-025` dan menggantikan status `OPEN`/
`BUSINESS_DECISION_REQUIRED` pada bagian 10.2, 11.5, 11.8, 11.9, 11.10, dan 11.11. Bagian lama
dipertahankan agar pembaca dapat menelusuri alasan keputusan.

### 12.1 Keputusan yang turun

| Decision ID | Keputusan | Status |
|---|---|---|
| `RWI-DEC-084` | Physician Visit adalah event klinis eksplisit. Event dapat ada tanpa SOAP/CPPT, memiliki tautan dokumen opsional, dan duplicate submission dicegah melalui `request id/idempotency key` | `approved` |
| `RWI-DEC-085` | Setiap visite nyata yang dicatat sebagai event berbeda dihitung satu pada riwayat klinis dan laporan operasional. Dua visite pada hari yang sama tetap dua | `approved` |
| `RWI-DEC-025` | Visite diturunkan dari SOAP/CPPT | `superseded` |
| `RWI-DEC-031` | Visite digabung maksimal satu per dokter per tanggal berdasarkan catatan pertama | `superseded` |
| `RWI-OQ-049` | Hitungan dua event visite aktual pada hari yang sama | `closed` oleh `RWI-DEC-085` |
| `DEC-INP-008` | Definisi dan perhitungan Physician Visit | **`CLOSED`** |

### 12.2 Aturan yang dapat diuji

1. Event visite pukul 07:40 tetap muncul pada history walaupun SOAP baru dibuat pukul 07:52 atau
   belum dibuat.
2. SOAP/CPPT tanpa event Physician Visit tidak otomatis menambah visite.
3. Dua visite nyata oleh dokter yang sama pukul 07:40 dan 16:10 menghasilkan dua event dan
   hitungan klinis/operasional dua.
4. Retry dengan `idempotency key` yang sama menghasilkan event yang sama, bukan visite kedua.
5. Billing boleh mengagregasikan dua event menjadi satu tagihan harian hanya melalui kebijakan
   owner Billing yang disetujui terpisah. Agregasi tidak boleh menghapus atau mengubah dua event
   klinis.
6. Koreksi mengikuti prinsip PRD final: tidak ada hard delete atau silent overwrite; actor, waktu,
   alasan, dan perubahan harus dapat diaudit. Detail lifecycle diturunkan pada arsitektur domain,
   bukan dikarang oleh requirement gate.

Bukti acceptance detail: `RWI-AC-150` s.d. `RWI-AC-156` pada decision log revision `8`.

### 12.3 Kesiapan capability setelah keputusan

| Capability | Kesiapan current | Blocker bisnis |
|---|---|---|
| `CAP-015` Supporting Services | `READY_FOR_DOMAIN_DESIGN` | — |
| `CAP-020` SOAP | `READY_FOR_DOMAIN_DESIGN` | — |
| `CAP-021` CPPT | `READY_FOR_DOMAIN_DESIGN` | — |
| `CAP-022` Medical Assessment | `READY_FOR_DOMAIN_DESIGN` | — |
| `CAP-023` Medication Management | `READY_FOR_DOMAIN_DESIGN` | — |
| `CAP-024` Physician Procedures | `READY_FOR_DOMAIN_DESIGN` | — |
| `CAP-025` Physician Visit | **`READY_FOR_DOMAIN_DESIGN`** | —; gap source `Missing` tetap pekerjaan teknis |

Hasil turunan:

- sub-modul `dokter-rawat-inap` berstatus **`READY_FOR_DOMAIN_DESIGN`** untuk seluruh tujuh
  capability;
- bagian dokter pada `INP-S05` siap; bagian keperawatan pada slice yang sama tidak dinilai ulang;
- `INP-S06` tetap siap;
- overall Rawat Inap tetap **`PARTIALLY_READY`** karena slice modul lain berada di luar keputusan
  ini dan tidak dinilai ulang.

### 12.4 Handoff current

| Field | Nilai |
|---|---|
| `next_owner` | `hospital-domain-architect` amendment |
| `ready_capabilities` | `CAP-015`, `CAP-020`, `CAP-021`, `CAP-022`, `CAP-023`, `CAP-024`, `CAP-025` |
| `blocked_capability` | — untuk scope Dokter Rawat Inap |
| `decision_ids` | `RWI-DEC-084`, `RWI-DEC-085`; `DEC-INP-008 CLOSED` |
| `required_boundary` | Dokumentasi dan visite tetap milik `ClinicalManagement`; jangan membuat tabel/engine `Inp*` tandingan. Billing hanya menerima dampak/agregasi melalui kontrak terpisah |
| `expected_output` | Amendment arsitektur domain ketujuh capability dokter, termasuk ownership event Physician Visit, relasi episode, lifecycle koreksi, authorization, audit, idempotency, integrasi Billing, dan traceability |

---

## 13. Focused reassessment Keperawatan — revision `1.4`

### 13.1 Kenapa penilaian ini dijalankan

Taksonomi slice `INP-S01` s.d. `INP-S15` pada bagian 4 disusun ketika scope modul masih **18 kemampuan**.
Setelah `RWI-DEC-080` mengangkat `PRD-RWI-FINAL-001` menjadi baseline dan scope menjadi **28 kemampuan**,
lalu `RWI-DEC-082` memecah modul menjadi tiga sub-modul, lima kemampuan Keperawatan tidak pernah mendapat
slice sendiri. Akibatnya terbaca dari dokumen ini sendiri:

| Kemampuan | Berapa kali disebut dokumen ini sebelum revision `1.4` | Akibatnya |
|---|:---:|---|
| `CAP-012` Nursing Assessment | 1 | Masuk daftar `INP-S05`, tetapi **tidak pernah dinyatakan siap** — bagian 7 hanya menyebut `CAP-015`, `020`, `021`, `022`, dan `024` |
| `CAP-013` Nursing Care | **0** | Tidak ada slice mana pun yang mencakupnya |
| `CAP-014` Nursing Interventions | 1 | Sama seperti `CAP-012` |
| `CAP-016` Equipment Usage | 2 | Hanya muncul sebagai catatan, bukan sebagai penilaian |
| `CAP-027` Nutrition Care | **0** | Tidak ada slice mana pun yang mencakupnya |

Bagian 12.3 dokumen ini sudah mencatatnya apa adanya: *"bagian keperawatan pada slice yang sama tidak
dinilai ulang"*. Penilaian `1.4` menutup lubang itu, dan **tidak** menilai ulang slice modul lain.

### 13.2 Scope penilaian

| Field | Nilai |
|---|---|
| Slice baru | **`INP-S16`** — Keperawatan rawat inap |
| Sub-modul | [`keperawatan/`](../keperawatan/), `RWI-BP-001` revision `5` |
| Kemampuan | `CAP-012`, `CAP-013`, `CAP-014`, `CAP-016`, `CAP-027` — sesuai `RWI-DEC-083` |
| Aturan bisnis terkait | `RWI-RULE-021`, `RWI-RULE-026`, `RWI-RULE-033` |
| Yang **tidak** dinilai | Seluruh slice `INP-S01` s.d. `INP-S15`. Hasilnya tetap sebagaimana revision `1.3` |

### 13.3 Bukti yang dipakai

| Bukti | Revision | Wewenang |
|---|---|---|
| `PRD-RWI-FINAL-001` bagian 16, 17, 20, 22, 26, 27, 29 | v1.0.0 | Requirement rumah sakit terkini — wewenang tertinggi untuk "apa yang seharusnya dibangun" |
| [`00-interview-decisions.md`](../00-interview-decisions.md) | `11` | Keputusan pemilik yang dikonfirmasi |
| [`01-existing-capability-map.md`](../01-existing-capability-map.md) | `1.3` | Bukti implementasi — **bagian 1–14 stale** terhadap SHA terbaru; dipakai hanya untuk pertanyaan "apa yang ada sekarang" |
| [`../keperawatan/`](../keperawatan/) sebelas artefak desain | `0.2` / `0.2.0` | Bukan bukti requirement; dipakai untuk memeriksa apakah requirement sudah cukup untuk dirancang |

> **Batas yang dijaga.** Keberadaan desain `keperawatan` **bukan** bukti bahwa requirement-nya lengkap.
> Penilaian ini menilai buktinya, bukan dokumen turunannya.

### 13.4 Hasil penilaian 18 dimensi

| ID | Dimensi | Status bukti | Hasil focused assessment | Dampak gap |
|---:|---|---|---|---|
| 01 | Tujuan | `CONFIRMED` | Catatan klinis perawat yang awal dan berkelanjutan sepanjang episode, beserta rencana asuhan, tindakan nyata, dan skrining gizi | — |
| 02 | Aktor | `CONFIRMED` | RBAC PRD bagian 26: Perawat `C/U/F`, Kepala Ruangan `R/O*`, Supervisor `O*`, Dokter dan DPJP `R` | — |
| 03 | Pemicu/prasyarat | `CONFIRMED` | Episode `Admitted` ditambah penugasan/kewenangan perawat — PRD 16.3 | Resolver `INT-KEP-01` masih `Missing`, tetapi itu **gap teknis** |
| 04 | Alur utama | `CONFIRMED` | PRD 16.3 menulis alurnya utuh: `Admitted` → penugasan → pengkajian awal → identifikasi risiko → asuhan/tindakan → pengkajian ulang harian → evaluasi → perencanaan pulang | — |
| 05 | Alternatif/exception | `CONFIRMED` | Pengkajian ulang tidak menimpa pengkajian awal (16.2 aturan 3); nilai nyeri lama tidak ditimpa (aturan 6); pasien salah dibatalkan Kepala Ruangan dengan alasan (PRD 29); kegagalan Billing tidak menghapus catatan klinis (`CAP-014` aturan 5) | — |
| 06 | Data minimum | `CONFIRMED` | PRD 16.2 aturan 4 dan 5 merinci isi General Assessment dan Fall Risk; `CAP-014` aturan 2 merinci tindakan: aksi, waktu, pelaku, hasil, konteks episode | — |
| 07 | Aturan/validation | `CONFIRMED` | Tiga belas aturan `CAP-012`, enam aturan `CAP-013`, lima aturan `CAP-014` | Katalog SDKI bersyarat — lihat 13.6 |
| 08 | Status/lifecycle | `CONFIRMED` | PRD 16.2 aturan 10 menyebut `NotStarted`, `Draft`, `Completed`, `Amended`, dan **melarang** status itu diturunkan dari status episode | — |
| 09 | Peran/authorization | `CONFIRMED` | RBAC bagian 26 ditambah `CAP-014` aturan AC-03: bukan penulis dan bukan supervisor tidak dapat menyunting diam-diam catatan final | — |
| 10 | Dependency antarmodul | `CONFIRMED` | Tabel milik `ClinicalManagement` (`RWI-DEC-081`, PRD 23.1); asuhan gizi milik modul Gizi; tagihan milik Billing | Modul Gizi **belum berwujud** — lihat 13.6 |
| 11 | Integrasi | `CONFIRMED` untuk bentuk minimum | Pemicu tagihan idempotent (`CAP-014` aturan 5); rujukan gizi tanpa menduplikasi konteks pasien (`CAP-027` AC-01) | — |
| 12 | Hasil akhir | `CONFIRMED` | AC-CAP012-03: pengkajian `Completed` tampil pada census/workspace **tanpa** menambah status episode baru | — |
| 13 | Pembatalan/koreksi | `CONFIRMED` | PRD 16.2 aturan 12 dan 13: dilarang hard-delete dan timpa diam-diam; amandemen wajib menyimpan pelaku, waktu, alasan, dan perubahan. PRD 27.3 aturan 7 menyatakan koreksi dokumen klinis **mengikuti aturan amandemen/versi masing-masing jenis dokumen** | Konsistensi dengan mesin keutuhan dokumen `NON_BLOCKING_STANDARD` — lihat 13.6 |
| 14 | Audit/histori | `CONFIRMED` | PRD 27.1 mewajibkan jejak finalisasi/verifikasi/amandemen dokumen klinis; 27.2 merinci isinya termasuk alasan dan nilai sebelum/sesudah | — |
| 15 | Notifikasi | `MISSING` | Requirement hanya menuntut **daftar pantau kepatuhan**, bukan pemberitahuan dorong | `NON_BLOCKING_STANDARD` |
| 16 | Billing/charge | `CONFIRMED` | `CAP-014` aturan 5 dan AC-02: tindakan billable mengirim pemicu beridentitas idempotency; kegagalan Billing **tidak** menghapus catatan klinis | — |
| 17 | Keselamatan klinis | `CONFIRMED` untuk desain | Pemisahan pengkajian awal dan ulang, riwayat nyeri longitudinal, larangan hard-delete, dan kewenangan penyuntingan menutup risiko utama | Nilai batas waktu klinis `CONFIGURABLE_DEFAULT` — lihat 13.6 |
| 18 | Pelaporan/traceability | `CONFIRMED` | Daftar pantau kepatuhan pengkajian, `DueAt`/`CompletedAt`/overdue berdasarkan konfigurasi aktif (16.2 aturan 11) | — |

### 13.5 Butir `CONFIRMED` yang menopang kesiapan

| No | Butir | Bukti |
|---:|---|---|
| 1 | Pengkajian terikat Patient + Encounter + Inpatient Episode, dan **tidak boleh** menuntut `QueueId` rawat jalan maupun kunjungan IGD aktif | PRD 16.2 aturan 1 dan 2; `AC-CAP012-01` |
| 2 | Pengkajian awal dan pengkajian ulang adalah record **terpisah** | PRD 16.2 aturan 3; `AC-CAP012-02` |
| 3 | Status pengkajian diturunkan dari record nyata, bukan dari status episode | PRD 16.2 aturan 10; `AC-CAP012-03` |
| 4 | Rencana asuhan diturunkan dari temuan pengkajian, punya masalah/tujuan/rencana/evaluasi dan lifecycle sendiri, serta menutup butir tanpa menghapus riwayat | PRD `CAP-013` aturan 1, 2, 6; `AC-CAP013-01` s.d. `03` |
| 5 | Tindakan mencatat apa yang **benar-benar dilakukan**, boleh ad-hoc tanpa rencana lebih dulu | PRD `CAP-014` aturan 1 dan 3 |
| 6 | Skrining gizi menghasilkan pemicu rujukan **tanpa** menjadikan perawat pemilik asuhan gizi profesional | PRD 16.2 aturan 7; `CAP-027` aturan 1 dan 3 |
| 7 | Kewenangan per peran lengkap, termasuk larangan penyuntingan diam-diam oleh selain penulis/supervisor | PRD bagian 26; `AC-CAP014-03` |

### 13.6 Butir `PROPOSED`, `MISSING`, dan `CONFLICT`

| ID | Butir | Status | Dampak | Alasannya |
|---|---|---|---|---|
| K-01 | Nilai batas waktu klinis pengkajian (`RWI-RULE-021`) | `MISSING` | **`CONFIGURABLE_DEFAULT`** | PRD 16.2 aturan 11 **secara eksplisit** mewajibkan SLA klinis dapat dikonfigurasi Clinical Governance dan **melarang PRD men-hard-code angka yang belum disetujui**. Mekanismenya wajib ada; angkanya memang konfigurasi. Karena itu butir ini **tidak memblokir desain** |
| K-02 | Katalog terminologi SDKI/SLKI/SIKI | `PROPOSED` | **`CONFIGURABLE_DEFAULT`** | PRD `CAP-013` aturan 3 bersyarat: *"Jika terminology SDKI/SLKI/SIKI digunakan rumah sakit"*. Pemakaiannya belum dinyatakan, dan struktur rencana asuhan tetap dapat dirancang tanpa katalognya |
| K-03 | Konsistensi mesin koreksi dokumen keperawatan terhadap mesin keutuhan dokumen `ClinicalManagement` | `PROPOSED` | **`NON_BLOCKING_STANDARD`** | `RWI-DEC-086`/`087` bercakupan **catatan dokter**; dokumen keperawatan tidak disebut. **PRD 27.3 aturan 7 menyelesaikannya**: koreksi dokumen klinis mengikuti aturan amandemen/versi **masing-masing jenis dokumen**. Model amandemen sendiri karenanya **sah**. Yang tersisa adalah pilihan keseragaman, bukan pertentangan |
| K-04 | Cakupan `CAP-013` terhadap scope MVP | **`CONFLICT`** | **`NON_BLOCKING_STANDARD`** | `RWI-DEC-034` (`approved`, 2026-08-20) menyatakan `CAP-013` berada di **luar scope** dan ditunda setelah MVP, turunan `RWI-DEC-004`. `RWI-DEC-080` (2026-09-02) **menggantikan batas scope itu**, dan `RWI-DEC-083` menugaskan `CAP-013` ke `keperawatan`. Keputusan yang lebih baru dan lebih spesifik menang, tetapi `RWI-DEC-004` dan `RWI-DEC-034` **belum ditandai `superseded`** — lihat 13.8 |
| K-05 | Asuhan gizi ujung ke ujung (`CAP-027` bagian modul Gizi) | `MISSING` | **`BLOCKING` hanya untuk bagian itu** | PRD 23.1 menaruh Nutrition Assessment/Care pada modul Gizi, dan modul itu **belum berwujud** di `Areas/`. Bagian milik Keperawatan — skrining dan rujukan — tidak ikut terblokir |
| K-06 | Pemberitahuan dorong | `MISSING` | `NON_BLOCKING_STANDARD` | Requirement hanya menuntut daftar pantau kepatuhan |

### 13.7 Gap teknis, bukan keputusan bisnis

Dua butir berikut **bukan** bahan gerbang ini dan tidak boleh dipakai untuk menahan kesiapan requirement:

| Butir | Sifatnya | Pemilik |
|---|---|---|
| `INT-KEP-01` *shared inpatient clinical context resolver* — `TrxPatientAssessment` masih menuntut `QueueId` | **Teknis.** Requirement-nya justru sudah tegas: PRD 16.2 aturan 2 melarang `QueueId` diwajibkan | `ClinicalManagement` |
| Modul Gizi berstatus `PLANNED` | **Ketersediaan modul**, bukan keputusan bisnis yang belum diambil | Roadmap Quilvian |

### 13.8 Decision Log

Tidak ada Decision ID **baru** yang memblokir. Satu butir kebersihan decision log perlu ditutup pemilik:

Decision ID: `DEC-INP-009`

| Field | Isi |
|---|---|
| Pertanyaan | Apakah `RWI-DEC-004` dan `RWI-DEC-034` dinyatakan `superseded` oleh `RWI-DEC-080` dan `RWI-DEC-083`, sehingga `CAP-013` resmi berada **di dalam** scope? |
| Kemampuan terdampak | `CAP-013` Nursing Care |
| Bukti saat ini | `RWI-DEC-034` `approved` menyatakan di luar scope; `RWI-DEC-083` `approved` dan lebih baru menugaskannya ke `keperawatan`; `keperawatan/04-prd-to-mvp.md` menulisnya `MUST HAVE` `EPIC KEP-03` |
| Usulan baseline | Keduanya ditandai `superseded` dengan rujukan ke `RWI-DEC-080` dan `RWI-DEC-083`, sesuai disiplin yang sudah dipakai pada `RWI-DEC-018`, `025`, dan `031` |
| Dampak | Hasil bisnis: apakah `EPIC KEP-03` dibangun. Tidak mengubah model domain, lifecycle, maupun authorization |
| Pemilik | Muhammad Hamzah, Product/Domain |
| Status | **`CLOSED` 2026-09-02** oleh `RWI-DEC-090`: `RWI-DEC-004` dan `RWI-DEC-034` dinyatakan `superseded`, `CAP-013` resmi di dalam scope. Ekornya, `OQ-RI-011` terbuka kembali sebagai butir non-blocking |
| Dampak implementasi | Nihil bila ditutup sesuai usulan. Bila pemilik justru menegaskan `CAP-013` tetap di luar scope, `EPIC KEP-03` dicabut dari `keperawatan/04-prd-to-mvp.md` |

### 13.9 Kesiapan per capability

| Capability | Slice | Kesiapan | Blocker bisnis | Catatan |
|---|---|---|---|---|
| `CAP-012` Nursing Assessment | `INP-S16` | **`READY_FOR_DOMAIN_DESIGN`** | — | Tiga belas aturan, lima acceptance criteria, status lifecycle, dan kewenangan lengkap. SLA `CONFIGURABLE_DEFAULT` |
| `CAP-013` Nursing Care | `INP-S16` | **`READY_FOR_DOMAIN_DESIGN`** | — | Enam aturan dan tiga acceptance criteria cukup. Katalog SDKI bersyarat dan tidak menahan struktur. `DEC-INP-009` bersifat kebersihan catatan |
| `CAP-014` Nursing Interventions | `INP-S16` | **`READY_FOR_DOMAIN_DESIGN`** | — | Konteks, pelaku, waktu, idempotency, dan pemisahan kegagalan Billing tegas |
| `CAP-016` Equipment Usage | `INP-S16` | **`DEFERRED`** — tidak dinilai | — | Dikeluarkan dari scope rilis pertama secara tertulis oleh `RWI-DEC-089`. Dinilai ulang saat `RWI-OQ-048` dibuka kembali |
| `CAP-027` Nutrition Care | `INP-S16` | **`PARTIALLY_READY`** | — | **Skrining dan rujukan siap** — itulah bagian milik Keperawatan. **Asuhan gizi ujung ke ujung berhenti** karena modul Gizi belum berwujud, dan itu ketersediaan modul, bukan keputusan bisnis |

Hasil turunan: slice **`INP-S16` `PARTIALLY_READY`**; sub-modul `keperawatan` **siap dirancang dan siap
direncanakan untuk empat kemampuan aktifnya**. Overall Rawat Inap tetap **`PARTIALLY_READY`** karena slice
modul lain berada di luar penilaian ini.

### 13.10 Apa yang boleh berjalan dan apa yang harus berhenti

**Boleh berjalan:**

- perencanaan delivery `keperawatan` untuk `CAP-012`, `CAP-013`, `CAP-014`, dan bagian skrining/rujukan `CAP-027`;
- desain lanjutan bila diperlukan, dengan atau tanpa `hospital-domain-architect` — sub-modul ini sudah mencatat `DOMAIN_ARCHITECTURE_NOT_RUN` beserta alasannya.

**Harus berhenti:**

- pekerjaan asuhan gizi ujung ke ujung, sampai modul Gizi berdiri;
- pekerjaan `CAP-016`, sampai `RWI-OQ-048` dibuka kembali;
- **pemakaian** kelima kemampuan untuk pasien sungguhan, sampai `INT-KEP-01` dikerjakan. Ini menahan rilis, **bukan** menahan desain maupun perencanaan.

### 13.11 Handoff

| Field | Nilai |
|---|---|
| `capability_scope` | `CAP-012`, `CAP-013`, `CAP-014`, `CAP-027` bagian skrining/rujukan |
| `requirement_readiness` | `INP-S16` `PARTIALLY_READY`; empat kemampuan aktif `READY_FOR_DOMAIN_DESIGN` |
| `requirement_evidence_status` | `CONFIRMED` untuk 16 dari 18 dimensi; `MISSING` pada notifikasi dan nilai SLA; satu `CONFLICT` kebersihan catatan |
| `domain_architecture_readiness` | `DOMAIN_ARCHITECTURE_NOT_RUN` — batas konteks dan kepemilikan data sudah ditetapkan `RWI-DEC-081` dan PRD 23.1, sehingga tidak ada batas domain yang perlu diturunkan ulang |
| `decision_ids` | `DEC-INP-009` `OPEN` non-blocking; `RWI-OQ-048` `CLOSED` oleh `RWI-DEC-089` |
| `dependency_ids` | `INT-KEP-01` teknis; modul Gizi `PLANNED` |
| `next_owner` | `plan-module-delivery` untuk sub-modul `keperawatan` |
| `required_boundary` | Dokumentasi keperawatan tetap milik `ClinicalManagement` (`RWI-DEC-081`). Sub-modul ini **MUST NOT** membuat tabel tandingan, termasuk tabel pemakaian alat |
| `expected_output` | Roadmap backend dan frontend `keperawatan` beserta traceability requirement, tanpa satu pun task untuk `EPIC KEP-06` |
