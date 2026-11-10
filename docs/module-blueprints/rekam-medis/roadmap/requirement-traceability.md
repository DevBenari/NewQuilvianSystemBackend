# Rekam Medis — Requirement Traceability

```yaml
module_id: RM-BP-001
roadmap_revision: 1
status: DRAFT
input_revisions:
  interview_decisions: 4
  capability_map: 2
contract_versions:
  api: 0.1.0 (draft)
  state_transition: 0.1.0 (draft)
  validation: 0.1.0 (draft)
  integration: 0.1.0 (draft)
  permission_audit: 0.1.0 (draft)
source_commits:
  backend: ab37e3a2e80f0e34efe22ec0f6a8c9b90a3ae45e
  frontend: c4e2ef2a6080f3ce328d2faad79be1893ac13e22
```

Dokumen ini menjawab satu pertanyaan: **apakah setiap keputusan yang diambil benar-benar
dikerjakan seseorang dan dibuktikan sesuatu?** Keputusan yang tidak terhubung ke task atau
tidak terhubung ke uji dikeluarkan sebagai celah pada bagian 4.

---

## 1. Telusur keputusan menuju task dan uji

| Requirement | Decision ID | Design/ERD | Contract | Backend task | Frontend task | Test/evidence | Status |
|---|---|---|---|---|---|---|---|
| Modul mengelola berkas, bukan menulis isi klinis | `RM-DEC-001` | `02-backend-architecture.md#3-tabel-kepemilikan-data` | — | Seluruh task | Seluruh task | Tabel kepemilikan data | Planned |
| Rilis pertama: penelusuran, keutuhan, jejak akses | `RM-DEC-002` | `02-backend-architecture.md#2-bounded-context` | api `0.1.0` | `BE-13`, `BE-14` | `FE-01` | `AT-RM-09`, `AT-RM-31` | **Terpenuhi** — `BE-13` selesai 26 Agustus 2026 dan `BE-14` selesai 27 Agustus 2026. `AT-RM-09` dan `AT-RM-31` terbukti pada `MedicalRecordTimelineTests`; endpoint-nya terbukti pada `MedicalRecordFileEndpointTests`. Bentuk balasan `/timeline` berubah, menunggu pengesahan pemilik API |
| Penguncian dua lapis | `RM-DEC-003` | `erd/keutuhan-dokumen.md` | state `0.1.0` | `BE-02`, `BE-04`, `BE-07` | `FE-03` | `AT-RM-02`, `AT-RM-03`, `AT-RM-11`, `AT-RM-18` | Planned |
| Addendum hanya oleh penulis, atau pengganti bila berhalangan | `RM-DEC-004` | `erd/keutuhan-dokumen.md` | state `0.1.0` | `BE-06` | `FE-04` | `AT-RM-04`, `AT-RM-05`, `AT-RM-14`, `AT-RM-17`, `AT-RM-28` | Planned |
| Akses terbuka dengan rem alasan | `RM-DEC-005` | `erd/jejak-akses.md` | permission `0.1.0` | `BE-11`, `BE-12` | `FE-02`, `FE-05` | `AT-RM-07`, `AT-RM-29` | Planned |
| Kunjungan tidak pernah dibuka kembali; entri susulan bertanda | `RM-DEC-006` | `contracts/state-transition-matrix.md#3` | state `0.1.0` | `BE-02` | — | `AT-RM-11` | **Sebagian** — lihat celah `GAP-01` |
| Tiga owner ditetapkan sebelum lanjut | `RM-DEC-008` | — | — | — | — | — | **Belum dikerjakan** — blocker, bukan task |
| Status keutuhan berdampingan, bukan mengganti | `RM-DEC-013` | `erd/keutuhan-dokumen.md`; `02-backend-architecture.md#1` | state `0.1.0` | `BE-01`, `BE-02` | `FE-01` | `AT-RM-24`, `AT-RM-39` | Planned |
| Perlakuan catatan lama | `RM-DEC-014` | `02-backend-architecture.md#82-rencana-migration` | — | `BE-08` | — | `AT-RM-21`, `AT-RM-33`, `AT-RM-43` | **Selesai** 2 November 2026 — 11 uji lulus; dijalankan di devYoga (10 CPPT → `LockedUnsigned`). Produksi: syarat pemasangan |
| Jejak akses di tabel database | `RM-DEC-015` | `erd/jejak-akses.md` | permission `0.1.0` | `BE-10` | — | `AT-RM-08`, `AT-RM-12`, `AT-RM-30` | Planned |
| Pasien rawatan = punya kunjungan aktif | `RM-DEC-016` | `erd/jejak-akses.md#3` | permission `0.1.0` | `BE-11` | `FE-02` | `AT-RM-06`, `AT-RM-07`, `AT-RM-25` | Planned |
| `SuperAdmin` tunduk aturan akses rekam medis | `RM-DEC-017` | `contracts/permission-audit-matrix.md#5` | permission `0.1.0` | `BE-11` | — | `AT-RM-13` | Planned |
| Kerahasiaan tetap label, diberi keterangan jujur | `RM-DEC-018` | `02-backend-architecture.md#11` | — | — | `FE-01` | `AT-RM-42` | Planned |
| Tiga celah ditutup pada slice pertama | `RM-DEC-019` | `02-backend-architecture.md#7` | api `0.1.0` | `BE-03` | — | `AT-RM-01`, `AT-RM-19`, `AT-RM-20`, `AT-RM-34` | Planned |
| Definisi berhalangan | `RM-DEC-020` | `erd/keutuhan-dokumen.md` | validation `0.1.0` | `BE-05` | — | `AT-RM-14`, `AT-RM-26`, `AT-RM-27` | Planned |
| Tanda tangan cukup identitas pengguna masuk | `RM-DEC-021` | `erd/data-dictionary.md#1` | api `0.1.0` | `BE-04` | `FE-03` | `AT-RM-02` | Planned |
| `PrivateNote` tersembunyi, terbuka lewat akses beralasan | `RM-DEC-022` | `contracts/validation-matrix.md#4` | api `0.1.0` | `BE-15` | `FE-01`, `FE-08` | `AT-RM-16`, `AT-RM-37` | **Terpenuhi pada kode** — `AT-RM-37` terbukti pada `BE-14` dan `AT-RM-16` pada `BE-15`, 9 uji lulus. **Butir komunikasi DoD belum dijalankan:** penulis CPPT belum diberi tahu, lihat `roadmap/BE-15-pemberitahuan-penulis-cppt.md` |
| Masa simpan jejak ditetapkan sebelum desain | `RM-DEC-023` | `erd/jejak-akses.md#4` | — | `BE-10` | — | — | **Tertutup** — 25 tahun, `RM-DEC-024` |
| Desain berjalan di atas keputusan draft | `RM-DEC-025` | Seluruh artefak | — | — | — | Peringatan pada setiap artefak | Selesai |

