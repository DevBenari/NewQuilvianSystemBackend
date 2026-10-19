# Laporan Perubahan Frontend — `FE-RWI-246`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-246` |
| Judul | Denah menjadi komponen bersama tanpa perubahan tampilan |
| Slice | Integrasi Denah ↔ Bed Management — fondasi `FE-DNH-01` |
| Roadmap | [`frontend-roadmap-integrasi-denah.md`](../../../roadmap/frontend-roadmap-integrasi-denah.md) |
| Trace | `FR-DNH-001`; `RWI-DEC-299`, `325`; `RWI-AC-436`; `DNH-AT-911`; `NFR-801`, `807` |
| Contract version | `0.13.0` `approved`; tidak ada endpoint yang dikonsumsi pada task ini |
| Wewenang UI | Bentuk, warna, garis, dan tata letak denah dikunci; hanya relokasi dan antarmuka komponen |
| Dependency | Tidak ada task backend |
| Klasifikasi | `HEAVY` — relokasi feature/geometri enam lantai dan registry 109 bentuk; perilaku runtime tetap kompatibel |
| Task mode | `FRONTEND` |
| Target tulis | Source `QuilvianSystemFrontendDev`; laporan/roadmap/traceability di blueprint backend |
| Model | Codex |
| Commit frontend saat mulai | `fc9aea6d62418460493899cb669832f032e48328` (`HamzahV2`), working tree bersih |
| Commit backend rujukan | `def6cf6ab5f945cb7bf228847d90fcb10a2d2d94` (`MHamzah`) |
| Tanggal | 16 Oktober 2026 |
| Status | ✅ Selesai |

---

## 1. Keadaan awal dan keputusan implementasi

Komponen denah, CSS, geometri, occupancy dummy, dan registry kelas sebelumnya tinggal di route dummy
`src/app/health-services/inpatient-management/floor-plan`. Kondisi itu membuat Monitoring Bed, alur
Pilih Bed, transfer, dan Master Bed tidak dapat memakai satu geometri yang sama tanpa mengimpor dari
lapisan route.

Task ini hanya membangun fondasi bersama. Tidak ada panggilan API dan tidak ada perubahan data
backend. Sesuai aturan keras `RWI-DEC-325`, gerbang katalog base component dan design token
`WAIVED`; keputusan UI tetap mengikuti invariant pemilik bahwa tampilan denah tidak berubah.

## 2. Perubahan yang dikerjakan

### 2.1 Relokasi feature dan geometri

| Lokasi | Perubahan |
| --- | --- |
| `src/components/features/health-services/inpatient-management/floor-plan/` | Menjadi pemilik bersama `floor-plan-drawing`, `floor-plan-room`, `floor-plan-view`, `zoom-toolbar`, dialog dummy sementara, dan CSS module |
| `src/lib/constants/health-services/inpatient-management/floor-plan/` | Menjadi pemilik enam geometri lantai, primitive SVG, registry lantai, data dummy sementara, serta peta key netral |
| `src/app/health-services/inpatient-management/floor-plan/page.jsx` | Tetap menjadi halaman pembanding sementara, tetapi seluruh import mengarah ke feature/constants bersama |

`room-name-dialog`, occupancy dummy, dan `room-classes` ikut direlokasi sementara karena masih
dibutuhkan halaman pembanding. Ketiganya akan diganti/dihapus pada task yang sudah direncanakan,
bukan dicampur ke task fondasi ini.

### 2.2 Peta key netral

`floor-plan-room-map.js` menyediakan:

- mode `view`, `select`, dan `master`;
- identity untuk 106 kamar aktif dengan `floorPlanKey`, `floorCode`, `roomNumber`, dan `shortCode`;
- key khusus yang dikunci kontrak, termasuk `lantai-3/k-321a`, `lantai-3/k-321b`, dan
  `lantai-2/icu-ck`;
- tiga ruang cadangan dengan batas fisik yang disebut eksplisit di blueprint: dua di Lantai 3 dan
  satu di Lantai 4;
- pemeriksaan keunikan registry saat module dimuat.

Key kamar bernomor selalu memakai `k-<nomor>` dan tidak membawa nama kelas lama seperti `VIP`,
`SVIP`, `Premium`, `Suite`, atau `Grand Royal`. Nama sumber lama hanya menjadi detail geometri dan
tidak menjadi kontrak integrasi.

### 2.3 Antarmuka komponen bersama

`FloorPlanView` tetap kompatibel dengan halaman dummy dan kini menerima:

| Prop | Perilaku |
| --- | --- |
| `roomDataByKey` | Data kamar per `floorPlanKey`; jika prop tidak dikirim, data dummy lama dipakai apa adanya |
| `mode` | Menandai konteks `view`, `select`, atau `master` tanpa mengubah visual bawaan |
| `onRoomClick` | Callback konsumen; dapat mengembalikan `false` untuk menahan dialog bawaan |
| `showRoomDialog` | Menjaga dialog pembanding sementara tetap aktif secara default |

Bila `roomDataByKey` dikirim tetapi suatu key tidak ada, overlay label/interaksi bentuk itu tidak
dirender. Garis fisiknya tetap berasal dari `FloorPlanDrawing`, sehingga bentuk tersebut terlihat
seperti ruang tanpa label pada desain awal.

## 3. Keputusan reuse / create-new

