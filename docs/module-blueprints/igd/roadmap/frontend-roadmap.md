# Roadmap Delivery Frontend — Modul IGD

## Metadata

```yaml
module_id: igd
roadmap_revision: 1
status: DRAFT
owners:
  - "Product/Domain Owner sementara (IGD-DEC-046) — nama belum diisi"
  - "Frontend authority untuk area DEV_DISCRETION (IGD-UI-004)"
approved_by: []
input_revisions:
  blueprint-manifest.md: 4
  03-frontend-architecture.md: 4
artifact_hashes:
  03-frontend-architecture.md: "14399dcca9ea21821aea24f99ebd42e990e44c5e2e0c4bc0e4d1958142a5f6e1"
  contracts/api-contract.md: "f64dea9e9c98a269091b18a5b72d817dc1bf263cdc7692e8a957055dfdb77719"
  contracts/state-transition-matrix.md: "208ddc38ff2367210d8783c29b8d9b2e0b09fa7691a51317006fa848145add5f"
  contracts/validation-matrix.md: "b4bc0a86b8122e9ff20749f9c25497fabea78e49bb0ecf1fbd6a83eac26169ee"
contract_versions:
  - "API 0.2.0"
  - "State 0.2.0"
  - "Validation 0.2.0"
source_commits:
  backend: "e5331a015fa416a89454b435de0014455f0326d8"
  frontend: "08c84d371ed90640189ce1758019184b0a955e13"
```

Repository frontend adalah `QuilvianSystemFrontendDev`. Dokumen ini adalah **satu-satunya**
roadmap frontend IGD. Frontend cukup merujuk `module_id`, `roadmap_revision`, task ID, dan
contract version di atas; jangan menyalin isinya menjadi sumber kebenaran kedua.

---

## 1. Aturan paralel dengan backend

Kontrak API sudah berstatus `approved` pada versi `0.2.0` dan hash-nya terkunci pada
`blueprint-manifest.md`. Karena itu frontend **boleh** berjalan paralel dengan backend, dengan
dua batas berikut.

| Boleh dikerjakan sekarang | Wajib menunggu |
| --- | --- |
| Layar yang memakai endpoint yang sudah ada di kode backend | Layar yang memakai tiga endpoint baru, sampai task backend-nya selesai |
| Penyelarasan payload registrasi IGD yang saat ini bertentangan dengan backend | — |
| Persiapan menangani nilai status baru `Completed` | — |

Frontend **tidak boleh** membuat sumber data palsu sebagai pengganti permanen endpoint yang
belum ada. Data contoh hanya boleh dipakai di dalam test, tidak di dalam layar yang dipakai
petugas.

Tiga endpoint yang belum tersedia beserta task backend yang menyediakannya:

| Endpoint | Menunggu task backend |
| --- | --- |
| `POST /emergency-triages/{id}/retriage` | `BE-IGD-004` |
| `GET /emergency-triages/sla-breaches` | `BE-IGD-007` |
| `PATCH /emergency-visits/{id}/complete` | `BE-IGD-009` |

---

## 2. Keadaan awal frontend

Impact scan 14 Agustus 2026 membuktikan **nol berkas `src/` berubah** sejak blueprint diaudit,
sehingga temuan capability map masih berlaku seluruhnya.

| Fakta | Bukti | Akibat |
| --- | --- | --- |
| Hanya ada satu route IGD yang dapat dicapai, yaitu pendaftaran gawat darurat | `CAP-17`; `src/app/health-services/registration-management/emergency-registration/page.jsx` | Seluruh layar klinis IGD berstatus belum dibuat |
| Route itu mengirim tipe encounter yang ditolak backend | `CAP-17`; frontend mengirim `ENCOUNTER_TYPE.Emergency = 2`, backend hanya menerima `Outpatient` | Pendaftaran gawat darurat bisa gagal di tengah jalan |
| Layar pendaftaran mewajibkan data administratif sebelum penanda pasien tidak dikenal muncul | `CAP-17`; berkas konstanta baris 394–415 | Bertentangan dengan jalur keselamatan pasien gawat |
| Tidak ada satu pun konsumen endpoint triage, transfer, disposition, observasi, dan resusitasi | `CAP-18` berstatus `Missing` | Modul backend yang sudah ada belum dapat dipakai siapa pun |

