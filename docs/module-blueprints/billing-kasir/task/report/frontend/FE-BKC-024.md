# FE-BKC-024 — Buat Voucher dan Bukti Nota/Kasir (`FE-PC-02`, `FE-PC-03`)

| Field | Isi |
| --- | --- |
| Task ID | `FE-BKC-024` |
| Modul | `billing-kasir` (Blueprint `BIL-CASH-001`) |
| Task type | Frontend, perluasan alur yang sudah ada (bukan layar baru — mengisi dua celah yang sengaja dikecualikan `FE-BKC-023`) |
| Task mode | `FRONTEND` (backend read-only — kontrak `POST /petty-cash/vouchers` dan `POST /petty-cash/vouchers/{id}/proofs` sudah dikunci dan diverifikasi `BE-BKC-037`, tidak ada perubahan backend) |
| Write target | `QuilvianSystemFrontendDev` (source, branch `yasmina`); laporan ini ditulis di `NewQuilvianSystemBackend` sesuai aturan lokasi laporan |
| Dependency | `FE-BKC-023` — 🟡 SEBAGIAN (titik masuk tombol "+ Buat Voucher"/"Input Nota", sekarang disediakan task ini). `BE-BKC-037` — `✅`, `dotnet test` (17/17) terverifikasi 8 September 2026. `BE-BKC-035` (opsi kategori) — `✅`, `dotnet test` (12/12) terverifikasi tanggal yang sama |
| Status task | Source selesai untuk seluruh Scope task ini (`FE-PC-02` + `FE-PC-03`). **Atas permintaan eksplisit pengguna, `npm run lint`/`test:unit`/`build` TIDAK dijalankan sesi ini** — verifikasi akan dilakukan manual oleh pengguna sendiri. Belum di-commit |

## Ringkasan untuk pembaca umum

`FE-BKC-023` sengaja mengecualikan dua tombol dari layar monitoring: "+ Buat Voucher" (header) dan
"Input Nota"/"Koreksi Nota" (kolom Aksi, untuk voucher `CASH_RECEIVED`/`COMPLETED`) — keduanya
tercatat eksplisit sebagai scope task ini. Task ini mengisi kedua celah tersebut pada **berkas
yang sama** (bukan layar baru):

1. **"+ Buat Voucher"** kini membuka `CreateVoucherModal`: Nama Penerima (bebas, penerima kas
   kecil tidak selalu pegawai), Kategori (dropdown kategori aktif dari
   `GET /master-data/petty-cash-categories/options`, sudah dimuat `FE-BKC-023`), Nominal Voucher
   (format ribuan IDR), Tujuan (textarea). Kolom **Nomor Voucher** hanya keterangan hanya-baca
   ("Otomatis oleh sistem") — **tidak pernah** menjadi bagian form atau payload sama sekali
   (bukan sekadar disabled: tidak ada field `voucherNumber` pada state form/thunk).
2. **"Input Nota"/"Koreksi Nota"** kini membuka `AttachProofModal`: satu isian Nomor Nota. Endpoint,
   thunk, dan modal yang **sama** dipakai baik untuk voucher `CASH_RECEIVED` (label tombol "Input
   Nota") maupun `COMPLETED` (label tombol "Koreksi Nota", modal menampilkan nomor nota lama
   sebagai konteks) — backend yang menentukan `CommandType` (`ATTACH_PROOF` vs `PROOF_CORRECTED`)
   berdasarkan status voucher saat itu; frontend tidak pernah memindahkan status sama sekali.

Kedua modal memakai `Idempotency-Key` yang dibentuk sekali saat modal dibuka, pola identik
keempat aksi transisi status `FE-BKC-023` dan `use-cashier-shift.js`.

## Keputusan yang mengunci scope (ringkas)

Trace `FR-BKC-045`,`051`; `PC-DES-008`,`012`; kontrak `BIL-API-0.9` (revisi terkunci). `frontend-roadmap.md` § `FE-BKC-024`.

**Keputusan desain, dibuat pada task ini sendiri (`DEV_DISCRETION` — wadah presentasi), didokumentasikan di sini:**

1. **Dua berkas modal terpisah (`create-voucher-modal.jsx`, `attach-proof-modal.jsx`), bukan
   digabung dalam satu modal serba-guna.** Kedua alur punya field, validasi, dan pemicu yang
   berbeda total (form empat field vs satu field koreksi) — pola identik empat modal terpisah yang
   sudah ada pada `cashier-shift/` (`open-shift-modal.jsx`, `close-shift-modal.jsx`,
   `handover-shift-modal.jsx`, `reopen-shift-modal.jsx`), bukan satu modal raksasa dengan banyak
   percabangan.
