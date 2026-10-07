# Laporan Perubahan Frontend — `FE-LAB-51`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-LAB-51` |
| Judul | Pemilih dokter pemeriksa memakai daftar Laboratorium |
| Slice | Gelombang `MVP-12b` — `EPIC-LAB-18` diperluas (BR-139) |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian *`MVP-12` diperluas — BR-139* |
| Trace | `FR-18.11`, `FR-18.10` (sisi layar); `LAB-DEC-200`, `LAB-DEC-201`, `LAB-DEC-203`, `LAB-FE-034`; temuan T2 [`FE-LAB-50.md`](FE-LAB-50.md) |
| Contract version | `LAB-API-v1` **`r41`** bagian 36; `LAB-PERM-v1` **revision 14** — `approved` 2026-10-07 (`LAB-REQ-017`) |
| Wewenang UI | `LAB-FE-034` (dibuka kosong) dan `LAB-DEC-200` (isi daftar) — keputusan produk. Bentuk pemilih, teks muat/kosong, susunan label mengikuti `ResourceFilterSelect` yang ada — `DEV_DISCRETION`, tidak diubah |
| Dependency | `BE-LAB-91` ⚠ (working tree backend, belum di-commit) — biner lokal; `FE-LAB-50` ⚠ (working tree frontend) |
| Klasifikasi | `LOW` — 2 berkas source diubah, 1 uji baru; nol komponen, route, slice, maupun thunk baru |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev`; laporan ini beserta status roadmap dan traceability |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `70dd4d4e0` (branch `YogaV2`) + working tree `FE-LAB-50` |
| Commit backend yang dijadikan rujukan | `f17cb984` (branch `yoga`) + working tree `BE-LAB-90`/`BE-LAB-91` |
| Tanggal | 2026-10-07 |
| Status | ⚠ **`SELESAI DENGAN BATAS VERIFIKASI`** — `AC-289` dan `AC-291` terbukti dengan akun analis asli **tanpa mengganti jawaban apa pun**; Konfirmasi sungguhan `LAB-RSMMC-000013` (*Diminta*) langsung menampilkan nama konfirmator, waktu, dan dokter tanpa muat ulang. **Batas:** `AC-292` pada baris *Diterima* (status tetap, Proses aktif) belum diamati di layar — dev tidak punya pesanan *Diterima* belum dikonfirmasi, dan pemilik modul memilih satu tulis minimal |

---

## 1. Keadaan yang ditemukan di awal

| Bukti | Isi |
| --- | --- |
| Pemilih dokter dialog Konfirmasi | `useSelectResource("doctors")` → `GET /v1/corporate/human-resource/master-data/doctors/options` (SDM, `KioskRead`) → `403` bagi analis |
| Registry pilihan | `health-service-select-resources.js` sudah memuat endpoint `options` Lab lain (`labOrganisms`, `labAntibiotics`, …) |
| Nilai awal | Sudah kosong; `openConfirm` dan `closeConfirm` mengosongkannya |
| Reducer konfirmasi | Sudah menyalin `confirmedAt`, `confirmedByName`, `examinerDoctorName` dari jawaban — nol perubahan dibutuhkan |
| Backend | `BE-LAB-91`: `GET /lab-orders/examiner-doctor-options` dan lima ruas jejak konfirmasi pada jawaban |

---

## 2. Proses bisnis dari sisi pengguna

**Pengguna:** analis laboratorium (pemegang `LabOrder : Confirm`).

1. Analis menekan ⋮ → **Konfirmasi** pada baris yang belum dikonfirmasi.
2. Kotak *Dokter Pemeriksa* terbuka kosong; **Simpan Konfirmasi** redup.
3. Analis membuka kotak itu: daftar dokter aktif muncul (nama, kode, spesialisasi).
4. Analis mengetik sebagian nama — daftar disaring di server.
5. Analis memilih dokter → Simpan aktif → Simpan.
6. Baris langsung berisi nama analis, waktu, dan *Dokter pemeriksa: …*; butir Konfirmasi redup.

**Contoh.** Analis mengetik "arif" → satu pilihan *dr. Arif Lesmana* → Simpan. Tanpa memuat ulang, kolom
Konfirmasi berisi nama analis, *7 Okt 2026, 14.03*, dan *Dokter pemeriksa: dr. Arif Lesmana*.

**Jalur tidak normal.**

| Keadaan | Yang dilihat petugas |
| --- | --- |
| Pencarian tanpa hasil | *"Dokter tidak ditemukan"* |
| Dokter dinonaktifkan sesudah dipilih | Simpan → `422` `VAL-73` di dialog (perilaku `FE-LAB-15`) |
| Tanpa `LabOrder : Confirm` | Butir Konfirmasi redup (`FE-LAB-50`); dialog tidak dibuka |

**Perubahan status:** tidak ada yang baru — Konfirmasi mengikuti `r40`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Roadmap `FE-LAB-51`; `r41` 36.3–36.4; `use-lab-monitoring.jsx`, `lab-monitoring-view.jsx` (dialog),
`use-select-resource.jsx`, `select-resource-registry.js`, `health-service-select-resources.js`,
`hr-select-resources.js` (entri `doctors`), `resource-filter-select.jsx`, `filter-select.jsx`, `lab-monitoring-slice.jsx`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/hooks/select/health-service/health-service-select-resources.js` | Entri baru `labExaminerDoctors` — endpoint `/v1/health-services/laboratory-management/lab-orders/examiner-doctor-options`, `valueKey: "id"`, label `fullName`/`doctorCode`, kode `doctorCode`, keterangan `specialistName`. CRLF dipertahankan |
| `src/lib/hooks/health-services/laboratory-management/use-lab-monitoring.jsx` | `examinerSelect` memakai `labExaminerDoctors`, bukan `doctors` |
| `tests/unit/lab-examiner-doctor-select.test.mjs` | **Baru.** 5 uji: endpoint Lab (bukan SDM), empat ruas sebagai label/kode/keterangan, entri `doctors` SDM tidak diubah, hook memakai entri baru, nilai awal kosong |