> **Contoh akibat nyata dari baris kedua:** petugas mengisi formulir pendaftaran korban
> kecelakaan. Panggilan pertama membuat encounter berhasil. Panggilan kedua yang membuat
> kunjungan IGD ditolak karena tipe encounter tidak cocok. Yang tertinggal adalah satu
> encounter tanpa kunjungan IGD, dan petugas tidak tahu harus mengulang dari mana.

---

## 3. Slice dan urutan

| Slice | Hasil yang dapat diperiksa | Task |
| --- | --- | --- |
| **F0 — Pendaftaran IGD berhenti bertentangan dengan backend** | Pendaftaran gawat darurat berhasil sampai selesai tanpa menyisakan encounter menggantung | `FE-IGD-001` |
| **F1 — Petugas dapat melihat pasien yang sedang di IGD** | Daftar kunjungan tampil dengan seluruh keadaan layar tertangani | `FE-IGD-002` |
| **F2 — Perawat bekerja pada antrean triage** | Antrean, penanda keterlambatan, penilaian, dan penilaian ulang | `FE-IGD-003`, `FE-IGD-004` |
| **F3 — Kunjungan dapat diselesaikan** | Status baru tertangani lebih dulu, lalu tombol menyelesaikan kunjungan | `FE-IGD-005`, `FE-IGD-006` |
| **F4 — Pelayanan harian lengkap** | Observasi, tindak lanjut, perpindahan, dan halaman detail | `FE-IGD-007`, `FE-IGD-008`, `FE-IGD-009`, `FE-IGD-010` |

### Urutan dependency

```text
FE-IGD-001 (selaraskan pendaftaran)      ← tanpa dependency backend
FE-IGD-002 (daftar kunjungan)            ← tanpa dependency backend
FE-IGD-005 (tangani status Completed)    ← tanpa dependency backend, WAJIB sebelum BE-IGD-008 rilis

FE-IGD-004 (formulir triage & retriage)  ← BE-IGD-004
FE-IGD-003 (antrean + penanda terlambat) ← BE-IGD-007
FE-IGD-006 (tombol selesaikan kunjungan) ← BE-IGD-009

FE-IGD-007 (observasi)   ← kontrak lama, tanpa dependency
FE-IGD-008 (disposition) ← kontrak lama, tanpa dependency
FE-IGD-009 (transfer)    ← kontrak lama, tanpa dependency
FE-IGD-010 (detail kunjungan) ← FE-IGD-004, FE-IGD-007, FE-IGD-008, FE-IGD-009
```

Empat task pertama tidak menunggu backend sama sekali, sehingga frontend dapat mulai pada hari
yang sama dengan backend.

---

## 4. Aturan yang berlaku untuk semua task frontend

Aturan ini tidak diulang pada setiap task, tetapi mengikat semuanya.

### 4.1 Keadaan layar yang wajib ditangani

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Kerangka konten, bukan layar kosong |
| Kosong | Kalimat yang menjelaskan, misalnya "Belum ada pasien menunggu triage." |
| Gagal | Penjelasan singkat beserta tombol Coba lagi |
| Tanpa hak akses | "Anda tidak memiliki hak akses untuk melihat data ini." |
| Data usang | Penanda bahwa data perlu dimuat ulang |
| Kirim ganda | Tombol dinonaktifkan selama proses menyimpan |
| Validasi gagal | Kolom bermasalah ditandai beserta alasannya |

### 4.2 Larangan yang tidak boleh dilanggar

