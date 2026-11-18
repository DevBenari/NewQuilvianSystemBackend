# ISSUE-005 — Isian Pegawai Menampilkan UUID dan Gagal Simpan 404 pada Pengajuan Surat Tugas, Surat Keterangan, dan Dokumen Administrasi

```yaml
issue_id: ISSUE-HRD-005
module_id: human-resource
submodule: null   # blueprint berbentuk SINGLE (HRD-DEC-003); blueprint root = human-resource/
layar: "Administrasi Kepegawaian — Surat Tugas Resmi (/hr/workforce-core/official-duties), Surat Keterangan Resmi (/hr/workforce-core/employee-documents), Dokumen Administrasi (/hr/workforce-core/administrative-documents)"
sumber_laporan: "Laporan pengguna 16-11-2026: 2 butir keluhan + tangkapan layar modal Pengajuan Surat Tugas + salinan respons error POST 404"
tanggal_issue: "2026-11-16"
status: SELESAI
keparahan_tertinggi: High
source_sha_backend: "80c94e8e (MHamzah)"
source_sha_frontend: "ec3616405 (HamzahV2)"
rencana_perbaikan: ../plan-repair/plan-repair-005-penugasan-surat-tugas-workforce-profile-uuid.md
```

---

## 1. Ringkasan

Petugas rumah sakit membuka formulir modal **Pengajuan Surat Tugas Dinas Baru** pada menu *Surat Tugas Resmi*, kemudian mendapati ruas input **Pegawai yang Ditugaskan (WorkforceProfileId)** tampil sebagai isian teks biasa yang terisi kode UUID teknis acak (`0ba84a1a-2559-49ba-a320-10fb1f399d70`), bukan berupa menu pilihan dropdown (*Select Field*) yang menampilkan nama pegawai yang bekerja.

Ketika tombol **Ajukan Surat Tugas** ditekan, pengajuan ditolak mentah-mentah oleh server dengan status **HTTP 404 Not Found** dan pesan galat: `"Data profil tenaga kerja (WorkforceProfile) tidak ditemukan."`.

Penyebab langsungnya adalah kekeliruan pencocokan identitas di frontend: formulir mengambil `currentUser?.workforceProfileId || currentUser?.id`. Bagi akun pengguna administratif (seperti SuperAdmin atau akun sistem), nilai `currentUser.workforceProfileId` kosong, sehingga sistem jatuh ke `currentUser.id` yang merupakan ID tabel akun login (`AspNetUsers`), bukan ID profil tenaga kerja (`MstWorkforceProfile`). Ketika kode akun ini dikirim ke backend, controller memvalidasi keberadaan data di tabel `MstWorkforceProfile` dan gagal menemukannya. Masalah serupa juga ditemukan pada formulir *Surat Keterangan Resmi* dan berimbas langsung pada registri *Dokumen Administrasi*.

Temuan ini melanggar **Aturan Konstitusi Workspace Quilvian**: *"Dilarang menampilkan atau meminta input kode teknis UUID/GUID pada layar pengguna"* serta merupakan cacat fungsional berprioritas tinggi (*High Severity*) karena menghalangi penerbitan naskah dinas resmi rumah sakit.

---

## 2. Laporan Asli

| No. laporan | Keluhan (kata-kata pelapor, diringkas) | Lampiran |
| :---: | :--- | :--- |
| 1 | "Pegawai yang Ditugaskan (WorkforceProfileId) ini seharus ya adalah profil id dari orang ya bekerja bentuk ya dalam bentuk select field masih terjadi eror pada menu ini Ketika melakukan ajukan surat tugas" | Tangkapan layar modal `Pengajuan Surat Tugas Dinas Baru` (16-11-2026), memperlihatkan field teks `WorkforceProfileId` terisi string UUID `0ba84a1a-2559-49ba-a320-10fb1f399d70`. |
| 2 | "Request URL: POST https://localhost:7184/api/v1/corporate/human-resource/attendance-management/official-duties — Status code: 404 Not Found — Response: Data profil tenaga kerja (WorkforceProfile) tidak ditemukan." | Log request JSON payload dan respons server HTTP 404. |
| 3 | "Dari ke-3 menu yang disebutkan masih sama eror ya: Surat Tugas Resmi, Surat Keterangan Resmi, Dokumen Administrasi, dikarenakan (WorkforceProfileId) seharus ya menampilkan employe yang bekerja. Peraturan Keras: Dilarang menampilkan UUID pada tampilan." | Daftar 3 menu rute `/hr/workforce-core/*` yang dilaporkan terdampak masalah identitas pegawai. |

