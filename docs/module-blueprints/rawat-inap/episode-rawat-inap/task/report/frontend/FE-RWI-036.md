# Laporan Perubahan Frontend — `FE-RWI-036`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-036` |
| Judul | Papan Tempat Tidur kembali menjadi layar kerja |
| Slice | `F12 — Repair layar existing` |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian 5, `FE-RWI-036` |
| Trace | Bukti runtime pemilik 28 Agustus 2026; `FE-INP-02`; `RWI-DEC-076` |
| Skema tampilan | [`05-skema-tampilan.md`](../../../05-skema-tampilan.md) — bagian 7 (`7.1` kerangka, `7.2` wilayah, `7.3` tombol, `7.4` keadaan) dan bagian 24.1 |
| Contract version | API `0.4.0`; addendum `RWI-BED-BOARD-RESERVATION-001` `1.0.0` `APPROVED`; permission/audit `0.4.0`. Tidak ada kontrak baru yang diminta task ini |
| Wewenang UI | Susunan wilayah dan label aksi mengikuti skema bagian 7. Warna, ukuran kartu, dan ikon `DEV_DISCRETION` |
| Dependency | `FE-RWI-026` ✅ selesai, `FE-RWI-030` ✅ selesai, `BE-RWI-036` ✅ selesai. `RWI-UI-GAP-007` **masih terbuka** dan menahan pembuktian runtime |
| Klasifikasi | `MEDIUM` — enam berkas source disunting, satu stylesheet baru, dua berkas e2e disesuaikan, nol berkas dihapus; satu komponen bersama disentuh dengan prop opsional |
| Task mode | `FRONTEND` — backend strict read-only, kecuali berkas laporan dan register modul ini |
| Target tulis | `QuilvianSystemFrontendDev` untuk source; berkas laporan ini beserta roadmap dan `requirement-traceability.md` modul Rawat Inap |
| Model | Claude Opus 5 |
| Commit frontend saat dikerjakan | `2535c1303` pada branch `HamzahV2` |
| Commit backend yang dijadikan rujukan | `3d14cac` pada branch `MHamzah` |
| Tanggal | 1 September 2026 |
| Status | ✅ **SELESAI 1 September 2026.** Kelima kriteria yang dapat dibuktikan dari source terpenuhi; kriteria 3 terpenuhi **secara struktural** dan batasnya dicatat pada bagian 7. `npm run lint` `0 errors` — 571 warning, sama persis dengan garis dasar dan nol pada keenam berkas task ini; `npm run build` `✓ Compiled successfully in 31.5s`. Test `.mjs` dan uji manual `NOT RUN` atas arahan pengguna 1 September 2026. **Pemeriksaan tampilan pemilik 1 September 2026** menemukan area papan masih dirangkai dari utility Bootstrap mentah sehingga terlihat tanpa style; perbaikannya masuk task ini karena `Layout/status bed` memang bagian scope-nya — lihat bagian 3.5 |

---

## 0. Gerbang roadmap yang dilewati atas arahan pengguna

Roadmap frontend revision `5` berstatus `DRAFT` dengan `approval_gate:
UI_SCHEMA_APPROVAL_REQUIRED`, dan baris status `FE-RWI-036` sebelum task ini dikerjakan
berbunyi `⛔ BLOCKED — menunggu approval skema/roadmap`.

Task tetap dikerjakan karena pemilik pekerjaan memerintahkannya secara eksplisit pada
1 September 2026, dengan pola yang sama seperti `FE-RWI-031` s.d. `034` yang juga selesai di
bawah revision `5` yang masih `DRAFT`. Yang dicatat apa adanya: gerbang approval skema
**belum** dicabut, dan pencabutannya tetap menjadi keputusan pemilik, bukan konsekuensi
selesainya task ini.

---

## 1. Keadaan yang ditemukan di awal

Papan Tempat Tidur sebenarnya **tidak rusak dalam membaca data**. Ia memanggil endpoint yang
benar, menyusun unit layanan → kamar → tempat tidur dengan benar, dan menampilkan lima angka
ringkasan dengan benar. Yang hilang adalah segala sesuatu yang membuat sebuah layar dapat
dipakai bekerja.

