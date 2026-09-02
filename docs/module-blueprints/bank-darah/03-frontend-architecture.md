# Bank Darah — Frontend Architecture

| Field | Value |
| --- | --- |
| Blueprint ID | `BD-BP-001` · Contract version `v1` — `draft` |
| Owner | Pemilik proses BDRS · pemilik proses klinis (pembeda golongan darah) |
| `approved_by` / `approved_at` | Kosong — `draft` |
| Sumber | `contracts/api-contract.md` · `contracts/permission-audit-matrix.md` · `00-interview-decisions.md` §7 (`FE-BD-001`..`009`) |
| Frontend SHA | `afbb8ab47a6a309f24cdaf6d72024f0dc1b2c254` cabang `sukmagpV2` |

Revisi ini memfokuskan kedalaman pada backend; frontend mengunci **keberadaan layar dan sumber
datanya**, bukan rupanya. Seluruh layar Bank Darah **baru** (`BD-CAP-020` `Missing`) dan memakai
komponen dasar yang sudah ada (`BD-CAP-021`) — dilarang membuat komponen dasar tandingan.

Hierarki wewenang: keamanan/privasi/invariant → brief produk yang disetujui → konvensi frontend V2 →
`DEV_DISCRETION`.

---

## 1. Kebutuhan layar

