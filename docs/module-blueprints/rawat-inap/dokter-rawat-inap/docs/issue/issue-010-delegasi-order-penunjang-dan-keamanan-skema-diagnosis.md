# ISSUE-010 — Verifikasi ulang: order penunjang oleh perawat sudah dibangun dan sengaja dikunci sampai uji live; risiko rollback migration diagnosis sudah terdokumentasi

```yaml
issue_id: ISSUE-DOK-010
module_id: rawat-inap
submodule: dokter-rawat-inap
layar: "Ruang Kerja Keperawatan — Penunjang Medis, tab Laboratorium dan Radiologi (FE-RWI-172); migration diagnosis rawat inap (BE-RWI-068)"
sumber_laporan: "Dokumen audit teknis dan catatan sisa pekerjaan 02-11-2026: 2 butir, tanpa screenshot dan tanpa capture database"
tanggal_issue: "2026-11-02"
status: TERBUKA
keparahan_tertinggi: Medium
source_sha_backend: "396c8363 (MHamzah)"
source_sha_frontend: "9c4000724 (HamzahV2)"
rencana_perbaikan: ../plan-repair/plan-repair-010-delegasi-order-penunjang-dan-keamanan-skema-diagnosis.md
```

> **Catatan revisi 2 (2026-11-02).** Versi 1 menyimpulkan bahwa backend belum punya alur pesanan
> perawat atas nama dokter, dan bahwa rollback migration akan merusak database. Kedua kesimpulan itu
> tidak sesuai source. Alur pesanan perawat sudah dibangun pada `BE-RWI-104`. Galat 401 yang dikutip
> berasal dari uji tanpa login. Risiko rollback memang ada, tetapi sudah tertulis lengkap dengan urutan
> aman, dan kegagalannya tidak meninggalkan skema setengah jadi. Berkas
> `InpatientSupportingOrderController.cs` dan `inpatient-doctor-supporting-view.jsx` serta policy
> `DoctorOnly` yang dikutip versi 1 tidak ada di repository.

---

## 1. Ringkasan

Dokumen ini memeriksa dua butir pada sub-modul `dokter-rawat-inap`. Butir pertama: tombol Pesan
Laboratorium dan Radiologi untuk perawat **memang masih terkunci**. Namun penyebabnya **bukan** hak
akses yang belum ada. Fiturnya sudah dibangun dan disetujui pemilik modul Laboratorium dan Radiologi.
Tombolnya sengaja dikunci sampai fitur itu terbukti berjalan pada uji langsung dengan akun perawat dan
dokter sungguhan. Yang dibutuhkan adalah uji langsung tersebut, bukan endpoint baru.

Butir kedua: migration diagnosis rawat inap memang tidak dapat dibalik bila sudah ada diagnosis rawat
inap tanpa nomor konsultasi. Risiko ini sudah tertulis di migration itu sendiri beserta urutan
mundurnya. Perubahan yang diusulkan versi 1 justru berbahaya, sehingga ditarik. Keparahan tertinggi
adalah `Medium`.

---

## 2. Laporan asli

| No. laporan | Keluhan (kata-kata pelapor) | Lampiran yang merujuk | Hasil pemeriksaan lampiran |
| ---: | --- | --- | --- |
| 1 | "Tombol order laboratorium dan radiologi di bangsal tidak bisa diklik oleh perawat karena error 401 Unauthorized. Perawat tidak bisa meneruskan instruksi pemeriksaan darah cito dari dokter." | `Daftar-Pekerjaan-Belum-Selesai.md:96-104` (`FE-RWI-172`), `InpatientSupportingOrderController.cs`, `LabOrderController.cs` | Baris 96-104 ada. `InpatientSupportingOrderController.cs` **tidak ada**. `LabOrderController.cs` ada dan justru memuat alur pesanan perawat |
| 2 | "Migrasi EF Core diagnosis rawat inap belum dieksekusi ke database bersama karena ada peringatan risiko down rollback yang asimetris pada kolom ConsultationId." | `Daftar-Pekerjaan-Belum-Selesai.md:188-194`, `Migrations/20260909065125_AddInpatientEpisodeContextToPatientDiagnosis.cs` | Keduanya ada dan sesuai |

