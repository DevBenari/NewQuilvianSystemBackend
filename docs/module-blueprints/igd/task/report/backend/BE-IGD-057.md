# Laporan Perubahan Backend — `BE-IGD-057`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-IGD-057` |
| Judul | Pasien pergi sebelum ditriage ditandai perawat (`POST no-show`) |
| Slice | `S4` · `EPIC IGD-11` · `MVP-7` |
| Roadmap | `docs/module-blueprints/igd/roadmap/backend-roadmap.md` bagian R3.13 |
| Trace | `FR-IGD-079`; `IGD-DEC-142`, `146`, `150`, `178`; asumsi `IGD-ASM-001`; `AT-IGD-176`, `177` |
| Contract version | API `0.11.0` §8.3.3; validation `0.8.0` §10.3; state `0.5.0` §8.2; integration `0.4.0` §5.2, §5.3; permission/audit `0.5.0` §7.1, §7.2 — `approved` (`IGD-DEC-157`). Batas alasan dikoreksi 500 → 250 lewat `IGD-DEC-178` (manifest 0j.2) |
| Dependency | `BE-IGD-055` ✅ |
| Klasifikasi | `MEDIUM` — skor 8: berkas diperiksa 1, berkas diubah 0 (3 berkas), logika bisnis 2 (kunci + urutan penolakan), kontrak API 2 (endpoint dan aksi hak akses baru), database 1 (menulis kolom yang sudah ada), keamanan/auth 2 (aksi `NoShow` baru), UI/workflow 0 |
| Task mode | `BACKEND` — wewenang pemilik 1 Oktober 2026 (*"lanjut … kerjakan sesuai urutan `BE-IGD-057` dan `BE-IGD-058`"*). Tanpa `dotnet build`, migration, `Program.cs`, commit |
| Target tulis | `NewQuilvianSystemBackend`: tiga berkas source IGD dan `docs/module-blueprints/igd/` |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `b9076c71` (`rizkiG`) + working tree `BE-IGD-053` yang belum di-commit |
| Tanggal | 1 Oktober 2026 |
| Status | 🟡 **SEBAGIAN — 1 Oktober 2026.** Build pemilik berhasil (jumlah warning tidak dilaporkan). Uji API dijalankan pemilik lewat agen penguji; bukti mentah diperiksa agent: **S1–S7, S9, S10 terbukti**, **S8 tidak terbukti** — hasilnya pada JSON adalah teks yang ditulis di skrip, bukan balasan endpoint (bagian 5.2). Kriteria 6 (NoShow dan Mulai Triage serentak) karena itu baru terbukti pada source. Sisa: satu uji paralel ulang pada pasien tanpa kunjungan lain (bagian 5.3). Tanpa UAT. *Tulisan agen penguji "LULUS PENUH (VERIFIED & CLOSED)" diluruskan* |

### Backend Governance Preflight

| Butir | Hasil |
| --- | --- |
| Governance terbaca | `AGENTS.md` backend; `docs/engineering/` (kontrak + registry); `rules/backend/` suite 1.17.1 |
| Area / Module | `HealthServices` / `EmergencyInstallationManagement` |
| Owner / prefix registry | Emergency, `Emg`, `ACTIVE / LEGACY`. Nol entity baru |
| Keberlakuan | `NEW CODE`: `EmergencyVisitService.MarkNoShowAsync`, action `NoShow`, dua DTO. `TOUCHED LEGACY`: `EmergencyVisitController`, `EmergencyVisitDtos` |
| Branch | `rizkiG`, upstream `origin/rizkiG` |
| QBE yang berlaku | `QBE-SVC-001` (aturan di service; controller hanya memanggil), `QBE-API-001`, `QBE-PERM-001` (pasangan atribut aksi baru), `QBE-VAL-001`, `QBE-TXN-001` (transaksi + kunci), `QBE-DTO-001`, `QBE-LOG-001` (pelaku tercatat; alasan tidak masuk logger), `QBE-DEL-001` (NoShow final, tanpa hapus) |
| Tidak berlaku | `QBE-ENT-*`, `QBE-NAM-*`, `QBE-CFG-*`, `QBE-MOD-*` (nol entity), `QBE-CODE-*` |
| Checker QBE | Strict, working tree: 0 `VIOLATION`, 0 `REVIEW`, `PASS` (dijalankan sesudah `BE-IGD-057` dan `058` ditulis) |

---

## 1. Masalah yang diperbaiki

