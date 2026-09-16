# Rawat Inap — Requirement Completeness Gate

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| Assessment revision | **`1.6`** |
| Assessment date | 21 Agustus 2026 (`Asia/Jakarta`); focused reassessment Dokter Rawat Inap dan Keperawatan, 2 September 2026; focused reassessment penyelarasan `PRD-RWI-V2-001`, 15 September 2026; **penutupan keputusan `RLN-PH-04`, 15 September 2026** |
| Assessment status | `CURRENT` |
| Koreksi `1.1` | Tiga keterangan yang menyatakan `DEC-INP-001` masih terbuka diperbaiki; kesiapan belum dinilai ulang pada revision itu |
| Focused reassessment `1.2` | Menilai ulang `INP-S05` bagian dokter, `INP-S06`, serta `CAP-015` berdasarkan decision log revision `7`, PRD final, dan capability map revision `1.3`. Hasil kanonisnya ada pada bagian 11 |
| Decision closure `1.3` | Menyerap hasil Amendment Pass `CAP-025` pada decision log revision `8`. `DEC-INP-008` ditutup oleh `RWI-DEC-084` dan `RWI-DEC-085`; hasil kanonis terbaru untuk Dokter Rawat Inap ada pada bagian 12 |
| Focused reassessment `1.4` | Menilai **lima kemampuan Keperawatan** yang tidak pernah punya slice sendiri: `CAP-012`, `CAP-013`, `CAP-014`, `CAP-016`, dan `CAP-027`. Slice baru `INP-S16`. Hasilnya pada bagian 13 |
| Focused reassessment `1.5` | Fase `RLN-PH-04`: kemampuan baru dan yatim dari `PRD-RWI-V2-001`. Slice baru `INP-S17` s.d. `INP-S21`. Decision ID baru `DEC-INP-010` s.d. `DEC-INP-012`. Menutup temuan manifest `RLN-04` dan `RLN-07`. Hasilnya pada bagian 14 |
| Decision closure `1.6` | Menyerap Amendment Pass penutupan gate `RLN-PH-04`: `RWI-DEC-145` s.d. `RWI-DEC-149` dan `RWI-AC-219` s.d. `RWI-AC-231`. `DEC-INP-010` dan `DEC-INP-011` **`CLOSED`**, `DEC-INP-012` **`DEFERRED`**. `INP-S17` naik ke `READY_FOR_DOMAIN_DESIGN`; `INP-S19` `READY_FOR_DOMAIN_DESIGN` terbatas pada sliding scale, handover shift dan transfusi `DEFERRED`. Hasil kanonis terbaru untuk `INP-S17` dan `INP-S19` ada pada bagian 15 |
| **Overall readiness** | **`PARTIALLY_READY`** |
| Ready destination | `hospital-domain-architect` atau langsung `design-business-module`. Ketujuh capability Dokter Rawat Inap siap sesuai bagian 12; empat kemampuan Keperawatan aktif siap sesuai bagian 13; **slice penyelarasan V2 `INP-S17`, `S18`, `S19` (sliding scale), `S20`, dan `S21` siap sesuai bagian 14 dan 15; handover shift dan transfusi `DEFERRED`** |
| Business evidence | **Bagian 15:** [`00-interview-decisions.md`](../00-interview-decisions.md) revision `21`, SHA-256 `1c55c80a50aee11ef005ccde6315c2935cbe21504e8596798b89bf7f2d45102a`, dihitung **setelah** sinkronisasi register oleh gate ini (catatan blocker, satu baris Gate Sebelum Produksi, satu baris Riwayat Pass). Isi keputusannya sama dengan hash sebelum sinkronisasi `1eaaa7ab…aa168ad`. **Bagian 14:** [`00-interview-decisions.md`](../00-interview-decisions.md) revision `20`, SHA-256 `b278013547dfa3c8f1bfa21fdd442cdaa416a8015939f1628794ab7e03db0fb7`. Sebelumnya revision `11`, SHA-256 `f34b7aef1352d4c5a817ffeaf988c6eed514d668d3d92051b78806bfc09e635c`. Revision `8` SHA-256 `065b5cd5…` dipakai pada penilaian `1.0` s.d. `1.3` |
| Capability evidence | **Bagian 14 dan 15:** [`01-existing-capability-map.md`](../01-existing-capability-map.md) revision `1.4`, SHA-256 `337a10f09d6e91b06395405098bad09623452de062e10a720a637bd22daa543a`. Sebelumnya revision `1.3`, SHA-256 `0155b345abea61f1b69e6adaf48ee91056b5efaf7fa672ea6300e0546bf4db03` |
| Primary business source | **Bagian 14 dan 15:** `PRD-RWI-V2-001` v`2.0` SHA-256 `2b3b2f29c9e547f448f186d7ac990e33dc3bdede8043a9b4bebfad6fbe0a679f` dan `PRD-to-MVP-Rawat-Inap-V2` v`1.0.0` SHA-256 `9804b9680580f6b15a5008b9bcbb13a419fef077ef6680d732a6e2135b3ce141`, dibaca berlapis sesuai `RWI-DEC-110`. Baseline: `docs/Modul-RS/Rawat-Inap/PRD_Final_Rawat_Inap_100_Persen.md`, `PRD-RWI-FINAL-001` v1.0.0, SHA-256 `fb5e75d7a1ffffdaddf084a90ec417b00b893b2be23aac0a98ddef5d7bbddc55` |
| Baseline rujukan | `indonesia-hospital-domain-reference`, berkas `references/inpatient.md`, `Reference coverage: PARTIAL`, seluruh observasi berstatus `REFERENCE_ONLY` |
| Backend snapshot | **Bagian 14: `df3679c0d5b2f08106702153eb242d3a6cb2929b`** (branch `MHamzah`); bagian 15 tidak membaca source ulang karena seluruh kemampuan yang dinilainya berstatus `Missing` pada capability map; sebelumnya `93b3227c431401d8f586dec4e1fb25fbf41766e3` |
| Frontend snapshot | **Bagian 14: `147355f505e875148b8416866ada6cf8b2f1ad99`** (branch `HamzahV2`), pembanding V1 `13c3a96b`; sebelumnya `863f24b0d1617069310c04e5770b47fd1b518b5b` |
| Write boundary | Dokumen evidence ini dan sinkronisasi metadata/hash blueprint. Revision `1.6` juga menyinkronkan register pemicunya: catatan blocker, tabel Gate Sebelum Produksi, dan Riwayat Pass pada `00-interview-decisions.md`, serta tabel fase dan baris artefak pada `blueprint-manifest.md`. Tidak ada source aplikasi, migration, entity, endpoint, UI, task, database, atau ClickUp yang diubah |

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
| 4 | ~~Isolasi dan pemisahan jenis kelamin sebagai penyaring, bukan penolak penempatan~~ **TERTUTUP 2026-09-11** | ~~`CONFLICT`~~ `RESOLVED` | ~~`BLOCKING`~~ tidak memblokir | `INP-S11` | `DEC-INP-004` **`CLOSED`** oleh `RWI-DEC-064`, `RWI-DEC-065`, dan `RWI-DEC-101`; dicatat `RWI-DEC-104` |
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
| Status | ~~`OPEN`~~ **`CLOSED` 2026-09-11.** Dijawab `RWI-DEC-064` dan `RWI-DEC-065` yang keduanya `approved` 21 Agustus 2026, lalu dipersempit `RWI-DEC-101` 11 September 2026 yang mencabut bagian kamar tanpa mengembalikannya menjadi penyaring. Penutupannya dicatat `RWI-DEC-104` |
| Dampak implementasi atau domain | ~~`INP-S11` berhenti.~~ **Tidak lagi berlaku.** `INP-S11` naik menjadi `READY_FOR_DOMAIN_DESIGN` sejak 2026-09-11. `INP-S01` dan `INP-S02` tetap berjalan sebagaimana sebelumnya |

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
| `INP-S11` Jenis kelamin dan isolasi | ~~`BUSINESS_DECISION_REQUIRED`~~ **`READY_FOR_DOMAIN_DESIGN`** sejak 2026-09-11 | ~~`DEC-INP-004`~~ — | ~~Satu-satunya butir berstatus `CONFLICT`~~ Pemblokirnya tertutup; lihat `RWI-DEC-104`. Aturan yang berlaku kini `RWI-DEC-101`: jenis kelamin dinilai hanya terhadap penanda tempat tidur |
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
| `INP-S11` | ~~`DEC-INP-004` adalah satu-satunya `CONFLICT`, dan menyentuh pengendalian infeksi~~ **Tertutup 2026-09-11** lewat `RWI-DEC-104`. Pengendalian infeksi tetap dijaga `ISOLATION_REQUIRED` dan `ISOLATION_BED_RESERVED`, yang **tidak** tersentuh pencabutan aturan kamar |
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

---

## 14. Focused reassessment penyelarasan `PRD-RWI-V2-001` — revision `1.5`

> **Catatan revision `1.6`.** Status `INP-S17`, `INP-S19`, gap `G-08`, `G-19`, `G-20`, serta
> `DEC-INP-010` s.d. `DEC-INP-012` pada bagian ini **digantikan bagian 15**. Isinya dipertahankan agar
> pembaca dapat menelusuri alasan keputusan. Hasil `INP-S18`, `INP-S20`, dan `INP-S21` tetap berlaku.

### 14.1 Kenapa penilaian ini dijalankan

Fase `RLN-PH-04` pada `blueprint-manifest.md` bagian 0-B.4 mewajibkan gerbang terfokus untuk
**kemampuan baru dan kemampuan yatim** yang dibawa `PRD-RWI-V2-001`, setelah `RLN-PH-02`
(wawancara) dan `RLN-PH-03` (impact scan) selesai. Dua keputusan juga mewajibkannya secara tertulis:

- `RWI-DEC-116` konsekuensi (2): MAR **wajib** melewati gerbang ini sebelum dirancang;
- `RWI-DEC-133` butir (2): rekonsiliasi obat ikut melewati gerbang bersama MAR.

Penilaian ini menutup temuan manifest `RLN-04` (Penunjang Medis melebar) dan `RLN-07` (tujuh isi
Pengkajian Pasien). Slice `INP-S01` s.d. `INP-S16` **tidak** dinilai ulang; hasilnya tetap
sebagaimana revision sebelumnya.

### 14.2 Scope penilaian

