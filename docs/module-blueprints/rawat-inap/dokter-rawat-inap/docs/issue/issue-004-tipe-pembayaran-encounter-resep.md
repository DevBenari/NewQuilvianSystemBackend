# ISSUE-004 — Tab Resep Gagal Muat Obat untuk Penjamin Perusahaan: Tipe Pembayaran Encounter Tidak Didukung dan Fallback Penjamin Menyesatkan

```yaml
issue_id: ISSUE-DOK-004
module_id: rawat-inap
submodule: dokter-rawat-inap
layar: "Dokter - Rawat Inap — Tab Resep (FE-RWI-137)"
sumber_laporan: "Laporan pengguna 06-10-2026: Screenshot layar Resep obat dengan galat 'Tipe pembayaran encounter tidak didukung', total 0 obat, badge 'Penjamin belum terbaca', dan header menampilkan 'BPJS Kesehatan'"
tanggal_issue: "2026-10-06"
status: DALAM_PERBAIKAN
keparahan_tertinggi: Blocker
source_sha_backend: "f2c48e5b251f80e7f3963bfd02b720e3ad8fb978 (MHamzah)"
source_sha_frontend: "b010ffb9722f236160477c95c931c22467550200 (HamzahV2)"
rencana_perbaikan: ../plan-repair/plan-repair-004-tipe-pembayaran-encounter-resep.md
ditulis_dengan: "skill diagnose-module-issue"
```

## 1. Ringkasan

Laporan ini menyoroti kegagalan fatal pada tab **Resep** di lembar kerja Dokter Rawat Inap ketika melayani pasien rawat inap yang menggunakan penjamin perusahaan. Saat tab Resep dibuka, katalog obat formularium sama sekali tidak muncul (0 obat ditampilkan) dan sistem memunculkan kotak galat bertuliskan *"Tipe pembayaran encounter tidak didukung."*. Di saat bersamaan, badge status penjamin di panel resep bertuliskan *"Penjamin belum terbaca"*, sedangkan header profil pasien di bagian atas justru menampilkan *"Penjamin: BPJS Kesehatan"*.

Berdasarkan audit langsung ke basis data PostgreSQL dan baris source code, ditemukan bahwa pasien sebenarnya terdaftar menggunakan **Penjamin Perusahaan** (`EncounterPaymentType.CompanyGuarantor = 3`, yaitu PT Telkom Indonesia), bukan BPJS Kesehatan. Modul Billing Kasir sebelumnya telah mendukung tipe ini lewat `CompanyGuarantorCoverageService` (`BE-BKC-044`), namun service peresepan klinis (`EncounterInsuranceService.cs`) tertinggal dan masih membatasi tipe pembayaran hanya pada Tunai (`Cash = 1`) atau Asuransi (`Insurance = 2`), sehingga melempar penolakan HTTP 400 ketika menemui tipe 3. Sementara itu, teks "BPJS Kesehatan" di header muncul akibat nilai fallback teks statis di frontend saat DTO episode rawat inap tidak membawa data nama penjamin.

Isu ini berstatus **Blocker** karena dokter rawat inap sama sekali tidak dapat melihat katalog formularium maupun membuat resep obat untuk pasien mana pun yang dijamin oleh perusahaan mitra rumah sakit.

---

## 2. Laporan Asli

| No. laporan | Keluhan pelapor (kata-kata asli / ringkas) | Lampiran |
| :---: | --- | --- |
| 1 | "ini masalah kenapa ya. Tipe pembayaran encounter tidak didukung. anda bisa coba cek pada source code dan database ya ?" | Screenshot layar dokter rawat inap, tab Resep aktif, kotak galat *"Tipe pembayaran encounter tidak didukung."*, total 0 obat, badge *"Penjamin belum terbaca"*, header *"Penjamin: BPJS Kesehatan"* |

---

## 3. Ringkasan Temuan

| ID | No. laporan | Judul | Jenis | Area | Keparahan | Status bukti | Perbaikan |
| --- | :---: | --- | --- | --- | --- | --- | --- |
| `ISS-DOK-004-01` | 1 | Galat HTTP 400 *"Tipe pembayaran encounter tidak didukung"* saat memuat katalog obat resep | `BUG` | Backend | Blocker | SUDAH-VERIFIKASI + DARI-CAPTURE | `FIX-DOK-004-01` |
| `ISS-DOK-004-02` | 1 | Badge penjamin menampilkan *"Penjamin belum terbaca"* akibat katalog gagal dimuat | `BUG` | Frontend | Medium | SUDAH-VERIFIKASI + DARI-CAPTURE | `FIX-DOK-004-02` |
| `ISS-DOK-004-T1` | — | Header profil dokter rawat inap menampilkan penjamin palsu *"BPJS Kesehatan"* karena fallback statis | `BUG` | Frontend | High | SUDAH-VERIFIKASI + DARI-CAPTURE | `FIX-DOK-004-03` |
| `ISS-DOK-004-T2` | — | Layanan coverage klinis (`InsuranceCoverageService`) belum terhubung dengan mesin penjamin perusahaan (`CompanyGuarantorCoverageService`) | `BUG` | Backend | High | SUDAH-VERIFIKASI | `FIX-DOK-004-04` |

