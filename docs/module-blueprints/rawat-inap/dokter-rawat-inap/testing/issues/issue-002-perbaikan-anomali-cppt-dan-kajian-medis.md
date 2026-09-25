# ISSUE-002 — Perbaikan Cacat Linimasa CPPT dari Konsultasi dan Penulis Kajian Medis Rawat Inap

```yaml
issue_id: ISSUE-DOK-002
module_id: rawat-inap
submodule: dokter-rawat-inap
blueprint_id: RWI-BP-001
contract_version: 0.6.1
sumber_temuan: docs/module-blueprints/rawat-inap/dokter-rawat-inap/testing/test-by-agy/laporan-verifikasi-issue-001.md
tanggal_pengujian: "2026-09-23"
tanggal_issue: "2026-09-23"
status: DIKERJAKAN / MENUNGGU VERIFIKASI RUNTIME
prioritas: MAJOR
target_task_backend: BE-RWI-128   # source selesai 23-09-2026; laporan ../../task/report/backend/BE-RWI-128.md
target_task_frontend: FE-RWI-097   # KOREKSI: FE-RWI-096 sudah dipakai sub-modul episode-rawat-inap pada 23-09-2026
verifikasi_source: "2026-09-23 — Dikonfirmasi langsung melalui penelusuran source code backend dan database PostgreSQL"
```

---

## 1. Latar Belakang

Pada 23 September 2026, pengujian runtime untuk verifikasi perbaikan `ISSUE-DOK-001` (`BE-RWI-127` dan `FE-RWI-095`) berhasil membuktikan seluruh perbaikan modul resep telah hijau dan 6 alur regresi klinis berfungsi normal. 

Namun, investigasi mendalam terhadap 3 anomali historis (Bagian C pada dokumen prompter testing) berhasil mereproduksi dan membuktikan keberadaan **2 cacat logika backend aktif** serta **1 ketidakkonsistenan data master**:

1. **C1 (CPPT from Consultation Hilang dari Lini Masa)**: Catatan Terintegrasi (CPPT) yang dibuat dokter dari hasil SOAP konsultasi rawat inap (`POST .../from-consultation/{id}`) berhasil disimpan dengan `HTTP 200 OK`, namun **tidak pernah muncul** pada linimasa riwayat rawat inap pasien karena kolom `InpEpisodeId` bernilai `NULL`.
2. **C2 (Dokter / Penulis Kosong `-` pada Riwayat Kajian Medis)**: Kolom Dokter / Penulis di antarmuka riwayat pengkajian medis selalu menampilkan tanda hubung (`-`) karena pemetaan DTO di backend bergantung pada antrean poliklinik rawat jalan (`x.Queue.Doctor`) yang selalu `null` pada pasien rawat inap.
3. **C3 (Inkonsistensi Kelas Pasien `UNIQUE`)**: Rekord master kelas pasien di database untuk episode aktif memiliki nama literal `"UNIQUE"`, sementara penempatan bed berkelas `"KELAS I"`.

Dokumen ini meresmikan temuan-temuan tersebut ke dalam tiket pelacakan isu rekayasa agar dapat dialokasikan perbaikannya ke task delivery berikutnya.

---

## 2. Daftar Isu dan Rincian Akar Masalah

### ISS-07 — `InpEpisodeId` Bernilai `NULL` saat Pembuatan CPPT dari Konsultasi (`from-consultation`)

| Atribut | Keterangan |
| :--- | :--- |
| **Kode Masalah** | `ISS-07` *(sebelumnya DEFECT-001 / C1)* |
| **Area** | Backend (`PatientIntegratedProgressNoteController.cs`) |
| **Tingkat Keparahan** | **Major** (Integritas Rekam Medis & Keselamatan Pasien) |
| **Status Verifikasi** | **TERBUKTI DI SOURCE & DATABASE** |
| **Gejala Klinis** | Dokter DPJP membuat catatan perkembangan pasien terintegrasi (CPPT) dari lembar konsultasi rawat inap. Sistem merespons `200 OK` dan menerbitkan nomor CPPT (contoh: `CPPT-20260923-0001`), namun catatan tersebut **hilang/tidak tampil** di tab Linimasa CPPT Pasien Rawat Inap (`GET .../episodes/{episodeId}`). Counter linimasa tetap menunjukkan 0 catatan. |

