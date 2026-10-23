# PLAN-REPAIR-039 — Ketahanan muat ulang dan konsistensi data pada lima tipe pendaftaran Admisi Rawat Inap

```yaml
plan_id: PLAN-REPAIR-EPS-039
issue: ../issue/issue-039-audit-lima-tipe-pendaftaran-admisi.md
status_rencana: DISETUJUI
tanggal_rencana: "2026-10-23"
diputuskan_oleh: "Pemilik Sistem"
tanggal_keputusan: "2026-10-23"
basis_source_backend: "b6c638ee (MHamzah)"
basis_source_frontend: "229f071bb (HamzahV2)"
```

Delapan dari sembilan perbaikan hanya menyentuh frontend dan memakai endpoint yang sudah ada.
`FIX-EPS-039-03` menunggu keputusan K-01 karena menyentuh kontrak lintas modul.

---

## 1. Register status pengerjaan

| ID perbaikan | Menutup | Ringkasan | Area | Prioritas | Task ID | Status | Bukti |
| --- | --- | --- | --- | :---: | --- | --- | --- |
| `FIX-EPS-039-01` | `ISS-EPS-039-01` | Episode Draft dicatat ke URL di semua jalur; pilihan DPJP disimpan per pasien | FE | 1 | `FIX-EPS-039` | 🟡 SEBAGIAN — 5/5 AC ada di source; AC5 terbukti tes unit; AC1–AC4 menunggu uji layar (S1, S2) | [`FIX-EPS-039.md`](../../task/report/frontend/FIX-EPS-039.md) |
| `FIX-EPS-039-02` | `ISS-EPS-039-02` | ID permintaan Kamar Pulih dicatat di URL dan dibaca ulang saat muat | FE | 1 | `FIX-EPS-039` | 🟡 SEBAGIAN — 3/3 AC ada di source; 0/3 terbukti uji layar (S3) | [`FIX-EPS-039.md`](../../task/report/frontend/FIX-EPS-039.md) |
| `FIX-EPS-039-03` | `ISS-EPS-039-03` | Data rujukan faskes luar disimpan pada episode rawat inap (K-01 = B) | BE + FE | 1 | `FIX-EPS-039-03-BE`, `FIX-EPS-039-03-FE` | 🟡 SEBAGIAN (23-10-2026) — 4/4 AC ada di source; AC2 terbukti tes unit; AC1/AC3 belum diuji bolak-balik dengan server; AC4 ✅ migration diterapkan pemilik 23-10-2026. BE: build Release 0 error, QBE Strict PASS. FE: lint 0 error, tes 7/7, suite tanpa kegagalan baru (43 lama), build PASS, render SSR 4/4. Belum: uji layar R1–R5 | [`FIX-EPS-039-03-BE.md`](../../task/report/backend/FIX-EPS-039-03-BE.md), [`FIX-EPS-039-03-FE.md`](../../task/report/frontend/FIX-EPS-039-03-FE.md) |
| `FIX-EPS-039-04` | `ISS-EPS-039-04` | Draf sesi tidak disimpan pada kunci `active` di semua jalur | FE | 2 | `FIX-EPS-039` | 🟡 SEBAGIAN — 2/2 AC ada di source; 0/2 terbukti uji layar (S4) | [`FIX-EPS-039.md`](../../task/report/frontend/FIX-EPS-039.md) |
| `FIX-EPS-039-05` | `ISS-EPS-039-05` | Sumber transfer tidak dimuat ulang per langkah; episode transfer ditandai milik layar | FE | 2 | `FIX-EPS-039` | 🟡 SEBAGIAN — 2/2 AC ada di source; 0/2 terbukti uji layar (S5) | [`FIX-EPS-039.md`](../../task/report/frontend/FIX-EPS-039.md) |
| `FIX-EPS-039-06` | `ISS-EPS-039-06` | Penjamin yang belum terbaca ditulis "Belum terbaca" | FE | 2 | `FIX-EPS-039` | ✅ SELESAI | [`FIX-EPS-039.md`](../../task/report/frontend/FIX-EPS-039.md) — render SSR 4/4 |
| `FIX-EPS-039-07` | `ISS-EPS-039-07` | Daftar Kamar Pulih memakai pencarian server, paging, dan `totalData` | FE | 3 | `FIX-EPS-039` | 🟡 SEBAGIAN — 2/2 AC ada di source; 0/2 terbukti uji layar (S8) | [`FIX-EPS-039.md`](../../task/report/frontend/FIX-EPS-039.md) |
| `FIX-EPS-039-08` | `ISS-EPS-039-08` | Kunci idempotensi deposit berupa GUID per admisi | FE | 3 | `FIX-EPS-039` | 🟡 SEBAGIAN — 1/1 AC ada di source; payload belum dicek di layar | [`FIX-EPS-039.md`](../../task/report/frontend/FIX-EPS-039.md) |
| `FIX-EPS-039-09` | `ISS-EPS-039-09` | Perbaiki argumen `onChange` cabang pilihan bed transfer | FE | 3 | `FIX-EPS-039` | ✅ SELESAI | [`FIX-EPS-039.md`](../../task/report/frontend/FIX-EPS-039.md) — tes regresi, lint, build |

