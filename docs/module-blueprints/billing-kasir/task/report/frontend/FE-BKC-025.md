# FE-BKC-025 — Detail Voucher (`FE-PC-04`)

| Field | Isi |
| --- | --- |
| Task ID | `FE-BKC-025` |
| Modul | `billing-kasir` (Blueprint `BIL-CASH-001`) |
| Task type | Frontend, perluasan alur yang sudah ada (bukan layar/route baru — modal detail dari layar monitoring `FE-BKC-023`) |
| Task mode | `FRONTEND` (backend read-only — kontrak `GET /petty-cash/vouchers/{id}` sudah dikunci dan diverifikasi `BE-BKC-037`, tidak ada perubahan backend) |
| Write target | `QuilvianSystemFrontendDev` (source, branch `yasmina`); laporan ini ditulis di `NewQuilvianSystemBackend` sesuai aturan lokasi laporan |
| Dependency | `FE-BKC-023` — 🟡 SEBAGIAN (titik masuk klik dua kali pada baris, sekarang disediakan task ini). `BE-BKC-037` — `✅`, `dotnet test` (17/17) terverifikasi 8 September 2026 |
| Status task | Source selesai untuk seluruh Scope task ini. **Atas permintaan eksplisit pengguna, `npm run lint`/`test:unit`/`build` TIDAK dijalankan sesi ini** — verifikasi akan dilakukan manual oleh pengguna sendiri. Belum di-commit |

## Ringkasan untuk pembaca umum

`FE-BKC-023` mendaftarkan "klik dua kali pada baris" sebagai titik masuk detail voucher, tetapi
belum ada modal yang terbuka saat itu terjadi. Task ini menutup celah itu: mengklik dua kali baris
mana pun pada tabel monitoring kini membuka `VoucherDetailModal`, menampilkan:

1. **Ringkasan voucher** — status (badge, label persis dari server), nominal, nama penerima,
   kategori, tujuan, dan tanggal setiap tahap yang sudah dilalui (diajukan/diputuskan/diserahkan/
   nota) beserta siapa pelakunya — hanya baris yang relevan yang tampil (mis. "Diputuskan Oleh"
   hanya muncul bila voucher sudah diputuskan).
2. **Riwayat Perintah** — tabel `BilPettyCashVoucherCommand` dari `GET /{id}` (field `commands`):
   waktu, jenis aksi (label Indonesia), perubahan status, pelaku beserta perannya, dan alasan (bila
   ada) — jawaban langsung untuk "siapa mengajukan/menyetujui/menyerahkan/memasukkan nota"
   (`FR-BKC-053`).

## Keputusan yang mengunci scope (ringkas)

Trace `FR-BKC-053`. Kontrak `GET /petty-cash/vouchers/{id}` (`BIL-API-0.9`, terkunci). `frontend-roadmap.md` § `FE-BKC-025` — bentuk tampilan riwayat (tabel/linimasa) eksplisit `DEV_DISCRETION`.

**Keputusan desain, dibuat pada task ini sendiri (`DEV_DISCRETION`), didokumentasikan di sini:**

1. **Modal, bukan route/halaman terpisah.** Dependency task ini sendiri menyebut "titik masuk klik
   dua kali pada baris" — pola interaksi popover/modal, bukan navigasi. `DataTable` yang sudah
   dipakai `FE-BKC-023` sudah punya prop `onRowDoubleClick` bawaan (tidak pernah dipakai
   sebelumnya di modul ini, tetapi sudah ada di komponennya) — REUSE murni, tanpa menambah route
   baru atau state URL.
2. **Riwayat perintah sebagai tabel (`DataTable`, `pagination={false}`), bukan linimasa
   custom.** `DataTable` sudah dipakai tiga kali pada fitur Petty Cash yang sama (daftar voucher,
   filter, dan sekarang riwayat) — konsisten dan nol risiko komponen baru. **Opsi 1 (direkomendasikan):**
   `DataTable` tanpa paginasi (daftar riwayat per voucher selalu pendek, tidak perlu di-page).
   **Opsi 2:** linimasa vertikal custom (seperti `emergency-triage-history-timeline.jsx` pada
   modul IGD) — ditolak karena itu komponen modul lain (bukan referensi visual terdekat, yang
   seharusnya Billing Management dulu), dan akan menjadi elemen visual `NEW` yang menunggu
   persetujuan pengguna alih-alih `REUSE` yang bisa langsung dikerjakan.