#### Akar Masalah Teknis
Pada file `Areas/HealthServices/ClinicalManagement/Controllers/PatientIntegratedProgressNoteController.cs` (metode `CreateFromConsultation`, baris 692–732):

```csharp
var entity = new TrxPatientIntegratedProgressNote
{
    Id = Guid.NewGuid(),
    ProgressNoteNumber = await GenerateProgressNoteNumberAsync(now),
    PatientId = draft.PatientId,
    EncounterId = draft.EncounterId,
    QueueId = draft.QueueId,
    ConsultationId = draft.ConsultationId,
    AssessmentId = draft.AssessmentId,
    VitalSignId = draft.VitalSignId,
    DoctorId = draft.DoctorId,
    ServiceUnitId = draft.ServiceUnitId,
    ClinicId = draft.ClinicId,
    // CATATAN: InpEpisodeId TIDAK PERNAH DI-ASSIGN DI SINI!
    ...
};
```

Pada blok instansiasi entitas di atas, pengembang lupa memetakan properti `InpEpisodeId`. Akibatnya, `entity.InpEpisodeId` terisi nilai bawaan `NULL` di tabel database `TrxPatientIntegratedProgressNote`.

Sementara itu, endpoint pembaca riwayat CPPT episode rawat inap:
`GET /api/v1/health-services/clinical-management/patient-integrated-progress-notes/episodes/{episodeId}`
menjalankan filter query:
```csharp
query = query.Where(x => x.InpEpisodeId == episodeId);
```
Karena nilai `InpEpisodeId` di database adalah `NULL`, baris CPPT tersebut secara otomatis tersaring keluar (*filtered out*), sehingga dokter tidak dapat melihat riwayat SOAP yang baru saja dibuatnya.

#### Dampak Bisnis & Medikolegal
- **Pelanggaran Kepatuhan Akreditasi RS (STARKES / JCI)**: Catatan perkembangan dokter wajib terintegrasi dan terbaca oleh seluruh PPA (Profesional Pemberi Asuhan) lain (perawat, farmasis, gizi) dalam episode rawat inap yang sama.
- **Risiko Keselamatan Pasien (*Patient Safety*)**: Instruksi medis dan evaluasi SOAP dokter tidak terbaca oleh perawat jaga shift berikutnya, berpotensi memicu kegagalan komunikasi instruksi kritis obat atau tindakan.

#### Rencana Solusi
1. Di `PatientIntegratedProgressNoteController.cs`, perbarui pemanggilan pembentukan entitas agar menyalin `InpEpisodeId` dari `consultation.InpEpisodeId` atau dari `draft.InpEpisodeId`:
   ```csharp
   InpEpisodeId = draft.InpEpisodeId ?? consultation.InpEpisodeId,
   ```
2. Pastikan DTO request atau builder `BuildRequestFromConsultation` juga menyertakan `consultation.InpEpisodeId`.
3. Buat skrip migrasi/perbaikan data (*data fix*) untuk mengisi `InpEpisodeId` pada baris data CPPT historis yang memiliki `ConsultationId` rawat inap namun `InpEpisodeId`-nya masih `NULL`.

---

### ISS-08 — Nama Dokter Pengkaji Bernilai `null` (`-`) pada Riwayat Kajian Medis Rawat Inap

| Atribut | Keterangan |
| :--- | :--- |
| **Kode Masalah** | `ISS-08` *(sebelumnya DEFECT-002 / C2)* |
| **Area** | Backend (`PatientAssessmentController.cs`) |
| **Tingkat Keparahan** | **Minor** (Kerapian & Kepatuhan Tampilan Rekam Medis) |
| **Status Verifikasi** | **TERBUKTI DI SOURCE & DATABASE** |
| **Gejala Klinis** | Pada antarmuka Lembar Kerja Rawat Inap tab Kajian Pasien (sub-tab Riwayat Kajian Medis), kolom **Dokter / Penulis** selalu menampilkan tanda hubung (`-`), meskipun kajian medis diisi dan disahkan oleh DPJP dr. Rendy Pangalila. |