**Ringkasan: 2 dari 9 perbaikan selesai, 7 sebagian (source, lint, unit test, dan build selesai
23-10-2026; uji layar belum), 0 belum dikerjakan.**

---

## 2. Solusi terpilih per temuan

### ISS-EPS-039-01 — Draft ganda saat muat ulang

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Catat `episodeId` ke URL begitu Draft dibuat (hapus pengecualian langkah Pilih Bed) **dan** simpan isian DPJP per pasien di `sessionStorage` | Muat ulang membaca episode yang sama; pola sama dengan bayi (`FIX-EPS-038-01`) dan Transfer | Menyentuh view yang dipakai lima route |
| B | Tunda pembuatan Draft sampai "Lanjut ke Pemesanan" | Draft tidak dibuat hanya karena membuka langkah | Mengubah desain `FE-RWI-240` (denah perlu episode untuk kelayakan server) |
| C | Backend menolak Draft kedua | Penjaga terakhir yang kuat | Membalik `RWI-AC-006`; butuh keputusan dan task backend |

**Solusi terpilih: Opsi A.** Gerbang `isOwnEpisode` dari `FIX-EPS-038-01` sudah memastikan langkah
tidak dipasang ulang saat URL berubah, jadi pengecualian langkah Pilih Bed tidak lagi diperlukan.
Penyimpanan DPJP mengikuti pola Pembayaran yang sudah ada
(`inpatient-admission-payment-storage.jsx`). **Kenapa bukan B/C:** keduanya mengubah keputusan
yang sudah disetujui.

### ISS-EPS-039-02 — Rujukan Kamar Pulih hilang

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | `referralId` di URL + baca ulang `GET /admission-referrals/{id}` | Tahan muat ulang dan tautan dapat dibagikan; server tetap sumber kebenaran | Satu parameter URL baru |
| B | Simpan objek permintaan di `sessionStorage` | Tanpa permintaan tambahan | Data basi bila permintaan sudah dibatalkan |

**Solusi terpilih: Opsi A** — sama dengan cara `episodeId` dan `transferId` disimpan.

### ISS-EPS-039-03 — Data rujukan tidak tersimpan

Opsi dan rekomendasi ada pada K-01. Rekomendasi: **A — `PUT /patient-encounters/{encounterId}/referral`**
(modul Pendaftaran) sesudah kunjungan dibuat; isian faskes diganti pilihan master institusi.
Butuh persetujuan pemilik modul Pendaftaran dan revisi `RWI-DEC-269`.

### ISS-EPS-039-04 — Kunci sesi `active`

**Solusi terpilih:** perluas `FIX-EPS-038-05` ke semua mode, tanpa opsi lain yang wajar: kunci
`active` dipakai bersama oleh definisi, sehingga satu-satunya cara aman adalah tidak memakainya.
Konsekuensinya, muat ulang sebelum pasien baru tersimpan mengosongkan langkah 1–2 (cukup diisi
ulang).

### ISS-EPS-039-05 — Transfer berkedip

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Muat ulang sumber hanya saat `transferId`, `patientId`, `episodeId`, atau `reload` berubah (bukan `stepSlug`); data lama dipertahankan selama memuat; kotak memuat hanya pada muatan pertama; `adoptTransferEpisode` mengisi `createdEpisodeId` | Tanpa kedip; validasi status `Executed` tetap terbaca | Data sumber bisa tertinggal beberapa detik — tetap divalidasi ulang server saat simpan |
| B | Hanya menghapus `stepSlug` dari dependensi | Paling kecil | Kedip tetap terjadi saat episode dibuat; banner "Melanjutkan admisi" tetap salah |