---

## 4. Rincian per Temuan

### ISS-DOK-004-01 — Galat HTTP 400 "Tipe pembayaran encounter tidak didukung" saat Memuat Katalog Obat Resep

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `BUG` |
| **Area** | Backend |
| **Keparahan** | Blocker |
| **Status bukti** | SUDAH-VERIFIKASI di source code dan PostgreSQL |
| **Perbaikan** | `FIX-DOK-004-01` |

**Apa yang terjadi.**
Saat dokter membuka tab Resep pada lembar kerja rawat inap untuk pasien penjamin perusahaan (misal: PT Telkom Indonesia), daftar obat formularium rumah sakit tidak muncul sama sekali. Terdapat pesan kesalahan *"Tipe pembayaran encounter tidak didukung."* dengan tombol *"Coba Lagi"*. Dokter tidak dapat mencari obat, memilih obat, maupun meresepkan terapi obat apa pun untuk pasien.

**Kenapa terjadi.**
Rantai penyebab dari antarmuka hingga baris kode:
1. Komponen `PrescriptionBuilderPanel` memanggil hook `useInpatientDrugCatalog({ encounterId })` di `QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/physician-workspace/tabs/prescription/prescription-builder-panel.jsx:89`.
2. Hook tersebut memanggil endpoint backend `GET /api/v1/PrescribingDrug?encounterId=...` di `QuilvianSystemFrontendDev/src/lib/services/health-services/clinical-management/prescribing-drug.service.js`.
3. Di backend, `PrescribingDrugController.GetPrescribingDrugs` memvalidasi konteks asuransi encounter melalui `_encounterInsuranceService.GetContextAsync(encounterId)` di `NewQuilvianSystemBackend/Areas/HealthServices/ClinicalManagement/Controllers/PrescribingDrugController.cs:120-131`. Bila `!context.IsValid`, controller langsung mengembalikan HTTP 400 Bad Request dengan pesan dari `context.ErrorMessage`.
4. Pada `NewQuilvianSystemBackend/Areas/HealthServices/ClinicalManagement/Services/EncounterInsuranceService.cs:79-84`:
```csharp
if (encounter.PaymentType == EncounterPaymentType.Cash)
{
    return new EncounterInsuranceContext { ... };
}

if (encounter.PaymentType != EncounterPaymentType.Insurance)
{
    return EncounterInsuranceContext.Fail(
        encounterId,
        "Tipe pembayaran encounter tidak didukung.");
}
```
5. Di basis data PostgreSQL (`RegPatientEncounter`), pasien memiliki `PaymentType = 3` (`CompanyGuarantor`). Karena `3 != 2` (`EncounterPaymentType.Insurance`), kode mengeksekusi percabangan gagal dan mengembalikan pesan `"Tipe pembayaran encounter tidak didukung."`.

**Apakah ini menyimpang dari desain?**
`BUG`. Desain rawat inap (`02-backend-architecture.md` dan kontrak penjamin perusahaan `BE-RWI-035`) secara sah telah menambahkan `EncounterPaymentType.CompanyGuarantor = 3` sebagai opsi penjamin resmi di pendaftaran dan admisi rawat inap. Layanan peresepan klinis tidak boleh menolak kunjungan sah yang berstatus penjamin perusahaan.

**Dampak nyata.**
Pasien Tn. Hendro Wibowo (No RM `00-00-00-19`, Episode `RI-261006042632-C0303`), pegawai PT Telkom Indonesia yang dirawat inap di Kamar Kelas II Bed 001, tidak dapat diberikan resep obat oleh DPJP (dr. Rendy Pangalila). Dokter terblokir total dalam memberikan instruksi medikamentosa harian maupun obat pulang.

**Rekomendasi.**
Perbarui `EncounterInsuranceService.GetContextAsync` agar mendukung `EncounterPaymentType.CompanyGuarantor`, membaca relasi `PaymentSource.CompanyGuarantor` dan `PaymentSource.PatientCompanyGuarantor`, serta mengembalikan konteks penjamin perusahaan yang valid (`IsValid = true`).

