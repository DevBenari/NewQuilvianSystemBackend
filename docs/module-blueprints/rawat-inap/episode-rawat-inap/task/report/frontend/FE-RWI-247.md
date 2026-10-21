# Laporan Perubahan Frontend — `FE-RWI-247`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-247` |
| Judul | Monitoring Bed memakai denah dan pop up kamar |
| Slice | Integrasi Denah ↔ Bed Management — `FE-DNH-02` mode Lihat |
| Roadmap | [`frontend-roadmap-integrasi-denah.md`](../../../roadmap/frontend-roadmap-integrasi-denah.md) |
| Trace | `FR-DNH-001`, `002`, `003`, `014`; `RWI-DEC-287`, `299`, `303`, `316`, `320`, `325`; `RWI-AC-436`, `442`, `443`, `453`; `NFR-802`, `807` |
| Contract version | `0.13.0` approved |
| Dependency | `FE-RWI-246` ✅; `BE-RWI-238` ✅ |
| Klasifikasi | `HEAVY` — satu layar operasional diganti dari kartu/tabel ke denah, dengan aksi bed dan privacy boundary |
| Task mode | `FRONTEND` |
| Target tulis | Source `QuilvianSystemFrontendDev`; laporan/roadmap/traceability di blueprint backend |
| Model | Codex |
| Commit frontend saat mulai rangkaian | `fc9aea6d62418460493899cb669832f032e48328` (`HamzahV2`) |
| Commit backend rujukan | `def6cf6ab5f945cb7bf228847d90fcb10a2d2d94` (`MHamzah`) |
| Tanggal | 16 Oktober 2026 |
| Status | ✅ Selesai |

---

## 1. Outcome

Tab Monitoring pada Manajemen Tempat Tidur sekarang memakai denah bersama dari `FE-RWI-246`.
Satu request `GET .../bed-management/floor-plan?floorCode=...` menjadi sumber nama kelas, status,
badge `kapasitas/terisi`, selektabilitas, ringkasan dependensi, daftar bed, identitas yang boleh
terlihat, serta aksi pembersihan. Kartu dan tabel monitoring lama tidak lagi dirender oleh
`bed-management-view.jsx`.

Tampilan dasar denah tetap dipertahankan. Perubahan visual terbatas pada permintaan terkunci:
outline merah kamar penuh dihapus, outline hijau permanen berasal dari `IsSelectable`, badge berasal
dari `Counts`, dan popup kamar diperluas dengan daftar bed.

## 2. Perubahan source

| Area | Implementasi |
| --- | --- |
| Service | `bed-management.service.js` menambah `getFloorPlan(floorCode, config)` dengan query dan `AbortSignal` |
| Hook denah | `use-floor-plan.js` menormalisasi Pascal/camel case, menyimpan lantai terakhir lewat `localStorage` dalam `try/catch`, membatalkan request sebelumnya, dan menolak stale response lewat sequence ID |
| Monitoring | `floor-plan-monitoring.jsx` menangani Loading, Error + Coba Lagi, data, pergantian lantai, refresh, popup, dan perpindahan ke riwayat |
| Denah | `FloorPlanView` menerima pergantian lantai dan active key; `FloorPlanRoom` memakai `IsSelectable` untuk outline hijau; seluruh layer merah kamar penuh dihapus |
| Popup | `floor-plan-room-popup.jsx` mempertahankan judul/aksen/rincian popup lama lalu menambah baris bed, titik status, penghuni, RM, HD/LD, countdown, konflik, pembersihan, riwayat, dan tutup/buka bed |
| Layar existing | `bed-management-view.jsx` tidak lagi merender Summary/Grid/Table/filter monitoring atau modal detail lama; tab Transfer dan Riwayat tetap dipertahankan |
| Hook layar | `use-bed-management.js` kini hanya memuat metadata/tab; tidak lagi melakukan request endpoint monitoring lama |

## 3. Kontrak, aksi, dan privasi

- Identitas pasien, No. RM, dan HD/LD hanya dinormalisasi dari `HoldingContext` serta
  `DependencySummary` pada respons sekarang. Nilai tidak dicari atau diisi ulang dari cache.
- Payload Housekeeping dengan kedua field tersebut `null` tetap menghasilkan `null`; test negatif
  membuktikan string pasien dan RM tidak muncul kembali pada hasil normalisasi.
- Pembersihan hanya tampil jika `AvailableActions` memuat `StartCleaning`, `CompleteCleaning`, atau
  `VerifyReadiness`; implementasi existing `InpatientBedCleaningDetail` dipakai ulang dan suksesnya
  memuat ulang lantai.
- Riwayat hanya tampil ketika metadata tab menyatakan akses riwayat tersedia.
- Tutup/buka bed hanya tampil bagi keputusan permission ketat `Bed : Update`. Tutup mengirim status
  `Blocked`; buka hanya ditawarkan bila server mengirim `BED_UNAVAILABLE`, lalu mengirim status
  `Available`. Keduanya meminta alasan, mengirim `ExpectedBedVersion`, memakai adapter idempotensi
  existing, dan memuat ulang proyeksi server. Membuka master tidak diklaim langsung siap.
- Guid tidak ditampilkan atau diminta dari pengguna.

## 4. Keputusan reuse / create-new