---

## 3. Ringkasan temuan

| ID | No. laporan | Judul | Hasil verifikasi | Jenis | Area | Keparahan | Status bukti | ID perbaikan |
| --- | :---: | --- | --- | --- | --- | :---: | --- | --- |
| `ISS-DOK-010-01` | 1 | Tombol pesan Lab/Rad perawat terkunci | **TERBUKTI SEBAGIAN** — gejalanya benar, akar masalah versi 1 salah | Tidak ada cacat source. Penguncian disengaja sampai ada bukti uji langsung | Frontend (kunci), uji langsung | Medium | `SUDAH-VERIFIKASI` | `FIX-DOK-010-03` (`FIX-DOK-010-01` dibatalkan) |
| `ISS-DOK-010-02` | 2 | Langkah mundur migration diagnosis tidak simetris | **TERBUKTI SEBAGIAN** — risikonya benar, dampak dan solusi versi 1 keliru | Tidak ada cacat source. Risiko terdokumentasi | Migration | Low | `SUDAH-VERIFIKASI`; perilaku transaksi `DUGAAN` kuat | `FIX-DOK-010-02` (dibatalkan) |
| `ISS-DOK-010-T1` | — | Pesanan penunjang dikira tidak punya penanda cito | **TIDAK TERBUKTI** | Tidak ada | — | — | `SUDAH-VERIFIKASI` | Tidak ada |

---

## 4. Rincian per temuan

### ISS-DOK-010-01 — Tombol pesan Lab/Rad perawat terkunci

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Hasil verifikasi** | TERBUKTI SEBAGIAN |
| **Jenis** | Tidak ada cacat source. Kunci tombol adalah gerbang keselamatan yang disengaja |
| **Area** | Frontend (tombol terkunci) dan uji langsung lintas modul |
| **Keparahan** | Medium |
| **Status bukti** | `SUDAH-VERIFIKASI` di source backend, laporan task, dan log keputusan |
| **Perbaikan** | `FIX-DOK-010-03`; `FIX-DOK-010-01` dibatalkan |

**Apa yang terjadi.** Pada malam hari, dokter memberi instruksi lewat telepon untuk memeriksa kalium
pasien. Perawat membuka tab Laboratorium di ruang kerja keperawatan, tetapi tombol **Pesan** tidak dapat
ditekan. Pesanan harus dibuat sendiri oleh dokter yang login.

**Kenapa terjadi.**

1. Alur pesanan perawat atas instruksi dokter **sudah dibangun** pada `BE-RWI-104`. Pemilik
   Laboratorium dan Radiologi menyetujui kolom pemberi instruksi dan status verifikasi pada kedua tabel
   pesanan (`RWI-DEC-153`, `00-interview-decisions.md:2655`).
2. Permintaan pesanan Lab memuat dokter pemberi instruksi —
   `Areas/HealthServices/LaboratoryManagement/DTOs/LabOrderDtos.cs:127-131`:

   ```csharp
   /// Dokter pemberi instruksi — BE-RWI-104, FR-DOK-106. Hanya dibaca pada pesanan rawat inap
   /// (InpEpisodeId terisi) yang dibuat pengguna tanpa tautan dokter: wajib diisi dan wajib
   /// bertugas atas pasien. ...
   ```

3. Pesanan dibuat lewat endpoint pesanan Lab yang biasa, dengan hak akses `LabOrder : Create`
   (`LabOrderController.cs:372-378`). Bila perawat yang memesan, pesanan tercatat **Menunggu
   verifikasi** (`LabOrderService.cs:751`). Bila dokter yang disebut tidak bertugas atas pasien, server
   menolak dengan kode 403 (`LabOrderController.cs:395`).
4. Dokter memverifikasi lewat `PUT .../lab-orders/{id}/verify-instruction` dengan hak akses
   `LabOrder : Verify` (`LabOrderController.cs:420`) dan melihat antreannya lewat
   `GET .../lab-orders/instruction-verification-worklist` (`LabOrderController.cs:454`). Radiologi
   memiliki pasangan yang sama (`RadOrderController.cs:356`, `:370`).
