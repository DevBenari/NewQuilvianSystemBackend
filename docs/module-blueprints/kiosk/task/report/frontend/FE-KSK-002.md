# Laporan Perubahan Frontend — `FE-KSK-002`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-KSK-002` |
| Judul | Format tanggal lahir dan nama utuh |
| Slice | Isi epic `MVP-2`, mandiri → gelombang eksekusi 1 |
| Roadmap | `docs/module-blueprints/kiosk/roadmap/frontend-roadmap.md` — kartu `FE-KSK-002` |
| Trace | `FR-KSK-025`; `KSK-DEC-015`; KSK-BASE-001/002; `03-frontend-architecture.md` §3 `FE-KSK-03` (Step 2 dan 7) |
| Contract version | `NOT APPLICABLE` — tanpa perubahan API |
| Wewenang UI | `KSK-DEC-015` (tanggal lahir bulan pendek); KSK-BASE-001 (nama tidak dipotong) |
| Dependency | `NONE` |
| Klasifikasi | `LIGHT` — 2 berkas, 2 pemanggilan formatter |
| Task mode | `FRONTEND` — pengguna 2026-09-30 ("oke lanjutkan" setelah `FE-KSK-001`) |
| Target tulis | `V2QuilvianSystemFrontendDev`: `kiosk-old-patient-step-review.jsx`, `kiosk-old-patient-step-confirm.jsx`. Repository backend: laporan ini dan baris status roadmap/traceability |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `4ec51b0bf` (branch `sukmagpV2`), belum di-commit |
| Commit backend yang dijadikan rujukan | `NOT APPLICABLE` |
| Tanggal | 30 September 2026 |
| Status | ✅ SELESAI — 3 dari 3 acceptance criteria terbukti |

---

## 1. Keadaan yang ditemukan di awal

1. **Tanggal lahir** di Review Data dan Konfirmasi memakai `formatDateId` (bulan panjang), misalnya "12 September 1990". Keputusan `KSK-DEC-015` meminta bulan pendek: "12 Sep 1990". Formatter bulan pendek `formatShortDateId` sudah ada di service yang sama, tetapi belum dipakai.
2. **Nama lengkap**: pemeriksaan CSS (`kiosk-old-patient-view.module.css`) tidak menemukan aturan `text-overflow`, `line-clamp`, atau `white-space: nowrap` pada elemen nama di Review, Konfirmasi, maupun Tiket. Kesan "nama terpotong" pada screenshot `FE-KSK-005` ternyata karena tertutup dialog peringatan, bukan ellipsis. Tetap dibuktikan di browser dengan nama lima kata.

---

## 2. Proses bisnis dari sisi pengguna

1. Pasien memilih dirinya di Step Identifikasi, lalu melihat **Review Data**. Tanggal lahir tampil **"12 Sep 1990"**, dan nama panjang seperti "KSKTEST Muhammad Rizky Pratama Wijayakusuma" tampil utuh, terbungkus ke baris berikutnya bila perlu.
2. Di **Konfirmasi**, baris "Tempat/Tgl Lahir" tampil "12 Sep 1990" (didahului tempat lahir bila ada), dan nama tampil utuh di kotak pasien.
3. Di **Tiket (Cetak Antrean)**, nama tampil utuh. **Tanggal kunjungan tidak berubah**, tetap format panjang, misalnya "30 September 2026".

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`kiosk-old-patient-step-review.jsx`, `kiosk-old-patient-step-confirm.jsx`, `kiosk-old-patient-step-ticket.jsx`; `kiosk-old-patient-registration.service.js` (`formatDateId`, `formatShortDateId`, `createOldPatientEncounter`); `kiosk-old-patient-view.module.css` (seluruh aturan pemotong teks).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/kiosk/registration/old-patient/kiosk-old-patient-step-review.jsx` | "Tanggal Lahir" memakai `formatShortDateId`; import `formatDateId` diganti (tidak ada pemakaian lain di berkas ini) |
| `src/components/view/kiosk/registration/old-patient/kiosk-old-patient-step-confirm.jsx` | `birthText` memakai `formatShortDateId`; import diganti (tidak ada pemakaian lain) |

`kiosk-old-patient-step-ticket.jsx` **tidak diubah**: tanggal kunjungan tetap `formatDateId` (AC 3), dan nama sudah tampil utuh.

### 3.3 Kepatuhan arsitektur frontend

- Memakai formatter existing (`formatShortDateId`), tanpa helper baru.
- Tidak ada perubahan CSS maupun elemen UI — `UI GATE: N/A — hanya penggantian formatter tanggal pada teks existing`.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat / Kosong / Gagal / Tanpa hak akses | Existing, tidak berubah. Tanggal lahir kosong atau tidak sah tetap tampil "-" (fallback `formatShortDateId`) |

---

## 5. Endpoint yang dikonsumsi

`NOT APPLICABLE` — tidak ada endpoint baru atau yang berubah.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` 2 berkas task | 0 error; 6 warning **identik dengan versi `HEAD`** (dibandingkan lewat `git show HEAD:<berkas> \| eslint --stdin`): 3× `react-hooks/refs` dan 2× `<img>` di Konfirmasi, 1× `<img>` di Review — tidak ada di baris yang diubah | `PASS` (tanpa warning baru) | Keluaran perintah |
| `npm run build` | Exit `0`, "Compiled successfully in 71s" | `PASS` | Keluaran `next build` |
| Uji browser Playwright Review → Konfirmasi → Tiket | 7/7 PASS pada run final | `PASS` | Tabel di bawah + screenshot |