---

### ISS-DOK-004-02 — Badge Penjamin Menampilkan "Penjamin belum terbaca"

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI di source code |
| **Perbaikan** | `FIX-DOK-004-02` |

**Apa yang terjadi.**
Di sebelah kanan tombol "Gunakan Template" pada banner Resep Obat, terdapat pill badge yang menampilkan teks *"Penjamin belum terbaca"*.

**Kenapa terjadi.**
Di `prescription-builder-panel.jsx:259-261`, badge membaca `payerLabel` yang diturunkan dari `regularCatalog.payerContext`:
```jsx
<span className={styles.payerChip} data-testid="prescription-payer-badge">
  {payerLabel || (regularCatalog.loading ? "Memuat penjamin..." : "Penjamin belum terbaca")}
</span>
```
Karena pemanggilan `getPrescribingDrugs` gagal akibat galat HTTP 400 pada `ISS-DOK-004-01`, state `regularCatalog.payerContext` tetap berada pada nilai inisial kosong (`readCatalogPayerContext({})`). Akibatnya `payerLabel` bernilai string kosong, sehingga teks beralih ke fallback *"Penjamin belum terbaca"*.

**Apakah ini menyimpang dari desain?**
`BUG`. Desain mengharuskan badge menampilkan penjamin aktif pasien, misalnya "Penjamin: PT Telkom Indonesia" atau jenis pertanggungan terkait.

**Dampak nyata.**
Dokter mengira data penjamin pasien belum terisi atau sistem kehilangan jejak data kepesertaan pasien saat admisi.

**Rekomendasi.**
Setelah `ISS-DOK-004-01` diperbaiki sehingga backend mengembalikan metadata penjamin perusahaan (`PaymentTypeName = "Penjamin Perusahaan"`, `PaymentSourceName = "PT Telkom Indonesia"`), perbarui fungsi pembaca konteks frontend `readCatalogPayerContext` dan `describePayerBadge` di `inpatient-prescription-builder-utils.jsx` agar memformat badge perusahaan dengan jelas.

---

### ISS-DOK-004-T1 — Header Profil Pasien Menampilkan Penjamin Palsu "BPJS Kesehatan"

| | |
| --- | --- |
| **No. laporan** | Temuan tambahan (terlihat jelas di screenshot lampiran 1) |
| **Kenapa dicantumkan** | Anomali ini membuat pelapor bingung: header menuliskan "BPJS Kesehatan", namun sistem menolak bahwa tipe pembayaran tidak didukung |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | High |
| **Status bukti** | SUDAH-VERIFIKASI di source code dan PostgreSQL |
| **Perbaikan** | `FIX-DOK-004-03` |

**Apa yang terjadi.**
Di bagian atas lembar kerja dokter rawat inap, kartu ringkasan identitas pasien menampilkan informasi:
`PENJAMIN: BPJS Kesehatan`
Padahal di basis data, pasien Tn. Hendro Wibowo tidak memiliki kartu BPJS Kesehatan dan kunjungan ini didaftarkan dengan Penjamin Perusahaan PT Telkom Indonesia.

**Kenapa terjadi.**
Di `QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/doctor-inpatient/inpatient-physician-context-header.jsx:150-153`:
```javascript
const payerName =
  workspaceContext?.episode?.payerName ||
  workspaceContext?.episode?.insuranceName ||
  "BPJS Kesehatan";
```
Kode ini menggunakan string statis `"BPJS Kesehatan"` sebagai nilai cadangan (*hardcoded fallback*). Karena service `inpatientEpisodeService.get(episodeId)` mengembalikan data episode di mana DTO `InpEpisode` tidak memiliki properti `payerName` atau `insuranceName` secara langsung, variabel `workspaceContext?.episode?.payerName` bernilai `undefined`, yang menyebabkan kode selalu jatuh ke fallback statis `"BPJS Kesehatan"`.

**Apakah ini menyimpang dari desain?**
`BUG`. Aturan tata kelola Quilvian melarang keras pencantuman data palsu atau asumsi default untuk data finansial/penjamin pasien. Bila penjamin belum terbaca, sistem harus menampilkan penjamin dari snapshot kunjungan atau tanda strip (`"-"`), bukan mengarang asuransi pemerintah.

**Dampak nyata.**
Dokter atau perawat mengira pasien dijamin oleh BPJS Kesehatan (JKN) dan mempertimbangkan restriksi formularium nasional (Fornas), padahal pasien dijamin oleh korporat swasta dengan plafon dan benefit perusahaan tersendiri.

