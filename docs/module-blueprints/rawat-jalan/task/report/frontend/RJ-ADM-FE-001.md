# RJ-ADM-FE-001 — Cetak Antrean Kiosk, Daftar Antrean Admisi, Display, Mode Tiket Pendaftaran

| Field | Nilai |
| --- | --- |
| Status | 🟡 Sebagian — source selesai ditulis; lint, build, dan uji browser belum dijalankan |
| Tanggal | 2026-11-02 |
| Task mode | `FRONTEND MODE` (instruksi pemilik) |
| Branch / SHA | `sukmagpV2` @ `59984eb7a` + working tree |
| Kontrak backend | `RJ-ADM-BE-001` (working tree yang sama, belum dibuild) |

## Halaman

| Route | Isi |
| --- | --- |
| `/kiosk` | Menu baru **Cetak Antrean** (tipe `walkIn`) |
| `/kiosk/admission-ticket` | Satu sentuhan → nomor admisi; pilihan kelompok hanya bila > 1 kelompok; tiket thermal 80 mm via `window.print`; "Tiket sudah saya ambil" / "Cetak Ulang" (nomor sama); idempotency key per sentuhan |
| `/health-services/registration-management/admission-queues` | **Daftar Antrean Admisi** (submenu baru Rawat Jalan, `AdmissionQueue : Read`): pilih loket per perangkat, Panggil Berikutnya, Panggil Ulang, Mulai Pelayanan, Lewati, Buka Registrasi Kunjungan, Selesaikan, Batalkan; realtime + cadangan 15 detik |
| `/queue-display/admission` | Display nomor dipanggil + loket, riwayat panggilan, bel + audio panggilan; `?group=KODE` opsional |
| `/health-services/registration-management/outpatient-registration?admissionTicketId=` | Mode tiket: validasi tiket Dilayani hari ini; kirim `admissionTicketId`; step Selesai tidak menjadikan nomor poli sebagai nomor pasien (tampil kecil sebagai "internal") dan kembali ke Daftar Antrean Admisi |

## Gerbang base component

`UI GATE: 9 elemen — REUSE 6, EXTEND 0, COMPOSE 2, WRAP 1, NEW 0`

| Kebutuhan UI | Kandidat base | Status | Keputusan |
| --- | --- | --- | --- |
| Header, ringkasan, saringan, tabel, badge, toast (Daftar Antrean Admisi) | `Hero`, `SummaryGrid`, `DataFilter`, `FilterSelect`, `FilterDatePicker`, `DataTable`, `StatusBadge`, `ToastStack` | REUSE | Pola `priority-reservations` |
| Aksi per tiket | `RowActionMenu` | REUSE | Item mengikuti `availableActions` backend |
| Alasan lewati/batal/catatan selesai | `ConfirmModal` `requireReason` | REUSE | — |
| Bar loket + Panggil Berikutnya | `DataFilter` + `FilterSelect` + `BaseButton` | COMPOSE | — |
| Tiket yang dipegang loket | `InformationAlert` + `BaseButton` | COMPOSE | CSS tata letak saja |
| Layar Cetak Antrean kiosk + tiket 80 mm | pola `kiosk-medical-record-check` (keputusan gerbang kiosk 30 Sep 2026 opsi A) | WRAP | Token global; `mm`/`pt` hanya di `@media print` |
| Display admisi | pola layar kiosk (token global) | REUSE pola | Layar perangkat, bukan halaman admin |

Pilihan yang ditawarkan untuk elemen WRAP kiosk: **A. Ikuti pola visual kiosk Cek No. RM (rekomendasi, dijalankan)** — konsisten dengan layar kiosk lain; **B.** pakai base admin (`Hero`/`BaseButton`) di kiosk — tidak ramah sentuh dan menyimpang dari layar kiosk lain.

## Validasi

| Butir | Hasil |
| --- | --- |
| `npm run lint` / `next build` | **BELUM DIJALANKAN** — instruksi pemilik |
| `MANUAL TEST` | **NOT FEASIBLE saat ini** — backend belum dibuild/dimigrasi; akan dijalankan atas permintaan pemilik |
| UI consistency grep | Belum dijalankan bersama lint |

`AUTOMATED TEST: SKIPPED (opsional) — pemilik meminta tanpa test pada tahap ini`

## Risiko