| Slice baru | Nama | Sub-modul | Kemampuan dan isi yang dinilai | Keputusan yang mengikat |
|---|---|---|---|---|
| **`INP-S17`** | Pengkajian Pasien V2 | `keperawatan` | Kajian Umum, Resiko Jatuh, Monitoring Nyeri, Assesment Edukasi, Pengawasan Harian Pasien, Evaluasi Awal (MPP), Perencanaan Pulang, progres | `RWI-DEC-118` s.d. `120`, `124`, `131`, `136`, `141` |
| **`INP-S18`** | Obat keperawatan | `keperawatan` + `PharmacyManagement` | Pemberian obat per dosis (MAR) dan rekonsiliasi obat saat admisi | `RWI-DEC-116`, `117`, `132` s.d. `134` |
| **`INP-S19`** | Kemampuan keselamatan `P1` PRD-to-MVP-V2 | `keperawatan` | Handover shift (`FR-MVP-KEP-015`, `016`), sliding scale (`FR-MVP-KEP-018`), transfusi (`FR-MVP-KEP-019`) | `RWI-DEC-097`, `RWI-DEC-110` |
| **`INP-S20`** | Penunjang Medis enam layanan | `dokter-rawat-inap` + `keperawatan` | Laboratorium, Radiologi, Gizi, Hemodialisa, Bank Darah, Rehab Medik | `RWI-DEC-108`, `RWI-DEC-113` |
| **`INP-S21`** | Dokumentasi klinis dan ruang kerja dokter V2 | `dokter-rawat-inap` + `keperawatan` | Kesamaan layout, Catatan Saya, jenis catatan CPPT, koreksi dan penguncian konsep, penulis dan pembatalan tindakan, verifikasi CPPT, Resep Harian, Template Resep, tab Resume Medis dan ODC | `RWI-DEC-107`, `112`, `115`, `121` s.d. `123`, `125` s.d. `130`, `135`, `137` s.d. `144` |

**Yang tidak dinilai:** seluruh isi sub-modul `episode-rawat-inap`, termasuk serah terima klinis
transfer antarunit (`FR-MVP-EP-016`, `017`), karena `RWI-DEC-113` menampilkannya "Integrasi belum
tersedia" dan kepemilikannya ada di sub-modul itu; `CAP-016` Pemakaian Alat (`RWI-DEC-089`,
`RWI-DEC-108`); lima cara keluar (`MVP-RWI-D-011`).

### 14.3 Bukti yang dipakai

| Bukti | Revision / hash | Wewenang dan penggunaannya |
|---|---|---|
| `PRD-RWI-V2-001` v`2.0`, `docs/Modul-RS/Rawat-Inap/04-prd-to-mvp-final.md` bagian 5, 16–23, 26–47, 56 | SHA-256 `2b3b2f29…0a679f` | Requirement rumah sakit terkini untuk ruang kerja dokter dan keperawatan — menang bila bertentangan, sesuai `RWI-DEC-110` |
| `PRD-to-MVP-Rawat-Inap-V2` v`1.0.0` bagian 13 dan 16 (`FR-MVP-KEP-001` s.d. `020`, `AC-MVP-025` s.d. `034`, `OPEN-MVP-001` s.d. `010`) | SHA-256 `9804b968…3ce141` | Requirement yang tetap berlaku lewat `RWI-DEC-097` dan `RWI-DEC-110` |
| `PRD-RWI-FINAL-001` v1.0.0 | SHA-256 `fb5e75d7…bddc55` | Baseline 28 kemampuan |
| [`00-interview-decisions.md`](../00-interview-decisions.md) | revision `20`, SHA-256 `b2780135…03db0fb7`, keputusan terakhir `RWI-DEC-144` | Keputusan pemilik yang dikonfirmasi |
| [`01-existing-capability-map.md`](../01-existing-capability-map.md) | revision `1.4`, SHA-256 `337a10f0…d22daa543a`, bagian 17 current pada `BE@df3679c0` dan `FE@147355f5` | Bukti "apa yang ada sekarang" saja |
| Frontend V1 `QuilvianSystemFrontendDev@13c3a96b` | commit lokal | Bukti legacy isi formulir V1 — wewenang terendah, dipakai sebagai usulan isian |
| Pembacaan source tambahan pada `BE@df3679c0` | — | `TrxPatientAllergy.cs` (data alergi terstruktur), `TrxPatientProcedure.IsBillingGenerated` (tidak pernah disetel `true` di mana pun) |
| Blueprint `bank-darah` | `00-interview-decisions.md`, manifest status `IN_PROGRESS` | Bukti titik sentuh transfusi; tidak dinilai isinya |

> **Batas yang dijaga.** Keputusan pemilik yang lahir dari opsi yang disusun agent tetap
> `CONFIRMED` karena pemilik memilihnya secara tertulis. Usulan agent yang **tidak** dipilih
> pemilik tetap `PROPOSED` pada penilaian ini.

### 14.4 Hasil penilaian 18 dimensi per slice

Kode: `C` = `CONFIRMED`, `P` = `PROPOSED`, `M` = `MISSING`, `X` = `CONFLICT`. Nomor `G-##` merujuk
tabel 14.6.

| ID | Dimensi | `INP-S17` Pengkajian | `INP-S18` Obat | `INP-S19` P1 keselamatan | `INP-S20` Penunjang | `INP-S21` Dokumentasi dokter |
|---:|---|---|---|---|---|---|
| 01 | Tujuan | `C` PRD 26–34 | `C` PRD 40; `FR-MVP-KEP-009` | `C` `FR-MVP-KEP-015` s.d. `019` | `C` PRD 23, 43 | `C` PRD 7–22 |
| 02 | Aktor | `C` perawat `RWI-DEC-100`; MPP `RWI-DEC-118`, `131` | `C` perawat unit, dokter pemutus rekonsiliasi `RWI-DEC-132` | `C` pemberi/penerima handover; `X` transfusi G-20 | `C` `RWI-DEC-113` | `C` `RWI-DEC-111`, `125`, `139` |
| 03 | Pemicu/prasyarat | `C` episode `Admitted` | `C` resep aktif `RWI-DEC-117`; admisi untuk rekonsiliasi | `M` G-19 | `C` order `CAP-015` | `C` penugasan `RWI-DEC-099`, `128` |
| 04 | Alur utama | `C` PRD 27–34 | `C` `RWI-DEC-116`, `132` | `C` pada PRD-to-MVP-V2; `M` penempatan rilis G-19 | `C` | `C` |
| 05 | Alternatif/exception | `C` `BR-RWI-004`, `007`; `RWI-DEC-119` | `C` `Held`/`Refused`/`Missed` beralasan; obat di luar master `RWI-DEC-134` | `C` `AC-MVP-032`, `033` | `C` "Integrasi belum tersedia" `RWI-DEC-108` | `C` `RWI-DEC-129`, `138`, `143` |
| 06 | Data minimum | `C` bagian dan kelompok PRD; `P` isian rinci G-01; `M` intake obat/darah G-08 | `C` `FR-MVP-KEP-009` s.d. `011`; isian V1 rekonsiliasi | `C` `FR-MVP-EP-017`, `FR-MVP-KEP-015` s.d. `019` | `C` | `C` `RWI-DEC-140`, `144` |
| 07 | Aturan/validation | `C` `RWI-DEC-124`, `136`, `141`; `M` isi klinis G-02 | `C`; `M` jam standar dan jendela `Missed` G-12 | `C`; `M` protokol sliding scale G-19 | `C` | `C` |
| 08 | Status/lifecycle | `C` `RWI-DEC-119`, `120`; `M` status rencana pulang G-10 | `C` enam status dosis `FR-MVP-KEP-010` | `C` snapshot handover immutable `FR-MVP-KEP-016` | `C` | `C` `RWI-DEC-138`, `143`, `144` |
| 09 | Peran/authorization | `C` `RWI-DEC-100`, `118`, `131`, `136`; `P` penanda MPP G-11 | `C` `RWI-DEC-117` butir (3); cek ganda pengguna kedua | `X` transfusi G-20 | `C` | `C` `RWI-DEC-125`, `127`, `128`, `135`, `139`, `143` |
| 10 | Dependency antarmodul | `C` `ClinicalManagement` `RWI-DEC-081`, `118` | `C` `PharmacyManagement` `RWI-DEC-117`, `132`; `MasterData` `RWI-DEC-134` | `X` Bank Darah G-20 | `C` modul penunjang masing-masing | `C` `MedicalRecordManagement` `RWI-DEC-138`, `142`, `144` |
| 11 | Integrasi | `C` internal saja | `C` Farmasi internal | `M` sumber hasil gula darah untuk sliding scale G-19 | `C` | `C` |
| 12 | Hasil akhir | `C` `RWI-DEC-120` | `C` riwayat dosis; draft resep dari rekonsiliasi | `C` | `C` | `C` |
| 13 | Pembatalan/koreksi | `C` addendum `RWI-DEC-091` | `C` koreksi tidak menimpa `RWI-DEC-116` (c) | `C` `FR-MVP-KEP-016`, `017` | `C` | `C` `RWI-DEC-127`, `138`, `143` |
| 14 | Audit/histori | `C` versi instrumen `RWI-DEC-124` | `C` pelaksana, waktu rencana/aktual | `C` | `C` | `C` `RWI-DEC-144` |
| 15 | Notifikasi | Tidak material — daftar pantau `RWI-DEC-029` | `M` penerima notifikasi dugaan reaksi obat G-15 | `C` acknowledgment handover | Tidak material | Tidak material — daftar pantau `RWI-DEC-126`, `129` |
| 16 | Billing/charge | Tidak material — pengkajian tidak menagih | `C` resep dan pemberian bukan tagihan `BR-RWI-013` | `M` transfusi dan sliding scale G-19 | `C` modul penunjang | `C` biaya tindakan lahir dari pelaksanaan, lihat 14.5 butir 9 |
| 17 | Keselamatan klinis | `C` untuk desain; `M` intake obat/darah G-08 | `C` high-alert, idempotency; `M` isi daftar G-13 | `X` G-20 | `C` | `C` alergi saat pakai template `RWI-DEC-122`, lihat 14.7 |
| 18 | Pelaporan/traceability | `C` progres dan kepatuhan | `C` riwayat per dosis | `C` | `C` | `C` |

### 14.5 Butir `CONFIRMED` yang menopang kesiapan

