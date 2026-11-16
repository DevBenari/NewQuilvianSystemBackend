# Laporan Perubahan Backend — `BE-HRD-338`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-338` |
| Judul | `ShiftRevisionRecorder` dan perbaikan Ganti Petugas |
| Slice | Amandemen `A52` atas `S-B4` Penjadwalan — `EPIC HRD-110`, gelombang `MVP-67` |
| Roadmap | `roadmap/backend-roadmap.md` bagian 24, Grafik B85 |
| Trace | `FR-PK-003`, `FR-PK-004`; `HRD-DEC-195`; `INV-A52-01`, `INV-A52-03`; `02-backend-architecture.md` 35.4.2, 35.9.3; `api-contract.md` 32 `POST /{id}/replace` (**Diubah**); `VR-PK-03`, `-04`, `-05`, `-07`; `INT-HR-105` |
| Contract version | `v26` — `approved` (pengguna, 12 November 2026) |
| Dependency | `BE-HRD-336` 🟡 (cakupan unit), `BE-HRD-337` 🟡 (`ValidateSingleShiftAsync`); tabel `HrdShiftAssignmentRevision` dari `BE-HRD-334` 🟡 |
| Klasifikasi | `MEDIUM` — skor 6: satu repository (0), sekitar 15 berkas diperiksa (1), 7 berkas source (1), transaksi + konkurensi (2), perubahan perilaku endpoint (1), tanpa schema (0), tanpa keamanan baru (0), tanpa UI (0), ditambah 1 untuk integrasi pemberitahuan |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/Corporate/HumanResource/SchedulingManagement/{Services,DTOs,Controllers}/`, `Program.cs`; dokumen roadmap/traceability/laporan |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `9cc8097c` (branch `yoga`) + perubahan `BE-HRD-335`–`337` yang belum di-commit |
| Tanggal | 12 November 2026 |
| Status | 🟡 Sebagian — kode terbukti lewat harness; lewat API devYoga `AT-HRD-PK-01` dan `-04` lulus (13 November 2026). Tersisa `AT-HRD-PK-02` lewat API: devYoga tidak punya pegawai berstatus Didampingi (rantai onboarding kosong) |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `Corporate` |
| Module / Submodule | `HumanResource` / `SchedulingManagement` |
| Pemilik / prefix registry | `Human Resource` / `Hrd` (`ACTIVE / LEGACY`) |
| Keberlakuan | `NEW CODE` untuk `ShiftRevisionRecorder`; `TOUCHED LEGACY` untuk `ShiftReplacementService`, `SchedulingRequestServiceResult`, `ShiftReplacementController` (hanya peneruskan `Errors`), `RosterSchedulingDtos`, `Program.cs` |
| QBE yang berlaku | `QBE-NAM-001`, `QBE-SVC-001` (seluruh aturan dan transaksi di service; controller hanya meneruskan hasil) |
| Sumber governance | `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; registry; `rules/backend/` |

---

## 1. Masalah yang diperbaiki

| Cacat sebelum task ini | Akibat |
| --- | --- |
| Ganti Petugas hanya memeriksa `B1`, `B2`, `B4`, `B5` dengan kode sendiri. Bila status kesiapan **tidak terbaca**, pemeriksaan **dilewati** | Pengganti berstatus Didampingi, sedang tugas belajar, ditempatkan di unit lain, atau yang kesiapannya tidak diketahui tetap dapat ditugaskan (melanggar `INV-A52-03`) |
| Jejak penggantian mencatat `OriginalEmployeeId ?? petugas saat ini` sebagai petugas asal | Pada pergantian kedua (Tono → Andi), Budi tercatat sebagai petugas sebelumnya (contoh 35.9.3) |
| Tidak ada baris Riwayat Revisi | `HRD-DEC-195` butir 3 tidak terpenuhi |
| Dua Kepala Unit yang mengganti petugas bersamaan sama-sama berhasil; yang terakhir menimpa | Perubahan hilang tanpa jejak |
| "Pemberitahuan" hanya teks di respons, tidak benar-benar dikirim | Pegawai tidak tahu jadwalnya berubah |
| Periode roster dipaksa `Published` setiap kali Ganti Petugas | Roster draf ikut berubah status tanpa diterbitkan |

---

## 2. Proses bisnis

**Contoh (35.9.3).** Roster ICU Februari 2027 terbit dengan versi 3.

1. Pukul 19.04 Sari mengganti Budi dengan Tono pada Malam 14 Februari, alasan "Sakit Mendadak".
   Hasilnya satu baris riwayat Budi → Tono. Roster tetap `Published` dengan versi 3. Budi dan
   Tono menerima pemberitahuan.
2. Pukul 20.10 Sari mengganti Tono dengan Andi, alasan "Lainnya — Tono dipanggil siaga IGD".
   Hasilnya baris kedua **Tono** → Andi. Petugas pertama (Budi) tetap tersimpan di
   `OriginalEmployeeId`.

