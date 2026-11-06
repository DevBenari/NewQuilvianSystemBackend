# Laporan Perubahan Backend — `BE-NV-016`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-NV-016` |
| Judul | API ambang lama menunggu |
| Slice | `MVP-9` — `EPIC NV-09`, dikerjakan di gelombang backend 2 (lihat "Catatan urutan `BE-NV-016`" pada roadmap) |
| Roadmap | `docs/module-blueprints/nurse-view/roadmap/backend-roadmap.md` — bagian Revision 2, kartu `BE-NV-016` |
| Trace | `FR-NV-053`; `NV-DEC-048`, `056`; `G-15`; `NV-OQ-022` (angka saja, runtime); `VAL-NV-40`; `NV-AC-040`; `UAT-25`; `contracts/api-contract.md` R2 §8; `02-backend-architecture.md` R2.1, R2.4.4, R2.4.5, R2.4.6, R2.9; `data/data-dictionary.md` R2.2 (contoh perhitungan warna); permission-audit R2 §9 baris "Ganti ambang" |
| Contract version | `0.2.0` — `approved` (Rizki Gunawan, 2026-11-05, `NV-DEC-055`) |
| Dependency | `BE-NV-008` ✅ (tabel `NvwPatientCallWaitThreshold` dan check constraint `CK_NvwPatientCallWaitThreshold_Order` sudah ada di DB dev) |
| Klasifikasi | `MEDIUM` — skor 5: satu repository (0), berkas diperiksa 9–20 (1), berkas diubah 4–8 termasuk berkas berbagi dengan `BE-NV-009` (1), logika sedang (1), endpoint baru sesuai kontrak terkunci (1), persistence existing tanpa schema baru (1), auth tidak ada (0), UI tidak ada (0) |
| Task mode | `BACKEND` — prompt Rizki 6 Nov 2026, target tulis `NewQuilvianSystemBackend` branch `rizkiG` |
| Target tulis | `Areas/HealthServices/NurseViewManagement/{Constants,Controllers,DTOs,Services}`, `Program.cs` (satu `AddScoped`, restu `NV-DEC-056`), laporan ini, baris status roadmap dan traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `0c6eab34` (branch `rizkiG`); perubahan belum di-commit |
| Tanggal | 2026-11-06 |
| Status | 🟡 **SEBAGIAN** — source lengkap untuk AC-1–5, QBE Strict `PASS` (9 berkas gabungan dengan `BE-NV-009`, 0 violation, 0 review), pasangan atribut akses 2/2 cocok, AC-4 terbukti lewat penelusuran source. **Belum:** `dotnet build` oleh Rizki dan uji HTTP AC-1 → AC-5 oleh Rizki/Antigravity (§5) |

---

## 1. Masalah yang diperbaiki

Konsol Panggilan (`BE-NV-012`) dan strip panggilan di TV (`BE-NV-015`) akan mewarnai panggilan
yang terlalu lama menunggu: kuning, lalu merah. Angka menitnya kebijakan Komite Keperawatan
(`NV-DEC-048`), belum disahkan (`NV-OQ-022`), dan tidak boleh ada angka bawaan di kode (`G-15`).
Sebelum task ini belum ada tempat memasukkan angka itu, dan belum ada satu rumus warna yang bisa
dipakai bersama oleh konsol dan strip.

**Contoh.** Komite mengesahkan kuning 3 menit dan merah 5 menit lewat SK. Tanpa API ini angka itu
hanya bisa masuk lewat SQL, yang dilarang; dan setiap layar berisiko menghitung warnanya sendiri
dengan batas yang berbeda.

---

## 2. Proses bisnis

**Tujuan:** Admin memasukkan ambang kuning dan merah yang disahkan Komite. Selama belum ada
ambang, semua panggilan tampil tanpa warna (aman-gagal).

**Pelaku:** Admin IT dengan `NurseViewPatientCallWaitThreshold : Update`; pembaca dengan `: Read`.