| Larangan | Alasan |
| --- | --- |
| Menanamkan warna kategori triase di kode | Warna berasal dari master `ColorName` dan `ColorHex`; kebijakan rumah sakit harus dapat berubah tanpa mengubah source |
| Menanamkan target waktu tunggu di kode | Sama, berasal dari master. Frontend juga tidak menghitung `ResponseDueAt` sendiri |
| Menampilkan UUID sebagai label | Identifier teknis bukan label pengguna; tampilkan nama |
| Menyembunyikan tombol sebagai satu-satunya pengaman | Backend tetap memvalidasi; menyembunyikan tombol hanya soal kenyamanan |
| Menyimpan data klinis ke penyimpanan lokal | Termasuk cache peramban yang tidak terenkripsi |

### 4.3 Yang boleh diputuskan sendiri oleh pengembang

Sesuai `IGD-UI-004`, urutan kolom tabel, penempatan tombol, dan pilihan komponen dari design
system berstatus `DEV_DISCRETION`. Struktur menu dan route publik diputuskan Manajer Sistem
Informasi. Aksesibilitas dan privasi tetap mengikat dan tidak pernah menjadi `DEV_DISCRETION`.

---

## 5. Task

### `FE-IGD-001` — Pendaftaran gawat darurat berhenti bertentangan dengan kontrak backend

| Field | Isi |
| --- | --- |
| **Outcome** | Petugas pendaftaran menyelesaikan pendaftaran korban tanpa panggilan kedua yang ditolak, dan tanpa meninggalkan encounter menggantung yang membingungkan |
| **Trace** | `IGD-DEC-041` (tipe encounter canonical adalah `Outpatient`); `CAP-17` berstatus `Conflict`; contract `0.2.0` |
| **Reuse** | Route, formulir, dan service pendaftaran yang sudah ada. Ini penyelarasan, bukan penulisan ulang layar |
| **Scope** | `src/utils/health-services/registration-management/emergency-management/emergency-registration.utils.js` (`buildEmergencyEncounterPayload`, `buildEmergencyVisitPayload`); `src/lib/constants/.../emergency-registration.constants.js` (`ENCOUNTER_TYPE`, `EMERGENCY_VISIT_REGISTRATION_STATUS`); `src/lib/services/.../emergency-registration.service.js` (`isEmergencyServiceUnit`) |
| **Dependency** | — |
| **Acceptance criteria** | 1. Payload encounter mengirim `Outpatient`, bukan `Emergency`. 2. Pemetaan status registrasi mengikuti backend, yaitu `Pending = 1`, `Provisional = 2`, `Registered = 3`, `Completed = 4`; label "Registered" tidak lagi dipakai untuk nilai 1. 3. Unit IGD diambil dari pengaturan IGD melalui `EmergencySettingController`, bukan dari tebakan kode seperti `SU-ER-001` dan `IGD`. 4. Bila panggilan kedua gagal, pengguna memperoleh pesan yang jelas beserta cara melanjutkan, bukan layar buntu |
| **Verification** | Test unit pembentukan payload untuk keempat kriteria; test komponen alur gagal pada panggilan kedua; uji manual satu pendaftaran utuh |
| **Risk/blocker** | Butir 3 mensyaratkan `BE-IGD-001` sudah selesai, karena `EmergencySettingController` juga termasuk yang tidak dapat dipanggil saat ini. Butir 1, 2, dan 4 tidak menunggu apa pun. Owner: Frontend authority + Backend/API |
| **DoD** | Empat kriteria terbukti; lint dan test lulus; laporan perubahan mencatat bahwa `CAP-17` sudah tidak berstatus conflict untuk tipe encounter dan status registrasi |

---

### `FE-IGD-002` — Petugas dapat melihat pasien yang sedang berada di IGD

