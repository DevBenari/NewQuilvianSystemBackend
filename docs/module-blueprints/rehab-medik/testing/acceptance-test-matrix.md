# Rehab Medik — Acceptance Test Matrix

| Field | Nilai |
|---|---|
| Blueprint | `RHB-BP-001` r1, `draft` |
| Kebijakan test | Backend tidak memelihara automated test (`rules/backend/TEST_POLICY.md`). Bukti berupa build manual oleh owner, verifikasi kontrak API manual, cek DB, dan verifikasi UI di browser. |
| Jenis test | **API** = permintaan manual ke endpoint dev; **DB** = query cek data; **UI** = langkah di browser; **Statis** = review kode/diff |

| Requirement | Skenario | Jenis | Bukti yang diharapkan |
|---|---|---|---|
| FR-RHB-001 / V-REQ-01..06 | Submit permintaan tanpa indikasi | API, UI | 422 `RHB-VAL-004`, field indikasi ditandai |
| FR-RHB-001 | Submit permintaan lengkap dari kunjungan RJ | API, DB | `RequestStatus = Submitted`, `RequestNumber` format `RHB-REQ-2026-xxxxxx` unik |
| FR-RHB-002 / V-REQ-07 | Submit permintaan fisioterapi untuk pasien yang punya episode fisioterapi `Active`, tanpa alasan | API, UI | 409 `RHB-VAL-007` + kandidat. Dengan alasan → `Submitted`. |
| FR-RHB-003 / RHB-INV-02 | Pengguna dengan `RehabRequest:Return` saja memanggil `/reject` | API | 403 |
| FR-RHB-003 | Sp.KFR menerima permintaan | API, DB | Permintaan `Accepted`, satu `RhbEpisode` `AwaitingMedicalAssessment`, dua baris `RhbStatusHistory` |
| FR-RHB-003 | `accept` pada permintaan `Rejected` | API | 409 `RHB-VAL-020` |
| FR-RHB-004 / RHB-DEC-016 | Lookup kunjungan dengan `patientId` pasien A, lalu simpan permintaan pasien B memakai encounter pasien A | API | Lookup hanya berisi kunjungan A; simpan ditolak 422 `RHB-VAL-002` |
| FR-RHB-010 / RHB-DEC-005 | Aktivasi program sebelum asesmen medis final | API, UI | 422 `RHB-VAL-040`, layar menampilkan daftar prasyarat |
| FR-RHB-011 | Tanda tangan asesmen medis dengan 2 disiplin diresepkan | API, DB | Episode `Planning`, dua `RhbEpisodeDiscipline`, `MrcClinicalDocumentIntegrity` `Signed` kind 47 |
| FR-RHB-012 / V-ASM-02 | Terapis okupasi membuat asesmen fisioterapi | API | 403 `RHB-VAL-031` |
| FR-RHB-013 / RHB-INV-05 | `PUT` asesmen yang sudah final | API | 409 `RHB-VAL-021`, isi tidak berubah |
| FR-RHB-014 / RHB-DEC-019 | Addendum atas asesmen final lewat endpoint RM | API, UI | Addendum tersimpan dengan alasan, catatan asli tetap utuh, dan tampil di berkas RM pasien |
| FR-RHB-015 / RHB-DEC-017 | Asesmen fisioterapi dengan Berg (instrumen disahkan) + ROM bahu (terstruktur) | API, DB | Dua `RhbMeasurement`: satu `Instrument` berskor, satu `Structured` dengan satuan "derajat" |
| FR-RHB-016 | Instrumen konsep (belum disahkan) dipakai di lingkungan non-dev | API | 422 `RHB-VAL-035` |
| FR-RHB-020 / V-PRG-06 | Edukasi `Refused` lalu aktivasi program | API | 422 `RHB-VAL-045` |
| FR-RHB-021 | Revisi program aktif v1 → v2 lalu aktivasi | API, DB | v1 `Superseded`, v2 `Active`, unik aktif per disiplin terjaga |
| FR-RHB-022 / V-GEN-01 | Dua pengguna menyimpan program yang sama dengan `expectedVersion` sama | API | Satu berhasil, satu 409 `RHB-VAL-010` |
| FR-RHB-030 / V-SES-02 | Mulai sesi dengan encounter yang sudah `Completed` | API | 422 `RHB-VAL-051` |
| FR-RHB-031 / V-SES-03 | Mulai sesi RWI untuk pasien yang sudah pulang fisik | API | 422 `RHB-VAL-052` |
| FR-RHB-032 / V-SES-04 | Mulai sesi tanpa verifikasi identitas | API, UI | 422 `RHB-VAL-053`, bagian 2–4 tetap terkunci |
| FR-RHB-033 | Sesi lengkap: mulai → 2 intervensi → selesai → tanda tangan | API, DB, UI | Sesi `Completed`/`Final`, dua `TrxPatientProcedure` (`IdempotencyKey RHB-SESINT-*`), handoff `Succeeded`, item tampil di invoice Billing pasien |
| FR-RHB-034 / RHB-INV-06 | Sesi `NotDone` (tidak hadir) ditandatangani | API, DB | Tidak ada `TrxPatientProcedure`. `BillingHandoffStatus = NotRequired`. |
| FR-RHB-035 | Handoff gagal (tindakan tanpa tarif), lalu tarif diperbaiki dan diulang dua kali | API, DB | `Failed` → `Succeeded`. Ulang kedua = `Replayed`, tidak ada item ganda di invoice. |
| FR-RHB-036 / V-SES-07 | Henti dini tanpa tindakan segera | API | 422 `RHB-VAL-056` |
| FR-RHB-037 | Henti dini dengan "perlu telaah" | API, DB | Program `NeedsReview = true`, sesi `StoppedEarly` |
| FR-RHB-040 / V-MEA-02 | Reasesmen memakai instrumen berbeda tanpa alasan | API | 422 `RHB-VAL-036` |
| FR-RHB-041 | Reasesmen keputusan `Stop` | API, DB | Program `Stopped`, status history tercatat |
| FR-RHB-050 / V-DIS-02 | Tanda tangan ringkasan akhir saat ada sesi `InProgress` | API, UI | 409 `RHB-VAL-060` + daftar |
| FR-RHB-051 | Ringkasan akhir alasan `LostToFollowUp` | API, DB | Episode `Closed` dengan `ClosureReason = LostToFollowUp` (bukan `ClinicalCompletion`) |
| FR-RHB-052 | Membuat dokumen apa pun pada episode `Closed` | API | 409 `RHB-VAL-023` |
| FR-RHB-060 / Privasi | Periksa log aplikasi setelah skenario di atas | Statis, Log | Log hanya memuat id, controller, action, status. Tidak ada indikasi, temuan, respons, maupun diagnosis. |
| FR-RHB-061 / Anti-IDOR | `GET /rehab-sessions/{id}` sesi pasien lain oleh pengguna berhak; `ReadDetailAsync` RM dengan patientId lain | API | Endpoint rehab: data hanya untuk pengguna berizin; pembaca RM mengembalikan 404/null untuk pasien yang tidak cocok |
| FR-RHB-062 / Menu | Pengguna dengan `RehabRequest:Read` saja | UI | Hanya butir "Permintaan Rehab" yang tampil; URL episode langsung → `access-denied-gate` |
| FR-RHB-070 / Konfigurasi | Aktifkan disiplin tanpa profesi | API | 422 `RHB-VAL-070` |
| Migration | Setelah owner apply migration | DB, Build | 20 tabel `Rhb*` ada; `has-pending-model-changes` = tidak ada; `dotnet build` sukses (dijalankan owner) |
