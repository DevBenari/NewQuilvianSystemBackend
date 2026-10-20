# Laporan Task Frontend — `FE-RJP-004` Daftar Reservasi Prioritas

Definisi task: [priority-reservation-frontend-roadmap.md](../../../roadmap/priority-reservation-frontend-roadmap.md) bagian `FE-RJP-004`.

## 1. Ringkasan

| Field | Isi |
| --- | --- |
| Task ID | `FE-RJP-004` |
| Slice | `MVP-1`, `EPIC RP-04` |
| Trace | `FR-RP-030`, `062`; `RJ-PRI-DEC-008`, `011`, `021`; `RJ-PRI-FE-002`, `003`; `UAT-RP-07`, `08`, `09` |
| Kontrak | `RJ-PRI-RESV-001@1.0.0`; rework 19 Okt 2026: `@1.1.0` (`RJ-PRI-DEC-032`) — `GET /`, `GET /summary`, `GET /eligibility`, `POST /` pada `outpatient-priority-reservations` |
| Dependency | `FE-RJP-003` ✅ |
| Task mode | `FRONTEND` (`CROSS-REPO MODE`, `RJ-PRI-DEC-031`) |
| Branch / baseline | FE `sukmagpV2` @ `17e0ffec0`, working tree belum di-commit |
| Tanggal | 19 Oktober 2026 |
| Model | Claude Opus 5 |
| Status | ✅ Selesai — uji layar 15/15 `PASS`; rework 19 Okt 2026 (`RJ-PRI-DEC-032`) uji layar 12/12 + regresi 15/15 `PASS`, lint dan build `PASS` (bagian 8) |

## 2. Proses bisnis

Pasien member menghubungi rumah sakit lewat WhatsApp atau telepon. Petugas admisi membuka
**Rawat Jalan → Reservasi Prioritas**, menekan **+ Buat Reservasi**, mencari pasien, dan memilih tanggal kunjungan.

Layar langsung memeriksa kelayakan ke backend:

| Keadaan pasien | Yang tampil | Tombol Simpan |
| --- | --- | --- |
| Member Aktif tanpa reservasi terbuka | "Pasien dapat dibuatkan reservasi prioritas." | Aktif |
| Reguler atau member tidak aktif | Pesan "bukan member aktif" | Nonaktif |
| Sudah punya reservasi terbuka pada tanggal itu | Pesan berisi nomor reservasi yang sudah ada, mis. `RSV-20261019-0006` | Nonaktif |

Sesudah disimpan, dialog **Reservasi Prioritas Dibuat** menampilkan nomor reservasi, link konfirmasi, dan teks pesan
WA. Petugas menekan **Kirim via WhatsApp** (membuka `wa.me/62…?text=…` ke No. HP pasien) atau **Salin Teks** /
**Salin Link**. Link hanya tampil sekali. Begitu dialog ditutup, link hilang dari layar dan dari state. Bila link
hilang, petugas memakai **Kirim Ulang Link** di detail reservasi (`FE-RJP-005`).

Daftar memuat saringan tanggal (bawaan hari ini), status, kata kunci (No. RM, nama, atau no. reservasi, minimal
3 karakter), jumlah baris, serta enam kartu ringkasan status. Klik dua kali baris membuka detail.

## 3. Perubahan

| Berkas (repo frontend) | Status | Isi |
| --- | --- | --- |
| `src/app/health-services/registration-management/priority-reservations/page.jsx` | Baru | Entry route dan metadata |
| `src/components/view/health-services/registration-management/priority-reservations/priority-reservation-list-client.jsx` | Baru | Pembungkus klien |
| `src/components/view/health-services/registration-management/priority-reservations/priority-reservation-list-view.jsx` | Baru | Hero, ringkasan, saringan, tabel, paginasi, toast, `AccessDeniedGate` |
| `src/components/view/health-services/registration-management/priority-reservations/priority-reservation-table-columns.jsx` | Baru | Kolom No, No. Reservasi, Pasien (nama + No. RM), Tingkat, Tiba, Skrining, Aksi (Lihat detail, Lengkapi pendaftaran), Status |
| `src/components/view/health-services/registration-management/priority-reservations/priority-reservation-create-modal.jsx` | Baru | Dialog buat reservasi dan dialog hasil terbit |
| `src/components/view/health-services/registration-management/priority-reservations/priority-reservation-issued-panel.jsx` | Baru | Link, teks pesan, tujuan WA (No. HP disamarkan), tombol WA dan salin |
| `src/lib/hooks/health-services/registration-management/priority-reservations/use-priority-reservation-list.jsx` | Baru | Muat daftar + ringkasan, saringan, realtime board, bunyi saat pasien tiba, cadangan muat ulang 60 detik |
| `src/lib/hooks/health-services/registration-management/priority-reservations/use-priority-reservation-create.jsx` | Baru | Pencarian pasien (debounce), kelayakan, simpan |
| `src/style/health-services/registration-management/priority-reservations/priority-reservation.module.css` | Baru | Tata letak isi dialog saja; seluruh nilai memakai token |
| `src/utils/menu-sidebar/menu-items.jsx` | Diperbarui | Butir "Reservasi Prioritas" sesudah "Pendaftaran Pasien" di Rawat Jalan, ikon `RiCalendarCheckLine`, `requiredPermission` `OutpatientPriorityReservation : Read` |

