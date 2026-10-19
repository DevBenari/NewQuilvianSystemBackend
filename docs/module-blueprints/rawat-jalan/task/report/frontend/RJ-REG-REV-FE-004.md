# Laporan Perubahan Frontend — `RJ-REG-REV-FE-004`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `RJ-REG-REV-FE-004` |
| Judul | Identitas: KTP utuh, metode scanner/kamera/upload, lengkapi KTP pasien lama |
| Requirement | `RJ-REG-REV-004` (REV-02), `RJ-REG-REV-005` (REV-05), `RJ-REG-REV-006` (REV-06) |
| Roadmap | [registration-revision-roadmap.md](../../../roadmap/registration-revision-roadmap.md) bagian `4` |
| Trace | `RJ-REG-DEC-008`, `RJ-REG-DEC-009` |
| Dependency | `RJ-REG-REV-BE-004` ✅ |
| Task mode | `CROSS-REPO MODE` — frontend (`RJ-REG-DEC-001`) |
| Commit frontend | `70b3735d` (`sukmagpV2`), belum di-commit |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Tanggal | 14 Oktober 2026 |
| Status | ✅ Selesai teknis (`TESTED`); UAT pengguna belum |

## 1. Proses dari sisi pengguna

**Pasien lama (REV-02, REV-06).** Panel kiri verifikasi pasien menampilkan KTP dalam bingkai berasio kartu
(85,6 × 54) dengan `object-fit: contain` — gambar portrait maupun landscape utuh — dan dapat diklik untuk
diperbesar. Sumber gambar: dokumen identitas privat (diambil sebagai blob berizin), atau foto hasil scan lama.
- Belum ada dokumen → **Tambah Identitas** (izin `PatientIdentityDocument : Create`).
- Sudah ada → **Ganti Identitas** (izin `Create` + `Update`); dokumen lama dinonaktifkan server.
- Modal unggah: nomor identitas (terisi dari pasien), metode upload/kamera, pratinjau, Simpan.

**Pasien baru (REV-05).** Kartu *Metode A — Scanner Identitas* (scanner Plustek existing, tidak diubah) dan
*Metode B — Upload Manual / Kamera*: **Unggah File** (JPG/PNG ≤ 10 MB) atau **Ambil Foto Kamera**
(`getUserMedia`, kamera belakang). Pratinjau + **Batalkan / Ganti Foto / Foto Ulang**. Izin kamera ditolak atau
kamera tidak ada → pesan untuk memakai Unggah File. Hasil kedua cara di-encode ulang menjadi JPEG (≤ 1600 px,
metadata/EXIF terbuang) dan masuk ke `photoBase64` — jalur kanonik yang sama dengan scanner. Pratinjau di panel
scanner dapat diperbesar.

## 2. Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/…/emergency-registration/identity-image-capture.jsx` | Baru — upload/kamera/pratinjau/validasi |
| `src/components/view/…/outpatient-registration/outpatient-identity-panel.jsx` | Baru — panel KTP pasien lama |
| `src/lib/services/…/patient-identity-document.service.js` | Baru — daftar, unggah multipart, konten blob |
| `src/components/view/…/emergency-registration/patient-selection-step.jsx` | Prop opt-in `enableIdentityCapture`, `renderIdentityPanel`; zoom pratinjau |
| `src/components/view/…/outpatient-registration/outpatient-registration-page.jsx` | Mengaktifkan keduanya |
| `src/style/…/emergency-registration.module.css` | Kelas `identity*` (token; gambar `position:absolute` + `contain`) |

## 3. Keputusan base component

`UI GATE: 5 elemen — REUSE 3, WRAP 2, NEW 0`

| Kebutuhan UI | Status | Keputusan |
| --- | --- | --- |
| Tombol aksi | REUSE | `BaseButton` |
| Modal unggah / perbesar | REUSE | `ConfirmModal` dengan `children` (pola Lihat Kartu penjamin) |
| Nomor identitas | REUSE | `BaseTextField` (controlled) |
| Komponen upload/kamera | WRAP | `identity-image-capture.jsx` di domain registrasi (tidak ada base upload/kamera) |
| Panel KTP | WRAP | `outpatient-identity-panel.jsx` (RJ saja) |

## 4. Verifikasi

| Pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| `npm run lint:errors` (repo) | exit 0 | `PASS` |
| ESLint berkas tersentuh | 0 error; 5 warning lama di `patient-selection-step.jsx` (effect lama, ada di `HEAD`) | `PASS` / `EXISTING` |
| `npm run build` | exit 0, 493 halaman | `PASS` |
| `node --test tests/unit/*.test.mjs` | 2823 pass / 30 fail; 30 kegagalan identik dengan salinan `HEAD` (35 fail, 5 karena salinan tak lengkap) — modul lain (Rawat Inap, Bank Darah, Hemodialisa, menu RWI) | `EXISTING / ENVIRONMENT ISSUE` |
| Grep anti-regresi CSS baru (warna literal, typography, `!important`) | kosong | `PASS` |
| U4 tanpa lampiran → Tambah Identitas | lulus | `PASS` |
| U5 pratinjau + Ganti Foto/Batalkan | lulus | `PASS` |
| U6 berkas teks ditolak | lulus | `PASS` |
| U7 kamera tidak tersedia → pesan fallback | lulus (Chromium headless tanpa kamera) | `PASS` |
| U8 simpan `200` tertaut ke pasien | lulus | `PASS` |
| U9 / U9b bingkai rasio 1,585, `contain`, gambar tidak melebihi bingkai | lulus (sesudah perbaikan CSS) | `PASS` |
| U10 Ganti Identitas tampil; U11 perbesar | lulus | `PASS` |
| N1–N3 Metode A/B, pratinjau, perbesar; N7 `photoBase64` JPEG terkirim | lulus | `PASS` |

