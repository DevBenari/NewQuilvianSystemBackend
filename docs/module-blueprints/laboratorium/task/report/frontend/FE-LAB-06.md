# Laporan Perubahan Frontend — `FE-LAB-06`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-LAB-06` |
| Judul | Layar pesanan laboratorium dan penanda cito |
| Slice | `S1a` — pesanan dan penandaan cito (`roadmap/frontend-roadmap.md` bagian 4, gelombang `MVP-1`) |
| Roadmap | `docs/module-blueprints/laboratorium/roadmap/frontend-roadmap.md` bagian 4 |
| Trace | `FR-07.1`, `FR-01.1` .. `FR-01.3`; `LAB-DEC-025`, `LAB-DEC-026`; `LAB-FE-008`; `VAL-03`, `VAL-04`; `AC-18`, `AC-40`, `AC-43`; `03-frontend-architecture.md` bagian 3.1 dan 6 |
| Contract version | `LAB-API-v1` r3 — `approved`, dikunci 2026-09-02. Grup Lab Order dan Lab Examination |
| Wewenang UI | `LAB-FE-008` konvensi project — bentuk penanda cito `DEV_DISCRETION`, **asalkan hanya dokter pemesan yang dapat menandainya**. `LAB-DEC-026` mengikat penempatannya pada tingkat pemeriksaan. `03-frontend-architecture.md` bagian 6 mengikat penanda cito tidak dibedakan hanya dengan warna |
| Dependency | `FE-LAB-04` — **selesai**. Endpoint dari `BE-LAB-10` dan `BE-LAB-16` — **selesai**, keduanya diverifikasi langsung pada source backend. **`FE-LAB-05` diwaive pemilik modul** — lihat bagian 1 |
| Klasifikasi | `HEAVY` — skor 12: repository 2, berkas diperiksa 2, berkas diubah 2, logika bisnis 2, kontrak API 1, database 0, keamanan 1, UI/workflow 2 |
| Task mode | `FRONTEND` — frontend target tulis, backend strict read-only sebagai sumber kebenaran kontrak |
| Target tulis | `QuilvianSystemFrontendDev` — lapis modul `laboratory-management`, `src/utils/menu-sidebar/menu-items.jsx`, dua berkas utils master data Laboratorium, dan satu berkas uji; serta `NewQuilvianSystemBackend` — **hanya** `docs/module-blueprints/laboratorium/task/report/frontend/FE-LAB-06.md` beserta tautan buktinya pada `roadmap/` |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Commit frontend saat dikerjakan | `443270f3f`, branch `YogaV2`, upstream `origin/YogaV2` |
| Commit backend yang dijadikan rujukan | `2dfc4f2`, branch `yoga` |
| Tanggal | 2026-09-04 |
| Status | **Selesai.** Tiga layar berdiri, penanda cito dan duplo melekat pada baris pemeriksaan, dan kontrol tanpa kewenangan disembunyikan atau dinonaktifkan sejak awal. Satu batas kontrak yang menghalangi penegakan penuh `VAL-03` dicatat pada bagian 8 |

---

## 1. Keadaan yang ditemukan di awal

### 1.1 Dependency `FE-LAB-05` terblokir, dan diwaive pemilik modul

Roadmap mencantumkan `FE-LAB-05` sebagai dependency task ini. `FE-LAB-05` sendiri **terblokir**,
dan buktinya diambil dari source backend `2dfc4f2`, bukan dari dokumen:

| Yang dicari | Hasil |
| --- | --- |
| `LabPatientRegistrationController` | tidak ada |
| Route `lab-patient-registrations` di seluruh `Areas/` | tidak ada |
| DTO `RegisterLabWalkInRequest` | tidak ada |
| Laporan `BE-LAB-08.md` | tidak ada; roadmap backend menandainya `Siap direncanakan` |

Penahannya berada di luar Laboratorium: `BE-LAB-08` menuntut endpoint pelaksana `INT-05` yang
kepemilikannya ada pada pemegang `registration-management`.

**Keputusan pemilik modul, diambil 2026-09-04: dependency `FE-LAB-05` diwaive dan `FE-LAB-06`
dikerjakan lebih dulu.** Dasar yang membuat pelonggaran itu masuk akal:

1. Endpoint pasangannya, `BE-LAB-10` dan `BE-LAB-16`, **sudah selesai** dan terverifikasi.
2. `FE-LAB-06` **tidak memakai ulang satu pun artefak** dari `FE-LAB-05`. Reuse yang disebut
   roadmap justru dari `FE-LAB-04` — komponen pemilih katalog — yang sudah berdiri.
3. Pesanan lab memang dapat dibuat untuk kunjungan yang sudah ada. `AC-11` menyatakannya
   terbuka: pesanan dibuat dari kunjungan Rawat Jalan, Rawat Inap, maupun IGD, yang seluruhnya
   dibentuk modul lain. Layar ini karena itu memilih kunjungan dari daftar kunjungan yang
   sudah ada, bukan mendaftarkan pasien baru.