Perbaikan pada akhir sesi: sel Pasien dan Tingkat semula belum memakai kelas sel tabel yang sama dengan modul lain.
Keduanya kini memakai `regionNameCell` + `primaryCell`, lalu build dan uji layar dijalankan ulang.

## 4. Gerbang komponen dasar

Modul referensi visual: halaman list master data generasi terbaru (pola Hero, SummaryGrid, DataFilter, DataTable,
ToastStack).

| Elemen | Keputusan | Bukti |
| --- | --- | --- |
| Kepala halaman + tombol "+ Buat Reservasi" | `REUSE` `Hero`, `BaseButton` | `base-features/hero`, `base-button` |
| Enam kartu status | `REUSE` `SummaryCards` | `base-features/summary-grid` |
| Saringan tanggal, status, kata kunci, jumlah baris | `REUSE` `DataFilter`, `FilterDatePicker`, `FilterSelect` | `base-features/data-filter`, `filter-date-picker`, `filter-select` |
| Tabel + aksi baris | `REUSE` `DataTable`, `RowActionMenu`, `StatusBadge` | `base-features/data-table`, `row-action-menu`, `status-badge` |
| Paginasi | `REUSE` `Pagination` | `features/pagination/pagination` |
| Pesan gagal, kosong, tanpa akses | `REUSE` `InformationAlert`, `AccessDeniedGate` | `base-features/information-alert`, `access-denied-gate` |
| Toast | `REUSE` `ToastStack` | `base-features/toast-stack` |
| Dialog buat reservasi | `COMPOSE` | Lihat pilihan 1 |
| Panel hasil terbit (link + WA) | `COMPOSE` | Lihat pilihan 2 |

**Pilihan 1 — dialog buat reservasi**

1. Rangkai `ConfirmModal` + `BaseSelectField` (cari pasien) + `BaseDateField` + `InformationAlert` (Direkomendasikan,
   dipakai) — semua bagian sudah base component, rupa dialog sama dengan dialog konfirmasi lain, tanpa komponen baru.
2. Halaman editor terpisah (`BaseEditorView`) — konsisten dengan form master data, tetapi petugas harus pindah halaman
   untuk dua isian saja dan kehilangan konteks daftar.

**Pilihan 2 — panel hasil terbit**

1. Rangkai `InformationAlert` + `BaseTextField` (link, baca saja) + `BaseTextAreaField` (teks pesan) + `BaseButton`
   (Kirim via WhatsApp sebagai tautan, Salin Teks, Salin Link) di dalam `ConfirmModal` (Direkomendasikan, dipakai) —
   tanpa komponen baru, link hanya hidup di state lokal dialog.
2. Tampilkan link di toast — lebih ringkas, tetapi toast hilang sendiri sehingga petugas bisa kehilangan link sebelum
   sempat mengirim.

`UI GATE: PASS — elemen REUSE dan COMPOSE saja; tidak ada NEW, dan tidak ada EXTEND yang mengubah perilaku default base component.`

### Checklist konsistensi UI

| Pemeriksaan | Hasil |
| --- | --- |
| Warna literal (`#…`, `rgb(…)`) di stylesheet baru | Tidak ada |
| `font-size`/`font-weight`/`line-height` yang menimpa komponen shared | Tidak ada |
| `<button>` mentah atau `.btn` Bootstrap di JSX baru | Tidak ada |
| `<table>` mentah | Tidak ada |
| `!important`, blok `prefers-color-scheme`, inline style statis | Tidak ada |
| Kolom tabel di berkas `*-table-columns.jsx` terpisah | Ya |
| Nilai kosong lewat `sanitizeDisplayText` / tanda hubung | Ya |

