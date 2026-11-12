# Rehab Medik — Laporan Kesiapan End-to-End Phase 1

| Field | Nilai |
|---|---|
| `blueprint_id` | `RHB-BP-001` |
| Revision | `1`, bentuk `SINGLE` |
| Status blueprint | `approved` |
| Cakupan audit | Phase 1 (rilis 1) tanpa FEAT-011: gelombang `MVP-0`..`MVP-4` |
| Contract version berlaku | Set kontrak `v1`, `approved` 2026-11-05 |
| Roadmap | `backend-roadmap.md` r2 dan `frontend-roadmap.md` r2 |
| Backend source SHA | `6bb3fba7`, branch `sukmagp`. HEAD tidak bergeser sejak capability map. **Seluruh kode Rehab ada di working tree dan belum di-commit.** |
| Frontend source SHA | `0c06c0930`, branch `sukmagpV2`. Sama: HEAD tidak bergeser, seluruh kode Rehab belum di-commit. |
| Keputusan relevan | RHB-DEC-002 (pengesahan klinis sebelum aktivasi produksi), RHB-DEC-020 (data master go-live), RHB-DEC-021 (handoff billing) |
| Pertanyaan terbuka relevan | RHB-OQ-001, RHB-OQ-016. Ditutup dengan pernyataan owner: RHB-OQ-012, RHB-OQ-015 |
| Sifat audit | **Read-only.** Tidak ada source yang diubah, tidak ada data yang ditulis, tidak ada build atau commit. DB dev hanya dibaca. |
| Tanggal | 9 November 2026 |

---

## 1. Verdict

# `READY_WITH_CONDITIONS`

**Artinya:** fitur Phase 1 sudah lengkap dan sudah terbukti berjalan di lingkungan pengembangan. Belum
boleh dipakai pasien sungguhan karena lima syarat di bagian 5 belum terpenuhi. Tidak satu pun dari
kelima syarat itu bisa diselesaikan dengan menulis kode lagi.

**Contoh jalur yang sudah terbukti di dev:**
1. Dokter poli meminta rehab untuk seorang pasien.
2. Dokter rehab menerima permintaan itu, lalu menulis asesmen medis.
3. Fisioterapis menulis asesmen profesi, menetapkan tujuan, dan mengaktifkan program.
4. Fisioterapis menjalankan sesi. Saat sesi ditandatangani, tindakan bertarif terkirim ke Billing
   tepat satu kali.
5. Dokter rehab menulis reasesmen dan ringkasan akhir, lalu menutup episode.
6. Dokumennya tampil di berkas rekam medis.

### Kenapa bukan `READY`

1. **Pengesah klinis belum ditunjuk.** RHB-DEC-002 mewajibkan pengesahan klinis sebelum aktivasi
   produksi, dan RHB-OQ-001 masih terbuka.
2. **Data master go-live masih kosong.** Pada DB dev, empat disiplin seed
   (`PHYSIOTHERAPY`, `OCCUPATIONAL_THERAPY`, `SPEECH_THERAPY`, `ORTHOTIC_PROSTHETIC`) nonaktif,
   tanpa profesi, dan tanpa tindakan aktif. Belum ada instrumen rehab yang disahkan.
3. **Kode belum masuk repository.** Kode belum di-review, belum di-merge, dan belum di-deploy.
   Migration baru diterapkan di DB dev. `next build` belum pernah dijalankan.

### Kenapa bukan `NOT_READY`

Setiap syarat punya pemilik yang jelas dan jalan penyelesaian yang terbatas. Tidak ada invariant yang
terbuka. Semua aturan berikut terbukti di server dan di layar:
- program tidak aktif tanpa asesmen final;
- catatan final tidak berubah;
- sesi tidak tertagih ganda;
- episode tidak tertutup selama masih ada draft.

---

## 2. Papan skor kesiapan

Skor menilai bukti, bukan jumlah berkas. Build yang sukses bukan bukti kesiapan.

