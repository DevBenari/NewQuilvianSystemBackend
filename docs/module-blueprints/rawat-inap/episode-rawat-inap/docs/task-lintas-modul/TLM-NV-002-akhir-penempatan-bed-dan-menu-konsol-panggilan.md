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
status: PEMBERITAHUAN — ditulis 2026-11-05, belum disampaikan ke Muhammad Hamzah; tidak ada permintaan perubahan kode
keputusan_asal: [NV-DEC-046, NV-DEC-050, NV-DEC-052, NV-DEC-055, NV-DEC-056, NV-FE-008]
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

**Arti `EndReason` yang dipakai Nurse View.** Ini pemetaan yang akan dibangun di `BE-NV-013`:

| `EndReason` | Yang dilakukan Nurse View pada panggilan aktif | Alasan tercatat |
|---|---|---|
| `Transfer` | Ditutup sistem | "pasien pindah" |
| `EpisodeClosed`, `PatientDeparted` | Ditutup sistem | "pasien pulang" |
| `AdmissionCancelled`, `CorrectedEntry` | **Tidak** ditutup; panggilan tetap menunggu perawat | — |

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

`dokter-rawat-inap`, `keperawatan`, dan `integrasi-billing` **tidak tersentuh** revision 2, sehingga
tidak diberi dokumen.