| Field | Isi |
| --- | --- |
| **Outcome** | Siapa pun yang berwenang dapat membuka satu layar dan tahu siapa saja yang sedang ditangani di IGD saat ini, tanpa bertanya ke meja perawat |
| **Trace** | `03-frontend-architecture.md` bagian 2 baris "Daftar kunjungan IGD"; `GET /emergency-visits` yang sudah ada; contract `0.2.0` |
| **Reuse** | Komponen tabel, penyaringan, dan halaman yang sudah dipakai modul lain di project |
| **Scope** | Route baru daftar kunjungan IGD; komponen tabel; service konsumen endpoint kunjungan |
| **Dependency** | `BE-IGD-001` agar endpoint benar-benar menjawab |
| **Acceptance criteria** | 1. Daftar tampil beserta penyaringan dan halaman. 2. Ketujuh keadaan layar pada bagian 4.1 tertangani. 3. Nama pasien tampil sebagai nama. 4. Pasien tidak dikenal tampil dengan penanda yang jelas, bukan kolom kosong. 5. Aksi yang tidak dimiliki pengguna tidak ditampilkan |
| **Verification** | Test komponen untuk ketujuh keadaan; test tanpa hak akses; uji tampilan pada layar kecil |
| **Risk/blocker** | Kolom bertanda sensitif pada data dictionary tidak boleh ditampilkan pada daftar. Owner: Security/privacy |
| **DoD** | Lima kriteria terbukti; lint dan test lulus; tidak ada kolom sensitif di daftar |

---

### `FE-IGD-003` — Perawat melihat antrean triage beserta penanda pasien terlambat

| Field | Isi |
| --- | --- |
| **Outcome** | Perawat melihat siapa yang menunggu dinilai dan siapa yang sudah terlambat ditangani, tanpa menghitung sendiri selisih waktunya |
| **Trace** | `IGD-DEC-027`; `03-frontend-architecture.md` bagian 2 dan 4; `GET /emergency-triages` dan `GET /emergency-triages/sla-breaches`; `AT-IGD-063` |
| **Reuse** | Komponen tabel yang sama dengan `FE-IGD-002` |
| **Scope** | Route antrean triage; komponen tabel antrean; service konsumen kedua endpoint |
| **Dependency** | `BE-IGD-007` |
| **Acceptance criteria** | 1. Antrean tampil beserta ketujuh keadaan layar. 2. Pasien yang melampaui batas ditandai beserta keterangan lama menunggu, misalnya "menunggu 31 menit". 3. Warna kategori diambil dari master, tidak ditanam di kode. 4. Pasien yang target waktunya belum diatur **tidak** ditampilkan sebagai terlambat maupun sebagai patuh; tampilkan apa adanya bahwa targetnya belum diatur. 5. Nama pasien tampil sebagai nama |
| **Verification** | Test komponen untuk ketujuh keadaan; test khusus butir 4 dengan data target kosong; uji tampilan layar kecil |
| **Risk/blocker** | Butir 4 mudah terlewat dan akibatnya serius: menandai pasien Kuning sebagai terlambat padahal targetnya memang belum ada akan menutupi peringatan pasien Merah. Owner: Clinical governance sebagai pengesah akhir |
| **DoD** | Lima kriteria terbukti; warna dan target sepenuhnya dari master; lint dan test lulus |

---

### `FE-IGD-004` — Perawat menilai dan menilai ulang pasien