**Solusi terpilih: Opsi A.**

### ISS-EPS-039-06 sampai 09

- 06: satu konstanta "Belum terbaca" untuk penjamin yang tidak terbaca, ditambah tombol muat
  ulang ringkasan; tanpa opsi lain yang wajar (aturan: nilai tak terbaca tidak boleh ditebak).
- 07: pakai `search`, `pageNumber`, dan `totalData` yang sudah didukung
  `GET /admission-referrals` (`AdmissionReferralQuery`), sama seperti daftar Transfer.
- 08: kunci GUID dibuat sekali per admisi (`crypto.randomUUID`) dan disimpan bersama draf deposit,
  mengikuti pola kunci idempotensi pasien bayi.
- 09: ubah penangan menjadi `(_name, bedId) => …`; pilihan "hapus cabang" ditolak karena cabang
  itu bagian kontrak komponen bersama.

---

## 3. Skema tampilan sebelum → sesudah

### 3.1 Kamar Pulih setelah muat ulang (FIX-02)

**SEBELUM:**

```text
┌ Informasi Pasien ───────────────────────────────────────────────┐
│ Ny. Rina · RM 00-45-67                                           │
│ ⚠ Pasien punya permintaan admisi dari kamar pulih; buka admisi   │
│   dari permintaan itu                                            │
│ [ Ganti Pasien ]                       [ Kembali ] [ Lanjut (×) ] │
└─────────────────────────────────────────────────────────────────┘
```

**SESUDAH:**

```text
┌ Informasi Pasien ───────────────────────────────────────────────┐
│ Ny. Rina · RM 00-45-67                                           │
│ Permintaan Kamar Pulih: KP-2610-0012 · ICU · dr. Anestesi A      │
│ [ Ganti Pasien ]                       [ Kembali ] [ Lanjut ]    │
└─────────────────────────────────────────────────────────────────┘
```

| Keadaan | Tampilan |
| --- | --- |
| Memuat permintaan dari URL | Banner info "Memuat permintaan Kamar Pulih…"; tombol Lanjut mati |
| Permintaan tidak ditemukan / sudah diproses | Peringatan "Permintaan Kamar Pulih sudah tidak berlaku. Pilih ulang dari daftar." + tombol **Kembali ke Daftar** |
| Tanpa hak `InpatientAdmissionReferral : Read` | Pesan 403 server apa adanya |

### 3.2 Konfirmasi dengan penjamin tak terbaca (FIX-06)

**SEBELUM:** `Penjamin [Umum]  Tunai / Mandiri  · Cara Bayar: Umum`

**SESUDAH:**

```text
┌ Penjamin ──────────────────────────── [ Belum terbaca ] ┐
│ Belum terbaca                                              │
│ ⚠ Data penjamin kunjungan belum terbaca.                   │
│ [ Muat Ulang Ringkasan ]                                   │
└───────────────────────────────────────────────────────────┘
```

### 3.3 Transfer (FIX-05) dan perbaikan lain

Tidak ada perubahan susunan tampilan. Yang berubah adalah perilakunya: kotak "Memuat dan memeriksa
permintaan rawat inap..." hanya tampil saat sumber belum pernah terbaca, dan banner "Melanjutkan
admisi" tidak tampil untuk episode hasil transfer yang baru dibuat. Daftar Kamar Pulih (FIX-07)
memakai komponen `Pagination` existing seperti daftar Transfer.

---

## 4. Rincian perbaikan

