# Preflight `RJ-BIL-BE-003` — Lab Milestone sampai `Accepted`

| Field | Nilai |
|---|---|
| Task | `RJ-BIL-BE-003` |
| Outcome | Menyediakan Lab milestone minimal sampai `Accepted` |
| Blueprint | `RJ-BIL-BP-001` revision `11` |
| Requirement sumber | `RJ-BIL-GATE-DEC-003` (Closure Amendment `2026-08-19`) |
| Task approval | `APPROVED_FOR_EXECUTION` |
| Jenis dokumen | Preflight read-only — belum ada satu baris code pun yang diubah |
| Wewenang yang dipakai | `READ_DISCOVERY_AUTHORITY`, `QBE_PREFLIGHT_AUTHORITY` |
| Wewenang yang belum ada | `BACKEND_WRITE_AUTHORITY` untuk scope `RJ-BIL-BE-003` |
| Backend SHA | `92108587e69b9a935b2fd264882100149f80ed02` cabang `sukmagp` |
| Tanggal | `2026-08-24` |
| Verdict preflight | `CLOSED` — keempat pertanyaan dijawab author pada `2026-08-24`; eksekusi tercatat pada [execution-evidence-RJ-BIL-BE-003.md](execution-evidence-RJ-BIL-BE-003.md) |

## 1. Ringkasan untuk pembaca non-teknis

Tugas ini menjawab satu pertanyaan sederhana: **kapan sebuah pemeriksaan laboratorium boleh mulai
ditagihkan kepada pasien?**

Jawaban yang sudah Anda kunci pada `2026-08-19` adalah: **saat sampel diterima dan dinyatakan layak
periksa oleh petugas lab** — bukan saat dokter memesan, bukan saat darah diambil, dan bukan saat
sampel sampai di meja lab. Istilah teknisnya `Accepted`.

Contoh konkret. Pemeriksaan gula darah puasa bertarif `Rp150.000`:

| Waktu | Kejadian | Boleh ditagih? | Alasan |
|---|---|---|---|
| `08:00` | Dokter memesan pemeriksaan | Tidak | Baru pesanan, belum ada pekerjaan lab |
| `08:20` | Perawat mengambil darah pasien | Tidak | Sampel ada, tapi lab belum menilainya |
| `08:35` | Sampel tiba di laboratorium | Tidak | Diterima secara fisik bukan berarti dinyatakan layak |
| `08:40` | Petugas lab menyatakan sampel layak periksa | **Ya — `Rp150.000` terbentuk** | Milestone `Accepted` tercapai |
| `09:30` | Hasil selesai dan divalidasi | Tidak menambah tagihan | Hasil bukan pemicu tagihan |

Kalau pada `08:40` petugas lab justru **menolak** sampel karena hemolisis (darah rusak), pemeriksaan
`Rp150.000` itu **tidak ditagihkan**. Bila penolakan terjadi karena kesalahan internal rumah sakit
dan darah harus diambil ulang, pengambilan ulang itu **tidak dibebankan kepada pasien**.

Masalahnya: **modul Laboratorium di sistem saat ini belum bisa mencatat satu pun dari langkah di
atas.** Yang ada hanya "buat pesanan" dan "batalkan pesanan". Tidak ada pencatatan pengambilan
sampel, penerimaan sampel, penilaian kelayakan, apalagi pengambilan ulang.

## 2. Keadaan source Laboratorium hari ini — `FACT`

Seluruh modul Laboratorium berisi **4 file dan 374 baris**.

| File | Baris | Isi |
|---|---|---|
| `Models/LabOrder.cs` | `22` | Hanya `Id`, `EncounterId`, `ProcedureId` |
| `DTOs/LabOrderDtos.cs` | `37` | Request buat, response list dan detail |
| `Services/LabOrderService.cs` | `183` | `GetList`, `GetDetail`, `Create`, `Cancel` |
| `Controllers/LabOrderController.cs` | `132` | 4 endpoint |

Fakta yang terverifikasi dari source:

