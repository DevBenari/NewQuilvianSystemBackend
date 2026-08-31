# Roadmap Delivery Frontend — Modul Rawat Inap

## Metadata

```yaml
module_id: rawat-inap
repository: QuilvianSystemFrontendDev
roadmap_revision: 2
status: APPROVED
approval_gate: BLUEPRINT_APPROVED
owners:
  - "Product/Domain: Muhammad Hamzah (RWI-DEC-061)"
  - "Frontend authority: sesuai 03-frontend-architecture.md bagian 9"
approved_by:
  - "Muhammad Hamzah — Product/Domain owner (RWI-DEC-061), lewat RWI-DEC-067; sinkronisasi revision 2 lewat RWI-DEC-074"
approved_at: "2026-08-24"
input_revisions:
  blueprint-manifest.md: 4
  03-frontend-architecture.md: 0.3
  04-prd-to-mvp.md: 0.4.0
  01-existing-capability-map.md: 1.2
artifact_hashes:
  blueprint-manifest.md: "07f4ed008a53bab5186e0de059ab593b48966ef684d9702216354ba9891ebba0"
  contracts/api-contract.md: "a451e778e37a6596977ce6c2c9e24bc1548cd9dd4efa9a63e642ba02539b709b"
  contracts/permission-audit-matrix.md: "50a48e990ac9aaf1d97fc6f7448fd60f513292fd7da717faaaba2eced4d4e19b"
  contracts/validation-matrix.md: "6ff47efa675605e78bcdb8836fb636bd8744a1c07f2522508aa64261fd3f838d"
contract_versions:
  - "API 0.4.0"
  - "Permission/Audit 0.4.0"
  - "Validation 0.4.0"
source_commits:
  backend: "5afb54bd75281648010e50ef14f43ca1f80d8efd"
  frontend: "dec4fdeff07c3c96ad9f07f41f184c54cf771371"
task_count: 19
```

---

## 0. Peringatan yang tidak boleh dilewati

> **Roadmap ini berstatus `APPROVED` sejak 2026-08-24.** Blueprint disetujui Muhammad Hamzah
> lewat `RWI-DEC-067`. Penulisan `.jsx` dibuka untuk task yang dependency-nya sudah selesai;
> selebihnya tetap menunggu endpoint backend benar-benar ada.

Keadaan frontend hari ini, dari capability map revision `1.2`:

| Hal | Keadaannya |
| --- | --- |
| Route Rawat Inap | **Tidak ada satu pun.** `src/app/health-services/` hanya memuat enam folder, tidak ada inpatient |
| Menu Rawat Inap | **Tidak ada.** Menu hanya mengenal "Rawat Jalan" dan "Instalasi Gawat Darurat" |
| Layar master tempat tidur | Ada, dan **satu tombolnya rusak** — memanggil endpoint yang tidak pernah ada |
| Berkas test frontend | **Empat.** Tidak satu pun menyentuh tempat tidur atau kunjungan |

---

## 1. Batas kewenangan dokumen ini

`03-frontend-architecture.md` menetapkan **kontrak fungsional**: layar apa yang dibutuhkan, siapa
boleh melakukan apa, data dan status apa yang dikonsumsi, dan bagaimana keadaan gagal ditangani.
Ia **tidak** menetapkan menu sidebar, urutan menu, route final, pemakaian tab atau modal atau
drawer, warna, tata letak, maupun pustaka komponen.

Urutan wewenang yang berlaku pada setiap task di bawah:

```text
keamanan / privasi / invariant
  -> brief produk atau UI yang disetujui
  -> konvensi dan design system project
  -> DEV_DISCRETION
```

Empat hal yang **bukan** `DEV_DISCRETION`, dan karena itu ditulis sebagai acceptance criteria yang
mengikat: aturan tombol pada arsitektur bagian 3, penanganan 409 dan 422 pada bagian 5.4, privasi
pada bagian 6, dan perbaikan pada bagian 7.

Empat belas layar fungsional **boleh digabung** selama seluruh kemampuannya tercapai. Karena itu
task di bawah diberi nama menurut **kemampuan**, bukan menurut jumlah halaman.

---

## 2. Aturan paralel dengan backend

Api contract `0.3.0` memuat 50 baris: **49 endpoint baru** yang seluruhnya berstatus **Rencana
(belum tersedia)**, ditambah satu perubahan perilaku pada `PATCH /beds/{id}/availability` yang
endpoint-nya sudah ada. Konsekuensinya tegas:

| Boleh mendahului backend | Harus menunggu backend |
| --- | --- |
| `FE-RWI-001` perbaikan tombol tempat tidur — memakai endpoint `PATCH /beds/{id}/status` yang **sudah ada** | Seluruh task lain |
| `FE-RWI-002` kerangka service, slice, dan route — bentuk `ApiResponse` sudah terkunci kontrak | — |

Untuk task selain kedua itu, "menunggu backend" berarti **endpoint pasangannya sudah dapat
dipanggil dan mengembalikan 200**, bukan sekadar sudah ada di dokumen kontrak.

---

## 3. Slice dan milestone

| Slice | Hasil yang dapat diperiksa | Task |
| --- | --- | --- |
| **F0 — Cacat prasyarat dibereskan** | Admin dapat menutup tempat tidur rusak | `FE-RWI-001` |
| **F1 — Fondasi dan master** | Kerangka pemanggilan berdiri; admin dapat mengubah pengaturan dan butir administrasi | `FE-RWI-002` s.d. `FE-RWI-004` |
| **F2 — Admisi dan penempatan** | Petugas dapat membuka admisi, mencari tempat tidur, dan menempatkan pasien; penolakan terbaca alasannya | `FE-RWI-005` s.d. `FE-RWI-007` |
| **F3 — Census dan detail episode** | Perawat tahu siapa dirawat di mana; detail episode utuh | `FE-RWI-008`, `FE-RWI-009` |
| **F4 — Penanggung jawab dan perpindahan** | Pasien dapat dipindahkan; DPJP dan perawat dapat dialihkan | `FE-RWI-010`, `FE-RWI-011` |
| **F5 — Pulang dan penutupan** | Resume, kelayakan keuangan, kepergian, dan penutupan | `FE-RWI-012` s.d. `FE-RWI-015` |
| **F6 — Pantau dan koreksi** | Empat daftar pantau, laporan selisih, sesi koreksi | `FE-RWI-016` s.d. `FE-RWI-018` |
| **F7 — Kesiapan** | Layar terbukti hanya dijangkau peran yang berhak | `FE-RWI-019` |

### Urutan dependency

```text
FE-RWI-001 (perbaikan tombol bed)  ← prasyarat lintas repo bagi BE-RWI-006
FE-RWI-002 (service + slice + route)
   ├── FE-RWI-003 (pengaturan)          ← butuh BE-RWI-005
   ├── FE-RWI-004 (butir administrasi)  ← butuh BE-RWI-005
   ├── FE-RWI-005 (papan tempat tidur)  ← butuh BE-RWI-010
   │      └── FE-RWI-006 (admisi + isolasi awal)   ← butuh BE-RWI-007, BE-RWI-014
   │             └── FE-RWI-007 (penempatan + 409/422)  ← butuh BE-RWI-011, 013, 015
   │                    ├── FE-RWI-008 (census)          ← butuh BE-RWI-016
   │                    └── FE-RWI-009 (detail episode)  ← butuh BE-RWI-009, BE-RWI-014
   │                           ├── FE-RWI-010 (perpindahan)   ← butuh BE-RWI-019
   │                           └── FE-RWI-011 (DPJP + perawat) ← butuh BE-RWI-017, 018
   │                                  ├── FE-RWI-012 (pulang + resume)  ← butuh BE-RWI-020..022
   │                                  ├── FE-RWI-013 (kelayakan keuangan) ← butuh BE-RWI-024
   │                                  ├── FE-RWI-014 (penutupan + override) ← butuh BE-RWI-025, 026
   │                                  └── FE-RWI-015 (catat kepergian)  ← butuh BE-RWI-027
   │                                         ├── FE-RWI-016 (daftar pantau) ← butuh BE-RWI-029
   │                                         ├── FE-RWI-017 (laporan selisih) ← butuh BE-RWI-029
   │                                         └── FE-RWI-018 (sesi koreksi)  ← butuh BE-RWI-030
FE-RWI-019 (e2e kesiapan) — paling akhir
```