---

## 3. Ringkasan Temuan

| ID | No. laporan | Judul | Jenis | Area | Keparahan | Status bukti | Perbaikan |
| :--- | :---: | :--- | :---: | :---: | :---: | :---: | :--- |
| `ISS-HRD-005-01` | 1, 3 | Field Pegawai yang Ditugaskan menampilkan UUID mentah pada formulir | `RULE_VIOLATION` | Frontend | High | SUDAH-VERIFIKASI | `FIX-HRD-005-01`, `FIX-HRD-005-02` |
| `ISS-HRD-005-02` | 2 | Pengajuan Surat Tugas gagal 404 karena salah mengirim User ID sebagai WorkforceProfileId | `BUG` | Fullstack | High | SUDAH-VERIFIKASI | `FIX-HRD-005-02` |
| `ISS-HRD-005-03` | 3 | Formulir Surat Keterangan Resmi meminta WorkforceProfileId berupa textfield UUID | `RULE_VIOLATION` | Frontend | High | SUDAH-VERIFIKASI | `FIX-HRD-005-03` |
| `ISS-HRD-005-04` | 3 | Registri Dokumen Administrasi rentan kehilangan relasi identitas pemilik dokumen | `BUG` | Frontend | Medium | SUDAH-VERIFIKASI | `FIX-HRD-005-04` |

---

## 4. Rincian per Temuan

### ISS-HRD-005-01 — Field Pegawai yang Ditugaskan menampilkan UUID mentah pada formulir

| | |
| :--- | :--- |
| **No. laporan** | 1, 3 |
| **Jenis** | `RULE_VIOLATION` |
| **Area** | Frontend |
| **Keparahan** | High |
| **Status bukti** | `SUDAH-VERIFIKASI` di source code dan capture pelapor |
| **Perbaikan** | `FIX-HRD-005-01`, `FIX-HRD-005-02` |

**Apa yang terjadi.**
Pada formulir modal *Pengajuan Surat Tugas Dinas Baru*, terdapat kotak isian bertuliskan *"Pegawai yang Ditugaskan (WorkforceProfileId)"*. Kotak ini menampilkan deretan angka dan huruf acak teknis (UUID: `0ba84a1a-2559-49ba-a320-10fb1f399d70`). Pengguna staf administrasi tidak dapat mengetahui siapa pegawai yang sedang dipilih, dan bila ingin menugaskan pegawai lain, pengguna harus mencari dan menyalin UUID pegawai secara manual dari database, yang mustahil dilakukan oleh staf umum.

**Kenapa terjadi.**
Pada berkas `QuilvianSystemFrontendDev/src/components/view/hr/workforce-core/official-duties-view.jsx`:
```jsx
// Baris 171
setCreateForm({
  workforceProfileId: currentUser?.workforceProfileId || currentUser?.id || "",
  ...
});

// Baris 537-550
<BaseInputField
  label="Pegawai yang Ditugaskan (WorkforceProfileId)"
  required
  name="workforceProfileId"
  placeholder="Masukkan ID Profil Pegawai atau gunakan profil aktif"
  value={createForm.workforceProfileId}
  ...
/>
```
Komponen menggunakan `BaseInputField` (input teks biasa) alih-alih `BaseSelectField` yang terhubung ke katalog pegawai.

