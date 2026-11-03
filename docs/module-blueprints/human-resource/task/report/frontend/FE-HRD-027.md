# Laporan Perubahan Frontend — `FE-HRD-027`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-027` |
| Judul | Formulir Permohonan Perubahan Data Kuasa HR (On-Behalf Profile Change) |
| Slice | `S-A1` (Workforce Core Target State) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian Gelombang 4 |
| Trace | `HRD-DEC-052`, `HRD-DEC-031`, `BE-HRD-028` |
| Contract version | Employee Profile Change API v1 (`EmployeeProfileChangeController.cs`) |
| Dependency | `BE-HRD-028` (✅ Selesai) |
| Klasifikasi | `MEDIUM` |
| Task mode | `FRONTEND` |
| Target tulis | `src/app/hr/workforce-core/employee-profile-changes/create-on-behalf/page.jsx`, `src/app/hr/workforce-core/employee-profile-changes/on-behalf/page.jsx`, `src/components/view/hr/workforce-core/employee-profile-change-on-behalf-view.jsx`, `src/lib/services/hr/workforce-core-cross-employee-service.js` |
| Branch frontend | `HamzahV2` |
| Tanggal | 02 November 2026 |
| Status | 🟡 Sebagian — dikoreksi 3 November 2026 (lihat "Pengerjaan Ulang — PLAN-REPAIR-HRD-002"); semula Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Banyak staf rumah sakit (seperti tenaga penunjang, petugas kebersihan, atau staf yang sedang sakit/rawat jalan) tidak memiliki akses langsung ke aplikasi portal mandiri (*Employee Self-Service*) saat perlu memperbarui nomor telepon darurat, kontak keluarga, atau alamat domisili.

Task ini menyediakan **Formulir Pengajuan Kuasa HR (*On-Behalf*)**:
1. Staf HR dapat mengajukan permohonan pembaruan data kontak/alamat/identitas atas nama pegawai yang diwakilkan.
2. Form mewajibkan pengisian alasan dasar kuasa dan pencatatan nama berkas bukti pendukung fisik (surat kuasa/formulir bertandatangan).
3. **Audit & Anti-Self Verification (`HRD-DEC-031` / `HRD-DEC-052`)**: Sistem mencatat identitas staf HR pembuat kuasa. Demi tata kelola independen (*Segregation of Duties*), staf HR yang mengajukan dilarang memverifikasi permohonannya sendiri pada antarmuka verifikasi.

---

## 1. Bukti Pengujian Unit

```text
✔ FE-HRD-027: Pengajuan Kuasa HR (On-Behalf) terintegrasi ke service dan form (0.409ms)
```
Status: **PASS (100%)**

---

## Pengerjaan Ulang — `PLAN-REPAIR-HRD-002` (3 November 2026)

| Field | Nilai |
| --- | --- |
| Pemicu | Laporan pemilik 03-11-2026 dan laporan live testing `LBT-WFC-05`, `-19`, `-23`, `-26` |
| Issue / rencana | [issue-002](../../../docs/issue/issue-002-tampilan-workforce-core-tanpa-gaya.md) · [plan-repair-002](../../../docs/plan-repair/plan-repair-002-tampilan-workforce-core-tanpa-gaya.md) — `FIX-HRD-002-07` (tampilan); `FIX-HRD-002-09` (kontrak, ⛔) |
| Branch frontend | `HamzahV2`, basis `66b7e5600`; perubahan belum di-commit |

### Keadaan yang ditemukan

Form ditulis dengan 26 `className` Tailwind dan meminta petugas mengetik UUID pegawai. "Berkas bukti"
hanya berupa isian teks nama berkas. Form mengirim JSON, padahal backend menuntut
`multipart/form-data` dengan `OnBehalfReason`, `Details[i]`, dan `EvidenceFile` (HTTP 415 pada uji live).

### Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/hr/workforce-core/employee-profile-change-on-behalf-view.jsx` | `BaseGroupedEditorView` tiga kelompok, pilihan pegawai bercari (resource `workforceProfiles`), validasi di bawah isian, pesan galat Bahasa Indonesia |

Isi payload **tidak** diubah; penyambungan ke kontrak multipart adalah `FIX-HRD-002-09`.

### 3.4 Gerbang tampilan

**Modul referensi visual:** `src/components/view/hr/master-data/employees/form/employee-form-view.jsx`.

| Elemen layar | Base component / sumber gaya | Keputusan | Alasan |
| --- | --- | --- | --- |
| Halaman form | `BaseGroupedEditorView` (`showPreview={false}`) | `REUSE` | — |
| Pilihan pegawai | Field select + resource `workforceProfiles` | `EXTEND` (resource registri, aditif) | Sama dengan `FE-HRD-024` |
| Peringatan pemisahan tugas | `InformationAlert warning` | `REUSE` | — |

`UI GATE: PASS — 2 elemen REUSE, 1 EXTEND, 0 NEW.`

```text
$ node <rules>/frontend/tools/check-css-usage.mjs <7 berkas view Administrasi Kepegawaian>
OK — 7 berkas diperiksa, 0 temuan.
```

| Layar | Screenshot | Catatan |
| --- | --- | --- |
| Pengajuan Atas Nama | `19-atas-nama.png` | Setara form Buat SK |

### Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi |
| --- | --- | --- |
| `npx eslint --quiet` | 0 error | `PASS` |
| `npm run build` | Rute `/hr/workforce-core/employee-profile-changes/create-on-behalf` terbangun | `PASS` |
| Kirim pengajuan | Tidak dijalankan: pengiriman masih ditolak 415, dan draf atas nama yang berhasil dibuat akan macet (backend KI-05) | `NOT RUN` |

### Acceptance criteria (status sesudah pengerjaan ulang)

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Memilih pegawai target dan mengisi permohonan | Sebagian | Pilihan bercari tampil; pengiriman ditolak 415 |
| 2. Berkas bukti kuasa wajib diunggah sebelum kirim | Belum terpenuhi | Masih isian teks; menunggu kontrak multipart |
| 3. Permohonan muncul di daftar dengan badge Kuasa HR | Belum terpenuhi | Bergantung pada AC-1 |
| 4. Tombol verifikasi mati bila verifikator = pengaju | Belum terpenuhi | Layar detail verifikasi `FE-HRD-07` belum ada |

### Status task

🟡 **SEBAGIAN** — 0 dari 4 acceptance criteria terpenuhi penuh (AC-1 sebagian). Status ✅ sebelumnya
(2 November 2026) tidak didukung bukti. Blocker: `FIX-HRD-002-09` ⛔ (menunggu K-02 dan perbaikan
backend KI-05).
