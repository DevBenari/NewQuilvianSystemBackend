# Laporan Perubahan Frontend — `FE-RWI-026`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-026` |
| Judul | Tempat tidur dicari lalu dipesan — titik tulis 2 |
| Slice | Langkah **Pilih Bed** dan **Booking Bed** pada alur admisi dua jalur; titik tulis kedua |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian 4, entri `FE-RWI-026` |
| Trace | `RWI-CAP-006` **Wajib**; `FLOW-RI-MVP-001` langkah 5; `03-frontend-architecture.md` 3A.2 langkah 5–6, 3A.4 titik tulis 2, 3A.5, 3A.6, 4.3A; `05-skema-tampilan.md` bagian 3.7–3.8; `RWI-DEC-039`, `RWI-DEC-076`; `RWI-UI-GAP-003` |
| Contract version | API `0.4.0`; permission/audit `0.4.0`. Kontrak dibaca langsung dari source backend `InpatientBedOccupancyController.cs` dan `InpatientBedOccupancyDtos.cs` |
| Wewenang UI | Bentuk penandaan tempat tidur `DEV_DISCRETION`. **Batasnya:** sisa waktu pemesanan wajib terbaca — batas ini dipatuhi, lihat bagian 3.4 |
| Dependency | `FE-RWI-025` (**selesai 31 Agustus 2026**). Tidak ada dependency backend baru: ketiga endpoint sudah ada sejak `BE-RWI-010` |
| Klasifikasi | `HEAVY` — skor 9: repository 0, berkas diperiksa 2, berkas diubah 2, logika bisnis 2, kontrak API 1, database 0, keamanan/auth 0, UI/workflow 2 |
| Task mode | `FRONTEND` — backend strict read-only, kecuali laporan dan register modul ini |
| Target tulis | `QuilvianSystemFrontendDev` untuk source; `NewQuilvianSystemBackend` **hanya** untuk laporan ini beserta tautan buktinya pada roadmap dan `requirement-traceability.md` |
| Model | Claude Opus 5 |
| Commit frontend saat dikerjakan | `133c2390fe6bd6c19bfa2958bd8871185c12b64d`, branch `HamzahV2` |
| Commit backend yang dijadikan rujukan | `85439d32a884adcde3067304774151b317b058a2`, branch `MHamzah` |
| Tanggal | 1 September 2026 |
| Status | ✅ **SELESAI 1 September 2026.** Keenam acceptance criteria dipetakan ke bukti implementasi; `npm run lint` `0 errors`, `npm run build` `✓ Compiled successfully`, dan 24 test unit yang menyentuh berkas task ini lulus. Butir DoD e2e **dikecualikan atas keputusan pengguna 1 September 2026**; alasan teknisnya tetap berlaku dan tidak dihapus — data master rawat inap pada environment target belum layak (`RWI-UI-GAP-007` dan baris "Kesiapan data master" pada roadmap), rinciannya di bagian 6 dan 7.1 |

---

## 1. Keadaan yang ditemukan di awal

Alur admisi berlangkah sudah berdiri sampai langkah **Dokter**, lalu berhenti di sana.

| Yang sudah ada | Bukti |
| --- | --- |
| Kerangka alur dua jalur, langkah tersimpan di URL | `use-inpatient-admission-flow.jsx` (`FE-RWI-022`) |
| Langkah Pembayaran dan langkah Dokter, termasuk titik tulis 1 | `use-inpatient-admission-payment.jsx`, `use-inpatient-admission-doctor.jsx` (`FE-RWI-024`, `FE-RWI-025`) |
| Papan tempat tidur beserta penandaan boleh-dipilih dari `available-beds` | `inpatient-bed-board.jsx`, `use-inpatient-bed-board.jsx` (`FE-RWI-005`) |
| Penerjemah penolakan 409/422 beserta daftar aturan kelayakan | `placement-failure-list.jsx`, `inpatient-placement-utils.jsx` (`FE-RWI-007`) |
| Ketiga endpoint yang dibutuhkan | `InpatientBedOccupancyController.cs` — `GET /available-beds`, `POST /reservations`, `PATCH /reservations/{id}/cancel` |

| Yang belum ada | Akibatnya |
| --- | --- |
| Langkah **Pilih Bed** dan **Booking Bed** | Slug `bed-selection` dan `bed-booking` jatuh ke `PendingStep` — hanya menampilkan kalimat "akan dilengkapi task lanjutan". Alur mati total sesudah titik tulis 1 |
| Satu pun pemanggil `POST /bed-occupancies/reservations` | Pencarian di seluruh `src/` tidak menemukan pemanggil. Kemampuan `RWI-CAP-006` yang ditandai **Wajib** memang tidak pernah dibangun |
| Pembacaan `expiresAt` dan hitung mundurnya | Tidak ada satu pun utility yang membaca kolom waktu pemesanan |

**Akibat nyatanya bagi pengguna.** Episode `Draft` sudah terbentuk pada titik tulis 1, tetapi tempat
tidurnya tidak pernah ditahan. Dua petugas yang bekerja bersamaan dapat mengarahkan dua pasien ke
tempat tidur yang sama, dan tabrakannya baru ketahuan saat penempatan — ketika pasien sudah datang.

---

## 2. Proses bisnis dari sisi pengguna

**Siapa penggunanya.** Petugas admisi yang memiliki hak akses `InpatientBedOccupancy : Read` untuk
melihat daftar dan `InpatientBedOccupancy : Create` untuk memesan.

**Kapan layar ini dibuka.** Setelah langkah Dokter berhasil disimpan, yaitu ketika kunjungan dan
episode `Draft` sudah terbentuk. Selama episodenya belum terbaca, kedua langkah ini menolak bekerja
dan menyuruh petugas kembali ke langkah Dokter.

### 2.1 Langkah 5 — Pilih Bed

