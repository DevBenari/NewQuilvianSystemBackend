# Laporan Perubahan Frontend — `FIX-EPS-041-FE`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FIX-EPS-041-FE` (task payung bagian frontend `PLAN-REPAIR-EPS-041`; preseden `FIX-EPS-040-FE`) |
| Judul | Perbaikan temuan uji Admisi Pasien Lama, Bayi Baru Lahir, dan Transfer IGD & Poliklinik — bagian frontend `FIX-EPS-041-02`, `-03`, `-06`, `-07`, `-08`, `-09`, `-10`, `-12`, `-15`, `-17`, `-18` |
| Slice | Admisi Rawat Inap (`FE-INP-03`) — jalur Transfer, Pasien Lama, Bayi |
| Roadmap | **Tidak ada entri roadmap.** Register status dipegang [PLAN-REPAIR-EPS-041](../../../docs/plan-repair/plan-repair-041-temuan-uji-admisi-lama-bayi-transfer.md) bagian 1 |
| Trace | [ISSUE-EPS-041](../../../docs/issue/issue-041-temuan-uji-admisi-lama-bayi-transfer.md); K-01–K-06 disetujui pemilik 26-10-2026; skema 3.1–3.3 rencana; amandemen 041 di `05-skema-tampilan.md` |
| Contract version | Respons antrean diperluas `FIX-EPS-041-01` ([kontrak 006 revisi 041](../../../contracts/admission-transfer-repair-006.md)); kandidat ibu `admittedBabyCount`/`pendingBabyCount`/`isPendingAdmission` ([kontrak 022 revisi 041](../../../contracts/newborn-admission-repair-022.md)); bed-board `reservationReservedAt`. Disetujui melalui rencana |
| Wewenang UI | Skema sebelum → sesudah PLAN-REPAIR-EPS-041 bagian 3 yang disetujui pemilik; di luar skema itu hanya teks peringatan yang dibutuhkan temuan runtime (VR-01) |
| Dependency | [`FIX-EPS-041-BE`](../backend/FIX-EPS-041-BE.md) — source di `1872b399`, build dan runtime terverifikasi sesi ini |
| Klasifikasi | `HEAVY` — 24 berkas source, 3 jalur admisi, hook bersama (denah, kelayakan, dokter) |
| Task mode | `FRONTEND` (source frontend); laporan dan dokumen blueprint di repository backend atas instruksi pemilik |
| Target tulis | `QuilvianSystemFrontendDev/src/**`, `tests/unit/**`, `test-with-agy/**`; laporan ini |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `be9c2f129` (`HamzahV2`, memuat FE-RWI-229) — perubahan belum di-commit |
| Commit backend yang dijadikan rujukan | `ee56c659` (`MHamzah`, memuat `1872b399`) |
| Tanggal | 2026-10-27 |
| Status | Source selesai untuk 11 perbaikan; lint/unit/build dijalankan; uji browser menyeluruh dijalankan. 6 perbaikan terbukti penuh, 5 sebagian (lihat bagian 7) |

---

## 1. Keadaan yang ditemukan di awal

- Jalur transfer memanggil `available-beds` tanpa episode dan tanpa pasien, sehingga denah menawarkan bangsal biasa untuk
  pasien isolasi (`use-inpatient-bed-board.jsx` hanya mengisi konteks dari bayi).
- Langkah Antrean & Verifikasi Transfer (hasil FE-RWI-229, `transfer-referral`) tidak memeriksa kelayakan pasien;
  lencana tertulis mati "Siap Melanjutkan".
- Kartu rujukan memakai dokter yang memutuskan sebagai "DPJP dituju"; kebutuhan IGD, diagnosis, dan kelas Poli tidak tampil.
- Sesudah `admit`, konsulen dan ketergantungan tidak disimpan; pembacaan ulang episode menimpa isian langkah Dokter.
- Satu state `error` dipakai untuk galat muat dan galat simpan; galat simpan mengganti seluruh langkah dengan judul
  "Gagal Memuat Permintaan Rawat Inap".
