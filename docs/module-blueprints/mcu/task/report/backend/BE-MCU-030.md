# Laporan Perubahan Backend — `BE-MCU-030`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-MCU-030` |
| Judul | Resume: draf, approve, kembalikan, verifikasi |
| Slice | `MVP-3` — `EPIC MCU-05` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian *`BE-MCU-030`* |
| Trace | `FR-MCU-040`..`043`; `MCU-DEC-008`, `009`, `018`, `032`, `036`, `041`; `MCU-AC-010`, `011`, `025`; `AT-MCU-38`, `39` (dua bagian pertama), `40` |
| Contract version | `MCU-API-v1` grup Mcu Resume (`GET /`, `PUT current`, `POST current/approve`, `current/return`, `current/verify`); `MCU-STATE-v1` 1.1 dan 3; `MCU-VAL-v1` `VAL-MCU-35`..`40`, `46`; `MCU-PERM-v1` (`McuResume : Read/Update/Approve/Verify`) — seluruhnya `approved` 2026-10-21; hash kontrak cocok dengan manifest |
| Dependency | `BE-MCU-022` ✅, `BE-MCU-023` ✅ |
| Klasifikasi | `HEAVY` — skor 9: berkas diperiksa 9–20 (1), berkas diubah 4–8 (1), logika kompleks (2), memakai kontrak yang sudah ada (1), perilaku persistence yang ada (1), keamanan — pemisahan tugas dokter/petugas (2), workflow terbatas (1) |
| Task mode | `BACKEND` |
| Target tulis | MCU: `McuResumeService.cs`, `McuResumeController.cs`, `McuResumeDtos.cs` (baru); `McuOperationResult.cs`, `McuLabels.cs`; `Program.cs` (satu registrasi). Laporan ini; tanda roadmap/traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `e86d98aa` (branch `yoga`) — tidak bergeser |
| Tanggal | 2026-10-23 |
| Status | Selesai — `AT-MCU-38`, `39` (dua bagian pertama), `40` terbukti di harness integrasi |

### Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `MedicalCheckupManagement / Medical Check-Up`, prefix `Mcu`, registry `ACTIVE` |
| Keberlakuan | `NEW CODE` (service, controller, DTO baru; tanpa entity baru) |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001` (`McuResume`), `QBE-LOG-001` (isi resume dan alasan pengembalian tidak masuk log), `QBE-VAL-001`, `QBE-TXN-001` (resume + order + riwayat satu `SaveChanges`), `QBE-DTO-001` |
| Checker QBE | `PASS` — 5 berkas task nol temuan; kontrol negatif `ZzzKontrol` terkena `QBE-ENT-001`, `QBE-CFG-001`, `QBE-MOD-002`, lalu dihapus. Run pertama sesudah `dotnet test` tidak menangkap kontrol (pola berulang di `BE-MCU-030`..`033`, sebab belum diketahui); run berikutnya selalu menangkapnya |

---

## 1. Masalah yang diperbaiki

Sesudah semua pemeriksaan selesai, order MCU berhenti di *Menunggu Resume*: dokter belum bisa menulis dan
menyetujui kesimpulan, dan petugas belum bisa memverifikasinya. Hasil MCU tidak dapat maju ke penyerahan.

**Contoh.** Thorax Budi belum diekspertise → approve dr. Rina ditolak *"Masih ada 1 komponen yang belum tuntas:
Thorax PA."* Sesudah Thorax selesai, dr. Rina menulis *Sehat, kolesterol tinggi*, kategori *Fit dengan catatan*,
rekomendasi *Kontrol kolesterol 3 bulan lagi*, lalu approve → order *Disetujui Dokter*. dr. Rina mencoba
memverifikasi sendiri → ditolak. Petugas Dewi memverifikasi → *Terverifikasi*; isi tidak berubah.

---

## 2. Proses bisnis

1. **Draf** — dokter MCU yang ditetapkan menyimpan kondisi umum, temuan penting, rekomendasi, kategori kelayakan,
   dan rekomendasi kerja. Draf boleh disimpan selama order *Dalam Pemeriksaan* atau *Menunggu Resume*, berulang kali.
2. **Approve** — dokter yang sama menyetujui. Syarat berurutan:
   1. semua komponen *Selesai* atau *Dibatalkan* (termasuk pemeriksaan tambahan);
   2. skrining perawat selesai;
   3. kategori kelayakan, kondisi umum, dan rekomendasi terisi.

   Hasil: resume *Disetujui Dokter*, order *Disetujui Dokter*, isian Pemeriksaan MCU terkunci (`VAL-MCU-44`).
3. **Petugas MCU** memeriksa kelengkapan administratif lalu memilih satu:
   - **Kembalikan** dengan alasan → resume kembali *Draf* (versi sama), order *Menunggu Resume*; dokter memperbaiki
     dan approve lagi.
   - **Verifikasi** → resume dan order *Terverifikasi*. Petugas tidak mengirim isi apa pun, sehingga tidak dapat
     mengubah isi klinis. Pemverifikasi bukan dokter pemberi approve.
4. Setiap perpindahan order tercatat di riwayat beserta pelaku (dan alasan bila dikembalikan).

**Jalur tidak normal:**

| Keadaan | Akibat |
| --- | --- |
| Komponen belum tuntas | `409` `VAL-MCU-35` *"Masih ada 1 komponen yang belum tuntas: Thorax PA."* (semua nama disebut) |
| Skrining belum selesai | `409` `VAL-MCU-36` *"Skrining awal peserta belum selesai."* |
| Isi minimum kosong | `400` `VAL-MCU-37` *"Lengkapi kategori kelayakan sebelum menyetujui."* — yang pertama kosong disebut: kategori kelayakan, kondisi umum, atau rekomendasi |
| Bukan dokter yang ditetapkan (draf/approve) | `403` `VAL-MCU-38` |
| Dokter pemberi approve memverifikasi | `403` `VAL-MCU-39` *"Verifikasi tidak boleh dilakukan oleh dokter yang menyetujui resume."* |
| Kembalikan tanpa alasan | `400` `VAL-MCU-40` *"Alasan wajib diisi."* |
| Versi order berbeda / dua draf pertama serentak | `409` `VAL-MCU-46` |
| Ubah draf sesudah approve | `409` *"Resume sudah disetujui. Minta dokter membuka ulang resume."* |
| Verifikasi/kembalikan sebelum approve | `409` *"Resume belum disetujui dokter."* |
| Hasil Lab/Rad komponen yang selesai tidak lagi selesai di modulnya (mis. laporan Rad dibuka lagi) | `409` *"Hasil Thorax PA belum tersedia di modulnya."* |
| Order belum dimulai | `409` *"Pemeriksaan belum dimulai."* |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`roadmap/backend-roadmap.md` (`BE-MCU-030`), `contracts/api-contract.md` (grup Mcu Resume), `state-transition-matrix.md`
(1.1, 1.2, 3), `validation-matrix.md` (`VAL-MCU-35`..`46`), `permission-audit-matrix.md`, `testing/acceptance-test-matrix.md`,
`00-interview-decisions.md` (`MCU-DEC-008`, `009`, `018`, `032`, `041`; `MCU-AC-010`, `011`, `025`), `02-backend-architecture.md`
(§4.4, `McuResumeService`, pemisahan tugas, kolom sensitif), `McuResume`, `McuFitnessCategory`, `MedicalCheckupPermissions`,
`McuExaminationService` (kunci hasil `VAL-MCU-44`, skrining, `RefreshComponentsAsync`), `McuHttp`, `McuLabels`,
`InpatientClinicalContextService.ResolveActorDoctorIdAsync`, `Program.cs`, governance `AGENTS.md`, `docs/engineering/`, `rules/backend/`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `MedicalCheckupManagement/Services/McuResumeService.cs` (baru) | `GetAllAsync`, `SaveCurrentAsync`, `ApproveAsync`, `ReturnAsync`, `VerifyAsync` beserta penjaga di atas; satu `SaveChanges` untuk resume, order, dan riwayat |
| `MedicalCheckupManagement/Controllers/McuResumeController.cs` (baru) | Lima endpoint grup Mcu Resume; log tanpa isi resume dan tanpa alasan |
| `MedicalCheckupManagement/DTOs/McuResumeDtos.cs` (baru) | `SaveMcuResumeRequest`, `McuResumeActionRequest`, `ReturnMcuResumeRequest`, `McuResumeResponse` |
| `MedicalCheckupManagement/Services/McuOperationResult.cs` | Kode `VAL-MCU-35`, `37`, `39` |
| `MedicalCheckupManagement/Services/McuLabels.cs` | Label status resume (Draf, Disetujui Dokter, Terverifikasi, Dikoreksi) |
| `Program.cs` | `AddScoped<McuResumeService>()` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Lima endpoint baru sesuai `MCU-API-v1`. `POST reopen` belum ada (`BE-MCU-031`) |
| Database | Tanpa migration (tabel `McuResume` sejak `BE-MCU-002`). **Tidak ada eksekusi ke basis data mana pun** |
| Keamanan/Auth | Draf dan approve: `McuResume : Update/Approve` **dan** akun tertaut ke `McuDoctorId` order. Kembalikan/verifikasi: `McuResume : Verify`, dan pemverifikasi bukan dokter pemberi approve. Isi resume dan alasan pengembalian tidak masuk log aplikasi |

### 3.4 Delta terhadap kontrak dan roadmap

| Butir | Kontrak | Implementasi | Alasan |
| --- | --- | --- | --- |
| Bentuk DTO | Hanya nama (`SaveMcuResumeRequest`, `version`, `McuResumeResponse`) | Ruas sesuai kolom `McuResume`; `McuResumeActionRequest` untuk `version`; response membawa `OrderVersion` | Kontrak tidak merinci ruas; `version` = `Version` order (`VAL-MCU-46`), dikirim balik untuk tindakan berikutnya |
| Verifikasi kelengkapan | *"seluruh komponen Lab/Rad yang tuntas-selesai punya hasil tervalidasi"* | Komponen Lab/Rad *Selesai* wajib pesanannya masih *Completed* di modulnya; pesan *"Hasil … belum tersedia di modulnya."* | Pesan tidak ditetapkan kontrak; *Completed* adalah arti *selesai* yang sama dengan sinkron komponen |
| Approve dari *Dalam Pemeriksaan* | Order *Menunggu Resume* → *Disetujui Dokter* | Bila komponen terakhir tuntas tanpa sinkron, riwayat sistem *Dalam Pemeriksaan → Menunggu Resume* dicatat lebih dulu (1 µs sebelumnya) | Menjaga urutan state machine tanpa memaksa dokter menekan *muat ulang* |
| Data approve sesudah dikembalikan | — | `ApprovedByDoctorId`/`ApprovedAt` tetap berisi persetujuan terakhir; status *Draf* yang menentukan | Jejak persetujuan tidak hilang; isian yang diubah sesudahnya tetap tercatat revisinya (`BE-MCU-023`) |

---

## 4. Dokumentasi endpoint

#### Health Services / Medical Checkup Management / Mcu Resume

Base URL: `/api/v1/health-services/medical-checkup-management/orders/{id}/resumes`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Seluruh versi resume, terbaru di atas | `McuResume : Read` | - | `List<McuResumeResponse>` |
| `PUT` | `/current` | Simpan draf resume berlaku | `McuResume : Update` + dokter yang ditetapkan | `SaveMcuResumeRequest` | `McuResumeResponse` |
| `POST` | `/current/approve` | Approve dokter | `McuResume : Approve` + dokter yang ditetapkan | `McuResumeActionRequest` (version) | `McuResumeResponse` |
| `POST` | `/current/return` | Kembalikan ke dokter | `McuResume : Verify` | `ReturnMcuResumeRequest` (reason, version) | `McuResumeResponse` |
| `POST` | `/current/verify` | Verifikasi administratif | `McuResume : Verify`, bukan pemberi approve | `McuResumeActionRequest` (version) | `McuResumeResponse` |

Kode status: `200`, `400`, `403`, `404`, `409`. Log aplikasi: nomor order, versi resume, nama dokter/petugas.

---

## 5. Verifikasi

Harness uji **sementara** dipulihkan dari salinan `BE-MCU-024` (`Tests/QuilvianSystemBackend.McuTests`, SQLite, service
produksi MCU/Lab/Rad/Billing/Registrasi). Order lahir lewat *walk-in*, dibayar, dimulai; komponen Lab/Rad dituntaskan
dengan menandai pesanannya *Completed* di modulnya lalu `RefreshComponentsAsync`. Harness **dihapus** sesudahnya.

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -p:RunAnalyzers=False` | 0 error | `PASS` | Keluaran build |
| `dotnet test Tests/QuilvianSystemBackend.McuTests` | 24 lulus (11 resume + 13 regresi batal komponen) | `PASS` | `Passed! - Failed: 0, Passed: 24` |
| `AT-MCU-38` | Thorax belum selesai → `409` `VAL-MCU-35` *"Masih ada 1 komponen yang belum tuntas: Thorax PA."*; order tetap *Dalam Pemeriksaan* | `PASS` | `AT_MCU_38_…` |
| `AT-MCU-39` bagian 1 | dr. Rina draf → approve: resume *Disetujui Dokter* oleh dr. Rina, kategori *Fit dengan catatan*, order *Disetujui Dokter*, riwayat pelaku dr. Rina; isian Audiometri terkunci `VAL-MCU-44`; draf tidak dapat diubah lagi | `PASS` | `AT_MCU_39_dokter_yang_ditetapkan_…` |
| `AT-MCU-39` bagian 2 | dr. Bima → draf `403` dan approve `403` `VAL-MCU-38`; order tetap *Menunggu Resume* | `PASS` | `AT_MCU_39_dokter_lain_…` |
| `AT-MCU-40` | dr. Rina memverifikasi → `403` `VAL-MCU-39`; Dewi → *Terverifikasi*, empat ruas isi identik dengan saat approve, order *Terverifikasi*; verifikasi ulang `409` | `PASS` | `AT_MCU_40_…` |
| Kembalikan | Alasan kosong `400` `VAL-MCU-40`; dengan alasan → *Draf* versi 1, riwayat *Disetujui Dokter → Menunggu Resume* beralasan oleh Dewi; dokter mengubah draf lalu approve lagi; tetap satu versi | `PASS` | `Kembalikan_…` |
| Skrining dan isi minimum | Tanpa skrining `409` `VAL-MCU-36`; tanpa rekomendasi `400` *"Lengkapi rekomendasi…"*; tanpa kategori *"Lengkapi kategori kelayakan…"* | `PASS` | `Approve_menunggu_skrining_…` |
| Approve dari *Dalam Pemeriksaan* | Riwayat sistem → *Menunggu Resume* lalu → *Disetujui Dokter*, berurutan | `PASS` | `Approve_dari_Dalam_Pemeriksaan_…` |
| Verifikasi tanpa hasil Rad | Pesanan Rad dibuka lagi → `409` *"Hasil Thorax PA belum tersedia di modulnya."* | `PASS` | `Verifikasi_ditolak_…` |
| Validasi lain | Versi lama `VAL-MCU-46`; kategori tidak dikenal `400`; order belum dimulai `409`; verifikasi sebelum approve `409` | `PASS` | `Versi_lama_…`, `Verifikasi_sebelum_…` |
| Kontrak endpoint | Route, lima template, dan izin `McuResume : Read/Update/Approve/Verify/Verify` | `PASS` | `GET_mengembalikan_…` |
| Uji mutasi | Penjaga `VAL-MCU-35`, `36`, `38`, `39`, dan cek hasil mitra dilepas → 5 uji **gagal** tepat pada penjaganya; dipulihkan identik (`cmp`), kembali 24/24 | `PASS` | Keluaran `dotnet test` |
| Checker QBE `-Path` (5 berkas + kontrol) | Nol temuan; kontrol terkena 3 aturan | `PASS` | Keluaran checker |
| PostgreSQL (index unik versi berlaku, konkurensi), `403` lewat HTTP | Tidak dijalankan; startup ke devYoga menunggu dua migration MCU/Billing | `NOT RUN` | — |