1. Petugas melihat lima angka ringkasan keadaan tempat tidur: Total Tempat Tidur, Tersedia, Terisi, Dipesan, dan Ditutup.
2. Di bawahnya ada kalimat "N tempat tidur dapat dipilih untuk pasien ini". Angka N adalah jumlah tempat tidur yang **dijawab server** untuk episode tersebut, bukan hasil hitungan layar.
3. Petugas boleh menyaring berdasarkan Unit Layanan, Kamar, dan Kelas, atau mencari kode/nama tempat tidur dan nama kamar.
4. Daftar tampil dikelompokkan per unit layanan, lalu per kamar, lalu per tempat tidur.
5. Tempat tidur yang **tidak** dapat dipakai pasien ini tampil redup, tombol pilihnya mati, dan alasannya ditulis di sebelahnya — misalnya "Terisi", "Dipesan", "Maintenance", atau "Tidak lolos kelayakan".
6. Sakelar **Sembunyikan yang tidak dapat dipakai** tersedia di baris penyaring. Bawaannya menampilkan semuanya, karena petugas perlu tahu tempat tidurnya ada tetapi tidak boleh dipakai — bukan mengira kamarnya penuh.
7. Petugas menekan **Pilih** pada satu tempat tidur, lalu menekan **Lanjut ke Pemesanan**.

### 2.2 Langkah 6 — Booking Bed, sebelum dipesan

1. Layar menampilkan kartu **Tempat Tidur Dipilih** berisi nama tempat tidur, kamar, kelas kamar, dan unit layanan.
2. Dua keterangan wajib ikut tampil: masa pemesanan mengikuti pengaturan server, dan pasien baru menjadi Sedang Dirawat setelah kedatangannya dikonfirmasi di Papan Tempat Tidur.
3. Petugas menekan **Pesan Tempat Tidur**. Dialog konfirmasi menyebut nama tempat tidurnya lebih dulu.
4. Setelah dikonfirmasi, `POST /bed-occupancies/reservations` dikirim.

### 2.3 Langkah 6 — Booking Bed, sesudah dipesan

1. Judul berubah menjadi **Tempat Tidur Sudah Dipesan**, dan di sebelahnya muncul lencana `Dipesan` beserta **Sisa waktu** yang berdetak setiap detik dalam format jam:menit:detik.
2. Kartu **Rincian Pemesanan** menampilkan tempat tidur, kamar, nomor episode, waktu dipesan, dan waktu berlaku sampai.
3. Tombol **Batalkan Pemesanan** tersedia; tombol **Lanjut ke Konfirmasi** menjadi aktif.
4. Bila petugas menekan **Kembali** selagi pemesanan masih berjalan, layar **menolak** dan meminta pemesanannya dibatalkan lebih dulu.

### 2.4 Jalur tidak normal

| Keadaan | Yang dilihat pengguna |
| --- | --- |
| Halaman dimuat ulang di langkah tempat tidur | Peringatan merah: episode belum terbaca, kembali ke langkah Dokter lalu simpan admisi. Papan tidak melakukan permintaan apa pun |
| Tempat tidur direbut petugas lain — `409` | Pesan server apa adanya, daftar **dimuat ulang otomatis**, dan kalimat "Pilih tempat tidur lain; isian yang sudah Anda ketik tetap tersimpan di layar ini" |
| Tempat tidur tidak dapat dipesan — `422` `BED_NOT_RESERVABLE` | Pesan server "Tempat tidur ini tidak dapat dipesan." tampil apa adanya beserta nomor aturan yang gagal |
| Episode sudah memesan tempat tidur lain — `409` | Pesan server "Episode ini sudah memesan tempat tidur lain. Batalkan dulu pemesanan sebelumnya." tampil apa adanya |
| Masa pemesanan habis saat layar terbuka | Papan dimuat ulang, pemesanan dilepas dari layar, petugas dikembalikan ke langkah Pilih Bed disertai kalimat bahwa pemesanan sebelumnya sudah habis |
| Pemesanan dibatalkan | Pilihan tempat tidur ikut dilepas, papan dimuat ulang, dan petugas dikembalikan ke langkah Pilih Bed |
| Tombol mundur peramban dipakai selagi pemesanan aktif | Langkah Pilih Bed terbuka, tetapi tempat tidur yang dipesan terbaca `Dipesan` beserta sisa waktunya, dan peringatan kuning menyebut pemesanan masih berjalan |
| Tempat tidur terpilih hilang dari daftar | Peringatan kuning: tempat tidur yang dipilih sudah tidak tersedia, kembali ke langkah Pilih Bed lalu pilih yang lain |
| Gagal mengambil papan | Pesan merah dari `loadError`, penyaring tetap utuh |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

**Tata kelola**

- `AGENTS.md` frontend; `rules/GLOBAL_RULES.md`; `rules/frontend/frontend-architecture.md`; `rules/frontend/base-component-catalog.md`; `rules/frontend/base-component-decision-gate.md`; `rules/frontend/design-tokens.md`; `rules/frontend/page-composition-patterns.md`; `rules/frontend/ui-consistency-checklist.md`; `rules/frontend/test-policy.md`; `rules/frontend/REPORT_TEMPLATE.md`; `rules/rule-output/lokasi-laporan-task.md`

**Blueprint dan register**

- `05-skema-tampilan.md` bagian 3.7–3.8; `roadmap/frontend-roadmap.md` bagian 0, 4, dan 6; `roadmap/requirement-traceability.md`; laporan `FE-RWI-025.md`

**Source backend (read-only, sebagai kontrak otoritatif)**

- `InpatientBedOccupancyController.cs` — rute, hak akses, dan pemetaan status penolakan
- `InpatientBedOccupancyDtos.cs` — `AvailableBedQuery`, `AvailableBedResponse`, `BedBoardResponse`, `ReserveBedRequest`, `CancelReservationRequest`, `BedReservationResponse`
- `InpBedOccupancyService.cs` — `SearchAvailableBedsAsync`, `ReserveBedAsync`, `CancelReservationAsync`, `GetReservationAsync`, `EvaluatePlacementEligibilityAsync`
- `InpBedReservationStatus.cs`; `Repositories/Configurations/.../InpBedReservationConfiguration.cs`; `Migrations/ApplicationDbContextModelSnapshot.cs` untuk tipe kolom waktu

**Source frontend**

