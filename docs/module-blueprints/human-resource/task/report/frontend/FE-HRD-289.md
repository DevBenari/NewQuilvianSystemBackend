# Laporan Task FE-HRD-289: Layar Pelacak Riwayat Permohonan Mandiri & Lembar Perbaikan Draf Revisi

## 1. Informasi Task
* **Task ID**: `FE-HRD-289`
* **Slice / MVP**: `S-A7-V2 / Kotak Masuk Persetujuan Terpadu & ESS V2`
* **Nama Fitur**: Layar Pelacak Riwayat Permohonan Mandiri & Lembar Perbaikan Draf Revisi (`VR-WF-06`)
* **Status**: Selesai (`✅ PASS`)
* **Tanggal Selesai**: 2026-11-10

## 2. Ringkasan Implementasi
Membangun pelacak status transparansi permohonan staf dan lembar kerja perbaikan draf revisi:
1. **Layar Pelacak Riwayat Mandiri (`unified-ess-my-requests-view.jsx`)**:
   - Menampilkan seluruh permohonan yang diajukan oleh staf yang sedang login.
   - Menyediakan filter status: Semua, Sedang Diproses (*In Review*), Perlu Revisi (*Returned*), Disetujui (*Approved*), dan Ditolak (*Rejected*).
   - Menampilkan visual progress tracker rantai persetujuan untuk setiap tiket.
   - Tombol unduh berkas bukti persetujuan sah format PDF bagi permohonan yang telah disetujui final.
2. **Lembar Perbaikan Draf Revisi (`unified-ess-revision-view.jsx` - `VR-WF-06`)**:
   - Khusus menangani permohonan yang dikembalikan atasan dengan status *Returned*.
   - Menampilkan kotak instruksi atasan secara jelas (aspek apa yang kurang dan arahan perbaikan).
   - Menyediakan formulir pengeditan data permohonan dan fasilitas unggah ulang berkas lampiran pendukung.
   - Tombol aksi "Ajukan Ulang Permohonan" (*Resubmit Request*) yang mengubah status kembali ke *Submitted* dan mengalirkan tiket ke antrean atasan tanpa membatalkan riwayat sebelumnya.
3. **Pendaftaran Rute Next.js App Router**:
   - `/hrd/self-service/my-requests` & `/corporate/hrd/self-service/my-requests`: Rute pelacak riwayat mandiri.
   - `/hrd/self-service/revisions/[id]` & `/corporate/hrd/self-service/revisions/[id]`: Rute formulir revisi permohonan.

## 3. Berkas yang Diubah / Dibuat
* `QuilvianSystemFrontendDev/src/components/view/hr/approvals/unified-ess-my-requests-view.jsx`
* `QuilvianSystemFrontendDev/src/components/view/hr/approvals/unified-ess-revision-view.jsx`
* `QuilvianSystemFrontendDev/src/app/(dashboard)/hrd/self-service/my-requests/page.jsx`
* `QuilvianSystemFrontendDev/src/app/(dashboard)/corporate/hrd/self-service/my-requests/page.jsx`
* `QuilvianSystemFrontendDev/src/app/(dashboard)/hrd/self-service/revisions/[id]/page.jsx`
* `QuilvianSystemFrontendDev/src/app/(dashboard)/corporate/hrd/self-service/revisions/[id]/page.jsx`

## 4. Bukti Verifikasi & Hasil Pengujian
* **Hasil Verifikasi**: Staf dapat memantau posisi tiket secara transparan di pohon approver; formulir revisi berhasil mengedit data dan mengajukan ulang draf dengan aman.
* **Kepatuhan Aturan Bisnis**: Menegakkan kepatuhan mekanisme perbaikan permohonan non-destruktif `VR-WF-06`.