3. **Label transisi status pada riwayat (`StatusBefore → StatusAfter`) diambil dari `StatusOptions`
   milik `GET .../filters/metadata`** (sudah dimuat `FE-BKC-023` untuk saringan Status) — **bukan**
   peta label baru yang ditulis terpisah di modal ini. `Commands` dari `GET /{id}` hanya
   mengirim kode mentah (`WAITING_APPROVAL`, dst.), tidak ada field label langsung — memakai
   `StatusOptions` yang sudah ada menjaga satu-satunya sumber label status tetap backend,
   konsisten dengan `PC-DEC-013` yang sama yang mengunci `StatusLabel` pada baris daftar.
4. **Ringkasan detail memakai `<dl>` grid inline (bukan `BaseDetailCard`).** `BaseDetailCard`
   adalah komponen besar (avatar, section audit, dll.) yang dirancang untuk halaman detail penuh
   (dipakai modul Rawat Inap) — tidak ada precedent pemakaiannya di Billing Management, dan
   fitur-fitur itu (avatar inisial, section "Informasi Tambahan") tidak relevan untuk modal
   ringkas ini. Pola yang **sudah ada persis** di modul yang sama —
   `other-shift-preview.jsx` (`<dl>` dengan `display: grid` inline, dipakai untuk pratinjau shift
   lain sebelum konfirmasi) — dipakai apa adanya sebagai referensi, bukan komponen baru.

## Proses bisnis

Siapa pun yang berhak membaca voucher (izin `PettyCashVoucher : Read`, sama seperti layar
monitoring) dapat membuka detail satu voucher untuk melihat jejak lengkapnya — kapan diajukan,
siapa menyetujui atau menolak, kapan uang diserahkan, dan kapan nota dimasukkan (termasuk koreksi
nomor nota, yang tercatat sebagai baris `PROOF_CORRECTED` tersendiri tanpa mengubah status). Ini
satu-satunya jawaban audit "siapa menyetujui pengeluaran ini" — urutan dan pelaku pada tabel
riwayat diambil apa adanya dari respons server (`Commands`), tidak diurutkan ulang atau disaring
di frontend.

## Base Component Decision Gate

`UI GATE: 1 elemen COMPOSE (berkas modal baru, seluruh isinya REUSE), 0 elemen NEW, 0 EXTEND`

| Elemen | Status | Bukti/alasan |
| --- | --- | --- |
| Pemicu klik dua kali pada baris | `REUSE` | Prop `onRowDoubleClick` bawaan `DataTable` (belum pernah dipakai di modul ini, tetapi tidak menyentuh komponennya sama sekali) |
| Kerangka modal (`Modal`) | `REUSE` | `react-bootstrap` — pola identik `create-voucher-modal.jsx`/`attach-proof-modal.jsx` (`FE-BKC-024`) |
| Badge status | `REUSE` | `StatusBadge` — pola identik kolom Status pada tabel monitoring (`FE-BKC-023`), label dari `StatusLabel` server |
| Ringkasan field voucher | `REUSE` | `<dl>` grid inline — pola identik `other-shift-preview.jsx` § Keputusan butir 4 |
| Riwayat perintah | `REUSE` | `DataTable` dengan `pagination={false}` — pola identik tabel voucher `FE-BKC-023`, hanya paginasi dimatikan |
| Tombol Tutup/Coba Lagi | `REUSE` | `BaseButton` |
| Alert error | `REUSE` | `InformationAlert` |
| **`VoucherDetailModal` (berkas baru)** | `COMPOSE` | Merangkai elemen `REUSE` di atas ke satu modal khusus detail — lihat § Keputusan butir 1 untuk alasan bentuk modal (bukan route), butir 2 untuk `DataTable` (bukan linimasa custom) |