- `inpatient-admission-view.jsx`, `inpatient-admission-doctor-step.jsx`, `inpatient-admission-payment-step.jsx`
- `use-inpatient-admission-flow.jsx`, `use-inpatient-admission-doctor.jsx`, `use-inpatient-bed-board.jsx`, `use-select-resource.jsx`
- `inpatient-bed-board.jsx`, `placement-failure-list.jsx`, `inpatient-bed-board-view.jsx`
- `inpatient-bed-utils.jsx`, `inpatient-placement-utils.jsx`, `inpatient-episode-utils.jsx`, `inpatient-setting-utils.jsx`
- `base-detail-card.jsx`, `confirm-modal.jsx`, `data-filter.jsx`, `status-badge.jsx`, `base-button.jsx`, `base-form-control.jsx`
- `inpatient-api.service.js`, `bed-occupancy.service.js`
- `tests/unit/inpatient-bed-board.test.mjs`, `tests/unit/inpatient-placement.test.mjs`, `tests/unit/inpatient-admission.test.mjs`, `tests/unit/base-components-regression.test.mjs`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-bed.jsx` (baru) | Controller kedua langkah tempat tidur: pemesanan, pembatalan, hitung mundur, penjagaan mundur, penjagaan klik ganda, dan penerjemahan penolakan 409/422 |
| `src/components/view/health-services/inpatient-management/inpatient-admission-bed-step.jsx` (baru) | Dua tampilan langkah — `InpatientAdmissionBedSelectionStep` dan `InpatientAdmissionBedBookingStep` — dalam satu berkas, mengikuti pola `inpatient-admission-existing-patient-step.jsx` |
| `src/lib/constants/health-services/inpatient-management/inpatient-admission-flow-constants.jsx` | Menambah bagian `FE-RWI-026`: slug kedua langkah, sembilan kalimat tetap, dan salinan dua dialog konfirmasi |
| `src/lib/constants/health-services/inpatient-management/inpatient-bed-board-constants.jsx` | Menambah nilai enum `InpBedReservationStatus` beserta label Bahasa Indonesianya |
| `src/utils/health-services/inpatient-management/inpatient-bed-utils.jsx` | Menambah pembaca waktu server yang tahan zona, normalisasi `BedReservationResponse`, hitung sisa waktu, format jam:menit:detik, perangkai lokasi tempat tidur, dan dua pembangun payload |
| `src/utils/health-services/inpatient-management/inpatient-placement-utils.jsx` | `parsePlacementFailure` menerima kalimat cadangan opsional. Nilai bawaannya tidak berubah, sehingga pemanggil penempatan tidak terdampak |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-bed-board.jsx` | Menambah opsi `enabled` (bawaan `true`). Dipakai supaya controller boleh hidup di level halaman tanpa memicu permintaan sebelum langkah tempat tidur terbuka |
| `src/components/features/health-services/inpatient-management/inpatient-bed-board.jsx` | Menambah empat prop opsional: `showUnavailable`, `onToggleShowUnavailable`, `reservedBedId`, `reservedBedDetail`. Seluruh nilai bawaannya menghasilkan tampilan yang sama persis dengan sebelumnya |
| `src/components/features/health-services/inpatient-management/placement-failure-list.jsx` | Dua judul dapat diganti pemanggil lewat prop `conflictTitle` dan `ruleTitle`; bawaannya tetap kalimat penempatan |
| `src/components/view/health-services/inpatient-management/inpatient-admission-view.jsx` | Memasang controller tempat tidur di level view dan mengarahkan slug `bed-selection` serta `bed-booking` ke langkahnya |
| `src/style/health-services/inpatient-management/inpatient-admission.module.css` | Menambah tata letak kedua langkah. Hanya tata letak — nol warna literal, nol typography, nol `!important` |

### 3.3 Gerbang keputusan base component

`UI GATE: 12 elemen — REUSE 10, EXTEND 1, COMPOSE 1, WRAP 0, NEW 0`

Tidak ada elemen berstatus `NEW`, sehingga tidak ada butir yang menunggu keputusan pengguna.

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Judul langkah | `admissionStyles.sectionHeading` | Dipakai seluruh langkah alur ini | REUSE | Dipakai apa adanya |
| Ringkasan keadaan tempat tidur | `InpatientBedBoard` | `features/health-services/inpatient-management/inpatient-bed-board.jsx` | REUSE | Sudah memuat kelima angka |
| Baris jumlah yang dapat dipilih | `InpatientBedBoard` prop `selectable` | idem | REUSE | Dinyalakan lewat `selectable` |
| Penyaring unit, kamar, kelas, dan search | `DataFilter`, `ResourceFilterSelect` | Dirangkai di dalam `InpatientBedBoard` | REUSE | Dipakai apa adanya |
| Daftar kamar dan baris tempat tidur | `InpatientBedBoard` | idem | REUSE | Dipakai apa adanya |
| Sakelar tampilkan yang tidak dapat dipakai | `InpatientBedBoard` — belum punya | Pencarian pada berkas komponen: tidak ada prop sakelar | **EXTEND** | Dua prop opsional; lihat keputusan 1 |
| Kartu tempat tidur terpilih | `BaseDetailCard` | `base-features/base-detail-card.jsx`, dipakai `BaseDetailView` | REUSE | Memakai kontrak `item` + `rows` |
| Kartu pemesanan aktif beserta sisa waktu | `BaseDetailCard` + `StatusBadge` | idem, dan `base-features/status-badge.jsx` | **COMPOSE** | Dirangkai di view; lihat keputusan 2 |
| Dialog konfirmasi pesan dan batal | `ConfirmModal` | `base-features/confirm-modal.jsx` | REUSE | Varian `info` dan `danger` |
| Penolakan 409/422 beserta daftar aturan | `PlacementFailureList` | `features/health-services/inpatient-management/placement-failure-list.jsx` | REUSE | Judul diganti lewat prop yang ditambahkan |
| Peringatan dan pesan galat | `InformationAlert` | `base-features/information-alert.jsx` | REUSE | Varian `info`/`warning`/`danger`/`success` |
| Tombol aksi | `BaseButton` | `base-features/base-button.jsx` | REUSE | Varian `primary`/`secondary`/`danger`/`subtle` |

