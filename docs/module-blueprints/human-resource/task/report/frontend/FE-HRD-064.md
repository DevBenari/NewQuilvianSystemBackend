# Laporan Perubahan Frontend — `FE-HRD-064`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `FE-HRD-064` |
| Judul | Keputusan Kontrak (Penandatanganan, Perpanjangan PKWT, Pengangkatan PKWTT, PHK) & Koreksi |
| Slice | `S-C4-ONB (Onboarding dan Kontrak Kerja Pegawai RS) · MVP-16` |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian Gelombang 3 S-C4-ONB |
| Trace | `HRD-DEC-091, HRD-DEC-092, HRD-DEC-093, VR-ONB-35 s.d. VR-ONB-54, BE-HRD-080, BE-HRD-081` |
| Contract version | API Contract v9 bagian 15.2 (sign, extend, permanent, terminate, correct) |
| Dependency | `FE-HRD-063 (✅ Selesai), BE-HRD-080 (✅ Selesai), BE-HRD-081 (✅ Selesai)` |
| Klasifikasi | `HIGH` |
| Task mode | `FRONTEND` |
| Target tulis | `src/components/view/hr/lifecycle/employment-contract-detail-view.jsx` |
| Branch frontend | `HamzahV2` |
| Tanggal | 09 November 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Eksekusi tindakan formal hukum ketenagakerjaan pada kontrak kerja pegawai dengan pengawasan batas kumulatif 5 tahun PKWT sesuai PP No. 35/2021.

Task ini mengimplementasikan:
1. **Penandatanganan Kontrak (Sign Contract)**:
   - Mencatat tanggal tanda tangan kedua belah pihak dan nomor registrasi arsip hukum RS.
2. **Perpanjangan Kontrak PKWT (Extend PKWT)**:
   - Validasi kumulatif durasi: Menghitung total durasi masa kerja PKWT terdahulu ditambah perpanjangan baru. Jika total melebihi 60 bulan (5 tahun), form menampilkan peringatan keras (VR-ONB-35 s.d. VR-ONB-44) dan mengarahkan ke pengangkatan PKWTT.
3. **Pengangkatan Menjadi Pegawai Tetap (Promote to PKWTT)**:
   - Menerbitkan SK Direksi pengangkatan pegawai tetap, mengakhiri siklus kontrak berjangka secara otomatis.
4. **Pemutusan Hubungan Kerja (Termination)**:
   - Pilihan alasan formal (habis masa kontrak, pengunduran diri, pensiun, pelanggaran disiplin berat) disertai unggahan surat keputusan/berita acara.
5. **Koreksi Administratif (Correction)**:
   - Hanya dapat diakses oleh Administrator HRD dengan pencatatan alasan koreksi (audit trail).

---

## 1. Bukti Verifikasi & Acceptance Criteria

| No | Kriteria Penerimaan (Acceptance Criteria) | Status | Bukti Implementasi pada Source Code |
| :-: | :--- | :---: | :--- |
| 1 | Peringatan batas akumulasi 5 tahun PKWT dihitung otomatis (VR-ONB-37) | ✅ PASS | Fungsi kalkulasi durasi kumulatif pada modal perpanjangan kontrak mendeteksi batas 60 bulan. |
| 2 | Aksi penandatanganan dan pengangkatan terhubung ke endpoint API | ✅ PASS | Terhubung ke LifecycleService.signContract, extendContract, promoteToPermanent. |
| 3 | Pemutusan kontrak mewajibkan pengisian alasan dan lampiran SK | ✅ PASS | Form pemutusan kontrak memvalidasi form field secara komprehensif. |
| 4 | Koreksi administratif mencatat audit log perubahan | ✅ PASS | Endpoint correctContract dipanggil dengan payload catatan revisi. |

---

## 2. Rincian Berkas yang Dibuat / Diubah

- `src/components/view/hr/lifecycle/employment-contract-detail-view.jsx (Modal interaktif sign, extend, permanent, terminate, correct)`
