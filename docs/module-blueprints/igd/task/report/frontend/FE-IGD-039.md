# Laporan Perubahan Frontend — `FE-IGD-039`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-IGD-039` |
| Judul | Aksi *Pergi sebelum ditriage* pada daftar Menunggu Triage |
| Slice | `S4` · `EPIC IGD-11` · `MVP-7` |
| Roadmap | `docs/module-blueprints/igd/roadmap/frontend-roadmap.md` bagian R3.12 |
| Trace | `FR-IGD-079` (sisi layar); `IGD-DEC-142`, `150`, `178`; `AT-IGD-176`, `177` (sisi layar); `03-frontend-architecture.md` §13.3 B, §13.5 |
| Contract version | API `0.11.0` §8.3.3 (`POST /no-show`); validation `0.8.0` §10.3 — `approved` (`IGD-DEC-157`), dengan koreksi batas alasan 250 (`IGD-DEC-178`, manifest 0j.2) |
| Wewenang UI | `DEV_DISCRETION` untuk bentuk konfirmasi; memakai dialog konfirmasi beralasan yang sudah ada; nol elemen `NEW` |
| Dependency | `BE-IGD-057` 🟡 (endpoint terpasang pada build pemilik; S1–S7, S9, S10 terbukti, uji paralel S8 tersisa) — pemilik mengizinkan lanjut; `FE-IGD-035` ✅ |
| Klasifikasi | `MEDIUM` — skor 5: repository 0, berkas diperiksa 1, berkas diubah 1 (7), logika 1, kontrak API 1, database 0, keamanan/auth 0, UI/workflow 1 |
| Task mode | `FRONTEND` — wewenang pemilik 1 Oktober 2026. Backend baca-saja, kecuali laporan ini dan status pada roadmap/traceability |
| Target tulis | `QuilvianSystemFrontendDev` (branch `RizkiV2`); laporan di repository backend |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `aa0b1168b` (`RizkiV2`) + working tree `FE-IGD-038` dan `FE-IGD-036` yang belum di-commit |
| Commit backend yang dijadikan rujukan | `b9076c71` + working tree `BE-IGD-053`, `057`, `058`, `059` |
| Tanggal | 1 Oktober 2026 |
| Status | 🟡 **SEBAGIAN — Implementation Complete.** Tujuh berkas (enam source, satu test); `eslint` 0 error; unit test berkas terkait 73/73. **Belum:** `npm run build` dan uji layar U1–U6 (milik pemilik) |

---

## 1. Keadaan yang ditemukan di awal

| Temuan | Bukti |
| --- | --- |
| Baris tanpa kunjungan membawa aksi `NoShow` dari server, tetapi layar tidak punya tombolnya | `availableActions` pada `triage-queue`; tabel hanya merender *Mulai Triage* dan *Tangani Segera* (`FE-IGD-036`) |
| Endpoint-nya sudah ada | `POST /emergency-visits/no-show` (`BE-IGD-057`), batas alasan 250 |
| Dialog konfirmasi beralasan sudah dipakai modul IGD | `ConfirmModal` dengan `requireReason` pada tab Tindak Lanjut, Observasi, dan Transfer |

Tanpa tombol ini, pasien yang pergi sebelum ditriage tetap berada di daftar Menunggu Triage dan tertahan penjaga
pendaftaran bila ia kembali.

---

## 2. Proses bisnis dari sisi pengguna

Pengguna: perawat triage, pada daftar Triage Pasien.

1. Pada baris pasien yang belum punya kunjungan, perawat menekan **Pergi**.
2. Dialog menyebut nama pasien dan nomor encounter, menyatakan bahwa tindakan ini **final dan tidak dapat
   dibatalkan**, dan bahwa pasien yang kembali harus didaftarkan ulang.
3. Perawat mengisi **alasan** (wajib, maksimal 250 karakter). Selama alasan kosong, tombol *Tandai Pergi* tidak aktif.
4. Menekan *Tandai Pergi* → daftar dimuat ulang, baris pasien hilang, dan spanduk hijau berbunyi *"… ditandai pergi
   sebelum ditriage."*

*Contoh.* Bu Sari terdaftar 10.05 dan dipanggil tiga kali. Perawat menekan Pergi, mengisi *"Dipanggil 3 kali pukul
10.20–10.35, tidak ada di ruang tunggu"*, lalu baris Bu Sari hilang dari daftar.

**Jalur tidak normal.**

