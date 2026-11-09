# Laporan Perubahan Frontend — `FE-NV-011`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-NV-011` |
| Judul | Layar tablet: panggil perawat, Darurat, batal, perawat hadir |
| Slice | `MVP-8` — `EPIC NV-06` (gelombang frontend 2, revision 2) |
| Roadmap | `docs/module-blueprints/nurse-view/roadmap/frontend-roadmap.md` — bagian Revision 2, kartu `FE-NV-011` |
| Trace | `FR-NV-037`, `038`, `040`, `042`, `045`; `NV-DEC-037`–`039`, `041`, `046`, `050`, `054`; `NV-FE-009`, `010`; `G-09`, `G-13`; state §6; `03-frontend-architecture.md` R2.3 `FE-NVW-07`, R2.5, R2.9 |
| Contract version | `0.2.1` `approved` (`NV-DEC-057`); api §6 dan §10 tidak berubah sejak `0.2.0` |
| Wewenang UI | `NV-FE-010` (ukuran, warna, susunan) dengan syarat Cakupan (1) dan (4); `NV-FE-009` (cadangan ≤ 30 detik saat ada panggilan) |
| Dependency | `BE-NV-011` ✅, `FE-NV-009` 🟡, `FE-NV-010` 🟡 (dikerjakan di sesi yang sama) |
| Klasifikasi | `MEDIUM` — skor 7: satu repository (0), berkas diperiksa > 20 (2), berkas diubah 9 (2), logika kompleks — kirim ulang idempoten, tahan 3 detik (2), kontrak yang sudah ada (1), UI satu layar (0) |
| Task mode | `FRONTEND` — perintah Rizki 9 Nov 2026 |
| Target tulis | `src/components/view/nurse-view-bed-tablet/`, `src/lib/hooks/nurse-view-bed-tablet/`, `src/lib/services/nurse-view-bed-tablet/`, `src/utils/nurse-view-bed-tablet/`, `src/style/nurse-view-bed-tablet/` |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `7d32e7cec` (branch `RizkiV2`) |
| Commit backend yang dijadikan rujukan | `589fb513` + `BE-NV-011`–`014` belum di-commit (branch `rizkiG`) |
| Tanggal | 2026-11-09 |
| Status | 🟡 **SEBAGIAN 9 November 2026** — AC 6/6 terpetakan ke source; `npx eslint` 0 error, 0 warning; UI gate tanpa elemen `NEW`. Belum: uji layar ukuran tablet termasuk memutus jaringan, ditunda atas permintaan Rizki |

---

## 1. Keadaan yang ditemukan di awal

Backend tablet sudah lengkap (`BE-NV-010`, `011`; sinyal `BE-NV-014`). Belum ada layar tablet;
route `/nurse-view-bed-tablet` dibuat di `FE-NV-009` pada sesi yang sama.

## 2. Proses bisnis dari sisi pengguna

**Contoh normal.** Pasien 326-C butuh bantuan.

1. Layar `Siap` menampilkan dua tombol besar: **Panggil perawat** (biru, "Untuk kebutuhan biasa")
   dan **Darurat** (merah, "Tekan bila butuh pertolongan segera"), berjarak lebar.
2. Pasien menekan Panggil perawat. Tablet membuat satu `Idempotency-Key` dan mengirim `POST /calls`.
3. Layar berubah: "Panggilan terkirim · menunggu 0:12", tombol **Batal** dan **Darurat**.
4. Sari mengklaim di konsol. Tablet menerima sinyal dan langsung menampilkan "Sari menuju"; Batal
   hilang.
5. Sari tiba dan menahan area paling bawah **"Perawat hadir — tahan 3 detik"** sampai bilah hijau
   penuh. Panggilan selesai dan layar kembali `Siap`.

**Jalur tidak normal:**

