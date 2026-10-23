# ISSUE-034 — Tidak Ditampilkannya Kartu Identitas dan Nomor Rekam Medis Bayi pada Langkah 7 (Dokumen Bayi)

```yaml
issue_id: ISSUE-EPS-034
module_id: rawat-inap
submodule: episode-rawat-inap
blueprint_id: RWI-BP-001
layar: "Admisi Rawat Inap — Langkah 7 Dokumen Bayi (FE-INP-026)"
sumber_laporan: "Laporan pengguna 22 Oktober 2026: 'kenapa nomor rekam medis tidak terbit pada saat melakukan admisi bayi ?' — Petugas mengira Nomor Rekam Medis bayi belum terbit karena Langkah 7 hanya menampilkan kartu 'Waktu layanan' tanpa kartu identitas pasien bayi dan No. RM."
tanggal_issue: "2026-10-22"
status: SELESAI
tanggal_selesai: "2026-10-22"
keparahan_tertinggi: Sedang
source_sha_backend: "6406fc76 (MHamzah)"
source_sha_frontend: "381ec25fa (HamzahV2)"
rencana_perbaikan: ../plan-repair/plan-repair-034-kartu-identitas-dan-no-rm-bayi-langkah-dokumen.md
```

---

## 1. Ringkasan

Setelah menyelesaikan 6 langkah admisi bayi baru lahir dan menekan tombol *"Aktifkan Admisi Bayi"* pada Langkah 6, sistem berhasil mengaktifkan episode rawat inap bayi dan mengarahkan petugas ke Langkah 7 ("Dokumen Bayi"). 

Secara basis data, nomor rekam medis bayi **sudah berhasil dibuat dan diterbitkan** (misalnya `00-00-00-47` untuk pasien `By. Ny. DEWI ANGGRAINI`). Namun, antarmuka Langkah 7 (`NewbornDocumentsStep`) **tidak menampilkan kartu ringkasan identitas pasien bayi**. Layar hanya menyajikan satu kartu berjudul *"Waktu layanan"* yang berisi tanggal lahir dan waktu penempatan, tanpa menyebutkan Nomor Rekam Medis (No. RM) bayi, nama bayi, nama ibu, maupun nomor episode admisi.

Hal ini menimbulkan kebingungan bagi petugas admisi dan perawat yang mengira bahwa sistem gagal menerbitkan nomor rekam medis bayi.

---

## 2. Bukti Data Basis Data

Pemeriksaan langsung pada basis data `QuilvianNewDevHamzah`:
- **Tabel `public."MstPatient"`**:
  - `Id`: `bfda657d-3113-4220-8913-457a92bb18bb`
  - `MedicalRecordNumber`: **`00-00-00-47`** ✅
  - `FullName`: `By. Ny. DEWI ANGGRAINI`
  - `BirthOrder`: `6`
  - `IsNewborn`: `true`
- **Tabel `public."InpEpisode"`**:
  - `Id`: `3f9d3779-48fd-44e6-bce0-9f330307bb46`
  - `EpisodeNumber`: `RI-261022035305-26C199`
  - `EpisodeStatus`: `1 (Active / Admitted)`

Data rekam medis dan penempatan bayi telah tersimpan lengkap dan valid di server.

---

## 3. Akar Masalah UI/UX

Pada berkas `src/components/view/health-services/inpatient-management/inpatient-newborn-admission-steps.jsx` komponen `NewbornDocumentsStep`:
```javascript
export function NewbornDocumentsStep({ patient, confirmation, doctor, flow }) {
  const episode = confirmation.episode || doctor.episode;
  ...
  return <div className={styles.flowPanel}>
    <Heading title="Dokumen Bayi">Gelang dan label memakai identitas bayi yang telah tersimpan. Cetak ulang tidak mengulang admisi.</Heading>
    <BaseDetailCard title="Waktu layanan" item={episode || {}} showAudit={false} rows={[
      { label: "Waktu lahir", value: `${String(patient.selectedPatient?.birthDate || "").slice(0, 10)} ${patient.selectedPatient?.birthTime || ""}`.trim() },
      { label: "Draft admisi dibuat", value: formatDateTime(episode?.createDateTime) || "Belum dapat diverifikasi" },
      { label: "Penempatan aktif", value: formatDateTime(episode?.admittedAt) || "Belum dapat diverifikasi" },
    ]} />
    ...
```

Komponen ini hanya merender kartu `Waktu layanan`. Kartu identitas pasien bayi (`patient.selectedPatient?.medicalRecordNumber`, `fullName`, `birthOrder`, `motherName`, `episodeNumber`) yang sebelumnya ada di Langkah 6 (`NewbornReviewStep`) dihilangkan pada Langkah 7.

---

## 4. Rencana Solusi

1. Tambahkan banner konfirmasi keberhasilan admisi (`InformationAlert` variant `success`) di bagian atas Langkah 7 yang menegaskan bahwa nomor rekam medis telah resmi terbit.
2. Tambahkan kartu utama **"Identitas Pasien Bayi & Rekam Medis"** (`BaseDetailCard`) lengkap dengan badge status No. RM, nama bayi, urutan kelahiran, nama & RM ibu, nomor episode, serta kamar penempatan aktif.
3. Pertahankan kartu waktu layanan dan panel cetak gelang/label di bawahnya.
4. Lakukan verifikasi pengujian unit otomatis.

---

## 5. Riwayat Dokumen

| Tanggal | Perubahan | Oleh |
| :--- | :--- | :--- |
| 2026-10-22 | Dokumen issue dibuat berdasarkan laporan keluhan pengguna terkait No. RM bayi yang tidak terlihat di Langkah 7. | `diagnose-module-issue` |
| 2026-10-22 | Status ditutup SELESAI: Kartu identitas pasien bayi & No. RM serta banner konfirmasi sukses resmi ditambahkan di `NewbornDocumentsStep`. 15/15 unit test lulus. | `build-module-frontend` |
