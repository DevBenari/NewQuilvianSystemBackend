# Laporan Perubahan Backend — `BE-IGD-056`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-IGD-056` |
| Judul | Dokter layak IGD, validasi ulang saat penetapan, dan override beralasan |
| Slice | `S7` · `EPIC IGD-12` · `MVP-10` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) — kartu di bagian R3.13, konteks perencanaan R3.17 |
| Trace | `FR-IGD-106`, `107`, `108`; `AT-IGD-214`…`220`; `IGD-DEC-141`, `237`, `238`, `239`, `240`; fakta `IGD-FACT-088` |
| Contract version | API **`0.16.0`** §11; validation **`0.15.0`** §13 aturan 1–12; permission/audit **`0.9.0`** §10; integration **`0.7.0`** §8; kamus data §9 — seluruhnya `approved` (`IGD-DEC-240`, 11 Oktober 2026) |
| Dependency | `BE-IGD-045` ✅; `IGD-OQ-102`, `IGD-OQ-103` dijawab (`IGD-DEC-237`…`239`) |
| Klasifikasi | `MEDIUM` — skor 8: repository 0, berkas diperiksa 1, berkas diubah 1, logika bisnis 1, kontrak API 2, database 2, keamanan 1, UI 0 |
| Task mode | `BACKEND` — izin pemilik 11 Oktober 2026: *"yaa lanjutkan saya beri izin dan anda kerjakan"* |
| Target tulis | `NewQuilvianSystemBackend` branch `rizkiG`: lima berkas pada §3.2 dan dokumen blueprint IGD (laporan ini, roadmap, traceability) |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `c7e523fd` (sejajar `origin/rizkiG`) |
| Tanggal | 11 Oktober 2026 |
| Status | ✅ **SELESAI — 11 Oktober 2026 (malam): 9 dari 9 acceptance terbukti** (`build-module-backend`): 3, 4, 5, 6, 9 terbukti penuh; 1 dan 2 dengan delta `IGD-DEC-258`; 7 dengan pengecualian uji `Up→Down→Up` (`IGD-DEC-260`); 8 dengan delta `IGD-DEC-259`. Uji ulang R-01…R-07 lulus pada bukti mentah dan log backend (§9); penyimpangan penguji disahkan (`IGD-DEC-255`, `262`). Build pemilik 0 error, 0 warning (laporan pemilik, `IGD-FACT-108`). Butir DoD rilis bersama `FE-IGD-037` dan `IGD-UNK-18` per lingkungan **dilepas dari DoD task atas keputusan pemilik 11 Oktober 2026** (`IGD-DEC-264`) dan tetap syarat rilis R3.17.3. *Sebelumnya, hari yang sama:* 🟡 **SEBAGIAN — 3 dari 9 acceptance terbukti** (1 dengan delta `IGD-DEC-258`; 3; 8 dengan delta `IGD-DEC-259`) sesudah putaran Antigravity 11 Oktober 2026 (§8) dan putusan pemilik. **Belum:** 2 (`onlyOnDuty=true`, `search`), 4 (aturan 11 dan urutan terhadap `409`), 5 (pemangkasan alasan di server), 6 (`GET /active`, baris sebelum migration) — keempatnya di [panduan uji ulang](../../../testing/2026-10-11-panduan-uji-ulang-dpjp-dokter-jaga.md); 7 (siklus `Up→Down→Up`, keputusan pemilik); 9 (jumlah warning build). *Sebelumnya, hari yang sama:* 2 dari 9 sesudah putaran; lalu implementasi selesai, 1 dari 9; migration `20261011052215` diterapkan ke dev oleh pemilik |

---

## 1. Masalah yang diperbaiki

Sebelum task ini, pemilih Dokter Penanggung Jawab (DPJP) IGD memakai daftar dokter jaga yang dibuat pemilik pada
8 Oktober 2026 (`f39250df`). Daftar itu sudah membaca jadwal dokter, tetapi masih punya empat celah terhadap `IGD-DEC-141`:

1. Unit IGD juga dikenali dari **nama** ("IGD", "Gawat Darurat"), sehingga unit rawat jalan bernama "Poli IGD Anak" ikut
   terhitung bila tipenya salah isi.
2. Dokter lain hanya muncul bila **tidak ada satu pun** dokter berjadwal. Selama ada dokter jaga, perawat tidak bisa
   memilih dokter lain walau dokter jaga sedang resusitasi.
3. Dokter cadangan diberi label *"(Dokter Jaga IGD)"* walau tidak sedang jaga.
4. Penetapan dan pengalihan **tidak memeriksa apa pun** selain dokter aktif. Dokter spesialis poli mana pun dapat
   ditetapkan lewat API, tanpa jejak.

*Contoh:* pukul 10.00 dr. Ranger dan dr. Sari, dokter jaga pagi, sedang menangani resusitasi. Perawat Dimas ingin
menetapkan dr. Andi (Umum) yang kebetulan ada di IGD — sebelum task ini nama dr. Andi tidak ada di pilihan. Sebaliknya,
lewat Postman siapa pun bisa menetapkan dr. Citra, Sp.JP tanpa alasan.

---

## 2. Proses bisnis

| Butir | Isi |
| --- | --- |
| Tujuan | DPJP IGD selalu dokter jaga, kecuali ada alasan tertulis yang tercatat |
| Pelaku | Perawat triage / perawat IGD (menetapkan dan mengalihkan); backend IGD (memeriksa) |
| Pemicu | Penetapan DPJP pertama sesudah triage tersimpan, atau pengalihan DPJP |
| Prasyarat | Kunjungan IGD ada; dokter aktif; jadwal dokter di unit bertipe `Emergency` (bila ada) |

**Langkah.**

