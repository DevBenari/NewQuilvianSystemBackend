# Laporan Perubahan Backend — `BE-IGD-063`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-IGD-063` |
| Judul | Saringan "menunggu penutupan" pada daftar kunjungan |
| Slice | `S5` · `EPIC IGD-13` · `MVP-8` |
| Roadmap | `docs/module-blueprints/igd/roadmap/backend-roadmap.md` bagian R3.14 |
| Trace | `FR-IGD-091`; `IGD-DEC-164`, `168`; `AT-IGD-191` |
| Contract version | API `0.12.0` §9.2 (tidak berubah pada `0.13.0`); validation `0.9.0` §11 aturan 10 — `approved` (`IGD-DEC-170`). Hash cocok dengan manifest 0j, 0j.1, 0j.2 |
| Dependency | `BE-IGD-060` ✅ |
| Klasifikasi | `STANDARD` — skor 6: repository 0, berkas diperiksa 1, berkas diubah 1 (3 berkas), logika bisnis 1, kontrak API 2 (satu query dan dua ruas response baru, aditif), database 1 (baca-saja), keamanan/auth 0, UI/workflow 0 |
| Task mode | `BACKEND` — wewenang pemilik 1 Oktober 2026. Tanpa `dotnet build`, migration, `Program.cs`, eksekusi database, commit |
| Target tulis | `NewQuilvianSystemBackend`: tiga berkas source IGD dan `docs/module-blueprints/igd/` |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `b9076c71` (`rizkiG`) + working tree `BE-IGD-053`, `057`, `058`, `059`, `061`, `062` yang belum di-commit |
| Tanggal | 1 Oktober 2026 |
| Status | ✅ **SELESAI — 2 Oktober 2026.** Build pemilik dan uji API: **6 terbukti penuh** (S1, S4–S8), **2 sebagian** (S2, S3) pada bukti mentah. Dikecualikan dan dicatat: kalimat penahan jenis *pesanan* tidak teramati (data uji tidak dapat membuat pesanan — `403` kewenangan unit), dan pembandingan sebelum–sesudah build tidak dilakukan. Tanpa UAT. *Sebelumnya:* 🟡 **SEBAGIAN — Implementation Complete.** Tiga berkas source (+74/−2); QBE checker `PASS`. **Belum:** build pemilik dan uji API S1–S8 (bagian 5.1) |

### Backend Governance Preflight

| Butir | Hasil |
| --- | --- |
| Governance terbaca | `AGENTS.md` backend; `rules/backend/` suite 1.17.1; kontrak rekayasa dan registry |
| Area / Module | `HealthServices` / `EmergencyInstallationManagement` |
| Owner / prefix registry | Emergency, `Emg`, `ACTIVE / LEGACY`. Nol entity baru |
| Keberlakuan | `TOUCHED LEGACY`: `EmergencyVisitController.GetAll`, `GetById`; `EmergencyVisitService`; `EmergencyVisitResponse` |
| Arketipe endpoint | Transaksi — daftar aggregate ber-lifecycle. Nol endpoint baru |
| Branch | `rizkiG`, upstream `origin/rizkiG` |
| QBE yang berlaku | `QBE-API-001`, `QBE-SVC-001` (aturan saringan dan alasan penahan ditaruh di service), `QBE-PERM-001` (metadata akses tidak berubah) |
| Tidak berlaku | `QBE-ENT-*`, `QBE-NAM-*`, `QBE-CFG-*`, `QBE-MOD-*`, `QBE-CODE-*`, `QBE-TXN-001` (baca-saja), `QBE-VAL-001` (nol masukan baru yang perlu divalidasi; `bool?` diikat kerangka) |
| Checker QBE | Strict, working tree, 15 berkas: 0 `VIOLATION`, 0 `REVIEW`, `PASS` |

---

## 1. Masalah yang diperbaiki

Sejak `BE-IGD-060`, kunjungan tertutup sendiri begitu tindak lanjutnya dilaksanakan. Bila masih ada penahan —
observasi yang belum selesai, kepergian yang belum tuntas, pesanan yang belum ditentukan sikapnya — kunjungan itu
tertahan pada status `Disposed`. Petugas tidak punya cara melihat kunjungan mana saja yang tertahan dan mengapa,
selain membuka satu per satu.