**Rekomendasi.**
Hapus teks hardcoded `"BPJS Kesehatan"`. Gunakan pembacaan penjamin berjenjang:
1. `patient?.payerName` atau `patient?.primaryGuarantorNameSnapshot` dari data sensus rawat inap.
2. `workspaceContext?.episode?.payerName` dari DTO episode.
3. Fallback netral: `"-"` atau `"Penjamin Tidak Terdefinisi"`.

---

### ISS-DOK-004-T2 — Mesin Coverage Klinis Obat Belum Terhubung dengan Penjamin Perusahaan

| | |
| --- | --- |
| **No. laporan** | Temuan tambahan (analisis backend downstream) |
| **Kenapa dicantumkan** | Akar masalah kedua: bahkan jika `GetContextAsync` diloloskan, kalkulasi tarif obat di `PrescribingDrugController` memanggil `InsuranceCoverageService.ResolveDrugAsync` yang juga hanya mengenal Insurance |
| **Jenis** | `BUG` |
| **Area** | Backend |
| **Keparahan** | High |
| **Status bukti** | SUDAH-VERIFIKASI di source code |
| **Perbaikan** | `FIX-DOK-004-04` |

**Apa yang terjadi.**
Pada `PrescribingDrugController.cs:154-164`, setiap obat dalam loop dihitung status pertanggungannya menggunakan `_insuranceCoverageService.ResolveDrugAsync`:
```csharp
foreach (var drug in drugs)
{
    var coverage = await _insuranceCoverageService.ResolveDrugAsync(
        encounterId,
        drug.Id,
        quantity: 1,
        cancellationToken: cancellationToken
    );
    items.Add(MapResponse(drug, coverage));
}
```
Layanan `InsuranceCoverageService` di baris 152–162 hanya memiliki dua penanganan:
- `Cash` / `!HasInsurance` &rarr; `BuildCashResult` (harga tarif normal rumah sakit).
- `Insurance` &rarr; mencari ke `MstInsuranceTariff` dan `MstInsuranceCoverageRule`.

Jika pasien adalah `CompanyGuarantor`, `InsuranceCoverageService` akan mencoba mencari ke tabel asuransi atau gagal. Di sisi lain, modul Billing Kasir sudah memiliki `CompanyGuarantorCoverageService` di `Areas/HealthServices/ClinicalManagement/Services/CompanyGuarantorCoverageService.cs` yang mengevaluasi `MstCompanyGuarantorCoverageRule`. Layanan ini belum diintegrasikan ke peresepan dokter.

**Apakah ini menyimpang dari desain?**
`BUG`. Resep obat untuk penjamin perusahaan harus dapat menampilkan tarif normal rumah sakit dan status pertanggungan aturan perusahaan (atau minimal fallback ke tarif normal rumah sakit sebagai advisory klinis tanpa crash).

**Dampak nyata.**
Peresepan obat untuk penjamin perusahaan berisiko melempar galat lanjutan atau menampilkan kalkulasi nol/tidak dicover secara keliru.

**Rekomendasi.**
Pada `InsuranceCoverageService` (atau dispatcher baru), saat `context.PaymentType == EncounterPaymentType.CompanyGuarantor`, delegasikan resolusi advisory ke `CompanyGuarantorCoverageService.ResolveTariffAsync` atau berlakukan resolusi tarif rumah sakit dengan aturan pertanggungan perusahaan.

---

## 5. Tanya-Jawab Pelapor

> **T:** Ini masalah kenapa ya? *Tipe pembayaran encounter tidak didukung.*
>
> **J:** Masalah ini terjadi karena pasien yang sedang dibuka terdaftar dengan cara bayar **Penjamin Perusahaan** (PT Telkom Indonesia, kode `PaymentType = 3`), sedangkan fungsi backend peresepan obat (`EncounterInsuranceService`) saat ini hanya mengizinkan dua cara bayar: Tunai (`Cash = 1`) dan Asuransi (`Insurance = 2`). Saat membaca data pasien bertipe 3, backend langsung menolak permintaan dengan pesan galat *"Tipe pembayaran encounter tidak didukung."*, sehingga daftar obat tidak dapat dimuat.

