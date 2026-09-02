# FE-RWI-007 — Penolakan penempatan terbaca alasannya, bukan sekadar gagal

- TASK ID: `FE-RWI-007`
- TASK TYPE: Implementasi perilaku frontend lintas layar
- COMPLEXITY: `HEAVY`
- CLASSIFICATION SCORE: 9 — dua repository 2; lebih dari 20 berkas diperiksa 2; empat berkas diubah 1; logika moderat 1; mengonsumsi kontrak yang sudah ada 1; database 0; menyentuh penjagaan akses tanpa mengubahnya 1; alur berbatas 1
- MODEL: Claude Opus 5
- TASK MODE: `FRONTEND`
- WRITE TARGET: `QuilvianSystemFrontendDev` pada branch `HamzahV2`. Backend hanya dibaca
- TASK/CONTRACT VERSION: roadmap frontend revision `2`; api contract `0.4.0` — `POST /bed-occupancies/placements` berstatus ✅ **Tersedia**
- FILES INSPECTED: roadmap `FE-RWI-007`; api contract bagian Bed Occupancy beserta tabel kode statusnya; `InpatientBedOccupancyController.cs` bagian `FromFailure`; `InpatientSharedDtos.cs` (`PlacementEligibilityFailureResponse`); `InpBedOccupancyService.cs` baris 1324–1398 tempat kedelapan aturan kelayakan dibentuk; `InpatientBedOccupancyDtos.cs` (`PlacePatientRequest`); fondasi `FE-RWI-002`
- FILES CHANGED: `src/utils/health-services/inpatient-management/inpatient-placement-utils.jsx` (baru); `src/components/features/health-services/inpatient-management/placement-failure-list.jsx` (baru); `tests/unit/inpatient-placement.test.mjs` (baru); aksi penempatan pada `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission.jsx` dan penyajiannya pada `src/components/view/health-services/inpatient-management/inpatient-admission-view.jsx`

## Yang membuat task ini bisa dikerjakan dengan benar

Backend tidak hanya mengirim satu kalimat penolakan. `FromFailure` menaruh **daftar** aturan yang
gagal pada kolom `errors`, dan tiap butirnya berbentuk `PlacementEligibilityFailureResponse` dengan
empat kolom: nomor aturan, kode, kalimat, dan kode status. Kolom **kode** itulah yang membuat
kriteria 4 dapat dipenuhi dengan pasti.

Kelima kode yang dipakai layar ini, disalin apa adanya dari service:

| Aturan | Kode | Arti |
| ---: | --- | --- |
| 4 | `BED_GENDER_MISMATCH` | Tempat tidur tidak menerima jenis kelamin pasien |
| 5 | `PATIENT_GENDER_UNKNOWN` | Jenis kelamin pasien belum tercatat |
| 6 | `ROOM_GENDER_MIXED` | Kamar sedang dihuni jenis kelamin berbeda — kalimatnya memuat nama kamar |
| 7 | `ISOLATION_REQUIRED` | Pasien butuh isolasi, tempat tidurnya bukan isolasi |
| 8 | `ISOLATION_BED_RESERVED` | Tempat tidur isolasi, pasiennya tidak butuh isolasi |

- IMPLEMENTATION: (1) `parsePlacementFailure` membaca kode status HTTP dan daftar `errors`, lalu menandai `isConflict` untuk 409 dan `isRuleRejection` untuk 422. (2) 409 memicu pembacaan ulang daftar tempat tidur dan mengosongkan pilihan, tanpa menyentuh satu pun isian yang sedang diketik. (3) 422 menampilkan seluruh butir daftar, masing-masing dengan nomor aturannya dan kalimat server apa adanya. (4) Label arah isolasi diturunkan dari **kode**, bukan dari kalimat, sehingga kedua pesan yang berlawanan arah tidak mungkin tertukar walau kalimat servernya diperbaiki.
- API CONTRACT IMPACT: Tidak mengubah kontrak.
- DATABASE IMPACT: Tidak ada.
- SECURITY IMPACT: Tidak mengubah authorization maupun authentication.
- VISUAL REFERENCE: NOT REQUIRED.
- WEWENANG UI YANG DIPAKAI: "Bentuk penyajian daftar alasan bebas". Dipilih daftar bertingkat: satu peringatan berisi pesan utama server, lalu tiap aturan sebagai baris tersendiri berpenanda nomor aturan.

## Acceptance criteria