*Contoh.* Pukul 10.00 dokter melaksanakan tindak lanjut pulang untuk Ibu Sari, tetapi observasinya belum diselesaikan.
Kunjungan tetap `Disposed`. Sesudah task ini, `GET /emergency-visits?awaitingClosure=true` menampilkan kunjungan itu
dengan `isAwaitingClosure: true` dan `awaitingClosureReason: "Masih ada observasi yang belum diselesaikan."`, dan
`totalData` memberi jumlah seluruh kunjungan yang sedang tertahan.

---

## 2. Proses bisnis

**Pelaku.** Pemegang izin `EmergencyVisit : Read`.

**Definisi "menunggu penutupan".** Kunjungan yang punya sedikitnya satu tindak lanjut berstatus `Executed` (tidak
terhapus) dan episodenya masih berjalan — statusnya bukan `Completed` dan bukan `Cancelled`.

| Nilai `awaitingClosure` | Baris yang ditampilkan |
| --- | --- |
| kosong | Tidak menyaring — sama seperti sebelumnya |
| `true` | Hanya kunjungan yang menunggu penutupan |
| `false` | Kebalikannya: kunjungan yang sudah `Completed`/`Cancelled`, atau yang belum punya tindak lanjut `Executed` |

**Dua ruas baru pada tiap baris.**

| Ruas | Isi |
| --- | --- |
| `isAwaitingClosure` | `true` bila baris itu menunggu penutupan — terisi dengan atau tanpa saringan |
| `awaitingClosureReason` | Kalimat penjaga penutupan yang sudah ada; `null` bila baris tidak menunggu penutupan, atau bila tidak ada lagi penahan |

Kalimat yang mungkin muncul, seluruhnya milik `EmergencyDispositionService.ValidateVisitClosureAsync`:

| Penahan | Kalimat |
| --- | --- |
| Observasi masih aktif | *"Masih ada observasi yang belum diselesaikan."* |
| Kepergian belum tiba dan belum dibatalkan | *"Masih ada proses kepergian pasien yang belum selesai."* |
| Pesanan belum ditentukan sikapnya | *"Masih ada pesanan yang belum ditentukan sikapnya: …."* |
| Status kunjungan bukan `Disposed` padahal tindak lanjutnya `Executed` (data sebelum `BE-IGD-060`) | *"Kunjungan hanya dapat diselesaikan setelah keputusan tindak lanjut ditetapkan."* |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kartu `BE-IGD-063`; API §9.2; validation §11 aturan 10; `03-frontend-architecture.md` baris saringan dan penanda.
Source: `EmergencyVisitController` (`GetAll`, `GetById`, `ToResponse`, `NormalizePaging`), `EmergencyVisitDtos.cs`,
`EmergencyVisitService` (`EpisodeMasihBerjalan`, `TryCloseAfterDispositionAsync`),
`EmergencyDispositionService.ValidateVisitClosureAsync`, `EmgDisposition`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `…/DTOs/EmergencyVisitDtos.cs` | `EmergencyVisitResponse` + `IsAwaitingClosure` (`bool`), `AwaitingClosureReason` (`string?`). +3 baris |
| `…/Services/EmergencyVisitService.cs` | + `SaringMenungguPenutupan(query, menungguPenutupan)` — satu sub-kueri `EXISTS` atas tindak lanjut `Executed`; + `AmbilAlasanMenungguPenutupanAsync(kunjungan, ct)` — menghitung alasan penahan hanya untuk baris yang diserahkan. +42 baris |
| `…/Controllers/EmergencyVisitController.cs` | `GetAll` + query `awaitingClosure`, pemanggilan saringan, dan pengisian dua ruas untuk baris halaman; `GetById` mengisi dua ruas yang sama; + overload `ToResponse(kunjungan, alasan)`. +29/−2 baris |

