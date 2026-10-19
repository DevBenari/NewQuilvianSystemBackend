# Laporan Perubahan Frontend — `FE-RWI-264`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-264` |
| Judul | Kembalikan Alur Pembersihan pada Denah Mode Pilih Bed |
| Slice | Repair 027 — integrasi pembersihan pada mode Pilih FE-DNH-02 |
| Roadmap | `roadmap/frontend-roadmap-repair-027.md` |
| Trace | `ISS-EPS-027-01`, `ISS-EPS-027-T1`, `FIX-EPS-027-01`, `FIX-EPS-027-02`, `K-027-01` |
| Contract version | `0.13.0` + amandemen `PLAN-REPAIR-EPS-027`, disetujui pemilik 17 Oktober 2026 |
| Wewenang UI | Menambahkan pintu masuk `Pembersihan` pada mode Pilih berdasarkan `AvailableActions ∩ permission`; tidak mengubah kelayakan pemilihan, status, atau permission |
| Dependency | `FE-RWI-249` ✅ dan `FE-RWI-263` ✅ |
| Klasifikasi | `MEDIUM`: shared popup dan selection controller berubah; API, service, backend, dan CSS tidak berubah |
| Task mode | `CROSS-REPO`: source frontend serta dokumen/report blueprint backend |
| Target tulis | `QuilvianSystemFrontendDev` pada dua komponen denah; blueprint Repair 027 dan laporan task pada `NewQuilvianSystemBackend` |
| Model | Codex / GPT-5 |
| Commit frontend saat dikerjakan | `15ae73984ad26a408e3038566d7e1d1533fe10c3` (`HamzahV2`) |
| Commit backend yang dijadikan rujukan | `c2bc563ed9332f08fa95b9fe35786fea7a647279` (`MHamzah`) |
| Tanggal | 17 Oktober 2026 |
| Status | **🟡 SEBAGIAN — source dan production build selesai; UAT pemilik belum dilaporkan** |

---

## 1. Keadaan yang ditemukan di awal

`FloorPlanSelection` membuka `FloorPlanRoomPopup` dengan `mode="selection"`. Pada cabang ini,
`BedRow` hanya merender `Pilih bed ini` untuk bed eligible atau alasan `Belum dapat dipilih` untuk
bed ineligible. Tombol `Pembersihan` berada pada cabang mode Lihat yang saling meniadakan, walaupun
respons bed sudah membawa `AvailableActions` dan komponen `InpatientBedCleaningDetail` masih ada.

Mode Pilih juga belum meneruskan `state.refresh` ke pop-up. Akibatnya, bila aksi pembersihan
sekadar ditampilkan tanpa wiring tambahan, denah dapat mempertahankan status dan eligibility lama.
Source terkait sedang memuat perubahan lokal dari repair lain; seluruh perubahan tersebut
dipertahankan dan tidak di-reset.

---

## 2. Proses bisnis dari sisi pengguna

1. Petugas membuka langkah Pilih Bed lalu memilih kamar pada denah.
2. Bed belum siap tetap menampilkan alasan mengapa ia belum dapat dipilih.
3. Bila server mengirim aksi pembersihan dan permission efektif petugas mengizinkannya, tombol
   `Pembersihan` tampil pada baris yang sama.
4. Tombol membuka detail pembersihan existing. Detail membaca ulang monitoring serta riwayat
   sebelum menawarkan `Mulai Pembersihan`, `Selesai Pembersihan`, atau `Verifikasi Kesiapan`.
5. Setelah tindakan berhasil, callback menunggu refresh state denah. Pada jalur admisi transfer,
   refresh efektif tetap memuat denah dan overlay kelayakan secara bersamaan.
6. Bila bed terpilih tidak lagi eligible pada snapshot baru, pilihan dilepas. Bed pembersihan tetap
   tidak dapat dipilih sampai server menyatakan `Available` dan `Eligibility.IsEligible = true`.