Uji manual: `NOT FEASIBLE` di sesi ini (startup devYoga menunggu migration).

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AT-MCU-38` — approve saat Thorax belum diekspertise `409` menyebut Thorax | Terpenuhi | §5 |
| `AT-MCU-39` (dua bagian pertama) — dokter yang ditetapkan berhasil; dokter lain `403` | Terpenuhi | §5 |
| `AT-MCU-39` bagian ketiga — sesudah ganti dokter | Di luar task (`BE-MCU-032`) | — |
| `AT-MCU-40` — pemberi approve `403`; petugas berhasil tanpa mengubah isi | Terpenuhi | §5 |
| DoD: kriteria terbukti | Terpenuhi | §5 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Checker QBE: run pertama sesudah `dotnet test` tidak menangkap kontrol negatif; run berikutnya selalu menangkapnya. Sebab belum diketahui (berulang sampai `BE-MCU-033`) |
| Masalah yang diketahui | Buka ulang resume belum ada (`BE-MCU-031`); sampai itu, resume yang sudah disetujui hanya dapat diubah lewat **kembalikan** sebelum verifikasi |
| Risiko tersisa | Hak akses `McuResume` mengikuti `SysAccessPolicies` per departemen-posisi; pemberiannya ke jabatan dokter dan petugas MCU di devYoga belum diperiksa. Index unik versi berlaku hanya teruji lewat penanganan kode, bukan PostgreSQL |
| Perubahan sampingan | Folder uji sementara dipulihkan lalu dihapus (salinan diperbarui di scratchpad); kontrol negatif QBE dibuat lalu dihapus |
| Interupsi | `NONE` |
| Status Git | Baru dari task ini: berkas di folder `MedicalCheckupManagement/` (`??`); satu baris `Program.cs` (` M` sejak task sebelumnya); laporan ini; tanda roadmap/traceability |
| Langkah berikutnya | `BE-MCU-031` (buka ulang, versi, revisi nilai isian) siap; `BE-MCU-032` (ganti dokter) siap |