| No | Butir | Bukti |
|---:|---|---|
| 1 | Kajian Umum delapan bagian; Psikososial, Alat Bantu, dan Catatan Relevan sebagai isian; satu fakta satu tempat. Daftar sepuluh bagian PRD bagian 28 dan kemampuan instrumen `FR-MVP-KEP-003` (termasuk sirkulasi dan neurologi) dibaca lewat lapisan `RWI-DEC-110`: isi form mengikuti `RWI-DEC-141` | `RWI-DEC-141`; `RWI-DEC-110` |
| 2 | Resiko Jatuh dan isian wajib berupa konfigurasi berversi, dihitung server, disahkan terpisah dari pengubahnya, tanpa angka tertanam di source | `RWI-DEC-124`, `RWI-DEC-136`; `FR-MVP-KEP-004`, `005`; `AC-MVP-034` |
| 3 | Monitoring Nyeri membedakan Belum dinilai, Tidak nyeri, Ada nyeri, dan Tidak dapat dinilai; riwayat longitudinal tidak menimpa nilai lama | PRD bagian 30; `BR-RWI-007`; `PRD-RWI-FINAL-001` 16.2 aturan 6 |
| 4 | Assesment Edukasi punya penerima, kebutuhan, hambatan, materi, metode, dan evaluasi pemahaman — **bukan** satu catatan teks | PRD bagian 31 |
| 5 | Pengawasan Harian mencatat tanda vital, nyeri, intake, output, balance, gula darah, diet, mobilisasi; total dihitung dari data terstruktur per shift dan rolling 24 jam; koreksi mempertahankan nilai lama | PRD bagian 32; `FR-MVP-KEP-017`; `AC-MVP-031`; `RWI-DEC-120` butir (1) |
| 6 | Evaluasi Awal milik MPP dengan delapan bagian, jangkauan per unit penempatan, dan tidak menahan progres perawat | `RWI-DEC-118`, `RWI-DEC-120`, `RWI-DEC-131` |
| 7 | Perencanaan Pulang dengan delapan kelompok isi dan tidak menutup episode | PRD bagian 34; `BR-RWI-016` |
| 8 | MAR `P0`: enam status dosis beralasan, pelaksana dari akun login, idempoten, cek ganda high-alert oleh pengguna kedua, dugaan reaksi obat tertaut dosis; tabel milik `PharmacyManagement`; rekonsiliasi obat `P0` bersama MAR dari master obat | `RWI-DEC-116`, `117`, `132` s.d. `134`; `FR-MVP-KEP-009` s.d. `014`; `AC-MVP-025` s.d. `029` |
| 9 | Pesanan tindakan yang belum dilaksanakan tidak punya tagihan, sehingga pembatalan otomatis saat episode ditutup (`RWI-DEC-143`) tidak bersinggungan dengan Billing. Source juga tidak pernah menyetel `TrxPatientProcedure.IsBillingGenerated = true` | `BR-RWI-013`; `PRD-RWI-FINAL-001` `CAP-014` aturan 5; `BE@df3679c0` pencarian seluruh `*.cs` |
| 10 | Keenam layanan penunjang adalah perluasan permukaan `CAP-015`, **bukan kemampuan yatim**; Gizi, Hemodialisa, Bank Darah, dan Rehab Medik tampil "Integrasi belum tersedia" tanpa backend baru | `RWI-DEC-108`, `RWI-DEC-113` |
| 11 | Dokumentasi dokter V2 terkunci keputusan: layout mengikuti PRD, Catatan Saya dari satu sumber, jenis catatan CPPT, penulis dan penguncian konsep, penulis dan pembatalan tindakan, verifikasi CPPT hanya DPJP, Template Resep pribadi dengan pemilik dari akun login | `RWI-DEC-107`, `121` s.d. `130`, `135`, `137` s.d. `144` |

### 14.6 Butir `PROPOSED`, `MISSING`, dan `CONFLICT`

| ID | Slice | Butir | Status | Dampak | Alasannya |
|---|---|---|---|---|---|
| G-01 | `S17` | Isian rinci setiap bagian Kajian Umum, Assesment Edukasi, dan Perencanaan Pulang | `PROPOSED` | `NON_BLOCKING_STANDARD` | PRD memberi bagian dan kelompok; isian rinci diusulkan dari label frontend V1 (`01-existing-capability-map.md` bagian 17 `RLN3-CAP-05`, `08`, `11`). Struktur dokumen tidak berubah oleh jumlah isian |
| G-02 | `S17` | Isi klinis instrumen risiko jatuh per kelompok usia dan isian wajib setiap jenis pengkajian | `MISSING` | `CONFIGURABLE_DEFAULT` | `RWI-OQ-056`, `RWI-OQ-057`, `OPEN-MVP-003`. `RWI-DEC-124` sudah menetapkan mekanisme konfigurasi; menahan pemakaian untuk pasien sungguhan, bukan desain |
| G-03 | `S17` | Satu rangkaian data tanda vital dan nyeri per episode yang ditampilkan di Kondisi Umum, Pengawasan Harian, Vital Sign Keperawatan, dan Monitoring Nyeri | `PROPOSED` | `NON_BLOCKING_STANDARD` | Turunan prinsip yang sudah dikonfirmasi pemilik: data terstruktur tidak diduplikasi (`RWI-DEC-140` butir 4) dan satu fakta satu tempat (`RWI-DEC-141` butir 4). Desain wajib menghormatinya; bentuk penyimpanannya milik desain |
| G-04 | `S17` | Instrumen nyeri untuk pasien yang tidak dapat menilai sendiri (bayi, anak, tidak sadar) dan interval kajian ulang setelah intervensi | `MISSING` | `CONFIGURABLE_DEFAULT` | PRD menyediakan keadaan "Tidak dapat dinilai" dan skala 0–10, tanpa instrumen alternatif maupun interval. Wajar berbeda antar rumah sakit dan dapat mengikuti pola konfigurasi berversi `RWI-DEC-124` |
| G-05 | `S17` | Tanda tangan penerima edukasi dan tanda tangan pasien/keluarga pada perencanaan pulang, yang ada di V1 | `MISSING` | `NON_BLOCKING_STANDARD` | PRD v`2.0` tidak menyebutnya. Bila kelak diminta, dapat ditambahkan sebagai bukti pelengkap tanpa mengubah dokumen inti |
| G-06 | `S17` | Batas jam shift untuk total intake/output | `MISSING` | `CONFIGURABLE_DEFAULT` | `FR-MVP-KEP-017` menuntut total per shift, sedangkan sistem tidak punya konsep shift perawat (`RWI-DEC-100`). Jam shift wajar berbeda per unit |
| G-07 | `S17` | Isi checklist setiap bagian Evaluasi Awal dan pengesahnya | `MISSING` | `CONFIGURABLE_DEFAULT` | PRD hanya memberi delapan judul; V1 memakai master `/ChecklistItem` yang dinamis. Menahan pemakaian untuk pasien sungguhan, bukan struktur dokumen |
| G-08 | `S17` | **Sumber volume intake obat dan darah** pada Pengawasan Harian: diketik perawat, atau diturunkan dari MAR dan catatan transfusi | `MISSING` | **`BLOCKING` hanya untuk sub-bagian intake obat dan darah** | PRD bagian 32 mencantumkan intake Obat dan Darah. MAR `FR-MVP-KEP-011` menyimpan dosis dan rute, bukan volume cairan pelarut. Pilihannya mengubah integrasi MAR–intake/output dan berisiko menghitung ganda balance cairan — `DEC-INP-010` |
| G-09 | `S17` | Frekuensi Evaluasi Awal: sekali per episode atau berulang | `MISSING` | `NON_BLOCKING_STANDARD` | Usulan: satu dokumen per episode yang dilengkapi lewat addendum, sesuai namanya "Evaluasi Awal". Tetap terlihat sebagai usulan, bukan kebijakan |
| G-10 | `S17` | Nilai "Status Rencana" pada Perencanaan Pulang | `MISSING` | `NON_BLOCKING_STANDARD` | Usulan: isian informatif yang **tidak** memicu perubahan status episode (`BR-RWI-016`); status dokumennya mengikuti `RWI-DEC-119` |
| G-11 | `S17` | Cara sistem mengenali pengguna berperan MPP | `PROPOSED` | `NON_BLOCKING_STANDARD` | Konvensi proyek: hak akses pada layar Akses Role, bukan nama peran di kode (`RWI-FACT-031`, `RWI-DEC-118` konsekuensi a) |
| G-12 | `S18` | Jam standar pemberian per frekuensi dan jendela waktu sebelum dosis dianggap `Missed` | `MISSING` | `CONFIGURABLE_DEFAULT` | Wajar berbeda antar rumah sakit dan unit; mekanisme status dosis sudah dikonfirmasi `FR-MVP-KEP-010` |
| G-13 | `S18` | Isi daftar obat high-alert dan aturan cek gandanya | `MISSING` | `CONFIGURABLE_DEFAULT` | `RWI-DEC-116` butir (3) menjadikannya keputusan klinis/Farmasi dan **gerbang sebelum produksi** |
| G-14 | `S18` | Interval evaluasi obat PRN | `MISSING` | `CONFIGURABLE_DEFAULT` | `FR-MVP-KEP-013` `P1` menyebut "interval yang ditetapkan" tanpa angka |
| G-15 | `S18` | Penerima notifikasi dugaan reaksi obat | `MISSING` | `CONFIGURABLE_DEFAULT` | `FR-MVP-KEP-014` mewajibkan notifikasi klinis tanpa menyebut penerima. Usulan bawaan: DPJP aktif dan apoteker. Rute notifikasi tidak mengubah catatan pemberian |
| G-16 | `S18` | Kepemilikan catatan dugaan reaksi obat dan kontraknya dengan `ClinicalManagement` | `PROPOSED` | `NON_BLOCKING_STANDARD` | `RWI-DEC-117` butir (a) sudah menetapkannya sebagai titik sentuh yang dikontrakkan desain |
| G-17 | `S18` | Rekonsiliasi obat saat **transfer** dan saat **pulang** | `MISSING` | `NON_BLOCKING_STANDARD` untuk rekonsiliasi admisi | `RWI-DEC-132` hanya menyebut admisi. Obat pulang sudah menjadi jenis resep `RWI-DEC-046`. Perluasan ke transfer dan pulang adalah pertanyaan scope, tidak menahan rekonsiliasi admisi |
| G-18 | `S18` | Batas waktu rekonsiliasi sejak admisi | `MISSING` | `CONFIGURABLE_DEFAULT` | Pola sama dengan `RWI-RULE-021` |
| G-19 | `S19` | **Penempatan rilis handover shift, sliding scale, dan transfusi** | `MISSING` | **`BLOCKING` untuk `INP-S19`** | Ketiganya `P1` pada PRD-to-MVP-V2, tidak muncul sebagai menu pada PRD v`2.0`, dan blueprint `keperawatan` menundanya (`keperawatan/04-prd-to-mvp.md` bagian 21.6). `RWI-DEC-116` hanya menarik MAR. Belum ada keputusan apakah ketiganya masuk penyelarasan ini. Sliding scale juga membutuhkan pemilik protokol berversi dan sumber hasil gula darah — `DEC-INP-011` |
| G-20 | `S19` | **Kepemilikan alur transfusi di bangsal** | **`CONFLICT`** | **`BLOCKING` untuk transfusi** | `FR-MVP-KEP-019` menaruh workflow transfusi pada Keperawatan Rawat Inap, sedangkan blueprint `bank-darah` (`IN_PROGRESS`) menyatakan penanganan reaksi dan pemantauan pasca-transfusi sebagai lingkupnya. Tidak dapat diselesaikan dari urutan wewenang karena keduanya modul berbeda — `DEC-INP-012` |
| G-21 | `S20` | Nomor `CAP` final Gizi, Hemodialisa, Bank Darah, dan Rehab Medik | `PROPOSED` | `NON_BLOCKING_STANDARD` | `RWI-DEC-113` menugaskannya ke `design-business-module` |

### 14.7 Gap teknis, bukan keputusan bisnis

Butir berikut **bukan** bahan gerbang ini dan tidak boleh dipakai untuk menahan kesiapan requirement:

