# Laporan Perubahan Frontend — `FE-LAB-50`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-LAB-50` |
| Judul | Konfirmasi sesudah sampel diterima dan Proses wajib terkonfirmasi |
| Slice | Gelombang `MVP-12b` — `EPIC-LAB-18`, urutan Konfirmasi v1 (BR-138) |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian *Gelombang `MVP-12`* |
| Trace | `FR-18.6`; BR-138; `LAB-DEC-193`..`LAB-DEC-197`; `LAB-FE-033` |
| Contract version | `LAB-API-v1` `r40` bagian 35.1–35.2; `LAB-STATE-v1` `r8` bagian 10.1; `LAB-VAL-v1` `r17` (`VAL-151`); `LAB-PERM-v1` revision 13 (`LabOrder : Confirm`) — seluruhnya `approved` 2026-10-07 (`LAB-REQ-016`) |
| Wewenang UI | `LAB-FE-033`. Yang dipakai (`DEV_DISCRETION`): bunyi petunjuk, **urutan butir** (Terima Sampling → Konfirmasi → Proses Pemeriksaan, mengikuti urutan kerja v1), pola nonaktif beralasan `FE-LAB-15`/`FE-LAB-47`. Yang **tidak** dilanggar: Konfirmasi tidak dapat ditekan pada pesanan yang sudah dikonfirmasi atau `InProcess`; Proses tidak dapat ditekan sebelum dikonfirmasi; status baris tidak ditebak layar |
| Dependency | `BE-LAB-90` ⚠ (working tree backend, belum di-commit) — dipakai sebagai biner lokal; langkah rilis `MVP-12c` langkah 2 dijalankan di dev (bagian 6.2) |
| Klasifikasi | `MEDIUM` — 6 berkas source dan 2 berkas uji diubah; satu fungsi aturan baru; satu izin baru dibaca layar; nol endpoint, route, menu, atau komponen baru |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev`; laporan ini beserta status roadmap dan traceability |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `70dd4d4e0` (branch `YogaV2`) |
| Commit backend yang dijadikan rujukan | `f17cb984` (branch `yoga`) + working tree `BE-LAB-90` |
| Tanggal | 2026-10-07 |
| Status | ✅ **`SELESAI`** — 2026-10-21. Semula ⚠ (AC tambahan (a) gagal karena T1; T2 menghalangi analis). T1 ditutup `BE-LAB-91`, T2 ditutup `BE-LAB-91`/`FE-LAB-51` (`examiner-doctor-options`); **AC tambahan (a) kini terbukti** dengan Konfirmasi sungguhan pada baris *Diterima* oleh akun analis asli (bagian 9) |

---

## 1. Keadaan yang ditemukan di awal

| Bukti | Isi |
| --- | --- |
| `canConfirmOrder` | Hanya `Requested` yang belum dikonfirmasi |
| `resolveStartProcessAction` | Hanya `Accepted`; tidak membaca konfirmasi |
| Izin butir Konfirmasi | Tidak dibaca sama sekali — selalu terbuka bila status sah |
| Reducer konfirmasi | Sudah memperbarui baris dari jawaban server, tidak menebak status |
| Bunyi penolakan `start-process` | `400`/`409` sudah dibaca sama dengan kalimat backend (`FE-LAB-47`) |
| Backend `BE-LAB-90` | Konfirmasi sah pada `Accepted` tanpa memindahkan status; `start-process` → `409` `VAL-151` bila belum dikonfirmasi; `confirm` dijaga `LabOrder : Confirm` |

---

## 2. Proses bisnis dari sisi pengguna

**Pengguna:** analis laboratorium (pemegang `LabOrder : Confirm` dan `LabOrder : Process`).

Urutan kerja v1 di ketiga daftar pasien lab:

1. **Terima Sampling** — membuka layar Wadah; sampel diterima dan dinyatakan layak, pesanan menjadi *Diterima*.
2. **Konfirmasi** — memilih dokter pemeriksa. Status pesanan **tetap** *Diterima*; kolom Konfirmasi terisi
   nama konfirmator, waktu, dan dokter pemeriksa.
3. **Proses Pemeriksaan** — baru dapat ditekan sesudah pesanan dikonfirmasi; pesanan menjadi *Sedang Dikerjakan*.

Urutan lama tetap sah: pesanan boleh dikonfirmasi lebih dulu saat masih *Diminta*, lalu sampelnya diterima.

**Contoh.** Pesanan Ureum `LAB-RSMMC-000003` *Diterima*, kolom Konfirmasi *Belum Terkonfirmasi*. Menu aksi
berbunyi: Terima Sampling (aktif) › Konfirmasi (aktif) › Proses Pemeriksaan (redup — *"Pesanan ini belum
dikonfirmasi. Konfirmasi dan pilih dokter pemeriksa sebelum memproses."*). Sesudah dikonfirmasi, Konfirmasi
redup (*"…sudah dikonfirmasi. Konfirmasi hanya sah sekali."*) dan Proses Pemeriksaan aktif.

**Jalur tidak normal.**

| Keadaan | Yang dilihat petugas |
| --- | --- |
| *Diterima*, belum dikonfirmasi | Proses redup: *"Pesanan ini belum dikonfirmasi. Konfirmasi dan pilih dokter pemeriksa sebelum memproses."* |
| Sudah dikonfirmasi | Konfirmasi redup: *"Pesanan ini sudah dikonfirmasi. Konfirmasi hanya sah sekali."* |
| *Sedang Dikerjakan*, *Selesai*, *Dibatalkan* | Konfirmasi redup: *"Konfirmasi hanya sah pada pesanan berstatus Diminta atau Diterima yang belum diproses."* |
| Tanpa `LabOrder : Confirm` | Konfirmasi redup: *"Anda tidak memiliki izin mengonfirmasi pesanan laboratorium."* |
| Daftar basi, Proses ditekan, backend `409` `VAL-151` | Dialog Proses tetap terbuka; kalimat backend + *"Muat ulang daftar untuk melihat status terbaru."*; baris tidak berubah |

**Perubahan status.**

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat |
| --- | --- | --- | --- | --- |
| `Requested` | Konfirmasi | `Confirmed` | `LabOrder : Confirm` | Belum pernah dikonfirmasi; dokter pemeriksa aktif |
| `Accepted` | Konfirmasi | `Accepted` (tetap) | `LabOrder : Confirm` | Belum pernah dikonfirmasi; dokter pemeriksa aktif |
| `Accepted` | Proses Pemeriksaan | `InProcess` | `LabOrder : Process` | Sudah dikonfirmasi (`VAL-151`) |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Roadmap `FE-LAB-50`; kontrak `r40` 35, `r8` 10, `r17` `VAL-151`; backend `LabOrderService.cs#ConfirmAsync`,
`#StartProcessAsync`, `#GetDetailAsync`, `LabOrderDtos.cs`, `RoleAccessController.cs`, `DoctorController.cs`,
`Program.cs` (kebijakan `KioskRead`); frontend aturan konfirmasi/proses, hook, slice, kolom, view.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/hooks/.../lab-confirmation-rules.js` | `STATUS_DAPAT_DIKONFIRMASI` = `Requested`, `Accepted`; `canConfirmOrder` membacanya; **baru** `CONFIRM_HINT` dan `resolveConfirmAction(row, { allowed })` — izin lebih dulu, lalu sudah dikonfirmasi, lalu status. +40 −6 |
| `src/lib/hooks/.../lab-order-process-rules.js` | `resolveStartProcessAction` mewajibkan `isOrderConfirmed(row)` (jejak `confirmedAt`, bukan status); `PROCESS_HINT.notConfirmed`; komentar `400`/`409` diperbarui untuk `BE-LAB-90`. +16 −5 |
| `src/lib/hooks/.../use-lab-monitoring.jsx` | `usePermission("LabOrder", "Confirm")` → `canConfirmLabOrder`. +8 |
| `src/components/view/.../lab-monitoring-view.jsx` | Meneruskan `canConfirmLabOrder` ke kolom. +5 |
| `src/components/view/.../lab-monitoring-table-columns.jsx` | Butir Konfirmasi memakai `resolveConfirmAction`; urutan butir Terima Sampling → Konfirmasi → Proses. +22 −20 |
| `src/lib/state/slice/.../lab-monitoring-slice.jsx` | Komentar reducer konfirmasi saja (status tetap `Accepted`); nol perubahan perilaku. +3 −2 |
| `tests/unit/lab-confirmation-rules.test.mjs` | S6 diubah artinya: `Accepted` belum dikonfirmasi kini **sah**; 3 uji baru `FE-LAB-50`. +47 −2 |
| `tests/unit/lab-order-process-rules.test.mjs` | Empat kasus `FE-LAB-47` yang mengharapkan Proses terbuka kini memakai pesanan *Diterima* terkonfirmasi (diubah, bukan dihapus); 4 uji baru `FE-LAB-50`. +52 −7 |

### 3.3 Kepatuhan arsitektur frontend

Aturan murni di berkas aturan, izin lewat `usePermission` dengan konvensi *terbuka selama daftar izin belum
termuat*, butir menu lewat `RowActionMenu` yang sudah ada. Nol komponen, thunk, atau endpoint baru.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tidak berubah dari `FE-LAB-15`/`FE-LAB-47` — tombol simpan dialog berputar dan nonaktif |
| Kosong | Daftar kosong — tidak ada butir aksi |
| Gagal | Pesan backend di dalam dialog Konfirmasi atau Proses |
| Tanpa hak akses | Konfirmasi redup berpetunjuk izin; selama izin belum termuat backend tetap menjawab `403` |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Laboratory Management / Lab Order

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/v1/health-services/laboratory-management/lab-orders/{id}/confirm` | Konfirmasi | `LabOrder : Confirm` (sejak `r40`) |
| `PUT` | `/v1/health-services/laboratory-management/lab-orders/{id}/start-process` | Proses Pemeriksaan | `LabOrder : Process` |