**Apakah ini menyimpang dari desain?**
Ya, ini berstatus `RULE_VIOLATION` terhadap aturan konstitusi rekayasa Quilvian: *"Dilarang menampilkan pengenal teknis (UUID/GUID) kepada pengguna akhir."* Form harus menyediakan pilihan nama pegawai (`fullName`), nomor induk (`employeeCode`/NIP), beserta jabatan/unitnya.

**Dampak nyata.**
Staf HRD yang ingin menerbitkan surat tugas untuk perawat ambulans tidak dapat memilih nama perawat dari daftar, sehingga alur penerbitan tugas dinas terhenti total.

**Rekomendasi.**
Ganti ruas input menjadi `BaseSelectField` yang mengambil data opsi pegawai dari endpoint master `/api/v1/corporate/human-resource/master-data/employees/options` (resource `workforceProfiles`).

---

### ISS-HRD-005-02 — Pengajuan Surat Tugas gagal 404 karena salah mengirim User ID sebagai WorkforceProfileId

| | |
| :--- | :--- |
| **No. laporan** | 2 |
| **Jenis** | `BUG` |
| **Area** | Frontend & API Integration |
| **Keparahan** | High |
| **Status bukti** | `SUDAH-VERIFIKASI` di backend C# dan frontend JSX |
| **Perbaikan** | `FIX-HRD-005-02` |

**Apa yang terjadi.**
Saat pengguna menekan tombol *"Ajukan Surat Tugas"*, aplikasi mengirim permintaan HTTP POST ke backend, namun server membalas dengan status **404 Not Found** dan pesan: *"Data profil tenaga kerja (WorkforceProfile) tidak ditemukan."*

**Kenapa terjadi.**
Rantai penyebab dapat diverifikasi secara runtut:
1. Di frontend, state awal form mengambil fallback `currentUser?.id`:
   ```javascript
   workforceProfileId: currentUser?.workforceProfileId || currentUser?.id || ""
   ```
2. Ketika login sebagai akun SuperAdmin/Admin, akun tersebut memiliki `id` bertipe GUID di tabel `AspNetUsers` (misalnya `0ba84a1a-2559-49ba-a320-10fb1f399d70`), tetapi akun tersebut **belum tentu memiliki baris profil** di tabel master tenaga kerja `MstWorkforceProfile`.
3. Di backend (`OfficialDutyRequestService.cs` baris 231-239):
   ```csharp
   var workforce = await _context.Set<MstWorkforceProfile>().AsNoTracking()
       .FirstOrDefaultAsync(w => w.Id == request.WorkforceProfileId && !w.IsDelete, cancellationToken);

   if (workforce == null)
   {
       return OfficialDutyRequestServiceResult<OfficialDutyResponse>.Fail(
           StatusCodes.Status404NotFound,
           "Data profil tenaga kerja (WorkforceProfile) tidak ditemukan.");
   }
   ```
   Backend memeriksa apakah ID yang dikirim ada di tabel `MstWorkforceProfile`. Karena yang dikirim adalah User Account ID (`AspNetUsers`), data tidak ditemukan sehingga memicu 404.

**Apakah ini menyimpang dari desain?**
Ya, ini adalah `BUG` integrasi kontrak. Payload `workforceProfileId` wajib diisi dengan primary key tabel `MstWorkforceProfile` (`w.Id`), bukan `SysUser.Id`.

**Dampak nyata.**
Seluruh upaya pengajuan naskah dinas gagal tersimpan, menyebabkan operasional penugasan resmi terhambat.

**Rekomendasi.**
Dengan mengganti kontrol input menjadi dropdown pilihan karyawan yang mengambil nilai `item.workforceProfileId` dari endpoint options karyawan, nilai yang dikirim ke server dipastikan selalu berupa GUID profil tenaga kerja yang valid di database.

---

### ISS-HRD-005-03 — Formulir Surat Keterangan Resmi meminta WorkforceProfileId berupa textfield UUID