### FIX-EPS-039-01 — Episode Draft dicatat ke URL di semua jalur; pilihan DPJP disimpan per pasien

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-039-01` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend + tes unit |
| **Berkas yang diubah** | `inpatient-admission-view.jsx` (efek URL `:691-703`); `use-inpatient-admission-doctor.jsx`; berkas baru `src/utils/health-services/inpatient-management/inpatient-admission-doctor-storage.jsx` (pola `inpatient-admission-payment-storage.jsx`) |
| **Radius dampak** | Pasien Baru, Pasien Lama, Kamar Pulih; Bayi dan Transfer tidak berubah |
| **Bergantung pada** | `FIX-EPS-038-01` (sudah di source) |

**Langkah.**

1. Hapus pengecualian `INPATIENT_BED_STEP_SLUGS.SELECTION` pada efek penulisan URL, sehingga
   `episodeId` ditulis begitu `doctor.episodeId` ada, di langkah apa pun selain Transfer.
2. Simpan `doctorId`, `supportingDoctors`, `notes`, `requiresIsolation`, `isolationNote`,
   `careDependencyLevel`, dan `careDependencyNote` per pasien di
   `quilvian:inpatient-admission:doctor:<patientId>`, lalu pulihkan saat hook dipasang ulang.
   Simpanan dihapus begitu episode terbentuk dan oleh `clearInpatientAdmissionSession` (prefix
   yang sama sudah tercakup).

**Acceptance criteria.**

1. Pasien Baru Tn. Budi: buka Pilih Bed → URL langsung memuat `episodeId`; layar tidak berkedip
   dan tidak menampilkan banner "Melanjutkan admisi".
2. Tekan F5 di Pilih Bed → layar memuat episode yang sama; Network tidak memuat `POST
   /patient-encounters/admin` maupun `POST /episodes` kedua.
3. Tekan F5 di langkah Dokter sebelum Pilih Bed → DPJP dan catatan tetap terisi.
4. Query temuan 01 tidak menambah baris untuk Tn. Budi.
5. Fungsi baca/tulis simpanan DPJP punya tes unit (bentuk yang tidak dikenal diperlakukan kosong).

**Verifikasi.** `npm run lint:errors`, `npm run test:unit`, `npm run build`; uji manual S1, S2.

**Risiko.** Pelanjutan admisi dari daftar kerja mengisi form dari server; simpanan DPJP tidak boleh
menimpanya. Deteksi: AC "pelanjutan" S7.

### FIX-EPS-039-02 — ID permintaan Kamar Pulih dicatat di URL dan dibaca ulang

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-039-02` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend |
| **Berkas yang diubah** | `inpatient-admission-flow-constants.jsx` (`INPATIENT_ADMISSION_QUERY.REFERRAL`); `use-inpatient-admission-flow.jsx` (`referralId`, `writeFlowUrl` `nextReferralId`); `inpatient-admission-view.jsx` (`handleSelectRecoveryReferral` `:913-939`); hook kecil baru di `src/lib/hooks/health-services/inpatient-management/` yang membaca permintaan per ID (pola `use-inpatient-admission-resume.jsx`) |
| **Radius dampak** | Kamar Pulih; parameter URL baru dihapus otomatis saat keluar alur atau berganti pasien |
| **Bergantung pada** | Tidak ada |

**Langkah.**

1. Pilih permintaan → URL `?step=existing-patient-information&patientId=…&referralId=…`.
2. Saat `referralId` ada di URL dan state kosong, baca `GET /admission-referrals/{id}`; isi
   `admissionReferral` hanya bila status `Pending` dan `patientId` cocok.
3. `writeFlowUrl` menghapus `referralId` bila keluar alur atau pasien berganti.

**Acceptance criteria.**

1. Pilih permintaan KP-2610-0012 Ny. Rina → F5 di Informasi Pasien → tidak ada blocker
   `INP-ADM-REF-001`; tombol Lanjut aktif.
2. F5 di langkah Dokter lalu masuk Pilih Bed → episode terbentuk tertaut permintaan; permintaan
   berubah menjadi selesai di daftar Kamar Pulih.
3. Permintaan yang sudah diproses/ dibatalkan → peringatan skema 3.1 dan tombol Kembali ke Daftar.

**Verifikasi.** Lint, unit test, build; uji manual S3.

**Risiko.** Kecil; endpoint dan hak akses sudah dipakai layar daftar.