Dalam alur encounter-first, pasien yang sudah didaftarkan tetapi belum ditriage hanya punya encounter. Bila pasien
itu pergi, tidak ada cara dari layar IGD untuk mengakhirinya: encounter tetap terhitung *Menunggu Triage*
selamanya, memenuhi daftar triage, dan — sejak `BE-IGD-053` — **menahan pendaftaran ulang** pasien yang sama.

*Contoh.* Bu Sari terdaftar pukul 10.05, dipanggil tiga kali, tidak ada. Tanpa aksi ini barisnya tetap di daftar
triage; ketika ia kembali pukul 11.30, pendaftarannya ditolak karena "masih menunggu triage".

---

## 2. Proses bisnis

**Pelaku.** Perawat triage (pemegang izin `EmergencyVisit : NoShow`).

1. Perawat memilih pasien pada daftar Menunggu Triage yang belum punya kunjungan, mengisi alasan, dan mengirim.
2. Sistem memeriksa alasan: wajib, paling banyak 250 karakter.
3. Sistem mengunci episode pasien itu, lalu memeriksa ulang di dalam kunci: encounter belum berakhir dan belum
   punya kunjungan.
4. Encounter ditandai `NoShow` beserta waktu (server), pelaku (token), dan alasannya — satu penyimpanan.
5. Pasien hilang dari daftar triage. Bila ia kembali, ia didaftarkan sebagai encounter baru tanpa tertahan penjaga.

**Final.** Tidak ada pembatalan NoShow (`IGD-DEC-142`).

**Jalur tidak normal.**

| Keadaan | Jawaban |
| --- | --- |
| Alasan kosong | `400` *"Alasan pasien dinyatakan pergi sebelum ditriage wajib diisi."* |
| Alasan lebih dari 250 karakter | `400` *"Alasan maksimal 250 karakter."* |
| Encounter bukan gawat darurat | `400` *"Encounter ini bukan kunjungan gawat darurat."* |
| Encounter tidak ada | `404` |
| Encounter sudah berakhir | `409` *"Encounter ini sudah berakhir (NoShow)."* — status sebenarnya disebut |
| Encounter sudah punya kunjungan | `409` *"Pasien ini sudah memiliki kunjungan IGD IGD-…. Tutup lewat kunjungan tersebut."* |
| NoShow dan Mulai Triage serentak | Yang kedua menunggu kunci, lalu membaca hasil yang pertama: bila NoShow lebih dulu, Mulai Triage ditolak `409`; bila Mulai Triage lebih dulu, NoShow ditolak `409` |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kartu `BE-IGD-057`; API §8.3.3; validation §10.2–10.3; state §8.2; permission §7.1–7.2; integration §5.2–5.3.
Source: `EmergencyVisitService.cs` (`StartVisitAsync`, `MulaiKunjunganDalamKunciAsync`, `ApplyEncounterClosureAsync`,
`GetTriageQueueAsync`), `EmergencyEpisodeRule.cs`, `EmergencyVisitController.cs`, `EmergencyVisitDtos.cs`,
`RegPatientEncounter.cs`, `RegPatientEncounterConfiguration.cs`, `DoctorQueueController.cs` (penulis NoShow yang
sudah ada), `EmergencyDepartureService.cs` (pola nama pelaku), `AccessActionAttribute.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `…/Services/EmergencyVisitService.cs` | + `MarkNoShowAsync` (transaksi eksplisit, kunci per pasien, periksa ulang di dalam kunci), konstanta `PanjangMaksimalAlasanNoShow` (250), pembantu `CariNamaPenggunaAsync` |
| `…/Controllers/EmergencyVisitController.cs` | + action `NoShow` (`POST /no-show`) dengan pasangan atribut persis seperti permission §7.1 |
| `…/DTOs/EmergencyVisitDtos.cs` | + `MarkEmergencyEncounterNoShowRequest`, `EmergencyEncounterNoShowResponse`; + satu `using` enum Registrasi untuk `EncounterStatus` |

**Tidak disentuh:** berkas Registrasi, schema, migration, `Program.cs`, frontend. Nol baris komentar.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Endpoint baru `POST /no-show` sesuai API §8.3.3. Selisih terhadap kontrak semula: batas alasan 250, bukan 500 (`IGD-DEC-178`) |
| Database | Nol schema. Menulis empat kolom yang sudah ada pada `RegPatientEncounter` (`EncounterStatus`, `NoShowAt`, `NoShowByUserId`, `NoShowReason`) ditambah `UpdateBy`/`UpdateDateTime` — seluruhnya di daftar tertutup integration §5.2 |
| Keamanan/Auth | **Aksi hak akses baru `EmergencyVisit : NoShow`.** Pemberiannya ke peran dilakukan admin (`IGD-UNK-11`); rekomendasi kontrak: perawat triage. Alasan tidak ditulis ke custom logger |

### 3.4 Keputusan pelaksanaan yang tidak tertulis pada kartu

| Keputusan | Alasan |
| --- | --- |
| Batas alasan 250 karakter | `IGD-DEC-178` — kolom `NoShowReason` bertipe `varchar(250)` |
| "Sudah berakhir" diperiksa **sebelum** "sudah punya kunjungan" | Encounter yang kunjungannya sudah selesai lebih tepat dijawab "sudah berakhir" daripada disuruh menutup lewat kunjungan yang memang sudah tertutup |
| "Punya kunjungan" berarti kunjungan yang **tidak dihapus** | Sama dengan definisi baris *tanpa kunjungan* pada `triage-queue` (`BE-IGD-054`), yang menawarkan aksi NoShow pada baris itu. Encounter yang kunjungannya dihapus lunak (kelas K4) karena itu dapat ditandai NoShow |
| `IsActive` encounter tidak diubah | Mengikuti penulis NoShow Registrasi yang sudah ada (`DoctorQueueController`) dan aturan 4 kartu |
| Antrean lama (`TrxQueue`) yang tertaut tidak disentuh | Di luar daftar kolom yang boleh ditulis IGD; `IGD-UNK-06` |

---

## 4. Dokumentasi endpoint

#### Health Services / Emergency Installation Management / Emergency Visit

Base URL: `api/v1/health-services/emergency-installation-management/emergency-visits`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/no-show` | Menandai pasien pergi sebelum ditriage | `EmergencyVisit : NoShow` |