- `normalizeEpisodeDetail` membuang `depositPaymentOrder`, sehingga kartu order deposit Konfirmasi yang sudah ada tidak
  pernah tampil.
- Bayi ber-RM tanpa episode tidak dapat dilanjutkan; centang kembar wajib untuk bayi yang sama.

---

## 2. Proses bisnis dari sisi pengguna

1. **Petugas admisi membuka permintaan transfer.** Layar membaca sumber, lalu langsung memeriksa kelayakan pasien.
   - Pasien sedang dirawat → alert merah, badge "Sedang Dirawat", tombol "Pindah Tempat Tidur"/"Lihat Detail Episode";
     "Lanjut ke Pembayaran" nonaktif (judul tombol menyebut sebabnya). Contoh: surat Poli Ny. Anisa Lestari (VIP 302).
   - Ada admisi Draft lain → peringatan kuning dan "Lanjutkan Admisi RI-…"; Lanjut tetap dapat dipakai.
   - Kunjungan Poli/IGD asal yang masih aktif **tidak** menahan (server `admit` pun tidak menolaknya).
   - Gagal diperiksa → alert dengan "Periksa Ulang".
2. **Petugas membaca rujukan.** DPJP usulan dan dokter yang memutuskan terpisah; chip kebutuhan (Isolasi, Perawatan
   Intensif, Oksigen, Monitoring Khusus); Kelas Diminta dan Diagnosa untuk Poli; "Alasan Rawat Inap".
3. **Langkah Dokter jalur transfer terisi awal**: DPJP usulan (atau dokter yang memutuskan), sakelar isolasi dan
   keterangannya bila IGD meminta isolasi. Isian yang diubah petugas tetap menang.
4. **Pilih Bed.** Denah hanya mengaktifkan bed yang lolos aturan pasien itu; mengubah sakelar isolasi membaca ulang denah.
5. **Ya, Pesan.** `admit` tersimpan → layar menyimpan ketergantungan dan mencoba dokter pendukung. Gagal karena jaringan →
   peringatan + "Simpan Ulang"; ditolak aturan server → kalimat server + arahan ke Detail Episode. `admit` ditolak →
   judul "Admisi Belum Tersimpan" di atas langkah dengan "Baca Ulang Permintaan".
6. **Konfirmasi** menampilkan status bed "Dipesan", tingkat ketergantungan, order deposit, dan — pada transfer — kalimat
   bahwa pembatalan menutup permintaan IGD/Poli. Dialog Batalkan Admisi menyebut nomor kunjungan sumber.
7. **Cetak Persetujuan**: "Selesai" langsung ke Daftar Kerja Episode bila persetujuan tersimpan.
8. **Admisi bayi**: bayi "Admisi belum selesai" dilanjutkan dengan "Lanjutkan admisi bayi ini"; bayi baru wajib centang
   "Ini bayi yang berbeda" bila kasus sudah punya bayi lain.
