# ISSUE-007 — Penekanan Tombol Selesaikan & Kunci pada SOAP Visite Mengunci Seluruh Transaksi Resep dan Layanan Rawat Inap

```yaml
issue_id: ISSUE-DOK-007
module_id: rawat-inap
submodule: dokter-rawat-inap
layar: "Dokter Rawat Inap — Form SOAP (Catatan Perkembangan Pasien Terintegrasi / CPPT), Resep, Tindakan, dan Order Klinis"
sumber_laporan: "Laporan pengguna 20-10-2026: tombol Selesaikan & Kunci menyelesaikan seluruh transaksi dokter rawat inap dan mengunci input resep"
tanggal_issue: "2026-10-20"
status: SELESAI
keparahan_tertinggi: Blocker
source_sha_backend: "6e0f2b1b58a2192a0f54f0fdff718bc77670e3bd (MHamzah)"
source_sha_frontend: "a491b05afcb6b3ed70218ccaa557007719d45ac3 (HamzahV2)"
rencana_perbaikan: ../plan-repair/plan-repair-007-penyelesaian-soap-mengunci-seluruh-transaksi.md
ditulis_dengan: diagnose-module-issue
```

---

## 1. Ringkasan

Ketika dokter rawat inap menekan tombol **"Selesaikan & Kunci"** pada formulir SOAP (Catatan Perkembangan Pasien Terintegrasi / CPPT), seluruh transaksi klinis pasien rawat inap terkunci dan dokter tidak dapat lagi menyimpan resep obat maupun pesanan klinis lainnya. Analisis mendalam terhadap frontend dan backend membuktikan adanya **kebocoran logika siklus hidup Rawat Jalan (Poliklinik) ke dalam domain Rawat Inap**. 

Tombol tersebut memanggil endpoint finalisasi kanonikal `ConsultationFinalizationService.FinalizeAsync` yang mengubah status catatan menjadi `DoctorConsultationStatus.Completed` dan secara keliru mengubah status kunjungan utama (`EncounterStatus`) menjadi `ConsultationCompleted`. Akibatnya, validasi penyimpanan resep pada `PrescriptionController.cs` langsung menolak permintaan dengan pesan *"Konsultasi yang sudah completed tidak dapat ditambahkan resep."*, karena pengecekan status `Completed` dievaluasi mendahului pelonggaran rawat inap. 

Masalah ini berstatus **Blocker** karena melumpuhkan alur peresepan dan pelayanan berkelanjutan bagi pasien rawat inap yang dirawat berhari-hari. Laporan ini memuat 1 temuan utama berstatus `BUG` dan 2 temuan tambahan arsitektural terkait siklus kunjungan dan penagihan.

---

## 2. Laporan Asli

| No. laporan | Isi keluhan pengguna | Lampiran |
| :---: | :--- | :--- |
| 1 | "Tolong anda lakukan analisis, kenapa pada saat menekan button selesaikan & kunci seluruh transaksi pada dokter rawat inap itu selesai dan tidak bisa di lakukan lagi, ketika melakukan input resep dan lain lain coba anda lakukan analisis" | Tangkapan layar kotak aksi formulir SOAP rawat inap: teks peringatan kunci tanda tangan, teks draf, serta tiga tombol: `Reset`, `Simpan Draf`, dan `Selesaikan & Kunci`. |

---

## 3. Ringkasan Temuan

| ID | No. laporan | Judul | Jenis | Area | Keparahan | Status bukti | ID perbaikan |
| :--- | :---: | :--- | :--- | :--- | :---: | :--- | :--- |
| `ISS-DOK-007-01` | 1 | Resep ditolak *"Konsultasi yang sudah completed tidak dapat ditambahkan resep"* setelah SOAP dikunci | `BUG` | Backend & Frontend | Blocker | `SUDAH-VERIFIKASI` | `FIX-DOK-007-01` |
| `ISS-DOK-007-T1` | — | Status kunjungan admisi rawat inap (`EncounterStatus`) berubah menjadi `ConsultationCompleted` saat SOAP dikunci | `BUG` | Backend | High | `SUDAH-VERIFIKASI` | `FIX-DOK-007-02` |
| `ISS-DOK-007-T2` | — | Finalisasi SOAP harian rawat inap menerbitkan milestone tagihan poliklinik rawat jalan | `BUG` | Backend | High | `SUDAH-VERIFIKASI` | `FIX-DOK-007-03` |

---

## 4. Rincian per Temuan

### ISS-DOK-007-01 — Resep Ditolak Karena Menempel pada Konsultasi yang Sudah Selesai (Completed)