| Yang diperiksa | Keadaan sebelum task ini |
| --- | --- |
| Aksi pada tempat tidur `Reserved` | Hanya **Konfirmasi Masuk** dari `FE-RWI-030`. **Batalkan Pesanan** tidak ada sama sekali di layar papan, padahal endpoint-nya sudah lama tersedia dan sudah dipakai langkah Booking Bed |
| Sisa waktu pemesanan | **Tidak terbaca.** Kolom `reservationExpiresAt` sudah dinormalisasi `FE-RWI-030`, tetapi tidak satu pun baris menampilkannya. Petugas melihat "Dipesan episode EP-xxx" tanpa tahu pemesanan itu akan gugur satu menit lagi atau satu jam lagi |
| Pemegang pemesanan | Hanya nomor episode. Nama pasiennya dikirim server pada kolom `patientName`, tetapi baris `Reserved` tidak memakainya |
| Membaca ulang | **Tidak ada satu pun jalan.** Tidak ada tombol muat ulang, dan layar tidak membaca ulang ketika jendela kembali difokuskan. Papan yang ditinggal terbuka satu jam menampilkan keadaan satu jam lalu |
| Gagal baca | Hanya kalimat merah. **Tidak ada Coba Lagi**, sehingga satu-satunya pemulihan adalah menyegarkan halaman peramban — dan itu menghapus seluruh penyaring yang sudah dipilih |
| Papan kosong | Satu kalimat untuk dua sebab yang berlawanan: "Tidak ada unit layanan atau kamar yang cocok dengan penyaring saat ini." Kalimat itu **salah** ketika penyebabnya adalah master tempat tidur yang memang belum pernah diisi, dan justru menyuruh petugas mengubah penyaring yang sebenarnya tidak bersalah |
| Tampilan papan | **Dirangkai dari utility Bootstrap mentah.** `border rounded p-2`, `fs-5 fw-semibold`, dan `list-unstyled` — nol token Quilvian. Akibatnya hero dan penyaringnya bergaya aplikasi, sedangkan papannya di bawah terlihat seperti halaman tanpa CSS. Ditemukan pemilik lewat pemeriksaan tampilan, bukan lewat lint atau build — keduanya memang lulus |
| Kamar tanpa tempat tidur | **Merender kotak kosong tanpa satu kata pun**, sehingga terbaca seperti layar yang gagal memuat separuh jalan |
| Ringkasan setelah gagal | **Angka lama dibiarkan.** Ketika pembacaan gagal, lima kartu ringkasan tetap menampilkan angka pembacaan sebelumnya, berdampingan dengan pesan gagal — dan tidak ada cara membedakannya dari angka yang baru saja dijawab server |

Akar masalah yang dicatat roadmap — `selectable={false}` — ternyata bukan cacatnya. Prop itu
memang **benar** padam pada papan berdiri sendiri: memilih tempat tidur hanya bermakna di
dalam alur admisi yang punya episode, dan menyalakannya di sini akan menghasilkan tombol
"Pilih" yang tidak menuju ke mana pun. Yang salah adalah **tidak ada aksi lain yang
menggantikannya**. Task ini karena itu tidak mengubah nilai `selectable`, melainkan menghapus
keadaan "layar tanpa aksi efektif" yang disebut roadmap.

---

## 2. Proses bisnis dari sisi pengguna

**Siapa yang membukanya.** Petugas admisi rawat inap, kepala ruangan, perawat ruangan, dan
supervisor — siapa pun yang memegang `InpatientBedOccupancy : Read`. Layar ini biasanya
dibiarkan terbuka sepanjang giliran kerja sebagai papan pantau, bukan dibuka sebentar lalu
ditutup.

**Alur normal — menindaklanjuti pemesanan yang sudah jatuh tempo kedatangannya.**

1. Petugas membuka **Rawat Inap → Papan Tempat Tidur**. Lima angka ringkasan tampil di atas:
   total, tersedia, terisi, dipesan, dan ditutup.
2. Petugas mempersempit papan dengan penyaring unit layanan, kamar, kelas, atau kata kunci.
3. Sebuah tempat tidur bertanda **Dipesan** menampilkan tiga hal sekaligus:
   `Untuk Budi Santoso — EP-2026-000123` dan lencana `Sisa 00:12:41` yang berdetak turun
   setiap detik.
4. Pasiennya datang. Petugas menekan **Konfirmasi Masuk**. Papan dibaca ulang lebih dulu, lalu
   dialog konfirmasi menyebut nama pasien dan tempat tidurnya. Setelah disetujui, pasien
   ditempatkan, episode menjadi `Admitted`, dan papan dibaca ulang.
5. Pasiennya batal datang. Petugas menekan **Batalkan Pesanan**. Dialog menyebut tempat tidur
   dan untuk siapa pemesanannya, lalu memperingatkan bahwa episode pemegangnya tetap ada dan
   harus memilih tempat tidur lagi. Setelah disetujui, tempat tidurnya dilepas dan dapat
   dipesan pasien lain.

**Jalur tidak normal.**

