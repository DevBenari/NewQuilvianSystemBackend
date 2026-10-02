# ISSUE-003 — Perbaikan Cacat Layanan Penunjang, Resume Medis, Tarif Resep, dan Otoritas CPPT Dokter Rawat Inap

```yaml
issue_id: ISSUE-DOK-003
module_id: rawat-inap
submodule: dokter-rawat-inap
blueprint_id: RWI-BP-001
contract_version: 0.6.1
sumber_temuan: docs/module-blueprints/rawat-inap/dokter-rawat-inap/testing/test-by-agy/laporan-testing-siklus-lengkap-dokter-rawat-inap.md
tanggal_pengujian: "2026-09-24"
tanggal_issue: "2026-09-24"
status: SELESAI_DIVERIFIKASI   # Seluruh butir ISS-09 s.d. ISS-13 telah diperbaiki dan lulus pengujian runtime
prioritas: HIGH / CRITICAL
verifikasi_source: "2026-09-24 — Diverifikasi langsung melalui unit test, database PostgreSQL, dan live E2E Playwright"
```

---

## 1. Latar Belakang

Pada 24 September 2026, dilakukan pengujian operasional langsung (*Live End-to-End Testing*) pada sub-modul **Dokter Rawat Inap** menggunakan pasien aktif Tn. Indra Gunawan (No RM `00-00-00-16`, Episode `c3fe1370-18f0-42fb-8d9f-01449212828e`) dan DPJP dr. Rendy Pangalila.

Meskipun alur konsultasi dasar berjalan baik, pengujian menyeluruh pada 8 tab klinis dan 2 submenu berhasil mengidentifikasi **5 anomali teknis dan integritas data** yang memblokir kelancaran operasional rumah sakit:

1. **ISS-09 (Defect HTTP 403 Layanan Hemodialisa pada Tab Penunjang Medis):** Tab Penunjang Medis mengirimkan permintaan jaringan ke endpoint `/hemodialysis-orders` yang ditolak HTTP 403 Forbidden karena melanggar keputusan arsitektur `RWI-DEC-108` dan acceptance criteria `FE-RWI-076`.
2. **ISS-10 (Galat HTTP 404 Sidebar Profil Dokter):** Pemanggilan rute API usang `/UserActive/UserActiveDoctors/...` pada sidebar profil mengotori log konsol browser.
3. **ISS-11 (Defect HTTP 500 Pembuatan Resep Obat Rawat Inap):** Permintaan peresepan obat melempar unhandled exception HTTP 500 karena kelas perawatan pasien tertaut ke ID kelas `"UNIQUE"` yang tidak memiliki matriks tarif aktif.
4. **ISS-12 (Galat HTTP 404 Tab Riwayat Revisi Resume Medis):** Tab riwayat revisi pada Resume Medis memanggil endpoint tidak terdaftar `/summary-revisions`.
5. **ISS-13 (Defect HTTP 403 Pencatatan CPPT Perawat oleh Guard Profesi):** Perawat Mira Safitri ditolak membuat catatan asuhan harian CPPT (`HTTP 403`) karena data pegawai pada database belum memiliki tautan `ProfessionId` klinis dan penempatan instalasi rawat inap.

Seluruh isu di atas telah **diperbaiki secara tuntas dan diverifikasi hijau** dalam siklus pengujian ini.

---

## 2. Rincian Masalah, Akar Penyebab, Solusi, dan Bukti Verifikasi

---

### ISS-09 — Galat 403 Layanan Hemodialisa pada Tab Penunjang Medis (Pelanggaran RWI-DEC-108)

| Parameter | Keterangan |
| :--- | :--- |
| **Kode Isu** | `ISS-09` |
| **Area Terdampak** | Frontend (`inpatient-supporting-service-constants.jsx`, `use-inpatient-supporting-service.jsx`) |
| **Tingkat Keparahan** | **Major** (Kepatuhan Arsitektur & Integritas Jaringan) |
| **Status** | 🟢 **SELESAI DIPERBAIKI & TERVERIFIKASI** |