| # | Fakta | Bukti |
|---|---|---|
| `F-01` | `LabOrder` **tidak punya kolom status sama sekali** | `LabOrder.cs:8-21` |
| `F-02` | Satu-satunya penanda keadaan adalah `IsCancel` warisan `IdentityModel` | `LabOrderService.cs:150-153` |
| `F-03` | **Tidak ada entity specimen** di seluruh repository | Pencarian `specimen` tidak menghasilkan satu file pun |
| `F-04` | **Tidak ada entity hasil lab** di seluruh repository | Pencarian `LabResult`, `LabTest` nihil |
| `F-05` | Tabel `LabOrder` sudah ada di database | `Migrations/20260815103436_initializeLabOrder.cs` |
| `F-06` | `LabOrderService` sudah terdaftar di DI | `Program.cs:273` |
| `F-07` | `Create` sudah memvalidasi encounter aktif dan procedure ber-flag `IsLaboratory`, aktif, tidak terhapus | `LabOrderService.cs:96-104` |
| `F-08` | **Tidak ada satu pun consumer frontend** untuk `api/v1/health-services/laboratory-management/lab-orders` | Pencarian di `V2QuilvianSystemFrontendDev/src` nihil |
| `F-09` | Kata "Laboratorium" di frontend hanya label kategori catatan CPPT, bukan pemesanan lab | `doctor-cppt-tab.jsx:136` |
| `F-10` | Modul Tindakan **menolak** procedure ber-flag `IsLaboratory` | `PatientProcedureController.cs:1388-1389` |
| `F-11` | Daftar pilihan tindakan juga menyaring keluar lab dan radiologi | `PatientProcedureController.cs:123-124` |

**`F-10` dan `F-11` adalah kabar baik.** Artinya pemeriksaan lab tidak mungkin masuk lewat dua pintu
sekaligus. Fakta `ProcedureCharge` yang sudah dibangun `RJ-BIL-BE-002` tidak akan pernah menagih
pemeriksaan lab, sehingga **tidak ada risiko tagihan ganda** antara `BE-002` dan `BE-003`.

## 3. Keputusan yang sudah terkunci — `FACT`

`RJ-BIL-GATE-DEC-003` sudah mengunci aturan lab secara rinci. **Saya tidak perlu mengarang SOP lab
apa pun** — larangan author pada handoff `BE-002` terpenuhi dengan sendirinya karena aturannya
memang sudah tertulis.

| Kode | Aturan terkunci |
|---|---|
| `D-01` | Siklus pesanan lab: `Draft` ke `Requested` ke `Accepted` ke `InProcess` ke `Completed`; pengecualian `OnHold`, `CancelRequested`, `Cancelled` |
| `D-02` | Siklus sampel: `Planned` ke `Collected` ke `Received` ke `Accepted`; pengecualian `Rejected`, `RecollectionRequired`, `Cancelled`, `OnHold` |
| `D-03` | Siklus hasil `Pending` sampai `Released` **bukan** state pesanan dan **bukan** pemicu awal tagihan |
| `D-04` | Satu pesanan boleh punya lebih dari satu sampel |
| `D-05` | Pengambilan ulang membuat identitas sampel **baru**, sampel lama tetap disimpan beserta tautan sebabnya |
| `D-06` | Dokter boleh mengubah langsung hanya sampai `Requested`; setelah itu hanya melihat, menambah informasi klinis, atau mengajukan pembatalan |
| `D-07` | Pengambilan, penerimaan, penilaian kelayakan, pemrosesan, validasi, dan rilis memakai **kewenangan berbeda**; jabatan tidak otomatis memberi kewenangan |
| `D-08` | **Diterima secara fisik bukan berarti dinyatakan layak.** Penilaian kelayakan wajib merekam keputusan, pelaku, waktu, alasan terkendali, dan catatan opsional |
| `D-09` | Setiap sampel punya identitas/barcode sendiri dengan jejak pasti ke pesanan, encounter, dan pasien; teks barcode tidak menggantikan relasi database |
| `D-10` | `OnHold` mempertahankan keadaan operasional sebelumnya beserta alasan, pelaku, dan waktu |
| `D-11` | Semua perpindahan status material menghasilkan histori yang tidak bisa diubah |
| `D-12` | **`Accepted` adalah milestone kelayakan tagih**; `Requested`, `Collected`, `Received` bukan pemicu tagihan pemeriksaan |
| `D-13` | Lab mengirim fakta klinis; **Billing satu-satunya pemilik konsekuensi tagihan** |
| `D-14` | Pembatalan sebelum `Accepted` tidak menghasilkan tagihan pemeriksaan |
| `D-15` | Pembatalan setelah `Accepted`/`InProcess` **tidak menghapus tagihan otomatis**; Billing yang menentukan void, tagihan sebagian, atau penyesuaian |
| `D-16` | `Rejected` secara default tidak menghasilkan tagihan pemeriksaan |
| `D-17` | Pengambilan ulang akibat kesalahan internal rumah sakit memakai FOC/write-off, **tidak dibebankan otomatis kepada pasien** |
| `D-18` | Pengambilan ulang karena kondisi pasien/sampel atau sebab eksternal butuh alasan, otorisasi, dan dasar kebijakan sebelum tagihan baru |

