# Laporan Perubahan Frontend — `FE-RWI-253`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-253` |
| Judul | Sisa tampilan bed lama dan isian jenis kelamin dihapus |
| Slice | Integrasi Denah ↔ Bed Management — cleanup breaking |
| Roadmap | [`frontend-roadmap-integrasi-denah.md`](../../../roadmap/frontend-roadmap-integrasi-denah.md) |
| Trace | `FR-DNH-003`, `006`; `RWI-DEC-299`, `315`, `325`; `RWI-AC-436`, `439`, `452`; `DNH-AT-912`; `DNH-MIS-08`, `09` |
| Contract version | `0.13.0` approved |
| Dependency | `FE-RWI-249` ✅; `BE-RWI-240` ✅ |
| Klasifikasi | `MEDIUM` — penghapusan breaking lintas route/component/form/payload/test dengan preservation endpoint worklist |
| Task mode | `FRONTEND` |
| Target tulis | Source `QuilvianSystemFrontendDev`; laporan/roadmap/traceability di blueprint backend |
| Commit frontend saat mulai rangkaian | `fc9aea6d62418460493899cb669832f032e48328` (`HamzahV2`) |
| Commit backend rujukan | `def6cf6ab5f945cb7bf228847d90fcb10a2d2d94` (`MHamzah`) |
| Tanggal | 17 Oktober 2026 |
| Status | ✅ Selesai |

---

## 1. Outcome

Tidak ada lagi route dummy denah, komponen kartu papan bed lama, CSS papan lama, atau daftar tabel
Master Bed lama. Monitoring, admisi, transfer, dan Master Bed memakai implementasi denah yang sudah
ditutup pada FE-RWI-247 s.d. FE-RWI-250. Production build turun dari 499 menjadi 498 halaman dan
manifest Next.js membuktikan route `/health-services/inpatient-management/floor-plan` tidak ada.

Field `IsForMale`/`IsForFemale` beserta bentuk camelCase, kode penolakan gender bed, isian form,
payload Room/Bed, detail Room/Bed, dan pesan UI terkait sudah nol di seluruh `src`. Frontend kini
selaras dengan backend breaking change `BE-RWI-240`.

Endpoint `bed-board` tidak dihapus. Ia tetap dipakai oleh daftar pasien rawat inap dan bootstrap
internal admisi/resume. Hook datanya juga dipertahankan karena masih mempunyai consumer nyata;
yang dihapus adalah tampilan kartu kedua. Alur transfer IGD/Poli yang membuat episode+reservasi
secara atomik memakai satu select ringkas dari daftar eligible server, bukan menghidupkan kembali
papan kartu.

## 2. Perubahan source

| Area | Implementasi |
| --- | --- |
| Route dummy | `src/app/health-services/inpatient-management/floor-plan/` dihapus; route tidak masuk build manifest |
| Papan lama | `inpatient-bed-board.jsx` dan CSS module dihapus; tidak ada import tersisa |
| Master Bed lama | `master-data-bed-view.jsx` dan hook list yatim `use-master-data-bed.jsx` dihapus setelah `bed-client` memakai editor denah |
| Data dummy | Enam `lantai-*.occupancy.js`, `room-classes.js`, serta salinan `_data` lama dihapus |
| Manifest impor | Kapasitas target 106 kamar dipisahkan ke `floor-plan-bed-capacities.js`; tidak memuat angka terisi/status runtime dan tetap menghasilkan 168 bed/8 boks |
| Label gambar | Helper label-only `floor-plan-room-labels.js` menjaga hasil gambar identik tanpa mengaku sebagai master kelas operasional |
| Master Room | Default, field form, mapper, payload, dan detail gender dihapus dari constants/utils |
| Master Bed | Default, field form, mapper, payload, dan detail gender dihapus dari constants/utils |
| Placement | Kode `BED_GENDER_MISMATCH` dan `PATIENT_GENDER_UNKNOWN` dihapus dari registry frontend; parser error generic tetap menampilkan respons server aktif |
| Admisi | Import papan kartu dihapus; admisi biasa/bayi tetap denah; transfer atomik memakai select eligible server tanpa aturan eligibility frontend |
| Test | Test perilaku lama diperbarui dan `floor-plan-legacy-cleanup.test.mjs` mengunci penghapusan fisik, zero identifier, manifest, dan preservation endpoint |

## 3. Kontrak dan kompatibilitas breaking

- `RoomRequest` dan `BedRequest` frontend tidak lagi mengirim `isForMale` atau `isForFemale`.
- Respons lama yang kebetulan masih memiliki field gender tidak dibaca atau ditampilkan.
- Parser penolakan tetap generic untuk 409/422 dan daftar error server, tetapi tidak mempunyai
  registry/copy khusus kode gender yang sudah dicabut.
- Eligibility tidak dihitung ulang. Semua bed pada denah maupun select transfer atomik berasal dari
  endpoint server; frontend tidak memeriksa data demografis pasien.
- `GET bed-occupancies/bed-board` tetap ada dan dipakai worklist lokasi pasien serta bootstrap
  reservasi. Penghapusan komponen tidak menghapus kontrak data yang masih dimiliki consumer lain.
- Release frontend ini harus dipasangkan dengan `BE-RWI-240`; kedua sisi source sudah PASS
  `DNH-AT-912`.

## 4. Keputusan reuse / delete / replace

