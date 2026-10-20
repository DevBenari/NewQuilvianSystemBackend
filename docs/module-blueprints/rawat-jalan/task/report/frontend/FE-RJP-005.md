# Laporan Task Frontend — `FE-RJP-005` Detail dan aksi reservasi

Definisi task: [priority-reservation-frontend-roadmap.md](../../../roadmap/priority-reservation-frontend-roadmap.md) bagian `FE-RJP-005`.

## 1. Ringkasan

| Field | Isi |
| --- | --- |
| Task ID | `FE-RJP-005` |
| Slice | `MVP-1`, `EPIC RP-04`, `RP-06` (sisi petugas) |
| Trace | `FR-RP-033` s.d. `035`, `038`, `052`; `RJ-PRI-DEC-012`, `014`, `019`, `022`, `023`, `026`, `028`, `029`; `R-RP-14`; `VAL-RP-14` |
| Kontrak | `RJ-PRI-RESV-001@1.0.0`: `GET /{id}`, `POST /{id}/arrival`, `POST /{id}/arrival-reversal`, `PUT /{id}/screening`, `POST /{id}/arrival-code`, `POST /{id}/cancel`; hub `/hubs/queues` (`JoinPriorityReservationBoard`, `PriorityReservationChanged`) |
| Dependency | `FE-RJP-004` ✅, `BE-RJP-007` ✅, `BE-RJP-010` ✅ |
| Task mode | `FRONTEND` (`CROSS-REPO MODE`, `RJ-PRI-DEC-031`) |
| Branch / baseline | FE `sukmagpV2` @ `17e0ffec0`, working tree belum di-commit |
| Tanggal | 19 Oktober 2026 |
| Model | Claude Opus 5 |
| Status | ✅ Selesai — uji layar 9/9 + 2/2 `PASS` dengan dua akun (admin dan petugas skrining), lint dan build `PASS`; rework 19 Okt 2026 (`RJ-PRI-DEC-032`, warna tombol) 7/7 `PASS` (bagian 8) |

## 2. Proses bisnis

Petugas membuka detail reservasi dengan klik dua kali baris di daftar. Detail berisi kepala (nomor, status, pasien,
tanggal, tingkat), checklist skrining, dan riwayat. Tombol aksi hanya tampil bila backend menyatakan aksi itu mungkin
(`can*`) **dan** pengguna punya izinnya.

| Status reservasi | Tombol yang tampil (akun lengkap) |
| --- | --- |
| Menunggu Kedatangan | Tandai Tiba, Kirim Ulang Link, Batalkan |
| Sudah Tiba | Skrining, Lengkapi Pendaftaran, Kembalikan ke Belum Tiba, Batalkan |
| Menjadi Kunjungan, Dibatalkan, Tidak Datang, Jalur Lain | Tidak ada aksi |

Contoh kejadian di lapangan:

1. Pasien Gold menekan link di HP. Layar daftar milik admisi dan petugas skrining berbunyi, toast "Pasien prioritas
   tiba" muncul, dan baris berubah menjadi **Sudah Tiba** tanpa muat ulang manual.
2. Petugas skrining mengisi Batuk, Demam, dan Risiko jatuh. Bila salah satu kosong, dialog menolak dengan
   "Isi ketiga pertanyaan skrining".
3. Dua petugas membuka reservasi yang sama. Petugas A mengembalikan ke Belum Tiba. Petugas B yang masih membuka dialog
   skrining menekan Simpan dan mendapat toast "baru saja diubah petugas lain". Dialog tertutup dan detail dimuat ulang.
4. Bila koneksi realtime putus, daftar dimuat ulang otomatis setiap 60 detik sampai koneksi pulih.

Kembalikan ke Belum Tiba dan Batalkan wajib memakai alasan 10–500 karakter. Kirim Ulang Link menerbitkan link baru;
link lama langsung tidak berlaku.

## 3. Perubahan