| | |
| :--- | :--- |
| **No. laporan** | 3 |
| **Jenis** | `RULE_VIOLATION` |
| **Area** | Frontend |
| **Keparahan** | High |
| **Status bukti** | `SUDAH-VERIFIKASI` di source code |
| **Perbaikan** | `FIX-HRD-005-03` |

**Apa yang terjadi.**
Pada menu *Surat Keterangan Resmi* (`employee-documents-view.jsx`), modal pengajuan permohonan surat keterangan juga memiliki baris input teks *"ID Profil Pegawai Penerima (WorkforceProfileId)"* yang terisi fallback UUID dan meminta input manual.

**Kenapa terjadi.**
Pola yang sama diterapkan di `employee-documents-view.jsx`:
```jsx
// Baris 153
requestedForWorkforceProfileId: currentUser?.workforceProfileId || currentUser?.id || "",

// Baris 520-534
<BaseInputField
  label="ID Profil Pegawai Penerima (WorkforceProfileId)"
  required
  name="requestedForWorkforceProfileId"
  value={requestForm.requestedForWorkforceProfileId}
  ...
/>
```

**Apakah ini menyimpang dari desain?**
Ya, merupakan `RULE_VIOLATION`. Penerbitan surat keterangan resmi rumah sakit (seperti Surat Keterangan Aktif Bekerja atau Keterangan Penghasilan KPR) harus ditujukan kepada pegawai tertentu yang dipilih berdasarkan nama dan NIP, bukan melalui pengetikan UUID manual.

**Dampak nyata.**
Pengajuan surat keterangan resmi gagal tersimpan atau tertuju pada ID akun yang tidak memiliki catatan riwayat kepegawaian dan payroll, sehingga surat keterangan tidak dapat dicetak atau divalidasi QR Code-nya.

**Rekomendasi.**
Ubah field penerima surat menjadi `BaseSelectField` yang menggunakan hook pilihan pegawai (`workforceProfiles`), menampilkan nama lengkap dan departemen pegawai.

---

### ISS-HRD-005-04 — Registri Dokumen Administrasi rentan kehilangan relasi identitas pemilik dokumen

| | |
| :--- | :--- |
| **No. laporan** | 3 |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | Medium |
| **Status bukti** | `SUDAH-VERIFIKASI` di source code |
| **Perbaikan** | `FIX-HRD-005-04` |

**Apa yang terjadi.**
Layar *Dokumen Administrasi* menggabungkan arsip terpadu dari tiga aliran data: SK Kepegawaian, Surat Perintah Tugas, dan Surat Keterangan Resmi. Karena data Surat Tugas dan Surat Keterangan rentan salah mencatat `WorkforceProfileId`, registri arsip terpadu menampilkan pemilik dokumen kosong atau tidak dapat disaring secara akurat berdasarkan nama pegawai.

**Kenapa terjadi.**
Pada `administrative-documents-view.jsx`, pemetaan pemilik dokumen mengandalkan relasi navigasi:
- `st.employeeName` (dari `HrdOfficialDutyRequest.WorkforceProfile.DisplayName`)
- `doc.employeeName` (dari `HrdEmployeeDocumentIssuance.RequestedForWorkforceProfile.DisplayName`)
Ketika `WorkforceProfileId` tidak valid atau tidak terhubung ke entitas yang sah, nama pemilik dokumen tampil sebagai `"-"` atau `"Pegawai"`.

**Apakah ini menyimpang dari desain?**
Ya (`BUG`). Dokumen arsip rumah sakit wajib memiliki ketertelusuran (*traceability*) pemilik yang sah dan terverifikasi.

**Rekomendasi.**
Pastikan input upstream pada Surat Tugas dan Surat Keterangan terkunci pada relasi `WorkforceProfileId` yang valid, dan perkuat pemetaan fallback pada registri dokumen administrasi.

---

## 5. Tanya-Jawab Pelapor