## 5. Validasi

| Langkah | Hasil |
| --- | --- |
| `npm run lint:errors` | exit 0 |
| `npm run build` | exit 0 (build akhir 19 Oktober 2026) |
| `AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/priority-reservation-data-layer.test.mjs` | `PASS` 7/7, termasuk penormalan No. HP ke format 62 |
| `AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/*.test.mjs` | 2976/3003; 27 gagal di modul lain, sudah gagal pada snapshot `HEAD` |
| `MANUAL TEST` (Playwright, build standalone port 3000, backend uji 7185) | `PASS` 15/15, rincian di bawah |

### Uji layar

Data samaran dibuat lewat API dan SQL data uji: pasien Gold Aktif (`RJPR UJI FE-CREATE2 …`, `FE-DETAIL …`), pasien
Reguler (`FE-REG …`), dan dua akun uji non-SuperAdmin: petugas skrining (`Read` + `Screen`) dan akun tanpa `Read`.

| ID | Pemeriksaan | Hasil |
| --- | --- | --- |
| MENU read | Butir menu "Reservasi Prioritas" tampil untuk pemegang `Read` | `PASS` |
| CREATE hidden | Tanpa `Create`, tombol "+ Buat Reservasi" tidak tampil | `PASS` |
| MENU noread | Butir menu tidak tampil untuk pengguna tanpa `Read` | `PASS` |
| LOADING | Teks "Mengambil data reservasi prioritas..." tampil saat memuat | `PASS` |
| LIST | Baris tampil; pencarian no. RSV menyaring ke satu baris berisi pasien, RM, status | `PASS` |
| SUMMARY | Kartu ringkasan "Menunggu Kedatangan" tampil | `PASS` |
| EMPTY | Pencarian tanpa hasil (`ZZZTIDAKADA`): "Belum ada reservasi prioritas pada tanggal ini." | `PASS` |
| SEARCH short | Kata kunci 2 karakter: petunjuk minimal 3 karakter, tidak mengirim request 400 | `PASS` |
| ERROR | API gagal (tiruan 500): pesan server dan tombol "Coba lagi" | `PASS` |
| RETRY | "Coba lagi" memuat ulang daftar | `PASS` |
| **`UAT-RP-08`** | Pasien Reguler: pesan bukan member aktif, Simpan nonaktif | `PASS` |
| **`UAT-RP-09`** | Reservasi terbuka sudah ada: pesan menyebut nomor yang ada, Simpan nonaktif | `PASS` |
| ELIGIBLE | Gold Aktif, tanggal lusa: pesan layak, Simpan aktif | `PASS` |
| **`UAT-RP-07`** | Terbit: link 43 karakter tampil, teks WA tanpa poli/dokter, tombol WA ke `wa.me/62…?text=` | `PASS` |
| LINK gone | Link tidak ada lagi di HTML halaman sesudah dialog ditutup | `PASS` |

Tangkapan layar: `fe-rjp-004-list.png`, `fe-rjp-004-create.png`, `fe-rjp-004-issued.png` di scratchpad sesi.

## 6. Delta dan catatan

| Butir | Catatan |
| --- | --- |
| Pencarian pasien | Memakai `searchEmergencyRegistrationPatients` yang sudah ada (sama dengan Pendaftaran Pasien). Tidak ada endpoint baru |
| Detail | Detail reservasi dibuka sebagai route anak `priority-reservations/[id]` (`FE-RJP-005`), bukan drawer. Bentuk ini `DEV_DISCRETION` (`RJ-PRI-FE-003`) |
| Pesan kosong | Bunyi keadaan kosong mengikuti skema `FE-RP-01`: "Belum ada reservasi prioritas pada tanggal ini." Bunyi yang sama juga tampil bila kata kunci tidak menemukan apa pun. Bila perlu dibedakan, usulkan pesan terpisah untuk hasil pencarian kosong pada revisi skema berikutnya |
| Risiko No. HP | Bila normalisasi salah, WA terbuka ke nomor lain. Mitigasi: dialog menampilkan tujuan WA yang disamarkan, dan petugas tetap melihat nomor tujuan di aplikasi WA sebelum mengirim |

## 7. Wewenang dan batas

- Posisi menu, ikon, bentuk dialog, dan kalimat persis pesan WA adalah `DEV_DISCRETION` (`RJ-PRI-FE-002`, `003`).
  Isi pesan WA mengikuti kontrak: tanpa poli dan dokter.