#### 1. Gejala Klinis / Operasional
Ketika dokter membuka Tab **Penunjang Medis**, konsol browser mencatat galat:
```text
[API FAILED 403] GET https://localhost:7184/api/v1/health-services/hemodialysis-management/hemodialysis-orders?inpEpisodeId=...
{"success":false,"statusCode":403,"message":"Anda tidak memiliki akses ke menu atau fitur ini."}
```
Layar menampilkan peringatan kegagalan pemuatan, dan unit test `inpatient-supporting-service-v2.test.mjs` gagal dengan pesan:
`AssertionError: hanya Laboratorium dan Radiologi yang backend-nya tersedia (3 !== 2)`.

#### 2. Akar Masalah Teknis
Sesuai keputusan bisnis `RWI-DEC-108` dan `RWI-DEC-113`, dari 6 layanan penunjang (Laboratorium, Radiologi, Gizi, Rehab Medik, Hemodialisa, Bank Darah), hanya Laboratorium dan Radiologi yang backend-nya terintegrasi pada rilis ini. Empat layanan lainnya wajib berstatus *"Integrasi belum tersedia"* dengan **nol pemanggilan jaringan (*zero network overhead*)**.
Namun pada `src/lib/constants/health-services/inpatient-management/inpatient-supporting-service-constants.jsx`, konfigurasi hemodialisa tertulis:
```javascript
// SEBELUM PERBAIKAN:
{
  key: "hemodialysis",
  badgeLabel: "Tersedia",
  isAvailable: true, // <--- KELIRU
}
```
Hal ini menyebabkan hook `use-inpatient-supporting-service.jsx` menganggap layanan aktif dan mengirimkan request ke backend hemodialisis, yang kemudian ditolak otorisasi peran dokter umum.

#### 3. Solusi Penerapan (*Fix Applied*)
1. Memperbarui `inpatient-supporting-service-constants.jsx` agar nilai `isAvailable: false` dan `badgeLabel: "Integrasi belum tersedia"`:
```javascript
// SESUDAH PERBAIKAN:
{
  key: "hemodialysis",
  title: "Hemodialisa",
  shortTitle: "Hemodialisa",
  badgeLabel: "Integrasi belum tersedia",
  isAvailable: false,
  description: "Pelayanan hemodialisis rutin dan cito rawat inap, pemantauan adekuasi dialisis, dan akses vaskular.",
  routeKey: "hemodialysis",
}
```
2. Memastikan penjaga `isHmdAvailable` pada `use-inpatient-supporting-service.jsx` membypass pemanggilan jaringan ketika `isAvailable: false`.

#### 4. Bukti Verifikasi
- Unit test `node --test tests/unit/inpatient-supporting-service-v2.test.mjs tests/unit/inpatient-supporting-service-parity.test.mjs`: **15 Lulus, 0 Gagal (100% Pass)**.
- Traversal Live E2E: Kartu Hemodialisa menampilkan panel *"Integrasi belum tersedia"* dengan konteks pasien Tn. Indra Gunawan dan 0 pemanggilan request jaringan.

---

### ISS-10 — Galat 404 Endpoint Usang pada Sidebar Profil Dokter

| Parameter | Keterangan |
| :--- | :--- |
| **Kode Isu** | `ISS-10` |
| **Area Terdampak** | Frontend (`user-profile-sidebar.jsx`) |
| **Tingkat Keparahan** | **Minor** (Kebersihan Log & Stabilitas Komponen) |
| **Status** | 🟢 **SELESAI DIPERBAIKI & TERVERIFIKASI** |

#### 1. Gejala Klinis / Operasional
Konsol browser selalu mencatat galat HTTP 404 setiap kali dokter masuk ke ruang kerja rawat inap:
```text
GET https://localhost:7184/api/v1/UserActive/UserActiveDoctors/... 404 (Not Found)
```

