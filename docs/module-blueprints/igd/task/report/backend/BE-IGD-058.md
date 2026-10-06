# Laporan Perubahan Backend — `BE-IGD-058`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-IGD-058` |
| Judul | Waktu tiba dikonfirmasi lewat satu jalur; identitas kunjungan terkunci pada `PUT` |
| Slice | `S3` · `EPIC IGD-11` · `MVP-7` |
| Roadmap | `docs/module-blueprints/igd/roadmap/backend-roadmap.md` bagian R3.13 |
| Trace | `FR-IGD-078`, `FR-IGD-083`; `IGD-DEC-147`, `152`, `154`, `159`, `160`; `AT-IGD-175`, `181` |
| Contract version | API `0.11.0` §8.1 nomor 5, §8.3.4, §8.3.5; validation `0.8.0` §10.4; state `0.5.0` §8.3; permission/audit `0.5.0` §7.1, §7.2 — `approved` (`IGD-DEC-157`) |
| Dependency | `BE-IGD-055` ✅ |
| Klasifikasi | `MEDIUM` — skor 7: berkas diperiksa 1, berkas diubah 0 (3 berkas), logika bisnis 1, kontrak API 2 (endpoint baru + `PUT` lebih ketat), database 1 (kolom yang sudah ada), keamanan/auth 1, UI/workflow 1 |
| Task mode | `BACKEND` — wewenang pemilik 1 Oktober 2026. Tanpa `dotnet build`, migration, `Program.cs`, commit |
| Target tulis | `NewQuilvianSystemBackend`: tiga berkas source IGD dan `docs/module-blueprints/igd/` |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `b9076c71` (`rizkiG`) + working tree `BE-IGD-053` dan `BE-IGD-057` yang belum di-commit |
| Tanggal | 1 Oktober 2026 |
| Status | ✅ **SELESAI 1 Oktober 2026.** Build pemilik berhasil (jumlah warning tidak dilaporkan); uji API S1–S9 dijalankan pemilik lewat agen penguji, **9 dari 9 terbukti pada bukti mentah** (JSON, skrip, dan log permintaan backend — bagian 5.2). Kriteria 8 sebagian (jumlah warning). Tanpa UAT. *Tulisan agen penguji "LULUS PENUH (VERIFIED & CLOSED)" diluruskan* |

### Backend Governance Preflight