### 1.2 Dua batas kontrak yang menentukan bentuk layar

**Pertama: identitas dokter pemesan tidak dikirim.** `VAL-03` menetapkan hanya dokter pemesan
yang boleh menandai kesegeraan, dan backend menegakkannya dengan membandingkan penunjuk
pengguna terhadap `LabOrder.RequestedByUserId`. Ruas itu **tidak ada** pada
`LabOrderListResponse` maupun `LabOrderDetailResponse` — keduanya diperiksa baris demi baris.
Akibatnya layar tidak dapat membandingkan siapa yang sedang membuka dengan siapa yang memesan.
Apa yang tetap dapat dikerjakan dijelaskan pada bagian 3.3.

**Kedua: pemeriksaan lahir bersama wadahnya.** `AddLabExaminationRequest` menuntut `SpecimenId`
— artinya menambah pemeriksaan menuntut wadah yang sudah direncanakan, dan perencanaan wadah
adalah cakupan `FE-LAB-07`. Layar detail karena itu **menampilkan dan menandai** pemeriksaan
yang ada, tetapi tidak menyediakan jalur menambahkannya. Itu bukan kekurangan task ini,
melainkan urutan yang memang ditetapkan roadmap.

**Ditambah satu batas bentuk pesanan.** `CreateLabOrderRequest` menerima **tepat satu**
`procedureId`. Satu pesanan karena itu menampung satu jenis pemeriksaan pada rilis ini, dan
komponen pemilih katalog dipakai dalam mode pilihan tunggal.

---

## 2. Proses bisnis dari sisi pengguna

**Siapa penggunanya.** Dokter pemesan membuat pesanan dan menandai pemeriksaannya sebagai
cito. Petugas laboratorium melihat saja — dan itu terlihat langsung di layar, bukan baru
terasa saat tombol ditekan.

**Kenapa cito melekat pada pemeriksaan, bukan pada pesanan.** Satu kunjungan dapat memuat
Kalium yang mendesak dan Kolesterol yang tidak. Bila kesegeraan ditandai pada pesanan, kedua
pemeriksaan itu terpaksa berbagi satu tingkat kesegeraan, dan daftar kerja laboratorium
mendahulukan pekerjaan yang sebenarnya tidak mendesak. `LAB-DEC-026` menutup kemungkinan itu,
dan `AC-40` menjaganya.

### 2.1 Membuat pesanan

1. Pengguna membuka **Pelayanan Kesehatan → Laboratorium → Pesanan Laboratorium**, lalu
   menekan **+ Buat Pesanan**.
2. Ia memilih **kunjungan pasien** dari daftar kunjungan yang sudah ada, dan boleh memilih
   disiplinnya. Disiplin tidak wajib — kontrak `LAB-API-v1` r3 mengunci `POST /lab-orders`
   tetap menerima permintaan tanpa ruas itu — tetapi mengisinya membuat pesanan muncul pada
   daftar pantau disiplin yang tepat.
3. Ia memilih **satu pemeriksaan** dari katalog. Katalognya dapat disaring per disiplin dan
   dicari bebas, dan setiap baris menampilkan **harga satuannya**. Panel di bawahnya
   menampilkan pemeriksaan terpilih beserta subtotal dan totalnya.
4. Satu kalimat berdiri permanen di bawah total: angka itu perkiraan biaya, bukan tagihan.
   Menyimpan pesanan **tidak** membentuk satu pun baris tagihan.
5. Setelah tersimpan, pengguna diarahkan ke halaman detail pesanan.

### 2.2 Membaca daftar pesanan

Layar daftar menampilkan waktu dibuat, kode dan nama pemeriksaan, jumlah wadah, jumlah wadah
yang sudah dinyatakan layak, dan status pesanan. Penyaringnya: tanggal mulai, tanggal akhir,
kunjungan pasien, disiplin, status pesanan, jumlah baris, dan pencarian bebas.

Penyaring kunjungan itulah yang membuat layar dapat menampilkan pesanan satu pasien saja tanpa
menarik seluruh tabel pesanan rumah sakit.

**Tidak ada kolom kesegeraan pada daftar pesanan**, dan itu disengaja — satu pesanan tidak
punya satu kesegeraan.

### 2.3 Menandai cito pada baris pemeriksaan

1. Dari daftar, klik dua kali sebuah baris untuk membuka detail.
2. Kartu atas menampilkan informasi pesanan beserta status besarnya. **Kartu ini tidak punya
   satu pun tombol kesegeraan.**
3. Panel **Pemeriksaan Terpesan** menampilkan setiap pemeriksaan beserta barcode wadah
   penopangnya, status, kesegeraan, penanda duplo, dan harga satuannya.