| Elemen | Keputusan | Bukti |
| --- | --- | --- |
| Denah/geometri/zoom dari `FE-RWI-246` | `REUSE_WITH_ADAPTER` | Props data, active key, dan floor callback ditambahkan tanpa mengubah geometri |
| Pembersihan dan operation recovery | `REUSE` | `InpatientBedCleaningDetail` serta `requestBedMasterMutation` dipakai langsung |
| Riwayat penggunaan | `REUSE` | Popup mengalihkan bed terpilih ke `InpatientBedUsageHistory` existing |
| Hook query dan popup denah | `CREATE_NEW` | Belum ada pemilik state satu-lantai dan popup room-first berbasis key denah |
| Kartu/tabel monitoring lama | `STOP_RENDERING` | Import dan markup dihapus dari view; berkas fisik dibersihkan terjadwal pada `FE-RWI-253` |
| Base component baru | `NOT CREATED` | Tidak diperlukan; komponen existing dipakai untuk tombol, alert, konfirmasi, dan aksi cleaning |

`UI GATE: WAIVED BY RWI-DEC-325`. Pengecualian hanya berlaku untuk katalog/pola visual; kontrak,
permission, privasi, stack, lint, build, dan laporan tetap dipenuhi.

## 5. State dan kegagalan

| State | Perilaku |
| --- | --- |
| Loading awal | Panel menyatakan denah dan status bed sedang dimuat |
| Empty/missing key | Bentuk dasar tetap tampak tanpa overlay label/interaksi; kamar tanpa bed menyatakan belum ada bed |
| Error awal | Pesan error + tombol `Coba Lagi` |
| Error refresh | Denah terakhir dipertahankan dan alert error tampil |
| Pergantian lantai cepat | Request lama di-abort dan hasil dengan sequence lama diabaikan |
| Local storage gagal | Denah tetap berjalan memakai lantai default/server |
| Mutation 409/422/403 | Pesan server aman tampil di konfirmasi; denah tidak mengarang hasil |
| Mutation ambigu | Adapter operation existing mempertahankan key/payload untuk pemeriksaan/retry identik |

## 6. Verifikasi

| Skenario / perintah | Hasil | Klasifikasi |
| --- | --- | --- |
| Test fokus denah + regresi Rawat Inap | 26/26 lulus; termasuk fixture `3/2`, penuh `3/3`, Housekeeping tersamar, registry key, stale guard, aksi availability, dan fondasi transaksi | `PASS` |
| `npm run lint:errors` setelah perubahan final | Exit `0`, tidak ada error ESLint | `PASS` |
| Lint terarah file task | Exit `0`, tidak ada error maupun warning | `PASS` |
| `npm run build` setelah perubahan final | Exit `0`; Next.js 16.2.12 compiled, TypeScript selesai, 498/498 static pages, standalone assets siap | `PASS` |
| `git diff --check` | Exit `0`; hanya warning normalisasi LF→CRLF Git pada beberapa file | `PASS` |
| HTTP `/health-services/inpatient-management/bed-board` | `200`, respons 53.300 byte | `PASS` |
| `npm run test:unit` | 2.941 test; 2.913 lulus, 28 gagal existing di CRM, hemodialisis, operasi/keperawatan, menu, dan area lain; seluruh test baru/relevan lulus | `EXISTING OUT-OF-SCOPE FAILURES` |
| UAT-801/802 dengan fixture dan akun aktual | Lingkungan browser lokal hanya mencapai bootstrap/verifikasi sesi; tidak ada kredensial/fixture berotorisasi yang diberikan | `NOT RUN` |

UAT browser tidak menahan status selesai berdasarkan keputusan pemilik 1 September 2026 pada D-FE.
Source, test fixture kontrak, lint, build, dan HTTP route menjadi bukti task ini.

## 7. Acceptance criteria dan DoD

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Kamar mixed menampilkan badge `3/2` dan outline hijau | ✅ | Normalizer mengambil `Counts`; class hijau hanya dari `IsSelectable`; fixture test PASS |
| Kamar penuh `3/3` tanpa merah dan tetap dapat diklik | ✅ | Layer/class/legend merah dihapus; interaksi room tidak dibatasi `isFull`; fixture test PASS |
| Popup 329 menampilkan `High Dependency (1 HD · 1 LD)` | ✅ | Formatter `DependencySummary`; fixture 1 HD + 1 LD PASS |
| Pembersihan dari popup lalu reload denah | ✅ | Aksi dikunci `AvailableActions`; callback sukses memanggil refresh lantai |
| Tutup/buka dan riwayat tersedia sesuai hak | ✅ | Permission `Bed:Update`, alasan + expected version; riwayat dijaga metadata tab |
| Housekeeping tidak melihat nama, RM, atau HD/LD | ✅ | Tidak ada cache rehydrate; fixture redacted menghasilkan seluruh field `null` |
| Gagal muat punya pesan dan retry | ✅ | Error state + `Coba Lagi` |
| Respons lama tidak menimpa lantai baru | ✅ | `AbortController` + monotonik `sequence.current` |
| Kartu/tabel monitoring tidak lagi dirender | ✅ | View tidak mengimpor/merender `BedMonitoringCards`, `DataTable`, atau endpoint monitoring lama |
| Laporan, roadmap, graph, register, traceability | ✅ | Laporan ini dan pembaruan dokumen delivery |

Definition of Done `D-FE` terpenuhi. Tidak ada perubahan backend source, database, migrasi, commit,
push, merge, atau deployment.

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Masalah existing | Suite global tetap mempunyai 28 kegagalan yang sama dengan baseline `FE-RWI-246`; tidak ada yang menyentuh diff task |
| Batas runtime | UAT peran/fixture aktual `NOT RUN`; endpoint dan privacy contract dibuktikan melalui source serta fixture unit |
| Berkas lama | Kartu/tabel berhenti dirender sekarang; penghapusan fisiknya tetap milik `FE-RWI-253` agar breaking cleanup terjadi pada task yang dikunci |
| Perubahan sampingan | `NONE` |
| Langkah berikutnya | `FE-RWI-248`: mode Pilih untuk admisi biasa dan bayi |