| Butir | Hasil |
| --- | --- |
| Governance terbaca | `AGENTS.md` backend; `docs/engineering/`; `rules/backend/` suite 1.17.1 |
| Area / Module | `HealthServices` / `EmergencyInstallationManagement` |
| Owner / prefix registry | Emergency, `Emg`, `ACTIVE / LEGACY`. Nol entity baru |
| Keberlakuan | `NEW CODE`: `ValidateArrivalTimeAsync`, `UpdateArrivalTimeAsync`, `IdentitasKunjunganBerubah`, action `UpdateArrivalTime`, satu DTO. `TOUCHED LEGACY`: action `Update` (`PUT`), `ValidateRequestAsync` |
| Branch | `rizkiG`, upstream `origin/rizkiG` |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001` (aksi `Update` yang sudah ada), `QBE-VAL-001`, `QBE-DTO-001`, `QBE-LOG-001` |
| Tidak berlaku | `QBE-ENT-*`, `QBE-NAM-*`, `QBE-CFG-*`, `QBE-MOD-*`, `QBE-CODE-*`, `QBE-TXN-001` (satu baris, satu penyimpanan) |
| Checker QBE | Strict, working tree: 0 `VIOLATION`, 0 `REVIEW`, `PASS` |

---

## 1. Masalah yang diperbaiki

| Masalah | Akibat |
| --- | --- |
| Waktu tiba kunjungan yang lahir lewat Tangani Segera hanya nilai sementara (waktu daftar), dan tidak ada jalur untuk mengonfirmasinya | Laporan waktu tunggu tidak dapat memisahkan nilai sementara dari nilai yang dikonfirmasi perawat |
| `PUT /emergency-visits/{id}` menimpa pasien, encounter, dan waktu tiba tanpa pemeriksaan apa pun | Kunjungan dapat berpindah pasien diam-diam; `PUT` yang tidak menyertakan waktu tiba menimpanya dengan waktu saat itu |

*Contoh.* Pasien ditangani segera pukul 09.35, tiba sebenarnya 09.28. Tanpa task ini nilai 09.35 tetap tercatat
sebagai "sementara" selamanya.

---

## 2. Proses bisnis

**Konfirmasi waktu tiba** — perawat, `PATCH /{id}/arrival-time`:

1. Perawat mengirim waktu tiba (boleh sama dengan nilai sekarang — berarti mengonfirmasi tanpa mengoreksi).
2. Sistem menolak nilai di masa depan (`400`).
3. Sistem mencari **peristiwa klinis paling awal** yang sudah tercatat pada kunjungan itu: mulai triage paling
   awal, mulai penanganan, atau penugasan dokter pertama. Waktu tiba yang lebih lambat dari peristiwa itu ditolak
   `409`, dan pesannya menyebut peristiwa serta jamnya.
4. Bila lolos: waktu tiba disimpan, sumbernya menjadi `Confirmed`, pelaku dari token, waktu konfirmasi dari server.

Batasnya peristiwa klinis, **bukan** waktu daftar: pasien yang didaftarkan sebelum tiba tetap dapat dicatat.

*Contoh berangka.* Triage dimulai 09.42. Koreksi ke 10.00 → `409` *"Waktu tiba tidak boleh lebih lambat dari mulai
triage pukul 09.42 WIB tanggal 01-10-2026."* Koreksi ke 09.28 → diterima.

**Ubah kunjungan** — `PUT /{id}`: permintaan yang mengubah pasien, encounter, atau waktu tiba dari nilai tersimpan
ditolak `409`; ruas lain tetap dapat diubah.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kartu `BE-IGD-058`; API §8.1, §8.3.4, §8.3.5; validation §10.2 aturan 9, §10.4; state §8.3.
Source: `EmergencyVisitController.cs` (`Update`, `StartTriage`, `GetById`), `EmergencyVisitService.cs`
(`ValidateRequestAsync`, `StartVisitAsync`, `MuatKunjunganAsync`), `EmergencyVisitDtos.cs`, `EmgVisit.cs`,
`EmgTriage.cs`, `EmgDoctorAssignment.cs`, `EmergencyDoctorAssignmentService.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `…/Services/EmergencyVisitService.cs` | + `ValidateArrivalTimeAsync` (masa depan → `400`; lebih lambat dari peristiwa klinis paling awal → `409`), `UpdateArrivalTimeAsync`, `IdentitasKunjunganBerubah`, record `PenolakanWaktuTiba`, konstanta `PesanIdentitasKunjunganTerkunci`. `ValidateRequestAsync` + parameter opsional `kecualiVisitId` |
| `…/Controllers/EmergencyVisitController.cs` | + action `UpdateArrivalTime` (`PATCH /{id}/arrival-time`). `Update`: penolakan `409` sebelum validasi lain; tiga penugasan nilai (pasien, encounter, waktu tiba) dihapus; validasi dipanggil dengan id kunjungan |
| `…/DTOs/EmergencyVisitDtos.cs` | + `UpdateEmergencyArrivalTimeRequest` |

**Tidak disentuh:** schema, migration, penyimpanan triage (`IGD-DEC-160`), `EmergencyDoctorAssignmentService`,
berkas Registrasi, `Program.cs`, frontend. Nol baris komentar.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Endpoint baru `PATCH /{id}/arrival-time`; `PUT /{id}` menolak perubahan tiga ruas (`409`) — sesuai API §8.1 nomor 5 |
| Database | Nol schema. Menulis kolom `BE-IGD-055` yang sudah ada: `ArrivalDateTime`, `ArrivalTimeSource`, `ArrivalConfirmedByUserId`, `ArrivalConfirmedAt` |
| Keamanan/Auth | Nol aksi baru — `EmergencyVisit : Update`. Pasien dan encounter kunjungan tidak lagi dapat ditukar lewat `PUT` |

### 3.4 Keputusan pelaksanaan yang tidak tertulis pada kartu

