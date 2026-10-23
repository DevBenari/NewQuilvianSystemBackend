# PLAN-REPAIR-007 — Rencana Perbaikan Pemulihan Alur Peresepan dan Isolasi Siklus Kunjungan Rawat Inap

```yaml
plan_id: PLAN-REPAIR-DOK-007
issue: ../issue/issue-007-penyelesaian-soap-mengunci-seluruh-transaksi.md
status_rencana: SELESAI
tanggal_rencana: "2026-10-20"
diputuskan_oleh: "User Directive & Antigravity Engineering"
tanggal_keputusan: "2026-10-20"
basis_source_backend: "6e0f2b1b58a2192a0f54f0fdff718bc77670e3bd (MHamzah)"
basis_source_frontend: "a491b05afcb6b3ed70218ccaa557007719d45ac3 (HamzahV2)"
```

---

## 1. Register Status Pengerjaan

| ID perbaikan | Menutup | Ringkasan | Area | Prioritas | Task ID | Status | Bukti |
| :--- | :--- | :--- | :---: | :---: | :---: | :--- | :--- |
| `FIX-DOK-007-01` | `ISS-DOK-007-01` | Izinkan pembuatan resep pada konsultasi/catatan rawat inap yang berstatus `Completed` | Backend & Frontend | 1 | — | SELESAI | `PrescriptionController.cs:749-756` lewati penolakan completed untuk rawat inap |
| `FIX-DOK-007-02` | `ISS-DOK-007-T1` | Cegah mutasi `EncounterStatus` ke `ConsultationCompleted` untuk kunjungan admisi rawat inap | Backend | 1 | — | SELESAI | `ConsultationFinalizationService.cs:189-195` isolasi status encounter rawat inap |
| `FIX-DOK-007-03` | `ISS-DOK-007-T2` | Kecualikan rawat inap dari emisi milestone tagihan poliklinik saat mengunci SOAP | Backend | 2 | — | SELESAI | `ConsultationFinalizationService.cs:254-262` kecualikan encounter rawat inap |

**Ringkasan: 3 dari 3 perbaikan selesai.**

---

## 2. Solusi Terpilih per Temuan

### ISS-DOK-007-01 — Resep ditolak saat catatan SOAP berstatus Completed

| Opsi | Cara | Kelebihan | Kekurangan |
| :---: | :--- | :--- | :--- |
| **A (Terpilih)** | Mengubah urutan validasi pada `PrescriptionController.cs`: Jika catatan menempel pada episode rawat inap (`consultation.InpEpisodeId.HasValue`), lewati penolakan `Completed` | Tidak mengubah skema database, DTO, maupun kontrak API. Resep tetap terhubung ke catatan visite dokter yang sah sebagai dasar klinis. | Membutuhkan test regresi agar jalur rawat jalan tetap ditolak bila konsultasi selesai. |
| **B** | Mengubah `ConsultationId` pada `CreatePrescriptionRequest` menjadi opsional/nullable untuk rawat inap | Resep tidak lagi bergantung pada catatan SOAP | Mengubah kontrak DTO, skema tabel resep, dan merusak keterlacakan siapa DPJP yang mendasari resep. |
| **C** | Membuat konsultasi draf baru otomatis di balik layar setiap kali resep dibuat | Pengguna tidak melihat error | Melahirkan draf SOAP palsu/kosong yang mengotori rekam medis pasien. |

**Solusi terpilih: Opsi A.**
1. Memenuhi prinsip rekayasa Quilvian: perubahan terisolasi dengan radius dampak terkecil tanpa mengubah kontrak atau skema tabel.
2. Mempertahankan nilai medikolegal: di rawat inap, catatan visite DPJP yang sudah ditandatangani dan dikunci (`Completed`) justru merupakan bukti instruksi medis yang sah untuk pengeluaran obat oleh farmasi.

**Kenapa bukan opsi lain:**
- Opsi B mengubah kontrak API kanonikal dan menghilangkan tautan akuntabilitas resep ke visite dokter.
- Opsi C merusak integritas data rekam medis dengan memunculkan catatan klinis fiktif.

---

### ISS-DOK-007-T1 — Mutasi status encounter rawat inap menjadi ConsultationCompleted