**Kode status yang ditangani:** `200` — baris diperbarui dari jawaban; `409` (termasuk `VAL-151`) dan
`400` — kalimat backend + muat ulang; `403` — pesan backend.

---

## 6. Verifikasi

### 6.1 Otomatis

| Perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| ESLint 8 berkas yang disentuh | 0 error, 0 warning | `PASS` | Exit 0 |
| Uji terarah (konfirmasi, proses, selesaikan) | 35/35 lulus | `PASS` | 7 uji baru |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | 2506 uji: 2498 lulus, 8 gagal | `PASS` (nol kegagalan baru) | Kedelapan nama identik dengan baseline 2026-10-06, seluruhnya di luar Laboratorium |
| `npm run build` | `Compiled successfully in 54s` | `PASS` | Dijalankan saat nol server lokal hidup |

### 6.2 Langkah rilis `MVP-12c` langkah 2 di dev (izin user 2026-10-07)

| Langkah | Hasil |
| --- | --- |
| Pra-cek baca-saja | Jabatan *Penunjang Medis / Analis Laboratorium*: 31 kebijakan aktif, **0 tersembunyi** — mengirim ulang set utuh tidak menghapus apa pun. `policies/copy` tidak dapat dipakai karena belum ada jabatan sumber pemegang `Confirm` |
| Uji kering | `GET …/policies` = 31, `Confirm` belum ada |
| `POST …/role-access/policies` (superadmin) | `200`, `totalAllowed=32` |
| Bandingan set | Sesudah 32; **hilang 0**; tambahan tepat satu = `LabOrder : Confirm` |
| DB sesudah | Analis `LabOrder` = `Confirm`, `Hold`, `Process`, `Read`; satu-satunya pemegang `Confirm` |