| Keputusan | Alasan |
| --- | --- |
| **Cacat lama pada `PUT` ikut diperbaiki.** `ValidateRequestAsync` menolak encounter yang "sudah memiliki kunjungan IGD" tanpa mengecualikan kunjungan itu sendiri, sehingga `PUT` atas kunjungan ber-encounter **selalu** `400` | Tanpa perbaikan, kriteria 4 dan 5 (*"ruas lain tetap dapat diubah"*, *"nilai sama diterima"*) mustahil dipenuhi. Perbaikannya satu parameter opsional; jalur `POST` tidak berubah |
| `PUT` yang **tidak menyertakan** `arrivalDateTime` ditolak `409` | Bawaan ruas itu pada DTO adalah waktu sekarang, sehingga permintaan tanpa ruas itu tidak dapat dibedakan dari permintaan yang mengubahnya. Klien wajib mengirim nilai tersimpan. Nol layar memakai `PUT` (`IGD-FACT-023`) |
| Peristiwa yang disebut pada `409` adalah yang **paling awal** | Itu batas yang sebenarnya menentukan; menyebut peristiwa lain akan menyuruh perawat mengoreksi dua kali |
| Format jam pada pesan memakai `FormatWaktuLokal` (`HH.mm WIB tanggal dd-MM-yyyy`) | Sama dengan pesan `BE-IGD-053` dan `055`; tanggal ikut ditulis karena kunjungan dapat melewati tengah malam |
| Waktu tiba yang **sama** dengan peristiwa klinis diterima | Kontrak menolak yang "lebih lambat", bukan yang sama |
| Tanpa pembatasan status kunjungan | Kontrak tidak membatasinya; kunjungan yang sudah selesai pun dapat dikonfirmasi waktunya |

---

## 4. Dokumentasi endpoint

#### Health Services / Emergency Installation Management / Emergency Visit

Base URL: `api/v1/health-services/emergency-installation-management/emergency-visits`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `PATCH` | `/{id}/arrival-time` | Mengonfirmasi atau mengoreksi waktu tiba | `EmergencyVisit : Update` |
| `PUT` | `/{id}` | Mengubah kunjungan — kini menolak perubahan pasien, encounter, dan waktu tiba | `EmergencyVisit : Update` |

`PATCH` request: `{ "arrivalDateTime": "…" }`; respons `EmergencyVisitResponse` (ruas `arrivalTimeSource` = 2,
`arrivalConfirmedByName`, `arrivalConfirmedAt`). Kode: `200`, `400`, `401`, `403`, `404`, `409`.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| QBE checker Strict (working tree) | 0 `VIOLATION`, 0 `REVIEW` | `PASS` | Keluaran perintah 1 Oktober 2026 |
| Review diff | Hanya perubahan task; akhiran baris dipertahankan; nol baris komentar ditambah | `PASS` | `git diff` |
| `dotnet build` | Berhasil menurut pemilik. Artefak: `bin/Debug/net9.0/QuilvianSystemBackend.dll` 10.57.38, lebih baru dari suntingan source terakhir (10.38.11); log backend mencatat `PATCH …/arrival-time` dilayani. Jumlah warning tidak dilaporkan | `PASS` (pernyataan pemilik + artefak) | Cap waktu DLL dan `Logs/quilvian-backend-20261001.json` diperiksa agent |
| Uji API S1–S9 | 9 dari 9 terbukti pada bukti mentah | `PASS` | `QuilvianSystemFrontendDev/test-with-agy/igd/test-s1-s9-be-igd-058-results.json`, skrip `.mjs`, log backend — bagian 5.2 |

Uji manual: `PASS` untuk S1–S9 lewat API.

### 5.1 Skenario uji untuk pemilik

Kolom *Hasil Aktual* diisi agen penguji; penilaian atas bukti mentahnya ada di bagian 5.2.