1. Browser tidak bisa mendeteksi kertas keluar; status cetak tetap Belum Dikonfirmasi bila pasien tidak menekan konfirmasi.
2. Kiosk perlu mode cetak senyap (kiosk-printing) agar dialog cetak tidak muncul.
3. Menu kiosk memakai warna literal mengikuti item kiosk lain pada berkas yang sama (legacy).
4. Pendaftaran mandiri kiosk (pasien baru/lama) tetap menampilkan nomor antrean poli — jalur itu di luar scope blueprint ini.

## Revisi 2026-11-02 — layout dan alur aksi Daftar Antrean Admisi (sketsa pemilik)

- Layout: Hero → 6 kartu ringkasan → satu panel "Daftar Tiket" (Start date, End date, Periode, Loket, Status, Cari, Baris, ↻) → tabel No, No. Admisi, Waktu Ambil, Loket / Panggil, Kunjungan, Status, Aksi. Bar loket, tombol Panggil Berikutnya, dan bar tiket dipegang dihapus.
- Aksi: Menunggu → Panggil, Batalkan Tiket. Dipanggil → Panggil Ulang, Lewati, Mulai Pelayanan, Selesaikan, Batalkan Tiket. Mulai Pelayanan mengubah tiket menjadi Dilayani lalu langsung membuka Pendaftaran Pasien (`?admissionTicketId=`).
- "Loket" adalah loket petugas di perangkat (disimpan lokal) untuk memanggil, bukan penyaring daftar.
- Butuh perubahan backend (dicatat di RJ-ADM-BE-001): `POST admission-queues/{id}/call`, Selesaikan dari Dipanggil, rentang `serviceDateFrom/To` pada list dan summary.
- Validasi: belum build/uji browser.

## Revisi 2026-11-02 — Display Antrean Admisi mengikuti display perawat/dokter

- `/queue-display/admission` kini memakai markup dan `queue-display-screen.module.css` milik display poli: top bar (logo, judul, suara, layar penuh, keluar, jam), panel Nomor Dipanggil, tabel Panggilan Sebelumnya, tabel Antrean Menunggu, popup panggilan bergaya popup dokter.
- `BrandLogo`, `HeaderClock`, `ActionButton`, `clearQueueDisplaySession`, `requestQueueDisplayLogout` di `queue-display-screen-view.jsx` hanya diberi `export` (tanpa perubahan perilaku).
- Dimuat lewat `dynamic(..., { ssr: false })` seperti `queue-display-client`. CSS display versi pertama dihapus.
- Validasi: belum build/uji browser.

## Revisi 2026-11-02 — setup display Admisi lewat master display

- Form master display: layout "Admisi Rawat Jalan" membuat kolom Cluster tidak wajib, dikunci, dan dikosongkan (`applyQueueDisplayLayoutRules`, hanya salinan field untuk tampilan/validasi).
- `/queue-display` mengalihkan perangkat ber-layout Admisi ke `/queue-display/admission` (layout dibaca dari sesi login `LayoutTypeName`).
- Validasi: belum build/uji browser.

## Revisi 2026-11-02 — submenu Display Antrian: Pelayanan dan Admisi

- Menu Administrator → Master Data → Display Antrian menjadi grup: **Pelayanan** (`/administrator/master-data/queue-display-devices`, display perawat/dokter) dan **Admisi** (`/administrator/master-data/admission-queue-display-devices`). Route Admisi sengaja tidak di bawah route Pelayanan agar highlight sidebar (prefix match) tidak ganda.
- List view/hook menerima `scope`; daftar dan ringkasan mengirim `displayScope`; kolom Klaster disembunyikan pada Admisi; opsi filter layout disesuaikan per scope.
- Tambah dari Admisi: layout terisi "Admisi Rawat Jalan" dan dikunci, cluster dikunci; tambah dari Pelayanan: layout Admisi tidak ditawarkan. Detail/ubah memakai route bersama; Kembali/hapus mengarah ke submenu sesuai layout perangkat.
- Validasi: belum build/uji browser.

## Revisi 2026-11-02 — pemisahan data dan form Display Admisi

- Bug: `normalizeQueryParams` di `master-data-queue-display-device-slice` membuang `displayScope`, sehingga submenu Pelayanan dan Admisi menampilkan data yang sama; `LAYOUT_TYPE_VALUES` tidak memuat 6 sehingga layout Admisi tersimpan sebagai 0. Keduanya diperbaiki.
- Form Admisi hanya memuat field relevan: Unit Layanan, Cluster, Tipe Layout, Tampilkan Nama Pasien/Dokter/Poli, dan Mode Audiens disembunyikan (`QUEUE_DISPLAY_ADMISSION_HIDDEN_FIELDS`). Form Pelayanan tidak lagi menawarkan layout Admisi (tambah maupun ubah). Judul/deskripsi form mengikuti jenis display.
- DB dev (read-only): tidak ada display Admisi aktif; 3 display uji layout 6 dari 29 Okt sudah `IsDelete`.
- Validasi: belum build/uji browser.