#### Akar Masalah Teknis
Pada file `Areas/HealthServices/ClinicalManagement/Controllers/PatientAssessmentController.cs` (metode `ToResponse`, baris 2934–2935):

```csharp
DoctorId = x.DoctorId,
DoctorName = x.Queue != null && x.Queue.Doctor != null ? x.Queue.Doctor.FullName : null,
```

Pemetaan properti `DoctorName` digantungkan secara kaku pada objek antrean poliklinik rawat jalan (`x.Queue.Doctor`). 
Pada pasien Rawat Inap (*Inpatient*), episode perawatan berlangsung berbasis kamar/bed tanpa melalui entitas antrean harian klinik (`QueueId` bernilai `NULL`). Meskipun nilai `DoctorId` terisi valid dengan ID dokter (misal ID dr. Rendy: `bc389b2c-9b4e-47a7-8a28-98033ef7f97a`), backend tetap mengembalikan `"doctorName": null` pada JSON respons:

```json
{
  "id": "e7c65cba-7a46-4447-975a-0630fc6e680a",
  "doctorId": "bc389b2c-9b4e-47a7-8a28-98033ef7f97a",
  "doctorName": null,
  "assessmentType": 4
}
```
Frontend kemudian merender nilai `null` tersebut sebagai `-`.

#### Dampak Bisnis & Medikolegal
- Dokumen rekam medis wajib mencantumkan nama jelas dan gelar tenaga medis yang melakukan pengkajian klinis awal maupun lanjutan (Permenkes Rekam Medis No. 24 Tahun 2022).
- Tampilan `-` membingungkan perawat dan verifikator rekam medis karena mengesankan dokumen tersebut dibuat tanpa penanggung jawab medis.

#### Rencana Solusi
1. Di `PatientAssessmentController.cs`, ubah pemetaan `DoctorName` pada metode `ToResponse`:
   ```csharp
   DoctorName = x.Doctor != null 
       ? x.Doctor.FullName 
       : (x.Queue != null && x.Queue.Doctor != null ? x.Queue.Doctor.FullName : null),
   ```
2. Pastikan query EF Core pada endpoint `GET /patient-assessments` dan `GET /patient-assessments/{id}` menyertakan `.Include(x => x.Doctor)` selain `.Include(x => x.Queue).ThenInclude(q => q.Doctor)`.

---

### ISS-09 — Standardisasi Master Kelas Pasien (`MstPatientClass`) & Sinkronisasi Kelas Rawat Inap

| Atribut | Keterangan |
| :--- | :--- |
| **Kode Masalah** | `ISS-09` *(sebelumnya Investigasi C3)* |
| **Area** | Master Data Governance & Backend API Contract |
| **Tingkat Keparahan** | **Trivial / Maintenance** |
| **Status Verifikasi** | **TERBUKTI DI DATABASE** |
| **Uraian Masalah** | Di tabel PostgreSQL `InpEpisode.PatientClassId` untuk Tn. Indra Gunawan merujuk ke ID kelas yang memiliki nama `"UNIQUE"` pada tabel master `MstPatientClass`. Di sisi lain, penempatan tempat tidur (`InpBedPlacement.PatientClassId`) merujuk ke `"KELAS I"`. |

#### Rekomendasi Solusi
1. Lakukan audit dan standardisasi master data kelas pasien pada modul Administrator agar tidak ada record kelas dengan penamaan teknis/ujicoba seperti `"UNIQUE"`.
2. Pastikan endpoint ringkasan episode rawat inap secara eksplisit membedakan antara:
   - `CoverageClass` / `EntitledClass` (Hak kelas pertanggungan penjamin/BPJS)
   - `RoomClass` / `BedClass` (Kelas ruang/bed tempat pasien saat ini dirawat)
   agar antarmuka frontend tidak menampilkan teks yang membingungkan staf rumah sakit.

---