- Suntingan user yang belum di-commit di `emergency-registration/patient-selection-step.jsx` tidak disentuh.
- Tidak ada stage, commit, push, merge, atau deploy.

## 8. Rework 19 Oktober 2026 — revisi layar atas permintaan owner (`RJ-PRI-DEC-032`)

### 8.1 Permintaan dan keputusan

Owner meminta enam perubahan pada layar **Rawat Jalan → Reservasi Prioritas** (disertai tangkapan layar):

| No | Permintaan | Yang dikerjakan |
| --- | --- | --- |
| 1 | Tombol "+ Buat Reservasi" pindah ke kartu saringan seperti modul lain | Tombol dipindah dari Hero ke `actions` milik `DataFilter`, seperti Order Darah |
| 2 | Saringan Tanggal Awal, Tanggal Akhir, Periode, pencarian, jumlah baris, ikon muat ulang | Ditambah Tanggal Awal, Tanggal Akhir, Periode. Status tetap. Semuanya satu baris pada layar ≥ 1200 px, mengikuti Daftar Pasien Rawat Jalan |
| 3 | Kolom wajib No, Tanggal Dibuat, Nama, Dibuat Oleh, Diubah Oleh, Status | Kolom menjadi No, Tanggal Dibuat, No. Reservasi, Nama Pasien, Tanggal Kunjungan, Tingkat, Tiba, Skrining, Dibuat Oleh, Diubah Oleh, Status |
| 4 | Hapus kolom Aksi; detail lewat klik dua kali; Lengkapi hanya dari detail | Kolom Aksi dihapus. Klik dua kali sudah membuka detail; tombol Lengkapi Pendaftaran tetap ada di detail |
| 5 | Tombol primer hijau menjadi warna primer seperti modul lain | Tombol "Tandai Tiba" di detail (laporan `FE-RJP-005` bagian 8) dan "Kirim via WhatsApp" di dialog terbit (8.3) |
| 6 | "Tidak bisa buat reservasi" | Lihat 8.2 |

Yang disaring adalah **tanggal kunjungan**. Owner memilih ini dari dua opsi (tanggal kunjungan atau tanggal dibuat),
karena admisi bekerja per hari kedatangan. Contoh: reservasi yang dibuat kemarin untuk hari ini tetap tampil di
periode "Hari Ini".

Pilihan Periode: Hari Ini (bawaan), Besok, 7 Hari ke Depan, 30 Hari ke Depan, 7 Hari Terakhir, Bulan Ini, dan Rentang
Tanggal. Pilihan ke depan ditambahkan karena reservasi boleh dibuat sampai 30 hari ke depan; Daftar Pasien Rawat Jalan
hanya punya pilihan ke belakang. Mengisi Tanggal Awal atau Tanggal Akhir sendiri otomatis beralih ke Rentang Tanggal,
yang sesuai konvensi `DataFilter` tampil sebagai "Periode". Ikon muat ulang mengembalikan saringan ke Hari Ini.

### 8.2 Penelusuran "tidak bisa buat reservasi"

| Langkah | Temuan |
| --- | --- |
| Reproduksi di stack owner (FE dev 3000 + backend Debug 7184) dengan pasien uji Gold dan tanggal 22 Okt 2026 | Berhasil: `RSV-20261019-0067` terbit, link dan teks WA tampil |
| Pencarian pasien lewat nama, No. RM berformat, dan No. HP | Berhasil. No. RM tanpa tanda hubung (`0000000158`) tidak ditemukan, sama seperti pencarian Pendaftaran Pasien |
| Gejala menurut owner | Muncul **pesan penolakan** |
| Isi DB uji `QuilvianNewDevSukma` | Hanya 82 pasien member Aktif, semuanya data uji. Label "Gold Member" di pencarian konsisten dengan aturan kelayakan |

Kesimpulan: tidak ada bug penyimpanan. Pasien yang dicoba hampir pasti bukan member Aktif, sehingga backend menolak
dengan benar ("Pasien bukan member aktif hari ini…"). Supaya petugas tahu sebelum memilih, dialog kini:

1. Menampilkan petunjuk di bawah isian Pasien: "Hanya pasien member Aktif. Pasien Reguler didaftarkan lewat Pendaftaran
   Pasien."
2. Menampilkan setiap pilihan pasien dalam dua baris: nama, lalu "RM … · Gold Member" atau "RM … · Bukan member".

### 8.3 Perubahan

