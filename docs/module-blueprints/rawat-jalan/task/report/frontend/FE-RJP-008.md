# Laporan Task Frontend — `FE-RJP-008` Halaman Konfirmasi Kedatangan

Definisi task: [priority-reservation-frontend-roadmap.md](../../../roadmap/priority-reservation-frontend-roadmap.md) bagian `FE-RJP-008`.

## 1. Ringkasan

| Field | Isi |
| --- | --- |
| Task ID | `FE-RJP-008` |
| Slice | `MVP-2`, `EPIC RP-06` |
| Trace | `FR-RP-051`; `RJ-PRI-DEC-012`; `RJ-PRI-FE-001`; `NFR-RP-008`; `R-RP-11`, `R-RP-12`, `R-RP-13`, `UAT-RP-14`, `UAT-RP-15` |
| Kontrak | `RJ-PRI-RESV-001@1.0.0`: Priority Reservation Arrival `GET /{code}` dan `POST /{code}/confirm` (tanpa login, rate limit) |
| Dependency | `BE-RJP-012` ✅ |
| Task mode | `FRONTEND` (`CROSS-REPO MODE`, `RJ-PRI-DEC-031`) |
| Branch / baseline | FE `sukmagpV2` @ `17e0ffec0`, working tree belum di-commit |
| Tanggal | 19 Oktober 2026 |
| Model | Claude Opus 5 |
| Status | ✅ Selesai — uji layar 12/12 + 4/4 `PASS` di jendela tanpa cookie lebar 360 px, lint dan build `PASS`. **Go-live menunggu `RJ-PRI-OQ-003`** dan dicatat terpisah |

## 2. Proses bisnis

Petugas mengirim link lewat WhatsApp, misalnya `https://<alamat-frontend>/priority-arrival/<kode 43 karakter>`.
Pasien membukanya di HP saat tiba di rumah sakit, tanpa login.

| Keadaan | Yang tampil |
| --- | --- |
| Sebelum hari H (`NOT_YET_OPEN`) | Nomor reservasi, tanggal, dan "Konfirmasi kedatangan baru bisa dilakukan pada 20 Oktober 2026." Tanpa tombol |
| Hari H, belum konfirmasi (`READY`) | Nomor reservasi, tanggal, dan tombol **Saya sudah tiba** |
| Sudah konfirmasi (`CONFIRMED`) | "Kedatangan Anda sudah tercatat pukul 14.03 WIB. Silakan menunggu di Ruang Tunggu Prioritas; petugas akan menghampiri Anda." Jam tetap sama walau link dibuka ulang |
| Link dibatalkan, Tidak Datang, sudah diganti, atau kode acak (404) | "Link tidak berlaku. Silakan hubungi petugas pendaftaran." |
| Terlalu sering dicoba (429) | "Terlalu banyak percobaan. Coba lagi sebentar lagi." |
| Jaringan gagal | "Halaman belum dapat dimuat. Periksa koneksi lalu coba lagi." + tombol **Coba lagi** |

Saat pasien menekan **Saya sudah tiba**, reservasi menjadi **Sudah Tiba**. Layar Reservasi Prioritas milik admisi dan
petugas skrining berbunyi dan menampilkan toast "Pasien prioritas tiba".

Halaman ini sengaja hanya memuat nomor reservasi dan tanggal. Nama, No. RM, NIK, tingkat member, poli, dan dokter tidak
pernah tampil (`RJ-PRI-FE-001`), karena siapa pun yang memegang link bisa membukanya.

## 3. Perubahan