`D-13`, `D-14`, dan `D-15` sejalan persis dengan yang sudah dibangun `RJ-BIL-BE-002`. Pola "fakta
klinis versi baru lalu Billing memutuskan koreksi" sudah berjalan dan teruji, jadi Lab tinggal
menumpang jalur yang sama.

## 4. Yang belum ada — `NOT_FOUND`

| Kode | Artefak yang dicari | Hasil |
|---|---|---|
| `N-01` | Baris Lab pada `contracts/state-transition-matrix.md` | Nihil |
| `N-02` | Baris Lab pada `contracts/validation-matrix.md` | Nihil |
| `N-03` | Baris Lab pada `contracts/permission-audit-matrix.md` | Nihil |
| `N-04` | ERD entity Lab pada `erd/` | Nihil — ERD yang ada hanya konteks dan billing operational |
| `N-05` | Daftar alasan penolakan sampel terkendali (`D-08` menyebut "alasan terkendali") | Nihil, baik sebagai enum maupun master data |
| `N-06` | Aturan penomoran/format barcode sampel (`D-09`) | Nihil |
| `N-07` | Entity konsumsi reagen/material lab | Nihil |
| `N-08` | Sign-off formal Lab, Clinical Governance, dan Billing/Finance atas `GATE-DEC-003` | Belum dilampirkan — status tercatat `locked-draft`, governance `OPEN` |