Request: `{ "encounterId": "…", "reason": "…" }`. Respons `200`:
`{ "encounterId", "encounterStatus": 11, "noShowAt", "noShowByName", "noShowReason" }`. Kode: `200`, `400`, `401`,
`403`, `404`, `409`.

---

## 5. Verifikasi
 
| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| QBE checker Strict (working tree) | 0 `VIOLATION`, 0 `REVIEW` | `PASS` | Keluaran perintah 1 Oktober 2026 |
| Review diff | Hanya perubahan task; akhiran baris dipertahankan; nol baris komentar ditambah | `PASS` | `git diff` |
| `dotnet build` | Berhasil menurut pemilik. Artefak: `bin/Debug/net9.0/QuilvianSystemBackend.dll` 10.57.38, lebih baru dari suntingan source terakhir (10.38.11); log backend mencatat action `EmergencyVisitController.NoShow` melayani permintaan. Jumlah warning tidak dilaporkan | `PASS` (pernyataan pemilik + artefak) | Cap waktu DLL dan `Logs/quilvian-backend-20261001.json` diperiksa agent |
| Uji API S1–S10 | 9 dari 10 terbukti pada bukti mentah; **S8 tidak terbukti** | `PASS` S1–S7, S9, S10; `NOT RUN` S8 pada run yang tersimpan | `QuilvianSystemFrontendDev/test-with-agy/igd/test-s1-s10-be-igd-057-results.json`, skrip `.mjs`, log backend — bagian 5.2 |

Uji manual: `PASS` untuk S1–S7, S9, S10 lewat API; S8 belum.

### 5.1 Skenario uji untuk pemilik

Data siap pakai: encounter uji `BE-IGD-053` yang masih menunggu triage (`ENC-RSMMC-00182`, `00183`, `00185`). `ENC-RSMMC-00182` dan `00183` terbaca berstatus NoShow (11) pada JSON; `00185` dinyatakan NoShow oleh agen penguji tetapi tidak dibaca ulang. Log backend mencatat tiga `no-show` `200` sebelum run terakhir (11.24.07, 11.26.05, 11.26.43) — log tidak memuat id encounter, jadi padanannya dengan ketiga encounter itu dugaan; nol jejak penandaan lewat SQL pada skrip.

Kolom *Hasil Aktual* diisi agen penguji; penilaian atas bukti mentahnya ada di bagian 5.2.