### 3.3 Kepatuhan arsitektur frontend

Alamat endpoint ditulis **satu kali** di registry (aturan yang dicatat registry itu sendiri); hook memakai
`useSelectResource` yang sudah ada. Nol pengambilan tersendiri di slice, nol komponen baru.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | *"Memuat dokter..."* (pemilih yang sama) |
| Kosong | *"Dokter tidak ditemukan"* |
| Gagal | Pesan galat pemilih; Simpan tetap redup |
| Tanpa hak akses | Butir Konfirmasi redup sejak `FE-LAB-50` |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Laboratory Management / Lab Order

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/laboratory-management/lab-orders/examiner-doctor-options` | Pilihan dokter pemeriksa | `LabOrder : Confirm` |
| `POST` | `/v1/health-services/laboratory-management/lab-orders/{id}/confirm` | Konfirmasi — tidak berubah | `LabOrder : Confirm` |

Pemilih umum mengirim `onlyActive=true`; backend mengabaikannya (`r41` 36.3).

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| ESLint 3 berkas yang disentuh | 0 error, 0 warning | `PASS` | Exit 0 |
| Uji unit baru | 5/5 | `PASS` | |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | 2511 uji: 2503 lulus, 8 gagal | `PASS` (nol kegagalan baru) | Kedelapan nama identik dengan baseline, seluruhnya di luar Laboratorium |
| `npm run build` | `Compiled successfully in 54s` | `PASS` | Nol server hidup saat build; nol berkas sampingan |
| S1 `AC-291` — dialog Konfirmasi `000013` | Pemicu *"Pilih dokter pemeriksa"*, Simpan redup | `PASS` | Akun analis asli |
| S2 `AC-289` — buka pemilih | 16 opsi dari endpoint Lab (`200`); **nol** panggilan ke daftar dokter SDM sejak daftar pasien lab dibuka | `PASS` | Pencatat respons peramban; **tanpa** mengganti jawaban |
| S3 — pencarian | Ketik "arif" → permintaan `search=arif` ke server → 1 opsi *dr. Arif Lesmana* | `PASS` | |
| S4 — pilih | Simpan menjadi aktif | `PASS` | |
| S5 — Konfirmasi **sungguhan** `LAB-RSMMC-000013` | `POST confirm` `200` (status `Confirmed`, `confirmedAt`/`confirmedByName`/`examinerDoctorName` terisi); dialog tertutup; **tanpa muat ulang** kolom berisi nama analis, waktu, *Dokter pemeriksa: dr. Arif Lesmana*; Konfirmasi redup *"…sudah dikonfirmasi…"* | `PASS` | Tangkapan layar; DB: `Confirmed`, konfirmator = akun analis, versi 0→1, riwayat `Order.Confirm Requested→Confirmed` |
| S6 — sesudah muat ulang | Kolom tetap terisi | `PASS` | |
| Penjaga tulis | Hanya satu `POST confirm` `000013` diteruskan; nol tulis lain | `PASS` | Log skrip |
| `AC-292` pada baris *Diterima* (status tetap, Proses aktif tanpa muat ulang) | — | `NOT RUN` | Dev nol pesanan *Diterima* belum dikonfirmasi; pemilik modul memilih satu tulis pada pesanan *Diminta*. Jalur reducer dan jawaban sama; backend sudah membuktikan lima ruas pada `Accepted` (`BE-LAB-91`) |

**Pembanding sebelum perbaikan.** Pada `FE-LAB-50` S4, Konfirmasi yang sama berhasil `200`, tetapi kolom tetap
*Belum Terkonfirmasi* sampai muat ulang. Pada S5 di atas, kolom langsung terisi — itulah T1 yang tertutup.

**Temuan di luar cakupan.** Satu panggilan `GET …/human-resource/master-data/doctors/kiosk/options` → `403`
tercatat sesudah login, **di halaman beranda `/`** — dasbor beranda, bukan Laboratorium (dicatat sejak 2026-10-01
sebagai endpoint `KioskRead`). Label status *Confirmed* belum diterjemahkan — sudah tercatat sejak `FE-LAB-48`.

**Lingkungan:** backend lokal Development (https 7184) dari biner `BE-LAB-91`; `next dev` port 3000 ke backend
lokal; DB dev bersama. Server dimatikan sesudah uji; port 3000, 7184, 5107 bebas. Sandi akun analis hanya ada di
berkas sementara scratchpad dan sudah dihapus.

Uji manual: `PASS` — kecuali `AC-292` baris *Diterima* (`NOT RUN`, alasan di atas).

AUTOMATED TEST: PASS (5 uji baru; nol kegagalan baru).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-289` sisi layar — daftar dari endpoint Lab, nol panggilan SDM | Terpenuhi | S2, S3 |
| `AC-291` — dibuka kosong; Simpan redup sampai dipilih | Terpenuhi | S1, S4, uji unit |
| `AC-292` sisi layar — tanpa muat ulang kolom terisi, Konfirmasi redup | Terpenuhi pada baris *Diminta* | S5, S6 |
| `AC-292` sisi layar — baris *Diterima* tetap *Diterima*, Proses aktif | **Belum** | Butuh pesanan uji *Diterima* |
| Batas AC tambahan (a) `FE-LAB-50` | **Sebagian ditutup** — kolom kini langsung terisi dari jawaban; pengamatan pada baris *Diterima* menyusul | S5 |
| DoD — uji unit, lint, build, laporan | Terpenuhi | Bagian 6 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Nol pada berkas yang disentuh |
| Masalah yang diketahui | Label status *Confirmed* belum diterjemahkan (sejak `FE-LAB-48`); beranda memanggil daftar dokter kios (`403` bagi analis) — keduanya di luar cakupan |
| Dependency backend | `BE-LAB-91` ⚠ — **wajib** dideploy bersama (`LAB-DEC-202`); tanpa itu pemilih menerima `404` |
| Perubahan sampingan | `NONE` |
| Data dev | `LAB-RSMMC-000013` kini *Confirmed* — dikonfirmasi akun analis, dokter pemeriksa *dr. Arif Lesmana* (izin pemilik modul 2026-10-07, satu tulis) |
| Interupsi | Backend pertama kali gagal menyala karena dijalankan dari direktori yang salah; dinyalakan ulang dengan `--project` eksplisit. Tidak berdampak pada data |
| Status Git | Frontend: sembilan berkas `M` (delapan `FE-LAB-50` + registry), satu berkas baru `??` — gabungan `FE-LAB-50`/`FE-LAB-51`, belum di-commit |
| Langkah berikutnya | Langkah rilis `MVP-12c` serempak. Untuk menutup `AC-292` baris *Diterima*: satu pesanan dibawa ke *Diterima* (memicu fakta tagihan) atas izin pemilik modul |