| Keadaan | Yang dilihat dan dapat dilakukan pengguna |
| --- | --- |
| Sisa waktu habis selagi layar terbuka | Lencana berubah menjadi **Batas waktu lewat**, dan papan dibaca ulang **satu kali** secara otomatis. Layar tidak pernah menyatakan sendiri bahwa pemesanan sudah gugur — ia menanyakannya kembali kepada server, karena server hanya mengirimkan pemesanan yang masih berlaku |
| Petugas lain merebut tempat tidurnya lebih dulu | Server menjawab `409`. Papan dibaca ulang otomatis, dialog ditutup, dan pesan "Keadaan tempat tidur sudah berubah" tampil |
| Aturan kelayakan menolak penempatan | Server menjawab `422` beserta **daftar** aturan yang gagal. Daftar itu ditampilkan apa adanya di dalam dialog, lengkap dengan nomor aturannya. Tidak ada kalimat buatan layar yang menggantikannya |
| Jaringan atau server gagal menjawab | Kalimat merah tampil, **kelima angka ringkasan dikosongkan menjadi nol**, dan tombol **Coba Lagi** muncul. Menekannya membaca ulang tanpa menyentuh penyaring — unit, kamar, kelas, dan kata kunci yang sudah dipilih tetap terpasang |
| Papan kosong padahal tidak ada penyaring | "Master tempat tidur belum tersedia", disertai tombol **Buka Master Tempat Tidur** menuju `/health-services/master-data/bed` |
| Papan kosong karena penyaring | "Tidak ada tempat tidur pada penyaring ini", tanpa tautan master data — karena master data tidak ada hubungannya dengan sebab kosongnya |
| Petugas tidak berhak membuka papan | Server menjawab `401`/`403`, dan seluruh isi halaman digantikan alert "Ups! Akses Ditolak" |
| Jendela ditinggal lalu dibuka lagi | Papan dibaca ulang otomatis. Ini yang membuat papan pantau tidak menampilkan keadaan satu jam lalu |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

**Governance dan blueprint**

- `AGENTS.md` frontend; `rules/GLOBAL_RULES.md`; `rules/frontend/frontend-architecture.md`,
  `base-component-catalog.md`, `base-component-decision-gate.md`, `design-tokens.md`,
  `page-composition-patterns.md`, `ui-consistency-checklist.md`, `test-policy.md`,
  `REPORT_TEMPLATE.md`
- `roadmap/frontend-roadmap.md`; `05-skema-tampilan.md` bagian 7; laporan `FE-RWI-026`,
  `FE-RWI-030`, `FE-RWI-034`

**Source backend — read-only**

- `Areas/HealthServices/InPatientManagement/Controllers/InpatientBedOccupancyController.cs`
- `Areas/HealthServices/InPatientManagement/DTOs/InpatientBedOccupancyDtos.cs`

**Source frontend**

- `inpatient-bed-board.jsx`, `inpatient-bed-board-view.jsx`, `inpatient-bed-board-constants.jsx`,
  `use-inpatient-bed-board.jsx`, `use-inpatient-bed-board-actions.jsx`, `inpatient-bed-utils.jsx`
- Pembanding: `use-inpatient-admission-bed.jsx` (hitung mundur `FE-RWI-026`),
  `inpatient-admission-bed-step.jsx` (dialog pembatalan), `inpatient-census-view.jsx`
  (pola **Coba Lagi**), `use-inpatient-census.jsx` (pola baca ulang saat fokus),
  `inpatient-dashboard-view.jsx` (pola **Muat Ulang** pada hero),
  `base-button.jsx`, `confirm-modal.jsx`, `toast-stack.jsx`, `access-denied-gate.jsx`
- Pemeriksaan negatif: tidak ada katalog permission di sisi peramban —
  `grep -rn "InpatientBedOccupancy" src/` hanya menemukan komentar, dan
  `filter-menu-items-by-role.jsx` seluruh aturan per-peran-nya sudah dinonaktifkan

### 3.2 Berkas yang berubah

Enam berkas source disunting, satu stylesheet dibuat, dua berkas e2e disesuaikan. **Nol** berkas dihapus.

| Berkas | Perubahan |
| --- | --- |
| `src/utils/health-services/inpatient-management/inpatient-bed-utils.jsx` | Tiga fungsi baru: `hasActiveBedBoardFilter` membedakan dua sebab papan kosong; `describeBedReservationHold` menyusun pemegang, sisa waktu, dan kelayakan aksi satu tempat tidur `Reserved`; `findEarliestReservationExpiry` mencari batas waktu pemesanan paling awal pada papan |
| `src/lib/constants/health-services/inpatient-management/inpatient-bed-board-constants.jsx` | `INPATIENT_BED_BOARD_EMPTY_STATES` berisi dua kalimat kosong yang berbeda, dan `INPATIENT_BED_BOARD_CANCEL_CONFIRM` berisi judul serta label dialog pembatalan |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-bed-board.jsx` | Opsi baru `refreshOnWindowFocus` (bawaan padam) untuk membaca ulang saat jendela kembali difokuskan; pada kegagalan baca, papan dan daftar tempat tidur yang boleh dipilih **dikosongkan** supaya ringkasan lama tidak tertinggal |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-bed-board-actions.jsx` | Jam pemesanan yang hanya berdetak selama ada pemesanan, beserta pembacaan ulang **satu kali** ketika batas waktu paling awal lewat; siklus penuh pembatalan pemesanan — `requestCancelReservation`, `dismissCancelReservation`, `submitCancelReservation` — lengkap dengan penjaga pengiriman ganda, penanganan `409`/`422`, dan toast |
| `src/components/features/health-services/inpatient-management/inpatient-bed-board.jsx` | Empat prop opsional baru — `onRetry`, `reservationNowMs`, `onCancelReservation`, `emptyState` — seluruhnya padam secara bawaan; baris tempat tidur `Reserved` kini menyebut pemegang dan sisa waktunya, serta menawarkan **Batalkan Pesanan** |
| `src/components/view/health-services/inpatient-management/inpatient-bed-board-view.jsx` | **Muat Ulang** pada hero; baca ulang saat fokus dinyalakan; **Coba Lagi** disambungkan; dua kalimat kosong beserta tautan **Buka Master Tempat Tidur**; dialog konfirmasi pembatalan pemesanan |
| `src/style/health-services/inpatient-management/inpatient-bed-board.module.css` | **Berkas baru.** Seluruh gaya papan — kartu unit layanan, kotak kamar, dan kartu tempat tidur — disusun dari token `globals.css`. Nol warna, radius, atau ukuran huruf literal, dan nol selector yang menyasar komponen shared |
| `tests/e2e/inpatient-bed-board.spec.mjs`, `tests/e2e/inpatient-admission-cancellation.spec.mjs` | Tiga pemeriksaan angka ringkasan diarahkan ke elemen nilai di dalam kartu `SummaryGrid`, karena `data-testid` kini menempel pada kartunya yang juga memuat label. Pemeriksaannya **tidak** dilonggarkan — tetap `toHaveText` pada angka yang persis, bukan `toContainText` |