> **T:** Mengapa field Pegawai yang Ditugaskan meminta input UUID dan bukannya nama pegawai?
>
> **J:** Pada implementasi awal, pengembang menurunkan formulir langsung dari struktur properti DTO backend (`Guid WorkforceProfileId`) dan menggunakan input teks biasa sebagai jalan pintas sementara dengan fallback `currentUser.id`. Ini melanggar standar antarmuka Quilvian yang mewajibkan seluruh pengenal teknis digantikan oleh kontrol dropdown (*Select Field*) dengan label nama manusiawi.

> **T:** Mengapa server mengembalikan error 404 "Data profil tenaga kerja (WorkforceProfile) tidak ditemukan"?
>
> **J:** Akun login yang digunakan untuk menguji (misalnya SuperAdmin) hanya terdaftar di tabel akun pengguna sistem (`AspNetUsers`) dan tidak memiliki baris data di tabel master pegawai (`MstWorkforceProfile`). Ketika frontend mengirim User ID tersebut, backend mencari di tabel pegawai dan tidak menemukannya, sehingga memicu penolakan 404.

---

## 6. Temuan Tambahan

### ISS-HRD-005-T1 — Hook `useSelectWorkforceProfiles` belum diekspor pada `use-hr-select.js`

| | |
| :--- | :--- |
| **Kenapa dicantumkan** | Diperlukan sebagai reusable hook resmi agar komponen form tidak memanggil endpoint secara ad-hoc |
| **Area** | Frontend Service/Hook |
| **Keparahan** | Medium |
| **Status bukti** | `SUDAH-VERIFIKASI` di `QuilvianSystemFrontendDev/src/lib/hooks/select/hr/use-hr-select.js` |
| **Perbaikan** | `FIX-HRD-005-01` |

Konfigurasi resource `workforceProfiles` sudah tersedia di `hr-select-resources.js` (lines 188–200) dengan `valueKey: "workforceProfileId"`, namun belum dibuatkan wrapper hook siap pakai `useSelectWorkforceProfiles` di `use-hr-select.js`.

---

## 7. Pertanyaan dan Keputusan yang Dibutuhkan

### Untuk Pelapor
Tidak ada pertanyaan yang menahan perbaikan; laporan sudah jelas dan akar masalah telah diverifikasi 100% pada source code backend dan frontend.

### Untuk Pemilik Sistem

| No | Keputusan | Pilihan | Rekomendasi | Menahan Perbaikan |
| :---: | :--- | :--- | :--- | :---: |
| K-01 | Apakah akun SuperAdmin boleh mengajukan tugas dinas untuk dirinya sendiri bila tidak punya profil tenaga kerja? | **A.** Wajib memilih pegawai aktif yang terdaftar di master data.<br>**B.** SuperAdmin otomatis dibuatkan profil dummy. | **Pilihan A**: Sesuai tata kelola RS, tugas dinas hanya dapat diterbitkan bagi personel yang tercatat pada struktur organisasi RS. | `FIX-HRD-005-02` |

---

## 8. Catatan Pola

Masalah pengetikan atau pemaparan UUID ke layar pengguna adalah pola yang berulang dari `ISSUE-HRD-002` (butir LBT-WFC-26 pada formulir SK Kepegawaian). Pencegahan jangka panjang:
1. Setiap field berakhiran `*Id` pada DTO wajib dipetakan ke komponen `BaseSelectField` dengan opsi nama yang terbaca manusia sebelum diserahkan ke peninjauan.
2. Form validator dilarang mengizinkan pengetikan string format UUID secara manual oleh pengguna akhir.

---

## 9. Riwayat Dokumen

| Tanggal | Perubahan | Oleh |
| :---: | :--- | :---: |
| 2026-11-16 | Dokumen issue dibuat dari laporan keluhan UUID dan galat POST 404 pada menu Surat Tugas, Surat Keterangan, dan Dokumen Administrasi. | `diagnose-module-issue` |
| 2026-11-16 | Perbaikan telah selesai diimplementasikan sesuai PLAN-REPAIR-HRD-005 (FIX-01 s/d FIX-04). Seluruh acceptance criteria terpenuhi. Status issue ditutup: SELESAI. | Antigravity AI |
