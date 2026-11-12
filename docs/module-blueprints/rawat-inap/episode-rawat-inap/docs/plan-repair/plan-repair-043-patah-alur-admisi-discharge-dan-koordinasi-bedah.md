# PLAN-REPAIR-043 — Rencana kontrol terstruktur dan sumber dokter perujuk luar; pelepasan bed dan pemesanan bedah dibatalkan karena sudah berjalan

```yaml
plan_id: PLAN-REPAIR-EPS-043
issue: ../issue/issue-043-patah-alur-admisi-discharge-dan-koordinasi-bedah.md
status_rencana: MENUNGGU_PERSETUJUAN
tanggal_rencana: "2026-11-02"
diputuskan_oleh: null
tanggal_keputusan: null
basis_source_backend: "396c8363 (MHamzah)"
basis_source_frontend: "9c4000724 (HamzahV2)"
```

> **Catatan revisi 2 (2026-11-02).** Rencana versi 1 berisi empat perbaikan. Dua di antaranya
> (`FIX-EPS-043-01` dan `FIX-EPS-043-04`) akan membangun ulang fitur yang sudah berjalan, sehingga
> keduanya **dibatalkan**. Satu perbaikan (`FIX-EPS-043-03`) memuat rumus tarif yang dikarang, sehingga
> rumus itu ditarik dan perbaikannya dikembalikan ke status menunggu keputusan. Perbaikan yang tersisa
> disusun ulang dari source nyata. Baris lama tetap ada di register, sesuai aturan register.

---

## 1. Register status pengerjaan

| ID perbaikan | Menutup | Ringkasan | Area | Prioritas | Task ID | Status | Bukti |
| --- | --- | --- | --- | :---: | --- | --- | --- |
| `FIX-EPS-043-01` | `ISS-EPS-043-01` | **DIBATALKAN.** Usulan status `DischargedPhysically_PendingBilling` tidak diperlukan, karena pelepasan bed tanpa menunggu kasir sudah berjalan (`RWI-DEC-186`, `BE-RWI-153`) | — | — | — | ⛔ TERBLOKIR — dibatalkan, jangan dikerjakan; menunggu pemilik mengesahkan pembatalan (K-05) | `InpDischargeService.Departure.cs:38`, `:47` |
| `FIX-EPS-043-02` | `ISS-EPS-043-02` | Tahap 1: rencana kontrol pada resume medis menjadi data terstruktur (tanggal, poli, dokter, catatan) | BE + FE + dokumen | 1 | — | 🟡 SEBAGIAN — `BE-RWI-256` ✅; `FE-RWI-286` 🟡 (uji simpan sebagai DPJP belum, butuh akun DPJP); revisi dokumen desain belum | [BE-RWI-256](../../task/report/backend/BE-RWI-256.md), [FE-RWI-286](../../task/report/frontend/FE-RWI-286.md) |
| `FIX-EPS-043-03` | `ISS-EPS-043-03` bagian A | Estimasi biaya PPRI. Rumus karangan versi 1 ditarik; dikerjakan lewat task yang sudah ada setelah tarif disahkan | BE + FE | 2 | `BE-RWI-203`, `FE-RWI-221` | ⛔ TERBLOKIR — menunggu K-03 (`DEC-INP-020`, `RWI-OQ-122`) dari kasir dan manajemen RS | — |
| `FIX-EPS-043-04` | `ISS-EPS-043-04` | **DIBATALKAN.** Pemesanan bedah dari bangsal sudah membuat kasus OK berstatus Diminta (`BE-RWI-175`) | — | — | — | ⛔ TERBLOKIR — dibatalkan, jangan dikerjakan; menunggu pemilik mengesahkan pembatalan (K-05) | `InpatientSurgeryBookingController.cs:58-64`, `InpSurgeryBookingAdapter.cs:138` |
| `FIX-EPS-043-05` | `ISS-EPS-043-03` bagian B | Lembar IPD memakai nama perujuk luar yang sudah tersimpan pada episode sebagai sumber ketiga | BE | 2 | — | ✅ SELESAI 9 November 2026 — `BE-RWI-257`; K-04 disetujui 2026-11-02; pencatatan amandemen di `00-interview-decisions.md` masuk Slice 6 MVP | [BE-RWI-257](../../task/report/backend/BE-RWI-257.md) |
| `FIX-EPS-043-06` | `ISS-EPS-043-02` | Tahap 2: janji temu poliklinik terbit otomatis saat resume medis difinalisasi | BE + lintas modul | 3 | — | ⛔ TERBLOKIR — menunggu K-01 (pemilik `RegistrationManagement`) dan `FIX-EPS-043-02` | — |
| `FIX-EPS-043-07` | `ISS-EPS-043-02` | Tahap 3: konfirmasi janji temu dikirim ke pasien lewat WhatsApp | BE + integrasi luar | 4 | — | ⛔ TERBLOKIR — menunggu K-02 (penyedia WhatsApp) dan `FIX-EPS-043-06` | — |