| Field | Isi |
| --- | --- |
| **Outcome** | Perawat mencatat penilaian triage, dan ketika kondisi pasien berubah, menilai ulang tanpa kehilangan catatan sebelumnya |
| **Trace** | `IGD-DEC-004`, `IGD-DEC-048`; `POST /emergency-triages` dan `POST /emergency-triages/{id}/retriage`; validation matrix bagian 2 |
| **Reuse** | Komponen formulir dan validasi yang sudah ada di project |
| **Scope** | Route formulir triage; komponen formulir; service konsumen kedua endpoint |
| **Dependency** | `BE-IGD-004` |
| **Acceptance criteria** | 1. Level triage dipilih dari master, bukan dari daftar yang ditanam di kode. 2. Menyelesaikan penilaian tanpa level ditolak dan kolomnya ditandai beserta alasannya. 3. Riwayat penilaian sebelumnya terlihat saat menilai ulang. 4. Penilaian yang sudah digantikan ditampilkan sebagai riwayat dan tidak dapat diubah dari layar ini. 5. Kategori Hitam tidak dapat dipilih sebagai bagian skala antrean biasa. 6. Menekan tombol simpan dua kali hanya menghasilkan satu penilaian |
| **Verification** | Test komponen keenam kriteria; test pesan galat 409 saat menilai ulang penilaian yang sudah dibatalkan |
| **Risk/blocker** | Butir 5 berasal dari keputusan keselamatan: aplikasi tidak boleh menetapkan kategori Hitam sendiri. Owner: Clinical governance |
| **DoD** | Enam kriteria terbukti; pesan galat dari backend ditampilkan apa adanya kepada perawat; lint dan test lulus |

---

### `FE-IGD-005` — Layar menangani nilai status kunjungan yang baru

| Field | Isi |
| --- | --- |
| **Outcome** | Ketika backend mulai mengirim status `Completed`, tidak ada layar yang menampilkan kolom kosong, label salah, atau berhenti bekerja |
| **Trace** | `IGD-DEC-049`; `03-frontend-architecture.md` bagian 7 catatan perubahan berpotensi memutus; manifest baris `compatibility_impact` |
| **Reuse** | Peta status yang sudah ada di frontend |
| **Scope** | Seluruh tempat frontend memetakan `EmergencyVisitStatus` secara eksklusif |
| **Dependency** | — |
| **Acceptance criteria** | 1. Nilai `Completed = 9` memiliki label yang dapat dibaca pengguna. 2. Nilai status yang tidak dikenal tidak membuat layar berhenti bekerja; tampilkan nilainya apa adanya. 3. Layar tetap benar walaupun backend belum mengirim nilai baru itu |
| **Verification** | Test unit peta status termasuk satu nilai tak dikenal; test komponen daftar kunjungan dengan data berstatus `Completed` |
| **Risk/blocker** | **Task ini wajib rilis sebelum `BE-IGD-008`.** Bila urutannya terbalik, layar yang memetakan status secara eksklusif akan rusak di lingkungan yang sama pada hari backend rilis. Owner: Product/Domain |
| **DoD** | Tiga kriteria terbukti; sudah rilis lebih dulu daripada `BE-IGD-008`; lint dan test lulus |

---

### `FE-IGD-006` — Dokter dapat menyelesaikan kunjungan dari layar

| Field | Isi |
| --- | --- |
| **Outcome** | Dokter menutup kunjungan langsung dari layar detail, dan ketika ada syarat yang belum tuntas, dia tahu persis apa yang kurang |
| **Trace** | `IGD-DEC-049`; `PATCH /emergency-visits/{id}/complete`; validation matrix bagian 3 |
| **Reuse** | Komponen konfirmasi tindakan yang sudah ada |
| **Scope** | Tombol dan dialog pada layar detail kunjungan; service konsumen endpoint |
| **Dependency** | `BE-IGD-009`, `FE-IGD-005`, `FE-IGD-010` |
| **Acceptance criteria** | 1. Tombol hanya muncul untuk pengguna yang berwenang dan hanya pada kunjungan berstatus `Disposed`. 2. Penolakan 409 ditampilkan sebagai kalimat yang dapat dibaca dokter, misalnya "Masih ada observasi yang belum diselesaikan.", bukan kode galat. 3. Setelah berhasil, status dan waktu selesai tampil di layar tanpa perlu memuat ulang manual. 4. Menekan tombol dua kali hanya mengirim satu permintaan |
| **Verification** | Test komponen keempat kriteria; test tiga jenis penolakan 409 sesuai validation matrix |
| **Risk/blocker** | Tombol yang disembunyikan bukan pengaman; backend tetap yang memutuskan. Owner: Frontend authority |
| **DoD** | Empat kriteria terbukti; ketiga pesan penolakan tampil dalam bahasa pengguna; lint dan test lulus |