| Berkas (repo frontend) | Status | Isi |
| --- | --- | --- |
| `src/lib/constants/health-services/registration-management/priority-reservation-constants.js` | Diperbarui | `PRIORITY_RESERVATION_PERIODS`, `PRIORITY_RESERVATION_PERIOD_OPTIONS`; saringan bawaan `period`/`dateFrom`/`dateTo` menggantikan `visitDate`; pesan kosong "…pada rentang tanggal ini." |
| `src/utils/health-services/registration-management/priority-reservation/priority-reservation.utils.js` | Diperbarui | `resolvePriorityReservationPeriodRange`, `resolvePriorityReservationFilterRange` |
| `src/lib/services/health-services/registration-management/priority-reservation.service.js` | Diperbarui | `getPriorityReservationSummary({ visitDateFrom, visitDateTo })` (kontrak `1.1.0`) |
| `src/lib/state/slice/health-services/registration-management/priority-reservation-slice.jsx` | Diperbarui | Thunk ringkasan meneruskan rentang |
| `src/lib/hooks/health-services/registration-management/priority-reservations/use-priority-reservation-list.jsx` | Diperbarui | Rentang dari periode/isian, `setPeriod`, `setDateFilter`; kabar realtime disaring per rentang; aksi Lengkapi dari daftar dihapus |
| `src/lib/hooks/health-services/registration-management/priority-reservations/use-priority-reservation-create.jsx` | Diperbarui | Pilihan pasien membawa `name` + `description` (No. RM dan status member) |
| `src/components/view/health-services/registration-management/priority-reservations/priority-reservation-list-view.jsx` | Diperbarui | Tombol di `DataFilter.actions`; Tanggal Awal, Tanggal Akhir, Periode; kelas `singleRowFilter` |
| `src/components/view/health-services/registration-management/priority-reservations/priority-reservation-table-columns.jsx` | Diperbarui | Kolom baru; tanpa kolom Aksi |
| `src/components/view/health-services/registration-management/priority-reservations/priority-reservation-create-modal.jsx` | Diperbarui | Petunjuk member Aktif; `renderOption` dua baris |
| `src/components/view/health-services/registration-management/priority-reservations/priority-reservation-issued-panel.jsx` | Diperbarui | "Kirim via WhatsApp" `variant="primary"` (sebelumnya `success` hijau) |
| `src/style/health-services/registration-management/priority-reservations/priority-reservation.module.css` | Diperbarui | `.singleRowFilter` (pola Daftar Pasien Rawat Jalan, tanpa warna atau huruf), `.patientOption`, `.patientOptionMeta` (token) |
| `tests/unit/priority-reservation-data-layer.test.mjs` | Diperbarui | Ringkasan rentang; uji baru perhitungan periode dan rentang (total 8 uji) |

### 8.4 Gerbang komponen dasar

Modul referensi visual: Order Darah (tombol di kartu saringan) dan Daftar Pasien Rawat Jalan (saringan
tanggal/periode satu baris).

| Elemen | Keputusan | Bukti |
| --- | --- | --- |
| Tombol "+ Buat Reservasi" di kartu saringan | `REUSE` prop `actions` milik `DataFilter` + `BaseButton` primer | `blood-order-list-view.jsx` |
| Tanggal Awal, Tanggal Akhir | `REUSE` `FilterDatePicker` | `outpatient-encounter-list-view.jsx` |
| Periode, Status, Jumlah Baris | `REUSE` `FilterSelect` | idem |
| Saringan satu baris | `COMPOSE` kelas feature `singleRowFilter` | Lihat pilihan 1 |
| Kolom audit (Dibuat/Diubah Oleh) | `REUSE` kelas sel tabel `regionNameCell`, `supportingCell` | `kiosk-device-table-columns.jsx` ("Dibuat Oleh") |
| Pilihan pasien dua baris | `REUSE` prop `renderOption` milik `BaseSelectField`/`FilterSelect` | `billing-data-view.jsx` |

**Pilihan 1 — saringan satu baris**

1. Kelas feature `.singleRowFilter` yang meniru aturan Daftar Pasien Rawat Jalan: `nowrap` pada ≥ 1200 px, pencarian
   lebih lebar, Jumlah Baris lebar tetap (**Direkomendasikan**, dipakai). Rupa sama dengan layar Rawat Jalan lain, tidak
   menyentuh `DataFilter`, tanpa risiko regresi di modul lain. Biaya: 20 baris CSS tanpa warna atau huruf.