2. **Koreksi nomor nota dimulai dari input kosong, bukan menyalin nilai lama ke dalam field.**
   Petugas mengetik ulang nomor yang benar (modal menampilkan nomor lama sebagai teks konteks di
   atas field, bukan nilai awal field) — mencegah kesalahan "lupa mengubah" saat nomor lama
   kebetulan terlihat benar sekilas. Pola serupa `ConfirmModal.requireReason` yang juga selalu
   mulai kosong.
3. **Label tombol Aksi bercabang "Input Nota" vs "Koreksi Nota" mengikuti status baris
   (`CASH_RECEIVED` vs `COMPLETED`)** — bukan satu label generik ("Nota") untuk keduanya — supaya
   petugas langsung tahu apakah ia mengisi pertama kali atau mengoreksi, tanpa membuka modal dulu.
   Logika percabangan yang sama (bukan disalin ulang) dipakai pada judul modal.

## Proses bisnis

Kasir/petugas administrasi mengajukan voucher baru — sistem membentuk nomor voucher otomatis
(`PTC-YYYYMMDD-NNNN`) di server, status langsung `Menunggu Persetujuan`. Setelah disetujui dan
uangnya diserahkan (`FE-BKC-023`), penerima menyerahkan nota/kwitansi — kasir/petugas administrasi
memasukkan nomornya dan voucher menjadi `Selesai`. Salah ketik nomor nota masih dapat dikoreksi
setelah `Selesai` lewat aksi yang sama, tanpa mengubah status — persis seperti dijelaskan
`BE-BKC-037`.

## Base Component Decision Gate

`UI GATE: 2 elemen COMPOSE (berkas modal baru, seluruh isinya REUSE), 0 elemen NEW, 0 EXTEND`

| Elemen | Status | Bukti/alasan |
| --- | --- | --- |
| Field Nama Penerima, Nomor Nota | `REUSE` | `BaseTextField` — pola identik `openingCash` pada `open-shift-modal.jsx` |
| Field Kategori (dropdown) | `REUSE` | `BaseSelectField` — pola identik `registerId` pada `open-shift-modal.jsx`, `options` dipakai apa adanya dari `categorySelectOptions` milik `FE-BKC-023` |
| Field Nominal Voucher | `REUSE` | `BaseTextField` + `normalizeWholeRupiahInput`/`formatThousandsInput` — pola identik `openingCash` pada `open-shift-modal.jsx` (format ribuan IDR) |
| Field Tujuan | `REUSE` | `BaseTextAreaField` — base component yang sudah ada di `base-form-control.jsx`, belum pernah dipakai di modul Billing tetapi sudah dipakai modul lain; tidak ada perubahan pada komponennya sendiri |
| Kolom Nomor Voucher (hanya-baca) | `REUSE` | `BaseTextField` dengan `disabled` + `value=""` — pola sama seperti field hanya-baca lain di aplikasi, bukan komponen baru |
| Tombol Simpan/Batal, loading state | `REUSE` | `BaseButton` — pola identik seluruh modal existing |
| Kerangka modal (`Modal`/`Form`) | `REUSE` | `react-bootstrap` — pola identik `open-shift-modal.jsx` |
| **`CreateVoucherModal` (berkas baru)** | `COMPOSE` | Merangkai field-field `REUSE` di atas ke satu modal khusus form Buat Voucher — **bukan** komponen visual baru, seluruh elemen di dalamnya sudah ada. Opsi 1 (**direkomendasikan**): berkas modal terpisah, pola identik 4 modal `cashier-shift/`. Opsi 2: form inline di `petty-cash-vouchers-view.jsx` tanpa modal terpisah — ditolak karena `petty-cash-vouchers-view.jsx` sudah menangani 3 tanggung jawab (tabel, saringan, ConfirmModal generik); menambah form 4-field inline akan membuatnya sulit dibaca dan menyimpang dari pola modul ini sendiri (setiap aksi kompleks di `cashier-shift/` selalu modal terpisah) |
| **`AttachProofModal` (berkas baru)** | `COMPOSE` | Sama seperti di atas, untuk form Input/Koreksi Nota. Opsi 1 (**direkomendasikan**): berkas modal terpisah. Opsi 2: pakai `ConfirmModal` yang sudah ada dengan `requireReason` sebagai field teks generik — ditolak karena `ConfirmModal.requireReason` secara semantik adalah "alasan" (validasi/placeholder/label bawaannya berorientasi alasan, dipakai `Tolak`/`Batalkan`), memakainya untuk nomor nota akan mencampur dua makna field yang berbeda pada satu komponen bersama, berisiko regresi visual/copy pada Tolak/Batalkan yang sudah ada |