---

## 2. Telusur capability menuju task

Kolom terakhir menjawab: apakah temuan audit benar-benar ditutup seseorang?

| Capability | Status audit | Ditutup task | Test/evidence | Keterangan |
|---|---|---|---|---|
| `RM-CAP-004` — pengambilan gabungan 13 sumber | `Reuse with adapter` | `BE-13`, `BE-14` | `AT-RM-09`, `AT-RM-31`, `AT-RM-32`, `AT-RM-37` — `MedicalRecordTimelineTests.cs` 10 uji dan `MedicalRecordFileEndpointTests.cs` 11 uji, seluruhnya lulus | **Ditutup.** Lapisan penggabung `MedicalRecordTimelineService` beserta 4 endpoint `MedicalRecordController`. Tanpa tabel baru, sesuai status audit |
| `RM-CAP-007` — penggabungan pasien duplikat | **`Conflict`** (naik dari `Unknown`, ditelusuri 24 Agustus 2026) | `BE-16` | `AT-RM-22` — `tests/.../MergedPatientGuardTests.cs`, 7 uji lulus | **Ditutup.** Closure question nomor 8 dijawab `RM-DEC-026`: menolak membuka, bukan menyatukan saat dibaca. Keempat pintu masuk berkas dijaga `409`. Sisa: closure question nomor 10 — berapa banyak pasien seperti ini pada data nyata |
| `RM-CAP-033` — penggabungan tidak dapat dipakai dari antarmuka | **`Repair`** (temuan baru) | **Tidak ditutup** | — | Lihat celah `GAP-07`. Milik `PatientManagement`, bukan modul ini |
| `RM-CAP-008` — rute dan menu frontend | `Missing` | — | `route-smoke.spec.mjs` | `FE-07` |
| `RM-CAP-009` — model status tidak seragam | `Conflict` | `BE-01`, `BE-02` | `AT-RM-24` | Ditutup dengan status berdampingan |
| `RM-CAP-010` — aturan penguncian tersebar | `Extend` | `BE-02` | `AT-RM-01` | Dipusatkan di satu service |
| `RM-CAP-011` — CPPT dapat diubah tanpa batas | `Repair` | `BE-03` | `AT-RM-01` | — |
| `RM-CAP-012` — penulis dapat dipindahkan | `Repair` | `BE-03` | `AT-RM-19` | Ditutup dua lapis: perbaikan controller dan `AuthorUserId` |
| `RM-CAP-013` — penanda read-only dapat dilepas | `Repair` | `BE-03` | `AT-RM-20` | — |
| `RM-CAP-014` — tanda tangan elektronik | `Missing` | `BE-04` | `AT-RM-02` | — |
| `RM-CAP-016` — addendum | `Missing` | `BE-06` | `AT-RM-04` | — |
| `RM-CAP-017` — penyimpanan nilai lama | `Missing` | **Tidak ditutup** | — | Lihat celah `GAP-02` |
| `RM-CAP-018` — penguncian saat kunjungan ditutup | `Extend` | `BE-07` | `AT-RM-03` | — |
| `RM-CAP-019` — validasi perpindahan status kunjungan | `Repair` | **Tidak ditutup** | — | Lihat celah `GAP-03` |
| `RM-CAP-022` — jejak akses baca | `Missing` | `BE-10`, `BE-11` | `AT-RM-12` | Planned |
| `RM-CAP-023` — `AuditAsync` tidak terpakai | `Repair` | **Tidak ditutup** | — | Lihat celah `GAP-04` |
| `RM-CAP-024` — kewenangan per pasien | `Missing` | `BE-11` | `AT-RM-06`, `AT-RM-07` | — |
| `RM-CAP-025` — bypass `SuperAdmin` | `Conflict` | `BE-11` | `AT-RM-13` | Ditutup **sebagian**, lihat `GAP-05` |
| `RM-CAP-026` — kerahasiaan tidak ditegakkan | `Conflict` | **Tidak ditutup** | `AT-RM-42` | Sengaja, `RM-DEC-018`. Ditutup dengan keterangan jujur, bukan penegakan |
| `RM-CAP-027` — kerahasiaan `PrivateNote` | `Unknown` | `BE-15` | `AT-RM-16` — `tests/.../MedicalRecordPrivateNoteTests.cs`, 9 uji lulus | **Ditutup pada kode.** Kolom ini ternyata tidak pernah rahasia; sekarang dibuka lewat satu jalur beralasan, berizin terpisah, dan tercatat. Sisa: pemberitahuan ke penulis CPPT |
| `RM-CAP-032` — tidak ada uji otomatis | `Missing` | `BE-00` **selesai**; `BE-17`, `FE-09` menyusul | `dotnet test`: `Failed: 0, Passed: 4`, tiga kali berturut-turut | Fondasi uji backend sudah ada di `tests/`. Cakupan masih fondasi, belum menyentuh alur klinis |

