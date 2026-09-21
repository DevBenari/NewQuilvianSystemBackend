# Laboratorium — Laporan Kesiapan Modul

| Field | Value |
|---|---|
| `blueprint_id` | `LAB-BP-001` |
| `readiness_id` | `LAB-RDY-001` |
| Revision | `4` |
| Status | `draft` |
| Tanggal | 2026-09-16 |
| **Verdict** | **`READY_WITH_CONDITIONS`** — untuk slice MVP Rilis 1 yang sudah dibangun. Empat kondisi; **`C-01` dan `C-02` ditutup 2026-09-16**. Dua tersisa (`C-03`, `C-04`), **keduanya di luar wewenang modul Laboratorium** |
| Sifat | **Read-only.** Nol baris source aplikasi diubah saat audit ini |
| Cakupan verdict | `S1a`, `S2`, `S3`, `S7`, `S10`, `S11`, `S13a`, `S13b`, `S14`, `S15` |
| Di luar cakupan verdict | `S17` (Menu Hasil) dan `S4`..`S6` — tertahan `LAB-SIGN-001`, sudah tercatat `traceability` bagian `4.1c` |
| Backend SHA saat audit | **`13665452`** |
| Frontend SHA saat audit | **`686038858`** |
| Masukan | Decisions rev `36`; manifest rev `38`; reconciliation rev `4`; traceability rev `39`; capability map rev `3` |
| Contract berlaku | `LAB-API-v1` r15, `LAB-STATE-v1` r3, `LAB-VAL-v1` r6, `LAB-INT-v1` r3, `LAB-PERM-v1` rev 5 — seluruhnya `approved` |

---

## 1. Ringkasan untuk yang tidak punya waktu

Modul Laboratorium **berdiri kokoh pada apa yang dibangunnya**, dan **lemah pada kemampuan
membuktikannya kembali**.

Backend berkompilasi bersih, kontrak yang tertulis benar-benar ada di kode, dan aturan validasi
yang dijanjikan benar-benar ditegakkan — bukan sekadar didokumentasikan. Frontend punya 16 berkas
uji berisi **158 pemeriksaan yang seluruhnya lulus hari ini**.

Yang menjadi persoalan: **seluruh bukti uji backend tidak dapat dijalankan ulang dari
repository**. Sejak 2026-09-11, berkas uji dikecualikan dari repository lewat `.gitignore`.
Traceability mencatat 70 baris berstatus `SELESAI` yang sebagian besar bersandar pada laporan
task yang mengutip uji itu — dan uji itu tidak ada lagi di sini. Klaimnya tidak terbukti salah;
ia menjadi **tidak dapat diperiksa**.

Satu cacat nyata ditemukan pada kode yang sudah berjalan: penyaring tanggal di tiga menu
Pemeriksaan menerima tanggal masa depan dan rentang terbalik tanpa penolakan apa pun.

Satu hal justru **melegakan**: `LAB-DEC-062` — penguncian tombol Proses oleh status pembayaran —
**tidak diimplementasikan diam-diam**. Ia tertahan `LAB-COORD-010`, dan kodenya memang kosong.
Penahan dihormati, bukan dipalsukan.

---

## 2. Skor per Dimensi