Urutan pemeriksaan sebelum menyimpan:

| Langkah | Ditolak bila | Kode |
| --- | --- | --- |
| Cakupan unit (`BE-HRD-336`) | Unit di luar cakupan kelola | 403 |
| Periode terkunci | Roster `Closed`/terkunci | 403 |
| Alasan (`VR-PK-07`) | Kategori atau catatan kosong | 400 "Alasan perubahan wajib diisi." |
| Petugas yang diharapkan (`VR-PK-05`) | `ExpectedWorkforceProfileId` berbeda dari petugas saat ini | 409 |
| Pemeriksaan penuh (`INV-A52-03`) | Ada blokir `B1`–`B8`. Untuk `B6`, pesannya "Status kesiapan pegawai tidak dapat dibaca. Coba lagi atau hubungi HRD." | 422 + `errors` berisi butir berkode |
| Alasan peringatan (`VR-PK-04`) | Ada peringatan `W1`–`W7` yang kodenya tidak punya alasan di `WarningReasons` | 422 + `errors` berisi peringatan |
| Simpan (satu transaksi) | Petugas di basis data sudah berubah sesaat sebelum disimpan | 409 |

Penyimpanan dilakukan dalam satu transaksi:

1. Ganti petugas lewat pembaruan bersyarat ("hanya bila petugasnya masih Budi").
2. Tulis jejak `TrxShiftReplacement`.
3. Tulis baris `HrdShiftAssignmentRevision`.
4. Commit.