---

## 3. Telusur uji menuju task

43 uji penerimaan pada `testing/acceptance-test-matrix.md`. Seluruhnya terhubung ke sekurangnya
satu task.

| Kelompok uji | Jumlah | Task yang menghasilkannya |
|---|---:|---|
| Keutuhan dokumen | 7 | `BE-02`, `BE-03`, `BE-04` |
| Addendum dan kewenangan penulis | 7 | `BE-05`, `BE-06` |
| Jejak dan kewenangan akses | 9 | `BE-10`, `BE-11`, `BE-12` |
| Penelusuran berkas | 4 | `BE-13`, `BE-14`, `BE-16` |
| Perubahan perilaku kode berjalan | 7 | `BE-03`, `BE-07`, `BE-08` |
| Privasi dan pencatatan | 3 | `BE-06`, `BE-11`, `BE-15` |
| Antarmuka | 5 | `FE-01`, `FE-02`, `FE-04` |
| Konsolidasi jalur gagal | — | `BE-17` |

**Tidak ada uji yang menggantung tanpa task.**

---

## 4. Celah cakupan

Ini bagian terpenting dokumen ini. Tujuh celah ditemukan dan dinyatakan terbuka, bukan
disamarkan.

### `GAP-01` — Entri susulan belum punya task

| Aspek | Isi |
|---|---|
| Yang tidak tercakup | `RM-DEC-006` menetapkan hasil susulan masuk sebagai entri bertanda `Susulan`, tetapi tidak ada task yang membuatnya |
| Mengapa | Entri susulan baru muncul ketika ada modul yang menghasilkan hasil terlambat, yaitu Laboratorium. Modul itu belum ada |
| Dampak sekarang | Tidak ada. Tanpa Laboratorium, tidak ada hasil susulan |
| Kapan harus ditutup | Bersamaan modul Laboratorium |
| Terkait | `RM-DEC-007` masih terbuka: sampai kapan entri susulan diterima |

### `GAP-02` — Nilai lama sebelum penguncian tidak tersimpan

| Aspek | Isi |
|---|---|
| Yang tidak tercakup | `RM-CAP-017` berstatus `Missing` dan tidak ditutup task mana pun |
| Mengapa | Keputusan sadar pada arsitektur bagian 11. Setelah terkunci, isi tidak berubah lagi, sehingga tidak ada nilai lama yang perlu disimpan |
| Dampak sekarang | **Perubahan pada dokumen yang masih `Draft` tetap tidak meninggalkan jejak.** Bila sebuah catatan diubah lima kali sebelum ditandatangani, keempat versi awalnya hilang |
| Kapan harus ditutup | Bila owner menilai jejak perubahan sebelum penandatanganan diperlukan |
| Tercatat di | Arsitektur bagian 12 keterbatasan nomor 2 |

### `GAP-03` — Validasi perpindahan status kunjungan

| Aspek | Isi |
|---|---|
| Yang tidak tercakup | `RM-CAP-019` berstatus `Repair` dan tidak ditutup |
| Mengapa | Di luar tiga celah yang ditetapkan `RM-DEC-019` |
| Dampak sekarang | Status kunjungan dapat melompat dari nilai mana pun ke `Completed`. Penguncian tetap bekerja karena dipicu tujuan perpindahan, bukan urutannya |
| Kapan harus ditutup | Sebaiknya oleh pemilik `RegistrationManagement`, sebagai task tersendiri |
| Tercatat di | Arsitektur bagian 12 keterbatasan nomor 5 |

### `GAP-04` — `LoggerService.AuditAsync` tetap tidak terpakai

| Aspek | Isi |
|---|---|
| Yang tidak tercakup | `RM-CAP-023` berstatus `Repair`. Metode ada tetapi nol pemanggil |
| Mengapa | `RM-DEC-015` memilih tabel database, bukan log teks. Metode itu karena itu tidak dipakai modul ini |
| Dampak sekarang | Kode mati tetap ada di `Services/Logging/LoggerService.cs:34-37` |
| Kapan harus ditutup | Sebagai pembersihan tersendiri, atau dibiarkan bila modul lain kelak memakainya |
| Catatan | Membiarkan metode bernama `AuditAsync` yang tidak pernah dipakai berpotensi menyesatkan: peninjau bisa mengira audit sudah berjalan |

### `GAP-05` — `SuperAdmin` hanya tertutup di dalam modul ini

