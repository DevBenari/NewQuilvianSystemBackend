# Laporan Task Frontend — `FE-RJP-009` Ganti tanggal reservasi

Definisi task: [priority-reservation-frontend-roadmap.md](../../../roadmap/priority-reservation-frontend-roadmap.md) r2, bagian `FE-RJP-009`.

## 1. Ringkasan

| Field | Isi |
| --- | --- |
| Task ID | `FE-RJP-009` |
| Slice | `MVP-3`, `EPIC RP-09` |
| Trace | `FR-RP-090`, `093` s.d. `096`; `RJ-PRI-DEC-034` s.d. `039`; `RJ-PRI-FE-003`, `005`; `UAT-RP-20` s.d. `23`; `R-RP-41` (sisi layar) |
| Kontrak | `RJ-PRI-RESV-001@1.2.0` `approved`: `POST /{id}/reschedule`, `canReschedule`, `histories[].fromVisitDate`/`toVisitDate`, event `PriorityReservationChanged.previousVisitDate` |
| Desain | `03-frontend-architecture.md` RP-GT-FE.1–6 |
| Dependency | `FE-RJP-005` ✅, `BE-RJP-013` ✅ |
| Task mode | `FRONTEND` (`CROSS-REPO MODE`, `RJ-PRI-DEC-039`) |
| Branch / baseline | FE `sukmagpV2` @ `037c975b7`, working tree bersih saat mulai, belum di-commit |
| Tanggal | 21 Oktober 2026 |
| Model | Claude Opus 5 |
| Status | ✅ Selesai — uji layar 13/13 `PASS` (dua akun, 1440 px dan 390 px, kabar realtime asli dari backend uji), unit task 12/12 `PASS`, lint dan build `PASS`. Rework 21 Okt 2026 (`RJ-PRI-DEC-040`, tanggal lampau dinonaktifkan di kalender): uji layar 3/3 `PASS`, lint dan build `PASS` (bagian 8) |

## 2. Proses bisnis

Admisi membuka detail reservasi yang masih **Menunggu Kedatangan** sebelum hari kunjungan. Tombol **Ganti Tanggal**
tampil di samping Kirim Ulang Link, hanya bila backend mengirim `canReschedule = true` **dan** pengguna memegang
`OutpatientPriorityReservation : Create`.

1. Admisi menekan Ganti Tanggal. Dialog menampilkan tanggal sekarang, isian tanggal baru, dan alasan.
2. Tombol simpan nonaktif sampai tanggal dipilih dan alasan terisi.
3. Bila backend menolak, misalnya karena pasien bukan member Aktif lagi atau tanggal tujuan sudah punya reservasi,
   pesannya tampil di dalam dialog, dan dialog tetap terbuka.
4. Bila berhasil, dialog berganti ke panel link baru, yang sama dengan panel buat dan kirim ulang: link, teks WA versi
   ganti tanggal, tombol Kirim via WhatsApp, dan tombol Salin.
5. Setelah Selesai, detail menampilkan tanggal baru. Riwayat bertambah "Tanggal kunjungan diganti: 22 Oktober 2026 →
   25 Oktober 2026" beserta alasannya.

Bila petugas lain mengubah reservasi saat dialog terbuka, simpan menghasilkan toast "Reservasi baru saja diubah petugas
lain". Dialog lalu tertutup dan detail dimuat ulang, sama seperti aksi lain.

Layar daftar yang menyaring tanggal lama ikut diperbarui seketika. Contoh dari uji: daftar periode "Besok" menampilkan
`RSV-…-0025`. Petugas lain memindahkannya ke tanggal 26, dan baris itu hilang dari daftar dalam ±1 detik tanpa muat
ulang halaman.

Dialog Batalkan kini mengarahkan: "Untuk ganti tanggal sebelum hari kunjungan, gunakan Ganti Tanggal."

## 3. Perubahan