| Berkas (repo frontend) | Status | Isi |
| --- | --- | --- |
| `src/app/priority-arrival/layout.jsx` | Baru | Layout tanpa shell aplikasi, `force-dynamic` |
| `src/app/priority-arrival/[code]/page.jsx` | Baru | Entry route; metadata `robots: noindex, nofollow` dan `referrer: no-referrer` |
| `src/components/view/priority-arrival/priority-arrival-view.jsx` | Baru | Kartu logo, judul, isi per keadaan |
| `src/lib/hooks/priority-arrival/use-priority-arrival.jsx` | Baru | Muat keadaan, konfirmasi, coba lagi; 404 dan 429 dipetakan ke keadaan masing-masing; tidak menyimpan apa pun di browser |
| `src/style/priority-arrival/priority-arrival.module.css` | Baru | Tata letak kartu satu kolom ramah HP; seluruh nilai memakai token |
| `src/proxy.js` | Diperbarui | `/priority-arrival` di `PUBLIC_PATH_PREFIXES` |
| `src/components/ui/auth-wrapper.jsx` | Diperbarui | `/priority-arrival` di `PUBLIC_PATH_PREFIXES` |
| `src/components/providers/root-layout-shell.jsx` | Diperbarui | `/priority-arrival` di `SHELLLESS_PATH_PREFIXES` (tanpa sidebar dan header) |
| `src/components/providers/session-activity-manager.jsx` | Diperbarui | `/priority-arrival` di `OPEN_ROUTE_PREFIXES` (tanpa pemantau sesi) |

Fungsi service publik (`getPriorityReservationArrival`, `confirmPriorityReservationArrival`) berasal dari `FE-RJP-003`
dan memakai `_skipAuthRefresh: true`.

## 4. Gerbang komponen dasar

Modul referensi visual: route publik tanpa shell yang sudah ada (kiosk dan queue display) untuk cara mendaftarkan
prefix; isi kartu memakai base component.

| Elemen | Keputusan | Bukti |
| --- | --- | --- |
| Tombol "Saya sudah tiba", "Coba lagi" | `REUSE` `BaseButton` (`loading`, `loadingLabel`, `fullWidth`) | `base-features/base-button` |
| Pesan berhasil, peringatan, galat | `REUSE` `InformationAlert` | `base-features/information-alert` |
| Kartu halaman publik (logo, judul, nomor reservasi) | `COMPOSE` | Lihat pilihan 1 |

**Pilihan 1 — kartu halaman publik**

1. Rangkai kartu sederhana dengan CSS module bertoken + `BaseButton` + `InformationAlert` (Direkomendasikan, dipakai) —
   halaman publik tidak boleh memuat shell aplikasi, dan isi kartunya hanya tiga baris. Tanpa komponen baru yang
   dibagikan.
2. Pakai `Hero` + `BaseDetailView` — konsisten dengan layar petugas, tetapi membawa tata letak lebar layar desktop dan
   elemen yang tidak relevan bagi pasien di HP 360 px.

`UI GATE: PASS — REUSE dan COMPOSE saja; tidak ada NEW atau EXTEND.`

### Checklist konsistensi UI

| Pemeriksaan | Hasil |
| --- | --- |
| Warna literal di `priority-arrival.module.css` | Tidak ada |
| Typography | Hanya pada elemen milik halaman ini (judul, nomor, pesan) memakai token `--font-size-*`, `--font-weight-*`, `--line-height-*`; tidak menimpa komponen shared |
| `<button>` mentah, `<table>`, `!important`, `prefers-color-scheme`, inline style | Tidak ada |
| Lebar 360 px | Tanpa gulir horizontal (diuji) |

## 5. Validasi

| Langkah | Hasil |
| --- | --- |
| `npm run lint:errors` | exit 0 |
| `npm run build` | exit 0 (build akhir 19 Oktober 2026) |
| `AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/priority-reservation-data-layer.test.mjs` | `PASS` 7/7, termasuk `_skipAuthRefresh` pada endpoint publik |
| `AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/*.test.mjs` | 2976/3003; 27 gagal di modul lain, sudah gagal pada snapshot `HEAD` |
| `MANUAL TEST` (Playwright, jendela baru tanpa cookie, lebar 360 × 740, build standalone port 3000, backend uji 7185) | `PASS` 12/12 + 4/4, rincian di bawah |

### Uji layar — jendela tanpa cookie, 360 px

