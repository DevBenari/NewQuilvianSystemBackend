# Laporan Perubahan Backend — `BE-RWI-227`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-227` |
| Judul | Edit Persetujuan berversi dan Riwayat Perubahan General Consent |
| Slice | Perbaikan `PLAN-REPAIR-EPS-008`, `FIX-EPS-008-01` |
| Roadmap | [`../../../roadmap/backend-roadmap-admisi-pendaftaran.md`](../../../roadmap/backend-roadmap-admisi-pendaftaran.md) — register dan kartu 4.3 `BE-RWI-227` |
| Trace | [`ISSUE-EPS-008`](../../../docs/issue/issue-008-edit-riwayat-general-consent.md) (`ISS-EPS-008-01`, `-02`, `-T1`); [`PLAN-REPAIR-EPS-008`](../../../docs/plan-repair/plan-repair-008-edit-riwayat-general-consent.md); `RWI-DEC-240`, `RWI-DEC-236` butir 3, `RWI-DEC-272`, `RWI-DEC-273`; `RWI-AC-355`, `RWI-AC-358` |
| Contract version | Kontrak `episode-rawat-inap` `0.11.0` `approved` + delta pada rencana perbaikan yang disetujui pemilik 13 Oktober 2026. Pencatatan delta ke `contracts/api-contract.md` = `FIX-EPS-008-04` |
| Dependency | `BE-RWI-205` ✅ |
| Klasifikasi | `MEDIUM` — satu service, satu DTO, satu controller; tanpa entity atau migration; menyentuh transaksi dan konkurensi |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` branch `MHamzah`: tiga berkas di `Areas/HealthServices/InPatientManagement`, laporan ini, roadmap, traceability, dan register rencana perbaikan |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `4ec02370` (`MHamzah`), perubahan belum di-commit |
| Tanggal | 13 Oktober 2026 |
| Status | ✅ Selesai pada tingkat source dan build. Uji transaksi pada PostgreSQL `NOT RUN` (lihat bagian 5) |

---

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `InPatientManagement`, Workspace PPRI dan persetujuan admisi |
| Prefix registry | `Inp` — `ACTIVE` (`docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 30) |
| Keberlakuan | `TOUCHED LEGACY` (`InpatientAdmissionConsentService`, `InpatientAdmissionDocumentController`) + `NEW CODE` (dua DTO, dua action, tiga method service) |
| Arketipe | Transaksi, sub-proses yang di-scope ke episode (standar endpoint transaksi 2.3). Koreksi adalah perintah yang menghasilkan baris riwayat, maka `POST …/revisions`, bukan `PUT` |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-DTO-001`, `QBE-VAL-001`, `QBE-TXN-001`, `QBE-LOG-001`, `QBE-DEL-001` |
| Wewenang | Source backend: ya. Migration: tidak dibutuhkan (kolom versi sudah ada). Database dan deployment: tidak |

---

## 1. Masalah yang diperbaiki

Sebelum task ini, persetujuan umum rawat inap yang sudah ditandatangani digital tidak dapat dikoreksi
dari Workspace PPRI. Satu-satunya jalan mengubahnya adalah menyimpan ulang dari langkah admisi, dan
jalan itu **menimpa** isi lama di tempat. Isi lama, tanda tangan lama, dan siapa yang menandatangani
semula hilang tanpa jejak (`InpatientAdmissionConsentService.cs` versi `4ec02370` baris 170–208).

Contoh: pukul 09.40 tersimpan "Rina Santoso (istri)". Pukul 10.15 petugas lain menyimpan ulang
"Andi Santoso (anak)". Sesudahnya sistem hanya mengenal Andi, dan tanda tangan Rina hilang.

---

## 2. Proses bisnis

| Butir | Isi |
| --- | --- |
| Tujuan | Data persetujuan yang salah dapat dikoreksi, dan setiap koreksi dapat ditelusuri: siapa, kapan, alasannya, dan apa yang berubah |
| Pelaku | Petugas PPRI pemegang `InpatientAdmissionDocument : Update` (koreksi); pemegang `InpatientAdmissionDocument : Read` (melihat riwayat); petugas admisi (simpan dari langkah admisi) |
| Pemicu | Petugas menemukan data penanda tangan yang salah, misalnya NIK salah satu angka |
| Prasyarat | Episode `Admitted` atau `DischargePending`; persetujuan sudah tersimpan dan `Completed` |

**Langkah utama**

1. Petugas membuka Workspace PPRI → General Consent dan menekan Edit Persetujuan.
2. Layar mengirim isian lengkap, `RowVersion` versi aktif, dan alasan perubahan.
3. Server memeriksa episode, versi, alasan, nama, invariant bayi, tanda tangan, dan ada tidaknya
   perubahan.
4. Dalam satu transaksi: versi lama menjadi `Superseded`, lalu versi baru `Completed` dibuat dengan
   `VersionNo + 1`, `PreviousVersionId`, alasan, pihak penanda tangan, dan slot tanda tangan baru.
5. Riwayat menampilkan versi baru di atas versi lama, lengkap dengan pengubah dan daftar perubahan.

**Perubahan status**

| Dari | Tindakan | Ke | Siapa | Syarat |
| --- | --- | --- | --- | --- |
| Versi N `Completed` | Edit Persetujuan | Versi N `Superseded` + versi N+1 `Completed` | `InpatientAdmissionDocument : Update` | Episode berjalan, `RowVersion` cocok, alasan 10–500 karakter, minimal satu isian berubah |
| Versi N `Completed` | Simpan ulang dari langkah admisi, isi berbeda | Sama; alasan otomatis "Disimpan ulang dari langkah Formulir Persetujuan admisi." | `InpatientEpisode : Create` (tetap) | Isi berbeda |
| Versi N `Completed` | Simpan ulang dari langkah admisi, isi sama | Tetap versi N | — | — |

**Jalur tidak normal**

| Kejadian | Hasil | Contoh |
| --- | --- | --- |
| Dua petugas menyimpan koreksi dari versi yang sama | Yang kedua 409 `INP-ADM-DOC-004` "Persetujuan sudah diubah petugas lain…" | Dewi dan Sari membuka versi 2; Dewi menyimpan lebih dulu |
| Episode `Closed`/`Cancelled` | 409 `INP-ADM-DOC-001` | Koreksi sesudah pasien pulang ditolak; riwayat tetap terbaca |
| Jenis penanda tangan berganti tanpa tanda tangan baru | 422 `TTD_BARU_WAJIB` | `Spouse` → `Family` tanpa kanvas baru |
| Tidak ada isian yang berubah | 422 `TIDAK_ADA_PERUBAHAN` | Menyimpan form tanpa mengubah apa pun |
| Bayi baru lahir dengan penanda tangan `Self` | 422 `BAYI_PENANDATANGAN_WAJIB_WALI` | Invariant `RWI-DEC-273` tetap berlaku pada koreksi |

**Hasil akhir.** Selalu tepat satu versi aktif per episode. Versi lama beserta citra tanda tangannya
tetap tersimpan dan terbaca di riwayat. `GET …/consent` dan kedua tab General Consent selalu membaca
versi aktif.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`InpatientAdmissionConsentService.cs`, `InpatientAdmissionConsentController.cs`,
`InpatientAdmissionConsentDtos.cs`, `InpatientAdmissionDocumentController.cs`,
`InpAdmissionDocumentService.cs` (pola `ReviseAsync`, `Touch`, `IsUniqueViolation`),
`InpAdmissionWriteGuard.cs`, `InpAdmissionSourceReader.cs` (`GetUsersAsync`), `InpAdmissionResult.cs`,
`InpAdmissionDocument.cs`, `InpAdmissionDocumentStatus.cs`,
`Repositories/Configurations/HealthServices/InPatientManagement/InpAdmissionDocument*Configuration.cs`
(check constraint `CK_InpAdmissionDocument_State`, unique index aktif dan `PreviousVersionId`,
`RowVersion` sebagai concurrency token), `Program.cs` (registrasi DI), konsumen frontend
`inpatient-admission-consent-form-step.jsx` dan `use-admission-general-consent.js`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/InPatientManagement/DTOs/InpatientAdmissionConsentDtos.cs` | `CreateDto` dan `Response` mendapat `GuidanceReceived` dan `GuidanceNote`. `Response` mendapat `VersionNo`, `RowVersion`, `LastChangedByName`, dan `LastChangedAt`. DTO baru: `InpatientAdmissionConsentRevisionRequest`, `InpatientAdmissionConsentHistoryResponse`, `InpatientAdmissionConsentChangeResponse`. `&` pada komentar XML diganti "dan" (CS1570) |
| `Areas/HealthServices/InPatientManagement/Services/InpatientAdmissionConsentService.cs` | Pembacaan hanya versi aktif. `POST` pada persetujuan `Completed` → tanpa perubahan atau versi baru beralasan otomatis. Method baru `ReviseConsentAsync` dan `GetConsentHistoryAsync`. Pembantu `CreateRevisionAsync` (transaksi dua langkah seperti `ReviseAsync`), `DescribeChanges`, `ReadContent`, `FillVersionInfoAsync`. Konstruktor menerima `InpAdmissionWriteGuard` dan `InpAdmissionSourceReader` (sudah terdaftar di `Program.cs`) |
| `Areas/HealthServices/InPatientManagement/Controllers/InpatientAdmissionDocumentController.cs` | Dua action baru `ReviseGeneralConsent` dan `GetGeneralConsentHistory`. Konstruktor menerima `InpatientAdmissionConsentService` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Dua endpoint baru di grup Workspace PPRI. `GET`/`POST …/consent` tetap kompatibel: field baru bersifat tambahan; `POST` tetap mengembalikan 201 termasuk saat tidak ada perubahan. **Delta terhadap rencana:** endpoint edit dipindah dari `PUT …/consent` ke `POST …/admission-workspace/general-consent/revisions`, dan riwayat ke `GET …/admission-workspace/general-consent/history`, karena aturan akses role mewajibkan argumen ke-1 `[AccessPermission]` sama dengan `ControllerName` controller-nya (`InpatientAdmissionDocument`) |
| Database | Tanpa perubahan schema dan tanpa migration. Memakai kolom `VersionNo`, `PreviousVersionId`, `CorrectionReason`, `SupersededAt`, status `Superseded`, check constraint, dan unique index yang sudah ada sejak `BE-RWI-192` |
| Keamanan/Auth | Edit: `InpatientAdmissionDocument : Update`; riwayat: `InpatientAdmissionDocument : Read` — keduanya baris yang sudah terdaftar di layar Akses Role. Tidak ada hardcode peran. Logger hanya mencatat id, controller, aksi, dan status; NIK, nama, dan alasan tidak masuk logger. Kelayakan status episode diperiksa `InpAdmissionWriteGuard` |