Jalur tidak normal: permission `denied/unknown` menyembunyikan tombol; aksi server yang kosong
menyembunyikan tombol; 403/409/422 dan konflik version/cycle tetap ditangani hook pembersihan
existing tanpa perubahan status optimistis di komponen denah.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `src/components/features/health-services/inpatient-management/floor-plan/floor-plan-room-popup.jsx`
- `src/components/features/health-services/inpatient-management/floor-plan/floor-plan-selection.jsx`
- `src/lib/hooks/health-services/inpatient-management/use-inpatient-bed-cleaning.js`
- `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-bed.jsx`
- `src/lib/hooks/auth/use-permission.jsx`
- controller Bed Occupancy dan Bed Management backend
- issue, plan repair, arsitektur frontend, skema tampilan, flowchart, API contract, dan permission matrix sub-modul

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `floor-plan-room-popup.jsx` | Memakai `useEffectivePermissions`; menghitung aksi cleaning dari `Read` dan action permission yang `allowed`; merender tombol pada mode Pilih; mengembalikan promise callback refresh |
| `floor-plan-selection.jsx` | Meneruskan `state.refresh` dan membersihkan pilihan bila snapshot baru tidak lagi eligible |
| `03-frontend-architecture.md` | Mengunci pembersihan bersyarat pada mode Pilih tanpa mengubah aturan pemilihan |
| `05-skema-tampilan.md` | Menambahkan amandemen Repair 027 beserta contoh `325-A` |
| `flowcharts/15-denah-pilih-bed.md` | Menambahkan cabang pembersihan → refresh → evaluasi ulang |
| artefak Repair 027 | Mencatat approval, delivery lock, roadmap satu task, traceability, status, dan bukti |

Tidak ada perubahan source backend, database, migration, konfigurasi, service API frontend, atau
stylesheet untuk task ini.

### 3.3 Kepatuhan arsitektur frontend

Alur tetap `view/hook state → FloorPlanSelection → FloorPlanRoomPopup → komponen detail existing`.
Komponen tidak memanggil Axios secara langsung. Mutasi dan pembacaan terbaru tetap dimiliki
`useInpatientBedCleaning` serta service existing. Alias `@/` dipakai untuk import permission hook.

### 3.4 Keputusan reuse / create-new

UI GATE: 3 elemen — REUSE 2, COMPOSE 1, EXTEND 0, WRAP 0, NEW 0.

| Kebutuhan UI | Kandidat | Bukti | Status | Keputusan |
| --- | --- | --- | --- | --- |
| Tombol pintu masuk | `BaseButton` | Sudah dipakai oleh seluruh aksi bed pada pop-up | `REUSE` | Pakai ukuran `sm`, varian `secondary`, ikon `SprayCan` existing |
| Panel dan tindakan pembersihan | `InpatientBedCleaningDetail` | Sudah memuat snapshot, attempts, permission, konfirmasi, mutation, dan error | `REUSE` | Buka melalui state `cleaningTarget` existing |
| Alasan kelayakan + aksi operasional | `bedEligibility` + `BaseButton` dalam `bedSelectionActions` | Kedua unsur dan layout sudah tersedia di komponen yang sama | `COMPOSE` | Opsi A yang disetujui; tidak membuat komponen atau CSS baru |

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | State denah existing menampilkan “Memuat denah dan kelayakan bed untuk pasien ini…”; detail pembersihan mengunci tindakan saat membaca snapshot |
| Kosong | Kamar tanpa bed tetap menampilkan “Belum ada bed pada kamar ini.”; riwayat kosong ditangani detail existing |
| Gagal | Error denah menyediakan `Muat Ulang Denah`; error 403/409/422 ditampilkan detail/hook existing |
| Tanpa hak akses | Tombol `Pembersihan` tidak dirender ketika `Read` atau action permission bukan `allowed`; alasan bed belum dapat dipilih tetap terlihat |

---

## 5. Endpoint yang dikonsumsi

Tidak ada endpoint baru. Endpoint berikut tetap dipanggil oleh hook/service existing.