| Dimensi | Bobot | Skor | Bukti | Gap / blocker |
|---|---:|---:|---|---|
| Fondasi (keputusan, blueprint, kontrak) | 10% | 85 | Manifest `approved` r1; kontrak `v1` `approved`; FR tertelusur 31/31 (`roadmap/requirement-traceability.md`) | RHB-OQ-001 terbuka; 3 catatan traceability basi (RHB-GAP-007) |
| Backend | 20% | 90 | 16/16 task BE ✅ dan berlaporan; 13 controller, 95 endpoint; build terakhir 2026-11-09 20:17: 0 error, 426 warning lama | Aksi tujuan masih ditawarkan pada episode tertutup (RHB-GAP-008) |
| Frontend | 20% | 80 | 11/11 task FE ✅ dan berlaporan; 17 halaman; ESLint 0 error, 0 warning; uji browser Playwright tiap task | `next build` belum dijalankan; belum di-commit (RHB-GAP-003) |
| Integrasi dan runtime | 15% | 75 | Handoff billing: 5 tindakan, masing-masing tepat 1 fakta klinis dan 1 charge line, yang bertarif `Synced` (`BE-RHB-013`, `FE-RHB-009`); pembaca berkas RM (`BE-RHB-015`) | Tindakan tanpa tarif diterima Billing dan tidak gagal (RHB-GAP-004); keadaan *Gagal* nyata belum teruji |
| Data dan konfigurasi | 10% | 30 | Seed disiplin ada | Master go-live belum diisi; Akses Role produksi belum dikonfigurasi (RHB-GAP-002) |
| Keamanan dan privasi | 10% | 85 | 13/13 controller memakai `[AccessPermission]`; uji akses ditolak di server dan layar (UAT-03, 15); grep `console`/`localStorage` = 0; log bebas data klinis (UAT-16) | Tidak ada gap pemblokir |
| Verifikasi | 10% | 80 | UAT-01..16 berbukti di laporan task | UAT-11 *Gagal* ditiru lewat stub; `RHB-VAL-052` di-stub; beberapa jalur data nyata belum teruji (RHB-GAP-006) |
| Operasional dan rilis | 5% | 15 | Migration `20261105045613_AddRehabilitationManagement` diterapkan di dev | Belum di-commit, di-merge, atau di-deploy; migration belum diterapkan di DB target; persetujuan formal belum ada (RHB-GAP-003, 005) |
| **Total tertimbang** | 100% | **≈ 74** | | |

**Fitur selesai bukan berarti siap produksi.** Skor backend dan frontend tinggi, tetapi skor data dan
operasional rendah. Modul baru bisa dipakai setelah kedua dimensi itu naik.

---

## 3. Denominator bukti

| Hal | Terpenuhi | Denominator | Catatan |
|---|---:|---:|---|
| Task roadmap ✅ | 27 | 27 | BE-RHB-000..015 dan FE-RHB-001..011 |
| Laporan task tracked | 27 | 27 | `task/report/backend/` dan `task/report/frontend/` |
| Tanda grafik dependency cocok dengan kartu | 27 | 27 | Tidak ada gap dokumentasi delivery |
| Functional requirement tertelusur ke bukti | 31 | 31 | Tiga catatan basi, lihat RHB-GAP-007 |
| Skenario UAT berbukti | 16 | 16 | 2 di antaranya memakai tiruan (UAT-11 *Gagal*, `RHB-VAL-052`) |
| Controller dengan `[AccessPermission]` | 13 | 13 | |
| Butir Definition of Done PRD §19 | 9 | 12 | 2 sebagian, 1 belum (lihat bagian 4) |

---

## 4. Definition of Done (PRD §19)

| Butir | Status | Bukti / alasan |
|---|---|---|
| Satu pasien berjalan dari permintaan sampai episode `Closed` | Ya (dev) | UAT-01, 04, 07, 08, 13 |
| Penolakan klinis hanya oleh pemegang disposisi | Ya | UAT-03 |
| Program tidak aktif tanpa asesmen final | Ya | UAT-06 |
| Catatan final tidak berubah; koreksi lewat addendum | Ya | UAT-05 |
| Sesi terlaksana tertagih tepat sekali; `NotDone` tidak tertagih | Ya, dengan catatan | UAT-08, 10, 11. *Gagal* ditiru, lihat RHB-GAP-004 |
| Episode tidak tertutup selama ada draft | Ya | UAT-14 |
| Akses ditolak di server dan layar | Ya | UAT-03, 15 |
| Log bebas data klinis | Ya | UAT-16 |
| Seluruh layar terjangkau dari menu atau layar induk | Ya | 17 halaman; menu dijaga `requiredPermission`; UAT-15 |
| Data master go-live terisi | **Belum** | RHB-GAP-002 |
| Migration diterapkan, build sukses, tanpa perubahan model tertunda | **Sebagian** | Build 0 error; migration hanya di DB dev |
| Bukti persetujuan RM dan izin Cli/Billing terlampir | **Sebagian** | `evidence/RHB-OQ-015-persetujuan-clinical-billing.md` dan penutupan OQ-012 hanya berisi pernyataan owner, tanpa artefak dari pemilik RM atau Billing |

---

## 5. Syarat sebelum produksi (kondisi `READY_WITH_CONDITIONS`)

Syarat diurutkan dari dampak terbesar.