4. Penanda cito tampil sebagai lencana bertuliskan **CITO** — membawa teks, bukan hanya
   warna, sehingga tetap terbaca pengguna dengan gangguan penglihatan warna. Bila sudah
   ditandai, waktu penandaannya ikut tampil.
5. Tombol **Tandai Cito** dan **Kembalikan Biasa** berada pada baris pemeriksaan itu. Menekannya
   memunculkan konfirmasi yang menyebutkan akibatnya: pemeriksaan cito didahulukan pada daftar
   kerja, dan penandaannya tercatat beserta waktu dan dokter yang menandainya.
6. Tombol **Tandai Duplo** berada pada baris yang sama, dan keterangannya menyebut apa adanya:
   penanda duplo tidak mengubah salinan tarif pada baris pemeriksaan.

### 2.4 Jalur yang tidak normal

| Keadaan | Yang dialami pengguna |
| --- | --- |
| Pengguna bukan dokter | Kolom aksi kesegeraan **tidak dirender sama sekali**, dan satu kotak keterangan menjelaskan kenapa: hanya dokter pemesan yang dapat menandainya |
| Pesanan sudah selesai atau dibatalkan | Tombol kesegeraan **nonaktif sejak awal**, dan alasannya muncul sebagai keterangan tombol: pesanan yang sudah selesai tidak dapat diubah kesegeraannya (`VAL-04`) |
| Pemeriksaan sudah digugurkan atau dibatalkan | Tombolnya nonaktif dengan alasan yang sesuai |
| Dokter lain yang bukan pemesan menekan tombol | Backend menolak `403` beserta pesan `VAL-03`, dan pesannya ditampilkan apa adanya sebagai notifikasi merah. Inilah sisa yang belum dapat dicegah layar — lihat bagian 8 |
| Pesanan belum punya pemeriksaan | Panel menampilkan keterangan bahwa pemeriksaan ditambahkan bersama perencanaan wadah, pada layar wadah dan pemeriksaan |
| Menyimpan pesanan tanpa memilih kunjungan atau pemeriksaan | Ditolak di layar lebih dulu, dengan pesan pada isian yang bersangkutan |
| Tautan detail dibuka pada sesi baru | Token route sudah tidak ada, sehingga muncul ajakan membuka ulang dari daftar pesanan |
| Tanpa hak akses | Seluruh isi halaman diganti layar "Ups! Akses Ditolak" |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

**Tata kelola dan aturan.** `AGENTS.md` frontend; `CLAUDE.md` frontend;
`rules/frontend/frontend-architecture.md`; `rules/frontend/base-component-catalog.md`;
`rules/frontend/base-component-decision-gate.md`; `rules/frontend/design-tokens.md`;
`rules/frontend/ui-consistency-checklist.md`; `rules/frontend/test-policy.md`;
`rules/frontend/REPORT_TEMPLATE.md`.

**Blueprint dan kontrak.** `roadmap/frontend-roadmap.md` bagian 4;
`roadmap/backend-roadmap.md` bagian `BE-LAB-08` dan tabel status task;
`03-frontend-architecture.md` bagian 3.1, 5, dan 6; `contracts/api-contract.md` grup Lab Order
dan Lab Examination; `contracts/validation-matrix.md` `VAL-03`, `VAL-04`;
`testing/acceptance-test-matrix.md` `AC-18`, `AC-40`, `AC-43`;
`task/report/backend/BE-LAB-10.md`, `BE-LAB-16.md`.

**Backend sebagai sumber kebenaran kontrak — strict read-only.**
`Areas/HealthServices/LaboratoryManagement/Controllers/LabOrderController.cs`;
`.../Controllers/LabExaminationController.cs`; `.../DTOs/LabOrderDtos.cs`;
`.../DTOs/LabExaminationDtos.cs`; `.../Services/LabExaminationService.cs` — khususnya
`SetUrgencyAsync` yang memperlihatkan urutan `VAL-04` sebelum `VAL-03`;
`.../Services/LabFilterMetadataFactory.cs`; `.../Enums/LaboratoryEnums.cs`.

**Frontend sebagai acuan pola.** Kerangka `FE-LAB-01` dan komponen pemilih katalog `FE-LAB-04`;
`components/features/base-features/` (data-table, data-filter, filter-select,
resource-filter-select, confirm-modal, status-badge, information-alert, toast-stack, hero,
base-button); `lib/hooks/select/health-service/health-service-select-resources.js` untuk
sumber daftar kunjungan; `lib/state/slice/auth/login-slice.jsx` untuk ketersediaan peran;
`utils/Formatters.jsx`; `src/app/globals.css`.

### 3.2 Berkas yang berubah

**Berkas baru:**