#### Health Services / Inpatient Management / Bed Occupancy

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/inpatient-management/bed-occupancies/floor-plan` | Denah, status, `AvailableActions`, dan eligibility pasien | `InpatientBedOccupancy : Read` |

#### Health Services / Inpatient Management / Bed Management

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/inpatient-management/bed-management/monitoring` | Snapshot terbaru sebelum menawarkan tindakan | `InpatientBedManagement : Read` |
| `GET` | `/api/v1/health-services/inpatient-management/bed-management/beds/{bedId}/cleaning-attempts` | Riwayat dan attempt aktif | `InpatientBedManagement : Read` |
| `POST` | `/api/v1/health-services/inpatient-management/bed-management/beds/{bedId}/cleaning-attempts` | Mulai pembersihan | `InpatientBedManagement : StartCleaning` |
| `PATCH` | `/api/v1/health-services/inpatient-management/bed-management/cleaning-attempts/{attemptId}/complete` | Selesai pembersihan fisik | `InpatientBedManagement : CompleteCleaning` |
| `POST` | `/api/v1/health-services/inpatient-management/bed-management/beds/{bedId}/readiness-verifications` | Verifikasi kesiapan | `InpatientBedManagement : VerifyReadiness` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run build` | Tidak masuk ke npm karena `npm.ps1` diblokir execution policy PowerShell | `EXISTING / ENVIRONMENT ISSUE` | `PSSecurityException`; tidak ada perubahan policy sistem |
| `npm.cmd run build` | Selesai dengan exit code 0 | `PASS` | Next.js 16.2.12 compiled; TypeScript selesai; 498 halaman statis dihasilkan; standalone assets siap |
| Automated test | Tidak dijalankan atas instruksi eksplisit pemilik | `NOT RUN` | Pemilik meminta tidak melakukan testing |
| Lint | Tidak dijalankan atas instruksi eksplisit pemilik | `NOT RUN` | Batas validasi agent hanya production build |
| Browser/UAT | Dilakukan mandiri oleh pemilik dan belum dilaporkan | `NOT RUN` | Menunggu konfirmasi pemilik |
| `dotnet build` / migration | Tidak dijalankan dan tidak diperlukan untuk source frontend-only | `NOT RUN` | Pemilik menjalankan secara mandiri bila diperlukan |

Uji manual: `REQUIRED — NOT RUN`; pemilik menyatakan akan melakukan testing secara mandiri.

**AUTOMATED TEST: SKIPPED atas instruksi pemilik — perubahan diuji command hanya melalui
`npm.cmd run build`.**

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `RWI-AC-R027-01` — action + permission allowed menampilkan Pembersihan pada mode Pilih | Terpenuhi pada source; UAT pending | `canClean` dikirim ke `BedRow`; tombol ada pada cabang `selectionMode` |
| `RWI-AC-R027-02` — aksi kosong/denied/unknown fail closed | Terpenuhi pada source; UAT pending | Kedua keputusan permission wajib sama dengan `allowed`; `some()` wajib menemukan action server |
| `RWI-AC-R027-03` — detail existing dan fresh snapshot | Terpenuhi | State `cleaningTarget` tetap membuka `InpatientBedCleaningDetail`; hook existing melakukan load sebelum prepare |
| `RWI-AC-R027-04` — refresh efektif setelah aksi | Terpenuhi pada source; UAT pending | Callback mengembalikan promise `onChanged`; selection meneruskan `state.refresh`; adapter transfer existing memakai `Promise.all` |
| `RWI-AC-R027-05` — pilihan stale dilepas dan bed belum eligible tidak dapat dipilih | Terpenuhi pada source; UAT pending | Effect mencari bed terpilih pada snapshot dan memanggil `onClearSelection` bila tidak eligible |
| `RWI-AC-R027-06` — 403/409/422 serta version/cycle guard existing | Terpenuhi melalui reuse | Tidak ada perubahan pada `useInpatientBedCleaning` atau service mutasi |
| `RWI-AC-R027-07` — mode Monitoring dan aksi existing tidak dihapus | Terpenuhi pada source; UAT pending | Cabang Monitoring dipertahankan; perubahan Repair 026 pada file yang sama tidak ditimpa; build lulus |
| `RWI-AC-R027-08` — frontend-only dan production build | Terpenuhi | Tidak ada source backend/database berubah; `npm.cmd run build` exit code 0 |

`D-FE-R027` belum terpenuhi penuh karena UAT pemilik belum dilaporkan. Task karena itu tetap
`🟡 SEBAGIAN`, bukan `✅`.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Shim `npm.ps1` tidak dapat dijalankan oleh PowerShell; build yang sama berhasil melalui `npm.cmd` tanpa mengubah execution policy |
| Masalah yang diketahui | UAT permission/action/status belum dilakukan oleh agent sesuai batas yang ditetapkan pemilik |
| Dependency backend | `NOT APPLICABLE` — seluruh endpoint dan aturan server sudah tersedia |
| Perubahan sampingan | `NONE`; perubahan working tree lain milik pengguna/repair sebelumnya dipertahankan |
| Interupsi | `NONE` |
| Status Git | Dua source frontend task berubah; artefak blueprint Repair 027 berubah/baru. Worktree juga memuat perubahan lain yang sudah ada dan tidak disentuh task ini |
| Langkah berikutnya | Pemilik menguji akun allowed dan denied pada bed `WaitingCleaning`, lalu mengonfirmasi hasil agar status dapat dinaikkan menjadi `✅` |
