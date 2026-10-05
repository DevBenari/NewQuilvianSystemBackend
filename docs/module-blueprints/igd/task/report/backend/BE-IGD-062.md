# Laporan Perubahan Backend — `BE-IGD-062`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-IGD-062` |
| Judul | Pembatalan disposisi ditolak pada kunjungan yang sudah selesai |
| Slice | `S5` · `EPIC IGD-13` · `MVP-8` |
| Roadmap | `docs/module-blueprints/igd/roadmap/backend-roadmap.md` bagian R3.14 |
| Trace | `FR-IGD-090`; `IGD-DEC-166`, `176`; `AT-IGD-189` |
| Contract version | validation `0.9.0` §11 aturan 8–9 (tidak berubah pada `0.10.0`); API `0.13.0` §9.1 nomor 3 (dikoreksi `IGD-DEC-176`) dan §9.3 — `approved` (`IGD-DEC-170`). Hash cocok dengan manifest 0j, 0j.1, 0j.2 |
| Dependency | `BE-IGD-060` ✅ |
| Klasifikasi | `LIGHT` — skor 3: repository 0, berkas diperiksa 0, berkas diubah 0 (1 berkas), logika bisnis 0, kontrak API 2 (kode dan pesan penolakan berubah), database 1 (satu pembacaan), keamanan/auth 0, UI/workflow 0 |
| Task mode | `BACKEND` — wewenang pemilik 1 Oktober 2026. Tanpa `dotnet build`, migration, `Program.cs`, eksekusi database, commit |
| Target tulis | `NewQuilvianSystemBackend`: satu berkas source IGD dan `docs/module-blueprints/igd/` |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `b9076c71` (`rizkiG`) + working tree `BE-IGD-053`, `057`, `058`, `059`, `061` yang belum di-commit |
| Tanggal | 1 Oktober 2026 |
| Status | ✅ **SELESAI — 2 Oktober 2026.** Build pemilik dan uji API S1–S5 **5 dari 5** pada bukti mentah. Tanpa UAT. *Sebelumnya:* 🟡 **SEBAGIAN — Implementation Complete.** Satu berkas source (+17 baris); QBE checker `PASS`. **Belum:** build pemilik dan uji API S1–S5 (bagian 5.1) |

### Backend Governance Preflight

| Butir | Hasil |
| --- | --- |
| Governance terbaca | `AGENTS.md` backend; `rules/backend/` suite 1.17.1; kontrak rekayasa dan registry |
| Area / Module | `HealthServices` / `EmergencyInstallationManagement` |
| Owner / prefix registry | Emergency, `Emg`, `ACTIVE / LEGACY`. Nol entity baru |
| Keberlakuan | `TOUCHED LEGACY`: `EmergencyDispositionController.UpdateDispositionStatus` |
| Branch | `rizkiG`, upstream `origin/rizkiG` |
| QBE yang berlaku | `QBE-API-001`, `QBE-VAL-001`, `QBE-PERM-001` (metadata akses tidak berubah) |
| Tidak berlaku | `QBE-ENT-*`, `QBE-NAM-*`, `QBE-CFG-*`, `QBE-MOD-*`, `QBE-CODE-*`, `QBE-TXN-001` (penolakan, nol penulisan), `QBE-SVC-001` (controller lama yang memang mengakses context; legacy ratchet) |
| Checker QBE | Strict, working tree: 0 `VIOLATION`, 0 `REVIEW`, `PASS` |

---

## 1. Masalah yang diperbaiki

Kunjungan yang sudah selesai memang tidak dapat dibuka kembali lewat pembatalan tindak lanjut — itu sudah terjaga.
Yang kurang adalah **jawabannya**: petugas yang mencoba membatalkan tindak lanjut yang sudah dilaksanakan menerima
`400` *"Perubahan status dari Executed ke Cancelled tidak diperbolehkan."*, kalimat teknis yang tidak menyebut
mengapa dan tidak memberi jalan keluar.