| Dimensi | Bobot | Skor | Bukti | Gap/blocker |
|---|---:|---:|---|---|
| **Fondasi** | 20% | **10/10** | Blueprint lengkap: manifest rev 38, decisions rev 36, 5 kontrak `approved`, roadmap BE+FE, traceability rev 39. `input_hashes` diverifikasi **cocok** dengan isi berkas pada hari audit | ~~`C-01`~~ — ✅ ditutup 2026-09-16, manifest rev 40 |
| **Backend** | 25% | **8/10** | `dotnet build -p:RunAnalyzers=False` → **`Build succeeded, 0 Warning(s), 0 Error(s)`** pada `13665452`. Kontrak terverifikasi ada di kode: `LabOrderController.cs:328` (`POST /{id}/confirm`, r12), `:383` (`PUT /{id}/cancel`, r12), `LabOrderDtos.cs:330` (`OrderedProcedures`, r15) | Tidak ada gap fungsional yang ditemukan pada cakupan audit |
| **Frontend** | 25% | **9/10** | Tujuh lapis wajib terisi; 16 berkas uji Laboratorium; **158/158 pemeriksaan lulus**, dijalankan 2026-09-16 pada `686038858` | ~~`C-02`~~ — ✅ ditutup 2026-09-16 lewat `FE-LAB-18` |
| **Integrasi/runtime** | 15% | **4/10** | `MstPatient.WhatsAppNumber` tersedia; `QRCoder 1.8.0` tersedia | `C-03` — `LAB-COORD-010` dan `LAB-COORD-011` keduanya belum ada. **Bukan regresi**; keduanya memang belum pernah ada |
| **Verifikasi** | 15% | **3/10** | Sisi frontend **dapat dijalankan ulang** dan sudah dijalankan — 999 uji unit ditambah 2 pemeriksaan layar Playwright. Sisi backend **tidak** | `C-04` — `/Tests/` dikecualikan `.gitignore` sejak `fcabdff9` (2026-09-11). **Diperiksa ulang 2026-09-16: berkas ujinya tidak ada pula secara lokal**, sehingga tidak dapat dijalankan bahkan di mesin ini |

**Skor tertimbang: 7,7/10** — naik dari 7,0 sesudah `C-01` dan `C-02` ditutup 2026-09-16. **Dua kondisi tersisa, keduanya di luar wewenang modul Laboratorium.**

---

## 3. Bukti yang Dikumpulkan

### 3.1 Backend berkompilasi bersih