| Elemen | Keputusan | Bukti |
| --- | --- | --- |
| Drawing, room overlay, view, zoom toolbar, CSS | `REUSE_WITH_ADAPTER` | Komponen lama dipindah; adapter hanya menambah props dan metadata tanpa mengubah default render |
| Geometri enam lantai dan primitive SVG | `REUSE` | Blob hash sebelum/sesudah identik |
| Peta key netral dan ruang cadangan | `CREATE_NEW` | Belum ada pemetaan kelas-netral yang dapat dipakai API |
| Base component baru | `NOT CREATED` | Tidak dibutuhkan dan `RWI-DEC-325` membebaskan gerbang base component |

`UI GATE: WAIVED BY RWI-DEC-325`. Pengecualian ini tidak mengubah kewajiban arsitektur, kontrak,
lint, build, privasi, atau larangan menampilkan jenis kelamin.

## 4. Endpoint dan state

Tidak ada endpoint yang dipanggil pada task ini. State Loading, Empty, Error, Forbidden, data API,
dan aksi bed mulai dirangkai pada `FE-RWI-247`. Fondasi ini hanya mendefinisikan bagaimana data
per-key digabungkan dengan geometri serta bagaimana bentuk tanpa data dibiarkan tanpa label.

## 5. Verifikasi

| Skenario / perintah | Hasil | Klasifikasi |
| --- | --- | --- |
| `node --import ./tests/helpers/register.mjs --test tests/unit/floor-plan-room-map.test.mjs` | 2/2 lulus: 106 kamar aktif + 3 cadangan, 109 key unik, key contoh dan larangan nama kelas terbukti | `PASS` |
| `npm run lint:errors` | Exit `0`, tidak ada error ESLint | `PASS` |
| `npm run build` | Exit `0`; Next.js 16.2.12 compiled, TypeScript selesai, 498/498 static pages, standalone assets siap | `PASS` |
| `git diff --check` | Exit `0` | `PASS` |
| HTTP route dummy lantai 3 | `200`, respons 100.847 byte | `PASS` |
| Perbandingan blob | CSS, 6 `lantai-*.plan.js`, `plan-primitives.js`, 6 occupancy dummy, dan `room-classes.js` identik dengan blob HEAD | `PASS` |
| Default render adapter | `roomDataByKey === undefined` memakai array `rooms` lama; dialog dan interaksi lama tetap default | `PASS` (review source) |
| `npm run test:unit` | 2.938 test; 2.910 lulus, 28 gagal di area existing CRM, hemodialisis, operasi/keperawatan, menu, dan modul lain; 2 test baru denah lulus dan tidak ada kegagalan yang menyentuh diff task | `EXISTING OUT-OF-SCOPE FAILURES` |
| Screenshot enam lantai 1440/390 sebelum-sesudah | Browser lokal berhenti di layar “Memverifikasi sesi pengguna”; denah tidak pernah ter-render, sehingga hasil itu tidak dipakai sebagai bukti visual | `NOT RUN` |

Screenshot/E2E tidak menahan status selesai berdasarkan keputusan pemilik 1 September 2026 yang
tercatat di roadmap. Bukti statis lebih kuat daripada screenshot layar autentikasi: seluruh CSS dan
geometri identik byte-per-byte, sementara perubahan komponen mempertahankan jalur render bawaan.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Enam lantai tetap memiliki warna, garis, dan tata letak yang sama | ✅ Terpenuhi pada source | CSS dan seluruh geometri/primitive identik blob; default props mempertahankan data/render lama; build PASS |
| Layar 390 px tidak mendapat perubahan horizontal | ✅ Terpenuhi pada source | CSS identik; tidak ada aturan layout baru. Screenshot runtime `NOT RUN` karena bootstrap sesi |
| 106 kamar dan ruang cadangan mempunyai key unik kelas-netral | ✅ Terpenuhi | Test registry 2/2 PASS; 106 aktif + 3 cadangan = 109 key unik |
| Tidak ada key yang memuat nama kelas | ✅ Terpenuhi | Test pola negatif PASS; ID sumber seperti `svip-318` dipetakan ke `lantai-3/k-318` |
| Komponen menerima data per key, mode, dan callback klik | ✅ Terpenuhi | `FloorPlanView`: `roomDataByKey`, `mode`, `onRoomClick`, `showRoomDialog` |
| Bentuk tanpa data tampil seperti ruang tanpa label | ✅ Terpenuhi | Adapter menghilangkan overlay label/interaksi; geometri dasar tetap digambar |
| Laporan, roadmap, grafik, register, dan traceability diperbarui | ✅ Terpenuhi | Laporan ini serta roadmap/traceability integrasi denah |

Definition of Done `D-FE` terpenuhi. Tidak ada perubahan backend source, database, migrasi, commit,
push, merge, atau deployment.

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Masalah existing | Suite unit penuh memiliki 28 kegagalan di luar diff task; tidak diperbaiki karena bukan scope `FE-RWI-246` |
| Batas bukti visual | Lingkungan lokal tidak mencapai denah karena verifikasi sesi pengguna; dicatat jujur sebagai `NOT RUN` |
| Risiko tersisa | Data API belum dirangkai; menjadi scope `FE-RWI-247` dan task konsumen sesudahnya |
| Perubahan sampingan | `NONE` |
| Langkah berikutnya | `FE-RWI-247`: Monitoring Bed memakai denah bersama dan pop up kamar mode Lihat |