### 6.3 Peramban — akun analis asli (`gilang.mahendra@rsmmc.local`), daftar Patologi Klinik

| Skenario | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| S1 `AC-287` — `000003` *Diterima* belum dikonfirmasi | Konfirmasi aktif; Proses redup berpetunjuk *belum dikonfirmasi*; urutan Buka Hasil › Terima Sampling › Konfirmasi › Proses Pemeriksaan › … | `PASS` | Menu baris |
| S2 AC tambahan (b) — daftar dibuat basi (`confirmedAt` palsu di peramban) | Proses terbuka; `PUT start-process` **asli** → `409` *"Pesanan ini belum dikonfirmasi…"*; dialog tetap, kalimat backend + muat ulang; baris tetap *Diterima* | `PASS` | Tangkapan layar; DB versi tidak berubah |
| S3 `AC-288` layar — `LabOrder : Confirm` dibuang dari daftar izin yang disuapkan (32 → 31) | Konfirmasi redup *"Anda tidak memiliki izin mengonfirmasi…"* | `PASS` | Menu baris |
| S4 AC tambahan (a) — Konfirmasi **sungguhan** `000003` | `POST confirm` → `200` atas nama analis; dialog tertutup; status **tetap** *Diterima*. **Namun** kolom Konfirmasi tetap *Belum Terkonfirmasi*, Konfirmasi tetap aktif, Proses tetap redup sampai daftar dimuat ulang | **`FAIL`** (sebab backend, T1) | Jawaban `200` membawa `confirmedAt: null` |
| S6a — sesudah muat ulang | Kolom Konfirmasi terisi; Konfirmasi redup *"…sudah dikonfirmasi…"*; Proses aktif | `PASS` | Daftar pantau membawa `confirmedAt` (`r14`) |
| S6b — Proses **sungguhan** `000003` | `PUT start-process` → `200`; baris *Sedang Dikerjakan* tanpa muat ulang dan sesudahnya | `PASS` | DB: `InProcess`, riwayat `Order.Confirm Accepted->Accepted`, `Order.StartProcess Accepted->InProcess`, versi 1 → 3 |
| Penjaga tulis | Hanya `confirm` (sekali) dan `start-process` (sekali basi, sekali nyata) `000003` yang diteruskan | `PASS` | Log skrip |

