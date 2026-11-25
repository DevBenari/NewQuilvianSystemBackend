# TLM-NV-002 — Pembacaan Akhir Penempatan Bed dan Butir Menu Konsol Panggilan oleh Nurse View

```yaml
dokumen_id: TLM-NV-002
jenis: pemberitahuan ketergantungan baca lintas modul + kabar butir menu
modul_peminta: nurse-view
pemilik_peminta: Rizki Gunawan
modul_tujuan: rawat-inap
submodul_tujuan: episode-rawat-inap
pemilik_tujuan: Muhammad Hamzah
tanggal: "2026-11-05"
status: DIJAWAB — ditulis 2026-11-05, disampaikan Rizki Gunawan dan dijawab Muhammad Hamzah 2026-11-09 (bagian 8); tidak ada permintaan perubahan kode
keputusan_asal: [NV-DEC-046, NV-DEC-050, NV-DEC-052, NV-DEC-055, NV-DEC-056, NV-DEC-057, NV-FE-008]
source_sha_backend: "0c6eab34 (rizkiG)"
source_sha_frontend: "91a11b08f (RizkiV2)"
task_terkait: [BE-NV-010, BE-NV-011, BE-NV-013, FE-NV-012]
rujukan:
  - ../../../../nurse-view/00-interview-decisions.md
  - ../../../../nurse-view/01-existing-capability-map.md
  - ../../../../nurse-view/contracts/integration-contract.md
  - ../../../../nurse-view/roadmap/backend-roadmap.md
  - ../../../../nurse-view/roadmap/frontend-roadmap.md
  - ./TLM-NV-001-pembacaan-data-bed-dan-episode-oleh-nurse-view.md
```

---

## 1. Ringkasan

Nurse View revision 2 menambah **tablet di setiap bed** untuk memanggil perawat, **strip panggilan
aktif** di TV nurse station, dan **Konsol Panggilan** di PC nurse station. Desainnya disetujui Rizki
Gunawan pada 5 November 2026 (`NV-DEC-055`).

Dokumen ini memberi tahu dua hal kepada Muhammad Hamzah:

1. Nurse View kini **membaca satu data lagi** milik `episode-rawat-inap`, yaitu akhir penempatan
   bed (`InpBedPlacement.EndDateTime` dan `EndReason`). Nurse View **tidak mengubah kode, tabel,
   maupun data** Rawat Inap.
2. Butir menu baru "Konsol Panggilan Pasien" ditempatkan di **grup menu Rawat Inap**.

Keduanya pemberitahuan, bukan permintaan persetujuan. Tujuannya agar perubahan di sisi Rawat Inap
yang menyentuh hal ini bisa dikabarkan ke Rizki lebih dulu.

## 2. Data yang dibaca

Tambahan atas data yang sudah tercatat di `TLM-NV-001`.

| Data | Sumber | Dipakai untuk | Seberapa sering |
|---|---|---|---|
| Penempatan aktif satu bed | `InpBedPlacement` dengan `BedId` tablet dan `EndDateTime IS NULL` (unik per bed, `IX_InpBedPlacement_BedId_Active`, `INV-INP-02`) | Tablet menampilkan "Bed belum berpenghuni" bila tidak ada; panggilan baru ditolak (`NV-DEC-046`) | Setiap tablet memuat keadaannya tiap 15 detik selama ada panggilan, tiap 60 detik bila tidak |
| Penempatan saat panggilan dibuat | `InpBedPlacement.Id` dan `EpisodeId`, disalin ke panggilan sebagai riwayat | Menandai panggilan milik penempatan mana; membaca inisial pasien di konsol lewat episode | Sekali per panggilan baru |
| Akhir penempatan | `InpBedPlacement.EndDateTime` dan `EndReason` (enum `InpBedPlacementEndReason`) untuk panggilan yang masih aktif | Menutup panggilan otomatis saat pasien pindah atau pulang (`NV-DEC-050`, `052`) | Pemeriksaan berkala Nurse View tiap 30 detik, hanya atas panggilan yang masih aktif |