## 3. Spesifikasi Endpoint Terkait (Swagger-Style)

### Modul Catatan Perkembangan Pasien Terintegrasi (CPPT)
```csharp
[Tags("Clinical Management - Patient Integrated Progress Notes")]
```
| Metode | Jalur Endpoint | Deskripsi Bisnis | Otorisasi | DTO Request / Response |
| :---: | :--- | :--- | :---: | :--- |
| `POST` | `/api/v1/health-services/clinical-management/patient-integrated-progress-notes/from-consultation/{consultationId}` | Membuat CPPT otomatis dari SOAP konsultasi dokter | DPJP / Dokter | `CreatePatientIntegratedProgressNoteFromConsultationRequest`<br>`PatientIntegratedProgressNoteCreateResponse` |
| `GET` | `/api/v1/health-services/clinical-management/patient-integrated-progress-notes/episodes/{episodeId}` | Mengambil linimasa CPPT seluruh PPA untuk satu episode rawat inap | Dokter / Perawat / Farmasis | `PagedListResponse<PatientIntegratedProgressNoteResponse>` |

### Modul Pengkajian Medis Pasien (Patient Assessment)
```csharp
[Tags("Clinical Management - Patient Assessments")]
```
| Metode | Jalur Endpoint | Deskripsi Bisnis | Otorisasi | DTO Request / Response |
| :---: | :--- | :--- | :---: | :--- |
| `GET` | `/api/v1/health-services/clinical-management/patient-assessments` | Mengambil daftar riwayat kajian medis pasien berdasarkan episode/kunjungan | Dokter / Perawat | `PagedListResponse<PatientAssessmentResponse>` |
| `GET` | `/api/v1/health-services/clinical-management/patient-assessments/{id}` | Mengambil detail lengkap satu kajian medis | Dokter / Perawat | `PatientAssessmentDetailResponse` |

---

## 4. Rencana Alokasi Task Delivery

Untuk menyelesaikan kedua cacat di atas secara terisolasi tanpa mengganggu stabilitas modul resep yang baru saja diperbaiki, dialokasikan rencana task berikut:

| Task ID | Lapisan | Judul Task | Ruang Lingkup Perubahan |
| :--- | :---: | :--- | :--- |
| **`BE-RWI-128`** | Backend | Perbaikan `InpEpisodeId` CPPT from-consultation dan Pemetaan `DoctorName` Kajian Medis | 1. Modifikasi `PatientIntegratedProgressNoteController.cs` (set `InpEpisodeId`).<br>2. Modifikasi `PatientAssessmentController.cs` (include `x.Doctor` & mapping `DoctorName`).<br>3. Unit test & regression test API. |
| **`FE-RWI-096`** | Frontend | Verifikasi UI Linimasa CPPT dan Tampilan Penulis Kajian Medis | 1. Verifikasi kartu CPPT muncul di tab Catatan Terintegrasi setelah dibuat dari SOAP.<br>2. Verifikasi nama dokter muncul di kolom Dokter/Penulis pada tabel riwayat kajian medis.<br>3. Tidak ada perubahan logika frontend jika data dari backend sudah lengkap. |

---

## 5. Kriteria Penerimaan (Acceptance Criteria)

### Kriteria `ISS-07` (CPPT Linimasa Rawat Inap)
- [ ] Pemanggilan `POST /patient-integrated-progress-notes/from-consultation/{consultationId}` menghasilkan baris di tabel `TrxPatientIntegratedProgressNote` dengan nilai `InpEpisodeId` yang terisi sama dengan episode rawat inap konsultasi tersebut (bukan `NULL`).
- [ ] Endpoint `GET /patient-integrated-progress-notes/episodes/{episodeId}` mengembalikan catatan CPPT yang baru dibuat tersebut.
- [ ] Pada antarmuka lembar kerja dokter, kartu CPPT langsung terlihat pada linimasa riwayat catatan pasien dan counter catatan bertambah sesuai jumlah CPPT yang diterbitkan.