Bila salah satu langkah gagal, semuanya batal. Pemberitahuan dikirim **sesudah commit**, dan
kegagalannya hanya dicatat di log.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `02-backend-architecture.md` 35.4.2, 35.9.3; `HRD-DEC-195`; `validation-matrix.md` `VR-PK-03`–`07`; `acceptance-test-matrix.md` `AT-HRD-PK-01`–`04`; kartu `BE-HRD-338`, `BE-HRD-339`
- `ShiftReplacementService.cs`, `ShiftReplacementController.cs`, `SchedulingRequestServiceResult.cs`, `Responses/ApiResponse.cs`
- `HrdShiftAssignmentRevision.cs`, `TrxShiftAssignment.cs` (tanpa token konkurensi), `TrxShiftReplacement`
- `UnifiedNotificationGatewayService.cs` (sudah menyalurkan ke pusat pemberitahuan sejak remediasi 10 November 2026)
- `Program.cs` (tanpa retry strategy, sehingga transaksi eksplisit aman)

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/SchedulingManagement/Services/ShiftRevisionRecorder.cs` | Baru. `RecordReplacement` dan `Record` hanya menambah baris ke konteks, tanpa `SaveChanges`. Memvalidasi jenis perubahan, rujukan, pelaku, dan alasan wajib serta panjangnya. Tidak ada metode ubah atau hapus |
| `Areas/Corporate/HumanResource/SchedulingManagement/Services/ShiftReplacementService.cs` | `ExecuteDirectReplacementAsync` ditulis ulang sesuai bagian 2. Konstruktor menerima `IRosterValidationService`, `ShiftRevisionRecorder`, `IUnifiedNotificationGatewayService` |
| `Areas/Corporate/HumanResource/SchedulingManagement/Services/SchedulingRequestServiceResult.cs` | `Errors` opsional pada `Fail` (aditif) |
| `Areas/Corporate/HumanResource/SchedulingManagement/Controllers/ShiftReplacementController.cs` | Meneruskan `result.Errors` ke `ApiResponse.Fail` |
| `Areas/Corporate/HumanResource/SchedulingManagement/DTOs/RosterSchedulingDtos.cs` | `DirectShiftReplacementRequest`: `ExpectedWorkforceProfileId` (opsional), `WarningReasons`, `ReasonCategory` maksimal 50 (sama dengan kolom riwayat). `DirectShiftReplacementResponse`: `RevisionId`, `Warnings`, `InfoMessages` |
| `Program.cs` | `AddScoped<ShiftRevisionRecorder>()` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Sesuai `v26` `POST /{id}/replace` (**Diubah**). Tambahan di luar tabel kontrak: `errors` pada 422 dan `infoMessages` pada respons sukses (aditif). `ReasonCategory` diperketat dari 100 menjadi 50 karakter agar muat di kolom riwayat |
| Database | Tanpa migration. Menulis `HrdShiftAssignmentRevision` (tabel `A52`), sehingga butuh migration `BE-HRD-334` diterapkan |
| Keamanan/Auth | Tidak berubah. `Notes`/`ReasonNote` tidak ditulis ke log |

---

## 4. Dokumentasi endpoint

### HR Scheduling - Shift Assignment & Replacement

Base URL: `api/v1/corporate/human-resource/scheduling-management/shift-replacements`

| Method | Path | Hak akses | Request | Response | Kode status |
| --- | --- | --- | --- | --- | --- |
| `POST` | `/{id}/replace` | `ShiftReplacement : Update` | `DirectShiftReplacementRequest` | `DirectShiftReplacementResponse` | 200, 400, 403, 404, 409, 422 |

Contoh request:

```json
{
  "replacementEmployeeId": "…Tono…",
  "expectedWorkforceProfileId": "…Budi…",
  "reasonCategory": "Sakit Mendadak",
  "notes": "Budi menelepon sakit pukul 19.00",
  "warningReasons": [ { "warningCode": "W4", "reasonCategory": "Kekurangan tenaga" } ]
}
```

Contoh 422 (pengganti Didampingi tanpa perawat Mandiri):

```json
{
  "success": false, "statusCode": 422,
  "message": "Ganti Petugas ditolak: VR-SCH-07 (B7): Shift 14-02-2027 memiliki staf Didampingi (Dina) …",
  "errors": [ { "ruleCode": "B7", "severity": "BLOCKING", "staffName": "Dina", "shiftDate": "2027-02-14", "message": "…" } ]
}
```

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -p:RunAnalyzers=False` | 0 galat, 603 peringatan (tetap) | `PASS` | Keluaran build |
| Harness xUnit SQLite di memori (35 uji: 24 sebelumnya + 11 baru) | 35/35 lulus | `PASS` | `dotnet test` di `C:\Users\Admin\AppData\Local\Temp\claude\d823ad68bb5a8f68\ef24ad71-9cc8-48c3-b37f-ddd8b9cac1bd\scratchpad\scopetest` |
| — `AT-HRD-PK-01` + 35.9.3: Budi → Tono → Andi, dua baris riwayat dengan petugas sebelum Budi lalu Tono; roster tetap `Published` versi 3; `OriginalEmployeeId` tetap Budi; jejak `TrxShiftReplacement` Budi, Tono; pemberitahuan ke akun Budi dan Tono | Sesuai | `PASS` | `Budi_Tono_Andi...` |
| — `AT-HRD-PK-02`: pengganti Didampingi → 422, `errors` memuat `B7`, shift tidak berubah, tanpa riwayat | Sesuai | `PASS` | `Pengganti_Didampingi...` |
| — `AT-HRD-PK-03`: kesiapan tidak terbaca → 422 dengan pesan `VR-PK-03`, `B6` di `errors` | Sesuai | `PASS` | `Kesiapan_tidak_terbaca...` |
| — `AT-HRD-PK-04`: (a) `ExpectedWorkforceProfileId` usang → 409; (b) permintaan lain mengganti petugas tepat sebelum pembaruan bersyarat (disela lewat interceptor) → 409, tanpa riwayat, tanpa pemberitahuan | Sesuai | `PASS` | Dua uji |
| — `INV-A52-01`: penyimpanan riwayat dipaksa gagal → pembaruan petugas ikut batal | Sesuai | `PASS` | `Riwayat_gagal_ditulis...` |
| — `VR-PK-04`: peringatan `W4` tanpa alasan → 422; dengan alasan → berhasil, `Warnings` dan `InfoMessages` terisi | Sesuai | `PASS` | `Peringatan_tanpa_alasan...` |
| — Pemberitahuan gagal → Ganti Petugas tetap berhasil dan tercatat | Sesuai | `PASS` | `Pemberitahuan_gagal...` |
| — `VR-PK-07` (dua kasus) dan kompatibilitas tanpa `ExpectedWorkforceProfileId` | Sesuai | `PASS` | Tiga uji |
| Kontrol negatif: riwayat sengaja diisi petugas pertama (cacat lama), backend dibangun ulang | Tepat uji rangkaian Budi → Tono → Andi yang gagal. Kode dikembalikan (dibandingkan byte demi byte), dibangun ulang, 35/35 lulus lagi | `PASS` | Keluaran `dotnet test` |
| `AT-HRD-PK-01` lewat API + basis data devYoga (13 November 2026): roster ICU terbit, Malam 14 Februari 2027, Budi → Tono ("Sakit Mendadak", `ExpectedWorkforceProfileId` = Budi) lalu Tono → Andi ("Lainnya") | Keduanya 200 dengan `revisionId`. Periode tetap `Published` versi 1 sebelum dan sesudah. Riwayat: dua baris `Replacement` (Budi → Tono, lalu Tono → Andi), keduanya bertaut `ShiftReplacementId`. Shift: petugas Andi, `OriginalEmployeeId` Budi, `IsManualOverride` true, sumber `DirectReplacement`. Jejak `TrxShiftReplacement`: Budi → Tono, Tono → Andi, keduanya `Applied` | `PASS` | `at-log.jsonl` |
| `AT-HRD-PK-04` lewat API devYoga: (a) `ExpectedWorkforceProfileId` basi; (b) dua permintaan serentak (`Promise.all`) pada Pagi 17 Februari, Indra → Tono dan Indra → Andi, keduanya `Expected` = Indra | (a) 409 "Shift ini baru saja diubah orang lain. Muat ulang lalu coba lagi.", tanpa riwayat. (b) Satu 200 (Andi), satu 409 (Tono); tepat satu riwayat Indra → Andi; shift berpetugas Andi | `PASS` | `at-log.jsonl` |
| `AT-HRD-PK-02` lewat API devYoga | — | `BLOCKED` | Butuh pegawai berstatus Didampingi. Status itu hanya terbaca dari `HrdClinicalPreceptorship`, yang wajib menunjuk `HrdOnboardingCase` (berstatus `Completed` agar tidak menjadi `B4`), yang wajib menunjuk `HrdCandidateHiringHandoff`. devYoga tidak punya satu pun kasus onboarding, dan memalsukan rantai rekrutmen tidak dilakukan. Bukti tetap dari harness (baris di atas) |
| `AT-HRD-PK-03` | — | `NOT APPLICABLE` lewat API | Matriks menetapkan metode "Unit service" (layanan kesiapan dibuat gagal); bukti dari harness |