| Kejadian | Yang dilihat pengguna |
| --- | --- |
| Wi-Fi putus saat menekan | Pita merah "Panggilan belum terkirim — tekan bel di dinding · Mengirim ulang otomatis..." Kirim ulang 2, 4, 8, lalu tiap 15 detik dengan kunci yang **sama**, dan langsung saat `online` |
| Jawaban server lambat lebih dari 4 detik | Pita merah yang sama sambil permintaan tetap ditunggu |
| Menekan Darurat saat panggilan biasa sedang dikirim ulang | Pengiriman biasa dihentikan, Darurat dikirim dengan kunci baru (server menaikkan atau membuat panggilan Darurat) |
| Area perawat dilepas sebelum 3 detik | Tidak ada yang dikirim; bilah kembali kosong |
| Batal atau perawat hadir mendapat `409` | Keadaan dimuat ulang tanpa pesan |
| Bed kosong sementara panggilan masih aktif | "Bed belum berpenghuni" di atas status; tombol panggil hilang; area perawat tetap ada |
| Muat ulang gagal | Pita kuning "Koneksi terputus — gunakan bel di dinding" |
| `401`/`403` | Layar login |

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`base-button.jsx` (varian dan ukuran), katalog base component, `use-nurse-view-floor-board.jsx`
dan view papan TV (modul rujukan visual: layar perangkat tanpa sidebar), `globals.css` token,
`NvwBedTabletRuntimeController`, `NvwBedTabletRuntimeDtos`, `NvwPatientCallService` (perilaku
tekan, batal, perawat hadir, `Idempotency-Key`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/nurse-view-bed-tablet/nurse-view-bed-tablet-view.jsx` | **Baru.** Kepala, pita gagal kirim/terputus, keadaan `BedKosong`, `Siap`, panggilan aktif |
| `…/nurse-view-bed-tablet-call-button.jsx` | **Baru.** WRAP di atas `BaseButton` (varian, `disabled`, `aria-label`); teks besar di elemen anak |
| `…/nurse-view-nurse-arrived-area.jsx` | **Baru.** WRAP di atas `BaseButton variant="ghost"` dengan bilah progres |
| `src/lib/hooks/nurse-view-bed-tablet/use-nurse-view-bed-tablet.jsx` | **Baru.** Muat `GET /current`; selisih jam server; realtime grup tablet; cadangan sesuai `RefreshIntervalSeconds` (15/60, batas 60); tekan dengan kirim ulang idempoten; batal; perawat hadir; `401`/`403` → logout |
| `…/use-nurse-view-hold-action.jsx` | **Baru.** Tahan 3 detik lewat pointer dan Spasi/Enter; dilepas lebih awal → batal |
| `src/lib/services/nurse-view-bed-tablet/nurse-view-bed-tablet-service.jsx` | **Baru.** Empat panggilan `InstanceAxios`; header `Idempotency-Key` |
| `src/utils/nurse-view-bed-tablet/nurse-view-bed-tablet-utils.jsx` | **Baru.** Pembuat kunci (`crypto.randomUUID` + cadangan), klasifikasi galat, jeda kirim ulang, durasi, keadaan layar |
| `src/style/nurse-view-bed-tablet/nurse-view-bed-tablet.module.css` | **Baru.** Hanya token; ukuran huruf mengikuti layar lewat `--tablet-unit` (pola TV `--board-unit`) |

### 3.3 Kepatuhan arsitektur frontend

Alur `app` → `components/view` → `hooks` → `services` → `InstanceAxios`. Tanpa Redux untuk data
layar (pola TV); Redux hanya untuk `LogoutUser`. `globals.css` tidak diubah. Lama menunggu dihitung
dari `FirstPressedAt` dan selisih `ServerTimeUtc`, bukan jam tablet.

**Tabel keputusan base component:**

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Layar memuat dan mengalihkan | `LoadingScreen` | `features/loading/loadingScreen.jsx` | `REUSE` | Prop `text` |
| Tombol Panggil perawat dan Darurat | `BaseButton` | Ukuran hanya `sm`/`md`/`lg`; typography base tidak boleh ditimpa CSS fitur | `WRAP` | `BaseButton` untuk semantik dan warna varian; teks besar di elemen anak milik view |
| Tombol Batal | `BaseButton` | `variant="secondary"` | `WRAP` | Sama dengan baris di atas |
| Area "Perawat hadir — tahan 3 detik" | `BaseButton` | Tidak ada base untuk tekan-tahan | `WRAP` | `BaseButton variant="ghost"` + hook tahan + bilah progres |
| Kepala bed dan kamar | — | Preseden kepala TV `FE-NV-005` | `COMPOSE` | CSS module bertoken |
| Pita gagal kirim / terputus | `InformationAlert` | Bisa ditutup dan berukuran halaman staf | `COMPOSE` | Pita tetap tampil sampai berhasil (keselamatan); preseden pita TV |
| Status dan lama menunggu | — | Preseden kartu TV | `COMPOSE` | CSS module bertoken |

`UI GATE: 7 elemen — REUSE 1, EXTEND 0, COMPOSE 3, WRAP 3, NEW 0`

**Keputusan: tombol besar layar tablet**

- **A. WRAP `BaseButton` dengan teks besar di elemen anak — Rekomendasi (dijalankan).** Varian
  warna, `disabled`, dan `aria-busy` tetap dari base; typography base tidak ditimpa. Biaya kecil,
  tanpa risiko ke modul lain.
- **B. Extend `BaseButton` dengan ukuran baru `xl`.** Semua modul bisa memakainya, tetapi ukuran
  tablet bergantung pada tinggi layar dan tidak cocok menjadi ukuran umum; menyentuh base
  component bersama.
- **C. `<button>` baru khusus tablet.** Bebas secara visual, tetapi menduplikasi kontrak tombol
  (`disabled`, fokus, varian). Perlu persetujuan eksplisit.

**Keputusan: area perawat tahan 3 detik**

- **A. WRAP `BaseButton variant="ghost"` + hook tahan — Rekomendasi (dijalankan).** Terlihat jelas
  berbeda dari tombol pasien (garis putus-putus, letak paling bawah, huruf lebih kecil).
- **B. Elemen `div` dengan peran tombol.** Butuh aksesibilitas dikerjakan ulang; tidak ada
  keuntungan.

**Keputusan: pita gagal kirim dan terputus**

- **A. Compose pita bertoken — Rekomendasi (dijalankan).** Tidak bisa ditutup pasien, berhuruf
  besar, dan mengikuti preseden pita TV `FE-NV-005`.
- **B. `InformationAlert` apa adanya.** Konsisten dengan halaman staf, tetapi huruf terlalu kecil
  untuk pasien di bed dan alert bisa ditutup.

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| `Memuat` | "Memuat layar tablet..." |
| `BedKosong` | "Bed belum berpenghuni"; tombol panggil tidak ada |
| `Siap` | Dua tombol besar |
| `Menunggu` | "Panggilan terkirim · menunggu m:ss" atau "Panggilan DARURAT terkirim"; Batal bila `CanCancel`; Darurat bila `CanEscalate`; area perawat |
| `Diklaim` | "‹nama› menuju"; area perawat |
| `GagalKirim` | Pita merah "Panggilan belum terkirim — tekan bel di dinding" |
| `Terputus` | Pita kuning "Koneksi terputus — gunakan bel di dinding" |
| `Ditolak` | Layar login |

## 5. Endpoint yang dikonsumsi

#### Health Services / Nurse View Management / Bed Tablet Runtime

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/nurse-view-management/bed-tablet-runtime/current` | Keadaan layar | Policy `NurseViewBedTabletAccess` |
| `POST` | `…/bed-tablet-runtime/calls` | Panggil perawat / Darurat, header `Idempotency-Key` | idem |
| `POST` | `…/bed-tablet-runtime/calls/current/cancel` | Batal | idem |
| `POST` | `…/bed-tablet-runtime/calls/current/nurse-arrived` | Perawat hadir | idem |

Realtime: `JoinNurseViewBedTablet` dan event `NurseViewPatientCallChanged` (`FE-NV-010`).

## 6. Verifikasi

| Pemeriksaan | Hasil |
| --- | --- |
| `npx eslint` seluruh berkas tablet | 0 error, 0 warning |
| Grep anti-regresi (`ui-consistency-checklist` G) | Warna literal 0; `<button>` mentah 0; `<table>` 0; utility Bootstrap 0; `!important` 0. Typography: 36 deklarasi, semuanya pada elemen milik view (kelas yang dipasang ke `BaseButton` hanya berisi tata letak). Inline style: 1, lebar bilah progres yang dinamis |
| Uji layar ukuran tablet dan putus jaringan | `NOT RUN` — ditunda atas permintaan Rizki |

`AUTOMATED TEST: SKIPPED (opsional) — repository tidak memakai Jest.`
`MANUAL TEST: NOT FEASIBLE — uji ditunda atas permintaan Rizki; backend BE-NV-014 juga belum dibangun ulang.`

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC-1 bed berpenghuni → `Siap` dua tombol, tanpa nama pasien | 🟡 Terpetakan ke source | `resolveTabletScreen`; `GET /current` tanpa identitas |
| AC-2 Wi-Fi diputus → pesan bel; tersambung lagi → tepat satu panggilan | 🟡 Terpetakan ke source | Kirim ulang dengan kunci sama; `online` memicu kirim ulang |
| AC-3 dilepas < 3 detik → tetap aktif; 3 detik → selesai | 🟡 Terpetakan ke source | `use-nurse-view-hold-action` |
| AC-4 Batal hanya untuk biasa `Menunggu` | 🟡 Terpetakan ke source | `CanCancel` dari backend |
| AC-5 bed kosong → tombol hilang, area perawat tetap | 🟡 Terpetakan ke source | Cabang `isBedOccupied` di panel panggilan |
| AC-6 status berubah dalam hitungan detik sesudah klaim | 🟡 Terpetakan ke source | Sinyal grup tablet → muat ulang (jeda minimum 1 detik) |
| DoD — lint 0 errors | Terpenuhi | §6 |
| DoD — rekaman tablet termasuk putus jaringan | **Belum** | — |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Risiko | Pasien mengira panggilan terkirim padahal belum; pita gagal kirim tampil paling lambat 4 detik tanpa jawaban. Perilaku tablet sungguhan (layar mati, mode kiosk) menunggu `NV-UNK-007` |
| Status Git | `??` berkas baru tablet |
| Langkah berikutnya | Uji layar di ukuran tablet, termasuk memutus jaringan, bersama uji `BE-NV-014` |