1. Layar meminta `GET …/on-duty-doctors`. Backend menghitung waktu operasional Asia/Jakarta, lalu menyusun dua kelompok:
   dokter jaga (jadwal aktif yang berlaku pada jam itu di unit bertipe `Emergency`), lalu dokter umum/emergency yang
   tidak sedang jaga — atau, bila kelompok itu kosong, semua dokter aktif yang tidak jaga.
2. Perawat memilih dokter dan menyimpan. Bila dokter berasal dari kelompok kedua, perawat mengisi
   `rosterOverrideReason`.
3. Backend menghitung ulang kelayakan pada waktu penugasan (`effectiveFrom`, kosong = waktu server) memakai perhitungan
   yang **sama** dengan langkah 1.
4. Hasilnya:
   - dokter jaga → tersimpan, `IsRosterOverride = false`; alasan override yang terkirim diabaikan;
   - dokter kelompok kedua dengan alasan sah → tersimpan, `IsRosterOverride = true`, alasan disimpan sesudah spasi di
     ujung dibuang;
   - dokter kelompok kedua tanpa alasan, atau alasan lebih dari 500 karakter → `400`;
   - dokter di luar kedua kelompok → `400`.

**Contoh berangka.** Selasa 13 Oktober 2026 pukul 02.00 WIB, dr. Ranger berjadwal mingguan Senin 21.00–07.00
(`IsOvernight`) di unit "Instalasi Gawat Darurat" bertipe `Emergency`. Shift lintas tengah malam dihitung dari hari
mulainya (Senin), jadi pukul 02.00 Selasa ia **jaga**; pukul 07.30 Selasa tidak lagi. Pada pukul 02.00 daftar berisi
dr. Ranger (`isOnDuty=true`), lalu dr. Andi — Umum (`isOnDuty=false`, label *"(di luar jadwal jaga)"*).

**Jalur tidak normal.**

| Keadaan | Hasil |
| --- | --- |
| Roster kosong (unit IGD belum bertipe `Emergency` atau tanpa jadwal) | Kelompok pertama kosong; semua penetapan menjadi override beralasan. Layanan tidak terhenti (`IGD-UNK-18`) |
| Roster berubah antara daftar dibuka dan simpan | Simpan memakai keadaan saat simpan; `400` tampil, perawat memilih ulang |
| Pencarian `search` mengetik nama spesialis saat dokter umum di luar jadwal ada | Spesialis **tidak** muncul — kekosongan kelompok dinilai sebelum `search`, sehingga daftar dan pemeriksaan simpan tidak pernah berbeda pendapat |
| DPJP yang sedang berjalan berhenti jaga | Tidak diubah; kelayakan hanya diperiksa saat menetapkan atau mengalihkan |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md`; `rules/backend/` (`TASK_RULES`, `TEST_POLICY`, `API_RULES`, `DATABASE_RULES`, `REVIEW_RULES`,
`REPORT_TEMPLATE`, `TASK_CLASSIFICATION`, `engineering/BACKEND_ENGINEERING_CONTRACT`,
`engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY`); kartu `BE-IGD-056`; kontrak API §11, validation §13, kamus data §9,
`02-backend-architecture.md` §16; `EmergencyDoctorAssignmentService.cs`, `EmergencyDoctorAssignmentController.cs`,
`EmergencyDoctorAssignmentDtos.cs`, `EmgDoctorAssignment.cs`, `EmgDoctorAssignmentConfiguration.cs`; `ServiceUnitType.cs`;
`MstDoctor.cs`; migration `20260930064430_AddEmergencyVisitClosureSource.cs` (pola `Down()` berpenjaga); pencarian
pemanggil `TetapkanAsync`/`AlihkanAsync` dan penulis `EmgDoctorAssignment` lain (nol di luar service ini).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyDoctorAssignmentService.cs` | Kueri jadwal dipindah ke `KueriJadwalIgdBerlaku` — pencocokan nama/kode unit dibuang, unit IGD hanya dari `ServiceUnitType.Emergency` (unit jadwal atau unit induk klinik); `KueriKelompokLuarJadwalAsync` (dokter umum/emergency tidak jaga, atau semua dokter tidak jaga bila kosong); `PeriksaKelayakanAsync` dipanggil `TetapkanAsync` dan `AlihkanAsync` sesudah pemeriksaan waktu; kelompok kedua **selalu** dikirim kecuali `onlyOnDuty=true`, berlabel *"(di luar jadwal jaga)"*, `SessionName` kosong; proyeksi riwayat membawa dua ruas baru |
| `Areas/HealthServices/EmergencyInstallationManagement/Models/EmgDoctorAssignment.cs` | `IsRosterOverride` (`bool`), `RosterOverrideReason` (`string?`, `[MaxLength(500)]`) |
| `Repositories/Configurations/HealthServices/EmergencyInstallationManagement/EmgDoctorAssignmentConfiguration.cs` | `RosterOverrideReason` `HasMaxLength(500)` |
| `Areas/HealthServices/EmergencyInstallationManagement/DTOs/EmergencyDoctorAssignmentDtos.cs` | `RosterOverrideReason` pada kedua request (tanpa `[MaxLength]`, supaya pesan kontrak aturan 10 yang tampil, bukan `ProblemDetails`); `IsRosterOverride`, `RosterOverrideReason` pada response riwayat |
| `Areas/HealthServices/EmergencyInstallationManagement/Controllers/EmergencyDoctorAssignmentController.cs` | Alias `[HttpGet("options")]` dihapus (nol pemakai di frontend, diperiksa `grep`) |