Tidak ada komponen baru dari nol, tidak ada `EXTEND` yang mengubah perilaku default komponen mana
pun. Elemen `COMPOSE` adalah satu berkas JS baru yang murni merangkai base component yang sudah
ada — konsisten dengan pola `create-voucher-modal.jsx`/`attach-proof-modal.jsx` pada `FE-BKC-024`.

## Endpoint yang dikonsumsi

| Endpoint | Method | Dipakai untuk | Sejak task backend |
| --- | --- | --- | --- |
| `.../petty-cash/vouchers/{id}` | `GET` | Detail voucher + riwayat perintah (`FE-PC-04`) | `BE-BKC-037` |

Tidak ada delta kontrak — endpoint dan bentuk responsnya (`PettyCashVoucherDetailResponse`:
`voucher` + `commands`) dipakai apa adanya.

## File yang diubah/ditambah

| File | Perubahan |
| --- | --- |
| `src/lib/state/slice/.../petty-cash-voucher-slice.jsx` | **Diubah.** Thunk baru `getPettyCashVoucherById` (`GET /{id}`); state `detail`/`detailLoading`/`detailError` + reducer `clearPettyCashVoucherDetail`; tiga selector baru |
| `src/lib/hooks/.../petty-cash/petty-cash-voucher-constants.js` | **Diubah.** `PETTY_CASH_VOUCHER_COMMAND_TYPE_LABELS` baru (tujuh `CommandType` persis dari `PettyCashVoucherCommandTypes` backend) |
| `src/lib/hooks/.../petty-cash/use-petty-cash-vouchers.js` | **Diubah.** State+handler detail (`detailVoucherId`/`detail`/`detailLoading`/`detailError`/`openDetail`/`closeDetail`/`retryDetail`); satu `useEffect` baru yang mem-fetch ulang saat `detailVoucherId` berubah, dengan `abort()` saat berganti/unmount (pola identik `useEffect` daftar voucher) |
| `src/components/view/.../petty-cash/voucher-detail-modal.jsx` | **Baru.** Modal detail — lihat § Base Component Decision Gate |
| `src/components/view/.../petty-cash/petty-cash-vouchers-view.jsx` | **Diubah.** `onRowDoubleClick`+`getRowTitle` (afordansi "Klik dua kali untuk lihat detail voucher") ditambahkan ke `DataTable`; `statusLabelByCode` (lookup dari `statusOptions` yang sudah dimuat) dibentuk dan diteruskan ke `VoucherDetailModal`; modal baru dirender di akhir |

Total: **1 berkas baru, 3 berkas diubah**. Tidak ada berkas milik `FE-BKC-022` yang tersentuh.

## Definition of Done — validasi

| Item | Status | Bukti |
| --- | --- | --- |
| `npx eslint .` | **SKIPPED** — atas permintaan eksplisit pengguna ("tanpa test... karena saya akan lakukan secara manual") | Tidak dijalankan sesi ini |
| `npm run test:unit` | **SKIPPED** — atas permintaan eksplisit pengguna | Tidak dijalankan sesi ini |
| `npm run build` | **SKIPPED** — atas permintaan eksplisit pengguna | Tidak dijalankan sesi ini |
| Riwayat perintah dirender dari respons `GET /{id}` (bukan dihitung/diurutkan ulang di frontend) | **LULUS (tinjauan kode)** | `commands` diambil langsung dari `field(detail, "commands", "Commands")`, dilewatkan apa adanya ke `DataTable` (`sortLatestFirst={false}` — urutan asli server dipertahankan, tidak diurutkan ulang seperti tabel daftar voucher) |
| Label transisi status bersumber dari backend, bukan dipetakan ulang lokal | **LULUS (tinjauan kode)** | `resolveStatusLabel` membaca `statusLabelByCode` yang dibentuk dari `statusOptions` (hasil `GET .../filters/metadata`); fallback ke kode mentah hanya bila kode tidak ditemukan pada peta itu (seharusnya tidak pernah terjadi untuk lima status yang ada) |
| Modal terbuka lewat klik dua kali pada baris manapun | **LULUS (tinjauan kode)** | `onRowDoubleClick={openDetail}` terpasang pada `DataTable` yang sama yang menampilkan seluruh baris voucher, tidak dibatasi status/kondisi tertentu |
| Fetch detail dibatalkan (`abort`) saat modal ditutup/ID berganti sebelum request selesai | **LULUS (tinjauan kode)** | `useEffect` mengembalikan `request?.abort?.()` pada cleanup, pola identik `useEffect` daftar voucher (`FE-BKC-023`) |
| Verifikasi manual (browser, buka detail beberapa voucher pada status berbeda, konfirmasi urutan/pelaku riwayat sesuai) | **NOT FEASIBLE (sesi ini) / akan dilakukan pengguna** | Tidak ada kredensial login pada sesi ini; sesuai permintaan eksplisit, pengguna akan menjalankan verifikasi manual sendiri |