**Langkah normal:**

1. Admin membuka ambang yang berlaku (`GET /`). Sebelum ada pengesahan, jawabannya
   `IsConfigured = false` dan seluruh angka kosong.
2. Sesudah SK terbit, admin mengirim kuning, merah, nomor SK, dan tanggal pengesahan (`PUT /`).
   Sistem memeriksa angka dan tanggal, lalu dalam **satu transaksi** menandai hapus baris ambang
   lama (beserta `DeleteBy` dan `DeleteDateTime`) dan menyisipkan baris baru.
3. Bila Komite mengubah angka, admin mengirim ulang seluruh isian (ganti-seluruh). Baris lama
   tetap tersimpan sebagai jejak, bertanda hapus.
4. Bila Komite mencabut ambang, admin mengirim kedua angka kosong beserta nomor surat
   pencabutannya. Seluruh baris aktif ditandai hapus dan panggilan kembali tanpa warna.

**Aturan angka (`VAL-NV-40`):** kuning 1–240 menit; merah lebih besar dari kuning dan paling banyak
480 menit; keduanya diisi atau keduanya kosong. Rujukan wajib, paling banyak 200 karakter. Tanggal
pengesahan wajib dan tidak boleh melewati hari ini menurut jam operasional (WIB).

**Rumus warna** (`NvwPatientCallWaitThresholdService.ResolveWaitLevel`, dipakai `BE-NV-012` dan
`BE-NV-015`): lama menunggu ≥ merah × 60 detik → `Critical`; ≥ kuning × 60 detik → `Warning`;
selain itu, atau bila ambang belum ada, → `None`.

**Contoh berangka** (kuning 3, merah 5; contoh kamus data R2.2). Panggilan 305-A ditekan 10.00.00:

| Jam | Lama menunggu | Tingkat |
| --- | ---: | --- |
| 10.02.30 | 150 detik | `None` |
| 10.03.00 | 180 detik | `Warning` |
| 10.05.00 | 300 detik | `Critical` |

Selama tabel kosong, ketiganya `None`.

**Jalur tidak normal:**

| Kejadian | Pesan | Kode |
| --- | --- | --- |
| Kuning 3, merah 2 (merah tidak lebih besar) | "Ambang kuning 1–240 menit dan ambang merah harus lebih besar, paling banyak 480 menit." | `400` `VAL-NV-40` |
| Hanya kuning yang diisi, atau hanya merah | Pesan yang sama | `400` `VAL-NV-40` |
| Kuning 0 atau 241; merah 481 | Pesan yang sama | `400` `VAL-NV-40` |
| Rujukan kosong atau > 200 karakter | "Nomor surat/notulen pengesahan Komite Keperawatan wajib diisi." | `400` `VAL-NV-40` |
| Tanggal pengesahan tidak dikirim | "Tanggal pengesahan wajib diisi." | `400` `VAL-NV-40` |
| Tanggal pengesahan besok | "Tanggal pengesahan tidak boleh melewati hari ini." | `400` `VAL-NV-40` |
| Dua admin menyimpan bersamaan | Kunci advisory `pg_advisory_xact_lock` membuat keduanya berurutan; yang kedua menjadi ambang aktif | `200` |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- Governance: sama dengan `BE-NV-009` (`AGENTS.md`, `CLAUDE.md`, `docs/engineering/*`, suite skill
  `rules/backend/*`, `rule-output/status-task-roadmap.md`)