**Arti `EndReason` yang dipakai Nurse View.** Pemetaan di `BE-NV-013`, sesudah diubah `NV-DEC-057` (2026-11-09) atas jawaban bagian 8. Pemeriksaan lebih dulu mengikuti `SupersededByCorrectionId` sampai baris penempatan terkini:

| `EndReason` | Yang dilakukan Nurse View pada panggilan aktif | Alasan tercatat |
|---|---|---|
| `Transfer` | Ditutup sistem | "pasien pindah" |
| `EpisodeClosed`, `PatientDeparted` | Ditutup sistem | "pasien pulang" |
| `AdmissionCancelled` | Ditutup sistem | "admisi dibatalkan" |
| `CorrectedEntry`, pengganti di bed lain | Ditutup sistem | "pasien pindah" |
| `CorrectedEntry`, pengganti di bed yang sama | **Tidak** ditutup; dinilai ulang sampai pengganti berakhir | — |

**Contoh.** Pukul 10.05.00 petugas admisi memindahkan pasien 331-C ke 402-B lewat layar Rawat
Inap. Rawat Inap mengakhiri penempatan 331-C dengan `EndReason = Transfer`, seperti biasa, tanpa
tahu Nurse View ada. Paling lambat 10.06.00, pemeriksaan berkala Nurse View membaca akhir
penempatan itu dan menutup panggilan 331-C dengan alasan "pasien pindah". Tablet 331-C lalu
menampilkan "Bed belum berpenghuni".

**Contoh beban baca.** Lantai 3 berisi 40 tablet. Saat tidak ada panggilan, 40 tablet memuat
keadaan tiap 60 detik, yaitu sekitar 40 pembacaan satu baris berindeks per menit. Saat 5 bed
sedang memanggil, kelima tablet itu memuat tiap 15 detik, sehingga bertambah sekitar 15 pembacaan
per menit. Pemeriksaan berkala berjalan dua putaran per menit untuk seluruh rumah sakit; setiap
putaran membaca akhir penempatan hanya untuk panggilan yang masih aktif.

## 3. Yang sengaja tidak dilakukan Nurse View

| Tidak dilakukan | Alasan |
|---|---|
| Menambah titik terbit (event) di `InpBedOccupancyService` atau `InpEpisodeService` | Ditolak owner Nurse View (`NV-DEC-052`); pemeriksaan berkala cukup dan tidak menyentuh kode Rawat Inap |
| Memakai event `InpatientBedSelectionChanged` | Payload memuat `PatientName`; tetap dilarang untuk akun perangkat Nurse View (`NV-DEC-026`, `TLM-NV-001` bagian 3) |
| Menulis ke tabel Rawat Inap | Nurse View hanya membaca (`04-prd-to-mvp.md` R2.15) |
| Memindahkan panggilan ikut pasien ke bed baru | Panggilan melekat ke bed (`NV-DEC-046`, `050`) |

## 4. Butir menu Konsol Panggilan di grup Rawat Inap (`NV-FE-008`)

| Hal | Isi |
|---|---|
| Butir baru | "Konsol Panggilan Pasien" (nama akhir `DEV_DISCRETION`) |
| Letak | Tingkat 1 di bawah grup tingkat 0 "Rawat Inap" pada `src/utils/menu-sidebar/menu-items.jsx` (baris 2059 di `91a11b08f`) |
| Route | `/health-services/nurse-view-management/patient-call-console` — milik Nurse View |
| Hak akses | `requiredPermission` `NurseViewPatientCall : Read`; pengguna tanpa izin tidak melihat butir ini |
| Pengguna | Perawat rawat inap dan kepala ruang, dengan akun masing-masing |
| Dampak pada butir Rawat Inap yang ada | Tidak ada: tidak ada butir yang diubah, dipindah, atau diganti namanya |
| Task | `FE-NV-012` |
| Alternatif bila Hamzah keberatan | Butir tingkat 0 sendiri bernama "Nurse View" (`03-frontend-architecture.md` R2.2). Mengubah letak ini menjadi keputusan baru Rizki Gunawan |

Penempatan ini sudah disahkan Rizki Gunawan (`NV-DEC-055`) dan perubahan berkas `menu-items.jsx`
sudah direstui lead (`NV-DEC-056`). Restu lead tidak menggantikan kabar ke pemilik grup menu.

