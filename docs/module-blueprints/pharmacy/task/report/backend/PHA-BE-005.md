# PHA-BE-005 — Gerbang Penahanan pada Empat Titik

## Ringkasan untuk Pembaca Umum

Izin mengerjakan sebuah resep dapat dicabut di tengah jalan: pembayaran dibatalkan, penjaminan
ditarik, atau tagihannya berubah. Sebelum task ini, pencabutan itu hanya tercatat dan tidak
menghentikan apa pun — apoteker tetap dapat menelaah, meracik, dan menyerahkan obat yang izinnya
sudah tidak ada.

Task ini memasang gerbang di empat titik: memulai telaah apoteker, memulai dan menyelesaikan
penyiapan, menyelesaikan telaah obat akhir, dan menyerahkan obat. Menjaga penyerahan saja tidak
cukup — apoteker akan terlanjur meracik, lalu pekerjaannya terbuang.

Dua hal yang sengaja **tidak** dilakukan. Pertama, pekerjaan yang sudah berjalan **tidak ditarik
mundur**: resep yang sedang disiapkan tetap tercatat sedang disiapkan, dan racikan yang sudah
dibuat tidak dikembalikan menjadi bahan — obat yang sudah diracik memang tidak dapat dibatalkan
secara fisik. Ketika izinnya pulih, pekerjaan dilanjutkan dari titik terakhir; apoteker tidak
meracik dua kali. Kedua, **tidak ada tombol darurat** bagi siapa pun, termasuk Kepala Farmasi dan
Supervisor. Gangguan sinkronisasi bukan keadaan darurat klinis, dan tombol semacam itu hanya akan
menjadi jalan pintas yang selalu ditekan.

Keadaan yang belum diketahui dan salinan yang tertinggal ikut ditolak. Ketiadaan surat bukan izin.

---

- TASK ID: PHA-BE-005
- TASK TYPE: Fitur (gerbang penahanan finansial pada empat titik pekerjaan Farmasi)
- COMPLEXITY: MEDIUM
- MODEL: Claude Opus 5
- TASK MODE: BACKEND
- WRITE TARGET: `NewQuilvianSystemBackend` — `Areas/HealthServices/PharmacyManagement/Services/**`
- FILES INSPECTED:
  - `docs/module-blueprints/pharmacy/roadmap/backend-roadmap.md` (`PHA-BE-005`)
  - `docs/module-blueprints/pharmacy/contracts/state-transition-matrix.md`, `validation-matrix.md`
  - `Areas/HealthServices/PharmacyManagement/Services/PrescriptionReviewService.cs`, `PrescriptionPreparationService.cs`, `PrescriptionFinalCheckService.cs`, `PrescriptionDispensingService.cs`
- FILES CHANGED:
  - **Diperbarui**: `Services/PrescriptionFinancialClearanceService.cs` (`EvaluateGateAsync`, `EnsureGateAllowedAsync`, enum `PrescriptionClearanceGate`, record `PrescriptionClearanceGateResult`)
  - **Diperbarui**: `Services/PrescriptionReviewService.cs` (gerbang 1)
  - **Diperbarui**: `Services/PrescriptionPreparationService.cs` (gerbang 2, pada mulai **dan** selesai)
  - **Diperbarui**: `Services/PrescriptionFinalCheckService.cs` (gerbang 3)
  - **Diperbarui**: `Services/PrescriptionDispensingService.cs` (gerbang 4, pada penyiapan penyerahan **dan** eksekusinya)