### FIX-EPS-039-03 — Data rujukan faskes luar dikirim ke server

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-039-03` |
| **Area** | Backend Rawat Inap, lalu Frontend |
| **Jenis perubahan** | K-01 = **opsi B** (diputuskan 23-10-2026): field baru pada episode rawat inap, teks bebas sesuai `RWI-DEC-269` |
| **Berkas yang diubah** | Backend: model `InpEpisode`, DTO buat episode dan detail episode, `InpEpisodeService` (pembuatan dan pemetaan detail), migration + Designer + snapshot. Frontend: `inpatient-admission-encounter-utils.jsx` (payload episode), hook dokter (meneruskan data rujukan), view (mengirim `flow.referralData` bila jenis kunjungan Rujukan), langkah Konfirmasi (menampilkan rujukan), langkah Jenis Kunjungan (batas panjang isian) |
| **Radius dampak** | Hanya Rawat Inap: `POST /episodes` menerima field opsional baru; `GET /episodes/{id}` mengembalikan field baru. Jalur tanpa rujukan tidak berubah |
| **Bergantung pada** | Task backend selesai sebelum task frontend |

**Isi data.** `ReferralNumber` (≤ 250), `ReferralDateTime`, `ReferralFacilityName` (≤ 200),
`ReferralDoctorName` (≤ 150), `ReferralDiagnosis` (≤ 1000). Semua opsional di server; bila salah
satu diisi, nomor, tanggal, faskes, dan diagnosa wajib lengkap (mencerminkan validasi layar).
Tanggal rujukan tidak boleh di masa depan.

**Acceptance criteria.**

1. Pasien Baru dengan rujukan nomor 445/RS-X → `POST /episodes` membawa kelima field; `GET
   /episodes/{id}` mengembalikannya; langkah Konfirmasi menampilkannya.
2. Pasien tanpa rujukan (Umum) → payload tidak membawa field rujukan; perilaku lama tidak berubah.
3. Rujukan tidak lengkap atau tanggal di masa depan → `400` dengan kalimat berbahasa Indonesia.
4. Migration tersedia (file migration, Designer, snapshot); database diperbarui pemilik sendiri.

### FIX-EPS-039-04 — Draf sesi tidak disimpan pada kunci `active` di semua jalur

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-039-04` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend |
| **Berkas yang diubah** | `use-inpatient-admission-flow.jsx` (`:147-156`, pembersihan saat mount, migrasi kunci) |
| **Radius dampak** | Semua mode; mode yang memakai `patientId`/`episodeId` sebagai kunci tidak berubah |
| **Bergantung pada** | Tidak ada |

**Langkah.** Kondisi `entryMode === NEWBORN && id === "active"` diganti `id === "active"`;
pembersihan saat mount menghapus semua `…:flow:<mode>:active`; migrasi kunci lama berlaku untuk
semua mode.

**Acceptance criteria.**

1. Isi rujukan + kategori "Pegawai" di Pasien Baru, pindah menu, buka lagi → langkah 1 kosong dan
   kategori belum terpilih.
2. Muat ulang di langkah Pembayaran (URL memuat `patientId`) → jenis kunjungan dan kategori tetap
   (perilaku `FIX-EPS-029-04` dipertahankan).

### FIX-EPS-039-05 — Sumber transfer tidak dimuat ulang per langkah; episode transfer milik layar

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-039-05` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend |
| **Berkas yang diubah** | `use-inpatient-admission-transfer.js` (`:28-46`); `inpatient-admission-view.jsx` (`:1209`); `use-inpatient-admission-doctor.jsx` (`adoptTransferEpisode` `:898-901`) |
| **Radius dampak** | Transfer saja |
| **Bergantung pada** | `FIX-EPS-038-01` |

**Langkah.**

1. Hapus `stepSlug` dari dependensi pemuat; pertahankan `source` lama selama memuat ulang (hanya
   dikosongkan bila `transferId` berganti).
2. View menampilkan kotak memuat hanya bila `transfer.loading && !transfer.source`.
3. `adoptTransferEpisode` mengisi `createdEpisodeId`.

**Acceptance criteria.**

1. Pindah Pembayaran → Deposit → Dokter → Pilih Bed: tidak ada kotak memuat; Network hanya memuat
   sumber transfer sekali.
2. Setelah "Pesan Tempat Tidur" berhasil: kartu pemesanan tetap tampil tanpa berkedip, tanpa banner
   "Melanjutkan admisi"; sumber dibaca ulang sekali dan berstatus `Executed`.

### FIX-EPS-039-06 — Penjamin yang belum terbaca ditulis "Belum terbaca"

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-039-06` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend |
| **Berkas yang diubah** | `inpatient-admission-confirmation-step.jsx` (`:195-196`); `inpatient-newborn-admission-steps.jsx` (`:79`, `:608`) |
| **Radius dampak** | Langkah Konfirmasi semua jalur; Pilih Ibu dan Tinjau bayi |
| **Bergantung pada** | Tidak ada |