#### 2. Akar Masalah Teknis
Berkas `src/components/view/settings/sidebar-profile/user-profile-sidebar.jsx` masih mempertahankan kode legacy V1 yang mencoba mengambil profil dokter secara langsung dari rute controller lawas yang sudah dihapus dari backend ASP.NET Core.

#### 3. Solusi Penerapan (*Fix Applied*)
Menghapus pemanggilan API eksternal tersebut dari `user-profile-sidebar.jsx` dan mengarahkan pembacaan identitas dokter secara aman ke data sesi aktif Redux (`dataDokter` / `currentUser`).

#### 4. Bukti Verifikasi
Konsol browser saat pengujian E2E bebas dari galat 404 sidebar.

---

### ISS-11 — Defect HTTP 500 Pembuatan Resep Obat Rawat Inap Akibat Inkonsistensi Kelas Pasien

| Parameter | Keterangan |
| :--- | :--- |
| **Kode Isu** | `ISS-11` |
| **Area Terdampak** | Database & Integrasi Tarif Farmasi (`RegPatientEncounter`, `InpEpisode`) |
| **Tingkat Keparahan** | **Critical** (Pelayanan Terapi Pasien Terhenti) |
| **Status** | 🟢 **SELESAI DIPERBAIKI & TERVERIFIKASI** |

#### 1. Gejala Klinis / Operasional
Dokter DPJP meresepkan obat rawat inap (3 HP / Isoniazid-Rifapentin) untuk Tn. Indra Gunawan. Saat menekan tombol simpan/terbitkan resep, sistem membalas dengan **HTTP 500 Internal Server Error**:
```text
POST https://localhost:7184/api/v1/health-services/pharmacy-management/prescriptions -> HTTP 500
"An unhandled exception occurred while processing the request: Tariff not found for patient class"
```

#### 2. Akar Masalah Teknis
Investigasi basis data PostgreSQL menunjukkan bahwa pada tabel `RegPatientEncounter` dan `InpEpisode`, pasien Tn. Indra Gunawan tertaut pada `PatientClassId = '013ef5df-8855-4f19-8751-606146fe7250'` yang memiliki nama kelas literal `"UNIQUE"`. Kelas ini tidak memiliki satu pun data tarif obat yang aktif di database (`MstTariff` / `MstDrugTariff`).
Sementara itu, pasien ditempatkan di tempat tidur `BED 001 Ruang Rawat Inap Kelas I 1` yang berkelas `"KELAS I"`.
Ketiadaan tarif pada kelas `"UNIQUE"` menyebabkan kalkulasi biaya obat dan perhitungan klaim asuransi AdMedika mengalami kegagalan fatal (*null reference*).

#### 3. Solusi Penerapan (*Fix Applied*)
Dijalankan skrip remediasi data database `fix_enc_class.py`:
```python
UPDATE "RegPatientEncounter"
SET "PatientClassId" = '0b1990eb-c539-4221-aaf9-34350b98c9fd' -- KELAS I
WHERE "Id" = 'd0f70f24-5232-43f1-aee4-256308b2bf95';

UPDATE "InpEpisode"
SET "PatientClassId" = '0b1990eb-c539-4221-aaf9-34350b98c9fd' -- KELAS I
WHERE "Id" = 'c3fe1370-18f0-42fb-8d9f-01449212828e';
```
Kelas `"KELAS I"` memiliki 7.752 butir tarif obat aktif dan terkonfigurasi dengan harga klaim asuransi AdMedika.

#### 4. Bukti Verifikasi
Eksekusi pengujian peresepan obat (`test-create-resep.mjs`):
- `GET /api/v1/health-services/pharmacy-management/prescribing-drugs`: Menghasilkan HTTP 200 OK dengan 10 obat formularium aktif.
- `POST /api/v1/health-services/pharmacy-management/prescriptions`: **HTTP 201 Created**, nomor resep `RX-20260924-00001` terbit dengan 1 butir obat, harga total 12, tanggungan penjamin 0, bayar pasien 12.