#### Keputusan 1 — sakelar "tampilkan juga yang tidak dapat dipakai"

Skema tampilan 3.7 menaruh sakelar ini di baris penyaring, sedangkan `InpatientBedBoard` belum
memilikinya.

- **A. Extend `InpatientBedBoard` dengan dua prop opsional `showUnavailable` dan `onToggleShowUnavailable` — Rekomendasi, dan inilah yang dijalankan.** Sakelarnya duduk tepat di baris penyaring seperti gambar skema, dirender memakai `BaseButton` ber-`aria-pressed`, dan hanya muncul ketika pemanggil mengirim penanganannya. Layar papan berdiri sendiri tidak mengirim keduanya, sehingga tampilannya tidak berubah satu piksel pun. Biaya paling kecil dan risiko regresi paling kecil.
- **B. Menaruh sakelar di langkah admisi lalu mengirim `serviceUnits` yang sudah disaring.** Komponen bersama tidak disentuh sama sekali, tetapi sakelarnya terlempar ke atas papan — bukan di baris penyaring — dan penyaringan tampilan berpindah ke lapisan view. Itu membuat layar terlihat seolah ikut menyaring kelayakan, persis kesan yang dilarang kriteria 1.
- **C. Tidak membuat sakelarnya.** Paling murah, tetapi menghapus satu wilayah yang disebut skema tanpa alasan.

Opsi A adalah `EXTEND` yang **tidak** mengubah perilaku bawaan base component, sehingga menurut
gerbang keputusan ia tidak menunggu keputusan pengguna. Bila pemilik menghendaki B atau C,
perubahannya kecil dan hanya menyentuh dua berkas.

#### Keputusan 2 — kartu pemesanan aktif beserta sisa waktu

- **A. Compose `BaseDetailCard` untuk rinciannya, ditambah `StatusBadge` dan teks sisa waktu di kepala langkah — Rekomendasi, dan inilah yang dijalankan.** Seluruh unsurnya base component; yang ditambahkan hanya tata letak. Tidak ada typography baru sama sekali.
- **B. Komponen kartu pemesanan baru.** Paling bebas secara visual, tetapi menduplikasi kontrak kartu detail dan berisiko menyimpang ketika token kartu berubah. Berstatus `NEW`, sehingga wajib menunggu persetujuan.

#### Catatan penting — `BaseDetailCard` tidak menerima `children`

`BaseDetailCard` **mengabaikan** `children`; isinya hanya dirender dari pasangan `item` dan `rows`.
Karena itu kedua kartu pada task ini memakai kontrak `item` + `rows`, bukan `children`. Temuan
turunannya pada langkah Dokter dicatat di bagian 8.2 — tidak diperbaiki task ini karena berada di
luar wewenangnya.

#### Catatan — memakai ulang `InpatientBedBoard` yang bergaya lama

`InpatientBedBoard` masih memakai gaya Bootstrap lama: `fs-5 fw-semibold` pada angka ringkasan,
`<button className="btn btn-primary btn-sm">` pada tombol pilih, dan `fw-semibold` pada nama tempat
tidur. Seluruhnya **sudah ada sejak `FE-RWI-005`** dan tidak dibuat task ini — buktinya pada
bagian 6, baris grep 3 dan 5.

Perbaikan visual papan adalah scope `FE-RWI-036`, yang berstatus `BLOCKED` menunggu approval
skema/roadmap serta gap 003 dan 007. Merapikannya sekarang berarti mengerjakan task yang sedang
diblokir sekaligus mengambil risiko regresi pada layar papan berdiri sendiri, sehingga tidak
dilakukan. Roadmap `FE-RWI-026` sendiri secara eksplisit memerintahkan memakai ulang komponen ini.

### 3.4 Kepatuhan arsitektur frontend

| Aspek | Kepatuhan |
| --- | --- |
| Alur dependensi | `view → hook → service → InstanceAxios`. Komponen tidak memanggil Axios langsung |
| Penempatan folder | Hook di `src/lib/hooks/health-services/inpatient-management/`, view di `src/components/view/...`, utility di `src/utils/...`, konstanta di `src/lib/constants/...` — sama persis dengan `FE-RWI-024` dan `FE-RWI-025` |
| Pola HTTP | `bedOccupancyService` yang sudah ada, dibangun `createInpatientApiService`. Tidak ada Axios instance baru, tidak ada service baru |
| Pola state | `useState` lokal pada hook di level view, sama seperti `useInpatientAdmissionPayment` dan `useInpatientAdmissionDoctor`. Tidak ada Redux slice baru |
| Endpoint | Berasal dari `INPATIENT_API_BASE_URLS.bedOccupancies`; tidak ada string endpoint yang tersebar di view |
| Design token | Seluruh nilai visual memakai `var(--token)`. Nol `#hex`, nol `rgba()`, nol `px` mentah, nol `!important`, nol `font-size`/`font-weight`/`line-height` baru |
| Batas wewenang UI | Sisa waktu pemesanan terbaca di dua tempat: hitung mundur jam:menit:detik yang berdetak di kepala langkah Booking Bed, dan pada baris tempat tidur di papan ketika langkah Pilih Bed dibuka selagi pemesanan berjalan |
| Aturan satu sumber kelayakan | Layar tidak pernah menghitung ulang kelayakan. `buildSelectableBedIndex` hanya mencatat id yang dijawab `available-beds`, dan sakelar tampilan hanya menyembunyikan baris yang server sudah nyatakan tidak dapat dipilih |
| Pola baru | `NONE` |

### 3.5 Tiga keputusan teknis yang perlu dicatat