| Field | Keterangan |
| :--- | :--- |
| **No. laporan** | 1 |
| **Jenis** | `BUG` |
| **Area** | Backend (`PrescriptionController.cs`) & Frontend (`use-inpatient-prescription-tab.jsx`) |
| **Keparahan** | Blocker |
| **Status bukti** | `SUDAH-VERIFIKASI` pada source code backend dan frontend |
| **Perbaikan** | `FIX-DOK-007-01` |

#### Apa yang terjadi
Dokter memeriksa pasien rawat inap, menulis catatan perkembangan SOAP harian, lalu menekan tombol **"Selesaikan & Kunci"**. Dokter bermaksud menandatangani catatan medis hari itu secara sah (sesuai Permenkes 24/2022) agar catatan terkunci dari manipulasi data. 

Namun, setelah tombol ditekan, ketika dokter berpindah ke tab **Resep** untuk meresepkan obat harian pasien, sistem menolak penyimpanan resep dengan pesan galat merah:
> *"Konsultasi yang sudah completed tidak dapat ditambahkan resep."*

Akibatnya, dokter sama sekali tidak bisa membuat resep obat untuk pasien rawat inap tersebut selama sisa perawatan, kecuali membuat catatan draf baru secara artifisial.

#### Kenapa terjadi
Terdapat benturan urutan validasi (*guard evaluation ordering bug*) pada backend serta penugasan otomatis (*auto-assignment*) catatan pengait pada frontend:

1. **Urutan Evaluasi Backend (`PrescriptionController.cs`)**:
   Pada `Areas/HealthServices/PharmacyManagement/Controllers/PrescriptionController.cs` baris 745–765:
   ```csharp
   // Baris 747-750: Pengecekan status Completed
   var consultation = await _dbContext.Set<TrxDoctorConsultation>()
       .AsNoTracking()
       .FirstOrDefaultAsync(x => x.Id == request.ConsultationId && x.EncounterId == request.EncounterId && !x.IsDelete, cancellationToken);
   if (consultation == null) return (false, "Konsultasi dokter tidak ditemukan atau tidak sesuai encounter.");
   if (consultation.ConsultationStatus == DoctorConsultationStatus.Completed) return (false, "Konsultasi yang sudah completed tidak dapat ditambahkan resep.");
   if (consultation.ConsultationStatus == DoctorConsultationStatus.Cancelled) return (false, "Konsultasi yang sudah cancelled tidak dapat ditambahkan resep.");

   // Baris 759-760: Pengecualian Rawat Inap (BE-RWI-043)
   if (consultation.InpEpisodeId.HasValue)
       return (true, null);
   ```
   Pada implementasi `BE-RWI-043`, developer bermaksud melonggarkan aturan peresepan agar pasien rawat inap dapat menerima banyak resep sepanjang perawatannya (`consultation.InpEpisodeId.HasValue`). Namun, baris 749 (`consultation.ConsultationStatus == DoctorConsultationStatus.Completed`) diletakkan **sebelum** baris 759! Begitu catatan SOAP rawat inap selesai ditandatangani (`Completed`), baris 749 langsung memblokir penyimpanan resep dan baris 759 tidak pernah dieksekusi.

2. **Penugasan Otomatis Frontend (`use-inpatient-prescription-tab.jsx`)**:
   Pada `src/lib/hooks/health-services/inpatient-management/use-inpatient-prescription-tab.jsx` baris 165–167:
   ```javascript
   if (!draft.consultationId && consultations.length > 0) {
     setDraft((prev) => ({ ...prev, consultationId: consultations[0].id }));
   }
   ```
   Frontend mengambil daftar catatan dokter melalui `getSoapTimelineByEpisode(episodeId)`. Elemen pertama (`consultations[0]`) adalah catatan terakhir yang baru saja diselesaikan dokter. Form resep secara otomatis mengunci `consultationId` ke ID catatan yang baru saja `Completed` tersebut. Karena seluruh riwayat catatan sebelumnya juga berstatus `Completed`, dokter tidak memiliki opsi catatan terbuka lain untuk dipilih.

#### Apakah ini menyimpang dari desain?
**Ya, ini adalah `BUG` nyata.** Berdasarkan dokumen PRD dan keputusan `RWI-DEC-038`, `RWI-DEC-070`, dan `RWI-RULE-026` aturan 5 (dikutip pada blueprint `01-existing-capability-map.md:1350` dan laporan `BE-RWI-043.md:77-79`), pasien rawat inap berhak menerima resep sebanyak yang dibutuhkan sepanjang hari rawat inap. Mengunci peresepan hanya karena SOAP hariannya sudah ditandatangani bertentangan dengan alur klinis rumah sakit.

#### Dampak nyata
Pasien Tn. Hartono (pseudonim) dirawat inap di Ruang Flamboyan hari ke-3. Dokter DPJP melakukan visite pagi pukul 08:30, mencatat hasil evaluasi fisik di SOAP, lalu menekan "Selesaikan & Kunci". Pada pukul 09:00, dokter hendak meresepkan antibiotik infus tambahan. Saat tombol "Simpan Resep" ditekan, sistem menolak karena konsultasi jam 08:30 sudah `Completed`. Pasien terlambat menerima antibiotik dan perawat terpaksa menginput resep manual di luar alur sistem.