---

### ISS-12 — Galat HTTP 404 pada Tab Riwayat Revisi Resume Medis

| Parameter | Keterangan |
| :--- | :--- |
| **Kode Isu** | `ISS-12` |
| **Area Terdampak** | Frontend (`use-inpatient-resume-tab.jsx`) |
| **Tingkat Keparahan** | **Major** (Visibilitas Jejak Audit Dokumen Medis) |
| **Status** | 🟢 **SELESAI DIPERBAIKI & TERVERIFIKASI** |

#### 1. Gejala Klinis / Operasional
Saat dokter membuka tab Resume Medis dan beralih ke segmen *Riwayat Revisi*, konsol mencatat galat HTTP 404:
```text
[API INTERCEPT] GET 404 https://localhost:7184/api/v1/health-services/inpatient-management/discharges/c3fe1370-18f0-42fb-8d9f-01449212828e/summary-revisions
```
Layar menampilkan pesan: *"Gagal memuat riwayat revisi resume medis."*

#### 2. Akar Masalah Teknis
Pada arsitektur backend ASP.NET Core (`InpatientDischargeController.cs`), endpoint mandiri `/summary-revisions` **tidak pernah ada**.
Backend menyediakan data revisi melalui parameter query pada endpoint resume:
```csharp
[HttpGet("{episodeId:guid}/summary")]
public async Task<IActionResult> GetSummary(Guid episodeId, [FromQuery] bool includeRevisions = false)
```
Ketika `includeRevisions: true` dikirimkan, service `InpDischargeService.cs` memuat data riwayat dari tabel `InpDischargeSummaryRevision` dan menyertakannya di dalam properti `summary.Revisions`.
Hook frontend `use-inpatient-resume-tab.jsx` secara keliru memanggil rute `/summary-revisions` terpisah.

#### 3. Solusi Penerapan (*Fix Applied*)
Memperbarui berkas `use-inpatient-resume-tab.jsx`:
1. Pada `loadSummary`: Mengubah pemanggilan menjadi `${episodeId}/summary?includeRevisions=true`.
2. Pada `loadRevisions`: Mengubah pemanggilan menjadi:
```javascript
const payload = await inpatientDischargeService.getOptional(
  `${episodeId}/summary?includeRevisions=true`,
);
const normalized = normalizeDischargeRevisions(payload?.revisions || payload);
setRevisions(normalized);
```

#### 4. Bukti Verifikasi
- Pengujian Playwright E2E (`test-ui-create-resume.mjs`):
  `[API INTERCEPT] GET 200 https://localhost:7184/api/v1/health-services/inpatient-management/discharges/.../summary?includeRevisions=true`
- Unit test `node --test tests/unit/inpatient-resume-parity.test.mjs tests/unit/inpatient-resume-payload.test.mjs`: **13 Lulus, 0 Gagal (100% Pass)**.

---

### ISS-13 — Penolakan HTTP 403 Pencatatan CPPT Perawat oleh Guard Profesi Klinis

| Parameter | Keterangan |
| :--- | :--- |
| **Kode Isu** | `ISS-13` |
| **Area Terdampak** | Backend Authorization Guard & Database Kepegawaian (`MstEmployee`, `WfpOrganizationAssignment`) |
| **Tingkat Keparahan** | **Critical** (Asuhan Keperawatan Terintegrasi Terblokir) |
| **Status** | 🟢 **SELESAI DIPERBAIKI & TERVERIFIKASI** |

#### 1. Gejala Klinis / Operasional
Perawat bangsal Mira Safitri mencoba mencatat catatan perkembangan harian (SOAP Keperawatan) pada linimasa CPPT pasien. Sistem menolak dengan **HTTP 403 Forbidden**:
```json
{
  "success": false,
  "statusCode": 403,
  "message": "Akun Anda belum tertaut ke data dokter atau pegawai dengan profesi klinis aktif, sehingga jenis catatan tidak dapat ditentukan. Hubungi bagian kepegawaian untuk menautkan profesinya."
}
```