**Nol baris komentar ditambah** (hitungan komentar ketiga berkas tetap 96, 84, 262). **Tidak disentuh:**
`EmergencyDispositionService`, `Program.cs`, model, migration, konfigurasi EF, frontend.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Aditif, sesuai API §9.2: satu query opsional dan dua ruas response. Ruas lama, urutan, dan paginasi tidak berubah |
| Database | Nol schema. Baca-saja |
| Keamanan/Auth | `NOT APPLICABLE` — `[AccessAction]` dan `[AccessPermission]` kedua action tidak berubah |

### 3.4 Cara alasan penahan dihitung

| Langkah | Jumlah kueri |
| --- | --- |
| Hitung `totalData` dan ambil baris halaman (sudah ada) | 2 |
| Dari baris halaman yang episodenya masih berjalan, cari mana yang punya tindak lanjut `Executed` | 1 — dilewati bila tidak ada kandidat |
| Untuk tiap baris halaman yang memang menunggu penutupan, jalankan penjaga penutupan | Paling banyak 4 per baris, berhenti pada penahan pertama |

Penjaga tidak pernah dijalankan untuk baris di luar halaman, dan tidak dijalankan untuk baris halaman yang tidak
menunggu penutupan. Daftar tanpa saringan yang halamannya tidak memuat satu pun kunjungan tertahan hanya bertambah
satu kueri.

### 3.5 Keputusan pelaksanaan dan selisih terhadap kartu

| Keputusan | Alasan |
| --- | --- |
| "Belum selesai" dibaca sebagai episode masih berjalan: bukan `Completed` **dan** bukan `Cancelled` | Kunjungan yang dibatalkan tidak akan pernah ditutup, jadi tidak "menunggu". Sama dengan ukuran yang dipakai `TryCloseAfterDispositionAsync` |
| `GET /{id}` ikut mengisi dua ruas — **selisih**, kartu hanya menyebut `GET /` | Ruasnya ada pada DTO yang sama. Tanpa pengisian, detail kunjungan yang tertahan akan menjawab `isAwaitingClosure: false`, yaitu keterangan yang salah. Biayanya satu kueri tambahan bila kunjungan masih berjalan |
| Aturan saringan dan perhitungan alasan ditaruh di `EmergencyVisitService`, bukan di controller | `QBE-SVC-001`; ukuran "menunggu penutupan" tinggal di satu tempat bersama pemicu penutupan |
| Kalimat alasan diambil dengan memanggil penjaga yang sudah ada, bukan ditulis ulang | Kartu meminta kalimat penjaga yang sudah ada; menyalin aturannya ke kueri gabungan akan membuat dua sumber aturan |

---

## 4. Dokumentasi endpoint

#### Health Services / Emergency Installation Management / Emergency Visit