| Berkas (repo frontend) | Status | Isi |
| --- | --- | --- |
| `src/lib/services/health-services/registration-management/priority-reservation.service.js` | Diperbarui | `reschedulePriorityReservation(id, { expectedRowVersion, newVisitDate, reason })` → `POST /{id}/reschedule` |
| `src/lib/state/slice/health-services/registration-management/priority-reservation-slice.jsx` | Diperbarui | Thunk `submitPriorityReservationReschedule`, masuk kelompok perintah terbit: detail disimpan, link **tidak** |
| `src/lib/hooks/health-services/registration-management/priority-reservations/use-priority-reservation-detail.jsx` | Diperbarui | Aksi `RESCHEDULE`, isian `newVisitDate`, `confirmReschedule` (cek tanggal dan alasan, dialog tetap terbuka lalu panel link), izin `reschedule = canReschedule && Create` |
| `src/lib/hooks/health-services/registration-management/priority-reservations/use-priority-reservation-list.jsx` | Diperbarui | Penyaring kabar realtime memakai `isPriorityReservationEventInRange` (tanggal baru atau lama) |
| `src/utils/health-services/registration-management/priority-reservation/priority-reservation.utils.js` | Diperbarui | `isPriorityReservationEventInRange`, `describePriorityReservationHistoryEvent` |
| `src/lib/constants/health-services/registration-management/priority-reservation-constants.js` | Diperbarui | Pesan `RESCHEDULE_DATE_REQUIRED` |
| `src/components/view/health-services/registration-management/priority-reservations/detail/priority-reservation-action-modals.jsx` | Diperbarui | Dialog Ganti Tanggal (isian lalu panel link); teks dialog Batalkan |
| `src/components/view/health-services/registration-management/priority-reservations/detail/priority-reservation-detail-view.jsx` | Diperbarui | Tombol Ganti Tanggal; kolom Kejadian riwayat memakai tanggal lama → baru |
| `tests/unit/priority-reservation-data-layer.test.mjs` | Diperbarui | Empat uji baru (endpoint dan payload, link tidak di Redux, 409 tersimpan, penyaring realtime, teks riwayat) |

Tidak ada route, menu, stylesheet, atau komponen baru.

## 4. Gerbang komponen dasar

Modul referensi visual: detail Reservasi Prioritas sendiri (`FE-RJP-005`), terutama dialog Kirim Ulang Link dan
dialog Buat Reservasi (`FE-RJP-004`).

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Tombol Ganti Tanggal di baris aksi | `BaseButton` (`variant="secondary"`) | `base-features/base-button`, sama dengan Kirim Ulang Link | `REUSE` | Dipakai |
| Dialog dengan alasan wajib | `ConfirmModal` (`requireReason`, `reasonLabel`, `maxReasonLength`, `confirmProps.disabled`) | `base-features/confirm-modal.jsx:42-102`; alasan dikosongkan `onExited` | `REUSE` | Dipakai |
| Isian tanggal baru | `BaseDateField` | `base-form-control.jsx:500`; dipakai dialog Buat Reservasi | `REUSE` | Dipakai, dengan keterangan batas tanggal seperti dialog Buat Reservasi |
| Keterangan dan pesan gagal | `InformationAlert` | `base-features/information-alert` | `REUSE` | Dipakai |
| Panel link baru | `PriorityReservationIssuedPanel` (pembungkus domain yang sudah ada) | `priority-reservations/priority-reservation-issued-panel.jsx` | `REUSE` | Dipakai apa adanya |
| Teks riwayat tanggal lama → baru | `DataTable` (render kolom) | Kolom "Kejadian" yang sudah ada | `REUSE` | Dipakai |
| Toast hasil dan 409 | `ToastStack` | Pola `run()` hook detail | `REUSE` | Dipakai |

`UI GATE: PASS — semua elemen REUSE; tidak ada EXTEND, COMPOSE baru, WRAP, maupun NEW.`

Delta yang dicatat: `BaseDateField` tidak punya batas tanggal minimum. Batas "paling awal hari ini, paling jauh 30 hari"
ditulis sebagai keterangan isian, dan ditegakkan backend (`VAL-RP-03`, `VAL-RP-04`). Pola ini sama dengan dialog Buat
Reservasi. Menambah prop `min` ke base component bukan bagian task ini. **Diperbarui oleh rework `RJ-PRI-DEC-040`
(bagian 8): batas minimum kini dipasang di kalender.**