- Blueprint: `roadmap/backend-roadmap.md` Revision 2 (kartu `BE-NV-016`, "Catatan urutan
  `BE-NV-016`"); `contracts/api-contract.md` R2 §8; `contracts/validation-matrix.md` `VAL-NV-12`,
  `13`, `40`; `02-backend-architecture.md` R2.4.4–R2.4.6; `data/data-dictionary.md` R2.2;
  laporan `BE-NV-004.md` (pola ganti-seluruh dan pelajaran no. 13)
- Pola terdekat: `Services/NvwVitalSignIntervalService.cs`, `Controllers/NvwVitalSignIntervalController.cs`,
  `DTOs/NvwVitalSignIntervalDtos.cs`, `Controllers/NvwHttp.cs`
- Model dan configuration `BE-NV-008`: `Models/NvwPatientCallWaitThreshold.cs`,
  `Repositories/Configurations/HealthServices/NurseViewManagement/NvwPatientCallWaitThresholdConfiguration.cs`
- `DTOs/NvwFloorBoardDtos.cs` (preseden tingkat sebagai teks: `EwsRiskLevel` bertipe `string`)

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/NurseViewManagement/Services/NvwPatientCallWaitThresholdService.cs` | **Baru.** `GetActiveAsync` (baris aktif lingkup `GLOBAL`); `ReplaceAsync` ganti-seluruh dalam satu transaksi dengan kunci advisory (pola `NvwVitalSignIntervalService`); `ResolveWaitLevel(threshold, waitingSeconds)` → `None`/`Warning`/`Critical`; `BuildAuditMessage` untuk teks log. Tanpa angka bawaan |
| `…/Controllers/NvwPatientCallWaitThresholdController.cs` | **Baru.** `GET /` dan `PUT /`; `[Tags("Health Services / Nurse View Management / Patient Call Wait Threshold")]`; `ControllerName = NurseViewPatientCallWaitThreshold`; log aksi ditulis lewat `BuildAuditMessage` |
| `…/DTOs/NvwPatientCallWaitThresholdDtos.cs` | **Baru.** `NvwPatientCallWaitThresholdReplaceRequest`, `NvwPatientCallWaitThresholdResponse` sesuai api §8 |
| `…/Services/NvwResult.cs` | Kode `VAL-NV-40` dan pesan `WaitThresholdRange` (berbagi berkas dengan `BE-NV-009`); pesan rujukan dan tanggal memakai pesan `VAL-NV-12`/`13` yang sudah ada |
| `…/Constants/NurseViewPermissions.cs` | Resource `NurseViewPatientCallWaitThreshold` (berbagi berkas dengan `BE-NV-009`) |
| `Program.cs` | +1 baris `builder.Services.AddScoped<NvwPatientCallWaitThresholdService>();` (restu `NV-DEC-056`). Diff gabungan `Program.cs` ada di laporan `BE-NV-009` §3.2 |

Nol baris komentar pada berkas baru.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | 2 endpoint baru persis api §8. Seluruh penolakan memakai kode `VAL-NV-40`, termasuk rujukan dan tanggal, karena matriks R2 menaruh keduanya di baris `VAL-NV-40` dengan pesan `VAL-NV-12`/`13`. **Catatan untuk `BE-NV-012`/`015`:** tingkat warna dikembalikan sebagai teks (`"None"`, `"Warning"`, `"Critical"`), sama dengan contoh JSON api §7 `"waitLevel": "None"` dan preseden `EwsRiskLevel` papan. `ResolveWaitLevel` menerima himpunan ambang sebagai argumen pertama, sehingga konsol dan strip cukup membaca ambang **sekali** per permintaan lalu menghitung setiap item di memori (`NFR-013`); roadmap menuliskannya ringkas sebagai `ResolveWaitLevel(waitingSeconds)` |
| Database | Tidak ada schema baru dan tidak ada migration. Menulis `NvwPatientCallWaitThreshold` (tandai hapus + sisip). Check constraint dan unique berfilter dari `BE-NV-008` menjadi pengaman kedua bila validasi service terlewati |
| Keamanan/Auth | `NOT APPLICABLE` di luar hak akses `[AccessPermission]` biasa. Tidak ada data pasien |

**Teks log** (pelajaran `BE-NV-004` no. 13: rujukan pengesahan wajib di teks pesan, karena argumen
`data` tidak tersimpan):

- "Ambang lama menunggu panggilan pasien diganti: kuning 3 menit, merah 5 menit. Lingkup: GLOBAL. Rujukan: SK Komite Keperawatan No. 12/2026; disahkan 2026-11-06."
- "Ambang lama menunggu panggilan pasien dicabut; panggilan tampil tanpa warna. Lingkup: GLOBAL. Rujukan: …; disahkan …."

Rujukan pencabutan hanya tercatat di log, sama seperti interval TTV (`BE-NV-004`): pencabutan tidak
menyisipkan baris. Berkas log disimpan 14 berkas, jadi jejak ini tidak permanen.

---

## 4. Dokumentasi endpoint

#### Health Services / Nurse View Management / Patient Call Wait Threshold

Base URL: `api/v1/health-services/nurse-view-management/patient-call-wait-thresholds`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/` | Ambang aktif beserta rujukan pengesahan dan pencatatnya; `IsConfigured = false` bila kosong | `NurseViewPatientCallWaitThreshold : Read` |
| `PUT` | `/` | Ganti-seluruh ambang, atau mencabutnya dengan kedua angka kosong | `NurseViewPatientCallWaitThreshold : Update` |

Kode status: `200` berhasil · `400` `VAL-NV-40` · `403` tanpa hak akses.

**Contoh body.**
`{ "warningAfterMinutes": 3, "criticalAfterMinutes": 5, "ratificationReference": "SK Komite Keperawatan No. 12/2026", "ratifiedOn": "2026-11-06" }`
— nomor SK ini samaran untuk uji.

**Contoh body pencabutan.**
`{ "warningAfterMinutes": null, "criticalAfterMinutes": null, "ratificationReference": "Notulen Komite Keperawatan No. 13/2026", "ratifiedOn": "2026-11-06" }`

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path <9 berkas>` (array path, gabungan dengan `BE-NV-009`) | Scope `ExplicitFiles` · Files evaluated **9** (= 9 berkas) · VIOLATION 0 · REVIEW 0 · INFO 0 · `Final result: PASS` | `PASS` | Keluaran checker 2026-11-06 |
| Pasangan atribut akses | 2/2 cocok (`Read`, `Update` × `NurseViewPatientCallWaitThreshold`) | `PASS` | Skrip pemeriksa di scratchpad |
| Tanpa angka bawaan (`G-15`) | Tidak ada nilai awal kuning/merah di service, DTO, maupun seed; `GetActiveAsync` mengembalikan `IsConfigured = false` bila tidak ada baris | `PASS` (source) | Penelusuran source |
| AC-4 penelusuran source `ResolveWaitLevel` dengan kuning 3, merah 5 | 150 detik: 150 ≥ 300? tidak; 150 ≥ 180? tidak → `None`. 180 detik: 180 ≥ 300? tidak; 180 ≥ 180? ya → `Warning`. 300 detik: 300 ≥ 300? ya → `Critical`. Ambang `null` atau `IsConfigured = false` → `None` | `PASS` (source) | `NvwPatientCallWaitThresholdService.ResolveWaitLevel` |
| Nol komentar, akhiran baris CRLF, review diff | Sama dengan `BE-NV-009` §5 | `PASS` | `grep`, hitungan byte Node, `git diff` |
| `SELECT` read-only DB dev | `NvwPatientCallWaitThreshold` 0 baris — titik awal AC-1 | `PASS` | Keluaran `psql`; tidak ada penulisan |
| `dotnet build` | Belum | `NOT RUN` | Dijalankan Rizki; perintah di laporan `BE-NV-009` §5 (satu kali build untuk kedua task) |
| Uji HTTP AC-1 → AC-5 | Belum | `NOT RUN` | Skenario di bawah |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`.

Uji manual: `REQUIRED`.

**Tidak dijalankan:** `dotnet build` dan uji HTTP — milik Rizki menurut aturan task.

### Skenario uji HTTP untuk Antigravity — urut

**Aturan uji.** Sama dengan `BE-NV-009`: aktor berhak = akun peran yang diberi
`NurseViewPatientCallWaitThreshold : Read/Update` lewat Akses Role (salin → tambah → pulihkan),
bukan SuperAdmin; aktor tanpa hak = akun yang tidak memegang resource ini. Tanpa SQL tulis.
Rujukan memakai awalan `UJI-DEV-NV-016-` agar mudah dicari di log. Bukti di
`QuilvianSystemFrontendDev/test-with-agy/nurse-view/uji-be-nv-016-<tanggal>/`. **Angka bukan angka
sah Komite** (`NV-OQ-022`), dan ambang **wajib dicabut lagi** di akhir (no. 12) karena DB dev
dipakai bersama. Tanggal "hari ini" = tanggal WIB saat uji.

| No | Langkah | Hasil yang diharapkan | AC |
| ---: | --- | --- | --- |
| 1 | `GET /` | `200`; `isConfigured` false; semua angka dan rujukan `null` | AC-1 |
| 2 | `PUT /` kuning 3, merah 2, rujukan, hari ini | `400` "Ambang kuning 1–240 menit dan ambang merah harus lebih besar, paling banyak 480 menit." | AC-2 |
| 3 | `PUT /` hanya kuning 3 (merah tidak dikirim), lalu hanya merah 5 | Keduanya `400`, pesan no. 2 | AC-2 |
| 4 | `PUT /` kuning 0 merah 5; kuning 241 merah 300; kuning 3 merah 481 | Ketiganya `400`, pesan no. 2 | AC-2 |
| 5 | `PUT /` kuning 3, merah 5, rujukan, **besok** | `400` "Tanggal pengesahan tidak boleh melewati hari ini." | AC-2 |
| 6 | `PUT /` kuning 3, merah 5, rujukan `""`, hari ini; lalu rujukan 201 karakter | Keduanya `400` "Nomor surat/notulen pengesahan Komite Keperawatan wajib diisi." | AC-2 |
| 7 | `PUT /` kuning 3, merah 5, rujukan, tanpa `ratifiedOn` | `400` "Tanggal pengesahan wajib diisi." | AC-2 |
| 8 | `GET /` | Masih `isConfigured` false — no. 2–7 tidak menulis apa pun | AC-2 |
| 9 | `PUT /` kuning 3, merah 5, `UJI-DEV-NV-016-SET-<jam>`, hari ini | `200`; `isConfigured` true; `warningAfterMinutes` 3; `criticalAfterMinutes` 5; `recordedByName` = aktor; `recordedAt` terisi | AC-3 |
| 10 | `PUT /` kuning 4, merah 6, `UJI-DEV-NV-016-GANTI-<jam>`, hari ini | `200`; angka 4 dan 6, rujukan baru | AC-3 |
| 11 | Akun tanpa hak: `GET /` dan `PUT /` | Keduanya `403` | — |
| 12 | `PUT /` kedua angka `null`, `UJI-DEV-NV-016-CABUT-<jam>`, hari ini | `200`; `isConfigured` false | AC-5 |
| 13 | `GET /` | `isConfigured` false | AC-5, AC-1 |
| 14 | Pindai log hari uji | Tiga baris: "…diganti: kuning 3 menit, merah 5 menit… Rujukan: UJI-DEV-NV-016-SET-…", "…kuning 4 menit, merah 6 menit… GANTI…", "…dicabut… CABUT…", masing-masing dengan pengguna aktor | AC-3, AC-5 |

Sesudah uji, agent memeriksa lewat `SELECT` read-only: dua baris `NvwPatientCallWaitThreshold`,
keduanya `IsDelete = true` dengan `DeleteBy` = aktor dan `DeleteDateTime` terisi; nol baris aktif.
AC-4 tidak diuji lewat HTTP pada task ini karena belum ada endpoint yang mengembalikan `WaitLevel`;
buktinya penelusuran source di atas, dan sisi runtime-nya ikut uji `BE-NV-012` (`NV-AC-040`) dan
`BE-NV-015`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC-1 tabel kosong → `GET /` `IsConfigured = false`, dan `ResolveWaitLevel` selalu `None` (`NV-AC-040`) | Terpetakan ke source; runtime belum | `GetActiveAsync` (tanpa baris → `IsConfigured = false`); `ResolveWaitLevel` mengembalikan `None` bila `IsConfigured` bukan `true`. Uji no. 1 |
| AC-2 kuning 3 merah 2 → `400`; hanya kuning → `400`; tanggal besok → `400`; rujukan kosong → `400` (`VAL-NV-40`) | Terpetakan ke source; runtime belum | `ReplaceAsync`: `IsValidPair`, rujukan, tanggal (`AppDateTimeHelper.OperationalDate`). Uji no. 2–8 |
| AC-3 kuning 3, merah 5, rujukan, hari ini → `200`; baris lama bertanda hapus beserta pelakunya | Terpetakan ke source; runtime belum | `ReplaceAsync`: baris aktif `IsDelete = true`, `DeleteBy = actorUserId`, `DeleteDateTime = now`, lalu sisip baris baru, satu transaksi. Uji no. 9–10 + `SELECT` sesudah uji |
| AC-4 menunggu 2 menit 30 detik → `None`, 3 menit → `Warning`, 5 menit → `Critical` | Terpenuhi lewat penelusuran source | §5 baris AC-4; sesuai cara verifikasi kartu ("AC-4 lewat penelusuran source") |
| AC-5 kedua angka dikosongkan dengan rujukan pencabutan → `IsConfigured = false` | Terpetakan ke source; runtime belum | `IsRevocation` → semua baris aktif ditandai hapus, tanpa sisipan; rujukan masuk teks log. Uji no. 12–14 |
| DoD — build Rizki lulus | **Belum terpenuhi** | Menunggu build |
| DoD — uji HTTP tercatat | **Belum terpenuhi** | Menunggu uji no. 1–14 |
| DoD — laporan ada | Terpenuhi | Berkas ini |

**QBE yang berlaku dan dipatuhi:** `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`
(aktor + rujukan di teks pesan), `QBE-DTO-001`, `QBE-VAL-001`, `QBE-TXN-001` (tandai hapus + sisip
dalam satu transaksi berkunci), `QBE-DEL-001` (tidak ada hapus fisik; baris lama bertanda hapus
beserta aktornya), `QBE-AUD-001` (jejak baris tetap di tabel, terpisah dari log aplikasi). Tidak
berlaku: `QBE-ENT-*`, `QBE-CFG-*`, `QBE-CODE-*` (tanpa nomor bisnis), `QBE-PAGE-001` (bukan daftar),
`QBE-DB-*`.

**Backend Governance Preflight:** sama dengan `BE-NV-009` — Area `HealthServices` · Module
`NurseViewManagement` · prefix `Nvw` `ACTIVE` · `NEW CODE`; `Program.cs` = `TOUCHED LEGACY` (hanya
tambahan, restu `NV-DEC-056`) · branch `rizkiG`.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Angka uji (3/5, 4/6) bukan angka sah. Selama `NV-OQ-022` belum diputuskan Komite, ambang di DB dev harus kosong sesudah uji |
| Masalah yang diketahui | Rujukan pencabutan tidak tersimpan di tabel, hanya di log 14 berkas — batas yang sama dengan `BE-NV-004` dan sudah diterima Rizki untuk interval TTV |
| Risiko tersisa | `ResolveWaitLevel` belum dipanggil siapa pun sampai `BE-NV-012`/`015`; kesalahan pemakaian (misalnya memanggil `GetActiveAsync` per item) baru terlihat di sana |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Sama dengan laporan `BE-NV-009` §7 (satu working tree) |
| Langkah berikutnya | Rizki build sekali; uji HTTP no. 1–14; sesudah itu `BE-NV-016` bisa ✅ dan menjadi prasyarat `BE-NV-012`, `BE-NV-015`, serta pasangan frontend `FE-NV-015` |