---

## 4. Task

### `FE-RWI-001` — Admin dapat kembali menutup tempat tidur yang rusak

| Field | Isi |
| --- | --- |
| **Outcome** | Tombol aktifkan dan nonaktifkan pada layar master tempat tidur berfungsi. Tanpa ini, `BE-RWI-006` akan mencabut wewenang admin atas `Reserved` dan `Occupied` sementara satu-satunya jalan keluarnya juga rusak |
| **Trace** | `RWI-CON-TRC-001`, `RWI-DEC-049`; `03-frontend-architecture.md` bagian 7; `EPIC RI-32` |
| **Reuse** | Endpoint `PATCH /beds/{id}/status` yang **sudah ada** di backend. **Tidak ada perubahan backend sama sekali** |
| **Scope** | `src/lib/state/slice/health-services/master-data/master-data-bed-slice.jsx` baris 315–322 dan 334–341 |
| **Dependency** | — |
| **Wewenang UI** | Semula "tidak ada, ini perbaikan pemanggilan". **Diperluas 26 Agustus 2026** oleh pemilik pekerjaan setelah terbukti tombolnya memang belum pernah ada di layar, sehingga perbaikan pemanggilan saja tidak dapat memenuhi Outcome |
| **Acceptance criteria** | 1. Tombol aktifkan memanggil `PATCH /beds/{id}/status`, bukan `/activate`. 2. Tombol nonaktifkan memanggil endpoint yang sama, bukan `/deactivate`. 3. Keduanya berhasil dan status di layar berubah tanpa muat ulang halaman. 4. Tidak ada lagi permintaan yang mengembalikan 404 dari layar ini |
| **Verification** | Unit test atau e2e pada slice tempat tidur — **wajib**, karena hari ini tidak ada satu pun test yang menyentuh layar bed. Periksa jaringan dan pastikan tidak ada 404 |
| **Risk/blocker** | Ini prasyarat lintas repository. `BE-RWI-006` **tidak boleh** rilis sebelum task ini rilis. Koordinasikan urutannya, jangan asumsikan. Owner: Frontend bersama Backend/API |
| **DoD** | Dua pemanggilan diperbaiki; keempat kriteria lulus; test baru ada dan lulus; laporan menyatakan tidak ada perubahan backend |
| **Status** | ✅ **Selesai, menunggu rilis.** Keempat kriteria lulus dengan bukti: e2e di browser sungguhan mencatat satu PATCH ke `/{id}/status` berisi `{"isActive":false}`, label tombol dan baris status berubah tanpa muat ulang halaman, dan nol respons 404. `lint:errors`, `test:unit` 19/19, dan `build` lulus. Dua celah yang semula tidak tercatat ikut ditutup: tombolnya memang belum pernah ada di layar, dan form Perbarui diam-diam mengaktifkan ulang tempat tidur nonaktif. Perubahan **masih lokal, belum di-commit** ([laporan](../task/report/frontend/FE-RWI-001.md)) |

---

### `FE-RWI-002` — Kerangka pemanggilan Rawat Inap berdiri

| Field | Isi |
| --- | --- |
| **Outcome** | Layar-layar berikutnya tidak perlu masing-masing menemukan ulang cara memanggil API, menangani `ApiResponse`, dan menyimpan keadaan. Satu kerangka, dipakai semua |
| **Trace** | `03-frontend-architecture.md` bagian 8; api contract `0.3.0` seluruh base URL |
| **Reuse** | `InstanceAxios` pada `src/lib/axiosInstance/InstanceAxios`; pembungkus jawaban dan pemakluman 404 pada `src/lib/services/health-services/clinical-management/doctor-consultation.service.js:15-23`; pola slice Redux pada `src/lib/state/slice/health-services/master-data/` |
| **Scope** | Berkas service untuk lima base URL modul dan dua base URL master; slice Redux terkait; kerangka route di bawah `src/app/health-services/` |
| **Dependency** | — |
| **Wewenang UI** | Nama menu, urutan menu, dan route final adalah `DEV_DISCRETION`, mengikuti konvensi `src/app/health-services/` yang sudah ada |
| **Acceptance criteria** | 1. Seluruh pemanggilan lewat `InstanceAxios`, tidak ada `fetch` telanjang. 2. Bentuk `ApiResponse` ditangani di satu tempat, bukan diulang tiap layar. 3. Route baru dapat dijangkau dan tidak merusak route yang sudah ada. 4. **Ruang kerja antrean dokter `useDoctorConsultationWorkspace.js` tidak disalin** |
| **Verification** | Uji jelajah route yang sudah ada untuk membuktikan tidak ada yang rusak; pemeriksaan kode untuk kriteria 1 dan 4 |
| **Risk/blocker** | Kriteria 4 penting: ruang kerja antrean dokter berputar pada `queueId`, sedangkan pasien rawat inap **tidak punya antrean**. Menyalinnya akan membawa masalah yang sama yang membuat `DEC-INP-001` terhenti. Owner: Frontend |
| **DoD** | Kerangka berdiri; keempat kriteria lulus; route lama terbukti utuh |
| **Status** | ✅ **Selesai, sudah di-commit** (`75d174db2`). Tujuh base URL kontrak berdiri di atas satu pembungkus `ApiResponse`, state terdaftar di store, route `/health-services/inpatient-management` terbaca pada keluaran `npm run build`, dan menu Rawat Inap muncul. Empat test fondasi lulus ([laporan](../task/report/frontend/FE-RWI-002.md)) |

---

### `FE-RWI-003` — Admin dapat mengubah pengaturan Rawat Inap

| Field | Isi |
| --- | --- |
| **Outcome** | Batas waktu pemesanan, ambang daftar pantau, dan awalan nomor episode dapat diubah lewat layar, sehingga tidak ada permintaan perubahan kode hanya untuk mengganti angka |
| **Trace** | `FE-INP-12`; `RWI-DEC-008`, `RWI-DEC-032`; api contract bagian Inpatient Setting; `EPIC RI-31` |
| **Reuse** | Pola layar master data yang sudah ada pada `src/lib/state/slice/health-services/master-data/` |
| **Scope** | Layar pengaturan; slice terkait |
| **Dependency** | `FE-RWI-002`; **`BE-RWI-005` sudah dapat dipanggil** |
| **Wewenang UI** | Tata letak dan pengelompokan isian bebas |
| **Acceptance criteria** | 1. Kedelapan nilai pengaturan terbaca dan dapat diubah. 2. Hanya peran admin master data yang dapat membuka; peran lain tidak melihat menunya. 3. Perubahan yang gagal menampilkan pesan server apa adanya, bukan kalimat umum. 4. Isian yang sudah diketik **tidak hilang** ketika penyimpanan ditolak |
| **Verification** | e2e satu jalur berhasil dan satu jalur gagal; pemeriksaan visibilitas menu per peran |
| **Risk/blocker** | Kriteria 4 berlaku untuk **seluruh** layar pada roadmap ini dan disebut di sini pertama kali. Layar yang mengosongkan formulir setiap kali server menolak akan membuat petugas mengetik ulang berkali-kali. Owner: Frontend |
| **DoD** | Layar selesai; keempat kriteria lulus; test e2e ada |
| **Status** | 🟡 **Layar selesai, tiga dari empat kriteria lulus.** Kriteria 1, 3, dan 4 terbukti lewat e2e di browser sungguhan; kriteria 4 diuji balik dengan mutasi. Kriteria 2 baru separuh: penolakan server sudah memunculkan layar Akses Ditolak, tetapi **menu tidak dapat disembunyikan per peran** karena `filter-menu-items-by-role.jsx` tidak menyaring apa pun dan frontend tidak punya data hak akses per butir. `lint:errors` dan `build` lulus ([laporan](../task/report/frontend/FE-RWI-003.md)) |