| Berkas (repo frontend) | Status | Isi |
| --- | --- | --- |
| `src/app/health-services/registration-management/priority-reservations/[id]/page.jsx` | Baru | Entry route detail |
| `src/components/view/health-services/registration-management/priority-reservations/detail/priority-reservation-detail-view.jsx` | Baru | `BaseDetailView` + `renderDetailActions`, tabel riwayat, toast, `AccessDeniedGate` |
| `src/components/view/health-services/registration-management/priority-reservations/detail/priority-reservation-action-modals.jsx` | Baru | Lima dialog: Tandai Tiba, Kembalikan (alasan), Skrining (tiga isian), Kirim Ulang Link (lalu panel link), Batalkan (alasan) |
| `src/lib/hooks/health-services/registration-management/priority-reservations/use-priority-reservation-detail.jsx` | Baru | Muat detail, jalankan aksi dengan `expectedRowVersion`, tangani 409 (toast + muat ulang), izin per tombol |
| `src/lib/hooks/health-services/registration-management/priority-reservations/use-priority-reservation-realtime.jsx` | Baru | Gabung/keluar board selama layar terbuka, status putus |
| `src/lib/hooks/health-services/registration-management/priority-reservations/use-priority-reservation-list.jsx` | Baru (bersama `FE-RJP-004`) | Bunyi + toast saat event `ARRIVED`, kabar beruntun digabung, cadangan muat ulang 60 detik |

## 4. Gerbang komponen dasar

Modul referensi visual: halaman detail master data yang memakai `BaseDetailView` + `renderDetailActions`
(mis. `administrator-kiosk-device-detail-view.jsx`).

| Elemen | Keputusan | Bukti |
| --- | --- | --- |
| Kerangka detail + baris tombol | `REUSE` `BaseDetailView` (`renderDetailActions`), `BaseButton` | `base-features/base-detail-view`; prop sudah dipakai di detail kiosk device, chart of account |
| Lencana status | `REUSE` `StatusBadge` | `base-features/status-badge` |
| Tabel riwayat | `REUSE` `DataTable` | `base-features/data-table` |
| Toast hasil aksi dan 409 | `REUSE` `ToastStack` | `base-features/toast-stack` |
| Dialog Tandai Tiba | `REUSE` `ConfirmModal` | `base-features/confirm-modal` |
| Dialog dengan alasan (Kembalikan, Batalkan) | `REUSE` `ConfirmModal` (`requireReason`, `reasonLabel`, `maxReasonLength`) | Prop alasan bawaan `confirm-modal.jsx`; batas 10–500 karakter dari `PRIORITY_RESERVATION_LIMITS` |
| Dialog skrining | `COMPOSE` | Lihat pilihan 1 |
| Dialog Kirim Ulang Link | `COMPOSE` `ConfirmModal` + panel hasil terbit dari `FE-RJP-004` | Sama dengan pilihan 2 di laporan `FE-RJP-004` |

**Pilihan 1 — dialog skrining**

1. `ConfirmModal` berisi tiga `BaseNativeSelectField` (Ya/Tidak) dalam grid tiga kolom yang turun ke satu kolom di
   layar sempit (Direkomendasikan, dipakai) — isian base component, rupa sama dengan dialog lain.
2. Form skrining penuh di halaman Pendaftaran — konsisten dengan form panjang, tetapi skrining awal hanya tiga
   pertanyaan dan harus bisa diisi petugas skrining tanpa hak pendaftaran.

`UI GATE: PASS — elemen REUSE dan COMPOSE saja; tidak ada NEW, dan tidak ada EXTEND yang mengubah perilaku default base component.`

Checklist konsistensi UI: tidak ada warna literal, `<button>` mentah, `<table>` mentah, `!important`, atau inline
style statis. Stylesheet bersama `priority-reservation.module.css` hanya mengatur tata letak (`grid`, `gap`) dengan token.

## 5. Validasi

| Langkah | Hasil |
| --- | --- |
| `npm run lint:errors` | exit 0 |
| `npm run build` | exit 0 (build akhir 19 Oktober 2026) |
| `AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/priority-reservation-data-layer.test.mjs` | `PASS` 7/7, termasuk 409 `VAL-RP-14` |
| `AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/*.test.mjs` | 2976/3003; 27 gagal di modul lain, sudah gagal pada snapshot `HEAD` |
| `MANUAL TEST` (Playwright, build standalone port 3000, backend uji 7185, WebSocket lewat proxy TCP sementara 7184 → 7185) | `PASS` 9/9 + 2/2, rincian di bawah |

### Uji layar — admin dan petugas skrining