*Contoh.* Pasien dipulangkan dan kunjungannya tertutup pukul 14.00. Pukul 16.00 ia kembali dan petugas mencoba
membatalkan tindak lanjut pulangnya supaya kunjungan lama "terbuka lagi". Sesudah task ini jawabannya `409`:
*"Kunjungan IGD ini sudah selesai, sehingga disposisinya tidak dapat dibatalkan. Daftarkan pasien sebagai episode
baru bila ia kembali."*

---

## 2. Proses bisnis

**Pelaku.** Pemegang izin `EmergencyDisposition : Update`.

Urutan pemeriksaan pada `PATCH /{id}/disposition-status`:

1. Tindak lanjut tidak ada → `404` (tidak berubah).
2. **Baru:** tujuan `Cancelled`, tindak lanjutnya berstatus `Executed`, dan kunjungannya `Completed` → `409` dengan
   kalimat validation §11 aturan 8.
3. Penjaga perpindahan status tindak lanjut → `400` (tidak berubah).
4. Alasan pembatalan wajib → `400` (tidak berubah).

Langkah 2 mendahului langkah 4: walaupun alasannya diisi, pembatalan itu tetap ditolak.

**Yang tidak berubah.**

| Keadaan | Jawaban |
| --- | --- |
| Membatalkan tindak lanjut `Executed` pada kunjungan yang **belum** selesai (menunggu penutupan) | Tetap `400` penjaga perpindahan — `Executed` final (`IGD-DEC-176`) |
| Membatalkan tindak lanjut `Draft` atau `Confirmed` beralasan | Tetap berhasil |
| Membatalkan tanpa alasan pada tindak lanjut `Draft`/`Confirmed` | Tetap `400` alasan wajib |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kartu `BE-IGD-062` beserta *Hasil pemeriksaan ulang*; validation §11; API §9.1, §9.3; state §9.4.
Source: `EmergencyDispositionController.cs`, `EmergencyDispositionService.CanTransition`,
`EmergencyVisitService` (`CanTransition`, `TryApplyVisitStatus`, `EpisodeMasihBerjalan`), dan seluruh titik tulis
status kunjungan pada modul IGD.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `…/Controllers/EmergencyDispositionController.cs` | + konstanta kalimat aturan 8; + satu pemeriksaan pada `UpdateDispositionStatus`, sesudah `404` dan sebelum penjaga perpindahan: membaca status kunjungan hanya bila tujuannya `Cancelled` dan tindak lanjutnya `Executed`. +17 baris, nol baris dihapus |

**Nol baris komentar ditambah.** **Tidak disentuh:** `EmergencyDispositionService.CanTransition`,
`EmergencyVisitService`, `Program.cs`, model, migration, frontend.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Sesuai kontrak terkunci (API §9.1 nomor 3): untuk satu keadaan, kode berubah `400` → `409` dan pesannya berganti. Bentuk request dan response tidak berubah |
| Database | Nol schema. Satu pembacaan status kunjungan, hanya pada jalur pembatalan tindak lanjut `Executed` |
| Keamanan/Auth | `NOT APPLICABLE` — metadata akses tidak berubah |

### 3.4 Keputusan pelaksanaan

| Keputusan | Alasan |
| --- | --- |
| Hanya kunjungan `Completed`, bukan `Cancelled` | Aturan 8 menyebut kunjungan yang *sudah selesai*; kartu menulis `Completed`. Kunjungan `Cancelled` tetap dijawab penjaga perpindahan `400` |
| Status kunjungan dibaca tanpa pelacakan | Jalur ini hanya menolak; tidak ada yang ditulis |

---

## 4. Dokumentasi endpoint

#### Health Services / Emergency Installation Management / Emergency Disposition

Base URL: `api/v1/health-services/emergency-installation-management/emergency-dispositions`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `PATCH` | `/{id}/disposition-status` | Mengubah status tindak lanjut — pembatalan tindak lanjut `Executed` pada kunjungan selesai kini `409` dengan kalimat aturan 8 | `EmergencyDisposition : Update` (tidak berubah) |