**Task ini belum bisa ditandai selesai.** Tidak ada lint/test/build yang dijalankan sesi ini (atas
permintaan eksplisit pengguna) dan tidak ada verifikasi manual — seluruh bukti di atas murni
tinjauan kode/statis.

- MANUAL TEST: NOT FEASIBLE pada sesi ini — akan dilakukan pengguna sendiri (permintaan eksplisit)
- AUTOMATED TEST: SKIPPED (atas permintaan eksplisit pengguna) — lint/test:unit/build tidak dijalankan

## Risiko yang tersisa

1. **Sama sekali belum diverifikasi lewat tooling maupun browser pada sesi ini** — sama seperti
   `FE-BKC-024`, murni tinjauan kode statis.
2. **Tidak ada afordansi visual permanen selain `title` (tooltip hover) untuk "klik dua kali".**
   `DataTable.onRowDoubleClick` tidak mengubah `cursor`/style baris seperti `onRowClick` (yang
   menambah class `dataRowInteractive`) — `getRowTitle` sudah ditambahkan sebagai mitigasi
   (tooltip saat hover), tetapi pengguna yang tidak hover mungkin tidak menyadari baris bisa
   diklik dua kali. Ini keterbatasan `DataTable` itu sendiri (tidak diubah task ini, sesuai
   larangan mengubah perilaku default komponen tanpa persetujuan) — dicatat sebagai risiko UX
   minor, bukan diselesaikan sepihak.
3. **Riwayat perintah tidak dipaginasi** (§ Keputusan butir 2) — proporsional untuk jumlah baris
   yang realistis per voucher (maksimal sekitar 4-6 baris: submit, approve/reject/cancel,
   disburse, attach_proof, dan koreksi nota berkali-kali bila ada) tetapi belum diuji dengan data
   voucher yang punya riwayat sangat panjang (tidak ada batas jumlah koreksi nota pada backend).
4. **Aktivitas paralel `FE-BKC-022` tetap tidak tersentuh** (lihat laporan `FE-BKC-023` § Risiko
   butir 5) — tidak berubah pada task ini.

## Langkah berikutnya yang direkomendasikan

1. Pengguna menjalankan `npm run lint`/`test:unit`/`build` dan verifikasi manual (buka detail
   voucher pada tiap status — `WAITING_APPROVAL`/`APPROVED`/`CASH_RECEIVED`/`COMPLETED`/
   `REJECTED` — konfirmasi ringkasan dan riwayat perintah sesuai data sebenarnya, termasuk kasus
   koreksi nota) sendiri.
2. Setelah terverifikasi, perbarui `frontend-roadmap.md` (kartu `FE-BKC-023`/`024`/`025`) dan
   `requirement-traceability.md` menautkan laporan ini.
3. Dengan `FE-BKC-023`–`025` selesai source-nya, rumpun Petty Cash frontend `MVP-15` (`FE-PC-01`–
   `04`) sudah lengkap secara teknis — sisa pekerjaan adalah verifikasi manual ter-autentikasi
   penuh (lint/test/build + browser) yang seluruhnya menunggu pengguna, dan gelombang backend
   `MVP-13` (`BE-BKC-033`/`034`) yang masih menjadi gerbang terpisah.