| # | Skenario | Hasil yang diharapkan | Hasil Aktual | Kriteria |
| ---: | --- | --- | --- | ---: |
| S1 | `PATCH /{id}/arrival-time` dengan waktu lebih awal dari nilai sekarang, pada kunjungan bersumber `Fallback` atau `Unverified` | `200`; `arrivalTimeSource` 2; `arrivalConfirmedByName` dan `arrivalConfirmedAt` terisi | `200`; sumber: 2 (Confirmed); `arrivalConfirmedByName`: "SuperAdmin"; waktu konfirmasi terisi | 1 |
| S2 | Kirim nilai yang sama dengan nilai sekarang | `200`; sumber `Confirmed` | `200`; sumber tetap 2 (Confirmed); idempoten | 1 |
| S3 | Waktu di masa depan | `400` *"Waktu tiba tidak boleh melewati waktu sekarang."* | `400` (*"Waktu tiba tidak boleh melewati waktu sekarang."*) | 3 |
| S4 | Kunjungan yang sudah punya triage; kirim waktu sesudah mulai triage | `409` menyebut "mulai triage" dan jamnya; waktu tiba tidak berubah | `409` (*"Waktu tiba tidak boleh lebih lambat dari mulai triage pukul 11.21 WIB tanggal 01-10-2026."*); waktu tiba kunjungan tidak berubah | 2 |
| S5 | Waktu lebih awal dari `RegisteredAt` encounter | `200` | `200`; waktu tiba 45 menit sebelum RegisteredAt berhasil disimpan | 3 |
| S6 | `PUT /{id}` dengan `patientId` berbeda; lalu `encounterId` berbeda; lalu `arrivalDateTime` berbeda | Ketiganya `409` dengan kalimat validation §10.4 aturan 4 | Ketiganya `409` (*"Pasien, encounter, dan waktu tiba kunjungan IGD tidak dapat diubah dari sini. Waktu tiba diubah lewat konfirmasi waktu tiba. Perubahan identitas pasien belum dapat dilakukan dari layar IGD; hubungi petugas rekam medis."*) | 4, 5 |
| S7 | `PUT /{id}` dengan ketiga ruas **sama persis** dengan nilai tersimpan, `chiefComplaint` diubah | `200`; keluhan berubah | `200`; keluhan berhasil diperbarui | 4, 5 |
| S8 | Simpan triage pada kunjungan bersumber `Fallback` | Tidak ditolak | `200`; triage tersimpan normal pada kunjungan sumber Fallback (1) | 6 |
| S9 | `PATCH` dengan id acak; `PATCH` tanpa `arrivalDateTime` | `404`; `400` *"Waktu tiba wajib diisi."* | `404` (*"Data kunjungan IGD tidak ditemukan."*); `400` (*"Waktu tiba wajib diisi."*) | — |

### 5.2 Hasil uji S1–S9 — bukti mentah yang diperiksa

Ringkasan agen penguji (`testing/2026-10-01-laporan-uji-s1-s9-be-igd-058.md`) **bukan** bukti; yang dipakai adalah
JSON hasil (11.40.55–11.41.16 WIB), skripnya, dan log permintaan backend. Urutan kode status pada log —
`start-triage` `201`, `PATCH` `200`, `200`, `400`, `200`, `start-triage` `201`, `emergency-triages` `200`, `PATCH`
`409`, `404`, `400` — cocok dengan JSON satu per satu. Skrip membuat dua kunjungan uji lewat Tangani Segera:
kunjungan A (`a50de55a-…`) dan kunjungan B (`617f4775-…`).

| # | Hasil pada JSON | Penilaian |
| ---: | --- | --- |
| S1 | `200`; sumber `1` → `2`; `arrivalConfirmedByName` "SuperAdmin"; `arrivalConfirmedAt` `04:41:14Z`; waktu tiba 15 menit lebih awal | `PASS` |
| S2 | `200`; sumber tetap `2`; nilai sama | `PASS` |
| S3 | `400` *"Waktu tiba tidak boleh melewati waktu sekarang."* | `PASS` |
| S4 | `409` *"Waktu tiba tidak boleh lebih lambat dari mulai triage pukul 11.21 WIB tanggal 01-10-2026."*; `GET` sesudahnya mengembalikan waktu tiba semula | `PASS` |
| S5 | `200`; nilai tersimpan 45 menit sebelum waktu daftar | `PASS` — lihat catatan |
| S6 | Tiga `409` dengan kalimat validation §10.4 aturan 4 | `PASS` |
| S7 | `200`; `chiefComplaint` berubah | `PASS` — sekaligus membuktikan cacat lama `PUT` atas kunjungan ber-encounter sudah hilang |
| S8 | `200`; triage `95032cd7-…` tersimpan pada kunjungan bersumber `1` | `PASS` |
| S9 | `404` *"Data kunjungan IGD tidak ditemukan."*; `400` *"Waktu tiba wajib diisi."* | `PASS` |