| Butir | Sifatnya | Rujukan |
|---|---|---|
| Risiko jatuh tersimpan dengan arti berbeda, enum pengkajian tidak cocok, daftar pengkajian keperawatan salah bentuk, isian belum dikaji terkirim sebagai normal | **Teknis — perbaikan keselamatan.** Arahnya sudah dikunci `RWI-DEC-124`, `136`, `BR-RWI-007` | `RWI-FACT-038`; capability map bagian 17 `RLN3-CON-01` s.d. `04` |
| Pemeriksaan ulang alergi saat memakai template belum ada | **Teknis.** Requirement sudah tegas (`RWI-DEC-122` butir 4) dan sumber data alergi terstruktur **sudah ada**: `TrxPatientAllergy` punya `DrugId`, kode/nama/kelompok alergen, tingkat keparahan, dan status | `RLN3-CAP-42`; `BE@df3679c0 ClinicalManagement/Models/TrxPatientAllergy.cs` baris 42–73 |
| Penjaga penulis pada jalur ubah dan final, verifikasi CPPT menerima peran apa pun, tindakan baru pada episode tertutup | **Teknis.** Arah dikunci `RWI-DEC-125`, `128`, `139`, `143`, `144` | `RLN3-CAP-25`, `33` s.d. `36` |
| MAR, rekonsiliasi obat, intake/output, isian terstruktur lima bagian Kajian Umum belum ada di source | **Ketersediaan implementasi**, bukan keputusan yang belum diambil | `RLN3-CAP-05`, `09`, `15` |
| Persetujuan pemilik `MedicalRecordManagement` atas daftar catatan terkunci milik penulis | **Dependency persetujuan modul tetangga**, menahan implementasi bagian itu | `RWI-DEC-142` butir (3) |
| Komponen layout Dokter Rawat Jalan milik pemilik `rawat-jalan` | **Dependency modul tetangga** untuk kesamaan layout | `RLN3-CAP-03` |

### 14.8 Decision Log

Decision ID: `DEC-INP-010`

| Field | Isi |
|---|---|
| Pertanyaan | Dari mana volume **intake obat** dan **intake darah** pada Pengawasan Harian Pasien diambil: diketik perawat, diturunkan dari MAR dan catatan transfusi, atau gabungan dengan aturan tertentu? Termasuk apakah volume cairan pelarut obat intravena dihitung |
| Kemampuan terdampak | `INP-S17` Pengawasan Harian — sub-bagian intake obat dan darah; `INP-S18` MAR sebagai sumber potensial |
| Bukti saat ini | PRD v`2.0` bagian 32 mencantumkan Intake Infus, Oral, NGT, **Darah**, **Obat**. `FR-MVP-KEP-011` menyimpan dosis dan rute aktual, bukan volume. `FR-MVP-KEP-017` menuntut total per shift dari data terstruktur. `RWI-DEC-140` butir (4) melarang duplikasi data terstruktur ke catatan naratif, tetapi tidak mengatur hubungan MAR dengan intake/output |
| Usulan baseline | Intake obat intravena dan darah tidak diketik ulang bila sumbernya sudah tercatat terstruktur; perhitungan volume pelarut mengikuti kebijakan klinis yang disahkan. Usulan ini **belum** kebijakan rumah sakit |
| Dampak | Integrasi MAR–intake/output, integritas data balance cairan, dan keselamatan klinis karena risiko hitung ganda atau terlewat |
| Pemilik | Muhammad Hamzah bersama pemilik klinis/keperawatan yang belum ditunjuk |
| Status | `OPEN` |
| Dampak implementasi/domain | Sub-bagian intake obat dan darah **berhenti**. Bagian lain Pengawasan Harian — tanda vital, nyeri, intake infus/oral/NGT, output, diet, mobilisasi — **boleh berjalan** |

Decision ID: `DEC-INP-011`

| Field | Isi |
|---|---|
| Pertanyaan | Apakah handover shift (`FR-MVP-KEP-015`, `016`), sliding scale (`FR-MVP-KEP-018`), dan transfusi (`FR-MVP-KEP-019`) masuk penyelarasan `PRD-RWI-V2-001` ini seperti MAR, atau tetap ditunda ke irisan berikutnya? Bila masuk: siapa pemilik dan pengesah protokol sliding scale berversi, dan dari mana hasil gula darah diambil |
| Kemampuan terdampak | `INP-S19` seluruhnya |
| Bukti saat ini | Ketiganya `P1` pada PRD-to-MVP-V2 yang tetap berlaku (`RWI-DEC-110`). PRD v`2.0` tidak menampilkannya sebagai menu. Blueprint `keperawatan` yang disetujui menundanya bersama MAR (`04-prd-to-mvp.md` bagian 21.6), lalu `RWI-DEC-116` hanya menarik MAR. Capability map `V2-CAP-09`, `10`, `12`: `Missing` |
| Usulan baseline | Tidak diberikan. Ini keputusan scope dan prioritas milik pemilik |
| Dampak | Batas rilis gelombang keperawatan, model domain protokol berversi, integrasi dengan Bank Darah dan sumber hasil laboratorium/POCT |
| Pemilik | Muhammad Hamzah, Product/Domain; protokol sliding scale memerlukan pemilik klinis |
| Status | `OPEN` |
| Dampak implementasi/domain | `INP-S19` **berhenti**. Tidak menahan `INP-S17`, `S18`, `S20`, maupun `S21` |

Decision ID: `DEC-INP-012`

| Field | Isi |
|---|---|
| Pertanyaan | Siapa pemilik alur transfusi di bangsal — pre-check, tanda vital awal, mulai/berhenti, observasi berkala, dan penanganan reaksi: Keperawatan Rawat Inap sesuai `FR-MVP-KEP-019`, atau modul Bank Darah sesuai blueprint `bank-darah`? |
| Kemampuan terdampak | `INP-S19` bagian transfusi; `INP-S17` intake darah melalui `DEC-INP-010` |
| Bukti saat ini | `FR-MVP-KEP-019` dan `AC-MVP-033` pada PRD-to-MVP-V2; blueprint `bank-darah` `00-interview-decisions.md` menyebut penanganan reaksi transfusi dan pemantauan pasca-transfusi sebagai lingkupnya, dengan pemilik proses yang belum bernama; `RWI-DEC-108` menampilkan menu Bank Darah sebagai "Integrasi belum tersedia" |
| Usulan baseline | Satu pemilik data observasi transfusi, dengan modul lain membaca lewat kontrak; bukan dua catatan paralel |
| Dampak | Kepemilikan data, authorization pemeriksa kedua, keselamatan klinis reaksi transfusi, dan integrasi antarmodul |
| Pemilik | Muhammad Hamzah bersama pemilik modul Bank Darah |
| Status | `OPEN` — hanya relevan bila `DEC-INP-011` memasukkan transfusi ke penyelarasan ini |
| Dampak implementasi/domain | Transfusi **berhenti**. Bila diputuskan milik Bank Darah, Rawat Inap hanya menyediakan permukaan dan konteks episode |

### 14.9 Kesiapan per slice dan kemampuan

| Slice | Kemampuan/isi | Kesiapan | Blocker bisnis | Catatan |
|---|---|---|---|---|
| `INP-S17` | Kajian Umum | **`READY_FOR_DOMAIN_DESIGN`** | — | G-01 usulan isian; G-02 isian wajib `CONFIGURABLE_DEFAULT` |
| `INP-S17` | Resiko Jatuh | **`READY_FOR_DOMAIN_DESIGN`** | — | Mekanisme dikunci; isi klinis gerbang produksi |
| `INP-S17` | Monitoring Nyeri | **`READY_FOR_DOMAIN_DESIGN`** | — | G-03, G-04 |
| `INP-S17` | Assesment Edukasi | **`READY_FOR_DOMAIN_DESIGN`** | — | G-01, G-05 |
| `INP-S17` | Pengawasan Harian Pasien | **`PARTIALLY_READY`** | `DEC-INP-010` | Boleh: tanda vital, nyeri, intake infus/oral/NGT, output, balance dari data terstruktur, diet, mobilisasi. Berhenti: intake obat dan darah |
| `INP-S17` | Evaluasi Awal (MPP) | **`READY_FOR_DOMAIN_DESIGN`** | — | G-07, G-09, G-11 |
| `INP-S17` | Perencanaan Pulang | **`READY_FOR_DOMAIN_DESIGN`** | — | G-05, G-10 |
| `INP-S17` | Progres Pengkajian | **`READY_FOR_DOMAIN_DESIGN`** | — | `RWI-DEC-119`, `120` |
| `INP-S18` | MAR | **`READY_FOR_DOMAIN_DESIGN`** | — | G-12 s.d. G-16; high-alert gerbang produksi |
| `INP-S18` | Rekonsiliasi obat saat admisi | **`READY_FOR_DOMAIN_DESIGN`** | — | G-17 scope transfer/pulang non-blocking; G-18 |
| `INP-S19` | Handover shift, sliding scale | **`BUSINESS_DECISION_REQUIRED`** | `DEC-INP-011` | — |
| `INP-S19` | Transfusi | **`BUSINESS_DECISION_REQUIRED`** | `DEC-INP-011`, `DEC-INP-012` | — |
| `INP-S20` | Laboratorium dan Radiologi | **`READY_FOR_DOMAIN_DESIGN`** | — | Sudah siap sejak revision `1.3` sebagai `CAP-015` |
| `INP-S20` | Gizi, Hemodialisa, Bank Darah, Rehab Medik | **`READY_FOR_DOMAIN_DESIGN`** untuk permukaan "Integrasi belum tersedia" saja | — | Backend **`DEFERRED`** sampai modul pemiliknya tersedia, sesuai `RWI-DEC-108` |
| `INP-S21` | Dokumentasi klinis dan ruang kerja dokter V2 | **`READY_FOR_DOMAIN_DESIGN`** | — | Dependency persetujuan `MedicalRecordManagement` menahan implementasi daftar catatan terkunci, bukan desain |

**Hasil turunan:**

- `INP-S17` **`PARTIALLY_READY`**; `INP-S18` **`READY_FOR_DOMAIN_DESIGN`**; `INP-S19`
  **`BUSINESS_DECISION_REQUIRED`**; `INP-S20` **`READY_FOR_DOMAIN_DESIGN`**; `INP-S21`
  **`READY_FOR_DOMAIN_DESIGN`**.
- **`RLN-04` tertutup:** tidak ada kemampuan penunjang yang yatim (`RWI-DEC-113`).
- **`RLN-07` tertutup:** ketujuh isi Pengkajian Pasien dan progresnya dinilai; hanya sub-bagian
  intake obat dan darah yang berhenti.
- Overall Rawat Inap tetap **`PARTIALLY_READY`**.

### 14.10 Apa yang boleh berjalan dan apa yang harus berhenti

**Boleh berjalan ke amandemen desain `RLN-PH-06`:**

- `INP-S17` kecuali sub-bagian intake obat dan darah;
- `INP-S18` MAR dan rekonsiliasi obat saat admisi;
- `INP-S20` seluruhnya, dengan empat layanan non-laboratorium/radiologi sebatas permukaan;
- `INP-S21` seluruhnya.

**Harus berhenti:**

- sub-bagian intake obat dan darah Pengawasan Harian, sampai `DEC-INP-010` diputuskan;
- handover shift, sliding scale, dan transfusi, sampai `DEC-INP-011` diputuskan; transfusi juga
  menunggu `DEC-INP-012`;