| Opsi | Cara | Kelebihan | Kekurangan |
| :---: | :--- | :--- | :--- |
| **A (Terpilih)** | Pasang filter pada `ConsultationFinalizationService.cs`: mutasi `EncounterStatus = ConsultationCompleted` hanya jika bukan rawat inap (`!consultation.InpEpisodeId.HasValue` dan `EncounterType != Inpatient`) | Kunjungan rawat inap tetap berstatus aktif (`Admitted`) sampai pasien dipulangkan secara resmi (discharge). | Tidak ada |
| **B** | Biarkan termutasi, lalu tambahkan logic di semua modul untuk menganggap `ConsultationCompleted` sebagai `Admitted` | Tidak menyentuh service finalisasi | Merusak konsistensi status dan menyebarkan asumsi rumit ke seluruh modul. |

**Solusi terpilih: Opsi A.**
Sesuai rancangan arsitektur domain rumah sakit: siklus hidup kunjungan rawat inap berbeda dengan poliklinik. Mengunci catatan harian rawat inap tidak boleh menghentikan status kunjungan rawat inap pasien.

---

### ISS-DOK-007-T2 — Emisi tagihan poliklinik saat mengunci catatan rawat inap

| Opsi | Cara | Kelebihan | Kekurangan |
| :---: | :--- | :--- | :--- |
| **A (Terpilih)** | Tambahkan kondisi pengecualian `EncounterType.Inpatient` dan `consultation.InpEpisodeId.HasValue` pada blok emisi `_clinicalMilestoneFactProducer.EmitChargeEligibilityAsync` di `ConsultationFinalizationService.cs` | Mencegah tagihan jasa poliklinik salah masuk ke invoice rawat inap pasien. Tagihan visite rawat inap tetap dikelola secara tepat lewat modul Visite DPJP (`TrxInpPhysicianVisit`). | Tidak ada |
| **B** | Batalkan tagihan di sisi kasir/billing setelah faktanya terbit | Tidak mengubah kode klinis | Membebani kasir dan rawan lolos tagih ke pasien. |

**Solusi terpilih: Opsi A.**
Mencegah penerbitan fakta tagihan yang salah langsung dari sumbernya (*at source*).

---

## 3. Skema Tampilan Sebelum → Sesudah

### Tampilan Tab Resep Saat Catatan SOAP Terakhir Sudah Dikunci

#### SEBELUM (Kondisi Bermasalah Saat Ini)

```text
+----------------------------------------------------------------------------------------------------+
|  [ Form Buat Resep ]   ( Riwayat Resep )   ( Resep Harian )   ( Template Resep )   ( Sliding Scale )|
+----------------------------------------------------------------------------------------------------+
| [Catatan Dokter Pengait (SOAP)]: [ 20/10/2026 08:30 — dr. Hendra (SOAP Visite)                 v ]  |
|                                                                                                    |
| ⚠ PERINGATAN:                                                                                      |
| [ Gagal menyimpan resep: Konsultasi yang sudah completed tidak dapat ditambahkan resep.          ] |
|                                                                                                    |
| +------------------------------------------------------------------------------------------------+ |
| | Cari Obat: [ Paracetamol 500mg tab                            ]                                | |
| | Jumlah: [ 10 ]  Dosis: [ 3x1 ]  Aturan Pakai: [ Sesudah makan                               ]  | |
| | [ + Tambah ke Daftar ]                                                                         | |
| +------------------------------------------------------------------------------------------------+ |
|                                                                                                    |
| Daftar Obat yang Diresepkan:                                                                       |
| 1. Paracetamol 500mg tab - 10 tab - 3x1 sesudah makan                                             |
|                                                                                                    |
| [ Reset Form ]                                                 [ Simpan Draf Resep (DITOLAK 400) ] |
+----------------------------------------------------------------------------------------------------+
```

#### SESUDAH (Setelah Perbaikan)