---

### `FE-IGD-007` — Perawat mencatat pemantauan berkala

| Field | Isi |
| --- | --- |
| **Outcome** | Perawat mencatat perkembangan pasien selama observasi tanpa menyalin ulang tanda vital yang sudah ada di rekam klinis |
| **Trace** | `03-frontend-architecture.md` bagian 2 baris "Observasi"; endpoint observasi dan detail observasi yang sudah ada |
| **Reuse** | Komponen formulir dan daftar kronologis yang sudah ada; tanda vital dan catatan terpadu hanya **dirujuk**, tidak disalin |
| **Scope** | Route observasi; komponen daftar dan formulir; service konsumen endpoint |
| **Dependency** | `BE-IGD-001` |
| **Acceptance criteria** | 1. Catatan berkala tampil urut waktu. 2. Tanda vital dan catatan terpadu tampil sebagai rujukan, bukan salinan yang dapat diubah dari layar ini. 3. Observasi tidak dapat diselesaikan tanpa kesimpulan. 4. Ketujuh keadaan layar tertangani |
| **Verification** | Test komponen keempat kriteria |
| **Risk/blocker** | Menyalin data klinis ke layar IGD akan membuat dua sumber kebenaran. Owner: Clinical Management sebagai pemilik data |
| **DoD** | Empat kriteria terbukti; tidak ada penyalinan data klinis; lint dan test lulus |

---

### `FE-IGD-008` — Dokter menetapkan tindak lanjut pasien

| Field | Isi |
| --- | --- |
| **Outcome** | Dokter mencatat keputusan akhir pasien, misalnya pulang, rawat inap, atau rujuk, beserta tujuannya bila diperlukan |
| **Trace** | `IGD-DEC-005`; endpoint disposition yang sudah ada; validation matrix bagian 4; state matrix bagian 3 |
| **Reuse** | Komponen formulir dan pilihan yang sudah ada |
| **Scope** | Route disposition; komponen formulir; service konsumen endpoint |
| **Dependency** | `BE-IGD-003` agar jenis tindak lanjut tersedia di master |
| **Acceptance criteria** | 1. Jenis tindak lanjut dipilih dari master. 2. Unit tujuan wajib muncul dan wajib diisi bila jenisnya mensyaratkan. 3. Fasilitas rujukan wajib muncul dan wajib diisi bila jenisnya mensyaratkan. 4. Pembatalan wajib mengisi alasan. 5. Layar tidak menyiratkan bahwa menetapkan tindak lanjut berarti kunjungan sudah selesai |
| **Verification** | Test komponen kelima kriteria |
| **Risk/blocker** | Butir 5 penting: `Executed` pada tindak lanjut tidak sama dengan kunjungan selesai. Bila layar menyiratkan sebaliknya, laporan lama tinggal pasien menjadi salah. Owner: Product/Domain |
| **DoD** | Lima kriteria terbukti; perbedaan tindak lanjut dan penyelesaian kunjungan jelas di layar; lint dan test lulus |

---

### `FE-IGD-009` — Perawat mengajukan dan memantau perpindahan pasien