- **pemakaian untuk pasien sungguhan** atas Resiko Jatuh, isian wajib pengkajian, checklist Evaluasi
  Awal, dan daftar high-alert, sampai isi klinisnya disahkan. Ini menahan rilis, **bukan** desain.

**Dependency di antara keduanya:** `DEC-INP-010` bergantung pada `DEC-INP-012` untuk intake darah.
MAR tidak bergantung pada `DEC-INP-010`; yang bergantung justru intake obat pada MAR.

### 14.11 Keputusan pemilik yang dibutuhkan

| Prioritas | Keputusan | Memblokir |
|---|---|---|
| 1 | `DEC-INP-011` — penempatan rilis handover shift, sliding scale, transfusi | `INP-S19` |
| 2 | `DEC-INP-010` — sumber intake obat dan darah | Sub-bagian Pengawasan Harian |
| 3 | `DEC-INP-012` — pemilik alur transfusi di bangsal | Transfusi, bila masuk |
| Non-blocking | G-17 rekonsiliasi saat transfer dan pulang; G-05 tanda tangan penerima edukasi dan rencana pulang; G-09 frekuensi Evaluasi Awal | — |
| Gerbang produksi | `RWI-OQ-056`, `RWI-OQ-057`, isi high-alert, checklist Evaluasi Awal, penunjukan pemilik klinis | Rilis |

### 14.12 Handoff

| Field | Nilai |
|---|---|
| `capability_scope` | `INP-S17` (kecuali intake obat dan darah), `INP-S18`, `INP-S20`, `INP-S21` |
| `requirement_readiness` | `S17` `PARTIALLY_READY`; `S18`, `S20`, `S21` `READY_FOR_DOMAIN_DESIGN`; `S19` `BUSINESS_DECISION_REQUIRED` |
| `requirement_evidence_status` | Mayoritas `CONFIRMED`; 21 butir gap: 5 `PROPOSED`, 15 `MISSING`, 1 `CONFLICT` |
| `domain_architecture_readiness` | `DOMAIN_ARCHITECTURE_NOT_RUN` untuk slice yang siap, dan tidak diperlukan: batas konteks dan kepemilikan datanya sudah ditetapkan `RWI-DEC-081`, `113`, `117`, `118`, `132`, `142`, `144`. `RLN-PH-05` baru relevan untuk transfusi setelah `DEC-INP-012` |
| `decision_ids` | `DEC-INP-010`, `DEC-INP-011`, `DEC-INP-012` — ketiganya `OPEN` |
| `dependency_ids` | Persetujuan `MedicalRecordManagement` (`RWI-DEC-142`); komponen `rawat-jalan` (`RLN3-CAP-03`); modul Bank Darah (`DEC-INP-012`); gerbang produksi isi klinis |
| `next_owner` | `design-business-module` untuk amandemen `dokter-rawat-inap` dan `keperawatan`, fase `RLN-PH-06`; `grill-me` untuk `DEC-INP-010` s.d. `012` |
| `required_boundary` | Dokumentasi keperawatan dan dokter tetap milik `ClinicalManagement`; MAR dan rekonsiliasi milik `PharmacyManagement`; metadata keutuhan milik `MedicalRecordManagement`. Desain **MUST NOT** membuat tabel intake obat/darah maupun tabel transfusi sebelum keputusannya turun |
| `expected_output` | Amandemen blueprint kedua sub-modul revision `7` untuk slice yang siap, dengan sub-bagian dan slice yang berhenti ditandai eksplisit |

---

## 15. Penutupan keputusan penyelarasan `PRD-RWI-V2-001` — revision `1.6`

Bagian ini adalah **hasil kanonis terbaru** untuk `INP-S17` dan `INP-S19`. Ia menggantikan status pada
14.4 (kolom `INP-S19` dan baris intake obat/darah kolom `INP-S17`), 14.6 butir `G-08`, `G-19`, `G-20`,
serta 14.8 s.d. 14.12. Hasil `INP-S18`, `INP-S20`, dan `INP-S21` tetap sebagaimana bagian 14.

### 15.1 Kenapa penutupan ini dijalankan

Revision `1.5` menahan dua hal dengan tiga Decision ID:

- sub-bagian **intake obat dan darah** Pengawasan Harian, oleh `DEC-INP-010`;
- **handover shift, sliding scale, dan transfusi**, oleh `DEC-INP-011`, dengan transfusi juga menunggu
  `DEC-INP-012`.

Pemilik menjawab ketiganya pada Amendment Pass penutupan gate `RLN-PH-04`, 15 September 2026, lima
pertanyaan, seluruhnya pilihan A. Gate ini menyerap jawabannya, menilai ulang kemampuan yang terbuka
oleh jawaban itu, lalu menetapkan kesiapan baru. Gate **tidak** menambah keputusan atas nama pemilik.

### 15.2 Scope penilaian

| Slice | Kemampuan atau isi | Cara dinilai | Keputusan yang mengikat |
|---|---|---|---|
| `INP-S17` | Pengawasan Harian — sub-bagian **intake obat dan intake darah** | Penuh, 18 dimensi | `RWI-DEC-149` |
| `INP-S17` | Pengawasan Harian — **pencatatan gula darah** | Sebatas peran barunya sebagai satu-satunya sumber dosis sliding scale | `RWI-DEC-148` |
| `INP-S19` | **Sliding scale** — template protokol, order per pasien, pelaksanaan | Penuh, 18 dimensi | `RWI-DEC-145` s.d. `RWI-DEC-148` |
| `INP-S19` | Handover shift (`FR-MVP-KEP-015`, `016`) | **`DEFERRED` — tidak dinilai** | `RWI-DEC-145` butir (4) |
| `INP-S19` | Transfusi (`FR-MVP-KEP-019`) | **`DEFERRED` — tidak dinilai** | `RWI-DEC-145` butir (4) dan (5) |
| `INP-S18` | MAR | Hanya dampak silang: MAR mendapat dua pengguna baru, yaitu tautan entri intake obat dan dosis sliding scale | `RWI-DEC-145` butir (2), `RWI-DEC-149` butir (3) dan (4) |

**Arti `DEFERRED` di sini.** Handover shift dan transfusi **tidak terblokir** oleh keputusan yang belum
diambil. Pemilik sudah memutuskan mengeluarkan keduanya dari batas rilis penyelarasan ini, tanpa mencabut
requirement-nya. Pola ini sama dengan `CAP-016` pada bagian 13.9. Keduanya dinilai ulang saat dijadwalkan.

**Yang tidak dinilai:** slice `INP-S01` s.d. `INP-S16`, serta isi `INP-S17`, `S18`, `S20`, dan `S21` di
luar baris di atas.

### 15.3 Bukti yang dipakai

| Bukti | Revision / hash | Wewenang dan penggunaannya |
|---|---|---|
| [`00-interview-decisions.md`](../00-interview-decisions.md) | revision `21`, SHA-256 `1c55c80a…2d45102a`, keputusan terakhir `RWI-DEC-149`, acceptance criteria terakhir `RWI-AC-231` | Keputusan pemilik yang dikonfirmasi — wewenang tertinggi untuk bagian ini |
| `PRD-to-MVP-Rawat-Inap-V2` v`1.0.0` bagian 13.2, 13.3, dan 16 (`FR-MVP-KEP-009` s.d. `019`, `AC-MVP-031`, `AC-MVP-032`) | SHA-256 `9804b968…3ce141`, diperiksa ulang tidak berubah | Requirement yang tetap berlaku lewat `RWI-DEC-110` |
| `PRD-RWI-V2-001` v`2.0` bagian 32 dan `BR-RWI-013` | SHA-256 `2b3b2f29…0a679f`, diperiksa ulang tidak berubah | Daftar isi Pengawasan Harian; aturan "order ≠ pelaksanaan ≠ hasil ≠ tagihan" |
| [`01-existing-capability-map.md`](../01-existing-capability-map.md) | revision `1.4`, SHA-256 `337a10f0…d22daa543a`, tidak berubah | "Apa yang ada sekarang": `V2-CAP-10` sliding scale `Missing`, `RLN3-CAP-09` Pengawasan Harian `Missing`, `RLN3-CAP-15` MAR `Missing` |
| Frontend V1 `13c3a96b` `catatan-keperawatan/sliding-scale/` | dibaca pada Amendment Pass, dikutip `RWI-DEC-145` | Bukti legacy: V1 hanya catatan bebas tanggal, GDS, insulin, insulin drip, dan catatan, tanpa protokol |

> **Batas yang dijaga.** Kelima keputusan lahir dari opsi yang disusun agent, lalu dipilih pemilik
> secara tertulis, sehingga berstatus `CONFIRMED`. Usulan pada tabel 15.9 **tidak** dipilih pemilik dan
> tetap `PROPOSED` atau `MISSING`.

### 15.4 Keputusan yang turun

| ID | Isi singkat | Status |
|---|---|---|
| `RWI-DEC-145` | Sliding scale masuk batas rilis bersama MAR dan rekonsiliasi obat. Handover shift dan transfusi ditunda ke irisan berikutnya, tanpa dicabut | `approved` |
| `RWI-DEC-146` | Protokol sliding scale dua lapis: template standar berversi yang disahkan terpisah dari pengubahnya, lalu order per pasien yang boleh disesuaikan dengan alasan wajib | `approved` |
| `RWI-DEC-147` | Template dan order sliding scale milik `PharmacyManagement`, bersama resep dan MAR | `approved` |
| `RWI-DEC-148` | Dosis insulin hanya dihitung dari GDS bangsal, yang tersimpan satu kali sebagai gula darah Pengawasan Harian. Hasil laboratorium hanya informasi | `approved` |
| `RWI-DEC-149` | Intake obat dan darah dicatat perawat sebagai volume aktual, termasuk pelarut. Entri obat tertaut ke satu dosis MAR `Administered`; MAR tidak mendapat isian volume | `approved` |
| `RWI-OQ-091`, `092`, `094`, `095`, `096` | Pertanyaan pass | `TERTUTUP` |
| `RWI-OQ-093` | Pemilik alur transfusi di bangsal | `DITUNDA` bersama transfusi |
| `DEC-INP-010` | Sumber intake obat dan darah | **`CLOSED`** oleh `RWI-DEC-149` |
| `DEC-INP-011` | Penempatan rilis handover shift, sliding scale, transfusi | **`CLOSED`** oleh `RWI-DEC-145`, dirinci `RWI-DEC-146` s.d. `148` |
| `DEC-INP-012` | Pemilik alur transfusi di bangsal | **`DEFERRED`** bersama transfusi |

### 15.5 Hasil penilaian 18 dimensi

Kode: `C` = `CONFIRMED`, `P` = `PROPOSED`, `M` = `MISSING`, `X` = `CONFLICT`. Nomor `G-##` merujuk tabel
15.8 dan 15.9.

