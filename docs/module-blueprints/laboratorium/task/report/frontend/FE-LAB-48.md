# Laporan Perubahan Frontend — `FE-LAB-48`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-LAB-48` |
| Judul | Selesaikan pada daftar Patologi Klinik dan Mikrobiologi |
| Slice | Susulan `S15` (daftar pantau) — amendment pass putaran 22 |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian *Susulan putaran 22* |
| Trace | BR-136 butir 3, 4; `LAB-DEC-190`, `LAB-DEC-154`; **`Q-P22-01` (2026-10-07) — menutup `LAB-CONFLICT-017`**; capability map rev 7 `CAP-P22-02`, `-05`, `-07`, `-21` |
| Contract version | `LAB-API-v1` `r39` bagian 31 (`PUT /lab-orders/{id}/complete`, 31.3 rincian penolakan); `LAB-VAL-v1` `r16` `VAL-146`; `LAB-STATE-v1` `r7` bagian 9 — `approved`, tidak diamandemen |
| Wewenang UI | `LAB-FE-030` `DEV_DISCRETION` dibatasi BR-136 — yang dipakai: ikon, letak sesudah Proses Pemeriksaan, nonaktif beralasan, bentuk daftar penahan di dalam dialog. Yang **tidak** dilanggar: Selesaikan nol di daftar Patologi Anatomi; tidak dapat ditekan sebelum `AllReleased` (kecuali kosong, bunyi amandemen `AC-278`); baris berubah hanya dari `200` |
| Dependency | `FE-LAB-47` ✅ (pembaca galat membawa status dan `errors`; pola dialog). Backend: nol — `BE-LAB-81` sudah berdiri |
| Klasifikasi | `MEDIUM` — 5 berkas diubah, 1 uji baru; satu thunk dan keadaan Redux baru; satu dialog baru; nol endpoint baru |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev`; laporan ini beserta status roadmap dan traceability |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `5427ddbe4` (branch `YogaV2`), di atas `FE-LAB-46` dan `FE-LAB-47` yang belum di-commit |
| Commit backend yang dijadikan rujukan | `171dc314` (branch `yoga`) |
| Tanggal | 2026-10-07 |
| Status | ⚠ **`SELESAI DENGAN BATAS VERIFIKASI`** — `AC-278` (amandemen) dan `AC-279` terbukti di peramban, `AC-279` dengan penolakan **asli** backend. **Penyelesaian sungguhan (`200`) belum pernah dijalankan**: dev nol pesanan `InProcess` yang seluruh pemeriksaannya dirilis; jalur `200` dibuktikan dengan jawaban tiruan berbentuk detail asli |

---

## 1. Keadaan yang ditemukan di awal

| Bukti | Isi |
| --- | --- |
| Frontend | Nol pemanggil `PUT …/complete` |
| Backend `CompleteAsync` | `InProcess` + seluruh pemeriksaan terhitung dirilis → `Completed`; ada yang belum dirilis → `409` `VAL-146` dengan `errors.details`; nol pemeriksaan terhitung → **diterima** (rancangan 22.7 butir 3); status lain → `409` tanpa rincian |
| Baris daftar | `resultProgress` terisi bagi PK dan Mikro (`InProgress`/`AllReleased`), kosong bila nol pemeriksaan terhitung, kosong bagi PA |
| `AC-278` bunyi lama | Hanya `AllReleased` — akan mengunci pesanan tanpa pemeriksaan terhitung (`LAB-CONFLICT-017`). Diamandemen pemilik modul 2026-10-07: **`AllReleased` atau kosong** |

---

## 2. Proses bisnis dari sisi pengguna

**Pengguna:** analis laboratorium (pemegang `LabOrder : Process`).

1. Analis membuka *Pemeriksaan Patologi Klinik* atau *Pemeriksaan Mikrobiologi*, lalu menekan tombol aksi (⋮).
2. **Selesaikan** hanya dapat ditekan bila pesanan *Sedang Dikerjakan* **dan** seluruh pemeriksaannya sudah
   dirilis — atau pesanan itu tidak punya lagi pemeriksaan yang dihitung karena seluruhnya batal/gugur.
3. Dialog menyebut pasien, nomor pesanan, dan pemeriksaannya. *Selesaikan* mengirim satu permintaan.
4. Berhasil → baris berbunyi *Selesai* dan butir Selesaikan redup.

**Contoh.** Pesanan Hemoglobin *Sedang Dikerjakan* berisi Hemoglobin (dirilis), Leukosit (*Tervalidasi*),
dan Urinalisis Protein (*Draft*). Selesaikan redup: *"Masih ada pemeriksaan yang belum dirilis."* Bila
daftar di layar sudah basi dan petugas tetap menekannya, backend menolak, dan dialog menulis:

> Order belum dapat diselesaikan karena masih terdapat pemeriksaan yang belum dirilis.
> **Pemeriksaan yang menahan** — Leukosit — Tervalidasi · Urinalisis Protein — Draft

**Jalur tidak normal.**

| Keadaan | Yang dilihat petugas |
| --- | --- |
| Belum *Sedang Dikerjakan* | Redup: *"Selesaikan hanya sah pada pesanan berstatus Sedang Dikerjakan."* |
| Masih ada pemeriksaan belum dirilis | Redup: *"Masih ada pemeriksaan yang belum dirilis."* |
| Nilai keadaan hasil tidak dikenal | Redup: *"Keadaan hasil pesanan belum terbaca. Muat ulang daftar."* |
| Daftar basi, ditolak `VAL-146` | Dialog tetap; pesan backend + daftar setiap pemeriksaan penahan beserta keadaannya; baris tidak berubah |
| Status sudah berubah (`409` tanpa rincian) | Pesan backend + *"Muat ulang daftar untuk melihat status terbaru."* |
| Tanpa `LabOrder : Process` | Redup: *"Anda tidak memiliki izin memproses pesanan laboratorium."* |
| Daftar Patologi Anatomi | Butir Selesaikan **tidak ada** sampai `S4e` |

**Perubahan status.**

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat |
| --- | --- | --- | --- | --- |
| `InProcess` | Selesaikan | `Completed` | `LabOrder : Process` | Setiap pemeriksaan terhitung dirilis, atau nol pemeriksaan terhitung (`VAL-146`) |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Roadmap `FE-LAB-48`; capability map rev 7 (`LAB-CONFLICT-017`); `api-contract.md` 31.3; backend
`LabOrderController.cs#Complete`, `#ExecuteAsync`, `LabOrderService.cs#CompleteAsync`,
`#ReadCompletionBlockersAsync`, `LabOrderDtos.cs#LabOrderCompletionBlockedItem`,
`LabOrderResultProgressRules.cs`; berkas frontend hasil `FE-LAB-47`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/hooks/.../lab-order-process-rules.js` | `resolveCompleteAction` (`InProcess` + `AllReleased`/kosong; `InProgress` dan nilai asing nonaktif; izin lebih dulu), `readCompletionBlockers` (`errors.details` `VAL-146`, bentuk asing → kosong), `buildCompleteFailureMessage` |
| `tests/unit/lab-order-complete-rules.test.mjs` | **Baru.** 9 uji, termasuk contoh 31.3 dan kasus `LAB-CONFLICT-017` |
| `src/lib/services/.../lab-order.service.js` | `completeLabOrder(labOrderId)` — `PUT …/complete` tanpa badan |
| `src/lib/state/slice/.../lab-monitoring-slice.jsx` | Thunk `submitLabOrderComplete`; keadaan `completeLoading`/`completeError`/`completeErrorStatus`/`completeErrors` per disiplin; `clearLabOrderCompleteError`; baris diperbarui lewat `mergeOrderIntoRow` hanya dari `200` |
| `src/lib/hooks/.../use-lab-monitoring.jsx` | Dialog Selesaikan (`openComplete`, `closeComplete`, `submitComplete` — diabaikan selama mengirim); `completeBlockers` dibaca dari `errors` |
| `src/components/view/.../lab-monitoring-table-columns.jsx` | Butir *Selesaikan* lewat callback opsional `onComplete` — nol dirender bila tidak dikirim |
| `src/components/view/.../lab-monitoring-view.jsx` | `onComplete` hanya untuk PK dan Mikro; dialog *Selesaikan Pesanan* dengan pengantar, ringkasan, pesan, dan daftar *Pemeriksaan yang menahan* |

### 3.3 Kepatuhan arsitektur frontend

Pola `FE-LAB-47` diikuti persis: aturan murni di berkas aturan, thunk dan keadaan per disiplin di slice,
dialog `ConfirmModal` dengan isi sebagai anak. Pembatasan per disiplin memakai pola callback opsional
`onOpenResult`, sehingga susunan kolom tetap **satu** untuk ketiga daftar. Nol komponen dasar baru.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tombol *Selesaikan* berputar dan nonaktif; klik kedua tidak mengirim apa pun |
| Kosong | Daftar kosong — tidak ada butir aksi |
| Gagal | Pesan di dalam dialog; `VAL-146` disertai daftar pemeriksaan penahan |
| Tanpa hak akses | Butir redup berpetunjuk izin; selama izin belum termuat backend tetap menjawab `403` |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Laboratory Management / Lab Order

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `PUT` | `/v1/health-services/laboratory-management/lab-orders/{id}/complete` | Selesaikan | `LabOrder : Process` |

**Kode status yang ditangani:** `200` — baris diperbarui dari `LabOrderDetailResponse`; `409` dengan
`errors.code = LAB_ORDER_COMPLETION_BLOCKED` — daftar penahan; `409`/`400` tanpa rincian — muat ulang;
`403` — tanpa izin.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| ESLint 7 berkas yang disentuh | 0 error, 0 warning | `PASS` | Keluaran kosong |
| `npm run lint:errors` | Nol error | `PASS` | Keluaran kosong |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | 2492 uji: 2484 lulus, 8 gagal | `PASS` (nol kegagalan baru) | 9 uji baru lulus; kedelapan nama kegagalan identik dengan baseline 2026-10-06, di luar Laboratorium |
| `npm run build` | `Compiled successfully in 47s` | `PASS` | Dijalankan saat nol server lokal hidup |
| S1 — butir per daftar | Ada di PK dan Mikro; **tidak ada** di PA | `PASS` | Isi `RowActionMenu` ketiga daftar |
| S2 — keadaan per baris PK dibanding data asli | Aktif tepat pada `InProcess` + `AllReleased`/kosong; `000001` dan `000002` (`InProcess`/`InProgress`) redup *"Masih ada pemeriksaan…"*; status lain redup berpetunjuk status | `PASS` | 6 baris; data asli dari respons daftar |
| S3 — `AC-279` dengan penolakan **asli** | Daftar dibuat basi (`AllReleased`) di peramban; `PUT complete` `LAB-RSMMC-000001` **diteruskan sekali** → backend `409` `LAB_ORDER_COMPLETION_BLOCKED`, penahan *Leukosit — Tervalidasi*, *Urinalisis Protein — Draft*; layar menampilkan **keduanya persis**; baris tetap *Sedang Dikerjakan* | `PASS` | Diteruskan hanya karena daftar asli menyatakan `InProgress` — backend menolak sebelum menulis (`CompleteAsync`) |
| S4 — `200` tiruan + klik ganda | Tepat satu `PUT`; dialog tertutup; baris *Selesai*; Selesaikan redup | `PASS` | Detail asli diambil, `orderStatus` diubah `Completed` |
| S5 — `InProcess` + `resultProgress` kosong | Selesaikan **dapat ditekan** (`LAB-CONFLICT-017`) | `PASS` | Daftar disunting di peramban |
| S6 — izin `LabOrder : Process` dicabut | Redup *"Anda tidak memiliki izin…"* | `PASS` | Daftar izin disuapkan |
| S7 — Mikrobiologi | Butir ada, redup *"…hanya sah pada pesanan berstatus Sedang Dikerjakan."* (seluruh pesanan Mikro dev `Requested`) | `PASS` | Menu baris pertama |
| S8 — keadaan dev sesudah uji | `000001` tetap `InProcess`/`InProgress` | `PASS` | Respons daftar asli |
| Penjaga tulis, galat halaman | Nol tulis lain; `complete` diteruskan tepat sekali dan ditolak; nol galat JavaScript | `PASS` | Log skrip |
| Penyelesaian sungguhan (`200`) | — | `NOT RUN` | Dev nol pesanan `InProcess` yang seluruh pemeriksaannya dirilis; menyiapkannya berarti memvalidasi dan merilis hasil di DB bersama |

**Lingkungan:** backend lokal Development (https 7184), `next dev` port 3000 ke backend lokal, DB dev
bersama. **Akun:** superadmin sebagai pengganti analis; skenario tanpa izin lewat daftar izin yang
disuapkan. Server dimatikan sesudah uji; port 3000, 7184, 5107 bebas.

Uji manual: `PASS` — kecuali penyelesaian sungguhan (`NOT RUN`, alasan di atas).

AUTOMATED TEST: `node --import ./tests/helpers/register.mjs --test tests/unit/` — PASS (9 uji baru lulus; nol kegagalan baru).

**Tidak dijalankan:** penyelesaian sungguhan; akun analis asli.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-278` (amandemen 2026-10-07) — tampil di PK dan Mikro, tidak di PA; dapat ditekan hanya bila `InProcess` dan `resultProgress` = `AllReleased` **atau kosong**; satu pemeriksaan belum dirilis → nonaktif | Terpenuhi | S1, S2, S5, S7, uji unit |
| `AC-279` — penolakan `409` menampilkan pemeriksaan yang menahan; status baris tidak berubah | Terpenuhi | S3 dengan jawaban **asli** backend |
| Tambahan — tidak dapat ditekan tanpa `LabOrder : Process` | Terpenuhi | S6, uji unit |
| DoD — uji unit hijau, lint dan build hijau, laporan | Terpenuhi | Bagian 6 |
| Risiko (d) roadmap — task ini menjadi bukti pertama jalur `200` `complete` | **Belum** | Hanya tiruan; menunggu pesanan `InProcess` yang seluruh hasilnya dirilis |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Nol pada berkas yang disentuh |
| Masalah yang diketahui | Sama dengan `FE-LAB-47`: pengantar dialog Konfirmasi/Batalkan lama tidak tampil; label `Confirmed` belum diterjemahkan — di luar cakupan |
| Dependency backend | `NONE`. Catatan: penolakan status selain `InProcess` sudah `409` di backend (`BE-LAB-81`), berbeda dari `start-process` (`400`, `BE-LAB-89`) |
| Perubahan sampingan | `NONE` |
| Data dev | Nol perubahan — satu `PUT complete` asli ditolak `409` sebelum menulis |
| Interupsi | `NONE` |
| Status Git | Frontend: enam berkas `M`, tiga berkas baru `??` — gabungan `FE-LAB-46`..`48`, belum di-commit |
| Langkah berikutnya | `FE-LAB-49` (menu dan judul). Untuk menutup batas verifikasi: rilis Leukosit dan Urinalisis Protein `LAB-RSMMC-000001` lewat Halaman Hasil, lalu Selesaikan sungguhan |