**Ringkasan: 0 dari 5 perbaikan aktif selesai. Dua perbaikan dibatalkan dan tidak dihitung.**

---

## 2. Solusi terpilih per temuan

### ISS-EPS-043-01 — Pelepasan tempat tidur dikira tertahan kasir

Tidak ada opsi yang perlu dipilih, karena perilaku yang diminta sudah berjalan. Perawat mencatat
kepergian, mengonfirmasi peringatan kasir bila izin belum ada, lalu bed langsung masuk pembersihan.
Izin kasir baru diperiksa saat episode ditutup. Rincian dan buktinya ada pada issue bagian 4.

**Solusi terpilih: tidak mengubah apa pun.** `FIX-EPS-043-01` dibatalkan.

**Kenapa usulan versi 1 ditolak.** Status `DischargedPhysically_PendingBilling` akan menjadi status
kedua untuk keadaan yang sama, yaitu `DischargePending` ditambah `PhysicallyLeftAt` terisi. Dua penanda
untuk satu keadaan mudah saling bertentangan, dan endpoint baru `POST .../vacate-bed` akan menjadi
pintu kedua yang melewati penjaga versi bed (`ExpectedBedVersion`) milik `record-departure`.

---

### ISS-EPS-043-02 — Rencana kontrol hanya teks

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Langsung kirim event janji temu dari resume medis (usulan versi 1) | Satu langkah | Tidak dapat dikerjakan: Registrasi belum punya kemampuan janji temu, dan gateway WhatsApp belum ada |
| **B** | **Bertahap: (1) rencana kontrol terstruktur di rawat inap; (2) janji temu otomatis setelah Registrasi siap; (3) WhatsApp setelah penyedia dipilih** | Tahap 1 dapat dikerjakan sekarang di dalam wewenang rawat inap; tahap berikutnya tinggal memakai data terstruktur dari tahap 1 | Manfaat penuh baru terasa setelah tahap 2 |
| C | Biarkan teks bebas | Tanpa perubahan | Data tidak dapat dipakai sistem lain |

**Solusi terpilih: Opsi B.**

1. Tanpa tanggal, poli, dan dokter yang terstruktur, janji temu otomatis tidak dapat dibuat sama
   sekali. Tahap 1 adalah prasyarat apa pun keputusan K-01 nanti.
2. Tahap 1 hanya menyentuh resume medis milik rawat inap. Tahap 2 dan 3 menunggu pemilik modul lain,
   sesuai larangan mengubah modul lain tanpa wewenang pemiliknya.

**Kenapa bukan opsi lain.** Opsi A bergantung pada service yang tidak ada. Opsi C tidak menyelesaikan
keluhan.

---

### ISS-EPS-043-03 — Estimasi biaya dan dokter perujuk luar

**Bagian A — estimasi biaya.** Tidak ada opsi yang boleh dipilih di dokumen ini. Rumus tarif adalah
keputusan kasir dan manajemen (K-03). **Solusi terpilih: tetap `⛔` dan eskalasikan keputusan.** Setelah
rumusnya disahkan, pekerjaan berjalan lewat task yang sudah ada, yaitu `BE-RWI-203` dan `FE-RWI-221`.

**Bagian B — dokter perujuk luar.**

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Tambah kolom baru `ExternalReferralDoctorName` (usulan versi 1) | — | Kolomnya **sudah ada** (`InpEpisode.cs:83-84`) |
| **B** | **Lembar IPD membaca kolom perujuk luar pada episode sebagai sumber ketiga** | Tanpa kolom baru, tanpa migration; nama yang sudah diketik admisi langsung tercetak | Mengubah urutan sumber yang ditetapkan `RWI-DEC-254`, sehingga perlu keputusan pemilik |
| C | Tunggu `BE-RWI-190` dari Registrasi | Sesuai desain sekarang | Garis kosong bertahan tanpa batas waktu |