### 3.3 Kepatuhan arsitektur frontend

- **Alur dependensi tidak dibalik.** `page.jsx` → `view` → `hook` → `service` → Axios instance
  existing. Utility murni tetap di `src/utils`, konstanta tetap di `src/lib/constants`.
- **Tidak ada Axios instance kedua.** Seluruh panggilan lewat `bedOccupancyService` yang sudah
  ada, memakai `patch` yang sudah dipakai `use-inpatient-admission-bed.jsx`.
- **Tidak ada arsitektur state baru.** Tidak ada slice Redux baru; papan memang sejak awal
  memakai state lokal hook, dan pola itu dipertahankan.
- **Komponen bersama tidak dipaksa berubah.** Keempat prop baru pada `InpatientBedBoard`
  padam secara bawaan, sehingga langkah **Pilih Bed** pada alur admisi merender persis
  seperti sebelumnya. Hal yang sama berlaku untuk opsi `refreshOnWindowFocus` pada
  `useInpatientBedBoard`.
- **Aturan kelayakan tetap milik server.** Tidak ada satu pun aturan penempatan yang dihitung
  ulang di peramban. Hitung mundur murni tampilan; yang menentukan sebuah pemesanan masih
  berlaku tetap jawaban server.
- **Route hanya entry point.** `bed-board/page.jsx` tidak disentuh.

### 3.4 Perbaikan tampilan papan

Pemilik pekerjaan memeriksa layarnya di peramban dan melaporkan papannya "seperti tidak ada
CSS". Laporan itu benar, dan penyebabnya dapat ditunjuk: **hero dan penyaring memakai base
component sehingga bergaya Quilvian, sedangkan area papan di bawahnya masih dirangkai dari
utility Bootstrap mentah.** Lint dan build tidak akan pernah menangkap ini — keduanya memang
lulus sejak awal.

| Bagian | Sebelum | Sesudah |
| --- | --- | --- |
| Ringkasan lima angka | Lima `<div className="border rounded px-3 py-2">` rakitan sendiri dengan `fs-5 fw-semibold` | `SummaryGrid`, kartu ringkasan baku aplikasi — sama dengan yang dipakai seluruh layar master data |
| Unit layanan | `<h3 className="h6">` dan `<p className="small">` tanpa wadah | Kartu ber-`--color-surface`, `--radius-lg`, dan `--shadow-sm`, dengan jumlah tersedia di sisi kanan judul |
| Kamar | `<div className="border rounded p-2 mb-2">` | Kotak ber-`--color-surface-soft` di dalam kartu unit; nama kamar sebagai judul dan kelas perawatan sebagai lencana pil |
| Tempat tidur | `<li>` memanjang ke bawah, satu baris per tempat tidur | **Kartu dalam grid responsif** (`repeat(auto-fill, minmax(248px, 1fr))`, satu kolom di bawah 768px) — sesuai kerangka skema tampilan bagian 7.1 yang memang menggambarkan tempat tidur berjajar, bukan bertumpuk |
| Keadaan tempat tidur | Hanya lencana teks | Lencana teks **ditambah** aksen warna di tepi kiri kartu: hijau tersedia, kuning dipesan, biru terisi, merah tidak dapat dipakai. Warnanya melengkapi, bukan menggantikan — pembaca yang tidak dapat membedakan warna tetap membaca keadaan yang sama dari lencananya |
| Tombol "Pilih" | `<button className="btn btn-primary btn-sm">` Bootstrap mentah | `BaseButton` `variant="primary"`/`"secondary"`, dengan `aria-pressed` dan keadaan nonaktif yang dipertahankan |
| Lencana "Isolasi" dan "Boks bayi" | `status="active"` — hijau, terbaca seolah keduanya berarti "tersedia" | `status="info"` — biru, sesuai artinya sebagai keterangan, bukan keadaan ketersediaan |
| Kamar tanpa tempat tidur | Kotak kosong tanpa satu kata pun | "Belum ada tempat tidur terdaftar pada kamar ini." |