| ID layar | Nama | Jenis | Butir hak akses utama |
| --- | --- | --- | --- |
| `FE-BD-01` | Daftar Order Darah (daftar kerja #1) | Daftar | `BloodOrder : Read` |
| `FE-BD-02` | Kerja Order Darah (detail + buat + batal + pemenuhan) | Kerja per entity | `BloodOrder : Read/Create/Update` |
| `FE-BD-03` | Daftar & Kerja Permintaan PMI (buat, catat penerimaan) | Daftar + kerja | `BloodProviderRequest : *` |
| `FE-BD-04` | Daftar Kantong Darah (+ saringan `PendingReview` #2 dan tunggakan bukti darurat #3) | Daftar | `BloodUnit : Read` |
| `FE-BD-05` | Kerja Kantong (alokasi, bukti, pemberian, darurat, koreksi, penyelesaian) | Kerja per entity | `BloodUnit : *` |
| `FE-BD-06` | Pemeriksaan Golongan Darah (sampel, hasil, validasi, penyelesaian konflik) | Kerja per entity | `BloodGroupExam : *` |
| `FE-BD-07` | Daftar Tindakan Bank Darah | Daftar + kerja | `BloodBankProcedure : *` |
| `FE-BD-08` | Setup — Katalog Komponen Darah | Master CRUD | `BloodComponent : *` |
| `FE-BD-09` | Setup — Daftar Alasan Terkendali | Master CRUD | `BloodBankReason : *` |

Tiga daftar kerja MVP (`DEC-BD-023`): `FE-BD-01`, saringan `PendingReview` pada `FE-BD-04`
(`FE-BD-002`), dan saringan tunggakan bukti darurat pada `FE-BD-04` (`FE-BD-004`). Bukan modul laporan.

---

## 2. Peta butir menu

Yang mengikat: layar mana dapat butir menu, route tujuannya, kedalaman nesting. Nama butir, urutan,
ikon, dan pengelompokan visual tetap `DEV_DISCRETION` (`FE-BD-006`). Route final mengikuti konvensi V2;
berkas yang disunting `src/utils/menu-sidebar/menu-items.jsx`.

```text
Bank Darah                               <- tingkat 0
├── Order Darah                          -> /health-services/blood-bank-management/blood-orders
├── Permintaan PMI                       -> .../provider-requests
├── Kantong Darah                        -> .../blood-units
├── Pemeriksaan Golongan Darah           -> .../blood-group-exams
├── Tindakan Bank Darah                  -> .../blood-bank-procedures
└── Setup                                <- grup tingkat 1
    ├── Katalog Komponen Darah           -> /health-services/master-data/blood-components
    └── Daftar Alasan Terkendali         -> .../master-data/blood-bank-reasons
```

| Butir menu | Tingkat | Induk | Layar | Butir hak akses | Status |
| --- | :---: | --- | --- | --- | --- |
| Bank Darah | 0 | — | — | — | Baru |
| Order Darah | 1 | Bank Darah | `FE-BD-01` | `BloodOrder : Read` | Baru |
| Permintaan PMI | 1 | Bank Darah | `FE-BD-03` | `BloodProviderRequest : Read` | Baru |
| Kantong Darah | 1 | Bank Darah | `FE-BD-04` | `BloodUnit : Read` | Baru |
| Pemeriksaan Golongan Darah | 1 | Bank Darah | `FE-BD-06` | `BloodGroupExam : Read` | Baru |
| Tindakan Bank Darah | 1 | Bank Darah | `FE-BD-07` | `BloodBankProcedure : Read` | Baru |
| Setup | 1 | Bank Darah | — | — | Baru |
| Katalog Komponen Darah | 2 | Setup | `FE-BD-08` | `BloodComponent : Read` | Baru |
| Daftar Alasan Terkendali | 2 | Setup | `FE-BD-09` | `BloodBankReason : Read` | Baru |

**Layar anak (tanpa butir menu sendiri):** `FE-BD-02` dicapai dari baris `FE-BD-01`; `FE-BD-05` dicapai
dari baris `FE-BD-04`. Pendaftaran butir menu **wajib** menjadi acceptance criteria salah satu task
layar, bukan pekerjaan menganggur.

---

## 3. Skema fitur per layar

Skema mengunci isi & sumber data; warna/jarak/ikon/tab/modal tetap `DEV_DISCRETION`. Skema **tidak**
memuat field yang tidak ada pada response `contracts/api-contract.md`.

### `FE-BD-01` Daftar Order Darah

```text
+- Daftar Order Darah -------------------------------------- FE-BD-01 -+
| [cari no. order / nama pasien]   [Unit v] [Komponen v] [Status v]    |
+---------------------------------------------------------------------+
| No. Order | Pasien | Unit | Komponen | Diminta/Diberikan | Status |  |
| --------- | ------ | ---- | -------- | ---------------- | chip  |[Detail]|
+---------------------------------------------------------------------+
| memuat -> kerangka baris                                             |
| kosong -> "Belum ada order darah pada saringan ini." [Atur ulang]   |
| gagal  -> "Data gagal dimuat." [Coba lagi]                          |
+- Halaman 1 dari n -------------------- [< Sebelumnya] [Berikutnya >]+
```

| Wilayah | Isi | Sumber data | Hak akses | Bila kosong/gagal |
| --- | --- | --- | --- | --- |
| Saringan | Cari, unit, komponen, status | `GET /blood-orders` (query) | `BloodOrder : Read` | — |
| Tabel | No. order, pasien, unit, komponen, pemenuhan, status | `GET /blood-orders` | `BloodOrder : Read` | Kosong/gagal seperti skema |
| Tombol "Buat Order" | Buka `FE-BD-02` mode buat | — | `BloodOrder : Create` | Disembunyikan bila tak berhak |

### `FE-BD-02` Kerja Order Darah

| Wilayah | Isi | Sumber data | Hak akses | Bila kosong/gagal |
| --- | --- | --- | --- | --- |
| Kepala | No. order, pasien, kunjungan, unit, dokter, status | `GET /blood-orders/{id}` | `BloodOrder : Read` | Gagal → seluruh layar diganti pesan + coba lagi |
| Form buat (elektronik/manual) | Pasien, kunjungan, komponen+jumlah, dokter, (pelaku input bila manual) | `POST /` · `POST /manual` | `BloodOrder : Create` | Order ganda → tampil tahan + minta alasan (`FE-BD-003`); `POST /confirm-duplicate` |
| Ringkasan pemenuhan | Diminta/diberikan/sisa (menghormati koreksi) | `GET /blood-orders/{id}/fulfillment` | `BloodOrder : Read` | Kosong → "Belum ada pemberian." |
| Tombol Batalkan | Batal + pilih alasan | `POST /{id}/cancel` | `BloodOrder : Update` | Alasan wajib dari daftar |

**`FE-BD-001` (mengikat):** golongan darah **diminta** wajib terlihat jelas berbeda dari golongan darah
**hasil pemeriksaan**. Bukan kebebasan pengembang.

### `FE-BD-04` Daftar Kantong Darah (+ dua daftar kerja)

| Wilayah | Isi | Sumber data | Hak akses | Bila kosong/gagal |
| --- | --- | --- | --- | --- |
| Saringan status | Termasuk preset `PendingReview` (#2) dan `emergencyPendingEvidence` (#3) | `GET /blood-units?status=` / `?emergencyPendingEvidence=true` | `BloodUnit : Read` | — |
| Tabel | No. kantong, komponen, asal permintaan, status, penanda berlebih/darurat | `GET /blood-units` | `BloodUnit : Read` | Kosong/gagal seperti skema daftar |

**`FE-BD-002` & `FE-BD-004` (mengikat):** daftar `PendingReview` dan daftar tunggakan bukti darurat
**wajib ada**. Bentuk tampilan bebas.

### `FE-BD-05` Kerja Kantong

| Wilayah | Isi | Sumber data | Hak akses | Bila kosong/gagal |
| --- | --- | --- | --- | --- |
| Kepala | No. kantong, komponen, status, asal permintaan | `GET /blood-units/{id}` | `BloodUnit : Read` | Gagal → pesan + coba lagi |
| Tombol Alokasikan / Batalkan alokasi | Pilih baris order / alasan | `POST /{id}/allocate` · `/cancel-allocation` | `BloodUnit : Allocate` | Sudah dialokasikan → tombol/aksi ditolak dengan pesan terbaca |
| Tombol Catat bukti kecocokan | Terhadap pasien tujuan | `POST /{id}/compatibility-evidence` | `BloodUnit : Compatibility` | — |
| Tombol Berikan | Gerbang: bukti untuk pasien tujuan & belum lewat masa berlaku | `POST /{id}/issue` | `BloodUnit : Issue` | Penanda **bukti lewat masa berlaku** wajib terlihat sebelum tombol ditekan (`FE-BD-008`) |
| Tombol Jalur darurat | Otorisasi + alasan | `POST /{id}/emergency-issue` | `BloodUnit : EmergencyIssue` | **Wajib tampak jelas sebagai jalur tidak normal**, bukan tombol setara (`FE-BD-005`) |
| Tombol Koreksi pencatatan | Apa keliru/benar + alasan | `POST /{id}/correction` | `BloodUnit : Correct` | Tersembunyi bila tak berhak |
| Tombol Penyelesaian (`PendingReview`) | Alihkan / kembalikan / tidak layak | `POST /{id}/reallocate|return-to-provider|mark-not-usable` | `BloodUnit : Resolve` | Alasan wajib |
| Riwayat kantong | Alokasi, bukti, pemberian, koreksi, penyelesaian | `GET /blood-units/{id}` | `BloodUnit : Read` | Kosong → "Belum ada pergerakan." |

### `FE-BD-06` Pemeriksaan Golongan Darah

| Wilayah | Isi | Sumber data | Hak akses | Bila kosong/gagal |
| --- | --- | --- | --- | --- |
| Kepala + status golongan darah sah pasien | Hasil sah / **penanda konflik** | `GET /blood-group-exams/patient/{patientId}/valid` | `BloodGroupExam : Read` | Konflik → penanda wajib terlihat & menahan pemakaian (`FE-BD-007`) |
| Catat sampel / hasil | Identifier sampel, ABO, Rhesus | `POST /` · `POST /{id}/result` | `BloodGroupExam : Create/Update` | Pemeriksa/waktu wajib |
| Tombol Validasi | Validasi hasil | `POST /{id}/validate` | `BloodGroupExam : Validate` | Tersembunyi bila bukan validator |
| **Penyelesaian konflik** | Histori hasil, hasil ulang, tindakan validator | `POST /conflict-resolution` (menunjuk pemeriksaan ulang) | `BloodGroupExam : Validate` | **Di layar ini, bukan daftar kerja keempat** (`FE-BD-009`, `DEC-BD-033`) |

### `FE-BD-08` / `FE-BD-09` Setup master

Layar CRUD sederhana memakai `base-editor-form.jsx` + `data-table.jsx`. `FE-BD-08` memuat kolom
`CompatibilityEvidenceValidityHours` per komponen. `FE-BD-09` memuat `ReasonCategory`. Keduanya
memakai pola master yang sudah ada; digambar sekali, dirujuk.

---

## 4. Aksi per peran

Diturunkan dari `contracts/permission-audit-matrix.md` (bukan dikarang ulang). Tombol yang tak berhak
**MUST disembunyikan**, bukan ditampilkan lalu ditolak `403`.

| Peran | Aksi yang terlihat |
| --- | --- |
| Unit pelayanan / dokter peminta | Buat order, lihat status |
| Petugas Bank Darah | Order, permintaan, penerimaan, alokasi, batal alokasi, bukti, pemberian, tindakan, sampel & hasil golongan darah |
| Peran berwenang (`DEF-BD-004`) | Jalur darurat, validasi & penyelesaian konflik golongan darah, koreksi pemberian |
| Admin master Bank Darah | Katalog komponen, daftar alasan |

---

## 5. Penanganan keadaan (wajib tiap layar daftar & kerja)

| Keadaan | Perilaku |
| --- | --- |
| Memuat | Kerangka baris/awalan, bukan layar kosong |
| Kosong | Kalimat terbaca petugas + tombol atur ulang saringan |
| Gagal | "Data gagal dimuat." + [Coba lagi] |
| Data basi | Setelah aksi status (alokasi/pemberian/validasi), daftar & detail di-*refetch*; token konkurensi `409` → "Data sudah berubah, muat ulang." |
| Pengiriman ganda | Tombol aksi status dinonaktifkan selama proses; server tetap dijaga token konkurensi |
| Konflik golongan darah | Penanda menahan tombol pemberian/alokasi yang menuntut golongan darah sah (`FE-BD-007`) |

---

## 6. Kewenangan UI (`DEV_DISCRETION`) dan yang mengikat

| Decision | Keadaan |
| --- | --- |
| `FE-BD-001` beda tampilan golongan darah diminta vs hasil | **Mengikat** |
| `FE-BD-002` daftar `PendingReview` | **Mengikat** ada; bentuk bebas |
| `FE-BD-003` perilaku order ganda (tahan + alasan) | **Mengikat** |
| `FE-BD-004` daftar tunggakan bukti darurat | **Mengikat** ada |
| `FE-BD-005` tampilan jalur darurat sebagai jalur tidak normal | **Mengikat** |
| `FE-BD-007` penanda hasil bertentangan | **Mengikat** terlihat & menahan |
| `FE-BD-008` penanda bukti lewat masa berlaku | **Mengikat** terlihat sebelum pemberian |
| `FE-BD-009` penyelesaian konflik di layar pemeriksaan | **Mengikat** |
| `FE-BD-006` menu, route, susunan tab, modal, warna, tata letak | `DEV_DISCRETION` — ikuti konvensi V2 |