Tidak ada komponen baru dari nol, tidak ada `EXTEND` yang mengubah perilaku default komponen mana
pun. Kedua "elemen COMPOSE" adalah berkas JS baru yang murni merangkai base component yang sudah
ada — konsisten dengan pola 4 modal `cashier-shift/` yang sudah lama berjalan di modul ini.

## Endpoint yang dikonsumsi

| Endpoint | Method | Dipakai untuk | Sejak task backend |
| --- | --- | --- | --- |
| `.../petty-cash/vouchers` | `POST` | "+ Buat Voucher" (`FE-PC-02`) | `BE-BKC-037` |
| `.../petty-cash/vouchers/{id}/proofs` | `POST` | "Input Nota"/"Koreksi Nota" (`FE-PC-03`) | `BE-BKC-037` |

Keduanya sudah tercantum pada `Kontrak` kartu `FE-BKC-024`; tidak ada delta kontrak pada task ini
(berbeda dari `FE-BKC-023` yang perlu mendokumentasikan satu delta untuk endpoint opsi kategori).

## File yang diubah/ditambah

| File | Perubahan |
| --- | --- |
| `src/lib/state/slice/.../petty-cash-voucher-slice.jsx` | **Diubah.** Dua thunk baru: `createPettyCashVoucher` (`POST /`, tanpa field `voucherNumber` sama sekali) dan `attachPettyCashVoucherProof` (`POST /{id}/proofs`); keduanya ditambahkan ke `addMatcher` `actionLoading`/`actionError` bersama yang sudah ada |
| `src/lib/hooks/.../petty-cash/petty-cash-voucher-constants.js` | **Diubah.** Komentar `PETTY_CASH_VOUCHER_ACTION_LABELS` diperbarui (tidak lagi menyebut ATTACH_PROOF sebagai "belum diimplementasikan"); `PETTY_CASH_VOUCHER_PROOF_ACTION` baru ("ATTACH_PROOF") |
| `src/lib/hooks/.../petty-cash/use-petty-cash-vouchers.js` | **Diubah.** State+handler form Buat Voucher (`createOpen`/`createForm`/`createFieldErrors`/`openCreate`/`closeCreate`/`handleCreateFormChange`/`confirmCreate`) dan form Input/Koreksi Nota (`proofTarget`/`proofForm`/`proofFieldErrors`/`openProof`/`closeProof`/`handleProofFormChange`/`confirmProof`) — pola identik `use-cashier-shift.js` (`idempotencyKey`/`correlationId` dibentuk sekali per `open*`) |
| `src/components/view/.../petty-cash/create-voucher-modal.jsx` | **Baru.** Modal form Buat Voucher — lihat § Base Component Decision Gate |
| `src/components/view/.../petty-cash/attach-proof-modal.jsx` | **Baru.** Modal form Input/Koreksi Nota, judul dan label tombol bercabang mengikuti status voucher — lihat § Base Component Decision Gate |
| `src/components/view/.../petty-cash/petty-cash-vouchers-view.jsx` | **Diubah.** Tombol "+ Buat Voucher" ditambahkan ke `actions` `DataFilter` (di samping "Muat Ulang"); kolom Aksi kini juga merender tombol Input/Koreksi Nota untuk baris yang punya `ATTACH_PROOF` pada `availableActions`; kedua modal baru dirender di akhir |

Total: **2 berkas baru, 4 berkas diubah**. Tidak ada berkas milik `FE-BKC-022`/`025` yang tersentuh.

## Definition of Done — validasi