| Lapis | Berkas |
| --- | --- |
| Konstanta | `lib/constants/health-services/laboratory-management/lab-order-constants.jsx` |
| API service | `lib/services/health-services/laboratory-management/lab-examination.service.js` |
| Hook | `lib/hooks/health-services/laboratory-management/use-lab-order-list.jsx` |
| Hook | `lib/hooks/health-services/laboratory-management/use-lab-order-detail.jsx` |
| Hook | `lib/hooks/health-services/laboratory-management/use-lab-order-form.jsx` |
| Hook | `lib/hooks/health-services/laboratory-management/lab-order-urgency-rules.js` — aturan murni kewenangan kesegeraan |
| Komponen tampilan | `components/view/health-services/laboratory-management/lab-orders/lab-order-list-view.jsx` |
| Komponen tampilan | `.../lab-orders/lab-order-table-columns.jsx` |
| Komponen tampilan | `.../lab-orders/detail/lab-order-detail-view.jsx` |
| Komponen tampilan | `.../lab-orders/form/lab-order-form-view.jsx` |
| Style | `style/health-services/laboratory-management/lab-orders/lab-order.module.css` |
| Route | `app/health-services/laboratory-management/lab-orders/page.jsx`, `create/page.jsx`, `[slug]/route-token.js`, `[slug]/page.jsx` |
| Uji | `tests/unit/lab-order-urgency-rules.test.mjs` |

**Berkas yang disunting:**

| Berkas | Perubahan |
| --- | --- |
| `lib/services/health-services/laboratory-management/lab-order.service.js` | Ditambah `getLabOrders`, `getLabOrderDetail`, `createLabOrder`, dan `getLabOrderFilterMetadata` |
| `lib/state/slice/health-services/laboratory-management/lab-order-slice.jsx` | Ditambah state daftar, detail, pemeriksaan, dan penanda perintah; enam thunk baru; dua reducer pembersih. Seluruh ruas dan thunk milik `FE-LAB-01` **tidak diubah** |
| `lib/hooks/health-services/laboratory-management/use-lab-catalog-picker.jsx` | Ditambah mode `singleSelection` |
| `components/features/.../lab-catalog-picker/lab-catalog-picker.jsx` | Meneruskan `singleSelection` |
| `utils/health-services/master-data/lab-value-bounds/lab-value-bound-utils.jsx` | `createToast` kini juga mengisi `variant` — lihat bagian 3.3 |
| `utils/health-services/master-data/lab-rejection-reasons/lab-rejection-reason-utils.jsx` | Perbaikan yang sama |
| `utils/menu-sidebar/menu-items.jsx` | Satu butir menu **Pesanan Laboratorium**, diletakkan sebelum Tarif Laboratorium karena ia layar kerja harian sedangkan tarif hanya rujukan |

**Satu thunk yang dikembalikan.** `getLabOrderFilterMetadata` dan thunk-nya sempat dihapus pada
`FE-LAB-01` justru karena belum ada yang memanggilnya — dan itu dicatat terbuka di laporan
task tersebut. Keduanya dikembalikan di sini **bersama konsumennya**: pilihan status dan
disiplin pada penyaring daftar pesanan.

### 3.3 Kepatuhan arsitektur frontend

**Tujuh lapis dipatuhi.** Aturan murni kewenangan kesegeraan ditempatkan **bersebelahan dengan
hook pemakainya** di dalam lapis hook, bukan di lapis `utils` yang memang tidak dibuka modul
ini. Ia modul biasa tanpa React, sehingga dapat diuji langsung.

**Bagaimana `LAB-DEC-026` dan `AC-40` ditegakkan — tiga lapis.**

1. **Tidak ada kolom kesegeraan pada daftar pesanan.** Definisi kolomnya tidak memuat
   `urgency`, `cito`, maupun `duplo`, dan alasannya ditulis di berkasnya. Dijaga uji `S8`.
2. **Tidak ada kontrol kesegeraan pada kartu pesanan.** Kartu informasi pesanan pada halaman
   detail hanya menampilkan data; seluruh tombol kesegeraan berada di dalam tabel pemeriksaan.
3. **Perintahnya per pemeriksaan.** Kedua thunk kesegeraan menerima `examinationId`, bukan
   `labOrderId`, karena endpoint-nya pun begitu.

**Bagaimana "tersembunyi atau nonaktif, bukan gagal saat ditekan" dikerjakan.**

| Keadaan | Yang dilakukan layar | Dasarnya |
| --- | --- | --- |
| Bukan dokter | Kolom aksi **tidak dirender**, ditambah kotak keterangan | `VAL-03`, sejauh yang dapat diketahui frontend |
| Pesanan selesai atau dibatalkan | Tombol **nonaktif** beserta alasannya sebagai keterangan tombol | `VAL-04`, sepenuhnya dapat disimpulkan dari `orderStatus` |
| Pemeriksaan gugur atau dibatalkan | Tombol **nonaktif** beserta alasannya | Turunan dari `examinationStatus` |
| Dokter lain yang bukan pemesan | **Belum dapat dicegah layar** — backend menolak `403`, dan pesannya ditampilkan apa adanya | `RequestedByUserId` tidak dikirim pada respons |