Uji layar Playwright: FE dev `localhost:3000` (source terkini) dengan API `7184` dialihkan ke backend uji `https://localhost:7185` (build Release `RJ-REG-REV-BE-*`), DB `QuilvianNewDevSukma`, akun SuperAdmin. Build standalone 3100 tidak dipakai karena CORS backend hanya mengizinkan origin 3000. Uji pertama menemukan KTP portrait terpotong bawah (`height:100%` tidak terhitung di dalam tombol grid);
diperbaiki dengan `position:absolute; inset:0`.

`MANUAL TEST: NOT FEASIBLE` untuk kamera fisik dan scanner Plustek fisik di lingkungan uji.

## 5. Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. KTP tampil utuh (landscape/portrait) | Terpenuhi | U9, U9b, tangkapan layar |
| 2. Scanner existing tetap jalan | Terpenuhi lewat kode (alur scanner tidak diubah); fisik `NOT FEASIBLE` | kode |
| 3. Upload file dan kamera → pratinjau + Ganti/Batalkan | Upload terpenuhi (U5); kamera: jalur galat/fallback teruji, pengambilan foto nyata `NOT FEASIBLE` | U5, U7 |
| 4. Pasien tanpa dokumen dapat menambah; Ganti hanya berizin | Terpenuhi | U4, U8, U10 |
| 5. Berkas salah ditolak | Terpenuhi | U6, BE I8 |

## Catatan penutup

| Hal | Isi |
| --- | --- |
| AUTOMATED TEST | `SKIPPED (opsional)` — tidak ada test baru; suite unit dijalankan sebagai regresi |
| Risiko tersisa | Kamera dan scanner fisik belum diuji; foto KTP pasien baru masih lewat `PhotoPath` publik (perilaku kanonik lama, `RJ-REG-DEC-009`) |
| Perubahan sampingan | `NONE` |
| Status Git | Berkas di atas `M`/`??` pada `V2QuilvianSystemFrontendDev`, belum di-commit |
| Langkah berikutnya | UAT di perangkat berkamera dan dengan scanner Plustek |

## Revisi 2026-10-15 — bingkai identitas tanpa letterbox

Masukan user: foto KTP pada panel identitas masih menyisakan ruang kosong di dalam bingkai. Penyebabnya adalah bingkai beraspek rasio kartu tetap (`aspect-ratio: 85.6 / 54`) yang dipadukan dengan `object-fit: contain`. Akibatnya, foto dengan rasio berbeda menyisakan letterbox.

| Hal | Isi |
| --- | --- |
| Berkas | `src/style/health-services/registration-management/emergency-management/emergency-registration.module.css` (`.identityKtpFrame`, `.identityKtpFrame img`); komentar JSDoc `outpatient-identity-panel.jsx` |
| Perubahan | `aspect-ratio` dan posisi absolut dihapus dari bingkai. Gambar memakai `width: 100%; height: auto`, jadi tinggi bingkai mengikuti rasio asli foto. Foto tampil utuh, tanpa letterbox, dan tanpa terpotong. |
| Keputusan base component | `.identityKtpFrame` hanya dipakai oleh panel ini (grep). Status `REUSE` untuk kelas yang sama, tanpa komponen baru. `UI GATE: PASS` |
| ESLint berkas tersentuh | exit 0 — `PASS` |
| Grep anti-regresi (warna literal, `!important`) pada blok yang diubah | kosong — `PASS` |
| Uji CSS headless (Chromium, Playwright) | Foto landscape 1000×620: bingkai 238×148 sama dengan gambar 238×148. Foto portrait 600×900: bingkai 238×357 sama dengan gambar 238×357. Tidak ada ruang kosong — `PASS` |
| MANUAL TEST | Layar pendaftaran nyata belum diuji ulang. User perlu memuat ulang halaman di FE 3000. |
| Risiko | Foto portrait membuat bingkai lebih tinggi (sekitar 357 px pada lebar 240 px). |
| AUTOMATED TEST | `SKIPPED (opsional)` — perubahan CSS kecil |

## Revisi 2026-10-15 — tombol Batal pada modal Ganti/Tambah Identitas

Masukan user: pada step Cari/Input Pasien, tombol **Batal** di modal *Ganti Dokumen Identitas* tidak bisa diklik sebelum foto dipilih. Penyebabnya adalah prop `disabled={!captured?.blob}` pada `ConfirmModal`. Prop itu menandai seluruh modal sibuk (`isBusy`), sehingga tombol Batal, ESC, dan tombol tutup ikut terkunci.

| Hal | Isi |
| --- | --- |
| Berkas | `outpatient-identity-panel.jsx` |
| Perubahan | Diganti menjadi `confirmProps={{ disabled: !captured?.blob }}`. Sekarang hanya *Simpan Identitas* yang nonaktif sampai foto dipilih. Pola ini sama dengan `journal-reversal-dialog.jsx` dan `blood-bank-procedure-list-view.jsx`. Saat menyimpan, `loading={saving}` tetap mengunci seluruh modal. |
| Keputusan base component | `ConfirmModal` `REUSE`, perilaku default tidak diubah. `UI GATE: PASS` |
| ESLint berkas tersentuh | exit 0 — `PASS` |
| MANUAL TEST | Belum diuji ulang di layar. User perlu memastikan Batal menutup modal sebelum foto dipilih. |
| AUTOMATED TEST | `SKIPPED (opsional)` — perubahan satu prop |