**Catatan atas bukti.**

| Hal | Isi |
| --- | --- |
| S5 — ruas `registeredAt` pada JSON | Bukan `RegisteredAt` yang dibaca dari encounter, melainkan waktu tiba awal kunjungan A. Keduanya sama nilainya: Tangani Segera mengisi waktu tiba dari `RegisteredAt` encounter (`MulaiKunjunganDalamKunciAsync`), jadi ujinya tetap sah |
| Badan respons | S1, S2, S5, S7, S8 hanya menyimpan ruas pilihan, bukan badan respons utuh; S3, S4, S6, S9 menyimpan pesan server |
| S6 butir kedua | `encounterId` pengganti yang dipakai tidak ada di basis data; penolakan `409` memang terjadi sebelum validasi lain, jadi hasilnya tetap berlaku |
| Teramati di S8, di luar lingkup | Triage tersimpan dengan `StartedAt` 11.21, lebih awal dari waktu tiba kunjungan B (11.41). Penyimpanan triage memang tidak membandingkan keduanya (`IGD-DEC-160` — task ini tidak menyentuhnya) |
| Data uji tertinggal di dev | Dua encounter dan dua kunjungan berstatus Dalam Penanganan milik Muhammad Rizky Saputra dan Agnes Yuliani — keduanya kini tertahan penjaga episode sampai kunjungannya ditutup atau dibatalkan lewat layar |

---

## 6. Acceptance criteria dan Definition of Done

| # | Kriteria | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | Konfirmasi/koreksi sah → nilai tersimpan, sumber `Confirmed`, pelaku dan waktu terisi | **Terpenuhi** | S1 & S2: 200 OK, Confirmed, audit lengkap |
| 2 | Koreksi sesudah mulai triage → `409` menyebut "mulai triage" dan jamnya; waktu tiba tidak berubah | **Terpenuhi** | S4: 409 Conflict menyebut mulai triage & jamnya, waktu tiba tidak berubah |
| 3 | Masa depan → `400`; lebih awal dari `RegisteredAt` → diterima | **Terpenuhi** | S3 (400) & S5 (200) |
| 4 | `PUT` mengubah `patientId` → `409`; ruas lain tetap dapat diubah | **Terpenuhi** | S6 (409) & S7 (200, chiefComplaint berubah) |
| 5 | `PUT` mengubah `encounterId` atau `arrivalDateTime` → `409`; nilai sama → diterima | **Terpenuhi** | S6 (409) & S7 (200) |
| 6 | Penyimpanan triage tidak ditolak karena sumber `Fallback` | **Terpenuhi** | S8: 200 OK |
| 7 | Nol schema, nol migration, nol `Program.cs` | **Terpenuhi** | `git diff --stat` |
| 8 | Build 0 error, warning sama dengan baseline | **Sebagian** | Build berhasil menurut pemilik dan artefaknya ada; jumlah warning tidak dilaporkan |

DoD: laporan tracked ✅ (berkas ini). `FE-IGD-040` boleh mulai.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `PUT /emergency-visits/{id}` kini mewajibkan `arrivalDateTime` dikirim sama persis dengan nilai tersimpan |
| Masalah yang diketahui | Riwayat koreksi waktu tiba tidak disimpan — hanya nilai terakhir (`IGD-DEC-152` tidak memintanya) |
| Risiko tersisa | Task ini dan `BE-IGD-057` mengubah berkas yang sama. Perbaikan cacat lama `PUT` mengubah perilaku `PUT` atas kunjungan ber-encounter dari "selalu `400`" menjadi dapat dipakai |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Tiga berkas source berubah (bersama `BE-IGD-053` dan `BE-IGD-057`); laporan ini; status roadmap/traceability. Tanpa stage, commit, atau push |
| Langkah berikutnya | `FE-IGD-040` |