Checklist konsistensi UI: `git diff` frontend tidak menambah `<button>` mentah, `.btn`, `<table>`, inline style,
`!important`, maupun warna literal (grep pada baris tambahan: 0 hasil). Tidak ada stylesheet yang disentuh. Salinan teks
berbahasa Indonesia; kalimat persisnya `DEV_DISCRETION` (`RJ-PRI-FE-005`).

## 5. Validasi

| Langkah | Hasil | Klasifikasi |
| --- | --- | --- |
| `npx eslint` pada 9 berkas yang berubah | exit 0, tanpa peringatan | `PASS` |
| `npm run lint:errors` | exit 0 | `PASS` |
| `npm run build` | exit 0 (Next.js 16.2.12; `next dev` owner di 3000 tetap berjalan) | `PASS` |
| `AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/priority-reservation-data-layer.test.mjs` | 12/12 `PASS` (8 lama + 4 baru) | `PASS` |
| `AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/` | 3287/3322; 35 gagal, semuanya di modul lain (CRM, Rawat Inap, Hemodialisa, Bank Darah, dll.), tidak satu pun Reservasi Prioritas. Jumlahnya sama dengan yang tercatat di laporan `FE-RJP-004` (35 gagal sesudah merge `d1c186a12`) | `UNRELATED EXISTING ISSUE` |
| `npm run test:unit` | Tidak menemukan berkas: glob berpetik `"tests/unit/*.test.mjs"` tidak diperluas di shell Windows. Diganti bentuk folder di atas | `EXISTING / ENVIRONMENT ISSUE` |
| `MANUAL TEST` (Playwright pada `next dev` owner di 3000; HTTP 7184 → backend uji 7185; WebSocket SignalR dijembatani ke 7185 lewat `routeWebSocket`) | 13/13 `PASS`, rincian di bawah | `PASS` |

Data samaran dibuat lewat API admin dan SQL data uji (membership Gold). Akun uji non-SuperAdmin `Read` + `Screen` dibuat
seperti `BE-RJP-013`, lalu kebijakannya dikosongkan dan akunnya dinonaktifkan.

### Uji layar

| ID | Pemeriksaan | Hasil |
| --- | --- | --- |
| BTN | Menunggu Kedatangan sebelum hari H: Ganti Tanggal tampil di samping Kirim Ulang Link | `PASS` |
| `UAT-RP-20` a | Tombol simpan nonaktif sebelum tanggal dan alasan diisi. Sesudah simpan, dialog berganti ke panel link baru (43 karakter); teks WA memuat "sudah diganti" dan link | `PASS` |
| `UAT-RP-20` b | Dialog tertutup; riwayat "Tanggal kunjungan diganti: 22 Oktober 2026 → 25 Oktober 2026"; backend menyimpan tanggal 25 dan riwayat `RESCHEDULED` | `PASS` |
| LINK STATE | Membuka dialog lagi menampilkan isian, bukan link lama | `PASS` |
| `UAT-RP-21` | Reservasi hari ini: Ganti Tanggal tidak tampil, Tandai Tiba tampil | `PASS` |
| `UAT-RP-22` | Membership Kedaluwarsa: pesan backend tampil di dialog dengan tanggal lama, dialog tetap terbuka, tanggal tidak berubah | `PASS` |
| `UAT-RP-23` | Tanggal tujuan sudah punya reservasi: "Pasien sudah punya reservasi prioritas RSV-…-0023 untuk 24 Oktober 2026." di dialog; tanggal tidak berubah | `PASS` |
| CANCEL TEXT | Dialog Batalkan memuat arahan baru | `PASS` |
| STALE 409 | Kirim ulang oleh "petugas lain" saat dialog terbuka: toast "Data berubah", dialog tertutup, detail dimuat ulang, tanggal tetap | `PASS` |
| `R-RP-41` layar | Daftar "Besok" kehilangan baris yang dipindah ke H+5 oleh petugas lain, tanpa muat ulang halaman (`load` 0 kali), tanpa penanda "Pembaruan otomatis terputus" | `PASS` |
| NO CREATE | Akun `Read` + `Screen`: Ganti Tanggal dan Kirim Ulang Link tidak tampil | `PASS` |
| MOBILE | Dialog Ganti Tanggal di 390 px tanpa gulir horizontal halaman | `PASS` |
| CLEANUP | Kebijakan uji 0, akun uji nonaktif | `PASS` |