| ID | Dimensi | `INP-S17` Intake obat dan darah | `INP-S19` Sliding scale |
|---:|---|---|---|
| 01 | Tujuan | `C` balance cairan memuat obat dan darah, PRD bagian 32, `FR-MVP-KEP-017` | `C` dosis insulin mengikuti protokol aktif, `FR-MVP-KEP-018`, `RWI-DEC-145` (1) |
| 02 | Aktor | `C` perawat pencatat dari akun login, `RWI-DEC-149` (1) | `C` dokter pemesan `RWI-DEC-146` (3); perawat pelaksana; pengubah dan pengesah template terpisah `RWI-DEC-146` (1), `147` (4); apoteker membaca `RWI-DEC-147`. Pengesah isi klinis belum ditunjuk — gerbang produksi, bukan desain |
| 03 | Pemicu/prasyarat | `C` obat: dosis MAR sudah `Administered` `RWI-AC-230`; darah: tanpa prasyarat transfusi `RWI-DEC-149` (5) | `C` order aktif dari versi template yang disahkan `RWI-AC-219`, `222`; GDS bangsal tercatat `RWI-DEC-148`; `M` jadwal pemeriksaan G-22 |
| 04 | Alur utama | `C` contoh `RWI-DEC-149` | `C` contoh `RWI-DEC-145`, `146`, `148` |
| 05 | Alternatif/exception | `C` entri kedua, tanpa tautan, dosis `Held`, koreksi volume `RWI-DEC-149` (a)–(d); `M` koreksi dosis MAR yang sudah tertaut G-26 | `C` tanpa order aktif, template draft, penyesuaian tanpa alasan, rentang bertumpuk, order dihentikan, hasil lab dipilih, kontrak baca gagal, dosis berbeda sebagai pengecualian, cek ganda high-alert |
| 06 | Data minimum | `C` sumber, volume, satuan, waktu, pelaksana, tautan dosis `RWI-DEC-149` (1), (3) | `C` template: versi, rentang, dosis; order: versi template, penyesuaian, alasan, dokter, waktu; pelaksanaan: rujukan GDS, aturan yang cocok, dosis, pengecualian, versi order `RWI-DEC-146` (2), (4); `M` satuan GDS G-25 |
| 07 | Aturan/validation | `C` volume aktual termasuk pelarut; satu dosis satu entri `RWI-AC-229`; `M` dosis bervolume tanpa entri G-27 | `C` rentang tidak bertumpuk dan tidak berlubang `RWI-DEC-146` (1); dosis hanya dari GDS bangsal `RWI-AC-228`; `P` cakupan rentang terbuka G-23 |
| 08 | Status/lifecycle | `C` entri aktif dan terkoreksi, nilai lama tetap `FR-MVP-KEP-017`, `RWI-AC-231` | `C` template draft → disahkan; order aktif → dihentikan; penyesuaian menjadi versi order baru `RWI-DEC-146` (6); dosis memakai enam status MAR; `M` letak dosis sliding scale dalam lifecycle MAR G-22 |
| 09 | Peran/authorization | `C` mengikuti kewenangan Pengawasan Harian `RWI-DEC-100` | `C` pemesan mengikuti penulis resep `RWI-DEC-099`, `128`; pemisahan pengesahan `RWI-DEC-136`, `147` (4); cek ganda pengguna kedua `FR-MVP-KEP-012` |
| 10 | Dependency antarmodul | `C` entri milik `ClinicalManagement` merujuk dosis MAR milik `PharmacyManagement` `RWI-DEC-149` konsekuensi (2) | `C` `PharmacyManagement` pemilik `RWI-DEC-147`; `ClinicalManagement` pemilik gula darah `RWI-DEC-148`; MAR `INP-S18` |
| 11 | Integrasi | `C` internal saja; tanpa Bank Darah `RWI-DEC-149` (5) | `C` internal: baca gula darah `ClinicalManagement`, tampil hasil lab sebagai informasi `RWI-DEC-148` (3). Tanpa integrasi glukometer — angka diketik perawat |
| 12 | Hasil akhir | `C` total per shift dan rolling 24 jam dari lima sumber `RWI-AC-231` | `C` dosis tercatat satu kali di MAR dengan rujukan GDS dan aturan `RWI-AC-220`, `227` |
| 13 | Pembatalan/koreksi | `C` koreksi mempertahankan nilai lama; `M` G-26 | `C` penghentian order `RWI-AC-226`; versi order baru; koreksi GDS mempertahankan nilai asli `RWI-DEC-148` (b); koreksi pemberian mengikuti `RWI-DEC-116` (c) |
| 14 | Audit/histori | `C` pelaksana, waktu, nilai lama | `C` versi template, versi order, alasan penyesuaian, rujukan GDS `RWI-DEC-146` (2), `148` (4) |
| 15 | Notifikasi | Tidak material — pencatatan volume tidak diminta memberi tahu siapa pun | `M` instruksi "lapor dokter" pada rentang G-24 |
| 16 | Billing/charge | Tidak material — pencatatan volume bukan kejadian tagihan; pelaksanaan bukan tagihan `BR-RWI-013` | `C` pemberian insulin bukan tagihan `BR-RWI-013`; `M` tagihan pemeriksaan GDS bangsal G-28 |
| 17 | Keselamatan klinis | `C` hitung ganda dicegah satu dosis satu entri; `M` entri terlewat G-27 | `C` satu sumber dosis, validasi rentang, cek ganda high-alert; isi template dan daftar high-alert menjadi gerbang produksi; `M` satuan GDS G-25 |
| 18 | Pelaporan/traceability | `C` entri obat tertelusur ke dosis MAR | `C` dosis tertelusur ke angka dan waktu GDS `RWI-DEC-148` (4); `M` catatan sliding scale V1 G-29 |

### 15.6 Aturan yang dapat diuji

Seluruh contoh memakai data samaran.

**Sliding scale**

1. Template "Sliding Scale Insulin Dewasa v2" masih draft. dr. Rina memilihnya untuk Budi → **ditolak**.
   Setelah v2 disahkan oleh pengguna lain yang bukan pengubah terakhirnya, dr. Rina dapat memesannya
   (`RWI-AC-222`).
2. v2 berisi GDS 150–199 → 2 unit, 200–249 → 4 unit, 250–299 → 6 unit, ≥ 300 → 8 unit. dr. Rina membuat
   dosis setiap rentang separuh dengan alasan "pasien sensitif insulin". Order Budi menjadi 1, 2, 3, dan 4
   unit. Penyesuaian yang sama **tanpa** alasan ditolak (`RWI-AC-223`).
3. dr. Rina mengubah rentang menjadi 200–260 dan 250–299. Nilai 255 jatuh ke dua rentang → order
   **ditolak** saat disimpan (`RWI-AC-223`).
4. Pukul 06.00 hasil GDS laboratorium Budi 190. Pukul 11.00 perawat mengisi GDS glukometer 280 dari layar
   sliding scale. Angka 280 tersimpan **satu kali** sebagai gula darah Pengawasan Harian pukul 11.00,
   pelaksanaan merujuknya, dan sistem menampilkan 3 unit. Angka 190 tampil bertanda "informasi" dan
   **tidak dapat dipilih** sebagai dasar dosis (`RWI-AC-227`, `228`).
5. Perawat mencatat pemberian 3 unit. Riwayat MAR Budi menampilkan pemberian itu **tepat satu kali**,
   bersama rujukan GDS 280 dan aturan 250–299 (`RWI-AC-220`).
6. Rabu template v3 disahkan dengan rentang berbeda. Order Budi tetap memakai v2 beserta penyesuaiannya,
   dan pelaksanaan Selasa tetap terbaca dengan v2 (`RWI-AC-224`).
7. Pukul 15.00 dr. Rina menghentikan order. Pukul 17.00 perawat mencoba mencatat pelaksanaan dari order
   itu → **ditolak** dengan keterangan order sudah dihentikan (`RWI-AC-226`).
8. Tidak ada satu pun tabel sliding scale di `ClinicalManagement` maupun `InPatientManagement`, dan tidak
   ada menu, tabel, atau endpoint handover shift dan transfusi pada penyelarasan ini (`RWI-AC-221`, `225`).

**Intake obat dan darah**

9. Pukul 08.00 Budi mendapat Ceftriaxone 1 g dalam NaCl 100 ml, dan dosisnya `Administered` di MAR.
   Perawat mencatat intake "Obat — 100 ml" tertaut ke dosis itu. Perawat lain mencoba mencatat entri
   kedua untuk dosis yang sama → **ditolak**. Entri "Obat" tanpa tautan dosis → **ditolak** (`RWI-AC-229`).
10. Dosis Ceftriaxone 20.00 berstatus `Held`. Dosis itu **tidak dapat** ditautkan ke entri intake
    (`RWI-AC-230`).
11. Transfusi PRC 250 ml berhenti di 200 ml karena Budi menggigil. Perawat mencatat intake "Darah —
    200 ml" tanpa tautan ke catatan transfusi, karena transfusi ditunda.
12. Shift pagi Budi: Infus 500 ml, Oral 200 ml, Obat 100 ml, Darah 200 ml → total intake **1.000 ml**.
    Entri Obat ternyata hanya 80 ml dan dikoreksi → total dihitung ulang menjadi **980 ml**, dan nilai
    100 ml tetap terbaca pada riwayat (`RWI-AC-231`).

### 15.7 Butir `CONFIRMED` baru yang menopang kesiapan

Melanjutkan penomoran tabel 14.5.

| No | Butir | Bukti |
|---:|---|---|
| 12 | Sliding scale masuk batas rilis yang sama dengan MAR; label `P1` hanya asal requirement | `RWI-DEC-145` (1) |
| 13 | Dua lapis data berversi: template standar dan order per pasien. Pelaksanaan selalu memakai order, bukan template langsung | `RWI-DEC-146` (1), (2), (4) |
| 14 | Template, order, dan pelaksanaan milik `PharmacyManagement`; tabel baru di sana wajib dicatat eksplisit pada amandemen desain | `RWI-DEC-147`; `RWI-DEC-062` |
| 15 | Satu sumber dosis dan satu tempat simpan: GDS bangsal di Pengawasan Harian, dirujuk oleh pelaksanaan; hasil laboratorium tidak pernah menjadi sumber dosis | `RWI-DEC-148` |
| 16 | Pengawasan Harian wajib menyediakan pencatatan gula darah pada rilis yang sama dengan sliding scale | `RWI-DEC-148` konsekuensi (2) |
| 17 | Intake obat dan darah adalah entri intake terstruktur, volume aktual termasuk pelarut; entri obat tertaut satu dosis MAR `Administered`; MAR tidak mendapat isian volume | `RWI-DEC-149` (1)–(4) |
| 18 | Handover shift dan transfusi tetap `P1` dan tidak dicabut, tetapi tanpa menu, tabel, maupun endpoint pada penyelarasan ini | `RWI-DEC-145` (4); `RWI-AC-221` |

### 15.8 Status gap lama setelah keputusan

