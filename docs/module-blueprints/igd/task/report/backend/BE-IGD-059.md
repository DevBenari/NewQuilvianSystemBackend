# Laporan Perubahan Backend — `BE-IGD-059`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-IGD-059` |
| Judul | Jalur umum Registrasi dibatasi untuk encounter `Emergency` |
| Slice | `S1` · `EPIC IGD-11` · `MVP-7` |
| Roadmap | `docs/module-blueprints/igd/roadmap/backend-roadmap.md` bagian R3.13 |
| Trace | `FR-IGD-082`; `IGD-DEC-153`, `146`, `135`; `AT-IGD-180` |
| Contract version | API `0.11.0` §8.1 nomor 3 dan 4, §8.2 baris `PATCH`; validation `0.8.0` §10.1 aturan 8–9; state `0.5.0` §8.2; integration `0.4.0` §5.2 baris kedua, §5.3 baris `cancel` — `approved` (`IGD-DEC-157`). Berkas kontrak kini API `0.13.0` / validation `0.10.0`; hash kelima berkas cocok dengan manifest bagian 0j, 0j.1, dan 0j.2 saat task dimulai |
| Dependency | `BE-IGD-051` ✅; `BE-IGD-057` 🟡 — endpoint NoShow ada dan terpasang pada build pemilik, tersisa satu uji paralel; pemilik mengizinkan task ini berjalan |
| Klasifikasi | `MEDIUM` — skor 6: repository 0, berkas diperiksa 1, berkas diubah 0 (1 berkas), logika bisnis 1 (kunci + periksa ulang), kontrak API 2 (`409` baru pada dua endpoint), database 1 (kolom yang sudah ada), keamanan/auth 1, UI/workflow 0 |
| Task mode | `BACKEND` — wewenang pemilik 1 Oktober 2026 (*"Lanjut sesuai urutan: `BE-IGD-059` → `FE-IGD-036` → …"*). Tanpa `dotnet build`, migration, `Program.cs`, eksekusi database, commit |
| Target tulis | `NewQuilvianSystemBackend`: satu berkas Registrasi di bawah `IGD-DEC-135`, dan `docs/module-blueprints/igd/` |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `b9076c71` (`rizkiG`, upstream `origin/rizkiG`) + working tree `BE-IGD-053`, `057`, `058` yang belum di-commit |
| Tanggal | 1 Oktober 2026 |
| Status | ✅ **SELESAI — 2 Oktober 2026.** Build pemilik (DLL 1 Oktober 2026 15.18) dan uji API S1–S8 **8 dari 8** pada bukti mentah, termasuk S6 serentak dengan kedua urutan teramati. Tanpa UAT. *Sebelumnya:* 🟡 **SEBAGIAN — Implementation Complete.** Source selesai (satu berkas); QBE checker `PASS`. **Belum:** build pemilik dan uji API S1–S8 (bagian 5.1) |

### Backend Governance Preflight

| Butir | Hasil |
| --- | --- |
| Governance terbaca | `AGENTS.md` backend; `rules/backend/` suite skill 1.17.1 (`TASK_RULES`, `TASK_CLASSIFICATION`, `API_RULES`, `REVIEW_RULES`, `REPORT_TEMPLATE`, kontrak rekayasa, registry) |
| Area / Module | `HealthServices` / `RegistrationManagement` — titik sentuh di bawah izin remediasi teknis `IGD-DEC-135`; aturan yang ditegakkan milik `EmergencyInstallationManagement` |
| Owner / prefix registry | Registration `Reg`, Emergency `Emg` — keduanya `ACTIVE / LEGACY`. Nol entity baru |
| Keberlakuan | `TOUCHED LEGACY`: `PatientEncounterController.UpdateEncounterStatus` dan `CancelEncounter` |
| Branch | `rizkiG`, upstream `origin/rizkiG` |
| QBE yang berlaku | `QBE-API-001` (envelope dan kode status yang sudah ada), `QBE-PERM-001` (metadata akses tidak berubah), `QBE-VAL-001`, `QBE-TXN-001` (kunci + pembatalan dalam satu transaksi), `QBE-DEL-001` (isi pembatalan dan pelakunya tidak berubah) |
| Tidak berlaku | `QBE-ENT-*`, `QBE-NAM-*`, `QBE-CFG-*`, `QBE-MOD-*` (nol entity), `QBE-CODE-*`, `QBE-SVC-001` (controller lama yang memang mengakses context langsung; legacy ratchet — tidak ditulis ulang) |
| Checker QBE | `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict`, working tree: 12 berkas dinilai, 0 `VIOLATION`, 0 `REVIEW`, `PASS` |