Percobaan pertama berhenti di `UAT-RP-23` karena skrip menunggu teks dengan regex sementara pesannya sudah tampil. Hal
ini dibuktikan dengan skrip diagnosis yang mencetak isi dialog. Cara menunggu diperbaiki, lalu seluruh uji dijalankan
ulang dari awal dengan data baru: 13/13. Akun uji dari percobaan pertama sudah dibersihkan sebelum run ulang.

Skrip dan tangkapan layar: `ui_fe009.cjs`, `ui13_setup.py`, `ui13_api.py`, `dbg_fe009.cjs`, dan `fe-rjp-009-*.png` di
scratchpad sesi.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `UAT-RP-20` s.d. `23` lewat layar | Terpenuhi | Uji layar |
| `R-RP-41` sisi layar | Terpenuhi | Uji layar dengan kabar realtime asli |
| 409 tampil di dialog, lalu detail dimuat ulang | Terpenuhi: 409 non-basi di dialog (`UAT-RP-22`, `23`); 409 data basi lewat toast dan muat ulang (STALE 409) | Uji layar |
| Tombol tidak tampil tanpa `Create` dan pada hari H | Terpenuhi | NO CREATE, `UAT-RP-21` |
| Link tidak tersisa di state setelah dialog ditutup | Terpenuhi | LINK STATE; uji unit "link tidak disimpan di Redux" |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Wewenang UI | `RJ-PRI-FE-003`, `RJ-PRI-FE-005` (`DEV_DISCRETION`: posisi tombol, bentuk dialog, kalimat) |
| Risiko tersisa | Label detail "Tingkat Member (saat reservasi)" kini berarti "saat reservasi dibuat atau terakhir diganti tanggalnya". Label tidak diubah karena di luar task; dapat dipertimbangkan di revisi UI berikutnya |
| Dependency backend | `BE-RJP-013` ✅. Bila frontend dirilis tanpa backend `1.2.0`, tombol tidak tampil karena `canReschedule` tidak ada |
| Perubahan sampingan | `NONE`. Keluaran `npm run build` (`.next/`, standalone) tidak dilacak Git |
| Status Git | 9 berkas berubah, belum di-commit. Tidak ada stage, commit, push, merge, atau deploy |
| Langkah berikutnya | Review dan commit oleh owner; rilis backend (migration) bersama atau sebelum frontend |

## 8. Rework 21 Oktober 2026 — tanggal lampau dinonaktifkan di kalender (`RJ-PRI-DEC-040`)

Permintaan owner: petugas tidak boleh bisa memilih tanggal sebelum hari ini, jadi tanggal itu dinonaktifkan di kalender.
Sebelumnya tanggal lampau bisa dipilih lalu ditolak backend dengan "Tanggal kunjungan tidak boleh sebelum hari ini."
Perubahan ini juga dipasang di dialog Buat Reservasi (`FE-RJP-004`), karena aturan dan komponennya sama (`VAL-RP-03`).

Contoh: pada 21 Okt 2026, kalender menampilkan 1–20 Okt abu-abu dan tidak dapat diklik. Seluruh hari di bulan September
juga nonaktif. 21 Okt dan sesudahnya, serta tombol "Hari ini", tetap aktif.

### 8.1 Gerbang komponen dasar

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Isian tanggal dengan batas bawah | `BaseDateField` → `FilterDatePicker` | `FilterDatePicker` sudah punya prop opsional `min`/`max` (`filter-date-picker.jsx:414-466`), yang menonaktifkan tombol hari dan menolak pilihan di `selectDate`/`selectToday`. `BaseDateField` (`base-form-control.jsx:500`) belum meneruskannya | `EXTEND` | Teruskan `field.min`/`field.max` |