5. Tombol **sengaja** dikunci. Laporan `FE-RWI-172` mencatat: *"Tombol Pesan tetap disabled={true}"*
   (`dokter-rawat-inap/task/report/frontend/FE-RWI-172.md:24`), dan kuncinya dipertahankan sampai
   `BE-RWI-104` terbukti di lingkungan langsung (baris 116-117).
6. Galat 401 yang dikutip versi 1 berasal dari **uji baca tanpa login** (`FE-RWI-172.md:29`, `:48`).
   Setiap endpoint yang bertanda `[Authorize]` menolak permintaan tanpa login dengan 401. Galat itu tidak
   membuktikan bahwa perawat yang sudah login ditolak.
7. Policy `DoctorOnly` yang disebut versi 1 tidak ada di source. Keputusan `RWI-DEC-178` yang dikutip
   versi 1 mengatur **diet**, bukan pesanan Lab/Rad.

**Proses bisnis yang sudah dibangun.**

1. Dokter memberi instruksi kepada perawat lewat telepon atau lisan.
2. Perawat membuat pesanan Lab atau Radiologi dan memilih dokter pemberi instruksi yang bertugas atas
   pasien.
3. Server menyimpan pesanan berstatus verifikasi `Pending`.
4. Dokter pemberi instruksi melihat pesanan itu pada antrean verifikasi, lalu memverifikasinya.

| Dari status verifikasi | Tindakan | Ke status | Siapa yang boleh | Syarat |
| --- | --- | --- | --- | --- |
| — | Perawat membuat pesanan | `Pending` | Perawat dengan `LabOrder : Create` | Dokter pemberi instruksi bertugas atas pasien |
| — | Dokter membuat pesanan sendiri | `NotRequired` | Dokter | — |
| `Pending` | Verifikasi instruksi | Terverifikasi | Dokter pemberi instruksi dengan `LabOrder : Verify` | Pesanan masih `Pending` (`LabOrderService.cs:814`) |
| — | Perawat menyebut dokter yang tidak bertugas | Ditolak (403) | — | — |

**Apakah ini menyimpang dari desain?** Tidak. Backend sesuai `RWI-DEC-153`. Kunci tombol sesuai gerbang
bukti runtime pada kartu `FE-RWI-172`.

**Dampak nyata.** Perawat jaga malam belum dapat memasukkan pesanan cito atas instruksi telepon.
Dokter harus login sendiri, atau pesanan tertunda sampai dokter datang.

**Rekomendasi.** Jangan membuat endpoint `delegated-supporting-orders` atau permission baru seperti
usulan versi 1. Endpoint baru akan menjadi jalur kedua untuk pesanan yang sama. Langkah yang benar:
jalankan uji langsung `BE-RWI-104` dengan akun perawat dan dokter sungguhan, lalu lanjutkan `FE-RWI-172`
untuk membuka tombol dan memasang form.

---

### ISS-DOK-010-02 — Langkah mundur migration diagnosis tidak simetris

| | |
| --- | --- |
| **No. laporan** | 2 |
| **Hasil verifikasi** | TERBUKTI SEBAGIAN |
| **Jenis** | Tidak ada cacat source. Risiko yang sudah terdokumentasi |
| **Area** | Migration database |
| **Keparahan** | Low — tidak menyentuh pengguna; hanya prosedur rollback teknis yang sudah punya urutan aman |
| **Status bukti** | `SUDAH-VERIFIKASI` di berkas migration dan dokumen arsitektur; perilaku transaksi `DUGAAN` kuat |
| **Perbaikan** | `FIX-DOK-010-02` — dibatalkan |

**Apa yang terjadi.** Migration `20260909065125_AddInpatientEpisodeContextToPatientDiagnosis`
membuat diagnosis dapat dicatat per episode rawat inap tanpa nomor konsultasi poliklinik. Caranya,
kolom `ConsultationId` dilonggarkan menjadi boleh kosong. Langkah mundurnya mengembalikan kolom itu
menjadi wajib terisi. Bila sudah ada diagnosis rawat inap tanpa nomor konsultasi, langkah mundur itu
ditolak database.

