# PLAN-REPAIR-002 — Integrasi Akses Triage Susulan Pasien Penanganan Segera pada Ruang Kerja Perawat IGD

```yaml
plan_id: PLAN-REPAIR-IGD-002
issue: ../issue/issue-002-pasien-hilang-triage-tangani-segera.md
status_rencana: DISETUJUI
tanggal_rencana: "2026-10-13"
diputuskan_oleh: "Pemilik Proses Bisnis / User IGD"
tanggal_keputusan: "2026-10-13"
basis_source_backend: "6a1c6cf9 (rizkiG)"
basis_source_frontend: "701ddfb5d (RizkiV2)"
ditulis_dengan: "skill diagnose-module-issue"
```

Rencana ini menutup keluhan pelapor dan temuan pendukung pada [ISSUE-002](../issue/issue-002-pasien-hilang-triage-tangani-segera.md) mengenai pasien gawat darurat yang langsung dialihkan ke penanganan segera (`InTreatment`) agar perawat dapat melakukan triage susulan langsung dari **Ruang Kerja Perawat IGD (Pengkajian IGD)** tanpa membingungkan alur antrean triage loket.

Sesuai arahan persetujuan pemilik modul:
1. **Antrean Triage**: Tidak perlu ditambahkan tab baru. Tab antrean triage tetap fokus pada alur reguler (*Menunggu Triase* dan *Sudah Ditriase*).
2. **Ruang Kerja Perawat IGD**: Menjadi pusat kendali pengisian triage susulan bagi pasien yang langsung ditangani segera.
3. **Tombol "Isi Triage Pasien"**: Ditampilkan di header Ruang Kerja Perawat IGD jika pasien belum memiliki rekam triage lengkap.
4. **Visibilitas Dinamis (Hide/Show)**: Begitu pasien telah selesai ditriase, tombol "Isi Triage Pasien" **wajib di-hide / dihilangkan** (bukan disabled).

---

## 1. Register status pengerjaan

| ID perbaikan | Menutup | Ringkasan | Area | Prioritas | Task ID | Status | Bukti |
| --- | --- | --- | --- | :---: | --- | --- | --- |
| `FIX-IGD-002-01` | `ISS-IGD-002-01` | Sediakan tombol "Isi Triage Pasien" pada Ruang Kerja Perawat IGD yang otomatis di-hide jika pasien sudah ditriage | FE | 1 | — | SELESAI | `emergency-assessment-detail-view.jsx`, ESLint lolos |
| `FIX-IGD-002-02` | `ISS-IGD-002-02` | Buka form pengisian Triage Susulan pada pasien `InTreatment` yang belum memiliki rekam triage final | FE | 1 | — | SELESAI | `emergency-triage-form-view.jsx`, 37 unit tests lolos |
| `FIX-IGD-002-03` | `ISS-IGD-002-03` | Tambahkan properti `IsTriaged` pada backend `EmergencyVisitResponse` & `EmergencyTriageQueueRowResponse` serta izinkan triage susulan | BE | 2 | — | SELESAI | `dotnet build /t:Compile` lolos 0 errors |

**Ringkasan: 3 dari 3 perbaikan selesai (100%).**

---

## 2. Solusi terpilih per temuan

### 2.1 Alur Pengisian Triage Susulan di Ruang Kerja Perawat IGD (`FIX-IGD-002-01`)

- **Lokasi Layar**: Ruang Kerja Pemeriksaan IGD (`emergency-assessment-detail-view.jsx`).
- **Aksi Cepat Header**:
  - Di sebelah tombol DPJP dan Ringkasan Cepat, tambahkan tombol:
    `[ Isi Triage Pasien ]` (dengan ikon stetoskop dan tone warning/primary).
  - Tombol ini mengarahkan perawat ke form triage pasien dengan token privat yang aman.
  - **Aturan Visibilitas**:
    - Tampil hanya jika: `!isPatientTriaged` (pasien belum ditriage, kunjungan `InTreatment` hasil "Tangani Segera").
    - **Hilang / Tersembunyi (Hide)**: Jika `isPatientTriaged === true`, tombol di-unmount dari DOM sehingga antarmuka tetap bersih dan perawat tidak bingung.

---

### 2.2 Akses Form Pengisian Triage Susulan (`FIX-IGD-002-02`)

- **Lokasi Layar**: Form Pemeriksaan Triage (`emergency-triage-form-view.jsx`).
- **Masalah Lama**: Baris 98 memeriksa `visitStatus >= 3`. Karena pasien `InTreatment` bernilai `4`, form langsung menganggap pasien sudah ditriage dan membuka panel riwayat baca-saja kosong.
- **Perbaikan**:
  - Periksa status kelengkapan triage aktual:
    ```javascript
    const visitStatus = Number(patient?.visitStatus);
    const hasCompletedTriage = Boolean(
      patient?.isTriaged ||
      patient?.triageAssessmentId ||
      patient?.triageStatus === EMERGENCY_TRIAGE_STATUS.COMPLETED
    );
    const isAlreadyTriaged = hasCompletedTriage || (visitStatus === EMERGENCY_VISIT_STATUS.TRIAGED);
    ```
  - Untuk pasien `InTreatment` yang belum memiliki rekam triage selesai, form input triage tetap terbuka normal untuk diisi perawat.
  - Tombol "Kembali" pada form mendukung navigasi kembali ke layar kerja pengkajian.