---

## 1. Masalah yang diperbaiki

Layar dan pemanggil Registrasi punya dua endpoint umum yang berlaku untuk semua jenis encounter: ubah status dan
batalkan. Untuk encounter IGD keduanya berbahaya, karena tidak tahu apa-apa tentang kunjungan IGD.

| Celah | Akibat |
| --- | --- |
| `PATCH …/status` dapat menandai encounter IGD `Completed`, `Cancelled`, atau `NoShow` | Kunjungan IGD-nya masih berjalan, tetapi encounter-nya sudah berakhir; atau NoShow tercatat tanpa pelaku dan tanpa alasan |
| `PATCH …/cancel` dapat membatalkan encounter IGD yang sudah punya kunjungan | Pasien masih ditangani di IGD, encounter-nya batal |
| Pembatalan dan Mulai Triage dapat berjalan serentak | Kunjungan lahir pada encounter yang baru saja dibatalkan |

*Contoh.* Pasien sedang ditangani dokter IGD pada kunjungan `IGD-0012`. Petugas loket, yang mengira pasien salah
daftar, membatalkan encounter-nya dari layar Registrasi. Sebelum task ini pembatalan itu berhasil, dan kunjungan
`IGD-0012` tertinggal berjalan pada encounter yang sudah batal.

---

## 2. Proses bisnis

**Tujuan.** Status akhir encounter IGD hanya ditetapkan lewat aksi IGD; satu-satunya yang tetap boleh dari
Registrasi adalah membatalkan pendaftaran yang belum melahirkan kunjungan.

**Pelaku.** Petugas pendaftaran (pemegang `PatientEncounter : Update`).

**Ubah status (`PATCH …/status`).**

1. Nilai status diperiksa seperti semula (`400` bila bukan nilai yang dikenal).
2. Encounter dicari (`404` bila tidak ada).
3. **Bila encounter bertipe Emergency → ditolak `409`**, dengan kalimat yang menunjukkan jalan penggantinya.
4. Tipe lain berjalan persis seperti sebelumnya.

**Batalkan (`PATCH …/cancel`) untuk encounter Emergency.**

1. Encounter dicari (`404` bila tidak ada).
2. Transaksi dibuka dan **kunci per pasien** diambil — kunci yang sama dengan pendaftaran, Mulai Triage, dan NoShow.
3. Encounter **dibaca ulang** di dalam kunci.
4. Bila encounter sudah selesai → `400` seperti semula.
5. Bila encounter sudah punya kunjungan IGD → **ditolak `409`**, menyebut nomor kunjungannya.
6. Selain itu pembatalan berjalan dengan isi yang sama seperti hari ini, lalu transaksi disimpan.

Untuk tipe selain Emergency tidak ada transaksi, kunci, maupun pemeriksaan tambahan.

**Jalur tidak normal.**