| Item | Status | Bukti |
| --- | --- | --- |
| `npx eslint .` | **SKIPPED** — atas permintaan eksplisit pengguna ("tanpa test... karena saya akan lakukan secara manual") | Tidak dijalankan sesi ini |
| `npm run test:unit` | **SKIPPED** — atas permintaan eksplisit pengguna | Tidak dijalankan sesi ini |
| `npm run build` | **SKIPPED** — atas permintaan eksplisit pengguna | Tidak dijalankan sesi ini |
| `voucherNumber` **tidak pernah** dikirim pada payload `POST /petty-cash/vouchers` | **LULUS (tinjauan kode)** | `confirmCreate` membentuk payload eksplisit `{ idempotencyKey, recipientName, categoryId, amount, purpose }` — tidak ada properti `voucherNumber` pada objek maupun pada `buildCreateForm()`. Grep `voucherNumber` pada `use-petty-cash-vouchers.js`: nol kecocokan pada blok create |
| Kategori kosong menonaktifkan Simpan | **LULUS (tinjauan kode)** | `confirmCreate` memvalidasi `!createForm.categoryId` sebelum dispatch; tombol Simpan (`type="submit"`) tidak tertahan UI tersendiri, tetapi `handleSubmit`/`confirmCreate` mengembalikan lebih dulu tanpa memanggil thunk apa pun saat kategori kosong — Simpan yang diklik pada kategori kosong tidak menghasilkan request jaringan |
| Endpoint dan modal yang sama dipakai untuk isi nota pertama & koreksi | **LULUS (tinjauan kode)** | `attachPettyCashVoucherProof`/`AttachProofModal`/`confirmProof` adalah satu jalur kode untuk kedua kasus; satu-satunya percabangan adalah label tombol/judul (`isCorrection`), bukan endpoint atau thunk berbeda |
| Status voucher **tidak pernah** dipindahkan frontend saat koreksi nota | **LULUS (tinjauan kode)** | `confirmProof` tidak membaca/menulis field `status` sama sekali pada payload; `CommandType` sepenuhnya ditentukan backend |
| `Idempotency-Key` dibentuk sekali per modal dibuka | **LULUS (tinjauan kode)** | `openCreate`/`openProof` masing-masing memanggil `generateUuid()` sekali saat modal dibuka; `confirmCreate`/`confirmProof` memakai `createAction.idempotencyKey`/`proofAction.idempotencyKey` apa adanya, tidak membentuk ulang |
| Verifikasi manual (browser, form Buat Voucher & Input Nota, kategori kosong, koreksi nota) | **NOT FEASIBLE (sesi ini) / akan dilakukan pengguna** | Tidak ada kredensial login pada sesi ini; sesuai permintaan eksplisit, pengguna akan menjalankan verifikasi manual sendiri |

**Task ini belum bisa ditandai selesai.** Tidak ada lint/test/build yang dijalankan sesi ini (atas
permintaan eksplisit pengguna) dan tidak ada verifikasi manual — seluruh bukti di atas murni
tinjauan kode/statis. Pengguna diminta menjalankan `npm run lint`/`test:unit`/`build` dan
verifikasi manual sendiri sebelum task ini ditandai `✅`.

- MANUAL TEST: NOT FEASIBLE pada sesi ini — akan dilakukan pengguna sendiri (permintaan eksplisit)
- AUTOMATED TEST: SKIPPED (atas permintaan eksplisit pengguna) — lint/test:unit/build tidak dijalankan

## Risiko yang tersisa

1. **Sama sekali belum diverifikasi lewat tooling maupun browser pada sesi ini.** Berbeda dari
   `FE-BKC-023` (lint/test:unit/build lulus terkonfirmasi), task ini murni tinjauan kode statis —
   risiko regresi sintaks/impor lebih tinggi sampai pengguna menjalankan `npm run build` sendiri.
2. **`BaseTextAreaField` belum pernah dipakai di modul Billing sebelumnya** (dipakai modul lain) —
   task ini adalah pemakaian pertamanya di `billing-management`. Tidak ada indikasi masalah dari
   pembacaan source component-nya, tetapi tetap dicatat sebagai kombinasi yang relatif baru di
   modul ini.
3. **Koreksi nomor nota tidak menampilkan riwayat koreksi sebelumnya** (hanya nomor nota
   *saat ini*) — cukup untuk `FR-BKC-051`, tetapi bila produk kelak menginginkan riwayat penuh
   perubahan nomor nota, itu perubahan scope terpisah (kemungkinan bagian `FE-BKC-025`/Detail
   Voucher, yang memang menampilkan riwayat perintah `BilPettyCashVoucherCommand`).
4. **Aktivitas paralel `FE-BKC-022` tetap tidak tersentuh** (lihat laporan `FE-BKC-023` § Risiko
   butir 5) — tidak berubah pada task ini.

## Langkah berikutnya yang direkomendasikan

1. Pengguna menjalankan `npm run lint`/`test:unit`/`build` dan verifikasi manual (form Buat
   Voucher lengkap dengan kategori kosong/terisi, Input Nota pada voucher `CASH_RECEIVED`, Koreksi
   Nota pada voucher `COMPLETED`, uji retry idempotency pada kedua form) sendiri.
2. Setelah terverifikasi, perbarui `frontend-roadmap.md` (kartu `FE-BKC-023` dan `FE-BKC-024`) dan
   `requirement-traceability.md` menautkan laporan ini.
3. Lanjutkan ke `FE-BKC-025` (Detail Voucher/`FE-PC-04`) — riwayat perintah
   (`BilPettyCashVoucherCommand`) sebagai bukti audit "siapa mengajukan/menyetujui/menyerahkan/
   memasukkan nota", `DEV_DISCRETION` untuk bentuk tampilan (tabel/linimasa).