**Penggantian satu jawaban baca.** Dialog Konfirmasi memuat dokter dari
`GET /v1/corporate/human-resource/master-data/doctors/options`, yang menjawab **`403`** bagi analis (T2). Agar
tulis yang diizinkan tetap dapat dijalankan dengan akun analis, jawaban `403` itu diganti di peramban dengan
jawaban **asli** endpoint yang sama milik superadmin. Dokter yang dipilih: dokter nyata pertama di daftar
(*dr. Aditya Pranata, Sp.PK*). Permintaan `confirm` sendiri tidak disentuh.

### 6.4 HTTP — akun analis

| Panggilan | Hasil | Arti |
| --- | --- | --- |
| `PUT /lab-orders/{000002}/cancel` | `403` | Pemegang `Confirm` tanpa `Update` tidak dapat membatalkan (`AC-288`). `000002` `InProcess` dipilih supaya tetap ditolak walau izin lolos |
| `PUT /lab-orders/{000002}/pathology-context` | `403` | Tidak dapat menulis konteks klinis (`AC-288`) |
| `GET /lab-orders/{000003}` (superadmin) sesudah Konfirmasi | `200`, `confirmedAt`, `confirmedByName`, `examinerDoctorName`, `confirmedByUserId`, `examinerDoctorId` **semuanya `null`** padahal DB terisi | Bukti T1 |
| `GET …/doctors/options` | `403` | Bukti T2 |

**Lingkungan:** backend lokal Development (https 7184) dari biner `BE-LAB-90`; `next dev` port 3000 ke
backend lokal; DB dev bersama. Server dimatikan sesudah uji; port 3000 dan 7184 bebas. Sandi akun analis hanya
dipakai di berkas sementara scratchpad dan sudah dihapus.

Uji manual: `PASS` kecuali S4 (`FAIL`, sebab backend).

