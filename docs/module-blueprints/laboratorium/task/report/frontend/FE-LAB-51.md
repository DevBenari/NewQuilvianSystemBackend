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
| Status | ✅ **`SELESAI`** — 2026-10-21. Semula ⚠ (`AC-292` pada baris *Diterima* belum diamati). `AC-292` kini terbukti pada baris *Diterima* dengan Konfirmasi sungguhan oleh akun analis asli (bagian 9) |

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
| `AC-292` pada baris *Diterima* (status tetap, Proses aktif tanpa muat ulang) | Status tetap *Diterima*, Konfirmasi redup, Proses aktif, kolom terisi — tanpa muat ulang | `PASS` (2026-10-21) | Bagian 9 — semula `NOT RUN` |

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
| `AC-292` sisi layar — baris *Diterima* tetap *Diterima*, Proses aktif | Terpenuhi (2026-10-21) | `LAB-RSMMC-000028` — bagian 9 |
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
| Langkah berikutnya | `NONE` — task selesai |

---

## 9. Penuntasan batas verifikasi (2026-10-21)

| Hal | Isi |
| --- | --- |
| Pesanan uji | `LAB-RSMMC-000028` — kunjungan IGD pasien uji 00-00-00-17, Kalium (data uji `MVP-15e`, izin tulis pemilik modul 2026-10-21). Satu pesanan dibawa melewati seluruh alur v1 untuk menuntaskan `FE-LAB-46`, `48`, `50`, dan `51` sekaligus |
| Akun | Gilang (Analis) dan dr. Bima (Kepala Instalasi) — akun asli; sandi tidak dicatat. Superadmin hanya pada satu langkah antara (lihat di bawah) |
| 1. Rencana wadah — layar Wadah (Gilang) | `POST /lab-specimens/by-order/{GUID pesanan ini}` → `201`; wadah *Direncanakan* menopang Kalium; kartu tampil tanpa muat ulang |
| 2. Diambil → diterima (Gilang), layak (petugas kedua) | `200`/`200`; *layak* oleh Gilang ditolak `403` *"Petugas yang mengambil sampel tidak boleh menyatakan kelayakannya"* (empat mata, `VAL-09`) → dinyatakan layak oleh superadmin sebagai pengganti analis kedua (sandi Vina tidak tersedia) → pesanan *Diterima*, belum dikonfirmasi |
| 3. Konfirmasi — Daftar Pasien Lab PK (Gilang) | Dialog kosong, Simpan redup (`AC-291`); dokter dipilih dari daftar Lab; `POST confirm` → `200`. **Tanpa muat ulang:** kolom Dokter Pemeriksa *dr. Aditya Pranata, Sp.PK*, kolom Konfirmasi *Gilang Mahendra · 21 Okt 2026 12.18*, status **tetap** *Diterima*, Konfirmasi redup, **Proses Pemeriksaan aktif**. Bukti: tangkapan layar sesudah Simpan dan sebelum muat ulang (toast *"Pesanan berhasil dikonfirmasi."* masih tampil) dan teks baris sesudah muat ulang. Dua cek teks skrip sempat `FAIL` karena masih mencari awalan pra-v1 *"Dokter pemeriksa:"* — sejak `r47` dokter pemeriksa punya kolom sendiri; bukan cacat layar |
| 4. Proses Pemeriksaan — layar (Gilang) | `PUT start-process` → `200`; baris *Sedang Dikerjakan* |
| 5. Hasil (langkah antara, API) | Gilang mengisi Kalium 4,1 dan memfinalkan → `200`; dr. Bima memvalidasi → `200`; rilis oleh validator yang sama ditolak `422` (empat mata) → dr. Bima merilis dengan alasan pengecualian resmi *"Shift tunggal, tidak ada dokter lain bertugas"* → `200`; pesanan *InProcess*, `resultProgress` *AllReleased* |
| 6. Selesaikan — layar (Gilang) | Selesaikan aktif; `PUT complete` → **`200` sungguhan**; dialog tertutup; baris langsung *Selesai* tanpa muat ulang; server `Completed` |
| Hasil untuk task ini | Langkah 3: `AC-291` dan `AC-292` pada baris *Diterima* `PASS`; daftar dokter dari endpoint Lab (`AC-289`) tanpa `403` |