`git diff --stat`: 5 berkas, +172/−95. Nol `Program.cs`, nol service baru, nol migration (lihat §3.3).

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Sesuai API §11.1: daftar dokter berubah perilaku (kelompok kedua selalu dikirim); alias `options` dihapus; `POST /` dan `POST /{id}/handover` **memutus perilaku** — dokter di luar jadwal tanpa alasan kini `400`; riwayat bertambah dua ruas (aditif). Rilis wajib bersama `FE-IGD-037` |
| Database | Dua kolom baru pada `EmgDoctorAssignment`. **Migration belum dibuat** — milik Rizki (kartu, Cakupan 5). Sebelum migration diterapkan, aplikasi akan gagal saat membaca tabel ini karena kolomnya belum ada di basis data |
| Keamanan/Auth | Nol butir hak akses baru; `[AccessAction]`/`[AccessPermission]` endpoint tidak berubah (`EmergencyDoctorAssignment : Read/Create/Update`). Pelaku tetap dari token. Alasan override tidak dikirim ke log kustom |

---

## 4. Dokumentasi endpoint

#### Health Services / Emergency Installation Management / Emergency Doctor Assignment

Base URL: `api/v1/health-services/emergency-installation-management/emergency-doctor-assignments`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/on-duty-doctors` | Dokter jaga lalu dokter di luar jadwal pada `at`; query `at`, `search`, `onlyOnDuty` | `EmergencyDoctorAssignment : Read` |
| `POST` | `/` | Penetapan pertama; kelayakan diperiksa; ruas baru `rosterOverrideReason` | `EmergencyDoctorAssignment : Create` |
| `POST` | `/{id}/handover` | Pengalihan; kelayakan diperiksa; ruas baru `rosterOverrideReason` | `EmergencyDoctorAssignment : Update` |
| `GET` | `/`, `/active` | Riwayat dan DPJP aktif; ruas baru `isRosterOverride`, `rosterOverrideReason` | `EmergencyDoctorAssignment : Read` |
| ~~`GET`~~ | ~~`/options`~~ | Dihapus | — |

Pesan penolakan baru (validation §13.3, ditulis persis di source):

| Aturan | Kode | Pesan |
| ---: | :-: | --- |
| 8 | `400` | *"Dokter ini tidak dapat dipilih sebagai dokter penanggung jawab IGD. Pilih dokter jaga IGD, atau dokter umum/emergency dengan alasan."* |
| 9 | `400` | *"Dokter ini tidak sedang jaga IGD pada waktu penugasan. Isi alasan memilih dokter di luar jadwal jaga."* |
| 10 | `400` | *"Alasan memilih dokter di luar jadwal jaga paling banyak 500 karakter."* |

Urutan `POST /`: `404` kunjungan → `400` dokter → `400` waktu → **`400` aturan 8–10** → `409` penugasan berjalan.
Urutan `handover`: `400` alasan pengalihan → `404`/`409` penugasan lama → `404` kunjungan → `400` dokter → `400` waktu
(dua aturan) → **`400` aturan 8–10**. Keduanya sesuai validation §13.3.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| QBE preflight | Area `HealthServices`, Module `EmergencyInstallationManagement`, prefix registry `Emg` (`ACTIVE / LEGACY`); keberlakuan `TOUCHED LEGACY` (service, controller, DTO, model, configuration). QBE yang berlaku: `QBE-SVC-001` (logika di service; controller tetap tipis), `QBE-API-001`, `QBE-PERM-001` (metadata akses tidak berubah), `QBE-VAL-001` (aturan 8–12), `QBE-DTO-001`, `QBE-ENT-001` (entity tetap mewarisi `IdentityModel`), `QBE-CFG-002` | `PASS` | Bagian 3 |
| Review diff/scope | 5 berkas, +172/−95; hanya berkas pada Cakupan kartu | `PASS` | `git diff --stat` |
| `git diff --check` | Bersih | `PASS` | Keluaran perintah |
| Akhiran baris | Kelima berkas tetap CRLF tanpa BOM | `PASS` | Hitungan byte lewat Node |
| Komentar | Nol baris komentar **baru**. Tiga komentar lama ikut pindah utuh ke `KueriJadwalIgdBerlaku`. **Lima baris komentar lama dihapus** karena isinya menjadi keliru — lihat §7 | `PASS` dengan delta | `git diff -U0` |
| Pemanggil lain | `TetapkanAsync`/`AlihkanAsync` hanya dipanggil controller ini; nol penulis `EmgDoctorAssignment` lain | `PASS` | `grep` |
| `dotnet build` | Berhasil — dijalankan pemilik 11 Oktober 2026 (build pertama lewat `migrations add`; build ulang sebelum `database update` sempat membuat laptop mati, lalu berhasil). DLL `bin/Debug/net9.0` terakhir ditulis 15.23 WIB, sesudah source terakhir berubah (11.25 WIB). **Pemilik melaporkan 0 error dan 0 warning** (11 Oktober 2026 malam). Build ulang tanpa perubahan source melaporkan 0 warning karena tidak mengompilasi; build penuh 6 Oktober tercatat 235 warning (`IGD-FACT-064`, `108`) | `PASS` | Pernyataan pemilik |
| Migration | `20261011052215_AddEmergencyDoctorRosterOverride` dibuat pemilik. Isi diperiksa agent: dua `AddColumn` (`boolean` bawaan `false`; `character varying(500)` nullable); snapshot +8/−1 (dua properti, ditambah BOM baris pertama dari alat EF); nol perubahan model lain. Penjaga `Down()` ditambahkan agent (pola `BE-IGD-060`); berkas tetap CRLF ber-BOM seperti migration lain. **Diterapkan ke basis data dev oleh pemilik 11 Oktober 2026** (`dotnet ef database update`). Uji `Up→Down→Up` **dikecualikan atas keputusan pemilik 11 Oktober 2026** (`IGD-DEC-260`) | `PASS` (isi, penerapan) / dikecualikan (`Down`) | Pernyataan pemilik; berkas migration |
| Uji API dan layar `AT-IGD-214`…`220` — putaran Antigravity 11 Oktober 2026, akun perawat IGD | 214, 215, 217, 218 lulus; 216 lulus di sisi backend; 220 sebagian (a lulus; b dan d disahkan sebagai delta, `IGD-DEC-258`); 219 tidak dijalankan | `PASS` / `PARTIAL` / `NOT RUN` | §8 |
| Uji ulang API R-01…R-07 — Antigravity 11 Oktober 2026 malam | Ketujuh lulus: `onlyOnDuty=true`, `search`, `GET /active`, baris sebelum migration, urutan `400` sebelum `409`, aturan 11 pada pengalihan, pemangkasan alasan di server | `PASS` | §9 |

AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)

Uji manual: dijalankan agen Antigravity atas nama pemilik bersama `FE-IGD-037` (roadmap R3.17.2) dan diulang malam
harinya untuk R-01…R-07; agent memutus dari bukti mentah dan log backend (§8, §9).

**Tidak dijalankan oleh agent:** `dotnet build` dan `dotnet ef migrations add` (keduanya milik pemilik);
`dotnet ef database update`; uji `Down()` (dikecualikan, `IGD-DEC-260`).

---

## 6. Acceptance criteria dan Definition of Done

| # | Kriteria | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | Dokter jaga = jadwal aktif yang berlaku pada T di unit (atau unit induk klinik) bertipe `Emergency`, termasuk shift lintas tengah malam; nama/kode unit tidak dibaca | **Terpenuhi dengan delta** — jadwal di unit *Instalasi Gawat Darurat* (`Emergency`) dihitung (`AT-IGD-214`); shift lintas tengah malam berlaku dari hari mulainya (`AT-IGD-220a`). Batas akhir 07.30 (220b) dan unit bernama "IGD" bertipe lain (220d) **disahkan pemilik sebagai delta berdasarkan source** (`IGD-DEC-258`) | §8.2; `KueriJadwalIgdBerlaku` tanpa `ILike` nama/kode |
| 2 | Kelompok di luar jadwal selalu dikirim (kecuali `onlyOnDuty=true`), aturan 5–7; satu dokter sekali; dokter jaga lebih dulu | **Terpenuhi dengan delta** — kelompok kedua dikirim walau ada dokter jaga, dokter jaga lebih dulu, urut nama, satu dokter sekali, spesialis tidak ditawarkan (`AT-IGD-214`, `218`); `onlyOnDuty=true` hanya mengirim dokter jaga (R-01); `search` menyaring kedua kelompok dan tidak memunculkan spesialis (R-02). Aturan 6 disahkan sebagai delta berdasarkan source (`IGD-DEC-258`) | `AT214-api.json`, `AT218-api.json`, `R-01-api.json`, `R-02-api.json` |
| 3 | Label kelompok kedua *"(di luar jadwal jaga)"*; ruas jadwal dan `sessionName` `null`; *"Dokter Jaga IGD"* tidak pernah pada `isOnDuty=false` | **Terpenuhi** | `AT214-api.json` dan keempat respons `AT220-api.json`: baris `isOnDuty=false` berlabel *"— Umum (di luar jadwal jaga)"*, ruas jadwal dan `sessionName` `null`; *"Dokter Jaga IGD"* tidak muncul |
| 4 | Aturan 8–12 pada urutan validation §13.3, pesan persis | **Terpenuhi** — aturan 8, 9 (tanpa ruas dan spasi saja), 10 dengan pesan persis dan nol baris tercatat (`AT-IGD-217`, `218`); aturan 11 — alasan yang terkirim untuk dokter jaga diabaikan pada pengalihan (R-06); aturan 12 (`AT-IGD-216`, R-07); urutan: `400` aturan 9 mendahului `409` penugasan berjalan (R-05) | §8.2, §9 |
| 5 | Override tersimpan dengan alasan ter-*trim*; pada pengalihan `AssignmentReason` terpisah | **Terpenuhi** — override tersimpan pada penetapan dan pengalihan, `AssignmentReason` terpisah (`AT-IGD-216`); alasan `"   Uji pemangkasan alasan di server   "` yang dikirim langsung lewat API tersimpan tanpa spasi di ujung (R-07) | `AT216-api.json`, `R-07-api.json` |
| 6 | `GET /` dan `GET /active` membawa dua ruas baru; baris lama `false`/`null` | **Terpenuhi** — `GET /` membawa kedua ruas (`AT-IGD-215`, `216`); `GET /active` membawa `isRosterOverride=true` dan alasannya (R-03); baris 9 Oktober 2026, sebelum migration, bernilai `false`/`null` (R-04) | `AT215-api.json`, `AT216-api.json`, `R-03-api.json`, `R-04-api.json` |
| 7 | Migration dua kolom diterapkan pemilik; `Down()` berpenjaga; siklus `Up→Down→Up` di basis data terpisah | **Terpenuhi dengan pengecualian** | Migration dibuat, berpenjaga, dan diterapkan ke dev (pemilik, 11 Oktober 2026). Siklus `Up→Down→Up` **dikecualikan atas keputusan pemilik 11 Oktober 2026** (`IGD-DEC-260`) |
| 8 | Diff hanya Cakupan; nol `Program.cs`; alias `options` hilang; nol komentar baru; komentar lama tidak disunting | **Terpenuhi dengan delta** | Lima baris komentar lama dihapus karena menjadi keliru — **disahkan pemilik 11 Oktober 2026** (`IGD-DEC-259`, §7) |
| 9 | Build 0 error; warning dilaporkan | **Terpenuhi** | Build pemilik berhasil 11 Oktober 2026; pemilik melaporkan 0 error dan 0 warning (`IGD-FACT-108`) |

DoD: laporan ini ada; register, grafik, dan traceability diperbarui pada pass yang sama. Butir "rilis bersama
`FE-IGD-037`" dan "`IGD-UNK-18` dicatat untuk lingkungan rilis" **dilepas dari DoD task atas keputusan pemilik 11 Oktober
2026** (`IGD-DEC-264`); keduanya tetap syarat rilis R3.17.3 dan belum dipenuhi.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Migration `20261011052215` sudah diterapkan ke dev. **Lingkungan lain, termasuk demo, wajib menerapkannya sebelum backend ini dijalankan** — tanpa kolom baru, kueri riwayat penugasan gagal. *Sebelumnya:* kolom baru belum ada di basis data mana pun |
| Delta yang disahkan (`IGD-DEC-259`, 11 Oktober 2026) | Lima baris komentar lama **dihapus**, tidak diubah, karena isinya menjadi keliru: tiga baris di atas cadangan lama (*"Jika belum ada jadwal shift terdata … sediakan cadangan … HANYA dokter umum …"*); satu baris remarks controller (*"Dokter praktek poli umum/spesialis otomatis difilter keluar."*); satu baris remarks DTO (*"Dropdown … hanya menampilkan dokter yang sedang bertugas pada shift aktif."*). Bila Anda ingin aturan *"komentar lama tidak disunting"* tetap mutlak, ketiganya dapat dikembalikan apa adanya |
| Masalah yang diketahui | Pembacaan `SpecialistName` memakai pencocokan teks ("Umum", "Emergency", "Gawat Darurat") — warisan source yang disahkan `IGD-DEC-239`; isian yang tidak baku (mis. "GP") membuat dokter umum tidak masuk kelompok kedua (integration §8.2) |
| Risiko tersisa | (a) Rilis tanpa `FE-IGD-037` membuat layar lama tidak dapat menetapkan dokter di luar jadwal. (b) Bila unit IGD di sebuah lingkungan belum bertipe `Emergency`, setiap penetapan menjadi override beralasan (`IGD-UNK-18`). (c) Waktu `effectiveFrom` dari layar ditafsirkan sama dengan query `at` yang sudah ada: nilai UTC diubah ke WIB, nilai lain dianggap sudah jam operasional |
| Perubahan sampingan | `NONE`. Dua berkas Registrasi yang berubah di working tree (`ReferralTargetUnitType.cs`, `EncounterReferralService.cs`) **bukan** dari task ini dan tidak disentuh |
| Interupsi | `NONE` |
| Status Git | Lima berkas source pada §3.2 (`M`), dokumen blueprint IGD (`M`), flowchart baru (`??`), dua berkas Registrasi milik pekerjaan lain (`M`), migration `20261011052215_*` (`??`, dibuat pemilik; `Down()` disunting agent), dan `ApplicationDbContextModelSnapshot.cs` (`M`) |
| Langkah berikutnya | Pemilik: commit, lalu rilis `BE-IGD-056` **bersama** `FE-IGD-037` sesudah `FE-IGD-037` ✅, dengan migration `20261011052215` diterapkan lebih dulu dan `IGD-UNK-18` dicatat untuk lingkungan rilis (R3.17.3, `IGD-DEC-264`). Daftar di bawah adalah urutan awal; seluruhnya sudah dikerjakan 11 Oktober 2026 |

> **Pembaruan 11 Oktober 2026 (siang).** Langkah 1–3 di bawah sudah dikerjakan: migration
> `20261011052215_AddEmergencyDoctorRosterOverride` dibuat pemilik, isinya diperiksa agent, dan penjaga `Down()` sudah
> dipasang. Build kedua yang dipicu `dotnet ef database update` membuat laptop pemilik mati sendiri. Jalur penggantinya:
> (a) build ringan `dotnet build-server shutdown` → `dotnet build -p:RunAnalyzers=false -m:1 -nodeReuse:false` →
> `dotnet ef database update --no-build`; atau (b) skrip SQL setara migration ini, dijalankan pemilik di klien
> PostgreSQL, termasuk baris `__EFMigrationsHistory` (`MigrationId` = nama migration, `ProductVersion` = `9.0.18`).
> Uji API tetap butuh satu build supaya backend dapat dijalankan.

**Langkah berikutnya untuk Rizki.**

1. Build backend dan laporkan jumlah error dan warning:
   `dotnet build -p:RunAnalyzers=false`
2. Buat migration (nama `DEV_DISCRETION`), lalu beri tahu agent supaya isinya diperiksa:
   `dotnet ef migrations add AddEmergencyDoctorRosterOverride`
3. Agent menambahkan penjaga di awal `Down()` mengikuti pola `BE-IGD-060` (atau Anda menempelkannya sendiri):

   ```csharp
   migrationBuilder.Sql("""
       DO $$
       BEGIN
           IF EXISTS (SELECT 1 FROM public."EmgDoctorAssignment" WHERE "IsRosterOverride") THEN
               RAISE EXCEPTION 'Down BE-IGD-056 dihentikan: ada penugasan DPJP IGD di luar jadwal jaga (IsRosterOverride benar). Menghapus kolom ini menghilangkan jejak override tanpa bekas.';
           END IF;
       END $$;
       """);
   ```

4. `dotnet ef database update` pada basis data dev (oleh Anda).
5. Izin `build-module-frontend` `FE-IGD-037`, lalu satu putaran uji Antigravity `AT-IGD-214`…`220`.

---

## 8. Pemeriksaan bukti uji — 11 Oktober 2026

Uji dijalankan agen Antigravity atas nama pemilik bersama `FE-IGD-037`, 16.07–16.42 WIB. Agent memutus dari bukti mentah
`QuilvianSystemFrontendDev/test-with-agy/igd/uji-dpjp-dokter-jaga-2026-10-11/` (`data-uji.json`, dua belas JSON
skenario dan jaringan, lima PNG, sembilan skrip) yang dicocokkan dengan `Logs/quilvian-backend-20261011.json` (ruas
`UserName`, `Path`, `StatusCode`, `LogType` `HttpRequest` dan `UserAction`), bukan dari ringkasan
[laporan penguji](../../../testing/2026-10-11-laporan-uji-antigravity-dpjp-dokter-jaga.md). Sisi layar:
[laporan `FE-IGD-037` §9](../frontend/FE-IGD-037.md).

### 8.1 Lingkungan dan aturan uji

| Pemeriksaan | Hasil |
| --- | --- |
| Backend | Dinyalakan 15.32 WIB dan tidak dinyalakan ulang sampai uji selesai. Respons memuat pesan aturan 8–10 dan ruas `isRosterOverride`, jadi backend yang melayani memuat diff ini dan migration-nya |
| Frontend | Build produksi: lima PNG dengan `nextjsPortalNull: true` dan tanpa lencana "N" (laporan `FE-IGD-037` §9.1) |
| Akun | `dimas.kurniawan` (perawat IGD), `isSuperAdmin: false`; nol permintaan akun ini ke `/api/v1/administrator/**` |
| Sandi | Dibaca dari variabel lingkungan; nol nilai sandi atau token di JSON; `Authorization` dan `Cookie` disamarkan |
| Larangan | Nol SuperAdmin, *Simpan Akses*, `role-access`, atau SQL di skrip |
| SuperAdmin | Satu sesi SuperAdmin 15.33–15.54 WIB (Chrome desktop dan Android; 42 permintaan, semuanya `GET` kecuali login; layar pasien, penjamin, asuransi, rujukan) — sebelum putaran dan bukan pola skrip penguji. **Menunggu konfirmasi pemilik** bahwa itu sesi pemilik |

### 8.2 Putusan per skenario

| Skenario | Putusan agent | Bukti yang menentukan |
| --- | --- | --- |
| `AT-IGD-214` | **PASS** | `GET …/on-duty-doctors` 16.30.51 `200`: dr. Farhan Malik `isOnDuty=true`, Shift Siang 14:00–21:00, unit *Instalasi Gawat Darurat*; dr. Ranger Biru dan dr. Rendy Pangalila `isOnDuty=false`, *"— Umum (di luar jadwal jaga)"*; D3 tidak ada; tanpa duplikat. Layar: laporan `FE-IGD-037` §9.2 |
| `AT-IGD-215` | **PASS** | `POST` 16.34.32 tanpa `rosterOverrideReason` → `201`; riwayat V1 `isRosterOverride=false`, alasan `null` |
| `AT-IGD-216` | **PASS** di sisi backend | V2: `POST` 16.35.37 `201`, `isRosterOverride=true`, alasan *"Dua dokter jaga sedang resusitasi"*. V1: `POST …/handover` 16.35.48 `200`, `assignmentReason` *"Pergantian shift dinas IGD"* terpisah dari alasan override; `effectiveTo` D1 = `effectiveFrom` D2 = `09:35:48.017766Z`. Penanda di layar: laporan `FE-IGD-037` §9.2 |
| `AT-IGD-217` | **PASS** | Kunjungan V3, dokter D2: tanpa ruas dan spasi saja → `400` aturan 9; 501 karakter → `400` aturan 10; pesan persis; riwayat V3 tetap 0 baris. Log: tiga `POST` `400` 16.32.30–32 |
| `AT-IGD-218` | **PASS** | Prasyarat: kelompok kedua tidak kosong dan D3 tidak ditawarkan. `POST` V3 + D3 + alasan → `400` aturan 8, pesan persis; riwayat V3 0 baris. Penolakan sampai aturan 8 (bukan pesan dokter tidak aktif) membuktikan D3 dokter aktif; spesialisasinya tidak terlihat di bukti mana pun |
| `AT-IGD-219` | **NOT RUN** | Tidak ada jam tanpa jadwal IGD di dev. Bukan kegagalan |
| `AT-IGD-220` a | **PASS** | `?at=2026-10-12T02:00:00` (Senin) → dr. Farhan Malik Shift Malam 21:00–07:00, `isOvernight=true`, `isOnDuty=true` — berlaku lewat cabang hari sebelumnya |
| `AT-IGD-220` b | **Tidak meyakinkan** — keterbatasan data | `?at=…T07:30:00` → dr. Farhan Malik tetap `isOnDuty=true` lewat Shift Pagi 07:00–14:00. Daftar menampilkan satu baris per dokter (urut `SortOrder`), jadi respons tidak dapat menunjukkan apakah Shift Malam masih dihitung pada 07.30. Harapan kontrak "07.30 `false`" tidak dapat diamati dengan data ini. Bukan kegagalan skrip maupun perilaku |
| `AT-IGD-220` c (tambahan panduan) | **Sebagian** | 20:59 → Shift Siang; 21:00 → Shift Malam. Peralihan teramati, tetapi karena dokter yang sama memegang kedua shift, hanya satu dari dua batas yang terbukti |
| `AT-IGD-220` d | **NOT RUN** | Data uji tidak ada: unit bernama "IGD" bertipe bukan `Emergency` (prasyarat R3.17.2, disiapkan pemilik) |

### 8.3 Pencocokan dengan log backend

| Putaran resmi | Jam WIB | Catatan bukti | Log akun `dimas.kurniawan` | Cocok |
| --- | --- | --- | --- | :-: |
| `AT-IGD-214` API | 16.30.50–51 | Login + 1 `GET` | Sama | Ya |
| `AT-IGD-214` layar | 16.30.55–16.31.08 | Pintu A 11, pintu B 9 | 11 dan 9 | Ya |
| `AT-IGD-220` | 16.32.00–01 | Login + 4 `GET ?at=` | Sama | Ya |
| `AT-IGD-217` | 16.32.30–32 | Login + 3 `POST` `400` + 1 `GET` | Sama | Ya |
| `AT-IGD-218` | 16.32.53–54 | Login + `GET` + `POST` `400` + `GET` | Sama | Ya |
| `AT-IGD-215` | 16.34.22–34 | 7 permintaan layar (termasuk `POST` `201`) + 1 `GET` skrip | Sama | Ya |
| `AT-IGD-216` | 16.35.27–50 | 7 + 7 permintaan layar (`POST` `201`, `POST …/handover` `200`) + 2 `GET` skrip | Sama | Ya |
| `AT-IGD-219` | 16.42.10 | Login + 1 `GET` | Sama | Ya |

Seluruh penulisan hari itu: 3 berhasil — semuanya dilaporkan dan cocok dengan 3 baris `UserAction` — dan 8 `POST` `400`
(4 dilaporkan, 4 tidak; §8.4 a). Nol `PUT`, `PATCH`, atau `DELETE`. Percobaan pertama `AT-IGD-215` (16.33.29, galat
selector) dilaporkan penguji dan tidak menulis apa pun.

### 8.4 Penyimpangan — menunggu putusan pemilik

| # | Penyimpangan | Bukti | Dampak data | Usulan agent |
| ---: | --- | --- | --- | --- |
| a | Eksplorasi 16.07–16.22 WIB tidak dilaporkan dan skripnya tidak tersimpan: 190 permintaan, termasuk **4 `POST /emergency-doctor-assignments` → `400`** (16.20.05; 16.22.22 tiga kali), 112 `GET on-duty-doctors`, tiga `403` (`doctors/options`, `doctors`, `doctor-schedules`), dan lima `404` ke endpoint yang tidak ada. Laporan penguji menulis jam mulai 16.24 dan *"seluruh percobaan … dicatat"* | Log | Nol — keempat `POST` ditolak | Sahkan |
| b | Pemindaian 16.36–16.41 WIB tidak dilaporkan: 65 `GET on-duty-doctors` dan satu `404`. Klaim *"168 jam per minggu terisi"* bersandar pada pemindaian ini, hasilnya tidak disimpan | Log | Nol | Sahkan; klaim itu dicatat sebagai pernyataan penguji, bukan bukti |
| c | Satu sesi layar 16.29.17–16.29.26 (pintu A dan B kunjungan V1) tidak dilaporkan — kemungkinan percobaan pertama `AT-IGD-214` | Log | Nol | Sahkan |
| d | Nama pasien di `data-uji.json` dan laporan penguji berbeda dari layar: V1 *Ny. Sari* (layar *Rivaldo Januar*), V2 *Tn. Hartono* (layar *Indra Gunawan*); nomor kunjungan cocok | PNG | Nol | Catat; laporan penguji tidak disunting |
| e | Laporan penguji menulis `effectiveTo` D1 `…09:35:48.337968Z`; bukti `…09:35:48.017766Z` | `AT216-api.json` | Nol | Catat |
| f | Spesialisasi D3 ditulis *"Poli Anak"* di laporan dan *"Poli Spesialis"* di `data-uji.json`; tidak terlihat di bukti mana pun | JSON | Nol | Catat |
| g | Harapan `AT-IGD-220` b diganti di skrip tanpa disebut di laporan | `run-at220.mjs` | Nol | Catat; putusan di §8.2 |

Usulan **sahkan** untuk a–c karena tidak ada penulisan yang berhasil di luar putaran resmi dan setiap permintaan putaran
resmi cocok dengan log. Uji ulang tidak akan mengubah putusan §8.2. **Disahkan pemilik 11 Oktober 2026** (`IGD-DEC-255`);
d–g dicatat (`IGD-FACT-103`) dan laporan penguji tidak disunting.

### 8.5 Sisa sebelum ✅

| # | Hal | Pemilik | Cara menutup |
| ---: | --- | --- | --- |
| 1 | ~~Penyimpangan §8.4~~ | Rizki | **Disahkan** — `IGD-DEC-255` |
| 2 | ~~Acceptance 1: `AT-IGD-220` b dan d~~ | Rizki | **Disahkan sebagai delta berdasarkan source** — `IGD-DEC-258` |
| 3 | Acceptance 2, 4, 5, 6 yang belum diuji | Antigravity | [Panduan uji ulang](../../../testing/2026-10-11-panduan-uji-ulang-dpjp-dokter-jaga.md) R-01…R-07, tanpa mengubah data. Urutan dan pengalihan memakai V1 dan V2 supaya V3 tetap kosong untuk uji layar dan `AT-IGD-219` |
| 4 | ~~Aturan 6 (acceptance 2)~~ | Rizki | **Disahkan sebagai delta berdasarkan source** — `IGD-DEC-258` |
| 5 | ~~Acceptance 7~~ | Rizki | **Dikecualikan** — `IGD-DEC-260` |
| 6 | ~~Acceptance 8~~ | Rizki | **Disahkan** — `IGD-DEC-259` |
| 7 | ~~Acceptance 9~~ | Rizki | **0 warning dilaporkan pemilik** — `IGD-FACT-108` |

Butir 3 ditutup uji ulang malam itu (§9).

---

## 9. Pemeriksaan uji ulang — 11 Oktober 2026 (malam)

Uji ulang dijalankan agen Antigravity atas nama pemilik, 21.07–21.18 WIB, mengikuti
[panduan uji ulang](../../../testing/2026-10-11-panduan-uji-ulang-dpjp-dokter-jaga.md). Agent memutus dari bukti mentah
`QuilvianSystemFrontendDev/test-with-agy/igd/uji-ulang-dpjp-dokter-jaga-2026-10-11/` (`percobaan.json`, `R-01-api.json`
…`R-07-api.json`, sebelas skrip) yang dicocokkan dengan `Logs/quilvian-backend-20261011.json`, bukan dari ringkasan
[laporan penguji](../../../testing/2026-10-11-laporan-uji-ulang-antigravity-dpjp-dokter-jaga.md). Keputusan pemilik:
`IGD-DEC-260`…`264`; fakta `IGD-FACT-106`…`108`.

**Lingkungan.** Backend tetap proses yang dinyalakan 15.32 WIB dari DLL 15.23 WIB; source lima berkas tidak berubah sejak
11.25 WIB (`git diff --stat` tetap +172/−95). Akun `dimas.kurniawan` (`isSuperAdmin: false`); nol SuperAdmin, nol
`/api/v1/administrator/**`, nol SQL; sandi dari variabel lingkungan.

| Skenario | Putusan agent | Bukti yang menentukan |
| --- | --- | --- |
| R-01 | **PASS** — acceptance 2 | `?onlyOnDuty=true` 21.08.29 WIB: satu baris, dr. Farhan Malik `isOnDuty=true` (Shift Malam 21:00–07:00); nol baris `isOnDuty=false` |
| R-02 | **PASS** — acceptance 2 | `?search=Ranger`: hanya D2 `isOnDuty=false`; `?search=Farhan`: hanya D1 `isOnDuty=true`; `?search=Maya`: kosong |
| R-03 | **PASS** — acceptance 6 | `GET /active?emergencyVisitId=V1`: penugasan `904ffcd2-…`, D2, `isRosterOverride=true`, alasan *"Dua dokter jaga sedang resusitasi"* |
| R-04 | **PASS** — acceptance 6 | Kunjungan `IGD-261009030511-26F549`: baris `effectiveFrom` `2026-10-09T03:05:11Z` (sebelum migration) bernilai `isRosterOverride=false`, `rosterOverrideReason=null` |
| R-05 | **PASS** — acceptance 4 (urutan) | `POST` V1 + D2 tanpa alasan → `400` aturan 9 (bukan `409`); `POST` V1 + D1 → `409` *"Kunjungan ini sudah memiliki dokter penanggung jawab. Gunakan aksi pengalihan dokter."*; riwayat V1 tetap 2 baris |
| R-06 | **PASS** — acceptance 4 (aturan 11), 5 | Pengalihan V2 `a9b2cd26-…` → D1 dengan `rosterOverrideReason` *"  alasan ini harus diabaikan  "* → `200`; baris baru `b5129bc3-…` `isRosterOverride=false`, alasan `null`, `assignmentReason` *"Uji aturan 11"* |
| R-07 | **PASS** — acceptance 4 (aturan 12), 5 | Pengalihan V2 `b5129bc3-…` → D2 dengan *"   Uji pemangkasan alasan di server   "* lewat API → `200`; tersimpan *"Uji pemangkasan alasan di server"* |

**Pencocokan.** Setiap permintaan R-01…R-07 cocok dengan log (method, path, kode status, akun): R-01…R-06 21.08.28–21.08.49
WIB (login; 9 `GET` — empat `on-duty-doctors`, dua `/active`, satu `emergency-visits`, dua riwayat; `POST` `400`; `POST`
`409`; `POST …/handover` `200`); R-07 21.11.48–21.11.57 WIB
(login, `GET /active`, `POST …/handover` `200`). Kedua pengalihan tercatat sebagai `UserAction`. Penulisan berhasil lain
pada putaran itu nol.

**Penyimpangan.** Ketiganya ada di sisi layar (`IGD-FACT-107` a–c) dan tidak menyentuh bukti R-01…R-07; disahkan pemilik
(`IGD-DEC-262`). Galat `500` percobaan 2 R-13 terjadi di kueri baca `PeriksaKelayakanAsync` `:669` karena klien memutus
permintaan, sebelum penyimpanan — bukan cacat task ini; riwayat V3 tetap nol baris.

| Hal | Isi |
| --- | --- |
| Status | ✅ **SELESAI — 11 Oktober 2026 (malam)**: 9 dari 9 acceptance terbukti (§6) |
| Backend Governance Preflight | Area `HealthServices`, Module `EmergencyInstallationManagement`, prefix `Emg` (`ACTIVE / LEGACY`), keberlakuan `TOUCHED LEGACY`; QBE: `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-VAL-001`, `QBE-DTO-001`, `QBE-ENT-001`, `QBE-CFG-002` (§5) — tidak berubah sejak implementasi |
| Implementation | Lima berkas source (§3.2), +172/−95, tidak berubah sejak 11.25 WIB; migration `20261011052215` milik pemilik |
| Developer verification | QBE `PASS`; build pemilik 0 error, 0 warning; putaran Antigravity `AT-IGD-214`…`220` (§8) dan uji ulang R-01…R-07 (§9) pada bukti mentah dan log |
| UAT | Belum dijalankan — diserahkan ke tim UAT; bukan klaim UAT |
| Keadaan migration dan eksekusi database | Migration diterapkan pemilik ke dev; uji `Down` dikecualikan (`IGD-DEC-260`); agent tidak menjalankan kueri basis data |
| Risiko tersisa | **Menengah sampai rilis**: rilis tanpa `FE-IGD-037` membuat layar lama tidak dapat menetapkan dokter di luar jadwal; lingkungan yang unit IGD-nya belum bertipe `Emergency` atau tanpa jadwal membuat setiap penetapan menjadi override (`IGD-UNK-18`). Spesialis tetap ditolak sebagai DPJP sesuai `IGD-DEC-261` (spesialis = konsulen) |
| Status Git | Lima berkas source (`M`), migration `20261011052215_*` (`??`), snapshot (`M`), dokumen blueprint IGD; dua berkas Registrasi milik pekerjaan lain (`M`) tidak disentuh |
| Langkah berikutnya | Pemilik: commit; rilis bersama `FE-IGD-037` sesudah task itu ✅ (R3.17.3) |