Urutan pemeriksaannya sengaja **sama dengan backend**: status pesanan diperiksa lebih dulu,
karena pesanan yang sudah selesai tidak dapat diubah oleh siapa pun — termasuk dokter
pemesannya. Dijaga uji `S5`.

**Perbaikan satu cacat yang ditemukan pada pekerjaan sendiri.** `ToastStack` membaca ruas
`variant`, sedangkan pembuat toast bersama yang dipakai `FE-LAB-02` dan `FE-LAB-03` — disalin
dari modul rujukan `job-level` — hanya mengisi `type`. Akibatnya notifikasi gagal tampil
sebagai notifikasi biasa, bukan merah. Kedua berkas utils milik task-task itu diperbaiki agar
mengisi **keduanya**, sehingga varian warnanya berlaku tanpa memutus pemanggil yang membaca
`type`. Cacat yang sama ada pada modul rujukan dan modul lain yang menyalinnya; **itu tidak
disentuh** karena berada di luar cakupan, dan dicatat di sini sebagai temuan.

**Gerbang keputusan base component.**

```text
UI GATE: 9 elemen — REUSE 8, EXTEND 1, COMPOSE 0, WRAP 0, NEW 0
```

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Header halaman | `Hero` | `.../hero.jsx` | `REUSE` | Dipakai ketiga layar |
| Penyaring dan pencarian | `DataFilter`, `FilterDatePicker`, `FilterSelect` | `.../data-filter.jsx` | `REUSE` | Urutan baku dipertahankan |
| Penyaring kunjungan | `ResourceFilterSelect` | `.../resource-filter-select.jsx` | `REUSE` | Membaca registry `patientEncounters` |
| Tabel pesanan dan pemeriksaan | `DataTable` | `.../data-table.jsx` | `REUSE` | Dua pemakaian |
| Lencana status, kesegeraan, dan duplo | `StatusBadge` | `.../status-badge.jsx` | `REUSE` | Lencana cito membawa teks, bukan hanya warna |
| Konfirmasi penandaan cito | `ConfirmModal` | `.../confirm-modal.jsx` | `REUSE` | Pesannya menyebut akibat dan pencatatannya |
| Tombol aksi | `BaseButton` | `.../base-button.jsx` | `REUSE` | Termasuk `title` sebagai keterangan tombol nonaktif |
| Notifikasi hasil aksi | `ToastStack` | `.../toast-stack.jsx` | `REUSE` | Kini memakai ruas `variant` yang benar |
| Pemilih katalog pemeriksaan | `LabCatalogPicker` dari `FE-LAB-04` | `components/features/.../lab-catalog-picker` | `EXTEND` | Ditambah prop opsional `singleSelection` |

**Keputusan untuk satu-satunya baris yang bukan `REUSE`:**

> **Keputusan: memilih tepat satu pemeriksaan pada formulir pemesanan**
>
> - **A. Tambah prop opsional `singleSelection` pada `LabCatalogPicker` — Rekomendasi.**
>   Prop opsional, sehingga pemakai lain tidak terdampak sama sekali; perilaku bawaannya tetap
>   pilihan jamak. Harga satuan, subtotal, dan total tetap datang dari komponen yang sama,
>   sehingga `AC-43` terpenuhi tanpa menulis ulang perhitungan apa pun.
> - **B. Pakai `FilterSelect` biasa untuk memilih pemeriksaan.** Paling ringkas, tetapi harga
>   tidak tampil saat memesan — dan itulah yang justru dituntut `AC-43`.
> - **C. Biarkan pilihan jamak, lalu ambil baris pertamanya.** Tanpa perubahan komponen sama
>   sekali, tetapi pengguna dibiarkan memilih tiga pemeriksaan padahal hanya satu yang
>   tersimpan — layar yang menjanjikan sesuatu yang tidak ia lakukan.
>
> Opsi **A** yang dijalankan. Komponen yang di-`EXTEND` adalah komponen milik modul ini
> sendiri, bukan base component bersama, sehingga tidak ada modul lain yang terdampak.

