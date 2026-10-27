# ISSUE-033 — Langkah 4 Penempatan Bayi Terjebak di Tampilan Booking ("Tempat Tidur Belum Dipilih") Akibat Evaluasi resumeEpisodeId

```yaml
issue_id: ISSUE-EPS-033
module_id: rawat-inap
submodule: episode-rawat-inap
blueprint_id: RWI-BP-001
layar: "Admisi Rawat Inap — Langkah 4 Penempatan dan Pemesanan Bayi (FE-INP-026)"
sumber_laporan: "Laporan pengguna 22 Oktober 2026: Muncul peringatan 'Tempat tidur belum dipilih. Tempat tidur yang dipilih sudah tidak tersedia untuk pasien ini. Kembali ke langkah Pilih Bed lalu pilih tempat tidur lain.' saat melanjutkan admisi bayi (RI-261022032121-D7BE66) dan tombol Kembali melempar ke Langkah 3 tanpa bisa memilih denah bed."
tanggal_issue: "2026-10-22"
status: SELESAI
tanggal_selesai: "2026-10-22"
keparahan_tertinggi: Kritis
source_sha_backend: "6406fc76 (MHamzah)"
source_sha_frontend: "381ec25fa (HamzahV2)"
rencana_perbaikan: ../plan-repair/plan-repair-033-langkah-penempatan-bayi-terjebak-di-booking-tanpa-bed.md
```

---

## 1. Ringkasan

Ketika petugas melanjutkan proses admisi bayi baru lahir (atau setelah draf episode berhasil dibuat pada Langkah 3), sistem masuk ke Langkah 4 ("Penempatan dan Pemesanan"). Namun, layar tidak menampilkan denah lantai dan daftar pemilihan kamar tempat tidur (`InpatientAdmissionBedSelectionStep`), melainkan langsung melompat ke sub-langkah pemesanan (`InpatientAdmissionBedBookingStep`).

Karena petugas belum sempat memilih tempat tidur pada draf tersebut (`selectedBed` masih `null`), sub-langkah pemesanan menampilkan peringatan:
> *"Tempat tidur belum dipilih. Tempat tidur yang dipilih sudah tidak tersedia untuk pasien ini. Kembali ke langkah Pilih Bed lalu pilih tempat tidur lain."*

Lebih lanjut, ketika petugas menekan tombol **"Kembali"**, sistem memanggil `flow.goBack` yang mengembalikan petugas ke **Langkah 3 ("Perawatan dan DPJP Bayi")**, bukan ke pemilihan tempat tidur di Langkah 4. Saat petugas menekan tombol lanjut dari Langkah 3, sistem kembali masuk ke sub-langkah booking kosong tersebut, menyebabkan petugas terjebak dalam *infinite loop* tanpa ada jalan untuk memilih tempat tidur bayi.

---

## 2. Tangkapan Layar dan Bukti Laporan

- **Nomor Draf Episode**: `RI-261022032121-D7BE66`
- **Banner Atas**:
  > *"Melanjutkan admisi RI-261022032121-D7BE66. Admisi dilanjutkan. Episode ini sedang tidak memegang pemesanan tempat tidur, jadi pilih tempat tidurnya kembali. Pemesanan sebelumnya, bila memang pernah ada, sudah tidak berlaku."*
- **Konten Layar**:
  > *"LANGKAH BOOKING BED — Pesan Tempat Tidur. Tempat tidur belum dipilih. Tempat tidur yang dipilih sudah tidak tersedia untuk pasien ini. Kembali ke langkah Pilih Bed lalu pilih tempat tidur lain."*
- **Aksi Tersedia**:
  - `Batalkan Admisi`
  - `Kembali` (melempar ke Langkah 3)
  - `Pesan Tempat Tidur` (dinonaktifkan / *disabled*)

---

## 3. Akar Masalah (Root Cause Analysis)

Pada berkas `src/components/view/health-services/inpatient-management/inpatient-newborn-admission-steps.jsx` baris 187–243:

```javascript
export function NewbornPlacementStep({ flow, bed, newborn, onCancelAdmission }) {
  const [preparedId, setPreparedId] = useState("");
  const ready = Boolean(preparedId || flow.resumeEpisodeId || bed.hasActiveReservation);
  ...
  {!ready ? (
    <InpatientAdmissionBedSelectionStep bed={bed} onBack={flow.goBack} onNext={(id) => { ... }} />
  ) : (
    <InpatientAdmissionBedBookingStep bed={{ ...bed, canExtendReservation: false }} onBack={flow.goBack} ... />
  )}
}
```

### Penyebab 1: Evaluasi `ready` Menyertakan `flow.resumeEpisodeId`
Variabel `ready` menentukan apakah layar menampilkan tahap **Pilih Bed** (`InpatientAdmissionBedSelectionStep`) atau tahap **Booking Bed** (`InpatientAdmissionBedBookingStep`).
Penyertaan `flow.resumeEpisodeId` mengasumsikan bahwa episode yang dilanjutkan sudah memegang bed terpilih. Padahal, banner admisi secara eksplisit menyatakan bahwa episode lanjutan tidak memegang reservasi tempat tidur dan petugas harus memilih ulang tempat tidurnya.
Karena `flow.resumeEpisodeId` bernilai *truthy*, `ready` langsung bernilai `true`, melompati tahap pemilihan denah dan langsung membuka sub-langkah booking dengan `selectedBed = null`.

### Penyebab 2: Pengendali `onBack` pada Sub-Langkah Booking Memanggil `flow.goBack`
Pada `InpatientAdmissionBedBookingStep`, prop `onBack` diberi `flow.goBack` (langkah global wizard). Padahal, di dalam Langkah 4 terdapat dua sub-tahap internal (Pemilihan Bed -> Pemesanan Bed). Tombol "Kembali" pada tahap pemesanan seharusnya mengembalikan petugas ke tahap pemilihan bed di dalam Langkah 4, bukan melempar ke Langkah 3 ("Perawatan dan DPJP Bayi").

---

## 4. Rencana Solusi

1. Ubah logika transisi pada `NewbornPlacementStep`:
   - Ganti `ready` dengan kondisi deterministik: sub-langkah booking HANYA dibuka jika `bed.hasActiveReservation === true` ATAU petugas secara eksplisit telah memilih bed dan menekan tombol lanjut dari pemilihan bed (`inBookingView && bed.selectedBed`).
   - Jika `!bed.hasActiveReservation && !bed.selectedBed`, layar WAJIB menampilkan `InpatientAdmissionBedSelectionStep` (denah dan daftar bed).
2. Perbaiki fungsi `handleBookingBack`:
   - Jika ditekan dari sub-langkah booking tanpa reservasi aktif, cukup kembalikan state internal ke tahap pemilihan bed (`inBookingView = false`), sehingga petugas dapat memilih bed lain tanpa keluar dari Langkah 4.
3. Jalankan pengujian unit otomatis untuk memastikan alur 7 langkah bayi tetap patuh 100%.

---

## 5. Riwayat Dokumen

| Tanggal | Perubahan | Oleh |
| :--- | :--- | :--- |
| 2026-10-22 | Dokumen issue dibuat berdasarkan tangkapan layar user yang terjebak pada layar booking kosong di Langkah 4. | `diagnose-module-issue` |
| 2026-10-22 | Status ditutup SELESAI: Restrukturisasi visibilitas sub-langkah dan navigasi kembali internal pada `NewbornPlacementStep` selesai diterapkan dan 14/14 unit test lolos. | `build-module-frontend` |