| ID | Pemeriksaan | Hasil |
| --- | --- | --- |
| BTN awaiting | Menunggu Kedatangan: Tandai Tiba, Kirim Ulang, Batalkan tampil; Skrining, Lengkapi, Kembalikan tidak | `PASS` |
| ARRIVAL | Tandai tiba: toast, status Sudah Tiba; tombol Skrining, Lengkapi, Kembalikan, Batalkan tampil | `PASS` |
| SCREENING | Isian kurang ditolak di dialog; tersimpan "Batuk: Ya · Demam: Tidak · Risiko jatuh: Ya" | `PASS` |
| **STALE 409** | Data diubah lewat API saat dialog skrining terbuka: toast "baru saja diubah petugas lain", dialog tertutup, detail dimuat ulang ke Menunggu Kedatangan | `PASS` |
| HISTORY | Riwayat memuat Reservasi dibuat, Pasien tiba, Skrining awal disimpan, Dikembalikan ke Belum Tiba | `PASS` |
| REISSUE | Kirim ulang: link baru 43 karakter tampil dan berbeda dari link lama | `PASS` |
| CANCEL | Alasan "pendek" ditolak di layar; sesudah batal status Dibatalkan dan seluruh tombol aksi hilang | `PASS` |
| **`R-RP-14` layar** | Akun petugas skrining (`Read` + `Screen` saja): pasien konfirmasi lewat link, toast "Pasien prioritas tiba", baris berubah Sudah Tiba tanpa muat ulang, bunyi dipicu (`plays=1`) | `PASS` |
| BTN screener | Akun `Read` + `Screen`: di detail Sudah Tiba hanya tombol Skrining yang tampil | `PASS` |
| FALLBACK 60s | Koneksi hub diblokir: daftar dimuat ulang otomatis dalam 65 detik (1 → 2 request) | `PASS` |
| NO POLL | Koneksi hub normal: tidak ada muat ulang berkala dalam 65 detik (1 → 1 request) | `PASS` |

Bunyi diuji dengan mengganti `window.Audio.play` di browser uji dan menghitung pemanggilannya. Uji `UAT-RP-14` di
laporan `FE-RJP-008` membuktikan alur yang sama dari klik tombol di halaman publik.

Tangkapan layar: `fe-rjp-005-screening.png`, `fe-rjp-005-detail.png`, `fe-rjp-005-realtime.png` di scratchpad sesi.

## 6. Delta dan risiko

| Butir | Catatan |
| --- | --- |
| Bentuk detail | Route anak `priority-reservations/[id]`, bukan drawer. `DEV_DISCRETION` (`RJ-PRI-FE-003`) |
| Bunyi | Browser baru mengizinkan bunyi sesudah pengguna berinteraksi dengan halaman. Bila petugas belum pernah mengklik apa pun sejak membuka layar, bunyi pertama bisa tertahan; toast dan perubahan baris tetap tampil |
| Kabar realtime | Hanya pemicu muat ulang, bukan sumber data. Data selalu dibaca ulang dari API |

## 7. Wewenang dan batas

- Tombol tampil hanya bila `can*` dari backend benar **dan** izin pengguna ada. Backend tetap menolak aksi tanpa izin.
- Suntingan user yang belum di-commit di `emergency-registration/patient-selection-step.jsx` tidak disentuh.
- Tidak ada stage, commit, push, merge, atau deploy.

## 8. Rework 19 Oktober 2026 — warna tombol (`RJ-PRI-DEC-032`)

Owner meminta semua tombol primer yang masih hijau memakai warna primer seperti modul lain. Di halaman detail, tombol
"Tandai Tiba" sebelumnya `BaseButton variant="success"` (hijau). Kini `variant="primary"`, sama dengan tombol
"Skrining" dan "Lengkapi Pendaftaran". Tombol konfirmasi di dalam dialog (`ConfirmModal`) tidak diubah karena sudah
memakai gaya primer bersama `region-btn-primary` yang berlaku di semua modul.

Sejak rework `FE-RJP-004`, tombol "Lengkapi Pendaftaran" hanya ada di halaman detail ini. Kolom Aksi di daftar sudah
dihapus, dan detail dibuka dengan klik dua kali baris.

| Berkas (repo frontend) | Status | Isi |
| --- | --- | --- |
| `src/components/view/health-services/registration-management/priority-reservations/detail/priority-reservation-detail-view.jsx` | Diperbarui | "Tandai Tiba" `variant="primary"` |

`UI GATE: PASS — REUSE BaseButton varian primer; tidak ada NEW atau EXTEND.`

| Langkah | Hasil |
| --- | --- |
| `npm run lint:errors` / `npm run build` | exit 0 / exit 0 |
| `MANUAL TEST` bagian admin (Playwright pada `next dev` 3000, API → 7185, reservasi uji baru `RSV-20261019-0072`/`0073`) | `PASS` 7/7: tombol sesuai status, tandai tiba, skrining, 409 data basi, riwayat, kirim ulang, batal |
| Kelas tombol "Tandai Tiba" (uji `FE-RJP-004` rework, `PRIMARY`) | `PASS` — kelas `primary`, bukan `success` |
| Bagian akun petugas skrining (`R-RP-14`) | Tidak diulang. Kodenya tidak berubah, dan hasil 2026-10-19 sebelumnya tetap berlaku (bagian 5) |