| ID | Syarat | Dampak bila dilewati | Pemilik | Mitigasi / jalan penyelesaian | Alur hulu |
|---|---|---|---|---|---|
| RHB-GAP-001 | Tunjuk pengesah klinis, lalu sahkan alur dan isi klinis Phase 1 | Melanggar RHB-DEC-002. Aturan klinis dipakai tanpa pihak klinis yang bertanggung jawab. | Owner modul + komite medik | Tutup RHB-OQ-001, lalu catat pengesahan di manifest | `grill-me` Amendment pass |
| RHB-GAP-002 | Isi data master go-live: poli rehab, profesi OT dan TW, tindakan rehab bertarif, `RhbDiscipline` aktif berprofesi, instrumen yang disahkan, dan Akses Role per peran | Modul tidak dapat dipakai: disiplin nonaktif sehingga tidak ada program yang bisa diaktifkan, dan tindakan tanpa tarif masuk invoice bernilai nol | Owner modul + Billing + admin sistem | Isi lewat layar master data (`FE-RHB-002`) dan Akses Role, lalu cek ulang dengan kueri READ ONLY | RHB-OQ-016, RHB-DEC-020 (konfigurasi, bukan task builder) |
| RHB-GAP-003 | Commit, review, merge, lalu deploy BE dan FE; terapkan migration di DB target; jalankan `next build` | Tidak ada yang bisa dirilis. Kode hanya ada di working tree satu mesin, sehingga ada risiko hilang. | Owner repository | Review diff, lalu commit lewat alur rilis biasa. `compose-release-manifest` untuk usulan versi BE. | Alur rilis owner |
| RHB-GAP-004 | Sepakati dengan pemilik Billing: tindakan tanpa tarif diterima sebagai `Emitted` dan ditandai `TARIFF_NOT_FOUND` saat sinkron invoice, bukan ditolak | Petugas melihat *Terkirim* walau invoice tidak memuat nilai, sehingga ada risiko kebocoran pendapatan | Owner Billing | Putuskan: diterima apa adanya (cukup dicatat), atau handoff ditandai *Gagal* bila tarif tidak ada (task BE terbatas di Rehab) | `grill-me` Amendment pass |
| RHB-GAP-005 | Lampirkan artefak persetujuan formal dari pemilik RM (OQ-012) dan Cli/Billing (OQ-015) | Butir DoD §19 tidak terpenuhi; jejak audit persetujuan lemah | Owner modul | Tambahkan notulen atau persetujuan tertulis ke `evidence/` | Dokumentasi owner |

---

## 6. Temuan minor (tidak memblokir)

| ID | Temuan | Rekomendasi | Arahan |
|---|---|---|---|
| RHB-GAP-006 | Jalur data nyata yang belum teruji: renderer instrumen dengan instrumen yang disahkan, `RHB-VAL-052` (pasien sudah pulang), jalur UI persetujuan ditolak, dan *Gagal* billing nyata | Uji ulang setelah RHB-GAP-002 terisi, sebagai bagian UAT go-live | UAT owner |
| RHB-GAP-007 | Catatan basi di `requirement-traceability.md`: baris "Episode (baca)", "FR-030.. (tanpa billing)", dan FR-060 "Selesai (BE)" | Rapikan teksnya; status task tidak berubah | `plan-module-delivery` (hanya teks) |
| RHB-GAP-008 | Backend masih menawarkan aksi tujuan pada episode `Closed`. FE sudah menyembunyikannya, tetapi server tidak menolak secara eksplisit. | Satu task BE terbatas: tolak aksi tujuan bila episode `Closed` | `build-module-backend` (task baru) |
| RHB-GAP-009 | Kolom "item tagihan" tidak tampil di layar sesi karena penanda `IsBillingGenerated` jalur lama tidak diperbarui | Bila status invoice perlu tampil di layar rehab, diperlukan endpoint baca dari Billing (di luar Rehab) | Keputusan owner Billing |
| RHB-GAP-010 | Residu data uji di DB dev: draft tertahan di EP1, 2 sesi `InProgress`, draft `NotDone`, dan program v2 `Stopped` di EP6, serta sesi `000015` `InProgress` di EP8 | Bersihkan lewat API atau abaikan; ini hanya DB dev | Owner dev |

---

## 7. Hal yang sengaja tidak dinilai

- **Automated test backend.** Mengikuti `rules/backend/TEST_POLICY.md`, ketiadaannya bukan gap dan
  bukan syarat.
- **Automated test frontend.** Repository tidak memakai Jest. Bukti perilaku berasal dari uji browser
  dan jaringan di setiap laporan task.
- **POST-MVP.** FEAT-004..006, 011, 014, 019..023, 030, 031, 033, serta RHB-OQ-005, 013, dan 014
  berada di luar rilis 1.

---

## 8. Task berikutnya

1. **Owner:** tutup RHB-OQ-001 dengan menunjuk pengesah klinis (RHB-GAP-001).
2. **Owner Billing:** putuskan perlakuan tindakan tanpa tarif (RHB-GAP-004).
3. **Admin + Billing:** isi data master go-live dan Akses Role (RHB-GAP-002), lalu ulangi uji jalur
   data nyata (RHB-GAP-006).
4. **Owner repository:** review, commit, dan merge BE serta FE, jalankan `next build`, lalu terapkan
   migration di DB target (RHB-GAP-003).
5. **Opsional:** task BE kecil untuk RHB-GAP-008, dan perapian teks traceability untuk RHB-GAP-007.

Setelah butir 1–4 selesai dan buktinya terlampir, audit ulang dapat menaikkan verdict menjadi `READY`.