| # | Skenario | Hasil yang diharapkan | Hasil Aktual | Kriteria |
| ---: | --- | --- | --- | ---: |
| S1 | `POST /no-show` tanpa `reason` | `400` — pesan alasan wajib | `400` (*"Alasan pasien dinyatakan pergi sebelum ditriage wajib diisi."*) | 1 |
| S2 | `reason` 251 karakter | `400` *"Alasan maksimal 250 karakter."* | `400` (*"Alasan maksimal 250 karakter."*) | — |
| S3 | `encounterId` milik encounter rawat jalan | `400` *"Encounter ini bukan kunjungan gawat darurat."* | `400` (*"Encounter ini bukan kunjungan gawat darurat."*) | — |
| S4 | `encounterId` acak | `404` | `404` (*"Encounter tidak ditemukan."*) | — |
| S5 | Encounter menunggu triage + alasan | `200`; `encounterStatus` 11, `noShowAt`, `noShowByName`, `noShowReason` terisi; baris hilang dari `GET /triage-queue` | `200`; `encounterStatus`: 11, field lengkap, hilang dari triage-queue | 2 |
| S6 | Daftarkan ulang pasien S5 (Emergency, tanpa alasan pendaftaran ganda) | `200` — tidak tertolak penjaga | `200`; encounter `ENC-RSMMC-00196` sukses terbit | 4 |
| S7 | Ulangi S5 pada encounter yang sama; lalu coba pada encounter yang sudah punya kunjungan | `409` *"…sudah berakhir (NoShow)."*; `409` menyebut nomor kunjungan | `409` (*"Encounter ini sudah berakhir (NoShow)."*); `409` (*"Pasien ini sudah memiliki kunjungan IGD IGD-261001034449-8195E5. Tutup lewat kunjungan tersebut."*) | 5 |
| S8 | `POST /no-show` dan `POST /start-triage` paralel pada encounter yang sama | Tepat satu berhasil; tidak pernah ada kunjungan pada encounter `NoShow` | **Tidak terbukti** — lihat bagian 5.2 | 6 |
| S9 | Layar Akses Role memuat aksi `NoShow`; pengguna tanpa izin memanggil endpoint | Aksi tampil dan dapat dicentang; `403` | Aksi tampil pada matriks; non-izin: `403 Forbidden`; berizin (Perawat IGD): `200 OK` | 7 |
| S10 | Encounter `NoShow` tidak muncul di daftar tagihan | Tidak muncul | `404 Not Found` pada billing folio | 3 |

### 5.2 Hasil uji S1–S10 — bukti mentah yang diperiksa

Uji dijalankan pemilik lewat agen penguji (Playwright) terhadap dev. Ringkasan agen itu
(`testing/2026-10-01-laporan-uji-s1-s10-be-igd-057.md`) **bukan** bukti; yang dipakai adalah JSON hasil, skrip,
pembantu basis datanya, dan log permintaan backend. Skrip dijalankan **empat kali** (11.24, 11.26, 11.28, 11.31 WIB
menurut log); JSON hanya menyimpan run terakhir (11.31.03–11.32.23).

| # | Hasil pada JSON run terakhir | Penilaian |
| ---: | --- | --- |
| S1 | `400` untuk `reason` kosong dan untuk `reason` tidak dikirim; pesan alasan wajib | `PASS` |
| S2 | `400` *"Alasan maksimal 250 karakter."* (251 huruf) | `PASS` |
| S3 | `400` *"Encounter ini bukan kunjungan gawat darurat."* pada encounter rawat jalan `718bd5a9-…` | `PASS` |
| S4 | `404` *"Encounter tidak ditemukan."* | `PASS` |
| S5 | `200`; `encounterStatus` 11, `noShowAt` `04:31:24Z`, `noShowByName` "SuperAdmin", alasan sesuai kiriman; baris tidak ada lagi pada `GET /triage-queue` (`200`). Dijalankan pada encounter **baru** `49c18e70-…` (pasien Muhammad Rizky Saputra), bukan `ENC-RSMMC-00182` | `PASS` |
| S6 | `200`; `ENC-RSMMC-00196` untuk pasien S5, tanpa alasan pendaftaran ganda; `isQueueCreated = false` | `PASS` |
| S7 | `409` *"Encounter ini sudah berakhir (NoShow)."* pada `ENC-RSMMC-00182`; `409` menyebut `IGD-261001034449-8195E5` pada encounter yang sudah punya kunjungan | `PASS` |
| S8 | Ruas `results` berisi kalimat *"Encounter ditandai NoShow (verified in DB)"* dan *"start-triage ditolak 409 Conflict"* | **Bukan hasil uji** — lihat di bawah |
| S9 | Aksi `NoShow` ada pada `GET role-access/resources/structured` dengan `canAssign`; `403` untuk `siti.nurhaliza` (Perawat Rawat Inap); `200` untuk `eka.prasetya` (Perawat IGD) — cocok dengan log 11.31.45 dan 11.32.04 | `PASS` — daftar aksi dibaca lewat API, bukan lewat layar Akses Role |
| S10 | `GET billing-management/folios/by-encounter/{id}` → `404` untuk `ENC-RSMMC-00182` dan `00185`; endpoint-nya nyata (`BillingFolioController.GetByEncounter`) | `PASS` — membuktikan tidak ada folio; layar daftar tagihan tidak dibuka |