Dua akibat yang perlu disebut apa adanya:

1. **Langkah Pilih Bed pada alur admisi ikut berubah tampilannya**, karena
   `InpatientBedBoard` memang dipakai bersama. Perubahannya searah — layar itu mendapat kartu
   dan token yang sama — dan tidak satu pun perilakunya berubah: `data-testid`,
   `data-selectable`, `data-reserved`, teks lencana, serta nama tombol "Pilih"/"Terpilih"
   dipertahankan persis.
2. **Tiga pemeriksaan e2e angka ringkasan disesuaikan** karena `SummaryGrid` menempatkan
   `data-testid` pada kartu, bukan pada angkanya. Pemeriksaannya tetap ketat.

### 3.5 Tabel keputusan base component

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Tombol **Muat Ulang** pada hero | `Hero` prop `actions` + `BaseButton` | `base-features/hero`, pola sama pada `inpatient-dashboard-view.jsx` baris 170 | `REUSE` | `variant="secondary"` dengan `loadingLabel="Memuat ulang..."` |
| Tombol **Coba Lagi** pada gagal baca | `BaseButton` | pola sama pada `inpatient-census-view.jsx` baris 188 | `REUSE` | `variant="secondary" size="sm"`, nonaktif selagi memuat |
| Tombol **Batalkan Pesanan** pada baris bed | `BaseButton` | `base-features/base-button` | `REUSE` | `variant="danger" size="sm"` — semantik destruktif |
| Dialog konfirmasi pembatalan | `ConfirmModal` | `base-features/confirm-modal`, `variant="danger"`, `children` untuk rincian | `REUSE` | Pola sama dengan dialog konfirmasi masuk `FE-RWI-030` |
| Lencana sisa waktu pemesanan | `StatusBadge` | `base-features/status-badge`, status `warning` dan `inactive` tersedia | `REUSE` | `warning` selagi berjalan, `inactive` ketika batas waktu lewat |
| Kalimat papan kosong | `InformationAlert` | `base-features/information-alert`, `variant="info"` | `REUSE` | Judul dan pesan datang dari konstanta, bukan ditanam di JSX |
| Tautan **Buka Master Tempat Tidur** | `BaseButton` `as={Link}` | `base-button.jsx` baris 21 mendukung prop `as` | `REUSE` | Menghindari `className="btn btn-outline-primary"` mentah yang masih dipakai layar census |
| Penolakan `409`/`422` pembatalan | `PlacementFailureList` | `inpatient-management/placement-failure-list.jsx`, kedua judulnya sudah dapat diganti pemanggil sejak `FE-RWI-026` | `REUSE` | Judul diganti menjadi konteks pemesanan, bukan penempatan |
| Lima kartu angka ringkasan | `SummaryGrid` | `base-features/summary-grid`, sudah menerima `items`, `loading`, dan `testId` per kartu | `REUSE` | Menggantikan lima kotak rakitan sendiri; skeleton saat memuat ikut didapat gratis |
| Lencana keterangan isolasi dan boks bayi | `StatusBadge` | `base-features/status-badge` mendukung status `info` | `REUSE` | `status="info"`, bukan `active` — keduanya keterangan, bukan keadaan ketersediaan |
| Tombol Pilih pada alur admisi | `BaseButton` | `base-features/base-button` | `REUSE` | Menggantikan `<button className="btn btn-primary btn-sm">` Bootstrap mentah yang tersisa dari kode lama |
| Toast hasil pembatalan | `ToastStack` | `base-features/toast-stack`, sudah terpasang di layar ini | `REUSE` | Memakai `addToast` yang sudah ada |

**`UI GATE: 12 elemen — REUSE 12, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0.`**