| Aspek | Isi |
|---|---|
| Yang tidak tercakup | `RM-CAP-025` ditutup hanya untuk endpoint rekam medis |
| Mengapa | `RM-DEC-017` menyentuh perilaku seluruh aplikasi, sementara security/privacy owner belum ditunjuk. Penerapan sengaja dibatasi agar tidak menyebar tanpa persetujuan |
| Dampak sekarang | **`SuperAdmin` masih dapat membaca data klinis lewat endpoint `ClinicalManagement` yang sudah ada, tanpa jejak akses dan tanpa alasan** |
| Kapan harus ditutup | Setelah security/privacy owner menilai `RM-DEC-017` untuk cakupan penuh |
| Tercatat di | `contracts/permission-audit-matrix.md` bagian 5 |

### `GAP-06` — Dua belas jenis dokumen belum tunduk aturan keutuhan

| Aspek | Isi |
|---|---|
| Yang tidak tercakup | Hanya CPPT yang didaftarkan pada rilis pertama |
| Mengapa | `RM-DEC-019` dan arsitektur bagian 7. CPPT dipilih karena paling sering ditulis dan satu-satunya yang temuannya `Repair` |
| Dampak sekarang | Asesmen, SOAP, diagnosis, tindakan, dan delapan jenis lain **masih dapat diubah tanpa aturan keutuhan** |
| Cara menutup sementara | `RM-FE-009` mewajibkan layar menyatakan cakupan ini terbuka, bukan mendiamkannya |
| Kapan harus ditutup | Rilis berikutnya, satu jenis dokumen per potongan kerja |

---

### `GAP-07` — Penggabungan pasien tidak dapat dipakai dari antarmuka

| Aspek | Isi |
|---|---|
| Yang tidak tercakup | `RM-CAP-033`. Layar pasien menyediakan pilihan "Digabung ke Pasien" tetapi tidak pernah mengirim `mergeReason`, padahal backend mewajibkannya |
| Bukti | `FE .../patient-constants.jsx:120` menyediakan pilihannya; `FE .../patient-editor-utils.jsx:157` mengirim `mergedToPatientId` saja; `BE .../PatientController.cs:2380` menolak tanpa alasan |
| Mengapa tidak ditutup di sini | Ini milik `PatientManagement`, bukan modul rekam medis. Menutupnya lewat roadmap ini melanggar batas scope `RM-DEC-001` |
| Dampak sekarang | Siapa pun yang mencoba menggabungkan pasien lewat antarmuka **selalu menerima galat 400**. Fitur tampak tersedia padahal tidak berfungsi |
| Dampak bagi rekam medis | **Justru menguntungkan sementara ini.** Selama fitur tidak dapat dipakai, tidak ada pasien bernomor ganda baru yang tercipta, sehingga risiko riwayat terpecah tidak bertambah |
| Peringatan | Bila celah ini diperbaiki **sebelum** closure question nomor 8 dijawab, penggabungan menjadi mungkin dilakukan sementara aturan tampilan riwayatnya belum ditetapkan. Urutannya penting |
| Kapan harus ditutup | Diserahkan ke pemilik `PatientManagement`, sebaiknya **setelah** keputusan nomor 8 diambil |

---

## 5. Ringkasan kesiapan

| Pertanyaan | Jawaban |
|---|---|
| Berapa keputusan yang punya task? | 17 dari 19 keputusan yang dapat dikerjakan. Dua sisanya blocker, bukan task |
| Berapa capability audit yang ditutup? | 16 dari 21 yang relevan. Lima menjadi celah pada bagian 4, ditambah `GAP-07` yang milik modul lain |
| Berapa uji yang menggantung tanpa task? | Nol |
| Berapa task yang dapat dimulai hari ini? | **Satu**, yaitu `BE-00` |
| Berapa task frontend yang dapat dimulai? | **Nol.** Seluruhnya menunggu kontrak `APPROVED` |

Angka terakhir adalah kesimpulan paling berguna dari seluruh perencanaan ini. Dari 29 task
backend dan frontend, **satu** dapat berjalan sekarang — dan kebetulan justru task itulah yang
menjadi prasyarat bagi tiga perbaikan paling berisiko di seluruh modul.


---

# Roadmap revision 2 — Traceability amandemen

```yaml
roadmap_revision: 2
blueprint_revision: 4 (approved, RM-DEC-062)
contract_versions: { api: 0.2.0, state_transition: 0.2.0, validation: 0.2.0, integration: 0.2.0, permission_audit: 0.2.0 }   # seluruhnya approved
input_revisions: { interview_decisions: 10, capability_map: 3, requirement_completeness: RM-RCG-001 r2, domain_architecture: RM-DA-001 r1 }
source_commits: { backend: 363de34f, frontend: f0a3b07b8 }
```

## T2.1 Requirement → task → uji