```
dotnet build -p:RunAnalyzers=False --nologo -v q
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

Dijalankan 2026-09-16 pada `NewQuilvianSystemBackend@13665452`.

### 3.2 Kontrak benar-benar ada di kode, bukan hanya di dokumen

Tiga amandemen terbaru diperiksa satu per satu:

| Kontrak | Yang dijanjikan | Yang ditemukan |
|---|---|---|
| `LAB-API-v1` r12 | `POST /lab-orders/{id}/confirm` | **Ada** — `LabOrderController.cs:328` |
| `LAB-API-v1` r12 | `PUT /lab-orders/{id}/cancel` diperketat | **Ada** — `LabOrderController.cs:383` |
| `LAB-API-v1` r15 | `orderedProcedures` pada detail pesanan | **Ada** — `LabOrderDtos.cs:330` |

### 3.3 Aturan validasi ditegakkan, bukan sekadar ditulis

`VAL-74` dan `VAL-75` diperiksa sampai ke baris penegaknya pada `LabOrderService.cs`:

- **`VAL-75`** (`:946`) — pembatalan ditolak di luar `Requested`/`Confirmed`.
- **`VAL-74`** (`:963-968`) — `CancelReason` dipangkas lalu ditolak bila kosong, dengan pesan
  `"Alasan pembatalan wajib diisi."`

**Satu catatan yang perlu dibaca benar.** Ruas `CancelReason` pada DTO (`LabOrderDtos.cs:186`)
bertipe `string?` — **nullable**. Dibaca sendirian, itu terlihat seperti `VAL-74` tidak
ditegakkan. Ia **ditegakkan di service**, dengan pesan dan jalur galat yang benar. Nullable pada
DTO justru yang membuat pesan galatnya rapi, bukan `400` model binding yang tidak bercerita apa
pun. **Bukan gap.**

### 3.4 Uji frontend dijalankan, bukan dikutip

```
node --import ./tests/helpers/register.mjs --test tests/unit/lab-*.test.mjs …
# tests 158
# pass 158
# fail 0
```

16 berkas uji Laboratorium, dijalankan 2026-09-16 pada `686038858`.

### 3.5 Pergeseran SHA diukur, bukan diasumsikan

| Repo | Tercatat manifest | `HEAD` saat audit | Commit terlewat | **Yang menyentuh Laboratorium** |
|---|---|---|---:|---|
| Backend | `e2152709` | `13665452` | 23 | **1** — `a6e74742 updates BE modul lab` (11 berkas) |
| Frontend | `9cd4cd03f` | `686038858` | 21 | **3** — seluruhnya `updates FE modul lab` (31 berkas) |

**Ini kabar baik yang pantas ditulis terang.** Dari 23 commit backend yang terlewat, **hanya
satu** menyentuh source Laboratorium, dan commit itu adalah pekerjaan modul ini sendiri. Bukti
capability map **tidak dirusak** oleh pekerjaan modul lain. Yang tertinggal pembukuannya, bukan
kesahihannya.

---

## 4. Kondisi yang Menyertai Verdict

### ~~`C-01`~~ — Kedua SHA pada manifest tertinggal — ✅ **ditutup 2026-09-16**

> **Ditutup sesudah audit ini selesai, bukan saat berjalan.** Manifest disegarkan ke
> `backend_commit_sha: 13665452` dan `frontend_commit_sha: 686038858` (manifest revision 40),
> beserta catatan bahwa hanya 1 commit backend dan 3 commit frontend menyentuh Laboratorium.
> Urutannya disengaja: penilai tidak boleh menyunting masukan yang sedang dinilainya.

| Aspek | Isi |
|---|---|
| **Dampak** | Rendah. Setiap pembaca berikutnya akan mengira bukti dipindai pada commit yang sudah bukan `HEAD` |
| **Bukti** | `backend_commit_sha: e2152709` versus `HEAD` `13665452`; `frontend_commit_sha: 9cd4cd03f` versus `686038858` |
| **Pemilik** | Yoga Aji Pratama |
| **Mitigasi** | Segarkan kedua nilai beserta catatan bahwa hanya 1 commit backend dan 3 commit frontend menyentuh Laboratorium. **Impact scan penuh tidak diperlukan** — sudah dibuktikan bagian 3.5 |
| **Kenapa tidak dikerjakan audit ini** | Audit ini read-only terhadap manifest. Menyunting masukannya sendiri saat menilai adalah konflik peran |

### ~~`C-02`~~ — Penyaring tanggal nol penjaga pada layar yang sudah dipakai — ✅ **ditutup 2026-09-16**

> **Ditutup lewat `LAB-DEC-074` dan `FE-LAB-18` pada hari yang sama.** Pemilik modul
> menetapkan aturan tanggal berlaku module-wide; perbaikannya ditempuh secara aditif — prop
> **opsional** `max` pada `FilterDatePicker` dengan bawaan tidak berubah, sehingga 132
> pemakai lain nol terdampak, dan itu **dibuktikan terbalik** lewat tiga asersi tersendiri.
> Bukti: 999/999 uji unit lulus, build produksi lulus, lint nol tambahan.
>
> **Ditutup juga atas dasar layar yang dilihat, bukan hanya uji unit:** 2 pemeriksaan
> Playwright lulus terhadap `.next/standalone`, dan `AC-99` diukur dengan menghitung
> permintaan yang tiba — nol permintaan berangkat selama rentangnya terbalik.

| Aspek | Isi |
|---|---|
| **Dampak** | **Sedang, dan terlihat pengguna.** Petugas dapat memilih tanggal 20 tahun ke depan dan rentang terbalik; keduanya diterima tanpa penolakan dan menghasilkan daftar kosong yang tidak menjelaskan sebabnya |
| **Bukti** | `filter-date-picker.jsx:413-419` — nol prop `max`, `min`, maupun `disabledDate`. `:21` — `ABSOLUTE_FORWARD_LIMIT = 20`. `lab-monitoring-view.jsx:191-203` — kedua `FilterDatePicker` dipasang tanpa penjaga, dan nol pemeriksaan `startDate <= endDate` di seluruh hook maupun view |
| **Melanggar** | `LAB-DEC-064` (tanggal masa depan tidak boleh) dan `LAB-DEC-071` (`Tgl Awal <= Tgl Akhir`) |
| **Pemilik** | Yoga Aji Pratama + pemilik komponen base frontend |
| **Mitigasi** | **Bukan perbaikan lokal Laboratorium.** `FilterDatePicker` dipakai **133 berkas** di seluruh aplikasi. Jalur teraman: tambah prop **opsional** `max` (default tidak berubah, 132 pemakai lain nol terdampak), lalu pakai di Laboratorium. Menuntut satu task frontend dan penetapan apakah `LAB-DEC-064`/`LAB-DEC-071` berlaku module-wide |
| **Catatan** | Kedua keputusan itu lahir dalam konteks `BR-50` (Menu Hasil, `S17`). Memberlakukannya pada menu Pemeriksaan yang sudah jalan adalah **perluasan cakupan**, dan itu keputusan pemilik — bukan kesimpulan audit |

### `C-03` — Dua jalur integrasi belum ada

| Aspek | Isi |
|---|---|
| **Dampak** | Tinggi bagi kemampuan yang bergantung padanya, **nol bagi yang sudah berjalan** |
| **Bukti** | `LAB-COORD-010` — Laboratorium nol kolom pembayaran dan nol jalur baca milik Billing. `LAB-COORD-011` — nol pengirim pesan dan nol pustaka PDF di seluruh backend |
| **Pemilik** | Pemilik `billing-kasir` (`LAB-COORD-010`); pemilik platform (`LAB-COORD-011`, lewat `LAB-REQ-007` bagian 4) |
| **Mitigasi** | Sudah diajukan. Tidak ada pekerjaan Laboratorium yang dapat mendahuluinya |
| **Temuan yang melegakan** | `LAB-DEC-062` **tidak diimplementasikan diam-diam**. Penelusuran `Lunas`, `paymentStatus`, dan `belum ditagihkan` pada seluruh hook dan view Laboratorium menghasilkan **nol**. Penahan dihormati, bukan ditambal dengan tebakan |

### `C-04` — Bukti uji backend tidak dapat dijalankan ulang dari repository

| Aspek | Isi |
|---|---|
| **Dampak** | **Tertinggi pada laporan ini** — dan paling mudah disalahpahami |
| **Bukti** | `fcabdff9 chore: exclude test projects from repository` (2026-09-11), sudah menjadi leluhur `HEAD`. `.gitignore:378` memuat `/Tests/`. Penelusuran berkas uji Laboratorium di backend: **0 berkas**. Satu-satunya `.csproj` adalah `QuilvianSystemBackend.csproj`. **Diperiksa ulang 2026-09-16 terhadap berkas tak-terlacak di mesin ini: `Tests/` pun tidak ada, dan nol `*Test*.csproj` di seluruh `QuilvianV2`** — jadi bukan sekadar tidak ter-commit, melainkan memang tidak ada di sini |
| **Yang TIDAK berarti** | **Ini bukan bukti bahwa uji itu tidak pernah ada atau tidak pernah lulus.** 40 laporan task backend mengutip hasilnya, dan sebagian besar ditulis sebelum pengecualian itu berlaku |
| **Yang berarti** | 70 baris `SELESAI` pada traceability bersandar pada bukti yang **tidak dapat diperiksa ulang** oleh siapa pun hari ini — termasuk CI. Regresi pada backend Laboratorium **tidak akan tertangkap** sampai ada yang menjalankan uji yang tidak ada di repository |
| **Pemilik** | Pemilik repository backend |
| **Mitigasi** | Keputusan tata kelola, bukan perbaikan kode: apakah pengecualian itu disengaja permanen, dan bila ya, di mana bukti uji backend disimpan supaya dapat diaudit. **Di luar wewenang modul Laboratorium** |

---

## 5. Kenapa Verdict-nya Bukan `READY`, dan Bukan `NOT_READY`

**Bukan `READY`** karena `READY` menuntut skenario acceptance **terbukti**. `C-04` membuat
sebagian besar pembuktian backend tidak dapat diperiksa dari repository. Menyebut modul ini
`READY` berarti menerima klaim yang tidak dapat diverifikasi sebagai bukti — persis kesalahan
yang diperingatkan pada kepala Skill ini: menyamakan build yang sukses dengan kesiapan.

**Bukan `NOT_READY`** karena tidak satu pun temuan menunjukkan **kerusakan**. Backend
berkompilasi bersih; kontrak yang dijanjikan ada; validasi yang dijanjikan ditegakkan sampai ke
pesan galatnya; 158 pemeriksaan frontend lulus; dan penahan yang belum terjawab **dihormati alih-alih
dipalsukan**. Satu-satunya cacat perilaku yang ditemukan (`C-02`) terlihat pengguna tetapi tidak
merusak data dan tidak menghilangkan pekerjaan.

Keempat kondisi punya pemilik dan mitigasi, dan tiga di antaranya berada **di luar wewenang modul
Laboratorium**. Itu tepat definisi `READY_WITH_CONDITIONS`.

---

## 6. Task Berikutnya

| # | Task | Jenis | Pemilik | Catatan |
|---|---|---|---|---|
| ~~1~~ | ~~Segarkan kedua `*_commit_sha` pada manifest~~ | Pembukuan | — | ✅ **Selesai 2026-09-16**, manifest rev 40. Impact scan penuh tidak dijalankan — bagian 3.5 sudah membuktikan cakupannya |
| ~~2~~ | ~~Tetapkan apakah `LAB-DEC-064`/`LAB-DEC-071` berlaku module-wide~~ | Keputusan pemilik | — | ✅ **Dijawab 2026-09-16** — berlaku module-wide, dibukukan `LAB-DEC-074` |
| ~~3~~ | ~~Satu task frontend: prop opsional `max` + penjaga rentang~~ | `build-module-frontend` | — | ✅ **Selesai 2026-09-16** sebagai `FE-LAB-18`, sesudah task 2 dijawab dan task-nya diturunkan ke roadmap lebih dulu |
| 4 | Jawab `LAB-REQ-007` dan `LAB-REQ-004` | Persetujuan | Clinical Governance, platform, kepala instalasi | `C-03`, dan penahan pertama `S17` |
| 5 | Putuskan tata kelola bukti uji backend | Keputusan repository | Pemilik repository backend | `C-04`. **Paling berdampak, dan paling tidak terlihat** |

---

## 7. Batas Audit Ini

Supaya pembaca berikutnya tahu apa yang **tidak** diperiksa:

- **Nol pengujian runtime ujung-ke-ujung.** Aplikasi tidak dijalankan; database tidak disentuh.
  Yang diperiksa adalah kode, kontrak, bukti tertulis, dan uji yang tersedia.
- **Nol pemeriksaan performa.** Termasuk peringatan `LAB-DEC-072` soal pola alokasi nomor yang
  memuat seluruh nomor terpakai ke memori — itu **belum dibangun**, jadi belum ada yang diukur.
- **Nol audit keamanan.** Permission dan authorization hanya diperiksa keberadaannya pada
  kontrak, bukan ditembus.
- **`S17` dan `S4`..`S6` sengaja di luar verdict.** Keduanya tertahan `LAB-SIGN-001` dan sudah
  tercatat sebagai gap pada `traceability` bagian `4.1c`. Menilai kesiapan sesuatu yang belum
  boleh dibangun tidak menghasilkan informasi.