**Kenapa terjadi (bagian yang benar).**
`Migrations/20260909065125_AddInpatientEpisodeContextToPatientDiagnosis.cs:80-89`:

```csharp
migrationBuilder.AlterColumn<Guid>(
    name: "ConsultationId",
    ...
    nullable: false,
    defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
```

**Bagian yang dilebih-lebihkan versi 1.**

1. **Risikonya sudah tertulis**, lengkap dengan urutan mundur yang aman: pada keterangan migration itu
   sendiri (baris 16-22), pada `dokter-rawat-inap/02-backend-architecture.md` bagian 7.3, dan pada
   `Sisa-Pekerjaan-Rawat-Inap.md` bagian 5.1. Urutannya: kembalikan validasi lebih dulu, tangani baris
   yang telanjur ada bersama pemilik klinis, baru turunkan migration.
2. **Tidak ada "skema korup".** EF Core pada PostgreSQL menjalankan setiap migration di dalam satu
   transaksi, dan PostgreSQL membatalkan perubahan struktur tabel bersama transaksinya. Bila
   `AlterColumn` gagal, `DropForeignKey`, `DropIndex`, dan `DropColumn` sebelumnya (baris 65, 70, 75)
   ikut dibatalkan. Database kembali ke keadaan sebelum rollback. Status klaim ini `DUGAAN` kuat, karena
   ia perilaku pustaka, bukan baris di repository. **Cara membuktikan:** jalankan
   `dotnet ef migrations script 20260909065125_AddInpatientEpisodeContextToPatientDiagnosis <migration-sebelumnya>`
   lalu pastikan perintah-perintahnya diapit `START TRANSACTION;` dan `COMMIT;`.

**Kenapa usulan versi 1 berbahaya.** Kode `Down()` usulan versi 1:

- memakai kolom `"EpisodeId"`, padahal kolomnya bernama `InpEpisodeId`, sehingga perintah SQL-nya
  gagal;
- membuang `DropForeignKey`, `DropIndex`, dan `DropColumn`, sehingga rollback tidak lagi melepas kolom
  `InpEpisodeId`;
- mengubah kolom menjadi `nullable: true` padahal kolomnya memang sudah boleh kosong, sehingga langkah
  mundur tidak mengembalikan apa pun. Akibatnya isi database tidak lagi cocok dengan snapshot model EF.

**Apakah ini menyimpang dari desain?** Tidak. Asimetri ini dirancang dan diterima sejak `BE-RWI-068`.

**Dampak nyata.** Hanya pada skenario rollback darurat setelah diagnosis rawat inap mulai tercatat. Tim
teknis yang mengikuti urutan pada bagian 7.3 tidak akan mengalaminya.

**Rekomendasi.** Tidak mengubah source. Sebelum rollback apa pun, pemilik memeriksa keadaan database
dengan query baca pada plan repair bagian 8, lalu mengikuti urutan bagian 7.3.

---

## 5. Tanya-jawab pelapor

> **T:** Mengapa perawat bangsal tidak langsung diberi peran Dokter saja agar bisa memesan lab?
>
> **J:** Tidak perlu, dan memang tidak boleh. Sistem sudah punya jalur pesanan perawat atas nama dokter
> pemberi instruksi, lengkap dengan verifikasi dokter (`BE-RWI-104`). Yang belum dilakukan hanya uji
> langsungnya.

> **T:** Apakah diagnosis pasien rawat inap boleh berdiri sendiri tanpa kunjungan rawat jalan?
>
> **J:** Ya. Itulah tujuan migration `BE-RWI-068`. Diagnosis rawat inap terikat pada episode
> (`InpEpisodeId`), bukan pada konsultasi poliklinik.

---

## 6. Temuan tambahan

### ISS-DOK-010-T1 — Pesanan penunjang dikira tidak punya penanda cito