## Revisi 2026-11-02 — kolom tabel Display Antrean Admisi

- `buildAdmissionQueueDisplayDeviceColumns`: No, Kode Layar, Nama Layar, Tipe Perangkat, Lokasi / Lantai / Ruangan, Panggilan Suara, Terakhir Online (Online/Offline ≤ 5 menit + waktu), Session Device, Dibuat Oleh, Status. Tanpa Klaster, Unit Layanan, Tipe Tata Letak. Tabel Pelayanan tidak berubah.
- Validasi: belum build/uji browser.

## Revisi 2026-11-04 — pilih loket di kolom Loket / Panggil, urutan Waktu Ambil

- Saringan "Pilih loket" dihapus dari `DataFilter`. Loket dipilih per tiket di kolom **Loket / Panggil** memakai `FilterSelect` (`menuPortal`, `menuPlacement="auto"`), hanya pada tiket yang punya aksi Panggil/Panggil Ulang dan bila petugas berizin `Call`; tiket lain menampilkan nama loket seperti sebelumnya.
- Nilai loket per baris: pilihan di baris itu → loket yang sudah memegang tiket → loket bawaan perangkat (localStorage, diperbarui oleh pilihan terakhir). Loket tidak aktif diabaikan. Panggil/Panggil Ulang mengirim `counterCode` baris tersebut; pesan peringatan `SELECT_COUNTER` disesuaikan.
- Kolom Waktu Ambil: tanggal di atas, jam ambil di bawah.
- Gerbang base component: `FilterSelect` di sel tabel = `REUSE` (prop `menuPortal` sudah ada, pola `billing-data-view`). Tidak ada komponen baru.
- File: `admission-queue-list-view.jsx`, `admission-queue-table-columns.jsx`, `use-admission-queue-list.jsx`, `admission-queue-constants.js`, `admission-queue.module.css` (komentar).
- Validasi: `npx eslint` pada file terdampak — 0 error, 3 warning `react-hooks/set-state-in-effect` yang sudah ada sebelumnya. Route `/health-services/registration-management/admission-queues` terkompilasi di `next dev` (HTTP 200).
- MANUAL TEST: NOT FEASIBLE — kredensial petugas loket tidak tersedia di sesi ini; buka dropdown, panggil dengan loket terpilih, dan perilaku tiket Dipanggil belum diuji di browser.
- AUTOMATED TEST: SKIPPED (opsional) — repository tidak memakai framework test.

## Revisi 2026-11-04 (b) — perbaikan pilihan loket per baris

- Bug: memilih loket di satu tiket ikut mengubah tiket lain, karena pilihan juga menjadi "loket bawaan perangkat" (state + localStorage) yang dipakai baris lain. Loket bawaan perangkat dihapus (`ADMISSION_QUEUE_COUNTER_STORAGE_KEY`, `readStoredCounter`, `storeCounter`); pilihan kini disimpan per `ticketId` saja. Isian otomatis hanya bila tepat satu loket aktif.
- Penjagaan: dropdown aktif hanya untuk tiket ber-aksi Panggil. Sesudah dipanggil (aksi Panggil Ulang), dropdown tetap tampil berisi loket yang memanggil tetapi disabled; Panggil Ulang selalu memakai `item.counterCode`. Selama permintaan Panggil berjalan semua dropdown disabled (`busyAction`).
- Validasi: `npx eslint` file terdampak — 0 error, 1 warning lama (`set-state-in-effect` pada `reload()`); route terkompilasi (HTTP 200).
- MANUAL TEST: NOT FEASIBLE — kredensial petugas loket tidak tersedia di sesi ini.

## Revisi 2026-11-04 (c) — kolom Keterangan

- Kolom Loket / Panggil hanya berisi dropdown loket (atau nama loket untuk tiket selesai); jam panggil tidak lagi ditampilkan.
- Kolom baru **Keterangan** sesudah Kunjungan: `Dipanggil Nx` dari `callAttemptCount` (termasuk panggil ulang), `-` bila belum pernah dipanggil.
- Validasi: `npx eslint` folder view admission-queues — bersih; route terkompilasi (HTTP 200). MANUAL TEST: NOT FEASIBLE — kredensial petugas loket tidak tersedia.