---

## 4. Dokumentasi endpoint

#### Health Services / Inpatient Management / Inpatient Admission Workspace

Base URL: `api/v1/health-services/inpatient-management/episodes/{episodeId}/admission-workspace`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `POST` | `/general-consent/revisions` | Edit Persetujuan: membuat versi baru beralasan, versi lama menjadi riwayat | `InpatientAdmissionDocument : Update` | `InpatientAdmissionConsentRevisionRequest` | `InpatientAdmissionConsentResponse` |
| `GET` | `/general-consent/history` | Riwayat semua versi, terbaru di atas | `InpatientAdmissionDocument : Read` | — | `List<InpatientAdmissionConsentHistoryResponse>` |

Kode status `POST /general-consent/revisions`:

- **200**: versi baru tersimpan, "Persetujuan diperbarui menjadi versi 2."
- **400**: alasan kurang dari 10 karakter, nama kurang dari 3 karakter, `RowVersion` kosong, atau
  format tanda tangan bukan gambar.
- **404**: persetujuan belum pernah disimpan.
- **409**: diubah petugas lain sejak dibuka, atau episode sudah ditutup.
- **422**: `BAYI_PENANDATANGAN_WAJIB_WALI`, `TTD_BARU_WAJIB`, atau `TIDAK_ADA_PERUBAHAN`.