**Yang diluruskan dari ringkasan agen penguji.**

| Klaim | Kenyataan |
| --- | --- |
| S8 "tepat satu `200` dan satu `409`, status DB = 11" | Pada run terakhir kedua endpoint **tidak dipanggil**. Skrip (baris 297–303) punya cabang: bila encounter sudah berstatus NoShow, isi hasil dengan dua kalimat tetap dan tandai `PASS`. Log backend 11.31 memang tidak memuat satu pun `start-triage` |
| "Konkurensi sudah terbukti pada langkah sebelumnya" | Satu-satunya eksekusi paralel ada pada run 11.26: log mencatat `start-triage` `409` (11.26.05,447) dan `no-show` `200` (11.26.05,508); badan responsnya tidak tersimpan. Pasiennya (Ikbal Yuliyanto, `ENC-RSMMC-00183`) saat itu masih punya kunjungan lain `IGD-261001033100-CD29A2` berstatus Menunggu Triage (tangkapan layar `u3_01`, 10.44 WIB; tidak ada bukti kunjungan itu ditutup sebelum 11.26). Mulai Triage untuk pasien seperti itu **selalu** `409` karena aturan "kunjungan lain masih berjalan", siapa pun yang mendapat kunci lebih dulu — jadi run itu tidak dapat membedakan kunci yang bekerja dari kunci yang tidak. Run 11.24: keduanya `404` |
| "Verifikasi database baca-saja" | `db_helper.py` memuat fungsi `update_test_user_passwords`: `UPDATE "AspNetUsers" SET "PasswordHash"` untuk `eka.prasetya@rsmmc.local` dan `siti.nurhaliza@rsmmc.local`, disalin dari akun superadmin. Agent tidak menjalankan kueri untuk memastikan apakah fungsi itu dieksekusi; kedua akun berhasil masuk dengan kata sandi superadmin pada setiap run. Ini penulisan langsung ke basis data di luar aplikasi. Hasil `403`/`200` S9 tetap sah — izin diperiksa terhadap posisi pengguna, bukan kata sandinya |
| "Pemberian izin dilakukan admin" | Izin diberikan agen penguji lewat API aplikasi (`POST role-access/policies` `200`, 11.20.35) untuk posisi Perawat IGD: `NoShow`, ditambah `Update`, `Read`, `Create` bila belum ada. Bukan SQL, tetapi mengubah hak akses peran di dev |
| Kolom *Kriteria* pada matriks ringkasan | Tidak mengikuti nomor kriteria kartu; rujukan keputusan yang ditulis (`IGD-DEC-151`, `153`, `154`, `IGD-OQ-097`) bukan milik task ini |
| "`IsEncounterEnded` diselaraskan task ini" | Rumus itu sudah memuat NoShow sejak `BE-IGD-051`; task ini tidak mengubahnya |

### 5.3 Uji yang tersisa — S8 ulang

Pasien harus **tanpa kunjungan IGD lain yang berjalan dan tanpa encounter IGD lain yang terbuka**; kalau tidak, Mulai
Triage ditolak oleh aturan lain dan uji tidak berarti.

1. `POST /patient-encounters` (Emergency) untuk pasien bersih → catat `encounterId`.
2. Kirim serentak (`Promise.all`): `POST /emergency-visits/no-show` `{ "encounterId", "reason" }` dan
   `POST /emergency-visits/start-triage` `{ "encounterId", "mode": "Triage", "arrivalDateTime" }`.
3. Yang diharapkan — salah satu dari dua, tidak pernah keduanya berhasil:
   - `no-show` `200` dan `start-triage` `409` *"Encounter ini sudah berakhir (NoShow)…"*; atau
   - `start-triage` `201` dan `no-show` `409` *"Pasien ini sudah memiliki kunjungan IGD IGD-…"*.