| Kriteria | Hasil | Bukti |
| --- | :---: | --- |
| 1. 409 memicu muat ulang, menampilkan pesan server, membiarkan memilih ulang; isian tidak hilang | **LULUS** | e2e: 409 muncul dengan kalimat server, jumlah panggilan `/available-beds` bertambah, catatan "Pasien minta ranjang dekat jendela." tetap ada, lalu pemilihan ulang berhasil pada percobaan kedua |
| 2. 422 menampilkan daftar aturan yang gagal, bukan satu kalimat umum | **LULUS** | e2e: dua butir aturan tampil sebagai baris terpisah, masing-masing dengan nomor aturannya |
| 3. Pesan pencampuran kamar ditampilkan apa adanya, termasuk nama kamarnya | **LULUS** | e2e mencocokkan kalimat lengkap "Kamar Melati 1 sedang dihuni pasien Laki-laki, sehingga tidak dapat menerima pasien Perempuan." |
| 4. Dua pesan isolasi yang berbeda arah tidak tertukar | **LULUS** | e2e: baris `ISOLATION_BED_RESERVED` berlabel "Tempat tidur khusus pasien isolasi" dan **tidak** memuat "Pasien butuh isolasi"; skenario sebaliknya diperiksa terpisah. Test unit membuktikan label tetap benar walau kalimat servernya sengaja ditukar |
| 5. Tempat tidur yang direbut pasien lain hilang dari daftar setelah muat ulang | **LULUS** | e2e memastikan `/available-beds` dipanggil ulang sesudah 409; papan disusun ulang dari jawaban baru itu |

- VALIDATION: e2e `tests/e2e/inpatient-admission.spec.mjs` skenario penolakan | PASS, 3/3 | TASK | perebutan tempat tidur (409), penolakan berlapis termasuk pencampuran kamar (422), dan kedua arah pesan isolasi
- VALIDATION: `node --test tests/unit/inpatient-placement.test.mjs` | PASS, 8/8 | TASK | pembacaan 409 dan 422, daftar aturan, nama kamar utuh, arah isolasi yang tidak tertukar, penolakan tanpa daftar, bentuk payload, jalur gagal tidak menghapus isian, dan penjaga penempatan ganda
- VALIDATION: `npm run lint:errors` | PASS, exit 0 | TASK
- VALIDATION: `npm run build` beserta `postbuild` | PASS, exit 0 | TASK
- VALIDATION: seluruh unit test kecuali berkas yang rusak sejak merge | PASS, 82/82 | TASK
- MANUAL TEST: NOT FEASIBLE — dua petugas yang benar-benar merebut tempat tidur yang sama memerlukan dua sesi login dan database bersama. Skenarionya dijalankan di browser sungguhan lewat e2e dengan server tiruan yang menolak 409 pada percobaan pertama lalu menerima pada percobaan kedua, sehingga urutan yang dialami petugas kedua tetap terwakili.
- WARNINGS: Penanganan ini terpasang pada aksi penempatan di layar admisi. Roadmap menyebut scope-nya "penanganan 409 dan 422 di seluruh layar Rawat Inap"; layar perpindahan (`FE-RWI-010`) dan penutupan (`FE-RWI-014`) belum ada, dan keduanya harus memakai `parsePlacementFailure` serta `PlacementFailureList` yang sama, bukan menulis ulang penanganannya.
- KNOWN ISSUES: Tidak ada cacat implementasi yang diketahui pada scope task.
- DEPENDENCY BACKEND: `BE-RWI-011`, `BE-RWI-013`, dan `BE-RWI-015` — `POST /placements` berstatus ✅ `Tersedia`. `BE-RWI-011` masih 🟡 karena test tabrakan dua transaksi terhadap PostgreSQL belum dijalankan; itu menyangkut pertahanan di sisi database, bukan bentuk balasan yang dikonsumsi layar ini.
- INCIDENTAL CHANGES: `test-results/.last-run.json` dipulihkan; konfigurasi Playwright sementara dihapus.
- INTERRUPTIONS: NONE
- GIT STATUS: Berkas baru dan perubahan pada hook serta view admisi, **belum di-stage dan belum di-commit**.
- NEXT RECOMMENDED STEP: Saat `FE-RWI-010` dan `FE-RWI-014` dikerjakan, pakai ulang `parsePlacementFailure` dan `PlacementFailureList` supaya penanganan 409/422 tetap satu tempat.