| Keadaan | Yang terjadi |
| --- | --- |
| Alasan kosong atau hanya spasi | Tombol *Tandai Pergi* tidak aktif |
| Server menolak `400` | Dialog tertutup; pesan server tampil pada spanduk merah |
| Server menolak `409` (pasien baru saja dimulai triagenya, atau encounter sudah berakhir) | Pesan server tampil apa adanya; daftar dimuat ulang sehingga barisnya menampilkan keadaan terbaru |
| Tanpa izin `EmergencyVisit : NoShow` | Pesan penolakan server tampil pada spanduk merah |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kartu `FE-IGD-039`; API §8.3.3; validation §10.3; 03 §13.3 B, §13.5; laporan `BE-IGD-057`.
Source: `confirm-modal.jsx` (base component), pemakaian `requireReason` pada tiga tab Pengkajian IGD, dan seluruh
berkas pada 3.2.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/…/emergency-management-triage-constant.jsx` | + `TRIAGE_NO_SHOW_REASON_MAX_LENGTH` (250), `TRIAGE_NO_SHOW_LABEL`, pesan cadangan |
| `src/utils/…/emergency-management-triage-utils.jsx` | + `buildNoShowPayload`, `buildNoShowConfirmMessage`, `buildNoShowSuccessMessage` |
| `src/lib/state/slice/…/emergency-management-triage-slice.jsx` | + thunk `markEmergencyEncounterNoShow` (`POST /no-show`), cabang state `noShow`, reducer pembersihnya |
| `src/lib/hooks/…/use-emergency-management-list.jsx` | + `markNoShow` (memuat ulang daftar saat berhasil dan saat `409`), `clearNoShow` |
| `src/components/view/…/components/emergency-triage-patient-table.jsx` | Tombol **Pergi** pada baris tanpa kunjungan bila `availableActions` memuat `NoShow`; kolom aksi dilebarkan dari 240 menjadi 300 untuk tiga tombol |
| `src/components/view/…/emergency-triage-patient-list-view.jsx` | Dialog konfirmasi beralasan dan dua spanduk hasil |
| `tests/unit/emergency-triage-utils.test.mjs` | + tiga kasus `FE-IGD-039` |

Nol baris komentar baru. **Tidak disentuh:** CSS mana pun, route, backend.

### 3.3 Kepatuhan arsitektur frontend

View → hook → slice → util/constant; permintaan lewat `InstanceAxios` di thunk.

`UI GATE: 3 elemen — REUSE 3, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat | Bukti | Status |
| --- | --- | --- | --- |
| Konfirmasi dengan alasan wajib | `ConfirmModal` (`requireReason`, `maxReasonLength`, `reasonLabel`, `reasonPlaceholder`) | `base-features/confirm-modal.jsx`; dipakai tab Tindak Lanjut, Observasi, Transfer | `REUSE` |
| Tombol baris **Pergi** | Tombol baris tabel `primaryMiniButton` | Tombol lain pada kolom yang sama | `REUSE` |
| Spanduk berhasil dan galat | `successBanner`, `errorBanner` | Sudah dipakai layar ini | `REUSE` |

**Temuan grep anti-regresi yang dipertahankan.** Satu `<button>` baru pada tabel, dengan alasan yang sama seperti
`FE-IGD-036`: satu kolom, satu bentuk tombol.

### 3.4 Keputusan pelaksanaan

| Keputusan | Alasan |
| --- | --- |
| Batas alasan 250, bukan 500 seperti tertulis pada perilaku kartu | `IGD-DEC-178`; catatan pada baris status kartu sendiri |
| Dialog tertutup sesudah permintaan selesai, berhasil maupun ditolak | Pola *Tangani Segera* pada layar yang sama: hasil dibaca pada spanduk halaman |
| Dialog dipasang baru untuk tiap pasien | `ConfirmModal` menyimpan alasan di dalamnya; dipasang baru supaya alasan pasien sebelumnya tidak terbawa |
| Tombol berbunyi **Pergi**; judul dialog dan keterangan tombol berbunyi *Pergi sebelum ditriage* | Kartu menetapkan tombol *Pergi*; label keadaan 03 §13.5 |

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tombol dialog nonaktif selama permintaan; tombol baris *"Memproses..."*; tombol baris lain nonaktif |
| Kosong | Tidak berubah |
| Gagal | Spanduk merah memuat pesan server apa adanya |
| Tanpa hak akses | Spanduk merah memuat pesan penolakan server |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Emergency Installation Management / Emergency Visit

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/v1/health-services/emergency-installation-management/emergency-visits/no-show` | Menandai pasien pergi sebelum ditriage: `{ "encounterId", "reason" }` | `EmergencyVisit : NoShow` |
| `GET` | `/v1/health-services/emergency-installation-management/emergency-visits/triage-queue` | Daftar dimuat ulang sesudah aksi | `EmergencyVisit : Read` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` tujuh berkas task | 0 error, 0 warning | `PASS` | Keluaran perintah |
| `node --test` empat berkas IGD terkait | 73 dari 73 lulus | `PASS` | Termasuk tiga kasus `FE-IGD-039` |
| Baris komentar pada tambahan | Nol | `PASS` | Pencarian pada `git diff` |
| Akhiran baris | CRLF dipertahankan pada seluruh berkas | `PASS` | Pemeriksaan byte |
| `npm run build` | — | `NOT RUN` | Milik pemilik |
| Uji layar U1–U6 | — | `NOT RUN` | Milik pemilik |