Request: `{ "dispositionStatus": <angka>, "notes": "…" }`.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| QBE checker Strict (working tree) | 0 `VIOLATION`, 0 `REVIEW`, `Final result: PASS` | `PASS` | Keluaran perintah 1 Oktober 2026 |
| Review diff | +17/−0 pada satu action; akhiran baris CRLF dipertahankan; jumlah baris komentar tetap 12 | `PASS` | `git diff --numstat`, hitungan komentar |
| Jalur lain yang membuka kembali kunjungan selesai | Nol: seluruh penulisan `VisitStatus` melewati `EmergencyVisitService.CanTransition`, yang menolak apa pun dari `Completed` | `PASS` | Pencarian `VisitStatus =` dan `TryApplyVisitStatus` pada seluruh `Areas/` |
| `dotnet build` | Build pemilik 1 Oktober 2026 (DLL 15.18; jumlah warning tidak dilaporkan); build Rizki 3 Oktober 2026 atas `c1f79f79`, yang memuat source task ini tanpa perubahan: 0 error, 0 warning | `PASS` | Bagian *Pemeriksaan bukti uji gabungan — 2 Oktober 2026*; laporan `BE-IGD-061`, *Build pemilik — dicatat 4 Oktober 2026*. *Ditulis 1 Oktober: `NOT RUN`* |
| Uji API S1–S5 | 5 dari 5 terbukti pada bukti mentah | `PASS` | Bagian *Pemeriksaan bukti uji gabungan — 2 Oktober 2026*. *Ditulis 1 Oktober: `NOT RUN`* |

Uji manual: `REQUIRED` — lewat API saja; layar tidak pernah menawarkan Batalkan untuk tindak lanjut `Executed`.

### 5.1 Skenario uji untuk pemilik

| # | Skenario | Hasil yang diharapkan | Kriteria |
| ---: | --- | --- | ---: |
| S1 | Tindak lanjut `Executed` pada kunjungan `Completed`: `PATCH disposition-status` ke `Cancelled` **dengan** `notes` | `409` *"Kunjungan IGD ini sudah selesai, sehingga disposisinya tidak dapat dibatalkan. Daftarkan pasien sebagai episode baru bila ia kembali."*; tindak lanjut dan kunjungan tidak berubah | 1 |
| S2 | Sama dengan S1 **tanpa** `notes` | `409` kalimat yang sama, bukan `400` alasan wajib | 2 |
| S3 | Tindak lanjut `Executed` pada kunjungan yang masih menunggu penutupan: ke `Cancelled` beralasan | `400` *"Perubahan status dari Executed ke Cancelled tidak diperbolehkan."* | 3 |
| S4 | Tindak lanjut `Draft` atau `Confirmed` pada kunjungan berjalan: ke `Cancelled` beralasan | `200` | 4 |
| S5 | Tindak lanjut `Draft` pada kunjungan berjalan: ke `Cancelled` tanpa alasan | `400` *"Alasan pembatalan wajib diisi ketika tindak lanjut dibatalkan."* | — |

---

## 6. Acceptance criteria dan Definition of Done

| # | Kriteria | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | Batalkan `Executed` pada kunjungan `Completed`, beralasan → `409` kalimat aturan 8 persis; nol perubahan | **Terpenuhi — 2 Oktober 2026** | Kalimat sama huruf demi huruf dengan validation §11 aturan 8; uji S1 terbukti |
| 2 | Tanpa alasan → tetap `409`, bukan `400` | **Terpenuhi — 2 Oktober 2026** | Pemeriksaan baru mendahului pemeriksaan alasan; uji S2 terbukti |
| 3 | `Executed` pada kunjungan belum selesai → tetap `400` penjaga | **Terpenuhi — 2 Oktober 2026** | `CanTransition` tidak diubah; uji S3 terbukti |
| 4 | `Draft`/`Confirmed` beralasan → tetap berhasil | **Terpenuhi — 2 Oktober 2026** | Pemeriksaan baru hanya untuk tindak lanjut `Executed`; uji S4 terbukti (S5 pembanding alasan wajib juga terbukti) |
| 5 | Nol jalur lain yang membuka kembali kunjungan selesai | **Terpenuhi** | Bagian 5, baris ketiga |
| 6 | Build 0 error | **Terpenuhi** | Build pemilik 1 Oktober 2026 (artefak DLL); build Rizki 3 Oktober 2026 atas `c1f79f79`: 0 error, 0 warning |