| ID | Pemeriksaan | Hasil |
| --- | --- | --- |
| **`R-RP-11`** belum dibuka | Reservasi besok: nomor + tanggal tampil, tanpa tombol "Saya sudah tiba" | `PASS` |
| **`R-RP-13`** privasi | Tanpa nama, No. RM, tingkat, poli, dokter; tanpa sidebar, menu, atau header akun | `PASS` |
| 360px | Tanpa gulir horizontal | `PASS` |
| **`R-RP-11`** konfirmasi | Hari H: tombol tampil; klik → pesan berhasil, tombol hilang, reservasi Sudah Tiba (`status=2`) | `PASS` |
| **`R-RP-11`** buka ulang | Link terkonfirmasi dibuka lagi: keadaan CONFIRMED, tanpa tombol | `PASS` |
| **`R-RP-11`** jam tetap | Dibuka dua kali: jam konfirmasi pertama sama ("pukul 14.15 WIB") | `PASS` |
| **`R-RP-12`** kode acak | "Link tidak berlaku. Silakan hubungi petugas pendaftaran." | `PASS` |
| **`R-RP-12`** batal | Link yang masih berlaku menjadi "Link tidak berlaku" sesudah reservasi dibatalkan | `PASS` |
| INVALID old | Kode lama reservasi yang sudah diterbitkan ulang lalu dibatalkan: link tidak berlaku | `PASS` |
| **`UAT-RP-15`** | Sesudah kirim ulang: link lama "Link tidak berlaku", link baru siap dikonfirmasi | `PASS` |
| 429 | Respons tiruan 429: "Terlalu banyak percobaan. Coba lagi sebentar lagi." | `PASS` |
| RETRY | Jaringan diputus: pesan gagal + "Coba lagi"; sesudah jaringan pulih dan diklik, halaman termuat | `PASS` |
| NO AUTH | Hanya endpoint publik `priority-reservation-arrivals` yang dipanggil (9 panggilan); tanpa auth, refresh, atau menu | `PASS` |
| NO STORAGE | Kode link tidak ada di `localStorage`, `sessionStorage`, atau cookie | `PASS` |
| URL | Alamat tetap `/priority-arrival/…`, tidak dialihkan ke login | `PASS` |
| **`UAT-RP-14`** | Pasien menekan "Saya sudah tiba" di HP: reservasi Sudah Tiba; layar admisi menampilkan toast, baris berubah tanpa muat ulang, bunyi dipicu (`plays=1`) | `PASS` |

`R-RP-12` untuk reservasi **Tidak Datang** tidak diuji di layar, karena kode link reservasi Tidak Datang yang ada hanya
tersimpan sebagai hash. Halaman memperlakukan setiap 404 sama, dan backend sudah membuktikan isi 404 untuk Tidak Datang
identik dengan kode acak (`BE-RJP-012`). `R-RP-13` sisi respons API juga dibuktikan di `BE-RJP-012`.

Tangkapan layar: `fe-rjp-008-not-yet-open.png`, `fe-rjp-008-ready.png`, `fe-rjp-008-confirmed.png`,
`fe-rjp-008-invalid.png`, `fe-rjp-008-cancelled.png`, `fe-rjp-008-failed.png`, `fe-rjp-008-admission-bell.png` di
scratchpad sesi.

## 6. Delta dan risiko

| Butir | Catatan |
| --- | --- |
| Daftar prefix publik | Kartu task menyebut dua daftar (`src/proxy.js`, `auth-wrapper.jsx`). Agar halaman benar-benar tanpa shell dan tanpa pemantau sesi, prefix juga ditambahkan ke `root-layout-shell.jsx` dan `session-activity-manager.jsx`. Totalnya empat daftar |
| Kode di URL | Kode link ada di path. Backend tidak mencatat path ini ke log (`BE-RJP-012`). Halaman memakai `referrer: no-referrer` agar kode tidak bocor lewat header Referer |
| **Go-live** | **Menunggu `RJ-PRI-OQ-003`.** Fitur sudah bisa diuji di lingkungan dev; peluncuran ke pasien dicatat terpisah sesudah pertanyaan itu ditutup |

## 7. Wewenang dan batas

- Isi halaman mengikat (`RJ-PRI-FE-001`): hanya nomor reservasi, tanggal, tombol, dan pesan keadaan.
- Suntingan user yang belum di-commit di `emergency-registration/patient-selection-step.jsx` tidak disentuh.
- Tidak ada stage, commit, push, merge, atau deploy.