`AUTOMATED TEST: node --test (empat berkas IGD) — PASS (73/73)`

`MANUAL TEST: NOT FEASIBLE — backend tidak berjalan pada sesi ini dan uji layar dijalankan pemilik`

Uji manual: `REQUIRED`.

### 6.1 Skenario uji layar untuk pemilik

Prasyarat: pengguna memegang izin `EmergencyVisit : NoShow` (di dev sudah diberikan ke posisi Perawat IGD).

| # | Langkah | Yang diharapkan | Kriteria |
| ---: | --- | --- | ---: |
| U1 | Daftar Triage Pasien: baris tanpa kunjungan | Tiga tombol: Mulai Triage, Tangani Segera, **Pergi** | — |
| U2 | Tekan Pergi, biarkan alasan kosong | Tombol *Tandai Pergi* tidak aktif; nol permintaan | 1 |
| U3 | Isi alasan, tekan *Tandai Pergi* | `POST /no-show` `200`; baris hilang; spanduk hijau *"… ditandai pergi sebelum ditriage."*; isian berhenti di 250 karakter | 2 |
| U4 | Baris yang **sudah** punya kunjungan | Tidak ada tombol Pergi | 3 |
| U5 | Dua tab: tab A menekan Mulai Triage, tab B (belum dimuat ulang) menekan Pergi pada pasien yang sama | Tab B: `409` *"Pasien ini sudah memiliki kunjungan IGD …"* pada spanduk merah; daftar dimuat ulang dan barisnya menjadi baris kunjungan | 1 |
| U6 | Periksa seluruh teks pada tombol, dialog, dan spanduk | Tidak ada kata "Tidak Hadir" | 4 |

---

## 7. Acceptance criteria dan Definition of Done

| # | Kriteria | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | Tanpa alasan → tombol simpan tidak aktif; bila tetap terkirim, pesan `400` backend tampil | **Terpenuhi pada source** | `ConfirmModal` `requireReason` menonaktifkan tombol; thunk meneruskan pesan server. Uji layar U2, U5 belum |
| 2 | Dengan alasan → baris hilang dari daftar | **Terpenuhi pada source** | `markNoShow` memuat ulang daftar; unit test `FE-IGD-039 K2`. Uji layar U3 belum |
| 3 | Tombol tidak tampil pada baris kunjungan | **Terpenuhi pada source** | Tombol hanya dirender pada cabang baris tanpa kunjungan. Uji layar U4 belum |
| 4 | Label tidak pernah "Tidak Hadir" | **Terpenuhi** | Unit test `FE-IGD-039 K4` |
| 5 | `eslint` 0 error; unit test lulus; nol CSS baru; build dan uji layar — milik pemilik | **Sebagian** | `eslint` 0 error; 73/73; nol CSS; build dan uji layar belum |

DoD: laporan tracked ✅ (berkas ini). Dirilis bersama `FE-IGD-035` dan `FE-IGD-036`.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tindakan ini final. Salah tandai berarti pasien harus didaftarkan ulang |
| Masalah yang diketahui | Tombol tampil menurut `availableActions`, bukan menurut izin pengguna; pengguna tanpa izin melihat tombol dan menerima pesan penolakan |
| Dependency backend | `BE-IGD-057` 🟡 — perilaku satu-per-satunya terbukti; uji NoShow lawan Mulai Triage serentak belum (U5 di atas menguji sisi layarnya secara berurutan) |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Tujuh berkas berubah oleh task ini, enam di antaranya juga memuat perubahan `FE-IGD-036`. Tanpa stage, commit, atau push |
| Langkah berikutnya | `FE-IGD-040` |