Data uji dan perbaikan penghadang uji: lihat [BE-HRD-337](BE-HRD-337.md) bagian 5 dan
[BE-HRD-FIX-001](BE-HRD-FIX-001.md). Pegawai uji tidak punya akun login, sehingga pemberitahuan
ke Budi/Tono tidak terkirim di devYoga; perilaku pemberitahuan terbukti di harness.

Catatan harness: keserentakan diuji dengan menyela perintah `UPDATE` di SQLite. Perlindungan
sebenarnya di PostgreSQL datang dari kunci baris `UPDATE … WHERE "WorkforceProfileId" = …`
pada tingkat `READ COMMITTED`: permintaan kedua menunggu, lalu kondisinya tidak lagi cocok.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Contoh 35.9.3 menghasilkan dua baris dengan petugas sebelum yang benar | Terpenuhi di harness | Bagian 5 |
| Pemberitahuan dikirim sesudah commit dan kegagalannya tidak membatalkan | Terpenuhi di harness | Bagian 5 |
| `AT-HRD-PK-01`–`04` | Terpenuhi di harness; lewat API devYoga `-01` dan `-04` terpenuhi; `-02` **belum** lewat API (data Didampingi tidak dapat disiapkan); `-03` bermetode unit service | Bagian 5 |
| DoD umum: build lulus, tanpa angka tertanam, catatan alasan tidak masuk log | Terpenuhi | Log hanya memuat ID shift, tanggal, dan nama |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada peringatan baru |
| Masalah yang diketahui | (1) **Urutan pemasangan:** Ganti Petugas kini menulis tabel `HrdShiftAssignmentRevision` dan membaca `HrdStudyAssignment`. Jangan dipasang sebelum migration `A52` diterapkan. (2) **Temuan di luar cakupan:** `ApproveReplacementAsync` (`POST /{id}/approve`, jalur permintaan pengganti) masih mengganti petugas tanpa pemeriksaan penuh dan tanpa Riwayat Revisi. Ini melanggar `INV-A52-01`/`03` bila dipakai pada roster terbit. Perlu task tambahan atau diserap ke `BE-HRD-340`/`341` (keputusan pemilik roadmap). (3) Pemberitahuan memakai kategori dan tautan bawaan gateway (kotak persetujuan), karena rute layar Jadwal Saya belum ada (`FE-HRD-31x`) |
| Risiko tersisa | (1) `ExpectedWorkforceProfileId` masih opsional sampai `FE-HRD-307` rilis. Tanpanya, perlindungan tabrakan tetap berlaku terhadap petugas yang dibaca server, tetapi tidak terhadap layar yang sudah basi. (2) Kategori alasan yang lebih dari 50 karakter kini ditolak 400 (sebelumnya 100) |
| Perubahan sampingan | `NONE`. Pemaksaan status `Published` dihapus dengan sengaja (bagian 1) |
| Interupsi | `NONE` |
| Status Git | 7 berkas source task ini (1 baru, 6 diubah) di atas perubahan `BE-HRD-335`–`337` yang belum di-commit; 2 dokumen roadmap/traceability; laporan ini. Tidak ada stage atau commit |
| Langkah berikutnya | (a) `AT-HRD-PK-02` lewat API begitu ada pegawai Didampingi hasil alur onboarding di devYoga, atau pemilik roadmap menerima bukti harness untuk butir ini. (b) `BE-HRD-339`: koreksi mundur dengan riwayat dan pemicu hitung ulang (memakai `ShiftRevisionRecorder.Record`) |