```text
+----------------------------------------------------------------------------------------------------+
|  [ Form Buat Resep ]   ( Riwayat Resep )   ( Resep Harian )   ( Template Resep )   ( Sliding Scale )|
+----------------------------------------------------------------------------------------------------+
| [Catatan Dokter Pengait (SOAP)]: [ 20/10/2026 08:30 — dr. Hendra [Terkunci/Sah]                v ]  |
| ℹ Resep rawat inap mengacu pada catatan visite DPJP yang sudah ditandatangani dan terkunci.        |
|                                                                                                    |
| +------------------------------------------------------------------------------------------------+ |
| | Cari Obat: [ Paracetamol 500mg tab                            ]                                | |
| | Jumlah: [ 10 ]  Dosis: [ 3x1 ]  Aturan Pakai: [ Sesudah makan                               ]  | |
| | [ + Tambah ke Daftar ]                                                                         | |
| +------------------------------------------------------------------------------------------------+ |
|                                                                                                    |
| Daftar Obat yang Diresepkan:                                                                       |
| 1. Paracetamol 500mg tab - 10 tab - 3x1 sesudah makan                                             |
|                                                                                                    |
| [ Reset Form ]                                                 [ Simpan Draf Resep (SUKSES 201) ]  |
+----------------------------------------------------------------------------------------------------+
```

### Tabel Perubahan Rangka Layar

| Wilayah | Isi & Komponen | Sumber Data | Perubahan yang Terjadi |
| :--- | :--- | :--- | :--- |
| **Pemilih Catatan Pengait** | Dropdown pilihan catatan SOAP visite (`BaseSelectField`) | `getSoapTimelineByEpisode` | Menampilkan indikator status catatan (misal `[Terkunci/Sah]`) dan tetap dapat dipilih sebagai dasar penerbitan resep obat rawat inap. |
| **Banner Peringatan** | Alert informasi status peresepan (`InformationAlert`) | Evaluasi frontend | Pesan galat merah penolakan `400` hilang; digantikan pemberitahuan ramah bahwa catatan visite sah menjadi rujukan peresepan. |
| **Tombol Simpan Resep** | Tombol aksi primer (`BaseButton variant="primary"`) | State `draft` | Berhasil memanggil `POST /prescriptions` dengan status balasan `201 Created`. |

---

## 4. Rincian Perbaikan

### FIX-DOK-007-01 — Izinkan Pembuatan Resep pada Catatan Rawat Inap Berstatus Completed

| Field | Keterangan |
| :--- | :--- |
| **Menutup** | `ISS-DOK-007-01` |
| **Area** | Backend (`PrescriptionController.cs`) & Frontend (`use-inpatient-prescription-tab.jsx`) |
| **Jenis perubahan** | Source Code Backend & Frontend |
| **Berkas yang diubah** | 1. `NewQuilvianSystemBackend/Areas/HealthServices/PharmacyManagement/Controllers/PrescriptionController.cs`<br>2. `QuilvianSystemFrontendDev/src/lib/hooks/health-services/inpatient-management/use-inpatient-prescription-tab.jsx` |
| **Radius dampak** | Pembuatan resep Rawat Inap (seluruh dokter bangsal rawat inap). Rawat Jalan dan MCU tetap tertolak jika konsultasi sudah selesai. |
| **Bergantung pada** | Persetujuan keputusan `K-01` |

#### Langkah Pengerjaan
1. Buka `PrescriptionController.cs` pada method `ValidateCreateRequestAsync`.
2. Ubah urutan pemeriksaan:
   ```csharp
   if (consultation == null) 
       return (false, "Konsultasi dokter tidak ditemukan atau tidak sesuai encounter.");
   
   // Rawat Inap: jika menempel pada episode rawat inap aktif, konsultasi Completed tetap sah
   // sebagai acuan resep pasien rawat inap.
   if (consultation.InpEpisodeId.HasValue)
   {
       if (consultation.ConsultationStatus == DoctorConsultationStatus.Cancelled)
           return (false, "Konsultasi yang sudah cancelled tidak dapat ditambahkan resep.");
       return (true, null);
   }

   // Rawat Jalan & Jalur Lain: pertahankan aturan ketat poliklinik
   if (consultation.ConsultationStatus == DoctorConsultationStatus.Completed) 
       return (false, "Konsultasi yang sudah completed tidak dapat ditambahkan resep.");
   if (consultation.ConsultationStatus == DoctorConsultationStatus.Cancelled) 
       return (false, "Konsultasi yang sudah cancelled tidak dapat ditambahkan resep.");
   ```
3. Pada frontend `use-inpatient-prescription-tab.jsx`, pastikan dropdown konsultasi pengait tetap dapat memilih catatan yang sudah `Completed` dan labelnya informatif.