Contoh request:

```json
{
  "rowVersion": "<rowVersion versi aktif dari GET …/consent>",
  "correctionReason": "NIK penanda tangan salah ketik saat admisi",
  "signerType": "Spouse",
  "signerName": "Rina Santoso",
  "signerRelationship": "Spouse",
  "signerRelationshipText": "Istri",
  "signerIdentityNumber": "3171xxxxxxxx9999",
  "signerPhoneNumber": "081200000001",
  "signerAddress": "Jl. Kenanga No. 5, Bekasi",
  "guidanceReceived": true,
  "guidanceNote": "brosur diserahkan",
  "signatureImageBase64": null
}
```

Contoh satu baris riwayat:

```json
{
  "versionNo": 2,
  "status": "Aktif",
  "isActive": true,
  "changedAt": "2026-10-13T03:15:00Z",
  "changedByName": "Dewi",
  "correctionReason": "NIK penanda tangan salah ketik saat admisi",
  "isSignatureReplaced": false,
  "changes": [
    { "field": "SignerIdentityNumber", "label": "No. Identitas", "before": "3171xxxxxxxx9998", "after": "3171xxxxxxxx9999" }
  ]
}
```

#### InpatientAdmissionConsent (tetap, perilaku diperbarui)

Base URL: `api/v1/health-services/inpatient-management/episodes/{episodeId}/consent` beserta dua
alias lamanya.

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/` | Versi aktif beserta `VersionNo`, `RowVersion`, panduan, dan "terakhir diubah" | `InpatientEpisode : Read` (tetap) |
| `POST` | `/` | Simpan dari langkah admisi. Bila sudah ada versi aktif: isi sama → tanpa perubahan; isi berbeda → versi baru | `InpatientEpisode : Create` (tetap) |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -c Repair008 --no-restore -p:UseAppHost=false` (sesudah source final) | Berhasil: exit 0, **0 error, 279 warning**, 1 menit 46 detik. Tidak ada warning pada `InpatientAdmissionConsentService.cs`, `InpatientAdmissionConsentDtos.cs`, maupun `InpatientAdmissionDocumentController.cs` | `PASS` | Log `be-build-008b.log` di scratchpad sesi |
| Build pertama dengan konfigurasi sama, sebelum perapihan komentar | Berhasil: 0 error, 281 warning, 2 menit 27 detik. Tiga warning `CS1570` pada berkas yang disentuh berasal dari baris lama (karakter `&`); dua di antaranya dirapikan | `PASS` | `be-build-008.log` |
| Konfigurasi terpisah `Repair008` | Dipakai karena backend pengguna sedang berjalan dari `bin/Debug` (proses `dotnet run --no-build`); proses itu tidak dihentikan | — | `Get-CimInstance Win32_Process` |
| `Invoke-QbeConformanceCheck.ps1 -Path <berkas> -Mode Strict` untuk ketiga berkas | PASS; 0 violation, 0 review, 0 info pada masing-masing berkas | `PASS` | Keluaran perintah |
| `git diff --check` pada folder `InPatientManagement` | Bersih | `PASS` | Keluaran perintah |
| Kecocokan metadata akses (role-access-rules bagian 7) | `[AccessPermission("InpatientAdmissionDocument", "Update")]` + `[AccessAction("Update", …, AccessType = AccessTypes.Update)]`; `[AccessPermission("InpatientAdmissionDocument", "Read")]` + `[AccessAction("Read", …, AccessTypes.Read)]`; `ControllerName = "InpatientAdmissionDocument"` | `PASS` | Review diff controller |
| Uji transaksi PostgreSQL (AC 1–10 lewat Swagger/API pada database uji) | Tidak dijalankan | `NOT RUN` | Backend pengguna masih memakai DLL lama (`--no-build`). Uji butuh restart backend dengan hasil build ini dan data episode uji. Memori pemilik: build dan uji runtime dijalankan pemilik |

