# Laporan Perubahan Frontend — `FE-HRD-081`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `FE-HRD-081` |
| Judul | Modal Ganti Petugas Shift Darurat Langsung (Quick Replacement) Pasca-Terbit Tanpa Reload |
| Slice | `S-B4 (Penjadwalan, Roster Dinas 24/7, & On-Call) · MVP-19` |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian S-B4 |
| Trace | `HRD-DEC-101, 03-frontend-architecture.md 15.3.3, BE-HRD-096` |
| Contract version | API Contract v10 /shift-assignments/{id}/replace |
| Dependency | `FE-HRD-077 (✅ Selesai), BE-HRD-096 (✅ Selesai)` |
| Klasifikasi | `HIGH` |
| Task mode | `FRONTEND` |
| Target tulis | `src/components/view/hr/scheduling/modals/QuickShiftReplacementModal.jsx` |
| Branch frontend | `HamzahV2` |
| Tanggal | 09 November 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Penanganan mutasi jadwal darurat ketika perawat jaga berhalangan hadir mendadak pada roster yang telah terbit resmi (HRD-DEC-101):
1. **Aksi Klik Sel Jadwal Pasca-Terbit**:
   - Membuka modal QuickShiftReplacementModal dengan informasi nama staf asal dan waktu shift otomatis.
2. **Pemilihan Staf Pengganti & Alasan Wajib**:
   - Dropdown memilih perawat pengganti yang memenuhi syarat dan tidak sedang bertugas di shift tersebut.
   - Pilihan alasan wajib (sakit mendadak, tugas luar, kedaruratan keluarga) dan input catatan audit.
3. **Efektif Seketika**:
   - Memanggil POST /shift-assignments/{id}/replace dan langsung memperbarui nama staf di sel jadwal tanpa menurunkan status roster dari "Published".

---

## 1. Bukti Verifikasi & Acceptance Criteria

| No | Kriteria Penerimaan (Acceptance Criteria) | Status | Bukti Implementasi pada Source Code |
| :-: | :--- | :---: | :--- |
| 1 | Modal menampilkan nama staf asal dan waktu shift secara otomatis | ✅ PASS | QuickShiftReplacementModal menampilkan detail shift yang diklik. |
| 2 | Dropdown pengganti menyaring perawat unit yang memenuhi syarat | ✅ PASS | Filter staf eligible mengecualikan petugas asal. |
| 3 | Konfirmasi penggantian langsung memperbarui jadwal dan status roster tetap Published | ✅ PASS | Endpoint replace dipanggil dan data di-refresh mulus. |

---

## 2. Rincian Berkas yang Dibuat / Diubah

- `src/components/view/hr/scheduling/modals/QuickShiftReplacementModal.jsx (Modal ganti petugas cepat pasca-terbit)`