2. Biarkan `DataFilter` membungkus. Tanpa CSS, tetapi "25 baris" turun ke baris kedua dan melebar penuh sehingga
   terlihat janggal (ditemukan pada uji layar pertama).

`UI GATE: PASS — REUSE dan satu COMPOSE gaya feature; tidak ada NEW, dan tidak ada EXTEND pada base component.`

Grep anti-regresi pada berkas yang berubah: tidak ada warna literal, `font-size`/`font-weight`/`line-height`,
`!important`, `<button>` mentah, `<table>`, atau inline style.

### 8.5 Validasi

| Langkah | Hasil |
| --- | --- |
| `npm run lint:errors` | exit 0 |
| `npm run build` | exit 0 (Next.js 16.2.12; `next dev` user di port 3000 tetap berjalan karena memakai `.next/dev`) |
| `AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/priority-reservation-data-layer.test.mjs` | `PASS` 8/8 |
| `AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/*.test.mjs` | 2977/3004; 27 gagal di modul lain, daftarnya sama persis dengan sebelum rework |
| `MANUAL TEST` (Playwright pada `next dev` user di 3000; API browser uji diarahkan 7184 → backend uji 7185) | `PASS` 12/12 rework + 15/15 regresi |

| ID | Pemeriksaan | Hasil |
| --- | --- | --- |
| BTN filter | "+ Buat Reservasi" ada di kartu saringan, bukan di Hero | `PASS` |
| DEFAULT | Bawaan "Hari Ini": daftar dan ringkasan meminta `visitDateFrom = visitDateTo = 2026-10-19` | `PASS` |
| CONTROLS | Tanggal Awal, Tanggal Akhir, Periode, Status, Jumlah Baris, pencarian, ikon muat ulang tampil | `PASS` |
| COLUMNS | Urutan kolom sesuai 8.1 butir 3; tanpa "Aksi" | `PASS` |
| AUDIT | Baris `RSV-20261019-0068`: "19 Okt 2026, 15.28", Dibuat Oleh "SuperAdmin", Diubah Oleh "Link Pasien" + waktu | `PASS` |
| PERIOD | "7 Hari ke Depan": daftar dan ringkasan meminta 19–25 Okt; reservasi besok ditemukan | `PASS` |
| COMBINED | Periode + Status "Menunggu Kedatangan": request membawa keduanya, semua baris Menunggu Kedatangan, halaman 1 | `PASS` |
| CUSTOM | Tanggal Awal diganti 20 Okt: beralih ke rentang, request 20–25 Okt, status tetap | `PASS` |
| RESET | Ikon muat ulang: kembali ke Hari Ini tanpa status | `PASS` |
| DBLCLICK | Klik dua kali baris membuka detail | `PASS` |
| PRIMARY | "Tandai Tiba" memakai kelas `primary`, bukan `success` | `PASS` |
| CREATE hint | Petunjuk member Aktif tampil; pasien Reguler berlabel "Bukan member" | `PASS` |
| Regresi `FE-RJP-004` | 15/15 (menu per izin, tombol Buat tersembunyi tanpa `Create`, empat keadaan, `UAT-RP-07`/`08`/`09`, link hilang) | `PASS` |

Untuk regresi "tombol tersembunyi tanpa izin", dua akun uji sementara dibuat lalu dibersihkan lagi (policy dikosongkan
dan akun dinonaktifkan).

Tangkapan layar: `fe-rjp-004b-list.png`, `fe-rjp-004b-period.png`, `fe-rjp-004b-detail.png`, `fe-rjp-004b-create.png`,
`fe-rjp-004-issued.png` di scratchpad sesi.

### 8.6 Catatan

| Hal | Isi |
| --- | --- |
| Dependency backend | `BE-RJP-006` rework (kontrak `1.1.0`) ✅. Backend user di 7184 perlu dijalankan ulang dari working tree terbaru agar kolom Tanggal Dibuat, Dibuat Oleh, dan Diubah Oleh terisi; sebelum itu kolom tampil "-" |
| Overlay dev "Issues" | Saat uji, overlay `next dev` menghitung 404 foto profil dan WebSocket hub. Penyebabnya API browser uji dialihkan ke 7185 sementara WebSocket tetap ke 7184. Ini artefak lingkungan uji, bukan dari kode fitur |
| Wewenang | Posisi tombol, pilihan periode, dan susunan kolom diminta owner (`RJ-PRI-DEC-032`). Suntingan user di `patient-selection-step.jsx` tidak disentuh. Tidak ada stage, commit, push, merge, atau deploy |
