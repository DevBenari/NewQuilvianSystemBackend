# Laporan Task FE-RWI-197: Tolak Order Operasi di Layar OK

| Metadata | Nilai |
|---|---|
| **Task ID** | `FE-RWI-197` |
| **Layar Blueprint** | `FE-INP-33` (Tolak Order Operasi pada Detail Kasus & Laporan OK) |
| **Status** | Selesai (Completed) |
| **Tanggal Selesai** | 5 Oktober 2026 |
| **Pelaksana** | Muhammad Hamzah (Agentic Pair Programming) |
| **Requirement Terkait** | `FR-RWF-086`, `RWI-DEC-204`, `RWI-DEC-208`, `AC-RWF-085`, `099`, `UAT-RWF-24` |
| **Dependency** | `BE-RWI-174` |

---

## 1. Ringkasan Pekerjaan
Mengimplementasikan kemampuan bagi petugas penjadwalan Kamar Operasi (IBS) untuk menolak kasus operasi berstatus Diminta (`Requested`) dengan alasan wajib (10–500 karakter). Kasus yang ditolak menampilkan status Ditolak berlabel merah (`danger`) lengkap dengan alasan penolakan, nama penolak, dan waktu penolakan. Semua tombol ubah dan penjadwalan disembunyikan untuk kasus yang ditolak. Kolom status laporan operasi juga membedakan status Ditolak dari status Dibatalkan.

---

## 2. Berkas yang Dibuat dan Diubah

1. **`src/lib/services/health-services/operating-room-management/operating-room-case.service.js`**
   - Menambahkan fungsi `rejectOperatingRoomCase(caseId, payload, config)` yang memanggil `PATCH /v1/health-services/operating-room-management/cases/{caseId}/reject` dengan header `Idempotency-Key`.
2. **`src/lib/constants/health-services/operating-room-management/operating-room-case-constant.jsx`**
   - Menambahkan status `OPR_CASE_STATUS.REJECTED = 8`, label `"Ditolak"`, dan varian warna badge `"danger"`.
3. **`src/utils/health-services/operating-room-management/operating-room-case-utils.jsx`**
   - Mendukung resolusi status string maupun enum integer pada `getCaseStatusLabel` dan `getCaseStatusVariant`.
4. **`src/components/view/health-services/operating-room-management/operating-room-case-view/components/operating-room-case-reject-modal.jsx`**
   - Modal konfirmasi penolakan dengan validasi alasan 10–500 karakter, counter karakter, penanganan error server, dan pengiriman request penolakan.
5. **`src/components/view/health-services/operating-room-management/operating-room-case-view/operating-room-case-detail-view.jsx`**
   - Tombol "Tolak Permintaan" tampil pada Hero actions hanya bila kasus berstatus Diminta dan pengguna memegang `OperatingRoomCase : Reject`.
   - Menampilkan kartu rincian penolakan berlatar merah lembut dengan rincian alasan penolakan, penolak, dan waktu penolakan saat kasus berstatus `Rejected`.
   - Menyembunyikan seluruh tindakan lanjutan dan tombol ubah pada kasus `Rejected`.
6. **`tests/unit/operating-room-reject-case.test.mjs`**
   - Unit test otomatis memvalidasi service, modal dialog, izin hak akses, dan status konstanta.

---

## 3. Bukti Verifikasi

### AUTOMATED TEST
```bash
cmd.exe /c node --test tests/unit/operating-room-reject-case.test.mjs
```
Hasil:
- ✔ FE-RWI-197 AC-1: Service memuat fungsi rejectOperatingRoomCase memanggil PATCH cases/{id}/reject
- ✔ FE-RWI-197 AC-2: Modal dialog penolakan memvalidasi panjang alasan 10–500 karakter
- ✔ FE-RWI-197 AC-3: Detail kasus menyediakan tombol Tolak bagi OperatingRoomCase:Reject pada status Diminta, dan menyembunyikan tombol ubah pada status Ditolak
- ✔ FE-RWI-197 AC-4: Konstanta mendukung status REJECTED dengan label Ditolak dan varian danger
- ℹ tests 4, pass 4, fail 0 (PASS)

### MANUAL TEST
- Tombol Tolak hanya muncul pada kasus Diminta bagi user dengan hak `OperatingRoomCase:Reject`.
- Validasi alasan minimal 10 karakter mencegah penolakan tanpa alasan yang jelas.
- Kasus yang telah ditolak mengunci seluruh pengubahan jadwal dan memberi penjelasan jelas kepada staf medis.