`N-01` sampai `N-04` **bukan blocker**. Matriks dan ERD adalah dokumen turunan; isinya justru
dihasilkan oleh eksekusi `BE-003` dan sudah tercantum sebagai DoD task ("acceptance test matrix
updated").

`N-07` **menutup satu bagian aturan**. `D-14` menyebut "material yang benar-benar dikonsumsi dinilai
terpisah menurut Actual Consumption Rule", tetapi sistem tidak punya pencatatan konsumsi reagen sama
sekali. Penagihan material lab karena itu **saya nyatakan di luar scope `BE-003`** dan perlu task
tersendiri; tanpa pernyataan ini, aturan tersebut akan menggantung tanpa pelaksana.

## 5. Kesimpulan teknis yang saya tarik sendiri — `INFERENCE`

Ini bukan fakta dan bukan keputusan Anda. Ini usulan teknis yang saya anggap paling aman, dan
**tetap bisa Anda tolak**.

| Kode | Inferensi | Dasar |
|---|---|---|
| `I-01` | Data `LabOrder` lama diisi status `Requested`, kecuali `IsCancel = true` yang diisi `Cancelled` | `CreateAsync` hari ini setara "sudah dipesan"; `CancelAsync` setara "dibatalkan". Tidak ada data yang kehilangan makna |
| `I-02` | Status `Draft` tidak dipakai oleh endpoint `Create` yang ada sekarang, agar perilaku lama tidak berubah | Tidak ada consumer frontend (`F-08`), tapi mengubah arti endpoint yang sudah ada tetap risiko tanpa manfaat |
| `I-03` | `SourceContext` baru bernama `Laboratory` dengan `EffectType` `LaboratoryCharge`, ditambahkan ke `BillingSourceContract` | Mengikuti persis pola `Prescription` dan `Procedure` yang sudah lulus uji di `BE-002` |
| `I-04` | Identitas sumber fakta memakai `SourceAggregateId = LabOrder.Id` dan `SourceItemId = LabSpecimen.Id` | `SourceItemId` memang disediakan `RJ-BIL-INT-001` untuk item di bawah agregat |
| `I-05` | Endpoint pembatalan lama `PUT /{id}/cancel` dipertahankan dan diperluas, tidak diganti | Sejalan dengan keputusan `1B` pada `BE-002`: pembatalan klinis adalah aksi klinis yang sah |
| `I-06` | Barcode sampel dibangkitkan sistem bila pemilik tidak menetapkan format | Hanya berlaku kalau `RJ-BIL-OQ-010` dijawab "sistem yang menentukan" |

## 6. Yang harus Anda putuskan — `OWNER_DECISION_REQUIRED`

Empat pertanyaan. Yang pertama paling menentukan karena mengubah bentuk tagihan.

### `RJ-BIL-OQ-008` — Satu pesanan, beberapa sampel: tagihannya per apa?

`D-04` mengizinkan satu pesanan lab punya beberapa sampel, dan `D-12` menyatakan `Accepted` adalah
milestone tagih. Yang tidak disebutkan: **bila satu pesanan punya 3 sampel dan baru 2 yang layak,
apakah tagihan sudah terbentuk?**

Contoh konkret. Dokter memesan paket `Rp450.000` berisi tiga pemeriksaan:

| Sampel | Pemeriksaan | Tarif | Status pukul `09:00` |
|---|---|---|---|
| `SP-01` | Darah lengkap | `Rp150.000` | Layak (`Accepted`) |
| `SP-02` | Fungsi hati | `Rp200.000` | Layak (`Accepted`) |
| `SP-03` | Urin lengkap | `Rp100.000` | Ditolak, pasien belum bisa buang air kecil |

| Opsi | Yang terjadi pukul `09:00` | Konsekuensi |
|---|---|---|
| **A. Per sampel** (rekomendasi teknis) | Tagihan `Rp350.000` terbentuk untuk `SP-01` dan `SP-02`. `SP-03` belum menagih apa pun | Tagihan selalu mencerminkan pekerjaan nyata. Butuh satu baris tagihan per sampel, jadi kuitansi lebih panjang |
| **B. Per pesanan, tunggu semua layak** | Belum ada tagihan sama sekali sampai `SP-03` selesai | Kuitansi lebih ringkas, tapi bila `SP-03` tidak pernah beres, pekerjaan `Rp350.000` yang sudah dikerjakan tidak tertagih |
| **C. Per pesanan, cukup satu sampel layak** | Tagihan penuh `Rp450.000` terbentuk | **Pasien ditagih `Rp100.000` untuk pemeriksaan yang tidak pernah dikerjakan.** Bertentangan dengan Actual Consumption Rule |

Rekomendasi teknis: **A**. Opsi C bertabrakan langsung dengan aturan Anda sendiri.

### `RJ-BIL-OQ-009` — Daftar alasan penolakan sampel

`D-08` mewajibkan "alasan terkendali", artinya petugas memilih dari daftar, bukan mengetik bebas.
Daftarnya belum ada (`N-05`). Alasan ini penting karena menentukan siapa menanggung pengambilan
ulang: kesalahan internal ditanggung rumah sakit (`D-17`), sebab pasien atau eksternal bisa
ditagihkan setelah otorisasi (`D-18`).

| Opsi | Isi |
|---|---|
| **A. Master data yang bisa diubah admin** (rekomendasi) | Tabel alasan penolakan dengan penanda "kesalahan internal ya/tidak". Lab bisa menambah alasan tanpa perlu rilis baru |
| **B. Enum tetap di code** | Lebih cepat dibangun, tapi setiap alasan baru butuh rilis program |
| **C. Anda kirimkan daftarnya sekarang** | Saya pakai persis daftar itu |

Pertanyaan pendukung: minimal daftar awalnya apa? Contoh lazim: hemolisis, jumlah sampel kurang,
salah tabung, tanpa identitas, sampel bocor, terlalu lama di perjalanan, sampel beku.

### `RJ-BIL-OQ-010` — Format barcode sampel

`D-09` mewajibkan setiap sampel punya barcode sendiri. Formatnya belum ditentukan (`N-06`).

| Opsi | Bentuk | Catatan |
|---|---|---|
| **A. Sistem yang membangkitkan** (rekomendasi) | `LAB-20260824-000123` | Dijamin unik, tidak butuh alat tambahan |
| **B. Ikuti format alat atau lab yang sudah berjalan** | Menyesuaikan | Perlu contoh format nyata dari lab |
| **C. Petugas mengetik barcode dari label yang sudah tercetak** | Bebas | Berisiko salah ketik dan tabrakan nomor; perlu validasi unik |

### `RJ-BIL-OQ-011` — Sign-off formal `GATE-DEC-003`

`GATE-DEC-003` berstatus `locked-draft` dengan governance `OPEN` (`N-08`): isinya sudah Anda setujui,
tetapi tanda tangan Lab, Clinical Governance, dan Billing/Finance belum dilampirkan.

| Opsi | Konsekuensi |
|---|---|
| **A. Jalan dengan status sekarang** | `BE-003` dikerjakan atas dasar persetujuan Anda sebagai author. Catatan "governance sign-off pending" tetap melekat pada evidence |
| **B. Tunggu tanda tangan tiga pihak** | Paling aman untuk keselamatan pasien, tapi `BE-003` berhenti sampai tanda tangan turun |

Ini keputusan tata kelola, bukan teknis. Saya tidak mengambilnya sendiri.

## 7. Endpoint hari ini dan rencana penambahan

### `[Tags("Health Services / Laboratory Management / Lab Order")]`

Yang sudah ada:

| Method | Route | Permission | Kegunaan |
|---|---|---|---|
| `GET` | `/api/v1/health-services/laboratory-management/lab-orders` | `LabOrder : Read` | Daftar pesanan lab |
| `GET` | `/api/v1/health-services/laboratory-management/lab-orders/{id}` | `LabOrder : Read` | Detail satu pesanan |
| `POST` | `/api/v1/health-services/laboratory-management/lab-orders` | `LabOrder : Create` | Membuat pesanan |
| `PUT` | `/api/v1/health-services/laboratory-management/lab-orders/{id}/cancel` | `LabOrder : Update` | Membatalkan pesanan |

Yang perlu ditambahkan agar `D-01` sampai `D-12` terpenuhi. **Perhatikan kolom permission**: `D-07`
mewajibkan kewenangan yang berbeda untuk tiap langkah, sehingga `LabOrder : Update` tunggal tidak
cukup — persis pelajaran yang sama dengan `Prescription : Update` pada `BE-002`.

| Method | Route rencana | Permission rencana | Kegunaan | Pemicu tagihan? |
|---|---|---|---|---|
| `POST` | `/lab-orders/{id}/request` | `LabOrder : Request` | Dokter mengirim pesanan ke lab | Tidak |
| `POST` | `/lab-orders/{id}/specimens` | `LabSpecimen : Plan` | Merencanakan sampel yang akan diambil | Tidak |
| `POST` | `/lab-specimens/{id}/collect` | `LabSpecimen : Collect` | Mencatat pengambilan sampel | Tidak |
| `POST` | `/lab-specimens/{id}/receive` | `LabSpecimen : Receive` | Mencatat sampel tiba di lab | Tidak |
| `POST` | `/lab-specimens/{id}/accept` | `LabSpecimen : Accept` | Menyatakan sampel layak periksa | **Ya** |
| `POST` | `/lab-specimens/{id}/reject` | `LabSpecimen : Accept` | Menolak sampel dengan alasan terkendali | Tidak (`D-16`) |
| `POST` | `/lab-specimens/{id}/request-recollection` | `LabSpecimen : Accept` | Meminta pengambilan ulang, membuat identitas sampel baru | Tergantung `D-17` dan `D-18` |
| `POST` | `/lab-orders/{id}/hold` dan `/resume` | `LabOrder : Hold` | Menahan dan melanjutkan pesanan | Tidak |
| `GET` | `/lab-orders/{id}/history` | `LabOrder : Read` | Melihat riwayat perpindahan status | Tidak |

Jumlah dan bentuk akhirnya masih bisa berubah saat implementasi; tabel ini adalah rencana, bukan
kontrak yang sudah dikunci.

## 8. Dampak terhadap Billing dan hasil `RJ-BIL-BE-002`

| Aspek | Dampak |
|---|---|
| `BillingSourceContract` | Perlu satu entri baru `Laboratory` dengan `LaboratoryCharge`. Hari ini `Laboratory` **sengaja ditolak**, dan penolakan itu punya test sendiri di `ClinicalMilestoneFactProducerTests` |
| `ClinicalMilestoneFactProducer` | **Tidak perlu diubah.** Producer sudah generik terhadap `SourceContext` |
| `BilClinicalMilestoneFact` | **Tidak perlu diubah.** `SourceItemId` sudah tersedia untuk identitas sampel |
| `BillingFolioService` | **Tidak perlu diubah** |
| Pembatalan setelah `Accepted` (`D-15`) | Sudah tertangani. Jalur "fakta versi baru lalu folio `ReviewRequired`, tagihan asli utuh" yang dibangun `BE-002` berlaku sama |
| Test yang harus disesuaikan | Test yang menegaskan `Laboratory` **ditolak** akan berubah maknanya menjadi `Laboratory` **diterima** |

Artinya arsitektur `BE-002` menahan beban `BE-003` tanpa perombakan. Pekerjaan `BE-003` berat di
sisi Laboratorium, ringan di sisi Billing.

## 9. Perkiraan migration — belum dibuat, belum dijalankan

| Perubahan | Alasan |
|---|---|
| Tambah kolom status, waktu, pelaku, alasan, dan `Version` pada `LabOrder` | `D-01`, `D-11` |
| Tabel baru `LabSpecimen` | `D-02`, `D-04`, `D-05`, `D-09` |
| Tabel baru riwayat perpindahan status | `D-11` |
| Master data alasan penolakan | `D-08`, tergantung jawaban `RJ-BIL-OQ-009` |
| Pengisian data lama | Sesuai `I-01`, menunggu persetujuan |

**Peringatan yang harus Anda ketahui sebelum `BE-003` dijalankan.** `BillingTestDatabaseFixture`
memanggil `Database.Migrate()` sebelum test pertama, dan connection string test jatuh ke
`appsettings.Development.json`. Selama itu belum diperbaiki, **menjalankan `dotnet test` akan
menerapkan migration `BE-003` ke `QuilvianNewDevTim01`** — persis seperti yang terjadi pada `BE-002`
dan sudah dilaporkan pada bagian `8` [execution-evidence-RJ-BIL-BE-002.md](execution-evidence-RJ-BIL-BE-002.md).
`BE-003` mengubah tabel yang **sudah berisi data**, bukan sekadar menambah tabel kosong, sehingga
risikonya lebih besar daripada `BE-002`. Saya sarankan menunjuk database test tersendiri lewat
`QUILVIAN_BILLING_TEST_DB` sebelum implementasi dimulai.

## 10. Batas wewenang

| Hal | Status |
|---|---|
| Source code diubah | `NO` — dokumen ini murni hasil pembacaan |
| Migration dibuat | `NO` |
| Migration dijalankan | `NO` |
| Commit | `NO` |
| Push | `NO` |
| Merge | `NO` |
| Deploy | `NO` |
| Adapter eksternal `RJ-BIL-DEP-009` | Tetap `INACTIVE` |
| SOP lab dikarang sendiri | `NO` — seluruh aturan berasal dari `RJ-BIL-GATE-DEC-003` |