**Solusi terpilih: Opsi B, setelah K-04 disetujui.** Datanya sudah ada, dan perubahannya hanya pada
satu query baca.

---

### ISS-EPS-043-04 — Pemesanan bedah dari bangsal

Tidak ada opsi yang perlu dipilih. Pemesanan dari bangsal sudah membuat kasus Kamar Operasi, dan
statusnya tampil kembali di bangsal. **Solusi terpilih: tidak mengubah apa pun.** `FIX-EPS-043-04`
dibatalkan.

---

## 3. Skema tampilan sebelum → sesudah

### A. Detail Episode — Catat Kepergian (`FE-INP-04`, `FE-INP-14`)

**Tidak ada perubahan tampilan.** Rangka "SEBELUM" pada versi 1 (tombol abu-abu bertuliskan "Tidak
dapat melepas tempat tidur sebelum kasir berstatus Cleared") tidak sesuai source. Tampilan yang
berjalan sekarang adalah sebagai berikut, sebagai rujukan untuk uji lapangan:

```text
┌ Catat Kepergian Pasien ───────────────────────────────────────────────┐
│ Pasien : Tn. A (pseudonim) · VIP 401                                   │
│ Waktu keluar ruangan * [ 02/11/2026 10:45 ]                            │
│                                                                        │
│ ⚠ Kasir belum memberi izin pulang. Pastikan keluarga sudah diarahkan   │
│   ke kasir, lalu konfirmasi bila pasien tetap meninggalkan ruangan.    │
│                                                                        │
│                         [ Batal ]  [ Konfirmasi & Catat Kepergian ]    │
└────────────────────────────────────────────────────────────────────────┘
→ sesudah konfirmasi: bed VIP 401 tampil "Pembersihan" pada denah;
  episode tetap DischargePending sampai kasir CLEARED dan episode ditutup.
```

Teks peringatan di atas disalin dari pesan server (`InpDischargeService.Departure.cs:41-42`). Label
tombol pada rangka bersifat ilustratif; yang mengikat adalah alurnya: kiriman pertama dijawab
`INP-DEP-001`, dialog tetap terbuka, lalu kiriman kedua membawa pengakuan peringatan.

### B. Resume Medis — bagian 7 "Rencana Kontrol" (`FE-INP-06`) — `FIX-EPS-043-02`

**SEBELUM** — `src/components/view/health-services/inpatient-management/inpatient-discharge-view.jsx:137-143`, `:259`

```text
7. Rencana Kontrol
[ Tuliskan jadwal kontrol, anjuran istirahat, dan instruksi tindak lanjut.        ]
[                                                                     0/2000      ]
```

**SESUDAH**

```text
7. Rencana Kontrol
( ) Tidak perlu kontrol      (•) Kontrol ke poliklinik
Tanggal kontrol *   [ 09/11/2026 ]                  ← tidak boleh sebelum tanggal keputusan pulang
Poliklinik *        [ Poli Penyakit Dalam        ▾ ]
Dokter              [ dr. F, Sp.PD               ▾ ] ← opsional; disaring menurut poli terpilih
Catatan tindak lanjut
[ Bawa hasil lab terakhir. Istirahat 3 hari.                                0/2000 ]
```

| Wilayah | Isi | Sumber data | Komponen |
| --- | --- | --- | --- |
| Pilihan kontrol | Perlu kontrol atau tidak | Kolom baru pada resume | Radio pilihan tunggal yang sudah dipakai form resume |
| Tanggal kontrol | Tanggal rencana kontrol | Kolom baru | `FilterDatePicker` |
| Poliklinik | Poli tujuan | Master unit layanan rawat jalan yang sudah ada | Select base component |
| Dokter | Dokter tujuan, opsional | Master dokter yang sudah ada, disaring poli | Select base component |
| Catatan | Teks bebas lama | `FollowUpInstruction` (tetap dipakai) | Textarea yang sekarang |

| Tombol | Jenis | Kapan aktif | Yang terjadi |
| --- | --- | --- | --- |
| Simpan Draf | Sekunder | Selalu | Menyimpan isian walau belum lengkap |
| Finalisasi Resume | Utama | Selalu dapat ditekan | Bila "Kontrol ke poliklinik" dipilih tetapi tanggal atau poli kosong, isian yang kurang ditandai dan resume tidak difinalisasi |

| Keadaan | Tampilan |
| --- | --- |
| Memuat | Kerangka form seperti sekarang |
| Resume lama (hanya teks) | Pilihan "Kontrol ke poliklinik" kosong; catatan lama tampil utuh pada kotak Catatan |
| Master poli gagal dimuat | Pesan "Daftar poliklinik tidak dapat dimuat" dan tombol Muat Ulang; catatan tetap dapat diisi |
| Tidak berhak | Form baca-saja seperti aturan resume sekarang |

### C. Lembar IPD — baris "Dokter perujuk" (`FE-INP-35` s.d. `44`) — `FIX-EPS-043-05`

```text
SEBELUM                                         SESUDAH (bila K-04 disetujui)
Dokter perujuk : _______________________        Dokter perujuk : dr. D (Klinik E)
```

Bila pasien membawa surat pengantar rawat inap, nama dari surat tetap didahulukan. Tidak ada
perubahan pada komponen frontend, karena frontend sudah menampilkan nilai `ReferringDoctor` dari server.

### D. Estimasi Biaya, janji temu otomatis, dan WhatsApp

**Belum digambar.** Rangkanya bergantung pada keputusan K-01, K-02, dan K-03. Menggambarnya sekarang
berarti mengarang isi layar.

---

## 4. Rincian perbaikan

### FIX-EPS-043-01 — DIBATALKAN

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-043-01` |
| **Alasan pembatalan** | Pelepasan bed tanpa menunggu kasir sudah berjalan. Bukti: `InpDischargeService.Departure.cs:22-60`, `InpBedOccupancyService.cs:1991`, `inpatient-episode-detail-view.jsx:594`, keputusan `RWI-DEC-186` |

Tidak ada langkah. Endpoint `POST .../episodes/{id}/vacate-bed` dan status
`DischargedPhysically_PendingBilling` dari versi 1 **tidak boleh** dibuat.

---

### FIX-EPS-043-02 — Rencana kontrol terstruktur pada resume medis (tahap 1)

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-043-02` (sebagian; sisanya `FIX-EPS-043-06` dan `07`) |
| **Area** | Backend dan frontend rawat inap, ditambah revisi dokumen desain |
| **Jenis perubahan** | Source backend (model, DTO, validasi, migration), source frontend, dokumen blueprint |
| **Berkas yang diubah** | Backend: `Areas/HealthServices/InPatientManagement/Models/InpDischargeSummary.cs`, `InpDischargeSummaryRevision.cs` (salinan riwayat memuat `FollowUpInstruction` pada baris 36), `DTOs/InpatientDischargeDtos.cs` (baris 59, 130, 194), service resume pada `InpDischargeService.cs`, satu migration baru. Frontend: `inpatient-discharge-view.jsx`, `utils/health-services/inpatient-management/inpatient-discharge-utils.jsx`, cetakan resume |
| **Radius dampak** | Form, detail, riwayat revisi, dan cetakan resume medis rawat inap. `FollowUpInstruction` milik IGD (`emergency-doctor-disposition-tab.jsx`) dan serah terima admisi (`InpatientAdmissionTransferDtos.cs:77`) **tidak** disentuh |
| **Bergantung pada** | Persetujuan rencana ini dan revisi `05-skema-tampilan.md` bagian 11 |

**Langkah.**

1. Revisi `05-skema-tampilan.md` bagian 11 (`FE-INP-06`) sesuai rangka 3.B lewat jalur blueprint.
2. Tambah tiga kolom opsional pada resume medis dan salinan revisinya: `FollowUpDate` (tanggal),
   `FollowUpServiceUnitId` (poli), `FollowUpDoctorId` (dokter). Nama akhir mengikuti kontrak rekayasa
   backend dan registry prefix.
3. Pertahankan `FollowUpInstruction` sebagai catatan tindak lanjut, supaya resume lama tetap terbaca.
4. Tambah validasi server: bila tanggal kontrol diisi, poli wajib diisi, dan tanggal tidak boleh
   sebelum tanggal keputusan pulang. Dokter yang dipilih harus berpraktik di poli itu.
5. Ubah form frontend dan cetakan resume sesuai rangka 3.B.

**Acceptance criteria.**

1. Dokter memilih "Kontrol ke poliklinik", tanggal 9 November 2026, Poli Penyakit Dalam, dan dr. F,
   lalu memfinalisasi. Detail resume menampilkan ketiga nilai itu, dan cetakan resume memuat baris
   "Kontrol: Senin, 9 November 2026 — Poli Penyakit Dalam — dr. F".
2. Keputusan pulang bertanggal 2 November 2026. Dokter mengisi tanggal kontrol 1 November 2026. Server
   menolak dengan pesan "Tanggal kontrol tidak boleh sebelum tanggal keputusan pulang", dan layar
   menandai isian tanggal.
3. Dokter memilih "Kontrol ke poliklinik" tetapi tidak memilih poli. Tombol Finalisasi tetap dapat
   ditekan, isian poli ditandai wajib, dan resume tidak difinalisasi.
4. Resume lama yang hanya berisi teks "Kontrol 1 minggu" tetap terbuka tanpa galat. Teksnya tampil
   pada kotak Catatan, dan pilihan kontrol kosong.
5. Revisi resume menyimpan nilai tanggal, poli, dan dokter sebelum perubahan.

**Verifikasi.** Backend: `dotnet build` pada project aplikasi. Frontend: `npm run lint:errors`,
`npm run test:unit`, `npm run build`. Uji manual satu resume baru dan satu resume lama.

**Risiko.** Resume lama tanpa data terstruktur dapat salah dibaca sebagai "tidak perlu kontrol". Untuk
mencegahnya, pilihan kontrol dibiarkan kosong (bukan "Tidak perlu") pada data lama, dan catatan lama
selalu ditampilkan.

---

### FIX-EPS-043-03 — Estimasi biaya PPRI (tertahan)

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-043-03` bagian A |
| **Area** | Backend dan frontend Workspace PPRI |
| **Task ID** | `BE-RWI-203`, `FE-RWI-221` — sudah terdaftar pada `backend-roadmap-workspace-ppri.md` dan `frontend-roadmap-workspace-ppri.md` |
| **Bergantung pada** | K-03 |

Tidak ada langkah yang boleh dimulai sebelum K-03. Rumus `(Hari × Kamar) + Paket Operasi + Visit` pada
versi 1 **ditarik**, karena belum disahkan siapa pun. Setelah K-03, kerjakan kartu task yang sudah ada
dan perbarui register ini dengan tautan laporannya.

---

### FIX-EPS-043-04 — DIBATALKAN

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-043-04` |
| **Alasan pembatalan** | Pemesanan dari bangsal sudah membuat kasus OK. Bukti: `InpatientSurgeryBookingController.cs:58-64`, `InpSurgeryBookingAdapter.cs:138`, `patient-surgery-cases-table.jsx:150`; task `BE-RWI-175` |

Tidak ada langkah. `IOprCaseService.CreateElectiveCaseAsync` dan status `RequestedFromWard` dari
versi 1 **tidak boleh** dibuat.

---

### FIX-EPS-043-05 — Lembar IPD membaca perujuk luar yang tersimpan pada episode

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-043-03` bagian B |
| **Area** | Backend |
| **Jenis perubahan** | Source backend (query baca), revisi keputusan blueprint |
| **Berkas yang diubah** | `Areas/HealthServices/InPatientManagement/Services/InpAdmissionWorkspaceQueryService.cs`, sekitar baris 494-510 |
| **Radius dampak** | Lembar IPD pada Workspace PPRI saja. Form admisi dan tabel `InpEpisode` tidak berubah |
| **Bergantung pada** | K-04 dan amandemen `RWI-DEC-254` / `RWI-AC-386` |

**Langkah.**

1. Catat amandemen `RWI-DEC-254` dan `RWI-AC-386` pada `00-interview-decisions.md` lewat jalur
   keputusan yang berwenang.
2. Pada pembentukan baris "Dokter perujuk", tambahkan sumber ketiga sesudah surat pengantar dan
   perujuk luar dari Registrasi: `InpEpisode.ExternalReferralDoctorName`, ditambah
   `ExternalReferralFacilityName` dalam kurung bila terisi.
3. Bila ketiganya kosong, tetap cetak garis kosong. Cetak IPD tidak boleh gagal.

**Acceptance criteria.**

1. Tn. C membawa surat pengantar rawat inap dari dr. G yang sudah terbit, dan pada admisi juga
   diketik perujuk luar dr. D. Lembar IPD mencetak "dr. G", karena surat pengantar didahulukan.
2. Tn. C tidak membawa surat pengantar. Pada admisi diketik dr. D dari Klinik E. Lembar IPD mencetak
   "dr. D (Klinik E)".
3. Tn. C tidak membawa surat pengantar, dan pada admisi diketik dr. D tanpa nama fasilitas. Lembar IPD
   mencetak "dr. D".
4. Tidak ada surat pengantar dan tidak ada perujuk luar. Lembar IPD mencetak garis kosong, tanpa galat.

**Verifikasi.** `dotnet build`, lalu cetak IPD untuk keempat skenario di atas.

**Risiko.** Nama yang diketik bebas bisa salah eja. Ini sama dengan keadaan sekarang pada langkah
konfirmasi admisi. Koreksinya mengikuti jalur koreksi data episode yang sudah ada.

---

### FIX-EPS-043-06 — Janji temu otomatis (tahap 2, tertahan)

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-043-02` (sebagian) |
| **Bergantung pada** | K-01 dan `FIX-EPS-043-02` |

Rincian disusun setelah pemilik `RegistrationManagement` menetapkan kemampuan janji temu dan
kontraknya. Arah yang direkomendasikan: saat resume difinalisasi dengan tanggal dan poli kontrol,
rawat inap memanggil kontrak janji temu milik Registrasi melalui pola integrasi yang disepakati.
Kegagalan Registrasi tidak boleh menggagalkan finalisasi resume; kegagalan itu tampil sebagai
peringatan tindak lanjut.

---

### FIX-EPS-043-07 — Konfirmasi WhatsApp (tahap 3, tertahan)

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-043-02` (sebagian) |
| **Bergantung pada** | K-02 dan `FIX-EPS-043-06` |

Rincian disusun setelah penyedia dan kebijakan pesan ditetapkan. Nomor telepon pasien dan isi pesan
termasuk data pribadi, sehingga persetujuan pasien dan kebijakan privasi rumah sakit ikut menjadi
prasyarat.

---

## 5. Urutan pengerjaan

```text
FIX-EPS-043-02   tanpa dependency teknis — dikerjakan setelah rencana disetujui
└── K-01 (pemilik Registrasi)
    └── ⛔ FIX-EPS-043-06   janji temu memakai tanggal & poli dari FIX-02
        └── K-02 (penyedia WhatsApp)
            └── ⛔ FIX-EPS-043-07
K-04 (pemilik modul)
└── ⛔ FIX-EPS-043-05
K-03 (kasir & manajemen)
└── ⛔ FIX-EPS-043-03   → BE-RWI-203, FE-RWI-221
K-05 (pengesahan pembatalan)
├── ⛔ FIX-EPS-043-01   dibatalkan
└── ⛔ FIX-EPS-043-04   dibatalkan
```

---

## 6. Keputusan pemilik yang dibutuhkan

| No | Keputusan | Rekomendasi | Menahan perbaikan |
| ---: | --- | --- | --- |
| K-01 | Siapa yang membangun kemampuan janji temu poliklinik, dan apakah rawat inap boleh memanggilnya | Pemilik Registrasi membangun janji temu baru; reservasi prioritas sengaja tidak membuat antrean | `FIX-EPS-043-06` |
| K-02 | Penyedia dan kebijakan pengiriman WhatsApp | Tunda sampai K-01 selesai | `FIX-EPS-043-07` |
| K-03 | Rumus estimasi biaya PPRI dan sifat mengikatnya (`DEC-INP-020`, `RWI-OQ-122`) | Disahkan kasir dan manajemen dari tarif nyata | `FIX-EPS-043-03` |
| K-04 | Nama perujuk luar yang diketik admisi dipakai sebagai sumber ketiga lembar IPD | Setuju | `FIX-EPS-043-05` |
| K-05 | Pengesahan pembatalan `FIX-EPS-043-01` dan `FIX-EPS-043-04` | Setuju | Penutupan issue |

---

## 7. Dokumen hulu yang ikut direvisi

| Dokumen | Bagian | Sekarang | Menjadi | Untuk |
| --- | --- | --- | --- | --- |
| `episode-rawat-inap/05-skema-tampilan.md` | 11, `FE-INP-06` | Rencana kontrol berupa satu teks bebas | Pilihan kontrol, tanggal, poli, dokter, dan catatan sesuai rangka 3.B | `FIX-EPS-043-02` |
| `episode-rawat-inap/data/data-dictionary.md` | Tabel resume medis | Hanya `FollowUpInstruction` | Ditambah tiga kolom rencana kontrol | `FIX-EPS-043-02` |
| `episode-rawat-inap/contracts/api-contract.md` | Resume medis | Body tanpa rencana kontrol terstruktur | Body memuat tiga kolom baru beserta validasinya | `FIX-EPS-043-02` |
| `00-interview-decisions.md` | `RWI-DEC-254`, `RWI-AC-386` | "Surat pengantar, lalu perujuk luar kunjungan, selain itu garis kosong" | "Surat pengantar, lalu perujuk luar kunjungan, lalu perujuk luar yang dicatat admisi pada episode, selain itu garis kosong" | `FIX-EPS-043-05` |
| `02-backend-architecture.md`, `contracts/state-transition-matrix.md` | — | Versi 1 meminta status `DischargedPhysically_PendingBilling` | **Tidak direvisi**, karena `FIX-EPS-043-01` dibatalkan | — |

Revisi dokumen desain dikerjakan lewat jalur blueprint yang berwenang, bukan oleh dokumen ini.

---

## 8. Verifikasi menyeluruh

1. **Kepulangan tanpa izin kasir (perilaku yang sudah ada, untuk menutup P-01).** Login sebagai
   perawat berhak `InpatientDischarge : RecordDeparture`. Pada episode `DischargePending` dengan kasir
   belum `CLEARED`, tekan Catat Kepergian, lalu konfirmasi peringatan. Hasil yang diharapkan: bed
   berubah menjadi Pembersihan pada denah, episode tampil pada tab Monitoring "Pulang Sebelum Izin
   Kasir", dan penutupan episode ditolak sampai kasir `CLEARED`.
2. **Pemesanan bedah (perilaku yang sudah ada, untuk menutup P-02).** Dari Pelayanan Bedah (IBS),
   pesan ruang bedah untuk order tindakan `Ordered`. Hasil yang diharapkan: kasus muncul di daftar Kamar
   Operasi berstatus Diminta, dan status yang sama tampil pada tabel kasus di bangsal.
3. **Rencana kontrol (`FIX-EPS-043-02`).** Finalisasi resume dengan kontrol 9 November 2026 ke Poli
   Penyakit Dalam. Hasil yang diharapkan: data terstruktur tersimpan, tercetak, dan tersalin pada
   riwayat revisi.
4. **Lembar IPD (`FIX-EPS-043-05`).** Jalankan keempat acceptance criteria `FIX-EPS-043-05`.

---

## 9. Riwayat

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-11-02 | Versi 1: rencana diterbitkan, 4 perbaikan, menunggu persetujuan | AI Agent (`diagnose-module-issue`) |
| 2026-11-02 | Revisi 2: verifikasi ulang terhadap source SHA yang sama. `FIX-EPS-043-01` dan `FIX-EPS-043-04` dibatalkan karena fiturnya sudah berjalan. Rumus estimasi biaya pada `FIX-EPS-043-03` ditarik, dan perbaikannya dikembalikan ke `BE-RWI-203`/`FE-RWI-221` yang menunggu K-03. `FIX-EPS-043-02` disusun ulang menjadi tahap 1 terstruktur, lalu dipecah dengan `FIX-EPS-043-06` (janji temu) dan `FIX-EPS-043-07` (WhatsApp). `FIX-EPS-043-05` ditambahkan untuk sumber perujuk luar. Rangka "SEBELUM" versi 1 yang tidak sesuai source diganti | AI Agent (`diagnose-module-issue`) atas permintaan pemilik |