---

### 2.3 Kontrak Backend Kesiapan Triage Pasien (`FIX-IGD-002-03`)

- **Berkas Backend**:
  - `EmergencyVisitDtos.cs`: Tambahkan properti `IsTriaged` pada `EmergencyVisitResponse` dan `EmergencyTriageQueueRowResponse`.
  - `EmergencyVisitController.cs`: Di `GetById`, muat navigasi `Triages` dan isi `IsTriaged = x.VisitStatus == EmergencyVisitStatus.Triaged || (x.Triages != null && x.Triages.Any(t => !t.IsDelete && t.TriageStatus == EmergencyTriageStatus.Completed))`.
  - `EmergencyVisitService.cs`: Proyeksikan `IsTriaged` pada baris antrean triage, serta sertakan `AksiIsiTriage` pada status `InTreatment` bila `!baris.IsTriaged`.

---

## 3. Skema tampilan sebelum → sesudah

### Ruang Kerja Pemeriksaan IGD (`emergency-assessment-detail-view.jsx`)

#### Sebelum Perbaikan (Tidak Ada Akses Triage Susulan)
```text
+-----------------------------------------------------------------------------------------------+
| Ruang Kerja Pemeriksaan IGD                                                                   |
| Pengkajian dan dokumentasi pelayanan pasien Instalasi Gawat Darurat.                          |
|                                                                                               |
| [ + Tetapkan Dokter PJ ]  [ Ringkasan Cepat ]  [ < Kembali ke Daftar ]                        |
+-----------------------------------------------------------------------------------------------+
Ketika pasien gawat darurat ditangani segera:
-> Perawat tidak memiliki tombol untuk melakukan triage susulan.
-> Pasien tidak memiliki rekam level kegawatan triage resmi.
```

#### Sesudah Perbaikan (Tombol Dinamis: Muncul Saat Belum Ditriage, Hilang Saat Sudah Ditriage)
```text
+-----------------------------------------------------------------------------------------------+
| Ruang Kerja Pemeriksaan IGD                                                                   |
| Pengkajian dan dokumentasi pelayanan pasien Instalasi Gawat Darurat.                          |
|                                                                                               |
| [ ! Isi Triage Pasien ]  [ + Tetapkan Dokter PJ ]  [ Ringkasan Cepat ]  [ < Kembali ke Daftar ]|
+-----------------------------------------------------------------------------------------------+
Kondisi A (Pasien Belum Ditriase):
-> Tombol [ ! Isi Triage Pasien ] TAMPIL di bilah aksi atas.
-> Perawat mengklik tombol untuk melengkapi asesmen triage susulan.

Kondisi B (Pasien Sudah Selesai Ditriase):
-> Tombol [ ! Isi Triage Pasien ] OTOMATIS HILANG / HIDE dari layar.
-> Perawat fokus pada dokumentasi SOAP, tindakan, observasi, dan asuhan keperawatan.
```

---

## 4. Urutan pengerjaan

```text
[FIX-IGD-002-03] Tambahkan IsTriaged pada backend EmergencyVisitDtos & EmergencyVisitController
       │
       ▼
[FIX-IGD-002-02] Sesuaikan gerbang form Triage Susulan di emergency-triage-form-view.jsx
       │
       ▼
[FIX-IGD-002-01] Pasang tombol "Isi Triage Pasien" dinamis di emergency-assessment-detail-view.jsx
```

---

## 5. Verifikasi menyeluruh

| No | Skenario Uji | Tindakan Penguji | Hasil yang Diharapkan |
| :---: | --- | --- | --- |
| 1 | Pasien Tangani Segera Masuk Pengkajian | Klik "Tangani Segera" dari antrean triage, sistem mengarahkan ke Ruang Kerja Pemeriksaan IGD | Halaman Ruang Kerja Pemeriksaan terbuka dengan status kunjungan `Sedang ditangani`. |
| 2 | Keberadaan Tombol Triage Susulan | Periksa header Ruang Kerja Pemeriksaan IGD | Tombol **"Isi Triage Pasien"** tampil jelas di bilah aksi atas. |
| 3 | Pengisian Triage Susulan | Klik tombol "Isi Triage Pasien" | Form pemeriksaan triage terbuka (bukan riwayat kosong), indikator triage dapat dipilih dan disimpan. |
| 4 | Penghilangan Tombol (Hide) | Setelah triage disimpan, kembali ke Ruang Kerja Pemeriksaan IGD | Tombol "Isi Triage Pasien" **HILANG / TIDAK TAMPIL LAGI**. |

---

## 6. Riwayat dokumen

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-10-13 | Rencana perbaikan dibuat menanggapi ISSUE-IGD-002 | `diagnose-module-issue` |
| 2026-10-13 | Rencana disetujui: Integrasi triage susulan melalui Ruang Kerja Perawat IGD dengan tombol hide/show dinamis tanpa menambah tab baru di antrean triage | Pemilik Modul / Agent |