| ID | Semula (revision `1.5`) | Kini | Alasannya |
|---|---|---|---|
| `G-08` | `MISSING` / `BLOCKING` untuk intake obat dan darah | **`CONFIRMED`** — tertutup | `RWI-DEC-149` menjawab sumber, volume pelarut, dan hubungan dengan MAR |
| `G-19` | `MISSING` / `BLOCKING` untuk `INP-S19` | **`CONFIRMED`** — tertutup | `RWI-DEC-145` menetapkan penempatan rilis; `RWI-DEC-146` s.d. `148` menjawab pemilik protokol, pengesah, dan sumber gula darah |
| `G-20` | `CONFLICT` / `BLOCKING` untuk transfusi | **`CONFLICT` tetap, tetapi transfusi `DEFERRED`** | Pertentangan `FR-MVP-KEP-019` dengan blueprint `bank-darah` belum diselesaikan. Karena transfusi keluar dari batas rilis, butir ini **tidak menahan slice aktif mana pun**. Dibuka kembali lewat `RWI-OQ-093` saat transfusi dijadwalkan |

Butir `G-01` s.d. `G-07`, `G-09` s.d. `G-18`, dan `G-21` tidak berubah.

### 15.9 Gap baru

Seluruh gap baru **tidak memblokir**. Setiap usulan tetap usulan sampai pemilik memilihnya.

| ID | Slice | Butir | Status | Dampak | Alasan dan usulan |
|---|---|---|---|---|---|
| G-22 | `S19` | **Jadwal pemeriksaan GDS pada order, dan letak dosis sliding scale dalam lifecycle MAR** | `MISSING` | `NON_BLOCKING_STANDARD` | `FR-MVP-KEP-010` hanya mengenal dosis terjadwal dengan enam status, `FR-MVP-KEP-013` mengenal PRN, dan `RWI-DEC-145` s.d. `148` tidak menyebut jadwal. **Usulan:** order memuat jadwal pemeriksaan yang ditulis dokter, seperti frekuensi resep. Setiap jadwal menjadi dosis terjadwal MAR yang dosisnya baru ditentukan saat GDS tercatat. Hasil pada rentang tanpa insulin dicatat `Held` dengan alasan dari aturan skala. **Contoh:** order "cek GDS 06.00, 11.00, 17.00" menghasilkan tiga dosis `Due`; pukul 17.00 GDS 130 jatuh pada "< 150 → 0 unit", sehingga dosis itu `Held` beralasan "GDS di bawah rentang pemberian". Usulan ini tidak menambah status dan tidak mengubah kepemilikan. **Disarankan dikonfirmasi pemilik saat approval `RLN-PH-06`** |
| G-23 | `S19` | Cakupan rentang template | `PROPOSED` | `NON_BLOCKING_STANDARD` | Turunan `RWI-DEC-146` (1) "setiap hasil jatuh ke tepat satu rentang": template wajib mencakup seluruh kemungkinan nilai GDS, termasuk rentang terbuka di bawah dan di atas. **Contoh:** v2 pada contoh keputusan mulai dari 150, sehingga GDS 120 tidak jatuh ke rentang mana pun; versi itu belum boleh disahkan sampai ada rentang "< 150". Isi tindak lanjut hipoglikemia, misalnya GDS < 70, adalah isi klinis template dan ikut gerbang produksi |
| G-24 | `S19` | Instruksi tindak lanjut pada rentang, misalnya "≥ 300 → 8 unit dan lapor dokter" | `MISSING` | `CONFIGURABLE_DEFAULT` | Belum ditetapkan apakah instruksi itu hanya tampil ke perawat atau juga mengirim notifikasi ke dokter. Rute notifikasi tidak mengubah catatan pelaksanaan maupun dosis, pola yang sama dengan `G-15`. **Usulan bawaan:** instruksi tersimpan sebagai isi rentang dan tampil di layar pelaksanaan; notifikasi aktif mengikuti keputusan penerima pada `G-15` |
| G-25 | `S19`, `S17` | Satuan GDS: mg/dL atau mmol/L | `MISSING` | `NON_BLOCKING_STANDARD` | Pilihan satuan tidak mengubah struktur, tetapi salah satuan berbahaya: 280 mg/dL setara sekitar 15,6 mmol/L. **Usulan:** satuan disimpan eksplisit bersama nilai pada catatan gula darah dan pada rentang template; pencocokan skala **menolak** bila satuannya berbeda, bukan mengonversi diam-diam |
| G-26 | `S17`, `S18` | Koreksi dosis MAR yang sudah tertaut entri intake | `MISSING` | `NON_BLOCKING_STANDARD` | `RWI-DEC-149` mengatur tautan saat dibuat, dan `RWI-DEC-116` (c) mengatur koreksi MAR tanpa menimpa. Belum ada yang mengatur nasib entri intake bila dosisnya dikoreksi kemudian. **Usulan yang tidak mengubah data diam-diam:** entri intake tidak dihapus atau diubah otomatis; entri itu ditandai "dosis tertaut dikoreksi", lalu perawat mengoreksinya lewat koreksi intake beralasan. **Contoh:** dosis Ceftriaxone 08.00 dikoreksi karena hanya separuh yang masuk; entri "Obat — 100 ml" tetap terhitung sampai perawat mengoreksinya menjadi 50 ml. Bila pemilik ingin total langsung mengeluarkan entri itu, yang berubah hanya aturan hitung, bukan struktur. **Disarankan dikonfirmasi pemilik saat approval `RLN-PH-06`** |
| G-27 | `S17` | Dosis `Administered` bervolume yang belum punya entri intake | `MISSING` | `NON_BLOCKING_STANDARD` | `RWI-DEC-149` mencegah hitung ganda, tetapi tidak mewajibkan entri untuk setiap dosis, sehingga risiko "terlewat" pada `DEC-INP-010` masih ada. **Usulan:** entri tetap tidak diwajibkan, sesuai keputusan. Desain boleh menampilkan penanda informatif yang tidak menahan apa pun. **Contoh:** hari ini Budi mendapat tiga dosis intravena `Administered`, dua sudah punya entri intake; Pengawasan Harian menampilkan "1 dosis intravena belum dicatat volumenya" |
| G-28 | `S19`, `S17` | Tagihan pemeriksaan GDS bangsal, misalnya strip glukometer | `MISSING` | `NON_BLOCKING_STANDARD` | Pengawasan Harian tidak menagih (14.4 dimensi 16), dan pelaksanaan bukan tagihan `BR-RWI-013`. Bila rumah sakit menagih pemeriksaan GDS, jalurnya tindakan keperawatan `CAP-014` yang sudah siap sejak revision `1.4`, atau pemakaian alat `CAP-016` yang `DEFERRED` — **bukan** sliding scale. Struktur sliding scale tidak berubah. Pilihannya dikonfirmasi pemilik Billing sebelum rilis |
| G-29 | `S19` | Nasib catatan sliding scale V1 yang berupa catatan bebas tanpa protokol | `MISSING` | `NON_BLOCKING_STANDARD` | `RWI-DEC-145` konsekuensi (3) memasukkannya ke cakupan migrasi `OPEN-MVP-010`, sedangkan `RWI-AC-219` menolak pelaksanaan tanpa protokol. **Usulan:** catatan V1 dibawa sebagai riwayat baca-saja, bukan pelaksanaan sliding scale V2 dan bukan dosis MAR. Keputusan cutover tetap milik Product + Data Owner |

**Rekap status gap revision `1.6`:** 29 butir tercatat (`G-01` s.d. `G-29`). 2 kini `CONFIRMED`
(`G-08`, `G-19`); 6 `PROPOSED`; 20 `MISSING`; 1 `CONFLICT` (`G-20`, transfusi `DEFERRED`). **Nol butir
`BLOCKING` untuk slice aktif.**

### 15.10 Gap teknis, bukan keputusan bisnis

| Butir | Sifatnya | Rujukan |
|---|---|---|
| Sliding scale, template, order, Pengawasan Harian termasuk gula darah dan intake/output, serta MAR belum punya model di source | **Ketersediaan implementasi** | `V2-CAP-10`, `RLN3-CAP-09`, `RLN3-CAP-15` — ketiganya `Missing` |
| Kolom pemilik `V2-CAP-10` pada capability map masih `ClinicalManagement` | **Catatan basi pada peta "apa yang ada"**. Untuk "apa yang harus dibangun", `RWI-DEC-147` menang: `PharmacyManagement`. Status `Missing` tetap benar. Diperbarui pada impact scan berikutnya | `01-existing-capability-map.md` bagian 16 |
| Dua kontrak rujukan lintas modul: `PharmacyManagement` membaca gula darah `ClinicalManagement`, dan entri intake `ClinicalManagement` merujuk dosis MAR `PharmacyManagement` beserta aturan satu dosis satu entri | **Pekerjaan desain kontrak**, milik `design-business-module` | `RWI-DEC-148` konsekuensi (1); `RWI-DEC-149` konsekuensi (2) |
| Baris kepemilikan baru `PharmacyManagement` pada `02-module-map.md`, dan pencatatan eksplisit tabel baru di modul itu | **Pekerjaan desain** | `RWI-DEC-147` konsekuensi (1), (3) |
| `keperawatan/04-prd-to-mvp.md` bagian 21.6 masih mencatat sliding scale tertunda | **Dokumen basi**, diperbarui pada amandemen `RLN-PH-06`. Handover shift dan transfusi tetap tertunda di sana | `RWI-DEC-145` konsekuensi (2) |
| Persetujuan pemilik `MedicalRecordManagement` atas daftar catatan terkunci milik penulis | **Dependency persetujuan modul tetangga**, tidak berubah dari 14.7. Menahan implementasi bagian itu, bukan desain | `RWI-DEC-142` butir (3); Yoga Aji Pratama |

### 15.11 Decision Log — status akhir

Decision ID: `DEC-INP-010`

| Field | Isi |
|---|---|
| Pertanyaan | Tetap seperti 14.8 |
| Jawaban | Volume aktual dicatat perawat sebagai entri intake terstruktur bersumber "Obat" atau "Darah", termasuk pelarut. Entri obat tertaut ke satu dosis MAR `Administered`, satu dosis satu entri. MAR tidak mendapat isian volume. Intake darah tanpa tautan transfusi |
| Ditutup oleh | `RWI-DEC-149`; `RWI-OQ-092` |
| Status | **`CLOSED`** |
| Dampak implementasi/domain | Sub-bagian intake obat dan darah **boleh dirancang**. Kontrak rujukan ke dosis MAR wajib dirancang. Gap tersisa `G-26`, `G-27` tidak memblokir |

Decision ID: `DEC-INP-011`

| Field | Isi |
|---|---|
| Pertanyaan | Tetap seperti 14.8 |
| Jawaban | Sliding scale masuk bersama MAR; handover shift dan transfusi ditunda. Protokol dua lapis berversi, milik `PharmacyManagement`, disahkan pemilik klinis. Sumber dosis hanya GDS bangsal di Pengawasan Harian |
| Ditutup oleh | `RWI-DEC-145` s.d. `RWI-DEC-148`; `RWI-OQ-091`, `094`, `095`, `096` |
| Status | **`CLOSED`** |
| Dampak implementasi/domain | Sliding scale **boleh dirancang** bersama MAR. Handover shift dan transfusi **`DEFERRED`**. Pemakaian untuk pasien sungguhan menunggu pengesahan isi template |