**Token desain.** Berkas style barunya tidak memuat satu pun nilai warna, radius, bayangan,
atau jarak sebagai literal. Tidak ada `!important` dan tidak ada inline style.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tabel menampilkan "Mengambil daftar pesanan laboratorium..." atau "Mengambil pemeriksaan terpesan..."; tombol aksi terkunci selama proses |
| Kosong | "Pesanan laboratorium tidak ditemukan." pada daftar; pada panel pemeriksaan, keterangan bahwa pemeriksaan ditambahkan bersama perencanaan wadah |
| Gagal | Pesan dari server ditampilkan apa adanya — kotak merah pada layar, notifikasi merah untuk kegagalan aksi. `VAL-03` dan `VAL-04` sampai ke pengguna dengan kalimat aslinya |
| Tanpa hak akses | Seluruh isi halaman diganti layar "Ups! Akses Ditolak" lewat `AccessDeniedGate` |
| Tanpa kewenangan pada baris | Kolom aksi kesegeraan tidak dirender bagi bukan dokter, dan tombolnya nonaktif beserta alasannya pada pesanan atau pemeriksaan yang sudah tertutup |
| Kirim ganda | Seluruh tombol aksi terkunci sejak ditekan sampai jawaban datang |
| Data basi | Setelah penandaan berhasil, detail dan daftar pemeriksaan dimuat ulang — bukan ditebak dari jawaban satu perintah |
| Permintaan usang | Permintaan daftar lama dibatalkan ketika penyaring berubah |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Laboratory Management / Lab Order

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/laboratory-management/lab-orders/filters/metadata` | Pilihan status, disiplin, dan ukuran halaman pada penyaring | `LabOrder : Read` |
| `GET` | `/v1/health-services/laboratory-management/lab-orders` | Tabel daftar pesanan beserta penyaring dan paginasinya | `LabOrder : Read` |
| `GET` | `/v1/health-services/laboratory-management/lab-orders/{id}` | Kartu informasi pada halaman detail | `LabOrder : Read` |
| `POST` | `/v1/health-services/laboratory-management/lab-orders` | Menyimpan pesanan baru. Harga **tidak** dikirim — backend menyalinnya dari tarif yang berlaku | `LabOrder : Create` |

#### Health Services / Laboratory Management / Lab Examination

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/laboratory-management/lab-examinations/by-order/{labOrderId}` | Tabel pemeriksaan terpesan pada halaman detail | `LabExamination : Read` |
| `PUT` | `/v1/health-services/laboratory-management/lab-examinations/{id}/urgency` | Tombol Tandai Cito dan Kembalikan Biasa | `LabExamination : Update` |
| `PUT` | `/v1/health-services/laboratory-management/lab-examinations/{id}/duplo` | Tombol Tandai Duplo | `LabExamination : Update` |