**Kenapa controller-nya dipasang di level view.** Kedua langkah tempat tidur berbagi satu
controller supaya tempat tidur terpilih dan pemesanannya tidak hilang saat berpindah di antara
keduanya, dan tetap terbaca ketika langkah Konfirmasi dibuka `FE-RWI-027`. Konsekuensinya,
`useInpatientBedBoard` akan ikut hidup sejak langkah pertama. Karena itu ia diberi opsi `enabled`:
tanpa itu, papan dan ketiga opsi penyaringnya akan menembak lima permintaan pada setiap pembukaan
halaman admisi, termasuk saat petugas masih di layar Tipe Pasien. Lebih buruk lagi, permintaan itu
akan berjalan **tanpa** `episodeId` — dan tanpa `episodeId`, `available-beds` menjawab pertanyaan
yang berbeda.

**Kenapa hitung mundur tidak memutuskan apa pun.** Angka mundur diturunkan dari `expiresAt` jawaban
server. Ketika habis, layar **memuat ulang papan** lalu memakai keadaan tempat tidur yang baru
dijawab server; ia tidak menyatakan sendiri bahwa pemesanannya gugur. Ini `03-frontend-architecture.md`
3A.6. Seluruh perubahan state terjadi di dalam callback interval, bukan di badan effect, sehingga
tidak ada render berantai.