| Requirement | Decision ID | Design | Contract | Backend task | Frontend task | Test | Status |
|---|---|---|---|---|---|---|---|
| `FR-RM-001` Tidak ada jenis kosong diam-diam | `RM-DEC-030`, `058` | A4.9 | API A1 | `BE-21` | `FE-10` | `AT-RM-51` | **Backend selesai** (`BE-21`); `unavailableKinds` baru sampai ke selubung `/timeline` pada `BE-41` (sebelumnya hanya `/summary`); **Frontend selesai** (`FE-10`, 2 November 2026; Laporan `task/report/frontend/FE-10.md`) — keterangan "Belum tersedia di berkas ini" terpisah dari "tidak ada dokumen" |
| `FR-RM-002` Batas waktu per sumber | `RM-DEC-044` | A4.14 | API A1 | `BE-21` | `FE-10` | `AT-RM-50`, `AT-RM-49` | **Backend selesai** (`BE-21`); **Frontend selesai** (`FE-10`, 2 November 2026; Laporan `task/report/frontend/FE-10.md`) — sumber yang melewati batas waktu tampil lewat blok "RIWAYAT INI TIDAK LENGKAP" rilis pertama |
| `FR-RM-003` Pemberitahuan cakupan | — (`RM-CAP-041`) | F4.1 | — | — | `FE-10` | Uji UI | **Frontend selesai** (`FE-10`, 2 November 2026; Laporan `task/report/frontend/FE-10.md`) — kalimat cakupan disusun dari `isIntegrityEnforced` metadata, tidak lagi "hanya CPPT"; diuji terhadap backend nyata |
| `FR-RM-010` Hasil lab PK/Mikro | `RM-DEC-030`–`035`, `057` | A4.10 | `INT-RM-10a` | `BE-23` | `FE-10` | `AT-RM-53`, `AT-RM-51`, `AT-RM-46` | **Backend selesai** (`BE-23`); **Frontend selesai** (`FE-10`, 2 November 2026; Laporan `task/report/frontend/FE-10.md`) — baris bersumber "Laboratorium" terbukti pada backend nyata |
| `FR-RM-011` Laporan radiologi | `RM-DEC-038`, `042` | A4.10 | `INT-RM-10b` | `BE-24` | `FE-10` | `AT-RM-45`, `AT-RM-48` | **Backend selesai** (`BE-24`); **Frontend selesai** (`FE-10`, 2 November 2026; Laporan `task/report/frontend/FE-10.md`) |
| `FR-RM-020`–`022` Sumber pelayanan | `RM-DEC-039`, `041`, `059` | A4.10 | `INT-RM-10c`–`10h` | `BE-29`–`BE-34` | `FE-10` | `AT-RM-44`, `45`, `54` | **Backend selesai** — `BE-29` IGD, `BE-30` Rawat Inap, `BE-31` Kamar Operasi, `BE-32` Hemodialisa, `BE-33` Gizi, dan `BE-34` Bank Darah (`AT-RM-44` keenam modul; `AT-RM-45`, `AT-RM-54`, `AT-RM-61`, `AT-RM-63`, `AT-RM-66` bagian HD dengan pembaca nyata; laporan `task/report/backend/BE-29.md`–`BE-34.md`); **Frontend selesai** (`FE-10`, 2 November 2026; Laporan `task/report/frontend/FE-10.md`) |
| `FR-RM-030` Farmasi | `RM-DEC-039` | A4.10 | `INT-RM-10i` | `BE-35` | `FE-10` | `AT-RM-44` | **Backend selesai** (`BE-35`, 6 uji lulus, laporan `task/report/backend/BE-35.md`); `AT-RM-44` Farmasi terbukti; **Frontend selesai** (`FE-10`, 2 November 2026; Laporan `task/report/frontend/FE-10.md`) |
| `FR-RM-031` Ringkasan berkala | `RM-DEC-052` | A4.10 | API A1 | `BE-22` | `FE-11` | `AT-RM-52` | **Backend selesai** (`BE-22`); **Frontend selesai** (`FE-11`, 2 November 2026; laporan `task/report/frontend/FE-11.md`) — baris dibuka di panel kanan, selisih jumlah dinyatakan, galat rincian tidak menutup linimasa |
| `FR-RM-040` MCU | `RM-DEC-040`, `065`, `066` | A4.10 (dikoreksi) | `INT-RM-10j` | `BE-36` | `FE-10` | `AT-RM-44`, `UAT-09`, `UAT-10` | **Backend selesai** (`BE-36`); **Frontend selesai** (`FE-10`, 2 November 2026; Laporan `task/report/frontend/FE-10.md`). Keterangan "sedang dikoreksi" MCU masih lewat teks ringkasan |
| `FR-RM-050`, `051` Penutupan klinis | `RM-DEC-051` | A4.12 | State S-A | `BE-25` | — | `AT-RM-56`, `57` | **Backend selesai** (`BE-25`, 13 uji lulus); tanpa layar |
| `FR-RM-052` Susulan | `RM-DEC-006`, `043` | A4.9 | API A1 | `BE-26` | `FE-10` | `AT-RM-47`, `58` | **Backend selesai** (`BE-26`, 23 uji lulus, laporan `task/report/backend/BE-26.md`); **Frontend selesai** (`FE-10`, 2 November 2026; Laporan `task/report/frontend/FE-10.md`) — penanda "Susulan" beserta waktu tutup klinis |
| `FR-RM-053` Disposisi IGD mengunci | `RM-DEC-003` | A4.12 | `INT-RM-12` | `BE-27` | — | `AT-RM-59` | **Backend selesai** (`BE-27`, 7 uji lulus, laporan `task/report/backend/BE-27.md`); `AT-RM-59` terbukti |
| `FR-RM-060`–`062` Profil dokumen wajib | `RM-DEC-045`, `047`, `060`, `061` | A4.8 | API A3 | `BE-37` | `FE-13` | `AT-RM-69`, `70`, `71` | **Backend master selesai** (`BE-37`, 17 uji lulus, laporan `task/report/backend/BE-37.md`); `AT-RM-70` terbukti; `AT-RM-71` terbukti lewat `BE-39`, `AT-RM-69` lewat `BE-40`; **Frontend selesai** (`FE-13`, 2 November 2026; laporan `task/report/frontend/FE-13.md`; UAT-14 terbukti terhadap backend nyata, UAT-13 menunggu `FE-14`) |
| `FR-RM-070`–`074` Penilaian dan daftar kerja | `RM-DEC-045`, `046`, `050`, `053`–`056`, `060` | A4.11 | API A2, state S-B | `BE-39` | `FE-14` | `AT-RM-60`–`68` | **Backend selesai** (`BE-39`, 21 uji lulus, laporan `task/report/backend/BE-39.md`); `AT-RM-60`–`68` dan `71` terbukti (pembaca IGD, Rawat Inap, Kamar Operasi, Hemodialisa memakai pembaca tiruan sampai `BE-29`–`BE-32`; `AT-RM-61` dan `AT-RM-63` diulang dengan pembaca nyata `BE-31`; bagian HD `AT-RM-66` dan `AT-RM-63` diulang dengan pembaca nyata `BE-32`); **Frontend selesai** (`FE-14`, 2 November 2026; laporan `task/report/frontend/FE-14.md`; UAT-16 terbukti terhadap backend nyata, UAT-15 dan UAT-17 menunggu penutupan klinis sesudah rilis di devYoga) |
| `FR-RM-075` Pemeriksaan berkala | — (`GAP-ARCH-RM-04`) | A4.13 | — | `BE-40` | — | `AT-RM-69` | **Backend selesai** (`BE-40`, 9 uji lulus, laporan `task/report/backend/BE-40.md`); `AT-RM-69` terbukti |
| Catatan rilis dan keterangan Swagger amandemen | `RM-DEC-062` | A4.9 | API A1–A4 | `BE-41` | — | `ReleaseContractTests` | **Backend selesai** (`BE-41`, 6 uji lulus, laporan `task/report/backend/BE-41.md`); catatan rilis `0.2.0` **menunggu pengesahan** pemilik API |
| `FR-RM-080`, `081` Addendum pengesahan | `RM-DEC-049` | A4.12 | API A4, state S-C | `BE-38` | `FE-12` | `AT-RM-72`–`74` | **Backend selesai** (`BE-38`, 12 uji lulus, laporan `task/report/backend/BE-38.md`); `AT-RM-73`, `74` terbukti; `AT-RM-72` terbukti kecuali "rincian tidak ditandatangani hilang" yang menunggu penilaian `BE-39`; **Frontend selesai** di layar berkas (`FE-12`, 2 November 2026; laporan `task/report/frontend/FE-12.md`; isian awal `RM-DEC-077`) |