## 5. Dampak bagi episode-rawat-inap

| Aspek | Dampak |
|---|---|
| Kode, tabel, migration, endpoint | **Tidak ada perubahan** |
| Perilaku transfer, pemulangan, koreksi, dan pembatalan penempatan | Tidak berubah |
| Hak akses Rawat Inap | Tidak berubah |
| Grup menu Rawat Inap | Bertambah satu butir milik Nurse View (bagian 4) |

## 6. Yang dibutuhkan dari Muhammad Hamzah

| No | Hal | Keterangan |
|---:|---|---|
| 1 | **Mohon kabar lebih dulu** bila salah satu berikut berubah | Nilai atau arti `InpBedPlacementEndReason`; cara `EndDateTime` diisi saat transfer, pemulangan, koreksi, atau pembatalan; index `IX_InpBedPlacement_BedId_Active`; alur `InpBedOccupancyService#TransferCoreAsync`, `#ReleaseActivePlacementAsync`, atau `InpEpisodeService#ReleaseBedHoldsAsync` |
| 2 | **Kabar penempatan butir menu** (bagian 4) | Tidak butuh jawaban bila tidak keberatan. Bila keberatan, sampaikan ke Rizki |

## 7. Status dan tindak lanjut

| Tanggal | Status | Catatan |
|---|---|---|
| 2026-11-05 | `PEMBERITAHUAN` | Ditulis `plan-module-delivery` Nurse View revision 2, sesuai `contracts/integration-contract.md` R2 §5. Belum disampaikan; Rizki Gunawan yang menyampaikannya ke Muhammad Hamzah. Sebaiknya disampaikan sebelum `BE-NV-013` dimulai, tetapi dokumen ini tidak menahan task mana pun |
| 2026-11-09 | `PEMBERITAHUAN` | Pemetaan bagian 2 dibangun persis di `BE-NV-013` (status 🟡: source selesai, build dan uji runtime belum). Pemeriksaan hanya membaca `EndDateTime` dan `EndReason` penempatan milik panggilan aktif; tidak ada perubahan kode Rawat Inap. Dokumen ini masih belum disampaikan. Laporan: [`BE-NV-013.md`](../../../../nurse-view/task/report/backend/BE-NV-013.md) |
| 2026-11-09 | `DIJAWAB` | Disampaikan Rizki Gunawan bersama sepuluh pertanyaan; dijawab Muhammad Hamzah hari yang sama (bagian 8). Dua jawaban berbeda dengan pemetaan bagian 2 (`AdmissionCancelled`, `CorrectedEntry` beda bed) dan menunggu keputusan Rizki Gunawan; pemetaan bagian 2 belum diubah |
| 2026-11-09 | `DITINDAKLANJUTI` | Rizki Gunawan memutuskan `NV-DEC-057`: batal admisi dan koreksi ke bed lain menutup panggilan; rantai koreksi diikuti. Pemetaan bagian 2 diperbarui; source `BE-NV-013` disesuaikan. Tetap tanpa perubahan kode Rawat Inap |
| 2026-11-09 | `DIUJI` | Uji `BE-NV-013` di DB dev bersama ✅ 9/9 `PASS`. Pasien 326-C ternyata wajib ruang isolasi dan tidak punya bed tujuan layak, sehingga atas keputusan Rizki Gunawan pasien yang dipindah diganti ke **325-A** (326-C hanya pembanding, tidak dipindah). 325-A dipindah ke 420-B (sekelas) pukul 11.52.03 WIB dan dikembalikan pukul 11.52.45 (singgah 42 detik); kedua bed dibersihkan dan diverifikasi siap lewat bed-management dan kembali ke keadaan semula. Sisa permanen: 2 baris penempatan `Transfer` pada episode 325-A, 2 outbox `BED_OCCUPIED`, 2 upaya pembersihan. Laporan: [`BE-NV-013.md`](../../../../nurse-view/task/report/backend/BE-NV-013.md) §5.3 |

`dokter-rawat-inap`, `keperawatan`, dan `integrasi-billing` **tidak tersentuh** revision 2, sehingga
tidak diberi dokumen.

