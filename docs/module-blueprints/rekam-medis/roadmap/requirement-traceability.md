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
| Perlakuan catatan lama | `RM-DEC-014` | `02-backend-architecture.md#82-rencana-migration` | — | `BE-08` | — | `AT-RM-21`, `AT-RM-33`, `AT-RM-43` | Planned |
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