## T2.2 Coverage gap

| Celah | Sebab | Penanganan |
|---|---|---|
| `AT-RM-44` untuk IGD, Rawat Inap, Kamar Operasi, Hemodialisa, Gizi, Bank Darah, Farmasi belum dapat dijalankan | Pembacanya tertahan persetujuan pemilik modul — **seluruh persetujuan turun 29 Oktober 2026** | **Tertutup** — terbukti untuk ketujuh modul lewat `BE-29`–`BE-35` |
| AC-50 (disposisi `Draft`/`Confirmed`) tanpa uji backend sampai `BE-29` | **Tertutup** — terbukti uji `BE-29` | Dicatat terbuka |
| AC-43, AC-60 untuk Rawat Inap terbukti hanya dengan pembaca tiruan sampai `BE-30` | **Tertutup** — `AT-RM-60` diulang dengan pembaca nyata `BE-30` | Uji `BE-39` memakai pembaca tiruan; diulang dengan pembaca nyata |
| `ClosureSource` per jalur (`BE-28`) | **Selesai** (`BE-28`, laporan `task/report/backend/BE-28.md`) | Tidak memblokir apa pun |
| Hasil PA | `RM-DEC-058`, open question 18 | Di luar rilis amandemen |
| Indikator mutu kelengkapan | Ditunda `04-prd-to-mvp.md` bagian 8 | `FirstCompletedAt` tersedia |
| Pengesahan dari layar "Catatan Saya Belum Ditandatangani" (outcome `FE-12`) | `/my-unsigned` hanya mengembalikan catatan `Draft`; catatan `LockedUnsigned` hanya terbaca lewat `/my-authored?status=3`, di luar kontrak rekam medis `0.2.0` | **Terbuka** — `RM-DEC-077`: `FE-12` dibatasi ke layar berkas; perlu `/my-authored` masuk kontrak `0.2.1` |
| `unavailableKinds` tidak sampai ke klien `/timeline` | Uji kompatibilitas `BE-21` menserialisasi hasil service, bukan selubung HTTP | **Tertutup** `BE-41` — field dipetakan; `ReleaseContractTests` mengunci kesetaraan field selubung dengan hasil service |
| Kolom status `contracts/api-contract.md` A1–A4 masih `Rencana (belum tersedia)` | Kontrak `0.2.0` terkunci hash di manifest; status tidak diperbarui selama delivery | **Terbuka** — penyegaran status `0.2.1` (bentuk tidak berubah, seperti preseden `0.1.1`) oleh pemilik API; berlaku juga untuk `04-prd-to-mvp.md` dan `03-frontend-architecture.md` |
| Metadata profil dokumen wajib menawarkan jenis kunjungan `Unknown` (`0`, "Tidak diketahui") | `BuildFilterMetadata` menyusun pilihan dari `Enum.GetValues<EncounterType>()`, dan `ValidateFilter` menerimanya sebagai kunci profil; ditemukan saat `FE-13`; metadata daftar kerja `BE-39` sama | **Terbuka** — usul perbaikan `BE-37`: buang `Unknown` dari metadata dan tolak di `ValidateFilter`. Layar `FE-13` menampilkan metadata apa adanya |
| Penyaring `closedFrom`/`closedTo` daftar kerja kelengkapan menjawab `500` untuk tanggal polos `YYYY-MM-DD` | `MedicalRecordCompletenessService.Saring` membandingkan nilai `Unspecified` langsung dengan kolom UTC (`Cannot write DateTime with Kind=Unspecified…`); terbukti terhadap backend nyata saat `FE-14` | **Terbuka** — usul perbaikan `BE-39`: normalkan dengan `AppDateTimeHelper.ToUtc`. Layar `FE-14` mengirim batas hari WIB sebagai UTC ber-`Z`, yang menjawab `200` |
| Parameter halaman daftar kerja kelengkapan bernama `page`, bukan `pageNumber` seperti kontrak A2 | Source `MedicalRecordCompletenessPagedQuery`; ditemukan saat `FE-14` | **Terbuka** — dicatat untuk penyegaran kontrak `0.2.1`. Layar mengirim `page` |