| Field | Isi |
| --- | --- |
| **Outcome** | Perpindahan pasien ke unit lain terlacak dari pengajuan sampai tiba, dan setiap pihak hanya melakukan bagiannya |
| **Trace** | `IGD-DEC-005`, `IGD-DEC-026`; endpoint transfer yang sudah ada; state matrix bagian 4; `AT-IGD-041`, `AT-IGD-042` |
| **Reuse** | Komponen daftar dan formulir yang sudah ada |
| **Scope** | Route transfer; komponen daftar dan formulir; service konsumen endpoint |
| **Dependency** | `BE-IGD-010` untuk butir 3 |
| **Acceptance criteria** | 1. Rangkaian `Requested`, `Accepted`, `InTransit`, `Completed` terlihat beserta pelaku dan waktunya. 2. Penolakan wajib mengisi alasan. 3. Pengguna yang mengajukan tidak melihat tombol menerima untuk perpindahan yang sama; bila tetap dikirim, penolakan 403 dari backend ditampilkan sebagai kalimat yang dapat dibaca. 4. Ketujuh keadaan layar tertangani |
| **Verification** | Test komponen keempat kriteria; test penolakan 403 |
| **Risk/blocker** | Menyembunyikan tombol tidak cukup; pemisahan tugas dijaga backend pada `BE-IGD-010`. Owner: Security/privacy |
| **DoD** | Empat kriteria terbukti; pemisahan pengaju dan penerima terlihat jelas; lint dan test lulus |

---

### `FE-IGD-010` — Halaman detail satu kunjungan IGD

| Field | Isi |
| --- | --- |
| **Outcome** | Dokter dan perawat melihat seluruh perjalanan satu pasien di IGD dalam satu halaman: penilaian, penilaian ulang, observasi, tindakan, tindak lanjut, dan perpindahan |
| **Trace** | `03-frontend-architecture.md` bagian 2 baris "Detail kunjungan" |
| **Reuse** | Komponen dari `FE-IGD-004`, `FE-IGD-007`, `FE-IGD-008`, dan `FE-IGD-009`; halaman ini merangkai, bukan menulis ulang |
| **Scope** | Route detail kunjungan; komponen perangkai |
| **Dependency** | `FE-IGD-004`, `FE-IGD-007`, `FE-IGD-008`, `FE-IGD-009` |
| **Acceptance criteria** | 1. Riwayat penilaian tampil urut, penilaian yang digantikan jelas terlihat sebagai riwayat. 2. Setiap bagian menangani ketujuh keadaan layar secara mandiri, sehingga satu bagian gagal tidak mematikan seluruh halaman. 3. Nama pasien, nama unit, dan nama petugas tampil sebagai nama. 4. Kolom bertanda sensitif hanya tampil bagi yang berhak |
| **Verification** | Test komponen keempat kriteria; satu test membuktikan kegagalan satu bagian tidak mematikan halaman |
| **Risk/blocker** | Halaman ini menampilkan data paling lengkap, sehingga paling berisiko membocorkan kolom sensitif. Owner: Security/privacy |
| **DoD** | Empat kriteria terbukti; pemeriksaan kolom sensitif tercatat di laporan perubahan; lint dan test lulus |

---

## 6. Layar yang belum dapat direncanakan

Empat kebutuhan berikut disebut pada decision log tetapi belum punya kontrak backend, sehingga
belum dapat menjadi task frontend. Rinciannya ada pada bagian 7 `backend-roadmap.md`.

| Layar | Menunggu | Coverage gap |
| --- | --- | --- |
| Pendaftaran pasien tidak dikenal tanpa data administratif lengkap | Desain encounter provisional | `CG-01` |
| Layar mode korban massal atau bencana | Desain mode bencana | `CG-02` |
| Penggabungan identitas pasien sementara | Desain reconciliation | `CG-03` |
| Tindak lanjut hasil penunjang yang datang terlambat | Sistem pemilik hasil belum bernama | `CG-05` |

Layar pendaftaran saat ini yang mewajibkan data administratif lebih dulu **tetap** bertentangan
dengan jalur keselamatan pasien gawat. `FE-IGD-001` hanya memperbaiki tipe encounter, pemetaan
status, dan pemilihan unit; ia tidak menyelesaikan pertentangan itu, dan tidak boleh diklaim
menyelesaikannya.