**Keputusan: batas bawah pada `BaseDateField`**

- **A. Extend `BaseDateField` agar meneruskan `field.min`/`field.max` ke `FilterDatePicker` — Rekomendasi, dipakai.**
  Prop opsional; tanpa nilai, `FilterDatePicker` memakai bawaan kosong (tanpa batas). Ke-18 pemakai `BaseDateField`
  lain diperiksa: tidak satu pun field tanggal mengirim `min`/`max` (grep `type: "date"` dan pemanggil
  `<BaseDateField>`), dan nilai yang bukan tanggal diabaikan `isSafeDateInputValue`. Biaya 2 baris.
- **B. Compose `FilterDatePicker` langsung di dialog dengan shell label sendiri.** Base component tidak berubah, tetapi
  label, keterangan, dan galat dirakit ulang di dua dialog, sehingga rupa bisa menyimpang dari isian lain.

`UI GATE: PASS — satu EXTEND yang tidak mengubah perilaku default base component; elemen lain tetap REUSE.`

### 8.2 Perubahan

| Berkas (repo frontend) | Isi |
| --- | --- |
| `src/components/features/base-features/base-form-control.jsx` | `BaseDateField` meneruskan `min={field.min}`, `max={field.max}` |
| `src/lib/hooks/health-services/registration-management/priority-reservations/use-priority-reservation-detail.jsx` | `minVisitDate` (hari ini, `yyyy-MM-dd`); penjaga cadangan di `confirmReschedule` bila tanggal < hari ini |
| `src/lib/hooks/health-services/registration-management/priority-reservations/use-priority-reservation-create.jsx` | `minVisitDate` (hari ini) |
| `src/lib/constants/health-services/registration-management/priority-reservation-constants.js` | Pesan `PAST_DATE` |
| `src/components/view/health-services/registration-management/priority-reservations/detail/priority-reservation-action-modals.jsx` | Isian tanggal baru memakai `min: minVisitDate` |
| `src/components/view/health-services/registration-management/priority-reservations/priority-reservation-create-modal.jsx` | Isian tanggal kunjungan memakai `min: minVisitDate` |

Batas atas 30 hari tidak dipasang di kalender, karena nilainya konfigurasi backend (`MaxDaysAhead`). Backend tetap menolak
dengan pesan yang menyebut tanggal terakhir (`VAL-RP-04`).

### 8.3 Validasi

| Langkah | Hasil | Klasifikasi |
| --- | --- | --- |
| `npx eslint` pada 6 berkas yang berubah | exit 0 | `PASS` |
| `npm run lint:errors` | exit 0 | `PASS` |
| `npm run build` | exit 0 | `PASS` |
| `AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/priority-reservation-data-layer.test.mjs` | 12/12 | `PASS` |
| `MANUAL TEST` (Playwright pada `next dev` 3000, HTTP 7184 → backend uji 7185) | 3/3, rincian di bawah | `PASS` |

| ID | Pemeriksaan | Hasil |
| --- | --- | --- |
| RESCHEDULE CAL | Dialog Ganti Tanggal: kemarin nonaktif, seluruh hari bulan lalu nonaktif (0 aktif); hari ini, besok, dan tombol "Hari ini" aktif | `PASS` |
| CREATE CAL | Dialog Buat Reservasi: kemarin nonaktif, bulan lalu 0 aktif; hari ini dan besok aktif | `PASS` |
| REGRESI | `BaseDateField` tanpa `min` di IGD Pra-Kedatangan: kemarin tetap aktif, bulan lalu 30 hari aktif | `PASS` |

Penanda "Issues" Next.js di pojok kiri bawah tangkapan layar berasal dari WebSocket SignalR ke 7184 yang tidak
dijembatani pada uji ini. Ini artefak uji, bukan galat fitur. Skrip: `ui_fe009b.cjs`, `ui13b_setup.py`; tangkapan:
`fe-rjp-009b-*.png` di scratchpad sesi. Data uji: satu pasien samaran `RJPR UJI GK CAL…` dengan satu reservasi besok.