9. **Deposit**: nominal di atas batas presisi ditahan di langkah Deposit; koma memunculkan petunjuk.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md` frontend; `rules/frontend/*` (arsitektur, gerbang komponen, checklist, test policy, template);
PLAN-REPAIR-EPS-041, ISSUE-EPS-041, tiga laporan uji 26-10; view admisi, langkah transfer (split/review), Konfirmasi,
Deposit, Cetak, langkah bayi; hook dokter, bed, papan, denah, kelayakan, transfer, bayi, deposit, exit guard; utilitas
bed/episode/kelayakan/bayi/transfer; backend: DTO dan proyeksi antrean, `InpBedOccupancyService` (`available-beds`),
`InpEpisodeService.AdmissionEligibility`, `OpenAdmissionAsync`, `OutpatientEncounterRules`, kandidat ibu,
`InpEpisodeService.Assignments` (aturan dokter pendukung).

### 3.2 Berkas yang berubah

Semua jalur relatif terhadap `QuilvianSystemFrontendDev/`.

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/health-services/inpatient-management/inpatient-admission-view.jsx` | FIX-06: kelayakan aktif pada `transfer-referral` begitu sumber terbaca, `ACTIVE_ENCOUNTER_EXISTS` diabaikan, komentar diperbarui. FIX-03: `patientContext` transfer ke hook bed. FIX-08: `completeTransferAdmission` sesudah `adoptTransferEpisode`; peringatan + "Simpan Ulang" di Booking/Konfirmasi. FIX-09: alert "Admisi Belum Tersimpan". FIX-07: peringatan sumber di dialog batal; fallback nama pasien. FIX-17: `onCompleteAdmission` |
| `src/components/view/health-services/inpatient-management/admission-transfers/inpatient-admission-transfer-review-step.jsx` | FIX-02: DPJP usulan/Diputuskan oleh, chip Kebutuhan, Kelas Diminta, Diagnosa, "Alasan Rawat Inap". FIX-06: lencana dan alert kelayakan, Lanjut lewat `eligibility.proceed`. FIX-09: pembersih titik sisa TTV |
| `src/components/view/health-services/inpatient-management/admission-transfers/inpatient-admission-transfer-split-step.jsx` | Meneruskan `eligibility` (di atas FE-RWI-229) |
| `src/components/view/health-services/inpatient-management/inpatient-admission-confirmation-step.jsx` | FIX-09: status bed "Dipesan"/label Indonesia. FIX-07: kalimat kunci transfer. FIX-08/18: baris Tingkat Ketergantungan |
| `src/components/view/health-services/inpatient-management/inpatient-admission-deposit-step.jsx` | FIX-15: pesan batas, petunjuk koma, Lanjut menahan bila nominal tidak sah |
| `src/components/view/health-services/inpatient-management/inpatient-admission-print-steps.jsx` | FIX-17: `onComplete` untuk "Selesai" bila persetujuan tersimpan; komentar "dikunci" |
| `src/components/view/health-services/inpatient-management/inpatient-newborn-admission-steps.jsx` | FIX-10: baris bayi + "Lanjutkan admisi bayi ini", info RM ganda, centang "Ini bayi yang berbeda". FIX-12: batas picker, label kelas, format waktu lahir |
| `src/components/features/health-services/inpatient-management/floor-plan/floor-plan-summary.jsx` | FIX-17: prop opsional `loading` → "Memuat denah…" |
| `src/components/features/health-services/inpatient-management/floor-plan/floor-plan-view.jsx` | Meneruskan prop opsional `loading` (bawaan `false`) |
| `src/components/features/health-services/inpatient-management/floor-plan/floor-plan-selection.jsx` | Mengirim `loading` selama data lantai belum terbaca |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-doctor.jsx` | FIX-08: `saveSupportingDoctors`/`saveCareDependency` bersama; `completeTransferAdmission`, `retryTransferSupplements`, peringatan dapat-ulang vs ditolak aturan. FIX-02: isian awal DPJP usulan + isolasi |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-transfer.js` | FIX-09: `submitError` terpisah dari galat muat |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-eligibility.jsx` | Opsi `ignoredBlockingCodes` (bawaan kosong) |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-bed-board.jsx` | FIX-03: opsi `patientContext` di samping `newbornContext` |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-bed.jsx` | FIX-03: teruskan konteks transfer; FIX-12: lantai awal neonatal; FIX-17: `reservationReservedAt` papan denah |
| `src/lib/hooks/health-services/inpatient-management/use-floor-plan.js` | FIX-12: opsi `initialFloorCode` tanpa menimpa lantai terakhir; FIX-17: `reservationReservedAt` |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-deposit.jsx` | FIX-15: `amountError` (`Number.isSafeInteger`), `decimalHint`, format digit asli di atas batas |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-newborn-admission.js` | FIX-10: `pendingBabies`, `continuePendingBaby`, pesan gerbang kembar |
| `src/lib/constants/health-services/inpatient-management/inpatient-admission-flow-constants.jsx` | Teks deposit FIX-15; catatan cetak FIX-17 |
| `src/lib/constants/health-services/inpatient-management/floor-plan/floors.js` | `NEWBORN_FLOOR_NUMBER` diturunkan dari denah (kini Lantai 2) |
| `src/utils/health-services/inpatient-management/inpatient-admission-eligibility-utils.js` | `omitInpatientAdmissionEligibilityBlockers` |
| `src/utils/health-services/inpatient-management/inpatient-newborn-admission-utils.js` | `buildNewbornBirthDateLimits`, `formatNewbornBirthDateTime`, `formatPatientClassLabel`, `describeRegisteredBaby` |
| `src/utils/health-services/inpatient-management/inpatient-bed-utils.jsx` | `reservationReservedAt` di normalizer papan dan pemulihan pemesanan |
| `src/utils/health-services/inpatient-management/inpatient-episode-utils.jsx` | FIX-18: normalisasi `depositPaymentOrder` |
| `tests/unit/inpatient-admission-repair-041.test.mjs` (baru) | 6 test fungsi murni FIX-06/-10/-12/-17/-18 |
| `tests/unit/floor-plan-summary.test.mjs`, `inpatient-newborn-admission.test.mjs`, `care-dependency-panel.test.mjs` | Asersi teks source disesuaikan dengan salinan/bentuk yang disetujui rencana |

### 3.3 Kepatuhan arsitektur frontend

Alur `view → hook → service → InstanceAxios` tetap; tidak ada Axios di view, tidak ada state Redux baru. Logika baru
murni ditaruh di `utils/` dan diuji; konstanta teks di `constants/`. Pekerjaan pemilik FE-RWI-229 (langkah
`transfer-referral` 9 langkah) dipertahankan; perbaikan ditambahkan di atasnya.

**Gerbang base component.** `UI GATE: 11 elemen — REUSE 6, EXTEND 1, COMPOSE 4, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat base | Status | Keputusan |
| --- | --- | --- | --- |
| Alert kelayakan transfer | `InpatientAdmissionEligibilityNotice` | REUSE | Dipakai apa adanya |
| Lencana identitas | `BasePatientVerificationCard.badgeText` | REUSE | `resolveInpatientAdmissionEligibilityBadge` |
| Tombol Lanjut memeriksa | `BaseButton loading/loadingLabel` | REUSE | Sama dengan Pasien Lama |
| Chip Kebutuhan, DPJP usulan, Kelas, Diagnosa | `StatusBadge` + kelas kartu existing | COMPOSE | Di kartu rujukan |
| Baris Konfirmasi | kelas `infoRow`, `ConfirmationDepositOrderSummary` existing | REUSE | Tambah baris + normalisasi |
| Peringatan "Simpan Ulang" | `InformationAlert` + `BaseButton` | COMPOSE | Di view |
| Peringatan dialog batal | `ConfirmModal` + `InformationAlert` | COMPOSE | Anak dialog |
| Baris bayi + Lanjutkan | `BaseDetailCard` + `StatusBadge` + `BaseButton` | COMPOSE | Value node |
| Centang/info bayi | `BaseCheckboxField`, `InformationAlert` | REUSE | Teks baru |
| Batas tanggal lahir | `BaseDateField field.min/max` | REUSE | Kirim batas |
| Ringkasan denah memuat | `FloorPlanSummary` (+ `FloorPlanView`) | EXTEND | Prop opsional `loading`, bawaan `false` (opsi A yang direkomendasikan; pemakai lain tidak berubah) |

**Grep anti-regresi** (baris tambahan pada JSX yang diubah): `<button` mentah 0, `<table` 0, utilitas tipografi
Bootstrap 0, warna literal 0, inline style baru 0, CSS diubah 0, `!important` 0.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Lencana "Memeriksa kelayakan..." dan tombol berlabel sama; denah "Memuat denah…" alih-alih "Belum ada kamar" |
| Kosong | Baris Kebutuhan/Kelas/Diagnosa/DPJP usulan disembunyikan bila kosong; ibu tanpa bayi tidak melihat daftar/centang |
| Gagal | Kelayakan: alert + "Periksa Ulang". Simpan transfer: "Admisi Belum Tersimpan" + "Baca Ulang Permintaan". Konsulen/ketergantungan: peringatan + "Simpan Ulang" bila dapat diulang |
| Ditolak aturan | Pasien sedang dirawat: alert merah + tindak lanjut; konsulen pada Draft: kalimat server + arahan |
| Tanpa hak akses | Tidak berubah: antrean transfer menunggu keputusan `InpatientAdmissionTransfer : Read`; kelayakan `forbidden` memakai kalimat existing |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Inpatient Management / Inpatient Admission Transfer

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/inpatient-management/admission-transfers/{id}?sourceType=` | Kolom baru FIX-01 untuk kartu rujukan dan isian awal | `InpatientAdmissionTransfer : Read` |
| `POST` | `/api/v1/health-services/inpatient-management/admission-transfers/admit` | Admisi; galat dipisah dari galat muat | `InpatientEpisode : Create` |

#### Health Services / Inpatient Management / Inpatient Episode

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/inpatient-management/episodes/admission-eligibility?patientId=` | Kelayakan langkah transfer | `InpatientEpisode : Read` |
| `GET` | `/api/v1/health-services/inpatient-management/episodes/newborn-mother-candidates` | `pendingBabyCount`, `registeredBabies[].isPendingAdmission` | `InpatientEpisode : Read` |
| `POST` | `/api/v1/health-services/inpatient-management/episodes/{id}/doctor-assignments/supporting` | Dokter pendukung sesudah `admit` | sesuai controller existing |
| `PATCH` | `/api/v1/health-services/inpatient-management/episodes/{id}/care-dependency` | Ketergantungan sesudah `admit` | sesuai controller existing |

#### Health Services / Inpatient Management / Bed Occupancy

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/inpatient-management/bed-occupancies/available-beds` | Kini dengan `patientId`, `requiresIsolation`, `includeIneligible` pada transfer | sesuai controller existing |
| `GET` | `/api/v1/health-services/inpatient-management/bed-occupancies/bed-board` | `reservationReservedAt` untuk "Dipesan Pada" sesudah F5 | sesuai controller existing |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint <24 berkas src + test baru>` | 0 error, 13 warning; `HEAD` pada berkas yang sama juga 13 warning | `PASS` (warning `EXISTING`) | keluaran perintah |
| `npm run lint:errors` (seluruh repo) | 4 error, seluruhnya di `test-with-agy/chrome-profile-*/…/craw_*.js` (ekstensi Chrome pada profil uji yang di-ignore Git); 0 di `src/` | `EXISTING / ENVIRONMENT ISSUE` | keluaran perintah |
| `npm run test:unit` | 3851 test: 3810 lulus, 41 gagal — **himpunan kegagalan identik dengan `HEAD`** (snapshot `git archive HEAD`); 0 kegagalan baru | `PASS` (41 `EXISTING`) | perbandingan nama test |
| Test terarah (5 berkas, termasuk test baru) | 50/50 lulus | `PASS` | keluaran perintah |
| `npm run build` (final) | `Compiled successfully in 60s`, `prepare-standalone` selesai, exit 0 | `PASS` | keluaran perintah |
| Live browser (Chrome headed, 8 skenario bagian 8) | 7 lulus, 1 lulus dengan catatan (konsulen ditolak aturan server) | `PASS` / temuan | [laporan uji](../../../testing/live-testing/laporan-live-testing-verifikasi-perbaikan-041-admisi.md) |
| Jaringan selama uji | 511 respons: 508 2xx, 3 × 422 yang diharapkan, 0 × 5xx, 0 galat JavaScript | `PASS` | `network.log` |

Uji manual: `PASS` (dijalankan agent di browser; rincian di laporan uji).

`AUTOMATED TEST: npm run test:unit — FAIL (41 kegagalan lama, identik dengan HEAD; 0 baru)`. Test baru ditulis karena
perubahannya berbentuk fungsi murni (opsional menurut test policy).

**Tidak dijalankan:** `test:e2e`/`test:uat` (tidak diminta); aktivasi bayi sampai Dokumen Bayi (dibatalkan agar data uji
bersih); skenario ibu tanpa bayi (tidak ada datanya).

---

## 7. Acceptance criteria dan Definition of Done

| Perbaikan | Kriteria | Status | Bukti |
| --- | --- | --- | --- |
| FIX-02 | 1 Chip Isolasi + sakelar tercentang + keterangan | Terpenuhi | uji VR-P2-01 |
| | 2 DPJP usulan tampil dan terpilih | Terpenuhi | dr. Rendy Pangalila |
| | 3 Kelas Diminta/Diagnosa Poli; tersembunyi untuk IGD kosong | Terpenuhi | `AGY1027-14`, `AGY04` |
| | 4 "Catatan Disposisi" tidak "." | Terpenuhi | VR-P4-02 |
| FIX-03 | 1 329/307 nonaktif dengan alasan isolasi | **Sebagian** — nonaktif ✓, alasan `ISOLATION_REQUIRED` ada di jawaban server dan data bed, tetapi teksnya tidak terlihat karena kamar yang seluruh bed-nya tidak layak tidak dapat dibuka (perilaku denah yang sama dengan jalur bayi) | VR-P1-01 |
| | 2 ODC nonaktif untuk pasien non-isolasi | Terpenuhi | VR-P1-02 |
| | 3 Papan Tempat Tidur dan admisi bayi tidak berubah | Terpenuhi | Papan termuat, bayi Perinatologi |
| FIX-06 | 1 Pasien sedang dirawat diblokir, 0 POST | Terpenuhi | VR-P1-03 |
| | 2 Pasien layak "Siap Melanjutkan" dan lanjut | Terpenuhi | VR-P0-01 |
| | 3 Admisi Draft lain → peringatan, Lanjut aktif | Terpenuhi | VR-P1-04 |
| FIX-07 | 1 Konfirmasi transfer tanpa "dibuka ulang"; Pasien Lama tetap | Terpenuhi | VR-P1-05, s9-k |
| | 2 Dialog menyebut sumber dan tidak kembali ke antrean | Terpenuhi | VR-P1-05 |
| FIX-08 | 1 Dua `InpDoctorAssignment` + ketergantungan Low | **Sebagian** — ketergantungan ✓ (level 2, 1 riwayat); konsulen ⛔ ditolak aturan server episode Draft | VR-P1-06/07 |
| | 2 High Dependency transfer → level 1 | Terpenuhi | DB |
| | 3 Pasien Lama tetap menyimpan High Dependency | Terpenuhi | `RI-261027105512-EB0A69` level 1 |
| | 4 Konfirmasi menampilkan konsulen | **Belum** — konsulen tidak tersimpan (VR-01) | — |
| FIX-09 (FE) | 1 "Dipesan" | Terpenuhi | VR-P4-03 |
| | 2 Judul "Admisi Belum Tersimpan" vs "Gagal Memuat" | Terpenuhi | VR-P2-06 |
| | 3 TTV "…95%." → tanpa "." | Terpenuhi | VR-P4-02 |
| | 5 Buka ulang URL transfer berpersetujuan → Cetak | **Belum dibuktikan** (masih dugaan issue; tidak ada transfer yang selesai sampai persetujuan) | — |
| FIX-10 (FE) | 1 "Admisi belum selesai" + tombol | Terpenuhi | VR-P1-08 |
| | 2 Tanpa RM baru, bayi tetap 1 | Terpenuhi sampai pemesanan boks; aktivasi tidak dijalankan | DB |
| | 3 Bayi baru tanpa centang ditolak, pesan menyebut kedua pilihan | Terpenuhi | VR-P2-02 |
| | 4 Ibu tanpa bayi tidak melihat daftar/centang | **Belum teramati** (`NOT FEASIBLE`: tidak ada data); logika `hasExistingBabies` tidak berubah | — |
| FIX-12 | 1 Picker tanggal terbatas | Terpenuhi | VR-P2-03 |
| | 2 Tanpa "Kelas KELAS" | Terpenuhi | VR-P4-08 |
| | 3 Waktu lahir "… WIB" | Terpenuhi | VR-P4-08 |
| | 4 Denah neonatal di lantai ruang bayi | Terpenuhi | VR-P4-09 |
| FIX-15 (FE) | 1 Dua puluh angka 9 ditahan dengan pesan | Terpenuhi | VR-P2-05 |
| | 2 Petunjuk koma | Terpenuhi | VR-P2-04 |
| | 4 Rp 5.000.000 tetap menghasilkan order | Terpenuhi | DPO-20261027-00001 |
| FIX-17 (FE) | 1 Selesai tanpa dialog | Terpenuhi | VR-P4-07 |
| | 2 "Memuat denah…" | Terpenuhi | VR-P4-05 |
| | 3 "Dipesan Pada" sesudah F5 | Terpenuhi | VR-P4-06 |
| | 5 Teks lembar cetak | Terpenuhi | "Data admisi sudah diperiksa pada langkah Konfirmasi." |
| FIX-18 | Order deposit + ketergantungan baca-saja | Terpenuhi | VR-P4-04 |

**Ringkasan:** terbukti penuh FIX-02, -06, -07, -12, -17 (FE), -18; sebagian FIX-03 (teks alasan), -08 (konsulen ⛔
aturan server), -09 (butir 5), -10 (butir 4), -15 (butir 3 sisi server — lihat laporan BE).

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 13 warning ESLint pada berkas yang disentuh, sama persis dengan `HEAD` (`react-hooks/*` lama) |
| Masalah yang diketahui | VR-01 konsulen pada episode Draft ditolak server (perlu keputusan K-07 usulan); teks alasan bed pada kamar yang seluruhnya tidak layak tidak terlihat; FIX-09 butir 5 belum dibuktikan |
| Dependency backend | `FIX-EPS-041-BE` terverifikasi; migration `20261022130000_MakeTransferHandoverPlacementsNullable` diterapkan di DB Hamzah 27-10-2026 sesudah uji (VR-02, lingkungan; [laporan BE](../backend/FIX-EPS-041-BE.md) bagian 8) |
| Selisih dengan dokumen | Amandemen 010 menyebut tombol Lanjut "tidak dimatikan"; source Pasien Lama saat ini menonaktifkannya bila ditolak, dan langkah transfer mengikuti source. Kriteria FIX-09 butir 1 meminta "Dipesan" sementara `getBedReservationStatusLabel` bernilai "Aktif" untuk pemesanan aktif; layar memakai "Dipesan" untuk pemesanan aktif dan helper untuk status lain |
| Perubahan sampingan | Dua berkas yang ditulis ulang lewat skrip sempat berubah ke LF; dikembalikan ke CRLF seperti checkout pemilik |
| Interupsi | Satu tab peramban tidak menjawab sesudah Fast Refresh; uji dilanjutkan di tab baru, tidak terulang |
| Lingkungan uji | Backend dijalankan dari folder build sementara lalu dihentikan. `next dev` yang dinyalakan agent (PID 3812, port 3000) **masih berjalan** karena penghentiannya ditolak pengaman otomatis — pemilik dapat menutupnya |
| Status Git | Frontend: 27 berkas `M` + 1 berkas test baru (`??`), belum di-commit. Backend: dokumen blueprint rawat-inap (laporan, plan, kontrak, skema) — selain itu ada perubahan modul Human Resource yang bukan bagian task ini |
| Langkah berikutnya | Pemilik memutuskan K-07 (konsulen saat admisi), lalu task lanjutan menutup FIX-08 butir 1/4 |