Base URL: `api/v1/health-services/emergency-installation-management/emergency-visits`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/` | Daftar kunjungan IGD — + query `awaitingClosure`; tiap baris + `isAwaitingClosure`, `awaitingClosureReason` | `EmergencyVisit : Read` (tidak berubah) |
| `GET` | `/{id}` | Detail kunjungan IGD — + `isAwaitingClosure`, `awaitingClosureReason` | `EmergencyVisit : Read` (tidak berubah) |

Query baru pada `GET /`:

| Nama | Tipe | Wajib | Keterangan |
| --- | --- | --- | --- |
| `awaitingClosure` | `bool?` | Tidak | `true` hanya yang menunggu penutupan; `false` kebalikannya; kosong tidak menyaring. Dapat digabung dengan saringan lain |

Contoh potongan response `GET /?awaitingClosure=true`:

```json
{
  "pageNumber": 1,
  "pageSize": 25,
  "totalData": 3,
  "items": [
    {
      "emergencyVisitNumber": "IGD-…",
      "visitStatus": 7,
      "isAwaitingClosure": true,
      "awaitingClosureReason": "Masih ada observasi yang belum diselesaikan."
    }
  ]
}
```

Nilai `visitStatus` pada contoh hanya ilustrasi; angka enum sebenarnya mengikuti `EmergencyVisitStatus`.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| QBE checker Strict (working tree) | 15 berkas, 0 `VIOLATION`, 0 `REVIEW`, `Final result: PASS` | `PASS` | Keluaran perintah 1 Oktober 2026 |
| Review diff | DTO +3, service +42, controller +29/−2; akhiran baris CRLF dipertahankan; hitungan baris komentar ketiga berkas tidak berubah | `PASS` | Hitungan baris sebelum dan sesudah |
| Nol `N+1` atas seluruh hasil | Penjaga hanya dipanggil di dalam `AmbilAlasanMenungguPenutupanAsync`, yang menerima baris halaman (`entities` sesudah `Skip`/`Take`) | `PASS` | Baca source |
| Tanpa parameter, kueri daftar tidak berubah | Saringan hanya dipasang bila `awaitingClosure.HasValue` | `PASS` | Baca source |
| `dotnet build` | Build pemilik 1 Oktober 2026 (DLL 15.18; jumlah warning tidak dilaporkan); build Rizki 3 Oktober 2026 atas `c1f79f79`, yang memuat source task ini tanpa perubahan: 0 error, 0 warning | `PASS` | Bagian *Pemeriksaan bukti uji gabungan — 2 Oktober 2026*; laporan `BE-IGD-061`, *Build pemilik — dicatat 4 Oktober 2026*. *Ditulis 1 Oktober: `NOT RUN`* |
| Uji API S1–S8 | 6 terbukti penuh (S1, S4–S8), 2 sebagian (S2, S3 — dikecualikan) | `PASS` dengan pengecualian | Bagian *Pemeriksaan bukti uji gabungan — 2 Oktober 2026*; dikuatkan uji gabungan `MVP-8` 4 Oktober 2026. *Ditulis 1 Oktober: `NOT RUN`* |

Uji manual: `REQUIRED` — lewat API; layar menyusul pada `FE-IGD-041`.

### 5.1 Skenario uji untuk pemilik

| # | Skenario | Hasil yang diharapkan | Kriteria |
| ---: | --- | --- | ---: |
| S1 | `GET /?awaitingClosure=true` | Hanya kunjungan yang punya tindak lanjut `Executed` dan belum `Completed`/`Cancelled`; tiap baris `isAwaitingClosure: true` | 1 |
| S2 | Kunjungan tertahan oleh observasi aktif, lalu oleh kepergian belum tiba, lalu oleh pesanan belum disikapi | `awaitingClosureReason` berturut-turut: *"Masih ada observasi yang belum diselesaikan."*, *"Masih ada proses kepergian pasien yang belum selesai."*, *"Masih ada pesanan yang belum ditentukan sikapnya: …."* | 2 |
| S3 | `GET /` tanpa `awaitingClosure`, dibandingkan dengan hasil sebelum build ini | Baris, urutan, `totalData`, dan ruas lama sama; tiap baris bertambah dua ruas baru | 3 |
| S4 | `GET /?awaitingClosure=true&pageSize=1` | `totalData` tetap jumlah seluruh kunjungan yang menunggu penutupan, `items` berisi satu baris | 4 |
| S5 | `GET /?awaitingClosure=false` | `totalData`-nya ditambah `totalData` S1 sama dengan `totalData` tanpa saringan; nol baris ber-`isAwaitingClosure: true` | 1 |
| S6 | `GET /{id}` untuk kunjungan tertahan, lalu untuk kunjungan `Completed` | Yang pertama `isAwaitingClosure: true` beserta alasannya; yang kedua `false` dan `null` | — |
| S7 | `awaitingClosure=true` digabung `search` atau `startDate`/`endDate` | Kedua saringan berlaku bersama | 1 |
| S8 | Selesaikan penahan sebuah kunjungan tertahan (`BE-IGD-061`), lalu ulangi S1 | Kunjungan itu tertutup dan tidak lagi muncul; `totalData` berkurang satu | 1, 4 |

---

## 6. Acceptance criteria dan Definition of Done

| # | Kriteria | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | `awaitingClosure=true` hanya menampilkan kunjungan berdisposisi dilaksanakan yang belum selesai | **Terpenuhi — 2 Oktober 2026** | `SaringMenungguPenutupan`; uji S1, S5, S7 terbukti. 4 Oktober 2026: `061-S13` dan `061-S19-S5` (daftar `awaitingClosure=true&search=…` tepat satu baris) |
| 2 | Tiap baris memuat alasan penahan memakai kalimat penjaga yang sudah ada | **Terpenuhi dengan pengecualian — 2 Oktober 2026** | Alasan diambil dari `ValidateVisitClosureAsync`, tanpa kalimat baru. Kalimat observasi dan kepergian teramati (S2; 4 Oktober 2026 juga `061-S13` observasi Dieskalasi dan `061-S19-S5` kepergian, sesudah penjaga diubah `BE-IGD-061`); kalimat pesanan tidak teramati — pesanan tidak dapat dibuat karena `403` kewenangan unit (`BE-IGD-039`) |
| 3 | Tanpa parameter, hasil daftar sama persis seperti sebelumnya | **Terpenuhi dengan pengecualian — 2 Oktober 2026** | Saringan bersyarat `HasValue`; tambahan hanya dua ruas aditif. S3: ruas lama tetap dan dua ruas baru ada; pembandingan sebelum–sesudah build tidak dilakukan |
| 4 | `totalData` memberi jumlah kunjungan yang menunggu penutupan | **Terpenuhi — 2 Oktober 2026** | `CountAsync` dijalankan sesudah saringan; uji S4 terbukti. 4 Oktober 2026: *"23 kunjungan menunggu penutupan"* = `totalData` (`FE-IGD-041`) |
| 5 | Alasan dihitung hanya untuk baris halaman | **Terpenuhi** | Bagian 3.4 dan bagian 5, baris ketiga |
| 6 | Build 0 error | **Terpenuhi** | Build pemilik 1 Oktober 2026 (artefak DLL); build Rizki 3 Oktober 2026 atas `c1f79f79`: 0 error, 0 warning |

DoD: laporan tracked ✅ (berkas ini). `FE-IGD-041` boleh mulai pada sisi source; uji layarnya menunggu build ini.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Response aksi tulis kunjungan (buat, ubah, ubah status, selesaikan, mulai triage, konfirmasi waktu tiba) memakai DTO yang sama tetapi **tidak** menghitung dua ruas baru: nilainya selalu `false` dan `null`. Layar membaca kedua ruas dari `GET /` atau `GET /{id}` |
| Masalah yang diketahui | Kunjungan lama yang tindak lanjutnya `Executed` tetapi statusnya belum `Disposed` ikut tampil sebagai menunggu penutupan dengan kalimat *"Kunjungan hanya dapat diselesaikan setelah keputusan tindak lanjut ditetapkan."* — benar menurut definisi kontrak, tetapi kalimatnya kurang menjelaskan keadaan data lama itu |
| Risiko tersisa | *Ditulis 1 Oktober: "Belum ada build" — tidak berlaku lagi sejak build 1 Oktober 2026.* Pada `awaitingClosure=true` dengan `pageSize` maksimum (100), penjaga dijalankan sampai 100 kali — sampai ±400 kueri kecil per permintaan. Pada ukuran halaman bawaan (25) paling banyak ±100 |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Tiga berkas source berubah oleh task ini; laporan ini; status roadmap/traceability. Tanpa stage, commit, atau push |
| Langkah berikutnya | Selesai — tidak ada. *Ditulis 1 Oktober: "Pemilik: build, lalu S1–S8. Agent: `FE-IGD-042`, lalu `FE-IGD-041`"* |

---

## Pemeriksaan bukti uji gabungan — 2 Oktober 2026

Bukti mentah di `QuilvianSystemFrontendDev/test-with-agy/igd/` (`results-tahap-1.json`…`results-tahap-4.json`, skrip `test-tahap-*.mjs`, tangkapan layar `<ID>.png`) dan log backend `Logs/quilvian-backend-20261001.json`, `quilvian-backend-20261002.json`. Ringkasan agen penguji ([laporan uji gabungan](../../../testing/2026-10-01-laporan-uji-gabungan-r313-r314.md)) **tidak** dipakai sebagai bukti: uraian skenarionya pada beberapa task tidak sama dengan panduan, dan daftar `FAIL`-nya tidak cocok dengan JSON mentah.

Build pemilik terbukti dari artefak: `bin/Debug/net9.0/QuilvianSystemBackend.dll` bertanggal 1 Oktober 2026 15.18, sesudah edit source terakhir (14.54); jumlah warning tidak dilaporkan. Source di-commit pemilik sebagai `74a72399`.

| Skenario | Putusan | Yang teramati pada bukti mentah |
| --- | --- | --- |
| `063-S1` | **Terbukti** | `totalData` 13; ketiga belas baris `visitStatus` 7 dan `isAwaitingClosure: true` |
| `063-S2` | **Sebagian** | Dua dari tiga kalimat teramati: observasi dan kepergian. Kalimat pesanan tidak teramati; skrip tetap menulis lulus |
| `063-S3` | **Sebagian** | Tanpa saringan: `totalData` 68, tiap baris memuat dua ruas baru di samping ruas lama. Tidak dibandingkan dengan hasil sebelum build |
| `063-S4` | **Terbukti** | `pageSize=1` → `totalData` 13, satu baris |
| `063-S5` | **Terbukti** | `awaitingClosure=false` → 55; 55 + 13 = 68; nol baris bertanda |
| `063-S6` | **Terbukti** | `GET /{id}` kunjungan tertahan `true` + alasan; kunjungan `Completed` `false` + `null` |
| `063-S7` | **Terbukti** | Digabung `search` nomor kunjungan → 1 baris, tetap bertanda |
| `063-S8` | **Terbukti** | Sesudah penahan dibereskan: 13 → 12; kunjungan itu tidak lagi muncul |

**Catatan.**

- **Temuan lama, bukan dari task ini.** `search` pada `GET /emergency-visits` mencari nomor kunjungan, keluhan, tiga lokasi, nama sementara, dan catatan — **tidak** nama pasien maupun nomor rekam medis. Layar Pengkajian menulis *"Cari No. RM, nama pasien…"*; lihat laporan `FE-IGD-041`.

Putusan: **✅ selesai** — Build pemilik dan uji API: **6 terbukti penuh** (S1, S4–S8), **2 sebagian** (S2, S3) pada bukti mentah. Dikecualikan dan dicatat: kalimat penahan jenis *pesanan* tidak teramati (data uji tidak dapat membuat pesanan — `403` kewenangan unit), dan pembandingan sebelum–sesudah build tidak dilakukan. Tanpa UAT.

---

## Koreksi laporan — 4 Oktober 2026

Syarat C6 [kesiapan `MVP-8`](../../../evidence/2026-10-04-kesiapan-mvp-8.md). Bagian 5, 6, dan 7 di atas masih
menulis keadaan 1 Oktober (*"Uji belum"*, *"Build belum"*, `NOT RUN`) walau bagian *Pemeriksaan bukti uji gabungan — 2
Oktober 2026* sudah membuktikannya. Baris-baris itu diselaraskan dengan bukti tersebut; teks lama dipertahankan sebagai
catatan miring. **Nol perubahan source, nol build baru, status tetap ✅** dengan dua pengecualian yang sudah tercatat
2 Oktober (kalimat penahan pesanan; pembandingan sebelum–sesudah build).

**Bukti ulang sesudah perubahan penjaga.** Pengerjaan ulang `BE-IGD-061` (3 Oktober 2026, `c1f79f79`) mengubah
`ValidateVisitClosureAsync`, sumber alasan penahan task ini. Uji gabungan `MVP-8` 4 Oktober 2026 membuktikan saringan
dan alasannya tetap benar sesudah perubahan itu: `061-S13` (alasan *"Masih ada observasi yang belum diselesaikan."*
untuk observasi Dieskalasi), `061-S19-S5` (alasan kepergian sesudah observasi diselesaikan), dan `FE-IGD-041` U2, U6,
U7, R pada hasil build — lihat laporan [`BE-IGD-061`](BE-IGD-061.md) dan [`FE-IGD-041`](../frontend/FE-IGD-041.md),
bagian *Pemeriksaan bukti uji gabungan `MVP-8`*.

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`