## T2.3 Persetujuan pemilik modul

| Modul | Pemilik | Persetujuan | Task yang tertahan |
|---|---|---|---|
| Laboratorium | Yoga Aji Pratama | **Ya**, `RM-DEC-063` | — |
| MCU | Yoga Aji Pratama | **Ya**, `RM-DEC-063` | — |
| Radiologi | Yoga Aji Pratama (`RAD-DEC-014`) | **Ya**, `RM-DEC-064` | — |
| IGD | Rizki Gunawan (`IGD-DEC-089`) | **Ya**, `RM-DEC-067` | — |
| Rawat Inap | Muhammad Hamzah (`RWI-DEC-061`) | **Ya**, `RM-DEC-069` | — |
| Kamar Operasi | Muhammad Hamzah (blueprint Rawat Inap) dan Ikbal Yuliyanto (registry `Opr`) | **Ya**, `RM-DEC-071` | — |
| Hemodialisa | Muhammad Hamzah (`HMD-DEC-006`) | **Ya**, `RM-DEC-072` | — |
| Gizi | Ikbal Yuliyanto (registry `Gzi`) | **Ya**, `RM-DEC-073` | — |
| Bank Darah | Sukma (pemilik proses BDRS) | **Ya**, `RM-DEC-074` | — |
| Farmasi | Ikbal Yuliyanto | **Ya**, `RM-DEC-075` | — |
| Registrasi | Sukma / Andry | **Ya**, `RM-DEC-068` | — |
| Billing | Yasmina | **Ya**, `RM-DEC-070` | — |

## T2.4 Ringkasan

| Pertanyaan | Jawaban |
|---|---|
| Task backend amandemen | 21 (`BE-21`–`BE-41`) |
| Task frontend amandemen | 6 (`FE-10`–`FE-15`) |
| Dapat dimulai sekarang tanpa menunggu siapa pun | ~~`BE-21`, `BE-25`, `BE-37`, `BE-38`~~ — **seluruh backend amandemen `SELESAI` 2 November 2026**; **seluruh frontend amandemen `FE-10`–`FE-15` `SELESAI` 2 November 2026**. Kesiapan UAT: 7 dari 19 lulus terhadap backend nyata, 12 terbuka menunggu data uji (`testing/uat-amandemen-0.2.0.md`) |
| Tertahan persetujuan modul | **Nol** — `BE-27`–`BE-35` disetujui 29 Oktober 2026 (`RM-DEC-067`–`075`) |
| Keputusan bisnis yang masih memblokir | **Nol** |


---

# Roadmap revision 3 — Traceability amandemen Optik dan Rehab Medik

```yaml
roadmap_revision: 3
blueprint_revision: 5 (approved, RM-DEC-088)
contract_versions: { api: 0.3.0, state_transition: 0.3.0, validation: 0.3.0, integration: 0.3.0, permission_audit: 0.3.0 }   # seluruhnya approved
input_revisions: { interview_decisions: 16, capability_map: 4 }
source_commits: { backend: c7686f07, frontend: 3397211dd }
```

## T3.1 Requirement → task → uji