AUTOMATED TEST: PASS (7 uji baru; nol kegagalan baru).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-287` — Konfirmasi aktif pada `Requested` dan `Accepted` belum dikonfirmasi; Proses redup beralasan pada `Accepted` belum dikonfirmasi; status baris tidak berubah sebelum `200` | Terpenuhi | S1, S2, S4 (status tetap), uji unit |
| `AC-288` — pemegang Konfirmasi tanpa `Update` dapat mengonfirmasi, tidak dapat membatalkan maupun menulis konteks klinis; pemegang `Update` saja tidak dapat mengonfirmasi | Terpenuhi, kecuali butir terakhir | `confirm` `200` dan dua `403` atas nama analis; S3. Butir *pemegang `Update` saja* hanya dibuktikan harness `BE-LAB-90` — dev nol akun seperti itu |
| Tambahan (a) — sesudah Konfirmasi `200` pada *Diterima*: baris tetap *Diterima*, kolom terisi, Proses dapat ditekan, tanpa muat ulang | **Belum** | Status tetap *Diterima* terbukti; kolom dan Proses baru benar sesudah muat ulang (T1) |
| Tambahan (b) — `409` `VAL-151` dari daftar basi tampil di dialog, baris tidak berubah | Terpenuhi | S2 dengan jawaban **asli** |
| DoD — uji unit hijau, lint dan build hijau, laporan | Terpenuhi | Bagian 6.1 |

---

## 8. Temuan

### T1 — Jawaban detail pesanan tidak membawa ruas konfirmasi (backend, memblokir AC tambahan (a))

`LabOrderService.GetDetailAsync` — dipakai `GET /lab-orders/{id}` dan jawaban `confirm`/`start-process` — tidak
memetakan `ConfirmedAt`, `ConfirmedByName`, `ExaminerDoctorName` (warisan `LabOrderListResponse`) maupun
`ConfirmedByUserId`, `ExaminerDoctorId`. Kontrak `r13` (`approved` 2026-09-16) menjanjikan kelimanya. Selama
Konfirmasi selalu memindahkan status ke `Confirmed`, cacat ini tertutup karena `isOrderConfirmed` juga membaca
status `Confirmed`. Sejak urutan v1, Konfirmasi pada *Diterima* membiarkan status `Accepted`, sehingga layar
tidak punya bukti bahwa pesanan sudah dikonfirmasi sampai daftar dimuat ulang.

**Usulan:** task backend kecil — lima ruas ditambahkan ke proyeksi `GetDetailAsync` (aditif, nol migration,
nol perubahan kontrak karena `r13` sudah menetapkannya). Frontend tidak perlu berubah: reducer sudah menyalin
kelima ruas itu dari jawaban. Sesudahnya S4 diulang pada pesanan *Diterima* lain yang belum dikonfirmasi.
Tidak dikerjakan di sini karena di luar wewenang task frontend.

### T2 — Analis tidak dapat memuat daftar dokter pemeriksa (memblokir Konfirmasi lewat layar)

Pemilih dokter di dialog Konfirmasi (`FE-LAB-15`) memanggil `GET …/human-resource/master-data/doctors/options`,
yang dijaga kebijakan Identity `KioskRead` (`Program.cs`): hanya peran SuperAdmin, Administrator, Kiosk, atau
akun kios. Kebijakan ini **tidak dapat** dibuka lewat Akses Role. Akibatnya **analis — dan setiap pengguna
klinis biasa — tidak dapat memilih dokter pemeriksa, sehingga tidak dapat mengonfirmasi lewat layar** walau
sudah memegang `LabOrder : Confirm`. Cacat ini sudah ada sejak `FE-LAB-15`, tetapi tidak pernah terlihat karena
Konfirmasi selama ini hanya diuji dengan superadmin.

**Perlu keputusan sebelum `MVP-12c` langkah 3 (deploy frontend):** jalur baca dokter pemeriksa untuk pemegang
`LabOrder : Confirm` — misalnya endpoint pilihan dokter di modul Lab, atau pemakaian ulang jalur dokter lain
yang dijaga Akses Role. Pilihan endpoint mengubah `LAB-API-v1`, sehingga butuh amandemen kontrak.

---

## 9. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Nol pada berkas yang disentuh |
| Masalah yang diketahui | `NONE` — T1 dan T2 ditutup (bagian 9) |
| Dependency backend | `BE-LAB-90` ⚠ (belum di-commit). Usulan task backend untuk T1; keputusan dan amandemen kontrak untuk T2 |
| Perubahan sampingan | `NONE` |
| Data dev | `LabOrder : Confirm` diberikan ke jabatan Analis (31 → 32). `LAB-RSMMC-000003`: dikonfirmasi analis (dokter pemeriksa *dr. Aditya Pranata, Sp.PK*), lalu `InProcess`. `000002` tidak berubah (dua `403`, satu `409` sebelum menulis). **Kini nol pesanan `Accepted` yang belum dikonfirmasi di dev** |
| Interupsi | `NONE` |
| Status Git | Frontend: delapan berkas `M`, belum di-commit |
| Langkah berikutnya | `NONE` — task selesai; T1/T2 sudah diperbaiki (`BE-LAB-91`, `FE-LAB-51`) |

---

## 9. Penuntasan batas verifikasi (2026-10-21)

| Hal | Isi |
| --- | --- |
| T1 | Ditutup `BE-LAB-91` — jawaban `confirm`/detail kini membawa `confirmedAt`, `confirmedByName`, `examinerDoctorName` |
| T2 | Ditutup `BE-LAB-91`/`FE-LAB-51` — pemilih dokter memakai `GET /lab-orders/examiner-doctor-options` (`LabOrder : Confirm`); Gilang memilih dokter tanpa `403` |
| Pesanan uji | `LAB-RSMMC-000028` — kunjungan IGD pasien uji 00-00-00-17, Kalium (data uji `MVP-15e`, izin tulis pemilik modul 2026-10-21). Satu pesanan dibawa melewati seluruh alur v1 untuk menuntaskan `FE-LAB-46`, `48`, `50`, dan `51` sekaligus |
| Akun | Gilang (Analis) dan dr. Bima (Kepala Instalasi) — akun asli; sandi tidak dicatat. Superadmin hanya pada satu langkah antara (lihat di bawah) |
| 1. Rencana wadah — layar Wadah (Gilang) | `POST /lab-specimens/by-order/{GUID pesanan ini}` → `201`; wadah *Direncanakan* menopang Kalium; kartu tampil tanpa muat ulang |
| 2. Diambil → diterima (Gilang), layak (petugas kedua) | `200`/`200`; *layak* oleh Gilang ditolak `403` *"Petugas yang mengambil sampel tidak boleh menyatakan kelayakannya"* (empat mata, `VAL-09`) → dinyatakan layak oleh superadmin sebagai pengganti analis kedua (sandi Vina tidak tersedia) → pesanan *Diterima*, belum dikonfirmasi |
| 3. Konfirmasi — Daftar Pasien Lab PK (Gilang) | Dialog kosong, Simpan redup (`AC-291`); dokter dipilih dari daftar Lab; `POST confirm` → `200`. **Tanpa muat ulang:** kolom Dokter Pemeriksa *dr. Aditya Pranata, Sp.PK*, kolom Konfirmasi *Gilang Mahendra · 21 Okt 2026 12.18*, status **tetap** *Diterima*, Konfirmasi redup, **Proses Pemeriksaan aktif**. Bukti: tangkapan layar sesudah Simpan dan sebelum muat ulang (toast *"Pesanan berhasil dikonfirmasi."* masih tampil) dan teks baris sesudah muat ulang. Dua cek teks skrip sempat `FAIL` karena masih mencari awalan pra-v1 *"Dokter pemeriksa:"* — sejak `r47` dokter pemeriksa punya kolom sendiri; bukan cacat layar |
| 4. Proses Pemeriksaan — layar (Gilang) | `PUT start-process` → `200`; baris *Sedang Dikerjakan* |
| 5. Hasil (langkah antara, API) | Gilang mengisi Kalium 4,1 dan memfinalkan → `200`; dr. Bima memvalidasi → `200`; rilis oleh validator yang sama ditolak `422` (empat mata) → dr. Bima merilis dengan alasan pengecualian resmi *"Shift tunggal, tidak ada dokter lain bertugas"* → `200`; pesanan *InProcess*, `resultProgress` *AllReleased* |
| 6. Selesaikan — layar (Gilang) | Selesaikan aktif; `PUT complete` → **`200` sungguhan**; dialog tertutup; baris langsung *Selesai* tanpa muat ulang; server `Completed` |
| Hasil untuk task ini (S4 diulang) | AC tambahan (a) `PASS`: sesudah Konfirmasi sungguhan baris **Diterima** langsung terbaca dikonfirmasi, Konfirmasi redup, Proses aktif — tanpa muat ulang. Langkah 4 (Proses) `200` membuktikan urutan v1 berlanjut |
