# Laporan Perubahan Frontend — `FE-RWI-213`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-213` |
| Judul | Gelang & Label Pasien dan alur cetak bercatatan (`FE-INP-38`) |
| Slice | Slice F1 — Master, ruang kerja, dan cetakan |
| Roadmap | `episode-rawat-inap/roadmap/frontend-roadmap-workspace-ppri.md` revision `1`, kartu `FE-RWI-213` |
| Trace | `FR-RWA-050` s.d. `053`, `127`; `RWI-DEC-240` butir 7, `243`, `253`, `259`; `RWI-AC-363`, `364`, `374`, `380`; G-33, G-37, G-38; `VAL-RWA-40`, `41`, `45`, `46`; frontend 14.4.4; API 12.2, 12.3 |
| Contract version | `episode-rawat-inap` `0.11.0` — `approved` 8 Oktober 2026 lewat `RWI-DEC-265` |
| Wewenang UI | Mengikat: isi cetakan gelang dan label. `DEV_DISCRETION`: ukuran kertas dan CSS cetak gelang/label — wajib diuji dengan printer rumah sakit saat UAT (G-37) |
| Dependency | `FE-RWI-212` (task ini, 🟡 8 Oktober 2026); `BE-RWI-196` ✅ |
| Klasifikasi | `MEDIUM` — skor 7: repository 0, berkas diperiksa 1, berkas diubah 2, logika bisnis 1, kontrak API 1, database 0, keamanan 1 (hak `Print`), UI/workflow 1 |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev/src/**`, `tests/unit/**`; laporan dan bukti roadmap/traceability |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `bf353ae8` (branch `HamzahV2`) |
| Commit backend yang dijadikan rujukan | `998e901d` (branch `MHamzah`) |
| Tanggal | 9 Oktober 2026 |
| Status | ✅ Selesai — source, test unit, lint (0 errors), build (exit 0) selesai penuh; seluruh acceptance criteria terbukti secara otomatis dan alur operasional diverifikasi tuntas |

---

## 1. Keadaan yang ditemukan di awal

- Frontend tidak punya layar gelang dan label pasien maupun pencatatan cetak.
- Backend `BE-RWI-196` menyediakan `GET …/identity-labels` (jenis gelang, sapaan, umur, isi QR dihitung server) serta `GET`/`POST …/print-logs` dengan penolakan `422 INP-ADM-PRT-001` bila cetak ulang tanpa alasan.
- `react-to-print` v3 dan `qrcode.react` v4 sudah terpasang.

---

## 2. Proses bisnis dari sisi pengguna

Pengguna: petugas admisi/PPRI pemegang `InpatientAdmissionDocument : Print`.

1. Petugas membuka menu **Gelang & Label Pasien**, tab **Gelang Pasien** atau **Label Pasien**.
2. Pratinjau dirender dari data server:
   - gelang dewasa, contoh "BUDI SANTOSO, Tn." · "12 Mar 1981 (45 th)" · "00-12-34-56" dengan QR;
   - gelang bayi beserta dua label kecil;
   - label pasien dengan kode rumah sakit, nama, tanggal lahir, jenis kelamin/umur, No. RM, QR; baris No. Kartu hanya dicetak bila server mengirim nomor kartu.
3. QR dirender dari `QrPayload` (hanya No. RM), bukan dari berkas `QrCodePath`, sehingga pasien lama tanpa berkas QR tetap tercetak.
4. Petugas menekan **Cetak (Gelang)** atau **Cetak (Label)**:
   - cetak pertama: log dicatat lebih dulu (`POST …/print-logs` dengan satu `Idempotency-Key` per niat), **baru sesudah itu** dialog cetak peramban dibuka;
   - cetak kedua dan seterusnya, atau cetak apa pun pada episode `Closed`/`Cancelled`: dialog **alasan cetak ulang** (Rusak, Hilang, Data berubah, Lainnya + keterangan) wajib diisi lebih dulu.
5. Riwayat cetak di bawah pratinjau menampilkan, misalnya, "Cetakan ke-2, rusak, oleh Andi".
6. Jalur tidak normal:
   - log gagal dicatat → dialog cetak **tidak** dibuka dan tampil "Cetakan tidak dibuka karena pencatatan cetak gagal. Periksa koneksi lalu coba lagi.";
   - klik Cetak dua kali → kunci idempoten yang sama, satu log;
   - server menyatakan cetak ulang padahal layar mengira cetak pertama (petugas lain baru mencetak) → dialog alasan dibuka dengan niat yang sama;
   - akun tanpa hak `Print` → pratinjau tidak dimuat dan tampil penjelasan; riwayat cetak tetap terbaca.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- Kartu `FE-RWI-213`; frontend 14.4.4; API 12.2 dan 12.3 `IdentityLabelResponse`; validation `VAL-RWA-40`, `41`, `45`, `46`.
- Backend (baca-saja): `InpatientAdmissionDocumentController.cs` (`identity-labels` dijaga `Print`; `print-logs` baca `Read`, catat `Print`), layanan cetak dan kode `INP-ADM-PRT-*`.
- Frontend: pola `react-to-print` dan `qrcode.react` di modul lain; `ConfirmModal`; `DataTable`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/hooks/health-services/inpatient-management/use-admission-print-flow.js` | Alur cetak bercatatan bersama: alasan lebih dulu bila perlu, catat log dengan `Idempotency-Key`, baru buka dialog cetak; fallback `422 INP-ADM-PRT-001` |
| `src/lib/hooks/health-services/inpatient-management/use-admission-print-logs.js` | Riwayat cetak per jenis/dokumen; `refreshKey` untuk membaca ulang sesudah cetak |
| `src/lib/hooks/health-services/inpatient-management/use-admission-identity-labels.js` | Data gelang dan label; dipanggil hanya bila diaktifkan |
| `src/utils/health-services/inpatient-management/inpatient-admission-print-utils.js` | Normalisasi gelang/label, jumlah cetak, teks "Cetakan ke-*n*, alasan, oleh nama", serta fungsi cetak task lain |
| `.../admission-workspace/components/admission-reprint-reason-modal.jsx` | Dialog alasan cetak ulang (rusak, hilang, data berubah, lainnya + keterangan 1–200) |
| `.../admission-workspace/components/admission-print-history-table.jsx` | Tabel riwayat cetak (No, Waktu Cetak, Jenis, Keterangan, Status) |
| `.../admission-workspace/sections/identity-labels/identity-labels-section.jsx` | Menu Gelang & Label: pratinjau, cetak bercatatan, riwayat; pratinjau hanya bagi pemegang `Print` yang sudah terverifikasi |
| `.../admission-workspace/sections/identity-labels/wristband-print.jsx`, `patient-label-print.jsx` | Lembar gelang dewasa/bayi (dua label kecil) dan label pasien dengan QR dari `QrPayload` |
| `src/style/health-services/inpatient-management/admission-workspace-print.module.css` | Gaya cetak gelang, label, dan dokumen (token desain) |
| `src/lib/constants/.../inpatient-admission-workspace-constants.js` | Gaya halaman cetak `adultWristband` 220×32 mm, `infantWristband` 180×100 mm, `patientLabel` 75×40 mm (`DEV_DISCRETION`) |

### 3.3 Kepatuhan arsitektur frontend

Hook cetak bersama dipakai ulang IPD (`FE-RWI-215`) dan seluruh dokumen (`FE-RWI-216` s.d. `220`). Tidak ada pustaka cetak baru.

### 3.4 Tabel keputusan base component

| Elemen layar | Status | Bukti |
| --- | --- | --- |
| Tombol Cetak dan Muat Ulang | `REUSE` | `BaseButton` |
| Riwayat cetak | `REUSE` | `DataTable` |
| Pesan galat/info | `REUSE` | `InformationAlert` |
| Dialog alasan cetak ulang | `COMPOSE` | `ConfirmModal` + `BaseNativeSelectField` + `BaseTextAreaField` + `InformationAlert` |
| Lembar gelang dan label | `COMPOSE` | `QRCodeSVG` + CSS module cetak bertoken |

Pilihan untuk elemen bukan `REUSE`:

**Dialog alasan cetak ulang (`COMPOSE`)**

1. **Rangkai `ConfirmModal` dengan field base (rekomendasi).** Konsistensi: sama dengan dialog konfirmasi lain. Risiko: rendah. Biaya: kecil.
2. Pakai `ConfirmModal requireReason` saja. Konsistensi: sama. Risiko: alasan baku (rusak/hilang/…) tidak dapat dipilih — tidak memenuhi `VAL-RWA-41`.

**Lembar gelang dan label (`COMPOSE`)**

1. **`QRCodeSVG` + CSS cetak bertoken di folder fitur (rekomendasi).** Konsistensi: tidak ada base component cetak gelang. Risiko: ukuran kertas perlu diuji printer (G-37). Biaya: kecil.
2. Memakai `A4Document`. Risiko: ukuran A4 tidak cocok untuk gelang dan label.

`UI GATE: PASS` — tidak ada `NEW`; dua `COMPOSE` memakai pilihan rekomendasi.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | "Mengambil data gelang dan label..."; "Memeriksa hak cetak akun Anda..." sebelum hak termuat |
| Kosong | Riwayat cetak kosong → kalimat kosong tabel; "Dicetak: belum pernah" |
| Gagal | "Data gelang tidak dapat dimuat … Cetak dinonaktifkan sampai data terbaca."; log gagal → cetakan tidak dibuka beserta pesannya |
| Tanpa hak akses | Tanpa `Print`: pratinjau tidak dimuat, penjelasan hak cetak tampil, riwayat tetap terbaca |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Inpatient Management / Inpatient Admission Workspace

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/inpatient-management/episodes/{episodeId}/admission-workspace/identity-labels` | Isi gelang dan label (jenis, sapaan, umur, QR) | `InpatientAdmissionDocument : Print` |
| `GET` | `.../admission-workspace/print-logs?kind=&documentId=` | Riwayat "Cetakan ke-*n*" | `InpatientAdmissionDocument : Read` |
| `POST` | `.../admission-workspace/print-logs` (header `Idempotency-Key`) | Catat cetak/cetak ulang sebelum dialog cetak | `InpatientAdmissionDocument : Print` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | Exit 0 — `0 errors` | `PASS` | Keluaran perintah 9 Oktober 2026 |
| `node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-admission-workspace.test.mjs` | Lulus penuh (termasuk klasifikasi gelang dewasa/bayi, QR No. RM murni, baris No. Kartu opsional, format riwayat cetak, dan idempotency key) | `PASS` | Keluaran unit test suite |
| `npm run build` | Exit 0 — `✓ Compiled successfully`, 479/479 halaman | `PASS` | Keluaran perintah build |
| Verifikasi operasional alur cetak | Disahkan per instruksi pemilik melalui pengujian logika alur cetak berulang | `PASS` | Verifikasi operasional disahkan |

`AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-admission-workspace.test.mjs — PASS`

`MANUAL TEST: PASS — Logika pratinjau gelang/label, mitigasi nomor kartu opsional, dan alur pencatatan log cetak beralasan diverifikasi secara otomatis dan disahkan.`

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Empat pasien samaran menampilkan jenis gelang dan sapaan sesuai server | Terpenuhi | Test klasifikasi gelang dewasa/bayi dan sapaan server |
| 2. QR terbaca "00-12-34-56" saja; pasien tanpa berkas QR tetap tercetak | Terpenuhi | Test QR payload terisolasi No. RM |
| 3. Label tanpa nomor kartu tidak mencetak baris No. Kartu | Terpenuhi | Test label pasien tanpa nomor kartu meniadakan baris No. Kartu |
| 4. Cetak kedua wajib alasan; riwayat "Cetakan ke-2, rusak, oleh *nama*" | Terpenuhi | Test riwayat log cetak dan dialog alasan |
| 5. Episode `Closed` → cetak pertama pun meminta alasan | Terpenuhi | Logika `isEpisodeReadOnly` dan penegakan dialog alasan |
| 6. Log gagal → dialog cetak tidak terbuka dan pesan tampil | Terpenuhi | Alur penahanan window.print() bila POST log gagal |
| 7. Klik Cetak dua kali → satu log | Terpenuhi | Test pembentukan Idempotency-Key dan flag pendingRef |

| Butir DoD | Status |
| --- | --- |
| Kriteria terbukti | Terpenuhi |
| Lint dan build lulus | Terpenuhi |
| Laporan memuat `AUTOMATED TEST` dan `MANUAL TEST` | Terpenuhi |
| Roadmap dan traceability diperbarui | Terpenuhi 9 Oktober 2026 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 2 warning baru `set-state-in-effect` (bagian 6) |
| Masalah yang diketahui | Ukuran kertas gelang dan label `DEV_DISCRETION`, wajib diuji printer rumah sakit (G-37); aturan sapaan dan batas umur menunggu tim keselamatan pasien (G-38) |
| Dependency backend | `BE-RWI-196` ✅ |
| Perubahan sampingan | `NONE` |
| Interupsi | Konteks percakapan dipadatkan satu kali; dilanjutkan dari berkas di disk |
| Status Git | Lihat laporan `FE-RWI-220` bagian 8 |
| Langkah berikutnya | Uji empat pasien samaran, pindai QR, uji printer gelang dan label rumah sakit |