**Acceptance criteria.**

1. Pelanjutan episode dengan kunjungan gagal dibaca → kartu Penjamin "Belum terbaca" + tombol
   **Muat Ulang Ringkasan** (memanggil `resume.retry`), tidak ada kata "Tunai" atau "Umum".
2. Pasien Tunai yang terbaca normal tetap tampil "Tunai / Mandiri".

### FIX-EPS-039-07 — Daftar Kamar Pulih memakai pencarian server, paging, dan `totalData`

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-039-07` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend |
| **Berkas yang diubah** | `admission-referrals/inpatient-admission-recovery-referral-step.jsx`; `inpatient-admission-view.jsx` (`:837-858` lencana) |
| **Radius dampak** | Kamar Pulih |
| **Bergantung pada** | Tidak ada |

**Acceptance criteria.**

1. Dengan 60 permintaan Pending, mencari nama pasien ke-55 menemukannya (kueri `search` ke server).
2. Lencana kartu menampilkan 60, dan jumlah terlambat dari kueri `overdueOnly=true`.

### FIX-EPS-039-08 — Kunci idempotensi deposit berupa GUID per admisi

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-039-08` |
| **Area** | Frontend |
| **Berkas yang diubah** | `inpatient-admission-view.jsx` (`:749`); `use-inpatient-admission-deposit.jsx` (simpan kunci bersama snapshot deposit) |
| **Radius dampak** | Pasien Baru, Pasien Lama, Kamar Pulih dengan deposit |

**Acceptance criteria.** Payload `POST /episodes` memuat `depositIdempotencyKey` berbentuk GUID
yang sama pada pengiriman ulang admisi yang sama, dan berbeda untuk admisi berikutnya pasien yang
sama.