Uji manual: `REQUIRED` — skenario pada `PLAN-REPAIR-EPS-008` bagian 8, sesudah frontend `FE-RWI-241`
dan `FE-RWI-242`.

AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md).

---

## 6. Acceptance criteria dan Definition of Done

Kriteria diambil dari `PLAN-REPAIR-EPS-008` `FIX-EPS-008-01`. "Terpenuhi di source" berarti
perilakunya ada pada source yang terbangun tanpa error. Pembuktian transaksi nyata menunggu uji
runtime di atas.

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Koreksi NIK → versi 1 `Superseded` utuh, versi 2 `Completed` menunjuk versi 1, `GET` mengembalikan versi 2 | Terpenuhi di source | `CreateRevisionAsync` (versi lama hanya status, `SupersededAt`, dan `Touch`; isi tidak disentuh); `LoadActiveAsync` menyaring status aktif |
| 2. Riwayat dua baris dengan pengubah, waktu, alasan, `Changes` | Terpenuhi di source | `GetConsentHistoryAsync` + `DescribeChanges` + `GetUsersAsync` |
| 3. Alasan 5 karakter → 400, tanpa versi baru | Terpenuhi di source | `ReviseConsentAsync`, pemeriksaan sebelum transaksi |
| 4. Dua petugas, `RowVersion` lama → 409, satu versi aktif | Terpenuhi di source | Pemeriksaan `RowVersion` di awal + `DbUpdateConcurrencyException`/unique violation → `Stale()` |
| 5. Ganti jenis penanda tangan tanpa TTD → 422 `TTD_BARU_WAJIB`; dengan TTD → berhasil, `SignedAt` baru | Terpenuhi di source | `signerTypeChanged`, `signatureReplaced` |
| 6. Tanpa perubahan → 422 `TIDAK_ADA_PERUBAHAN` | Terpenuhi di source | `DescribeChanges(...).Count == 0` |
| 7. Episode `Closed` → 409; `GET` dan riwayat tetap terbaca | Terpenuhi di source | `EnsureWritableAsync` untuk edit; `EnsureReadableAsync` untuk riwayat; `GET …/consent` tanpa penjaga status |
| 8. Simpan ulang isi sama dari admisi → sukses, versi tetap; isi berbeda → versi baru beralasan otomatis | Terpenuhi di source | Cabang `Completed` pada `SaveConsentAsync`. Kode sukses tetap 201 seperti kontrak `BE-RWI-205` (rencana menyebut 200; dikoreksi di register rencana) |
| 9. Panduan tersimpan, terbaca, dan muncul di `Changes` | Terpenuhi di source | `GuidanceReceived`/`GuidanceNote` di DTO, salinan isi, dan `DescribeChanges` |
| 10. Bayi `Self` pada koreksi → 422 | Terpenuhi di source | `IsNewborn` + `IsSelfSigner` di `ReviseConsentAsync` |
| DoD: build tanpa error | Terpenuhi | Bagian 5 |
| DoD: QBE | Terpenuhi | Bagian 5 |
| DoD: verifikasi runtime | Belum | `NOT RUN`, lihat bagian 5 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Warning build bawaan repository (281, termasuk `NU1902/NU1903` ImageSharp). Tidak ada warning baru dari baris yang ditambahkan |
| Masalah yang diketahui | (1) `InpatientAdmissionConsentController` mendaftarkan `[AccessAction]` atas nama `InpatientAdmissionConsent` tetapi menegakkan `[AccessPermission("InpatientEpisode", …)]`, sehingga baris Akses Role miliknya sendiri tidak pernah dipakai. Ini kode lama `BE-RWI-205`; dicatat, tidak diubah. (2) `InpatientAdmissionConsentCreateDto.SignerName` mengizinkan 200 karakter, padahal kolom `InpAdmissionDocumentParty.FullName` 150. DTO revisi baru memakai 150; DTO lama tidak diubah. (3) Endpoint generik `documents/{id}/revisions` masih dapat membuat konsep untuk jenis `GeneralConsent`; tidak ada jalur layar ke sana |
| Risiko tersisa | Belum diuji pada PostgreSQL. Citra tanda tangan tersalin per versi, sehingga ukuran baris bertambah satu PNG per koreksi |
| Perubahan sampingan | `NONE`. Output build berada di `bin/Repair008` dan `obj/Repair008` (abaikan Git) |
| Interupsi | `NONE` |
| Status Git | Tiga berkas source task ini berstatus `M`; berkas `docs/**` task ini; perubahan pengguna lain (`ClinicalManagement/Services/EncounterInsuranceService.cs`, dokumen kelas perawatan) tidak disentuh |
| Langkah berikutnya | `FE-RWI-241` (form edit) → `FE-RWI-242` (panel riwayat) → `FIX-EPS-008-04` (dokumen blueprint). Pemilik: hentikan backend lokal, build, jalankan ulang, lalu uji skenario rencana bagian 8 |