### Kriteria `ISS-08` (Dokter Pengkaji pada Kajian Medis)
- [ ] Endpoint `GET /patient-assessments` untuk episode rawat inap mengembalikan field `"doctorName"` yang memuat nama lengkap dokter (contoh: `"dr. Rendy Pangalila"`), bukan `null`.
- [ ] Endpoint `GET /patient-assessments/{id}` mengembalikan `"doctorName"` yang terisi lengkap.
- [ ] Pada antarmuka web lembar kerja dokter tab Kajian Pasien, tabel Riwayat Kajian Medis menampilkan nama lengkap dokter di kolom **Dokter / Penulis** (tidak lagi menampilkan `-`).
---

## 6. Status Penyelesaian — diperbarui 23 September 2026

Dikerjakan pada task **`BE-RWI-128`** — [laporan](../../task/report/backend/BE-RWI-128.md).

| Butir | Lapisan | Status | Keterangan |
| :--- | :---: | :--- | :--- |
| `ISS-07` | Backend | 🟡 **Source selesai, kompilasi PASS** | `InpEpisodeId` diisi pada `CreateFromConsultation`, `BuildRequestFromConsultation`, dan `BuildDraftFromConsultation`. Verifikasi runtime `NOT RUN` |
| `ISS-07` butir 3 | Database | ⚪ **Belum dijalankan** | Skrip perbaikan data historis tersedia: `Migrations/scripts/repair-cppt-inpepisode-from-consultation-20260923.sql`. Eksekusi menunggu wewenang database |
| `ISS-08` | Backend | 🟡 **Source selesai, kompilasi PASS** | `DoctorName` diturunkan dari `DoctorId` kajian lewat pembacaan `MstDoctor`, dengan antrean sebagai cadangan. Verifikasi runtime `NOT RUN` |
| `ISS-09` | Master data + kontrak | ⛔ **Tidak dikerjakan** | Di luar kepemilikan sub-modul ini; lihat alasan di bawah |
| Pasangan frontend | Frontend | — **Tidak ada perubahan source** | Layar sudah merender `doctorName` dengan fallback `-` dan sudah memanggil lini masa CPPT satu perawatan. Yang tersisa murni verifikasi tampilan |

### 6.1 Penyimpangan dari rencana solusi yang tertulis di atas

**`ISS-07` butir 1.** Rencana menulis `InpEpisodeId = draft.InpEpisodeId ?? consultation.InpEpisodeId`.
Yang dikerjakan menambahkan satu penurunan lagi: bila konsultasinya belum distempel perawatan —
keadaan yang mungkin pada konsultasi lama yang lahir sebelum `BE-RWI-043` — perawatan diturunkan
dari kunjungannya lewat `FindOpenEpisodeIdAsync`, sumber yang sama dengan yang dipakai
`CreateProgressNote`. Tanpa langkah kedua ini, konsultasi lama tetap melahirkan CPPT yang hilang
dari lembar terpadu.

**`ISS-08` butir 2.** Rencana meminta `.Include(x => x.Doctor)` pada query kajian. **Navigasi itu
tidak ada** — `TrxPatientAssessment` hanya punya kolom `DoctorId`, tanpa navigation property ke
`MstDoctor`. Menambahkannya akan melahirkan foreign key baru dan menuntut migration, untuk sesuatu
yang dapat dijawab satu pembacaan `MstDoctor` per halaman. Yang dikerjakan karena itu adalah
pembacaan terkumpul (`ResolveDoctorNamesAsync`), bukan `Include` — tanpa satu pun perubahan schema.

### 6.2 Alasan `ISS-09` tidak dikerjakan

1. Butir 1-nya adalah pembersihan **data induk** milik modul Administrator, yaitu record kelas pasien
   bernama `"UNIQUE"`. Itu eksekusi database, bukan perubahan source, dan memerlukan wewenang
   terpisah.
2. Butir 2-nya menyentuh kontrak DTO milik sub-modul **`episode-rawat-inap`**, bukan
   `dokter-rawat-inap`.
3. Dokumen ini sendiri tidak mengalokasikan task maupun acceptance criteria untuk `ISS-09` — bagian
   4 dan bagian 5 hanya memuat `ISS-07` dan `ISS-08`.