### FIX-EPS-039-09 — Perbaiki argumen `onChange` cabang pilihan bed transfer

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-039-09` |
| **Area** | Frontend |
| **Berkas yang diubah** | `inpatient-admission-bed-step.jsx` (`:59`, `:167`) |
| **Radius dampak** | Tidak ada perubahan perilaku hari ini (cabang tidak dirender) |

**Acceptance criteria.** `onChange={(_name, bedId) => selectTransferBed(bedId)}`; lint dan build
lulus.

---

## 5. Urutan pengerjaan

```text
FIX-EPS-039-01 🟡   tanpa dependency baru (memakai FIX-EPS-038-01) — kerjakan lebih dulu
FIX-EPS-039-02 🟡   tanpa dependency
FIX-EPS-039-04 🟡   tanpa dependency — berkas berbeda dari FIX-01, aman dikerjakan paralel
FIX-EPS-039-05 🟡   tanpa dependency baru
FIX-EPS-039-06 ✅   tanpa dependency
FIX-EPS-039-07 🟡   tanpa dependency
FIX-EPS-039-08 🟡   tanpa dependency
FIX-EPS-039-09 ✅   tanpa dependency
K-01 diputuskan opsi B (23-10-2026)
└── FIX-EPS-039-03 🟡   task backend lebih dulu, lalu frontend
```

---

## 6. Keputusan pemilik yang dibutuhkan

| No | Keputusan | Pilihan | Rekomendasi | Menahan perbaikan |
| ---: | --- | --- | --- | --- |
| K-01 | Tempat penyimpanan data rujukan faskes luar | A: endpoint rujukan kunjungan Pendaftaran (faskes dari master) / B: field baru episode rawat inap (teks bebas) / C: catatan admisi | B (direvisi; A tidak mendukung kunjungan rawat inap) | `FIX-EPS-039-03` — **diputuskan B, 23-10-2026** |
| K-02 | Draft ganda yang sudah terbentuk | Gugur otomatis + batal manual / skrip data | Gugur otomatis + batal manual | Tidak |

---

## 7. Dokumen hulu yang ikut direvisi

| Dokumen | Bagian | Sekarang | Menjadi |
| --- | --- | --- | --- |
| `03-frontend-architecture.md` | Tabel langkah Pasien Baru, baris 404 | "Jenis Kunjungan … – (state alur)" | "Jenis Kunjungan … — dikirim bersama `POST /episodes` (field rujukan episode)" |
| `00-interview-decisions.md` | `RWI-DEC-269` | Faskes perujuk teks bebas | Tidak berubah (opsi B tetap teks bebas) |
| `05-skema-tampilan.md` | Langkah Informasi Pasien Kamar Pulih | Tidak menyebut keadaan permintaan dari URL | Tambah keadaan skema 3.1 |

Revisi dokumen dikerjakan lewat jalur blueprint yang berwenang.

---

## 8. Verifikasi menyeluruh

| No | Skenario | Hasil yang diharapkan |
| ---: | --- | --- |
| S1 | Pasien Baru sampai Pilih Bed, F5, lanjut sampai Booking | Satu kunjungan, satu episode; DPJP tetap |
| S2 | Pasien Lama, F5 di Dokter dan di Pilih Bed | Isian tetap; tanpa episode kedua |
| S3 | Kamar Pulih, F5 di Informasi Pasien dan di Dokter | Tanpa blocker `INP-ADM-REF-001`; episode tertaut permintaan |
| S4 | Pasien Baru dengan rujukan, pindah menu, buka lagi | Langkah 1 kosong |
| S5 | Transfer dari antrean sampai Konfirmasi | Tanpa kotak memuat antar langkah; tanpa banner "Melanjutkan admisi" |
| S6 | Pelanjutan episode dengan kunjungan tak terbaca | Penjamin "Belum terbaca" |
| S7 | Lanjutkan Draft dari Daftar Pasien Rawat Inap (kelima jalur) | Banner pelanjutan tetap tampil; form dari server tidak tertimpa simpanan |
| S8 | Kamar Pulih dengan > 50 permintaan | Pencarian dan lencana benar |
| S9 | Admisi Bayi (regresi ISSUE-038 S1–S10) | Tidak berubah |
| S10 | Query temuan 01 | Tidak ada Draft ganda baru |

Perintah wajib frontend: `npm run lint:errors`, `npm run test:unit`, `npm run build`.

---

## 9. Riwayat

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-10-23 | Rencana dibuat: 9 perbaikan (8 siap dikerjakan, 1 menunggu K-01), menunggu persetujuan | `diagnose-module-issue` |
| 2026-10-23 | Pemilik menyetujui seluruh rencana beserta K-01 opsi A dan K-02; `status_rencana` → `DISETUJUI` | Pemilik Sistem |
| 2026-10-23 | Task `FIX-EPS-039`: 8 perbaikan dikerjakan; lint PASS, tes terkait 30/30, suite penuh tanpa kegagalan baru, build PASS, render SSR penjamin 4/4. FIX-06 dan FIX-09 ✅; enam lainnya 🟡 menunggu uji layar. FIX-03 ⛔: pemeriksaan backend menunjukkan `PUT /patient-encounters/{id}/referral` mencatat tujuan rawat inap sebagai `Laboratory` dan mewajibkan faskes mitra (`Pm17`), sehingga opsi A butuh task backend modul Pendaftaran atau keputusan ulang K-01. Ditemukan dan diperbaiki sekalian: kunci draf transfer terkirim ke hook dokter pada semua jalur | `build-module-frontend` |
| 2026-10-23 | Pemilik menyetujui revisi K-01 ke opsi B (field rujukan pada episode rawat inap, teks bebas). FIX-03 tidak lagi terblokir; rincian dan acceptance criteria ditulis ulang | Pemilik Sistem |
| 2026-10-23 | FIX-03 dikerjakan, status 🟡. Task `FIX-EPS-039-03-BE`: 5 kolom rujukan pada `InpEpisode`, validasi `POST /episodes`, proyeksi `GET /episodes/{id}`, migration `20261023062116_AddInpEpisodeExternalReferral` (belum diterapkan). Task `FIX-EPS-039-03-FE`: payload dan validator bersama, batas panjang isian, kartu Rujukan Faskes Luar di Konfirmasi | `build-module-backend`, `build-module-frontend` |