Tidak ada elemen berstatus `NEW` maupun `EXTEND`, sehingga tidak ada keputusan yang perlu
ditunggu dari pengguna. Stylesheet `inpatient-bed-board.module.css` **bukan** komponen baru: ia
wadah gaya milik fitur, mengikuti `inpatient-dashboard.module.css` yang sudah ada di modul yang
sama, dan seluruh nilainya token. Empat prop baru pada `InpatientBedBoard` **bukan** `EXTEND` terhadap
base component: `InpatientBedBoard` adalah komponen fitur milik modul Rawat Inap di
`components/features/health-services/`, bukan penghuni `base-features/`.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Kalimat "Mengambil papan ketersediaan..."; tombol **Muat Ulang** berubah menjadi "Memuat ulang..."; tombol reset penyaring dan **Coba Lagi** dinonaktifkan |
| Kosong — master belum ada | "Master tempat tidur belum tersedia" beserta penjelasan bahwa admisi tidak dapat memesan maupun menempatkan pasien selama master kosong, disertai tombol **Buka Master Tempat Tidur** |
| Kosong — penyaring tidak cocok | "Tidak ada tempat tidur pada penyaring ini" beserta anjuran mengubah atau mengosongkan penyaringnya. Tanpa tautan master data |
| Gagal | Kalimat kesalahan dari server dalam alert merah, kelima angka ringkasan dikosongkan menjadi nol, dan tombol **Coba Lagi** yang membaca ulang tanpa kehilangan penyaring |
| Gagal aksi — `409` | "Keadaan tempat tidur sudah berubah" atau "Keadaan pemesanan sudah berubah"; papan dibaca ulang otomatis dan dialognya ditutup |
| Gagal aksi — `422` | Daftar aturan kelayakan yang gagal ditampilkan apa adanya di dalam dialog, lengkap dengan nomor aturan dan arah isolasinya |
| Tanpa hak akses | Seluruh isi halaman digantikan alert "Ups! Akses Ditolak" beserta anjuran menghubungi IT Helpdesk |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Inpatient Management / Bed Occupancy

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/inpatient-management/bed-occupancies/bed-board` | Menyusun ringkasan dan seluruh kartu tempat tidur, termasuk metadata pemesanan `HoldingEpisodeId`, `HoldingEpisodeNumber`, `PatientName`, `ReservationId`, dan `ReservationExpiresAt` | `InpatientBedOccupancy : Read` |
| `GET` | `/api/v1/health-services/inpatient-management/bed-occupancies/available-beds` | Menandai tempat tidur mana yang lolos kelayakan menurut server. Layar tidak menyaring ulang | `InpatientBedOccupancy : Read` |
| `POST` | `/api/v1/health-services/inpatient-management/bed-occupancies/placements` | **Konfirmasi Masuk** — menempatkan pasien dan mengaktifkan episodenya | `InpatientBedOccupancy : Create` |
| `PATCH` | `/api/v1/health-services/inpatient-management/bed-occupancies/reservations/{id}/cancel` | **Batalkan Pesanan** — melepas pemesanan sebelum dipakai | `InpatientBedOccupancy : Update` |

Catatan kontrak: kedua aksi memakai **permission yang berbeda** — `Create` untuk penempatan
dan `Update` untuk pembatalan. Keduanya sengaja tidak digabung menjadi satu aksi di layar.
Badan permintaan pembatalan (`CancelReservationRequest.Reason`, maksimum 500 karakter)
bersifat opsional pada kontrak backend dan skema tampilan bagian 7.3 tidak memintanya,
sehingga layar mengirim `reason: null` — sama dengan pembatalan pada langkah Booking Bed.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint` | `0 errors`, 571 warning | `PASS` | Angka warning **sama persis** dengan garis dasar `FE-RWI-031` s.d. `034`. Penyaringan keluaran atas nama keenam berkas task ini menghasilkan **nol** baris |
| `npm run build` | `✓ Compiled successfully in 31.5s`; `postbuild` `prepare-standalone.mjs` berhasil | `PASS` | Route `/health-services/inpatient-management/bed-board` ikut terbangun sebagai halaman statis |
| Grep anti-regresi warna literal | Kosong | `PASS` | `#hex`, `rgba(`, dan `hsla(` — nol hit, termasuk pada stylesheet baru yang seluruhnya memakai `var(--...)` |
| Grep anti-regresi typography | Kosong | `PASS` | `font-size`, `font-weight`, `line-height` — nol hit |
| Grep anti-regresi tombol mentah | Kosong | `PASS` | Nol hit. Dua hit warisan yang sebelumnya tersisa — tombol "Pilih" Bootstrap — kini ikut hilang karena diganti `BaseButton` |
| Grep anti-regresi tabel | Kosong | `PASS` | Tidak ada `<table>` mentah |
| Grep anti-regresi `fw-*` / `fs-*` | Kosong | `PASS` | Nol hit. Kedua hit warisan hilang bersama kotak ringkasan rakitan sendiri dan nama tempat tidur yang kini memakai class module |
| Grep anti-regresi `!important`, dark mode, inline style | Kosong | `PASS` | Nol hit ketiganya |
| `npm run test:unit` | Tidak dijalankan | `NOT RUN` | Arahan pengguna 1 September 2026 membatasi validasi pada `lint` dan `build` |
| `npm run test:e2e` | Tidak dijalankan | `NOT RUN` | Arahan pengguna yang sama; environment juga belum punya data runtime — `RWI-UI-GAP-007` |
| Penelusuran manual keadaan Available/Reserved/Occupied/Unavailable | Tidak dijalankan | `NOT FEASIBLE` | `RWI-UI-GAP-007` masih terbuka: environment target tidak punya master tempat tidur maupun episode dengan pemesanan aktif, sehingga tidak ada satu pun tempat tidur `Reserved` yang dapat dibuka di peramban |

Uji manual: `NOT FEASIBLE`.

**Tidak dijalankan:** `npm run test:unit`, `npm run test:e2e`, dan `npm run test:uat` —
seluruhnya atas arahan pengguna, bukan karena gagal atau terhalang. Ketiga fungsi murni yang
ditambahkan pada `inpatient-bed-utils.jsx` termasuk kategori yang **layak** diberi test unit
menurut `rules/frontend/test-policy.md`; penulisannya ditunda, bukan dinyatakan tidak perlu.