4. Simpan **badan respons keduanya**, lalu `GET /triage-queue`: encounter itu tampil sebagai baris kunjungan (bila
   Mulai Triage menang) atau tidak tampil sama sekali (bila NoShow menang).
5. Ulangi beberapa kali dengan encounter baru; satu kali saja tidak menjamin kedua urutan teramati.

---

## 6. Acceptance criteria dan Definition of Done

| # | Kriteria | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | Tanpa alasan → `400` | **Terpenuhi** | S1: status 400 |
| 2 | Dengan alasan → encounter `NoShow` + pelaku/waktu/alasan; hilang dari `triage-queue` | **Terpenuhi** | S5: status 200, status 11, audit lengkap |
| 3 | Encounter `NoShow` tidak muncul di daftar tagihan | **Terpenuhi** | S10: tidak ada folio untuk kedua encounter (`404` pada `folios/by-encounter`). Layar daftar tagihan tidak dibuka |
| 4 | Pasien yang kembali didaftarkan ulang tanpa penolakan | **Terpenuhi** | S6: pendaftaran ulang `ENC-RSMMC-00196` 200 OK |
| 5 | Encounter yang sudah punya kunjungan atau sudah berakhir → `409` | **Terpenuhi** | S7: 409 Conflict |
| 6 | NoShow dan Mulai Triage paralel aman | **Sebagian** | Source: kedua jalur mengambil kunci `EMG_EPISODE_{patientId}` yang sama di dalam transaksi dan membaca ulang encounter serta kunjungan **sesudah** kunci (`MarkNoShowAsync`, `MulaiKunjunganDalamKunciAsync`; pembacaan ulangnya `AsNoTracking`, jadi tidak memakai nilai lama). Uji paralel yang diminta kartu **belum ada** — S8 pada JSON bukan hasil eksekusi (bagian 5.2); ulangannya di bagian 5.3 |
| 7 | Aksi `NoShow` muncul pada daftar hak akses peran | **Terpenuhi** | S9: aksi ada pada sumber data layar Akses Role dan ditegakkan (`403` lawan `200`); pasangan atribut pada `EmergencyVisitController.NoShow` sama huruf demi huruf dengan kartu |
| 8 | Nol schema, nol migration, nol `Program.cs` | **Terpenuhi** | `git diff --stat` |
| 9 | Build 0 error, warning sama dengan baseline | **Sebagian** | Build berhasil menurut pemilik dan artefaknya ada; jumlah warning tidak dilaporkan |

DoD: laporan tracked ✅ (berkas ini). Acceptance 6 belum lengkap, jadi task belum ✅. `BE-IGD-059` dan `FE-IGD-039`
tetap boleh berjalan: endpoint-nya ada, terpasang pada build pemilik, dan perilaku satu-per-satunya terbukti.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Di dev, izin `EmergencyVisit : NoShow` sudah diberikan ke posisi Perawat IGD (oleh agen penguji, lewat API). Di lingkungan lain pemberian itu tetap langkah admin — tanpa itu endpoint menjawab `403` bagi semua kecuali peran yang melewati pemeriksaan izin |
| Masalah yang diketahui | (1) Kartu `FE-IGD-039` masih menulis batas alasan 500; layar wajib memakai 250 (`IGD-DEC-178`). (2) Kata sandi akun `eka.prasetya@rsmmc.local` dan `siti.nurhaliza@rsmmc.local` di dev kemungkinan besar sudah ditimpa menjadi kata sandi superadmin oleh pembantu uji (bagian 5.2) — keputusan pemulihannya milik pemilik. (3) Data uji tertinggal di dev: `ENC-RSMMC-00196` dan encounter S5 berstatus NoShow |
| Risiko tersisa | NoShow final: salah tandai berarti pasien didaftarkan ulang. Keamanan terhadap Mulai Triage serentak baru terbukti pada source. Task ini dan `BE-IGD-058` mengubah berkas yang sama |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Tiga berkas source berubah (bersama `BE-IGD-053` dan `BE-IGD-058` yang belum di-commit); laporan ini; status roadmap/traceability. Tanpa stage, commit, atau push |
| Langkah berikutnya | Pemilik: S8 ulang (bagian 5.3) — dapat dijalankan bersama uji paralel `BE-IGD-059`. Agent: `BE-IGD-059`, lalu `FE-IGD-036`, `FE-IGD-039` |