- IMPLEMENTATION:
  1. **Penanda tahan dihitung, bukan disimpan.** Tidak ada kolom penanda baru pada tabel resep;
     gerbang membaca salinan finansial setiap kali. Menyimpannya berarti menambah keadaan kedua
     yang dapat menyimpang dari salinan yang otoritatif.
  2. **Hanya `CLEARED` beserta hasil finansial yang dikenali yang meloloskan.** `REVOKED`,
     `UNKNOWN`, `PENDING_VERIFICATION`, dan `STALE` menolak seluruh gerbang.
  3. **Kalimat penolakan mengikuti `PHA-VAL-CLEARANCE-v1`.** Gerbang penyerahan memakai
     `PHA_CLR_NOT_SETTLED`; tiga gerbang lain memakai `PHA_CLR_NOT_CLEARED`. Keadaan yang punya
     alasan sendiri — ditahan, belum diketahui, tertinggal — memakai kalimatnya sendiri apa pun
     gerbangnya.
  4. **Keadaan pemenuhan tidak pernah ditarik mundur.** Gerbang menolak tindakan; ia tidak
     menulis keadaan. Penyiapan yang izinnya dicabut berhenti pada `InPreparation` dan tidak naik
     ke `AwaitingFinalCheck`; racikannya tidak disentuh.
  5. **Pemulihan melanjutkan dari titik terakhir.** Karena tidak ada yang ditarik mundur, surat
     pemulihan cukup membuat gerbang meloloskan lagi — antrean, telaah, dan penyiapan yang sudah
     selesai tidak diulang.
  6. **Obat yang sudah diserahkan tidak tersentuh.** Gerbang hanya berjalan pada tindakan;
     resep `Dispensed` tidak melewati satu pun dari keempatnya.
  7. **Nol permukaan override.** Tidak ada parameter, klaim peran, maupun konfigurasi yang dapat
     melewatinya.
- API CONTRACT IMPACT: Nol endpoint baru. Tiga gerbang pertama menolak lewat `InvalidOperationException` yang sudah dipetakan controller menjadi `400`; gerbang penyerahan memakai `PrescriptionDispensingConflictException` yang sudah ada, dengan kode `PHA_CLR_*`.
- DATABASE IMPACT: Nol perubahan skema. Nol kolom penanda baru pada `PhmPrescription`.
- SECURITY IMPACT: Fail-closed pada seluruh keadaan yang tidak pasti. Tidak ada jalur yang memungkinkan obat keluar saat izin finansialnya tidak ada.
- VALIDATION:
  | Command/Check | Result | Classification | Evidence/Note |
  | --- | --- | --- | --- |
  | `dotnet build -p:SkipMigrationMetadata=true` | `0 Error` | VERIFIED | 17,8 detik |
  | `PHA-AT-CLR-04`–`09`, `12` | Terpenuhi pada source | VERIFIED (Logic) | Keempat gerbang menolak pada keadaan yang benar; nol penulisan keadaan pemenuhan oleh gerbang; nol permukaan override |
  | Runtime — nol baris stok bergerak saat penahanan | **Belum** | NOT VERIFIED | Menunggu migration diterapkan pada database uji |
- WARNINGS:
  - **Alur uji yang sudah ada akan tertahan.** Resep yang belum memiliki baris salinan finansial kini ditolak di gerbang pertama dengan `PHA_CLR_UNKNOWN`. Itu perilaku yang benar (ketiadaan surat bukan izin), tetapi berarti setiap pengujian ujung-ke-ujung harus menyediakan surat clearance — bukan sekadar menyetel status resep.
- KNOWN ISSUES:
  - Perpindahan salinan ke `PENDING_VERIFICATION` dan `STALE` belum dijalankan siapa pun: ambang percobaan ulangnya belum ditetapkan. Kedua keadaan sudah **ditolak** gerbang, jadi arah fail-closed-nya sudah benar; yang belum ada adalah yang menaikkannya ke sana.
  - Permukaan pemeriksaan ulang milik Billing (`ReadPrescriptionClearanceAsync`) belum dipanggil saat salinan bermasalah. Konsumsi surat sudah memperbaiki salinan pada jalur baca, sehingga jalur pemulihannya ada; pemanggilan langsung ke Billing menjadi perbaikan berikutnya.
- NEXT TASKS: Penetapan ambang percobaan ulang dan pemakaian permukaan pemeriksaan ulang.