> **T:** Anda bisa coba cek pada source code dan database ya?
>
> **J:** Sudah dicek secara mendalam pada keduanya:
> 1. **Di Database (PostgreSQL):** Pada tabel `RegPatientEncounter` untuk episode `RI-261006042632-C0303` (pasien Hendro Wibowo, No. RM `00-00-00-19`), kolom `PaymentType` bernilai `3`. Pada tabel `RegPatientEncounterGuarantor`, nama penjamin tercatat sebagai `"PT Telkom Indonesia"` dengan kode `"COMP-TELKOM"`.
> 2. **Di Source Code Backend:** Pada file `EncounterInsuranceService.cs` baris 79–84, terdapat validasi kaku `if (encounter.PaymentType != EncounterPaymentType.Insurance) { return EncounterInsuranceContext.Fail(..., "Tipe pembayaran encounter tidak didukung."); }` yang belum mengakomodasi `CompanyGuarantor = 3`.
> 3. **Di Source Code Frontend:** Ditemukan pula bahwa tulisan *"BPJS Kesehatan"* pada header pasien di screenshot berasal dari teks cadangan statis (*hardcoded fallback*) di `inpatient-physician-context-header.jsx` baris 153 yang aktif karena data nama penjamin belum disertakan pada jawaban server untuk detail episode.

---

## 6. Temuan Tambahan

| ID | Judul | Area | Keparahan | Status bukti |
| --- | --- | --- | --- | --- |
| `ISS-DOK-004-T1` | Fallback statis "BPJS Kesehatan" di header dokter rawat inap | Frontend | High | SUDAH-VERIFIKASI di `inpatient-physician-context-header.jsx:153` |
| `ISS-DOK-004-T2` | Modul peresepan klinis belum memanggil `CompanyGuarantorCoverageService` | Backend | High | SUDAH-VERIFIKASI di `PrescribingDrugController.cs:156` |

---

## 7. Pertanyaan dan Keputusan yang Dibutuhkan

### Untuk Pelapor:
| No | Pertanyaan | Kenapa ditanyakan | Dampak bila belum dijawab |
| ---: | --- | --- | --- |
| P-01 | Apakah alur peresepan dokter untuk pasien penjamin perusahaan perlu menampilkan kuota/plafon coverage obat di UI katalog resep, atau cukup menampilkan harga tarif RS dan status dijamin oleh perusahaan? | Menentukan apakah preview coverage di UI resep perlu detail limit atau advisory dasar | Solusi default: menggunakan advisory tarif RS dan pencocokan aturan `MstCompanyGuarantorCoverageRule` seperti modul Billing Kasir |

### Untuk Pemilik Modul (Owner):
| No | Keputusan | Pilihan | Rekomendasi | Menahan perbaikan |
| ---: | --- | --- | --- | --- |
| K-01 | Kebijakan pertanggungan obat penjamin perusahaan saat dokter meresepkan | (A) Terapkan aturan `MstCompanyGuarantorCoverageRule` via `CompanyGuarantorCoverageService`<br>(B) Perlakukan seperti Tunai untuk pricing, klaim diserahkan ke kasir | **Pilihan A** (sesuai kontrak `BE-BKC-044` yang sudah berjalan di Billing) | `FIX-DOK-004-04` |
| K-02 | Penanganan tampilan header bila penjamin gagal dibaca | (A) Tampilkan `"-"` atau `"Penjamin Tidak Terdefinisi"`<br>(B) Tampilkan jenis pendaftaran | **Pilihan A** (jangan pernah memunculkan nama penjamin palsu) | `FIX-DOK-004-03` |

---

## 8. Catatan Pola

1. **Pola Penambahan Enum Tanpa Pembaruan Menyeluruh (*Enum Exhaustiveness Drift*):**
   Saat nilai enum `CompanyGuarantor = 3` ditambahkan pada `EncounterPaymentType` di pendaftaran dan billing (`BE-RWI-035` dan `BE-BKC-044`), layanan pendukung di Clinical Management (`EncounterInsuranceService.cs`) tidak ikut diperbarui. Validasi berbasis negasi (`if (type != Insurance) fail`) rentan memblokir nilai enum baru yang sah.
2. **Pola Fallback Hardcoded UI (*Misleading Hardcoded Fallback*):**
   Memberikan string default spesifik seperti `"BPJS Kesehatan"` pada antarmuka pengguna rumah sakit sangat berisiko memicu kesalahan interpretasi klinis dan administratif. Nilai default untuk data identitas/finansial wajib bersifat netral (`"-"`).

---

## 9. Riwayat Dokumen

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-10-06 | Dokumen issue dibuat dari analisis laporan screenshot galat tipe pembayaran encounter dan audit DB PostgreSQL | `diagnose-module-issue` |
| 2026-10-06 | Rencana perbaikan disetujui penuh oleh pemilik; status diubah ke DALAM_PERBAIKAN | Pemilik Sistem (User) |