#### Acceptance Criteria
1. Diberikan pasien rawat inap dengan catatan SOAP yang baru saja dikunci (`ConsultationStatus == Completed`).
2. Ketika dokter menginput obat dan menekan "Simpan Draf Resep", sistem berhasil menyimpan resep (`201 Created`) tanpa galat penolakan.
3. Diberikan pasien rawat jalan dengan konsultasi yang sudah `Completed`.
4. Ketika dicoba menambah resep pada konsultasi poliklinik tersebut, sistem tetap menolak dengan pesan *"Konsultasi yang sudah completed tidak dapat ditambahkan resep."* (regresi poliklinik aman).

#### Verifikasi
- Jalankan test suite backend terkait peresepan rawat inap dan rawat jalan.
- Uji manual lewat browser pada workspace dokter rawat inap.

#### Risiko
Kesalahan penempatan kondisi dapat melonggarkan rawat jalan. Penjaga `consultation.InpEpisodeId.HasValue` wajib dipastikan hanya aktif pada episode rawat inap.

---

### FIX-DOK-007-02 — Cegah Mutasi EncounterStatus Saat Menutup Catatan Rawat Inap

| Field | Keterangan |
| :--- | :--- |
| **Menutup** | `ISS-DOK-007-T1` |
| **Area** | Backend (`ConsultationFinalizationService.cs`) |
| **Jenis perubahan** | Source Code Backend |
| **Berkas yang diubah** | `NewQuilvianSystemBackend/Areas/HealthServices/PharmacyManagement/Services/ConsultationFinalizationService.cs` |
| **Radius dampak** | Alur finalisasi catatan SOAP rawat inap dan rawat jalan |
| **Bergantung pada** | Persetujuan keputusan `K-02` |

#### Langkah Pengerjaan
1. Buka `ConsultationFinalizationService.cs` pada method `FinalizeAsync`.
2. Pada blok pembaruan status `consultation.Encounter` (sekitar baris 189–194), tambahkan syarat penjaga:
   ```csharp
   if (consultation.Encounter != null && 
       !consultation.InpEpisodeId.HasValue && 
       consultation.Encounter.EncounterType != EncounterType.Inpatient)
   {
       consultation.Encounter.EncounterStatus = EncounterStatus.ConsultationCompleted;
       consultation.Encounter.UpdateDateTime = now;
       consultation.Encounter.UpdateBy = actorUserId;
   }
   ```
3. Pastikan untuk rawat inap, status `consultation.Encounter.EncounterStatus` tidak disentuh, sehingga tetap berstatus `Admitted` / dalam perawatan.

#### Acceptance Criteria
1. Ketika dokter mengunci catatan SOAP rawat inap, record `RegPatientEncounter` pasien tetap mempertahankan `EncounterStatus` aktifnya dan tidak berubah menjadi `ConsultationCompleted`.
2. Ketika dokter poliklinik menyelesaikan konsultasi rawat jalan, `RegPatientEncounter` tetap berubah menjadi `ConsultationCompleted`.

#### Verifikasi
Jalankan tes unit `DoctorConsultationCompletionTests.cs` dan verifikasi nilai status encounter.

---

### FIX-DOK-007-03 — Lindungi Rawat Inap dari Emisi Tagihan Poliklinik

| Field | Keterangan |
| :--- | :--- |
| **Menutup** | `ISS-DOK-007-T2` |
| **Area** | Backend (`ConsultationFinalizationService.cs`) |
| **Jenis perubahan** | Source Code Backend |
| **Berkas yang diubah** | `NewQuilvianSystemBackend/Areas/HealthServices/PharmacyManagement/Services/ConsultationFinalizationService.cs` |
| **Radius dampak** | Modul Billing / Kasir Rawat Inap |
| **Bergantung pada** | `FIX-DOK-007-02` |

#### Langkah Pengerjaan
1. Buka `ConsultationFinalizationService.cs` sekitar baris 254.
2. Perluas pengecualian emisi tagihan jasa konsultasi:
   ```csharp
   if (consultation.Encounter?.EncounterType != EncounterType.Emergency &&
       consultation.Encounter?.EncounterType != EncounterType.Inpatient &&
       !consultation.InpEpisodeId.HasValue)
   {
       // Emisi tagihan jasa konsultasi poliklinik
       var consultationEmission = await _clinicalMilestoneFactProducer.EmitChargeEligibilityAsync(...);
       ...
   }
   ```