`AUTOMATED TEST: SKIPPED (opsional) — arahan pengguna 1 September 2026 membatasi validasi pada npm run lint dan npm run build.`

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Ringkasan dan kartu bed mengikuti keadaan server | ✅ Terpenuhi | Lima kartu ringkasan dibaca dari `BED_BOARD_SUMMARY_FIELDS` langsung atas jawaban `bed-board`. Yang **diperbaiki** task ini adalah perilaku ketika pembacaan gagal: `use-inpatient-bed-board.jsx` kini mengosongkan `board` dan `availableBeds` di blok `catch`, sehingga angka pembacaan sebelumnya tidak tertinggal berdampingan dengan pesan gagal — skema tampilan bagian 7.2 "gagal tidak menyisakan ringkasan lama" |
| 2. Bed `Reserved` menampilkan pemegang, sisa waktu, dan aksi yang diizinkan | ✅ Terpenuhi | `describeBedReservationHold` menyusun ketiganya dari kolom yang memang dikirim server. Baris menampilkan `Untuk <nama pasien> — <nomor episode>`, lencana `Sisa hh:mm:ss` yang berdetak turun setiap detik, lalu **Konfirmasi Masuk** dan **Batalkan Pesanan**. Nomor episode dipakai sebagai cadangan ketika nama pasien tidak dikirim, dan kalimat lama `describeBedUnavailability` dipakai ketika keduanya kosong |
| 3. **Konfirmasi Masuk** serta **Batalkan Pesanan** tidak muncul bagi peran tanpa hak | 🟡 Terpenuhi secara struktural | Repository frontend **tidak memiliki katalog permission di sisi peramban** — `grep -rn "InpatientBedOccupancy" src/` hanya menemukan komentar, dan `filter-menu-items-by-role.jsx` seluruh aturan per-peran-nya sudah dinonaktifkan. Yang berlaku adalah tiga lapis penjagaan: (a) papan itu sendiri dibungkus `AccessDeniedGate`, sehingga peran tanpa `InpatientBedOccupancy : Read` tidak melihat layarnya sama sekali; (b) kedua tombol hanya dirender ketika server mengirim metadata yang dibutuhkannya — `holdingEpisodeId` untuk konfirmasi, `reservationId` untuk pembatalan; (c) server menolak `403` lewat `[AccessPermission]` bila tetap dipanggil. Batas yang jujur dicatat pada bagian 8: peran yang punya `Read` tetapi tidak punya `Create`/`Update` **masih melihat tombolnya** dan baru mengetahui penolakannya setelah menekan. Menutup celah ini menuntut kontrak baca permission pengguna yang belum ada, bukan perubahan rancangan layar |
| 4. Empty state membedakan "master bed belum tersedia" dari "tidak cocok dengan filter" dan memberi jalan ke Master Data bagi admin yang berhak | 🟡 Terpenuhi, satu batas | Pembedaannya berjalan: `hasActiveBedBoardFilter` memeriksa unit layanan, kamar, kelas, dan kata kunci — `pageNumber` dan `pageSize` sengaja tidak dihitung karena mengatur potongan hasil, bukan mempersempit pencarian. Tanpa penyaring apa pun, papan kosong berarti masternya memang belum ada, dan tombol **Buka Master Tempat Tidur** menuju `/health-services/master-data/bed` ditampilkan. Batasnya sama dengan kriteria 3: layar tidak dapat mengetahui siapa "admin yang berhak", sehingga tautannya tampil bagi setiap pembaca papan. Layar master data itu sendiri menolak yang tidak berhak |
| 5. Gagal baca menyediakan **Coba Lagi** tanpa kehilangan filter | ✅ Terpenuhi | Prop `onRetry` memasang tombol **Coba Lagi** tepat di bawah pesan gagal. Tombolnya memanggil `refresh()`, yang hanya menaikkan `reloadToken` dan **tidak menyentuh state `filters`** sama sekali — unit layanan, kamar, kelas, dan kata kunci yang sudah dipilih tetap terpasang sesudahnya. Tombol dinonaktifkan selagi pembacaan berjalan |
| 6. Tidak ada aturan kelayakan yang dihitung ulang di browser | ✅ Terpenuhi | Tidak ada satu pun aturan penempatan yang ditulis di layar. Penanda "boleh dipilih" tetap berasal dari daftar `available-beds`; alasan tidak dapat dipakai tetap dibaca dari kolom yang dikirim server, dan ketika tidak ada kolom yang menjelaskannya layar mengaku tidak tahu alih-alih menebak. Hitung mundur murni tampilan — ketika habis, layar **membaca ulang papan** dan memakai jawaban server, bukan menyatakan sendiri bahwa pemesanannya gugur |

**Definition of Done** — "keenam kriteria lulus; laporan membuktikan layar tidak lagi pasif
dan tidak memakai mock tersembunyi":