**Kenapa kolom waktu dibaca dengan penjaga zona.** `ReservedAt` dan `ExpiresAt` disimpan sebagai
`timestamp with time zone`, sehingga server mengirimnya lengkap dengan penanda zona. Meski begitu
`parseInpatientServerDateTime` tetap menambahkan penanda UTC bila suatu saat penandanya hilang.
Salah menebak zona pada hitung mundur bukan salah kosmetik: sisa waktu bisa meleset berjam-jam, dan
petugas akan melihat pemesanan yang tampak masih lama padahal sudah gugur.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | "Mengambil papan ketersediaan..." di atas daftar; penyaring tetap dapat dipakai |
| Kosong | "Belum ada tempat tidur yang cocok". Kalimatnya membedakan dua sebab: tidak ada yang cocok dengan penyaring, atau tidak ada yang **dapat dipilih** karena sakelar sedang menyembunyikan yang tidak dapat dipakai |
| Menyimpan | Tombol berubah menjadi "Memesan..." atau "Membatalkan...", dan tombol lain dinonaktifkan sehingga klik kedua tidak mungkin |
| Gagal memuat papan | Pesan merah dari `loadError` di atas penyaring; penyaring dan pilihan tetap utuh |
| Gagal memesan — aturan bisnis | Judul "Pemesanan ditolak aturan kelayakan", kalimat server apa adanya, lalu daftar nomor aturan yang gagal |
| Gagal memesan — keadaan berubah | Judul "Keadaan tempat tidur sudah berubah", daftar dimuat ulang, dan kalimat bahwa isian tetap tersimpan |
| Episode belum terbaca | Peringatan merah beserta arahan kembali ke langkah Dokter; papan tidak dirender dan tidak melakukan permintaan apa pun |
| Pemesanan gugur | Peringatan kuning beserta pengembalian otomatis ke langkah Pilih Bed |
| Tanpa hak akses | `NOT APPLICABLE` di lapisan layar ini — penjagaan `403` ditangani `InstanceAxios` dan `access-denied-gate` pada tingkat route, tidak diubah task ini |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Inpatient Management / Bed Occupancy

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/inpatient-management/bed-occupancies/available-beds?episodeId={id}` | Menentukan tempat tidur mana yang **boleh dipilih** untuk episode ini. `episodeId` wajib dikirim; tanpa itu server menjawab pertanyaan yang berbeda | `InpatientBedOccupancy : Read` |
| `GET` | `/v1/health-services/inpatient-management/bed-occupancies/bed-board` | Menampilkan seluruh tempat tidur beserta keadaannya, termasuk yang tidak dapat dipakai beserta alasannya | `InpatientBedOccupancy : Read` |
| `POST` | `/v1/health-services/inpatient-management/bed-occupancies/reservations` | Menahan satu tempat tidur atas nama episode `Draft` | `InpatientBedOccupancy : Create` |
| `PATCH` | `/v1/health-services/inpatient-management/bed-occupancies/reservations/{id}/cancel` | Melepas pemesanan sebelum dipakai | `InpatientBedOccupancy : Update` |

Ketiga endpoint tulis dan baca di atas **sudah ada** sebelum task ini; tidak ada permintaan
perubahan backend.

**Yang sengaja tidak dipanggil.** `POST /bed-occupancies/placements` **tidak pernah** dipanggil
kedua layar ini. `RWI-DEC-076` menetapkan pasien baru menjadi `Admitted` ketika kedatangannya
dikonfirmasi di Papan Tempat Tidur — pekerjaan `FE-RWI-030`. Pencarian pada kedua berkas view dan
hook task ini tidak menemukan satu pun kemunculan `placements`.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | Tanpa keluaran, artinya tanpa error | `PASS` | Keluaran perintah |
| `npm run lint` | `571 problems (0 errors, 571 warnings)` — jumlahnya **sama persis** dengan garis dasar sebelum task ini | `PASS` | Keluaran perintah |
| Warning lint pada berkas yang dibuat/diubah task ini | Nol | `PASS` | Keluaran lint disaring kesebelas nama berkas |
| `npm run build` | `✓ Compiled successfully in 35.6s`; route `/health-services/inpatient-management/admissions` terbentuk | `PASS` | Keluaran perintah |
| `node --test tests/unit/inpatient-bed-board.test.mjs tests/unit/inpatient-placement.test.mjs tests/unit/base-components-regression.test.mjs tests/unit/inpatient-bed-status.test.mjs` | `pass 24, fail 0` | `PASS` | Keluaran perintah |
| `npm run test:unit` — seluruh direktori | Gagal sebelum satu test pun berjalan: `ERR_UNSUPPORTED_DIR_IMPORT` pada `tests/unit` | `EXISTING / ENVIRONMENT ISSUE` | Script memberikan direktori ke `node --test` melalui loader `register.mjs`; tidak berjalan pada Node `v24.13.0` yang terpasang. Tidak berhubungan dengan perubahan task ini |
| `tests/unit/inpatient-admission.test.mjs` — "menyetel kebutuhan isolasi memicu pembacaan ulang tempat tidur" | Gagal | `UNRELATED EXISTING ISSUE` | Test menuntut `inpatient-admission-view.jsx` memuat `refreshToken: admission.bedRefreshToken`. `git show HEAD:...` membuktikan string itu **sudah tidak ada di commit `133c2390`**, yaitu sebelum task ini menyentuh berkasnya. Test tersebut tertinggal dari era `FE-RWI-005` dan sudah usang sejak `FE-RWI-022` mengganti view-nya. Pembersihannya milik `FE-RWI-035`, bukan task ini |
| Grep anti-regresi 1 — warna literal pada blok CSS baru | Nol temuan | `PASS` | `grep -nEi "#[0-9a-f]{3,8}\b\|rgba?\("` pada blok `FE-RWI-026` |
| Grep anti-regresi 2 — typography pada blok CSS baru | Nol temuan | `PASS` | `grep -nE "font-size\|font-weight\|line-height"` pada blok `FE-RWI-026`. Delapan temuan lain pada berkas yang sama seluruhnya baris lama milik `FE-RWI-024`/`025` dan tidak disentuh |
| Grep anti-regresi 3 — tombol non-base pada berkas baru | Nol temuan | `PASS` | `grep -nE "<button\|className=\"btn"` pada kedua berkas baru |
| Grep anti-regresi 3b — tombol non-base pada berkas yang disentuh | Dua temuan, keduanya **sudah ada di HEAD** | `UNRELATED EXISTING ISSUE` | `inpatient-bed-board.jsx` baris 203 dan 206; `git show HEAD:...` menemukannya di baris 139 dan 142. Milik `FE-RWI-036` yang berstatus `BLOCKED` — lihat bagian 3.3 |
| Grep anti-regresi 4 — tabel mentah | Nol temuan | `PASS` | Kedua layar tidak memakai `<table>` |
| Grep anti-regresi 5 — utility `fw-`/`fs-` | Nol temuan pada berkas baru; dua temuan lama pada `inpatient-bed-board.jsx` | `UNRELATED EXISTING ISSUE` | Sama seperti baris di atas, terbukti ada di HEAD |
| Grep anti-regresi 6 — `!important` baru | Nol temuan | `PASS` | `grep -n "!important"` |
| Bukti e2e dan uji di peramban | Tidak dijalankan | `NOT RUN` | Alasannya di bawah |

`AUTOMATED TEST: node --test tests/unit/inpatient-bed-board.test.mjs tests/unit/inpatient-placement.test.mjs tests/unit/base-components-regression.test.mjs tests/unit/inpatient-bed-status.test.mjs — PASS (24/24)`

`AUTOMATED TEST: npm run test:unit — BLOCKED (ERR_UNSUPPORTED_DIR_IMPORT pada Node v24.13.0; runner memberikan direktori ke node --test lewat loader register.mjs)`

**Uji manual: `NOT FEASIBLE`.** Alasannya konkret dan sudah tercatat pada roadmap. Baris
"Kesiapan data master" pada bagian 6 roadmap menyatakan `RWI-DEC-063` — unit layanan bertipe rawat
inap, kamar, tempat tidur, kelas, dan penjamin — belum layak pada environment target, dan bahwa
"`FE-RWI-026` ke atas **tidak dapat dibuktikan dengan data nyata**". `RWI-UI-GAP-007` mencatat
buktinya: papan menampilkan nol tempat tidur pada screenshot pemilik. Tanpa satu pun tempat tidur,
tidak ada yang dapat dipesan, dibatalkan, atau direbut sesi kedua. Mengisi data itu dari peramban
secara eksplisit dinyatakan **di luar** roadmap frontend, bagian 7: wewenangnya milik Admin Master
Data dan seeder `BE-RWI-002`.

**Tidak dijalankan:** uji di peramban, uji e2e Playwright, dan `npm run test:uat`. Yang pertama dan
kedua terhalang kesiapan data di atas; repository ini juga tidak memiliki `playwright.config.*` di
akar. Ketiganya tidak diklaim lulus.

---

## 7. Acceptance criteria dan Definition of Done

| # | Kriteria persis seperti roadmap | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | Daftar tempat tidur berasal **hanya** dari `available-beds`; layar tidak menyaring ulang sendiri | **Terpenuhi** | `buildSelectableBedIndex` hanya mengumpulkan id yang dikembalikan `available-beds`, lalu `useInpatientBedBoard` memakai daftar itu apa adanya sebagai penanda `isSelectable`. Tidak ada satu pun cabang kelayakan di layar. Sakelar tampilan yang ditambahkan task ini hanya **menyembunyikan** baris yang server sudah nyatakan tidak dapat dipilih dan tidak pernah menambah satu pun tempat tidur ke daftar yang boleh dipilih |
| 2 | Tempat tidur yang tidak layak tampil sebagai baris nonaktif beserta alasannya dan **tidak dapat dipilih** | **Terpenuhi** | Baris memakai kelas `opacity-50`, tombol pilihnya `disabled={!bed.isSelectable}`, dan `describeBedUnavailability` menampilkan alasan dari kolom yang memang dikirim server. `selectBed` pada hook papan juga menolak `bed` yang tidak `isSelectable`, sehingga klik program pun tertahan |
| 3 | Pemesanan berhasil membuat tempat tidur terbaca `Reserved`, dan **sisa waktunya terbaca** | **Terpenuhi** | Dua tempat. Pada langkah Booking Bed, lencana `Dipesan` beserta hitung mundur jam:menit:detik yang berdetak setiap detik dari `expiresAt`. Pada papan, prop `reservedBedId` menandai baris tempat tidur itu `Dipesan` beserta sisa waktunya — penanda ini **wajib** ada karena tempat tidur yang dipesan episode itu sendiri tetap lolos `available-beds`, sehingga tanpa penanda barisnya akan terbaca "Dapat dipakai" |
| 4 | Tempat tidur ber-`IsReservable` salah ditolak dengan pesan server apa adanya | **Terpenuhi** | Layar sengaja **tidak** membaca `IsReservable` dan tidak memblokirnya lebih dulu — `EvaluatePlacementEligibilityAsync` hanya memeriksa penanda itu pada konteks `Reservation`, sehingga tempat tidur tersebut memang lolos pencarian. Ketika pemesanan ditolak `422` `BED_NOT_RESERVABLE`, `parsePlacementFailure` meneruskan kalimat server "Tempat tidur ini tidak dapat dipesan." apa adanya ke `PlacementFailureList` |
| 5 | Membatalkan pemesanan lalu memilih tempat tidur lain berhasil, dan **tidak** meninggalkan dua pemesanan aktif | **Terpenuhi** | Tiga lapis. Pertama, tombol Kembali pada langkah Booking Bed menolak mundur selagi pemesanan aktif dan meminta pembatalan lebih dulu — aturan 3A.5. Kedua, pembatalan yang berhasil melepas pemesanan **dan** pilihan tempat tidurnya, memuat ulang papan, lalu mengembalikan petugas ke langkah Pilih Bed. Ketiga, bila tombol mundur peramban dipakai untuk melewati penjagaan itu, backend tetap menolak dengan `409` "Episode ini sudah memesan tempat tidur lain" dan papan menandai tempat tidur yang sudah dipesan |
| 6 | 409 karena tempat tidur direbut memicu muat ulang daftar, dan isian tidak hilang | **Terpenuhi** | `parsed.shouldReloadBeds` bernilai benar tepat pada `409`, dan `refreshBoard()` dipanggil di jalur gagal. Tidak ada satu pun pemanggilan yang mengosongkan isian admisi: kunjungan, episode, penjamin, dan seluruh isian langkah Dokter dipegang controller yang berbeda dan tidak disentuh jalur ini |

### 7.1 Definition of Done

| Butir DoD | Status | Catatan |
| --- | --- | --- |
| Keenam kriteria lulus | **Terpenuhi pada level source** | Tabel 7. Pembuktian runtime menunggu data master |
| E2E ada dan lulus | **Belum terpenuhi** | Dinyatakan apa adanya. Roadmap sendiri mencatat `FE-RWI-026` ke atas tidak dapat dibuktikan dengan data nyata sampai `RWI-UI-GAP-007` dan `RWI-DEC-063` tertutup. Menulis e2e yang lulus dengan data tiruan justru dilarang gerbang skema roadmap: "e2e tidak boleh menyamarkannya dengan mock" |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `npm run lint` memulangkan 571 warning, seluruhnya dari berkas lama yang tidak disentuh task ini — jumlahnya sama persis dengan garis dasar. Berkas yang dibuat dan diubah task ini menghasilkan **nol** warning |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |

### 8.1 Delta kontrak dan skema yang dilaporkan

**Roadmap menyebut `PATCH /bed-occupancies/reservations/{id}/cancel`; source memakai rute yang
sama.** Tidak ada selisih. Untuk `FE-RWI-036`, roadmap menyebut `GET /bed-occupancies/board`
sedangkan source memakai `GET /bed-occupancies/bed-board`. Selisih itu **bukan** milik task ini,
tetapi dicatat di sini karena ditemukan saat membaca controller yang sama.

**`GET /bed-board` hanya menerima `serviceUnitId`.** `useInpatientBedBoard` mengirim `roomId`,
`patientClassId`, dan `search` ke endpoint papan, padahal `GetBedBoard` hanya menerima
`serviceUnitId`. Ketiga parameter lain diabaikan server, sehingga penyaring Kamar, Kelas, dan
pencarian **hanya berpengaruh pada `available-beds`** — daftar yang boleh dipilih menyempit, tetapi
papan tetap menampilkan seluruh kamar. Perilaku ini **sudah ada sejak `FE-RWI-005`** dan tidak
diubah task ini karena menyentuhnya berarti mengubah perilaku layar papan berdiri sendiri yang
menjadi scope `FE-RWI-036`. **Yang perlu diputuskan pemilik:** apakah `GET /bed-board` menambah
ketiga penyaring itu, atau papan memang dirancang selalu utuh sementara penyempitan hanya berlaku
pada daftar yang dapat dipilih.

**Langkah Pilih Bed belum membuka "mode koreksi" ke langkah Dokter.** Skema 3.7 menyebut tombol
Kembali membuka mode koreksi pascatitik tulis 1, tempat unit, kelas, dan catatan dapat diubah lewat
`PUT`. Scope `FE-RWI-026` hanya memuat `GET /available-beds`, `POST /reservations`, dan
`PATCH .../cancel`; `PUT /episodes/{id}` adalah scope `FE-RWI-027`. Tombol Kembali karena itu
mengembalikan petugas ke langkah Dokter apa adanya, dan langkah itu masih terkunci sesudah titik
tulis 1 sebagaimana `FE-RWI-025` merancangnya.

**Status roadmap dan skema.** `roadmap_revision: 5` berstatus `DRAFT` dan `05-skema-tampilan.md`
`0.4` masih draft, sehingga skema **tidak** diperlakukan sebagai brief UI yang mengikat. Yang
dipakai sebagai sumber mengikat adalah acceptance criteria dan wewenang UI pada kartu task
`FE-RWI-026` sendiri, yang berasal dari revision `3` — revision yang sudah disetujui. Skema dipakai
sebagai panduan bentuk, dan setiap penyimpangan darinya disebut di bagian ini. Cara yang sama sudah
ditempuh `FE-RWI-025`.

### 8.2 Masalah yang diketahui

| Masalah | Keterangan |
| --- | --- |
| **Panel tinjauan penjamin pada langkah Dokter tidak menampilkan isinya** | Ditemukan saat memeriksa `BaseDetailCard` untuk task ini. `PayerReviewPanel` di `inpatient-admission-doctor-step.jsx` mengirim isinya sebagai `children`, padahal `BaseDetailCard` tidak menerima `children` sama sekali — isinya hanya dirender dari pasangan `item` dan `rows`. Tanpa `item`, kartu itu merender `notFoundText`, yaitu "Data tidak ditemukan.". Akibatnya cara bayar, nama penjamin, dan nomor polis **tidak terlihat** petugas. Ini milik `FE-RWI-025`, bukan task ini, sehingga **tidak diperbaiki** sesuai aturan cakupan `AGENTS.md`. Perbaikannya kecil: mengganti `children` menjadi `item` + `rows`, persis seperti kedua kartu pada task ini |
| Pemesanan tidak dapat dibaca ulang setelah halaman dimuat ulang | `RWI-UI-GAP-003`. Tidak ada operasi baca yang mengembalikan pemesanan aktif sebuah episode. Karena episode dan kunjungan juga hanya tersimpan di memori sejak `FE-RWI-025`, memuat ulang halaman pada langkah tempat tidur menampilkan peringatan merah yang menyuruh kembali ke langkah Dokter. Layar **tidak** menebak-nebak keadaan pemesanan |
| Test usang `inpatient-admission.test.mjs` | Satu skenario menuntut view lama yang sudah diganti `FE-RWI-022`. Sudah gagal sebelum task ini. Pembersihannya scope `FE-RWI-035` |
| `npm run test:unit` tidak dapat dijalankan sebagai satu suite | `ERR_UNSUPPORTED_DIR_IMPORT` pada Node `v24.13.0`. Berkas test tetap dapat dijalankan satu per satu. Bukan scope task ini |
| Gaya lama pada `InpatientBedBoard` | Tombol dan angka ringkasan masih memakai utility Bootstrap. Scope `FE-RWI-036`, yang berstatus `BLOCKED` |

### 8.3 Dependency backend

| Dependency | Status |
| --- | --- |
| `BE-RWI-010` | **Selesai.** Ketiga endpoint yang dipakai task ini sudah ada dan sudah punya `[AccessAction]` serta `[AccessPermission]` yang benar |
| `RWI-UI-GAP-003` | **Terbuka.** Menahan pembuktian pada episode existing dan papan, serta memblokir `FE-RWI-032`. Tidak menahan alur baru, karena kartu task ini secara eksplisit mengizinkan alur baru memakai hasil `POST` pada sesi aktif |
| `RWI-UI-GAP-007` dan `RWI-DEC-063` | **Terbuka.** Menahan seluruh bukti runtime task ini. Owner: Admin Master Data dan Tim Master Data |
| `RWI-OQ-045` | **Terbuka.** Tidak menahan task ini; hak akses konfirmasi masuk baru dibutuhkan `FE-RWI-030` |

### 8.4 Status Git

Repository frontend `QuilvianSystemFrontendDev`, branch `HamzahV2`:

```text
 M src/components/features/health-services/inpatient-management/inpatient-bed-board.jsx
 M src/components/features/health-services/inpatient-management/placement-failure-list.jsx
 M src/components/view/health-services/inpatient-management/inpatient-admission-view.jsx
 M src/lib/constants/health-services/inpatient-management/inpatient-admission-flow-constants.jsx
 M src/lib/constants/health-services/inpatient-management/inpatient-bed-board-constants.jsx
 M src/lib/hooks/health-services/inpatient-management/use-inpatient-bed-board.jsx
 M src/style/health-services/inpatient-management/inpatient-admission.module.css
 M src/utils/health-services/inpatient-management/inpatient-bed-utils.jsx
 M src/utils/health-services/inpatient-management/inpatient-placement-utils.jsx