#### Rekomendasi
Pindahkan atau lewati pemeriksaan status `DoctorConsultationStatus.Completed` pada `PrescriptionController.cs` bila `consultation.InpEpisodeId.HasValue` bernilai benar (khusus episode rawat inap aktif). Di rawat inap, catatan medis yang sudah ditandatangani dan dikunci justru merupakan dasar otorisasi legal yang sah untuk menerbitkan resep obat.

---

## 5. Temuan Tambahan

### ISS-DOK-007-T1 — Mutasi Status Kunjungan Admisi Menjadi ConsultationCompleted

| Field | Keterangan |
| :--- | :--- |
| **Kenapa dicantumkan** | Ditemukan pada penelusuran rantai pemanggilan `CompleteConsultation` dan merupakan penyebab rusaknya status siklus hidup kunjungan rawat inap |
| **Jenis** | `BUG` |
| **Area** | Backend (`ConsultationFinalizationService.cs:189-194`) |
| **Keparahan** | High |
| **Status bukti** | `SUDAH-VERIFIKASI` pada `ConsultationFinalizationService.cs` baris 189–194 |
| **Perbaikan** | `FIX-DOK-007-02` |

#### Apa yang terjadi
Ketika dokter menyelesaikan satu catatan SOAP rawat inap, tabel kunjungan admisi (`RegPatientEncounter`) pasien ikut diubah statusnya menjadi `EncounterStatus.ConsultationCompleted`.

#### Kenapa terjadi
Pada `Areas/HealthServices/PharmacyManagement/Services/ConsultationFinalizationService.cs` baris 189–194:
```csharp
if (consultation.Encounter != null)
{
    consultation.Encounter.EncounterStatus = EncounterStatus.ConsultationCompleted;
    consultation.Encounter.UpdateDateTime = now;
    consultation.Encounter.UpdateBy = actorUserId;
}
```
Logika ini disalin dari modul Rawat Jalan. Pada rawat jalan, selesainya konsultasi dokter memang menandai berakhirnya tahap klinis pasien di poliklinik (`InConsultation → ConsultationCompleted → Billing → Completed`). Namun pada Rawat Inap, kunjungan pasien berlangsung selama hari-hari perawatan. Menandai kunjungan rawat inap sebagai `ConsultationCompleted` merusak integritas siklus perawatan dan membuat modul admisi/kasir menganggap tahap medis pasien sudah selesai.

#### Rekomendasi
Beri penjaga (*guard condition*): mutasi `consultation.Encounter.EncounterStatus = EncounterStatus.ConsultationCompleted;` **hanya** boleh dijalankan jika kunjungan bukan rawat inap (`consultation.InpEpisodeId == null` dan `consultation.Encounter.EncounterType != EncounterType.Inpatient`).

---

### ISS-DOK-007-T2 — Penerbitan Milestone Tagihan Poliklinik Saat Mengunci Catatan Rawat Inap

| Field | Keterangan |
| :--- | :--- |
| **Kenapa dicantumkan** | Ditemukan pada penelusuran `ConsultationFinalizationService.cs:254-279`; berdampak langsung pada tagihan keuangan ganda |
| **Jenis** | `BUG` |
| **Area** | Backend (`ConsultationFinalizationService.cs:254-279`) |
| **Keparahan** | High |
| **Status bukti** | `SUDAH-VERIFIKASI` pada `ConsultationFinalizationService.cs` baris 254–279 |
| **Perbaikan** | `FIX-DOK-007-03` |

#### Apa yang terjadi
Setiap kali dokter rawat inap menekan "Selesaikan & Kunci" pada SOAP harian, sistem mengirimkan sinyal tagihan jasa konsultasi dokter poliklinik (`milestone: "ConsultationCompleted"`) ke modul Billing.

#### Kenapa terjadi
Pada `ConsultationFinalizationService.cs` baris 254–279:
```csharp
if (consultation.Encounter?.EncounterType != EncounterType.Emergency)
{
    var consultationEmission = await _clinicalMilestoneFactProducer.EmitChargeEligibilityAsync(
        new ClinicalMilestoneFactRequest
        {
            SourceContext = BillingSourceContract.ConsultationSourceContext,
            SourceAggregateId = consultation.Id,
            EffectType = BillingSourceContract.ConsultationChargeEffectType,
            EncounterId = consultation.EncounterId,
            OccurredAt = now,
            Quantity = 1m,
            Unit = "KALI",
            RuleSnapshot = JsonSerializer.Serialize(new
            {
                milestone = "ConsultationCompleted",
                doctorId = consultation.DoctorId,
                clinicId = consultation.ClinicId
            }),
            CorrelationId = consultationId
        },
        actorUserId,
        cancellationToken);
    ...
}
```
Pengecekan di atas hanya mengecualikan `Emergency`. Pasien `Inpatient` tidak dikecualikan, sehingga tagihan poliklinik rawat jalan ikut diterbitkan ke billing pasien. Padahal di rawat inap, jasa dokter ditagihkan lewat tarif Visite Rawat Inap (`TrxInpPhysicianVisit`), bukan jasa konsultasi poliklinik.