| Butir DoD | Status |
| --- | --- |
| Keenam kriteria lulus | 🟡 Empat terpenuhi penuh, dua terpenuhi dengan batas yang sama dan tercatat: layar tidak dapat mengetahui hak akses pengguna karena kontrak bacanya belum ada |
| Layar tidak lagi pasif | ✅ Enam aksi efektif kini ada di layar: **Muat Ulang**, **Coba Lagi**, **Konfirmasi Masuk**, **Batalkan Pesanan**, **Buka Master Tempat Tidur**, dan pembacaan ulang otomatis saat jendela kembali difokuskan |
| Tidak memakai mock tersembunyi | ✅ Nol data tiruan. Seluruh kolom yang ditampilkan berasal dari `BedBoardBedResponse` yang dapat ditunjuk baris per baris pada `InpatientBedOccupancyDtos.cs`. Tidak ada nilai bawaan yang mengarang keadaan tempat tidur, dan tidak ada endpoint kiosk maupun state peramban yang dipakai menyamarkan gap |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `npm run lint` menghasilkan 571 warning, **sama persis** dengan garis dasar sebelum task ini. Nol di antaranya berasal dari keenam berkas task ini. Seluruhnya `EXISTING WARNING` |
| Masalah yang diketahui — hak akses tidak terbaca layar | Frontend tidak punya cara mengetahui permission pengguna. Akibatnya tombol **Konfirmasi Masuk**, **Batalkan Pesanan**, dan tautan **Buka Master Tempat Tidur** tampil bagi setiap pembaca papan; penolakannya baru terlihat setelah tombol ditekan. Menutupnya menuntut satu kontrak baca yang menyebutkan permission pengguna saat ini — perubahan backend, bukan perubahan layar |
| Masalah yang diketahui — `UNRELATED EXISTING ISSUE` | `createToast(type, title, message)` pada `inpatient-setting-utils.jsx` menghasilkan objek berkolom `type`, sedangkan `ToastStack` membaca kolom `variant`. Akibatnya seluruh toast modul Rawat Inap — termasuk toast berhasil milik `FE-RWI-030` dan toast pembatalan yang ditambahkan task ini — tampil dengan warna `info`, bukan warna semantiknya. **Sengaja tidak diperbaiki**: `createToast` dipakai banyak layar Rawat Inap lain, dan mengubahnya akan mengubah tampilan layar yang bukan cakupan task ini |
| Dampak yang disengaja — langkah Pilih Bed ikut berubah tampilannya | `InpatientBedBoard` dipakai bersama layar papan dan langkah **Pilih Bed** pada alur admisi, sehingga perbaikan tampilan mengenai keduanya. Perubahannya searah dan tidak satu pun perilaku berubah; seluruh `data-testid`, atribut `data-selectable`/`data-reserved`, teks lencana, dan nama tombol dipertahankan persis. Disebut di sini supaya tidak terbaca sebagai regresi tak sengaja |
| Dependency backend | `BE-RWI-036` ✅ selesai — seluruh metadata pemesanan yang dibutuhkan layar sudah tersedia. `RWI-UI-GAP-007` **masih terbuka**: environment target belum punya master tempat tidur maupun episode dengan pemesanan aktif, sehingga tidak ada satu pun keadaan `Reserved` yang dapat dibuktikan di peramban. Pembuktian runtime keenam kriteria menunggu gap itu, dan `FE-RWI-035` yang memilikinya |
| Perubahan sampingan | `NONE`. Enam berkas disunting, seluruhnya milik layar papan tempat tidur |
| Interupsi | `NONE` |
| Status Git | Lihat blok di bawah |
| Langkah berikutnya | `FE-RWI-039` (repair Selisih Bed) sudah terbuka karena dependency-nya `FE-RWI-036` kini selesai. Di luar itu, pengisian data master rawat inap pada environment target adalah satu-satunya hal yang menahan pembuktian runtime task ini — pemiliknya Admin Master Data, bukan Frontend |

### `git status --short`

```
 M src/components/features/health-services/inpatient-management/inpatient-bed-board.jsx
 M src/components/view/health-services/inpatient-management/inpatient-bed-board-view.jsx
 M src/lib/constants/health-services/inpatient-management/inpatient-bed-board-constants.jsx
 M src/lib/hooks/health-services/inpatient-management/use-inpatient-bed-board-actions.jsx
 M src/lib/hooks/health-services/inpatient-management/use-inpatient-bed-board.jsx
 M src/utils/health-services/inpatient-management/inpatient-bed-utils.jsx
 M tests/e2e/inpatient-admission-cancellation.spec.mjs
 M tests/e2e/inpatient-bed-board.spec.mjs
?? src/style/health-services/inpatient-management/inpatient-bed-board.module.css
```

> Berkas berubah lain pada working tree — `inpatient-census-view.jsx`,
> `inpatient-admission-flow-constants.jsx`, `inpatient-dashboard-constants.jsx`,
> `use-inpatient-admission-doctor.jsx`, `use-inpatient-census.jsx`,
> `use-inpatient-episode-detail.jsx`, `inpatient-census-utils.jsx`,
> `inpatient-episode-utils.jsx`, `menu-items.jsx`, beserta berkas test dan tiga berkas yang
> dihapus — **bukan** dari task ini. Seluruhnya peninggalan `FE-RWI-033` dan `FE-RWI-034`
> yang belum di-commit, dan tidak disentuh sama sekali.