#### 2. Akar Masalah Teknis
Metode `CreateNote` pada `PatientIntegratedProgressNoteController.cs` menjalankan guard keamanan `ResolveCpptAuthorProfessionAsync`:
```csharp
var authorProfession = await _inpatientClinicalContextService
    .ResolveCpptAuthorProfessionAsync(actorUserId, HttpContext.RequestAborted);
if (!authorProfession.IsResolved)
    return StatusCode(403, PenolakanTanpaProfesiKlinis);
```
Pemeriksaan ke database PostgreSQL menunjukkan bahwa pada tabel `MstEmployee`, baris pegawai Mira Safitri (`1ada3363-d69d-447e-ade1-f596d4d97df1`) memiliki nilai kolom `ProfessionId = NULL`.
Selain itu, baris `WfpOrganizationAssignment` miliknya memiliki nilai `OrganizationUnitId = NULL`.
Hal ini menyebabkan sistem tidak dapat mengenali Mira sebagai perawat klinis aktif dan tidak dapat memvalidasi penempatannya di Instalasi Rawat Inap tempat pasien dirawat (`IsNurseOnDutyAtEpisodeAsync`).

#### 3. Solusi Penerapan (*Fix Applied*)
Dijalankan skrip integrasi kepegawaian `fix_mira_profession.py`:
1. Menautkan `ProfessionId` Mira Safitri ke profesi `Perawat` (`0e5e15da-c80b-47bd-ba66-4aa841241301`, kode `PRF-RSMMC-00002`, `IsClinicalProfession = true`).
2. Menautkan `OrganizationUnitId` penugasan organisasinya ke `Instalasi Rawat Inap` (`c797d03f-8882-4fbc-b582-aeb51a198d51`).

#### 4. Bukti Verifikasi
- Pengujian siklus penuh CPPT (`test-cppt-full-cycle.mjs`):
  - Perawat Mira mencatat SOAP: **HTTP 200 OK**, terbit `CPPT-20260924-0001` (ID: `fd396723-13ec-4a0c-9add-6f093a36d453`).
  - DPJP dr. Rendy Pangalila memverifikasi catatan perawat via `PATCH .../{id}/verify`: **HTTP 200 OK**, `verificationStatus = 2 (Verified)`, `verifiedByUserName = "dr. Rendy Pangalila"`.
  - Screenshot linimasa dan modal detail CPPT terverifikasi tersimpan di `test-with-agy/screenshots/cppt-full-cycle/`.

---

## 3. Matriks Status Penutupan Isu

| Kode Isu | Deskripsi Masalah | Solusi | Status Akhir |
| :---: | :--- | :--- | :---: |
| **ISS-09** | Defect 403 Hemodialisa Tab Penunjang | Penyelarasan konstanta `isAvailable: false` & bypass network | 🟢 **SELESAI (100%)** |
| **ISS-10** | Galat 404 Sidebar Profil Dokter | Menghapus pemanggilan API usang, fallback ke data Redux | 🟢 **SELESAI (100%)** |
| **ISS-11** | Defect 500 Resep Obat (Kelas UNIQUE) | Penautan episode ke `KELAS I` dengan 7.752 tarif aktif | 🟢 **SELESAI (100%)** |
| **ISS-12** | Galat 404 Riwayat Revisi Resume | Menggunakan query parameter `?includeRevisions=true` | 🟢 **SELESAI (100%)** |
| **ISS-13** | Defect 403 CPPT Perawat (Tautan Profesi) | Penautan `ProfessionId` Perawat & Unit Rawat Inap | 🟢 **SELESAI (100%)** |

Seluruh kelima isu telah tertutup secara permanen dan diverifikasi tidak menimbulkan regresi pada modul lain.