`AUTOMATED TEST: SKIPPED (opsional) — uji browser lewat skrip Playwright sekali pakai di scratchpad; repository tidak memakai Jest`

### Uji browser

Lingkungan: `next dev` port 3000 → backend `bin/Release` di `https://localhost:7184` → DB `QuilvianNewDevSukma`. Login akun perangkat Kiosk `kiosk-test` (kredensial dari pengguna, tidak dicatat).

Data (tanpa menulis DB):
- Respons pencarian dan detail pasien samaran P7 diambil dari backend asli lalu diubah di browser (`page.route` + `route.fetch`): nama menjadi "KSKTEST Muhammad Rizky Pratama Wijayakusuma" (lima kata) dan tanggal lahir menjadi 12 September 1990.
- Jenis kunjungan, pembayaran, poliklinik, dan jadwal dokter memakai data asli (baca-saja).
- `POST …/patient-encounters` disimulasikan agar tidak ada kunjungan tersimpan.

Pemeriksaan nama bersifat objektif: setiap elemen yang teksnya persis nama lengkap diukur, dan dinyatakan terpotong bila `scrollWidth > clientWidth`, `text-overflow: ellipsis`, atau `-webkit-line-clamp` aktif.

| ID | Skenario | Hasil | Klasifikasi |
| --- | --- | --- | --- |
| AC1a | Review Data — tanggal lahir | "12 Sep 1990"; "12 September 1990" tidak ada | `PASS` |
| AC2a | Review Data — nama lima kata | 2 elemen (kartu profil + kolom Nama Lengkap), 0 terpotong; tampil terbungkus 2–4 baris | `PASS` |
| AC1b | Konfirmasi — "Tempat/Tgl Lahir" | "12 Sep 1990" | `PASS` |
| AC2b | Konfirmasi — nama | 1 elemen (kotak pasien), 0 terpotong | `PASS` |
| R-T | Buat Antrean (POST disimulasikan) | Tiket "A-001" tampil; 1 POST tertangkap tiruan | `PASS` |
| AC2c | Tiket — nama | 1 elemen, 0 terpotong | `PASS` |
| AC3 | Tiket — item "Tanggal Kunjungan" | "30 September 2026" (format panjang, tidak berubah) | `PASS` |
| R-akhir | DB | `MstPatient` 17 (nama P7 tetap "KSKTEST Karyawan PT Samaran"), `RegPatientEncounter` 173, `RegPatientEncounterGuarantor` 173, `TrxQueue` 173 | `PASS` |

**Kejadian selama uji (dicatat apa adanya):**
- Pada run pertama, rute tiruan hanya mencocokkan `…/patient-encounters/kiosk`, padahal Pasien Lama mengirim ke alias `…/patient-encounters`. Akibatnya **satu `POST` pembuatan kunjungan sampai ke backend asli**.
- Browser uji ditutup saat request itu berjalan. Backend mencatat `500` "The operation was canceled", dan transaksi tidak ter-commit: jumlah kunjungan, penjamin, dan antrean tetap 173.
- Rute diperbaiki untuk menangkap `POST` ke kedua path; run final tidak mengirim `POST` kunjungan ke backend.
- Pada run yang sama, pemeriksaan tanggal tiket sempat memberi hasil palsu karena teks footer ("Rabu, 30 September 2026") ikut terbaca. Pemeriksaan dibatasi ke item "Tanggal Kunjungan".

Uji manual oleh orang: `NOT RUN` — digantikan uji browser otomatis dan pemeriksaan visual screenshot Review, Konfirmasi, dan Tiket.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria (roadmap) | Status | Bukti |
| --- | --- | --- |
| 1. Pasien lahir 12 September 1990 tampil `12 Sep 1990` di Review dan Konfirmasi | Terpenuhi | AC1a, AC1b |
| 2. Nama lima kata tampil utuh (boleh terbungkus baris) di Review, Konfirmasi, dan Tiket | Terpenuhi (tanpa perubahan kode; sudah benar di CSS existing) | AC2a–c |
| 3. Tanggal kunjungan/tiket tidak ikut berubah format | Terpenuhi | AC3; `step-ticket.jsx` tidak diubah |
| DoD: AC 1–3 di `FE-KSK-002.md` | Terpenuhi | Laporan ini |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 6 warning lint lama di dua berkas task (3× `react-hooks/refs` di Konfirmasi, 3× `<img>`) — tidak diperbaiki karena di luar cakupan |
| Masalah yang diketahui | `NONE` |
| Dependency backend | `NONE` |
| Perubahan sampingan | `NONE` di source. Satu `POST` kunjungan uji sempat mencapai backend dan dibatalkan tanpa data tersimpan (lihat §6) |
| Interupsi | Sesi agent terputus saat server uji masih berjalan; dilanjutkan dengan server yang sama dari keadaan terverifikasi |
| Status Git | Frontend (task ini): ` M src/components/view/kiosk/registration/old-patient/kiosk-old-patient-step-review.jsx`, ` M src/components/view/kiosk/registration/old-patient/kiosk-old-patient-step-confirm.jsx`, di samping perubahan `FE-KSK-001`, `FE-KSK-003..006` yang belum di-commit |
| Langkah berikutnya | Tutup `KSK-OQ-004` (pencatatan amendment `FE-LAB-13` / `AC-93` di blueprint Laboratorium) untuk membuka `FE-KSK-007`, lalu `FE-KSK-008` |