---

### `FE-RWI-004` — Admin dapat mengelola butir daftar periksa administrasi

| Field | Isi |
| --- | --- |
| **Outcome** | Rumah sakit dapat menambah, mengubah, menonaktifkan, dan menghapus butir administrasi sendiri, tanpa menunggu rilis |
| **Trace** | `FE-INP-13`; `RWI-DEC-026`, `RWI-DEC-033`; api contract bagian Inpatient Clearance Item (6 endpoint); `EPIC RI-31` |
| **Reuse** | Sama dengan `FE-RWI-003` |
| **Scope** | Layar master butir administrasi; slice terkait |
| **Dependency** | `FE-RWI-002`; **`BE-RWI-005` sudah dapat dipanggil** |
| **Wewenang UI** | Bebas, termasuk pemakaian modal atau halaman terpisah |
| **Acceptance criteria** | 1. Enam kemampuan tersedia: daftar, detail, tambah, ubah, ubah status aktif, dan tandai terhapus. 2. Penanda wajib atau tidak wajib terbaca jelas pada daftar. 3. `ItemCode` kembar ditolak dengan pesan server apa adanya. 4. Hanya admin master data yang dapat membukanya |
| **Verification** | e2e enam kemampuan; test kode kembar |
| **Risk/blocker** | Butir `DISCHARGE-MED` bawaan bertanda **tidak wajib** karena modul Farmasi di luar scope. Layar tidak boleh menyarankan bahwa penandaannya otomatis. Owner: Frontend bersama Product/Domain |
| **DoD** | Layar selesai; keempat kriteria lulus; test e2e ada |
| **Status** | 🟡 **Layar selesai, tiga dari empat kriteria lulus.** Keenam kemampuan dijalankan berurutan di browser sungguhan dan dicocokkan dengan permintaan yang benar-benar terkirim ke keenam endpoint kontrak; penanda **Wajib**/**Tidak wajib** terbaca sebagai kata pada daftar; kode kembar ditolak 409 dengan kalimat server apa adanya tanpa menghapus isian. Kriteria 4 mewarisi kekurangan `FE-RWI-003`: menu belum dapat disembunyikan per peran. `lint:errors` dan `build` lulus ([laporan](../task/report/frontend/FE-RWI-004.md)) |

---

### `FE-RWI-005` — Petugas dapat melihat tempat tidur yang benar-benar dapat dipakai

| Field | Isi |
| --- | --- |
| **Outcome** | Petugas melihat papan ketersediaan per unit layanan dan kamar, dan tahu bukan hanya tempat tidur mana yang kosong tetapi juga **kenapa** sebagian tidak boleh dipakai |
| **Trace** | `FE-INP-02`; `03-frontend-architecture.md` bagian 4.2 dan **4.3A**; api contract `/available-beds` dan `/bed-board`; `EPIC RI-22`, `EPIC RI-34` |
| **Reuse** | Isian pilihan sumber daya pada `src/lib/hooks/select/health-service/health-service-select-resources.js` untuk pilihan unit layanan, kamar, dan kelas |
| **Scope** | Layar papan ketersediaan; slice tempat tidur Rawat Inap |
| **Dependency** | `FE-RWI-002`; **`BE-RWI-010` sudah dapat dipanggil** |
| **Wewenang UI** | Bentuk penandaan bebas — baris redup, ikon, kelompok terpisah, atau penyaring yang dapat dimatikan |
| **Acceptance criteria** | 1. Hasil `GET /available-beds` ditampilkan **apa adanya**; layar **tidak** menyaring ulang dengan aturannya sendiri. 2. Tempat tidur yang tersaring keluar boleh ditampilkan sebagai baris nonaktif disertai alasannya, dan itu dianjurkan. 3. Tempat tidur yang tidak layak **tidak dapat dipilih**. 4. Alasan penolakan terbaca petugas, bukan hanya tersembunyi di balik ikon. 5. Papan mengelompokkan per unit layanan dan kamar |
| **Verification** | e2e satu jalur normal; pemeriksaan kode untuk kriteria 1 |
| **Risk/blocker** | Kriteria 1 adalah aturan keras. Aturan Kelayakan Penempatan yang bercabang dua — satu di server, satu di layar — akan berselisih dalam hitungan minggu, dan yang di layar akan kalah benar. Owner: Frontend |
| **DoD** | Layar selesai; kelima kriteria lulus; terbukti tidak ada logika penyaringan kedua di frontend |
| **Status** | 🟡 **Layar selesai, kelima kriteria lulus.** Papan berkelompok per unit layanan dan kamar; tempat tidur yang tidak dapat dipakai tetap tampil sebagai baris redup beserta alasan tertulis; hanya yang diloloskan `/available-beds` yang dapat dipilih. Ketiadaan aturan kelayakan kedua dibuktikan test kode: `isForMale`, `requiresIsolation`, dan kata gender tidak muncul sama sekali pada layar. Tersisa satu hal di luar kendali task: gerbang kesiapan data master `RWI-DEC-063` masih terbuka, sehingga papan belum diuji dengan data nyata ([laporan](../task/report/frontend/FE-RWI-005.md)) |

---

### `FE-RWI-006` — Petugas dapat membuka admisi beserta catatan awal isolasi

| Field | Isi |
| --- | --- |
| **Outcome** | Petugas admisi memilih pasien, penjamin, DPJP, kelas, dan unit layanan dalam satu alur; dan bila surat rujukan menyebut kebutuhan isolasi, ia dapat merekamnya di sini juga — supaya pencarian tempat tidur langsung menyaring dengan benar |
| **Trace** | `FE-INP-03`, `FE-INP-15` bagian admisi; `RWI-DEC-011`, `RWI-DEC-065`; `FR-RI-159`; api contract `POST /episodes` dan `PATCH /episodes/{id}/isolation-requirement`; `EPIC RI-21`, `EPIC RI-34` |
| **Reuse** | Isian pilihan sumber daya untuk pasien, dokter, penjamin, unit layanan, dan kelas |
| **Scope** | Layar admisi; penetapan kebutuhan isolasi selagi episode `Draft` |
| **Dependency** | `FE-RWI-005`; **`BE-RWI-007` dan `BE-RWI-014` sudah dapat dipanggil** |
| **Wewenang UI** | Pemakaian tab, modal, atau drawer bebas, selama aturan pengiriman ganda pada arsitektur bagian 5.3 dipenuhi |
| **Acceptance criteria** | 1. Admisi tanpa DPJP ditolak, dan pesannya menyebut DPJP wajib. 2. Menyalakan kebutuhan isolasi **wajib** disertai keterangan; tanpa keterangan tombol simpan tidak aktif atau ditolak dengan pesan jelas. 3. Setelah kebutuhan isolasi disetel, pencarian tempat tidur pada layar yang sama ikut tersaring. 4. Menekan tombol simpan dua kali hanya menghasilkan satu episode. 5. Isian tidak hilang ketika server menolak |
| **Verification** | e2e satu jalur berhasil dan tiga jalur gagal; test pengiriman ganda |
| **Risk/blocker** | Kriteria 3 adalah alasan kenapa penetapan isolasi diletakkan di layar admisi, bukan hanya di detail episode: menyetelnya setelah tempat tidur dipilih berarti pilihannya sudah telanjur salah. Owner: Frontend |
| **DoD** | Layar selesai; kelima kriteria lulus; test e2e ada |
| **Status** | ✅ **Selesai.** Kelima kriteria lulus lewat enam e2e di browser sungguhan: admisi tanpa DPJP ditolak dengan pesan yang menyebut DPJP, kebutuhan isolasi tanpa keterangan membuat tombol simpan mati, menyetel isolasi langsung menyaring ulang tempat tidur pada layar yang sama, dua klik beruntun saat balasan ditunda tetap menghasilkan satu episode, dan isian bertahan saat server menolak ([laporan](../task/report/frontend/FE-RWI-006.md)) |

---

### `FE-RWI-007` — Penolakan penempatan terbaca alasannya, bukan sekadar gagal

| Field | Isi |
| --- | --- |
| **Outcome** | Petugas yang penempatannya ditolak langsung tahu **aturan mana** yang gagal — tempat tidur sudah terisi, kamar sedang dihuni jenis kelamin berbeda, atau kebutuhan isolasi tidak cocok — dan tahu apa langkah berikutnya |
| **Trace** | `03-frontend-architecture.md` bagian 5.4; validation matrix bagian 4; `RWI-RULE-012`; `EPIC RI-23`, `EPIC RI-34`; `UAT-29`, `UAT-31` |
| **Reuse** | Penanganan `ApiResponse` dari `FE-RWI-002` |
| **Scope** | Aksi penempatan; penanganan 409 dan 422 di seluruh layar Rawat Inap |
| **Dependency** | `FE-RWI-006`; **`BE-RWI-011`, `BE-RWI-013`, dan `BE-RWI-015` sudah dapat dipanggil** |
| **Wewenang UI** | Bentuk penyajian daftar alasan bebas |
| **Acceptance criteria** | 1. **409** memicu muat ulang data, menampilkan pesan server, dan membiarkan pengguna memilih ulang. Isian yang sudah diketik **tidak boleh hilang**. 2. **422** menampilkan **daftar** aturan yang gagal, bukan satu kalimat umum. 3. Pesan penolakan pencampuran kamar ditampilkan **apa adanya**, termasuk nama kamarnya — tidak diganti kalimat buatan sendiri. 4. Dua pesan isolasi yang berbeda arah tidak tertukar. 5. Tempat tidur yang direbut pasien lain hilang dari daftar setelah muat ulang |
| **Verification** | e2e dua petugas merebut tempat tidur yang sama; e2e penolakan jenis kelamin; e2e dua arah penolakan isolasi |
| **Risk/blocker** | Kriteria 4 mudah terlewat karena kedua pesan berkode 422 dan panjangnya mirip, padahal artinya berlawanan. Salah satu berarti "pasien ini butuh isolasi", satunya "tempat tidur ini untuk pasien lain". Owner: Frontend |
| **DoD** | Kelima kriteria lulus; ketiga skenario e2e ada dan lulus |
| **Status** | ✅ **Selesai.** Ketiga skenario e2e ada dan lulus: perebutan tempat tidur (409), penolakan berlapis termasuk pencampuran kamar (422), dan kedua arah pesan isolasi. Arah pesan isolasi diturunkan dari **kode** `ISOLATION_REQUIRED` dan `ISOLATION_BED_RESERVED`, bukan dari kalimatnya, sehingga tidak mungkin tertukar walau kalimat server diperbaiki ([laporan](../task/report/frontend/FE-RWI-007.md)) |

---

### `FE-RWI-008` — Perawat tahu siapa dirawat, di mana, dan sudah berapa hari

| Field | Isi |
| --- | --- |
| **Outcome** | Satu layar menjawab pertanyaan yang hari ini hanya dapat dijawab dengan berkeliling bangsal |
| **Trace** | `FE-INP-01`; `RWI-FE-001` `DEV_DISCRETION`; `RWI-RULE-019`; api contract bagian Census; `EPIC RI-24` |
| **Reuse** | Pola daftar bertingkat beserta penyaringnya dari layar sejenis |
| **Scope** | Layar census; slice census |
| **Dependency** | `FE-RWI-007`; **`BE-RWI-016` sudah dapat dipanggil** |
| **Wewenang UI** | Kata untuk angka hari rawat adalah `RWI-FE-001` `DEV_DISCRETION`. **Batasnya:** wajib menyebut jelas bahwa itu **hitungan hari rawat**, bukan lama waktu sebenarnya |
| **Acceptance criteria** | 1. Census menampilkan nomor episode, nama pasien, lokasi, DPJP, perawat, lama dirawat, dan status. 2. **Tanpa diagnosis dan tanpa isi resume** — keduanya hanya pada detail bagi peran berhak. 3. Angka hari rawat tidak dapat disalahartikan sebagai jam. 4. Dapat disaring unit layanan dan kelas. 5. Pasien yang kepergiannya sudah dicatat tidak muncul |
| **Verification** | e2e penyaring; **pemeriksaan payload** untuk kriteria 2 — bukan hanya memeriksa yang tampil di layar |
| **Risk/blocker** | Kriteria 2 diperiksa pada **payload**, bukan pada tampilan. Data sensitif yang terkirim ke browser lalu disembunyikan CSS tetap bocor. Owner: Frontend bersama Security/Privacy — pemilik privasi belum ditunjuk, jadi aturan tertulis yang berlaku |
| **DoD** | Layar selesai; kelima kriteria lulus; pemeriksaan payload terlampir |
| **Status** | ✅ **Selesai.** Kelima kriteria lulus lewat tiga e2e di browser sungguhan beserta delapan test unit. Kriteria 2 dibuktikan pada **payload**: e2e menangkap body jawaban `GET /census` yang sampai ke browser dan memastikan tidak satu pun dari sembilan kolom klinis ada di sana, ditambah lapis kedua berupa daftar kolom yang diizinkan yang menjatuhkan kolom di luar `CensusItemResponse` sebelum data masuk pohon React ([laporan](../task/report/frontend/FE-RWI-008.md)) |

---

### `FE-RWI-009` — Satu episode terbaca utuh beserta riwayatnya

| Field | Isi |
| --- | --- |
| **Outcome** | Semua peran klinis dan admisi dapat melihat satu episode secara utuh: status, lokasi terkini, DPJP, perawat, kebutuhan isolasi, dan riwayatnya |
| **Trace** | `FE-INP-04`, `FE-INP-15` bagian DPJP; api contract `GET /episodes/{id}`, `/status-history`, `/doctor-assignments`, `/nurse-assignments`, `PATCH /{id}/isolation-requirement`; `GUARD-INP-04` |
| **Reuse** | Kerangka dari `FE-RWI-002` |
| **Scope** | Layar detail episode; tombol ubah kebutuhan isolasi beserta aturan tampilnya |
| **Dependency** | `FE-RWI-007`; **`BE-RWI-009` dan `BE-RWI-014` sudah dapat dipanggil** |
| **Wewenang UI** | Penggabungan dengan layar lain diperbolehkan |
| **Acceptance criteria** | 1. Lokasi terkini dibaca dari riwayat penempatan, bukan dari kolom pada episode. 2. **Tombol ubah kebutuhan isolasi berperilaku mengikuti status episode:** aktif bagi petugas admisi dan DPJP selagi `Draft`; begitu `Admitted`, **nonaktif** bagi petugas admisi disertai keterangan "Setelah pasien dirawat, kebutuhan isolasi hanya dapat diubah DPJP". 3. Bagi dokter yang bukan DPJP aktif, keterangannya "Anda bukan DPJP episode ini". 4. Keterangan kebutuhan isolasi hanya tampil bagi peran berhak, **tidak** pada daftar mana pun. 5. Riwayat status, DPJP, dan perawat terbaca urut |
| **Verification** | e2e visibilitas tombol untuk empat kombinasi: admisi+`Draft`, admisi+`Admitted`, DPJP aktif, dokter lain |
| **Risk/blocker** | Ini **satu-satunya** tombol pada modul yang kewenangannya berpindah mengikuti status episode. Mesin hak akses menjawab `SetIsolation` dengan "boleh" untuk petugas admisi **dan** dokter mana pun; yang membedakan dijaga service lewat `GUARD-INP-04`. Layar yang hanya membaca hak akses akan menampilkan tombol yang pasti ditolak server. Owner: Frontend |
| **DoD** | Layar selesai; kelima kriteria lulus; keempat kombinasi e2e lulus |
| **Status** | ✅ **Selesai.** Keempat kombinasi e2e lulus — admisi+`Draft`, admisi+`Admitted`, DPJP aktif, dan dokter lain — dan kedua keterangan yang berlawanan arah diperiksa tidak tertukar. Lokasi terkini terbukti dibaca dari riwayat penempatan: server tiruan sengaja mengisi kolom `currentLocation` episode dengan nilai berbeda, dan nilai itu tidak muncul sama sekali di layar ([laporan](../task/report/frontend/FE-RWI-009.md)) |

---

### `FE-RWI-010` — Pasien dapat dipindahkan, dan yang tidak berwenang melihatnya nonaktif

| Field | Isi |
| --- | --- |
| **Outcome** | Perpindahan dapat dikerjakan dari layar, dan dokter yang bukan DPJP episode itu melihat tombolnya nonaktif — bukan menekannya lalu ditolak |
| **Trace** | `FE-INP-05`; `03-frontend-architecture.md` bagian 3; `GUARD-INP-01`; api contract `POST /placements/transfer`; `EPIC RI-26`, `EPIC RI-34`; `UAT-08` |
| **Reuse** | Daftar tempat tidur dan penyaring yang **sama persis** dengan `FE-RWI-005` |
| **Scope** | Layar perpindahan; aturan tampil tombol pindah |
| **Dependency** | `FE-RWI-009`; **`BE-RWI-019` sudah dapat dipanggil** |
| **Wewenang UI** | Bentuk layar bebas |
| **Acceptance criteria** | 1. Tombol pindah hanya aktif bagi dokter yang merupakan **DPJP aktif episode tersebut**; bila bukan, tombol dinonaktifkan disertai keterangan "Anda bukan DPJP episode ini". 2. Perpindahan wajib disertai alasan medis. 3. Daftar tempat tidur tujuan memakai penyaring yang sama dengan layar penempatan — **tidak ada** daftar kedua. 4. Penolakan 422 karena jenis kelamin atau isolasi ditampilkan dengan pesan yang sama seperti penempatan. 5. Isian tidak hilang ketika ditolak |
| **Verification** | e2e tombol nonaktif bagi dokter bukan DPJP — membuktikan `GUARD-INP-01` terlihat di layar; e2e penolakan pencampuran lewat jalur perpindahan |
| **Risk/blocker** | Kriteria 3 mengulang pelajaran `FE-RWI-005`: dua daftar tempat tidur yang berbeda akan berselisih, dan jalur perpindahan justru yang paling sering dipakai petugas terburu-buru. Owner: Frontend |
| **DoD** | Layar selesai; kelima kriteria lulus; kedua skenario e2e lulus |
| **Status** | ✅ **Selesai.** Kedua skenario e2e lulus: tombol pindah nonaktif bagi dokter yang bukan DPJP — lengkap dengan tombol pilih tempat tidur yang tidak dirender sehingga tidak ada jalan memutar — dan penolakan pencampuran kamar lewat jalur perpindahan yang menampilkan kalimat server apa adanya. Daftar tempat tidur tujuan memakai hook dan komponen papan yang sama persis dengan layar penempatan ([laporan](../task/report/frontend/FE-RWI-010.md)) |

---

### `FE-RWI-011` — DPJP dan perawat penanggung jawab dapat dialihkan

| Field | Isi |
| --- | --- |
| **Outcome** | Kepala ruangan dapat mengalihkan DPJP saat dokter berhalangan dan menugaskan perawat penanggung jawab, keduanya disertai alasan dan tersimpan sebagai riwayat |
| **Trace** | `FE-INP-04` bagian penanggung jawab; api contract `POST /episodes/{id}/doctor-assignments` dan `/nurse-assignments`; `EPIC RI-25` |
| **Reuse** | Isian pilihan dokter dan pegawai dari hook pemilihan sumber daya |
| **Scope** | Aksi pengalihan DPJP dan penugasan perawat pada layar detail episode |
| **Dependency** | `FE-RWI-009`; **`BE-RWI-017` dan `BE-RWI-018` sudah dapat dipanggil** |
| **Wewenang UI** | Bebas |
| **Acceptance criteria** | 1. Hanya kepala ruangan dan supervisor melihat kedua aksi. 2. Pengalihan DPJP wajib beralasan; tanpa alasan ditolak dengan pesan jelas. 3. Riwayat DPJP dan perawat terbaca urut beserta periodenya. 4. Episode **tanpa** perawat penanggung jawab tetap dapat dibuka dan seluruh tindakan lain tetap tersedia |
| **Verification** | e2e per peran; e2e episode tanpa perawat yang membuktikan tidak ada tindakan tertahan |
| **Risk/blocker** | Kriteria 4 mudah dikerjakan terbalik menjadi peringatan yang memblokir. `RWI-DEC-032` memilih **tidak menahan**, karena penugasan perawat sering menyusul beberapa menit setelah pasien tiba. Owner: Frontend |
| **DoD** | Layar selesai; keempat kriteria lulus; test e2e ada |
| **Status** | ✅ **Selesai.** Keempat kriteria lulus. E2E per peran membuktikan kedua aksi **tidak dirender** bagi perawat, bukan dirender lalu dinonaktifkan; e2e episode tanpa perawat membuktikan tidak satu pun tindakan tertahan, sesuai `RWI-DEC-032`. Nama peran kepala ruangan tetap **asumsi** yang disalin dari `InpatientActorClaims` dan dicatat sebagai risiko terbuka ([laporan](../task/report/frontend/FE-RWI-011.md)) |

---

### `FE-RWI-012` — DPJP dapat menyatakan pasien boleh pulang dan menandatangani resume

| Field | Isi |
| --- | --- |
| **Outcome** | Keputusan pulang dan resume dikerjakan dalam satu alur, tanda tangan hanya dapat dibubuhkan DPJP, dan koreksi setelah tanda tangan menyimpan versi lamanya |
| **Trace** | `FE-INP-06`; `GUARD-INP-02`, `GUARD-INP-03`; api contract bagian discharge summary; `EPIC RI-27`; `UAT-10`, `UAT-27` |
| **Reuse** | Kerangka dari `FE-RWI-002` |
| **Scope** | Layar keputusan pulang dan resume; tampilan daftar versi resume |
| **Dependency** | `FE-RWI-011`; **`BE-RWI-020`, `BE-RWI-021`, dan `BE-RWI-022` sudah dapat dipanggil** |
| **Wewenang UI** | Bebas |
| **Acceptance criteria** | 1. Aksi menyatakan boleh pulang dan menandatangani resume hanya tampil bagi DPJP aktif. 2. Lima cara pulang tersedia dan dipilih sadar, bukan ada nilai bawaan yang tersimpan diam-diam. 3. Resume yang sudah ditandatangani tidak dapat disunting dari layar ini. 4. Daftar versi resume terbaca beserta nama penandatangan tiap versi. 5. Isi resume **tidak** tampil pada daftar episode maupun census |
| **Verification** | e2e per peran; e2e koreksi resume yang membuktikan versi lama tetap terbaca |
| **Risk/blocker** | Kriteria 2: cara pulang yang punya nilai bawaan akan tersimpan salah pada pasien yang petugasnya terburu-buru, dan dua di antaranya — meninggal dan kabur — aturan klinisnya bahkan **belum disahkan** pemilik klinis. Owner: Frontend bersama Product/Domain |
| **DoD** | Layar selesai; kelima kriteria lulus; test e2e ada |
| **Status** | 🟡 **Selesai, satu kriteria tertahan di luar frontend.** Empat kriteria lulus penuh. Kedua aksi **tidak dirender** bagi lima peran lain — termasuk supervisor dan kepala ruangan — bukan dirender lalu dinonaktifkan; resume tertandatangani terkunci bahkan bagi DPJP aktif sendiri; dua versi resume yang dikirim terbalik terbaca urut beserta nama penandatangan tiap versi; dan penjaga payload census diperluas sehingga kini mencakup ketujuh kolom isi resume, bukan enam. **Kriteria 2 baru terpenuhi sebagian:** bagian "dipilih sadar" lulus — tidak ada nilai bawaan dan permintaan tanpa pilihan ditolak sebelum dialog konfirmasi tampil — sedangkan bagian "lima cara pulang" tertahan `RWI-OQ-039` dan `RWI-DEC-059` yang berstatus `draft`, butir yang **sudah** dicatat `BE-RWI-020`. Layar menyebut kedua cara pulang yang belum tersedia beserta jalan keluar supervisornya, bukan menyembunyikannya. Perubahan **masih lokal, belum di-commit** ([laporan](../task/report/frontend/FE-RWI-012.md)) |

---

### `FE-RWI-013` — Kasir dapat menandai kelayakan keuangan

| Field | Isi |
| --- | --- |
| **Outcome** | Kasir menandai `Cleared` atau `Blocked` beserta catatannya, dan penandaan itu menjadi gerbang penutupan episode |
| **Trace** | `FE-INP-08`; `RWI-DEC-040`; api contract `POST .../financial-clearance`; `EPIC RI-28` |
| **Reuse** | Kerangka dari `FE-RWI-002` |
| **Scope** | Layar penandaan kelayakan keuangan |
| **Dependency** | `FE-RWI-011`; **`BE-RWI-024` sudah dapat dipanggil** |
| **Wewenang UI** | Bebas |
| **Acceptance criteria** | 1. Hanya peran kasir dan billing yang dapat membukanya. 2. Catatan **wajib**; tanpa catatan tidak dapat disimpan. 3. Pelaku dan waktu penandaan terbaca pada layar detail. 4. Tiga nilai tersedia dan artinya terbaca jelas oleh petugas non-teknis |
| **Verification** | e2e per peran; e2e penandaan tanpa catatan |
| **Risk/blocker** | Penandaan ini **manual** dan bukan cerminan tagihan sebenarnya — `RWI-RISK-003`. Layar tidak boleh memberi kesan angkanya berasal dari sistem billing. Owner: Frontend bersama Product/Domain |
| **DoD** | Layar selesai; keempat kriteria lulus; test e2e ada |
| **Status** | 🟡 **Selesai, satu kriteria tertahan kontrak.** Kriteria 1, 2, dan 4 lulus. Aksi penandaan **tidak dirender** bagi petugas admisi, perawat, kepala ruangan, dan DPJP; penandaan tanpa catatan ditolak dengan nol permintaan terkirim; ketiga nilai punya penjelasan bebas istilah teknis di layar. Peringatan `RWI-RISK-003` dirender tanpa syarat dan tidak hilang bahkan ketika nilainya sudah lunas. **Kriteria 3 baru terpenuhi sebagian:** kontrak `0.4.0` tidak menyediakan `GET .../financial-clearance` — method service-nya **sudah ada** di `InpDischargeService.Closure.cs:222` tetapi tidak pernah dipasang sebagai aksi controller — sehingga riwayat penandaan baru terbaca setelah ada penandaan baru dikirim dari layar itu. Layar menyatakan batasan itu apa adanya, bukan menampilkan daftar kosong yang terbaca seolah belum pernah ditandai. Satu delta lain dicatat: matriks perpindahan bagian 4 hanya mengenal dua tindakan, sehingga `Pending` terbaca tetapi tidak dapat ditandai. Perubahan **masih lokal, belum di-commit** ([laporan](../task/report/frontend/FE-RWI-013.md)) |

---

### `FE-RWI-014` — Kelima syarat penutupan tampil lengkap, dan jalan keluar supervisor tetap sempit

| Field | Isi |
| --- | --- |
| **Outcome** | Petugas yang gagal menutup episode melihat kelima syarat beserta tanda sudah atau belum, sehingga tahu apa yang harus dikejar. Jalan keluar supervisor **tidak** tampil berdampingan seolah dua pilihan setara |
| **Trace** | `FE-INP-07`; `03-frontend-architecture.md` bagian 3 dan 5.4; api contract `/closure-readiness`, `/close`, `/close-with-override`; `EPIC RI-28`; `UAT-11`, `UAT-12` |
| **Reuse** | Penanganan 422 dari `FE-RWI-007` |
| **Scope** | Layar penutupan; penandaan butir administrasi; tombol tutup menembus gerbang |
| **Dependency** | `FE-RWI-013`; **`BE-RWI-025` dan `BE-RWI-026` sudah dapat dipanggil** |
| **Wewenang UI** | Bentuk daftar syarat bebas, selama kelimanya terbaca |
| **Acceptance criteria** | 1. Kelima syarat tampil beserta tanda sudah atau belum, **selalu** — bukan hanya ketika penutupan ditolak. 2. Tombol tutup menembus gerbang keuangan **tidak** ditampilkan berdampingan dengan tombol tutup biasa. Ia muncul **hanya setelah** tombol tutup biasa ditolak karena kelayakan keuangan, dan **hanya** untuk supervisor. 3. Menembus gerbang wajib beralasan. 4. Butir administrasi dapat ditandai dari layar ini. 5. Setelah penutupan berhasil, tempat tidur terbaca kembali kosong pada papan ketersediaan |
| **Verification** | e2e penolakan yang menampilkan daftar syarat; e2e yang membuktikan tombol override **tidak ada** sebelum penolakan; e2e per peran |
| **Risk/blocker** | Kriteria 2 adalah aturan keras, bukan preferensi tata letak. Jalan keluar yang selalu terlihat akan menjadi jalur normal dalam hitungan minggu, dan kelima syarat kehilangan arti. Owner: Frontend bersama Product/Domain |
| **DoD** | Layar selesai; kelima kriteria lulus; ketiga skenario e2e lulus |
| **Status** | ✅ **Selesai, kelima kriteria lulus.** Kelima syarat dirender sejak layar dibuka — bukan hanya ketika penutupan ditolak — masing-masing beserta tanda "Sudah" atau "Belum" sebagai teks, kalimat penolakan server apa adanya, dan keterangan siapa yang dapat memenuhinya. Jalan keluar supervisor **tidak dirender sama sekali** sampai penutupan biasa ditolak karena kelayakan keuangan, dan hanya bagi supervisor: e2e membuktikan supervisor tidak melihatnya walau syarat keuangan sudah terbaca "Belum" sejak layar dibuka, sedangkan petugas admisi, perawat, dan kepala ruangan tetap tidak melihatnya bahkan sesudah penolakan. Butir administrasi dapat ditandai dari layar ini, dan syarat ketiga ikut dibaca ulang sesudahnya. **Satu cacat perilaku ditemukan lewat verifikasi manual dan diperbaiki:** footer aplikasi yang berposisi `fixed` menelan penekanan tombol tutup, dan lint/test/build semuanya hijau saat itu. **Dua delta hak akses dicatat:** penandaan butir administrasi memakai `InpatientDischarge : Update` yang sama persis dengan penyusunan resume sehingga server tidak dapat menolak DPJP, dan tidak ada daftar nama peran "petugas admisi" di backend sehingga tombol tutup biasa tidak dapat disembunyikan dari perawat. Perubahan **masih lokal, belum di-commit** ([laporan](../task/report/frontend/FE-RWI-014.md)) |

---

### `FE-RWI-015` — Petugas dapat mencatat pasien sudah meninggalkan ruangan

| Field | Isi |
| --- | --- |
| **Outcome** | Tempat tidur bebas sejak pasien benar-benar pergi, tanpa menunggu urusan administrasi selesai |
| **Trace** | `FE-INP-14`; `RWI-DEC-055`; `RWI-RULE-036`; api contract `POST .../record-departure`; `EPIC RI-28`; `UAT-24`, `UAT-25` |
| **Reuse** | Kerangka dari `FE-RWI-002` |
| **Scope** | Aksi pencatatan kepergian |
| **Dependency** | `FE-RWI-014`; **`BE-RWI-027` sudah dapat dipanggil** |
| **Wewenang UI** | Penempatan tombol adalah `DEV_DISCRETION`. **Batasnya:** konfirmasinya wajib menyebut bahwa tindakan **tidak dapat dibatalkan** |
| **Acceptance criteria** | 1. Aksi tersedia bagi petugas admisi, perawat, kepala ruangan, dan supervisor; **tidak** bagi DPJP. 2. Konfirmasi menyebut bahwa tindakan tidak dapat dibatalkan. 3. Setelah dicatat, tempat tidur terbaca kosong pada papan ketersediaan **tanpa** episode ditutup. 4. Pasien hilang dari census tetapi episodenya tetap muncul pada daftar pantau penutupan tertunda. 5. Mencatat pada episode yang belum diputuskan pulang ditolak, dan pesannya menjelaskan urutannya |
| **Verification** | e2e per peran; e2e yang memeriksa papan ketersediaan dan census setelah pencatatan |
| **Risk/blocker** | Kriteria 4 melawan intuisi: pasien hilang dari census tetapi episodenya masih hidup. Layar harus membuat keadaan itu terbaca, bukan membuat petugas mengira episodenya sudah selesai. Owner: Frontend |
| **DoD** | Layar selesai; kelima kriteria lulus; test e2e ada |
| **Status** | 🟡 **Selesai, satu kriteria tertahan task berikutnya.** Kriteria 1, 2, 3, dan 5 lulus penuh. Aksinya **tidak dirender** bagi DPJP maupun dokter lain — permission matrix bagian 3 menuliskan dokter tanpa `RecordDeparture` — sedangkan petugas admisi, perawat, kepala ruangan, dan supervisor melihatnya. Konfirmasinya menyebut bahwa tindakan tidak dapat dibatalkan, dan detail episode dibaca ulang sebelum dialognya tampil. Sesudah pencatatan, status episode **tetap** Menunggu pulang sementara tempat tidurnya terbaca kosong pada papan ketersediaan. Episode `Admitted` ditolak di layar dengan kalimat urutan yang disalin apa adanya dari service, dan nol permintaan terkirim. **Kriteria 4 baru terpenuhi sebagian:** keterbacaannya lulus — layar menyatakan pasien hilang dari census, episodenya tetap pada daftar pantau penutupan tertunda, dan masih wajib ditutup — tetapi daftar pantau itu sendiri adalah `FE-RWI-016`, yang **kini sudah dikerjakan**: daftar penutupan tertunda ada, dan barisnya membedakan tempat tidur yang masih ditahan dari yang sudah bebas karena kepergiannya tercatat ([laporan `FE-RWI-016`](../task/report/frontend/FE-RWI-016.md)). Kriteria 4 karena itu **siap dinaikkan penuh** oleh pemilik pekerjaan. Aksinya sengaja diletakkan pada detail episode, bukan pada layar penutupan, karena perawat pelaksana tidak punya `InpatientDischarge : Read`. Perubahan **masih lokal, belum di-commit** ([laporan](../task/report/frontend/FE-RWI-015.md)) |

---

### `FE-RWI-016` — Empat daftar pantau tersedia dan tidak menghalangi tindakan

| Field | Isi |
| --- | --- |
| **Outcome** | Kepala ruangan dan supervisor punya daftar yang menunjukkan apa yang perlu ditindaklanjuti, tanpa satu pun daftar itu menahan pekerjaan siapa pun |
| **Trace** | `FE-INP-09`; `RWI-FE-002` `DEV_DISCRETION`; `03-frontend-architecture.md` bagian 4.4; api contract bagian Monitoring; `EPIC RI-29`, `EPIC RI-34`; `UAT-33` |
| **Reuse** | Pola daftar bertingkat |
| **Scope** | Layar daftar pantau: penutupan tertunda, penutupan menembus gerbang, episode tanpa perawat, dan **penempatan tidak sesuai isolasi** |
| **Dependency** | `FE-RWI-015`; **`BE-RWI-029` sudah dapat dipanggil** |
| **Wewenang UI** | `RWI-FE-002` `DEV_DISCRETION`: satu halaman gabungan atau beberapa halaman terpisah, urutan kolom, cara menandai keterlambatan, dan penempatan menu semuanya bebas. **Batasnya:** lama keterlambatan wajib terbaca, dan daftar **tidak boleh** menghalangi tindakan apa pun |
| **Acceptance criteria** | 1. Keempat daftar tersedia. 2. Lama keterlambatan terbaca pada tiga daftar pertama. 3. Daftar **penempatan tidak sesuai** menampilkan tindakan berikutnya — memindahkan pasien — dan **nadanya tidak menuduh**, karena isinya akibat wajar perubahan kondisi klinis, bukan kelalaian petugas. 4. Daftar kosong menampilkan keadaan kosong yang jelas, bukan galat. 5. Membuka daftar tidak menahan satu pun tindakan di layar lain |
| **Verification** | e2e keempat daftar; e2e keadaan kosong |
| **Risk/blocker** | Kriteria 3 adalah pembeda daftar keempat dari tiga lainnya. Menyeragamkan nadanya menjadi "keterlambatan" akan membuat perawat merasa dituduh atas perubahan kondisi pasien yang bukan salahnya. Owner: Frontend bersama Clinical governance |
| **DoD** | Layar selesai; kelima kriteria lulus; test e2e ada |
| **Status** | ✅ **Selesai, kelima kriteria lulus.** Keempat daftar berdiri pada satu halaman bertab `/inpatient-management/monitoring`, dan kunci tiap tab sama persis dengan potongan path endpointnya. Lama keterlambatan terbaca pada ketiga daftar pertama: daftar penutupan tertunda memakai `PendingHours` **dari server** beserta ambangnya, sedangkan dua daftar lain diturunkan dari waktu yang dikirim server karena DTO-nya memang tidak punya kolom lama — delta itu dicatat, bukan ditutupi. Daftar penempatan tidak sesuai **tidak punya kolom keterlambatan sama sekali**, membawa tindakan berikutnya "Pindahkan Pasien" pada tiap baris, dan menyangkal nada tuduhan dengan kalimat yang dikunci test. Keempat daftar kosong menampilkan kalimat yang berbeda dan menjelaskan kenapa kosong. Kriteria 5 dibuktikan dua arah: **nol** permintaan selain `GET` dari halaman ini, dan alur perpindahan pada detail episode terbukti berjalan penuh sesudah keempat daftar dibuka. **Satu delta dicatat:** kolom pelaku penutupan menembus gerbang sengaja **tidak ditampilkan** sampai sumbernya diputuskan — `BE-RWI-029` bagian 6.1. Perubahan **masih lokal, belum di-commit** ([laporan](../task/report/frontend/FE-RWI-016.md)) |

---

### `FE-RWI-017` — Admin dapat menemukan tempat tidur yang statusnya menyimpang

| Field | Isi |
| --- | --- |
| **Outcome** | Selisih antara salinan status tempat tidur dan catatan penempatan tidak lagi menyimpang diam-diam, karena ada layar yang menampilkannya |
| **Trace** | `FE-INP-10`; `RWI-DEC-039`; `RWI-RULE-027`; api contract `GET /monitoring/bed-drift`; `EPIC RI-29`; `UAT-21` |
| **Reuse** | Pola daftar bertingkat |
| **Scope** | Layar laporan selisih tempat tidur |
| **Dependency** | `FE-RWI-015`; **`BE-RWI-029` sudah dapat dipanggil** |
| **Wewenang UI** | Bebas |
| **Acceptance criteria** | 1. Menampilkan tempat tidur yang salinan statusnya tidak cocok dengan catatan penempatan. 2. Setiap baris menyebut kedua nilai yang berselisih, bukan hanya menyatakan ada selisih. 3. Hanya admin dan supervisor yang dapat membukanya. 4. Daftar kosong menampilkan keadaan kosong yang jelas |
| **Verification** | e2e dengan selisih yang dibuat sengaja; e2e keadaan kosong |
| **Risk/blocker** | Laporan ini adalah satu-satunya pengawas atas satu-satunya arah tulis lintas modul. Bila tidak pernah dibuka siapa pun, ia tidak berguna. Ini soal proses, bukan kode — pastikan ada peran yang bertugas membacanya. Owner: Frontend bersama Product/Domain |
| **DoD** | Layar selesai; keempat kriteria lulus; test e2e ada |
| **Status** | ✅ **Selesai, keempat kriteria lulus — satu dengan batas bukti yang dinyatakan.** Layar `/inpatient-management/bed-drift` menampilkan dua selisih berarah berlawanan yang dibuat sengaja, dan setiap baris menyebut **kedua** nilainya beserta sumber selisihnya. Nilai status diterjemahkan dari **angkanya**, sehingga nama enum `Available` dan `Occupied` terbukti tidak pernah sampai ke layar. Daftar kosong menampilkan keadaan kosong, bukan galat. **Kriteria 3 lulus di layar tetapi tidak dapat ditegakkan server:** `GET /monitoring/bed-drift` dijaga `InpatientMonitoring : Read` — butir yang sama persis dengan keempat daftar pantau lain, dan permission matrix bagian 3 memberikannya juga kepada petugas admisi serta kepala ruangan. Layar menutup halaman bagi keempat peran itu dan **tidak mengirim permintaan sama sekali**, tetapi batas sesungguhnya perlu butir hak akses tersendiri. "Admin" dipetakan ke `SuperAdmin`, karena peran Admin master data justru tidak punya `InpatientMonitoring : Read`. Perubahan **masih lokal, belum di-commit** ([laporan](../task/report/frontend/FE-RWI-017.md)) |

---

### `FE-RWI-018` — Supervisor dapat membetulkan catatan lewat sesi koreksi

| Field | Isi |
| --- | --- |
| **Outcome** | Kesalahan pada episode yang sudah ditutup dapat dibetulkan lewat jalur resmi yang meninggalkan jejak, bukan lewat penyuntingan diam-diam |
| **Trace** | `FE-INP-11`; `RWI-DEC-028`; `RWI-RULE-020`; api contract bagian correction sessions; `EPIC RI-30`; `UAT-14`, `UAT-15` |
| **Reuse** | Kerangka dari `FE-RWI-002` |
| **Scope** | Layar sesi koreksi |
| **Dependency** | `FE-RWI-015`; **`BE-RWI-030` sudah dapat dipanggil** |
| **Wewenang UI** | Bebas |
| **Acceptance criteria** | 1. Hanya supervisor yang melihat aksi membuka sesi koreksi. 2. Selama sesi berjalan, layar menunjukkan bahwa status episode **tetap** `Closed`. 3. Menutup sesi wajib menyertakan daftar perubahannya. 4. Koreksi resume yang sudah ditandatangani menampilkan peringatan bahwa versi lama akan disimpan. 5. Satu episode tidak dapat punya dua sesi terbuka |
| **Verification** | e2e per peran; e2e koreksi resume |
| **Risk/blocker** | Kriteria 2 penting supaya supervisor tidak mengira episode terbuka kembali dan tempat tidurnya kembali. Sesi koreksi **tidak** mengembalikan tempat tidur dan **tidak** menambah hari rawat. Owner: Frontend |
| **DoD** | Layar selesai; kelima kriteria lulus; test e2e ada |
| **Status** | ✅ **Selesai, kelima kriteria lulus — satu dengan batas bukti yang dinyatakan.** Layar `/episodes/{id}/correction` berdiri, dan seluruh aksinya **tidak dirender** bagi peran selain supervisor — termasuk bagi DPJP aktif episode itu, dan termasuk tautan masuknya pada detail episode. Kriteria 2 dibuktikan dari data, bukan dari kalimat: detail episode **dibaca ulang dari server** sesudah sesi dibuka, dan statusnya terbaca tetap Sudah ditutup. Menutup sesi tanpa daftar perubahan ditolak di layar dengan kalimat servernya dan **nol** permintaan terkirim. Koreksi resume tertandatangani memunculkan peringatan versi lama, dialog konfirmasinya mengulanginya, dan sesudah tersimpan versi lamanya terbaca beserta nama penandatangan lamanya. **Kriteria 5 punya batas bukti:** kontrak `0.4.0` tidak menyediakan `GET .../correction-sessions`, sehingga sesi terbuka milik supervisor lain tidak dapat dibaca layar — penjaganya tetap server, dan layar menyatakan batas itu apa adanya. **Satu cacat perilaku ditemukan lewat verifikasi manual dan diperbaiki:** footer `fixed` menelan penekanan tombol pada dua kasus uji, pengulangan cacat yang sama dengan `FE-RWI-014`. Perubahan **masih lokal, belum di-commit** ([laporan](../task/report/frontend/FE-RWI-018.md)) |

---

### `FE-RWI-019` — Layar terbukti hanya dijangkau peran yang berhak

| Field | Isi |
| --- | --- |
| **Outcome** | Setiap layar Rawat Inap terbukti tertutup bagi peran yang tidak berhak, dan tiga aturan tombol yang menyentuh kewenangan terbukti terlihat di layar — bukan hanya ditegakkan server |
| **Trace** | `03-frontend-architecture.md` bagian 10; `RWI-DEC-051`; `GUARD-INP-01` s.d. `GUARD-INP-04`; permission matrix `0.3.0` |
| **Reuse** | `tests/e2e/route-smoke.spec.mjs` yang **sudah ada** — menambah kasus, bukan membuat kerangka baru |
| **Scope** | Kasus baru pada rangkaian e2e yang sudah ada |
| **Dependency** | Seluruh task `FE-RWI-001` s.d. `FE-RWI-018` |
| **Wewenang UI** | Tidak ada |
| **Acceptance criteria** | 1. Setiap layar Rawat Inap dapat dijangkau peran yang berhak dan **tidak** dapat dijangkau yang tidak berhak. 2. Tombol pindah nonaktif bagi dokter yang bukan DPJP aktif — `GUARD-INP-01`. 3. Tombol ubah kebutuhan isolasi berperilaku sesuai status episode — `GUARD-INP-04`. 4. Daftar syarat penutupan tampil lengkap saat 422. 5. Perbaikan tombol tempat tidur punya test-nya sendiri |
| **Verification** | Jalankan rangkaian e2e penuh; lampirkan keluarannya apa adanya |
| **Risk/blocker** | Frontend hari ini hanya punya **empat** berkas test, tidak satu pun menyentuh tempat tidur. Task ini adalah pertama kalinya layar tempat tidur punya penjaga. Jangan perlakukan sebagai formalitas penutup. Owner: Frontend |
| **DoD** | Kelima kriteria lulus; keluaran rangkaian e2e terlampir; tidak ada kasus yang ditandai dilewati |

---

## 5. Gerbang yang masih terbuka

| Gerbang | Keadaannya | Menahan |
| --- | --- | --- |
| **Approval blueprint** | `approved_by` kosong | **Seluruh task** |
| Endpoint backend tersedia | Ke-49 endpoint baru berstatus "Rencana" | Seluruh task kecuali `FE-RWI-001` dan `FE-RWI-002` |
| Security/privacy owner | Belum ditunjuk | Tidak menahan; aturan privasi yang sudah tertulis tetap berlaku dan tetap diuji |
| Kesiapan data master | `RWI-DEC-063`, target 22 Agustus 2026 | `FE-RWI-005` ke atas tidak dapat diuji dengan data nyata |

---

## 6. Yang sengaja tidak ada di roadmap ini

| Yang tidak dikerjakan | Alasan |
| --- | --- |
| Layar pengkajian, catatan dokter, CPPT, dan resep | Slice di luar scope MVP — `DEC-INP-001` |
| Layar serah terima IGD | Di luar scope — `DEC-INP-002` |
| Layar persetujuan umum rawat inap | Di luar scope — `DEC-INP-003` |
| Daftar pantau kepatuhan pengkajian dan CPPT | Bergantung pada slice yang di luar scope — `DEC-INP-001` |
| Menyalin ruang kerja antrean dokter | Pasien rawat inap tidak punya antrean; menyalinnya membawa masalah yang sama |
| Menyaring ulang tempat tidur di sisi layar | Aturan Kelayakan Penempatan hanya boleh ada **satu**, dan tempatnya di server |