DoD: laporan tracked ✅ (berkas ini).

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Pemanggil yang mencocokkan kode `400` untuk keadaan ini kini menerima `409` |
| Masalah yang diketahui | Membuat tindak lanjut **baru** pada kunjungan `Completed` tidak ditolak saat dibuat, tetapi tidak dapat dipindahkan ke `Executed` dan tidak membuka kembali kunjungan — dicatat kartu, bukan cakupan task ini |
| Risiko tersisa | Rendah. *Ditulis 1 Oktober: "Belum ada build" — sudah tidak berlaku sejak build 1 Oktober 2026* |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Satu berkas source berubah oleh task ini; laporan ini; status roadmap/traceability. Tanpa stage, commit, atau push |
| Langkah berikutnya | Selesai — tidak ada. *Ditulis 1 Oktober: "Pemilik: build, lalu S1–S5. Agent: `BE-IGD-063`"* |

---

## Pemeriksaan bukti uji gabungan — 2 Oktober 2026

Bukti mentah di `QuilvianSystemFrontendDev/test-with-agy/igd/` (`results-tahap-1.json`…`results-tahap-4.json`, skrip `test-tahap-*.mjs`, tangkapan layar `<ID>.png`) dan log backend `Logs/quilvian-backend-20261001.json`, `quilvian-backend-20261002.json`. Ringkasan agen penguji ([laporan uji gabungan](../../../testing/2026-10-01-laporan-uji-gabungan-r313-r314.md)) **tidak** dipakai sebagai bukti: uraian skenarionya pada beberapa task tidak sama dengan panduan, dan daftar `FAIL`-nya tidak cocok dengan JSON mentah.

Build pemilik terbukti dari artefak: `bin/Debug/net9.0/QuilvianSystemBackend.dll` bertanggal 1 Oktober 2026 15.18, sesudah edit source terakhir (14.54); jumlah warning tidak dilaporkan. Source di-commit pemilik sebagai `74a72399`.

| Skenario | Putusan | Yang teramati pada bukti mentah |
| --- | --- | --- |
| `062-S1` | **Terbukti** | `409` dengan kalimat `IGD-DEC-166` |
| `062-S2` | **Terbukti** | Tanpa `notes` tetap `409` kalimat yang sama |
| `062-S3` | **Terbukti** | `400` *"Perubahan status dari Executed ke Cancelled tidak diperbolehkan."* |
| `062-S4` | **Terbukti** | `200`; `dispositionStatus` 4 |
| `062-S5` | **Terbukti** | `400` *"Alasan pembatalan wajib diisi ketika tindak lanjut dibatalkan."* |

Putusan: **✅ selesai** — Build pemilik dan uji API S1–S5 **5 dari 5** pada bukti mentah. Tanpa UAT.

---

## Koreksi laporan — 4 Oktober 2026

Syarat C6 [kesiapan `MVP-8`](../../../evidence/2026-10-04-kesiapan-mvp-8.md). Bagian 5, 6, dan 7 di atas masih
menulis keadaan 1 Oktober (*"Uji belum"*, *"Build belum"*, `NOT RUN`) walau bagian *Pemeriksaan bukti uji gabungan — 2
Oktober 2026* sudah membuktikannya. Baris-baris itu diselaraskan dengan bukti tersebut; teks lama dipertahankan sebagai
catatan miring. **Nol perubahan source, nol build baru, status tetap ✅.** Jalur pembatalan tindak lanjut tidak disentuh
pengerjaan ulang `BE-IGD-061` 3 Oktober 2026, sehingga bukti 2 Oktober tetap berlaku untuk source `c1f79f79`.

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`