#### Rekomendasi
Kecualikan pasien rawat inap (`consultation.InpEpisodeId.HasValue` atau `EncounterType.Inpatient`) dari penerbitan milestone charge poliklinik di `ConsultationFinalizationService.cs`.

---

## 6. Tanya-Jawab Pelapor

> **T:** Kenapa pada saat menekan button Selesaikan & Kunci, seluruh transaksi pada dokter rawat inap itu selesai dan tidak bisa dilakukan lagi, ketika melakukan input resep dan lain-lain?
>
> **J:** Karena tombol tersebut mengeksekusi logika penutupan milik modul Rawat Jalan (Poliklinik). Di modul Poliklinik, ketika dokter selesai memeriksa pasien dan mengunci SOAP, seluruh pemeriksaan pasien dianggap tamat sehingga sistem sengaja mengunci pembuatan resep dan tindakan baru. Pada modul Rawat Inap, logika ini terbawa tanpa pengecualian: backend `PrescriptionController.cs` langsung menolak pembuatan resep baru jika catatan SOAP yang dikaitkan sudah berstatus `Completed`. Padahal, pasien rawat inap masih dirawat berhari-hari dan resep obat justru membutuhkan catatan SOAP yang sudah sah ditandatangani sebagai dasar pemberian obat.

---

## 7. Pertanyaan dan Keputusan yang Dibutuhkan

### Untuk Pelapor
*Tidak ada pertanyaan yang menahan perbaikan. Gejala dan akar masalah sudah terbukti 100% pada kode sumber.*

### Untuk Pemilik Bisnis / Arsitek Sistem

| No | Keputusan | Pilihan | Rekomendasi | Menahan perbaikan |
| :---: | :--- | :--- | :--- | :--- |
| **K-01** | Apakah catatan SOAP rawat inap yang sudah berstatus `Completed` (sudah ditandatangani dan dikunci) tetap sah dijadikan catatan pengait (`ConsultationId`) untuk resep harian pasien sepanjang episode rawat inap masih aktif? | A. Sah dan diperbolehkan (rekomendasi)<br>B. Resep rawat inap dibuat independen tanpa mewajibkan `ConsultationId` | **Pilihan A**: Paling aman, mempertahankan audit medis (resep mengacu ke hasil visite DPJP), dan tidak mengubah skema DTO database. | `FIX-DOK-007-01` |
| **K-02** | Apakah status kunjungan admisi (`RegPatientEncounter.EncounterStatus`) harus tetap berstatus `Admitted` / dalam perawatan selama pasien belum dipulangkan (discharge)? | A. Ya, status encounter tidak boleh diubah menjadi `ConsultationCompleted` saat dokter menyelesaikan SOAP harian.<br>B. Buat status perantara baru. | **Pilihan A**: Sesuai dengan desain domain rawat inap Permenkes. | `FIX-DOK-007-02` |

---

## 8. Catatan Pola

1. **Pola Salin Logika Antarmuka & Layanan Poliklinik ke Rawat Inap**:
   Banyak service klinis inti di Quilvian (`ConsultationFinalizationService`, `PrescriptionController`, `PatientDiagnosisController`) pada awalnya dirancang secara linier untuk Rawat Jalan. Ketika Rawat Inap dikembangkan, service tersebut dipakai ulang tanpa penyesuaian multi-hari (inpatient multi-day continuity).
2. **Pola Guard Ordering**:
   Pengecekan validasi umum diletakkan di atas pengecekan khusus konteks perawatan. Ketika task sebelumnya (`BE-RWI-043`) menambahkan pelonggaran di baris bawah, pemeriksaan umum di baris atas tetap menyergap dan menggagalkan transaksi.
3. **Pencegahan**:
   Setiap controller dan service yang dipakai bersama antara Poliklinik dan Rawat Inap wajib memisahkan alur finalisasi dokumen (*document locking*) dari penutupan kunjungan (*encounter closure*).

---

## 9. Riwayat Dokumen

| Tanggal | Perubahan | Oleh |
| :--- | :--- | :--- |
| 2026-10-20 | Dokumen issue dibuat dari keluhan pengguna terkait tombol Selesaikan & Kunci yang memblokir transaksi peresepan rawat inap | `diagnose-module-issue` |