?? src/components/view/health-services/inpatient-management/inpatient-admission-bed-step.jsx
?? src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-bed.jsx
```

Repository backend `NewQuilvianSystemBackend`, branch `MHamzah`: hanya laporan ini beserta
pembaruan roadmap dan `requirement-traceability.md`.

Working tree kedua repository bersih ketika task dimulai. Tidak ada `git add`, `commit`, `push`,
`pull`, `merge`, `rebase`, atau `switch` yang dijalankan pada repository mana pun.

### 8.5 Langkah berikutnya

| Urutan | Langkah | Penanggung jawab |
| ---: | --- | --- |
| 1 | Mengisi data master rawat inap pada environment target — unit bertipe rawat inap, kamar, tempat tidur, kelas — supaya keenam acceptance criteria dapat dibuktikan di peramban dan e2e dapat ditulis tanpa data tiruan | Admin Master Data bersama Tim Master Data |
| 2 | Memperbaiki `PayerReviewPanel` pada langkah Dokter sesuai bagian 8.2, sebagai pengerjaan ulang `FE-RWI-025` | Frontend |
| 3 | Menutup `RWI-UI-GAP-003` dengan operasi baca pemesanan aktif per episode, supaya pemesanan bertahan melewati muat ulang halaman dan `FE-RWI-032` terbuka | Backend/API bersama Product/Domain |
| 4 | Memutuskan delta penyaring `GET /bed-board` pada bagian 8.1 | Product/Domain bersama Backend/API |
| 5 | Melanjutkan ke `FE-RWI-027` — Konfirmasi, titik tulis 3 | Frontend |


---

## Penutupan status — 1 September 2026

| Field | Isi |
| --- | --- |
| Status akhir | ✅ **SELESAI** |
| Dasar | Keputusan pemilik pekerjaan 1 September 2026: butir Definition of Done yang mensyaratkan test `.mjs`, E2E, atau uji manual **tidak lagi menahan status selesai** untuk task frontend yang seluruh acceptance criterianya sudah terpetakan ke source yang benar-benar ada. Dicatat pada [`frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian "Keputusan penutupan verifikasi" |
| Yang dikecualikan | Butir DoD e2e/`.mjs`/uji manual pada task ini |
| Yang tidak dihapus | Seluruh catatan verifikasi di atas tetap berlaku apa adanya. Alasan teknisnya — repository tanpa `playwright.config.*`, `npm run test:unit` gagal oleh `ERR_UNSUPPORTED_DIR_IMPORT` pada Node `v24.13.0`, dan data master rawat inap yang belum layak (`RWI-UI-GAP-007`) — tidak dianggap gugur |
| Pembuktian runtime ujung-ke-ujung | Tetap menjadi milik `FE-RWI-035` dan tidak dihapus dari roadmap |
| Register yang ikut diperbarui | [`frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md), [`requirement-traceability.md`](../../../roadmap/requirement-traceability.md) |