| Elemen | Keputusan | Bukti |
| --- | --- | --- |
| Denah shared + endpoint floor-plan | `REUSE_UNCHANGED` | Monitoring, selection, transfer, dan master sudah memakai komponen bersama; geometri visual tidak diubah |
| `inpatient-bed-board.jsx` + CSS | `DELETE` | Tidak ada consumer/import; test memastikan file absent |
| `use-inpatient-bed-board.jsx` | `RETAIN_AS_DATA_ADAPTER` | Masih dipakai bootstrap Draft/resume pada `use-inpatient-admission-bed.jsx`; roadmap hanya menghapusnya bila tanpa consumer |
| Endpoint `bed-board` | `REUSE` | `use-inpatient-episode-worklist.jsx` tetap memuat lokasi bed pasien |
| Master list lama + hook list yatim | `DELETE` | Entry point Master Bed sudah memakai `master-data-bed-floor-plan-view.jsx` |
| Occupancy dummy | `DELETE_AND_REPLACE` | Status runtime berasal API; hanya kapasitas manifest disimpan pada file baru |
| Registry kelas dummy | `DELETE_AND_REPLACE` | Kelas operasional dari API; helper baru hanya mempertahankan label gambar denah |
| Field/pesan gender bed | `DELETE` | Kontrak backend 0.13.0/BE-RWI-240 telah menghapusnya |

`UI GATE: WAIVED BY RWI-DEC-325`. Tidak ada perubahan geometri/tampilan floor plan. Cleanup hanya
menghapus jalur UI kedua dan data dummy yang tidak lagi authoritative.

## 5. Verifikasi

| Skenario / perintah | Hasil | Klasifikasi |
| --- | --- | --- |
| Test fokus cleanup + denah + master + admisi + detail + discharge | 98/98 lulus | `PASS` |
| Test baru `floor-plan-legacy-cleanup.test.mjs` | 5/5 lulus | `PASS` |
| Pencarian identifier terlarang pada `src` | 0 hasil untuk `IsForMale`, `IsForFemale`, bentuk camelCase, `BED_GENDER_MISMATCH`, `PATIENT_GENDER_UNKNOWN` | `PASS` |
| Pencarian referensi legacy pada `src` | 0 hasil untuk papan/CSS lama, `room-classes.js`, dan `*.occupancy.js` | `PASS` |
| ESLint fokus file task | Exit `0` | `PASS` |
| `npm run lint:errors` | Exit `0`, tidak ada error ESLint | `PASS` |
| `npm run build` | Exit `0`; Next.js 16.2.12 compiled, TypeScript selesai, 498/498 static pages, standalone siap | `PASS` |
| Build route manifest | Dummy floor-plan `ABSENT`; route bed-board pasien `PRESENT` | `PASS` |
| Seluruh unit test | 2.969 test; 2.939 lulus, 30 gagal existing di luar scope; lima test baru lulus dan jumlah gagal tetap baseline | `EXISTING OUT-OF-SCOPE FAILURES` |
| `git diff --check` frontend | Exit `0` | `PASS` |
| DNH-AT-912 | Frontend zero field/copy/kode gender + backend `BE-RWI-240` PASS; pairing release terdokumentasi | `PASS SOURCE/BUILD` |
| Live browser | Server, sesi, akun, dan fixture runtime berotorisasi tidak tersedia | `NOT RUN` |

## 6. Acceptance criteria dan DoD

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Tidak ada tampilan bed kedua | ✅ | Komponen papan kartu, CSS, master list lama, dan seluruh import-nya dihapus |
| Halaman dummy tidak dapat dibuka | ✅ | File page absent dan route tidak ada pada build manifest |
| Occupancy dan registry kelas dummy dihapus | ✅ | Enam file occupancy + `room-classes.js` absent; source search nol |
| Manifest impor tidak rusak | ✅ | 106 kamar, 168 bed, 8 boks tetap PASS dari kapasitas target non-runtime |
| Form Room/Bed tanpa isian gender | ✅ | Constants tidak mempunyai field/default gender |
| Payload Room/Bed tanpa field gender | ✅ | Builder payload dan mapper tidak membaca/mengirim field |
| Detail Room/Bed tanpa tampilan gender | ✅ | Detail row dihapus |
| Tidak ada pesan/kode penolakan gender bed | ✅ | Placement registry dan source search nol |
| Daftar pasien tetap memuat lokasi bed | ✅ | Worklist tetap memanggil `get("bed-board")`; route bed-board tetap build |
| Tidak ada consumer komponen yang dihapus | ✅ | Source search/import scan nol; production build PASS |
| Gelombang frontend/backend serentak | ✅ | Dependency `BE-RWI-240` selesai; `DNH-AT-912` PASS source/build |
| Laporan, roadmap, graph, register, traceability | ✅ | Laporan ini dan pembaruan dokumen delivery |

Definition of Done `D-FE` terpenuhi. Tidak ada perubahan backend source, database, migrasi, commit,
push, merge, deployment, ataupun mutasi data nyata.

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Masalah existing | Suite global memiliki 30 kegagalan modul lain; tidak ada kegagalan baru |
| Runtime | Live browser aktual `NOT RUN`; build manifest memberi bukti route dummy hilang |
| Hook papan | Sengaja dipertahankan sebagai adapter data karena consumer admisi/resume masih nyata; bukan UI kedua |
| Status roadmap | Seluruh 8/8 task frontend Integrasi Denah selesai |