### 8.1 Lanjutan 20 Oktober 2026 — tombol konfirmasi dialog

Tombol konfirmasi primer di dialog Tandai Tiba ("Ya, Tandai Tiba"), Skrining ("Simpan Skrining"), dan Link Baru Diterbitkan
("Selesai") kini memakai kelas `primary` milik `BaseButton` lewat prop `confirmButtonClassName`, menggantikan
`region-btn-primary` hijau tua bawaan `ConfirmModal`. Dialog Kirim Ulang Link (peringatan), Kembalikan, dan Batalkan tetap
memakai warna bawaannya. Rinciannya di laporan `FE-RJP-004` bagian 8.7. Berkas: `detail/priority-reservation-action-modals.jsx`,
konstanta di `priority-reservation-modal-buttons.js`. `npm run lint:errors` dan `npm run build` exit 0.

### 8.2 Lanjutan 20 Oktober 2026 — tombol "Kirim Ulang Link"

Owner melaporkan tombol "Kirim Ulang Link" di hero detail tampak seperti teks biasa. Penyebabnya: varian `ghost` (transparan
tanpa bingkai) di atas latar hero gelap. Tombol kini `BaseButton variant="secondary"`, sama dengan "Kembali" dan aksi
pendukung di halaman detail lain (kiosk device, chart of account).

| Berkas (repo frontend) | Status | Isi |
| --- | --- | --- |
| `src/components/view/health-services/registration-management/priority-reservations/detail/priority-reservation-detail-view.jsx` | Diperbarui | "Kirim Ulang Link" `variant="secondary"`, dan atas permintaan owner dipindah tepat di samping "Kembali". Urutan hero: Kembali, Kirim Ulang Link, Lengkapi Pendaftaran, Tandai Tiba, Skrining, Kembalikan ke Belum Tiba, Batalkan (yang tampil mengikuti status dan izin) |

`UI GATE: PASS — REUSE varian BaseButton yang ada.`

| Langkah | Hasil |
| --- | --- |
| `npx eslint --quiet` (folder fitur) | exit 0 |
| `MANUAL TEST` di stack owner, detail `RSV-20261019-0002` (Menunggu Kedatangan) | `PASS` — tombol berlatar putih, bingkai `rgba(8, 155, 171, 0.16)`, teks gelap; klik membuka dialog "Kirim Ulang Link?" (dibatalkan, tidak ada link baru terbit) |

Tangkapan layar: `reissue-hero-full.png` di scratchpad sesi.

### 8.3 Lanjutan 20 Oktober 2026 — tombol "Skrining" dihapus (`RJ-PRI-DEC-033`)

Atas permintaan owner, tombol "Skrining" dihapus dari hero detail supaya petugas langsung ke "Lengkapi Pendaftaran".
Pelengkapan tidak pernah menunggu skrining (`RJ-PRI-DEC-019`), jadi alur pendaftaran tidak terhalang. Backend tidak diubah:
endpoint `PUT /{id}/screening`, kolom data, dan izin `Screen` tetap ada, sehingga tombol bisa dikembalikan tanpa migration.

Dampak yang perlu diketahui:

1. Checklist skrining awal (`FR-RP-034`, `RJ-PRI-DEC-019`) tidak lagi bisa diisi dari layar. Bagian frontend `FR-RP-034`
   ditarik oleh keputusan owner; skenario `SCREENING`, `STALE 409` (melalui dialog skrining), dan `BTN screener` pada bagian 5
   tidak berlaku lagi untuk layar.
2. Akun yang hanya punya `Read` + `Screen` tidak lagi punya aksi di detail. Bunyi dan toast "Pasien prioritas tiba" (`R-RP-14`)
   tetap berjalan di daftar.
3. Baris "Skrining Awal" di detail dan kolom "Skrining" di daftar masih tampil, dan akan selalu "Belum" untuk reservasi baru.

| Berkas (repo frontend) | Status | Isi |
| --- | --- | --- |
| `src/components/view/health-services/registration-management/priority-reservations/detail/priority-reservation-detail-view.jsx` | Diperbarui | Tombol "Skrining" dihapus dari `renderDetailActions` |

| Langkah | Hasil |
| --- | --- |
| `npx eslint --quiet` (folder detail) | exit 0 |
| `MANUAL TEST` di stack owner, detail `RSV-20261020-0008` (Sudah Tiba) | `PASS` — tombol: Kembali, Lengkapi Pendaftaran, Kembalikan ke Belum Tiba, Batalkan; tanpa Skrining |