## 8. Jawaban Muhammad Hamzah — 2026-11-09

Pertanyaan disampaikan Rizki Gunawan; jawaban dicatat sesuai isi yang diteruskan Rizki.

| No | Pertanyaan | Jawaban | Tindak lanjut di Nurse View |
|---:|---|---|---|
| 1 | Arti `EndReason` | `Transfer` benar (pindah bed). `EpisodeClosed`, `PatientDeparted` benar (pulang/kepergian fisik). `CorrectedEntry` benar bersyarat: koreksi administratif (kelas/jam) di bed yang sama → panggilan tetap aktif; koreksi salah pilih bed (`CorrectedBedId` berbeda) → pasien secara fisik dipindah ke bed lain. `AdmissionCancelled` **keliru**: itu pembatalan admisi (pasien batal dirawat, episode dibatalkan, bed dilepas), pasien tidak menempati bed, sehingga panggilan wajib ditutup | Diputuskan Rizki Gunawan 2026-11-09 (`NV-DEC-057`): `AdmissionCancelled` ditutup dengan alasan baru "admisi dibatalkan"; koreksi ke bed lain ditutup "pasien pindah". Source mendukung jawaban ini: `InpEpisodeService.CancelAdmissionAsync` membatalkan episode `Admitted`, lalu `ReleaseBedHoldsAsync` mengakhiri penempatan dan melepas bed |
| 2 | Rencana perubahan enum/`EndDateTime` | Tidak ada. Nilai 1–5 dan semantik `EndDateTime` stabil dan terkunci di kontrak | Tidak ada |
| 3 | Kapan transfer dicatat | Saat pasien benar-benar pindah. Rencana pindah memakai reservasi (`InpBedReservation`), bukan API transfer | Menjawab sisi alur sistem `NV-UNK-006`; pengamatan lapangan `G-16` tetap milik kepala ruang |
| 4 | Frekuensi koreksi di bed yang sama | Sangat jarang (hanya salah input data admisi). **Disarankan tetap ditambal** di Nurse View: baca penempatan aktif terkini, telusuri rantai `SupersededByCorrectionId`, atau ikat ke `EpisodeId` + `BedId` | Diputuskan Rizki Gunawan 2026-11-09 (`NV-DEC-057`): ditambal di `BE-NV-013` dengan mengikuti rantai `SupersededByCorrectionId` |
| 5 | Butir menu "Konsol Panggilan Pasien" di grup Rawat Inap | Tidak keberatan; letak tingkat 1 grup "Rawat Inap" tepat untuk stasiun perawat bangsal | `NV-FE-008` tertutup; `FE-NV-012` memakai letak bagian 4 |
| 6 | Cacat koreksi salah catat (`ApplyPlacementCorrectionCoreAsync`) | Sependapat, cacat nyata; belum ada di backlog, dicatat untuk dibuatkan tiket perbaikan | Milik tim Rawat Inap; Nurse View tidak memperbaiki |
| 7 | Cara membebaskan bed yang tertahan | Belum ada cara resmi lewat aplikasi; saat ini hanya intervensi database (mengoreksi `InpBedReadiness` ke `WaitingCleaning` atau mereset kesiapan bed) | Koreksi penempatan tetap tidak dipakai untuk uji Nurse View |
| 8 | Pasien 326-C untuk uji pindah pulang-balik | Tidak sedang dipakai tim Rawat Inap; boleh dipakai | Uji `BE-NV-013` boleh dijalankan |
| 9 | Dampak ke Billing | Aman dari galat pembuatan folio. Tarif dihitung dari baris `InpBedPlacement`; pindah bolak-balik singkat (< 1 menit) dan/atau di kelas yang sama tidak membuat tagihan ganda, tetapi ketiga segmen tetap terlihat di rincian audit segmen kamar | Skrip uji memakai bed tujuan sekelas (`SameGrade`) |
| 10 | Efek samping lain | Sensus dan papan bed dihitung langsung dari penempatan aktif, normal kembali begitu pasien kembali. Laporan pindah kamar menyimpan 2 baris riwayat permanen; wajar untuk DB dev | Dicatat di laporan uji `BE-NV-013` |