| Keadaan | Jawaban |
| --- | --- |
| Ubah status encounter IGD, dari rute biasa maupun rute `admin` | `409` *"Status kunjungan gawat darurat tidak dapat diubah dari sini. Tutup lewat layar IGD: tandai pasien pergi sebelum ditriage, atau selesaikan/batalkan kunjungan IGD-nya."* |
| Batalkan encounter IGD yang sudah punya kunjungan | `409` *"Encounter ini sudah memiliki kunjungan IGD IGD-…. Batalkan lewat kunjungan IGD tersebut; encounter akan ikut dibatalkan."* |
| Batalkan encounter IGD yang salah daftar, belum ditriage | `200` — batal seperti biasa; pasien hilang dari daftar Menunggu Triage dan dapat didaftarkan ulang |
| Pembatalan dan Mulai Triage serentak | Yang kedua menunggu kunci, lalu membaca hasil yang pertama: bila pembatalan lebih dulu, Mulai Triage ditolak `409` (*"Encounter ini sudah berakhir…"*); bila Mulai Triage lebih dulu, pembatalan ditolak `409` |
| Batalkan encounter IGD yang sudah selesai | `400` *"Kunjungan yang sudah selesai tidak dapat dibatalkan."* — tidak berubah |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kartu `BE-IGD-059`; API §8.1, §8.2; validation §10.1; state §8.2; integration §5.2, §5.3.
Source: `PatientEncounterController.cs` (`UpdateEncounterStatus`, `CancelEncounter`, `DeleteEncounter`,
`CancelQueuesByEncounterAsync`, `CreateEncounterCoreAsync`), `PatientEncounterDtos.cs`, `RegPatientEncounter.cs`,
`EmergencyEpisodeRule.cs`, `EmergencyVisitService.cs` (`MulaiKunjunganDalamKunciAsync`, `MarkNoShowAsync`), `EmgVisit.cs`.
Frontend (baca saja): seluruh service yang menyebut `patient-encounters` — **nol** layar memanggil `…/status`
maupun `…/cancel`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/RegistrationManagement/Controllers/PatientEncounterController.cs` — **berkas Registrasi** | `UpdateEncounterStatus`: penolakan `409` untuk Emergency sesudah pemeriksaan `404`. `CancelEncounter`: khusus Emergency — transaksi, kunci per pasien, baca ulang, lalu penolakan `409` bila sudah punya kunjungan; transaksi disimpan sesudah `SaveChanges`. + konstanta pesan aturan 8, + `ProducesResponseType` `409` pada kedua action, + satu `using` model IGD. Tambahan 48 baris; nol baris dihapus |

Berkas yang sama juga memuat perubahan `BE-IGD-053` yang belum di-commit (penjaga pada pembuatan encounter); kedua
perubahan tidak bersinggungan. **Nol baris komentar ditambah** — jumlah baris komentar berkas tetap 59; akhiran
baris tetap CRLF.

**Tidak disentuh:** berkas IGD, DTO, schema, migration, `Program.cs`, berkas kontrak, frontend.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Sesuai kontrak terkunci (API §8.1 nomor 3 dan 4 — perubahan yang **memutus**): `409` baru pada `PATCH …/status` dan `PATCH …/cancel` untuk encounter Emergency. Nol route baru; request dan response tidak berubah |
| Database | Nol schema, nol migration. Pembatalan menulis kolom yang sama seperti sebelumnya |
| Keamanan/Auth | Metadata akses tidak berubah (`PatientEncounter : Update`). Jalur yang memungkinkan NoShow tanpa pelaku dan alasan pada encounter IGD ditutup |

### 3.4 Keputusan pelaksanaan yang tidak tertulis pada kartu

| Keputusan | Alasan |
| --- | --- |
| "Punya kunjungan" berarti kunjungan yang **tidak dihapus** | Sama dengan `BE-IGD-057` dan dengan definisi baris *tanpa kunjungan* pada `triage-queue`. Encounter kelas K4 (kunjungannya dihapus lunak) karena itu tetap dapat dibatalkan |
| Kunjungan yang sudah `Cancelled` atau `Completed` tetap terhitung "punya kunjungan" | Aturan 9 tidak membedakan status kunjungan. Encounter seperti itu semestinya sudah berakhir bersama kunjungannya (`BE-IGD-051`); penolakan mencegah tanda pembatalannya ditimpa |
| `400` "sudah selesai" diperiksa **sebelum** `409` "punya kunjungan" | Encounter yang selesai bersama kunjungannya lebih tepat dijawab "sudah selesai" daripada disuruh membatalkan kunjungan yang sudah tertutup. Urutan ini juga membuat pesan lama tetap sama |
| Encounter dibaca ulang sesudah kunci diambil | Nilai yang dibaca sebelum kunci bisa sudah usang bila permintaan lain baru saja selesai |
| Transaksi dan kunci hanya untuk Emergency | Acceptance 5: tipe lain tidak berubah sedikit pun |
| Pemeriksaan kunjungan ditulis di controller Registrasi, bukan di `EmergencyEpisodeRule` | Kartu membatasi berkas yang disentuh pada controller ini dan menyebut berkas IGD hanya *dipakai* |

---

## 4. Dokumentasi endpoint

#### Health Services / Registration Management / Patient Encounter

Base URL: `api/v1/health-services/registration-management/patient-encounters` — milik Registration Management

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `PATCH` | `/{id}/status`, `/admin/{id}/status` | Mengubah status encounter — kini **menolak** encounter Emergency | `PatientEncounter : Update` (tidak berubah) |
| `PATCH` | `/{id}/cancel`, `/admin/{id}/cancel` | Membatalkan encounter — kini **menolak** encounter Emergency yang sudah punya kunjungan | `PatientEncounter : Update` (tidak berubah) |

Kode status baru pada keduanya: `409`. Request `status`: `{ "encounterStatus": <angka>, "reason": "…" }`. Request
`cancel`: `{ "cancelReason": "…" }` (wajib, maksimal 250).

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| QBE checker Strict (working tree) | 12 berkas, 0 `VIOLATION`, 0 `REVIEW`, `Final result: PASS` | `PASS` | Keluaran perintah 1 Oktober 2026 |
| Review diff | Hanya perubahan task pada dua action; akhiran baris CRLF dipertahankan (2871 baris, nol LF); jumlah baris komentar tetap 59 | `PASS` | `git diff` berkas controller |
| Tabrakan nama akibat `using` model IGD | Seluruh tipe pada `EmergencyInstallationManagement/Models` berawalan `Emg` | `PASS` | Pencarian deklarasi tipe |
| `dotnet build` | — | `NOT RUN` | Milik pemilik |
| Uji API S1–S8 | — | `NOT RUN` | Menunggu build pemilik |

Uji manual: `REQUIRED`.

**Tidak dijalankan:** `dotnet build` (arahan pemilik), uji API (butuh build), eksekusi database.

### 5.1 Skenario uji untuk pemilik

Build lebih dulu: `dotnet build ./QuilvianSystemBackend.sln -p:RunAnalyzers=false`. Simpan **badan respons** tiap
langkah.

| # | Skenario | Hasil yang diharapkan | Kriteria |
| ---: | --- | --- | ---: |
| S1 | `PATCH /patient-encounters/{id}/status` `{ "encounterStatus": 9 }` (`Completed`) pada encounter Emergency yang masih menunggu triage | `409` dengan kalimat aturan 8; `GET` encounter sesudahnya: status tidak berubah | 1 |
| S2 | Sama dengan S1 lewat `/admin/{id}/status` | `409` kalimat yang sama | 1 |
| S3 | `PATCH /patient-encounters/{id}/cancel` `{ "cancelReason": "uji" }` pada encounter Emergency yang **sudah punya kunjungan** berjalan | `409` *"Encounter ini sudah memiliki kunjungan IGD IGD-…. Batalkan lewat kunjungan IGD tersebut; encounter akan ikut dibatalkan."*; encounter tidak berubah | 2 |
| S4 | `PATCH …/cancel` pada encounter Emergency **tanpa** kunjungan | `200` *"Patient encounter berhasil dibatalkan."*; `GET` encounter: `isCancel` benar, `cancelledAt`, `cancelReason`, pelaku terisi, `isActive` salah; baris hilang dari `GET /emergency-visits/triage-queue` | 3 |
| S5 | Daftarkan ulang pasien S4 (Emergency, tanpa alasan pendaftaran ganda) | `200` — tidak tertolak penjaga | 3 |
| S6 | Pasien **tanpa episode IGD lain**: daftarkan (Emergency), lalu kirim serentak `PATCH …/cancel` dan `POST /emergency-visits/start-triage` (`mode: "Triage"`, `arrivalDateTime`) pada encounter itu. Ulangi beberapa kali dengan encounter baru | Tiap ulangan tepat satu berhasil: `cancel` `200` + `start-triage` `409` *"Encounter ini sudah berakhir…"*, **atau** `start-triage` `201` + `cancel` `409` menyebut nomor kunjungan. Tidak pernah keduanya berhasil | 4 |
| S7 | `PATCH …/status` dan `PATCH …/cancel` pada encounter **rawat jalan** | Keduanya `200` seperti sebelumnya; antrean rawat jalan ikut dibatalkan pada `cancel` | 5 |
| S8 | `PATCH …/cancel` pada encounter Emergency yang kunjungannya sudah **selesai** | `400` *"Kunjungan yang sudah selesai tidak dapat dibatalkan."* | — |

S6 dapat dijalankan bersama ulangan S8 `BE-IGD-057` (NoShow lawan Mulai Triage) — keduanya butuh pasien bersih.

---

## 6. Acceptance criteria dan Definition of Done

| # | Kriteria | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | `PATCH …/status` pada encounter Emergency → `409` dengan pesan aturan 8 | **Terpenuhi pada source** | `UpdateEncounterStatus`; kalimat sama huruf demi huruf dengan validation §10.1 aturan 8. Uji API S1–S2 belum |
| 2 | `PATCH …/cancel` pada encounter Emergency yang sudah punya kunjungan → `409` dengan pesan aturan 9 | **Terpenuhi pada source** | `CancelEncounter`. Uji API S3 belum |
| 3 | Pembatalan encounter Emergency tanpa kunjungan tetap berhasil, isi pembatalannya sama dengan hari ini | **Terpenuhi pada source** | Blok penulisan pembatalan tidak disentuh (`git diff`). Uji API S4–S5 belum |
| 4 | Pembatalan dan Mulai Triage paralel → tidak pernah ada kunjungan pada encounter yang dibatalkan | **Terpenuhi pada source** | Kedua jalur mengambil kunci `EMG_EPISODE_{patientId}` di dalam transaksi dan membaca ulang sesudah kunci. Uji paralel S6 belum |
| 5 | Encounter rawat jalan/rawat inap: kedua endpoint tidak berubah | **Terpenuhi pada source** | Seluruh tambahan berada di balik pemeriksaan tipe Emergency. Uji API S7 belum |
| 6 | Nol schema, nol migration, nol `Program.cs` | **Terpenuhi** | `git status`: satu berkas source berubah oleh task ini |
| 7 | Build 0 error, warning sama dengan baseline | **Belum** | Build milik pemilik |

DoD: laporan tracked ✅ (berkas ini). Belum ✅ karena acceptance 1–5 belum diuji dan acceptance 7 belum ada.
`FE-IGD-036` boleh **dikerjakan**; perilisannya menunggu task ini aktif (urutan rilis R3.13.5).

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Perubahan ini **memutus** bagi pemanggil yang selama ini mengubah status atau membatalkan encounter IGD ber-kunjungan lewat Registrasi. Pada frontend saat ini nol layar memanggil kedua endpoint |
| Masalah yang diketahui | (1) `DELETE /patient-encounters/{id}` tetap dapat menghapus encounter IGD yang punya kunjungan — di luar kartu ini. (2) Karena nol layar memanggil `…/cancel`, pembatalan encounter IGD salah daftar (termasuk enam encounter K3 di dev) hari ini hanya dapat dilakukan lewat API; aksi layar yang tersedia sesudah `FE-IGD-039` adalah *Pergi sebelum ditriage* |
| Risiko tersisa | Belum ada build, jadi galat kompilasi belum tersingkir. Controller ini membaca tabel kunjungan IGD secara langsung — bila kelak definisi "punya kunjungan" berubah, ada tiga tempat yang harus sepakat (`triage-queue`, NoShow, pembatalan) |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Satu berkas source berubah oleh task ini (bersama perubahan `BE-IGD-053` di berkas yang sama); laporan ini; status roadmap/traceability. Tanpa stage, commit, atau push |
| Langkah berikutnya | Pemilik: build, lalu S1–S8. Agent: `FE-IGD-036` |

---

## Pemeriksaan bukti uji gabungan — 2 Oktober 2026

Bukti mentah di `QuilvianSystemFrontendDev/test-with-agy/igd/` (`results-tahap-1.json`…`results-tahap-4.json`, skrip `test-tahap-*.mjs`, tangkapan layar `<ID>.png`) dan log backend `Logs/quilvian-backend-20261001.json`, `quilvian-backend-20261002.json`. Ringkasan agen penguji ([laporan uji gabungan](../../../testing/2026-10-01-laporan-uji-gabungan-r313-r314.md)) **tidak** dipakai sebagai bukti: uraian skenarionya pada beberapa task tidak sama dengan panduan, dan daftar `FAIL`-nya tidak cocok dengan JSON mentah.

Build pemilik terbukti dari artefak: `bin/Debug/net9.0/QuilvianSystemBackend.dll` bertanggal 1 Oktober 2026 15.18, sesudah edit source terakhir (14.54); jumlah warning tidak dilaporkan. Source di-commit pemilik sebagai `74a72399`.

| Skenario | Putusan | Yang teramati pada bukti mentah |
| --- | --- | --- |
| `059-S1` | **Terbukti** | `409` dengan kalimat aturan 8; `GET` encounter: `encounterStatus` tetap 1 |
| `059-S2` | **Terbukti** | `409` kalimat yang sama lewat `/admin/{id}/status` |
| `059-S3` | **Terbukti** | `409` *"Encounter ini sudah memiliki kunjungan IGD IGD-…"*; encounter tidak berubah |
| `059-S4` | **Terbukti** | `200`; `cancelledAt` dan `cancelReason` terisi, `isActive` salah |
| `059-S5` | **Terbukti** | Pendaftaran ulang `200`, encounter baru terbit |
| `059-S6` | **Terbukti** | Tiga putaran serentak: dua kali Mulai Triage menang (`201` + batal `409`), satu kali batal menang (`200` + `start-triage` `409` *"…sudah berakhir (Cancelled)…"*) |
| `059-S7` | **Terbukti** | Encounter rawat jalan: `PATCH status` `200`, `PATCH cancel` `200` |
| `059-S8` | **Terbukti** | `400` *"Kunjungan yang sudah selesai tidak dapat dibatalkan."* |

**Catatan.**

- `059-S4`: respons `GET` tidak memuat ruas `isCancel` maupun nama pelaku, jadi keduanya tidak teramati; hilangnya baris dari antrean hanya tercatat sebagai penanda pada ringkasan skrip.
- `059-S7`: pembatalan antrean rawat jalan tidak diperiksa oleh skrip.

Putusan: **✅ selesai** — Build pemilik (DLL 1 Oktober 2026 15.18) dan uji API S1–S8 **8 dari 8** pada bukti mentah, termasuk S6 serentak dengan kedua urutan teramati. Tanpa UAT.