#### Acceptance Criteria
1. Penguncian SOAP rawat inap tidak lagi menghasilkan entri tagihan jasa konsultasi rawat jalan di modul Billing.
2. Penguncian konsultasi rawat jalan tetap menerbitkan tagihan jasa dokter poliklinik seperti biasa.

---

## 5. Urutan Pengerjaan

```text
K-01 & K-02 (Keputusan Pemilik)
├── FIX-DOK-007-02 (Cegah mutasi EncounterStatus kunjungan rawat inap)
│   └── FIX-DOK-007-03 (Kecualikan tagihan jasa poliklinik untuk rawat inap)
└── FIX-DOK-007-01 (Izinkan resep menempel pada catatan rawat inap Completed)
```

---

## 6. Keputusan Pemilik yang Dibutuhkan

| No | Keputusan | Implikasi jika Disetujui | Menahan Perbaikan |
| :---: | :--- | :--- | :--- |
| **K-01** | Menyetujui bahwa catatan SOAP visite rawat inap yang berstatus `Completed` sah menjadi catatan pengait (`ConsultationId`) bagi resep rawat inap. | Membuka blokir validasi pada `PrescriptionController.cs`. Dokter dapat meresepkan obat kapan saja selama masa rawat inap. | `FIX-DOK-007-01` |
| **K-02** | Menyetujui bahwa status kunjungan admisi (`EncounterStatus`) tidak diubah ke `ConsultationCompleted` saat dokter menyelesaikan catatan harian. | Menjaga status admisi tetap aktif di seluruh modul selama pasien belum dipulangkan. | `FIX-DOK-007-02`, `FIX-DOK-007-03` |

---

## 7. Dokumen Hulu yang Ikut Direvisi

| Dokumen | Bagian | Rumusan Sekarang | Rumusan Baru |
| :--- | :--- | :--- | :--- |
| `docs/module-blueprints/rawat-inap/dokter-rawat-inap/02-backend-architecture.md` | Bagian Alur Peresepan Rawat Inap | Catatan dokter pengait resep harus berstatus aktif. | Catatan dokter pengait resep rawat inap sah menggunakan catatan yang sudah berstatus `Completed` sepanjang episode rawat inap masih aktif. |
| `docs/module-blueprints/rawat-inap/dokter-rawat-inap/contracts/integration-contract.md` | Bagian Integrasi Farmasi | Resep menolak konsultasi yang sudah `Completed`. | Pengecekan status `Completed` hanya berlaku bagi Rawat Jalan; Rawat Inap dikecualikan. |

---

## 8. Verifikasi Menyeluruh

Setelah ketiga perbaikan diimplementasikan, jalankan skenario pengujian ujung-ke-ujung (*end-to-end*):

1. **Skenario Visite dan Peresepan Pasien Rawat Inap**:
   - Buka ruang kerja dokter rawat inap untuk pasien teradmisi aktif.
   - Isi form SOAP harian, lalu klik tombol **"Selesaikan & Kunci"**.
   - Verifikasi catatan berhasil ditandatangani dan berstatus `Terkunci / Completed`.
   - Periksa tabel `RegPatientEncounter`: pastikan statusnya **TIDAK** berubah menjadi `ConsultationCompleted`.
   - Buka tab **Resep**, pilih obat dari katalog, tentukan dosis dan aturan pakai, lalu klik **"Simpan Draf Resep"**.
   - Verifikasi resep berhasil tersimpan (`201 Created`) dan muncul di Riwayat Resep.
2. **Skenario Uji Regresi Pasien Rawat Jalan (Poliklinik)**:
   - Buka ruang kerja dokter poliklinik rawat jalan.
   - Selesaikan konsultasi dokter hingga selesai (`Completed`).
   - Coba buat resep baru melalui API/UI pada konsultasi poliklinik tersebut: pastikan sistem **tetap menolak** dengan pesan galat kanonikal *"Konsultasi yang sudah completed tidak dapat ditambahkan resep."*.

---

## 9. Riwayat Dokumen

| Tanggal | Perubahan | Oleh |
| :--- | :--- | :--- |
| 2026-10-20 | Dokumen rencana perbaikan dibuat (3 perbaikan, prioritas 1 & 2, status MENUNGGU_PERSETUJUAN) | `diagnose-module-issue` |