#### Health Services / Laboratory Management / Lab Catalog

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/laboratory-management/lab-catalog/examinations` | Katalog pada formulir pemesanan, lewat komponen `FE-LAB-04` | `LabCatalog : Read` |
| `GET` | `/v1/health-services/laboratory-management/lab-catalog/examinations/{procedureId}/price` | Harga berlaku pemeriksaan terpilih | `LabCatalog : Read` |

Tiga endpoint grup Lab Examination yang **tidak** dikonsumsi task ini:
`GET /by-specimen/{specimenId}`, `POST /by-order/{labOrderId}`, dan `POST /{id}/cancel`.
Ketiganya bertumpu pada wadah yang sudah direncanakan, dan perencanaan wadah adalah cakupan
`FE-LAB-07`.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | Selesai tanpa satu pun error | `PASS` | Kode keluar `0` |
| `npx eslint` pada berkas baru, termasuk peringatan | 0 error, 1 peringatan | `EXISTING WARNING` | Satu `react-hooks/set-state-in-effect` dari pola resolusi token route yang sama dengan modul rujukan. Satu peringatan memoization yang sempat muncul **diperbaiki lebih dulu** dengan menyamakan dependency `useMemo` dengan yang disimpulkan React Compiler |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | 471 uji, 471 lulus, 0 gagal | `PASS` | Seluruh suite dijalankan, termasuk delapan uji baru |
| Uji `S1` (`VAL-03`) — bukan dokter | Lulus | `PASS` | Kontrol tidak ditampilkan, dan alasannya ikut dikembalikan |
| Uji `S2` — dokter pada pesanan berjalan | Lulus | `PASS` | Kontrol tampil dan dapat ditekan |
| Uji `S3` (`VAL-04`) — pesanan selesai atau dibatalkan | Lulus | `PASS` | Tombol tetap terlihat tetapi nonaktif, dengan alasan yang menyebut sebabnya |
| Uji `S4` — pemeriksaan gugur atau dibatalkan | Lulus | `PASS` | Tombol nonaktif beserta alasannya |
| Uji `S5` — urutan pemeriksaan mengikuti backend | Lulus | `PASS` | Status pesanan diperiksa lebih dulu, sama seperti `SetUrgencyAsync` yang menempatkan `VAL-04` sebelum `VAL-03` |
| Uji `S6` — peran tidak terbaca | Lulus | `PASS` | Tidak menutup jalan dokter yang sah; backend yang menolak |
| Uji `S7` — kesegeraan dibaca dari nama enum | Lulus | `PASS` | `Cito` dan `Routine` dikenali apa adanya |
| Uji `S8` (`AC-40`, `LAB-DEC-026`) — tidak ada kolom kesegeraan pada daftar pesanan | Lulus | `PASS` | Definisi kolom daftar pesanan tidak memuat `urgency`, `cito`, maupun `duplo` |
| `npm run build` | Selesai, termasuk `postbuild` standalone | `PASS` | Kode keluar `0`. Ketiga route terbit: `/lab-orders`, `/lab-orders/create`, dan `/lab-orders/[slug]` |
| Tinjauan: penanda cito tidak dibedakan hanya dengan warna | Terpenuhi | `PASS` | Lencananya bertuliskan **CITO** dan **Biasa**; warnanya hanya penguat |
| Tinjauan: barcode wadah sebagai teks yang dapat disalin | Terpenuhi | `PASS` | Dirender sebagai teks pada kolom tabel, bukan gambar |
| Grep anti-regresi warna literal, `!important`, inline style statis | Tidak ada temuan | `PASS` | Seluruh nilai style memakai token `var(...)` |
| Grep anti-regresi tombol non-base dan tabel mentah | Tidak ada temuan | `PASS` | Seluruh tombol `BaseButton`; seluruh tabel `DataTable` |

**Uji manual: `NOT FEASIBLE`.**

1. Seluruh perilaku yang menentukan menuntut **backend yang berjalan beserta datanya**:
   pesanan, wadah, dan pemeriksaan yang sudah ada. Lingkungan sesi ini tidak menyediakannya.
2. Pembuktian `VAL-03` menuntut **dua akun dokter berbeda** — satu pemesan, satu bukan — dan
   sesi ini hanya punya nol.
3. Panel pemeriksaan hanya terisi setelah wadah direncanakan, dan perencanaan wadah adalah
   cakupan `FE-LAB-07` yang belum dikerjakan.

Penggantinya bukan asumsi: kedelapan uji unit menjaga aturan kewenangan dan penempatan yang
paling menentukan, termasuk `AC-40`, dan keluaran build membuktikan ketiga layarnya terbit.

**Tidak dijalankan:**

| Pemeriksaan | Alasan |
| --- | --- |
| `npm run test:e2e` | Tidak diminta task; lingkungan tidak menyediakan backend, data pesanan, maupun dua akun dokter |
| `npm run test:uat` | Hanya dijalankan bila diminta secara eksplisit |
| `npm run dev` | `AGENTS.md` melarang menjalankan development server tanpa kebutuhan konkret |

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-18` — penandaan cito tercatat beserta pelaku dan waktunya | **Terpenuhi di sisi layar** | Layar tidak mengirim waktu maupun pelaku; keduanya diisi backend. Waktu penandaan ditampilkan pada baris pemeriksaan yang bertanda cito. Pembuktian pencatatannya adalah cakupan `BE-LAB-10`, dan sudah terbukti di sana |
| `AC-40` — penanda melekat pada pemeriksaan, bukan pada pesanan | **Terpenuhi** | Uji `S8`, ditambah tiga lapis penegakan pada bagian 3.3: tanpa kolom kesegeraan pada daftar, tanpa kontrol pada kartu pesanan, dan perintah yang menerima `examinationId` |
| `AC-43` — harga satuan, subtotal, dan total tampil saat memesan, tanpa baris tagihan terbentuk | **Terpenuhi** | Formulir memakai komponen pemilih katalog `FE-LAB-04` yang perhitungannya sudah diuji pada task itu. Seluruh perintah pada jalur ini adalah baca, kecuali `POST /lab-orders` yang tidak membentuk tagihan |

**Definition of Done.**