| Requirement | Decision ID | Design | Contract | Backend task | Frontend task | Test | Status |
|---|---|---|---|---|---|---|---|
| `FR-RM-082` Kunjungan Optik dinilai `RJ-OPT` | `RM-DEC-080` | A5.8, A5.9.1 | API B2 | `BE-42` | — | `AT-RM-75`, `76`, `79`, `80`; `UAT-20` | **Backend selesai** (`BE-42`, 10 November 2026; `AT-RM-75`, `76`, `79`, `80` lulus); `UAT-20` belum |
| `FR-RM-083` Addendum koreksi Optik ditolak | `RM-DEC-084` | A5.9.2 | API B3, `VAL-RM-B01`, state S-E | `BE-43` | — | `AT-RM-91`, `92` | **Backend selesai** (`BE-43`, 10 November 2026; `AT-RM-91`, `92` lulus) |
| `FR-RM-084` Penanda Optik menghitung addendum koreksi | `RM-DEC-084` | A5.9.4 | API B1, `INT-RM-10k` | `BE-44` | — | `AT-RM-77`, `78` | **Backend selesai** (`BE-44`, 10 November 2026; `AT-RM-77`, `78` lulus) |
| `FR-RM-085` Tombol koreksi Optik tidak digambar | `RM-DEC-084` | F5.2 | API B3 | `BE-43` | `FE-16` | `UAT-21` | Backend selesai (`BE-43`); **frontend selesai** (`FE-16`, E2E API tiruan); `UAT-21` terbuka dan terlacak (`RM-DEC-093`, `RM-DEC-096`) |
| `FR-RM-086` Profil berpoli | `RM-DEC-085` | A5.9.1 | API B2, `VAL-RM-B02`–`B04` | `BE-42` | `FE-17` | `AT-RM-81`, `82`, `89`, `90`; `UAT-22` | **Backend selesai** (`BE-42`; `AT-RM-81`, `82`, `89`, `90` lulus — `90` lewat service, index PostgreSQL `NOT RUN`); `FE-17` belum |
| `FR-RM-087` Kunjungan poli rehab | `RM-DEC-081`, `086` | A5.9.3 | `INT-RM-11c` | `BE-45` | — | `AT-RM-83`, `84`; `UAT-23` | **Backend selesai** (`BE-45`, 10 November 2026; `AT-RM-83`, `84` lulus); `UAT-23` terbuka dan terlacak (`RM-DEC-096`, `RM-FACT-037`: master Rehab belum siap) |
| `FR-RM-088` Probe Rehab tanpa hasil tertunda | `RM-DEC-082`, `087` | A5.9.3 | `INT-RM-11c` | `BE-45` | — | `AT-RM-85`, `86` | **Backend selesai** (`BE-45`; `AT-RM-85`, `86` lulus) |
| `FR-RM-089` Penanda Rehab tanpa pengesahan | `DS-R5-04` | A5.9.4 | API B1, `INT-RM-10l` | `BE-45` | — | `AT-RM-87`, `88` | **Backend selesai** (`BE-45`; `AT-RM-87`, `88` lulus) |
| `FR-RM-090` Jenis poli di layar profil | `RM-DEC-085` | F5.2 | API B2 | `BE-42` | `FE-17` | `UAT-22` | Backend selesai (`BE-42`); **frontend selesai** (`FE-17`, E2E API tiruan); **`UAT-22` lulus** 10 November 2026 terhadap backend nyata |
| Jenis 45–52 di linimasa | `RM-DEC-079` | F5.1 | API B1 | — (sudah hidup) | — (sudah hidup) | `UAT-20` | Hidup; `UAT-20` terbuka dan terlacak (`RM-DEC-096`): data Optik/Rehab belum dapat diadakan |
| Catatan rilis `0.3.0` | `RM-DEC-088` | A5.11 | API `0.3.0` | `BE-46` | — | `ReleaseContractTests` | **Selesai** (`BE-46`; 10 uji lulus); catatan rilis menunggu pengesahan |

## T3.2 Coverage gap

| Celah | Sebab | Penanganan |
|---|---|---|
| `RM-AC-R14-04` (sesi rehab bertanda tangan ditolak diubah; koreksi lewat addendum) | Perilaku sudah hidup sejak `BE-RHB-004`, tanpa uji di proyek rekam medis | Dibuktikan `AT-RM-87` di dalam `BE-45` |
| `UAT-23` | Bergantung data poli rehab di master (`RM-UN-R4-01`) | Cek kueri baca-saja sebelum `FE-18`; bila belum ada, dicatat terbuka |
| `AT-RM-77` sisi revisi (versi 2) | Sudah hidup; uji ada di proyek Optik, bukan rekam medis | `BE-44` menjalankan ulang uji Optik |
| Tinjauan komite medik atas pengecualian Optik (open question 24) | Di luar kewenangan kode | Tidak menahan; dicatat terbuka |

## T3.3 Persetujuan pemilik modul

| Folder | Task | Pemilik | Keadaan |
|---|---|---|---|
| `OpticalManagement/Services/` | `BE-44` | Yoga Aji Pratama (pemilik Optik) | **Disetujui** 10 November 2026 (`RM-DEC-089`) |
| `RehabilitationManagement/Services/`, `Program.cs` (pendaftaran probe Rehab) | `BE-45` | Sukma Giri Pratama (pemilik Rehab Medik) | **Disetujui** 10 November 2026 (`RM-DEC-090`, disampaikan Yoga Aji Pratama) |
| Seluruh task lain | `BE-42`, `BE-43`, `BE-46` | Rekam medis (Yoga Aji Pratama) | Tercakup `RM-DEC-088` |

## T3.4 Ringkasan

Delapan task baru: lima backend (`BE-42`–`BE-46`), tiga frontend (`FE-16`–`FE-18`). Siap dimulai
sekarang: **`BE-42`, `BE-43`, `FE-16`, `FE-17`**. Tertahan persetujuan: `BE-44` (Optik),
`BE-45` (Rehab). Seluruh 18 acceptance criteria revision 14/15 terpetakan ke uji.