| | |
| --- | --- |
| **Hasil verifikasi** | TIDAK TERBUKTI |
| **Kenapa dicantumkan** | Dicantumkan versi 1 sebagai kekurangan form pesanan bangsal |
| **Status bukti** | `SUDAH-VERIFIKASI` |

Kontrak pesanan Lab sudah memuat daftar pemeriksaan cito (`LabOrderDtos.cs:94`, `CitoExaminations`),
dan hasilnya membawa kesegeraan `Routine` atau `Cito` per pemeriksaan (`LabOrderDtos.cs:372-375`). Form
perawat pada `FE-RWI-172` belum dipasang. Saat dipasang nanti, form itu memakai kolom cito yang sudah
ada, tidak perlu kolom `PriorityLevel` baru.

---

## 7. Pertanyaan dan keputusan yang dibutuhkan

**Untuk pelapor:**

| No | Pertanyaan | Kenapa ditanyakan | Dampak bila belum dijawab |
| ---: | --- | --- | --- |
| P-01 | Adakah perawat yang **sudah login** lalu ditolak 401 atau 403 saat membuka tab Laboratorium? Bila ada, sebutkan peran dan waktunya | Bukti 401 yang ada berasal dari uji tanpa login | Butir 1 diperlakukan sebagai kunci yang disengaja |

**Untuk pemilik:**

| No | Keputusan | Pilihan | Rekomendasi | Menahan perbaikan |
| ---: | --- | --- | --- | --- |
| K-01 | Menyediakan lingkungan dan akun uji langsung: satu perawat dan satu dokter yang bertugas atas pasien uji yang sama, beserta petugas Lab | Sediakan / tunda | Sediakan. Pertanyaan ini sudah diajukan pada laporan `FE-RWI-172` dan belum dijawab | `FIX-DOK-010-03` |
| K-02 | Mengesahkan pembatalan `FIX-DOK-010-01` (endpoint dan permission baru) dan `FIX-DOK-010-02` (ubah `Down()`) | Setuju / tolak | Setuju | Penutupan issue |
| K-03 | Apakah Laboratorium boleh memproses sampel pesanan perawat sebelum dokter memverifikasi? | (a) Boleh, verifikasi menyusul; (b) tunggu verifikasi | Pastikan dulu perilaku yang berjalan sekarang lewat uji langsung. Saat ini `DUGAAN`: service sampel Lab tidak memeriksa status verifikasi. Bila perilakunya perlu diubah, putuskan lewat `grill-me` bersama pemilik Lab | Tidak menahan; menjadi bagian skenario uji `FIX-DOK-010-03` |

---

## 8. Catatan pola

Versi 1 menyimpulkan "fitur belum ada" dari kode 401 dan status task 🟡, tanpa membuka controller
pesanan Lab. Pencegahan untuk issue berikutnya:

1. galat 401 berarti belum login, sedangkan 403 berarti tidak berhak — keduanya dibedakan sebelum
   menyimpulkan;
2. status 🟡 dibaca bersama alasan pada laporan task-nya;
3. keputusan yang dikutip (`RWI-DEC-*`) dibuka dan dibaca isinya, bukan hanya nomornya.

---

## 9. Riwayat dokumen

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-11-02 | Versi 1: issue delegasi penunjang dan keselamatan database diterbitkan | AI Agent (`diagnose-module-issue`) |
| 2026-11-02 | Revisi 2: verifikasi ulang terhadap source SHA yang sama. Akar masalah `ISS-DOK-010-01` dikoreksi: delegasi sudah ada (`BE-RWI-104`, `RWI-DEC-153`), 401 berasal dari uji tanpa login, dan kunci tombol disengaja. `ISS-DOK-010-02` dikoreksi: risiko sudah terdokumentasi, kegagalan rollback tidak merusak skema, dan usulan `Down()` versi 1 ditarik karena salah nama kolom serta tidak lagi mengembalikan skema. `ISS-DOK-010-T1` dinyatakan tidak terbukti. Keparahan tertinggi turun dari Blocker ke Medium. P-01 dan K-01 s.d. K-03 ditambahkan | AI Agent (`diagnose-module-issue`) atas permintaan pemilik |