Decision ID: `DEC-INP-012`

| Field | Isi |
|---|---|
| Pertanyaan | Tetap seperti 14.8 |
| Bukti saat ini | Tetap seperti 14.8; `G-20` masih `CONFLICT` |
| Pemilik | Muhammad Hamzah bersama pemilik modul Bank Darah |
| Status | **`DEFERRED`** oleh `RWI-DEC-145` butir (5); alias `RWI-OQ-093` `DITUNDA` |
| Pemicu dibuka kembali | Transfusi dijadwalkan masuk irisan rilis. Saat itu juga dibahas cara intake darah diturunkan dari catatan transfusi, `RWI-DEC-149` butir (5) |
| Dampak implementasi/domain | Tidak menahan slice aktif. Desain **MUST NOT** membuat tabel, menu, atau endpoint transfusi |

### 15.12 Kesiapan per slice dan kemampuan

Baris yang berubah dari 14.9 dicetak tebal.

| Slice | Kemampuan/isi | Kesiapan | Blocker bisnis | Catatan |
|---|---|---|---|---|
| `INP-S17` | Kajian Umum, Resiko Jatuh, Monitoring Nyeri, Assesment Edukasi, Evaluasi Awal (MPP), Perencanaan Pulang, Progres Pengkajian | `READY_FOR_DOMAIN_DESIGN` | — | Tidak berubah dari 14.9 |
| **`INP-S17`** | **Pengawasan Harian Pasien, termasuk intake obat dan darah** | **`READY_FOR_DOMAIN_DESIGN`** | — | Naik dari `PARTIALLY_READY`. Pencatatan gula darah wajib hadir pada rilis sliding scale. `G-25` s.d. `G-28` |
| `INP-S18` | MAR | `READY_FOR_DOMAIN_DESIGN` | — | Tidak berubah. Dua pengguna baru: tautan intake obat dan dosis sliding scale. MAR **tidak** mendapat isian volume |
| `INP-S18` | Rekonsiliasi obat saat admisi | `READY_FOR_DOMAIN_DESIGN` | — | Tidak berubah |
| **`INP-S19`** | **Sliding scale** | **`READY_FOR_DOMAIN_DESIGN`** | — | Naik dari `BUSINESS_DECISION_REQUIRED`. Dirancang bersama MAR `INP-S18`. `G-22` s.d. `G-25`, `G-28`, `G-29`. Isi template gerbang produksi |
| **`INP-S19`** | **Handover shift** | **`DEFERRED`** — tidak dinilai | — | Dikeluarkan dari batas rilis oleh `RWI-DEC-145` (4); requirement tetap `P1` |
| **`INP-S19`** | **Transfusi** | **`DEFERRED`** — tidak dinilai | — | `RWI-DEC-145` (4), (5). `DEC-INP-012` dan `G-20` ikut ditunda |
| `INP-S20` | Enam layanan penunjang | `READY_FOR_DOMAIN_DESIGN` | — | Tidak berubah; empat layanan non-laboratorium/radiologi sebatas permukaan |
| `INP-S21` | Dokumentasi klinis dan ruang kerja dokter V2 | `READY_FOR_DOMAIN_DESIGN` | — | Tidak berubah |

**Hasil turunan:**

- `INP-S17` **`READY_FOR_DOMAIN_DESIGN`** — naik dari `PARTIALLY_READY`.
- `INP-S18` `READY_FOR_DOMAIN_DESIGN` — tetap.
- `INP-S19` **`READY_FOR_DOMAIN_DESIGN` terbatas pada sliding scale** — naik dari
  `BUSINESS_DECISION_REQUIRED`; handover shift dan transfusi `DEFERRED`.
- `INP-S20` dan `INP-S21` `READY_FOR_DOMAIN_DESIGN` — tetap.
- Seluruh slice penyelarasan `PRD-RWI-V2-001` kini **tanpa blocker keputusan bisnis** di dalam batas rilisnya.
- Overall Rawat Inap tetap **`PARTIALLY_READY`**, karena `INP-S07`, `S08`, `S09`, `S10`, `S15`, dan `S16`
  berada di luar penilaian ini dan tidak dinilai ulang.

### 15.13 Apa yang boleh berjalan dan apa yang harus berhenti

**Boleh berjalan ke amandemen desain `RLN-PH-06`:**

- `INP-S17` seluruhnya, termasuk intake obat dan darah serta pencatatan gula darah;
- `INP-S18` MAR dan rekonsiliasi obat saat admisi;
- `INP-S19` sliding scale — template, order per pasien, dan pelaksanaan;
- `INP-S20` seluruhnya, dengan empat layanan non-laboratorium/radiologi sebatas permukaan;
- `INP-S21` seluruhnya.

**Harus berhenti:**

- **handover shift dan transfusi**, sampai dijadwalkan ke irisan berikutnya. Tidak ada menu, tabel,
  maupun endpoint untuk keduanya (`RWI-AC-221`);
- tautan intake darah ke catatan transfusi, sampai transfusi dibangun;
- **pemakaian untuk pasien sungguhan** atas sliding scale sampai isi template disahkan pemilik klinis
  yang belum ditunjuk; atas insulin dan obat high-alert lain sampai daftarnya disahkan; serta atas
  Resiko Jatuh, isian wajib pengkajian, dan checklist Evaluasi Awal. Semua ini menahan **rilis**, bukan
  desain;
- **implementasi** daftar catatan terkunci "Catatan Saya", sampai Yoga Aji Pratama menyetujui
  (`RWI-DEC-142`). Ini tidak menahan desain.

**Dependency di antara slice yang boleh berjalan:**

```text
INP-S18 MAR ───────────────┬──► INP-S17 intake obat   (entri merujuk dosis Administered)
                           │
INP-S17 gula darah ────────┴──► INP-S19 sliding scale (dosis dicatat di MAR, GDS dirujuk dari Pengawasan Harian)

INP-S17 intake darah  ──  berdiri sendiri (tanpa tautan transfusi)
```

Karena itu MAR, pencatatan gula darah, intake obat, dan sliding scale sebaiknya dirancang dalam satu
amandemen `keperawatan` dan satu gelombang rilis. Urutan pembangunannya ditetapkan `plan-module-delivery`.

### 15.14 Keputusan pemilik yang dibutuhkan

| Jenis | Butir | Menahan | Pemilik |
|---|---|---|---|
| Blocker desain | **Tidak ada** | — | — |
| Non-blocking, disarankan dikonfirmasi saat approval `RLN-PH-06` | `G-22` jadwal dan lifecycle dosis sliding scale; `G-26` koreksi dosis MAR yang tertaut intake | — | Muhammad Hamzah |
| Non-blocking lain | `G-24` notifikasi "lapor dokter"; `G-25` satuan GDS; `G-27` penanda dosis tanpa entri intake; `G-28` tagihan GDS; `G-29` catatan V1; serta `G-05`, `G-09`, `G-17` dari bagian 14 | — | Muhammad Hamzah; `G-28` pemilik Billing; `G-29` Product + Data Owner |
| Gerbang produksi — **bertambah** | Pengesahan isi template protokol sliding scale (`RWI-DEC-146`) | Rilis sliding scale | Pemilik klinis — **belum ditunjuk** |
| Gerbang produksi — tetap | `RWI-OQ-056`, `RWI-OQ-057`, daftar high-alert termasuk insulin, checklist Evaluasi Awal | Rilis | Pemilik klinis/Farmasi — belum ditunjuk |
| Tidak dapat dijawab lewat wawancara | `RWI-OQ-064` Pemakaian Alat | `CAP-016` | Menunggu modul persediaan/aset |
| Ditunda | `RWI-OQ-093` pemilik alur transfusi | Transfusi, saat dijadwalkan | Muhammad Hamzah bersama pemilik modul Bank Darah |
| Dependency implementasi | Daftar catatan terkunci milik penulis (`RWI-DEC-142`) | Implementasi bagian itu | Yoga Aji Pratama |

### 15.15 Handoff

| Field | Nilai |
|---|---|
| `capability_scope` | `INP-S17` seluruhnya, `INP-S18`, `INP-S19` sliding scale, `INP-S20`, `INP-S21` |
| `requirement_readiness` | `S17`, `S18`, `S19` (sliding scale), `S20`, `S21` `READY_FOR_DOMAIN_DESIGN`; handover shift dan transfusi `DEFERRED` |
| `requirement_evidence_status` | Mayoritas `CONFIRMED`; 29 butir gap: 2 tertutup `CONFIRMED`, 6 `PROPOSED`, 20 `MISSING`, 1 `CONFLICT` pada transfusi yang `DEFERRED`. Nol `BLOCKING` untuk slice aktif |
| `domain_architecture_readiness` | `DOMAIN_ARCHITECTURE_NOT_RUN`, dan tidak diperlukan. Sliding scale dan intake obat memang melintasi `ClinicalManagement` dan `PharmacyManagement`, tetapi kepemilikan data dan arah rujukannya sudah ditetapkan pemilik lewat `RWI-DEC-147` s.d. `149`; yang tersisa desain kontrak. `RLN-PH-05` baru relevan saat transfusi dijadwalkan bersama `DEC-INP-012` |
| `decision_ids` | `DEC-INP-010` `CLOSED`, `DEC-INP-011` `CLOSED`, `DEC-INP-012` `DEFERRED`; `RWI-DEC-145` s.d. `149`; `RWI-AC-219` s.d. `231` |
| `dependency_ids` | Persetujuan `MedicalRecordManagement` `RWI-DEC-142`; komponen `rawat-jalan` `RLN3-CAP-03`; gerbang produksi isi template sliding scale, daftar high-alert, `RWI-OQ-056`, `RWI-OQ-057`, checklist Evaluasi Awal |
| `next_owner` | `design-business-module` untuk amandemen `dokter-rawat-inap` dan `keperawatan`, fase `RLN-PH-06` |
| `required_boundary` | Dokumentasi keperawatan dan dokter, termasuk entri intake dan gula darah, tetap milik `ClinicalManagement`. MAR, rekonsiliasi, template, order, dan pelaksanaan sliding scale milik `PharmacyManagement`. Desain **MUST NOT**: membuat tabel sliding scale di `ClinicalManagement` atau `InPatientManagement` (`RWI-AC-225`); menyimpan salinan GDS pada pelaksanaan sliding scale (`RWI-DEC-148` (2)); menambah isian volume pada MAR (`RWI-DEC-149` (4)); membuat menu, tabel, atau endpoint handover shift dan transfusi (`RWI-AC-221`); memakai hasil laboratorium sebagai sumber dosis (`RWI-AC-228`) |
| `expected_output` | Amandemen blueprint kedua sub-modul revision `7`: sliding scale dan intake obat/darah dirancang penuh bersama MAR, termasuk dua kontrak rujukan lintas modul dan baris kepemilikan `PharmacyManagement` pada `02-module-map.md`; handover shift dan transfusi ditandai `DEFERRED`; `G-22` dan `G-26` diajukan untuk dikonfirmasi saat approval; `keperawatan/04-prd-to-mvp.md` bagian 21.6 diperbarui |