| Butir DoD | Status | Bukti |
| --- | --- | --- |
| Layar pesanan ada | **Terpenuhi** | Ketiga route terbit pada keluaran build: daftar, buat, dan detail |
| Penanda cito berada pada baris pemeriksaan | **Terpenuhi** | Uji `S8` dan bagian 3.3 |
| Kontrol tanpa kewenangan tersembunyi atau nonaktif sejak awal | **Terpenuhi sebagian** | Tiga dari empat keadaan ditegakkan layar dan dijaga uji `S1`, `S3`, dan `S4`. Keadaan keempat — dokter lain yang bukan pemesan — belum dapat dicegah layar karena responsnya tidak membawa identitas pemesan; lihat bagian 8 |
| Harga tampil saat memesan | **Terpenuhi** | Komponen pemilih katalog menampilkan harga satuan per baris, subtotal, dan total |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Satu peringatan lint tersisa, dari pola resolusi token route yang sama dengan modul rujukan. Satu peringatan memoization diperbaiki, bukan dibiarkan |
| Masalah yang diketahui — 1 | **Respons pesanan tidak membawa identitas dokter pemesan.** `LabOrderListResponse` maupun `LabOrderDetailResponse` tidak memuat `requestedByUserId`, sedangkan `SetUrgencyAsync` justru membandingkan penunjuk pengguna dengan ruas itu. Akibatnya layar hanya dapat mendekati `VAL-03` lewat peran: petugas non-dokter tidak melihat tombolnya sama sekali, tetapi dokter lain yang bukan pemesan masih melihatnya dan menerima `403`. **Perbaikannya milik backend** — menambahkan `requestedByUserId`, dan sebaiknya `requestedByUserName`, pada kedua respons itu. Ini temuan ketiga dari pola yang sama, setelah `VAL-33` pada `FE-LAB-02` dan `LabRejectionReason : SystemFlag` pada `FE-LAB-03`: **frontend tidak dapat menyembunyikan kontrol tanpa kewenangan bila responsnya tidak membawa identitas pelaku** |
| Masalah yang diketahui — 2 | **Pemeriksaan belum dapat ditambahkan dari layar ini.** `AddLabExaminationRequest` menuntut `SpecimenId`, dan perencanaan wadah adalah cakupan `FE-LAB-07`. Sampai task itu dikerjakan, panel pemeriksaan akan kosong untuk pesanan baru, dan keterangannya sudah menyebut ke mana pengguna harus pergi |
| Masalah yang diketahui — 3 | **Cacat `variant` pada pembuat toast bersama.** `ToastStack` membaca `variant`, sedangkan pembuat toast pada modul rujukan `job-level` — dan seluruh modul master data yang menyalinnya — hanya mengisi `type`, sehingga notifikasi gagal tidak pernah tampil merah. Dua berkas milik `FE-LAB-02` dan `FE-LAB-03` sudah diperbaiki; modul lain **tidak disentuh** karena di luar cakupan, dan perbaikannya layak dijadikan task tersendiri |
| Dependency backend | `NONE` yang menahan task ini. `BE-LAB-10` dan `BE-LAB-16` sudah selesai dan diverifikasi pada source `2dfc4f2`. **`BE-LAB-08` tetap belum ada**, dan itulah yang membuat `FE-LAB-05` masih terblokir |
| Perubahan sampingan | Dua berkas utils milik `FE-LAB-02` dan `FE-LAB-03` disunting untuk memperbaiki ruas `variant` pada notifikasi. Keduanya pekerjaan sendiri pada sesi yang sama, bukan modul orang lain, dan perbaikannya backward-compatible |
| Interupsi | `NONE` |
| Status Git | Lihat blok di bawah tabel ini |
| Langkah berikutnya | `FE-LAB-07` — layar wadah dan pemeriksaan. Endpoint pasangannya `BE-LAB-12` sudah selesai. Risikonya **tinggi**: dua invariant keselamatan sekaligus, `LAB-FE-009` (seluruh pemeriksaan yang ditopang satu wadah terlihat sebelum tombol tolak) dan `LAB-FE-010` (peringatan bahwa menolak wadah menggugurkan semuanya). Task itu juga yang membuka jalur menambahkan pemeriksaan, sehingga panel pada layar detail pesanan akan terisi |

```text
 M src/lib/services/health-services/laboratory-management/lab-order.service.js
 M src/lib/state/slice/health-services/laboratory-management/lab-order-slice.jsx
 M src/lib/hooks/health-services/laboratory-management/use-lab-catalog-picker.jsx
 M src/components/features/health-services/laboratory-management/lab-catalog-picker/lab-catalog-picker.jsx
 M src/utils/health-services/master-data/lab-value-bounds/lab-value-bound-utils.jsx
 M src/utils/health-services/master-data/lab-rejection-reasons/lab-rejection-reason-utils.jsx
 M src/utils/menu-sidebar/menu-items.jsx
?? src/app/health-services/laboratory-management/lab-orders/
?? src/components/view/health-services/laboratory-management/lab-orders/
?? src/lib/constants/health-services/laboratory-management/lab-order-constants.jsx
?? src/lib/hooks/health-services/laboratory-management/lab-order-urgency-rules.js
?? src/lib/hooks/health-services/laboratory-management/use-lab-order-detail.jsx
?? src/lib/hooks/health-services/laboratory-management/use-lab-order-form.jsx
?? src/lib/hooks/health-services/laboratory-management/use-lab-order-list.jsx
?? src/lib/services/health-services/laboratory-management/lab-examination.service.js
?? src/style/health-services/laboratory-management/lab-orders/
?? tests/unit/lab-order-urgency-rules.test.mjs
```

Blok di atas hanya memuat perubahan `FE-LAB-06`. Perubahan `FE-LAB-02`, `FE-LAB-03`, dan
`FE-LAB-04` yang belum di-commit masih berdiri berdampingan pada working tree yang sama.

Tidak ada `git add`, commit, push, merge, rebase, maupun perpindahan branch yang dilakukan pada
kedua repository.