**Temuan yang perlu diketahui pemiliknya.** Kontrak episode rawat inap **sudah** memisahkan kedua
kelas itu secara struktur: kelas hak penjamin ada pada `InpatientEpisodeDetailResponse`, kelas bed
ada pada `InpatientEpisodeCurrentLocationResponse`. Yang belum ada adalah pembedaan **nama ruasnya**
— keduanya sama-sama bernama `PatientClassName`, dan itulah yang membuat layar terbaca
membingungkan. Pekerjaan tersisa karena itu pembersihan data induk ditambah penamaan ulang ruas.

### 6.3 Dua koreksi pada dokumen ini

1. **Nama tabel.** Tertulis `CliPatientIntegratedProgressNote`; nama sebenarnya
   `TrxPatientIntegratedProgressNote`. Sudah dibetulkan di badan dokumen.
2. **ID task frontend.** `FE-RWI-096` **sudah dipakai** sub-modul `episode-rawat-inap` pada tanggal
   yang sama — laporannya ada di
   `docs/module-blueprints/rawat-inap/episode-rawat-inap/task/report/frontend/FE-RWI-096.md`, dan
   roadmap frontend sub-modul itu mencatat `task_id_next_free: FE-RWI-097`. ID frontend yang benar
   untuk isu ini karena itu **`FE-RWI-097`**. Sudah dibetulkan pada metadata di atas; **bagian 4
   dibiarkan apa adanya sebagai catatan sejarah**.

### 6.4 Bukti verifikasi yang sudah ada

`dotnet msbuild QuilvianSystemBackend.csproj -t:Compile -p:Configuration=Debug` — exit code `0`,
**`0 error`**, `224 warning`. Dari 224 warning itu hanya **satu** yang berada pada berkas yang
disunting, yaitu `CS8602` pada `PatientAssessmentController.cs` baris `2973` di dalam
`BuildBaseQuery` — baris yang **tidak disentuh task ini** dan hanya bergeser nomornya. Dengan kata
lain **tidak ada warning baru**. Target `Compile` sengaja dipakai agar kompilasi terbukti tanpa
menyentuh `bin`.

`dotnet build` penuh **sudah dicoba** dan berhenti bukan karena kode: kompilasinya `0 error CS`,
tetapi penyalinan ke `bin` ditolak dengan `MSB3027`/`MSB3021` karena
`bin\Debug
et9.0\QuilvianSystemBackend.exe` sedang dikunci proses
`QuilvianSystemBackend (PID 6608)` yang berjalan. Diklasifikasikan
`EXISTING / ENVIRONMENT ISSUE`, sama seperti yang dialami `BE-RWI-127`.

Review penutup `REVIEW_RULES` dijalankan dan bersih: diff hanya memuat perilaku yang diminta, tidak
ada berkas project/konfigurasi/workflow/dependency yang tersentuh, `EnrichDetailAsync` diperiksa dan
tidak menimpa `DoctorName`, tidak ada rahasia pada berkas yang berubah, dan skrip SQL terbukti hanya
memuat `1 BEGIN`, `1 COMMIT`, `8 SELECT`, `2 UPDATE` — nol `DROP`, `DELETE`, maupun `TRUNCATE`.

### 6.5 Yang masih perlu dijalankan pemilik

1. Hentikan proses `QuilvianSystemBackend (PID 6608)`, lalu jalankan `dotnet build` sampai hijau.
2. Jalankan ulang skenario: terbitkan CPPT dari SOAP konsultasi rawat inap, lalu buka tab Catatan
   Terintegrasi — kartunya harus muncul dan penghitungnya bertambah.
3. Buka tab Kajian Pasien sub-tab Riwayat Kajian Medis — kolom Dokter / Penulis harus memuat nama
   dokter, bukan tanda hubung.
4. Jalankan `Migrations/scripts/repair-cppt-inpepisode-from-consultation-20260923.sql`, lalu periksa
   baris `sesudah` pada keluarannya — sisa baris yang masih kosong harus habis dijelaskan oleh dua
   kolom alasan di sebelah kanannya.
